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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MediaPortal.Hooks;
using MediaPortal.Util;
using Microsoft.Win32;
//
using Hid = SharpLib.Hid;
using SharpLib.Win32;

namespace MPTray
{
  public class ShellApplication : IMessageFilter
  {
    private NotifyIcon _systemNotificationAreaIcon;
    private Hid.Handler _hidHandler;

    #region Methods

    public bool PreFilterMessage(ref Message message)
    {
      //Delay registering for HID events until we get our hWnd
      if (_hidHandler == null)
      {
        RegisterHidDevices(message.HWnd);
      }

      switch (message.Msg)
      {
          case Const.WM_INPUT:
              //Returning zero means we processed that message.
              //message.Result = new IntPtr(0);
              _hidHandler.ProcessInput(ref message);
              break;
      }

      //Should we somehow determine if we received start button and return true then?
      //Would that change anything? Guess not.
      return false;
    }

    private void InstallKeyboardHook()
    {
      Log.Write("MPTray: InstallKeyboardHook");
      _keyboardHook = new KeyboardHook();
      _keyboardHook.KeyDown += OnKeyDown;
      _keyboardHook.KeyUp += OnKeyUp;
      _keyboardHook.IsEnabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private static void SwitchFocus()
    {
      Log.Write("MPTray: SwitchFocus");

      Process[] processes = Process.GetProcessesByName("mediaportal");

      if (processes.Length > 0)
      {
        IntPtr handle = processes[0].MainWindowHandle;

        // Make MediaPortal window normal ( if minimized )
        Win32API.ShowWindow(handle, Win32API.ShowWindowFlags.ShowNormal);

        // Make Mediaportal window focused
        if (Win32API.SetForegroundWindow(handle, true))
        {
          Log.Write("MPTray: Successfully switched focus.");
        }
      }
      else
      {
        Log.Write("MPTray: MediaPortal is not running (yet).");
      }
    }

    /// <summary>
    /// Register HID devices so that we receive corresponding WM_INPUT messages.
    /// </summary>
    protected void RegisterHidDevices(IntPtr aHwnd)
    {
      // Register the input device to receive the commands from the
      // remote device. See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnwmt/html/remote_control.asp
      // for the vendor defined usage page.

      RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];

      int i = 0;
      rid[i].usUsagePage = (ushort)SharpLib.Hid.UsagePage.WindowsMediaCenterRemoteControl;
      rid[i].usUsage = (ushort)SharpLib.Hid.UsageCollection.WindowsMediaCenter.WindowsMediaCenterRemoteControl;
      rid[i].dwFlags = RawInputDeviceFlags.RIDEV_INPUTSINK;
      rid[i].hwndTarget = aHwnd; //Process.GetCurrentProcess().MainWindowHandle;

      _hidHandler = new SharpLib.Hid.Handler(rid);
      if (!_hidHandler.IsRegistered)
      {
        Log.Write("Failed to register raw input devices: " + Marshal.GetLastWin32Error().ToString());
      }
      _hidHandler.OnHidEvent += HandleHidEvent;
    }

    /// <summary>
    /// Here we receive HID events from our HID library.
    /// </summary>
    /// <param name="aSender"></param>
    /// <param name="aHidEvent"></param>
    public void HandleHidEvent(object aSender, SharpLib.Hid.Event aHidEvent)
    {
      Log.Write("MPTray: HandleHidEvent");

      if (aHidEvent.IsStray)
      {
        //Stray event just ignore it
        return;
      }

      
        //We are in the proper thread
        if (!(aHidEvent.Usages.Count > 0
            && aHidEvent.UsagePage == (ushort)Hid.UsagePage.WindowsMediaCenterRemoteControl
            && aHidEvent.Usages[0] == (ushort)Hid.Usage.WindowsMediaCenterRemoteControl.GreenStart))
        //&& aHidEvent.UsagePage == (ushort)Hid.UsagePage.Consumer
        //&& aHidEvent.Usages[0] == (ushort)Hid.Usage.ConsumerControl.ThinkPadFullscreenMagnifier)
        {
          //Discard anything but the Green Start Button
          return;
        }

      DoStart();

    }

    /// <summary>
    /// 
    /// </summary>
    private void DoStart()
    {
      Process[] processes = Process.GetProcessesByName("mediaportal");

      if (processes.Length != 0)
      {
        if (processes.Length > 1)
        {
          Log.Write("MPTray: More than one window named \"MediaPortal\" has been found!");
          foreach (Process procName in processes)
          {
            Log.Write("MPTray: {0} (Started: {1}, ID: {2})", procName.ProcessName,
                      procName.StartTime.ToShortTimeString(), procName.Id);
          }
        }
        Log.Write("MPTray: MediaPortal is already running - switching focus.");
        SwitchFocus();
      }
      else
      {
        try
        {
          Uri uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);

          Process process = new Process
          {
            StartInfo =
            {
              FileName = "mediaportal.exe",
              WorkingDirectory = Path.GetDirectoryName(uri.LocalPath),
              UseShellExecute = true
            }
          };

          Log.Write("MPTray: starting MediaPortal");
          process.Start();
        }
        catch (Exception ex)
        {
          Log.Write("MPTray: Error starting MediaPortal {0}", ex.Message);
        }
      }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      Log.Write("MPTray: OnKeyDown");
      if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
      {
        _windowsKeyPressed = true;
      }

      if (_windowsKeyPressed && e.KeyCode == Keys.E)
      {
        Process[] processes = Process.GetProcessesByName("explorer");

        if (processes.Length == 0)
          Process.Start("explorer.exe");
      }

      if (_windowsKeyPressed && e.KeyCode == Keys.S)
      {
        DoStart();
        e.Handled = true;
      }

      if (_windowsKeyPressed && e.KeyCode == Keys.K)
      {
        TerminateProcess("mediaportal");
        e.Handled = true;
      }

      if (_windowsKeyPressed && (e.KeyCode == Keys.T || e.KeyCode == Keys.K || e.KeyCode != Keys.M))
      {
        IntPtr handle = Win32API.FindWindow("Shell_TrayWnd", null);

        if (handle != IntPtr.Zero)
        {
          if (Win32API.IsWindowVisible(handle) == false)
          {
            Win32API.ShowWindow(handle, Win32API.ShowWindowFlags.ShowNotActivated);
          }
          if (Win32API.IsWindowEnabled(handle) == false)
          {
            Win32API.EnableWindow(handle, 1);
          }
        }

        e.Handled = e.KeyCode != Keys.M;
      }

      if (_windowsKeyPressed && e.KeyCode == Keys.C)
      {
        Uri uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);

        Process.Start(Path.GetDirectoryName(uri.LocalPath) + @"\configuration.exe");

        e.Handled = true;
      }

      if (e.KeyCode == Keys.Escape && (e.Modifiers & Keys.Control | Keys.Shift) == (Keys.Control | Keys.Shift))
        Process.Start("taskmgr.exe");

      if (e.KeyCode == Keys.Delete && (e.Modifiers & Keys.Control | Keys.Alt) == (Keys.Control | Keys.Alt))
        Process.Start("taskmgr.exe");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      Log.Write("MPTray: OnKeyUp");
      if (e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin)
        _windowsKeyPressed = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="register"></param>
    /// <param name="hive"></param>
    private static void Register(bool register, RegistryKey hive)
    {
      Log.Write("MPTray: Register");
      try
      {
        if (register)
        {
          RegistryKey key = hive.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
          if (key != null)
          {
            key.SetValue("MediaPortal Shell", Application.ExecutablePath);
          }
        }
        else
        {
          RegistryKey key = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
          if (key != null)
          {
            key.DeleteValue("MediaPortal Shell");
          }
        }
      }
      catch (Exception e)
      {
        Log.Write("MPTray: Failed to modify autostart entry {0}", e.ToString());
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Run()
    {
      try
      {
        try
        {
          Thread.CurrentThread.Name = "MPTray";
        }
        catch (InvalidOperationException) {}

        Log.Open("MPTray.log");
        Log.Write("MPTray: Starting...");

        if (TerminateProcess("ehtray"))
        {
          Log.Write("MPTray: Terminating running instance(s) of ehtray.exe");
        }

        // reduce the memory footprint of the app
        Process process = Process.GetCurrentProcess();

        process.MaxWorkingSet = (IntPtr)900000;
        process.MinWorkingSet = (IntPtr)300000;

        InitTrayIcon();        
        Application.AddMessageFilter(this);
        Application.Run();
      }
      catch (Exception e)
      {
        Log.Write("MPTray: Error on startup {0}", e.ToString());
      }

      if (_keyboardHook != null)
      {
        Log.Write("MPTray: Disabling keyboard hook");
        _keyboardHook.IsEnabled = false;
      }

      Log.Write("MPTray: Exiting...");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="processName"></param>
    /// <returns></returns>
    private static bool TerminateProcess(string processName)
    {
      Log.Write("MPTray: TerminateProcess");

      bool terminatedProcess = false;

      try
      {
        foreach (Process process in Process.GetProcessesByName(processName))
        {
          if (process != Process.GetCurrentProcess())
          {
            process.Kill();
            process.Close();

            if (terminatedProcess == false)
              terminatedProcess = true;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write("MPTray: Error while terminating process(es): {0}, {1}", processName, ex.ToString());
      }

      return terminatedProcess;
    }

    #endregion Methods

    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary> 
    [STAThread]
    private static void Main(string[] args)
    {
      ShellApplication handler = new ShellApplication();

      foreach (string arg in args)
      {
        switch (arg.ToLowerInvariant())
        {
          case "/shell":
          case "-shell":
            handler.InstallKeyboardHook();
            break;
          case "/noicon":
          case "-noicon":
            break;
          case "/kill":
          case "-kill":
            TerminateProcess("mptray");
            return;
          case "/register":
          case "-register":
          case "/register:user":
          case "-register:user":
            Register(true, Registry.CurrentUser);
            return;
          case "/register:all":
          case "-register:all":
            Register(true, Registry.LocalMachine);
            return;
          case "/unregister":
          case "-unregister":
          case "/unregister:user":
          case "-unregister:user":
            Register(false, Registry.CurrentUser);
            return;
          case "/unregister:all":
          case "-unregister:all":
            Register(false, Registry.LocalMachine);
            return;
          case "/unregister:both":
          case "-unregister:both":
            Register(false, Registry.CurrentUser);
            Register(false, Registry.LocalMachine);
            return;
          default:
            Log.Write("MPTray: Ignoring unknown command line parameter: '{0}'", arg);
            break;
        }
      }

      Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

      if (processes.Length != 1)
      {
        Log.Write("MPTray: Another instance of MPTray is already running");
        return;
      }

      handler.Run();
    }

    /// <summary>
    /// 
    /// </summary>
    private void InitTrayIcon()
    {
      Log.Write("MPTray: InitTrayIcon");
      if (_systemNotificationAreaIcon == null)
      {
        try
        {
          ContextMenu contextMenuTray = new ContextMenu();
          MenuItem menuItem1 = new MenuItem();

          // Initialize contextMenuTray
          contextMenuTray.MenuItems.AddRange(new[] {menuItem1});

          // Initialize menuItem1
          menuItem1.Index = 0;
          menuItem1.Text = "Close";
          menuItem1.Click += MenuItem1Click;

          _systemNotificationAreaIcon = new NotifyIcon
                                          {
                                            ContextMenu = contextMenuTray,
                                            Icon = Properties.Resources.MPTrayIcon,
                                            Text = "MediaPortal Tray Launcher",
                                            Visible = true
                                          };
        }
        catch (Exception ex)
        {
          Log.Write("MPTray: Could not init tray icon - {0}", ex.ToString());
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem1Click(object sender, EventArgs e)
    {
      Log.Write("MPTray: MenuItem1Click");
      _systemNotificationAreaIcon.Visible = false;
      _systemNotificationAreaIcon = null;
      Application.Exit();
    }

    #endregion Entry Point

    #region Fields

    private KeyboardHook _keyboardHook;
    private bool _windowsKeyPressed;

    #endregion Fields

    #region Log

    /// <summary>
    /// 
    /// </summary>
    public class Log
    {
      private static StreamWriter _streamWriter;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileName"></param>
      public static void Open(string fileName)
      {
        if (_streamWriter != null)
        {
          return;
        }

        string filePath =
          Path.Combine(MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Log), fileName);
        if (File.Exists(filePath))
        {
          try
          {
            string backup = Path.ChangeExtension(filePath, ".bak");

            if (File.Exists(backup))
            {
              File.Delete(backup);
            }
            File.Move(filePath, backup);
          }
          catch {}
        }

        try
        {
          _streamWriter = new StreamWriter(filePath, false)
                            {
                              AutoFlush = true
                            };
          string message = String.Format("{0:yyyy-MM-dd HH:mm:ss.ffffff} - {1}: Log Opened", DateTime.Now,
                                         Thread.CurrentThread.Name);
          _streamWriter.WriteLine(message);

          message = String.Format("{0:yyyy-MM-dd HH:mm:ss.ffffff} - {1}: {2}", DateTime.Now, Thread.CurrentThread.Name,
                                  FileVersionInfo.GetVersionInfo(Application.ExecutablePath).FileVersion);
          _streamWriter.WriteLine(message);
        }
        catch {}
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileName"></param>
      public static void ReOpen(string fileName)
      {
        if (_streamWriter != null)
        {
          return;
        }

        string filePath =
          Path.Combine(MediaPortal.Configuration.Config.GetFolder(MediaPortal.Configuration.Config.Dir.Log), fileName);


        try
        {
          _streamWriter = new StreamWriter(filePath, true)
                            {
                              AutoFlush = true
                            };
        }
        catch {}
      }

      /// <summary>
      /// 
      /// </summary>
      public static void Close()
      {
        if (_streamWriter == null)
        {
          return;
        }
        _streamWriter.Dispose();
        _streamWriter = null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="format"></param>
      /// <param name="args"></param>
      public static void Write(string format, params object[] args)
      {
        Log.ReOpen("MPTray.log");
        if (_streamWriter == null)
        {
          return;
        }
        string message = String.Format("{0:yyyy-MM-dd HH:mm:ss.ffffff} - ", DateTime.Now) + String.Format(format, args);

        _streamWriter.WriteLine(message);
        Log.Close();
      }
    }

    #endregion
  }
}