using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

using MediaPortal.GUI.Library;
using MediaPortal;

using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;
using MediaPortal.TV.Recording;
using MediaPortal.Dialogs;

public class MediaPortalApp : D3DApp, IRender
{

    int       m_iLastMousePositionX=0;
    int       m_iLastMousePositionY=0;
    private System.Threading.Mutex m_Mutex;
    private string m_UniqueIdentifier;

  const int WM_KEYDOWN    =0x0100;
  const int WM_SYSCOMMAND =0x0112;
  const int SC_SCREENSAVE =0xF140;
		[STAThread]
    public static void Main()
    {
      
        bool bDirectXInstalled=false;
        bool bWindowsMediaPlayer9=false;
        string strVersion="";
        RegistryKey hkcu =Registry.CurrentUser;
        hkcu.CreateSubKey(@"Software\MediaPortal");
        RegistryKey hklm =Registry.LocalMachine;
        
        hklm.CreateSubKey(@"Software\MediaPortal");
        RegistryKey subkey=hklm.OpenSubKey(@"Software\Microsoft\DirectX");
        if (subkey!=null)
        {
          strVersion=(string)subkey.GetValue("Version");
          if (strVersion!=null)
          {
            if (strVersion.Length>0)
            {
              string strTmp="";
              for (int i=0; i < strVersion.Length;++i)
              {
                if (Char.IsDigit(strVersion[i]) ) strTmp += strVersion[i];
              }
              long lVersion = System.Convert.ToInt64(strTmp);
              if (lVersion >=409000902)
              {
                bDirectXInstalled=true;
              }
            }
            else strVersion="?";
          }
          else strVersion="?";
          subkey.Close();
          subkey=null;
        }
        subkey=hklm.OpenSubKey(@"Software\Microsoft\MediaPlayer\9.0");
        if (subkey!=null)
        {
          bWindowsMediaPlayer9=true;
          subkey.Close();
          subkey=null;
        }
        hklm.Close();
        hklm=null;

        if (!bDirectXInstalled)
        {
          string strLine="Please install DirectX 9.0b!\r\n";
          strLine=strLine+ "Current version installed:"+strVersion+"\r\n\r\n";
          strLine=strLine+ "Mediaportal cannot run without DirectX 9.0b";
          System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
//TESTTESTTEST
bWindowsMediaPlayer9=true;

        if (!bWindowsMediaPlayer9)
        {
          string strLine="Please install Windows Mediaplayer 9\r\n";
          strLine=strLine+ "Mediaportal cannot run without Windows Mediaplayer 9";
          System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }
        
        if (bDirectXInstalled && bWindowsMediaPlayer9)
        {

          MediaPortalApp app = new MediaPortalApp();
          if( app.CreateGraphicsSample() )
          {
            //app.PreRun();
            Log.Write("Start MediaPortal");
#if DEBUG
						app.Run();
#else
            try
            {
              app.Run();
            }
            catch (Exception ex)
            {
              Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message,ex.Source, ex.StackTrace);
            }
#endif
            app.OnExit();
            Log.Write("MediaPortal done");
            Win32API.EnableStartBar(true);
            Win32API.ShowStartBar(true);
          }
        }
    }

    public MediaPortalApp()
		{
      m_UniqueIdentifier = Application.ExecutablePath.Replace("\\","_");
      m_Mutex = new System.Threading.Mutex(false, m_UniqueIdentifier);
      if(!m_Mutex.WaitOne(1,true))
      {
        string strMsg="Mediaportal is already running!";
        System.Windows.Forms.MessageBox.Show(strMsg, this.Text, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        throw new Exception(strMsg);
      }
      Utils.FileDelete("capture.log");
      if (Screen.PrimaryScreen.Bounds.Width>720)
      {
        this.MinimumSize = new Size(720+8,576+27);
      }
      else
      {
        this.MinimumSize = new Size(720,576);
      }
			this.Text = "Media Portal";
      
      Log.Write("-------------------------------------------------------------------------------");
      Log.Write("starting");
      GUIGraphicsContext.form=this;
      GUIGraphicsContext.graphics=null;
      GUIGraphicsContext.ActionHandlers += new OnActionHandler(this.OnAction);
      try
      {
        using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
        {
          m_strSkin=xmlreader.GetValueAsString("skin","name","MetalMedia");
          m_strLanguage=xmlreader.GetValueAsString("skin","language","English");
          m_bAutoHideMouse=xmlreader.GetValueAsBool("general","autohidemouse",false);
          GUIGraphicsContext.MouseSupport=xmlreader.GetValueAsBool("general","mousesupport",true);
        }
      }
      catch(Exception)
      {
        m_strSkin="CrystalCenter";
        m_strLanguage="english";
      }
     

      GUIWindowManager.Callbacks+=new GUIWindowManager.OnCallBackHandler(this.Process);
      GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STARTING;

      // load keymapping from keymap.xml
      ActionTranslator.Load();

      //register the playlistplayer for thread messages (like playback stopped,ended)
      Log.Write("Init playlist player");
      PlayListPlayer.Init();

      //registers the player for video window size notifications
      Log.Write("Init players");
      g_Player.Init(this);
      Log.Write("done");
    }

  public override bool PreProcessMessage(ref Message msg)
  {
    if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("pre keydown");

    return base.PreProcessMessage (ref msg);
  }

    protected override void WndProc( ref Message msg )
    {
      
      if (msg.Msg==WM_SYSCOMMAND && msg.WParam.ToInt32()==SC_SCREENSAVE)
      {
        // windows wants to activate the screensaver
        if (GUIGraphicsContext.IsFullScreenVideo) 
        {
          //disable it when we're watching tv/movies/...
          msg.Result=new IntPtr(0);
          return;
        }
      }
      if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("msg keydown");
      g_Player.WndProc(ref msg);
      base.WndProc( ref msg );
    }

    /// <summary>
    /// Process() gets called when a dialog is presented.
    /// It contains the message loop 
    /// </summary>
    public void Process()
    {
      FullRender();
      System.Windows.Forms.Application.DoEvents();
    }

    public void RenderFrame()
    {
      try
      {
        GUIWindowManager.Render();
      }
      catch(Exception ex)
      {
        Log.Write("RenderFrame exception {0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
      }
    }

    /// <summary>
    /// OnStartup() gets called just before the application starts
    /// </summary>
    protected override void OnStartup() 
    {
      // set window form styles
      // these styles enable double buffering, which results in no flickering
      SetStyle(ControlStyles.Opaque, true);
      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.DoubleBuffer, false);

      // set process priority
      m_MouseTimeOut=DateTime.Now;
      //System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;

      Recorder.Start();
    } 

    /// <summary>
    /// OnExit() Gets called just b4 application stops
    /// </summary>
    protected override void OnExit() 
    {
      GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STOPPING;
      // stop any file playback
      g_Player.Stop();
      
      // tell window manager that application is closing
      // this gives the windows the change to do some cleanup
      Recorder.Stop();
    }


    protected override void Render() 
    { 
      if (!g_Player.Playing && !Recorder.IsRecording)
      {
        GUIPropertyManager.RemovePlayerProperties();
      }

      // disable TV preview when playing a movie
      if ( g_Player.Playing && g_Player.HasVideo )
      {
        if (!g_Player.IsTV)
        {
          Recorder.Previewing=false;
        }
      }

      
      // if there's no DX9 device (during resizing for exmaple) then just return
      if (GUIGraphicsContext.DX9Device==null) return;

      // Do we need a screen refresh
      // screen refresh is needed when for example alt+tabbing between programs
      // when that happens the screen needs to get refreshed
      if (!m_bNeedUpdate)
      {
        // no refresh needed
        // are we playing a video fullscreen or watching TV?
        if(GUIGraphicsContext.IsFullScreenVideo)
        {
          // yes, then just handle the outstanding messages
          GUIGraphicsContext.IsPlaying=true;
          
          // and return
          return;
        }
      }

      // we're not playing a video fullscreen
      // or screen needs a refresh
      m_bNeedUpdate=false;


      // update playing status
      if (g_Player.Playing)
      {
        GUIGraphicsContext.IsPlaying=true;
      }
      else
      {
        if (!Recorder.Previewing)
          GUIGraphicsContext.IsFullScreenVideo=false;
        GUIGraphicsContext.IsPlaying=false;
      }
 
      // clear the surface
      GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
      GUIGraphicsContext.DX9Device.BeginScene();

      // ask the window manager to render the current active window
      GUIWindowManager.Render(); 
 
      GUIGraphicsContext.DX9Device.EndScene();
      try
      {
        // Show the frame on the primary surface.
        GUIGraphicsContext.DX9Device.Present();
      }
      catch(DeviceLostException)
      {
        g_Player.Stop();
        deviceLost = true;
      }
    }

  protected override void FrameMove()
  {
    
    int iWindow=GUIWindowManager.ActiveWindow;
    string strModule=GUILocalizeStrings.Get(10000+iWindow);
    GUIPropertyManager.Properties["#currentmodule"]=strModule;

    Recorder.Process();
    g_Player.Process();
    GUIWindowManager.DispatchThreadMessages(); 


		if (g_Player.Paused) GUIPropertyManager.Properties["#playlogo"]="logo_pause.png";
		else if (g_Player.Speed>1) GUIPropertyManager.Properties["#playlogo"]="logo_fastforward.png";
		else if (g_Player.Speed<1) GUIPropertyManager.Properties["#playlogo"]="logo_rewind.png";
		else if (g_Player.Playing) GUIPropertyManager.Properties["#playlogo"]="logo_play.png";
		else GUIPropertyManager.Properties["#playlogo"]="";

    // update playing status
    if (g_Player.Playing)
    {
      GUIGraphicsContext.IsPlaying=true;
			GUIGraphicsContext.IsPlayingVideo=(g_Player.IsVideo || g_Player.IsTV) ;

			GUIPropertyManager.Properties["#currentplaytime"]=Utils.SecondsToHMSString((int)g_Player.CurrentPosition );
			GUIPropertyManager.Properties["#shortcurrentplaytime"]=Utils.SecondsToShortHMSString((int)g_Player.CurrentPosition );
			GUIPropertyManager.Properties["#duration"]=Utils.SecondsToHMSString((int)g_Player.Duration );
			GUIPropertyManager.Properties["#shortduration"]=Utils.SecondsToShortHMSString((int)g_Player.Duration );
			GUIPropertyManager.Properties["#playspeed"]=g_Player.Speed.ToString();

			double fPercentage=g_Player.CurrentPosition / g_Player.Duration;
			int iPercent=(int)(100*fPercentage);
			GUIPropertyManager.Properties["#percentage"]=iPercent.ToString();
    }
    else
    {
      if (!Recorder.Previewing)
        GUIGraphicsContext.IsFullScreenVideo=false;
      GUIGraphicsContext.IsPlaying=false;
				
    }
  }


  /// <summary>
  /// The device has been created.  Resources that are not lost on
  /// Reset() can be created here -- resources in Pool.Default,
  /// Pool.Scratch, or Pool.SystemMemory.  Image surfaces created via
  /// CreateImageSurface are never lost and can be created here.  Vertex
  /// shaders and pixel shaders can also be created here as they are not
  /// lost on Reset().
  /// </summary>
    protected override void InitializeDeviceObjects()
		{
      GUIWindowManager.Clear();
      GUITextureManager.Dispose();
      GUIFontManager.Dispose();

      GUIGraphicsContext.Skin=@"skin\"+m_strSkin;
      GUIGraphicsContext.ActiveForm = this.Handle;
      
      GUIFontManager.LoadFonts(@"skin\"+m_strSkin+@"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      
      GUILocalizeStrings.Load(@"language\"+m_strLanguage+ @"\strings.xml");
      
      Log.Write("Load skin {0}", m_strSkin);
      GUIWindowManager.Initialize();
      GUIWindowManager.Load();
      Log.Write("initialize skin");
      GUIWindowManager.PreInit();
      GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.RUNNING;
			GUIGraphicsContext.Load();
			GUIWindowManager.ActivateWindow( GUIWindowManager.ActiveWindow );
			Log.Write("skin initalized");
      if (GUIGraphicsContext.DX9Device!=null)
      {
        Log.Write("DX9 size: {0}x{1}", GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth,GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight);
        Log.Write("video ram left:{0} KByte", GUIGraphicsContext.DX9Device.AvailableTextureMemory/1024);
      }
    }

    /// <summary>
    /// The device exists, but may have just been Reset().  Resources in
    /// Pool.Default and any other device state that persists during
    /// rendering should be set here.  Render states, matrices, textures,
    /// etc., that don't change during rendering can be set once here to
    /// avoid redundant state setting during Render() or FrameMove().
    /// </summary>
    protected override void OnDeviceReset(System.Object sender, System.EventArgs e) 
		{
      Log.Write("OnDeviceReset()");
      //g_Player.Stop();
      GUIGraphicsContext.Load();
			GUIFontManager.Dispose();
      GUIFontManager.LoadFonts(@"skin\"+m_strSkin+@"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      if (GUIGraphicsContext.DX9Device!=null)
      {
        GUIWindowManager.Restore();
        GUIWindowManager.PreInit();
        GUIWindowManager.ActivateWindow( GUIWindowManager.ActiveWindow );
        GUIWindowManager.OnDeviceRestored();
        GUIGraphicsContext.Load();
      }
      Log.Write(" done");
      g_Player.PositionX++;
      g_Player.PositionX--;
      m_bNeedUpdate=true;
		}

    void OnAction(Action action)
    {
      switch (action.wID)
      {
				case Action.ActionType.ACTION_BACKGROUND_TOGGLE:
					//show livetv or video as background instead of the static GUI background
					using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
					{
						// only works when VMR9 is enabled, so check that
						bool bUseVMR9=xmlreader.GetValueAsBool("general","vmr9",true);
						if (bUseVMR9)
						{
							// toggle livetv/video in background on/pff
              if (GUIGraphicsContext.ShowBackground)
              {
                Log.Write("Use live TV as background");
                GUIGraphicsContext.ShowBackground =false;
                // if on, but we're not playing any video or watching tv
                if (!GUIGraphicsContext.IsPlaying)
                {
                  //then start watching live tv in background
                  Log.Write("start livetv");
                  Recorder.Previewing=true;
									
									string strRecPath;
									strRecPath=xmlreader.GetValueAsString("capture","recordingpath","");
									strRecPath=Utils.RemoveTrailingSlash(strRecPath);
									string strFileName=String.Format(@"{0}\live.tv",strRecPath);
									g_Player.Play(strFileName);
                }
              }
              else
              {
                Log.Write("Use GUI as background");
                if (g_Player.Playing && g_Player.IsTV)
                {
                  Log.Write("stop livetv");
                  int iWindow=GUIWindowManager.ActiveWindow;
                  if (iWindow != (int)GUIWindow.Window.WINDOW_TVGUIDE &&
                      iWindow != (int)GUIWindow.Window.WINDOW_TV &&
                      iWindow != (int)GUIWindow.Window.WINDOW_SCHEDULER &&
                      iWindow != (int)GUIWindow.Window.WINDOW_TVFULLSCREEN &&
                      iWindow != (int)GUIWindow.Window.WINDOW_RECORDEDTV)
                  {
                    
                    GUIGraphicsContext.ShowBackground =true;
                    if (g_Player.Playing && g_Player.IsTV) g_Player.Stop();
                    Recorder.Previewing=false;
                  }
                }
                GUIGraphicsContext.ShowBackground =true;
              }
						}
					}
				break;

        case Action.ActionType.ACTION_EXIT:
          GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STOPPING;
          break;

        case Action.ActionType.ACTION_REBOOT:
        {
          //reboot
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null!=dlgYesNo)
          {
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(630));
            dlgYesNo.SetLine(0, "");
            dlgYesNo.SetLine(1, "");
            dlgYesNo.SetLine(2, "");
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (dlgYesNo.IsConfirmed)
            {
              WindowsController.ExitWindows(RestartOptions.Reboot,true);
              GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STOPPING;
            }
          }
        }
          break;

        case Action.ActionType.ACTION_EJECTCD:
          Utils.EjectCDROM();
        break;

        case Action.ActionType.ACTION_SHUTDOWN:
        {
          GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
          if (null!=dlgYesNo)
          {
            dlgYesNo.SetHeading(GUILocalizeStrings.Get(631));
            dlgYesNo.SetLine(0, "");
            dlgYesNo.SetLine(1, "");
            dlgYesNo.SetLine(2, "");
            dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

            if (dlgYesNo.IsConfirmed)
            {
              WindowsController.ExitWindows(RestartOptions.PowerOff,true);
              GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STOPPING;
            }
          }
          break;
        }
      }
      if (g_Player.Playing)
      {
        switch (action.wID)
        {
          case Action.ActionType.ACTION_SHOW_GUI:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              GUIWindow win= GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
              if (win.FullScreenVideoAllowed)
              {
                if (g_Player.IsTV)
                  GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
                else
                  GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                GUIGraphicsContext.IsFullScreenVideo=true;
              }
              return;
            }
          break;
          
          case Action.ActionType.ACTION_PREV_ITEM:
            if (Utils.IsCDDA(g_Player.CurrentFile)||Utils.IsAudio(g_Player.CurrentFile) )
            {
              PlayListPlayer.PlayPrevious();
              return;
            }
            break;

          case Action.ActionType.ACTION_NEXT_ITEM:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              PlayListPlayer.PlayNext(true);
              return;
            }
          break;

          case Action.ActionType.ACTION_STOP:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Stop();
              return;
            }
            break;

					case Action.ActionType.ACTION_MUSIC_PLAY:
						if (!GUIGraphicsContext.IsFullScreenVideo)
						{
							if (g_Player.Paused) g_Player.Pause();
              return;
						}
					break;
          
          case Action.ActionType.ACTION_PAUSE:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Pause();
              return;
            }
          break;

          case Action.ActionType.ACTION_PLAY:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              if (g_Player.Speed!=1)
              {
                g_Player.Speed=1;
              }
							if (g_Player.Paused) g_Player.Pause();
              return;
            }
          break;

          
          case Action.ActionType.ACTION_MUSIC_FORWARD:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Speed=Utils.GetNextForwardSpeed(g_Player.Speed);
              return;
            }
            break;
          case Action.ActionType.ACTION_MUSIC_REWIND:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Speed=Utils.GetNextRewindSpeed(g_Player.Speed);
              return;
            }
            break;
        }
      }
      GUIWindowManager.OnAction(action);
    }
		protected override void keypressed(System.Windows.Forms.KeyPressEventArgs e)
		{
			char keyc=e.KeyChar;
			Key key = new Key(e.KeyChar,0);
#if  DEBUG
      if (key.KeyChar=='t')
      {
        //Utils.StartProcess(@"C:\media\graphedt.exe","",true,false);
				g_Player.SeekAsolutePercentage(99);
      }
#endif
			if (key.KeyChar=='?')
			{
				GC.Collect();
				GC.Collect();
				GC.Collect();
				GC.Collect();
			}
			Action action=new Action();
			if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow,key,ref action))
      {
        if (action.SoundFileName.Length>0)
          Utils.PlaySound(action.SoundFileName, false, true);
				OnAction(action);
			}
      action = new Action(key,Action.ActionType.ACTION_KEY_PRESSED,0,0);
      OnAction(action);
		}

		protected override void keydown(System.Windows.Forms.KeyEventArgs e)
		{
				Key key = new Key(0,(int)e.KeyCode);
				Action action=new Action();
				if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow,key,ref action))
				{
          if (action.SoundFileName.Length>0)
            Utils.PlaySound(action.SoundFileName, false, true);
					OnAction(action);
				}
      
	  }



	protected override void mousemove(System.Windows.Forms.MouseEventArgs e)
  {
    base.mousemove(e);
    if (!m_bShowCursor) return;
    if (m_iLastMousePositionX !=e.X || m_iLastMousePositionY!=e.Y)
    {
      //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,m_iLastMousePositionX,m_iLastMousePositionY);
      m_iLastMousePositionX=e.X;
      m_iLastMousePositionY=e.Y;

      float fX= ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
      float fY= ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
      float x =(fX*((float)e.X)) - GUIGraphicsContext.OffsetX;
      float y =(fY*((float)e.Y)) - GUIGraphicsContext.OffsetY;;
      GUIWindow window=GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (window!=null)
      {
        Action action=new Action(Action.ActionType.ACTION_MOUSE_MOVE,x,y);
        action.MouseButton=e.Button;
        OnAction(action);
        
      }
    }
	}


	protected override void mouseclick(MouseEventArgs e)
	{
    base.mouseclick(e);
    if (!m_bShowCursor) return;
    Action action;

    // first move mouse
    float fX= ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
    float fY= ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
    float x =(fX*((float)m_iLastMousePositionX)) - GUIGraphicsContext.OffsetX;
    float y =(fY*((float)m_iLastMousePositionY)) - GUIGraphicsContext.OffsetY;;
    action=new Action(Action.ActionType.ACTION_MOUSE_MOVE,x,y);
    OnAction(action);

    // right mouse button=back
    if (e.Button==MouseButtons.Right)
    {
      Key key = new Key(0,(int)Keys.Escape);
      action=new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow,key,ref action))
      {
        if (action.SoundFileName.Length>0)
          Utils.PlaySound(action.SoundFileName, false, true);
        OnAction(action);
      }
    }
/*
    //left mouse button = A
    if (e.Button==MouseButtons.Left)
    {
      Key key = new Key(0,(int)Keys.Enter);
      action=new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow,key,ref action))
      {
        if (action.SoundFileName.Length>0)
            Utils.PlaySound(action.SoundFileName, false, true);
        OnAction(action);
      }
    }*/

    //middle mouse button=Y
    if (e.Button==MouseButtons.Middle)
    {
      Key key = new Key('y',0);
      action=new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow,key,ref action))
      {
        if (action.SoundFileName.Length>0)
          Utils.PlaySound(action.SoundFileName, false, true);
        OnAction(action);
      }
    }
    action=new Action(Action.ActionType.ACTION_MOUSE_CLICK,x,y);
    action.MouseButton=e.Button;
    action.SoundFileName="click.wav";
    if (action.SoundFileName.Length>0)
      Utils.PlaySound(action.SoundFileName, false, true);
    OnAction(action);

	}
		
}
