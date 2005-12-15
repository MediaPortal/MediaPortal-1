using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace irremote
{
  static class irremote
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
    /// The GetLongPathName function converts the specified path to its long form.
    /// If no long path is found, this function simply returns the specified name.
    /// http://msdn.microsoft.com/library/en-us/fileio/fs/getlongpathname.asp
    /// </summary>
    /// <param name="ShortPath">Pointer to a null-terminated path to be converted.</param>
    /// <param name="LongPath">Pointer to the buffer to receive the long path.</param>
    /// <param name="Buffer">Size of the buffer.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
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

    public static uint IRGetLongPathName(string ShortPath, [Out] StringBuilder LongPath, uint Buffer)
    {
      return GetLongPathName(ShortPath, LongPath, Buffer);
    }

    public static bool IRClose(IntPtr WindowHandle, uint Msg)
    {
      return IR_Close(WindowHandle, Msg);
    }

    public static bool IRGetSystemKeyCode(ref IntPtr RepeatCount, ref IntPtr RemoteCode, ref IntPtr KeyCode)
    {
      return IR_GetSystemKeyCode(ref RepeatCount, ref RemoteCode, ref KeyCode);
    }

    public static bool IROpen(IntPtr WindowHandle, uint Msg, bool Verbose, uint IRPort)
    {
      return IR_Open(WindowHandle, Msg, Verbose, IRPort);
    }

    public static bool IRSetDllDirectory(string PathName)
    {
      return SetDllDirectory(PathName);
    }
  }
}
