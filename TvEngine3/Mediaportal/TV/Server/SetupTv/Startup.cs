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
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Integration;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.SetupTV
{
  public enum StartupMode
  {
    Normal,
    Wizard,
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
          applicationForm = new SetupTvSettingsForm(_debugOptions);
          break;

        case StartupMode.Wizard:
          applicationForm = new WizardForm(_sectionsConfiguration);
          break;

        case StartupMode.DbCleanup:
          applicationForm = new SetupTvSettingsForm(_debugOptions);
          break;
      }

      if (applicationForm != null)
      {
        Application.Run(applicationForm);
      }
    }

    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Log.Error(e.Exception, "Exception in setuptv");
    }

    [STAThread]
    public static void Main(string[] arguments)
    {
      // Initialize hosting environment, check for provider inside "Integration" subfolder. This helps to avoid assembly version conflicts.
      IntegrationProviderHelper.Register(PathManager.BuildAssemblyRelativePath("Integration"));

      if (System.IO.File.Exists("c:\\debug_setuptv.txt"))
      {
        System.Diagnostics.Debugger.Launch();
      }

      Thread.CurrentThread.Name = "SetupTv";
      Application.SetCompatibleTextRenderingDefault(false);

      if (ConfigurationManager.AppSettings.Count > 0)
      {
        string appSetting = ConfigurationManager.AppSettings["tvserver"];
        if (appSetting != null)
        {
          ServiceAgents.Instance.Hostname = appSetting;
        }
      }
      
      bool tvserviceInstalled = WaitAndQueryForTvServiceUntilFound();
      if (tvserviceInstalled)
      {
        Log.Info("---- check if tvservice is running ----");
        if (!ServiceHelper.IsRestrictedMode && !ServiceHelper.IsRunning)
        {
          Log.Info("---- tvservice is not running ----");
          if (_startupMode != StartupMode.DeployMode && PromptStartTvService() != DialogResult.Yes)
          {
            Environment.Exit(0);
          }
          Log.Info("---- start tvservice----");
          ServiceHelper.Start();
        }
        ServiceHelper.WaitInitialized();
      }


      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      config.AppSettings.Settings.Remove("tvserver");
      config.AppSettings.Settings.Add("tvserver", ServiceAgents.Instance.Hostname);
      config.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");

      _serverMonitor.OnServerConnected += new ServerMonitor.ServerConnectedDelegate(_serverMonitor_OnServerConnected);
      _serverMonitor.OnServerDisconnected += new ServerMonitor.ServerDisconnectedDelegate(_serverMonitor_OnServerDisconnected);
      _serverMonitor.Start();

      /*Process[] p = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
      if (p.Length > 1)
      {
        System.Environment.Exit(0);
      }*/

      string DeploySql = string.Empty;
      string DeployPwd = string.Empty;

      foreach (string param in arguments)
      {
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
              Log.Debug("---- started in Deploy mode ----");
              _startupMode = StartupMode.DeployMode;
              break;

            case "--DeploySql:":
              DeploySql = param.Split(':')[1].ToLower();
              break;

            case "--DeployPwd:":
              DeployPwd = param.Split(':')[1];
              break;
          }
        }
      }

      // set working dir from application.exe
      string applicationPath = Application.ExecutablePath;
      applicationPath = System.IO.Path.GetFullPath(applicationPath);
      applicationPath = System.IO.Path.GetDirectoryName(applicationPath);
      System.IO.Directory.SetCurrentDirectory(applicationPath);

      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

      Log.Info("---- SetupTv v" + versionInfo.FileVersion + " is starting up on " + OSInfo.OSInfo.GetOSDisplayVersion());

      //Check for unsupported operating systems
      OSPrerequisites.OSPrerequisites.OsCheck(true);

      Application.ThreadException += Application_ThreadException;



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

      // Avoid the visual part of SetupTv if in DeployMode
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
      DialogResult result = MessageBox.Show("The TV service is not running.\rStart it now?",
                                            "MediaPortal TV Server", MessageBoxButtons.YesNo);
      return result;
    }

    private static bool WaitAndQueryForTvServiceUntilFound()
    {
      bool tvServiceInstalled = false;
      while (!tvServiceInstalled)
      {
        //maybe tvservice is started as a console app or as MP2TV server ?
        try
        {
          IEnumerable<string> ipAdresses = ServiceAgents.Instance.ControllerServiceAgent.ServerIpAdresses;
          ServiceHelper.IsRestrictedMode = true;
          break;
        }
        catch (Exception)
        {
        }
        
        tvServiceInstalled = ServiceHelper.IsInstalled(ServiceHelper.SERVICENAME_TVSERVICE, ServiceAgents.Instance.Hostname);
        if (!tvServiceInstalled)
        {
          if (ServiceHelper.IsRestrictedMode)
          {
            break;
          }
          if (!String.IsNullOrEmpty(ServiceAgents.Instance.Hostname))
          {
            string hostName;
            if (PromptHostName(out hostName) != DialogResult.OK)
            {
              Environment.Exit(0);
            }
            UpdateTvServerConfiguration(hostName);
          }
        }
      }

      int cardCount = -1;
      while (cardCount == -1)
      {
        try
        {
          cardCount = ServiceAgents.Instance.ControllerServiceAgent.Cards;
        }
        catch (Exception)
        {
          if (tvServiceInstalled)
          {
            Log.Info("---- restart tvservice----");
            if (PromptStartTvService() != DialogResult.Yes)
            {
              Environment.Exit(0);
            }

            try
            {
              ServiceHelper.Restart();
              ServiceHelper.WaitInitialized();
            }
            catch (Exception ex)
            {
              Log.Error("SetupTV: failed to start tvservice : {0}", ex);
            }
          }
          else
          {
            HandleRestrictiveMode();
          }
        }
      }
      return tvServiceInstalled;
    }

    private static void HandleRestrictiveMode()
    {
      Log.Info("---- unable to restart tvservice, possible multiseat setup with no access to remote windows service ----");
      string hostName;
      if (PromptHostName(out hostName) != DialogResult.OK)
      {
        Environment.Exit(0);
      }
      UpdateTvServerConfiguration(hostName);
    }

    private static void UpdateTvServerConfiguration(string newHostName)
    {
      Log.Info("UpdateTvServerConfiguration newHostName = {0}", newHostName);
      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      ServiceAgents.Instance.Hostname = newHostName;
      ConfigurationManager.AppSettings["tvserver"] = newHostName;
      config.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");
    }

    static void _serverMonitor_OnServerDisconnected()
    {
      if (!ServiceHelper.IgnoreDisconnections)
      {
        WaitAndQueryForTvServiceUntilFound();
      }
    }

    private static DialogResult PromptHostName(out string hostName)
    {
      InputBoxResult result = InputBox.Show(
        "The TV service could not be found.\n\n" +
        "Please confirm the name of the computer on which the TV service is running.\n\n" +
        "If the TV service is installed on a different computer, check your network connectivity and security.",
        "MediaPortal TV Server", ConfigurationManager.AppSettings["tvserver"]);
      hostName = result.Text;
      return result.ReturnCode;
    }

    static void _serverMonitor_OnServerConnected()
    {
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources"))
        return null;
      if (args.Name.Contains(".XmlSerializers"))
        return null;
      MessageBox.Show(
        "Failed to locate assembly '" + args.Name + "'." + Environment.NewLine +
        "Note that the configuration program must be executed from/reside in the MediaPortal folder, the execution will now end.",
        "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Application.Exit();
      return null;
    }
  }
}