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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using MediaPortal.ServiceImplementations;

namespace MediaPortal.Util
{

  #region Structs

  // The CharSet must match the CharSet of the corresponding PInvoke signature
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  internal struct WIN32_FIND_DATA
  {
    public uint dwFileAttributes;
    public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
    public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
    public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
    public uint nFileSizeHigh;
    public uint nFileSizeLow;
    public uint dwReserved0;
    public uint dwReserved1;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string cFileName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public string cAlternateFileName;
  }

  [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
  public struct OFSTRUCT
  {
    public byte cBytes;
    public byte fFixedDisc;
    public UInt16 nErrCode;
    public UInt16 Reserved1;
    public UInt16 Reserved2;
    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 128)] public string szPathName;
  }

  #endregion //structs

  public class NativeFileSystemOperations
  {
    #region Imports

    // using unicode removes a problem of a max file path in the ANSI implementation
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FindClose(IntPtr hFindFile);

    #endregion //Imports

    #region Constants

    private const string fileSystemPrefix = @"\\?\";
    private const string UNCPrefix = @"\\?\UNC\";

    #endregion //Constants

    #region Methods

    /// <summary>
    /// Gets all the filenames in a directory.
    /// Does not include subdirectories.
    /// </summary>
    /// <param name="directory">The directory to parse</param>
    /// <returns>A string array of the filenames</returns>
    public static string[] GetFiles(string directory)
    {
      return GetFiles(directory, true);
    }

    /// <summary>
    /// Gets all the filenames in a directory.
    /// Does not include subdirectories.
    /// </summary>
    /// <param name="directory">The directory to parse</param>
    /// <returns>A string array of the filenames</returns>
    public static string[] GetFiles(string directory, bool includeSystemAndHidden)
    {
      List<string> fi = new List<string>();
      IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
      string prefix = fileSystemPrefix;
      IntPtr findHandle = new IntPtr(0);
      WIN32_FIND_DATA findData;
      if (directory.StartsWith(@"\\"))
      {
        prefix = UNCPrefix;
        directory = directory.Substring(2);
      }
      try
      {
        findHandle = FindFirstFile(prefix + directory + @"\*", out findData);
        if (findHandle != INVALID_HANDLE_VALUE)
        {
          do
          {
            //only pick up files
            if ((findData.dwFileAttributes & (uint)FileAttributes.Directory) == 0)
            {
              if (includeSystemAndHidden ||
                  (findData.dwFileAttributes & ((uint)FileAttributes.Hidden | (uint)FileAttributes.System)) == 0)
              {
                string fName = Path.Combine(directory, findData.cFileName);
                if (prefix == UNCPrefix) fName = @"\\" + fName;
                fi.Add(fName);
              }
            }
          } while (FindNextFile(findHandle, out findData));
          FindClose(findHandle);
          findHandle = IntPtr.Zero;
        }
      }
      finally
      {
        if (findHandle.ToInt32() > 0) FindClose(findHandle);
      }
      return fi.ToArray();
    }

    /// <summary>
    /// Gets all the filenames in a directory.
    /// Does not include subdirectories.
    /// </summary>
    /// <param name="directory">The directory to parse</param>
    /// <returns>A string array of the filenames</returns>
    public static FileInformation[] GetFileInformation(string directory)
    {
      return GetFileInformation(directory, true);
    }

    /// <summary>
    /// Gets all the filenames in a directory.
    /// Does not include subdirectories.
    /// </summary>
    /// <param name="directory">The directory to parse</param>
    /// <returns>A string array of the filenames</returns>
    public static FileInformation[] GetFileInformation(string directory, bool includeSystemAndHidden)
    {
      List<FileInformation> fi = new List<FileInformation>();
      IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
      string prefix = fileSystemPrefix;
      IntPtr findHandle = new IntPtr(0);
      WIN32_FIND_DATA findData;
      if (directory.StartsWith(@"\\"))
      {
        prefix = UNCPrefix;
        //remove the leading \\
        directory = directory.Substring(2);
      }

      try
      {
        findHandle = FindFirstFile(prefix + directory + @"\*", out findData);
        if (findHandle != INVALID_HANDLE_VALUE)
        {
          do
          {
            //only pick up files
            if ((findData.dwFileAttributes & (uint)FileAttributes.Directory) == 0)
            {
              if (includeSystemAndHidden ||
                  (findData.dwFileAttributes & ((uint)FileAttributes.Hidden | (uint)FileAttributes.System)) == 0)
              {
                FileInformation fileInfo = new FileInformation();               
                long ftCreationTime  = (((long)findData.ftCreationTime.dwHighDateTime) << 32) | (uint)findData.ftCreationTime.dwLowDateTime;
                long ftLastWriteTime = (((long)findData.ftLastWriteTime.dwHighDateTime) << 32) | (uint)findData.ftLastWriteTime.dwLowDateTime;   
                fileInfo.Name = Path.Combine(directory, findData.cFileName);
                if (prefix == UNCPrefix) fileInfo.Name = @"\\" + fileInfo.Name;                               
                fileInfo.Length = (((long)findData.nFileSizeHigh) << 32) | (uint)findData.nFileSizeLow;
                fileInfo.CreationTime = DateTime.FromFileTimeUtc(ftCreationTime);
                fileInfo.ModificationTime = DateTime.FromFileTimeUtc(ftLastWriteTime);
                fi.Add(fileInfo);
              }
            }
          } while (FindNextFile(findHandle, out findData));
          FindClose(findHandle);
          findHandle = IntPtr.Zero;
        }
      }
      finally
      {
        if (findHandle.ToInt32() > 0) FindClose(findHandle);
      }
      return fi.ToArray();
    }

    #endregion //Methods
  }
}