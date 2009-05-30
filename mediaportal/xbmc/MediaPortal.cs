#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using MediaPortal;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.Dispatcher;
using MediaPortal.GUI.Library;
using MediaPortal.InputDevices;
using MediaPortal.IR;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.RedEyeIR;
using MediaPortal.Ripper;
using MediaPortal.SerialIR;
using MediaPortal.Util;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using OsDetection;
using Timer = System.Timers.Timer;

#endregion

namespace MediaPortal
{
  [Flags]
  public enum EXECUTION_STATE : uint
  {
    ES_SYSTEM_REQUIRED = 0x00000001,
    ES_DISPLAY_REQUIRED = 0x00000002,
    ES_CONTINUOUS = 0x80000000
  }
}

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
  private bool useScreenSaver = true;
  private bool useIdleblankScreen = false;
  private int timeScreenSaver = 300;
  private bool restoreTopMost = false;
  private bool _startWithBasicHome = false;
  private bool _suspended = false;
  private bool _runAutomaticResume = false;
  private bool ignoreContextMenuAction = false;
  private DateTime lastContextMenuAction = DateTime.MaxValue;
  private bool _onResumeRunning = false;
  private bool _onResumeAutomaticRunning = false;
  protected string _dateFormat = string.Empty;
  protected bool _useLongDateFormat = false;
  private bool showLastActiveModule = false;
  private int lastActiveModule = -1;
  private bool lastActiveModuleFullscreen = false;
  private static bool _mpCrashed = false;
  private static int _startupDelay = 0;
  private static bool _waitForTvServer = false;
#if AUTOUPDATE
  string m_strNewVersion = "";
  bool m_bNewVersionAvailable = false;
  bool m_bCancelVersion = false;
#endif
  private MouseEventArgs eLastMouseClickEvent = null;
  private Timer tMouseClickTimer = null;
  private bool bMouseClickFired = false;
  private const int WM_NCACTIVATE = 0x0086;
  private const int WM_SYSCOMMAND = 0x0112;
  private const int WM_POWERBROADCAST = 0x0218;
  private const int WM_ENDSESSION = 0x0016;
  private const int WM_DEVICECHANGE = 0x0219;
  private const int WM_QUERYENDSESSION = 0x0011;
  private const int PBT_APMQUERYSUSPEND = 0x0000;
  private const int PBT_APMQUERYSTANDBY = 0x0001;
  private const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
  private const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
  private const int PBT_APMSUSPEND = 0x0004;
  private const int PBT_APMSTANDBY = 0x0005;
  private const int PBT_APMRESUMECRITICAL = 0x0006;
  private const int PBT_APMRESUMESUSPEND = 0x0007;
  private const int PBT_APMRESUMESTANDBY = 0x0008;
  private const int PBT_APMRESUMEAUTOMATIC = 0x0012;
  private const int BROADCAST_QUERY_DENY = 0x424D5144;
  private const int SC_SCREENSAVE = 0xF140;
  private const string mpMutex = "{E0151CBA-7F81-41df-9849-F5298A779EB3}";
  private const string configMutex = "{0BFD648F-A59F-482A-961B-337D70968611}";
  private bool supportsFiltering = false;
  private bool bSupportsAlphaBlend = false;
  private int g_nAnisotropy;
  private DateTime m_updateTimer = DateTime.MinValue;
  private int m_iDateLayout;
  private static SplashScreen splashScreen;
  private static bool _avoidVersionChecking;

  #endregion

  [DllImport("Kernel32.DLL")]
  private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE state);

  private static RestartOptions restartOptions = RestartOptions.Reboot;
  private static bool useRestartOptions = false;

  [DllImport("shlwapi.dll")]
  private static extern bool PathIsNetworkPath(string Path);

  #region main()

  //NProf doesnt work if the [STAThread] attribute is set
  //but is needed when you want to play music or video
  [STAThread]
  public static void Main(string[] args)
  {
    Thread.CurrentThread.Name = "MPMain";
    if (args.Length > 0)
    {
      foreach (string arg in args)
      {
        if (arg == "/fullscreen")
        {
          _fullscreenOverride = true;
        }
        if (arg == "/windowed")
        {
          _windowedOverride = true;
        }
        if (arg.StartsWith("/fullscreen="))
        {
          string argValue = arg.Remove(0, 12); // remove /?= from the argument  
          _fullscreenOverride |= argValue != "no";
          _windowedOverride |= argValue.Equals("no");
        }
        if (arg == "/crashtest")
        {
          _mpCrashed = true;
        }
        if (arg.StartsWith("/screen="))
        {
          GUIGraphicsContext._useScreenSelector = true;
          string screenarg = arg.Remove(0, 8); // remove /?= from the argument          
          if (!int.TryParse(screenarg, out _screenNumberOverride))
          {
            _screenNumberOverride = -1;
          }
        }
        if (arg.StartsWith("/skin="))
        {
          string skinOverrideArg = arg.Remove(0, 6); // remove /?= from the argument
          _strSkinOverride = skinOverrideArg;
        }
        _avoidVersionChecking = false;
        if (arg.ToLowerInvariant() == "/avoidversioncheck")
        {
          _avoidVersionChecking = true;
          Log.Warn("Version check is disabled by command line switch \"/avoidVersionCheck\"");
        }
      }
    }
    Log.BackupLogFiles();
    if (!Config.DirsFileUpdateDetected)
    {
      try
      {
        using (RegistryKey hklm = Registry.LocalMachine)
        {
          SetREGSZRegKey(hklm, @"SOFTWARE\Team MediaPortal\MediaPortal", "ApplicationDir",
                         Config.GetFolder(Config.Dir.Base));
          SetREGSZRegKey(hklm, @"SOFTWARE\Team MediaPortal\MediaPortal", "ConfigDir",
                         Config.GetFolder(Config.Dir.Config));
        }
      }
      catch (SecurityException)
      {
        Log.Error("Not enough permissions to set registry keys for SVN installer");
      }
      catch (UnauthorizedAccessException)
      {
        Log.Error("No write permissions to set registry keys for SVN installer");
      }
      //check if mediaportal has been configured
      FileInfo fi = new FileInfo(Config.GetFile(Config.Dir.Config, "mediaportal.xml"));
      if ((!File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.xml"))) || (fi.Length < 10000))
      {
        //no, then start configuration.exe in wizard form
        Log.Info("MediaPortal.xml not found. Launching configuration tool and exiting...");
        Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard");
        return;
      }

      if ((File.Exists(Config.GetFile(Config.Dir.Installer, "queue.xml"))))
      {
        //test if exist mpi installer queue
        Log.Info("MPI queue not empty, Launching MPI installer tool .... ");
        Process.Start(Config.GetFile(Config.Dir.Base, "MPInstaller.exe"), @"/queue");
        return;
      }

      bool autoHideTaskbar = true;
      bool watchdogEnabled = true;
      bool restartOnError = false;
      int restartDelay = 10;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string MPThreadPriority = xmlreader.GetValueAsString("general", "ThreadPriority", "Normal");
        if (MPThreadPriority == "AboveNormal")
        {
          Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
          Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
        }
        else if (MPThreadPriority == "High")
        {
          Thread.CurrentThread.Priority = ThreadPriority.Highest;
          Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        }
        else if (MPThreadPriority == "BelowNormal")
        {
          Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
          Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        }
        autoHideTaskbar = xmlreader.GetValueAsBool("general", "hidetaskbar", false);
        _startupDelay = xmlreader.GetValueAsInt("general", "startup delay", 0);
        _waitForTvServer = xmlreader.GetValueAsBool("general", "wait for tvserver", false);
        watchdogEnabled = xmlreader.GetValueAsBool("general", "watchdogEnabled", true);
        restartOnError = xmlreader.GetValueAsBool("general", "restartOnError", false);
        restartDelay = xmlreader.GetValueAsInt("general", "restart delay", 10);

        GUIGraphicsContext._useScreenSelector |= xmlreader.GetValueAsBool("screenselector", "usescreenselector", false);
      }
#if !DEBUG
      AddExceptionHandler();
      if (watchdogEnabled)
      {
        //StreamWriter sw = new StreamWriter(Application.StartupPath + "\\mediaportal.running", false);
        // BAV: fixing mantis bug 1216: Watcher process uses a wrong folder for integrity file
        using (StreamWriter sw = new StreamWriter(Config.GetFile(Config.Dir.Config, "mediaportal.running"), false))
        {
          sw.WriteLine("running");
          sw.Close();
        }
        Log.Info("Main: Starting MPWatchDog");
        string cmdargs = "-watchdog";
        if (restartOnError)
        {
          cmdargs += " -restartMP " + restartDelay.ToString();
        }
        Process mpWatchDog = new Process();
        mpWatchDog.StartInfo.ErrorDialog = true;
        mpWatchDog.StartInfo.UseShellExecute = true;
        mpWatchDog.StartInfo.WorkingDirectory = Application.StartupPath;
        mpWatchDog.StartInfo.FileName = "WatchDog.exe";
        mpWatchDog.StartInfo.Arguments = cmdargs;
        mpWatchDog.Start();
      }
#endif

      //Log MediaPortal version build and operating system level
      OSVersionInfo os = new OperatingSystemVersion();
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      string ServicePack = String.Empty;
      if (!String.IsNullOrEmpty(os.OSCSDVersion))
      {
        ServicePack = " (" + os.OSCSDVersion + ")";
      }
      Log.Info("Main: MediaPortal v" + versionInfo.FileVersion + " is starting up on " + os.OSVersionString +
               ServicePack);

      // Store OS version for next checks
      int OsVer = (os.OSMajorVersion * 10) + os.OSMinorVersion;

      //If OS = WIndpwsXP64, WindowsServer2003 or Windows7 then we won't support them
      bool unsupported = false;
      switch (OsVer)
      {
        case 52:  //WindowsXP 64 and Windows2003
        case 61:  //Windows 7
          unsupported = true;
          break;
      }
      if (unsupported)
      {
        Log.Warn("****************************************");
        Log.Warn("* WARNING, OS not officially supported *");
        Log.Warn("****************************************");
      }

      //Log last install of WindowsUpdate patches
      string LastSuccessTime = "NEVER !!!";
      UIntPtr res = UIntPtr.Zero;

      int options = Convert.ToInt32(RegistryRights.ReadKey);
      if (OsVer >= 52)
      {
        options = options | Convert.ToInt32(RegWow64Options.KEY_WOW64_64KEY);
      }
      UIntPtr rKey = new UIntPtr(Convert.ToUInt32(RegistryRoot.HKLM));
      int lastError = 0;
      int retval = NativeMethods.RegOpenKeyEx(rKey, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\Results\\Install", 0, options, out res);
      if (retval == 0)
      {
        uint tKey;
        uint lKey = 100;
        System.Text.StringBuilder sKey = new System.Text.StringBuilder((int)lKey);
        retval = NativeMethods.RegQueryValueEx(res, "LastSuccessTime", 0, out tKey, sKey, ref lKey);
        if (retval == 0)
        {
          LastSuccessTime = sKey.ToString();
        }
        else
        {
          lastError = Marshal.GetLastWin32Error();
          Log.Debug("RegQueryValueEx retval=<{0}>, lastError=<{1}>", retval, lastError);
        }
      }
      else
      {
        lastError = Marshal.GetLastWin32Error();
        Log.Debug("RegOpenKeyEx retval=<{0}>, lastError=<{1}>", retval, lastError);
      }
      Log.Info("Main: Last install from WindowsUpdate is dated {0}", LastSuccessTime);

      //Disable "ghosting" for WindowsVista and up
      if (OsVer >= 60)
      {
        Log.Debug("Disabling process window ghosting");
        NativeMethods.DisableProcessWindowsGhosting();
      }

      //Start MediaPortal
      Log.Info("Main: Using Directories:");
      foreach (Config.Dir option in Enum.GetValues(typeof(Config.Dir)))
      {
        Log.Info("{0} - {1}", option, Config.GetFolder(option));
      }
      FileInfo mpFi = new FileInfo(Assembly.GetExecutingAssembly().Location);
      Log.Info("Main: Assembly creation time: {0} (UTC)", mpFi.LastWriteTimeUtc.ToUniversalTime());
      using (ProcessLock processLock = new ProcessLock(mpMutex))
      {
        if (processLock.AlreadyExists)
        {
          Log.Warn("Main: MediaPortal is already running");
          Win32API.ActivatePreviousInstance();
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        //Set current directory
        string applicationPath = Application.ExecutablePath;
        applicationPath = Path.GetFullPath(applicationPath);
        applicationPath = Path.GetDirectoryName(applicationPath);
        Directory.SetCurrentDirectory(applicationPath);
        Log.Info("Main: Set current directory to: {0}", applicationPath);
#if !DEBUG
        string version = ConfigurationManager.AppSettings["version"];
        //ClientApplicationInfo clientInfo = ClientApplicationInfo.Deserialize("MediaPortal.exe.config");
        splashScreen = new SplashScreen();
        splashScreen.Version = version;
        splashScreen.Run();
        //clientInfo=null;
#endif
        Application.DoEvents();
        if (_waitForTvServer)
        {
          Log.Debug("Main: Wait for TV service requested. Checking if installed...");
          ServiceController ctrl = null;
          try
          {
            ctrl = new ServiceController("TVService");
            string name = ctrl.ServiceName;
          }
          catch (Exception)
          {
            ctrl = null;
            Log.Debug("Main: TV service not installed - proceeding...");
          }
          if (ctrl != null)
          {
            Log.Debug("Main: TV service found. Checking status...");
            if (splashScreen != null)
            {
              splashScreen.SetInformation("Waiting for startup of TV service...");
            }
            if (ctrl.Status == ServiceControllerStatus.StartPending || ctrl.Status == ServiceControllerStatus.Stopped)
            {
              if (ctrl.Status == ServiceControllerStatus.StartPending)
              {
                Log.Info("Main: TV service start is pending. Waiting...");
              }
              if (ctrl.Status == ServiceControllerStatus.Stopped)
              {
                Log.Info("Main: TV service is stopped, so we try start it...");
                try
                {
                  ctrl.Start();
                }
                catch (Exception)
                {
                  Log.Info("TvService seems to be already starting up.");
                }
              }
              try
              {
                ctrl.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 45));
              }
              catch (Exception)
              {
              }
              if (ctrl.Status == ServiceControllerStatus.Running)
              {
                Log.Info("Main: The TV service has started successfully.");
              }
              else
              {
                Log.Info("Main: Startup of the TV service failed - current status: {0}", ctrl.Status.ToString());
              }
            }
            Log.Info("Main: TV service is in status {0} - proceeding...", ctrl.Status.ToString());
            ctrl.Close();
          }
        }
        Application.DoEvents();
        if (_startupDelay > 0)
        {
          Log.Info("Main: Waiting {0} second(s) before startup", _startupDelay);
          for (int i = _startupDelay; i > 0; i--)
          {
            if (splashScreen != null)
            {
              splashScreen.SetInformation("Waiting " + i.ToString() + " second(s) before startup...");
            }
            Application.DoEvents();
            Thread.Sleep(1000);
          }
        }
        Log.Debug("Main: Checking prerequisites");
        try
        {
          // CHECK if DirectX 9.0c if installed
          Log.Debug("Main: Verifying DirectX 9");
          if (!DirectXCheck.IsInstalled())
          {
            string strLine = "Please install a newer DirectX 9.0c redist!\r\n";
            strLine = strLine + "MediaPortal cannot run without DirectX 9.0c redist (August 2008)\r\n";
            strLine = strLine + "http://install.team-mediaportal.com/DirectX";
#if !DEBUG
            if (splashScreen != null)
            {
              splashScreen.Stop();
              splashScreen = null;
            }
#endif
            MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
          }
          Application.DoEvents();

          // CHECK if Windows MediaPlayer 11 is installed
          string WMP_Main_Ver = "11";
          Log.Debug("Main: Verifying Windows Media Player");

          Version aParamVersion;
          if (FilterChecker.CheckFileVersion(Environment.SystemDirectory + "\\wmp.dll", WMP_Main_Ver + ".0.0000.0000",
                                             out aParamVersion))
          {
            Log.Info("Main: Windows Media Player version {0} installed", aParamVersion);
          }
          else
          {
#if !DEBUG
            if (splashScreen != null)
            {
              splashScreen.Stop();
              splashScreen = null;
            }
#endif
            string strLine = "Please install Windows Media Player " + WMP_Main_Ver + "\r\n";
            strLine = strLine + "MediaPortal cannot run without Windows Media Player " + WMP_Main_Ver;
            MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //return;
          }

#if !DEBUG
          // Check TvPlugin version
          string MpExe = Assembly.GetExecutingAssembly().Location;
          string tvPlugin = Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll";
          if (File.Exists(tvPlugin) && !_avoidVersionChecking)
          {
            string tvPluginVersion = FileVersionInfo.GetVersionInfo(tvPlugin).ProductVersion;
            string MpVersion = FileVersionInfo.GetVersionInfo(MpExe).ProductVersion;
            if (MpVersion != tvPluginVersion)
            {
              string strLine = "TvPlugin and MediaPortal don't have the same version.\r\n";
              strLine += "Please update the older component to the same version as the newer one.\r\n";
              strLine += "MediaPortal Version: " + MpVersion + "\r\n";
              strLine += "TvPlugin    Version: " + tvPluginVersion;
              if (splashScreen != null)
              {
                splashScreen.Stop();
                splashScreen = null;
              }
              MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
              Log.Info(strLine);
              return;
            }
          }
#endif

        }
        catch (Exception)
        {
        }
        //following crashes on some pc's, dunno why
        //Log.Info("  Stop any known recording processes");
        //Utils.KillExternalTVProcesses();
#if !DEBUG
        try
        {
#endif
        Application.DoEvents();
        if (splashScreen != null)
        {
          splashScreen.SetInformation("Initializing DirectX...");
        }

        MediaPortalApp app = new MediaPortalApp();
        Log.Debug("Main: Initializing DirectX");
        if (app.CreateGraphicsSample())
        {
          IMessageFilter filter = new ThreadMessageFilter(app);
          Application.AddMessageFilter(filter);
          // Initialize Input Devices
          if (splashScreen != null)
          {
            splashScreen.SetInformation("Initializing input devices...");
          }
          InputDevices.Init();
          try
          {
            //app.PreRun();
            Log.Info("Main: Running");
            GUIGraphicsContext.BlankScreen = false;
            Application.Run(app);
            app.Focus();
            Debug.WriteLine("after Application.Run");
          }
          //#if !DEBUG
          catch (Exception ex)
          {
            Log.Error(ex);
            Log.Error("MediaPortal stopped due to an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
            _mpCrashed = true;
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
          Log.Error(ex);
          Log.Error("MediaPortal stopped due to an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
          _mpCrashed = true;
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

        if (autoHideTaskbar)
        {
          // only re-show the startbar if MP is the one that has hidden it.
          Win32API.EnableStartBar(true);
          Win32API.ShowStartBar(true);
        }
        if (useRestartOptions)
        {
          Log.Info("Main: Exiting Windows - {0}", restartOptions);
          if (File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
          {
            File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
          }
          WindowsController.ExitWindows(restartOptions, false);
        }
        else
        {
          if (!_mpCrashed)
          {
            if (File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
            {
              File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
            }
          }
        }
      }
    }
    else
    {
      string msg =
        "The file MediaPortalDirs.xml has been changed by a recent update in the MediaPortal application directory.\n\n";
      msg += "You have to open the file ";
      msg += Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Team MediaPortal\MediaPortalDirs.xml";
      msg +=
        " with an editor, update it with all changes and SAVE it at least once to start up MediaPortal successfully after this update.\n\n";
      msg += "If you are not using windows user profiles for MediaPortal's configuration management, ";
      msg += "just delete the whole directory mentioned above and reconfigure MediaPortal.";
      string msg2 = "\n\n\nDo you want to open your local file now?";
      Log.Error(msg);
#if !DEBUG
      if (splashScreen != null)
      {
        splashScreen.Stop();
        splashScreen = null;
      }
#endif
      DialogResult result = MessageBox.Show(msg + msg2, "MediaPortal - Update Conflict", MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Stop);
      try
      {
        if (result == DialogResult.Yes)
        {
          Process.Start("notepad.exe",
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                        @"\Team MediaPortal\MediaPortalDirs.xml");
        }
      }
      catch (Exception)
      {
        MessageBox.Show(
          "Error opening file " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
          @"\Team MediaPortal\MediaPortalDirs.xml using notepad.exe", "Error", MessageBoxButtons.OK,
          MessageBoxIcon.Error);
      }
    }
  }

#if !DEBUG
  private static UnhandledExceptionLogger logger;

  /// <remark>This method is only used in release builds.
  private static void AddExceptionHandler()
  {
    logger = new UnhandledExceptionLogger();
    AppDomain current = AppDomain.CurrentDomain;
    current.UnhandledException += new UnhandledExceptionEventHandler(logger.LogCrash);
  }
#endif

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
    int clientSizeX = 720;
    int clientSizeY = 576;
    int screenNumber = 0;
    // check to load plugins
    using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
    {
      useScreenSaver = xmlreader.GetValueAsBool("general", "IdleTimer", true);
      timeScreenSaver = xmlreader.GetValueAsInt("general", "IdleTimeValue", 300);
      useIdleblankScreen = xmlreader.GetValueAsBool("general", "IdleBlanking", false);
      clientSizeX = xmlreader.GetValueAsInt("general", "sizex", clientSizeX);
      clientSizeY = xmlreader.GetValueAsInt("general", "sizey", clientSizeY);
      showLastActiveModule = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
      lastActiveModule = xmlreader.GetValueAsInt("general", "lastactivemodule", -1);
      lastActiveModuleFullscreen = xmlreader.GetValueAsBool("general", "lastactivemodulefullscreen", false);
      screenNumber = xmlreader.GetValueAsInt("screenselector", "screennumber", screenNumber);
      //GUIGraphicsContext.UseSeparateRenderThread = xmlreader.GetValueAsBool("general", "userenderthread", true);
      // BAV: to be fixed -> until then deactivated to save user aggrivation 
      GUIGraphicsContext.UseSeparateRenderThread = false;
    }
    if (GUIGraphicsContext._useScreenSelector)
    {
      if (_screenNumberOverride >= 0)
      {
        screenNumber = _screenNumberOverride;
      }
      if (screenNumber < 0 || screenNumber >= Screen.AllScreens.Length)
      {
        screenNumber = 0;
      }
      Log.Info("currentScreenNr:" + screenNumber);
      GUIGraphicsContext.currentScreen = Screen.AllScreens[screenNumber];
    }
    // check if MediaPortal is already running...
    Log.Info("Main: Checking for running MediaPortal instance");
    Log.Info(@"Main: Deleting old log\capture.log");
    Utils.FileDelete(Config.GetFile(Config.Dir.Log, "capture.log"));
    if (GUIGraphicsContext.currentScreen.Bounds.Width > clientSizeX)
    {
      MinimumSize = new Size(clientSizeX + 8, clientSizeY + 27);
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
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (_strSkinOverride.Length > 0)
        {
          m_strSkin = _strSkinOverride;
        }
        else
        {
          m_strSkin = xmlreader.GetValueAsString("skin", "name", "Blue3");
        }
        m_strLanguage = xmlreader.GetValueAsString("skin", "language", "English");
        _autoHideMouse = xmlreader.GetValueAsBool("general", "autohidemouse", true);
        GUIGraphicsContext.MouseSupport = xmlreader.GetValueAsBool("general", "mousesupport", false);
        GUIGraphicsContext.DBLClickAsRightClick =
          xmlreader.GetValueAsBool("general", "dblclickasrightclick", false);
        _minimizeOnStartup = xmlreader.GetValueAsBool("general", "minimizeonstartup", false);
        _minimizeOnGuiExit = xmlreader.GetValueAsBool("general", "minimizeonexit", false);
      }
    }
    catch (Exception)
    {
      m_strSkin = "Blue3";
      m_strLanguage = "English";
    }
    SetStyle(ControlStyles.Opaque, true);
    SetStyle(ControlStyles.UserPaint, true);
    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    SetStyle(ControlStyles.DoubleBuffer, false);
    Activated += new EventHandler(MediaPortalApp_Activated);
    Deactivate += new EventHandler(MediaPortalApp_Deactivate);
    Log.Info("Main: Checking skin version");
    CheckSkinVersion();
    DoStartupJobs();
    //    startThread.Priority = ThreadPriority.BelowNormal;
    //    startThread.Start();
  }

  private static void MediaPortalApp_Deactivate(object sender, EventArgs e)
  {
    GUIGraphicsContext.HasFocus = false;
  }

  private static void MediaPortalApp_Activated(object sender, EventArgs e)
  {
    GUIGraphicsContext.HasFocus = true;
  }

  #endregion

  #region RenderStats() method

  private void RenderStats()
  {
    UpdateStats();
    if (m_bShowStats)
    {
      GetStats();
      GUIFont font = GUIFontManager.GetFont(0);
      if (font != null)
      {
        GUIGraphicsContext.SetScalingResolution(0, 0, false);
        // '\n' doesnt work with the DirectX9 Ex device, so the string is splitted into two
        font.DrawText(80, 40, 0xffffffff, frameStatsLine1, GUIControl.Alignment.ALIGN_LEFT, -1);
        font.DrawText(80, 55, 0xffffffff, frameStatsLine2, GUIControl.Alignment.ALIGN_LEFT, -1);
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
      if (msg.Msg == WM_POWERBROADCAST)
      {
        Log.Info("Main: WM_POWERBROADCAST: {0}", msg.WParam.ToInt32());
        switch (msg.WParam.ToInt32())
        {
          //The PBT_APMQUERYSUSPEND message is sent to request permission to suspend the computer.
          //An application that grants permission should carry out preparations for the suspension before returning.
          //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSUSPEND:
            Log.Info("Main: Windows is requesting hibernate mode - UI bit: {0}", msg.LParam.ToInt32());
            break;

          //The PBT_APMQUERYSTANDBY message is sent to request permission to suspend the computer.
          //An application that grants permission should carry out preparations for the suspension before returning.
          //Return TRUE to grant the request to suspend. To deny the request, return BROADCAST_QUERY_DENY.
          case PBT_APMQUERYSTANDBY:
            // Stop all media before suspending or hibernating
            Log.Info("Main: Windows is requesting standby mode - UI bit: {0}", msg.LParam.ToInt32());
            break;

          //The PBT_APMQUERYSUSPENDFAILED message is sent to notify the application that suspension was denied
          //by some other application. However, this message is only sent when we receive PBT_APMQUERY* before.
          case PBT_APMQUERYSUSPENDFAILED:
            Log.Info("Main: Windows is denied to go to suspended mode");
            // dero: IT IS NOT SAFE to rely on this message being sent! Sometimes it is not sent even if we
            // processed PBT_AMQUERYSUSPEND/PBT_APMQUERYSTANDBY
            // I observed this using TVService.PowerScheduler
            break;

          //The PBT_APMQUERYSTANDBYFAILED message is sent to notify the application that suspension was denied
          //by some other application. However, this message is only sent when we receive PBT_APMQUERY* before.
          case PBT_APMQUERYSTANDBYFAILED:
            Log.Info("Main: Windows is denied to go to standby mode");
            // dero: IT IS NOT SAFE to rely on this message being sent! Sometimes it is not sent even if we
            // processed PBT_AMQUERYSUSPEND/PBT_APMQUERYSTANDBY
            // I observed this using TVService.PowerScheduler
            break;

          case PBT_APMSTANDBY:
            Log.Info("Main: Windows is going to standby");
            OnSuspend(ref msg);
            break;

          case PBT_APMSUSPEND:
            Log.Info("Main: Windows is suspending");
            OnSuspend(ref msg);
            break;

          //The PBT_APMRESUMECRITICAL event is broadcast as a notification that the system has resumed operation. 
          //this event can indicate that some or all applications did not receive a PBT_APMSUSPEND event. 
          //For example, this event can be broadcast after a critical suspension caused by a failing battery.
          case PBT_APMRESUMECRITICAL:
            Log.Info("Main: Windows has resumed from critical hibernate mode");
            OnResume();
            break;

          //The PBT_APMRESUMESUSPEND event is broadcast as a notification that the system has resumed operation after being suspended.
          case PBT_APMRESUMESUSPEND:
            Log.Info("Main: Windows has resumed from hibernate mode");
            OnResume();
            break;

          //The PBT_APMRESUMESTANDBY event is broadcast as a notification that the system has resumed operation after being standby.
          case PBT_APMRESUMESTANDBY:
            Log.Info("Main: Windows has resumed from standby mode");
            OnResume();
            break;

          //The PBT_APMRESUMEAUTOMATIC event is broadcast when the computer wakes up automatically to
          //handle an event. An application will not generally respond unless it is handling the event, because the user is not present.
          case PBT_APMRESUMEAUTOMATIC:
            Log.Info("Main: Windows has resumed from standby or hibernate mode to handle a requested event");
            OnResumeAutomatic();

            bool useS3Hack;
            using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
              useS3Hack = xmlreader.GetValueAsBool("debug", "useS3Hack", false);
              if (useS3Hack)
              {
                Log.Info("Main: useS3Hack enabled, calling OnResume() on automatic resume");
                OnResume();
              }
            }

            break;
        }
      }
      else if (msg.Msg == WM_QUERYENDSESSION)
      {
        Log.Info("Main: Windows is requesting shutdown mode");
        base.WndProc(ref msg);
        Log.Info("Main: shutdown mode granted");
        _shuttingDown = true;
        msg.Result = (IntPtr)1; //tell windows we are ready to shutdown          
      }
      // gibman - http://mantis.team-mediaportal.com/view.php?id=1073     
      else if (msg.Msg == WM_ENDSESSION) // && msg.WParam == ((IntPtr)1))
      {
        base.WndProc(ref msg);
        Log.Info("Main: shutdown mode executed");
        msg.Result = IntPtr.Zero; // tell windows it's ok to shutdown        
        tMouseClickTimer.Stop();
        tMouseClickTimer.Dispose();
        Application.ExitThread();
        Application.Exit();
      }
      else if (msg.Msg == WM_DEVICECHANGE)
      {
        if (RemovableDriveHelper.HandleDeviceChangedMessage(msg))
        {
          return;
        }
      }
      if (PluginManager.WndProc(ref msg))
      {
        // msg.Result = new IntPtr(0); <-- do plugins really set it on their own?
        return;
      }
      else
      {
        Action action;
        char key;
        Keys keyCode;
        if (InputDevices.WndProc(ref msg, out action, out key, out keyCode))
        {
          if (msg.Result.ToInt32() != 1)
          {
            msg.Result = new IntPtr(0);
          }
          if (action != null && action.wID != Action.ActionType.ACTION_INVALID)
          {
            Log.Info("Main: Incoming action: {0}", action.wID);
            if (ActionTranslator.GetActionDetail(GUIWindowManager.ActiveWindowEx, action))
            {
              if (action.SoundFileName.Length > 0 && !g_Player.Playing)
              {
                Utils.PlaySound(action.SoundFileName, false, true);
              }
            }
            GUIGraphicsContext.OnAction(action);
            GUIGraphicsContext.ResetLastActivity();
          }
          if (keyCode != Keys.A)
          {
            Log.Info("Main: Incoming Keycode: {0}", keyCode.ToString());
            KeyEventArgs ke = new KeyEventArgs(keyCode);
            keydown(ke);
            return;
          }
          if (key != 0)
          {
            Log.Info("Main: Incoming Key: {0}", key);
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
      //if (msg.Msg==WM_KEYDOWN) Debug.WriteLine("msg keydown");
      g_Player.WndProc(ref msg);
      base.WndProc(ref msg);
    }
    catch (Exception ex)
    {
      Log.Error(ex);
    }
  }

  private static object syncObj = new object();

  // we only dispose the DB connection if the DB path is remote.      
  // since local db's have no problems.
  private void DisposeDBs()
  {
    string dbPath = Config.GetFile(Config.Dir.Database, "FolderDatabase3.db3");
    bool isRemotePath = PathIsNetworkPath(dbPath);
    if (isRemotePath)
    {
      Log.Info("Main: disposing FolderDatabase3 sqllite database.");
      DatabaseFactory.GetFolderDatabase().Dispose();
    }
    dbPath = Config.GetFile(Config.Dir.Database, "PictureDatabase.db3");
    isRemotePath = PathIsNetworkPath(dbPath);
    if (isRemotePath)
    {
      Log.Info("Main: disposing PictureDatabase sqllite database.");
      DatabaseFactory.GetPictureDatabase().Dispose();
    }
    dbPath = Config.GetFile(Config.Dir.Database, "VideoDatabaseV5.db3");
    isRemotePath = PathIsNetworkPath(dbPath);
    if (isRemotePath)
    {
      Log.Info("Main: disposing VideoDatabaseV5.db3 sqllite database.");
      DatabaseFactory.GetVideoDatabase().Dispose();
    }
    //TODO close music and TV db's
    /*
    dbPath = Config.GetFile(Config.Dir.Database, "MusicDatabaseV8.db3");
    isRemotePath = PathIsNetworkPath(dbPath);
    if (isRemotePath)
    {
      Log.Info("Main: disposing MusicDatabaseV8.db3 sqllite database.");      
      MediaPortal.Database.DatabaseFactory.GetVideoDatabase().Dispose();
    }
    dbPath = Config.GetFile(Config.Dir.Database, "TVDatabaseV21.db3");
    isRemotePath = PathIsNetworkPath(dbPath);
    if (isRemotePath)
    {
      Log.Info("Main: disposing TVDatabaseV21.db3 sqllite database.");
      MediaPortal.Database.DatabaseFactory.GetVideoDatabase().Dispose();
    }         
    */
  }

  //called when windows hibernates or goes into standbye mode
  private void OnSuspend(ref Message msg)
  {
    lock (syncObj)
    {
      if (_suspended)
      {
        return;
      }
      ignoreContextMenuAction = true;
      _suspended = true;
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING; // this will close all open dialogs
      SaveLastActiveModule();
      Log.Info("Main: Stopping playback");
      if (GUIGraphicsContext.IsPlaying)
      {
        g_Player.Stop();
        //wait for player to stop before proceeding                
        while (GUIGraphicsContext.IsPlaying)
        {
          Thread.Sleep(100);
        }
      }
      //switch to windowed mode
      if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed == false && !windowed)
      {
        Log.Info("Main: Switching to windowed mode");
        SwitchFullScreenOrWindowed(true);
      }
      //stop playback
      _suspended = true;
      _runAutomaticResume = true;
      InputDevices.Stop();
      Log.Info("Main: Stopping AutoPlay");
      AutoPlay.StopListening();
      // we only dispose the DB connection if the DB path is remote.      
      // since local db's have no problems.
      DisposeDBs();
      Log.Info("Main: OnSuspend - Done");
    }
  }

  //called when windows wakes up again
  private static object syncResume = new object();
  private static object syncResumeAutomatic = new object();

  private bool IsNetworkConnected()
  {
    DateTime now = DateTime.Now;
    TimeSpan ts = now - DateTime.Now;
    bool connected = false;
    if (SystemInformation.Network)
    {
      NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
      while (ts.TotalSeconds > -10)
      {
        foreach (NetworkInterface n in adapters)
        {
          if (n.OperationalStatus == OperationalStatus.Up)
          {
            connected = true;
            break;
          }
        }
        if (connected)
        {
          break;
        }
        ts = now - DateTime.Now;
      }
    }
    return connected;
  }

  private void OnResumeAutomatic()
  {
    if (_onResumeAutomaticRunning == true)
    {
      Log.Info("Main: OnResumeAutomatic - already running -> return without further action");
      return;
    }
    Log.Debug("Main: OnResumeAutomatic - set lock for syncronous inits");
    lock (syncResumeAutomatic)
    {
      if (!_runAutomaticResume)
      {
        Log.Info("Main: OnResumeAutomatic - OnResume called but !_suspended");
        return;
      }

      _onResumeAutomaticRunning = false;
      _runAutomaticResume = false;
      Log.Info("Main: OnResumeAutomatic - Done");
    }
  }

  private void OnResume()
  {
    GUIGraphicsContext.ResetLastActivity(); // avoid ScreenSaver after standby
    ignoreContextMenuAction = true;
    if (_onResumeRunning == true)
    {
      Log.Info("Main: OnResume - already running -> return without further action");
      return;
    }
    Log.Debug("Main: OnResume - set lock for syncronous inits");
    lock (syncResume)
    {
      if (!_suspended)
      {
        Log.Info("Main: OnResume - OnResume called but !_suspended");
        return;
      }

      _onResumeRunning = true;

      // Systems without DirectX9 Ex have lost graphics device in suspend/hibernate cycle
      if (!GUIGraphicsContext.IsDirectX9ExUsed())
      {
        Log.Info("Main: OnResume - set GUIGraphicsContext.State.LOST");
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
      }

      EXECUTION_STATE oldState = EXECUTION_STATE.ES_CONTINUOUS;
      bool turnMonitorOn;
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        turnMonitorOn = xmlreader.GetValueAsBool("general", "turnmonitoronafterresume", false);
        if (turnMonitorOn)
        {
          Log.Info("Main: OnResume - Trying to wake up the monitor / tv");
          EXECUTION_STATE state = EXECUTION_STATE.ES_CONTINUOUS |
                                  EXECUTION_STATE.ES_DISPLAY_REQUIRED;
          oldState = SetThreadExecutionState(state);
        }
        if (xmlreader.GetValueAsBool("general", "restartonresume", false))
        {
          Log.Info("Main: OnResume - prepare for restart!");
          File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
          Log.Info("Main: OnResume - saving settings...");
          Settings.SaveCache();
          Process restartScript = new Process();
          restartScript.EnableRaisingEvents = false;
          restartScript.StartInfo.WorkingDirectory = Config.GetFolder(Config.Dir.Base);
          restartScript.StartInfo.FileName = Config.GetFile(Config.Dir.Base, @"restart.vbs");
          Log.Debug("Main: OnResume - executing script {0}", restartScript.StartInfo.FileName);
          restartScript.Start();
          try
          {
            // Maybe the scripting host is not available therefore do not wait infinitely.
            if (!restartScript.HasExited)
            {
              restartScript.WaitForExit();
            }
          }
          catch (Exception ex)
          {
            Log.Error("Main: OnResume - WaitForExit: {0}", ex.Message);
          }
        }
      }
      if (_startWithBasicHome)
      {
        Log.Info("Main: OnResume - Switch to basic home screen");
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SECOND_HOME);
      }
      else
      {
        Log.Info("Main: OnResume - Switch to home screen");
        GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_HOME);
      }
      Log.Info("Main: OnResume - calling recover device");
      base.RecoverDevice();
      if (turnMonitorOn)
      {
        SetThreadExecutionState(oldState);
      }
      Log.Info("Main: OnResume - init InputDevices");
      InputDevices.Init();
      _suspended = false;
      Log.Debug("Main: OnResume - show last active module?");
      bool result = base.ShowLastActiveModule();

      if (GUIGraphicsContext.IsDirectX9ExUsed())
      {
        Log.Info("Main: OnResume - set GUIGraphicsContext.State.RUNNING");
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
      }

      Log.Debug("Main: OnResume - autoplay start listening");
      AutoPlay.StartListening();
      _onResumeRunning = false;
      ignoreContextMenuAction = false;
      Log.Info("Main: OnResume - Done");
    }
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
  public void MPProcess()
  {
    if (_suspended)
    {
      return;
    } //we are suspended/hibernated
    try
    {
      g_Player.Process();
      HandleMessage();
      FrameMove();
      //if (GUIGraphicsContext.UseSeparateRenderThread)
      //{
      //  // the part of FullRender() [ from Render3DEnvironment(); ] which is needed on Resume...
      //  if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
      //  {
      //    RecoverDevice();
      //  }
      //}
      //else
      //{
      FullRender();
      //}
      if (GUIGraphicsContext.Vmr9Active)
      {
        Thread.Sleep(50);
      }
    }
    catch (Exception ex)
    {
      Log.Error(ex);
    }
  }

  #endregion

  #region RenderFrame()

  public void RenderFrame(float timePassed)
  {
    if (_suspended)
    {
      return;
    } //we are suspended/hibernated
    try
    {
      CreateStateBlock();
      GUILayerManager.Render(timePassed);
      RenderStats();
    }
    catch (Exception ex)
    {
      Log.Error(ex);
      Log.Error("RenderFrame exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
    }
  }

  #endregion

  #region Onstartup() / OnExit()

  /// <summary>
  /// OnStartup() gets called just before the application starts
  /// </summary>
  protected override void OnStartup()
  {
    //Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
    // set window form styles
    // these styles enable double buffering, which results in no flickering
    Log.Info("Main: Starting up");
    // set process priority
    _mouseTimeOutTimer = DateTime.Now;
    //System.Threading.Thread.CurrentThread.Priority=System.Threading.ThreadPriority.BelowNormal;
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Starting recorder...");
    }
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
    using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
    {
      _dateFormat = xmlreader.GetValueAsString("home", "dateformat", "<Day> <Month> <DD>");
    }
    // Asynchronously pre-initialize the music engine if we're using the BassMusicPlayer
    if (BassMusicPlayer.IsDefaultMusicPlayer)
    {
      BassMusicPlayer.CreatePlayerAsync();
    }
    try
    {
      GUIPropertyManager.SetProperty("#date", GetDate());
      GUIPropertyManager.SetProperty("#time", GetTime());
      GUIPropertyManager.SetProperty("#Day", GetDay()); // 01
      GUIPropertyManager.SetProperty("#SDOW", GetShortDayOfWeek()); // Sun
      GUIPropertyManager.SetProperty("#DOW", GetDayOfWeek()); // Sunday
      GUIPropertyManager.SetProperty("#Month", GetMonth()); // 01
      GUIPropertyManager.SetProperty("#SMOY", GetShortMonthOfYear()); // Jan
      GUIPropertyManager.SetProperty("#MOY", GetMonthOfYear()); // January
      GUIPropertyManager.SetProperty("#SY", GetShortYear()); // 80
      GUIPropertyManager.SetProperty("#Year", GetYear()); // 1980
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
    catch (Exception ex)
    {
      Log.Error("MediaPortalApp: Error setting date and time properties - {0}", ex.Message);
    }
    AutoPlay.StartListening();
  }

  /// <summary>
  /// saves last active module.
  /// </summary>
  private void SaveLastActiveModule()
  {
    // persist the currently selected module to XML for later use.
    Log.Debug("Main: SaveLastActiveModule - enabled {0}", showLastActiveModule);
    if (showLastActiveModule)
    {
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        string currentmoduleid = GUIPropertyManager.GetProperty("#currentmoduleid");
        bool currentmodulefullscreen = (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                                        GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC ||
                                        GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
                                        GUIWindowManager.ActiveWindow ==
                                        (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
        string currentmodulefullscreenstate = GUIPropertyManager.GetProperty("#currentmodulefullscreenstate");
        // if MP was closed/hibernated by the use of remote control, we have to retrieve the fullscreen state in an alternative manner.
        if (!currentmodulefullscreen && currentmodulefullscreenstate == "True")
        {
          currentmodulefullscreen = true;
        }
        if (currentmoduleid.Length == 0)
        {
          currentmoduleid = "0";
        }
        xmlreader.SetValue("general", "lastactivemodule", currentmoduleid);
        xmlreader.SetValueAsBool("general", "lastactivemodulefullscreen", currentmodulefullscreen);
        Log.Debug("Main: SaveLastActiveModule - module {0}", currentmoduleid);
        Log.Debug("Main: SaveLastActiveModule - fullscreen {0}", currentmodulefullscreen);
      }
    }
  }

  /// <summary>
  /// OnExit() Gets called just b4 application stops
  /// </summary>
  protected override void OnExit()
  {
    SaveLastActiveModule();
    Log.Info("Main: Exiting");
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
    InputDevices.Stop();
    AutoPlay.StopListening();
    PluginManager.Stop();
    if (tMouseClickTimer != null)
    {
      tMouseClickTimer.Stop();
      tMouseClickTimer.Dispose();
      tMouseClickTimer = null;
    }
    GUIWaitCursor.Dispose();
    GUIFontManager.ReleaseUnmanagedResources();
    GUIFontManager.Dispose();
    GUITextureManager.Dispose();
    GUIWindowManager.Clear();
    GUILocalizeStrings.Dispose();
    TexturePacker.Cleanup();
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
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading keymap.xml...");
    }
    ActionTranslator.Load();
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading strings...");
    }
    GUIGraphicsContext.Skin = m_strSkin;
    GUIGraphicsContext.ActiveForm = Handle;
    GUILocalizeStrings.Load(m_strLanguage); //Config.GetFile(Config.Dir.Language, m_strLanguage, "strings.xml"));
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Initialize texture manager...");
    }
    GUITextureManager.Init();
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading fonts...");
    }
    GUIFontManager.LoadFonts(Config.GetFile(Config.Dir.Skin, m_strSkin, "fonts.xml"));
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Initializing fonts...");
    }
    GUIFontManager.InitializeDeviceObjects();
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading skin...");
    }
    Log.Info("Main: Loading {0} skin", m_strSkin);
    GUIWindowManager.Initialize();
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Loading window plugins...");
    }
    PluginManager.LoadWindowPlugins();
    Log.Info("Main: Loading windowmanager");
    if (splashScreen != null)
    {
      splashScreen.SetInformation("Initializing skin...");
    }
    Log.Info("Main: Resizing windowmanager");
    using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
    {
      _useLongDateFormat = xmlreader.GetValueAsBool("home", "LongTimeFormat", false);
      _startWithBasicHome = xmlreader.GetValueAsBool("general", "startbasichome", false);
      bool autosize = xmlreader.GetValueAsBool("general", "autosize", true);
      if (autosize && !GUIGraphicsContext.Fullscreen)
      {
        if (GUIGraphicsContext.currentScreen.Bounds.Width > GUIGraphicsContext.SkinSize.Width)
        {
          // Subtract window decorations, etc
          //int nettoHeightOffset = GUIGraphicsContext.currentScreen.Bounds.Height - GUIGraphicsContext.currentScreen.WorkingArea.Heigh t;
          //int nettoWidthOffset = GUIGraphicsContext.currentScreen.Bounds.Width - GUIGraphicsContext.currentScreen.WorkingArea.Width ;
          //Size = new Size(GUIGraphicsContext.SkinSize.Width + nettoWidthOffset, GUIGraphicsContext.SkinSize.Height + nettoHeightOffset);
          Size = new Size(GUIGraphicsContext.SkinSize.Width + 8, GUIGraphicsContext.SkinSize.Height + 54);
        }
        else
        {
          Size = new Size(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
        }
        if (GUIGraphicsContext.IsDirectX9ExUsed())
        {
          SwitchFullScreenOrWindowed(true);
          OnDeviceReset(null, null);
        }
      }
      else
      {
        GUIGraphicsContext.Load();
        GUIWindowManager.OnResize();
      }
    }
    Log.Info("Main: Initializing windowmanager");
    GUIWindowManager.PreInit();
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
    Log.Info("Main: Activating windowmanager");
    if ((_startWithBasicHome) && (File.Exists(GUIGraphicsContext.Skin + @"\basichome.xml")))
    {
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SECOND_HOME);
    }
    else
    {
      GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
    }
    Log.Info("Main: Initialized skin");
    if (GUIGraphicsContext.DX9Device != null)
    {
      Log.Info("Main: DX9 size: {0}x{1}", GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferWidth,
               GUIGraphicsContext.DX9Device.PresentationParameters.BackBufferHeight);
      Log.Info("Main: Video memory left: {0} MB", GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1048576);
      // consider the aperture size
    }
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
    WorkingSet.Minimize();
  }

  protected override void OnDeviceLost(object sender, EventArgs e)
  {
    Log.Warn("Main: ***** OnDeviceLost *****");
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
    base.OnDeviceLost(sender, e);
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
    // Only perform the device reset if we're not shutting down MediaPortal.
    if (GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.STOPPING)
    {
      Log.Info("Main: Resetting DX9 device");

      GUITextureManager.Dispose();
      GUIFontManager.Dispose();

      GUIGraphicsContext.DX9Device.EvictManagedResources();
      GUIWaitCursor.Dispose();
      if (!m_strSkin.Equals(GUIGraphicsContext.Skin))
      {
        m_strSkin = GUIGraphicsContext.Skin;
      }
      GUIFontManager.LoadFonts(Config.GetFile(Config.Dir.Skin, m_strSkin, "fonts.xml"));
      GUIFontManager.InitializeDeviceObjects();
      if (GUIGraphicsContext.DX9Device != null)
      {
        GUIWindowManager.OnResize();
        GUIGraphicsContext.Load();
        GUIWindowManager.PreInit();
        GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
        GUIWindowManager.OnDeviceRestored();
      }
      // Must set the FVF after reset
      GUIFontManager.SetDevice();
      Log.Info("Main: Resetting DX9 device done");
    }
  }

  #endregion

  #region Render()

  private static bool reentrant = false;

  protected override void Render(float timePassed)
  {
    if (_suspended)
    {
      return;
    }
    if (reentrant)
    {
      Log.Info("Main: DX9 re-entrant"); //remove
      return;
    }
    if (GUIGraphicsContext.InVmr9Render)
    {
      Log.Error("Main: MediaPortal.Render() called while VMR9 render - {0} / {1}",
                GUIGraphicsContext.Vmr9Active, GUIGraphicsContext.Vmr9FPS);
      return;
    }
    if (GUIGraphicsContext.Vmr9Active)
    {
      Log.Error("Main: MediaPortal.Render() called while VMR9 active");
      return;
    }
    if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
    {
      return;
    }
    try
    {
      reentrant = true;
      // if there's no DX9 device (during resizing for exmaple) then just return
      if (GUIGraphicsContext.DX9Device == null)
      {
        reentrant = false;
        //Log.Info("dx9 device=null");//remove
        return;
      }
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
      {
        reentrant = false;
        //Log.Info("dx9 state=lost");//remove
        return;
      }
      ++frames;
      // clear the surface
      GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
      GUIGraphicsContext.DX9Device.BeginScene();
      CreateStateBlock();
      GUIGraphicsContext.SetScalingResolution(0, 0, false);
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
        Log.Error("Main: Device lost - {0}", ex.ToString());
        if (!RefreshRateChanger.RefreshRateChangePending)
        {
          g_Player.Stop();
        }
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
      }
    }
    catch (DirectXException dex)
    {
      if (dex.ErrorCode == -2005530508 || // GPU_HUNG
        dex.ErrorCode == -2005530512) // GPU_REMOVED
      {
        Log.Info("Main: GPU_HUNG - {0}", dex.ToString());
        if (!RefreshRateChanger.RefreshRateChangePending)
        {
          g_Player.Stop();
        }
        GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
      }
      else
      {
        Log.Error(dex);
      }
    }
    catch (Exception ex)
    {
      Log.Error(ex);
    }
    finally
    {
      //Log.Info("app:render() done");
      reentrant = false;
    }
  }

  #endregion

  #region OnProcess()

  protected override void OnProcess()
  {
    // Set the date & time
    if (DateTime.Now.Second != m_updateTimer.Second)
    {
      m_updateTimer = DateTime.Now;
      GUIPropertyManager.SetProperty("#date", GetDate());
      GUIPropertyManager.SetProperty("#time", GetTime());
    }
#if AUTOUPDATE
    CheckForNewUpdate();
#endif
    g_Player.Process();
    // update playing status
    if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
    {
      RecoverDevice();
    }

    if (g_Player.Playing)
    {
      if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
      {
        GUIGraphicsContext.IsFullScreenVideo = true;
        /*if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.LOST)
        {
          RecoverDevice();
        }*/
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
        GUIPropertyManager.SetProperty("#shortcurrentplaytime",
                                       GUIPropertyManager.GetProperty("#TV.Record.current"));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentplaytime",
                                       Utils.SecondsToHMSString((int)g_Player.CurrentPosition));
        GUIPropertyManager.SetProperty("#currentremaining",
                                       Utils.SecondsToHMSString(
                                         (int)(g_Player.Duration - g_Player.CurrentPosition)));
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
        GUIPropertyManager.SetProperty("#duration", string.Empty);
        GUIPropertyManager.SetProperty("#shortduration", string.Empty);
        GUIPropertyManager.SetProperty("#percentage", "0");
      }
      GUIPropertyManager.SetProperty("#playspeed", g_Player.Speed.ToString());
    }
    else
    {
      GUIGraphicsContext.IsPlaying = false;
    }
    if (!g_Player.Playing)
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
    if (_suspended)
    {
      return;
    } //we are suspended/hibernated
#if !DEBUG
    try
#endif
    {
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      {
        Log.Info("Main: Stopping FrameMove");
        Close();
      }
      try
      {
        GUIWindowManager.DispatchThreadMessages();
        GUIWindowManager.ProcessWindows();
      }
      catch (FileNotFoundException ex)
      {
        Log.Error(ex);
        MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
        Close();
      }
      if (useScreenSaver)
      {
        if ((GUIGraphicsContext.IsFullScreenVideo && g_Player.Paused == false) ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW)
        {
          GUIGraphicsContext.ResetLastActivity();
        }
        if (!GUIGraphicsContext.BlankScreen)
        {
          if (isMaximized)
          {
            int window = GUIWindowManager.ActiveWindow;
            if (window < (int)GUIWindow.Window.WINDOW_WIZARD_WELCOME ||
                window > (int)GUIWindow.Window.WINDOW_WIZARD_FINISHED)
            {
              TimeSpan ts = DateTime.Now - GUIGraphicsContext.LastActivity;
              if (ts.TotalSeconds >= timeScreenSaver)
              {
                if (useIdleblankScreen)
                {
                  if (!GUIGraphicsContext.BlankScreen)
                  {
                    Log.Debug("Main: Idle timer is blanking the screen after {0} seconds of inactivity", ts.TotalSeconds.ToString("n0"));
                  }
                  GUIGraphicsContext.BlankScreen = true;
                }
                else
                {
                  // Slower rendering will have an impact on scrolling labels or list items
                  // As long as we're e.g. listening to music on "Playing Now" screen 
                  // we might not want to slow things down here.
                  // This feature is mainly intended to save energy on idle 24/7 rigs.
                  if (GUIWindowManager.ActiveWindow != (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
                  {
                    if (!GUIGraphicsContext.SaveRenderCycles)
                    {
                      Log.Debug("Main: Idle timer is entering power save mode after {0} seconds of inactivity", ts.TotalSeconds.ToString("n0"));
                    }
                    GUIGraphicsContext.SaveRenderCycles = true;
                  }
                }
              }
            }
            else
            {
              GUIGraphicsContext.ResetLastActivity();
            }
          }
        }
      }
      //if (GUIGraphicsContext.UseSeparateRenderThread)
      //{
      //  HandleCursor();
      //}
    }
#if !DEBUG
    catch (Exception ex)
    {
      Log.Error(ex);
    }
#endif
  }

  #endregion

  #region Handle messages, keypresses, mouse moves etc

  private void OnAction(Action action)
  {
    try
    {
      // hack/fix for lastactivemodulefullscreen
      // when recovering from hibernation/standby after closing with remote control somehow a F9 (keycode 120) onkeydown event is thrown from outside
      // we are currently filtering it away.
      // sometimes more than one F9 keydown event fired.
      // if these events are not filtered away the F9 context menu is shown on the restored/shown module.
      if ((action.wID == Action.ActionType.ACTION_CONTEXT_MENU || _suspended) && (showLastActiveModule))
      {
        //Log.Info("ACTION_CONTEXT_MENU, ignored = {0}, suspended = {1}", ignoreContextMenuAction, _suspended);      
        if (ignoreContextMenuAction)
        {
          ignoreContextMenuAction = false;
          lastContextMenuAction = DateTime.Now;
          return;
        }
        else if (lastContextMenuAction != DateTime.MaxValue)
        {
          TimeSpan ts = lastContextMenuAction - DateTime.Now;
          if (ts.TotalMilliseconds > -100)
          {
            ignoreContextMenuAction = false;
            lastContextMenuAction = DateTime.Now;
            return;
          }
        }
        lastContextMenuAction = DateTime.Now;
      }

      GUIWindow window;
      if (action.IsUserAction())
      {
        GUIGraphicsContext.ResetLastActivity();
      }
      switch (action.wID)
      {
        // record current tv program
        case Action.ActionType.ACTION_RECORD:
          if ((GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_TVGUIDE) &&
              (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_DIALOG_TVGUIDE))
          {
            GUIWindow tvHome = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            if (tvHome != null)
            {
              if (tvHome.GetID != GUIWindowManager.ActiveWindow)
              {
                tvHome.OnAction(action);
                return;
              }
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
          if (g_Player.Playing && (g_Player.IsDVD || g_Player.HasChapters))
          {
            g_Player.OnAction(action);
            return;
          }
          break;

        //DVD: goto previous chapter
        case Action.ActionType.ACTION_PREV_CHAPTER:
          if (g_Player.Playing && (g_Player.IsDVD || g_Player.HasChapters))
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
        case Action.ActionType.ACTION_TOGGLE_WINDOWED_FULLSCREEN:
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
            Log.Info("Main: Using live TV as background");
            // if on, but we're not playing any video or watching tv
            if (GUIGraphicsContext.Vmr9Active)
            {
              GUIGraphicsContext.ShowBackground = false;
              GUIGraphicsContext.Overlay = false;
            }
            else
            {
              //show warning message
              GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0);
              msg.Param1 = 727; //Live tv in background
              msg.Param2 = 728; //No Video/TV playing
              msg.Param3 = 729; //Make sure that something is playing
              GUIWindowManager.SendMessage(msg);
              return;
            }
          }
          else
          {
            Log.Info("Main: Using GUI as background");
            GUIGraphicsContext.ShowBackground = true;
            GUIGraphicsContext.Overlay = true;
          }
          return;

        //switch between several home windows
        case Action.ActionType.ACTION_SWITCH_HOME:
          GUIMessage homeMsg;
          if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_HOME)
          {
            homeMsg =
              new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                             (int)GUIWindow.Window.WINDOW_SECOND_HOME, 0, null);
          }
          else if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SECOND_HOME)
          {
            homeMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                                     (int)GUIWindow.Window.WINDOW_HOME, 0, null);
          }
          else
          {
            if (_startWithBasicHome)
            {
              homeMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                                       (int)GUIWindow.Window.WINDOW_SECOND_HOME, 0, null);
            }
            else
            {
              homeMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0,
                                       (int)GUIWindow.Window.WINDOW_HOME, 0, null);
            }
          }
          GUIWindowManager.SendThreadMessage(homeMsg);
          return;

        //exit mediaportal
        case Action.ActionType.ACTION_EXIT:
          Log.Info("Main: Exit requested");
          // is the minimize on gui option set?  If so, minimize to tray...
          if (_minimizeOnGuiExit && !_shuttingDown)
          {
            if (WindowState != FormWindowState.Minimized)
            {
              Log.Info("Main: Minimizing to tray on GUI exit and restoring taskbar");
            }
            WindowState = FormWindowState.Minimized;
            Hide();
            if (autoHideTaskbar)
            {
              // only re-show the startbar if MP is the one that has hidden it.
              Win32API.EnableStartBar(true);
              Win32API.ShowStartBar(true);
            }
            if (g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD)
            {
              if (g_Player.Volume > 0)
              {
                m_iVolume = g_Player.Volume;
                g_Player.Volume = 0;
              }
              if (g_Player.Paused == false && !GUIGraphicsContext.IsVMR9Exclusive)
              {
                g_Player.Pause();
              }
            }
            return;
          }
          GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
          return;

        case Action.ActionType.ACTION_MPRESTORE:
          {
            Log.Info("Main: Restore MP by action");
            Restore();
            if ((g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD) && m_iVolume > 0)
            {
              g_Player.Volume = m_iVolume;
              g_Player.ContinueGraph();
              if (g_Player.Paused && !GUIGraphicsContext.IsVMR9Exclusive)
              {
                g_Player.Pause();
              }
            }
          }
          return;

        //reboot pc
        case Action.ActionType.ACTION_REBOOT:
          {
            //reboot
            Log.Info("Main: Reboot requested");
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
            msg.Param1 = 630;
            msg.Param2 = 0;
            msg.Param3 = 0;
            GUIWindowManager.SendMessage(msg);
            if (msg.Param1 == 1)
            {
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
            Log.Info("Main: Shutdown dialog");
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg != null)
            {
              dlg.Reset();
              dlg.SetHeading(GUILocalizeStrings.Get(498)); //Menu
              dlg.AddLocalizedString(1030); //PowerOff
              dlg.AddLocalizedString(1031); //Reboot
              dlg.AddLocalizedString(1032); //Suspend
              dlg.AddLocalizedString(1049); //Hibernate
              dlg.DoModal(GUIWindowManager.ActiveWindow);
              //RestartOptions option = RestartOptions.Suspend;
              if (dlg.SelectedId < 0)
              {
                GUIWindow win = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_HOME);
                if (win != null)
                {
                  win.OnAction(new Action(Action.ActionType.ACTION_MOVE_LEFT, 0, 0));
                }
                /* GUIOverlayWindow topBar =
                     (GUIOverlayWindow)
                     GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TOPBARHOME);
                 if (topBar != null)
                 {
                   topBar.Focused = true;
                 }*/
                return;
              }
              switch (dlg.SelectedId)
              {
                case 1030:
                  restartOptions = RestartOptions.PowerOff;
                  useRestartOptions = true;
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  base._shuttingDown = true;
                  break;

                case 1031:
                  restartOptions = RestartOptions.Reboot;
                  useRestartOptions = true;
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  base._shuttingDown = true;
                  break;

                case 1032:
                  restartOptions = RestartOptions.Suspend;
                  Utils.SuspendSystem(false);
                  break;

                case 1049:
                  restartOptions = RestartOptions.Hibernate;
                  Utils.HibernateSystem(false);
                  break;
              }
            }
            break;
          }

        //stop radio
        case Action.ActionType.ACTION_STOP:
          break;

        // Take Screenshot
        case Action.ActionType.ACTION_TAKE_SCREENSHOT:
          {
            try
            {
              string directory =
                string.Format("{0}\\MediaPortal Screenshots\\{1:0000}-{2:00}-{3:00}",
                              Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                              DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
              if (!Directory.Exists(directory))
              {
                Log.Info("Main: Taking screenshot - Creating directory: {0}", directory);
                Directory.CreateDirectory(directory);
              }

              string fileName =
                string.Format("{0}\\{1:00}-{2:00}-{3:00}", directory, DateTime.Now.Hour,
                              DateTime.Now.Minute, DateTime.Now.Second);
              Log.Info("Main: Taking screenshot - Target: {0}.png", fileName);
              Surface backbuffer = GUIGraphicsContext.DX9Device.GetBackBuffer(0, 0, BackBufferType.Mono);
              SurfaceLoader.Save(fileName + ".png", ImageFileFormat.Png, backbuffer);
              backbuffer.Dispose();
              Log.Info("Main: Taking screenshot done");
            }
            catch (Exception ex)
            {
              Log.Info("Main: Error taking screenshot: {0}", ex.Message);
            }
          }
          break;

        case Action.ActionType.ACTION_SHOW_GUI:
          {
            // can we handle the switch to fullscreen?
            if (!GUIGraphicsContext.IsFullScreenVideo && g_Player.ShowFullScreenWindow())
            {
              return;
            }
          }
          break;
      }
      if (g_Player.Playing)
      {
        switch (action.wID)
        {
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
              Log.Info("Main: Stopping media");
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
              //SV Fixes Music player seek with new Bass Audio Engine
              if (g_Player.IsMusic && BassMusicPlayer.IsDefaultMusicPlayer)
              {
                break;
              }

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
              //SV Fixes Music player seek with new Bass Audio Engine
              if (g_Player.IsMusic && BassMusicPlayer.IsDefaultMusicPlayer)
              {
                break;
              }

              if (!g_Player.IsTV)
              {
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
      Log.Error(ex);
      MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Close();
    }
    catch (Exception ex)
    {
      Log.Error(ex);
      Log.Error("  exception: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
#if !DEBUG
      throw new Exception("exception occured", ex);
#endif
    }
  }

  #region keypress handlers

  protected override void keypressed(KeyPressEventArgs e)
  {
    GUIGraphicsContext.BlankScreen = false;
    // Log.Info("key:{0} 0x{1:X} (2)", (int)keyc, (int)keyc, keyc);
    Key key = new Key(e.KeyChar, 0);
    Action action = new Action();
    if (GUIWindowManager.IsRouted || GUIWindowManager.ActiveWindowEx == (int)GUIWindow.Window.WINDOW_TV_SEARCH)
    // is a dialog open or maybe the tv schedule search (GUISMSInputControl)?
    {
      GUIGraphicsContext.ResetLastActivity();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_VIRTUAL_SMS_KEYBOARD) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_MSN_CHAT) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_MSNOSD) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_TVMSNOSD) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_TV_SEARCH))
      {
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
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
    if (key.KeyChar == '|' && g_Player.Playing == false)
    {
      g_Player.Play("rtsp://localhost/stream0");
      g_Player.ShowFullScreenWindow();
      return;
    }
    if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
    {
      if (action.ShouldDisableScreenSaver)
      {
        GUIGraphicsContext.ResetLastActivity();
      }
      if (action.SoundFileName.Length > 0 && !g_Player.Playing)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
    else
    {
      GUIGraphicsContext.ResetLastActivity();
    }
    action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
    GUIGraphicsContext.OnAction(action);
  }

  protected override void keydown(KeyEventArgs e)
  {
    if (_suspended)
    {
      return;
    }
    GUIGraphicsContext.ResetLastActivity();
    Key key = new Key(0, (int)e.KeyCode);
    Action action = new Action();
    if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
    {
      if (action.SoundFileName.Length > 0 && !g_Player.Playing)
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
      GUIGraphicsContext.ResetLastActivity();
    }
    else if (e.Delta < 0)
    {
      Action action = new Action(Action.ActionType.ACTION_MOVE_DOWN, x, y);
      action.MouseButton = e.Button;
      GUIGraphicsContext.OnAction(action);
      GUIGraphicsContext.ResetLastActivity();
    }
    base.OnMouseWheel(e);
  }

  protected override void mousemove(MouseEventArgs e)
  {
    // Disable first mouse action when mouse was hidden
    base.mousemove(e);
    if (!_showCursor)
    {
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
        GUIGraphicsContext.ResetLastActivity();
      }
      //check any still waiting single click events
      if (GUIGraphicsContext.DBLClickAsRightClick && bMouseClickFired)
      {
        if ((Math.Abs(m_iLastMousePositionX - iCursorX) > 10) ||
            (Math.Abs(m_iLastMousePositionY - iCursorY) > 10))
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

  protected override void mousedoubleclick(MouseEventArgs e)
  {
    if ((GUIGraphicsContext.DBLClickAsRightClick))
    {
      return;
    }
    GUIGraphicsContext.ResetLastActivity();
    // Disable first mouse action when mouse was hidden
    if (!_showCursor)
    {
      base.mouseclick(e);
      return;
    }
    Action actionMove;
    Action action;
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
    // Save last position
    m_iLastMousePositionX = iCursorX;
    m_iLastMousePositionY = iCursorY;
    // Send move moved action
    actionMove = new Action(Action.ActionType.ACTION_MOUSE_MOVE, x, y);
    GUIGraphicsContext.OnAction(actionMove);
    action = new Action(Action.ActionType.ACTION_MOUSE_DOUBLECLICK, x, y);
    action.MouseButton = e.Button;
    action.SoundFileName = "click.wav";
    if (action.SoundFileName.Length > 0 && !g_Player.Playing)
    {
      Utils.PlaySound(action.SoundFileName, false, true);
    }
    GUIGraphicsContext.OnAction(action);
    return;
  }

  protected override void mouseclick(MouseEventArgs e)
  {
    GUIGraphicsContext.ResetLastActivity();
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
    //;
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
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
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
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
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
          if (action.SoundFileName.Length > 0 && !g_Player.Playing)
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
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
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
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }
        GUIGraphicsContext.OnAction(action);
      }
    }
  }

  protected override void Restore_OnClick(Object sender, EventArgs e)
  {
    if (m_iVolume > 0 && (g_Player.IsVideo || g_Player.IsTV))
    {
      g_Player.Volume = m_iVolume;
      if (g_Player.Paused)
      {
        g_Player.Pause();
      }
    }
    Restore();
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
 
  private void OnUpdaterDownloadStartedHandler(object sender, UpdaterActionEventArgs e) 
  {		
    Log.Info("Main: Update - Download started for: {0}",e.ApplicationName);
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
      Log.Info("Main: Update - User canceled download");
      m_bCancelVersion = true;
      m_bNewVersionAvailable = false;
      return;
    }
    m_bCancelVersion = false;
    m_bNewVersionAvailable = false;
  }

  private void OnUpdaterUpdateAvailable(object sender, UpdaterActionEventArgs e)
  {
    Log.Info("Main: Update - New version available: {0}", e.ApplicationName);
    m_strNewVersion = e.ServerInformation.AvailableVersion;
    m_bNewVersionAvailable = true;
    while (m_bNewVersionAvailable) System.Threading.Thread.Sleep(100);
    if (m_bCancelVersion)
    {
      _updater.StopUpdater(e.ApplicationName);
    }
  }

  private void OnUpdaterDownloadCompletedHandler(object sender, UpdaterActionEventArgs e)
  {
    Log.Info("Main: Update - Download completed");
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

  private void StartNewVersion()
  {
    Log.Info("Main: Update - Starting appstart.exe");
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
    Log.Info("Main: Update - Stopping MP");
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
    if (_suspended)
    {
      return;
    }
    switch (message.Message)
    {
      case GUIMessage.MessageType.GUI_MSG_RESTART_REMOTE_CONTROLS:
        Log.Info("Main: Restart remote controls");
        InputDevices.Stop();
        InputDevices.Init();
        break;

      case GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW:
        GUIWindowManager.ActivateWindow(message.Param1);
        if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
            GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC)
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
        bIsInteger =
          Double.TryParse(message.Label, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
        try
        {
          if (bIsInteger)
          {
            usbuirtdevice.ChangeTunerChannel(message.Label);
          }
        }
        catch (Exception)
        {
        }
        try
        {
          winlircdevice.ChangeTunerChannel(message.Label);
        }
        catch (Exception)
        {
        }
        try
        {
          if (bIsInteger)
          {
            redeyedevice.ChangeTunerChannel(message.Label);
          }
        }
        catch (Exception)
        {
        }
        break;

      case GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED:
        if (GUIGraphicsContext.IsDirectX9ExUsed() && useEnhancedVideoRenderer)
        {
          return;
        }
        bool fullscreen = (message.Param1 != 0);
        Log.Debug("Main: Received DX exclusive mode switch message. Fullscreen && maximized == {0}",
                 fullscreen && isMaximized);
        if (isMaximized == false || GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
        {
          return;
        }
        if (fullscreen)
        {
          //switch to fullscreen mode
          Log.Debug("Main: Goto fullscreen: {0}", GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
          if (GUIGraphicsContext.DX9Device.PresentationParameters.Windowed)
          {
            SwitchFullScreenOrWindowed(false);
          }
        }
        else
        {
          //switch to windowed mode
          Log.Debug("Main: Goto windowed mode: {0}",
                    GUIGraphicsContext.DX9Device.PresentationParameters.Windowed);
          if (!GUIGraphicsContext.DX9Device.PresentationParameters.Windowed)
          {
            SwitchFullScreenOrWindowed(true);
          }
        }
        // Must set the FVF after reset
        GUIFontManager.SetDevice();
        break;

      case GUIMessage.MessageType.GUI_MSG_GETFOCUS:
        Log.Debug("Main: Setting focus");
        if (WindowState == FormWindowState.Minimized)
        {
          if (m_iVolume > 0 && (g_Player.IsVideo || g_Player.IsTV))
          {
            g_Player.Volume = m_iVolume;
            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
          }
          Restore();
        }
        else
        {
          Activate();
        }
        //Force.SetForegroundWindow(this.Handle, true);
        break;

      case GUIMessage.MessageType.GUI_MSG_CODEC_MISSING:
        GUIDialogOK dlgOk = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        dlgOk.SetHeading(string.Empty);
        dlgOk.SetLine(1, message.Label);
        dlgOk.SetLine(2, string.Empty);
        dlgOk.SetLine(3, message.Label2);
        dlgOk.SetLine(4, message.Label3);
        dlgOk.DoModal(GUIWindowManager.ActiveWindow);
        break;

      case GUIMessage.MessageType.GUI_MSG_REFRESHRATE_CHANGED:

        GUIDialogNotify dlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
        if (dlgNotify != null)
        {
          dlgNotify.Reset();
          dlgNotify.ClearAll();
          dlgNotify.SetHeading(message.Label);
          dlgNotify.SetText(message.Label2);
          dlgNotify.TimeOut = message.Param1;
          dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
        }

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
    GUIGraphicsContext.DX9Device.RenderState.CullMode = Cull.None;
    GUIGraphicsContext.DX9Device.RenderState.Lighting = false;
    GUIGraphicsContext.DX9Device.RenderState.ZBufferEnable = true;
    GUIGraphicsContext.DX9Device.RenderState.FogEnable = false;
    GUIGraphicsContext.DX9Device.RenderState.FillMode = FillMode.Solid;
    GUIGraphicsContext.DX9Device.RenderState.AlphaBlendEnable = true;
    GUIGraphicsContext.DX9Device.RenderState.SourceBlend = Blend.SourceAlpha;
    GUIGraphicsContext.DX9Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorOperation = TextureOperation.Modulate;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
    GUIGraphicsContext.DX9Device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
    GUIGraphicsContext.DX9Device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;
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
    return;
  }

  /// <summary>
  /// Get the current date from the system and localize it based on the user preferences.
  /// </summary>
  /// <returns>A string containing the localized version of the date.</returns>
  protected string GetDate()
  {
    string dateString = _dateFormat;
    if ((dateString == null) || (dateString.Length == 0))
    {
      return string.Empty;
    }
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
    dateString = Utils.ReplaceTag(dateString, "<Day>", day, "unknown");
    dateString = Utils.ReplaceTag(dateString, "<DD>", cur.Day.ToString(), "unknown");
    dateString = Utils.ReplaceTag(dateString, "<Month>", month, "unknown");
    dateString = Utils.ReplaceTag(dateString, "<MM>", cur.Month.ToString(), "unknown");
    dateString = Utils.ReplaceTag(dateString, "<Year>", cur.Year.ToString(), "unknown");
    dateString = Utils.ReplaceTag(dateString, "<YY>", (cur.Year - 2000).ToString("00"), "unknown");
    GUIPropertyManager.SetProperty("#date", dateString);
    return dateString;
  }

  /// <summary>
  /// Get the current time from the system. Set the format in the Home plugin's config
  /// </summary>
  /// <returns>A string containing the current time.</returns>
  protected string GetTime()
  {
    if (_useLongDateFormat)
    {
      return DateTime.Now.ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern);
    }
    else
    {
      return DateTime.Now.ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern);
    }
  }

  protected string GetDay()
  {
    DateTime cur = DateTime.Now;
    return String.Format("{0}", cur.Day);
  }

  protected string GetShortDayOfWeek()
  {
    DateTime cur = DateTime.Now;
    string ddd;
    switch (cur.DayOfWeek)
    {
      case DayOfWeek.Monday:
        ddd = GUILocalizeStrings.Get(657);
        break;
      case DayOfWeek.Tuesday:
        ddd = GUILocalizeStrings.Get(658);
        break;
      case DayOfWeek.Wednesday:
        ddd = GUILocalizeStrings.Get(659);
        break;
      case DayOfWeek.Thursday:
        ddd = GUILocalizeStrings.Get(660);
        break;
      case DayOfWeek.Friday:
        ddd = GUILocalizeStrings.Get(661);
        break;
      case DayOfWeek.Saturday:
        ddd = GUILocalizeStrings.Get(662);
        break;
      default:
        ddd = GUILocalizeStrings.Get(663);
        break;
    }
    return ddd;
  }

  protected string GetDayOfWeek()
  {
    DateTime cur = DateTime.Now;
    string dddd;
    switch (cur.DayOfWeek)
    {
      case DayOfWeek.Monday:
        dddd = GUILocalizeStrings.Get(11);
        break;
      case DayOfWeek.Tuesday:
        dddd = GUILocalizeStrings.Get(12);
        break;
      case DayOfWeek.Wednesday:
        dddd = GUILocalizeStrings.Get(13);
        break;
      case DayOfWeek.Thursday:
        dddd = GUILocalizeStrings.Get(14);
        break;
      case DayOfWeek.Friday:
        dddd = GUILocalizeStrings.Get(15);
        break;
      case DayOfWeek.Saturday:
        dddd = GUILocalizeStrings.Get(16);
        break;
      default:
        dddd = GUILocalizeStrings.Get(17);
        break;
    }
    return dddd;
  }

  protected string GetMonth()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("MM");
  }

  protected string GetShortMonthOfYear()
  {
    DateTime cur = DateTime.Now;
    string SMOY = GetMonthOfYear();
    SMOY = SMOY.Substring(0, 3);
    return SMOY;
  }

  protected string GetMonthOfYear()
  {
    DateTime cur = DateTime.Now;
    string MMMM;
    switch (cur.Month)
    {
      case 1:
        MMMM = GUILocalizeStrings.Get(21);
        break;
      case 2:
        MMMM = GUILocalizeStrings.Get(22);
        break;
      case 3:
        MMMM = GUILocalizeStrings.Get(23);
        break;
      case 4:
        MMMM = GUILocalizeStrings.Get(24);
        break;
      case 5:
        MMMM = GUILocalizeStrings.Get(25);
        break;
      case 6:
        MMMM = GUILocalizeStrings.Get(26);
        break;
      case 7:
        MMMM = GUILocalizeStrings.Get(27);
        break;
      case 8:
        MMMM = GUILocalizeStrings.Get(28);
        break;
      case 9:
        MMMM = GUILocalizeStrings.Get(29);
        break;
      case 10:
        MMMM = GUILocalizeStrings.Get(30);
        break;
      case 11:
        MMMM = GUILocalizeStrings.Get(31);
        break;
      default:
        MMMM = GUILocalizeStrings.Get(32);
        break;
    }
    return MMMM;
  }

  protected string GetShortYear()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("yy");
  }

  protected string GetYear()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("yyyy");
  }

  protected void CheckSkinVersion()
  {
    OldSkinForm form = new OldSkinForm();
    if (form.CheckSkinVersion(m_strSkin))
    {
      return;
    }
#if !DEBUG
    if (splashScreen != null)
    {
      splashScreen.AllowWindowOverlay((Form)form);
    }
#endif
    form.ShowDialog(this);
  }

  #region registry helper function

  public static void SetDWORDRegKey(RegistryKey hklm, string Key, string Value, Int32 iValue)
  {
    try
    {
      using (RegistryKey subkey = hklm.CreateSubKey(Key))
      {
        if (subkey != null)
        {
          subkey.SetValue(Value, iValue);
        }
      }
    }
    catch (SecurityException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", Key);
    }
    catch (UnauthorizedAccessException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", Key);
    }
  }

  public static void SetREGSZRegKey(RegistryKey hklm, string Key, string Name, string Value)
  {
    try
    {
      using (RegistryKey subkey = hklm.CreateSubKey(Key))
      {
        if (subkey != null)
        {
          subkey.SetValue(Name, Value);
        }
      }
    }
    catch (SecurityException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", Key);
    }
    catch (UnauthorizedAccessException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", Key);
    }
  }

  #endregion

  #endregion

  private void DoStartupJobs()
  {
    FilterChecker.CheckInstalledVersions();

    Version aParamVersion;
    //
    // 6.5.2600.3243 = KB941568,   6.5.2600.3024 = KB927544
    //
    if (
      !FilterChecker.CheckFileVersion(Environment.SystemDirectory + "\\quartz.dll", "6.5.2600.3024", out aParamVersion))
    {
      string ErrorMsg =
        string.Format("Your version {0} of quartz.dll has too many bugs! \nPlease check our Wiki's requirements page.",
                      aParamVersion.ToString());
      Log.Info("Util: quartz.dll error - {0}", ErrorMsg);
      if (
        MessageBox.Show(ErrorMsg, "Core directshow component (quartz.dll) is outdated!", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Exclamation) == DialogResult.OK)
      {
        Process.Start(@"http://wiki.team-mediaportal.com/GeneralRequirements");
      }
    }

    // Stop MCE services
    Utils.StopMCEServices();
    EnableS3Trick();
    GUIWindowManager.OnNewAction += new OnActionHandler(OnAction);
    GUIWindowManager.Receivers += new SendMessageHandler(OnMessage);
    GUIWindowManager.Callbacks += new GUIWindowManager.OnCallBackHandler(MPProcess);
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STARTING;
    Utils.OnStartExternal += new Utils.UtilEventHandler(OnStartExternal);
    Utils.OnStopExternal += new Utils.UtilEventHandler(OnStopExternal);
    // load keymapping from keymap.xml
    ActionTranslator.Load();
    //register the playlistplayer for thread messages (like playback stopped,ended)
    Log.Info("Main: Init playlist player");
    g_Player.Factory = new PlayerFactory();
    playlistPlayer.Init();
    // Only load the USBUIRT device if it has been enabled in the configuration
    using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
    {
      bool inputEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false);
      bool outputEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false);
      if (inputEnabled || outputEnabled)
      {
        Log.Info("Main: Creating the USBUIRT device");
        usbuirtdevice = USBUIRT.Create(new USBUIRT.OnRemoteCommand(OnRemoteCommand));
        Log.Info("Main: Creating the USBUIRT device done");
      }
      //Load Winlirc if enabled.
      bool winlircInputEnabled = xmlreader.GetValueAsString("WINLIRC", "enabled", "false") == "true";
      if (winlircInputEnabled)
      {
        Log.Info("Main: Creating the WINLIRC device");
        winlircdevice = new WinLirc();
        Log.Info("Main: Creating the WINLIRC device done");
      }
      //Load RedEye if enabled.
      bool redeyeInputEnabled = xmlreader.GetValueAsString("RedEye", "internal", "false") == "true";
      if (redeyeInputEnabled)
      {
        Log.Info("Main: Creating the REDEYE device");
        redeyedevice = RedEye.Create(new RedEye.OnRemoteCommand(OnRemoteCommand));
        Log.Info("Main: Creating the RedEye device done");
      }
      inputEnabled = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";
      if (inputEnabled)
      {
        Log.Info("Main: Creating the SerialUIR device");
        serialuirdevice = SerialUIR.Create(new SerialUIR.OnRemoteCommand(OnRemoteCommand));
        Log.Info("Main: Creating the SerialUIR device done");
      }
    }
    //registers the player for video window size notifications
    Log.Info("Main: Init players");
    g_Player.Init();
    GUIGraphicsContext.ActiveForm = Handle;
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
      doc.Save("MediaPortal.exe.config");
    }
    catch (Exception)
    {
    }
    Thumbs.CreateFolders();
    try
    {
#if DEBUG
#else
#if AUTOUPDATE
      UpdaterConfiguration config = UpdaterConfiguration.Instance;
      config.Logging.LogPath = Config.Get(Config.Dir.Log) + "updatelog.log";
      config.Applications[0].Client.BaseDir = Config.Get(Config.Dir.Base)
      config.Applications[0].Client.TempDir =  Config.Get(Config.Dir.Base) + "temp";
      config.Applications[0].Client.XmlFile =  Config.Get(Config.Dir.Base) + "MediaPortal.exe.config";
      config.Applications[0].Server.ServerManifestFileDestination =  Config.Get(Config.Dir.Base) + @"xml\ServerManifest.xml";
      try
      {
        System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"temp");
				System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"xml");
				System.IO.Directory.CreateDirectory(config.Applications[0].Client.BaseDir + @"log");
			}
			catch(Exception){}
			Utils.DeleteFiles(config.Applications[0].Client.BaseDir + "log", "*.log");
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
    {
    }
    using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
    {
      m_iDateLayout = xmlreader.GetValueAsInt("home", "datelayout", 0);
    }
    GUIGraphicsContext.ResetLastActivity();
  }

  /// <summary>
  /// Enables the S3 system power state for standby when USB devices are armed for wake, 
  /// if this option is enabled in the configuration.
  /// </summary>
  /// <remarks>
  /// <para>The trick is to create the following registry value: 
  /// HKLM\SYSTEM\CurrentControlSet\Services\usb\USBBIOSx and set it to 0.</para>
  /// <para>This method checks whether the <b>enables3trick</b> option in the <b>general</b>
  /// section of the mediaportal.xml file is enabled (which is the default), before setting
  /// the value.  The reason for this configuration option is a bug in the S3 implementation
  /// of various (Asus) motherboards like the A7NX8 and A7VX8 that causes the computer to
  /// reboot immediately after hibernating.</para>
  /// <para>The previous implementation of this method also created an USBBIOSHACKS value,
  /// which according to this article http://support.microsoft.com/kb/841858/en-us should
  /// not be used.  That is why the current implementation deletes this key if it still
  /// exists.</para>
  /// </remarks>
  private static void EnableS3Trick()
  {
    try
    {
      using (RegistryKey services = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true))
      {
        RegistryKey usb = services.OpenSubKey("usb", true);
        if (usb == null)
        {
          usb = services.CreateSubKey("usb");
        }
        //Delete the USBBIOSHACKS value if it is still there.  See the remarks.
        if (usb.GetValue("USBBIOSHacks") != null)
        {
          usb.DeleteValue("USBBIOSHacks");
        }
        //Check the general.enables3trick configuration option and create/delete the USBBIOSx
        //value accordingly
        using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
        {
          bool enableS3Trick = xmlreader.GetValueAsBool("general", "enables3trick", true);
          if (enableS3Trick)
          {
            usb.SetValue("USBBIOSx", 0);
          }
          else
          {
            if (usb.GetValue("USBBIOSx") != null)
            {
              usb.DeleteValue("USBBIOSx");
            }
          }
        }
      }
    }
    catch (SecurityException)
    {
      Log.Info("Not enough permissions to enable/disable the S3 standby trick");
    }
    catch (UnauthorizedAccessException)
    {
      Log.Info("No write permissions to enable/disable the S3 standby trick");
    }
  }

  [Flags]
  public enum RegWow64Options : int
  {
    None = 0,
    KEY_WOW64_64KEY = 0x100,
    KEY_WOW64_32KEY = 0x200
  }

  [Flags]
  public enum RegistryRights : int
  {
    ReadKey = 131097,
    WriteKey = 131078
  }

  [Flags]
  public enum RegistryRoot : uint
  {
    HKCU = 0x80000001,
    HKLM = 0x80000002
  }

}
