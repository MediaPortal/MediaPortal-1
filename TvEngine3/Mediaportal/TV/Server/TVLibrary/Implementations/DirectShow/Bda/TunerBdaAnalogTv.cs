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
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Country;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Implementations.Analog;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;
using AnalogVideoStandard = Mediaportal.TV.Server.Common.Types.Enum.AnalogVideoStandard;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for analog TV tuners with BDA
  /// drivers.
  /// </summary>
  internal class TunerBdaAnalogTv : TunerBdaBase
  {
    #region variables

    private ExternalTuner _externalTuner = new ExternalTuner();

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaAnalogTv"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public TunerBdaAnalogTv(DsDevice device)
      : base(device, device.DevicePath + "ATV", BroadcastStandard.AnalogTelevision | BroadcastStandard.ExternalInput)
    {
      // The external input broadcast standard is included above because an STB
      // can be connected via RF/coax.
    }

    #endregion

    #region graph building

    /// <summary>
    /// Create and register a BDA tuning space for the tuner type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("BDA analog TV: create tuning space");

      IAnalogTVTuningSpace tuningSpace = null;
      try
      {
        tuningSpace = (IAnalogTVTuningSpace)new AnalogTVTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(NetworkType.ANALOG_AUX_IN);
        hr |= tuningSpace.put_CountryCode(0);
        hr |= tuningSpace.put_MinChannel(1);
        hr |= tuningSpace.put_MaxChannel(158);

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA analog TV: potential error creating tuning space, hr = 0x{0:x}", hr);
        }
        return tuningSpace;
      }
      catch
      {
        Release.ComObject("BDA analog TV tuner tuning space", ref tuningSpace);
        throw;
      }
    }

    /// <summary>
    /// Get the class ID of the network provider for the tuner type.
    /// </summary>
    protected override Guid NetworkProviderClsid
    {
      get
      {
        // I know that the generic network provider supports the tuning space.
        // However I don't know whether or which specific network provider
        // supports the tuning space.
        return typeof(DVBTNetworkProvider).GUID;
      }
    }

    /// <summary>
    /// Get the registered name of the BDA tuning space for the tuner type.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal Analog TV Tuning Space";
      }
    }

    #endregion

    #region tuning

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="tuningSpace">The tuner's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected override ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel)
    {
      ChannelAnalogTv analogTvChannel = channel as ChannelAnalogTv;
      if (analogTvChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IAnalogTVTuningSpace analogTvTuningSpace = tuningSpace as IAnalogTVTuningSpace;
      if (analogTvTuningSpace == null)
      {
        throw new TvException("Failed to find analog TV tuning space interface on tuning space.");
      }

      int hr = analogTvTuningSpace.put_CountryCode(analogTvChannel.Country.ItuCode);

      TunerInputType inputType;
      if (analogTvChannel.TunerSource == AnalogTunerSource.Antenna)
      {
        inputType = TunerInputType.Antenna;
      }
      else if (analogTvChannel.TunerSource == AnalogTunerSource.Cable)
      {
        inputType = TunerInputType.Cable;
      }
      else
      {
        throw new TvException("Failed to translate tuner source {0}.", analogTvChannel.TunerSource);
      }
      hr |= analogTvTuningSpace.put_InputType(inputType);

      ITuneRequest tuneRequest;
      hr |= tuningSpace.CreateTuneRequest(out tuneRequest);
      TvExceptionDirectShowError.Throw(hr, "Failed to create tuning request from tuning space.");
      try
      {
        IChannelTuneRequest channelTuneRequest = tuneRequest as IChannelTuneRequest;
        if (channelTuneRequest == null)
        {
          throw new TvException("Failed to find channel tune request interface on tune request.");
        }

        // Note: custom frequency mappings currently not supported.
        hr |= channelTuneRequest.put_Channel(analogTvChannel.PhysicalChannelNumber);

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA analog TV: potential error assembling tune request, hr = 0x{0:x}", hr);
        }

        return channelTuneRequest;
      }
      catch
      {
        Release.ComObject("BDA auxiliary input tuner tune request", ref tuneRequest);
        throw;
      }
    }

    /// <summary>
    /// Tune using the MediaPortal network provider.
    /// </summary>
    /// <param name="networkProvider">The network provider interface to use for tuning.</param>
    /// <param name="channel">The channel to tune to.</param>
    /// <returns>an HRESULT indicating whether the tuning parameters were applied successfully</returns>
    protected override int PerformMediaPortalNetworkProviderTuning(IDvbNetworkProvider networkProvider, IChannel channel)
    {
      throw new TvException("The MediaPortal network provider does not support tuning analog television.");
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
      this.LogDebug("BDA analog TV: reload configuration");
      base.ReloadConfiguration(configuration);

      if (configuration.AnalogTunerSettings == null)
      {
        configuration.AnalogTunerSettings = CreateDefaultConfiguration();
      }

      _externalTuner.ReloadConfiguration(configuration.AnalogTunerSettings);
    }

    /// <summary>
    /// Create sensible default configuration based on hardware capabilities.
    /// </summary>
    private AnalogTunerSettings CreateDefaultConfiguration()
    {
      this.LogDebug("BDA analog TV: first detection, create default configuration");
      AnalogTunerSettings settings = new AnalogTunerSettings();

      settings.IdAnalogTunerSettings = TunerId;
      settings.IdVideoEncoder = null;
      settings.IdAudioEncoder = null;
      settings.EncoderBitRateModeTimeShifting = (int)EncodeMode.ConstantBitRate;
      settings.EncoderBitRateTimeShifting = 100;
      settings.EncoderBitRatePeakTimeShifting = 100;
      settings.EncoderBitRateModeRecording = (int)EncodeMode.ConstantBitRate;
      settings.EncoderBitRateRecording = 100;
      settings.EncoderBitRatePeakRecording = 100;

      settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.Tuner;
      settings.SupportedVideoSources = (int)CaptureSourceVideo.Tuner;
      settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.Tuner;
      settings.SupportedAudioSources = (int)CaptureSourceAudio.Tuner;

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

      return AnalogTunerSettingsManagement.SaveAnalogTunerSettings(settings);
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
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Analog;
      }
      return base.PerformLoading(streamFormat);
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
      if (
        captureChannel == null ||
        (
          CaptureSourceVideo.TunerDefault.HasFlag(captureChannel.VideoSource) &&
          CaptureSourceAudio.TunerDefault.HasFlag(captureChannel.AudioSource)
        )
      )
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("BDA analog TV: perform tuning");
      base.PerformTuning(_externalTuner.Tune(channel));
    }

    #endregion

    #endregion
  }
}