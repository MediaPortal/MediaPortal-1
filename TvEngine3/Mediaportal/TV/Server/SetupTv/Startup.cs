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
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV
{
  public enum StartupMode
  {
    Normal,
    DbCleanup,
    DbConfig,
    DeployMode
  }

  /// <summary>
  /// Summary description for Startup.
  /// </summary>
  public class Startup
  {
    private static StartupMode _startupMode = StartupMode.Normal;
    private static bool _debugOptions;
    private static readonly ServerMonitor _serverMonitor = new ServerMonitor();

    private readonly string _sectionsConfiguration = String.Empty;

    /// <summary>
    /// 
    /// </summary>
    public Startup()
    {
      _startupMode = StartupMode.Normal;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {      
      Form applicationForm = null;

      switch (_startupMode)
      {
        case StartupMode.Normal:
          applicationForm = new SettingsForm(_debugOptions);
          break;

        case StartupMode.DbCleanup:
          applicationForm = new SettingsForm(_debugOptions);
          break;
      }

      if (applicationForm != null)
      {
        Application.Run(applicationForm);
      }
    }

    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Log.Error(e.Exception, "Unhandled exception in TV Server Configuration.");
    }

    [STAThread]
    public static void Main(string[] arguments)
    {
      if (System.IO.File.Exists("c:\\debug_setuptv.txt"))
      {
        System.Diagnostics.Debugger.Launch();
      }
      Application.ThreadException += Application_ThreadException;

      // Initialise hosting environment, check for provider inside "Integration" subfolder. This helps to avoid assembly version conflicts.
      IntegrationProviderHelper.Register(PathManager.BuildAssemblyRelativePath("Integration"));

      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      Log.Info("---- TV Server Configuration v{0} is starting up on {1} ----", versionInfo.FileVersion, OSInfo.OSInfo.GetOSDisplayVersion());

      Thread.CurrentThread.Name = "SetupTv";
      Application.SetCompatibleTextRenderingDefault(false);

      // Check for unsupported operating systems.
      OSPrerequisites.OSPrerequisites.OsCheck(true);

      // Set the working directory based on the EXE location.
      string applicationPath = Application.ExecutablePath;
      applicationPath = System.IO.Path.GetFullPath(applicationPath);
      applicationPath = System.IO.Path.GetDirectoryName(applicationPath);
      System.IO.Directory.SetCurrentDirectory(applicationPath);

      // Handle command line parameters.
      string deploySql = string.Empty;
      string deployPwd = string.Empty;
      foreach (string param in arguments)
      {
        Log.Info("---- param: {0} ----", param);
        switch (param.ToLowerInvariant())
        {
          case "/delete-db":
            _startupMode = StartupMode.DbCleanup;
            break;

          case "/configure-db":
            _startupMode = StartupMode.DbConfig;
            break;

          case "/debugoptions":
            _debugOptions = true;
            break;
        }

        if (param.StartsWith("--Deploy"))
        {
          switch (param.Substring(0, 12))
          {
            case "--DeployMode":
              _startupMode = StartupMode.DeployMode;
              break;

            case "--DeploySql:":
              deploySql = param.Split(':')[1].ToLower();
              break;

            case "--DeployPwd:":
              deployPwd = param.Split(':')[1];
              break;
          }
        }
      }

      // Start and/or connect to the TV service.
      if (ConfigurationManager.AppSettings.Count > 0)
      {
        string appSetting = ConfigurationManager.AppSettings["tvserver"];
        if (appSetting != null)
        {
          ServiceAgents.Instance.Hostname = appSetting;
        }
      }
      Log.Info("---- TV service host is {0} ----", ServiceAgents.Instance.Hostname ?? "[null]");
      bool tvserviceInstalled = WaitAndQueryForTvServiceUntilFound();
      EnsureTvServiceRunningOrDie(tvserviceInstalled && _startupMode != StartupMode.DeployMode);

      _serverMonitor.OnServerConnected += new ServerMonitor.ServerConnectedDelegate(_serverMonitor_OnServerConnected);
      _serverMonitor.OnServerDisconnected += new ServerMonitor.ServerDisconnectedDelegate(_serverMonitor_OnServerDisconnected);
      _serverMonitor.Start();

      /*this.LogInfo("---- check if database needs to be updated/created ----");
      int currentSchemaVersion = dlg.GetCurrentShemaVersion(startupMode);
      if (currentSchemaVersion <= 36) // drop pre-1.0 DBs and handle -1
      {
        // Allow users to cancel DB recreation to backup their old DB
        if (currentSchemaVersion > 0)
          if (
            MessageBox.Show(
              "Your existing database cannot be upgraded and will be replaced by an empty database. Continue now?",
              "DB recreation needed", MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
            return;

        this.LogInfo("---- create database ----");
        if (!dlg.ExecuteSQLScript("create"))
        {
          MessageBox.Show("Failed to create the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        this.LogInfo("- Database created.");
        currentSchemaVersion = dlg.GetCurrentShemaVersion(startupMode);
      }

      this.LogInfo("---- upgrade database schema ----");
      // Get MySQL server version
      string currentServerVersion = dlg.GetCurrentServerVersion(startupMode);
      if (!dlg.UpgradeDBSchema(currentSchemaVersion, currentServerVersion))
      {
        MessageBox.Show("Failed to upgrade the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      */

      // Avoid the visual part of SetupTv if in deploy mode.
      if (_startupMode == StartupMode.DeployMode)
      {
        return;
      }

      try
      {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        Application.EnableVisualStyles();
        Application.DoEvents();

        new Startup().Start();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      _serverMonitor.Stop();
    }

    private static DialogResult PromptStartTvService()
    {
      DialogResult result = MessageBox.Show("The TV service is not running." + Environment.NewLine + "Start it now?",
                                            SetupControls.SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.YesNo);
      return result;
    }

    private static bool WaitAndQueryForTvServiceUntilFound()
    {
      bool updateConfig = false;
      bool tvServiceInstalled = false;
      while (!tvServiceInstalled)
      {
        tvServiceInstalled = ServiceHelper.IsInstalled(ServiceHelper.SERVICE_NAME_TV_SERVICE, ServiceAgents.Instance.Hostname);
        if (!tvServiceInstalled)
        {
          // The TV service may be running as a console application or as a
          // plugin within the MP2 Server. In that case it won't be registered
          // as a Windows service, and the only way to detect it is simply to
          // try to connect to it.
          if (!string.IsNullOrEmpty(ServiceAgents.Instance.Hostname))
          {
            try
            {
              IEnumerable<string> ipAdresses = ServiceAgents.Instance.ControllerServiceAgent.ServerIpAddresses;
              ServiceHelper.IsRestrictedMode = true;
              Log.Info("---- restricted mode active ----");
              return false;
            }
            catch
            {
            }
          }

          // Service not registered and/or not connectable => wrong configuration?
          string hostName;
          if (PromptHostName(out hostName) != DialogResult.OK)
          {
            Environment.Exit(0);
          }
          ServiceAgents.Instance.Hostname = hostName;
          updateConfig = true;
        }
      }

      if (updateConfig)
      {
        UpdateTvServerConfiguration(ServiceAgents.Instance.Hostname);
      }
      return tvServiceInstalled;
    }

    private static void EnsureTvServiceRunningOrDie(bool promptToStart = true)
    {
      bool isRunning = false;
      if (!ServiceHelper.IsRestrictedMode)
      {
        Log.Info("---- check if TV service is running ----");
        isRunning = ServiceHelper.IsRunning;
        if (isRunning)
        {
          Log.Info("---- TV service is already running ----");
        }
        else
        {
          Log.Info("---- TV service is not running ----");
          if (promptToStart && PromptStartTvService() != DialogResult.Yes)
          {
            Environment.Exit(0);
          }
          Log.Info("---- start TV service ----");
          if (!ServiceHelper.Start())
          {
            Log.Warn("---- possible failure to start TV service ----");
            isRunning = false;
          }
        }
        if (!ServiceHelper.WaitInitialized())
        {
          Log.Warn("---- TV service not started or non-communicative ----");
          isRunning = false;
        }
      }

      // Dummy call to confirm the service is running/available.
      try
      {
        Log.Info("---- check connection to TV service ----");
        string version = ServiceAgents.Instance.ControllerServiceAgent.GetAssemblyVersion;
        Log.Info("---- TV service connection seems to be okay ----");
      }
      catch (Exception ex)
      {
        if (ServiceHelper.IsRestrictedMode)
        {
          WaitAndQueryForTvServiceUntilFound();
        }
        else
        {
          Log.Error(ex, "Failed to start and/or communicate with the TV service.");
          if (isRunning)
          {
            MessageBox.Show("The TV service seems to be running but is not responding. This can happen when an older version of TV Server is active. Please check log files for errors.", SetupControls.SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
          else
          {
            MessageBox.Show("Failed to start the TV service. Please check log files for errors.", SetupControls.SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
          Environment.Exit(-1);
        }
      }
    }

    private static void UpdateTvServerConfiguration(string newHostName)
    {
      Log.Info("---- update TV service host name to {0} ----", newHostName ?? "[null]");
      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      config.AppSettings.Settings.Remove("tvserver");
      config.AppSettings.Settings.Add("tvserver", ServiceAgents.Instance.Hostname);
      config.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");
    }

    static void _serverMonitor_OnServerDisconnected()
    {
      Log.Info("---- TV service connection disconnected, host name = {0} ----", ServiceAgents.Instance.Hostname ?? "[null]");
      if (!ServiceHelper.IgnoreDisconnections)
      {
        WaitAndQueryForTvServiceUntilFound();
        EnsureTvServiceRunningOrDie();
      }
    }

    private static DialogResult PromptHostName(out string hostName)
    {
      InputBoxResult result = InputBox.Show(
        "The TV service could not be found." + Environment.NewLine + Environment.NewLine +
        "Please confirm the name of the computer on which the TV service is running." + Environment.NewLine + Environment.NewLine +
        "If the TV service is installed on a different computer, check your network connectivity and security configuration.",
        "MediaPortal TV Server", ConfigurationManager.AppSettings["tvserver"]);
      hostName = result.Text;
      Log.Info("---- ask for new host name, response = {0}, host name = {1} ----", result.ReturnCode, result.Text ?? "[null]");
      return result.ReturnCode;
    }

    static void _serverMonitor_OnServerConnected()
    {
      Log.Info("---- TV service connection connected, host name = {0} ----", ServiceAgents.Instance.Hostname ?? "[null]");
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources"))
        return null;
      if (args.Name.Contains(".XmlSerializers"))
        return null;
      MessageBox.Show(
        "Failed to locate assembly '" + args.Name + "'." + Environment.NewLine +
        "TV Server Configuration will close now.",
        SetupControls.SectionSettings.MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
      Application.Exit();
      return null;
    }
  }
}