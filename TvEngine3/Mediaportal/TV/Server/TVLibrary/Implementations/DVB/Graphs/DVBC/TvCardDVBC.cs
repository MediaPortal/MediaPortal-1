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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs.DVBC
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-C tuners with BDA drivers.
  /// </summary>
  public class TvCardDVBC : TvCardDvbBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TvCardDVBC"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface for the instance to use.</param>
    /// <param name="device">The <see cref="DsDevice"/> instance that the instance will encapsulate.</param>
    public TvCardDVBC(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _tunerType = CardType.DvbC;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Create and register the BDA tuning space for the device.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("TvCardDvbC: CreateTuningSpace()");

      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      IDVBTuningSpace tuningSpace = null;
      IDVBCLocator locator = null;
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to get ITuningSpaceContainer handle from SystemTuningSpaces instance.");
        }

        tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(typeof(DVBCNetworkProvider).GUID);
        hr |= tuningSpace.put_SystemType(DVBSystemType.Cable);

        locator = (IDVBCLocator)new DVBCLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_Modulation(ModulationType.ModNotSet);
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != 0)
        {
          this.LogWarn("TvCardDvbC: potential error in CreateTuningSpace(), hr = 0x{0:X}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        HResult.ThrowException(hr, "Failed to Add() on ITuningSpaceContainer.");
        return tuningSpace;
      }
      catch (Exception)
      {
        Release.ComObject("cable tuner tuning space", ref tuningSpace);
        Release.ComObject("cable tuner locator", ref locator);
        throw;
      }
      finally
      {
        Release.ComObject("cable tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// The registered name of BDA tuning space for the device.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal Cable Tuning Space";
      }
    }

    #endregion

    #region tuning & scanning

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="tuningSpace">The device's tuning space.</param>
    /// <param name="channel">The channel to translate into a tune request.</param>
    /// <returns>a tune request instance</returns>
    protected override ITuneRequest AssembleTuneRequest(ITuningSpace tuningSpace, IChannel channel)
    {
      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ILocator locator;
      int hr = tuningSpace.get_DefaultLocator(out locator);
      HResult.ThrowException(hr, "Failed to get_DefaultLocator() on ITuningSpace.");
      try
      {
        IDVBCLocator dvbcLocator = locator as IDVBCLocator;
        if (dvbcLocator == null)
        {
          throw new TvException("Failed to get IDVBCLocator handle from ILocator.");
        }
        hr = dvbcLocator.put_CarrierFrequency((int)dvbcChannel.Frequency);
        hr |= dvbcLocator.put_SymbolRate(dvbcChannel.SymbolRate);
        hr |= dvbcLocator.put_Modulation(dvbcChannel.ModulationType);

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        HResult.ThrowException(hr, "Failed to CreateTuneRequest() on ITuningSpace.");
        try
        {
          IDVBTuneRequest dvbTuneRequest = tuneRequest as IDVBTuneRequest;
          if (dvbTuneRequest == null)
          {
            throw new TvException("Failed to get IDVBTuneRequest handle from ITuneRequest.");
          }
          hr |= dvbTuneRequest.put_ONID(dvbcChannel.NetworkId);
          hr |= dvbTuneRequest.put_TSID(dvbcChannel.TransportId);
          hr |= dvbTuneRequest.put_SID(dvbcChannel.ServiceId);
          hr |= dvbTuneRequest.put_Locator(locator);

          if (hr != 0)
          {
            this.LogWarn("TvCardDvbC: potential error in AssembleTuneRequest(), hr = 0x{0:X}", hr);
          }

          return dvbTuneRequest;
        }
        catch (Exception)
        {
          Release.ComObject("cable tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("cable tuner locator", ref locator);
      }
    }

    /// <summary>
    /// Check if the device can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is DVBCChannel;
    }

    #endregion

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new DVBCChannel();
    }
  }
}