using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;


using MediaPortal.GUI.Library;
using MediaPortal;

using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;

public class MediaPortalApp : D3DApp
{

    int       m_iLastMousePositionX=0;
    int       m_iLastMousePositionY=0;
    [STAThread]
    public static void Main()
    {
      try
      {

        MediaPortalApp app = new MediaPortalApp();
        if( app.CreateGraphicsSample() )
        {
          //app.PreRun();
          Log.Write("Start MediaPortal");
          try
          {
            GC.Collect();
            GC.Collect();
            GC.Collect();
            app.Run();
          }
          catch (Exception ex)
          {
            Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message,ex.Source, ex.StackTrace);
          }
          app.OnExit();
          Log.Write("MediaPortal done");
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
      Utils.FileDelete("capture.log");
      if (Screen.PrimaryScreen.Bounds.Width>720)
      {
        this.MinimumSize = new Size(768+8,576+46);
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
      try
      {
        AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml");
        m_strSkin=xmlreader.GetValueAsString("skin","name","MediaCenter");
        m_strLanguage=xmlreader.GetValueAsString("skin","language","English");
        m_bAutoHideMouse=xmlreader.GetValueAsBool("general","autohidemouse",false);
				xmlreader=null;
      }
      catch(Exception)
      {
        m_strSkin="MediaCenter";
        m_strLanguage="english";
      }
     
      GUIWindowManager.Callbacks+=new GUIWindowManager.OnCallBackHandler(this.Process);
      GUIGraphicsContext.CurrentState=GUIGraphicsContext.State.STARTING;
      ActionTranslator.Load();
			
      PlayListPlayer.Init();
    }

    protected override void WndProc( ref Message m )
    {
      g_Player.WndProc(ref m);
      base.WndProc( ref m );
    }


    public void Process()
    {
      FullRender();
      System.Windows.Forms.Application.DoEvents();
    }

    protected override void OnStartup() 
    {
      SetStyle(ControlStyles.Opaque, true);
      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.DoubleBuffer, false);

      m_MouseTimeOut=DateTime.Now;
      System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;
      GUIWindowManager.OnAppStarting();

    } 
    protected override void OnExit() 
    {
      g_Player.Stop();
      GUIWindowManager.OnAppClosing();
    }
	
    public void RenderVideoOverlay()
    {
      if (!g_Player.Playing) return;
      if (!g_Player.HasVideo) return;
      if (GUIGraphicsContext.IsFullScreenVideo) return;
      if (GUIGraphicsContext.Calibrating) return;
      if (!GUIGraphicsContext.Overlay) return;
      if (Utils.IsAudio(g_Player.CurrentFile)) return;
      GUIWindow windowOverlay= (GUIWindow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIDEO_OVERLAY);
      if (null!=windowOverlay)
      {
        windowOverlay.Render();
      }
    }

    public void RenderMusicOverlay()
    {
      if (!g_Player.Playing) return;
      if (GUIGraphicsContext.IsFullScreenVideo) return;
      if (!GUIGraphicsContext.Overlay) return;
      if (!Utils.IsAudio(g_Player.CurrentFile)) return;
      GUIWindow windowOverlay= (GUIWindow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_MUSIC_OVERLAY);
      if (null!=windowOverlay)
      {
        windowOverlay.Render();
      }
    }

    protected override void Render() 
    { 
      if (GUIGraphicsContext.DX9Device==null) return;

      if (!m_bNeedUpdate)
      {
        if(GUIGraphicsContext.IsFullScreenVideo && g_Player.Playing)
        {
          GUIWindowManager.DispatchThreadMessages();
          SetVideoRect();
          g_Player.Process();
          GUIGraphicsContext.IsPlaying=true;
          return;
        }
      }
      m_bNeedUpdate=false;

      GUIWindowManager.DispatchThreadMessages();
      g_Player.Process();

      if (g_Player.Playing)
      {
        GUIGraphicsContext.IsPlaying=true;
      }
      else
      {
        GUIGraphicsContext.IsFullScreenVideo=false;
        GUIGraphicsContext.IsPlaying=false;
      }
      GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
      GUIGraphicsContext.DX9Device.BeginScene();
      GUIWindowManager.Render(); 
      RenderVideoOverlay();
      RenderMusicOverlay();
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
      SetVideoRect();
       
    }

    public void SetVideoRect()
    {
      if (!g_Player.Playing) return;
      if (!g_Player.HasVideo) return;

      g_Player.FullScreen=GUIGraphicsContext.IsFullScreenVideo;
      if (!g_Player.FullScreen)
      {
        g_Player.PositionX=GUIGraphicsContext.VideoWindow.Left;
        g_Player.PositionY=GUIGraphicsContext.VideoWindow.Top;
        g_Player.RenderWidth=GUIGraphicsContext.VideoWindow.Width;
        g_Player.RenderHeight=GUIGraphicsContext.VideoWindow.Height;
        g_Player.ARType=GUIGraphicsContext.ARType;
      }
      g_Player.SetVideoWindow();
    }


  /// <summary>
  /// The device has been created.  Resources that are not lost on
  /// Reset() can be created here -- resources in Pool.Managed,
  /// Pool.Scratch, or Pool.SystemMemory.  Image surfaces created via
  /// CreateImageSurface are never lost and can be created here.  Vertex
  /// shaders and pixel shaders can also be created here as they are not
  /// lost on Reset().
  /// </summary>
    protected override void InitializeDeviceObjects()
		{
      if (!GUIWindowManager.Initalized)
      {
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
      }
      else
      {
        GUIWindowManager.ActivateWindow( GUIWindowManager.ActiveWindow );
      }
    }

    protected override void OnDeviceLost(System.Object sender, System.EventArgs e)
    {
      Log.Write("OnDeviceLost() ");
      GUIWindowManager.Dispose();
      GUITextureManager.Dispose();
      GUIWindowManager.OnDeviceLost();
    }
    protected override void OnDeviceDisposing(System.Object sender, System.EventArgs e)
    {
      Log.Write("OnDeviceDisposing()");
			GUIWindowManager.Dispose();
      GUITextureManager.Dispose();
      Log.Write("  done");
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
      if (g_Player.Playing)
      {
        switch (action.wID)
        {
          case Action.ActionType.ACTION_SHOW_GUI:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
              GUIGraphicsContext.IsFullScreenVideo=true;
              return;
            }
          break;
          
          case Action.ActionType.ACTION_PREV_ITEM:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              PlayListPlayer.PlayPrevious();
            }
            break;

          case Action.ActionType.ACTION_NEXT_ITEM:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              PlayListPlayer.PlayNext(true);
            }
          break;

          case Action.ActionType.ACTION_STOP:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Stop();
            }
            break;
          
          case Action.ActionType.ACTION_PAUSE:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Pause();
            }
          break;

          case Action.ActionType.ACTION_PLAY:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              if (g_Player.Speed!=1)
              {
                g_Player.Speed=1;
              }
            }
          break;

          
          case Action.ActionType.ACTION_MUSIC_FORWARD:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Speed++;
            }
            break;
          case Action.ActionType.ACTION_MUSIC_REWIND:
            if (!GUIGraphicsContext.IsFullScreenVideo)
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
    if (m_iLastMousePositionX !=e.X || m_iLastMousePositionY!=e.Y)
    {
      //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,m_iLastMousePositionX,m_iLastMousePositionY);
      m_iLastMousePositionX=e.X;
      m_iLastMousePositionY=e.Y;

      m_MouseTimeOut=DateTime.Now;
      Cursor.Show();
    }


    float fX= ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
    float fY= ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
    float x =(fX*((float)e.X)) - GUIGraphicsContext.OffsetX;
    float y =(fY*((float)e.Y)) - GUIGraphicsContext.OffsetY;;
    GUIWindow window=GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
    if (window!=null)
    {
      int iControl=window.GetFocusControl();
      Action action=new Action(Action.ActionType.ACTION_MOUSE_MOVE,x,y);
      OnAction(action);
      
    }
	}

	protected override void mouseclick(MouseEventArgs e)
	{
    base.mouseclick(e);
    Action action;
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

    //left mouse button = A
    if (e.Button==MouseButtons.Left)
    {
      Key key = new Key(0,(int)Keys.Enter);
      action=new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow,key,ref action))
      {
        OnAction(action);
      }
    }

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

    m_MouseTimeOut=DateTime.Now;
    Cursor.Show();
	}
		
}
