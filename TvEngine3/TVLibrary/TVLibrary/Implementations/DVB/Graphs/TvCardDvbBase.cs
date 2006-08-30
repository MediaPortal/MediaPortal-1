/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
//#define FORM
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using DirectShowLib;
using DirectShowLib.BDA;
using TvLibrary.Implementations;
using DirectShowLib.SBE;
using TvLibrary.Interfaces;
using TvLibrary.Interfaces.Analyzer;
using TvLibrary.Interfaces.TsFileSink;
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Epg;
using TvLibrary.Teletext;
using TvLibrary.Log;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// base class for DVB devices
  /// </summary>
  public class TvCardDvbBase : IDisposable, ITeletextCallBack, IPMTCallback
  {
    #region structs
    #endregion

    #region enums
    protected enum GraphState
    {
      Idle,
      Created,
      TimeShifting,
      Recording
    }
    #endregion

    #region imports
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxerPin(IPin pin, int pid, int elementaryStream, bool unmapOtherPins);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxerPids(IPin pin, int[] pids, int pidCount, int elementaryStream, bool unmapOtherPins);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int DumpMpeg2DemuxerMappings(IBaseFilter filter);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int GetPidMapping(IPin pin, IntPtr pids, IntPtr elementary_stream, ref Int32 count);
    #endregion

    #region delegates
    public delegate void EpgProcessedHandler(object sender, List<EpgChannel> epg);
    public event EpgProcessedHandler OnEpgReceived;
    #endregion

    #region constants

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    protected class MpTsAnalyzer { }

    [ComImport, Guid("BC650178-0DE4-47DF-AF50-BBD9C7AEF5A9")]
    protected class CyberLinkMuxer { }

    [ComImport, Guid("7F2BBEAF-E11C-4D39-90E8-938FB5A86045")]
    protected class PowerDirectorMuxer { }

    [ComImport, Guid("3E8868CB-5FE8-402C-AA90-CB1AC6AE3240")]
    protected class CyberLinkDumpFilter { };

    #endregion

    #region imports
    [DllImport("advapi32", CharSet = CharSet.Auto)]
    protected static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);
    #endregion


    #region variables
    protected ConditionalAccess _conditionalAccess = null;
    protected IFilterGraph2 _graphBuilder = null;
    protected ICaptureGraphBuilder2 _capBuilder = null;
    protected DsROTEntry _rotEntry = null;

    protected IBaseFilter _filterNetworkProvider = null;
    protected IBaseFilter _filterMpeg2DemuxTif = null;
#if MULTI_DEMUX
    protected IBaseFilter _filterMpeg2DemuxAnalyzer = null;
    protected IBaseFilter _filterMpeg2DemuxTs = null;
#endif
    protected IBaseFilter _infTeeMain = null;
    protected IBaseFilter _filterTuner = null;
    protected IBaseFilter _filterCapture = null;
    protected IBaseFilter _filterTIF = null;
    protected IBaseFilter _filterSectionsAndTables = null;

    protected DsDevice _tunerDevice = null;
    protected DsDevice _captureDevice = null;
    protected DVBTeletext _teletextDecoder;


    protected List<IBDA_SignalStatistics> _tunerStatistics = new List<IBDA_SignalStatistics>();
    protected bool _signalPresent;
    protected bool _tunerLocked;
    protected int _signalQuality;
    protected int _signalLevel;
    protected string _name;
    protected string _devicePath;
    protected string _recordingFileName;
    protected IChannel _currentChannel;
    protected IBaseFilter _filterTsAnalyzer;

    protected int _pmtVersion;
    protected ChannelInfo _channelInfo;
    protected DateTime _lastSignalUpdate;
    protected bool _graphRunning = false;
    protected int _managedThreadId = -1;
    protected bool _isATSC = false;
    protected ITsEpgScanner _interfaceEpgGrabber;
    protected ITsChannelScan _interfaceChannelScan;
    protected ITsPmtGrabber _interfacePmtGrabber;

    protected bool _epgGrabbing = false;
    protected bool _isScanning = false;
    string _timeshiftFileName = "";
    protected GraphState _graphState = GraphState.Idle;
    protected bool _startTimeShifting = false;
    DVBAudioStream _currentAudioStream;



    #region teletext
    protected bool _grabTeletext = false;
    protected bool _hasTeletext = false;
    TSHelperTools.TSHeader _packetHeader;
    TSHelperTools _tsHelper;
    protected DateTime _dateTimeShiftStarted = DateTime.MinValue;
    protected DateTime _dateRecordingStarted = DateTime.MinValue;
    #endregion

#if FORM
    protected bool _newPMT = false;
    System.Windows.Forms.Timer _pmtTimer = new System.Windows.Forms.Timer();
#else
    System.Timers.Timer _pmtTimer = new System.Timers.Timer();
#endif
    #endregion

    #region graph building

    /// <summary>
    /// ctor
    /// </summary>
    public TvCardDvbBase()
    {
      _lastSignalUpdate = DateTime.MinValue;
      _pmtTimer.Enabled = false;
      _pmtTimer.Interval = 100;
#if FORM
      _pmtTimer.Tick += new EventHandler(_pmtTimer_ElapsedForm);
#else
      _pmtTimer.Elapsed += new System.Timers.ElapsedEventHandler(_pmtTimer_Elapsed);
#endif
      _teletextDecoder = new DVBTeletext();
      _packetHeader = new TSHelperTools.TSHeader();
      _tsHelper = new TSHelperTools();
      _channelInfo = new ChannelInfo();


    }
    /// <summary>
    /// Checks the thread id.
    /// </summary>
    /// <returns></returns>
    protected bool CheckThreadId()
    {
      return true;
      if (_managedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
      {

        Log.Log.WriteFile("dvb:Invalid thread id {0}!={1}", _managedThreadId, System.Threading.Thread.CurrentThread.ManagedThreadId);
        return true;
        //return false;
      }
      return true;
    }
    /// <summary>
    /// submits a tune request to the card. 
    /// throws an TvException if card cannot tune to the channel requested
    /// </summary>
    /// <param name="tuneRequest">tune requests</param>
    protected void SubmitTuneRequest(ITuneRequest tuneRequest)
    {
      if (!CheckThreadId()) return;
#if FORM      
      _newPMT=false;
#endif
      Log.Log.WriteFile("dvb:SubmitTuneRequest");
      _startTimeShifting = false;
      _channelInfo = new ChannelInfo();
      _pmtTimer.Enabled = false;
      _hasTeletext = false;
      _currentAudioStream = null;

      //Log.Log.WriteFile("dvb:SubmitTuneRequest");
      if (_interfaceEpgGrabber != null)
      {
        _interfaceEpgGrabber.Reset();
      }

      int hr = 0;
      hr = (_filterNetworkProvider as ITuner).put_TuneRequest(tuneRequest);
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:SubmitTuneRequest  returns:0x{0:X}", hr);
        throw new TvException("Unable to tune to channel");
      }
      //      Log.Log.WriteFile("dvb:SubmitTuneRequest ok");

      _pmtTimer.Enabled = true;
      _lastSignalUpdate = DateTime.MinValue;
      SendHwPids(new ArrayList());

      _pmtVersion = -1;
    }


    /// <summary>
    /// sets the filename used for timeshifting
    /// </summary>
    /// <param name="fileName">timeshifting filename</param>
    protected void SetTimeShiftFileName(string fileName)
    {
      if (!CheckThreadId()) return;
      _timeshiftFileName = fileName;
      Log.Log.WriteFile("dvb:SetTimeShiftFileName:{0}", fileName);
      int hr;
      if (_filterTsAnalyzer != null)
      {
        ITsTimeShift record = _filterTsAnalyzer as ITsTimeShift;
        record.SetTimeShiftingFileName(fileName);
        if (_channelInfo.pids.Count == 0)
        {
          Log.Log.WriteFile("dvb:SetTimeShiftFileName no pmt received yet");
          _startTimeShifting = true;
        }
        else
        {
          Log.Log.WriteFile("dvb:SetTimeShiftFileName fill in pids");
          _startTimeShifting = false;
          DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
          record.SetPcrPid((short)dvbChannel.PcrPid);
          foreach (PidInfo info in _channelInfo.pids)
          {
            if (info.isAC3Audio || info.isAudio || info.isVideo)
            {
              record.AddPesStream((short)info.pid, (info.isAC3Audio || info.isAudio), info.isVideo);
            }
          }
          record.Start();
        }
      }
    }

    /// <summary>
    /// this method gets the signal statistics interfaces from the bda tuner device
    /// and stores them in _tunerStatistics
    /// </summary>
    protected void GetTunerSignalStatistics()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb: GetTunerSignalStatistics()");
      //no tuner filter? then return;
      _tunerStatistics = new List<IBDA_SignalStatistics>();
      if (_filterTuner == null)
      {
        Log.Log.WriteFile("dvb: could not get IBDA_Topology since no tuner device");
        return;
      }
      //get the IBDA_Topology from the tuner device
      //Log.Log.WriteFile("dvb: get IBDA_Topology");
      IBDA_Topology topology = _filterTuner as IBDA_Topology;
      if (topology == null)
      {
        Log.Log.WriteFile("dvb: could not get IBDA_Topology from tuner");
        return;
      }

      //get the NodeTypes from the topology
      //Log.Log.WriteFile("dvb: GetNodeTypes");
      int nodeTypeCount = 0;
      int[] nodeTypes = new int[33];
      Guid[] guidInterfaces = new Guid[33];

      int hr = topology.GetNodeTypes(out nodeTypeCount, 32, nodeTypes);
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb: FAILED could not get node types from tuner:0x{0:X}", hr);
        return;
      }
      if (nodeTypeCount == 0)
      {
        Log.Log.WriteFile("dvb: FAILED could not get any node types");
      }
      Guid GuidIBDA_SignalStatistic = new Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338");
      //for each node type
      //Log.Log.WriteFile("dvb: got {0} node types", nodeTypeCount);
      for (int i = 0; i < nodeTypeCount; ++i)
      {
        object objectNode;
        int numberOfInterfaces = 32;
        hr = topology.GetNodeInterfaces(nodeTypes[i], out numberOfInterfaces, 32, guidInterfaces);
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb: FAILED could not GetNodeInterfaces for node:{0} 0x:{1:X}", i, hr);
        }

        hr = topology.GetControlNode(0, 1, nodeTypes[i], out objectNode);
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb: FAILED could not GetControlNode for node:{0} 0x:{1:X}", i, hr);
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

      }//for (int i=0; i < nodeTypeCount;++i)
      //hr=Release.ComObject(topology);
      return;
    }//IBDA_SignalStatistics GetTunerSignalStatistics()

    /// <summary>
    /// Methods which starts the graph
    /// </summary>
    protected void RunGraph()
    {
      if (!CheckThreadId()) return;
      FilterState state;
      (_graphBuilder as IMediaControl).GetState(10, out state);
      if (state == FilterState.Running)
      {
        Log.Log.WriteFile("dvb:RunGraph: already running");
        _pmtVersion = -1;
        DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
        if (channel != null)
        {
          SetAnalyzerMapping(channel.PmtPid);
        }
        return;
      }
      Log.Log.WriteFile("dvb:RunGraph");
      _teletextDecoder.ClearBuffer();
      _pmtVersion = -1;

      int hr = 0;


      hr = (_graphBuilder as IMediaControl).Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("dvb:RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }

      _epgGrabbing = false;
      _dateTimeShiftStarted = DateTime.Now;
      DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
      if (dvbChannel != null)
      {
        SetAnalyzerMapping(dvbChannel.PmtPid);
      }
      _pmtTimer.Enabled = true;
      _graphRunning = true;

    }

    /// <summary>
    /// Methods which stops the graph
    /// </summary>
    protected void StopGraph()
    {
      if (!CheckThreadId()) return;
      if (_epgGrabbing)
      {
        _epgGrabbing = false;
        if (OnEpgReceived != null)
        {
          Log.Log.WriteFile("dvb:cancel epg->stop graph");
          OnEpgReceived(this, null);
        }
      }
      _graphRunning = false;
      _epgGrabbing = false;
      _isScanning = false;
      _dateTimeShiftStarted = DateTime.MinValue;
      _dateRecordingStarted = DateTime.MinValue;
      if (_graphBuilder == null) return;
      FilterState state;
      (_graphBuilder as IMediaControl).GetState(10, out state);
      _pmtTimer.Enabled = false;
      _startTimeShifting = false;
      _pmtVersion = -1;
      _recordingFileName = "";
      _channelInfo = new ChannelInfo();
      if (_filterTsAnalyzer != null)
      {
        ITsRecorder recorder = _filterTsAnalyzer as ITsRecorder;
        recorder.StopRecord();
        ITsTimeShift timeshift = _filterTsAnalyzer as ITsTimeShift;
        timeshift.Stop();
      }
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
      }
      if (state == FilterState.Stopped) return;
      Log.Log.WriteFile("dvb:StopGraph");
      int hr = 0;
      //hr = (_graphBuilder as IMediaControl).StopWhenReady();
      hr = (_graphBuilder as IMediaControl).Stop();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("dvb:RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to stop graph");
      }
    }

    /// <summary>
    /// This method adds the bda network provider filter to the graph
    /// </summary>
    protected void AddNetworkProviderFilter(Guid networkProviderClsId)
    {
      _isATSC = false;
      _managedThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
      Log.Log.WriteFile("dvb:AddNetworkProviderFilter");

      Guid genProviderClsId = new Guid("{B2F3A67C-29DA-4C78-8831-091ED509A475}");


      // First test if the Generic Network Provider is available (only on MCE 2005 + Update Rollup 2)
      if (FilterGraphTools.IsThisComObjectInstalled(genProviderClsId))
      {
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, genProviderClsId, "Generic Network Provider");

        Log.Log.WriteFile("dvb:Add Generic Network Provider");
        return;
      }


      // Get the network type of the requested Tuning Space
      if (networkProviderClsId == typeof(DVBTNetworkProvider).GUID)
      {
        Log.Log.WriteFile("dvb:Add DVBTNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "DVBT Network Provider");
      }
      else if (networkProviderClsId == typeof(DVBSNetworkProvider).GUID)
      {
        Log.Log.WriteFile("dvb:Add DVBSNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "DVBS Network Provider");
      }
      else if (networkProviderClsId == typeof(ATSCNetworkProvider).GUID)
      {
        _isATSC = true;
        Log.Log.WriteFile("dvb:Add ATSCNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "ATSC Network Provider");
      }
      else if (networkProviderClsId == typeof(DVBCNetworkProvider).GUID)
      {
        Log.Log.WriteFile("dvb:Add DVBCNetworkProvider");
        _filterNetworkProvider = FilterGraphTools.AddFilterFromClsid(_graphBuilder, networkProviderClsId, "DVBC Network Provider");
      }
      else
      {
        Log.Log.WriteFile("dvb:This application doesn't support this Tuning Space");
        // Tuning Space can also describe Analog TV but this application don't support them
        throw new TvException("This application doesn't support this Tuning Space");
      }
    }


    /// <summary>
    /// Finds the correct bda tuner/capture filters and adds them to the graph
    /// </summary>
    /// <param name="device"></param>
    protected void AddAndConnectBDABoardFilters(DsDevice device)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:AddAndConnectBDABoardFilters");
      int hr = 0;
      DsDevice[] devices;


      Log.Log.WriteFile("dvb: find bda tuner");
      // Enumerate BDA Source filters category and found one that can connect to the network provider
      devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        Log.Log.WriteFile("dvb:  Got {0}", devices[i].Name);
        if (device.DevicePath != devices[i].DevicePath) continue;
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
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


        hr = _capBuilder.RenderStream(null, null, _filterNetworkProvider, null, tmp);
        if (hr == 0)
        {
          // Got it !
          _filterTuner = tmp;
          _tunerDevice = devices[i];
          DevicesInUse.Instance.Add(devices[i]);

          Log.Log.WriteFile("dvb:  OK");
          break;
        }
        else
        {
          // Try another...
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("bda tuner", tmp);
        }
      }
      // Assume we found a tuner filter...

      if (_filterTuner == null)
      {
        Log.Log.WriteFile("dvb:No TvTuner installed");
        throw new TvException("No TvTuner installed");
      }
      Log.Log.WriteFile("dvb:find bda receiver");
      // Then enumerate BDA Receiver Components category to found a filter connecting 
      // to the tuner and the MPEG2 Demux
      devices = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);

      string guidBdaMPEFilter = @"\{8e60217d-a2ee-47f8-b0c5-0f44c55f66dc}";
      string guidBdaSlipDeframerFilter = @"\{03884cb6-e89a-4deb-b69e-8dc621686e6a}";
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].DevicePath.ToLower().IndexOf(guidBdaMPEFilter) >= 0) continue;
        if (devices[i].DevicePath.ToLower().IndexOf(guidBdaSlipDeframerFilter) >= 0) continue;

        IBaseFilter tmp;
        Log.Log.WriteFile("dvb:  got {0}", devices[i].Name);
        if (DevicesInUse.Instance.IsUsed(devices[i])) continue;
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
            Release.ComObject("bda receiver", tmp);
          }
          continue;
        }
        hr = _capBuilder.RenderStream(null, null, _filterTuner, null, tmp);
        if (hr == 0)
        {
          // Got it !
          // Connect it to the MPEG-2 Demux
          hr = _capBuilder.RenderStream(null, null, tmp, null, _infTeeMain);
          //hr = _capBuilder.RenderStream(null, null, tmp, null, _filterMpeg2DemuxTif);
          if (hr != 0)
          {
            Log.Log.WriteFile("dvb:  Render->main inftee demux failed");
            hr = _graphBuilder.RemoveFilter(tmp);
            Release.ComObject("bda receiver", tmp);
          }
          else
          {
            _filterCapture = tmp;
            _captureDevice = devices[i];
            DevicesInUse.Instance.Add(devices[i]);
            Log.Log.WriteFile("dvb:OK");
            break;
          }
        }
        else
        {
          // Try another...
          hr = _graphBuilder.RemoveFilter(tmp);
          Release.ComObject("bda receiver", tmp);
        }
      }

      if (_filterCapture == null)
      {
        Log.Log.WriteFile("dvb:  No TvCapture device found....");
        IPin pinIn = DsFindPin.ByDirection(_infTeeMain, PinDirection.Input, 0);
        //IPin pinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Input, 0);
        IPin pinOut = DsFindPin.ByDirection(_filterTuner, PinDirection.Output, 0);
        hr = _graphBuilder.Connect(pinOut, pinIn);
        if (hr == 0)
        {
          Release.ComObject("inftee main pin in", pinIn);
          Release.ComObject("tuner pin out", pinOut);
          Log.Log.WriteFile("dvb:  using only tv tuner ifilter...");
          ConnectMpeg2DemuxersToMainTee();
          GetVideoAudioPins();
          _conditionalAccess = new ConditionalAccess(_filterTuner, _filterCapture);
          return;
        }
        Release.ComObject("tuner pin out", pinOut);
        Release.ComObject("inftee main pin in", pinIn);
        Log.Log.WriteFile("dvb:  unable to use single tv tuner filter...");
        throw new TvException("No Tv Receiver filter found");
      }
      ConnectMpeg2DemuxersToMainTee();
      GetVideoAudioPins();
      _conditionalAccess = new ConditionalAccess(_filterTuner, _filterCapture);
    }

    #region IDisposable

    public void Dispose()
    {
      Decompose();
    }
    #endregion



    /// <summary>
    /// adds the mpeg-2 demultiplexer filter to the graph
    /// </summary>
    protected void AddMpeg2DemuxerTif()
    {
      if (!CheckThreadId()) return;
      if (_filterMpeg2DemuxTif != null) return;
      Log.Log.WriteFile("dvb:Add MPEG2 Demultiplexer filter");
      int hr = 0;

      _filterMpeg2DemuxTif = (IBaseFilter)new MPEG2Demultiplexer();

      hr = _graphBuilder.AddFilter(_filterMpeg2DemuxTif, "MPEG2-Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddMpeg2DemuxerTif returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer for tif");
      }
      //multi demux

#if MULTI_DEMUX
      _filterMpeg2DemuxAnalyzer = (IBaseFilter)new MPEG2Demultiplexer();

      hr = _graphBuilder.AddFilter(_filterMpeg2DemuxAnalyzer, "Analyzer MPEG2 Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddMpeg2DemuxerDemux returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer for analyzer");
      }

      _filterMpeg2DemuxTs = (IBaseFilter)new MPEG2Demultiplexer();
      hr = _graphBuilder.AddFilter(_filterMpeg2DemuxTs, "Timeshift MPEG2 Demultiplexer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddMpeg2DemuxerDemux returns:0x{0:X}", hr);
        throw new TvException("Unable to add MPEG2 demultiplexer for analyzer");
      }

#endif
      Log.Log.WriteFile("dvb:add Inf Tee filter");
      _infTeeMain = (IBaseFilter)new InfTee();
      hr = _graphBuilder.AddFilter(_infTeeMain, "Inf Tee");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
    }
    /// <summary>
    /// Connects the mpeg2 demuxers to main tee.
    /// </summary>
    protected void ConnectMpeg2DemuxersToMainTee()
    {
      //multi demux

      //connect the inftee main -> TIF MPEG2 Demultiplexer
      Log.Log.WriteFile("dvb:connect mpeg-2 demuxer to maintee");
      IPin mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 0);
      IPin demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Input, 0);
      int hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
      Release.ComObject("maintee pin0", mainTeeOut);
      Release.ComObject("tifdemux pinin", demuxPinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
#if MULTI_DEMUX
      mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 1);
      demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxAnalyzer, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
      Release.ComObject("maintee pin0", mainTeeOut);
      Release.ComObject("analyzer demux pinin", demuxPinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
      mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 2);
      demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTs, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
      Release.ComObject("maintee pin0", mainTeeOut);
      Release.ComObject("mpg demux pinin", demuxPinIn);
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
#endif
    }

    /// <summary>
    /// Gets the video audio pins.
    /// </summary>
    protected void GetVideoAudioPins()
    {

      if (_filterTsAnalyzer == null)
      {
        Log.Log.WriteFile("dvb:Add Mediaportal Ts Analyzer filter");
        _filterTsAnalyzer = (IBaseFilter)new MpTsAnalyzer();
        int hr = _graphBuilder.AddFilter(_filterTsAnalyzer, "MediaPortal Ts Analyzer");
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:Add main Ts Analyzer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Ts Analyzer filter");
        }
        IPin pinTee = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 1);
        if (pinTee == null)
        {
          if (hr != 0)
          {
            Log.Log.WriteFile("dvb:unable to find pin#2 on inftee filter");
            throw new TvException("unable to find pin#2 on inftee filter");
          }
        }
        IPin pin = DsFindPin.ByDirection(_filterTsAnalyzer, PinDirection.Input, 0);
        if (pin == null)
        {
          if (hr != 0)
          {
            Log.Log.WriteFile("dvb:unable to find pin on ts analyzer filter");
            throw new TvException("unable to find pin on ts analyzer filter");
          }
        }
        hr = _graphBuilder.Connect(pinTee, pin);
        Release.ComObject("pinTsWriterIn", pin);
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:unable to connect inftee to analyzer filter :0x{0:X}", hr);
          throw new TvException("unable to connect inftee to analyzer filter");
        }

        _interfaceChannelScan = (ITsChannelScan)_filterTsAnalyzer;
        _interfaceEpgGrabber = (ITsEpgScanner)_filterTsAnalyzer;
        _interfacePmtGrabber = (ITsPmtGrabber)_filterTsAnalyzer;
      }
    }



    /// <summary>
    /// adds the BDA Transport Information Filter  and the
    /// MPEG-2 sections & tables filter to the graph 
    /// </summary>
    protected void AddTransportStreamFiltersToGraph()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:AddTransportStreamFiltersToGraph");
      int hr = 0;
      DsDevice[] devices;

      // Add two filters needed in a BDA graph
      devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        if (devices[i].Name.Equals("BDA MPEG2 Transport Information Filter"))
        {
          Log.Log.Write("    add BDA MPEG2 Transport Information Filter filter");
          try
          {
            hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out _filterTIF);
            if (hr != 0)
            {
              Log.Log.Write("    unable to add BDA MPEG2 Transport Information Filter filter:0x{0:X}", hr);
            }
          }
          catch (Exception)
          {
            Log.Log.Write("    unable to add BDA MPEG2 Transport Information Filter filter");
          }
          continue;
        }

        if (devices[i].Name.Equals("MPEG-2 Sections and Tables"))
        {
          Log.Log.Write("    add MPEG-2 Sections and Tables filter");
          try
          {
            hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out _filterSectionsAndTables);
            if (hr != 0)
            {
              Log.Log.Write("    unable to add MPEG-2 Sections and Tables filter:0x{0:X}", hr);
            }
          }
          catch (Exception)
          {
            Log.Log.Write("    unable to add MPEG-2 Sections and Tables filter");
          }
          continue;
        }
      }

      IPin pinInTif = DsFindPin.ByDirection(_filterTIF, PinDirection.Input, 0);
      IPin pinInSec = DsFindPin.ByDirection(_filterSectionsAndTables, PinDirection.Input, 0);
      Log.Log.WriteFile("    pinTif:{0}", FilterGraphTools.LogPinInfo(pinInTif));
      Log.Log.WriteFile("    pinSec:{0}", FilterGraphTools.LogPinInfo(pinInSec));
      //connect tif
      Log.Log.WriteFile("    Connect tif and mpeg2 sections and tables");
      IEnumPins enumPins;
      _filterMpeg2DemuxTif.EnumPins(out enumPins);
      bool tifConnected = false;
      bool mpeg2SectionsConnected = false;
      int pinNr = 0;
      while (true)
      {
        pinNr++;
        PinDirection pinDir;
        AMMediaType[] mediaTypes = new AMMediaType[2];
        IPin[] pins = new IPin[2];
        int fetched;
        enumPins.Next(1, pins, out fetched);
        if (fetched != 1) break;
        pins[0].QueryDirection(out pinDir);
        if (pinDir == PinDirection.Input)
        {
          Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
          continue;
        }
        IEnumMediaTypes enumMedia;
        pins[0].EnumMediaTypes(out enumMedia);
        enumMedia.Next(1, mediaTypes, out fetched);
        Release.ComObject("IEnumMedia", enumMedia);
        if (mediaTypes[0].majorType == MediaType.Audio || mediaTypes[0].majorType == MediaType.Video)
        {
          DsUtils.FreeAMMediaType(mediaTypes[0]);
          Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
          continue;
        }
        DsUtils.FreeAMMediaType(mediaTypes[0]);
        if (tifConnected == false)
        {
          //Log.Log.WriteFile("dvb:try tif:{0}", FilterGraphTools.LogPinInfo(pins[0]));
          hr = _graphBuilder.Connect(pins[0], pinInTif);
          if (hr == 0)
          {
            Log.Log.WriteFile("    tif connected");
            tifConnected = true;
            Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
            continue;
          }
          else
          {
            // Log.Log.WriteFile("    tif not connected:0x{0:X}", hr);
          }
        }
        if (mpeg2SectionsConnected == false)
        {
          //Log.Log.WriteFile("    try sections&tables:{0}", FilterGraphTools.LogPinInfo(pins[0]));
          hr = _graphBuilder.Connect(pins[0], pinInSec);
          if (hr == 0)
          {
            Log.Log.WriteFile("    mpeg 2 sections and tables connected");
            mpeg2SectionsConnected = true;
          }
          else
          {
            //            Log.Log.WriteFile("    dvb:mpeg 2 sections and tables not connected:0x{0:X}", hr);
          }
        }
        Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
      }
      Release.ComObject("IEnumMedia", enumPins);
      Release.ComObject("TIF pin in", pinInTif);
      Release.ComObject("mpeg2 sections&tables pin in", pinInSec);
      if (tifConnected == false)
      {
        Log.Log.WriteFile("    unable to connect transport information filter");
        throw new TvException("unable to connect transport information filter");
      }
      if (mpeg2SectionsConnected == false)
      {
        Log.Log.WriteFile("    unable to connect mpeg 2 sections and tables filter");
        throw new TvException("unable to connect mpeg 2 sections and tables filter");
      }
    }



    /// <summary>
    /// Connects the outputs of the mpeg-2 demultiplexer filter to streambuffer sink filter
    /// </summary>
    protected void ConnectSinkFilter()
    {/*
      if (!CheckThreadId()) return;
      if (_filterStreamBufferSink == null) return;
      Log.Log.WriteFile("dvb:ConnectSinkFilter");
      int hr = 0;
      IPin pinOut;
      int pinsConnected = 0;
      // Connect the 5 MPEG-2 Demux output pins
      for (int i = 0; i < 10; i++)
      {
        pinOut = DsFindPin.ByDirection(_filterMpeg2DemuxTs, PinDirection.Output, i);
        if (pinOut == null) return;
        IEnumMediaTypes enumMedia;
        pinOut.EnumMediaTypes(out enumMedia);

        while (true)
        {
          int fetched;
          AMMediaType[] mediaTypes = new AMMediaType[2];
          enumMedia.Next(1, mediaTypes, out fetched);
          if (fetched != 1) break;

          if (mediaTypes[0].majorType == MediaType.Video || mediaTypes[0].majorType == MediaType.Audio)
          {
            IPin pin1 = DsFindPin.ByDirection((IBaseFilter)_filterStreamBufferSink, PinDirection.Input, 0);
            IPin pin2 = DsFindPin.ByDirection((IBaseFilter)_filterStreamBufferSink, PinDirection.Input, 1);
            hr = _graphBuilder.Connect(pinOut, pin1);
            if (hr == 0)
            {
              Log.Log.WriteFile("dvb:connected stream buffer sink pin 1");
              pinsConnected++;
            }
            else if (pin2 != null)
            {
              hr = _graphBuilder.Connect(pinOut, pin2);
              if (hr == 0)
              {
                Log.Log.WriteFile("dvb:connected stream buffer sink pin 2");
                pinsConnected++;
              }
            }
            if (pin1 != null) Release.ComObject("Streambuffersink pin0", pin1);
            if (pin2 != null) Release.ComObject("Streambuffersink pin1", pin2);
            DsUtils.FreeAMMediaType(mediaTypes[0]);
            if (pinsConnected == 2)
            {
              return;
            }
            break;
          }
          else
          {
            DsUtils.FreeAMMediaType(mediaTypes[0]);
          }
        }
        Release.ComObject("mpeg2 demux pin:" + i.ToString(), pinOut);
      }*/
    }

    /// <summary>
    /// destroys the graph and cleans up any resources
    /// </summary>
    protected void Decompose()
    {
      if (_graphBuilder == null) return;
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:Decompose");
      int hr = 0;

      if (_epgGrabbing)
      {
        _epgGrabbing = false;
        if (OnEpgReceived != null)
        {
          Log.Log.WriteFile("dvb:cancel epg->decompose");
          OnEpgReceived(this, null);
        }
      }
      _pmtTimer.Enabled = false;
      _graphRunning = false;


      Log.Log.WriteFile("  stop");
      // Decompose the graph
      //hr = (_graphBuilder as IMediaControl).StopWhenReady();
      hr = (_graphBuilder as IMediaControl).Stop();

      Log.Log.WriteFile("  remove all filters");
      FilterGraphTools.RemoveAllFilters(_graphBuilder);


      Log.Log.WriteFile("  free...");
      _interfaceChannelScan = null;
      _interfaceEpgGrabber = null;

#if MULTI_DEMUX
      if (_filterMpeg2DemuxTs != null)
      {
        Release.ComObject("MPG MPEG2 demux filter", _filterMpeg2DemuxTs); _filterMpeg2DemuxTs = null;
      }
      if (_filterMpeg2DemuxAnalyzer != null)
      {
        Release.ComObject("Analyzer MPEG2 demux filter", _filterMpeg2DemuxAnalyzer); _filterMpeg2DemuxAnalyzer = null;
      }
#endif

      if (_infTeeMain != null)
      {
        Release.ComObject("main inftee filter", _infTeeMain); _infTeeMain = null;
      }
      if (_filterMpeg2DemuxTif != null)
      {
        Release.ComObject("TIF MPEG2 demux filter", _filterMpeg2DemuxTif); _filterMpeg2DemuxTif = null;
      }
      if (_filterTuner != null)
      {
        Release.ComObject("tuner filter", _filterTuner); _filterTuner = null;
      }
      if (_filterCapture != null)
      {
        Release.ComObject("capture filter", _filterCapture); _filterCapture = null;
      }
      if (_filterTIF != null)
      {
        Release.ComObject("TIF filter", _filterTIF); _filterTIF = null;
      }
      if (_filterSectionsAndTables != null)
      {
        Release.ComObject("secions&tables filter", _filterSectionsAndTables); _filterSectionsAndTables = null;
      }
      Log.Log.WriteFile("  free pins...");
      if (_filterTsAnalyzer != null)
      {
        Release.ComObject("TSWriter filter", _filterTsAnalyzer); _filterTsAnalyzer = null;
      }

      Log.Log.WriteFile("  free graph...");
      if (_rotEntry != null)
      {
        _rotEntry.Dispose();
      }
      if (_capBuilder != null)
      {
        Release.ComObject("capture builder", _capBuilder); _capBuilder = null;
      }
      if (_graphBuilder != null)
      {
        Release.ComObject("graph builder", _graphBuilder); _graphBuilder = null;
      }
      Log.Log.WriteFile("  free devices...");
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
      if (_teletextDecoder != null)
      {
        _teletextDecoder.ClearBuffer();
        _teletextDecoder = null;
      }

      Log.Log.WriteFile("  decompose done...");
    }

    #endregion

    #region pidmapping

    /// <summary>
    /// Maps the correct pids to the SI analyzer pin
    /// </summary>
    /// <param name="pmtPid">pid of the PMT</param>
    protected void SetAnalyzerMapping(int pmtPid)
    {
      if (pmtPid < 0) return;
      if (!CheckThreadId()) return;
      _interfacePmtGrabber.SetCallBack(this);
      _interfacePmtGrabber.SetPmtPid(pmtPid);
      //Log.Log.WriteFile("dvb:SetAnalyzerMapping done");
    }


    /// <summary>
    /// maps the correct pids to the TsFileSink filter & teletext pins
    /// </summary>
    /// <param name="info"></param>
    protected void SetMpegPidMapping(ChannelInfo info)
    {
      //if (!CheckThreadId()) return;
      try
      {
        Log.Log.WriteFile("dvb:SetMpegPidMapping");

        ITsVideoAnalyzer writer = (ITsVideoAnalyzer)_filterTsAnalyzer;
        ArrayList hwPids = new ArrayList();
        hwPids.Add((ushort)0x0);//PAT
        hwPids.Add((ushort)0x1);//CAT
        hwPids.Add((ushort)0x10);//NIT
        hwPids.Add((ushort)0x11);//SDT
        if (_isATSC)
        {
          hwPids.Add((ushort)0x1ffb);//ATSC
        }
        if (_epgGrabbing)
        {
          hwPids.Add((ushort)0xd2);//MHW
          hwPids.Add((ushort)0xd3);//MHW
          hwPids.Add((ushort)0x12);//EIT
        }
        Log.Log.WriteFile("    pcr pid:0x{0:X}", info.pcr_pid);
        Log.Log.WriteFile("    pmt pid:0x{0:X}", info.network_pmt_PID);
        foreach (PidInfo pmtData in info.pids)
        {
          Log.Log.WriteFile("  pid:{0:X} type::{1} audio:{2} video:{3} ac3:{4} txt:{5} sub:{6}",
              pmtData.pid, pmtData.stream_type, pmtData.isAudio, pmtData.isVideo, pmtData.isAC3Audio, pmtData.isTeletext, pmtData.isDVBSubtitle);
          if (pmtData.pid == 0 || pmtData.pid > 0x1fff) continue;
          if (pmtData.isTeletext)
          {
            Log.Log.WriteFile("    map teletext pid:0x{0:X}", pmtData.pid);
            if (GrabTeletext)
            {
              ITsTeletextGrabber grabber = (ITsTeletextGrabber)_filterTsAnalyzer;
              grabber.SetTeletextPid((short)pmtData.pid);
            }
            hwPids.Add((ushort)pmtData.pid);
            _hasTeletext = true;
          }
          if (pmtData.isAC3Audio || pmtData.isAudio)
          {
            if (_currentAudioStream == null)
            {
              _currentAudioStream = new DVBAudioStream();
              _currentAudioStream.Pid = pmtData.pid;
              _currentAudioStream.Language = pmtData.language;
              _currentAudioStream.StreamType = AudioStreamType.Mpeg2;
              if (pmtData.isAC3Audio)
                _currentAudioStream.StreamType = AudioStreamType.AC3;
            }

            if (_currentAudioStream.Pid == pmtData.pid)
            {
              Log.Log.WriteFile("    map audio pid:0x{0:X}", pmtData.pid);
              writer.SetAudioPid((short)pmtData.pid);
            }
            hwPids.Add((ushort)pmtData.pid);
          }

          if (pmtData.isVideo)
          {
            Log.Log.WriteFile("    map video pid:0x{0:X}", pmtData.pid);
            hwPids.Add((ushort)pmtData.pid);
            writer.SetVideoPid((short)pmtData.pid);
            if (info.pcr_pid > 0 && info.pcr_pid != pmtData.pid)
            {
              hwPids.Add((ushort)info.pcr_pid);
            }
          }
        }
        if (info.network_pmt_PID >= 0 && ((DVBBaseChannel)_currentChannel).ServiceId >= 0)
        {
          hwPids.Add((ushort)info.network_pmt_PID);
          SendHwPids(hwPids);
        }

        if (_startTimeShifting)
        {
          Log.Log.WriteFile("dvb: set timeshifting pids");
          _startTimeShifting = false;
          ITsTimeShift record = _filterTsAnalyzer as ITsTimeShift;
          DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
          record.Reset();
          record.SetPcrPid((short)dvbChannel.PcrPid);
          foreach (PidInfo pidInfo in info.pids)
          {
            if (pidInfo.isAC3Audio || pidInfo.isAudio || pidInfo.isVideo)
            {
              record.AddPesStream((short)pidInfo.pid, (pidInfo.isAC3Audio || pidInfo.isAudio), pidInfo.isVideo);
            }
          }
          Log.Log.WriteFile("dvb: start timeshifting");
          record.Start();
        }
        else if (_graphState == GraphState.TimeShifting || _graphState == GraphState.Recording)
        {
          Log.Log.WriteFile("dvb: set timeshifting pids");
          ITsTimeShift record = _filterTsAnalyzer as ITsTimeShift;
          DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
          if (dvbChannel != null)
          {
            record.Reset();
            record.SetPcrPid((short)dvbChannel.PcrPid);
            foreach (PidInfo pidInfo in info.pids)
            {
              if (pidInfo.isAC3Audio || pidInfo.isAudio || pidInfo.isVideo)
              {
                record.AddPesStream((short)pidInfo.pid, (pidInfo.isAC3Audio || pidInfo.isAudio), pidInfo.isVideo);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }
    #endregion

    #region signal quality, level etc

    public void ResetSignalUpdate()
    {
      _lastSignalUpdate = DateTime.MinValue;
    }
    /// <summary>
    /// updates the signal quality/level & tuner locked statusses
    /// </summary>
    protected void UpdateSignalQuality()
    {
      TimeSpan ts = DateTime.Now - _lastSignalUpdate;
      if (ts.TotalMilliseconds < 5000) return;
      _lastSignalUpdate = DateTime.Now;
      _tunerLocked = false;
      _signalLevel = 0;
      _signalPresent = false;
      _signalQuality = 0;
      if (_graphRunning == false) return;
      if (Channel == null) return;
      if (_filterNetworkProvider == null)
      {
        return;
      }
      if (!CheckThreadId()) return;

      //Log.Log.WriteFile("dvb:UpdateSignalQuality");
      //if we dont have an IBDA_SignalStatistics interface then return
      if (_tunerStatistics == null)
      {
        Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
        return;
      }
      if (_tunerStatistics.Count == 0)
      {
        Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
        return;
      }
      bool isTunerLocked = false;
      bool isSignalPresent = false;
      long signalQuality = 0;
      long signalStrength = 0;

      for (int i = 0; i < _tunerStatistics.Count; i++)
      {
        IBDA_SignalStatistics stat = (IBDA_SignalStatistics)_tunerStatistics[i];
        bool isLocked = false;
        bool isPresent = false;
        int quality = 0;
        int strength = 0;
        try
        {
          //is the tuner locked?
          stat.get_SignalLocked(out isLocked);
          isTunerLocked |= isLocked;
        }
        catch (COMException)
        {
          //          Log.Log.WriteFile( "UpdateSignalPresent() locked :{0}", ex.Message);
        }
        catch (Exception)
        {
          //          Log.Log.WriteFile( "UpdateSignalPresent() locked :{0}", ex.Message);
        }
        try
        {
          //is a signal present?
          stat.get_SignalPresent(out isPresent);
          isSignalPresent |= isPresent;
        }
        catch (COMException)
        {
          //          Log.Log.WriteFile( "UpdateSignalPresent() present :{0}", ex.Message);
        }
        catch (Exception)
        {
          //           Log.Log.WriteFile( "UpdateSignalPresent() present :{0}", ex.Message);
        }
        try
        {
          //is a signal quality ok?
          stat.get_SignalQuality(out quality); //1-100
          if (quality > 0) signalQuality += quality;
        }
        catch (COMException)
        {
          //          Log.Log.WriteFile( "UpdateSignalPresent() quality :{0}", ex.Message);
        }
        catch (Exception)
        {
          //          Log.Log.WriteFile( "UpdateSignalPresent() quality :{0}", ex.Message);
        }
        try
        {
          //is a signal strength ok?
          stat.get_SignalStrength(out strength); //1-100
          if (strength > 0) signalStrength += strength;
        }
        catch (COMException)
        {
          //          Log.Log.WriteFile( "UpdateSignalPresent() quality :{0}", ex.Message);
        }
        catch (Exception)
        {
          //          Log.Log.WriteFile( "UpdateSignalPresent() quality :{0}", ex.Message);
        }
        //        Log.Log.WriteFile( "  #{0}  locked:{1} present:{2} quality:{3} strength:{4}", i, isLocked, isPresent, quality, strength);
      }
      if (_tunerStatistics.Count > 0)
      {
        _signalQuality = (int)signalQuality / _tunerStatistics.Count;
        _signalLevel = (int)signalStrength / _tunerStatistics.Count;
      }
      if (isTunerLocked)
        _tunerLocked = true;
      else
        _tunerLocked = false;

      if (isTunerLocked)
      {
        _signalPresent = true;
      }
      else
      {
        _signalPresent = false;
      }
    }//public bool SignalPresent()
    #endregion

    #region properties
    /// <summary>
    /// Turn on/off teletext grabbing
    /// </summary>
    public bool GrabTeletext
    {
      get
      {
        return _grabTeletext;
      }
      set
      {
        _grabTeletext = value;
        ITsTeletextGrabber grabber = (ITsTeletextGrabber)_filterTsAnalyzer;
        if (_grabTeletext)
        {
          int teletextPid = -1;
          foreach (PidInfo pidInfo in _channelInfo.pids)
          {
            if (pidInfo.isTeletext)
            {
              teletextPid = pidInfo.pid;
              break;
            }
          }

          if (teletextPid == -1)
          {
            grabber.Stop();
            _grabTeletext = false;
            return;
          }
          grabber.SetCallBack(this);
          grabber.SetTeletextPid((short)teletextPid);
          grabber.Start();
        }
        else
        {
          grabber.Stop();
        }
      }
    }

    /// <summary>
    /// Property which returns true when the current channel contains teletext
    /// </summary>
    public bool HasTeletext
    {
      get
      {
        return (_hasTeletext);
      }
    }
    /// <summary>
    /// returns the ITeletext interface which can be used for
    /// getting teletext pages
    /// </summary>
    public ITeletext TeletextDecoder
    {
      get
      {
        return _teletextDecoder;
      }
    }
    /// <summary>
    /// boolean indicating if tuner is locked to a signal
    /// </summary>
    public bool IsTunerLocked
    {
      get
      {
        UpdateSignalQuality();
        return _tunerLocked;
      }
    }
    /// <summary>
    /// returns the signal quality
    /// </summary>
    public int SignalQuality
    {
      get
      {
        UpdateSignalQuality();
        if (_signalLevel < 0) _signalQuality = 0;
        if (_signalLevel > 100) _signalQuality = 100;
        return _signalQuality;
      }
    }
    /// <summary>
    /// returns the signal level
    /// </summary>
    public int SignalLevel
    {
      get
      {
        UpdateSignalQuality();
        if (_signalLevel < 0) _signalLevel = 0;
        if (_signalLevel > 100) _signalLevel = 100;
        return _signalLevel;
      }
    }

    /// <summary>
    /// returns the ITsChannelScan interface for the graph
    /// </summary>
    public ITsChannelScan StreamAnalyzer
    {
      get
      {
        return _interfaceChannelScan;
      }
    }
    /// <summary>
    /// returns the IChannel to which we are currently tuned
    /// </summary>
    public IChannel Channel
    {
      get
      {
        return _currentChannel;
      }
    }


    #endregion

    #region recording

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <param name="startTime">time the recording should start (0=now)</param>
    /// <returns></returns>
    protected void StartRecord(string fileName, RecordingType recordingType, ref long startTime)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:StartRecord({0})", fileName);

      int hr;
      if (_filterTsAnalyzer != null)
      {
        Log.Log.WriteFile("dvb:SetRecordingFileName: uses .mpg");
        ITsRecorder record = _filterTsAnalyzer as ITsRecorder;
        hr = record.SetRecordingFileName(fileName);
        DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
        record.SetPcrPid((short)dvbChannel.PcrPid);
        foreach (PidInfo info in _channelInfo.pids)
        {
          if (info.isAC3Audio || info.isAudio || info.isVideo)
          {
            record.AddPesStream((short)info.pid, (info.isAC3Audio || info.isAudio), info.isVideo);
          }
        }
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:SetRecordingFileName failed:{0:X}", hr);
        }
        hr = record.StartRecord();
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:StartRecord failed:{0:X}", hr);
        }

      }

      _dateRecordingStarted = DateTime.Now;
    }


    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected void StopRecord()
    {
      if (!CheckThreadId()) return;
      int hr;
      Log.Log.WriteFile("dvb:StopRecord()");

      if (_filterTsAnalyzer != null)
      {
        ITsRecorder record = _filterTsAnalyzer as ITsRecorder;
        record.StopRecord();

      }
      _recordingFileName = "";
    }
    #endregion

    #region epg & scanning


    /// <summary>
    /// returns the ITVEPG interface used for grabbing the epg
    /// </summary>
    public ITVEPG EpgInterface
    {
      get
      {
        return new DvbEpgGrabber(this);
      }
    }
    /// <summary>
    /// Start grabbing the epg
    /// </summary>
    public void GrabEpg()
    {
      if (!CheckThreadId()) return;

      Log.Log.Write("dvb:grab epg...");
      _interfaceEpgGrabber.GrabEPG();
      _interfaceEpgGrabber.GrabMHW();
      _epgGrabbing = true;
    }
    /// <summary>
    /// Gets the UTC.
    /// </summary>
    /// <param name="val">The val.</param>
    /// <returns></returns>
    int getUTC(int val)
    {
      if ((val & 0xF0) >= 0xA0)
        return 0;
      if ((val & 0xF) >= 0xA)
        return 0;
      return ((val & 0xF0) >> 4) * 10 + (val & 0xF);
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
          if (dvbReady == false || mhwReady == false) return null;

          List<EpgChannel> epgChannels = new List<EpgChannel>();
          if (mhwReady)
          {
            short titleCount;
            _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
            Log.Log.WriteFile("dvb:mhw ready {0} titles found", titleCount);
            for (int i = 0; i < titleCount; ++i)
            {
              short id = 0, transportid = 0, networkid = 0, channelnr = 0, channelid = 0, programid = 0, themeid = 0, PPV = 0, duration = 0;
              byte summaries = 0;
              uint datestart = 0, timestart = 0;
              IntPtr ptrTitle, ptrProgramName;
              IntPtr ptrChannelName, ptrSummary, ptrTheme;
              _interfaceEpgGrabber.GetMHWTitle((short)i, ref id, ref transportid, ref networkid, ref channelnr, ref programid, ref themeid, ref PPV, ref summaries, ref duration, ref datestart, ref timestart, out ptrTitle, out ptrProgramName);
              _interfaceEpgGrabber.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid, out ptrChannelName);
              _interfaceEpgGrabber.GetMHWSummary(programid, out ptrSummary);
              _interfaceEpgGrabber.GetMHWTheme(themeid, out ptrTheme);

              string channelName, title, programName, summary, theme;
              channelName = Marshal.PtrToStringAnsi(ptrChannelName);
              title = Marshal.PtrToStringAnsi(ptrTitle);
              programName = Marshal.PtrToStringAnsi(ptrProgramName);
              summary = Marshal.PtrToStringAnsi(ptrSummary);
              theme = Marshal.PtrToStringAnsi(ptrTheme);
              if (channelName == null) channelName = "";
              if (title == null) title = "";
              if (programName == null) programName = "";
              if (summary == null) summary = "";
              if (theme == null) theme = "";
              channelName = channelName.Trim();
              title = title.Trim();
              programName = programName.Trim();
              summary = summary.Trim();
              theme = theme.Trim();

              EpgChannel epgChannel = null;
              foreach (EpgChannel chan in epgChannels)
              {
                DVBBaseChannel dvbChan = (DVBBaseChannel)chan.Channel;
                if (dvbChan.NetworkId == networkid && dvbChan.TransportId == transportid && dvbChan.ServiceId == channelnr)
                {
                  epgChannel = chan;
                  break;
                }
              }
              if (epgChannel == null)
              {
                DVBBaseChannel dvbChan = new DVBBaseChannel();
                dvbChan.NetworkId = networkid;
                dvbChan.TransportId = transportid;
                dvbChan.ServiceId = channelnr;
                dvbChan.Name = channelName;
                epgChannel = new EpgChannel();
                epgChannel.Channel = dvbChan;
                epgChannels.Add(epgChannel);
              }


              uint d1 = datestart;
              uint m = timestart & 0xff;
              uint h1 = (timestart >> 16) & 0xff;
              DateTime programStartTime = System.DateTime.Now;
              DateTime dayStart = System.DateTime.Now;
              dayStart = dayStart.Subtract(new TimeSpan(1, dayStart.Hour, dayStart.Minute, dayStart.Second, dayStart.Millisecond));
              int day = (int)dayStart.DayOfWeek;

              programStartTime = dayStart;
              int minVal = (int)((d1 - day) * 86400 + h1 * 3600 + m * 60);
              if (minVal < 21600)
                minVal += 604800;

              programStartTime = programStartTime.AddSeconds(minVal);

              EpgProgram program = new EpgProgram(programStartTime, programStartTime.AddMinutes(duration));

              EpgLanguageText epgLang = new EpgLanguageText("ALL", title, summary, theme);
              program.Text.Add(epgLang);
              epgChannel.Programs.Add(program);
            }
          }

          if (dvbReady)
          {
            uint channelCount = 0;
            ushort networkid = 0;
            ushort transportid = 0;
            ushort serviceid = 0;
            _interfaceEpgGrabber.GetEPGChannelCount(out channelCount);
            Log.Log.WriteFile("dvb:dvb ready. Epg for {0} channels", channelCount);
            for (uint x = 0; x < channelCount; ++x)
            {
              _interfaceEpgGrabber.GetEPGChannel((uint)x, ref networkid, ref transportid, ref serviceid);
              EpgChannel epgChannel = new EpgChannel();
              DVBBaseChannel chan = new DVBBaseChannel();
              chan.NetworkId = networkid;
              chan.TransportId = transportid;
              chan.ServiceId = serviceid;
              epgChannel.Channel = chan;


              uint eventCount = 0;
              _interfaceEpgGrabber.GetEPGEventCount((uint)x, out eventCount);
              for (uint i = 0; i < eventCount; ++i)
              {
                uint start_time_MJD = 0, start_time_UTC = 0, duration = 0, languageId = 0, languageCount = 0;
                string title, description, genre;
                IntPtr ptrTitle = IntPtr.Zero;
                IntPtr ptrDesc = IntPtr.Zero;
                IntPtr ptrGenre = IntPtr.Zero;
                _interfaceEpgGrabber.GetEPGEvent((uint)x, (uint)i, out languageCount, out start_time_MJD, out start_time_UTC, out duration, out ptrGenre);
                genre = Marshal.PtrToStringAnsi(ptrGenre);

                int duration_hh = getUTC((int)((duration >> 16)) & 255);
                int duration_mm = getUTC((int)((duration >> 8)) & 255);
                int duration_ss = 0;//getUTC((int) (duration )& 255);
                int starttime_hh = getUTC((int)((start_time_UTC >> 16)) & 255);
                int starttime_mm = getUTC((int)((start_time_UTC >> 8)) & 255);
                int starttime_ss = 0;//getUTC((int) (start_time_UTC )& 255);

                if (starttime_hh > 23) starttime_hh = 23;
                if (starttime_mm > 59) starttime_mm = 59;
                if (starttime_ss > 59) starttime_ss = 59;

                if (duration_hh > 23) duration_hh = 23;
                if (duration_mm > 59) duration_mm = 59;
                if (duration_ss > 59) duration_ss = 59;

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

                try
                {
                  DateTime dtUTC = new DateTime(starttime_y, starttime_m, starttime_d, starttime_hh, starttime_mm, starttime_ss, 0);
                  DateTime dtStart = dtUTC.ToLocalTime();
                  DateTime dtEnd = dtStart.AddHours(duration_hh);
                  dtEnd = dtEnd.AddMinutes(duration_mm);
                  dtEnd = dtEnd.AddSeconds(duration_ss);
                  EpgProgram epgProgram = new EpgProgram(dtStart, dtEnd);
                  //EPGEvent newEvent = new EPGEvent(genre, dtStart, dtEnd);
                  for (int z = 0; z < languageCount; ++z)
                  {
                    _interfaceEpgGrabber.GetEPGLanguage((uint)x, (uint)i, (uint)z, out languageId, out ptrTitle, out ptrDesc);
                    title = Marshal.PtrToStringAnsi(ptrTitle);
                    description = Marshal.PtrToStringAnsi(ptrDesc);
                    string language = String.Empty;
                    language += (char)((languageId >> 16) & 0xff);
                    language += (char)((languageId >> 8) & 0xff);
                    language += (char)((languageId) & 0xff);

                    if (title == null) title = "";
                    if (description == null) description = "";
                    if (language == null) language = "";
                    if (genre == null) genre = "";
                    title = title.Trim();
                    description = description.Trim();
                    language = language.Trim();
                    genre = genre.Trim();
                    EpgLanguageText epgLangague = new EpgLanguageText(language, title, description, genre);
                    epgProgram.Text.Add(epgLangague);

                  }
                  epgChannel.Programs.Add(epgProgram);
                }
                catch (Exception ex)
                {
                  Log.Log.Write(ex);
                }
              }//for (uint i = 0; i < eventCount; ++i)
              epgChannel.Sort();
              epgChannels.Add(epgChannel);
            }//for (uint x = 0; x < channelCount; ++x)
          }
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

    #region Conditional Access
    /// <summary>
    /// timer callback. This method checks if a new PMT has been received
    /// and ifso updates the various pid mappings and updates the CI interface
    /// with the new PMT
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void _pmtTimer_ElapsedForm(object sender, EventArgs args)
    {
      _pmtTimer_Elapsed(null, null);
    }
    /// <summary>
    /// Handles the Elapsed event of the _pmtTimer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="T:System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
    void _pmtTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        if (_graphRunning == false) return;
#if FORM
        if (_newPMT)
        {
          if (SendPmtToCam())
          {
                SetMpegPidMapping(_channelInfo);
          } 
          _newPMT=false;
        }
#endif

        if (_epgGrabbing)
        {
          bool dvbReady, mhwReady;
          _interfaceEpgGrabber.IsEPGReady(out dvbReady);
          _interfaceEpgGrabber.IsMHWReady(out mhwReady);
          if (dvbReady && mhwReady)
          {
            _epgGrabbing = false;
            Log.Log.Write("epg done");
            if (OnEpgReceived != null)
            {
              OnEpgReceived(this, Epg);
            }
          }
        }

        /*if (IsReceivingAudioVideo == false)
        {
          Log.Log.WriteFile("dvb: resend pmt to cam");
          _pmtVersion = -1;
          if (SendPmtToCam())
          {
                SetMpegPidMapping(_channelInfo);
          } 
        }*/
      }
      catch (Exception ex)
      {
        Log.Log.WriteFile("dvb:{0}", ex.Message);
        Log.Log.WriteFile("dvb:{0}", ex.Source);
        Log.Log.WriteFile("dvb:{0}", ex.StackTrace);
      }
    }
    /// <summary>
    /// Sends the hw pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    public void SendHwPids(ArrayList pids)
    {
      if (System.IO.File.Exists("usehwpids.txt"))
      {
        if (_conditionalAccess != null)
        {
          _conditionalAccess.SendPids((DVBBaseChannel)_currentChannel, pids);
        }
        return;
      }
    }
    #endregion

    #region ITeletextCallBack Members

    /// <summary>
    /// callback from the TsWriter filter when it received a new teletext packets
    /// </summary>
    /// <param name="data">teletext data</param>
    /// <returns></returns>
    public int OnTeletextReceived(IntPtr data, short packetCount)
    {
      try
      {
        for (int i = 0; i < packetCount; ++i)
        {
          IntPtr packetPtr = new IntPtr(data.ToInt32() + i * 188);
          ProcessPacket(packetPtr);
        }

      }
      catch (Exception ex)
      {
        Log.Log.WriteFile(ex.ToString());
      }
      return 0;
    }
    /// <summary>
    /// processes a single transport packet
    /// Called from BufferCB
    /// </summary>
    /// <param name="ptr">pointer to the transport packet</param>
    public void ProcessPacket(IntPtr ptr)
    {
      if (ptr == IntPtr.Zero) return;

      _packetHeader = _tsHelper.GetHeader((IntPtr)ptr);
      if (_packetHeader.SyncByte != 0x47)
      {
        Log.Log.Write("packet sync error");
        return;
      }
      if (_packetHeader.TransportError == true)
      {
        Log.Log.Write("packet transport error");
        return;
      }
      // teletext
      if (_grabTeletext)
      {
        if (_teletextDecoder != null)
        {
          _teletextDecoder.SaveData((IntPtr)ptr);
        }
      }
    }
    #endregion

    #region audio streams
    /// <summary>
    /// returns the list of available audio streams
    /// </summary>
    public List<IAudioStream> AvailableAudioStreams
    {
      get
      {
        if (!CheckThreadId()) return null;
        List<IAudioStream> streams = new List<IAudioStream>();
        foreach (PidInfo info in _channelInfo.pids)
        {
          if (info.isAC3Audio)
          {
            DVBAudioStream stream = new DVBAudioStream();
            stream.Language = info.language;
            stream.Pid = info.pid;
            stream.StreamType = AudioStreamType.AC3;
            streams.Add(stream);
          }
          else if (info.isAudio)
          {
            DVBAudioStream stream = new DVBAudioStream();
            stream.Language = info.language;
            stream.Pid = info.pid;
            stream.StreamType = AudioStreamType.Mpeg2;
            streams.Add(stream);
          }
        }
        return streams;
      }
    }

    /// <summary>
    /// get/set the current selected audio stream
    /// </summary>
    public IAudioStream CurrentAudioStream
    {
      get
      {
        return _currentAudioStream;
      }
      set
      {
        if (!CheckThreadId()) return;
        List<IAudioStream> streams = AvailableAudioStreams;
        DVBAudioStream audioStream = (DVBAudioStream)value;
        if (_filterTsAnalyzer != null)
        {
          ITsVideoAnalyzer writer = (ITsVideoAnalyzer)_filterTsAnalyzer;
          writer.SetAudioPid((short)audioStream.Pid);

          ITsTimeShift timeshift = _filterTsAnalyzer as ITsTimeShift;
          if (_currentAudioStream != null)
          {
            timeshift.RemovePesStream((short)_currentAudioStream.Pid);
          }
          timeshift.AddPesStream((short)audioStream.Pid, true, false);

          ITsRecorder recorder = _filterTsAnalyzer as ITsRecorder;
          if (_currentAudioStream != null)
          {
            recorder.RemovePesStream((short)_currentAudioStream.Pid);
          }
          recorder.AddPesStream((short)audioStream.Pid, true, false);
        }
        _currentAudioStream = audioStream;
        _pmtVersion = -1;
        SendPmtToCam();
      }
    }
    #endregion


    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    public string DevicePath
    {
      get
      {
        return _devicePath;
      }
    }
    /// <summary>
    /// gets the current filename used for timeshifting
    /// </summary>
    public string TimeShiftFileName
    {
      get
      {
        return _timeshiftFileName;
      }
    }

    /// <summary>
    /// returns true if card is currently grabbing the epg
    /// </summary>
    public bool IsEpgGrabbing
    {
      get
      {
        return _epgGrabbing;
      }
    }

    /// <summary>
    /// returns true if card is currently scanning
    /// </summary>
    public bool IsScanning
    {
      get
      {
        return _isScanning;
      }
      set
      {
        _isScanning = value;
        if (_isScanning)
        {
          _epgGrabbing = false;
          if (OnEpgReceived != null)
          {
            Log.Log.WriteFile("dvb:cancel epg->scanning");
            OnEpgReceived(this, null);
          }
        }
      }
    }

    /// <summary>
    /// returns the min/max channel numbers for analog cards
    /// </summary>
    public int MinChannel
    {
      get { return -1; }
    }
    /// <summary>
    /// Gets the max channel.
    /// </summary>
    /// <value>The max channel.</value>
    public int MaxChannel
    {
      get { return -1; }
    }

    /// <summary>
    /// returns the date/time when timeshifting has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when timeshifting was started</returns>
    public DateTime StartOfTimeShift
    {
      get
      {
        return _dateTimeShiftStarted;
      }
    }
    /// <summary>
    /// returns the date/time when recording has been started for the card specified
    /// </summary>
    /// <returns>DateTime containg the date/time when recording was started</returns>
    public DateTime RecordingStarted
    {
      get
      {
        return _dateRecordingStarted;
      }
    }

    #region IPMTCallback Members

    /// <summary>
    /// Returns true when unscrambled audio/video is received otherwise false
    /// </summary>
    /// <returns>true of false</returns>
    public bool IsReceivingAudioVideo
    {
      get
      {
        if (_graphRunning == false) return false;
        if (_filterTsAnalyzer == null) return false;
        if (_currentChannel == null) return false;
        ITsVideoAnalyzer writer = (ITsVideoAnalyzer)_filterTsAnalyzer;
        short audioEncrypted = 0;
        short videoEncrypted = 0;
        writer.IsAudioEncrypted(out audioEncrypted);
        if (_currentChannel.IsTv)
        {
          writer.IsVideoEncrypted(out videoEncrypted);
        }
        return ((audioEncrypted == 0) && (videoEncrypted == 0));
      }
    }

    #region IPMTCallback interface
    /// <summary>
    /// Called when tswriter.ax has received a new pmt
    /// </summary>
    /// <returns></returns>
    public int OnPMTReceived()
    {
      try
      {
        Log.Log.WriteFile("dvb:OnPMTReceived()");
        if (_graphRunning == false) return 0;
#if FORM
      _newPMT=true;
#else
        if (SendPmtToCam())
        {
          SetMpegPidMapping(_channelInfo);
        }
#endif
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      return 0;
    }
    #endregion

    /// <summary>
    /// Sends the PMT to cam.
    /// </summary>
    protected bool SendPmtToCam()
    {
      lock (this)
      {
        DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
        if (channel == null) return false;
        IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
        try
        {
          int pmtLength = _interfacePmtGrabber.GetPMTData(pmtMem);
          if (pmtLength != -1)
          {
            byte[] pmt = new byte[pmtLength];
            int version = -1;
            Marshal.Copy(pmtMem, pmt, 0, pmtLength);
            version = ((pmt[5] >> 1) & 0x1F);
            int pmtProgramNumber = (pmt[3] << 8) + pmt[4];
            if (pmtProgramNumber == channel.ServiceId)
            {
              if (_pmtVersion != version)
              {
                _channelInfo = new ChannelInfo();
                _channelInfo.DecodePmt(pmt);
                _channelInfo.network_pmt_PID = channel.PmtPid;
                _channelInfo.pcr_pid = channel.PcrPid;

                Log.Log.WriteFile("dvb:SendPMT version:{0} len:{1}", version, pmtLength);
                if (_conditionalAccess != null)
                {
                  int audioPid = -1;
                  if (_currentAudioStream != null)
                  {
                    audioPid = _currentAudioStream.Pid;
                  }

                  if (_conditionalAccess.SendPMT((DVBBaseChannel)Channel, pmt, pmtLength, audioPid) == false)
                  {
                    return true;
                  }
                }
                _pmtVersion = version;
                return true;
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Log.Write(ex);
        }
        finally
        {
          Marshal.FreeCoTaskMem(pmtMem);
        }
      }
      return false;
    }
    #endregion
  }
}
