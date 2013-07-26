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
using DirectShowLib;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Analog.GraphComponents;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Analog.Components
{
  /// <summary>
  /// The crossbar component of the graph
  /// </summary>
  internal class Crossbar : IDisposable
  {


    #region variables

    /// <summary>
    /// The crossbar filter
    /// </summary>
    private IBaseFilter _filterCrossBar;

    /// <summary>
    /// The crossbar device
    /// </summary>
    private DsDevice _crossBarDevice;

    /// <summary>
    /// The crossbar interface
    /// </summary>
    private IAMCrossbar _crossBarFilter;

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

    /// <summary>
    /// The audio tuner in pin
    /// </summary>
    private IPin _audioTunerIn;

    /// <summary>
    /// The video output pin index 
    /// </summary>
    private int _videoOutPinIndex;

    /// <summary>
    /// The audio output pin index
    /// </summary>
    private int _audioOutPinIndex;

    /// <summary>
    /// The video output pin
    /// </summary>
    private IPin _videoOut;

    /// <summary>
    /// The audio output pin
    /// </summary>
    private IPin _audioOut;

    /// <summary>
    /// The current analog channel
    /// </summary>
    private AnalogChannel _currentChannel;

    #endregion

    #region properties

    /// <summary>
    /// Gets the video output pin
    /// </summary>
    public IPin VideoOut
    {
      get { return _videoOut; }
    }

    /// <summary>
    /// Gets the audio output pin
    /// </summary>
    public IPin AudioOut
    {
      get { return _audioOut; }
    }

    /// <summary>
    /// Gets the name of the crossbar device
    /// </summary>
    public String CrossBarName
    {
      get { return _crossBarDevice.Name; }
    }

    /// <summary>
    /// Gets the crossbar filter
    /// </summary>
    public IBaseFilter Filter
    {
      get { return _filterCrossBar; }
    }

    /// <summary>
    /// Gets the audio tuner input pin
    /// </summary>
    public IPin AudioTunerIn
    {
      get { return _audioTunerIn; }
    }

    #endregion

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // get rid of managed resources
        if (_crossBarDevice != null)
        {
          DevicesInUse.Instance.Remove(_crossBarDevice);
          _crossBarDevice.Dispose();
          _crossBarDevice = null;
        }
      }

      // get rid of unmanaged resources
      Release.ComObject("crossbar tuner audio input pin", ref _audioTunerIn);
      Release.ComObject("crossbar video output pin", ref _videoOut);
      Release.ComObject("crossbar audio output pin", ref _audioOut);
      Release.ComObject("crossbar filter", ref _filterCrossBar);
    }


    /// <summary>
    /// Disposes the crossbar component
    /// </summary>   
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Crossbar()
    {
      Dispose(false);
    }

    #endregion

    #region CreateFilterInstance method

    /// <summary>
    /// Adds the cross bar filter to the graph and connects the tv tuner to the crossbar.
    /// at the end of this method the graph looks like:
    /// [tv tuner]----->[crossbar]
    /// </summary>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    public bool CreateFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Tuner tuner)
    {
      if (!string.IsNullOrEmpty(graph.Crossbar.Name))
      {
        this.LogDebug("analog: Using Crossbar configuration from stored graph");
        if (CreateConfigurationBasedFilterInstance(graph, graphBuilder, tuner))
        {
          this.LogDebug("analog: Using Crossbar configuration from stored graph succeeded");
          return true;
        }
      }
      this.LogDebug("analog: No stored or invalid graph for Crossbar component - Trying to detect");
      return CreateAutomaticFilterInstance(graph, graphBuilder, tuner);
    }

    #endregion

    #region private helper methods

    /// <summary>
    /// Creates the filter based on the configuration file
    /// </summary>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateConfigurationBasedFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Tuner tuner)
    {
      string deviceName = graph.Crossbar.Name;
      _audioTunerIn = null;
      DsDevice[] devices;
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
        devices = DeviceSorter.Sort(devices, graph.Tuner.Name);
      }
      catch (Exception)
      {
        this.LogDebug("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      if (devices == null || devices.Length == 0)
      {
        this.LogDebug("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      //try each crossbar
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        //if crossbar is already in use then we can skip it
        if (!DevicesInUse.Instance.Add(devices[i]))
        {
          continue;
        }
        IPin tunerOut = null;
        try
        {
          if (!deviceName.Equals(devices[i].Name))
            continue;
          this.LogDebug("analog: AddCrossBarFilter use:{0} {1}", devices[i].Name, i);
          int hr;
          try
          {
            //add the crossbar to the graph
            hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            this.LogDebug("analog: cannot add filter to graph");
            continue;
          }
          if (hr != (int)HResult.Severity.Success)
          {
            //failed. try next crossbar
            if (tmp != null)
            {
              graphBuilder.RemoveFilter(tmp);
              Release.ComObject("crossbar filter candidate", ref tmp);
            }
            continue;
          }
          _crossBarFilter = (IAMCrossbar)tmp;
          _videoPinMap = graph.Crossbar.VideoPinMap;
          _audioPinMap = graph.Crossbar.AudioPinMap;
          _videoPinRelatedAudioMap = graph.Crossbar.VideoPinRelatedAudioMap;
          _videoOutPinIndex = graph.Crossbar.VideoOut;
          _audioOutPinIndex = graph.Crossbar.AudioOut;
          if (_videoOutPinIndex == -1)
          {
            this.LogDebug("analog: AddCrossbarFilter no video output found");
            graphBuilder.RemoveFilter(tmp);
            Release.ComObject("crossbar filter candidate", ref tmp);
            _crossBarFilter = null;
            continue;
          }
          //connect tv tuner->crossbar
          tunerOut = DsFindPin.ByDirection(tuner.Filter, PinDirection.Output,
                                                graph.Tuner.VideoPin);
          if (tunerOut != null && _videoPinMap.ContainsKey(CaptureVideoSource.Tuner) &&
              FilterGraphTools.ConnectPin(graphBuilder, tunerOut, tmp, _videoPinMap[CaptureVideoSource.Tuner]))
          {
            // Got it, we're done
            _filterCrossBar = tmp;
            _crossBarDevice = devices[i];
            if (_audioTunerIn == null)
            {
              _audioTunerIn = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Input,
                                                    _audioPinMap[CaptureAudioSource.Tuner]);
            }
            Release.ComObject("crossbar tuner filter output pin", ref tunerOut);
            _videoOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _videoOutPinIndex);
            if (_audioOutPinIndex != -1)
            {
              _audioOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _audioOutPinIndex);
            }
            this.LogDebug("analog: AddCrossBarFilter succeeded");
            break;
          }
        }
        finally
        {
          if (_filterCrossBar == null)
          {
            DevicesInUse.Instance.Remove(devices[i]);
          }
        }
        // cannot connect tv tuner to crossbar, try next crossbar device
        if (tmp != null)
        {
          graphBuilder.RemoveFilter(tmp);
          Release.ComObject("crossbar filter candidate", ref tmp);
        }
        Release.ComObject("crossbar tuner filter output pin", ref tunerOut);
      }
      return _filterCrossBar != null;
    }

    /// <summary>
    /// Creates the filter by trying to detect it
    /// </summary>
    /// <param name="tuner">The tuner component</param>
    /// <param name="graph">The stored graph</param>
    /// <param name="graphBuilder">The graphBuilder</param>
    /// <returns>true, if the graph building was successful</returns>
    private bool CreateAutomaticFilterInstance(Graph graph, IFilterGraph2 graphBuilder, Tuner tuner)
    {
      _audioTunerIn = null;
      _filterCrossBar = null;
      DsDevice[] devices;
      //get list of all crossbar devices installed on this system
      try
      {
        devices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCrossbar);
        devices = DeviceSorter.Sort(devices, graph.Tuner.Name);
      }
      catch (Exception)
      {
        this.LogDebug("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      if (devices == null || devices.Length == 0)
      {
        this.LogDebug("analog: AddCrossBarFilter no crossbar devices found");
        return false;
      }
      //try each crossbar
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        this.LogDebug("analog: AddCrossBarFilter try:{0} {1}", devices[i].Name, i);
        //if crossbar is already in use then we can skip it
        if (!DevicesInUse.Instance.Add(devices[i]))
        {
          continue;
        }
        IPin pinIn = null;
        try
        {
          int hr;
          try
          {
            //add the crossbar to the graph
            hr = graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
          }
          catch (Exception)
          {
            this.LogDebug("analog: cannot add filter to graph");
            continue;
          }
          if (hr != (int)HResult.Severity.Success)
          {
            //failed. try next crossbar
            if (tmp != null)
            {
              graphBuilder.RemoveFilter(tmp);
              Release.ComObject("crossbar filter candidate", ref tmp);
            }
            continue;
          }
          _crossBarFilter = (IAMCrossbar)tmp;
          CheckCapabilities();
          if (_videoOutPinIndex == -1)
          {
            this.LogDebug("analog: AddCrossbarFilter no video output found");
            graphBuilder.RemoveFilter(tmp);
            _crossBarFilter = null;
            Release.ComObject("crossbar filter candidate", ref tmp);
            continue;
          }

          // Check that the crossbar has a tuner video input pin.
          if (_videoPinMap.ContainsKey(CaptureVideoSource.Tuner))
          {
            pinIn = DsFindPin.ByDirection(tmp, PinDirection.Input, _videoPinMap[CaptureVideoSource.Tuner]);
          }
          if (pinIn == null)
          {
            // no pin found, continue with next crossbar
            this.LogDebug("analog: AddCrossBarFilter no video tuner input pin detected");
            if (tmp != null)
            {
              graphBuilder.RemoveFilter(tmp);
              _crossBarFilter = null;
              Release.ComObject("crossbar filter candidate", ref tmp);
            }
            continue;
          }
          //connect tv tuner->crossbar
          int tempVideoPinIndex;
          if (FilterGraphTools.ConnectFilter(graphBuilder, tuner.Filter, pinIn, out tempVideoPinIndex))
          {
            // Got it, we're done
            _filterCrossBar = tmp;
            _crossBarDevice = devices[i];
            if (_audioTunerIn == null)
            {
              _audioTunerIn = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Input,
                                                    _audioPinMap[CaptureAudioSource.Tuner]);
            }
            Release.ComObject("crossbar tuner input pin", ref pinIn);
            _videoOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _videoOutPinIndex);
            if (_audioOutPinIndex != -1)
            {
              _audioOut = DsFindPin.ByDirection(_filterCrossBar, PinDirection.Output, _audioOutPinIndex);
            }
            this.LogDebug("analog: AddCrossBarFilter succeeded");
            graph.Crossbar.AudioOut = _audioOutPinIndex;
            graph.Crossbar.AudioPinMap = _audioPinMap;
            graph.Crossbar.Name = _crossBarDevice.Name;
            graph.Crossbar.VideoOut = _videoOutPinIndex;
            graph.Crossbar.VideoPinMap = _videoPinMap;
            graph.Crossbar.VideoPinRelatedAudioMap = _videoPinRelatedAudioMap;
            graph.Tuner.VideoPin = tempVideoPinIndex;
            break;
          }
        }
        finally
        {
          if (_filterCrossBar == null)
          {
            DevicesInUse.Instance.Remove(devices[i]);
          }
        }

        // cannot connect tv tuner to crossbar, try next crossbar device
        graphBuilder.RemoveFilter(tmp);
        Release.ComObject("crossbar tuner input pin", ref pinIn);
        Release.ComObject("crossbar filter candidate", ref tmp);
      }
      return _filterCrossBar != null;
    }


    /// <summary>
    /// Checks the capabilities
    /// </summary>
    private void CheckCapabilities()
    {
      int outputs, inputs;
      _crossBarFilter.get_PinCounts(out outputs, out inputs);
      _videoOutPinIndex = -1;
      _audioOutPinIndex = -1;
      _videoPinMap = new Dictionary<CaptureVideoSource, int>();
      _audioPinMap = new Dictionary<CaptureAudioSource, int>();
      _videoPinRelatedAudioMap = new Dictionary<CaptureVideoSource, int>();
      int relatedPinIndex;
      PhysicalConnectorType connectorType;
      for (int i = 0; i < outputs; ++i)
      {
        _crossBarFilter.get_CrossbarPinInfo(false, i, out relatedPinIndex, out connectorType);
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
        _crossBarFilter.get_CrossbarPinInfo(true, i, out relatedPinIndex, out connectorType);
        this.LogDebug(" crossbar pin:{0} type:{1}, related:{2}", i, connectorType, relatedPinIndex);
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
                _videoPinMap.Add(CaptureVideoSource.Composite3, i);
                _videoPinRelatedAudioMap.Add(CaptureVideoSource.Composite3, relatedPinIndex);
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
                _videoPinMap.Add(CaptureVideoSource.Svideo3, i);
                _videoPinRelatedAudioMap.Add(CaptureVideoSource.Svideo3, relatedPinIndex);
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

    #endregion

    #region public methods

    /// <summary>
    /// Indicates if it is a special plextor card
    /// </summary>
    /// <returns>true, if it is a special plextor card</returns>
    public void PerformTune(AnalogChannel channel)
    {
      if (_currentChannel != null)
      {
        bool updateRequired = _currentChannel.MediaType == channel.MediaType;        
        if (updateRequired ||
            (_currentChannel.VideoSource != channel.VideoSource && _videoPinMap.ContainsKey(channel.VideoSource)))
        {
          _crossBarFilter.Route(_videoOutPinIndex, _videoPinMap[channel.VideoSource]);
          updateRequired = true;
        }
        if (_audioOutPinIndex == -1)
        {
          return;
        }
        if (updateRequired || _currentChannel.AudioSource != channel.AudioSource)
        {
          if (channel.AudioSource == CaptureAudioSource.Automatic &&
              _videoPinRelatedAudioMap.ContainsKey(channel.VideoSource))
          {
            _crossBarFilter.Route(_audioOutPinIndex, _videoPinRelatedAudioMap[channel.VideoSource]);
          }
          else if (_audioPinMap.ContainsKey(channel.AudioSource))
          {
            _crossBarFilter.Route(_audioOutPinIndex, _audioPinMap[channel.AudioSource]);
          }
        }
      }
      else
      {
        if (_videoPinMap.ContainsKey(channel.VideoSource))
        {
          _crossBarFilter.Route(_videoOutPinIndex, _videoPinMap[channel.VideoSource]);
        }
        if (_audioOutPinIndex == -1)
        {
          return;
        }
        if (channel.AudioSource == CaptureAudioSource.Automatic &&
            _videoPinRelatedAudioMap.ContainsKey(channel.VideoSource))
        {
          _crossBarFilter.Route(_audioOutPinIndex, _videoPinRelatedAudioMap[channel.VideoSource]);
        }
        else if (_audioPinMap.ContainsKey(channel.AudioSource))
        {
          _crossBarFilter.Route(_audioOutPinIndex, _audioPinMap[channel.AudioSource]);
        }
      }
      _currentChannel = channel;
    }

    #endregion
  }
}