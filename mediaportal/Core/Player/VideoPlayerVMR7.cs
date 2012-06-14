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
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;
using MediaPortal.Player.Subtitles;
using System.Collections.Generic;
using MediaPortal.Player.PostProcessing;

namespace MediaPortal.Player
{
  public abstract class VideoPlayerVMR7 : IPlayer
  {
    protected const int MAX_STREAMS = 100;
    protected const string FFDSHOW_AUDIO_DECODER_FILTER = "ffdshow Audio Decoder";
    protected const string MEDIAPORTAL_AUDIOSWITCHER_FILTER = "MediaPortal AudioSwitcher";
    protected const string LAV_SPLITTER_FILTER = "LAV Splitter";
    protected const string LAV_SPLITTER_FILTER_SOURCE = "LAV Splitter Source";
    protected const string LAV_AUDIO = "LAV Audio Decoder";
    protected const string LAV_VIDEO = "LAV Video Decoder";
    protected const string FILE_SYNC_FILTER = "File Source (Async.)";
    protected IGraphRebuildDelegate _IGraphRebuildDelegate = null;

    protected struct FilterStreamInfos
    {
      public int Id;
      public string Name;
      public bool Current;
      public string Filter;
      public StreamType Type;
      public int LCID;
    } ;

    protected class FilterStreams
    {
      public FilterStreams()
      {
        cStreams = 0;
        Streams = new FilterStreamInfos[MAX_STREAMS];
        cStreamsExternal = 0;
        StreamsExternal = new FilterStreamInfos[MAX_STREAMS];
      }

      public FilterStreamInfos GetStreamInfos(StreamType type, int id)
      {
        return GetStreamInfos(type, id, cStreams, Streams);
      }

      public FilterStreamInfos GetStreamInfosExternal(StreamType type, int id)
      {
        return GetStreamInfos(type, id, cStreamsExternal, StreamsExternal);
      }

      private static FilterStreamInfos GetStreamInfos(StreamType type, int id, int streamsCount, FilterStreamInfos[] streams)
      {
        var empty = new FilterStreamInfos();
        for (int i = 0; i < streamsCount; i++)
        {
          if (type == streams[i].Type)
          {
            if (id == 0)
            {
              return streams[i];
            }
            id--;
          }
        }
        return empty;
      }

      public int GetStreamCount(StreamType Type)
      {
        int ret = 0;
        for (int i = 0; i < cStreams; i++)
        {
          if (Type == Streams[i].Type)
          {
            ret++;
          }
        }
        return ret;
      }

      public bool AddStreamInfos(FilterStreamInfos streamInfos)
      {
        return AddStreamInfos(streamInfos, ref cStreams, Streams);
      }      

      public bool AddStreamInfosEx(FilterStreamInfos streamInfos)
      {
        return AddStreamInfos(streamInfos, ref cStreamsExternal, StreamsExternal);
      }

      private static bool AddStreamInfos(FilterStreamInfos streamInfos, ref int streamsCount, FilterStreamInfos[] streams)
      {
        if (streamsCount == MAX_STREAMS)
        {
          return false;
        }
        streams[streamsCount] = streamInfos;
        streamsCount++;
        return true;
      }

      public bool SetCurrentValue(StreamType Type, int Id, bool Value)
      {
        for (int i = 0; i < cStreams; i++)
        {
          if (Type == Streams[i].Type)
          {
            if (Id == 0)
            {
              Streams[i].Current = Value;
              return true;
            }
            Id--;
          }
        }
        return false;
      }

      public void DeleteAllStreams()
      {
        cStreams = 0;
        cStreamsExternal = 0;
      }

      private FilterStreamInfos[] Streams;
      private int cStreams;
      private FilterStreamInfos[] StreamsExternal;
      private int cStreamsExternal;
    } ;

    public enum PlayState
    {
      Init,
      Playing,
      Paused,
      Ended,
    }

    public enum StreamType
    {
      Video,
      Audio,
      Subtitle,
      Subtitle_hidden,
      Subtitle_shown,
      Edition,
      Subtitle_file,
      PostProcessing,
      Unknown,
    }

    protected int m_iPositionX = 0;
    protected int m_iPositionY = 0;
    protected int m_iWidth = 200;
    protected int m_iHeight = 100;
    protected int m_iVideoWidth = 100;
    protected int m_iVideoHeight = 100;
    protected string m_strCurrentFile = "";
    protected bool _updateNeeded = false;
    protected Geometry.Type m_ar = Geometry.Type.Normal;
    protected bool m_bFullScreen = true;
    protected PlayState m_state = PlayState.Init;
    protected int m_iVolume = 100;
    protected int m_volumeBeforeSeeking = 0;
    protected IGraphBuilder graphBuilder;
    protected long m_speedRate = 10000;
    protected double m_dCurrentPos;
    protected double m_dDuration;
    protected int m_aspectX = 1;
    protected int m_aspectY = 1;
    protected bool m_bStarted = false;
    protected DsROTEntry _rotEntry = null;

    /// <summary> control interface. </summary>
    protected IMediaControl mediaCtrl;

    /// <summary> graph event interface. </summary>
    protected IMediaEventEx mediaEvt;

    /// <summary> seek interface for positioning in stream. </summary>
    protected IMediaSeeking mediaSeek;

    protected bool m_bRateSupport = false;

    /// <summary> seek interface to set position in stream. </summary>
    protected IMediaPosition mediaPos;

    /// <summary> video preview window interface. </summary>
    protected IVideoWindow videoWin;

    /// <summary> interface to get information and control video. </summary>
    protected IBasicVideo2 basicVideo;

    /// <summary> audio interface used to control volume. </summary>
    protected IBasicAudio basicAudio;

    protected const int WM_GRAPHNOTIFY = 0x00008001; // message from graph
    protected const int WS_CHILD = 0x40000000; // attributes for video window
    protected const int WS_CLIPCHILDREN = 0x02000000;
    protected const int WS_CLIPSIBLINGS = 0x04000000;
    protected bool m_bVisible = false;
    protected DateTime updateTimer;
    protected FilterStreams FStreams = null;
    protected double[] chapters = null;
    protected string[] chaptersname = null;
    private DateTime elapsedTimer = DateTime.Now;
    protected int m_lastFrameCounter = 0;

    protected const string defaultLanguageCulture = "EN";

    protected bool AudioExternal = false;

    protected IBaseFilter _audioSwitcher = null;
    protected IBaseFilter _FFDShowAudio = null;
    protected IBaseFilter _AudioSourceFilter = null;
    protected IBaseFilter _AudioExtFilter = null;
    protected IBaseFilter _AudioExtSplitterFilter = null;
    protected static MediaInfoWrapper _mediaInfo = null;
    protected IBaseFilter _interfaceSourceFilter = null;
    protected IBaseFilter Splitter = null;
    protected bool FileSync = false;
    protected int iChangedMediaTypes;
    protected bool SourceFilesyncFind = false;
    protected bool GetInterface = false;
    protected bool AutoRenderingCheck = false;
    protected bool VideoChange = false;
    protected bool firstinit = false;
    protected bool vc1ICodec = false;
    protected bool vc1Codec = false;
    protected bool h264Codec = false;
    protected bool xvidCodec = false;
    protected bool aacCodec = false;
    protected bool aacCodecLav = false;
    protected bool MediatypeVideo = false;
    protected bool MediatypeAudio = false;
    protected bool MediatypeSubtitle = false;
    protected bool AudioOnly = false;

    public override double[] Chapters
    {
      get { return chapters; }
    }

    public override string[] ChaptersName
    {
      get { return chaptersname; }
    }

    protected g_Player.MediaType _mediaType;

    public VideoPlayerVMR7()
    {
      _mediaType = g_Player.MediaType.Video;
    }

    public VideoPlayerVMR7(g_Player.MediaType type)
    {
      _mediaType = type;
    }

    public override bool Play(string strFile)
    {
      updateTimer = DateTime.Now;
      m_speedRate = 10000;
      m_bVisible = false;
      m_iVolume = 100;
      m_state = PlayState.Init;
      m_strCurrentFile = strFile;
      m_bFullScreen = true;
      m_ar = GUIGraphicsContext.ARType;
      VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
      _updateNeeded = true;
      Log.Info("VideoPlayer:play {0}", strFile);
      //lock ( typeof(VideoPlayerVMR7) )
      {
        CloseInterfaces();
        m_bStarted = false;
        if (!GetInterfaces())
        {
          m_strCurrentFile = "";
          CloseInterfaces();
          return false;
        }

        #region FFDShowEngine and PostProcessingEngine Detection

        ISubEngine engine = SubEngine.GetInstance(true);
        if (!engine.LoadSubtitles(graphBuilder, m_strCurrentFile))
        {
          SubEngine.engine = new SubEngine.DummyEngine();
        }

        IPostProcessingEngine postengine = PostProcessingEngine.GetInstance(true);
        if (!postengine.LoadPostProcessing(graphBuilder))
        {
          PostProcessingEngine.engine = new PostProcessingEngine.DummyEngine();
        }

        #endregion

        AnalyseStreams();
        SelectSubtitles();
        SelectAudioLanguage();
        OnInitialized();

        int hr = mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
        if (hr < 0)
        {
          Error.SetError("Unable to play movie", "Can not set notifications");
          m_strCurrentFile = "";
          CloseInterfaces();
          return false;
        }
        if (videoWin != null)
        {
          videoWin.put_Owner(GUIGraphicsContext.ActiveForm);
          videoWin.put_WindowStyle(
            (WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipChildren + (int)WindowStyle.ClipSiblings));
          videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);
        }
        if (basicVideo != null)
        {
          hr = basicVideo.GetVideoSize(out m_iVideoWidth, out m_iVideoHeight);
          if (hr < 0)
          {
            Error.SetError("Unable to play movie", "Can not find movie width/height");
            m_strCurrentFile = "";
            CloseInterfaces();
            return false;
          }
        }
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
        DirectShowUtil.SetARMode(graphBuilder, AspectRatioMode.Stretched);
        // DsUtils.DumpFilters(graphBuilder);
        try
        {
          hr = mediaCtrl.Run();
          DsError.ThrowExceptionForHR(hr);
        }
        catch (Exception error)
        {
          Log.Error("VideoPlayer: Unable to play with reason - {0}", error.Message);
        }
        if (hr < 0)
        {
          Error.SetError("Unable to play movie", "Unable to start movie");
          m_strCurrentFile = "";
          CloseInterfaces();
          return false;
        }
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
        msg.Label = strFile;
        GUIWindowManager.SendThreadMessage(msg);
        m_state = PlayState.Playing;
        //Brutus GUIGraphicsContext.IsFullScreenVideo=true;
        m_iPositionX = GUIGraphicsContext.VideoWindow.X;
        m_iPositionY = GUIGraphicsContext.VideoWindow.Y;
        m_iWidth = GUIGraphicsContext.VideoWindow.Width;
        m_iHeight = GUIGraphicsContext.VideoWindow.Height;
        m_ar = GUIGraphicsContext.ARType;
        _updateNeeded = true;
        SetVideoWindow();
        mediaPos.get_Duration(out m_dDuration);
        Log.Info("VideoPlayer:Duration:{0}", m_dDuration);
      }
      return true;
    }

    protected void SelectSubtitles()
    {
      if (SubtitleStreams == 0) return;
      if (!SubEngine.GetInstance().AutoShow) return;
      CultureInfo ci = null;

      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          ci = new CultureInfo(xmlreader.GetValueAsString("subtitles", "language", defaultLanguageCulture));
          Log.Info("VideoPlayerVMR7: Subtitle CultureInfo {0}", ci);
        }
        catch (Exception ex)
        {
          ci = new CultureInfo(defaultLanguageCulture);
          Log.Error(
            "SelectSubtitleLanguage - unable to build CultureInfo, make sure MediaPortal.xml is not corrupted! - {0}",
            ex);
        }
      }
      int subsCount = SubtitleStreams; // Not in the loop otherwise it will be reaccessed at each pass
      for (int i = 0; i < subsCount; i++)
      {
        string subtitleLanguage = SubtitleLanguage(i);
        //Add localized stream names for FFDshow when OS language = Skin language
        string localizedCINameSub = Util.Utils.TranslateLanguageString(ci.EnglishName);
        if (localizedCINameSub.Equals(SubtitleLanguage(i), StringComparison.OrdinalIgnoreCase) ||
            ci.EnglishName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase) ||
            ci.TwoLetterISOLanguageName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterISOLanguageName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterWindowsLanguageName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase))
        {
          CurrentSubtitleStream = i;
          Log.Info("VideoPlayerVMR7: CultureInfo Selected active subtitle track language: {0} ({1})", ci.EnglishName, i);
          break;
        }
      }
      EnableSubtitle = true;
    }

    protected void SelectAudioLanguage()
    {
      CultureInfo ci = null;
      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          ci = new CultureInfo(xmlreader.GetValueAsString("movieplayer", "audiolanguage", defaultLanguageCulture));
          Log.Info("VideoPlayerVMR7: AudioLanguage CultureInfo {0}", ci);
        }
        catch (Exception ex)
        {
          ci = new CultureInfo(defaultLanguageCulture);
          Log.Error(
            "SelectAudioLanguage - unable to build CultureInfo, make sure MediaPortal.xml is not corrupted! - {0}", ex);
        }
      }
      for (int i = 0; i < AudioStreams; i++)
      {
        // Unfortunately we use localized stream names...
        string localizedCIName = Util.Utils.TranslateLanguageString(ci.EnglishName);
        if (localizedCIName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase) ||
            ci.EnglishName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase) ||
            ci.TwoLetterISOLanguageName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterISOLanguageName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterWindowsLanguageName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase))
        {
          CurrentAudioStream = i;
          Log.Info("VideoPlayerVMR7: CultureInfo Selected active audio track language: {0} ({1})", ci.EnglishName, i);
          break;
        }
      }
    }

    public override bool PlayStream(string strFile, string streamName)
    {
      bool isPlaybackPossible = Play(strFile);
      if (isPlaybackPossible)
      {
        if (streamName != null)
        {
          if (streamName != "")
          {
            m_strCurrentFile = streamName;
          }
        }
      }
      return isPlaybackPossible;
    }

    public override void SetVideoWindow()
    {
      if (GUIGraphicsContext.Vmr9Active)
      {
        _updateNeeded = false;
        m_bStarted = true;
        return;
      }
      if (GUIGraphicsContext.IsFullScreenVideo != m_bFullScreen)
      {
        m_bFullScreen = GUIGraphicsContext.IsFullScreenVideo;
        _updateNeeded = true;
      }
      if (!_updateNeeded)
      {
        return;
      }
      _updateNeeded = false;
      m_bStarted = true;
      float x = m_iPositionX;
      float y = m_iPositionY;
      int nw = m_iWidth;
      int nh = m_iHeight;
      if (nw > GUIGraphicsContext.OverScanWidth)
      {
        nw = GUIGraphicsContext.OverScanWidth;
      }
      if (nh > GUIGraphicsContext.OverScanHeight)
      {
        nh = GUIGraphicsContext.OverScanHeight;
      }
      //lock ( typeof(VideoPlayerVMR7) )
      {
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          x = m_iPositionX = GUIGraphicsContext.OverScanLeft;
          y = m_iPositionY = GUIGraphicsContext.OverScanTop;
          nw = m_iWidth = GUIGraphicsContext.OverScanWidth;
          nh = m_iHeight = GUIGraphicsContext.OverScanHeight;
        }
        if (x < 0 || y < 0)
        {
          return;
        }
        if (nw <= 0 || nh <= 0)
        {
          return;
        }
        int aspectX, aspectY;
        if (basicVideo != null)
        {
          basicVideo.GetVideoSize(out m_iVideoWidth, out m_iVideoHeight);
        }
        aspectX = m_iVideoWidth;
        aspectY = m_iVideoHeight;
        if (basicVideo != null)
        {
          basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
        }
        m_aspectX = aspectX;
        m_aspectY = aspectY;
        GUIGraphicsContext.VideoSize = new Size(m_iVideoWidth, m_iVideoHeight);
        Rectangle rSource, rDest;
        Geometry m_geometry = new Geometry();
        m_geometry.ImageWidth = m_iVideoWidth;
        m_geometry.ImageHeight = m_iVideoHeight;
        m_geometry.ScreenWidth = nw;
        m_geometry.ScreenHeight = nh;
        m_geometry.ARType = GUIGraphicsContext.ARType;
        m_geometry.PixelRatio = GUIGraphicsContext.PixelRatio;
        m_geometry.GetWindow(aspectX, aspectY, out rSource, out rDest);
        rDest.X += (int)x;
        rDest.Y += (int)y;
        Log.Info("overlay: video WxH  : {0}x{1}", m_iVideoWidth, m_iVideoHeight);
        Log.Info("overlay: video AR   : {0}:{1}", aspectX, aspectY);
        Log.Info("overlay: screen WxH : {0}x{1}", nw, nh);
        Log.Info("overlay: AR type    : {0}", GUIGraphicsContext.ARType);
        Log.Info("overlay: PixelRatio : {0}", GUIGraphicsContext.PixelRatio);
        Log.Info("overlay: src        : ({0},{1})-({2},{3})",
                 rSource.X, rSource.Y, rSource.X + rSource.Width, rSource.Y + rSource.Height);
        Log.Info("overlay: dst        : ({0},{1})-({2},{3})",
                 rDest.X, rDest.Y, rDest.X + rDest.Width, rDest.Y + rDest.Height);
        SetSourceDestRectangles(rSource, rDest);
        SetVideoPosition(rDest);
        _sourceRectangle = rSource;
        _videoRectangle = rDest;
      }
    }

    protected virtual void SetVideoPosition(Rectangle rDest)
    {
      if (videoWin != null)
      {
        if (rDest.Left < 0 || rDest.Top < 0 || rDest.Width <= 0 || rDest.Height <= 0)
        {
          return;
        }
        videoWin.SetWindowPosition(rDest.Left, rDest.Top, rDest.Width, rDest.Height);
      }
    }

    protected virtual void SetSourceDestRectangles(Rectangle rSource, Rectangle rDest)
    {
      if (basicVideo != null)
      {
        if (rSource.Left < 0 || rSource.Top < 0 || rSource.Width <= 0 || rSource.Height <= 0)
        {
          return;
        }
        if (rDest.Width <= 0 || rDest.Height <= 0)
        {
          return;
        }
        basicVideo.SetSourcePosition(rSource.Left, rSource.Top, rSource.Width, rSource.Height);
        basicVideo.SetDestinationPosition(0, 0, rDest.Width, rDest.Height);
      }
    }

    private void MovieEnded(bool bManualStop)
    {
      // this is triggered only if movie has ended
      // ifso, stop the movie which will trigger MovieStopped
      m_strCurrentFile = "";
      if (!bManualStop)
      {
        CloseInterfaces();
        m_state = PlayState.Ended;
        GUIGraphicsContext.IsPlaying = false;
      }
    }

    public override void Process()
    {
      if (!Playing)
      {
        return;
      }
      if (!m_bStarted)
      {
        return;
      }
      TimeSpan ts = DateTime.Now - updateTimer;
      if (ts.TotalMilliseconds >= 800 || m_speedRate != 10000)
      {
        if (mediaPos != null)
        {
          mediaPos.get_Duration(out m_dDuration); //(refresh timeline when change EDITION)
          mediaPos.get_CurrentPosition(out m_dCurrentPos);
        }
        if (GUIGraphicsContext.BlankScreen ||
            (GUIGraphicsContext.Overlay == false && GUIGraphicsContext.IsFullScreenVideo == false))
        {
          if (m_bVisible)
          {
            m_bVisible = false;
            if (videoWin != null)
            {
              videoWin.put_Visible(OABool.False);
            }
          }
        }
        else if (!m_bVisible)
        {
          m_bVisible = true;
          if (videoWin != null)
          {
            videoWin.put_Visible(OABool.True);
          }
        }
        CheckVideoResolutionChanges();
        updateTimer = DateTime.Now;
      }
      if (m_speedRate != 10000)
      {
        DoFFRW();
      }
      else
      {
        m_lastFrameCounter = 0;
      }
      OnProcess();
    }

    private void CheckVideoResolutionChanges()
    {
      if (videoWin == null || basicVideo == null)
      {
        return;
      }
      int aspectX, aspectY;
      int videoWidth = 1, videoHeight = 1;
      if (basicVideo != null)
      {
        basicVideo.GetVideoSize(out videoWidth, out videoHeight);
      }
      aspectX = videoWidth;
      aspectY = videoHeight;
      if (basicVideo != null)
      {
        basicVideo.GetPreferredAspectRatio(out aspectX, out aspectY);
      }
      if (videoHeight != m_iVideoHeight || videoWidth != m_iVideoWidth ||
          aspectX != m_aspectX || aspectY != m_aspectY)
      {
        _updateNeeded = true;
        SetVideoWindow();
      }
    }

    protected virtual void OnProcess() {}

    public override int PositionX
    {
      get { return m_iPositionX; }
      set
      {
        if (value != m_iPositionX)
        {
          m_iPositionX = value;
          _updateNeeded = true;
        }
      }
    }

    public override int PositionY
    {
      get { return m_iPositionY; }
      set
      {
        if (value != m_iPositionY)
        {
          m_iPositionY = value;
          _updateNeeded = true;
        }
      }
    }

    public override int RenderWidth
    {
      get { return m_iWidth; }
      set
      {
        if (value != m_iWidth)
        {
          m_iWidth = value;
          _updateNeeded = true;
        }
      }
    }

    public override int RenderHeight
    {
      get { return m_iHeight; }
      set
      {
        if (value != m_iHeight)
        {
          m_iHeight = value;
          _updateNeeded = true;
        }
      }
    }

    public override double Duration
    {
      get
      {
        if (m_state != PlayState.Init)
        {
          return m_dDuration;
        }
        return 0.0d;
      }
    }

    public override double CurrentPosition
    {
      get
      {
        if (m_state != PlayState.Init)
        {
          return m_dCurrentPos;
        }
        return 0.0d;
      }
    }

    public override bool FullScreen
    {
      get { return GUIGraphicsContext.IsFullScreenVideo; }
      set
      {
        if (value != m_bFullScreen)
        {
          m_bFullScreen = value;
          _updateNeeded = true;
        }
      }
    }

    public override int Width
    {
      get { return m_iVideoWidth; }
    }

    public override int Height
    {
      get { return m_iVideoHeight; }
    }

    public override void Pause()
    {
      if (m_state == PlayState.Paused)
      {
        m_speedRate = 10000;
        if (mediaCtrl != null) mediaCtrl.Run();
        m_state = PlayState.Playing;
      }
      else if (m_state == PlayState.Playing)
      {
        m_state = PlayState.Paused;
        if (mediaCtrl != null) mediaCtrl.Pause();
      }
    }

    public override bool Paused
    {
      get { return (m_state == PlayState.Paused); }
    }

    public override bool Playing
    {
      get { return (m_state == PlayState.Playing || m_state == PlayState.Paused); }
    }

    public override bool Stopped
    {
      get { return (m_state == PlayState.Init); }
    }

    public override bool Initializing
    {
      get { return (m_state == PlayState.Init); }
    }

    public override string CurrentFile
    {
      get { return m_strCurrentFile; }
    }

    public override void Stop()
    {
      if (m_state != PlayState.Init)
      {
        m_state = PlayState.Init;
        Log.Info("VideoPlayer:ended {0}", m_strCurrentFile);
        m_strCurrentFile = "";
        CloseInterfaces();
        m_state = PlayState.Init;
        GUIGraphicsContext.IsPlaying = false;
      }
    }

    private void TrySpeed(double rate, int speed)
    {
      m_speedRate = speed;
      if (mediaSeek != null)
      {
        if (rate > 0)
        {
          int hr = mediaSeek.SetRate(rate);
          if (hr == 0)
          {
            Log.Info("VideoPlayer:Successfully set rate to {0}", rate);
            m_bRateSupport = true;
            if (mediaCtrl != null) mediaCtrl.Run();
            return;
          }
          else
          {
            Log.Info("VideoPlayer:Could not set rate to {0}, error: 0x{1:x}", rate, hr);
          }
        }
        else
        {
          int hr = mediaSeek.SetRate(1.0);
        }
      }
      //fallback to skip steps
      Log.Info("VideoPlayer:Using skip-step fast-forward/rewind, rate {0}", rate);
      m_bRateSupport = false;
    }

    public override int Speed
    {
      get
      {
        if (m_state == PlayState.Init)
        {
          return 1;
        }
        if (mediaSeek == null)
        {
          return 1;
        }
        switch (m_speedRate)
        {
          case -10000:
            return -1;
          case -20000:
            return -2;
          case -40000:
            return -4;
          case -80000:
            return -8;
          case -160000:
            return -16;
          case -320000:
            return -32;
          case 20000:
            return 2;
          case 40000:
            return 4;
          case 80000:
            return 8;
          case 160000:
            return 16;
          case 320000:
            return 32;
          default:
            return 1;
        }
      }
      set
      {
        if (m_state != PlayState.Init)
        {
          if (mediaSeek != null)
          {
            switch ((int)value)
            {
              case -1:
                TrySpeed(-1, -10000);
                break;
              case -2:
                TrySpeed(-2, -20000);
                break;
              case -4:
                TrySpeed(-4, -40000);
                break;
              case -8:
                TrySpeed(-8, -80000);
                break;
              case -16:
                TrySpeed(-16, -160000);
                break;
              case -32:
                TrySpeed(-32, -320000);
                break;
              case 2:
                TrySpeed(2, 20000);
                break;
              case 4:
                TrySpeed(4, 40000);
                break;
              case 8:
                TrySpeed(8, 80000);
                break;
              case 16:
                TrySpeed(16, 160000);
                break;
              case 32:
                TrySpeed(32, 320000);
                break;
              default:
                TrySpeed(1, 10000);
                // mediaCtrl.Run();
                break;
            }

            if (VMR9Util.g_vmr9 != null) VMR9Util.g_vmr9.EVRProvidePlaybackRate((double)value);
            Log.Info("VideoPlayer:SetRate to:{0}", value);

            if (value == 1.0)
            {
              // unmute audio when speed returns to 1.0x
              if (m_volumeBeforeSeeking != 0)
              {
                Volume = m_volumeBeforeSeeking;
              }
              m_volumeBeforeSeeking = 0;
            }
            else
            {
              // mute audio during > 1.0x playback
              if (m_volumeBeforeSeeking == 0)
              {
                m_volumeBeforeSeeking = Volume;
              }
              Volume = 0;
            }
          }
        }
      }
    }

    public override int Volume
    {
      get { return m_iVolume; }
      set
      {
        if (m_iVolume != value)
        {
          m_iVolume = value;
          if (m_state != PlayState.Init)
          {
            if (basicAudio != null)
            {
              // Divide by 100 to get equivalent decibel value. For example, 10,000 is 100 dB. 
              float fPercent = (float)m_iVolume / 100.0f;
              int iVolume = (int)(5000.0f * fPercent);
              basicAudio.put_Volume((iVolume - 5000));
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
        if (m_ar != value)
        {
          m_ar = value;
          _updateNeeded = true;
        }
      }
    }

    #region Seeking

    public override void SeekRelative(double dTime)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          double dCurTime;
          mediaPos.get_CurrentPosition(out dCurTime);
          dTime = dCurTime + dTime;
          if (dTime < 0.0d)
          {
            dTime = 0.0d;
          }
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekAbsolute(double dTime)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          if (dTime < 0.0d)
          {
            dTime = 0.0d;
          }
          if (dTime < Duration)
          {
            mediaPos.put_CurrentPosition(dTime);
          }
        }
      }
    }

    public override void SeekRelativePercentage(int iPercentage)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
        {
          double dCurrentPos;
          mediaPos.get_CurrentPosition(out dCurrentPos);
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
            mediaPos.put_CurrentPosition(fCurPercent);
          }
        }
      }
    }

    public override void SeekAsolutePercentage(int iPercentage)
    {
      if (m_state != PlayState.Init)
      {
        if (mediaCtrl != null && mediaPos != null)
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
          mediaPos.put_CurrentPosition(fPercent);
        }
      }
    }

    #endregion

    public override bool HasVideo
    {
      get
      {
        return (_mediaType == g_Player.MediaType.TV || _mediaType == g_Player.MediaType.Recording ||
                _mediaType == g_Player.MediaType.Video);
      }
    }

    public override bool Ended
    {
      get { return m_state == PlayState.Ended; }
    }

    #region Get/Close Interfaces

    /// <summary> create the used COM components and get the interfaces. </summary>
    // VMR7 is no longer used and GetInterfaces() overridden by VideoPlayerVMR9
    protected abstract bool GetInterfaces();

    protected abstract void DoGraphRebuild();

    /// <summary> do cleanup and release DirectShow. </summary>
    // VMR7 is no longer used and CloseInterfaces() overridden by VideoPlayerVMR9
    protected abstract void CloseInterfaces();

    #endregion

    public override void WndProc(ref Message m)
    {
      if (m.Msg == WM_GRAPHNOTIFY)
      {
        if (mediaEvt != null)
        {
          OnGraphNotify();
        }
        return;
      }
      base.WndProc(ref m);
    }

    private void OnGraphNotify()
    {
      if (mediaEvt == null)
      {
        return;
      }
      int p1, p2, hr = 0;
      EventCode code;
      do
      {
        hr = mediaEvt.GetEvent(out code, out p1, out p2, 0);
        if (hr < 0)
        {
          break;
        }
        hr = mediaEvt.FreeEventParams(code, p1, p2);
        if (code == EventCode.Complete || code == EventCode.ErrorAbort)
        {
          MovieEnded(false);
          /* EABIN: needed for threaded render-thread. please do not delete.
            Action keyAction = new Action(Action.ActionType.ACTION_STOP, 0, 0);
            GUIGraphicsContext.OnAction(keyAction);*/
          return;
        }
      } while (hr == 0);
    }


    protected void DoFFRW()
    {
      if (!Playing || m_bRateSupport == true)
      {
        return;
      }
      if ((m_speedRate == 10000) || (mediaSeek == null) || (mediaCtrl == null))
      {
        return;
      }
      TimeSpan ts = DateTime.Now - elapsedTimer;
      //max out at 5 seeks per second
      //if (ts.TotalMilliseconds < 200)
      if ((ts.TotalMilliseconds < 200) || (ts.TotalMilliseconds < Math.Abs(10000000.0f / m_speedRate)) ||
          ((VMR9Util.g_vmr9 != null && m_lastFrameCounter == VMR9Util.g_vmr9.FreeFrameCounter) &&
           (ts.TotalMilliseconds < 2000))) // It's good to verify a new frame has been presented.
      {
        return;
      }
      long earliest, latest, current, stop, rewind, pStop;
      lock (mediaCtrl)
      {
        elapsedTimer = DateTime.Now;
        mediaSeek.GetAvailable(out earliest, out latest);
        mediaSeek.GetPositions(out current, out stop);
        // Log.Info("earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
        //         earliest/10000000,latest/10000000,current/10000000,stop/10000000,m_speedRate, (latest-earliest)/10000000);
        //earliest += + 30 * 10000000;
        // new time = current time + 2*timerinterval* (speed)
        long lTimerInterval = (long)ts.TotalMilliseconds;
        if (lTimerInterval > 1000)
        {
          lTimerInterval = 1000;
        }
        //rewind = (long)(current + (2 * (long)(lTimerInterval) * m_speedRate));
        rewind = (current + (lTimerInterval * m_speedRate));
        int hr;
        pStop = 0;
        // if we end up before the first moment of time then just
        // start @ the beginning
        if ((rewind < earliest) && (m_speedRate < 0))
        {
          m_speedRate = 10000;
          rewind = earliest;
          Log.Info("VideoPlayer: timeshift SOF seek rew:{0} {1}", rewind, earliest);
          hr = mediaSeek.SetPositions(new DsLong(rewind),
                                      AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                      new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          Speed = 1;
          return;
        }
        // if we end up at the end of time then just
        // start @ the end-100msec
        if ((rewind > (latest - 1000000)) && (m_speedRate > 0))
        {
          m_speedRate = 10000;
          rewind = latest - 1000000;
          Log.Info("VideoPlayer: timeshift EOF seek ff:{0} {1}", rewind, latest);
          hr = mediaSeek.SetPositions(new DsLong(rewind),
                                      AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                      new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
          Speed = 1;
          return;
        }
        //seek to new moment in time
        //Log.Info(" seek :{0}",rewind/10000);
        if (VMR9Util.g_vmr9 != null)
        {
          m_lastFrameCounter = VMR9Util.g_vmr9.FreeFrameCounter;
          VMR9Util.g_vmr9.EVRProvidePlaybackRate(0.0);
        }
        hr = mediaSeek.SetPositions(new DsLong(rewind),
                                    AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                    new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
        mediaCtrl.Run();
        //mediaCtrl.StopWhenReady();
      }
    }

    protected virtual void OnInitialized() {}

    #region subtitle/audio stream selection

    /// <summary>
    /// Property which returns the total number of audio streams available
    /// </summary>
    public override int AudioStreams
    {
      get { return FStreams.GetStreamCount(StreamType.Audio); }
    }

    /// <summary>
    /// Property to get/set the current audio stream
    /// </summary>
    public override int CurrentAudioStream
    {
      get
      {
        int audioStreams = AudioStreams;
        for (int i = 0; i < audioStreams; i++)
        {
          if (FStreams.GetStreamInfos(StreamType.Audio, i).Current)
          {
            return i;
          }
        }
        return 0;
      }
      set
      {
        int audioStreams = AudioStreams;
        for (int i = 0; i < audioStreams; i++)
        {
          if (FStreams.GetStreamInfos(StreamType.Audio, i).Current)
          {
            FStreams.SetCurrentValue(StreamType.Audio, i, false);
          }
        }
        FStreams.SetCurrentValue(StreamType.Audio, value, true);
        EnableStream(FStreams.GetStreamInfos(StreamType.Audio, value).Id, 0,
                     FStreams.GetStreamInfos(StreamType.Audio, value).Filter);
        EnableStream(FStreams.GetStreamInfos(StreamType.Audio, value).Id, AMStreamSelectEnableFlags.Enable,
                     FStreams.GetStreamInfos(StreamType.Audio, value).Filter);

        if (FStreams.GetStreamInfos(StreamType.Audio, value).Filter != MEDIAPORTAL_AUDIOSWITCHER_FILTER && FStreams.GetStreamInfosExternal(StreamType.Audio, 0).Filter == MEDIAPORTAL_AUDIOSWITCHER_FILTER && FStreams.GetStreamInfosExternal(StreamType.Audio, 0).Name == "Audio " && AudioExternal && !AutoRenderingCheck && GetInterface)
        {
          EnableStream(FStreams.GetStreamInfosExternal(StreamType.Audio, 0).Id, 0,
                       FStreams.GetStreamInfosExternal(StreamType.Audio, 0).Filter);
          EnableStream(FStreams.GetStreamInfosExternal(StreamType.Audio, 0).Id, AMStreamSelectEnableFlags.Enable,
                       FStreams.GetStreamInfosExternal(StreamType.Audio, 0).Filter);
        }

        /*if (FStreams.GetStreamInfos(StreamType.Audio, value).Filter != MEDIAPORTAL_AUDIOSWITCHER_FILTER && !AutoRenderingCheck && GetInterface)
        {
          //iChangedMediaTypes = 3;
          //DoGraphRebuild();
        }*/
        return;
      }
    }

    /// <summary>
    /// Property to get the language from stream name
    /// </summary>
    public override string AudioLanguage(int iStream)
    {
      #region return splitter IAMStreamSelect LCID

      int LCIDCheck = FStreams.GetStreamInfos(StreamType.Audio, iStream).LCID;

      if (LCIDCheck != 0)
      {
        int size = Util.Win32API.GetLocaleInfo(LCIDCheck, 2, null, 0);
        if (size > 0)
        {
          string languageName = new string(' ', size);
          Util.Win32API.GetLocaleInfo(LCIDCheck, 2, languageName, size);

          if (!string.IsNullOrEmpty(languageName))
          {
            if (languageName.Contains("\0"))
              languageName = languageName.Substring(0, languageName.IndexOf("\0"));

            if (languageName.Contains("("))
              languageName = languageName.Substring(0, languageName.IndexOf("("));

            return Util.Utils.TranslateLanguageString(languageName.Trim());
          }
        }
      }

      #endregion

      string streamName = FStreams.GetStreamInfos(StreamType.Audio, iStream).Name;

      #region External Audio File

      if (streamName.EndsWith(".mp3") || streamName.EndsWith(".ac3") || streamName.EndsWith(".mka") ||
          streamName.EndsWith(".dts"))
      {
        streamName =
          Path.GetFileNameWithoutExtension(streamName).Replace(Path.GetFileNameWithoutExtension(m_strCurrentFile), "").
            Trim('.');

        if (string.IsNullOrEmpty(streamName))
          streamName = GUILocalizeStrings.Get(2599);
        else
          streamName = Util.Utils.TranslateLanguageString(streamName);

        return streamName;
      }

      #endregion

      // No stream info from splitter
      if (streamName.Contains(Path.GetFileName(m_strCurrentFile)))
        return GUILocalizeStrings.Get(2599);

      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^A: ", "");
      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[.+\]");
      //Audio - English, Dolby Digital, 48.0 kHz, 6 chn, 640.0 kbit/s 
      //Audio - Dolby TrueHD, 48.0 kHz, 6 chn, 640.0 kbit/s (1100,fd,00)
      Regex regexMPS = new Regex(@"Audio\s*-\s*(?<1>.+?),\s*.+?,\s*.+?,\s*.+?,\s*.+", RegexOptions.IgnoreCase);
      Regex regexMPAUDIONoType = new Regex(@"^(.+?)(?<!]*,\s.+?)\s\((Audio\s.+?)$");
      Regex regexMPAUDIO = new Regex(@"^(.+?)]*,\s(.+?)\s\((Audio\s.+?)$");
      Regex regexMPC = new Regex("([^, ]+)");
      Match result = regex.Match(streamName);
      Match resultMPS = regexMPS.Match(streamName);
      Match resultMPAUDIONoType = regexMPAUDIONoType.Match(streamName);
      Match resultMPAUDIO = regexMPAUDIO.Match(streamName);
      Match resultMPC = regexMPC.Match(streamName);

      if (result.Success)
      {
        //Cut off and translate the language part
        string language = Util.Utils.TranslateLanguageString(streamName.Substring(result.Index + 1, result.Length - 2));
        //Get the trackname part by removing the language part from the string.
        streamName = regex.Replace(streamName, "").Trim();
        //Put things back together
        //streamName = language + (streamName == string.Empty ? "" : " [" + streamName + "]");
        if (language.Length > 0) // && streamName.Length <= 0)
        {
          streamName = language;
        }
      }
      else if (resultMPS.Success)
        // check for mpegsplitter response format, e.g.:  
        // Audio - English, Dolby Digital, 48.0 kHz, 6 chn, 640.0 kbit/s 
      {
        string language = Util.Utils.TranslateLanguageString(resultMPS.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      else if (resultMPAUDIO.Success)
        // check for mpc-hc audio switcher response format, e.g.: 
        // Language, Trackname (Audio 2)
      {
        string language = Util.Utils.TranslateLanguageString(resultMPAUDIO.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      else if (resultMPAUDIONoType.Success)
        // check for mpc-hc audio switcher response format, e.g.: 
        // Language (Audio 2)
      {
        string language = Util.Utils.TranslateLanguageString(resultMPAUDIONoType.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      else if (resultMPC.Success)
        // check for mpc-hc audio response format, e.g.: 
        // English, DTS-HD MA core 1536k (Audio 1) - 48000 Hz, 6 channels dts (libavcodec)
      {
        string language = Util.Utils.TranslateLanguageString(resultMPC.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }

      // Remove extraneous info from splitter in parenthesis at end of line, e.g.:
      // English, DTS-HD MA core 1536k (Audio 1) - 48000 Hz, 6 channels dts (libavcodec)
      // Audio - Dolby TrueHD, 48.0 kHz, 6 chn, 640.0 kbit/s (1100,fd,00)
      streamName = Regex.Replace(streamName, @"\(.+?\)$", "");

      return streamName;
    }

    /// <summary>
    /// Property to get the type of an audio stream
    /// </summary>
    public override string AudioType(int iStream)
    {
      string streamName = FStreams.GetStreamInfos(StreamType.Audio, iStream).Name;
      string streamNameFalse = FStreams.GetStreamInfos(StreamType.Audio, iStream).Name;
      if (streamName.EndsWith(".mp3") || streamName.EndsWith(".ac3") || streamName.EndsWith(".mka") ||
          streamName.EndsWith(".dts"))
      {
        return Path.GetExtension(streamName).ToUpper().Replace(".", "EXTERNAL ");
      }

      // No stream info from splitter
      if (streamName.Contains(Path.GetFileName(m_strCurrentFile)))
        return Path.GetExtension(m_strCurrentFile).ToUpper().Replace(".", "");

      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^A: ", "");

      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[.+\]");
      //Audio - English, Dolby Digital, 48.0 kHz, 6 chn, 640.0 kbit/s 
      //Audio - Dolby TrueHD, 48.0 kHz, 6 chn, 640.0 kbit/s (1100,fd,00)
      Regex regexMPS = new Regex(@"Audio\s*-\s*.+?,\s*(?<1>.+?,\s*.+?,\s*.+?,\s*.+)", RegexOptions.IgnoreCase);
      Regex regexMPSNoLang = new Regex(@"Audio\s*-\s*(?<1>.+?,\s*.+?,\s*.+?,\s*.+)", RegexOptions.IgnoreCase);
      Regex regexLAVF =
        new Regex(
          @"(?:A:\s)(?<lang_or_title>.+?)(?:\s*\[(?<lang>[^\]]*?)\])?(?:\s*\((?<info>[^\)]*?)\))?(?:\s*\[(?<Default>[^\]]*?)\])?$");
      Regex regexMPAUDIONoType = new Regex(@"^(.+?)(?<!]*,\s.+?)\s\((Audio\s.+?)$");
      Regex regexMPAUDIO = new Regex(@"^(.+?)]*,\s(.+?)\s\((Audio\s.+?)$");
      Regex regexMPC = new Regex(@"\S+,\s+(?<1>.+)");
      Match result = regex.Match(streamName);
      Match resultMPS = regexMPS.Match(streamName);
      Match resultMPSNoLang = regexMPSNoLang.Match(streamName);
      Match resultMPAUDIO = regexMPAUDIO.Match(streamName);
      Match resultMPAUDIONoType = regexMPAUDIONoType.Match(streamName);
      Match resultMPC = regexMPC.Match(streamName);
      Match resultLAVF = regexLAVF.Match(streamNameFalse);

      if (resultLAVF.Success)
        // check for LAVF response format, e.g.: 
        // S: Title [Lang] (Info) when only Language in stream -> answer is S: Lang -> start to detect if [lang] is present if not replace Lang by "" 
      {
        string lang_or_title = resultLAVF.Groups[1].Value;
        string lang = resultLAVF.Groups[2].Value;
        string info = resultLAVF.Groups[3].Value;
        if (!string.IsNullOrEmpty(info))
        {
          if (!string.IsNullOrEmpty(lang))
          {
            streamName = "" + lang_or_title + "]" + " [" + info + "";
          }
          else
          {
            streamName = info;
          }
        }
        else if (string.IsNullOrEmpty(info))
        {
          streamName = regex.Replace(streamName, "").Trim();
        }
      }
      else if (result.Success)
      {
        //Get the trackname part by removing the language part from the string.
        streamName = regex.Replace(streamName, "").Trim();
      }
      else if (resultMPS.Success)
        // check for mpegsplitter response format, e.g.:  
        // Audio - English, Dolby Digital, 48.0 kHz, 6 chn, 640.0 kbit/s 
      {
        string audioType = Util.Utils.TranslateLanguageString(resultMPS.Groups[1].Value);
        if (audioType.Length > 0)
        {
          streamName = audioType;
        }
      }
      else if (resultMPSNoLang.Success)
        // check for mpegsplitter response format, e.g.:  
        // Audio - Dolby Digital, 48.0 kHz, 6 chn, 640.0 kbit/s 
      {
        string audioType = Util.Utils.TranslateLanguageString(resultMPSNoLang.Groups[1].Value);
        if (audioType.Length > 0)
        {
          streamName = audioType;
        }
      }
      else if (resultMPAUDIO.Success)
        // check for mpc-hc audio switcher response format, e.g.: 
        // Language, Trackname (Audio 2)
      {
        string audioType = Util.Utils.TranslateLanguageString(resultMPAUDIO.Groups[2].Value).TrimStart();
        if (audioType.Length > 0)
        {
          streamName = audioType;
        }
      }
      else if (resultMPAUDIONoType.Success)
        // check for mpc-hc audio switcher response format, e.g.: 
        // Language (Audio 2)
      {
        streamName = "";
      }
      else if (resultMPC.Success)
        // check for mpc-hc audio response format, e.g.: 
        // English, DTS-HD MA core 1536k (Audio 1) - 48000 Hz, 6 channels dts (libavcodec)
      {
        string audioType = Util.Utils.TranslateLanguageString(resultMPC.Groups[1].Value);
        if (audioType.Length > 0)
        {
          streamName = audioType;
        }
      }
      // Remove extraneous info from splitter in parenthesis at end of line, e.g.:
      // English, DTS-HD MA core 1536k (Audio 1) - 48000 Hz, 6 channels dts (libavcodec)
      // Audio - Dolby TrueHD, 48.0 kHz, 6 chn, 640.0 kbit/s (1100,fd,00)
      streamName = Regex.Replace(streamName, @"\(.+?\)$", "");
      return streamName;
    }

    /// <summary>
    /// Property to get the total number of subtitle streams
    /// </summary>
    public override int SubtitleStreams
    {
      get
      {
        int ss = 0;
        ISubEngine t = SubEngine.GetInstance();
        try
        {
          ss = t.GetCount();
        }
        catch (Exception ex)
        {
          Log.Warn("get_SubtitleStreams: {0}", ex.Message);
        }
        return ss;
      }
    }

    /// <summary>
    /// Property to get/set the current subtitle stream
    /// </summary>
    public override int CurrentSubtitleStream
    {
      get { return SubEngine.GetInstance().Current; }
      set { SubEngine.GetInstance().Current = value; }
    }

    /// <summary>
    /// Property to get/set the name for a subtitle stream
    /// </summary>
    public override string SubtitleLanguage(int iStream)
    {
      string streamName = SubEngine.GetInstance().GetLanguage(iStream);
      string langName = SubEngine.GetInstance().GetLanguage(iStream);
      string streamNameUND = SubEngine.GetInstance().GetSubtitleName(iStream);

      if (streamName == null)
      {
        return Strings.Unknown;
      }

      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^S: ", "");
      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[([^\]]+)\]");
      Regex regexFFD = new Regex(@"\[.+\]");
      Regex regexLAVF =
        new Regex(
          @"(?:S:\s)(?<lang_or_title>.+?)(?:\s*\[(?<lang>[^\]]*?)\])?(?:\s*\((?<info>[^\)]*?)\))?(?:\s*\[(?<Default>[^\]]*?)\])?$");
      // For example MPC Splitter and MPC Engine returns mkv streams as: "language (trackname)",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regexMPCEngine = new Regex(@"(\w.+)\((\D+[^\]]+)\)"); //(@"(\w.+)\(([^\]]+)\)");
      Match result = regex.Match(streamName);
      Match resultFFD = regexFFD.Match(streamName);
      Match resultMPCEngine = regexMPCEngine.Match(streamName);
      Match resultLAVF = regexLAVF.Match(streamNameUND);
      if (result.Success)
      {
        string language = Util.Utils.TranslateLanguageString(result.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      else if (resultFFD.Success)
      {
        string subtitleLanguage = Util.Utils.TranslateLanguageString(resultFFD.Groups[0].Value);
        if (subtitleLanguage.Length > 0)
        {
          streamName = subtitleLanguage;
        }
      }
      else if (resultMPCEngine.Success)
        // check for mpc-hc engine response format, e.g.: 
        // Language (Trackname)
      {
        streamName = resultMPCEngine.Groups[1].Value.TrimEnd();
      }
      else if (resultLAVF.Success)
        // check for LAVF response format, e.g.: 
        // S: Title [Lang] (Info) here is to detect if langID = 0 so the language is set as Undetermined
      {
        string lang_or_title = resultLAVF.Groups[1].Value;
        string lang = resultLAVF.Groups[2].Value;
        string info = resultLAVF.Groups[3].Value;
        streamNameUND = Regex.Replace(streamNameUND, @"^S: ", "");
        if (lang_or_title == streamNameUND && lang_or_title == streamName && lang_or_title != langName &&
            string.IsNullOrEmpty(lang) && string.IsNullOrEmpty(info))
          //|| lang_or_title.Contains("Stream #") && string.IsNullOrEmpty(info)) //string.IsNullOrEmpty(lang_or_title) && string.IsNullOrEmpty(lang))
        {
          streamName = "Undetermined";
        }
      }
      // mpeg splitter subtitle format
      Match m = Regex.Match(streamName, @"Subtitle\s+-\s+(?<1>.+?),", RegexOptions.IgnoreCase);
      if (m.Success)
      {
        string language = Util.Utils.TranslateLanguageString(m.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      return streamName;
    }

    public override string SubtitleName(int iStream)
    {
      string streamName = SubEngine.GetInstance().GetSubtitleName(iStream);
      string streamNameFalse = SubEngine.GetInstance().GetSubtitleName(iStream);
      string langName = SubEngine.GetInstance().GetLanguage(iStream);
      if (streamName == null)
      {
        return Strings.Unknown;
      }
      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^S: ", "");

      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[([^\]]+)\]");
      Regex regexFFDShow = new Regex(@"\s\[.+\]");
      Regex regexMPCEngine = new Regex(@"\((\D+[^\]]+)\)");
      Regex regexLAVF =
        new Regex(@"(?:S:\s)(?<lang_or_title>.+?)(?:\s*\[(?<lang>[^\]]*?)\])?(?:\s*\((?<info>[^\)]*?)\))?$");
      Match result = regex.Match(streamName);
      Match resultFFDShow = regexFFDShow.Match(streamName);
      Match resultMPCEngine = regexMPCEngine.Match(streamName);
      Match resultLAVF = regexLAVF.Match(streamNameFalse);
      if (resultFFDShow.Success)
      {
        //Get the trackname part by removing the language part from the string.
        streamName = regex.Replace(streamName, "").Trim();

        //Put things back together
        streamName = (streamName == string.Empty ? "" : "" + streamName + "");
      }
      else if (result.Success)
      {
        //if language only is detected -> set to ""
        streamName = "";
      }
      else if (resultMPCEngine.Success)
        // check for mpc-hc engine response format, e.g.: 
        // Language (Trackname)
      {
        //Get the trackname.
        streamName = resultMPCEngine.Groups[1].Value;
      }
      else if (resultLAVF.Success)
        // check for LAVF response format, e.g.: 
        // S: Title [Lang] (Info) when only Language in stream -> answer is S: Lang -> start to detect if [lang] is present if not replace Lang by "" 
      {
        string lang_or_title = resultLAVF.Groups[1].Value;
        string lang = resultLAVF.Groups[2].Value;
        string info = resultLAVF.Groups[3].Value;
        if (lang_or_title == langName || lang_or_title.Contains("Stream #") && string.IsNullOrEmpty(info))
        {
          streamName = "";
        }
      }
      // mpeg splitter subtitle format
      Match m = Regex.Match(streamName, @"Subtitle\s+-\s+(?<1>.+?),", RegexOptions.IgnoreCase);
      if (m.Success)
      {
        string language = Util.Utils.TranslateLanguageString(m.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = "";
        }
      }

      #region Remove the false detection of Language Name when is detected as Stream Name

      //Look if Language Name is equal Stream Name, if it's equal, remove it.
      if (streamName == langName)
      {
        streamName = "";
      }

      #endregion

      return streamName;
    }

    public override bool EnableSubtitle
    {
      get { return SubEngine.GetInstance().Enable; }
      set { SubEngine.GetInstance().Enable = value; }
    }

    public override bool EnableForcedSubtitle
    {
      get { return SubEngine.GetInstance().AutoShow; }
      set { SubEngine.GetInstance().AutoShow = value; }
    }

    public bool AnalyseStreams()
    {
      try
      {
        if (FStreams == null)
        {
          FStreams = new FilterStreams();
        }
        FStreams.DeleteAllStreams();
        //RETRIEVING THE CURRENT SPLITTER
        string filter;
        IBaseFilter[] foundfilter = new IBaseFilter[2];
        int fetched = 0;
        IEnumFilters enumFilters;
        graphBuilder.EnumFilters(out enumFilters);
        if (enumFilters != null)
        {
          enumFilters.Reset();
          while (enumFilters.Next(1, foundfilter, out fetched) == 0)
          {
            if (foundfilter[0] != null && fetched == 1)
            {
              if (chapters == null)
              {
                IAMExtendedSeeking pEs = foundfilter[0] as IAMExtendedSeeking;
                if (pEs != null)
                {
                  int markerCount = 0;
                  if (pEs.get_MarkerCount(out markerCount) == 0 && markerCount > 0)
                  {
                    chapters = new double[markerCount];
                    chaptersname = new string[markerCount];
                    for (int i = 1; i <= markerCount; i++)
                    {
                      double markerTime = 0;
                      pEs.GetMarkerTime(i, out markerTime);
                      chapters[i - 1] = markerTime;
                      //fill up chapter names
                      string name = null;
                      pEs.GetMarkerName(i, out name);
                      chaptersname[i - 1] = name;
                    }
                  }
                }
              }
              IAMStreamSelect pStrm = foundfilter[0] as IAMStreamSelect;
              if (pStrm != null)
              {
                FilterInfo foundfilterinfos = new FilterInfo();
                foundfilter[0].QueryFilterInfo(out foundfilterinfos);
                filter = foundfilterinfos.achName;
                int cStreams = 0;
                pStrm.Count(out cStreams);
                if (cStreams < 2)
                {
                  continue;
                }
                //GET STREAMS
                for (int istream = 0; istream < cStreams; istream++)
                {
                  AMMediaType sType;
                  AMStreamSelectInfoFlags sFlag;
                  int sPDWGroup, sPLCid;
                  string sName;
                  object pppunk, ppobject;
                  //STREAM INFO
                  pStrm.Info(istream, out sType, out sFlag, out sPLCid,
                             out sPDWGroup, out sName, out pppunk, out ppobject);
                  FilterStreamInfos FSInfos = new FilterStreamInfos();
                  FSInfos.Current = false;
                  FSInfos.Filter = filter;
                  FSInfos.Name = sName;
                  FSInfos.LCID = sPLCid;
                  FSInfos.Id = istream;
                  FSInfos.Type = StreamType.Unknown;
                  //Avoid listing ffdshow video filter's plugins amongst subtitle and audio streams and editions.
                  if ((FSInfos.Filter == "ffdshow DXVA Video Decoder" || FSInfos.Filter == "ffdshow Video Decoder" ||
                       FSInfos.Filter == "ffdshow raw video filter") &&
                      ((sPDWGroup == 1) || (sPDWGroup == 2) || (sPDWGroup == 18) || (sPDWGroup == 4)))
                  {
                    FSInfos.Type = StreamType.Unknown;
                  }
                  //VIDEO
                  else if (sPDWGroup == 0)
                  {
                    FSInfos.Type = StreamType.Video;
                  }
                  //AUDIO
                  else if (sPDWGroup == 1)
                  {
                    FSInfos.Type = StreamType.Audio;
                  }
                  //SUBTITLE
                  else if (sPDWGroup == 2 && sName.LastIndexOf("off") == -1 && sName.LastIndexOf("Hide ") == -1 &&
                           sName.LastIndexOf("No ") == -1 && sName.LastIndexOf("Miscellaneous ") == -1)
                  {
                    FSInfos.Type = StreamType.Subtitle;
                  }
                  //NO SUBTITILE TAG
                  else if ((sPDWGroup == 2 && (sName.LastIndexOf("off") != -1 || sName.LastIndexOf("No ") != -1)) ||
                           (sPDWGroup == 6590033 && sName.LastIndexOf("Hide ") != -1))
                  {
                    FSInfos.Type = StreamType.Subtitle_hidden;
                  }
                  //DirectVobSub SHOW SUBTITLE TAG
                  else if (sPDWGroup == 6590033 && sName.LastIndexOf("Show ") != -1)
                  {
                    FSInfos.Type = StreamType.Subtitle_shown;
                  }
                  //EDITION
                  else if (sPDWGroup == 18)
                  {
                    FSInfos.Type = StreamType.Edition;
                  }
                  else if (sPDWGroup == 4) //Subtitle file
                  {
                    FSInfos.Type = StreamType.Subtitle_file;
                  }
                  else if (sPDWGroup == 10) //Postprocessing filter
                  {
                    FSInfos.Type = StreamType.PostProcessing;
                  }
                  Log.Debug("VideoPlayer: FoundStreams: Type={0}; Name={1}, Filter={2}, Id={3}, PDWGroup={4}, LCID={5}",
                            FSInfos.Type.ToString(), FSInfos.Name, FSInfos.Filter, FSInfos.Id.ToString(),
                            sPDWGroup.ToString(), sPLCid.ToString());

                  switch (FSInfos.Type)
                  {
                    case StreamType.Unknown:
                    case StreamType.Subtitle:
                    case StreamType.Subtitle_file:
                      break;
                    case StreamType.Video:
                    case StreamType.Audio:
                    case StreamType.Edition:
                    case StreamType.PostProcessing:
                      if (FSInfos.Type == StreamType.Audio && FSInfos.Filter == MEDIAPORTAL_AUDIOSWITCHER_FILTER && FSInfos.Name == "Audio " && !AutoRenderingCheck && GetInterface)
                      {
                        FStreams.AddStreamInfosEx(FSInfos);
                        break;
                      }
                      if (FStreams.GetStreamCount(FSInfos.Type) == 0)
                      {
                        FSInfos.Current = true;
                        pStrm.Enable(FSInfos.Id, 0);
                        pStrm.Enable(FSInfos.Id, AMStreamSelectEnableFlags.Enable);
                        /*if (FSInfos.Type == StreamType.Audio && FSInfos.Filter != MEDIAPORTAL_AUDIOSWITCHER_FILTER && GetInterface && !AutoRenderingCheck)
                        {
                          iChangedMediaTypes = 1;
                          //DoGraphRebuild();
                        }*/
                      }
                      goto default;
                    default:
                      FStreams.AddStreamInfos(FSInfos);
                      break;
                  }
                }
              }
              DirectShowUtil.ReleaseComObject(foundfilter[0]);
            }
          }
          DirectShowUtil.ReleaseComObject(enumFilters);
        }
      }
      catch { }
      return true;
    }

    public bool EnableStream(int Id, AMStreamSelectEnableFlags dwFlags, string Filter)
    {
      try
      {
        IBaseFilter foundfilter = DirectShowUtil.GetFilterByName(graphBuilder, Filter);
        if (foundfilter != null)
        {
          IAMStreamSelect pStrm = foundfilter as IAMStreamSelect;
          if (pStrm != null)
          {
            pStrm.Enable(Id, dwFlags);
          }
          pStrm = null;
          DirectShowUtil.ReleaseComObject(foundfilter);
        }
      }
      catch {}
      return true;
    }

    public override bool HasPostprocessing
    {
      get { return PostProcessingEngine.GetInstance().HasPostProcessing; }
    }

    #endregion

    #region edition selection

    /// <summary>
    /// Property to get the language for an edition stream
    /// </summary>
    public override string EditionLanguage(int iStream)
    {
      string streamName = FStreams.GetStreamInfos(StreamType.Edition, iStream).Name;
      return streamName;
    }

    public override string EditionType(int iStream)
    {
      string streamName = FStreams.GetStreamInfos(StreamType.Edition, iStream).Name;
      return streamName;
    }

    public override int EditionStreams
    {
      get { return FStreams.GetStreamCount(StreamType.Edition); }
    }

    /// <summary>
    /// Property to get/set the current edition stream
    /// </summary>
    public override int CurrentEditionStream
    {
      get
      {
        for (int i = 0; i < FStreams.GetStreamCount(StreamType.Edition); i++)
        {
          if (FStreams.GetStreamInfos(StreamType.Edition, i).Current)
          {
            return i;
          }
        }
        return 0;
      }
      set
      {
        for (int i = 0; i < FStreams.GetStreamCount(StreamType.Edition); i++)
        {
          if (FStreams.GetStreamInfos(StreamType.Edition, i).Current)
          {
            FStreams.SetCurrentValue(StreamType.Edition, i, false);
          }
        }
        FStreams.SetCurrentValue(StreamType.Edition, value, true);
        EnableStream(FStreams.GetStreamInfos(StreamType.Edition, value).Id, 0,
                     FStreams.GetStreamInfos(StreamType.Edition, value).Filter);
        EnableStream(FStreams.GetStreamInfos(StreamType.Edition, value).Id, AMStreamSelectEnableFlags.Enable,
                     FStreams.GetStreamInfos(StreamType.Edition, value).Filter);
        Log.Info("VideoPlayer:Edition Duration Change:{0}", m_dDuration);
        return;
      }
    }

    #endregion

    #region video selection

    /// <summary>
    /// Property to get the language for an edition stream
    /// </summary>
    public override string VideoLanguage(int iStream)
    {
      #region return splitter IAMStreamSelect LCID

      int LCIDCheck = FStreams.GetStreamInfos(StreamType.Video, iStream).LCID;

      if (LCIDCheck != 0)
      {
        int size = Util.Win32API.GetLocaleInfo(LCIDCheck, 2, null, 0);
        if (size > 0)
        {
          string languageName = new string(' ', size);
          Util.Win32API.GetLocaleInfo(LCIDCheck, 2, languageName, size);

          if (!string.IsNullOrEmpty(languageName))
          {
            if (languageName.Contains("\0"))
              languageName = languageName.Substring(0, languageName.IndexOf("\0"));

            if (languageName.Contains("("))
              languageName = languageName.Substring(0, languageName.IndexOf("("));

            return Util.Utils.TranslateLanguageString(languageName.Trim());
          }
        }
      }

      #endregion

      string streamName = FStreams.GetStreamInfos(StreamType.Video, iStream).Name;

      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^V: ", "");
      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[.+\]");
      Regex regexMPS = new Regex(@"Video\s*-\s*(?<1>.+?),\s*.+?,\s*.+?,\s*.+?,\s*.+", RegexOptions.IgnoreCase);
      Regex regexMPVIDEONoType = new Regex(@"^(.+?)(?<!]*,\s.+?)\s\((Video\s.+?)$");
      Regex regexMPVIDEO = new Regex(@"^(.+?)]*,\s(.+?)\s\((Video\s.+?)$");
      //Regex regexMPC = new Regex("([^, ]+)");
      Match result = regex.Match(streamName);
      Match resultMPS = regexMPS.Match(streamName);
      Match resultMPVIDEONoType = regexMPVIDEONoType.Match(streamName);
      Match resultMPVIDEO = regexMPVIDEO.Match(streamName);
      //Match resultMPC = regexMPC.Match(streamName);

      if (result.Success)
      {
        //Cut off and translate the language part
        string language = Util.Utils.TranslateLanguageString(streamName.Substring(result.Index + 1, result.Length - 2));
        //Get the trackname part by removing the language part from the string.
        streamName = regex.Replace(streamName, "").Trim();
        //Put things back together
        //streamName = language + (streamName == string.Empty ? "" : " [" + streamName + "]");
        if (language.Length > 0) // && streamName.Length <= 0)
        {
          streamName = language;
        }
      }
      else if (resultMPS.Success)
      {
        string language = Util.Utils.TranslateLanguageString(resultMPS.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      else if (resultMPVIDEO.Success)
      {
        string language = Util.Utils.TranslateLanguageString(resultMPVIDEO.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      else if (resultMPVIDEONoType.Success)
      {
        string language = Util.Utils.TranslateLanguageString(resultMPVIDEONoType.Groups[1].Value);
        if (language.Length > 0)
        {
          streamName = language;
        }
      }
      // Remove extraneous info from splitter in parenthesis at end of line:
      streamName = Regex.Replace(streamName, @"\(.+?\)$", "");
      return streamName;
    }

    public override string VideoType(int iStream)
    {
      string streamName = FStreams.GetStreamInfos(StreamType.Video, iStream).Name;
      string streamNameFalse = FStreams.GetStreamInfos(StreamType.Video, iStream).Name;

      // No stream info from splitter
      if (streamName.Contains(Path.GetFileName(m_strCurrentFile)))
        return Path.GetExtension(m_strCurrentFile).ToUpper().Replace(".", "");

      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^V: ", "");

      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[.+\]");
      //Audio - English, Dolby Digital, 48.0 kHz, 6 chn, 640.0 kbit/s 
      //Audio - Dolby TrueHD, 48.0 kHz, 6 chn, 640.0 kbit/s (1100,fd,00)
      Regex regexMPS = new Regex(@"Video\s*-\s*.+?,\s*(?<1>.+?,\s*.+?,\s*.+?,\s*.+)", RegexOptions.IgnoreCase);
      Regex regexMPSNoLang = new Regex(@"Video\s*-\s*(?<1>.+?,\s*.+?,\s*.+?,\s*.+)", RegexOptions.IgnoreCase);
      Regex regexLAVF =
        new Regex(
          @"(?:V:\s)(?<lang_or_title>.+?)(?:\s*\[(?<lang>[^\]]*?)\])?(?:\s*\((?<info>[^\)]*?)\))?(?:\s*\[(?<Default>[^\]]*?)\])?$");
      Regex regexMPC = new Regex(@"\S+,\s+(?<1>.+)");
      Match result = regex.Match(streamName);
      Match resultMPS = regexMPS.Match(streamName);
      Match resultMPSNoLang = regexMPSNoLang.Match(streamName);
      Match resultMPC = regexMPC.Match(streamName);
      Match resultLAVF = regexLAVF.Match(streamNameFalse);

      if (resultLAVF.Success)
      // check for LAVF response format, e.g.: 
      // S: Title [Lang] (Info) when only Language in stream -> answer is S: Lang -> start to detect if [lang] is present if not replace Lang by "" 
      {
        string lang_or_title = resultLAVF.Groups[1].Value;
        string lang = resultLAVF.Groups[2].Value;
        string info = resultLAVF.Groups[3].Value;
        if (!string.IsNullOrEmpty(info))
        {
          if (!string.IsNullOrEmpty(lang))
          {
            streamName = "" + lang_or_title + " [" + info + "]";
          }
          else
          {
            streamName = info;
          }
        }
        else if (string.IsNullOrEmpty(info))
        {
          streamName = regex.Replace(streamName, "").Trim();
        }
      }
      else if (result.Success)
      {
        //Get the trackname part by removing the language part from the string.
        streamName = regex.Replace(streamName, "").Trim();
      }
      else if (resultMPS.Success)
      // check for mpegsplitter response format, e.g.:      
      {
        string videoType = Util.Utils.TranslateLanguageString(resultMPS.Groups[1].Value);
        if (videoType.Length > 0)
        {
          streamName = videoType;
        }
      }
      else if (resultMPSNoLang.Success)
      // check for mpegsplitter response format:
      {
        string videoType = Util.Utils.TranslateLanguageString(resultMPSNoLang.Groups[1].Value);
        if (videoType.Length > 0)
        {
          streamName = videoType;
        }
      }
      else if (resultMPC.Success)
      // check for mpc-hc Video response format, e.g.: 
      {
        string videoType = Util.Utils.TranslateLanguageString(resultMPC.Groups[1].Value);
        if (videoType.Length > 0)
        {
          streamName = videoType;
        }
      }
      // Remove extraneous info from splitter in parenthesis at end of line:      
      streamName = Regex.Replace(streamName, @"\(.+?\)$", "");
      return streamName;
    }

    public override int VideoStreams
    {
      get { return FStreams.GetStreamCount(StreamType.Video); }
    }

    /// <summary>
    /// Property to get/set the current edition stream
    /// </summary>
    public override int CurrentVideoStream
    {
      get
      {
        for (int i = 0; i < FStreams.GetStreamCount(StreamType.Video); i++)
        {
          if (FStreams.GetStreamInfos(StreamType.Video, i).Current)
          {
            return i;
          }
        }
        return 0;
      }
      set
      {
        for (int i = 0; i < FStreams.GetStreamCount(StreamType.Video); i++)
        {
          if (FStreams.GetStreamInfos(StreamType.Video, i).Current)
          {
            FStreams.SetCurrentValue(StreamType.Video, i, false);
          }
        }
        FStreams.SetCurrentValue(StreamType.Video, value, true);
        EnableStream(FStreams.GetStreamInfos(StreamType.Video, value).Id, 0,
                     FStreams.GetStreamInfos(StreamType.Video, value).Filter);
        EnableStream(FStreams.GetStreamInfos(StreamType.Video, value).Id, AMStreamSelectEnableFlags.Enable,
                     FStreams.GetStreamInfos(StreamType.Video, value).Filter);
        Log.Info("VideoPlayer:Video Duration Change:{0}", m_dDuration);
        return;
      }
    }

    #endregion

    #region IDisposable Members

    public override void Dispose()
    {
      CloseInterfaces();
    }

    #endregion

    public override bool IsTV
    {
      get { return (_mediaType == g_Player.MediaType.TV || _mediaType == g_Player.MediaType.Recording); }
    }

    public override bool IsTimeShifting
    {
      get { return false; }
    }

    public override bool IsRadio
    {
      get { return _mediaType == g_Player.MediaType.Radio; }
    }
  }
}