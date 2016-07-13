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
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channel;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="ITuner"/> for DVB-C and ISDB-C tuners
  /// with BDA drivers.
  /// </summary>
  internal class TunerBdaQam : TunerBdaBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaQam"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public TunerBdaQam(DsDevice device)
      : base(device, device.DevicePath + "QAM", BroadcastStandard.DvbC)
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
      this.LogDebug("BDA QAM: create tuning space");

      IDVBTuningSpace tuningSpace = null;
      IDVBCLocator locator = null;
      try
      {
        tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(NetworkType.DVB_CABLE);
        hr |= tuningSpace.put_SystemType(DVBSystemType.Cable);

        locator = (IDVBCLocator)new DVBCLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_Modulation(ModulationType.ModNotSet);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_SymbolRate(-1);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA QAM: potential error creating tuning space, hr = 0x{0:x}", hr);
        }
        return tuningSpace;
      }
      catch
      {
        Release.ComObject("BDA QAM tuner tuning space", ref tuningSpace);
        Release.ComObject("BDA QAM tuner locator", ref locator);
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
        return typeof(DVBCNetworkProvider).GUID;
      }
    }

    /// <summary>
    /// Get the registered name of the BDA tuning space for the tuner type.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal QAM Tuning Space";
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
      ChannelDvbC dvbcChannel = channel as ChannelDvbC;
      if (dvbcChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ILocator locator;
      int hr = tuningSpace.get_DefaultLocator(out locator);
      TvExceptionDirectShowError.Throw(hr, "Failed to get the default locator for the tuning space.");
      try
      {
        IDVBCLocator dvbcLocator = locator as IDVBCLocator;
        if (dvbcLocator == null)
        {
          throw new TvException("Failed to find DVB-C locator interface on locator.");
        }
        hr = dvbcLocator.put_CarrierFrequency((int)dvbcChannel.Frequency);
        hr |= dvbcLocator.put_SymbolRate(dvbcChannel.SymbolRate);
        hr |= dvbcLocator.put_Modulation(GetBdaModulation(dvbcChannel.ModulationScheme));

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

          if (dvbcChannel.OriginalNetworkId > 0)
          {
            hr |= dvbTuneRequest.put_ONID(dvbcChannel.OriginalNetworkId);
          }
          if (dvbcChannel.TransportStreamId > 0)
          {
            hr |= dvbTuneRequest.put_TSID(dvbcChannel.TransportStreamId);
          }
          if (dvbcChannel.ServiceId > 0)
          {
            hr |= dvbTuneRequest.put_SID(dvbcChannel.ServiceId);
          }

          hr |= dvbTuneRequest.put_Locator(locator);

          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("BDA QAM: potential error assembling tune request, hr = 0x{0:x}", hr);
          }

          return dvbTuneRequest;
        }
        catch
        {
          Release.ComObject("BDA QAM tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("BDA QAM tuner locator", ref locator);
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
      ChannelDvbC dvbcChannel = channel as ChannelDvbC;
      if (dvbcChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Multiplier = 1000,
        Frequency = (uint)(dvbcChannel.Frequency),
        Bandwidth = uint.MaxValue,
        Polarity = DirectShowLib.BDA.Polarisation.NotSet,
        Range = uint.MaxValue
      };
      DigitalDemodulatorSettings demodulatorSettings = new DigitalDemodulatorSettings
      {
        InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
        InnerFECMethod = FECMethod.MethodNotSet,
        Modulation = GetBdaModulation(dvbcChannel.ModulationScheme),
        OuterFECMethod = FECMethod.MethodNotSet,
        OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
        SpectralInversion = SpectralInversion.NotSet,
        SymbolRate = (uint)dvbcChannel.SymbolRate
      };
      return networkProvider.TuneDVBC(frequencySettings, demodulatorSettings);
    }

    #region tuning parameter translation

    private static ModulationType GetBdaModulation(ModulationSchemeQam modulation)
    {
      switch (modulation)
      {
        case ModulationSchemeQam.Qam16:
          return ModulationType.Mod16Qam;
        case ModulationSchemeQam.Qam32:
          return ModulationType.Mod32Qam;
        case ModulationSchemeQam.Qam64:
          return ModulationType.Mod64Qam;
        case ModulationSchemeQam.Qam128:
          return ModulationType.Mod128Qam;
        case ModulationSchemeQam.Qam256:
          return ModulationType.Mod256Qam;
        case ModulationSchemeQam.Qam512:
          return ModulationType.Mod512Qam;
        case ModulationSchemeQam.Qam1024:
          return ModulationType.Mod1024Qam;
        case ModulationSchemeQam.Qam2048:
        case ModulationSchemeQam.Qam4096:
          Log.Warn("BDA QAM: unsupported modulation scheme {0}, falling back to automatic", modulation);
          return ModulationType.ModNotSet;
        case ModulationSchemeQam.Automatic:
          Log.Warn("BDA QAM: falling back to automatic modulation scheme");
          return ModulationType.ModNotSet;
        default:
          return (ModulationType)modulation;
      }
    }

    #endregion

    #endregion
  }
}