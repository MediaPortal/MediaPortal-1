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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Hardware;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Channels;
using TvLibrary.Implementations.Helper;
using TvLibrary.Epg;
using TvLibrary.ChannelLinkage;
using MediaPortal.TV.Epg;
using TvDatabase;

namespace TvLibrary.Implementations.DVB
{

  #region enums;

  internal enum BdaDigitalModulator
  {
    MODULATION_TYPE = 0,
    INNER_FEC_TYPE,
    INNER_FEC_RATE,
    OUTER_FEC_TYPE,
    OUTER_FEC_RATE,
    SYMBOL_RATE,
    SPECTRAL_INVERSION,
    GUARD_INTERVAL,
    TRANSMISSION_MODE
  } ;

  internal enum BdaTunerExtension
  {
    KSPROPERTY_BDA_DISEQC = 0,
    KSPROPERTY_BDA_SCAN_FREQ,
    KSPROPERTY_BDA_CHANNEL_CHANGE,
    KSPROPERTY_BDA_EFFECTIVE_FREQ,
    KSPROPERTY_BDA_PILOT = 0x20,
    KSPROPERTY_BDA_ROLL_OFF = 0x21
  } ;

  internal enum DisEqcVersion
  {
    DISEQC_VER_1X = 1,
    DISEQC_VER_2X,
  } ;

  internal enum RxMode
  {
    RXMODE_INTERROGATION = 1, // Expecting multiple devices attached
    RXMODE_QUICKREPLY, // Expecting 1 rx (rx is suspended after 1st rx received)
    RXMODE_NOREPLY, // Expecting to receive no Rx message(s)
    RXMODE_DEFAULT = 0 // use current register setting
  } ;

  internal enum BurstModulationType
  {
    TONE_BURST_UNMODULATED = 0,
    TONE_BURST_MODULATED
  } ;

  #endregion;

  /// <summary>
  /// base class for DVB cards
  /// </summary>
  public abstract class TvCardDvbBase : TvCardBase, IDisposable, ITVCard
  {
    #region constants

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    private class MpTsAnalyzer {}

    [ComImport, Guid("BC650178-0DE4-47DF-AF50-BBD9C7AEF5A9")]
    private class CyberLinkMuxer {}

    [ComImport, Guid("7F2BBEAF-E11C-4D39-90E8-938FB5A86045")]
    private class PowerDirectorMuxer {}

    [ComImport, Guid("3E8868CB-5FE8-402C-AA90-CB1AC6AE3240")]
    private class CyberLinkDumpFilter {} ;

    #endregion

    #region variables

    /// <summary>
    /// holds the the DVB tuning space
    /// </summary>
    protected ITuningSpace _tuningSpace;

    /// <summary>
    /// holds the current DVB tuning request
    /// </summary>
    protected ITuneRequest _tuneRequest;

    /// <summary>
    /// Capture graph builder
    /// </summary>
    protected ICaptureGraphBuilder2 _capBuilder;

    /// <summary>
    /// ROT entry
    /// </summary>
    protected DsROTEntry _rotEntry;

    /// <summary>
    /// Network provider filter
    /// </summary>
    protected IBaseFilter _filterNetworkProvider;

    /// <summary>
    /// MPEG2 Demux filter
    /// </summary>
    protected IBaseFilter _filterMpeg2DemuxTif;

    /// <summary>
    /// Main inf tee
    /// </summary>
    protected IBaseFilter _infTeeMain;

    /// <summary>
    /// Main inf tee
    /// </summary>
    protected IBaseFilter _infTeeSecond;

    /// <summary>
    /// Tuner filter
    /// </summary>
    protected IBaseFilter _filterTuner;

    /// <summary>
    /// Capture filter
    /// </summary>
    protected IBaseFilter _filterCapture;

    /// <summary>
    /// TIF filter
    /// </summary>
    protected IBaseFilter _filterTIF;

    /// <summary>
    /// WinTV CI filter
    /// </summary>
    protected IBaseFilter _filterWinTvUsb;

    /// <summary>
    /// DigitalDevices CI filter
    /// </summary>
    protected IBaseFilter _filterDigitalDevicesCI;

    /// <summary>
    /// Capture device
    /// </summary>
    protected DsDevice _captureDevice;

    /// <summary>
    /// WinTV CI device
    /// </summary>
    protected DsDevice _deviceWinTvUsb;

    /// <summary>
    /// DigitalDevices CI device
    /// </summary>
    protected DsDevice _deviceDigitalDevicesCI;

    /// <summary>
    /// EPG Grabber callback
    /// </summary>
    protected BaseEpgGrabber _epgGrabberCallback;

    /// <summary>
    /// MD plugs
    /// </summary>
    protected MDPlugs _mdplugs;

    /// <summary>
    /// Tuner statistics
    /// </summary>
    protected List<IBDA_SignalStatistics> _tunerStatistics = new List<IBDA_SignalStatistics>();

    /// <summary>
    /// TsWriter filter
    /// </summary>
    protected IBaseFilter _filterTsWriter;

    /// <summary>
    /// Managed Thread id
    /// </summary>
    protected int _managedThreadId = -1;

    /// <summary>
    /// Is ATSC indicator
    /// </summary>
    protected bool _isATSC;

    /// <summary>
    /// EPG Grabber interface
    /// </summary>
    protected ITsEpgScanner _interfaceEpgGrabber;

    /// <summary>
    /// Channel scan interface
    /// </summary>
    protected ITsChannelScan _interfaceChannelScan;

    /// <summary>
    /// Internal Network provider instance
    /// </summary>
    protected IDvbNetworkProvider _interfaceNetworkProvider;

    /// <summary>
    /// Indicates if the internal network provider is used
    /// </summary>
    protected bool useInternalNetworkProvider;

    /// <summary>
    /// Channel linkage scanner interface
    /// </summary>
    protected ITsChannelLinkageScanner _interfaceChannelLinkageScanner;

    /// <summary>
    /// Hauppauge inteface
    /// </summary>
    protected Hauppauge _hauppauge;

    /// <summary>
    /// Tuner only card indicator
    /// </summary>
    protected bool tunerOnly;

    /// <summary>
    /// Device paths are matching indicator
    /// </summary>
    protected bool matchDevicePath;

    private readonly TimeShiftingEPGGrabber _timeshiftingEPGGrabber;
    private WinTvCiModule winTvCiHandler;

    /// <summary>
    /// The previous channel
    /// </summary>
    protected IChannel _previousChannel;

    protected bool _cancelTune;

    #endregion

    #region ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDvbBase"/> class.
    /// </summary>
    public TvCardDvbBase(IEpgEvents epgEvents, DsDevice device)
      : base(device)
    {
      matchDevicePath = true;
      _lastSignalUpdate = DateTime.MinValue;
      _mapSubChannels = new Dictionary<int, BaseSubChannel>();
      _parameters = new ScanParameters();
      _timeshiftingEPGGrabber = new TimeShiftingEPGGrabber(epgEvents, (ITVCard)this);
      _minChannel = -1;
      _maxChannel = -1;
      _supportsSubChannels = true;
      Guid networkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
      useInternalNetworkProvider = FilterGraphTools.IsThisComObjectInstalled(networkProviderClsId);
    }

    #endregion

    #region tuning

    protected virtual void OnAfterTune(IChannel channel) {}

    /// <summary>
    /// Scans the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public virtual ITvSubChannel Scan(int subChannelId, IChannel channel)
    {
      return DoTune(subChannelId, channel, true);
    }

    public abstract ITVScanning ScanningInterface { get; }

    /// <summary>
    /// Tunes the specified channel.
    /// </summary>
    /// <param name="subChannelId">The sub channel id</param>
    /// <param name="channel">The channel.</param>
    /// <returns></returns>
    public virtual ITvSubChannel Tune(int subChannelId, IChannel channel)
    {
      return DoTune(subChannelId, channel, false);
    }

    private ITvSubChannel DoTune(int subChannelId, IChannel channel, bool ignorePMT)
    {      
      bool performTune = (_previousChannel == null || _previousChannel.IsDifferentTransponder(channel));
      ITvSubChannel ch = SubmitTuneRequest(subChannelId, channel, _tuneRequest, performTune);
      _previousChannel = channel;

      try
      {
        if (ch != null)
        {
          try
          {
            RunGraph(ch.SubChannelId);
          }
          catch (TvExceptionNoPMT)
          {
            if (!ignorePMT)
            {
              throw;
            }
          }
          OnAfterTune(channel);
          return ch;
        }
        else
        {
          throw new TvException("TvCardDvbBase.Tune: Subchannel was null");
        }
      }
      catch (Exception)
      {
        if (ch != null)
        {
          FreeSubChannel(ch.SubChannelId);
        }
        throw;
      }
      finally
      {
        _cancelTune = false;
      }
    }

    #endregion

    #region subchannel management

    /// <summary>
    /// Frees the sub channel.
    /// </summary>
    /// <param name="id">Handle to the subchannel.</param>
    public override void FreeSubChannel(int id)
    {      
      if (_mdplugs != null)
      {        
        _mdplugs.FreeSubChannel(id);
      }
      base.FreeSubChannel(id);
    }

    //public delegate void OnNewSubChannelDelegate(int id);
    public event OnNewSubChannelDelegate OnNewSubChannelEvent;

    private void FireOnNewSubChannelEvent(int id)
    {
     if (OnNewSubChannelEvent != null)
     {
       OnNewSubChannelEvent(id);
     }
    }

    /// <summary>
    /// Allocates a new instance of TvDvbChannel which handles the new subchannel
    /// </summary>
    /// <returns>handle for to the subchannel</returns>
    protected int GetNewSubChannel(IChannel channel)
    {
      int id = _subChannelId++;
      Log.Log.Info("dvb:GetNewSubChannel:{0} #{1}", _mapSubChannels.Count, id);

      TvDvbChannel subChannel = new TvDvbChannel(_graphBuilder, _conditionalAccess, _mdplugs, _filterTIF,
                                                 _filterTsWriter, id, channel);
      subChannel.Parameters = Parameters;
      subChannel.CurrentChannel = channel;
      _mapSubChannels[id] = subChannel;
      FireOnNewSubChannelEvent(id);
      return id;
    }

    #endregion

    #region graph building

    /// <summary>
    /// Builds the graph.
    /// </summary>
    public override void BuildGraph() {}

    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    protected static bool CheckThreadId()
    {
      return true;
    }

    /// <summary>
    /// submits a tune request to the card.
    /// throws an TvException if card cannot tune to the channel requested
    /// </summary>
    /// <param name="subChannelId">The sub channel id.</param>
    /// <param name="channel">The channel.</param>
    /// <param name="tuneRequest">tune requests</param>
    /// <param name="performTune">Indicates if a tune is required</param>
    /// <returns></returns>
    private ITvSubChannel SubmitTuneRequest(int subChannelId, IChannel channel, ITuneRequest tuneRequest,
                                              bool performTune)
    {
      Log.Log.Info("dvb:Submiting tunerequest Channel:{0} subChannel:{1} ", channel.Name, subChannelId);
      bool newSubChannel = false;
      if (_mapSubChannels.ContainsKey(subChannelId) == false)
      {
        Log.Log.Info("dvb:Getting new subchannel");
        newSubChannel = true;
        subChannelId = GetNewSubChannel(channel);
      }
      else
      {
        Log.Log.Info("dvb:using existing subchannel:{0}", subChannelId);
      }
      Log.Log.Info("dvb:Submit tunerequest size:{0} new:{1}", _mapSubChannels.Count, subChannelId);

      _mapSubChannels[subChannelId].CurrentChannel = channel;
      try
      {         
        _mapSubChannels[subChannelId].OnBeforeTune();        
        if (_interfaceEpgGrabber != null)
        {
          _interfaceEpgGrabber.Reset();
        }
        if (performTune)
        {
          if (useInternalNetworkProvider)
          {
            PerformInternalNetworkProviderTuning(channel);
          }
          else
          {
            // HW provider supported tuning methods (i.e. TeVii API)
            if (_conditionalAccess != null &&
                _conditionalAccess.HWProvider != null &&
                _conditionalAccess.HWProvider is ICustomTuning &&
                (_conditionalAccess.HWProvider as ICustomTuning).SupportsTuningForChannel(channel))
            {
              Log.Log.WriteFile("dvb:Custom tune method detected");
              bool res = (_conditionalAccess.HWProvider as ICustomTuning).CustomTune(channel, _parameters);
              Log.Log.WriteFile("dvb:Custom tune method finished with result {0}", res);
              if (!res)
              {
                throw new TvException("Unable to tune to channel");
              }
            }
            else
            {
              Log.Log.WriteFile("dvb:Submit tunerequest calling put_TuneRequest");
              if (_cancelTune)
              {
                throw new TvExceptionTuneCancelled();
              }
              int hr = ((ITuner)_filterNetworkProvider).put_TuneRequest(tuneRequest);
              Log.Log.WriteFile("dvb:Submit tunerequest done calling put_TuneRequest");

              //  NOTE
              //  After mantis 3469 is confirmed working in 1.2.0 beta remove these comments and everything:
              //  ***** FROM HERE *****
              var revert = false;

              try
              {
                var revertFile = new System.IO.FileInfo(@"c:\revertputtunerequest.txt");

                if (revertFile.Exists)
                  revert = true;

                if (revert)
                  Log.Log.WriteFile("dvb:Reverting put_tuneRequest error catch to pre-1.2.0 beta");
              }
              catch (Exception)
              {
                //  Make sure no new errors are introduced
              }

              if ((!revert && hr < 0) || (revert && hr != 0))
              
              //  ***** TO HERE *****
              //  AND uncomment this line:
              //  if(hr < 0)
              {
                Log.Log.WriteFile("dvb:SubmitTuneRequest  returns:0x{0:X} - {1}{2}", hr, HResult.GetDXErrorString(hr),
                                  DsError.GetErrorText(hr));
                throw new TvException("Unable to tune to channel");
              }
            }
          }
        }
        _lastSignalUpdate = DateTime.MinValue;
        _mapSubChannels[subChannelId].OnAfterTune();
      }
      catch (Exception ex)
      {        
        if (newSubChannel)
        {
          Log.Log.WriteFile("dvb:SubmitTuneRequest  failed - removing subchannel: {0}, {1} - {2}", subChannelId, ex.Message, ex.StackTrace);
          if (_mapSubChannels.ContainsKey(subChannelId))
          {
            _mapSubChannels.Remove(subChannelId);
          }
        }
        else
        {
          Log.Log.WriteFile("dvb:SubmitTuneRequest  failed - subchannel: {0}", subChannelId);
        }

        throw;
      }

      return _mapSubChannels[subChannelId];
    }      

    /// <summary>
    /// Performs a tuning using the internal network provider
    /// </summary>
    /// <param name="channel">Channel to tune</param>
    private void PerformInternalNetworkProviderTuning(IChannel channel)
    {
      Log.Log.WriteFile("dvb:Submit tunerequest calling put_TuneRequest");
      int hr = 0;
      int undefinedValue = -1;
      if (channel is DVBTChannel)
      {
        DVBTChannel dvbtChannel = channel as DVBTChannel;
        FrequencySettings fSettings = new FrequencySettings
                                        {
                                          Multiplier = 1000,
                                          Frequency = (uint)(dvbtChannel.Frequency),
                                          Bandwidth = (uint)dvbtChannel.BandWidth,
                                          Polarity = Polarisation.NotSet,
                                          Range = (uint)undefinedValue
                                        };
        hr = _interfaceNetworkProvider.TuneDVBT(fSettings);
      }
      if (channel is DVBSChannel)
      {
        DVBSChannel dvbsChannel = channel as DVBSChannel;
        if (dvbsChannel.ModulationType == ModulationType.ModNotSet)
        {
          dvbsChannel.ModulationType = ModulationType.ModQpsk;
        }
        int lowOsc;
        int hiOsc;
        int lnbSwitch;
        BandTypeConverter.GetDefaultLnbSetup(Parameters, dvbsChannel.BandType, out lowOsc, out hiOsc, out lnbSwitch);
        Log.Log.Info("LNB low:{0} hi:{1} switch:{2}", lowOsc, hiOsc, lnbSwitch);
        if (lnbSwitch == 0)
        {
          lnbSwitch = 18000;
        }
        FrequencySettings fSettings = new FrequencySettings
                                        {
                                          Multiplier = 1000,
                                          Frequency = (uint)dvbsChannel.Frequency,
                                          Bandwidth = (uint)undefinedValue,
                                          Polarity = dvbsChannel.Polarisation,
                                          Range = (uint)undefinedValue
                                        };
        DigitalDemodulator2Settings dSettings = new DigitalDemodulator2Settings
                                                  {
                                                    InnerFECRate = dvbsChannel.InnerFecRate,
                                                    InnerFECMethod = FECMethod.MethodNotSet,
                                                    Modulation = dvbsChannel.ModulationType,
                                                    OuterFECMethod = FECMethod.MethodNotSet,
                                                    OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
                                                    Pilot = Pilot.NotSet,
                                                    RollOff = RollOff.NotSet,
                                                    SpectralInversion = SpectralInversion.NotSet,
                                                    SymbolRate = (uint)dvbsChannel.SymbolRate,
                                                    TransmissionMode = TransmissionMode.ModeNotSet
                                                  };
        LnbInfoSettings lSettings = new LnbInfoSettings
                                      {
                                        LnbSwitchFrequency = (uint)lnbSwitch * 1000,
                                        LowOscillator = (uint)lowOsc * 1000,
                                        HighOscillator = (uint)hiOsc * 1000
                                      };
        DiseqcSatelliteSettings sSettings = new DiseqcSatelliteSettings
                                              {
                                                ToneBurstEnabled = 0,
                                                Diseq10Selection = LNB_Source.LNBSourceNotSet,
                                                Diseq11Selection = DiseqC11Switches.Switch_NOT_SET,
                                                Enabled = 0
                                              };
        hr = _interfaceNetworkProvider.TuneDVBS(fSettings, dSettings, lSettings, sSettings);
      }
      if (channel is DVBCChannel)
      {
        DVBCChannel dvbcChannel = channel as DVBCChannel;
        FrequencySettings fSettings = new FrequencySettings
                                        {
                                          Multiplier = 1000,
                                          Frequency = (uint)dvbcChannel.Frequency,
                                          Bandwidth = (uint)undefinedValue,
                                          Polarity = Polarisation.NotSet,
                                          Range = (uint)undefinedValue
                                        };
        DigitalDemodulatorSettings dSettings = new DigitalDemodulatorSettings
                                                 {
                                                   InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
                                                   InnerFECMethod = FECMethod.MethodNotSet,
                                                   Modulation = dvbcChannel.ModulationType,
                                                   OuterFECMethod = FECMethod.MethodNotSet,
                                                   OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
                                                   SpectralInversion = SpectralInversion.NotSet,
                                                   SymbolRate = (uint)dvbcChannel.SymbolRate
                                                 };

        hr = _interfaceNetworkProvider.TuneDVBC(fSettings, dSettings);
      }
      if (channel is ATSCChannel)
      {
        ATSCChannel atscChannel = channel as ATSCChannel;
        if (atscChannel.ModulationType == ModulationType.Mod256Qam)
        {
          FrequencySettings fSettings = new FrequencySettings
                                          {
                                            Multiplier = 1000,
                                            Frequency = (uint)atscChannel.Frequency,
                                            Bandwidth = (uint)undefinedValue,
                                            Polarity = Polarisation.NotSet,
                                            Range = (uint)undefinedValue
                                          };
          DigitalDemodulatorSettings dSettings = new DigitalDemodulatorSettings
                                                   {
                                                     InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
                                                     InnerFECMethod = FECMethod.MethodNotSet,
                                                     Modulation = atscChannel.ModulationType,
                                                     OuterFECMethod = FECMethod.MethodNotSet,
                                                     OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
                                                     SpectralInversion = SpectralInversion.NotSet,
                                                     SymbolRate = (uint)undefinedValue
                                                   };

          hr = _interfaceNetworkProvider.TuneATSC((uint)undefinedValue, fSettings, dSettings);
        }
        else
        {
          FrequencySettings fSettings = new FrequencySettings
                                          {
                                            Multiplier = (uint)undefinedValue,
                                            Frequency = (uint)undefinedValue,
                                            Bandwidth = (uint)undefinedValue,
                                            Polarity = Polarisation.NotSet,
                                            Range = (uint)undefinedValue
                                          };
          DigitalDemodulatorSettings dSettings = new DigitalDemodulatorSettings
                                                   {
                                                     InnerFECRate = BinaryConvolutionCodeRate.RateNotSet,
                                                     InnerFECMethod = FECMethod.MethodNotSet,
                                                     Modulation = atscChannel.ModulationType,
                                                     OuterFECMethod = FECMethod.MethodNotSet,
                                                     OuterFECRate = BinaryConvolutionCodeRate.RateNotSet,
                                                     SpectralInversion = SpectralInversion.NotSet,
                                                     SymbolRate = (uint)undefinedValue
                                                   };

          hr = _interfaceNetworkProvider.TuneATSC((uint)undefinedValue, fSettings, dSettings);
        }
      }
      Log.Log.WriteFile("dvb:Submit tunerequest done calling put_TuneRequest");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:SubmitTuneRequest  returns:0x{0:X} - {1}{2}", hr, HResult.GetDXErrorString(hr),
                          HResult.GetDXErrorString(hr));
        //remove subchannel.
        /*if (newSubChannel)
            {
            _mapSubChannels.Remove(subChannelId);
            }*/
        throw new TvException("Unable to tune to channel");
      }
    }

    /// <summary>
    /// this method gets the signal statistics interfaces from the bda tuner device
    /// and stores them in _tunerStatistics
    /// </summary>
    protected void GetTunerSignalStatistics()
    {
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("dvb: GetTunerSignalStatistics()");
      //no tuner filter? then return;
      _tunerStatistics = new List<IBDA_SignalStatistics>();
      if (_filterTuner == null)
      {
        Log.Log.Error("dvb: could not get IBDA_Topology since no tuner device");
        return;
      }
      //get the IBDA_Topology from the tuner device
      //Log.Log.WriteFile("dvb: get IBDA_Topology");
      IBDA_Topology topology = _filterTuner as IBDA_Topology;
      if (topology == null)
      {
        Log.Log.Error("dvb: could not get IBDA_Topology from tuner");
        return;
      }
      //get the NodeTypes from the topology
      //Log.Log.WriteFile("dvb: GetNodeTypes");
      int nodeTypeCount;
      int[] nodeTypes = new int[33];
      Guid[] guidInterfaces = new Guid[33];
      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        Log.Log.Error("dvb: FAILED could not get node types from tuner:0x{0:X}", hr);
        return;
      }
      if (nodeTypeCount == 0)
      {
        Log.Log.Error("dvb: FAILED could not get any node types");
      }
      Guid GuidIBDA_SignalStatistic = new Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338");
      //for each node type
      //Log.Log.WriteFile("dvb: got {0} node types", nodeTypeCount);
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object objectNode;
        int numberOfInterfaces;
        hr = topology.GetNodeInterfaces(nodeTypes[i], out numberOfInterfaces, 32, guidInterfaces);
        if (hr != 0)
        {
          Log.Log.Error("dvb: FAILED could not GetNodeInterfaces for node:{0} 0x:{1:X}", i, hr);
        }
        hr = topology.GetControlNode(0, 1, nodeTypes[i], out objectNode);
        if (hr != 0)
        {
          Log.Log.Error("dvb: FAILED could not GetControlNode for node:{0} 0x:{1:X}", i, hr);
          return;
        }
        //and get the final IBDA_SignalStatistics
        for (int iface = 0; iface < numberOfInterfaces; iface++)
        {
          if (guidInterfaces[iface] == GuidIBDA_SignalStatistic)
          {
            //Log.Write(" got IBDA_SignalStatistics on node:{0} interface:{1}", i, iface);
            _tunerStatistics.Add((IBDA_SignalStatistics)objectNode);
          }
        }
      }
      if (_conditionalAccess != null)
      {
        if (_conditionalAccess.AllowedToStopGraph == false)
        {
          RunGraph(-1);
        }
      }
      return;
    }

    ///<summary>
    /// Checks if the tuner is locked in and a sginal is present
    ///</summary>    
    public override void LockInOnSignal()
    {
      //UpdateSignalQuality(true);
      bool isLocked = false;
      DateTime timeStart = DateTime.Now;
      TimeSpan ts = timeStart - timeStart;
      while (!isLocked && ts.TotalSeconds < _parameters.TimeOutTune)
      {
        foreach (IBDA_SignalStatistics stat in _tunerStatistics) 
        {
          try
          {
            if (_cancelTune)
            {
              Log.Log.WriteFile("dvb:  LockInOnSignal tune cancelled");
              throw new TvExceptionTuneCancelled();
            }
            stat.get_SignalLocked(out isLocked);
            if (isLocked)
            {
              break;
            }
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalLocked() locked :{0}", ex);
          }
        }
        if (!isLocked)
        {
          ts = DateTime.Now - timeStart;
          Log.Log.WriteFile("dvb:  LockInOnSignal waiting 20ms");
          System.Threading.Thread.Sleep(20);
        }
      }      

      if (!isLocked)
      {
        Log.Log.WriteFile("dvb:  LockInOnSignal could not lock onto channel - no signal or bad signal");        
        throw new TvExceptionNoSignal("Unable to tune to channel - no signal");
      }
      Log.Log.WriteFile("dvb:  LockInOnSignal ok");
    }

    protected virtual bool ShouldWaitForSignal()
    {
      return true;
      //default behaviour is nothing
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="subChannel"></param>
    public void CancelTune(int subChannel)
    {
      _cancelTune = true;
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        var dvbChannel = _mapSubChannels[subChannel] as TvDvbChannel;
        if (dvbChannel != null)
        {
          dvbChannel.CancelTune();
        }
      }
    }

    //protected Dictionary<int, TvDvbChannel> _mapSubChannels;
    /// <summary>
    /// Methods which starts the graph
    /// </summary>
    public override void RunGraph(int subChannel)
    {
      Log.Log.Info("RunGraph");
      bool graphRunning = GraphRunning();

      if (_mapSubChannels.ContainsKey(subChannel))
      {
        if (graphRunning)
        {
          LockInOnSignal();          
        }
        _mapSubChannels[subChannel].AfterTuneEvent -= new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
        _mapSubChannels[subChannel].AfterTuneEvent += new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
        _mapSubChannels[subChannel].OnGraphStart();
      }

      if (graphRunning)
      {
        return;
      }
      Log.Log.Info("dvb:  RunGraph");
      int hr = ((IMediaControl)_graphBuilder).Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("dvb:  RunGraph returns: 0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }

      if (!ShouldWaitForSignal())
      {
        return;
      }

      //GetTunerSignalStatistics();
      _epgGrabbing = false;
      if (_mapSubChannels.ContainsKey(subChannel))
      {
        LockInOnSignal();        
        _mapSubChannels[subChannel].AfterTuneEvent -= new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
        _mapSubChannels[subChannel].AfterTuneEvent += new BaseSubChannel.OnAfterTuneDelegate(OnAfterTuneEvent);
        _mapSubChannels[subChannel].OnGraphStarted();
      }
    }

    /// <summary>
    /// Methods which pauses the graph
    /// </summary>
    public override void PauseGraph()
    {
      Log.Log.WriteFile("dvb:PauseGraph called");
      if (!CheckThreadId())
        return;
      _epgGrabbing = false;
      _isScanning = false;
      FreeAllSubChannels();
      if (_mdplugs != null)
      {
        _mdplugs.FreeAllChannels();
      }
      if (_graphBuilder == null)
        return;

      FilterState state;
      ((IMediaControl)_graphBuilder).GetState(10, out state);
      if (state != FilterState.Running)
      {
        Log.Log.WriteFile("dvb:PauseGraph filterstate already paused, returning.");
        return;
      }
      Log.Log.WriteFile("dvb:PauseGraph");
      int hr = ((IMediaControl)_graphBuilder).Pause();
      if (hr < 0 || hr > 1)
      {
        Log.Log.Error("dvb:PauseGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to pause graph");
      }
      _graphState = GraphState.Created;
    }

    public abstract bool CanTune(IChannel channel);

    /// <summary>
    /// Methods which stops the graph
    /// </summary>
    public override void StopGraph()
    {
      Log.Log.WriteFile("dvb:StopGraph called");
      if (!CheckThreadId())
        return;
      _epgGrabbing = false;
      _isScanning = false;
      FreeAllSubChannels();
      if (_mdplugs != null)
      {
        _mdplugs.FreeAllChannels();
      }
      if (_graphBuilder == null)
        return;
      if (_conditionalAccess.AllowedToStopGraph)
      {
        FilterState state;
        ((IMediaControl)_graphBuilder).GetState(10, out state);
        if (state == FilterState.Stopped)
        {
          Log.Log.WriteFile("dvb:StopGraph filterstate already stopped, returning.");
          return;
        }
        Log.Log.WriteFile("dvb:StopGraph");
        int hr = ((IMediaControl)_graphBuilder).Stop();
        if (hr < 0 || hr > 1)
        {
          Log.Log.Error("dvb:StopGraph returns:0x{0:X}", hr);
          throw new TvException("Unable to stop graph");
        }
        _conditionalAccess.OnStopGraph();
        // *** this should be removed when solution for graph start problem exists
        if (DebugSettings.ResetGraph)
          Decompose();
        else
          _graphState = GraphState.Created;
        // ***
      }
      else
      {
        int hr = ((IMediaControl)_graphBuilder).Stop();
        Log.Log.WriteFile("dvb:StopGraph - conditionalAccess.AllowedToStopGraph = false");
        if (hr < 0 || hr > 1)
        {
          Log.Log.Error("dvb:StopGraph returns:0x{0:X}", hr);
          throw new TvException("Unable to stop graph");
        }
        _graphState = GraphState.Created;
      }
    }

    /// <summary>
    /// This method adds the bda network provider filter to the graph
    /// </summary>
    protected void AddNetworkProviderFilter(Guid networkProviderClsId)
    {
      Log.Log.WriteFile("dvb:AddNetworkProviderFilter");
      if (useInternalNetworkProvider)
      {
        const string networkProviderName = "MediaPortal Network Provider";
        Guid internalNetworkProviderClsId = new Guid("{D7D42E5C-EB36-4aad-933B-B4C419429C98}");
        Log.Log.WriteFile("dvb:Add {0}", networkProviderName);
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, internalNetworkProviderClsId,
                                                                     networkProviderName);
        _interfaceNetworkProvider = (IDvbNetworkProvider)_filterNetworkProvider;
        string hash = TvCardCollection.GetHash(DevicePath);
        _interfaceNetworkProvider.ConfigureLogging(TvCardCollection.GetFileName(DevicePath), hash,
                                                   LogLevelOption.Debug);
        return;
      }
      _isATSC = false;
      _managedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
      Log.Log.WriteFile("dvb:AddNetworkProviderFilter");
      TvBusinessLayer layer = new TvBusinessLayer();
      Card c = layer.GetCardByDevicePath(DevicePath);
      // generic network provider Guid.
      Guid genProviderClsId = new Guid("{B2F3A67C-29DA-4C78-8831-091ED509A475}");
      // First test if the Generic Network Provider is available (only on MCE 2005 + Update Rollup 2)
      // only if it is used in db, we need to change it
      // This check is because, "Genric network provider" is default value in Db
      // And we need to check if it's even installed.
      // If not, Then it must be changed automatically, to Network provider
      // based on Card type. It prevents users to run into problems
      // If Generic network provider is not available in Os.
      if (((TvDatabase.DbNetworkProvider)c.netProvider) == TvDatabase.DbNetworkProvider.Generic)
      {
        if (!FilterGraphTools.IsThisComObjectInstalled(genProviderClsId))
        {
          // it's not and we have it as default
          // change it per devtype.
          if (networkProviderClsId == typeof (DVBTNetworkProvider).GUID)
          {
            c.netProvider = (int)TvDatabase.DbNetworkProvider.DVBT;
            c.Persist();
          }
          else if (networkProviderClsId == typeof (DVBSNetworkProvider).GUID)
          {
            c.netProvider = (int)TvDatabase.DbNetworkProvider.DVBS;
            c.Persist();
          }
          else if (networkProviderClsId == typeof (ATSCNetworkProvider).GUID)
          {
            c.netProvider = (int)TvDatabase.DbNetworkProvider.ATSC;
            c.Persist();
          }
          else if (networkProviderClsId == typeof (DVBCNetworkProvider).GUID)
          {
            c.netProvider = (int)TvDatabase.DbNetworkProvider.DVBC;
            c.Persist();
          }
        }
      }

      // Final selecion for Network provider based on user selection.
      String NetworkProviderName = String.Empty;
      switch ((TvDatabase.DbNetworkProvider)c.netProvider)
      {
        case DbNetworkProvider.DVBT:
          NetworkProviderName = "DVBT Network Provider";
          break;
        case DbNetworkProvider.DVBS:
          NetworkProviderName = "DVBS Network Provider";
          break;
        case DbNetworkProvider.DVBC:
          NetworkProviderName = "DVBC Network Provider";
          break;
        case DbNetworkProvider.ATSC:
          _isATSC = true;
          NetworkProviderName = "ATSC Network Provider";
          break;
        case DbNetworkProvider.Generic:
          NetworkProviderName = "Generic Network Provider";
          networkProviderClsId = genProviderClsId;
          break;
        default:
          Log.Log.Error("dvb:This application doesn't support this Tuning Space");
          // Tuning Space can also describe Analog TV but this application don't support them
          throw new TvException("This application doesn't support this Tuning Space");
      }
      Log.Log.WriteFile("dvb:Add {0}", NetworkProviderName);
      _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId,
                                                                   NetworkProviderName);
    }

    /// <summary>
    /// Checks if the WinTV USB CI module is installed
    /// if so it adds it to the directshow graph
    /// in the following way:
    /// [Network Provider]->[Tuner Filter]->[Capture Filter]->[WinTvCI Filter]
    /// alternaively like this:
    /// [Network Provider]->[Tuner Filter]->[WinTvCI Filter]
    /// </summary>
    /// <param name="lastFilter">A reference to the last filter.</param>
    protected void AddWinTvCIModule(ref IBaseFilter lastFilter)
    {
      //check if the hauppauge wintv usb CI module is installed
      DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.AMKSCapture);
      DsDevice usbWinTvDevice = null;
      int hr;
      //Log.Log.WriteFile("AddWinTvCIModule: capDevices {0}", capDevices.Length);
      for (int capIndex = 0; capIndex < capDevices.Length; capIndex++)
      {
        if (capDevices[capIndex].Name != null)
        {
          //Log.Log.WriteFile("AddWinTvCIModule: {0}", capDevices[capIndex].Name.ToLower());
          if (capDevices[capIndex].Name.ToUpperInvariant() == "WINTVCIUSBBDA SOURCE")
          {
            if (false == DevicesInUse.Instance.IsUsed(capDevices[capIndex]))
            {
              usbWinTvDevice = capDevices[capIndex];
              break;
            }
          }
        }
      }
      if (usbWinTvDevice == null)
      {
        Log.Log.Info("dvb:  WinTv CI module not detected.");
        return;
      }
      //wintv ci usb module found
      Log.Log.Info("dvb:  WinTv CI module detected");

      //add logic to check if WinTV device should be built with this DVB graph.
      TvBusinessLayer layer = new TvBusinessLayer();
      int winTvTunerCardId = Int32.Parse(layer.GetSetting("winTvCiTuner", "-1").Value);
      if (winTvTunerCardId != this._cardId)
      {
        Log.Log.Info("dvb:  WinTv CI module not assigned to card: {0}", _tunerDevice.Name);
        return;
      }
      Log.Log.Info("dvb:  Adding WinTv CI to graph");

      //add filter to graph
      IBaseFilter tmpCiFilter;
      try
      {
        hr = _graphBuilder.AddSourceFilterForMoniker(usbWinTvDevice.Mon, null, usbWinTvDevice.Name, out tmpCiFilter);
      }
      catch (Exception)
      {
        Log.Log.Info("dvb:  failed to add WinTv CI filter to graph");
        return;
      }
      if (hr != 0)
      {
        //cannot add filter to graph...
        Log.Log.Info("dvb:  failed to add WinTv CI filter to graph");
        if (tmpCiFilter != null)
        {
          //release WinTV CI resources& remove filter & render graph without it.
          winTvCiHandler.Shutdown();
          _graphBuilder.RemoveFilter(tmpCiFilter);
          Release.ComObject("WintvUsbCI module", tmpCiFilter);
        }
        return;
      }
      //Check if WinTV CI is plugged in to USB port if not remove filter from graph.
      winTvCiHandler = new WinTvCiModule(tmpCiFilter);
      int winTVCIStatus = winTvCiHandler.Init();
      //Log.Log.Info("WinTVCI: Init() returned: {0}", winTVCIStatus);
      if (winTVCIStatus != (int)HResult.Serverity.Success)
      {
        //release WinTV CI resources& remove filter & render graph without it.
        winTvCiHandler.Shutdown();
        _graphBuilder.RemoveFilter(tmpCiFilter);
        Release.ComObject("WintvUsbCI module", tmpCiFilter);
        Log.Log.Info("dvb:  WinTv CI not plugged in or driver not installed correctly!");
        return;
      }
      //WinTV-CI tray icon no longer required as it is now fully native supported
      //now render ..->[WinTv USB]
      Log.Log.Info("dvb:  Render ...->[WinTvUSB]");
      hr = _capBuilder.RenderStream(null, null, lastFilter, null, tmpCiFilter);
      if (hr != 0)
      {
        Log.Log.Error("dvb:  Render ...->[WinTvUSB] failed");
        winTvCiHandler.Shutdown();
        _graphBuilder.RemoveFilter(tmpCiFilter);
        Release.ComObject("WintvUsbCI module", tmpCiFilter);
        return;
      }
      _filterWinTvUsb = tmpCiFilter;
      _deviceWinTvUsb = usbWinTvDevice;
      DevicesInUse.Instance.Add(usbWinTvDevice);
      Log.Log.WriteFile("dvb:  Setting lastFilter to WinTV CI");
      lastFilter = _filterWinTvUsb;
      return;
    }

    /// <summary>
    /// Checks if the DigitalDevices CI module is installed
    /// if so it adds it to the directshow graph
    /// in the following way:
    /// [Network Provider]->[Tuner Filter]->[Capture Filter]->[Digital Devices Common Interface]->[InfTee]
    /// alternaively like this:
    /// [Network Provider]->[Tuner Filter]->[Digital Devices Common Interface]->[InfTee]
    /// </summary>
    /// <param name="lastFilter">A reference to the last filter.</param>
    /// <returns>
    /// true if hardware found and graph building succeeded, else false
    /// </returns>
    protected bool AddDigitalDevicesCIModule(ref IBaseFilter lastFilter)
    {
      FilterInfo pInfo;
      IBaseFilter tmpCiFilter = null;
      String CiDeviceName = String.Empty;
      bool filterSuccess = false;

      lastFilter.QueryFilterInfo(out pInfo);
      //Log.Log.Debug(pInfo.achName);

      if (_captureDevice == null)
        return false;

      if (!_captureDevice.DevicePath.ToLowerInvariant().Contains("fbca-11de-b16f-000000004d56"))
        return false;

      Log.Log.WriteFile("dvb:  DigitalDevices CI: try to connect [demux]");
      IPin lastFilterOutputPin = null;
      IPin demuxPinIn = null;
      IBaseFilter tmpDemux = null;
      try
      {
        tmpDemux = (IBaseFilter)new MPEG2Demultiplexer();
        _graphBuilder.AddFilter(tmpDemux, "MPEG2-Demultiplexer");

        lastFilterOutputPin = DsFindPin.ByDirection(lastFilter, PinDirection.Output, 0);
        demuxPinIn = DsFindPin.ByDirection(tmpDemux, PinDirection.Input, 0);
        
        // If connection to Demux is possible, CI filter cannot be put between.
        // this test removes a 30 .. 45 second delay when the graphbuilder tries to 
        // render Capture->CI->Demux and CI is not enabled for this tuner.
        if (_graphBuilder.Connect(lastFilterOutputPin, demuxPinIn) == 0)
        {
          Log.Log.WriteFile("dvb:  DigitalDevices CI: connection to [demux] successful, CI not available or configured for this tuner.");
          Log.Log.WriteFile("dvb:  DigitalDevices CI: disconnect [demux], HR:" + lastFilterOutputPin.Disconnect());
          return false;
        }
      }
      finally
      {
        Release.ComObject(pInfo.achName+" pin0", lastFilterOutputPin);
        Release.ComObject("tifdemux pinin", demuxPinIn);
        _graphBuilder.RemoveFilter(tmpDemux);
        Release.ComObject("tmpDemux", tmpDemux);
      }

      try
      {
        DsDevice[] capDevices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
        DsDevice DDCIDevice = null;
        for (int capIndex = 0; capIndex < capDevices.Length; capIndex++)
        {
          // DD components have a common device path part. 
          if (!(capDevices[capIndex].DevicePath.ToLowerInvariant().Contains("fbca-11de-b16f-000000004d56") &&
                capDevices[capIndex].Name.ToLowerInvariant().Contains("common interface")))
            continue;

          CiDeviceName = capDevices[capIndex].Name;

          //try add filter to graph
          Log.Log.Info("dvb:  Adding {0} to graph", CiDeviceName);
          if (
            _graphBuilder.AddSourceFilterForMoniker(capDevices[capIndex].Mon, null, capDevices[capIndex].Name,
                                                    out tmpCiFilter) == 0)
          {
            //DigitalDevices ci module found
            Log.Log.Info("dvb:  {0} detected", CiDeviceName);

            String filterName = tunerOnly ? "Tuner" : "Capture";
            //now render [Tuner/Capture]->[CI]
            Log.Log.Info("dvb:  Render [{0}]->[{1}]", filterName, CiDeviceName);
            if (_capBuilder.RenderStream(null, null, lastFilter, null, tmpCiFilter) != 0)
            {
              Log.Log.Info("dvb:  Render [{0}]->[{1}] failed", filterName, CiDeviceName);
              _graphBuilder.RemoveFilter(tmpCiFilter);
              continue;
            }
            // filter connected, device found
            if (!DevicesInUse.Instance.IsUsed(capDevices[capIndex]))
            {
              DDCIDevice = capDevices[capIndex];
              break;
            }
          }
          //cannot add filter to graph...
          Log.Log.Info("dvb:  failed to add {0} filter to graph, try to find more devices.", CiDeviceName);
          //there can be multiple CI devices, try next one
        }
        if (DDCIDevice == null)
          return false;

        Log.Log.WriteFile("dvb:  Setting lastFilter to Digital Devices CI");
        lastFilter = tmpCiFilter;
        _filterDigitalDevicesCI = tmpCiFilter;
        _deviceDigitalDevicesCI = DDCIDevice;

        filterSuccess = true;
        return true;
      }
      catch (Exception ex)
      {
        Log.Log.Error("dvb:   Error adding CI: {0}", ex.Message);
        filterSuccess = false;
        return filterSuccess;
      }
      finally
      {
        if (!filterSuccess && tmpCiFilter != null)
        {
          _graphBuilder.RemoveFilter(tmpCiFilter);
          Release.ComObject(CiDeviceName, tmpCiFilter);
        }
      }
    }

    /// <summary>
    /// Finds the correct bda tuner/capture filters and adds them to the graph
    /// Creates a graph like
    /// [NetworkProvider]->[Tuner]->[Capture]->[TsWriter]
    /// or if no capture filter is present:
    /// [NetworkProvider]->[Tuner]->[TsWriter]
    /// When a wintv ci module is found the graph will look like:
    /// [NetworkProvider]->[Tuner]->[Capture]->[WinTvCiUSB]->[TsWriter]
    /// or if no capture filter is present:
    /// [NetworkProvider]->[Tuner]->[WinTvCiUSB]->[TsWriter]
    /// </summary>
    /// <param name="device">Tuner device</param>
    protected void AddAndConnectBDABoardFilters(DsDevice device)
    {
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("dvb:AddAndConnectBDABoardFilters");
      _rotEntry = new DsROTEntry(_graphBuilder);
      Log.Log.WriteFile("dvb: find bda tuner");
      // Enumerate BDA Source filters category and found one that can connect to the network provider
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        if (device.DevicePath != devices[i].DevicePath)
          continue;
        if (DevicesInUse.Instance.IsUsed(devices[i]))
        {
          Log.Log.Info("dvb:  [Tuner]: {0} is being used by TVServer already or another application!", devices[i].Name);
          continue;
        }
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          continue;
        }
        if (hr != 0)
        {
          if (tmp != null)
          {
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("bda tuner", tmp);
          }
          continue;
        }
        //render [Network provider]->[Tuner]
        hr = _capBuilder.RenderStream(null, null, _filterNetworkProvider, null, tmp);
        if (hr == 0)
        {
          // Got it !
          _filterTuner = tmp;
          Log.Log.WriteFile("dvb:  using [Tuner]: {0}", devices[i].Name);
          _tunerDevice = devices[i];
          DevicesInUse.Instance.Add(devices[i]);
          Log.Log.WriteFile("dvb:  Render [Network provider]->[Tuner] OK");
          break;
        }
        // Try another...
        _graphBuilder.RemoveFilter(tmp);
        Release.ComObject("bda tuner", tmp);
      }
      // Assume we found a tuner filter...
      if (_filterTuner == null)
      {
        Log.Log.Info(
          "dvb:  A useable TV Tuner cannot be found! Either the device no longer exists or it's in use by another application!");
        Log.Log.Error("dvb:  No TVTuner installed");
        throw new TvException("No TVTuner installed");
      }

      Log.Log.WriteFile("dvb:  Setting lastFilter to Tuner filter");
      IBaseFilter lastFilter = _filterTuner;

      // Attempt to connect [Tuner]->[Capture]
      if (UseCaptureFilter())
      {
        Log.Log.WriteFile("dvb:  Find BDA receiver");
        Log.Log.WriteFile("dvb:  match Capture by Tuner device path");
        AddBDARendererToGraph(device, ref lastFilter);
        if (_filterCapture == null)
        {
          Log.Log.WriteFile("dvb:  Match by device path failed - trying alternative method");
          matchDevicePath = false;
          Log.Log.WriteFile("dvb:  match Capture filter by Tuner device connection");
          AddBDARendererToGraph(device, ref lastFilter);
        }
      }
      // Add additional filters after the capture/tuner device
      AddWinTvCIModule(ref lastFilter);
      AddDigitalDevicesCIModule(ref lastFilter);
      AddMdPlugs(ref lastFilter);
      // Now connect the required filters if not using the internal network provider
      if (!useInternalNetworkProvider)
      {
        // Connect the inf tee and demux to the last filter (saves one inf tee)
        ConnectMpeg2DemuxToInfTee(ref lastFilter);
        // Connect and add the filters to the demux
        AddBdaTransportFiltersToGraph();
      }
      // Render the last filter with the tswriter
      if (!ConnectTsWriter(lastFilter))
      {
        throw new TvExceptionGraphBuildingFailed("Graph building of DVB card failed");
      }
      Log.Log.WriteFile("dvb: Checking for hardware specific extensions");
      _conditionalAccess = new ConditionalAccess(_filterTuner, _filterTsWriter, _filterWinTvUsb, this);
    }

    /// <summary>
    /// Determine whether the tuner filter needs to connect to a capture filter,
    /// or whether it can be directly connected to an inf tee.
    /// </summary>
    /// <returns><c>true</c> if the tuner filter must be connected to a capture filter, otherwise <c>false</c></returns>
    private bool UseCaptureFilter()
    {
      // First: check the media types and formats on the tuner output
      // pin. The WDK specifies (http://msdn.microsoft.com/en-us/library/ff557729%28v=vs.85%29.aspx)
      // a set of formats that the tuner and capture filter output pins should set
      // to allow the tuner filter to connect to the capture filter, and the capture
      // filter to connect to the MPEG 2 Demultiplexor filter (see http://msdn.microsoft.com/en-us/library/dd390716%28v=vs.85%29.aspx).
      // Most tuners support the MEDIASUBTYPE_MPEG2_TRANSPORT media sub-type on their
      // output pin, while most capture filters use either MEDIASUBTYPE_MPEG2_TRANSPORT
      // or STATIC_KSDATAFORMAT_SUBTYPE_BDA_MPEG2_TRANSPORT (also known as MEDIASUBTYPE_BDA_MPEG2_TRANSPORT).
      bool useCaptureFilter = true;
      IPin pinOut = DsFindPin.ByDirection(_filterTuner, PinDirection.Output, 0);
      if (pinOut != null)
      {
        IEnumMediaTypes enumMedia;
        pinOut.EnumMediaTypes(out enumMedia);
        if (enumMedia != null)
        {
          int fetched;
          AMMediaType[] mediaTypes = new AMMediaType[21];
          enumMedia.Next(20, mediaTypes, out fetched);
          if (fetched > 0)
          {
            for (int i = 0; i < fetched; ++i)
            {
              if (mediaTypes[i].majorType == MediaType.Stream && mediaTypes[i].subType == MediaSubType.BdaMpeg2Transport &&
                  mediaTypes[i].formatType == FormatType.None)
              {
                Log.Log.WriteFile("dvb:  tuner filter has capture filter output");
                useCaptureFilter = false;
              }
              DsUtils.FreeAMMediaType(mediaTypes[i]);
            }
          }
          Release.ComObject("tuner filter output pin media types enum", enumMedia);
        }
      }
      Release.ComObject("tuner filter output pin", pinOut);
      if (!useCaptureFilter)
      {
        return false;
      }

      // Second: check whether the tuner filter implements the
      // capture/receiver filter interface (KSCATEGORY_BDA_RECEIVER_COMPONENT).
      // NOTE: not all filters expose this information.
      IKsTopologyInfo topologyInfo = _filterTuner as IKsTopologyInfo;
      if (topologyInfo != null)
      {
        int categoryCount;
        topologyInfo.get_NumCategories(out categoryCount);
        for (int i = 0; i < categoryCount; i++)
        {
          Guid guid;
          topologyInfo.get_Category(i, out guid);
          if (guid == FilterCategory.BDAReceiverComponentsCategory)
          {
            Log.Log.WriteFile("dvb:  tuner filter is also capture filter");
            return false;
          }
        }
      }

      // Finally: if the tuner is a Silicondust HDHomeRun then a capture
      // filter is not required. Neither of the other two detection methods
      // work as of 2011-02-11 (mm1352000).
      if (_tunerDevice.Name.Contains("Silicondust HDHomeRun Tuner"))
      {
        return false;
      }

      // Default: capture filter is required.
      return true;
    }

    /// <summary>
    /// Connects the ts writer to the last given filter
    /// </summary>
    /// <param name="lastFilter">Last filter in the graph</param>
    /// <returns>true, if successful ; false otherwise</returns>
    protected bool ConnectTsWriter(IBaseFilter lastFilter)
    {
      int hr;
      Log.Log.Info("dvb:  Render ..->[TsWriter]");
      //no wintv ci usb module found. Render [Tuner] or [Capture]->[InfTee]
      hr = _capBuilder.RenderStream(null, null, lastFilter, null, _filterTsWriter);
      return (hr == 0);
    }

    /// <summary>
    /// Add MD Plugs
    /// </summary>
    /// <param name="lastFilter">Last Filter in the graph</param>
    protected void AddMdPlugs(ref IBaseFilter lastFilter)
    {
      int hr = -1;
      if (_cardType == CardType.DvbIP)
      {
        _mdplugs = MDPlugs.Create(Name, DevicePath);
      }
      else
      {
        DsDevice dv = _captureDevice ?? _tunerDevice;
        string DisplayMoniker;
        dv.Mon.GetDisplayName(null, null, out DisplayMoniker);
        _mdplugs = MDPlugs.Create(dv.Name, DisplayMoniker);
      }
      if (_mdplugs != null)
      {
        _infTeeSecond = (IBaseFilter)new InfTee();
        hr = _graphBuilder.AddFilter(_infTeeSecond, "Inf Tee Second");
        if (hr != 0)
        {
          Log.Log.Error("dvb:Add second InfTee returns:0x{0:X}", hr);
          throw new TvException("Unable to add  second InfTee");
        }
        Log.Log.WriteFile("dvb:  Render ...->[inftee] second ");
        hr = _capBuilder.RenderStream(null, null, lastFilter, null, _infTeeSecond);
        if (hr != 0)
        {
          Log.Log.Error("dvb:Unable to connect InfTee second returns:0x{0:X}", hr);
          throw new TvExceptionGraphBuildingFailed("Could not connect InfTee second Filter to last Filter");
        }
        lastFilter = _infTeeSecond;
        _mdplugs.Connectmdapifilter(_graphBuilder, ref lastFilter);
      }
    }

    /// <summary>
    /// adds the BDA renderer filter to the graph by elimination
    /// then tries to match tuner &amp; render filters if successful then connects them.
    /// </summary>
    /// <param name="device">Tuner device</param>
    /// <param name="currentLastFilter">The current last filter if we add multiple captures</param>
    protected void AddBDARendererToGraph(DsDevice device, ref IBaseFilter currentLastFilter)
    {
      if (!CheckThreadId())
        return;
      if (_filterCapture != null)
        return;
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
      const string guidBdaMPEFilter = @"\{8e60217d-a2ee-47f8-b0c5-0f44c55f66dc}";
      const string guidBdaSlipDeframerFilter = @"\{03884cb6-e89a-4deb-b69e-8dc621686e6a}";
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].DevicePath.ToUpperInvariant().IndexOf(guidBdaMPEFilter.ToUpperInvariant()) >= 0)
          continue;
        if (devices[i].DevicePath.ToUpperInvariant().IndexOf(guidBdaSlipDeframerFilter.ToUpperInvariant()) >= 0)
          continue;
        IBaseFilter tmp;
        const string deviceIdDelimter = @"#{";
        Log.Log.WriteFile("dvb:  -{0}", devices[i].Name);
        //Make sure the BDA Receiver Component is on the same physical device as the BDA Source Filter.
        //This is done by checking the DeviceId and DeviceInstance part of the DevicePath.
        if (matchDevicePath)
        {
          int indx1 = device.DevicePath.IndexOf(deviceIdDelimter);
          int indx2 = devices[i].DevicePath.IndexOf(deviceIdDelimter);
          if (indx1 < 0 || indx2 < 0)
          {
            continue;
          }

          if (device.DevicePath.Remove(indx1) != devices[i].DevicePath.Remove(indx2))
          {
            continue;
          }
        }
        if (DevicesInUse.Instance.IsUsed(devices[i]))
          continue;
        int hr;
        try
        {
          hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out tmp);
        }
        catch (Exception)
        {
          continue;
        }
        if (hr != 0)
        {
          if (tmp != null)
          {
            Log.Log.Error("dvb:  Failed to add bda receiver: {0}. Is it in use?", devices[i].Name);
            _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("bda receiver", tmp);
          }
          continue;
        }
        //render [Tuner]->[Capture]
        hr = _capBuilder.RenderStream(null, null, _filterTuner, null, tmp);
        if (hr == 0)
        {
          Log.Log.WriteFile("dvb:  Render [Tuner]->[Capture] AOK");
          // render [Capture]->[Inf Tee]
          _filterCapture = tmp;
          _captureDevice = devices[i];
          DevicesInUse.Instance.Add(devices[i]);
          Log.Log.WriteFile("dvb:  Setting lastFilter to Capture device");
          currentLastFilter = _filterCapture;
          break;
        }
        // Try another...
        Log.Log.WriteFile("dvb:  Looking for another bda receiver...");
        _graphBuilder.RemoveFilter(tmp);
        Release.ComObject("bda receiver", tmp);
      }
    }

    /// <summary>
    /// adds the mpeg-2 demultiplexer filter and inftee filter to the graph
    /// </summary>
    protected void AddMpeg2DemuxerToGraph()
    {
      if (!CheckThreadId())
        return;
      if (_filterMpeg2DemuxTif != null)
        return;
      Log.Log.WriteFile("dvb:Add MPEG2 Demultiplexer filter");
      _filterMpeg2DemuxTif = (IBaseFilter)new MPEG2Demultiplexer();
      int hr = _graphBuilder.AddFilter(_filterMpeg2DemuxTif, "MPEG2-Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddMpeg2DemuxerTif returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer for tif");
      }
    }

    /// <summary>
    /// Connects the mpeg2 demuxers to the inf tee filter.
    /// </summary>
    protected void ConnectMpeg2DemuxToInfTee(ref IBaseFilter lastFilter)
    {
      Log.Log.WriteFile("dvb:add Inf Tee filter");
      _infTeeMain = (IBaseFilter)new InfTee();
      int hr = _graphBuilder.AddFilter(_infTeeMain, "Inf Tee");
      if (hr != 0)
      {
        Log.Log.Error("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
      Log.Log.WriteFile("dvb:  Render ...->[inftee]");
      hr = _capBuilder.RenderStream(null, null, lastFilter, null, _infTeeMain);
      if (hr != 0)
      {
        Log.Log.Error("dvb:Unable to connect InfTee returns:0x{0:X}", hr);
        throw new TvExceptionGraphBuildingFailed("Could not connect InfTee Filter to last Filter");
      }
      Log.Log.WriteFile("dvb:  Setting lastFilter to Inf Tee");
      lastFilter = _infTeeMain;
      //connect the [inftee main] -> [TIF MPEG2 Demultiplexer]
      Log.Log.WriteFile("dvb:  Render [inftee]->[demux]");
      IPin mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 0);
      IPin demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
      Release.ComObject("maintee pin0", mainTeeOut);
      Release.ComObject("tifdemux pinin", demuxPinIn);
      if (hr != 0)
      {
        Log.Log.Error("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
    }

    /// <summary>
    /// Gets the video audio pins.
    /// </summary>
    protected void AddTsWriterFilterToGraph()
    {
      if (_filterTsWriter == null)
      {
        Log.Log.WriteFile("dvb:  Add Mediaportal TsWriter filter");
        _filterTsWriter = (IBaseFilter)new MpTsAnalyzer();
        int hr = _graphBuilder.AddFilter(_filterTsWriter, "MediaPortal Ts Analyzer");
        if (hr != 0)
        {
          Log.Log.Error("dvb:  Add main Ts Analyzer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Ts Analyzer filter");
        }
        _interfaceChannelScan = (ITsChannelScan)_filterTsWriter;
        _interfaceEpgGrabber = (ITsEpgScanner)_filterTsWriter;
        _interfaceChannelLinkageScanner = (ITsChannelLinkageScanner)_filterTsWriter;
      }
    }

    /// <summary>
    /// adds the BDA Transport Information Filter  and the
    /// MPEG-2 sections and tables filter to the graph 
    /// </summary>
    protected void AddBdaTransportFiltersToGraph()
    {
      if (!CheckThreadId())
        return;
      Log.Log.WriteFile("dvb:  AddTransportStreamFiltersToGraph");
      int hr;
      // Add two filters needed in a BDA graph
      DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        if (String.Compare(devices[i].Name, "BDA MPEG2 Transport Information Filter", true) == 0)
        {
          Log.Log.Write("    add BDA MPEG2 Transport Information Filter filter");
          try
          {
            hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out _filterTIF);
            if (hr != 0)
            {
              Log.Log.Error("    unable to add BDA MPEG2 Transport Information Filter filter:0x{0:X}", hr);
              return;
            }
          }
          catch (Exception)
          {
            Log.Log.Error("    unable to add BDA MPEG2 Transport Information Filter filter");
          }
          continue;
        }
      }
      if (_filterTIF == null)
      {
        Log.Log.Error("BDA MPEG2 Transport Information Filter not found");
        return;
      }
      IPin pinInTif = DsFindPin.ByDirection(_filterTIF, PinDirection.Input, 0);
      if (pinInTif == null)
      {
        Log.Log.Error("    unable to find input pin of TIF");
        return;
      }
      if (_filterMpeg2DemuxTif == null)
      {
        Log.Log.Error("   _filterMpeg2DemuxTif==null");
        return;
      }
      //IPin pinInSec = DsFindPin.ByDirection(_filterSectionsAndTables, PinDirection.Input, 0);
      Log.Log.WriteFile("    pinTif:{0}", FilterGraphTools.LogPinInfo(pinInTif));
      //Log.Log.WriteFile("    pinSec:{0}", FilterGraphTools.LogPinInfo(pinInSec));
      //connect tif
      Log.Log.WriteFile("    Connect tif and mpeg2 sections and tables");
      IEnumPins enumPins;
      _filterMpeg2DemuxTif.EnumPins(out enumPins);
      if (enumPins == null)
      {
        Log.Log.Error("   _filterMpeg2DemuxTif.enumpins returned null");
        return;
      }
      bool tifConnected = false;
      //bool mpeg2SectionsConnected = false;
      int pinNr = 0;
      while (true)
      {
        pinNr++;
        PinDirection pinDir;
        AMMediaType[] mediaTypes = new AMMediaType[2];
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1)
          break;
        if (pins[0] == null)
          break;
        pins[0].QueryDirection(out pinDir);
        if (pinDir == PinDirection.Input)
        {
          Release.ComObject("mpeg2 demux pin" + pinNr, pins[0]);
          continue;
        }
        IEnumMediaTypes enumMedia;
        pins[0].EnumMediaTypes(out enumMedia);
        if (enumMedia != null)
        {
          enumMedia.Next(1, mediaTypes, out fetched);
          Release.ComObject("IEnumMedia", enumMedia);
          if (fetched == 1 && mediaTypes[0] != null)
          {
            if (mediaTypes[0].majorType == MediaType.Audio || mediaTypes[0].majorType == MediaType.Video)
            {
              //skip audio/video pins
              DsUtils.FreeAMMediaType(mediaTypes[0]);
              Release.ComObject("mpeg2 demux pin" + pinNr, pins[0]);
              continue;
            }
          }
          DsUtils.FreeAMMediaType(mediaTypes[0]);
        }
        if (tifConnected == false)
        {
          try
          {
            Log.Log.WriteFile("dvb:try tif:{0}", FilterGraphTools.LogPinInfo(pins[0]));
            hr = _graphBuilder.Connect(pins[0], pinInTif);
            if (hr == 0)
            {
              Log.Log.WriteFile("    tif connected");
              tifConnected = true;
              Release.ComObject("mpeg2 demux pin" + pinNr, pins[0]);
              continue;
            }
            Log.Log.WriteFile("    tif not connected:0x{0:X}", hr);
          }
          catch (Exception ex)
          {
            Log.Log.WriteFile("Error while connecting TIF filter: {0}", ex);
          }
        }
        Release.ComObject("mpeg2 demux pin" + pinNr, pins[0]);
      }
      Release.ComObject("IEnumMedia", enumPins);
      Release.ComObject("TIF pin in", pinInTif);
      // Release.ComObject("mpeg2 sections&tables pin in", pinInSec);
      if (tifConnected == false)
      {
        Log.Log.Error("    unable to connect transport information filter");
        //throw new TvException("unable to connect transport information filter");
      }
    }

    /// <summary>
    /// Sends the hw pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    public virtual void SendHwPids(List<ushort> pids)
    {
      //if (System.IO.File.Exists("usehwpids.txt"))
      {
        if (_conditionalAccess != null)
        {
          //  _conditionalAccess.SendPids((DVBBaseChannel)_currentChannel, pids);
        }
        return;
      }
    }

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
      Decompose();
    }

    /// <summary>
    /// destroys the graph and cleans up any resources
    /// </summary>
    protected void Decompose()
    {
      if (_graphBuilder == null || !CheckThreadId())
        return;

      Log.Log.WriteFile("dvb:Decompose");
      if (_epgGrabbing)
      {
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          Log.Log.Epg("dvb:cancel epg->decompose");
          _epgGrabberCallback.OnEpgCancelled();
        }
        _epgGrabbing = false;
      }

      FreeAllSubChannels();
      Log.Log.WriteFile("  stop");
      // Decompose the graph

      int counter = 0, hr = 0;
      FilterState state = FilterState.Running;
      hr = ((IMediaControl)_graphBuilder).Stop();
      while (state != FilterState.Stopped)
      {
        System.Threading.Thread.Sleep(100);
        hr = ((IMediaControl)_graphBuilder).GetState(10, out state);
        counter++;
        if (counter >= 30)
        {
          if (state != FilterState.Stopped)
            Log.Log.Error("dvb:graph still running");
          break;
        }
      }

      //In case MDPlugs exists then close and release them
      if (_mdplugs != null)
      {
        Log.Log.Info("  Closing MDAPI Plugins");
        _mdplugs.Close();
        _mdplugs = null;
      }
      if (_conditionalAccess != null)
      {
        Log.Log.Info("  Disposing ConditionalAccess");
        _conditionalAccess.Dispose();
        _conditionalAccess = null;
      }

      Log.Log.WriteFile("  free...");
      _interfaceChannelScan = null;
      _interfaceEpgGrabber = null;
      _previousChannel = null;
      if (_filterMpeg2DemuxTif != null)
      {
        Release.ComObject("_filterMpeg2DemuxTif filter", _filterMpeg2DemuxTif);
        _filterMpeg2DemuxTif = null;
      }
      if (_filterNetworkProvider != null)
      {
        Release.ComObject("_filterNetworkProvider filter", _filterNetworkProvider);
        _filterNetworkProvider = null;
      }
      if (_infTeeMain != null)
      {
        Release.ComObject("main inftee filter", _infTeeMain);
        _infTeeMain = null;
      }
      if (_infTeeSecond != null)
      {
        Release.ComObject("second inftee filter", _infTeeSecond);
        _infTeeSecond = null;
      }
      if (_filterTuner != null)
      {
        while (Release.ComObject(_filterTuner) > 0)
          ;
        _filterTuner = null;
      }
      if (_filterCapture != null)
      {
        while (Release.ComObject(_filterCapture) > 0)
          ;
        _filterCapture = null;
      }
      if (_filterWinTvUsb != null)
      {
        Log.Log.Info("  Stopping WinTVCI module");
        winTvCiHandler.Shutdown();
        while (Release.ComObject(_filterWinTvUsb) > 0)
          ;
        _filterWinTvUsb = null;
      }
      if (_filterTIF != null)
      {
        Release.ComObject("TIF filter", _filterTIF);
        _filterTIF = null;
      }
      //if (_filterSectionsAndTables != null)
      //{
      //  Release.ComObject("secions&tables filter", _filterSectionsAndTables); _filterSectionsAndTables = null;
      //}
      Log.Log.WriteFile("  free pins...");
      if (_filterTsWriter as IBaseFilter != null)
      {
        Release.ComObject("TSWriter filter", _filterTsWriter);
        _filterTsWriter = null;
      }
      else
      {
        Log.Log.Debug("!!! Error releasing TSWriter filter (_filterTsWriter as IBaseFilter was null!)");
        _filterTsWriter = null;
      }
      Log.Log.WriteFile("  free graph...");
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
        _rotEntry = null;
      }
      if (_capBuilder != null)
      {
        Release.ComObject("capture builder", _capBuilder);
        _capBuilder = null;
      }
      if (_graphBuilder != null)
      {
        FilterGraphTools.RemoveAllFilters(_graphBuilder);
        Release.ComObject("graph builder", _graphBuilder);
        _graphBuilder = null;
      }
      Log.Log.WriteFile("  free devices...");
      if (_deviceWinTvUsb != null)
      {
        DevicesInUse.Instance.Remove(_deviceWinTvUsb);
        _deviceWinTvUsb = null;
      }
      if (_tunerDevice != null)
      {
        DevicesInUse.Instance.Remove(_tunerDevice);
        _tunerDevice = null;
      }
      if (_captureDevice != null)
      {
        DevicesInUse.Instance.Remove(_captureDevice);
        _captureDevice = null;
      }
      if (_tunerStatistics != null)
      {
        for (int i = 0; i < _tunerStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = _tunerStatistics[i];
          while (Release.ComObject(stat) > 0)
            ;
        }
        _tunerStatistics.Clear();
      }
      Log.Log.WriteFile("  decompose done...");
      _graphState = GraphState.Idle;
    }

    #endregion

    #endregion

    #region signal quality, level etc

    /// <summary>
    /// Updates the signal informations of the tv cards
    /// </summary>
    protected override void UpdateSignalQuality(bool force)
    {
      if (!force)
      {
        TimeSpan ts = DateTime.Now - _lastSignalUpdate;
        if (ts.TotalMilliseconds < 5000)
          return;
      }
      try
      {
        if (GraphRunning() == false)
        {
          //System.Diagnostics.Debugger.Launch();
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (CurrentChannel == null)
        {
          //System.Diagnostics.Debugger.Launch();
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (_filterNetworkProvider == null)
        {
          //System.Diagnostics.Debugger.Launch();
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (!CheckThreadId())
        {
          //System.Diagnostics.Debugger.Launch();
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        //Log.Log.WriteFile("dvb:UpdateSignalQuality");
        //if we dont have an IBDA_SignalStatistics interface then return
        if (_tunerStatistics == null)
        {
          //System.Diagnostics.Debugger.Launch();
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          //          Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
          return;
        }
        if (_tunerStatistics.Count == 0)
        {
          //System.Diagnostics.Debugger.Launch();
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          //          Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
          return;
        }
        bool isTunerLocked = false;
        long signalQuality = 0;
        long signalStrength = 0;

        //       Log.Log.Write("dvb:UpdateSignalQuality() count:{0}", _tunerStatistics.Count);
        for (int i = 0; i < _tunerStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = _tunerStatistics[i];
          //          Log.Log.Write("   dvb:  #{0} get locked",i );
          try
          {
            bool isLocked;
            //is the tuner locked?
            stat.get_SignalLocked(out isLocked);
            isTunerLocked |= isLocked;
            //  Log.Log.Write("   dvb:  #{0} isTunerLocked:{1}", i,isLocked);
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalLocked() locked :{0}", ex);
          }
          catch (Exception ex)
          {
            Log.Log.WriteFile("get_SignalLocked() locked :{0}", ex);
          }

          //          Log.Log.Write("   dvb:  #{0} get signalquality", i);
          try
          {
            int quality;
            //is a signal quality ok?
            stat.get_SignalQuality(out quality); //1-100
            if (quality > 0)
              signalQuality += quality;
            //   Log.Log.Write("   dvb:  #{0} signalQuality:{1}", i, quality);
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          catch (Exception ex)
          {
            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          //          Log.Log.Write("   dvb:  #{0} get signalstrength", i);
          try
          {
            int strength;
            //is a signal strength ok?
            stat.get_SignalStrength(out strength); //1-100
            if (strength > 0)
              signalStrength += strength;
            //    Log.Log.Write("   dvb:  #{0} signalStrength:{1}", i, strength);
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          catch (Exception ex)
          {
            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          //Log.Log.WriteFile("  dvb:#{0}  locked:{1} present:{2} quality:{3} strength:{4}", i, isLocked, isPresent, quality, strength);          
        }
        if (_tunerStatistics.Count > 0)
        {
          _signalQuality = (int)signalQuality / _tunerStatistics.Count;
          _signalLevel = (int)signalStrength / _tunerStatistics.Count;
        }
        _tunerLocked = isTunerLocked;

        _signalPresent = isTunerLocked;
      }
      finally
      {
        _lastSignalUpdate = DateTime.Now;
      }
    }

    /// <summary>
    /// updates the signal quality/level and tuner locked statusses
    /// </summary>    
    /// 
    protected override void UpdateSignalQuality()
    {
      UpdateSignalQuality(false);
    }

    //public bool SignalPresent()

    #endregion

    #region properties

    /// <summary>
    /// returns the ITsChannelScan interface for the graph
    /// </summary>
    public ITsChannelScan StreamAnalyzer
    {
      get { return _interfaceChannelScan; }
    }

    /// <summary>
    /// Activates / deactivates the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected override void UpdateEpgGrabber(bool value)
    {
      if (_epgGrabbing && value == false)
        _interfaceEpgGrabber.Reset();
    }

    /// <summary>
    /// Activates / deactivates the scanning
    /// </summary>
    protected override void OnScanning()
    {
      _epgGrabbing = false;
      if (_epgGrabberCallback != null && _epgGrabbing)
      {
        Log.Log.Epg("dvb:cancel epg->scanning");
        _epgGrabberCallback.OnEpgCancelled();
      }
    }

    #endregion

    #region Channel linkage handling

    private static bool SameAsPortalChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      return ((pChannel.NetworkId == lChannel.NetworkId) && (pChannel.TransportId == lChannel.NetworkId) &&
              (pChannel.ServiceId == lChannel.ServiceId));
    }

    private static bool IsNewLinkedChannel(PortalChannel pChannel, LinkedChannel lChannel)
    {
      bool bRet = true;
      foreach (LinkedChannel lchan in pChannel.LinkedChannels)
      {
        if ((lchan.NetworkId == lChannel.NetworkId) && (lchan.TransportId == lChannel.TransportId) &&
            (lchan.ServiceId == lChannel.ServiceId))
        {
          bRet = false;
          break;
        }
      }
      return bRet;
    }

    /// <summary>
    /// Starts scanning for linkage info
    /// </summary>
    public void StartLinkageScanner(BaseChannelLinkageScanner callback)
    {
      if (!CheckThreadId())
        return;

      _interfaceChannelLinkageScanner.SetCallBack(callback);
      _interfaceChannelLinkageScanner.Start();
    }

    /// <summary>
    /// Stops/Resets the linkage scanner
    /// </summary>
    public void ResetLinkageScanner()
    {
      _interfaceChannelLinkageScanner.Reset();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public List<PortalChannel> ChannelLinkages
    {
      get
      {
        try
        {
          uint channelCount;
          List<PortalChannel> portalChannels = new List<PortalChannel>();
          _interfaceChannelLinkageScanner.GetChannelCount(out channelCount);
          if (channelCount == 0)
            return portalChannels;
          for (uint i = 0; i < channelCount; i++)
          {
            ushort network_id = 0;
            ushort transport_id = 0;
            ushort service_id = 0;
            _interfaceChannelLinkageScanner.GetChannel(i, ref network_id, ref transport_id, ref service_id);
            PortalChannel pChannel = new PortalChannel();
            pChannel.NetworkId = network_id;
            pChannel.TransportId = transport_id;
            pChannel.ServiceId = service_id;
            uint linkCount;
            _interfaceChannelLinkageScanner.GetLinkedChannelsCount(i, out linkCount);
            if (linkCount > 0)
            {
              for (uint j = 0; j < linkCount; j++)
              {
                ushort nid = 0;
                ushort tid = 0;
                ushort sid = 0;
                IntPtr ptrName;
                _interfaceChannelLinkageScanner.GetLinkedChannel(i, j, ref nid, ref tid, ref sid, out ptrName);
                LinkedChannel lChannel = new LinkedChannel();
                lChannel.NetworkId = nid;
                lChannel.TransportId = tid;
                lChannel.ServiceId = sid;
                lChannel.Name = Marshal.PtrToStringAnsi(ptrName);
                if ((!SameAsPortalChannel(pChannel, lChannel)) && (IsNewLinkedChannel(pChannel, lChannel)))
                  pChannel.LinkedChannels.Add(lChannel);
              }
            }
            if (pChannel.LinkedChannels.Count > 0)
              portalChannels.Add(pChannel);
          }
          _interfaceChannelLinkageScanner.Reset();
          return portalChannels;
        }
        catch (Exception ex)
        {
          Log.Log.Write(ex);
          return new List<PortalChannel>();
        }
      }
    }

    #endregion

    #region epg & scanning

    /// <summary>
    /// checks if a received EPGChannel should be filtered from the resultlist
    /// </summary>
    /// <value></value>
    protected virtual bool FilterOutEPGChannel(EpgChannel epgChannel)
    {
      return false;
    }

    /// <summary>
    /// Start grabbing the epg
    /// </summary>
    public void GrabEpg(BaseEpgGrabber callback)
    {
      if (!CheckThreadId())
        return;
      _epgGrabberCallback = callback;
      Log.Log.Write("dvb:grab epg...");
      if (_interfaceEpgGrabber == null)
        return;
      _interfaceEpgGrabber.SetCallBack(callback);
      _interfaceEpgGrabber.GrabEPG();
      _interfaceEpgGrabber.GrabMHW();
      _epgGrabbing = true;
    }

    /// <summary>
    /// Start grabbing the epg while timeshifting
    /// </summary>
    public void GrabEpg()
    {
      if (_timeshiftingEPGGrabber.StartGrab())
        GrabEpg(_timeshiftingEPGGrabber);
    }

    /// <summary>
    /// Gets the UTC.
    /// </summary>
    /// <param name="val">The val.</param>
    /// <returns></returns>
    private static int getUTC(int val)
    {
      if ((val & 0xF0) >= 0xA0)
        return 0;
      if ((val & 0xF) >= 0xA)
        return 0;
      return ((val & 0xF0) >> 4) * 10 + (val & 0xF);
    }

    /// <summary>
    /// Aborts grabbing the epg
    /// </summary>
    public void AbortGrabbing()
    {
      Log.Log.Write("dvb:abort grabbing epg");
      if (_interfaceEpgGrabber != null)
        _interfaceEpgGrabber.AbortGrabbing();
      if (_timeshiftingEPGGrabber != null)
        _timeshiftingEPGGrabber.OnEpgCancelled();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public List<EpgChannel> Epg
    {
      get
      {
        //if (!CheckThreadId()) return null;
        try
        {
          bool dvbReady, mhwReady;
          _interfaceEpgGrabber.IsEPGReady(out dvbReady);
          _interfaceEpgGrabber.IsMHWReady(out mhwReady);
          if (dvbReady == false || mhwReady == false)
            return null;
          uint titleCount;
          uint channelCount;
          _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
          mhwReady = titleCount > 10;
          _interfaceEpgGrabber.GetEPGChannelCount(out channelCount);
          dvbReady = channelCount > 0;
          List<EpgChannel> epgChannels = new List<EpgChannel>();
          Log.Log.Epg("dvb:mhw ready MHW {0} titles found", titleCount);
          Log.Log.Epg("dvb:dvb ready.EPG {0} channels", channelCount);
          if (mhwReady)
          {
            _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
            for (int i = 0; i < titleCount; ++i)
            {
              uint id = 0;
              UInt32 programid = 0;
              uint transportid = 0, networkid = 0, channelnr = 0, channelid = 0, themeid = 0, PPV = 0, duration = 0;
              byte summaries = 0;
              uint datestart = 0, timestart = 0;
              uint tmp1 = 0, tmp2 = 0;
              IntPtr ptrTitle, ptrProgramName;
              IntPtr ptrChannelName, ptrSummary, ptrTheme;
              _interfaceEpgGrabber.GetMHWTitle((ushort)i, ref id, ref tmp1, ref tmp2, ref channelnr, ref programid,
                                               ref themeid, ref PPV, ref summaries, ref duration, ref datestart,
                                               ref timestart, out ptrTitle, out ptrProgramName);
              _interfaceEpgGrabber.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid,
                                                 out ptrChannelName);
              _interfaceEpgGrabber.GetMHWSummary(programid, out ptrSummary);
              _interfaceEpgGrabber.GetMHWTheme(themeid, out ptrTheme);
              string channelName = DvbTextConverter.Convert(ptrChannelName, "");
              string title = DvbTextConverter.Convert(ptrTitle, "");
              string summary = DvbTextConverter.Convert(ptrSummary, "");
              string theme = DvbTextConverter.Convert(ptrTheme, "");
              if (channelName == null)
                channelName = "";
              if (title == null)
                title = "";
              if (summary == null)
                summary = "";
              if (theme == null)
                theme = "";
              channelName = channelName.Trim();
              title = title.Trim();
              summary = summary.Trim();
              theme = theme.Trim();
              EpgChannel epgChannel = null;
              foreach (EpgChannel chan in epgChannels)
              {
                DVBBaseChannel dvbChan = (DVBBaseChannel)chan.Channel;
                if (dvbChan.NetworkId == networkid && dvbChan.TransportId == transportid &&
                    dvbChan.ServiceId == channelid)
                {
                  epgChannel = chan;
                  break;
                }
              }
              if (epgChannel == null)
              {
                DVBBaseChannel dvbChan = CreateChannel((int)networkid, (int)transportid, (int)channelid, channelName);
                epgChannel = new EpgChannel();
                epgChannel.Channel = dvbChan;
                //Log.Log.Epg("dvb: start filtering channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
                if (this.FilterOutEPGChannel(epgChannel) == false)
                {
                  //Log.Log.Epg("dvb: Not Filtered channel NID {0} TID {1} SID{2}", dvbChan.NetworkId, dvbChan.TransportId, dvbChan.ServiceId);
                  epgChannels.Add(epgChannel);
                }
              }
              uint d1 = datestart;
              uint m = timestart & 0xff;
              uint h1 = (timestart >> 16) & 0xff;
              DateTime dayStart = DateTime.Now;
              dayStart =
                dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
              int day = (int)dayStart.DayOfWeek;
              DateTime programStartTime = dayStart;
              int minVal = (int)((d1 - day) * 86400 + h1 * 3600 + m * 60);
              if (minVal < 21600)
                minVal += 604800;
              programStartTime = programStartTime.AddSeconds(minVal);
              EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
              EpgLanguageText epgLang = new EpgLanguageText("ALL", title, summary, theme, 0, "", -1);
              program.Text.Add(epgLang);
              epgChannel.Programs.Add(program);
            }
            for (int i = 0; i < epgChannels.Count; ++i)
            {
              epgChannels[i].Sort();
            }
            // free the epg infos in TsWriter so that the mem used gets released 
            _interfaceEpgGrabber.Reset();
            return epgChannels;
          }

          if (dvbReady)
          {
            ushort networkid = 0;
            ushort transportid = 0;
            ushort serviceid = 0;
            for (uint x = 0; x < channelCount; ++x)
            {
              _interfaceEpgGrabber.GetEPGChannel(x, ref networkid, ref transportid, ref serviceid);
              EpgChannel epgChannel = new EpgChannel();
              DVBBaseChannel chan = CreateChannel(networkid, transportid, serviceid, "");
              epgChannel.Channel = chan;
              uint eventCount;
              _interfaceEpgGrabber.GetEPGEventCount(x, out eventCount);
              for (uint i = 0; i < eventCount; ++i)
              {
                uint start_time_MJD, start_time_UTC, duration, languageCount;
                string title, description;
                IntPtr ptrGenre;
                int starRating;
                IntPtr ptrClassification;

                _interfaceEpgGrabber.GetEPGEvent(x, i, out languageCount, out start_time_MJD, out start_time_UTC,
                                                 out duration, out ptrGenre, out starRating, out ptrClassification);
                string genre = DvbTextConverter.Convert(ptrGenre, "");
                string classification = DvbTextConverter.Convert(ptrClassification, "");

                if (starRating < 1 || starRating > 7)
                  starRating = 0;

                int duration_hh = getUTC((int)((duration >> 16)) & 255);
                int duration_mm = getUTC((int)((duration >> 8)) & 255);
                int duration_ss = 0; //getUTC((int) (duration )& 255);
                int starttime_hh = getUTC((int)((start_time_UTC >> 16)) & 255);
                int starttime_mm = getUTC((int)((start_time_UTC >> 8)) & 255);
                int starttime_ss = 0; //getUTC((int) (start_time_UTC )& 255);

                if (starttime_hh > 23)
                  starttime_hh = 23;
                if (starttime_mm > 59)
                  starttime_mm = 59;
                if (starttime_ss > 59)
                  starttime_ss = 59;

                // DON'T ENABLE THIS. Some entries can be indeed >23 Hours !!!
                //if (duration_hh > 23) duration_hh = 23;
                if (duration_mm > 59)
                  duration_mm = 59;
                if (duration_ss > 59)
                  duration_ss = 59;

                // convert the julian date
                int year = (int)((start_time_MJD - 15078.2) / 365.25);
                int month = (int)((start_time_MJD - 14956.1 - (int)(year * 365.25)) / 30.6001);
                int day = (int)(start_time_MJD - 14956 - (int)(year * 365.25) - (int)(month * 30.6001));
                int k = (month == 14 || month == 15) ? 1 : 0;
                year += 1900 + k; // start from year 1900, so add that here
                month = month - 1 - k * 12;
                int starttime_y = year;
                int starttime_m = month;
                int starttime_d = day;
                if (year < 2000)
                  continue;

                try
                {
                  DateTime dtUTC = new DateTime(starttime_y, starttime_m, starttime_d, starttime_hh, starttime_mm,
                                                starttime_ss, 0);
                  DateTime dtStart = dtUTC.ToLocalTime();
                  if (dtStart < DateTime.Now.AddDays(-1) || dtStart > DateTime.Now.AddMonths(2))
                    continue;
                  DateTime dtEnd = dtStart.AddHours(duration_hh);
                  dtEnd = dtEnd.AddMinutes(duration_mm);
                  dtEnd = dtEnd.AddSeconds(duration_ss);
                  EpgProgram epgProgram = new EpgProgram(dtStart, dtEnd);
                  //EPGEvent newEvent = new EPGEvent(genre, dtStart, dtEnd);
                  for (int z = 0; z < languageCount; ++z)
                  {
                    uint languageId;
                    IntPtr ptrTitle;
                    IntPtr ptrDesc;
                    int parentalRating;
                    _interfaceEpgGrabber.GetEPGLanguage(x, i, (uint)z, out languageId, out ptrTitle, out ptrDesc,
                                                        out parentalRating);
                    //title = DvbTextConverter.Convert(ptrTitle,"");
                    //description = DvbTextConverter.Convert(ptrDesc,"");
                    string language = String.Empty;
                    language += (char)((languageId >> 16) & 0xff);
                    language += (char)((languageId >> 8) & 0xff);
                    language += (char)((languageId) & 0xff);
                    //allows czech epg
                    if (language.ToUpperInvariant() == "CZE" || language.ToUpperInvariant() == "CES")
                    {
                      title = Iso6937ToUnicode.Convert(ptrTitle);
                      description = Iso6937ToUnicode.Convert(ptrDesc);
                    }
                    else
                    {
                      title = DvbTextConverter.Convert(ptrTitle, "");
                      description = DvbTextConverter.Convert(ptrDesc, "");
                    }
                    if (title == null)
                      title = "";
                    if (description == null)
                      description = "";
                    if (string.IsNullOrEmpty(language))
                      language = "";
                    if (genre == null)
                      genre = "";
                    if (classification == null)
                      classification = "";
                    title = title.Trim();
                    description = description.Trim();
                    language = language.Trim();
                    genre = genre.Trim();
                    EpgLanguageText epgLangague = new EpgLanguageText(language, title, description, genre, starRating,
                                                                      classification, parentalRating);
                    epgProgram.Text.Add(epgLangague);
                  }
                  epgChannel.Programs.Add(epgProgram);
                }
                catch (Exception ex)
                {
                  Log.Log.Write(ex);
                }
              } //for (uint i = 0; i < eventCount; ++i)
              if (epgChannel.Programs.Count > 0)
              {
                epgChannel.Sort();
                //Log.Log.Epg("dvb: start filtering channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
                if (this.FilterOutEPGChannel(epgChannel) == false)
                {
                  //Log.Log.Epg("dvb: Not Filtered channel NID {0} TID {1} SID{2}", chan.NetworkId, chan.TransportId, chan.ServiceId);
                  epgChannels.Add(epgChannel);
                }
              }
            } //for (uint x = 0; x < channelCount; ++x)
          }
          // free the epg infos in TsWriter so that the mem used gets released 
          _interfaceEpgGrabber.Reset();
          return epgChannels;
        }
        catch (Exception ex)
        {
          Log.Log.Write(ex);
          return new List<EpgChannel>();
        }
      }
    }

    #endregion

    #region quality control

    /// <summary>
    /// Get/Set the quality
    /// </summary>
    /// <value></value>
    public IQuality Quality
    {
      get { return null; }
      set { if (value == null) Log.Log.WriteFile("Setting null quality control"); }
    }

    /// <summary>
    /// Property which returns true if card supports quality control
    /// </summary>
    /// <value></value>
    public bool SupportsQualityControl
    {
      get { return false; }
    }

    /// <summary>
    /// Reloads the card configuration
    /// </summary>
    public void ReloadCardConfiguration() {}

    #endregion
    protected abstract DVBBaseChannel CreateChannel(int networkid, int transportid, int serviceid, string name);
  }
}