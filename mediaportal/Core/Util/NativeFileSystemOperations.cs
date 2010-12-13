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
  struct WIN32_FIND_DATA
  {
    public uint dwFileAttributes;
    public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
    public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
    public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
    public uint nFileSizeHigh;
    public uint nFileSizeLow;
    public uint dwReserved0;
    public uint dwReserved1;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string cFileName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
    public string cAlternateFileName;
  }

  [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
  public struct OFSTRUCT
  {
    public byte cBytes;
    public byte fFixedDisc;
    public UInt16 nErrCode;
    public UInt16 Reserved1;
    public UInt16 Reserved2;
    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 128)] 
    public string szPathName;
  }

  #endregion //structs

  #region Enums
  [Flags]
  enum eOpenFile : uint 
  {
    OF_CANCEL    = 0x00000800,  // Ignored. For a dialog box with a Cancel button, use OF_PROMPT.
    OF_CREATE    = 0x00001000,  // Creates a new file. If file exists, it is truncated to zero (0) length.
    OF_DELETE    = 0x00000200,  // Deletes a file.
    OF_EXIST     = 0x00004000,  // Opens a file and then closes it. Used to test that a file exists
    OF_PARSE     = 0x00000100,  // Fills the OFSTRUCT structure, but does not do anything else.
    OF_PROMPT    = 0x00002000,  // Displays a dialog box if a requested file does not exist 
    OF_READ      = 0x00000000,  // Opens a file for reading only.
    OF_READWRITE = 0x00000002,  // Opens a file with read/write permissions.
    OF_REOPEN    = 0x00008000,  // Opens a file by using information in the reopen buffer.

    // For MS-DOS–based file systems, opens a file with compatibility mode, allows any process on a 
    // specified computer to open the file any number of times.
    // Other efforts to open a file with other sharing modes fail. This flag is mapped to the 
    // FILE_SHARE_READ|FILE_SHARE_WRITE flags of the CreateFile function.
    OF_SHARE_COMPAT = 0x00000000,

    // Opens a file without denying read or write access to other processes.
    // On MS-DOS-based file systems, if the file has been opened in compatibility mode
    // by any other process, the function fails.
    // This flag is mapped to the FILE_SHARE_READ|FILE_SHARE_WRITE flags of the CreateFile function.
    OF_SHARE_DENY_NONE = 0x00000040, 

    // Opens a file and denies read access to other processes.
    // On MS-DOS-based file systems, if the file has been opened in compatibility mode,
    // or for read access by any other process, the function fails.
    // This flag is mapped to the FILE_SHARE_WRITE flag of the CreateFile function.
    OF_SHARE_DENY_READ = 0x00000030,

    // Opens a file and denies write access to other processes.
    // On MS-DOS-based file systems, if a file has been opened in compatibility mode,
    // or for write access by any other process, the function fails.
    // This flag is mapped to the FILE_SHARE_READ flag of the CreateFile function.
    OF_SHARE_DENY_WRITE = 0x00000020,

    // Opens a file with exclusive mode, and denies both read/write access to other processes.
    // If a file has been opened in any other mode for read/write access, even by the current process,
    // the function fails.
    OF_SHARE_EXCLUSIVE = 0x00000010,

    // Verifies that the date and time of a file are the same as when it was opened previously.
    // This is useful as an extra check for read-only files.
    OF_VERIFY = 0x00000400,

    // Opens a file for write access only.
    OF_WRITE = 0x00000001
  }

  #endregion //Enums

  public class NativeFileSystemOperations
  {

    #region Imports
    // using unicode removes a problem of a max file path in the ANSI implementation
    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FindClose(IntPtr hFindFile);

    [DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
    static extern int OpenFile([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]string lpFileName, out OFSTRUCT lpReOpenBuff, eOpenFile uStyle);

    #endregion //Imports

    #region Constants

    const Int32 OF_ERROR = -1;
    const Int32 OFS_MAXPATHNAME = 128;
    const string fileSystemPrefix = @"\\?\";
    const string UNCPrefix = @"\\?\UNC\";

    #endregion //Constants

    #region Methods

    /// <summary>
    /// Tests for the existance of a file
    /// </summary>
    /// <param name="filename">The filename to test</param>
    /// <returns>true if the file exists</returns>
    public static bool FileExists(string filename)
    {
      bool ret=false;
      OFSTRUCT ofStruct;
      Int32 ofRet=OpenFile(filename,out ofStruct,eOpenFile.OF_EXIST);
      if (ofRet!=OF_ERROR)
      {
        ret = true;
      }
      return ret;
    }

    /// <summary>
    /// Gets all the filenames in a directory.
    /// Does not include subdirectories.
    /// </summary>
    /// <param name="directory">The directory to parse</param>
    /// <returns>A string array of the filenames</returns>
    public static string[] GetFiles(string directory)
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
              if ((findData.dwFileAttributes & ((uint)FileAttributes.Hidden | (uint)FileAttributes.System))==0)
              {
                string fName = Path.Combine(directory, findData.cFileName);
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
      Log.Debug("Searching "+directory);

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
              if ((findData.dwFileAttributes & ((uint)FileAttributes.Hidden | (uint)FileAttributes.System)) == 0)
              {
                Log.Debug(Path.Combine(directory, findData.cFileName));
                FileInformation fileInfo = new FileInformation();
                long ftCreationTime = (((long)findData.ftCreationTime.dwHighDateTime) << 32) + findData.ftCreationTime.dwLowDateTime;
                long ftLastWriteTime = (((long)findData.ftLastWriteTime.dwHighDateTime) << 32) + findData.ftLastWriteTime.dwLowDateTime;
                fileInfo.Name = Path.Combine(directory, findData.cFileName);
                fileInfo.Length = (((long)findData.nFileSizeHigh) << 32) + findData.nFileSizeLow;
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
