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
using System.Runtime.InteropServices;
using System.Threading;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;
using MediaPortal.Common.Utils;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.HauppaugeRemote
{
  /// <summary>
  /// A class for receiving remote control keypresses from Hauppauge tuners.
  /// </summary>
  public class HauppaugeRemote : BaseCustomDevice, IRemoteControlListener
  {
    #region enums

    private enum RemoteType : int
    {
      Classic21Button = 0,
      Pvr3_35Button = 0x1d,
      Pvr2_45Button = 0x1e,
      Pvr_34Button = 0x1f
    }

    #endregion

    #region DLL imports

    /*
     * Also available with version 2.66.28078:
     * - IR_Power
     * - IR_GetVersion
     * - IR_GetKeyCodeEx
     * - IR_GetSystemKeyCode
     */

    /// <summary>
    /// Register a window handle with the Hauppauge IR driver.
    /// </summary>
    [DllImport("irremote.dll")]
    private static extern bool IR_Open(int windowHandle, uint msg, bool verbose, ushort irPort);

    /// <summary>
    /// Retrieve details of a key-press event.
    /// </summary>
    [DllImport("irremote.dll")]
    private static extern bool IR_GetSystemKeyCode(out int repeatCount, out RemoteType remoteType, out int keyCode);

    /// <summary>
    /// Unregister a window handle with the Hauppauge IR driver.
    /// </summary>
    /// <param name="WindowHandle"></param>
    /// <param name="Msg"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    private static extern bool IR_Close(int windowHandle, uint Msg);

    #endregion

    #region constants

    private const string IR32_PROCESS_NAME = "Ir";
    private const string IR32_EXE_NAME = "Ir.exe";
    private const string IR32_DLL_NAME = "irremote.DLL";
    private const string MINIMUM_COMPATIBLE_VERSION = "2.49.23332";

    private const int WM_QUIT = 0x0012;
    private const int WM_TIMER = 0x0113;

    #endregion

    #region variables

    // This plugin is not tuner-specific. We use this variable to restrict to
    // one instance.
    private static bool _isLoaded = false;
    private static object _instanceLock = new object();

    private bool _isHauppaugeRemote = false;
    private bool _restartIrExe = false;
    private string _ir32Path = null;
    private volatile bool _isRemoteControlInterfaceOpen = false;
    private uint _remoteControlListenerThreadId = 0;
    private Thread _remoteControlListenerThread = null;

    #endregion

    /// <summary>
    /// Get the Hauppauge IR32 application installation path from the windows registry.
    /// </summary>
    /// <returns>the IR32 installation path if successful, otherwise <c>null</c></returns>
    private string GetIr32InstallPath()
    {
      RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Hauppauge WinTV Infrared Remote");
      if (key == null)
      {
        return null;
      }

      try
      {
        object pathValue = key.GetValue("InstallLocation");
        string path = null;
        if (pathValue != null)
        {
          path = pathValue.ToString();
          if (path.EndsWith(@"\"))
          {
            path += @"\";
          }
          return path;
        }
        pathValue = key.GetValue("UninstallString");
        if (pathValue == null)
        {
          return null;
        }
        path = pathValue.ToString();
        int index = path.ToLowerInvariant().IndexOf("unir32");
        if (index < 0)
        {
          return null;
        }
        path = path.Substring(0, index);
        if (File.Exists(path + IR32_EXE_NAME) && File.Exists(path + IR32_DLL_NAME))
        {
          return path;
        }
        return null;
      }
      finally
      {
        key.Close();
      }
    }

    #region remote control listener thread

    private void RemoteControlListener(object eventParam)
    {
      this.LogDebug("Hauppauge remote: starting remote control listener thread");
      Thread.BeginThreadAffinity();
      try
      {
        IntPtr handle;
        try
        {
          // We need a window handle to receive messages from IR32.
          NativeMethods.WNDCLASS wndclass;
          wndclass.style = 0;
          wndclass.lpfnWndProc = RemoteControlListenerWndProc;
          wndclass.cbClsExtra = 0;
          wndclass.cbWndExtra = 0;
          wndclass.hInstance = Process.GetCurrentProcess().Handle;
          wndclass.hIcon = IntPtr.Zero;
          wndclass.hCursor = IntPtr.Zero;
          wndclass.hbrBackground = IntPtr.Zero;
          wndclass.lpszMenuName = null;
          wndclass.lpszClassName = "HauppaugeRemoteListenerThreadWindowClass";

          NativeMethods.RegisterClass(ref wndclass);

          handle = NativeMethods.CreateWindowEx(0x80, wndclass.lpszClassName, "", 0x80000000, 0, 0, 0, 0, IntPtr.Zero,
                                                        IntPtr.Zero, wndclass.hInstance, IntPtr.Zero);
          if (handle.Equals(IntPtr.Zero))
          {
            this.LogError("Hauppauge remote: failed to create receive window");
            return;
          }

          // Now register the window to start receiving key press events.
          if (!IR_Open(handle.ToInt32(), 0, false, 0))
          {
            this.LogError("Hauppauge remote: failed to open interface, is other software active?");
            return;
          }

          this.LogDebug("Hauppauge remote: remote control listener thread is running");
          _remoteControlListenerThreadId = NativeMethods.GetCurrentThreadId();
          _isRemoteControlInterfaceOpen = true;
        }
        finally
        {
          ((ManualResetEvent)eventParam).Set();
        }

        // This thread needs a message loop to pump messages to our window
        // procedure.
        while (true)
        {
          try
          {
            NativeMethods.MSG msgApi = new NativeMethods.MSG();
            // This call will block until a message is received. It returns
            // false if the message is WM_QUIT.
            if (!NativeMethods.GetMessageA(ref msgApi, IntPtr.Zero, 0, 0))
            {
              if (!IR_Close(handle.ToInt32(), 0))
              {
                this.LogWarn("Hauppauge remote: failed to close interface");
              }
              return;
            }

            NativeMethods.TranslateMessage(ref msgApi);
            NativeMethods.DispatchMessageA(ref msgApi);
          }
          catch (Exception ex)
          {
            this.LogError(ex, "Hauppauge remote: remote control listener thread exception");
          }
        }
      }
      finally
      {
        Thread.EndThreadAffinity();
        this.LogDebug("Hauppauge remote: stopping remote control listener thread");
      }
    }

    private IntPtr RemoteControlListenerWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
      if (msg == WM_TIMER)  // key press event
      {
        int repeatCount = 0;
        RemoteType remoteCode = 0;
        int keyCode = 0;
        if (IR_GetSystemKeyCode(out repeatCount, out remoteCode, out keyCode))
        {
          this.LogDebug("Hauppauge remote: remote control key press, remote type = {0}, code = {1}, repeat = {2}", remoteCode, keyCode, repeatCount);
        }
      }
      return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    #endregion

    #region ICustomDevice members

    /// <summary>
    /// A human-readable name for the extension. This could be a manufacturer or reseller name, or
    /// even a model name and/or number.
    /// </summary>
    public override string Name
    {
      get
      {
        return "Hauppauge remote";
      }
    }

    /// <summary>
    /// Attempt to initialise the extension-specific interfaces used by the class. If
    /// initialisation fails, the <see ref="ICustomDevice"/> instance should be disposed
    /// immediately.
    /// </summary>
    /// <param name="tunerExternalId">The external identifier for the tuner.</param>
    /// <param name="tunerType">The tuner type (eg. DVB-S, DVB-T... etc.).</param>
    /// <param name="context">Context required to initialise the interface.</param>
    /// <returns><c>true</c> if the interfaces are successfully initialised, otherwise <c>false</c></returns>
    public override bool Initialise(string tunerExternalId, CardType tunerType, object context)
    {
      this.LogDebug("Hauppauge remote: initialising");

      lock (_instanceLock)
      {
        if (_isLoaded)
        {
          this.LogDebug("Hauppauge remote: already loaded");
          return false;
        }

        _ir32Path = GetIr32InstallPath();
        if (_ir32Path == null)
        {
          this.LogDebug("Hauppauge remote: IR32 not installed or incompatible");
          return false;
        }

        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(_ir32Path + IR32_EXE_NAME);
        if (versionInfo.FileVersion.CompareTo(MINIMUM_COMPATIBLE_VERSION) < 0)
        {
          this.LogDebug("Hauppauge remote: {0} {1} installed, please install IR32 {2} or later", IR32_EXE_NAME, versionInfo.FileVersion, MINIMUM_COMPATIBLE_VERSION);
          return false;
        }
        versionInfo = FileVersionInfo.GetVersionInfo(_ir32Path + IR32_DLL_NAME);
        if (versionInfo.FileVersion.CompareTo(MINIMUM_COMPATIBLE_VERSION) < 0)
        {
          this.LogDebug("Hauppauge remote: {0} {1} installed, please install IR32 {2} or later", IR32_DLL_NAME, versionInfo.FileVersion, MINIMUM_COMPATIBLE_VERSION);
          return false;
        }

        if (!NativeMethods.SetDllDirectory(_ir32Path))
        {
          this.LogError("Hauppauge remote: failed to add Hauppauge IR32 directory {0} to path", _ir32Path);
          return false;
        }
        _isLoaded = true;
      }

      this.LogInfo("Hauppauge remote: extension supported");
      _isHauppaugeRemote = true;
      return true;
    }

    #endregion

    #region IRemoteControlListener members

    /// <summary>
    /// Open the remote control interface and start listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully opened, otherwise <c>false</c></returns>
    public bool OpenRemoteControlInterface()
    {
      this.LogDebug("Hauppauge remote: open remote control interface");

      if (!_isHauppaugeRemote)
      {
        this.LogWarn("Hauppauge remote: not initialised or interface not supported");
        return false;
      }
      if (_isRemoteControlInterfaceOpen)
      {
        this.LogWarn("Hauppauge remote: remote control interface is already open");
        return true;
      }

      if (Process.GetProcessesByName(IR32_PROCESS_NAME).Length != 0)
      {
        // IR32 is running. We have to stop it to receive remote key presses.
        // We'll restart it when we're done.
        _restartIrExe = true;

        int i = 0;
        int processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
        this.LogDebug("Hauppauge remote: stop {0} IR32 process(es)", processCount);
        while ((Process.GetProcessesByName(IR32_PROCESS_NAME).Length != 0) && (i < processCount))
        {
          i++;
          Process.Start(_ir32Path + IR32_EXE_NAME, "/QUIT");   // upper case important
          Thread.Sleep(400);
        }
        processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
        if (processCount != 0)
        {
          this.LogWarn("Hauppauge remote: still {0} IR32 process(es), attempt terminate", processCount);
          foreach (Process proc in Process.GetProcessesByName(IR32_PROCESS_NAME))
          {
            proc.Kill();
          }
          Thread.Sleep(400);
          processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
          if (processCount != 0)
          {
            this.LogError("Hauppauge remote: failed to stop or terminate IR32, still {0} process(es)", processCount);
            return false;
          }
        }
      }

      _isRemoteControlInterfaceOpen = false;
      ManualResetEvent startEvent = new ManualResetEvent(false);
      _remoteControlListenerThread = new Thread(new ParameterizedThreadStart(RemoteControlListener));
      _remoteControlListenerThread.Name = "Hauppauge remote control listener";
      _remoteControlListenerThread.IsBackground = true;
      _remoteControlListenerThread.Priority = ThreadPriority.Lowest;
      _remoteControlListenerThread.Start(startEvent);
      startEvent.WaitOne();
      startEvent.Close();

      if (_isRemoteControlInterfaceOpen)
      {
        this.LogDebug("Hauppauge remote: result = success");
        return true;
      }
      return false;
    }

    /// <summary>
    /// Close the remote control interface and stop listening for commands.
    /// </summary>
    /// <returns><c>true</c> if the interface is successfully closed, otherwise <c>false</c></returns>
    public bool CloseRemoteControlInterface()
    {
      this.LogDebug("Hauppauge remote: close remote control interface");

      if (_isRemoteControlInterfaceOpen)
      {
        NativeMethods.PostThreadMessage(_remoteControlListenerThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
        _remoteControlListenerThread.Join();
        _remoteControlListenerThread = null;

        if (_restartIrExe)
        {
          this.LogDebug("Hauppauge remote: restart IR32 process");
          try
          {
            int processCount = Process.GetProcessesByName(IR32_PROCESS_NAME).Length;
            if (processCount == 0)
            {
              Process.Start(_ir32Path + IR32_EXE_NAME, "/QUIET");
            }
            else
            {
              this.LogWarn("Hauppauge remote: {0} IR32 process(es) already running", processCount);
            }
          }
          catch (Exception ex)
          {
            this.LogWarn(ex, "Hauppauge remote: failed to restart IR32 process");
          }
        }

        _isRemoteControlInterfaceOpen = false;
      }
      this.LogDebug("Hauppauge remote: result = success");
      return true;
    }

    #endregion

    #region IDisposable member

    /// <summary>
    /// Release and dispose all resources.
    /// </summary>
    public override void Dispose()
    {
      if (_isHauppaugeRemote)
      {
        CloseRemoteControlInterface();
        lock (_instanceLock)
        {
          _isLoaded = false;
        }
        _isHauppaugeRemote = false;
      }
    }

    #endregion
  }
}