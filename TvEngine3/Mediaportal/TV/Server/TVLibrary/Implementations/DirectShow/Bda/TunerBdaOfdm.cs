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
  /// An implementation of <see cref="ITuner"/> for DVB-T, DVB-T2 and ISDB-T
  /// tuners with BDA drivers.
  /// </summary>
  internal class TunerBdaOfdm : TunerBdaBase
  {
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
    /// Create and register a BDA tuning space for the tuner type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("BDA OFDM: create tuning space");

      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      IDVBTuningSpace tuningSpace = null;
      IDVBTLocator locator = null;
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to find tuning space container interface on system tuning spaces instance.");
        }

        tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(NetworkType.DVB_TERRESTRIAL);
        hr |= tuningSpace.put_SystemType(DVBSystemType.Terrestrial);

        locator = (IDVBTLocator)new DVBTLocator();
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

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)NativeMethods.HResult.S_OK)
        {
          this.LogWarn("BDA OFDM: potential error creating tuning space, hr = 0x{0:x}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        TvExceptionDirectShowError.Throw(hr, "Failed to add new tuning space to tuning space container.");
        return tuningSpace;
      }
      catch
      {
        Release.ComObject("BDA OFDM tuner tuning space", ref tuningSpace);
        Release.ComObject("BDA OFDM tuner locator", ref locator);
        throw;
      }
      finally
      {
        Release.ComObject("BDA OFDM tuner tuning space container", ref systemTuningSpaces);
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
    /// Get the registered name of the BDA tuning space for the tuner type.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal OFDM Tuning Space";
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
            hr |= dvbTuneRequest.put_ONID(dvbChannel.OriginalNetworkId);
            hr |= dvbTuneRequest.put_TSID(dvbChannel.TransportStreamId);
            hr |= dvbTuneRequest.put_SID(dvbChannel.ServiceId);
          }
          hr |= dvbTuneRequest.put_Locator(locator);

          if (hr != (int)NativeMethods.HResult.S_OK)
          {
            this.LogWarn("BDA OFDM: potential error assembling tune request, hr = 0x{0:x}", hr);
          }

          return dvbTuneRequest;
        }
        catch
        {
          Release.ComObject("BDA OFDM tuner tune request", ref tuneRequest);
          throw;
        }
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
        return BroadcastStandard.DvbT | BroadcastStandard.DvbT2;
      }
    }

    #endregion
  }
}