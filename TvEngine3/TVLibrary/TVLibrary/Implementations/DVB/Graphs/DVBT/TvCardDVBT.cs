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
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-T BDA cards
  /// </summary>
  public class TvCardDVBT : TvCardDvbBase
  {
    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDVBT"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardDVBT(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _cardType = CardType.DvbT;
    }

    #endregion

    #region graphbuilding

    /// <summary>
    /// Create the BDA tuning space for the tuner. This will be used for BDA tuning.
    /// </summary>
    protected override void CreateTuningSpace()
    {
      Log.Log.WriteFile("dvbt:CreateTuningSpace()");
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
        if (name == "MediaPortal DVBT TuningSpace")
        {
          Log.Log.WriteFile("dvbt:found correct tuningspace {0}", name);
          _tuningSpace = (IDVBTuningSpace)spaces[0];
          tuner.put_TuningSpace(_tuningSpace);
          _tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IDVBTuneRequest)request;
          return;
        }
        Release.ComObject("ITuningSpace", spaces[0]);
      }
      Release.ComObject("IEnumTuningSpaces", enumTuning);
      Log.Log.WriteFile("dvbt:Create new tuningspace");
      _tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
      IDVBTuningSpace tuningSpace = (IDVBTuningSpace)_tuningSpace;
      tuningSpace.put_UniqueName("MediaPortal DVBT TuningSpace");
      tuningSpace.put_FriendlyName("MediaPortal DVBT TuningSpace");
      tuningSpace.put__NetworkType(typeof (DVBTNetworkProvider).GUID);
      tuningSpace.put_SystemType(DVBSystemType.Terrestrial);

      IDVBTLocator locator = (IDVBTLocator)new DVBTLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      object newIndex;
      _tuningSpace.put_DefaultLocator(locator);
      container.Add(_tuningSpace, out newIndex);
      tuner.put_TuningSpace(_tuningSpace);
      Release.ComObject("ITuningSpaceContainer", container);
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = request;
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
      DVBTChannel dvbtChannel = channel as DVBTChannel;
      if (dvbtChannel == null)
      {
        Log.Log.WriteFile("TvCardDvbT: channel is not a DVB-T channel!!! {0}", channel.GetType().ToString());
        return false;
      }

      ILocator locator;
      _tuningSpace.get_DefaultLocator(out locator);
      IDVBTLocator dvbtLocator = (IDVBTLocator)locator;
      dvbtLocator.put_Bandwidth(dvbtChannel.BandWidth);
      IDVBTuneRequest tuneRequest = (IDVBTuneRequest)_tuneRequest;
      tuneRequest.put_ONID(dvbtChannel.NetworkId);
      tuneRequest.put_SID(dvbtChannel.ServiceId);
      tuneRequest.put_TSID(dvbtChannel.TransportId);
      locator.put_CarrierFrequency((int)dvbtChannel.Frequency);
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
        DVBTChannel dvbtchannel = new DVBTChannel();
        if (dbchannel == null)
        {
          return false;
        }
        foreach (TuningDetail detail in dbchannel.ReferringTuningDetail())
        {
          if (detail.ChannelType == 4)
          {
            dvbtchannel.Frequency = detail.Frequency;
            dvbtchannel.BandWidth = detail.Bandwidth;
          }
        }
        return this.CurrentChannel.IsDifferentTransponder(dvbtchannel);
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
      get
      {
        if (!CheckThreadId())
          return null;
        return new DVBTScanning(this);
      }
    }

    #endregion

    /// <summary>
    /// Check if the tuner can tune to a given channel.
    /// </summary>
    /// <param name="channel">The channel to check.</param>
    /// <returns><c>true</c> if the tuner can tune to the channel, otherwise <c>false</c></returns>
    public override bool CanTune(IChannel channel)
    {
      if ((channel as DVBTChannel) == null)
      {
        return false;
      }
      return true;
    }

    protected override DVBBaseChannel CreateChannel(int networkid, int transportid, int serviceid, string name)
    {
      DVBTChannel channel = new DVBTChannel();
      channel.NetworkId = networkid;
      channel.TransportId = transportid;
      channel.ServiceId = serviceid;
      channel.Name = name;
      return channel;
    }
  }
}