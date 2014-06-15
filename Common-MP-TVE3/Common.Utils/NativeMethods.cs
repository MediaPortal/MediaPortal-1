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
using System.Runtime.InteropServices;

namespace MediaPortal.Common.Utils
{
  /// <summary>
  /// This class is a static class containing definitions for system methods that are used in
  /// various places in MediaPortal.
  /// </summary>
  public static class NativeMethods
  {
    #region kernel32.dll

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
                                               IntPtr lpInBuffer, uint nInBufferSize,
                                               IntPtr lpOutBuffer, uint nOutBufferSize,
                                               out uint lpBytesReturned, IntPtr lpOverlapped);

    /// <summary>
    /// The FreeLibrary function decrements the reference count of the loaded dynamic-link library (DLL). When the reference count reaches zero, the module is unmapped from the address space of the calling process and the handle is no longer valid.
    /// </summary>
    /// <param name="hLibModule">Handle to the loaded DLL module. The LoadLibrary or GetModuleHandle function returns this handle.</param>
    /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool FreeLibrary(IntPtr hLibModule);

    /// <summary>
    /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
    /// </summary>
    /// <param name="lpModuleName">The name of the loaded module (either a .dll or .exe file).</param>
    /// <returns>If the function succeeds, the return value is a handle to the module.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    /// <summary>
    /// The GetProcAddress function retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
    /// </summary>
    /// <param name="hModule">Handle to the DLL module that contains the function or variable. The LoadLibrary or GetModuleHandle function returns this handle.</param>
    /// <param name="lpProcName">Pointer to a null-terminated string containing the function or variable name, or the function's ordinal value. If this parameter is an ordinal value, it must be in the low-order word; the high-order word must be zero.</param>
    /// <returns>If the function succeeds, the return value is the address of the exported function or variable.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    /// <summary>
    /// Determines whether the specified process is running under WOW64.
    /// </summary>
    /// <param name="hProcess">A handle to the process. The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right.</param>
    /// <param name="Wow64Process">A pointer to a value that is set to TRUE if the process is running under WOW64. If the process is running under 32-bit Windows, the value is set to FALSE. If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.</param>
    /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

    /// <summary>
    /// The LoadLibrary function maps the specified executable module into the address space of the calling process.
    /// </summary>
    /// <param name="lpLibFileName">Pointer to a null-terminated string that names the executable module (either a .dll or .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file.</param>
    /// <returns>If the function succeeds, the return value is a handle to the module.<br></br><br>If the function fails, the return value is NULL. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr LoadLibrary(string lpLibFileName);

    /// <summary>
    /// The SetDllDirectory function adds a directory to the search path used to locate DLLs for the application.
    /// </summary>
    /// <param name="PathName">Pointer to a null-terminated string that specifies the directory to be added to the search path.</param>
    /// <returns>If the function succeeds, the return value is nonzero.<br></br><br>If the function fails, the return value is zero. To get extended error information, call Marshal.GetLastWin32Error.</br></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool SetDllDirectory(string PathName);

    #endregion

    #region user32.dll

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
                                                int x, int y,
                                                int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
                                                IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr DispatchMessage(ref MSG msg);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetMessage(ref MSG msg, IntPtr hWnd, int uMsgFilterMin, int uMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int RegisterClass(ref WNDCLASS wndclass);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool TranslateMessage(ref MSG msg);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MSG
    {
      public IntPtr hwnd;
      public int message;
      public IntPtr wParam;
      public IntPtr lParam;
      public int time;
      public int pt_x;
      public int pt_y;
    }

    public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASS
    {
      public uint style;
      public WndProc lpfnWndProc;
      public int cbClsExtra;
      public int cbWndExtra;
      public IntPtr hInstance;
      public IntPtr hIcon;
      public IntPtr hCursor;
      public IntPtr hbrBackground;
      public string lpszMenuName;
      public string lpszClassName;
    }

    #endregion

    #region ksproxy.ax

    #region WinIoCtl.h

    public enum FileDevice
    {
      Beep = 0x00000001,
      CdRom = 0x00000002,
      CdRomFileSystem = 0x00000003,
      Controller = 0x00000004,
      DataLink = 0x00000005,
      Dfs = 0x00000006,
      Disk = 0x00000007,
      DiskFileSystem = 0x00000008,
      FileSystem = 0x00000009,
      InportPort = 0x0000000a,
      Keyboard = 0x0000000b,
      MailSlot = 0x0000000c,
      MidiIn = 0x0000000d,
      MidiOut = 0x0000000e,
      Mouse = 0x0000000f,
      MultiUncProvider = 0x00000010,
      NamedPipe = 0x00000011,
      Network = 0x00000012,
      NetworkBrowser = 0x00000013,
      NetworkFileSystem = 0x00000014,
      Null = 0x00000015,
      ParallelPort = 0x00000016,
      PhysicalNetcard = 0x00000017,
      Printer = 0x00000018,
      Scanner = 0x00000019,
      SerialMousePort = 0x0000001a,
      SerialPort = 0x0000001b,
      Screen = 0x0000001c,
      Sound = 0x0000001d,
      Streams = 0x0000001e,
      Tape = 0x0000001f,
      TapeFileSystem = 0x00000020,
      Transport = 0x00000021,
      Unknown = 0x00000022,
      Video = 0x00000023,
      VirtualDisk = 0x00000024,
      WaveIn = 0x00000025,
      WaveOut = 0x00000026,
      Port8042 = 0x00000027,
      NetworkRedirector = 0x00000028,
      Battery = 0x00000029,
      BusExtender = 0x0000002a,
      Modem = 0x0000002b,
      Vdm = 0x0000002c,
      MassStorage = 0x0000002d,
      Smb = 0x0000002e,
      Ks = 0x0000002f,
      DeviceChanger = 0x00000030,
      Smartcard = 0x00000031,
      Acpi = 0x00000032,
      Dvd = 0x00000033,
      FullScreenVideo = 0x00000034,
      DfsFileSystem = 0x00000035,
      DfsVolume = 0x00000036,
      Serenum = 0x00000037,
      TermSrv = 0x00000038,
      Ksec = 0x00000039,
      Fips = 0x0000003a,
      Infiniband = 0x0000003b,
      Vmbus = 0x0000003e,
      CryptProvider = 0x0000003f,
      Wpd = 0x00000040,
      Bluetooth = 0x00000041,
      MtComposite = 0x00000042,
      MtTransport = 0x00000043,
      Biometric = 0x00000044,
      Pmi = 0x00000045
    }

    /// <summary>
    /// Define the method codes for how buffers are passed for I/O and FS controls
    /// </summary>
    public enum Method
    {
      Buffered,
      InDirect,
      OutDirect,
      Neither,
      DirectToHardware = InDirect,
      DirectFromHardware = OutDirect
    }

    /// <summary>
    /// Define the access check value for any access
    /// </summary>
    /// <remarks>
    /// The FILE_READ_ACCESS and FILE_WRITE_ACCESS constants are also defined in
    /// ntioapi.h as FILE_READ_DATA and FILE_WRITE_DATA. The values for these
    /// constants *MUST* always be in sync.
    /// </remarks>
    public enum FileAccess
    {
      Any = 0,
      /// <remarks>
      /// FILE_SPECIAL_ACCESS is checked by the NT I/O system the same as FILE_ANY_ACCESS.
      /// The file systems, however, may add additional access checks for I/O and FS controls
      /// that use this value.
      /// </remarks>
      Special = Any,
      Read = 1,
      Write = 2
    }

    /// <summary>
    /// From CTL_CODE().
    /// </summary>
    public static uint CtlCode(FileDevice deviceType, uint function, Method method, FileAccess access)
    {
      return ((uint)deviceType << 16) | ((uint)access << 14) | (function << 2) | (uint)method;
    }

    #endregion

    #region ks.h

    public static readonly uint IOCTL_KS_PROPERTY = CtlCode(FileDevice.Ks, (uint)KsIoctl.Property, Method.Neither, FileAccess.Any);
    public static readonly uint IOCTL_KS_ENABLE_EVENT = CtlCode(FileDevice.Ks, (uint)KsIoctl.EnableEvent, Method.Neither, FileAccess.Any);
    public static readonly uint IOCTL_KS_DISABLE_EVENT = CtlCode(FileDevice.Ks, (uint)KsIoctl.DisableEvent, Method.Neither, FileAccess.Any);
    public static readonly uint IOCTL_KS_METHOD = CtlCode(FileDevice.Ks, (uint)KsIoctl.Method, Method.Neither, FileAccess.Any);
    public static readonly uint IOCTL_KS_WRITE_STREAM = CtlCode(FileDevice.Ks, (uint)KsIoctl.WriteStream, Method.Neither, FileAccess.Any);
    public static readonly uint IOCTL_KS_READ_STREAM = CtlCode(FileDevice.Ks, (uint)KsIoctl.ReadStream, Method.Neither, FileAccess.Any);
    public static readonly uint IOCTL_KS_RESET_STATE = CtlCode(FileDevice.Ks, (uint)KsIoctl.ResetState, Method.Neither, FileAccess.Any);

    public enum KsIoctl : uint
    {
      Property = 0,
      EnableEvent,
      DisableEvent,
      Method,
      WriteStream,
      ReadStream,
      ResetState
    }

    #endregion

    [DllImport("KsProxy.ax")]
    public static extern int KsSynchronousDeviceControl(IntPtr Handle, uint IoControl,
                                                        IntPtr InBuffer, uint InLength,
                                                        IntPtr OutBuffer, uint OutLength, out uint BytesReturned);

    #endregion
  }
}
