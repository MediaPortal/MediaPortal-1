#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
    extern static bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable, out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);

    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    extern static bool GetVolumeInformation(
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
    protected static extern int mciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, IntPtr hwndCallback);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
      IntPtr lpInBuffer, uint nInBufferSize,
      IntPtr lpOutBuffer, uint nOutBufferSize,
      out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr CreateFile(
      string filename,
      [MarshalAs(UnmanagedType.U4)] FileAccess fileaccess,
      [MarshalAs(UnmanagedType.U4)] FileShare fileshare,
      int securityattributes,
      [MarshalAs(UnmanagedType.U4)] FileMode creationdisposition,
      int flags, IntPtr template);


    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr hObject);
    
    //Variables for MCE services
    static bool _restartMCEehRecvr = false;
    static bool _restartMCEehSched = false;
    static bool _restartWmcEhtray = false;
    static bool _restartWmcEhmsas = false;
    static ServiceController _ehRecvr = new ServiceController("ehRecvr");
    static ServiceController _ehSched = new ServiceController("ehSched");
    static string _ehtrayPath = string.Empty;
    static string _ehmsasPath = string.Empty;

    public delegate void UtilEventHandler(Process proc, bool waitForExit);


    static char[] crypt = new char[10] { 'G', 'D', 'J', 'S', 'I', 'B', 'T', 'P', 'W', 'Q' };

    // singleton. Dont allow any instance of this class
    private Utils()
    {
    }
    static public string GetDriveSerial(string drive)
    {
      if (drive == null) return String.Empty;
      //receives volume name of drive
      StringBuilder volname = new StringBuilder(256);
      //receives serial number of drive,not in case of network drive(win95/98)
      uint sn;
      uint maxcomplen;//receives maximum component length
      uint sysflags;//receives file system flags
      StringBuilder sysname = new StringBuilder(256);//receives the file system name
      bool retval;//return value

      retval = GetVolumeInformation(drive.Substring(0, 2), volname, 256, out sn, out maxcomplen, out sysflags, sysname, 256);

      if (retval)
      {
        return String.Format("{0:X}", sn);
      }
      else return "";
    }
    static public string GetDriveName(string drive)
    {
      if (drive == null) return String.Empty;
      //receives volume name of drive
      StringBuilder volname = new StringBuilder(256);
      //receives serial number of drive,not in case of network drive(win95/98)
      uint sn;
      uint maxcomplen;//receives maximum component length
      uint sysflags;//receives file system flags
      StringBuilder sysname = new StringBuilder(256);//receives the file system name
      bool retval;//return value

      retval = GetVolumeInformation(drive, volname, 256, out sn, out maxcomplen, out sysflags, sysname, 256);

      if (retval)
      {
        return volname.ToString();
      }
      else return "";
    }
    static public int getDriveType(string drive)
    {
      if (drive == null) return 2;
      if ((GetDriveType(drive) & 5) == 5) return 5;//cd
      if ((GetDriveType(drive) & 3) == 3) return 3;//fixed
      if ((GetDriveType(drive) & 2) == 2) return 2;//removable
      if ((GetDriveType(drive) & 4) == 4) return 4;//remote disk
      if ((GetDriveType(drive) & 6) == 6) return 6;//ram disk
      return 0;
    }
    static public long GetDiskSize(string drive)
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

    static public string GetSize(long dwFileSize)
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

    static public void GetQualifiedFilename(string strBasePath, ref string strFileName)
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

    static public string stripHTMLtags(string strHTML)
    {
      if (strHTML == null) return String.Empty;
      if (strHTML.Length == 0) return String.Empty;
      string stripped = Regex.Replace(strHTML, @"<(.|\n)*?>", string.Empty);
      return stripped.Trim();
    }
    static public bool IsNetwork(string strPath)
    {
      if (strPath == null) return false;
      if (strPath.Length < 2) return false;
      string strDrive = strPath.Substring(0, 2);
      if (getDriveType(strDrive) == 4) return true;
      return false;
    }

    static public bool IsHD(string strPath)
    {
      if (strPath == null) return false;
      if (strPath.Length < 2) return false;
      string strDrive = strPath.Substring(0, 2);
      if (getDriveType(strDrive) == 3) return true;
      return false;
    }

    static public bool IsCDDA(string strFile)
    {
      if (strFile == null) return false;
      if (strFile.Length <= 0) return false;
      if (strFile.IndexOf("cdda:") >= 0) return true;
      if (strFile.IndexOf(".cda") >= 0) return true;
      return false;
    }

    static public bool IsDVD(string strFile)
    {
      if (strFile == null) return false;
      if (strFile.Length < 2) return false;
      string strDrive = strFile.Substring(0, 2);
      if (getDriveType(strDrive) == 5) return true;
      return false;
    }

    static public bool IsRemovable(string strFile)
    {
      if (strFile == null) return false;
      if (strFile.Length < 2) return false;
      string strDrive = strFile.Substring(0, 2);
      if (getDriveType(strDrive) == 2) return true;
      return false;
    }

    static public bool GetDVDLabel(string strFile, out string strLabel)
    {
      strLabel = "";
      if (strFile == null) return false;
      if (strFile.Length == 0) return false;
      string strDrive = strFile.Substring(0, 2);
      strLabel = GetDriveName(strDrive);
      return true;
    }
    static public bool ShouldStack(string strFile1, string strFile2)
    {
      if (strFile1 == null) return false;
      if (strFile2 == null) return false;
      try
      {
        // Patterns that are used for matching
        // 1st pattern matches [x-y] for example [1-2] which is disc 1 of 2 total
        // 2nd pattern matches ?cd?## and ?disc?## for example -cd2 which is cd 2.
        //     ? is -_ or space (second ? is optional), ## is 1 or 2 digits
        string[] pattern = {"\\[[0-9]{1,2}-[0-9]{1,2}\\]",
														 "[-_ ](CD|cd|DISC|disc)[-_ ]{0,1}[0-9]{1,2}"};

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
      catch (Exception)
      {
      }

      // No matches were found, so no stacking
      return false;
    }

    static public void RemoveStackEndings(ref string strFileName)
    {

      if (strFileName == null) return;
      string[] pattern = {"\\[[0-9]{1,2}-[0-9]{1,2}\\]",
													 "[-_ ](CD|cd|DISC|disc)[-_ ]{0,1}[0-9]{1,2}"};
      for (int i = 0; i < pattern.Length; i++)
      {
        // See if we can find the special patterns in both filenames
        if (Regex.IsMatch(strFileName, pattern[i]))
        {
          strFileName = Regex.Replace(strFileName, pattern[i], "");
        }
      }
    }


    static public void Split(string strFileNameAndPath, out string strPath, out string strFileName)
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

    static public bool EjectCDROM(string strDrive)
    {
      bool result = false;
      strDrive = @"\\.\" + strDrive;

      try
      {
        IntPtr fHandle = CreateFile(strDrive, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite, 0, System.IO.FileMode.Open, 0x80, IntPtr.Zero);
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
      catch (Exception)
      {
      }

      return result;
    }

    static public void EjectCDROM()
    {
      mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
    }
    static public DateTime longtodate(long ldate)
    {
      try
      {
        if (ldate < 0) return DateTime.MinValue;
        int year, month, day, hour, minute, sec;
        sec = (int)(ldate % 100L); ldate /= 100L;
        minute = (int)(ldate % 100L); ldate /= 100L;
        hour = (int)(ldate % 100L); ldate /= 100L;
        day = (int)(ldate % 100L); ldate /= 100L;
        month = (int)(ldate % 100L); ldate /= 100L;
        year = (int)ldate;
        DateTime dt = new DateTime(year, month, day, hour, minute, 0, 0);
        return dt;
      }
      catch (Exception)
      {
      }
      return DateTime.Now;
    }

    static public long datetolong(DateTime dt)
    {
      try
      {
        long iSec = 0;//(long)dt.Second;
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
      catch (Exception)
      {
      }
      return 0;
    }

    static public string MakeFileName(string strText)
    {
      if (strText == null) return String.Empty;
      if (strText.Length == 0) return String.Empty;
      string strFName = strText.Replace(':', '_');
      strFName = strFName.Replace('/', '_');
      strFName = strFName.Replace('\\', '_');
      strFName = strFName.Replace('*', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_'); ;
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');
      return strFName;
    }

    static public string MakeDirectoryPath(string strText)
    {
      if (strText == null) return String.Empty;
      if (strText.Length == 0) return String.Empty;
      string strFName = strText.Replace('*', '_');
      strFName = strFName.Replace(':', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_'); ;
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');
      return strFName;
    }

    static public bool FileDelete(string strFile)
    {
      if (strFile == null) return true;
      if (strFile.Length == 0) return true;
      try
      {
        if (!System.IO.File.Exists(strFile)) return true;
        System.IO.File.Delete(strFile);
        return true;
      }
      catch (Exception)
      {
      }
      return false;
    }
    static public bool DirectoryDelete(string strDir)
    {
      if (strDir == null) return false;
      if (strDir.Length == 0) return false;
      try
      {
        System.IO.Directory.Delete(strDir);
        return true;
      }
      catch (Exception)
      {
      }
      return false;
    }


    static public string RemoveTrailingSlash(string strLine)
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
    static public void RGB2YUV(int R, int G, int B, out int Y, out int U, out int V)
    {
      Y = (int)(((float)R) * 0.257f + ((float)G) * 0.504f + ((float)B) * 0.098f + 16.0f);
      U = (int)(((float)R) * -0.148f + ((float)G) * -0.291f + ((float)B) * 0.439f + 128.0f);
      V = (int)(((float)R) * 0.439f + ((float)G) * -0.368f + ((float)B) * -0.071f + 128.0f);
      Y = Y & 0xff;
      U = U & 0xff;
      V = V & 0xff;
    }
    static public void RGB2YUV(int iRGB, out int YUV)
    {
      int Y, U, V;
      RGB2YUV((iRGB >> 16) & 0xff, (iRGB >> 8) & 0xff, (iRGB & 0xff), out Y, out U, out V);

      Y <<= 16;
      U <<= 8;

      YUV = Y + U + V;
    }
    static public string FilterFileName(string strName)
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

    static public string RemoveParenthesis(string name)
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

    static public void DeleteFiles(string strDir, string strPattern)
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
          catch (Exception) { }
        }
      }
      catch (Exception) { }

    }

    static public DateTime ParseDateTimeString(string dateTime)
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
      catch (Exception)
      {
      }
      return DateTime.Now;
    }

    public static string ReplaceTag(string line, string tag, string value, string defaultValue)
    {
      if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(tag))
        return defaultValue;

      // check for [*%tag%*]
      Regex r = new Regex(String.Format(@"\[[^%]*\%{0}\%[^\]]*\]", tag));

      Match match = r.Match(line);
      if (match != null && match.Length > 0)
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
      else
      {
        //remove the % from the orignal line for this specific tag
        line = line.Replace(String.Format("%{0}%", tag), tag);
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

    static public bool HibernateSystem(bool forceShutDown)
    {
      return (SetSuspendState(PowerState.Hibernate, forceShutDown));
    }

    static public bool SuspendSystem(bool forceShutDown)
    {
      return (SetSuspendState(PowerState.Suspend, forceShutDown));
    }

    static private bool SetSuspendState(PowerState state, bool forceShutDown)
    {

      return (Application.SetSuspendState(state, forceShutDown, false));
    }
    static public string BlurConnectionStringPassword(string connStr)
    {
      if (connStr.IndexOf("Password", StringComparison.InvariantCultureIgnoreCase) != -1)
      {
        int start = connStr.IndexOf("Password=", StringComparison.InvariantCultureIgnoreCase);
        connStr = connStr.Remove(start + 9, connStr.IndexOf(';', start) - start);
        connStr = connStr.Insert(start + 9, "xxxxxx;");
      }
      return connStr;
    }

    /// <summary>
    /// Shut down the MCE services
    /// </summary>
    public static void ShutDownMCEServices()
    {
        // Stop Vista & XP services
        bool ehRecvrExist = false;
        bool ehSchedExist = false;
        bool success = true;

        // Check for existance of MCE services without throwing/catching exceptions
        ServiceController[] services = ServiceController.GetServices();
        foreach (ServiceController srv in services)
        {
            if (srv.ServiceName == "ehRecvr")
            {
                ehRecvrExist = true;
                if (ehSchedExist) //Found both services
                    break;
            }
            if (srv.ServiceName == "ehSched")
            {
                ehSchedExist = true;
                if (ehRecvrExist) //Found both services
                    break;
            }
        }

        // Stop MCE ehRecvr and ehSched services
        if ((ehRecvrExist && (_ehRecvr.Status != ServiceControllerStatus.Stopped) && (_ehRecvr.Status != ServiceControllerStatus.StopPending))
          || (ehSchedExist && (_ehSched.Status != ServiceControllerStatus.Stopped) && (_ehSched.Status != ServiceControllerStatus.StopPending)))
        {
            Log.Info("  Stopping Microsoft Media Center services");
            try
            {
                if ((_ehRecvr.Status != ServiceControllerStatus.Stopped) && (_ehRecvr.Status != ServiceControllerStatus.StopPending))
                {
                    _ehRecvr.Stop();
                    _restartMCEehRecvr = true;
                }
            }
            catch
            {
                success = false;
                Log.Error("Error stopping MCE service \"ehRecvr\"");
            }
            try
            {
                if ((_ehSched.Status != ServiceControllerStatus.Stopped) && (_ehSched.Status != ServiceControllerStatus.StopPending))
                {
                    _ehSched.Stop();
                    _restartMCEehSched = true;
                }
            }
            catch
            {
                success = false;
                Log.Error("Error stopping MCE service \"ehSched\"");
            }
        }

        if (success)
        {
            try
            {
                // Stop Vista specific services
                if (Process.GetProcessesByName("ehtray").Length != 0)
                {
                    _restartWmcEhtray = true;
                    _ehtrayPath = Process.GetProcessesByName("ehtray")[0].MainModule.FileName;
                    foreach (Process proc in Process.GetProcessesByName("ehtray"))
                        proc.Kill();
                }

                Thread.Sleep(200);
                if (Process.GetProcessesByName("ehtray").Length != 0)
                {
                    Log.Error("StopVistaServices: Cannot terminate ehtray.exe");
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }
        else
            Log.Error("!!! MediaPortal needs to be run as Administrator on Vista to stop the Media Center services that occupy your TV cards/remote control !!!");


    }

    /// <summary>
    /// Restart the MCE services
    /// </summary>
    public static void RestartMCEServices()
    {
        if (_restartMCEehRecvr || _restartMCEehSched)
        {
            Log.Info("Restarting MCE services");

            try
            {
                if (_restartMCEehRecvr)
                    _ehRecvr.Start();
            }
            catch (Exception ex)
            {
                if (_ehRecvr.Status != ServiceControllerStatus.Running)
                    Log.Info("Error starting MCE service \"ehRecvr\" {0}", ex.ToString());
            }

            try
            {
                if (_restartMCEehSched)
                    _ehSched.Start();
            }
            catch (Exception ex)
            {
                if (_ehSched.Status != ServiceControllerStatus.Running)
                    Log.Info("Error starting MCE service \"ehSched\" {0}", ex.ToString());
            }
        }

        if ((_restartWmcEhtray && _ehtrayPath != string.Empty) || (_restartWmcEhmsas && _ehmsasPath != string.Empty))
        {
            Log.Info("Restarting Vista MC specific background applications");
            if (_restartWmcEhtray)
                Process.Start(_ehtrayPath);
        }
    }
  }
}
