/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles ATSC BDA cards
  /// </summary>
  public class TvCardATSC : TvCardDvbBase, IDisposable, ITVCard
  {
    #region variables
    /// <summary>
    /// Hold the ATSC tuning space
    /// </summary>
    protected IATSCTuningSpace _tuningSpace;
    /// <summary>
    /// Holds the current ATSC tuning request
    /// </summary>
    protected IATSCChannelTuneRequest _tuneRequest;

    #endregion

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
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph()
    {
      try
      {
        if (_graphState != GraphState.Idle)
        {
          Log.Log.Error("Graph already build");
          throw new TvException("Graph already build");
        }
        Log.Log.WriteFile("BuildGraph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        // Method names should be self explanatory
        AddNetworkProviderFilter(typeof(ATSCNetworkProvider).GUID);
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
        throw;
      }
    }

    /// <summary>
    /// Creates the tuning space.
    /// </summary>
    protected void CreateTuningSpace()
    {
      Log.Log.WriteFile("CreateTuningSpace()");
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
        if (name == "ATSC TuningSpace2")
        {
          Log.Log.WriteFile("got tuningspace");
          _tuningSpace = (IATSCTuningSpace)spaces[0];
          tuner.put_TuningSpace(_tuningSpace);
          _tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IATSCChannelTuneRequest)request;
          return;
        }
      }
      Log.Log.WriteFile("Create new tuningspace");
      _tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();
      _tuningSpace.put_UniqueName("ATSC TuningSpace2");
      _tuningSpace.put_FriendlyName("ATSC TuningSpace2");
      _tuningSpace.put_MaxChannel(10000);
      _tuningSpace.put_MaxMinorChannel(10000);
      _tuningSpace.put_MaxPhysicalChannel(10000);
      _tuningSpace.put__NetworkType(typeof(ATSCNetworkProvider).GUID);
      _tuningSpace.put_MinChannel(0);
      _tuningSpace.put_MinMinorChannel(0);
      _tuningSpace.put_MinPhysicalChannel(0);
      _tuningSpace.put_InputType(TunerInputType.Antenna);
      IATSCLocator locator = (IATSCLocator)new ATSCLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.ModNotSet);
      locator.put_OuterFEC(FECMethod.MethodNotSet);
      locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_SymbolRate(-1);
      locator.put_CarrierFrequency(-1);
      locator.put_PhysicalChannel(-1);
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
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("atsc:Tune:{0}", channel);
      try
      {
        //_pmtVersion = -1;
        ATSCChannel atscChannel = channel as ATSCChannel;
        if (atscChannel == null)
        {
          Log.Log.WriteFile("atsc:Channel is not a ATSC channel!!! {0}", channel.GetType().ToString());
          return null;
        }
        ATSCChannel oldChannel = (ATSCChannel)CurrentChannel;
        if (CurrentChannel != null)
        {
          if (oldChannel.Equals(channel))
          {
            //@FIX this fails for back-2-back recordings
            //Log.Log.WriteFile("atsc:Already tuned to channel!!! ");
            //return _mapSubChannels[0];
          }
        }
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        ILocator locator;
        _tuningSpace.get_DefaultLocator(out locator);
        IATSCLocator atscLocator = (IATSCLocator)locator;
        //hr = _tuningSpace.put_InputType(TunerInputType.Cable);
        atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannel);
        atscLocator.put_SymbolRate(-1);//atscChannel.SymbolRate);
        atscLocator.put_TSID(-1);//atscChannel.TransportId);
        atscLocator.put_CarrierFrequency((int)atscChannel.Frequency);
        atscLocator.put_InnerFEC(FECMethod.MethodNotSet);
        atscLocator.put_Modulation(atscChannel.ModulationType);
        _tuneRequest.put_MinorChannel(atscChannel.MinorChannel);
        _tuneRequest.put_Channel(atscChannel.MajorChannel);
        _tuneRequest.put_Locator(locator);
        //ViXS ATSC QAM check
        _conditionalAccess.CheckVIXSQAM(atscChannel);
        //QAM set paramters...
        _conditionalAccess.CheckATSCQAM(atscChannel);
        ITvSubChannel ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest);
        //ViXS QAM set is done here...
        _conditionalAccess.CheckViXSATSCQAM(atscChannel);
        RunGraph(ch.SubChannelId);
        return ch;
      } catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw;
      }
      //unreachable return null;
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
        return new ATSCScanning(this);
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
      if ((channel as ATSCChannel) == null)
        return false;
      return true;
    }
  }
}
