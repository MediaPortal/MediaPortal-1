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

using System;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Threading;
using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
using TvControl;
using TvLibrary.Log;

using TvDatabase;
namespace SetupTv
{
  /// <summary>
  /// Summary description for Startup.
  /// </summary>
  public class Startup
  {
    enum StartupMode
    {
      Normal,
      Wizard
    }
    StartupMode startupMode = StartupMode.Normal;

    string sectionsConfiguration = String.Empty;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    public Startup(string[] arguments)
    {


      if (!System.IO.File.Exists("mediaportal.xml"))
        startupMode = StartupMode.Wizard;

      else if (arguments != null)
      {
        foreach (string argument in arguments)
        {
          string trimmedArgument = argument.ToLower();

          if (trimmedArgument.StartsWith("/wizard"))
          {
            startupMode = StartupMode.Wizard;
          }

          if (trimmedArgument.StartsWith("/section"))
          {
            string[] subArguments = argument.Split('=');

            if (subArguments.Length >= 2)
            {
              sectionsConfiguration = subArguments[1];
            }
          }
        }
      }
      startupMode = StartupMode.Normal;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {


      FileInfo mpFi = new FileInfo(Assembly.GetExecutingAssembly().Location);

      Form applicationForm = null;

      switch (startupMode)
      {
        case StartupMode.Normal:
          applicationForm = new SettingsForm();
          break;

        case StartupMode.Wizard:
          applicationForm = new WizardForm(sectionsConfiguration);
          break;
      }


      if (applicationForm != null)
      {
        System.Windows.Forms.Application.Run(applicationForm);
      }
    }
    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      Log.Write(e.Exception);
    }
    [STAThread]
    public static void Main(string[] arguments)
    {
      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
      //test connection with database
      SetupDatabaseForm dlg = new SetupDatabaseForm();
      if (!dlg.TestConnection())
      {
        dlg.ShowDialog();
      }
      if (dlg.ShouldDoUpgrade())
      {
        ServiceHelper.Stop();

        dlg.CreateDatabase();

        ServiceHelper.Restart();
      }

      // fill the cache
      int cards = 0;
      //auto start the tv-service
      if (!ServiceHelper.IsInstalled)
      {
        MessageBox.Show("The Tv service is not installed");
        return;
      }
      if (ServiceHelper.IsInstalled && !ServiceHelper.IsRunning)
      {
        ServiceHelper.Restart();
      }

      try
      {
        cards = RemoteControl.Instance.Cards;

      }
      catch (Exception)
      {
        MessageBox.Show("The Tv service is not running");
        return;
      }
      try
      {
        DatabaseManager.Instance.DefaultQueryStrategy = QueryStrategy.Normal;
        DatabaseManager.Instance.GetEntities<Channel>();
      }
      catch (Exception)
      {
        MessageBox.Show("Unable to open Tv Database");
        return;
      }

      try
      {
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        System.Windows.Forms.Application.EnableVisualStyles();
        System.Windows.Forms.Application.DoEvents();

        new Startup(arguments).Start();
      }
      finally
      {
        GC.Collect();
      }
    }

    private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources"))
        return null;
      if (args.Name.Contains(".XmlSerializers"))
        return null;
      MessageBox.Show("Failed to locate assembly '" + args.Name + "'." + Environment.NewLine + "Note that the configuration program must be executed from/reside in the MediaPortal folder, the execution will now end.", "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      System.Windows.Forms.Application.Exit();
      return null;
    }
  }
}
