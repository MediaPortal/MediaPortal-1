#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.IO;
using System.Runtime.InteropServices;
using MediaPortal.GUI.Library;
using Microsoft.Win32;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Wrapper class for irremote.dll
  /// </summary>
  public static class irremote
  {
    #region DLL-Imports

    /// <summary>
    /// The SetDllDirectory function adds a directory to the search path used to locate DLLs for the application.
    /// http://msdn.microsoft.com/library/en-us/dllproc/base/setdlldirectory.asp
    /// </summary>
    /// <param name="PathName">Pointer to a null-terminated string that specifies the directory to be added to the search path.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll")]
    private static extern bool SetDllDirectory(
      string PathName);

    /// <summary>
    /// Registers window handle with Hauppauge IR driver
    /// </summary>
    /// <param name="WindowHandle"></param>
    /// <param name="Msg"></param>
    /// <param name="Verbose"></param>
    /// <param name="IRPort"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    private static extern bool IR_Open(
      int WindowHandle,
      uint Msg,
      bool Verbose,
      ushort IRPort);

    /// <summary>
    /// Gets the received key code (new version, works for PVR-150 as well)
    /// </summary>
    /// <param name="RepeatCount"></param>
    /// <param name="RemoteCode"></param>
    /// <param name="KeyCode"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    private static extern bool IR_GetSystemKeyCode(
      out int RepeatCount,
      out int RemoteCode,
      out int KeyCode);

    /// <summary>
    /// Unregisters window handle from Hauppauge IR driver
    /// </summary>
    /// <param name="WindowHandle"></param>
    /// <param name="Msg"></param>
    /// <returns></returns>
    [DllImport("irremote.dll")]
    private static extern bool IR_Close(
      int WindowHandle,
      uint Msg);

    #endregion

    public static string CurrentVersion = "2.49.23332";

    public static bool IRClose(IntPtr WindowHandle, uint Msg)
    {
      return IR_Close((int)WindowHandle, Msg);
    }

    public static bool IRGetSystemKeyCode(out int RepeatCount, out int RemoteCode, out int KeyCode)
    {
      RepeatCount = 0;
      RemoteCode = 0;
      KeyCode = 0;
      bool result = false;
      try
      {
        result = IR_GetSystemKeyCode(out RepeatCount, out RemoteCode, out KeyCode);
      }
      catch (AccessViolationException) {}
      catch (Exception ex)
      {
        Log.Info("HCW: Exception while querying remote: {0}", ex.Message);
      }
      return result;
    }

    public static bool IROpen(IntPtr WindowHandle, uint Msg, bool Verbose, ushort IRPort)
    {
      return IR_Open((int)WindowHandle, Msg, Verbose, IRPort);
    }

    public static bool IRSetDllDirectory(string PathName)
    {
      return SetDllDirectory(PathName);
    }

    /// <summary>
    /// Get the Hauppauge IR components installation path from the windows registry.
    /// </summary>
    /// <returns>Installation path of the Hauppauge IR components</returns>
    public static string GetHCWPath()
    {
      string dllPath = null;
      using (
        RegistryKey rkey =
          Registry.LocalMachine.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Hauppauge WinTV Infrared Remote"))
      {
        if (rkey != null)
        {
          dllPath = rkey.GetValue("UninstallString").ToString();
          if (dllPath.IndexOf("UNir32") > 0)
          {
            dllPath = dllPath.Substring(0, dllPath.IndexOf("UNir32"));
          }
          else if (dllPath.IndexOf("UNIR32") > 0)
          {
            dllPath = dllPath.Substring(0, dllPath.IndexOf("UNIR32"));
          }
        }
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
  }
}