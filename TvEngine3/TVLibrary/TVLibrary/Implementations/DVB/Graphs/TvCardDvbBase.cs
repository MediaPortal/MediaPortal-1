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
using System.Text.RegularExpressions;
using System.Threading;
using DirectShowLib;
using DirectShowLib.BDA;
using Mediaportal.TV.Server.Common.Types.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum;
using Mediaportal.TV.Server.TVLibrary.Implementations.Dvb;
using Mediaportal.TV.Server.TVLibrary.Implementations.Scte;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using TvDatabase;
using TvLibrary.ChannelLinkage;
using TvLibrary.Channels;
using TvLibrary.Epg;
using TvLibrary.Hardware;
using TvLibrary.Implementations.Helper;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using DvbTextConverter = Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Dvb.DvbTextConverter;
using MediaType = DirectShowLib.MediaType;
using Polarisation = DirectShowLib.BDA.Polarisation;
using RunningStatus = Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum.RunningStatus;

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
  public abstract class TvCardDvbBase : TvCardBase, IDisposable, ITVCard, ICallBackGrabber
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

    private TimeShiftingEPGGrabber _timeshiftingEPGGrabber;
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
    public TvCardDvbBase(DsDevice device)
      : base(device)
    {
      matchDevicePath = true;
      _lastSignalUpdate = DateTime.MinValue;
      _mapSubChannels = new Dictionary<int, BaseSubChannel>();
      _parameters = new ScanParameters();
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
      if (performTune)
      {
        ITsWriter tsWriter = _filterTsWriter as ITsWriter;
        if (tsWriter != null)
        {
          tsWriter.Stop();
          lock (_pmtLock)
          {
            _pmt.Clear();
          }
          _cat = null;
        }
      }
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
                                                 _filterTsWriter, id, channel, this);
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
    protected virtual ITvSubChannel SubmitTuneRequest(int subChannelId, IChannel channel, ITuneRequest tuneRequest,
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
        /*if (_interfaceEpgGrabber != null)
        {
          _interfaceEpgGrabber.Reset();
        }*/
        StopEpgGrabbing();
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
        Log.Log.WriteFile("debug: IMediaControl stopped! hr = 0x{0:x} :)", hr);
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
        string hash = DeviceDetector.GetHash(DevicePath);
        _interfaceNetworkProvider.ConfigureLogging(DeviceDetector.GetFileName(DevicePath), hash,
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
    /// Finds the correct BDA tuner and capture filter(s) and connects them into the graph.
    /// [Network Provider]->[Tuner]->[Capture/Receiver]
    /// ...or if no capture filter is present:
    /// [Network Provider]->[Tuner]
    /// </summary>
    /// <param name="device">Tuner device</param>
    /// <param name="lastFilter">A reference to the last hardware-specific filter successfully connected into in the graph.</param>
    protected void AddAndConnectBDABoardFilters(DsDevice device, out IBaseFilter lastFilter)
    {
      lastFilter = null;
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
      lastFilter = _filterTuner;

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
    }

    /// <summary>
    /// Complete the BDA filter graph.
    /// ...[last filter]->{ [WinTV-CI] / [Infinite Tee]->[MD plugin 1]->..[MD plugin n] / [Digital Devices CI 1]->..[Digital Devices CI n] }->[Infinite Tee]
    /// There are two branches from the final infinite tee.
    ///       -->[MPEG Demultiplexor]->[BDA TIF]
    ///       -->[TsWriter]
    /// The demux branch is only created when a Microsoft network provider is in use.
    /// </summary>
    /// <param name="lastFilter">A reference to the last hardware-specific filter successfully connected into in the graph.</param>
    protected void CompleteGraph(ref IBaseFilter lastFilter)
    {
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
    protected virtual bool UseCaptureFilter()
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
      if (_tunerDevice.Name.Contains("HDHomeRun"))
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
        ITsWriter tsWriter = (ITsWriter)_filterTsWriter;
        if (tsWriter != null)
        {
          tsWriter.CheckSectionCrcs(true);
          tsWriter.SetObserver(this);
        }
        _grabberAtsc = _filterTsWriter as IGrabberEpgAtsc;
        _grabberDvb = _filterTsWriter as IGrabberEpgDvb;
        _grabberMhw = _filterTsWriter as IGrabberEpgMhw;
        _grabberOpenTv = _filterTsWriter as IGrabberEpgOpenTv;
        _grabberScte = new GrabberEpgScteWrapper(_filterTsWriter as IGrabberEpgScte);
        //_interfaceChannelScan = (ITsChannelScan)_filterTsWriter;
        //_interfaceEpgGrabber = (ITsEpgScanner)_filterTsWriter;
        _interfaceChannelLinkageScanner = null;//(ITsChannelLinkageScanner)_filterTsWriter;
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
      _grabberAtsc = null;
      _grabberDvb = null;
      _grabberMhw = null;
      _grabberOpenTv = null;
      _grabberScte = null;
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
    public /*ITsChannelScan*/IBaseFilter StreamAnalyzer
    {
      //get { return _interfaceChannelScan; }
      get { return _filterTsWriter; }
    }

    /// <summary>
    /// Activates / deactivates the epg grabber
    /// </summary>
    /// <param name="value">Mode</param>
    protected override void UpdateEpgGrabber(bool value)
    {
      if (_epgGrabbing && value == false)
      {
        //_interfaceEpgGrabber.Reset();
        StopEpgGrabbing();
      }
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
      Log.Log.Info("DVB: linkage scanner not supported");
      return;
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
      //_interfaceChannelLinkageScanner.Reset();
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
          uint channelCount = 0;
          List<PortalChannel> portalChannels = new List<PortalChannel>();
          //_interfaceChannelLinkageScanner.GetChannelCount(out channelCount);
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
    /// Register to receive EPG related events.
    /// </summary>
    /// <param name="eventListener">The event listener.</param>
    public override void RegisterEpgEventListener(IEpgEvents eventListener)
    {
      _timeshiftingEPGGrabber = new TimeShiftingEPGGrabber(eventListener, this);
    }

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
      /*if (_interfaceEpgGrabber == null)
        return;
      _interfaceEpgGrabber.SetCallBack(callback);
      _interfaceEpgGrabber.GrabEPG();
      _interfaceEpgGrabber.GrabMHW();*/
      if (_filterTsWriter == null)
      {
        return;
      }
      if (this is TvCardATSC)
      {
        if (_grabberAtsc != null)
        {
          _grabberAtsc.SetCallBack(this);
          _grabberAtsc.Start();
        }
        if (_grabberScte != null)
        {
          _grabberScte.SetCallBack(this);
          _grabberScte.Start();
        }
      }
      else
      {
        if (_grabberDvb != null)
        {
          _grabberDvb.SetCallBack(this);
          _grabberDvb.SetProtocols(true, true, true, true, true, true, true, true);
        }
        if (_grabberMhw != null)
        {
          _grabberMhw.SetCallBack(this);
          _grabberMhw.SetProtocols(true, true);
        }
        if (_grabberOpenTv != null)
        {
          _grabberOpenTv.SetCallBack(this);
          _grabberOpenTv.Start();
        }
      }
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
      //if (_interfaceEpgGrabber != null)
      //  _interfaceEpgGrabber.AbortGrabbing();
      if (_filterTsWriter != null)
      {
        //StopEpgGrabbing();
        TriggerEpgReceiveCallBack();
      }
      //if (_timeshiftingEPGGrabber != null)
      //  _timeshiftingEPGGrabber.OnEpgCancelled();
    }

    /// <summary>
    /// Returns the EPG grabbed or null if epg grabbing is still busy
    /// </summary>
    public List<EpgChannel> Epg
    {
      get
      {
        //if (!CheckThreadId()) return null;
        /*try
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
        }*/

        List<Tuple<IChannel, IList<EpgProgram>>> data = new List<Tuple<IChannel, IList<EpgProgram>>>();
        try
        {
          Log.Log.Info("EPG: collect data, DVB = {0} / {1}, MediaHighway = {2} / {3}, OpenTV = {4} / {5}, ATSC = {6} / {7}, SCTE = {8} / {9}",
                        _isSeenDvb, _isCompleteDvb, _isSeenMhw, _isCompleteMhw, _isSeenOpenTv, _isCompleteOpenTv, _isSeenAtsc, _isCompleteAtsc, _isSeenScte, _isCompleteScte);

          if (_isSeenMhw && _grabberMhw != null)
          {
            data.AddRange(CollectMediaHighwayData());
          }
          if (_isSeenOpenTv && _grabberOpenTv != null)
          {
            data.AddRange(CollectOpenTvData());
          }
          if (_isSeenDvb && _grabberDvb != null)
          {
            data.AddRange(CollectEitData());
          }
          if (_isSeenAtsc && _grabberAtsc != null)
          {
            data.AddRange(CollectData(_grabberAtsc, _filterTsWriter as IGrabberSiAtsc));
          }
          if (_isSeenScte && _grabberScte != null)
          {
            data.AddRange(CollectData(_grabberScte, new GrabberSiScteWrapper(_filterTsWriter as IGrabberSiScte)));
          }

          List<EpgChannel> channels = new List<EpgChannel>(data.Count);
          foreach (var channel in data)
          {
            DVBBaseChannel dvbChannel = channel.Item1 as DVBBaseChannel;
            if (dvbChannel == null)
            {
              continue;
            }
            EpgChannel epgChannel = new EpgChannel();
            epgChannel.Channel = CreateChannel(dvbChannel.NetworkId, dvbChannel.TransportId, dvbChannel.ServiceId, "");
            epgChannel.Programs = (List<EpgProgram>)channel.Item2;
            channels.Add(epgChannel);
          }
          return channels;
        }
        catch (Exception ex)
        {
          Log.Log.Error("EPG: failed to collect data\r\n{0}", ex.ToString());
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

    #region mm1352000 EPG

    protected void StopEpgGrabbing()
    {
      if (_filterTsWriter == null)
      {
        return;
      }

      Log.Log.Debug("EPG: stop EPG grabbing");
      if (this is TvCardATSC)
      {
        if (_grabberAtsc != null)
        {
          _grabberAtsc.Stop();
          _grabberAtsc.SetCallBack(null);
        }
        if (_grabberScte != null)
        {
          _grabberScte.Stop();
          _grabberScte.SetCallBack(null);
        }
      }
      else
      {
        if (_grabberDvb != null)
        {
          _grabberDvb.SetProtocols(false, false, false, false, false, false, false, false);
          _grabberDvb.SetCallBack(this);
        }
        if (_grabberMhw != null)
        {
          _grabberMhw.SetProtocols(false, false);
          _grabberMhw.SetCallBack(null);
        }
        if (_grabberOpenTv != null)
        {
          _grabberOpenTv.Stop();
          _grabberOpenTv.SetCallBack(null);
        }
      }
    }

    private void TriggerEpgReceiveCallBack()
    {
      if (_epgGrabberCallback == null)
      {
        return;
      }

      // Use a thread to collect the data. If we collect from this thread it
      // can cause stuttering and deadlocks.
      Thread collector = new Thread(delegate()
      {
        _epgGrabberCallback.OnEpgReceived();
      });
      collector.IsBackground = true;
      collector.Priority = ThreadPriority.Lowest;
      collector.Name = "EPG collector";
      collector.Start();
    }

    #region ICallBackGrabber members

    /// <summary>
    /// This function is invoked when the first section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was received.</param>
    public void OnTableSeen(ushort pid, byte tableId)
    {
      if (pid == 0x12)
      {
        _isSeenDvb = true;
        _isCompleteDvb = false;
      }
      else if (pid == 0xd2)
      {
        _isSeenMhw = true;
        _isCompleteMhw = false;
      }
      else if (pid == 0x30)
      {
        _isSeenOpenTv = true;
        _isCompleteOpenTv = false;
      }
      else if (pid == 0xcb)
      {
        _isSeenAtsc = true;
        _isCompleteAtsc = false;
      }
      else if (pid == 0xd6)
      {
        _isSeenScte = true;
        _isCompleteScte = false;
      }
    }

    /// <summary>
    /// This function is invoked after the last section from a table is received.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that was completed.</param>
    public void OnTableComplete(ushort pid, byte tableId)
    {
      if (pid == 0x12)
      {
        _isCompleteDvb = true;
      }
      else if (pid == 0xd2)
      {
        _isCompleteMhw = true;
      }
      else if (pid == 0x30)
      {
        _isCompleteOpenTv = true;
      }
      else if (pid == 0xcb)
      {
        _isCompleteAtsc = true;
      }
      else if (pid == 0xd6)
      {
        _isCompleteScte = true;
      }

      if (
        (!_isSeenDvb || _isCompleteDvb) &&
        (!_isSeenMhw || _isCompleteMhw) &&
        (!_isSeenOpenTv || _isCompleteOpenTv) &&
        (!_isSeenAtsc || _isCompleteAtsc) &&
        (!_isSeenScte || _isCompleteScte)
      )
      {
        Log.Log.Debug("EPG: EPG complete");
        TriggerEpgReceiveCallBack();
      }
    }

    /// <summary>
    /// This function is invoked after any section from a table changes.
    /// </summary>
    /// <param name="pid">The PID that the table section was recevied from.</param>
    /// <param name="tableId">The identifier of the table that changed.</param>
    public void OnTableChange(ushort pid, byte tableId)
    {
      OnTableSeen(pid, tableId);
    }

    /// <summary>
    /// This function is invoked after the grabber is reset.
    /// </summary>
    /// <param name="pid">The PID that is associated with the grabber.</param>
    public void OnReset(ushort pid)
    {
      Log.Log.Debug("EPG DVB: on reset, PID = {0}", pid);
      if (pid == 0x12)
      {
        _isSeenDvb = false;
        _isCompleteDvb = false;
      }
      else if (pid == 0xd2)
      {
        _isSeenMhw = false;
        _isCompleteMhw = false;
      }
      else if (pid == 0x30)
      {
        _isSeenOpenTv = false;
        _isCompleteOpenTv = false;
      }
      else if (pid == 0xcb)
      {
        _isSeenAtsc = false;
        _isCompleteAtsc = false;
      }
      else if (pid == 0xd6)
      {
        _isSeenScte = false;
        _isCompleteScte = false;
      }
    }

    #endregion

    #region mm1352000 DVB/OpenTV/MHW EPG

    #region constants

    private const TunerEpgGrabberProtocol PROTOCOLS_SUPPORTED = TunerEpgGrabberProtocol.BellTv | TunerEpgGrabberProtocol.DishNetwork | TunerEpgGrabberProtocol.DvbEit | TunerEpgGrabberProtocol.Freesat | TunerEpgGrabberProtocol.MediaHighway1 | TunerEpgGrabberProtocol.MediaHighway2 | TunerEpgGrabberProtocol.MultiChoice | TunerEpgGrabberProtocol.OpenTv | TunerEpgGrabberProtocol.OrbitShowtimeNetwork | TunerEpgGrabberProtocol.Premiere | TunerEpgGrabberProtocol.ViasatSweden;
    private const TunerEpgGrabberProtocol PROTOCOLS_DVB = PROTOCOLS_SUPPORTED & ~PROTOCOLS_MEDIA_HIGHWAY & ~TunerEpgGrabberProtocol.OpenTv;
    private const TunerEpgGrabberProtocol PROTOCOLS_MEDIA_HIGHWAY = TunerEpgGrabberProtocol.MediaHighway1 | TunerEpgGrabberProtocol.MediaHighway2;

    private const string LANG_CODE_ENG = "eng";
    private static readonly Regex MEDIA_HIGHWAY_SEASON_NAME = new Regex(@"^\s*(\d+)(\s*\/\s*(\d+))?\s*$");

    private static readonly IDictionary<byte, string> MAPPING_PROGRAM_CLASSIFICATIONS_MHW2_ESP = new Dictionary<byte, string>(8)
    {
      { 0, "SC" },      // "Sin calificación"
      { 1, "TP" },      // "Todos los públicos"
      { 3, "+18" },
      { 4, "X" },
      { 6, "+7" },
      { 7, "INF" },     // "Especialmente recomendada para la infancia"
      { 8, "+12" },
      { 9, "+13/16" }
    };

    // Note: there is some uncertainty about values 5, 6 and 7. Our old code
    // differs with MythTV, and this code differs from the ATSC RRT.
    private static readonly IDictionary<byte, string> MAPPING_PROGRAM_CLASSIFICATIONS_MPAA = new Dictionary<byte, string>(6)
    {
      { 1, "G" },       // general
      { 2, "PG" },      // parental guidance
      { 3, "PG-13" },   // parental guidance under 13
      { 4, "R" },       // restricted
      { 5, "NC-17" },   // nobody 17 and under
      { 6, "NR" }       // not rated
    };

    private static readonly IDictionary<byte, string> MAPPING_PROGRAM_RATINGS_VCHIP = new Dictionary<byte, string>(6)
    {
      { 1, "TV-Y" },    // all children
      { 2, "TV-Y7" },   // children 7 and older
      { 3, "TV-G" },    // general audience
      { 4, "TV-PG" },   // parental guidance
      { 5, "TV-14" },   // adults 14 and older
      { 6, "TV-MA" }    // mature audience
    };

    #region content type

    private static readonly IDictionary<byte, string> MAPPING_CONTENT_TYPES_DVB = new Dictionary<byte, string>(16)
    {
      { 0x01, "Movie/Drama" },
      { 0x02, "News/Current Affairs" },
      { 0x03, "Show/Game Show" },
      { 0x04, "Sports" },
      { 0x05, "Children/Youth" },
      { 0x06, "Music/Ballet/Dance" },
      { 0x07, "Arts/Culture" },
      { 0x08, "Social/Political/Economics" },
      { 0x09, "Education/Science/Factual" },
      { 0x0a, "Leisure/Hobbies" },
      //{ 0x0b, "Special Characteristic" },
      { 0x0f, "User Defined" }
    };

    private static readonly IDictionary<byte, string> MAPPING_CONTENT_SUB_TYPES_DVB = new Dictionary<byte, string>(256)
    {
      { 0x11, "Detective/Thriller" },
      { 0x12, "Adventure/Western/War" },
      { 0x13, "Science Fiction/Fantasy/Horror" },
      { 0x14, "Comedy" },
      { 0x15, "Soap/Melodrama/Folkloric" },
      { 0x16, "Romance" },
      { 0x17, "Serious/Classical/Religious/Historical Movie/Drama" },
      { 0x18, "Adult Movie/Drama" },

      { 0x21, "News/Weather Report" },
      { 0x22, "News Magazine" },
      { 0x23, "Documentary" },
      { 0x24, "Discussion/Interview/Debate" },

      { 0x31, "Game Show/Quiz/Contest" },
      { 0x32, "Variety Show" },
      { 0x33, "Talk Show" },

      { 0x41, "Special Event" },
      { 0x42, "Sports Magazine" },
      { 0x43, "Football/Soccer" },
      { 0x44, "Tennis/Squash" },
      { 0x45, "Team Sport" },
      { 0x46, "Athletics" },
      { 0x47, "Motor Sport" },
      { 0x48, "Water Sport" },
      { 0x49, "Winter Sport" },
      { 0x4a, "Equestrian" },
      { 0x4b, "Martial Sport" },

      { 0x51, "Pre-School" },
      { 0x52, "Entertainment (6 To 14 Year Old)" },
      { 0x53, "Entertainment (10 To 16 Year Old)" },
      { 0x54, "Information/Education/School" },
      { 0x55, "Cartoon/Puppets" },

      { 0x61, "Rock/Pop" },
      { 0x62, "Serious/Classical Music" },
      { 0x63, "Folk/Traditional Music" },
      { 0x64, "Jazz" },
      { 0x65, "Musical/Opera" },
      { 0x66, "Ballet" },

      { 0x71, "Performing Arts" },
      { 0x72, "Fine Arts" },
      { 0x73, "Religion" },
      { 0x74, "Popular Culture/Traditional Arts" },
      { 0x75, "Literature" },
      { 0x76, "Film/Cinema" },
      { 0x77, "Experimental Film/Video" },
      { 0x78, "Broadcasting/Press" },
      { 0x79, "New Media" },
      { 0x7a, "Arts/Culture Magazine" },
      { 0x7b, "Fashion" },

      { 0x81, "Magazine/Report/Documentary" },
      { 0x82, "Economics/Social Advisory" },
      { 0x83, "Remarkable People" },

      { 0x91, "Nature/Animals/Environment" },
      { 0x92, "Technology/Natural Science" },
      { 0x93, "Medicine/Physiology/Psychology" },
      { 0x94, "Foreign Countries/Expeditions" },
      { 0x95, "Social/Spiritual Science" },
      { 0x96, "Further Education" },
      { 0x97, "Languages" },

      { 0xa1, "Tourism/Travel" },
      { 0xa2, "Handicraft" },
      { 0xa3, "Motoring" },
      { 0xa4, "Fitness & Health" },
      { 0xa5, "Cooking" },
      { 0xa6, "Advertisement/Shopping" },
      { 0xa7, "Gardening" }

      /*{ 0xb0, "Original Language" },
      { 0xb1, "Black & White" },
      { 0xb2, "Unpublished" },
      { 0xb3, "Live Broadcast" },
      { 0xb4, "Plano-Stereoscopic" },
      { 0xb5, "Local Or Regional" }*/
    };

    private static readonly IDictionary<byte, string> MAPPING_CONTENT_TYPES_DISH = new Dictionary<byte, string>(16)
    {
      { 1, "Movie" },
      { 2, "Sports" },
      { 3, "News/Business" },
      { 4, "Family/Children" },
      { 5, "Education" },
      { 6, "Series/Special" },
      { 7, "Music/Art" },
      { 8, "Religious" }
    };

    private static readonly IDictionary<byte, string> MAPPING_CONTENT_SUB_TYPES_DISH = new Dictionary<byte, string>(256)
    {
      { 1, "Action" },
      { 2, "Adult" },
      { 3, "Adventure" },
      { 4, "Animals" },
      { 5, "Animated" },
      { 6, "Anime" },
      { 7, "Anthology" },
      { 8, "Art" },
      { 9, "Auto" },
      { 10, "Anthology" },
      { 11, "Ballet" },
      { 12, "Baseball" },
      { 13, "Basketball" },
      { 14, "Beach Soccer" },
      { 15, "Beach Volleyball" },
      { 16, "Biathlon" },
      { 17, "Biography" },
      { 18, "Boats" },
      { 19, "Boat Racing" },
      { 20, "Bowling" },
      { 21, "Boxing" },
      { 22, "Business - Financial" },

      { 26, "Children" },
      { 27, "Children - Special" },
      { 28, "Children - News" },
      { 29, "Children - Music" },

      { 31, "Collectibles" },
      { 32, "Comedy" },
      { 33, "Comedy Drama" },
      { 34, "Computers" },
      { 35, "Cooking" },
      { 36, "Crime" },
      { 37, "Crime Drama" },
      { 38, "Curling" },
      { 39, "Dance" },
      { 40, "Dark Comedy" },
      { 41, "Docudrama" },
      { 42, "Documentary" },
      { 43, "Drama" },
      { 44, "Educational" },
      { 45, "Erotic" },

      { 47, "Exercise" },

      { 49, "Fantasy" },
      { 50, "Fashion" },
      { 51, "Fencing" },
      { 52, "Fishing" },
      { 53, "Football" },
      { 54, "French" },
      { 55, "Fundraiser" },
      { 56, "Game Show" },
      { 57, "Golf" },
      { 58, "Gymnastics" },
      { 59, "Health" },
      { 60, "History" },
      { 61, "Historical Drama" },
      { 62, "Hockey" },
      { 63, "Holiday" },
      { 64, "Holiday - Children" },
      { 65, "Holiday - Children Special" },
      { 66, "Holiday - Music" },
      { 67, "Holiday - Music Special" },
      { 68, "Holiday - Special" },
      { 69, "Horror" },
      { 70, "Horse Racing" },
      { 71, "House & Garden" },

      { 73, "How To" },

      { 75, "Interview" },

      { 77, "Lacrossse" },

      { 79, "Martial Arts" },
      { 80, "Medical" },
      { 81, "Mini-Series" },
      { 82, "Motor Sport" },
      { 83, "Motorcycle" },
      { 84, "Music" },
      { 85, "Music Special" },
      { 86, "Music Talk" },
      { 87, "Musical" },
      { 88, "Musical Comedy" },

      { 90, "Mystery" },
      { 91, "Nature" },
      { 92, "News" },

      { 95, "Opera" },
      { 96, "Outdoors" },
      { 97, "Parade" },
      { 98, "Politics" },
      { 99, "Public Affairs" },

      { 102, "Reality" },
      { 103, "Religious" },
      { 104, "Rodeo" },
      { 105, "Romance" },
      { 106, "Romantic Comedy" },
      { 107, "Rugby" },
      { 108, "Running" },

      { 110, "Science" },
      { 111, "Science Fiction" },
      { 112, "Self Improvement" },
      { 113, "Shopping" },

      { 116, "Skiing" },

      { 119, "Soap" },

      { 123, "Soccer" },
      { 124, "Softball" },
      { 125, "Spanish" },
      { 126, "Special" },
      { 127, "Speedskating" },
      { 128, "Sports Event" },
      { 129, "Sports Non-event" },
      { 130, "Sports Discussion" },
      { 131, "Suspense" },

      { 133, "Swimming" },
      { 134, "Discussion" },
      { 135, "Tennis" },
      { 136, "Thriller" },
      { 137, "Track & Field" },
      { 138, "Travel" },
      { 139, "Variety" },
      { 140, "Volleyball" },
      { 141, "War" },
      { 142, "Watersports" },
      { 143, "Weather" },
      { 144, "Western" },

      { 146, "Wrestling" },
      { 147, "Yoga" },
      { 148, "Agriculture" },
      { 149, "Anime" },

      { 151, "Arm Wrestling" },
      { 152, "Arts & Crafts" },
      { 153, "Auction" },
      { 154, "Motor Racing" },
      { 155, "Air Racing" },
      { 156, "Badminton" },

      { 160, "Cycle Racing" },
      { 161, "Sailing" },
      { 162, "Bobsled" },
      { 163, "Body Building" },
      { 164, "Canoeing" },
      { 165, "Cheerleading" },
      { 166, "Community" },
      { 167, "Consumer" },

      { 170, "Debate" },
      { 171, "Diving" },
      { 172, "Dog Show" },
      { 173, "Drag Racing" },
      { 174, "Entertainment" },
      { 175, "Environment" },
      { 176, "Equestrian" },

      { 179, "Field Hockey" },
      { 180, "Figure Skating" },
      { 181, "Football" },
      { 182, "Gay/Lesbian" },
      { 183, "Handball" },
      { 184, "Home Improvement" },
      { 185, "Hunting" },
      { 186, "Hurling" },
      { 187, "Hydroplane Racing" },

      { 193, "Law" },

      { 195, "Motorcycle Racing" },

      { 197, "News Magazine" },

      { 199, "Paranormal" },
      { 200, "Parenting" },

      { 202, "Performing Arts" },
      { 203, "Play-Off" },
      { 204, "Politics" },
      { 205, "Polo" },
      { 206, "Pool" },
      { 207, "Pro Wrestling" },
      { 208, "Ringuette" },
      { 209, "Roller Derby" },
      { 210, "Rowing" },
      { 211, "Sailing" },
      { 212, "Shooting" },
      { 213, "Sitcom" },
      { 214, "Skateboarding" },
      { 215, "Skating" },
      { 216, "Skeleton" },
      { 217, "Snowboarding" },
      { 218, "Snowmobile" },

      { 221, "Standup" },
      { 222, "Sumo Wrestling" },
      { 223, "Surfing" },
      { 224, "Tennis" },
      { 225, "Triathlon" },
      { 226, "Water Polo" },
      { 227, "Water Skiing" },
      { 228, "Weightlifting" },
      { 229, "Yacht Racing" },
      { 230, "Card Games" },
      { 231, "Poker" },

      { 233, "Musical" },
      { 234, "Military" },
      { 235, "Technology" },
      { 236, "Mixed Martial Arts" },
      { 237, "Action Sports" }
    };

    // Incomplete due to limited sample content.
    private static readonly IDictionary<byte, string> MAPPING_CONTENT_TYPES_VIRGIN_MEDIA = new Dictionary<byte, string>(16)
    {
      { 0x00, "Children" },
      { 0x02, "Comedy/Drama" },
      { 0x03, "Entertainment/Reality" },
      { 0x04, "Movie" },
      { 0x05, "Lifestyle" },
      { 0x08, "Factual" },
      { 0x0c, "Sport" },
      { 0x0f, "Special Interest" }
    };

    // Incomplete due to limited sample content.
    private static readonly IDictionary<byte, string> MAPPING_CONTENT_SUB_TYPES_VIRGIN_MEDIA = new Dictionary<byte, string>(256)
    {
      { 0x01, "Sitcom" },
      { 0x02, "Adventure" },
      { 0x03, "Arts/Crafts/Educational" },

      { 0x24, "Science Fiction/Fantasy" },

      { 0x31, "Game Show/Quiz/Contest" },

      { 0x41, "Comedy" },
      { 0x42, "Drama/Thriller" },
      { 0x48, "Romance" },
      { 0x4a, "Horror" },

      { 0x52, "Cooking" },
      { 0x54, "Fashion" },
      { 0x55, "Fitness & Health" },

      { 0x83, "News/Weather" },
      { 0x85, "Business" },
      { 0x86, "Documentary/Discussion/Interview/Debate" },

      { 0xc1, "Team Sports" },
      { 0xc6, "Individual Sports" },
      { 0xc9, "Motor Sport" },

      { 0xf3, "Adult" }
    };

    #endregion

    #region OpenTV

    #region Australia

    private static readonly HashSet<ushort> OPENTV_ORIGINAL_NETWORK_IDS_AU = new HashSet<ushort>
    {
      105,    // 0x0069 Foxtel AU (Optus B3)
      168,    // 0x00a8 Foxtel AU
      4095,   // 0x0fff VAST AU (Optus Networks)
      4096,   // 0x1000 Foxtel AU (Optus B3)

      // 0x1010 - 101f AU broadcasters (ABC, SBS etc.)
      4112,
      4113,
      4114,
      4115,
      4116,
      4117,
      4118,
      4119,
      4120,
      4121,
      4122,
      4123,
      4124,
      4125,
      4126,
      4127
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_RATINGS_AU = new Dictionary<byte, string>(9)
    {
      { 0, "NC" },      // not classified (exempt)
      { 2, "P" },       // suitable for pre-school children
      { 4, "C" },       // suitable for children
      { 6, "G" },
      { 8, "PG" },
      { 10, "M" },
      { 12, "MA15+" },  // formerly MA
      { 14, "AV15+" },  // formerly AV (suitable for adult viewers only - violent content); may no longer be used, or merged with MA15+... or may be R18+ if 15 is X18+
      { 15, "R18+" }    // formerly R; may be X18+
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_AU = new Dictionary<byte, string>(8)
    {
      { 0x02, "Entertainment" },
      { 0x04, "Movie" },
      { 0x06, "Sport" },
      { 0x09, "News & Documentary" },
      { 0x0a, "Kids & Family" },
      { 0x0c, "Music & Radio" },
      { 0x0e, "Special Interest" },
      { 0x0f, "Adult" }
    };

    // Entries marked with a question mark are uncertain.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_AU = new Dictionary<ushort, string>(256)
    {
      { 0x0201, "War & Western" },
      { 0x0202, "Drama" },
      { 0x0203, "Comedy" },
      { 0x0204, "Reality" },
      { 0x0205, "Talk Show" },
      { 0x0206, "Sci-Fi & Fantasy" },
      { 0x0207, "Lifestyle & Documentary" },
      { 0x0208, "Light Entertainment" },
      // unknown
      { 0x020a, "Other" },

      { 0x0401, "Action & Adventure" },
      { 0x0402, "War, Western & History" },
      { 0x0403, "Drama/Romance" },                          // ?
      { 0x0404, "Horror / Sci-Fi & Fantasy" },              // ?
      { 0x0405, "Comedy / Musical & Dance" },               // ?
      { 0x0406, "Mystery & Crime / Thriller & Suspense" },  // ?
      { 0x0407, "Animation / Kids & Family" },              // ?
      { 0x0408, "Documentary/Other" },                      // ?

      { 0x0601, "Rugby League" },
      { 0x0602, "AFL" },
      { 0x0603, "Rugby Union" },
      { 0x0604, "Football & Soccer" },
      { 0x0605, "Cricket" },            // ?
      { 0x0606, "Baseball/Golf" },
      { 0x0607, "Court Sports" },
      { 0x0608, "Boxing & Wrestling" },
      { 0x0609, "Track & Pool" },
      { 0x060a, "Extreme Sports" },
      { 0x060b, "Racing" },
      { 0x060c, "Other" },

      { 0x0901, "Business & Finance" },
      { 0x0902, "Local" },
      { 0x0903, "International" },
      { 0x0904, "People & Culture" },
      { 0x0905, "History" },
      { 0x0906, "Natural World" },
      { 0x0907, "Travel & Adventure" },
      { 0x0908, "Other" },

      { 0x0a01, "Pre-school" },
      { 0x0a02, "Adventure & Action" },
      { 0x0a03, "Comedy" },
      { 0x0a04, "Animation & Cartoon" },
      { 0x0a05, "Educational" },
      // unknown
      { 0x0a07, "Game Show" },
      { 0x0a08, "Other" },

      { 0x0c01, "Pop" },                // ?
      // unknown
      { 0x0c03, "Blues & Jazz" },       // ?
      // unknown
      { 0x0c05, "Dance & Techno" },     // ?
      // unknown
      { 0x0c09, "Classical & Opera" },
      // unknown
      { 0x0c0b, "Country" },
      { 0x0c0c, "Live & Request" },     // ?
      { 0x0c0d, "Other" },              // ?

      { 0x0e01, "Religion" },
      { 0x0e02, "Foreign Language" },   // ?
      // unknown
      { 0x0e04, "Shopping" },
      { 0x0e05, "Help/Information" },   // ?

      { 0x0f02, "Adult" }               // ?
    };

    #endregion

    #region Italy

    // All original network IDs used on Hotbird 13E (which carries Sky Italia).
    private static readonly HashSet<ushort> OPENTV_ORIGINAL_NETWORK_IDS_IT = new HashSet<ushort>
    {
      113,    // 0x0071 (Polsat/Cyfra+)
      176,    // 0x00b0 Groupe CANAL+

      // 0x00c0 - 0x00cd Canal+
      192,
      193,
      194,
      195,
      196,
      197,
      198,
      199,
      200,
      201,
      202,
      203,
      204,
      205,

      272,    // 0x0110 Mediaset
      318,    // 0x013e Eutelsat Satellite System 13°E (European Telecommunications Satellite Organization)
      319,    // 0x013f Eutelsat Satellite System 13°E (European Telecommunications Satellite Organization)
      702,    // 0x02be ARABSAT - Arab Satellite Communications Organization
      64511   // 0xfbff Sky Italia
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_RATINGS_IT = new Dictionary<byte, string>(6)
    {
      { 1, "PT" },      // per tutti ("for all", green icon)
      { 2, "BA" },      // bambini accompagnati ("accompanied children", yellow icon)
      { 3, "VM 12" },   // vietato ai minori di 12 anni ("no one under 12 years", orange icon)
      { 4, "VM 14" },   // vietato ai minori di 14 anni ("no one under 14 years", pink icon)
      { 5, "VM 16" },   // guess; not actually seen
      { 6, "VM 18" }    // vietato ai minori di 18 anni ("no one under 18 years", red icon)
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_IT = new Dictionary<byte, string>(8)
    {
      { 0x01, "Intrattenimento" },
      { 0x02, "Sport" },
      { 0x03, "Film" },
      { 0x04, "Mondo e Tendenze" },
      { 0x05, "Informazione" },
      { 0x06, "Ragazzi e Musica" },
      { 0x07, "Altri Programmi" }
    };

    // Incomplete due to limited sample content.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_IT = new Dictionary<ushort, string>(256)
    {
      { 0x0100, "Intrattenimento" },
      { 0x0101, "Fiction" },
      { 0x0102, "Sit Com" },
      { 0x0103, "Show" },
      { 0x0104, "Telefilm" },
      { 0x0105, "Soap Opera" },
      { 0x0106, "Telenovela" },
      { 0x0107, "Fantascienza" },
      { 0x0108, "Animazione" },
      { 0x0109, "Giallo" },
      { 0x010a, "Drammatico" },
      { 0x010b, "Reality Show" },
      { 0x010c, "Miniserie" },
      { 0x010d, "Spettacolo" },
      { 0x010e, "Quiz" },
      { 0x010f, "Talk Show" },
      { 0x0110, "Varietà" },
      { 0x0111, "Festival" },
      { 0x0112, "Teatro" },
      { 0x0113, "Gioco" },

      { 0x0200, "Sport" },
      { 0x0201, "Calcio" },
      { 0x0202, "Tennis" },
      { 0x0203, "Motori" },
      { 0x0204, "Altri" },
      { 0x0205, "Baseball" },
      { 0x0206, "Ciclismo" },
      { 0x0207, "Rugby" },
      { 0x0208, "Basket" },
      { 0x0209, "Boxe" },
      { 0x020a, "Atletica" },
      { 0x020b, "Football USA" },
      { 0x020c, "Hockey" },
      { 0x020d, "Sci" },
      { 0x020e, "Equestri" },
      { 0x020f, "Golf" },
      { 0x0210, "Nuoto" },
      { 0x0211, "Wrestling" },
      // unknown
      { 0x0213, "Volley" },
      { 0x0214, "Poker" },
      { 0x0215, "Vela" },
      { 0x0216, "Sport Invernali" },

      { 0x0300, "Cinema" },
      { 0x0301, "Drammatico" },
      { 0x0302, "Commedia" },
      { 0x0303, "Romantico" },
      { 0x0304, "Azione" },
      { 0x0305, "Fantascienza" },
      { 0x0306, "Western" },
      { 0x0307, "Comico" },
      { 0x0308, "Fantastico" },
      { 0x0309, "Avventura" },
      { 0x030a, "Poliziesco" },
      { 0x030b, "Guerra" },
      { 0x030c, "Horror" },
      { 0x030d, "Animazione" },
      { 0x030e, "Thriller" },
      { 0x030f, "Musicale" },
      { 0x0310, "Corto" },
      { 0x0311, "Cortometraggio" },

      { 0x0400, "Mondi e Culture" },
      { 0x0401, "Natura" },
      { 0x0402, "Arte e Cultura" },
      { 0x0403, "Lifestyle" },
      { 0x0404, "Viaggi" },
      { 0x0405, "Documentario" },
      { 0x0406, "Società" },
      { 0x0407, "Scienza" },
      { 0x0408, "Storia" },
      { 0x0409, "Sport" },
      { 0x040a, "Pesca" },
      { 0x040b, "Popoli" },
      { 0x040c, "Cinema" },
      { 0x040d, "Musica" },
      { 0x040e, "Hobby" },
      { 0x040f, "Caccia" },
      { 0x0410, "Reportage" },
      { 0x0411, "Magazine" },
      { 0x0412, "Magazine Cultura" },
      { 0x0413, "Magazine Scienza" },
      { 0x0414, "Politica" },
      { 0x0415, "Magazine Cinema" },
      { 0x0416, "Magazine Sport" },
      { 0x0417, "Attualità" },
      { 0x0418, "Moda" },
      { 0x0419, "Economia" },
      { 0x041a, "Tecnologia" },
      { 0x041b, "Magazine Viaggi" },
      { 0x041c, "Magazine Natura" },
      { 0x041d, "Avventura" },
      { 0x041e, "Cucina" },
      { 0x041f, "Televendita" },

      { 0x0500, "News" },
      { 0x0501, "Notiziario" },
      { 0x0502, "Sport" },
      { 0x0503, "Economia" },
      // unknown
      { 0x0505, "Meteo" },

      { 0x0601, "Bambini" },
      { 0x0602, "Educational" },
      { 0x0603, "Cartoni Animati" },
      { 0x0604, "Musica" },
      { 0x0605, "Film Animazione" },
      { 0x0606, "Film" },
      { 0x0607, "Telefilm" },
      { 0x0608, "Magazine" },
      // unknown
      { 0x060a, "Documentario" },
      // unknown
      { 0x0612, "Jazz" },
      // unknown
      { 0x0614, "Danza" },
      { 0x0615, "Videoclip" },

      { 0x0700, "Altri Canali" },
      { 0x0701, "Educational" },
      { 0x0702, "Regionale" },
      { 0x0703, "Shopping" },
      { 0x0704, "Altri" },
      { 0x0705, "Inizio e Fine Trasmissioni" },
      { 0x0706, "Eventi Speciali" },
      { 0x0707, "Film per Adulti" }
    };

    #endregion

    #region New Zealand

    private static readonly HashSet<ushort> OPENTV_ORIGINAL_NETWORK_IDS_NZ = new HashSet<ushort>
    {
      47,     // 0x002f Freeview Satellite NZ (TVNZ)
      169     // 0x00a9 Sky NZ
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_RATINGS_NZ = new Dictionary<byte, string>(10)
    {
      { 0, "NC" },      // not classified (exempt)

      { 1, "G" },
      { 2, "G" },

      { 4, "PG" },

      { 6, "M" },
      { 8, "R16" },

      { 10, "R18" },
      { 12, "R18" },
      { 13, "R18" },
      { 14, "R18" }
    };

    // These are overrides for DVB content types.
    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_NZ = new Dictionary<byte, string>(8)
    {
      { 0x01, "Movie" },
      { 0x0f, "Adult" }
    };

    // These are overrides for DVB content types.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_NZ = new Dictionary<ushort, string>(256)
    {
      { 0x010e, "Western" },

      { 0x0304, "Reality" },
      { 0x0305, "Action" },
      { 0x0306, "Drama" },
      { 0x0307, "Comedy" },
      { 0x0308, "Documentary" },
      { 0x0309, "Soap" },
      { 0x030a, "Sci-Fi" },
      { 0x030b, "Crime" },
      { 0x030c, "Sport" },

      { 0x0402, "Golf" },
      { 0x0405, "Rugby" },
      { 0x040c, "Rugby League" },
      { 0x040d, "Cricket" },
      { 0x040e, "Cycling" },

      { 0x0507, "Sport" },
      { 0x0509, "Comedy" },
      // unknown
      { 0x050b, "Movie" },
      { 0x050c, "Game Show" },

      { 0x0609, "Country" },

      { 0x0908, "History" },
      { 0x0909, "Reality/Documentary" },
      { 0x090a, "Biography/Documentary" },
      { 0x090b, "Reality/Travel" },
      // unknown
      { 0x090d, "Human Science/Culture" },
      { 0x090e, "Crime/Investigation" },

      { 0x0a07, "Property" },
      { 0x0a0b, "Home Restoration/Make-Over" }
    };

    #endregion

    #region UK

    // All original network IDs used on Astra 28.2E (which carries Sky UK).
    private static readonly HashSet<ushort> OPENTV_ORIGINAL_NETWORK_IDS_UK = new HashSet<ushort>
    {
      2,    // 0x0002 Société Européenne des Satellites
      59    // 0x003b BBC (Freesat)
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_RATINGS_UK = new Dictionary<byte, string>(5)
    {
      { 1, "U" },       // universal
      { 2, "PG" },
      { 3, "12" },
      { 4, "15" },
      { 5, "18" }
    };

    private static readonly IDictionary<byte, string> MAPPING_OPENTV_PROGRAM_CATEGORIES_UK = new Dictionary<byte, string>(8)
    {
      { 0x01, "Specialist" },
      { 0x02, "Children" },
      { 0x03, "Entertainment" },
      { 0x04, "Music & Radio" },
      { 0x05, "News & Documentaries" },
      { 0x06, "Movies" },
      { 0x07, "Sports" }
    };

    // Entries marked with a question mark are uncertain.
    private static readonly IDictionary<ushort, string> MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_UK = new Dictionary<ushort, string>(256)
    {
      { 0x0101, "Adult" },
      { 0x0102, "Information" },
      { 0x0103, "Shopping" },
      { 0x0104, "Gaming" },

      { 0x0201, "Cartoons" },
      { 0x0202, "Comedy" },
      { 0x0203, "Drama" },
      { 0x0204, "Educational" },
      { 0x0205, "Under 5" },
      { 0x0206, "Factual" },
      { 0x0207, "Magazine" },           // ?
      { 0x0208, "Games Shows" },
      { 0x0209, "Games" },              // ?

      { 0x0301, "Action" },
      { 0x0302, "Comedy" },
      { 0x0303, "Detective" },
      { 0x0304, "Drama" },
      { 0x0305, "Game Shows" },
      { 0x0306, "Sci-Fi" },
      { 0x0307, "Soaps" },
      { 0x0308, "Animation" },
      { 0x0309, "Chat Show" },
      { 0x030a, "Cooking" },
      { 0x030b, "Factual" },
      { 0x030c, "Fashion" },
      { 0x030d, "Gardening" },
      { 0x030e, "Travel" },
      { 0x030f, "Technology" },
      { 0x0310, "Arts" },
      { 0x0311, "Lifestyle" },
      { 0x0312, "Home" },
      { 0x0313, "Magazine" },
      { 0x0314, "Medical" },
      { 0x0315, "Reviews" },
      { 0x0316, "Antiques" },
      { 0x0317, "Motors" },
      { 0x0318, "Art & Literature" },   // ?
      { 0x0319, "Ballet" },             // ?
      { 0x031a, "Opera" },

      { 0x0401, "Classical" },
      { 0x0402, "Folk & Country" },
      { 0x0403, "National Music" },
      { 0x0404, "Jazz" },               // ?
      { 0x0405, "Opera" },              // ?
      { 0x0406, "Rock & Pop" },
      { 0x0407, "Alternative Music" },
      { 0x0408, "Events" },
      { 0x0409, "Club & Dance" },
      { 0x040a, "Hip Hop" },
      { 0x040b, "Soul/R+B" },
      { 0x040c, "Dance" },
      { 0x040d, "Ballet" },             // ?
      // unknown
      { 0x040f, "Current Affairs" },    // ?
      { 0x0410, "Features" },
      { 0x0411, "Arts & Literature" },  // ?
      { 0x0412, "Factual" },
      // unknown
      { 0x0415, "Lifestyle" },          // ?
      { 0x0416, "News & Weather" },     // ?
      { 0x0417, "Easy Listening" },     // ?
      { 0x0418, "Discussion" },         // ?
      { 0x0419, "Entertainment" },
      { 0x041a, "Religious" },

      { 0x0501, "Business" },
      { 0x0502, "World Cultures" },
      { 0x0503, "Adventure" },
      { 0x0504, "Biography" },          // ?
      { 0x0505, "Educational" },
      { 0x0506, "Features" },
      { 0x0507, "Politics" },
      { 0x0508, "News" },
      { 0x0509, "Nature" },
      { 0x050a, "Religious" },
      { 0x050b, "Science" },
      { 0x050c, "Showbiz" },
      { 0x050d, "War Documentaries" },
      { 0x050e, "Historical" },
      { 0x050f, "Ancient" },            // ?
      { 0x0510, "Transport" },          // ?
      { 0x0511, "Docudrama" },          // ?
      { 0x0512, "World Affairs" },      // ?
      { 0x0513, "Features" },           // ?
      { 0x0514, "Showbiz" },            // ?
      { 0x0515, "Politics" },           // ?
      // unknown
      { 0x0517, "World Affairs" },      // ?

      { 0x0601, "Action" },
      { 0x0602, "Animation" },
      // unknown
      { 0x0604, "Comedy" },
      { 0x0605, "Family" },
      { 0x0606, "Drama" },
      // unknown
      { 0x0608, "Sci-Fi" },
      { 0x0609, "Thriller" },
      { 0x060a, "Horror" },
      { 0x060b, "Romance" },
      { 0x060c, "Musical" },
      { 0x060d, "Mystery" },
      { 0x060e, "Western" },
      { 0x060f, "Factual" },
      { 0x0610, "Fantasy" },
      { 0x0611, "Erotic" },
      { 0x0612, "Adventure" },
      { 0x0613, "War" },

      { 0x0701, "American Football" },  // ?
      { 0x0702, "Athletics" },
      { 0x0703, "Baseball" },
      { 0x0704, "Basketball" },
      { 0x0705, "Boxing" },
      { 0x0706, "Cricket" },
      { 0x0707, "Fishing" },
      { 0x0708, "Football" },
      { 0x0709, "Golf" },
      { 0x070a, "Ice Hockey" },
      { 0x070b, "Motor Sport" },
      { 0x070c, "Racing" },
      { 0x070d, "Rugby" },
      { 0x070e, "Equestrian" },
      { 0x070f, "Winter Sports" },
      { 0x0710, "Snooker/Pool" },
      { 0x0711, "Tennis" },
      { 0x0712, "Wrestling" },
      { 0x0713, "Darts" },              // ?
      { 0x0714, "Watersports" },        // ?
      { 0x0715, "Extreme" },
      { 0x0716, "Other" }               // ?
    };

    #endregion

    #endregion

    #endregion

    #region variables

    /// <summary>
    /// Indicator: is the grabber grabbing electronic programme guide data?
    /// </summary>
    private bool _isGrabbing = false;

    /// <summary>
    /// The set of protocols that the grabber is configured to grab.
    /// </summary>
    private TunerEpgGrabberProtocol _grabProtocols = TunerEpgGrabberProtocol.None;

    /// <summary>
    /// A delegate to notify about grab progress.
    /// </summary>
    //private IEpgGrabberCallBack _callBack = null;

    #region DVB

    /// <summary>
    /// Indicator: should the grabber grab DVB EPG data?
    /// </summary>
    private bool _grabDvb = false;

    /// <summary>
    /// The DVB EPG grabber.
    /// </summary>
    private IGrabberEpgDvb _grabberDvb = null;

    /// <summary>
    /// Indicator: has the grabber seen DVB EPG data?
    /// </summary>
    private bool _isSeenDvb = false;

    /// <summary>
    /// Indicator: has the grabber received all DVB EPG data?
    /// </summary>
    private bool _isCompleteDvb = false;

    #endregion

    #region MediaHighway

    /// <summary>
    /// Indicator: should the grabber grab MediaHighway EPG data?
    /// </summary>
    private bool _grabMhw = false;

    /// <summary>
    /// The MediaHighway EPG grabber.
    /// </summary>
    private IGrabberEpgMhw _grabberMhw = null;

    /// <summary>
    /// Indicator: has the grabber seen MediaHighway EPG data?
    /// </summary>
    private bool _isSeenMhw = false;

    /// <summary>
    /// Indicator: has the grabber received all MediaHighway EPG data?
    /// </summary>
    private bool _isCompleteMhw = false;

    #endregion

    #region OpenTV

    /// <summary>
    /// Indicator: should the grabber grab OpenTV EPG data?
    /// </summary>
    private bool _grabOpenTv = false;

    /// <summary>
    /// The OpenTV EPG grabber.
    /// </summary>
    private IGrabberEpgOpenTv _grabberOpenTv = null;

    /// <summary>
    /// Indicator: has the grabber seen OpenTV EPG data?
    /// </summary>
    private bool _isSeenOpenTv = false;

    /// <summary>
    /// Indicator: has the grabber received all OpenTV EPG data?
    /// </summary>
    private bool _isCompleteOpenTv = false;

    #endregion

    #endregion

    private IList<Tuple<IChannel, IList<EpgProgram>>> CollectMediaHighwayData()//IChannelDvb currentTuningDetail)
    {
      uint eventCount;
      Iso639Code language;
      _grabberMhw.GetEventCount(out eventCount, out language);
      Log.Log.Debug("EPG DVB: MediaHighway, event count = {0}, text language = {1}", eventCount, language.Code);
      IDictionary<ulong, List<EpgProgram>> tempData = new Dictionary<ulong, List<EpgProgram>>(100);

      const ushort BUFFER_SIZE_SERVICE_NAME = 300;
      IntPtr bufferServiceName = Marshal.AllocCoTaskMem(BUFFER_SIZE_SERVICE_NAME);
      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_DESCRIPTION = 1000;
      IntPtr bufferDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_DESCRIPTION);
      const ushort BUFFER_SIZE_SEASON_NAME = 1000;
      IntPtr bufferSeasonName = Marshal.AllocCoTaskMem(BUFFER_SIZE_SEASON_NAME);
      const ushort BUFFER_SIZE_EPISODE_NAME = 1000;
      IntPtr bufferEpisodeName = Marshal.AllocCoTaskMem(BUFFER_SIZE_EPISODE_NAME);
      const ushort BUFFER_SIZE_THEME_NAME = 50;
      IntPtr bufferThemeName = Marshal.AllocCoTaskMem(BUFFER_SIZE_THEME_NAME);
      const ushort BUFFER_SIZE_SUB_THEME_NAME = 50;
      IntPtr bufferSubThemeName = Marshal.AllocCoTaskMem(BUFFER_SIZE_SUB_THEME_NAME);
      try
      {
        uint eventId;
        byte version;
        ushort originalNetworkId;
        ushort transportStreamId;
        ushort serviceId;
        ulong startDateTimeEpoch;
        ushort duration;
        byte descriptionLineCount;
        uint seriesId;
        byte seasonNumber;
        uint episodeId;
        ushort episodeNumber;
        byte classification;
        bool isHighDefinition;
        bool hasSubtitles;
        bool isRecommended;
        bool isPayPerView;
        uint payPerViewId;
        for (uint i = 0; i < eventCount; i++)
        {
          ushort bufferSizeServiceName = BUFFER_SIZE_SERVICE_NAME;
          ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
          ushort bufferSizeDescription = BUFFER_SIZE_DESCRIPTION;
          ushort bufferSizeSeasonName = BUFFER_SIZE_SEASON_NAME;
          ushort bufferSizeEpisodeName = BUFFER_SIZE_EPISODE_NAME;
          ushort bufferSizeThemeName = BUFFER_SIZE_THEME_NAME;
          ushort bufferSizeSubThemeName = BUFFER_SIZE_SUB_THEME_NAME;
          bool result = _grabberMhw.GetEvent(i,
                                              out version,
                                              out eventId,
                                              out originalNetworkId,
                                              out transportStreamId,
                                              out serviceId,
                                              bufferServiceName,
                                              ref bufferSizeServiceName,
                                              out startDateTimeEpoch,
                                              out duration,
                                              bufferTitle,
                                              ref bufferSizeTitle,
                                              bufferDescription,
                                              ref bufferSizeDescription,
                                              out descriptionLineCount,
                                              out seriesId,
                                              bufferSeasonName,
                                              ref bufferSizeSeasonName,
                                              out episodeId,
                                              out episodeNumber,
                                              bufferEpisodeName,
                                              ref bufferSizeEpisodeName,
                                              bufferThemeName,
                                              ref bufferSizeThemeName,
                                              bufferSubThemeName,
                                              ref bufferSizeSubThemeName,
                                              out classification,
                                              out isHighDefinition,
                                              out hasSubtitles,
                                              out isRecommended,
                                              out isPayPerView,
                                              out payPerViewId);
          if (!result)
          {
            Log.Log.Error("EPG DVB: failed to get MediaHighway event, event index = {0}, event count = {1}", i, eventCount);
            continue;
          }

          string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
          if (string.IsNullOrEmpty(title))
          {
            // Placeholder or dummy event => discard.
            continue;
          }

          DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
          programStartTime = programStartTime.AddSeconds(startDateTimeEpoch);
          programStartTime = programStartTime.ToLocalTime();
          EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
          //program.Titles.Add(language.Code, title);

          string description = TidyString(DvbTextConverter.Convert(bufferDescription, bufferSizeDescription));
          for (byte j = 0; j < descriptionLineCount; j++)
          {
            bufferSizeDescription = BUFFER_SIZE_DESCRIPTION;
            if (!_grabberMhw.GetDescriptionLine(i, j, bufferDescription, ref bufferSizeDescription))
            {
              Log.Log.Error("EPG DVB: failed to get MediaHighway description line, event index = {0}, event ID = {1}, line count = {2}, line index = {3}",
                            i, eventId, descriptionLineCount, j);
            }
            else
            {
              description = string.Format("{0} {1}", description, TidyString(DvbTextConverter.Convert(bufferDescription, bufferSizeDescription)));
            }
          }
          //program.Descriptions.Add(language.Code, description);
          EpgLanguageText programText = new EpgLanguageText(language.Code, title, description, string.Empty, 0, string.Empty, -1);

          if (seriesId != 0xffffffff)
          {
            //program.SeriesId = string.Format("MediaHighway:{0}", seriesId);
          }
          string seasonName = TidyString(DvbTextConverter.Convert(bufferSeasonName, bufferSizeSeasonName));
          if (!string.IsNullOrEmpty(seasonName))
          {
            // Spanish MHW 2 (Movistar+): usually <season number>(/<season count>)?.
            Match m = MEDIA_HIGHWAY_SEASON_NAME.Match(seasonName);
            if (m.Success)
            {
              //program.SeasonNumber = int.Parse(m.Groups[1].Captures[0].Value);
            }
          }
          if (episodeId != 0xffffffff)
          {
            //program.EpisodeId = string.Format("MediaHighway:{0}", episodeId);
          }
          if (episodeNumber != 0)
          {
            //program.EpisodeNumber = episodeNumber;
          }
          string episodeName = TidyString(DvbTextConverter.Convert(bufferEpisodeName, bufferSizeEpisodeName));
          if (!string.IsNullOrEmpty(episodeName))
          {
            //program.EpisodeNames.Add(language.Code, episodeName);
          }

          string themeName = TidyString(DvbTextConverter.Convert(bufferThemeName, bufferSizeThemeName));
          if (!string.IsNullOrEmpty(themeName))
          {
            string subThemeName = TidyString(DvbTextConverter.Convert(bufferSubThemeName, bufferSizeSubThemeName));
            if (!string.IsNullOrEmpty(themeName))
            {
              themeName = string.Format("{0}: {1}", themeName, subThemeName);
            }
            programText.Genre = themeName;
            //program.Categories.Add(themeName);
          }

          // Assume Spanish MHW 2 (Movistar+) encoding.
          string classificationString;
          if (MAPPING_PROGRAM_CLASSIFICATIONS_MHW2_ESP.TryGetValue(classification, out classificationString))
          {
            programText.Classification = classificationString;
            //program.Classifications.Add("MediaHighway", classificationString);
          }

          program.Text.Add(programText);

          /*if (version == 2)
          {
            program.IsHighDefinition = isHighDefinition;
            if (hasSubtitles)
            {
              // assumption: subtitles language matches the country
              program.SubtitlesLanguages.Add(language.Code);
            }
          }*/

          ulong channelKey = ((ulong)originalNetworkId << 32) | ((ulong)transportStreamId << 16) | serviceId;
          List<EpgProgram> programs;
          if (!tempData.TryGetValue(channelKey, out programs))
          {
            programs = new List<EpgProgram>(100);
            tempData.Add(channelKey, programs);
          }
          programs.Add(program);
        }

        IList<Tuple<IChannel, IList<EpgProgram>>> data = new List<Tuple<IChannel, IList<EpgProgram>>>(tempData.Count);
        int validEventCount = 0;
        foreach (var channelData in tempData)
        {
          DVBBaseChannel dvbCompatibleChannel = CreateChannel((int)(channelData.Key >> 32), (int)((channelData.Key >> 16) & 0xffff), (int)(channelData.Key & 0xffff), "");
          EpgChannel epgChannel = new EpgChannel();
          epgChannel.Channel = dvbCompatibleChannel;
          if (FilterOutEPGChannel(epgChannel))
          {
            Log.Log.Debug("EPG DVB: discarding MHW events for channel from different transponder, event count = {0}, original network ID = {1}, transport stream ID = {2}, service ID = {3}", channelData.Value.Count, dvbCompatibleChannel.NetworkId, dvbCompatibleChannel.TransportId, dvbCompatibleChannel.ServiceId);
            continue;
          }
          /*IChannelDvbCompatible dvbCompatibleChannel = currentTuningDetail.Clone() as IChannelDvbCompatible;
          dvbCompatibleChannel.OriginalNetworkId = (int)(channel.Key >> 32);
          dvbCompatibleChannel.TransportStreamId = (int)((channel.Key >> 16) & 0xffff);
          dvbCompatibleChannel.ServiceId = (int)channel.Key & 0xffff;
          IChannelOpenTv openTvChannel = dvbCompatibleChannel as IChannelOpenTv;
          if (openTvChannel != null)
          {
            openTvChannel.OpenTvChannelId = 0;
          }*/
          channelData.Value.Sort();
          data.Add(new Tuple<IChannel, IList<EpgProgram>>(dvbCompatibleChannel, channelData.Value));
          validEventCount += channelData.Value.Count;
        }

        Log.Log.Debug("EPG DVB: MediaHighway, channel count = {0}, event count = {1}", tempData.Count, validEventCount);
        return data;
      }
      finally
      {
        if (bufferServiceName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferServiceName);
        }
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferDescription);
        }
        if (bufferSeasonName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferSeasonName);
        }
        if (bufferEpisodeName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferEpisodeName);
        }
        if (bufferThemeName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferThemeName);
        }
        if (bufferSubThemeName != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferSubThemeName);
        }
      }
    }

    private IList<Tuple<IChannel, IList<EpgProgram>>> CollectOpenTvData()//IChannelOpenTv currentTuningDetail)
    {
      uint eventCount;
      ushort originalNetworkId;
      Iso639Code language;
      _grabberOpenTv.GetEventCount(out eventCount, out originalNetworkId, out language);
      Log.Log.Debug("EPG DVB: OpenTV, initial event count = {0}, original network ID = {1}, text language = {2}", eventCount, originalNetworkId, language.Code);
      IDictionary<ushort, List<EpgProgram>> tempData = new Dictionary<ushort, List<EpgProgram>>(100);

      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_SHORT_DESCRIPTION = 1000;
      IntPtr bufferShortDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_SHORT_DESCRIPTION);
      const ushort BUFFER_SIZE_EXTENDED_DESCRIPTION = 1000;
      IntPtr bufferExtendedDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_EXTENDED_DESCRIPTION);
      try
      {
        ushort channelId;
        ushort eventId;
        ulong startDateTimeEpoch;
        ushort duration;
        byte categoryId;
        byte subCategoryId;
        bool isHighDefinition;
        bool hasSubtitles;
        byte parentalRating;
        ushort seriesLinkId;
        for (uint i = 0; i < eventCount; i++)
        {
          ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
          ushort bufferSizeShortDescription = BUFFER_SIZE_SHORT_DESCRIPTION;
          ushort bufferSizeExtendedDescription = BUFFER_SIZE_EXTENDED_DESCRIPTION;
          bool result = _grabberOpenTv.GetEvent(i,
                                                out channelId,
                                                out eventId,
                                                out startDateTimeEpoch,
                                                out duration,
                                                bufferTitle,
                                                ref bufferSizeTitle,
                                                bufferShortDescription,
                                                ref bufferSizeShortDescription,
                                                bufferExtendedDescription,
                                                ref bufferSizeExtendedDescription,
                                                out categoryId,
                                                out subCategoryId,
                                                out isHighDefinition,
                                                out hasSubtitles,
                                                out parentalRating,
                                                out seriesLinkId);
          if (!result)
          {
            Log.Log.Error("EPG DVB: failed to get OpenTV event, event index = {0}, event count = {1}", i, eventCount);
            continue;
          }

          string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
          if (string.IsNullOrEmpty(title))
          {
            // Placeholder or dummy event => discard.
            continue;
          }

          DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
          programStartTime = programStartTime.AddSeconds(startDateTimeEpoch);
          programStartTime = programStartTime.ToLocalTime();
          EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));
          //program.Titles.Add(language.Code, title);
          //program.Categories.Add(GetOpenTvProgramCategoryDescription(originalNetworkId, categoryId, subCategoryId));
          //program.IsHighDefinition = isHighDefinition;
          if (hasSubtitles)
          {
            // assumption: subtitles language matches the country
            //program.SubtitlesLanguages.Add(language.Code);
          }
          string classification = GetOpenTvParentalRatingDescription(originalNetworkId, parentalRating);
          if (classification != null)
          {
            //program.Classifications.Add("OpenTV", classification);
          }
          if (seriesLinkId != 0 && seriesLinkId != 0xffff)
          {
            //program.SeriesId = string.Format("OpenTV:{0}", seriesLinkId);
          }

          // When available, extended description seems to contain various event attributes.
          string description = TidyString(DvbTextConverter.Convert(bufferShortDescription, bufferSizeShortDescription));
          string extendedDescription = TidyString(DvbTextConverter.Convert(bufferExtendedDescription, bufferSizeExtendedDescription));
          if (string.IsNullOrEmpty(description) || string.Equals(title, description))
          {
            description = extendedDescription;
          }
          else if (!string.IsNullOrEmpty(extendedDescription) && !description.Contains(extendedDescription))
          {
            if (extendedDescription.Contains(description))
            {
              description = extendedDescription;
            }
            else
            {
              description += Environment.NewLine + extendedDescription;
            }
          }
          //program.Descriptions.Add(language.Code, description);

          program.Text.Add(new EpgLanguageText(language.Code, title, description, GetOpenTvProgramCategoryDescription(originalNetworkId, categoryId, subCategoryId), 0, classification ?? string.Empty, -1));

          List<EpgProgram> programs;
          if (!tempData.TryGetValue(channelId, out programs))
          {
            programs = new List<EpgProgram>(100);
            tempData.Add(channelId, programs);
          }
          programs.Add(program);
        }

        IDictionary<ushort, ulong> dvbIds = ReadOpenTvChannelDvbIds();
        IList<Tuple<IChannel, IList<EpgProgram>>> data = new List<Tuple<IChannel, IList<EpgProgram>>>(tempData.Count);
        int validEventCount = 0;
        foreach (var channelData in tempData)
        {
          ulong dvbId;
          if (!dvbIds.TryGetValue(channelData.Key, out dvbId))
          {
            Log.Log.Error("EPG DVB: failed to determine DVB ID for OpenTV channel {0}", channelData.Key);
            continue;
          }
          DVBBaseChannel openTvChannel = CreateChannel((int)(dvbId >> 32), (int)((dvbId >> 16) & 0xffff), (int)dvbId & 0xffff, "");
          EpgChannel epgChannel = new EpgChannel();
          epgChannel.Channel = openTvChannel;
          if (FilterOutEPGChannel(epgChannel))
          {
            Log.Log.Debug("EPG DVB: discarding OpenTV events for channel from different transponder, event count = {0}, original network ID = {1}, transport stream ID = {2}, service ID = {3}, OpenTV channel ID = {4}", channelData.Value.Count, openTvChannel.NetworkId, openTvChannel.TransportId, openTvChannel.ServiceId, channelData.Key);
            continue;
          }
          /*IChannelOpenTv openTvChannel = currentTuningDetail.Clone() as IChannelOpenTv;
          openTvChannel.OpenTvChannelId = channel.Key;*/
          channelData.Value.Sort();
          data.Add(new Tuple<IChannel, IList<EpgProgram>>(openTvChannel, channelData.Value));
          validEventCount += channelData.Value.Count;
        }

        Log.Log.Debug("EPG DVB: OpenTV, channel count = {0}, event count = {1}", tempData.Count, validEventCount);
        return data;
      }
      finally
      {
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferShortDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferShortDescription);
        }
        if (bufferExtendedDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferExtendedDescription);
        }
      }
    }

    private IDictionary<ushort, ulong> ReadOpenTvChannelDvbIds()
    {
      Dictionary<ushort, ulong> ids = new Dictionary<ushort, ulong>(500);
      IGrabberSiDvb grabber = _filterTsWriter as IGrabberSiDvb;
      if (grabber == null)
      {
        return ids;
      }
      ushort serviceCount;
      ushort actualOnid;
      grabber.GetServiceCount(out actualOnid, out serviceCount);

      byte tableId;
      ushort originalNetworkId;
      ushort transportStreamId;
      ushort serviceId;
      ushort referenceServiceId;
      ushort freesatChannelId;
      ushort openTvChannelId;
      ushort logicalChannelNumberCount = 0;
      byte dishSubChannelNumber;
      bool eitScheduleFlag;
      bool eitPresentFollowingFlag;
      RunningStatus runningStatus;
      bool freeCaMode;
      Mediaportal.TV.Server.TVLibrary.Implementations.Dvb.Enum.ServiceType serviceType;
      byte serviceNameCount;
      bool visibleInGuide;
      ushort streamCountVideo;
      ushort streamCountAudio;
      bool isHighDefinition;
      bool isStandardDefinition;
      bool isThreeDimensional;
      byte audioLanguageCount = 0;
      byte subtitlesLanguageCount = 0;
      byte networkIdCount = 0;
      byte bouquetIdCount = 0;
      byte availableInCountryCount = 0;
      byte unavailableInCountryCount = 0;
      byte availableInCellCount = 0;
      byte unavailableInCellCount = 0;
      byte targetRegionIdCount = 0;
      byte freesatRegionIdCount = 0;
      byte openTvRegionIdCount = 0;
      byte cyfrowyPolsatChannelCategoryId;
      byte freesatChannelCategoryIdCount = 0;
      byte mediaHighwayChannelCategoryIdCount = 0;
      byte openTvChannelCategoryIdCount = 0;
      byte virginMediaChannelCategoryId;
      ushort dishNetworkMarketId;
      byte norDigChannelListIdCount = 0;
      ushort previousOriginalNetworkId;
      ushort previousTransportStreamId;
      ushort previousServiceId;
      ushort epgOriginalNetworkId;
      ushort epgTransportStreamId;
      ushort epgServiceId;
      for (ushort i = 0; i < serviceCount; i++)
      {
        if (grabber.GetService(i,
                                out tableId, out originalNetworkId, out transportStreamId, out serviceId, out referenceServiceId,
                                out freesatChannelId, out openTvChannelId,
                                null, ref logicalChannelNumberCount, out dishSubChannelNumber,
                                out eitScheduleFlag, out eitPresentFollowingFlag,
                                out runningStatus, out freeCaMode, out serviceType, out serviceNameCount, out visibleInGuide,
                                out streamCountVideo, out streamCountAudio,
                                out isHighDefinition, out isStandardDefinition, out isThreeDimensional,
                                null, ref audioLanguageCount,
                                null, ref subtitlesLanguageCount,
                                null, ref networkIdCount,
                                null, ref bouquetIdCount,
                                null, ref availableInCountryCount, null, ref unavailableInCountryCount,
                                null, ref availableInCellCount, null, ref unavailableInCellCount,
                                null, ref targetRegionIdCount,
                                null, ref freesatRegionIdCount,
                                null, ref openTvRegionIdCount,
                                out cyfrowyPolsatChannelCategoryId,
                                null, ref freesatChannelCategoryIdCount,
                                null, ref mediaHighwayChannelCategoryIdCount,
                                null, ref openTvChannelCategoryIdCount,
                                out virginMediaChannelCategoryId,
                                out dishNetworkMarketId,
                                null, ref norDigChannelListIdCount,
                                out previousOriginalNetworkId, out previousTransportStreamId, out previousServiceId,
                                out epgOriginalNetworkId, out epgTransportStreamId, out epgServiceId))
        {
          if (openTvChannelId != 0 && openTvChannelId != 0xffff)
          {
            ids.Add(openTvChannelId, ((ulong)originalNetworkId << 32) | ((ulong)transportStreamId << 16) | serviceId);
          }
        }
      }
      return ids;
    }

    private IList<Tuple<IChannel, IList<EpgProgram>>> CollectEitData()//IChannelDvbCompatible currentTuningDetail)
    {
      ushort serviceCount = _grabberDvb.GetServiceCount();
      Log.Log.Debug("EPG DVB: EIT, initial service count = {0}", serviceCount);
      IList<Tuple<IChannel, IList<EpgProgram>>> data = new List<Tuple<IChannel, IList<EpgProgram>>>(serviceCount);
      if (serviceCount == 0)
      {
        return data;
      }

      const ushort BUFFER_SIZE_SERIES_ID = 300;
      IntPtr bufferSeriesId = Marshal.AllocCoTaskMem(BUFFER_SIZE_SERIES_ID);
      const ushort BUFFER_SIZE_EPISODE_ID = 300;
      IntPtr bufferEpisodeId = Marshal.AllocCoTaskMem(BUFFER_SIZE_EPISODE_ID);
      const byte ARRAY_SIZE_AUDIO_LANGUAGES = 20;
      const byte ARRAY_SIZE_SUBTITLES_LANGUAGES = 20;
      const byte ARRAY_SIZE_DVB_CONTENT_TYPE_IDS = 10;
      const byte ARRAY_SIZE_DVB_PARENTAL_RATINGS = 10;
      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_SHORT_DESCRIPTION = 1000;
      IntPtr bufferShortDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_SHORT_DESCRIPTION);
      const ushort BUFFER_SIZE_EXTENDED_DESCRIPTION = 1000;
      IntPtr bufferExtendedDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_EXTENDED_DESCRIPTION);
      try
      {
        ushort originalNetworkId;
        ushort transportStreamId;
        ushort serviceId;
        ushort eventCount;
        ulong eventId;
        ulong startDateTimeEpoch;
        uint duration;
        RunningStatus runningStatus;
        bool freeCaMode;
        ushort referenceServiceId;
        ulong referenceEventId;
        bool isHighDefinition;
        bool isStandardDefinition;
        bool isThreeDimensional;
        bool isPreviouslyShown;
        Iso639Code[] audioLanguages = new Iso639Code[ARRAY_SIZE_AUDIO_LANGUAGES];
        Iso639Code[] subtitlesLanguages = new Iso639Code[ARRAY_SIZE_SUBTITLES_LANGUAGES];
        ushort[] dvbContentTypeIds = new ushort[ARRAY_SIZE_DVB_CONTENT_TYPE_IDS];
        Iso639Code[] dvbParentalRatingCountryCodes = new Iso639Code[ARRAY_SIZE_DVB_PARENTAL_RATINGS];
        byte[] dvbParentalRatings = new byte[ARRAY_SIZE_DVB_PARENTAL_RATINGS];
        byte starRating;
        byte mpaaClassification;
        ushort dishBevAdvisories;
        byte vchipRating;
        byte textCount;
        Iso639Code language;
        byte descriptionItemCount;
        int validEventCount = 0;
        for (ushort i = 0; i < serviceCount; i++)
        {
          if (!_grabberDvb.GetService(i, out originalNetworkId, out transportStreamId, out serviceId, out eventCount))
          {
            Log.Log.Error("EPG DVB: failed to get EIT service, service index = {0}, service count = {1}", i, serviceCount);
            continue;
          }
          EpgChannel epgChannel = new EpgChannel();
          epgChannel.Channel = CreateChannel(originalNetworkId, transportStreamId, serviceId, "");
          if (FilterOutEPGChannel(epgChannel))
          {
            Log.Log.Debug("EPG DVB: discarding EIT events for channel from different transponder, event count = {0}, original network ID = {1}, transport stream ID = {2}, service ID = {3}", eventCount, originalNetworkId, transportStreamId, serviceId);
            continue;
          }

          List<EpgProgram> programs = new List<EpgProgram>(eventCount);
          for (ushort j = 0; j < eventCount; j++)
          {
            ushort bufferSizeSeriesId = BUFFER_SIZE_SERIES_ID;
            ushort bufferSizeEpisodeId = BUFFER_SIZE_EPISODE_ID;
            byte countAudioLanguages = ARRAY_SIZE_AUDIO_LANGUAGES;
            byte countSubtitlesLanguages = ARRAY_SIZE_SUBTITLES_LANGUAGES;
            byte countDvbContentTypeIds = ARRAY_SIZE_DVB_CONTENT_TYPE_IDS;
            byte countDvbParentalRatings = ARRAY_SIZE_DVB_PARENTAL_RATINGS;
            bool result = _grabberDvb.GetEvent(i, j,
                                                out eventId,
                                                out startDateTimeEpoch,
                                                out duration,
                                                out runningStatus,
                                                out freeCaMode,
                                                out referenceServiceId,
                                                out referenceEventId,
                                                bufferSeriesId,
                                                ref bufferSizeSeriesId,
                                                bufferEpisodeId,
                                                ref bufferSizeEpisodeId,
                                                out isHighDefinition,
                                                out isStandardDefinition,
                                                out isThreeDimensional,
                                                out isPreviouslyShown,
                                                audioLanguages,
                                                ref countAudioLanguages,
                                                subtitlesLanguages,
                                                ref countSubtitlesLanguages,
                                                dvbContentTypeIds,
                                                ref countDvbContentTypeIds,
                                                dvbParentalRatingCountryCodes,
                                                dvbParentalRatings,
                                                ref countDvbParentalRatings,
                                                out starRating,
                                                out mpaaClassification,
                                                out dishBevAdvisories,
                                                out vchipRating,
                                                out textCount);
            if (!result)
            {
              Log.Log.Error("EPG DVB: failed to get EIT event, service index = {0}, service count = {1}, event index = {2}, event count = {3}", i, serviceCount, j, eventCount);
              continue;
            }

            DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            programStartTime = programStartTime.AddSeconds(startDateTimeEpoch);
            programStartTime = programStartTime.ToLocalTime();
            EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddSeconds(duration));

            bool isPlaceholderOrDummyEvent = false;
            for (byte k = 0; k < textCount; k++)
            {
              ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
              ushort bufferSizeShortDescription = BUFFER_SIZE_SHORT_DESCRIPTION;
              ushort bufferSizeExtendedDescription = BUFFER_SIZE_EXTENDED_DESCRIPTION;
              result = _grabberDvb.GetEventText(i, j, k,
                                                out language,
                                                bufferTitle,
                                                ref bufferSizeTitle,
                                                bufferShortDescription,
                                                ref bufferSizeShortDescription,
                                                bufferExtendedDescription,
                                                ref bufferSizeExtendedDescription,
                                                out descriptionItemCount);
              if (!result)
              {
                Log.Log.Error("EPG DVB: failed to get EIT event text, service index = {0}, service count = {1}, event index = {2}, event count = {3}, text index = {4}, text count = {5}",
                              i, serviceCount, j, eventCount, k, textCount);
                continue;
              }

              string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
              if (string.IsNullOrEmpty(title) || title.Equals("."))
              {
                isPlaceholderOrDummyEvent = true;
                break;
              }
              //program.Titles.Add(language.Code, title);

              // When available, extended description seems to contain various event attributes.
              string description = TidyString(DvbTextConverter.Convert(bufferShortDescription, bufferSizeShortDescription));
              string extendedDescription = TidyString(DvbTextConverter.Convert(bufferExtendedDescription, bufferSizeExtendedDescription));
              if (string.IsNullOrEmpty(description) || string.Equals(title, description))
              {
                description = extendedDescription;
              }
              else if (!string.IsNullOrEmpty(extendedDescription) && !description.Contains(extendedDescription))
              {
                if (extendedDescription.Contains(description))
                {
                  description = extendedDescription;
                }
                else
                {
                  description += Environment.NewLine + extendedDescription;
                }
              }

              // Dish Network
              if (OriginalNetwork.IsDishNetwork(originalNetworkId))
              {
                description = ParseDishDescription(description, program);
              }

              Dictionary<string, string> premiereItems = new Dictionary<string, string>(5);
              for (byte l = 0; l < descriptionItemCount; l++)
              {
                // Note: reusing buffers here.
                ushort bufferSizeDescription = BUFFER_SIZE_SHORT_DESCRIPTION;
                ushort bufferSizeText = BUFFER_SIZE_EXTENDED_DESCRIPTION;
                result = _grabberDvb.GetEventDescriptionItem(i, j, k, l, bufferShortDescription, ref bufferSizeDescription, bufferExtendedDescription, ref bufferSizeText);
                if (!result)
                {
                  Log.Log.Error("EPG DVB: failed to get EIT event description item, service index = {0}, service count = {1}, event index = {2}, event count = {3}, text index = {4}, text count = {5}, item index = {6}, item count = {7}",
                                i, serviceCount, j, eventCount, k, textCount, l, descriptionItemCount);
                  continue;
                }

                string itemDescription = TidyString(DvbTextConverter.Convert(bufferShortDescription, bufferSizeDescription));
                string itemText = TidyString(DvbTextConverter.Convert(bufferExtendedDescription, bufferSizeText));
                if (itemDescription.StartsWith("Premiere order "))
                {
                  premiereItems.Add(itemDescription, itemText);
                }
                else
                {
                  HandleDescriptionItem(itemDescription, itemText, program, ref description);
                }
              }

              // Handle Premiere order items separately for display order consistency.
              if (premiereItems.Count > 0)
              {
                if (!string.IsNullOrEmpty(description))
                {
                  description += Environment.NewLine;
                }
                description += string.Format("Bestellnummer: {0}", premiereItems["Premiere order number"]);
                description += string.Format("{0}Preis: {1}", Environment.NewLine, premiereItems["Premiere order price"]);
                description += string.Format("{0}Telefonnummer: {1}", Environment.NewLine, premiereItems["Premiere order phone number"]);
                description += string.Format("{0}SMS: {1}", Environment.NewLine, premiereItems["Premiere order SMS number"]);
                description += string.Format("{0}URL: {1}", Environment.NewLine, premiereItems["Premiere order URL"]);
              }

              program.Text.Add(new EpgLanguageText(language.Code, title, description, string.Empty, 0, string.Empty, -1));
              //program.Descriptions.Add(language.Code, description);
            }

            if (isPlaceholderOrDummyEvent)
            {
              continue;
            }

            if (isHighDefinition)
            {
              //program.IsHighDefinition = true;
            }
            else if (isStandardDefinition)
            {
              //program.IsHighDefinition = false;
            }
            if (isThreeDimensional)
            {
              //program.IsThreeDimensional = true;
            }
            if (isPreviouslyShown)
            {
              //program.IsPreviouslyShown = true;
            }
            if (bufferSizeSeriesId > 0)
            {
              //program.SeriesId = DvbTextConverter.Convert(bufferSeriesId, bufferSizeSeriesId);
            }
            if (bufferSizeEpisodeId > 0)
            {
              //program.EpisodeId = DvbTextConverter.Convert(bufferEpisodeId, bufferSizeEpisodeId);
            }
            for (byte x = 0; x < countAudioLanguages; x++)
            {
              //program.AudioLanguages.Add(audioLanguages[x].Code);
            }
            for (byte x = 0; x < countSubtitlesLanguages; x++)
            {
              //program.SubtitlesLanguages.Add(subtitlesLanguages[x].Code);
            }
            for (byte x = 0; x < countDvbContentTypeIds; x++)
            {
              bool? tempIsLive;
              bool? tempIsThreeDimensional;
              //program.Categories.Add(GetContentTypeDescription(dvbContentTypeIds[x], originalNetworkId, out tempIsLive, out tempIsThreeDimensional));
              string contentTypeDescription = GetContentTypeDescription(dvbContentTypeIds[x], originalNetworkId, out tempIsLive, out tempIsThreeDimensional);
              if (tempIsLive.HasValue)
              {
                //program.IsLive = tempIsLive;
              }
              else if (tempIsThreeDimensional.HasValue)
              {
                //program.IsThreeDimensional |= tempIsThreeDimensional;
              }
              else if (contentTypeDescription != null)
              {
                foreach (var text in program.Text)
                {
                  if (!string.IsNullOrEmpty(text.Genre))
                  {
                    text.Genre += ", ";
                  }
                  text.Genre += contentTypeDescription;
                }
              }
            }
            for (byte x = 0; x < countDvbParentalRatings; x++)
            {
              string parentalRating = GetParentalRatingDescription(dvbParentalRatings[x]);
              if (parentalRating != null)
              {
                if (countDvbParentalRatings == 1)
                {
                  foreach (var text in program.Text)
                  {
                    if (!string.IsNullOrEmpty(text.Classification))
                    {
                      text.Classification += ", ";
                    }
                    text.Classification += parentalRating;
                    text.ParentalRating = dvbParentalRatings[x] + 3;
                  }
                }
                else
                {
                  foreach (var text in program.Text)
                  {
                    if (text.Language == dvbParentalRatingCountryCodes[x].Code)
                    {
                      if (!string.IsNullOrEmpty(text.Classification))
                      {
                        text.Classification += ", ";
                      }
                      text.Classification += parentalRating;
                      text.ParentalRating = dvbParentalRatings[x] + 3;
                    }
                    else if (text.ParentalRating == 0)
                    {
                      text.ParentalRating = dvbParentalRatings[x] + 3;
                    }
                  }
                }
                //program.Classifications.Add(dvbParentalRatingCountryCodes[x].Code, parentalRating);
              }
            }
            if (starRating > 0 && starRating < 8)
            {
              foreach (var text in program.Text)
              {
                text.StarRating = starRating;
              }
              //program.StarRating = (starRating + 1) / 2;    // 1 = 1 star, 2 = 1.5 stars etc.; max. value = 7
              //program.StarRatingMaximum = 4;
            }
            string mpaaClassificationDescription;
            if (MAPPING_PROGRAM_CLASSIFICATIONS_MPAA.TryGetValue(mpaaClassification, out mpaaClassificationDescription))
            {
              foreach (var text in program.Text)
              {
                if (!string.IsNullOrEmpty(text.Classification))
                {
                  text.Classification += ", ";
                }
                text.Classification += mpaaClassificationDescription;
              }
              //program.Classifications.Add("MPAA", mpaaClassificationDescription);
            }
            //program.Advisories = GetContentAdvisories(dishBevAdvisories);
            string vchipRatingDescription;
            if (MAPPING_PROGRAM_RATINGS_VCHIP.TryGetValue(vchipRating, out vchipRatingDescription))
            {
              foreach (var text in program.Text)
              {
                if (!string.IsNullOrEmpty(text.Classification))
                {
                  text.Classification += ", ";
                }
                text.Classification += vchipRatingDescription;
              }
              //program.Classifications.Add("V-Chip", vchipRatingDescription);
            }

            programs.Add(program);
          }

          /*IChannelDvbCompatible dvbCompatibleChannel = currentTuningDetail.Clone() as IChannelDvbCompatible;
          dvbCompatibleChannel.OriginalNetworkId = originalNetworkId;
          dvbCompatibleChannel.TransportStreamId = transportStreamId;
          dvbCompatibleChannel.ServiceId = serviceId;
          IChannelOpenTv openTvChannel = dvbCompatibleChannel as IChannelOpenTv;
          if (openTvChannel != null)
          {
            openTvChannel.OpenTvChannelId = 0;
          }*/
          programs.Sort();
          data.Add(new Tuple<IChannel, IList<EpgProgram>>(epgChannel.Channel, programs));
          validEventCount += programs.Count;
        }

        Log.Log.Debug("EPG DVB: EIT, channel count = {0}, event count = {1}", data.Count, validEventCount);
        return data;
      }
      finally
      {
        if (bufferSeriesId != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferSeriesId);
        }
        if (bufferEpisodeId != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferEpisodeId);
        }
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferShortDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferShortDescription);
        }
        if (bufferExtendedDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferExtendedDescription);
        }
      }
    }

    private static string TidyString(string s)
    {
      if (s == null)
      {
        return string.Empty;
      }
      return s.Trim();
    }

    private static void HandleDescriptionItem(string itemName, string itemText, EpgProgram program, ref string description)
    {
      // Fake items added by TsWriter.
      if (string.Equals(itemName, "Dish episode info"))
      {
        // Example: {0}{1|Don't Worry, Speed Racer}{31|Charlie Sheen}{32|Jon Cryer}{33|Angus T. Jones}{5|When Jake tells Charlie and Alan that he often overhears Judith and Herb (Ryan Stiles) making out, it triggers a repressed memory for Charlie.}{6|CC}{7|Stereo}
        foreach (string section in itemText.Split(new char[] { '}', '{' }, StringSplitOptions.RemoveEmptyEntries))
        {
          string[] parts = section.Split('|');
          int sectionType;
          if (int.TryParse(parts[0], out sectionType))
          {
            if (sectionType == 0)
            {
              // unknown meaning
            }
            else if (parts.Length > 1)
            {
              if (sectionType == 1)
              {
                //program.EpisodeNames.Add(LANG_CODE_ENG, parts[1]);
              }
              else if (sectionType >= 31 && sectionType <= 49)  // Note: only seen 31, 32 and 33. Assuming 34-49 are the same.
              {
                /*if (!program.Actors.Contains(parts[1]))
                {
                  program.Actors.Add(parts[1]);
                }*/
              }
              else if (sectionType == 5)
              {
                string shortDescription = parts[1];
                if (shortDescription.EndsWith(" (HD)"))
                {
                  //program.IsHighDefinition = true;
                  //shortDescription.Substring(0, shortDescription.Length - " (HD)".Length);
                }
                if (shortDescription.EndsWith(" New."))
                {
                  //program.IsPreviouslyShown = false;
                  //shortDescription.Substring(0, shortDescription.Length - " New.".Length);
                }
                if (!description.Contains(shortDescription))
                {
                  description = shortDescription + Environment.NewLine + description;
                }
              }
              else if (sectionType == 6)
              {
                /*if (parts[1].Equals("CC") && !program.SubtitlesLanguages.Contains(LANG_CODE_ENG))
                {
                  program.SubtitlesLanguages.Add(LANG_CODE_ENG);
                }*/
              }
              else if (sectionType == 7)
              {
                // nothing valuable
              }
              else
              {
                Log.Log.Error("EPG DVB: failed to interpret Dish Network episode detail, section type = {0}, section content = {1}", sectionType, section);
              }
            }
            else
            {
              Log.Log.Error("EPG DVB: failed to interpret Dish Network episode detail, section type = {0}", sectionType);
            }
          }
          else
          {
            Log.Log.Error("EPG DVB: failed to interpret Dish Network episode detail, section = {0}", section);
          }
        }
        return;
      }

      if (
        string.Equals(itemName, "actors", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "int", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        // EPG Collector: int (case unknown); CSV
        // Euskaltel (Spain DVB-C): actors (case unknown)
        // StarHub TV (Singapore DVB-C): Actors; CSV with spaces
        // Welho (Finland DVB-C): Actors; CSV without spaces
        foreach (string actor in itemText.Split(','))
        {
          string actorTemp = actor.Trim();
          /*if (!string.IsNullOrEmpty(actorTemp) && !program.Actors.Contains(actorTemp))
          {
            program.Actors.Add(actorTemp);
          }*/
        }
        return;
      }

      if (
        itemName.StartsWith("director", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "dir", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        // EPG Collector: director/directors/dir (case unknown); CSV
        // StarHub TV (Singapore DVB-C): Directors; CSV with spaces
        // Welho (Finland DVB-C): Directors; CSV without spaces
        foreach (string director in itemText.Split(','))
        {
          string directorTemp = director.Trim();
          /*if (!string.IsNullOrEmpty(directorTemp) && !program.Directors.Contains(directorTemp))
          {
            program.Directors.Add(directorTemp);
          }*/
        }
        return;
      }

      if (string.Equals(itemName, "gui", StringComparison.InvariantCultureIgnoreCase))
      {
        // EPG Collector: gui (case unknown); CSV
        foreach (string writer in itemText.Split(','))
        {
          string writerTemp = writer.Trim();
          /*if (!string.IsNullOrEmpty(writerTemp) && !program.Writers.Contains(writerTemp))
          {
            program.Writers.Add(writerTemp);
          }*/
        }
        return;
      }

      if (
        string.Equals(itemName, "production year", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "year", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "año", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        // EPG Collector: year/production year/año (case unknown)
        // Welho (Finland DVB-C): Production Year; 4 digit numeric string
        ushort year;
        if (ushort.TryParse(itemText, out year))
        {
          //program.ProductionYear = year;
        }
        else
        {
          Log.Log.Error("EPG DVB: failed to interpret production year description item, item name = {0}, item text = {1}", itemName, itemText);
        }
        return;
      }

      if (
        string.Equals(itemName, "country", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "nac", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        // EPG Collector: country/nac (case unknown)
        // Welho (Finland DVB-C): Country; 3 letter ISO code
        //program.ProductionCountry = itemText;
        return;
      }

      if (
        string.Equals(itemName, "episode", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "episodetitle", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        // EPG Collector: episode (case unknown)
        // StarHub TV (Singapore DVB-C): EpisodeTitle
        //program.EpisodeNames.Add(LANG_CODE_ENG, itemText);
        return;
      }

      if (string.Equals(itemName, "tep", StringComparison.InvariantCultureIgnoreCase))
      {
        // EPG Collector: tep (case unknown); format "<season number>:<episode number>"
        string[] parts = itemText.Split(':');
        if (parts.Length == 2)
        {
          int seasonNumber;
          int episodeNumber;
          if (int.TryParse(parts[0], out seasonNumber) && int.TryParse(parts[1], out episodeNumber))
          {
            //program.SeasonNumber = seasonNumber;
            //program.EpisodeNumber = episodeNumber;
            return;
          }
        }
        Log.Log.Error("EPG DVB: failed to interpret season/episode number description item, item text = {0}", itemText);
        return;
      }
      if (string.Equals(itemName, "episodeno", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): EpisodeNo; CSV or ampersand-SV "[Ep ]\d+\s*([a-zA-Z]\s*)?(\+\s*\d+\s*([a-zA-Z]\s*)?)*(\/\s*\d+\s*)?"; examples
        // - Ep 13
        // - Ep 7A, Ep 7B, Ep 21B
        // - 0047, 0048, 0049 & 0050
        // - Ep 18/22
        // - 42
        // - Ep 326+327/365
        // - Ep 17+18
        string[] episodeNumbers = itemText.Split(new char[] { ',', '&' });
        foreach (string episodeNumberString in episodeNumbers)
        {
          string numberString = episodeNumberString;
          if (episodeNumberString.StartsWith("Ep "))
          {
            numberString = episodeNumberString.Substring(3);
          }

          // episode count is sometimes included
          string[] parts = numberString.Split(new char[1] { '/' });
          numberString = parts[0];

          // multiple episodes specified
          parts = numberString.Split(new char[1] { '+' });
          numberString = parts[0];

          Match m = Regex.Match(numberString, @"^\s*(\d+)\s*([a-zA-Z])?\s*$");
          int episodeNumber;
          if (m.Success && int.TryParse(m.Groups[1].Captures[0].Value, out episodeNumber))
          {
            //program.EpisodeNumber = episodeNumber;
            if (m.Groups[2].Captures.Count == 1)
            {
              /*program.EpisodePartNumber = m.Groups[2].Captures[0].Value[0] - 64;
              if (program.EpisodePartNumber > 26)
              {
                program.EpisodePartNumber -= 32;
              }*/
            }
            return;
          }
          break;
        }
        Log.Log.Error("EPG DVB: failed to interpret episode number description item, item text = {0}", itemText);
        return;
      }

      if (string.Equals(itemName, "seriesid", StringComparison.InvariantCultureIgnoreCase))
      {
        // EPG Collector: seriesid (case unknown)
        //program.SeriesId = itemText;
        return;
      }

      if (string.Equals(itemName, "seasonid", StringComparison.InvariantCultureIgnoreCase))
      {
        // EPG Collector: seasonid (case unknown)
        /*if (string.IsNullOrEmpty(program.SeriesId))   // Prefer series ID over season ID.
        {
          program.SeriesId = itemText;
        }*/
        return;
      }

      if (
        string.Equals(itemName, "episodeid", StringComparison.InvariantCultureIgnoreCase) ||
        string.Equals(itemName, "Contentid_ref", StringComparison.InvariantCultureIgnoreCase)
      )
      {
        // EPG Collector: episodeid (case unknown)
        // StarHub TV (Singapore DVB-C): Contentid_ref; examples "T0019319077", "TA0018483678"
        //program.EpisodeId = itemText;
        return;
      }

      if (string.Equals(itemName, "AudioTrack", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): CSV 3 letter ISO language codes, lower case
        return;
      }
      if (string.Equals(itemName, "BlackedOutInternet", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "0", "1"; probably 0/1 boolean, but meaning uncertain
        return;
      }
      if (string.Equals(itemName, "CatchupInternet", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "0", "1"; probably 0/1 boolean, but meaning uncertain
        return;
      }
      if (string.Equals(itemName, "CatchupIPTV", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "0", "1"; probably 0/1 boolean, but meaning uncertain
        return;
      }
      if (string.Equals(itemName, "DelayTV", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "0", "1"; probably 0/1 boolean, but meaning uncertain
        return;
      }
      if (string.Equals(itemName, "MasterProductionID", StringComparison.InvariantCultureIgnoreCase))
      {
        // Welho (Finland DVB-C): example "IT_SKY000032"; perhaps an episode ID?
        return;
      }
      if (string.Equals(itemName, "ProgrammeStatus", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "E", "L" (= live???), "S", "FirstRun"
        return;
      }
      if (string.Equals(itemName, "Start_over_flag", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "0", "1"; probably 0/1 boolean, but meaning uncertain
        return;
      }
      if (string.Equals(itemName, "Trick_mode_ctrl_DTV", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "0", "2"
        return;
      }
      if (string.Equals(itemName, "Trick_mode_ctrl_SO", StringComparison.InvariantCultureIgnoreCase))
      {
        // StarHub TV (Singapore DVB-C): examples "0", "2"
        return;
      }

      // unhandled items referenced by EPG Collector
      // - ppd (previous play date)
      // - star (star rating)
      // - tv ratings

      Log.Log.Error("EPG DVB: failed to interpret description item, item name = {0}, item text = {1}", itemName, itemText);
    }

    private static string GetContentTypeDescription(ushort contentTypeId, ushort originalNetworkId, out bool? isLive, out bool? isThreeDimensional)
    {
      isLive = null;
      isThreeDimensional = null;
      byte level1Id = (byte)(contentTypeId >> 12);
      if (level1Id == 0xf)  // user defined
      {
        // Echostar Communications (Dish, Bell TV)
        if (OriginalNetwork.IsDishNetwork(originalNetworkId) || OriginalNetwork.IsEchostar(originalNetworkId))
        {
          return GetDishBevContentTypeDescription(contentTypeId);
        }
      }
      else if (contentTypeId >> 8 == 0)
      {
        // Virgin Media, UK DVB-C
        if (OriginalNetwork.IsVirginMediaUk(originalNetworkId))
        {
          return GetVirginMediaContentTypeDescription(contentTypeId);
        }
      }

      byte level2Id = (byte)((contentTypeId >> 8) & 0x0f);

      // special characteristics
      if (level1Id == 0xb)
      {
        if (level2Id == 3)
        {
          isLive = true;
        }
        else if (level2Id == 4)
        {
          isThreeDimensional = true;
        }
        return null;
      }

      string level1Text;
      if (!MAPPING_CONTENT_TYPES_DVB.TryGetValue(level1Id, out level1Text))
      {
        level1Text = string.Format("DVB Reserved {0}", level1Id);
      }

      string level2Text;
      if (!MAPPING_CONTENT_SUB_TYPES_DVB.TryGetValue((byte)((level1Id << 4) | level2Id), out level2Text))
      {
        if (level2Id == 0 && level1Id < 11)
        {
          level2Text = "General";
        }
        else if (level2Id == 0xf)
        {
          level2Text = "DVB User Defined";
        }
        else
        {
          level2Text = string.Format("DVB Reserved {0}", level2Id);
        }
      }

      return string.Format("{0}: {1}", level1Text, level2Text);
    }

    private static string GetDishBevContentTypeDescription(ushort contentTypeId)
    {
      byte level1Id = (byte)((contentTypeId >> 8) & 0x0f);
      byte level2Id = (byte)(contentTypeId & 0xff);

      string level1Text;
      if (!MAPPING_CONTENT_TYPES_DISH.TryGetValue(level1Id, out level1Text))
      {
        level1Text = null;
      }

      string level2Text;
      if (!MAPPING_CONTENT_SUB_TYPES_DISH.TryGetValue(level2Id, out level2Text))
      {
        level2Text = null;
      }

      if (level1Text == null && level2Text == null)
      {
        return string.Format("Dish/BEV Content Type {0}-{1}", level1Id, level2Id);
      }
      else if (level1Text != null && level2Text != null)
      {
        return string.Format("{0}: {1}", level1Text, level2Text);
      }
      return level1Text ?? level2Text;
    }

    private static string GetVirginMediaContentTypeDescription(ushort contentTypeId)
    {
      byte level1Id = (byte)((contentTypeId >> 4) & 0xf);
      byte level2Id = (byte)(contentTypeId & 0xf);

      string level1Text;
      if (!MAPPING_CONTENT_TYPES_VIRGIN_MEDIA.TryGetValue(level1Id, out level1Text))
      {
        return string.Format("Virgin Media Content Type {0}-{1}", level1Id, level2Id);
      }

      string level2Text;
      if (!MAPPING_CONTENT_SUB_TYPES_VIRGIN_MEDIA.TryGetValue((byte)((level1Id << 4) | level2Id), out level2Text))
      {
        if (level2Id != 0 && level2Id != 0xf)
        {
          return level1Text;
        }
        level2Text = "General";
      }

      return string.Format("{0}: {1}", level1Text, level2Text);
    }

    private static string GetOpenTvProgramCategoryDescription(ushort originalNetworkId, byte categoryId, byte subCategoryId)
    {
      bool isSkyNewZealand = false;
      IDictionary<byte, string> categoryNames = null;
      IDictionary<ushort, string> subCategoryNames = null;
      if (OPENTV_ORIGINAL_NETWORK_IDS_AU.Contains(originalNetworkId))
      {
        categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_AU;
        subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_AU;
      }
      else if (OPENTV_ORIGINAL_NETWORK_IDS_IT.Contains(originalNetworkId))
      {
        categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_IT;
        subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_IT;
      }
      else if (OPENTV_ORIGINAL_NETWORK_IDS_NZ.Contains(originalNetworkId))
      {
        // Based on DVB content types, with some overrides and extensions.
        isSkyNewZealand = true;
        categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_NZ;
        subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_NZ;
      }
      else if (OPENTV_ORIGINAL_NETWORK_IDS_UK.Contains(originalNetworkId))
      {
        categoryNames = MAPPING_OPENTV_PROGRAM_CATEGORIES_UK;
        subCategoryNames = MAPPING_OPENTV_PROGRAM_SUB_CATEGORIES_UK;
      }

      string categoryName;
      if (
        categoryNames == null ||
        (
          !categoryNames.TryGetValue(categoryId, out categoryName) &&
          (
            !isSkyNewZealand ||
            !MAPPING_CONTENT_TYPES_DVB.TryGetValue(categoryId, out categoryName)
          )
        )
      )
      {
        return string.Format("OpenTV Program Category {0}-{1}", categoryId, subCategoryId);
      }

      string subCategoryName;
      if (!subCategoryNames.TryGetValue((ushort)((categoryId << 8) | subCategoryId), out subCategoryName))
      {
        if (!isSkyNewZealand)
        {
          return categoryName;
        }

        if (!MAPPING_CONTENT_SUB_TYPES_DVB.TryGetValue((byte)((categoryId << 4) | subCategoryId), out subCategoryName))
        {
          if (subCategoryId != 0 && subCategoryId != 0xf)
          {
            return categoryName;
          }
          subCategoryName = "General";
        }
      }

      return string.Format("{0}: {1}", categoryName, subCategoryName);
    }

    private static string GetOpenTvParentalRatingDescription(ushort originalNetworkId, byte rating)
    {
      IDictionary<byte, string> ratingDescriptions = null;
      if (OPENTV_ORIGINAL_NETWORK_IDS_AU.Contains(originalNetworkId))
      {
        ratingDescriptions = MAPPING_OPENTV_PROGRAM_RATINGS_AU;
      }
      else if (OPENTV_ORIGINAL_NETWORK_IDS_IT.Contains(originalNetworkId))
      {
        ratingDescriptions = MAPPING_OPENTV_PROGRAM_RATINGS_IT;
      }
      else if (OPENTV_ORIGINAL_NETWORK_IDS_NZ.Contains(originalNetworkId))
      {
        ratingDescriptions = MAPPING_OPENTV_PROGRAM_RATINGS_NZ;
      }
      else if (OPENTV_ORIGINAL_NETWORK_IDS_UK.Contains(originalNetworkId))
      {
        ratingDescriptions = MAPPING_OPENTV_PROGRAM_RATINGS_UK;
      }

      string ratingDescription;
      if (ratingDescriptions != null && ratingDescriptions.TryGetValue(rating, out ratingDescription))
      {
        return ratingDescription;
      }
      return null;
    }

    private static string GetParentalRatingDescription(byte rating)
    {
      if (rating == 0 || rating > 0x0f)
      {
        // Undefined or broadcaster-specific.
        return null;
      }
      return string.Format("Min. age {0}", rating + 3);
    }

    private static ContentAdvisory GetContentAdvisories(ushort advisories)
    {
      ContentAdvisory advisoryFlags = ContentAdvisory.None;
      if ((advisories & 0x01) != 0)
      {
        advisoryFlags |= ContentAdvisory.SexualSituations;
      }
      if ((advisories & 0x02) != 0)
      {
        advisoryFlags |= ContentAdvisory.CourseOrCrudeLanguage;
      }
      if ((advisories & 0x04) != 0)
      {
        advisoryFlags |= ContentAdvisory.MildSensuality;
      }
      if ((advisories & 0x08) != 0)
      {
        advisoryFlags |= ContentAdvisory.FantasyViolence;
      }
      if ((advisories & 0x10) != 0)
      {
        advisoryFlags |= ContentAdvisory.Violence;
      }
      if ((advisories & 0x20) != 0)
      {
        advisoryFlags |= ContentAdvisory.MildPeril;
      }
      if ((advisories & 0x40) != 0)
      {
        advisoryFlags |= ContentAdvisory.Nudity;
      }
      if ((advisories & 0x8000) != 0)
      {
        advisoryFlags |= ContentAdvisory.SuggestiveDialogue;
      }
      return advisoryFlags;
    }

    private static string ParseDishDescription(string description, EpgProgram program)
    {
      // (<episode name>\r)? <category name>. (<CSV actors>.  \(<year>\))? <description>
      // <description> may end with zero or more of:
      // - New.
      // - Season Premiere.
      // - Series Premiere.
      // - Premiere.
      // - Season Finale.
      // - Series Finale.
      // - Finale.
      // - (HD)
      // - (DD)
      // - (SAP)
      // - (<PPV ID>) eg. (CE79D)
      // - (CC)
      // - (Stereo)
      string episodeName = null;
      int index = description.IndexOf((char)0xd);
      if (index > 0)
      {
        episodeName = description.Substring(0, index);
        //program.EpisodeNames.Add(LANG_CODE_ENG, description.Substring(0, index));
        description = description.Substring(index);
      }
      description = description.Trim();

      foreach (string contentType in MAPPING_CONTENT_TYPES_DISH.Values)
      {
        if (description.StartsWith(contentType + ". "))
        {
          description = description.Substring(contentType.Length + 2);
          break;
        }
      }

      if (episodeName != null)
      {
        description = string.Format("{0} {1}", episodeName, description);
      }

      Match m = Regex.Match(description, @"(  \(([12][0-9]{3})\) )");
      if (m.Success)
      {
        //program.ProductionYear = ushort.Parse(m.Groups[2].Captures[0].Value);
        index = description.IndexOf(m.Groups[1].Captures[0].Value);
        if (index > 0)
        {
          string people = description.Substring(0, index - 1);
          foreach (string person in people.Split(new string[] { ", " }, StringSplitOptions.None))
          {
            if (person.StartsWith("Voice of: "))
            {
              //program.Actors.Add(person.Substring("Voice of: ".Length));
              continue;
            }
            //program.Actors.Add(person);
          }
        }
        //description = description.Substring(index + 8);
      }

      index = description.IndexOf(" (HD)");
      if (index >= 0)
      {
        //program.IsHighDefinition = true;
        //description = description.Remove(index, " (HD)".Length);
      }

      index = description.IndexOf(" (CC)");
      if (index >= 0)
      {
        // assumption: language is English
        /*if (!program.SubtitlesLanguages.Contains(LANG_CODE_ENG))
        {
          program.SubtitlesLanguages.Add(LANG_CODE_ENG);
        }
        description = description.Remove(index, " (CC)".Length);*/
      }

      index = description.IndexOf(" New.");
      if (index >= 0)
      {
        //program.IsPreviouslyShown = false;
        //description = description.Remove(index, " New.".Length);
      }

      if (description.Contains(" Series Premiere."))
      {
        //program.SeasonNumber = 1;
        //program.EpisodeNumber = 1;
      }
      else if (description.Contains(" Season Premiere."))
      {
        //program.EpisodeNumber = 1;
      }

      return description;
    }

    #endregion

    #region mm1352000 ATSC/SCTE EPG

    #region constants

    // Note: the ATSC/SCTE RRT encoding differs from the Dish/BEV encoding.
    private static readonly IDictionary<byte, string> MAPPING_PROGRAM_CLASSIFICATIONS_MPAA_ATSC = new Dictionary<byte, string>(6)
    {
      { 1, "G" },       // general
      { 2, "PG" },      // parental guidance
      { 3, "PG-13" },   // parental guidance under 13
      { 4, "R" },       // restricted
      { 5, "NC-17" },   // nobody 17 and under
      { 6, "X" },       // explicit
      { 7, "NR" }       // not rated
    };

    private static readonly IDictionary<byte, string> MAPPING_ATSC_GENRES = new Dictionary<byte, string>(256)
    {
      { 0x20, "Education" },
      { 0x21, "Entertainment" },
      { 0x22, "Movie" },
      { 0x23, "News" },
      { 0x24, "Religious" },
      { 0x25, "Sports" },
      { 0x26, "Other" },
      { 0x27, "Action" },
      { 0x28, "Advertisement" },
      { 0x29, "Animated" },
      { 0x2a, "Anthology" },
      { 0x2b, "Automobile" },
      { 0x2c, "Awards" },
      { 0x2d, "Baseball" },
      { 0x2e, "Basketball" },
      { 0x2f, "Bulletin" },
      { 0x30, "Business" },
      { 0x31, "Classical" },
      { 0x32, "College" },
      { 0x33, "Combat" },
      { 0x34, "Comedy" },
      { 0x35, "Commentary" },
      { 0x36, "Concert" },
      { 0x37, "Consumer" },
      { 0x38, "Contemporary" },
      { 0x39, "Crime" },
      { 0x3a, "Dance" },
      { 0x3b, "Documentary" },
      { 0x3c, "Drama" },
      { 0x3d, "Elementary" },
      { 0x3e, "Erotica" },
      { 0x3f, "Exercise" },
      { 0x40, "Fantasy" },
      { 0x41, "Farm" },
      { 0x42, "Fashion" },
      { 0x43, "Fiction" },
      { 0x44, "Food" },
      { 0x45, "Football" },
      { 0x46, "Foreign" },
      { 0x47, "Fund Raiser" },
      { 0x48, "Game/Quiz" },
      { 0x49, "Garden" },
      { 0x4a, "Golf" },
      { 0x4b, "Government" },
      { 0x4c, "Health" },
      { 0x4d, "High School" },
      { 0x4e, "History" },
      { 0x4f, "Hobby" },
      { 0x50, "Hockey" },
      { 0x51, "Home" },
      { 0x52, "Horror" },
      { 0x53, "Information" },
      { 0x54, "Instruction" },
      { 0x55, "International" },
      { 0x56, "Interview" },
      { 0x57, "Language" },
      { 0x58, "Legal" },
      { 0x59, "Live" },
      { 0x5a, "Local" },
      { 0x5b, "Math" },
      { 0x5c, "Medical" },
      { 0x5d, "Meeting" },
      { 0x5e, "Military" },
      { 0x5f, "Miniseries" },
      { 0x60, "Music" },
      { 0x61, "Mystery" },
      { 0x62, "National" },
      { 0x63, "Nature" },
      { 0x64, "Police" },
      { 0x65, "Politics" },
      { 0x66, "Premier" },
      { 0x67, "Prerecorded" },
      { 0x68, "Product" },
      { 0x69, "Professional" },
      { 0x6a, "Public" },
      { 0x6b, "Racing" },
      { 0x6c, "Reading" },
      { 0x6d, "Repair" },
      { 0x6e, "Repeat" },
      { 0x6f, "Review" },
      { 0x70, "Romance" },
      { 0x71, "Science" },
      { 0x72, "Series" },
      { 0x73, "Service" },
      { 0x74, "Shopping" },
      { 0x75, "Soap Opera" },
      { 0x76, "Special" },
      { 0x77, "Suspense" },
      { 0x78, "Talk" },
      { 0x79, "Technical" },
      { 0x7a, "Tennis" },
      { 0x7b, "Travel" },
      { 0x7c, "Variety" },
      { 0x7d, "Video" },
      { 0x7e, "Weather" },
      { 0x7f, "Western" },
      { 0x80, "Art" },
      { 0x81, "Auto Racing" },
      { 0x82, "Aviation" },
      { 0x83, "Biography" },
      { 0x84, "Boating" },
      { 0x85, "Bowling" },
      { 0x86, "Boxing" },
      { 0x87, "Cartoon" },
      { 0x88, "Children" },
      { 0x89, "Classic Film" },
      { 0x8a, "Community" },
      { 0x8b, "Computers" },
      { 0x8c, "Country Music" },
      { 0x8d, "Court" },
      { 0x8e, "Extreme Sports" },
      { 0x8f, "Family" },
      { 0x90, "Financial" },
      { 0x91, "Gymnastics" },
      { 0x92, "Headlines" },
      { 0x93, "Horse Racing" },
      { 0x94, "Hunting/Fishing/Outdoors" },
      { 0x95, "Independent" },
      { 0x96, "Jazz" },
      { 0x97, "Magazine" },
      { 0x98, "Motorcycle Racing" },
      { 0x99, "Music/Film/Books" },
      { 0x9a, "News-International" },
      { 0x9b, "News-Local" },
      { 0x9c, "News-National" },
      { 0x9d, "News-Regional" },
      { 0x9e, "Olympics" },
      { 0x9f, "Original" },
      { 0xa0, "Performing Arts" },
      { 0xa1, "Pets/Animals" },
      { 0xa2, "Pop" },
      { 0xa3, "Rock & Roll" },
      { 0xa4, "Sci-Fi" },
      { 0xa5, "Self Improvement" },
      { 0xa6, "Sitcom" },
      { 0xa7, "Skating" },
      { 0xa8, "Skiing" },
      { 0xa9, "Soccer" },
      { 0xaa, "Track/Field" },
      { 0xab, "True" },
      { 0xac, "Volleyball" },
      { 0xad, "Wrestling" }
    };

    #endregion

    #region variables

    #region ATSC

    /// <summary>
    /// Indicator: should the grabber grab ATSC EPG data?
    /// </summary>
    private bool _grabAtsc = false;

    /// <summary>
    /// The ATSC EPG grabber.
    /// </summary>
    private IGrabberEpgAtsc _grabberAtsc = null;

    /// <summary>
    /// Indicator: has the grabber seen ATSC EPG data?
    /// </summary>
    private bool _isSeenAtsc = false;

    /// <summary>
    /// Indicator: has the grabber received all ATSC EPG data?
    /// </summary>
    private bool _isCompleteAtsc = false;

    #endregion

    #region SCTE

    /// <summary>
    /// Indicator: should the grabber grab SCTE EPG data?
    /// </summary>
    private bool _grabScte = false;

    /// <summary>
    /// The SCTE EPG grabber.
    /// </summary>
    private IGrabberEpgScte _grabberScte = null;

    /// <summary>
    /// Indicator: has the grabber seen SCTE EPG data?
    /// </summary>
    private bool _isSeenScte = false;

    /// <summary>
    /// Indicator: has the grabber received all SCTE EPG data?
    /// </summary>
    private bool _isCompleteScte = false;

    #endregion

    #endregion

    private IList<Tuple<IChannel, IList<EpgProgram>>> CollectData(IGrabberEpgAtsc grabberEpg, IGrabberSiAtsc grabberSi)
    {
      uint eventCount = grabberEpg.GetEventCount();
      Log.Log.Debug("EPG ATSC: initial event count = {0}", eventCount);
      IDictionary<ushort, List<EpgProgram>> tempData = new Dictionary<ushort, List<EpgProgram>>(100);

      const byte ARRAY_SIZE_AUDIO_LANGUAGES = 20;
      const byte ARRAY_SIZE_CAPTIONS_LANGUAGES = 20;
      const byte ARRAY_SIZE_GENRE_IDS = 20;
      const ushort BUFFER_SIZE_TITLE = 300;
      IntPtr bufferTitle = Marshal.AllocCoTaskMem(BUFFER_SIZE_TITLE);
      const ushort BUFFER_SIZE_DESCRIPTION = 1000;
      IntPtr bufferDescription = Marshal.AllocCoTaskMem(BUFFER_SIZE_DESCRIPTION);
      try
      {
        ushort sourceId;
        ushort eventId;
        ulong startDateTimeEpoch;
        uint duration;
        byte textCount;
        Iso639Code[] audioLanguages = new Iso639Code[ARRAY_SIZE_AUDIO_LANGUAGES];
        Iso639Code[] captionsLanguages = new Iso639Code[ARRAY_SIZE_CAPTIONS_LANGUAGES];
        byte[] genreIds = new byte[ARRAY_SIZE_GENRE_IDS];
        byte vchipRating;
        byte mpaaClassification;
        ushort advisories;
        Iso639Code language;
        for (uint i = 0; i < eventCount; i++)
        {
          byte countAudioLanguages = ARRAY_SIZE_AUDIO_LANGUAGES;
          byte countCaptionsLanguages = ARRAY_SIZE_CAPTIONS_LANGUAGES;
          byte countGenreIds = ARRAY_SIZE_GENRE_IDS;
          bool result = grabberEpg.GetEvent(i,
                                          out sourceId,
                                          out eventId,
                                          out startDateTimeEpoch,
                                          out duration,
                                          out textCount,
                                          audioLanguages,
                                          ref countAudioLanguages,
                                          captionsLanguages,
                                          ref countCaptionsLanguages,
                                          genreIds,
                                          ref countGenreIds,
                                          out vchipRating,
                                          out mpaaClassification,
                                          out advisories);
          if (!result)
          {
            Log.Log.Error("EPG ATSC: failed to get event, event index = {0}, event count = {1}", i, eventCount);
            continue;
          }

          DateTime programStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
          programStartTime = programStartTime.AddSeconds(startDateTimeEpoch);
          programStartTime = programStartTime.ToLocalTime();
          EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddSeconds(duration));

          bool isPlaceholderOrDummyEvent = false;
          for (byte j = 0; j < textCount; j++)
          {
            ushort bufferSizeTitle = BUFFER_SIZE_TITLE;
            ushort bufferSizeDescription = BUFFER_SIZE_DESCRIPTION;
            result = grabberEpg.GetEventTextByIndex(i, j,
                                                  out language,
                                                  bufferTitle,
                                                  ref bufferSizeTitle,
                                                  bufferDescription,
                                                  ref bufferSizeDescription);
            if (!result)
            {
              Log.Log.Error("EPG ATSC: failed to get event text, event index = {0}, event count = {1}, text index = {2}, text count = {3}", i, eventCount, j, textCount);
              continue;
            }

            string title = TidyString(DvbTextConverter.Convert(bufferTitle, bufferSizeTitle));
            if (string.IsNullOrEmpty(title))
            {
              isPlaceholderOrDummyEvent = true;
              continue;
            }
            EpgLanguageText programText = new EpgLanguageText(language.Code, title, string.Empty, string.Empty, 0, string.Empty, -1);
            //program.Titles.Add(language.Code, title);

            string description = TidyString(DvbTextConverter.Convert(bufferDescription, bufferSizeDescription));
            if (!string.IsNullOrEmpty(description))
            {
              programText.Description = description;
              //program.Descriptions.Add(language.Code, description);
            }
            program.Text.Add(programText);
          }

          if (isPlaceholderOrDummyEvent)
          {
            continue;
          }

          for (byte x = 0; x < countAudioLanguages; x++)
          {
            //program.AudioLanguages.Add(audioLanguages[x].Code);
          }
          for (byte x = 0; x < countCaptionsLanguages; x++)
          {
            //program.SubtitlesLanguages.Add(captionsLanguages[x].Code);
          }
          for (byte x = 0; x < countGenreIds; x++)
          {
            string genreDescription;
            if (MAPPING_ATSC_GENRES.TryGetValue(genreIds[x], out genreDescription))
            {
              foreach (var text in program.Text)
              {
                if (!string.IsNullOrEmpty(text.Genre))
                {
                  text.Genre += ", ";
                }
                text.Genre += genreDescription;
              }
              //program.Categories.Add(genreDescription);
            }
          }
          string mpaaClassificationDescription;
          if (MAPPING_PROGRAM_CLASSIFICATIONS_MPAA_ATSC.TryGetValue(mpaaClassification, out mpaaClassificationDescription))
          {
            foreach (var text in program.Text)
            {
              if (!string.IsNullOrEmpty(text.Classification))
              {
                text.Classification += ", ";
              }
              text.Classification += mpaaClassificationDescription;
            }
            //program.Classifications.Add("MPAA", mpaaClassificationDescription);
          }
          //program.Advisories = GetContentAdvisories(advisories);
          string vchipRatingDescription;
          if (MAPPING_PROGRAM_RATINGS_VCHIP.TryGetValue(vchipRating, out vchipRatingDescription))
          {
            foreach (var text in program.Text)
            {
              if (!string.IsNullOrEmpty(text.Classification))
              {
                text.Classification += ", ";
              }
              text.Classification += vchipRatingDescription;
            }
            //program.Classifications.Add("V-Chip", vchipRatingDescription);
          }

          List<EpgProgram> programs;
          if (!tempData.TryGetValue(sourceId, out programs))
          {
            programs = new List<EpgProgram>(100);
            tempData.Add(sourceId, programs);
          }
          programs.Add(program);
        }

        IDictionary<ushort, uint> atscScteIds = ReadAtscScteChannelIds(grabberSi);
        IList<Tuple<IChannel, IList<EpgProgram>>> data = new List<Tuple<IChannel, IList<EpgProgram>>>(tempData.Count);
        int validEventCount = 0;
        foreach (var channelData in tempData)
        {
          uint atscScteId;
          if (!atscScteIds.TryGetValue(channelData.Key, out atscScteId))
          {
            Log.Log.Error("EPG ATSC: failed to determine ATSC/SCTE ID for source ID {0}", channelData.Key);
            continue;
          }

          DVBBaseChannel atscScteChannel = CreateChannel(channelData.Key, (int)(atscScteId >> 16), (int)(atscScteId & 0xffff), "");
          EpgChannel epgChannel = new EpgChannel();
          epgChannel.Channel = atscScteChannel;
          if (FilterOutEPGChannel(epgChannel))
          {
            Log.Log.Debug("EPG ATSC: discarding ATSC/SCTE events for channel from different transponder, event count = {0}, source ID = {1}, transport stream ID = {2}, program number = {3}", channelData.Value.Count, atscScteChannel.NetworkId, atscScteChannel.TransportId, atscScteChannel.ServiceId);
            continue;
          }
          /*IChannel atscScteChannel = _currentTuningDetail.Clone() as IChannel;
          ChannelAtsc atscChannel = atscScteChannel as ChannelAtsc;
          if (atscChannel != null)
          {
            atscChannel.SourceId = channel.Key;
          }
          else
          {
            ChannelScte scteChannel = atscScteChannel as ChannelScte;
            if (scteChannel != null)
            {
              scteChannel.SourceId = channel.Key;
            }
            else
            {
              Log.Log.Error("EPG ATSC: the tuned channel is not an ATSC or SCTE channel");
              continue;
            }
          }*/
          channelData.Value.Sort();
          data.Add(new Tuple<IChannel, IList<EpgProgram>>(atscScteChannel, channelData.Value));
          validEventCount += channelData.Value.Count;
        }

        Log.Log.Debug("EPG ATSC: channel count = {0}, event count = {1}", tempData.Count, validEventCount);
        return data;
      }
      finally
      {
        if (bufferTitle != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferTitle);
        }
        if (bufferDescription != IntPtr.Zero)
        {
          Marshal.FreeCoTaskMem(bufferDescription);
        }
      }
    }

    private IDictionary<ushort, uint> ReadAtscScteChannelIds(IGrabberSiAtsc grabber)
    {
      Dictionary<ushort, uint> ids = new Dictionary<ushort, uint>(500);
      if (grabber == null)
      {
        return ids;
      }
      ushort channelCount = grabber.GetLvctChannelCount();

      byte tableId;
      ushort sectionTransportStreamId;
      ushort mapId;
      byte longNameCount;
      ushort majorChannelNumber;
      ushort minorChannelNumber;
      ModulationMode modulationMode;
      uint carrierFrequency;
      ushort transportStreamId;
      ushort programNumber;
      EtmLocation etmLocation;
      bool accessControlled;
      bool hidden;
      PathSelect pathSelect;
      bool outOfBand;
      bool hideGuide;
      ServiceType serviceType;
      ushort sourceId;
      byte streamCountVideo;
      byte streamCountAudio;
      bool isThreeDimensional;
      ushort shortNameBufferSize = 0;
      byte audioLanguageCount = 0;
      byte captionsLanguageCount = 0;
      for (ushort i = 0; i < channelCount; i++)
      {
        if (grabber.GetLvctChannel(i,
                                    out tableId,
                                    out sectionTransportStreamId,
                                    out mapId,
                                    IntPtr.Zero,
                                    ref shortNameBufferSize,
                                    out longNameCount,
                                    out majorChannelNumber,
                                    out minorChannelNumber,
                                    out modulationMode,
                                    out carrierFrequency,
                                    out transportStreamId,
                                    out programNumber,
                                    out etmLocation,
                                    out accessControlled,
                                    out hidden,
                                    out pathSelect,
                                    out outOfBand,
                                    out hideGuide,
                                    out serviceType,
                                    out sourceId,
                                    out streamCountVideo,
                                    out streamCountAudio,
                                    out isThreeDimensional,
                                    null,
                                    ref audioLanguageCount,
                                    null,
                                    ref captionsLanguageCount))
        {
          if (sourceId != 0 && sourceId != 0xffff)
          {
            uint key;
            if (ids.TryGetValue(sourceId, out key))
            {
              ushort tsid1 = (ushort)(key >> 16);
              ushort pn1 = (ushort)(key & 0xffff);
              Log.Log.Info("EPG ATSC: duplicate source definition, source ID = {0}, source 1 [ TSID = {1}, program number = {2} ], source 2 [ TSID = {3}, program number = {4} ]", sourceId, tsid1, pn1, transportStreamId, programNumber);
            }
            else
            {
              ids.Add(sourceId, ((uint)transportStreamId << 16) | programNumber);
            }
          }
        }
      }

      channelCount = grabber.GetSvctVirtualChannelCount();
      if (channelCount == 0)
      {
        return ids;
      }

      TransmissionMedium transmissionMedium;
      ushort vctId;
      Iso639Code mapNameLanguage;
      bool splice;
      uint activationTime;
      bool hdtvChannel;
      bool preferredSource;
      bool applicationVirtualChannel;
      Iso639Code sourceNameLanguage;
      BitstreamSelect bitstreamSelect;
      Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum.ChannelType channelType;
      ushort nvodChannelBase;
      TransportType transportType;
      bool wideBandwidthVideo;
      WaveformStandard waveformStandard;
      VideoStandard videoStandard;
      bool wideBandwidthAudio;
      bool compandedAudio;
      MatrixMode matrixMode;
      ushort subcarrier2Offset;
      ushort subcarrier1Offset;
      bool suppressVideo;
      AudioSelection audioSelection;
      byte satelliteId;
      Iso639Code satelliteNameLanguage;
      ushort satelliteReferenceNameBufferSize = 0;
      ushort satelliteFullNameBufferSize = 0;
      Hemisphere hemisphere;
      ushort orbitalPosition;
      bool youAreHere;
      FrequencyBand frequencyBand;
      bool outOfService;
      PolarisationType polarisationType;
      byte transponderNumber;
      Iso639Code transponderNameLanguage;
      ushort transponderNameBufferSize = 0;
      bool rootTransponder;
      ToneSelect toneSelect;
      Mediaportal.TV.Server.TVLibrary.Implementations.Atsc.Enum.Polarisation polarisation;
      uint frequency;
      uint symbolRate;
      TransmissionSystem transmissionSystem;
      InnerCodingMode innerCodingMode;
      bool splitBitstreamMode;
      ModulationFormat modulationFormat;
      ushort mapNameBufferSize = 0;
      ushort sourceNameBufferSize = 0;
      for (ushort i = 0; i < channelCount; i++)
      {
        if (grabber.GetSvctVirtualChannel(i,
                                            out transmissionMedium,
                                            out vctId,
                                            out mapNameLanguage,
                                            IntPtr.Zero,
                                            ref mapNameBufferSize,
                                            out splice,
                                            out activationTime,
                                            out hdtvChannel,
                                            out preferredSource,
                                            out applicationVirtualChannel,
                                            out majorChannelNumber,
                                            out minorChannelNumber,
                                            out sourceId,
                                            out sourceNameLanguage,
                                            IntPtr.Zero,
                                            ref sourceNameBufferSize,
                                            out accessControlled,
                                            out hideGuide,
                                            out serviceType,
                                            out outOfBand,
                                            out bitstreamSelect,
                                            out pathSelect,
                                            out channelType,
                                            out nvodChannelBase,
                                            out transportType,
                                            out wideBandwidthVideo,
                                            out waveformStandard,
                                            out videoStandard,
                                            out wideBandwidthAudio,
                                            out compandedAudio,
                                            out matrixMode,
                                            out subcarrier2Offset,
                                            out subcarrier1Offset,
                                            out suppressVideo,
                                            out audioSelection,
                                            out programNumber,
                                            out transportStreamId,
                                            out satelliteId,
                                            out satelliteNameLanguage,
                                            IntPtr.Zero,
                                            ref satelliteReferenceNameBufferSize,
                                            IntPtr.Zero,
                                            ref satelliteFullNameBufferSize,
                                            out hemisphere,
                                            out orbitalPosition,
                                            out youAreHere,
                                            out frequencyBand,
                                            out outOfService,
                                            out polarisationType,
                                            out transponderNumber,
                                            out transponderNameLanguage,
                                            IntPtr.Zero,
                                            ref transponderNameBufferSize,
                                            out rootTransponder,
                                            out toneSelect,
                                            out polarisation,
                                            out frequency,
                                            out symbolRate,
                                            out transmissionSystem,
                                            out innerCodingMode,
                                            out splitBitstreamMode,
                                            out modulationFormat))
        {
          if (!applicationVirtualChannel && sourceId != 0 && sourceId != 0xffff)
          {
            uint key;
            if (ids.TryGetValue(sourceId, out key))
            {
              ushort tsid1 = (ushort)(key >> 16);
              ushort pn1 = (ushort)(key & 0xffff);
              Log.Log.Info("EPG ATSC: duplicate source definition, source ID = {0}, source 1 [ TSID = {1}, program number = {2} ], source 2 [ TSID = {3}, program number = {4} ]", sourceId, tsid1, pn1, transportStreamId, programNumber);
            }
            else
            {
              ids.Add(sourceId, ((uint)transportStreamId << 16) | programNumber);
            }
          }
        }
      }
      return ids;
    }

    private static string GetAtscGenreDescription(byte genreId)
    {
      string description;
      if (!MAPPING_ATSC_GENRES.TryGetValue(genreId, out description))
      {
        description = string.Format("User Defined {0}", genreId);
      }
      return description;
    }

    #endregion

    #endregion
  }
}