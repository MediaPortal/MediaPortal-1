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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Countries;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

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

    private struct VideoProcAmpPropertyDetail
    {
      public int ValueMinimum;
      public int ValueMaximum;
      public int SteppingDelta;
      public int ValueDefault;
      public VideoProcAmpFlags Flags;

      internal VideoProcAmpPropertyDetail(int valueMinimum, int valueMaximum, int steppingDelta, int valueDefault, VideoProcAmpFlags flags)
      {
        ValueMinimum = valueMinimum;
        ValueMaximum = valueMaximum;
        SteppingDelta = steppingDelta;
        ValueDefault = valueDefault;
        Flags = flags;
      }
    }

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

    /// <summary>
    /// A set of flags indicating which video standards the capture source supports.
    /// </summary>
    private AnalogVideoStandard _supportedVideoStandards = AnalogVideoStandard.None;

    /// <summary>
    /// A map containing the supported video processing amplifier properties,
    /// their default values, and their limits.
    /// </summary>
    private Dictionary<VideoProcAmpProperty, VideoProcAmpPropertyDetail> _supportedVideoProcAmpProperties = new Dictionary<VideoProcAmpProperty, VideoProcAmpPropertyDetail>();

    #region configuration

    /// <summary>
    /// The configured video standard.
    /// </summary>
    private AnalogVideoStandard _currentVideoStandard = AnalogVideoStandard.PAL_B;

    /// <summary>
    /// The configured video processing amplifier property settings.
    /// </summary>
    private Dictionary<VideoProcAmpProperty, double> _currentVideoProcAmpPropertyValues = new Dictionary<VideoProcAmpProperty, double>();

    /// <summary>
    /// The configured frame width measured in pixels.
    /// </summary>
    private int _currentFrameWidth = 720;

    /// <summary>
    /// The configured frame height measured in pixels.
    /// </summary>
    private int _currentFrameHeight = 576;

    /// <summary>
    /// The configured frame rate measured in frames per second.
    /// </summary>
    private double _currentFrameRate = 25.0;

    #endregion

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
          throw new TvException("Failed to add filter for main capture component to graph.", ex);
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
      ConfigureAnalogVideoDecoder(_currentVideoStandard);
      ConfigureVideoProcessingAmplifier(_currentVideoProcAmpPropertyValues);
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
      HResult.ThrowException(hr, "Failed to obtain pin enumerator for filter.");
      try
      {
        int pinIndex = 0;
        int pinCount = 0;
        IPin[] pins = new IPin[2];
        while (pinEnum.Next(1, pins, out pinCount) == (int)HResult.Severity.Success && pinCount == 1)
        {
          IPin pin = pins[0];
          try
          {
            IEnumMediaTypes mediaTypeEnum;
            hr = pin.EnumMediaTypes(out mediaTypeEnum);
            HResult.ThrowException(hr, "Failed to obtain media type enumerator for pin.");
            try
            {
              // For each pin media type...
              int mediaTypeCount;
              AMMediaType[] mediaTypes = new AMMediaType[2];
              while (mediaTypeEnum.Next(1, mediaTypes, out mediaTypeCount) == (int)HResult.Severity.Success && mediaTypeCount == 1)
              {
                AMMediaType mediaType = mediaTypes[0];
                try
                {
                  if (mediaType.majorType == MediaType.AnalogVideo || mediaType.majorType == MediaType.Video)
                  {
                    this.LogDebug("WDM analog capture: pin {0} is a video pin", pinIndex);
                    isVideoSource = true;
                  }
                  else if (mediaType.majorType == MediaType.AnalogAudio || mediaType.majorType == MediaType.Audio)
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
        ConfigureStream(_currentFrameWidth, _currentFrameHeight, _currentFrameRate);
      }
      else if (_filterVideo != null)
      {
        this.LogWarn("WDM analog capture: failed to find stream configuration interface");
      }
    }

    #region check capabilities

    /// <summary>
    /// Check the capabilites of the analog video decoder interface.
    /// </summary>
    private void CheckCapabilitiesAnalogVideoDecoder()
    {
      if (_filterVideo != null)
      {
        IAMAnalogVideoDecoder analogVideoDecoder = _filterVideo as IAMAnalogVideoDecoder;
        if (analogVideoDecoder != null)
        {
          int hr = analogVideoDecoder.get_AvailableTVFormats(out _supportedVideoStandards);
          if (hr != (int)HResult.Severity.Success)
          {
            _supportedVideoStandards = AnalogVideoStandard.None;
            this.LogWarn("WDM analog capture: failed to get supported video standards, hr = 0x{0:x}", hr);
          }
          else
          {
            this.LogDebug("WDM analog capture: supported video standards = {0}", _supportedVideoStandards);
          }
        }
        else
        {
          this.LogWarn("WDM analog capture: failed to find analog video decoder interface on capture filter, not able to check video decoder capabilities");
        }
      }
    }

    /// <summary>
    /// Check the capabilites of the video processing amplifier interface.
    /// </summary>
    private void CheckCapabilitiesVideoProcessingAmplifier()
    {
      if (_filterVideo != null)
      {
        IAMVideoProcAmp videoProcAmp = _filterVideo as IAMVideoProcAmp;
        if (videoProcAmp != null)
        {
          int valueMinimum = 0;
          int valueMaximum = 0;
          int steppingDelta = 1;
          int valueDefault = 0;
          VideoProcAmpFlags flags = VideoProcAmpFlags.None;
          int hr = (int)HResult.Severity.Success;
          foreach (VideoProcAmpProperty property in Enum.GetValues(typeof(VideoProcAmpProperty)))
          {
            hr = videoProcAmp.GetRange(property, out valueMinimum, out valueMaximum, out steppingDelta, out valueDefault, out flags);
            if (hr == (int)HResult.Severity.Success)
            {
              this.LogDebug("WDM analog capture: processing amplifier property {0} is supported, min = {1}, max = {2}, step = {3}, default = {4}, flags = {5}", property, valueMinimum, valueMaximum, steppingDelta, valueDefault, flags);
              VideoProcAmpPropertyDetail propertyLimits = new VideoProcAmpPropertyDetail(valueMinimum, valueMaximum, steppingDelta, valueDefault, flags);
              _supportedVideoProcAmpProperties.Add(property, propertyLimits);
            }
            else
            {
              this.LogDebug("WDM analog capture: processing amplifier property {0} is not supported", property);
            }
          }
        }
        else
        {
          this.LogWarn("WDM analog capture: failed to find video processing amplifier interface on capture filter, not able to check video processing amplifier capabilities");
        }
      }
    }

    #endregion

    #region configure

    /// <summary>
    /// Reload the component's configuration.
    /// </summary>
    /// <param name="tunerId">The identifier for the associated tuner.</param>
    public void ReloadConfiguration(int tunerId)
    {
      this.LogDebug("WDM analog capture: reload configuration");

      // Do we have existing settings? If not, try to set sensible defaults.
      int settingCheck = SettingsManagement.GetValue("tuner" + tunerId + "VideoStandard", -1);
      if (settingCheck == -1)   // first load
      {
        this.LogDebug("WDM analog capture: first load, setting defaults");
        CountryCollection collection = new CountryCollection();
        string countryName = System.Globalization.RegionInfo.CurrentRegion.EnglishName;
        Country country = collection.GetTunerCountry(countryName);
        if (country == null)
        {
          this.LogWarn("WDM analog capture: failed to get country details for country {0}, using PAL/SECAM defaults", countryName ?? "[null]");
          _currentVideoStandard = AnalogVideoStandard.PAL_B;
        }
        else
        {
          this.LogDebug("WDM analog capture: recognised country {0}, using {1} defaults", countryName, country.VideoStandard);
          _currentVideoStandard = country.VideoStandard;
        }
        SettingsManagement.SaveValue("tuner" + tunerId + "VideoStandard", (int)_currentVideoStandard);

        _currentFrameWidth = 720;
        if (_currentVideoStandard == AnalogVideoStandard.NTSC_M || _currentVideoStandard == AnalogVideoStandard.NTSC_M_J || _currentVideoStandard == AnalogVideoStandard.NTSC_433 || _currentVideoStandard == AnalogVideoStandard.PAL_M)
        {
          _currentFrameHeight = 480;
          _currentFrameRate = 29.97;
        }
        else
        {
          _currentFrameHeight = 576;
          _currentFrameRate = 25;
        }
        SettingsManagement.SaveValue("tuner" + tunerId + "FrameWidth", _currentFrameWidth);
        SettingsManagement.SaveValue("tuner" + tunerId + "FrameHeight", _currentFrameHeight);
        SettingsManagement.SaveValue("tuner" + tunerId + "FrameRate", _currentFrameRate);

        SettingsManagement.SaveValue("tuner" + tunerId + "SupportedVideoStandards", (int)_supportedVideoStandards);

        foreach (KeyValuePair<VideoProcAmpProperty, VideoProcAmpPropertyDetail> p in _supportedVideoProcAmpProperties)
        {
          // The value is stored as a percentage.
          double propertyValuePercentage = (p.Value.ValueDefault - p.Value.ValueMinimum) * 100 / (p.Value.ValueMaximum - p.Value.ValueMinimum);
          SettingsManagement.SaveValue("tuner" + tunerId + "VideoProcAmpProperty" + p.Key + "Value", propertyValuePercentage);
          SettingsManagement.SaveValue("tuner" + tunerId + "VideoProcAmpProperty" + p.Key + "DefaultValue", propertyValuePercentage);
        }
      }

      AnalogVideoStandard newVideoStandard = (AnalogVideoStandard)SettingsManagement.GetValue("tuner" + tunerId + "VideoStandard", (int)_currentVideoStandard);
      if (newVideoStandard != _currentVideoStandard)
      {
        ConfigureAnalogVideoDecoder(newVideoStandard);
      }

      Dictionary<VideoProcAmpProperty, double> newVideoProcAmpSettings = new Dictionary<VideoProcAmpProperty, double>();
      foreach (VideoProcAmpProperty property in Enum.GetValues(typeof(VideoProcAmpProperty)))
      {
        double currentPropertyValuePercentage = -1;
        if (!_currentVideoProcAmpPropertyValues.TryGetValue(property, out currentPropertyValuePercentage))
        {
          currentPropertyValuePercentage = -1;
        }

        double newPropertyValuePercentage = SettingsManagement.GetValue("tuner" + tunerId + "VideoProcAmpProperty" + property + "Value", currentPropertyValuePercentage);
        if (newPropertyValuePercentage != -1 && newPropertyValuePercentage != currentPropertyValuePercentage)
        {
          newVideoProcAmpSettings.Add(property, newPropertyValuePercentage);
        }
      }
      ConfigureVideoProcessingAmplifier(newVideoProcAmpSettings);

      int newFrameWidth = SettingsManagement.GetValue("tuner" + tunerId + "FrameWidth", _currentFrameWidth);
      int newFrameHeight = SettingsManagement.GetValue("tuner" + tunerId + "FrameHeight", _currentFrameHeight);
      double newFrameRate = SettingsManagement.GetValue("tuner" + tunerId + "FrameRate", _currentFrameRate);
      if (newFrameWidth != _currentFrameWidth || newFrameHeight != _currentFrameHeight || newFrameRate != _currentFrameRate)
      {
        ConfigureStream(newFrameWidth, newFrameHeight, newFrameRate);
      }
    }

    /// <summary>
    /// Configure the analog video decoder interface.
    /// </summary>
    /// <param name="videoStandard">The decoder video standard.</param>
    private void ConfigureAnalogVideoDecoder(AnalogVideoStandard videoStandard)
    {
      if (_filterVideo == null || videoStandard == AnalogVideoStandard.None)
      {
        return;
      }

      this.LogDebug("WDM analog capture: configure analog video decoder, standard = {0}", videoStandard);
      IAMAnalogVideoDecoder analogVideoDecoder = _filterVideo as IAMAnalogVideoDecoder;
      if (analogVideoDecoder != null)
      {
        if (!_supportedVideoStandards.HasFlag(videoStandard))
        {
          this.LogWarn("WDM analog capture: requested video standard {0} is not supported", videoStandard);
          return;
        }

        int hr = analogVideoDecoder.put_TVFormat(videoStandard);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogError("WDM analog capture: failed to set video standard, hr = 0x{0:x}", hr);
        }
        else
        {
          _currentVideoStandard = videoStandard;
        }
      }
      else
      {
        this.LogWarn("WDM analog capture: failed to find analog video decoder interface on capture filter, not able to configure decoder");
      }
    }

    /// <summary>
    /// Configure the video processing amplifier.
    /// </summary>
    /// <param name="propertySettings">The amplifier property settings.</param>
    private void ConfigureVideoProcessingAmplifier(IDictionary<VideoProcAmpProperty, double> propertySettings)
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

      foreach (VideoProcAmpProperty property in propertySettings.Keys)
      {
        VideoProcAmpPropertyDetail propertyLimits;
        if (_supportedVideoProcAmpProperties.TryGetValue(property, out propertyLimits))
        {
          // The value is stored as a percentage. Convert it back to an
          // absolute value in the permitted range and quantise to the step
          // size for the property.
          double propertyValuePercentage = propertySettings[property];
          int propertyValueAbsolute = (int)Math.Round(propertyLimits.ValueMinimum + ((propertyValuePercentage * (propertyLimits.ValueMaximum - propertyLimits.ValueMinimum)) / 100), 0, MidpointRounding.AwayFromZero);

          // If the percentage doesn't work out to an exact permitted value
          // (ie. it is between steps), round to the closest step.
          int stepOffset = propertyValueAbsolute % propertyLimits.SteppingDelta;
          if (stepOffset > 0)
          {
            propertyValueAbsolute -= stepOffset;
            if (Math.Round((double)(stepOffset / propertyLimits.SteppingDelta), 0, MidpointRounding.AwayFromZero) == 1)
            {
              propertyValueAbsolute += propertyLimits.SteppingDelta;
            }
          }

          this.LogDebug("WDM analog capture: configure video processing amplifier property {0}, percentage = {1} %, value = {2}", property, propertyValuePercentage, propertyValueAbsolute);
          if (propertyLimits.ValueMinimum <= propertyValueAbsolute && propertyLimits.ValueMaximum >= propertyValueAbsolute)
          {
            int hr = videoProcAmp.Set(property, propertyValueAbsolute, VideoProcAmpFlags.Manual);
            if (hr != (int)HResult.Severity.Success)
            {
              this.LogError("WDM analog capture: failed to set video standard, hr = 0x{0:x}", hr);
            }
            else
            {
              _currentVideoProcAmpPropertyValues[property] = propertyValuePercentage;
            }
          }
          else
          {
            this.LogWarn("WDM analog capture: value {0} calculated from percentage {1} for processing amplifier property {2} is outside the allowed bounds, min = {2}, max = {3}", propertyValueAbsolute, propertyValuePercentage, property, propertyLimits.ValueMinimum, propertyLimits.ValueMaximum);
          }
        }
        else
        {
          this.LogDebug("WDM analog capture: requested processing amplifier property {0} is not supported", property);
        }
      }
    }

    /// <summary>
    /// Configure the stream configuration interface.
    /// </summary>
    /// <param name="frameWidth">The video frame width in pixels.</param>
    /// <param name="frameHeight">The video frame height in pixels.</param>
    /// <param name="frameRate">The video frame rate.</param>
    private void ConfigureStream(int frameWidth, int frameHeight, double frameRate)
    {
      if (_interfaceStreamConfiguration == null)
      {
        return;
      }

      // Get the current format information.
      AMMediaType format;
      int hr = _interfaceStreamConfiguration.GetFormat(out format);
      try
      {
        HResult.ThrowException(hr, "Failed to get format information.");

        this.LogDebug("WDM analog capture: configure stream, width = {0}, height = {1}, rate = {2}, format = {3}", frameWidth, frameHeight, frameRate, format.formatType);
        long averageTimePerFrame = (long)(10000000d / frameRate);

        // The structure of the content of formatPtr depends on formatType.
        object formatStruct = null;
        if (format.formatType == FormatType.VideoInfo)
        {
          VideoInfoHeader temp = new VideoInfoHeader();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.BmiHeader, frameWidth, frameHeight);
          temp.AvgTimePerFrame = averageTimePerFrame;
          formatStruct = temp;
        }
        else if (format.formatType == FormatType.VideoInfo2)
        {
          VideoInfoHeader2 temp = new VideoInfoHeader2();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.BmiHeader, frameWidth, frameHeight);
          temp.AvgTimePerFrame = averageTimePerFrame;
          formatStruct = temp;
        }
        else if (format.formatType == FormatType.Mpeg2Video)
        {
          MPEG2VideoInfo temp = new MPEG2VideoInfo();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.hdr.BmiHeader, frameWidth, frameHeight);
          temp.hdr.AvgTimePerFrame = averageTimePerFrame;
          formatStruct = temp;
        }
        else if (format.formatType == FormatType.MpegVideo)
        {
          MPEG1VideoInfo temp = new MPEG1VideoInfo();
          Marshal.PtrToStructure(format.formatPtr, temp);
          UpdateBmiHeader(ref temp.hdr.BmiHeader, frameWidth, frameHeight);
          temp.hdr.AvgTimePerFrame = averageTimePerFrame;
          formatStruct = temp;
        }
        else
        {
          this.LogWarn("WDM analog capture: format type {0} is not supported, not possible to configure stream", format.formatType);
        }

        if (formatStruct != null)
        {
          Marshal.StructureToPtr(formatStruct, format.formatPtr, false);
          hr = _interfaceStreamConfiguration.SetFormat(format);
          HResult.ThrowException(hr, "Failed to set format information.");
          _currentFrameWidth = frameWidth;
          _currentFrameHeight = frameHeight;
          _currentFrameRate = frameRate;
        }
      }
      catch (Exception ex)
      {
        this.LogError(ex, "WDM analog capture: failed to configure stream");
      }
      finally
      {
        Release.AmMediaType(ref format);
      }
    }

    private void UpdateBmiHeader(ref BitmapInfoHeader bmiHeader, int width, int height)
    {
      if (bmiHeader == null || bmiHeader.Size < 8)
      {
        this.LogWarn("WDM analog capture: not possible to set frame size");
      }
      else
      {
        bmiHeader.Width = width;
        bmiHeader.Height = height;
      }
    }

    #endregion

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public void PerformTuning(AnalogChannel channel)
    {
      if (channel.MediaType == MediaTypeEnum.TV && _filterVideo != null)
      {
        this.LogDebug("WDM analog capture: perform tuning");
        IAMAnalogVideoDecoder analogVideoDecoder = _filterVideo as IAMAnalogVideoDecoder;
        if (analogVideoDecoder != null)
        {
          // This property is not always supported, so don't throw an exception on failure.
          int hr = analogVideoDecoder.put_VCRHorizontalLocking(channel.IsVcrSignal);
          if (hr != (int)HResult.Severity.Success)
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

      _supportedVideoStandards = AnalogVideoStandard.None;
      _supportedVideoProcAmpProperties.Clear();

      // The stream interface is found on an output pin, so we must release
      // the reference to avoid a leak.
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

      if (_deviceMain != null)
      {
        DevicesInUse.Instance.Remove(_deviceMain);
        // Do NOT Dispose() or set the capture device to NULL. We would be
        // unable to reload. The tuner instance that instanciated this capture
        // is responsible for disposing it.
      }
      else
      {
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
      }
    }
  }
}