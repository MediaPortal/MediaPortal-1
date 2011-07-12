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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Profile;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for Startup.
  /// </summary>
  public class Startup
  {
    public static bool _automaticMovieCodec = false;
    public static bool _automaticBDCodec = false;

    private enum StartupMode
    {
      Normal
    }

    private StartupMode startupMode = StartupMode.Normal;
    private string sectionsConfiguration = string.Empty;
    private bool _avoidVersionChecking = false;
    private bool _debugOptions = false;
    private bool _preventGUILaunch = false;

    private const string mpMutex = "{E0151CBA-7F81-41df-9849-F5298A779EB3}";
    private const string configMutex = "{0BFD648F-A59F-482A-961B-337D70968611}";

    public Startup(string[] arguments)
    {
      Thread.CurrentThread.Name = "Config Main";
      Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

      // Logger should write into Configuration.log
      Log.SetConfigurationMode();
      Log.BackupLogFile(LogType.Config);

      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

      Log.Info("Configuration v" + versionInfo.FileVersion + " is starting up on " + OSInfo.OSInfo.GetOSDisplayVersion());
#if DEBUG
      Log.Info("Debug build: " + Application.ProductVersion);
#else
      Log.Info("Build: " + Application.ProductVersion);
#endif

      //Check for unsupported operating systems
      OSPrerequisites.OSPrerequisites.OsCheck(true);

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
        Log.Info("{0} - {1}", options, Config.GetFolder((Config.Dir)Enum.Parse(typeof (Config.Dir), options)));
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
            //Log.Debug("Startup: Argument did request Wizard mode - {0}", trimmedArgument);
            Log.Warn("Startup: Wizard mode invoked but currently disabled: argument ignored!");
          }

          if (trimmedArgument.StartsWith("/section"))
          {
            string[] subArguments = argument.Split('=');

            if (subArguments.Length >= 2)
            {
              sectionsConfiguration = subArguments[1];
            }
          }

          //  deploymode used to upgrade the configuration files
          if (trimmedArgument == "--deploymode")
          {
            Log.Info("Running in deploy mode - upgrading config file");

            try
            {
              ISettingsProvider mpConfig = new XmlSettingsProvider(MPSettings.ConfigPathName);
              SettingsUpgradeManager.Instance.UpgradeToLatest(mpConfig);
            }
            catch (Exception ex)
            {
              Log.Error("Unhandled exception when upgrading config file '" + MPSettings.ConfigPathName + "'\r\n\r\n" + ex.ToString());
            }
            finally
            {
              _preventGUILaunch = true;
            }
          }

          if (trimmedArgument == "/debugoptions")
          {
            _debugOptions = true;
          }

          if (trimmedArgument.ToLowerInvariant() == "/avoidversioncheck")
          {
            _avoidVersionChecking = true;
            Log.Warn("Version check is disabled by command line switch \"/avoidVersionCheck\"");
          }

        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Start()
    {
      //  If the application GUI shouldn't be loaded
      if (_preventGUILaunch)
        return;

      using (ProcessLock processLock = new ProcessLock(configMutex))
      {
        if (processLock.AlreadyExists)
        {
          Log.Warn("Main: Configuration is already running");
          Win32API.ActivatePreviousInstance();
        }

        // Check for a MediaPortal Instance running and don't allow Configuration to start
        using (ProcessLock mpLock = new ProcessLock(mpMutex))
        {
          if (mpLock.AlreadyExists)
          {
            DialogResult dialogResult = MessageBox.Show(
              "MediaPortal has to be closed for configuration.\nClose MediaPortal and start Configuration?",
              "MediaPortal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
              Util.Utils.KillProcess("Watchdog");
              Util.Utils.KillProcess("MediaPortal");
              Log.Info("MediaPortal closed, continue running Configuration.");
            }
            else
            {
              Log.Warn("Main: MediaPortal is running - start of Configuration aborted");
              return;
            }
          }
        }

        string MpConfig = Assembly.GetExecutingAssembly().Location;
#if !DEBUG
  // Check TvPlugin version
        string tvPlugin = Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll";
        if (File.Exists(tvPlugin) && !_avoidVersionChecking)
        {
          string tvPluginVersion = FileVersionInfo.GetVersionInfo(tvPlugin).ProductVersion;
          string CfgVersion = FileVersionInfo.GetVersionInfo(MpConfig).ProductVersion;
          if (CfgVersion != tvPluginVersion)
          {
            string strLine = "TvPlugin and MediaPortal don't have the same version.\r\n";
            strLine += "Please update the older component to the same version as the newer one.\r\n";
            strLine += "MpConfig Version: " + CfgVersion + "\r\n";
            strLine += "TvPlugin Version: " + tvPluginVersion;
            MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Log.Info(strLine);
            return;
          }
        }
#endif

        FileInfo mpFi = new FileInfo(MpConfig);
        Log.Info("Assembly creation time: {0} (UTC)", mpFi.LastWriteTimeUtc.ToUniversalTime());

        Form applicationForm = null;

        Thumbs.CreateFolders();

        switch (startupMode)
        {
          case StartupMode.Normal:
            Log.Info("Create new standard setup");
            applicationForm = new SettingsForm(_debugOptions);
            break;
        }

        if (applicationForm != null)
        {
          Log.Info("start application");
          Application.Run(applicationForm);
        }
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
  }
}