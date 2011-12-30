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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DaggerLib.DSGraphEdit;
using DaggerLib.UI;
using MediaPortal.Configuration;
using MediaPortal.Profile;

namespace WatchDog
{
  public partial class MPWatchDog : MPForm
  {
    #region Constants

    private const string Default4to3Skin = "Default";
    private const string Default16to9Skin = "DefaultWide";

    #endregion

    #region Variables

    private string _tempDir = "";
    private string _zipFile = "";
    private string _tempConfig;
    private bool _autoMode = false;
    private bool _watchdog = false;
    private bool _restartMP = false;
    private bool _restoreTaskbar = false;
    private int _cancelDelay = 10;
    private Process _processMP = null;
    //private int _lastMPLogLevel = 2;
    private int _graphsCreated = 0;
    private List<string> _knownPids = new List<string>();
    private bool _safeMode = false;

    #endregion

    #region Helper functions

    private void ShowUsage()
    {
      string usageText = "\n" +
                         "Usage: MPWatchDog.exe [-auto] [-watchdog] [-zipFile <path+filename>] [-restartMP <delay in seconds>] \n" +
                         "\n" +
                         "auto     : Perform all actions automatically and start MediaPortal in between\n" +
                         "safe     : Only load built-in plugins and load default skin. Used with auto. \n" +
                         "watchdog : Used internally by MediaPortal to monitor MP\n" +
                         "zipFile  : full path and filename to the zip where all logfiles will be included\n" +
                         "restartMP: automatically collects all logs, saves them as zip to desktop, restarts MP and closes\n" +
                         "           the delay is the time in where you can cancel the operation\n" +
                         "\n";
      MessageBox.Show(usageText, "MediaPortal test tool usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void setStatus(string status)
    {
      this.statusBar.Text = string.Format("Status: {0}", status);
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
        return String.Format("/skin={0} /safelist=\"{1}\\BuiltInPlugins.xml\" /config=\"{2}\"",
                             GetScreenAspect() <= 1.5 ? Default4to3Skin : Default16to9Skin,
                             Application.StartupPath, _tempConfig);
      }
      else
      {
        return String.Format("/config=\"{0}\"", _tempConfig);
      }
    }

    private string CreateTemporaryConfiguration()
    {
      string tempSettingsFilename = Path.Combine(_tempDir, "MediaPortalTemp.xml");

      try
      {
        File.Copy(MPSettings.ConfigPathName, tempSettingsFilename, true);
        using (Settings xmlreader = new Settings(tempSettingsFilename, false))
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
        if (xmlreader.GetValueAsBool("screenselector", "usescreenselector", false))
        {
          screenNumber = xmlreader.GetValueAsInt("screenselector", "screennumber", screenNumber);
        }
      }
      if (screenNumber < 0 || screenNumber >= Screen.AllScreens.Length)
      {
        screenNumber = 0;
      }
      Screen mpScreen = Screen.AllScreens[screenNumber];
      return (float)mpScreen.Bounds.Width / (float)mpScreen.Bounds.Height;
    }

    private string GetZipFilename()
    {
      _zipFile = tbZipFile.Text;
      return _zipFile
        .Replace("[date]", DateTime.Now.ToString("dd_MM_yy"))
        .Replace("[time]", DateTime.Now.ToString("HH_mm"));
    }

    #endregion

    public MPWatchDog()
    {
      Thread.CurrentThread.Name = "MPWatchDog";
      InitializeComponent();
      _tempDir = Path.GetTempPath();
      if (!_tempDir.EndsWith("\\"))
      {
        _tempDir += "\\";
      }
      _tempDir += "MPTemp";
      _zipFile = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) +
                 "\\MediaPortalLogs_[date]__[time].zip";
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
        setStatus("Running in auto/debug mode...");
        tmrUnAttended.Enabled = true;
      }
      if (_watchdog)
      {
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        tmrWatchdog.Enabled = true;
        using (MPSettings xmlreader = new MPSettings())
        {
          _restoreTaskbar = xmlreader.GetValueAsBool("general", "hidetaskbar", false);
        }
      }
    }

    #region Checks

    private bool ParseCommandLine()
    {
      string[] args = Environment.GetCommandLineArgs();
      for (int i = 1; i < args.Length;)
      {
        switch (args[i].ToLower())
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
      if (!Directory.Exists(Path.GetDirectoryName(zipFile)))
      {
        try
        {
          Directory.CreateDirectory(Path.GetDirectoryName(zipFile));
        }
        catch (Exception)
        {
          Utils.ErrorDlg("You supplied an invalid path for the zip file.");
          return false;
        }
      }
      return true;
    }

    #endregion

    #region Form Events

    private void btnZipFile_Click(object sender, EventArgs e)
    {
      SaveFileDialog saveDialog = new SaveFileDialog();
      //Default settings
      saveDialog.AddExtension = true;
      saveDialog.OverwritePrompt = true;
      saveDialog.DefaultExt = ".zip";
      saveDialog.Title = "Choose ZIP file to create";

      saveDialog.FileName = tbZipFile.Text;
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
      AboutForm dlg = new AboutForm();
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
      setStatus("Busy performing pre-test actions...");
      PreTestActions pta = new PreTestActions();
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);

      if (pta.PerformActions())
      {
        setStatus("Done performing pre-test actions.");
      }
      else
      {
        setStatus("Pre-test actions were aborted.");
      }
      if (autoClose)
      {
        pta.Close();
        pta = null;
      }
    }

    private void LaunchMediaPortalAction()
    {
      _knownPids.Clear();
      if (!Directory.Exists(_tempDir))
      {
        Directory.CreateDirectory(_tempDir);
      }
      setStatus("Launching MediaPortal...");
      _processMP = new Process();
      _processMP.StartInfo.WorkingDirectory = Application.StartupPath;
      _processMP.StartInfo.FileName = "mediaportal.exe";
      _processMP.StartInfo.Arguments = GetMPArguments();
      _processMP.Start();
      setStatus("MediaPortal started. Waiting for exit...");
      Update();
      tmrMPWatcher.Enabled = true;
    }

    private void PerformPostTestActions()
    {
      PerformPostTestActions(_autoMode);
    }

    private void PerformPostTestActions(bool autoClose)
    {
      setStatus("Busy performing post-test actions...");
      PostTestActions pta = new PostTestActions(_tempDir, GetZipFilename());
      pta.Show();

      // give windows 1 sec to render the form
      Utils.SleepNonBlocking(1000);

      if (pta.PerformActions())
      {
        setStatus("Done performing post-test actions.");
      }
      else
      {
        setStatus("Post-test actions were aborted.");
      }
      if (autoClose)
      {
        pta.Close();
        pta = null;
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
      if (_processMP.HasExited)
      {
        if (!string.IsNullOrEmpty(_tempConfig))
        {
          File.Delete(_tempConfig);
        }
        setStatus("idle");
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
      _graphsCreated++;
      DSGraphEditPanel panel = null;
      try
      {
        panel = new DSGraphEditPanel(rotEntry.ConnectToROTEntry());
      }
      catch (Exception)
      {
        return;
      }
      if (panel == null)
      {
        return;
      }
      panel.Width = 3000;
      panel.ShowPinNames = true;
      panel.ShowTimeSlider = false;
      panel.dsDaggerUIGraph1.AutoArrangeWidthOffset = 150;
      panel.dsDaggerUIGraph1.ArrangeNodes(AutoArrangeStyle.All);
      using (Bitmap b = new Bitmap(panel.Width, panel.Height))
      {
        panel.DrawToBitmap(b, panel.Bounds);
        string imgFile = _tempDir + "\\graph_" + rotEntry.ToString() + ".jpg";
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
      bool running = false;
      foreach (Process p in procs)
      {
        if (p.ProcessName == "MediaPortal")
        {
          running = true;
          break;
        }
      }
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
        this.ShowInTaskbar = true;
        this.WindowState = FormWindowState.Normal;
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
          CrashRestartDlg dlg = new CrashRestartDlg(_cancelDelay);
          if (dlg.ShowDialog() == DialogResult.OK)
          {
            PerformPostTestActions();
            string mpExe = Config.GetFolder(Config.Dir.Base) + "\\MediaPortal.exe";
            Process mp = new Process();
            mp.StartInfo.FileName = mpExe;
            mp.Start();
            Close();
          }
        }
      }
    }
  }
}