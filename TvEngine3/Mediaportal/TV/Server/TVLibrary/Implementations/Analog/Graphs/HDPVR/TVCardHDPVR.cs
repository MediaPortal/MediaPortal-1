#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DirectShowLib;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog.QualityControl;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Analog;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog.Graphs.HDPVR
{
  /// <summary>
  /// Class for handling supported capture cards, including the Hauppauge HD PVR and Colossus.
  /// </summary>
  public class TvCardHDPVR : DeviceDirectShow
  {
    #region constants

    // Assume the capture card is a Hauppauge HD PVR by default.
    private readonly string _deviceType = "HDPVR";
    private readonly string _crossbarDeviceName = "Hauppauge HD PVR Crossbar";
    private readonly string _captureDeviceName = "Hauppauge HD PVR Capture Device";
    private readonly string _encoderDeviceName = "Hauppauge HD PVR Encoder";

    #endregion

    #region variables

    private DsDevice _captureDevice;
    private DsDevice _encoderDevice;
    private IBaseFilter _filterCapture;
    private IBaseFilter _filterEncoder;
    private Configuration _configuration;
    private IQuality _qualityControl;

    /// <summary>
    /// The mapping of the video input sources to their pin index
    /// </summary>
    private Dictionary<CaptureVideoSource, int> _videoPinMap;

    /// <summary>
    /// The mapping of the video input sources to their related audio pin index
    /// </summary>
    private Dictionary<CaptureVideoSource, int> _videoPinRelatedAudioMap;

    /// <summary>
    /// The mapping of the audio input sources to their pin index
    /// </summary>
    private Dictionary<CaptureAudioSource, int> _audioPinMap;

    private int _videoOutPinIndex;
    private int _audioOutPinIndex;

    #endregion

    #region ctor

    ///<summary>
    /// Constructor for a capture card device.
    ///</summary>
    ///<param name="device">A crossbar device for a supported capture card.</param>
    public TvCardHDPVR(DsDevice device)
      : base(device)
    {
      // Determine what type of card this is.
      if (device.Name.Contains("Colossus"))
      {
        Match match = Regex.Match(device.Name, @".*?(\d+)$");
        int deviceNumber = 0;
        if (match.Success)
        {
          deviceNumber = Convert.ToInt32(match.Groups[1].Value);
        }
        _deviceType = "Colossus";
        _crossbarDeviceName = device.Name;
        _captureDeviceName = "Hauppauge Colossus Capture " + deviceNumber;
        _encoderDeviceName = "Hauppauge Colossus TS Encoder " + deviceNumber;
      }

      _tunerType = CardType.Analog;
      _configuration = Configuration.readConfiguration(_cardId, _name, DevicePath);
      Configuration.writeConfiguration(_configuration);
    }

    #endregion

    #region public methods

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      // My understanding is that the HD-PVR and Colossus are not able to capture audio-only streams. The
      // driver doesn't seem to create PMT if a video stream is not detected.
      return channel is AnalogChannel && channel.MediaType != MediaTypeEnum.Radio;
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Allocate a new subchannel instance.
    /// </summary>
    /// <param name="id">The identifier for the subchannel.</param>
    /// <returns>the new subchannel instance</returns>
    protected override ITvSubChannel CreateNewSubChannel(int id)
    {
      return new HDPVRChannel(id, this, _filterTsWriter);
    }

    #endregion

    #region quality control

    /// <summary>
    /// Get/Set the quality
    /// </summary>
    public override IQuality Quality
    {
      get { return _qualityControl; }
    }

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    public override bool SupportsQualityControl
    {
      get
      {
        if (_state == DeviceState.NotLoaded)
        {
          Load();
        }
        return _qualityControl != null;
      }
    }

    /// <summary>
    /// Reloads the quality control configuration
    /// </summary>
    public override void ReloadCardConfiguration()
    {
      if (_qualityControl != null)
      {
        _configuration = Configuration.readConfiguration(_cardId, _name, DevicePath);
        Configuration.writeConfiguration(_configuration);
        _qualityControl.SetConfiguration(_configuration);
      }
    }

    #endregion

    #region properties

    /// <summary>
    /// Update the tuner signal status statistics.
    /// </summary>
    /// <param name="force"><c>True</c> to force the status to be updated (status information may be cached).</param>
    protected override void UpdateSignalStatus(bool force)
    {
      if (_currentTuningDetail == null || _state != DeviceState.Started)
      {
        _tunerLocked = false;
        _signalPresent = true;
        _signalLevel = 0;
        _signalQuality = 0;
      }
      else
      {
        _tunerLocked = true;
        _signalPresent = true;
        _signalLevel = 100;
        _signalQuality = 100;
      }
    }

    #endregion

    /// <summary>
    /// Actually unload the device.
    /// </summary>
    protected override void PerformUnloading()
    {
      this.LogDebug("HDPVR:  Dispose()");
      base.Dispose();

      Release.ComObjectAllRefs("capture device capture filter", ref _filterCapture);
      Release.ComObjectAllRefs("capture device encoder filter", ref _filterEncoder);

      if (_captureDevice != null)
      {
        DevicesInUse.Instance.Remove(_captureDevice);
        _captureDevice = null;
      }
      if (_encoderDevice != null)
      {
        DevicesInUse.Instance.Remove(_encoderDevice);
        _encoderDevice = null;
      }
      this.LogDebug("HDPVR:  dispose completed");
    }

    #region graph handling

    /// <summary>
    /// Actually load the device.
    /// </summary>
    protected override void PerformLoading()
    {
      if (_cardId == 0)
      {
        _configuration = Configuration.readConfiguration(_cardId, _name, DevicePath);
        Configuration.writeConfiguration(_configuration);
      }

      _lastSignalUpdate = DateTime.MinValue;
      _tunerLocked = false;
      this.LogDebug("HDPVR: load device");
      InitialiseGraph();
      AddMainDeviceFilterToGraph();
      CheckCapabilities();
      AddCaptureFilter();
      AddEncoderFilter();
      AddAndConnectTsWriterIntoGraph(_filterEncoder);
      _qualityControl = QualityControlFactory.createQualityControl(_configuration, _filterEncoder, _filterCapture,
                                                                    null, null);
      if (_qualityControl == null)
      {
        this.LogDebug("HDPVR: No quality control support found");
      }

      _state = DeviceState.Stopped;
      _configuration.Graph.Crossbar.Name = _device.Name;
      _configuration.Graph.Crossbar.VideoPinMap = _videoPinMap;
      _configuration.Graph.Crossbar.AudioPinMap = _audioPinMap;
      _configuration.Graph.Crossbar.VideoPinRelatedAudioMap = _videoPinRelatedAudioMap;
      _configuration.Graph.Crossbar.VideoOut = _videoOutPinIndex;
      _configuration.Graph.Crossbar.AudioOut = _audioOutPinIndex;
      _configuration.Graph.Capture.Name = _captureDevice.Name;
      _configuration.Graph.Capture.FrameRate = -1d;
      _configuration.Graph.Capture.ImageHeight = -1;
      _configuration.Graph.Capture.ImageWidth = -1;
      Configuration.writeConfiguration(_configuration);
    }

    private void AddCaptureFilter()
    {
      DsDevice[] devices;
      this.LogDebug("HDPVR: Add Capture Filter");
      _filterCapture = null;
      //get a list of all video capture devices
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
        devices = DeviceSorter.Sort(devices, _device, _captureDevice, _filterEncoder);
      }
      catch (Exception)
      {
        this.LogDebug("HDPVR: AddTvCaptureFilter no tvcapture devices found");
        return;
      }
      if (devices.Length == 0)
      {
        this.LogDebug("HDPVR: AddTvCaptureFilter no tvcapture devices found");
        return;
      }
      //try each video capture filter
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].Name != _captureDeviceName)
        {
          continue;
        }
        this.LogDebug("HDPVR: AddTvCaptureFilter try:{0} {1}", devices[i].Name, i);
        // if video capture filter is in use, then we can skip it
        if (!DevicesInUse.Instance.Add(devices[i]))
        {
          continue;
        }
        IBaseFilter tmp;
        int hr;
        try
        {
          try
          {
            // add video capture filter to graph
            hr = _graph.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            this.LogDebug("HDPVR: cannot add filter to graph");
            continue;
          }
          if (hr != (int)HResult.Severity.Success)
          {
            //cannot add video capture filter to graph, try next one
            if (tmp != null)
            {
              _graph.RemoveFilter(tmp);
              Release.ComObject("capture device capture filter candidate", ref tmp);
            }
            continue;
          }
          // connect crossbar->video capture filter
          hr = _captureGraphBuilder.RenderStream(null, null, _filterDevice, null, tmp);
          if (hr == (int)HResult.Severity.Success)
          {
            // That worked. Since most crossbar devices require 2 connections from
            // crossbar->video capture filter, we do it again to connect the 2nd pin
            _captureGraphBuilder.RenderStream(null, null, _filterDevice, null, tmp);
            _filterCapture = tmp;
            _captureDevice = devices[i];
            this.LogDebug("HDPVR: AddTvCaptureFilter connected to crossbar successfully");
            break;
          }
        }
        finally
        {
          if (_filterCapture == null)
          {
            DevicesInUse.Instance.Remove(devices[i]);
          }
        }

        // cannot connect crossbar->video capture filter, remove filter from graph
        // cand continue with the next vieo capture filter
        this.LogDebug("HDPVR: AddTvCaptureFilter failed to connect to crossbar");
        _graph.RemoveFilter(tmp);
        Release.ComObject("capture device capture filter candidate", ref tmp);
      }
      if (_filterCapture == null)
      {
        this.LogError("HDPVR: unable to add TvCaptureFilter to graph");
        //throw new TvException("Unable to add TvCaptureFilter to graph");
      }
    }

    private void AddEncoderFilter()
    {
      DsDevice[] devices;
      this.LogDebug("HDPVR: AddEncoderFilter");
      _filterEncoder = null;
      // first get all encoder filters available on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.WDMStreamingEncoderDevices);
        devices = DeviceSorter.Sort(devices, _device, _captureDevice, _filterEncoder);
      }
      catch (Exception)
      {
        this.LogDebug("HDPVR: AddTvEncoderFilter no encoder devices found (Exception)");
        return;
      }

      if (devices == null)
      {
        this.LogDebug("HDPVR: AddTvEncoderFilter no encoder devices found (devices == null)");
        return;
      }

      if (devices.Length == 0)
      {
        this.LogDebug("HDPVR: AddTvEncoderFilter no encoder devices found");
        return;
      }

      //for each encoder
      this.LogDebug("HDPVR: AddTvEncoderFilter found:{0} encoders", devices.Length);
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].Name != _encoderDeviceName)
        {
          continue;
        }

        //if encoder is in use, we can skip it
        if (!DevicesInUse.Instance.Add(devices[i]))
        {
          this.LogDebug("HDPVR:  skip :{0} (inuse)", devices[i].Name);
          continue;
        }

        this.LogDebug("HDPVR:  try encoder:{0} {1}", devices[i].Name, i);
        IBaseFilter tmp;
        int hr;
        try
        {
          try
          {
            //add encoder filter to graph
            hr = _graph.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            this.LogDebug("HDPVR: cannot add filter {0} to graph", devices[i].Name);
            continue;
          }
          if (hr != (int)HResult.Severity.Success)
          {
            //failed to add filter to graph, continue with the next one
            if (tmp != null)
            {
              _graph.RemoveFilter(tmp);
              Release.ComObject("capture device encoder filter candidate", ref tmp);
            }
            continue;
          }
          if (tmp == null)
          {
            continue;
          }
          hr = _captureGraphBuilder.RenderStream(null, null, _filterCapture, null, tmp);
          if (hr == (int)HResult.Severity.Success)
          {
            // That worked. Since most crossbar devices require 2 connections from
            // crossbar->video capture filter, we do it again to connect the 2nd pin
            _captureGraphBuilder.RenderStream(null, null, _filterCapture, null, tmp);
            _filterEncoder = tmp;
            _encoderDevice = devices[i];
            this.LogDebug("HDPVR: AddTvEncoderFilter connected to catpure successfully");
            //and we're done
            return;
          }
        }
        finally
        {
          if (_filterEncoder == null)
          {
            DevicesInUse.Instance.Remove(devices[i]);
          }
        }
        // cannot connect crossbar->video capture filter, remove filter from graph
        // cand continue with the next vieo capture filter
        this.LogDebug("HDPVR: AddTvEncoderFilter failed to connect to capture");
        _graph.RemoveFilter(tmp);
        Release.ComObject("capture device encoder filter candidate", ref tmp);
      }
      this.LogDebug("HDPVR: AddTvEncoderFilter no encoder found");
    }

    #endregion

    #region private helper

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      this.LogDebug("HDPVR: Tune");
      AnalogChannel analogChannel = channel as AnalogChannel;
      if (analogChannel == null)
      {
        throw new NullReferenceException();
      }
      AnalogChannel currentChannel = _currentTuningDetail as AnalogChannel;
      if (_currentTuningDetail != null && currentChannel == null)
      {
        throw new NullReferenceException();
      }

      // Set up the crossbar.
      IAMCrossbar crossBarFilter = _filterDevice as IAMCrossbar;

      if (_currentTuningDetail == null || currentChannel.VideoSource != analogChannel.VideoSource)
      {
        // Video
        if (_videoPinMap.ContainsKey(analogChannel.VideoSource))
        {
          this.LogDebug("HDPVR:   video input -> {0}", analogChannel.VideoSource);
          crossBarFilter.Route(_videoOutPinIndex, _videoPinMap[analogChannel.VideoSource]);
        }

        // Automatic Audio
        if (analogChannel.AudioSource == CaptureAudioSource.Automatic)
        {
          if (_videoPinRelatedAudioMap.ContainsKey(analogChannel.VideoSource))
          {
            this.LogDebug("HDPVR:   audio input -> (auto)");
            crossBarFilter.Route(_audioOutPinIndex, _videoPinRelatedAudioMap[analogChannel.VideoSource]);
          }
        }
      }

      // Audio
      if ((_currentTuningDetail == null || currentChannel.AudioSource != analogChannel.AudioSource) &&
        analogChannel.AudioSource != CaptureAudioSource.Automatic &&
        _audioPinMap.ContainsKey(analogChannel.AudioSource))
      {
        this.LogDebug("HDPVR:   audio input -> {0}", analogChannel.AudioSource);
        crossBarFilter.Route(_audioOutPinIndex, _audioPinMap[analogChannel.AudioSource]);
      }

       this.LogDebug("HDPVR: Tuned to channel {0}", channel.Name);
    }

    /// <summary>
    /// Checks the capabilities
    /// </summary>
    private void CheckCapabilities()
    {
      IAMCrossbar crossBarFilter = _filterDevice as IAMCrossbar;
      if (crossBarFilter != null)
      {
        int outputs, inputs;
        crossBarFilter.get_PinCounts(out outputs, out inputs);
        _videoOutPinIndex = -1;
        _audioOutPinIndex = -1;
        _videoPinMap = new Dictionary<CaptureVideoSource, int>();
        _audioPinMap = new Dictionary<CaptureAudioSource, int>();
        _videoPinRelatedAudioMap = new Dictionary<CaptureVideoSource, int>();
        int relatedPinIndex;
        PhysicalConnectorType connectorType;
        for (int i = 0; i < outputs; ++i)
        {
          crossBarFilter.get_CrossbarPinInfo(false, i, out relatedPinIndex, out connectorType);
          if (connectorType == PhysicalConnectorType.Video_VideoDecoder)
          {
            _videoOutPinIndex = i;
          }
          if (connectorType == PhysicalConnectorType.Audio_AudioDecoder)
          {
            _audioOutPinIndex = i;
          }
        }

        int audioLine = 0;
        int audioSPDIF = 0;
        int audioAux = 0;
        int videoCvbsNr = 0;
        int videoSvhsNr = 0;
        int videoYrYbYNr = 0;
        int videoRgbNr = 0;
        int videoHdmiNr = 0;
        for (int i = 0; i < inputs; ++i)
        {
          crossBarFilter.get_CrossbarPinInfo(true, i, out relatedPinIndex, out connectorType);
          this.LogDebug(" crossbar pin:{0} type:{1}", i, connectorType);
          switch (connectorType)
          {
            case PhysicalConnectorType.Audio_Tuner:
              _audioPinMap.Add(CaptureAudioSource.Tuner, i);
              break;
            case PhysicalConnectorType.Video_Tuner:
              _videoPinMap.Add(CaptureVideoSource.Tuner, i);
              _videoPinRelatedAudioMap.Add(CaptureVideoSource.Tuner, relatedPinIndex);
              break;
            case PhysicalConnectorType.Audio_Line:
              audioLine++;
              switch (audioLine)
              {
                case 1:
                  _audioPinMap.Add(CaptureAudioSource.Line1, i);
                  break;
                case 2:
                  _audioPinMap.Add(CaptureAudioSource.Line2, i);
                  break;
                case 3:
                  _audioPinMap.Add(CaptureAudioSource.Line3, i);
                  break;
              }
              break;
            case PhysicalConnectorType.Audio_SPDIFDigital:
              audioSPDIF++;
              switch (audioSPDIF)
              {
                case 1:
                  _audioPinMap.Add(CaptureAudioSource.Spdif1, i);
                  break;
                case 2:
                  _audioPinMap.Add(CaptureAudioSource.Spdif2, i);
                  break;
                case 3:
                  _audioPinMap.Add(CaptureAudioSource.Spdif3, i);
                  break;
              }
              break;
            case PhysicalConnectorType.Audio_AUX:
              audioAux++;
              switch (audioAux)
              {
                case 1:
                  _audioPinMap.Add(CaptureAudioSource.Auxiliary1, i);
                  break;
                case 2:
                  _audioPinMap.Add(CaptureAudioSource.Auxiliary2, i);
                  break;
                case 3:
                  _audioPinMap.Add(CaptureAudioSource.Auxiliary3, i);
                  break;
              }
              break;
            case PhysicalConnectorType.Video_Composite:
              videoCvbsNr++;
              switch (videoCvbsNr)
              {
                case 1:
                  _videoPinMap.Add(CaptureVideoSource.Composite1, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Composite1, relatedPinIndex);
                  break;
                case 2:
                  _videoPinMap.Add(CaptureVideoSource.Composite2, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Composite2, relatedPinIndex);
                  break;
                case 3:
                  _videoPinMap.Add(CaptureVideoSource.Composite3, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Composite3, relatedPinIndex);
                  break;
              }
              break;
            case PhysicalConnectorType.Video_SVideo:
              videoSvhsNr++;
              switch (videoSvhsNr)
              {
                case 1:
                  _videoPinMap.Add(CaptureVideoSource.Svideo1, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Svideo1, relatedPinIndex);
                  break;
                case 2:
                  _videoPinMap.Add(CaptureVideoSource.Svideo2, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Svideo2, relatedPinIndex);
                  break;
                case 3:
                  _videoPinMap.Add(CaptureVideoSource.Svideo3, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Svideo3, relatedPinIndex);
                  break;
              }
              break;
            case PhysicalConnectorType.Video_RGB:
              videoRgbNr++;
              switch (videoRgbNr)
              {
                case 1:
                  _videoPinMap.Add(CaptureVideoSource.Rgb1, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Rgb1, relatedPinIndex);
                  break;
                case 2:
                  _videoPinMap.Add(CaptureVideoSource.Rgb2, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Rgb2, relatedPinIndex);
                  break;
                case 3:
                  _videoPinMap.Add(CaptureVideoSource.Rgb3, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Rgb3, relatedPinIndex);
                  break;
              }
              break;
            case PhysicalConnectorType.Video_YRYBY:
              videoYrYbYNr++;
              switch (videoYrYbYNr)
              {
                case 1:
                  _videoPinMap.Add(CaptureVideoSource.Yryby1, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Yryby1, relatedPinIndex);
                  break;
                case 2:
                  _videoPinMap.Add(CaptureVideoSource.Yryby2, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Yryby2, relatedPinIndex);
                  break;
                case 3:
                  _videoPinMap.Add(CaptureVideoSource.Yryby3, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Yryby3, relatedPinIndex);
                  break;
              }
              break;
            case PhysicalConnectorType.Video_SerialDigital:
              videoHdmiNr++;
              switch (videoHdmiNr)
              {
                case 1:
                  _videoPinMap.Add(CaptureVideoSource.Hdmi1, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Hdmi1, relatedPinIndex);
                  break;
                case 2:
                  _videoPinMap.Add(CaptureVideoSource.Hdmi2, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Hdmi2, relatedPinIndex);
                  break;
                case 3:
                  _videoPinMap.Add(CaptureVideoSource.Hdmi3, i);
                  _videoPinRelatedAudioMap.Add(CaptureVideoSource.Hdmi3, relatedPinIndex);
                  break;
              }
              break;
          }
        }
      }
    }

    #endregion
  }
}