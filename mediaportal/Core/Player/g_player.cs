using System;
using System.Windows.Forms;
using System.Drawing;




using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Subtitle;

namespace MediaPortal.Player 
{
  public class g_Player
  {
    public enum Steps :int 
    { 
      Hourm2=-2*60*60,
      Hourm1=-60*60,
      Minm30=-30*60, 
      Minm15=-15*60,
      Minm10=-10*60,
      Minm5=-5*60, 
      Minm3=-3*60,
      Minm1=-1*60,
      Secm30=-30,   
      Secm15=-15,
      Sec0=0,
      Sec15=15, 
      Sec30=30, 
      Min1=1*60, 
      Min3=3*60, 
      Min5=5*60, 
      Min10=10*60, 
      Min15=15*60, 
      Min30=30*60,
      Hour1=60*60,
      Hour2=2*60*60
    };
    static Steps                     m_currentStep=Steps.Sec0;
    static DateTime                  m_SeekTimer=DateTime.MinValue;

    static Player.IPlayer            m_player=null;
    static SubTitles                 m_subs=null;        
    static bool                      m_bInit=false;
    static IRender                   m_renderFrame;
    static string                    CurrentFilePlaying="";
    
    public enum MediaType  { Video, TV, Radio, Music,Recording };
    public delegate void StoppedHandler(MediaType type, int stoptime, string filename);
    public delegate void EndedHandler(MediaType type, string filename);
    public delegate void StartedHandler(MediaType type, string filename);
    static public event StoppedHandler PlayBackStopped;
    static public event EndedHandler PlayBackEnded;
    static public event StartedHandler PlayBackStarted;

    // singleton. Dont allow any instance of this class
    private g_Player()
    {
    }
    public static Player.IPlayer Player
    {
      get { return m_player;}
    }

    //called when current playing file is stopped
    static void OnStopped()
    {
      //check if we're playing
      if (g_Player.Playing && PlayBackStopped!=null)
      {
        //yes, then raise event 
        MediaType type=MediaType.Music;
        if (g_Player.IsTV) 
        {
          type=MediaType.TV;
          if (!m_player.IsTimeShifting) 
            type=MediaType.Recording;
        }
        else if (g_Player.IsRadio) type=MediaType.Radio;
        else if (g_Player.IsVideo) type=MediaType.Video;
        PlayBackStopped(type,(int)g_Player.CurrentPosition, g_Player.CurrentFile);
      }
    }

    //called when current playing file is stopped
    static void OnEnded()
    {
      //check if we're playing
      if (PlayBackEnded!=null)
      {
        //yes, then raise event 
        MediaType type=MediaType.Music;
        if (g_Player.IsTV) 
        {
          type=MediaType.TV;
          if (!m_player.IsTimeShifting) 
            type=MediaType.Recording;
        }
        else if (g_Player.IsRadio) type=MediaType.Radio;
        else if (g_Player.IsVideo) type=MediaType.Video;
        PlayBackEnded(type, CurrentFilePlaying);
      }
    }
    //called when starting playing a file
    static void OnStarted()
    {
      //check if we're playing
      if (m_player==null) return;
      if (m_player.Playing && PlayBackStarted!=null)
      {
        //yes, then raise event 
        MediaType type=MediaType.Music;
        if (g_Player.IsTV) 
        {
          type=MediaType.TV;
          if (!m_player.IsTimeShifting) 
            type=MediaType.Recording;
        }
        else if (g_Player.IsRadio) type=MediaType.Radio;
        else if (g_Player.IsVideo) type=MediaType.Video;
        if (PlayBackStarted!=null)        
          PlayBackStarted(type, CurrentFilePlaying);
      }
    }
    public static void Stop()
    {
      if (m_player!=null)
      {
        OnStopped();
        GUIGraphicsContext.ShowBackground=true;
        m_player.Stop();
        GUIGraphicsContext.form.Invalidate(true);
        GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED,0,0,0,0,0,null);
        GUIWindowManager.SendThreadMessage(msg);
        GUIGraphicsContext.IsFullScreenVideo=false;
        GUIGraphicsContext.IsPlaying=false;
        GUIGraphicsContext.IsPlayingVideo=false;
        m_player.Release();
        m_player=null;
      }
    }
    
    public static void Pause()
    {
      if (m_player!=null)
      {
        m_currentStep=Steps.Sec0;
        m_SeekTimer=DateTime.MinValue;
        m_player.Pause();
      }
    }
    public static bool OnAction(Action action)
    {
      if (m_player!=null)
      {
        return m_player.OnAction(action);
      }
      return false;
    }

    public static bool IsDVD
    {
      get 
      {
        if (m_player==null) return false;
        return m_player.IsDVD;
      }
    }
    public static bool IsTV
    {
      get 
      {
        if (m_player==null) return false;
        return m_player.IsTV;
      }
    }
    public static bool IsTimeShifting
    {
      get 
      {
        if (m_player==null) return false;
        return m_player.IsTimeShifting;
      }
    }

    public static void Release()
    {
      if (m_player!=null)
      {
        m_player.Stop();
        m_player.Release();
        m_player=null;
      }
    }
    public static bool PlayDVD()
    {
      return PlayDVD("");
    }

    public static bool PlayDVD(string strPath)
    {
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
      m_subs=null;
      if (m_player!=null)
      {
        GUIGraphicsContext.ShowBackground=true;
        OnStopped();
        m_player.Stop();
        GUIGraphicsContext.form.Invalidate(true);
        m_player=null;
      }

      if (Utils.PlayDVD()) return true;
      int iUseVMR9=0;
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        iUseVMR9=xmlreader.GetValueAsInt("dvdplayer","vmr9",0);
      }

      if (iUseVMR9!=0)
        m_player = new DVDPlayer9();
      else        
        m_player = new DVDPlayer();

      m_player.RenderFrame=m_renderFrame;
      bool bResult=m_player.Play(strPath);
      if (!bResult)
      {
        Log.Write("dvdplayer:failed to play");
        m_player.Release();
        m_player=null;
        m_subs=null;
        GC.Collect();GC.Collect();GC.Collect();
        Log.Write("dvdplayer:bla");
      }
      else if (m_player.Playing)
      {
        if (!m_player.IsTV)
        {
          GUIGraphicsContext.IsFullScreenVideo=true;
          GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
        }
        CurrentFilePlaying=m_player.CurrentFile;
        OnStarted();
        return true;
      }
      Log.Write("dvdplayer:sendmsg");

      //show dialog:unable to play dvd,
      GUIWindowManager.ShowWarning(722,723,-1);
      return false;
    }

    public static bool PlayAudioStream(string strURL)
    {
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
      m_bInit=true;
      m_subs=null;
      Log.Write("Player.Play({0})",strURL);
      if (m_player!=null)
      {
        GUIGraphicsContext.ShowBackground=true;
        OnStopped();
        m_player.Stop();
        GUIGraphicsContext.form.Invalidate(true);
        m_player=null;
      }
      m_player = new AudioPlayerWMP9();

      bool bResult=m_player.Play(strURL);
      if (!bResult)
      {
        Log.Write("player:ended");
        m_player.Release();
        m_player=null;
        m_subs=null;
        GC.Collect();GC.Collect();GC.Collect();
      }
      else if (m_player.Playing)
      {
        CurrentFilePlaying=m_player.CurrentFile;
        OnStarted();
      }
      m_bInit=false;
      return bResult;
    }

    public static bool Play(string strFile)
    {
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
      if (strFile==null) return false;
      if (strFile.Length==0) return false;
      m_bInit=true;
      m_subs=null;
      Log.Write("Player.Play({0})",strFile);
      if (m_player!=null)
      {
        GUIGraphicsContext.ShowBackground=true;
        OnStopped();
        m_player.Stop();
        m_player=null;
        GC.Collect();GC.Collect();GC.Collect();GC.Collect();
      }
      if (Utils.IsVideo(strFile))
      {
        if (Utils.PlayMovie(strFile))
        {
          m_bInit=false;
          return false;
        }
        string strExt = System.IO.Path.GetExtension(strFile).ToLower();
        if (strExt==".ifo"|| strExt==".vob")
        {

          int iUseVMR9=0;
          using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
          {
            iUseVMR9=xmlreader.GetValueAsInt("dvdplayer","vmr9",0);
          }

          if (iUseVMR9!=0)
            m_player = new DVDPlayer9();
          else        
            m_player = new DVDPlayer();
          m_player.RenderFrame=m_renderFrame;

          bool bResult=m_player.Play(strFile);
          if (!bResult)
          {
            Log.Write("player:ended");
            m_player.Release();
            m_player=null;
            m_subs=null;
            GC.Collect();GC.Collect();GC.Collect();
          }
          else if (m_player.Playing)
          {
            CurrentFilePlaying=m_player.CurrentFile;
            OnStarted();

            GUIGraphicsContext.IsFullScreenVideo=true;
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
          }
          m_bInit=false;
          return bResult;
        }
      }
      m_player = PlayerFactory.Create(m_renderFrame, strFile);
      if (m_player!=null)
      {
        if (Utils.IsVideo(strFile))
        {
          //string strSubFile=System.IO.Path.ChangeExtension(strFile,".srt");
          //m_subs=SubReader.ReadTag(strSubFile);
          //if (m_subs==null)
          //{
          //  strSubFile=System.IO.Path.ChangeExtension(strFile,".smi");
          //  m_subs=SubReader.ReadTag(strSubFile);
          //}
          //if (m_subs==null)
          //{
          //  strSubFile=System.IO.Path.ChangeExtension(strFile,".sami");
          //  m_subs=SubReader.ReadTag(strSubFile);
          //}
          //if (m_subs!=null)
          //{
          //  m_subs.LoadSettings();
          //}
        }

        bool bResult=m_player.Play(strFile);
        if (!bResult)
        {
          Log.Write("player:ended");
          m_player.Release();
          m_player=null;
          m_subs=null;
          GC.Collect();GC.Collect();GC.Collect();
        }
        else if (m_player.Playing)
        {
          CurrentFilePlaying=m_player.CurrentFile;
          OnStarted();
        }
        m_bInit=false;
        return bResult;
      }
      m_bInit=false;
      return false;
    }

    public static bool IsRadio
    {
      get 
      {
        if (m_player==null) return false;
        return m_player.IsRadio;
      }
    }
    public static bool Playing
    {
      get 
      { 
        if (m_bInit) return false;
        if (m_player==null) return false;
        bool bResult=m_player.Playing;
        return bResult;
      }
    }
    
    public static bool Paused
    {
      get 
      { 
        if (m_player==null) return false;
        return m_player.Paused;
      }
    }
    public static bool Stopped
    {
      get 
      { 
        if (m_bInit) return false;
        if (m_player==null) return false;
        bool bResult=m_player.Stopped;
        return bResult;
      }
    }

    public static int Speed
    {
      get 
      { 
        if (m_player==null) return 1;
        return m_player.Speed;
      }
      set 
      {
        if (m_player==null) return ;
        m_player.Speed=value;
        m_currentStep=Steps.Sec0;
        m_SeekTimer=DateTime.MinValue;
      }
    }


    public static string CurrentFile
    {
      get 
      { 
        if (m_player==null) return "";
        return m_player.CurrentFile;
      }
    }

    static public int Volume
    {
      get { 
        if (m_player==null) return 0;
        return m_player.Volume;
      }
      set 
      {
        if (m_player != null)
        {
          m_player.Volume=value;
        }
      }
    }

    public static Geometry.Type ARType
    {
      get { return GUIGraphicsContext.ARType;}
      set 
      {
        if (m_player != null)
        {
          m_player.ARType=value;
        }
      }
    }

    static public int PositionX
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.PositionX;
      }
      set 
      {
        if (m_player != null)
        {
          m_player.PositionX=value;
        }
      }
    }

    static public int PositionY
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.PositionY;
      }
      set 
      {
        if (m_player != null)
        {
          m_player.PositionY=value;
        }
      }
    }

    static public int RenderWidth
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.RenderWidth;
      }
      set 
      {
        if (m_player != null)
        {
          m_player.RenderWidth=value;
        }
      }
    }
    static public bool Visible
    {
        get 
     { 
       if (m_player==null) return false;
       return m_player.Visible;
     }
      set 
      {
        if (m_player != null)
        {
          m_player.Visible=value;
        }
      }
    }
    static public int RenderHeight
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.RenderHeight;
      }
      set 
      {
        if (m_player != null)
        {
          m_player.RenderHeight=value;
        }
      }
    }

    static public double Duration
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.Duration;
      }
    }

    static public double CurrentPosition
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.CurrentPosition;
      }
    }
    static public double ContentStart
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.ContentStart;
      }
    }

    static public bool FullScreen
    {
      get 
      { 
        if (m_player==null) return GUIGraphicsContext.IsFullScreenVideo;
        return m_player.FullScreen;
      }
      set 
      {
        if (m_player != null)
        {
          m_player.FullScreen=value;
        }
      }
    }
    static public int Width
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.Width;
      }
    }

    static public int Height
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.Height;
      }
    }
    static public void SeekRelative(double dTime)
    {
      if (m_player==null) return ;
      m_player.SeekRelative(dTime);
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
    }

    static public void   StepNow()
    {
      if (m_currentStep!=Steps.Sec0 && m_player!=null) 
      {
        double dTime=(int)m_currentStep+m_player.CurrentPosition;
        if (dTime<0) dTime=0d;
        if (dTime>m_player.Duration) dTime=m_player.Duration-5;
        m_player.SeekAbsolute(dTime);
      }
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
      
    }
    static public string GetStepDescription()
    {
      if (m_player==null) return "";
      int m_iTimeToStep =(int)m_currentStep;
      if (m_iTimeToStep==0) return "";
      if (m_player.CurrentPosition+m_iTimeToStep <= 0) return "START";//start
      if (m_player.CurrentPosition+m_iTimeToStep >= m_player.Duration) return "END";
      switch (m_currentStep)
      {
        case Steps.Hourm2: return "-2hrs";
        case Steps.Hourm1: return "-1hr";
        case Steps.Minm30: return "-30min";
        case Steps.Minm15: return "-15min";
        case Steps.Minm10: return "-10min";
        case Steps.Minm5 : return "-5min";
        case Steps.Minm3 : return "-3min";
        case Steps.Minm1 : return "-1min";        
        case Steps.Secm30 : return "-30sec";
        case Steps.Secm15 : return "-15sec";      
        case Steps.Sec0 : return "0 sec";        
        case Steps.Sec15 : return "+15sec";        
        case Steps.Sec30 : return "+30sec";        
        case Steps.Min1 : return "+1min";          
        case Steps.Min3 : return "+3min";          
        case Steps.Min5 : return "+5min";          
        case Steps.Min10 : return "+10min";        
        case Steps.Min15 : return "+15min";        
        case Steps.Min30 : return "+30min";
        case Steps.Hour1: return "+1hr";
        case Steps.Hour2: return "+2hrs";        
      }
      return "";
    }
    static public int GetSeekStep(out bool bStart, out bool bEnd)
    {
      bStart=false;
      bEnd=false;
      if (m_player==null) return 0;
      int m_iTimeToStep=(int)m_currentStep;
      if (m_player.CurrentPosition+m_iTimeToStep <= 0) bStart=true;//start
      if (m_player.CurrentPosition+m_iTimeToStep >= m_player.Duration) bEnd=true;
      return m_iTimeToStep;
    }

    static public void SeekStep(bool bFF)
    {
      if (bFF)
      {
        switch (m_currentStep)
        {
          case Steps.Hourm2: m_currentStep=Steps.Hourm1;break;
          case Steps.Hourm1: m_currentStep=Steps.Minm30;break;
          case Steps.Minm30: m_currentStep=Steps.Minm15;break;
          case Steps.Minm15: m_currentStep=Steps.Minm10;break;
          case Steps.Minm10: m_currentStep=Steps.Minm5;break;
          case Steps.Minm5: m_currentStep=Steps.Minm3;break;
          case Steps.Minm3: m_currentStep=Steps.Minm1;break;
          case Steps.Minm1: m_currentStep=Steps.Secm30;break;
          case Steps.Secm30: m_currentStep=Steps.Secm15;break;
          case Steps.Secm15: m_currentStep=Steps.Sec0;break;

          case Steps.Sec0:  m_currentStep=Steps.Sec15;break;

          case Steps.Sec15: m_currentStep=Steps.Sec30;break;
          case Steps.Sec30: m_currentStep=Steps.Min1;break;
          case Steps.Min1:  m_currentStep=Steps.Min3;break;
          case Steps.Min3:  m_currentStep=Steps.Min5;break;
          case Steps.Min5:  m_currentStep=Steps.Min10;break;
          case Steps.Min10: m_currentStep=Steps.Min15;break;
          case Steps.Min15: m_currentStep=Steps.Min30;break;
          case Steps.Min30: m_currentStep=Steps.Hour1;break;
          case Steps.Hour1: m_currentStep=Steps.Hour2;break;
          case Steps.Hour2: break;
        }
      }
      else
      {
        switch (m_currentStep)
        {
          case Steps.Hourm2:  break;
          case Steps.Hourm1:  m_currentStep=Steps.Hourm2;break;
          case Steps.Minm30:  m_currentStep=Steps.Hourm1;break;
          case Steps.Minm15: m_currentStep=Steps.Minm30;break;
          case Steps.Minm10: m_currentStep=Steps.Minm15;break;
          case Steps.Minm5: m_currentStep=Steps.Minm10;break;
          case Steps.Minm3: m_currentStep=Steps.Minm5;break;
          case Steps.Minm1: m_currentStep=Steps.Minm3;break;
          case Steps.Secm30: m_currentStep=Steps.Minm1;break;
          case Steps.Secm15: m_currentStep=Steps.Secm30;break;

          case Steps.Sec0:  m_currentStep=Steps.Secm15;break;

          case Steps.Sec15: m_currentStep=Steps.Sec0;break;
          case Steps.Sec30: m_currentStep=Steps.Sec15;break;
          case Steps.Min1:  m_currentStep=Steps.Sec30;break;
          case Steps.Min3:  m_currentStep=Steps.Min1;break;
          case Steps.Min5:  m_currentStep=Steps.Min3;break;
          case Steps.Min10: m_currentStep=Steps.Min5;break;
          case Steps.Min15: m_currentStep=Steps.Min10;break;
          case Steps.Min30: m_currentStep=Steps.Min15; break;
          case Steps.Hour1: m_currentStep=Steps.Min30; break;
          case Steps.Hour2: m_currentStep=Steps.Hour1; break;
        }
      }
      m_SeekTimer=DateTime.Now;
    }

    static public void SeekRelativePercentage(int iPercentage)
    {
      if (m_player==null) return ;
      m_player.SeekRelativePercentage(iPercentage);
      
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
    }

    static public void SeekAbsolute(double dTime)
    {
      if (m_player==null) return ;
      m_player.SeekAbsolute(dTime);
      
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
    }

    static public void SeekAsolutePercentage(int iPercentage)
    {
      if (m_player==null) return ;
      m_player.SeekAsolutePercentage(iPercentage);
      
      m_currentStep=Steps.Sec0;
      m_SeekTimer=DateTime.MinValue;
    }
    static public bool HasVideo
    {
      get {
        if (m_player==null) return false;
        return m_player.HasVideo;
      }
    }
    static public bool IsVideo
    {
      get 
      {
        if (m_player==null) return false;
        if (!m_player.HasVideo) return false;
        if (m_player.HasVideo)
        {
          if ( Utils.IsAudio (m_player.CurrentFile) )
          {
            return false;
          }

          if ( m_player.IsRadio )
          {
            return false;
          }
        }
        return true;
      }
    }

    static public bool HasSubs
    {
      get 
      {
        if (m_player==null) return false;
        return (m_subs!=null);
      }
    }
    static public void RenderSubtitles()
    {
      if (m_player==null) return ;
      if (m_subs==null) return ;
      if (HasSubs)
      {
        m_subs.Render( m_player.CurrentPosition );
      }
    }
    static public void WndProc( ref Message m )
    {
      if (m_player==null) return;
      m_player.WndProc(ref m);
    }

    
    static public void Process()
    {
      if (m_player==null) return ;
      m_player.Process();
      if (!m_player.Playing)
      {
        if (m_player.Ended)
        {
          GUIMessage msg=new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED,0,0,0,0,0,null);
          GUIWindowManager.SendThreadMessage(msg);
          OnEnded();
          return;
        }
        Stop();
      }
      else
      {

        if (m_currentStep!=Steps.Sec0)
        {
          TimeSpan ts = DateTime.Now-m_SeekTimer;
          if (ts.TotalMilliseconds>1500)
          {
            StepNow();
          }
        }
      }
    }

    static public int AudioStreams
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.AudioStreams;
      }
    }
    static public int CurrentAudioStream
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.CurrentAudioStream;
      }
      set 
      {
        if (m_player!=null) 
        {
          m_player.CurrentAudioStream=value;
        }
      }
    }
    static public string AudioLanguage(int iStream)
    {
      if (m_player==null) return "Unknown";
      return m_player.AudioLanguage(iStream);
    }

    static public int SubtitleStreams
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.SubtitleStreams;
      }
    }
    static public int CurrentSubtitleStream
    {
      get 
      { 
        if (m_player==null) return 0;
        return m_player.CurrentSubtitleStream;
      }
      set {
        if (m_player!=null) 
        {
          m_player.CurrentSubtitleStream=value;
        }
      }
    }
    static public void SetVideoWindow()
    {
      if (m_player==null) return ;
      m_player.SetVideoWindow();
    }

    static public string SubtitleLanguage(int iStream)
    {
      if (m_player==null) return "Unknown";
      return m_player.SubtitleLanguage(iStream);
    }
    static public bool EnableSubtitle
    {
      get 
      {
        if (m_player==null) return false;
        return m_player.EnableSubtitle;
      }
      set 
      {
        if (m_player==null) return ;
        m_player.EnableSubtitle=value;
      }
    }

    public static void Init(IRender renderFrame)
    {
      m_renderFrame=renderFrame;
      GUIGraphicsContext.OnVideoWindowChanged += new VideoWindowChangedHandler(g_Player.OnVideoWindowChanged);
      GUIGraphicsContext.OnGammaContrastBrightnessChanged += new VideoGammaContrastBrightnessHandler(g_Player.OnGammaContrastBrightnessChanged);
    }

    static void OnGammaContrastBrightnessChanged()
    {
      if (!Playing) return;
      if (!HasVideo) return;
      if (m_player==null) return;
      m_player.Contrast=GUIGraphicsContext.Contrast;
      m_player.Brightness=GUIGraphicsContext.Brightness;
      m_player.Gamma=GUIGraphicsContext.Gamma;
    }

    static void OnVideoWindowChanged()
    {
      if (!Playing) return;
      if (!HasVideo) return;

      FullScreen=GUIGraphicsContext.IsFullScreenVideo;
      ARType=GUIGraphicsContext.ARType;
      if (!FullScreen)
      {
        PositionX=GUIGraphicsContext.VideoWindow.Left;
        PositionY=GUIGraphicsContext.VideoWindow.Top;
        RenderWidth=GUIGraphicsContext.VideoWindow.Width;
        RenderHeight=GUIGraphicsContext.VideoWindow.Height;
      }
      bool inTV=false;
      int windowId=GUIWindowManager.ActiveWindow;
      if (windowId==(int)GUIWindow.Window.WINDOW_TV||
          windowId==(int)GUIWindow.Window.WINDOW_TVGUIDE||  
          windowId==(int)GUIWindow.Window.WINDOW_SEARCHTV||  
          windowId==(int)GUIWindow.Window.WINDOW_SCHEDULER||  
          windowId==(int)GUIWindow.Window.WINDOW_RECORDEDTV)
        inTV=true;
      Visible=(FullScreen||GUIGraphicsContext.Overlay||
          windowId==(int)GUIWindow.Window.WINDOW_SCHEDULER||inTV);
      SetVideoWindow();
    }

    /// <summary>
    /// returns video window rectangle
    /// </summary>
    static public Rectangle VideoWindow
    {
      get { 
        if (m_player==null) return new Rectangle(0,0,0,0);
        return m_player.VideoWindow;
      }
    }

    /// <summary>
    /// returns video source rectangle displayed
    /// </summary>
    static public Rectangle SourceWindow
    {
      get 
      { 
        if (m_player==null) return new Rectangle(0,0,0,0);
        return m_player.SourceWindow;
      }
    }
    static public int GetHDC()
    {
      if (m_player==null) return 0;
      return m_player.GetHDC();
    }

    static public void ReleaseHDC(int HDC)
    {
      if (m_player==null) return ;
      m_player.ReleaseHDC(HDC);
    }
    static public  bool DoesOwnRendering
    {
      get { 
        if (m_player==null) return false;
        return m_player.DoesOwnRendering;
      }
    }
    static public bool CanSeek
    {
      get 
      { 
        if (m_player==null) return false;
        return m_player.CanSeek();
      }
    }
  }
}
