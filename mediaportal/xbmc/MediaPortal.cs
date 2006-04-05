#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using MediaPortal;
using MediaPortal.Dialogs;
using MediaPortal.Dispatcher;
using MediaPortal.GUI.Library;
using MediaPortal.IR;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.RedEyeIR;
using MediaPortal.Ripper;
using MediaPortal.SerialIR;
using MediaPortal.TV.Recording;
using MediaPortal.Util;
using MediaPortal.WINLIRC;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using Geometry = MediaPortal.GUI.Library.Geometry;
using Timer = System.Timers.Timer;
using Utils = MediaPortal.Util.Utils;

#endregion

public class MediaPortalApp : D3DApp, IRender
{
  #region vars

#if AUTOUPDATE
  private ApplicationUpdateManager _updater = null;
    private Thread _updaterThread = null;
    private const int UPDATERTHREAD_JOIN_TIMEOUT = 3 * 1000;
#endif
  private int m_iLastMousePositionX = 0;
  private int m_iLastMousePositionY = 0;
  private bool m_bPlayingState = false;
  private bool m_bShowStats = false;
  private Rectangle[] region = new Rectangle[1];
  private int m_ixpos = 50;
  private int m_iFrameCount = 0;
  private SerialUIR serialuirdevice = null;
  private USBUIRT usbuirtdevice;
  private WinLirc winlircdevice; //sd00//
  private RedEye redeyedevice; //PB00//
  private DateTime screenSaverTimer = DateTime.Now;
  private bool useScreenSaver = true;
  private bool restoreTopMost = false;
  private bool startWithBasicHome = false;
  private bool _suspended = false;
#if AUTOUPDATE
  string m_strNewVersion = "";
    bool m_bNewVersionAvailable = false;
    bool m_bCancelVersion = false;
#endif
  //string m_strCurrentVersion = "";
  private MouseEventArgs eLastMouseClickEvent = null;
  private Timer tMouseClickTimer = null;
  private bool bMouseClickFired = false;

  //private const int WM_KEYDOWN = 0x0100;
  private const int WM_SYSCOMMAND = 0x0112;
  //private const int WM_CLOSE = 0x0010;
  private const int WM_POWERBROADCAST = 0x0218;

  private const int PBT_APMQUERYSUSPEND = 0x0000;
  private const int PBT_APMQUERYSTANDBY = 0x0001;
  //private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
  //private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
  private const int PBT_APMSUSPEND = 0x0004;
  //private const int PBT_APMSTANDBY = 0x0005;
  private const int PBT_APMRESUMECRITICAL = 0x0006;
  private const int PBT_APMRESUMESUSPEND = 0x0007;
  private const int PBT_APMRESUMESTANDBY = 0x0008;
  //private const int PBTF_APMRESUMEFROMFAILURE = 0x00000001;
  //private const int PBT_APMBATTERYLOW = 0x0009;
  //private const int PBT_APMPOWERSTATUSCHANGE = 0x000A;
  //private const int PBT_APMOEMEVENT = 0x000B;
  private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

  private const int SC_SCREENSAVE = 0xF140;

  private const string mpMutex = "{E0151CBA-7F81-41df-9849-F5298A779EB3}";

  private bool supportsFiltering = false;
  private bool bSupportsAlphaBlend = false;
  private int g_nAnisotropy;
  private DateTime m_updateTimer = DateTime.MinValue;
  private int m_iDateLayout;
  private static SplashScreen splashScreen;
  //private GUILayerRenderer _layerRenderer;

  #endregion

  private static RestartOptions restartOptions = RestartOptions.Reboot;
  private static bool useRestartOptions = false;

  #region main()

  //NProf doesnt work if the [STAThread] attribute is set
  //but is needed when you want to play music or video
  [STAThread]
  public static void Main()
  {
    AddExceptionHandler();

    Log.BackupLogFiles();
    Log.Write("MediaPortal is starting up");

    FileInfo mpFi = new FileInfo(Assembly.GetExecutingAssembly().Location);
    Log.Write("Assembly creation time: {0} (UTC)", mpFi.LastWriteTimeUtc.ToUniversalTime());

    using (ProcessLock processLock = new ProcessLock(mpMutex))
    {
      if (processLock.AlreadyExists)
      {
        Log.Write("Mediaportal is already running...");
        ActivatePreviousInstance();
      }

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      //Set current directory
      string applicationPath = Application.ExecutablePath;
      applicationPath = Path.GetFullPath(applicationPath);
      applicationPath = Path.GetDirectoryName(applicationPath);
      Directory.SetCurrentDirectory(applicationPath);
      Log.Write("  Set current directory to :{0}", applicationPath);
      //check if mediaportal has been configured
      if (!File.Exists("mediaportal.xml"))
      {
        //no, then start configuration.exe in wizard form
        System.Diagnostics.Process.Start("configuration.exe", @"/wizard");
        return;
      }
      //CodecsForm form = new CodecsForm();
      //if (!form.AreCodecsInstalled())
      //{
      //	form.ShowDialog();
      //}
      //form=null;

#if !DEBUG
      string version = System.Configuration.ConfigurationManager.AppSettings["version"];
      //ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
      splashScreen = new SplashScreen();
      splashScreen.Version = version;
      splashScreen.Run();
      //clientInfo=null;
#endif


      Log.Write("  verify that directx 9 is installed");
      try
      {
        // CHECK if DirectX 9.0c if installed
        RegistryKey hklm = Registry.LocalMachine;
        RegistryKey subkey = hklm.OpenSubKey(@"Software\Microsoft\DirectX");
        if (subkey != null)
        {
          string strVersion = (string)subkey.GetValue("Version");
          if (strVersion != null)
          {
            if (strVersion.Length > 0)
            {
              string strTmp = "";
              for (int i = 0; i < strVersion.Length; ++i)
              {
                if (Char.IsDigit(strVersion[i]))
                {
                  strTmp += strVersion[i];
                }
              }
              long lVersion = Convert.ToInt64(strTmp);
              if (lVersion < 409000904)
              {
                string strLine = "Please install DirectX 9.0c!\r\n";
                strLine = strLine + "Current version installed:" + strVersion + "\r\n\r\n";
                strLine = strLine + "Mediaportal cannot run without DirectX 9.0c\r\n";
                strLine = strLine + "http://www.microsoft.com/directx";
                MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
              }
            }
          }

          string strVersionMng = (string)subkey.GetValue("ManagedDirectXVersion");
          if (strVersionMng != null)
          {
            if (strVersionMng.Length > 0)
            {
              string strTmp = "";
              for (int i = 0; i < strVersionMng.Length; ++i)
              {
                if (Char.IsDigit(strVersionMng[i]))
                {
                  strTmp += strVersionMng[i];
                }
              }
              //              long lVersion = Convert.ToInt64(strTmp);
              //							if (lVersion < 409001126)
              //							{
              //                string strLine="Please install Managed DirectX 9.0c!\r\n";
              //                strLine=strLine+ "Current version installed:"+strVersionMng+"\r\n\r\n";
              //                strLine=strLine+ "Mediaportal cannot run without DirectX 9.0c\r\n";
              //                strLine=strLine+ "http://www.microsoft.com/directx";
              //                System.Windows.Forms.MessageBox.Show(strLine, "MediaPortal", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
              //                return;
              //							}
            }
          }
          subkey.Close();
        }

        // CHECK if Windows MediaPlayer 9 is installed
        Log.Write("  verify that windows mediaplayer 9 or 10 is installed");
        subkey =
          hklm.OpenSubKey(@"Software\Microsoft\Active Setup\Installed Components\{22d6f312-b0f6-11d0-94ab-0080c74c7e95}");
        if (subkey != null)
        {
          if (((int)subkey.GetValue("IsInstalled")) == 1)
          {
            string wmpversion = (string)subkey.GetValue("Version");
            Log.Write("Windows media player version:{0} installed", wmpversion);
          }
          subkey.Close();
        }
        else
        {
          string strLine = "Please install Windows Mediaplayer 9/10\r\n";
          strLine = strLine + "Mediaportal cannot run without Windows Mediaplayer 9/10";
          MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        hklm.Close();
      }
      catch (Exception)
      {
      }

      //following crashes on some pc's, dunno why
      //Log.Write("  Stop any known recording processes");
      //Utils.KillExternalTVProcesses();
#if !DEBUG
      try
      {
#endif
      if (splashScreen != null)
      {
        splashScreen.SetInformation("Initializing DirectX...");
      }
      MediaPortalApp app = new MediaPortalApp();
      Log.Write("  initializing DirectX");
      if (app.CreateGraphicsSample())
      {
        IMessageFilter filter = new ThreadMessageFilter(app);
        Application.AddMessageFilter(filter);
        try
        {
          //app.PreRun();
          Log.Write("running...");
          GUIGraphicsContext.BlankScreen = false;
          Application.Run(app);
          Debug.WriteLine("after Application.Run");
        }
        //#if !DEBUG
        catch (Exception ex)
        {
          Log.Write(ex);
          Log.WriteFile(Log.LogType.Log, true, "MediaPortal stopped due 2 an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
        }
        //#endif
        finally
        {
          Application.RemoveMessageFilter(filter);
        }
        app.OnExit();
      }
#if !DEBUG
      }
      catch (Exception ex)
      {
        Log.Write(ex);
        Log.WriteFile(Log.LogType.Log, true, "MediaPortal stopped due 2 an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
#endif
#if !DEBUG
      if (splashScreen != null)
      {
        splashScreen.Stop();
        splashScreen = null;
      }
#endif
      Settings.SaveCache();
      Log.Write("MediaPortal done");
      Win32API.EnableStartBar(true);
      Win32API.ShowStartBar(true);
      if (useRestartOptions)
      {
        Log.Write("MediaPortal exit windows:{0}", restartOptions);
        WindowsController.ExitWindows(restartOptions, true);
      }
    }
  }

  private static void ActivatePreviousInstance()
  {
    //Find the previous instance's process
    List<Process> processes = new List<Process>();
    string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
    if (processName.EndsWith(".vshost"))
      processName = processName.Substring(0, processName.Length - 7);
    processName = "mediaportal";
    processes.AddRange(System.Diagnostics.Process.GetProcessesByName(processName));
    processes.AddRange(System.Diagnostics.Process.GetProcessesByName(processName + ".vshost"));

    //System.Diagnostics.Process.GetCurrentProcess().ProcessName);
    foreach (Process process in processes)
    {
      if (process.Id == System.Diagnostics.Process.GetCurrentProcess().Id)
      {
        continue;
      }
      //Instructs the process to go to the foreground 
      SetForeGround(process);
      Environment.Exit(0);
    }
    Log.Write("Could not activate running instance.");
    MessageBox.Show("Could not activate running instance.", "MediaPortal is already running", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
    Environment.Exit(0);
  }

  /// <summary>
  /// Brings the previous MP instance to the front.
  /// </summary>
  /// <param name="_process">The <see cref="Process"/> that represents the previous MP instance</param>
  /// <remarks>
  /// We bring the application to the front by sending a WM_SHOWWINDOW message to all of it's threads.
  /// The main thread will detect this message using the <see cref="ThreadMessageFilter"/> and
  /// will instruct it's main window to show and activate itself.
  /// </remarks>
  private static void SetForeGround(Process _process)
  {
    foreach (ProcessThread thread in _process.Threads)
      Win32API.PostThreadMessage(thread.Id, Win32API.WM_SHOWWINDOW, 0, 0);
  }



  static UnhandledExceptionLogger logger;
  private static void AddExceptionHandler()
  {
    logger = new UnhandledExceptionLogger();
    AppDomain current = AppDomain.CurrentDomain;
    current.UnhandledException += new UnhandledExceptionEventHandler(logger.LogCrash);
  }

  #endregion

  #region remote callbacks

  private void OnRemoteCommand(object command)
  {
    GUIGraphicsContext.OnAction(new Action((Action.ActionType)command, 0, 0));
  }

  #endregion

  #region ctor

  public MediaPortalApp()
  {
    // check to load plugins
    using (Settings xmlreader = new Settings("MediaPortal.xml"))
    {
      useScreenSaver = xmlreader.GetValueAsBool("general", "screensaver", true);
    }

    // check if MediaPortal is already running...

    Log.Write("  Check if mediaportal is already started");

    Log.Write(@"  delete old log\capture.log file...");
    Utils.FileDelete(@"log\capture.log");
    if (Screen.PrimaryScreen.Bounds.Width > 720)
    {
      MinimumSize = new Size(720 + 8, 576 + 27);
    }
    else
    {
      MinimumSize = new Size(720, 576);
    }
    Text = "MediaPortal";


    GUIGraphicsContext.form = this;
    GUIGraphicsContext.graphics = null;
    GUIGraphicsContext.RenderGUI = this;
    try
    {
      using (Settings xmlreader = new Settings("MediaPortal.xml"))
      {
        m_strSkin = xmlreader.GetValueAsString("skin", "name", "BlueTwo");
        m_strLanguage = xmlreader.GetValueAsString("skin", "language", "English");
        _autoHideMouse = xmlreader.GetValueAsBool("general", "autohidemouse", false);
        GUIGraphicsContext.MouseSupport = xmlreader.GetValueAsBool("general", "mousesupport", true);
        GUIGraphicsContext.DBLClickAsRightClick = xmlreader.GetValueAsBool("general", "dblclickasrightclick", false);
        _minimizeOnStartup = xmlreader.GetValueAsBool("general", "minimizeonstartup", false);
        _minimizeOnGuiExit = xmlreader.GetValueAsBool("general", "minimizeonexit", false);
      }
    }
    catch (Exception)
    {
      m_strSkin = "BlueTwo";
      m_strLanguage = "english";
    }
    SetStyle(ControlStyles.Opaque, true);
    SetStyle(ControlStyles.UserPaint, true);
    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    SetStyle(ControlStyles.DoubleBuffer, false);

    Log.Write("  Check skin version");
    CheckSkinVersion();

    DoStartupJobs();
    //    startThread.Priority = ThreadPriority.BelowNormal;
    //    startThread.Start();
  }

  #endregion

  #region RenderStats() method

  private void RenderStats()
  {
    UpdateStats();
    if (m_bShowStats)
    {
      GUIFont font = GUIFontManager.GetFont(0);
      if (font != null)
      {
        font.DrawText(80, 40, 0xffffffff, frameStats, GUIControl.Alignment.ALIGN_LEFT, -1);
        region[0].X = m_ixpos;
        region[0].Y = 0;
        region[0].Width = 4;
        region[0].Height = GUIGraphicsContext.Height;
        GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.FromArgb(255, 255, 255, 255), 1.0f, 0, region);

        float fStep = (GUIGraphicsContext.Width - 100);
        fStep /= (2f * 16f);

        fStep /= GUIGraphicsContext.CurrentFPS;
        m_iFrameCount++;
        if (m_iFrameCount >= (int)fStep)
        {
          m_iFrameCount = 0;
          m_ixpos += 12;
          if (m_ixpos > GUIGraphicsContext.Width - 50)
          {
            m_ixpos = 50;
          }
        }
      }
    }
  }

  #endregion

  #region PreProcessMessage() and WndProc()

  protected override void WndProc(ref Message msg)
  {
    try
    {
      if (!PluginManager.WndProc(ref msg))
      {
        Action action;
        char key;
        Keys keyCode;

        if (MediaPortal.InputDevices.InputDevices.WndProc(ref msg, out action, out key, out keyCode))
        {
          if (msg.Result.ToInt32() != 1)
          {
            msg.Result = new IntPtr(0);
          }

          if (action != null && action.wID != Action.ActionType.ACTION_INVALID)
          {
            Log.Write("action:{0} ", action.wID);
            if (ActionTranslator.GetActionDetail(GUIWindowManager.ActiveWindowEx, action))
            {
              if (action.SoundFileName.Length > 0)
              {
                Utils.PlaySound(action.SoundFileName, false, true);
              }
            }
            GUIGraphicsContext.OnAction(action);
            screenSaverTimer = DateTime.Now;
            GUIGraphicsContext.BlankScreen = false;
          }

          if (keyCode != Keys.A)
          {
            Log.Write("keycode:{0} ", keyCode.ToString());
            KeyEventArgs ke = new KeyEventArgs(keyCode);
            keydown(ke);
            return;
          }
          if (key != 0)
          {
            Log.Write("key:{0} {1}", key, key);
            KeyPressEventArgs e = new KeyPressEventArgs(key);
            keypressed(e);
            return;
          }
          return;
        }
      }
      // plugins menu clicked?

      if (msg.Msg == WM_SYSCOMMAND && msg.WParam.ToInt32() == SC_SCREENSAVE)
      {
        // windows wants to activate the screensaver
        if (GUIGraphicsContext.IsFullScreenVideo ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
        {
          //disable it when we're watching tv/movies/...
          msg.Result = new IntPtr(0);
          return;
        }
      }

      if (msg.Msg == WM_POWERBROADCAST)
      {
        Log.Write("WM_POWERBROADCAST: {0}", msg.WParam.ToInt32());
        switch (msg.WParam.ToInt32())
        {
          //The PBT_APMQUERYSUSPEND message is sent to request permission to suspend the computer.
          //An application that grants permission should carry out preparations for the suspension before returning.
          //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSUSPEND:
            Log.Write("  windows wants to suspend(hibernate)");
            OnSuspend();
            break;

          //The PBT_APMQUERYSTANDBY message is sent to request permission to suspend the computer.
          //An application that grants permission should carry out preparations for the suspension before returning.
          //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSTANDBY:
            // Stop all media before suspending or hibernating
            Log.Write("  windows wants to go to standbye mode!");
            OnSuspend();
            break;
          case PBT_APMSUSPEND:
            Log.Write("  windows is suspending!");
            break;

          //The PBT_APMRESUMECRITICAL event is broadcast as a notification that the system has resumed operation. 
          //this event can indicate that some or all applications did not receive a PBT_APMSUSPEND event. 
          //For example, this event can be broadcast after a critical suspension caused by a failing battery.
          case PBT_APMRESUMECRITICAL:
            Log.Write("  windows has resumed from hibernate critical");
            OnResume();
            break;

          //The PBT_APMRESUMESUSPEND event is broadcast as a notification that the system has resumed operation after being suspended.
          case PBT_APMRESUMESUSPEND:
            Log.Write("  windows has resumed from hibernate normally");
            OnResume();
            break;

          //The PBT_APMRESUMESTANDBY event is broadcast as a notification that the system has resumed operation after being standbye.
          case PBT_APMRESUMESTANDBY:
            Log.Write("  windows has resumed from standbye normally");
            OnResume();
            break;

          //The PBT_APMRESUMEAUTOMATIC event is broadcast when the computer wakes up automatically to
          //handle an event. An application will not generally respond unless it is handling the event, because the user is not present.
          case PBT_APMRESUMEAUTOMATIC:
            Log.Write("  windows has resumed from standbye/hibernate to handle an event");
            OnResume();
            break;
        }
      }
      //if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("msg keydown");
      g_Player.WndProc(ref msg);
      base.WndProc(ref msg);
    }
    catch (Exception ex)
    {
      Log.Write(ex);
    }
  }
  //called when windows wants to hibernate or go into standbye mode
  void OnSuspend()
  {
    //stop playback
    _suspended = true;
    Log.Write("Mediaportal:stop playback");
    g_Player.Stop();
    Log.Write("Mediaportal:stop recorder");
    Recorder.Stop();

    //switch to windowed mode
    if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false && isMaximized)
    {
      Log.Write("Mediaportal:switch to windowed mode");
      SwitchFullScreenOrWindowed(true, true);
    }
  }

  //called when windows wakes up again
  void OnResume()
  {
    if (Recorder.Running) return;
    Log.Write("Mediaportal:start recorder");
    Recorder.Start();
    Log.Write("Mediaportal:switch to home screen");
    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
    _suspended = false;
  }

  #endregion

  // Trap the OnShown event so we can hide the window if the mimimize on startup option is set
  protected override void OnShown(EventArgs e)
  {
    if (_minimizeOnStartup && _firstTimeWindowDisplayed)
    {
      _firstTimeWindowDisplayed = false;
      DoMinimizeOnStartup();
    }

    base.OnShown(e);
  }

  #region process

  /// <summary>
  /// Process() gets called when a dialog is presented.
  /// It contains the message loop 
  /// </summary>
  public void Process()
  {
    if (_suspended) return;//we are suspended/hibernated
    try
    {
      g_Player.Process();
      HandleMessage();
      FrameMove();
      FullRender();
      if (GUIGraphicsContext.Vmr9Active)
      {
        Thread.Sleep(50);
      }
    }
    catch (Exception ex)
    {
      Log.Write(ex);
    }
  }

  #endregion

  #region RenderFrame()

  public void RenderFrame(float timePassed)
  {
    if (_suspended) return;//we are suspended/hibernated
    try
    {
      CreateStateBlock();
      GUILayerManager.Render(timePassed);
      RenderStats();
    }
    catch (Exception ex)
    {
      Log.Write(ex);
      Log.WriteFile(Log.LogType.Log, true, "RenderFrame exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
    }
  }

  #endregion

  #region Onstartup() / OnExit()

  /// <summary>
  /// OnStartup() gets called just before the application starts
  /// </summary>
  protected override void OnStartup()
  {
    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
    // set window form styles
    // these styles enable double buffering, which results in no flickering
    Log.Write("Mediaportal.OnStartup()");

    // set process priority
    _mouseTimeOutTimer = DateTime.Now;
    //System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Starting recorder...");
    }
    Recorder.Start();
    AutoPlay.StartListening();


    if (splashScreen != null)
    {
      splashScreen.SetInformation("Starting plugins...");
    }
    PluginManager.Load();
    PluginManager.Start();


    tMouseClickTimer = new Timer(SystemInformation.DoubleClickTime);
    tMouseClickTimer.AutoReset = false;
    tMouseClickTimer.Enabled = false;
    tMouseClickTimer.Elapsed += new ElapsedEventHandler(tMouseClickTimer_Elapsed);
    tMouseClickTimer.SynchronizingObject = this;

    using (Settings xmlreader = new Settings("MediaPortal.xml"))
    {
      string strDefault = xmlreader.GetValueAsString("myradio", "default", "");
      if (strDefault != "")
      {
        GUIMessage msg =
          new GUIMessage(GUIMessage.MessageType.GUI_MSG_RECORDER_TUNE_RADIO, (int)GUIWindow.Window.WINDOW_RADIO, 0, 0,
                         0, 0, null);
        msg.Label = strDefault;
        GUIGraphicsContext.SendMessage(msg);
      }
    }

    GUIPropertyManager.SetProperty("#date", GetDate());
    GUIPropertyManager.SetProperty("#time", GetTime());

    JobDispatcher.Init();
    //
    // Kill the splash screen
    //
    if (splashScreen != null)
    {
      splashScreen.Stop();
      Activate();
      while (!splashScreen.isStopped())
      {
        Thread.Sleep(100);
      }
      splashScreen = null;
    }
  }


  /// <summary>
  /// OnExit() Gets called just b4 application stops
  /// </summary>
  protected override void OnExit()
  {
    Log.Write("Mediaportal.OnExit()");
    JobDispatcher.Term();

    if (usbuirtdevice != null)
    {
      usbuirtdevice.Close();
    }
    if (serialuirdevice != null)
    {
      serialuirdevice.Close();
    }
    if (redeyedevice != null)
    {
      redeyedevice.Close();
    }

#if AUTOUPDATE
  StopUpdater();
#endif
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
    // stop any file playback
    g_Player.Stop();

    // tell window manager that application is closing
    // this gives the windows the chance to do some cleanup
    Recorder.Stop();

    MediaPortal.InputDevices.InputDevices.Stop();

    AutoPlay.StopListening();

    PluginManager.Stop();

    if (tMouseClickTimer != null)
    {
      tMouseClickTimer.Stop();
      tMouseClickTimer.Dispose();
      tMouseClickTimer = null;
    }


    GUIWaitCursor.Dispose();
    GUIFontManager.Dispose();
    GUIWindowManager.Clear();
    GUILocalizeStrings.Dispose();

    VolumeHandler.Dispose();

    // Restart MCE Services
    Utils.RestartMCEServices();
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
    GUIWindowManager.Clear();

    GUIWaitCursor.Dispose();
    GUITextureManager.Dispose();
    GUIFontManager.Dispose();

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading keymap.xml...");
    }
    ActionTranslator.Load();

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading strings...");
    }
    GUIGraphicsContext.Skin = @"skin\" + m_strSkin;
    GUIGraphicsContext.ActiveForm = Handle;
    GUILocalizeStrings.Load(@"language\" + m_strLanguage + @"\strings.xml");

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Initialize texture manager...");
    }
    GUITextureManager.Init();
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading fonts...");
    }
    GUIFontManager.LoadFonts(@"skin\" + m_strSkin + @"\fonts.xml");

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Initializing fonts...");
    }
    GUIFontManager.InitializeDeviceObjects();
    GUIFontManager.RestoreDeviceObjects();

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading skin...");
    }
    Log.Write("  Load skin {0}", m_strSkin);
    GUIWindowManager.Initialize();

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading window plugins...");
    }
    PluginManager.LoadWindowPlugins();


    Log.Write("  WindowManager.Load");
    GUIGraphicsContext.Load();

    if (splashScreen != null)
    {
      splashScreen.SetInformation("Initializing skin...");
    }
    Log.Write("  WindowManager.Preinitialize");
    GUIWindowManager.PreInit();
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;

    Log.Write("  WindowManager.ActivateWindow");
    // Edit Michel
    using (Settings xmlreader = new Settings("MediaPortal.xml"))
    {
      startWithBasicHome = xmlreader.GetValueAsBool("general", "startbasichome", false);
    }
    if ((startWithBasicHome) && (File.Exists(GUIGraphicsContext.Skin + @"\basichome.xml")))
    {
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SECOND_HOME);
    }
    else
    {
      GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
    }

    Log.Write("  skin initialized");
    if (GUIGraphicsContext.DX9Device != null)
    {
      Log.Write("  DX9 size: {0}x{1}", GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth,
                GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight);
      Log.Write("  video ram left:{0} KByte", GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1024);
    }

    MediaPortal.InputDevices.InputDevices.Init( /* splashScreen */);

    SetupCamera2D();

    g_nAnisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
    supportsFiltering = Manager.CheckDeviceFormat(
      GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
      GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
      GUIGraphicsContext.DX9Device.DisplayMode.Format,
      Usage.RenderTarget | Usage.QueryFilter, ResourceType.Textures,
      Format.A8R8G8B8);

    bSupportsAlphaBlend = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
                                                    GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
                                                    GUIGraphicsContext.DX9Device.DisplayMode.Format,
                                                    Usage.RenderTarget | Usage.QueryPostPixelShaderBlending,
                                                    ResourceType.Surface,
                                                    Format.A8R8G8B8);

    new GUILayerRenderer();
  }

  /// <summary>
  /// The device exists, but may have just been Reset().  Resources in
  /// Pool.Managed and any other device state that persists during
  /// rendering should be set here.  Render states, matrices, textures,
  /// etc., that don't change during rendering can be set once here to
  /// avoid redundant state setting during Render() or FrameMove().
  /// </summary>
  protected override void OnDeviceReset(Object sender, EventArgs e)
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
      GUIWaitCursor.Dispose();
      GUIFontManager.LoadFonts(@"skin\" + m_strSkin + @"\fonts.xml");
      GUIFontManager.InitializeDeviceObjects();
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIWindowManager.OnResize();
        GUIWindowManager.PreInit();
        GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
        GUIWindowManager.OnDeviceRestored();
        GUIGraphicsContext.Load();
      }
      Log.Write(" done");
      g_Player.PositionX++;
      g_Player.PositionX--;
    }
  }

  #endregion

  #region Render()

  private static bool reentrant = false;

  protected override void Render(float timePassed)
  {
    if (reentrant)
    {
      Log.Write("dx9 re-entrant"); //remove
      return;
    }
    if (GUIGraphicsContext.InVmr9Render)
    {
      Log.WriteFile(Log.LogType.Log, true, "Mediaportal.Render() called while in vmr9 render {0} {1}",
                    GUIGraphicsContext.Vmr9Active, GUIGraphicsContext.Vmr9FPS);
      return;
    }
    if (GUIGraphicsContext.Vmr9Active)
    {
      Log.WriteFile(Log.LogType.Log, true, "Mediaportal.Render() called while vmr9 active");
      return;
    }


    try
    {
      //	Log.Write("app:render()");
      reentrant = true;
      // if there's no DX9 device (during resizing for exmaple) then just return
      if (GUIGraphicsContext.DX9Device == null)
      {
        reentrant = false;
        //Log.Write("dx9 device=null");//remove
        return;
      }


      //Log.Write("render frame:{0}",frames);//remove
      ++frames;
      // clear the surface
      GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
      CreateStateBlock();
      GUIGraphicsContext.DX9Device.BeginScene();

      // ask the layer manager to render all layers
      GUILayerManager.Render(timePassed);
      RenderStats();

      GUIFontManager.Present();

      GUIGraphicsContext.DX9Device.EndScene();
      try
      {
        // Show the frame on the primary surface.
        GUIGraphicsContext.DX9Device.Present(); //SLOW
      }
      catch (DeviceLostException ex)
      {
        Log.Write(ex);
        //Log.Write("device lost exception {0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);//remove
        g_Player.Stop();
        deviceLost = true;
      }
      /*
        catch (Exception ex) // remove
        {
          Log.Write("exception {0} {1} {2}", ex.Message,ex.Source,ex.StackTrace);
        }*/
    }

    catch (Exception ex)
    {
      Log.Write(ex);
      //bool b = true;
    }
    finally
    {
      //					Log.Write("app:render() done");
      reentrant = false;
    }
  }

  #endregion

  #region OnProcess()

  protected override void OnProcess()
  {
    // Set the date & time
    if (DateTime.Now.Minute != m_updateTimer.Minute)
    {
      m_updateTimer = DateTime.Now;
      GUIPropertyManager.SetProperty("#date", GetDate());
      GUIPropertyManager.SetProperty("#time", GetTime());
    }

#if AUTOUPDATE
  CheckForNewUpdate();
#endif
    Recorder.Process();
    g_Player.Process();

    // update playing status
    if (g_Player.Playing)
    {
      if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
      {
        GUIGraphicsContext.IsFullScreenVideo = true;
      }
      GUIGraphicsContext.IsPlaying = true;
      GUIGraphicsContext.IsPlayingVideo = (g_Player.IsVideo || g_Player.IsTV);

      if (g_Player.Paused)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_pause.png");
      }
      else if (g_Player.Speed > 1)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_fastforward.png");
      }
      else if (g_Player.Speed < 1)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_rewind.png");
      }
      else if (g_Player.Playing)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_play.png");
      }

      if (g_Player.IsTV && !g_Player.IsTVRecording)
      {
        GUIPropertyManager.SetProperty("#currentplaytime", GUIPropertyManager.GetProperty("#TV.Record.current"));
        GUIPropertyManager.SetProperty("#shortcurrentplaytime", GUIPropertyManager.GetProperty("#TV.Record.current"));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentplaytime", Utils.SecondsToHMSString((int)g_Player.CurrentPosition));
        GUIPropertyManager.SetProperty("#currentremaining",
                                       Utils.SecondsToHMSString((int)(g_Player.Duration - g_Player.CurrentPosition)));
        GUIPropertyManager.SetProperty("#shortcurrentremaining",
                                       Utils.SecondsToShortHMSString(
                                         (int)(g_Player.Duration - g_Player.CurrentPosition)));
        GUIPropertyManager.SetProperty("#shortcurrentplaytime",
                                       Utils.SecondsToShortHMSString((int)g_Player.CurrentPosition));
      }

      if (g_Player.Duration > 0)
      {
        GUIPropertyManager.SetProperty("#duration", Utils.SecondsToHMSString((int)g_Player.Duration));
        GUIPropertyManager.SetProperty("#shortduration", Utils.SecondsToShortHMSString((int)g_Player.Duration));

        double fPercentage = g_Player.CurrentPosition / g_Player.Duration;
        int iPercent = (int)(100 * fPercentage);
        GUIPropertyManager.SetProperty("#percentage", iPercent.ToString());
      }
      else
      {
        GUIPropertyManager.SetProperty("#duration", String.Empty);
        GUIPropertyManager.SetProperty("#shortduration", String.Empty);
        GUIPropertyManager.SetProperty("#percentage", "0");
      }
      GUIPropertyManager.SetProperty("#playspeed", g_Player.Speed.ToString());
    }
    else
    {
      if (!Recorder.View)
      {
        GUIGraphicsContext.IsFullScreenVideo = false;
      }
      GUIGraphicsContext.IsPlaying = false;
    }
    if (!g_Player.Playing && !Recorder.IsRecording())
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
  }

  #endregion

  #region FrameMove()

  protected override void FrameMove()
  {
    if (_suspended) return;//we are suspended/hibernated
    try
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      {
        Log.Write("Mediaportal.FrameMove stopping()");
        Close();
      }
      try
      {
        GUIWindowManager.DispatchThreadMessages();
        GUIWindowManager.ProcessWindows();
      }
      catch (FileNotFoundException ex)
      {
        Log.Write(ex);
        MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Close();
      }
      if (useScreenSaver)
      {
        if (GUIGraphicsContext.IsFullScreenVideo ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
        {
          screenSaverTimer = DateTime.Now;
          GUIGraphicsContext.BlankScreen = false;
        }

        if (!GUIGraphicsContext.BlankScreen)
        {
          if (isMaximized)
          {
            int window = GUIWindowManager.ActiveWindow;
            if (window < (int)GUIWindow.Window.WINDOW_WIZARD_WELCOME ||
                window > (int)GUIWindow.Window.WINDOW_WIZARD_FINISHED)
            {
              TimeSpan ts = DateTime.Now - screenSaverTimer;
              if (ts.TotalSeconds >= 60)
              {
                GUIGraphicsContext.BlankScreen = true;
              }
            }
            else
            {
              screenSaverTimer = DateTime.Now;
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      Log.Write(ex);
    }
  }

  #endregion

  #region Handle messages, keypresses, mouse moves etc

  private void OnAction(Action action)
  {
    if (_suspended) return;
    try
    {
      GUIWindow window;
      if (action.IsUserAction())
      {
        screenSaverTimer = DateTime.Now;
        GUIGraphicsContext.BlankScreen = false;
      }
      switch (action.wID)
      {
        // record current tv program
        case Action.ActionType.ACTION_RECORD:
          GUIWindow tvHome = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
          if (tvHome != null)
          {
            if (tvHome.GetID != GUIWindowManager.ActiveWindow)
            {
              tvHome.OnAction(action);
              return;
            }
          }
          break;

        //show DVD menu
        case Action.ActionType.ACTION_DVD_MENU:
          if (g_Player.IsDVD && g_Player.Playing)
          {
            g_Player.OnAction(action);
            return;
          }
          break;

        //DVD: goto next chapter
        case Action.ActionType.ACTION_NEXT_CHAPTER:
          if (g_Player.IsDVD && g_Player.Playing)
          {
            g_Player.OnAction(action);
            return;
          }
          break;

        //DVD: goto previous chapter
        case Action.ActionType.ACTION_PREV_CHAPTER:
          if (g_Player.IsDVD && g_Player.Playing)
          {
            g_Player.OnAction(action);
            return;
          }
          break;

        //TV: zap to previous channel
        case Action.ActionType.ACTION_PREV_CHANNEL:
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        //TV: zap to next channel
        case Action.ActionType.ACTION_NEXT_CHANNEL:
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        //TV: zap to last channel viewed
        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL: // mPod
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        //toggle between directx windowed and exclusive mode
        case Action.ActionType.ACTION_TOGGLE_WINDOWED_FULSLCREEN:
          ToggleFullWindowed();
          return;
        //break;

        //mute or unmute audio
        case Action.ActionType.ACTION_VOLUME_MUTE:
          VolumeHandler.Instance.IsMuted = !VolumeHandler.Instance.IsMuted;
          break;

        //decrease volume 
        case Action.ActionType.ACTION_VOLUME_DOWN:
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Previous;
          return;

        //increase volume 
        case Action.ActionType.ACTION_VOLUME_UP:
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Next;
          break;

        //toggle live tv in background
        case Action.ActionType.ACTION_BACKGROUND_TOGGLE:

          //show livetv or video as background instead of the static GUI background
          // toggle livetv/video in background on/pff
          if (GUIGraphicsContext.ShowBackground)
          {
            Log.Write("Use live TV as background");
            // if on, but we're not playing any video or watching tv
            if (GUIGraphicsContext.Vmr9Active)
            {
              GUIGraphicsContext.ShowBackground = false;
              GUIGraphicsContext.ARType = Geometry.Type.Stretch;
            }
            else
            {
              //show warning message
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
              msg.Param1 = 727; //Live tv in background
              msg.Param2 = 728; //No Video/TV playing
              msg.Param3 = 729; //Make sure you use VMR9 and that something is playing
              GUIWindowManager.SendMessage(msg);
              return;
            }
          }
          else
          {
            Log.Write("Use GUI as background");
            GUIGraphicsContext.ShowBackground = true;
          }
          return;

        //switch between several home windows
        case Action.ActionType.ACTION_SWITCH_HOME:
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_HOME)
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SECOND_HOME);
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SECOND_HOME)
            GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
          else
          {
            if (startWithBasicHome)
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SECOND_HOME);
            else
              GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
          }
          return;

        //exit mediaportal
        case Action.ActionType.ACTION_EXIT:
          Log.Write("Mediaportal action exit()");
          // is the minimize on gui option set?  If so, minimize to tray...
          if (_minimizeOnGuiExit && !_shuttingDown)
          {
            if (WindowState != FormWindowState.Minimized)
            {
              Log.Write("Minimizing to tray on GUI exit and restoring Start Bar");
            }

            WindowState = FormWindowState.Minimized;
            Hide();

            Win32API.EnableStartBar(true);
            Win32API.ShowStartBar(true);
            return;
          }

          if (Recorder.IsAnyCardRecording())
          {
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
            msg.Param1 = 1033;
            msg.Param2 = 506;
            msg.Param3 = 0;
            GUIWindowManager.SendMessage(msg);

            if (msg.Param1 != 1)
            {
              return;
            }
          }
          GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
          return;

        //reboot pc
        case Action.ActionType.ACTION_REBOOT:
          {
            //reboot
            Log.Write("Mediaportal action reboot()");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
            msg.Param1 = 630;
            msg.Param2 = 0;
            msg.Param3 = 0;
            GUIWindowManager.SendMessage(msg);

            if (msg.Param1 == 1)
            {
              if (Recorder.IsAnyCardRecording())
              {
                msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
                msg.Param1 = 1033;
                msg.Param2 = 506;
                msg.Param3 = 0;
                GUIWindowManager.SendMessage(msg);

                if (msg.Param1 != 1)
                {
                  return;
                }
              }
              useRestartOptions = true;
              restartOptions = RestartOptions.Reboot;
              GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
            }
          }
          return;

        //eject cd
        case Action.ActionType.ACTION_EJECTCD:
          Utils.EjectCDROM();
          return;

        //shutdown pc
        case Action.ActionType.ACTION_SHUTDOWN:
          {
            Log.Write("Mediaportal: show shutdown/restart/sleep dialog");
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg != null)
            {
              dlg.Reset();
              dlg.SetHeading(GUILocalizeStrings.Get(924)); //Menu
              dlg.AddLocalizedString(1030); //PowerOff
              dlg.AddLocalizedString(1031); //Reboot
              dlg.AddLocalizedString(1032); //Suspend
              dlg.AddLocalizedString(1049); //Hibernate
              dlg.DoModal(GUIWindowManager.ActiveWindow);
              RestartOptions option = RestartOptions.Suspend;
              if (dlg.SelectedId < 0)
              {
                GUIWindow win = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_HOME);
                if (win != null) win.OnAction(new Action(Action.ActionType.ACTION_MOVE_LEFT, 0, 0));
                GUIOverlayWindow topBar = (GUIOverlayWindow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TOPBARHOME);
                if (topBar != null) topBar.Focused = true;
                return;
              }
              switch (dlg.SelectedId)
              {
                case 1030:
                  option = RestartOptions.PowerOff;
                  break;
                case 1031:
                  option = RestartOptions.Reboot;
                  break;
                case 1032:
                  option = RestartOptions.Suspend;
                  break;
                case 1049:
                  option = RestartOptions.Hibernate;
                  break;
              }
              if (Recorder.IsAnyCardRecording())
              {
                GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
                msg.Param1 = 1033;
                msg.Param2 = 506;
                msg.Param3 = 0;
                GUIWindowManager.SendMessage(msg);

                if (msg.Param1 != 1)
                {
                  GUIWindow win = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_HOME);
                  if (win != null) win.OnAction(new Action(Action.ActionType.ACTION_MOVE_LEFT, 0, 0));
                  GUIOverlayWindow topBar = (GUIOverlayWindow)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TOPBARHOME);
                  if (topBar != null) topBar.Focused = true;
                  return;
                }
              }
              if ((option == RestartOptions.Suspend) || (option == RestartOptions.Hibernate))
              {
                useRestartOptions = false;
                Log.Write("Mediaportal: exit :{0}", option);
                WindowsController.ExitWindows(option, false);
              }
              else
              {
                Log.Write("Mediaportal: exit :{0}", option);
                restartOptions = option;
                useRestartOptions = true;
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
              }
            }
            break;
          }

        //stop radio
        case Action.ActionType.ACTION_STOP:
          if (Recorder.IsRadio())
          {
            Recorder.StopRadio();
          }
          break;
      }

      if (g_Player.Playing || Recorder.IsRadio())
      {
        switch (action.wID)
        {
          //switch between GUI and fullscreen video/tv
          case Action.ActionType.ACTION_SHOW_GUI:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              GUIWindow win = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
              if (win.FullScreenVideoAllowed)
              {
                if (!g_Player.IsTV || g_Player.IsTVRecording)
                {
                  if (g_Player.HasVideo)
                  {
                    GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO);
                    GUIGraphicsContext.IsFullScreenVideo = true;
                    return;
                  }
                }
                else
                {
                  GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_TVFULLSCREEN);
                  GUIGraphicsContext.IsFullScreenVideo = true;
                  return;
                }
              }
            }
            break;

          //play previous item from playlist;
          case Action.ActionType.ACTION_PREV_ITEM:
            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key))
            {
              playlistPlayer.PlayPrevious();
            }
            break;

          //play next item from playlist;
          case Action.ActionType.ACTION_NEXT_ITEM:
            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key))
            {
              playlistPlayer.PlayNext();
            }
            break;

          //stop playback
          case Action.ActionType.ACTION_STOP:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              Log.Write("App.Onaction() stop media");
              g_Player.Stop();
              return;
            }
            break;

          //play music
          case Action.ActionType.ACTION_MUSIC_PLAY:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.StepNow();
              g_Player.Speed = 1;
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }

              return;
            }
            break;

          //pause (or resume playback)
          case Action.ActionType.ACTION_PAUSE:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              g_Player.Pause();
              return;
            }
            break;

          //resume playback
          case Action.ActionType.ACTION_PLAY:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              if (g_Player.Speed != 1)
              {
                g_Player.Speed = 1;
              }
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }
              return;
            }
            break;

          //fast forward...
          case Action.ActionType.ACTION_FORWARD:
          case Action.ActionType.ACTION_MUSIC_FORWARD:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              if (!g_Player.IsTV)
              {
                g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
                return;
              }
            }
            break;

          //fast rewind...
          case Action.ActionType.ACTION_REWIND:
          case Action.ActionType.ACTION_MUSIC_REWIND:
            if (!GUIGraphicsContext.IsFullScreenVideo)
            {
              if (!g_Player.IsTV)
              {
                Log.Write("***************************** rewind");
                g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
                return;
              }
            }
            break;
        }
      }
      GUIWindowManager.OnAction(action);
    }
    catch (FileNotFoundException ex)
    {
      Log.Write(ex);
      MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Close();
    }
    catch (Exception ex)
    {
      Log.Write(ex);
      Log.WriteFile(Log.LogType.Log, true, "  exception: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      throw new Exception("exception occured", ex);
    }
  }

  #region keypress handlers

  protected override void keypressed(KeyPressEventArgs e)
  {
    GUIGraphicsContext.BlankScreen = false;
    char keyc = e.KeyChar;
    // Log.Write("key:{0} 0x{1:X} (2)", (int)keyc, (int)keyc, keyc);
    Key key = new Key(e.KeyChar, 0);
    Action action = new Action();
    if (GUIWindowManager.IsRouted)
    {
      screenSaverTimer = DateTime.Now;
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
      {
        if (action.SoundFileName.Length > 0)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }
        GUIGraphicsContext.OnAction(action);
      }
      else
      {
        action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
        GUIGraphicsContext.OnAction(action);
      }
      return;
    }

    if (key.KeyChar == '!')
    {
      m_bShowStats = !m_bShowStats;
    }
    if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
    {
      if (action.ShouldDisableScreenSaver)
      {
        screenSaverTimer = DateTime.Now;
      }

      if (action.SoundFileName.Length > 0)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
    else
    {
      screenSaverTimer = DateTime.Now;
    }
    action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
    GUIGraphicsContext.OnAction(action);
  }

  protected override void keydown(KeyEventArgs e)
  {
    screenSaverTimer = DateTime.Now;
    GUIGraphicsContext.BlankScreen = false;
    Key key = new Key(0, (int)e.KeyCode);
    Action action = new Action();
    if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
    {
      if (action.SoundFileName.Length > 0)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
  }

  #endregion

  #region mouse event handlers

  protected override void OnMouseWheel(MouseEventArgs e)
  {
    // Calculate Mouse position
    Point ptClientUL;
    Point ptScreenUL = new Point();

    ptScreenUL.X = Cursor.Position.X;
    ptScreenUL.Y = Cursor.Position.Y;
    ptClientUL = PointToClient(ptScreenUL);

    int iCursorX = ptClientUL.X;
    int iCursorY = ptClientUL.Y;

    float fX = ((float)GUIGraphicsContext.Width) / ((float)ClientSize.Width);
    float fY = ((float)GUIGraphicsContext.Height) / ((float)ClientSize.Height);
    float x = (fX * iCursorX) - GUIGraphicsContext.OffsetX;
    float y = (fY * iCursorY) - GUIGraphicsContext.OffsetY;

    if (e.Delta > 0)
    {
      Action action = new Action(Action.ActionType.ACTION_MOVE_UP, x, y);
      action.MouseButton = e.Button;
      GUIGraphicsContext.OnAction(action);

      screenSaverTimer = DateTime.Now;
      GUIGraphicsContext.BlankScreen = false;
    }
    else if (e.Delta < 0)
    {
      Action action = new Action(Action.ActionType.ACTION_MOVE_DOWN, x, y);
      action.MouseButton = e.Button;
      GUIGraphicsContext.OnAction(action);

      screenSaverTimer = DateTime.Now;
      GUIGraphicsContext.BlankScreen = false;
    }
    base.OnMouseWheel(e);
  }

  protected override void mousemove(MouseEventArgs e)
  {
    // Disable first mouse action when mouse was hidden
    if (!_showCursor)
    {
      base.mousemove(e);
      return;
    }

    // Calculate Mouse position
    Point ptClientUL;
    Point ptScreenUL = new Point();

    ptScreenUL.X = Cursor.Position.X;
    ptScreenUL.Y = Cursor.Position.Y;
    ptClientUL = PointToClient(ptScreenUL);

    int iCursorX = ptClientUL.X;
    int iCursorY = ptClientUL.Y;

    if (m_iLastMousePositionX != iCursorX || m_iLastMousePositionY != iCursorY)
    {
      if ((Math.Abs(m_iLastMousePositionX - iCursorX) > 10) || (Math.Abs(m_iLastMousePositionY - iCursorY) > 10))
      {
        screenSaverTimer = DateTime.Now;
        GUIGraphicsContext.BlankScreen = false;
      }
      // check any still waiting single click events
      if (GUIGraphicsContext.DBLClickAsRightClick && bMouseClickFired)
      {
        if ((Math.Abs(m_iLastMousePositionX - iCursorX) > 10) || (Math.Abs(m_iLastMousePositionY - iCursorY) > 10))
        {
          CheckSingleClick();
        }
      }

      // Save last position
      m_iLastMousePositionX = iCursorX;
      m_iLastMousePositionY = iCursorY;

      //this.Text=String.Format("show {0},{1} {2},{3}",e.X,e.Y,m_iLastMousePositionX,m_iLastMousePositionY);

      float fX = ((float)GUIGraphicsContext.Width) / ((float)ClientSize.Width);
      float fY = ((float)GUIGraphicsContext.Height) / ((float)ClientSize.Height);
      float x = (fX * iCursorX) - GUIGraphicsContext.OffsetX;
      float y = (fY * iCursorY) - GUIGraphicsContext.OffsetY;

      GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if (window != null)
      {
        Action action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
        action.MouseButton = e.Button;
        GUIGraphicsContext.OnAction(action);
      }
    }
  }

  protected override void mouseclick(MouseEventArgs e)
  {
    screenSaverTimer = DateTime.Now;
    GUIGraphicsContext.BlankScreen = false;
    // Disable first mouse action when mouse was hidden
    if (!_showCursor)
    {
      base.mouseclick(e);
      return;
    }

    Action actionMove;
    Action action;
    bool MouseButtonRightClick = false;

    // Calculate Mouse position
    Point ptClientUL;
    Point ptScreenUL = new Point();

    ptScreenUL.X = Cursor.Position.X;
    ptScreenUL.Y = Cursor.Position.Y;
    ptClientUL = PointToClient(ptScreenUL);

    int iCursorX = ptClientUL.X;
    int iCursorY = ptClientUL.Y;

    // first move mouse
    float fX = ((float)GUIGraphicsContext.Width) / ((float)ClientSize.Width);
    float fY = ((float)GUIGraphicsContext.Height) / ((float)ClientSize.Height);
    float x = (fX * iCursorX) - GUIGraphicsContext.OffsetX;
    float y = (fY * iCursorY) - GUIGraphicsContext.OffsetY;
    ;

    // Save last position
    m_iLastMousePositionX = iCursorX;
    m_iLastMousePositionY = iCursorY;

    // Send move moved action
    actionMove = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
    GUIGraphicsContext.OnAction(actionMove);

    if (e.Button == MouseButtons.Left)
    {
      if (GUIGraphicsContext.DBLClickAsRightClick)
      {
        if (tMouseClickTimer != null)
        {
          bMouseClickFired = false;

          if (e.Clicks < 2)
          {
            eLastMouseClickEvent = e;
            bMouseClickFired = true;
            tMouseClickTimer.Start();
            return;
          }
          else
          {
            // Double click used as right click
            eLastMouseClickEvent = null;
            tMouseClickTimer.Stop();
            MouseButtonRightClick = true;
          }
        }
      }
      else
      {
        action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, x, y);
        action.MouseButton = e.Button;
        action.SoundFileName = "click.wav";
        if (action.SoundFileName.Length > 0)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }

        GUIGraphicsContext.OnAction(action);
        return;
      }
    }

    // right mouse button=back
    if ((e.Button == MouseButtons.Right) || (MouseButtonRightClick))
    {
      GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if ((window.GetFocusControlId() != -1) || GUIGraphicsContext.IsFullScreenVideo ||
          (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW))
      {
        // Get context menu
        action = new Action(Action.ActionType.ACTION_CONTEXT_MENU, x, y);
        action.MouseButton = e.Button;
        action.SoundFileName = "click.wav";
        if (action.SoundFileName.Length > 0)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }

        GUIGraphicsContext.OnAction(action);
      }
      else
      {
        Key key = new Key(0, (int)Keys.Escape);
        action = new Action();
        if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
        {
          if (action.SoundFileName.Length > 0)
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
          return;
        }
      }
    }

    //middle mouse button=Y
    if (e.Button == MouseButtons.Middle)
    {
      Key key = new Key('y', 0);
      action = new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
      {
        if (action.SoundFileName.Length > 0)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }
        GUIGraphicsContext.OnAction(action);
        return;
      }
    }
  }

  private void tMouseClickTimer_Elapsed(object sender, ElapsedEventArgs e)
  {
    CheckSingleClick();
  }

  private void CheckSingleClick()
  {
    Action action;

    // Check for touchscreen users and TVGuide items
    if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
    {
      GUIWindow pWindow = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if ((pWindow.GetFocusControlId() == 1) && (GUIWindowManager.RoutedWindow == -1))
      {
        // Dont send single click (only the mouse move event is send)
        bMouseClickFired = false;
        return;
      }
    }

    if (tMouseClickTimer != null)
    {
      tMouseClickTimer.Stop();
      if (bMouseClickFired)
      {
        float fX = ((float)GUIGraphicsContext.Width) / ((float)ClientSize.Width);
        float fY = ((float)GUIGraphicsContext.Height) / ((float)ClientSize.Height);
        float x = (fX * m_iLastMousePositionX) - GUIGraphicsContext.OffsetX;
        float y = (fY * m_iLastMousePositionY) - GUIGraphicsContext.OffsetY;

        bMouseClickFired = false;
        action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, x, y);
        action.MouseButton = eLastMouseClickEvent.Button;
        action.SoundFileName = "click.wav";
        if (action.SoundFileName.Length > 0)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }

        GUIGraphicsContext.OnAction(action);
      }
    }
  }

  #endregion

#if AUTOUPDATE
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

    GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO,0,0,0,0,0,0);
    msg.Param1=709;
    msg.Param2=710;
    msg.Param3=0;
    GUIWindowManager.SendMessage(msg);
    
    if (msg.Param1==0) 
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
    if (_updater==null) return;
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
#endif

  private void OnMessage(GUIMessage message)
  {
    if (_suspended) return;
    switch (message.Message)
    {
      case GUIMessage.MessageType.GUI_MSG_RESTART_REMOTE_CONTROLS:
        Log.Write("app:Restart remote controls");
        MediaPortal.InputDevices.InputDevices.Stop();
        MediaPortal.InputDevices.InputDevices.Init();
        break;

      case GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW:
        GUIWindowManager.ActivateWindow(message.Param1);
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
        {
          GUIGraphicsContext.IsFullScreenVideo = true;
        }
        else
        {
          GUIGraphicsContext.IsFullScreenVideo = false;
        }
        break;

      case GUIMessage.MessageType.GUI_MSG_CD_INSERTED:
        AutoPlay.ExamineCD(message.Label);
        break;

      case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
        AutoPlay.ExamineVolume(message.Label);
        break;

      case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL:
        bool bIsInteger;
        double retNum;
        bIsInteger = Double.TryParse(message.Label, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
        try
        {
          if (bIsInteger) //sd00//
          {
            usbuirtdevice.ChangeTunerChannel(message.Label);
          }
        }
        catch (Exception)
        { }
        try
        {
          winlircdevice.ChangeTunerChannel(message.Label);
        }
        catch (Exception)
        { }
        try
        {
          if (bIsInteger)
          {
            redeyedevice.ChangeTunerChannel(message.Label);
          }
        }
        catch (Exception)
        { }
        break;


      case GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED:
        bool fullscreen = false;
        if (message.Param1 != 0)
        {
          fullscreen = true;
        }
        message.Param1 = 0; //not full screen
        if (isMaximized == false)
        {
          return;
        }
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          return;
        }

        /*
                GUIWaitCursor.Dispose();
                GUITextureManager.CleanupThumbs();
                GUITextureManager.Dispose();
                GUIFontManager.Dispose();
        */
        if (fullscreen)
        {
          //switch to fullscreen mode
          Log.Write("goto fullscreen:{0}", GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
          if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed)
          {
            message.Param1 = 1;
          }
          else
          {
            SwitchFullScreenOrWindowed(false, true);
            if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed)
            {
              message.Param1 = 1;
            }
          }
        }
        else
        {
          //switch to windowed mode
          Log.Write("goto windowed:{0}", GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
          if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed)
          {
            return;
          }
          SwitchFullScreenOrWindowed(true, true);
        }
        //GUIWindowManager.OnResize();
        //GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);


        break;
    }
  }

  #endregion

  #region External process start / stop handling

  public void OnStartExternal(Process proc, bool waitForExit)
  {
    if (TopMost && waitForExit)
    {
      TopMost = false;
      restoreTopMost = true;
    }
  }

  public void OnStopExternal(Process proc, bool waitForExit)
  {
    if (restoreTopMost)
    {
      TopMost = true;
      restoreTopMost = false;
    }
  }

  #endregion

  #region helper funcs

  private void CreateStateBlock()
  {
    GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable = false;
    GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable = true;
    GUIGraphicsContext.DX9Device.RenderState.SourceBlend = Blend.SourceAlpha;
    GUIGraphicsContext.DX9Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
    GUIGraphicsContext.DX9Device.RenderState.FillMode = FillMode.Solid;
    GUIGraphicsContext.DX9Device.RenderState.CullMode = Cull.CounterClockwise;
    GUIGraphicsContext.DX9Device.RenderState.StencilEnable = false;
    //GUIGraphicsContext.DX9Device.RenderState.Clipping = true;
    GUIGraphicsContext.DX9Device.ClipPlanes.DisableAll();
    GUIGraphicsContext.DX9Device.RenderState.VertexBlend = VertexBlend.Disable;
    GUIGraphicsContext.DX9Device.RenderState.IndexedVertexBlendEnable = false;
    GUIGraphicsContext.DX9Device.RenderState.FogEnable = false;
    //GUIGraphicsContext.DX9Device.RenderState.ColorWriteEnable = ColorWriteEnable.RedGreenBlueAlpha;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation = TextureOperation.Modulate;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;
    GUIGraphicsContext.DX9Device.TextureState[0].TextureCoordinateIndex = 0;
    GUIGraphicsContext.DX9Device.TextureState[0].TextureTransform = TextureTransform.Disable; // REVIEW
    GUIGraphicsContext.DX9Device.TextureState[1].ColorOperation = TextureOperation.Disable;
    GUIGraphicsContext.DX9Device.TextureState[1].AlphaOperation = TextureOperation.Disable;
    /*		GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.None;
    GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.None;
    GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.None;
      */
    if (supportsFiltering)
    {
      GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[0].MaxAnisotropy = g_nAnisotropy;

      GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter = TextureFilter.Linear;
      GUIGraphicsContext.DX9Device.SamplerState[1].MaxAnisotropy = g_nAnisotropy;
    }
    else
    {
      GUIGraphicsContext.DX9Device.SamplerState[0].MinFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[0].MagFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[0].MipFilter = TextureFilter.Point;

      GUIGraphicsContext.DX9Device.SamplerState[1].MinFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[1].MagFilter = TextureFilter.Point;
      GUIGraphicsContext.DX9Device.SamplerState[1].MipFilter = TextureFilter.Point;
    }
    if (bSupportsAlphaBlend)
    {
      GUIGraphicsContext.DX9Device.RenderState.AlphaTestEnable = true;
      GUIGraphicsContext.DX9Device.RenderState.ReferenceAlpha = 0x01;
      GUIGraphicsContext.DX9Device.RenderState.AlphaFunction = Compare.GreaterEqual;
    }
  }

  /// <summary>
  /// Get the current date from the system and localize it based on the user preferences.
  /// </summary>
  /// <returns>A string containing the localized version of the date.</returns>
  protected string GetDate()
  {
    DateTime cur = DateTime.Now;
    string day;
    switch (cur.DayOfWeek)
    {
      case DayOfWeek.Monday:
        day = GUILocalizeStrings.Get(11);
        break;
      case DayOfWeek.Tuesday:
        day = GUILocalizeStrings.Get(12);
        break;
      case DayOfWeek.Wednesday:
        day = GUILocalizeStrings.Get(13);
        break;
      case DayOfWeek.Thursday:
        day = GUILocalizeStrings.Get(14);
        break;
      case DayOfWeek.Friday:
        day = GUILocalizeStrings.Get(15);
        break;
      case DayOfWeek.Saturday:
        day = GUILocalizeStrings.Get(16);
        break;
      default:
        day = GUILocalizeStrings.Get(17);
        break;
    }

    string month;
    switch (cur.Month)
    {
      case 1:
        month = GUILocalizeStrings.Get(21);
        break;
      case 2:
        month = GUILocalizeStrings.Get(22);
        break;
      case 3:
        month = GUILocalizeStrings.Get(23);
        break;
      case 4:
        month = GUILocalizeStrings.Get(24);
        break;
      case 5:
        month = GUILocalizeStrings.Get(25);
        break;
      case 6:
        month = GUILocalizeStrings.Get(26);
        break;
      case 7:
        month = GUILocalizeStrings.Get(27);
        break;
      case 8:
        month = GUILocalizeStrings.Get(28);
        break;
      case 9:
        month = GUILocalizeStrings.Get(29);
        break;
      case 10:
        month = GUILocalizeStrings.Get(30);
        break;
      case 11:
        month = GUILocalizeStrings.Get(31);
        break;
      default:
        month = GUILocalizeStrings.Get(32);
        break;
    }

    string strDate = String.Format("{0} {1} {2}", day, cur.Day, month);
    if (m_iDateLayout == 1)
    {
      strDate = String.Format("{0} {1} {2}", day, month, cur.Day);
    }
    return strDate;
  }

  /// <summary>
  /// Get the current time from the system.
  /// </summary>
  /// <returns>A string containing the current time.</returns>
  // TODO: Localize the time settings based on the user preferences
  protected string GetTime()
  {
    DateTime cur = DateTime.Now;
    string strTime = cur.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat);
    return strTime;
  }


  protected void CheckSkinVersion()
  {
    OldSkinForm form = new OldSkinForm();
    if (form.CheckSkinVersion(m_strSkin))
    {
      return;
    }
    form.ShowDialog(this);
  }

  #region registry helper function

  public static void SetDWORDRegKey(RegistryKey hklm, string Key, string Value, Int32 iValue)
  {
    RegistryKey subkey = hklm.CreateSubKey(Key);
    if (subkey != null)
    {
      subkey.SetValue(Value, iValue);
      subkey.Close();
    }
  }

  #endregion

  #endregion

  private void DoStartupJobs()
  {
    //Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
    // Stop MCE services
    Utils.StopMCEServices();

    Log.Write("Set registry keys for Hauppauge codecs");
    // Set Intervideo registry keys 
    try
    {
      RegistryKey hklm = Registry.LocalMachine;

      // windvd6 mpeg2 codec settings
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal", "BOBWEAVE", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\AudioDec", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "Dxva", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "DxvaFetchSample", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "ResendOnFamine", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "VgaQuery", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "VMR", 2);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "BOBWEAVE", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\AudioDec\MediaPortal", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "Dxva", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "DxvaFetchSample", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "ResendOnFamine", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "VgaQuery", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "VMR", 2);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "BOBWEAVE", 1);

      // hauppauge mpeg2 codec settings
      SetDWORDRegKey(hklm, @"SOFTWARE\IviSDK4Hauppauge\Common\VideoDec", "Hwmc", 1);
      SetDWORDRegKey(hklm, @"SOFTWARE\IviSDK4Hauppauge\Common\VideoDec", "Dxva", 1);

      hklm.Close();

      // windvd6 mpeg2 codec settings
      //hklm = Registry.CurrentUser;
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal", "BOBWEAVE", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\AudioDec", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "Dxva", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "DxvaFetchSample", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "ResendOnFamine", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "VgaQuery", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "VMR", 2);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal\VideoDec", "BOBWEAVE", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\MediaPortal", "BOBWEAVE", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\AudioDec\MediaPortal", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "DsContinuousRate", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "Dxva", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "DxvaFetchSample", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "ResendOnFamine", 0);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "VgaQuery", 1);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "VMR", 2);
      //SetDWORDRegKey(hklm, @"SOFTWARE\InterVideo\Common\VideoDec\MediaPortal", "BOBWEAVE", 1);

      //hklm.Close();
    }
    catch (Exception)
    { }

    GUIWindowManager.OnNewAction += new OnActionHandler(OnAction);


    GUIWindowManager.Receivers += new SendMessageHandler(OnMessage);
    GUIWindowManager.Callbacks += new GUIWindowManager.OnCallBackHandler(Process);
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STARTING;
    Utils.OnStartExternal += new Utils.UtilEventHandler(OnStartExternal);
    Utils.OnStopExternal += new Utils.UtilEventHandler(OnStopExternal);

    // load keymapping from keymap.xml
    ActionTranslator.Load();

    //register the playlistplayer for thread messages (like playback stopped,ended)
    Log.Write("  Init playlist player");
    g_Player.Factory = new PlayerFactory();
    playlistPlayer.Init();

    //
    // Only load the USBUIRT device if it has been enabled in the configuration
    //
    using (Settings xmlreader = new Settings("MediaPortal.xml"))
    {
      bool inputEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false);
      bool outputEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false);

      if (inputEnabled || outputEnabled)
      {
        Log.Write("  Creating the USBUIRT device");
        usbuirtdevice = USBUIRT.Create(new USBUIRT.OnRemoteCommand(OnRemoteCommand));
        Log.Write("  done creating the USBUIRT device");
      }
      //Load Winlirc if enabled.
      //sd00//
      bool winlircInputEnabled = xmlreader.GetValueAsString("WINLIRC", "enabled", "false") == "true";
      if (winlircInputEnabled)
      {
        Log.Write("  creating the WINLIRC device");
        winlircdevice = new WinLirc();
        Log.Write("  done creating the WINLIRC device");
      }
      //sd00//
      //Load RedEye if enabled.
      bool redeyeInputEnabled = xmlreader.GetValueAsString("RedEye", "internal", "false") == "true";
      if (redeyeInputEnabled)
      {
        Log.Write("creating the REDEYE device");
        redeyedevice = RedEye.Create(new RedEye.OnRemoteCommand(OnRemoteCommand));
        Log.Write("done creating the RedEye device");
      }
      inputEnabled = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";

      if (inputEnabled)
      {
        Log.Write("  creating the SerialUIR device");
        serialuirdevice = SerialUIR.Create(new SerialUIR.OnRemoteCommand(OnRemoteCommand));
        Log.Write("  done creating the SerialUIR device");
      }
    }

    //registers the player for video window size notifications
    Log.Write("  Init players");
    g_Player.Init();

    //  hook ProcessExit for a chance to clean up when closed peremptorily

#if AUTOUPDATE
  AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

      //  hook form close to stop updater too
      this.Closed += new EventHandler(MediaPortal_Closed);
#endif
    XmlDocument doc = new XmlDocument();
    try
    {
      doc.Load("mediaportal.exe.config");
      XmlNode node = doc.SelectSingleNode("/configuration/appStart/ClientApplicationInfo/appFolderName");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }

      node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/baseDir");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }

      node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/tempDir");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }
      doc.Save("Mediaportal.exe.config");
    }
    catch (Exception)
    { }


    Thumbs.CreateFolders();
    try
    {
#if DEBUG
#else
#if AUTOUPDATE
        UpdaterConfiguration config = UpdaterConfiguration.Instance;
				config.Logging.LogPath = System.IO.Directory.GetCurrentDirectory() + @"\log\updatelog.log";
				config.Applications[0].Client.BaseDir = System.IO.Directory.GetCurrentDirectory();
				config.Applications[0].Client.TempDir = System.IO.Directory.GetCurrentDirectory() + @"\temp";
				config.Applications[0].Client.XmlFile = System.IO.Directory.GetCurrentDirectory() + @"\MediaPortal.exe.config";
				config.Applications[0].Server.ServerManifestFileDestination = System.IO.Directory.GetCurrentDirectory() + @"\xml\ServerManifest.xml";
				
				try
				{
					System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\temp");
					System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\xml");
					System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"\log");
				}
				catch(Exception){}
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
#endif
#endif
    }
    catch (Exception)
    { }
    using (Settings xmlreader = new Settings("MediaPortal.xml"))
    {
      m_iDateLayout = xmlreader.GetValueAsInt("home", "datelayout", 0);
    }
    screenSaverTimer = DateTime.Now;
  }
}