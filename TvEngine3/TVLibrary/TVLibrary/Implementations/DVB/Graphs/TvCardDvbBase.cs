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
using TvLibrary.Channels;
using TvLibrary.Implementations.DVB.Structures;
using TvLibrary.Epg;
using TvLibrary.Teletext;
using TvLibrary.Log;
using TvLibrary.Helper;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// base class for DVB cards
  /// </summary>
  public class TvCardDvbBase : IDisposable, ITeletextCallBack, IPMTCallback, ICACallback
  {
    #region enums
    /// <summary>
    /// Different states of the card
    /// </summary>
    protected enum GraphState
    {
      /// <summary>
      /// Card is idle
      /// </summary>
      Idle,
      /// <summary>
      /// Card is idle, but graph is created
      /// </summary>
      Created,
      /// <summary>
      /// Card is timeshifting
      /// </summary>
      TimeShifting,
      /// <summary>
      /// Card is recording
      /// </summary>
      Recording
    }
    #endregion

    #region MDAPI structs

    #region Struct MDPlug
    // ********************* MDPlug *************************
    [StructLayout(LayoutKind.Sequential)]
    public struct CA_System82
    {
      public ushort CA_Typ;
      public ushort ECM;
      public ushort EMM;
      public uint Provider_Id;
    }
    // ********************* MDPlug *************************

    #endregion

    [StructLayout(LayoutKind.Sequential)]
    public struct TProgram82
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public byte[] Name;   // to simulate c++ char Name[30]
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public byte[] Provider;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
      public byte[] Country;
      public uint Freq;
      public byte PType;
      public byte Voltage;
      public byte Afc;
      public byte DiSEqC;
      public uint Symbolrate;
      public byte Qam;
      public byte Fec;
      public byte Norm;
      public ushort Tp_id;
      public ushort Video_pid;
      public ushort Audio_pid;
      public ushort TeleText_pid;          // Teletext PID 
      public ushort PMT_pid;
      public ushort PCR_pid;
      public ushort ECM_PID;
      public ushort SID_pid;
      public ushort AC3_pid;
      public byte TVType;           //  == 00 PAL ; 11 == NTSC    
      public byte ServiceTyp;
      public byte CA_ID;
      public ushort Temp_Audio;
      public ushort FilterNr;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public byte[] Filters;  // to simulate struct PIDFilters Filters[MAX_PID_IDS];
      public ushort CA_Nr;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
      public CA_System82[] CA_System82;  // to simulate struct TCA_System CA_System[MAX_CA_SYSTEMS];
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
      public byte[] CA_Country;
      public byte Marker;
      public ushort Link_TP;
      public ushort Link_SID;
      public byte PDynamic;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public byte[] Extern_Buffer;
    }

    #endregion

    #region constants

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    class MpTsAnalyzer { }

    [ComImport, Guid("BC650178-0DE4-47DF-AF50-BBD9C7AEF5A9")]
    class CyberLinkMuxer { }

    [ComImport, Guid("7F2BBEAF-E11C-4D39-90E8-938FB5A86045")]
    class PowerDirectorMuxer { }

    [ComImport, Guid("3E8868CB-5FE8-402C-AA90-CB1AC6AE3240")]
    class CyberLinkDumpFilter { };

    [ComImport, Guid("72E6DB8F-9F33-4D1C-A37C-DE8148C0BE74")]
    protected class MDAPIFilter { };

    #endregion

    #region interfaces
    [ComVisible(true), ComImport,
    Guid("C3F5AA0D-C475-401B-8FC9-E33FB749CD85"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IChangeChannel
    {
      /// <summary>
      /// Get the file name of media file.
      /// </summary>
      /// <param name="fn">The file name buffer.</param>
      /// <returns></returns>
      /// <remarks>fn should point to a buffer allocated to at least the length of MAX_PATH (=260)</remarks>
      [PreserveSig]
      int ChangeChannel(int frequency, int bandwidth, int polarity, int videopid, int audiopid, int ecmpid, int caid, int providerid);
      int ChangeChannelTP82([In] IntPtr tp82);
      //int ChangeChannel(IntPtr tp82);
    }
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
    protected IBaseFilter _infTeeSecond = null;
    protected IBaseFilter _filterTuner = null;
    protected IBaseFilter _filterCapture = null;
    protected IBaseFilter _filterTIF = null;
    //protected IBaseFilter _filterSectionsAndTables = null;

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
    protected ITsCaGrabber _interfaceCaGrabber;

    protected bool _epgGrabbing = false;
    protected bool _isScanning = false;
    string _timeshiftFileName = "";
    protected GraphState _graphState = GraphState.Idle;
    protected bool _startTimeShifting = false;
    protected bool _startRecording = false;
    protected bool _recordTransportStream = false;
    protected DVBAudioStream _currentAudioStream;
    protected BaseEpgGrabber _epgGrabberCallback = null;
    CamType _camType;
    protected IVbiCallback _teletextCallback = null;
    protected IBaseFilter _mdapiFilter = null;
    protected IChangeChannel _changeChannel = null;
    protected TProgram82 _mDPlugTProg82 = new TProgram82();
    protected object m_context;



    #region teletext
    protected bool _grabTeletext = false;
    protected bool _hasTeletext = false;
    TSHelperTools.TSHeader _packetHeader;
    TSHelperTools _tsHelper;
    protected DateTime _dateTimeShiftStarted = DateTime.MinValue;
    protected DateTime _dateRecordingStarted = DateTime.MinValue;
    #endregion

    protected bool _newPMT = false;
    protected bool _newCA = false;
#if FORM
    protected System.Windows.Forms.Timer _pmtTimer = new System.Windows.Forms.Timer();
#else
    protected System.Timers.Timer _pmtTimer = new System.Timers.Timer();
#endif
    bool _pmtTimerRentrant = false;
    #endregion

    #region graph building

    /// <summary>
    /// Initializes a new instance of the <see cref="TvCardDvbBase"/> class.
    /// </summary>
    public TvCardDvbBase()
    {
      _mDPlugTProg82.CA_Country = new byte[5];
      _mDPlugTProg82.CA_System82 = new CA_System82[32];
      _mDPlugTProg82.Country = new byte[30];
      _mDPlugTProg82.Extern_Buffer = new byte[16];
      _mDPlugTProg82.Filters = new byte[256];
      _mDPlugTProg82.Name = new byte[30];
      _mDPlugTProg82.Provider = new byte[30];

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
      /* if (_managedThreadId != System.Threading.Thread.CurrentThread.ManagedThreadId)
      {

        Log.Log.WriteFile("dvb:Invalid thread id {0}!={1}", _managedThreadId, System.Threading.Thread.CurrentThread.ManagedThreadId);
        return true;
        //return false;
      }*/
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

      if (_graphState == GraphState.TimeShifting)
      {
        if (_filterTsAnalyzer != null)
        {
          ITsTimeShift timeshift = _filterTsAnalyzer as ITsTimeShift;
          if (timeshift != null)
          {
            timeshift.Pause(1);
          }
        }
      }
      Log.Log.WriteFile("dvb:SubmitTuneRequest");
      _startTimeShifting = false;
      _startRecording = false;
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
      ArrayList pids = new ArrayList();
      pids.Add((ushort)0x0);//pat
      pids.Add((ushort)0x11);//sdt
      pids.Add((ushort)0x1fff);//padding stream
      if (_currentChannel != null)
      {
        DVBBaseChannel ch = (DVBBaseChannel)_currentChannel;
        if (ch.PmtPid > 0)
        {
          pids.Add((ushort)ch.PmtPid);//sdt
        }
      }
      SendHwPids(pids);

      _pmtVersion = -1;
      _newPMT = false;
      _newCA = false;
    }


    void SetTimeShiftPids()
    {
      if (_channelInfo == null) return;
      if (_channelInfo.pids.Count == 0) return;
      if (_currentChannel == null) return;
      //if (_currentAudioStream == null) return;
      DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
      if (dvbChannel == null) return;

      ITsTimeShift timeshift = _filterTsAnalyzer as ITsTimeShift;
      timeshift.Pause(1);
      timeshift.SetPcrPid((short)dvbChannel.PcrPid);
      timeshift.SetPmtPid((short)dvbChannel.PmtPid);
      foreach (PidInfo info in _channelInfo.pids)
      {
        if (info.isAC3Audio || info.isAudio || info.isVideo || info.isDVBSubtitle)
        {
          Log.Log.WriteFile("dvb: set timeshift {0}:{1}", info.stream_type, info);
          timeshift.AddStream((short)info.pid, (short)info.stream_type, info.language);
        }
      }
      timeshift.Pause(0);
    }

    void SetRecorderPids()
    {
      if (_channelInfo == null) return;
      if (_channelInfo.pids.Count == 0) return;
      if (_currentChannel == null) return;
      if (_currentAudioStream == null) return;
      DVBBaseChannel dvbChannel = _currentChannel as DVBBaseChannel;
      if (dvbChannel == null) return;

      ITsRecorder recorder = _filterTsAnalyzer as ITsRecorder;
      recorder.SetPcrPid((short)dvbChannel.PcrPid);
      bool programStream = true;
      bool audioPidSet = false;
      foreach (PidInfo info in _channelInfo.pids)
      {
        if (info.isAC3Audio || info.isAudio || info.isVideo)
        {
          bool addPid = false;
          if (info.isVideo)
          {
            addPid = true;
            if (info.IsMpeg4Video || info.IsH264Video)
            {
              programStream = false;
            }
          }
          if (info.isAudio || info.isAC3Audio)
          {
            if (audioPidSet == false)
            {
              addPid = true;
              audioPidSet = true;
            }
          }

          if (addPid)
          {
            Log.Log.WriteFile("dvb: set record {0}", info);
            recorder.AddStream((short)info.pid, (info.isAC3Audio || info.isAudio), info.isVideo);
          }
        }
      }
      if (programStream == false || _recordTransportStream)
      {
        recorder.AddStream((short)0x11, false, false);//sdt
        recorder.AddStream((short)dvbChannel.PmtPid, false, false);
        recorder.SetMode(TimeShiftingMode.TransportStream);
        Log.Log.WriteFile("dvb: record transport stream mode");
      }
      else
      {
        recorder.SetMode(TimeShiftingMode.ProgramStream);
        Log.Log.WriteFile("dvb: record program stream mode");
      }
    }

    /// <summary>
    /// returns true if we timeshift in transport stream mode
    /// false we timeshift in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsTimeshiftingTransportStream
    {
      get
      {
        return true;
      }
    }
    /// <summary>
    /// returns true if we record in transport stream mode
    /// false we record in program stream mode
    /// </summary>
    /// <value>true for transport stream, false for program stream.</value>
    public bool IsRecordingTransportStream
    {
      get
      {
        if (_channelInfo == null) return false;
        foreach (PidInfo info in _channelInfo.pids)
        {
          if (info.isVideo)
          {
            if (info.IsH264Video || info.IsMpeg4Video) return true;
          }
        }
        return false;
      }
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
      //int hr;
      if (_filterTsAnalyzer != null)
      {
        ITsTimeShift timeshift = _filterTsAnalyzer as ITsTimeShift;
        timeshift.SetTimeShiftingFileName(fileName);
        timeshift.SetMode(TimeShiftingMode.TransportStream);
        if (_channelInfo.pids.Count == 0)
        {
          Log.Log.WriteFile("dvb:SetTimeShiftFileName no pmt received yet");
          _startTimeShifting = true;
        }
        else
        {
          Log.Log.WriteFile("dvb:SetTimeShiftFileName fill in pids");
          _startTimeShifting = false;
          SetTimeShiftPids();
          timeshift.Start();
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
      int nodeTypeCount = 0;
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
        int numberOfInterfaces = 32;
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
        //_pmtVersion = -1;
        DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
        if (channel != null)
        {
          SetupPmtGrabber(channel.PmtPid);
        }
        return;
      }
      Log.Log.WriteFile("dvb:RunGraph");
      _teletextDecoder.ClearBuffer();
      _pmtVersion = -1;
      _newPMT = false;
      _newCA = false;

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
        SetupPmtGrabber(dvbChannel.PmtPid);
      }
      _pmtTimer.Enabled = true;
      _graphRunning = true;

    }

    /// <summary>
    /// Methods which stops the graph
    /// </summary>
    public void StopGraph()
    {
      if (!CheckThreadId()) return;
      if (_epgGrabbing)
      {
        _epgGrabbing = false;
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          Log.Log.Epg("dvb:cancel epg->stop graph");
          _epgGrabberCallback.OnEpgCancelled();
        }
      }
      m_context = null;
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
      _startRecording = false; 
      _pmtVersion = -1;
      _newPMT = false;
      _newCA = false;
      _recordingFileName = "";
      _channelInfo = new ChannelInfo();
      _currentChannel = null;
      m_context = null;
      _recordTransportStream = false;

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
        Log.Log.Error("dvb:RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to stop graph");
      }
      _graphState = GraphState.Created;
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
        Log.Log.Error("dvb:This application doesn't support this Tuning Space");
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
      _rotEntry = new DsROTEntry(_graphBuilder);

      Log.Log.WriteFile("dvb: find bda tuner");
      // Enumerate BDA Source filters category and found one that can connect to the network provider
      devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);
      for (int i = 0; i < devices.Length; i++)
      {
        IBaseFilter tmp;
        Log.Log.WriteFile("dvb:  -{0}", devices[i].Name);
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
        Log.Log.Error("dvb:No TvTuner installed");
        throw new TvException("No TvTuner installed");
      }
      bool skipCaptureFilter = false;
      IPin pinOut = DsFindPin.ByDirection(_filterTuner, PinDirection.Output, 0);
      if (pinOut != null)
      {
        IEnumMediaTypes enumMedia;
        int fetched;
        pinOut.EnumMediaTypes(out enumMedia);
        if (enumMedia != null)
        {
          AMMediaType[] mediaTypes = new AMMediaType[21];
          enumMedia.Next(20, mediaTypes, out fetched);
          if (fetched > 0)
          {
            for (int i = 0; i < fetched; ++i)
            {
              //Log.Log.Write("{0}", i);
              //Log.Log.Write(" major :{0} {1}", mediaTypes[i].majorType, (mediaTypes[i].majorType==MediaType.Stream));
              //Log.Log.Write(" sub   :{0} {1}", mediaTypes[i].subType, (mediaTypes[i].subType == MediaSubType.Mpeg2Transport ) );
              //Log.Log.Write(" format:{0} {1}", mediaTypes[i].formatType, (mediaTypes[i].formatType != FormatType.None) );
              if (mediaTypes[i].majorType == MediaType.Stream && mediaTypes[i].subType == MediaSubType.Mpeg2Transport && mediaTypes[i].formatType != FormatType.None)
              {
                skipCaptureFilter = false;
              }
              if (mediaTypes[i].majorType == MediaType.Stream && mediaTypes[i].subType == MediaSubType.BdaMpeg2Transport && mediaTypes[i].formatType == FormatType.None)
              {
                skipCaptureFilter = true;
              }
            }
          }
        }
      }
      if (false == skipCaptureFilter)
      {
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
          Log.Log.WriteFile("dvb:  -{0}", devices[i].Name);
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
              Log.Log.Error("dvb:  Render->main inftee demux failed");
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
      }
      if (_filterCapture == null)
      {
        Log.Log.WriteFile("dvb:  No TvCapture device found....");
        IPin pinIn = DsFindPin.ByDirection(_infTeeMain, PinDirection.Input, 0);
        //IPin pinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Input, 0);
        pinOut = DsFindPin.ByDirection(_filterTuner, PinDirection.Output, 0);
        hr = _graphBuilder.Connect(pinOut, pinIn);
        if (hr == 0)
        {
          Release.ComObject("inftee main pin in", pinIn);
          Release.ComObject("tuner pin out", pinOut);
          Log.Log.WriteFile("dvb:  using only tv tuner ifilter...");
          ConnectMpeg2DemuxToInfTee();
          AddTsAnalyzerToGraph();
          _conditionalAccess = new ConditionalAccess(_filterTuner, _filterTsAnalyzer);
          return;
        }
        Release.ComObject("tuner pin out", pinOut);
        Release.ComObject("inftee main pin in", pinIn);
        Log.Log.Error("dvb:  unable to use single tv tuner filter...");
        throw new TvException("No Tv Receiver filter found");
      }
      ConnectMpeg2DemuxToInfTee();
      AddTsAnalyzerToGraph();
      _conditionalAccess = new ConditionalAccess(_filterTuner, _filterTsAnalyzer);
    }

    #region IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
      Decompose();
    }
    #endregion



    /// <summary>
    /// adds the mpeg-2 demultiplexer filter and inftee filter to the graph
    /// </summary>
    protected void AddMpeg2DemuxerToGraph()
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
        Log.Log.Error("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
    }
    /// <summary>
    /// Connects the mpeg2 demuxers to the inf tee filter.
    /// </summary>
    protected void ConnectMpeg2DemuxToInfTee()
    {
      //multi demux
      int hr;
      if (System.IO.Directory.Exists("MDPLUGINS"))
      {
        Log.Log.WriteFile("dvb:add 2nd Inf Tee filter");
        _infTeeSecond = (IBaseFilter)new InfTee();
        hr = _graphBuilder.AddFilter(_infTeeSecond, "Inf Tee 2");
        if (hr != 0)
        {
          Log.Log.Error("dvb:Add 2nd InfTee returns:0x{0:X}", hr);
          throw new TvException("Unable to add  _infTeeSecond");
        }
        //capture -> maintee -> mdapi -> secondtee-> demux
        Log.Log.Info("dvb: add mdapi filter");
        _mdapiFilter = (IBaseFilter)new MDAPIFilter();
        hr = _graphBuilder.AddFilter(_mdapiFilter, "MDApi");

        Log.Log.Info("dvb: connect maintee->mdapi");
        IPin mainTeeOut = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 0);
        IPin mdApiIn = DsFindPin.ByDirection(_mdapiFilter, PinDirection.Input, 0);
        hr = _graphBuilder.Connect(mainTeeOut, mdApiIn);
        if (hr != 0)
        {
          Log.Log.Info("unable to connect maintee->mdapi");
        }
        Log.Log.Info("dvb: connect mdapi->2nd tee");
        IPin mdApiOut = DsFindPin.ByDirection(_mdapiFilter, PinDirection.Output, 0);
        IPin secondTeeIn = DsFindPin.ByDirection(_infTeeSecond, PinDirection.Input, 0);
        hr = _graphBuilder.Connect(mdApiOut, secondTeeIn);
        if (hr != 0)
        {
          Log.Log.Info("unable to connect mdapi->2nd tee");
        }

        //connect the 2nd inftee main -> TIF MPEG2 Demultiplexer
        Log.Log.WriteFile("dvb:connect mpeg-2 demuxer to 2ndtee");
        mainTeeOut = DsFindPin.ByDirection(_infTeeSecond, PinDirection.Output, 0);
        IPin demuxPinIn = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Input, 0);
        hr = _graphBuilder.Connect(mainTeeOut, demuxPinIn);
        Release.ComObject("maintee pin0", mainTeeOut);
        Release.ComObject("tifdemux pinin", demuxPinIn);
        if (hr != 0)
        {
          Log.Log.Error("dvb:Add main InfTee returns:0x{0:X}", hr);
          throw new TvException("Unable to add  mainInfTee");
        }
        _changeChannel = (IChangeChannel)_mdapiFilter;
      }
      else
      {
        //connect the inftee main -> TIF MPEG2 Demultiplexer
        Log.Log.WriteFile("dvb:connect mpeg-2 demuxer to maintee");
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
    protected void AddTsAnalyzerToGraph()
    {

      if (_filterTsAnalyzer == null)
      {
        Log.Log.WriteFile("dvb:Add Mediaportal Ts Analyzer filter");
        _filterTsAnalyzer = (IBaseFilter)new MpTsAnalyzer();
        int hr = _graphBuilder.AddFilter(_filterTsAnalyzer, "MediaPortal Ts Analyzer");
        if (hr != 0)
        {
          Log.Log.Error("dvb:Add main Ts Analyzer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Ts Analyzer filter");
        }
        IBaseFilter tee = _infTeeMain;
        if (_infTeeSecond != null)
          tee = _infTeeSecond;
        IPin pinTee = DsFindPin.ByDirection(tee, PinDirection.Output, 1);
        if (pinTee == null)
        {
          if (hr != 0)
          {
            Log.Log.Error("dvb:unable to find pin#2 on inftee filter");
            throw new TvException("unable to find pin#2 on inftee filter");
          }
        }
        IPin pin = DsFindPin.ByDirection(_filterTsAnalyzer, PinDirection.Input, 0);
        if (pin == null)
        {
          if (hr != 0)
          {
            Log.Log.Error("dvb:unable to find pin on ts analyzer filter");
            throw new TvException("unable to find pin on ts analyzer filter");
          }
        }
        hr = _graphBuilder.Connect(pinTee, pin);
        Release.ComObject("pinTsWriterIn", pin);
        if (hr != 0)
        {
          Log.Log.Error("dvb:unable to connect inftee to analyzer filter :0x{0:X}", hr);
          throw new TvException("unable to connect inftee to analyzer filter");
        }

        _interfaceChannelScan = (ITsChannelScan)_filterTsAnalyzer;
        _interfaceEpgGrabber = (ITsEpgScanner)_filterTsAnalyzer;
        _interfacePmtGrabber = (ITsPmtGrabber)_filterTsAnalyzer;
        _interfaceCaGrabber = (ITsCaGrabber)_filterTsAnalyzer;
      }
    }



    /// <summary>
    /// adds the BDA Transport Information Filter  and the
    /// MPEG-2 sections and tables filter to the graph 
    /// </summary>
    protected void AddBdaTransportFiltersToGraph()
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
              Log.Log.Error("    unable to add BDA MPEG2 Transport Information Filter filter:0x{0:X}", hr);
            }
          }
          catch (Exception)
          {
            Log.Log.Error("    unable to add BDA MPEG2 Transport Information Filter filter");
          }
          continue;
        }
        /*
        if (devices[i].Name.Equals("MPEG-2 Sections and Tables"))
        {
          Log.Log.Write("    add MPEG-2 Sections and Tables filter");
          try
          {
            hr = _graphBuilder.AddSourceFilterForMoniker(devices[i].Mon, null, devices[i].Name, out _filterSectionsAndTables);
            if (hr != 0)
            {
              Log.Log.Error("    unable to add MPEG-2 Sections and Tables filter:0x{0:X}", hr);
            }
          }
          catch (Exception)
          {
            Log.Log.Error("    unable to add MPEG-2 Sections and Tables filter");
          }
          continue;
        }*/
      }

      IPin pinInTif = DsFindPin.ByDirection(_filterTIF, PinDirection.Input, 0);
      //IPin pinInSec = DsFindPin.ByDirection(_filterSectionsAndTables, PinDirection.Input, 0);
      Log.Log.WriteFile("    pinTif:{0}", FilterGraphTools.LogPinInfo(pinInTif));
      //Log.Log.WriteFile("    pinSec:{0}", FilterGraphTools.LogPinInfo(pinInSec));
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
          Log.Log.WriteFile("dvb:try tif:{0}", FilterGraphTools.LogPinInfo(pins[0]));
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
            Log.Log.WriteFile("    tif not connected:0x{0:X}", hr);
          }
        }
        /*
        if (mpeg2SectionsConnected == false)
        {
          Log.Log.WriteFile("    try sections&tables:{0}", FilterGraphTools.LogPinInfo(pins[0]));
          hr = _graphBuilder.Connect(pins[0], pinInSec);
          if (hr == 0)
          {
            Log.Log.WriteFile("    mpeg 2 sections and tables connected");
            mpeg2SectionsConnected = true;
          }
          else
          {
            Log.Log.WriteFile("    dvb:mpeg 2 sections and tables not connected:0x{0:X}", hr);
          }
        }*/
        Release.ComObject("mpeg2 demux pin" + pinNr.ToString(), pins[0]);
      }
      Release.ComObject("IEnumMedia", enumPins);
      Release.ComObject("TIF pin in", pinInTif);
      // Release.ComObject("mpeg2 sections&tables pin in", pinInSec);
      if (tifConnected == false)
      {
        Log.Log.Error("    unable to connect transport information filter");
        //throw new TvException("unable to connect transport information filter");
      }
      // if (mpeg2SectionsConnected == false)
      // {
      // Log.Log.Error("    unable to connect mpeg 2 sections and tables filter");
      //throw new TvException("unable to connect mpeg 2 sections and tables filter");
      //}
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
        if (_epgGrabberCallback != null && _epgGrabbing)
        {
          Log.Log.Epg("dvb:cancel epg->decompose");
          _epgGrabberCallback.OnEpgCancelled();
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

      if (_mdapiFilter != null)
      {
        Release.ComObject("MDAPI filter", _mdapiFilter); _mdapiFilter = null;
      }
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

      if (_infTeeSecond != null)
      {
        Release.ComObject("_infTeeSecond filter", _infTeeSecond); _infTeeSecond = null;
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
      //if (_filterSectionsAndTables != null)
      //{
      //  Release.ComObject("secions&tables filter", _filterSectionsAndTables); _filterSectionsAndTables = null;
      //}
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
    /// Instructs the ts analyzer filter to start grabbing the PMT
    /// </summary>
    /// <param name="pmtPid">pid of the PMT</param>
    protected void SetupPmtGrabber(int pmtPid)
    {
      if (pmtPid < 0) return;
      if (!CheckThreadId()) return;


      if ((_currentChannel as ATSCChannel) != null)
      {
        ATSCChannel atscChannel = (ATSCChannel)_currentChannel;
        Log.Log.Write("SetAnalyzerMapping for atsc:{0}", atscChannel);
        _channelInfo = new ChannelInfo();
        _channelInfo.network_pmt_PID = atscChannel.PmtPid;
        _channelInfo.pcr_pid = atscChannel.PcrPid;
        PidInfo audioInfo = new PidInfo();
        PidInfo videoInfo = new PidInfo();
        audioInfo.Ac3Pid(atscChannel.AudioPid, "");
        videoInfo.VideoPid(atscChannel.VideoPid, 1);
        _channelInfo.AddPid(audioInfo);
        _channelInfo.AddPid(videoInfo);

        Log.Log.Write(" video:{0:X} audio:{1:X} pcr:{2:X} pmt:{3:X}", atscChannel.VideoPid, atscChannel.AudioPid, atscChannel.PcrPid, atscChannel.PmtPid);
        SetMpegPidMapping(_channelInfo);
      }
      else
      {
        Log.Log.Write("dvb: set pmt grabber pmt:{0:X}", pmtPid);
        _interfacePmtGrabber.SetCallBack(this);
        _interfacePmtGrabber.SetPmtPid(pmtPid);
        if (_mdapiFilter != null)
        {
          Log.Log.Write("dvb: set ca grabber ");
          _interfaceCaGrabber.SetCallBack(this);
          _interfaceCaGrabber.Reset();
        }
      }
    }


    /// <summary>
    /// maps the correct pids to the TsFileSink filter and teletext pins
    /// </summary>
    /// <param name="info"></param>
    protected void SetMpegPidMapping(ChannelInfo info)
    {
      if (info == null) return;
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
        Log.Log.WriteFile("  pid:{0:X} pcr", info.pcr_pid);
        Log.Log.WriteFile("  pid:{0:X} pmt", info.network_pmt_PID);

        if (info.pids != null)
        {
          foreach (PidInfo pidInfo in info.pids)
          {
            Log.Log.WriteFile("  {0}", pidInfo.ToString());
            if (pidInfo.pid == 0 || pidInfo.pid > 0x1fff) continue;
            if (pidInfo.isTeletext)
            {
              Log.Log.WriteFile("    map {0}", pidInfo);
              if (GrabTeletext)
              {
                ITsTeletextGrabber grabber = (ITsTeletextGrabber)_filterTsAnalyzer;
                grabber.SetTeletextPid((short)pidInfo.pid);
              }
              hwPids.Add((ushort)pidInfo.pid);
              _hasTeletext = true;
            }
            if (pidInfo.isAC3Audio || pidInfo.isAudio)
            {
              if (_currentAudioStream == null || pidInfo.isAC3Audio)
              {
                _currentAudioStream = new DVBAudioStream();
                _currentAudioStream.Pid = pidInfo.pid;
                _currentAudioStream.Language = pidInfo.language;
                if (pidInfo.IsMpeg1Audio)
                  _currentAudioStream.StreamType = AudioStreamType.Mpeg1;
                else if (pidInfo.IsMpeg3Audio)
                  _currentAudioStream.StreamType = AudioStreamType.Mpeg3;
                if (pidInfo.isAC3Audio)
                  _currentAudioStream.StreamType = AudioStreamType.AC3;
              }

              if (_currentAudioStream.Pid == pidInfo.pid)
              {
                Log.Log.WriteFile("    map {0}", pidInfo);
                writer.SetAudioPid((short)pidInfo.pid);
              }
              hwPids.Add((ushort)pidInfo.pid);
            }

            if (pidInfo.isVideo)
            {
              Log.Log.WriteFile("    map {0}", pidInfo);
              hwPids.Add((ushort)pidInfo.pid);
              writer.SetVideoPid((short)pidInfo.pid);
              if (info.pcr_pid > 0 && info.pcr_pid != pidInfo.pid)
              {
                hwPids.Add((ushort)info.pcr_pid);
              }
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
          _startTimeShifting = false;
          ITsTimeShift timeshift = _filterTsAnalyzer as ITsTimeShift;
          timeshift.Reset();
          SetTimeShiftPids();
          timeshift.Start();
        }
        if (_startRecording)
        {
          _startRecording = false;
          SetRecorderPids();

          ITsRecorder record = _filterTsAnalyzer as ITsRecorder;
          int hr = record.StartRecord();
          if (hr != 0)
          {
            Log.Log.Error("dvb:StartRecord failed:{0:X}", hr);
          }
          _dateRecordingStarted = DateTime.Now;
        }
        else if (_graphState == GraphState.TimeShifting || _graphState == GraphState.Recording)
        {
          SetTimeShiftPids();
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
    }
    #endregion

    #region signal quality, level etc

    /// <summary>
    /// Resets the signal update.
    /// </summary>
    public void ResetSignalUpdate()
    {
      _lastSignalUpdate = DateTime.MinValue;
    }
    /// <summary>
    /// updates the signal quality/level and tuner locked statusses
    /// </summary>
    protected virtual void UpdateSignalQuality()
    {
      TimeSpan ts = DateTime.Now - _lastSignalUpdate;
      if (ts.TotalMilliseconds < 5000) return;
      try
      {
        if (_graphRunning == false)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (Channel == null)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (_filterNetworkProvider == null)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          return;
        }
        if (!CheckThreadId())
        {
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
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          //          Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
          return;
        }
        if (_tunerStatistics.Count == 0)
        {
          _tunerLocked = false;
          _signalLevel = 0;
          _signalPresent = false;
          _signalQuality = 0;
          //          Log.Log.WriteFile("dvb:UpdateSignalPresent() no tuner stat interfaces");
          return;
        }
        bool isTunerLocked = false;
        bool isSignalPresent = false;
        long signalQuality = 0;
        long signalStrength = 0;

        //       Log.Log.Write("dvb:UpdateSignalQuality() count:{0}", _tunerStatistics.Count);
        for (int i = 0; i < _tunerStatistics.Count; i++)
        {
          IBDA_SignalStatistics stat = (IBDA_SignalStatistics)_tunerStatistics[i];
          bool isLocked = false;
          bool isPresent = false;
          int quality = 0;
          int strength = 0;
          //          Log.Log.Write("   dvb:  #{0} get locked",i );
          try
          {
            //is the tuner locked?
            stat.get_SignalLocked(out isLocked);
            isTunerLocked |= isLocked;
            //  Log.Log.Write("   dvb:  #{0} isTunerLocked:{1}", i,isLocked);
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalLocked() locked :{0}", ex);
          }
          catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalLocked() locked :{0}", ex);
          }

          //          Log.Log.Write("   dvb:  #{0} get signalpresent", i);
          try
          {
            //is a signal present?
            stat.get_SignalPresent(out isPresent);
            isSignalPresent |= isPresent;
            //  Log.Log.Write("   dvb:  #{0} isSignalPresent:{1}", i, isPresent);
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalPresent() locked :{0}", ex);
          }
          catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalPresent() locked :{0}", ex);
          }
          //          Log.Log.Write("   dvb:  #{0} get signalquality", i);
          try
          {
            //is a signal quality ok?
            stat.get_SignalQuality(out quality); //1-100
            if (quality > 0) signalQuality += quality;
            //   Log.Log.Write("   dvb:  #{0} signalQuality:{1}", i, quality);
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          //          Log.Log.Write("   dvb:  #{0} get signalstrength", i);
          try
          {
            //is a signal strength ok?
            stat.get_SignalStrength(out strength); //1-100
            if (strength > 0) signalStrength += strength;
            //    Log.Log.Write("   dvb:  #{0} signalStrength:{1}", i, strength);
          }
          catch (COMException)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          catch (Exception)
          {
            //            Log.Log.WriteFile("get_SignalQuality() locked :{0}", ex);
          }
          //Log.Log.WriteFile("  dvb:#{0}  locked:{1} present:{2} quality:{3} strength:{4}", i, isLocked, isPresent, quality, strength);
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
      }
      finally
      {
        _lastSignalUpdate = DateTime.Now;
      }
    }//public bool SignalPresent()
    #endregion

    #region properties

    public object Context 
    {
      get
      {
        return m_context;
      }
      set
      {
        m_context = value;
      }
    }
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


    /// <summary>
    /// Gets/sets the card device
    /// </summary>
    public virtual string DevicePath
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
      set
      {
        if (_epgGrabbing && value == false) _interfaceEpgGrabber.Reset();
        _epgGrabbing = value;
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
          if (_epgGrabberCallback != null && _epgGrabbing)
          {
            Log.Log.Epg("dvb:cancel epg->scanning");
            _epgGrabberCallback.OnEpgCancelled();
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



    /// <summary>
    /// Gets or sets the type of the cam.
    /// </summary>
    /// <value>The type of the cam.</value>
    public CamType CamType
    {
      get
      {
        return _camType;
      }
      set
      {
        _camType = value;
      }
    }

    /// <summary>
    /// Gets the interface for controlling the diseqc motor
    /// </summary>
    /// <value>Theinterface for controlling the diseqc motor.</value>
    public IDiSEqCMotor DiSEqCMotor
    {
      get
      {
        if (_conditionalAccess == null) return null;
        return _conditionalAccess.DiSEqCMotor;
      }
    }
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

    #endregion

    #region recording

    /// <summary>
    /// Starts recording
    /// </summary>
    /// <param name="recordingType">Recording type (content or reference)</param>
    /// <param name="fileName">filename to which to recording should be saved</param>
    /// <param name="startTime">time the recording should start (0=now)</param>
    /// <returns></returns>
    protected void StartRecord(bool transportStream,string fileName)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:StartRecord({0})", fileName);
      _recordTransportStream = transportStream;
      int hr;
      if (_filterTsAnalyzer != null)
      {
        ITsRecorder record = _filterTsAnalyzer as ITsRecorder;
        hr = record.SetRecordingFileName(fileName);
        if (hr != 0)
        {
          Log.Log.Error("dvb:SetRecordingFileName failed:{0:X}", hr);
        }
        if (_channelInfo.pids.Count == 0)
        {
          Log.Log.WriteFile("dvb:StartRecord no pmt received yet");
          _startRecording = true;
        }
        else
        {
          Log.Log.WriteFile("dvb:StartRecording...");
          SetRecorderPids();
          hr = record.StartRecord();
          if (hr != 0)
          {
            Log.Log.Error("dvb:StartRecord failed:{0:X}", hr);
          }
          _dateRecordingStarted = DateTime.Now;
        }
      }
    }


    /// <summary>
    /// Stop recording
    /// </summary>
    /// <returns></returns>
    protected void StopRecord()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:StopRecord()");

      if (_filterTsAnalyzer != null)
      {
        ITsRecorder record = _filterTsAnalyzer as ITsRecorder;
        record.StopRecord();

      }
      _startRecording = false;
      _recordTransportStream = false;
      _recordingFileName = "";
    }
    #endregion

    #region epg & scanning


    /// <summary>
    /// Start grabbing the epg
    /// </summary>
    public void GrabEpg(BaseEpgGrabber callback)
    {
      if (!CheckThreadId()) return;

      _epgGrabberCallback = callback;
      Log.Log.Write("dvb:grab epg...");
      _interfaceEpgGrabber.SetCallBack((IEpgCallback)callback);
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

          short titleCount;
          uint channelCount = 0;
          _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
          if (titleCount > 0)
            mhwReady = true;
          else
            mhwReady = false;
          _interfaceEpgGrabber.GetEPGChannelCount(out channelCount);
          if (channelCount > 0)
            dvbReady = true;
          else
            dvbReady = false;
          List<EpgChannel> epgChannels = new List<EpgChannel>();
          Log.Log.Epg("dvb:mhw ready MHW {0} titles found", titleCount);
          Log.Log.Epg("dvb:dvb ready.EPG {0} channels", channelCount);
          if (mhwReady)
          {
            _interfaceEpgGrabber.GetMHWTitleCount(out titleCount);
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
                if (dvbChan.NetworkId == networkid && dvbChan.TransportId == transportid && dvbChan.ServiceId == channelid)
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
                dvbChan.ServiceId = channelid;
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
            for (int i = 0; i < epgChannels.Count; ++i)
            {
              epgChannels[i].Sort();
            }
            return epgChannels;
          }

          if (dvbReady)
          {
            ushort networkid = 0;
            ushort transportid = 0;
            ushort serviceid = 0;
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
                if (year < 2000) continue;

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
    /// <param name="args"></param>
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
        if (_pmtTimerRentrant) return;
        _pmtTimerRentrant = true;

        if (_newPMT)
        {
          bool updatePids;
          if (SendPmtToCam(out updatePids))
          {
            _newPMT = false;
            if (updatePids)
            {
              if (_channelInfo != null)
              {
                SetMpegPidMapping(_channelInfo);
                SetChannel2MDPlug();
              }
              Log.Log.Info("dvb:stop tif");
              if (_filterTIF != null)
                _filterTIF.Stop();
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Log.WriteFile("dvb:{0}", ex.Message);
        Log.Log.WriteFile("dvb:{0}", ex.Source);
        Log.Log.WriteFile("dvb:{0}", ex.StackTrace);
      }
      finally
      {
        _pmtTimerRentrant = false;
      }
    }
    /// <summary>
    /// Sends the hw pids.
    /// </summary>
    /// <param name="pids">The pids.</param>
    public virtual void SendHwPids(ArrayList pids)
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
            if (info.IsMpeg1Audio)
              stream.StreamType = AudioStreamType.Mpeg1;
            else
              stream.StreamType = AudioStreamType.Mpeg3;
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
        }
        _currentAudioStream = audioStream;
        _pmtVersion = -1;
        bool updatePids;
        SendPmtToCam(out updatePids);
      }
    }
    #endregion

    #region tswriter callback handlers

    #region ITeletextCallBack Members

    public IVbiCallback TeletextCallback
    {
      get
      {
        return _teletextCallback;
      }
      set
      {
        _teletextCallback = value;
      }
    }
    /// <summary>
    /// callback from the TsWriter filter when it received a new teletext packets
    /// </summary>
    /// <param name="data">teletext data</param>
    /// <param name="packetCount">number of packets in data</param>
    /// <returns></returns>
    public int OnTeletextReceived(IntPtr data, short packetCount)
    {
      try
      {
        if (_teletextCallback != null)
        {
          _teletextCallback.OnVbiData(data, packetCount, false);
        }
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

    #region ICaCallback Members
    #region ICACallback Members

    /// <summary>
    /// Called when tswriter.ax has received a new ca section
    /// </summary>
    /// <returns></returns>
    public int OnCaReceived()
    {
      _newCA = true;
      Log.Log.WriteFile("dvb:OnCaReceived()");

      return 0;
    }

    #endregion
    #endregion

    #region IPMTCallback Members
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
        _newPMT = false;
        if (_graphRunning == false) return 0;
        bool updatePids;
        if (SendPmtToCam(out updatePids))
        {
          if (updatePids)
          {
            if (_channelInfo != null)
            {
              SetMpegPidMapping(_channelInfo);
              SetChannel2MDPlug();
            }
            Log.Log.Info("dvb:stop tif");
            if (_filterTIF != null)
              _filterTIF.Stop();
          }
        }
        else
        {
          _newPMT = true;
        }
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
    protected bool SendPmtToCam(out bool updatePids)
    {
      lock (this)
      {
        updatePids = false;
        if (_mdapiFilter != null)
        {
          if (_newCA == false) return false;//cat not received yet
        }
        if ((_currentChannel as ATSCChannel) != null) return true;
        DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
        if (channel == null) return true;
        IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
        IntPtr catMem = Marshal.AllocCoTaskMem(4096);// max. size for cat
        try
        {
          int pmtLength = _interfacePmtGrabber.GetPMTData(pmtMem);
          if (pmtLength > 6)
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

                if (_mdapiFilter != null)
                {
                  int catLength = _interfaceCaGrabber.GetCaData(catMem);
                  if (catLength > 0)
                  {
                    byte[] cat = new byte[catLength];
                    Marshal.Copy(catMem, cat, 0, catLength);
                    _channelInfo.DecodeCat(cat, catLength);
                  }
                }

                updatePids = true;
                Log.Log.WriteFile("dvb:SendPMT version:{0} len:{1} {2}", version, pmtLength, _channelInfo.caPMT.ProgramNumber);
                if (_conditionalAccess != null)
                {
                  int audioPid = -1;
                  if (_currentAudioStream != null)
                  {
                    audioPid = _currentAudioStream.Pid;
                  }

                  if (_conditionalAccess.SendPMT(_camType, (DVBBaseChannel)Channel, pmt, pmtLength, audioPid))
                  {
                    _pmtVersion = version;
                    Log.Log.WriteFile("dvb:cam flags:{0}", _conditionalAccess.IsCamReady());
                    _pmtTimer.Interval = 100;
                    return true;
                  }
                  else
                  {
                    //cam is not ready yet
                    Log.Log.WriteFile("dvb:SendPmt failed cam flags:{0}", _conditionalAccess.IsCamReady());
                    _pmtVersion = -1;
                    _pmtTimer.Interval = 3000;
                    return false;
                  }
                }
                _pmtTimer.Interval = 100;
                _pmtVersion = version;

                return true;
              }
              else
              {
                //already received this pmt
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
          Marshal.FreeCoTaskMem(catMem);
        }
      }
      return false;
    }
    #endregion
    #endregion

    #region MDAPI
    private void SetChannel2MDPlug()
    {

      int Index;
      int end_Index = 0;
      //is mdapi installed?
      if (_mdapiFilter == null) return; //nop, then return

      //did we already receive the pmt?
      if (_channelInfo == null) return; //nop, then return
      DVBSChannel dvbChannel = _currentChannel as DVBSChannel;
      if (dvbChannel == null) //not a DVB-S channel??
        return;

      if (_mDPlugTProg82.SID_pid == (ushort)dvbChannel.ServiceId)
        return; //already tuned to this service?

      //set channel name
      if (dvbChannel.Name != null)
      {
        end_Index = _mDPlugTProg82.Name.GetLength(0) - 1;

        if (dvbChannel.Name.Length < end_Index)
        {
          end_Index = dvbChannel.Name.Length;
        }
        for (Index = 0; Index < end_Index; ++Index)
        {
          _mDPlugTProg82.Name[Index] = (byte)dvbChannel.Name[Index];
        }
      }
      else
        end_Index = 0;
      _mDPlugTProg82.Name[end_Index] = 0;

      //set provide name
      if (dvbChannel.Provider != null)
      {
        end_Index = _mDPlugTProg82.Provider.GetLength(0) - 1;
        if (dvbChannel.Provider.Length < end_Index)
          end_Index = dvbChannel.Provider.Length;
        for (Index = 0; Index < end_Index; ++Index)
        {
          _mDPlugTProg82.Provider[Index] = (byte)dvbChannel.Provider[Index];
        }
      }
      else
        end_Index = 0;
      _mDPlugTProg82.Provider[end_Index] = 0;

      //public byte[] Country;
      _mDPlugTProg82.Freq = (uint)dvbChannel.Frequency;
      //public byte PType = (byte);
      _mDPlugTProg82.Afc = (byte)68;
      _mDPlugTProg82.DiSEqC = (byte)dvbChannel.DisEqc;
      _mDPlugTProg82.Symbolrate = (uint)dvbChannel.SymbolRate;
      //public byte Qam;

      _mDPlugTProg82.Fec = 0;
      //public byte Norm;
      _mDPlugTProg82.Tp_id = (ushort)dvbChannel.TransportId;
      _mDPlugTProg82.SID_pid = (ushort)dvbChannel.ServiceId;
      _mDPlugTProg82.PMT_pid = (ushort)dvbChannel.PmtPid;
      _mDPlugTProg82.PCR_pid = (ushort)dvbChannel.PcrPid;
      if (_channelInfo != null)
      {
        foreach (PidInfo pid in _channelInfo.pids)
        {
          if (pid.isVideo)
            _mDPlugTProg82.Video_pid = (ushort)pid.pid;
          if (pid.isAudio)
            _mDPlugTProg82.Audio_pid = (ushort)pid.pid;
          if (pid.isTeletext)
            _mDPlugTProg82.TeleText_pid = (ushort)pid.pid;
          if (pid.isAC3Audio)
            _mDPlugTProg82.AC3_pid = (ushort)pid.pid;
        }
        if (_currentChannel.IsTv)
          _mDPlugTProg82.ServiceTyp = (byte)1;
        else
          _mDPlugTProg82.ServiceTyp = (byte)2;
      }
      //public byte TVType;           //  == 00 PAL ; 11 == NTSC    
      //public ushort Temp_Audio;
      _mDPlugTProg82.FilterNr = (ushort)0; //to test
      //public byte[] Filters;  // to simulate struct PIDFilters Filters[MAX_PID_IDS];
      //public byte[] CA_Country;
      //public byte Marker;
      //public ushort Link_TP;
      //public ushort Link_SID;
      _mDPlugTProg82.PDynamic = (byte)0; //to test
      //public byte[] Extern_Buffer;
      if (_channelInfo.caPMT != null)
      {
        //get all EMM's (from CAT (pid 0x1))
        List<ECMEMM> emmList = _channelInfo.caPMT.GetEMM();
        if (emmList.Count <= 0) return;
        for (int i = 0; i < emmList.Count; ++i)
        {
          Log.Log.Info("EMM #{0} CA:0x{1:X} EMM:0x{2:X} ID:0x{3:X}",
                i, emmList[i].CaId, emmList[i].Pid, emmList[i].ProviderId);
        }

        //get all ECM's for this service
        List<ECMEMM> ecmList = _channelInfo.caPMT.GetECM();
        for (int i = 0; i < ecmList.Count; ++i)
        {
          Log.Log.Info("ECM #{0} CA:0x{1:X} ECM:0x{2:X} ID:0x{3:X}",
                i, ecmList[i].CaId, ecmList[i].Pid, ecmList[i].ProviderId);
        }


        _mDPlugTProg82.CA_Nr = (ushort)ecmList.Count;
        int count = 0;
        for (int x = 0; x < ecmList.Count; ++x)
        {
          _mDPlugTProg82.CA_System82[x].CA_Typ = (ushort)ecmList[x].CaId;
          _mDPlugTProg82.CA_System82[x].ECM = (ushort)ecmList[x].Pid;
          _mDPlugTProg82.CA_System82[x].EMM = 0;
          _mDPlugTProg82.CA_System82[x].Provider_Id = (uint)ecmList[x].ProviderId;
          count++;
        }

        for (int i = 0; i < emmList.Count; ++i)
        {
          bool found = false;
          for (int j = 0; j < count; ++j)
          {
            if (emmList[i].ProviderId == _mDPlugTProg82.CA_System82[j].Provider_Id && emmList[i].CaId == _mDPlugTProg82.CA_System82[j].CA_Typ)
            {
              found = true;
              _mDPlugTProg82.CA_System82[j].EMM = (ushort)emmList[i].Pid;
              break;
            }
          }
          if (!found)
          {
            _mDPlugTProg82.CA_System82[count].CA_Typ = (ushort)emmList[i].CaId;
            _mDPlugTProg82.CA_System82[count].ECM = 0;
            _mDPlugTProg82.CA_System82[count].EMM = (ushort)emmList[i].Pid;
            _mDPlugTProg82.CA_System82[count].Provider_Id = (uint)emmList[i].ProviderId;
            count++;
          }
        }

        _mDPlugTProg82.CA_ID = (byte)0;
        _mDPlugTProg82.CA_Nr = (ushort)count;
        _mDPlugTProg82.ECM_PID = _mDPlugTProg82.CA_System82[0].ECM;

        for (int i = 0; i < count; ++i)
        {
          Log.Log.Info("#{0} CA:0x{1:X} ECM:0x{2:X} EMM:0x{3:X}  provider:0x{4:X}",
                  i,
                  _mDPlugTProg82.CA_System82[i].CA_Typ,
                  _mDPlugTProg82.CA_System82[i].ECM,
                  _mDPlugTProg82.CA_System82[i].EMM,
                  _mDPlugTProg82.CA_System82[i].Provider_Id);
        }
      }
      //ca types:
      //0xb00 : conax
      //0x100 : seca
      //0x500 : Viaccess
      //0x622 : irdeto
      //0x1801: Nagravision
      /* C CINEMA PREMIERE 11856000 V 27500 1:1072:8206 video:165 audio:101 pcr:165 pmt:1285 emm:0 ecm:6664
       *      CA  ECM     EMM     ProviderId
              500 1a08     0      008100
              500 1a06     0      008300
              100 067e     0      0085
              100 067d     0      0084
              100 067c     0      0080
              ecm:6664 ServiceTyp: 1 Volt: 0 Typ: V AFC: 68 fec: 0 srate: 158572

        
        NED 1 12515000 H 22000 53:1105:4011 video:517 audio:88 pcr:8190 pmt:2111 emm:310 ecm:1383
              622  567   136      0
              100  643    B6      6a
              100  661    B6      6C
              d02    0  1389      0
              ecm:567 ServiceTyp: 1 Volt: 0 Typ: H AFC: 68 fec: 0 srate: 153072
                        0 1 2 3 4  5  6 7  8  9 10
              emm len:F 9 D 1 0 E0 B6 2 E0 B7 0 6A E0 B9 0 6C       
                100  b6  6a
                100  b6  6c
              
                
        PLAYBOYTV 10876000 V 22000 1:1060:30603 video:173 audio:132 pcr:173 pmt:1027 emm:193 ecm:1564
              100   61c    c1      4101
              1801  72E    c5         0
              1881  0      91         0
              1882  0      c6         0
              ecm:567 ServiceTyp: 1 Volt: 0 Typ: H AFC: 68 fec: 0 srate: 153072

       * 
      */
      IntPtr lparam = Marshal.AllocHGlobal(Marshal.SizeOf(_mDPlugTProg82));
      Marshal.StructureToPtr(_mDPlugTProg82, lparam, true);
      try
      {
        if (_changeChannel != null)
        {
          Log.Log.Info("Send channel change to MDAPI filter Ca_Id:0x{0:X} CA_Nr:{1:X} ECM_PID:{2:X}",
               _mDPlugTProg82.CA_ID,
               _mDPlugTProg82.CA_Nr,
               _mDPlugTProg82.ECM_PID);
          _changeChannel.ChangeChannelTP82(lparam);
        }
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
      }
      Marshal.FreeHGlobal(lparam);
    }
    #endregion
  }
}
