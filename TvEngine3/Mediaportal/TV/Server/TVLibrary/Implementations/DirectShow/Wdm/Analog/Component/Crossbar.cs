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
using System.Text.RegularExpressions;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A WDM analog DirectShow crossbar graph component.
  /// </summary>
  internal class Crossbar : BaseComponent
  {
    #region constants

    private static readonly Regex DEVICE_ID_HAUPPAUGE_COLOSSUS = new Regex("pci#ven_1745&dev_3000&subsys_([0-9a-f]{8})");
    private static readonly Regex DEVICE_ID_HAUPPAUGE_HDPVR2 = new Regex("usb#vid_2040&pid_([0-9a-f]{4})");

    #endregion

    #region variables

    /// <summary>
    /// The crossbar device.
    /// </summary>
    private DsDevice _device = null;

    /// <summary>
    /// The crossbar filter.
    /// </summary>
    private IBaseFilter _filter = null;

    /// <summary>
    /// A map linking each video source to its corresponding pin index.
    /// </summary>
    private IDictionary<CaptureSourceVideo, int> _pinMapVideo = new Dictionary<CaptureSourceVideo, int>();

    /// <summary>
    /// A map linking each video source to its corresponding default audio source pin index.
    /// </summary>
    private IDictionary<CaptureSourceVideo, int> _pinMapVideoDefaultAudio = new Dictionary<CaptureSourceVideo, int>();

    /// <summary>
    /// A map linking each audio source to its corresponding pin index.
    /// </summary>
    private IDictionary<CaptureSourceAudio, int> _pinMapAudio = new Dictionary<CaptureSourceAudio, int>();

    /// <summary>
    /// The video output pin index.
    /// </summary>
    private int _pinIndexOutputVideo = PIN_INDEX_NOT_SET;

    /// <summary>
    /// The audio output pin index.
    /// </summary>
    private int _pinIndexOutputAudio = PIN_INDEX_NOT_SET;

    /// <summary>
    /// The index of the video input pin which is currently routed to the video output pin.
    /// </summary>
    private int _pinIndexRoutedVideo = PIN_INDEX_NOT_SET;

    /// <summary>
    /// The index of the audio input pin which is currently routed to the audio output pin.
    /// </summary>
    private int _pinIndexRoutedAudio = PIN_INDEX_NOT_SET;

    #endregion

    #region properties

    /// <summary>
    /// Get the filter.
    /// </summary>
    public IBaseFilter Filter
    {
      get
      {
        return _filter;
      }
    }

    /// <summary>
    /// Get the tuner video input pin index.
    /// </summary>
    public int PinIndexInputTunerVideo
    {
      get
      {
        int index = PIN_INDEX_NOT_SET;
        if (_pinMapVideo.TryGetValue(CaptureSourceVideo.Tuner, out index))
        {
          return index;
        }
        return PIN_INDEX_NOT_SET;
      }
    }

    /// <summary>
    /// Get the tuner audio input pin index.
    /// </summary>
    public int PinIndexInputTunerAudio
    {
      get
      {
        int index = PIN_INDEX_NOT_SET;
        if (_pinMapAudio.TryGetValue(CaptureSourceAudio.Tuner, out index))
        {
          return index;
        }
        return PIN_INDEX_NOT_SET;
      }
    }

    /// <summary>
    /// Get the video output pin index.
    /// </summary>
    public int PinIndexOutputVideo
    {
      get
      {
        return _pinIndexOutputVideo;
      }
    }

    /// <summary>
    /// Get the audio output pin index.
    /// </summary>
    public int PinIndexOutputAudio
    {
      get
      {
        return _pinIndexOutputAudio;
      }
    }

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="Crossbar"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public Crossbar(DsDevice device)
    {
      _device = device;
    }

    #endregion

    /// <summary>
    /// Load the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    public void PerformLoading(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog crossbar: perform loading");

      if (!DevicesInUse.Instance.Add(_device))
      {
        throw new TvException("Main crossbar component is in use.");
      }
      try
      {
        _filter = FilterGraphTools.AddFilterFromDevice(graph, _device);
      }
      catch (Exception ex)
      {
        DevicesInUse.Instance.Remove(_device);
        throw new TvException(ex, "Failed to add filter for main crossbar component to graph.");
      }
    }

    /// <summary>
    /// Check the capabilites of the component.
    /// </summary>
    private void CheckCapabilities()
    {
      IAMCrossbar crossbar = _filter as IAMCrossbar;
      if (crossbar == null)
      {
        throw new TvException("Failed to find crossbar interface on filter.");
      }

      int countOutput = 0;
      int countInput = 0;
      int hr = crossbar.get_PinCounts(out countOutput, out countInput);
      TvExceptionDirectShowError.Throw(hr, "Failed to get pin counts.");

      int relatedPinIndex;
      PhysicalConnectorType connectorType;
      for (int i = 0; i < countOutput; i++)
      {
        hr = crossbar.get_CrossbarPinInfo(false, i, out relatedPinIndex, out connectorType);
        TvExceptionDirectShowError.Throw(hr, "Failed to get pin information for output pin {0}.", i);
        this.LogDebug("WDM analog crossbar: output pin {0}, type = {1}, related = {2}", i, connectorType, relatedPinIndex);
        if (connectorType == PhysicalConnectorType.Video_VideoDecoder && _pinIndexOutputVideo == PIN_INDEX_NOT_SET)
        {
          _pinIndexOutputVideo = i;
        }
        else if (connectorType == PhysicalConnectorType.Audio_AudioDecoder && _pinIndexOutputAudio == PIN_INDEX_NOT_SET)
        {
          _pinIndexOutputAudio = i;
        }
        else
        {
          this.LogWarn("WDM analog crossbar: unsupported or duplicate output type {0} detected", connectorType);
        }
      }

      bool isHauppaugeColossus = false;
      bool isHauppaugeHdPvr2 = false;
      bool isHauppaugeHdPvr60 = false;
      IsHauppaugeProduct(_device, out isHauppaugeColossus, out isHauppaugeHdPvr2, out isHauppaugeHdPvr60);

      int countAudioLine = 0;
      int countAudioSpdif = 0;
      int countAudioAuxiliary = 0;
      int countAudioAes = 0;
      int countAudioHdmi = 0;
      int countVideoComposite = 0;
      int countVideoSvideo = 0;
      int countVideoYryby = 0;
      int countVideoRgb = 0;
      int countVideoHdmi = 0;
      for (int i = 0; i < countInput; i++)
      {
        hr = crossbar.get_CrossbarPinInfo(true, i, out relatedPinIndex, out connectorType);
        TvExceptionDirectShowError.Throw(hr, "Failed to get pin information for input pin {0}.", i);
        this.LogDebug("WDM analog crossbar: input pin {0}, type = {1}, related = {2}", i, connectorType, relatedPinIndex);
        switch (connectorType)
        {
          case PhysicalConnectorType.Audio_Tuner:
            if (_pinMapAudio.ContainsKey(CaptureSourceAudio.Tuner))
            {
              this.LogWarn("WDM analog crossbar: multiple tuner audio inputs detected, not supported");
            }
            else
            {
              _pinMapAudio.Add(CaptureSourceAudio.Tuner, i);
            }
            break;
          case PhysicalConnectorType.Video_Tuner:
            if (_pinMapVideo.ContainsKey(CaptureSourceVideo.Tuner))
            {
              this.LogWarn("WDM analog crossbar: multiple tuner video inputs detected, not supported");
            }
            else
            {
              _pinMapVideo.Add(CaptureSourceVideo.Tuner, i);
              _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Tuner, relatedPinIndex);
            }
            break;
          case PhysicalConnectorType.Audio_Line:
            countAudioLine++;
            switch (countAudioLine)
            {
              case 1:
                _pinMapAudio.Add(CaptureSourceAudio.Line1, i);
                break;
              case 2:
                _pinMapAudio.Add(CaptureSourceAudio.Line2, i);
                break;
              case 3:
                _pinMapAudio.Add(CaptureSourceAudio.Line3, i);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} line audio inputs detected, only 3 supported", countAudioLine);
                break;
            }
            break;
          case PhysicalConnectorType.Audio_SPDIFDigital:
            if (countAudioHdmi == 0 && (isHauppaugeColossus || isHauppaugeHdPvr60))
            {
              countAudioHdmi = 1;
              _pinMapAudio.Add(CaptureSourceAudio.Hdmi1, i);
              break;
            }

            countAudioSpdif++;
            switch (countAudioSpdif)
            {
              case 1:
                _pinMapAudio.Add(CaptureSourceAudio.Spdif1, i);
                break;
              case 2:
                _pinMapAudio.Add(CaptureSourceAudio.Spdif2, i);
                break;
              case 3:
                _pinMapAudio.Add(CaptureSourceAudio.Spdif3, i);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} S/PDIF audio inputs detected, only 3 supported", countAudioSpdif);
                break;
            }
            break;
          case PhysicalConnectorType.Audio_AUX:
            countAudioAuxiliary++;
            switch (countAudioAuxiliary)
            {
              case 1:
                _pinMapAudio.Add(CaptureSourceAudio.Auxiliary1, i);
                break;
              case 2:
                _pinMapAudio.Add(CaptureSourceAudio.Auxiliary2, i);
                break;
              case 3:
                _pinMapAudio.Add(CaptureSourceAudio.Auxiliary3, i);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} auxiliary audio inputs detected, only 3 supported", countAudioAuxiliary);
                break;
            }
            break;
          case PhysicalConnectorType.Audio_AESDigital:
            if (isHauppaugeHdPvr2 && countAudioHdmi == 0)
            {
              countAudioHdmi++;
              _pinMapAudio.Add(CaptureSourceAudio.Hdmi1, i);
              break;
            }

            countAudioAes++;
            switch (countAudioAes)
            {
              case 1:
                _pinMapAudio.Add(CaptureSourceAudio.AesEbu1, i);
                break;
              case 2:
                _pinMapAudio.Add(CaptureSourceAudio.AesEbu2, i);
                break;
              case 3:
                _pinMapAudio.Add(CaptureSourceAudio.AesEbu3, i);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} AES audio inputs detected, only 3 supported", countAudioAes);
                break;
            }
            break;
          case PhysicalConnectorType.Video_Composite:
            countVideoComposite++;
            switch (countVideoComposite)
            {
              case 1:
                _pinMapVideo.Add(CaptureSourceVideo.Composite1, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Composite1, relatedPinIndex);
                break;
              case 2:
                _pinMapVideo.Add(CaptureSourceVideo.Composite2, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Composite2, relatedPinIndex);
                break;
              case 3:
                _pinMapVideo.Add(CaptureSourceVideo.Composite3, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Composite3, relatedPinIndex);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} composite video inputs detected, only 3 supported", countVideoComposite);
                break;
            }
            break;
          case PhysicalConnectorType.Video_SVideo:
            countVideoSvideo++;
            switch (countVideoSvideo)
            {
              case 1:
                _pinMapVideo.Add(CaptureSourceVideo.Svideo1, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Svideo1, relatedPinIndex);
                break;
              case 2:
                _pinMapVideo.Add(CaptureSourceVideo.Svideo2, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Svideo2, relatedPinIndex);
                break;
              case 3:
                _pinMapVideo.Add(CaptureSourceVideo.Svideo3, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Svideo3, relatedPinIndex);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} s-video video inputs detected, only 3 supported", countVideoSvideo);
                break;
            }
            break;
          case PhysicalConnectorType.Video_RGB:
            countVideoRgb++;
            switch (countVideoRgb)
            {
              case 1:
                _pinMapVideo.Add(CaptureSourceVideo.Rgb1, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Rgb1, relatedPinIndex);
                break;
              case 2:
                _pinMapVideo.Add(CaptureSourceVideo.Rgb2, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Rgb2, relatedPinIndex);
                break;
              case 3:
                _pinMapVideo.Add(CaptureSourceVideo.Rgb3, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Rgb3, relatedPinIndex);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} RGB video inputs detected, only 3 supported", countVideoRgb);
                break;
            }
            break;
          case PhysicalConnectorType.Video_YRYBY:
            countVideoYryby++;
            switch (countVideoYryby)
            {
              case 1:
                _pinMapVideo.Add(CaptureSourceVideo.Yryby1, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Yryby1, relatedPinIndex);
                break;
              case 2:
                _pinMapVideo.Add(CaptureSourceVideo.Yryby2, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Yryby2, relatedPinIndex);
                break;
              case 3:
                _pinMapVideo.Add(CaptureSourceVideo.Yryby3, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Yryby3, relatedPinIndex);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} YrYbY video inputs detected, only 3 supported", countVideoYryby);
                break;
            }
            break;
          case PhysicalConnectorType.Video_SerialDigital:
          case PhysicalConnectorType.Video_ParallelDigital:
            if (connectorType == PhysicalConnectorType.Video_ParallelDigital && !isHauppaugeHdPvr60)
            {
              this.LogWarn("WDM analog crossbar: unsupported input type {0} detected", connectorType);
              break;
            }

            countVideoHdmi++;
            switch (countVideoHdmi)
            {
              case 1:
                _pinMapVideo.Add(CaptureSourceVideo.Hdmi1, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Hdmi1, relatedPinIndex);
                break;
              case 2:
                _pinMapVideo.Add(CaptureSourceVideo.Hdmi2, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Hdmi2, relatedPinIndex);
                break;
              case 3:
                _pinMapVideo.Add(CaptureSourceVideo.Hdmi3, i);
                _pinMapVideoDefaultAudio.Add(CaptureSourceVideo.Hdmi3, relatedPinIndex);
                break;
              default:
                this.LogWarn("WDM analog crossbar: {0} serial digital (HDMI) video inputs detected, only 3 supported", countVideoHdmi);
                break;
            }
            break;
          default:
            this.LogWarn("WDM analog crossbar: unsupported input type {0} detected", connectorType);
            break;
        }
      }
    }

    /// <summary>
    /// Set sensible default configuration based on country and hardware capabilities.
    /// </summary>
    public void SetDefaultConfiguration(AnalogTunerSettings settings)
    {
      CaptureSourceVideo supportedVideoSources = CaptureSourceVideo.None;
      foreach (CaptureSourceVideo videoSource in _pinMapVideo.Keys)
      {
        supportedVideoSources |= videoSource;
      }
      settings.SupportedVideoSources = (int)supportedVideoSources;
      CaptureSourceAudio supportedAudioSources = CaptureSourceAudio.None;
      foreach (CaptureSourceAudio audioSource in _pinMapAudio.Keys)
      {
        supportedAudioSources |= audioSource;
      }
      settings.SupportedAudioSources = (int)supportedAudioSources;

      if (supportedVideoSources == CaptureSourceVideo.None)
      {
        settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.None;
        if (supportedAudioSources == CaptureSourceAudio.None)
        {
          settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.None;
          return;
        }

        List<CaptureSourceAudio> preferredAudioSources = new List<CaptureSourceAudio>
        {
          CaptureSourceAudio.Hdmi1,
          CaptureSourceAudio.AesEbu1,
          CaptureSourceAudio.Spdif1,
          CaptureSourceAudio.Line1,
          CaptureSourceAudio.Auxiliary1,
          CaptureSourceAudio.Tuner
        };
        foreach (CaptureSourceAudio audioSource in preferredAudioSources)
        {
          if (supportedAudioSources.HasFlag(audioSource))
          {
            settings.ExternalInputSourceAudio = (int)audioSource;
            break;
          }
        }
        return;
      }

      List<CaptureSourceVideo> preferredVideoSources = new List<CaptureSourceVideo>
      {
        CaptureSourceVideo.Hdmi1,
        CaptureSourceVideo.Yryby1,
        CaptureSourceVideo.Rgb1,
        CaptureSourceVideo.Svideo1,
        CaptureSourceVideo.Composite1
      };
      foreach (CaptureSourceAudio videoSource in preferredVideoSources)
      {
        if (supportedVideoSources.HasFlag(videoSource))
        {
          settings.ExternalInputSourceVideo = (int)videoSource;
          break;
        }
      }
      if (supportedAudioSources == CaptureSourceAudio.None)
      {
        settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.None;
      }
      else
      {
        settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.Automatic;
      }
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void PerformTuning(IChannel channel)
    {
      this.LogDebug("WDM analog crossbar: perform tuning");
      IAMCrossbar crossbar = _filter as IAMCrossbar;
      if (crossbar == null)
      {
        throw new TvException("Failed to find crossbar interface on filter.");
      }

      CaptureSourceVideo sourceVideo = CaptureSourceVideo.Tuner;
      CaptureSourceAudio sourceAudio = CaptureSourceAudio.Tuner;
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel != null)
      {
        sourceVideo = captureChannel.VideoSource;
        sourceAudio = captureChannel.AudioSource;
      }

      bool updateAudio = false;   // For compatibility we force re-routing of audio if/when video is routed.
      int hr;
      if (_pinIndexOutputVideo != PIN_INDEX_NOT_SET)
      {
        if (sourceVideo == CaptureSourceVideo.None)
        {
          this.LogDebug("WDM analog crossbar: no video");
        }
        else
        {
          int pinIndexRoutedVideoNew = PIN_INDEX_NOT_SET;
          if (!_pinMapVideo.TryGetValue(sourceVideo, out pinIndexRoutedVideoNew))
          {
            this.LogWarn("WDM analog crossbar: requested video source {0} is not available", sourceVideo);
          }
          else if (pinIndexRoutedVideoNew != _pinIndexRoutedVideo)
          {
            this.LogDebug("WDM analog crossbar: route video, source = {0}, pin index = {1}", sourceVideo, pinIndexRoutedVideoNew);
            hr = crossbar.Route(_pinIndexOutputVideo, pinIndexRoutedVideoNew);
            TvExceptionDirectShowError.Throw(hr, "Failed to route video from input pin {0} to output pin {1}.", pinIndexRoutedVideoNew, _pinIndexOutputVideo);
            _pinIndexRoutedVideo = pinIndexRoutedVideoNew;
            updateAudio = true;
          }
        }
      }

      if (_pinIndexOutputAudio != PIN_INDEX_NOT_SET)
      {
        if (sourceAudio == CaptureSourceAudio.None)
        {
          this.LogDebug("WDM analog crossbar: no audio");
        }
        else
        {
          int pinIndexRoutedAudioNew = PIN_INDEX_NOT_SET;
          bool gotIndex = false;
          if (sourceAudio == CaptureSourceAudio.Automatic)
          {
            gotIndex = _pinMapVideoDefaultAudio.TryGetValue(sourceVideo, out pinIndexRoutedAudioNew);
          }
          else
          {
            gotIndex = _pinMapAudio.TryGetValue(sourceAudio, out pinIndexRoutedAudioNew);
          }
          if (!gotIndex)
          {
            this.LogWarn("WDM analog crossbar: requested audio source {0} is not available", sourceAudio);
          }
          else if (updateAudio || pinIndexRoutedAudioNew != _pinIndexRoutedAudio)
          {
            this.LogDebug("WDM analog crossbar: route audio, source = {0}, pin index = {1}", sourceAudio, pinIndexRoutedAudioNew);
            hr = crossbar.Route(_pinIndexOutputAudio, pinIndexRoutedAudioNew);
            TvExceptionDirectShowError.Throw(hr, "Failed to route audio from input pin {0} to output pin {1}.", pinIndexRoutedAudioNew, _pinIndexOutputAudio);
            _pinIndexRoutedAudio = pinIndexRoutedAudioNew;
          }
        }
      }
    }

    /// <summary>
    /// Unload the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    public void PerformUnloading(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog crossbar: perform unloading");

      _pinIndexOutputVideo = PIN_INDEX_NOT_SET;
      _pinIndexOutputAudio = PIN_INDEX_NOT_SET;
      _pinIndexRoutedVideo = PIN_INDEX_NOT_SET;
      _pinIndexRoutedAudio = PIN_INDEX_NOT_SET;
      _pinMapVideo.Clear();
      _pinMapVideoDefaultAudio.Clear();
      _pinMapAudio.Clear();

      if (_filter != null)
      {
        if (graph != null)
        {
          graph.RemoveFilter(_filter);
        }
        Release.ComObject("WDM analog crossbar filter", ref _filter);

        DevicesInUse.Instance.Remove(_device);
        // Do NOT Dispose() or set the crossbar device to NULL. We would be
        // unable to reload. The tuner instance that instanciated this crossbar
        // is responsible for disposing it.
      }
    }

    private static void IsHauppaugeProduct(DsDevice device, out bool isHauppaugeColossus, out bool isHauppaugeHdPvr2, out bool isHauppaugeHdPvr60)
    {
      isHauppaugeColossus = false;
      isHauppaugeHdPvr2 = false;
      isHauppaugeHdPvr60 = false;
      if (device == null || string.IsNullOrEmpty(device.DevicePath))
      {
        return;
      }

      string devicePath = device.DevicePath.ToLowerInvariant();
      if (devicePath.Contains("usb#vid_048d&pid_9517") || devicePath.Contains("usb#vid_2040&pid_f810"))
      {
        isHauppaugeHdPvr60 = true;
        return;
      }

      Match m = DEVICE_ID_HAUPPAUGE_COLOSSUS.Match(devicePath);
      if (m.Success)
      {
        string subsys = m.Groups[1].Captures[0].Value;
        if (
          subsys.Equals("d1000070") || subsys.Equals("d1010070") ||
          subsys.Equals("d1800070") || subsys.Equals("d1810070") ||
          subsys.Equals("30001745")
        )
        {
          isHauppaugeColossus = true;
        }
        return;
      }

      m = DEVICE_ID_HAUPPAUGE_HDPVR2.Match(devicePath);
      if (m.Success)
      {
        string pid = m.Groups[1].Captures[0].Value;
        if (
          pid.Equals("e500") || pid.Equals("e501") || pid.Equals("e502") ||
          pid.Equals("e504") || pid.Equals("e505") || pid.Equals("e50a") ||
          pid.Equals("e514") || pid.Equals("e515") ||
          pid.Equals("e524") || pid.Equals("e525") ||
          pid.Equals("e52c") || pid.Equals("e554") || pid.Equals("e585")
        )
        {
          isHauppaugeHdPvr2 = true;
        }
      }
    }
  }
}