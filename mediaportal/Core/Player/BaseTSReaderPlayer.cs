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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using MediaPortal.Player.Subtitles;
using MediaPortal.Profile;
using MediaPortal.Player.PostProcessing;
using System.Collections;
using System.Collections.Generic;

namespace MediaPortal.Player
{
  [ComVisible(true), ComImport,
   Guid("324FAA1F-4DA6-47B8-832B-3993D8FF4151"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITSReaderCallback
  {
    [PreserveSig]
    int OnMediaTypeChanged(int mediaType);

    [PreserveSig]
    int OnVideoFormatChanged(int streamType, int width, int height, int aspectRatioX, int aspectRatioY, int bitrate,
                             int isInterlaced);
  }

  [ComVisible(true), ComImport,
   Guid("324FAA1F-4DA6-47B8-832B-3993D8FF4152"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ITSReaderCallbackAudioChange
  {
    [PreserveSig]
    int OnRequestAudioChange();
  }

  internal class BaseTSReaderPlayer : IPlayer, ITSReaderCallback, ITSReaderCallbackAudioChange
  {
    [Guid("b9559486-E1BB-45D3-A2A2-9A7AFE49B24F"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    protected interface ITSReader
    {
      [PreserveSig]
      int SetTsReaderCallback(ITSReaderCallback callback);

      [PreserveSig]
      int SetRequestAudioChangeCallback(ITSReaderCallbackAudioChange callback);

      [PreserveSig]
      int SetRelaxedMode(int relaxedReading);

      [PreserveSig]
      int OnZapping(int info);

      [PreserveSig]
      int OnGraphRebuild(int info);

      [PreserveSig]
      int SetMediaPosition(long MediaPos);
    }

    private int DSERR_NODRIVER = -2005401480; // No sound driver is available for use

    [ComImport, Guid("b9559486-E1BB-45D3-A2A2-9A7AFE49B23F")]
    protected class TsReader { }

    #region imports

    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int SetupDemuxerPin(IPin pin, int pid, int elementaryStream, bool unmapOtherPins);

    [DllImport("dvblib.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
    protected static extern int DumpMpeg2DemuxerMappings(IBaseFilter filter);

    #endregion

    #region enums

    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended
    }

    #endregion

    public delegate void AudioTracksReadyHandler();

    #region variables

    protected ITSReader _ireader = null;
    protected int iSpeed = 1;
    protected IBaseFilter _fileSource = null;
    protected int _curAudioStream = 0;
    protected int _positionX = 0;
    protected int _positionY = 0;
    protected int _width = 200;
    protected int _height = 100;
    protected int _videoWidth = 100;
    protected int _videoHeight = 100;
    protected string _currentFile = "";
    protected bool _updateNeeded = false;
    protected bool _isFullscreen = true;
    protected PlayState _state = PlayState.Init;
    protected int _volume = 100;
    protected int _volumeBeforeSeeking = 0;
    protected IGraphBuilder _graphBuilder = null;
    protected IMediaSeeking _mediaSeeking = null;
    protected int _speed = 1;
    protected double _currentPos;
    protected double _streamPos;
    protected double _duration = -1d;
    protected bool _isStarted = false;
    protected bool _isLive = false;
    protected double _lastPosition = 0;
    protected bool _isWindowVisible = false;
    protected DsROTEntry _rotEntry = null;
    protected int _aspectX = 1;
    protected int _aspectY = 1;
    protected long _speedRate = 10000;
    protected bool _CodecSupportsFastSeeking = true;
    protected bool _usingFastSeeking = false;
    protected IBaseFilter _interfaceTSReader = null;
    protected SubtitleSelector _subSelector = null;
    protected SubtitleRenderer _dvbSubRenderer = null;
    protected AudioSelector _audioSelector = null;
    protected IAMLine21Decoder _line21DecoderAnalog = null;
    protected IAMLine21Decoder _line21DecoderDigital = null;

    protected string CoreParserCodec = "";
    protected string audioRendererFilter = "";
    protected string AudioCodecName = "";
    public Guid LATMAAC = new Guid("000000ff-0000-0010-8000-00aa00389b71");
    public Guid AVC1 = new Guid("31435641-0000-0010-8000-00AA00389B71");

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

    private DateTime _elapsedTimer = DateTime.Now;
    private DateTime _FFRWtimer = DateTime.Now;
    protected const int WM_GRAPHNOTIFY = 0x00008001; // message from graph
    protected const int WS_CHILD = 0x40000000; // attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;
    protected DateTime _updateTimer = DateTime.Now;
    protected Geometry.Type _geometry = Geometry.Type.Normal;
    protected bool _endOfFileDetected = false;
    protected bool _startingUp;
    protected bool _isRadio = false;
    protected g_Player.MediaType _mediaType;
    protected int iChangedMediaTypes;
    protected VideoStreamFormat _videoFormat;
    protected int _lastFrameCounter;
    protected string videoFilter = "";
    protected string audioFilter = "";
    protected bool CoreCCPresent = false;
    protected bool CoreCCFilter = false;
    protected bool VideoChange = false;

    protected Dictionary<string, object> PostProcessFilterVideo = new Dictionary<string, object>();
    protected Dictionary<string, object> PostProcessFilterAudio = new Dictionary<string, object>();
    //protected ArrayList availableVideoFilters = FilterHelper.GetFilters(MediaType.Video, MediaSubType.Null);
    //protected ArrayList availableAudioFilters = FilterHelper.GetFilters(MediaType.Audio, MediaSubType.Null);

    public TVFilterConfig filterConfig;
    public FilterCodec filterCodec;

    #endregion

    #region ctor/dtor

    public BaseTSReaderPlayer()
    {
      _mediaType = g_Player.MediaType.Video;
      _videoFormat = new VideoStreamFormat();
    }

    public BaseTSReaderPlayer(g_Player.MediaType type)
    {
      _isRadio = false;
      if (type == g_Player.MediaType.Radio)
      {
        _isRadio = true;
      }
      _mediaType = type;
      _videoFormat = new VideoStreamFormat();
    }

    #endregion

    #region public members

    public class FilterCodec
    {
      public IBaseFilter _audioSwitcherFilter { get; set; }
      public IBaseFilter _audioRendererFilter { get; set; }
      public IBaseFilter VideoCodec { get; set; }
      public IBaseFilter AudioCodec { get; set; }
      public IBaseFilter CoreCCParser { get; set; }
      public IBaseFilter line21CoreCCParser { get; set; }
      public IBaseFilter line21VideoCodec { get; set; }
    }

    public virtual FilterCodec GetFilterCodec()
    {
      FilterCodec filterCodec = new FilterCodec();

      filterCodec._audioSwitcherFilter = null;
      filterCodec._audioRendererFilter = null;
      filterCodec.VideoCodec = null;
      filterCodec.AudioCodec = null;
      filterCodec.CoreCCParser = null;
      filterCodec.line21CoreCCParser = null;
      filterCodec.line21VideoCodec = null;

      return filterCodec;
    }

    public class TVFilterConfig
    {
      public TVFilterConfig()
      {
        OtherFilters = new List<string>();
      }

      public string Video { get; set; }
      public string VideoH264 { get; set; }
      public string Audio { get; set; }
      public string AudioAAC { get; set; }
      public string AudioDDPlus { get; set; }
      public string AudioRenderer { get; set; }
      public bool enableDVBBitmapSubtitles { get; set; }
      public bool enableDVBTtxtSubtitles { get; set; }
      public bool enableCCSubtitles { get; set; }
      public bool enableMPAudioSwitcher { get; set; }
      public bool autoShowSubWhenTvStarts { get; set; }
      public int relaxTsReader { get; set; }
      public bool _CodecSupportsFastSeeking { get; set; }
      public List<string> OtherFilters { get; set; }
      public Geometry.Type AR { get; set; }
    }

    /// <summary>
    /// Gets the filter configuration object from the user configuration
    /// </summary>
    /// <returns></returns>
    protected virtual TVFilterConfig GetFilterConfiguration()
    {
      TVFilterConfig filterConfig = new TVFilterConfig();

      using (Settings xmlreader = new MPSettings())
      {
        // get pre-defined filter setup
        filterConfig.Video = xmlreader.GetValueAsString("mytv", "videocodec", "");
        filterConfig.Audio = xmlreader.GetValueAsString("mytv", "audiocodec", "");
        filterConfig.AudioAAC = xmlreader.GetValueAsString("mytv", "aacaudiocodec", "");
        filterConfig.AudioDDPlus = xmlreader.GetValueAsString("mytv", "ddplusaudiocodec", "");
        filterConfig.VideoH264 = xmlreader.GetValueAsString("mytv", "h264videocodec", "");
        filterConfig.AudioRenderer = xmlreader.GetValueAsString("mytv", "audiorenderer", "Default DirectSound Device");
        filterConfig.enableDVBBitmapSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbbitmapsubtitles", false);
        filterConfig.enableDVBTtxtSubtitles = xmlreader.GetValueAsBool("tvservice", "dvbttxtsubtitles", false);
        filterConfig.enableCCSubtitles = xmlreader.GetValueAsBool("tvservice", "ccsubtitles", false);
        filterConfig.enableMPAudioSwitcher = xmlreader.GetValueAsBool("tvservice", "audiodualmono", false);
        filterConfig.relaxTsReader = (xmlreader.GetValueAsBool("mytv", "relaxTsReader", false) == false ? 0 : 1);
        filterConfig.autoShowSubWhenTvStarts = xmlreader.GetValueAsBool("tvservice", "autoshowsubwhentvstarts", true);

        // as int for passing through interface
        string strValue = xmlreader.GetValueAsString("mytv", "defaultar", "Normal");
        filterConfig._CodecSupportsFastSeeking = xmlreader.GetValueAsBool("debug", "CodecSupportsFastSeeking", true);

        // get post-processing filter setup
        int i = 0;
        while (xmlreader.GetValueAsString("mytv", "filter" + i, "undefined") != "undefined")
        {
          if (xmlreader.GetValueAsBool("mytv", "usefilter" + i, false))
          {
            filterConfig.OtherFilters.Add(xmlreader.GetValueAsString("mytv", "filter" + i, "undefined"));
          }
          i++;
        }

        // get AR setting
        filterConfig.AR = Util.Utils.GetAspectRatio(xmlreader.GetValueAsString("mytv", "defaultar", "Normal"));
        GUIGraphicsContext.ARType = Util.Utils.GetAspectRatio(strValue);
      }

      return filterConfig;
    }

    /// <summary>
    /// Implements the AudioStreams member which interfaces the TsReader filter to get the IAMStreamSelect interface for enumeration of available streams
    /// </summary>
    public override int AudioStreams
    {
      get
      {
        if (_interfaceTSReader == null)
        {
          Log.Info("TSReaderPlayer: Unable to get AudioStreams -> TSReader not initialized");
          return 0;
        }

        int streamCount = 0;
        IAMStreamSelect pStrm = _interfaceTSReader as IAMStreamSelect;
        if (pStrm != null)
        {
          pStrm.Count(out streamCount);
          pStrm = null;
        }
        return streamCount;
      }
    }

    /// <summary>
    /// Implements the CurrentAudioStream member which interfaces the TsReader filter to get the IAMStreamSelect interface for enumeration and switching of available audio streams
    /// </summary>
    public override int CurrentAudioStream
    {
      get { return _curAudioStream; }
      set
      {
        //_curAudioStreamRequest = value; // in tvhome, when we set currentaudiostream, tsreader might not be ready yet, so we wait for the "OnRequestAudioChange" callback and then set it.
        if (value > AudioStreams)
        {
          Log.Info("TSReaderPlayer: Unable to set CurrentAudioStream -> value does not exist");
          return;
        }
        if (_interfaceTSReader == null)
        {
          Log.Info("TSReaderPlayer: Unable to set CurrentAudioStream -> TSReader not initialized");
          return;
        }
        IAMStreamSelect pStrm = _interfaceTSReader as IAMStreamSelect;
        if (pStrm != null)
        {
          pStrm.Enable(value, AMStreamSelectEnableFlags.Enable);
          _curAudioStream = value;
        }
        else
        {
          Log.Info("TSReaderPlayer: Unable to set CurrentAudioStream -> IAMStreamSelect == null");
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

      if (_interfaceTSReader == null)
      {
        Log.Info("TSReaderPlayer: Unable to get AudioType -> TSReader not initialized");
        return Strings.Unknown;
      }
      IAMStreamSelect pStrm = _interfaceTSReader as IAMStreamSelect;
      if (pStrm != null)
      {
        AMMediaType sType;
        AMStreamSelectInfoFlags sFlag;
        int sPDWGroup, sPLCid;
        string sName;
        object pppunk, ppobject;
        pStrm.Info(iStream, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);
        /*
        if (IsTimeShifting)
        {
          // The offset +2 is necessary because the first 2 streams are always non-audio and the following are the audio streams
          pStrm.Info(iStream + 1, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);
        }
        else
        {
          pStrm.Info(iStream, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);
        }
        */
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
    /// Implements the AudioLanguage member which interfaces the TsReader filter to get the IAMStreamSelect interface for getting info about a stream
    /// </summary>
    /// <param name="iStream"></param>
    /// <returns></returns>
    public override string AudioLanguage(int iStream)
    {
      if ((iStream + 1) > AudioStreams)
      {
        return Strings.Unknown;
      }

      if (_interfaceTSReader == null)
      {
        Log.Info("TSReaderPlayer: Unable to get AudioLanguage -> TSReader not initialized");
        return Strings.Unknown;
      }

      IAMStreamSelect pStrm = _interfaceTSReader as IAMStreamSelect;
      if (pStrm != null)
      {
        AMMediaType sType;
        AMStreamSelectInfoFlags sFlag;
        int sPDWGroup, sPLCid;
        string sName;
        object pppunk, ppobject;

        pStrm.Info(iStream, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);
        /*
        if (IsTimeShifting)
        {
          // The offset +2 is necessary because the first 2 streams are always non-audio and the following are the audio streams
          pStrm.Info(iStream + 2, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);
        }
        else
        {
          pStrm.Info(iStream, out sType, out sFlag, out sPLCid, out sPDWGroup, out sName, out pppunk, out ppobject);
        }
        */
        sName = sName.Trim();
        if (sName.Length > 3) //dual language track
        {
          string lang = GetFullLanguageName(sName.Substring(0, 3));
          sName = string.Format("{0}/{1} ({2})", lang, GetFullLanguageName(sName.Substring(3, 3)), sName);
        }
        else
          sName = string.Format("{0} ({1})", GetFullLanguageName(sName), sName);

        return sName;
      }
      else
      {
        return Strings.Unknown;
      }
    }

    private string GetFullLanguageName(string language)
    {
      foreach (
        System.Globalization.CultureInfo ci in
          System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures))
      {
        if (language.Equals(ci.ThreeLetterISOLanguageName) ||
            ci.EnglishName.StartsWith(language, StringComparison.InvariantCultureIgnoreCase))
        {
          language = Util.Utils.TranslateLanguageString(ci.EnglishName);
          break;
        }
      }
      return language;
    }

    public override bool SupportsReplay
    {
      get { return false; }
    }

    public override bool Play(string strFile)
    {
      _endOfFileDetected = false;
      Log.Info("TSReaderPlayer play:{0} radio:{1}", strFile, _isRadio);
      if (strFile.ToLower().StartsWith("rtsp:") == false)
      {
        if (!File.Exists(strFile))
        {
          return false;
        }
      }
      Speed = 1;
      iSpeed = 1;
      _speedRate = 10000;
      _isLive = false;
      _duration = -1d;
      if (strFile.ToLower().IndexOf(".tsbuffer") >= 0)
      {
        Log.Info("TSReaderPlayer: live tv");
        _isLive = true;
      }
      if (strFile.ToLower().IndexOf("rtsp") >= 0)
      {
        Log.Info("TSReaderPlayer: live tv");
        _isLive = true;
      }
      ExclusiveMode(true);
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      _isVisible = false;
      _isWindowVisible = false;
      _volume = 100;
      _state = PlayState.Init;
      _currentFile = strFile;
      _isFullscreen = false;
      _geometry = Geometry.Type.Normal;
      _updateNeeded = true;

      Log.Info("TSReaderPlayer:play {0}", strFile);
      _isStarted = false;
      if (!GetInterfaces(strFile))
      {
        Log.Error("TSReaderPlayer:GetInterfaces() failed");
        _currentFile = "";
        CloseInterfaces();
        ExclusiveMode(false);
        return false;
      }
      int hr = _mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
      if (hr < 0)
      {
        Log.Error("TSReaderPlayer:SetNotifyWindow() failed");
        _currentFile = "";
        CloseInterfaces();
        ExclusiveMode(false);
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
          Log.Error("TSReaderPlayer:GetVideoSize() failed");
          _currentFile = "";
          CloseInterfaces();
          ExclusiveMode(false);
          return false;
        }
        Log.Info("TSReaderPlayer:VideoSize:{0}x{1}", _videoWidth, _videoHeight);
      }
      GUIGraphicsContext.VideoSize = new Size(_videoWidth, _videoHeight);
      if (_mediaCtrl == null)
      {
        Log.Error("TSReaderPlayer:_mediaCtrl==null");
        _currentFile = "";
        CloseInterfaces();
        ExclusiveMode(false);
        return false;
      }
      hr = _mediaCtrl.Run();
      if (hr < 0)
      {
        Log.Error("TSReaderPlayer: Unable to start playing");
        _currentFile = "";
        CloseInterfaces();
        ExclusiveMode(false);
        return false;
      }
      //      _interfaceTSReader = _fileSource;
      _startingUp = _isLive;
      GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
      msg.Label = strFile;
      GUIWindowManager.SendThreadMessage(msg);
      _state = PlayState.Playing;
      _positionX = GUIGraphicsContext.VideoWindow.X;
      _positionY = GUIGraphicsContext.VideoWindow.Y;
      _width = GUIGraphicsContext.VideoWindow.Width;
      _height = GUIGraphicsContext.VideoWindow.Height;
      _geometry = GUIGraphicsContext.ARType;
      _updateNeeded = true;
      SetVideoWindow();
      UpdateCurrentPosition();
      UpdateDuration();
      OnInitialized();
      Log.Info("TSReaderPlayer: position:{0}, duration:{1}", CurrentPosition, Duration);
      return true;
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.IsFullScreenVideo != _isFullscreen)
      {
        _isFullscreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }

      if (!_updateNeeded)
      {
        return;
      }
      _updateNeeded = false;
      _isStarted = true;
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
      if (GUIGraphicsContext.IsFullScreenVideo)
      {
        x = _positionX = GUIGraphicsContext.OverScanLeft;
        y = _positionY = GUIGraphicsContext.OverScanTop;
        nw = _width = GUIGraphicsContext.OverScanWidth;
        nh = _height = GUIGraphicsContext.OverScanHeight;
      }
      /*			Log.Info("{0},{1}-{2},{3}  vidwin:{4},{5}-{6},{7} fs:{8}", x,y,nw,nh, 
              GUIGraphicsContext.VideoWindow.Left,
              GUIGraphicsContext.VideoWindow.Top,
              GUIGraphicsContext.VideoWindow.Right,
              GUIGraphicsContext.VideoWindow.Bottom,
              GUIGraphicsContext.IsFullScreenVideo);*/
      if (nw <= 0 || nh <= 0)
      {
        return;
      }
      if (x < 0 || y < 0)
      {
        return;
      }
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
      _aspectX = aspectX;
      _aspectY = aspectY;
      Geometry m_geometry = new Geometry();
      Rectangle rSource, rDest;
      m_geometry.ImageWidth = _videoWidth;
      m_geometry.ImageHeight = _videoHeight;
      m_geometry.ScreenWidth = nw;
      m_geometry.ScreenHeight = nh;
      m_geometry.ARType = GUIGraphicsContext.ARType;
      m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
      m_geometry.GetWindow(aspectX, aspectY, out rSource, out rDest);
      rDest.X += (int)x;
      rDest.Y += (int)y;
      Log.Info("overlay: video WxH  : {0}x{1}", _videoWidth, _videoHeight);
      Log.Info("overlay: video AR   : {0}:{1}", aspectX, aspectY);
      Log.Info("overlay: screen WxH : {0}x{1}", nw, nh);
      Log.Info("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
      Log.Info("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
      Log.Info("overlay: src        : ({0},{1})-({2},{3})",
               rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
      Log.Info("overlay: dst        : ({0},{1})-({2},{3})",
               rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);
      Log.Info("TSStreamBufferPlayer:Window ({0},{1})-({2},{3}) - ({4},{5})-({6},{7})",
               rSource.X, rSource.Y, rSource.Right, rSource.Bottom,
               rDest.X, rDest.Y, rDest.Right, rDest.Bottom);
      if (rSource.Y == 0)
      {
        rSource.Y += 5;
        rSource.Height -= 10;
      }
      SetSourceDestRectangles(rSource, rDest);
      SetVideoPosition(rDest);
      _sourceRectangle = rSource;
      _videoRectangle = rDest;
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
      // with EVR this causes huge (up to one minute) delays to seeking - tourettes
      //if (GUIGraphicsContext.InVmr9Render) return;
      if (_bMediaTypeChanged)
      {
        DoGraphRebuild();
        _ireader.OnGraphRebuild(iChangedMediaTypes);
        _bMediaTypeChanged = false;
      }
      if (_bRequestAudioChange)
      {
        Log.Info("TSReaderPlayer:OnRequestAudioChange()");
        _bRequestAudioChange = false;
        g_Player.OnAudioTracksReady();
      }

      _startingUp = false;
      TimeSpan ts = DateTime.Now - _updateTimer;
      if (ts.TotalMilliseconds >= 50 || iSpeed != 1)
      {
        UpdateCurrentPosition();
        UpdateDuration();
        _updateTimer = DateTime.Now;
      }

      _lastPosition = CurrentPosition;
      if (GUIGraphicsContext.VideoWindow.Width <= 10 && GUIGraphicsContext.IsFullScreenVideo == false)
      {
        _isVisible = false;
      }
      if (GUIGraphicsContext.BlankScreen)
      {
        _isVisible = false;
      }
      if (_isWindowVisible && !_isVisible)
      {
        _isWindowVisible = false;
        //Log.Info("TSReaderPlayer:hide window");
        if (_videoWin != null)
        {
          _videoWin.put_Visible(OABool.False);
        }
      }
      else if (!_isWindowVisible && _isVisible)
      {
        _isWindowVisible = true;
        //Log.Info("TSReaderPlayer:show window");
        if (_videoWin != null)
        {
          _videoWin.put_Visible(OABool.True);
        }
      }
      OnProcess();
      CheckVideoResolutionChanges();
      if ((_currentPos > (_duration - 1.0)) && IsTimeShifting && (iSpeed != 1))
      {
        Log.Info("TSReaderPlayer : timeshifting FFWD/REW : currentPos > duration-1");
        MovieEnded();
      }
      if (_endOfFileDetected && IsTimeShifting)
      {
        UpdateDuration();
        UpdateCurrentPosition();
        double pos = _duration - 1.0; //Continue playing from end of file -1 seconds
        if (pos < 0)
        {
          pos = 0;
        }
        SeekAbsolute(pos);
        Speed = 1;
        _endOfFileDetected = false;
        Log.Info("TSReaderPlayer : timeshift EOF - start play");
      }
      if (_speedRate != 10000)
      {
        DoFFRW();
      }
      else
      {
        _lastFrameCounter = 0;
        _FFRWtimer = DateTime.Now;
      }
      // Workaround for Mantis issue: 0002698: Last frame can get stuck on the screen when TS file playback ends 
      if (_currentPos > _duration && !IsTimeShifting)
      {
        Log.Info("TSReaderPlayer : currentPos > duration");
        MovieEnded();
      }

      // DS can be called only from the application thread.
      // CI menu changes the thread where Process() gets run (nasty :))
      // http://mantis.team-mediaportal.com/view.php?id=2590
      if (_ireader != null && Thread.CurrentThread.Name == "MPMain")
      {
        _ireader.SetMediaPosition((long)(_streamPos * 10000000d));
      }
    }

    public override bool IsTV
    {
      get { return (_mediaType == g_Player.MediaType.TV || _mediaType == g_Player.MediaType.Recording); }
    }

    public override bool IsTimeShifting
    {
      get { return (_isLive && (_mediaType == g_Player.MediaType.TV || _mediaType == g_Player.MediaType.Radio)); }
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
        _updateNeeded = true;
      }
    }

    public override int Speed
    {
      get
      {
        if (_state == PlayState.Init)
        {
          return 1;
        }
        if (_mediaSeeking == null)
        {
          return 1;
        }
        if (_CodecSupportsFastSeeking && _usingFastSeeking)
        {
          return iSpeed;
        }
        switch (_speedRate)
        {
          case -10000:
            return -1;
          case -15000:
            return -2;
          case -30000:
            return -4;
          case -45000:
            return -8;
          case -60000:
            return -16;
          case -75000:
            return -32;
          case 10000:
            return 1;
          case 15000:
            return 2;
          case 30000:
            return 4;
          case 45000:
            return 8;
          case 60000:
            return 16;
          default:
            return 32;
        }
      }
      set
      {
        if (_mediaCtrl == null || _mediaSeeking == null)
          return;
        if (_state != PlayState.Init)
        {
          bool setRateFailed = false;
          if (_CodecSupportsFastSeeking)
          {
            // Inform dshowhelper of playback rate changes
            if (VMR9Util.g_vmr9 != null) VMR9Util.g_vmr9.EVRProvidePlaybackRate((double)value);

            if (iSpeed != value)
            {
              _usingFastSeeking = true;
              int previousSpeed = iSpeed;
              iSpeed = value;

              int hr = _mediaSeeking.SetRate((double)iSpeed);

              FilterState state;
              _mediaCtrl.GetState(100, out state);
              if (state != FilterState.Running)
              {
                _mediaCtrl.Run();
              }

              Log.Info("VideoPlayer:SetRate to:{0} {1:X}", iSpeed, hr);
              if (hr != 0)
              {
                IMediaSeeking oldMediaSeek = _graphBuilder as IMediaSeeking;
                hr = oldMediaSeek.SetRate((double)iSpeed);

                setRateFailed = true;
                _usingFastSeeking = false;
                iSpeed = 0;
                Log.Info("VideoPlayer:SetRate - fast seeking is not supported, falling back to normal seeking");
              }
              if (iSpeed == 1)
              {
                _mediaCtrl.Stop();
                Application.DoEvents();
                Thread.Sleep(200);
                Application.DoEvents();
                _mediaCtrl.Run();
                _speedRate = 10000;
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
          if (!_CodecSupportsFastSeeking || setRateFailed)
          {
            switch ((int)value)
            {
              case -1:
                _speedRate = -10000;
                break;
              case -2:
                _speedRate = -15000;
                break;
              case -4:
                _speedRate = -30000;
                break;
              case -8:
                _speedRate = -45000;
                break;
              case -16:
                _speedRate = -60000;
                break;
              case -32:
                _speedRate = -75000;
                break;

              case 1:
                _speedRate = 10000;
                _mediaCtrl.Run();
                break;
              case 2:
                _speedRate = 15000;
                break;
              case 4:
                _speedRate = 30000;
                break;
              case 8:
                _speedRate = 45000;
                break;
              case 16:
                _speedRate = 60000;
                break;
              default:
                _speedRate = 75000;
                break;
            }
          }
        }
      }
    }

    public override void ContinueGraph()
    {
      if (_mediaCtrl == null)
      {
        return;
      }
      Log.Info("TSReaderPlayer:Continue graph");
      if (VMR9Util.g_vmr9 != null)
      {
        VMR9Util.g_vmr9.Enable(true);
      }

      _mediaCtrl.Run();
      _mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
    }

    public override void PauseGraph()
    {
      if (_mediaCtrl == null)
      {
        return;
      }
      Log.Info("TSReaderPlayer:Pause graph");
      if (VMR9Util.g_vmr9 != null)
      {
        VMR9Util.g_vmr9.Enable(false);
      }

      _mediaEvt.SetNotifyWindow(IntPtr.Zero, WM_GRAPHNOTIFY, IntPtr.Zero);
      _mediaCtrl.Pause();
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
        UpdateCurrentPosition();
        // Ambass : calling Process() in another thread of "MPMain" can cause unexpected re-entrancy, deadlocks, out of sequence execution...
        /*
        if (_duration < 0)
        {
          Process();
        }*/
        return _duration;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        UpdateCurrentPosition();
        return _currentPos;
      }
    }

    public override double StreamPosition
    {
      get { return _streamPos; }
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
        //Log.Info("BaseTSStreamBufferPlayer:resume");
        Speed = 1;
        _mediaCtrl.Run();
        _state = PlayState.Playing;
      }
      else if (_state == PlayState.Playing)
      {
        //Log.Info("BaseTSStreamBufferPlayer:pause");
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
      get { return (_state == PlayState.Playing || _state == PlayState.Paused); }
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
      using (MPSettings xmlwriter = new MPSettings())
      {
        Log.Debug("TSReaderPlayer: Saving subtitle index: {0} ", CurrentSubtitleStream);
        xmlwriter.SetValue("tvservice", "lastsubtitleindex", CurrentSubtitleStream);
      }
      Stop(false);
    }

    public override void Stop(bool keepExclsuiveModeOn)
    {
      // set the current audio stream to the first one
      _curAudioStream = 0;
      if (SupportsReplay)
      {
        Log.Info("TSReaderPlayer:stop (zapping)");
        if (_mediaCtrl == null)
        {
          return;
        }
        _mediaCtrl.Stop();
      }
      else
      {
        // Do not disable the exclusive mode if we are zapping
        CloseInterfaces();
        if (!keepExclsuiveModeOn)
        {
          ExclusiveMode(false);
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
          _updateNeeded = true;
        }
      }
    }

    public override void SeekRelative(double dTime)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          double dCurTime = this.CurrentPosition;

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

    public override void SeekAbsolute(double dTimeInSecs)
    {
      Log.Info("TSReaderPlayer: SeekAbsolute:seekabs:{0}", dTimeInSecs);

      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          lock (_mediaCtrl)
          {
            if (dTimeInSecs < 0.0d)
            {
              dTimeInSecs = 0.0d;
            }
            if (dTimeInSecs > Duration)
            {
              dTimeInSecs = Duration;
            }
            dTimeInSecs = Math.Floor(dTimeInSecs);
            Log.Info("TSReaderPlayer seekabs: {0} duration:{1} current pos:{2}", dTimeInSecs, Duration, CurrentPosition);
            dTimeInSecs *= 10000000d;
            long pStop = 0;
            long lContentStart, lContentEnd;
            double fContentStart, fContentEnd;
            Log.Info("get available");
            _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
            Log.Info("get available done");
            fContentStart = lContentStart;
            fContentEnd = lContentEnd;

            dTimeInSecs += fContentStart;
            long lTime = (long)dTimeInSecs;
            Log.Info("set positions");
            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.FrameCounter = 123;
            }
            int hr = _mediaSeeking.SetPositions(new DsLong(lTime), AMSeekingSeekingFlags.AbsolutePositioning,
                                                new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
            if (VMR9Util.g_vmr9 != null)
            {
              VMR9Util.g_vmr9.FrameCounter = 123;
            }
            Log.Info("set positions done");
            if (hr != 0)
            {
              Log.Error("seek failed->seek to 0 0x:{0:X}", hr);
            }
          }
          UpdateCurrentPosition();
          if (_dvbSubRenderer != null)
          {
            _dvbSubRenderer.OnSeek(CurrentPosition);
          }
          _state = PlayState.Playing;
          Log.Info("TSReaderPlayer: current pos:{0}", CurrentPosition);
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (_state != PlayState.Init)
      {
        if (_mediaCtrl != null && _mediaSeeking != null)
        {
          double dCurrentPos = this.CurrentPosition;
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

    public override bool IsRadio
    {
      get { return _isRadio; }
    }

    public override bool HasVideo
    {
      get { return (_isRadio == false); }
    }

    public override VideoStreamFormat GetVideoFormat()
    {
      return _videoFormat;
    }

    #endregion

    #region private/protected members

    protected virtual void SetVideoPosition(Rectangle rDest)
    {
      if (_videoWin != null)
      {
        if (rDest.Left < 0 || rDest.Top < 0 || rDest.Width <= 0 || rDest.Height <= 0)
        {
          return;
        }
        _videoWin.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
    }

    protected virtual void SetSourceDestRectangles(Rectangle rSource, Rectangle rDest)
    {
      if (_basicVideo != null)
      {
        if (rSource.Left < 0 || rSource.Top < 0 || rSource.Width <= 0 || rSource.Height <= 0)
        {
          return;
        }
        if (rDest.Width <= 0 || rDest.Height <= 0)
        {
          return;
        }
        _basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
        _basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
      }
    }

    private void MovieEnded()
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped

      //Log.Info("TSStreamBufferPlayer:ended {0}", _currentFile);
      if (!IsTimeShifting)
      {
        CloseInterfaces();
        ExclusiveMode(false);
        _state = PlayState.Ended;
      }
      else
      {
        _endOfFileDetected = true;
        Log.Info("TSReaderPlayer timeshift EOF");
      }
    }

    private void CheckVideoResolutionChanges()
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
        _updateNeeded = true;
        SetVideoWindow();
      }
    }

    protected virtual void OnProcess() { }

#if DEBUG
    private static DateTime dtStart = DateTime.Now;
#endif

    protected void UpdateCurrentPosition()
    {
      if (_mediaSeeking == null || _graphBuilder == null || Thread.CurrentThread.Name != "MPMain")
      {
        return;
      }
      lock (_mediaCtrl)
      {
        //GetCurrentPosition(): Returns stream position. 
        //Stream position:The current playback position, relative to the content start
        long lStreamPos;
        double fCurrentPos;
        _mediaSeeking.GetCurrentPosition(out lStreamPos); // stream position
        fCurrentPos = lStreamPos;
        fCurrentPos /= 10000000d;
        _streamPos = fCurrentPos; // save the stream position 
        long lContentStart, lContentEnd;
        double fContentStart, fContentEnd;
        _mediaSeeking.GetAvailable(out lContentStart, out lContentEnd);
        fContentStart = lContentStart;
        fContentEnd = lContentEnd;
        fContentStart /= 10000000d;
        fContentEnd /= 10000000d;
        // Log.Info("pos:{0} start:{1} end:{2}  pos:{3} dur:{4}", fCurrentPos, fContentStart, fContentEnd, (fCurrentPos - fContentStart), (fContentEnd - fContentStart));
        fContentEnd -= fContentStart;
        fCurrentPos -= fContentStart;
        _duration = fContentEnd;
        _currentPos = fCurrentPos;
      }
    }

    private void UpdateDuration() { }

    private bool _bMediaTypeChanged;
    private bool _bRequestAudioChange;

    public int OnMediaTypeChanged(int mediaType)
    {
      _bMediaTypeChanged = true;
      iChangedMediaTypes = mediaType;
      return 0;
    }

    public int OnVideoFormatChanged(int streamType, int width, int height, int aspectRatioX, int aspectRatioY,
                                    int bitrate, int isInterlaced)
    {
      _isRadio = false;
      _videoFormat.IsValid = true;
      _videoFormat.streamType = (VideoStreamType)streamType;
      _videoFormat.width = width;
      _videoFormat.height = height;
      _videoFormat.arX = aspectRatioX;
      _videoFormat.arY = aspectRatioY;
      _videoFormat.bitrate = bitrate;
      _videoFormat.isInterlaced = (isInterlaced == 1);
      Log.Info("TsReaderPlayer: OnVideoFormatChanged - {0}", _videoFormat.ToString());
      return 0;
    }

    public int OnRequestAudioChange()
    {
      if (Thread.CurrentThread.Name == "MPMain")
      {
        // probably called on TsReader startup, we can ( and should ! ) do it immediately.
        Log.Info("TSReaderPlayer:OnRequestAudioChange()");
        g_Player.OnAudioTracksReady();
      }
      else
      {
        _bRequestAudioChange = true;
      }
      return 0;
    }

    public void DoGraphRebuild()
    {
      bool needRebuild = true; // GraphNeedsRebuild(); forcing is equal in speed
      if (_mediaCtrl != null)
      {
        lock (_mediaCtrl)
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
          }
          
          try
          {
            //Make sure the graph has really stopped
            FilterState state;
            hr = _mediaCtrl.GetState(1000, out state);
            DsError.ThrowExceptionForHR(hr);
            if (state != FilterState.Stopped)
            {
              Log.Error("TSReaderPlayer: graph still running");
            }
          }
          catch (Exception error)
          {
            Log.Error("Error checking graph state: {0}", error.Message);
          }

          if (needRebuild)
          {
            // this is a hack for MS Video Decoder and AC3 audio change
            // would suggest to always do full audio and video rendering for all filters
            IBaseFilter MSVideoCodec = null;
            _graphBuilder.FindFilterByName("Microsoft DTV-DVD Video Decoder", out MSVideoCodec);
            if (MSVideoCodec != null)
            {
              iChangedMediaTypes = 3;
              DirectShowUtil.ReleaseComObject(MSVideoCodec);
              MSVideoCodec = null;
            }
            // hack end
            switch (iChangedMediaTypes)
            {
              case 1: // audio changed
                Log.Info("Rerendering audio pin of tsreader filter.");
                UpdateFilters("Audio");
                break;
              case 2: // video changed
                Log.Info("Rerendering video pin of tsreader filter.");
                UpdateFilters("Video");
                break;
              case 3: // both changed
                Log.Info("Rerendering audio and video pins of tsreader filter.");
                UpdateFilters("Audio");
                UpdateFilters("Video");
                break;
            }

            if (iChangedMediaTypes != 1 && VideoChange)
            {
              if (filterConfig != null && filterConfig.enableCCSubtitles)
              {
                CleanupCC();
                DirectShowUtil.RenderGraphBuilderOutputPins(_graphBuilder, _fileSource);
                DirectShowUtil.RenderUnconnectedOutputPins(_graphBuilder, filterCodec.VideoCodec);                
                EnableCC();
                if (CoreCCPresent)
                {
                  DirectShowUtil.RenderUnconnectedOutputPins(_graphBuilder, filterCodec.CoreCCParser);                  
                  EnableCC2();
                }
              }
              else
              {
                DirectShowUtil.RenderGraphBuilderOutputPins(_graphBuilder, _fileSource);
                CleanupCC();
              }
              if (PostProcessingEngine.engine != null)
                PostProcessingEngine.GetInstance().FreePostProcess();

              IPostProcessingEngine postengine = PostProcessingEngine.GetInstance(true);
              if (!postengine.LoadPostProcessing(_graphBuilder))
              {
                PostProcessingEngine.engine = new PostProcessingEngine.DummyEngine();
              }
            }
            else
            {
              DirectShowUtil.RenderGraphBuilderOutputPins(_graphBuilder, _fileSource);
            }
            DirectShowUtil.RemoveUnusedFiltersFromGraph(_graphBuilder);

            try
            {
              hr = _mediaCtrl.Run();
              DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception error)
            {
              Log.Error("Error starting graph: {0}", error.Message);
              return;
            }
            Log.Info("Reconfigure graph done");
          }
        }
      }
    }

    /// <summary> create the used COM components and get the interfaces. </summary>
    protected virtual bool GetInterfaces(string filename) { return true; }

    protected virtual void CloseInterfaces() { }

    private void OnGraphNotify()
    {
      int p1, p2, hr = 0;
      EventCode code;
      int counter = 0;
      do
      {
        counter++;
        if ( /*Playing && */_mediaEvt != null)
        {
          hr = _mediaEvt.GetEvent(out code, out p1, out p2, 0);
          if (hr == 0)
          {
            hr = _mediaEvt.FreeEventParams(code, p1, p2);
            if (code == EventCode.Complete || code == EventCode.ErrorAbort)
            {
              Log.Info("TSReaderPlayer: event:{0} param1:{1} param2:{2} param1:0x{3:X} param2:0x{4:X}", code.ToString(),
                       p1, p2, p1, p2);
              MovieEnded();

              // Playback was aborted! No sound driver is available!
              if (code == EventCode.ErrorAbort && p1 == DSERR_NODRIVER)
              {
                CloseInterfaces();
                ExclusiveMode(false);
                _state = PlayState.Ended;
                Log.Error("TSReaderPlayer: No sound driver is available!");
              }
            }
            //else
            //Log.Info("TSReaderPlayer: event:{0} 0x{1:X} param1:{2} param2:{3} param1:0x{4:X} param2:0x{5:X}",code.ToString(), (int)code,p1,p2,p1,p2);
          }
          else
          {
            break;
          }
        }
        else
        {
          break;
        }
      } while (hr == 0 && counter < 20);
    }

    protected virtual void ExclusiveMode(bool onOff) { }

    protected virtual void OnInitialized() { }

    protected void DoFFRW()
    {
      if (!Playing || _usingFastSeeking)
      {
        return;
      }
      if ((_speedRate == 10000) || (_mediaSeeking == null) || (_mediaCtrl == null))
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
        // before the StopWhenReady() method has been completed. It results as a kind of mess in the tsReader....
      } // So, it's better to verify a new frame has been pesented.
      long earliest, latest, current, stop, rewind, pStop;
      lock (_mediaCtrl)
      {
        _mediaSeeking.GetAvailable(out earliest, out latest);
        _mediaSeeking.GetPositions(out current, out stop);

        //this is the real elapsed time from next seek.
        DateTime dt = DateTime.Now;
        ts = dt - _FFRWtimer;
        _FFRWtimer = dt;

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
        if ((rewind < earliest) && (_speedRate < 0))
        {
          _speedRate = 10000;
          rewind = earliest;
          Log.Info("TSReaderPlayer: timeshift SOF seek back:{0}", rewind / 10000000);
          hr = _mediaSeeking.SetPositions(new DsLong(rewind),
                                          AMSeekingSeekingFlags.AbsolutePositioning |
                                          AMSeekingSeekingFlags.SeekToKeyFrame,
                                          new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          Speed = 1;
          return;
        }
        // if we end up at the end of time then just
        // start @ the end -1sec
        long margin = (IsTimeShifting) ? 10000000 : 100000;

        if ((rewind > (latest - margin)) && (_speedRate > 0))
        {
          if (IsTimeShifting)
          {
            _speedRate = 10000;
            rewind = latest - margin;
            Log.Info("TSReaderPlayer: timeshift EOF seek ff:{0} {1}", rewind, latest);
            hr = _mediaSeeking.SetPositions(new DsLong(rewind),
                                            AMSeekingSeekingFlags.AbsolutePositioning |
                                            AMSeekingSeekingFlags.SeekToKeyFrame,
                                            new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
            Speed = 1;
          }
          else
          {
            Log.Info("TSReaderPlayer: Fastforward reached the end of file, stopping playback");
            _state = PlayState.Ended;
          }
          return;
        }
        //seek to new moment in time
        //Log.Info(" seek :{0}",rewind/10000000);
        if (VMR9Util.g_vmr9 != null)
        {
          _lastFrameCounter = VMR9Util.g_vmr9.FreeFrameCounter;
          VMR9Util.g_vmr9.EVRProvidePlaybackRate(0.0);
        }
        hr = _mediaSeeking.SetPositions(new DsLong(rewind),
                                        AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                        new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        _mediaCtrl.Run();
        //according to ms documentation, this is the prefered way to do seeking
        // _mediaCtrl.StopWhenReady();
        _elapsedTimer = DateTime.Now;
      }
    }

    protected virtual void PostProcessAddVideo() { }

    protected virtual void PostProcessAddAudio() { }

    protected virtual void MPAudioSwitcherAdd() { }

    protected virtual void AudioRendererAdd() { }

    protected virtual void SyncAudioRenderer() { }

    protected virtual string MatchFilters(string format) { return format; }

    protected virtual void CoreCCParserCheck() { }

    protected virtual void CleanupCC() { }

    protected virtual void ReleaseCC() { }

    protected virtual void ReleaseCC2() { }

    protected virtual void EnableCC() { }

    protected virtual void EnableCC2() { }

    protected void UpdateFilters(string selection)
    {
      if (selection == "Video")
      {
        VideoChange = false;
        if (PostProcessFilterVideo.Count > 0)
        {
          foreach (var ppFilter in PostProcessFilterVideo)
          {
            if (ppFilter.Value != null)
            {
              DirectShowUtil.RemoveFilters(_graphBuilder, ppFilter.Key);
              DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
            }
          }
          PostProcessFilterVideo.Clear();
          Log.Info("TSReaderPlayer: UpdateFilters Cleanup PostProcessVideo");
        }
        if (filterConfig != null && filterConfig.enableCCSubtitles)
        {
          ReleaseCC();
          ReleaseCC2();
        }
      }
      else
      {
        if (PostProcessFilterAudio.Count > 0)
        {
          foreach (var ppFilter in PostProcessFilterAudio)
          {
            if (ppFilter.Value != null)
            {
              DirectShowUtil.RemoveFilters(_graphBuilder, ppFilter.Key);
              DirectShowUtil.ReleaseComObject(ppFilter.Value);//, 5000);
            }
          }
          PostProcessFilterAudio.Clear();
          Log.Info("TSReaderPlayer: UpdateFilters Cleanup PostProcessAudio");
        }

        if (filterConfig != null && filterConfig.enableMPAudioSwitcher)
        {
          IBaseFilter switcher = DirectShowUtil.GetFilterByName(_graphBuilder, "MediaPortal AudioSwitcher");
          if (switcher != null)
          {
            _graphBuilder.RemoveFilter(switcher);
            DirectShowUtil.ReleaseComObject(switcher);
            switcher = null;
          }
        }
      }

      // we have to find first filter connected to tsreader which will be removed
      IPin pinFrom = DirectShowUtil.FindPin(_fileSource, PinDirection.Output, selection);
      IPin pinTo;
      if (pinFrom != null)
      {
        int hr = pinFrom.ConnectedTo(out pinTo);
        if (hr >= 0 && pinTo != null)
        {
          PinInfo pInfo;
          pinTo.QueryPinInfo(out pInfo);
          FilterInfo fInfo;
          pInfo.filter.QueryFilterInfo(out fInfo);

          if (selection == "Video" && fInfo.achName.Equals("Core CC Parser"))
          {
            IPin pinFromOut = DsFindPin.ByDirection(filterCodec.CoreCCParser, PinDirection.Output, 0);
            IPin pinToVideo;
            hr = pinFromOut.ConnectedTo(out pinToVideo);
            if (hr >= 0 && pinToVideo != null)
            {
              PinInfo pInfoNext;
              pinToVideo.QueryPinInfo(out pInfoNext);
              FilterInfo fInfoNext;
              pInfoNext.filter.QueryFilterInfo(out fInfoNext);
              Log.Debug("TSReaderPlayer: UpdateFilters Remove filter - {0}", fInfoNext.achName);
              DirectShowUtil.DisconnectAllPins(_graphBuilder, pInfo.filter);
              _graphBuilder.RemoveFilter(pInfoNext.filter);
              DsUtils.FreePinInfo(pInfoNext);
              DirectShowUtil.ReleaseComObject(fInfoNext.pGraph);
              DirectShowUtil.ReleaseComObject(pinToVideo); pinToVideo = null;
            }
            DirectShowUtil.ReleaseComObject(pinFromOut); pinFromOut = null;
          }
          else
          {
            DirectShowUtil.DisconnectAllPins(_graphBuilder, pInfo.filter);
            _graphBuilder.RemoveFilter(pInfo.filter);
            Log.Debug("TSReaderPlayer: UpdateFilters Remove filter - {0}", fInfo.achName);
          }
          DsUtils.FreePinInfo(pInfo);
          DirectShowUtil.ReleaseComObject(fInfo.pGraph);
          DirectShowUtil.ReleaseComObject(pinTo); pinTo = null;
        }
        DirectShowUtil.ReleaseComObject(pinFrom); pinFrom = null;
      }

      if (selection == "Video")
      {
        //Add Post Process Video Codec
        PostProcessAddVideo();

        //Add Video Codec
        if (filterCodec.VideoCodec != null)
        {
          DirectShowUtil.ReleaseComObject(filterCodec.VideoCodec);
          filterCodec.VideoCodec = null;
        }
        filterCodec.VideoCodec = DirectShowUtil.AddFilterToGraph(this._graphBuilder, MatchFilters(selection));

        if (filterConfig != null && selection == "Video" && filterConfig.enableCCSubtitles)
        {
          CoreCCPresent = false;
          CoreCCParserCheck();
        }
        VideoChange = true;
      }
      else
      {
        if (filterConfig != null && filterConfig.enableMPAudioSwitcher)
        {
          MPAudioSwitcherAdd();
        }

        //Add Post Process Audio Codec
        PostProcessAddAudio();

        //Add Audio Codec
        if (filterCodec.AudioCodec != null)
        {
          DirectShowUtil.ReleaseComObject(filterCodec.AudioCodec);
          filterCodec.AudioCodec = null;
        }
        filterCodec.AudioCodec = DirectShowUtil.AddFilterToGraph(this._graphBuilder, MatchFilters(selection));
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