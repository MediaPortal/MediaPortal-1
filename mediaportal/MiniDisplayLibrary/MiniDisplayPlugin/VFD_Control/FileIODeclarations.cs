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

using System.Runtime.InteropServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.VFD_Control
{
  internal class FileIOApiDeclarations
  {
    // API declarations relating to file I/O.

    // ******************************************************************************
    // API constants
    // ******************************************************************************

    public const uint GENERIC_READ = 0x80000000;
    public const uint GENERIC_WRITE = 0x40000000;
    public const uint FILE_SHARE_READ = 0x00000001;
    public const uint FILE_SHARE_WRITE = 0x00000002;
    public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
    public const int INVALID_HANDLE_VALUE = -1;
    public const short OPEN_EXISTING = 3;
    public const int WAIT_TIMEOUT = 0x102;
    public const short WAIT_OBJECT_0 = 0;

    // ******************************************************************************
    // Structures and classes for API calls, listed alphabetically
    // ******************************************************************************

    // ******************************************************************************
    // API functions, listed alphabetically
    // ******************************************************************************

    [DllImport("kernel32.dll")]
    public static extern int CancelIo(int hFile);

    [DllImport("kernel32.dll")]
    public static extern int CloseHandle(int hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern int CreateEvent(ref SECURITY_ATTRIBUTES SecurityAttributes, int bManualReset, int bInitialState,
                                         string lpName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern int
      CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, ref SECURITY_ATTRIBUTES lpSecurityAttributes,
                 int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);

    [DllImport("kernel32.dll")]
    public static extern int ReadFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToRead,
                                      ref int lpNumberOfBytesRead, ref OVERLAPPED lpOverlapped);

    [DllImport("kernel32.dll")]
    public static extern int WaitForSingleObject(int hHandle, int dwMilliseconds);

    [DllImport("kernel32.dll")]
    public static extern int WriteFile(int hFile, ref byte lpBuffer, int nNumberOfBytesToWrite,
                                       ref int lpNumberOfBytesWritten, int lpOverlapped);

    [StructLayout(LayoutKind.Sequential)]
    public struct OVERLAPPED
    {
      #region Fields

      public int Internal;
      public int InternalHigh;
      public int Offset;
      public int OffsetHigh;
      public int hEvent;

      #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
      #region Fields

      public int nLength;
      public int lpSecurityDescriptor;
      public int bInheritHandle;

      #endregion
    }
  }
}