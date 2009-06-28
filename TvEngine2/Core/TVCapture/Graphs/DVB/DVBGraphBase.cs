#region Copyright (C) 2005-2009 Team MediaPortal

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

#endregion

#define HW_PID_FILTERING
//#define DUMP
#define COMPARE_PMT
#if (UseCaptureCardDefinitions)

#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DirectShowLib;
using DirectShowLib.SBE;
using DShowNET.Helper;
using DShowNET.MPSA;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Radio.Database;
using MediaPortal.Services;
using MediaPortal.TV.Database;
using MediaPortal.TV.Epg;
using Microsoft.Win32;
using TunerLib;
using IBaseFilter=DirectShowLib.IBaseFilter;
using IEnumMoniker=System.Runtime.InteropServices.ComTypes.IEnumMoniker;
using IEnumPins=DirectShowLib.IEnumPins;
using IGraphBuilder=DirectShowLib.IGraphBuilder;
using IMoniker=System.Runtime.InteropServices.ComTypes.IMoniker;
using IPin=DirectShowLib.IPin;

#endregion

#pragma warning disable 618

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
  public class DVBGraphBase : IGraph, IHardwarePidFiltering
  {
    #region guids

    public static Guid MEDIATYPE_MPEG2_SECTIONS = new Guid(0x455f176c, 0x4b06, 0x47ce, 0x9a, 0xef, 0x8c, 0xae, 0xf7,
                                                           0x3d, 0xf7, 0xb5);

    public static Guid MEDIASUBTYPE_MPEG2_DATA = new Guid(0xc892e55b, 0x252d, 0x42b5, 0xa3, 0x16, 0xd9, 0x97, 0xe7, 0xa5,
                                                          0xd9, 0x95);

    public static Guid MEDIASUBTYPE_DVB_SI = new Guid(0xe9dd31a3, 0x221d, 0x4adb, 0x85, 0x32, 0x9a, 0xf3, 0x9, 0xc1,
                                                      0xa4, 0x8);

    public static Guid MEDIASUBTYPE_ATSC_SI = new Guid(0xb3c7397c, 0xd303, 0x414d, 0xb3, 0x3c, 0x4e, 0xd2, 0xc9, 0xd2,
                                                       0x97, 0x33);

    #endregion

    #region demuxer pin media types

    protected static byte[] Mpeg2ProgramVideo =
      {
        0x00, 0x00, 0x00, 0x00, //00  .hdr.rcSource.left              = 0x00000000
        0x00, 0x00, 0x00, 0x00, //04  .hdr.rcSource.top               = 0x00000000
        0xD0, 0x02, 0x00, 0x00, //08  .hdr.rcSource.right             = 0x000002d0 //720
        0x40, 0x02, 0x00, 0x00, //0c  .hdr.rcSource.bottom            = 0x00000240 //576
        0x00, 0x00, 0x00, 0x00, //10  .hdr.rcTarget.left              = 0x00000000
        0x00, 0x00, 0x00, 0x00, //14  .hdr.rcTarget.top               = 0x00000000
        0xD0, 0x02, 0x00, 0x00, //18  .hdr.rcTarget.right             = 0x000002d0 //720
        0x40, 0x02, 0x00, 0x00, //1c  .hdr.rcTarget.bottom            = 0x00000240// 576
        0x00, 0x09, 0x3D, 0x00, //20  .hdr.dwBitRate                  = 0x003d0900
        0x00, 0x00, 0x00, 0x00, //24  .hdr.dwBitErrorRate             = 0x00000000
        //0x051736=333667-> 10000000/333667 = 29.97fps
        //0x061A80=400000-> 10000000/400000 = 25fps
        0x80, 0x1A, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00,
        //28  .hdr.AvgTimePerFrame            = 0x0000000000051763 ->1000000/ 40000 = 25fps
        0x00, 0x00, 0x00, 0x00, //2c  .hdr.dwInterlaceFlags           = 0x00000000
        0x00, 0x00, 0x00, 0x00, //30  .hdr.dwCopyProtectFlags         = 0x00000000
        0x04, 0x00, 0x00, 0x00, //34  .hdr.dwPictAspectRatioX         = 0x00000004
        0x03, 0x00, 0x00, 0x00, //38  .hdr.dwPictAspectRatioY         = 0x00000003
        0x00, 0x00, 0x00, 0x00, //3c  .hdr.dwReserved1                = 0x00000000
        0x00, 0x00, 0x00, 0x00, //40  .hdr.dwReserved2                = 0x00000000
        0x28, 0x00, 0x00, 0x00, //44  .hdr.bmiHeader.biSize           = 0x00000028
        0xD0, 0x02, 0x00, 0x00, //48  .hdr.bmiHeader.biWidth          = 0x000002d0 //720
        0x40, 0x02, 0x00, 0x00, //4c  .hdr.bmiHeader.biHeight         = 0x00000240 //576
        0x00, 0x00, //50  .hdr.bmiHeader.biPlanes         = 0x0000
        0x00, 0x00, //54  .hdr.bmiHeader.biBitCount       = 0x0000
        0x00, 0x00, 0x00, 0x00, //58  .hdr.bmiHeader.biCompression    = 0x00000000
        0x00, 0x00, 0x00, 0x00, //5c  .hdr.bmiHeader.biSizeImage      = 0x00000000
        0xD0, 0x07, 0x00, 0x00, //60  .hdr.bmiHeader.biXPelsPerMeter  = 0x000007d0
        0x27, 0xCF, 0x00, 0x00, //64  .hdr.bmiHeader.biYPelsPerMeter  = 0x0000cf27
        0x00, 0x00, 0x00, 0x00, //68  .hdr.bmiHeader.biClrUsed        = 0x00000000
        0x00, 0x00, 0x00, 0x00, //6c  .hdr.bmiHeader.biClrImportant   = 0x00000000
        0x98, 0xF4, 0x06, 0x00, //70  .dwStartTimeCode                = 0x0006f498
        0x00, 0x00, 0x00, 0x00, //74  .cbSequenceHeader               = 0x00000056
        //0x00, 0x00, 0x00, 0x00,                         //74  .cbSequenceHeader               = 0x00000000
        0x02, 0x00, 0x00, 0x00, //78  .dwProfile                      = 0x00000002
        0x02, 0x00, 0x00, 0x00, //7c  .dwLevel                        = 0x00000002
        0x00, 0x00, 0x00, 0x00, //80  .Flags                          = 0x00000000
        //  .dwSequenceHeader [1]
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
      };

    protected static byte[] MPEG1AudioFormat =
      {
        0x50, 0x00, // format type      = 0x0050=WAVE_FORMAT_MPEG
        0x02, 0x00, // channels
        0x80, 0xBB, 0x00, 0x00, // samplerate       = 0x0000bb80=48000
        0x00, 0x7D, 0x00, 0x00, // nAvgBytesPerSec  = 0x00007d00=32000
        0x01, 0x00, // nBlockAlign      = 0x0300 = 768
        0x00, 0x00, // wBitsPerSample   = 16
        0x16, 0x00, // extra size       = 0x0016 = 22 bytes
        0x02, 0x00, // fwHeadLayer
        0x00, 0xE8, 0x03, 0x00, // dwHeadBitrate
        0x01, 0x00, // fwHeadMode
        0x01, 0x00, // fwHeadModeExt
        0x01, 0x00, // wHeadEmphasis
        0x16, 0x00, // fwHeadFlags
        0x00, 0x00, 0x00, 0x00, // dwPTSLow
        0x00, 0x00, 0x00, 0x00 // dwPTSHigh
      };

    #endregion

    #region imports

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    protected static extern unsafe bool DvrMsCreate(out int id, IBaseFilter streamBufferSink,
                                                    [In, MarshalAs(UnmanagedType.LPWStr)] string strPath,
                                                    uint dwRecordingType);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    protected static extern unsafe void DvrMsStart(int id, uint startTime);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    protected static extern unsafe void DvrMsStop(int id);

    [DllImport("dshowhelper.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    protected static extern unsafe bool AddTeeSinkToGraph(IGraphBuilder graph);

    [ComImport, Guid("6CFAD761-735D-4aa5-8AFC-AF91A7D61EBA")]
    protected class VideoAnalyzer
    {
    } ;

    [ComImport, Guid("AFB6C280-2C41-11D3-8A60-0000F81E0E4A")]
    protected class MPEG2Demultiplexer
    {
    }

    [ComImport, Guid("2DB47AE5-CF39-43c2-B4D6-0CD8D90946F4")]
    protected class StreamBufferSink
    {
    } ;

    [ComImport, Guid("FA8A68B2-C864-4ba2-AD53-D3876A87494B")]
    protected class StreamBufferConfig
    {
    }

    [DllImport("advapi32", CharSet = CharSet.Auto)]
    protected static extern bool ConvertStringSidToSid(string pStringSid, ref IntPtr pSID);

    [DllImport("kernel32", CharSet = CharSet.Auto)]
    protected static extern IntPtr LocalFree(IntPtr hMem);

    [DllImport("advapi32", CharSet = CharSet.Auto)]
    protected static extern ulong RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint sam, out IntPtr resultKey);

    [DllImport("dvblib.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    protected static extern bool GetPidMap(IPin filter, ref uint pid, ref uint mediasampletype);

    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxer(IPin pin, int pid, IPin pin1, int pid1, IPin pin2, int pid2);

    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxerPin(IPin pin, int pid, int elementaryStream, bool unmapOtherPins);

    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int DumpMpeg2DemuxerMappings(IBaseFilter filter);

    [ComImport, Guid("5CDD5C68-80DC-43E1-9E44-C849CA8026E7")]
    protected class TsFileSink
    {
    } ;

    [ComImport, Guid("BC650178-0DE4-47DF-AF50-BBD9C7AEF5A9")]
    protected class MpgMux
    {
    } ;

    #endregion

    #region enums

    private enum MediaSampleContent : int
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
    } ;

    #endregion

    #region consts

    protected static Guid CLSID_StreamBufferSink = new Guid(0x2db47ae5, 0xcf39, 0x43c2, 0xb4, 0xd6, 0xc, 0xd8, 0xd9, 0x9,
                                                            0x46, 0xf4);

    protected static Guid CLSID_Mpeg2VideoStreamAnalyzer = new Guid(0x6cfad761, 0x735d, 0x4aa5, 0x8a, 0xfc, 0xaf, 0x91,
                                                                    0xa7, 0xd6, 0x1e, 0xba);

    protected static Guid CLSID_StreamBufferConfig = new Guid(0xfa8a68b2, 0xc864, 0x4ba2, 0xad, 0x53, 0xd3, 0x87, 0x6a,
                                                              0x87, 0x49, 0x4b);

    private const int Mpeg2VideoServiceType = 1;
    private const int Mpeg2AudioServiceType = 2;
    private const int Mpeg4VideoServiceType = 1000;

    #endregion

    #region variables

    protected int _lastPMTVersion = -1;
    protected int _cardId = -1;
    protected int _currentChannelNumber = 28;
    protected DsROTEntry _rotEntry = null; // Cookie into the Running Object Table
    protected State _graphState = State.None;
    protected DateTime _startTimer = DateTime.Now;
    protected IPin _pinAC3Out = null;
    protected IPin _pinMPG1Out = null;
    protected IPin _pinDemuxerVideo = null;
    protected IPin _pinDemuxerVideoMPEG4 = null;
    protected IPin _pinDemuxerTS = null;
    protected IPin _pinDemuxerAudio = null;
    protected IPin _pinDemuxerSections = null;
    protected IPin _pinDemuxerEPG = null;
    protected IPin _pinDemuxerMHWd2 = null;
    protected IPin _pinDemuxerMHWd3 = null;
    protected IStreamBufferSink3 m_IStreamBufferSink = null;
    protected IStreamBufferConfigure m_IStreamBufferConfig = null;
    protected IBaseFilter _filterTIF = null; // Transport Information Filter
    protected IBaseFilter _filterNetworkProvider = null; // BDA Network Provider
    protected IBaseFilter _filterTunerDevice = null; // BDA Digital Tuner Device
    protected IBaseFilter _filterCaptureDevice = null; // BDA Digital Capture Device

    protected IBaseFilter _filterMpeg2Demultiplexer = null;
                          // Mpeg2 Demultiplexer that connects to Preview pin on Smart Tee (must connect before capture)

    protected IBaseFilter _filterTsMpeg2Demultiplexer = null;
    protected IStreamAnalyzer _analyzerInterface = null;
    protected IEPGGrabber _epgGrabberInterface = null;
    protected IMHWGrabber _mhwGrabberInterface = null;
    protected IATSCGrabber _atscGrabberInterface = null;
    protected IBaseFilter _filterDvbAnalyzer = null;
    protected TsFileSink _filterTsFileSink = null;
    protected bool _graphPaused = false;
    protected int _pmtSendCounter = 0;
    private ArrayList _scanPidList = new ArrayList();
    protected bool _scanPidListReady = false;
    protected VideoCaptureProperties _cardProperties = null;
    protected IBaseFilter _filterSmartTee = null;
    protected VideoAnalyzer m_mpeg2Analyzer = null;
    protected IGraphBuilder _graphBuilder = null;
    protected ICaptureGraphBuilder2 _captureGraphBuilderInterface = null;
    protected IVideoWindow _videoWindowInterface = null;
    protected IBasicVideo2 _basicVideoInterFace = null;
    protected IMediaControl _mediaControl = null;
    protected IBaseFilter _filterSampleGrabber = null;
    protected ISampleGrabber _sampleInterface = null;
    protected IBaseFilter _filterInfTee = null;
    protected IBaseFilter[] customFilters;
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
    protected bool _notifySignalLost = false;
    protected int _pmtRetyCount = 0;
    protected int _signalQuality;
    protected int _signalLevel;
    protected bool _signalPresent;
    protected bool _tunerLocked;
    protected bool _inScanningMode = false;
    protected DateTime _pmtTimer;
    protected DateTime _processTimer = DateTime.MinValue;
    protected IPin _pinTeletext;
    protected string _currentTimeShiftFileName;
    protected string _lastError = string.Empty;

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
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.Database));
      }
      catch (Exception)
      {
      }
      try
      {
        Directory.CreateDirectory(Config.GetSubFolder(Config.Dir.Database, "pmt"));
      }
      catch (Exception)
      {
      }
      //create registry keys needed by the streambuffer engine for timeshifting/recording
      try
      {
        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(@"Software\MediaPortal"))
        {
        }
        using (RegistryKey newKey = Registry.LocalMachine.CreateSubKey(@"Software\MediaPortal"))
        {
        }
      }
      catch (Exception)
      {
      }
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
    } //public bool CreateGraph()

    /// <summary>
    /// Deletes the current DirectShow graph created with CreateGraph()
    /// Frees any (unmanaged) resources
    /// </summary>
    /// <remarks>
    /// Graph must be created first with CreateGraph()
    /// </remarks>
    public virtual void DeleteGraph()
    {
    } //public void DeleteGraph()

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
    public bool StartRecording(Hashtable attributes, TVRecording recording, TVChannel channel, ref string strFileName,
                               bool bContentRecording, DateTime timeProgStart)
    {
      if (_graphState != State.TimeShifting)
      {
        return false;
      }
      if (m_StreamBufferSink == null)
      {
        return false;
      }
      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }
      Log.Info("DVBGraph:StartRecording()");
      uint iRecordingType = 0;
      if (bContentRecording)
      {
        iRecordingType = 0;
      }
      else
      {
        iRecordingType = 1;
      }
      try
      {
        bool success = DvrMsCreate(out m_recorderId, (IBaseFilter) m_IStreamBufferSink, strFileName, iRecordingType);
        if (!success)
        {
          Log.Error("DVBGraph:StartRecording() FAILED to create recording");
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
          lStartTime *= (long) uiMaxFiles;
          // if start of program is given, then use that as our starttime
          if (timeProgStart.Year > 2000)
          {
            TimeSpan ts = DateTime.Now - timeProgStart;
            Log.Info("DVBGraph: Start recording from {0}:{1:00}:{2:00} which is {3:00}:{4:00}:{5:00} in the past",
                     timeProgStart.Hour, timeProgStart.Minute, timeProgStart.Second,
                     ts.TotalHours, ts.TotalMinutes, ts.TotalSeconds);
            lStartTime = (long) ts.TotalSeconds;
          }
          else
          {
            Log.Info("DVBGraph: record entire timeshift buffer");
          }
          TimeSpan tsMaxTimeBack = DateTime.Now - _startTimer;
          if (lStartTime > tsMaxTimeBack.TotalSeconds)
          {
            lStartTime = (long) tsMaxTimeBack.TotalSeconds;
          }
        } //if (!bContentRecording)
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
        DvrMsStart(m_recorderId, (uint) lStartTime);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
      }
      _graphState = State.Recording;
      return true;
    }

    //public bool StartRecording(int country,AnalogVideoStandard standard,int iChannelNr, ref string strFileName, bool bContentRecording, DateTime timeProgStart)

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
      {
        return;
      }
      Log.Info("DVBGraph:stop recording...");
      if (m_recorderId >= 0)
      {
        //Log.Info("DVBGraph:stop recorder:{0}...", m_recorderId);
        DvrMsStop(m_recorderId);
        m_recorderId = -1;
      }
      _graphState = State.TimeShifting;
      //Log.Info("DVBGraph:stopped recording...");
    } //public void StopRecording()

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
      if (_graphState != State.Created)
      {
        return false;
      }
      Log.Info("DVBGraph:StartViewing() {0}", channel.Name);
      _isOverlayVisible = true;
      // add VMR9 renderer to graph
      if (_vmr9 == null)
      {
        _vmr9 = new VMR9Util();
      }
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
        _lastError = String.Format("No tuning information for {0}", channel.Name);
        return false;
      }
      if (_currentTuningObject.ServiceType == Mpeg4VideoServiceType)
      {
        // add the preferred video/audio codecs
        //AddPreferredMPEG4Codecs(true, true); Not required now us standard AddPreferredCodecs.
        AddPreferredCodecs(true, true);
        SetupDemuxerPin(_pinDemuxerVideoMPEG4, _currentTuningObject.VideoPid, (int) MediaSampleContent.ElementaryStream,
                        true);
        // render the video/audio pins of the mpeg2 demultiplexer so they get connected to the video/audio codecs
        if (_graphBuilder.Render(_pinDemuxerVideoMPEG4) != 0)
        {
          _lastError = String.Format("Unable to connect MPG4 pin");
          Log.Error("DVBGraph:Failed to render MPEG4 video out pin MPEG-2 Demultiplexer");
          return false;
        }
      }
      else
      {
        // add the preferred video/audio codecs
        AddPreferredCodecs(true, true);
        // render the video/audio pins of the mpeg2 demultiplexer so they get connected to the video/audio codecs
        if (_graphBuilder.Render(_pinDemuxerVideo) != 0)
        {
          _lastError = String.Format("Unable to connect MPG2 pin");
          Log.Error("DVBGraph:Failed to render video out pin MPEG-2 Demultiplexer");
          return false;
        }
      }
      int serviceType;
      _isUsingAC3 = TVDatabase.DoesChannelHaveAC3(channel, Network() == NetworkType.DVBC, Network() == NetworkType.DVBT,
                                                  Network() == NetworkType.DVBS, Network() == NetworkType.ATSC,
                                                  out serviceType);
      if (_isUsingAC3)
      {
        Log.Info("DVBGraph: channel {0} uses AC3", channel.Name);
      }
      else
      {
        Log.Info("DVBGraph: channel {0} uses MP2 audio", channel.Name);
      }
      Log.Info("DVBGraph:StartViewing(). : ac3={0}", _isUsingAC3);
      if (!_isUsingAC3)
      {
        //Log.Info("DVBGraph:render MP2 audio pin");
        if (_graphBuilder.Render(_pinDemuxerAudio) != 0)
        {
          _lastError = String.Format("Unable to connect audio pin");
          Log.Error("DVBGraph:Failed to render audio out pin MPEG-2 Demultiplexer");
          return false;
        }
      }
      else
      {
        //Log.Info("DVBGraph:render AC3 audio pin");
        if (_graphBuilder.Render(_pinAC3Out) != 0)
        {
          _lastError = String.Format("Unable to connect AC3 pin");
          Log.Error("DVBGraph:Failed to render AC3 pin MPEG-2 Demultiplexer");
          return false;
        }
      }
      //get the IMediaControl interface of the graph
      if (_mediaControl == null)
      {
        _mediaControl = (IMediaControl) _graphBuilder;
      }
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
          _lastError = String.Format("IVideoWindow not present");
          Log.Error("DVBGraph:FAILED:Unable to get IVideoWindow");
          return false;
        }
        _basicVideoInterFace = _graphBuilder as IBasicVideo2;
        if (_basicVideoInterFace == null)
        {
          _lastError = String.Format("IBasicVideo2 not present");
          Log.Error("DVBGraph:FAILED:Unable to get IBasicVideo2");
          return false;
        }
        // and set it up
        hr = _videoWindowInterface.put_Owner(GUIGraphicsContext.ActiveForm);
        if (hr != 0)
        {
          Log.Error("DVBGraph: FAILED:set Video window:0x{0:X}", hr);
        }
        hr =
          _videoWindowInterface.put_WindowStyle(
            (WindowStyle) ((int) WindowStyle.ClipSiblings + (int) WindowStyle.Child + (int) WindowStyle.ClipChildren));
        if (hr != 0)
        {
          Log.Error("DVBGraph: FAILED:set Video window style:0x{0:X}", hr);
        }
        //show overlay window
        hr = _videoWindowInterface.put_Visible(OABool.True);
        if (hr != 0)
        {
          Log.Error("DVBGraph: FAILED:put_Visible:0x{0:X}", hr);
        }
      }
      //start the graph
      //Log.WriteFile(LogType.Log,"DVBGraph: start graph");
      hr = _mediaControl.Run();
      if (hr < 0)
      {
        _lastError = String.Format("Unable to start graph");
        Log.Error("DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
        return false;
      }
      _isGraphRunning = true;
      using (Settings xmlreader = new MPSettings())
      {
        string strValue = xmlreader.GetValueAsString("mytve2", "defaultar", "Normal");
        GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(strValue);
      }
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      _graphState = State.Viewing;
      GUIGraphicsContext_OnVideoWindowChanged();
      // tune to the correct channel
      if (channel.Number >= 0)
      {
        TuneChannel(channel);
      }
      //Log.Info("DVBGraph:Viewing..");
      return true;
    } //public bool StartViewing(AnalogVideoStandard standard, int iChannel,int country)

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
      {
        return false;
      }
      GUIGraphicsContext.OnVideoWindowChanged -= new VideoWindowChangedHandler(GUIGraphicsContext_OnVideoWindowChanged);
      Log.Info("DVBGraph: StopViewing()");
      if (_videoWindowInterface != null)
      {
        _videoWindowInterface.put_Visible(OABool.False);
      }
      //Log.Info("DVBGraph: stop vmr9");
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
      if (_pinDemuxerTS != null)
      {
        DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerTS);
      }
      if (_pinDemuxerVideoMPEG4 != null)
      {
        DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideoMPEG4);
      }
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinAC3Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinMPG1Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerAudio);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideo);
      //Log.Info("DVBGraph: view stopped");
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
      {
        return false;
      }
      ulong freeSpace = Util.Utils.GetFreeDiskSpace(strFileName);
      if (freeSpace < (1024L*1024L*1024L)) // 1 GB
      {
        _lastError = GUILocalizeStrings.Get(765); // "Not enough free diskspace";
        Log.WriteFile(LogType.Recorder, true,
                      "Recorder:  failed to start timeshifting since drive {0}: has less then 1GB freediskspace",
                      strFileName[0]);
        return false;
      }
      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }
      Log.Info("DVBGraph:StartTimeShifting() {0}", channel.Name);
      GetTvChannelFromDatabase(channel);
      if (_currentTuningObject == null)
      {
        _lastError = String.Format("Tuning details for {0} not found", channel.Name);
        return false;
      }
      _isUsingAC3 = false;
      if (channel != null)
      {
        int serviceType;
        ;
        _isUsingAC3 = TVDatabase.DoesChannelHaveAC3(channel, Network() == NetworkType.DVBC,
                                                    Network() == NetworkType.DVBT, Network() == NetworkType.DVBS,
                                                    Network() == NetworkType.ATSC, out serviceType);
        if (_isUsingAC3)
        {
          Log.Info("DVBGraph: channel {0} uses AC3", channel.Name);
        }
        else
        {
          Log.Info("DVBGraph: channel {0} uses MP2 audio", channel.Name);
        }
      }
      if (_currentTuningObject.ServiceType == Mpeg4VideoServiceType)
      {
        SetupDemuxerPin(_pinDemuxerVideoMPEG4, _currentTuningObject.VideoPid, (int) MediaSampleContent.ElementaryStream,
                        true);
      }
      Log.Info("DVBGraph:(). StartTimeShifting: ac3={0}", _isUsingAC3);
      bool success = false;
      success = CreateSinkSource(strFileName, _isUsingAC3);
      if (success)
      {
        if (_mediaControl == null)
        {
          _mediaControl = (IMediaControl) _graphBuilder;
        }
        //now start the graph
        //Log.Info("DVBGraph: start graph");
        int hr = _mediaControl.Run();
        if (hr < 0)
        {
          _lastError = String.Format("Unable to run graph");
          Log.Error("DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
          return false;
        }
        TuneChannel(channel);
        _isGraphRunning = true;
        _graphState = State.TimeShifting;
      }
      else
      {
        Log.Error("DVBGraph:Unable to create sinksource()");
        return false;
      }
      //Log.Info("DVBGraph:timeshifting started");
      return true;
    } //public bool StartTimeShifting(int country,AnalogVideoStandard standard, int iChannel, string strFileName)

    /// <summary>
    /// Stops timeshifting and cleans up the timeshifting files
    /// </summary>
    /// <returns>boolean indicating if timeshifting is stopped or not</returns>
    /// <remarks>
    /// Graph should be timeshifting
    /// </remarks>
    public bool StopTimeShifting()
    {
      if (_graphState != State.TimeShifting)
      {
        return false;
      }
      Log.Info("DVBGraph: StopTimeShifting()");
      if (_mediaControl != null)
      {
        _mediaControl.Stop();
        _isGraphRunning = false;
      }
      if (_vmr9 != null)
      {
        _vmr9.Dispose();
        _vmr9 = null;
      }
      if (_pinDemuxerTS != null)
      {
        DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerTS);
      }
      if (_pinDemuxerVideoMPEG4 != null)
      {
        DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideoMPEG4);
      }
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinAC3Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinMPG1Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerAudio);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideo);
      _isGraphRunning = false;
      _graphState = State.Created;
      return true;
    } //public bool StopTimeShifting()

    #endregion

    #endregion

    #region overlay

    public bool Overlay
    {
      get { return _isOverlayVisible; }
      set
      {
        if (value == _isOverlayVisible)
        {
          return;
        }
        _isOverlayVisible = value;
        if (!_isOverlayVisible)
        {
          if (_videoWindowInterface != null)
          {
            _videoWindowInterface.put_Visible(OABool.False);
          }
        }
        else
        {
          if (_videoWindowInterface != null)
          {
            _videoWindowInterface.put_Visible(OABool.True);
          }
        }
      }
    }

    #endregion

    #region overrides

    /// <summary>
    /// Callback from GUIGraphicsContext. Will get called when the video window position or width/height changes
    /// </summary>
    protected void GUIGraphicsContext_OnVideoWindowChanged()
    {
      if (GUIGraphicsContext.Vmr9Active)
      {
        return;
      }
      if (_graphState != State.Viewing)
      {
        return;
      }
      if (_basicVideoInterFace == null)
      {
        return;
      }
      if (_videoWindowInterface == null)
      {
        return;
      }
      Log.Info("DVBGraph:OnVideoWindowChanged()");
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
        if (nw <= 0 || nh <= 0)
        {
          return;
        }


        Rectangle rSource, rDest;
        Geometry m_geometry = new Geometry();
        m_geometry.ImageWidth = iVideoWidth;
        m_geometry.ImageHeight = iVideoHeight;
        m_geometry.ScreenWidth = nw;
        m_geometry.ScreenHeight = nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(aspectX, aspectY, out rSource, out rDest);
        rDest.X += (int) x;
        rDest.Y += (int) y;

        if (rSource.Left < 0 || rSource.Top < 0 || rSource.Width <= 0 || rSource.Height <= 0)
        {
          return;
        }
        if (rDest.Left < 0 || rDest.Top < 0 || rDest.Width <= 0 || rDest.Height <= 0)
        {
          return;
        }

        Log.Info("overlay: video WxH  : {0}x{1}", iVideoWidth, iVideoHeight);
        Log.Info("overlay: video AR   : {0}:{1}", aspectX, aspectY);
        Log.Info("overlay: screen WxH : {0}x{1}", nw, nh);
        Log.Info("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
        Log.Info("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
        Log.Info("overlay: src        : ({0},{1})-({2},{3})",
                 rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
        Log.Info("overlay: dst        : ({0},{1})-({2},{3})",
                 rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);


        _basicVideoInterFace.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
        _basicVideoInterFace.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
        _videoWindowInterface.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
      else
      {
        if (GUIGraphicsContext.VideoWindow.Left < 0 || GUIGraphicsContext.VideoWindow.Top < 0 ||
            GUIGraphicsContext.VideoWindow.Width <= 0 || GUIGraphicsContext.VideoWindow.Height <= 0)
        {
          return;
        }
        if (iVideoHeight <= 0 || iVideoWidth <= 0)
        {
          return;
        }

        _basicVideoInterFace.SetSourcePosition(0, 0, iVideoWidth, iVideoHeight);
        _basicVideoInterFace.SetDestinationPosition(0, 0, GUIGraphicsContext.VideoWindow.Width,
                                                    GUIGraphicsContext.VideoWindow.Height);
        _videoWindowInterface.SetWindowPosition(GUIGraphicsContext.VideoWindow.Left, GUIGraphicsContext.VideoWindow.Top,
                                                GUIGraphicsContext.VideoWindow.Width,
                                                GUIGraphicsContext.VideoWindow.Height);
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
        Log.Info("DVBGraph: ShouldRebuildGraph({0})  false, not viewing", newChannel.Name);
        return false;
      }
      int serviceType;
      bool useAC3 = TVDatabase.DoesChannelHaveAC3(newChannel, Network() == NetworkType.DVBC,
                                                  Network() == NetworkType.DVBT, Network() == NetworkType.DVBS,
                                                  Network() == NetworkType.ATSC, out serviceType);
      Log.Info("DVBGraph: ShouldRebuildGraph({0})  current ac3:{1} new channel ac3:{2}",
               newChannel.Name, _isUsingAC3, useAC3);
      if (useAC3 != _isUsingAC3)
      {
        return true;
      }
      if (_currentTuningObject != null)
      {
        if (serviceType != _currentTuningObject.ServiceType)
        {
          return true;
        }
      }
      return false;
    }

    #region Stream-Audio handling

    public int GetAudioLanguage()
    {
      return _currentTuningObject.AudioPid;
    }

    public void SetAudioLanguage(int audioPid)
    {
      if (audioPid == _currentTuningObject.AudioPid)
      {
        return;
      }
      Log.Error("DVBGraph: change audio pid {0:X}-> pid:{1:X} {2}", _currentTuningObject.AudioPid, audioPid, _graphState);

      SetupDemuxerPin(_pinAC3Out, audioPid, (int) MediaSampleContent.ElementaryStream, true);
      SetupDemuxerPin(_pinDemuxerAudio, audioPid, (int) MediaSampleContent.ElementaryStream, true);

      if (audioPid == _currentTuningObject.AC3Pid)
      {
        Log.Error("DVBGraph: AC3 audio");
        //check if ac3 pin is connected
        IPin pin;
        _pinAC3Out.ConnectedTo(out pin);
        if (pin == null)
        {
          //no? then connect ac3 pin
          if (_mediaControl != null)
          {
            Log.Error("DVBGraph: stop graph");
            _mediaControl.Stop();
            Log.Error("DVBGraph: disconnect MP2 pin");
            _pinDemuxerAudio.Disconnect();
            Log.Error("DVBGraph: connect AC3 pin");
            _graphBuilder.Render(_pinAC3Out);
            Log.Error("DVBGraph: start graph");
            _mediaControl.Run();
          }
        }
        else
        {
          DirectShowUtil.ReleaseComObject(pin);
        }
      }
      else
      {
        Log.Error("DVBGraph: MP2 audio");
        //check if mpeg2 audio pin is connected
        IPin pin;
        _pinDemuxerAudio.ConnectedTo(out pin);
        if (pin == null)
        {
          //no? then connect mpeg2 audio pin
          if (_mediaControl != null)
          {
            Log.Error("DVBGraph: stop graph");
            _mediaControl.Stop();
            Log.Error("DVBGraph: disconnect AC3 pin");
            _pinAC3Out.Disconnect();
            Log.Error("DVBGraph: connect MP2 pin");
            _graphBuilder.Render(_pinDemuxerAudio);
            Log.Error("DVBGraph: start graph");
            _mediaControl.Run();
          }
        }
        else
        {
          DirectShowUtil.ReleaseComObject(pin);
        }
      }
      _currentTuningObject.AudioPid = audioPid;
      if (_cardProperties.IsCISupported())
      {
        Log.Error("DVBGraph: resending PMT to CAM");
        UpdateCAM();
      }

      //      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
    }

    public ArrayList GetAudioLanguageList()
    {
      if (_currentTuningObject == null)
      {
        return new ArrayList();
      }
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
        al = new DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.Audio1;
        al.AudioLanguageCode = _currentTuningObject.AudioLanguage1;
        audioPidList.Add(al);
      }
      if (_currentTuningObject.Audio2 > 0)
      {
        al = new DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.Audio2;
        al.AudioLanguageCode = _currentTuningObject.AudioLanguage2;
        audioPidList.Add(al);
      }
      if (_currentTuningObject.Audio3 > 0)
      {
        al = new DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.Audio3;
        al.AudioLanguageCode = _currentTuningObject.AudioLanguage3;
        audioPidList.Add(al);
      }
      if (_currentTuningObject.AC3Pid > 0)
      {
        al = new DVBSections.AudioLanguage();
        al.AudioPid = _currentTuningObject.AC3Pid;
        al.AudioLanguageCode = "AC-3";
        audioPidList.Add(al);
      }
      return audioPidList;
    }

    #endregion

    public bool HasTeletext()
    {
      if (_graphState != State.TimeShifting && _graphState != State.Recording && _graphState != State.Viewing)
      {
        return false;
      }
      if (_currentTuningObject == null)
      {
        return false;
      }
      if (_currentTuningObject.TeletextPid > 0)
      {
        return true;
      }
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
    } //public bool SignalPresent()

    public int SignalQuality()
    {
      if (_signalQuality < 0)
      {
        return 0;
      }
      if (_signalQuality > 100)
      {
        return 100;
      }
      return _signalQuality;
    }

    public int SignalStrength()
    {
      if (_signalLevel < 0)
      {
        return 0;
      }
      if (_signalLevel > 100)
      {
        return 100;
      }
      return _signalLevel;
    }

    /// <summary>
    /// not used
    /// </summary>
    /// <returns>-1</returns>
    public long VideoFrequency()
    {
      if (_currentTuningObject != null)
      {
        return _currentTuningObject.Frequency*1000;
      }
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
      string strH264VideoCodec = "";
      string strAudioCodec = "";
      string strAudioRenderer = "";
      int intFilters = 0;
      string strFilters = "";
      using (Settings xmlreader = new MPSettings())
      {
        int intCount = 0;
        while (xmlreader.GetValueAsString("mytve2", "filter" + intCount.ToString(), "undefined") != "undefined")
        {
          if (xmlreader.GetValueAsBool("mytve2", "usefilter" + intCount.ToString(), false))
          {
            strFilters += xmlreader.GetValueAsString("mytve2", "filter" + intCount.ToString(), "undefined") + ";";
            intFilters++;
          }
          intCount++;
        }
        strVideoCodec = xmlreader.GetValueAsString("mytve2", "videocodec", "");
        strH264VideoCodec = xmlreader.GetValueAsString("mytve2", "h264videocodec", "");
        strAudioCodec = xmlreader.GetValueAsString("mytve2", "audiocodec", "");
        strAudioRenderer = xmlreader.GetValueAsString("mytve2", "audiorenderer", "Default DirectSound Device");
      }
      if (video && strVideoCodec.Length > 0)
      {
        DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
      }
      if (video && strH264VideoCodec.Length > 0)
      {
        DirectShowUtil.AddFilterToGraph(_graphBuilder, strH264VideoCodec);
      }
      if (audio && strAudioCodec.Length > 0)
      {
        DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
      }
      if (audio && strAudioRenderer.Length > 0)
      {
        DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, false);
      }
      customFilters = new IBaseFilter[intFilters];
      string[] arrFilters = strFilters.Split(';');
      for (int i = 0; i < intFilters; i++)
      {
        customFilters[i] = DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
      }
    } //void AddPreferredCodecs()

    //Not necessary anymore.
    /*protected void AddPreferredMPEG4Codecs( bool audio, bool video )
    {
      // add preferred video & audio codecs
      string strVideoCodec = "";
      string strAudioCodec = "";
      string strAudioRenderer = "";
      int intFilters = 0;
      string strFilters = "";
      using ( MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
          int intCount = 0;
          while (xmlreader.GetValueAsString("mytve2", "filter" + intCount.ToString(), "undefined") != "undefined")
          {
              if (xmlreader.GetValueAsBool("mytve2", "usefilter" + intCount.ToString(), false))
              {
                  strFilters += xmlreader.GetValueAsString("mytve2", "filter" + intCount.ToString(), "undefined") + ";";
                  intFilters++;
              }
              intCount++;
          }
        strVideoCodec = xmlreader.GetValueAsString("mytve2", "videocodecMPEG4", "Elecard AVC/H.264 Decoder DMO");
        strAudioCodec = xmlreader.GetValueAsString("mytve2", "audiocodec", "");
        strAudioRenderer = xmlreader.GetValueAsString("mytve2", "audiorenderer", "Default DirectSound Device");
      }
      if ( video && strVideoCodec.Length > 0 )
        DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
      if ( audio && strAudioCodec.Length > 0 )
        DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
      if ( audio && strAudioRenderer.Length > 0 )
        DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudioRenderer, false);
      customFilters = new IBaseFilter[intFilters];
      string[] arrFilters = strFilters.Split(';');
      for (int i = 0; i < intFilters; i++)
      {
        customFilters[i] = DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
      }
    }//void AddPreferredCodecs()
    */

    protected virtual bool CreateSinkSource(string fileName, bool useAC3)
    {
      _currentTimeShiftFileName = fileName;
      int hr = 0;
      IPin pinObj0 = null;
      IPin pinObj1 = null;
      IPin pinObj2 = null;
      IPin pinObj3 = null;
      IPin outPin = null;
      try
      {
        int iTimeShiftBuffer = 30;
        using (Settings xmlreader = new MPSettings())
        {
          iTimeShiftBuffer = xmlreader.GetValueAsInt("capture", "timeshiftbuffer", 30);
          if (iTimeShiftBuffer < 5)
          {
            iTimeShiftBuffer = 5;
          }
        }
        iTimeShiftBuffer *= 60; //in seconds
        int iFileDuration = iTimeShiftBuffer/6;
        //create StreamBufferSink filter
        //				Log.WriteFile(LogType.Log,"DVBGraph:CreateSinkSource()");
        hr = _graphBuilder.AddFilter((IBaseFilter) m_StreamBufferSink, "StreamBufferSink");
        if (hr != 0)
        {
          _lastError = String.Format("Unable to add StreamBufferSink filter");
          Log.Error("DVBGraph:FAILED cannot add StreamBufferSink:{0:X}", hr);
          return false;
        }
        //create MPEG2 Analyzer filter
        //				Log.WriteFile(LogType.Log,"DVBGraph:Add mpeg2 analyzer()");
        hr = _graphBuilder.AddFilter((IBaseFilter) m_mpeg2Analyzer, "Mpeg2 Analyzer");
        if (hr != 0)
        {
          _lastError = String.Format("Unable to add mpeg2 analyzer filter");
          Log.Error("DVBGraph:FAILED cannot add mpeg2 analyzer to graph:{0:X}", hr);
          return false;
        }
        //connect mpeg2 demuxer video out->mpeg2 analyzer input pin
        //get input pin of MPEG2 Analyzer filter
        //				Log.WriteFile(LogType.Log,"DVBGraph:find mpeg2 analyzer input pin()");
        pinObj0 = DsFindPin.ByDirection((IBaseFilter) m_mpeg2Analyzer, PinDirection.Input, 0);
        if (pinObj0 == null)
        {
          _lastError = String.Format("failed connect mpeg2 demuxer to mpeg2 analyzer");
          Log.Error("DVBGraph:FAILED cannot find mpeg2 analyzer input pin");
          return false;
        }
        //				Log.WriteFile(LogType.Log,"DVBGraph:connect demux video output->mpeg2 analyzer");
        hr = _graphBuilder.Connect(_pinDemuxerVideo, pinObj0);
        if (hr != 0)
        {
          _lastError = String.Format("failed connect mpeg2 demuxer to mpeg2 analyzer");
          Log.Error("DVBGraph:FAILED to connect demux video output->mpeg2 analyzer:{0:X}", hr);
          return false;
        }
        //connect MPEG2 analyzer Filter->stream buffer sink pin 0
        //get output pin #0 from MPEG2 analyzer Filter
        //				Log.WriteFile(LogType.Log,"DVBGraph:mpeg2 analyzer output->streambuffersink in");
        pinObj1 = DsFindPin.ByDirection((IBaseFilter) m_mpeg2Analyzer, PinDirection.Output, 0);
        if (hr != 0)
        {
          _lastError = String.Format("failed connect mpeg2 demuxer to mpeg2 analyzer");
          Log.Error("DVBGraph:FAILED cannot find mpeg2 analyzer output pin:{0:X}", hr);
          return false;
        }
        //get input pin #0 from StreamBufferSink Filter
        pinObj2 = DsFindPin.ByDirection((IBaseFilter) m_StreamBufferSink, PinDirection.Input, 0);
        if (hr != 0)
        {
          _lastError = String.Format("failed connect mpeg2 demuxer to mpeg2 analyzer");
          Log.Error("DVBGraph:FAILED cannot find SBE input pin:{0:X}", hr);
          return false;
        }
        hr = _graphBuilder.Connect(pinObj1, pinObj2);
        if (hr != 0)
        {
          _lastError = String.Format("failed connect mpeg2 demuxer to mpeg2 analyzer");
          Log.Error("DVBGraph:FAILED to connect mpeg2 analyzer->streambuffer sink:{0:X}", hr);
          return false;
        }
        if (!useAC3)
        {
          //Log.Info("DVBGraph:connect MP2 audio pin->SBE");
          //connect MPEG2 demuxer audio output ->StreamBufferSink Input #1
          //Get StreamBufferSink InputPin #1
          pinObj3 = DsFindPin.ByDirection((IBaseFilter) m_StreamBufferSink, PinDirection.Input, 1);
          if (hr != 0)
          {
            _lastError = String.Format("failed connect stream buffer sink");
            Log.Error("DVBGraph:FAILED cannot find SBE input pin#2");
            return false;
          }
          hr = _graphBuilder.Connect(_pinDemuxerAudio, pinObj3);
          if (hr != 0)
          {
            _lastError = String.Format("failed connect stream buffer sink");
            Log.Error("DVBGraph:FAILED to connect mpeg2 demuxer audio out->streambuffer sink in#2:{0:X}", hr);
            return false;
          }
        }
        else
        {
          //connect ac3 pin ->stream buffersink input #2
          //Log.Info("DVBGraph:connect AC3 audio pin->SBE");
          if (_pinAC3Out != null)
          {
            pinObj3 = DsFindPin.ByDirection((IBaseFilter) m_StreamBufferSink, PinDirection.Input, 1);
            if (hr != 0)
            {
              _lastError = String.Format("failed connect stream buffer sink");
              Log.Error("DVBGraph:FAILED cannot find SBE input pin#2");
              return false;
            }
            hr = _graphBuilder.Connect(_pinAC3Out, pinObj3);
            if (hr != 0)
            {
              _lastError = String.Format("failed connect stream buffer sink");
              Log.Error("DVBGraph:FAILED to connect mpeg2 demuxer AC3 out->streambuffer sink in#2:{0:X}", hr);
              return false;
            }
          }
          else
          {
            _lastError = String.Format("failed AC3 audio pin not found");
            Log.Error("DVBGraph:FAILED ac3 pin not found?");
          }
        }
        int ipos = fileName.LastIndexOf(@"\");
        string strDir = fileName.Substring(0, ipos);
        m_StreamBufferConfig = new StreamBufferConfig();
        m_IStreamBufferConfig = (IStreamBufferConfigure) m_StreamBufferConfig;
        // setting the StreamBufferEngine registry key
        IntPtr HKEY = (IntPtr) unchecked((int) 0x80000002L);
        IStreamBufferInitialize pTemp = (IStreamBufferInitialize) m_IStreamBufferConfig;
        IntPtr subKey = IntPtr.Zero;
        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr = pTemp.SetHKEY(subKey);
        //set timeshifting folder
        //				Log.WriteFile(LogType.Log,"DVBGraph:set timeshift folder to:{0}", strDir);
        hr = m_IStreamBufferConfig.SetDirectory(strDir);
        if (hr != 0)
        {
          _lastError = String.Format("Timeshifting folder not present");
          Log.Error("DVBGraph:FAILED to set timeshift folder to:{0} {1:X}", strDir, hr);
          return false;
        }
        //set number of timeshifting files
        hr = m_IStreamBufferConfig.SetBackingFileCount(6, 8); //4-6 files
        if (hr != 0)
        {
          _lastError = String.Format("Unable to set timeshift buffer files to 6-8 files");
          Log.Error("DVBGraph:FAILED to set timeshifting files to 6-8 {0:X}", hr);
          return false;
        }
        //set duration of each timeshift file
        hr = m_IStreamBufferConfig.SetBackingFileDuration((int) iFileDuration); // 60sec * 4 files= 4 mins
        if (hr != 0)
        {
          _lastError = String.Format("Unable to set timeshift buffer length to {0}", iFileDuration);
          Log.Error("DVBGraph:FAILED to set timeshifting filesduration to {0} {1:X}", iFileDuration, hr);
          return false;
        }
        subKey = IntPtr.Zero;
        HKEY = (IntPtr) unchecked((int) 0x80000002L);
        IStreamBufferInitialize pConfig = (IStreamBufferInitialize) m_StreamBufferSink;
        RegOpenKeyEx(HKEY, "SOFTWARE\\MediaPortal", 0, 0x3f, out subKey);
        hr = pConfig.SetHKEY(subKey);
        //set timeshifting filename
        //				Log.WriteFile(LogType.Log,"DVBGraph:set timeshift file to:{0}", fileName);
        IStreamBufferConfigure2 streamConfig2 = m_StreamBufferConfig as IStreamBufferConfigure2;
        if (streamConfig2 != null)
        {
          streamConfig2.SetFFTransitionRates(8, 32);
        }
        // lock on the 'filename' file
        hr = m_IStreamBufferSink.LockProfile(fileName);
        if (hr != 0 && hr != 1)
        {
          _lastError = String.Format("Unable to start timeshifting");
          Log.Error("DVBGraph:FAILED to set timeshift file to:{0} {1:X}", fileName, hr);
          return false;
        }
      }
      finally
      {
        if (pinObj0 != null)
        {
          DirectShowUtil.ReleaseComObject(pinObj0);
        }
        if (pinObj1 != null)
        {
          DirectShowUtil.ReleaseComObject(pinObj1);
        }
        if (pinObj2 != null)
        {
          DirectShowUtil.ReleaseComObject(pinObj2);
        }
        if (pinObj3 != null)
        {
          DirectShowUtil.ReleaseComObject(pinObj3);
        }
        if (outPin != null)
        {
          DirectShowUtil.ReleaseComObject(outPin);
        }
        //if ( streamBufferInitialize !=null)
        //DirectShowUtil.ReleaseComObject(streamBufferInitialize );
      }
      //			(_graphBuilder as IMediaFilter).SetSyncSource(_filterMpeg2Demultiplexer as IReferenceClock);
      return true;
    } //private bool CreateSinkSource(string fileName)

    /// <summary>
    /// Finds and connects pins
    /// </summary>
    /// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
    /// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
    /// <returns>true if succeeded, false if failed</returns>
    protected bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter)
    {
      return ConnectFilters(ref UpstreamFilter, ref DownstreamFilter, 0);
    } //bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter)

    /// <summary>
    /// Finds and connects pins
    /// </summary>
    /// <param name="UpstreamFilter">The Upstream filter which has the output pin</param>
    /// <param name="DownstreamFilter">The downstream filter which has the input filter</param>
    /// <param name="preferredOutputPin">The one-based index of the preferred output pin to use on the Upstream filter.  This is tried first. Pin 1 = 1, Pin 2 = 2, etc</param>
    /// <returns>true if succeeded, false if failed</returns>
    protected bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter,
                                  int preferredOutputPin)
    {
      if (UpstreamFilter == null || DownstreamFilter == null)
      {
        return false;
      }
      int ulFetched = 0;
      int hr = 0;
      IEnumPins pinEnum;
      hr = UpstreamFilter.EnumPins(out pinEnum);
      if ((hr < 0) || (pinEnum == null))
      {
        return false;
      }

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
                    DirectShowUtil.ReleaseComObject(dsPin[0]);
                    DirectShowUtil.ReleaseComObject(outPin[0]);
                    DirectShowUtil.ReleaseComObject(pinEnum);
                    DirectShowUtil.ReleaseComObject(downstreamPins);
                    return true;
                  }
                  DirectShowUtil.ReleaseComObject(dsPin[0]);
                }
              } //while(downstreamPins.Next(1, dsPin, out ulFetched) == 0)
              DirectShowUtil.ReleaseComObject(downstreamPins);
            } //if (outputPinCounter == preferredOutputPin)
          } //if (pinDir == PinDirection.Output)
          DirectShowUtil.ReleaseComObject(outPin[0]);
        } //while(pinEnum.Next(1, outPin, out ulFetched) == 0)
        pinEnum.Reset(); // Move back to start of enumerator
      } //if (preferredOutputPin > 0)

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
                DirectShowUtil.ReleaseComObject(dsPin[0]);
                DirectShowUtil.ReleaseComObject(downstreamPins);
                DirectShowUtil.ReleaseComObject(testPin[0]);
                DirectShowUtil.ReleaseComObject(pinEnum);
                return true;
              }
            } //if (dsPinDir == PinDirection.Input)
            DirectShowUtil.ReleaseComObject(dsPin[0]);
          } //while(downstreamPins.Next(1, dsPin, out ulFetched) == 0)
          DirectShowUtil.ReleaseComObject(downstreamPins);
        } //if(pinDir == PinDirection.Output) // Go and find the input pin.
        DirectShowUtil.ReleaseComObject(testPin[0]);
      } //while(pinEnum.Next(1, testPin, out ulFetched) == 0)
      DirectShowUtil.ReleaseComObject(pinEnum);
      return false;
    }

    //private bool ConnectFilters(ref IBaseFilter UpstreamFilter, ref IBaseFilter DownstreamFilter, int preferredOutputPin)

    /// <summary>
    /// Used to find the Network Provider for addition to the graph.
    /// </summary>
    /// <param name="ClassID">The filter category to enumerate.</param>
    /// <param name="FriendlyName">An identifier based on the DevicePath, used to find the device.</param>
    /// <param name="device">The filter that has been found.</param>
    /// <returns>true of succeeded, false if failed</returns>
    protected bool findNamedFilter(Guid ClassID, string FriendlyName, out object device)
    {
      int hr;
      ICreateDevEnum sysDevEnum = null;
      IEnumMoniker enumMoniker = null;
      sysDevEnum = (ICreateDevEnum) Activator.CreateInstance(Type.GetTypeFromCLSID(ClassId.SystemDeviceEnum, true));
      // Enumerate the filter category
      hr = sysDevEnum.CreateClassEnumerator(ClassID, out enumMoniker, 0);
      if (hr != 0)
      {
        throw new NotSupportedException("No devices in this category");
      }
      IntPtr fetched = IntPtr.Zero;
      IMoniker[] deviceMoniker = new IMoniker[1];
      while (enumMoniker.Next(1, deviceMoniker, fetched) == 0) // while == S_OK
      {
        object bagObj = null;
        Guid bagId = typeof (IPropertyBag).GUID;
        deviceMoniker[0].BindToStorage(null, null, ref bagId, out bagObj);
        IPropertyBag propBag = (IPropertyBag) bagObj;
        object val = "";
        propBag.Read("FriendlyName", out val, null);
        string Name = val as string;
        val = "";
        DirectShowUtil.ReleaseComObject(propBag);
        if (String.Compare(Name, FriendlyName, true) == 0) // If found
        {
          object filterObj = null;
          Guid filterID = typeof (IBaseFilter).GUID;
          deviceMoniker[0].BindToObject(null, null, ref filterID, out filterObj);
          device = filterObj;
          filterObj = null;
          if (device != null)
          {
            DirectShowUtil.ReleaseComObject(deviceMoniker[0]);
            DirectShowUtil.ReleaseComObject(enumMoniker);
            return true;
          }
        } //if(String.Compare(Name.ToLower(), FriendlyName.ToLower()) == 0) // If found
        DirectShowUtil.ReleaseComObject(deviceMoniker[0]);
      } //while(enumMoniker.Next(1, deviceMoniker, out ulFetched) == 0) // while == S_OK
      DirectShowUtil.ReleaseComObject(enumMoniker);
      device = null;
      return false;
    } //private bool findNamedFilter(System.Guid ClassID, string FriendlyName, out object device)

    #endregion

    #region process helper functions

    // send PMT to firedtv device
    protected bool SendPMT()
    {
      try
      {
        //load PMT from disk
        string pmtName = Config.GetFile(Config.Dir.Database, @"pmt\", String.Format("pmt_{0}_{1}_{2}_{3}_{4}.dat",
                                                                                    Util.Utils.FilterFileName(
                                                                                      _currentTuningObject.ServiceName),
                                                                                    _currentTuningObject.NetworkID,
                                                                                    _currentTuningObject.
                                                                                      TransportStreamID,
                                                                                    _currentTuningObject.ProgramNumber,
                                                                                    (int) Network()));
        if (!File.Exists(pmtName))
        {
          //PMT is not on disk...
          Log.Error("pmt {0} not found", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }

        byte[] pmt = null;
        using (FileStream stream = new FileStream(pmtName, FileMode.Open, FileAccess.Read, FileShare.None))
        {
          long len = stream.Length;
          //is file length valid?
          if (len > 6)
          {
            //yes then read the PMT
            pmt = new byte[len];
            stream.Read(pmt, 0, (int) len);
            stream.Close();
          }
        }
        if (pmt == null)
        {
          //NO PMT read
          Log.Error("pmt {0} empty", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }
        if (pmt.Length < 6)
        {
          //PMT length is invalid
          Log.Error("pmt {0} invalid", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }

        //decode PMT
        DVBSections sections = new DVBSections();
        DVBSections.ChannelInfo info = new DVBSections.ChannelInfo();
        if (!sections.GetChannelInfoFromPMT(pmt, ref info))
        {
          //decoding failed
          Log.Error("pmt {0} failed to decode", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }
        //get all pids from the PMT and check for changes
        DVBChannel newChannel = new DVBChannel();
        if (info.pid_list != null)
        {
          bool hasAudio = false;
          int audioOptions = 0;
          for (int pids = 0; pids < info.pid_list.Count; pids++)
          {
            DVBSections.PMTData data = (DVBSections.PMTData) info.pid_list[pids];
            if (data.elementary_PID <= 0)
            {
              data.elementary_PID = -1;
              continue;
            }
            if (data.isVideo)
            {
              newChannel.VideoPid = data.elementary_PID;
            }

            if (data.isAudio)
            {
              switch (audioOptions)
              {
                case 0:
                  newChannel.Audio1 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      newChannel.AudioLanguage1 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
                case 1:
                  newChannel.Audio2 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      newChannel.AudioLanguage2 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
                case 2:
                  newChannel.Audio3 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      newChannel.AudioLanguage3 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
              }

              if (hasAudio == false)
              {
                newChannel.AudioPid = data.elementary_PID;
                if (data.data != null)
                {
                  if (data.data.Length == 3)
                  {
                    newChannel.AudioLanguage = DVBSections.GetLanguageFromCode(data.data);
                  }
                }
                hasAudio = true;
              }
            } //if (data.isAudio)

            if (data.isAC3Audio)
            {
              newChannel.AC3Pid = data.elementary_PID;
            }

            if (data.isTeletext)
            {
              newChannel.TeletextPid = data.elementary_PID;
            }

            if (data.isDVBSubtitle)
            {
              newChannel.SubtitlePid = data.elementary_PID;
            }
          } //for (int pids =0; pids < info.pid_list.Count;pids++)

          newChannel.PCRPid = info.pcr_pid;

          bool changed = false;
          if (_currentTuningObject.AC3Pid != newChannel.AC3Pid)
          {
            changed = true;
          }
          if (_currentTuningObject.AudioPid != newChannel.AudioPid)
          {
            changed = true;
          }
          if (_currentTuningObject.Audio1 != newChannel.Audio1)
          {
            changed = true;
          }
          if (_currentTuningObject.Audio2 != newChannel.Audio2)
          {
            changed = true;
          }
          if (_currentTuningObject.Audio3 != newChannel.Audio3)
          {
            changed = true;
          }
          if (_currentTuningObject.SubtitlePid != newChannel.SubtitlePid)
          {
            changed = true;
          }
          if (_currentTuningObject.TeletextPid != newChannel.TeletextPid)
          {
            changed = true;
          }
          if (_currentTuningObject.VideoPid != newChannel.VideoPid)
          {
            changed = true;
          }
          if (_currentTuningObject.PCRPid != newChannel.PCRPid)
          {
            changed = true;
          }
          if (_currentTuningObject.AudioLanguage != newChannel.AudioLanguage)
          {
            changed = true;
          }
          if (_currentTuningObject.AudioLanguage1 != newChannel.AudioLanguage1)
          {
            changed = true;
          }
          if (_currentTuningObject.AudioLanguage2 != newChannel.AudioLanguage2)
          {
            changed = true;
          }
          if (_currentTuningObject.AudioLanguage3 != newChannel.AudioLanguage3)
          {
            changed = true;
          }

          try
          {
            //did PMT change?
            if (changed)
            {
              _currentTuningObject.AC3Pid = newChannel.AC3Pid;
              _currentTuningObject.AudioPid = newChannel.AudioPid;
              _currentTuningObject.Audio1 = newChannel.Audio1;
              _currentTuningObject.Audio2 = newChannel.Audio2;
              _currentTuningObject.Audio3 = newChannel.Audio3;
              _currentTuningObject.AudioLanguage = newChannel.AudioLanguage;
              _currentTuningObject.AudioLanguage1 = newChannel.AudioLanguage1;
              _currentTuningObject.AudioLanguage2 = newChannel.AudioLanguage2;
              _currentTuningObject.AudioLanguage3 = newChannel.AudioLanguage3;
              _currentTuningObject.SubtitlePid = newChannel.SubtitlePid;
              _currentTuningObject.TeletextPid = newChannel.TeletextPid;
              _currentTuningObject.VideoPid = newChannel.VideoPid;
              _currentTuningObject.PCRPid = newChannel.PCRPid;

              //yes then set mpeg2 demultiplexer mappings so new pids are mapped correctly
              if (_graphState == State.Radio &&
                  (_currentTuningObject.PCRPid <= 0 || _currentTuningObject.PCRPid >= 0x1fff))
              {
                Log.Info("DVBGraph:SendPMT() setup demux:audio pid:{0:X} AC3 pid:{1:X} pcrpid:{2:X}",
                         _currentTuningObject.AudioPid, _currentTuningObject.AC3Pid, _currentTuningObject.PCRPid);
                SetupDemuxer(_pinDemuxerVideo, 0, _pinDemuxerAudio, 0, _pinAC3Out, 0);
                SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.AudioPid, (int) MediaSampleContent.TransportPayload,
                                true);
                SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.PCRPid, (int) MediaSampleContent.TransportPacket,
                                false);
                if (_streamDemuxer != null)
                {
                  _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid,
                                                _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid,
                                                _currentTuningObject.Audio3, _currentTuningObject.ServiceName,
                                                _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
                }
              }
              else
              {
                Log.Info(
                  "DVBGraph:SendPMT() set demux: video pid:{0:X} audio pid:{1:X} AC3 pid:{2:X} audio1 pid:{3:X} audio2 pid:{4:X} audio3 pid:{5:X} subtitle pid:{6:X} teletext pid:{7:X} pcr pid:{8:X}",
                  _currentTuningObject.VideoPid, _currentTuningObject.AudioPid, _currentTuningObject.AC3Pid,
                  _currentTuningObject.Audio1, _currentTuningObject.Audio2, _currentTuningObject.Audio3,
                  _currentTuningObject.SubtitlePid, _currentTuningObject.TeletextPid, _currentTuningObject.PCRPid);
                SetupDemuxer(_pinDemuxerVideo, _currentTuningObject.VideoPid, _pinDemuxerAudio,
                             _currentTuningObject.AudioPid, _pinAC3Out, _currentTuningObject.AC3Pid);
                if (_streamDemuxer != null)
                {
                  _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid,
                                                _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid,
                                                _currentTuningObject.Audio3, _currentTuningObject.ServiceName,
                                                _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
                }
              }
              SetupTsDemuxerMapping();

              //set pids for H/W filtering
              SetHardwarePidFiltering();

              //update tv/radio database
              if (_graphState != State.Radio)
              {
                //tv
                TVDatabase.UpdatePids(Network() == NetworkType.ATSC,
                                      Network() == NetworkType.DVBC,
                                      Network() == NetworkType.DVBS,
                                      Network() == NetworkType.DVBT,
                                      _currentTuningObject);
              }
              else
              {
                //radio
                RadioDatabase.UpdatePids(Network() == NetworkType.ATSC,
                                         Network() == NetworkType.DVBC,
                                         Network() == NetworkType.DVBS,
                                         Network() == NetworkType.DVBT,
                                         _currentTuningObject);
              }
            }
          }
          catch (Exception)
          {
          }
        } //if (info.pid_list!=null)

        _refreshPmtTable = false;
        int pmtVersion = ((pmt[5] >> 1) & 0x1F);
        _lastPMTVersion = pmtVersion;
        if (_cardProperties != null)
        {
          // send the PMT table to the device
          _pmtTimer = DateTime.Now;
          _pmtSendCounter++;
          if (_cardProperties.IsCISupported())
          {
            string camType = "";
            string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _card.FriendlyName));
            using (Settings xmlreader = new Settings(filename))
            {
              camType = xmlreader.GetValueAsString("dvbs", "cam", "Viaccess");
            }
            Log.Info("DVBGraph:Send PMT#{0} version:{1} signal strength:{2} signal quality:{3} locked:{4} cam:{5}",
                     _pmtSendCounter, pmtVersion, SignalStrength(), SignalQuality(), _tunerLocked, camType);
            _streamDemuxer.DumpPMT(pmt);
            int caPmtLen;
            byte[] caPmt = info.caPMT.CaPmtStruct(out caPmtLen);
            if (_currentTuningObject.AC3Pid != 0x0)
            {
              if (_cardProperties.SendPMT(camType, _currentTuningObject.ProgramNumber, _currentTuningObject.VideoPid,
                                          _currentTuningObject.AC3Pid, pmt, (int) pmt.Length, caPmt, caPmtLen))
              {
                return true;
              }
              else
              {
                Log.Info("DVBGraph:Send PMT#{0} version:{1} failed", _pmtSendCounter, pmtVersion);
                _refreshPmtTable = true;
                _lastPMTVersion = -1;
                _pmtSendCounter = 0;
                return true;
              }
            }
            else
            {
              if (_cardProperties.SendPMT(camType, _currentTuningObject.ProgramNumber, _currentTuningObject.VideoPid,
                                          _currentTuningObject.AudioPid, pmt, (int) pmt.Length, caPmt, caPmtLen))
              {
                return true;
              }
              else
              {
                Log.Info("DVBGraph:Send PMT#{0} version:{1} failed", _pmtSendCounter, pmtVersion);
                _refreshPmtTable = true;
                _lastPMTVersion = -1;
                _pmtSendCounter = 0;
                return true;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return false;
    } //SendPMT()

    //Here we update the CAM which is required for some cards when changing audio streams on the channel
    protected bool UpdateCAM()
    {
      try
      {
        //load PMT from disk
        string pmtName = Config.GetFile(Config.Dir.Database, @"pmt\", String.Format("pmt_{0}_{1}_{2}_{3}_{4}.dat",
                                                                                    Util.Utils.FilterFileName(
                                                                                      _currentTuningObject.ServiceName),
                                                                                    _currentTuningObject.NetworkID,
                                                                                    _currentTuningObject.
                                                                                      TransportStreamID,
                                                                                    _currentTuningObject.ProgramNumber,
                                                                                    (int) Network()));
        if (!File.Exists(pmtName))
        {
          //PMT is not on disk...
          Log.Error("pmt {0} not found", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }

        byte[] pmt = null;
        using (FileStream stream = new FileStream(pmtName, FileMode.Open, FileAccess.Read, FileShare.None))
        {
          long len = stream.Length;
          //is file length valid?
          if (len > 6)
          {
            //yes then read the PMT
            pmt = new byte[len];
            stream.Read(pmt, 0, (int) len);
            stream.Close();
          }
        }
        if (pmt == null)
        {
          //NO PMT read
          Log.Error("pmt {0} empty", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }
        if (pmt.Length < 6)
        {
          //PMT length is invalid
          Log.Error("pmt {0} invalid", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }

        //decode PMT
        DVBSections sections = new DVBSections();
        DVBSections.ChannelInfo info = new DVBSections.ChannelInfo();
        if (!sections.GetChannelInfoFromPMT(pmt, ref info))
        {
          //decoding failed
          Log.Error("pmt {0} failed to decode", pmtName);
          _pmtRetyCount = 0;
          _lastPMTVersion = -1;
          _refreshPmtTable = true;
          return false;
        }
        //get all pids from the PMT and check for changes
        DVBChannel newChannel = new DVBChannel();
        if (info.pid_list != null)
        {
          bool hasAudio = false;
          int audioOptions = 0;
          for (int pids = 0; pids < info.pid_list.Count; pids++)
          {
            DVBSections.PMTData data = (DVBSections.PMTData) info.pid_list[pids];
            if (data.elementary_PID <= 0)
            {
              data.elementary_PID = -1;
              continue;
            }
            if (data.isVideo)
            {
              newChannel.VideoPid = data.elementary_PID;
            }

            if (data.isAudio)
            {
              switch (audioOptions)
              {
                case 0:
                  newChannel.Audio1 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      newChannel.AudioLanguage1 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
                case 1:
                  newChannel.Audio2 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      newChannel.AudioLanguage2 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
                case 2:
                  newChannel.Audio3 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      newChannel.AudioLanguage3 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
              }

              if (hasAudio == false)
              {
                newChannel.AudioPid = data.elementary_PID;
                if (data.data != null)
                {
                  if (data.data.Length == 3)
                  {
                    newChannel.AudioLanguage = DVBSections.GetLanguageFromCode(data.data);
                  }
                }
                hasAudio = true;
              }
            } //if (data.isAudio)

            if (data.isAC3Audio)
            {
              newChannel.AC3Pid = data.elementary_PID;
            }

            if (data.isTeletext)
            {
              newChannel.TeletextPid = data.elementary_PID;
            }

            if (data.isDVBSubtitle)
            {
              newChannel.SubtitlePid = data.elementary_PID;
            }
          } //for (int pids =0; pids < info.pid_list.Count;pids++)

          newChannel.PCRPid = info.pcr_pid;
          _refreshPmtTable = false;
          int pmtVersion = ((pmt[5] >> 1) & 0x1F);
          _lastPMTVersion = pmtVersion;
          if (_cardProperties != null)
          {
            // send the PMT table to the device
            _pmtTimer = DateTime.Now;
            _pmtSendCounter++;
            if (_cardProperties.IsCISupported())
            {
              string camType = "";
              string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _card.FriendlyName));
              using (Settings xmlreader = new Settings(filename))
              {
                camType = xmlreader.GetValueAsString("dvbs", "cam", "Viaccess");
              }
              Log.Info("DVBGraph:Send PMT#{0} version:{1} signal strength:{2} signal quality:{3} locked:{4} cam:{5}",
                       _pmtSendCounter, pmtVersion, SignalStrength(), SignalQuality(), _tunerLocked, camType);
              _streamDemuxer.DumpPMT(pmt);
              int caPmtLen;
              byte[] caPmt = info.caPMT.CaPmtStruct(out caPmtLen);
              if (_currentTuningObject.AC3Pid != 0x0)
              {
                if (_cardProperties.SendPMT(camType, _currentTuningObject.ProgramNumber, _currentTuningObject.VideoPid,
                                            _currentTuningObject.AC3Pid, pmt, (int) pmt.Length, caPmt, caPmtLen))
                {
                  return true;
                }
                else
                {
                  Log.Info("DVBGraph:Send PMT#{0} version:{1} failed", _pmtSendCounter, pmtVersion);
                  _refreshPmtTable = true;
                  _lastPMTVersion = -1;
                  _pmtSendCounter = 0;
                  return true;
                }
              }
              else
              {
                if (_cardProperties.SendPMT(camType, _currentTuningObject.ProgramNumber, _currentTuningObject.VideoPid,
                                            _currentTuningObject.AudioPid, pmt, (int) pmt.Length, caPmt, caPmtLen))
                {
                  return true;
                }
                else
                {
                  Log.Info("DVBGraph:Send PMT#{0} version:{1} failed", _pmtSendCounter, pmtVersion);
                  _refreshPmtTable = true;
                  _lastPMTVersion = -1;
                  _pmtSendCounter = 0;
                  return true;
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return false;
    }

    /// <summary>
    /// GetDisEqcSettings()
    /// This method gets the disEqc settings for the tv channel specified
    /// </summary>
    /// <param name="ch">tvchannel</param>
    /// <param name="lowOsc">[out] low oscillator</param>
    /// <param name="hiOsc">[out] high oscillator</param>
    /// <param name="diseqcUsed">[out] diseqc used for this channel (0-6)</param>
    protected void GetDisEqcSettings(ref DVBChannel ch, out int lowOsc, out int hiOsc, out int lnbKhzTone,
                                     out int diseqcUsed)
    {
      diseqcUsed = 0;
      lowOsc = 9750;
      hiOsc = 10600;
      lnbKhzTone = 0;
      try
      {
        string filename = Config.GetFile(Config.Dir.Database, String.Format("card_{0}.xml", _card.FriendlyName));

        int lnbKhz = -1;
        int diseqc = -1;
        int lnbKind = 0;
        // lnb config
        int lnb0MHZ = 0;
        int lnb1MHZ = 0;
        int lnbswMHZ = 0;
        int cbandMHZ = 0;
        int circularMHZ = 0;

        using (Settings xmlreader = new Settings(filename))
        {
          // read global LNB settings
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
              Log.Info("DVBGraph: using profile diseqc 1 LNB:{0} kHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              break;
            case 2:
              // config b
              lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb2", 22);
              diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc2", 1);
              lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind2", 0);
              Log.Info("DVBGraph: using profile diseqc 2 LNB:{0} kHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              break;
            case 3:
              // config c
              lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb3", 22);
              diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc3", 1);
              lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind3", 0);
              Log.Info("DVBGraph: using profile diseqc 3 LNB:{0} kHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              break;
              //
            case 4:
              // config d
              lnbKhz = xmlreader.GetValueAsInt("dvbs", "lnb4", 22);
              diseqc = xmlreader.GetValueAsInt("dvbs", "diseqc4", 1);
              lnbKind = xmlreader.GetValueAsInt("dvbs", "lnbKind4", 0);
              Log.Info("DVBGraph: using profile diseqc 4 LNB:{0} kHz diseqc:{1} lnbKind:{2}", lnbKhz, diseqc, lnbKind);
              //
              break;
            default:
              Log.Warn("DVBGraph: unknown LNB#:{0}; using defaults: LNB:{1} kHz diseqc:{2} lnbKind:{3}", ch.DiSEqC,
                       lnbKhz, diseqc, lnbKind);
              break;
          } // switch(disNo)
        } //using(MediaPortal.Profile.Xml xmlreader=new MediaPortal.Profile.Xml(m_cardFilename))

        /*
        switch ( lnbKind )
        {
          case 0: // KU-Band
            break;
          case 1: // C-Band
            break;
          case 2: // Circular-Band
            break;
        }
         */

        // LNB switch frequency
        ch.LnbSwitchFrequency = lnbswMHZ*1000; // so 11700000

        if (ch.Frequency >= lnbswMHZ*1000)
        {
          // set LNB frequency to high band
          ch.LNBFrequency = lnb1MHZ; // 10600
        }
        else
        {
          // set LNB frequency to lo band
          ch.LNBFrequency = lnb0MHZ; // 9750
        }
        lowOsc = lnb0MHZ;
        hiOsc = lnb1MHZ;
        if (lnbKhz != -1)
        {
          lnbKhzTone = lnbKhz;
        }
        if (diseqc != -1)
        {
          diseqcUsed = diseqc;
        }

        Log.Info("DVBGraph: LNB#{0} Settings: freq={1} lnbKHz={2} lnbFreq={3} diseqc={4}", ch.DiSEqC, ch.Frequency,
                 ch.LnbSwitchFrequency, ch.LNBFrequency, diseqcUsed);
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
    protected void SetDVBSInputRangeParameter(int disEqcUsed, IDVBSTuningSpace dvbSpace)
    {
      try
      {
        if (dvbSpace == null)
        {
          return;
        }
        //
        // A:LOWORD -> LOBYTE -> Bit0 for Position (0-A,1-B)          
        // B:LOWORD -> HIBYTE -> Bit0 for 22kHz    (0-Off,1-On)
        // C:HIWORD -> LOBYTE -> Bit0 for Option   (0-A,1-B)
        // D:HIWORD -> HIBYTE -> Bit0 for Burst    (0-Off,1-On)
        // hi         low        hi        low
        // 87654321 | 87654321 | 87654321 | 87654321 |
        //        D          C          B          A
        long inputRange = 0;
        switch (disEqcUsed)
        {
          case 0: //none
            Log.Info("DVBGraph: disEqc:none");
            return;
          case 1: //simple A
            Log.Info("DVBGraph: disEqc:simple A (not supported)");
            return;
          case 2: //simple B
            Log.Info("DVBGraph: disEqc:simple B (not supported)");
            return;
          case 3: //Level 1 A/A     0000-0000 0000-0000
            Log.Info("DVBGraph: disEqc:level 1 A/A");
            inputRange = 0;
            break;
          case 4: //Level 1 B/A     0000-0001 0000-0000
            Log.Info("DVBGraph: disEqc:level 1 B/A");
            inputRange = 1 << 16;
            break;
          case 5: //Level 1 A/B     0000-0000 0000-0001
            Log.Info("DVBGraph: disEqc:level 1 A/B");
            inputRange = 1;
            break;
          case 6: //Level 1 B/B     0000-0001 0000-0001
            Log.Info("DVBGraph: disEqc:level 1 B/B");
            inputRange = (1 << 16) + 1;
            break;
        }
        // test with burst on
        //inputRange|=1<<24;

        if (_currentTuningObject.Frequency >= _currentTuningObject.LnbSwitchFrequency) // 22kHz
        {
          inputRange |= (1 << 8);
        }

        Log.Info("DVBGraph: Set inputrange to:{0:X}", inputRange);
        dvbSpace.InputRange = inputRange.ToString();
      }
      catch (Exception)
      {
      }
    }

    protected void CheckVideoResolutionChanges()
    {
      if (GUIGraphicsContext.Vmr9Active)
      {
        return;
      }
      if (_graphState != State.Viewing)
      {
        return;
      }
      if (_videoWindowInterface == null || _basicVideoInterFace == null)
      {
        return;
      }
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
      //Log.Info("DVBGraphBase.UpdateVideoState() Viewing: {0}", isViewing);
      if (!isViewing)
      {
        return;
      }
      //      Log.Info("packets:{0} pmt:{1:X}  vmr9:{2} fps:{3} locked:{4} quality:{5} level:{6}",
      //      _streamDemuxer.ReceivingPackets, _lastPMTVersion, GUIGraphicsContext.Vmr9Active, GUIGraphicsContext.Vmr9FPS, TunerLocked(), SignalQuality(), SignalStrength());

      // do we receive any packets?
      /*
      if (!SignalPresent())
      {
        TimeSpan ts = DateTime.Now - _signalLostTimer;
        if (ts.TotalSeconds < 5)
        {
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
          return;
        }
        //no, then state = no signal
        Log.Info("DVBGraphBDA: No signal quality:{0} strength:{1} locked:{2} fps:{3}", SignalQuality(), SignalStrength(), SignalPresent(), GUIGraphicsContext.Vmr9FPS);
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
        return;
      }
      else if (_streamDemuxer.IsScrambled)
      {
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.Scrambled;
        _signalLostTimer = DateTime.Now;
        return;
      }
      else 
       */
      if (_streamDemuxer.IsScrambled)
      {
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.Scrambled;
        return;
      }
      if (GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.Vmr9FPS < 1f)
      {
        if ((g_Player.Playing && !g_Player.Paused) || (!g_Player.Playing))
        {
          /*TimeSpan ts = DateTime.Now - _signalLostTimer;
          if (ts.TotalSeconds < 8) //if (ts.TotalSeconds <VideoRendererStatistics.NoSignalTimeOut)
          {
            VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
            return;
          }
          Log.Info("DVBGraphBDA: VMR9 stopped quality:{0} strength:{1} locked:{2} fps:{3}", SignalQuality(), SignalStrength(), SignalPresent(), GUIGraphicsContext.Vmr9FPS);
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
          return;*/
          if (_notifySignalLost)
          {
            Log.Info("DVBGraphBDA: VMR9 stopped quality:{0} strength:{1} locked:{2} fps:{3}", SignalQuality(),
                     SignalStrength(), SignalPresent(), GUIGraphicsContext.Vmr9FPS);
            VideoRendererStatistics.VideoState = VideoRendererStatistics.State.NoSignal;
            return;
          }
          else
          {
            VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
            return;
          }
        }
      }
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      //_signalLostTimer = DateTime.Now;
    }

    protected bool ProcessEpg()
    {
      _epgGrabber.Process();
      if (_epgGrabber.Done)
      {
        _epgGrabber.Reset();
        if (_graphState == State.Epg)
        {
          Log.Info("DVBGraph:EPG done");
          _mediaControl.Stop();
          _isGraphRunning = false;
          //_graphState = State.Created;
          return true;
        }
      }
      return false;
    }

    /// <summary>Checks signal and waits for Tuner Lock while a max given duration, then updates videostate 
    /// </summary>
    /// <param name="_duration">Specifies how long, in milliseconds, we are waiting for the signal to be back</param>
    protected void ProcessSignal(int _duration)
    {
      if (_duration == 0)
      {
        _duration = 3000; //default
      }
      if (!_signalPresent) // tuner is unlocked
      {
        Log.Info("DVBGraph: Unlocked... wait for tunerlock");
        // give one more chance to the tuner to be locked
        DateTime dt = DateTime.Now;
        while (!_signalPresent)
        {
          TimeSpan ts = DateTime.Now - dt;
          if (ts.TotalMilliseconds >= _duration)
          {
            break; // no longer than _duration
          }
          Thread.Sleep(100); // will check 10 times per second
          UpdateSignalPresent();
        }
        Log.Info("Tuner locked: {0}", _signalPresent);
        if (_signalPresent) // got signal back ?
        {
          _signalPresent = true;
          _notifySignalLost = false;
          VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
          _processTimer.AddSeconds(-10); // next process() call won't return 
          // is was actually entered with a lost signal
        }
        else
        {
          //  notify nosignal
          _notifySignalLost = true;
          UpdateVideoState();
          // 
          // todo
        }
      }
    }

    public void Process()
    {
      if (_graphState == State.None)
      {
        return;
      }
      if (_inScanningMode == false)
      {
        if (_cardProperties.IsCISupported() && _refreshPmtTable == true && _graphState != State.Epg)
        {
          //
          //we need to receive & transmit the PMT to the card as fast as possible
        }
        else
        {
          TimeSpan tsProc = DateTime.Now - _processTimer;
          if (tsProc.TotalMilliseconds < 5000 && _signalPresent && _refreshPmtTable == false)
          {
            return; // original value was set to 5' 
          }
          _processTimer = DateTime.Now; // will not return if we got no signal
        }
      }
      if (_graphState == State.Created)
      {
        return;
      }
      if (_inScanningMode == true)
      {
        return;
      }
      //if (_streamDemuxer != null)
      //  _streamDemuxer.Process();
      if (ProcessEpg())
      {
        return;
      }
      UpdateSignalPresent();
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
          Log.Info("wait for pmt :{0}", _lastPMTVersion);
          //bool gotPMT = false;
          //Log.Info("DVBGraph:Get PMT {0}", _graphState);
          IntPtr pmtMem = Marshal.AllocCoTaskMem(4096); // max. size for pmt
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
                //gotPMT = true;
                //Log.Info("DVBGraph:Got PMT version:{0} {1}", version, _lastPMTVersion);
                if (_lastPMTVersion != version)
                {
                  Log.Info("DVBGraph:Got PMT version:{0}", version);
                  m_streamDemuxer_OnPMTIsChanged(pmt);
                }
                else
                {
                  //	Log.Info("DVBGraph:Got old PMT version:{0} {1}",_lastPMTVersion,version);
                }
              }
              else
              {
                //ushort chcount = 0;
                //_analyzerInterface.GetChannelCount(ref chcount);
                //Log.Info("DVBGraph:Got wrong PMT:{0} {1} channels:{2}", pmtProgramNumber, _currentTuningObject.ProgramNumber, chcount);
              }
              pmt = null;
            }
            Marshal.FreeCoTaskMem(pmtMem);
          }
        }
        //_signalLostTimer = DateTime.Now;
      }
      ProcessSignal(5000);
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
    } //public void Process()

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
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneChannel() get ATSC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0, physicalChannel = 0;
            int minorChannel = 0, majorChannel = 0;
            TVDatabase.GetATSCTuneRequest(channel.ID, out physicalChannel, out providerName, out frequency,
                                          out symbolrate, out innerFec, out modulation, out ONID, out TSID, out SID,
                                          out audioPid, out videoPid, out teletextPid, out pmtPid, out audio1,
                                          out audio2, out audio3, out ac3Pid, out audioLanguage, out audioLanguage1,
                                          out audioLanguage2, out audioLanguage3, out minorChannel, out majorChannel,
                                          out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
            if (physicalChannel == -1)
            {
              Log.Error("DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
              return;
            }
            frequency = 0;
            symbolrate = 0;
            Log.Info(
              "DVBGraph:  tuning details: frequency:{0} kHz physicalChannel:{1} major channel:{2} minor channel:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8} video:0x{9:X} audio:0x{10:X} pcr:0x{11:X}",
              frequency, physicalChannel, minorChannel, majorChannel, modulation, ONID, TSID, SID, providerName,
              videoPid, audioPid, pcrPid);
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
            _currentTuningObject.ServiceType = 1;
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneChannel() submit tuning request");
          }
          break;

        case NetworkType.DVBC:
          {
            //get the DVB-C tuning details from the tv database
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneChannel() get DVBC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0;
            TVDatabase.GetDVBCTuneRequest(channel.ID, out providerName, out frequency, out symbolrate, out innerFec,
                                          out modulation, out ONID, out TSID, out SID, out audioPid, out videoPid,
                                          out teletextPid, out pmtPid, out audio1, out audio2, out audio3, out ac3Pid,
                                          out audioLanguage, out audioLanguage1, out audioLanguage2, out audioLanguage3,
                                          out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
            if (frequency <= 0)
            {
              Log.Error("DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
              return;
            }
            Log.Info(
              "DVBGraph:  tuning details: frequency:{0} kHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
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
            _currentTuningObject.ServiceType = 1;
          }
          break;

        case NetworkType.DVBS:
          {
            //get the DVB-S tuning details from the tv database
            //for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneChannel() get DVBS tuning details");
            DVBChannel ch = new DVBChannel();
            if (TVDatabase.GetSatChannel(channel.ID, Mpeg2VideoServiceType, ref ch) == false) //only television
            {
              if (TVDatabase.GetSatChannel(channel.ID, Mpeg4VideoServiceType, ref ch) == false) //only television
              {
                Log.Error("DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
                return;
              }
            }
            Log.Info(
              "DVBGraph:  tuning details: frequency:{0} kHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
              ch.Frequency, ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID, ch.ProgramNumber,
              ch.ServiceProvider);

            //bool needSwitch = true;
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
            _currentTuningObject.LnbSwitchFrequency = ch.LnbSwitchFrequency;
            _currentTuningObject.HasEITPresentFollow = ch.HasEITPresentFollow;
            _currentTuningObject.HasEITSchedule = ch.HasEITSchedule;
            _currentTuningObject.ServiceType = ch.ServiceType;
          }
          break;

        case NetworkType.DVBT:
          {
            //get the DVB-T tuning details from the tv database
            //for DVB-T this is the frequency, ONID , TSID and SID
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneChannel() get DVBT tuning details");
            TVDatabase.GetDVBTTuneRequest(channel.ID, out providerName, out frequency, out ONID, out TSID, out SID,
                                          out audioPid, out videoPid, out teletextPid, out pmtPid, out bandWidth,
                                          out audio1, out audio2, out audio3, out ac3Pid, out audioLanguage,
                                          out audioLanguage1, out audioLanguage2, out audioLanguage3,
                                          out HasEITPresentFollow, out HasEITSchedule, out pcrPid);
            if (frequency <= 0)
            {
              Log.Error("DVBGraph:database invalid tuning details for channel:{0}", channel.ID);
              return;
            }
            Log.Info("DVBGraph:  tuning details: frequency:{0} kHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency,
                     ONID, TSID, SID, providerName);
            //get the IDVBTLocator interface from the new tuning request
            //bool needSwitch = true;
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
            _currentTuningObject.ServiceType = 1;
          }
          break;
      } //switch (_networkType)
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
      /*
      if (UseTsTimeShifting)
      {
        if (_graphState == State.TimeShifting)
        {
          if (_filterTsFileSink!=null)
          {
            if (_graphState == State.TimeShifting)
            {
              _mediaControl.Stop();
              int hr=_graphBuilder.RemoveFilter((IBaseFilter)_filterTsFileSink);
              Log.Info("DVBGraph:remove TsFileSink:0x{0:X}", hr);
              CreateTsTimeShifting(_currentTimeShiftFileName, false);
              _mediaControl.Run();
            }
          }
        }
      }*/
      try
      {
        VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
        string fname = Recorder.GetTimeShiftFileNameByCardId(_cardId);
        if (g_Player.Playing && g_Player.CurrentFile == fname)
        {
          _graphPaused = true;
        }
        if (_vmr9 != null)
        {
          _vmr9.Enable(false);
        }
        _currentChannelNumber = channel.Number;
        _startTimer = DateTime.Now;
        Log.Info("DVBGraph:TuneChannel() tune to channel:{0}", channel.ID);
        GetTvChannelFromDatabase(channel);
        if (_currentTuningObject == null)
        {
          return;
        }
        SubmitTuneRequest(_currentTuningObject);
        if (_currentTuningObject.AC3Pid <= 0)
        {
          _currentTuningObject.AC3Pid = -1;
        }
        if (_currentTuningObject.AudioPid <= 0)
        {
          _currentTuningObject.AudioPid = -1;
        }
        if (_currentTuningObject.Audio1 <= 0)
        {
          _currentTuningObject.Audio1 = -1;
        }
        if (_currentTuningObject.Audio2 <= 0)
        {
          _currentTuningObject.Audio2 = -1;
        }
        if (_currentTuningObject.Audio2 <= 0)
        {
          _currentTuningObject.Audio2 = -1;
        }
        if (_currentTuningObject.SubtitlePid <= 0)
        {
          _currentTuningObject.SubtitlePid = -1;
        }
        if (_currentTuningObject.TeletextPid <= 0)
        {
          _currentTuningObject.TeletextPid = -1;
        }
        if (_currentTuningObject.VideoPid <= 0)
        {
          _currentTuningObject.VideoPid = -1;
        }
        if (_currentTuningObject.PMTPid <= 0)
        {
          _currentTuningObject.PMTPid = -1;
        }
        if (_currentTuningObject.PCRPid <= 0)
        {
          _currentTuningObject.PCRPid = -1;
        }
        try
        {
          DirectShowUtil.EnableDeInterlace(_graphBuilder);
        }
        catch (Exception ex)
        {
          Log.Error(ex);
        }
        //SendPMT();
        _refreshPmtTable = true;
        _lastPMTVersion = -1;
        _pmtRetyCount = 0;
        _analyzerInterface.ResetParser();
        //Log.Info("DVBGraph:set mpeg2demuxer video:0x{0:x} audio:0x{1:X} ac3:0x{2:X}",
        //                _currentTuningObject.VideoPid, _currentTuningObject.AudioPid, _currentTuningObject.AC3Pid);
        SetupDemuxer(_pinDemuxerVideo, _currentTuningObject.VideoPid, _pinDemuxerAudio, _currentTuningObject.AudioPid,
                     _pinAC3Out, _currentTuningObject.AC3Pid);
        if (_streamDemuxer != null)
        {
          _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid,
                                        _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid,
                                        _currentTuningObject.Audio3, _currentTuningObject.ServiceName,
                                        _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
          _streamDemuxer.OnTuneNewChannel();
        }
        SetupTsDemuxerMapping();
        //map teletext pid to ttx output pin of mpeg2 demultiplexer
        if (_pinTeletext != null)
        {
          SetupDemuxerPin(_pinTeletext, _currentTuningObject.TeletextPid, (int) MediaSampleContent.TransportPacket, true);
        }
      }
      finally
      {
        //				if (restartGraph)
        //         g_Player.ContinueGraph();
        if (_vmr9 != null)
        {
          _vmr9.Enable(true);
        }
        //_signalLostTimer = DateTime.Now;
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
      catch (Exception)
      {
      }
      Log.Info("DVBGraph: wait for tunerlock");
      //wait until tuner is locked
      DateTime dt = DateTime.Now;
      while (!_signalPresent) // will exit if we found a signal                              
      {
        TimeSpan ts = DateTime.Now - dt;
        if (ts.TotalMilliseconds >= 5000)
        {
          break; // won't wait more than 5'    
        }
        Thread.Sleep(100); // 10 checks/s
        UpdateSignalPresent();
      }
      Log.Info("DVBGraph:TuneChannel done signal strength:{0} signal quality:{1} locked:{2}", SignalStrength(),
               SignalQuality(), _tunerLocked);
    } //public void TuneChannel(AnalogVideoStandard standard,int iChannel,int country)

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
      if (tuningObject == null)
      {
        return;
      }
      _inScanningMode = true;
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      //start viewing if we're not yet viewing
      if (!_isGraphRunning)
      {
        Log.Info("Start graph!");
        if (_mediaControl == null)
        {
          _mediaControl = (IMediaControl) _graphBuilder;
        }
        int hr = _mediaControl.Run();
        if (hr < 0)
        {
          Log.Error("DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
          return;
        }
        _isGraphRunning = true;
      }
      _currentTuningObject = (DVBChannel) tuningObject;
      SetupDiseqc(disecqNo);
      _scanPidListReady = false;
      _scanPidList.Clear();
      try
      {
        _analyzerInterface.SetPidFilterCallback(this);
      }
      catch (Exception)
      {
      }
      SubmitTuneRequest(_currentTuningObject);
      _analyzerInterface.ResetParser();
      ArrayList pids = new ArrayList();
      if (Network() == NetworkType.ATSC)
      {
        pids.Add((ushort) 0x1ffb);
        SendHWPids(pids);
        SetupDemuxerPin(_pinDemuxerSections, 0x1ffb, (int) MediaSampleContent.Mpeg2PSI, true);
      }
      else
      {
        pids.Add((ushort) 0);
        pids.Add((ushort) 0x10);
        pids.Add((ushort) 0x11);
        SendHWPids(pids);
        SetupDemuxerPin(_pinDemuxerSections, 0, (int) MediaSampleContent.Mpeg2PSI, true);
        SetupDemuxerPin(_pinDemuxerSections, 0x10, (int) MediaSampleContent.Mpeg2PSI, false);
        SetupDemuxerPin(_pinDemuxerSections, 0x11, (int) MediaSampleContent.Mpeg2PSI, false);
      }
      Log.Info("DVBGraph: wait for tunerlock");
      //wait until tuner is locked
      DateTime dt = DateTime.Now;
      while (!_signalPresent) // will exit if we found a signal                              
      {
        TimeSpan ts = DateTime.Now - dt;
        if (ts.TotalMilliseconds >= 5000)
        {
          break; // won't wait more than 5'    
        }
        Thread.Sleep(100); // 10 checks/s
        UpdateSignalPresent();
      }
      //         
      //      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
    } //public void Tune(object tuningObject)

    protected virtual void SetupDiseqc(int disecqNo)
    {
    }

    /// <summary>
    /// Store any new tv and/or radio channels found in the tvdatabase
    /// </summary>
    /// <param name="radio">if true:Store radio channels found in the database</param>
    /// <param name="tv">if true:Store tv channels found in the database</param>
    public void StoreChannels(int ID, bool radio, bool tv, ref int newChannels, ref int updatedChannels,
                              ref int newRadioChannels, ref int updatedRadioChannels)
    {
      //it may take a while before signal quality/level is correct
      if (_filterDvbAnalyzer == null)
      {
        return;
      }
      Log.Info("DVBGraph: StoreChannels() signal level:{0} signal quality:{1} locked:{2}", SignalStrength(),
               SignalQuality(), _tunerLocked);
      TVDatabase.ClearCache();
      //get list of current tv channels present in the database
      List<TVChannel> tvChannels = new List<TVChannel>();
      TVDatabase.GetChannels(ref tvChannels);
      DVBSections.Transponder transp;
      transp.channels = null;
      DateTime dt = DateTime.Now;
      if (Network() == NetworkType.ATSC)
      {
        ushort count = 0;
        while (true)
        {
          TimeSpan ts = DateTime.Now - dt;
          if (ts.TotalMilliseconds > 8000)
          {
            break;
          }
          _analyzerInterface.GetChannelCount(ref count);
          if (count > 0)
          {
            break;
          }
          Thread.Sleep(2000);
        }
        _analyzerInterface.GetChannelCount(ref count);
        if (count == 0)
        {
          Log.Info("DVBGraph: found 0 channels");
          return;
        }
        using (DVBSections sections = new DVBSections())
        {
          sections.DemuxerObject = _streamDemuxer;
          sections.Timeout = 2500;
          //wait until all channels are received
          dt = DateTime.Now;
          transp.channels = new ArrayList();
          bool allFound = true;
          bool newChannelsFound = false;
          bool[] channelReady = new bool[count];
          do
          {
            newChannelsFound = false;
            allFound = true;
            for (int index = 0; index < count; index++)
            {
              if (channelReady[index])
              {
                continue;
              }
              if (_analyzerInterface.IsChannelReady(index) != 0)
              {
                //channel not ready
                allFound = false;
              }
              else
              {
                //channel is ready
                DVBSections.ChannelInfo chi = new DVBSections.ChannelInfo();
                UInt16 len = 0;
                int hr = 0;
                hr = _analyzerInterface.GetCISize(ref len);
                IntPtr mmch = Marshal.AllocCoTaskMem(len);
                try
                {
                  hr = _analyzerInterface.GetChannel((UInt16) (index), mmch);
                  chi = sections.GetChannelInfo(mmch);
                  chi.fec = _currentTuningObject.FEC;
                  _currentTuningObject.Frequency = 0;
                  _currentTuningObject.Modulation = chi.modulation;
                }
                finally
                {
                  Marshal.FreeCoTaskMem(mmch);
                }
                transp.channels.Add(chi);
                channelReady[index] = true;
                newChannelsFound = true;
              }
            } // for (int index = 0; index < hwPids.Count-offset; ++index)
            if (!allFound)
            {
              if (newChannelsFound)
              {
                dt = DateTime.Now;
              }
              Thread.Sleep(500);
              TimeSpan ts = DateTime.Now - dt;
              if (ts.TotalMilliseconds >= 2000)
              {
                break;
              }
            }
          } while (!allFound);
        }
      }
      else
      {
        //wait until PAT/SDT has been received (max 5 secs)
        while (true)
        {
          TimeSpan ts = DateTime.Now - dt;
          if (ts.TotalMilliseconds > 8000)
          {
            break;
          }
          Thread.Sleep(2000);
          if (_scanPidListReady)
          {
            break;
          }
        }
        ArrayList hwPids = (ArrayList) _scanPidList.Clone();
        string pidList = "";
        for (int i = 0; i < hwPids.Count; ++i)
        {
          pidList += String.Format("0x{0:X},", (ushort) hwPids[i]);
        }
        Log.Info("check...{0} pids:{1} {2}", _scanPidListReady, hwPids.Count, pidList);
        if (_scanPidListReady == false)
        {
          return;
        }
        //setup MPEG2 demuxer so it sends the PMT's to the analyzer filter
        for (int i = 0; i < hwPids.Count; ++i)
        {
          ushort pid = (ushort) hwPids[i];
          SetupDemuxerPin(_pinDemuxerSections, pid, (int) MediaSampleContent.Mpeg2PSI, (i == 0));
        }
        //      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
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
              if (channelReady[index])
              {
                continue;
              }
              if (_analyzerInterface.IsChannelReady(index) != 0)
              {
                //channel not ready
                allFound = false;
              }
              else
              {
                //channel is ready
                DVBSections.ChannelInfo chi = new DVBSections.ChannelInfo();
                UInt16 len = 0;
                int hr = 0;
                hr = _analyzerInterface.GetCISize(ref len);
                IntPtr mmch = Marshal.AllocCoTaskMem(len);
                try
                {
                  hr = _analyzerInterface.GetChannel((UInt16) (index), mmch);
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
                Log.Info("channel:{0}/{1} pid:0x{2:X} ready", index, (hwPids.Count - offset), hwPids[offset + index]);
                hwPids[offset + index] = (ushort) 0x2000;
              }
            } // for (int index = 0; index < hwPids.Count-offset; ++index)
            // update h/w pids
            if (!allFound && newChannelsFound)
            {
              ArrayList pids = new ArrayList();
              for (int i = 0; i < hwPids.Count; ++i)
              {
                ushort pid = (ushort) hwPids[i];
                if (pid < 0x1fff)
                {
                  pids.Add(pid);
                }
              }
              SendHWPids(pids);
            }
            if (!allFound)
            {
              if (newChannelsFound)
              {
                dt = DateTime.Now;
              }
              Thread.Sleep(500);
              TimeSpan ts = DateTime.Now - dt;
              if (ts.TotalMilliseconds >= 2000)
              {
                break;
              }
            }
          } while (!allFound);
        }
      }
      if (transp.channels == null)
      {
        Log.Info("DVBGraph: found no channels", transp.channels);
        return;
      }
      Log.Info("DVBGraph: found {0}", transp.channels.Count);
      for (int i = 0; i < transp.channels.Count; ++i)
      {
        DVBSections.ChannelInfo info = (DVBSections.ChannelInfo) transp.channels[i];
        if (info.service_provider_name == null)
        {
          info.service_provider_name = "";
        }
        if (info.service_name == null)
        {
          info.service_name = "";
        }
        info.service_provider_name = info.service_provider_name.Trim();
        info.service_name = info.service_name.Trim();
        if (info.service_provider_name.Length == 0)
        {
          info.service_provider_name = Strings.Unknown;
        }
        if (info.service_name.Length == 0)
        {
          if (_networkType == NetworkType.ATSC)
          {
            info.service_name = String.Format("NoName:  {0}-{1} ({2})", info.majorChannel, info.minorChannel,
                                              _currentTuningObject.PhysicalChannel);
          }
          else
          {
            info.service_name = String.Format("NoName:{0}{1}{2}{3}", info.networkID, info.transportStreamID,
                                              info.serviceID, i);
          }
        }
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
        _currentTuningObject.AudioLanguage = string.Empty;
        _currentTuningObject.AudioLanguage1 = string.Empty;
        _currentTuningObject.AudioLanguage2 = string.Empty;
        _currentTuningObject.AudioLanguage3 = string.Empty;
        //check if this channel has audio/video streams
        int audioOptions = 0;
        if (info.pid_list != null)
        {
          for (int pids = 0; pids < info.pid_list.Count; pids++)
          {
            DVBSections.PMTData data = (DVBSections.PMTData) info.pid_list[pids];
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
                    {
                      _currentTuningObject.AudioLanguage1 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
                case 1:
                  _currentTuningObject.Audio2 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      _currentTuningObject.AudioLanguage2 = DVBSections.GetLanguageFromCode(data.data);
                    }
                  }
                  audioOptions++;
                  break;
                case 2:
                  _currentTuningObject.Audio3 = data.elementary_PID;
                  if (data.data != null)
                  {
                    if (data.data.Length == 3)
                    {
                      _currentTuningObject.AudioLanguage3 = DVBSections.GetLanguageFromCode(data.data);
                    }
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
                  {
                    _currentTuningObject.AudioLanguage = DVBSections.GetLanguageFromCode(data.data);
                  }
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
        if (info.serviceType != Mpeg2VideoServiceType && info.serviceType != Mpeg2AudioServiceType &&
            info.serviceType != Mpeg4VideoServiceType)
        {
          Log.Info(
            "DVBGraph:unknown service type: provider:{0} service:{1} scrambled:{2} frequency:{3} kHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:0x{9:X} videopid:0x{10:X} teletextpid:0x{11:X} program:{12} pcr pid:0x{13:X} service type:{14} major:{15} minor:{16} pmt:0x{19:X}",
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
            info.serviceType, info.majorChannel, info.minorChannel, info.network_pmt_PID);
          continue;
        }
        Log.Info(
          "DVBGraph:Found provider:{0} service:{1} scrambled:{2} frequency:{3} kHz networkid:{4} transportid:{5} serviceid:{6} tv:{7} radio:{8} audiopid:0x{9:X} videopid:0x{10:X} teletextpid:0x{11:X} program:{12} pcr pid:0x{13:X} ac3 pid:0x{14:X} major:{15} minor:{16} LCN:{17} type:{18} pmt:0x{19)",
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
          info.pcr_pid, _currentTuningObject.AC3Pid, info.majorChannel, info.minorChannel, info.LCN, info.serviceType,
          info.network_pmt_PID);
        if (info.serviceID == 0)
        {
          Log.Info("DVBGraph: channel#{0} has no service id", i);
          continue;
        }
        bool isRadio = ((!hasVideo) && hasAudio);
        bool isTv = (hasVideo); //some tv channels dont have an audio stream
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
        newchannel.ServiceType = info.serviceType; //tv
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
        newchannel.LnbSwitchFrequency = _currentTuningObject.LnbSwitchFrequency;
        newchannel.PhysicalChannel = _currentTuningObject.PhysicalChannel;
        newchannel.MinorChannel = info.minorChannel;
        newchannel.MajorChannel = info.majorChannel;
        newchannel.HasEITPresentFollow = info.eitPreFollow;
        newchannel.HasEITSchedule = info.eitSchedule;
        if (info.serviceType == Mpeg2VideoServiceType || info.serviceType == Mpeg4VideoServiceType) //tv
        {
          Log.Info("DVBGraph: channel {0} is a tv channel", newchannel.ServiceName);
          bool isNewChannel = true;
          TVChannel existingTvChannel = new TVChannel();
          existingTvChannel.Name = newchannel.ServiceName;
          int channelId = -1;
          //check if there is a TV channel with the DVB channel service name
          foreach (TVChannel tvchan in tvChannels)
          {
            if (String.Compare(tvchan.Name, newchannel.ServiceName, true) == 0)
            {
              //yes TV channel with this name exists in the database...
              existingTvChannel = tvchan;
              isNewChannel = false;
              channelId = tvchan.ID;
              break;
            }
          }
          if (isNewChannel == false)
          {
            // tv channel with the service name already exists
            // check if there is a DVB channel mapped to the TVChannel
            string providerName;
            if (TVDatabase.IsMapped(existingTvChannel,
                                    Network() == NetworkType.ATSC,
                                    Network() == NetworkType.DVBT,
                                    Network() == NetworkType.DVBC,
                                    Network() == NetworkType.DVBS,
                                    out providerName))
            {
              //channel is already mapped
              //check if provider differs
              if (String.Compare(providerName, newchannel.ServiceProvider, true) != 0)
              {
                if (providerName.Length > 0 && newchannel.ServiceProvider.Length > 0)
                {
                  //different provider. change Tv channel name to include the provider as well
                  isNewChannel = true;
                  channelId = -1;
                  newchannel.ServiceName = String.Format("{0}-{1}", newchannel.ServiceName, newchannel.ServiceProvider);
                  existingTvChannel = new TVChannel();
                  existingTvChannel.Name = newchannel.ServiceName;
                  //check if there is a TV channel with the name: servicename-providername
                  foreach (TVChannel tvchan in tvChannels)
                  {
                    if (String.Compare(tvchan.Name, newchannel.ServiceName, true) == 0)
                    {
                      //yes TV channel with this name exists in the database...
                      existingTvChannel = tvchan;
                      isNewChannel = false;
                      channelId = tvchan.ID;
                      break;
                    }
                  }
                }
              }
              else
              {
                //channel is already mapped 
                //and provider name is the same
              }
            }
            else
            {
              //channel is not mapped yet so we can map the dvb channel to the TV channel
            }
          }
          else
          {
            //channel does not exists in tv database yet
          }
          existingTvChannel.Scrambled = newchannel.IsScrambled;
          if (isNewChannel)
          {
            //then add a new channel to the database
            existingTvChannel.ID = -1;
            existingTvChannel.Number = TVDatabase.FindFreeTvChannelNumber(newchannel.ProgramNumber);
            existingTvChannel.Sort = 40000;
            Log.Info("DVBGraph: add new channel for {0}:{1}:{2}", existingTvChannel.Name, existingTvChannel.Number,
                     existingTvChannel.Sort);
            int id = TVDatabase.AddChannel(existingTvChannel);
            if (id < 0)
            {
              Log.Error("DVBGraph: failed to add new channel for {0}:{1}:{2} to database", existingTvChannel.Name,
                        existingTvChannel.Number, existingTvChannel.Sort);
            }
            channelId = id;
            newChannels++;
          }
          else
          {
            TVDatabase.UpdateChannel(existingTvChannel, existingTvChannel.Sort);
            updatedChannels++;
            Log.Info("DVBGraph: update channel {0}:{1}:{2} {3}", existingTvChannel.Name, existingTvChannel.Number,
                     existingTvChannel.Sort, existingTvChannel.ID);
          }
          if (Network() == NetworkType.DVBT)
          {
            Log.Info("DVBGraph: map channel {0} id:{1} to DVBT card:{2}", newchannel.ServiceName, channelId, ID);
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
                                      newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid,
                                      newchannel.PCRPid,
                                      newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2,
                                      newchannel.AudioLanguage3,
                                      newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);
          }
          if (Network() == NetworkType.DVBC)
          {
            Log.Info("DVBGraph: map channel {0} id:{1} to DVBC card:{2}", newchannel.ServiceName, channelId, ID);
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
                                      newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid,
                                      newchannel.PCRPid,
                                      newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2,
                                      newchannel.AudioLanguage3,
                                      newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);
          }
          if (Network() == NetworkType.ATSC)
          {
            Log.Info("DVBGraph: map channel {0} id:{1} to ATSC card:{2}", newchannel.ServiceName, channelId, ID);
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
                                      newchannel.Audio1, newchannel.Audio2, newchannel.Audio3, newchannel.AC3Pid,
                                      newchannel.PCRPid,
                                      newchannel.AudioLanguage, newchannel.AudioLanguage1, newchannel.AudioLanguage2,
                                      newchannel.AudioLanguage3,
                                      newchannel.HasEITPresentFollow, newchannel.HasEITSchedule);
          }
          if (Network() == NetworkType.DVBS)
          {
            Log.Info("DVBGraph: map channel {0} id:{1} to DVBS card:{2}", newchannel.ServiceName, channelId, ID);
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
          tvTmp.Number = existingTvChannel.Number;
          tvTmp.ID = channelId;
          TVDatabase.MapChannelToGroup(group, tvTmp);
          //make group for service provider
          group = new TVGroup();
          group.GroupName = newchannel.ServiceProvider;
          groupid = TVDatabase.AddGroup(group);
          group.ID = groupid;
          tvTmp = new TVChannel();
          tvTmp.Name = newchannel.ServiceName;
          tvTmp.Number = existingTvChannel.Number;
          tvTmp.ID = channelId;
          TVDatabase.MapChannelToGroup(group, tvTmp);
        }
        else if (info.serviceType == Mpeg2AudioServiceType) //radio
        {
          //Log.WriteFile(LogType.Log,"DVBGraph: channel {0} is a radio channel",newchannel.ServiceName);
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
            Log.Info("DVBGraph: add new radio channel for {0} {1}", station.Name, station.Frequency);
            int id = RadioDatabase.AddStation(ref station);
            if (id < 0)
            {
              Log.Error("DVBGraph: failed to add new radio channel for {0} {1} to database", station.Name,
                        station.Frequency);
            }
            channelId = id;
            newRadioChannels++;
          }
          else
          {
            updatedRadioChannels++;
            Log.Info("DVBGraph: channel {0} already exists in tv database", newchannel.ServiceName);
          }
          if (Network() == NetworkType.DVBT)
          {
            Log.Info("DVBGraph: map radio channel {0} id:{1} to DVBT card:{2}", newchannel.ServiceName, channelId, ID);
            RadioDatabase.MapDVBTChannel(newchannel.ServiceName, newchannel.ServiceProvider, channelId,
                                         newchannel.Frequency, newchannel.NetworkID, newchannel.TransportStreamID,
                                         newchannel.ProgramNumber, _currentTuningObject.AudioPid, newchannel.PMTPid,
                                         newchannel.Bandwidth, newchannel.PCRPid);
          }
          if (Network() == NetworkType.DVBC)
          {
            Log.Info("DVBGraph: map radio channel {0} id:{1} to DVBC card:{2}", newchannel.ServiceName, channelId, ID);
            RadioDatabase.MapDVBCChannel(newchannel.ServiceName, newchannel.ServiceProvider, channelId,
                                         newchannel.Frequency, newchannel.Symbolrate, newchannel.FEC,
                                         newchannel.Modulation, newchannel.NetworkID, newchannel.TransportStreamID,
                                         newchannel.ProgramNumber, _currentTuningObject.AudioPid, newchannel.PMTPid,
                                         newchannel.PCRPid);
          }
          if (Network() == NetworkType.ATSC)
          {
            Log.Info("DVBGraph: map radio channel {0} id:{1} to DVBC card:{2}", newchannel.ServiceName, channelId, ID);
            RadioDatabase.MapATSCChannel(newchannel.ServiceName, newchannel.PhysicalChannel,
                                         newchannel.MinorChannel,
                                         newchannel.MajorChannel, newchannel.ServiceProvider, channelId,
                                         newchannel.Frequency, newchannel.Symbolrate, newchannel.FEC,
                                         newchannel.Modulation, newchannel.NetworkID, newchannel.TransportStreamID,
                                         newchannel.ProgramNumber, _currentTuningObject.AudioPid, newchannel.PMTPid,
                                         newchannel.PCRPid);
          }
          if (Network() == NetworkType.DVBS)
          {
            Log.Info("DVBGraph: map radio channel {0} id:{1} to DVBS card:{2}", newchannel.ServiceName, channelId, ID);
            newchannel.ID = channelId;
            int scrambled = 0;
            if (newchannel.IsScrambled)
            {
              scrambled = 1;
            }
            RadioDatabase.MapDVBSChannel(newchannel.ID, newchannel.Frequency, newchannel.Symbolrate,
                                         newchannel.FEC, newchannel.LnbSwitchFrequency, newchannel.DiSEqC,
                                         newchannel.ProgramNumber,
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
      } //for (int i=0; i < transp.channels.Count;++i)
      SetLCN();
    } //public void StoreChannels(bool radio, bool tv)

    protected void SetLCN()
    {
      //Log.Info("SetLCN");
      Int16 count = 0;
      while (true)
      {
        string provider;
        Int16 networkId, transportId, serviceID, LCN;
        _analyzerInterface.GetLCN(count, out networkId, out transportId, out serviceID, out LCN);
        if (networkId > 0 && transportId > 0 && serviceID > 0)
        {
          TVChannel channel = TVDatabase.GetTVChannelByStream(Network() == NetworkType.ATSC,
                                                              Network() == NetworkType.DVBT,
                                                              Network() == NetworkType.DVBC,
                                                              Network() == NetworkType.DVBS, networkId, transportId,
                                                              serviceID, out provider);
          if (channel != null)
          {
            channel.Sort = LCN;
            //           Log.Info("lcn:{0} network:0x{1:X} transportid:0x{2:X} serviceid:0x{3:X} {4}",LCN , networkId, transportId, serviceID, channel.Name);
            TVDatabase.UpdateChannel(channel, channel.Sort);
          }
          else
          {
            RadioStation station = RadioDatabase.GetStationByStream(Network() == NetworkType.ATSC,
                                                                    Network() == NetworkType.DVBT,
                                                                    Network() == NetworkType.DVBC,
                                                                    Network() == NetworkType.DVBS, networkId,
                                                                    transportId, serviceID, out provider);
            if (station != null)
            {
              station.Sort = LCN;
              RadioDatabase.UpdateStation(station);
            }
            //            Log.Info("unknown channel lcn:{0} network:0x{1:X} transportid:0x{2:X} serviceid:0x{3:X}",LCN, networkId, transportId, serviceID);
          }
        }
        else
        {
          //          Log.Info("LCN total:{0}", count);
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
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneChannel() get ATSC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0, physicalChannel = 0;
            int minorChannel = 0, majorChannel = 0;
            RadioDatabase.GetATSCTuneRequest(channel.ID, out physicalChannel, out minorChannel, out majorChannel,
                                             out providerName, out frequency, out symbolrate, out innerFec,
                                             out modulation, out ONID, out TSID, out SID, out audioPid, out pmtPid,
                                             out pcrPid);
            if (physicalChannel < 0)
            {
              Log.Error("DVBGraph:database invalid tuning details for station:{0}", channel.ID);
              return;
            }
            Log.Info(
              "DVBGraph:  tuning details: frequency:{0} kHz physicalChannel:{1} symbolrate:{2} innerFec:{3} modulation:{4} ONID:{5} TSID:{6} SID:{7} provider:{8}",
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
          }
          break;

        case NetworkType.DVBC:
          {
            //get the DVB-C tuning details from the tv database
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneRadioChannel() get DVBC tuning details");
            int symbolrate = 0, innerFec = 0, modulation = 0;
            RadioDatabase.GetDVBCTuneRequest(channel.ID, out providerName, out frequency, out symbolrate, out innerFec,
                                             out modulation, out ONID, out TSID, out SID, out audioPid, out pmtPid,
                                             out pcrPid);
            if (frequency <= 0)
            {
              Log.Error("DVBGraph:database invalid tuning details for channel:{0}", channel.Channel);
              return;
            }
            Log.Info(
              "DVBGraph:  tuning details: frequency:{0} kHz symbolrate:{1} innerFec:{2} modulation:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
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
          }
          break;

        case NetworkType.DVBS:
          {
            //get the DVB-S tuning details from the tv database
            //for DVB-S this is the frequency, polarisation, symbolrate,lnb-config, diseqc-config
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneRadioChannel() get DVBS tuning details");
            DVBChannel ch = new DVBChannel();
            if (RadioDatabase.GetDVBSTuneRequest(channel.ID, 0, ref ch) == false) //only radio
            {
              Log.Error("DVBGraph:database invalid tuning details for channel:{0}", channel.Channel);
              return;
            }
            Log.Info(
              "DVBGraph:  tuning details: frequency:{0} kHz polarisation:{1} innerFec:{2} symbolrate:{3} ONID:{4} TSID:{5} SID:{6} provider:{7}",
              ch.Frequency, ch.Polarity, ch.FEC, ch.Symbolrate, ch.NetworkID, ch.TransportStreamID, ch.ProgramNumber,
              ch.ServiceProvider);
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
            _currentTuningObject.LnbSwitchFrequency = ch.LnbSwitchFrequency;
            _currentTuningObject.HasEITPresentFollow = ch.HasEITPresentFollow;
            _currentTuningObject.HasEITSchedule = ch.HasEITSchedule;
          }
          break;

        case NetworkType.DVBT:
          {
            //get the DVB-T tuning details from the tv database
            //for DVB-T this is the frequency, ONID , TSID and SID
            //Log.WriteFile(LogType.Log,"DVBGraph:TuneRadioChannel() get DVBT tuning details");
            RadioDatabase.GetDVBTTuneRequest(channel.ID, out providerName, out frequency, out ONID, out TSID, out SID,
                                             out audioPid, out pmtPid, out bandwidth, out pcrPid);
            if (frequency <= 0)
            {
              Log.Error("DVBGraph:database invalid tuning details for channel:{0}", channel.Channel);
              return;
            }
            Log.Info("DVBGraph:  tuning details: frequency:{0} kHz ONID:{1} TSID:{2} SID:{3} provider:{4}", frequency,
                     ONID, TSID, SID, providerName);
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
          }
          break;
      } //switch (_networkType)
      //submit tune request to the tuner
    }

    public void TuneRadioChannel(RadioStation channel)
    {
      try
      {
        _currentChannelNumber = channel.Channel;
        _startTimer = DateTime.Now;
        Log.Info("DVBGraph:TuneRadioChannel() tune to radio station:{0}", channel.Name);
        GetRadioChannelFromDatabase(channel);
        if (_currentTuningObject == null)
        {
          return;
        }
        SubmitTuneRequest(_currentTuningObject);
        Log.Info("DVBGraph:TuneRadioChannel() done");
        if (_streamDemuxer != null)
        {
          _streamDemuxer.OnTuneNewChannel();
          _streamDemuxer.SetChannelData(_currentTuningObject.AudioPid, _currentTuningObject.VideoPid,
                                        _currentTuningObject.AC3Pid, _currentTuningObject.TeletextPid,
                                        _currentTuningObject.Audio3, _currentTuningObject.ServiceName,
                                        _currentTuningObject.PMTPid, _currentTuningObject.ProgramNumber);
        }
        //SendPMT();
        _refreshPmtTable = true;
      }
      finally
      {
        //_signalLostTimer = DateTime.Now;
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
      catch (Exception)
      {
      }
    } //public void TuneRadioChannel(AnalogVideoStandard standard,int iChannel,int country)

    public void StartRadio(RadioStation station)
    {
      if (_graphState != State.Radio)
      {
        if (_graphState != State.Created)
        {
          return;
        }
        if (_vmr9 != null)
        {
          _vmr9.Dispose();
          _vmr9 = null;
        }
        Log.Info("DVBGraph:StartRadio()");
        // add the preferred video/audio codecs
        AddPreferredCodecs(true, false);
        GetRadioChannelFromDatabase(station);
        if (_currentTuningObject == null)
        {
          return;
        }
        if (_currentTuningObject.PCRPid <= 0 || _currentTuningObject.PCRPid >= 0x1fff)
        {
          Log.Info("DVBGraph: map pid 0x:{0:X} to mpg1 pin pcr:0x{1:X}", _currentTuningObject.AudioPid,
                   _currentTuningObject.PCRPid);
          SetupDemuxer(_pinDemuxerVideo, 0, _pinDemuxerAudio, 0, _pinAC3Out, 0);
          SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.AudioPid, (int) MediaSampleContent.TransportPayload, true);
          SetupDemuxerPin(_pinMPG1Out, _currentTuningObject.PCRPid, (int) MediaSampleContent.TransportPacket, false);
          //setup demuxer MTS pin
          Log.Info("DVBGraph: render mpg1 pin");
          if (_graphBuilder.Render(_pinMPG1Out /*_pinDemuxerAudio*/) != 0)
          {
            Log.Error("DVBGraph:Failed to render audio out pin MPEG-2 Demultiplexer");
            return;
          }
        }
        else
        {
          Log.Info("DVBGraph: render audio pin");
          if (_graphBuilder.Render(_pinDemuxerAudio) != 0)
          {
            Log.Error("DVBGraph:Failed to render audio out pin MPEG-2 Demultiplexer");
            return;
          }
        }
        //get the IMediaControl interface of the graph
        if (_mediaControl == null)
        {
          _mediaControl = _graphBuilder as IMediaControl;
        }
        //start the graph
        //Log.WriteFile(LogType.Log,"DVBGraph: start graph");
        if (_mediaControl != null)
        {
          int hr = _mediaControl.Run();
          if (hr < 0)
          {
            Log.Error("DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
            return;
          }
        }
        else
        {
          Log.Error("DVBGraph: FAILED cannot get IMediaControl");
        }
        TuneRadioChannel(station);
        _isGraphRunning = true;
        _graphState = State.Radio;
        Log.Info("DVBGraph:Listening to radio..");
        return;
      }
      // tune to the correct channel
      TuneRadioChannel(station);
      Log.Info("DVBGraph:Listening to radio..");
    }

    public void TuneRadioFrequency(int frequency)
    {
    }

    #endregion

    #region demuxer callbacks

    protected bool m_streamDemuxer_AudioHasChanged(DVBDemuxer.AudioHeader audioFormat)
    {
      return false;
    }

    protected bool m_streamDemuxer_OnAudioFormatChanged(DVBDemuxer.AudioHeader audioFormat)
    {
/*
			Log.Info("DVBGraph:Audio format changed");
			Log.Info("DVBGraph:  Bitrate:{0}",audioFormat.Bitrate);
			Log.Info("DVBGraph:  Layer:{0}",audioFormat.Layer);
			Log.Info("DVBGraph:  SamplingFreq:{0}",audioFormat.SamplingFreq);
			Log.Info("DVBGraph:  Channel:{0}",audioFormat.Channel);
			Log.Info("DVBGraph:  Bound:{0}",audioFormat.Bound);
			Log.Info("DVBGraph:  Copyright:{0}",audioFormat.Copyright);
			Log.Info("DVBGraph:  Emphasis:{0}",audioFormat.Emphasis);
			Log.Info("DVBGraph:  ID:{0}",audioFormat.ID);
			Log.Info("DVBGraph:  Mode:{0}",audioFormat.Mode);
			Log.Info("DVBGraph:  ModeExtension:{0}",audioFormat.ModeExtension);
			Log.Info("DVBGraph:  Original:{0}",audioFormat.Original);
			Log.Info("DVBGraph:  PaddingBit:{0}",audioFormat.PaddingBit);
			Log.Info("DVBGraph:  PrivateBit:{0}",audioFormat.PrivateBit);
			Log.Info("DVBGraph:  ProtectionBit:{0}",audioFormat.ProtectionBit);
			Log.Info("DVBGraph:  TimeLength:{0}",audioFormat.TimeLength);*/
      return true;
    }

    protected void m_streamDemuxer_OnPMTIsChanged(byte[] pmtTable)
    {
      if (_graphState == State.None || _graphState == State.Created)
      {
        return;
      }
      if (pmtTable == null)
      {
        return;
      }
      if (pmtTable.Length < 6)
      {
        return;
      }
      if (_currentTuningObject.NetworkID < 0 ||
          _currentTuningObject.TransportStreamID < 0 ||
          _currentTuningObject.ProgramNumber < 0)
      {
        return;
      }
      try
      {
        string pmtName = Config.GetFile(Config.Dir.Database, @"pmt\", String.Format("pmt_{0}_{1}_{2}_{3}_{4}.dat",
                                                                                    Util.Utils.FilterFileName(
                                                                                      _currentTuningObject.ServiceName),
                                                                                    _currentTuningObject.NetworkID,
                                                                                    _currentTuningObject.
                                                                                      TransportStreamID,
                                                                                    _currentTuningObject.ProgramNumber,
                                                                                    (int) Network()));
#if COMPARE_PMT
        bool isSame = false;
        if (File.Exists(pmtName))
        {
          byte[] pmt = null;
          using (FileStream stream = new FileStream(pmtName, FileMode.Open, FileAccess.Read, FileShare.None))
          {
            long len = stream.Length;
            if (len > 6)
            {
              pmt = new byte[len];
              stream.Read(pmt, 0, (int) len);
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
          Log.Info("DVBGraph: OnPMTIsChanged:{0}", pmtName);
          using (FileStream stream = new FileStream(pmtName, FileMode.Create, FileAccess.Write, FileShare.None))
          {
            stream.Write(pmtTable, 0, pmtTable.Length);
            stream.Close();
          }
          SendPMT();
        }
        if (Recorder.IsCardViewing(_cardId) || _graphState == State.Epg || _graphState == State.Radio)
        {
          Log.Info("DVBGraph: grab epg for {0}", _currentTuningObject.ServiceName);
          _epgGrabber.GrabEPG(_currentTuningObject.ServiceName, _currentTuningObject.HasEITSchedule == true);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    #endregion

    protected void AddHardwarePid(int pid, ArrayList pids)
    {
      if (pid <= 0)
      {
        return;
      }
      foreach (ushort existingPid in pids)
      {
        if (existingPid == (ushort) pid)
        {
          return;
        }
      }
      pids.Add((ushort) pid);
    }

    protected void SetHardwarePidFiltering()
    {
      string pidsText = string.Empty;
      ArrayList pids = new ArrayList();
      if (_inScanningMode == false)
      {
        if (Network() == NetworkType.ATSC)
        {
          pids.Add((ushort) 0x1ffb);
        }
        else
        {
          pids.Add((ushort) 0);
          pids.Add((ushort) 1);
          pids.Add((ushort) 0x10);
          pids.Add((ushort) 0x11);
          pids.Add((ushort) 0x12);
          pids.Add((ushort) 0xd3);
          pids.Add((ushort) 0xd2);
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
          ushort pid = (ushort) pids[i];
          pidsText += String.Format("{0:X},", pid);
          switch ((int) pid)
          {
            case 0x12:
              break;
            case 0xd2:
              break;
            case 0xd3:
              break;
            default:
              if (_pinDemuxerSections != null)
              {
                SetupDemuxerPin(_pinDemuxerSections, (int) pid, (int) MediaSampleContent.Mpeg2PSI, (i == 0));
              }
              break;
          }
          //Log.Info("nr:{0} pid:0x{1:X}, hr:{2:X}", i, (int)pid, hr);
        }
        Log.Info("DVBGraph:SetHardwarePidFiltering to:{0}", pidsText);
      }
      SendHWPids(pids);
      //      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
    }

    protected virtual void SendHWPids(ArrayList pids)
    {
    }

    public string TvTimeshiftFileName()
    {
      return "live.tv";
    }

    public string RadioTimeshiftFileName()
    {
      return string.Empty;
    }

    public void GrabTeletext(bool yesNo)
    {
      if (_graphState == State.None || _graphState == State.Created)
      {
        return;
      }
      if (_streamDemuxer == null)
      {
        return;
      }
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
      if (_graphState == State.Epg && _isGraphRunning == false)
      {
        return true;
      }
      return false;
    }

    public bool IsEpgGrabbing()
    {
      return (_graphState == State.Epg);
    }

    public bool GrabEpg(TVChannel channel)
    {
      if (_graphState != State.Created)
      {
        return false;
      }
      // tune to the correct channel
      Log.Info("DVBGraph:Grab epg for :{0}", channel.Name);
      //now start the graph
      Log.Info("DVBGraph: start graph");
      if (_mediaControl == null)
      {
        _mediaControl = (IMediaControl) _graphBuilder;
      }
      int hr = _mediaControl.Run();
      if (hr < 0)
      {
        Log.Error("DVBGraph: FAILED unable to start graph :0x{0:X}", hr);
        return false;
      }
      TuneChannel(channel);
      if (_currentTuningObject == null)
      {
        _mediaControl.Stop();
        _isGraphRunning = false;
        _graphState = State.Created;
        return false;
      }
      else
      {
        _isGraphRunning = true;
        _graphState = State.Epg;
      }
      return true;
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
        Log.Info("FilterPids:{0}", count);
        string pidsText = string.Empty;
        _scanPidList = new ArrayList();
        for (int i = 0; i < count; ++i)
        {
          ushort pid = (ushort) Marshal.ReadInt32(pids, i*4);
          _scanPidList.Add(pid);
          pidsText += String.Format("{0:X},", pid);
        }
        //Log.Info("DVBGraph:analyzer pids to:{0}", pidsText);
        _scanPidListReady = true;
      }
      return 0;
    }

    public void StopRadio()
    {
      if (_graphState != State.Radio)
      {
        return;
      }
      if (_mediaControl != null)
      {
        _mediaControl.Stop();
      }
      if (_pinDemuxerTS != null)
      {
        DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerTS);
      }
      if (_pinDemuxerVideoMPEG4 != null)
      {
        DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideoMPEG4);
      }
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinAC3Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinMPG1Out);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerAudio);
      DirectShowUtil.RemoveDownStreamFilters(_graphBuilder, _pinDemuxerVideo);
      _isGraphRunning = false;
      _graphState = State.Created;
    }

    public bool StopEpgGrabbing()
    {
      if (_graphState != State.Epg)
      {
        return true;
      }
      if (_mediaControl != null)
      {
        _mediaControl.Stop();
      }
      _isGraphRunning = false;
      _graphState = State.Created;
      return true;
    }

    public virtual bool SupportsHardwarePidFiltering()
    {
      if (_cardProperties == null)
      {
        return false;
      }
      return _cardProperties.SupportsHardwarePidFiltering;
    }

    public virtual bool Supports5vAntennae()
    {
      if (_cardProperties == null)
      {
        return false;
      }
      return _cardProperties.Supports5vAntennae;
    }

    public bool SupportsCamSelection()
    {
      if (_cardProperties == null)
      {
        return false;
      }
      return _cardProperties.SupportsCamSelection;
    }

    public bool CanViewTimeShiftFile()
    {
      if (_graphState != State.TimeShifting && _graphState != State.Recording)
      {
        return false;
      }
      if (!SignalPresent())
      {
        return false;
      }
      return true;
    }

    public bool IsRadio()
    {
      return (_graphState == State.Radio);
    }

    public bool IsRecording()
    {
      return (_graphState == State.Recording);
    }

    public string LastError()
    {
      return _lastError;
    }

    #region TsTimeShifting

    private bool CreateTsTimeShifting(string fileName, bool useAc3)
    {
      _currentTimeShiftFileName = fileName;
      Log.Info("DVBGraph:add TsFileSink");
      //delete any old timeshifting files
      string file = Path.GetFileName(fileName);
      string path = fileName.Substring(0, fileName.Length - (file.Length + 1));
      string[] files = Directory.GetFiles(path);
      for (int i = 0; i < files.Length; ++i)
      {
        if (files[i].IndexOf(file) >= 0)
        {
          Log.Info("DVBGraph:delete old file {0}", files[i]);
          Util.Utils.FileDelete(files[i]);
        }
      }
      //create new MpgMux filter
      MpgMux mux = new MpgMux();
      IBaseFilter filterMux = (IBaseFilter) mux;
      int hr = _graphBuilder.AddFilter(filterMux, "MpgMux");
      if (hr != 0)
      {
        Log.Error("DVBGraph:FAILED cannot add MpgMux:{0:X}", hr);
        return false;
      }

      //connect mpeg-2 demuxer->MpgMux
      IPin pinMuxVideo = DsFindPin.ByDirection(filterMux, PinDirection.Input, 0);
      IPin pinMuxAudio = DsFindPin.ByDirection(filterMux, PinDirection.Input, 1);
      if (!useAc3)
      {
        _graphBuilder.Connect(_pinDemuxerAudio, pinMuxAudio);
        if (hr != 0)
        {
          Log.Error("DVBGraph:FAILED cannot connect audio mpeg2 demux->mux input 1:{0:X}", hr);
          return false;
        }
      }
      else
      {
        _graphBuilder.Connect(_pinAC3Out, pinMuxAudio);
        if (hr != 0)
        {
          Log.Error("DVBGraph:FAILED cannot connect ac3 mpeg2 demux->mux input 1:{0:X}", hr);
          return false;
        }
      }
      if (_currentTuningObject.ServiceType == Mpeg4VideoServiceType)
      {
        hr = _graphBuilder.Connect(_pinDemuxerVideoMPEG4, pinMuxVideo);
        if (hr != 0)
        {
          Log.Error("DVBGraph:FAILED cannot connect video mpeg4 demux->mux input 2:{0:X}", hr);
          return false;
        }
      }
      else
      {
        hr = _graphBuilder.Connect(_pinDemuxerVideo, pinMuxVideo);
        if (hr != 0)
        {
          Log.Error("DVBGraph:FAILED cannot connect video mpeg2 demux->mux input 2:{0:X}", hr);
          return false;
        }
      }
      //create new TsFileSink filter
      _filterTsFileSink = new TsFileSink();
      hr = _graphBuilder.AddFilter((IBaseFilter) _filterTsFileSink, "TsFileSink");
      if (hr != 0)
      {
        Log.Error("DVBGraph:FAILED cannot add TsFileSink:{0:X}", hr);
        return false;
      }
      IFileSinkFilter interfaceFile = _filterTsFileSink as IFileSinkFilter;
      if (interfaceFile == null)
      {
        Log.Error("DVBGraph:FAILED cannot get IFileSinkFilter from TsFileSink");
        return false;
      }
      //set filename
      AMMediaType mt = new AMMediaType();
      interfaceFile.SetFileName(fileName, mt);
      //connect MpgMux -> TsFileSink
      IBaseFilter baseFilterSink = _filterTsFileSink as IBaseFilter;
      ConnectFilters(ref filterMux, ref baseFilterSink, 0);
      /*
      //connect mpeg2 demux->TsFileSink
      IPin pinIn = DsFindPin.ByDirection((IBaseFilter)_filterTsFileSink, PinDirection.Input, 0);
      if (pinIn == null)
      {
        Log.Error("DVBGraph:FAILED cannot find input pin of TsFileSink");
        return false;
      }
      hr = _graphBuilder.Connect(_pinDemuxerTS, pinIn);
      if (hr != 0)
      {
        Log.Error("DVBGraph:FAILED cannot connect demuxer->TsFileSink:{0:X}", hr);
        return false;
      }*/
      return true;
    }

    private void SetupTsDemuxerMapping()
    {
      /*
      if (UseTsTimeShifting == false) return;
      if (Network() == NetworkType.ATSC)
      {
        SetupDemuxerPin(_pinDemuxerTS, 0x1ffb, (int)MediaSampleContent.TransportPacket, true);
        SetupDemuxerPin(_pinDemuxerTS, 0x0, (int)MediaSampleContent.TransportPacket, false);
        SetupDemuxerPin(_pinDemuxerTS, 0x10, (int)MediaSampleContent.TransportPacket, false);
        SetupDemuxerPin(_pinDemuxerTS, 0x11, (int)MediaSampleContent.TransportPacket, false);
      }
      else
      {
        SetupDemuxerPin(_pinDemuxerTS, 0x0, (int)MediaSampleContent.TransportPacket, true);
        SetupDemuxerPin(_pinDemuxerTS, 0x10, (int)MediaSampleContent.TransportPacket, false);
        SetupDemuxerPin(_pinDemuxerTS, 0x11, (int)MediaSampleContent.TransportPacket, false);
      }
      if (_currentTuningObject.VideoPid > 0 && _currentTuningObject.VideoPid < 0x1fff)
        SetupDemuxerPin(_pinDemuxerTS, _currentTuningObject.VideoPid, (int)MediaSampleContent.TransportPacket, false);

      if (_currentTuningObject.Audio1 > 0 && _currentTuningObject.Audio1 < 0x1fff)
        SetupDemuxerPin(_pinDemuxerTS, _currentTuningObject.Audio1, (int)MediaSampleContent.TransportPacket, false);

      if (_currentTuningObject.Audio2 > 0 && _currentTuningObject.Audio2 < 0x1fff)
        SetupDemuxerPin(_pinDemuxerTS, _currentTuningObject.Audio2, (int)MediaSampleContent.TransportPacket, false);

      if (_currentTuningObject.Audio3 > 0 && _currentTuningObject.Audio3 < 0x1fff)
        if (_currentTuningObject.Audio3 > 0) SetupDemuxerPin(_pinDemuxerTS, _currentTuningObject.Audio3, (int)MediaSampleContent.TransportPacket, false);

      if (_currentTuningObject.AC3Pid > 0 && _currentTuningObject.AC3Pid < 0x1fff)
        SetupDemuxerPin(_pinDemuxerTS, _currentTuningObject.AC3Pid, (int)MediaSampleContent.TransportPacket, false);

      if (_currentTuningObject.PCRPid > 0 && _currentTuningObject.PCRPid < 0x1fff)
        SetupDemuxerPin(_pinDemuxerTS, _currentTuningObject.PCRPid, (int)MediaSampleContent.TransportPacket, false);

      if (_currentTuningObject.PMTPid > 0 && _currentTuningObject.PMTPid < 0x1fff)
        SetupDemuxerPin(_pinDemuxerTS, _currentTuningObject.PMTPid, (int)MediaSampleContent.TransportPacket, false);

      DumpMpeg2DemuxerMappings(_filterMpeg2Demultiplexer);
       */
    }

    #endregion

    public VideoCaptureProperties CardProperties
    {
      get { return _cardProperties; }
    }
  }
}

#endif