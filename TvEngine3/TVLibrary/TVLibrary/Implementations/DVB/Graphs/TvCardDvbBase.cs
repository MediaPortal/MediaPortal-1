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
  public class TvCardDvbBase : IDisposable, ISampleGrabberCB, IPMTCallback
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
    [ComImport, Guid("BAAC8911-1BA2-4ec2-96BA-6FFE42B62F72")]
    protected class MPStreamAnalyzer { }

    [ComImport, Guid("BC650178-0DE4-47DF-AF50-BBD9C7AEF5A9")]
    protected class CyberLinkMuxer { }

    [ComImport, Guid("7F2BBEAF-E11C-4D39-90E8-938FB5A86045")]
    protected class PowerDirectorMuxer { }

    [ComImport, Guid("3E8868CB-5FE8-402C-AA90-CB1AC6AE3240")]
    protected class CyberLinkDumpFilter { };
    [ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
    protected class StreamBufferConfig { }
    [ComImport, Guid("DB35F5ED-26B2-4A2A-92D3-852E145BF32D")]
    protected class MpFileWriter { }

    [ComImport, Guid("fc50bed6-fe38-42d3-b831-771690091a6e")]
    protected class TsWriter { }
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
    protected IBaseFilter _filterAnalyzer = null;
    protected StreamBufferSink _filterStreamBufferSink;
    protected IStreamBufferRecordControl _recorder;
    protected DsDevice _tunerDevice = null;
    protected DsDevice _captureDevice = null;
    protected IBaseFilter _tsFileSink = null;
    protected IBaseFilter _filterGrabber;
    protected DVBTeletext _teletextDecoder;
    protected IBaseFilter _filterMpegMuxerTimeShift;


    protected List<IBDA_SignalStatistics> _tunerStatistics = new List<IBDA_SignalStatistics>();
    protected bool _signalPresent;
    protected bool _tunerLocked;
    protected int _signalQuality;
    protected int _signalLevel;
    protected string _name;
    protected string _devicePath;
    protected string _fileName;
    protected IChannel _currentChannel;
    protected IPin _pinAnalyzerSI;
    protected IPin _pinAnalyzerD2;
    protected IPin _pinAnalyzerD3;
    protected IPin _pinAnalyzerEPG;
    protected IPin _pinTTX;
    protected IBaseFilter _filterTsWriter;
    protected IPin _pinAudioTimeShift;
    protected IPin _pinVideoTimeShift;

    protected int _pmtVersion;
    protected ChannelInfo _channelInfo;
    protected DateTime _lastSignalUpdate;
    protected bool _graphRunning = false;
    protected int _managedThreadId = -1;
    protected bool _isATSC = false;
    protected IEPGGrabber _dvbGrabber;
    protected IMHWGrabber _mhwGrabber;
    protected IStreamAnalyzer _streamAnalyzer;
    protected bool _epgGrabbing = false;
    protected bool _isScanning = false;
    string _timeshiftFileName = "";
    protected GraphState _graphState = GraphState.Idle;
    DVBAudioStream _currentAudioStream;


    #region teletext
    protected bool _grabTeletext = false;
    protected bool _hasTeletext = false;
    int _restBufferLen;
    byte[] _restBuffer;
    IntPtr _ptrRestBuffer;
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
      _restBufferLen = 0;
      _restBuffer = new byte[4096];
      _ptrRestBuffer = Marshal.AllocCoTaskMem(200);
      _packetHeader = new TSHelperTools.TSHeader();
      _tsHelper = new TSHelperTools();
      _channelInfo = new ChannelInfo();

      //create registry keys needed by the streambuffer engine for timeshifting/recording
      try
      {
        using (RegistryKey hkcu = Registry.CurrentUser)
        {
          RegistryKey newKey = hkcu.CreateSubKey(@"Software\MediaPortal");
          newKey.Close();
          using (RegistryKey hklm = Registry.LocalMachine)
          {
            newKey = hklm.CreateSubKey(@"Software\MediaPortal");
            newKey.Close();
          }
        }

      }
      catch (Exception) { }

    }
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
      _pmtTimer.Enabled = false;
      _hasTeletext = false;
      _currentAudioStream = null;
      
      //Log.Log.WriteFile("dvb:SubmitTuneRequest");

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
      if (_tsFileSink != null)
      {
        IMPRecord recorder = _tsFileSink as IMPRecord;
        recorder.Reset();
      }
      _pmtVersion = -1;
    }

    protected void CreateTimeShiftingGraph()
    {
      AddMpegMuxer(_currentChannel.IsTv);
      AddTsFileSink();
      ConnectSinkFilter();
    }

    protected void DeleteTimeShiftingGraph()
    {
      if (_tsFileSink != null)
      {
        IMPRecord record = _tsFileSink as IMPRecord;
        record.StopTimeShifting();
        _graphBuilder.RemoveFilter((IBaseFilter)_tsFileSink);
        Release.ComObject("tsFileSink filter", _tsFileSink);
        _tsFileSink = null;
      }

      if (_filterMpegMuxerTimeShift != null)
      {
        _graphBuilder.RemoveFilter(_filterMpegMuxerTimeShift);
        Release.ComObject("MPEG2 mux filter", _filterMpegMuxerTimeShift); _filterMpegMuxerTimeShift = null;
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
      int hr;
      if (_filterStreamBufferSink != null)
      {
        //Log.Log.WriteFile("dvb:SetTimeShiftFileName: uses dvr-ms");
        IStreamBufferSink init = (IStreamBufferSink)_filterStreamBufferSink;
        hr = init.LockProfile(fileName);
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:SetTimeShiftFileName  returns:0x{0:X}", hr);
          throw new TvException("Unable to start timeshifting to " + fileName);
        }
      }
      if (_tsFileSink != null)
      {
        IMPRecord record = _tsFileSink as IMPRecord;
        record.SetTimeShiftFileName(fileName);
        record.StartTimeShifting();


        /*ITsFileSink sink = _tsFileSink as ITsFileSink;
        if (sink != null)
        {
          sink.SetChunkReserve(256 * 1024 * 1024);
          sink.SetMaxTSFiles(100);
          sink.SetMaxTSFileSize(256 * 1024 * 1024);
          sink.SetMinTSFiles(4);
          sink.SetRegSettings();
        }
        Log.Log.WriteFile("dvb:SetTimeShiftFileName: uses .ts");
        IFileSinkFilter interfaceFile = _tsFileSink as IFileSinkFilter;

        //set filename
        AMMediaType mt = new AMMediaType();
        hr = interfaceFile.SetFileName(fileName, mt);
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:SetTimeShiftFileName  returns:0x{0:X}", hr);
          throw new TvException("Unable to start timeshifting to " + fileName);
        }*/
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
      if (state == FilterState.Running) return;

      Log.Log.WriteFile("dvb:RunGraph");
      _teletextDecoder.ClearBuffer();
      _pmtVersion = -1;

      int hr = 0;
      //hr=_graphBuilder.SetDefaultSyncSource();

      //Log.Log.WriteFile("SetSyncSource:{0:X}", hr);
      /*
      IReferenceClock clock;
      hr = _filterMpeg2DemuxTs.GetSyncSource(out clock);
      Log.Log.WriteFile("GetSyncSource from timeshifting mux:{0:X} {1}", hr, clock);
      
      IMediaFilter mediaFilter = _graphBuilder as IMediaFilter;
      hr = mediaFilter.SetSyncSource(clock);
      Log.Log.WriteFile("SetSyncSource:{0:X}", hr);
      */

      hr = (_graphBuilder as IMediaControl).Run();
      if (hr < 0 || hr > 1)
      {
        Log.Log.WriteFile("dvb:RunGraph returns:0x{0:X}", hr);
        throw new TvException("Unable to start graph");
      }

      _epgGrabbing = false;
      if (_tsFileSink != null)
      {
        _dateTimeShiftStarted = DateTime.Now;
      }
      DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
      SetAnalyzerMapping(channel.PmtPid);
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
      _pmtVersion = -1;
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
    /// adds the streambuffer sink filter to the graph
    /// </summary>
    protected void AddStreamBufferSink(string fileName)
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:dvb:AddStreamBufferSink");
      _filterStreamBufferSink = new StreamBufferSink();
      int hr = _graphBuilder.AddFilter((IBaseFilter)_filterStreamBufferSink, "SBE Sink");

      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:dvb:AddStreamBufferSink returns:0x{0:X}", hr);
        throw new TvException("Unable to add Stream buffer engine");
      }
      string directory = "";
      int slashPosition = fileName.LastIndexOf(@"\");
      if (slashPosition >= 0)
      {
        directory = fileName.Substring(0, slashPosition);
      }
      else
      {
        directory = System.IO.Path.GetFullPath(fileName);
        slashPosition = directory.LastIndexOf(@"\");
        if (slashPosition >= 0)
          directory = directory.Substring(0, slashPosition);
        else directory = "";
      }


      StreamBufferConfig streamConfig = new StreamBufferConfig();

      IStreamBufferInitialize streamBufferInit = streamConfig as IStreamBufferInitialize;
      if (streamBufferInit != null)
      {
        IntPtr subKey = IntPtr.Zero;
        IntPtr HKEY = (IntPtr)unchecked((int)0x80000002L);
        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr = streamBufferInit.SetHKEY(subKey);
        IntPtr[] sids = new IntPtr[2];
        sids[0] = SidHelper.GetSidPtr("Everyone");
        if (sids[0] != IntPtr.Zero)
        {
          Log.Log.WriteFile("dvb:analog:SetSID for everyone");
          hr = streamBufferInit.SetSIDs(1, sids);
          if (hr != 0)
          {
            Log.Log.WriteFile("dvb:analog:SetSIDs returns:{0:X}", hr);
          }
          Marshal.FreeHGlobal(sids[0]);
        }
      }
      else
      {
        Log.Log.WriteFile("dvb:analog:Unable to get IStreamBufferInitialize");
      }

      IStreamBufferConfigure streamBufferConfig = streamConfig as IStreamBufferConfigure;
      if (streamBufferConfig != null)
      {
        Log.Log.WriteFile("dvb:analog:SetBackingFileCount min=6, max=8");
        streamBufferConfig.SetBackingFileCount(6, 8);
        Log.Log.WriteFile("dvb:analog:SetDirectory:{0}", directory);
        if (directory != String.Empty)
        {
          hr = streamBufferConfig.SetDirectory(directory);
          if (hr != 0)
          {
            Log.Log.WriteFile("dvb:analog:FAILED to set timeshift folder to:{0} {1:X}", directory, hr);
          }
        }
      }
      else
      {
        Log.Log.WriteFile("dvb:analog:Unable to get IStreamBufferConfigure");
      }
    }

    /// <summary>
    /// adds the TsFileSink filter to the graph
    /// </summary>
    protected void AddTsFileSink()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:AddTsFileSink");
      _tsFileSink = (IBaseFilter)new MpFileWriter();
      int hr = _graphBuilder.AddFilter((IBaseFilter)_tsFileSink, "TsFileSink");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddTsFileSink returns:0x{0:X}", hr);
        throw new TvException("Unable to add TsFileSink");
      }

      IPin pin = DsFindPin.ByDirection(_filterMpegMuxerTimeShift, PinDirection.Output, 0);
      FilterGraphTools.ConnectPin(_graphBuilder, pin, (IBaseFilter)_tsFileSink, 0);
      Release.ComObject("mpegmux pinin", pin);

    }

    /// <summary>
    /// adds the Mediaportal Stream Analyzer filter to the graph
    /// </summary>
    protected void AddAnalyzerFilter()
    {
      if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:AddAnalyzerFilter");
      _filterAnalyzer = (IBaseFilter)new MPStreamAnalyzer();
      int hr = _graphBuilder.AddFilter(_filterAnalyzer, "Analyzer");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddAnalyzerFilter returns:0x{0:X}", hr);
        throw new TvException("Unable to add AddAnalyzerFilter");
      }

#if MULTI_DEMUX
      IMpeg2Demultiplexer demuxer = (IMpeg2Demultiplexer)_filterMpeg2DemuxAnalyzer;
#else
      IMpeg2Demultiplexer demuxer = (IMpeg2Demultiplexer)_filterMpeg2DemuxTif;
#endif
      AMMediaType mediaType = new AMMediaType();
      mediaType.majorType = MediaType.Mpeg2Sections;
      mediaType.subType = MediaSubType.None;
      mediaType.formatType = FormatType.None;

      demuxer.CreateOutputPin(mediaType, "SI", out _pinAnalyzerSI);
      demuxer.CreateOutputPin(mediaType, "D2", out _pinAnalyzerD2);
      demuxer.CreateOutputPin(mediaType, "D3", out _pinAnalyzerD3);
      demuxer.CreateOutputPin(mediaType, "EPG", out _pinAnalyzerEPG);
      FilterGraphTools.ConnectPin(_graphBuilder, _pinAnalyzerSI, _filterAnalyzer, 0);
      FilterGraphTools.ConnectPin(_graphBuilder, _pinAnalyzerD2, _filterAnalyzer, 1);
      FilterGraphTools.ConnectPin(_graphBuilder, _pinAnalyzerD3, _filterAnalyzer, 2);
      FilterGraphTools.ConnectPin(_graphBuilder, _pinAnalyzerEPG, _filterAnalyzer, 3);

      _streamAnalyzer = (IStreamAnalyzer)_filterAnalyzer;
      _dvbGrabber = (IEPGGrabber)_filterAnalyzer;
      _mhwGrabber = (IMHWGrabber)_filterAnalyzer;
    }

    /// <summary>
    /// adds the mpeg-2 demultiplexer filter to the graph
    /// </summary>
    protected void AddMpeg2DemuxerTif()
    {
      if (!CheckThreadId()) return;
      if (_filterMpeg2DemuxTif != null) return;
      Log.Log.WriteFile("dvb:AddMPEG2DemuxFilter");
      int hr = 0;

      _filterMpeg2DemuxTif = (IBaseFilter)new MPEG2Demultiplexer();

      hr = _graphBuilder.AddFilter(_filterMpeg2DemuxTif, "TIF MPEG2 Demultiplexer");
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
      _infTeeMain = (IBaseFilter)new InfTee();
      hr = _graphBuilder.AddFilter(_infTeeMain, "Main Inf Tee");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:Add main InfTee returns:0x{0:X}", hr);
        throw new TvException("Unable to add  mainInfTee");
      }
    }
    protected void ConnectMpeg2DemuxersToMainTee()
    {
      //multi demux

      //connect the inftee main -> TIF MPEG2 Demultiplexer
      Log.Log.WriteFile("dvb:connect mpeg 2 demuxers to maintee");
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

    protected void GetVideoAudioPins()
    {
      if (!CheckThreadId()) return;
      //multi demux

#if MULTI_DEMUX
      Log.Log.WriteFile("GetVideoAudioPins");
      IMpeg2Demultiplexer demuxer = (IMpeg2Demultiplexer)_filterMpeg2DemuxTs;
      if (_pinVideoTimeShift == null)
      {
        Log.Log.WriteFile("Create video pin");
        demuxer.CreateOutputPin(FilterGraphTools.GetVideoMpg2Media(), "Video", out _pinVideoTimeShift);
      }

      if (_pinAudioTimeShift == null)
      {
        Log.Log.WriteFile("Create audio pin");
        demuxer.CreateOutputPin(FilterGraphTools.GetAudioMpg2Media(), "Audio", out _pinAudioTimeShift);
      }
#else

      // Connect the 5 MPEG-2 Demux output pins
      for (int i = 0; i < 5; i++)
      {
        IPin pinOut = DsFindPin.ByDirection(_filterMpeg2DemuxTif, PinDirection.Output, i);
        if (pinOut != null)
        {
          IEnumMediaTypes enumMedia;
          pinOut.EnumMediaTypes(out enumMedia);
          bool releasePin = true;
          while (true)
          {
            int fetched;
            AMMediaType[] mediaTypes = new AMMediaType[2];
            enumMedia.Next(1, mediaTypes, out fetched);
            if (fetched != 1) break;
            if (mediaTypes[0].majorType == MediaType.Video)
            {
              releasePin = false;
              _pinVideoTimeShift = pinOut;
              Log.Log.WriteFile("dvb:videopin:{0}", i);
            }
            if (mediaTypes[0].majorType == MediaType.Audio)
            {
              releasePin = false;
              _pinAudioTimeShift = pinOut;
              Log.Log.WriteFile("dvb:audiopin:{0}", i);
            }
            DsUtils.FreeAMMediaType(mediaTypes[0]);
          }
          Release.ComObject("IEnumMedia mpeg2 demux", enumMedia);
          if (releasePin)
            Release.ComObject("mpeg2 demux pin:" + i.ToString(), pinOut);
        }
      }
#endif

      if (_filterTsWriter == null)
      {
        _filterTsWriter = (IBaseFilter)new TsWriter();
        _graphBuilder.AddFilter(_filterTsWriter, "TSWriter");
        IPin pinTee = DsFindPin.ByDirection(_infTeeMain, PinDirection.Output, 1);
        IPin pin = DsFindPin.ByDirection(_filterTsWriter, PinDirection.Input, 0);
        _graphBuilder.Connect(pinTee, pin);
        Release.ComObject("pinTsWriterIn", pin);
      }
    }

    /// <summary>
    /// adds the sample grabber filter to the graph
    /// </summary>
    protected void AddSampleGrabber()
    {
      if (!CheckThreadId()) return;

      Log.Log.WriteFile("dvb:AddSampleGrabber");
      _filterGrabber = (IBaseFilter)new SampleGrabber();
      int hr = _graphBuilder.AddFilter(_filterGrabber, "Sample Grabber");
      if (hr != 0)
      {
        Log.Log.WriteFile("dvb:AddSampleGrabber returns:0x{0:X}", hr);
        throw new TvException("Unable to add Add sample grabber");
      }
#if MULTI_DEMUX
      IMpeg2Demultiplexer demuxer = (IMpeg2Demultiplexer)_filterMpeg2DemuxAnalyzer;
#else
      IMpeg2Demultiplexer demuxer = (IMpeg2Demultiplexer)_filterMpeg2DemuxTif;
#endif
      AMMediaType mediaType = new AMMediaType();
      mediaType.majorType = MediaType.Mpeg2Sections;
      mediaType.subType = MediaSubType.None;
      mediaType.formatType = FormatType.None;
      demuxer.CreateOutputPin(mediaType, "TTX", out _pinTTX);
      FilterGraphTools.ConnectPin(_graphBuilder, _pinTTX, _filterGrabber, 0);

      AMMediaType mt = new AMMediaType();
      mt.majorType = MediaType.Stream;
      mt.subType = MediaSubType.Mpeg2Transport;
      ISampleGrabber sampleInterface = (ISampleGrabber)_filterGrabber;

      sampleInterface.SetCallback(this, 1);
      sampleInterface.SetMediaType(mt);
      sampleInterface.SetBufferSamples(false);
    }

    protected void AddMpegMuxer(bool isTv)
    {
      if (!CheckThreadId()) return;
      if (_filterMpegMuxerTimeShift != null) return;
      Log.Log.WriteFile("dvb:AddMpegMuxer()");
      try
      {
        string monikerPowerDirectorMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{7F2BBEAF-E11C-4D39-90E8-938FB5A86045}";
        string monikerPowerDvdMuxer = @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{6770E328-9B73-40C5-91E6-E2F321AEDE57}";
        _filterMpegMuxerTimeShift = Marshal.BindToMoniker(monikerPowerDirectorMuxer) as IBaseFilter;
        int hr = _graphBuilder.AddFilter(_filterMpegMuxerTimeShift, "TimeShift MPEG Muxer");
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:AddMpegMuxer returns:0x{0:X}", hr);
          throw new TvException("Unable to add Cyberlink MPEG Muxer");
        }
        if (isTv)
        {
          FilterGraphTools.ConnectPin(_graphBuilder, _pinVideoTimeShift, _filterMpegMuxerTimeShift, 0);
        }
        FilterGraphTools.ConnectPin(_graphBuilder, _pinAudioTimeShift, _filterMpegMuxerTimeShift, 1);
      }
      catch (Exception ex)
      {
        Log.Log.Write(ex);
        throw new TvException("Cyberlink MPEG Muxer filter (mpgmux.ax) not installed");
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
      _streamAnalyzer = null;
      _dvbGrabber = null;
      _mhwGrabber = null;
      if (_recorder != null)
      {
        Release.ComObject("recorder filter", _recorder); _recorder = null;
      }

      if (_filterStreamBufferSink != null)
      {
        Release.ComObject("StreamBufferSink filter", _filterStreamBufferSink); _filterStreamBufferSink = null;
      }
      if (_tsFileSink != null)
      {
        Release.ComObject("tsFileSink filter", _tsFileSink); _tsFileSink = null;
      }
      if (_filterGrabber != null)
      {
        Release.ComObject("Grabber filter", _filterGrabber); _filterGrabber = null;
      }

      if (_filterMpegMuxerTimeShift != null)
      {
        Release.ComObject("MPEG2 mux filter", _filterMpegMuxerTimeShift); _filterMpegMuxerTimeShift = null;
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
      if (_pinTTX != null)
      {
        Release.ComObject("TTX pin", _pinTTX); _pinTTX = null;
      }
      if (_filterTsWriter != null)
      {
        Release.ComObject("TSWriter filter", _filterTsWriter); _filterTsWriter = null;
      }

      if (_pinAnalyzerSI != null)
      {
        Release.ComObject("SI pin", _pinAnalyzerSI); _pinAnalyzerSI = null;
      }
      if (_pinAnalyzerD2 != null)
      {
        Release.ComObject("D2 pin", _pinAnalyzerD2); _pinAnalyzerD2 = null;
      }
      if (_pinAnalyzerD3 != null)
      {
        Release.ComObject("D3 pin", _pinAnalyzerD3); _pinAnalyzerD3 = null;
      }
      if (_pinAnalyzerEPG != null)
      {
        Release.ComObject("EPG pin", _pinAnalyzerEPG); _pinAnalyzerEPG = null;
      }
      if (_pinAudioTimeShift != null)
      {
        Release.ComObject("Audio pin", _pinAudioTimeShift); _pinAudioTimeShift = null;
      }
      if (_pinVideoTimeShift != null)
      {
        Release.ComObject("Video pin", _pinVideoTimeShift); _pinVideoTimeShift = null;
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
      if (!CheckThreadId()) return;
      //do grab epg, pat,sdt or nit when not timeshifting

      _streamAnalyzer.Scanning(0);
      DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
      if (_graphState == GraphState.TimeShifting || _graphState == GraphState.Recording)
      {
        Log.Log.WriteFile("dvb:SetAnalyzerMapping TS pid:{0:X} {1:X}", pmtPid, channel.ServiceId);
        SetupDemuxerPin(_pinAnalyzerEPG, 0x2000, (int)MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(_pinAnalyzerD2, 0x2000, (int)MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(_pinAnalyzerD3, 0x2000, (int)MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(_pinAnalyzerSI, pmtPid, (int)MediaSampleContent.Mpeg2PSI, true);
      }
      else
      {
        Log.Log.WriteFile("dvb:SetAnalyzerMapping scan/epg pid:{0:X} {1:X}", pmtPid, channel.ServiceId);
        SetupDemuxerPin(_pinAnalyzerEPG, 0x12, (int)MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(_pinAnalyzerD2, 0xd2, (int)MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(_pinAnalyzerD3, 0xd3, (int)MediaSampleContent.Mpeg2PSI, true);


        if (_isATSC)
        {
          StreamAnalyzer.UseATSC(1);
          if (pmtPid >= 0 && pmtPid <= 0x1ffb)
          {
            int[] pids = new int[5];
            pids[0] = 0x0;
            pids[1] = 0x10;
            pids[2] = 0x11;
            pids[3] = pmtPid;
            pids[4] = 0x1ffb;
            SetupDemuxerPids(_pinAnalyzerSI, pids, 5, (int)MediaSampleContent.Mpeg2PSI, true);
          }
          else
          {
            int[] pids = new int[4];
            pids[0] = 0x0;
            pids[1] = 0x10;
            pids[2] = 0x11;
            pids[3] = 0x1ffb;
            SetupDemuxerPids(_pinAnalyzerSI, pids, 4, (int)MediaSampleContent.Mpeg2PSI, true);
          }
        }
        else
        {
          if (pmtPid >= 0 && pmtPid <= 0x1ffb)
          {
            int[] pids = new int[4];
            pids[0] = 0x0;
            pids[1] = 0x10;
            pids[2] = 0x11;
            pids[3] = pmtPid;
            SetupDemuxerPids(_pinAnalyzerSI, pids, 4, (int)MediaSampleContent.Mpeg2PSI, true);
          }
          else
          {
            int[] pids = new int[3];
            pids[0] = 0x0;
            pids[1] = 0x10;
            pids[2] = 0x11;
            SetupDemuxerPids(_pinAnalyzerSI, pids, 3, (int)MediaSampleContent.Mpeg2PSI, true);
          }
        }
      }
      _streamAnalyzer.SetPMTCallback(this);
      _streamAnalyzer.ResetParser();
      _streamAnalyzer.ResetPids();
      if (_isATSC)
        _streamAnalyzer.UseATSC(1);
      else
        _streamAnalyzer.UseATSC(0);
      _streamAnalyzer.SetPMTProgramNumber(channel.ServiceId);

      _streamAnalyzer.Scanning(1);
      //Log.Log.WriteFile("dvb:SetAnalyzerMapping done");
    }


    /// <summary>
    /// maps the correct pids to the TsFileSink filter & teletext pins
    /// </summary>
    /// <param name="info"></param>
    protected void SetMpegPidMapping(ChannelInfo info)
    {
      //if (!CheckThreadId()) return;
      Log.Log.WriteFile("dvb:SetMpegPidMapping");

      ITsWriter writer = (ITsWriter)_filterTsWriter;
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
        if (pmtData.pid == 0 || pmtData.pid > 0x1fff) continue;
        if (pmtData.isTeletext)
        {
          Log.Log.WriteFile("    map teletext pid:0x{0:X}", pmtData.pid);
          SetupDemuxerPin(_pinTTX, pmtData.pid, (int)MediaSampleContent.TransportPacket, true);
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
            if (_pinAudioTimeShift != null)
            {
              Log.Log.WriteFile("    map TS audio pid:0x{0:X} type:{1} language:{2}", pmtData.pid, _currentAudioStream.StreamType, _currentAudioStream.Language);
              SetupDemuxerPin(_pinAudioTimeShift, pmtData.pid, (int)MediaSampleContent.ElementaryStream, true);
            }
            hwPids.Add((ushort)pmtData.pid);
            writer.SetAudioPid(pmtData.pid);
          }
        }

        if (pmtData.isVideo)
        {
          if (_pinVideoTimeShift != null)
          {
            Log.Log.WriteFile("    map TS video pid:0x{0:X}", pmtData.pid);
            SetupDemuxerPin(_pinVideoTimeShift, pmtData.pid, (int)MediaSampleContent.ElementaryStream, true);
          }
          hwPids.Add((ushort)pmtData.pid);
          writer.SetVideoPid(pmtData.pid);
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
    /// returns the IStreamAnalyzer interface for the graph
    /// </summary>
    public IStreamAnalyzer StreamAnalyzer
    {
      get
      {
        return _streamAnalyzer;
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
    /// returns the output pin of the mpeg-2 demultiplexer which is 
    /// connected to the stream analyzer
    /// </summary>
    public IPin PinAnalyzerSI
    {
      get
      {
        return _pinAnalyzerSI;
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
      if (_filterStreamBufferSink != null)
      {
        object pRecordingIUnknown;
        IStreamBufferSink sink = (IStreamBufferSink)_filterStreamBufferSink;
        hr = sink.CreateRecorder(fileName, recordingType, out  pRecordingIUnknown);
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:Analog: Unable to create recorder:0x{0:X}", hr);
          throw new TvException("Unable to create recorder");
        }
        _recorder = pRecordingIUnknown as IStreamBufferRecordControl;
        hr = _recorder.Start(ref startTime);
        if (hr != 0)
        {
          throw new TvException("Unable to start recording");
        }
        bool started, stopped;
        int result;
        _recorder.GetRecordingStatus(out result, out started, out stopped);
        Log.Log.WriteFile("dvb:Recording started:{0} stopped:{1}", started, stopped);
        return;
      }
      if (_tsFileSink != null)
      {
        Log.Log.WriteFile("dvb:SetTimeShiftFileName: uses .mpg");
        IMPRecord record = _tsFileSink as IMPRecord;
        hr = record.SetRecordingFileName(fileName);
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
      if (_recorder != null)
      {
        hr = _recorder.Stop(1);
        if (hr != 0)
        {
          Log.Log.WriteFile("dvb:Stop record:0x{0:X}", hr);
          throw new TvException("Unable to stop recording");
        }
        Release.ComObject("recorder", _recorder);
        _recorder = null;
      }

      if (_tsFileSink != null)
      {
        IMPRecord record = _tsFileSink as IMPRecord;
        record.StopRecord();

      }
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
      _dvbGrabber.GrabEPG();
      _mhwGrabber.GrabMHW();
      _epgGrabbing = true;
    }
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
          _dvbGrabber.IsEPGReady(out dvbReady);
          _mhwGrabber.IsMHWReady(out mhwReady);
          if (dvbReady == false || mhwReady == false) return null;

          List<EpgChannel> epgChannels = new List<EpgChannel>();
          if (mhwReady)
          {
            short titleCount;
            _mhwGrabber.GetMHWTitleCount(out titleCount);
            Log.Log.WriteFile("dvb:mhw ready {0} titles found", titleCount);
            for (int i = 0; i < titleCount; ++i)
            {
              short id = 0, transportid = 0, networkid = 0, channelnr = 0, channelid = 0, programid = 0, themeid = 0, PPV = 0, duration = 0;
              byte summaries = 0;
              uint datestart = 0, timestart = 0;
              IntPtr ptrTitle, ptrProgramName;
              IntPtr ptrChannelName, ptrSummary, ptrTheme;
              _mhwGrabber.GetMHWTitle((short)i, ref id, ref transportid, ref networkid, ref channelnr, ref programid, ref themeid, ref PPV, ref summaries, ref duration, ref datestart, ref timestart, out ptrTitle, out ptrProgramName);
              _mhwGrabber.GetMHWChannel(channelnr, ref channelid, ref networkid, ref transportid, out ptrChannelName);
              _mhwGrabber.GetMHWSummary(programid, out ptrSummary);
              _mhwGrabber.GetMHWTheme(themeid, out ptrTheme);

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
            _dvbGrabber.GetEPGChannelCount(out channelCount);
            Log.Log.WriteFile("dvb:dvb ready. Epg for {0} channels", channelCount);
            for (uint x = 0; x < channelCount; ++x)
            {
              _dvbGrabber.GetEPGChannel((uint)x, ref networkid, ref transportid, ref serviceid);
              EpgChannel epgChannel = new EpgChannel();
              DVBBaseChannel chan = new DVBBaseChannel();
              chan.NetworkId = networkid;
              chan.TransportId = transportid;
              chan.ServiceId = serviceid;
              epgChannel.Channel = chan;


              uint eventCount = 0;
              _dvbGrabber.GetEPGEventCount((uint)x, out eventCount);
              for (uint i = 0; i < eventCount; ++i)
              {
                uint start_time_MJD = 0, start_time_UTC = 0, duration = 0, languageId = 0, languageCount = 0;
                string title, description, genre;
                IntPtr ptrTitle = IntPtr.Zero;
                IntPtr ptrDesc = IntPtr.Zero;
                IntPtr ptrGenre = IntPtr.Zero;
                _dvbGrabber.GetEPGEvent((uint)x, (uint)i, out languageCount, out start_time_MJD, out start_time_UTC, out duration, out ptrGenre);
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
                    _dvbGrabber.GetEPGLanguage((uint)x, (uint)i, (uint)z, out languageId, out ptrTitle, out ptrDesc);
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
    void _pmtTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        if (_graphRunning == false) return;
#if FORM
        if (_newPMT)
        {
          SendPmtToCam();
          _newPMT=false;
        }
#endif

        if (_epgGrabbing)
        {
          bool dvbReady, mhwReady;
          _dvbGrabber.IsEPGReady(out dvbReady);
          _mhwGrabber.IsMHWReady(out mhwReady);
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
          SendPmtToCam();
        }*/
      }
      catch (Exception ex)
      {
        Log.Log.WriteFile("dvb:{0}", ex.Message);
        Log.Log.WriteFile("dvb:{0}", ex.Source);
        Log.Log.WriteFile("dvb:{0}", ex.StackTrace);
      }
    }
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

    #region ISampleGrabberCB Members

    /// <summary>
    /// callback from the samplegrabber filter when it received a new sample
    /// </summary>
    /// <param name="SampleTime"></param>
    /// <param name="pSample"></param>
    /// <returns></returns>
    public int SampleCB(double SampleTime, IMediaSample pSample)
    {

      return 0;
    }
    /// <summary>
    /// callback from the samplegrabber filter when it received a new sample
    /// The method decodes the transport stream packets and if it
    /// contains teletext packets, sends them to the teletext decoder
    /// </summary>
    /// <param name="SampleTime"></param>
    /// <param name="pBuffer"></param>
    /// <param name="BufferLen"></param>
    /// <returns></returns>
    public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
    {
      try
      {

        int off = -1;
        if (_restBufferLen > 0)
        {
          int len = 188 - _restBufferLen;	//remaining bytes of packet
          if (len > 0 && len < BufferLen)
          {
            if (_restBufferLen >= 0 && _restBufferLen + len < 200)
            {

              //copy the remaining bytes 
              Marshal.Copy(pBuffer, _restBuffer, _restBufferLen, len);
              Marshal.Copy(_restBuffer, 0, _ptrRestBuffer, 188);

              ProcessPacket(_ptrRestBuffer);

              //set offset ...
              if (Marshal.ReadByte(pBuffer, len) == 0x47 && Marshal.ReadByte(pBuffer, len + 188) == 0x47 && Marshal.ReadByte(pBuffer, len + 2 * 188) == 0x47)
              {
                off = len;
              }
            }
            else _restBufferLen = 0;
          }
          else _restBufferLen = 0;
        }
        if (off == -1)
        {
          //no then find first 3 transport packets in mediasample
          for (int i = 0; i < BufferLen - 2 * 188; ++i)
          {
            if (Marshal.ReadByte(pBuffer, i) == 0x47 && Marshal.ReadByte(pBuffer, i + 188) == 0x47 && Marshal.ReadByte(pBuffer, i + 2 * 188) == 0x47)
            {
              //found first 3 ts packets
              //set the offset
              off = i;
              break;
            }
          }
        }
        for (uint t = (uint)off; t < BufferLen; t += 188)
        {
          if (t + 188 > BufferLen) break;
          ProcessPacket((IntPtr)((int)pBuffer + t));
        }
        if (_restBufferLen > 0)
        {
          _restBufferLen /= 188;
          _restBufferLen *= 188;
          _restBufferLen = (BufferLen - off) - _restBufferLen;
          if (_restBufferLen > 0 && _restBufferLen < 188)
          {
            //copy the incomplete packet in the rest buffer
            Marshal.Copy((IntPtr)((int)pBuffer + BufferLen - _restBufferLen), _restBuffer, 0, _restBufferLen);
          }
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
        return;
      }
      if (_packetHeader.TransportError == true)
      {
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
        if (!CheckThreadId()) return null;
        if (_pinAudioTimeShift == null) return null;
        List<IAudioStream> streams = AvailableAudioStreams;
        IntPtr pids = Marshal.AllocCoTaskMem(30 * sizeof(int));
        IntPtr elementary_stream = Marshal.AllocCoTaskMem(30 * sizeof(int));
        Int32 pidCount = 30;
        try
        {
          GetPidMapping(_pinAudioTimeShift, pids, elementary_stream, ref pidCount);
          foreach (DVBAudioStream stream in streams)
          {
            for (int i = 0; i < pidCount; ++i)
            {
              int pid = Marshal.ReadInt32(pids, i * 4);
              if (pid == stream.Pid)
              {
                return stream;
              }
            }
          }
          return null;
        }
        finally
        {
          Marshal.FreeCoTaskMem(pids);
          Marshal.FreeCoTaskMem(elementary_stream);
        }
      }
      set
      {
        if (!CheckThreadId()) return;
        List<IAudioStream> streams = AvailableAudioStreams;
        DVBAudioStream audioStream = (DVBAudioStream)value;
        Log.Log.WriteFile("dvb: setaudiostream:{0}", audioStream);
        SetupDemuxerPin(_pinAudioTimeShift, audioStream.Pid, (int)MediaSampleContent.ElementaryStream, true);
        _currentAudioStream = audioStream;
        
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
        if (_filterTsWriter == null) return false;
        if (_currentChannel == null) return false;
        ITsWriter writer = (ITsWriter)_filterTsWriter;
        int yesNo;
        writer.IsAudioEncrypted(out yesNo);
        if (yesNo == 1) return false;
        if (_currentChannel.IsTv)
        {
          writer.IsVideoEncrypted(out yesNo);
          if (yesNo == 1) return false;
        }
        return true;
      }
    }

    #region IPMTCallback interface
    public int OnPMTReceived()
    {
      Log.Log.WriteFile("dvb:OnPMTReceived()");
      if (_graphRunning == false) return 0;
#if FORM
      _newPMT=true;
#else
      SendPmtToCam();
#endif
      return 0;
    }
    #endregion

    protected void SendPmtToCam()
    {
      lock (this)
      {
        DVBBaseChannel channel = _currentChannel as DVBBaseChannel;
        if (channel == null) return;
        IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
        try
        {
          int pmtLength = _streamAnalyzer.GetPMTData(pmtMem);
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
                  if (_conditionalAccess.SendPMT((DVBBaseChannel)Channel, pmt, pmtLength) == false)
                  {
                    return;
                  }
                }
                _pmtVersion = version;
                SetMpegPidMapping(_channelInfo);
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
    }
    #endregion
  }
}
