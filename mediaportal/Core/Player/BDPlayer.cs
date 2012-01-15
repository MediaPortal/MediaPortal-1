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
    [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 4)] 
    public string audioLang;
    [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 4)] 
    public string menuLang;
    [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 4)] 
    public string subtitleLang;
    [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 4)] 
    public string countryCode;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDEvent
  {
    public int Event;
    public int Param;
  }

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct BDTitleInfo
  {
    public UInt32 idx;
    public UInt32 playlist;
    public UInt64 duration;
    public UInt32 clip_count;
    public byte angle_count;
    public UInt32 chapter_count;
    public BDClipInfo* clips;
    public BDChapter* chapters;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDChapter
  {
    public UInt32 idx;
    public UInt64 start;
    public UInt64 duration;
    public UInt64 offset;
  }

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct BDClipInfo
  {
    public UInt32 pkt_count;
    public byte still_mode;
    public UInt16 still_time;  /* seconds */
    public byte video_stream_count;
    public byte audio_stream_count;
    public byte pg_stream_count;
    public byte ig_stream_count;
    public byte sec_audio_stream_count;
    public byte sec_video_stream_count;
    public byte raw_stream_count;    
    public BDStreamInfo* video_streams;
    public BDStreamInfo* audio_streams;
    public BDStreamInfo* pg_streams;
    public BDStreamInfo* ig_streams;
    public BDStreamInfo* sec_audio_streams;
    public BDStreamInfo* sec_video_streams;
    public BDStreamInfo* raw_streams;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct BDStreamInfo
  {
    public byte coding_type;
    public byte format;
    public byte rate;
    public byte char_code;
    public byte lang;
    public UInt16 pid;
    public byte aspect;
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
    int OnMediaTypeChanged(int videoRate, int videoFormat, int audioFormat);

    [PreserveSig]
    int OnBDevent([Out] BDEvent bdEvent);

    [PreserveSig]
    int OnOSDUpdate([Out] OSDTexture osdTexture);

    [PreserveSig]
    int OnClockChange([Out] long duration, [Out] long position);
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
      int GetCurrentClipStreamInfo(ref BDStreamInfo stream);

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

      [PreserveSig]
      int SetVideoDecoder(int format, ref Guid decoder);

      [PreserveSig]
      int SetVC1Override(ref Guid decoder);
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
      public string VideoMPEG { get; set; }
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
      Ended      
    }

    protected enum MenuState
    {
      None,
      Root,
      PopUp
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
      BD_EVENT_READ_ERROR,
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

      BD_EVENT_SOUND_EFFECT,           /* effect ID */

      /* Pop-Up menu available */
      BD_EVENT_POPUP,                  /* 0 - no, 1 - yes */

      /* Interactive menu visible */
      BD_EVENT_MENU                   /* 0 - no, 1 - yes */
    }

    protected enum BluRayStreamFormats
    {
      BLURAY_STREAM_TYPE_UNKNOWN = 0,
      BLURAY_STREAM_TYPE_VIDEO_MPEG1 = 0x01,
      BLURAY_STREAM_TYPE_VIDEO_MPEG2 = 0x02,
      BLURAY_STREAM_TYPE_AUDIO_MPEG1 = 0x03,
      BLURAY_STREAM_TYPE_AUDIO_MPEG2 = 0x04,
      BLURAY_STREAM_TYPE_AUDIO_LPCM = 0x80,
      BLURAY_STREAM_TYPE_AUDIO_AC3 = 0x81,
      BLURAY_STREAM_TYPE_AUDIO_DTS = 0x82,
      BLURAY_STREAM_TYPE_AUDIO_TRUHD = 0x83,
      BLURAY_STREAM_TYPE_AUDIO_AC3PLUS = 0x84,
      BLURAY_STREAM_TYPE_AUDIO_DTSHD = 0x85,
      BLURAY_STREAM_TYPE_AUDIO_DTSHD_MASTER = 0x86,
      BLURAY_STREAM_TYPE_VIDEO_VC1 = 0xea,
      BLURAY_STREAM_TYPE_VIDEO_H264 = 0x1b,
      BLURAY_STREAM_TYPE_SUB_PG = 0x90,
      BLURAY_STREAM_TYPE_SUB_IG = 0x91,
      BLURAY_STREAM_TYPE_SUB_TEXT = 0x92
    }

    protected enum VideoRate
    {
      BLURAY_VIDEO_RATE_24000_1001 = 1,  // 23.976
      BLURAY_VIDEO_RATE_24 = 2,
      BLURAY_VIDEO_RATE_25 = 3,
      BLURAY_VIDEO_RATE_30000_1001 = 4,  // 29.97
      BLURAY_VIDEO_RATE_50 = 6,
      BLURAY_VIDEO_RATE_60000_1001 = 7   // 59.94
    }

    protected enum VideoFormat
    {
      BLURAY_VIDEO_FORMAT_480I = 1,  // ITU-R BT.601-5
      BLURAY_VIDEO_FORMAT_576I = 2,  // ITU-R BT.601-4
      BLURAY_VIDEO_FORMAT_480P = 3,  // SMPTE 293M
      BLURAY_VIDEO_FORMAT_1080I = 4,  // SMPTE 274M
      BLURAY_VIDEO_FORMAT_720P = 5,  // SMPTE 296M
      BLURAY_VIDEO_FORMAT_1080P = 6,  // SMPTE 274M
      BLURAY_VIDEO_FORMAT_576P = 7   // ITU-R BT.1358
    }

    [Flags]
    protected enum MediaType
    {
      None = 0,
      Video = 1,
      Audio = 2
    }

    #endregion

    #region decoder GUIDs

    private static readonly Guid MEDIASUBTYPE_WVC1_CYBERLINK = new Guid(0xD979F77B, 0xDBEA, 0x4BF6, 0x9E, 0x6D, 0x1D, 0x7E, 0x57, 0xFB, 0xAD, 0x53);
    private static readonly Guid MEDIASUBTYPE_WVC1_ARCSOFT = new Guid(0x629B40AD, 0xAD74, 0x4EF4, 0xA9, 0x85, 0xF0, 0xC8, 0xD9, 0x2E, 0x5E, 0xCA);

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
    protected double _currentPos;
    protected double _duration = -1d;
    protected DsLong _currentPosDS;
    protected DsLong _durationDS = -1;

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
    protected MediaType _mChangedMediaType;
    protected int _lastFrameCounter;    
    protected bool _bMediaTypeChanged;
    protected int _currentTitle = 0xffff;
    protected int _currentChapter = 0xffff;
    protected int _currentSubtitleStream = 0xfff;
    protected int _currentAudioStream = 0;
    protected EventBuffer eventBuffer = new EventBuffer();
    protected MenuItems menuItems = MenuItems.All;
    protected double[] chapters;
    protected BDFilterConfig filterConfig;
    protected int _currentVideoFormat;
    protected int _currentAudioFormat;
    protected int _currentVideoFormatRate;
    protected static BDPlayerSettings settings;
    protected MenuState menuState;
    protected bool _subtitlesEnabled = true;
    protected bool _bPopupMenuAvailable = true;
    #endregion

    #region ctor/dtor

    public BDPlayer()
      : this(g_Player.MediaType.Video)
    {

    }

    public BDPlayer(g_Player.MediaType mediaType)
    {      
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
            if (menuState == MenuState.None)
              return false;
            int x = (int)((action.fAmount1 - PlaneScene.DestRect.X) / ((float)PlaneScene.DestRect.Width / 1920.0f));
            int y = (int)((action.fAmount2 - PlaneScene.DestRect.Y) / ((float)PlaneScene.DestRect.Height / 1080.0f));
            //Log.Debug("BDPlayer: Mouse move: {0},{1}", x, y);
            _ireader.MouseMove(x, y);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOUSE_CLICK:
            if (menuState == MenuState.None)
              return false;
            Log.Debug("BDPlayer: Mouse select");
            _ireader.Action((int)BDKeys.BD_VK_MOUSE_ACTIVATE);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_LEFT:
            if (menuState == MenuState.None)
              return false;
            Log.Debug("BDPlayer: Move left");
            _ireader.Action((int)BDKeys.BD_VK_LEFT);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_RIGHT:
            if (menuState == MenuState.None)
              return false;
            Log.Debug("BDPlayer: Move right");
            _ireader.Action((int)BDKeys.BD_VK_RIGHT);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_UP:
            if (menuState == MenuState.None)
              return false;
            Log.Debug("BDPlayer: Move up");
            _ireader.Action((int)BDKeys.BD_VK_UP);
            return true;

          case GUI.Library.Action.ActionType.ACTION_MOVE_DOWN:
            if (menuState == MenuState.None)
              return false;
            Log.Debug("BDPlayer: Move down");
            _ireader.Action((int)BDKeys.BD_VK_DOWN);
            return true;

          case GUI.Library.Action.ActionType.ACTION_SELECT_ITEM:
            if (menuState == MenuState.None)
              return false;
            Log.Debug("BDPlayer: Select");
            _ireader.Action((int)BDKeys.BD_VK_ENTER);
            return true;

          case GUI.Library.Action.ActionType.ACTION_DVD_MENU:
            if (!Playing || _forceTitle || menuState != MenuState.None)
              return true;
            Speed = 1;
            //Log.Debug("BDPlayer: Main menu");
            if (_ireader.Action((int)BDKeys.BD_VK_ROOT_MENU) == 0)
              menuState = MenuState.Root;
            return true;

          case GUI.Library.Action.ActionType.ACTION_BD_POPUP_MENU:
            if (!Playing || _forceTitle || !_bPopupMenuAvailable)
              return true;
            Speed = 1;
            //Log.Debug("BDPlayer: Popup menu toggle");
            if (_ireader.Action((int)BDKeys.BD_VK_POPUP) == 0)
              menuState = MenuState.PopUp;
            return true;

          case GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
            if (menuState != MenuState.PopUp)
              return false;
            Speed = 1;
            //Log.Debug("BDPlayer: Popup menu off");
            _ireader.Action((int)BDKeys.BD_VK_POPUP);
            return true;

          case GUI.Library.Action.ActionType.ACTION_NEXT_CHAPTER:
            if (!Playing || _currentChapter == 0xffff)
              return true;
            Speed = 1;
            Log.Debug("BDPlayer: NextChapter, current: {0}", _currentChapter + 1);
            _ireader.SetChapter((uint)_currentChapter + 1);
            return true;

          case GUI.Library.Action.ActionType.ACTION_PREV_CHAPTER:
            if (!Playing || _currentChapter == 0xffff)
              return true;
            Speed = 1;
            Log.Debug("BDPlayer: PrevChapter, current: {0}", _currentChapter + 1);
            if (_currentChapter > 0)
              _ireader.SetChapter((uint)_currentChapter - 1);
            else if (_currentChapter == 0)
              _ireader.SetChapter(0);
            return true;
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
      return _state == PlayState.Playing && menuState == MenuState.None;      
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
          Log.Warn("BDPlayer: Unable to set CurrentAudioStream -> value does not exist");
          return;
        }
        if (_interfaceBDReader == null)
        {
          Log.Warn("BDPlayer: Unable to set CurrentAudioStream -> BDReader not initialized");
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
          Log.Warn("BDPlayer: Unable to set CurrentAudioStream -> IAMStreamSelect == null");
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
        Log.Warn("BDPlayer: Unable to get AudioType -> BDReader not initialized");
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
        return StreamTypetoString(sPDWGroup);        
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
        Log.Warn("BDPlayer: Unable to get AudioLanguage -> BDReader not initialized");
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
      return Strings.Unknown;
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
        Log.Debug("BDPlayer:VideoSize:{0}x{1}", _videoWidth, _videoHeight);
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
      _state = PlayState.Playing;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
      msg.Label = strFile;
      GUIWindowManager.SendThreadMessage(msg);
      _positionX = GUIGraphicsContext.VideoWindow.X;
      _positionY = GUIGraphicsContext.VideoWindow.Y;
      _width = GUIGraphicsContext.VideoWindow.Width;
      _height = GUIGraphicsContext.VideoWindow.Height;
      _geometry = GUIGraphicsContext.ARType;
      OnInitialized();
      Log.Debug("BDPlayer: position:{0}, duration:{1}", CurrentPosition, Duration);

      UpdateMenuItems();
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
        _ireader.OnGraphRebuild((int)_mChangedMediaType);
      }

      HandleBDEvent();
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds >= 200 || iSpeed != 1)
      {
        _updateTimer = DateTime.Now;

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
            Log.Debug("BDPlayer: Successfully set rate to {0}", iSpeed);
            if (iSpeed != 1)
            {
              _usingFastSeeking = true;
              if (_mediaCtrl != null) _mediaCtrl.Run();
            }
          }
          else
          {
            Log.Warn("BDPlayer: Could not set rate to {0}, error: 0x{1:x}", iSpeed, hr);
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
      if (menuState != MenuState.None)
        return;

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
      get { return (_state != PlayState.Init && _state != PlayState.Ended); }
    }

    public override bool Stopped
    {
      get { return (_state == PlayState.Init); }
    }

    public override string CurrentFile
    {
      get
      {
        if (_currentFile.Contains("index.bdmv"))
          _currentFile = GetDiscTitle(settings.menuLang);

        return _currentFile;
      }
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
            if (dTimeInSecs < 0)
            {
              dTimeInSecs = 0;
            }
            dTimeInSecs *= 10000000d;

            long lTime = (long)dTimeInSecs;
            long pStop = 0;

            Log.Debug("BDPlayer:seekabs: {0} duration :{1}", dTimeInSecs, Duration);

            int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning,
                                                new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);

            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.FrameCounter = 123;
            }
          }
        }

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
          catch(Exception)
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

    public int OnMediaTypeChanged(int videoRate, int videoFormat, int audioFormat)
    {
      Log.Info("BDPlayer OnMediaTypeChanged() - Video: {0}({1} fps), Audio: {2}", StreamTypetoString(videoFormat), VideoRatetoDouble(videoRate), StreamTypetoString(audioFormat));
      _mChangedMediaType = MediaType.None;

      if (videoFormat != _currentVideoFormat)
      {
        _mChangedMediaType |= MediaType.Video;
        _currentVideoFormat = videoFormat;
      }

      if (audioFormat != _currentAudioFormat)
      {
        _mChangedMediaType |= MediaType.Audio;
        _currentAudioFormat = audioFormat;
      }

      UpdateRefreshRate(videoRate);      

      if (_mChangedMediaType != MediaType.None)
      {
        _bMediaTypeChanged = Playing ? true : false;
      }
      return _bMediaTypeChanged ? 0 : 1;
    }  

    public int OnBDevent(BDEvent bdevent)
    {
      if (bdevent.Event != 0 && 
        bdevent.Event != (int)BDEvents.BD_EVENT_STILL &&
        bdevent.Event != (int)BDEvents.BD_EVENT_STILL_TIME)
      {
        eventBuffer.Set(bdevent);
        //Log.Debug("BDPlayer OnBDEvent: {0}, param: {1}", bdevent.Event, bdevent.Param);
      }
      return 0;
    }

    public int OnClockChange(long duration, long position)
    {
      _currentPosDS = position;
      _durationDS = duration;

      _currentPos = position / 10000000.0;
      _duration = duration / 10000000.0;
      
      return 0;
    }

    public static BDPlayerSettings Settings
    {
      get { return settings; }
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
        filterConfig.VideoMPEG = xmlreader.GetValueAsString("bdplayer", "mpeg2videocodec", "");
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
      settings = new BDPlayerSettings();

      using (Settings xmlreader = new MPSettings())
      {
        settings.audioLang = xmlreader.GetValueAsString("bdplayer", "audiolanguage", "English");
        settings.subtitleLang = xmlreader.GetValueAsString("bdplayer", "subtitlelanguage", "English");
        settings.parentalControl = xmlreader.GetValueAsInt("bdplayer", "parentalcontrol", 99);
        _subtitlesEnabled = xmlreader.GetValueAsBool("bdplayer", "subtitlesenabled", true);
        string regionCode = xmlreader.GetValueAsString("bdplayer", "regioncode", "B");
        switch (regionCode)
        {
          case "A":
            settings.regionCode = 1;
            break;
          case "B":
            settings.regionCode = 2;
            break;
          case "C":
            settings.regionCode = 4;
            break;
        }
      }

      foreach (CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
      {
        if (String.Compare(cultureInformation.EnglishName, settings.audioLang, true) == 0)
        {
          settings.countryCode = cultureInformation.TwoLetterISOLanguageName;
          settings.audioLang = cultureInformation.ThreeLetterISOLanguageName;
        }
        if (String.Compare(cultureInformation.EnglishName, settings.subtitleLang, true) == 0)
        {
          settings.subtitleLang = cultureInformation.ThreeLetterISOLanguageName;
        }
      }
      settings.menuLang = settings.audioLang;
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
            
      if (chapters.Length > 2) // only two chapters means beginning and end - no real chapters
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
        if (chapters[chapters.Length - 1] < 300) // 5 min sanity check
          chapters = null;
      }
      else
        chapters = null;

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
        List<uint> titleOptions = new List<uint>();

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
          int titleIdx = (int)titleOptions[dialog.SelectedId - 2];
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
              CurrentAudioStream = bdevent.Param - 1;
            break;

          case (int)BDEvents.BD_EVENT_PG_TEXTST:
            Log.Debug("BDPlayer: Subtitles available {0}", bdevent.Param);
            break;

          case (int)BDEvents.BD_EVENT_PG_TEXTST_STREAM:
            Log.Debug("BDPlayer: Subtitle changed to {0}", bdevent.Param);
            if (bdevent.Param != 0xfff)
              CurrentSubtitleStream = bdevent.Param;
            break;

          case (int)BDEvents.BD_EVENT_IG_STREAM:
            Log.Debug("BDPlayer: Interactive graphics available {0}", bdevent.Param);
            break;

          case (int)BDEvents.BD_EVENT_PLAYLIST:
            Log.Debug("BDPlayer: Playlist changed to {0}", bdevent.Param);
            if (_forceTitle || (_currentTitle != BLURAY_TITLE_FIRST_PLAY && _currentTitle != BLURAY_TITLE_TOP_MENU))
              UpdateChapters();
            break;

          case (int)BDEvents.BD_EVENT_PLAYITEM:
            Log.Debug("BDPlayer: Playitem changed to {0}", bdevent.Param);
            CurrentStreamInfo();
            _bPopupMenuAvailable = false;
            UpdateMenuItems();
            break;

          case (int)BDEvents.BD_EVENT_TITLE:
            Log.Debug("BDPlayer: Title changed to {0}", bdevent.Param);
            _currentTitle = bdevent.Param;
            _currentChapter = 0xffff;
            break;

          case (int)BDEvents.BD_EVENT_CHAPTER:
            Log.Debug("BDPlayer: Chapter changed to {0}", bdevent.Param);
            if (bdevent.Param != 0xffff)
              _currentChapter = bdevent.Param - 1;
            break;

          case (int)BDEvents.BD_EVENT_POPUP:
            Log.Debug("BDPlayer: Popup available {0}", bdevent.Param);
            _bPopupMenuAvailable = bdevent.Param == 1 ? true : false;
            UpdateMenuItems();
            break;          

          case (int)BDEvents.BD_EVENT_MENU:
            Log.Debug("BDPlayer: Menu available {0}", bdevent.Param);
            if (bdevent.Param == 1)
            {
              if (menuState != MenuState.PopUp)
                menuState = MenuState.Root;
             
              GUIGraphicsContext.DisableTopBar = true;
            }
            else
            {              
              menuState = MenuState.None;
              GUIGraphicsContext.DisableTopBar = false;
              GUIGraphicsContext.TopBarHidden = true;   
            }
            UpdateMenuItems();
            break;
        }
      }
    }

    protected void UpdateMenuItems()
    {
      if (_forceTitle)
        {
          menuItems = MenuItems.Chapter | MenuItems.Audio | MenuItems.Subtitle;
          return;
        }
      
      if (menuState == MenuState.Root)
      {        
        menuItems = MenuItems.None;
        return;
      }

      if (menuState == MenuState.PopUp)
      {
        menuItems = MenuItems.All;
        return;
      }

      if (chapters != null && _currentTitle != BLURAY_TITLE_FIRST_PLAY && _currentTitle != BLURAY_TITLE_TOP_MENU)
        if (_bPopupMenuAvailable)
          menuItems = MenuItems.All;
        else
          menuItems = MenuItems.Audio | MenuItems.Chapter | MenuItems.MainMenu | MenuItems.Subtitle;
      else
        menuItems = MenuItems.MainMenu;
    }

    protected void CurrentStreamInfo()
    {
      try
      {
        BDStreamInfo clipInfo = new BDStreamInfo();
        _ireader.GetCurrentClipStreamInfo(ref clipInfo);
        Log.Debug("BDPlayer: CurrentStreamInfo - video format: {0}({1})@{2}fps, duration: {3}", StreamTypetoString(clipInfo.coding_type), VideoFormattoString(clipInfo.format), VideoRatetoDouble(clipInfo.rate), _duration);
        UpdateRefreshRate(clipInfo.rate);
      }
      catch
      {
        Log.Error("BDPlayer: CurrentStreamInfo() failed.");
      }      
    }

    protected void UpdateRefreshRate(int videoRate)
    {
      if (_currentVideoFormatRate != videoRate && _duration > 300)
      {
        _currentVideoFormatRate = videoRate;
        RefreshRateChanger.SetRefreshRateBasedOnFPS(VideoRatetoDouble(videoRate), "", RefreshRateChanger.MediaType.Video);
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
          Log.Error("BDPlayer: Error stopping graph: {0}", error.Message);
          MovieEnded();
        }

        // this is a hack for MS Video Decoder and AC3 audio change
        // would suggest to always do full audio and video rendering for all filters
        IBaseFilter MSVideoCodec = null;
        _graphBuilder.FindFilterByName("Microsoft DTV-DVD Video Decoder", out MSVideoCodec);
        if (MSVideoCodec != null)
        {
          _mChangedMediaType = MediaType.Audio | MediaType.Video;
          DirectShowUtil.ReleaseComObject(MSVideoCodec); MSVideoCodec = null;
        }
        // hack end
        switch (_mChangedMediaType)
        {
          case MediaType.Audio: // audio changed
            Log.Info("BDPlayer: Rerendering audio pin of BDReader filter.");
            UpdateFilters("Audio");
            break;
          case MediaType.Video: // video changed
            Log.Info("BDPlayer: Rerendering video pin of BDReader filter.");
            UpdateFilters("Video");
            break;
          case MediaType.Audio | MediaType.Video: // both changed
            Log.Info("BDPlayer: Rerendering audio and video pins of BDReader filter.");
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
          Log.Error("BDPlayer: Error starting graph: {0}", error.Message);
          MovieEnded();
          return;
        }
        Log.Info("BDPlayer: Reconfigure graph done");
      }
    }

    protected void OnGraphNotify()
    {
      int param1, param2;
      EventCode eventCode;
      while (_mediaEvt != null && _mediaEvt.GetEvent(out eventCode, out param1, out param2, 0) >= 0)
      {
        _mediaEvt.FreeEventParams(eventCode, param1, param2);
        switch (eventCode)
        {
          case EventCode.Complete:
            // Ignore graph complete in menu mode - user must actively stop the player.
            // Abort code is sent when the playback runs into error (broken BD etc.)
            if (_forceTitle)
            {
              Log.Debug("BDPlayer - GraphNotify: Complete");
              MovieEnded();
            }
            break;
          case EventCode.ErrorAbort:
            Log.Debug("BDPlayer - GraphNotify: Error: {0}", param1);
            MovieEnded();
            break;
          default:
            break;
        }        
      }
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
      long rewind, pStop;
      lock (_mediaCtrl)
      {

        //Log.Info(" time from last : {6} {7} {8} earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
        //         earliest / 10000000, latest / 10000000, current / 10000000, stop / 10000000, _speedRate, (latest - earliest) / 10000000, (long)ts.TotalMilliseconds, VMR9Util.g_vmr9.FreeFrameCounter, Speed);

        // new time = current time + timerinterval * speed
        long lTimerInterval = (long)ts.TotalMilliseconds;
        if (lTimerInterval > 1000)
        {
          lTimerInterval = 1000;
        }
        rewind = _currentPosDS + (long)lTimerInterval * Speed * 10000;
        int hr;
        pStop = 0;
        // if we end up before the first moment of time then just
        // start @ the beginning
        if ((rewind < 0) && (iSpeed < 0))
        {
          rewind = 0;
          Log.Info("BDPlayer: seek to start");
          hr = _mediaSeeking.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                          new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          Speed = 1;
          return;
        }
        // if we end up at the end of time then just
        // start @ the end -1sec
        long margin = 100000;


        if ((rewind > (_durationDS - margin)) && (iSpeed > 0))
        {
          Log.Info("BDPlayer: Fastforward reached the end of file, stopping playback");
          //_state = PlayState.Ended;
          return;
        }
        // seek to new moment in time
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
        if (_currentVideoFormat == (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_MPEG2)
        {
          return filterConfig.VideoMPEG;
        }
        else if (_currentVideoFormat == (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_VC1)
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
        //if (_currentAudioFormat == MEDIASUBTYPE_AAC_AUDIO)
        {
        //  return filterConfig.AudioAAC;
        }
        //else if (_currentAudioFormat == MEDIASUBTYPE_DDPLUS_AUDIO)
        {
        //  return filterConfig.AudioDDPlus;
        }
        //else
        {
          return filterConfig.Audio;
        }
      }
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

        Log.Debug("BDPlayer: Add BDReader to graph");

        IFileSourceFilter interfaceFile = (IFileSourceFilter)_interfaceBDReader;

        LoadSettings(_ireader);
        _ireader.SetD3DDevice(DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device));
        _ireader.SetBDReaderCallback(this);

        hr = interfaceFile.Load(filename, null);

        DsError.ThrowExceptionForHR(hr);

        Log.Debug("BDPlayer: BDReader loaded: {0}", filename);

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

          Log.Debug("BDPlayer: Starting BDReader");
          eventBuffer.Clear();
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
            Log.Info("BDPlayer: BDReader started");
          }

          break;
        }

        #region Filters

        Log.Info("BDPlayer: Adding filters");

        _vmr9 = new VMR9Util();
        _vmr9.AddVMR9(_graphBuilder);
        _vmr9.Enable(false);

        // Set VideoDecoder and VC1Override before adding filter in graph
        SetVideoDecoder();
        SetVC1Override();

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
        EnableSubtitle = _subtitlesEnabled;
        
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

    private void SetVideoDecoder()
    {
      ExportGuidFilterAndRelease(filterConfig.VideoH264, BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_H264);

      if (filterConfig.VideoMPEG != filterConfig.VideoH264)
      {
        ExportGuidFilterAndRelease(filterConfig.VideoMPEG, BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_MPEG2);
      }
      else
      {
        ExportGuidFilterAndRelease(filterConfig.VideoH264, BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_MPEG2);
      }

      if (filterConfig.VideoVC1 != filterConfig.VideoH264 || filterConfig.VideoVC1 != filterConfig.VideoMPEG)
      {
        ExportGuidFilterAndRelease(filterConfig.VideoVC1, BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_VC1);
      }
      else
      {
        ExportGuidFilterAndRelease(filterConfig.VideoH264, BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_VC1);
      }
    }

    private void ExportGuidFilterAndRelease(string filter, BluRayStreamFormats BDStream)
    {
      Guid guid;
      IBaseFilter dsfilter;
      dsfilter = DirectShowUtil.GetFilterByName(_graphBuilder, filter);
      if (dsfilter == null)
        dsfilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, filter);
      dsfilter.GetClassID(out guid);
      _ireader.SetVideoDecoder((int)BDStream, ref guid);
      _graphBuilder.RemoveFilter(dsfilter);
      DirectShowUtil.ReleaseComObject(dsfilter);
      dsfilter = null;
    }

    protected void SetVC1Override()
    {
      Guid guid = new Guid();

      if (filterConfig.VideoVC1.StartsWith("ArcSoft Video Decoder"))
      {
        guid = MEDIASUBTYPE_WVC1_ARCSOFT;
      }
      else if (filterConfig.VideoVC1.StartsWith("CyberLink Video Decoder") ||
               filterConfig.VideoVC1.StartsWith("CyberLink H.264/AVC Decoder") ||
               filterConfig.VideoVC1.StartsWith("CyberLink VC-1 Decoder"))
      {
        guid = MEDIASUBTYPE_WVC1_CYBERLINK;
      }
      
      _ireader.SetVC1Override(ref guid);
    }

    /// <summary> do cleanup and release DirectShow. </summary>
    protected void CloseInterfaces()
    {
      if (_graphBuilder == null)
      {
        return;
      }
      int hr;
      Log.Debug("BDPlayer: Cleanup DShow graph {0}", GUIGraphicsContext.InVmr9Render);
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

        if (_vmr9 != null)
        {
          _vmr9.Enable(false);
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

        if (_vmr9 != null)
        {
          _vmr9.SafeDispose();
          _vmr9 = null;
        }

        GUIGraphicsContext.form.Invalidate(true);
        _state = PlayState.Init;
      }
      catch (Exception ex)
      {
        Log.Error("BDPlayer: Exception while cleaning DShow graph - {0} {1}", ex.Message, ex.StackTrace);
      }
      //switch back to directx windowed mode
      ExclusiveMode(false);
    }

    protected void ExclusiveMode(bool bOnOff)
    {
      GUIMessage msg = null;
      if (bOnOff)
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

    protected string GetDiscTitle(string language)
    {
      string discTitle = string.Empty;

      if (Directory.Exists(_currentFile.Replace("index.bdmv", @"META\DL")))
      {
        string[] xmls = Directory.GetFiles(_currentFile.Replace("index.bdmv", @"META\DL"), "bdmt*.xml", SearchOption.TopDirectoryOnly);

        foreach (string xml in xmls)
        {
          if (xml.Contains(language) || xml.Contains("eng"))
          {
            System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(xml);
            reader.WhitespaceHandling = System.Xml.WhitespaceHandling.Significant;

            while (reader.Read())
            {
              if (reader.Name == "di:name")
              {
                reader.Read();
                if (xml.Contains(language))
                  return reader.Value;
                else
                  discTitle = reader.Value;
                break;
              }
            }
            break;
          }
        }

        if (!String.IsNullOrEmpty(discTitle))
          return discTitle;
      }

      if (Util.Utils.IsDVD(_currentFile))
        Util.Utils.GetDVDLabel(_currentFile, out discTitle);

      if (String.IsNullOrEmpty(discTitle))
      {
        discTitle = _currentFile.Remove(_currentFile.IndexOf(@"\BDMV"));
        discTitle = discTitle.Substring(discTitle.LastIndexOf(@"\") + 1);
      }
      discTitle = discTitle.Replace("_", " ");

      return String.IsNullOrEmpty(discTitle) ? _currentFile : discTitle;
    }

    protected double VideoRatetoDouble(int videoRate)
    {
      switch (videoRate)
      {
        case 1:
          return 23.976;
        case 2:
          return 24;
        case 3:
          return 25;
        case 4:
          return 29.97;
        case 6:
          return 50;
        case 7:
          return 59.94;
        default:
          return 0;
      }
    }

    protected string VideoFormattoString(int videoFormat)
    {
      switch (videoFormat)
      {
        case 1:
          return "480i";
        case 2:
          return "576i";
        case 3:
          return "480p";
        case 4:
          return "1080i";
        case 6:
          return "1080p";
        case 7:
          return "576p";
        default:
          return Strings.Unknown;
      }
    }

    protected string StreamTypetoString(int stream)
    {
      switch (stream)
      { 
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_AC3:
          return "AC-3";          
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_AC3PLUS:
          return "DD+";          
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_DTS:
          return "DTS";          
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_DTSHD:
          return "DTS-HD";          
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_DTSHD_MASTER:
          return "DTS-HD Master";
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_LPCM:
          return "LPCM";
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_MPEG1:
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_MPEG1:
          return "MPEG1";
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_MPEG2:
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_MPEG2:
          return "MPEG2";
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_AUDIO_TRUHD:
          return "TrueHD";
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_H264:
          return "H264";
        case (int)BluRayStreamFormats.BLURAY_STREAM_TYPE_VIDEO_VC1:
          return "VC1";
      }
      return Strings.Unknown;
    }
    #endregion

    #region IDisposable Members

    public override void Dispose()
    {
      CloseInterfaces();
    }

    #endregion
  }
}
