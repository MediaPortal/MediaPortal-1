#region Copyright (C) 2005-2021 Team MediaPortal

// Copyright (C) 2005-2021 Team MediaPortal
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using DaggerLib.DSGraphEdit;
using DaggerLib.UI;

using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Util;
using MediaPortal.GUI.Library;

using WatchDog.Properties;

using Settings = MediaPortal.Profile.Settings;
using System.Runtime.InteropServices;

namespace WatchDog
{
  public partial class MPWatchDog : MPForm
  {
    #region Constants

    private const string Default4To3Skin = "Titan";
    private const string Default16To9Skin = "Titan";

    #endregion

    #region Variables

    private readonly string _tempDir = "";
    public static string _zipFile = "";
    private string _tempConfig;
    private bool _engage;
    private bool _autoMode;
    private bool _watchdog;
    private bool _restartMP;
    private readonly bool _restoreTaskbar;
    private int _cancelDelay = 10;
    private Process _processMP;
    private readonly List<string> _knownPids = new List<string>();
    private bool _safeMode;
    private int GraphsCreated { get; set; }
    private string _currentpath = Directory.GetCurrentDirectory();
    private string _watchdogtargetDir = "";
    private string _watchdogAppDir = "";
    private string _zipPath;
    private bool _TVEonly;

    #endregion

    #region Helper functions

    private void ShowUsage()
    {
      const string usageText = "\n" +
                               "Usage: MPWatchDog.exe [-auto] [-watchdog] [-zipFile <path+filename>] [-restartMP <delay in seconds>] \n" +
                               "\n" +
                               "auto     : Perform all actions automatically and start MediaPortal in between\n" +
                               "safe     : Only load built-in plugins and load default skin. Used with auto. \n" +
                               "watchdog : Used internally by MediaPortal to monitor MP\n" +
                               "zipFile  : full path and filename to the zip where all logfiles will be included\n" +
                               "restartMP: automatically collects all logs, saves them as zip to desktop, restarts MP and closes\n" +
                               "           the delay is the time in where you can cancel the operation\n" +
                               "\n";
      MessageBox.Show(usageText, Resources.MediaPortal_test_tool_usage, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SetStatus(string status)
    {
      statusBar.Text = string.Format("Status: {0}", status);
    }

    private void EnableChoice(bool enable)
    {
      SafeModeRadioButton.Enabled = enable;
      NormalModeRadioButton.Enabled = enable;
      ExportLogsRadioButton.Enabled = enable;
    }

    private string GetMPArguments()
    {
      _tempConfig = CreateTemporaryConfiguration();
      if (_safeMode)
      {
        return String.Format("/skin={0} /safelist=\"{1}\\BuiltInPlugins.xml\" /config=\"{2}\" /Debug /NoTheme",
                             GetScreenAspect() <= 1.5 ? Default4To3Skin : Default16To9Skin,
                             Application.StartupPath, _tempConfig);
      }
      return String.Format("/config=\"{0}\" /Debug /NoTheme", _tempConfig);
    }

    private string CreateTemporaryConfiguration()
    {
      string tempSettingsFilename = Path.Combine(_tempDir, "MediaPortalTemp.xml");

      // check if Mediaportal has been configured, if not start configuration.exe in wizard mode
      var fi = new FileInfo(MPSettings.ConfigPathName);
      if (!File.Exists(MPSettings.ConfigPathName) || (fi.Length < 10000))
      {
        MessageBox.Show(Resources.MediaPortal_has_never_been_configured, Resources.Configuration_not_found, MessageBoxButtons.OK, MessageBoxIcon.Error);
        try
        {
          var process = new Process
                          {
                            StartInfo = new ProcessStartInfo
                                          {
                                            FileName = Config.GetFile(Config.Dir.Base, "configuration.exe"),
                                            Arguments = @"/wizard"
                                          }
                          };
          process.Start();
          process.WaitForExit();
        }
        // ReSharper disable EmptyGeneralCatchClause
        catch { }
        // ReSharper restore EmptyGeneralCatchClause
      }

      try
      {
        File.Copy(MPSettings.ConfigPathName, tempSettingsFilename, true);
        using (var xmlreader = new Settings(tempSettingsFilename, false))
        {
          xmlreader.SetValue("general", "loglevel", 3);
        }
      }
      catch (Exception)
      {
        File.Delete(tempSettingsFilename);
        throw;
      }
      return tempSettingsFilename;
    }

    private static float GetScreenAspect()
    {
      int screenNumber = 0;
      using (Settings xmlreader = new MPSettings())
      {
        screenNumber = xmlreader.GetValueAsInt("screenselector", "screennumber", screenNumber);
      }
      if (screenNumber < 0 || screenNumber >= Screen.AllScreens.Length)
      {
        screenNumber = 0;
      }
      Screen mpScreen = Screen.AllScreens[screenNumber];
      return (float)mpScreen.Bounds.Width / mpScreen.Bounds.Height;
    }

    private string GetZipFilename()
    {
      _zipFile = tbZipFile.Text;
      return _zipFile
        .Replace("[date]", DateTime.Now.ToString("yy_MM_dd"))
        .Replace("[time]", DateTime.Now.ToString("HH_mm"));
    }

    #endregion

    public MPWatchDog()
    {
      // Read Watchdog setting from XML files
      _watchdogAppDir = Config.GetFile(Config.Dir.Config, "watchdog.xml");

      using (Settings xmlreader = new Settings(_watchdogAppDir, false))
      {
        _watchdogtargetDir = xmlreader.GetValueAsString("general", "watchdogTargetDir", "");
      }

      GraphsCreated = 0;
      Thread.CurrentThread.Name = "MPWatchDog";
      InitializeComponent();
      _tempDir = Path.GetTempPath();
      if (!_tempDir.EndsWith("\\"))
      {
        _tempDir += "\\";
      }
      _tempDir += "MPTemp";

      // Read Custom path for zip or apply default value
      if (_watchdogtargetDir == string.Empty)
      {
        _zipPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\MediaPortal-Logs\\";
      }
      else
      {
        _zipPath = string.Format(_watchdogtargetDir);
      }

      // Check If Watchdog is installed on TV Server folder for disabled 1st & 2nd choice & rename Zip file
      if (File.Exists(Path.Combine(_currentpath, "WatchDog.exe")) & File.Exists(Path.Combine(_currentpath, "SetupTV.exe")))
      {
        _TVEonly = true;
        _zipFile = string.Format("{0}\\MP_TVELogs_[date]_[time].zip",_zipPath);
        if (!ParseCommandLine())
        {
          Application.Exit();
        }
        tbZipFile.Text = _zipFile;

        SafeModeRadioButton.Enabled = false;
        NormalModeRadioButton.Enabled = false;
        ExportLogsRadioButton.Enabled = true;
        ExportLogsRadioButton.Checked = true;
      }
      else
      {
        _zipFile = string.Format("{0}\\MP_Logs_{1}_[date]_[time].zip",_zipPath,Environment.MachineName);
      }

      string tvPlugin = Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll";
      if (!File.Exists(tvPlugin))
      {
        menuItem14.Enabled = false;
      }

      if (!ParseCommandLine())
      {
        Application.Exit();
      }

      tbZipFile.Text = _zipFile;
      if (_autoMode)
      {
        if (!CheckRequirements())
        {
          Application.Exit();
        }
        tbZipFile.Text = _zipFile;
        if (_autoMode)
        {
          if (!CheckRequirements())
          {
            Application.Exit();
          }
          if (_safeMode)
          {
            SafeModeRadioButton.Checked = true;
          }
          else
          {
            NormalModeRadioButton.Checked = true;
          }
          EnableChoice(false);
          ProceedButton.Enabled = false;
          SetStatus("Running in auto/debug mode...");
          tmrUnAttended.Enabled = true;
        }
      }

      if (_watchdog)
      {
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        tmrWatchdog.Enabled = true;
        SetStatus("Running in WatchDog mode...");
        using (var xmlreader = new MPSettings())
        {
          _restoreTaskbar = xmlreader.GetValueAsBool("general", "hidetaskbar", false);
        }
      }

      if (!_watchdog && !_engage)
      {
        string[] args = Environment.GetCommandLineArgs();
        var info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, String.Join(" ", Enumerable.Concat(args.Skip(1), new[] { "-engage" })))
        {
          UseShellExecute = true,
          Verb = "runas", // indicates to eleavate privileges
        };

        var process = new Process
        {
          EnableRaisingEvents = true,
          StartInfo = info
        };
        try
        {
          process.Start();
        }
        catch
        {
          // This will be thrown if the user cancels the prompt
        }
        // process.WaitForExit(); // sleep calling process thread until evoked process exit
        System.Environment.Exit(0);
      }
    }

    #region Checks

    private bool ParseCommandLine()
    {
      string[] args = Environment.GetCommandLineArgs();
      for (int i = 1; i < args.Length; )
      {
        switch (args[i].ToLowerInvariant())
        {
          case "-zipfile":
            _zipFile = args[++i];
            break;
          case "-safe":
            _safeMode = true;
            break;
          case "-auto":
            _autoMode = true;
            break;
          case "-watchdog":
            _watchdog = true;
            break;
          case "-restartmp":
            _restartMP = true;
            if (!Int32.TryParse(args[++i], out _cancelDelay))
            {
              ShowUsage();
              return false;
            }
            break;
          case "-engage":
            _engage = true;
            break;
          default:
            ShowUsage();
            return false;
        }
        i++;
      }
      return true;
    }

    public bool CheckRequirements()
    {
      Directory.CreateDirectory(_tempDir);
      string zipFile = GetZipFilename();
      string directory = Path.GetDirectoryName(zipFile);
      if (directory != null && !Directory.Exists(directory))
      {
        try
        {
          Directory.CreateDirectory(directory);
        }
        catch (Exception ex)
        {
          Utils.ErrorDlg("You supplied an invalid path for the zip file. " + ex.Message);
          return false;
        }
      }
      return true;
    }

    #endregion

    #region Form Events

    private void btnZipFile_Click(object sender, EventArgs e)
    {
      var saveDialog = new SaveFileDialog
      {
        AddExtension = true,
        OverwritePrompt = true,
        DefaultExt = ".zip",
        Title = Resources.Choose_ZIP_file_to_create,
        FileName = tbZipFile.Text
      };
      //Default settings

      DialogResult dr = saveDialog.ShowDialog();
      if (dr == DialogResult.OK)
      {
        tbZipFile.Text = saveDialog.FileName;
        _zipFile = tbZipFile.Text;
      }
    }

    private void menuItem2_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void menuItem7_Click(object sender, EventArgs e)
    {
      var dlg = new AboutForm();
      dlg.ShowDialog();
    }

    private void ProceedButton_Click(object sender, EventArgs e)
    {
      if (!CheckRequirements())
      {
        return;
      }

      EnableChoice(false);
      ProceedButton.Enabled = false;
      if (ExportLogsRadioButton.Checked)
      {
        PerformPostTestActions();
        EnableChoice(true);
        ProceedButton.Enabled = true;
      }
      else
      {
        _safeMode = SafeModeRadioButton.Checked;
        PerformPreTestActions(true);
        LaunchMediaPortalAction();
      }
    }

    #endregion

    #region Perform actions

    private void PerformPreTestActions()
    {
      PerformPreTestActions(_autoMode);
    }

    private void PerformPreTestActions(bool autoClose)
    {
      SetStatus("Busy performing pre-test actions...");
      var pta = new PreTestActions(GetZipFilename());
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);

      SetStatus(pta.PerformActions() ? "Done performing pre-test actions." : "Pre-test actions were aborted.");
      if (autoClose)
      {
        pta.Close();
      }
    }

    private void LaunchMediaPortalAction()
    {
      _knownPids.Clear();
      if (!Directory.Exists(_tempDir))
      {
        Directory.CreateDirectory(_tempDir);
      }
      CreateTemporaryConfiguration();
      SetStatus("Launching MediaPortal...");
      /*
      _processMP = new Process
      {
        StartInfo =
        {
          WorkingDirectory = Application.StartupPath,
          FileName = "mediaportal.exe",
          Arguments = GetMPArguments()
        }
      };
      _processMP.Start();
      */
      RunAsDesktopUser(Application.StartupPath + @"\Mediaportal.exe", GetMPArguments());
      SetStatus("MediaPortal started. Waiting for exit...");
      Update();
      tmrMPWatcher.Enabled = true;
    }

    private void PerformPostTestActions()
    {
      PerformPostTestActions(_autoMode);
    }

    private void PerformPostTestActions(bool autoClose)
    {
      SetStatus("Busy performing post-test actions...");
      var pta = new PostTestActions(_tempDir, GetZipFilename());
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);

      SetStatus(pta.PerformActions() ? "Done performing post-test actions." : "Post-test actions were aborted.");
      if (autoClose)
      {
        pta.Close();
      }
    }

    #endregion

    #region Timer callbacks

    private void tmrUnAttended_Tick(object sender, EventArgs e)
    {
      tmrUnAttended.Enabled = false;
      PerformPreTestActions();
      LaunchMediaPortalAction();
    }

    private void tmrMPWatcher_Tick(object sender, EventArgs e)
    {
      tmrMPWatcher.Enabled = false;
      if (_processMP != null && _processMP.HasExited)
      {
        if (!string.IsNullOrEmpty(_tempConfig))
        {
          File.Delete(_tempConfig);
        }
        SetStatus("idle");
        PerformPostTestActions();

        EnableChoice(true);
        ProceedButton.Enabled = true;
        return;
      }

      List<DSGrapheditROTEntry> rotEntries = DaggerDSUtils.GetFilterGraphsFromROT();
      foreach (DSGrapheditROTEntry rot in rotEntries)
      {
        if (!_knownPids.Contains(rot.ToString()))
        {
          _knownPids.Add(rot.ToString());
          MakeGraphSnapshot(rot);
        }
      }
      tmrMPWatcher.Enabled = true;
    }

    #endregion

    private void MakeGraphSnapshot(DSGrapheditROTEntry rotEntry)
    {
      GraphsCreated++;
      DSGraphEditPanel panel;
      try
      {
        panel = new DSGraphEditPanel(rotEntry.ConnectToROTEntry());
      }
      catch (Exception)
      {
        return;
      }
      panel.Width = 3000;
      panel.ShowPinNames = true;
      panel.ShowTimeSlider = false;
      panel.dsDaggerUIGraph1.AutoArrangeWidthOffset = 150;
      panel.dsDaggerUIGraph1.ArrangeNodes(AutoArrangeStyle.All);
      using (var b = new Bitmap(panel.Width, panel.Height))
      {
        panel.DrawToBitmap(b, panel.Bounds);
        string imgFile = _tempDir + "\\graph_" + rotEntry + ".jpg";
        try
        {
          b.Save(imgFile, ImageFormat.Jpeg);
        }
        catch (Exception ex)
        {
          Utils.ErrorDlg("Exception raised while trying to save graph snapshot. file=[" + imgFile + "] message=[" +
                         ex.Message + "]");
        }
      }
      panel.Dispose();
    }

    private void tmrWatchdog_Tick(object sender, EventArgs e)
    {
      tmrWatchdog.Enabled = false;
      Process[] procs = Process.GetProcesses();
      bool running = procs.Any(p => p.ProcessName == "MediaPortal");
      if (running)
      {
        tmrWatchdog.Enabled = true;
      }
      else
      {
        if (!File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
        {
          Close();
          return;
        }
        ShowInTaskbar = true;
        WindowState = FormWindowState.Normal;
        CheckRequirements();
        EnableChoice(false);
        ExportLogsRadioButton.Checked = true;
        ProceedButton.Enabled = true;

        if (_restoreTaskbar)
        {
          MediaPortal.Util.Win32API.EnableStartBar(true);
          MediaPortal.Util.Win32API.ShowStartBar(true);
        }

        if (!_restartMP)
        {
          Utils.ErrorDlg("MediaPortal crashed unexpectedly.");
        }
        else
        {
          var dlg = new CrashRestartDlg(_cancelDelay);
          if (dlg.ShowDialog() == DialogResult.OK)
          {
            PerformPostTestActions();
            string mpExe = Config.GetFolder(Config.Dir.Base) + "\\MediaPortal.exe";
            var mp = new Process { StartInfo = { FileName = mpExe } };
            mp.Start();
            Close();
          }
        }
      }
    }

    private void menuItemStartTVserver_Click(object sender, EventArgs e)
    {
      SetStatus("Busy...");
      EnableChoice(false);
      ProceedButton.Enabled = false;

      TVServerManager mngr = new TVServerManager();
      mngr.TvServerRemoteStart();

      SetStatus("idle");
      EnableChoice(true);
      ProceedButton.Enabled = true;
    }

    private void menuItemStopTVserver_Click(object sender, EventArgs e)
    {
      SetStatus("Busy...");
      EnableChoice(false);
      ProceedButton.Enabled = false;

      TVServerManager mngr = new TVServerManager();
      mngr.TvServerRemoteStop();

      SetStatus("idle");
      EnableChoice(true);
      ProceedButton.Enabled = true;
    }

    private string GetDirectoryName(string f)
    {
      try
      {
        int posOfDirSep = f.LastIndexOf("\\");
        if (posOfDirSep >= 0)
          return f.Substring(0, posOfDirSep);
        else return string.Empty;
      }
      catch (Exception)
      {
        return string.Empty;
      }
    }

    private void ClearEventLog()
    {
      string[] logNames = { "Application", "System" };
      foreach (string strLogName in logNames)
      {
        EventLog e = new EventLog(strLogName);
        try
        {
          e.Clear();
        }
        catch (Exception) { }
      }
    }

    private void ClearDir(string strDir)
    {
      string[] files = Directory.GetFiles(strDir);
      string[] dirs = Directory.GetDirectories(strDir);

      foreach (string file in files)
      {
        if (File.Exists(file))
        {
          try
          {
            File.Delete(file);
          }
          catch (Exception) { }
        }
      }

      foreach (string dir in dirs)
      {
        if (Directory.Exists(dir))
        {
          try
          {
            Directory.Delete(dir, true);
          }
          catch (Exception) { }
        }
      }
    }

    private void tbZipFile_TextChanged(object sender, EventArgs e)
    {
      using (var xmlwriter = new Settings(_watchdogAppDir, false))
      {
        xmlwriter.SetValue("general", "watchdogTargetDir", GetDirectoryName(tbZipFile.Text));
      }
    }

    private void btnZipFileReset_Click(object sender, EventArgs e)
    {
      if (_TVEonly == true)
      {
        _zipFile = string.Format("{0}\\MediaPortal - Logs\\MP_TVELogs_[date]_[time].zip", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        tbZipFile.Text = _zipFile;
      }
      else
      {
        _zipFile = string.Format("{0}\\MediaPortal - Logs\\MP_Logs_{1}_[date]_[time].zip", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Environment.MachineName);
        tbZipFile.Text = _zipFile;
      }

    }

    private void menuItemClearEventLogs_Click(object sender, EventArgs e)
    {
      ClearEventLog();
    }

    private void menuItemClearMPlogs_Click(object sender, EventArgs e)
    {
      ClearDir(Config.GetFolder(Config.Dir.Log));
    }

    private void menuItemClearWEventLogOnTVserver_Click(object sender, EventArgs e)
    {
      TVServerManager mngr = new TVServerManager();
      mngr.ClearWindowsEventLogs();
    }

    private void menuItemClearTVserverLogs_Click(object sender, EventArgs e)
    {
      TVServerManager mngr = new TVServerManager();
      mngr.ClearTVserverLogs();
    }

    private void menuRebootTvServer_Click(object sender, EventArgs e)
    {
      string hostName;
      using (Settings xmlreader = new MPSettings())
      {
        hostName = xmlreader.GetValueAsString("tvservice", "hostname", string.Empty);
      }

      if (hostName == string.Empty)
      {
        return;
      }

      string msg = string.Format("Do you want to restart {0}?", hostName);

      var result = MessageBox.Show(msg, "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if (result == DialogResult.Yes)
      {
        TVServerManager mngr = new TVServerManager();
        mngr.RebootTvServer();
      }
    }

    private void menuShutdownTvServer_Click(object sender, EventArgs e)
    {
      string hostName;
      using (Settings xmlreader = new MPSettings())
      {
        hostName = xmlreader.GetValueAsString("tvservice", "hostname", string.Empty);
      }

      if (hostName == string.Empty)
      {
        return;
      }

      string msg = string.Format("Do you want to Shutdown {0}?", hostName);

      var result = MessageBox.Show(msg, "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if (result == DialogResult.Yes)
      {
        TVServerManager mngr = new TVServerManager();
        mngr.ShutdownTvServer();
      }
    }

    private void menuPowerOffTvServer_Click(object sender, EventArgs e)
    {
      string hostName;
      using (Settings xmlreader = new MPSettings())
      {
        hostName = xmlreader.GetValueAsString("tvservice", "hostname", string.Empty);
      }

      if (hostName == string.Empty)
      {
        return;
      }

      string msg = string.Format("Do you want to Power Off {0}?", hostName);

      var result = MessageBox.Show(msg, "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

      if (result == DialogResult.Yes)
      {
        TVServerManager mngr = new TVServerManager();
        mngr.PowerOffTvServer();
      }
    }

    private void menuItemWOLTvServer_Click(object sender, EventArgs e)
    {
      String macAddress, hostname;
      byte[] hwAddress;
      
      using (Settings xmlreader = new MPSettings())
      {
        macAddress = xmlreader.GetValueAsString("tvservice", "macAddress", null);
        hostname = xmlreader.GetValueAsString("tvservice", "hostname", null);

        if (!string.IsNullOrEmpty(macAddress))
        {
          try
          {
            WakeOnLanManager wakeOnLanManager = new WakeOnLanManager();

            Log.Debug("WOLMgr: Ping {0}", hostname);
            if (wakeOnLanManager.Ping(hostname, 200))
            {
              Log.Debug("WOLMgr: {0} already started", hostname);
              return;
            }

            hwAddress = wakeOnLanManager.GetHwAddrBytes(macAddress);

            if (!wakeOnLanManager.SendWakeOnLanPacket(hwAddress, IPAddress.Broadcast))
            {
              Log.Debug("WOLMgr: FAILED to send the first wake-on-lan packet!");
            }


          }
          catch (Exception ex)
          {
            Log.Error("WOL - Failed to start the TV server - {0}", ex.Message);
          }
            
          MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }

    }

    private void RunAsDesktopUser(string fileName, string arguments)
    {
      if (string.IsNullOrWhiteSpace(fileName))
      {
        throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
      }

      // To start process as shell user you will need to carry out these steps:
      // 1. Enable the SeIncreaseQuotaPrivilege in your current token
      // 2. Get an HWND representing the desktop shell (GetShellWindow)
      // 3. Get the Process ID(PID) of the process associated with that window(GetWindowThreadProcessId)
      // 4. Open that process(OpenProcess)
      // 5. Get the access token from that process (OpenProcessToken)
      // 6. Make a primary token with that token(DuplicateTokenEx)
      // 7. Start the new process with that primary token(CreateProcessWithTokenW)

      var hProcessToken = IntPtr.Zero;
      // Enable SeIncreaseQuotaPrivilege in this process.  (This won't work if current process is not elevated.)
      try
      {
        var process = GetCurrentProcess();
        if (!OpenProcessToken(process, 0x0020, ref hProcessToken))
          return;

        var tkp = new TOKEN_PRIVILEGES
        {
          PrivilegeCount = 1,
          Privileges = new LUID_AND_ATTRIBUTES[1]
        };

        if (!LookupPrivilegeValue(null, "SeIncreaseQuotaPrivilege", ref tkp.Privileges[0].Luid))
          return;

        tkp.Privileges[0].Attributes = 0x00000002;

        if (!AdjustTokenPrivileges(hProcessToken, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero))
          return;
      }
      finally
      {
        CloseHandle(hProcessToken);
      }

      // Get an HWND representing the desktop shell.
      // CAVEATS:  This will fail if the shell is not running (crashed or terminated), or the default shell has been
      // replaced with a custom shell.  This also won't return what you probably want if Explorer has been terminated and
      // restarted elevated.
      var hwnd = GetShellWindow();
      if (hwnd == IntPtr.Zero)
        return;

      var hShellProcess = IntPtr.Zero;
      var hShellProcessToken = IntPtr.Zero;
      var hPrimaryToken = IntPtr.Zero;
      try
      {
        // Get the PID of the desktop shell process.
        uint dwPID;
        if (GetWindowThreadProcessId(hwnd, out dwPID) == 0)
          return;

        // Open the desktop shell process in order to query it (get the token)
        hShellProcess = OpenProcess(ProcessAccessFlags.QueryInformation, false, dwPID);
        if (hShellProcess == IntPtr.Zero)
          return;

        // Get the process token of the desktop shell.
        if (!OpenProcessToken(hShellProcess, 0x0002, ref hShellProcessToken))
          return;

        var dwTokenRights = 395U;

        // Duplicate the shell's process token to get a primary token.
        // Based on experimentation, this is the minimal set of rights required for CreateProcessWithTokenW (contrary to current documentation).
        if (!DuplicateTokenEx(hShellProcessToken, dwTokenRights, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out hPrimaryToken))
          return;

        // Start the target process with the new token.
        var si = new STARTUPINFO();
        var pi = new PROCESS_INFORMATION();
        if (!CreateProcessWithTokenW(hPrimaryToken, 0, null, $"\"{fileName}\" {arguments}", 0, IntPtr.Zero, Path.GetDirectoryName(fileName), ref si, out pi))
          return;
        _processMP = Process.GetProcessById(pi.dwProcessId);
      }
      finally
      {
        CloseHandle(hShellProcessToken);
        CloseHandle(hPrimaryToken);
        CloseHandle(hShellProcess);
      }

    }

    #region Interop

    private struct TOKEN_PRIVILEGES
    {
      public UInt32 PrivilegeCount;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
      public LUID_AND_ATTRIBUTES[] Privileges;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct LUID_AND_ATTRIBUTES
    {
      public LUID Luid;
      public UInt32 Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LUID
    {
      public uint LowPart;
      public int HighPart;
    }

    [Flags]
    private enum ProcessAccessFlags : uint
    {
      All = 0x001F0FFF,
      Terminate = 0x00000001,
      CreateThread = 0x00000002,
      VirtualMemoryOperation = 0x00000008,
      VirtualMemoryRead = 0x00000010,
      VirtualMemoryWrite = 0x00000020,
      DuplicateHandle = 0x00000040,
      CreateProcess = 0x000000080,
      SetQuota = 0x00000100,
      SetInformation = 0x00000200,
      QueryInformation = 0x00000400,
      QueryLimitedInformation = 0x00001000,
      Synchronize = 0x00100000
    }

    private enum SECURITY_IMPERSONATION_LEVEL
    {
      SecurityAnonymous,
      SecurityIdentification,
      SecurityImpersonation,
      SecurityDelegation
    }

    private enum TOKEN_TYPE
    {
      TokenPrimary = 1,
      TokenImpersonation
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
      public IntPtr hProcess;
      public IntPtr hThread;
      public int dwProcessId;
      public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct STARTUPINFO
    {
      public Int32 cb;
      public string lpReserved;
      public string lpDesktop;
      public string lpTitle;
      public Int32 dwX;
      public Int32 dwY;
      public Int32 dwXSize;
      public Int32 dwYSize;
      public Int32 dwXCountChars;
      public Int32 dwYCountChars;
      public Int32 dwFillAttribute;
      public Int32 dwFlags;
      public Int16 wShowWindow;
      public Int16 cbReserved2;
      public IntPtr lpReserved2;
      public IntPtr hStdInput;
      public IntPtr hStdOutput;
      public IntPtr hStdError;
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool LookupPrivilegeValue(string host, string name, ref LUID pluid);

    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TOKEN_PRIVILEGES newst, int len, IntPtr prev, IntPtr relen);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, uint processId);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL impersonationLevel, TOKEN_TYPE tokenType, out IntPtr phNewToken);

    [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcessWithTokenW(IntPtr hToken, int dwLogonFlags, string lpApplicationName, string lpCommandLine, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

    #endregion
  }
}