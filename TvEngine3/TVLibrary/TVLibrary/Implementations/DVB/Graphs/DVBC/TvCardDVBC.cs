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
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Implementations.Helper;
using TvDatabase;
using TvLibrary.Epg;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-C BDA cards
  /// </summary>
  public class TvCardDVBC : TvCardDvbBase
  {
    #region variables

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDVBC"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardDVBC(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _cardType = CardType.DvbC;
    }

    #region graphbuilding

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.Error("dvbc:Graph already built");
          throw new TvException("Graph already built");
        }
        Log.Log.WriteFile("dvbc:BuildGraph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        AddNetworkProviderFilter(typeof (DVBCNetworkProvider).GUID);
        AddTsWriterFilterToGraph();
        if (!useInternalNetworkProvider)
        {
          CreateTuningSpace();
          AddMpeg2DemuxerToGraph();
        }
        AddAndConnectBDABoardFilters(_device);
        string graphName = _device.Name + " - DVBC Graph.grf";
        FilterGraphTools.SaveGraphFile(_graphBuilder, graphName);
        GetTunerSignalStatistics();
        _graphState = GraphState.Created;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        Dispose();
        _graphState = GraphState.Idle;
        throw new TvExceptionGraphBuildingFailed("Graph building failed", ex);
      }
    }

    /// <summary>
    /// Creates the tuning space.
    /// </summary>
    protected void CreateTuningSpace()
    {
      Log.Log.WriteFile("dvbc:CreateTuningSpace()");
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
      if (enumTuning != null)
      {
        while (true)
        {
          int fetched;
          if (enumTuning.Next(1, spaces, out fetched) != 0)
          {
            break;
          }
          if (fetched != 1)
            break;
          if (spaces[0] == null)
            break;
          string name;
          spaces[0].get_UniqueName(out name);
          if (name != null)
          {
            if (name == "MediaPortal DVBC TuningSpace")
            {
              Log.Log.WriteFile("dvbc:Found correct tuningspace {0}", name);
              _tuningSpace = (IDVBTuningSpace)spaces[0];
              tuner.put_TuningSpace(_tuningSpace);
              _tuningSpace.CreateTuneRequest(out request);
              _tuneRequest = (IDVBTuneRequest)request;
              return;
            }
          }
          Release.ComObject("ITuningSpace", spaces[0]);
        }
        Release.ComObject("IEnumTuningSpaces", enumTuning);
      }
      Log.Log.WriteFile("dvbc:Create new tuningspace");
      _tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
      IDVBTuningSpace tuningSpace = (IDVBTuningSpace)_tuningSpace;
      tuningSpace.put_UniqueName("MediaPortal DVBC TuningSpace");
      tuningSpace.put_FriendlyName("MediaPortal DVBC TuningSpace");
      tuningSpace.put__NetworkType(typeof (DVBCNetworkProvider).GUID);
      tuningSpace.put_SystemType(DVBSystemType.Cable);
      IDVBCLocator locator = (IDVBCLocator)new DVBCLocator();
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
      Release.ComObject("TuningSpaceContainer", container);
      _tuningSpace.CreateTuneRequest(out request);
      _tuneRequest = (IDVBTuneRequest)request;
    }

    /// <summary>
    /// Methods which stops the graph
    /// </summary>
    public override void StopGraph()
    {
      base.StopGraph();
      _previousChannel = null;
    }

    #endregion

    #region tuning & recording

    /// <summary>
    /// Scans the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public override ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("dvbc: Scan:{0}", channel);
      try
      {
        if (!BeforeTune(channel, ref subChannelId))
        {
          return null;
        }
        ITvSubChannel ch = base.Scan(subChannelId, channel);
        return ch;
      }
      catch (TvExceptionNoSignal)
      {
        throw;
      }
      catch (TvExceptionNoPMT)
      {
        throw;
      }
      catch (TvExceptionTuneCancelled)
      {
        throw;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw;
      }
    }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public override ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("dvbc: Tune:{0}", channel);
      try
      {
        if (!BeforeTune(channel, ref subChannelId))
        {
          return null;
        }
        ITvSubChannel ch = base.Tune(subChannelId, channel);
        return ch;
      }
      catch (TvExceptionTuneCancelled)
      {
        throw;
      }
      catch (TvExceptionNoSignal)
      {
        throw;
      }
      catch (TvExceptionNoPMT)
      {
        throw;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw;
      }
    }

    private bool BeforeTune(IChannel channel, ref int subChannelId)
    {
      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel == null)
      {
        Log.Log.WriteFile("dvbc:Channel is not a DVBC channel!!! {0}", channel.GetType().ToString());
        return false;
      }
      /*if (CurrentChannel != null)
        {
          //@FIX this fails for back-2-back recordings
          //if (oldChannel.Equals(channel)) return _mapSubChannels[0];
        }*/
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
        if (_mapSubChannels.ContainsKey(subChannelId) == false)
        {
          subChannelId = GetNewSubChannel(channel);
        }
      }
      if (useInternalNetworkProvider)
      {
        return true;
      }

      if (_previousChannel == null || _previousChannel.IsDifferentTransponder(dvbcChannel))
      {
        //_pmtPid = -1; 
        ILocator locator;
        _tuningSpace.get_DefaultLocator(out locator);
        IDVBCLocator dvbcLocator = (IDVBCLocator)locator;
        dvbcLocator.put_InnerFEC(FECMethod.MethodNotSet);
        dvbcLocator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        dvbcLocator.put_OuterFEC(FECMethod.MethodNotSet);
        dvbcLocator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
        dvbcLocator.put_Modulation(dvbcChannel.ModulationType);
        dvbcLocator.put_SymbolRate(dvbcChannel.SymbolRate);
        IDVBTuneRequest tuneRequest = (IDVBTuneRequest)_tuneRequest;
        tuneRequest.put_ONID(dvbcChannel.NetworkId);
        tuneRequest.put_SID(dvbcChannel.ServiceId);
        tuneRequest.put_TSID(dvbcChannel.TransportId);
        locator.put_CarrierFrequency((int)dvbcChannel.Frequency);
        _tuneRequest.put_Locator(locator);
        _tuneRequest = tuneRequest;
      }
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
        DVBCChannel dvbcchannel = new DVBCChannel();
        if (dbchannel == null)
        {
          return false;
        }
        foreach (TuningDetail detail in dbchannel.ReferringTuningDetail())
        {
          if (detail.ChannelType == 2)
          {
            dvbcchannel.Frequency = detail.Frequency;
            dvbcchannel.ModulationType = (ModulationType)detail.Modulation;
            dvbcchannel.SymbolRate = detail.Symbolrate;
          }
        }
        return this.CurrentChannel.IsDifferentTransponder(dvbcchannel);
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
        return new DVBCScanning(this);
      }
    }

    #endregion

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }

    /// <summary>
    /// Method to check if card can tune to the channel specified
    /// </summary>
    /// <param name="channel"></param>
    /// <returns>
    /// true if card can tune to the channel otherwise false
    /// </returns>
    public override bool CanTune(IChannel channel)
    {
      if ((channel as DVBCChannel) == null)
        return false;
      return true;
    }

    protected override DVBBaseChannel CreateChannel(int networkid, int transportid, int serviceid, string name)
    {
      DVBCChannel channel = new DVBCChannel();
      channel.NetworkId = networkid;
      channel.TransportId = transportid;
      channel.ServiceId = serviceid;
      channel.Name = name;
      return channel;
    }
  }
}