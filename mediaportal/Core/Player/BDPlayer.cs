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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player.PostProcessing;
using MediaPortal.Player.Subtitles;
using MediaPortal.Profile;
using System.Collections.Generic;

namespace MediaPortal.Player
{
  #region public structs
  [StructLayout(LayoutKind.Sequential)]
  public struct BDPlayerSettings
  {
    public int regionCode;
    public int parentalControl;
    public string audioLang;
    public string menuLang;
    public string subtitleLang;
    public string countryCode;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDEvent
  {
    public int Event;
    public int Param;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDTitleInfo
  {
    public int idx;
    public int playlist;
    public ulong duration;
    public int clip_count;
    public int angle_count;
    public int chapter_count;
    public unsafe BDClipInfo* clips;
    public unsafe BDChapter* chapters;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDChapter
  {
    public int idx;
    public long start;
    public long duration;
    public long offset;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDClipInfo
  {
    public int pkt_count;
    public int still_mode;
    public int still_time;  /* seconds */
    public int video_stream_count;
    public int audio_stream_count;
    public int pg_stream_count;
    public int ig_stream_count;
    public int sec_audio_stream_count;
    public int sec_video_stream_count;
    public unsafe BDStreamInfo* video_streams;
    public unsafe BDStreamInfo* audio_streams;
    public unsafe BDStreamInfo* pg_streams;
    public unsafe BDStreamInfo* ig_streams;
    public unsafe BDStreamInfo* sec_audio_streams;
    public unsafe BDStreamInfo* sec_video_streams;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDStreamInfo
  {
    public int coding_type;
    public int format;
    public int rate;
    public int char_code;
    public int lang;
    public int pid;
    public int aspect;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct OSDTexture
  {
    public int width;
    public int height;
    public int x;
    public int y;
    public IntPtr texture; // IDirect3DTexture9
  }
  #endregion

  #region public enums

  [Flags]
  public enum MenuItems
  {
    None = 0,
    MainMenu = 1,
    PopUpMenu = 2,
    Chapter = 4,
    Audio = 8,
    Subtitle = 16,
    All = 255
  }

  #endregion

  #region public interfaces
  [ComVisible(true), ComImport,
   Guid("324FAA1F-4DA6-47B8-832B-3993D8FF4151"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IBDReaderCallback
  {
    [PreserveSig]
    int OnMediaTypeChanged(int mediaType);

    [PreserveSig]
    int OnVideoFormatChanged(int streamType, int width, int height, int aspectRatioX, int aspectRatioY, int bitrate,
                             int isInterlaced);

    [PreserveSig]
    int OnBDevent([Out] BDEvent bdEvent);

    [PreserveSig]
    int OnOSDUpdate([Out] OSDTexture osdTexture);
  }
  #endregion

  internal class BDPlayer : IPlayer, IBDReaderCallback
  {
    #region protected interfaces
    [Guid("79A37017-3178-4859-8079-ECB9D546FEC2"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    protected interface IBDReader
    {
      [PreserveSig]
      int SetBDReaderCallback(IBDReaderCallback callback);

      [PreserveSig]
      int Action(int key);

      [PreserveSig]
      int SetAngle(int angle);

      [PreserveSig]
      int SetChapter(uint chapter);

      [PreserveSig]
      int GetAngle(ref int angle);

      [PreserveSig]
      int GetChapter(ref uint chapter);

      [PreserveSig]
      int GetTitleCount(ref uint count);

      [PreserveSig]
      IntPtr GetTitleInfo(int index);

      [PreserveSig]
      int FreeTitleInfo(IntPtr info);

      [PreserveSig]
      int OnGraphRebuild(int info);

      [PreserveSig]
      int ForceTitleBasedPlayback(bool force, UInt32 title);

      [PreserveSig]
      int SetD3DDevice(IntPtr d3dDevice);

      [PreserveSig]
      int SetBDPlayerSettings(BDPlayerSettings settings);

      [PreserveSig]
      int Start();

      [PreserveSig]
      int MouseMove(int x, int y);
    }
    #endregion

    #region protected classes
    [ComImport, Guid("79A37017-3178-4859-8079-ECB9D546FEB2")]
    protected class BDReader { }

    protected class EventBuffer
    {
      const int size = 128;
      BDEvent[] buffer = new BDEvent[size];
      int readPos = 0;
      int writePos = 0;

      public bool isEmpty()
      {
        if (readPos == writePos)
          return true;
        else
          return false;
      }

      public int Count
      {
        get
        {
          int len = writePos - readPos;
          if (len < 0)
            len += size;
          return len;
        }
      }

      public void Clear()
      {
        writePos = 0;
        readPos = 0;
      }

      public void Set(BDEvent data)
      {
        buffer[writePos] = data;
        writePos = (writePos + 1) % size;
        if (readPos == writePos)
          Log.Error("BDPlayer: Event buffer full");
      }

      public BDEvent Peek()
      {
        return buffer[readPos];
      }

      public BDEvent Get()
      {
        int pos = readPos;
        readPos = (readPos + 1) % size;
        return buffer[pos];
      }
    }

    protected class BDFilterConfig
    {
      public string Video { get; set; }
      public string VideoH264 { get; set; }
      public string VideoVC1 { get; set; }
      public string Audio { get; set; }
      //public string AudioAAC { get; set; }
      //public string AudioDDPlus { get; set; }
      public string AudioRenderer { get; set; }
      public Geometry.Type AR { get; set; }
    }

    #endregion

    #region enums

    protected enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended,
      Menu
    }

    protected enum BDKeys
    {
      /* numeric key events */
      BD_VK_0 = 0,
      BD_VK_1 = 1,
      BD_VK_2 = 2,
      BD_VK_3 = 3,
      BD_VK_4 = 4,
      BD_VK_5 = 5,
      BD_VK_6 = 6,
      BD_VK_7 = 7,
      BD_VK_8 = 8,
      BD_VK_9 = 9,

      BD_VK_ROOT_MENU = 10,  /* open root menu */
      BD_VK_POPUP = 11,  /* toggle popup menu */

      /* interactive key events */
      BD_VK_UP = 12,
      BD_VK_DOWN = 13,
      BD_VK_LEFT = 14,
      BD_VK_RIGHT = 15,
      BD_VK_ENTER = 16,

      /* Mouse click */
      /* Translated to BD_VK_ENTER if mouse is over valid button */
      BD_VK_MOUSE_ACTIVATE = 17,
    }

    protected enum BDEvents
    {
      BD_EVENT_NONE = 0,
      BD_EVENT_ERROR,
      BD_EVENT_ENCRYPTED,

      /* current playback position */
      BD_EVENT_ANGLE,     /* current angle, 1...N */
      BD_EVENT_TITLE,     /* current title, 1...N (0 = top menu) */
      BD_EVENT_PLAYLIST,  /* current playlist (xxxxx.mpls) */
      BD_EVENT_PLAYITEM,  /* current play item */
      BD_EVENT_CHAPTER,   /* current chapter, 1...N */
      BD_EVENT_END_OF_TITLE,

      /* stream selection */
      BD_EVENT_AUDIO_STREAM,           /* 1..32,  0xff  = none */
      BD_EVENT_IG_STREAM,              /* 1..32                */
      BD_EVENT_PG_TEXTST_STREAM,       /* 1..255, 0xfff = none */
      BD_EVENT_PIP_PG_TEXTST_STREAM,   /* 1..255, 0xfff = none */
      BD_EVENT_SECONDARY_AUDIO_STREAM, /* 1..32,  0xff  = none */
      BD_EVENT_SECONDARY_VIDEO_STREAM, /* 1..32,  0xff  = none */

      BD_EVENT_PG_TEXTST,              /* 0 - disable, 1 - enable */
      BD_EVENT_PIP_PG_TEXTST,          /* 0 - disable, 1 - enable */
      BD_EVENT_SECONDARY_AUDIO,        /* 0 - disable, 1 - enable */
      BD_EVENT_SECONDARY_VIDEO,        /* 0 - disable, 1 - enable */
      BD_EVENT_SECONDARY_VIDEO_SIZE,   /* 0 - PIP, 0xf - fullscreen */

      /* HDMV VM or JVM seeked the stream. Next read() will return data from new position. */
      BD_EVENT_SEEK,

      /* still playback (pause) */
      BD_EVENT_STILL,                  /* 0 - off, 1 - on */

      /* Still playback for n seconds (reached end of still mode play item) */
      BD_EVENT_STILL_TIME,             /* 0 = infinite ; 1...300 = seconds */

      BD_CUSTOM_EVENT_MENU_VISIBILITY = 1000  /* 0 - not shown, 1 shown*/
    }

    #endregion

    #region variables

    protected const string BD_READER_GRAPH_NAME = "MediaPortal BD Reader";
    protected const uint BLURAY_TITLE_CURRENT = 0xffffffff;
    protected const uint BLURAY_TITLE_FIRST_PLAY = 0xffff;
    protected const uint BLURAY_TITLE_TOP_MENU = 0;
    protected const int WM_GRAPHNOTIFY = 0x00008001; // message from graph
    protected const int WS_CHILD = 0x40000000; // attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;
    protected bool _forceTitle = false;
    protected int _titleToPlay = 0;
    protected VMR9Util _vmr9 = null;
    protected Player.TSReaderPlayer.ISubtitleStream _subtitleStream = null;
    protected IBDReader _ireader = null;
    protected int iSpeed = 1;
    protected int _positionX = 0;
    protected int _positionY = 0;
    protected int _width = 200;
    protected int _height = 100;
    protected int _videoWidth = 100;
    protected int _videoHeight = 100;
    protected string _currentFile = "";
    protected bool _isFullscreen = false;
    protected PlayState _state = PlayState.Init;
    protected int _volume = 100;
    protected int _volumeBeforeSeeking = 0;
    protected IGraphBuilder _graphBuilder = null;
    protected IMediaSeeking _mediaSeeking = null;
    protected IMediaPosition _mediaPos = null;
    protected double _currentPos;
    protected double _duration = -1d;
    protected DsROTEntry _rotEntry = null;
    protected int _aspectX = 1;
    protected int _aspectY = 1;
    protected bool _usingFastSeeking = false;
    protected IBaseFilter _interfaceBDReader = null;
    protected IBaseFilter _audioRendererFilter = null;
    protected SubtitleSelector _subSelector = null;
    protected SubtitleRenderer _dvbSubRenderer = null;

    /// <summary> control interface. </summary>
    protected IMediaControl _mediaCtrl = null;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx _mediaEvt = null;

    /// <summary> video preview window interface. </summary>
    protected IVideoWindow _videoWin = null;

    /// <summary> interface to get information and control video. </summary>
    protected IBasicVideo2 _basicVideo = null;

    /// <summary> audio interface used to control volume. </summary>
    protected IBasicAudio _basicAudio = null;
    protected DateTime _elapsedTimer = DateTime.Now;
    protected DateTime _updateTimer = DateTime.Now;
    protected Geometry.Type _geometry = Geometry.Type.Normal;
    protected int iChangedMediaTypes;
    protected VideoStreamFormat _videoFormat;
    protected int _lastFrameCounter;
    protected string videoFilter = "";
    protected string audioFilter = "";
    protected bool _bMediaTypeChanged;
    protected int _currentTitle = 0;
    protected int _currentChapter = 0;
    protected int _currentSubtitleStream = 0;
	  protected int _newSubtitleStream = -1;
    protected int _currentAudioStream = 0;
    protected EventBuffer eventBuffer = new EventBuffer();
    protected MenuItems menuItems = MenuItems.All;
    protected double[] chapters;

    protected BDFilterConfig filterConfig;
    protected bool vc1Codec = false;

    #endregion

    #region ctor/dtor

    public BDPlayer()
      : this(g_Player.MediaType.Video)
    {

    }

    public BDPlayer(g_Player.MediaType mediaType)
    {
      _videoFormat = new VideoStreamFormat();
    }



    #endregion

    #region public override members

    public override double[] Chapters
    {
      get
      {
        return chapters;
      }
    }

    public override bool OnAction(GUI.Library.Action action)
    {
      if (_ireader == null)
        return false;
      try
      {
        switch (action.wID)
        {
          case GUI.Library.Action.ActionType.ACTION_MOUSE_MOVE:
            if (_state != PlayState.Menu)
              return false;
            int x = (int)(action.fAmount1 / ((float)GUIGraphicsContext.Width / 1920.0f));
            int y = (int)(action.fAmount2 / ((float)GUIGraphicsContext.Height / 1080.0f));
            //Log.Debug("BDPlayer: Mouse move: {0},{1}", x, y);
            _ireader.MouseMove(x, y);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK:
            if (_state != PlayState.Menu)
              return false;
            Log.Debug("BDPlayer: Mouse select");
            _ireader.Action((int)BDKeys.BD_VK_MOUSE_ACTIVATE);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_LEFT:
            if (_state != PlayState.Menu)
              return false;
            Log.Debug("BDPlayer: Move left");
            _ireader.Action((int)BDKeys.BD_VK_LEFT);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT:
            if (_state != PlayState.Menu)
              return false;
            Log.Debug("BDPlayer: Move right");
            _ireader.Action((int)BDKeys.BD_VK_RIGHT);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_UP:
            if (_state != PlayState.Menu)
              return false;
            Log.Debug("BDPlayer: Move up");
            _ireader.Action((int)BDKeys.BD_VK_UP);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_DOWN:
            if (_state != PlayState.Menu)
              return false;
            Log.Debug("BDPlayer: Move down");
            _ireader.Action((int)BDKeys.BD_VK_DOWN);
            return true;

          case GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
            if (_state != PlayState.Menu)
              return false;
            Log.Debug("BDPlayer: Select");
            _ireader.Action((int)BDKeys.BD_VK_ENTER);
            return true;

          case GUI.Library.Action.ActionType.ACTION_DVD_MENU:
            if (!Playing || _forceTitle)
              return false;
            Speed = 1;
            Log.Debug("BDPlayer: Main menu");
            _ireader.Action((int)BDKeys.BD_VK_ROOT_MENU);
            return true;

          case GUI.Library.Action.ActionType.ACTION_BD_POPUP_MENU:
            if (!Playing || _forceTitle)
              return false;
            Speed = 1;
            Log.Debug("BDPlayer: Popup menu");
            _ireader.Action((int)BDKeys.BD_VK_POPUP);
            return true;

          case GUI.Library.Action.ActionType.ACTION_NEXT_CHAPTER:
            {
              if (_state != PlayState.Playing)
                return false;
              Speed = 1;
              uint chapter = 0;
              _ireader.GetChapter(ref chapter);
              Log.Debug("BDPlayer: NextChapter, current: {0}", chapter);
              _ireader.SetChapter(chapter + 1);
              return true;
            }

          case GUI.Library.Action.ActionType.ACTION_PREV_CHAPTER:
            {
              if (_state != PlayState.Playing)
                return false;
              Speed = 1;
              uint chapter = 0;
              _ireader.GetChapter(ref chapter);
              Log.Debug("BDPlayer: PrevChapter, current: {0}", chapter);
              if (chapter > 0)
                _ireader.SetChapter(chapter - 1);
              else if (chapter == 0)
                _ireader.SetChapter(0);
              return true;
            }
        }
      }
      catch (Exception ex)
      {
        Log.Error("BDPlayer:OnAction() {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
      return false;
    }

    public override bool CanSeek()
    {
      return _state == PlayState.Playing;
    }

    /// <summary>
    /// Implements the AudioStreams member which interfaces the BDReader filter to get the IAMStreamSelect interface for enumeration of available streams
    /// </summary>
    public override int AudioStreams
    {
      get
      {
        if (_interfaceBDReader == null)
        {
          return 0;
        }

        int streamCount = 0;
        IAMStreamSelect pStrm = _interfaceBDReader as IAMStreamSelect;
        if (pStrm != null)
        {
          pStrm.Count(out streamCount);
          pStrm = null;
        }
        return streamCount;
      }
    }

    /// <summary>
    /// Implements the CurrentAudioStream member which interfaces the BDReader filter to get the IAMStreamSelect interface for enumeration and switching of available audio streams
    /// </summary>
    public override int CurrentAudioStream
    {
      get { return _currentAudioStream; }
      set
      {
        if (value > AudioStreams)
        {
          Log.Info("BDPlayer: Unable to set CurrentAudioStream -> value does not exist");
          return;
        }
        if (_interfaceBDReader == null)
        {
          Log.Info("BDPlayer: Unable to set CurrentAudioStream -> BDReader not initialized");
          return;
        }
        IAMStreamSelect pStrm = _interfaceBDReader as IAMStreamSelect;
        if (pStrm != null)
        {
          pStrm.Enable(value, AMStreamSelectEnableFlags.Enable);
          _currentAudioStream = value;
        }
        else
        {
          Log.Info("BDPlayer: Unable to set CurrentAudioStream -> IAMStreamSelect == null");
        }
        return;
      }
    }

    /// <summary>
    /// Property to get the type of an audio stream
    /// </summary>
    public override string AudioType(int iStream)
    {
      if ((iStream + 1) > AudioStreams)
      {
        return Strings.Unknown;
      }

      if (_interfaceBDReader == null)
      {
        Log.Info("BDPlayer: Unable to get AudioType -> BDReader not initialized");
        return Strings.Unknown;
      }
      IAMStreamSelect pStrm = _interfaceBDReader as IAMStreamSelect;
      if (pStrm != null)
      {
        AMMediaType sType;
        AMStreamSelectInfoFlags sFlag;
        int sPDWGroup, sPLCid;
        string sName;
        object pppunk, ppobject;
        pStrm.Info(iStream, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);

        if (sType.subType == MEDIASUBTYPE_AC3_AUDIO)
        {
          return "AC3";
        }
        if (sType.subType == MEDIASUBTYPE_DDPLUS_AUDIO)
        {
          return "AC3plus";
        }
        if (sType.subType == MEDIASUBTYPE_MPEG1_PAYLOAD)
        {
          return "Mpeg1";
        }
        if (sType.subType == MEDIASUBTYPE_MPEG1_AUDIO)
        {
          return "Mpeg1";
        }
        if (sType.subType == MEDIASUBTYPE_MPEG2_AUDIO)
        {
          return "Mpeg2";
        }
        if (sType.subType == MEDIASUBTYPE_LATM_AAC_AUDIO) //MediaSubType.LATMAAC)
        {
          return "LATMAAC";
        }
        if (sType.subType == MEDIASUBTYPE_AAC_AUDIO) //MediaSubType.AAC)
        {
          return "AAC";
        }
      }
      return Strings.Unknown;
    }

    /// <summary>
    /// Implements the AudioLanguage member which interfaces the BDReader filter to get the IAMStreamSelect interface for getting info about a stream
    /// </summary>
    /// <param name="iStream"></param>
    /// <returns></returns>
    public override string AudioLanguage(int iStream)
    {
      if ((iStream + 1) > AudioStreams)
      {
        return Strings.Unknown;
      }

      if (_interfaceBDReader == null)
      {
        Log.Info("BDPlayer: Unable to get AudioLanguage -> BDReader not initialized");
        return Strings.Unknown;
      }

      IAMStreamSelect pStrm = _interfaceBDReader as IAMStreamSelect;
      if (pStrm != null)
      {
        AMMediaType sType;
        AMStreamSelectInfoFlags sFlag;
        int sPDWGroup, sPLCid;
        string sName;
        object pppunk, ppobject;

        pStrm.Info(iStream, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);

        return GetFullLanguageName(sName.Trim());
      }
      else
      {
        return Strings.Unknown;
      }
    }

    public override bool Play(string strFile)
    {
      Log.Info("BDPlayer play: {0}", strFile);
      if (!File.Exists(strFile))
      {
        return false;
      }      

      if (!GetInterfaces(strFile))
      {
        MovieEnded();

        if (_titleToPlay == -1)
          return true;

        Log.Error("BDPlayer:GetInterfaces() failed");        
        return false;
      }

      iSpeed = 1;
      _duration = -1d;

      ExclusiveMode(true);
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      _isVisible = false;
      _volume = 100;
      _state = PlayState.Init;
      _currentFile = strFile;
      _isFullscreen = false;
      _geometry = Geometry.Type.Normal;

      int hr = _mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
      if (hr < 0)
      {
        Log.Error("BDPlayer:SetNotifyWindow() failed");
        MovieEnded();
        return false;
      }
      if (_videoWin != null)
      {
        _videoWin.put_Owner(GUIGraphicsContext.ActiveForm);
        _videoWin.put_WindowStyle(
          (WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipSiblings + (int)WindowStyle.ClipChildren));
        _videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);
      }
      if (_basicVideo != null)
      {
        hr = _basicVideo.GetVideoSize(out _videoWidth, out _videoHeight);
        if (hr < 0)
        {
          Log.Error("BDPlayer:GetVideoSize() failed");
          MovieEnded();
          return false;
        }
        Log.Info("BDPlayer:VideoSize:{0}x{1}", _videoWidth, _videoHeight);
      }
      GUIGraphicsContext.VideoSize = new Size(_videoWidth, _videoHeight);
      if (_mediaCtrl == null)
      {
        Log.Error("BDPlayer:_mediaCtrl==null");
        MovieEnded();
        return false;
      }
      hr = _mediaCtrl.Run();
      if (hr < 0)
      {
        Log.Error("BDPlayer: Unable to start playing");
        MovieEnded();
        return false;
      }
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
      msg.Label = strFile;
      GUIWindowManager.SendThreadMessage(msg);
      _state = PlayState.Playing;
      _positionX = GUIGraphicsContext.VideoWindow.X;
      _positionY = GUIGraphicsContext.VideoWindow.Y;
      _width = GUIGraphicsContext.VideoWindow.Width;
      _height = GUIGraphicsContext.VideoWindow.Height;
      _geometry = GUIGraphicsContext.ARType;
      UpdateCurrentPosition();
      OnInitialized();
      Log.Info("BDPlayer: position:{0}, duration:{1}", CurrentPosition, Duration);

      if (_forceTitle)
        menuItems = MenuItems.Chapter | MenuItems.Audio | MenuItems.Subtitle;
      else
        menuItems = MenuItems.MainMenu;
      return true;
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullscreen)
      {
        _isFullscreen = GUIGraphicsContext.IsFullScreenVideo;
      }
    }

    public override bool Ended
    {
      get { return _state == PlayState.Ended; }
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }

      if (_bMediaTypeChanged)
      {
        _bMediaTypeChanged = false;
        DoGraphRebuild();
        _ireader.OnGraphRebuild(iChangedMediaTypes);
      }

      HandleBDEvent();
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds >= 200 || iSpeed != 1)
      {
        _updateTimer = DateTime.Now;
        UpdateCurrentPosition();
        if (_videoWin != null)
        {
          if (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false)
          {
            if (_isVisible)
            {
              _isVisible = false;
              _videoWin.put_Visible(OABool.False);
            }
          }
          else if (!_isVisible)
          {
            _isVisible = true;
            _videoWin.put_Visible(OABool.True);
          }
        }
        CheckVideoResolutionChanges();
      }
      if (iSpeed != 1)
      {
        DoFFRW();
      }
      else
      {
        _lastFrameCounter = 0;
      }
      OnProcess();
    }

    public override bool Visible
    {
      get { return _isVisible; }
      set
      {
        if (value == _isVisible)
        {
          return;
        }
        _isVisible = value;
      }
    }

    public override int Speed
    {
      get
      {
        if (_mediaSeeking == null || _state == PlayState.Init)
        {
          return 1;
        }
        return iSpeed;
      }
      set
      {
        if (_mediaCtrl == null || _mediaSeeking == null || _state == PlayState.Init)
        {
          return;
        }
        if (iSpeed != value)
        {
          iSpeed = value;
          // Inform dshowhelper of playback rate changes
          if (VMR9Util.g_vmr9 != null) VMR9Util.g_vmr9.EVRProvidePlaybackRate((double)value);

          int hr = _mediaSeeking.SetRate((double)iSpeed);
          if (hr == 0)
          {
            Log.Info("BDPlayer: Successfully set rate to {0}", iSpeed);
            if (iSpeed != 1)
            {
              _usingFastSeeking = true;
              if (_mediaCtrl != null) _mediaCtrl.Run();
            }
          }
          else
          {
            Log.Info("BDPlayer: Could not set rate to {0}, error: 0x{1:x}", iSpeed, hr);
          }

          if (iSpeed == 1)
          {
            _usingFastSeeking = false;

            // unmute audio when speed returns to 1.0x
            if (_volumeBeforeSeeking != 0)
            {
              Volume = _volumeBeforeSeeking;
            }
            _volumeBeforeSeeking = 0;
          }
          else
          {
            // mute audio during > 1.0x playback
            if (_volumeBeforeSeeking == 0)
            {
              _volumeBeforeSeeking = Volume;
            }
            Volume = 0;
          }
        }
      }
    }

    public override void WndProc(ref Message m)
    {
      if (m.Msg == WM_GRAPHNOTIFY)
      {
        if (_mediaEvt != null)
        {
          OnGraphNotify();
        }
        return;
      }
      base.WndProc(ref m);
    }

    public override int PositionX
    {
      get { return _positionX; }
      set
      {
        if (value != _positionX)
        {
          _positionX = value;
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
        }
      }
    }

    public override double Duration
    {
      get { return _duration; }
    }

    public override double CurrentPosition
    {
      get { return _currentPos; }
    }

    public override double StreamPosition
    {
      get { return _currentPos; }
    }

    public override double ContentStart
    {
      get { return 0.0; }
    }

    public override bool FullScreen
    {
      get { return GUIGraphicsContext.IsFullScreenVideo; }
      set
      {
        if (value != _isFullscreen)
        {
          _isFullscreen = value;
          GUIGraphicsContext.IsFullScreenVideo = _isFullscreen;
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
        Speed = 1;
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

    public override bool Initializing
    {
      get { return (_state == PlayState.Init); }
    }

    public override bool Playing
    {
      get { return (_state == PlayState.Playing || _state == PlayState.Paused || _state == PlayState.Menu); }
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
      MovieEnded();
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
              // Divide by 100 to get equivalent decibel value. For example, –10,000 is –100 dB. 
              float fPercent = (float)_volume / 100.0f;
              int iVolume = (int)(5000.0f * fPercent);
              _basicAudio.put_Volume((iVolume - 5000));
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
        if (_geometry != value)
        {
          _geometry = value;
        }
      }
    }

    public override void SeekAbsolute(double dTimeInSecs)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          lock (_mediaCtrl)
          {
            int SeekTries = 3;

            UpdateCurrentPosition();
            if (dTimeInSecs < 0)
            {
              dTimeInSecs = 0;
            }
            dTimeInSecs *= 10000000d;

            long lTime = (long)dTimeInSecs;

            while (SeekTries > 0)
            {
              long pStop = 0;
              long lContentStart, lContentEnd;
              _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
              lTime += lContentStart;
              Log.Info("BDPlayer:seekabs:{0} start:{1} end:{2}", lTime, lContentStart, lContentEnd);

              int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning,
                                                  new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
              long lStreamPos;
              _mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
              _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
              Log.Info("BDPlayer: pos: {0} start:{1} end:{2}", lStreamPos, lContentStart, lContentEnd);
              if (lStreamPos > lContentStart)
              {
                Log.Info("BDPlayer seek done:{0:X}", hr);
                SeekTries = 0;
              }
              else
              {
                // This could happen in LiveTv/Rstp when TsBuffers are reused and requested position is before "start"
                // Only way to recover correct position is to seek again on "start"
                SeekTries--;
                lTime = 0;
                Log.Info("BDPlayer seek again : pos: {0} lower than start:{1} end:{2} ( Cnt {3} )", lStreamPos, lContentStart, lContentEnd, SeekTries);
              }
            }

            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.FrameCounter = 123;
            }
          }
        }

        UpdateCurrentPosition();
        if (_dvbSubRenderer != null)
        {
          _dvbSubRenderer.OnSeek(CurrentPosition);
        }
        _state = PlayState.Playing;
        Log.Info("BDPlayer: current pos:{0} dur:{1}", CurrentPosition, Duration);
      }
    }

    public override void SeekRelative(double dTime)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          double dCurTime = CurrentPosition;

          dTime = dCurTime + dTime;
          if (dTime < 0.0d)
          {
            dTime = 0.0d;
          }
          if (dTime < Duration)
          {
            SeekAbsolute(dTime);
          }
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          double dCurrentPos = CurrentPosition;
          double dDuration = Duration;
          double fCurPercent = (dCurrentPos / Duration) * 100.0d;
          double fOnePercent = Duration / 100.0d;
          fCurPercent = fCurPercent + (double)iPercentage;
          fCurPercent *= fOnePercent;
          if (fCurPercent < 0.0d)
          {
            fCurPercent = 0.0d;
          }
          if (fCurPercent < Duration)
          {
            SeekAbsolute(fCurPercent);
          }
        }
      }
    }

    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          if (iPercentage < 0)
          {
            iPercentage = 0;
          }
          if (iPercentage >= 100)
          {
            iPercentage = 100;
          }
          double fPercent = Duration / 100.0f;
          fPercent *= (double)iPercentage;
          SeekAbsolute(fPercent);
        }
      }
    }

    public override bool HasVideo
    {
      get { return true; }
    }

    public override bool IsDVD
    {
      get { return true; }
    }

    public override MenuItems ShowMenuItems
    {
      get { return menuItems; }
    }

    public override VideoStreamFormat GetVideoFormat()
    {
      return _videoFormat;
    }

    /// <summary>
    /// Property to get the total number of subtitle streams
    /// </summary>
    public override int SubtitleStreams
    {
      get
      {
        if (_subSelector == null)
        {
          return 0;
        }
        return _subSelector.CountOptions();
      }
    }

    /// <summary>
    /// Property to get/set the current subtitle stream
    /// </summary>
    public override int CurrentSubtitleStream
    {
      get
      {
        if (_subSelector != null)
        {
          return _subSelector.GetCurrentOption();
        }
        else
        {
          return 0;
        }
      }
      set
      {
        if (_subSelector != null)
        {
          try
          {
            _subSelector.SetOption(value);
          }
          catch(Exception e)
          {
            Log.Error("BDPlayer: CurrentSubtitleStream failed - TODO: add stream cache on .ax side");
          }
        }
      }
    }

    /// <summary>
    /// Property to get the language for a subtitle stream
    /// </summary>
    public override string SubtitleLanguage(int iStream)
    {
      if (_subSelector != null)
      {
        return GetFullLanguageName(_subSelector.GetLanguage(iStream));
      }
      else
      {
        return Strings.Unknown;
      }
    }

    /// <summary>
    /// Property to get the name for a subtitle stream
    /// </summary>
    public override string SubtitleName(int iStream)
    {
      return SubtitleLanguage(iStream);
    }

    /// <summary>
    /// Property to enable/disable subtitles
    /// </summary>
    public override bool EnableSubtitle
    {
      get
      {
        if (_subSelector != null)
        {
          return _dvbSubRenderer.RenderSubtitles;
        }
        else
        {
          return false;
        }
      }
      set
      {
        if (_subSelector != null)
        {
          _dvbSubRenderer.RenderSubtitles = value;
        }
      }
    }

    /// <summary>
    /// Property to Get Postprocessing
    /// </summary>
    public override bool HasPostprocessing
    {
      get { return PostProcessingEngine.GetInstance().HasPostProcessing; }
    }

    #endregion

    #region public members

    public int OnOSDUpdate(OSDTexture osdTexture)
    {
      BDOSDRenderer.GetInstance().DrawItem(osdTexture);
      return 0;
    }

    public int OnMediaTypeChanged(int mediaType)
    {
      _bMediaTypeChanged = true;
      iChangedMediaTypes = mediaType;
      return 0;
    }

    public int OnVideoFormatChanged(int streamType, int width, int height, int aspectRatioX, int aspectRatioY,
                                    int bitrate, int isInterlaced)
    {
      _videoFormat.IsValid = true;
      _videoFormat.streamType = (VideoStreamType)streamType;
      _videoFormat.width = width;
      _videoFormat.height = height;
      _videoFormat.arX = aspectRatioX;
      _videoFormat.arY = aspectRatioY;
      _videoFormat.bitrate = bitrate;
      _videoFormat.isInterlaced = (isInterlaced == 1);
      Log.Info("BDPlayer: OnVideoFormatChanged - {0}", _videoFormat.ToString());
      return 0;
    }

    public int OnBDevent(BDEvent bdevent)
    {
      if (bdevent.Event != 0 && 
        bdevent.Event != (int)BDEvents.BD_EVENT_STILL &&
        bdevent.Event != (int)BDEvents.BD_EVENT_STILL_TIME)
      {
        eventBuffer.Set(bdevent);
        Log.Debug("BDPlayer OnBDEvent: {0}, param: {1}", bdevent.Event, bdevent.Param);
      }
      return 0;
    }
    #endregion

    #region private/protected members

    /// <summary>
    /// Gets the filter configuration object from the user configuration
    /// </summary>
    /// <returns></returns>
    protected virtual BDFilterConfig GetFilterConfiguration()
    {
      BDFilterConfig filterConfig = new BDFilterConfig();

      using (Settings xmlreader = new MPSettings())
      {

        // get pre-defined filter setup
        filterConfig.Video = xmlreader.GetValueAsString("bdplayer", "mpeg2videocodec", "");
        filterConfig.Audio = xmlreader.GetValueAsString("bdplayer", "mpeg2audiocodec", "");
        //filterConfig.AudioAAC = xmlreader.GetValueAsString("bdplayer", "aacaudiocodec", "");
        //filterConfig.AudioDDPlus = xmlreader.GetValueAsString("bdplayer", "ddplusaudiocodec", "");
        filterConfig.VideoH264 = xmlreader.GetValueAsString("bdplayer", "h264videocodec", "");
        filterConfig.VideoVC1 = xmlreader.GetValueAsString("bdplayer", "vc1videocodec", "");
        filterConfig.AudioRenderer = xmlreader.GetValueAsString("bdplayer", "audiorenderer", "Default DirectSound Device");

        // get AR setting
        filterConfig.AR = Util.Utils.GetAspectRatio(xmlreader.GetValueAsString("mytv", "defaultar", "Normal"));
        //GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(strValue);  
      }

      return filterConfig;
    }

    /// <summary>
    /// Loads the setting from the user configuration into the IBDReader object.
    /// </summary>
    /// <param name="reader">IBDReader object</param>
    protected virtual void LoadSettings(IBDReader reader)
    {
      BDPlayerSettings settings = new BDPlayerSettings();

      using (Settings xmlreader = new MPSettings())
      {
        // todo: get settings from the BD Player settings pane (to be created)
        settings.audioLang = xmlreader.GetValueAsString("bdplayer", "audiolanguage", "english");
        settings.subtitleLang = xmlreader.GetValueAsString("bdplayer", "subtitlelanguage", "english");
      }

      foreach (CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
      {
        if (String.Compare(cultureInformation.EnglishName, settings.audioLang, true) == 0)
        {
          //settings.audioLang = xmlreader.GetValueAsString("bdplayer", "audiolanguage", "english");
          //settings.subtitleLang = xmlreader.GetValueAsString("bdplayer", "subtitlelanguage", "english");
          settings.countryCode = cultureInformation.TwoLetterISOLanguageName;
          settings.audioLang = cultureInformation.ThreeLetterISOLanguageName;
        }
        if (String.Compare(cultureInformation.EnglishName, settings.subtitleLang, true) == 0)
        {
          settings.subtitleLang = cultureInformation.ThreeLetterISOLanguageName;
        }
      }
      settings.menuLang = settings.audioLang;
      settings.parentalControl = 0; // what to do with this?
      settings.regionCode = 7; // what to do with this?

      reader.SetBDPlayerSettings(settings);
    }

    /// <summary>
    /// Gets the title info collection from the given BDReader object.
    /// </summary>
    /// <param name="reader">IBDReader object</param>
    /// <returns>a collection of titles</returns>
    protected virtual List<BDTitleInfo> GetTitleInfoCollection(IBDReader reader)
    {
      List<BDTitleInfo> titles = new List<BDTitleInfo>();

      uint titleCount = 0;
      reader.GetTitleCount(ref titleCount);

      Log.Debug("BDPlayer: Title count - {0}", titleCount);

      for (int i = 0; i < titleCount; i++)
      {
        BDTitleInfo titleInfo = GetTitleInfo(reader, i);
        titles.Add(titleInfo);
      }

      return titles;
    }

    /// <summary>
    /// Gets the title info for the specified index
    /// </summary>
    /// <param name="reader">IBDReader object</param>
    /// <param name="index">index of the title</param>
    /// <returns></returns>
    protected virtual BDTitleInfo GetTitleInfo(IBDReader reader, int index)
    {
      BDTitleInfo titleInfo = new BDTitleInfo();
      IntPtr ptr = IntPtr.Zero;
      try
      {
        ptr = reader.GetTitleInfo(index);
        titleInfo = (BDTitleInfo)Marshal.PtrToStructure(ptr, typeof(BDTitleInfo));
      }
      catch
      {
        Log.Error("BDPlayer: GetTitleInfo({0}) failed.", index);
      }
      finally
      {
        if (ptr != IntPtr.Zero)
        {
          reader.FreeTitleInfo(ptr);
        }
      }

      return titleInfo;
    }

    /// <summary>
    /// Gets chapters from the given BDTitleInfo object
    /// </summary>
    /// <param name="titleInfo">BDTitleInfo object</param>
    /// <returns>chapters as an array consisting of the start time in seconds</returns>
    protected virtual double[] GetChapters(BDTitleInfo titleInfo)
    {
      double[] chapters = new double[titleInfo.chapter_count];

      if (chapters.Length > 1)
      {
        for (int i = 0; i < chapters.Length; i++)
        {
          unsafe
          {
            double s = titleInfo.chapters[i].start / 90000;
            chapters[i] = s;
            TimeSpan ts = TimeSpan.FromSeconds(s);
            Log.Debug("BDPlayer: Chapter info #{0}: start time: {1}", titleInfo.chapters[i].idx, String.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds));
          }
        }
      }

      return chapters;
    }

    /// <summary>
    /// Selects the title for playback.
    /// </summary>
    /// <param name="titles">a collection of titles to choose from</param>
    /// <returns>index of the title to play, -2 for navigation/menu or -1 if user canceled the dialog</returns>
    protected virtual int SelectTitle(List<BDTitleInfo> titles)
    {
      IDialogbox dialog = (IDialogbox)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
      bool listAllTitles = false;

      titles = titles.OrderByDescending(t => t.duration).ToList();

      while (true)
      {
        List<int> titleOptions = new List<int>();

        dialog.Reset();
        dialog.SetHeading(GUILocalizeStrings.Get(1701));   // Select play mode
        dialog.Add(GUILocalizeStrings.Get(924));           // Menu

        int c = 0;
        foreach (BDTitleInfo title in titles)
        {
          TimeSpan ts = TimeSpan.FromSeconds(title.duration / 90000);
          if (listAllTitles || titles.Count == 1 || ts.TotalMinutes >= 30 && title.chapter_count > 1 && title.chapter_count < 100) // do not list titles under 30mins
          {
            string duration = String.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
            dialog.Add(String.Format(GUILocalizeStrings.Get(1702), c++, title.chapter_count, duration));
            titleOptions.Add(title.idx);
          }
        }

        // add option to list all titles
        if (titles.Count > 1 && titles.Count != titleOptions.Count)
        {
          dialog.Add(GUILocalizeStrings.Get(1703));       // Show all titles
        }

        // show dialog
        dialog.DoModal(GUIWindowManager.ActiveWindow);

        // handle user selection
        if (dialog.SelectedId > 1)
        {

          // show all titles was selected, we continue the loop and list all titles
          if (dialog.SelectedId == c + 2)
          {
            listAllTitles = true;
            continue;
          }

          // user selected a title we now return the index of
          int titleIdx = titleOptions[dialog.SelectedId - 2];
          Log.Debug("BDPlayer: title with idx {0} selected", titleIdx);
          return titleIdx;
        }

        // user selected to display menu;
        if (dialog.SelectedId == 1)
        {
          return -2;
        }

        // user cancelled so break the loop
        return -1;
      }
    }

    object lockobj = new object();

    protected void HandleBDEvent()
    {
      if (eventBuffer.isEmpty())
        return;
      lock (lockobj)
      {
        BDEvent bdevent = eventBuffer.Get();
        switch (bdevent.Event)
        {
          case (int)BDEvents.BD_EVENT_AUDIO_STREAM:
            Log.Debug("BDPlayer: Audio changed to {0}", bdevent.Param);
            if (bdevent.Param != 0xff)
              CurrentAudioStream = bdevent.Param - 1; // one based on libbluray
            break;

          case (int)BDEvents.BD_EVENT_PG_TEXTST_STREAM:
            Log.Debug("BDPlayer: Subtitle changed to {0}", bdevent.Param);
            if (bdevent.Param != 0xfff)
            {
              int index = bdevent.Param - 1; // one based on libbluray
              CurrentSubtitleStream = index;

              if (CurrentSubtitleStream != index)
                _newSubtitleStream = index;
              else
                _newSubtitleStream = -1;
            }
            break;

          case (int)BDEvents.BD_EVENT_TITLE:
            Log.Debug("BDPlayer: Title changed to {0}", bdevent.Param);
            _currentTitle = bdevent.Param;
            break;

          case (int)BDEvents.BD_EVENT_CHAPTER:
            Log.Debug("BDPlayer: Chapter changed to {0}", bdevent.Param);
            _currentChapter = bdevent.Param;
            break;

          case (int)BDEvents.BD_EVENT_PLAYLIST:
            Log.Debug("BDPlayer: Playlist changed to {0}", bdevent.Param);
            if (_newSubtitleStream != -1)
            {
              Log.Debug("BDPlayer: try delayed subtitle stream update {0}", _newSubtitleStream);
              CurrentSubtitleStream = _newSubtitleStream;
              if (CurrentSubtitleStream == _newSubtitleStream)
                CurrentSubtitleStream = -1;
            }			
            break;

          case (int)BDEvents.BD_CUSTOM_EVENT_MENU_VISIBILITY:
            Log.Debug("BDPlayer: Menu toggle on/off {0}", bdevent.Param);
            if (bdevent.Param == 1)
            {
              _state = PlayState.Menu;
              GUIGraphicsContext.DisableTopBar = true;
              menuItems = MenuItems.MainMenu | MenuItems.PopUpMenu;
            }
            else
            {
              _state = PlayState.Playing;
              GUIGraphicsContext.DisableTopBar = false;
              GUIGraphicsContext.TopBarHidden = true;
              menuItems = MenuItems.All;
            }
            break;
        }
      }
    }

    protected void UpdateChapters()
    {
      try
      {
        BDTitleInfo titleInfo = GetTitleInfo(_ireader, unchecked((int)BLURAY_TITLE_CURRENT));
        chapters = GetChapters(titleInfo);
      }
      catch
      {
        Log.Error("BDPlayer: UpdateChapters failed");
      }
    }

    protected string GetFullLanguageName(string language)
    {
      foreach (System.Globalization.CultureInfo ci in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures))
      {
        if (language.Equals(ci.ThreeLetterISOLanguageName) || ci.EnglishName.StartsWith(language, StringComparison.InvariantCultureIgnoreCase))
        {
          language = Util.Utils.TranslateLanguageString(ci.EnglishName);
          break;
        }
      }
      return language;
    }

    protected void MovieEnded()
    {
      _currentFile = "";
      CloseInterfaces();
      ExclusiveMode(false);
      _state = PlayState.Ended;
    }

    protected void CheckVideoResolutionChanges()
    {
      if (_videoWin == null || _basicVideo == null)
      {
        return;
      }
      int aspectX, aspectY;
      int videoWidth = 1, videoHeight = 1;
      if (_basicVideo != null)
      {
        _basicVideo.GetVideoSize(out videoWidth, out videoHeight);
      }
      aspectX = videoWidth;
      aspectY = videoHeight;
      if (_basicVideo != null)
      {
        _basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
      }
      if (videoHeight != _videoHeight || videoWidth != _videoWidth ||
          aspectX != _aspectX || aspectY != _aspectY)
      {
        SetVideoWindow();
      }
    }

    protected void UpdateCurrentPosition()
    {
      if (_mediaPos != null)
      {
        _mediaPos.get_CurrentPosition(out _currentPos);
        _mediaPos.get_Duration(out _duration);
      }
    }

    protected void DoGraphRebuild()
    {
      if (_mediaCtrl != null)
      {
        int hr;
        try
        {
          hr = _mediaCtrl.Stop();
          DsError.ThrowExceptionForHR(hr);
        }
        catch (Exception error)
        {
          Log.Error("Error stopping graph: {0}", error.Message);
          MovieEnded();
        }

        // this is a hack for MS Video Decoder and AC3 audio change
        // would suggest to always do full audio and video rendering for all filters
        IBaseFilter MSVideoCodec = null;
        _graphBuilder.FindFilterByName("Microsoft DTV-DVD Video Decoder", out MSVideoCodec);
        if (MSVideoCodec != null)
        {
          iChangedMediaTypes = 3;
          DirectShowUtil.ReleaseComObject(MSVideoCodec); MSVideoCodec = null;
        }
        // hack end
        switch (iChangedMediaTypes)
        {
          case 1: // audio changed
            Log.Info("Rerendering audio pin of BDReader filter.");
            UpdateFilters("Audio");
            break;
          case 2: // video changed
            Log.Info("Rerendering video pin of BDReader filter.");
            UpdateFilters("Video");
            break;
          case 3: // both changed
            Log.Info("Rerendering audio and video pins of BDReader filter.");
            UpdateFilters("Audio");
            UpdateFilters("Video");
            break;
        }
        DirectShowUtil.RenderUnconnectedOutputPins(_graphBuilder, _interfaceBDReader);
        DirectShowUtil.RemoveUnusedFiltersFromGraph(_graphBuilder);

        try
        {
          hr = _mediaCtrl.Run();
          DsError.ThrowExceptionForHR(hr);
        }
        catch (Exception error)
        {
          Log.Error("Error starting graph: {0}", error.Message);
          MovieEnded();
          return;
        }
        Log.Info("Reconfigure graph done");
      }
    }

    protected void OnGraphNotify()
    {
      if (_mediaEvt == null)
      {
        return;
      }
      int p1, p2, hr = 0;
      EventCode code;
      do
      {
        hr = _mediaEvt.GetEvent(out code, out p1, out p2, 0);
        if (hr < 0)
        {
          break;
        }
        hr = _mediaEvt.FreeEventParams(code, p1, p2);
        if (code == EventCode.Complete || code == EventCode.ErrorAbort)
        {
          MovieEnded();
          break;
        }
      } while (hr == 0);
    }

    protected void OnInitialized()
    {
      if (_vmr9 != null)
      {
        _vmr9.Enable(true);
        SetVideoWindow();
      }
    }

    protected void DoFFRW()
    {
      if (!Playing || _usingFastSeeking)
      {
        return;
      }
      if ((iSpeed == 1) || (_mediaSeeking == null) || (_mediaCtrl == null))
      {
        return;
      }
      TimeSpan ts = DateTime.Now - _elapsedTimer;
      if ((ts.TotalMilliseconds < 200) || (ts.TotalMilliseconds < Math.Abs(1000.0f / Speed)) ||
          ((VMR9Util.g_vmr9 != null && _lastFrameCounter == VMR9Util.g_vmr9.FreeFrameCounter) &&
          (ts.TotalMilliseconds < 2000)))
      {
        // Ambass : Normally, 100 mS are enough to present the new frame, but sometimes the PC is thinking...and we launch a new seek
        return;
        // before the StopWhenReady() method has been completed. It results as a kind of mess in the BDReader....
      } // So, it's better to verify a new frame has been pesented.
      long earliest, latest, current, stop, rewind, pStop;
      lock (_mediaCtrl)
      {
        _mediaSeeking.GetAvailable(out earliest, out latest);
        _mediaSeeking.GetPositions(out current, out stop);

        //Log.Info(" time from last : {6} {7} {8} earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
        //         earliest / 10000000, latest / 10000000, current / 10000000, stop / 10000000, _speedRate, (latest - earliest) / 10000000, (long)ts.TotalMilliseconds, VMR9Util.g_vmr9.FreeFrameCounter, Speed);

        // new time = current time + timerinterval * speed
        long lTimerInterval = (long)ts.TotalMilliseconds;
        if (lTimerInterval > 1000)
        {
          lTimerInterval = 1000;
        }
        rewind = (long)(current + ((long)(lTimerInterval) * Speed * 10000));
        int hr;
        pStop = 0;
        // if we end up before the first moment of time then just
        // start @ the beginning
        if ((rewind < earliest) && (iSpeed < 0))
        {
          rewind = earliest;
          Log.Info("BDPlayer: timeshift SOF seek back:{0}", rewind / 10000000);
          hr = _mediaSeeking.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                          new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          Speed = 1;
          return;
        }
        // if we end up at the end of time then just
        // start @ the end -1sec
        long margin = 100000;

        if ((rewind > (latest - margin)) && (iSpeed > 0))
        {
          Log.Info("BDPlayer: Fastforward reached the end of file, stopping playback");
          _state = PlayState.Ended;
          return;
        }
        //seek to new moment in time
        //Log.Info(" seek :{0}",rewind/10000000);
        if (VMR9Util.g_vmr9 != null)
        {
          _lastFrameCounter = VMR9Util.g_vmr9.FreeFrameCounter;
          VMR9Util.g_vmr9.EVRProvidePlaybackRate(0.0);
        }
        hr = _mediaSeeking.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                        new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        _mediaCtrl.Run();
        //according to ms documentation, this is the prefered way to do seeking
        // _mediaCtrl.StopWhenReady();
        _elapsedTimer = DateTime.Now;
      }
    }

    protected string MatchFilters(string format)
    {
      if (format == "Video")
      {
        IPin pinOut1 = DsFindPin.ByDirection((IBaseFilter)_interfaceBDReader, PinDirection.Output, 1); //video
        if (pinOut1 != null)
        {
          //Detection if the Video Stream is VC-1 on output pin of the splitter
          IEnumMediaTypes enumMediaTypesVideo;
          int hr = pinOut1.EnumMediaTypes(out enumMediaTypesVideo);
          while (true)
          {
            AMMediaType[] mediaTypes = new AMMediaType[1];
            int typesFetched;
            hr = enumMediaTypesVideo.Next(1, mediaTypes, out typesFetched);
            if (hr != 0 || typesFetched == 0) break;
            if (mediaTypes[0].majorType == MediaType.Video && mediaTypes[0].subType == MediaSubType.VC1)
            {
              Log.Info("BDPlayer: found VC-1 video out pin");
              vc1Codec = true;
            }
          }
          DirectShowUtil.ReleaseComObject(enumMediaTypesVideo);
          enumMediaTypesVideo = null;
          if (pinOut1 != null)
          {
            DirectShowUtil.ReleaseComObject(pinOut1);
            pinOut1 = null;
          }
        }
      }
      if (format == "Video")
      {
        if (_videoFormat.streamType == VideoStreamType.MPEG2)
        {
          return filterConfig.Video;
        }
        else if (vc1Codec)
        {
          return filterConfig.VideoVC1;
        }
        else
        {
          return filterConfig.VideoH264;
        }
      }
      else
      {
        /*if (AudioType(CurrentAudioStream).Contains("AAC"))
        {
          return filterConfig.AudioAAC;
        }
        else
        {
          if (AudioType(CurrentAudioStream).Equals("AC3plus"))
          {
            return filterConfig.AudioDDPlus;
          }
          else*/
        {
          return filterConfig.Audio;
        }
      }
      //}
    }

    /// <summary>
    /// Update graph with proper filters
    /// </summary>
    /// <param name="selection">The selection.</param>
    protected void UpdateFilters(string selection)
    {
      IPin pinFrom = DirectShowUtil.FindPin(_interfaceBDReader, PinDirection.Output, selection);
      IPin pinTo;
      int hr = pinFrom.ConnectedTo(out pinTo);
      if (hr >= 0 && pinTo != null)
      {
        PinInfo pInfo;
        pinTo.QueryPinInfo(out pInfo);
        FilterInfo fInfo;
        pInfo.filter.QueryFilterInfo(out fInfo);
        Log.Debug("BDPlayer: Remove filter - {0}", fInfo.achName);
        _graphBuilder.RemoveFilter(pInfo.filter);
        DsUtils.FreePinInfo(pInfo);
        DirectShowUtil.ReleaseComObject(fInfo.pGraph);
        DirectShowUtil.ReleaseComObject(pinTo);
        pinTo = null;
      }
      DirectShowUtil.ReleaseComObject(pinFrom);
      pinFrom = null;
      DirectShowUtil.AddFilterToGraph(_graphBuilder, MatchFilters(selection));
    }

    protected bool GetInterfaces(string filename)
    {
      try
      {
        Log.Debug("BDPlayer: GetInterfaces()");

        _graphBuilder = (IGraphBuilder)new FilterGraph();
        _rotEntry = new DsROTEntry(_graphBuilder as IFilterGraph);

        filterConfig = GetFilterConfiguration();

        if (filterConfig.AudioRenderer.Length > 0)
        {
          _audioRendererFilter = DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, filterConfig.AudioRenderer, true);
        }

        BDReader reader = new BDReader();
        _interfaceBDReader = reader as IBaseFilter;
        _ireader = reader as IBDReader;

        if (_interfaceBDReader == null || _ireader == null)
        {
          // todo: add exception
          return false;
        }

        // add the BD reader
        int hr = _graphBuilder.AddFilter(_interfaceBDReader, BD_READER_GRAPH_NAME);
        DsError.ThrowExceptionForHR(hr);

        Log.Info("BDPlayer: Add BDReader to graph");

        IFileSourceFilter interfaceFile = (IFileSourceFilter)_interfaceBDReader;
        hr = interfaceFile.Load(filename, null);

        DsError.ThrowExceptionForHR(hr);

        Log.Info("BDPlayer: BDReader loaded: {0}", filename);

        #region setup BDReader

        LoadSettings(_ireader);
        _ireader.SetD3DDevice(DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device));
        _ireader.SetBDReaderCallback(this);

        List<BDTitleInfo> titles = GetTitleInfoCollection(_ireader);

        while (true)
        {
          _titleToPlay = SelectTitle(titles);
          if (_titleToPlay > -1)
          {
            // a specific title was selected
            _forceTitle = true;
          }
          else
          {
            if (_titleToPlay == -1)
            {
              // user cancelled dialog
              return false;
            }

            // user choose to display menu
            _forceTitle = false;
          }

          _ireader.ForceTitleBasedPlayback(_forceTitle, (uint)_titleToPlay);

          Log.Info("BDPlayer: Starting BDReader");
          hr = _ireader.Start();
          if (hr != 0)
          {

            if (!_forceTitle)
            {
              Log.Error("BDPlayer: Failed to start file:{0} :0x{1:x}", filename, hr);
              continue;
            }

            Log.Error("BDPlayer: Failed to start in title based mode file:{0} :0x{1:x}", filename, hr);            
            return false;
          }
          else
          {
            Log.Info("BDPlayer: BDReader started: {0}", filename);
          }

          break;
        }

        #endregion

        #region Filters

        Log.Info("BDPlayer: Adding filters");

        _vmr9 = new VMR9Util();
        _vmr9.AddVMR9(_graphBuilder);
        _vmr9.Enable(false);

        // Add preferred video filters
        UpdateFilters("Video");

        // Add preferred audio filters
        UpdateFilters("Audio");

        // Let the subtitle engine handle the proper filters
        try
        {
          SubtitleRenderer.GetInstance().AddSubtitleFilter(_graphBuilder);
        }
        catch (Exception e)
        {
          Log.Error(e);
        }

        #endregion

        #region PostProcessingEngine Detection

        IPostProcessingEngine postengine = PostProcessingEngine.GetInstance(true);
        if (!postengine.LoadPostProcessing(_graphBuilder))
        {
          PostProcessingEngine.engine = new PostProcessingEngine.DummyEngine();
        }

        #endregion

        #region render BDReader output pins

        Log.Info("BDPlayer: Render BDReader outputs");

        DirectShowUtil.RenderUnconnectedOutputPins(_graphBuilder, _interfaceBDReader);
        DirectShowUtil.RemoveUnusedFiltersFromGraph(_graphBuilder);

        #endregion

        _mediaCtrl = (IMediaControl)_graphBuilder;
        _mediaEvt = (IMediaEventEx)_graphBuilder;
        _mediaSeeking = (IMediaSeeking)_graphBuilder;
        _mediaPos = (IMediaPosition)_graphBuilder;

        try
        {
          SubtitleRenderer.GetInstance().SetPlayer(this);
          _dvbSubRenderer = SubtitleRenderer.GetInstance();
        }
        catch (Exception e)
        {
          Log.Error(e);
        }

        _subtitleStream = (Player.TSReaderPlayer.ISubtitleStream)_interfaceBDReader;
        if (_subtitleStream == null)
        {
          Log.Error("BDPlayer: Unable to get ISubtitleStream interface");
        }

        // if only dvb subs are enabled, pass null for ttxtDecoder
        _subSelector = new SubtitleSelector(_subtitleStream, _dvbSubRenderer, null);

        if (_audioRendererFilter != null)
        {
          //Log.Info("BDPlayer:set reference clock");
          IMediaFilter mp = (IMediaFilter)_graphBuilder;
          IReferenceClock clock = (IReferenceClock)_audioRendererFilter;
          hr = mp.SetSyncSource(null);
          hr = mp.SetSyncSource(clock);
          //Log.Info("BDPlayer:set reference clock:{0:X}", hr);
          _basicAudio = (IBasicAudio)_graphBuilder;
        }

        if (!_vmr9.IsVMR9Connected)
        {
          Log.Error("BDPlayer: Failed vmr9 not connected");          
          return false;
        }
        _vmr9.SetDeinterlaceMode();
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("BDPlayer: Exception while creating DShow graph {0}", ex.Message);        
        return false;
      }
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected void CloseInterfaces()
    {
      if (_graphBuilder == null)
      {
        return;
      }
      int hr;
      Log.Info("BDPlayer: cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
      try
      {
        BDOSDRenderer.Release();
        
        if (_mediaCtrl != null)
        {
          int counter = 0;
          FilterState state;
          hr = _mediaCtrl.Stop();
          hr = _mediaCtrl.GetState(10, out state);
          while (state != FilterState.Stopped || GUIGraphicsContext.InVmr9Render)
          {
            Thread.Sleep(100);
            hr = _mediaCtrl.GetState(10, out state);
            counter++;
            if (counter >= 30)
            {
              if (state != FilterState.Stopped)
                Log.Error("BDPlayer: graph still running");
              if (GUIGraphicsContext.InVmr9Render)
                Log.Error("BDPlayer: in renderer");
              break;
            }
          }
          _mediaCtrl = null;
        }

        if (_mediaEvt != null)
        {
          hr = _mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
          _mediaEvt = null;
        }

        _videoWin = _graphBuilder as IVideoWindow;
        if (_videoWin != null)
        {
          hr = _videoWin.put_Visible(OABool.False);
          hr = _videoWin.put_Owner(IntPtr.Zero);
          _videoWin = null;
        }

        _mediaSeeking = null;
        _basicAudio = null;
        _basicVideo = null;
        _ireader = null;

        if (_audioRendererFilter != null)
        {
          while (DirectShowUtil.ReleaseComObject(_audioRendererFilter) > 0) ;
          _audioRendererFilter = null;
        }

        if (_interfaceBDReader != null)
        {
          DirectShowUtil.ReleaseComObject(_interfaceBDReader, 5000);
          _interfaceBDReader = null;
        }

        if (_vmr9 != null)
        {
          _vmr9.Enable(false);
          _vmr9.SafeDispose();
          _vmr9 = null;
        }

        if (_graphBuilder != null)
        {
          DirectShowUtil.RemoveFilters(_graphBuilder);
          if (_rotEntry != null)
          {
            _rotEntry.SafeDispose();
            _rotEntry = null;
          }
          while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0) ;
          _graphBuilder = null;
        }

        if (_dvbSubRenderer != null)
        {
          _dvbSubRenderer.SetPlayer(null);
          _dvbSubRenderer = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        _state = PlayState.Init;
      }
      catch (Exception ex)
      {
        Log.Error("BDPlayer: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }
      //switch back to directx windowed mode
      Log.Info("BDPlayer: Disabling DX9 exclusive mode");
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      GUIWindowManager.SendMessage(msg);
    }

    protected void ExclusiveMode(bool onOff)
    {
      GUIMessage msg = null;
      if (onOff)
      {
        Log.Info("BDPlayer: Enabling DX9 exclusive mode");
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 1, 0, null);
      }
      else
      {
        Log.Info("BDPlayer: Disabling DX9 exclusive mode");
        msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED, 0, 0, 0, 0, 0, null);
      }
      GUIWindowManager.SendMessage(msg);
    }

    protected void OnProcess()
    {
      if (_vmr9 != null)
      {
        _videoWidth = _vmr9.VideoWidth;
        _videoHeight = _vmr9.VideoHeight;
      }
    }

    #endregion

    #region IDisposable Members

    public override void Dispose()
    {
      CloseInterfaces();
    }

    #endregion

    #region private properties -guids

    private static Guid MEDIASUBTYPE_AC3_AUDIO
    {
      get { return new Guid("e06d802c-db46-11cf-b4d1-00805f6cbbea"); }
    }

    private static Guid MEDIASUBTYPE_DDPLUS_AUDIO
    {
      get { return new Guid("a7fb87af-2d02-42fb-a4d4-05cd93843bdd"); }
    }

    private static Guid MEDIASUBTYPE_MPEG2_AUDIO
    {
      get { return new Guid("e06d802b-db46-11cf-b4d1-00805f6cbbea"); }
    }

    private static Guid MEDIASUBTYPE_MPEG1_PAYLOAD
    {
      get { return new Guid("e436eb81-524f-11ce-9f53-0020af0ba770"); }
    }

    private static Guid MEDIASUBTYPE_MPEG1_AUDIO
    {
      get { return new Guid("e436eb87-524f-11ce-9f53-0020af0ba770"); }
    }

    private static Guid MEDIASUBTYPE_LATM_AAC_AUDIO
    {
      get { return new Guid("000001ff-0000-0010-8000-00aa00389b71"); }
    }

    private static Guid MEDIASUBTYPE_AAC_AUDIO
    {
      get { return new Guid("000000ff-0000-0010-8000-00aa00389b71"); }
    }

    #endregion
  }
}
