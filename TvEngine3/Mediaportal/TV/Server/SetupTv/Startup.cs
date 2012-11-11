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

    private const string TypeValidHostnameForTvServerOrExitApplication = "Type valid hostname for tv server (or exit application):";
    private const string TvserviceNotFoundMaybeYouLackUserRightsToAccessControlRemoteWindowsService = "TvService not found (maybe you lack user rights to access/control remote windows service).";
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
      // Initialize hosting environment      
      IntegrationProviderHelper.Register();      

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
      
      bool tvserviceInstalled = WaitAndQueryForTvserviceUntilFound();      

      if (tvserviceInstalled)
      {
        Log.Info("---- check if tvservice is running ----");
        if (!ServiceHelper.IsRestrictedMode && !ServiceHelper.IsRunning)
        {
          Log.Info("---- tvservice is not running ----");
          if (_startupMode != StartupMode.DeployMode)
          {
            DialogResult result = ShowStartTvServiceDialog();
            if (result != DialogResult.Yes)
            {
              Environment.Exit(0);
            }
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
      if (!dlg.UpgradeDBSchema(currentSchemaVersion))
      {
        MessageBox.Show("Failed to upgrade the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      */                 

      // Mantis #0002138: impossible to configure TVGroups             
      ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels, MediaTypeEnum.TV);
      ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.RadioGroupNames.AllChannels, MediaTypeEnum.Radio);

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

    private static DialogResult ShowStartTvServiceDialog()
    {
      DialogResult result = MessageBox.Show("The Tv service is not running.\rStart it now?",
                                            "Mediaportal TV service", MessageBoxButtons.YesNo);
      return result;
    }

    private static bool WaitAndQueryForTvserviceUntilFound()
    {
      bool tvserviceInstalled = false;
      while (!tvserviceInstalled)
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
        
        tvserviceInstalled = ServiceHelper.IsInstalled(ServiceHelper.SERVICENAME_TVSERVICE, ServiceAgents.Instance.Hostname);
        if (!tvserviceInstalled)
        {
          if (ServiceHelper.IsRestrictedMode)
          {
            break;
          }
          if (!String.IsNullOrEmpty(ServiceAgents.Instance.Hostname))
          {
            string newHostName;
            bool inputNewHost = ConnectionLostPrompt(TypeValidHostnameForTvServerOrExitApplication,
                                                     TvserviceNotFoundMaybeYouLackUserRightsToAccessControlRemoteWindowsService,
                                                     out newHostName);

            if (inputNewHost)
            {
              UpdateTvServerConfiguration(newHostName);
            }
            else
            {
              Environment.Exit(0);
            }
          }
        }
      }

      int cards = -1;
      while (cards == -1)
      {
        try
        {
          cards = ServiceAgents.Instance.ControllerServiceAgent.Cards;
        }
        catch (Exception)
        {
          if (tvserviceInstalled)
          {
            Log.Info("---- restart tvservice----");
            DialogResult result = ShowStartTvServiceDialog();
            if (result == DialogResult.Yes)
            {
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
              MessageBox.Show("Chose not to start tvservice..exiting application");
              Environment.Exit(0);
            }                        
          }
          else
          {
            HandleRestrictiveMode();
          }
        }
      }
      return tvserviceInstalled;
    }

    private static void HandleRestrictiveMode()
    {
      Log.Info(
        "---- unable to restart tvservice, possible multiseat setup with no access to remote windows service ----");
      string newHostName;
      bool inputNewHost = ConnectionLostPrompt(TypeValidHostnameForTvServerOrExitApplication,
                                               TvserviceNotFoundMaybeYouLackUserRightsToAccessControlRemoteWindowsService,
                                               out newHostName);
      if (inputNewHost)
      {
        UpdateTvServerConfiguration(newHostName);
      }
      else
      {
        Environment.Exit(0);
      }
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
        WaitAndQueryForTvserviceUntilFound();       
      }      
    }

    private static bool ConnectionLostPrompt(string prompt, string title, out string newHostName)
    {      
      InputBoxResult result = InputBox.Show(prompt, title, ConfigurationManager.AppSettings["tvserver"]);
      newHostName = result.Text;
      bool connectionLostPrompt = (result.ReturnCode == DialogResult.OK);
      return connectionLostPrompt;
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