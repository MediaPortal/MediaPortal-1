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
    #region constants

    private const string TUNING_SPACE_NAME_ISDB = "MediaPortal ISDB-C Tuning Space";
    private const string TUNING_SPACE_NAME_QAM = "MediaPortal QAM Tuning Space";

    #endregion

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
    /// Create a BDA tuning space for tuning a given channel type.
    /// </summary>
    /// <param name="channelType">The channel type.</param>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace(Type channelType)
    {
      this.LogDebug("BDA QAM: create tuning space, type = {0}", channelType.Name);

      string name = TUNING_SPACE_NAME_QAM;
      Guid networkType = NetworkType.DVB_CABLE;
      if (channelType == typeof(ChannelIsdbC))
      {
        name = TUNING_SPACE_NAME_ISDB;
        networkType = NetworkType.ISDB_CABLE;
      }

      IDVBTuningSpace2 tuningSpace = null;
      IDVBCLocator locator = null;
      try
      {
        tuningSpace = (IDVBTuningSpace2)new DVBTuningSpace();
        int hr = tuningSpace.put_FriendlyName(name);
        hr |= tuningSpace.put_NetworkID(-1);
        hr |= tuningSpace.put__NetworkType(networkType);
        hr |= tuningSpace.put_SystemType(DVBSystemType.Dvb_Cable);
        hr |= tuningSpace.put_UniqueName(name);

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
          this.LogWarn("BDA QAM: potential error creating tuning space, hr = 0x{0:x}, type = {1}", hr, channelType.Name);
        }
        return tuningSpace;
      }
      catch
      {
        Release.ComObject(string.Format("BDA QAM tuner {0} tuning space", channelType.Name), ref tuningSpace);
        Release.ComObject(string.Format("BDA QAM tuner {0} locator", channelType.Name), ref locator);
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
    /// Get the name(s) of the registered BDA tuning space(s) for the tuner type.
    /// </summary>
    protected override IDictionary<string, Type> TuningSpaceNames
    {
      get
      {
        Dictionary<string, Type> names = new Dictionary<string, Type>(2)
        {
          { TUNING_SPACE_NAME_QAM, null }
        };
        if (Environment.OSVersion.Version.Major >= 6)
        {
          names.Add(TUNING_SPACE_NAME_ISDB, typeof(ChannelIsdbT));
        }
        return names;
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
      IChannelQam qamChannel = channel as IChannelQam;
      if (qamChannel == null)
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
        hr = dvbcLocator.put_CarrierFrequency(qamChannel.Frequency);
        hr |= dvbcLocator.put_SymbolRate(qamChannel.SymbolRate);
        hr |= dvbcLocator.put_Modulation(GetBdaModulation(qamChannel.ModulationScheme));

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA QAM: potential error configuring locator, hr = 0x{0:x}", hr);
        }

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        TvExceptionDirectShowError.Throw(hr, "Failed to create tune request from tuning space.");

        IChannelDvbCompatible dvbCompatibleChannel = channel as IChannelDvbCompatible;
        if (dvbCompatibleChannel != null)
        {
          IDVBTuneRequest dvbTuneRequest = tuneRequest as IDVBTuneRequest;
          if (dvbTuneRequest == null)
          {
            this.LogWarn("BDA QAM: DVB tune request interface is not available on tune request");
          }
          else
          {
            if (dvbCompatibleChannel.OriginalNetworkId > 0)
            {
              hr |= dvbTuneRequest.put_ONID(dvbCompatibleChannel.OriginalNetworkId);
            }
            if (dvbCompatibleChannel.TransportStreamId > 0)
            {
              hr |= dvbTuneRequest.put_TSID(dvbCompatibleChannel.TransportStreamId);
            }
            if (dvbCompatibleChannel.ServiceId > 0)
            {
              hr |= dvbTuneRequest.put_SID(dvbCompatibleChannel.ServiceId);
            }
          }
        }
        hr |= tuneRequest.put_Locator(locator);

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA QAM: potential error assembling tune request, hr = 0x{0:x}", hr);
        }

        return tuneRequest;
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
      IChannelQam qamChannel = channel as IChannelQam;
      if (qamChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Multiplier = 1000,
        Frequency = (uint)qamChannel.Frequency,
        Bandwidth = uint.MaxValue,
        Polarity = DirectShowLib.BDA.Polarisation.NotSet,
        Range = uint.MaxValue
      };
      DigitalDemodulatorSettings demodulatorSettings = new DigitalDemodulatorSettings
      {
        InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
        InnerFECMethod = FECMethod.MethodNotSet,
        Modulation = GetBdaModulation(qamChannel.ModulationScheme),
        OuterFECMethod = FECMethod.MethodNotSet,
        OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
        SpectralInversion = SpectralInversion.NotSet,
        SymbolRate = (uint)qamChannel.SymbolRate
      };
      return networkProvider.TuneDVBC(frequencySettings, demodulatorSettings);
    }

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    public override BroadcastStandard PossibleBroadcastStandards
    {
      get
      {
        return BroadcastStandard.DvbC | BroadcastStandard.IsdbC;
      }
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