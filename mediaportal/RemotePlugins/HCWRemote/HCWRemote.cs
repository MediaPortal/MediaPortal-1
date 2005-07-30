/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace MediaPortal
{
  /// <summary>
  /// Hauppauge HCW remote control class / by mPod
  /// all remotes are supported, if the buttons are defined in the XML file
  /// </summary>
  public class HCWRemote
  {
    bool controlEnabled;            // HCW remote enabled
    bool allowExternal;             // External processes are controlled by the Hauppauge app
    bool keepControl;               // Keep control, if MP loses focus
    bool logVerbose;                // Verbose logging
    int repeatDelay;                // Repeat delay
    bool restartIRApp     = false;  // Restart Haupp. IR-app. after MP quit
    IntPtr handlerIR;               // Window handler
    DateTime lastTime;              // Timestamp of last execution
    int lastCommand;                // Last executed command
    HCWHandler hcwHandler;

    const int IR_NOKEY               = 0x1FFF;  // No key received
    const int HCWPVR2                = 0x001E;  // 43-Button Remote
    const int HCWPVR                 = 0x001F;  // 34-Button Remote

    const int WM_TIMER               = 0x0113;
    const int WM_ACTIVATEAPP         = 0x001C;
    const int WM_ACTIVATE            = 0x0006;
    const int WM_POWERBROADCAST      = 0x0218;
    const int WA_INACTIVE            = 0;
    const int WA_ACTIVE              = 1;
    const int WA_CLICKACTIVE         = 2;
      
    const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    const int PBT_APMRESUMECRITICAL  = 0x0006;

    #region DLL-Imports

    /// <summary>
    /// The SetDllDirectory function adds a directory to the search path used to locate DLLs for the application.
    /// http://msdn.microsoft.com/library/en-us/dllproc/base/setdlldirectory.asp
    /// </summary>
    /// <param name="PathName">Pointer to a null-terminated string that specifies the directory to be added to the search path.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    static extern bool SetDllDirectory(
      string PathName);

    /// <summary>
    /// The GetLongPathName function converts the specified path to its long form.
    /// If no long path is found, this function simply returns the specified name.
    /// http://msdn.microsoft.com/library/en-us/fileio/fs/getlongpathname.asp
    /// </summary>
    /// <param name="ShortPath">Pointer to a null-terminated path to be converted.</param>
    /// <param name="LongPath">Pointer to the buffer to receive the long path.</param>
    /// <param name="Buffer">Size of the buffer.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
    static extern uint GetLongPathName(
      string ShortPath,
      [Out] StringBuilder LongPath,
      uint Buffer);

    /// <summary>
    /// Registers window handle with Hauppauge IR driver
    /// </summary>
    /// <param name="WindowHandle"></param>
    /// <param name="Msg"></param>
    /// <param name="Verbose"></param>
    /// <param name="IRPort"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    static extern bool IR_Open(
      IntPtr WindowHandle,
      uint Msg,
      bool Verbose,
      uint IRPort);

    /// <summary>
    /// Gets the received key code (new version, works for PVR-150 as well)
    /// </summary>
    /// <param name="RepeatCount"></param>
    /// <param name="RemoteCode"></param>
    /// <param name="KeyCode"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    static extern bool IR_GetSystemKeyCode(
      ref IntPtr RepeatCount,
      ref IntPtr RemoteCode,
      ref IntPtr KeyCode);

    /// <summary>
    /// Unregisters window handle from Hauppauge IR driver
    /// </summary>
    /// <param name="WindowHandle"></param>
    /// <param name="Msg"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    static extern bool IR_Close(
      IntPtr WindowHandle,
      uint Msg);

    #endregion

    /// <summary>
    /// HCW control enabled
    /// </summary>
    /// <returns>Returns true/false.</returns>
    public bool Enabled { get { return controlEnabled; } }

    /// <summary>
    /// Constructor: Initializes remote control settings
    /// </summary>
    public HCWRemote()
    {
      using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
      {
        controlEnabled = xmlreader.GetValueAsBool("remote", "HCW", false);
        allowExternal  = xmlreader.GetValueAsBool("remote", "HCWAllowExternal", false);
        keepControl    = xmlreader.GetValueAsBool("remote", "HCWKeepControl", false);
        logVerbose     = xmlreader.GetValueAsBool("remote", "HCWVerboseLog", false);
        repeatDelay    = xmlreader.GetValueAsInt ("remote", "HCWDelay", 0);
      }
      bool result = false;
      if (controlEnabled)
        hcwHandler = new HCWHandler("Hauppauge HCW", out result);
      controlEnabled = (controlEnabled && result);

      if (controlEnabled)
      {
        if (allowExternal)
        {
          Utils.OnStartExternal  += new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal  += new Utils.UtilEventHandler(OnStopExternal);
        }
        if (logVerbose) Log.Write("HCW: Repeat-delay: {0}", repeatDelay);

        try
        {
          if (!SetDllDirectory(GetDllPath()))
            Log.Write("HCW: Set DLL path failed!");
        }
        catch (Exception e)
        {
          if (logVerbose) Log.Write("HCW Exception: SetDllDirectory: " + e.Message);
        }
      }
      try
      {
        if (Process.GetProcessesByName("Ir").Length != 0)
        {
          restartIRApp = true;
        }
      }
      catch
      {
      }
    }
    
    public void Init(IntPtr hwnd)
    {
      Init();
    }

    /// <summary>
    /// Stop IR.exe and initiate HCW start
    /// </summary>
    public void Init()
    {
      if (!controlEnabled)
        return;

      try
      {
        if (Process.GetProcessesByName("Ir").Length != 0)
        {
          int i = 0;
          while ((Process.GetProcessesByName("Ir").Length != 0) && (i < 15))
          {
            i++;
            if (logVerbose) Log.Write("HCW: Terminating external control: attempt #{0}", i);
            if (Process.GetProcessesByName("Ir").Length != 0)
            {
              Process.Start(GetHCWPath() + "Ir.exe", "/QUIT");
              Thread.Sleep(200);
            }
          }
          if (Process.GetProcessesByName("Ir").Length != 0)
          {
            Log.Write("HCW: External control could not be terminated!");
          }
        }
        StartHCW();
      }
      catch (Exception e)
      {
        Log.Write("HCW: Failed to start driver components! (Not installed?)");
        if (logVerbose) Log.Write("HCW Exception: StartHCW: " + e.Message);
      }
    }


    /// <summary>
    /// Remove all events
    /// </summary>
    public void DeInit()
    {
      if (controlEnabled)
      {
        if (allowExternal)
        {
          Utils.OnStartExternal -= new Utils.UtilEventHandler(OnStartExternal);
          Utils.OnStopExternal -= new Utils.UtilEventHandler(OnStopExternal);
        }
        StopHCW();
      }
    }


    /// <summary>
    /// Start HCW control
    /// </summary>
    public void StartHCW()
    {
      if (!IR_Close(handlerIR, 0))
      {
        Log.Write("HCW: Internal control could not be terminated!");
      }
      handlerIR = GUIGraphicsContext.ActiveForm;
      if (!IR_Open(handlerIR, 0, false, 0))
      {
        Log.Write("HCW: Enabling internal control failed!");
      }
      else
      {
        if (logVerbose) Log.Write("HCW: Internal control enabled");
      }
    }


    /// <summary>
    /// Stop HCW control
    /// </summary>
    public void StopHCW()
    {
      try
      {
        if (!IR_Close(handlerIR, 0))
        {
          Log.Write("HCW: Internal control could not be terminated!");
        }
        else if ((Process.GetProcessesByName("Ir").Length == 0) && (restartIRApp))
        {
          Thread.Sleep(500);
          if (logVerbose) Log.Write("HCW: Enabling external control");
          Process.Start(GetHCWPath() + "Ir.exe", "/QUIET");
        }
      }
      catch (Exception e)
      {
        if (logVerbose) Log.Write("HCW Exception: StopHCW: " + e.Message);
      }
    }


    /// <summary>
    /// External process (e.g. myPrograms) started
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="waitForExit"></param>
    public void OnStartExternal(Process proc, bool waitForExit)
    {
      StopHCW();
    }


    /// <summary>
    /// External process (e.g. myPrograms) stopped
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="waitForExit"></param>
    public void OnStopExternal(Process proc, bool waitForExit)
    {
      Init();
    }


    /// <summary>
    /// Evaluates received messages and sends them to the mapper
    /// </summary>
    /// <param name="msg">Message</param>
    public void WndProc(Message msg)
    {
      if (controlEnabled)
      switch (msg.Msg)
      {
        case WM_POWERBROADCAST:
          if (msg.WParam.ToInt32() == PBT_APMRESUMEAUTOMATIC)
            StartHCW();
          break;

        case WM_ACTIVATE:
          if (allowExternal && !keepControl)
          {
            switch ((int)msg.WParam)
            {
              case WA_INACTIVE:
                DeInit();
                break;
              case WA_ACTIVE:
              case WA_CLICKACTIVE:
                Init();
                break;
            }
          }
          break;

//          if (((int)msg.WParam != 0) && allowExternal && !keepControl)
//          {
//            appActive = !appActive;
//            if (appActive)
//              Init();
//            else
//              DeInit();
//          }
//          break;

        case WM_TIMER:
          IntPtr repeatCount = new IntPtr();
          IntPtr remoteCode  = new IntPtr();
          IntPtr keyCode     = new IntPtr();
          try
          {
            if (IR_GetSystemKeyCode(ref repeatCount, ref remoteCode, ref keyCode))
            {
              if (logVerbose) Log.Write("HCW: Repeat Count: {0}", repeatCount.ToString());
              if (logVerbose) Log.Write("HCW: Remote Code : {0}", remoteCode.ToString());
              if (logVerbose) Log.Write("HCW: Key Code    : {0}", keyCode.ToString());
              int remoteCommand = 0;
              switch ((int) remoteCode)
              {
                case HCWPVR:
                  remoteCommand = ((int)keyCode) + 1000;
                  break;
                case HCWPVR2:
                  remoteCommand = ((int)keyCode) + 2000;
                  break;
              }
              if (((lastTime.AddMilliseconds(repeatDelay)) <= DateTime.Now) ||
                (lastCommand != remoteCommand) ||
                (repeatDelay == 0))
              {
                lastTime = DateTime.Now;
                lastCommand = remoteCommand;
                hcwHandler.MapAction(remoteCommand);
              }
            }
          }
          catch (Exception ex)
          {
            if (logVerbose) Log.Write("HCW: Driver exception: {0}", ex.Message);
          }
          break;
      }
    }


    #region Helper

    /// <summary>
    /// Converts a short to a long path.
    /// </summary>
    /// <param name="shortName">Short path</param>
    /// <returns>Long path</returns>
    static string LongPathName(string shortName)
    {
      StringBuilder longNameBuffer = new StringBuilder(256);
      uint bufferSize = (uint)longNameBuffer.Capacity;
      GetLongPathName(shortName, longNameBuffer, bufferSize);
      return longNameBuffer.ToString();
    }

    /// <summary>
    /// Get the Hauppauge IR components installation path from the windows registry.
    /// </summary>
    /// <returns>Installation path of the Hauppauge IR components</returns>
    public static string GetHCWPath()
    {
      string dllPath = null;
      try
      {
        RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Hauppauge WinTV Infrared Remote");
        dllPath = rkey.GetValue("UninstallString").ToString();
        if (dllPath.IndexOf("UNir32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNir32"));
        else if (dllPath.IndexOf("UNIR32") > 0)
          dllPath = dllPath.Substring(0, dllPath.IndexOf("UNIR32"));
      }
      catch (System.NullReferenceException)
      {
        Log.Write("HCW: Could not find registry entries for driver components! (Not installed?)");
      }
      return dllPath;
    }

    /// <summary>
    /// Returns the path of the DLL component
    /// </summary>
    /// <returns>DLL path</returns>
    public static string GetDllPath()
    {
      string dllPath = GetHCWPath();
      if (!File.Exists(dllPath + "irremote.DLL"))
      {
        dllPath = null;
      }
      return dllPath;
    }

    #endregion

  }
}