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
  /// An implementation of <see cref="ITuner"/> for DVB-T, DVB-T2 and ISDB-T
  /// tuners with BDA drivers.
  /// </summary>
  internal class TunerBdaOfdm : TunerBdaBase
  {
    #region constants

    private const string TUNING_SPACE_NAME_ISDB = "MediaPortal ISDB-T/SBTVD Tuning Space";
    private const string TUNING_SPACE_NAME_OFDM = "MediaPortal OFDM Tuning Space";

    #endregion

    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaOfdm"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    /// <param name="supportedBroadcastStandards">The broadcast standards supported by the hardware.</param>
    public TunerBdaOfdm(DsDevice device, BroadcastStandard supportedBroadcastStandards)
      : base(device, device.DevicePath + "OFDM", supportedBroadcastStandards)
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
      this.LogDebug("BDA OFDM: create tuning space, type = {0}", channelType.Name);

      IDVBTLocator locator = null;
      string name = TUNING_SPACE_NAME_OFDM;
      Guid networkType = NetworkType.DVB_TERRESTRIAL;
      DVBSystemType systemType = DVBSystemType.Dvb_Terrestrial;
      if (channelType == typeof(ChannelIsdbT))
      {
        // ISDB support is a bit of a murky subject. Vista has the network
        // type; 7 has the modulation type, system type *and* a new network
        // type.
        name = TUNING_SPACE_NAME_ISDB;
        networkType = NetworkType.ISDB_TERRESTRIAL;
        if (
          (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1) ||
          Environment.OSVersion.Version.Major > 6
        )
        {
          networkType = NetworkType.ISDB_T;
          systemType = DVBSystemType.Isdb_Terrestrial;
        }
      }
      else if (
        (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1) ||
        Environment.OSVersion.Version.Major > 6
      )
      {
        locator = (IDVBTLocator)new DVBTLocator2();
      }

      IDVBTuningSpace2 tuningSpace = null;
      try
      {
        tuningSpace = (IDVBTuningSpace2)new DVBTuningSpace();
        int hr = tuningSpace.put_FriendlyName(name);
        hr |= tuningSpace.put_NetworkID(-1);
        hr |= tuningSpace.put__NetworkType(networkType);
        hr |= tuningSpace.put_SystemType(systemType);
        hr |= tuningSpace.put_UniqueName(name);

        if (locator == null)
        {
          locator = (IDVBTLocator)new DVBTLocator();
        }
        hr |= locator.put_Bandwidth(-1);
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_Guard(GuardInterval.GuardNotSet);
        hr |= locator.put_HAlpha(HierarchyAlpha.HAlphaNotSet);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_LPInnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_LPInnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_Mode(TransmissionMode.ModeNotSet);
        hr |= locator.put_Modulation(ModulationType.ModNotSet);
        hr |= locator.put_OtherFrequencyInUse(false);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_SymbolRate(-1);
        IDVBTLocator2 locator2 = locator as IDVBTLocator2;
        if (locator2 != null)
        {
          locator2.put_PhysicalLayerPipeId(-1);
        }

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA OFDM: potential error creating tuning space, hr = 0x{0:x}, type = {1}", hr, channelType.Name);
        }
        return tuningSpace;
      }
      catch
      {
        Release.ComObject(string.Format("BDA OFDM tuner {0} tuning space", channelType.Name), ref tuningSpace);
        Release.ComObject(string.Format("BDA OFDM tuner {0} locator", channelType.Name), ref locator);
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
        Dictionary<string, Type> names = new Dictionary<string, Type>(2)
        {
          { TUNING_SPACE_NAME_OFDM, null }
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
      IChannelOfdm ofdmChannel = channel as IChannelOfdm;
      if (ofdmChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ILocator locator;
      int hr = tuningSpace.get_DefaultLocator(out locator);
      TvExceptionDirectShowError.Throw(hr, "Failed to get the default locator for the tuning space.");
      try
      {
        IDVBTLocator dvbtLocator = locator as IDVBTLocator;
        if (dvbtLocator == null)
        {
          throw new TvException("Failed to find DVB-T locator interface on locator.");
        }
        hr = dvbtLocator.put_CarrierFrequency(ofdmChannel.Frequency);
        hr |= dvbtLocator.put_Bandwidth(ofdmChannel.Bandwidth / 1000);

        ChannelDvbT2 dvbt2Channel = channel as ChannelDvbT2;
        if (dvbt2Channel != null)
        {
          hr |= dvbtLocator.put_InnerFEC(FECMethod.Ldpc);
          hr |= dvbtLocator.put_OuterFEC(FECMethod.Bch);
        }
        else
        {
          hr |= dvbtLocator.put_InnerFEC(FECMethod.Viterbi);
          hr |= dvbtLocator.put_OuterFEC(FECMethod.RS204_188);
        }

        if (
          channel is ChannelIsdbT &&
          (
            (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 1) ||
            Environment.OSVersion.Version.Major > 6
          )
        )
        {
          hr |= dvbtLocator.put_Modulation(ModulationType.ModIsdbtTmcc);
        }
        else
        {
          hr |= dvbtLocator.put_Modulation(ModulationType.ModNotSet);
        }

        IDVBTLocator2 dvbt2Locator = locator as IDVBTLocator2;
        if (dvbt2Locator != null)
        {
          if (dvbt2Channel != null)
          {
            hr |= dvbt2Locator.put_PhysicalLayerPipeId(dvbt2Channel.PlpId);
          }
          else
          {
            hr |= dvbt2Locator.put_PhysicalLayerPipeId(-1);
          }
        }

        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA OFDM: potential error configuring locator, hr = 0x{0:x}", hr);
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
            this.LogWarn("BDA OFDM: DVB tune request interface is not available on tune request");
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
          this.LogWarn("BDA OFDM: potential error assembling tune request, hr = 0x{0:x}", hr);
        }

        return tuneRequest;
      }
      finally
      {
        Release.ComObject("BDA OFDM tuner locator", ref locator);
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
      IChannelOfdm ofdmChannel = channel as IChannelOfdm;
      if (ofdmChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Frequency = (uint)ofdmChannel.Frequency,
        Multiplier = 1000,
        Bandwidth = (uint)ofdmChannel.Bandwidth,
        Polarity = DirectShowLib.BDA.Polarisation.NotSet,
        Range = uint.MaxValue
      };
      return networkProvider.TuneDVBT(frequencySettings);
    }

    /// <summary>
    /// Get the broadcast standards supported by the tuner code/class/type implementation.
    /// </summary>
    public override BroadcastStandard PossibleBroadcastStandards
    {
      get
      {
        return BroadcastStandard.DvbT | BroadcastStandard.DvbT2 | BroadcastStandard.IsdbT;
      }
    }

    #endregion
  }
}