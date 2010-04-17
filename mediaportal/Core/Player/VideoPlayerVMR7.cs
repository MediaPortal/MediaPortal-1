#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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


namespace MediaPortal.Player
{
  public abstract class VideoPlayerVMR7 : IPlayer
  {
    protected const int MAX_STREAMS = 100;

    protected struct FilterStreamInfos
    {
      public int Id;
      public string Name;
      public bool Current;
      public string Filter;
      public StreamType Type;
    } ;

    protected class FilterStreams
    {
      public FilterStreams()
      {
        cStreams = 0;
        Streams = new FilterStreamInfos[MAX_STREAMS];
      }

      public FilterStreamInfos GetStreamInfos(StreamType Type, int Id)
      {
        FilterStreamInfos empty = new FilterStreamInfos();
        for (int i = 0; i < cStreams; i++)
        {
          if (Type == Streams[i].Type)
          {
            if (Id == 0)
            {
              return Streams[i];
            }
            Id--;
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

      public bool AddStreamInfos(FilterStreamInfos StreamInfos)
      {
        if (cStreams == MAX_STREAMS)
        {
          return false;
        }
        Streams[cStreams] = StreamInfos;
        cStreams++;
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
      }

      private FilterStreamInfos[] Streams;
      private int cStreams;
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
    private DateTime elapsedTimer = DateTime.Now;

    protected const string defaultLanguageCulture = "EN";

    public override double[] Chapters
    {
      get { return chapters; }
    }

    private VMR7Util vmr7 = null;
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
        AnalyseStreams();
        SelectSubtitles();
        SelectAudioLanguage();
        OnInitialized();
      }
      return true;
    }

    private void SelectSubtitles()
    {
      CultureInfo ci = null;
      bool showSubtitles = true;

      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          ci = new CultureInfo(xmlreader.GetValueAsString("subtitles", "language", defaultLanguageCulture));
          showSubtitles = xmlreader.GetValueAsBool("subtitles", "enabled", true);
        }
        catch (Exception ex)
        {
          ci = new CultureInfo(defaultLanguageCulture);
          Log.Error(
            "SelectSubtitleLanguage - unable to build CultureInfo, make sure MediaPortal.xml is not corrupted! - {0}",
            ex);
        }
      }
      for (int i = 0; i < SubtitleStreams; i++)
      {
        string subtitleLanguage = SubtitleLanguage(i);
        if (ci.EnglishName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase) ||
            ci.TwoLetterISOLanguageName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterISOLanguageName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterWindowsLanguageName.Equals(subtitleLanguage, StringComparison.OrdinalIgnoreCase))
        {
          CurrentSubtitleStream = i;
          break;
        }
      }
      EnableSubtitle = showSubtitles;
    }

    private void SelectAudioLanguage()
    {
      CultureInfo ci = null;
      using (Settings xmlreader = new MPSettings())
      {
        try
        {
          ci = new CultureInfo(xmlreader.GetValueAsString("movieplayer", "audiolanguage", defaultLanguageCulture));
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
            ci.TwoLetterISOLanguageName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterISOLanguageName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase) ||
            ci.ThreeLetterWindowsLanguageName.Equals(AudioLanguage(i), StringComparison.OrdinalIgnoreCase))
        {
          CurrentAudioStream = i;
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
      if (ts.TotalMilliseconds >= 800 || m_speedRate != 1)
      {
        if (mediaPos != null)
        {
          //mediaPos.get_Duration(out m_dDuration);
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
      if (m_speedRate != 1)
      {
        DoFFRW();
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

    protected virtual void OnProcess()
    {
      if (vmr7 != null)
      {
        vmr7.Process();
      }
    }

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
        mediaCtrl.Run();
        m_state = PlayState.Playing;
      }
      else if (m_state == PlayState.Playing)
      {
        m_state = PlayState.Paused;
        mediaCtrl.Pause();
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
        int hr = mediaSeek.SetRate(rate);
        if (hr == 0)
        {
          Log.Debug("Successfully set rate to {0}", rate);
          m_bRateSupport = true;
          return;
        }
        Log.Debug("Could not set rate to {0}, error: 0x{1:x}", rate, hr);
      }
      //fallback to skip steps
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
                TrySpeed(-2, -15000);
                break;
              case -4:
                TrySpeed(-4, -30000);
                break;
              case -8:
                TrySpeed(-8, -45000);
                break;
              case -16:
                TrySpeed(-16, -60000);
                break;
              case -32:
                TrySpeed(-32, -75000);
                break;
              case 1:
                TrySpeed(1, 10000);
                mediaCtrl.Run();
                break;
              case 2:
                TrySpeed(2, 15000);
                break;
              case 4:
                TrySpeed(4, 30000);
                break;
              case 8:
                TrySpeed(8, 45000);
                break;
              case 16:
                TrySpeed(16, 60000);
                break;
              default:
                TrySpeed(32, 75000);
                break;
            }
          }
        }
        VMR9Util.g_vmr9.EVRProvidePlaybackRate((double)value);
        Log.Info("VideoPlayer:SetRate to:{0}", m_speedRate);
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
              // Divide by 100 to get equivalent decibel value. For example, �10,000 is �100 dB. 
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
      if (!Playing)
      {
        return;
      }
      if ((m_speedRate == 10000) || (mediaSeek == null) || m_bRateSupport == true)
      {
        return;
      }
      TimeSpan ts = DateTime.Now - elapsedTimer;
      //max out at 10 seeks per second
      if (ts.TotalMilliseconds < 100)
      {
        return;
      }
      elapsedTimer = DateTime.Now;
      long earliest, latest, current, stop, rewind, pStop;
      mediaSeek.GetAvailable(out earliest, out latest);
      mediaSeek.GetPositions(out current, out stop);
      // Log.Info("earliest:{0} latest:{1} current:{2} stop:{3} speed:{4}, total:{5}",
      //         earliest/10000000,latest/10000000,current/10000000,stop/10000000,m_speedRate, (latest-earliest)/10000000);
      //earliest += + 30 * 10000000;
      // new time = current time + 2*timerinterval* (speed)
      long lTimerInterval = (long)ts.TotalMilliseconds;
      if (lTimerInterval > 300)
      {
        lTimerInterval = 300;
      }
      lTimerInterval = 300;
      rewind = (long)(current + (2 * (long)(lTimerInterval) * m_speedRate));
      int hr;
      pStop = 0;
      // if we end up before the first moment of time then just
      // start @ the beginning
      if ((rewind < earliest) && (m_speedRate < 0))
      {
        m_speedRate = 10000;
        rewind = earliest;
        //Log.Info(" seek back:{0}",rewind/10000000);
        hr = mediaSeek.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop),
                                    AMSeekingSeekingFlags.NoPositioning);
        mediaCtrl.Run();
        return;
      }
      // if we end up at the end of time then just
      // start @ the end-100msec
      if ((rewind > (latest - 100000)) && (m_speedRate > 0))
      {
        m_speedRate = 10000;
        rewind = latest - 100000;
        //Log.Info(" seek ff:{0}",rewind/10000000);
        hr = mediaSeek.SetPositions(new DsLong(rewind), AMSeekingSeekingFlags.AbsolutePositioning, new DsLong(pStop),
                                    AMSeekingSeekingFlags.NoPositioning);
        mediaCtrl.Run();
        return;
      }
      //seek to new moment in time
      //Log.Info(" seek :{0}",rewind/10000);
      hr = mediaSeek.SetPositions(new DsLong(rewind),
                                  AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.SeekToKeyFrame,
                                  new DsLong(pStop), AMSeekingSeekingFlags.NoPositioning);
      mediaCtrl.StopWhenReady();
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
        for (int i = 0; i < FStreams.GetStreamCount(StreamType.Audio); i++)
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
        for (int i = 0; i < FStreams.GetStreamCount(StreamType.Audio); i++)
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
        return;
      }
    }

    /// <summary>
    /// Property to get the language for an audio stream
    /// </summary>
    public override string AudioLanguage(int iStream)
    {
      string streamName = FStreams.GetStreamInfos(StreamType.Audio, iStream).Name;
      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^A: ", "");
      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[.+\]");
      Match result = regex.Match(streamName);
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
      return streamName;
    }

    /// <summary>
    /// Property to get the name for an audio stream
    /// </summary>
    public override string AudioType(int iStream)
    {
      string streamName = FStreams.GetStreamInfos(StreamType.Audio, iStream).Name;
      // remove prefix, which is added by Haali Media Splitter
      streamName = Regex.Replace(streamName, @"^A: ", "");
      // Check if returned string contains both language and trackname info
      // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
      // where "trackname" is stream's "trackname" property muxed in the mkv.
      Regex regex = new Regex(@"\[.+\]");
      Match result = regex.Match(streamName);
      if (result.Success)
      {
        //Get the trackname part by removing the language part from the string.
        streamName = regex.Replace(streamName, "").Trim();
      }
      return streamName;
    }

    /// <summary>
    /// Property to get the total number of subtitle streams
    /// </summary>
    public override int SubtitleStreams
    {
      get { return SubEngine.GetInstance().GetCount(); }
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
      if (streamName == null)
      {
        return Strings.Unknown;
      }
      // Sometimes underline engine returns Haali mkv streams as: "S: trackname [language]"
      Regex regex = new Regex(@"\[([^\]]+)\]");
      Match result = regex.Match(streamName);
      if (result.Success)
      {
        streamName = result.Groups[1].Value;
      }

      return streamName;
    }

    public override bool EnableSubtitle
    {
      get { return SubEngine.GetInstance().Enable; }
      set { SubEngine.GetInstance().Enable = value; }
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
                    for (int i = 1; i <= markerCount; i++)
                    {
                      double markerTime = 0;
                      pEs.GetMarkerTime(i, out markerTime);
                      chapters[i - 1] = markerTime;
                      //there is no usage to chapter's names right now
                      //string name = null;
                      //pEs.GetMarkerName(i, out name);
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
                  FSInfos.Id = istream;
                  FSInfos.Type = StreamType.Unknown;
                  //Avoid listing ffdshow video filter's plugins amongst subtitle and audio streams.
                  if ((FSInfos.Filter == "ffdshow Video Decoder" || FSInfos.Filter == "ffdshow raw video filter") &&
                      ((sPDWGroup == 1) || (sPDWGroup == 2)))
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
                  Log.Debug("VideoPlayer: FoundStreams: Type={0}; Name={1}, Filter={2}, Id={3}, PDWGroup={4}",
                            FSInfos.Type.ToString(), FSInfos.Name, FSInfos.Filter, FSInfos.Id.ToString(),
                            sPDWGroup.ToString());

                  switch (FSInfos.Type)
                  {
                    case StreamType.Unknown:
                      break;
                    case StreamType.Video:
                    case StreamType.Audio:
                    case StreamType.Subtitle:
                      if (FStreams.GetStreamCount(FSInfos.Type) == 0)
                      {
                        FSInfos.Current = true;
                        pStrm.Enable(FSInfos.Id, 0);
                        pStrm.Enable(FSInfos.Id, AMStreamSelectEnableFlags.Enable);
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
      catch {}
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