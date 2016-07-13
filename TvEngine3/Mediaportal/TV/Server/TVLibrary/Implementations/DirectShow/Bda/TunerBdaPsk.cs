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
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;
using BdaPolarisation = DirectShowLib.BDA.Polarisation;
using TvePolarisation = Mediaportal.TV.Server.Common.Types.Enum.Polarisation;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for satellite tuners with BDA
  /// drivers.
  /// </summary>
  internal class TunerBdaPsk : TunerBdaBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaPsk"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    public TunerBdaPsk(DsDevice device, BroadcastStandard supportedBroadcastStandards)
      : base(device, device.DevicePath + "PSK", supportedBroadcastStandards)
    {
    }

    #endregion

    #region graph building

    /// <summary>
    /// Create and register a BDA tuning space for the tuner type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("BDA PSK: create tuning space");

      IDVBSTuningSpace tuningSpace = null;
      IDVBSLocator locator = null;
      try
      {
        tuningSpace = (IDVBSTuningSpace)new DVBSTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(NetworkType.DVB_SATELLITE);
        hr |= tuningSpace.put_SystemType(DVBSystemType.Satellite);
        hr |= tuningSpace.put_LowOscillator(9750000);
        hr |= tuningSpace.put_HighOscillator(10600000);
        hr |= tuningSpace.put_LNBSwitch(11700000);
        // Causes an access violation exception in some cases, presumably when
        // the tuner driver tries to send a DiSEqC command (which we don't
        // intend to do here).
        //hr |= tuningSpace.put_InputRange(-1);
        hr |= tuningSpace.put_NetworkID(-1);
        hr |= tuningSpace.put_SpectralInversion(SpectralInversion.Automatic);

        locator = (IDVBSLocator)new DVBSLocator();
        hr |= locator.put_Azimuth(-1);
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_Elevation(-1);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_Modulation(ModulationType.ModNotSet);
        hr |= locator.put_OrbitalPosition(-1);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_SignalPolarisation(BdaPolarisation.NotSet);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_WestPosition(false);

        IDVBSLocator2 s2Locator = locator as IDVBSLocator2;
        if (s2Locator != null)
        {
          hr |= s2Locator.put_DiseqLNBSource(LNB_Source.NOT_SET);
          hr |= s2Locator.put_LocalOscillatorOverrideLow(-1);
          hr |= s2Locator.put_LocalOscillatorOverrideHigh(-1);
          hr |= s2Locator.put_LocalLNBSwitchOverride(-1);
          hr |= s2Locator.put_LocalSpectralInversionOverride(SpectralInversion.NotSet);
          hr |= s2Locator.put_SignalPilot(Pilot.NotSet);
          hr |= s2Locator.put_SignalRollOff(RollOff.NotSet);
        }

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA PSK: potential error creating tuning space, hr = 0x{0:x}", hr);
        }
        return tuningSpace;
      }
      catch
      {
        Release.ComObject("BDA PSK tuner tuning space", ref tuningSpace);
        Release.ComObject("BDA PSK tuner locator", ref locator);
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
        return typeof(DVBSNetworkProvider).GUID;
      }
    }

    /// <summary>
    /// Get the registered name of the BDA tuning space for the tuner type.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal PSK Tuning Space";
      }
    }

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    /// <param name="streamFormat">The format(s) of the streams that the tuner is expected to support.</param>
    /// <returns>the set of extensions loaded for the tuner, in priority order</returns>
    public override IList<ITunerExtension> PerformLoading(StreamFormat streamFormat = StreamFormat.Default)
    {
      if (streamFormat == StreamFormat.Default)
      {
        streamFormat = StreamFormat.Mpeg2Ts | StreamFormat.Dvb | StreamFormat.Freesat;
      }
      return base.PerformLoading(streamFormat);
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
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IDVBSTuningSpace dvbsTuningSpace = tuningSpace as IDVBSTuningSpace;
      if (dvbsTuningSpace == null)
      {
        throw new TvException("Failed to find DVB-S tuning space interface on tuning space.");
      }

      int hr = dvbsTuningSpace.put_LowOscillator(SatelliteLnbHandler.LOW_BAND_LOF);
      hr |= dvbsTuningSpace.put_HighOscillator(SatelliteLnbHandler.HIGH_BAND_LOF);
      hr |= dvbsTuningSpace.put_LNBSwitch(SatelliteLnbHandler.SWITCH_FREQUENCY);

      ILocator locator;
      hr |= tuningSpace.get_DefaultLocator(out locator);
      TvExceptionDirectShowError.Throw(hr, "Failed to get the default locator for the tuning space.");
      try
      {
        IDVBSLocator dvbsLocator = locator as IDVBSLocator;
        if (dvbsLocator == null)
        {
          throw new TvException("Failed to find DVB-S locator interface on locator.");
        }

        ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;
        hr = dvbsLocator.put_CarrierFrequency(satelliteChannel.Frequency);
        hr |= dvbsLocator.put_SignalPolarisation(GetBdaPolarisation(satelliteChannel.Polarisation));
        hr |= dvbsLocator.put_SymbolRate(satelliteChannel.SymbolRate);
        hr |= dvbsLocator.put_Modulation(GetBdaModulation(satelliteChannel.ModulationScheme, dvbs2Channel != null));
        hr |= dvbsLocator.put_InnerFECRate(GetBdaFecCodeRate(satelliteChannel.FecCodeRate));

        IDVBSLocator2 dvbs2Locator = locator as IDVBSLocator2;
        if (dvbs2Locator != null)
        {
          if (dvbs2Channel != null)
          {
            hr |= dvbs2Locator.put_SignalPilot(GetBdaPilot(dvbs2Channel.PilotTonesState));
            hr |= dvbs2Locator.put_SignalRollOff(GetBdaRollOff(dvbs2Channel.RollOffFactor));
          }
          else
          {
            hr |= dvbs2Locator.put_SignalPilot(Pilot.NotSet);
            hr |= dvbs2Locator.put_SignalRollOff(RollOff.NotSet);
          }
        }

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        TvExceptionDirectShowError.Throw(hr, "Failed to create tuning request from tuning space.");
        try
        {
          IDVBTuneRequest dvbTuneRequest = tuneRequest as IDVBTuneRequest;
          if (dvbTuneRequest == null)
          {
            throw new TvException("Failed to find DVB tune request interface on tune request.");
          }

          ChannelDvbBase dvbChannel = channel as ChannelDvbBase;
          if (dvbChannel != null)
          {
            if (dvbChannel.OriginalNetworkId > 0)
            {
              hr |= dvbTuneRequest.put_ONID(dvbChannel.OriginalNetworkId);
            }
            if (dvbChannel.TransportStreamId > 0)
            {
              hr |= dvbTuneRequest.put_TSID(dvbChannel.TransportStreamId);
            }
            if (dvbChannel.ServiceId > 0)
            {
              hr |= dvbTuneRequest.put_SID(dvbChannel.ServiceId);
            }
          }
          hr |= dvbTuneRequest.put_Locator(locator);

          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("BDA PSK: potential error assembling tune request, hr = 0x{0:x}", hr);
          }

          return dvbTuneRequest;
        }
        catch
        {
          Release.ComObject("BDA PSK tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("BDA PSK tuner locator", ref locator);
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
      IChannelSatellite satelliteChannel = channel as IChannelSatellite;
      if (satelliteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }
      ChannelDvbS2 dvbs2Channel = channel as ChannelDvbS2;

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Multiplier = 1000,
        Frequency = (uint)satelliteChannel.Frequency,
        Bandwidth = uint.MaxValue,
        Polarity = GetBdaPolarisation(satelliteChannel.Polarisation),
        Range = uint.MaxValue
      };
      DigitalDemodulator2Settings demodulatorSettings = new DigitalDemodulator2Settings
      {
        InnerFECRate = GetBdaFecCodeRate(satelliteChannel.FecCodeRate),
        InnerFECMethod = FECMethod.MethodNotSet,
        Modulation = GetBdaModulation(satelliteChannel.ModulationScheme, dvbs2Channel != null),
        OuterFECMethod = FECMethod.MethodNotSet,
        OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
        SpectralInversion = SpectralInversion.NotSet,
        SymbolRate = (uint)satelliteChannel.SymbolRate,
        TransmissionMode = TransmissionMode.ModeNotSet
      };
      if (dvbs2Channel != null)
      {
        demodulatorSettings.Pilot = GetBdaPilot(dvbs2Channel.PilotTonesState);
        demodulatorSettings.RollOff = GetBdaRollOff(dvbs2Channel.RollOffFactor);
      }
      else
      {
        demodulatorSettings.Pilot = Pilot.NotSet;
        demodulatorSettings.RollOff = RollOff.NotSet;
      }

      LnbInfoSettings lnbSettings = new LnbInfoSettings
      {
        LnbSwitchFrequency = SatelliteLnbHandler.SWITCH_FREQUENCY,
        LowOscillator = SatelliteLnbHandler.LOW_BAND_LOF,
        HighOscillator = SatelliteLnbHandler.HIGH_BAND_LOF
      };
      DiseqcSatelliteSettings diseqcSettings = new DiseqcSatelliteSettings
      {
        ToneBurstEnabled = 0,
        Diseq10Selection = LNB_Source.NOT_SET,
        Diseq11Selection = DiseqC11Switches.Switch_NOT_SET,
        Enabled = 0
      };
      return networkProvider.TuneDVBS(frequencySettings, demodulatorSettings, lnbSettings, diseqcSettings);
    }

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    public override BroadcastStandard PossibleBroadcastStandards
    {
      get
      {
        return BroadcastStandard.DigiCipher2 | BroadcastStandard.DvbDsng | BroadcastStandard.DvbS | BroadcastStandard.DvbS2 | BroadcastStandard.SatelliteTurboFec;
      }
    }

    #region tuning parameter translation

    private static BdaPolarisation GetBdaPolarisation(TvePolarisation polarisation)
    {
      switch (polarisation)
      {
        case TvePolarisation.CircularLeft:
          return BdaPolarisation.CircularL;
        case TvePolarisation.CircularRight:
          return BdaPolarisation.CircularR;
        case TvePolarisation.LinearHorizontal:
          return BdaPolarisation.LinearH;
        case TvePolarisation.LinearVertical:
          return BdaPolarisation.LinearV;
        case TvePolarisation.Automatic:
          Log.Warn("BDA PSK: falling back to automatic polarisation");
          return BdaPolarisation.NotSet;
        default:
          return (BdaPolarisation)polarisation;
      }
    }

    private static ModulationType GetBdaModulation(ModulationSchemePsk modulation, bool isDvbS2)
    {
      switch (modulation)
      {
        case ModulationSchemePsk.Psk2:
        case ModulationSchemePsk.Psk4SplitI:
        case ModulationSchemePsk.Psk4SplitQ:
          return ModulationType.ModBpsk;
        case ModulationSchemePsk.Psk4:
          if (isDvbS2)
          {
            return ModulationType.ModQpsk;
          }
          return ModulationType.ModNotSet;
        case ModulationSchemePsk.Psk4Offset:
          return ModulationType.ModOqpsk;
        case ModulationSchemePsk.Psk8:
          return ModulationType.Mod8Psk;
        case ModulationSchemePsk.Psk16:
          return ModulationType.Mod16Apsk;
        case ModulationSchemePsk.Psk32:
          return ModulationType.Mod32Apsk;
        case ModulationSchemePsk.Psk64:
        case ModulationSchemePsk.Psk128:
        case ModulationSchemePsk.Psk256:
          Log.Warn("BDA PSK: unsupported modulation scheme {0}, falling back to automatic", modulation);
          return ModulationType.ModNotSet;
        case ModulationSchemePsk.Automatic:
          Log.Warn("BDA PSK: falling back to automatic modulation scheme");
          return ModulationType.ModNotSet;
        default:
          return (ModulationType)modulation;
      }
    }

    private static BinaryConvolutionCodeRate GetBdaFecCodeRate(FecCodeRate fecCodeRate)
    {
      switch (fecCodeRate)
      {
        case FecCodeRate.Rate1_2:
          return BinaryConvolutionCodeRate.Rate1_2;
        case FecCodeRate.Rate1_3:
          return BinaryConvolutionCodeRate.Rate1_3;
        case FecCodeRate.Rate1_4:
          return BinaryConvolutionCodeRate.Rate1_4;
        case FecCodeRate.Rate2_3:
          return BinaryConvolutionCodeRate.Rate2_3;
        case FecCodeRate.Rate2_5:
          return BinaryConvolutionCodeRate.Rate2_5;
        case FecCodeRate.Rate3_4:
          return BinaryConvolutionCodeRate.Rate3_4;
        case FecCodeRate.Rate3_5:
          return BinaryConvolutionCodeRate.Rate3_5;
        case FecCodeRate.Rate4_5:
          return BinaryConvolutionCodeRate.Rate4_5;
        case FecCodeRate.Rate5_6:
          return BinaryConvolutionCodeRate.Rate5_6;
        case FecCodeRate.Rate5_11:
          return BinaryConvolutionCodeRate.Rate5_11;
        case FecCodeRate.Rate6_7:
          return BinaryConvolutionCodeRate.Rate6_7;
        case FecCodeRate.Rate7_8:
          return BinaryConvolutionCodeRate.Rate7_8;
        case FecCodeRate.Rate8_9:
          return BinaryConvolutionCodeRate.Rate8_9;
        case FecCodeRate.Rate9_10:
          return BinaryConvolutionCodeRate.Rate9_10;
        case FecCodeRate.Rate1_5:
        case FecCodeRate.Rate4_15:
        case FecCodeRate.Rate5_9:
        case FecCodeRate.Rate7_15:
        case FecCodeRate.Rate8_15:
        case FecCodeRate.Rate9_20:
        case FecCodeRate.Rate11_15:
        case FecCodeRate.Rate11_20:
        case FecCodeRate.Rate11_45:
        case FecCodeRate.Rate13_18:
        case FecCodeRate.Rate13_45:
        case FecCodeRate.Rate14_45:
        case FecCodeRate.Rate23_36:
        case FecCodeRate.Rate25_36:
        case FecCodeRate.Rate26_45:
        case FecCodeRate.Rate28_45:
        case FecCodeRate.Rate29_45:
        case FecCodeRate.Rate31_45:
        case FecCodeRate.Rate32_45:
        case FecCodeRate.Rate77_90:
          Log.Warn("BDA PSK: unsupported FEC code rate {0}, falling back to automatic", fecCodeRate);
          return BinaryConvolutionCodeRate.RateNotSet;
        case FecCodeRate.Automatic:
          Log.Warn("BDA PSK: falling back to automatic FEC code rate");
          return BinaryConvolutionCodeRate.RateNotSet;
        default:
          return (BinaryConvolutionCodeRate)fecCodeRate;
      }
    }

    private static Pilot GetBdaPilot(PilotTonesState pilotTonesState)
    {
      switch (pilotTonesState)
      {
        case PilotTonesState.Off:
          return Pilot.Off;
        case PilotTonesState.On:
          return Pilot.On;
        case PilotTonesState.Automatic:
          Log.Warn("BDA PSK: falling back to automatic pilot tones state");
          return Pilot.NotSet;
        default:
          return (Pilot)pilotTonesState;
      }
    }

    private static RollOff GetBdaRollOff(RollOffFactor rollOffFactor)
    {
      switch (rollOffFactor)
      {
        case RollOffFactor.ThirtyFive:
          return RollOff.ThirtyFive;
        case RollOffFactor.TwentyFive:
          return RollOff.TwentyFive;
        case RollOffFactor.Twenty:
          return RollOff.Twenty;
        case RollOffFactor.Fifteen:
        case RollOffFactor.Ten:
        case RollOffFactor.Five:
          Log.Warn("BDA PSK: unsupported roll-off factor {0}, falling back to automatic", rollOffFactor);
          return RollOff.NotSet;
        case RollOffFactor.Automatic:
          Log.Warn("BDA PSK: falling back to automatic roll-off factor");
          return RollOff.NotSet;
        default:
          return (RollOff)rollOffFactor;
      }
    }

    #endregion

    #endregion
  }
}