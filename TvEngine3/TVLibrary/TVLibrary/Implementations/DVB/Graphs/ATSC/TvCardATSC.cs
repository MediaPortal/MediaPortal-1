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
using TvDatabase;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Device;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles ATSC BDA cards
  /// </summary>
  public class TvCardATSC : TvCardDvbBase, IDisposable, ITVCard
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardATSC"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardATSC(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _cardType = CardType.Atsc;
    }

    #region graphbuilding

    /// <summary>
    /// Create the BDA tuning space for the tuner. This will be used for BDA tuning.
    /// </summary>
    protected override void CreateTuningSpace()
    {
      Log.Log.WriteFile("atsc:CreateTuningSpace()");
      ITuner tuner = (ITuner)_filterNetworkProvider;
      SystemTuningSpaces systemTuningSpaces = new SystemTuningSpaces();
      ITuningSpaceContainer container = systemTuningSpaces as ITuningSpaceContainer;
      if (container == null)
      {
        Log.Log.Error("CreateTuningSpace() Failed to get ITuningSpaceContainer");
        return;
      }
      IEnumTuningSpaces enumTuning;
      ITuningSpace[] spaces = new ITuningSpace[2];
      ITuneRequest request;
      container.get_EnumTuningSpaces(out enumTuning);
      while (true)
      {
        int fetched;
        enumTuning.Next(1, spaces, out fetched);
        if (fetched != 1)
          break;
        string name;
        spaces[0].get_UniqueName(out name);
        if (name == "MediaPortal ATSC TuningSpace")
        {
          Log.Log.WriteFile("atsc:found correct tuningspace {0}", name);
          _tuningSpace = (IATSCTuningSpace)spaces[0];
          tuner.put_TuningSpace(_tuningSpace);
          _tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IATSCChannelTuneRequest)request;
          return;
        }
        Release.ComObject("ITuningSpace", spaces[0]);
      }
      Release.ComObject("IEnumTuningSpaces", enumTuning);
      Log.Log.WriteFile("atsc:Create new tuningspace");
      _tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();
      IATSCTuningSpace tuningSpace = (IATSCTuningSpace)_tuningSpace;

      tuningSpace.put_UniqueName("MediaPortal ATSC TuningSpace");
      tuningSpace.put_FriendlyName("MediaPortal ATSC TuningSpace");
      tuningSpace.put__NetworkType(typeof (ATSCNetworkProvider).GUID);
      tuningSpace.put_CountryCode(0);
      tuningSpace.put_InputType(TunerInputType.Antenna);
      tuningSpace.put_MaxMinorChannel(999); //minor channels per major
      tuningSpace.put_MaxPhysicalChannel(158); //69 for OTA 158 for QAM
      tuningSpace.put_MaxChannel(99); //major channels
      tuningSpace.put_MinMinorChannel(0);
      tuningSpace.put_MinPhysicalChannel(1); //OTA 1, QAM 2
      tuningSpace.put_MinChannel(1);

      IATSCLocator locator = (IATSCLocator)new ATSCLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.Mod8Vsb); //OTA modultation, QAM = .Mod256Qam
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_PhysicalChannel(-1);
      locator.put_SymbolRate(-1);
      locator.put_TSID(-1);
      object newIndex;
      _tuningSpace.put_DefaultLocator(locator);
      container.Add(_tuningSpace, out newIndex);
      tuner.put_TuningSpace(_tuningSpace);
      Release.ComObject("TuningSpaceContainer", container);
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IATSCChannelTuneRequest)request;
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Assemble a BDA tune request for a given channel.
    /// </summary>
    /// <param name="channel">The channel that will be tuned.</param>
    /// <returns><c>true</c> if the tune request is created successfully, otherwise <c>false</c></returns>
    protected override bool AssembleTuneRequest(IChannel channel)
    {
      ATSCChannel atscChannel = channel as ATSCChannel;
      if (atscChannel == null)
      {
        Log.Log.WriteFile("TvCardAtsc: channel is not an ATSC/QAM channel!!! {0}", channel.GetType().ToString());
        return false;
      }

      ITuneRequest request;
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = request;
      IATSCChannelTuneRequest tuneRequest = (IATSCChannelTuneRequest)_tuneRequest;
      ILocator locator;
      _tuningSpace.get_DefaultLocator(out locator);
      IATSCLocator atscLocator = (IATSCLocator)locator;
      atscLocator.put_SymbolRate(-1);
      atscLocator.put_TSID(-1);
      atscLocator.put_CarrierFrequency((int)atscChannel.Frequency);
      atscLocator.put_Modulation(atscChannel.ModulationType);
      tuneRequest.put_Channel(atscChannel.MajorChannel);
      tuneRequest.put_MinorChannel(atscChannel.MinorChannel);
      atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannel);
      _tuneRequest.put_Locator(locator);

      return true;
    }

    #endregion

    #region epg & scanning

    /// <summary>
    /// checks if a received EPGChannel should be filtered from the resultlist
    /// </summary>
    /// <value></value>
    protected override bool FilterOutEPGChannel(EpgChannel epgChannel)
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      if (layer.GetSetting("generalGrapOnlyForSameTransponder", "no").Value == "yes")
      {
        DVBBaseChannel chan = epgChannel.Channel as DVBBaseChannel;
        Channel dbchannel = layer.GetChannelByTuningDetail(chan.NetworkId, chan.TransportId, chan.ServiceId);
        ATSCChannel atscchannel = new ATSCChannel();
        if (dbchannel == null)
        {
          return false;
        }
        foreach (TuningDetail detail in dbchannel.ReferringTuningDetail())
        {
          if (detail.ChannelType == 1)
          {
            atscchannel.MajorChannel = detail.MajorChannel;
            atscchannel.MinorChannel = detail.MinorChannel;
            atscchannel.PhysicalChannel = detail.ChannelNumber;
          }
        }
        return this.CurrentChannel.IsDifferentTransponder(atscchannel);
      }
      else
        return false;
    }

    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    /// <value></value>
    public override ITVScanning ScanningInterface
    {
      get { return new ATSCScanning(this); }
    }

    #endregion

    /// <summary>
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if ((channel as ATSCChannel) == null)
      {
        return false;
      }
      return true;
    }

    protected override DVBBaseChannel CreateChannel(int networkid, int transportid, int serviceid, string name)
    {
      ATSCChannel channel = new ATSCChannel();
      channel.NetworkId = networkid;
      channel.TransportId = transportid;
      channel.ServiceId = serviceid;
      channel.Name = name;
      return channel;
    }
  }
}