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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DirectShowLib;
using DirectShowLib.Dvd;
using DShowNET;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player.MediaInfo;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.Player.PostProcessing;
using Action = MediaPortal.GUI.Library.Action;

//using DirectX.Capture;

// OK: 720x576 Windowed Mode, 
//     Use PixelRatio correction = no
//     ar correction = stretched,
//     display mode  = default
//     s             = 'normal mode'
//
// 
//

namespace MediaPortal.Player
{
  /// <summary>
  /// 
  /// </summary>
  public class DVDPlayer : IPlayer
  {
    private const uint VFW_E_DVD_OPERATION_INHIBITED = 0x80040276;
    private const uint VFW_E_DVD_INVALIDDOMAIN = 0x80040277;
    private const int UOP_FLAG_Play_Title_Or_AtTime = 0x00000001;
    private const int UOP_FLAG_Play_Chapter_Or_AtTime = 0x00000020;

    protected enum PlayState
    {
      Init,
      Menu,
      Playing,
      Paused,
      Stopped
    }

    /// <summary> current state of playback (playing/paused/...) </summary>
    protected PlayState _state;

    /// <summary> asynchronous command interface. </summary>
    protected IDvdCmd _cmdOption = null;

    /// <summary> asynchronous command pending. </summary>
    protected bool _pendingCmd;

    /// <summary> dvd graph builder interface. </summary>
    protected IDvdGraphBuilder _dvdGraph = null;

    /// <summary> dvd control interface. </summary>
    protected IDvdControl2 _dvdCtrl = null;

    /// <summary> dvd information interface. </summary>
    protected IDvdInfo2 _dvdInfo = null;

    protected IBaseFilter _dvdbasefilter = null;

    /// <summary> dvd video playback window interface. </summary>
    protected IVideoWindow _videoWin = null;

    protected IBasicVideo2 _basicVideo = null;
    protected IGraphBuilder _graphBuilder = null;
    protected IAMLine21Decoder _line21Decoder = null;

    protected DvdPreferredDisplayMode _videoPref = DvdPreferredDisplayMode.DisplayContentDefault;
    protected AspectRatioMode arMode = AspectRatioMode.Stretched;

    /// <summary> control interface. </summary>
    protected IMediaControl _mediaCtrl = null;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx _mediaEvt = null;

    /// <summary> interface to single-step video. </summary>
    //protected IVideoFrameStep			  videoStep=null;
    protected int _volume = 100;

    //protected OVTOOLLib.OvMgrClass	m_ovMgr=null;

    protected DvdHMSFTimeCode _currTime; // copy of current playback states, see OnDvdEvent()
    protected int _currTitle = 0;
    protected int _currChapter = 0;
    protected DvdDomain _currDomain;
    protected IBasicAudio _basicAudio = null;
    protected IMediaPosition _mediaPos = null;

    protected int _speed = 1;
    protected double _currentTime = 0;
    protected bool _visible = true;
    protected bool _started = false;
    protected DsROTEntry _rotEntry = null;

    protected int _positionX = 80;
    protected int _positionY = 400;
    protected int _width = 200;
    protected int _height = 100;
    protected int _videoWidth = 100;
    protected int _videoHeight = 100;
    protected bool _updateNeeded = false;
    protected bool _fullScreen = true;
    private string _defaultAudioLanguage = "";
    private string _defaultSubtitleLanguage = "";
    protected bool _forceSubtitles = true;
    protected bool _showClosedCaptions = false;
    protected bool _freeNavigator = false;
    protected int _UOPs;
    protected int buttonCount = 0;
    protected int focusedButton = 0;
    protected string _currentFile;
    protected double _duration;
    protected Geometry.Type _aspectRatio = Geometry.Type.Normal;
    protected Point pt;

    protected const int WM_DVD_EVENT = 0x00008002; // message from dvd graph
    protected const int WS_CHILD = 0x40000000; // attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;
    protected const int WM_MOUSEMOVE = 0x0200;
    protected const int WM_LBUTTONUP = 0x0202;

    protected bool _cyberlinkDVDNavigator = false;

    protected ArrayList _mouseMsg;

    private readonly Dictionary<DvdAudioFormat, string> _dvdAudioFormat = new Dictionary<DvdAudioFormat, string>
    {
        { DvdAudioFormat.AC3, "AC-3" },
        { DvdAudioFormat.DTS, "DTS" },
        { DvdAudioFormat.LPCM, "LPCM" },
        { DvdAudioFormat.MPEG1, "MPEG Audio" },
        { DvdAudioFormat.MPEG1_DRC, "MPEG Audio" },
        { DvdAudioFormat.MPEG2, "MPEG Audio" },
        { DvdAudioFormat.MPEG2_DRC, "MPEG Audio" },
        { DvdAudioFormat.Other, "" },
        { DvdAudioFormat.SDDS, "SDDS" },
    };

    private readonly Dictionary<DvdAudioFormat, AudioCodec> _dvdAudioCodec = new Dictionary<DvdAudioFormat, AudioCodec>
    {
        { DvdAudioFormat.AC3, AudioCodec.A_AC3 },
        { DvdAudioFormat.DTS, AudioCodec.A_DTS },
        { DvdAudioFormat.LPCM, AudioCodec.A_PCM_INT_BIG },
        { DvdAudioFormat.MPEG1, AudioCodec.A_MPEG_L1 },
        { DvdAudioFormat.MPEG1_DRC, AudioCodec.A_MPEG_L1 },
        { DvdAudioFormat.MPEG2, AudioCodec.A_MPEG_L2 },
        { DvdAudioFormat.MPEG2_DRC, AudioCodec.A_MPEG_L2 },
        { DvdAudioFormat.Other, AudioCodec.A_UNDEFINED },
        { DvdAudioFormat.SDDS, AudioCodec.A_UNDEFINED },
    };

    public DVDPlayer()
    {
    }

    public override void WndProc(ref Message m)
    {
      try
      {
        if (m.Msg == WM_DVD_EVENT)
        {
          if (_mediaEvt != null)
          {
            OnDvdEvent();
          }
          return;
        }

        if (m.Msg == WM_MOUSEMOVE)
        {
          if (_state == PlayState.Menu && buttonCount > 0)
          {
            _mouseMsg.Add(m);
          }
        }

        if (m.Msg == WM_LBUTTONUP)
        {
          if (_state == PlayState.Menu && buttonCount > 0 && focusedButton > 0)
          {
            _mouseMsg.Add(m);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer:WndProc() {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }

    public override bool Play(string file)
    {
      if (String.IsNullOrEmpty(file))
        return false;

      _currTime = new DvdHMSFTimeCode();
      _UOPs = 0;
      _started = false;
      _visible = false;
      _positionX = 80;
      _positionY = 400;
      _width = 200;
      _height = 100;
      _videoWidth = 100;
      _videoHeight = 100;
      _speed = 1;
      _defaultAudioLanguage = null;
      _defaultSubtitleLanguage = null;
      _forceSubtitles = true;
      _aspectRatio = Geometry.Type.Normal;
      _speed = 1;
      _currentTime = 0;
      _visible = true;
      _started = false;
      _rotEntry = null;
      _volume = 100;
      _mouseMsg = new ArrayList();

      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;

      if (!FirstPlayDvd(file))
      {
        return false;
      }

      _fullScreen = true;
      _updateNeeded = true;

      GUIGraphicsContext.IsFullScreenVideo = true;
      /*
      GUIGraphicsContext.DX9Device.Clear( ClearFlags.Target, Color.Black, 1.0f, 0);
      try
      {
        // Show the frame on the primary surface.
        GUIGraphicsContext.DX9Device.Present();
      }
      catch(DeviceLostException)
      {
      }*/
      SetVideoWindow();

      // With static DVD menus the 1st frame might happen before 
      GUIGraphicsContext.IsPlaying = true;
      GUIGraphicsContext.IsPlayingVideo = true;

      return true;
    }

    #region IDisposable Members

    public override void Dispose()
    {
      CloseInterfaces();
    }

    #endregion

    /// <summary> handling the very first start of dvd playback. </summary>
    private bool FirstPlayDvd(string file)
    {
      int hr;

      try
      {
        _pendingCmd = true;
        UpdateMenu();
        CloseInterfaces();
        string path = null;
        _currentFile = file;

        if (Util.VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(file)))
          file = DaemonTools.GetVirtualDrive() + @"\VIDEO_TS\VIDEO_TS.IFO";

        // Cyberlink navigator needs E:\\VIDEO_TS formatted path with double \
        path = file.Replace(@"\\", @"\").Replace(Path.GetFileName(file), "").Replace(@"\", @"\\");

        if (!GetInterfaces(path))
        {
          Log.Error("DVDPlayer:Unable getinterfaces()");
          CloseInterfaces();
          return false;
        }

        using (Settings xmlreader = new MPSettings())
        {
          _defaultAudioLanguage = xmlreader.GetValueAsString("dvdplayer", "audiolanguage", "english");
          _defaultSubtitleLanguage = xmlreader.GetValueAsString("dvdplayer", "subtitlelanguage", "english");
          _forceSubtitles = xmlreader.GetValueAsBool("dvdplayer", "showsubtitles", true);
          _showClosedCaptions = xmlreader.GetValueAsBool("dvdplayer", "showclosedcaptions", false);
        }

        SetDefaultLanguages();

        hr = _mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_DVD_EVENT, IntPtr.Zero);
        if (hr != 0)
        {
          Log.Error("DVDPlayer:Unable to SetNotifyWindow 0x{0:X}", hr);
        }

        if (_videoWin != null)
        {
          if (hr == 0)
          {
            hr = _videoWin.put_Owner(GUIGraphicsContext.ActiveForm);
            if (hr != 0)
            {
              Log.Error("DVDPlayer:Unable to set window owner 0x{0:X}", hr);
            }
          }
          if (hr == 0)
          {
            hr = _videoWin.put_WindowStyle((WindowStyle)((int)WindowStyle.Child +
                                                         (int)WindowStyle.ClipChildren + (int)WindowStyle.ClipSiblings));
            if (hr != 0)
            {
              Log.Error("DVDPlayer:Unable to set window style 0x{0:X}", hr);
            }
          }
        }

        if (hr != 0)
        {
          Log.Error("DVDPlayer:Unable to set options()");
          CloseInterfaces();
          return false;
        }
        if (_basicVideo != null)
        {
          _basicVideo.SetDefaultSourcePosition();
          _basicVideo.SetDefaultDestinationPosition();
        }
        if (_videoWin != null)
        {
          _videoWin.SetWindowPosition(0, 0, GUIGraphicsContext.Width, GUIGraphicsContext.Height);
        }

        hr = _mediaCtrl.Run();
        if (hr < 0 || hr > 1)
        {
          HResult hrdebug = new HResult(hr);
          Log.Info(hrdebug.ToDXString());
          Log.Error("DVDPlayer:Unable to start playing() 0x:{0:X}", hr);
          CloseInterfaces();
          return false;
        }
        //DsUtils.DumpFilters(_graphBuilder);

        DvdDiscSide side;
        int titles, numOfVolumes, volume;
        hr = _dvdInfo.GetDVDVolumeInfo(out numOfVolumes, out volume, out side, out titles);
        if (hr < 0)
        {
          Log.Error("DVDPlayer:Unable to get dvdvolumeinfo 0x{0:X}", hr);
          CloseInterfaces();
          return false; // can't read disc
        }
        else
        {
          if (titles <= 0)
          {
            Log.Error("DVDPlayer:DVD does not contain any titles? {0}", titles);
            //return false;
          }
        }

        if (_videoWin != null)
        {
          hr = _videoWin.put_MessageDrain(GUIGraphicsContext.ActiveForm);
        }

        hr = _dvdCtrl.SelectVideoModePreference(_videoPref);
        DvdVideoAttributes videoAttr;
        hr = _dvdInfo.GetCurrentVideoAttributes(out videoAttr);
        _videoWidth = videoAttr.sourceResolutionX;
        _videoHeight = videoAttr.sourceResolutionY;

        _state = PlayState.Playing;
        // if menu is not opened first then DVD navigator's events are correcting the state
        VMR9Util.g_vmr9.EVRSetDVDMenuState(true); 

        _pendingCmd = false;
        Log.Info("DVDPlayer:Started playing()");
        if (_currentFile == string.Empty)
        {
          for (int i = 0; i <= 26; ++i)
          {
            string dvd = String.Format("{0}:", (char)('A' + i));
            if (Util.Utils.IsDVD(dvd))
            {
              _currentFile = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", (char)('A' + i));
              if (File.Exists(_currentFile))
              {
                break;
              }
            }
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer:Could not start DVD:{0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        CloseInterfaces();
        return false;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected virtual void CloseInterfaces()
    {
      if (_graphBuilder == null)
      {
        return;
      }
      int hr;
      try
      {
        Log.Info("DVDPlayer:cleanup DShow graph");

        if (_mediaCtrl != null)
        {
          hr = _mediaCtrl.Stop();
          _mediaCtrl = null;
        }
        _state = PlayState.Stopped;
        VMR9Util.g_vmr9.EVRSetDVDMenuState(false);

        _mediaEvt = null;
        _visible = false;
        _videoWin = null;
        //				videoStep	= null;
        _dvdCtrl = null;
        _dvdInfo = null;
        _basicVideo = null;
        _basicAudio = null;
        _mediaPos = null;

        if (_dvdbasefilter != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_dvdbasefilter)) > 0)
          {
            ;
          }
          _dvdbasefilter = null;
        }

        if (_cmdOption != null)
        {
          DirectShowUtil.ReleaseComObject(_cmdOption);
        }
        _cmdOption = null;
        _pendingCmd = false;
        if (_line21Decoder != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_line21Decoder)) > 0)
          {
            ;
          }
          _line21Decoder = null;
        }


        if (_graphBuilder != null)
        {
          DirectShowUtil.RemoveFilters(_graphBuilder);
          if (_rotEntry != null)
          {
            _rotEntry.SafeDispose();
            _rotEntry = null;
          }
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0)
          {
            ;
          }
          _graphBuilder = null;
        }

        if (_dvdGraph != null)
        {
          while ((hr = DirectShowUtil.ReleaseComObject(_dvdGraph)) > 0)
          {
            ;
          }
          _dvdGraph = null;
        }
        _state = PlayState.Init;

        GUIGraphicsContext.form.Invalidate(true);
        GUIGraphicsContext.form.Activate();
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer:exception while cleanuping DShow graph {0} {1}", ex.Message, ex.StackTrace);
      }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected virtual bool GetInterfaces(string path)
    {
      int hr;
      //Type	            comtype = null;
      object comobj = null;
      _freeNavigator = true;
      _dvdInfo = null;
      _dvdCtrl = null;

      string dvdNavigator = "";
      string aspectRatioMode = "";
      string displayMode = "";
      bool useAC3Filter = false;
      using (Settings xmlreader = new MPSettings())
      {
        dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
        aspectRatioMode = xmlreader.GetValueAsString("dvdplayer", "armode", "").ToLowerInvariant();
        if (aspectRatioMode == "crop")
        {
          arMode = AspectRatioMode.Crop;
        }
        if (aspectRatioMode == "letterbox")
        {
          arMode = AspectRatioMode.LetterBox;
        }
        if (aspectRatioMode == "stretch")
        {
          arMode = AspectRatioMode.Stretched;
        }
        //if ( aspectRatioMode == "stretch" ) arMode = AspectRatioMode.zoom14to9;
        if (aspectRatioMode == "follow stream")
        {
          arMode = AspectRatioMode.StretchedAsPrimary;
        }
        useAC3Filter = xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
        displayMode = xmlreader.GetValueAsString("dvdplayer", "displaymode", "").ToLowerInvariant();
        if (displayMode == "default")
        {
          _videoPref = DvdPreferredDisplayMode.DisplayContentDefault;
        }
        if (displayMode == "16:9")
        {
          _videoPref = DvdPreferredDisplayMode.Display16x9;
        }
        if (displayMode == "4:3 pan scan")
        {
          _videoPref = DvdPreferredDisplayMode.Display4x3PanScanPreferred;
        }
        if (displayMode == "4:3 letterbox")
        {
          _videoPref = DvdPreferredDisplayMode.Display4x3LetterBoxPreferred;
        }
      }

      try
      {
        _dvdGraph = (IDvdGraphBuilder)new DvdGraphBuilder();

        hr = _dvdGraph.GetFiltergraph(out _graphBuilder);
        if (hr < 0)
        {
          Marshal.ThrowExceptionForHR(hr);
        }
        _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

        try
        {
          _dvdbasefilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, dvdNavigator);
          if (_dvdbasefilter != null)
          {
            IDvdControl2 cntl = (IDvdControl2)_dvdbasefilter;
            if (cntl != null)
            {
              _dvdInfo = (IDvdInfo2)cntl;
              _dvdCtrl = (IDvdControl2)cntl;
              if (path != null)
              {
                if (path.Length != 0)
                {
                  cntl.SetDVDDirectory(path);
                }
              }
              _dvdCtrl.SetOption(DvdOptionFlag.HMSFTimeCodeEvents, true); // use new HMSF timecode format
              _dvdCtrl.SetOption(DvdOptionFlag.ResetOnStop, false);

              AddPreferedCodecs(_graphBuilder);
              DirectShowUtil.RenderOutputPins(_graphBuilder, _dvdbasefilter);

              _videoWin = _graphBuilder as IVideoWindow;
              _freeNavigator = false;
            }

            //DirectShowUtil.ReleaseComObject( _dvdbasefilter); _dvdbasefilter = null;              
          }
        }
        catch (Exception ex)
        {
          string strEx = ex.Message;
        }

        Guid riid;

        if (useAC3Filter)
        {
          string ac3filterMonikerString =
            @"@device:sw:{083863F1-70DE-11D0-BD40-00A0C911CE86}\{A753A1EC-973E-4718-AF8E-A3F554D45C44}";
          Log.Info("DVDPlayer:Adding AC3 filter to graph");
          IBaseFilter filter = Marshal.BindToMoniker(ac3filterMonikerString) as IBaseFilter;
          if (filter != null)
          {
            hr = _graphBuilder.AddFilter(filter, "AC3 Filter");
            if (hr < 0)
            {
              Log.Info("DVDPlayer:FAILED:could not add AC3 filter to graph");
            }
          }
          else
          {
            Log.Info("DVDPlayer:FAILED:AC3 filter not installed");
          }
        }

        if (_dvdInfo == null)
        {
          riid = typeof (IDvdInfo2).GUID;
          hr = _dvdGraph.GetDvdInterface(riid, out comobj);
          if (hr < 0)
          {
            Marshal.ThrowExceptionForHR(hr);
          }
          _dvdInfo = (IDvdInfo2)comobj;
          comobj = null;
        }

        if (_dvdCtrl == null)
        {
          riid = typeof (IDvdControl2).GUID;
          hr = _dvdGraph.GetDvdInterface(riid, out comobj);
          if (hr < 0)
          {
            Marshal.ThrowExceptionForHR(hr);
          }
          _dvdCtrl = (IDvdControl2)comobj;
          comobj = null;
        }

        _mediaCtrl = (IMediaControl)_graphBuilder;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _basicAudio = _graphBuilder as IBasicAudio;
        _mediaPos = (IMediaPosition)_graphBuilder;
        _basicVideo = _graphBuilder as IBasicVideo2;
        _videoWin = _graphBuilder as IVideoWindow;

        // disable Closed Captions!
        IBaseFilter baseFilter;
        _graphBuilder.FindFilterByName("Line 21 Decoder", out baseFilter);
        if (baseFilter == null)
        {
          _graphBuilder.FindFilterByName("Line21 Decoder", out baseFilter);
        }
        if (baseFilter != null)
        {
          _line21Decoder = (IAMLine21Decoder)baseFilter;
          if (_line21Decoder != null)
          {
            AMLine21CCState state = AMLine21CCState.Off;
            hr = _line21Decoder.SetServiceState(state);
            if (hr == 0)
            {
              Log.Info("DVDPlayer:Closed Captions disabled");
            }
            else
            {
              Log.Info("DVDPlayer:failed 2 disable Closed Captions");
            }
          }
        }
        /*
                // get video window
                if (_videoWin==null)
                {
                  riid = typeof( IVideoWindow ).GUID;
                  hr = _dvdGraph.GetDvdInterface( ref riid, out comobj );
                  if( hr < 0 )
                    Marshal.ThrowExceptionForHR( hr );
                  _videoWin = (IVideoWindow) comobj; comobj = null;
                }
          */
        // GetFrameStepInterface();

        DirectShowUtil.SetARMode(_graphBuilder, arMode);
        DirectShowUtil.EnableDeInterlace(_graphBuilder);
        //m_ovMgr = new OVTOOLLib.OvMgrClass();
        //m_ovMgr.SetGraph(_graphBuilder);

        return true;
      }
      catch (Exception)
      {
        //MessageBox.Show( this, "Could not get interfaces\r\n" + ee.Message, "DVDPlayer.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop );
        CloseInterfaces();
        return false;
      }
      finally
      {
        if (comobj != null)
        {
          DirectShowUtil.ReleaseComObject(comobj);
        }
        comobj = null;
      }
    }

    /// <summary> DVD event message handler</summary>
    private void OnDvdEvent()
    {
      //Log.Info("OnDvdEvent()");
      if (_mediaEvt == null)
        return;

      int p1, p2, hr = 0;
      EventCode code;
      try
      {
        do
        {
          hr = _mediaEvt.GetEvent(out code, out p1, out p2, 0);
          if (hr < 0)
          {
            break;
          }

          //Log.Info( "DVDPlayer DVD EVT :" + code.ToString() );

          switch (code)
          {
            case EventCode.DvdPlaybackRateChange:
              if (_speed != p1 / 10000)
              {
                _speed = p1 / 10000; // if RWD reaches start then PlaybackRate is changing automaticly 
              }
              break;

            case EventCode.DvdWarning:
              Log.Debug("DVDPlayer DVD warning :{0}", p1, p2);
              break;

            case EventCode.DvdCurrentHmsfTime:
              byte[] ati = BitConverter.GetBytes(p1);
              if (ati != null)
              {
                _currTime.bHours = ati[0];
                _currTime.bMinutes = ati[1];
                _currTime.bSeconds = ati[2];
                _currTime.bFrames = ati[3];
                _currentTime = new TimeSpan(_currTime.bHours, _currTime.bMinutes, _currTime.bSeconds).TotalSeconds;
              }
              break;

            case EventCode.DvdSubPicictureStreamChange:
              Log.Debug("EVT:DvdSubPicture Changed to:{0} Enabled:{1}", p1, p2);
              break;

            case EventCode.DvdChapterStart:
              Log.Debug("EVT:DvdChaptStart:{0}", p1);
              _currChapter = p1;
              UpdateDuration();
              break;

            case EventCode.DvdTitleChange:
              Log.Debug("EVT:DvdTitleChange:{0}", p1);
              _currTitle = p1;
              UpdateTitle(_currTitle);
              break;

            case EventCode.DvdCmdStart:
              if (_pendingCmd)
              {
                Log.Debug("EVT:DvdCmdStart with pending");
              }
              break;

            case EventCode.DvdCmdEnd:
              OnCmdComplete(p1, p2);
              break;

            case EventCode.DvdStillOn:
              Log.Debug("EVT:DvdStillOn:{0}", p1);
              break;

            case EventCode.DvdStillOff:
              Log.Debug("EVT:DvdStillOff:{0}", p1);
              break;

            case EventCode.DvdButtonChange:
              Log.Debug("EVT:DvdButtonChange: buttons:#{0}, focused button: {1}", p1, p2);
              buttonCount = p1;
              focusedButton = p2;
              break;

            case EventCode.DvdNoFpPgc:
              Log.Debug("EVT:DvdNoFpPgc:{0}", p1);
              if (_dvdCtrl != null)
              {
                hr = _dvdCtrl.PlayTitle(1, DvdCmdFlags.None, out _cmdOption);
              }
              break;

            case EventCode.DvdAudioStreamChange:
              Log.Debug("EVT:DvdAudioStreamChange:{0}", p1);
              break;

            case EventCode.DvdValidUopsChange:
              Log.Debug("EVT:DvdValidUopsChange:0x{0:X}", p1);
              _UOPs = p1;
              break;

            case EventCode.DvdDomainChange:
              _currDomain = (DvdDomain)p1;
              switch ((DvdDomain)p1)
              {
                case DvdDomain.FirstPlay:
                  Log.Debug("EVT:DVDPlayer:domain=firstplay");
                  _state = PlayState.Playing;
                  break;
                  // The DVD Navigator has completed playback of the title or 
                  // chapter and did not find any other branching instruction for 
                  // subsequent playback.
                case DvdDomain.Stop:
                  Log.Debug("EVT:DVDPlayer:domain=stop");
                  //Fix unmounting/mounting for the same volume (i.e AnyDVD) 
                  if (!File.Exists(g_Player.CurrentFile))
                  {
                    Log.Debug("EVT:DVDPlayer:domain=stop");
                    Stop();
                  }
                  break;
                case DvdDomain.VideoManagerMenu:
                  Log.Debug("EVT:DVDPlayer:domain=videomanagermenu (menu)");
                  _state = PlayState.Menu;
                  VMR9Util.g_vmr9.EVRSetDVDMenuState(true);
                  break;
                case DvdDomain.VideoTitleSetMenu:
                  Log.Debug("EVT:DVDPlayer:domain=videotitlesetmenu (menu)");
                  _state = PlayState.Menu;
                  VMR9Util.g_vmr9.EVRSetDVDMenuState(true);
                  break;
                case DvdDomain.Title:
                  _state = PlayState.Playing;
                  VMR9Util.g_vmr9.EVRSetDVDMenuState(false);
                  break;
                default:
                  Log.Debug("EVT:DvdDomChange:{0}", p1);
                  break;
              }
              break;
          }

          hr = _mediaEvt.FreeEventParams(code, p1, p2);
        } while (hr == 0);
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer:OnDvdEvent() {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      //      Log.Info("DVDEvent done");
    }

    private void UpdateDuration()
    {
      DvdHMSFTimeCode totaltime = new DvdHMSFTimeCode();
      DvdTimeCodeFlags ulTimeCodeFlags;
      _dvdInfo.GetTotalTitleTime(totaltime, out ulTimeCodeFlags);
      _duration = new TimeSpan(totaltime.bHours, totaltime.bMinutes, totaltime.bSeconds).TotalSeconds;
    }

    private void UpdateTitle(int title)
    {
      UpdateDuration();

      _dvdInfo.GetCurrentDomain(out _currDomain);
      if (_currDomain == DvdDomain.Title)
      {
        bool pbIsDisabled;
        int pulStreamsAvailable, pulCurrentStream, subtitleStream;
        int hr = _dvdInfo.GetCurrentSubpicture(out pulStreamsAvailable, out pulCurrentStream, out pbIsDisabled);
        if (hr != 0)
        {
          Log.Error("DVDPlayer:CurrentSubtitleStream:Unable to get current subpicture with code {0}", hr);
          return;
        }
        if (_forceSubtitles && pulStreamsAvailable > 0)
        {
          for (subtitleStream = 0; subtitleStream < pulStreamsAvailable; subtitleStream++)
          {
            if (_defaultSubtitleLanguage == SubtitleLanguage(subtitleStream))
            {
              break;
            }
          }
          if (subtitleStream != pulCurrentStream)
            CurrentSubtitleStream = subtitleStream;
          if (pbIsDisabled && pulCurrentStream != -1)
            EnableSubtitle = true;
        }
      }
    }

    /// <summary> asynchronous command completed </summary>
    private void OnCmdComplete(int p1, int hrg)
    {
      try
      {
        //				Log.Info( "DVD OnCmdComplete.........." );
        if ((_pendingCmd == false) || (_dvdInfo == null))
        {
          return;
        }

        IDvdCmd cmd;
        int hr = _dvdInfo.GetCmdFromEvent(p1, out cmd);
        if ((hr != 0) || (cmd == null))
        {
          Log.Error("DVDPlayer:OnCmdComplete() GetCmdFromEvent failed!");
          return;
        }

        if (cmd != _cmdOption)
        {
          Log.Error("DVDPlayer:OnCmdComplete() Unknown command!");
          DirectShowUtil.ReleaseComObject(cmd);
          cmd = null;
          return;
        }

        DirectShowUtil.ReleaseComObject(cmd);
        cmd = null;
        DirectShowUtil.ReleaseComObject(_cmdOption);
        _cmdOption = null;
        _pendingCmd = false;
        //				Log.Info( "DVD OnCmdComplete OK." );
        UpdateMenu();
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer:OnCmdComplete() {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }

    /// <summary> update menu items to match current playback state </summary>
    protected void UpdateMenu() {}

    public override MenuItems ShowMenuItems
    {
      get { return MenuItems.All ^ MenuItems.PopUpMenu; }
    }

    public override int PositionX
    {
      get { return _positionX; }
      set
      {
        if (value != _positionX)
        {
          _positionX = value;
          _updateNeeded = true;
        }
      }
    }

    public override int PositionY
    {
      get { return _positionY; }
      set
      {
        if (value != _positionY)
        {
          _positionY = value;
          _updateNeeded = true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return _width; }
      set
      {
        if (value != _width)
        {
          _width = value;
          _updateNeeded = true;
        }
      }
    }

    public override int RenderHeight
    {
      get { return _height; }
      set
      {
        if (value != _height)
        {
          _height = value;
          _updateNeeded = true;
        }
      }
    }

    public override double Duration
    {
      get
      {
        if (_state != PlayState.Init)
        {
          return _duration;
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        if (_state != PlayState.Init)
        {
          return _currentTime;
        }
        return 0.0d;
      }
    }

    public override bool FullScreen
    {
      get { return GUIGraphicsContext.IsFullScreenVideo; }
      set
      {
        if (value != _fullScreen)
        {
          _fullScreen = value;
          _updateNeeded = true;
        }
      }
    }

    public override int Width
    {
      get { return _videoWidth; }
    }

    public override int Height
    {
      get { return _videoHeight; }
    }

    public override void Pause()
    {
      if (_state == PlayState.Paused)
      {
        _mediaCtrl.Run();
        _state = PlayState.Playing;
      }
      else if (_state == PlayState.Playing)
      {
        _state = PlayState.Paused;
        _mediaCtrl.Pause();
      }
    }

    public override bool Paused
    {
      get { return (_state == PlayState.Paused); }
    }

    public override bool Playing
    {
      get
      {
        return (_state == PlayState.Playing ||
                _state == PlayState.Paused ||
                _state == PlayState.Menu);
      }
    }

    public override bool Stopped
    {
      get { return (_state == PlayState.Init); }
    }

    public override string CurrentFile
    {
      get { return _currentFile; }
    }

    public override void Stop()
    {
      int hr = 0;
      if (_mediaCtrl == null || !Playing)
      {
        return;
      }
      Log.Info("DVDPlayer:Stop()");
      if (_mediaEvt != null)
      {
        hr = _mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_DVD_EVENT, IntPtr.Zero);
      }

      hr = _mediaCtrl.Stop();
      if (hr >= 0)
      {
        _state = PlayState.Stopped;
        UpdateMenu();
      }
      CloseInterfaces();
      GUIGraphicsContext.IsFullScreenVideo = false;
      GUIGraphicsContext.IsPlaying = false;
    }

    public override int Speed
    {
      get { return _speed; }
      set
      {
        if (_state != PlayState.Init)
        {
          try
          {
            _speed = value;
            if (_speed >= 1)
            {
              _dvdCtrl.PlayForwards((double)_speed, DvdCmdFlags.Flush, out _cmdOption);
            }
            else if (_speed < 0)
            {
              _dvdCtrl.PlayBackwards((double)-_speed, DvdCmdFlags.Flush, out _cmdOption);
            }
          }
          catch (Exception)
          {
            _speed = 1;
          }
          VMR9Util.g_vmr9.EVRProvidePlaybackRate((double)_speed);
        }
      }
    }

    public override int Volume
    {
      get { return _volume; }
      set
      {
        if (_volume != value)
        {
          _volume = value;
          if (_state != PlayState.Init)
          {
            if (_basicAudio != null)
            {
              // Divide by 100 to get equivalent decibel value. For example, �10,000 is �100 dB. 
              float percent = (float)_volume / 100.0f;
              int volume = (int)((DirectShowVolume.VOLUME_MAX - DirectShowVolume.VOLUME_MIN) * percent);
              _basicAudio.put_Volume((volume - DirectShowVolume.VOLUME_MIN));
            }
          }
        }
      }
    }

    public override Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType; }
      set
      {
        if (_aspectRatio != value)
        {
          _aspectRatio = value;
          _updateNeeded = true;
        }
      }
    }

    public override void SeekRelative(double newTime)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaPos != null)
        {
          double currentPosition = CurrentPosition;
          newTime = currentPosition + newTime;
          if (newTime < 0.0d)
          {
            newTime = 0.0d;
          }
          if (newTime < Duration)
          {
            Log.Debug("DVDPlayer.SeekRelative()");
            SeekAbsolute(newTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double newTime)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaPos != null)
        {
          if (newTime < 0.0d)
          {
            newTime = 0.0d;
          }
          if (newTime < Duration)
          {
            int hours = (int)(newTime / 3600d);
            newTime -= (hours * 3600);
            int minutes = (int)(newTime / 60d);
            newTime -= (minutes * 60);
            int seconds = (int)newTime;
            Log.Info("DVDPlayer:Seek to {0}:{1}:{2}", hours, minutes, seconds);
            DvdHMSFTimeCode timeCode = new DvdHMSFTimeCode();
            timeCode.bHours = (byte)(hours & 0xff);
            timeCode.bMinutes = (byte)(minutes & 0xff);
            timeCode.bSeconds = (byte)(seconds & 0xff);
            timeCode.bFrames = 0;
            DvdPlaybackLocation2 loc;
            _currTitle = _dvdInfo.GetCurrentLocation(out loc);

            try
            {
              int hr = _dvdCtrl.PlayAtTime(timeCode, DvdCmdFlags.Block | DvdCmdFlags.Flush, out _cmdOption);
              if (hr != 0)
              {
                if (((uint)hr) == VFW_E_DVD_OPERATION_INHIBITED)
                {
                  Log.Info("DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) not allowed at this point", hours, minutes,
                           seconds);
                }
                else if (((uint)hr) == VFW_E_DVD_INVALIDDOMAIN)
                {
                  Log.Info("DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) invalid domain", hours, minutes, seconds);
                }
                else
                {
                  Log.Error("DVDPlayer:PlayAtTimeInTitle( {0}:{1:00}:{2:00}) failed:0x{3:X}", hours, minutes, seconds,
                            hr);
                }
              }
              //SetDefaultLanguages();
              Log.Info("DVDPlayer:Seek to {0}:{1}:{2} done", hours, minutes, seconds);
            }
            catch (Exception)
            {
              //sometimes we get a DivideByZeroException  in _dvdCtrl.PlayAtTime()
            }
          }
        }
      }
    }

    public override void SeekRelativePercentage(int percentage)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaPos != null)
        {
          double currentPos = CurrentPosition;
          double duration = Duration;

          double curPercent = (currentPos / Duration) * 100.0d;
          double onePercent = Duration / 100.0d;
          curPercent = curPercent + (double)percentage;
          curPercent *= onePercent;
          if (curPercent < 0.0d)
          {
            curPercent = 0.0d;
          }
          if (curPercent < Duration)
          {
            Log.Debug("DVDPlayer.SeekRelativePercentage()");
            SeekAbsolute(curPercent);
          }
        }
      }
    }

    public override void SeekAsolutePercentage(int percentage)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaPos != null)
        {
          if (percentage < 0)
          {
            percentage = 0;
          }
          if (percentage >= 100)
          {
            percentage = 100;
          }
          double percent = Duration / 100.0f;
          percent *= (double)percentage;
          Log.Debug("DVDPlayer.SeekAbsolutePercentage()");
          SeekAbsolute(percent);
        }
      }
    }

    public override bool GetResumeState(out byte[] resumeData)
    {
      try
      {
        Log.Info("DVDPlayer::GetResumeState() begin");
        resumeData = null;
        IDvdState dvdState;
        int hr = _dvdInfo.GetState(out dvdState);
        if (hr != 0)
        {
          Log.Error("DVDPlayer:GetResumeState() _dvdInfo.GetState failed");
          return false;
        }

        IPersistMemory dvdStatePersistMemory = (IPersistMemory)dvdState;
        if (dvdStatePersistMemory == null)
        {
          Log.Info("DVDPlayer::GetResumeState() could not get IPersistMemory");
          DirectShowUtil.ReleaseComObject(dvdState);
          return false;
        }
        uint resumeSize = 0;
        dvdStatePersistMemory.GetSizeMax(out resumeSize);
        if (resumeSize <= 0)
        {
          Log.Info("DVDPlayer::GetResumeState() failed resumeSize={0}", resumeSize);
          DirectShowUtil.ReleaseComObject(dvdStatePersistMemory);
          DirectShowUtil.ReleaseComObject(dvdState);
          return false;
        }
        IntPtr stateData = Marshal.AllocCoTaskMem((int)resumeSize);

        try
        {
          dvdStatePersistMemory.Save(stateData, true, resumeSize);
          resumeData = new byte[resumeSize];
          Marshal.Copy(stateData, resumeData, 0, (int)resumeSize);
        }
        catch (Exception ex)
        {
          Log.Info("DVDPlayer::GetResumeState() failed {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }

        Marshal.FreeCoTaskMem(stateData);
        DirectShowUtil.ReleaseComObject(dvdStatePersistMemory);
        DirectShowUtil.ReleaseComObject(dvdState);
      }
      catch (Exception)
      {
        resumeData = null;
      }
      return true;
    }

    public override bool SetResumeState(byte[] resumeData)
    {
      if ((resumeData != null) && (resumeData.Length > 0))
      {
        Log.Info("DVDPlayer::SetResumeState() begin");
        try
        {
          IDvdState dvdState;

          int hr = _dvdInfo.GetState(out dvdState);

          if (hr < 0)
          {
            Log.Error("DVDPlayer:GetResumeState() _dvdInfo.GetState failed");
            return false;
          }

          IPersistMemory dvdStatePersistMemory = (IPersistMemory)dvdState;
          IntPtr stateData = Marshal.AllocHGlobal(resumeData.Length);
          Marshal.Copy(resumeData, 0, stateData, resumeData.Length);

          try
          {
            dvdStatePersistMemory.Load(stateData, (uint)resumeData.Length);
          }
          catch (Exception e)
          {
            throw e;
          }
          finally
          {
            Marshal.FreeHGlobal(stateData);
          }

          Log.Info("DVDPlayer::SetResumeState() SetState");
          hr = _dvdCtrl.SetState(dvdState, DvdCmdFlags.Block, out _cmdOption);
          if (hr == 0)
          {
            Log.Info("DVDPlayer::SetResumeState() end true");
            return true;
          }

          DirectShowUtil.ReleaseComObject(dvdState);
        }
        catch {}
      }
      Log.Info("DVDPlayer::SetResumeState() end false");
      return false;
    }

    public override bool HasVideo
    {
      get { return true; }
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }
      if (!_started)
      {
        return;
      }
      OnProcess();
    }

    protected virtual void OnProcess()
    {
      if (_videoWin != null)
      {
        if (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false)
        {
          if (_visible)
          {
            _visible = false;
            _videoWin.put_Visible(OABool.False);
          }
        }
        else if (!_visible)
        {
          _visible = true;
          _videoWin.put_Visible(OABool.True);
        }
      }
    }

    public override void SetVideoWindow()
    {
      if (_videoWin == null)
      {
        return;
      }
      if (GUIGraphicsContext.IsFullScreenVideo != _fullScreen)
      {
        _fullScreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }

      if (!_updateNeeded)
      {
        return;
      }

      _started = true;
      _updateNeeded = false;
      float x = _positionX;
      float y = _positionY;

      int nw = _width;
      int nh = _height;
      if (nw > GUIGraphicsContext.OverScanWidth)
      {
        nw = GUIGraphicsContext.OverScanWidth;
      }
      if (nh > GUIGraphicsContext.OverScanHeight)
      {
        nh = GUIGraphicsContext.OverScanHeight;
      }
      lock (typeof (DVDPlayer))
      {
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          x = _positionX = GUIGraphicsContext.OverScanLeft;
          y = _positionY = GUIGraphicsContext.OverScanTop;
          nw = _width = GUIGraphicsContext.OverScanWidth;
          nh = _height = GUIGraphicsContext.OverScanHeight;
        }
        if (nw <= 0 || nh <= 0)
        {
          return;
        }

        Rectangle source, destination;

        int aspectX, aspectY;
        if (_basicVideo != null)
        {
          _basicVideo.GetVideoSize(out _videoWidth, out _videoHeight);
        }
        aspectX = _videoWidth;
        aspectY = _videoHeight;
        if (_basicVideo != null)
        {
          _basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
        }
        GUIGraphicsContext.VideoSize = new Size(_videoWidth, _videoHeight);

        Geometry m_geometry = new Geometry();
        m_geometry.ImageWidth = _videoWidth;
        m_geometry.ImageHeight = _videoHeight;
        m_geometry.ScreenWidth = nw;
        m_geometry.ScreenHeight = nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        using (Settings xmlreader = new MPSettings())
        {
          bool bUseAR = xmlreader.GetValueAsBool("dvdplayer", "pixelratiocorrection", false);
          if (bUseAR)
          {
            m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
          }
          else
          {
            m_geometry.PixelRatio = 1.0f;
          }
        }
        m_geometry.GetWindow(aspectX, aspectY, out source, out destination);
        destination.X += (int)x;
        destination.Y += (int)y;


        Log.Info("overlay: video WxH  : {0}x{1}", _videoWidth, _videoHeight);
        Log.Info("overlay: video AR   : {0}:{1}", aspectX, aspectY);
        Log.Info("overlay: screen WxH : {0}x{1}", nw, nh);
        Log.Info("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
        Log.Info("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
        Log.Info("overlay: src        : ({0},{1})-({2},{3})",
                 source.X, source.Y, source.X + source.Width, source.Y + source.Height);
        Log.Info("overlay: dst        : ({0},{1})-({2},{3})",
                 destination.X, destination.Y, destination.X + destination.Width, destination.Y + destination.Height);


        SetSourceDestRectangles(source, destination);
        SetVideoPosition(destination);

        //hr=_videoWin.SetWindowPosition( destination.X, destination.Y, destination.Width, destination.Height );
        //hr=_dvdCtrl.SelectVideoModePreference(_videoPref);        
        DirectShowUtil.SetARMode(_graphBuilder, arMode);

        _sourceRectangle = source;
        _videoRectangle = destination;
      }
    }

    public override bool IsDVD
    {
      get { return true; }
    }

    public override bool IsDVDMenu
    {
      get { return _state == PlayState.Menu; }
    }

    public override bool OnAction(Action action)
    {
      try
      {
        switch (action.wID)
        {
          case GUI.Library.Action.ActionType.ACTION_MOUSE_MOVE:
            if (buttonCount > 0)
            {
              pt = new Point((int)action.fAmount1, (int)action.fAmount2);
              _dvdCtrl.SelectAtPosition(pt);
              return true;
            }
            break;

          case GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK:
            if (buttonCount > 0)
            {              
              _dvdCtrl.ActivateAtPosition(pt);
              return true;
            }
            break;

          case Action.ActionType.ACTION_MOVE_LEFT:
            Log.Info("DVDPlayer: move left {0}", buttonCount);
            if (buttonCount > 0)
            {
              _dvdCtrl.SelectRelativeButton(DvdRelativeButton.Left);
              return true;
            }
            break;

          case Action.ActionType.ACTION_MOVE_RIGHT:
            Log.Info("DVDPlayer: move right {0}", buttonCount);
            if (buttonCount > 0)
            {
              _dvdCtrl.SelectRelativeButton(DvdRelativeButton.Right);
              return true;
            }
            break;
          case Action.ActionType.ACTION_MOVE_UP:
            Log.Info("DVDPlayer: move up {0}", buttonCount);
            if (buttonCount > 0)
            {
              _dvdCtrl.SelectRelativeButton(DvdRelativeButton.Upper);
              return true;
            }
            break;

          case Action.ActionType.ACTION_MOVE_DOWN:
            Log.Info("DVDPlayer: move down {0}", buttonCount);
            if (buttonCount > 0)
            {
              _dvdCtrl.SelectRelativeButton(DvdRelativeButton.Lower);
              return true;
            }
            break;

          case Action.ActionType.ACTION_SELECT_ITEM:
            if (buttonCount > 0 && focusedButton > 0 && _dvdCtrl != null)
            {
              Log.Info("DVDPlayer: select");
              _dvdCtrl.ActivateButton();
              return true;
            }
            break;

          case Action.ActionType.ACTION_DVD_MENU:

            if ((!Playing) || (_dvdCtrl == null))
            {
              return false;
            }
            Speed = 1;
            _dvdCtrl.ShowMenu(DvdMenuId.Root, (DvdCmdFlags)((int)DvdCmdFlags.Block | (int)DvdCmdFlags.Flush),
                              out _cmdOption);
            return true;

          case Action.ActionType.ACTION_NEXT_CHAPTER:
            {
              if ((_state != PlayState.Playing) || (_dvdCtrl == null))
              {
                return false;
              }

              Speed = 1;
              int hr = _dvdCtrl.PlayNextChapter(DvdCmdFlags.SendEvents | DvdCmdFlags.Flush, out _cmdOption);
              if (hr < 0)
              {
                Log.Error("!!! PlayNextChapter error : 0x" + hr.ToString("x"));
                return false;
              }

              if (_cmdOption != null)
              {
                Trace.WriteLine("PlayNextChapter cmd pending..........");
                _pendingCmd = true;
              }

              UpdateMenu();
              return true;
            }

          case Action.ActionType.ACTION_PREV_CHAPTER:
            {
              if ((_state != PlayState.Playing) || (_dvdCtrl == null))
              {
                return false;
              }

              Speed = 1;
              int hr = _dvdCtrl.PlayPrevChapter(DvdCmdFlags.SendEvents | DvdCmdFlags.Flush, out _cmdOption);
              if (hr < 0)
              {
                Log.Error("DVDPlayer:!!! PlayPrevChapter error : 0x" + hr.ToString("x"));
                return false;
              }

              if (_cmdOption != null)
              {
                Trace.WriteLine("PlayPrevChapter cmd pending..........");
                _pendingCmd = true;
              }

              UpdateMenu();
              return true;
            }
        }
      }
      catch (Exception ex)
      {
        Log.Error("DVDPlayer:OnAction() {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      return false;
    }

    /// <summary>
    /// Set the default languages of the 
    /// </summary>
    private void SetDefaultLanguages()
    {
      Log.Info("SetDefaultLanguages");
      // Flip: Added more detailed message
      int setError = 0;
      string errorText = "";
      int lCID = GetLCID(_defaultAudioLanguage);
      if (lCID >= 0)
      {
        setError = 0;
        errorText = "";
        // Flip: Added more detailed message
        setError = _dvdCtrl.SelectDefaultAudioLanguage(lCID, DvdAudioLangExt.NotSpecified);
        switch (setError)
        {
          case 0:
            errorText = "Success.";
            break;
          case 631:
            errorText = "The DVD Navigator filter is not in the Stop domain.";
            break;
          default:
            errorText = "Unknown Error. " + setError;
            break;
        }
        Log.Info("DVDPlayer:Set default language:{0} {1} {2}", _defaultAudioLanguage, lCID, errorText);
      }

      // For now, the default menu language is the same as the subtitle language
      lCID = GetLCID(_defaultSubtitleLanguage);
      if (lCID >= 0)
      {
        setError = 0;
        errorText = "";
        setError = _dvdCtrl.SelectDefaultMenuLanguage(lCID);
        // Flip: Added more detailed message
        switch (setError)
        {
          case 0:
            errorText = "Success.";
            break;
          case 631:
            errorText = "The DVD Navigator filter is not in a valid domain.";
            break;
          default:
            errorText = "Unknown Error. " + setError;
            break;
        }
        Log.Info("DVDPlayer:Set default menu language:{0} {1} {2}", _defaultSubtitleLanguage, lCID, errorText);
      }

      lCID = GetLCID(_defaultSubtitleLanguage);
      if (lCID >= 0)
      {
        setError = 0;
        errorText = "";
        setError = _dvdCtrl.SelectDefaultSubpictureLanguage(lCID, DvdSubPictureLangExt.NotSpecified);
        // Flip: Added more detailed message
        switch (setError)
        {
          case 0:
            errorText = "Success.";
            break;
          case 631:
            errorText = "The DVD Navigator filter is not in a valid domain.";
            break;
          default:
            errorText = "Unknown Error. " + setError;
            break;
        }
        Log.Info("DVDPlayer:Set default subtitle language:{0} {1} {2}", _defaultSubtitleLanguage, lCID, errorText);
      }
    }

    private static int GetLCID(string language)
    {
      if (language == null)
      {
        return -1;
      }
      if (language.Length == 0)
      {
        return -1;
      }
      // Flip: Added to cut off the detailed name info
      string cutName;
      int start = 0;
      // Flip: Changed from CultureTypes.NeutralCultures to CultureTypes.SpecificCultures
      // Flip: CultureTypes.NeutralCultures did not work, provided the wrong CLID
      foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
      {
        // Flip: cut off detailed info, e.g. "English (United States)" -> "English"
        // Flip: to get correct compare
        start = ci.EnglishName.IndexOf(" (");
        if (start > 0)
        {
          cutName = ci.EnglishName.Substring(0, start);
        }
        else
        {
          cutName = ci.EnglishName;
        }

        if (String.Compare(cutName, language, true) == 0)
        {
          return ci.LCID;
        }
      }
      return -1;
    }

    public override int AudioStreams
    {
      get
      {
        int streamsAvailable, currentStream;
        int hr = _dvdInfo.GetCurrentAudio(out streamsAvailable, out currentStream);
        if (hr == 0)
        {
          return streamsAvailable;
        }
        return 1;
      }
    }

    public override int CurrentAudioStream
    {
      get
      {
        int streamsAvailable, currentStream;
        int hr = _dvdInfo.GetCurrentAudio(out streamsAvailable, out currentStream);
        if (hr == 0)
        {
          return currentStream;
        }
        return 0;
      }
      set
      {
        try
        {
          int hr = _dvdCtrl.SelectAudioStream(value, DvdCmdFlags.None, out _cmdOption);
          if (hr != 0)
          {
            Log.Error("DVDPlayer:Failed to set audiostream to {0} with error code:{1}", value, hr);
          }
        }
        catch {}
      }
    }

    public override string AudioLanguage(int iStream)
    {
      string details = String.Empty;
      int audioLanguage;
      int hr = _dvdInfo.GetAudioLanguage(iStream, out audioLanguage);

      if (hr == 0)
      {
        DvdAudioAttributes attr;
        hr = _dvdInfo.GetAudioAttributes(iStream, out attr);
        if (hr == 0)
        {
          string channelInfo = String.Empty;
          int noc = attr.bNumberOfChannels;

          if (noc > 2)
          {
            noc -= 1;
            channelInfo = noc + "." + "1";
          }
          else
          {
            channelInfo = noc + "." + "0";
          }

          details = " [" + attr.AudioFormat
                    + " " + channelInfo
                    + " - " + attr.dwFrequency + " Hz " +
                    + +attr.bQuantization + " bits]";
        }

        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
          if (ci.LCID == (audioLanguage & 0x3ff))
          {
            return ci.EnglishName + details;
          }
        }
      }
      return Strings.Unknown;
    }

    /// <summary>
    /// Get the number of available subpictures
    /// </summary>
    public override int SubtitleStreams
    {
      get
      {
        int streamsAvailable = 0;
        int currentStream = 0;
        bool isDisabled;
        int hr = _dvdInfo.GetCurrentSubpicture(out streamsAvailable, out currentStream, out isDisabled);
        if (hr == 0)
        {
          return streamsAvailable;
        }
        else
          return 0;
      }
    }

    /// <summary>
    /// Get/Set the current subpicture ID
    /// </summary>
    public override int CurrentSubtitleStream
    {
      get
      {
        int pulStreamsAvailable, pulCurrentStream;
        bool pbIsDisabled;
        if (_line21Decoder != null)
        {
          AMLine21CCState state = AMLine21CCState.Off;
          _line21Decoder.GetServiceState(out state);
          if (state == AMLine21CCState.On)
          {
            return -1;
          }
        }
        int hr = _dvdInfo.GetCurrentSubpicture(out pulStreamsAvailable, out pulCurrentStream, out pbIsDisabled);
        if (hr != 0)
        {
          Log.Error("DVDPlayer:CurrentSubtitleStream:Unable to get current subpicture with code {0}", hr);
          return 0;
        }
        //_log.Debug("DVDPlayer:CurrentSubtitleStream:Getting subpicture state: streams {0},stream {1}, enabled {2}", pulStreamsAvailable, pulCurrentStream, !pbIsDisabled);
        return pulCurrentStream;
      }
      set
      {
        try
        {
          int hr = 0;
          if (_line21Decoder != null)
          {
            if (value == -1)
            {
              hr = _line21Decoder.SetServiceState(AMLine21CCState.On);
            }
            else
            {
              hr = _line21Decoder.SetServiceState(AMLine21CCState.Off);
            }
          }
          if (value != -1)
          {
            hr = _dvdCtrl.SelectSubpictureStream(value, DvdCmdFlags.Flush | DvdCmdFlags.SendEvents, out _cmdOption);
          }
          if (hr != 0)
          {
            Log.Error("DVDPlayer:CurrentSubtitleStream:Unable to set current subpicture with code {0}", hr);
          }
        }
        catch {}
        //_log.Debug("DVDPlayer:CurrentSubtitleStream:Setting subpicture stream: stream {0}", value);
      }
    }

    /// <summary>
    /// Translate the subpicture ID to a human readable string.
    /// </summary>
    /// <param name="iStream">Subpicture ID</param>
    /// <returns>Human readable string representing the subpicture</returns>
    public override string SubtitleLanguage(int iStream)
    {
      int iLanguage;
      int hr = _dvdInfo.GetSubpictureLanguage(iStream, out iLanguage);
      if (hr == 0)
      {
        foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
          if (ci.LCID == (iLanguage & 0x3ff))
          {
            return ci.EnglishName;
          }
        }
        Log.Error("DVDPlayer:Failed translating subpicture ID to string");
        return Strings.Unknown;
      }
      Log.Error("DVDPlayer:Failed translating subpicture ID to string with error code {0}", hr);
      return Strings.Unknown;
    }

    public override string SubtitleName(int iStream)
    {
      return "";
    }

    public void AddPreferedCodecs(IGraphBuilder _graphBuilder)
    {
      // add preferred video & audio codecs
      string strVideoCodec = "";
      string strAudioCodec = "";
      string strAudiorenderer = "";
      int intFilters = 0; // FlipGer: count custom filters
      string strFilters = ""; // FlipGer: collect custom filters
      using (Settings xmlreader = new MPSettings())
      {
        // FlipGer: load infos for custom filters
        int intCount = 0;
        while (xmlreader.GetValueAsString("dvdplayer", "filter" + intCount.ToString(), "undefined") != "undefined")
        {
          if (xmlreader.GetValueAsBool("dvdplayer", "usefilter" + intCount.ToString(), false))
          {
            strFilters += xmlreader.GetValueAsString("dvdplayer", "filter" + intCount.ToString(), "undefined") + ";";
            intFilters++;
          }
          intCount++;
        }
        strVideoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
        strAudioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
        strAudiorenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
      }
      if (strVideoCodec.Length > 0)
      {
        DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
      }
      if (strAudioCodec.Length > 0)
      {
        DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
      }
      if (strAudiorenderer.Length > 0)
      {
        DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudiorenderer, false);
      }
      // FlipGer: add custom filters to graph
      string[] arrFilters = strFilters.Split(';');
      for (int i = 0; i < intFilters; i++)
      {
        DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
      }
    }

    /// <summary>
    /// Enable/Disable the subpictures
    /// </summary>
    public override bool EnableSubtitle
    {
      get
      {
        int pulStreamsAvailable, pulCurrentStream;
        bool pbIsDisabled;
        if (_line21Decoder != null)
        {
          AMLine21CCState state;
          _line21Decoder.GetServiceState(out state);
          if (state == AMLine21CCState.On)
          {
            return true;
          }
        }
        int hr = _dvdInfo.GetCurrentSubpicture(out pulStreamsAvailable, out pulCurrentStream, out pbIsDisabled);
        if (hr != 0)
        {
          Log.Error("DVDPlayer:EnableSubtitle:Unable to get subpicture state (enabled/disabled) with code {0}", hr);
          return false;
        }
        //_log.Debug("DVDPlayer:EnableSubtitle:Getting subpicture state: streams {0},stream {1}, enabled {2}", pulStreamsAvailable, pulCurrentStream, !pbIsDisabled);
        return !pbIsDisabled;
      }
      set
      {
        try
        {
          int hr;
          AMLine21CCState state = AMLine21CCState.Off;
          if (CurrentSubtitleStream == -1)
          {
            state = value ? AMLine21CCState.On : AMLine21CCState.Off;
            hr = _dvdCtrl.SetSubpictureState(false, DvdCmdFlags.Flush | DvdCmdFlags.SendEvents, out _cmdOption);
          }
          else
          {
            hr = _dvdCtrl.SetSubpictureState(value, DvdCmdFlags.Flush | DvdCmdFlags.SendEvents, out _cmdOption);
          }
          if (_line21Decoder != null)
          {
            hr = _line21Decoder.SetServiceState(state);
          }
          if (hr != 0)
          {
            Log.Error("DVDPlayer:EnableSubtitle:Unable to set subpicture state (enabled/disabled) with code {0}", hr);
          }
          //_log.Debug("DVDPlayer:EnableSubtitle:Setting subpicture state to enabled {0}", value);
        }
        catch {}
      }
    }

    // <summary>
    /// Property to Get Postprocessing
    /// </summary>
    public override bool HasPostprocessing
    {
      get { return PostProcessingEngine.GetInstance().HasPostProcessing; }
    }

    public bool SupportsCC
    {
      get
      {
        if (_showClosedCaptions)
        {
          return (_line21Decoder != null);
        }
        else
        {
          return false;
        }
      }
    }

    protected virtual void SetVideoPosition(Rectangle destination)
    {
      if (_videoWin != null)
      {
        _videoWin.SetWindowPosition(destination.Left, destination.Top, destination.Width, destination.Height);
      }
    }

    protected virtual void SetSourceDestRectangles(Rectangle source, Rectangle destination)
    {
      if (_basicVideo != null)
      {
        _basicVideo.SetSourcePosition(source.Left, source.Top, source.Width, source.Height);
        _basicVideo.SetDestinationPosition(0, 0, destination.Width, destination.Height);
      }
    }

    public override bool CanSeek()
    {
      if ((_UOPs & UOP_FLAG_Play_Title_Or_AtTime) == 0)
      {
        return true;
      }
      if ((_UOPs & UOP_FLAG_Play_Chapter_Or_AtTime) == 0)
      {
        return true;
      }
      return false;
    }

    protected virtual void Repaint() {}

    public override AudioStream BestAudio
    {
      get { return GetCurrentAudio(); }
    }

    private AudioStream GetCurrentAudio()
    {
      DvdAudioAttributes attr;
      var hr = _dvdInfo.GetAudioAttributes(CurrentAudioStream, out attr);
      if (hr == 0)
      {
        return new AudioStream(CurrentAudioStream)
        {
          Channel = attr.bNumberOfChannels,
          Format = _dvdAudioFormat[attr.AudioFormat],
          Codec = _dvdAudioCodec[attr.AudioFormat],
          SamplingRate = attr.dwFrequency,
          BitDepth = attr.bQuantization,
          Lcid = attr.Language
        };
      }

      return null;
    }

    public override AudioStream CurrentAudio
    {
      get { return GetCurrentAudio(); }
    }

    public override int VideoStreams
    {
      get
      {
        int anglesAvailable, currentAngle;
        int hr = _dvdInfo.GetCurrentAngle(out anglesAvailable, out currentAngle);
        if (hr == 0)
        {
          return anglesAvailable;
        }
        return 1;
      }
    }

    public override int CurrentVideoStream
    {
      get
      {
        int anglesAvailable, currentAngle;
        int hr = _dvdInfo.GetCurrentAngle(out anglesAvailable, out currentAngle);
        if (hr == 0)
        {
          return currentAngle;
        }
        return 0;
      }
      set
      {
        try
        {
          int hr = _dvdCtrl.SelectAngle(value, DvdCmdFlags.None, out _cmdOption);
          if (hr != 0)
          {
            Log.Error("DVDPlayer:Failed to set angle to {0} with error code:{1}", value, hr);
          }
        }
        catch { }
      }
    }

    private VideoStream GetCurrentVideo()
    {
        DvdVideoAttributes attr;
        var hr = _dvdInfo.GetCurrentVideoAttributes(out attr);
        if (hr == 0)
        {
            return new VideoStream(CurrentAudioStream)
            {
                AspectRatio = attr.aspectX == 4 && attr.aspectY == 3 ? AspectRatio.Tv : AspectRatio.HighDefinitionTv,
                Codec = attr.compression == DvdVideoCompression.Mpeg1 ? VideoCodec.V_MPEG1 : VideoCodec.V_MPEG2,
                Format = "MPEG Video",
                Height = attr.sourceResolutionY,
                Width = attr.sourceResolutionX,
                Stereoscopic = StereoMode.Mono,
                Interlaced = attr.sourceResolutionY < 576,
                Default = true,
            };
        }

        return null;
    }

    public override VideoStream BestVideo
    {
        get { return GetCurrentVideo(); }
    }

    public override VideoStream CurrentVideo
    {
        get { return GetCurrentVideo(); }
    }

    public override int EditionStreams { get { return 0; } }

    public override int CurrentEditionStream
    {
        get { return 0; }
        set { }
    }
  }
}