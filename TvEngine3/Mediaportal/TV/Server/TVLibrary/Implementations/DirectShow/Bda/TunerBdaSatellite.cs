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
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.TVLibrary.Implementations.Helper;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Diseqc;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles satellite
  /// tuners with BDA drivers.
  /// </summary>
  public class TunerBdaSatellite : TunerBdaBase
  {
    #region variables

    /// <summary>
    /// The DiSEqC control interface for the tuner.
    /// </summary>
    private IDiseqcController _diseqcController = null;

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaSatellite"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public TunerBdaSatellite(DsDevice device)
      : base(device, device.DevicePath + "S")
    {
      _tunerType = CardType.DvbS;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Actually load the tuner.
    /// </summary>
    protected override void PerformLoading()
    {
      this.LogDebug("BDA satellite: perform loading");
      base.PerformLoading();

      // Check if one of the supported extensions is capable of sending DiSEqC commands.
      foreach (ICustomDevice extensionInterface in _customDeviceInterfaces)
      {
        IDiseqcDevice diseqcDevice = extensionInterface as IDiseqcDevice;
        if (diseqcDevice != null)
        {
          this.LogDebug("BDA satellite: found DiSEqC command interface");
          _diseqcController = new DiseqcController(diseqcDevice);
          _diseqcController.ReloadConfiguration(_cardId);
          break;
        }
      }
    }

    /// <summary>
    /// Create and register a BDA tuning space for the tuner type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("BDA satellite: create tuning space");

      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      IDVBSTuningSpace tuningSpace = null;
      IDVBSLocator locator = null;
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to find tuning space container interface on system tuning spaces instance.");
        }

        tuningSpace = (IDVBSTuningSpace)new DVBSTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(NetworkType.DVB_SATELLITE);
        hr |= tuningSpace.put_SystemType(DVBSystemType.Satellite);
        hr |= tuningSpace.put_SpectralInversion(SpectralInversion.Automatic);
        hr |= tuningSpace.put_LowOscillator(9750000);
        hr |= tuningSpace.put_HighOscillator(10600000);
        hr |= tuningSpace.put_LNBSwitch(11700000);

        locator = (IDVBSLocator)new DVBSLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_Modulation(ModulationType.ModNotSet);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("BDA satellite: potential error creating tuning space, hr = 0x{0:x}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        HResult.ThrowException(hr, "Failed to add new tuning space to tuning space container.");
        return tuningSpace;
      }
      catch (Exception)
      {
        Release.ComObject("BDA satellite tuner tuning space", ref tuningSpace);
        Release.ComObject("BDA satellite tuner locator", ref locator);
        throw;
      }
      finally
      {
        Release.ComObject("BDA satellite tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// Get the class ID of the network provider for the tuner type.
    /// </summary>
    protected abstract Guid NetworkProviderClsid
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
        return "MediaPortal Satellite Tuning Space";
      }
    }

    #endregion

    #region tuning

    /// <summary>
    /// Actually tune to a channel.
    /// </summary>
    /// <param name="channel">The channel to tune to.</param>
    protected override void PerformTuning(IChannel channel)
    {
      // Send DiSEqC commands (if necessary) before actually tuning in case the driver applies the commands
      // during the tuning process.
      if (_diseqcController != null)
      {
        _diseqcController.SwitchToChannel(channel as DVBSChannel);
      }
      base.PerformTuning(channel);
    }

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="tuningSpace">The tuner's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected override ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel)
    {
      DVBSChannel satelliteChannel = channel as DVBSChannel;
      if (satelliteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      IDVBSTuningSpace dvbsTuningSpace = tuningSpace as IDVBSTuningSpace;
      if (dvbsTuningSpace == null)
      {
        throw new TvException("Failed to get IDVBSTuningSpace handle from ITuningSpace.");
      }
      int hr = dvbsTuningSpace.put_LowOscillator(satelliteChannel.LnbType.LowBandFrequency);
      hr |= dvbsTuningSpace.put_HighOscillator(satelliteChannel.LnbType.HighBandFrequency);
      hr |= dvbsTuningSpace.put_LNBSwitch(satelliteChannel.LnbType.SwitchFrequency);

      ILocator locator;
      hr |= tuningSpace.get_DefaultLocator(out locator);
      HResult.ThrowException(hr, "Failed to get the default locator for the tuning space.");
      try
      {
        IDVBSLocator dvbsLocator = locator as IDVBSLocator;
        if (dvbsLocator == null)
        {
          throw new TvException("Failed to find DVB-S locator interface on locator.");
        }
        hr = dvbsLocator.put_CarrierFrequency((int)satelliteChannel.Frequency);
        hr |= dvbsLocator.put_SymbolRate(satelliteChannel.SymbolRate);
        hr |= dvbsLocator.put_SignalPolarisation(satelliteChannel.Polarisation);
        hr |= dvbsLocator.put_Modulation(satelliteChannel.ModulationType);
        hr |= dvbsLocator.put_InnerFECRate(satelliteChannel.InnerFecRate);

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        HResult.ThrowException(hr, "Failed to create tuning request from tuning space.");
        try
        {
          IDVBTuneRequest dvbTuneRequest = tuneRequest as IDVBTuneRequest;
          if (dvbTuneRequest == null)
          {
            throw new TvException("Failed to find DVB tune request interface on tune request.");
          }
          hr |= dvbTuneRequest.put_ONID(satelliteChannel.NetworkId);
          hr |= dvbTuneRequest.put_TSID(satelliteChannel.TransportId);
          hr |= dvbTuneRequest.put_SID(satelliteChannel.ServiceId);
          hr |= dvbTuneRequest.put_Locator(locator);

          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("BDA satellite: potential error assembling tune request, hr = 0x{0:x}", hr);
          }

          return dvbTuneRequest;
        }
        catch (Exception)
        {
          Release.ComObject("BDA satellite tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("BDA satellite tuner locator", ref locator);
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
      DVBSChannel satelliteChannel = channel as DVBSChannel;
      if (satelliteChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Multiplier = 1000,
        Frequency = (uint)(satelliteChannel.Frequency),
        Bandwidth = uint.MaxValue,
        Polarity = satelliteChannel.Polarisation,
        Range = uint.MaxValue
      };
      DigitalDemodulator2Settings demodulatorSettings = new DigitalDemodulator2Settings
      {
        InnerFECRate = satelliteChannel.InnerFecRate,
        InnerFECMethod = FECMethod.MethodNotSet,
        Modulation = satelliteChannel.ModulationType,
        OuterFECMethod = FECMethod.MethodNotSet,
        OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
        Pilot = Pilot.NotSet,
        RollOff = RollOff.NotSet,
        SpectralInversion = SpectralInversion.NotSet,
        SymbolRate = (uint)satelliteChannel.SymbolRate,
        TransmissionMode = TransmissionMode.ModeNotSet
      };
      LnbInfoSettings lnbSettings = new LnbInfoSettings
      {
        LnbSwitchFrequency = (uint)satelliteChannel.LnbType.SwitchFrequency,
        LowOscillator = (uint)satelliteChannel.LnbType.LowBandFrequency,
        HighOscillator = (uint)satelliteChannel.LnbType.HighBandFrequency
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
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBSChannel;
    }

    #endregion

    /// <summary>
    /// Reload the tuner's configuration.
    /// </summary>
    public override void ReloadConfiguration()
    {
      base.ReloadConfiguration();
      if (_diseqcController != null)
      {
        _diseqcController.ReloadConfiguration(_cardId);
      }
    }

    /// <summary>
    /// Get the tuner's DiSEqC control interface. This interface is only applicable for satellite tuners.
    /// It is used for controlling switch, positioner and LNB settings.
    /// </summary>
    /// <value><c>null</c> if the tuner is not a satellite tuner or the tuner does not support sending/receiving
    /// DiSEqC commands</value>
    public override IDiseqcController DiseqcController
    {
      get
      {
        return _diseqcController;
      }
    }

    /// <summary>
    /// Stop the tuner. The actual result of this function depends on tuner configuration.
    /// </summary>
    public override void Stop()
    {
      base.Stop();
      // Force the DiSEqC controller to forget the previously tuned channel. This guarantees that the
      // next call to SwitchToChannel() will actually cause commands to be sent.
      if (_diseqcController != null)
      {
        _diseqcController.SwitchToChannel(null);
      }
    }

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBSChannel();
    }
  }
}