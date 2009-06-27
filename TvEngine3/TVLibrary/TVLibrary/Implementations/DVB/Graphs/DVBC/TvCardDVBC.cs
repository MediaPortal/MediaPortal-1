/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Interfaces;
using TvLibrary.Channels;
using TvLibrary.Implementations.Helper;

namespace TvLibrary.Implementations.DVB
{

  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-C BDA cards
  /// </summary>
  public class TvCardDVBC : TvCardDvbBase, IDisposable, ITVCard
  {

    #region variables
    /// <summary>
    /// holds the the DVB-C tuning space
    /// </summary>
    protected IDVBTuningSpace _tuningSpace;
    /// <summary>
    /// holds the current DVB-C tuning request
    /// </summary>
    protected IDVBTuneRequest _tuneRequest;

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
          throw new TvException("Graph already build");
        }
        Log.Log.WriteFile("dvbc:BuildGraph");

        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);

        AddNetworkProviderFilter(typeof(DVBCNetworkProvider).GUID);
        CreateTuningSpace();
        AddMpeg2DemuxerToGraph();
        AddAndConnectBDABoardFilters(_device);
        AddBdaTransportFiltersToGraph();

        GetTunerSignalStatistics();
        _graphState = GraphState.Created;

      } catch (Exception ex)
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
      //Log.Log.WriteFile("CreateTuningSpace()");
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
            //Log.Log.WriteFile("Found tuningspace {0}", name);
            if (name == "DVBC TuningSpace")
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
      //Log.Log.WriteFile("Create new tuningspace");
      _tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();
      _tuningSpace.put_UniqueName("DVBC TuningSpace");
      _tuningSpace.put_FriendlyName("DVBC TuningSpace");
      _tuningSpace.put__NetworkType(typeof(DVBCNetworkProvider).GUID);
      _tuningSpace.put_SystemType(DVBSystemType.Cable);

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
    #endregion

    #region tuning & recording
    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("dvbc: Tune:{0}", channel);

      DVBCChannel dvbcChannel = channel as DVBCChannel;
      if (dvbcChannel == null)
      {
        Log.Log.WriteFile("dvbc:Channel is not a DVBC channel!!! {0}", channel.GetType().ToString());
        return null;
      }
      if (CurrentChannel != null)
      {
        //@FIX this fails for back-2-back recordings
        //if (oldChannel.Equals(channel)) return _mapSubChannels[0];
      }
      if (_graphState == GraphState.Idle)
      {
        BuildGraph();
        if (_mapSubChannels.ContainsKey(subChannelId) == false)
        {
          subChannelId = GetNewSubChannel(channel);
        }
      }

      ITvSubChannel ch;
      if (_previousChannel == null ||  _previousChannel.IsDifferentTransponder(dvbcChannel))
      {

        //_pmtPid = -1; 
        ILocator locator;
        _tuningSpace.get_DefaultLocator(out locator);
        IDVBCLocator dvbcLocator = (IDVBCLocator) locator;
        dvbcLocator.put_InnerFEC(FECMethod.MethodNotSet);
        dvbcLocator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
        dvbcLocator.put_OuterFEC(FECMethod.MethodNotSet);
        dvbcLocator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);


        dvbcLocator.put_Modulation(dvbcChannel.ModulationType);
        dvbcLocator.put_SymbolRate(dvbcChannel.SymbolRate);
        _tuneRequest.put_ONID(dvbcChannel.NetworkId);
        _tuneRequest.put_SID(dvbcChannel.ServiceId);
        _tuneRequest.put_TSID(dvbcChannel.TransportId);
        locator.put_CarrierFrequency((int) dvbcChannel.Frequency);

        _tuneRequest.put_Locator(locator);

        ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest,true);
        _previousChannel = dvbcChannel;
      }else
      {
        ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest,false);
      }
      try
      {
        RunGraph(ch.SubChannelId);
      } catch (TvExceptionNoSignal)
      {
        FreeSubChannel(ch.SubChannelId);
        throw;
      }
      return ch;
      //SetupPmtGrabber(dvbcChannel.PmtPid);

    }

    #endregion

    #region epg & scanning
    /// <summary>
    /// returns the ITVScanning interface used for scanning channels
    /// </summary>
    /// <value></value>
    public ITVScanning ScanningInterface
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
    public bool CanTune(IChannel channel)
    {
      if ((channel as DVBCChannel) == null)
        return false;
      return true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
      _previousChannel = null;
      base.Dispose();
    }
  }
}
