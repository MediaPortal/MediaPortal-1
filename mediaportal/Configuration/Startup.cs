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
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Util;
using System.Reflection;
using System.IO;

namespace MediaPortal.Configuration
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

    public delegate bool IECallBack(int hwnd, int lParam);
    private const int SW_SHOWNORMAL = 1;

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr window, int message, int wparam, int lparam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.Dll")]
    public static extern int EnumWindows(IECallBack x, int y);

    [DllImport("User32.Dll")]
    public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

    [DllImport("User32.Dll")]
    public static extern void GetClassName(int h, StringBuilder s, int nMaxCount);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    public Startup(string[] arguments)
    {
      Thread.CurrentThread.Name = "Config Main";
      // Logger should write into Configuration.log
      Log.SetConfigurationMode();
      Log.BackupLogFile(LogType.Config);
      Log.Info("Using Directories:");
      foreach (string options in Enum.GetNames(typeof(Config.Dir)))
      {
        Log.Info("{0} - {1}", options, Config.GetFolder((Config.Dir)Enum.Parse(typeof(Config.Dir), options)));
      }
      if (!File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
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
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {
      Log.Info("Configuration is starting up");

      bool exitConfiguration = false;

      // Check for a MediaPortal Instance running and don't allow Configuration to start
      try
      {
        string processName = "MediaPortal";

        
        foreach (Process process in Process.GetProcesses())
        {
          if (process.ProcessName.Equals(processName))
          {
            DialogResult dialogResult =
              MessageBox.Show("MediaPortal has to be closed for configuration.\nClose MediaPortal and start Configuration?",
                              "MediaPortal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
              try
              {
                //
                // Terminate the MediaPortal process by finding window and sending ALT+F4 to it.
                //
                IECallBack ewp = new IECallBack(EnumWindowCallBack);
                EnumWindows(ewp, 0);
                process.CloseMainWindow();
              }
              catch
              { }
              Log.Info("MediaPortal closed, continue running Configuration.");
              break;
            }
            else
            {
              exitConfiguration = true;
              break;
            }
          }
        }
      }
      catch (Exception)
      { }

      if (exitConfiguration)
      {
        Log.Info("Configuration ended, due to a running MediaPortal.");
        return;
      }

      FileInfo mpFi = new FileInfo(Assembly.GetExecutingAssembly().Location);
      Log.Info("Assembly creation time: {0} (UTC)", mpFi.LastWriteTimeUtc.ToUniversalTime());

      Form applicationForm = null;

      Thumbs.CreateFolders();

      switch (startupMode)
      {
        case StartupMode.Normal:
          Log.Info("Create new standard setup");
          applicationForm = new SettingsForm();
          break;

        case StartupMode.Wizard:
          Log.Info("Create new wizard setup");
          applicationForm = new WizardForm(sectionsConfiguration);
          break;
      }


      if (applicationForm != null)
      {

        Log.Info("start application");
        System.Windows.Forms.Application.Run(applicationForm);
      }
    }

    [STAThread]
    public static void Main(string[] arguments)
    {
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

    private bool EnumWindowCallBack(int hwnd, int lParam)
    {
      IntPtr windowHandle = (IntPtr)hwnd;
      StringBuilder sb = new StringBuilder(1024);
      GetWindowText((int)windowHandle, sb, sb.Capacity);
      string window = sb.ToString().ToLower();
      if (window.IndexOf("mediaportal") >= 0 || window.IndexOf("media portal") >= 0)
      {
        ShowWindow(windowHandle, SW_SHOWNORMAL);
      }
      return true;
    }
  }
}
