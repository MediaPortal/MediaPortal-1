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
using System.Runtime.InteropServices;

namespace MediaPortal.Ripper
{
  /// <summary>
  /// Wrapper class for Win32 functions and structures needed to handle CD.
  /// </summary>
  internal class Win32Functions
  {
    public enum DriveTypes : uint
    {
      DRIVE_UNKNOWN = 0,
      DRIVE_NO_ROOT_DIR,
      DRIVE_REMOVABLE,
      DRIVE_FIXED,
      DRIVE_REMOTE,
      DRIVE_CDROM,
      DRIVE_RAMDISK
    } ;

    [DllImport("Kernel32.dll")]
    public static extern DriveTypes GetDriveType(string drive);

    //DesiredAccess values
    public const uint GENERIC_READ = 0x80000000;
    public const uint GENERIC_WRITE = 0x40000000;
    public const uint GENERIC_EXECUTE = 0x20000000;
    public const uint GENERIC_ALL = 0x10000000;

    //Share constants
    public const uint FILE_SHARE_READ = 0x00000001;
    public const uint FILE_SHARE_WRITE = 0x00000002;
    public const uint FILE_SHARE_DELETE = 0x00000004;

    //CreationDisposition constants
    public const uint CREATE_NEW = 1;
    public const uint CREATE_ALWAYS = 2;
    public const uint OPEN_EXISTING = 3;
    public const uint OPEN_ALWAYS = 4;
    public const uint TRUNCATE_EXISTING = 5;

    /// <summary>
    /// Win32 CreateFile function, look for complete information at Platform SDK
    /// </summary>
    /// <param name="FileName">In order to read CD data FileName must be "\\.\\D:" where D is the CDROM drive letter</param>
    /// <param name="DesiredAccess">Must be GENERIC_READ for CDROMs others access flags are not important in this case</param>
    /// <param name="ShareMode">O means exlusive access, FILE_SHARE_READ allow open the CDROM</param>
    /// <param name="lpSecurityAttributes">See Platform SDK documentation for details. NULL pointer could be enough</param>
    /// <param name="CreationDisposition">Must be OPEN_EXISTING for CDROM drives</param>
    /// <param name="dwFlagsAndAttributes">0 in fine for this case</param>
    /// <param name="hTemplateFile">NULL handle in this case</param>
    /// <returns>INVALID_HANDLE_VALUE on error or the handle to file if success</returns>
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateFile(string FileName, uint DesiredAccess,
                                           uint ShareMode, IntPtr lpSecurityAttributes,
                                           uint CreationDisposition, uint dwFlagsAndAttributes,
                                           IntPtr hTemplateFile);

    /// <summary>
    /// The CloseHandle function closes an open object handle.
    /// </summary>
    /// <param name="hObject">Handle to an open object.</param>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int CloseHandle(IntPtr hObject);

    public const uint IOCTL_CDROM_READ_TOC = 0x00024000;
    public const uint IOCTL_STORAGE_CHECK_VERIFY = 0x002D4800;
    public const uint IOCTL_CDROM_RAW_READ = 0x0002403E;
    public const uint IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;
    public const uint IOCTL_STORAGE_EJECT_MEDIA = 0x002D4808;
    public const uint IOCTL_STORAGE_LOAD_MEDIA = 0x002D480C;

    /// <summary>
    /// Most general form of DeviceIoControl Win32 function
    /// </summary>
    /// <param name="hDevice">Handle of device opened with CreateFile, <see cref="Ripper.Win32Functions.CreateFile"/></param>
    /// <param name="IoControlCode">Code of DeviceIoControl operation</param>
    /// <param name="lpInBuffer">Pointer to a buffer that contains the data required to perform the operation.</param>
    /// <param name="InBufferSize">Size of the buffer pointed to by lpInBuffer, in bytes.</param>
    /// <param name="lpOutBuffer">Pointer to a buffer that receives the operation's output data.</param>
    /// <param name="nOutBufferSize">Size of the buffer pointed to by lpOutBuffer, in bytes.</param>
    /// <param name="lpBytesReturned">Receives the size, in bytes, of the data stored into the buffer pointed to by lpOutBuffer. </param>
    /// <param name="lpOverlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int DeviceIoControl(IntPtr hDevice, uint IoControlCode,
                                             IntPtr lpInBuffer, uint InBufferSize,
                                             IntPtr lpOutBuffer, uint nOutBufferSize,
                                             ref uint lpBytesReturned,
                                             IntPtr lpOverlapped);

    [StructLayout(LayoutKind.Sequential)]
    public struct TRACK_DATA
    {
      public byte Reserved;
      private byte BitMapped;

      public byte Control
      {
        get { return (byte)(BitMapped & 0x0F); }
        set { BitMapped = (byte)((BitMapped & 0xF0) | (value & (byte)0x0F)); }
      }

      public byte Adr
      {
        get { return (byte)((BitMapped & (byte)0xF0) >> 4); }
        set { BitMapped = (byte)((BitMapped & (byte)0x0F) | (value << 4)); }
      }

      public byte TrackNumber;
      public byte Reserved1;

      /// <summary>
      /// Don't use array to avoid array creation
      /// </summary>
      public byte Address_0;

      public byte Address_1;
      public byte Address_2;
      public byte Address_3;
    } ;

    public const int MAXIMUM_NUMBER_TRACKS = 100;

    [StructLayout(LayoutKind.Sequential)]
    public class TrackDataList
    {
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXIMUM_NUMBER_TRACKS * 8)] private byte[] Data;

      public TRACK_DATA this[int Index]
      {
        get
        {
          if ((Index < 0) | (Index >= MAXIMUM_NUMBER_TRACKS))
          {
            throw new IndexOutOfRangeException();
          }
          TRACK_DATA res;
          GCHandle handle = GCHandle.Alloc(Data, GCHandleType.Pinned);
          try
          {
            IntPtr buffer = handle.AddrOfPinnedObject();
            buffer = (IntPtr)(buffer.ToInt32() + (Index * Marshal.SizeOf(typeof (TRACK_DATA))));
            res = (TRACK_DATA)Marshal.PtrToStructure(buffer, typeof (TRACK_DATA));
          }
          finally
          {
            handle.Free();
          }
          return res;
        }
      }

      public TrackDataList()
      {
        Data = new byte[MAXIMUM_NUMBER_TRACKS * Marshal.SizeOf(typeof (TRACK_DATA))];
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CDROM_TOC
    {
      public ushort Length;
      public byte FirstTrack = 0;
      public byte LastTrack = 0;

      public TrackDataList TrackData;

      public CDROM_TOC()
      {
        TrackData = new TrackDataList();
        Length = (ushort)Marshal.SizeOf(this);
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PREVENT_MEDIA_REMOVAL
    {
      public byte PreventMediaRemoval = 0;
    }

    public enum TRACK_MODE_TYPE
    {
      YellowMode2,
      XAForm2,
      CDDA
    }

    [StructLayout(LayoutKind.Sequential)]
    public class RAW_READ_INFO
    {
      public long DiskOffset = 0;
      public uint SectorCount = 0;
      public TRACK_MODE_TYPE TrackMode = TRACK_MODE_TYPE.CDDA;
    }

    /// <summary>
    /// Overload version of DeviceIOControl to read the TOC (Table of contents)
    /// </summary>
    /// <param name="hDevice">Handle of device opened with CreateFile, <see cref="Ripper.Win32Functions.CreateFile"/></param>
    /// <param name="IoControlCode">Must be IOCTL_CDROM_READ_TOC for this overload version</param>
    /// <param name="InBuffer">Must be <code>IntPtr.Zero</code> for this overload version </param>
    /// <param name="InBufferSize">Must be 0 for this overload version</param>
    /// <param name="OutTOC">TOC object that receive the CDROM TOC</param>
    /// <param name="OutBufferSize">Must be <code>(UInt32)Marshal.SizeOf(CDROM_TOC)</code> for this overload version</param>
    /// <param name="BytesReturned">Receives the size, in bytes, of the data stored into OutTOC</param>
    /// <param name="Overlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int DeviceIoControl(IntPtr hDevice, uint IoControlCode,
                                             IntPtr InBuffer, uint InBufferSize,
                                             [Out] CDROM_TOC OutTOC, uint OutBufferSize,
                                             ref uint BytesReturned,
                                             IntPtr Overlapped);

    /// <summary>
    /// Overload version of DeviceIOControl to lock/unlock the CD
    /// </summary>
    /// <param name="hDevice">Handle of device opened with CreateFile, <see cref="Ripper.Win32Functions.CreateFile"/></param>
    /// <param name="IoControlCode">Must be IOCTL_STORAGE_MEDIA_REMOVAL for this overload version</param>
    /// <param name="InMediaRemoval">Set the lock/unlock state</param>
    /// <param name="InBufferSize">Must be <code>(UInt32)Marshal.SizeOf(PREVENT_MEDIA_REMOVAL)</code> for this overload version</param>
    /// <param name="OutBuffer">Must be <code>IntPtr.Zero</code> for this overload version </param>
    /// <param name="OutBufferSize">Must be 0 for this overload version</param>
    /// <param name="BytesReturned">A "dummy" varible in this case</param>
    /// <param name="Overlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int DeviceIoControl(IntPtr hDevice, uint IoControlCode,
                                             [In] PREVENT_MEDIA_REMOVAL InMediaRemoval, uint InBufferSize,
                                             IntPtr OutBuffer, uint OutBufferSize,
                                             ref uint BytesReturned,
                                             IntPtr Overlapped);

    /// <summary>
    /// Overload version of DeviceIOControl to read digital data
    /// </summary>
    /// <param name="hDevice">Handle of device opened with CreateFile, <see cref="Ripper.Win32Functions.CreateFile"</param>
    /// <param name="IoControlCode">Must be IOCTL_CDROM_RAW_READ for this overload version</param>
    /// <param name="rri">RAW_READ_INFO structure</param>
    /// <param name="InBufferSize">Size of RAW_READ_INFO structure</param>
    /// <param name="OutBuffer">Buffer that will receive the data to be read</param>
    /// <param name="OutBufferSize">Size of the buffer</param>
    /// <param name="BytesReturned">Receives the size, in bytes, of the data stored into OutBuffer</param>
    /// <param name="Overlapped">Pointer to an OVERLAPPED structure. Discarded for this case</param>
    /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern int DeviceIoControl(IntPtr hDevice, uint IoControlCode,
                                             [In] RAW_READ_INFO rri, uint InBufferSize,
                                             [In, Out] byte[] OutBuffer, uint OutBufferSize,
                                             ref uint BytesReturned,
                                             IntPtr Overlapped);
  }
}