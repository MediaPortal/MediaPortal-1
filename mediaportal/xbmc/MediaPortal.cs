using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.Win32;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;

using Microsoft.ApplicationBlocks.ApplicationUpdater;

using MediaPortal.GUI.Library;
using MediaPortal;

using MediaPortal.Player;
using MediaPortal.Util;
using MediaPortal.Playlists;
using MediaPortal.TV.Recording;
using MediaPortal.Dialogs;
using MediaPortal.IR;
using MediaPortal.Ripper;

public class MediaPortalApp : D3DApp, IRender
{
    private ApplicationUpdateManager _updater = null;
    private Thread _updaterThread = null;
    private const int UPDATERTHREAD_JOIN_TIMEOUT = 3 * 1000;
    int m_iLastMousePositionX = 0;
    int m_iLastMousePositionY = 0;
    private System.Threading.Mutex m_Mutex;
    private string m_UniqueIdentifier;
    bool m_bPlayingState = false;
    bool m_bShowStats = false;
    Rectangle[]                     region = new Rectangle[1];
    int m_ixpos = 50;
    int m_iFrameCount = 0;
	  private USBUIRT usbuirtdevice;
    string m_strNewVersion = "";
    string m_strCurrentVersion = "";
    bool m_bNewVersionAvailable = false;
    bool m_bCancelVersion = false;

    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSCOMMAND = 0x0112;
    const int SC_SCREENSAVE = 0xF140;

    static SplashScreen splashScreen;

		[STAThread]
    public static void Main()
    {
      //
      // Display splash screen
      //

      ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
      splashScreen = new SplashScreen();
      splashScreen.SetVersion(clientInfo .InstalledVersion);
      splashScreen.Show();
      splashScreen.Update();

      // CHECK if DirectX 9.0c if installed
      bool bDirectXInstalled = false;
      //bool bManagedDirectXInstalled = false;
      bool bWindowsMediaPlayer9 = false;
      string strVersion = "";
      //string strVersionMng = "";
      RegistryKey hkcu = Registry.CurrentUser;
      hkcu.CreateSubKey(@"Software\MediaPortal");
      RegistryKey hklm = Registry.LocalMachine;
      
      hklm.CreateSubKey(@"Software\MediaPortal");
      RegistryKey subkey = hklm.OpenSubKey(@"Software\Microsoft\DirectX");
      if (subkey != null)
      {
        strVersion = (string)subkey.GetValue("Version");
        if (strVersion != null)
        {
          if (strVersion.Length > 0)
          {
            string strTmp = "";
            for (int i = 0; i < strVersion.Length; ++i)
            {
              if (Char.IsDigit(strVersion[i])) strTmp += strVersion[i];
            }
            long lVersion = System.Convert.ToInt64(strTmp);
            if (lVersion >= 409000904)
            {
              bDirectXInstalled = true;
            }
          }
          else strVersion = "?";
        }
        else strVersion = "?";
/*
        strVersionMng = (string)subkey.GetValue("ManagedDirectXVersion");
        if (strVersionMng != null)
        {
          if (strVersionMng.Length > 0)
          {
            string strTmp = "";
            for (int i = 0; i < strVersionMng.Length; ++i)
            {
              if (Char.IsDigit(strVersionMng[i])) strTmp += strVersionMng[i];
            }
            long lVersion = System.Convert.ToInt64(strTmp);
            if (lVersion >= 409001126)
            {
              bManagedDirectXInstalled = true;
            }
          }
          else strVersionMng = "?";
        }
        else strVersionMng = "?";
        */
        subkey.Close();
        subkey = null;
      }


      // CHECK if Windows MediaPlayer 9 is installed
      subkey = hklm.OpenSubKey(@"Software\Microsoft\MediaPlayer\9.0");
      if (subkey != null)
      {
        bWindowsMediaPlayer9 = true;
        subkey.Close();
        subkey = null;
      }
      hklm.Close();
      hklm = null;

      if (!bDirectXInstalled)
      {
        string strLine = "Please install DirectX 9.0c!\r\n";
        strLine = strLine + "Current version installed:" + strVersion + "\r\n\r\n";
        strLine = strLine + "Mediaportal cannot run without DirectX 9.0c\r\n";
        strLine = strLine + "http://www.microsoft.com/directx";
        System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
      }
      /*
      if (!bManagedDirectXInstalled)
      {
        string strLine="Please install Managed DirectX 9.0c!\r\n";
        strLine=strLine+ "Current version installed:"+strVersion+"\r\n\r\n";
        strLine=strLine+ "Mediaportal cannot run without DirectX 9.0c\r\n";
        strLine=strLine+ "http://www.microsoft.com/directx";
        System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
      }*/

      if (!bWindowsMediaPlayer9)
      {
        string strLine = "Please install Windows Mediaplayer 9\r\n";
        strLine = strLine + "Mediaportal cannot run without Windows Mediaplayer 9";
        System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
      }
      
      if (bDirectXInstalled && bWindowsMediaPlayer9)
      {

        try
        {
          MediaPortalApp app = new MediaPortalApp();
          if (app.CreateGraphicsSample())
          {
            //app.PreRun();
            Log.Write("Start MediaPortal");
            try
            {
              app.Run();
            }
            catch (Exception ex)
            {
              Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message, ex.Source, ex.StackTrace);
            }
            app.OnExit();
            Log.Write("MediaPortal done");
            Win32API.EnableStartBar(true);
            Win32API.ShowStartBar(true);
          }
        }
        catch (Exception ex)
        {
          Log.Write("MediaPortal stopped due 2 an exception {0} {1} {2}",ex.Message, ex.Source, ex.StackTrace);
        }
      }

      if (splashScreen != null)
        splashScreen.Close();
    }
	  private void OnRemoteCommand(object command)
	  {
		  OnAction(new Action((Action.ActionType) command, 0, 0));
	  }

    public MediaPortalApp()
		{
      // check if MediaPortal is already running...
      m_UniqueIdentifier = Application.ExecutablePath.Replace("\\","_");
      m_Mutex = new System.Threading.Mutex(false, m_UniqueIdentifier);
      if (!m_Mutex.WaitOne(1, true))
      {
        string strMsg = "Mediaportal is already running!";
        System.Windows.Forms.MessageBox.Show(strMsg, this.Text, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        throw new Exception(strMsg);
      }

      Utils.FileDelete("capture.log");
      if (Screen.PrimaryScreen.Bounds.Width > 720)
      {
        this.MinimumSize = new Size(720 + 8, 576 + 27);
      }
      else
      {
        this.MinimumSize = new Size(720, 576);
      }
			this.Text = "Media Portal";
      
      Log.Write("-------------------------------------------------------------------------------");
      Log.Write("starting");
      GUIGraphicsContext.form = this;
      GUIGraphicsContext.graphics = null;
      GUIGraphicsContext.ActionHandlers += new OnActionHandler(this.OnAction);
      try
      {
        using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
        {
          m_strSkin = xmlreader.GetValueAsString("skin","name","CrystalCenter");
          m_strLanguage = xmlreader.GetValueAsString("skin","language","English");
          m_bAutoHideMouse = xmlreader.GetValueAsBool("general","autohidemouse",false);
          GUIGraphicsContext.MouseSupport = xmlreader.GetValueAsBool("general","mousesupport",true);
        }
      }
      catch (Exception)
      {
        m_strSkin = "CrystalCenter";
        m_strLanguage = "english";
      }
     

      GUIWindowManager.Receivers += new SendMessageHandler(OnMessage);
      GUIWindowManager.Callbacks += new GUIWindowManager.OnCallBackHandler(this.Process);
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STARTING;

      // load keymapping from keymap.xml
      ActionTranslator.Load();

      // One time setup for proxy servers
      // also set credentials to allow use with firewalls that require them
      // this means we can use the .NET internet objects and not have
      // to worry about proxies elsewhere in the code
      System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
      proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
      System.Net.GlobalProxySelection.Select = proxy;

      //register the playlistplayer for thread messages (like playback stopped,ended)
      Log.Write("Init playlist player");
      PlayListPlayer.Init();

		//
		// Only load the USBUIRT device if it has been enabled in the configuration
		//
		using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
		{
			bool inputEnabled = xmlreader.GetValueAsString("USBUIRT", "internal", "false") == "true";
			bool outputEnabled = xmlreader.GetValueAsString("USBUIRT", "external", "false") == "true";

			if (inputEnabled == true || outputEnabled == true)
			{
				Log.Write("creating the USBUIRT device");
				this.usbuirtdevice = USBUIRT.Create(new USBUIRT.OnRemoteCommand(OnRemoteCommand));
				Log.Write("done creating the USBUIRT device");
			}
		}

      //registers the player for video window size notifications
      Log.Write("Init players");
      g_Player.Init(this);
      Log.Write("done");


      //  hook ProcessExit for a chance to clean up when closed peremptorily
      AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

      //  hook form close to stop updater too
      this.Closed += new EventHandler(MediaPortal_Closed);

      XmlDocument doc = new XmlDocument();
      try
      {
        doc.Load("mediaportal.exe.config");
        XmlNode node = doc.SelectSingleNode("/configuration/appStart/ClientApplicationInfo/appFolderName");
        node.InnerText = System.IO.Directory.GetCurrentDirectory();

        node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/baseDir");
        node.InnerText = System.IO.Directory.GetCurrentDirectory();
        
        node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/tempDir");
        node.InnerText = System.IO.Directory.GetCurrentDirectory();
        doc.Save("Mediaportal.exe.config");
      }
      catch (Exception)
      {
      }
      UpdaterConfiguration config = UpdaterConfiguration.Instance;
      config.Logging.LogPath = System.IO.Directory.GetCurrentDirectory() + @"\log\updatelog.log";
      config.Applications[0].Client.BaseDir = System.IO.Directory.GetCurrentDirectory();
      config.Applications[0].Client.TempDir = System.IO.Directory.GetCurrentDirectory() + @"\temp";
      config.Applications[0].Client.XmlFile = System.IO.Directory.GetCurrentDirectory() + @"\MediaPortal.exe.config";
      config.Applications[0].Server.ServerManifestFileDestination = System.IO.Directory.GetCurrentDirectory() + @"\xml\ServerManifest.xml";
      System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\temp");
      System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\xml");
      System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\log");

      Utils.DeleteFiles(config.Applications[0].Client.BaseDir + @"\log", "*.log");
      

      ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
      clientInfo.AppFolderName = System.IO.Directory.GetCurrentDirectory();
      ClientApplicationInfo.Save("MediaPortal.exe.config",clientInfo.AppFolderName, clientInfo.InstalledVersion);
      m_strCurrentVersion = clientInfo.InstalledVersion;
      Text += (" - [v" + m_strCurrentVersion + "]");

      //  make an Updater for use in-process with us
      _updater = new ApplicationUpdateManager();

      //  hook Updater events
      _updater.DownloadStarted += new UpdaterActionEventHandler(OnUpdaterDownloadStarted);
      _updater.UpdateAvailable += new UpdaterActionEventHandler(OnUpdaterUpdateAvailable);
      _updater.DownloadCompleted += new UpdaterActionEventHandler(OnUpdaterDownloadCompleted);

      //  start the updater on a separate thread so that our UI remains responsive
      _updaterThread = new Thread(new ThreadStart(_updater.StartUpdater));
      _updaterThread.Start();
    }

    void RenderStats()
    {
      UpdateStats();
      if (m_bShowStats)
      {
        GUIFont font = GUIFontManager.GetFont(0);
        if (font != null)
        {
          font.DrawText(80, 80, 0xffffffff, frameStats, GUIControl.Alignment.ALIGN_LEFT);
          
          region[0].X = m_ixpos;
          region[0].Y = 0;
          region[0].Width = 4;
          region[0].Height = GUIGraphicsContext.Height;
          GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.FromArgb(255, 255, 255, 255), 1.0f, 0, region);
         
          float fStep = (GUIGraphicsContext.Width - 100);
          fStep /= (2f * 16f);

          fStep /= framePerSecond;
          m_iFrameCount++;
          if (m_iFrameCount >= (int)fStep)
          {
            m_iFrameCount = 0;
            m_ixpos += 12;
            if (m_ixpos > GUIGraphicsContext.Width - 50) m_ixpos = 50;
          }
        }
      }
    }

    public override bool PreProcessMessage(ref Message msg)
    {
      //if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("pre keydown");

      return base.PreProcessMessage(ref msg);
    }

    protected override void WndProc(ref Message msg)
    {
      
      if (msg.Msg == WM_SYSCOMMAND && msg.WParam.ToInt32() == SC_SCREENSAVE)
      {
        // windows wants to activate the screensaver
        if (GUIGraphicsContext.IsFullScreenVideo || GUIWindowManager.ActiveWindow==(int)GUIWindow.Window.WINDOW_SLIDESHOW) 
        {
          //disable it when we're watching tv/movies/...
          msg.Result = new IntPtr(0);
          return;
        }
      }
      //if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("msg keydown");
      g_Player.WndProc(ref msg);
      base.WndProc(ref msg);
    }

    /// <summary>
    /// Process() gets called when a dialog is presented.
    /// It contains the message loop 
    /// </summary>
    public void Process()
    {
      
      FrameMove();
      FullRender();
      System.Windows.Forms.Application.DoEvents();
    }

    public void RenderFrame()
    {
      try
      {
        GUIWindowManager.Render();
        RenderStats();
      }
      catch (Exception ex)
      {
        Log.Write("RenderFrame exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
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
      m_MouseTimeOut = DateTime.Now;
      //System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;

      Recorder.Start();
      AutoPlay.StartListening();
      //
      // Kill the splash screen
      //
      if (splashScreen != null)
      {
        splashScreen.Close();
        splashScreen.Dispose();
        splashScreen = null;
      }
    } 

    /// <summary>
    /// OnExit() Gets called just b4 application stops
    /// </summary>
    protected override void OnExit() 
    {
      StopUpdater();
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
      // stop any file playback
      g_Player.Stop();
      
      // tell window manager that application is closing
      // this gives the windows the change to do some cleanup
      Recorder.Stop();

      AutoPlay.StopListening();
    }


    protected override void Render() 
    { 
      

      
      // if there's no DX9 device (during resizing for exmaple) then just return
      if (GUIGraphicsContext.DX9Device == null) return;

      // Do we need a screen refresh
      // screen refresh is needed when for example alt+tabbing between programs
      // when that happens the screen needs to get refreshed
      if (!m_bNeedUpdate)
      {
        // no refresh needed
        // are we playing a video fullscreen or watching TV?
        if (GUIGraphicsContext.IsFullScreenVideo)
        {
          // yes, then just handle the outstanding messages
          GUIGraphicsContext.IsPlaying = true;
          
          // and return
          return;
        }
      }

      // we're not playing a video fullscreen
      // or screen needs a refresh
      m_bNeedUpdate = false;

 
      // clear the surface
      GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
      GUIGraphicsContext.DX9Device.BeginScene();

      // ask the window manager to render the current active window
      GUIWindowManager.Render();
      RenderStats();
     
 
      GUIGraphicsContext.DX9Device.EndScene();
      try
      {
        // Show the frame on the primary surface.
        GUIGraphicsContext.DX9Device.Present();
      }
      catch (DeviceLostException)
      {
        g_Player.Stop();
        deviceLost = true;
      }
    }

  protected override void FrameMove()
  {
    CheckForNewUpdate();
    System.Windows.Forms.Application.DoEvents();
    Recorder.Process();
    System.Windows.Forms.Application.DoEvents();
    g_Player.Process();
    System.Windows.Forms.Application.DoEvents();
    GUIWindowManager.DispatchThreadMessages();
    System.Windows.Forms.Application.DoEvents();
    GUIWindowManager.ProcessWindows();
    System.Windows.Forms.Application.DoEvents();

    // update playing status
    if (g_Player.Playing)
    {
      GUIGraphicsContext.IsPlaying = true;
			GUIGraphicsContext.IsPlayingVideo = (g_Player.IsVideo || g_Player.IsTV);

      if (g_Player.Paused) GUIPropertyManager.Properties["#playlogo"] = "logo_pause.png";
      else if (g_Player.Speed > 1) GUIPropertyManager.Properties["#playlogo"] = "logo_fastforward.png";
      else if (g_Player.Speed < 1) GUIPropertyManager.Properties["#playlogo"] = "logo_rewind.png";
      else if (g_Player.Playing) GUIPropertyManager.Properties["#playlogo"] = "logo_play.png";

			GUIPropertyManager.Properties["#currentplaytime"] = Utils.SecondsToHMSString((int)g_Player.CurrentPosition);
			GUIPropertyManager.Properties["#shortcurrentplaytime"] = Utils.SecondsToShortHMSString((int)g_Player.CurrentPosition);
			GUIPropertyManager.Properties["#duration"] = Utils.SecondsToHMSString((int)g_Player.Duration);
			GUIPropertyManager.Properties["#shortduration"] = Utils.SecondsToShortHMSString((int)g_Player.Duration);
			GUIPropertyManager.Properties["#playspeed"] = g_Player.Speed.ToString();

			double fPercentage = g_Player.CurrentPosition / g_Player.Duration;
			int iPercent = (int)(100 * fPercentage);
			GUIPropertyManager.Properties["#percentage"] = iPercent.ToString();
    }
    else
    {
      if (!Recorder.View)
        GUIGraphicsContext.IsFullScreenVideo = false;
      GUIGraphicsContext.IsPlaying = false;
    }
    if (!g_Player.Playing && !Recorder.IsRecording)
    {
      if (m_bPlayingState)
      {
        GUIPropertyManager.RemovePlayerProperties();
      }
      m_bPlayingState = false;
    }
    else 
    {
      m_bPlayingState = true;
    }

    // disable TV preview when playing a movie
    if (g_Player.Playing && g_Player.HasVideo)
    {
      if (!g_Player.IsTV)
      {
        Recorder.View = false;
      }
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

      GUIGraphicsContext.Skin = @"skin\" + m_strSkin;
      GUIGraphicsContext.ActiveForm = this.Handle;
      GUILocalizeStrings.Load(@"language\" + m_strLanguage + @"\strings.xml");
      
      GUIFontManager.LoadFonts(@"skin\" + m_strSkin + @"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      
      
      Log.Write("Load skin {0}", m_strSkin);
      GUIWindowManager.Initialize();
      GUIWindowManager.Load();
      Log.Write("initialize skin");
      GUIWindowManager.PreInit();
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
			GUIGraphicsContext.Load();
			GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
			Log.Write("skin initalized");
      if (GUIGraphicsContext.DX9Device != null)
      {
        Log.Write("DX9 size: {0}x{1}", GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth, GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight);
        Log.Write("video ram left:{0} KByte", GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1024);
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
		//
		// Only perform the device reset if we're not shutting down MediaPortal.
		//
		if (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
		{
			Log.Write("OnDeviceReset()");
			//g_Player.Stop();
			GUIGraphicsContext.Load();
			GUIFontManager.Dispose();
			GUIFontManager.LoadFonts(@"skin\" + m_strSkin + @"\fonts.xml");
			GUIFontManager.InitializeDeviceObjects();
			if (GUIGraphicsContext.DX9Device != null)
			{
				GUIWindowManager.Restore();
				GUIWindowManager.PreInit();
				GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
				GUIWindowManager.OnDeviceRestored();
				GUIGraphicsContext.Load();
			}
			Log.Write(" done");
			g_Player.PositionX++;
			g_Player.PositionX--;
			m_bNeedUpdate = true;
		}
	}

    void OnAction(Action action)
    {
      int iCurrent, iMin, iMax, iStep;
			try
			{
				switch (action.wID)
				{
          case Action.ActionType.ACTION_TOGGLE_WINDOWED_FULSLCREEN:
            ToggleFullWindowed(); 
            return;
          break;

					case Action.ActionType.ACTION_VOLUME_DOWN : 
            iCurrent = AudioMixerHelper.GetMinMaxVolume(out iMin, out iMax);
            iStep = (iMax - iMin) / 10;
            iCurrent -= iStep;
            if (iCurrent < iMin) iCurrent = iMin;
            AudioMixerHelper.SetVolume(iCurrent);
          break;

					case Action.ActionType.ACTION_VOLUME_UP : 
            iCurrent = AudioMixerHelper.GetMinMaxVolume(out iMin, out iMax);
            iStep = (iMax - iMin) / 10;
            iCurrent += iStep;
            if (iCurrent > iMax) iCurrent = iMax;
            AudioMixerHelper.SetVolume(iCurrent);
          break;
            
					case Action.ActionType.ACTION_BACKGROUND_TOGGLE : 
						//show livetv or video as background instead of the static GUI background
						using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
						{
							// only works when VMR9 is enabled, so check that
							bool bUseVMR9 = xmlreader.GetValueAsBool("general","vmr9",true);
							if (bUseVMR9)
							{
								// toggle livetv/video in background on/pff
								if (GUIGraphicsContext.ShowBackground)
								{
									Log.Write("Use live TV as background");
									GUIGraphicsContext.ShowBackground = false;
									// if on, but we're not playing any video or watching tv
									if (!GUIGraphicsContext.IsPlaying)
									{
										//then start watching live tv in background
										Log.Write("start livetv");
										Recorder.Timeshifting = true;
                    Recorder.View = true;
										
										string strRecPath;
										strRecPath = xmlreader.GetValueAsString("capture","recordingpath","");
										strRecPath = Utils.RemoveTrailingSlash(strRecPath);
										string strFileName = String.Format(@"{0}\live.tv",strRecPath);
										g_Player.Play(strFileName);
									}
								}
								else
								{
									Log.Write("Use GUI as background");
									if (g_Player.Playing && g_Player.IsTV)
									{
										Log.Write("stop livetv");
										int iWindow = GUIWindowManager.ActiveWindow;
										if (iWindow != (int)GUIWindow.Window.WINDOW_TVGUIDE && 
											iWindow != (int)GUIWindow.Window.WINDOW_TV && 
											iWindow != (int)GUIWindow.Window.WINDOW_SCHEDULER && 
											iWindow != (int)GUIWindow.Window.WINDOW_TVFULLSCREEN && 
											iWindow != (int)GUIWindow.Window.WINDOW_RECORDEDTV)
										{
	                    
											GUIGraphicsContext.ShowBackground = true;
											if (g_Player.Playing && g_Player.IsTV) g_Player.Stop();
											Recorder.View = false;
										}
									}
									GUIGraphicsContext.ShowBackground = true;
								}
							}
						}
						break;

					case Action.ActionType.ACTION_EXIT : 
						GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
						break;

					case Action.ActionType.ACTION_REBOOT : 
					{
						//reboot
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null != dlgYesNo)
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(630));
							dlgYesNo.SetLine(0, "");
							dlgYesNo.SetLine(1, "");
							dlgYesNo.SetLine(2, "");
							dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

							if (dlgYesNo.IsConfirmed)
							{
								WindowsController.ExitWindows(RestartOptions.Reboot, true);
								GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
							}
						}
					}
						break;

					case Action.ActionType.ACTION_EJECTCD : 
						Utils.EjectCDROM();
						break;

					case Action.ActionType.ACTION_SHUTDOWN : 
					{
						GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
						if (null != dlgYesNo)
						{
							dlgYesNo.SetHeading(GUILocalizeStrings.Get(631));
							dlgYesNo.SetLine(0, "");
							dlgYesNo.SetLine(1, "");
							dlgYesNo.SetLine(2, "");
							dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

							if (dlgYesNo.IsConfirmed)
							{
								WindowsController.ExitWindows(RestartOptions.PowerOff, true);
								GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
							}
						}
						break;
					}
				}
				if (g_Player.Playing)
				{
					switch (action.wID)
					{
						case Action.ActionType.ACTION_SHOW_GUI : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								GUIWindow win = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
								if (win.FullScreenVideoAllowed)
								{
                  if (!g_Player.IsTV)
                  {
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                    GUIGraphicsContext.IsFullScreenVideo = true;
                    return;
                  }
								}
							}
							break;
	          
						case Action.ActionType.ACTION_PREV_ITEM : 
							if (Utils.IsCDDA(g_Player.CurrentFile) || Utils.IsAudio(g_Player.CurrentFile))
							{
								PlayListPlayer.PlayPrevious();
								return;
							}
							break;

						case Action.ActionType.ACTION_NEXT_ITEM : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								PlayListPlayer.PlayNext(true);
								return;
							}
							break;

						case Action.ActionType.ACTION_STOP : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								g_Player.Stop();
								return;
							}
							break;

						case Action.ActionType.ACTION_MUSIC_PLAY : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								if (g_Player.Paused) g_Player.Pause();
								return;
							}
							break;
	          
						case Action.ActionType.ACTION_PAUSE : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								g_Player.Pause();
								return;
							}
							break;

						case Action.ActionType.ACTION_PLAY : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								if (g_Player.Speed != 1)
								{
									g_Player.Speed = 1;
								}
								if (g_Player.Paused) g_Player.Pause();
								return;
							}
							break;

	          
						case Action.ActionType.ACTION_MUSIC_FORWARD : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
								return;
							}
							break;
						case Action.ActionType.ACTION_MUSIC_REWIND : 
							if (!GUIGraphicsContext.IsFullScreenVideo)
							{
								g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
								return;
							}
							break;
					}
				}
				GUIWindowManager.OnAction(action);
			}
			catch (Exception ex)
			{
        Log.Write("  exception: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        throw new Exception("exception occured",ex);
			}
    }

		protected override void keypressed(System.Windows.Forms.KeyPressEventArgs e)
		{
			char keyc = e.KeyChar;
			Key key = new Key(e.KeyChar, 0);
      if (key.KeyChar == '!') m_bShowStats = !m_bShowStats;
			if (key.KeyChar == '?')
			{
        GC.Collect();GC.Collect();GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();GC.Collect();GC.Collect();
      }
			Action action = new Action();
      if (GUIWindowManager.IsRouted && GUIWindowManager.RoutedWindow == (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD)
      {
        action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
        OnAction(action);
        return;
      }

			if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
      {
        if (action.SoundFileName.Length > 0)
          Utils.PlaySound(action.SoundFileName, false, true);
				OnAction(action);
			}
      action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
      OnAction(action);
		}

		protected override void keydown(System.Windows.Forms.KeyEventArgs e)
		{
				Key key = new Key(0, (int)e.KeyCode);
				Action action = new Action();
				if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
				{
          if (action.SoundFileName.Length > 0)
            Utils.PlaySound(action.SoundFileName, false, true);
					OnAction(action);
				}
      
	  }



	protected override void mousemove(System.Windows.Forms.MouseEventArgs e)
  {
    base.mousemove(e);
    if (!m_bShowCursor) return;
    if (m_iLastMousePositionX != e.X || m_iLastMousePositionY != e.Y)
    {
      //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,m_iLastMousePositionX,m_iLastMousePositionY);
      m_iLastMousePositionX = e.X;
      m_iLastMousePositionY = e.Y;

      float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
      float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
      float x = (fX * ((float)e.X)) - GUIGraphicsContext.OffsetX;
      float y = (fY * ((float)e.Y)) - GUIGraphicsContext.OffsetY; ;
      GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (window != null)
      {
        Action action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
        action.MouseButton = e.Button;
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
    float fX = ((float)GUIGraphicsContext.Width) / ((float)this.ClientSize.Width);
    float fY = ((float)GUIGraphicsContext.Height) / ((float)this.ClientSize.Height);
    float x = (fX * ((float)m_iLastMousePositionX)) - GUIGraphicsContext.OffsetX;
    float y = (fY * ((float)m_iLastMousePositionY)) - GUIGraphicsContext.OffsetY; ;
    action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
    OnAction(action);

    // right mouse button=back
    if (e.Button == MouseButtons.Right)
    {
      Key key = new Key(0, (int)Keys.Escape);
      action = new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
      {
        if (action.SoundFileName.Length > 0)
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
    if (e.Button == MouseButtons.Middle)
    {
      Key key = new Key('y',0);
      action = new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindow, key, ref action))
      {
        if (action.SoundFileName.Length > 0)
          Utils.PlaySound(action.SoundFileName, false, true);
        OnAction(action);
      }
    }
    action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, x, y);
    action.MouseButton = e.Button;
    action.SoundFileName = "click.wav";
    if (action.SoundFileName.Length > 0)
      Utils.PlaySound(action.SoundFileName, false, true);
    OnAction(action);

	}
	
  private void MediaPortal_Closed(object sender, EventArgs e)
  {
    StopUpdater();
  }

		
  private void CurrentDomain_ProcessExit(object sender, EventArgs e)
  {
    StopUpdater();
  }
  private delegate void MarshalEventDelegate(object sender, UpdaterActionEventArgs e);

	//---------------------------------------------------
  private void OnUpdaterDownloadStartedHandler(object sender, UpdaterActionEventArgs e) 
  {		
    Log.Write("update:Download started for:{0}",e.ApplicationName);
  }

  private void OnUpdaterDownloadStarted(object sender, UpdaterActionEventArgs e)
  { 
    this.Invoke(
      new MarshalEventDelegate(this.OnUpdaterDownloadStartedHandler), 
      new object[] { sender, e });
  }

  private void CheckForNewUpdate()
  {
    if (!m_bNewVersionAvailable) return;
    if (GUIWindowManager.IsRouted) return;
    g_Player.Stop();
    GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
    if (null == dlgYesNo) return;
    dlgYesNo.SetHeading(GUILocalizeStrings.Get(709)); //Auto update
    dlgYesNo.SetLine(0, GUILocalizeStrings.Get(710)); //A new version is available
    string strFmt = GUILocalizeStrings.Get(711);
    strFmt = strFmt.Replace("#old", m_strCurrentVersion);
    strFmt = strFmt.Replace("#new", m_strNewVersion);
    dlgYesNo.SetLine(1, strFmt);
    dlgYesNo.SetLine(2, "");
    dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);

    if (!dlgYesNo.IsConfirmed) 
    {
      Log.Write("update:User canceled download");
      m_bCancelVersion = true;
      m_bNewVersionAvailable = false;
      return;
    }
    m_bCancelVersion = false;
    m_bNewVersionAvailable = false;
  }

  private void OnUpdaterUpdateAvailable(object sender, UpdaterActionEventArgs e)
  {
    Log.Write("update:new version available:{0}", e.ApplicationName);
    m_strNewVersion = e.ServerInformation.AvailableVersion;
    m_bNewVersionAvailable = true;
    while (m_bNewVersionAvailable) System.Threading.Thread.Sleep(100);
    if (m_bCancelVersion)
    {
      _updater.StopUpdater(e.ApplicationName);
    }
  }

  //---------------------------------------------------
  private void OnUpdaterDownloadCompletedHandler(object sender, UpdaterActionEventArgs e)
  {
    Log.Write("update:Download Completed.");
    StartNewVersion();
  }

  private void OnUpdaterDownloadCompleted(object sender, UpdaterActionEventArgs e)
  {
    //  using the synchronous "Invoke".  This marshals from the eventing thread--which comes from the Updater and should not
    //  be allowed to enter and "touch" the UI's window thread
    //  so we use Invoke which allows us to block the Updater thread at will while only allowing window thread to update UI
    this.Invoke(
      new MarshalEventDelegate(this.OnUpdaterDownloadCompletedHandler), 
      new object[] { sender, e });
  }


  //---------------------------------------------------
  private void StartNewVersion()
  {
    Log.Write("update:start appstart.exe");
    XmlDocument doc = new XmlDocument();

    //  load config file to get base dir
    doc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

    //  get the base dir
    string baseDir = System.IO.Directory.GetCurrentDirectory(); //doc.SelectSingleNode("configuration/appUpdater/UpdaterConfiguration/application/client/baseDir").InnerText;
    string newDir = Path.Combine(baseDir, "AppStart.exe");
		
		ClientApplicationInfo clientInfoNow = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
    ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("AppStart.exe.config");
    clientInfo.AppFolderName = System.IO.Directory.GetCurrentDirectory();
    ClientApplicationInfo.Save("AppStart.exe.config",clientInfo.AppFolderName, clientInfoNow.InstalledVersion);
          
    ProcessStartInfo process = new ProcessStartInfo(newDir);
    process.WorkingDirectory = baseDir;
    process.Arguments = clientInfoNow.InstalledVersion;

    //  launch new version (actually, launch AppStart.exe which HAS pointer to new version )
    System.Diagnostics.Process.Start(process);

    //  tell updater to stop
    Log.Write("update:stop mp...");
    CurrentDomain_ProcessExit(null, null);
    //  leave this app
    Environment.Exit(0);
  }

  private void btnStop_Click(object sender, System.EventArgs e)
  {
    StopUpdater();
  }

		
  private void StopUpdater()
  {
    //  tell updater to stop
    _updater.StopUpdater();
    if (null != _updaterThread)
    {
      //  join the updater thread with a suitable timeout
      bool isThreadJoined = _updaterThread.Join(UPDATERTHREAD_JOIN_TIMEOUT);
      //  check if we joined, if we didn't interrupt the thread
      if (!isThreadJoined)
      {
        _updaterThread.Interrupt();
      }
      _updaterThread = null;
    }
  }

  private void OnMessage(GUIMessage message)
  {
    switch (message.Message)
    {
      case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL : 
        try
        {
          usbuirtdevice.ChangeTunerChannel(message.Label);
        }
        catch (Exception) {}
      break;

      case GUIMessage.MessageType.GUI_MSG_GET_STRING : 
        VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
        if (null == keyboard) return;
        keyboard.Reset();
        keyboard.Text = message.Label;
        keyboard.DoModal(GUIWindowManager.ActiveWindow);
        if (keyboard.IsConfirmed)
        {
          message.Label = keyboard.Text;
        }
        else message.Label = "";
      break;

      case GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED : 
        if (isMaximized == false) return;

        if (message.Param1 != 0)
        {
          //switch to fullscreen mode
          if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed) return;
          //SwitchFullScreenOrWindowed(false,true);
        }
        else
        {
          //switch to windowed mode
          if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed) return;
          //SwitchFullScreenOrWindowed(true,true);
        }

      break;
    }
  }
}
