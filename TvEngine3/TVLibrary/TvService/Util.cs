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
using System.Text.RegularExpressions;
using System.Management;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Windows.Forms;
using TvLibrary.Log;
using System.Threading;


namespace TvService
{
  /// <summary>
  /// 
  /// </summary>
  public class Utils
  {
    [DllImport("kernel32.dll")]
    private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable,
                                                  out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);

    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetVolumeInformation(
      string RootPathName,
      StringBuilder VolumeNameBuffer,
      int VolumeNameSize,
      out uint VolumeSerialNumber,
      out uint MaximumComponentLength,
      out uint FileSystemFlags,
      StringBuilder FileSystemNameBuffer,
      int nFileSystemNameSize);

    [DllImport("kernel32.dll")]
    public static extern long GetDriveType(string driveLetter);

    [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
    protected static extern int mciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength,
                                              IntPtr hwndCallback);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
                                               IntPtr lpInBuffer, uint nInBufferSize,
                                               IntPtr lpOutBuffer, uint nOutBufferSize,
                                               out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
      string filename,
      [MarshalAs(UnmanagedType.U4)] FileAccess fileaccess,
      [MarshalAs(UnmanagedType.U4)] FileShare fileshare,
      int securityattributes,
      [MarshalAs(UnmanagedType.U4)] FileMode creationdisposition,
      int flags, IntPtr template);


    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    public delegate void UtilEventHandler(Process proc, bool waitForExit);

    private static char[] crypt = new char[10] {'G', 'D', 'J', 'S', 'I', 'B', 'T', 'P', 'W', 'Q'};

    // singleton. Dont allow any instance of this class
    private Utils() {}

    public static string GetDriveSerial(string drive)
    {
      if (drive == null) return String.Empty;
      //receives volume name of drive
      StringBuilder volname = new StringBuilder(256);
      //receives serial number of drive,not in case of network drive(win95/98)
      uint sn;
      uint maxcomplen; //receives maximum component length
      uint sysflags; //receives file system flags
      StringBuilder sysname = new StringBuilder(256); //receives the file system name
      bool retval; //return value

      retval = GetVolumeInformation(drive.Substring(0, 2), volname, 256, out sn, out maxcomplen, out sysflags, sysname,
                                    256);

      if (retval)
      {
        return String.Format("{0:X}", sn);
      }
      else return "";
    }

    public static string GetDriveName(string drive)
    {
      if (drive == null) return String.Empty;
      //receives volume name of drive
      StringBuilder volname = new StringBuilder(256);
      //receives serial number of drive,not in case of network drive(win95/98)
      uint sn;
      uint maxcomplen; //receives maximum component length
      uint sysflags; //receives file system flags
      StringBuilder sysname = new StringBuilder(256); //receives the file system name
      bool retval; //return value

      retval = GetVolumeInformation(drive, volname, 256, out sn, out maxcomplen, out sysflags, sysname, 256);

      if (retval)
      {
        return volname.ToString();
      }
      else return "";
    }

    public static int getDriveType(string drive)
    {
      if (drive == null) return 2;
      if ((GetDriveType(drive) & 5) == 5) return 5; //cd
      if ((GetDriveType(drive) & 3) == 3) return 3; //fixed
      if ((GetDriveType(drive) & 2) == 2) return 2; //removable
      if ((GetDriveType(drive) & 4) == 4) return 4; //remote disk
      if ((GetDriveType(drive) & 6) == 6) return 6; //ram disk
      return 0;
    }

    public static long GetDiskSize(string drive)
    {
      long diskSize = 0;
      try
      {
        string cmd = String.Format("win32_logicaldisk.deviceid=\"{0}:\"", drive[0]);
        using (ManagementObject disk = new ManagementObject(cmd))
        {
          disk.Get();
          diskSize = Int64.Parse(disk["Size"].ToString());
        }
      }
      catch (Exception)
      {
        return -1;
      }
      return diskSize;
    }

    public static string GetSize(long dwFileSize)
    {
      if (dwFileSize < 0) return "0";
      string szTemp;
      // file < 1 kbyte?
      if (dwFileSize < 1024)
      {
        //  substract the integer part of the float value
        float fRemainder = (((float)dwFileSize) / 1024.0f) - (((float)dwFileSize) / 1024.0f);
        float fToAdd = 0.0f;
        if (fRemainder < 0.01f)
          fToAdd = 0.1f;
        szTemp = String.Format("{0:f} KB", (((float)dwFileSize) / 1024.0f) + fToAdd);
        return szTemp;
      }
      long iOneMeg = 1024 * 1024;

      // file < 1 megabyte?
      if (dwFileSize < iOneMeg)
      {
        szTemp = String.Format("{0:f} KB", ((float)dwFileSize) / 1024.0f);
        return szTemp;
      }

      // file < 1 GByte?
      long iOneGigabyte = iOneMeg;
      iOneGigabyte *= (long)1000;
      if (dwFileSize < iOneGigabyte)
      {
        szTemp = String.Format("{0:f} MB", ((float)dwFileSize) / ((float)iOneMeg));
        return szTemp;
      }
      //file > 1 GByte
      int iGigs = 0;
      while (dwFileSize >= iOneGigabyte)
      {
        dwFileSize -= iOneGigabyte;
        iGigs++;
      }
      float fMegs = ((float)dwFileSize) / ((float)iOneMeg);
      fMegs /= 1000.0f;
      fMegs += iGigs;
      szTemp = String.Format("{0:f} GB", fMegs);
      return szTemp;
    }

    public static void GetQualifiedFilename(string strBasePath, ref string strFileName)
    {
      if (strFileName == null) return;
      if (strFileName.Length <= 2) return;
      if (strFileName[1] == ':') return;
      strBasePath = Utils.RemoveTrailingSlash(strBasePath);
      while (strFileName.StartsWith(@"..\") || strFileName.StartsWith("../"))
      {
        strFileName = strFileName.Substring(3);
        int pos = strBasePath.LastIndexOf(@"\");
        if (pos > 0)
        {
          strBasePath = strBasePath.Substring(0, pos);
        }
        else
        {
          pos = strBasePath.LastIndexOf(@"/");
          if (pos > 0)
          {
            strBasePath = strBasePath.Substring(0, pos);
          }
        }
      }
      if (strBasePath.Length == 2 && strBasePath[1] == ':')
        strBasePath += @"\";
      strFileName = System.IO.Path.Combine(strBasePath, strFileName);
    }

    public static string stripHTMLtags(string strHTML)
    {
      if (strHTML == null) return String.Empty;
      if (strHTML.Length == 0) return String.Empty;
      string stripped = Regex.Replace(strHTML, @"<(.|\n)*?>", string.Empty);
      return stripped.Trim();
    }

    public static bool IsNetwork(string strPath)
    {
      if (strPath == null) return false;
      if (strPath.Length < 2) return false;
      string strDrive = strPath.Substring(0, 2);
      if (getDriveType(strDrive) == 4) return true;
      return false;
    }

    public static bool IsHD(string strPath)
    {
      if (strPath == null) return false;
      if (strPath.Length < 2) return false;
      string strDrive = strPath.Substring(0, 2);
      if (getDriveType(strDrive) == 3) return true;
      return false;
    }

    public static bool IsCDDA(string strFile)
    {
      if (strFile == null) return false;
      if (strFile.Length <= 0) return false;
      if (strFile.IndexOf("cdda:") >= 0) return true;
      if (strFile.IndexOf(".cda") >= 0) return true;
      return false;
    }

    public static bool IsDVD(string strFile)
    {
      if (strFile == null) return false;
      if (strFile.Length < 2) return false;
      string strDrive = strFile.Substring(0, 2);
      if (getDriveType(strDrive) == 5) return true;
      return false;
    }

    public static bool IsRemovable(string strFile)
    {
      if (strFile == null) return false;
      if (strFile.Length < 2) return false;
      string strDrive = strFile.Substring(0, 2);
      if (getDriveType(strDrive) == 2) return true;
      return false;
    }

    public static bool GetDVDLabel(string strFile, out string strLabel)
    {
      strLabel = "";
      if (strFile == null) return false;
      if (strFile.Length == 0) return false;
      string strDrive = strFile.Substring(0, 2);
      strLabel = GetDriveName(strDrive);
      return true;
    }

    public static bool ShouldStack(string strFile1, string strFile2)
    {
      if (strFile1 == null) return false;
      if (strFile2 == null) return false;
      try
      {
        // Patterns that are used for matching
        // 1st pattern matches [x-y] for example [1-2] which is disc 1 of 2 total
        // 2nd pattern matches ?cd?## and ?disc?## for example -cd2 which is cd 2.
        //     ? is -_ or space (second ? is optional), ## is 1 or 2 digits
        string[] pattern = {
                             "\\[[0-9]{1,2}-[0-9]{1,2}\\]",
                             "[-_ ](CD|cd|DISC|disc)[-_ ]{0,1}[0-9]{1,2}"
                           };

        // Strip the extensions and make everything lowercase
        string strFileName1 = System.IO.Path.GetFileNameWithoutExtension(strFile1).ToLowerInvariant();
        string strFileName2 = System.IO.Path.GetFileNameWithoutExtension(strFile2).ToLowerInvariant();

        // Check all the patterns
        for (int i = 0; i < pattern.Length; i++)
        {
          // See if we can find the special patterns in both filenames
          if (Regex.IsMatch(strFileName1, pattern[i]) && Regex.IsMatch(strFileName2, pattern[i]))
          {
            // Both strings had the special pattern. Now see if the filenames are the same.
            // Do this by removing the special pattern and compare the remains.
            if (Regex.Replace(strFileName1, pattern[i], "")
                == Regex.Replace(strFileName2, pattern[i], ""))
            {
              // It was a match so stack it
              return true;
            }
          }
        }
      }
      catch (Exception) {}

      // No matches were found, so no stacking
      return false;
    }

    public static void RemoveStackEndings(ref string strFileName)
    {
      if (strFileName == null) return;
      string[] pattern = {
                           "\\[[0-9]{1,2}-[0-9]{1,2}\\]",
                           "[-_ ](CD|cd|DISC|disc)[-_ ]{0,1}[0-9]{1,2}"
                         };
      for (int i = 0; i < pattern.Length; i++)
      {
        // See if we can find the special patterns in both filenames
        if (Regex.IsMatch(strFileName, pattern[i]))
        {
          strFileName = Regex.Replace(strFileName, pattern[i], "");
        }
      }
    }


    public static void Split(string strFileNameAndPath, out string strPath, out string strFileName)
    {
      strFileName = "";
      strPath = "";
      if (strFileNameAndPath == null) return;
      if (strFileNameAndPath.Length == 0) return;
      try
      {
        strFileNameAndPath = strFileNameAndPath.Trim();
        if (strFileNameAndPath.Length == 0) return;
        int i = strFileNameAndPath.Length - 1;
        while (i >= 0)
        {
          char ch = strFileNameAndPath[i];
          if (ch == ':' || ch == '/' || ch == '\\') break;
          else i--;
        }
        if (i >= 0)
        {
          strPath = strFileNameAndPath.Substring(0, i).Trim();
          strFileName = strFileNameAndPath.Substring(i + 1).Trim();
        }
        else
        {
          strPath = "";
          strFileName = strFileNameAndPath;
        }
      }
      catch (Exception)
      {
        strPath = "";
        strFileName = strFileNameAndPath;
      }
    }

    public static bool EjectCDROM(string strDrive)
    {
      bool result = false;
      strDrive = @"\\.\" + strDrive;

      try
      {
        IntPtr fHandle = CreateFile(strDrive, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 0,
                                    System.IO.FileMode.Open, 0x80, IntPtr.Zero);
        if (fHandle.ToInt64() != -1) //INVALID_HANDLE_VALUE)
        {
          uint Result;
          if (DeviceIoControl(fHandle, 0x002d4808, IntPtr.Zero, 0, IntPtr.Zero, 0, out Result, IntPtr.Zero) == true)
          {
            result = true;
          }
          CloseHandle(fHandle);
        }
      }
      catch (Exception) {}

      return result;
    }

    public static void EjectCDROM()
    {
      mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
    }

    public static DateTime longtodate(long ldate)
    {
      try
      {
        if (ldate < 0) return DateTime.MinValue;
        int year, month, day, hour, minute, sec;
        sec = (int)(ldate % 100L);
        ldate /= 100L;
        minute = (int)(ldate % 100L);
        ldate /= 100L;
        hour = (int)(ldate % 100L);
        ldate /= 100L;
        day = (int)(ldate % 100L);
        ldate /= 100L;
        month = (int)(ldate % 100L);
        ldate /= 100L;
        year = (int)ldate;
        DateTime dt = new DateTime(year, month, day, hour, minute, 0, 0);
        return dt;
      }
      catch (Exception) {}
      return DateTime.Now;
    }

    public static long datetolong(DateTime dt)
    {
      try
      {
        long iSec = 0; //(long)dt.Second;
        long iMin = (long)dt.Minute;
        long iHour = (long)dt.Hour;
        long iDay = (long)dt.Day;
        long iMonth = (long)dt.Month;
        long iYear = (long)dt.Year;

        long lRet = (iYear);
        lRet = lRet * 100L + iMonth;
        lRet = lRet * 100L + iDay;
        lRet = lRet * 100L + iHour;
        lRet = lRet * 100L + iMin;
        lRet = lRet * 100L + iSec;
        return lRet;
      }
      catch (Exception) {}
      return 0;
    }

    public static string MakeFileName(string strText)
    {
      if (strText == null) return String.Empty;
      if (strText.Length == 0) return String.Empty;
      string strFName = strText.Replace(':', '_');
      strFName = strFName.Replace('/', '_');
      strFName = strFName.Replace('\\', '_');
      strFName = strFName.Replace('*', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_');
      ;
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');
      return strFName;
    }

    public static string MakeDirectoryPath(string strText)
    {
      if (strText == null) return String.Empty;
      if (strText.Length == 0) return String.Empty;
      string strFName = strText.Replace('*', '_');
      strFName = strFName.Replace(':', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_');
      ;
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');
      return strFName;
    }

    public static bool FileDelete(string strFile)
    {
      if (strFile == null) return true;
      if (strFile.Length == 0) return true;
      try
      {
        if (!System.IO.File.Exists(strFile)) return true;
        System.IO.File.Delete(strFile);
        return true;
      }
      catch (Exception) {}
      return false;
    }

    /// <summary>
    /// Deletes a file. After this the containing folder is deleted also, if it's empty
    /// </summary>
    /// <param name="strFile">filename</param>
    /// <returns>true if successful</returns>
    public static bool DeleteFileAndEmptyDirectory(string strFile)
    {
      if (String.IsNullOrEmpty(strFile)) return true;
      try
      {
        if (System.IO.File.Exists(strFile))
        {
          System.IO.File.Delete(strFile);
        }
        String FolderName = System.IO.Path.GetDirectoryName(strFile);
        DirectoryInfo ContainingFolder = new DirectoryInfo(FolderName);
        if (ContainingFolder != null)
        {
          DirectoryInfo[] subfolders = ContainingFolder.GetDirectories();
          FileInfo[] files = ContainingFolder.GetFiles();
          if (files.Length == 0 && subfolders.Length == 0)
          {
            System.IO.Directory.Delete(FolderName);
          }
        }
        return true;
      }
      catch (Exception) {}
      return false;
    }

    public static bool DirectoryDelete(string strDir)
    {
      if (strDir == null) return false;
      if (strDir.Length == 0) return false;
      try
      {
        System.IO.Directory.Delete(strDir);
        return true;
      }
      catch (Exception) {}
      return false;
    }


    public static string RemoveTrailingSlash(string strLine)
    {
      if (strLine == null) return String.Empty;
      if (strLine.Length == 0) return String.Empty;
      string strPath = strLine;
      while (strPath.Length > 0)
      {
        if (strPath[strPath.Length - 1] == '\\' || strPath[strPath.Length - 1] == '/')
        {
          strPath = strPath.Substring(0, strPath.Length - 1);
        }
        else break;
      }
      return strPath;
    }

    public static void RGB2YUV(int R, int G, int B, out int Y, out int U, out int V)
    {
      Y = (int)(((float)R) * 0.257f + ((float)G) * 0.504f + ((float)B) * 0.098f + 16.0f);
      U = (int)(((float)R) * -0.148f + ((float)G) * -0.291f + ((float)B) * 0.439f + 128.0f);
      V = (int)(((float)R) * 0.439f + ((float)G) * -0.368f + ((float)B) * -0.071f + 128.0f);
      Y = Y & 0xff;
      U = U & 0xff;
      V = V & 0xff;
    }

    public static void RGB2YUV(int iRGB, out int YUV)
    {
      int Y, U, V;
      RGB2YUV((iRGB >> 16) & 0xff, (iRGB >> 8) & 0xff, (iRGB & 0xff), out Y, out U, out V);

      Y <<= 16;
      U <<= 8;

      YUV = Y + U + V;
    }

    public static string FilterFileName(string strName)
    {
      if (strName == null) return String.Empty;
      if (strName.Length == 0) return String.Empty;
      strName = strName.Replace(@"\", "_");
      strName = strName.Replace("/", "_");
      strName = strName.Replace(":", "_");
      strName = strName.Replace("*", "_");
      strName = strName.Replace("?", "_");
      strName = strName.Replace("\"", "_");
      strName = strName.Replace("<", "_");
      strName = strName.Replace(">", "_");
      strName = strName.Replace("|", "_");
      return strName;
    }

    public static string RemoveParenthesis(string name)
    {
      while (name.IndexOf("(") != -1)
      {
        int start = name.IndexOf("(");
        int end = name.IndexOf(")");
        if (end != -1)
        {
          name = name.Substring(0, start) + name.Substring(end + 1);
        }
        else
        {
          break;
        }
      }
      while (name.IndexOf("[") != -1)
      {
        int start = name.IndexOf("[");
        int end = name.IndexOf("]");
        if (end != -1)
        {
          name = name.Substring(0, start) + name.Substring(end + 1);
        }
        else
        {
          break;
        }
      }
      return name;
    }

    public static void DeleteFiles(string strDir, string strPattern)
    {
      if (strDir == null) return;
      if (strDir.Length == 0) return;

      if (strPattern == null) return;
      if (strPattern.Length == 0) return;

      string[] strFiles;
      try
      {
        if (!System.IO.Directory.Exists(strDir))
          return;
        strFiles = System.IO.Directory.GetFiles(strDir, strPattern);
        foreach (string strFile in strFiles)
        {
          try
          {
            System.IO.File.Delete(strFile);
          }
          catch (Exception) {}
        }
      }
      catch (Exception) {}
    }

    public static DateTime ParseDateTimeString(string dateTime)
    {
      try
      {
        if (dateTime == null) return DateTime.Now;
        if (dateTime.Length == 0) return DateTime.Now;
        //format is d-m-y h:m:s
        dateTime = dateTime.Replace(":", "-");
        string[] parts = dateTime.Split('-');
        if (parts.Length < 6) return DateTime.Now;
        int hour, min, sec, year, day, month;
        day = Int32.Parse(parts[0]);
        month = Int32.Parse(parts[1]);
        year = Int32.Parse(parts[2]);

        hour = Int32.Parse(parts[3]);
        min = Int32.Parse(parts[4]);
        sec = Int32.Parse(parts[5]);
        return new DateTime(year, month, day, hour, min, sec, 0);
      }
      catch (Exception) {}
      return DateTime.Now;
    }

    public static string ReplaceTag(string line, string tag, string value, string defaultValue)
    {
      if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(tag))
        return defaultValue;

      // check for [*%tag%*]
      // +
      // escape % for regex parsing
      string strRegex = String.Format(@"\[[^%]*{0}[^\]]*[\]]", tag.Replace("%", "\\%"));

      Regex r;
      try
      {
        r = new Regex(strRegex);
      }
      catch (Exception ex)
      {
        Log.Error("ReplaceTag: Regex generated the following error: {0}", ex.Message);
        return line;
      }

      Match match = r.Match(line);
      if (match.Success)
      {
        // Remove [ xxx ] completly
        line = line.Remove(match.Index, match.Length);
        if (!String.IsNullOrEmpty(value))
        {
          // Add again xxx if value != null
          string m = match.Value.Substring(1, match.Value.Length - 2);
          line = line.Insert(match.Index, m);
        }
      }
      // finally replace tag with value
      return line.Replace(tag, value);
    }

    public static string ReplaceTag(string line, string tag, string value)
    {
      return ReplaceTag(line, tag, value, string.Empty);
    }

    public static ulong GetFreeDiskSpace(string drive)
    {
      if (drive.StartsWith(@"\"))
      {
        return GetFreeShareSpace(drive);
      }
      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;

      GetDiskFreeSpaceEx(
        drive[0] + @":\",
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return freeBytesAvailable;
    }

    public static ulong GetFreeShareSpace(string UNCPath)
    {
      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;

      GetDiskFreeSpaceEx(
        System.IO.Path.GetPathRoot(UNCPath),
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return freeBytesAvailable;
    }

    public static bool HibernateSystem(bool forceShutDown)
    {
      return (SetSuspendState(PowerState.Hibernate, forceShutDown));
    }

    public static bool SuspendSystem(bool forceShutDown)
    {
      return (SetSuspendState(PowerState.Suspend, forceShutDown));
    }

    private static bool SetSuspendState(PowerState state, bool forceShutDown)
    {
      return (Application.SetSuspendState(state, forceShutDown, false));
    }

    public static string BlurConnectionStringPassword(string connStr)
    {
      if (connStr.IndexOf("Password", StringComparison.InvariantCultureIgnoreCase) != -1)
      {
        int start = connStr.IndexOf("Password=", StringComparison.InvariantCultureIgnoreCase);
        connStr = connStr.Remove(start + 9, connStr.IndexOf(';', start) - start);
        connStr = connStr.Insert(start + 9, "xxxxxx;");
      }
      return connStr;
    }
  }
}