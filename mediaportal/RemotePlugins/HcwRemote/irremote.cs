#region Copyright (C) 2005-2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal - Author: mPod
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
using System.Text;
using System.Runtime.InteropServices;

namespace MediaPortal.InputDevices
{
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
    static extern bool SetDllDirectory(
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


    public class IRFailedException : Exception
    {
      const string message = "Can't open IR device - IR in use?";
      public IRFailedException()
        : base(message)
      { }
    }

    public class IRNoMessage : Exception
    {
      const string message = "No IR command queued.";
      public IRNoMessage()
        : base(message)
      { }
    }

    public static void IRClose(IntPtr WindowHandle, uint Msg)
    {
      bool result = IR_Close(WindowHandle, Msg);
    }

    public static void IRGetSystemKeyCode(ref IntPtr RepeatCount, ref IntPtr RemoteCode, ref IntPtr KeyCode)
    {
      bool result = IR_GetSystemKeyCode(ref RepeatCount, ref RemoteCode, ref KeyCode);
      if (!result)
        throw new IRNoMessage();
    }

    public static void IROpen(IntPtr WindowHandle, uint Msg, bool Verbose, uint IRPort)
    {
      bool result = IR_Open(WindowHandle, Msg, Verbose, IRPort);
      if (!result)
        throw new IRFailedException();
    }

    public static bool IRSetDllDirectory(string PathName)
    {
      return SetDllDirectory(PathName);
    }
  }
}
