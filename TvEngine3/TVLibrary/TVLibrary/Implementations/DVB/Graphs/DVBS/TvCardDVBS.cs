#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Implementation of <see cref="T:TvLibrary.Interfaces.ITVCard"/> which handles DVB-S BDA cards
  /// </summary>
  public class TvCardDVBS : TvCardDvbBase, IDisposable, ITVCard
  {
    #region variables

    private bool _diseqCsucceded = true;
    private int _diseqCretries = 0;
    private DVBSChannel _dvbsChannel;    

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDVBS"/> class.
    /// </summary>
    /// <param name="epgEvents">The EPG events interface.</param>
    /// <param name="device">The device.</param>
    public TvCardDVBS(IEpgEvents epgEvents, DsDevice device)
      : base(epgEvents, device)
    {
      _cardType = CardType.DvbS;
    }

    #endregion

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
          Log.Log.Error("dvbs:Graph already built");
          throw new TvException("Graph already built");
        }
        Log.Log.WriteFile("dvbs:BuildGraph");
        _graphBuilder = (IFilterGraph2)new FilterGraph();
        _capBuilder = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        _capBuilder.SetFiltergraph(_graphBuilder);
        _rotEntry = new DsROTEntry(_graphBuilder);
        // Method names should be self explanatory
        AddNetworkProviderFilter(typeof(DVBSNetworkProvider).GUID);
        CreateTuningSpace();
        AddMpeg2DemuxerToGraph();
        AddAndConnectBDABoardFilters(_device);
        AddBdaTransportFiltersToGraph();
        string graphName = _device.Name + " - DVBS Graph.grf";
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
      Log.Log.WriteFile("dvbs:CreateTuningSpace()");
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
      int lowOsc;
      int hiOsc;
      int lnbSwitch;
      if (_parameters.UseDefaultLnbFrequencies)
      {
        lowOsc = 9750;
        hiOsc = 10600;
        lnbSwitch = 11700;
      }
      else
      {
        lowOsc = _parameters.LnbLowFrequency;
        hiOsc = _parameters.LnbHighFrequency;
        lnbSwitch = _parameters.LnbSwitchFrequency;
      }
      ITuneRequest request;
      container.get_EnumTuningSpaces(out enumTuning);
      IDVBSTuningSpace tuningSpace;
      while (true)
      {
        int fetched;
        enumTuning.Next(1, spaces, out fetched);
        if (fetched != 1)
          break;
        string name;
        spaces[0].get_UniqueName(out name);
        if (name == "MediaPortal DVBS TuningSpace")
        {
          Log.Log.WriteFile("dvbs:found correct tuningspace {0}", name);

          _tuningSpace = (IDVBSTuningSpace)spaces[0];
          tuningSpace = (IDVBSTuningSpace) _tuningSpace;
          tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
          tuningSpace.put_SpectralInversion(SpectralInversion.Automatic);
          tuningSpace.put_LowOscillator(lowOsc * 1000);
          tuningSpace.put_HighOscillator(hiOsc * 1000);
          tuner.put_TuningSpace(tuningSpace);
          tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IDVBTuneRequest)request;          
          return;
        }
        Release.ComObject("ITuningSpace", spaces[0]);
      }
      Release.ComObject("IEnumTuningSpaces", enumTuning);
      Log.Log.WriteFile("dvbs:Create new tuningspace");
      _tuningSpace = (IDVBSTuningSpace)new DVBSTuningSpace();
      tuningSpace = (IDVBSTuningSpace) _tuningSpace;
      tuningSpace.put_UniqueName("MediaPortal DVBS TuningSpace");
      tuningSpace.put_FriendlyName("MediaPortal DVBS TuningSpace");
      tuningSpace.put__NetworkType(typeof(DVBSNetworkProvider).GUID);
      tuningSpace.put_SystemType(DVBSystemType.Satellite);
      tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
      tuningSpace.put_LowOscillator(lowOsc * 1000);
      tuningSpace.put_HighOscillator(hiOsc * 1000);      
      IDVBSLocator locator = (IDVBSLocator)new DVBSLocator();
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
      Log.Log.WriteFile("dvbs: Tune:{0}", channel);
      try
      {
        DVBSChannel dvbsChannel = channel as DVBSChannel;
        if (dvbsChannel == null)
        {
          Log.Log.WriteFile("Channel is not a DVBS channel!!! {0}", channel.GetType().ToString());
          return null;
        }
        /*if (CurrentChannel != null)
        {
          //@FIX this fails for back-2-back recordings
          //if (oldChannel.Equals(channel)) return _mapSubChannels[0];
        }*/
        if (dvbsChannel.SwitchingFrequency < 10)
        {
          dvbsChannel.SwitchingFrequency = 11700000;
        }
        Log.Log.WriteFile("dvbs:  Tune:{0}", channel);
        if (_graphState == GraphState.Idle)
        {
          BuildGraph();
        }
        if (_mapSubChannels.ContainsKey(subChannelId) == false)
        {
          subChannelId = GetNewSubChannel(channel);
        }

        if (_previousChannel == null || _previousChannel.IsDifferentTransponder(dvbsChannel))
        {
          //_pmtPid = -1;
          ILocator locator;
          int lowOsc;
          int hiOsc;
          int lnbSwitch;
          BandTypeConverter.GetDefaultLnbSetup(Parameters, dvbsChannel.BandType, out lowOsc, out hiOsc, out lnbSwitch);
          Log.Log.Info("LNB low:{0} hi:{1} switch:{2}", lowOsc, hiOsc, lnbSwitch);
          if (lnbSwitch == 0)
            lnbSwitch = 18000;
          IDVBSTuningSpace tuningSpace = (IDVBSTuningSpace)_tuningSpace;
          tuningSpace.put_LNBSwitch(lnbSwitch * 1000);
          tuningSpace.put_LowOscillator(lowOsc * 1000);
          tuningSpace.put_HighOscillator(hiOsc * 1000);
          ITuneRequest request;
          _tuningSpace.CreateTuneRequest(out request);
          _tuneRequest = (IDVBTuneRequest)request;
          _tuningSpace.get_DefaultLocator(out locator);
          IDVBSLocator dvbsLocator = (IDVBSLocator)locator;
          IDVBTuneRequest tuneRequest = (IDVBTuneRequest)_tuneRequest;
          tuneRequest.put_ONID(dvbsChannel.NetworkId);
          tuneRequest.put_SID(dvbsChannel.ServiceId);
          tuneRequest.put_TSID(dvbsChannel.TransportId);
          locator.put_CarrierFrequency((int)dvbsChannel.Frequency);
          dvbsLocator.put_SymbolRate(dvbsChannel.SymbolRate);
          dvbsLocator.put_SignalPolarisation(dvbsChannel.Polarisation);
          //DVB-S2 specific modulation class call here if DVB-S2 card detected
          DVBSChannel tuneChannel = dvbsChannel;
          if (_conditionalAccess != null)
          {
            tuneChannel = _conditionalAccess.SetDVBS2Modulation(_parameters, dvbsChannel);
          }
          dvbsLocator.put_Modulation(tuneChannel.ModulationType);
          Log.Log.WriteFile("dvbs:channel modulation is set to {0}", tuneChannel.ModulationType);
          dvbsLocator.put_InnerFECRate(dvbsChannel.InnerFecRate);
          Log.Log.WriteFile("dvbs:channel FECRate is set to {0}", tuneChannel.InnerFecRate);
          _tuneRequest.put_Locator(locator);
          //set the DisEqC parameters 
          if (_conditionalAccess != null)
          {
            _diseqCsucceded = _conditionalAccess.SendDiseqcCommand(_parameters, dvbsChannel);
          }          
        }

        _dvbsChannel = dvbsChannel;
        ITvSubChannel ch = base.Tune(subChannelId, channel);

        // workaround for hauppauge dvb-s cards that fail to properly set diseqC the first time around.
        // restart graph, but only once.
        if (_diseqCretries == 0 && !_diseqCsucceded && _conditionalAccess != null)
        {
          _diseqCretries++;
          Log.Log.WriteFile("dvbs: setting diseqC failed the first time, resetting diseqC");
          StopGraph();
          Tune(subChannelId, channel);
        }

        if (dvbsChannel.ServiceId < 0 || dvbsChannel.NetworkId < 0 || dvbsChannel.TransportId < 0)
          _filterTIF.Stop();
        return ch;
      }
      catch (TvExceptionNoSignal)
      {
        throw;
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw;
      }
    }

    protected override void OnAfterTune (IChannel channel)
    {
      DVBSChannel dvbsChannel = channel as DVBSChannel;
      if (dvbsChannel != null)
      {
        //move diseqc motor to correct satellite
        if (_conditionalAccess != null)
        {
          if (dvbsChannel.SatelliteIndex > 0 && _conditionalAccess.DiSEqCMotor != null)
          {
            _conditionalAccess.DiSEqCMotor.GotoPosition((byte)dvbsChannel.SatelliteIndex);
          }
        } 
      }      
    }

    protected override bool ShouldWaitForSignal()
    {
      bool shouldWait = true;

      if (_diseqCretries == 0 && !_diseqCsucceded && _conditionalAccess != null)
      {
        shouldWait = false;
      }

      return shouldWait;
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
        return new DVBSScanning(this);
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
      if ((channel as DVBSChannel) == null)
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