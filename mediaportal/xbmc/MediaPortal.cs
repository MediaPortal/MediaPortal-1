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

public class MediaPortalApp : D3DApp
{

    int       m_iLastMousePositionX=0;
    int       m_iLastMousePositionY=0;
    private System.Threading.Mutex m_Mutex;
    private string m_UniqueIdentifier;

  const int WM_KEYDOWN    =0x0100;

    [STAThread]
    public static void Main()
    {
      try
      {
        bool bDirectXInstalled=false;
        bool bWindowsMediaPlayer9=false;
        string strVersion="";
        RegistryKey hklm =Registry.LocalMachine;
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
            try
            {
              app.Run();
            }
            catch (Exception ex)
            {
              Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message,ex.Source, ex.StackTrace);
            }
            app.OnExit();
            Log.Write("MediaPortal done");
            Win32API.EnableStartBar(true);
            Win32API.ShowStartBar(true);
          }
        }
      }
      catch(Exception ex)
      {
        Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message,ex.Source, ex.StackTrace);
        return;
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
          m_strSkin=xmlreader.GetValueAsString("skin","name","MediaCenter");
          m_strLanguage=xmlreader.GetValueAsString("skin","language","English");
          m_bAutoHideMouse=xmlreader.GetValueAsBool("general","autohidemouse",false);
          GUIGraphicsContext.MouseSupport=xmlreader.GetValueAsBool("general","mousesupport",true);
        }
      }
      catch(Exception)
      {
        m_strSkin="MediaCenter";
        m_strLanguage="english";
      }
     

      GUIWindowManager.Callbacks+=new GUIWindowManager.OnCallBackHandler(this.Process);
      GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STARTING;

      // load keymapping from keymap.xml
      ActionTranslator.Load();

      //register the playlistplayer for thread messages (like playback stopped,ended)
      PlayListPlayer.Init();

      //registers the player for video window size notifications
      g_Player.Init();
    }

  public override bool PreProcessMessage(ref Message msg)
  {
    if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("pre keydown");

    return base.PreProcessMessage (ref msg);
  }

    protected override void WndProc( ref Message msg )
    {
      
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
      // disable TV preview when playing a movie
      if ( g_Player.Playing && g_Player.HasVideo )
      {
        Recorder.Previewing=false;
      }

      // sleep 10msec so we dont use 100% cpu time
      System.Threading.Thread.Sleep(10);

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
          GUIWindowManager.DispatchThreadMessages();
          g_Player.Process();
          GUIGraphicsContext.IsPlaying=true;
          
          // and return
          return;
        }
      }

      // we're not playing a video fullscreen
      // or screen needs a refresh
      m_bNeedUpdate=false;

      // handle any outstanding messages
      GUIWindowManager.DispatchThreadMessages();
      g_Player.Process();

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
                GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                GUIGraphicsContext.IsFullScreenVideo=true;
              }
              return;
            }
          break;
          
          case Action.ActionType.ACTION_PREV_ITEM:
            if (Utils.IsAudio(g_Player.CurrentFile) )
            {
              PlayListPlayer.PlayPrevious();
            }
            break;

          case Action.ActionType.ACTION_NEXT_ITEM:
            if (Utils.IsAudio(g_Player.CurrentFile) )
            {
              PlayListPlayer.PlayNext(true);
            }
          break;

          case Action.ActionType.ACTION_STOP:
            if (Utils.IsAudio(g_Player.CurrentFile) )
            {
              g_Player.Stop();
            }
            break;
          
          case Action.ActionType.ACTION_PAUSE:
            if (Utils.IsAudio(g_Player.CurrentFile) )
            {
              g_Player.Pause();
            }
          break;

          case Action.ActionType.ACTION_PLAY:
            if (Utils.IsAudio(g_Player.CurrentFile) )
            {
              if (g_Player.Speed!=1)
              {
                g_Player.Speed=1;
              }
            }
          break;

          
          case Action.ActionType.ACTION_MUSIC_FORWARD:
            if (Utils.IsAudio(g_Player.CurrentFile) )
            {
              g_Player.Speed++;
            }
            break;
          case Action.ActionType.ACTION_MUSIC_REWIND:
            if (Utils.IsAudio(g_Player.CurrentFile) )
            {
              g_Player.Speed--;
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
			Action action=new Action();
			if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow,key,ref action))
			{
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
        OnAction(action);
      }
    }
    action=new Action(Action.ActionType.ACTION_MOUSE_CLICK,x,y);
    action.MouseButton=e.Button;
    OnAction(action);

	}
		
}
