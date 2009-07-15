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

using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using TvControl;
using TvDatabase;
using TvLibrary.Log;
using TvLibrary.Interfaces;

namespace SetupTv
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
    static StartupMode startupMode = StartupMode.Normal;

    readonly string sectionsConfiguration = String.Empty;

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
          applicationForm = new SetupTvSettingsForm();
          break;

        case StartupMode.Wizard:
          applicationForm = new WizardForm(sectionsConfiguration);
          break;

        case StartupMode.DbCleanup:
          applicationForm = new SetupTvSettingsForm();
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

      // set working dir from application.exe
      string applicationPath = Application.ExecutablePath;
      applicationPath = System.IO.Path.GetFullPath(applicationPath);
      applicationPath = System.IO.Path.GetDirectoryName(applicationPath);
      System.IO.Directory.SetCurrentDirectory(applicationPath);

      string DeploySql = string.Empty;
      string DeployPwd = string.Empty;

      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

      string ServicePack = OSInfo.OSInfo.GetOSServicePack();
      if (!string.IsNullOrEmpty(ServicePack))
      {
        ServicePack = " ( " + ServicePack + " )";
      }
      Log.Info("---- SetupTv v" + versionInfo.FileVersion + " is starting up on " + OSInfo.OSInfo.GetOSNameString() + ServicePack + " [" + OSInfo.OSInfo.OSVersion + "] ----");

      //Check for unsupported operating systems
      if (OSInfo.OSInfo.GetOSSupported() != 1)
      {
        //Used .Info as .Warning is missing
        Log.Info("****************************************");
        Log.Info("* WARNING, OS not officially supported *");
        Log.Info("****************************************");
      }

      NameValueCollection appSettings = ConfigurationManager.AppSettings;
      appSettings.Set("GentleConfigFile", String.Format(@"{0}\gentle.config", Log.GetPathName()));

      Application.ThreadException += Application_ThreadException;

      foreach (string param in arguments)
      {
        if (param == "--delete-db" || param == "-d" || param == @"/d")
        {
          startupMode = StartupMode.DbCleanup;
        }
        if (param == "--configure-db" || param == "-c" || param == @"/c")
        {
          startupMode = StartupMode.DbConfig;
        }
        if (param == "--DeployMode")
        {
          Log.Debug("---- started in Deploy mode ----");
          startupMode = StartupMode.DeployMode;
        }
        if (param.StartsWith("--DeploySql"))
        {
          DeploySql = param.Split(':')[1].ToLower();
        }
        if (param.StartsWith("--DeployPwd"))
        {
          DeployPwd = param.Split(':')[1];
        }
      }

      //test connection with database
      Log.Info("---- check connection with database ----");
      SetupDatabaseForm dlg = new SetupDatabaseForm(startupMode);

      if (startupMode == StartupMode.DeployMode)
      {
        if (DeploySql == "mysql")
        {
          dlg.provider = SetupDatabaseForm.ProviderType.MySql;
          dlg.rbMySQL.Checked = true;
          dlg.tbUserID.Text = "root";
          dlg.tbServerHostName.Text = Dns.GetHostName();
          dlg.tbServiceDependency.Text = @"MySQL5";
        }
        else
        {
          dlg.provider = SetupDatabaseForm.ProviderType.SqlServer;
          dlg.rbSQLServer.Checked = true;
          dlg.tbUserID.Text = "sa";
          dlg.tbServerHostName.Text = Dns.GetHostName() + @"\SQLEXPRESS";
          dlg.tbServiceDependency.Text = @"SQLBrowser";
        }
        dlg.tbPassword.Text = DeployPwd;
        dlg.tbDatabaseName.Text = dlg.schemaNameDefault;
        dlg.schemaName = dlg.schemaNameDefault;
      }
      if ((startupMode != StartupMode.Normal && startupMode != StartupMode.DeployMode) || (!dlg.TestConnection(startupMode)))
      {
        Log.Info("---- ask user for connection details ----");
        dlg.ShowDialog();

        return; // close the application without restart here.
      }
      dlg.CheckServiceName();
      if (startupMode == StartupMode.DeployMode)
      {
        dlg.SaveGentleConfig();
      }

      Log.Info("---- check if database needs to be updated/created ----");
      int currentSchemaVersion = dlg.GetCurrentShemaVersion(startupMode);
      if (currentSchemaVersion <= 36) // drop pre-1.0 DBs and handle -1
      {
        // Allow users to cancel DB recreation to backup their old DB
        if (currentSchemaVersion > 0)
          if (MessageBox.Show("Your existing database cannot be upgraded and will be replaced by an empty database. Continue now?", "DB recreation needed", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
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

      int cards = 0;
      try
      {
        cards = RemoteControl.Instance.Cards;
      }
      catch (Exception)
      {
        Log.Info("---- restart tvservice----");
        ServiceHelper.Restart();
        try
        {
          RemoteControl.Clear();
          RemoteControl.HostName = Dns.GetHostName();
          cards = RemoteControl.Instance.Cards;
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
      IList<Card> TvCards = Card.ListAll();
      foreach (Card card in TvCards)
      {
        if (card.RecordingFormat != 0)
        {
          card.RecordingFormat = 0;
          Log.Info("Card {0} switched from .MPG to .TS format", card.Name);
          card.Persist();
        }
      }

      // Mantis #0002138: impossible to configure TVGroups 
      TvBusinessLayer layer = new TvBusinessLayer();
      layer.CreateGroup(TvConstants.TvGroupNames.AllChannels);

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
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources"))
        return null;
      if (args.Name.Contains(".XmlSerializers"))
        return null;
      MessageBox.Show("Failed to locate assembly '" + args.Name + "'." + Environment.NewLine + "Note that the configuration program must be executed from/reside in the MediaPortal folder, the execution will now end.", "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Application.Exit();
      return null;
    }
  }
}
