/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
//#define USEMTSWRITER
using System;
using Microsoft.Win32;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DShowNET;
using MediaPortal;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using DirectX.Capture;
using MediaPortal.TV.Database;
using MediaPortal.Player;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;



namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Zusammenfassung für DVBGraphSS2.
  /// </summary>
  /// 
  public class DVBGraphSS2 : IGraph
  {

    #region Mpeg2-Arrays
    static byte[] Mpeg2ProgramVideo = 
				{
					0x00, 0x00, 0x00, 0x00,                         //00  hdr.rcSource.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //04  hdr.rcSource.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //08  hdr.rcSource.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //0c  hdr.rcSource.bottom            = 0x00000240 //576
					0x00, 0x00, 0x00, 0x00,                         //10  hdr.rcTarget.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //14  hdr.rcTarget.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //18  hdr.rcTarget.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //1c  hdr.rcTarget.bottom            = 0x00000240// 576
					0x00, 0x09, 0x3D, 0x00,                         //20  hdr.dwBitRate                  = 0x003d0900
					0x00, 0x00, 0x00, 0x00,                         //24  hdr.dwBitErrorRate             = 0x00000000

					//0x051736=333667-> 10000000/333667 = 29.97fps
					//0x061A80=400000-> 10000000/400000 = 25fps
					0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
					0x00, 0x00, 0x00, 0x00,                         //2c  hdr.dwInterlaceFlags           = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //30  hdr.dwCopyProtectFlags         = 0x00000000
					0x04, 0x00, 0x00, 0x00,                         //34  hdr.dwPictAspectRatioX         = 0x00000004
					0x03, 0x00, 0x00, 0x00,                         //38  hdr.dwPictAspectRatioY         = 0x00000003
					0x00, 0x00, 0x00, 0x00,                         //3c  hdr.dwReserved1                = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //40  hdr.dwReserved2                = 0x00000000
					0x28, 0x00, 0x00, 0x00,                         //44  hdr.bmiHeader.biSize           = 0x00000028
					0xD0, 0x02, 0x00, 0x00,                         //48  hdr.bmiHeader.biWidth          = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //4c  hdr.bmiHeader.biHeight         = 0x00000240 //576
					0x00, 0x00,                                     //50  hdr.bmiHeader.biPlanes         = 0x0000
					0x00, 0x00,                                     //54  hdr.bmiHeader.biBitCount       = 0x0000
					0x00, 0x00, 0x00, 0x00,                         //58  hdr.bmiHeader.biCompression    = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //5c  hdr.bmiHeader.biSizeImage      = 0x00000000
					0xD0, 0x07, 0x00, 0x00,                         //60  hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
					0x27, 0xCF, 0x00, 0x00,                         //64  hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
					0x00, 0x00, 0x00, 0x00,                         //68  hdr.bmiHeader.biClrUsed        = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //6c  hdr.bmiHeader.biClrImportant   = 0x00000000
					0x98, 0xF4, 0x06, 0x00,                         //70  dwStartTimeCode                = 0x0006f498
					0x00, 0x00, 0x00, 0x00,                         //74  cbSequenceHeader               = 0x00000056
					//0x00, 0x00, 0x00, 0x00,                         //74  cbSequenceHeader               = 0x00000000
					0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
					0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
					0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
					//  .dwSequenceHeader [1]
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
		};
    static byte[] MPEG1AudioFormat = 
	  {
		  0x50, 0x00,             // format type      = 0x0050=WAVE_FORMAT_MPEG
		  0x02, 0x00,             // channels		  = 2
		  0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
		  0x00, 0xEE, 0x02, 0x00, // nAvgBytesPerSec  = 0x00007d00=192000
		  0x04, 0x00,             // nBlockAlign      = 4 (channels*(bitspersample/8))
		  0x10, 0x00,             // wBitsPerSample   = 0
		  0x00, 0x00,             // extra size       = 0x0000 = 0 bytes
		};
    #endregion

    #region Enums
    protected enum State
    {
      None,
      Created,
      TimeShifting,
      Recording,
      Viewing,
      Radio,
      Epg
    };
    /* tuner type from SDK
      TUNER_SATELLITE = 0,
      TUNER_CABLE  = 1,
      TUNER_TERRESTRIAL = 2,
      TUNER_ATSC = 3,
      TUNER_TERRESTRIAL_DVB = TUNER_TERRESTRIAL,
      TUNER_TERRESTRIAL_ATSC = TUNER_ATSC,
      TUNER_UNKNOWN = -1,
    */
    public enum TunerType
    {
      ttSat = 0,
      ttCable = 1,
      ttTerrestrial = 2,
      ttATSC = 3,
      ttUnknown = -1
    }
    protected enum eModulationTAG
    {
      QAM_4 = 2,
      QAM_16,
      QAM_32,
      QAM_64,
      QAM_128,
      QAM_256,
      MODE_UNKNOWN = -1
    }

    #endregion

    #region Structs
    /*
				*	Structure completedy by GetTunerCapabilities() to return tuner capabilities
				*/
    public struct tTunerCapabilities
    {
      public TunerType eModulation;
      public int dwConstellationSupported;       // Show if SetModulation() is supported
      public int dwFECSupported;                 // Show if SetFec() is suppoted
      public int dwMinTransponderFreqInKHz;
      public int dwMaxTransponderFreqInKHz;
      public int dwMinTunerFreqInKHz;
      public int dwMaxTunerFreqInKHz;
      public int dwMinSymbolRateInBaud;
      public int dwMaxSymbolRateInBaud;
      public int bAutoSymbolRate;				// Obsolte		
      public int dwPerformanceMonitoring;        // See bitmask definitions below
      public int dwLockTimeInMilliSecond;		// lock time in millisecond
      public int dwKernelLockTimeInMilliSecond;	// lock time for kernel
      public int dwAcquisitionCapabilities;
    }

    #endregion

    #region Imports
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern bool DvrMsCreate(out int id, IBaseFilter streamBufferSink, [In, MarshalAs(UnmanagedType.LPWStr)]string strPath, uint dwRecordingType);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void DvrMsStart(int id, uint startTime);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void DvrMsStop(int id);

    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    public static extern int SetupDemuxer(IPin pin, int pid, IPin pin1, int pid1, IPin pin2, int pid2);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetSectionData(DShowNET.IBaseFilter filter, int pid, int tid, ref int secCount, int tabSec, int timeout);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SetPidToPin(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin, UInt16 pid);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DeleteAllPIDs(DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 dataCtrl, UInt16 pin);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetSNR(DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2 tunerCtrl, [Out] out int a, [Out] out int b);

    // registry settings
    [DllImport("advapi32", CharSet = CharSet.Auto)]
    private static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

    [ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
    class StreamBufferConfig { }
    #endregion

    #region Definitions
    //
    public static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid(0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
    public static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid(0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
    //
    const int WS_CHILD = 0x40000000;
    const int WS_CLIPCHILDREN = 0x02000000;
    const int WS_CLIPSIBLINGS = 0x04000000;
    //
    // 

    protected bool m_bOverlayVisible = false;
    protected DVBChannel _currentChannel = new DVBChannel();
    //
    //
    protected bool m_firstTune = false;
    //
    protected IBaseFilter _filterSampleGrabber = null;
    protected ISampleGrabber m_sampleInterface = null;
    IEPGGrabber _interfaceEpg = null;
    IMHWGrabber _interfaceMHW = null;
    IBaseFilter _filterDvbAnalyzer = null;
    IStreamAnalyzer _interfaceStreamAnalyser = null;
#if USEMTSWRITER
		IBaseFilter						  _filterTsWriter=null;
		IMPTSWriter							_interfaceTsWriter=null;
		IMPTSRecord						  _interfaceTsRecord=null;
		IBaseFilter							_filterKernelTee= null;			
#endif
    EpgGrabber _epgGrabber = new EpgGrabber();

    protected IMpeg2Demultiplexer _filterMpeg2DemuxerInterface = null;
    protected IBasicVideo2 _interfaceBasicVideo = null;
    protected IVideoWindow _interfaceVideoWindow = null;
    protected State _graphState = State.None;
    protected IMediaControl _interfaceMediaControl = null;
    protected int _cardId = -1;
    protected IBaseFilter _filterB2C2Adapter = null;
    protected IPin _pinVideo = null;
    protected IPin _pinAudio = null;
    protected IPin _filterMpeg2DemuxerVideoPin = null;
    protected IPin _filterMpeg2DemuxerAudioPin = null;
    protected IPin _filterMpeg2DemuxerSectionsPin = null;
    protected IPin _pinData0 = null;
    protected IPin _pinData1 = null;
    protected IPin _pinData2 = null;
    protected IPin _pinData3 = null;
    protected IPin _pinAc3Audio = null;
    // stream buffer sink filter
    protected IStreamBufferConfigure _interfaceStreamBufferConfig = null;
    protected IStreamBufferSink3 _interfaceStreamBufferSink = null;
    protected IBaseFilter _filterStreamBuffer = null;
    protected IBaseFilter _filterMpeg2Analyzer = null;
    protected IBaseFilter _filterMpeg2Demuxer = null;
    // def. the interfaces
    protected DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3 _interfaceB2C2DataCtrl = null;
    protected DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2 _interfaceB2C2TunerCtrl = null;
    protected DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2 _interfaceB2C2AvcCtrl = null;
    // player graph
    protected IGraphBuilder _graphBuilder = null;
    protected bool _timeshifting = true;
    protected int _cookie = 0; // for the rot
    protected DateTime _startTime = DateTime.Now;
    protected int _channelNumber = -1;
    protected bool _channelFound = false;
    StreamBufferConfig _streamBufferConfig = null;
    protected VMR9Util _vmr9 = null;
    protected VMR7Util _vmr7 = null;

    protected string _fileName = "";
    protected DVBSections _dvbSections = new DVBSections();
    protected bool _pluginsEnabled = false;
    int[] _ecmPids = new int[3] { 0, 0, 0 };
    int[] _ecmIds = new int[3] { 0, 0, 0 };
    DVBDemuxer _dvbDemuxer = new DVBDemuxer();
    string _cardType = "";
    string _cardFilename = "";
    DVBChannel _currentTuningObject;
    int _recordedId = -1;
    int _selectedAudioPid = 0;
    int _videoWidth = 1;
    int _videoHeight = 1;
    int _aspectRatioX = 1;
    int _aspectRatioY = 1;
    bool _isUsingAc3 = false;
    int _lastPmtVersion = -1;
    DateTime _timerSignalLost;
    DateTime _timerSignalLost2;

    DateTime _timeDisplayed = DateTime.Now;

    bool _lastTuneFailed = false;
    NetworkType _networkType = NetworkType.DVBS;
    int _signalQuality;
    bool _isGraphRunning;


    #endregion


    public DVBGraphSS2(int cardId, int iCountryCode, bool bCable, string strVideoCaptureFilter, string strAudioCaptureFilter, string strVideoCompressor, string strAudioCompressor, Size frameSize, double frameRate, string strAudioInputPin, int RecordingLevel)
    {
      _cardId = cardId;

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        _pluginsEnabled = xmlreader.GetValueAsBool("dvb_ts_cards", "enablePlugins", false);
        _cardType = xmlreader.GetValueAsString("DVBSS2", "cardtype", "");
        _cardFilename = xmlreader.GetValueAsString("dvb_ts_cards", "filename", "");
      }


      //            _dvbDemuxer.OnAudioFormatChanged += new DVBDemuxer.OnAudioChanged(OnAudioFormatChanged);

      _dvbDemuxer.SetCardType((int)DVBEPG.EPGCard.TechnisatStarCards, NetworkType.DVBS);
      //			_dvbDemuxer.OnPMTIsChanged+=new MediaPortal.TV.Recording.DVBDemuxer.OnPMTChanged(_dvbDemuxer_OnPMTIsChanged);
      //			_dvbDemuxer.OnGotTable+=new MediaPortal.TV.Recording.DVBDemuxer.OnTableReceived(_dvbDemuxer_OnGotTable);
      // reg. settings
      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm = Registry.LocalMachine;
        hklm.CreateSubKey(@"Software\MediaPortal");

      }
      catch (Exception) { }
    }

    bool OnAudioFormatChanged(DVBDemuxer.AudioHeader audioFormat)
    {
      // set demuxer
      // release memory

      //				AMMediaType mpegAudioOut = new AMMediaType();
      //				mpegAudioOut.majorType = MediaType.Audio;
      //				mpegAudioOut.subType = MediaSubType.MPEG2_Audio;
      //				mpegAudioOut.sampleSize = 0;
      //				mpegAudioOut.temporalCompression = false;
      //				mpegAudioOut.fixedSizeSamples = true;
      //				mpegAudioOut.unkPtr = IntPtr.Zero;
      //				mpegAudioOut.formatType = FormatType.WaveEx;
      //				mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
      //				mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
      //				System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);

      return true;
    }
    ~DVBGraphSS2()
    {
    }
    //
    public static void Message()
    {
    }
    //
    //
    /// <summary>
    /// Callback from Card. Sets an information struct with video settings
    /// </summary>

    public bool CreateGraph(int Quality)
    {
      int hr;
      //SetAppHandle((int)GUIGraphicsContext.form.Handle);
      if (_graphState != State.None) return false;
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph()");
      _cookie = 0;
      if (_dvbDemuxer != null)
        _dvbDemuxer.GrabTeletext(false);
      // create graphs
      _vmr9 = new VMR9Util("mytv");
      _vmr7 = new VMR7Util();
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph() create graph");
      _graphBuilder = (IGraphBuilder)Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.FilterGraph, true));
      _isUsingAc3 = false;

      int n = 0;
      _filterB2C2Adapter = null;
      // create filters & interfaces
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph() create filters");
      try
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:CreateGraph() create B2C2 adapter");
        _filterB2C2Adapter = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(DVBSkyStar2Helper.CLSID_B2C2Adapter, false));

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:CreateGraph() create streambuffer");
        _filterStreamBuffer = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(DVBSkyStar2Helper.CLSID_StreamBufferSink, false));

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:CreateGraph() create MPEG2 analyzer");
        _filterMpeg2Analyzer = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(DVBSkyStar2Helper.CLSID_Mpeg2VideoStreamAnalyzer, true));
        _interfaceStreamBufferSink = (IStreamBufferSink3)_filterStreamBuffer;

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:CreateGraph() create MPEG2 demultiplexer");
        _filterMpeg2Demuxer = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.Mpeg2Demultiplexer, true));
        _filterMpeg2DemuxerInterface = (IMpeg2Demultiplexer)_filterMpeg2Demuxer;

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:CreateGraph() create sample grabber");
        _filterSampleGrabber = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.SampleGrabber, true));
        m_sampleInterface = (ISampleGrabber)_filterSampleGrabber;


        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:CreateGraph() create dvbanalyzer");
        _filterDvbAnalyzer = (IBaseFilter)Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.MPStreamAnalyzer, true));
        _interfaceStreamAnalyser = (IStreamAnalyzer)_filterDvbAnalyzer;
        _interfaceEpg = _filterDvbAnalyzer as IEPGGrabber;
        _interfaceMHW = _filterDvbAnalyzer as IMHWGrabber;
        hr = _graphBuilder.AddFilter(_filterDvbAnalyzer, "Stream-Analyzer");
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2: FAILED to add SectionsFilter 0x{0:X}", hr);
          return false;
        }
      }

      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
        return false;
        //System.Windows.Forms.MessageBox.Show(ex.Message);
      }

      if (_filterB2C2Adapter == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph() _filterB2C2Adapter not found");
        return false;
      }
      try
      {

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph() add filters to graph");
        n = _graphBuilder.AddFilter(_filterB2C2Adapter, "B2C2-Source");
        if (n != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: FAILED to add B2C2-Adapter");
          return false;
        }
        n = _graphBuilder.AddFilter(_filterSampleGrabber, "GrabberFilter");
        if (n != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: FAILED to add SampleGrabber");
          return false;
        }

        n = _graphBuilder.AddFilter(_filterMpeg2Demuxer, "MPEG-2 Demultiplexer");
        if (n != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: FAILED to add Demultiplexer");
          return false;
        }
        // get interfaces
        _interfaceB2C2DataCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2DataCtrl3;
        if (_interfaceB2C2DataCtrl == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: cannot get IB2C2MPEG2DataCtrl3");
          return false;
        }
        _interfaceB2C2TunerCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2TunerCtrl2;
        if (_interfaceB2C2TunerCtrl == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: cannot get IB2C2MPEG2TunerCtrl2");
          return false;
        }
        _interfaceB2C2AvcCtrl = _filterB2C2Adapter as DVBSkyStar2Helper.IB2C2MPEG2AVCtrl2;
        if (_interfaceB2C2AvcCtrl == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: cannot get IB2C2MPEG2AVCtrl2");
          return false;
        }
        // init for tuner

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Initialize Tuner()");
        n = _interfaceB2C2TunerCtrl.Initialize();
        if (n != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Tuner initialize failed");
          return false;
        }
        // Get tuner type (DVBS, DVBC, DVBT, ATSC)

        tTunerCapabilities tc;
        int lTunerCapSize = Marshal.SizeOf(typeof(tTunerCapabilities));

        IntPtr ptCaps = Marshal.AllocHGlobal(lTunerCapSize);

        n = _interfaceB2C2TunerCtrl.GetTunerCapabilities(ptCaps, ref lTunerCapSize);
        if (n != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Tuner Type failed");
          return false;
        }

        tc = (tTunerCapabilities)Marshal.PtrToStructure(ptCaps, typeof(tTunerCapabilities));

        switch (tc.eModulation)
        {
          case TunerType.ttSat:
            _networkType = NetworkType.DVBS;
            break;
          case TunerType.ttCable:
            _networkType = NetworkType.DVBC;
            break;
          case TunerType.ttTerrestrial:
            _networkType = NetworkType.DVBT;
            break;
          case TunerType.ttATSC:
            _networkType = NetworkType.ATSC;
            break;
          case TunerType.ttUnknown:
            _networkType = NetworkType.Unknown;
            break;
        }
        Marshal.FreeHGlobal(ptCaps);

        // call checklock once, the return value dont matter

        n = _interfaceB2C2TunerCtrl.CheckLock();
        bool b = false;


        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: SetVideoAudioPins()");
        b = SetVideoAudioPins();
        if (b == false)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: SetVideoAudioPins() failed");
          return false;
        }

        //create EPG pins
        Log.Write("DVBGraphSS2:Create EPG pin");
        AMMediaType mtEPG = new AMMediaType();
        mtEPG.majorType = MEDIATYPE_MPEG2_SECTIONS;
        mtEPG.subType = MediaSubType.None;
        mtEPG.formatType = FormatType.None;

        AMMediaType mtSections = new AMMediaType();
        mtSections.majorType = MEDIATYPE_MPEG2_SECTIONS;
        mtSections.subType = MediaSubType.None;
        mtSections.formatType = FormatType.None;


        IPin pinEPGout, pinMHW1Out, pinMHW2Out, pinSectionsOut;
        hr = _filterMpeg2DemuxerInterface.CreateOutputPin(ref mtSections, "sections", out pinSectionsOut);
        if (hr != 0 || pinSectionsOut == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to create sections pin:0x{0:X}", hr);
          return false;
        }
        hr = _filterMpeg2DemuxerInterface.CreateOutputPin(ref mtEPG, "EPG", out pinEPGout);
        if (hr != 0 || pinEPGout == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to create EPG pin:0x{0:X}", hr);
          return false;
        }
        hr = _filterMpeg2DemuxerInterface.CreateOutputPin(ref mtEPG, "MHW1", out pinMHW1Out);
        if (hr != 0 || pinMHW1Out == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to create MHW1 pin:0x{0:X}", hr);
          return false;
        }
        hr = _filterMpeg2DemuxerInterface.CreateOutputPin(ref mtEPG, "MHW2", out pinMHW2Out);
        if (hr != 0 || pinMHW2Out == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to create MHW2 pin:0x{0:X}", hr);
          return false;
        }

        Log.Write("DVBGraphSS2:Get EPGs pin of analyzer");
        IPin pinSectionsIn = DirectShowUtil.FindPinNr(_filterDvbAnalyzer, PinDirection.Input, 0);
        if (pinSectionsIn == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to get sections pin on MSPA");
          return false;
        }

        IPin pinMHW1In = DirectShowUtil.FindPinNr(_filterDvbAnalyzer, PinDirection.Input, 1);
        if (pinMHW1In == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to get MHW1 pin on MSPA");
          return false;
        }
        IPin pinMHW2In = DirectShowUtil.FindPinNr(_filterDvbAnalyzer, PinDirection.Input, 2);
        if (pinMHW2In == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to get MHW2 pin on MSPA");
          return false;
        }
        IPin pinEPGIn = DirectShowUtil.FindPinNr(_filterDvbAnalyzer, PinDirection.Input, 3);
        if (pinEPGIn == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to get EPG pin on MSPA");
          return false;
        }

        Log.Write("DVBGraphSS2:Connect epg pins");
        hr = _graphBuilder.Connect(pinSectionsOut, pinSectionsIn);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to connect sections pin:0x{0:X}", hr);
          return false;
        }
        hr = _graphBuilder.Connect(pinEPGout, pinEPGIn);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to connect EPG pin:0x{0:X}", hr);
          return false;
        }
        hr = _graphBuilder.Connect(pinMHW1Out, pinMHW1In);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to connect MHW1 pin:0x{0:X}", hr);
          return false;
        }
        hr = _graphBuilder.Connect(pinMHW2Out, pinMHW2In);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:FAILED to connect MHW2 pin:0x{0:X}", hr);
          return false;
        }

        Log.Write("DVBGraphSS2:Demuxer is setup");


        if (m_sampleInterface != null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: add sample grabber");
          AMMediaType mt = new AMMediaType();
          mt.majorType = DShowNET.MediaType.Stream;
          mt.subType = DShowNET.MediaSubType.MPEG2Transport;
          //m_sampleInterface.SetOneShot(true);
          m_sampleInterface.SetCallback(_dvbDemuxer, 1);
          m_sampleInterface.SetMediaType(ref mt);
          m_sampleInterface.SetBufferSamples(false);
        }
        else
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph() SampleGrabber-Interface not found");


        _epgGrabber.EPGInterface = _interfaceEpg;
        _epgGrabber.MHWInterface = _interfaceMHW;
        _epgGrabber.Network = Network();

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: graph created");
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:creategraph() exception:{0}", ex.ToString());
        return false;
      }

      DsROT.AddGraphToRot(_graphBuilder, out _cookie);
      _graphState = State.Created;
      return true;
    }

    //
    private bool Tune(int Frequency, int SymbolRate, int FEC, int POL, int LNBKhz, int Diseq, int AudioPID, int VideoPID, int LNBFreq, int ecmPID, int ttxtPID, int pmtPID, int pcrPID, string pidText, int dvbsubPID, int programNumber, DVBChannel ch)
    {

      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2 Tune() freq:{0} SR:{1} FEC:{2} POL:{3} LNBKhz:{4} Diseq:{5} audiopid:{6:X} videopid:{7:X} LNBFreq:{8} ecmPid:{9:X} pmtPid:{10:X} pcrPid{11:X}",
                    Frequency, SymbolRate, FEC, POL, LNBKhz, Diseq, AudioPID, VideoPID, LNBFreq, ecmPID, pmtPID, pcrPID);
      int hr = 0;				// the result
      int modulation = 5;		//QAM_64
      int guardinterval = 4;	//GUARD_INTERVAL_AUTO

      _lastTuneFailed = false;
      // clear epg
      if (Frequency > 13000)
        Frequency /= 1000;

      if (_interfaceB2C2TunerCtrl == null || _interfaceB2C2DataCtrl == null || _filterB2C2Adapter == null || _interfaceB2C2AvcCtrl == null)
        return false;

      // skystar
      hr = _interfaceB2C2TunerCtrl.SetFrequency(Frequency);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetFrequency:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
      }
      hr = _interfaceB2C2TunerCtrl.SetSymbolRate(SymbolRate);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetSymbolRate:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
      }

      hr = _interfaceB2C2TunerCtrl.SetLnbFrequency(LNBFreq);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetLnbFrequency:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
      }
      hr = _interfaceB2C2TunerCtrl.SetFec(FEC);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetFec:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
      }
      hr = _interfaceB2C2TunerCtrl.SetPolarity(POL);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetPolarity:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
      }
      hr = _interfaceB2C2TunerCtrl.SetLnbKHz(LNBKhz);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetLnbKHz:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
      }
      hr = _interfaceB2C2TunerCtrl.SetDiseqc(Diseq);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetDiseqc:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
      }
      // cablestar
      if (_networkType == NetworkType.DVBC)
      {
        switch (ch.Modulation)
        {
          case (int)TunerLib.ModulationType.BDA_MOD_16QAM:
            modulation = (int)eModulationTAG.QAM_16;
            break;
          case (int)TunerLib.ModulationType.BDA_MOD_32QAM:
            modulation = (int)eModulationTAG.QAM_32;
            break;
          case (int)TunerLib.ModulationType.BDA_MOD_64QAM:
            modulation = (int)eModulationTAG.QAM_64;
            break;
          case (int)TunerLib.ModulationType.BDA_MOD_128QAM:
            modulation = (int)eModulationTAG.QAM_128;
            break;
          case (int)TunerLib.ModulationType.BDA_MOD_256QAM:
            modulation = (int)eModulationTAG.QAM_256;
            break;
        }
        hr = _interfaceB2C2TunerCtrl.SetModulation(modulation);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetModulation:0x{0:X}", hr);
          return false;	// *** FUNCTION EXIT POINT
        }
      }

      // airstar
      if (_networkType == NetworkType.DVBT)
      {
        hr = _interfaceB2C2TunerCtrl.SetGuardInterval(guardinterval);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetGuardInterval:0x{0:X}", hr);
          return false;	// *** FUNCTION EXIT POINT
        }
        // Set Channel Bandwidth (NOTE: Temporarily use polarity function to avoid having to 
        // change SDK interface for SetBandwidth)
        // from Technisat SDK 02/2005
        hr = _interfaceB2C2TunerCtrl.SetPolarity(ch.Bandwidth);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetBandwidth:0x{0:X}", hr);
          return false;	// *** FUNCTION EXIT POINT
        }
      }

      // final
      hr = _interfaceB2C2TunerCtrl.SetTunerStatus();
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "Tune for SkyStar2 FAILED: on SetTunerStatus:0x{0:X}", hr);
        return false;	// *** FUNCTION EXIT POINT
        //
      }

      DeleteAllPIDs(_interfaceB2C2DataCtrl, 0);

      int n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)VideoPID);
      n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)AudioPID);
      n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)pcrPID);
      n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)pmtPID);
      n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)0);
      n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)0x10);
      n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)0x11);
      n = SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)0x12);



      _lastPmtVersion = -1;

      return true;
    }
    //
    /// <summary>
    /// Overlay-Controlling
    /// </summary>

    public bool Overlay
    {
      get
      {
        return m_bOverlayVisible;
      }
      set
      {
        if (value == m_bOverlayVisible) return;
        m_bOverlayVisible = value;
        if (!m_bOverlayVisible)
        {
          if (_interfaceVideoWindow != null)
            _interfaceVideoWindow.put_Visible(DsHlp.OAFALSE);

        }
        else
        {
          if (_interfaceVideoWindow != null)
            _interfaceVideoWindow.put_Visible(DsHlp.OATRUE);

        }
      }
    }
    /// <summary>
    /// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
    /// </summary>

    private void GUIGraphicsContext_OnVideoWindowChanged()
    {
      if (GUIGraphicsContext.Vmr9Active) return;

      if (_interfaceBasicVideo == null) return;
      if (_interfaceVideoWindow == null) return;
      if (_graphState != State.Viewing) return;

      if (GUIGraphicsContext.BlankScreen)
      {
        Overlay = false;
      }
      else
      {
        Overlay = true;
      }
      int iVideoWidth = 0;
      int iVideoHeight = 0;
      int aspectX = 4, aspectY = 3;
      if (_interfaceBasicVideo != null)
      {
        _interfaceBasicVideo.GetVideoSize(out iVideoWidth, out iVideoHeight);
        _interfaceBasicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
      }

      GUIGraphicsContext.VideoSize = new Size(iVideoWidth, iVideoHeight);
      _videoWidth = iVideoWidth;
      _videoHeight = iVideoHeight;
      _aspectRatioX = aspectX;
      _aspectRatioY = aspectY;

      if (GUIGraphicsContext.IsFullScreenVideo || GUIGraphicsContext.ShowBackground == false)
      {
        float x = GUIGraphicsContext.OverScanLeft;
        float y = GUIGraphicsContext.OverScanTop;
        int nw = GUIGraphicsContext.OverScanWidth;
        int nh = GUIGraphicsContext.OverScanHeight;
        if (nw <= 0 || nh <= 0) return;


        System.Drawing.Rectangle rSource, rDest;
        MediaPortal.GUI.Library.Geometry m_geometry = new MediaPortal.GUI.Library.Geometry();
        m_geometry.ImageWidth = iVideoWidth;
        m_geometry.ImageHeight = iVideoHeight;
        m_geometry.ScreenWidth = nw;
        m_geometry.ScreenHeight = nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(aspectX, aspectY, out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;
        Log.Write("overlay: video WxH  : {0}x{1}", iVideoWidth, iVideoHeight);
        Log.Write("overlay: video AR   : {0}:{1}", aspectX, aspectY);
        Log.Write("overlay: screen WxH : {0}x{1}", nw, nh);
        Log.Write("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
        Log.Write("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
        Log.Write("overlay: src        : ({0},{1})-({2},{3})",
          rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
        Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
          rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);

        if (_interfaceBasicVideo != null)
        {
          _interfaceBasicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
          _interfaceBasicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
        }
        if (_interfaceVideoWindow != null)
          _interfaceVideoWindow.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
      else
      {
        if (GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 ||
          GUIGraphicsContext.VideoWindow.Width <= 0 || GUIGraphicsContext.VideoWindow.Height <= 0) return;
        if (iVideoHeight <= 0 || iVideoWidth <= 0) return;

        if (_interfaceBasicVideo != null)
        {
          _interfaceBasicVideo.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
          _interfaceBasicVideo.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
        }
        if (_interfaceVideoWindow != null)
          _interfaceVideoWindow.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);

      }

    }

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public void DeleteGraph()
    {
      if (_graphState < State.Created) return;
      int hr;
      DirectShowUtil.DebugWrite("DVBGraphSS2:DeleteGraph()");
      _isUsingAc3 = false;
      _channelNumber = -1;
      //m_fileWriter.Close();
      StopRecording();
      StopTimeShifting();
      StopViewing();

      if (_graphState == State.Epg)
      {
        _epgGrabber.Reset();
        _graphState = State.Created;
      }

      if (_dvbDemuxer != null)
      {
        _dvbDemuxer.GrabTeletext(false);
        _dvbDemuxer.SetChannelData(0, 0, 0, 0, "", 0, 0);
      }

      if (_vmr9 != null)
      {
        _vmr9.RemoveVMR9();
        _vmr9.Release();
        _vmr9 = null;
      }
      if (_vmr7 != null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: free vmr7");
        _vmr7.RemoveVMR7();
        _vmr7 = null;
      }
      if (_recordedId >= 0)
      {
        DvrMsStop(_recordedId);
        _recordedId = -1;
      }


      if (_interfaceMediaControl != null) _interfaceMediaControl.Stop();
      _isGraphRunning = false; 
      _interfaceMediaControl = null;
      _interfaceBasicVideo = null;
      _pinVideo = null;
      _pinAudio = null;
      _filterMpeg2DemuxerVideoPin = null;
      _filterMpeg2DemuxerAudioPin = null;
      _filterMpeg2DemuxerSectionsPin = null;
      _pinData0 = null;
      _pinData1 = null;
      _pinData2 = null;
      _pinData3 = null;
      _pinAc3Audio = null;
      _interfaceStreamBufferSink = null;
      _filterMpeg2DemuxerInterface = null;
      m_sampleInterface = null;
      _interfaceB2C2TunerCtrl = null;
      _interfaceB2C2AvcCtrl = null;
      _interfaceB2C2DataCtrl = null;
      _interfaceStreamBufferConfig = null;
      _interfaceStreamAnalyser = null;
      _interfaceEpg = null;
      _interfaceMHW = null;
#if USEMTSWRITER
			_interfaceTsWriter=null;
			_interfaceTsRecord=null;
#endif

      if (_filterDvbAnalyzer != null)
      {
        Log.Write("free dvbanalyzer");
        hr = Marshal.ReleaseComObject(_filterDvbAnalyzer);
        if (hr != 0) Log.Write("ReleaseComObject(_filterDvbAnalyzer):{0}", hr);
        _filterDvbAnalyzer = null;
      }
#if USEMTSWRITER
			if (_filterTsWriter!=null)
			{
				Log.Write("free MPTSWriter");
				hr=Marshal.ReleaseComObject(_filterTsWriter);
				if (hr!=0) Log.Write("ReleaseComObject(_filterTsWriter):{0}",hr);
				_filterTsWriter=null;
			}

			if (_filterKernelTee != null)
			{
				while ((hr=Marshal.ReleaseComObject(_filterKernelTee))>0); 
				if (hr!=0) Log.Write("DVBGraphBDA:ReleaseComObject(_filterKernelTee):{0}",hr);
				_filterKernelTee = null;
			}
#endif
      if (_interfaceVideoWindow != null)
      {
        m_bOverlayVisible = false;
        _interfaceVideoWindow.put_Visible(DsHlp.OAFALSE);
        _interfaceVideoWindow = null;
      }
      if (_streamBufferConfig != null)
      {
        while ((hr = Marshal.ReleaseComObject(_streamBufferConfig)) > 0) ;
        if (hr != 0) Log.Write("ReleaseComObject(_streamBufferConfig):{0}", hr);
        _streamBufferConfig = null;
      }

      if (_filterSampleGrabber != null)
      {
        while ((hr = Marshal.ReleaseComObject(_filterSampleGrabber)) > 0) ;
        if (hr != 0) Log.Write("ReleaseComObject(_filterSampleGrabber):{0}", hr);
        _filterSampleGrabber = null;
      }
      if (_filterMpeg2Demuxer != null)
      {
        while ((hr = Marshal.ReleaseComObject(_filterMpeg2Demuxer)) > 0) ;
        if (hr != 0) Log.Write("ReleaseComObject(_filterMpeg2Demuxer):{0}", hr);
        _filterMpeg2Demuxer = null;
      }

      if (_filterMpeg2Analyzer != null)
      {
        while ((hr = Marshal.ReleaseComObject(_filterMpeg2Analyzer)) > 0) ;
        if (hr != 0) Log.Write("ReleaseComObject(_filterMpeg2Analyzer):{0}", hr);
        _filterMpeg2Analyzer = null;
      }

      if (_filterStreamBuffer != null)
      {
        while ((hr = Marshal.ReleaseComObject(_filterStreamBuffer)) > 0) ;
        if (hr != 0) Log.Write("ReleaseComObject(_filterStreamBuffer):{0}", hr);
        _filterStreamBuffer = null;
      }

      if (_filterB2C2Adapter != null)
      {
        while ((hr = Marshal.ReleaseComObject(_filterB2C2Adapter)) > 0) ;
        if (hr != 0) Log.Write("ReleaseComObject(_filterB2C2Adapter):{0}", hr);
        _filterB2C2Adapter = null;
      }
      DsUtils.RemoveFilters(_graphBuilder);

      if (_cookie != 0)
        DsROT.RemoveGraphFromRot(ref _cookie);
      _cookie = 0;

      if (_graphBuilder != null)
      {
        while ((hr = Marshal.ReleaseComObject(_graphBuilder)) > 0) ;
        if (hr != 0) Log.Write("ReleaseComObject(_graphBuilder):{0}", hr);
        _graphBuilder = null;
      }

      //add collected stuff into programs database
      if (GUIGraphicsContext.form != null)
      {
        GUIGraphicsContext.form.Invalidate(true);
      }
      GC.Collect(); GC.Collect(); GC.Collect();
      _graphState = State.None;

      return;
    }
    //
    //

    void AddPreferredCodecs(bool audio, bool video)
    {
      // add preferred video & audio codecs
      string strVideoCodec = "";
      string strAudioCodec = "";
      string strAudioRenderer = "";
      bool bAddFFDshow = false;
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        bAddFFDshow = xmlreader.GetValueAsBool("mytv", "ffdshow", false);
        strVideoCodec = xmlreader.GetValueAsString("mytv", "videocodec", "");
        strAudioCodec = xmlreader.GetValueAsString("mytv", "audiocodec", "");
        strAudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "");
      }
      if (video && strVideoCodec.Length > 0) DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
      if (audio && strAudioCodec.Length > 0) DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
      if (audio && strAudioRenderer.Length > 0) DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, false);
      if (video && bAddFFDshow) DirectShowUtil.AddFilterToGraph(_graphBuilder, "ffdshow raw video filter");
    }
    /// <summary>
    /// Starts timeshifting the TV channel and stores the timeshifting 
    /// files in the specified filename
    /// </summary>
    /// <param name="iChannelNr">TV channel to which card should be tuned</param>
    /// <param name="strFileName">Filename for the timeshifting buffers</param>
    /// <returns>boolean indicating if timeshifting is running or not</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    /// 
    private bool SetVideoAudioPins()
    {
      int hr = 0;
      PinInfo pInfo = new PinInfo();

      // video pin
      hr = DsUtils.GetPin(_filterB2C2Adapter, PinDirection.Output, 0, out _pinVideo);
      if (hr != 0)
        return false;

      _pinVideo.QueryPinInfo(out pInfo);
      // audio pin
      hr = DsUtils.GetPin(_filterB2C2Adapter, PinDirection.Output, 1, out _pinAudio);
      if (hr != 0)
        return false;

      if (_pinVideo == null || _pinAudio == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: pins not found on adapter");
        return false;
      }
      _pinAudio.QueryPinInfo(out pInfo);

      // data pins
      hr = DsUtils.GetPin(_filterB2C2Adapter, PinDirection.Output, 2, out _pinData0);
      if (hr != 0)
        return false;
      hr = DsUtils.GetPin(_filterB2C2Adapter, PinDirection.Output, 3, out _pinData1);
      if (hr != 0)
        return false;
      hr = DsUtils.GetPin(_filterB2C2Adapter, PinDirection.Output, 4, out _pinData2);
      if (hr != 0)
        return false;
      hr = DsUtils.GetPin(_filterB2C2Adapter, PinDirection.Output, 5, out _pinData3);
      if (hr != 0)
        return false;


      return true;
    }
    //
#if USEMTSWRITER
		private bool CreateMTSWriter(string fileName)
		{
			if(_graphState!=State.Created && _graphState!=State.TimeShifting)
				return false;
			if (_filterTsWriter!=null) 
			{
				SetupMTSDemuxerPin();
				return true;
			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:CreateMTSWriter()");
			//connect capture->sample grabber
			IPin grabberIn;
			grabberIn=DirectShowUtil.FindPinNr(_filterSampleGrabber,PinDirection.Input,0);
			if (grabberIn==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed cannot find input pin of sample grabber");
				return false;
			}
			int hr=_graphBuilder.Connect(_pinData0,grabberIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(grabberIn);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:Failed to connect capture->sample grabber");
				return false;
			}
			Marshal.ReleaseComObject(grabberIn);
			grabberIn=null;

			//connect sample grabber->inf tee
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:add InfTee filter");
			_filterKernelTee = (IBaseFilter) Activator.CreateInstance(Type.GetTypeFromCLSID(Clsid.InfTee, true));
			_graphBuilder.AddFilter(_filterKernelTee,"Inf Tee");
			
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect sample grabber->inf tee");
			IPin grabberOut, smartTeeIn;
			grabberOut=DirectShowUtil.FindPinNr(_filterSampleGrabber,PinDirection.Output,0);
			smartTeeIn=DirectShowUtil.FindPinNr(_filterKernelTee,PinDirection.Input,0);	
			if (grabberOut==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find output pin of samplegrabber");
				return false;
			}
			if (smartTeeIn==null)
			{
				Marshal.ReleaseComObject(grabberOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find input pin of inftee");
				return false;
			}

			hr=_graphBuilder.Connect(grabberOut,smartTeeIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(grabberOut);
				Marshal.ReleaseComObject(smartTeeIn);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot grabber->inftee :0x{0:X}",hr);
				return false;
			}
			Marshal.ReleaseComObject(grabberOut);
			Marshal.ReleaseComObject(smartTeeIn);

			//connect inftee->demuxer
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect inftee->demuxer");
			IPin smartTeeOut, demuxerIn;
			smartTeeOut = DirectShowUtil.FindPinNr(_filterKernelTee,PinDirection.Output,0);
			demuxerIn   = DirectShowUtil.FindPinNr(_filterMpeg2Demuxer,PinDirection.Input,0);	
			if (smartTeeOut==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find output pin#0 of inftee");
				return false;
			}
			if (demuxerIn==null)
			{
				Marshal.ReleaseComObject(smartTeeOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find input pin of demuxer");
				return false;
			}

			hr=_graphBuilder.Connect(smartTeeOut,demuxerIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(demuxerIn);
				Marshal.ReleaseComObject(smartTeeOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot inftee->demuxer :0x{0:X}",hr);
				return false;
			}
			Marshal.ReleaseComObject(demuxerIn);
			Marshal.ReleaseComObject(smartTeeOut);

			//add mpts writer
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:add MPTSWriter");
			_filterTsWriter=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.MPTSWriter, true ) );
			_interfaceTsWriter = _filterTsWriter as IMPTSWriter;
			_interfaceTsRecord = _filterTsWriter as IMPTSRecord;

			hr=_graphBuilder.AddFilter((IBaseFilter)_filterTsWriter,"MPTS Writer");
			if(hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot add MPTS Writer:{0:X}",hr);
				return false;
			}			

			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:set filename on MPTSWriter");
			IFileSinkFilter fileWriter=_filterTsWriter as IFileSinkFilter;
			AMMediaType mt = new AMMediaType();
			mt.majorType=MediaType.Stream;
			mt.subType=MediaSubType.None;
			mt.formatType=FormatType.None;
			hr=fileWriter.SetFileName(fileName, ref mt);
			if (hr!=0)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot set filename on MPTS writer:0x{0:X}",hr);
				return false;
			}


			// connect inftee->mpts writer
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:connect inftee->mpts writer");
			IPin tsWriterIn;
			smartTeeOut=DirectShowUtil.FindPinNr(_filterKernelTee,PinDirection.Output,1);
			tsWriterIn=DirectShowUtil.FindPinNr(_filterTsWriter,PinDirection.Input,0);	
			if (smartTeeOut==null)
			{
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find output pin#1 of inftee");
				return false;
			}
			if (tsWriterIn==null)
			{
				Marshal.ReleaseComObject(smartTeeOut);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot find input pin of tswriter");
				return false;
			}

			hr=_graphBuilder.Connect(smartTeeOut,tsWriterIn);
			if (hr!=0)
			{
				Marshal.ReleaseComObject(smartTeeOut);
				Marshal.ReleaseComObject(tsWriterIn);
				Log.WriteFile(Log.LogType.Capture,true,"DVBGraphBDA:FAILED cannot inftee->tswriter :0x{0:X}",hr);
				return false;
			}
			Marshal.ReleaseComObject(smartTeeOut);
			Marshal.ReleaseComObject(tsWriterIn);
			SetupMTSDemuxerPin();
			return true;
		}
#endif
    void SetupMTSDemuxerPin()
    {
#if USEMTSWRITER
			if (_interfaceTsWriter== null || _interfaceTsWriter==null || _currentChannel==null) return;
			_interfaceTsWriter.ResetPids();
			if (_currentChannel.AC3Pid>0)
				_interfaceTsWriter.SetAC3Pid((ushort)_currentChannel.AC3Pid);
			else
				_interfaceTsWriter.SetAC3Pid(0);

			if (_currentChannel.AudioPid>0)
				_interfaceTsWriter.SetAudioPid((ushort)_currentChannel.AudioPid);
			else
			{
				if (_currentChannel.Audio1>0)
					_interfaceTsWriter.SetAudioPid((ushort)_currentChannel.Audio1);
				else
					_interfaceTsWriter.SetAudioPid(0);
			}
			
			if (_currentChannel.Audio2>0)
				_interfaceTsWriter.SetAudioPid2((ushort)_currentChannel.Audio2);
			else
				_interfaceTsWriter.SetAudioPid2(0);

			//_interfaceTsWriter.SetSubtitlePid((ushort)_currentChannel.SubtitlePid);
			if (_currentChannel.TeletextPid>0)
				_interfaceTsWriter.SetTeletextPid((ushort)_currentChannel.TeletextPid);
			else
				_interfaceTsWriter.SetTeletextPid(0);

			if (_currentChannel.VideoPid>0)
				_interfaceTsWriter.SetVideoPid((ushort)_currentChannel.VideoPid);
			else
				_interfaceTsWriter.SetVideoPid(0);

			if (_currentChannel.PCRPid>0)
				_interfaceTsWriter.SetPCRPid((ushort)_currentChannel.PCRPid);
			else
				_interfaceTsWriter.SetPCRPid(0);

			_interfaceTsWriter.SetPMTPid((ushort)_currentChannel.PMTPid);
#endif
    }


    private bool CreateSinkSource(string fileName, bool useAC3)
    {
      if (_graphState != State.Created && _graphState != State.TimeShifting)
        return false;
      int hr = 0;
      IPin pinObj0 = null;
      IPin pinObj1 = null;
      IPin outPin = null;


      hr = _graphBuilder.AddFilter(_filterStreamBuffer, "StreamBufferSink");
      hr = _graphBuilder.AddFilter(_filterMpeg2Analyzer, "Stream-Analyzer");

      // setup sampleGrabber and demuxer
      IPin samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Input, 0);
      IPin demuxInPin = DirectShowUtil.FindPinNr(_filterMpeg2Demuxer, PinDirection.Input, 0);
      hr = _graphBuilder.Connect(_pinData0, samplePin);
      if (hr != 0)
        return false;

      samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Output, 0);
      hr = _graphBuilder.Connect(demuxInPin, samplePin);
      if (hr != 0)
        return false;

      SetDemux(_currentChannel.AudioPid, _currentChannel.VideoPid, _currentChannel.AC3Pid);


      if (_filterMpeg2DemuxerVideoPin == null || _filterMpeg2DemuxerAudioPin == null)
        return false;

      pinObj0 = DirectShowUtil.FindPinNr(_filterMpeg2Analyzer, PinDirection.Input, 0);
      if (pinObj0 != null)
      {

        hr = _graphBuilder.Connect(_filterMpeg2DemuxerVideoPin, pinObj0);
        if (hr == 0)
        {
          // render all out pins
          pinObj1 = DirectShowUtil.FindPinNr(_filterMpeg2Analyzer, PinDirection.Output, 0);
          hr = _graphBuilder.Render(pinObj1);
          if (hr != 0)
            return false;
          if (!useAC3)
          {
            hr = _graphBuilder.Render(_filterMpeg2DemuxerAudioPin);
            if (hr != 0)
              return false;
          }
          else
          {
            hr = _graphBuilder.Render(_pinAc3Audio);
            if (hr != 0)
              return false;
          }
          if (demuxInPin != null)
            Marshal.ReleaseComObject(demuxInPin);
          if (samplePin != null)
            Marshal.ReleaseComObject(samplePin);
          if (pinObj1 != null)
            Marshal.ReleaseComObject(pinObj1);
          if (pinObj0 != null)
            Marshal.ReleaseComObject(pinObj0);

          demuxInPin = null;
          samplePin = null;
          pinObj1 = null;
          pinObj0 = null;
        }
      } // render of sink is ready

      int ipos = fileName.LastIndexOf(@"\");
      string strDir = fileName.Substring(0, ipos);

      _streamBufferConfig = new StreamBufferConfig();
      _interfaceStreamBufferConfig = (IStreamBufferConfigure)_streamBufferConfig;
      // setting the timeshift behaviors
      IntPtr HKEY = (IntPtr)unchecked((int)0x80000002L);
      IStreamBufferInitialize pTemp = (IStreamBufferInitialize)_interfaceStreamBufferConfig;
      IntPtr subKey = IntPtr.Zero;

      RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
      hr = pTemp.SetHKEY(subKey);
      hr = _interfaceStreamBufferConfig.SetDirectory(strDir);
      if (hr != 0)
        return false;
      hr = _interfaceStreamBufferConfig.SetBackingFileCount(6, 8);    //4-6 files
      if (hr != 0)
        return false;

      hr = _interfaceStreamBufferConfig.SetBackingFileDuration(300); // 60sec * 4 files= 4 mins
      if (hr != 0)
        return false;

      subKey = IntPtr.Zero;
      HKEY = (IntPtr)unchecked((int)0x80000002L);
      IStreamBufferInitialize pConfig = (IStreamBufferInitialize)_filterStreamBuffer;

      RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
      hr = pConfig.SetHKEY(subKey);

      IStreamBufferConfigure2 streamConfig2 = _streamBufferConfig as IStreamBufferConfigure2;
      if (streamConfig2 != null)
        streamConfig2.SetFFTransitionRates(8, 32);

      // lock on the 'filename' file
      hr = _interfaceStreamBufferSink.LockProfile(fileName);
      _fileName = fileName;
      if (hr != 0)
        return false;

      if (pinObj0 != null)
        Marshal.ReleaseComObject(pinObj0);
      if (pinObj1 != null)
        Marshal.ReleaseComObject(pinObj1);
      if (outPin != null)
        Marshal.ReleaseComObject(outPin);

      return true;
    }
    //
    bool DeleteDataPids(int pin)
    {
      bool res = false;

      res = DeleteAllPIDs(_interfaceB2C2DataCtrl, 0);

      return res;

    }
    int AddDataPidsToPin(int pin, int pid)
    {
      int res = 0;

      res = SetPidToPin(_interfaceB2C2DataCtrl, (ushort)pin, (ushort)pid);

      return res;
    }
    //
    public bool StartTimeShifting(TVChannel channel, string fileName)
    {

      if (_graphState != State.Created)
        return false;
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Start timeshifting: {0}", channel.Name);

      int hr = 0;

      if (channel != null)
        TuneChannel(channel);

      if (_channelFound == false)
        return false;

      if (_vmr9 != null)
      {
        _vmr9.RemoveVMR9();
        _vmr9.Release();
        _vmr9 = null;
      }
      if (_vmr7 != null)
      {
        _vmr7.RemoveVMR7();
        _vmr7 = null;
      }
      _isUsingAc3 = TVDatabase.DoesChannelHaveAC3(channel, Network() == NetworkType.DVBC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBS, Network() == NetworkType.ATSC);

#if USEMTSWRITER
			if (CreateMTSWriter(fileName))
			{
				_interfaceMediaControl=(IMediaControl)_graphBuilder;
				hr=_interfaceMediaControl.Run();
				_graphState = State.TimeShifting;
        _isGraphRunning=true; 
			}
			else 
			{
				_graphState=State.Created;
				return false;
			}
#else
      if (CreateSinkSource(fileName, _isUsingAc3) == true)
      {
        _interfaceMediaControl = (IMediaControl)_graphBuilder;
        hr = _interfaceMediaControl.Run();
        _graphState = State.TimeShifting;
        _isGraphRunning = true; 
      }
      else
      {
        _graphState = State.Created;
        return false;
      }
#endif
      return true;
    }

    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting 
    /// </remarks>
    public bool StopTimeShifting()
    {
      if (_graphState != State.TimeShifting) return false;
      DirectShowUtil.DebugWrite("DVBGraphSS2:StopTimeShifting()");
      if (_interfaceMediaControl != null)
        _interfaceMediaControl.Stop();
      _isGraphRunning = false; 
      _graphState = State.Created;
      DeleteGraph();
      return true;
    }


    /// <summary>
    /// Starts recording live TV to a file
    /// <param name="strFileName">filename for the new recording</param>
    /// <param name="bContentRecording">Specifies whether a content or reference recording should be made</param>
    /// <param name="timeProgStart">Contains the starttime of the current tv program</param>
    /// </summary>
    /// <returns>boolean indicating if recorded is started or not</returns> 
    /// <remarks>
    /// Graph should be timeshifting. When Recording is started the graph is still 
    /// timeshifting
    /// 
    /// A content recording will start recording from the moment this method is called
    /// and ignores any data left/present in the timeshifting buffer files
    /// 
    /// A reference recording will start recording from the moment this method is called
    /// It will examine the timeshifting files and try to record as much data as is available
    /// from the timeProgStart till the moment recording is stopped again
    /// </remarks>
    public bool StartRecording(Hashtable attribtutes, TVRecording recording, TVChannel channel, ref string strFilename, bool bContentRecording, DateTime timeProgStart)
    {
      if (_graphState != State.TimeShifting) return false;
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Start recording: {0}", channel.Name);
#if USEMTSWRITER
			if (_interfaceTsRecord==null) 
			{
				return false;
			}

			if (_vmr9!=null)
			{
				_vmr9.RemoveVMR9();
				_vmr9.Release();
				_vmr9=null;
			}
			Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA:StartRecording()");
			strFilename=System.IO.Path.ChangeExtension(strFilename,".ts");
			int hr=_interfaceTsRecord.SetRecordingFileName(strFilename);
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:unable to set filename:%x", hr);
				return false;
			}

			long lStartTime=0;
			if (!bContentRecording)
			{
				// if start of program is given, then use that as our starttime
				if (timeProgStart.Year > 2000)
				{
					//how many seconds are present in the timeshift buffer?
					long timeInBuffer;
					_interfaceTsWriter.TimeShiftBufferDuration(out timeInBuffer); // get the amount of time in the timeshiftbuffer
					if (timeInBuffer>0) timeInBuffer/=10000000;

					//how many seconds in the past we want to record?
					TimeSpan ts = DateTime.Now - timeProgStart;
					lStartTime=(long)ts.TotalSeconds;

					//does timeshift buffer contain all this info, if not then limit it
					if (lStartTime > timeInBuffer)
						lStartTime=timeInBuffer;
					

					DateTime dtStart = DateTime.Now;
					dtStart=dtStart.AddSeconds( - lStartTime);
					ts=DateTime.Now-dtStart;
					Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
						dtStart.Hour, dtStart.Minute, dtStart.Second,
						ts.Hours, ts.Minutes, ts.Seconds);
															
					lStartTime *= 10000000;
				}
			}

			if(lStartTime<1) lStartTime=1;

			//hr=_interfaceTsRecord.StartRecord(lStartTime);
			if (hr!=0)
			{
				Log.Write("DVBGraphBDA:unable to start recording:%x", hr);
				return false;
			}
			_graphState=State.Recording;
			return true;
#else
      if (_vmr9 != null)
      {
        _vmr9.RemoveVMR9();
        _vmr9.Release();
        _vmr9 = null;
      }
      try
      {
        uint iRecordingType = 0;
        if (bContentRecording) iRecordingType = 0;
        else iRecordingType = 1;

        bool success = DvrMsCreate(out _recordedId, (IBaseFilter)_filterStreamBuffer, strFilename, iRecordingType);
        if (!success)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:StartRecording() FAILED to create recording");
          return false;
        }
        long lStartTime = 0;

        // if we're making a reference recording
        // then record all content from the past as well
        if (!bContentRecording)
        {
          // so set the startttime...
          uint uiSecondsPerFile;
          uint uiMinFiles, uiMaxFiles;
          _interfaceStreamBufferConfig.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
          _interfaceStreamBufferConfig.GetBackingFileDuration(out uiSecondsPerFile);
          lStartTime = uiSecondsPerFile;
          lStartTime *= (long)uiMaxFiles;

          // if start of program is given, then use that as our starttime
          if (timeProgStart.Year > 2000)
          {
            TimeSpan ts = DateTime.Now - timeProgStart;
            DirectShowUtil.DebugWrite("mpeg2:Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
              timeProgStart.Hour, timeProgStart.Minute, timeProgStart.Second,
              ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);

            lStartTime = (long)ts.TotalSeconds;
          }
          else DirectShowUtil.DebugWrite("mpeg2:record entire timeshift buffer");

          TimeSpan tsMaxTimeBack = DateTime.Now - _startTime;
          if (lStartTime > tsMaxTimeBack.TotalSeconds)
          {
            lStartTime = (long)tsMaxTimeBack.TotalSeconds;
          }


          lStartTime *= -10000000L;//in reference time 
        }
        /*
        foreach (MetadataItem item in attribtutes.Values)
        {
          try
          {
          if (item.Type == MetadataItemType.String)
            m_recorder.SetAttributeString(item.Name,item.Value.ToString());
          if (item.Type == MetadataItemType.Dword)
            m_recorder.SetAttributeDWORD(item.Name,UInt32.Parse(item.Value.ToString()));
          }
          catch(Exception){}
        }*/
        DvrMsStart(_recordedId, (uint)lStartTime);
      }
      catch (Exception ex)
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:Failed to start recording :{0} {1} {2}",
          ex.Message, ex.Source, ex.StackTrace);
      }
      _graphState = State.Recording;
      return true;
#endif

    }


    /// <summary>
    /// Stops recording 
    /// </summary>
    /// <remarks>
    /// Graph should be recording. When Recording is stopped the graph is still 
    /// timeshifting
    /// </remarks>
    public void StopRecording()
    {
      if (_graphState != State.Recording)
        return;

      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Stop recording: ");
      if (_recordedId >= 0)
      {
        DvrMsStop(_recordedId);
        _recordedId = -1;
      }

#if USEMTSWRITER
			if (_interfaceTsRecord!=null)
			{
				_interfaceTsRecord.StopRecord(0);
			}
#endif
      _graphState = State.TimeShifting;
      return;

    }

    //
    //

    public void TuneChannel(TVChannel channel)
    {

      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Tune: {0}", channel.Name);
      try
      {
        if (_vmr9 != null) _vmr9.Enable(false);
        if (_graphState == State.Recording)
          return;

        int channelID = channel.ID;
        _channelNumber = channel.Number;
        if (channelID != -1)
        {

          DVBChannel ch = new DVBChannel();
          ch.ServiceName = channel.Name;
          switch (_networkType)
          {
            case NetworkType.DVBS:
              {
                if (TVDatabase.GetSatChannel(channelID, 1, ref ch) == false)//only television
                {
                  Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Tune: channel not found in database (idChannel={0})", channelID);
                  _channelFound = false;
                  return;
                }
                break;
              }
            case NetworkType.DVBC:
              {
                if (TVDatabase.GetDVBCChannel(channelID, ref ch) == false)//only television
                {
                  Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Tune: channel not found in database (idChannel={0})", channelID);
                  _channelFound = false;
                  return;
                }
                break;
              }
            case NetworkType.DVBT:
              {
                if (TVDatabase.GetDVBTChannel(channelID, ref ch) == false)//only television
                {
                  Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Tune: channel not found in database (idChannel={0})", channelID);
                  _channelFound = false;
                  return;
                }
                break;
              }
            case NetworkType.ATSC:
              {
                if (TVDatabase.GetATSCChannel(channelID, ref ch) == false)//only television
                {
                  Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Tune: channel not found in database (idChannel={0})", channelID);
                  _channelFound = false;
                  return;
                }
                break;
              }
          }
          if (_pluginsEnabled == false && ch.IsScrambled == true)
          {
            _channelFound = false;
            return;
          }
          _channelFound = true;
          _currentChannel = ch;
          _selectedAudioPid = ch.AudioPid;
          if (Tune(ch.Frequency, ch.Symbolrate, 6, ch.Polarity, ch.LNBKHz, ch.DiSEqC, ch.AudioPid, ch.VideoPid, ch.LNBFrequency, ch.ECMPid, ch.TeletextPid, ch.PMTPid, ch.PCRPid, ch.AudioLanguage3, ch.Audio3, ch.ProgramNumber, ch) == false)
          {
            _lastTuneFailed = true;
            _channelFound = false;
            return;
          }

          _lastTuneFailed = false;
          if (_dvbDemuxer != null)
          {
            _dvbDemuxer.OnTuneNewChannel();
            _dvbDemuxer.SetChannelData(ch.AudioPid, ch.VideoPid, ch.TeletextPid, ch.Audio3, ch.ServiceName, ch.PMTPid, ch.ProgramNumber);
          }
          if (_interfaceMediaControl != null && _filterMpeg2DemuxerVideoPin != null && _filterMpeg2DemuxerAudioPin != null && _filterMpeg2Demuxer != null && _filterMpeg2DemuxerInterface != null)
          {

            int hr = SetupDemuxer(_filterMpeg2DemuxerVideoPin, ch.VideoPid, _filterMpeg2DemuxerAudioPin, ch.AudioPid, _pinAc3Audio, ch.AC3Pid);
            if (hr != 0)
            {
              Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: SetupDemuxer FAILED: errorcode {0}", hr.ToString());
              return;
            }
          }

          //SetMediaType();
          //m_gotAudioFormat=false;
          _interfaceStreamAnalyser.ResetParser();
          _startTime = DateTime.Now;

          SetupMTSDemuxerPin();
        }
      }
      finally
      {
        if (_vmr9 != null) _vmr9.Enable(true);
        _timerSignalLost = DateTime.Now;
        if (_interfaceStreamBufferSink != null)
        {
          long refTime = 0;
          _interfaceStreamBufferSink.SetAvailableFilter(ref refTime);
        }
      }
    }
    void SetDemux(int audioPid, int videoPid, int ac3Pid)
    {

      if (_filterMpeg2DemuxerInterface == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: SetDemux FAILED: no Demux-Interface");
        return;
      }
      int hr = 0;

      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:SetDemux() audio pid:0x{0:X} video pid:0x{1:X}", audioPid, videoPid);
      AMMediaType mpegVideoOut = new AMMediaType();
      mpegVideoOut.majorType = MediaType.Video;
      mpegVideoOut.subType = MediaSubType.MPEG2_Video;

      Size FrameSize = new Size(100, 100);
      mpegVideoOut.unkPtr = IntPtr.Zero;
      mpegVideoOut.sampleSize = 0;
      mpegVideoOut.temporalCompression = false;
      mpegVideoOut.fixedSizeSamples = true;

      //Mpeg2ProgramVideo=new byte[Mpeg2ProgramVideo.GetLength(0)];
      mpegVideoOut.formatType = FormatType.Mpeg2Video;
      mpegVideoOut.formatSize = Mpeg2ProgramVideo.GetLength(0);
      mpegVideoOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegVideoOut.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(Mpeg2ProgramVideo, 0, mpegVideoOut.formatPtr, mpegVideoOut.formatSize);

      AMMediaType mpegAudioOut = new AMMediaType();
      mpegAudioOut.majorType = MediaType.Audio;
      mpegAudioOut.subType = MediaSubType.MPEG2_Audio;
      mpegAudioOut.sampleSize = 0;
      mpegAudioOut.temporalCompression = false;
      mpegAudioOut.fixedSizeSamples = true;
      mpegAudioOut.unkPtr = IntPtr.Zero;
      mpegAudioOut.formatType = FormatType.WaveEx;
      mpegAudioOut.formatSize = MPEG1AudioFormat.GetLength(0);
      mpegAudioOut.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mpegAudioOut.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mpegAudioOut.formatPtr, mpegAudioOut.formatSize);
      ////IPin pinVideoOut,pinAudioOut;
      AMMediaType mediaAC3 = new AMMediaType();
      mediaAC3.majorType = MediaType.Audio;
      mediaAC3.subType = MediaSubType.DolbyAC3;
      mediaAC3.sampleSize = 0;
      mediaAC3.temporalCompression = false;
      mediaAC3.fixedSizeSamples = false;
      mediaAC3.unkPtr = IntPtr.Zero;
      mediaAC3.formatType = FormatType.WaveEx;
      mediaAC3.formatSize = MPEG1AudioFormat.GetLength(0);
      mediaAC3.formatPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(mediaAC3.formatSize);
      System.Runtime.InteropServices.Marshal.Copy(MPEG1AudioFormat, 0, mediaAC3.formatPtr, mediaAC3.formatSize);

      hr = _filterMpeg2DemuxerInterface.CreateOutputPin(ref mediaAC3/*vidOut*/, "AC3", out _pinAc3Audio);
      if (hr != 0 || _pinAc3Audio == null)
      {
        Log.WriteFile(Log.LogType.Capture, true, "mpeg2:FAILED to create AC3 pin:0x{0:X}", hr);
      }

      hr = _filterMpeg2DemuxerInterface.CreateOutputPin(ref mpegVideoOut/*vidOut*/, "video", out _filterMpeg2DemuxerVideoPin);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED to create video output pin on demuxer");
        return;
      }
      hr = _filterMpeg2DemuxerInterface.CreateOutputPin(ref mpegAudioOut, "audio", out _filterMpeg2DemuxerAudioPin);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED to create audio output pin on demuxer");
        return;
      }

      hr = SetupDemuxer(_filterMpeg2DemuxerVideoPin, videoPid, _filterMpeg2DemuxerAudioPin, audioPid, _pinAc3Audio, ac3Pid);
      if (hr != 0)//ignore audio pin
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: FAILED to config Demuxer");
        return;
      }



      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:SetDemux() done:{0}", hr);
      //int //=0;
    }
    /// <summary>
    /// Returns the current tv channel
    /// </summary>
    /// <returns>Current channel</returns>
    public int GetChannelNumber()
    {
      return _channelNumber;
    }

    /// <summary>
    /// Property indiciating if the graph supports timeshifting
    /// </summary>
    /// <returns>boolean indiciating if the graph supports timeshifting</returns>
    public bool SupportsTimeshifting()
    {
      return true;
    }


    /// <summary>
    /// Starts viewing the TV channel 
    /// </summary>
    /// <param name="iChannelNr">TV channel to which card should be tuned</param>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public bool StartViewing(TVChannel channel)
    {
      if (_graphState != State.Created) return false;
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() {0}", channel.Name);
      TuneChannel(channel);
      int hr = 0;
      bool setVisFlag = false;

      m_bOverlayVisible = true;
      if (_channelFound == false)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() channel not found");
        return false;
      }
      AddPreferredCodecs(true, true);

      if (_vmr9 != null)
      {
        if (_vmr9.UseVMR9inMYTV)
        {
          GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
          GUIWindowManager.SendMessage(msg);
          _vmr9.AddVMR9(_graphBuilder);
          if (_vmr9.VMR9Filter == null)
          {
            _vmr9.RemoveVMR9();
            _vmr9.Release();
            _vmr9 = null;
            _vmr7.AddVMR7(_graphBuilder);
          }
        }
        else _vmr7.AddVMR7(_graphBuilder);
      }
      else _vmr7.AddVMR7(_graphBuilder);


      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() ");
      //connect capture->sample grabber
      IPin samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Input, 0);
      if (samplePin == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find samplePin");
        return false;
      }

      hr = _graphBuilder.Connect(_pinData0, samplePin);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot connect data0->samplepin");
        return false;
      }

      //connect sample grabber->demuxer
      IPin demuxInPin = DirectShowUtil.FindPinNr(_filterMpeg2Demuxer, PinDirection.Input, 0);
      if (demuxInPin == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demuxInPin");
        return false;
      }

      samplePin = null;
      samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Output, 0);
      if (samplePin == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find sampleGrabber output pin");
        return false;
      }
      hr = _graphBuilder.Connect(samplePin, demuxInPin);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: connect sample->demux");
        return false;
      }

      //setup demuxer
      SetDemux(_currentChannel.AudioPid, _currentChannel.VideoPid, _currentChannel.AC3Pid);

      if (_filterMpeg2DemuxerVideoPin == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demux video output pin");
        return false;
      }
      if (_filterMpeg2DemuxerAudioPin == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demux audio output pin");
        return false;
      }

      hr = _graphBuilder.Render(_filterMpeg2DemuxerVideoPin);
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot render demux video output pin");
        return false;
      }
      _isUsingAc3 = TVDatabase.DoesChannelHaveAC3(channel, Network() == NetworkType.DVBC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBS, Network() == NetworkType.ATSC);
      if (!_isUsingAc3)
      {
        if (_graphBuilder.Render(_filterMpeg2DemuxerAudioPin) != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:Failed to render audio out pin MPEG-2 Demultiplexer");
          return false;
        }
      }
      else
      {
        if (_pinAc3Audio == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demux audio ac3 output pin");
          return false;
        }
        if (_graphBuilder.Render(_pinAc3Audio) != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2:Failed to render AC3 pin MPEG-2 Demultiplexer");
          return false;
        }
      }

      //
      if (demuxInPin != null)
        Marshal.ReleaseComObject(demuxInPin);
      if (samplePin != null)
        Marshal.ReleaseComObject(samplePin);

      //

      bool useOverlay = true;
      if (_vmr9 != null)
      {
        if (_vmr9.IsVMR9Connected)
        {
          useOverlay = false;
          _vmr9.SetDeinterlaceMode();
        }
        else
        {
          _vmr9.RemoveVMR9();
          _vmr9.Release();
          _vmr9 = null;
        }
      }

      //
      //
      //
      //
      _interfaceMediaControl = (IMediaControl)_graphBuilder;
      if (useOverlay)
      {
        _interfaceVideoWindow = (IVideoWindow)_graphBuilder as IVideoWindow;
        if (_interfaceVideoWindow == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:FAILED:Unable to get IVideoWindow");
        }

        _interfaceBasicVideo = (IBasicVideo2)_graphBuilder as IBasicVideo2;
        if (_interfaceBasicVideo == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:FAILED:Unable to get IBasicVideo2");
        }
        hr = _interfaceVideoWindow.put_Owner(GUIGraphicsContext.form.Handle);
        if (hr != 0)
        {
          DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:set Video window:0x{0:X}", hr);
        }
        hr = _interfaceVideoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS);
        if (hr != 0)
        {
          DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:set Video window style:0x{0:X}", hr);
        }
        setVisFlag = true;

      }
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;

      }

      m_bOverlayVisible = true;
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      _graphState = State.Viewing;
      GUIGraphicsContext_OnVideoWindowChanged();
      //

      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() start graph");

      _interfaceMediaControl.Run();
      _isGraphRunning = true; 

      if (setVisFlag)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() show video window");
        hr = _interfaceVideoWindow.put_Visible(DsHlp.OATRUE);
        if (hr != 0)
        {
          DirectShowUtil.DebugWrite("DVBGraphSS2:FAILED:put_Visible:0x{0:X}", hr);
        }

      }
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() startviewing done");
      return true;

    }

    int GetPidNumber(string pidText, int number)
    {
      if (pidText == "")
        return 0;
      string[] pidSegments;

      pidSegments = pidText.Split(new char[] { ';' });
      if (pidSegments.Length - 1 < number || pidSegments.Length == 0)
        return -1;

      string[] pid = pidSegments[number - 1].Split(new char[] { '/' });
      if (pid.Length != 2)
        return -1;

      try
      {
        return Convert.ToInt16(pid[0]);
      }
      catch
      {
        return -1;
      }
    }
    int GetPidID(string pidText, int number)
    {
      if (pidText == "")
        return 0;
      string[] pidSegments;

      pidSegments = pidText.Split(new char[] { ';' });
      if (pidSegments.Length - 1 < number || pidSegments.Length == 0)
        return 0;

      string[] pid = pidSegments[number - 1].Split(new char[] { '/' });
      if (pid.Length != 2)
        return 0;

      try
      {
        return Convert.ToInt16(pid[1]);
      }
      catch
      {
        return 0;
      }
    }

    /// <summary>
    /// Stops viewing the TV channel 
    /// </summary>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be viewing first with StartViewing()
    /// </remarks>
    public bool StopViewing()
    {
      if (_graphState != State.Viewing)
        return false;

      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      DirectShowUtil.DebugWrite("DVBGraphSS2:StopViewing()");
      if (_interfaceVideoWindow != null)
        _interfaceVideoWindow.put_Visible(DsHlp.OAFALSE);
      m_bOverlayVisible = false;

      if (_vmr9 != null)
      {
        _vmr9.Enable(false);
      }
      if (_interfaceMediaControl != null)
      {
        _interfaceMediaControl.Stop();
        _interfaceMediaControl = null;
        _isGraphRunning = false; 
      }
      _graphState = State.Created;
      DeleteGraph();
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);

      return true;
    }

    //
    public bool ShouldRebuildGraph(TVChannel newChannel)
    {
      //check if we switch from an channel with AC3 to a channel without AC3
      //or vice-versa. ifso, graphs should be rebuild
      if (_graphState != State.Viewing && _graphState != State.TimeShifting && _graphState != State.Recording) return false;
      bool useAC3 = TVDatabase.DoesChannelHaveAC3(newChannel, Network() == NetworkType.DVBC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBS, Network() == NetworkType.ATSC);
      if (useAC3 != _isUsingAc3) return true;
      return false;
    }

    /// <summary>
    /// This method returns whether a signal is present. Meaning that the
    /// TV tuner (or video input) is tuned to a channel
    /// </summary>
    /// <returns>true:  tvtuner is tuned to a channel (or video-in has a video signal)
    ///          false: tvtuner is not tuned to a channel (or video-in has no video signal)
    /// </returns>
    public bool SignalPresent()
    {
      return (_lastTuneFailed == true ? false : true);
    }

    public int SignalQuality()
    {
      return _signalQuality;
    }
    void UpdateSignalQuality()
    {
      if (_graphState == State.None) return;
      if (_interfaceB2C2TunerCtrl == null) return;
      int level;
      int quality;
      GetSNR(_interfaceB2C2TunerCtrl, out level, out quality);
      _signalQuality = quality;
    }

    public int SignalStrength()
    {
      return 100;
    }

    /// <summary>
    /// This method returns the frequency to which the tv tuner is currently tuned
    /// </summary>
    /// <returns>frequency in Hertz
    /// </returns>
    public long VideoFrequency()
    {
      return 0;
    }

    void CheckVideoResolutionChanges()
    {
      if (GUIGraphicsContext.Vmr9Active) return;
      if (_graphState != State.Viewing) return;
      if (_interfaceVideoWindow == null || _interfaceBasicVideo == null) return;
      int aspectX, aspectY;
      int videoWidth = 1, videoHeight = 1;
      if (_interfaceBasicVideo != null)
      {
        _interfaceBasicVideo.GetVideoSize(out videoWidth, out videoHeight);
      }
      aspectX = videoWidth;
      aspectY = videoHeight;
      if (_interfaceBasicVideo != null)
      {
        _interfaceBasicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
      }
      if (videoHeight != _videoHeight || videoWidth != _videoWidth ||
        aspectX != _aspectRatioX || aspectY != _aspectRatioY)
      {
        GUIGraphicsContext_OnVideoWindowChanged();
      }

    }
    void UpdateVideoState()
    {
      //check if this card is used for watching tv
      bool isViewing = Recorder.IsCardViewing(_cardId);
      if (!isViewing) return;
      TimeSpan ts = DateTime.Now - _timerSignalLost;

      if (ts.TotalSeconds < 10)
      {
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
        _timerSignalLost2 = DateTime.Now;
        return;
      }
      ts = DateTime.Now - _timerSignalLost2;
      if (ts.TotalSeconds < 2) return;
      _timerSignalLost2 = DateTime.Now;

      //			Log.Write("demuxer:{0} signal:{1} fps:{2}",_dvbDemuxer.RecevingPackets,SignalPresent() ,GUIGraphicsContext._vmr9FPS);

      // do we receive any packets?
      if (!_dvbDemuxer.RecevingPackets)
      {
        //no, then state = no signal
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
      }
      else
      {
        // we receive packets, got a PMT.
        // is channel scrambled ?
        if (_dvbDemuxer.IsScrambled)
        {
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.Scrambled;
        }
        else
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      }
    }

    public void Process()
    {
      if (_graphState == State.None) return;
      UpdateSignalQuality();

      _epgGrabber.Process();
      if (_epgGrabber.Done)
      {
        _epgGrabber.Reset();
        if (_graphState == State.Epg)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:EPG done");
          _interfaceMediaControl.Stop();
          _isGraphRunning = false; 
          //_graphState = State.Created;
          return;
        }
      }
      if (_graphState == State.Created) return;
      if (_graphState == State.Epg) return;

      if (!GUIGraphicsContext.Vmr9Active && _vmr7 != null && _graphState == State.Viewing)
      {
        _vmr7.Process();
      }
      //_epgGrabber.GrabEPG(_currentChannel.HasEITSchedule==true);
      if (_dvbDemuxer != null) _dvbDemuxer.Process();
      CheckVideoResolutionChanges();


      UpdateVideoState();

      if (_currentChannel != null)
      {
        IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
        if (pmtMem != IntPtr.Zero)
        {
          //get the PMT
          _interfaceStreamAnalyser.SetPMTProgramNumber(_currentChannel.ProgramNumber);
          int res = _interfaceStreamAnalyser.GetPMTData(pmtMem);
          if (res != -1)
          {
            //check PMT version
            byte[] pmt = new byte[res];
            int version = -1;
            Marshal.Copy(pmtMem, pmt, 0, res);
            version = ((pmt[5] >> 1) & 0x1F);
            int pmtProgramNumber = (pmt[3] << 8) + pmt[4];
            if (pmtProgramNumber == _currentChannel.ProgramNumber)
            {
              if (_lastPmtVersion != version)
              {
                //decode pmt
                _lastPmtVersion = version;
                DVBSections sections = new DVBSections();
                DVBSections.ChannelInfo info = new DVBSections.ChannelInfo();
                if (sections.GetChannelInfoFromPMT(pmt, ref info))
                {
                  //map pids
                  if (info.pid_list != null)
                  {

                    DeleteAllPIDs(_interfaceB2C2DataCtrl, 0);
                    //										DeleteAllPIDs(_interfaceB2C2DataCtrl,0);
                    SetPidToPin(_interfaceB2C2DataCtrl, 0, 0);
                    SetPidToPin(_interfaceB2C2DataCtrl, 0, 0x10);
                    SetPidToPin(_interfaceB2C2DataCtrl, 0, 0x11);
                    SetPidToPin(_interfaceB2C2DataCtrl, 0, 0x12);
                    SetPidToPin(_interfaceB2C2DataCtrl, 0, 0xd2);
                    SetPidToPin(_interfaceB2C2DataCtrl, 0, 0xd3);
                    SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)_currentChannel.PMTPid);
                    if (_currentChannel.PCRPid > 0)
                      SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)_currentChannel.PCRPid);
                    for (int pids = 0; pids < info.pid_list.Count; pids++)
                    {
                      DVBSections.PMTData data = (DVBSections.PMTData)info.pid_list[pids];
                      if (data.elementary_PID > 0 && (data.isAC3Audio || data.isAudio || data.isVideo || data.isTeletext))
                      {
                        SetPidToPin(_interfaceB2C2DataCtrl, 0, (ushort)data.elementary_PID);
                      }
                    }
                    _epgGrabber.GrabEPG(_currentChannel.HasEITSchedule == true);
                  }
                }
              }
            }
          }
        }
        Marshal.FreeCoTaskMem(pmtMem);
      }
    }

    public PropertyPageCollection PropertyPages()
    {
      return null;
    }


    public bool SupportsFrameSize(Size framesize)
    {
      return false;
    }
    public NetworkType Network()
    {
      return _networkType;
    }
    //
    public void Tune(object tuningObject, int disecqNo)
    {
      DVBChannel ch = (DVBChannel)tuningObject;
      ch = LoadDiseqcSettings(ch, disecqNo);
      _currentTuningObject = new DVBChannel();
      if (_filterDvbAnalyzer == null)
        return;
      try
      {
        if (_interfaceMediaControl == null)
        {
          _graphBuilder.Render(_pinData0);
          _interfaceMediaControl = _graphBuilder as IMediaControl;
          _interfaceMediaControl.Run();
          _isGraphRunning = true; 
        }
      }
      catch { }
      if (Tune(ch.Frequency, ch.Symbolrate, 6, ch.Polarity, ch.LNBKHz, ch.DiSEqC, -1, -1, ch.LNBFrequency, 0, 0, 0, 0, "", 0, 0, ch) == false)
      {
        _lastTuneFailed = true;
        Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: FAILED to tune channel");
        return;
      }
      else
      {
        DeleteAllPIDs(_interfaceB2C2DataCtrl, 0);
        SetPidToPin(_interfaceB2C2DataCtrl, 0, 0x2000);
        _lastTuneFailed = false;
        Log.WriteFile(Log.LogType.Capture, "called Tune(object)");
      }
      _currentTuningObject = ch;
      _interfaceStreamAnalyser.ResetParser();
    }
    //
    public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels, ref int newRadioChannels, ref int updatedRadioChannels)
    {
      Log.WriteFile(Log.LogType.Capture, "called StoreChannels()");
      if (_filterDvbAnalyzer == null) return;


      //get list of current tv channels present in the database
      List<TVChannel> tvChannels = new List<TVChannel>();
      TVDatabase.GetChannels(ref tvChannels);

      _interfaceStreamAnalyser.ResetParser(); //analyser will map pids needed
      Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: StoreChannels()");
      DVBSections.Transponder transp;
      transp.channels = null;

      Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: get channels()");
      System.Threading.Thread.Sleep(2500);
      using (DVBSections sections = new DVBSections())
      {
        ushort count = 0;
        sections.DemuxerObject = _dvbDemuxer;
        sections.Timeout = 2500;

        for (int i = 0; i < 100; ++i)
        {
          bool allFound = true;
          _interfaceStreamAnalyser.GetChannelCount(ref count);
          if (count > 0)
          {
            for (int t = 0; t < count; t++)
            {
              if (_interfaceStreamAnalyser.IsChannelReady(t) != 0)
              {
                allFound = false;
                break;
              }
            }
          }
          else allFound = false;
          if (!allFound) System.Threading.Thread.Sleep(50);
        }

        _interfaceStreamAnalyser.GetChannelCount(ref count);
        if (count > 0)
        {
          transp.channels = new ArrayList();
          for (int t = 0; t < count; t++)
          {
            if (_interfaceStreamAnalyser.IsChannelReady(t) == 0)
            {
              DVBSections.ChannelInfo chi = new MediaPortal.TV.Recording.DVBSections.ChannelInfo();
              UInt16 len = 0;
              int hr = 0;
              hr = _interfaceStreamAnalyser.GetCISize(ref len);
              IntPtr mmch = Marshal.AllocCoTaskMem(len);
              hr = _interfaceStreamAnalyser.GetChannel((UInt16)t, mmch);
              //byte[] ch=new byte[len];
              //Marshal.Copy(mmch,ch,0,len);
              chi = sections.GetChannelInfo(mmch);
              chi.fec = _currentTuningObject.FEC;
              if (Network() != NetworkType.ATSC)
              {
                chi.freq = _currentTuningObject.Frequency;
              }
              else
              {
                _currentTuningObject.Frequency = 0;
                _currentTuningObject.Modulation = chi.modulation;
              }
              Marshal.FreeCoTaskMem(mmch);
              transp.channels.Add(chi);
            }
            else Log.Write("DVBGraphSS2:channel {0} is not ready!!!", t);
          }
        }
      }

      if (transp.channels == null)
      {
        Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: found no channels", transp.channels);
        return;
      }
      Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: found {0} channels", transp.channels.Count);
      for (int i = 0; i < transp.channels.Count; ++i)
      {
        _currentTuningObject.AC3Pid = -1;
        _currentTuningObject.VideoPid = 0;
        _currentTuningObject.AudioPid = 0;
        _currentTuningObject.TeletextPid = 0;
        _currentTuningObject.Audio1 = 0;
        _currentTuningObject.Audio2 = 0;
        _currentTuningObject.Audio3 = 0;
        _currentTuningObject.AudioLanguage = String.Empty;
        _currentTuningObject.AudioLanguage1 = String.Empty;
        _currentTuningObject.AudioLanguage2 = String.Empty;
        _currentTuningObject.AudioLanguage3 = String.Empty;
        System.Windows.Forms.Application.DoEvents();
        System.Windows.Forms.Application.DoEvents();


        int audioOptions = 0;

        DVBSections.ChannelInfo info = (DVBSections.ChannelInfo)transp.channels[i];
        if (info.service_provider_name == null) info.service_provider_name = "";
        if (info.service_name == null) info.service_name = "";

        info.service_provider_name = info.service_provider_name.Trim();
        info.service_name = info.service_name.Trim();
        if (info.service_provider_name.Length == 0)
          info.service_provider_name = Strings.Unknown;
        if (info.service_name.Length == 0)
          info.service_name = String.Format("NoName:{0}{1}{2}{3}", info.networkID, info.transportStreamID, info.serviceID, i);


        if (info.serviceID == 0)
        {
          Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: channel#{0} has no service id", i);
          continue;
        }
        bool hasAudio = false;
        bool hasVideo = false;
        info.freq = _currentTuningObject.Frequency;
        DVBChannel newchannel = new DVBChannel();

        //check if this channel has audio/video streams
        if (info.pid_list != null)
        {
          audioOptions = 0;
          for (int pids = 0; pids < info.pid_list.Count; pids++)
          {
            DVBSections.PMTData data = (DVBSections.PMTData)info.pid_list[pids];
            if (data.isAudio && hasAudio == true && audioOptions < 2)
            {
              switch (audioOptions)
              {
                case 0:
                  newchannel.Audio1 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      newchannel.AudioLanguage1 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions = 1;
                  break;
                case 1:
                  newchannel.Audio2 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      newchannel.AudioLanguage2 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions = 2;
                  break;

              }
            }
            if (data.isAC3Audio)
            {
              _currentTuningObject.AC3Pid = data.elementary_PID;
            }
            if (data.isVideo)
            {
              _currentTuningObject.VideoPid = data.elementary_PID;
              hasVideo = true;
            }
            if (data.isAudio && hasAudio == false)
            {
              _currentTuningObject.AudioPid = data.elementary_PID;
              if (data.data != null)
              {
                if (data.data.Length == 3)
                  newchannel.AudioLanguage = DVBSections.GetLanguageFromCode(data.data);
              }
              hasAudio = true;
            }
            if (data.isTeletext)
            {
              _currentTuningObject.TeletextPid = data.elementary_PID;
            }
            if (data.isDVBSubtitle)
            {
              _currentTuningObject.Audio3 = data.elementary_PID;
            }
          }
        }
        Log.WriteFile(Log.LogType.Capture, "auto-tune ss2:Found provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:{9} videopid:{10} teletextpid:{11}",
          info.service_provider_name,
          info.service_name,
          info.scrambled,
          info.freq,
          info.networkID,
          info.transportStreamID,
          info.serviceID,
          hasVideo, ((!hasVideo) && hasAudio),
          _currentTuningObject.AudioPid, _currentTuningObject.VideoPid, _currentTuningObject.TeletextPid);
        bool IsRadio = ((!hasVideo) && hasAudio);
        bool IsTv = (hasVideo);//some tv channels dont have an audio stream

        newchannel.Frequency = info.freq;
        newchannel.ServiceName = info.service_name;
        newchannel.ServiceProvider = info.service_provider_name;
        newchannel.IsScrambled = info.scrambled;
        newchannel.NetworkID = info.networkID;
        newchannel.TransportStreamID = info.transportStreamID;
        newchannel.ProgramNumber = info.serviceID;
        newchannel.FEC = info.fec;
        newchannel.Polarity = _currentTuningObject.Polarity;
        newchannel.Bandwidth = _currentTuningObject.Bandwidth;
        newchannel.Modulation = _currentTuningObject.Modulation;
        newchannel.Symbolrate = _currentTuningObject.Symbolrate;
        newchannel.ServiceType = info.serviceType;//tv
        newchannel.PCRPid = info.pcr_pid;
        newchannel.PMTPid = info.network_pmt_PID;
        newchannel.LNBFrequency = _currentTuningObject.LNBFrequency;
        newchannel.LNBKHz = _currentTuningObject.LNBKHz;
        newchannel.DiSEqC = _currentTuningObject.DiSEqC;
        newchannel.AudioPid = _currentTuningObject.AudioPid;
        newchannel.VideoPid = _currentTuningObject.VideoPid;
        newchannel.TeletextPid = _currentTuningObject.TeletextPid;
        newchannel.AC3Pid = _currentTuningObject.AC3Pid;
        newchannel.HasEITSchedule = info.eitSchedule;
        newchannel.HasEITPresentFollow = info.eitPreFollow;
        newchannel.AudioLanguage3 = info.pidCache;
        newchannel.Audio3 = _currentTuningObject.Audio3;


        if (info.serviceType != 1 && info.serviceType != 2) continue;
        if (info.serviceType == 1 && tv)
        {
          Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: channel {0} is a tv channel", newchannel.ServiceName);
          //check if this channel already exists in the tv database
          bool isNewChannel = true;
          int channelId = -1;
          TVChannel tvChan = new TVChannel();
          tvChan.Name = newchannel.ServiceName;
          foreach (TVChannel tvchan in tvChannels)
          {
            if (tvchan.Name.Equals(newchannel.ServiceName))
            {
              if (TVDatabase.DoesChannelExist(tvchan.ID, newchannel.TransportStreamID, newchannel.NetworkID))
              {
                //yes already exists
                tvChan = tvchan;
                isNewChannel = false;
                channelId = tvchan.ID;
                break;
              }
            }
          }

          //if the tv channel found is not yet in the tv database
          tvChan.Scrambled = newchannel.IsScrambled;
          if (isNewChannel)
          {
            //then add a new channel to the database
            tvChan.Number = TVDatabase.FindFreeTvChannelNumber(newchannel.ProgramNumber);
            tvChan.Sort = newchannel.ProgramNumber;
            Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: create new tv channel for {0}", newchannel.ServiceName);
            int id = TVDatabase.AddChannel(tvChan);
            channelId = id;
            newChannels++;
          }
          else
          {
            TVDatabase.UpdateChannel(tvChan, tvChan.Sort);
            updatedChannels++;
            Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: channel {0} already exists in tv database", newchannel.ServiceName);
          }
          if (Network() == NetworkType.DVBT)
          {
            //Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBT card:{2}",newchannel.ServiceName,channelId,ID);
            TVDatabase.MapDVBTChannel(newchannel.ServiceName,
              newchannel.ServiceProvider,
              channelId,
              newchannel.Frequency,
              newchannel.NetworkID,
              newchannel.TransportStreamID,
              newchannel.ProgramNumber,
              newchannel.AudioPid,
              newchannel.VideoPid,
              newchannel.TeletextPid,
              newchannel.PMTPid,
              newchannel.Bandwidth,
              newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid, newchannel.PCRPid,
              newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2, newchannel.AudioLanguage3,
              newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);
          }
          if (Network() == NetworkType.DVBC)
          {
            //Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
            TVDatabase.MapDVBCChannel(newchannel.ServiceName,
              newchannel.ServiceProvider,
              channelId,
              newchannel.Frequency,
              newchannel.Symbolrate,
              newchannel.FEC,
              newchannel.Modulation,
              newchannel.NetworkID,
              newchannel.TransportStreamID,
              newchannel.ProgramNumber,
              newchannel.AudioPid,
              newchannel.VideoPid,
              newchannel.TeletextPid,
              newchannel.PMTPid,
              newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid, newchannel.PCRPid,
              newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2, newchannel.AudioLanguage3,
              newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);

          }
          if (Network() == NetworkType.ATSC)
          {
            //Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to ATSC card:{2}",newchannel.ServiceName,channelId,ID);
            TVDatabase.MapATSCChannel(newchannel.ServiceName,
              newchannel.PhysicalChannel,
              newchannel.MinorChannel,
              newchannel.MajorChannel,
              newchannel.ServiceProvider,
              channelId,
              newchannel.Frequency,
              newchannel.Symbolrate,
              newchannel.FEC,
              newchannel.Modulation,
              newchannel.NetworkID,
              newchannel.TransportStreamID,
              newchannel.ProgramNumber,
              newchannel.AudioPid,
              newchannel.VideoPid,
              newchannel.TeletextPid,
              newchannel.PMTPid,
              newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid, newchannel.PCRPid,
              newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2, newchannel.AudioLanguage3,
              newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);

          }

          if (Network() == NetworkType.DVBS)
          {
            Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: map channel {0} id:{1} to DVBS card:{2}", newchannel.ServiceName, channelId, ID);
            newchannel.ID = channelId;
            TVDatabase.AddSatChannel(newchannel);
          }
          TVDatabase.MapChannelToCard(channelId, ID);

          TVGroup group = new TVGroup();
          if (info.scrambled)
          {
            group.GroupName = "Scrambled";
          }
          else
          {
            group.GroupName = "Unscrambled";
          }
          int groupid = TVDatabase.AddGroup(group);
          group.ID = groupid;
          TVChannel tvTmp = new TVChannel();
          tvTmp.Name = newchannel.ServiceName;
          tvTmp.Number = tvChan.Number;
          tvTmp.ID = channelId;
          TVDatabase.MapChannelToGroup(group, tvTmp);

          //make group for service provider
          group = new TVGroup();
          group.GroupName = newchannel.ServiceProvider;
          groupid = TVDatabase.AddGroup(group);
          group.ID = groupid;
          tvTmp = new TVChannel();
          tvTmp.Name = newchannel.ServiceName;
          tvTmp.Number = tvChan.Number;
          tvTmp.ID = channelId;
          TVDatabase.MapChannelToGroup(group, tvTmp);

        }
        else
        {
          if (info.serviceType == 2)
          {
            //todo: radio channels
            Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: channel {0} is a radio channel", newchannel.ServiceName);
            //check if this channel already exists in the radio database
            bool isNewChannel = true;
            int channelId = -1;
            ArrayList radioStations = new ArrayList();

            RadioDatabase.GetStations(ref radioStations);
            foreach (RadioStation station in radioStations)
            {
              if (station.Name.Equals(newchannel.ServiceName))
              {
                //yes already exists
                isNewChannel = false;
                channelId = station.ID;
                station.Scrambled = info.scrambled;
                RadioDatabase.UpdateStation(station);
                break;
              }
            }

            //if the tv channel found is not yet in the tv database
            if (isNewChannel)
            {
              //then add a new channel to the database
              Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: create new radio channel for {0}", newchannel.ServiceName);
              RadioStation station = new RadioStation();
              station.Name = newchannel.ServiceName;
              station.Channel = newchannel.ProgramNumber;
              station.Frequency = newchannel.Frequency;
              station.Scrambled = info.scrambled;
              int id = RadioDatabase.AddStation(ref station);
              channelId = id;
              newRadioChannels++;
            }
            else
            {
              updatedRadioChannels++;
              Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: channel {0} already exists in tv database", newchannel.ServiceName);
            }

            if (Network() == NetworkType.DVBT)
            {
              //Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBT card:{2}",newchannel.ServiceName,channelId,ID);
              RadioDatabase.MapDVBTChannel(newchannel.ServiceName, newchannel.ServiceProvider, channelId, newchannel.Frequency, newchannel.NetworkID, newchannel.TransportStreamID, newchannel.ProgramNumber, newchannel.AudioPid, newchannel.PMTPid, newchannel.Bandwidth, newchannel.PCRPid);
            }
            if (Network() == NetworkType.DVBC)
            {
              //Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
              RadioDatabase.MapDVBCChannel(newchannel.ServiceName, newchannel.ServiceProvider, channelId, newchannel.Frequency, newchannel.Symbolrate, newchannel.FEC, newchannel.Modulation, newchannel.NetworkID, newchannel.TransportStreamID, newchannel.ProgramNumber, newchannel.AudioPid, newchannel.PMTPid, newchannel.PCRPid);
            }
            if (Network() == NetworkType.ATSC)
            {
              //Log.WriteFile(Log.LogType.Capture,"DVBGraphBDA: map channel {0} id:{1} to DVBC card:{2}",newchannel.ServiceName,channelId,ID);
              RadioDatabase.MapATSCChannel(newchannel.ServiceName, newchannel.PhysicalChannel,
                newchannel.MinorChannel,
                newchannel.MajorChannel, newchannel.ServiceProvider, channelId, newchannel.Frequency, newchannel.Symbolrate, newchannel.FEC, newchannel.Modulation, newchannel.NetworkID, newchannel.TransportStreamID, newchannel.ProgramNumber, newchannel.AudioPid, newchannel.PMTPid, newchannel.PCRPid);
            }
            if (Network() == NetworkType.DVBS)
            {
              Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: map channel {0} id:{1} to DVBS card:{2}", newchannel.ServiceName, channelId, ID);
              newchannel.ID = channelId;

              int scrambled = 0;
              if (newchannel.IsScrambled) scrambled = 1;
              RadioDatabase.MapDVBSChannel(newchannel.ID, newchannel.Frequency, newchannel.Symbolrate,
                newchannel.FEC, newchannel.LNBKHz, newchannel.DiSEqC, newchannel.ProgramNumber,
                0, newchannel.ServiceProvider, newchannel.ServiceName,
                0, 0, newchannel.AudioPid, 0, newchannel.AC3Pid,
                0, 0, 0, 0, scrambled,
                newchannel.Polarity, newchannel.LNBFrequency
                , newchannel.NetworkID, newchannel.TransportStreamID, newchannel.PCRPid,
                newchannel.AudioLanguage, newchannel.AudioLanguage1,
                newchannel.AudioLanguage2, newchannel.AudioLanguage3,
                newchannel.ECMPid, newchannel.PMTPid);
            }
            RadioDatabase.MapChannelToCard(channelId, ID);
            Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: channel {0} is a radio channel", newchannel.ServiceName);
          }
        }
      }//for (int i=0; i < transp.channels.Count;++i)
      SetLCN();
    }

    void SetLCN()
    {
      Int16 count = 0;
      while (true)
      {
        Int16 networkId, transportId, serviceID, LCN;
        string provider;
        _interfaceStreamAnalyser.GetLCN(count, out  networkId, out transportId, out serviceID, out LCN);
        if (networkId > 0 && transportId > 0 && serviceID >= 0 && LCN > 0)
        {
          TVChannel channel = TVDatabase.GetTVChannelByStream(Network() == NetworkType.ATSC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBC, Network() == NetworkType.DVBS, networkId, transportId, serviceID, out provider);
          if (channel != null)
          {
            TVDatabase.SetChannelSort(channel.Name, LCN);
            TVGroup group = new TVGroup();
            if (channel.Scrambled)
            {
              group.GroupName = "Scrambled";
            }
            else
            {
              group.GroupName = "Unscrambled";
            }
            int groupid = TVDatabase.AddGroup(group);
            group.ID = groupid;
            TVDatabase.MapChannelToGroup(group, channel);

            group = new TVGroup();
            group.GroupName = provider;
            groupid = TVDatabase.AddGroup(group);
            group.ID = groupid;
            TVDatabase.MapChannelToGroup(group, channel);

          }
        }
        else
        {
          return;
        }
        count++;
      }
    }


    DVBChannel LoadDiseqcSettings(DVBChannel ch, int disNo)
    {
      if (_cardFilename == "")
        return ch;

      int lnbKhz = 0;
      int lnbKhzVal = 0;
      int diseqc = 0;
      int lnbKind = 0;
      // lnb config
      int lnb0MHZ = 0;
      int lnb1MHZ = 0;
      int lnbswMHZ = 0;
      int cbandMHZ = 0;
      int circularMHZ = 0;

      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml(_cardFilename))
      {
        lnb0MHZ = xmlreader.GetValueAsInt("dvbs", "LNB0", 9750);
        lnb1MHZ = xmlreader.GetValueAsInt("dvbs", "LNB1", 10600);
        lnbswMHZ = xmlreader.GetValueAsInt("dvbs", "Switch", 11700);
        cbandMHZ = xmlreader.GetValueAsInt("dvbs", "CBand", 5150);
        circularMHZ = xmlreader.GetValueAsInt("dvbs", "Circular", 10750);
        //				bool useLNB1=xmlreader.GetValueAsBool("dvbs","useLNB1",false);
        //				bool useLNB2=xmlreader.GetValueAsBool("dvbs","useLNB2",false);
        //				bool useLNB3=xmlreader.GetValueAsBool("dvbs","useLNB3",false);
        //				bool useLNB4=xmlreader.GetValueAsBool("dvbs","useLNB4",false);
        switch (disNo)
        {
          case 1:
            // config a
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb", 44);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc", 0);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind", 0);
            break;
          case 2:
            // config b
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb2", 44);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 0);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind2", 0);
            break;
          case 3:
            // config c
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb3", 44);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 0);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind3", 0);
            break;
          //
          case 4:
            // config d
            lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb4", 44);
            diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 0);
            lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind4", 0);
            //
            break;
        }// switch(disNo)
        switch (lnbKhz)
        {
          case 0: lnbKhzVal = 0; break;
          case 22: lnbKhzVal = 1; break;
          case 33: lnbKhzVal = 2; break;
          case 44: lnbKhzVal = 3; break;
        }


      }//using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(_cardFilename))

      // set values to dvbchannel-object
      ch.DiSEqC = diseqc;
      // set the lnb parameter 
      if (ch.Frequency >= lnbswMHZ * 1000)
      {
        ch.LNBFrequency = lnb1MHZ;
        ch.LNBKHz = lnbKhzVal;
      }
      else
      {
        ch.LNBFrequency = lnb0MHZ;
        ch.LNBKHz = 0;
      }
      Log.WriteFile(Log.LogType.Capture, "auto-tune ss2: freq={0} lnbKhz={1} lnbFreq={2} diseqc={3}", ch.Frequency, ch.LNBKHz, ch.LNBFrequency, ch.DiSEqC);
      return ch;

    }// LoadDiseqcSettings()

    public void TuneRadioChannel(RadioStation station)
    {
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:TuneChannel() get DVBS tuning details");
      DVBChannel ch = new DVBChannel();
      if (RadioDatabase.GetDVBSTuneRequest(station.ID, 0, ref ch) == false)//only radio
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:database invalid tuning details for channel:{0}", station.Channel);
        return;
      }
      if (Tune(ch.Frequency, ch.Symbolrate, ch.FEC, ch.Polarity, ch.LNBKHz, ch.DiSEqC, ch.AudioPid, 0, ch.LNBFrequency, 0, 0, ch.PMTPid, ch.PCRPid, ch.AudioLanguage3, 0, ch.ProgramNumber, ch) == true)
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: Radio tune ok");
      else
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: FAILED cannot tune");
        return;
      }

      _currentChannel = ch;

      if (_dvbDemuxer != null)
      {
        _dvbDemuxer.OnTuneNewChannel();
        _dvbDemuxer.SetChannelData(ch.AudioPid, ch.VideoPid, ch.TeletextPid, ch.Audio3, ch.ServiceName, ch.PMTPid, ch.ProgramNumber);
      }

      if (_filterMpeg2DemuxerVideoPin != null && _filterMpeg2DemuxerAudioPin != null)
        SetupDemuxer(_filterMpeg2DemuxerVideoPin, _currentChannel.VideoPid, _filterMpeg2DemuxerAudioPin, _currentChannel.AudioPid, _pinAc3Audio, _currentChannel.AC3Pid);

    }

    public void StartRadio(RadioStation station)
    {
      if (_graphState != State.Radio)
      {
        if (_graphState != State.Created)
          return;

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: start radio");


        if (_vmr9 != null)
        {
          _vmr9.RemoveVMR9();
          _vmr9 = null;
        }


#if USEMTSWRITER
				TuneRadioChannel(station);
				string fname=Recorder.GetTimeShiftFileNameByCardId(_cardId);
				StartTimeShifting(null,fname);
				SetupMTSDemuxerPin();
				return ;
			}
#else
        AddPreferredCodecs(true, false);

        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartRadio() Using plugins");
        IPin samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Input, 0);
        IPin demuxInPin = DirectShowUtil.FindPinNr(_filterMpeg2Demuxer, PinDirection.Input, 0);

        if (samplePin == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find samplePin");
          return;
        }
        if (demuxInPin == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot find demuxInPin");
          return;
        }

        int hr = _graphBuilder.Connect(_pinData0, samplePin);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartViewing() FAILED: cannot connect data0->samplepin");
          return;
        }
        samplePin = null;
        samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Output, 0);
        if (samplePin == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartRadio() FAILED: cannot find sampleGrabber output pin");
          return;
        }
        hr = _graphBuilder.Connect(samplePin, demuxInPin);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartRadio() FAILED: connect sample->demux");
          return;
        }

        SetDemux(_currentChannel.AudioPid, _currentChannel.VideoPid, _currentChannel.AC3Pid);

        if (_filterMpeg2DemuxerAudioPin == null)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartRadio() FAILED: cannot find demux audio output pin");
          return;
        }

        hr = _graphBuilder.Render(_filterMpeg2DemuxerAudioPin);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:StartRadio() FAILED: cannot render demux audio output pin");
          return;
        }
        //
        if (demuxInPin != null)
          Marshal.ReleaseComObject(demuxInPin);
        if (samplePin != null)
          Marshal.ReleaseComObject(samplePin);

        //


        //
        //
        _interfaceMediaControl = (IMediaControl)_graphBuilder;
        _graphState = State.Radio;
        //
        _interfaceMediaControl.Run();
        _isGraphRunning = true; 


      }

      // tune to the correct channel
      TuneRadioChannel(station);
#endif
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:Listening to radio..");
    }
    public void TuneRadioFrequency(int frequency)
    {
    }
    public bool HasTeletext()
    {
      if (_graphState != State.TimeShifting && _graphState != State.Recording && _graphState != State.Viewing) return false;
      if (_currentChannel == null) return false;
      if (_currentChannel.TeletextPid > 0) return true;
      return false;
    }
    #region Stream-Audio handling
    public int GetAudioLanguage()
    {
      return _selectedAudioPid;
    }
    public void SetAudioLanguage(int audioPid)
    {
      if (audioPid != _selectedAudioPid)
      {
        int hr = 0;
        if (audioPid == _currentChannel.AC3Pid)
        {
          hr = SetupDemuxer(_filterMpeg2DemuxerVideoPin, _currentChannel.VideoPid, _filterMpeg2DemuxerAudioPin, audioPid, _pinAc3Audio, audioPid);
        }
        else
        {
          hr = SetupDemuxer(_filterMpeg2DemuxerVideoPin, _currentChannel.VideoPid, _filterMpeg2DemuxerAudioPin, audioPid, _pinAc3Audio, _currentChannel.AC3Pid);
        }
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: SetupDemuxer FAILED: errorcode {0}", hr.ToString());
          return;
        }
        else
        {
          _selectedAudioPid = audioPid;
          if (_dvbDemuxer != null)
            _dvbDemuxer.SetChannelData(audioPid, _currentChannel.VideoPid, _currentChannel.TeletextPid, _currentChannel.Audio3, _currentChannel.ServiceName, _currentChannel.PMTPid, _currentChannel.ProgramNumber);

        }
      }
    }

    public ArrayList GetAudioLanguageList()
    {

      DVBSections.AudioLanguage al;
      ArrayList alList = new ArrayList();
      if (_currentChannel == null) return alList;
      if (_currentChannel.AudioPid != 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentChannel.AudioPid;
        al.AudioLanguageCode = _currentChannel.AudioLanguage;
        alList.Add(al);
      }
      if (_currentChannel.Audio1 != 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentChannel.Audio1;
        al.AudioLanguageCode = _currentChannel.AudioLanguage1;
        alList.Add(al);
      }
      if (_currentChannel.Audio2 != 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentChannel.Audio2;
        al.AudioLanguageCode = _currentChannel.AudioLanguage2;
        alList.Add(al);
      }
      if (_currentChannel.AC3Pid != 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentChannel.AC3Pid;
        al.AudioLanguageCode = "AC3";
        alList.Add(al);
      }
      return alList;
    }
    #endregion

    private void _dvbDemuxer_OnPMTIsChanged(byte[] pmtTable)
    {
    }

    private void _dvbDemuxer_OnGotSection(int pid, int tableID, byte[] sectionData)
    {
    }

    private void _dvbDemuxer_OnGotTable(int pid, int tableID, ArrayList tableList)
    {
      if (tableList == null)
        return;
      if (tableList.Count < 1)
        return;
    }

    public string TvTimeshiftFileName()
    {
#if USEMTSWRITER
			return "live.ts";
#else
      return "live.tv";
#endif
    }

    public string RadioTimeshiftFileName()
    {
#if USEMTSWRITER
			return "radio.ts";
#else
      return String.Empty;
#endif
    }
    public void GrabTeletext(bool yesNo)
    {
      if (_graphState == State.None || _graphState == State.Created) return;
      if (_dvbDemuxer == null) return;
      _dvbDemuxer.GrabTeletext(yesNo);
    }
    public IBaseFilter AudiodeviceFilter()
    {
      return null;
    }

    public bool IsTimeShifting()
    {
      return _graphState == State.TimeShifting;
    }

    public bool IsEpgDone()
    {
      if (_graphState == State.Epg && _isGraphRunning == false) return true;
      return false;
    }
    public bool IsEpgGrabbing()
    {
      return (_graphState == State.Epg);
    }

    public void GrabEpg(TVChannel channel)
    {

      // tune to the correct channel
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2:Grab epg for :{0}", channel.Name);
      TuneChannel(channel);


      // setup sampleGrabber and demuxer
      IPin samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Input, 0);
      IPin demuxInPin = DirectShowUtil.FindPinNr(_filterMpeg2Demuxer, PinDirection.Input, 0);
      int hr = _graphBuilder.Connect(_pinData0, samplePin); //SS2->sample grabber
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: failed to connect ss2->grabber");
        return;
      }
      samplePin = DirectShowUtil.FindPinNr(_filterSampleGrabber, PinDirection.Output, 0);
      hr = _graphBuilder.Connect(samplePin, demuxInPin); //sample grabber->demux
      if (hr != 0)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: failed to connect grabber->demux");
        return;
      }
      if (_vmr9 != null)
      {
        _vmr9.RemoveVMR9();
        _vmr9.Release();
        _vmr9 = null;
      }

      //now start the graph
      Log.WriteFile(Log.LogType.Capture, "DVBGraphSS2: start graph");

      if (_interfaceMediaControl == null)
      {
        _interfaceMediaControl = (IMediaControl)_graphBuilder;
      }
      hr = _interfaceMediaControl.Run();
      if (hr < 0)
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraphSS2: FAILED unable to start graph :0x{0:X}", hr);
      }
      _graphState = State.Epg;
      _isGraphRunning = true; 
    }
  }// class
}// namespace

