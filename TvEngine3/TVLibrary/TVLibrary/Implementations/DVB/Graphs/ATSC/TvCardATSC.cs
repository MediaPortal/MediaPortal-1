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
          Log.Log.Error("atsc:Graph already built");
          throw new TvException("Graph already built");
        }
        Log.Log.WriteFile("atsc:BuildGraph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        AddNetworkProviderFilter(typeof(ATSCNetworkProvider).GUID);
        CreateTuningSpace();
        AddMpeg2DemuxerToGraph();
        AddAndConnectBDABoardFilters(_device);
        AddBdaTransportFiltersToGraph();
        string graphName = _device.Name + " - ATSC Graph.grf";
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
      _tuningSpace.put_UniqueName("MediaPortal ATSC TuningSpace");
      _tuningSpace.put_FriendlyName("MediaPortal ATSC TuningSpace");
      _tuningSpace.put__NetworkType(typeof(ATSCNetworkProvider).GUID);
      _tuningSpace.put_CountryCode(0);
      _tuningSpace.put_InputType(TunerInputType.Antenna);
      _tuningSpace.put_MaxMinorChannel(999);//minor channels per major
      _tuningSpace.put_MaxPhysicalChannel(158);//69 for OTA 158 for QAM
      _tuningSpace.put_MaxChannel(99);//major channels
      _tuningSpace.put_MinMinorChannel(0);
      _tuningSpace.put_MinPhysicalChannel(1);//OTA 1, QAM 2
      _tuningSpace.put_MinChannel(1);
      IATSCLocator locator = (IATSCLocator)new ATSCLocator();
      locator.put_CarrierFrequency(-1);
      locator.put_InnerFEC(FECMethod.MethodNotSet);
      locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
      locator.put_Modulation(ModulationType.Mod8Vsb);//OTA modultation, QAM = .Mod256Qam
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
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <returns>true if succeeded else false</returns>
    public ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      Log.Log.WriteFile("atsc:Tune:{0} ", channel);
      try
      {
        ATSCChannel atscChannel = channel as ATSCChannel;
        if (atscChannel == null)
        {
          Log.Log.WriteFile("atsc:Channel is not a ATSC channel!!! {0}", channel.GetType().ToString());
          return null;
        }
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        ITvSubChannel ch;
        if (_previousChannel == null ||  _previousChannel.IsDifferentTransponder(atscChannel))
        {
          Log.Log.WriteFile("atsc:using new channel tuning settings");
          ITuneRequest request;
          int hr = _tuningSpace.CreateTuneRequest(out request);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - CreateTuneRequest");
          _tuneRequest = (IATSCChannelTuneRequest)request;
          ILocator locator;
          hr = _tuningSpace.get_DefaultLocator(out locator);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - get_DefaultLocator");
          IATSCLocator atscLocator = (IATSCLocator)locator;
          hr = atscLocator.put_SymbolRate(-1);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_SymbolRate");
          hr = atscLocator.put_TSID(-1);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_TSID");
          hr = atscLocator.put_CarrierFrequency((int)atscChannel.Frequency);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_CarrierFrequency");
          hr = atscLocator.put_Modulation(atscChannel.ModulationType);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_Modulation");
          hr = _tuneRequest.put_Channel(atscChannel.MajorChannel);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_Channel");
          hr = _tuneRequest.put_MinorChannel(atscChannel.MinorChannel);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_MinorChannel");
          hr = atscLocator.put_PhysicalChannel(atscChannel.PhysicalChannel);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_PhysicalChannel");
          hr = _tuneRequest.put_Locator(locator);
          if (hr != 0)
            Log.Log.WriteFile("atsc: Failed - put_Locator");
          //set QAM paramters if necessary...
          _conditionalAccess.CheckATSCQAM(atscChannel);
          ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest, true);
          _previousChannel = atscChannel;
        }
        else
        {
          Log.Log.WriteFile("atsc:using previous channel tuning settings");
          ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest, false);
        }
        try
        {
          RunGraph(ch.SubChannelId);
        }
        catch (TvExceptionNoSignal)
        {
          FreeSubChannel(ch.SubChannelId);
          throw;
        }
        return ch;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw;
      }
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