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
using System.Globalization;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using AnalogVideoStandard = Mediaportal.TV.Server.Common.Types.Enum.AnalogVideoStandard;
using Regex = System.Text.RegularExpressions.Regex;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for analog tuners and capture
  /// devices with WDM/DirectShow drivers.
  /// </summary>
  internal class TunerAnalog : TunerDirectShowMpeg2TsBase
  {
    #region constants

    private static readonly Regex HAUPPAUGE_HVR22XX_DEVICE_ID = new Regex("pci#ven_1131&dev_7164&subsys_[0-9a-f]{4}0070");

    #endregion

    #region variables

    private Tuner _tuner = null;
    private Crossbar _crossbar = null;
    private Capture _capture = null;
    private Encoder _encoder = null;

    private CaptureSourceVideo _tunableSourcesVideo;
    private CaptureSourceAudio _tunableSourcesAudio;
    private ExternalTuner _externalTuner = new ExternalTuner();

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerAnalog"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="category">The device/filter category of <paramref name="device"/>.</param>
    /// <param name="tunerInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single tuner.</param>
    /// <param name="productInstanceId">The identifier shared by all <see cref="ITuner"/> instances derived from a single product.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the tuner.</param>
    public TunerAnalog(DsDevice device, Guid category, string tunerInstanceId, string productInstanceId, BroadcastStandard supportedBroadcastStandards)
      : base(device.Name, device.DevicePath, tunerInstanceId, productInstanceId, supportedBroadcastStandards)
    {
      _deviceMain = device;

      if (category == FilterCategory.AMKSCrossbar)
      {
        if (supportedBroadcastStandards != BroadcastStandard.ExternalInput)
        {
          _tuner = new Tuner();
        }
        _crossbar = new Crossbar(device);
        _capture = new Capture();
      }
      else
      {
        _capture = new Capture(device);
      }

      // Hauppauge currently have a bug in the driver for their HVR-22**
      // products. The bug causes a BSOD when the capture filter's VBI output
      // pin is connected and the tuner transitions from being locked onto
      // signal to unlocked. Due to the nature of the pre-conditions, the bug
      // mainly affects scanning:
      // http://forum.team-mediaportal.com/threads/hvr-2250-scanning-channels-causes-crash.102858/
      // http://forum.team-mediaportal.com/threads/bsod-when-scanning-channels-analog-cable-hvr-2255-f111.132566/
      //
      // However it can potentially hit at any time. We work-around the bug by
      // not connecting the capture filter VBI output pin for HVR-22** tuners.
      _encoder = new Encoder(!HAUPPAUGE_HVR22XX_DEVICE_ID.Match(device.DevicePath.ToLowerInvariant()).Success);
    }

    #endregion

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    /// <param name="configuration">The tuner's configuration.</param>
    public override void ReloadConfiguration(TVDatabase.Entities.Tuner configuration)
    {
      base.ReloadConfiguration(configuration);

      this.LogDebug("WDM analog: reload configuration");

      if (configuration.AnalogTunerSettings == null)
      {
        TVDatabase.Entities.AnalogTunerSettings settings;
        IEnumerable<TVDatabase.Entities.TunerProperty> properties;
        CreateDefaultConfiguration(out settings, out properties);
        configuration.AnalogTunerSettings = settings;
        if (properties != null)
        {
          foreach (var p in properties)
          {
            configuration.TunerProperties.Add(p);
          }
        }
      }

      _tunableSourcesVideo = (CaptureSourceVideo)configuration.AnalogTunerSettings.SupportedVideoSources;
      _tunableSourcesVideo |= CaptureSourceVideo.TunerDefault;
      _tunableSourcesAudio = (CaptureSourceAudio)configuration.AnalogTunerSettings.SupportedAudioSources;
      _tunableSourcesAudio |= CaptureSourceAudio.Automatic | CaptureSourceAudio.TunerDefault;

      if (_capture != null)
      {
        _capture.ReloadConfiguration(configuration);
      }
      if (_encoder != null)
      {
        _encoder.ReloadConfiguration(configuration);
      }
      _externalTuner.ReloadConfiguration(configuration.AnalogTunerSettings);
    }

    /// <summary>
    /// Create sensible default configuration based on country and hardware capabilities.
    /// </summary>
    private void CreateDefaultConfiguration(out TVDatabase.Entities.AnalogTunerSettings settings, out IEnumerable<TVDatabase.Entities.TunerProperty> properties)
    {
      this.LogDebug("WDM analog: first detection, create default configuration");
      settings = new TVDatabase.Entities.AnalogTunerSettings();
      properties = null;

      settings.IdAnalogTunerSettings = TunerId;
      settings.IdVideoEncoder = null;
      settings.IdAudioEncoder = null;
      settings.EncoderBitRateModeTimeShifting = (int)EncodeMode.ConstantBitRate;
      settings.EncoderBitRateTimeShifting = 100;
      settings.EncoderBitRatePeakTimeShifting = 100;
      settings.EncoderBitRateModeRecording = (int)EncodeMode.ConstantBitRate;
      settings.EncoderBitRateRecording = 100;
      settings.EncoderBitRatePeakRecording = 100;

      settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.None;
      settings.SupportedAudioSources = (int)CaptureSourceAudio.None;
      settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.None;
      settings.SupportedVideoSources = (int)CaptureSourceVideo.None;

      settings.ExternalTunerProgram = string.Empty;
      settings.ExternalTunerProgramArguments = string.Empty;
      settings.ExternalInputPhysicalChannelNumber = 7;
      string countryName = RegionInfo.CurrentRegion.EnglishName;
      Country country = CountryCollection.Instance.GetCountryByName(countryName);
      if (country == null)
      {
        settings.ExternalInputCountryId = CountryCollection.Instance.GetCountryByIsoCode("US").Id;
      }
      else
      {
        settings.ExternalInputCountryId = country.Id;
      }

      settings.FrameRate = (int)FrameRate.Automatic;
      settings.SupportedFrameSizes = (int)FrameRate.Automatic;
      settings.FrameSize = (int)FrameSize.Automatic;
      settings.SupportedFrameSizes = (int)FrameSize.Automatic;
      settings.VideoStandard = (int)AnalogVideoStandard.None;
      settings.SupportedVideoStandards = (int)AnalogVideoStandard.None;

      // Minimal loading, enough to build configuration.
      try
      {
        InitialiseGraph();
        if (_crossbar != null)
        {
          _crossbar.PerformLoading(Graph);
        }
        _capture.PerformLoading(Graph, ProductInstanceId, _crossbar);
        _encoder.PerformLoading(Graph, ProductInstanceId, _capture);
      }
      catch (TvExceptionNeedSoftwareEncoder)
      {
      }
      catch (Exception ex)
      {
        this.LogWarn(ex, "WDM analog: minimal loading failed, defaults may be inaccurate");
      }

      try
      {
        if (_crossbar != null)
        {
          CaptureSourceVideo videoSources = _crossbar.SupportedVideoSources;
          settings.SupportedVideoSources = (int)videoSources;
          CaptureSourceAudio audioSources = _crossbar.SupportedAudioSources;
          settings.SupportedAudioSources = (int)audioSources;

          if (videoSources == CaptureSourceVideo.None)
          {
            settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.None;
            foreach (CaptureSourceAudio audioSource in System.Enum.GetValues(typeof(CaptureSourceAudio)))
            {
              if (audioSource != CaptureSourceAudio.None && audioSources.HasFlag(audioSource))
              {
                settings.ExternalInputSourceAudio = (int)audioSource;
              }
            }
          }
          else
          {
            foreach (CaptureSourceVideo videoSource in System.Enum.GetValues(typeof(CaptureSourceVideo)))
            {
              if (videoSource != CaptureSourceVideo.None && videoSources.HasFlag(videoSource))
              {
                settings.ExternalInputSourceVideo = (int)videoSource;
              }
            }
            if (audioSources == CaptureSourceAudio.None)
            {
              settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.None;
            }
            else
            {
              settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.Automatic;
            }
          }
        }
        else
        {
          if (_capture.VideoFilter != null)
          {
            settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.TunerDefault;
            settings.SupportedVideoSources = (int)CaptureSourceVideo.TunerDefault;
          }
          if (_capture.AudioFilter != null)
          {
            settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.TunerDefault;
            settings.SupportedAudioSources = (int)CaptureSourceAudio.TunerDefault;
          }
        }

        if (_capture.VideoFilter != null)
        {
          if (country == null)
          {
            this.LogWarn("WDM analog: failed to get details for country {0}, using defaults for current standard {1}", countryName ?? "[null]", _capture.CurrentVideoStandard);
            settings.VideoStandard = (int)_capture.CurrentVideoStandard;
          }
          else if (!_capture.SupportedVideoStandards.HasFlag(country.VideoStandard))
          {
            this.LogWarn("WDM analog: recognised country {0} but standard {1} not supported, using defaults for current standard {2}", countryName, country.VideoStandard, _capture.CurrentVideoStandard);
            settings.VideoStandard = (int)_capture.CurrentVideoStandard;
          }
          else
          {
            this.LogDebug("WDM analog: recognised country {0}, using {1} defaults", countryName, country.VideoStandard);
            settings.VideoStandard = (int)country.VideoStandard;
          }
          settings.SupportedVideoStandards = (int)_capture.SupportedVideoStandards;

          bool isNtscStandard = (
            settings.VideoStandard == (int)AnalogVideoStandard.NtscM ||
            settings.VideoStandard == (int)AnalogVideoStandard.NtscMj ||
            settings.VideoStandard == (int)AnalogVideoStandard.Ntsc433 ||
            settings.VideoStandard == (int)AnalogVideoStandard.PalM
          );
          if (
            _capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1920_1080) ||
            _capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1440_1080) ||
            _capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1280_720)
          )
          {
            // Probably a capture device. Prefer high resolution.
            if (_capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1920_1080))
            {
              settings.FrameSize = (int)FrameSize.Fs1920_1080;
            }
            else if (_capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1440_1080))
            {
              settings.FrameSize = (int)FrameSize.Fs1440_1080;
            }
            else
            {
              settings.FrameSize = (int)FrameSize.Fs1280_720;
            }
            if (isNtscStandard)
            {
              if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr60))
              {
                settings.FrameRate = (int)FrameRate.Fr60;
              }
              else if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr59_94))
              {
                settings.FrameRate = (int)FrameRate.Fr59_94;
              }
              else if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr30))
              {
                settings.FrameRate = (int)FrameRate.Fr30;
              }
              else if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr29_97))
              {
                settings.FrameRate = (int)FrameRate.Fr29_97;
              }
              else
              {
                settings.FrameRate = GetMaxFlagValue((int)_capture.SupportedFrameRates);
              }
            }
            else
            {
              if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr50))
              {
                settings.FrameRate = (int)FrameRate.Fr50;
              }
              else if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr25))
              {
                settings.FrameRate = (int)FrameRate.Fr25;
              }
              else
              {
                settings.FrameRate = GetMaxFlagValue((int)_capture.SupportedFrameRates);
              }
            }
          }
          else
          {
            if (isNtscStandard && _capture.SupportedFrameSizes.HasFlag(FrameSize.Fs720_480))
            {
              settings.FrameSize = (int)FrameSize.Fs720_480;
              if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr29_97))
              {
                settings.FrameRate = (int)FrameRate.Fr29_97;
              }
              else
              {
                settings.FrameRate = GetMaxFlagValue((int)_capture.SupportedFrameRates);
              }
            }
            else if (!isNtscStandard && _capture.SupportedFrameSizes.HasFlag(FrameSize.Fs720_576))
            {
              settings.FrameSize = (int)FrameSize.Fs720_576;
              if (_capture.SupportedFrameRates.HasFlag(FrameRate.Fr25))
              {
                settings.FrameRate = (int)FrameRate.Fr25;
              }
              else
              {
                settings.FrameRate = GetMaxFlagValue((int)_capture.SupportedFrameRates);
              }
            }
            else
            {
              settings.FrameSize = GetMaxFlagValue((int)_capture.SupportedFrameSizes);
              settings.FrameRate = GetMaxFlagValue((int)_capture.SupportedFrameRates);
            }
          }
          this.LogDebug("WDM analog: frame size = {0}, frame rate = {1}", (FrameSize)settings.FrameSize, (FrameRate)settings.FrameRate);
          settings.SupportedFrameSizes = (int)_capture.SupportedFrameSizes;
          settings.SupportedFrameRates = (int)_capture.SupportedFrameRates;

          var tempProperties = new List<TVDatabase.Entities.TunerProperty>();
          foreach (var p in _capture.SupportedProperties)
          {
            p.IdTuner = TunerId;
            tempProperties.Add(p);
          }
          properties = tempProperties;
        }
      }
      finally
      {
        if (_crossbar != null)
        {
          _crossbar.PerformUnloading(Graph);
        }
        _capture.PerformUnloading(Graph);
        _encoder.PerformUnloading(Graph);
        CleanUpGraph(false);
      }

      settings = AnalogTunerSettingsManagement.SaveAnalogTunerSettings(settings);
      if (properties != null)
      {
        properties = TunerPropertyManagement.SaveTunerProperties(properties);
      }
    }

    private static int GetMaxFlagValue(int flags)
    {
      if (flags < 2)
      {
        return flags;
      }
      int value = 1;
      while (flags > 1)
      {
        flags >>= 1;
        value <<= 1;
      }
      return value;
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      this.LogDebug("WDM analog: perform loading");

      InitialiseGraph();

      if (_crossbar != null)
      {
        _crossbar.PerformLoading(Graph);
        if (_tuner != null)
        {
          _tuner.PerformLoading(Graph, ProductInstanceId, _crossbar);
        }
      }

      _capture.PerformLoading(Graph, ProductInstanceId, _crossbar);
      _encoder.PerformLoading(Graph, ProductInstanceId, _capture);

      // Check for and load extensions, adding any additional filters to the graph.
      IList<ITunerExtension> extensions;
      IBaseFilter lastFilter = _encoder.TsMultiplexerFilter;
      if (_crossbar != null)
      {
        extensions = LoadExtensions(_crossbar.Filter, ref lastFilter);
      }
      else
      {
        extensions = LoadExtensions(_capture.VideoFilter ?? _capture.AudioFilter, ref lastFilter);
      }

      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Analog;
      }
      AddAndConnectTsWriterIntoGraph(lastFilter, streamFormat);
      CompleteGraph();

      _capture.OnGraphCompleted();
      return extensions;
    }

    /// <summary>
    /// Actually unload the tuner.
    /// </summary>
    /// <param name="isFinalising"><c>True</c> if the tuner is being finalised.</param>
    public override void PerformUnloading(bool isFinalising = false)
    {
      this.LogDebug("WDM analog: perform unloading");

      if (!isFinalising)
      {
        if (_tuner != null)
        {
          _tuner.PerformUnloading(Graph);
        }
        if (_crossbar != null)
        {
          _crossbar.PerformUnloading(Graph);
        }
        if (_capture != null)
        {
          _capture.PerformUnloading(Graph);
        }
        if (_encoder != null)
        {
          _encoder.PerformUnloading(Graph);
        }

        RemoveTsWriterFromGraph();
      }

      CleanUpGraph(isFinalising);
    }

    #endregion

    #region tuning

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if (!base.CanTune(channel))
      {
        return false;
      }

      // Check that the selected inputs are available for capture channels.
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel == null)
      {
        return true;
      }
      return _tunableSourcesVideo.HasFlag(captureChannel.VideoSource) && _tunableSourcesAudio.HasFlag(captureChannel.AudioSource);
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("WDM analog: perform tuning");
      ChannelAnalogTv analogTvChannel = channel as ChannelAnalogTv;
      ChannelCapture captureChannel = channel as ChannelCapture;
      ChannelFmRadio fmRadioChannel = channel as ChannelFmRadio;
      if (analogTvChannel == null && captureChannel == null && fmRadioChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IChannel tuneChannel;
      _externalTuner.Tune(channel, out tuneChannel);

      if (tuneChannel is ChannelAnalogTv || tuneChannel is ChannelFmRadio)
      {
        if (_tuner != null)
        {
          _tuner.PerformTuning(tuneChannel);
        }
        else
        {
          this.LogWarn("WDM analog: ignored request to tune with a capture device");
        }
      }
      if (_crossbar != null)
      {
        _crossbar.PerformTuning(tuneChannel);
      }
      _capture.PerformTuning(tuneChannel);
      _encoder.PerformTuning(tuneChannel);
    }

    #endregion

    #region signal

    /// <summary>
    /// Get the tuner's signal status.
    /// </summary>
    /// <param name="isLocked"><c>True</c> if the tuner has locked onto signal.</param>
    /// <param name="isPresent"><c>True</c> if the tuner has detected signal.</param>
    /// <param name="strength">An indication of signal strength. Range: 0 to 100.</param>
    /// <param name="quality">An indication of signal quality. Range: 0 to 100.</param>
    /// <param name="onlyGetLock"><c>True</c> to only get lock status.</param>
    public override void GetSignalStatus(out bool isLocked, out bool isPresent, out int strength, out int quality, bool onlyGetLock)
    {
      if (_tuner == null)
      {
        // Capture sources don't have a tuner.
        isLocked = true;
        isPresent = true;
        strength = 100;
        quality = 100;
        return;
      }

      _tuner.GetSignalStatus(out isLocked, out strength);
      isPresent = isLocked;
      quality = strength;
    }

    #endregion

    #endregion
  }
}