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
using System.Net;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using Mediaportal.TV.Server.TVControl;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.TVBusinessLayer;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVService.ServiceAgents;

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
    private static StartupMode startupMode = StartupMode.Normal;
    private static bool debugOptions = false;
    private static ServerMonitor _serverMonitor = new ServerMonitor();

    private readonly string sectionsConfiguration = String.Empty;

    /// <summary>
    /// 
    /// </summary>
    public Startup()
    {
      startupMode = StartupMode.Normal;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {      
      Form applicationForm = null;

      switch (startupMode)
      {
        case StartupMode.Normal:
          applicationForm = new SetupTvSettingsForm(debugOptions);
          break;

        case StartupMode.Wizard:
          applicationForm = new WizardForm(sectionsConfiguration);
          break;

        case StartupMode.DbCleanup:
          applicationForm = new SetupTvSettingsForm(debugOptions);
          break;
      }

      if (applicationForm != null)
      {
        Application.Run(applicationForm);
      }
    }

    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Log.Write("Exception in setuptv");
      Log.Write(e.ToString());
      Log.Write(e.Exception);
    }

    [STAThread]
    public static void Main(string[] arguments)
    {
      Thread.CurrentThread.Name = "SetupTv";
      Application.SetCompatibleTextRenderingDefault(false);

      //System.Diagnostics.Debugger.Launch();

      if (ConfigurationManager.AppSettings.Count > 0)
      {
        string appSetting = ConfigurationManager.AppSettings["tvserver"];
        if (appSetting != null)
        {
          ServiceAgents.Instance.Hostname = appSetting;
        }
      }

      bool tvserviceInstalled = false;
      while (!tvserviceInstalled)
      {
        tvserviceInstalled = ServiceHelper.IsInstalled(@"TvService", ServiceAgents.Instance.Hostname);
        if (!tvserviceInstalled)
        {
          ConnectionLostPrompt("Type valid hostname for tv server:", "TvService not found.");
        }
        else
        { 
          Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
          config.AppSettings.Settings.Remove("tvserver");
          config.AppSettings.Settings.Add("tvserver", ServiceAgents.Instance.Hostname);
          config.Save(ConfigurationSaveMode.Modified);
          ConfigurationManager.RefreshSection("appSettings");
        }
      }

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
            startupMode = StartupMode.DbCleanup;
            break;

          case "/configure-db":
            startupMode = StartupMode.DbConfig;
            break;

          case "/debugoptions":
            debugOptions = true;
            break;
        }

        if (param.StartsWith("--Deploy"))
        {
          switch (param.Substring(0, 12))
          {
            case "--DeployMode":
              Log.Debug("---- started in Deploy mode ----");
              startupMode = StartupMode.DeployMode;
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

       

      /*Log.Info("---- check if database needs to be updated/created ----");
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

        Log.Info("---- create database ----");
        if (!dlg.ExecuteSQLScript("create"))
        {
          MessageBox.Show("Failed to create the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        Log.Info("- Database created.");
        currentSchemaVersion = dlg.GetCurrentShemaVersion(startupMode);
      }

      Log.Info("---- upgrade database schema ----");
      if (!dlg.UpgradeDBSchema(currentSchemaVersion))
      {
        MessageBox.Show("Failed to upgrade the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      */
      Log.Info("---- check if tvservice is running ----");
      if (!ServiceHelper.IsRunning)
      {
        Log.Info("---- tvservice is not running ----");
        if (startupMode != StartupMode.DeployMode)
        {
          DialogResult result = MessageBox.Show("The Tv service is not running.\rStart it now?",
                                                "Mediaportal TV service", MessageBoxButtons.YesNo);
          if (result != DialogResult.Yes) return;
        }
        Log.Info("---- start tvservice----");
        ServiceHelper.Start();
      }

      ServiceHelper.WaitInitialized();
      int cards = 0;
      try
      {
        cards = ServiceAgents.Instance.ControllerServiceAgent.Cards;
      }
      catch (Exception)
      {
        Log.Info("---- restart tvservice----");
        ServiceHelper.Restart();
        ServiceHelper.WaitInitialized();
        try
        {
          RemoteControl.HostName = Dns.GetHostName();
          cards = ServiceAgents.Instance.ControllerServiceAgent.Cards;
        }
        catch (Exception ex)
        {
          Log.Info("---- Unable to restart tv service----");
          Log.Write(ex);
          MessageBox.Show("Failed to startup tvservice" + ex);
          return;
        }
      }

      // Mantis #0001991: disable mpg recording  (part I: force TS recording format)
      IList<Card> TvCards = ServiceAgents.Instance.CardServiceAgent.ListAllCards();
      foreach (Card card in TvCards)
      {
        if (card.recordingFormat != 0)
        {
          card.recordingFormat = 0;
          Log.Info("Card {0} switched from .MPG to .TS format", card.name);
          ServiceAgents.Instance.CardServiceAgent.SaveCard(card);
        }
      }

      // Mantis #0002138: impossible to configure TVGroups             
      ServiceAgents.Instance.ChannelGroupServiceAgent.GetOrCreateGroup(TvConstants.TvGroupNames.AllChannels);

      // Avoid the visual part of SetupTv if in DeployMode
      if (startupMode == StartupMode.DeployMode)
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
        Log.Write(ex);
      }
      _serverMonitor.Stop();
    }

    static void _serverMonitor_OnServerDisconnected()
    {
      ConnectionLostPrompt("Type valid hostname for tv server:", "Connection lost.");
    }

    private static void ConnectionLostPrompt(string prompt, string title)
    {
      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      InputBoxResult result = InputBox.Show(prompt, title,
                                            ConfigurationManager.AppSettings["tvserver"]);
      ServiceAgents.Instance.Hostname = result.Text;
      ConfigurationManager.AppSettings["tvserver"] = result.Text;
      config.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");
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