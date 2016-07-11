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
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using TveAnalogVideoStandard = Mediaportal.TV.Server.Common.Types.Enum.AnalogVideoStandard;
using TveMediaType = Mediaportal.TV.Server.Common.Types.Enum.MediaType;
using WdmAnalogVideoStandard = DirectShowLib.AnalogVideoStandard;
using WdmMediaType = DirectShowLib.MediaType;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component
{
  /// <summary>
  /// A WDM analog DirectShow capture graph component.
  /// </summary>
  internal class Capture
  {
    #region structs

    // All fields are used by the Marshal.PtrToStructure function in ConfigureStream().
    #pragma warning disable 649, 169, 0649
    private struct MPEG2VideoInfo
    {
      internal VideoInfoHeader2 hdr;
      internal uint dwStartTimeCode;
      internal uint cbSequenceHeader;
      internal uint dwProfile;
      internal uint dwLevel;
      internal uint dwFlags;
      internal uint dwSequenceHeader;
    }
    #pragma warning restore 649, 169, 0649

    #endregion

    #region constants

    private static ICollection<string> CAPTURE_DEVICE_BLACKLIST = new List<string>
    {
      // Don't use NVIDIA DualTV YUV Capture filters. They have video and audio
      // inputs but don't have an audio output. Prefer NVIDIA DualTV Capture.
      "NVIDIA DualTV YUV Capture",
      "NVIDIA DualTV YUV Capture 2"
    };

    #endregion

    #region variables

    /// <summary>
    /// The main capture device.
    /// </summary>
    private DsDevice _deviceMain = null;

    /// <summary>
    /// The video capture device.
    /// </summary>
    private DsDevice _deviceVideo = null;

    /// <summary>
    /// The audio capture device.
    /// </summary>
    private DsDevice _deviceAudio = null;

    /// <summary>
    /// The video capture filter.
    /// </summary>
    private IBaseFilter _filterVideo = null;

    /// <summary>
    /// The audio capture filter.
    /// </summary>
    protected IBaseFilter _filterAudio = null;

    /// <summary>
    /// The stream configuration interface.
    /// </summary>
    private IAMStreamConfig _interfaceStreamConfiguration = null;

    // Current setting values.
    private TveAnalogVideoStandard _currentVideoStandard = TveAnalogVideoStandard.None;
    private FrameSize _currentFrameSize = FrameSize.Automatic;
    private FrameRate _currentFrameRate = FrameRate.Automatic;

    // Supported values for settings.
    private TveAnalogVideoStandard _supportedVideoStandards = TveAnalogVideoStandard.None;
    private FrameSize _supportedFrameSizes = FrameSize.Automatic;
    private FrameRate _supportedFrameRates = FrameRate.Automatic;

    /// <summary>
    /// A map containing the supported video processing amplifier and camera
    /// control properties, their current and default values, and their limits.
    /// </summary>
    private Dictionary<VideoOrCameraProperty, TunerProperty> _currentVideoOrCameraPropertySettings = new Dictionary<VideoOrCameraProperty, TunerProperty>();

    /// <summary>
    /// Configuration cache.
    /// </summary>
    private TVDatabase.Entities.Tuner _configuration = null;

    #endregion

    #region properties

    /// <summary>
    /// Get the video capture filter.
    /// </summary>
    public IBaseFilter VideoFilter
    {
      get
      {
        return _filterVideo;
      }
    }

    /// <summary>
    /// Get the audio capture filter.
    /// </summary>
    public IBaseFilter AudioFilter
    {
      get
      {
        return _filterAudio;
      }
    }

    /// <summary>
    /// Get the the capture device's current video standard.
    /// </summary>
    public TveAnalogVideoStandard CurrentVideoStandard
    {
      get
      {
        return _currentVideoStandard;
      }
    }

    /// <summary>
    /// Get the video standards supported by the capture device.
    /// </summary>
    public TveAnalogVideoStandard SupportedVideoStandards
    {
      get
      {
        return _supportedVideoStandards;
      }
    }

    /// <summary>
    /// Get the video frame sizes supported by the capture device.
    /// </summary>
    public FrameSize SupportedFrameSizes
    {
      get
      {
        return _supportedFrameSizes;
      }
    }

    /// <summary>
    /// Get the video frame rates supported by the capture device.
    /// </summary>
    public FrameRate SupportedFrameRates
    {
      get
      {
        return _supportedFrameRates;
      }
    }

    /// <summary>
    /// Get the properties supported by the capture device.
    /// </summary>
    public IEnumerable<TunerProperty> SupportedProperties
    {
      get
      {
        return _currentVideoOrCameraPropertySettings.Values;
      }
    }

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="Capture"/> class.
    /// </summary>
    public Capture()
    {
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="Capture"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public Capture(DsDevice device)
    {
      _deviceMain = device;
    }

    #endregion

    /// <summary>
    /// Load the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    /// <param name="productInstanceId">A common identifier shared by the tuner's components.</param>
    /// <param name="crossbar">The crossbar component.</param>
    public void PerformLoading(IFilterGraph2 graph, string productInstanceId, Crossbar crossbar)
    {
      if (_deviceMain != null)
      {
        this.LogDebug("WDM analog capture: perform loading (main)");
        if (!DevicesInUse.Instance.Add(_deviceMain))
        {
          throw new TvException("Main capture component is in use.");
        }
        try
        {
          _filterVideo = FilterGraphTools.AddFilterFromDevice(graph, _deviceMain);
        }
        catch (Exception ex)
        {
          DevicesInUse.Instance.Remove(_deviceMain);
          throw new TvException(ex, "Failed to add filter for main capture component to graph.");
        }
        bool isVideoSource;
        bool isAudioSource;
        IsVideoOrAudioSource(out isVideoSource, out isAudioSource);
        if (isAudioSource)
        {
          _filterAudio = _filterVideo;
        }
        if (!isVideoSource && isAudioSource)
        {
          _filterVideo = null;
        }
      }
      else
      {
        this.LogDebug("WDM analog capture: perform loading");

        int crossbarOutputPinIndexVideo = crossbar.PinIndexOutputVideo;
        if (crossbarOutputPinIndexVideo >= 0)
        {
          this.LogDebug("WDM analog capture: add video capture");
          IPin crossbarOutputPinVideo = DsFindPin.ByDirection(crossbar.Filter, PinDirection.Output, crossbarOutputPinIndexVideo);
          try
          {
            if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, crossbarOutputPinVideo, FilterCategory.AMKSCapture, out _filterVideo, out _deviceVideo, productInstanceId, PinDirection.Output, CAPTURE_DEVICE_BLACKLIST) &&
              !FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, crossbarOutputPinVideo, FilterCategory.VideoInputDevice, out _filterVideo, out _deviceVideo, productInstanceId, PinDirection.Output, CAPTURE_DEVICE_BLACKLIST))
            {
              throw new TvException("Failed to connect video capture.");
            }
          }
          finally
          {
            Release.ComObject("WDM analog capture crossbar video output pin", ref crossbarOutputPinVideo);
          }
        }

        int crossbarOutputPinIndexAudio = crossbar.PinIndexOutputAudio;
        if (crossbarOutputPinIndexAudio >= 0)
        {
          IPin crossbarOutputPinAudio = DsFindPin.ByDirection(crossbar.Filter, PinDirection.Output, crossbarOutputPinIndexAudio);
          try
          {
            if (_filterVideo != null)
            {
              this.LogDebug("WDM analog capture: connect crossbar audio to video capture");
              if (FilterGraphTools.ConnectFilterWithPin(graph, crossbarOutputPinAudio, PinDirection.Output, _filterVideo))
              {
                _filterAudio = _filterVideo;
                _deviceAudio = _deviceVideo;
              }
            }
            if (_filterAudio == null)
            {
              this.LogDebug("WDM analog capture: add audio capture");
              if (!FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, crossbarOutputPinAudio, FilterCategory.AMKSCapture, out _filterAudio, out _deviceAudio, productInstanceId) &&
                !FilterGraphTools.AddAndConnectHardwareFilterByCategoryAndMedium(graph, crossbarOutputPinAudio, FilterCategory.AudioInputDevice, out _filterAudio, out _deviceAudio, productInstanceId))
              {
                throw new TvException("Failed to connect audio capture.");
              }
            }
          }
          finally
          {
            Release.ComObject("WDM analog capture crossbar audio output pin", ref crossbarOutputPinAudio);
          }
        }
      }

      CheckCapabilitiesAnalogVideoDecoder();
      CheckCapabilitiesVideoProcessingAmplifier();
      CheckCapabilitiesCameraControl();
    }

    /// <summary>
    /// Try to determine if the capture source is a video or audio source.
    /// </summary>
    /// <param name="isVideoSource"><c>True</c> if the capture source is a video source.</param>
    /// <param name="isAudioSource"><c>True</c> if the capture source is an audio source.</param>
    private void IsVideoOrAudioSource(out bool isVideoSource, out bool isAudioSource)
    {
      this.LogDebug("WDM analog capture: is video or audio source");
      isVideoSource = false;
      isAudioSource = false;

      IEnumPins pinEnum;
      int hr = _filterVideo.EnumPins(out pinEnum);
      TvExceptionDirectShowError.Throw(hr, "Failed to obtain pin enumerator for filter.");
      try
      {
        int pinIndex = 0;
        int pinCount = 0;
        IPin[] pins = new IPin[2];
        while (pinEnum.Next(1, pins, out pinCount) == (int)NativeMethods.HResult.S_OK && pinCount == 1)
        {
          IPin pin = pins[0];
          try
          {
            IEnumMediaTypes mediaTypeEnum;
            hr = pin.EnumMediaTypes(out mediaTypeEnum);
            TvExceptionDirectShowError.Throw(hr, "Failed to obtain media type enumerator for pin.");
            try
            {
              // For each pin media type...
              int mediaTypeCount;
              AMMediaType[] mediaTypes = new AMMediaType[2];
              while (mediaTypeEnum.Next(1, mediaTypes, out mediaTypeCount) == (int)NativeMethods.HResult.S_OK && mediaTypeCount == 1)
              {
                AMMediaType mediaType = mediaTypes[0];
                try
                {
                  if (mediaType.majorType == WdmMediaType.AnalogVideo || mediaType.majorType == WdmMediaType.Video)
                  {
                    this.LogDebug("WDM analog capture: pin {0} is a video pin", pinIndex);
                    isVideoSource = true;
                  }
                  else if (mediaType.majorType == WdmMediaType.AnalogAudio || mediaType.majorType == WdmMediaType.Audio)
                  {
                    this.LogDebug("WDM analog capture: pin {0} is an audio pin", pinIndex);
                    isAudioSource = true;
                  }
                  if (isVideoSource && isAudioSource)
                  {
                    break;
                  }
                }
                finally
                {
                  Release.AmMediaType(ref mediaType);
                }
              }
            }
            finally
            {
              Release.ComObject("WDM analog capture pin media type enumerator", ref mediaTypeEnum);
            }
          }
          finally
          {
            pinIndex++;
            Release.ComObject("WDM analog capture filter video/audio test pin", ref pin);
          }
        }
      }
      finally
      {
        Release.ComObject("WDM analog capture filter video/audio pin enumerator", ref pinEnum);
      }
    }

    /// <summary>
    /// Set/override the audio capture filter.
    /// </summary>
    /// <remarks>
    /// This function is used by the encoder component when it discovers that
    /// what it thought was an audio encoder filter is actually an audio
    /// capture filter.
    /// The Hauppauge Nova S Plus crossbar video and audio outputs both connect
    /// to a capture filter. To us it looks like that filter is a combined
    /// video and audio capture filter, but actually a separate audio capture
    /// filter is required. The audio output pin "I2S" on the video capture
    /// filter claims to be a PCM output, but it won't connect to any filter
    /// except the dedicated hardware audio capture filter.
    /// </remarks>
    /// <param name="filter">The audio capture filter.</param>
    /// <param name="device">The audio capture device.</param>
    public void SetAudioCapture(IBaseFilter filter, DsDevice device)
    {
      _filterAudio = filter;
      _deviceAudio = device;
    }

    /// <summary>
    /// Set the stream configuration interface.
    /// </summary>
    /// <remarks>
    /// This function is used by the encoder component when it has found the
    /// video output pin.
    /// </remarks>
    /// <param name="streamConfig">The stream configuration interface.</param>
    public void SetStreamConfigInterface(IAMStreamConfig streamConfig)
    {
      _interfaceStreamConfiguration = streamConfig;
      if (streamConfig != null)
      {
        this.LogDebug("WDM analog capture: found stream configuration interface");

        // It seems that available stream capabilities depend on the selected
        // video standard. Therefore we cycle through all supported video
        // standards to build the full set of stream capabilities.
        foreach (TveAnalogVideoStandard standard in System.Enum.GetValues(typeof(TveAnalogVideoStandard)))
        {
          if (standard != TveAnalogVideoStandard.None && _supportedVideoStandards.HasFlag(standard))
          {
            ConfigureAnalogVideoDecoder(standard);
            CheckCapabilitiesStream();
          }
        }
      }
      else if (_filterVideo != null)
      {
        this.LogWarn("WDM analog capture: failed to find stream configuration interface");
      }

      // For some hardware, configuration has to be set before the encoder is connected.
      ReloadConfiguration(_configuration);
    }

    #region check capabilities

    /// <summary>
    /// Check the capabilites of the analog video decoder interface.
    /// </summary>
    private void CheckCapabilitiesAnalogVideoDecoder()
    {
      if (_filterVideo == null)
      {
        return;
      }

      IAMAnalogVideoDecoder analogVideoDecoder = _filterVideo as IAMAnalogVideoDecoder;
      if (analogVideoDecoder == null)
      {
        this.LogWarn("WDM analog capture: failed to find analog video decoder interface on capture filter, not able to check video decoder capabilities");
      }

      WdmAnalogVideoStandard videoStandard;
      int hr = analogVideoDecoder.get_AvailableTVFormats(out videoStandard);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        _supportedVideoStandards = TveAnalogVideoStandard.None;
        this.LogWarn("WDM analog capture: failed to get supported video standards, hr = 0x{0:x}", hr);
      }
      else
      {
        this.LogDebug("WDM analog capture: supported video standards = {0}", videoStandard);
        _supportedVideoStandards = GetTveVideoStandards(videoStandard);
      }

      hr = analogVideoDecoder.get_TVFormat(out videoStandard);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        _currentVideoStandard = TveAnalogVideoStandard.None;
        this.LogWarn("WDM analog capture: failed to get current video standard, hr = 0x{0:x}", hr);
      }
      else
      {
        this.LogDebug("WDM analog capture: current video standard = {0}", videoStandard);
        _currentVideoStandard = GetTveVideoStandards(videoStandard);
      }
    }

    /// <summary>
    /// Check the capabilites of the stream configuration interface.
    /// </summary>
    private void CheckCapabilitiesStream()
    {
      if (_interfaceStreamConfiguration == null)
      {
        return;
      }

      int countCapabilities;
      int streamConfigCapabilitiesSize;
      int hr = _interfaceStreamConfiguration.GetNumberOfCapabilities(out countCapabilities, out streamConfigCapabilitiesSize);
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        TvExceptionDirectShowError.Throw(hr, "Failed to get stream configuration capability count.");
      }
      this.LogDebug("WDM analog capture: supported stream configuration capability count = {0}", countCapabilities);

      int outputWidthMinimum = 100000;
      int outputWidthMaximum = 0;
      int outputHeightMinimum = 100000;
      int outputHeightMaximum = 0;
      AMMediaType format;
      FrameSize frameSize;
      FrameRate frameRate;
      IntPtr configBuffer = Marshal.AllocCoTaskMem(streamConfigCapabilitiesSize);
      try
      {
        for (int i = 0; i < countCapabilities; i++)
        {
          hr = _interfaceStreamConfiguration.GetStreamCaps(i, out format, configBuffer);
          TvExceptionDirectShowError.Throw(hr, "Failed to get stream configuration format {0} information.", i);

          try
          {
            GetTveFrameDetails(format, out frameSize, out frameRate);
            if (frameSize != FrameSize.Automatic || frameRate != FrameRate.Automatic)
            {
              this.LogDebug("WDM analog capture:   frame size = {0}, frame rate = {1}", frameSize, frameRate);
              _supportedFrameSizes |= frameSize;
              _supportedFrameRates |= frameRate;
            }

            VideoStreamConfigCaps configCaps = (VideoStreamConfigCaps)Marshal.PtrToStructure(configBuffer, typeof(VideoStreamConfigCaps));
            if (configCaps.MinOutputSize.Width < outputWidthMinimum)
            {
              outputWidthMinimum = configCaps.MinOutputSize.Width;
            }
            if (configCaps.MinOutputSize.Height < outputHeightMinimum)
            {
              outputHeightMinimum = configCaps.MinOutputSize.Height;
            }
            if (configCaps.MaxOutputSize.Width > outputWidthMaximum)
            {
              outputWidthMaximum = configCaps.MaxOutputSize.Width;
            }
            if (configCaps.MaxOutputSize.Height > outputHeightMaximum)
            {
              outputHeightMaximum = configCaps.MaxOutputSize.Height;
            }
          }
          finally
          {
            Release.AmMediaType(ref format);
          }
        }

        if (
          outputWidthMinimum < outputWidthMaximum &&
          outputHeightMinimum < outputHeightMaximum &&
          outputWidthMinimum > 0 &&
          outputHeightMinimum > 0
        )
        {
          this.LogDebug("WDM analog capture: include frame sizes between {0}x{1} and {2}x{3}", outputWidthMinimum, outputHeightMinimum, outputWidthMaximum, outputHeightMaximum);
          foreach (FrameSize frameSize1 in System.Enum.GetValues(typeof(FrameSize)))
          {
            int frameWidth = GetTveFrameWidth(frameSize1);
            int frameHeight = GetTveFrameHeight(frameSize1);
            if (
              frameWidth > 0 &&
              frameWidth >= outputWidthMinimum &&
              frameWidth <= outputWidthMaximum &&
              frameHeight > 0 &&
              frameHeight >= outputHeightMinimum &&
              frameHeight <= outputHeightMaximum
            )
            {
              this.LogDebug("WDM analog capture:   frame size = {0}", frameSize1);
              _supportedFrameSizes |= frameSize1;
            }
          }
        }
      }
      finally
      {
        Marshal.FreeCoTaskMem(configBuffer);
      }

      hr = _interfaceStreamConfiguration.GetFormat(out format);
      TvExceptionDirectShowError.Throw(hr, "Failed to get current stream configuration format information.");
      try
      {
        GetTveFrameDetails(format, out frameSize, out frameRate);
        this.LogDebug("WDM analog capture: current stream configuration, frame size = {0}, frame rate = {1}, format = {2}", frameSize, frameRate, format.formatType);
        _currentFrameSize = frameSize;
        _currentFrameRate = frameRate;
      }
      finally
      {
        Release.AmMediaType(ref format);
      }
    }

    private static void ReadBmiHeader(ref BitmapInfoHeader bmiHeader, out int width, out int height)
    {
      if (bmiHeader == null || bmiHeader.Size < 8)
      {
        Log.Warn("WDM analog capture: not possible to read frame size");
        width = -1;
        height = -1;
      }
      else
      {
        width = bmiHeader.Width;
        height = bmiHeader.Height;
      }
    }

    /// <summary>
    /// Check the capabilites of the video processing amplifier interface.
    /// </summary>
    private void CheckCapabilitiesVideoProcessingAmplifier()
    {
      if (_filterVideo == null)
      {
        return;
      }

      IAMVideoProcAmp videoProcAmp = _filterVideo as IAMVideoProcAmp;
      if (videoProcAmp == null)
      {
        this.LogWarn("WDM analog capture: failed to find video processing amplifier interface on capture filter, not able to check video processing amplifier capabilities");
        return;
      }

      int value = 0;
      int valueMinimum = 0;
      int valueMaximum = 0;
      int steppingDelta = 1;
      int valueDefault = 0;
      VideoProcAmpFlags flagsSupported = VideoProcAmpFlags.None;
      VideoProcAmpFlags flagsValue = VideoProcAmpFlags.None;
      int hr = (int)NativeMethods.HResult.S_OK;
      this.LogDebug("WDM analog capture: video processing amplifier properties...");
      foreach (VideoProcAmpProperty property in System.Enum.GetValues(typeof(VideoProcAmpProperty)))
      {
        hr = videoProcAmp.GetRange(property, out valueMinimum, out valueMaximum, out steppingDelta, out valueDefault, out flagsSupported);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("WDM analog capture:   {0} is not supported by the hardware", property);
          continue;
        }

        this.LogDebug("WDM analog capture:   {0} is supported, min = {1}, max = {2}, step = {3}, default = {4}, flags = {5}", property, valueMinimum, valueMaximum, steppingDelta, valueDefault, flagsSupported);
        VideoOrCameraProperty? mpProperty = GetTveVideoOrCameraProperty(property);
        if (!mpProperty.HasValue)
        {
          this.LogWarn("WDM analog capture: video processing amplifier property {0} is not supported by TV Server", property);
          continue;
        }

        hr = videoProcAmp.Get(property, out value, out flagsValue);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("WDM analog capture: failed to get current value for supported video processing amplifier property {0}", property);
          value = valueDefault;
          if (flagsSupported.HasFlag(VideoProcAmpFlags.Auto))
          {
            flagsValue = VideoProcAmpFlags.Auto;
          }
          else if (flagsSupported.HasFlag(VideoProcAmpFlags.Manual))
          {
            flagsValue = VideoProcAmpFlags.Manual;
          }
        }
        else
        {
          this.LogDebug("WDM analog capture:     current value = {0}, flag = {1}", value, flagsValue);
        }

        _currentVideoOrCameraPropertySettings.Add(mpProperty.Value, new TunerProperty
        {
          PropertyId = (int)mpProperty.Value,
          Value = value,
          Default = valueDefault,
          Minimum = valueMinimum,
          Maximum = valueMaximum,
          Step = steppingDelta,
          PossibleValueFlags = (int)GetTveVideoOrCameraPropertyFlags(flagsSupported),
          ValueFlags = (int)GetTveVideoOrCameraPropertyFlags(flagsValue)
        });
      }
    }

    /// <summary>
    /// Check the capabilites of the camera control interface.
    /// </summary>
    private void CheckCapabilitiesCameraControl()
    {
      if (_filterVideo == null)
      {
        return;
      }

      IAMCameraControl cameraControl = _filterVideo as IAMCameraControl;
      if (cameraControl == null)
      {
        this.LogWarn("WDM analog capture: failed to find camera control interface on capture filter, not able to check camera control capabilities");
        return;
      }

      int value = 0;
      int valueMinimum = 0;
      int valueMaximum = 0;
      int steppingDelta = 1;
      int valueDefault = 0;
      CameraControlFlags flagsSupported = CameraControlFlags.None;
      CameraControlFlags flagsValue = CameraControlFlags.None;
      int hr = (int)NativeMethods.HResult.S_OK;
      this.LogDebug("WDM analog capture: camera control properties...");
      foreach (CameraControlProperty property in System.Enum.GetValues(typeof(CameraControlProperty)))
      {
        hr = cameraControl.GetRange(property, out valueMinimum, out valueMaximum, out steppingDelta, out valueDefault, out flagsSupported);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogDebug("WDM analog capture:   {0} is not supported by the hardware", property);
          continue;
        }

        this.LogDebug("WDM analog capture:   {0} is supported, min = {1}, max = {2}, step = {3}, default = {4}, flags = {5}", property, valueMinimum, valueMaximum, steppingDelta, valueDefault, flagsSupported);
        VideoOrCameraProperty? mpProperty = GetMpVideoOrCameraProperty(property);
        if (!mpProperty.HasValue)
        {
          this.LogWarn("WDM analog capture: camera control property {0} is not supported by TV Server", property);
          continue;
        }

        hr = cameraControl.Get(property, out value, out flagsValue);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("WDM analog capture: failed to get current value for supported camera control property {0}", property);
          value = valueDefault;
          if (flagsSupported.HasFlag(CameraControlFlags.Auto))
          {
            flagsValue = CameraControlFlags.Auto;
          }
          else if (flagsSupported.HasFlag(CameraControlFlags.Manual))
          {
            flagsValue = CameraControlFlags.Manual;
          }
        }
        else
        {
          this.LogDebug("WDM analog capture:     current value = {0}, flag = {1}", value, flagsValue);
        }

        _currentVideoOrCameraPropertySettings.Add(mpProperty.Value, new TunerProperty
        {
          PropertyId = (int)mpProperty.Value,
          Value = value,
          Default = valueDefault,
          Minimum = valueMinimum,
          Maximum = valueMaximum,
          Step = steppingDelta,
          PossibleValueFlags = (int)GetTveVideoOrCameraPropertyFlags(flagsSupported),
          ValueFlags = (int)GetTveVideoOrCameraPropertyFlags(flagsValue)
        });
      }
    }

    #endregion

    #region configuration

    /// <summary>
    /// Reload the component's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public void ReloadConfiguration(TVDatabase.Entities.Tuner configuration)
    {
      if (_configuration == null || _filterVideo == null)
      {
        // Not loaded yet or video not supported.
        _configuration = configuration;
        return;
      }

      this.LogDebug("WDM analog capture: reload configuration");

      if (configuration.AnalogTunerSettings.VideoStandard != (int)_currentVideoStandard)
      {
        ConfigureAnalogVideoDecoder((TveAnalogVideoStandard)configuration.AnalogTunerSettings.VideoStandard);
      }

      if (configuration.AnalogTunerSettings.FrameSize != (int)_currentFrameSize ||
        configuration.AnalogTunerSettings.FrameRate != (int)_currentFrameRate)
      {
        ConfigureStream((FrameSize)configuration.AnalogTunerSettings.FrameSize, (FrameRate)configuration.AnalogTunerSettings.FrameRate);
      }

      if (configuration.TunerProperties != null && configuration.TunerProperties.Count > 0)
      {
        ConfigureVideoProcessingAmplifier(configuration.TunerProperties);
        ConfigureCameraControl(configuration.TunerProperties);
      }
    }

    public void OnGraphCompleted()
    {
      // For some hardware, configuration has to be set after the encoder is connected.
      ReloadConfiguration(_configuration);
    }

    /// <summary>
    /// Configure the analog video decoder interface.
    /// </summary>
    /// <param name="videoStandard">The decoder video standard.</param>
    private void ConfigureAnalogVideoDecoder(TveAnalogVideoStandard videoStandard)
    {
      if (_filterVideo == null || videoStandard == TveAnalogVideoStandard.None)
      {
        return;
      }

      IAMAnalogVideoDecoder analogVideoDecoder = _filterVideo as IAMAnalogVideoDecoder;
      if (analogVideoDecoder == null)
      {
        this.LogWarn("WDM analog capture: failed to find analog video decoder interface on capture filter, not able to configure decoder");
        return;
      }

      if (!_supportedVideoStandards.HasFlag(videoStandard))
      {
        this.LogWarn("WDM analog capture: requested video standard {0} is not supported", videoStandard);
        return;
      }

      this.LogDebug("WDM analog capture: configure analog video decoder, standard = {0}", videoStandard);
      int hr = analogVideoDecoder.put_TVFormat(GetWdmVideoStandard(videoStandard));
      if (hr != (int)NativeMethods.HResult.S_OK)
      {
        this.LogError("WDM analog capture: failed to set video standard, hr = 0x{0:x}, standard = {1}", hr, videoStandard);
      }
      else
      {
        _currentVideoStandard = videoStandard;
      }
    }

    /// <summary>
    /// Configure the stream configuration interface.
    /// </summary>
    /// <param name="frameSize">The video frame size.</param>
    /// <param name="frameRate">The video frame rate.</param>
    private void ConfigureStream(FrameSize frameSize, FrameRate frameRate)
    {
      if (_interfaceStreamConfiguration == null)
      {
        return;
      }

      if (!_supportedFrameSizes.HasFlag(frameSize))
      {
        this.LogWarn("WDM analog capture: requested frame size {0} is not supported", frameSize);
        return;
      }
      if (!_supportedFrameRates.HasFlag(frameRate))
      {
        this.LogWarn("WDM analog capture: requested frame rate {0} is not supported", frameRate);
        return;
      }

      // Get the current format information.
      AMMediaType format;
      int hr = _interfaceStreamConfiguration.GetFormat(out format);
      try
      {
        TvExceptionDirectShowError.Throw(hr, "Failed to get stream configuration format information.");

        this.LogDebug("WDM analog capture: configure stream, frame size = {0}, frame rate = {1}, format = {2}", frameSize, frameRate, format.formatType);
        double rawFrameRate = 50;
        switch (frameRate)
        {
          case FrameRate.Fr15:
            rawFrameRate = 15;
            break;
          case FrameRate.Fr23_976:
            rawFrameRate = 23.976;
            break;
          case FrameRate.Fr24:
            rawFrameRate = 24;
            break;
          case FrameRate.Fr25:
            rawFrameRate = 25;
            break;
          case FrameRate.Fr29_97:
            rawFrameRate = 29.97;
            break;
          case FrameRate.Fr30:
            rawFrameRate = 30;
            break;
          case FrameRate.Fr50:
            rawFrameRate = 50;
            break;
          case FrameRate.Fr59_94:
            rawFrameRate = 59.94;
            break;
          case FrameRate.Fr60:
            rawFrameRate = 60;
            break;
        }
        long averageTimePerFrame = (long)(10000000d / rawFrameRate);

        // The structure of the content of formatPtr depends on formatType.
        object formatStruct = null;
        if (format.formatType == FormatType.VideoInfo)
        {
          VideoInfoHeader temp = new VideoInfoHeader();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.BmiHeader, frameSize);
          if (frameRate != FrameRate.Automatic)
          {
            temp.AvgTimePerFrame = averageTimePerFrame;
          }
          formatStruct = temp;
        }
        else if (format.formatType == FormatType.VideoInfo2)
        {
          VideoInfoHeader2 temp = new VideoInfoHeader2();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.BmiHeader, frameSize);
          if (frameRate != FrameRate.Automatic)
          {
            temp.AvgTimePerFrame = averageTimePerFrame;
          }
          formatStruct = temp;
        }
        else if (format.formatType == FormatType.Mpeg2Video)
        {
          MPEG2VideoInfo temp = new MPEG2VideoInfo();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.hdr.BmiHeader, frameSize);
          if (frameRate != FrameRate.Automatic)
          {
            temp.hdr.AvgTimePerFrame = averageTimePerFrame;
          }
          formatStruct = temp;
        }
        else if (format.formatType == FormatType.MpegVideo)
        {
          MPEG1VideoInfo temp = new MPEG1VideoInfo();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.hdr.BmiHeader, frameSize);
          if (frameRate != FrameRate.Automatic)
          {
            temp.hdr.AvgTimePerFrame = averageTimePerFrame;
          }
          formatStruct = temp;
        }
        else
        {
          this.LogWarn("WDM analog capture: format type {0} is not supported, not possible to configure stream", format.formatType);
        }

        if (formatStruct != null && (frameSize != FrameSize.Automatic || frameRate != FrameRate.Automatic))
        {
          Marshal.StructureToPtr(formatStruct, format.formatPtr, false);
          hr = _interfaceStreamConfiguration.SetFormat(format);
          TvExceptionDirectShowError.Throw(hr, "Failed to set stream configuration format information, frame size = {0}, frame rate = {1}, format type = {2}.", frameSize, frameRate, format.formatType);
          _currentFrameSize = frameSize;
          _currentFrameRate = frameRate;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "WDM analog capture: failed to configure stream, frame size = {0}, frame rate = {1}, format type = {2}", frameSize, frameRate, format.formatType);
      }
      finally
      {
        Release.AmMediaType(ref format);
      }
    }

    private void UpdateBmiHeader(ref BitmapInfoHeader bmiHeader, FrameSize frameSize)
    {
      if (bmiHeader == null || bmiHeader.Size < 8)
      {
        this.LogWarn("WDM analog capture: not possible to set frame size");
        return;
      }

      switch (frameSize)
      {
        case FrameSize.Fs320_240:
          bmiHeader.Width = 320;
          bmiHeader.Height = 240;
          break;
        case FrameSize.Fs352_240:
          bmiHeader.Width = 352;
          bmiHeader.Height = 240;
          break;
        case FrameSize.Fs352_288:
          bmiHeader.Width = 352;
          bmiHeader.Height = 288;
          break;
        case FrameSize.Fs384_288:
          bmiHeader.Width = 384;
          bmiHeader.Height = 288;
          break;
        case FrameSize.Fs480_360:
          bmiHeader.Width = 480;
          bmiHeader.Height = 360;
          break;
        case FrameSize.Fs640_360:
          bmiHeader.Width = 640;
          bmiHeader.Height = 360;
          break;
        case FrameSize.Fs640_480:
          bmiHeader.Width = 640;
          bmiHeader.Height = 480;
          break;
        case FrameSize.Fs704_480:
          bmiHeader.Width = 704;
          bmiHeader.Height = 480;
          break;
        case FrameSize.Fs704_576:
          bmiHeader.Width = 704;
          bmiHeader.Height = 576;
          break;
        case FrameSize.Fs720_480:
          bmiHeader.Width = 720;
          bmiHeader.Height = 480;
          break;
        case FrameSize.Fs720_576:
          bmiHeader.Width = 720;
          bmiHeader.Height = 576;
          break;
        case FrameSize.Fs768_480:
          bmiHeader.Width = 768;
          bmiHeader.Height = 480;
          break;
        case FrameSize.Fs768_576:
          bmiHeader.Width = 768;
          bmiHeader.Height = 576;
          break;
        case FrameSize.Fs1280_720:
          bmiHeader.Width = 1280;
          bmiHeader.Height = 720;
          break;
        case FrameSize.Fs1440_1080:
          bmiHeader.Width = 1440;
          bmiHeader.Height = 1080;
          break;
        case FrameSize.Fs1920_1080:
          bmiHeader.Width = 1920;
          bmiHeader.Height = 1080;
          break;
      }
    }

    /// <summary>
    /// Configure the video processing amplifier.
    /// </summary>
    /// <param name="newPropertySettings">The amplifier property settings.</param>
    private void ConfigureVideoProcessingAmplifier(ICollection<TunerProperty> newPropertySettings)
    {
      if (_filterVideo == null)
      {
        return;
      }

      IAMVideoProcAmp videoProcAmp = _filterVideo as IAMVideoProcAmp;
      if (videoProcAmp == null)
      {
        this.LogWarn("WDM analog capture: failed to find video processing amplifier interface on capture filter, not able to configure amplifier");
        return;
      }

      bool isVideoProcAmpProperty;
      VideoProcAmpProperty wdmProperty;
      TunerProperty propertyLimits;
      foreach (TunerProperty property in newPropertySettings)
      {
        VideoOrCameraProperty mpProperty = (VideoOrCameraProperty)property.PropertyId;
        VideoOrCameraPropertyFlag mpValueFlag = (VideoOrCameraPropertyFlag)property.ValueFlags;
        GetWdmVideoProcAmpProperty(mpProperty, out isVideoProcAmpProperty, out wdmProperty);
        if (!isVideoProcAmpProperty)
        {
          continue;
        }
        if (!_currentVideoOrCameraPropertySettings.TryGetValue(mpProperty, out propertyLimits))
        {
          this.LogWarn("WDM analog capture: requested video processing amplifier property {0} is not supported", mpProperty);
          continue;
        }
        if ((property.ValueFlags & propertyLimits.PossibleValueFlags) == 0)
        {
          this.LogWarn("WDM analog capture: requested video processing amplifier property flag {0} for property {1} is not supported", mpValueFlag, mpProperty);
          continue;
        }
        if (property.Value == propertyLimits.Value && property.ValueFlags == propertyLimits.ValueFlags)
        {
          continue;
        }

        // Confine to supported range and steps.
        int newValue = property.Value;
        VideoProcAmpFlags newFlag = GetWdmVideoProcAmpPropertyFlag((VideoOrCameraPropertyFlag)property.ValueFlags);
        int stepOffset = (newValue - propertyLimits.Minimum) % propertyLimits.Step;
        if (stepOffset > 0)
        {
          newValue -= stepOffset;
          if (stepOffset / propertyLimits.Step > 0.5)
          {
            newValue += propertyLimits.Step;
          }
        }
        if (newValue > propertyLimits.Maximum)
        {
          newValue = propertyLimits.Maximum;
        }
        else if (newValue < propertyLimits.Minimum)
        {
          newValue = propertyLimits.Minimum;
        }
        if (newValue == propertyLimits.Value && property.ValueFlags == propertyLimits.ValueFlags)
        {
          continue;
        }

        this.LogDebug("WDM analog capture: configure video processing amplifier property {0}, value = {1}, flag = {2}", mpProperty, newValue, mpValueFlag);
        int hr = videoProcAmp.Set(wdmProperty, newValue, newFlag);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("WDM analog capture: failed to set video processing amplifier property, hr = 0x{0:x}, property = {1}, value = {2}, flag = {3}", hr, wdmProperty, newValue, newFlag);
        }
        else
        {
          propertyLimits.Value = newValue;
          propertyLimits.ValueFlags = (int)mpValueFlag;
        }
      }
    }

    /// <summary>
    /// Configure the camera control.
    /// </summary>
    /// <param name="newPropertySettings">The control property settings.</param>
    private void ConfigureCameraControl(ICollection<TunerProperty> newPropertySettings)
    {
      if (_filterVideo == null)
      {
        return;
      }

      IAMCameraControl cameraControl = _filterVideo as IAMCameraControl;
      if (cameraControl == null)
      {
        this.LogWarn("WDM analog capture: failed to find camera control interface on capture filter, not able to configure control");
        return;
      }

      bool isCameraControlProperty;
      CameraControlProperty wdmProperty;
      TunerProperty propertyLimits;
      foreach (TunerProperty property in newPropertySettings)
      {
        VideoOrCameraProperty mpProperty = (VideoOrCameraProperty)property.PropertyId;
        VideoOrCameraPropertyFlag mpValueFlag = (VideoOrCameraPropertyFlag)property.ValueFlags;
        GetWdmCameraControlProperty(mpProperty, out isCameraControlProperty, out wdmProperty);
        if (!isCameraControlProperty)
        {
          continue;
        }
        if (!_currentVideoOrCameraPropertySettings.TryGetValue(mpProperty, out propertyLimits))
        {
          this.LogWarn("WDM analog capture: requested camera control property {0} is not supported", mpProperty);
          continue;
        }
        if ((property.ValueFlags & propertyLimits.PossibleValueFlags) == 0)
        {
          this.LogWarn("WDM analog capture: requested camera control property flag {0} for property {1} is not supported", mpValueFlag, mpProperty);
          continue;
        }
        if (property.Value == propertyLimits.Value && property.ValueFlags == propertyLimits.ValueFlags)
        {
          continue;
        }

        // Confine to supported range and steps.
        int newValue = property.Value;
        CameraControlFlags newFlag = GetWdmCameraControlPropertyFlag((VideoOrCameraPropertyFlag)property.ValueFlags);
        int stepOffset = (newValue - propertyLimits.Minimum) % propertyLimits.Step;
        if (stepOffset > 0)
        {
          newValue -= stepOffset;
          if (stepOffset / propertyLimits.Step > 0.5)
          {
            newValue += propertyLimits.Step;
          }
        }
        if (newValue > propertyLimits.Maximum)
        {
          newValue = propertyLimits.Maximum;
        }
        else if (newValue < propertyLimits.Minimum)
        {
          newValue = propertyLimits.Minimum;
        }
        if (newValue == propertyLimits.Value && property.ValueFlags == propertyLimits.ValueFlags)
        {
          continue;
        }

        this.LogDebug("WDM analog capture: configure camera control property {0}, value = {1}, flag = {2}", mpProperty, newValue, mpValueFlag);
        int hr = cameraControl.Set(wdmProperty, newValue, newFlag);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogError("WDM analog capture: failed to set camera control property, hr = 0x{0:x}, property = {1}, value = {2}, flag = {3}", hr, wdmProperty, newValue, newFlag);
        }
        else
        {
          propertyLimits.Value = newValue;
          propertyLimits.ValueFlags = (int)mpValueFlag;
        }
      }
    }

    #endregion

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void PerformTuning(IChannel channel)
    {
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel != null && channel.MediaType == TveMediaType.Television && _filterVideo != null)
      {
        this.LogDebug("WDM analog capture: perform tuning");
        IAMAnalogVideoDecoder analogVideoDecoder = _filterVideo as IAMAnalogVideoDecoder;
        if (analogVideoDecoder != null)
        {
          // This property is not always supported, so don't throw an exception on failure.
          int hr = analogVideoDecoder.put_VCRHorizontalLocking(captureChannel.IsVcrSignal);
          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("WDM analog capture: failed to set VCR horizontal locking, hr = 0x{0:x}", hr);
          }
        }
        else
        {
          this.LogWarn("WDM analog capture: failed to find analog video decoder interface on capture filter, not able to apply VCR horizontal locking");
        }
      }
    }

    /// <summary>
    /// Unload the component.
    /// </summary>
    /// <param name="graph">The tuner's DirectShow graph.</param>
    public void PerformUnloading(IFilterGraph2 graph)
    {
      this.LogDebug("WDM analog capture: perform unloading");

      _currentVideoStandard = TveAnalogVideoStandard.None;
      _supportedVideoStandards = TveAnalogVideoStandard.None;
      _currentFrameSize = FrameSize.Automatic;
      _supportedFrameSizes = FrameSize.Automatic;
      _currentFrameRate = FrameRate.Automatic;
      _supportedFrameRates = FrameRate.Automatic;
      _currentVideoOrCameraPropertySettings.Clear();

      // The stream interface is found on an output pin, so we must release the
      // reference to avoid a leak.
      Release.ComObject("WDM analog capture stream format interface", ref _interfaceStreamConfiguration);

      if (_filterVideo == null && _filterAudio == null)
      {
        return;
      }

      if (graph != null)
      {
        if (_filterAudio != null && _filterAudio != _filterVideo)
        {
          graph.RemoveFilter(_filterAudio);
        }
        graph.RemoveFilter(_filterVideo);
      }
      if (_filterAudio != null && _filterAudio != _filterVideo)
      {
        Release.ComObject("WDM analog capture audio filter", ref _filterAudio);
      }
      Release.ComObject("WDM analog capture video filter", ref _filterVideo);
      _filterAudio = null;

      if (_deviceMain != null)
      {
        DevicesInUse.Instance.Remove(_deviceMain);
        // Do NOT Dispose() or set the main device to NULL. We would be unable
        // to reload. The tuner instance that instanciated this capture is
        // responsible for disposing it.
        return;
      }

      if (_deviceAudio != null && _deviceAudio != _deviceVideo)
      {
        DevicesInUse.Instance.Remove(_deviceAudio);
        _deviceAudio.Dispose();
        _deviceAudio = null;
      }
      if (_deviceVideo != null)
      {
        DevicesInUse.Instance.Remove(_deviceVideo);
        _deviceVideo.Dispose();
        _deviceVideo = null;
      }
      _deviceAudio = null;
    }

    #region parameter translation

    private static WdmAnalogVideoStandard GetWdmVideoStandard(TveAnalogVideoStandard videoStandard)
    {
      switch (videoStandard)
      {
        case TveAnalogVideoStandard.NtscM:
          return WdmAnalogVideoStandard.NTSC_M;
        case TveAnalogVideoStandard.NtscMj:
          return WdmAnalogVideoStandard.NTSC_M_J;
        case TveAnalogVideoStandard.Ntsc433:
          return WdmAnalogVideoStandard.NTSC_433;
        case TveAnalogVideoStandard.PalB:
          return WdmAnalogVideoStandard.PAL_B;
        case TveAnalogVideoStandard.PalD:
          return WdmAnalogVideoStandard.PAL_D;
        case TveAnalogVideoStandard.PalG:
          return WdmAnalogVideoStandard.PAL_G;
        case TveAnalogVideoStandard.PalH:
          return WdmAnalogVideoStandard.PAL_H;
        case TveAnalogVideoStandard.PalI:
          return WdmAnalogVideoStandard.PAL_I;
        case TveAnalogVideoStandard.PalM:
          return WdmAnalogVideoStandard.PAL_M;
        case TveAnalogVideoStandard.PalN:
          return WdmAnalogVideoStandard.PAL_N;
        case TveAnalogVideoStandard.Pal60:
          return WdmAnalogVideoStandard.PAL_60;
        case TveAnalogVideoStandard.SecamB:
          return WdmAnalogVideoStandard.SECAM_B;
        case TveAnalogVideoStandard.SecamD:
          return WdmAnalogVideoStandard.SECAM_D;
        case TveAnalogVideoStandard.SecamG:
          return WdmAnalogVideoStandard.SECAM_G;
        case TveAnalogVideoStandard.SecamH:
          return WdmAnalogVideoStandard.SECAM_H;
        case TveAnalogVideoStandard.SecamK:
          return WdmAnalogVideoStandard.SECAM_K;
        case TveAnalogVideoStandard.SecamK1:
          return WdmAnalogVideoStandard.SECAM_K1;
        case TveAnalogVideoStandard.SecamL:
          return WdmAnalogVideoStandard.SECAM_L;
        case TveAnalogVideoStandard.SecamL1:
          return WdmAnalogVideoStandard.SECAM_L1;
        case TveAnalogVideoStandard.PalNCombo:
          return WdmAnalogVideoStandard.PAL_N_COMBO;
        default:
          Log.Warn("WDM analog capture: unsupported analog video standard {0}, falling back to PAL B", videoStandard);
          return WdmAnalogVideoStandard.PAL_B;
      }
    }

    private static TveAnalogVideoStandard GetTveVideoStandards(WdmAnalogVideoStandard videoStandards)
    {
      TveAnalogVideoStandard tveAvs = TveAnalogVideoStandard.None;
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.NTSC_M))
      {
        tveAvs |= TveAnalogVideoStandard.NtscM;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.NTSC_M_J))
      {
        tveAvs |= TveAnalogVideoStandard.NtscMj;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.NTSC_433))
      {
        tveAvs |= TveAnalogVideoStandard.Ntsc433;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_B))
      {
        tveAvs |= TveAnalogVideoStandard.PalB;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_D))
      {
        tveAvs |= TveAnalogVideoStandard.PalD;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_G))
      {
        tveAvs |= TveAnalogVideoStandard.PalG;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_H))
      {
        tveAvs |= TveAnalogVideoStandard.PalH;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_I))
      {
        tveAvs |= TveAnalogVideoStandard.PalI;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_M))
      {
        tveAvs |= TveAnalogVideoStandard.PalM;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_N))
      {
        tveAvs |= TveAnalogVideoStandard.PalN;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_60))
      {
        tveAvs |= TveAnalogVideoStandard.Pal60;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_B))
      {
        tveAvs |= TveAnalogVideoStandard.SecamB;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_D))
      {
        tveAvs |= TveAnalogVideoStandard.SecamD;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_G))
      {
        tveAvs |= TveAnalogVideoStandard.SecamG;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_H))
      {
        tveAvs |= TveAnalogVideoStandard.SecamH;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_K))
      {
        tveAvs |= TveAnalogVideoStandard.SecamK;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_K1))
      {
        tveAvs |= TveAnalogVideoStandard.SecamK1;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_L))
      {
        tveAvs |= TveAnalogVideoStandard.SecamL;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.SECAM_L1))
      {
        tveAvs |= TveAnalogVideoStandard.SecamL1;
      }
      if (videoStandards.HasFlag(WdmAnalogVideoStandard.PAL_N_COMBO))
      {
        tveAvs |= TveAnalogVideoStandard.PalNCombo;
      }
      return tveAvs;
    }

    private static void GetWdmVideoProcAmpProperty(VideoOrCameraProperty property, out bool isVideoProcAmpProperty, out VideoProcAmpProperty videoProcAmpProperty)
    {
      isVideoProcAmpProperty = true;
      videoProcAmpProperty = VideoProcAmpProperty.BacklightCompensation;
      switch (property)
      {
        case VideoOrCameraProperty.Brightness:
          videoProcAmpProperty = VideoProcAmpProperty.Brightness;
          return;
        case VideoOrCameraProperty.Contrast:
          videoProcAmpProperty = VideoProcAmpProperty.Contrast;
          return;
        case VideoOrCameraProperty.Hue:
          videoProcAmpProperty = VideoProcAmpProperty.Hue;
          return;
        case VideoOrCameraProperty.Saturation:
          videoProcAmpProperty = VideoProcAmpProperty.Saturation;
          return;
        case VideoOrCameraProperty.Sharpness:
          videoProcAmpProperty = VideoProcAmpProperty.Sharpness;
          return;
        case VideoOrCameraProperty.Gamma:
          videoProcAmpProperty = VideoProcAmpProperty.Gamma;
          return;
        case VideoOrCameraProperty.ColorEnable:
          videoProcAmpProperty = VideoProcAmpProperty.ColorEnable;
          return;
        case VideoOrCameraProperty.WhiteBalance:
          videoProcAmpProperty = VideoProcAmpProperty.WhiteBalance;
          return;
        case VideoOrCameraProperty.BacklightCompensation:
          videoProcAmpProperty = VideoProcAmpProperty.BacklightCompensation;
          return;
        case VideoOrCameraProperty.Gain:
          videoProcAmpProperty = VideoProcAmpProperty.Gain;
          return;
        case VideoOrCameraProperty.DigitalMultiplier:
          videoProcAmpProperty = VideoProcAmpProperty.DigitalMultiplier;
          return;
        case VideoOrCameraProperty.DigitalMultiplierLimit:
          videoProcAmpProperty = VideoProcAmpProperty.DigitalMultiplierLimit;
          return;
        case VideoOrCameraProperty.WhiteBalanceComponent:
          videoProcAmpProperty = VideoProcAmpProperty.WhiteBalanceComponent;
          return;
        case VideoOrCameraProperty.PowerLineFrequency:
          videoProcAmpProperty = VideoProcAmpProperty.PowerLineFrequency;
          return;
        default:
          isVideoProcAmpProperty = false;
          return;
      }
    }

    private static VideoOrCameraProperty? GetTveVideoOrCameraProperty(VideoProcAmpProperty property)
    {
      switch (property)
      {
        case VideoProcAmpProperty.Brightness:
          return VideoOrCameraProperty.Brightness;
        case VideoProcAmpProperty.Contrast:
          return VideoOrCameraProperty.Contrast;
        case VideoProcAmpProperty.Hue:
          return VideoOrCameraProperty.Hue;
        case VideoProcAmpProperty.Saturation:
          return VideoOrCameraProperty.Saturation;
        case VideoProcAmpProperty.Sharpness:
          return VideoOrCameraProperty.Sharpness;
        case VideoProcAmpProperty.Gamma:
          return VideoOrCameraProperty.Gamma;
        case VideoProcAmpProperty.ColorEnable:
          return VideoOrCameraProperty.ColorEnable;
        case VideoProcAmpProperty.WhiteBalance:
          return VideoOrCameraProperty.WhiteBalance;
        case VideoProcAmpProperty.BacklightCompensation:
          return VideoOrCameraProperty.BacklightCompensation;
        case VideoProcAmpProperty.Gain:
          return VideoOrCameraProperty.Gain;
        case VideoProcAmpProperty.DigitalMultiplier:
          return VideoOrCameraProperty.DigitalMultiplier;
        case VideoProcAmpProperty.DigitalMultiplierLimit:
          return VideoOrCameraProperty.DigitalMultiplierLimit;
        case VideoProcAmpProperty.WhiteBalanceComponent:
          return VideoOrCameraProperty.WhiteBalanceComponent;
        case VideoProcAmpProperty.PowerLineFrequency:
          return VideoOrCameraProperty.PowerLineFrequency;
      }
      return null;
    }

    private static VideoProcAmpFlags GetWdmVideoProcAmpPropertyFlag(VideoOrCameraPropertyFlag flag)
    {
      switch (flag)
      {
        case VideoOrCameraPropertyFlag.None:
          return VideoProcAmpFlags.None;
        case VideoOrCameraPropertyFlag.Manual:
          return VideoProcAmpFlags.Manual;
        case VideoOrCameraPropertyFlag.Auto:
          return VideoProcAmpFlags.Auto;
        default:
        Log.Warn("WDM analog capture: unsupported property flag {0}, falling back to none", flag);
        return VideoProcAmpFlags.None;
      }
    }

    private static VideoOrCameraPropertyFlag GetTveVideoOrCameraPropertyFlags(VideoProcAmpFlags flags)
    {
      VideoOrCameraPropertyFlag tveFlags = VideoOrCameraPropertyFlag.None;
      if (flags.HasFlag(VideoProcAmpFlags.Auto))
      {
        tveFlags |= VideoOrCameraPropertyFlag.Auto;
      }
      if (flags.HasFlag(VideoProcAmpFlags.Manual))
      {
        tveFlags |= VideoOrCameraPropertyFlag.Manual;
      }
      return tveFlags;
    }

    private static void GetWdmCameraControlProperty(VideoOrCameraProperty property, out bool isCameraControlProperty, out CameraControlProperty cameraControlProperty)
    {
      isCameraControlProperty = true;
      cameraControlProperty = CameraControlProperty.AutoExposurePriority;
      switch (property)
      {
        case VideoOrCameraProperty.Pan:
          cameraControlProperty = CameraControlProperty.Pan;
          return;
        case VideoOrCameraProperty.Tilt:
          cameraControlProperty = CameraControlProperty.Tilt;
          return;
        case VideoOrCameraProperty.Roll:
          cameraControlProperty = CameraControlProperty.Roll;
          return;
        case VideoOrCameraProperty.Zoom:
          cameraControlProperty = CameraControlProperty.Zoom;
          return;
        case VideoOrCameraProperty.Exposure:
          cameraControlProperty = CameraControlProperty.Exposure;
          return;
        case VideoOrCameraProperty.Iris:
          cameraControlProperty = CameraControlProperty.Iris;
          return;
        case VideoOrCameraProperty.Focus:
          cameraControlProperty = CameraControlProperty.Focus;
          return;
        case VideoOrCameraProperty.ScanMode:
          cameraControlProperty = CameraControlProperty.ScanMode;
          return;
        case VideoOrCameraProperty.Privacy:
          cameraControlProperty = CameraControlProperty.Privacy;
          return;
        case VideoOrCameraProperty.PanTilt:
          cameraControlProperty = CameraControlProperty.PanTilt;
          return;
        case VideoOrCameraProperty.PanRelative:
          cameraControlProperty = CameraControlProperty.PanRelative;
          return;
        case VideoOrCameraProperty.TiltRelative:
          cameraControlProperty = CameraControlProperty.TiltRelative;
          return;
        case VideoOrCameraProperty.RollRelative:
          cameraControlProperty = CameraControlProperty.RollRelative;
          return;
        case VideoOrCameraProperty.ZoomRelative:
          cameraControlProperty = CameraControlProperty.ZoomRelative;
          return;
        case VideoOrCameraProperty.ExposureRelative:
          cameraControlProperty = CameraControlProperty.ExposureRelative;
          return;
        case VideoOrCameraProperty.IrisRelative:
          cameraControlProperty = CameraControlProperty.IrisRelative;
          return;
        case VideoOrCameraProperty.FocusRelative:
          cameraControlProperty = CameraControlProperty.FocusRelative;
          return;
        case VideoOrCameraProperty.PanTiltRelative:
          cameraControlProperty = CameraControlProperty.PanTiltRelative;
          return;
        case VideoOrCameraProperty.AutoExposurePriority:
          cameraControlProperty = CameraControlProperty.AutoExposurePriority;
          return;
        default:
          isCameraControlProperty = false;
          return;
      }
    }

    private static VideoOrCameraProperty? GetMpVideoOrCameraProperty(CameraControlProperty property)
    {
      switch (property)
      {
        case CameraControlProperty.Pan:
          return VideoOrCameraProperty.Pan;
        case CameraControlProperty.Tilt:
          return VideoOrCameraProperty.Tilt;
        case CameraControlProperty.Roll:
          return VideoOrCameraProperty.Roll;
        case CameraControlProperty.Zoom:
          return VideoOrCameraProperty.Zoom;
        case CameraControlProperty.Exposure:
          return VideoOrCameraProperty.Exposure;
        case CameraControlProperty.Iris:
          return VideoOrCameraProperty.Iris;
        case CameraControlProperty.Focus:
          return VideoOrCameraProperty.Focus;
        case CameraControlProperty.ScanMode:
          return VideoOrCameraProperty.ScanMode;
        case CameraControlProperty.Privacy:
          return VideoOrCameraProperty.Privacy;
        case CameraControlProperty.PanTilt:
          return VideoOrCameraProperty.PanTilt;
        case CameraControlProperty.PanRelative:
          return VideoOrCameraProperty.PanRelative;
        case CameraControlProperty.TiltRelative:
          return VideoOrCameraProperty.TiltRelative;
        case CameraControlProperty.RollRelative:
          return VideoOrCameraProperty.RollRelative;
        case CameraControlProperty.ZoomRelative:
          return VideoOrCameraProperty.ZoomRelative;
        case CameraControlProperty.ExposureRelative:
          return VideoOrCameraProperty.ExposureRelative;
        case CameraControlProperty.IrisRelative:
          return VideoOrCameraProperty.IrisRelative;
        case CameraControlProperty.FocusRelative:
          return VideoOrCameraProperty.FocusRelative;
        case CameraControlProperty.PanTiltRelative:
          return VideoOrCameraProperty.PanTiltRelative;
        case CameraControlProperty.FocalLength:
          return null;    // not supported - read only, unusual structure
        case CameraControlProperty.AutoExposurePriority:
          return VideoOrCameraProperty.AutoExposurePriority;
      }
      return null;
    }

    private static CameraControlFlags GetWdmCameraControlPropertyFlag(VideoOrCameraPropertyFlag flag)
    {
      switch (flag)
      {
        case VideoOrCameraPropertyFlag.None:
          return CameraControlFlags.None;
        case VideoOrCameraPropertyFlag.Manual:
          return CameraControlFlags.Manual;
        case VideoOrCameraPropertyFlag.Auto:
          return CameraControlFlags.Auto;
        default:
        Log.Warn("WDM analog capture: unsupported property flag {0}, falling back to none", flag);
        return CameraControlFlags.None;
      }
    }

    private static VideoOrCameraPropertyFlag GetTveVideoOrCameraPropertyFlags(CameraControlFlags flags)
    {
      VideoOrCameraPropertyFlag tveFlags = VideoOrCameraPropertyFlag.None;
      if (flags.HasFlag(CameraControlFlags.Auto))
      {
        tveFlags |= VideoOrCameraPropertyFlag.Auto;
      }
      if (flags.HasFlag(CameraControlFlags.Manual))
      {
        tveFlags |= VideoOrCameraPropertyFlag.Manual;
      }
      return tveFlags;
    }

    private static int GetTveFrameWidth(FrameSize frameSize)
    {
      switch (frameSize)
      {
        case FrameSize.Fs320_240:
          return 320;
        case FrameSize.Fs352_240:
        case FrameSize.Fs352_288:
          return 352;
        case FrameSize.Fs384_288:
          return 384;
        case FrameSize.Fs480_360:
          return 480;
        case FrameSize.Fs640_360:
        case FrameSize.Fs640_480:
          return 640;
        case FrameSize.Fs704_480:
        case FrameSize.Fs704_576:
          return 704;
        case FrameSize.Fs720_480:
        case FrameSize.Fs720_576:
          return 720;
        case FrameSize.Fs768_480:
        case FrameSize.Fs768_576:
          return 768;
        case FrameSize.Fs1280_720:
          return 1280;
        case FrameSize.Fs1440_1080:
          return 1440;
        case FrameSize.Fs1920_1080:
          return 1920;
      }
      return -1;
    }

    private static int GetTveFrameHeight(FrameSize frameSize)
    {
      switch (frameSize)
      {
        case FrameSize.Fs320_240:
        case FrameSize.Fs352_240:
          return 240;
        case FrameSize.Fs352_288:
        case FrameSize.Fs384_288:
          return 288;
        case FrameSize.Fs480_360:
        case FrameSize.Fs640_360:
          return 360;
        case FrameSize.Fs640_480:
        case FrameSize.Fs704_480:
        case FrameSize.Fs720_480:
        case FrameSize.Fs768_480:
          return 480;
        case FrameSize.Fs704_576:
        case FrameSize.Fs720_576:
        case FrameSize.Fs768_576:
          return 576;
        case FrameSize.Fs1280_720:
          return 720;
        case FrameSize.Fs1440_1080:
        case FrameSize.Fs1920_1080:
          return 1080;
      }
      return -1;
    }

    private static void GetTveFrameDetails(AMMediaType format, out FrameSize frameSize, out FrameRate frameRate)
    {
      frameSize = FrameSize.Automatic;
      frameRate = FrameRate.Automatic;

      int frameWidth;
      int frameHeight;
      long averageTimePerFrame;
      if (format.formatType == FormatType.VideoInfo)
      {
        VideoInfoHeader temp = new VideoInfoHeader();
        Marshal.PtrToStructure(format.formatPtr, temp);
        ReadBmiHeader(ref temp.BmiHeader, out frameWidth, out frameHeight);
        averageTimePerFrame = temp.AvgTimePerFrame;
      }
      else if (format.formatType == FormatType.VideoInfo2)
      {
        VideoInfoHeader2 temp = new VideoInfoHeader2();
        Marshal.PtrToStructure(format.formatPtr, temp);
        ReadBmiHeader(ref temp.BmiHeader, out frameWidth, out frameHeight);
        averageTimePerFrame = temp.AvgTimePerFrame;
      }
      else if (format.formatType == FormatType.Mpeg2Video)
      {
        MPEG2VideoInfo temp = new MPEG2VideoInfo();
        Marshal.PtrToStructure(format.formatPtr, temp);
        ReadBmiHeader(ref temp.hdr.BmiHeader, out frameWidth, out frameHeight);
        averageTimePerFrame = temp.hdr.AvgTimePerFrame;
      }
      else if (format.formatType == FormatType.MpegVideo)
      {
        MPEG1VideoInfo temp = new MPEG1VideoInfo();
        Marshal.PtrToStructure(format.formatPtr, temp);
        ReadBmiHeader(ref temp.hdr.BmiHeader, out frameWidth, out frameHeight);
        averageTimePerFrame = temp.hdr.AvgTimePerFrame;
      }
      else
      {
        Log.Warn("WDM analog capture: format type {0} is not supported", format.formatType);
        return;
      }

      if (frameWidth < 0 || frameHeight < 0)
      {
        return;
      }

      if (frameWidth == 320 && frameHeight == 240)
      {
        frameSize = FrameSize.Fs320_240;
      }
      else if (frameWidth == 352 && frameHeight == 288)
      {
        frameSize = FrameSize.Fs352_288;
      }
      else if (frameWidth == 384 && frameHeight == 288)
      {
        frameSize = FrameSize.Fs384_288;
      }
      else if (frameWidth == 480 && frameHeight == 360)
      {
        frameSize = FrameSize.Fs480_360;
      }
      else if (frameWidth == 640 && frameHeight == 360)
      {
        frameSize = FrameSize.Fs640_360;
      }
      else if (frameWidth == 640 && frameHeight == 480)
      {
        frameSize = FrameSize.Fs640_480;
      }
      else if (frameWidth == 704 && frameHeight == 480)
      {
        frameSize = FrameSize.Fs704_480;
      }
      else if (frameWidth == 704 && frameHeight == 576)
      {
        frameSize = FrameSize.Fs704_576;
      }
      else if (frameWidth == 720 && frameHeight == 480)
      {
        frameSize = FrameSize.Fs720_480;
      }
      else if (frameWidth == 720 && frameHeight == 576)
      {
        frameSize = FrameSize.Fs720_576;
      }
      else if (frameWidth == 768 && frameHeight == 480)
      {
        frameSize = FrameSize.Fs768_480;
      }
      else if (frameWidth == 768 && frameHeight == 576)
      {
        frameSize = FrameSize.Fs768_576;
      }
      else if (frameWidth == 1280 && frameHeight == 720)
      {
        frameSize = FrameSize.Fs1280_720;
      }
      else if (frameWidth == 1440 && frameHeight == 1080)
      {
        frameSize = FrameSize.Fs1440_1080;
      }
      else if (frameWidth == 1920 && frameHeight == 1080)
      {
        frameSize = FrameSize.Fs1920_1080;
      }
      else
      {
        Log.Warn("WDM analog capture: unsupported frame size, width = {0} px, height = {1} px", frameWidth, frameHeight);
      }

      double rawFrameRate = 10000000d / averageTimePerFrame;
      if (rawFrameRate > 14.9 && rawFrameRate < 15.1)
      {
        frameRate = FrameRate.Fr15;
      }
      else if (rawFrameRate > 23.97 && rawFrameRate < 23.98)
      {
        frameRate = FrameRate.Fr23_976;
      }
      else if (rawFrameRate > 23.98 && rawFrameRate < 24.02)
      {
        frameRate = FrameRate.Fr24;
      }
      else if (rawFrameRate > 24.9 && rawFrameRate < 25.1)
      {
        frameRate = FrameRate.Fr25;
      }
      else if (rawFrameRate > 29.96 && rawFrameRate < 29.98)
      {
        frameRate = FrameRate.Fr29_97;
      }
      else if (rawFrameRate > 29.98 && rawFrameRate < 30.02)
      {
        frameRate = FrameRate.Fr30;
      }
      else if (rawFrameRate > 49 && rawFrameRate < 51)
      {
        frameRate = FrameRate.Fr50;
      }
      else if (rawFrameRate > 59.93 && rawFrameRate < 59.95)
      {
        frameRate = FrameRate.Fr59_94;
      }
      else if (rawFrameRate > 59.95 && rawFrameRate < 60.05)
      {
        frameRate = FrameRate.Fr60;
      }
      else
      {
        Log.Warn("WDM analog capture: unsupported frame rate, raw rate = {0} fps", rawFrameRate);
      }
    }

    #endregion
  }
}