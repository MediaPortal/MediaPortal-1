/* 
 *	Copyright (C) 2005 Team MediaPortal
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
#define HW_PID_FILTERING
//#define DUMP
//#define USEMTSWRITER
#define COMPARE_PMT
#if (UseCaptureCardDefinitions)
#region usings
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using DShowNET;
using DShowNET.Helper;
using DShowNET.MPSA;
using DShowNET.MPTSWriter;
using DirectShowLib;
using DirectShowLib.BDA;
using DirectShowLib.SBE;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.TV.Database;
using MediaPortal.TV.Epg;
using TVCapture;
using System.Xml;
//using DirectX.Capture;
using MediaPortal.Radio.Database;
using Toub.MediaCenter.Dvrms.Metadata;
using MediaPortal.TV.BDA;
#endregion

namespace MediaPortal.TV.Recording
{
  /// <summary>
  /// Implementation of IGraph for digital TV capture cards using the BDA driver architecture
  /// It handles any DVB-T, DVB-C, DVB-S TV Capture card with BDA drivers
  ///
  /// A graphbuilder object supports one or more TVCapture cards and
  /// contains all the code/logic necessary for
  /// -tv viewing
  /// -tv recording
  /// -tv timeshifting
  /// -radio
  /// </summary>
  public class DVBGraphBase : MediaPortal.TV.Recording.IGraph, IHardwarePidFiltering
  {
    #region guids
    public static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid(0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7, 0x3d, 0xf7, 0xb5);
    public static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid(0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5, 0xd9, 0x95);
    public static Guid MEDIASUBTYPE_DVB_SI = new Guid(0xe9dd31a3, 0x221d, 0x4adb, 0x85, 0x32, 0x9a, 0xf3, 0x9, 0xc1, 0xa4, 0x8);
    public static Guid MEDIASUBTYPE_ATSC_SI = new Guid(0xb3c7397c, 0xd303, 0x414d, 0xb3, 0x3c, 0x4e, 0xd2, 0xc9, 0xd2, 0x97, 0x33);
    #endregion

    #region demuxer pin media types
    protected static byte[] Mpeg2ProgramVideo = 
				{
					0x00, 0x00, 0x00, 0x00,                         //00  .hdr.rcSource.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //04  .hdr.rcSource.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //08  .hdr.rcSource.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //0c  .hdr.rcSource.bottom            = 0x00000240 //576
					0x00, 0x00, 0x00, 0x00,                         //10  .hdr.rcTarget.left              = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //14  .hdr.rcTarget.top               = 0x00000000
					0xD0, 0x02, 0x00, 0x00,                         //18  .hdr.rcTarget.right             = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
					0x00, 0x09, 0x3D, 0x00,                         //20  .hdr.dwBitRate                  = 0x003d0900
					0x00, 0x00, 0x00, 0x00,                         //24  .hdr.dwBitErrorRate             = 0x00000000

					//0x051736=333667-> 10000000/333667 = 29.97fps
					//0x061A80=400000-> 10000000/400000 = 25fps
					0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
					0x00, 0x00, 0x00, 0x00,                         //2c  .hdr.dwInterlaceFlags           = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //30  .hdr.dwCopyProtectFlags         = 0x00000000
					0x04, 0x00, 0x00, 0x00,                         //34  .hdr.dwPictAspectRatioX         = 0x00000004
					0x03, 0x00, 0x00, 0x00,                         //38  .hdr.dwPictAspectRatioY         = 0x00000003
					0x00, 0x00, 0x00, 0x00,                         //3c  .hdr.dwReserved1                = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //40  .hdr.dwReserved2                = 0x00000000
					0x28, 0x00, 0x00, 0x00,                         //44  .hdr.bmiHeader.biSize           = 0x00000028
					0xD0, 0x02, 0x00, 0x00,                         //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
					0x40, 0x02, 0x00, 0x00,                         //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
					0x00, 0x00,                                     //50  .hdr.bmiHeader.biPlanes         = 0x0000
					0x00, 0x00,                                     //54  .hdr.bmiHeader.biBitCount       = 0x0000
					0x00, 0x00, 0x00, 0x00,                         //58  .hdr.bmiHeader.biCompression    = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
					0xD0, 0x07, 0x00, 0x00,                         //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
					0x27, 0xCF, 0x00, 0x00,                         //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
					0x00, 0x00, 0x00, 0x00,                         //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
					0x00, 0x00, 0x00, 0x00,                         //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
					0x98, 0xF4, 0x06, 0x00,                         //70  .dwStartTimeCode                = 0x0006f498
					0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000056
					//0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
					0x02, 0x00, 0x00, 0x00,                         //78  .dwProfile                      = 0x00000002
					0x02, 0x00, 0x00, 0x00,                         //7c  .dwLevel                        = 0x00000002
					0x00, 0x00, 0x00, 0x00,                         //80  .Flags                          = 0x00000000
					
					//  .dwSequenceHeader [1]
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
					0x00, 0x00, 0x00, 0x00,
		};

    protected static byte[] MPEG1AudioFormat = 
			{
				0x50, 0x00,             // format type      = 0x0050=WAVE_FORMAT_MPEG
				0x02, 0x00,             // channels
				0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
				0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
				0x01, 0x00,             // nBlockAlign      = 0x0300 = 768
				0x00, 0x00,             // wBitsPerSample   = 16
				0x16, 0x00,             // extra size       = 0x0016 = 22 bytes
				0x02, 0x00,             // fwHeadLayer
				0x00, 0xE8,0x03, 0x00,  // dwHeadBitrate
				0x01, 0x00,             // fwHeadMode
				0x01, 0x00,             // fwHeadModeExt
				0x01, 0x00,             // wHeadEmphasis
				0x16, 0x00,             // fwHeadFlags
				0x00, 0x00, 0x00, 0x00, // dwPTSLow
				0x00, 0x00, 0x00, 0x00  // dwPTSHigh
			};
    #endregion

    #region imports
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe protected static extern bool DvrMsCreate(out int id, IBaseFilter streamBufferSink, [In, MarshalAs(UnmanagedType.LPWStr)]string strPath, uint dwRecordingType);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe protected static extern void DvrMsStart(int id, uint startTime);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe protected static extern void DvrMsStop(int id);
    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe protected static extern bool AddTeeSinkToGraph(IGraphBuilder graph);

    [ComImport, Guid("6CFAD761-735D-4aa5-8AFC-AF91A7D61EBA")]
    protected class VideoAnalyzer { };

    [ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
    protected class MPEG2Demultiplexer { }

    [ComImport, Guid("2DB47AE5-CF39-43c2-B4D6-0CD8D90946F4")]
    protected class StreamBufferSink { };

    [ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
    protected class StreamBufferConfig { }

    [DllImport("advapi32", CharSet = CharSet.Auto)]
    protected static extern bool ConvertStringSidToSid(string pStringSid, ref IntPtr pSID);

    [DllImport("kernel32", CharSet = CharSet.Auto)]
    protected static extern IntPtr LocalFree(IntPtr hMem);

    [DllImport("advapi32", CharSet = CharSet.Auto)]
    protected static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);


    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    protected static extern bool GetPidMap(DirectShowLib.IPin filter, ref uint pid, ref uint mediasampletype);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxer(IPin pin, int pid, IPin pin1, int pid1, IPin pin2, int pid2);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxerPin(IPin pin, int pid, int elementaryStream, bool unmapOtherPins);
    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int DumpMpeg2DemuxerMappings(IBaseFilter filter);

    #endregion

    #region class member variables
    #region enums
    enum MediaSampleContent : int
    {
      TransportPacket,
      ElementaryStream,
      Mpeg2PSI,
      TransportPayload
    } ;

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
    #endregion

    #region consts
    protected static Guid CLSID_StreamBufferSink = new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9, 0x46, 0xf4);
    protected static Guid CLSID_Mpeg2VideoStreamAnalyzer = new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91, 0xa7, 0xd6, 0x1e, 0xba);
    protected static Guid CLSID_StreamBufferConfig = new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a, 0x87, 0x49, 0x4b);
    #endregion

    #region variables
    protected int _lastPMTVersion = -1;
    protected int _cardId = -1;
    protected int _currentChannelNumber = 28;
    protected DsROTEntry _rotEntry = null;			// Cookie into the Running Object Table

    protected State _graphState = State.None;
    protected DateTime _startTimer = DateTime.Now;


    protected IPin _pinAC3Out = null;
    protected IPin _pinMPG1Out = null;
    protected IPin _pinDemuxerVideo = null;
    protected IPin _pinDemuxerAudio = null;
    protected IPin _pinDemuxerSections = null;
    protected IPin _pinDemuxerEPG = null;
    protected IPin _pinDemuxerMHWd2 = null;
    protected IPin _pinDemuxerMHWd3 = null;
    protected IStreamBufferSink3 m_IStreamBufferSink = null;
    protected IStreamBufferConfigure m_IStreamBufferConfig = null;
    protected IBaseFilter _filterTIF = null;			// Transport Information Filter
    protected IBaseFilter _filterNetworkProvider = null;			// BDA Network Provider
    protected IBaseFilter _filterTunerDevice = null;			// BDA Digital Tuner Device
    protected IBaseFilter _filterCaptureDevice = null;			// BDA Digital Capture Device
    protected IBaseFilter _filterMpeg2Demultiplexer = null;			// Mpeg2 Demultiplexer that connects to Preview pin on Smart Tee (must connect before capture)
    protected IStreamAnalyzer _analyzerInterface = null;
    protected IEPGGrabber _epgGrabberInterface = null;
    protected IMHWGrabber _mhwGrabberInterface = null;
    protected IATSCGrabber _atscGrabberInterface = null;
    protected IBaseFilter _filterDvbAnalyzer = null;
    protected bool _graphPaused = false;
    protected int _pmtSendCounter = 0;
    ArrayList _scanPidList = new ArrayList();
    protected bool _scanPidListReady = false;
    protected VideoCaptureProperties _cardProperties = null;
    #endregion

#if USEMTSWRITER
		IBaseFilter						  _filterTsWriter=null;
		IMPTSWriter							_tsWriterInterface=null;
		IMPTSRecord						  _tsRecordInterface=null;
#endif
    protected IBaseFilter _filterSmartTee = null;

    protected VideoAnalyzer m_mpeg2Analyzer = null;
    protected IGraphBuilder _graphBuilder = null;
    protected ICaptureGraphBuilder2 _captureGraphBuilderInterface = null;
    protected IVideoWindow _videoWindowInterface = null;
    protected IBasicVideo2 _basicVideoInterFace = null;
    protected IMediaControl _mediaControl = null;
    protected IBaseFilter _filterSampleGrabber = null;
    protected ISampleGrabber _sampleInterface = null;

    protected StreamBufferSink m_StreamBufferSink = null;
    protected StreamBufferConfig m_StreamBufferConfig = null;
    protected VMR9Util _vmr9 = null;
    protected NetworkType _networkType = NetworkType.Unknown;
    protected TVCaptureDevice _card;
    protected bool _isGraphRunning = false;
    protected DVBChannel _currentTuningObject = null;
    protected TSHelperTools _transportHelper = new TSHelperTools();
    protected bool _refreshPmtTable = false;
    protected DateTime _updateTimer = DateTime.Now;
    protected DVBDemuxer _streamDemuxer = new DVBDemuxer();
    protected EpgGrabber _epgGrabber = new EpgGrabber();
    protected int m_recorderId = -1;

    protected int _videoWidth = 1;
    protected int _videoHeight = 1;
    protected int _aspectRatioX = 1;
    protected int _aspectRatioY = 1;
    protected bool _isUsingAC3 = false;
    protected bool _isOverlayVisible = true;
    protected DateTime _signalLostTimer = DateTime.Now;
    protected int _pmtRetyCount = 0;
    protected int _signalQuality;
    protected int _signalLevel;
    protected bool _signalPresent;
    protected bool _tunerLocked;
    protected bool _inScanningMode = false;
    protected DateTime _pmtTimer;
    protected DateTime _processTimer = DateTime.MinValue;
    protected IPin _pinTeletext;

#if DUMP
		System.IO.FileStream fileout;
#endif
    #endregion

    #region constructor
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pCard">instance of a TVCaptureDevice which contains all details about this card</param>
    public DVBGraphBase(TVCaptureDevice pCard)
    {
      _card = pCard;
      _cardId = pCard.ID;
      _graphState = State.None;

      try
      {
        System.IO.Directory.CreateDirectory("database");
      }
      catch (Exception) { }

      try
      {
        System.IO.Directory.CreateDirectory(@"database\pmt");
      }
      catch (Exception) { }
      //create registry keys needed by the streambuffer engine for timeshifting/recording
      try
      {
        RegistryKey hkcu = Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm = Registry.LocalMachine;
        hklm.CreateSubKey(@"Software\MediaPortal");
      }
      catch (Exception) { }

    }

    #endregion

    #region create/view/timeshift/record
    #region createGraph/DeleteGraph()
    /// <summary>
    /// Creates a new DirectShow graph for the TV capturecard.
    /// This graph can be a DVB-T, DVB-C or DVB-S graph
    /// </summary>
    /// <returns>bool indicating if graph is created or not</returns>
    public virtual bool CreateGraph(int Quality)
    {
      return false;
    }//public bool CreateGraph()

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// Frees any (unmanaged) resources
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public virtual void DeleteGraph()
    {
    }//public void DeleteGraph()

    #endregion
    #region Start/Stop Recording
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
    public bool StartRecording(Hashtable attributes, TVRecording recording, TVChannel channel, ref string strFileName, bool bContentRecording, DateTime timeProgStart)
    {
      if (_graphState != State.TimeShifting)
        return false;

      if (m_StreamBufferSink == null)
        return false;

      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }
#if USEMTSWRITER
			Log.WriteFile(Log.LogType.Capture,"DVBGraph:StartRecording()");
			strFileName=System.IO.Path.ChangeExtension(strFileName,".ts");
			int hr=_tsRecordInterface.SetRecordingFileName(strFileName);
			if (hr!=0)
			{
				Log.Write("DVBGraph:unable to set filename:%x", hr);
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
					_tsWriterInterface.TimeShiftBufferDuration(out timeInBuffer); // get the amount of time in the timeshiftbuffer
					if (timeInBuffer>0) timeInBuffer/=10000000;
					Log.Write("DVBGraph: timeshift buffer length:{0}",timeInBuffer);

					//how many seconds in the past we want to record?
					TimeSpan ts = DateTime.Now - timeProgStart;
					lStartTime=(long)ts.TotalSeconds;

					//does timeshift buffer contain all this info, if not then limit it
				  if (lStartTime > timeInBuffer)
						lStartTime=timeInBuffer;
					

					DateTime dtStart = DateTime.Now;
					dtStart=dtStart.AddSeconds( - lStartTime);
					ts=DateTime.Now-dtStart;
					Log.WriteFile(Log.LogType.Capture,"DVBGraph: Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
						dtStart.Hour, dtStart.Minute, dtStart.Second,
						ts.Hours, ts.Minutes, ts.Seconds);
															
					lStartTime *= 10000000;
				}
			}

			hr=_tsRecordInterface.StartRecord(lStartTime);
			if (hr!=0)
			{
				Log.Write("DVBGraph:unable to start recording:%x", hr);
				return false;
			}

			_graphState = State.Recording;
			return true;
#else
      Log.WriteFile(Log.LogType.Capture, "DVBGraph:StartRecording()");
      uint iRecordingType = 0;
      if (bContentRecording)
        iRecordingType = 0;
      else
        iRecordingType = 1;

      try
      {
        bool success = DvrMsCreate(out m_recorderId, (IBaseFilter)m_IStreamBufferSink, strFileName, iRecordingType);
        if (!success)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:StartRecording() FAILED to create recording");
          return false;
        }
        long lStartTime = 0;

        // if we're making a reference recording
        // then record all content from the past as well
        if (!bContentRecording)
        {
          // so set the startttime...
          int uiSecondsPerFile;
          int uiMinFiles, uiMaxFiles;
          m_IStreamBufferConfig.GetBackingFileCount(out uiMinFiles, out uiMaxFiles);
          m_IStreamBufferConfig.GetBackingFileDuration(out uiSecondsPerFile);
          lStartTime = uiSecondsPerFile;
          lStartTime *= (long)uiMaxFiles;

          // if start of program is given, then use that as our starttime
          if (timeProgStart.Year > 2000)
          {
            TimeSpan ts = DateTime.Now - timeProgStart;
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
              timeProgStart.Hour, timeProgStart.Minute, timeProgStart.Second,
              ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);

            lStartTime = (long)ts.TotalSeconds;
          }
          else Log.WriteFile(Log.LogType.Capture, "DVBGraph: record entire timeshift buffer");

          TimeSpan tsMaxTimeBack = DateTime.Now - _startTimer;
          if (lStartTime > tsMaxTimeBack.TotalSeconds)
          {
            lStartTime = (long)tsMaxTimeBack.TotalSeconds;
          }


          lStartTime *= -10000000L;//in reference time 
        }//if (!bContentRecording)
        /*
            foreach (MetadataItem item in attributes.Values)
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
        DvrMsStart(m_recorderId, (uint)lStartTime);
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
      }
      _graphState = State.Recording;
      return true;

#endif
    }//public bool StartRecording(int country,AnalogVideoStandard standard,int iChannelNr, ref string strFileName, bool bContentRecording, DateTime timeProgStart)

    /// <summary>
    /// Stops recording 
    /// </summary>
    /// <remarks>
    /// Graph should be recording. When Recording is stopped the graph is still 
    /// timeshifting
    /// </remarks>
    public void StopRecording()
    {
      if (_graphState != State.Recording) return;
      Log.WriteFile(Log.LogType.Capture, "DVBGraph:stop recording...");
#if USEMTSWRITER
			if (_tsRecordInterface!=null)
			{
				_tsRecordInterface.StopRecord(0);
			}
#else
      if (m_recorderId >= 0)
      {
        //Log.WriteFile(Log.LogType.Capture, "DVBGraph:stop recorder:{0}...", m_recorderId);
        DvrMsStop(m_recorderId);
        m_recorderId = -1;

      }

#endif

      _graphState = State.TimeShifting;
      //Log.WriteFile(Log.LogType.Capture, "DVBGraph:stopped recording...");
    }//public void StopRecording()

    #endregion
    #region Start/Stop Viewing
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
      Log.WriteFile(Log.LogType.Capture, "DVBGraph:StartViewing() {0}", channel.Name);

      _isOverlayVisible = true;
      // add VMR9 renderer to graph
      if (_vmr9 == null)
        _vmr9 = new VMR9Util();

      if (_vmr9 != null)
      {
        if (false == _vmr9.AddVMR9(_graphBuilder))
        {
          _vmr9.Dispose();
          _vmr9 = null;
        }
      }
      GetTvChannelFromDatabase(channel);
      if (_currentTuningObject == null)
      {
        return false;
      }
      // add the preferred video/audio codecs
      AddPreferredCodecs(true, true);

      // render the video/audio pins of the mpeg2 demultiplexer so they get connected to the video/audio codecs
      if (_graphBuilder.Render(_pinDemuxerVideo) != 0)
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:Failed to render video out pin MPEG-2 Demultiplexer");
        return false;
      }

      _isUsingAC3 = TVDatabase.DoesChannelHaveAC3(channel, Network() == NetworkType.DVBC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBS, Network() == NetworkType.ATSC);
      if (_isUsingAC3)
        Log.WriteFile(Log.LogType.Capture, "DVBGraph: channel {0} uses AC3", channel.Name);
      else
        Log.WriteFile(Log.LogType.Capture, "DVBGraph: channel {0} uses MP2 audio", channel.Name);
      Log.Write("DVBGraph:StartViewing(). : ac3={0}", _isUsingAC3);

      if (!_isUsingAC3)
      {
        //Log.WriteFile(Log.LogType.Capture, false, "DVBGraph:render MP2 audio pin");
        if (_graphBuilder.Render(_pinDemuxerAudio) != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:Failed to render audio out pin MPEG-2 Demultiplexer");
          return false;
        }

      }
      else
      {
        //Log.WriteFile(Log.LogType.Capture, false, "DVBGraph:render AC3 audio pin");
        if (_graphBuilder.Render(_pinAC3Out) != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:Failed to render AC3 pin MPEG-2 Demultiplexer");
          return false;
        }
      }

      //get the IMediaControl interface of the graph
      if (_mediaControl == null)
        _mediaControl = (IMediaControl)_graphBuilder;

      int hr;
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
          _vmr9.Dispose();
          _vmr9 = null;
        }
      }
      //if are using the overlay video renderer
      if (useOverlay)
      {
        //then get the overlay video renderer interfaces
        _videoWindowInterface = _graphBuilder as IVideoWindow;
        if (_videoWindowInterface == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED:Unable to get IVideoWindow");
          return false;
        }

        _basicVideoInterFace = _graphBuilder as IBasicVideo2;
        if (_basicVideoInterFace == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED:Unable to get IBasicVideo2");
          return false;
        }

        // and set it up
        hr = _videoWindowInterface.put_Owner(GUIGraphicsContext.form.Handle);
        if (hr != 0)
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED:set Video window:0x{0:X}", hr);

        hr = _videoWindowInterface.put_WindowStyle((WindowStyle)((int)WindowStyle.ClipSiblings + (int)WindowStyle.Child + (int)WindowStyle.ClipChildren));
        if (hr != 0)
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED:set Video window style:0x{0:X}", hr);

        //show overlay window
        hr = _videoWindowInterface.put_Visible(OABool.True);
        if (hr != 0)
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED:put_Visible:0x{0:X}", hr);
      }

      //start the graph
      //Log.WriteFile(Log.LogType.Capture,"DVBGraph: start graph");
      hr = _mediaControl.Run();
      if (hr < 0)
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
      }

      _isGraphRunning = true;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "normal");
        if (strValue.Equals("zoom")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Zoom;
        if (strValue.Equals("stretch")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Stretch;
        if (strValue.Equals("normal")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Normal;
        if (strValue.Equals("original")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.Original;
        if (strValue.Equals("letterbox")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.LetterBox43;
        if (strValue.Equals("panscan")) GUIGraphicsContext.ARType = MediaPortal.GUI.Library.Geometry.Type.PanScan43;

      }

      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      _graphState = State.Viewing;
      GUIGraphicsContext_OnVideoWindowChanged();


      // tune to the correct channel
      if (channel.Number >= 0)
        TuneChannel(channel);


      //Log.WriteFile(Log.LogType.Capture, "DVBGraph:Viewing..");
      return true;
    }//public bool StartViewing(AnalogVideoStandard standard, int iChannel,int country)


    /// <summary>
    /// Stops viewing the TV channel 
    /// </summary>
    /// <returns>boolean indicating if succeed</returns>
    /// <remarks>
    /// Graph must be viewing first with StartViewing()
    /// </remarks>
    public bool StopViewing()
    {
      if (_graphState != State.Viewing) return false;

      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      Log.WriteFile(Log.LogType.Capture, "DVBGraph: StopViewing()");
      if (_videoWindowInterface != null)
        _videoWindowInterface.put_Visible(OABool.False);

      //Log.WriteFile(Log.LogType.Capture, "DVBGraph: stop vmr9");
      if (_vmr9 != null)
      {
        _vmr9.Enable(false);
      }
      if (_mediaControl != null)
      {
        _mediaControl.Stop();
      }
      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinAC3Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinMPG1Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerAudio);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideo);

      //Log.WriteFile(Log.LogType.Capture, "DVBGraph: view stopped");
      _isGraphRunning = false;
      _graphState = State.Created;

      return true;
    }

    #endregion
    #region Start/Stop Timeshifting
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
    public bool StartTimeShifting(TVChannel channel, string strFileName)
    {
      if (_graphState != State.Created && _graphState != State.TimeShifting)
        return false;
      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }
      Log.WriteFile(Log.LogType.Capture, "DVBGraph:StartTimeShifting() {0}", channel.Name);

      GetTvChannelFromDatabase(channel);
      if (_currentTuningObject == null)
      {
        return false;
      }
      _isUsingAC3 = false;
      if (channel != null)
      {
        _isUsingAC3 = TVDatabase.DoesChannelHaveAC3(channel, Network() == NetworkType.DVBC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBS, Network() == NetworkType.ATSC);
        if (_isUsingAC3)
          Log.WriteFile(Log.LogType.Capture, "DVBGraph: channel {0} uses AC3", channel.Name);
        else
          Log.WriteFile(Log.LogType.Capture, "DVBGraph: channel {0} uses MP2 audio", channel.Name);
      }
      Log.Write("DVBGraph:(). StartTimeShifting: ac3={0}", _isUsingAC3);
      if (CreateSinkSource(strFileName, _isUsingAC3))
      {
        if (_mediaControl == null)
        {
          _mediaControl = (IMediaControl)_graphBuilder;
        }
        //now start the graph
        //Log.WriteFile(Log.LogType.Capture, "DVBGraph: start graph");
        int hr = _mediaControl.Run();
        if (hr < 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
        }
        TuneChannel(channel);
        _isGraphRunning = true;
        _graphState = State.TimeShifting;
      }
      else
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:Unable to create sinksource()");
        return false;
      }

      //Log.WriteFile(Log.LogType.Capture, "DVBGraph:timeshifting started");
      return true;
    }//public bool StartTimeShifting(int country,AnalogVideoStandard standard, int iChannel, string strFileName)

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
      Log.WriteFile(Log.LogType.Capture, "DVBGraph: StopTimeShifting()");

      if (_mediaControl != null)
      {
        _mediaControl.Stop();
      }
      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinAC3Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinMPG1Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerAudio);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideo);

      _isGraphRunning = false;
      _graphState = State.Created;
      return true;
    }//public bool StopTimeShifting()

    #endregion
    #endregion

    public bool Overlay
    {
      get
      {
        return _isOverlayVisible;
      }
      set
      {
        if (value == _isOverlayVisible) return;
        _isOverlayVisible = value;
        if (!_isOverlayVisible)
        {
          if (_videoWindowInterface != null)
            _videoWindowInterface.put_Visible(OABool.False);

        }
        else
        {
          if (_videoWindowInterface != null)
            _videoWindowInterface.put_Visible(OABool.True);

        }
      }
    }

    #region overrides
    /// <summary>
    /// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
    /// </summary>
    protected void GUIGraphicsContext_OnVideoWindowChanged()
    {

      if (GUIGraphicsContext.Vmr9Active) return;
      if (_graphState != State.Viewing) return;
      if (_basicVideoInterFace == null) return;
      if (_videoWindowInterface == null) return;
      Log.Write("DVBGraph:OnVideoWindowChanged()");
      int iVideoWidth, iVideoHeight;
      int aspectX, aspectY;
      _basicVideoInterFace.GetVideoSize(out iVideoWidth, out iVideoHeight);
      _basicVideoInterFace.GetPreferredAspectRatio(out aspectX, out aspectY);

      _videoWidth = iVideoWidth;
      _videoHeight = iVideoHeight;
      _aspectRatioX = aspectX;
      _aspectRatioY = aspectY;


      if (GUIGraphicsContext.BlankScreen)
      {
        Overlay = false;
      }
      else
      {
        Overlay = true;
      }
      if (GUIGraphicsContext.IsFullScreenVideo)
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

        if (rSource.Left < 0 || rSource.Top < 0 || rSource.Width <= 0 || rSource.Height <= 0) return;
        if (rDest.Left < 0 || rDest.Top < 0 || rDest.Width <= 0 || rDest.Height <= 0) return;

        Log.Write("overlay: video WxH  : {0}x{1}", iVideoWidth, iVideoHeight);
        Log.Write("overlay: video AR   : {0}:{1}", aspectX, aspectY);
        Log.Write("overlay: screen WxH : {0}x{1}", nw, nh);
        Log.Write("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
        Log.Write("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
        Log.Write("overlay: src        : ({0},{1})-({2},{3})",
          rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
        Log.Write("overlay: dst        : ({0},{1})-({2},{3})",
          rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);


        _basicVideoInterFace.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
        _basicVideoInterFace.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
        _videoWindowInterface.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
      else
      {
        if (GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 ||
          GUIGraphicsContext.VideoWindow.Width <= 0 || GUIGraphicsContext.VideoWindow.Height <= 0) return;
        if (iVideoHeight <= 0 || iVideoWidth <= 0) return;

        _basicVideoInterFace.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
        _basicVideoInterFace.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
        _videoWindowInterface.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top, GUIGraphicsContext.VideoWindow.Width, GUIGraphicsContext.VideoWindow.Height);
      }
    }

    /// <summary>
    /// This method can be used to ask the graph if it should be rebuild when
    /// we want to tune to the new channel:ichannel
    /// </summary>
    /// <param name="iChannel">new channel to tune to</param>
    /// <returns>true : graph needs to be rebuild for this channel
    ///          false: graph does not need to be rebuild for this channel
    /// </returns>
    public bool ShouldRebuildGraph(TVChannel newChannel)
    {
      //check if we switch from an channel with AC3 to a channel without AC3
      //or vice-versa. ifso, graphs should be rebuild
      if (_graphState != State.Viewing && _graphState != State.TimeShifting && _graphState != State.Recording)
      {
        Log.Write("DVBGraph: ShouldRebuildGraph({0})  false, not viewing", newChannel.Name);
        return false;
      }
      bool useAC3 = TVDatabase.DoesChannelHaveAC3(newChannel, Network() == NetworkType.DVBC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBS, Network() == NetworkType.ATSC);
      Log.Write("DVBGraph: ShouldRebuildGraph({0})  current ac3:{1} new channel ac3:{2}",
                  newChannel.Name, _isUsingAC3, useAC3);
      if (useAC3 != _isUsingAC3) return true;
      return false;
    }

    #region Stream-Audio handling
    public int GetAudioLanguage()
    {
      return _currentTuningObject.AudioPid;
    }

    public void SetAudioLanguage(int audioPid)
    {
      if (audioPid == _currentTuningObject.AudioPid) return;
      Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: change audio pid {0:X}-> pid:{1:X} {2}", _currentTuningObject.AudioPid, audioPid,_graphState);

      SetupDemuxerPin(_pinAC3Out, audioPid, (int)MediaSampleContent.ElementaryStream, true);
      SetupDemuxerPin(_pinDemuxerAudio, audioPid, (int)MediaSampleContent.ElementaryStream, true);

      if (audioPid == _currentTuningObject.AC3Pid)
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: AC3 audio");
        //check if ac3 pin is connected
        IPin pin;
        _pinAC3Out.ConnectedTo(out pin);
        if (pin == null)
        {
          //no? then connect ac3 pin
          if (_mediaControl != null)
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: stop graph");
            _mediaControl.Stop();
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: disconnect MP2 pin");
            _pinDemuxerAudio.Disconnect();
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: connect AC3 pin");
            _graphBuilder.Render(_pinAC3Out);
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: start graph");
            _mediaControl.Run();
          }
        }
        else
        {
          Marshal.ReleaseComObject(pin);
        }
      }
      else
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: MP2 audio");
        //check if mpeg2 audio pin is connected
        IPin pin;
        _pinDemuxerAudio.ConnectedTo(out pin);
        if (pin == null)
        {
          //no? then connect mpeg2 audio pin
          if (_mediaControl != null)
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: stop graph");
            _mediaControl.Stop();
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: disconnect AC3 pin");
            _pinAC3Out.Disconnect();
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: connect MP2 pin");
            _graphBuilder.Render(_pinDemuxerAudio);
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: start graph");
            _mediaControl.Run();
          }
        }
        else
        {
          Marshal.ReleaseComObject(pin);
        }
      }

      _currentTuningObject.AudioPid = audioPid;
      SetupMTSDemuxerPin();
      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
    }

    public ArrayList GetAudioLanguageList()
    {
      if (_currentTuningObject == null) return new ArrayList();
      DVBSections.AudioLanguage al;
      ArrayList audioPidList = new ArrayList();
      /*
      if(_currentTuningObject.AudioPid>0)
      {
        al=new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid=_currentTuningObject.AudioPid;
        al.AudioLanguageCode=_currentTuningObject.AudioLanguage;
        audioPidList.Add(al);
      }*/
      if (_currentTuningObject.Audio1 > 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.Audio1;
        al.AudioLanguageCode = _currentTuningObject.AudioLanguage1;
        audioPidList.Add(al);
      }
      if (_currentTuningObject.Audio2 > 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.Audio2;
        al.AudioLanguageCode = _currentTuningObject.AudioLanguage2;
        audioPidList.Add(al);
      }
      if (_currentTuningObject.Audio3 > 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.Audio3;
        al.AudioLanguageCode = _currentTuningObject.AudioLanguage3;
        audioPidList.Add(al);
      }
      if (_currentTuningObject.AC3Pid > 0)
      {
        al = new MediaPortal.TV.Recording.DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.AC3Pid;
        al.AudioLanguageCode = "AC-3";
        audioPidList.Add(al);
      }
      return audioPidList;
    }
    #endregion

    public bool HasTeletext()
    {
      if (_graphState != State.TimeShifting && _graphState != State.Recording && _graphState != State.Viewing) return false;
      if (_currentTuningObject == null) return false;
      if (_currentTuningObject.TeletextPid > 0) return true;
      return false;
    }
    /// <summary>
    /// Returns the current tv channel
    /// </summary>
    /// <returns>Current channel</returns>
    public int GetChannelNumber()
    {
      return _currentChannelNumber;
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
    /// Add preferred mpeg video/audio codecs to the graph
    /// the user has can specify these codecs in the setup
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    /// <summary>
    /// returns true if tuner is locked to a frequency and signalstrength/quality is > 0
    /// </summary>
    /// <returns>
    /// true: tuner has a signal and is locked
    /// false: tuner is not locked
    /// </returns>
    /// <remarks>
    /// Graph should be created and GetTunerSignalStatistics() should be called
    /// </remarks>
    public bool SignalPresent()
    {
      return _signalPresent;
    }
    /// <summary>
    /// returns true if tuner is locked to a frequency and signalstrength/quality is > 0
    /// </summary>
    /// <returns>
    /// true: tuner has a signal and is locked
    /// false: tuner is not locked
    /// </returns>
    /// <remarks>
    /// Graph should be created and GetTunerSignalStatistics() should be called
    /// </remarks>
    public bool TunerLocked()
    {
      return _tunerLocked;
    }

    protected virtual void UpdateSignalPresent()
    {
    }//public bool SignalPresent()

    public int SignalQuality()
    {
      if (_signalQuality < 0) return 0;
      if (_signalQuality > 100) return 100;
      return _signalQuality;
    }

    public int SignalStrength()
    {
      if (_signalLevel < 0) return 0;
      if (_signalLevel > 100) return 100;
      return _signalLevel;
    }



    /// <summary>
    /// not used
    /// </summary>
    /// <returns>-1</returns>
    public long VideoFrequency()
    {
      if (_currentTuningObject != null) return _currentTuningObject.Frequency * 1000;
      return -1;
    }

    public PropertyPageCollection PropertyPages()
    {
      return null;
    }


    //not used
    public bool SupportsFrameSize(Size framesize)
    {
      return false;
    }

    /// <summary>
    /// return the network type (DVB-T, DVB-C, DVB-S)
    /// </summary>
    /// <returns>network type</returns>
    public virtual NetworkType Network()
    {
      return NetworkType.Unknown;
    }

    #endregion

    #region graph building helper functions
    protected void AddPreferredCodecs(bool audio, bool video)
    {
      // add preferred video & audio codecs
      string strVideoCodec = "";
      string strAudioCodec = "";
      string strAudioRenderer = "";
      bool bAddFFDshow = false;
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
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
    }//void AddPreferredCodecs()



    protected void SetupMTSDemuxerPin()
    {
#if USEMTSWRITER
			if (_tsWriterInterface== null || _tsWriterInterface==null || _currentTuningObject==null) return;
			Log.Write("DVBGraph:SetupMTSDemuxerPin");
			_tsWriterInterface.ResetPids();
			if (_currentTuningObject.AC3Pid>0)
				_tsWriterInterface.SetAC3Pid((ushort)_currentTuningObject.AC3Pid);
			else
				_tsWriterInterface.SetAC3Pid(0);

			if (_currentTuningObject.AudioPid>0)
				_tsWriterInterface.SetAudioPid((ushort)_currentTuningObject.AudioPid);
			else
			{
				if (_currentTuningObject.Audio1>0)
					_tsWriterInterface.SetAudioPid((ushort)_currentTuningObject.Audio1);
				else
					_tsWriterInterface.SetAudioPid(0);
			}
			
			if (_currentTuningObject.Audio2>0)
				_tsWriterInterface.SetAudioPid2((ushort)_currentTuningObject.Audio2);
			else
				_tsWriterInterface.SetAudioPid2(0);

			if (_currentTuningObject.SubtitlePid>0)
				_tsWriterInterface.SetSubtitlePid((ushort)_currentTuningObject.SubtitlePid);
			else
				_tsWriterInterface.SetSubtitlePid(0);

			if (_currentTuningObject.TeletextPid>0)
				_tsWriterInterface.SetTeletextPid((ushort)_currentTuningObject.TeletextPid);
			else
				_tsWriterInterface.SetTeletextPid(0);

			if (_currentTuningObject.VideoPid>0)
				_tsWriterInterface.SetVideoPid((ushort)_currentTuningObject.VideoPid);
			else
				_tsWriterInterface.SetVideoPid(0);

			if (_currentTuningObject.PCRPid>0)
				_tsWriterInterface.SetPCRPid((ushort)_currentTuningObject.PCRPid);
			else
				_tsWriterInterface.SetPCRPid(0);

			_tsWriterInterface.SetPMTPid((ushort)_currentTuningObject.PMTPid);
#endif
    }

    protected virtual bool CreateSinkSource(string fileName, bool useAC3)
    {
#if USEMTSWRITER
			if(_graphState!=State.Created && _graphState!=State.TimeShifting)
				return false;

			if (_filterTsWriter==null)
			{
				Log.WriteFile(Log.LogType.Capture,"DVBGraph:CreateGraph() add MPTSWriter");
				_filterTsWriter=(IBaseFilter) Activator.CreateInstance( Type.GetTypeFromCLSID( Clsid.MPTSWriter, true ) );
				_tsWriterInterface = _filterTsWriter as IMPTSWriter;
				_tsRecordInterface = _filterTsWriter as IMPTSRecord;

				int hr=_graphBuilder.AddFilter((IBaseFilter)_filterTsWriter,"MPTS Writer");
				if(hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraph:FAILED cannot add MPTS Writer:{0:X}",hr);
					return false;
				}			

				IFileSinkFilter fileWriter=_filterTsWriter as IFileSinkFilter;
				AMMediaType mt = new AMMediaType();
				mt.majorType=MediaType.Stream;
				mt.subType=MediaSubType.None;
				mt.formatType=FormatType.None;
				hr=fileWriter.SetFileName(fileName, ref mt);
				if (hr!=0)
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraph:FAILED cannot set filename '{0}' on MPTS writer:0x{1:X}",fileName,hr);
					return false;
				}


				// connect demuxer->mpts writer
				
				if (!ConnectFilters(ref _filterSmartTee, ref _filterTsWriter))
				{
					Log.WriteFile(Log.LogType.Capture,true,"DVBGraph:FAILED cannot demuxer->MPTS writer:0x{0:X}",hr);
					return false;
				}
			}
			SetupMTSDemuxerPin();
			return true;
#else
      int hr = 0;
      IPin pinObj0 = null;
      IPin pinObj1 = null;
      IPin pinObj2 = null;
      IPin pinObj3 = null;
      IPin outPin = null;

      try
      {
        int iTimeShiftBuffer = 30;
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          iTimeShiftBuffer = xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
          if (iTimeShiftBuffer < 5) iTimeShiftBuffer = 5;
        }
        iTimeShiftBuffer *= 60; //in seconds
        int iFileDuration = iTimeShiftBuffer / 6;

        //create StreamBufferSink filter
        //				Log.WriteFile(Log.LogType.Capture,"DVBGraph:CreateSinkSource()");
        hr = _graphBuilder.AddFilter((IBaseFilter)m_StreamBufferSink, "StreamBufferSink");
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED cannot add StreamBufferSink:{0:X}", hr);
          return false;
        }
        //create MPEG2 Analyzer filter
        //				Log.WriteFile(Log.LogType.Capture,"DVBGraph:Add mpeg2 analyzer()");
        hr = _graphBuilder.AddFilter((IBaseFilter)m_mpeg2Analyzer, "Mpeg2 Analyzer");
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED cannot add mpeg2 analyzer to graph:{0:X}", hr);
          return false;
        }

        //connect mpeg2 demuxer video out->mpeg2 analyzer input pin
        //get input pin of MPEG2 Analyzer filter
        //				Log.WriteFile(Log.LogType.Capture,"DVBGraph:find mpeg2 analyzer input pin()");
        pinObj0 = DsFindPin.ByDirection((IBaseFilter)m_mpeg2Analyzer, PinDirection.Input, 0);
        if (pinObj0 == null)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED cannot find mpeg2 analyzer input pin");
          return false;
        }

        //				Log.WriteFile(Log.LogType.Capture,"DVBGraph:connect demux video output->mpeg2 analyzer");
        hr = _graphBuilder.Connect(_pinDemuxerVideo, pinObj0);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to connect demux video output->mpeg2 analyzer:{0:X}", hr);
          return false;
        }

        //connect MPEG2 analyzer Filter->stream buffer sink pin 0
        //get output pin #0 from MPEG2 analyzer Filter
        //				Log.WriteFile(Log.LogType.Capture,"DVBGraph:mpeg2 analyzer output->streambuffersink in");
        pinObj1 = DsFindPin.ByDirection((IBaseFilter)m_mpeg2Analyzer, PinDirection.Output, 0);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED cannot find mpeg2 analyzer output pin:{0:X}", hr);
          return false;
        }

        //get input pin #0 from StreamBufferSink Filter
        pinObj2 = DsFindPin.ByDirection((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 0);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED cannot find SBE input pin:{0:X}", hr);
          return false;
        }

        hr = _graphBuilder.Connect(pinObj1, pinObj2);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to connect mpeg2 analyzer->streambuffer sink:{0:X}", hr);
          return false;
        }

        if (!useAC3)
        {
          //Log.WriteFile(Log.LogType.Capture, false, "DVBGraph:connect MP2 audio pin->SBE");
          //connect MPEG2 demuxer audio output ->StreamBufferSink Input #1
          //Get StreamBufferSink InputPin #1
          pinObj3 = DsFindPin.ByDirection((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 1);
          if (hr != 0)
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED cannot find SBE input pin#2");
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerAudio, pinObj3);
          if (hr != 0)
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to connect mpeg2 demuxer audio out->streambuffer sink in#2:{0:X}", hr);
            return false;
          }
        }
        else
        {
          //connect ac3 pin ->stream buffersink input #2
          //Log.WriteFile(Log.LogType.Capture, false, "DVBGraph:connect AC3 audio pin->SBE");
          if (_pinAC3Out != null)
          {
            pinObj3 = DsFindPin.ByDirection((IBaseFilter)m_StreamBufferSink, PinDirection.Input, 1);
            if (hr != 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED cannot find SBE input pin#2");
              return false;
            }
            hr = _graphBuilder.Connect(_pinAC3Out, pinObj3);
            if (hr != 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to connect mpeg2 demuxer AC3 out->streambuffer sink in#2:{0:X}", hr);
              return false;
            }
          }
          else
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED ac3 pin not found?");
          }
        }
        int ipos = fileName.LastIndexOf(@"\");
        string strDir = fileName.Substring(0, ipos);
        m_StreamBufferConfig = new StreamBufferConfig();
        m_IStreamBufferConfig = (IStreamBufferConfigure)m_StreamBufferConfig;

        // setting the StreamBufferEngine registry key
        IntPtr HKEY = (IntPtr)unchecked((int)0x80000002L);
        IStreamBufferInitialize pTemp = (IStreamBufferInitialize)m_IStreamBufferConfig;
        IntPtr subKey = IntPtr.Zero;

        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr = pTemp.SetHKEY(subKey);

        //set timeshifting folder
        //				Log.WriteFile(Log.LogType.Capture,"DVBGraph:set timeshift folder to:{0}", strDir);
        hr = m_IStreamBufferConfig.SetDirectory(strDir);
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to set timeshift folder to:{0} {1:X}", strDir, hr);
          return false;
        }

        //set number of timeshifting files
        hr = m_IStreamBufferConfig.SetBackingFileCount(6, 8);    //4-6 files
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to set timeshifting files to 6-8 {0:X}", hr);
          return false;
        }

        //set duration of each timeshift file
        hr = m_IStreamBufferConfig.SetBackingFileDuration((int)iFileDuration); // 60sec * 4 files= 4 mins
        if (hr != 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to set timeshifting filesduration to {0} {1:X}", iFileDuration, hr);
          return false;
        }

        subKey = IntPtr.Zero;
        HKEY = (IntPtr)unchecked((int)0x80000002L);
        IStreamBufferInitialize pConfig = (IStreamBufferInitialize)m_StreamBufferSink;

        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr = pConfig.SetHKEY(subKey);
        //set timeshifting filename
        //				Log.WriteFile(Log.LogType.Capture,"DVBGraph:set timeshift file to:{0}", fileName);

        IStreamBufferConfigure2 streamConfig2 = m_StreamBufferConfig as IStreamBufferConfigure2;
        if (streamConfig2 != null)
          streamConfig2.SetFFTransitionRates(8, 32);

        // lock on the 'filename' file
        hr = m_IStreamBufferSink.LockProfile(fileName);
        if (hr != 0 && hr != 1)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:FAILED to set timeshift file to:{0} {1:X}", fileName, hr);
          return false;
        }
      }
      finally
      {
        if (pinObj0 != null)
          Marshal.ReleaseComObject(pinObj0);
        if (pinObj1 != null)
          Marshal.ReleaseComObject(pinObj1);
        if (pinObj2 != null)
          Marshal.ReleaseComObject(pinObj2);
        if (pinObj3 != null)
          Marshal.ReleaseComObject(pinObj3);
        if (outPin != null)
          Marshal.ReleaseComObject(outPin);

        //if ( streamBufferInitialize !=null)
        //Marshal.ReleaseComObject(streamBufferInitialize );

      }
      //			(_graphBuilder as IMediaFilter).SetSyncSource(_filterMpeg2Demultiplexer as IReferenceClock);
      return true;
#endif
    }//private bool CreateSinkSource(string fileName)

    /// <summary>
    /// Finds and connects pins
    /// </summary>
    /// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
    /// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
    /// <returns>true if succeeded, false if failed</returns>
    protected bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter)
    {
      return ConnectFilters(ref UpstreamFilter, ref DownstreamFilter, 0);
    }//bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter) 

    /// <summary>
    /// Finds and connects pins
    /// </summary>
    /// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
    /// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
    /// <param name="preferredOutputPin">The one-based index of the preferred output pin to use on the Upstream filter.  This is tried first. Pin 1 = 1, Pin 2 = 2, etc</param>
    /// <returns>true if succeeded, false if failed</returns>
    protected bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter, int preferredOutputPin)
    {
      if (UpstreamFilter == null || DownstreamFilter == null)
        return false;

      int ulFetched = 0;
      int hr = 0;
      IEnumPins pinEnum;

      hr = UpstreamFilter.EnumPins(out pinEnum);
      if ((hr < 0) || (pinEnum == null))
        return false;

      #region Attempt to connect preferred output pin first
      if (preferredOutputPin > 0)
      {
        IPin[] outPin = new IPin[1];
        int outputPinCounter = 0;
        while (pinEnum.Next(1, outPin, out ulFetched) == 0)
        {
          PinDirection pinDir;
          outPin[0].QueryDirection(out pinDir);

          if (pinDir == PinDirection.Output)
          {
            outputPinCounter++;
            if (outputPinCounter == preferredOutputPin) // Go and find the input pin.
            {
              IEnumPins downstreamPins;
              DownstreamFilter.EnumPins(out downstreamPins);

              IPin[] dsPin = new IPin[1];
              while (downstreamPins.Next(1, dsPin, out ulFetched) == 0)
              {
                PinDirection dsPinDir;
                dsPin[0].QueryDirection(out dsPinDir);
                if (dsPinDir == PinDirection.Input)
                {
                  hr = _graphBuilder.Connect(outPin[0], dsPin[0]);
                  if (hr == 0)
                  {
                    Marshal.ReleaseComObject(dsPin[0]);
                    Marshal.ReleaseComObject(outPin[0]);
                    Marshal.ReleaseComObject(pinEnum);
                    Marshal.ReleaseComObject(downstreamPins);
                    return true;
                  }
                  Marshal.ReleaseComObject(dsPin[0]);
                }
              }//while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
              Marshal.ReleaseComObject(downstreamPins);
            }//if (outputPinCounter == preferredOutputPin)
          }//if (pinDir == PinDirection.Output)
          Marshal.ReleaseComObject(outPin[0]);
        }//while(pinEnum.Next(1, outPin, out ulFetched) == 0) 
        pinEnum.Reset();        // Move back to start of enumerator
      }//if (preferredOutputPin > 0) 
      #endregion

      IPin[] testPin = new IPin[1];
      while (pinEnum.Next(1, testPin, out ulFetched) == 0)
      {
        PinDirection pinDir;
        testPin[0].QueryDirection(out pinDir);

        if (pinDir == PinDirection.Output) // Go and find the input pin.
        {
          IEnumPins downstreamPins;

          DownstreamFilter.EnumPins(out downstreamPins);

          IPin[] dsPin = new IPin[1];
          while (downstreamPins.Next(1, dsPin, out ulFetched) == 0)
          {
            PinDirection dsPinDir;
            dsPin[0].QueryDirection(out dsPinDir);
            if (dsPinDir == PinDirection.Input)
            {
              hr = _graphBuilder.Connect(testPin[0], dsPin[0]);
              if (hr == 0)
              {
                Marshal.ReleaseComObject(dsPin[0]);
                Marshal.ReleaseComObject(downstreamPins);
                Marshal.ReleaseComObject(testPin[0]);
                Marshal.ReleaseComObject(pinEnum);
                return true;
              }
            }//if (dsPinDir == PinDirection.Input)
            Marshal.ReleaseComObject(dsPin[0]);
          }//while(downstreamPins.Next(1, dsPin, out ulFetched) == 0) 
          Marshal.ReleaseComObject(downstreamPins);
        }//if(pinDir == PinDirection.Output) // Go and find the input pin.
        Marshal.ReleaseComObject(testPin[0]);
      }//while(pinEnum.Next(1, testPin, out ulFetched) == 0) 
      Marshal.ReleaseComObject(pinEnum);
      return false;
    }//private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter, int preferredOutputPin) 

    /// <summary>
    /// Used to find the Network Provider for addition to the graph.
    /// </summary>
    /// <param name="ClassID">The filter category to enumerate.</param>
    /// <param name="FriendlyName">An identifier based on the DevicePath, used to find the device.</param>
    /// <param name="device">The filter that has been found.</param>
    /// <returns>true of succeeded, false if failed</returns>
    protected bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device)
    {
      int hr;
      ICreateDevEnum sysDevEnum = null;
      UCOMIEnumMoniker enumMoniker = null;

      sysDevEnum = (ICreateDevEnum)Activator.CreateInstance(Type.GetTypeFromCLSID(ClassId.SystemDeviceEnum, true));
      // Enumerate the filter category
      hr = sysDevEnum.CreateClassEnumerator(ClassID, out enumMoniker, 0);
      if (hr != 0)
        throw new NotSupportedException("No devices in this category");

      int fetched;
      UCOMIMoniker[] deviceMoniker = new UCOMIMoniker[1];
      while (enumMoniker.Next(1, deviceMoniker, out fetched) == 0) // while == S_OK
      {
        object bagObj = null;
        Guid bagId = typeof(IPropertyBag).GUID;
        deviceMoniker[0].BindToStorage(null, null, ref bagId, out bagObj);
        IPropertyBag propBag = (IPropertyBag)bagObj;
        object val = "";
        propBag.Read("FriendlyName", out val, null);
        string Name = val as string;
        val = "";
        Marshal.ReleaseComObject(propBag);
        if (String.Compare(Name, FriendlyName, true) == 0) // If found
        {
          object filterObj = null;
          System.Guid filterID = typeof(IBaseFilter).GUID;
          deviceMoniker[0].BindToObject(null, null, ref filterID, out filterObj);
          device = filterObj;

          filterObj = null;
          if (device != null)
          {
            Marshal.ReleaseComObject(deviceMoniker[0]);
            Marshal.ReleaseComObject(enumMoniker);
            return true;
          }
        }//if(String.Compare(Name.ToLower(), FriendlyName.ToLower()) == 0) // If found
        Marshal.ReleaseComObject(deviceMoniker[0]);
      }//while(enumMoniker.Next(1, deviceMoniker, out ulFetched) == 0) // while == S_OK
      Marshal.ReleaseComObject(enumMoniker);
      device = null;
      return false;
    }//private bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device) 

    #endregion

    #region process helper functions
    // send PMT to firedtv device
    protected bool SendPMT()
    {
      try
      {

        string pmtName = String.Format(@"database\pmt\pmt_{0}_{1}_{2}_{3}_{4}.dat",
          Utils.FilterFileName(_currentTuningObject.ServiceName),
          _currentTuningObject.NetworkID,
          _currentTuningObject.TransportStreamID,
          _currentTuningObject.ProgramNumber,
          (int)Network());
        if (!System.IO.File.Exists(pmtName))
        {
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }

        byte[] pmt = null;
        using (System.IO.FileStream stream = new System.IO.FileStream(pmtName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None))
        {
          long len = stream.Length;
          if (len > 6)
          {
            pmt = new byte[len];
            stream.Read(pmt, 0, (int)len);
            stream.Close();
          }
        }
        if (pmt == null)
        {
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }
        if (pmt.Length < 6)
        {
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }

        DVBSections sections = new DVBSections();
        DVBSections.ChannelInfo info = new DVBSections.ChannelInfo();
        if (!sections.GetChannelInfoFromPMT(pmt, ref info))
        {
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }
        if (info.pid_list != null)
        {
          if (info.pcr_pid <= 0)
            info.pcr_pid = -1;
          bool changed = false;
          bool hasAudio = false;
          int audioOptions = 0;
          for (int pids = 0; pids < info.pid_list.Count; pids++)
          {
            DVBSections.PMTData data = (DVBSections.PMTData)info.pid_list[pids];
            if (data.elementary_PID <= 0)
              data.elementary_PID = -1;
            if (data.isVideo)
            {
              if (_currentTuningObject.VideoPid != data.elementary_PID) changed = true;
              _currentTuningObject.VideoPid = data.elementary_PID;
            }

            if (data.isAudio)
            {
              switch (audioOptions)
              {
                case 0:
                  if (_currentTuningObject.Audio1 != data.elementary_PID) changed = true;
                  _currentTuningObject.Audio1 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      _currentTuningObject.AudioLanguage1 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions++;
                  break;
                case 1:
                  _currentTuningObject.Audio2 = data.elementary_PID;
                  if (_currentTuningObject.Audio2 != data.elementary_PID) changed = true;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      _currentTuningObject.AudioLanguage2 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions++;
                  break;
                case 2:
                  _currentTuningObject.Audio3 = data.elementary_PID;
                  if (_currentTuningObject.Audio3 != data.elementary_PID) changed = true;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      _currentTuningObject.AudioLanguage3 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions++;
                  break;

              }

              if (hasAudio == false)
              {
                if (_currentTuningObject.AudioPid != data.elementary_PID) changed = true;
                _currentTuningObject.AudioPid = data.elementary_PID;
                if (data.data != null)
                {
                  if (data.data.Length == 3)
                    _currentTuningObject.AudioLanguage = DVBSections.GetLanguageFromCode(data.data);
                }
                hasAudio = true;
              }
            }//if (data.isAudio) 

            if (data.isAC3Audio)
            {
              if (_currentTuningObject.AC3Pid != data.elementary_PID) changed = true;
              _currentTuningObject.AC3Pid = data.elementary_PID;
            }

            if (data.isTeletext)
            {
              if (_currentTuningObject.TeletextPid != data.elementary_PID) changed = true;
              _currentTuningObject.TeletextPid = data.elementary_PID;
            }

            if (data.isDVBSubtitle)
            {
              if (_currentTuningObject.SubtitlePid != data.elementary_PID) changed = true;
              _currentTuningObject.SubtitlePid = data.elementary_PID;
            }
          }//for (int pids =0; pids < info.pid_list.Count;pids++)

          if (_currentTuningObject.PCRPid != info.pcr_pid) changed = true;
          _currentTuningObject.PCRPid = info.pcr_pid;

          if (_currentTuningObject.AC3Pid <= 0) _currentTuningObject.AC3Pid = -1;
          if (_currentTuningObject.AudioPid <= 0) _currentTuningObject.AudioPid = -1;
          if (_currentTuningObject.Audio1 <= 0) _currentTuningObject.Audio1 = -1;
          if (_currentTuningObject.Audio2 <= 0) _currentTuningObject.Audio2 = -1;
          if (_currentTuningObject.Audio3 <= 0) _currentTuningObject.Audio3 = -1;
          if (_currentTuningObject.SubtitlePid <= 0) _currentTuningObject.SubtitlePid = -1;
          if (_currentTuningObject.TeletextPid <= 0) _currentTuningObject.TeletextPid = -1;
          if (_currentTuningObject.VideoPid <= 0) _currentTuningObject.VideoPid = -1;
          if (_currentTuningObject.PCRPid <= 0) _currentTuningObject.PCRPid = -1;
          try
          {
            SetupMTSDemuxerPin();
            if (changed)
            {
              if (_graphState == State.Radio && (_currentTuningObject.PCRPid <= 0 || _currentTuningObject.PCRPid >= 0x1fff))
              {
                Log.Write("DVBGraph:SendPMT() setup demux:audio pid:{0:X} AC3 pid:{1:X} pcrpid:{2:X}", _currentTuningObject.AudioPid, _currentTuningObject.AC3Pid, _currentTuningObject.PCRPid);
                SetupDemuxer(_pinDemuxerVideo, 0, _pinDemuxerAudio, 0, _pinAC3Out, 0);
                SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.AudioPid, (int)MediaSampleContent.TransportPayload, true);
                SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.PCRPid, (int)MediaSampleContent.TransportPacket, false);
                if (_streamDemuxer != null)
                {
                  _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid, _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid, _currentTuningObject.Audio3, _currentTuningObject.ServiceName, _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
                }
              }
              else
              {
                Log.Write("DVBGraph:SendPMT() set demux: video pid:{0:X} audio pid:{1:X} AC3 pid:{2:X} audio1 pid:{3:X} audio2 pid:{4:X} audio3 pid:{5:X} subtitle pid:{6:X} teletext pid:{7:X} pcr pid:{8:X}",
                            _currentTuningObject.VideoPid, _currentTuningObject.AudioPid, _currentTuningObject.AC3Pid,
                            _currentTuningObject.Audio1, _currentTuningObject.Audio2, _currentTuningObject.Audio3,
                            _currentTuningObject.SubtitlePid, _currentTuningObject.TeletextPid, _currentTuningObject.PCRPid);
                SetupDemuxer(_pinDemuxerVideo, _currentTuningObject.VideoPid, _pinDemuxerAudio, _currentTuningObject.AudioPid, _pinAC3Out, _currentTuningObject.AC3Pid);
                if (_streamDemuxer != null)
                {
                  _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid, _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid, _currentTuningObject.Audio3, _currentTuningObject.ServiceName, _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
                }

              }
            }
          }
          catch (Exception)
          {
          }
        }//if (info.pid_list!=null)

        _refreshPmtTable = false;
        if (_cardProperties != null)
        {
          int pmtVersion = ((pmt[5] >> 1) & 0x1F);

          // send the PMT table to the device
          _pmtTimer = DateTime.Now;
          _pmtSendCounter++;
          if (_cardProperties.IsCISupported())
          {
            string camType = "";
            string filename = String.Format(@"database\card_{0}.xml", _card.FriendlyName);
            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
            {
              camType = xmlreader.GetValueAsString("dvbs", "cam", "Viaccess");
            }
            Log.Write("DVBGraph:Send PMT#{0} version:{1} signal strength:{2} signal quality:{3} locked:{4} cam:{5}", _pmtSendCounter, pmtVersion, SignalStrength(), SignalQuality(), _tunerLocked, camType);
            _streamDemuxer.DumpPMT(pmt);
            if (_cardProperties.SendPMT(camType, _currentTuningObject.ProgramNumber, _currentTuningObject.VideoPid, _currentTuningObject.AudioPid, pmt, (int)pmt.Length))
            {
              _lastPMTVersion = pmtVersion;
              return true;
            }
            else
            {
              _refreshPmtTable = true;
              _pmtSendCounter = 0;
              return true;
            }
          }
          else
          {
            _lastPMTVersion = pmtVersion;
            return true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      return false;
    }//SendPMT()

    /// <summary>
    /// GetDisEqcSettings()
    /// This method gets the disEqc settings for the tv channel specified
    /// </summary>
    /// <param name="ch">tvchannel</param>
    /// <param name="lowOsc">[out] low oscillator</param>
    /// <param name="hiOsc">[out] high oscillator</param>
    /// <param name="diseqcUsed">[out] diseqc used for this channel (0-6)</param>
    protected void GetDisEqcSettings(ref DVBChannel ch, out int lowOsc, out int hiOsc, out int diseqcUsed)
    {
      diseqcUsed = 0;
      lowOsc = 9750;
      hiOsc = 10600;
      try
      {
        string filename = String.Format(@"database\card_{0}.xml", _card.FriendlyName);

        int lnbKhz = 0;
        int diseqc = 0;
        int lnbKind = 0;
        // lnb config
        int lnb0MHZ = 0;
        int lnb1MHZ = 0;
        int lnbswMHZ = 0;
        int cbandMHZ = 0;
        int circularMHZ = 0;

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(filename))
        {
          lnb0MHZ = xmlreader.GetValueAsInt("dvbs", "LNB0", 9750);
          lnb1MHZ = xmlreader.GetValueAsInt("dvbs", "LNB1", 10600);
          lnbswMHZ = xmlreader.GetValueAsInt("dvbs", "Switch", 11700);
          cbandMHZ = xmlreader.GetValueAsInt("dvbs", "CBand", 5150);
          circularMHZ = xmlreader.GetValueAsInt("dvbs", "Circular", 10750);
          switch (ch.DiSEqC)
          {
            case 1:
              // config a
              lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb", 22);
              diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc", 1);
              lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind", 0);
              Log.Write("DVBGraph: using profile diseqc 1 LNB:{0} KHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              break;
            case 2:
              // config b
              lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb2", 22);
              diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 1);
              lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind2", 0);
              Log.Write("DVBGraph: using profile diseqc 2 LNB:{0} KHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              break;
            case 3:
              // config c
              lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb3", 22);
              diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 1);
              lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind3", 0);
              Log.Write("DVBGraph: using profile diseqc 3 LNB:{0} KHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              break;
            //
            case 4:
              // config d
              lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb4", 22);
              diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 1);
              lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind4", 0);
              Log.Write("DVBGraph: using profile diseqc 4 LNB:{0} KHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              //
              break;
          }// switch(disNo)
        }//using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(m_cardFilename))

        switch (lnbKind)
        {
          case 0: // KU-Band
            break;
          case 1: // C-Band
            break;
          case 2: // Circular-Band
            break;
        }
        // set values to dvbchannel-object
        // set the lnb parameter 
        ch.LNBKHz = lnbswMHZ * 1000;
        if (ch.Frequency >= lnbswMHZ * 1000)
        {
          ch.LNBFrequency = lnb1MHZ;
          //ch.LNBKHz = lnbKhz;
        }
        else
        {
          ch.LNBFrequency = lnb0MHZ;
          //ch.LNBKHz = lnbKhz;
        }
        lowOsc = lnb0MHZ;
        hiOsc = lnb1MHZ;
        diseqcUsed = diseqc;
        Log.WriteFile(Log.LogType.Capture, "DVBGraph: LNB Settings: freq={0} lnbKhz={1} lnbFreq={2} diseqc={3}", ch.Frequency, ch.LNBKHz, ch.LNBFrequency, ch.DiSEqC);
      }
      catch (Exception)
      {
      }
    } //void GetDisEqcSettings(TunerLib.IDVBTuneRequest tuneRequest)

    /// <summary>
    /// SetDVBSInputRangeParameter()
    /// This method will set the BDA IDVBSTuningSpace.InputRange parameter
    /// based on the disEqc number specified
    /// </summary>
    /// <param name="disEqcUsed">disEqc number (0-6)</param>
    /// <param name="dvbSpace">DVB tuningspace</param>
    protected void SetDVBSInputRangeParameter(int disEqcUsed, TunerLib.IDVBSTuningSpace dvbSpace)
    {
      try
      {
        if (dvbSpace == null) return;
        //
        // A:LOWORD -> LOBYTE -> Bit0 for Position (0-A,1-B)
        // B:LOWORD -> HIBYTE -> Bit0 for 22Khz    (0-Off,1-On)
        // C:HIWORD -> LOBYTE -> Bit0 for Option   (0-A,1-B)
        // D:HIWORD -> HIBYTE -> Bit0 for Burst    (0-Off,1-On)
        // hi         low        hi        low
        // 87654321 | 87654321 | 87654321 | 87654321 | 
        //        D          C          B          A
        long inputRange = 0;
        switch (disEqcUsed)
        {
          case 0: //none
            Log.Write("DVBGraph: disEqc:none");
            return;
          case 1: //simple A
            Log.Write("DVBGraph: disEqc:simple A (not supported)");
            return;
          case 2: //simple B
            Log.Write("DVBGraph: disEqc:simple B (not supported)");
            return;
          case 3: //Level 1 A/A
            Log.Write("DVBGraph: disEqc:level 1 A/A");
            inputRange = 0;
            break;
          case 4: //Level 1 B/A
            Log.Write("DVBGraph: disEqc:level 1 B/A");
            inputRange = 1 << 16;
            break;
          case 5: //Level 1 A/B
            Log.Write("DVBGraph: disEqc:level 1 A/B");
            inputRange = 1;
            break;
          case 6: //Level 1 B/B
            Log.Write("DVBGraph: disEqc:level 1 B/B");
            inputRange = (1 << 16) + 1;
            break;
        }
        // test with burst on
        //inputRange|=1<<24;

        if (_currentTuningObject.LNBKHz == 1) // 22khz 
          inputRange |= (1 << 8);

        Log.Write("DVBGraph: Set inputrange to:{0:X}", inputRange);
        dvbSpace.InputRange = inputRange.ToString();
      }
      catch (Exception)
      {
      }
    }

    protected void CheckVideoResolutionChanges()
    {
      if (GUIGraphicsContext.Vmr9Active) return;
      if (_graphState != State.Viewing) return;
      if (_videoWindowInterface == null || _basicVideoInterFace == null) return;
      int aspectX, aspectY;
      int videoWidth = 1, videoHeight = 1;
      if (_basicVideoInterFace != null)
      {
        _basicVideoInterFace.GetVideoSize(out videoWidth, out videoHeight);
      }
      aspectX = videoWidth;
      aspectY = videoHeight;
      if (_basicVideoInterFace != null)
      {
        _basicVideoInterFace.GetPreferredAspectRatio(out aspectX, out aspectY);
      }
      if (videoHeight != _videoHeight || videoWidth != _videoWidth ||
        aspectX != _aspectRatioX || aspectY != _aspectRatioY)
      {
        GUIGraphicsContext_OnVideoWindowChanged();
      }
    }

    protected void UpdateVideoState()
    {
      bool isViewing = Recorder.IsCardViewing(_cardId);
      if (!isViewing) return;
      //      Log.Write("packets:{0} pmt:{1:X}  vmr9:{2} fps:{3} locked:{4} quality:{5} level:{6}",
      //      _streamDemuxer.ReceivingPackets, _lastPMTVersion, GUIGraphicsContext.Vmr9Active, GUIGraphicsContext.Vmr9FPS, TunerLocked(), SignalQuality(), SignalStrength());

      // do we receive any packets?
      if (!SignalPresent())
      {
        TimeSpan ts = DateTime.Now - _signalLostTimer;
        if (ts.TotalSeconds < 5)
        {
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
          return;
        }
        //no, then state = no signal
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
        return;
      }/*
      else if (_streamDemuxer.IsScrambled)
      {
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.Scrambled;
        _signalLostTimer = DateTime.Now;
        return;
      }*/
      else if (GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS < 1f)
      {
        if ((g_Player.Playing && !g_Player.Paused) || (!g_Player.Playing))
        {
          TimeSpan ts = DateTime.Now - _signalLostTimer;
          if (ts.TotalSeconds < 5)
          {
            VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
            return;
          }
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
          return;
        }
      }
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      _signalLostTimer = DateTime.Now;
    }

    protected bool ProcessEpg()
    {
      _epgGrabber.Process();
      if (_epgGrabber.Done)
      {
        _epgGrabber.Reset();
        if (_graphState == State.Epg)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraph:EPG done");
          _mediaControl.Stop();
          _isGraphRunning = false;
          //_graphState = State.Created;
          return true;
        }
      }
      return false;
    }


    public void Process()
    {
      if (_graphState == State.None) return;
      if (_inScanningMode == false)
      {
        if (_cardProperties.IsCISupported() && _refreshPmtTable == true)
        {
          //
          //we need to receive & transmit the PMT to the card as fast as possible
        }
        else
        {
          TimeSpan tsProc = DateTime.Now - _processTimer;
          if (tsProc.TotalSeconds < 5) return;
          _processTimer = DateTime.Now;
        }
      }
      UpdateSignalPresent();

      if (_graphState == State.Created) return;

      if (_inScanningMode == true) return;

      if (_streamDemuxer != null)
        _streamDemuxer.Process();

      if (ProcessEpg()) return;

      if (_graphState != State.Epg)
      {
        if (!_inScanningMode)
        {
          UpdateVideoState();
        }
        if (_graphState == State.Viewing)
        {
          if (GUIGraphicsContext.Vmr9Active && _vmr9 != null)
          {
            _vmr9.Process();
          }
        }

        TimeSpan ts = DateTime.Now - _updateTimer;

        if (ts.TotalMilliseconds > 800)
        {
          if (!GUIGraphicsContext.Vmr9Active && !g_Player.Playing)
          {
            CheckVideoResolutionChanges();
          }
          _updateTimer = DateTime.Now;
        }
      }

      if (SignalPresent())
      {
        if (_refreshPmtTable && Network() != NetworkType.ATSC)
        {
          //FilterState state;
          //_mediaControl.GetState(50, out state);
          //Log.Write("graph:{0}", state);
          bool gotPMT = false;
          _refreshPmtTable = false;
          //Log.Write("DVBGraph:Get PMT {0}", _graphState);
          IntPtr pmtMem = Marshal.AllocCoTaskMem(4096);// max. size for pmt
          if (pmtMem != IntPtr.Zero)
          {
            _analyzerInterface.SetPMTProgramNumber(_currentTuningObject.ProgramNumber);
            int res = _analyzerInterface.GetPMTData(pmtMem);
            if (res != -1)
            {
              byte[] pmt = new byte[res];
              int version = -1;
              Marshal.Copy(pmtMem, pmt, 0, res);
              version = ((pmt[5] >> 1) & 0x1F);
              int pmtProgramNumber = (pmt[3] << 8) + pmt[4];
              if (pmtProgramNumber == _currentTuningObject.ProgramNumber)
              {
                gotPMT = true;
                //Log.Write("DVBGraph:Got PMT version:{0} {1}", version, _lastPMTVersion);
                if (_lastPMTVersion != version)
                {
                  Log.Write("DVBGraph:Got PMT version:{0}", version);
                  m_streamDemuxer_OnPMTIsChanged(pmt);
                }
                else
                {
                  //	Log.Write("DVBGraph:Got old PMT version:{0} {1}",_lastPMTVersion,version);
                }
              }
              else
              {
                //ushort chcount = 0;
                //_analyzerInterface.GetChannelCount(ref chcount);
                //Log.Write("DVBGraph:Got wrong PMT:{0} {1} channels:{2}", pmtProgramNumber, _currentTuningObject.ProgramNumber, chcount);
              }
              pmt = null;
            }
            Marshal.FreeCoTaskMem(pmtMem);
          }

          if (!gotPMT)
          {
            _refreshPmtTable = true;
          }
        }
      }

      if (_graphState != State.Epg)
      {
        if (_graphPaused /*&& !_streamDemuxer.IsScrambled*/)
        {
          _graphPaused = false;
          if (m_IStreamBufferSink != null)
          {
            long refTime = 0;
            m_IStreamBufferSink.SetAvailableFilter(ref refTime);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SEEK_POSITION, 0, 0, 0, 0, 0, null);
            GUIGraphicsContext.SendMessage(msg);
          }
        }
        /*
        if (SignalPresent())
        {
          //if (_streamDemuxer.IsScrambled)
          {
            if (GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS < 1f)
            {
              if (_lastPMTVersion >= 0 && _pmtRetyCount < 3)
              {
                TimeSpan ts = DateTime.Now - _pmtTimer;
                if (ts.TotalSeconds >= 3)
                {
                  _pmtRetyCount++;
                  SendPMT();
                }
              }
            }
          }
        }*/
      }
    }//public void Process()

    #endregion

    protected void GetTvChannelFromDatabase(TVChannel channel)
    {
      int bandWidth = -1;
      int frequency = -1, ONID = -1, TSID = -1, SID = -1;
      int audioPid = -1, videoPid = -1, teletextPid = -1, pmtPid = -1, pcrPid = -1;
      string providerName;
      int audio1, audio2, audio3, ac3Pid;
      string audioLanguage, audioLanguage1, audioLanguage2, audioLanguage3;
      bool HasEITPresentFollow, HasEITSchedule;
      _currentTuningObject = null;
      switch (_networkType)
      {
        case NetworkType.ATSC:
          {
            //get the ATSC tuning details from the tv database
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneChannel() get ATSC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0, physicalChannel = 0;
            int minorChannel = 0, majorChannel = 0;
            TVDatabase.GetATSCTuneRequest(channel.ID, out physicalChannel, out providerName, out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out minorChannel, out majorChannel, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
            if (physicalChannel == -1)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
              return;
            }
            frequency = 0;
            symbolrate = 0;
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz physicalChannel:{1} major channel:{2} minor channel:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8} video:0x{9:X} audio:0x{10:X} pcr:0x{11:X}",
              frequency, physicalChannel, minorChannel, majorChannel, modulation, ONID, TSID, SID, providerName, videoPid, audioPid, pcrPid);

            _currentTuningObject = new DVBChannel();
            _currentTuningObject.PhysicalChannel = physicalChannel;
            _currentTuningObject.MinorChannel = minorChannel;
            _currentTuningObject.MajorChannel = majorChannel;
            _currentTuningObject.Frequency = frequency;
            _currentTuningObject.Symbolrate = symbolrate;
            _currentTuningObject.FEC = innerFec;
            _currentTuningObject.Modulation = modulation;
            _currentTuningObject.NetworkID = ONID;
            _currentTuningObject.TransportStreamID = TSID;
            _currentTuningObject.ProgramNumber = SID;
            _currentTuningObject.AudioPid = audioPid;
            _currentTuningObject.VideoPid = videoPid;
            _currentTuningObject.TeletextPid = teletextPid;
            _currentTuningObject.SubtitlePid = 0;
            _currentTuningObject.PMTPid = pmtPid;
            _currentTuningObject.ServiceName = channel.Name;
            _currentTuningObject.AudioLanguage = audioLanguage;
            _currentTuningObject.AudioLanguage1 = audioLanguage1;
            _currentTuningObject.AudioLanguage2 = audioLanguage2;
            _currentTuningObject.AudioLanguage3 = audioLanguage3;
            _currentTuningObject.AC3Pid = ac3Pid;
            _currentTuningObject.PCRPid = pcrPid;
            _currentTuningObject.Audio1 = audio1;
            _currentTuningObject.Audio2 = audio2;
            _currentTuningObject.Audio3 = audio3;
            _currentTuningObject.HasEITSchedule = HasEITSchedule;
            _currentTuningObject.HasEITPresentFollow = HasEITPresentFollow;
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneChannel() submit tuning request");
          } break;

        case NetworkType.DVBC:
          {
            //get the DVB-C tuning details from the tv database
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneChannel() get DVBC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0;
            TVDatabase.GetDVBCTuneRequest(channel.ID, out providerName, out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
            if (frequency <= 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
              return;
            }
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
              frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, providerName);
            //bool needSwitch = true;
            /*if (_currentTuningObject!=null)
            {
              if (_currentTuningObject.Frequency==frequency &&
                _currentTuningObject.Symbolrate==symbolrate &&
                _currentTuningObject.Modulation==innerFec &&
                _currentTuningObject.FEC==innerFec &&
                _currentTuningObject.NetworkID==ONID &&
                _currentTuningObject.TransportStreamID==TSID)
              {
                needSwitch=false;
              }
            }*/
            _currentTuningObject = new DVBChannel();
            _currentTuningObject.Frequency = frequency;
            _currentTuningObject.Symbolrate = symbolrate;
            _currentTuningObject.FEC = innerFec;
            _currentTuningObject.Modulation = modulation;
            _currentTuningObject.NetworkID = ONID;
            _currentTuningObject.TransportStreamID = TSID;
            _currentTuningObject.ProgramNumber = SID;
            _currentTuningObject.AudioPid = audioPid;
            _currentTuningObject.VideoPid = videoPid;
            _currentTuningObject.TeletextPid = teletextPid;
            _currentTuningObject.SubtitlePid = 0;
            _currentTuningObject.PMTPid = pmtPid;
            _currentTuningObject.ServiceName = channel.Name;
            _currentTuningObject.AudioLanguage = audioLanguage;
            _currentTuningObject.AudioLanguage1 = audioLanguage1;
            _currentTuningObject.AudioLanguage2 = audioLanguage2;
            _currentTuningObject.AudioLanguage3 = audioLanguage3;
            _currentTuningObject.AC3Pid = ac3Pid;
            _currentTuningObject.Audio1 = audio1;
            _currentTuningObject.Audio2 = audio2;
            _currentTuningObject.Audio3 = audio3;
            _currentTuningObject.PCRPid = pcrPid;
            _currentTuningObject.HasEITPresentFollow = HasEITPresentFollow;
            _currentTuningObject.HasEITSchedule = HasEITSchedule;

          } break;

        case NetworkType.DVBS:
          {
            //get the DVB-S tuning details from the tv database
            //for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneChannel() get DVBS tuning details");
            DVBChannel ch = new DVBChannel();
            if (TVDatabase.GetSatChannel(channel.ID, 1, ref ch) == false)//only television
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
              return;
            }
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
              ch.Frequency, ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID, ch.ProgramNumber, ch.ServiceProvider);

            bool needSwitch = true;
            /*if (_currentTuningObject!=null)
            {
              if (_currentTuningObject.Frequency==frequency &&
                  _currentTuningObject.FEC==ch.FEC &&
                  _currentTuningObject.Polarity==ch.Polarity &&
                  _currentTuningObject.LNBFrequency==ch.LNBFrequency &&
                  _currentTuningObject.LNBKHz==ch.LNBKHz &&
                  _currentTuningObject.DiSEqC==ch.DiSEqC &&
                  _currentTuningObject.NetworkID==ch.NetworkID&&
                  _currentTuningObject.TransportStreamID==ch.TransportStreamID)
              {
                needSwitch=false;
              }
            }*/
            _currentTuningObject = new DVBChannel();
            _currentTuningObject.Frequency = ch.Frequency;
            _currentTuningObject.Symbolrate = ch.Symbolrate;
            _currentTuningObject.FEC = ch.FEC;
            _currentTuningObject.Polarity = ch.Polarity;
            _currentTuningObject.NetworkID = ch.NetworkID;
            _currentTuningObject.TransportStreamID = ch.TransportStreamID;
            _currentTuningObject.ProgramNumber = ch.ProgramNumber;
            _currentTuningObject.AudioPid = ch.AudioPid;
            _currentTuningObject.VideoPid = ch.VideoPid;
            _currentTuningObject.TeletextPid = ch.TeletextPid;
            _currentTuningObject.SubtitlePid = 0;
            _currentTuningObject.PMTPid = ch.PMTPid;
            _currentTuningObject.ServiceName = channel.Name;
            _currentTuningObject.AudioLanguage = ch.AudioLanguage;
            _currentTuningObject.AudioLanguage1 = ch.AudioLanguage1;
            _currentTuningObject.AudioLanguage2 = ch.AudioLanguage2;
            _currentTuningObject.AudioLanguage3 = ch.AudioLanguage3;
            _currentTuningObject.AC3Pid = ch.AC3Pid;
            _currentTuningObject.PCRPid = ch.PCRPid;
            _currentTuningObject.Audio1 = ch.Audio1;
            _currentTuningObject.Audio2 = ch.Audio2;
            _currentTuningObject.Audio3 = ch.Audio3;
            _currentTuningObject.DiSEqC = ch.DiSEqC;
            _currentTuningObject.LNBFrequency = ch.LNBFrequency;
            _currentTuningObject.LNBKHz = ch.LNBKHz;
            _currentTuningObject.HasEITPresentFollow = ch.HasEITPresentFollow;
            _currentTuningObject.HasEITSchedule = ch.HasEITSchedule;
          } break;

        case NetworkType.DVBT:
          {
            //get the DVB-T tuning details from the tv database
            //for DVB-T this is the frequency, ONID , TSID and SID
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneChannel() get DVBT tuning details");
            TVDatabase.GetDVBTTuneRequest(channel.ID, out providerName, out frequency, out ONID, out TSID, out SID, out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth, out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3, out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
            if (frequency <= 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
              return;
            }
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency, ONID, TSID, SID, providerName);
            //get the IDVBTLocator interface from the new tuning request

            bool needSwitch = true;
            /*if (_currentTuningObject!=null)
            {
              if (_currentTuningObject.Frequency==frequency &&
                  _currentTuningObject.NetworkID==ONID &&
                  _currentTuningObject.TransportStreamID==TSID)
              {
                needSwitch=false;
              }
            }*/
            _currentTuningObject = new DVBChannel();
            _currentTuningObject.Bandwidth = bandWidth;
            _currentTuningObject.Frequency = frequency;
            _currentTuningObject.NetworkID = ONID;
            _currentTuningObject.TransportStreamID = TSID;
            _currentTuningObject.ProgramNumber = SID;
            _currentTuningObject.AudioPid = audioPid;
            _currentTuningObject.VideoPid = videoPid;
            _currentTuningObject.TeletextPid = teletextPid;
            _currentTuningObject.SubtitlePid = 0;
            _currentTuningObject.PMTPid = pmtPid;
            _currentTuningObject.ServiceName = channel.Name;
            _currentTuningObject.AudioLanguage = audioLanguage;
            _currentTuningObject.AudioLanguage1 = audioLanguage1;
            _currentTuningObject.AudioLanguage2 = audioLanguage2;
            _currentTuningObject.AudioLanguage3 = audioLanguage3;
            _currentTuningObject.AC3Pid = ac3Pid;
            _currentTuningObject.PCRPid = pcrPid;
            _currentTuningObject.Audio1 = audio1;
            _currentTuningObject.Audio2 = audio2;
            _currentTuningObject.Audio3 = audio3;
            _currentTuningObject.HasEITPresentFollow = HasEITPresentFollow;
            _currentTuningObject.HasEITSchedule = HasEITSchedule;
          } break;
      }	//switch (_networkType)
    }
    #region Tuning
    /// <summary>
    /// Switches / tunes to another TV channel
    /// </summary>
    /// <param name="iChannel">New channel</param>
    /// <remarks>
    /// Graph should be viewing or timeshifting. 
    /// </remarks>
    public void TuneChannel(TVChannel channel)
    {
      //bool restartGraph=false;
      _lastPMTVersion = -1;
      _pmtRetyCount = 0;
      _inScanningMode = false;
      //bool restartGraph=false;
      try
      {
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
#if USEMTSWRITER
				/*if (_graphState==State.TimeShifting)
				{
					string fname=Recorder.GetTimeShiftFileNameByCardId(_cardId);
					if (g_Player.Playing && g_Player.CurrentFile == fname)
					{
						restartGraph=true;
						g_Player.PauseGraph();
						_mediaControl.Stop();
					}
				}*/
#else
        string fname = Recorder.GetTimeShiftFileNameByCardId(_cardId);
        if (g_Player.Playing && g_Player.CurrentFile == fname)
        {
          _graphPaused = true;
        }
#endif
        if (_vmr9 != null) _vmr9.Enable(false);

        _currentChannelNumber = channel.Number;
        _startTimer = DateTime.Now;

        Log.WriteFile(Log.LogType.Capture, "DVBGraph:TuneChannel() tune to channel:{0}", channel.ID);

        GetTvChannelFromDatabase(channel);
        if (_currentTuningObject == null)
        {
          return;
        }
        SubmitTuneRequest(_currentTuningObject);

        if (_currentTuningObject.AC3Pid <= 0) _currentTuningObject.AC3Pid = -1;
        if (_currentTuningObject.AudioPid <= 0) _currentTuningObject.AudioPid = -1;
        if (_currentTuningObject.Audio1 <= 0) _currentTuningObject.Audio1 = -1;
        if (_currentTuningObject.Audio2 <= 0) _currentTuningObject.Audio2 = -1;
        if (_currentTuningObject.Audio2 <= 0) _currentTuningObject.Audio2 = -1;
        if (_currentTuningObject.SubtitlePid <= 0) _currentTuningObject.SubtitlePid = -1;
        if (_currentTuningObject.TeletextPid <= 0) _currentTuningObject.TeletextPid = -1;
        if (_currentTuningObject.VideoPid <= 0) _currentTuningObject.VideoPid = -1;
        if (_currentTuningObject.PMTPid <= 0) _currentTuningObject.PMTPid = -1;
        if (_currentTuningObject.PCRPid <= 0) _currentTuningObject.PCRPid = -1;
        try
        {
          DirectShowUtil.EnableDeInterlace(_graphBuilder);

        }
        catch (Exception ex)
        {
          Log.Write(ex);
        }
        Log.WriteFile(Log.LogType.Capture, "DVBGraph:TuneChannel() done freq:{0} ONID:{1} TSID:{2} prog:{3} audio:{4:X} video:{5:X} pmt:{6:X} ac3:{7:X} txt:{8:X}",
                                            _currentTuningObject.Frequency,
                                            _currentTuningObject.NetworkID, _currentTuningObject.TransportStreamID,
                                            _currentTuningObject.ProgramNumber, _currentTuningObject.Audio1,
                                            _currentTuningObject.VideoPid, _currentTuningObject.PMTPid,
                                            _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid);

        //SendPMT();
        _refreshPmtTable = true;
        _lastPMTVersion = -1;
        _pmtRetyCount = 0;
        _analyzerInterface.ResetParser();
        SetupMTSDemuxerPin();
        Log.WriteFile(Log.LogType.Capture, false, "DVBGraph:set mpeg2demuxer video:0x{0:x} audio:0x{1:X} ac3:0x{2:X}",
                        _currentTuningObject.VideoPid, _currentTuningObject.AudioPid, _currentTuningObject.AC3Pid);
        SetupDemuxer(_pinDemuxerVideo, _currentTuningObject.VideoPid, _pinDemuxerAudio, _currentTuningObject.AudioPid, _pinAC3Out, _currentTuningObject.AC3Pid);
        if (_streamDemuxer != null)
        {
          _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid, _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid, _currentTuningObject.Audio3, _currentTuningObject.ServiceName, _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
          _streamDemuxer.OnTuneNewChannel();
        }

        //map teletext pid to ttx output pin of mpeg2 demultiplexer
        if (_pinTeletext != null)
        {
          SetupDemuxerPin(_pinTeletext, _currentTuningObject.TeletextPid, (int)MediaSampleContent.TransportPacket, true);
        }
      }
      finally
      {
#if USEMTSWRITER
				/*if (restartGraph)
				{
					string fname=Recorder.GetTimeShiftFileNameByCardId(_cardId);
					StartTimeShifting(null,fname);
					g_Player.ContinueGraph();
				}*/
#else
        //				if (restartGraph)
        //         g_Player.ContinueGraph();
#endif
        if (_vmr9 != null) _vmr9.Enable(true);
        _signalLostTimer = DateTime.Now;
        UpdateVideoState();

        if (m_IStreamBufferSink != null)
        {
          long refTime = 0;
          m_IStreamBufferSink.SetAvailableFilter(ref refTime);
        }
      }
      _scanPidListReady = false;
      _scanPidList.Clear();
      try
      {
        //_analyzerInterface.SetPidFilterCallback(this);
      }
      catch (Exception) { }

    }//public void TuneChannel(AnalogVideoStandard standard,int iChannel,int country)


    public void TuneFrequency(int frequency)
    {
    }


    #region TuneRequest
    protected virtual void SubmitTuneRequest(DVBChannel ch)
    {
    }

    #endregion
    #region AutoTuning
    /// <summary>
    /// Tune to a specific channel
    /// </summary>
    /// <param name="tuningObject">
    /// DVBChannel object containing the tuning parameter.
    /// </param>
    /// <remarks>
    /// Graph should be created 
    /// </remarks>
    public void Tune(object tuningObject, int disecqNo)
    {
      //if no network provider then return;

      if (tuningObject == null) return;
      _inScanningMode = true;
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      //start viewing if we're not yet viewing
      if (!_isGraphRunning)
      {

        Log.Write("Start graph!");
        if (_mediaControl == null)
          _mediaControl = (IMediaControl)_graphBuilder;
        int hr = _mediaControl.Run();
        if (hr < 0)
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
        }

        _isGraphRunning = true;
      }


      _currentTuningObject = (DVBChannel)tuningObject;

      SetupDiseqc(disecqNo);


      _scanPidListReady = false;
      _scanPidList.Clear();
      try
      {
        _analyzerInterface.SetPidFilterCallback(this);
      }
      catch (Exception) { }


      SubmitTuneRequest(_currentTuningObject);
      _analyzerInterface.ResetParser();

      ArrayList pids = new ArrayList();
      if (Network() == NetworkType.ATSC)
      {
        pids.Add((ushort)0x1ffb);
        SendHWPids(pids);
        SetupDemuxerPin(_pinDemuxerSections, 0x1ffb, (int)MediaSampleContent.Mpeg2PSI, true);
      }
      else
      {
        pids.Add((ushort)0);
        pids.Add((ushort)0x10);
        pids.Add((ushort)0x11);
        SendHWPids(pids);
        SetupDemuxerPin(_pinDemuxerSections, 0, (int)MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(_pinDemuxerSections, 0x10, (int)MediaSampleContent.Mpeg2PSI, false);
        SetupDemuxerPin(_pinDemuxerSections, 0x11, (int)MediaSampleContent.Mpeg2PSI, false);
      }

      Log.Write("DVBGraph: wait for tunerlock");
      //wait until tuner has locked
      DateTime dt = DateTime.Now;
      while (true)
      {
        TimeSpan ts = DateTime.Now - dt;
        if (ts.TotalMilliseconds >= 2000) break;
        System.Threading.Thread.Sleep(200);
      }
      _signalLostTimer = DateTime.Now;
      UpdateSignalPresent();
      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
    }//public void Tune(object tuningObject)

    protected virtual void SetupDiseqc(int disecqNo)
    {
    }

    /// <summary>
    /// Store any new tv and/or radio channels found in the tvdatabase
    /// </summary>
    /// <param name="radio">if true:Store radio channels found in the database</param>
    /// <param name="tv">if true:Store tv channels found in the database</param>
    public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels, ref int newRadioChannels, ref int updatedRadioChannels)
    {
      //it may take a while before signal quality/level is correct
      if (_filterDvbAnalyzer == null) return;
      Log.WriteFile(Log.LogType.Capture, "DVBGraph: StoreChannels() signal level:{0} signal quality:{1} locked:{2}", SignalStrength(), SignalQuality(), _tunerLocked);
      TVDatabase.ClearCache();
      //get list of current tv channels present in the database
      List<TVChannel> tvChannels = new List<TVChannel>();
      TVDatabase.GetChannels(ref tvChannels);

      DVBSections.Transponder transp;
      transp.channels = null;
      DateTime dt = DateTime.Now;

      //wait until PAT/SDT has been received (max 5 secs)
      while (true)
      {
        TimeSpan ts = DateTime.Now - dt;
        if (ts.TotalMilliseconds > 8000) break;
        System.Threading.Thread.Sleep(2000);
        if (_scanPidListReady) break;
      }
      ArrayList hwPids = (ArrayList)_scanPidList.Clone();

      string pidList = "";
      for (int i = 0; i < hwPids.Count; ++i)
        pidList += String.Format("0x{0:X},", (ushort)hwPids[i]);

      Log.Write("check...{0} pids:{1} {2}", _scanPidListReady, hwPids.Count, pidList);
      if (_scanPidListReady == false) return;

      //setup MPEG2 demuxer so it sends the PMT's to the analyzer filter
      for (int i = 0; i < hwPids.Count; ++i)
      {
        ushort pid = (ushort)hwPids[i];
        SetupDemuxerPin(_pinDemuxerSections, pid, (int)MediaSampleContent.Mpeg2PSI, (i == 0));
      }

      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
      SendHWPids(hwPids);

      int offset = 3;
      if (Network() == NetworkType.ATSC)
      {
        offset = 1;
      }
      using (DVBSections sections = new DVBSections())
      {
        sections.DemuxerObject = _streamDemuxer;
        sections.Timeout = 2500;

        //wait until all PMT's are received 
        dt = DateTime.Now;
        transp.channels = new ArrayList();
        bool allFound = true;
        bool newChannelsFound = false;
        bool[] channelReady = new bool[hwPids.Count];
        do
        {
          newChannelsFound = false;
          allFound = true;
          for (int index = 0; index < hwPids.Count - offset; index++)
          {
            if (channelReady[index]) continue;
            if (_analyzerInterface.IsChannelReady(index) != 0)
            {
              //channel not ready
              allFound = false;
            }
            else
            {
              //channel is ready
              DVBSections.ChannelInfo chi = new MediaPortal.TV.Recording.DVBSections.ChannelInfo();
              UInt16 len = 0;
              int hr = 0;
              hr = _analyzerInterface.GetCISize(ref len);
              IntPtr mmch = Marshal.AllocCoTaskMem(len);
              try
              {
                hr = _analyzerInterface.GetChannel((UInt16)(index), mmch);
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
              }
              finally
              {
                Marshal.FreeCoTaskMem(mmch);
              }

              transp.channels.Add(chi);

              channelReady[index] = true;
              newChannelsFound = true;
              Log.Write("channel:{0}/{1} pid:0x{2:X} ready", index, (hwPids.Count - offset), hwPids[offset + index]);
              hwPids[offset + index] = (ushort)0x2000;
            }
          }// for (int index = 0; index < hwPids.Count-offset; ++index)

          // update h/w pids
          if (!allFound && newChannelsFound)
          {
            ArrayList pids = new ArrayList();
            for (int i = 0; i < hwPids.Count; ++i)
            {
              ushort pid = (ushort)hwPids[i];
              if (pid < 0x1fff)
                pids.Add(pid);
            }
            SendHWPids(pids);
          }


          if (!allFound)
          {
            if (newChannelsFound)
            {
              dt = DateTime.Now;
            }
            System.Threading.Thread.Sleep(500);
            TimeSpan ts = DateTime.Now - dt;
            if (ts.TotalMilliseconds >= 2000) break;
          }
        } while (!allFound);
      }

      if (transp.channels == null)
      {
        Log.WriteFile(Log.LogType.Capture, "DVBGraph: found no channels", transp.channels);
        return;
      }

      Log.WriteFile(Log.LogType.Capture, "DVBGraph: found {0}/{1} channels", transp.channels.Count, hwPids.Count - offset);
      for (int i = 0; i < transp.channels.Count; ++i)
      {
        DVBSections.ChannelInfo info = (DVBSections.ChannelInfo)transp.channels[i];
        if (info.service_provider_name == null) info.service_provider_name = "";
        if (info.service_name == null) info.service_name = "";

        info.service_provider_name = info.service_provider_name.Trim();
        info.service_name = info.service_name.Trim();
        if (info.service_provider_name.Length == 0)
          info.service_provider_name = Strings.Unknown;
        if (info.service_name.Length == 0)
          info.service_name = String.Format("NoName:{0}{1}{2}{3}", info.networkID, info.transportStreamID, info.serviceID, i);


        bool hasAudio = false;
        bool hasVideo = false;
        info.freq = _currentTuningObject.Frequency;
        DVBChannel newchannel = new DVBChannel();

        _currentTuningObject.VideoPid = 0;
        _currentTuningObject.AudioPid = 0;
        _currentTuningObject.TeletextPid = 0;
        _currentTuningObject.SubtitlePid = 0;
        _currentTuningObject.AC3Pid = 0;
        _currentTuningObject.Audio1 = 0;
        _currentTuningObject.Audio2 = 0;
        _currentTuningObject.Audio3 = 0;
        _currentTuningObject.AudioLanguage = String.Empty;
        _currentTuningObject.AudioLanguage1 = String.Empty;
        _currentTuningObject.AudioLanguage2 = String.Empty;
        _currentTuningObject.AudioLanguage3 = String.Empty;
        //check if this channel has audio/video streams
        int audioOptions = 0;
        if (info.pid_list != null)
        {
          for (int pids = 0; pids < info.pid_list.Count; pids++)
          {
            DVBSections.PMTData data = (DVBSections.PMTData)info.pid_list[pids];
            if (data.isVideo)
            {
              _currentTuningObject.VideoPid = data.elementary_PID;
              hasVideo = true;
            }

            if (data.isAudio)
            {
              switch (audioOptions)
              {
                case 0:
                  _currentTuningObject.Audio1 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      _currentTuningObject.AudioLanguage1 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions++;
                  break;
                case 1:
                  _currentTuningObject.Audio2 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      _currentTuningObject.AudioLanguage2 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions++;
                  break;
                case 2:
                  _currentTuningObject.Audio3 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                      _currentTuningObject.AudioLanguage3 = DVBSections.GetLanguageFromCode(data.data);
                  }
                  audioOptions++;
                  break;

              }

              if (hasAudio == false)
              {
                _currentTuningObject.AudioPid = data.elementary_PID;
                if (data.data != null)
                {
                  if (data.data.Length == 3)
                    _currentTuningObject.AudioLanguage = DVBSections.GetLanguageFromCode(data.data);
                }
                hasAudio = true;
              }
            }
            if (data.isAC3Audio)
            {
              hasAudio = true;
              _currentTuningObject.AC3Pid = data.elementary_PID;
            }

            if (data.isTeletext)
            {
              _currentTuningObject.TeletextPid = data.elementary_PID;
            }

            if (data.isDVBSubtitle)
            {
              _currentTuningObject.SubtitlePid = data.elementary_PID;
            }
          }
        }

        if (info.serviceType != 1 && info.serviceType != 2)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraph:unknown service type: provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:0x{9:X} videopid:0x{10:X} teletextpid:0x{11:X} program:{12} pcr pid:0x{13:X} service type:{14} major:{15} minor:{16}",
                                            info.service_provider_name,
                                            info.service_name,
                                            info.scrambled,
                                            info.freq,
                                            info.networkID,
                                            info.transportStreamID,
                                            info.serviceID,
                                            hasVideo, ((!hasVideo) && hasAudio),
                                            _currentTuningObject.AudioPid, _currentTuningObject.VideoPid, _currentTuningObject.TeletextPid,
                                            info.program_number,
                                            info.pcr_pid,
                                            info.serviceType, info.majorChannel, info.minorChannel);
          continue;
        }
        Log.WriteFile(Log.LogType.Capture, "DVBGraph:Found provider:{0} service:{1} scrambled:{2} frequency:{3} KHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:0x{9:X} videopid:0x{10:X} teletextpid:0x{11:X} program:{12} pcr pid:0x{13:X} ac3 pid:0x{14:X} major:{15} minor:{16} LCN:{17} type:{18}",
                                            info.service_provider_name,
                                            info.service_name,
                                            info.scrambled,
                                            info.freq,
                                            info.networkID,
                                            info.transportStreamID,
                                            info.serviceID,
                                            hasVideo, ((!hasVideo) && hasAudio),
                                            _currentTuningObject.AudioPid, _currentTuningObject.VideoPid, _currentTuningObject.TeletextPid,
                                            info.program_number,
                                            info.pcr_pid, _currentTuningObject.AC3Pid, info.majorChannel, info.minorChannel, info.LCN, info.serviceType);

        if (info.serviceID == 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraph: channel#{0} has no service id", i);
          continue;
        }
        bool isRadio = ((!hasVideo) && hasAudio);
        bool isTv = (hasVideo);//some tv channels dont have an audio stream

        newchannel.Frequency = info.freq;
        newchannel.ServiceName = info.service_name;
        newchannel.ServiceProvider = info.service_provider_name;
        newchannel.IsScrambled = info.scrambled;
        newchannel.NetworkID = info.networkID;
        newchannel.TransportStreamID = info.transportStreamID;
        newchannel.ProgramNumber = info.serviceID;
        newchannel.FEC = info.fec;
        newchannel.Polarity = _currentTuningObject.Polarity;
        newchannel.Modulation = _currentTuningObject.Modulation;
        newchannel.Symbolrate = _currentTuningObject.Symbolrate;
        newchannel.ServiceType = 1;//tv
        newchannel.AudioPid = _currentTuningObject.AudioPid;
        newchannel.AudioLanguage = _currentTuningObject.AudioLanguage;
        newchannel.VideoPid = _currentTuningObject.VideoPid;
        newchannel.TeletextPid = _currentTuningObject.TeletextPid;
        newchannel.SubtitlePid = _currentTuningObject.SubtitlePid;
        newchannel.Bandwidth = _currentTuningObject.Bandwidth;
        newchannel.PMTPid = info.network_pmt_PID;
        newchannel.Audio1 = _currentTuningObject.Audio1;
        newchannel.Audio2 = _currentTuningObject.Audio2;
        newchannel.Audio3 = _currentTuningObject.Audio3;
        newchannel.AC3Pid = _currentTuningObject.AC3Pid;
        newchannel.PCRPid = info.pcr_pid;
        newchannel.AudioLanguage1 = _currentTuningObject.AudioLanguage1;
        newchannel.AudioLanguage2 = _currentTuningObject.AudioLanguage2;
        newchannel.AudioLanguage3 = _currentTuningObject.AudioLanguage3;
        newchannel.DiSEqC = _currentTuningObject.DiSEqC;
        newchannel.LNBFrequency = _currentTuningObject.LNBFrequency;
        newchannel.LNBKHz = _currentTuningObject.LNBKHz;
        newchannel.PhysicalChannel = _currentTuningObject.PhysicalChannel;
        newchannel.MinorChannel = info.minorChannel;
        newchannel.MajorChannel = info.majorChannel;
        newchannel.HasEITPresentFollow = info.eitPreFollow;
        newchannel.HasEITSchedule = info.eitSchedule;


        if (info.serviceType == 1)//tv
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraph: channel {0} is a tv channel", newchannel.ServiceName);
          //check if this channel already exists in the tv database
          bool isNewChannel = true;
          TVChannel tvChan = new TVChannel();
          tvChan.Name = newchannel.ServiceName;

          int channelId = -1;
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

          tvChan.Scrambled = newchannel.IsScrambled;
          if (isNewChannel)
          {
            //then add a new channel to the database
            tvChan.ID = -1;
            tvChan.Number = TVDatabase.FindFreeTvChannelNumber(newchannel.ProgramNumber);
            tvChan.Sort = 40000;
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: add new channel for {0}:{1}:{2}", tvChan.Name, tvChan.Number, tvChan.Sort);
            int id = TVDatabase.AddChannel(tvChan);
            if (id < 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: failed to add new channel for {0}:{1}:{2} to database", tvChan.Name, tvChan.Number, tvChan.Sort);
            }
            channelId = id;
            newChannels++;
          }
          else
          {
            TVDatabase.UpdateChannel(tvChan, tvChan.Sort);
            updatedChannels++;
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: update channel {0}:{1}:{2} {3}", tvChan.Name, tvChan.Number, tvChan.Sort, tvChan.ID);
          }

          if (Network() == NetworkType.DVBT)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map channel {0} id:{1} to DVBT card:{2}", newchannel.ServiceName, channelId, ID);
            TVDatabase.MapDVBTChannel(newchannel.ServiceName,
              newchannel.ServiceProvider,
              channelId,
              newchannel.Frequency,
              newchannel.NetworkID,
              newchannel.TransportStreamID,
              newchannel.ProgramNumber,
              _currentTuningObject.AudioPid,
              _currentTuningObject.VideoPid,
              _currentTuningObject.TeletextPid,
              newchannel.PMTPid,
              newchannel.Bandwidth,
              newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid, newchannel.PCRPid,
              newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2, newchannel.AudioLanguage3,
              newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);
          }
          if (Network() == NetworkType.DVBC)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map channel {0} id:{1} to DVBC card:{2}", newchannel.ServiceName, channelId, ID);
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
              _currentTuningObject.AudioPid,
              _currentTuningObject.VideoPid,
              _currentTuningObject.TeletextPid,
              newchannel.PMTPid,
              newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid, newchannel.PCRPid,
              newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2, newchannel.AudioLanguage3,
              newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);

          }
          if (Network() == NetworkType.ATSC)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map channel {0} id:{1} to ATSC card:{2}", newchannel.ServiceName, channelId, ID);
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
              _currentTuningObject.AudioPid,
              _currentTuningObject.VideoPid,
              _currentTuningObject.TeletextPid,
              newchannel.PMTPid,
              newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid, newchannel.PCRPid,
              newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2, newchannel.AudioLanguage3,
              newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);

          }

          if (Network() == NetworkType.DVBS)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map channel {0} id:{1} to DVBS card:{2}", newchannel.ServiceName, channelId, ID);
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
        else if (info.serviceType == 2) //radio
        {
          //Log.WriteFile(Log.LogType.Capture,"DVBGraph: channel {0} is a radio channel",newchannel.ServiceName);
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
            RadioStation station = new RadioStation();
            station.Name = newchannel.ServiceName;
            station.Channel = newchannel.ProgramNumber;
            station.Frequency = newchannel.Frequency;
            station.Scrambled = info.scrambled;
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: add new radio channel for {0} {1}", station.Name, station.Frequency);
            int id = RadioDatabase.AddStation(ref station);
            if (id < 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: failed to add new radio channel for {0} {1} to database", station.Name, station.Frequency);
            }
            channelId = id;
            newRadioChannels++;
          }
          else
          {
            updatedRadioChannels++;
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: channel {0} already exists in tv database", newchannel.ServiceName);
          }

          if (Network() == NetworkType.DVBT)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map radio channel {0} id:{1} to DVBT card:{2}", newchannel.ServiceName, channelId, ID);
            RadioDatabase.MapDVBTChannel(newchannel.ServiceName, newchannel.ServiceProvider, channelId, newchannel.Frequency, newchannel.NetworkID, newchannel.TransportStreamID, newchannel.ProgramNumber, _currentTuningObject.AudioPid, newchannel.PMTPid, newchannel.Bandwidth, newchannel.PCRPid);
          }
          if (Network() == NetworkType.DVBC)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map radio channel {0} id:{1} to DVBC card:{2}", newchannel.ServiceName, channelId, ID);
            RadioDatabase.MapDVBCChannel(newchannel.ServiceName, newchannel.ServiceProvider, channelId, newchannel.Frequency, newchannel.Symbolrate, newchannel.FEC, newchannel.Modulation, newchannel.NetworkID, newchannel.TransportStreamID, newchannel.ProgramNumber, _currentTuningObject.AudioPid, newchannel.PMTPid, newchannel.PCRPid);
          }
          if (Network() == NetworkType.ATSC)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map radio channel {0} id:{1} to DVBC card:{2}", newchannel.ServiceName, channelId, ID);
            RadioDatabase.MapATSCChannel(newchannel.ServiceName, newchannel.PhysicalChannel,
              newchannel.MinorChannel,
              newchannel.MajorChannel, newchannel.ServiceProvider, channelId, newchannel.Frequency, newchannel.Symbolrate, newchannel.FEC, newchannel.Modulation, newchannel.NetworkID, newchannel.TransportStreamID, newchannel.ProgramNumber, _currentTuningObject.AudioPid, newchannel.PMTPid, newchannel.PCRPid);
          }
          if (Network() == NetworkType.DVBS)
          {
            Log.WriteFile(Log.LogType.Capture, "DVBGraph: map radio channel {0} id:{1} to DVBS card:{2}", newchannel.ServiceName, channelId, ID);
            newchannel.ID = channelId;

            int scrambled = 0;
            if (newchannel.IsScrambled) scrambled = 1;
            RadioDatabase.MapDVBSChannel(newchannel.ID, newchannel.Frequency, newchannel.Symbolrate,
              newchannel.FEC, newchannel.LNBKHz, 0, newchannel.ProgramNumber,
              0, newchannel.ServiceProvider, newchannel.ServiceName,
              0, 0, newchannel.AudioPid, newchannel.VideoPid, newchannel.AC3Pid,
              0, 0, 0, 0, scrambled,
              newchannel.Polarity, newchannel.LNBFrequency
              , newchannel.NetworkID, newchannel.TransportStreamID, newchannel.PCRPid,
              newchannel.AudioLanguage, newchannel.AudioLanguage1,
              newchannel.AudioLanguage2, newchannel.AudioLanguage3,
              newchannel.ECMPid, newchannel.PMTPid);
          }
          RadioDatabase.MapChannelToCard(channelId, ID);
        }
      }//for (int i=0; i < transp.channels.Count;++i)

      SetLCN();
    }//public void StoreChannels(bool radio, bool tv)

    protected void SetLCN()
    {

      //Log.Write("SetLCN");
      Int16 count = 0;
      while (true)
      {
        string provider;
        Int16 networkId, transportId, serviceID, LCN;
        _analyzerInterface.GetLCN(count, out  networkId, out transportId, out serviceID, out LCN);
        if (networkId > 0 && transportId > 0 && serviceID > 0)
        {

          TVChannel channel = TVDatabase.GetTVChannelByStream(Network() == NetworkType.ATSC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBC, Network() == NetworkType.DVBS, networkId, transportId, serviceID, out provider);
          if (channel != null)
          {
            channel.Sort = LCN;
            //           Log.Write("lcn:{0} network:0x{1:X} transportid:0x{2:X} serviceid:0x{3:X} {4}",LCN , networkId, transportId, serviceID, channel.Name);
            TVDatabase.UpdateChannel(channel, channel.Sort);
          }
          else
          {
            RadioStation station = RadioDatabase.GetStationByStream(Network() == NetworkType.ATSC, Network() == NetworkType.DVBT, Network() == NetworkType.DVBC, Network() == NetworkType.DVBS, networkId, transportId, serviceID, out provider);
            if (station != null)
            {
              station.Sort = LCN;
              RadioDatabase.UpdateStation(station);
            }
            //            Log.Write("unknown channel lcn:{0} network:0x{1:X} transportid:0x{2:X} serviceid:0x{3:X}",LCN, networkId, transportId, serviceID);
          }
        }
        else
        {
          //          Log.Write("LCN total:{0}", count);
          return;
        }
        count++;
      }
    }
    #endregion

    #endregion

    #region Radio
    protected void GetRadioChannelFromDatabase(RadioStation channel)
    {

      int frequency = -1, ONID = -1, TSID = -1, SID = -1, pmtPid = -1, pcrPid = -1;
      int audioPid = -1, bandwidth = 8;
      string providerName;
      _currentTuningObject = null;
      switch (_networkType)
      {
        case NetworkType.ATSC:
          {
            //get the ATSC tuning details from the tv database
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneChannel() get ATSC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0, physicalChannel = 0;
            int minorChannel = 0, majorChannel = 0;
            RadioDatabase.GetATSCTuneRequest(channel.ID, out physicalChannel, out minorChannel, out majorChannel, out providerName, out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out pmtPid, out pcrPid);
            if (physicalChannel < 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for station:{0}", channel.ID);
              return;
            }
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz physicalChannel:{1} symbolrate:{2} innerFec:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8}",
              frequency, physicalChannel, symbolrate, innerFec, modulation, ONID, TSID, SID, providerName);
            _currentTuningObject = new DVBChannel();
            _currentTuningObject.PhysicalChannel = physicalChannel;
            _currentTuningObject.MinorChannel = minorChannel;
            _currentTuningObject.MajorChannel = majorChannel;
            _currentTuningObject.Frequency = frequency;
            _currentTuningObject.Symbolrate = symbolrate;
            _currentTuningObject.FEC = innerFec;
            _currentTuningObject.Modulation = modulation;
            _currentTuningObject.NetworkID = ONID;
            _currentTuningObject.TransportStreamID = TSID;
            _currentTuningObject.ProgramNumber = SID;
            _currentTuningObject.AudioPid = audioPid;
            _currentTuningObject.VideoPid = 0;
            _currentTuningObject.PMTPid = pmtPid;
            _currentTuningObject.PCRPid = pcrPid;
            _currentTuningObject.ServiceName = channel.Name;
          } break;

        case NetworkType.DVBC:
          {
            //get the DVB-C tuning details from the tv database
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneRadioChannel() get DVBC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0;
            RadioDatabase.GetDVBCTuneRequest(channel.ID, out providerName, out frequency, out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID, out audioPid, out pmtPid, out pcrPid);
            if (frequency <= 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for channel:{0}", channel.Channel);
              return;
            }
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
              frequency, symbolrate, innerFec, modulation, ONID, TSID, SID, providerName);

            _currentTuningObject = new DVBChannel();
            _currentTuningObject.Frequency = frequency;
            _currentTuningObject.Symbolrate = symbolrate;
            _currentTuningObject.FEC = innerFec;
            _currentTuningObject.Modulation = modulation;
            _currentTuningObject.NetworkID = ONID;
            _currentTuningObject.TransportStreamID = TSID;
            _currentTuningObject.ProgramNumber = SID;
            _currentTuningObject.AudioPid = audioPid;
            _currentTuningObject.VideoPid = 0;
            _currentTuningObject.TeletextPid = 0;
            _currentTuningObject.SubtitlePid = 0;
            _currentTuningObject.PMTPid = pmtPid;

            _currentTuningObject.PCRPid = pcrPid;
            _currentTuningObject.ServiceName = channel.Name;

          } break;

        case NetworkType.DVBS:
          {
            //get the DVB-S tuning details from the tv database
            //for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneRadioChannel() get DVBS tuning details");
            DVBChannel ch = new DVBChannel();
            if (RadioDatabase.GetDVBSTuneRequest(channel.ID, 0, ref ch) == false)//only radio
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for channel:{0}", channel.Channel);
              return;
            }
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
              ch.Frequency, ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID, ch.ProgramNumber, ch.ServiceProvider);


            _currentTuningObject = new DVBChannel();
            _currentTuningObject.Frequency = ch.Frequency;
            _currentTuningObject.Symbolrate = ch.Symbolrate;
            _currentTuningObject.FEC = ch.FEC;
            _currentTuningObject.Polarity = ch.Polarity;
            _currentTuningObject.NetworkID = ch.NetworkID;
            _currentTuningObject.TransportStreamID = ch.TransportStreamID;
            _currentTuningObject.ProgramNumber = ch.ProgramNumber;
            _currentTuningObject.AudioPid = ch.AudioPid;
            _currentTuningObject.VideoPid = 0;
            _currentTuningObject.TeletextPid = 0;
            _currentTuningObject.SubtitlePid = 0;
            _currentTuningObject.PMTPid = ch.PMTPid;
            _currentTuningObject.ServiceName = channel.Name;
            _currentTuningObject.DiSEqC = ch.DiSEqC;
            _currentTuningObject.LNBFrequency = ch.LNBFrequency;
            _currentTuningObject.LNBKHz = ch.LNBKHz;

          } break;

        case NetworkType.DVBT:
          {
            //get the DVB-T tuning details from the tv database
            //for DVB-T this is the frequency, ONID , TSID and SID
            //Log.WriteFile(Log.LogType.Capture,"DVBGraph:TuneRadioChannel() get DVBT tuning details");
            RadioDatabase.GetDVBTTuneRequest(channel.ID, out providerName, out frequency, out ONID, out TSID, out SID, out audioPid, out pmtPid, out bandwidth, out pcrPid);
            if (frequency <= 0)
            {
              Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:database invalid tuning details for channel:{0}", channel.Channel);
              return;
            }
            Log.WriteFile(Log.LogType.Capture, "DVBGraph:  tuning details: frequency:{0} KHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency, ONID, TSID, SID, providerName);


            _currentTuningObject = new DVBChannel();
            _currentTuningObject.Frequency = frequency;
            _currentTuningObject.NetworkID = ONID;
            _currentTuningObject.TransportStreamID = TSID;
            _currentTuningObject.ProgramNumber = SID;
            _currentTuningObject.AudioPid = audioPid;
            _currentTuningObject.VideoPid = 0;
            _currentTuningObject.TeletextPid = 0;
            _currentTuningObject.SubtitlePid = 0;
            _currentTuningObject.PMTPid = pmtPid;
            _currentTuningObject.PCRPid = pcrPid;
            _currentTuningObject.Bandwidth = bandwidth;
            _currentTuningObject.ServiceName = channel.Name;
          } break;
      }	//switch (_networkType)
      //submit tune request to the tuner
    }
    public void TuneRadioChannel(RadioStation channel)
    {


      try
      {
        _currentChannelNumber = channel.Channel;
        _startTimer = DateTime.Now;
        Log.WriteFile(Log.LogType.Capture, "DVBGraph:TuneRadioChannel() tune to radio station:{0}", channel.Name);
        GetRadioChannelFromDatabase(channel);
        if (_currentTuningObject == null)
        {
          return;
        }
        SubmitTuneRequest(_currentTuningObject);

        Log.WriteFile(Log.LogType.Capture, "DVBGraph:TuneRadioChannel() done");

        if (_streamDemuxer != null)
        {
          _streamDemuxer.OnTuneNewChannel();
          _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid, _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid, _currentTuningObject.Audio3, _currentTuningObject.ServiceName, _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
        }
        //SendPMT();

        _refreshPmtTable = true;

      }
      finally
      {
        _signalLostTimer = DateTime.Now;

      }
      _scanPidListReady = false;
      _scanPidList.Clear();
      _inScanningMode = false;
      _refreshPmtTable = true;
      _lastPMTVersion = -1;
      _pmtRetyCount = 0;
      _analyzerInterface.ResetParser();
      try
      {
        //_analyzerInterface.SetPidFilterCallback(this);
      }
      catch (Exception) { }

    }//public void TuneRadioChannel(AnalogVideoStandard standard,int iChannel,int country)

    public void StartRadio(RadioStation station)
    {
      if (_graphState != State.Radio)
      {
        if (_graphState != State.Created) return;
        if (_vmr9 != null)
        {
          _vmr9.Dispose();
          _vmr9 = null;
        }
        Log.WriteFile(Log.LogType.Capture, "DVBGraph:StartRadio()");


#if USEMTSWRITER
				string fileName=Recorder.GetTimeShiftFileNameByCardId(_cardId);
				StartTimeShifting(null,fileName);
				SetupMTSDemuxerPin();
				return ;
			}
#else
        // add the preferred video/audio codecs
        AddPreferredCodecs(true, false);

        GetRadioChannelFromDatabase(station);
        if (_currentTuningObject == null)
        {
          return;
        }
        if (_currentTuningObject.PCRPid <= 0 || _currentTuningObject.PCRPid >= 0x1fff)
        {
          SetupDemuxer(_pinDemuxerVideo, 0, _pinDemuxerAudio, 0, _pinAC3Out, 0);
          SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.AudioPid, (int)MediaSampleContent.TransportPayload, true);
          SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.PCRPid, (int)MediaSampleContent.TransportPacket, false);
          //setup demuxer MTS pin
          SetupMTSDemuxerPin();

          IMpeg2Demultiplexer mpeg2Demuxer = _filterMpeg2Demultiplexer as IMpeg2Demultiplexer;
          if (mpeg2Demuxer != null)
          {
            Log.Write("DVBGraph:MPEG2 demultiplexer PID mapping:");
            uint pid = 0, sampletype = 0;
            GetPidMap(_pinMPG1Out, ref pid, ref sampletype);
            Log.Write("DVBGraph:  Pin:mpg1 is mapped to pid:{0:x} content:{1}", pid, sampletype);
          }
          if (_graphBuilder.Render(_pinMPG1Out/*_pinDemuxerAudio*/) != 0)
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:Failed to render audio out pin MPEG-2 Demultiplexer");
            return;
          }
        }
        else
        {
          //Log.WriteFile(Log.LogType.Capture,"DVBGraph:StartRadio() render demux output pin");
          if (_graphBuilder.Render(_pinDemuxerAudio) != 0)
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph:Failed to render audio out pin MPEG-2 Demultiplexer");
            return;
          }
        }
        //get the IMediaControl interface of the graph
        if (_mediaControl == null)
          _mediaControl = _graphBuilder as IMediaControl;

        //start the graph
        //Log.WriteFile(Log.LogType.Capture,"DVBGraph: start graph");
        if (_mediaControl != null)
        {
          int hr = _mediaControl.Run();
          if (hr < 0)
          {
            Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
          }
        }
        else
        {
          Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED cannot get IMediaControl");
        }


        TuneRadioChannel(station);
        _isGraphRunning = true;
        _graphState = State.Radio;
        Log.WriteFile(Log.LogType.Capture, "DVBGraph:Listening to radio..");
        return;
      }

      // tune to the correct channel

      TuneRadioChannel(station);
      Log.WriteFile(Log.LogType.Capture, "DVBGraph:Listening to radio..");
#endif
    }

    public void TuneRadioFrequency(int frequency)
    {
    }
    #endregion


    #region demuxer callbacks
    protected bool m_streamDemuxer_AudioHasChanged(MediaPortal.TV.Recording.DVBDemuxer.AudioHeader audioFormat)
    {
      return false;
    }
    protected bool m_streamDemuxer_OnAudioFormatChanged(MediaPortal.TV.Recording.DVBDemuxer.AudioHeader audioFormat)
    {/*
			Log.Write("DVBGraph:Audio format changed");
			Log.Write("DVBGraph:  Bitrate:{0}",audioFormat.Bitrate);
			Log.Write("DVBGraph:  Layer:{0}",audioFormat.Layer);
			Log.Write("DVBGraph:  SamplingFreq:{0}",audioFormat.SamplingFreq);
			Log.Write("DVBGraph:  Channel:{0}",audioFormat.Channel);
			Log.Write("DVBGraph:  Bound:{0}",audioFormat.Bound);
			Log.Write("DVBGraph:  Copyright:{0}",audioFormat.Copyright);
			Log.Write("DVBGraph:  Emphasis:{0}",audioFormat.Emphasis);
			Log.Write("DVBGraph:  ID:{0}",audioFormat.ID);
			Log.Write("DVBGraph:  Mode:{0}",audioFormat.Mode);
			Log.Write("DVBGraph:  ModeExtension:{0}",audioFormat.ModeExtension);
			Log.Write("DVBGraph:  Original:{0}",audioFormat.Original);
			Log.Write("DVBGraph:  PaddingBit:{0}",audioFormat.PaddingBit);
			Log.Write("DVBGraph:  PrivateBit:{0}",audioFormat.PrivateBit);
			Log.Write("DVBGraph:  ProtectionBit:{0}",audioFormat.ProtectionBit);
			Log.Write("DVBGraph:  TimeLength:{0}",audioFormat.TimeLength);*/
      return true;
    }

    protected void m_streamDemuxer_OnPMTIsChanged(byte[] pmtTable)
    {
      if (_graphState == State.None || _graphState == State.Created) return;
      if (pmtTable == null) return;
      if (pmtTable.Length < 6) return;
      if (_currentTuningObject.NetworkID < 0 ||
        _currentTuningObject.TransportStreamID < 0 ||
        _currentTuningObject.ProgramNumber < 0) return;
      try
      {
        string pmtName = String.Format(@"database\pmt\pmt_{0}_{1}_{2}_{3}_{4}.dat",
          Utils.FilterFileName(_currentTuningObject.ServiceName),
          _currentTuningObject.NetworkID,
          _currentTuningObject.TransportStreamID,
          _currentTuningObject.ProgramNumber,
          (int)Network());
#if COMPARE_PMT
        bool isSame = false;
        if (System.IO.File.Exists(pmtName))
        {
          byte[] pmt = null;
          using (System.IO.FileStream stream = new System.IO.FileStream(pmtName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None))
          {
            long len = stream.Length;
            if (len > 6)
            {
              pmt = new byte[len];
              stream.Read(pmt, 0, (int)len);
              stream.Close();
              if (pmt.Length == pmtTable.Length)
              {
                isSame = true;
                for (int i = 0; i < pmt.Length; ++i)
                {
                  if (pmt[i] != pmtTable[i])
                  {
                    isSame = false;
                  }
                }
              }
            }
          }
        }

#endif
        if (!isSame || _pmtSendCounter == 0)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraph: OnPMTIsChanged:{0}", pmtName);
          using (System.IO.FileStream stream = new System.IO.FileStream(pmtName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
          {
            stream.Write(pmtTable, 0, pmtTable.Length);
            stream.Close();
          }
          _refreshPmtTable = true;
          SendPMT();
        }
        if (Recorder.IsCardViewing(_cardId) || _graphState == State.Epg || _graphState == State.Radio)
        {
          Log.WriteFile(Log.LogType.Capture, "DVBGraph: grab epg for {0}", _currentTuningObject.ServiceName);
          _epgGrabber.GrabEPG(_currentTuningObject.ServiceName, _currentTuningObject.HasEITSchedule == true);
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
    }

    #endregion


    protected void AddHardwarePid(int pid, ArrayList pids)
    {
      if (pid <= 0) return;
      foreach (ushort existingPid in pids)
      {
        if (existingPid == (ushort)pid) return;
      }
      pids.Add((ushort)pid);
    }

    protected void SetHardwarePidFiltering()
    {
      string pidsText = String.Empty;
      ArrayList pids = new ArrayList();
      if (_inScanningMode == false)
      {
        if (Network() == NetworkType.ATSC)
        {
          pids.Add((ushort)0x1ffb);
        }
        else
        {
          pids.Add((ushort)0);
          pids.Add((ushort)1);
          pids.Add((ushort)0x10);
          pids.Add((ushort)0x11);
          pids.Add((ushort)0x12);
          pids.Add((ushort)0xd3);
          pids.Add((ushort)0xd2);
        }
        AddHardwarePid(_currentTuningObject.VideoPid, pids);
        AddHardwarePid(_currentTuningObject.AudioPid, pids);
        AddHardwarePid(_currentTuningObject.Audio1, pids);
        AddHardwarePid(_currentTuningObject.Audio2, pids);
        AddHardwarePid(_currentTuningObject.Audio3, pids);
        AddHardwarePid(_currentTuningObject.AC3Pid, pids);
        AddHardwarePid(_currentTuningObject.PMTPid, pids);
        AddHardwarePid(_currentTuningObject.TeletextPid, pids);
        AddHardwarePid(_currentTuningObject.PCRPid, pids);
        AddHardwarePid(_currentTuningObject.ECMPid, pids);
        for (int i = 0; i < pids.Count; ++i)
        {
          ushort pid = (ushort)pids[i];
          pidsText += String.Format("{0:X},", pid);
          switch ((int)pid)
          {
            case 0x12:
              break;
            case 0xd2:
              break;
            case 0xd3:
              break;
            default:
              if (_pinDemuxerSections != null)
                SetupDemuxerPin(_pinDemuxerSections, (int)pid, (int)MediaSampleContent.Mpeg2PSI, (i == 0));
              break;
          }

          //Log.Write("nr:{0} pid:0x{1:X}, hr:{2:X}", i, (int)pid, hr);
        }
        Log.WriteFile(Log.LogType.Capture, "DVBGraph:SetHardwarePidFiltering to:{0}", pidsText);
      }
      SendHWPids(pids);
      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
    }
    protected virtual void SendHWPids(ArrayList pids)
    {
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
      if (_streamDemuxer == null) return;
      _streamDemuxer.GrabTeletext(yesNo);
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
      Log.WriteFile(Log.LogType.Capture, "DVBGraph:Grab epg for :{0}", channel.Name);


      //now start the graph
      Log.WriteFile(Log.LogType.Capture, "DVBGraph: start graph");
      if (_mediaControl == null)
      {
        _mediaControl = (IMediaControl)_graphBuilder;
      }
      int hr = _mediaControl.Run();
      if (hr < 0)
      {
        Log.WriteFile(Log.LogType.Capture, true, "DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
      }
      TuneChannel(channel);
      if (_currentTuningObject == null)
      {
        _mediaControl.Stop();
        _isGraphRunning = false;
        _graphState = State.Created;
      }
      else
      {
        _isGraphRunning = true;
        _graphState = State.Epg;
      }
    }

    public void RadioChannelMinMax(out int chanmin, out int chanmax)
    {
      chanmin = -1;
      chanmax = -1;
    }

    public void TVChannelMinMax(out int chanmin, out int chanmax)
    {
      chanmin = -1;
      chanmax = -1;
    }
    public int FilterPids(short count, IntPtr pids)
    {
      // if (_inScanningMode == false) return 0;
      lock (this)
      {
        //if (_scanPidListReady) return 0;
        Log.Write("FilterPids:{0}", count);
        string pidsText = String.Empty;
        _scanPidList = new ArrayList();
        for (int i = 0; i < count; ++i)
        {
          ushort pid = (ushort)Marshal.ReadInt32(pids, i * 4);
          _scanPidList.Add(pid);
          pidsText += String.Format("{0:X},", pid);
        }

        //Log.WriteFile(Log.LogType.Capture, "DVBGraph:analyzer pids to:{0}", pidsText);
        _scanPidListReady = true;
      }
      return 0;
    }
    public void StopRadio()
    {
      if (_graphState != State.Radio) return;
      if (_mediaControl != null)
      {
        _mediaControl.Stop();
      }
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinAC3Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinMPG1Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerAudio);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideo);
      _isGraphRunning = false;
      _graphState = State.Created;
    }

    public void StopEpgGrabbing()
    {
      if (_graphState != State.Epg) return;
      if (_mediaControl != null)
        _mediaControl.Stop();
      _isGraphRunning = false;
      _graphState = State.Created;
    }

    public virtual bool SupportsHardwarePidFiltering()
    {
      if (_cardProperties == null) return false;
      return _cardProperties.SupportsHardwarePidFiltering;
    }
    public bool SupportsCamSelection()
    {
      if (_cardProperties == null) return false;
      return _cardProperties.SupportsCamSelection;
    }
  }//public class DVBGraphBDA 


}//namespace MediaPortal.TV.Recording
//end of file
#endif