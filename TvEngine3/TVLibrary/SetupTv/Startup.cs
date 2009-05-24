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

      OsDetection.OSVersionInfo os = new OsDetection.OperatingSystemVersion();
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      string ServicePack = "";
      if (!String.IsNullOrEmpty(os.OSCSDVersion))
        ServicePack = " (" + os.OSCSDVersion + ")";
      Log.Info("---- SetupTv v" + versionInfo.FileVersion + " is starting up on " + os.OSVersionString + ServicePack + " ----");

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
          startupMode = StartupMode.DbCleanup;
        if (param == "--configure-db" || param == "-c" || param == @"/c")
          startupMode = StartupMode.DbConfig;
      }

      //test connection with database
      Log.Info("---- check connection with database ----");
      SetupDatabaseForm dlg = new SetupDatabaseForm(startupMode);
      if (startupMode != StartupMode.Normal || !dlg.TestConnection())
      {
        Log.Info("---- ask user for connection details ----");
        dlg.ShowDialog();

        return; // close the application without restart here.
      }

      Log.Info("---- check if database needs to be updated/created ----");
      int currentSchemaVersion = dlg.GetCurrentShemaVersion();
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
        currentSchemaVersion = dlg.GetCurrentShemaVersion();
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
        DialogResult result = MessageBox.Show("The Tv service is not running.\rStart it now?", "Mediaportal TV service", MessageBoxButtons.YesNo);
        if (result != DialogResult.Yes) return;
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
          RemoteControl.HostName = "localhost";
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
