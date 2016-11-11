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
  /// An implementation of <see cref="ITuner"/> for any tuner with BDA drivers
  /// that has auxiliary inputs.
  /// </summary>
  internal class TunerBdaAuxiliaryInput : TunerBdaBase
  {
    #region constants

    private const string TUNING_SPACE_NAME = "MediaPortal Auxiliary Input Tuning Space";

    #endregion

    #region variables

    private CaptureSourceVideo _supportedVideoSources;
    private ExternalTuner _externalTuner = new ExternalTuner();

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaAuxiliaryInput"/>
    /// class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="supportedVideoSources">The video sources supported by the hardware.</param>
    public TunerBdaAuxiliaryInput(DsDevice device, CaptureSourceVideo supportedVideoSources)
      : base(device, device.DevicePath + "AUX", BroadcastStandard.ExternalInput)
    {
      _supportedVideoSources = supportedVideoSources;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Create a BDA tuning space for tuning a given channel type.
    /// </summary>
    /// <param name="channelType">The channel type.</param>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace(Type channelType)
    {
      this.LogDebug("BDA auxiliary input: create tuning space, type = {0}", channelType.Name);

      IAuxInTuningSpace2 tuningSpace = null;
      try
      {
        tuningSpace = (IAuxInTuningSpace2)new AuxInTuningSpace();
        int hr = tuningSpace.put_CountryCode(0);
        hr |= tuningSpace.put_FriendlyName(TUNING_SPACE_NAME);
        hr |= tuningSpace.put__NetworkType(NetworkType.ANALOG_AUX_IN);
        hr |= tuningSpace.put_UniqueName(TUNING_SPACE_NAME);

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA auxiliary input: potential error creating tuning space, hr = 0x{0:x}, type = {1}", hr, channelType.Name);
        }
        return tuningSpace;
      }
      catch
      {
        Release.ComObject(string.Format("BDA auxiliary input tuner {0} tuning space", channelType.Name), ref tuningSpace);
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
    /// Get the name(s) of the registered BDA tuning space(s) for the tuner type.
    /// </summary>
    protected override IDictionary<string, Type> TuningSpaceNames
    {
      get
      {
        return new Dictionary<string, Type>
        {
          { TUNING_SPACE_NAME, null }
        };
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
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ITuneRequest tuneRequest;
      int hr = tuningSpace.CreateTuneRequest(out tuneRequest);
      TvExceptionDirectShowError.Throw(hr, "Failed to create tune request from tuning space.");
      try
      {
        IChannelTuneRequest channelTuneRequest = tuneRequest as IChannelTuneRequest;
        if (channelTuneRequest == null)
        {
          throw new TvException("Failed to find channel tune request interface on tune request.");
        }

        // This mapping is based on the MSDN auxiliary input tuning space
        // description:
        // https://msdn.microsoft.com/en-us/library/windows/desktop/dd692997(v=vs.85).aspx
        // ...and verified with an ATI CableCARD tuner.
        int channelNumber;
        if (captureChannel.VideoSource == CaptureSourceVideo.Svideo1)
        {
          channelNumber = 0;
        }
        else if (captureChannel.VideoSource == CaptureSourceVideo.Svideo2)
        {
          channelNumber = 2;
        }
        else if (captureChannel.VideoSource == CaptureSourceVideo.Svideo3)
        {
          channelNumber = 4;
        }
        else if (captureChannel.VideoSource == CaptureSourceVideo.Composite1)
        {
          channelNumber = 1;
        }
        else if (captureChannel.VideoSource == CaptureSourceVideo.Composite2)
        {
          channelNumber = 3;
        }
        else if (captureChannel.VideoSource == CaptureSourceVideo.Composite3)
        {
          channelNumber = 5;
        }
        else
        {
          throw new TvException("Failed to map video source {0} to channel number.", captureChannel.VideoSource);
        }
        hr |= channelTuneRequest.put_Channel(channelNumber);

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA auxiliary input: potential error assembling tune request, hr = 0x{0:x}", hr);
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
      throw new TvException("The MediaPortal network provider does not support tuning auxiliary inputs.");
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
      this.LogDebug("BDA auxiliary input: reload configuration");
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
      this.LogDebug("BDA auxiliary input: first detection, create default configuration");
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

      if (_supportedVideoSources.HasFlag(CaptureSourceVideo.Svideo1))
      {
        settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.Svideo1;
      }
      else
      {
        settings.ExternalInputSourceVideo = (int)CaptureSourceVideo.Composite1;
      }
      settings.SupportedVideoSources = (int)_supportedVideoSources;

      // It isn't possible to independently select the audio source.
      settings.ExternalInputSourceAudio = (int)CaptureSourceAudio.TunerDefault;
      settings.SupportedAudioSources = (int)CaptureSourceAudio.TunerDefault;

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

      // Check that the selected inputs are available.
      ChannelCapture captureChannel = channel as ChannelCapture;
      if (captureChannel == null)
      {
        return true;
      }
      return _supportedVideoSources.HasFlag(captureChannel.VideoSource) || captureChannel.VideoSource == CaptureSourceVideo.TunerDefault;
    }

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    public override void PerformTuning(IChannel channel)
    {
      this.LogDebug("BDA auxiliary input: perform tuning");
      base.PerformTuning(_externalTuner.Tune(channel));
    }

    #endregion

    #endregion
  }
}