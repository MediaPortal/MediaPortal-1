#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Util;
using OsDetection;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for Startup.
  /// </summary>
  public class Startup
  {
    private enum StartupMode
    {
      Normal,
      Wizard
    }

    private StartupMode startupMode = StartupMode.Normal;

    private string sectionsConfiguration = string.Empty;

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
      Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

      // Added OS requirements checks
      CheckPrerequisites();

      // Logger should write into Configuration.log
      Log.SetConfigurationMode();
      Log.BackupLogFile(LogType.Config);

      Log.Info("Verifying DirectX 9");
      if (!DirectXCheck.IsInstalled())
      {
        string strLine = "Please install a newer DirectX 9.0c redist!\r\n";
        strLine = strLine + "MediaPortal cannot run without DirectX 9.0c redist (August 2008)\r\n";
        strLine = strLine + "http://install.team-mediaportal.com/DirectX";
        MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      Log.Info("Using Directories:");
      foreach (string options in Enum.GetNames(typeof (Config.Dir)))
      {
        Log.Info("{0} - {1}", options, Config.GetFolder((Config.Dir) Enum.Parse(typeof (Config.Dir), options)));
      }

      // rtv: disabled Wizard due to frequent bug reports on serveral sections.
      // please fix those before re-enabling.
      //
      //if (!File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.xml")))
      //  startupMode = StartupMode.Wizard;
      //else
      if (arguments != null)
      {
        foreach (string argument in arguments)
        {
          string trimmedArgument = argument.ToLower();

          if (trimmedArgument.StartsWith("/wizard"))
          {
            //startupMode = StartupMode.Wizard;
            Log.Debug("Startup: Argument did request Wizard mode - {0}", trimmedArgument);
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
      GC.Collect();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {
      OSVersionInfo os = new OperatingSystemVersion();
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
      string ServicePack = "";
      if (!String.IsNullOrEmpty(os.OSCSDVersion))
      {
        ServicePack = " (" + os.OSCSDVersion + ")";
      }
      Log.Info("Configuration v" + versionInfo.FileVersion + " is starting up on " + os.OSVersionString + ServicePack);

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
              MessageBox.Show(
                "MediaPortal has to be closed for configuration.\nClose MediaPortal and start Configuration?",
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
              {
              }
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
      {
      }

      if (exitConfiguration)
      {
        Log.Info("Configuration ended, due to a running MediaPortal.");
        return;
      }

      // Check TvPlugin version
      string tvPlugin = Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll";
      if (File.Exists(tvPlugin))
      {
        string tvPluginVersion = FileVersionInfo.GetVersionInfo(tvPlugin).ProductVersion;
        string mpVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        if (mpVersion != tvPluginVersion)
        {
          string strLine = "TvPlugin and MediaPortal don't have the same version.\r\n";
          strLine += "Please update the older component to the same version as the newer one.\r\n";
          strLine += "MP Version: " + mpVersion + "\r\n";
          strLine += "TvPlugin Version: " + tvPluginVersion;
          MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
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
        Application.Run(applicationForm);
      }
    }

    [STAThread]
    public static void Main(string[] arguments)
    {
      try
      {
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        Application.EnableVisualStyles();
        Application.DoEvents();

        new Startup(arguments).Start();
      }
      finally
      {
        GC.Collect();
      }
    }

    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      if (args.Name.Contains(".resources"))
      {
        return null;
      }
      if (args.Name.Contains(".XmlSerializers"))
      {
        return null;
      }
      MessageBox.Show(
        "Failed to locate assembly '" + args.Name + "'." + Environment.NewLine +
        "Note that the configuration program must be executed from/reside in the MediaPortal folder, the execution will now end.",
        "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      Application.Exit();
      return null;
    }

    private bool EnumWindowCallBack(int hwnd, int lParam)
    {
      IntPtr windowHandle = (IntPtr) hwnd;
      StringBuilder sb = new StringBuilder(1024);
      GetWindowText((int) windowHandle, sb, sb.Capacity);
      string window = sb.ToString().ToLower();
      if (window.IndexOf("mediaportal") >= 0 || window.IndexOf("media portal") >= 0)
      {
        ShowWindow(windowHandle, SW_SHOWNORMAL);
      }
      return true;
    }

    private static void CheckPrerequisites()
    {
      OSVersionInfo os = new OperatingSystemVersion();
      DialogResult res;

      string MsgNotSupported =
        "Your platform is not supported by MediaPortal Team because it lacks critical hotfixes! \nPlease check our Wiki's requirements page.";
      string MsgNotInstallable =
        "Your platform is not supported and cannot be used for MediaPortal/TV-Server! \nPlease check our Wiki's requirements page.";
      string MsgBetaServicePack =
        "You are running a BETA version of Service Pack {0}.\n Please don't do bug reporting with such configuration.";
      string ServicePack = "";
      if (!string.IsNullOrEmpty(os.OSCSDVersion))
      {
        ServicePack = " (" + os.OSCSDVersion + ")";
      }
      string MsgOsVersion = os.OSVersionString + ServicePack;

      int ver = (os.OSMajorVersion*10) + os.OSMinorVersion;

      // Disable OS if < XP
      if (ver < 51)
      {
        MessageBox.Show(MsgNotInstallable, MsgOsVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
        Application.Exit();
      }
      switch (ver)
      {
        case 51:
          if (os.OSServicePackMajor < 2)
          {
            MessageBox.Show(MsgNotInstallable, MsgOsVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
          }
          break;
        case 52:
          if (os.OSProductType == OSProductType.Workstation)
          {
            MsgOsVersion = MsgOsVersion + " [64bit]";
            MessageBox.Show(MsgNotInstallable, MsgOsVersion, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
          }
          res = MessageBox.Show(MsgNotSupported, MsgOsVersion, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
          if (res == DialogResult.Cancel)
          {
            Application.Exit();
          }
          break;
        case 60:
          if (os.OSProductType != OSProductType.Workstation || os.OSServicePackMajor < 1)
          {
            res = MessageBox.Show(MsgNotSupported, MsgOsVersion, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (res == DialogResult.Cancel)
            {
              Application.Exit();
            }
          }
          break;
      }
      if (os.OSServicePackBuild != 0)
      {
        res = MessageBox.Show(String.Format(MsgBetaServicePack, os.OSServicePackMajor), MsgOsVersion,
                              MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
        if (res == DialogResult.Cancel)
        {
          Application.Exit();
        }
      }
    }
  }
}