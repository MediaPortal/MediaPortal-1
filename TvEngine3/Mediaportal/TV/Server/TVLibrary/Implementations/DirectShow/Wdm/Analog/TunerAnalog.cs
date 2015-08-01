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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils.ExtensionMethods;
using MediaType = Mediaportal.TV.Server.Common.Types.Enum.MediaType;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Wdm.Analog
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles analog tuners
  /// and capture devices with WDM/DirectShow drivers.
  /// </summary>
  internal class TunerAnalog : TunerDirectShowBase
  {
    #region variables

    private Mediaportal.TV.Server.TVDatabase.Entities.AnalogTunerSettings _settings = null;

    private Tuner _tuner = null;
    private Crossbar _crossbar = null;
    private Capture _capture = null;
    private Encoder _encoder = null;

    private IChannel _externalTunerChannel = null;
    private string _externalTunerCommand = string.Empty;
    private string _externalTunerCommandArguments = string.Empty;

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
        _crossbar = new Crossbar(_deviceMain);
        _capture = new Capture();
      }
      else
      {
        _capture = new Capture(_deviceMain);
      }
      _encoder = new Encoder();
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
        channel.LogicalChannelNumber = "10000";
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
            channel.LogicalChannelNumber = "10000";
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
            channel.LogicalChannelNumber = "10000";
            channel.Provider = "External Input";
            channels.Add(channel);
          }
        }
      }
      return channels;
    }

    private void DebugSettings()
    {
      this.LogDebug("  external input...");
      this.LogDebug("    video source   = {0} ({1})", (CaptureSourceVideo)_settings.ExternalInputSourceVideo, (CaptureSourceVideo)_settings.SupportedVideoSources);
      this.LogDebug("    audio source   = {0} ({1})", (CaptureSourceAudio)_settings.ExternalInputSourceAudio, (CaptureSourceAudio)_settings.SupportedAudioSources);
      this.LogDebug("    country        = {0}", _settings.ExternalInputCountryId);
      this.LogDebug("    phys. channel  = {0}", _settings.ExternalInputPhysicalChannelNumber);
      this.LogDebug("  external tuner...");
      this.LogDebug("    program        = {0}", _settings.ExternalTunerProgram ?? string.Empty);
      this.LogDebug("    program args   = {0}", _settings.ExternalTunerProgramArguments ?? string.Empty);
    }

    #region ITunerInternal members

    #region configuration

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();

      this.LogDebug("WDM analog: reload configuration");

      bool isFirstLoad = false;
      _settings = AnalogTunerSettingsManagement.GetAnalogTunerSettings(TunerId);
      if (_settings == null)
      {
        isFirstLoad = true;
        _settings = new TVDatabase.Entities.AnalogTunerSettings();
        _settings.IdAnalogTunerSettings = TunerId;
        _settings.ExternalTunerProgram = string.Empty;
        _settings.ExternalTunerProgramArguments = string.Empty;

        Country country = CountryCollection.Instance.GetCountryByName(RegionInfo.CurrentRegion.EnglishName);
        if (country == null)
        {
          country = CountryCollection.Instance.GetCountryByIsoCode("US");
        }
        _settings.ExternalInputCountryId = country.Id;
        _settings.ExternalInputPhysicalChannelNumber = 7;

        // We'll fill in these details after loading.
        _settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.None;
        _settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.None;
        _settings.SupportedVideoSources = (int)CaptureSourceVideo.None;
        _settings.SupportedAudioSources = (int)CaptureSourceAudio.None;
      }
      else
      {
        DebugSettings();
      }

      if (_capture != null)
      {
        _capture.ReloadConfiguration(_settings);
      }
      if (_encoder != null)
      {
        _encoder.ReloadConfiguration(_settings);
      }
      if (isFirstLoad)
      {
        _settings = AnalogTunerSettingsManagement.SaveAnalogTunerSettings(_settings);
      }
    }

    #endregion

    #region state control

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading()
    {
      this.LogDebug("WDM analog: perform loading");

      InitialiseGraph();

      if (_crossbar != null)
      {
        _crossbar.PerformLoading(_graph);
        if (_tuner != null)
        {
          _tuner.PerformLoading(_graph, ProductInstanceId, _crossbar);
        }
      }

      _capture.PerformLoading(_graph, ProductInstanceId, _crossbar, _settings);
      _encoder.PerformLoading(_graph, ProductInstanceId, _capture, _settings);

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

      AddAndConnectTsWriterIntoGraph(lastFilter);
      CompleteGraph();

      // Update supported video and audio sources after first load.
      if (
        (SupportedBroadcastStandards.HasFlag(BroadcastStandard.AnalogTelevision) || SupportedBroadcastStandards.HasFlag(BroadcastStandard.ExternalInput)) &&
        _settings.SupportedVideoSources == (int)CaptureSourceVideo.None &&
        _settings.SupportedAudioSources == (int)CaptureSourceAudio.None
      )
      {
        this.LogDebug("WDM analog: first load, setting defaults");
        if (_crossbar != null)
        {
          CaptureSourceVideo videoSources = _crossbar.SupportedVideoSources;
          foreach (CaptureSourceVideo videoSource in System.Enum.GetValues(typeof(CaptureSourceVideo)))
          {
            if (videoSource != CaptureSourceVideo.None)
            {
              _settings.ExternalInputSourceVideo = (int)videoSource;
            }
          }
          _settings.SupportedVideoSources = (int)videoSources;

          CaptureSourceAudio audioSources = _crossbar.SupportedAudioSources;
          foreach (CaptureSourceAudio audioSource in System.Enum.GetValues(typeof(CaptureSourceAudio)))
          {
            if (audioSource != CaptureSourceAudio.None)
            {
              _settings.ExternalInputSourceAudio = (int)audioSource;
            }
          }
          _settings.SupportedAudioSources = (int)audioSources;
        }
        else
        {
          if (_capture.VideoFilter != null)
          {
            _settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.TunerDefault;
            _settings.SupportedVideoSources = (int)CaptureSourceVideo.TunerDefault;
          }
          if (_capture.AudioFilter != null)
          {
            _settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.TunerDefault;
            _settings.SupportedAudioSources = (int)CaptureSourceAudio.TunerDefault;
          }
        }

        _settings = AnalogTunerSettingsManagement.SaveAnalogTunerSettings(_settings);
        DebugSettings();
      }

      _capture.OnGraphCompleted(_settings);

      _epgGrabber = null;   // Teletext scraping and RDS grabbing currently not supported.

      _channelScanner = new ChannelScannerDirectShowAnalog(this, _filterTsWriter as ITsChannelScan);
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
          _tuner.PerformUnloading(_graph);
        }
        if (_crossbar != null)
        {
          _crossbar.PerformUnloading(_graph);
        }
        if (_capture != null)
        {
          _capture.PerformUnloading(_graph);
        }
        if (_encoder != null)
        {
          _encoder.PerformUnloading(_graph);
        }
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
          if (_settings.ExternalInputSourceVideo != (int)CaptureSourceVideo.Tuner)
          {
            captureChannel.VideoSource = (CaptureSourceVideo)_settings.ExternalInputSourceVideo;
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
            externalAnalogTvChannel.Country = CountryCollection.Instance.GetCountryById(_settings.ExternalInputCountryId);
            externalAnalogTvChannel.PhysicalChannelNumber = (short)_settings.ExternalInputPhysicalChannelNumber;
            tuneChannel = externalAnalogTvChannel;
          }
        }

        if (captureChannel.AudioSource == CaptureSourceAudio.TunerDefault)
        {
          useExternalTuner = true;
          captureChannel.AudioSource = (CaptureSourceAudio)_settings.ExternalInputSourceAudio;
        }
      }

      // External tuner?
      if (useExternalTuner && !string.IsNullOrEmpty(_settings.ExternalTunerProgram))
      {
        this.LogDebug("WDM analog: using external tuner");
        tuneChannel = _externalTunerChannel;
        _externalTunerChannel.LogicalChannelNumber = channel.LogicalChannelNumber;
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = true;
        startInfo.ErrorDialog = false;
        startInfo.LoadUserProfile = false;
        startInfo.UseShellExecute = true;
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.FileName = _externalTunerCommand;
        startInfo.Arguments = _externalTunerCommandArguments.Replace("%channel number%", channel.LogicalChannelNumber.ToString());
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