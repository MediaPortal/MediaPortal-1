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
using System.Diagnostics;
using System.Globalization;
using DirectShowLib;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog.Component;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils.ExtensionMethods;
using AnalogVideoStandard = Mediaportal.TV.Server.Common.Types.Enum.AnalogVideoStandard;
using MediaType = Mediaportal.TV.Server.Common.Types.Enum.MediaType;
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

    // Current setting values.
    private CaptureSourceVideo _externalInputSourceVideo;
    private CaptureSourceAudio _externalInputSourceAudio;
    private int _externalInputCountryId;
    private short _externalInputPhysicalChannelNumber;
    private string _externalTunerProgram = string.Empty;
    private string _externalTunerProgramArguments = string.Empty;

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

    /// <summary>
    /// Get a list of channels representing the external non-tuner sources available from this tuner.
    /// </summary>
    /// <returns>a list of channels, one channel per source</returns>
    public IList<IChannel> GetSourceChannels()
    {
      if (_crossbar == null)
      {
        ChannelCapture channel = new ChannelCapture();
        channel.AudioSource = CaptureSourceAudio.TunerDefault;
        channel.IsEncrypted = false;
        channel.IsVcrSignal = false;
        channel.LogicalChannelNumber = channel.DefaultLogicalChannelNumber;
        channel.Provider = "Capture";
        if (_capture.VideoFilter != null)
        {
          channel.MediaType = MediaType.Television;
          channel.Name = "Tuner " + TunerId + " Video Capture";
          channel.VideoSource = CaptureSourceVideo.TunerDefault;
        }
        else
        {
          channel.MediaType = MediaType.Radio;
          channel.Name = "Tuner " + TunerId + " Audio Capture";
          channel.VideoSource = CaptureSourceVideo.None;
        }
        return new List<IChannel>() { channel };
      }

      IList<IChannel> channels = new List<IChannel>();
      CaptureSourceVideo supportedVideoSources = _crossbar.SupportedVideoSources;
      if (supportedVideoSources != CaptureSourceVideo.None)
      {
        foreach (CaptureSourceVideo source in System.Enum.GetValues(typeof(CaptureSourceVideo)))
        {
          if (source != CaptureSourceVideo.None && source != CaptureSourceVideo.Tuner && supportedVideoSources.HasFlag(source))
          {
            ChannelCapture channel = new ChannelCapture();
            channel.AudioSource = CaptureSourceAudio.Automatic;
            channel.Name = string.Format("Tuner {0} {1} Video Source", TunerId, channel.VideoSource.GetDescription());
            channel.MediaType = MediaType.Television;
            channel.VideoSource = source;
            channel.IsEncrypted = false;
            channel.IsVcrSignal = false;
            channel.LogicalChannelNumber = channel.DefaultLogicalChannelNumber;
            channel.Provider = "External Input";
            channels.Add(channel);
          }
        }
        return channels;
      }

      CaptureSourceAudio supportedAudioSources = _crossbar.SupportedAudioSources;
      if (supportedAudioSources != CaptureSourceAudio.None)
      {
        foreach (CaptureSourceAudio source in System.Enum.GetValues(typeof(CaptureSourceAudio)))
        {
          if (source != CaptureSourceAudio.None && source != CaptureSourceAudio.Tuner && supportedAudioSources.HasFlag(source))
          {
            ChannelCapture channel = new ChannelCapture();
            channel.AudioSource = source;
            channel.MediaType = MediaType.Radio;
            channel.Name = string.Format("Tuner {0} {1} Audio Source", TunerId, channel.AudioSource.GetDescription());
            channel.VideoSource = CaptureSourceVideo.None;
            channel.IsEncrypted = false;
            channel.IsVcrSignal = false;
            channel.LogicalChannelNumber = channel.DefaultLogicalChannelNumber;
            channel.Provider = "External Input";
            channels.Add(channel);
          }
        }
      }
      return channels;
    }

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
        GetDefaultConfiguration(out settings, out properties);
        configuration.AnalogTunerSettings = settings;
        if (properties != null)
        {
          foreach (var p in properties)
          {
            configuration.TunerProperties.Add(p);
          }
        }
      }

      _externalInputSourceVideo = (CaptureSourceVideo)configuration.AnalogTunerSettings.ExternalInputSourceVideo;
      _externalInputSourceAudio = (CaptureSourceAudio)configuration.AnalogTunerSettings.ExternalInputSourceAudio;
      _externalInputCountryId = configuration.AnalogTunerSettings.ExternalInputCountryId;
      _externalInputPhysicalChannelNumber = (short)configuration.AnalogTunerSettings.ExternalInputPhysicalChannelNumber;
      _externalTunerProgram = configuration.AnalogTunerSettings.ExternalTunerProgram;
      _externalTunerProgramArguments = configuration.AnalogTunerSettings.ExternalTunerProgramArguments;

      this.LogDebug("  external input...");
      this.LogDebug("    video source  = {0} ({1})", _externalInputSourceVideo, (CaptureSourceVideo)configuration.AnalogTunerSettings.SupportedVideoSources);
      this.LogDebug("    audio source  = {0} ({1})", _externalInputSourceAudio, (CaptureSourceAudio)configuration.AnalogTunerSettings.SupportedAudioSources);
      this.LogDebug("    country       = {0}", _externalInputCountryId);
      this.LogDebug("    phys. channel = {0}", _externalInputPhysicalChannelNumber);
      this.LogDebug("  external tuner...");
      this.LogDebug("    program       = {0}", _externalTunerProgram);
      this.LogDebug("    program args  = {0}", _externalTunerProgramArguments);

      if (_capture != null)
      {
        _capture.ReloadConfiguration(configuration);
      }
      if (_encoder != null)
      {
        _encoder.ReloadConfiguration(configuration);
      }
    }

    /// <summary>
    /// Get sensible default configuration based on country and hardware capabilities.
    /// </summary>
    private void GetDefaultConfiguration(out TVDatabase.Entities.AnalogTunerSettings settings, out IEnumerable<TVDatabase.Entities.TunerProperty> properties)
    {
      this.LogDebug("WDM analog: first detection, get default configuration");
      settings = new TVDatabase.Entities.AnalogTunerSettings();
      properties = null;

      settings.IdAnalogTunerSettings = TunerId;
      settings.IdVideoEncoder = null;
      settings.IdAudioEncoder = null;
      settings.ExternalTunerProgram = string.Empty;
      settings.ExternalTunerProgramArguments = string.Empty;

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
      settings.ExternalInputPhysicalChannelNumber = 7;

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
          foreach (CaptureSourceVideo videoSource in System.Enum.GetValues(typeof(CaptureSourceVideo)))
          {
            if (videoSource != CaptureSourceVideo.None && videoSources.HasFlag(videoSource))
            {
              settings.ExternalInputSourceVideo = (int)videoSource;
            }
          }
          settings.SupportedVideoSources = (int)videoSources;

          CaptureSourceAudio audioSources = _crossbar.SupportedAudioSources;
          foreach (CaptureSourceAudio audioSource in System.Enum.GetValues(typeof(CaptureSourceAudio)))
          {
            if (audioSource != CaptureSourceAudio.None && audioSources.HasFlag(audioSource))
            {
              settings.ExternalInputSourceAudio = (int)audioSource;
            }
          }
          settings.SupportedAudioSources = (int)audioSources;
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
            this.LogWarn("WDM analog capture: failed to get details for country {0}, using defaults for current standard {1}", countryName ?? "[null]", _capture.CurrentVideoStandard);
            settings.VideoStandard = (int)_capture.CurrentVideoStandard;
          }
          else if (!_capture.SupportedVideoStandards.HasFlag(country.VideoStandard))
          {
            this.LogWarn("WDM analog capture: recognised country {0} but standard {1} not supported, using defaults for current standard {2}", countryName, country.VideoStandard, _capture.CurrentVideoStandard);
            settings.VideoStandard = (int)_capture.CurrentVideoStandard;
          }
          else
          {
            this.LogDebug("WDM analog capture: recognised country {0}, using {1} defaults", countryName, country.VideoStandard);
            settings.VideoStandard = (int)country.VideoStandard;
          }
          settings.SupportedVideoStandards = (int)_capture.SupportedVideoStandards;

          bool isNtscStandard = (
            settings.VideoStandard == (int)AnalogVideoStandard.NtscM ||
            settings.VideoStandard == (int)AnalogVideoStandard.NtscMj ||
            settings.VideoStandard == (int)AnalogVideoStandard.Ntsc433 ||
            settings.VideoStandard == (int)AnalogVideoStandard.PalM
          );
          if (_capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1920_1080) || _capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1280_720))
          {
            // Probably a capture device. Prefer high resolution.
            if (_capture.SupportedFrameSizes.HasFlag(FrameSize.Fs1920_1080))
            {
              settings.FrameSize = (int)FrameSize.Fs1920_1080;
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
          this.LogDebug("WDM analog capture: frame size = {0}, frame rate = {1}", (FrameSize)settings.FrameSize, (FrameRate)settings.FrameRate);
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
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      ChannelAnalogTv analogTvChannel = channel as ChannelAnalogTv;
      ChannelCapture captureChannel = channel as ChannelCapture;
      ChannelFmRadio fmRadioChannel = channel as ChannelFmRadio;
      if (analogTvChannel == null && captureChannel == null && fmRadioChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      // Channel or external input settings?
      bool useExternalTuner = false;
      IChannel tuneChannel = channel;
      if (captureChannel != null)
      {
        if (captureChannel.VideoSource == CaptureSourceVideo.TunerDefault)
        {
          useExternalTuner = true;
          if (_externalInputSourceVideo != CaptureSourceVideo.Tuner)
          {
            captureChannel.VideoSource = _externalInputSourceVideo;
            captureChannel.MediaType = MediaType.Television;
            if (captureChannel.VideoSource == CaptureSourceVideo.None)
            {
              captureChannel.MediaType = MediaType.Radio;
            }
          }
          else
          {
            ChannelAnalogTv externalAnalogTvChannel = new ChannelAnalogTv();
            externalAnalogTvChannel.Name = channel.Name;
            externalAnalogTvChannel.Provider = channel.Provider;
            externalAnalogTvChannel.LogicalChannelNumber = channel.LogicalChannelNumber;
            externalAnalogTvChannel.MediaType = MediaType.Television;
            externalAnalogTvChannel.IsEncrypted = channel.IsEncrypted;
            externalAnalogTvChannel.IsHighDefinition = channel.IsHighDefinition;
            externalAnalogTvChannel.IsThreeDimensional = channel.IsThreeDimensional;
            externalAnalogTvChannel.TunerSource = AnalogTunerSource.Cable;
            externalAnalogTvChannel.Country = CountryCollection.Instance.GetCountryById(_externalInputCountryId);
            externalAnalogTvChannel.PhysicalChannelNumber = _externalInputPhysicalChannelNumber;
            tuneChannel = externalAnalogTvChannel;
          }
        }

        if (captureChannel.AudioSource == CaptureSourceAudio.TunerDefault)
        {
          useExternalTuner = true;
          captureChannel.AudioSource = _externalInputSourceAudio;
        }
      }

      // External tuner?
      if (useExternalTuner && !string.IsNullOrEmpty(_externalTunerProgram))
      {
        this.LogDebug("WDM analog: using external tuner");
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = true;
        startInfo.ErrorDialog = false;
        startInfo.LoadUserProfile = false;
        startInfo.UseShellExecute = true;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = _externalTunerProgram;
        startInfo.Arguments = _externalTunerProgramArguments.Replace("%channel number%", channel.LogicalChannelNumber.ToString());
        try
        {
          Process p = Process.Start(startInfo);
          if (p.WaitForExit(10000))
          {
            this.LogDebug("WDM analog: external tuner process exited with code {0}", p.ExitCode);
          }
          else
          {
            this.LogWarn("WDM analog: external tuner process failed to exit within 10 seconds");
          }
        }
        catch (Exception ex)
        {
          this.LogWarn(ex, "WDM analog: external tuner process threw exception");
        }
      }

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