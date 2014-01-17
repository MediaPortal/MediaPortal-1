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
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.NetworkProvider;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DirectShow.Bda
{
  /// <summary>
  /// An implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-T, DVB-T2
  /// and ISDB-T tuners with BDA drivers.
  /// </summary>
  public class TunerBdaTerrestrial : TunerBdaBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TunerBdaTerrestrial"/> class.
    /// </summary>
    /// <param name="device">The <see cref="DsDevice"/> instance to encapsulate.</param>
    public TunerBdaTerrestrial(DsDevice device)
      : base(device, device.DevicePath + "T")
    {
      _tunerType = CardType.DvbT;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Create and register a BDA tuning space for the tuner type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("BDA terrestrial: create tuning space");

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
          this.LogWarn("BDA terrestrial: potential error creating tuning space, hr = 0x{0:x}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        HResult.ThrowException(hr, "Failed to add new tuning space to tuning space container.");
        return tuningSpace;
      }
      catch (Exception)
      {
        Release.ComObject("BDA terrestrial tuner tuning space", ref tuningSpace);
        Release.ComObject("BDA terrestrial tuner locator", ref locator);
        throw;
      }
      finally
      {
        Release.ComObject("BDA terrestrial tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// Get the class ID of the network provider for the tuner type.
    /// </summary>
    protected abstract Guid NetworkProviderClsid
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
        return "MediaPortal Terrestrial Tuning Space";
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
      DVBTChannel terrestrialChannel = channel as DVBTChannel;
      if (terrestrialChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ILocator locator;
      int hr = tuningSpace.get_DefaultLocator(out locator);
      HResult.ThrowException(hr, "Failed to get the default locator for the tuning space.");
      try
      {
        IDVBTLocator dvbtLocator = locator as IDVBTLocator;
        if (dvbtLocator == null)
        {
          throw new TvException("Failed to find DVB-T locator interface on locator.");
        }
        hr = dvbtLocator.put_CarrierFrequency((int)terrestrialChannel.Frequency);
        hr |= dvbtLocator.put_Bandwidth(terrestrialChannel.Bandwidth / 1000);

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
          hr |= dvbTuneRequest.put_ONID(terrestrialChannel.NetworkId);
          hr |= dvbTuneRequest.put_TSID(terrestrialChannel.TransportId);
          hr |= dvbTuneRequest.put_SID(terrestrialChannel.ServiceId);
          hr |= dvbTuneRequest.put_Locator(locator);

          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("BDA terrestrial: potential error assembling tune request, hr = 0x{0:x}", hr);
          }

          return dvbTuneRequest;
        }
        catch (Exception)
        {
          Release.ComObject("BDA terrestrial tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("BDA terrestrial tuner locator", ref locator);
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
      DVBTChannel terrestrialChannel = channel as DVBTChannel;
      if (terrestrialChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      FrequencySettings frequencySettings = new FrequencySettings
      {
        Multiplier = 1000,
        Frequency = (uint)(terrestrialChannel.Frequency),
        Bandwidth = (uint)terrestrialChannel.Bandwidth,
        Polarity = Polarisation.NotSet,
        Range = uint.MaxValue
      };
      return networkProvider.TuneDVBT(frequencySettings);
    }

    /// <summary>
    /// Check if the tuner can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBTChannel;
    }

    #endregion

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBTChannel();
    }
  }
}