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
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Channels;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.DVB.Graphs.ATSC
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles ATSC terrestrial and cable tuners with BDA drivers.
  /// </summary>
  public class TvCardATSC : TvCardDvbBase
  {
    #region constructor

    /// <summary>
    /// Initialise a new instance of the <see cref="TvCardATSC"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface for the instance to use.</param>
    /// <param name="device">The <see cref="DsDevice"/> instance that the instance will encapsulate.</param>
    public TvCardATSC(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _tunerType = CardType.Atsc;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Create and register a BDA tuning space for the device type.
    /// </summary>
    /// <returns>the tuning space that was created</returns>
    protected override ITuningSpace CreateTuningSpace()
    {
      this.LogDebug("TvCardAtsc: create tuning space");

      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      IATSCTuningSpace tuningSpace = null;
      IATSCLocator locator = null;
      try
      {
        ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
        if (container == null)
        {
          throw new TvException("Failed to get ITuningSpaceContainer handle from SystemTuningSpaces instance.");
        }

        tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();
        int hr = tuningSpace.put_UniqueName(TuningSpaceName);
        hr |= tuningSpace.put_FriendlyName(TuningSpaceName);
        hr |= tuningSpace.put__NetworkType(typeof(ATSCNetworkProvider).GUID);
        hr |= tuningSpace.put_CountryCode(0);
        hr |= tuningSpace.put_InputType(TunerInputType.Antenna);
        hr |= tuningSpace.put_MaxMinorChannel(999);     // the number of minor channels per major channel
        hr |= tuningSpace.put_MaxPhysicalChannel(158);  // 69 for terrestrial, 158 for cable
        hr |= tuningSpace.put_MaxChannel(99);           // the number of scannable major channels
        hr |= tuningSpace.put_MinMinorChannel(0);
        hr |= tuningSpace.put_MinPhysicalChannel(1);    // 1 for terrestrial, 2 for cable
        hr |= tuningSpace.put_MinChannel(1);

        locator = (IATSCLocator)new ATSCLocator();
        hr |= locator.put_CarrierFrequency(-1);
        hr |= locator.put_PhysicalChannel(-1);
        hr |= locator.put_SymbolRate(-1);
        hr |= locator.put_Modulation(ModulationType.Mod8Vsb); // 8 VSB is terrestrial, 64 or 256 QAM is cable
        hr |= locator.put_InnerFEC(FECMethod.MethodNotSet);
        hr |= locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_OuterFEC(FECMethod.MethodNotSet);
        hr |= locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        hr |= locator.put_TSID(-1);

        hr |= tuningSpace.put_DefaultLocator(locator);
        if (hr != (int)HResult.Severity.Success)
        {
          this.LogWarn("TvCardAtsc: potential error in CreateTuningSpace(), hr = 0x{0:X}", hr);
        }

        object index;
        hr = container.Add(tuningSpace, out index);
        HResult.ThrowException(hr, "Failed to Add() on ITuningSpaceContainer.");
        return tuningSpace;
      }
      catch (Exception)
      {
        Release.ComObject("ATSC tuner tuning space", ref tuningSpace);
        Release.ComObject("ATSC tuner locator", ref locator);
        throw;
      }
      finally
      {
        Release.ComObject("ATSC tuner tuning space container", ref systemTuningSpaces);
      }
    }

    /// <summary>
    /// The registered name of the BDA tuning space for the device type.
    /// </summary>
    protected override string TuningSpaceName
    {
      get
      {
        return "MediaPortal ATSC Tuning Space";
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
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        throw new TvException("Received request to tune incompatible channel.");
      }

      ILocator locator;
      int hr = tuningSpace.get_DefaultLocator(out locator);
      HResult.ThrowException(hr, "Failed to get_DefaultLocator() on ITuningSpace.");
      try
      {
        IATSCLocator atscLocator = locator as IATSCLocator;
        if (atscLocator == null)
        {
          throw new TvException("Failed to get IATSCLocator handle from ILocator.");
        }
        hr = atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannel);
        hr |= atscLocator.put_CarrierFrequency((int)atscChannel.Frequency);
        hr |= atscLocator.put_Modulation(atscChannel.ModulationType);
        hr |= atscLocator.put_TSID(atscChannel.TransportId);

        ITuneRequest tuneRequest;
        hr = tuningSpace.CreateTuneRequest(out tuneRequest);
        HResult.ThrowException(hr, "Failed to CreateTuneRequest() on ITuningSpace.");
        try
        {
          IATSCChannelTuneRequest atscTuneRequest = tuneRequest as IATSCChannelTuneRequest;
          if (atscTuneRequest == null)
          {
            throw new TvException("Failed to get IATSCChannelTuneRequest handle from ITuneRequest.");
          }
          hr |= atscTuneRequest.put_Channel(atscChannel.MajorChannel);
          hr |= atscTuneRequest.put_MinorChannel(atscChannel.MinorChannel);
          hr |= atscTuneRequest.put_Locator(locator);

          if (hr != (int)HResult.Severity.Success)
          {
            this.LogWarn("TvCardAtsc: potential error in AssembleTuneRequest(), hr = 0x{0:X}", hr);
          }

          return atscTuneRequest;
        }
        catch (Exception)
        {
          Release.ComObject("ATSC tuner tune request", ref tuneRequest);
          throw;
        }
      }
      finally
      {
        Release.ComObject("ATSC tuner locator", ref locator);
      }
    }

    /// <summary>
    /// Get the device's channel scanning interface.
    /// </summary>
    public override ITVScanning ScanningInterface
    {
      get
      {
        return new ATSCScanning(this, _filterTsWriter as ITsChannelScan);
      }
    }

    /// <summary>
    /// Check if the device can tune to a specific channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the device can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      return channel is ATSCChannel;
    }

    #endregion

    // TODO: remove this method, it should not be required and it is bad style!
    protected override DVBBaseChannel CreateChannel()
    {
      return new ATSCChannel();
    }
  }
}