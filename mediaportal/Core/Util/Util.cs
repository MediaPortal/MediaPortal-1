#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Management;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.ServiceProcess;
using System.Windows.Forms;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using MediaPortal.Player;


namespace MediaPortal.Util
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


    public delegate void UtilEventHandler(Process proc, bool waitForExit);
    static public event UtilEventHandler OnStartExternal = null;	// Event: Start external process / waeberd & mPod
    static public event UtilEventHandler OnStopExternal = null;		// Event: Stop external process	/ waeberd & mPod
    static ArrayList m_AudioExtensions = new ArrayList();
    static ArrayList m_VideoExtensions = new ArrayList();
    static ArrayList m_PictureExtensions = new ArrayList();
    static bool m_bHideExtensions = false;
    static bool enableGuiSounds;

    static bool restartMCEehRecvr = false;
    static bool restartMCEehSched = false;
    static ServiceController ehRecvr = new ServiceController("ehRecvr");
    static ServiceController ehSched = new ServiceController("ehSched");

    static char[] crypt = new char[10] { 'G', 'D', 'J', 'S', 'I', 'B', 'T', 'P', 'W', 'Q' };

    // singleton. Dont allow any instance of this class
    private Utils()
    {
    }
    static Utils()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        m_bHideExtensions = xmlreader.GetValueAsBool("general", "hideextensions", true);

        string strTmp = xmlreader.GetValueAsString("music", "extensions", ".mp3,.wma,.ogg,.flac,.wav,.cda,.b4s,.m4a,.m4p");
        Tokens tok = new Tokens(strTmp, new char[] { ',' });
        foreach (string extension in tok)
        {
          m_AudioExtensions.Add(extension.ToLower());
        }

        strTmp = xmlreader.GetValueAsString("movies", "extensions", ".avi,.mpg,.ogm,.mpeg,.mkv,.wmv,.ifo,.qt,.rm,.mov,.sbe,.dvr-ms,.ts");
        tok = new Tokens(strTmp, new char[] { ',' });
        foreach (string extension in tok)
        {
          m_VideoExtensions.Add(extension.ToLower());
        }

        strTmp = xmlreader.GetValueAsString("pictures", "extensions", ".jpg,.jpeg,.gif,.bmp,.png");
        tok = new Tokens(strTmp, new char[] { ',' });
        foreach (string extension in tok)
        {
          m_PictureExtensions.Add(extension.ToLower());
        }

        enableGuiSounds = xmlreader.GetValueAsBool("general", "enableguisounds", true);
      }
    }


    public static ArrayList VideoExtensions
    {
      get { return m_VideoExtensions; }
    }

    public static ArrayList AudioExtensions
    {
      get { return m_AudioExtensions; }
    }
    public static ArrayList PictureExtensions
    {
      get { return m_PictureExtensions; }
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

    static public bool IsLiveTv(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (strPath.ToLower().IndexOf("live.tv") >= 0) return true;
        if (strPath.ToLower().IndexOf("live.ts") >= 0) return true;
      }
      catch (Exception) { }
      return false;

    }
    static public bool IsLiveRadio(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (strPath.ToLower().IndexOf("radio.ts") >= 0) return true;
      }
      catch (Exception) { }
      return false;

    }

    static public bool IsVideo(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!System.IO.Path.HasExtension(strPath)) return false;
        if (IsPlayList(strPath)) return false;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        if (extensionFile.ToLower().Equals(".tv")) return true;
        if (extensionFile.ToLower().Equals(".ts")) return true;
        if (extensionFile.ToLower().Equals(".sbe")) return true;
        if (extensionFile.ToLower().Equals(".dvr-ms")) return true;
        if (VirtualDirectory.IsImageFile(extensionFile.ToLower())) return true;
        foreach (string extension in m_VideoExtensions)
        {
          if (extension == extensionFile) return true;
        }
      }
      catch (Exception)
      {
      }
      return false;
    }

    static public bool IsAudio(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!System.IO.Path.HasExtension(strPath)) return false;
        if (IsPlayList(strPath)) return false;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        foreach (string extension in m_AudioExtensions)
        {
          if (extension == extensionFile) return true;
        }
      }
      catch (Exception)
      {
      }
      return false;
    }

    static public bool IsPicture(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!System.IO.Path.HasExtension(strPath)) return false;
        if (IsPlayList(strPath)) return false;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        foreach (string extension in m_PictureExtensions)
        {
          if (extension == extensionFile) return true;
        }
      }
      catch (Exception)
      {
      }
      return false;
    }

    static public bool IsPlayList(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!System.IO.Path.HasExtension(strPath)) return false;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        if (extensionFile == ".m3u") return true;
        if (extensionFile == ".pls") return true;
        if (extensionFile == ".b4s") return true;
        if (extensionFile == ".wpl") return true;
      }
      catch (Exception)
      {
      }
      return false;
    }
    static public bool IsProgram(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!System.IO.Path.HasExtension(strPath)) return false;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        if (extensionFile == ".exe") return true;
      }
      catch (Exception)
      {
      }
      return false;
    }
    static public bool IsShortcut(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!System.IO.Path.HasExtension(strPath)) return false;
        string extensionFile = System.IO.Path.GetExtension(strPath).ToLower();
        if (extensionFile == ".lnk") return true;
      }
      catch (Exception)
      {
      }
      return false;
    }

    public static void SetDefaultIcons(GUIListItem item)
    {
      if (item == null) return;
      if (!item.IsFolder)
      {
        if (IsPlayList(item.Path))
        {
          item.IconImage = "DefaultPlaylist.png";
          item.IconImageBig = "DefaultPlaylistBig.png";
        }
        else if (IsVideo(item.Path))
        {
          item.IconImage = "defaultVideo.png";
          item.IconImageBig = "defaultVideoBig.png";
        }
        else if (IsCDDA(item.Path))
        {
          item.IconImage = "defaultCdda.png";
          item.IconImageBig = "defaultCddaBig.png";
        }
        else if (IsAudio(item.Path))
        {
          item.IconImage = "defaultAudio.png";
          item.IconImageBig = "defaultAudioBig.png";
        }
        else if (IsPicture(item.Path))
        {
          item.IconImage = "defaultPicture.png";
          item.IconImageBig = "defaultPictureBig.png";
        }
        else if (IsProgram(item.Path))
        {
          item.IconImage = "DefaultProgram.png";
          item.IconImageBig = "DefaultProgramBig.png";
        }
        else if (IsShortcut(item.Path))
        {
          item.IconImage = "DefaultShortcut.png";
          item.IconImageBig = "DefaultShortcutBig.png";
        }
      }
      else
      {
        if (item.Label == "..")
        {
          item.IconImage = "defaultFolderBack.png";
          item.IconImageBig = "defaultFolderBackBig.png";
        }
        else
        {
          if (item.Path.Length <= 3)
          {
            if (IsDVD(item.Path))
            {
              item.IconImage = "defaultDVDRom.png";
              item.IconImageBig = "defaultDVDRomBig.png";
            }
            else if (IsHD(item.Path))
            {
              item.IconImage = "defaultHardDisk.png";
              item.IconImageBig = "defaultHardDiskBig.png";
            }
            else if (IsNetwork(item.Path))
            {
              item.IconImage = "defaultNetwork.png";
              item.IconImageBig = "defaultNetworkBig.png";
            }
            else if (IsRemovable(item.Path))
            {
              item.IconImage = "defaultRemovable.png";
              item.IconImageBig = "defaultRemovableBig.png";
            }
            else
            {
              item.IconImage = "defaultFolder.png";
              item.IconImageBig = "defaultFolderBig.png";
            }
          }
          else
          {
            item.IconImage = "defaultFolder.png";
            item.IconImageBig = "defaultFolderBig.png";
          }
        }
      }
    }

    public static void SetThumbnails(ref GUIListItem item)
    {
      if (item == null) return;
      try
      {
        if (!item.IsFolder
        || (item.IsFolder && VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(item.Path).ToLower())))
        {
          if (IsPicture(item.Path)) return;

          // check for filename.tbn
          string strThumb = System.IO.Path.ChangeExtension(item.Path, ".tbn");
          if (System.IO.File.Exists(strThumb))
          {
            // yep got it
            item.ThumbnailImage = strThumb;
            item.IconImage = strThumb;
            item.IconImageBig = strThumb;
            return;
          }
          strThumb = System.IO.Path.ChangeExtension(item.Path, ".jpg");
          if (System.IO.File.Exists(strThumb))
          {
            // yep got it
            item.ThumbnailImage = strThumb;
            item.IconImage = strThumb;
            item.IconImageBig = strThumb;
            return;
          }
          strThumb = System.IO.Path.ChangeExtension(item.Path, ".png");
          if (System.IO.File.Exists(strThumb))
          {
            // yep got it
            item.ThumbnailImage = strThumb;
            item.IconImage = strThumb;
            item.IconImageBig = strThumb;
            return;
          }

          // check for thumbs\filename.tbn

          strThumb = GetThumb(item.Path);
          if (System.IO.File.Exists(strThumb))
          {
            // yep got it
            item.ThumbnailImage = strThumb;
            item.IconImage = strThumb;
            item.IconImageBig = strThumb;
            return;
          }
        }
        else
        {
          if (item.Label != "..")
          {
            // check for folder.jpg
            string strThumb = item.Path + @"\folder.jpg";
            if (System.IO.File.Exists(strThumb))
            {
              // got it
              item.ThumbnailImage = strThumb;
              item.IconImage = strThumb;
              item.IconImageBig = strThumb;
            }
          }
        }
      }
      catch (Exception)
      {
      }
    }

    static public string SecondsToShortHMSString(int lSeconds)
    {
      if (lSeconds < 0) return ("0:00");
      int hh = lSeconds / 3600;
      lSeconds = lSeconds % 3600;
      int mm = lSeconds / 60;
      int ss = lSeconds % 60;

      string strHMS = "";
      strHMS = String.Format("{0}:{1:00}", hh, mm);
      return strHMS;
    }

    static public string SecondsToHMSString(TimeSpan timespan)
    {
      return SecondsToHMSString(timespan.Seconds);
    }

    static public string SecondsToHMSString(int lSeconds)
    {
      if (lSeconds < 0) return ("0:00");
      int hh = lSeconds / 3600;
      lSeconds = lSeconds % 3600;
      int mm = lSeconds / 60;
      int ss = lSeconds % 60;

      string strHMS = "";
      if (hh >= 1)
        strHMS = String.Format("{0}:{1:00}:{2:00}", hh, mm, ss);
      else
        strHMS = String.Format("{0}:{1:00}", mm, ss);
      return strHMS;
    }
    static public string GetShortDayString(DateTime dt)
    {
      try
      {
        string day;
        switch (dt.DayOfWeek)
        {
          case DayOfWeek.Monday: day = GUILocalizeStrings.Get(657); break;
          case DayOfWeek.Tuesday: day = GUILocalizeStrings.Get(658); break;
          case DayOfWeek.Wednesday: day = GUILocalizeStrings.Get(659); break;
          case DayOfWeek.Thursday: day = GUILocalizeStrings.Get(660); break;
          case DayOfWeek.Friday: day = GUILocalizeStrings.Get(661); break;
          case DayOfWeek.Saturday: day = GUILocalizeStrings.Get(662); break;
          default: day = GUILocalizeStrings.Get(663); break;
        }
        return String.Format("{0} {1}-{2}", day, dt.Day, dt.Month);
      }
      catch (Exception)
      {
      }
      return String.Empty;
    }
    static public string SecondsToHMString(int lSeconds)
    {
      if (lSeconds < 0) return "0:00";
      int hh = lSeconds / 3600;
      lSeconds = lSeconds % 3600;
      int mm = lSeconds / 60;

      string strHM = "";
      if (hh >= 1)
        strHM = String.Format("{0:00}:{1:00}", hh, mm);
      else
        strHM = String.Format("0:{0:00}", mm);
      return strHM;
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
        string strFileName1 = System.IO.Path.GetFileNameWithoutExtension(strFile1).ToLower();
        string strFileName2 = System.IO.Path.GetFileNameWithoutExtension(strFile2).ToLower();

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

    static public string GetThumb(string strLine)
    {
      if (strLine == null) return "000";
      try
      {
        if (String.Compare(Strings.Unknown, strLine, true) == 0) return "";
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        ulong dwcrc = crc.calc(strLine);
        string strRet = System.IO.Path.GetFullPath(String.Format("thumbs\\{0}.jpg", dwcrc));
        return strRet;
      }
      catch (Exception)
      {
      }
      return "000";
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
        while (i > 0)
        {
          char ch = strFileNameAndPath[i];
          if (ch == ':' || ch == '/' || ch == '\\') break;
          else i--;
        }
        strPath = strFileNameAndPath.Substring(0, i).Trim();
        strFileName = strFileNameAndPath.Substring(i, strFileNameAndPath.Length - i).Trim();
      }
      catch (Exception)
      {
        strPath = "";
        strFileName = strFileNameAndPath;
      }
    }

    static public string GetFolderThumb(string strFile)
    {
      if (strFile == null) return "";
      if (strFile.Length == 0) return "";
      string strPath, strFileName;
      Utils.Split(strFile, out strPath, out strFileName);
      string strFolderJpg = String.Format(@"{0}\folder.jpg", strPath);
      return strFolderJpg;
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

    static public Process StartProcess(ProcessStartInfo procStartInfo, bool bWaitForExit)
    {
      Process proc = new Process();
      proc.StartInfo = procStartInfo;
      try
      {
        Log.Write("Start process {0} {1}", procStartInfo.FileName, procStartInfo.Arguments);
        if (OnStartExternal != null)
        {
          // Event: Starting external process
          OnStartExternal(proc, bWaitForExit);
        }
        proc.Start();
        if (bWaitForExit)
        {
          proc.WaitForExit();
        }
        if (OnStopExternal != null)
        {
          // Event: After launching external process
          OnStopExternal(proc, bWaitForExit);
        }
      }
      catch (Exception ex)
      {
        string ErrorString = String.Format("Utils: Error starting process!\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
          proc.StartInfo.FileName,
          proc.StartInfo.Arguments,
          proc.StartInfo.WorkingDirectory,
          ex.Message,
          ex.Source,
          ex.StackTrace);
        Log.Write(ErrorString);
      }
      return proc;
    }

    static public Process StartProcess(string strProgram, string strParams, bool bWaitForExit, bool bMinimized)
    {
      if (strProgram == null) return null;
      if (strProgram.Length == 0) return null;

      string strWorkingDir = System.IO.Path.GetFullPath(strProgram);
      string strFileName = System.IO.Path.GetFileName(strProgram);
      strWorkingDir = strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length + 1));

      ProcessStartInfo procInfo = new ProcessStartInfo();
      procInfo.FileName = strFileName;
      procInfo.WorkingDirectory = strWorkingDir;
      procInfo.Arguments = strParams;
      if (bMinimized)
      {
        procInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
        procInfo.CreateNoWindow = true;
      }
      return StartProcess(procInfo, bWaitForExit);
    }

    static public bool PlayDVD()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
      {
        string strPath = xmlreader.GetValueAsString("dvdplayer", "path", "");
        string strParams = xmlreader.GetValueAsString("dvdplayer", "arguments", "");
        bool bInternal = xmlreader.GetValueAsBool("dvdplayer", "internal", true);
        if (bInternal) return false;

        if (strPath != "")
        {
          if (System.IO.File.Exists(strPath))
          {
            Process dvdplayer = new Process();

            string strWorkingDir = System.IO.Path.GetFullPath(strPath);
            string strFileName = System.IO.Path.GetFileName(strPath);
            strWorkingDir = strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length + 1));
            dvdplayer.StartInfo.FileName = strFileName;
            dvdplayer.StartInfo.WorkingDirectory = strWorkingDir;

            if (strParams.Length > 0)
            {
              dvdplayer.StartInfo.Arguments = strParams;
            }
            Log.Write("start process {0} {1}", strPath, dvdplayer.StartInfo.Arguments);

            if (OnStartExternal != null)
            {
              OnStartExternal(dvdplayer, true);		// Event: Starting external process
            }
            dvdplayer.Start();
            dvdplayer.WaitForExit();
            if (OnStopExternal != null)
            {
              OnStopExternal(dvdplayer, true);		// Event: External process stopped
            }
            Log.Write("{0} done", strPath);
          }
          else
          {
            Log.Write("file {0} does not exists", strPath);
          }
        }
      }
      return true;
    }

    static public bool PlayMovie(string strFile)
    {
      if (strFile == null) return false;
      if (strFile.Length == 0) return false;

      try
      {
        string extension = System.IO.Path.GetExtension(strFile).ToLower();
        if (strFile.ToLower().IndexOf("live.ts") >= 0) return false;
        if (strFile.ToLower().IndexOf("live.tv") >= 0) return false;
        if (extension.Equals(".sbe")) return false;
        if (extension.Equals(".dvr-ms")) return false;
        if (extension.Equals(".radio")) return false;
        if (strFile.IndexOf("record0.") > 0 || strFile.IndexOf("record1.") > 0 ||
          strFile.IndexOf("record2.") > 0 || strFile.IndexOf("record3.") > 0 ||
          strFile.IndexOf("record4.") > 0 || strFile.IndexOf("record5.") > 0) return false;

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings("MediaPortal.xml"))
        {
          bool bInternal = xmlreader.GetValueAsBool("movieplayer", "internal", true);
          if (bInternal) return false;
          string strPath = xmlreader.GetValueAsString("movieplayer", "path", "");
          string strParams = xmlreader.GetValueAsString("movieplayer", "arguments", "");
          if (extension.ToLower() == ".ifo" || extension.ToLower() == ".vob")
          {
            strPath = xmlreader.GetValueAsString("dvdplayer", "path", "");
            strParams = xmlreader.GetValueAsString("dvdplayer", "arguments", "");
          }
          if (strPath != "")
          {
            if (System.IO.File.Exists(strPath))
            {
              if (strParams.IndexOf("%filename%") >= 0)
                strParams = strParams.Replace("%filename%", "\"" + strFile + "\"");

              Process movieplayer = new Process();
              string strWorkingDir = System.IO.Path.GetFullPath(strPath);
              string strFileName = System.IO.Path.GetFileName(strPath);
              strWorkingDir = strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length + 1));
              movieplayer.StartInfo.FileName = strFileName;
              movieplayer.StartInfo.WorkingDirectory = strWorkingDir;
              if (strParams.Length > 0)
              {
                movieplayer.StartInfo.Arguments = strParams;
              }
              else
              {
                movieplayer.StartInfo.Arguments = "\"" + strFile + "\"";
              }
              Log.Write("start process {0} {1}", strPath, movieplayer.StartInfo.Arguments);
              if (OnStartExternal != null)
              {
                OnStartExternal(movieplayer, true);		// Event: Starting external process
              }
              movieplayer.Start();
              movieplayer.WaitForExit();
              if (OnStopExternal != null)
              {
                OnStopExternal(movieplayer, true);		// Event: External process stopped
              }
              Log.Write("{0} done", strPath);
              return true;
            }
            else
            {
              Log.Write("file {0} does not exists", strPath);
            }
          }
        }
      }
      catch (Exception)
      {
      }
      return false;
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
    static public void DownLoadImage(string strURL, string strFile, System.Drawing.Imaging.ImageFormat imageFormat)
    {
      if (strURL == null) return;
      if (strURL.Length == 0) return;
      if (strFile == null) return;
      if (strFile.Length == 0) return;

      using (WebClient client = new WebClient())
      {
        try
        {
          string extensionURL = System.IO.Path.GetExtension(strURL);
          string extensionFile = System.IO.Path.GetExtension(strFile);
          if (extensionURL.Length > 0 && extensionFile.Length > 0)
          {
            extensionURL = extensionURL.ToLower();
            extensionFile = extensionFile.ToLower();
            string strLogo = System.IO.Path.ChangeExtension(strFile, extensionURL);
            client.DownloadFile(strURL, strLogo);
            if (extensionURL != extensionFile)
            {
              using (Image imgSrc = Image.FromFile(strLogo))
              {
                imgSrc.Save(strFile, imageFormat);
              }
              Utils.FileDelete(strLogo);
            }
            GUITextureManager.CleanupThumbs();
          }
        }
        catch (Exception ex)
        {
          Log.Write("download failed:{0}", ex.Message);
        }
      }
    }


    static public void DownLoadAndCacheImage(string strURL, string strFile)
    {
      if (strURL == null) return;
      if (strURL.Length == 0) return;
      if (strFile == null) return;
      if (strFile.Length == 0) return;
      string url = String.Format("cache{0}", EncryptLine(strURL));

      string file = GetCoverArt("thumbs", url);
      if (file != String.Empty)
      {
        try
        {
          System.IO.File.Copy(file, strFile, true);
        }
        catch (Exception)
        {
        }
        return;
      }
      DownLoadImage(strURL, strFile);
      if (System.IO.File.Exists(strFile))
      {
        try
        {
          file = GetCoverArtName("thumbs", url);
          System.IO.File.Copy(strFile, file, true);
        }
        catch (Exception)
        {
        }
      }

    }
    static public void DownLoadImage(string strURL, string strFile)
    {
      if (strURL == null) return;
      if (strURL.Length == 0) return;
      if (strFile == null) return;
      if (strFile.Length == 0) return;
      try
      {

        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(strURL);
        wr.Timeout = 5000;
        HttpWebResponse ws = (HttpWebResponse)wr.GetResponse();

        Stream str = ws.GetResponseStream();
        byte[] inBuf = new byte[900000];
        int bytesToRead = (int)inBuf.Length;
        int bytesRead = 0;

        DateTime dt = DateTime.Now;
        while (bytesToRead > 0)
        {
          dt = DateTime.Now;
          int n = str.Read(inBuf, bytesRead, bytesToRead);
          if (n == 0)
            break;
          bytesRead += n;
          bytesToRead -= n;
          TimeSpan ts = DateTime.Now - dt;
          if (ts.TotalSeconds >= 5)
          {
            throw new Exception("timeout");
          }
        }
        FileStream fstr = new FileStream(strFile, FileMode.OpenOrCreate, FileAccess.Write);
        fstr.Write(inBuf, 0, bytesRead);
        str.Close();
        fstr.Close();

        GUITextureManager.CleanupThumbs();
      }
      catch (Exception ex)
      {
        Log.Write("download failed:{0}", ex.Message);
      }
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
    static public string GetFilename(string strPath)
    {
      return GetFilename(strPath, false);
    }

    static public string GetFilename(string strPath, bool withoutExtension)
    {
      if (strPath == null) return String.Empty;
      if (strPath.Length == 0) return String.Empty;
      try
      {
        if (m_bHideExtensions || withoutExtension)
          return System.IO.Path.GetFileNameWithoutExtension(strPath);
        else
          return System.IO.Path.GetFileName(strPath);
      }
      catch (Exception)
      {
      }
      return strPath;
    }

    ///<summary>
    ///Plays a sound from a byte array. 
    ///Note: If distortion or corruption of 
    //     audio playback occurs, 
    ///try using synchronous playback, or sa
    //     ve to a temp file and
    ///use the file-based option.
    ///</summary>
    public static int PlaySound(byte[] audio, bool bSynchronous, bool bIgnoreErrors)
    {
      if (audio == null) return 0;
      return PlaySound(audio, bSynchronous, bIgnoreErrors, false, false, false);
    }
    ///<summary>
    ///Plays a sound from a byte array. 
    ///Note: If distortion or corruption of 
    //     audio playback occurs, 
    ///try using synchronous playback, or sa
    //     ve to a temp file and
    ///use the file-based option.
    ///</summary>
    public static int PlaySound(byte[] audio, bool bSynchronous, bool bIgnoreErrors,
      bool bNoDefault, bool bLoop, bool bNoStop)
    {
      if (audio == null) return 0;
      const int SND_ASYNC = 1;
      const int SND_NODEFAULT = 2;
      const int SND_MEMORY = 4;
      const int SND_LOOP = 8;
      const int SND_NOSTOP = 16;
      int Snd_Options = SND_MEMORY;
      if (!bSynchronous)
      {
        Snd_Options += SND_ASYNC;
      }
      if (bNoDefault) Snd_Options += SND_NODEFAULT;
      if (bLoop) Snd_Options += SND_LOOP;
      if (bNoStop) Snd_Options += SND_NOSTOP;
      try
      {
        return PlaySound(audio, 0, Snd_Options);
      }
      catch (Exception ex)
      {
        if (!bIgnoreErrors)
        {
          throw ex;
        }
        else
        {
          return 0;
        }
      }
    }
    public static int PlaySound(string sSoundFile, bool bSynchronous, bool bIgnoreErrors)
    {
      if (sSoundFile == null) return 0;
      if (sSoundFile.Length == 0) return 0;
      return PlaySound(sSoundFile, bSynchronous, bIgnoreErrors, false, false, false);
    }
    public static int PlaySound(string sSoundFile, bool bSynchronous, bool bIgnoreErrors,
      bool bNoDefault, bool bLoop, bool bNoStop)
    {
      if (!enableGuiSounds)
        return 0;

      const int SND_ASYNC = 1;
      const int SND_NODEFAULT = 2;
      const int SND_LOOP = 8;
      const int SND_NOSTOP = 16;

      if (sSoundFile == null) return 0;
      if (sSoundFile.Length == 0) return 0;
      if (!System.IO.File.Exists(sSoundFile))
      {
        string strSkin = GUIGraphicsContext.Skin;
        if (System.IO.File.Exists(strSkin + "\\sounds\\" + sSoundFile))
        {
          sSoundFile = strSkin + "\\sounds\\" + sSoundFile;
        }
        else if (System.IO.File.Exists(strSkin + "\\" + sSoundFile + ".wav"))
        {
          sSoundFile = strSkin + "\\" + sSoundFile + ".wav";
        }
        else
        {
          Log.Write(@"Cannot find sound:{0}\sounds\{1} ", strSkin, sSoundFile);
          return 0;
        }
      }
      int Snd_Options = 0;
      if (!bSynchronous)
      {
        Snd_Options = SND_ASYNC;
      }
      if (bNoDefault) Snd_Options += SND_NODEFAULT;
      if (bLoop) Snd_Options += SND_LOOP;
      if (bNoStop) Snd_Options += SND_NOSTOP;
      try
      {
        return sndPlaySoundA(sSoundFile, Snd_Options);
      }
      catch (Exception ex)
      {
        if (!bIgnoreErrors)
        {
          throw ex;
        }
        else
        {
          return 0;
        }
      }
    }
    [DllImport("winmm.dll")]
    private static extern int sndPlaySoundA(string lpszSoundName, int uFlags);
    [DllImport("winmm.dll")]
    private static extern int PlaySound(byte[] pszSound, Int16 hMod, long fdwSound);
    static public int GetNextForwardSpeed(int iCurSpeed)
    {
      switch (iCurSpeed)
      {
        case -32:
          return -16;
        case -16:
          return -8;
        case -8:
          return -4;
        case -4:
          return -2;
        case -2:
          return 1;
        case 1:
          return 2;
        case 2:
          return 4;
        case 4:
          return 8;
        case 8:
          return 16;
        case 16:
          return 32;
      }

      return 1;
    }

    static public int GetNextRewindSpeed(int iCurSpeed)
    {
      switch (iCurSpeed)
      {
        case -16:
          return -32;
        case -8:
          return -16;
        case -4:
          return -8;
        case -2:
          return -4;
        case 1:
          return -2;
        case 2:
          return 1;
        case 4:
          return 2;
        case 8:
          return 4;
        case 16:
          return 8;
        case 32:
          return 16;
      }
      return 1;
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

    static public string EncryptLine(string strLine)
    {
      if (strLine == null) return String.Empty;
      if (strLine.Length == 0) return String.Empty;
      if (String.Compare(Strings.Unknown, strLine, true) == 0) return String.Empty;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);
      ulong dwcrc = crc.calc(strLine);
      string strRet = String.Format("{0}", dwcrc);
      return strRet;
    }

    static public string GetCoverArt(string strFolder, string strFileName)
    {
      if (strFolder == null) return String.Empty;
      if (strFolder.Length == 0) return String.Empty;

      if (strFileName == null) return String.Empty;
      if (strFileName.Length == 0) return String.Empty;
      if (strFileName == String.Empty) return String.Empty;/*
			try
			{
				string tbnImage = System.IO.Path.ChangeExtension(strFileName,".tbn");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
				tbnImage = System.IO.Path.ChangeExtension(strFileName,".png");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
				tbnImage = System.IO.Path.ChangeExtension(strFileName,".gif");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
				tbnImage = System.IO.Path.ChangeExtension(strFileName,".jpg");
				if (System.IO.File.Exists(tbnImage)) return tbnImage;
			}
			catch(Exception){}*/

      string strThumb = String.Format(@"{0}\{1}", strFolder, Utils.FilterFileName(strFileName));
      if (System.IO.File.Exists(strThumb + ".jpg")) return strThumb + ".jpg";
      else if (System.IO.File.Exists(strThumb + ".png")) return strThumb + ".png";
      else if (System.IO.File.Exists(strThumb + ".gif")) return strThumb + ".gif";
      else if (System.IO.File.Exists(strThumb + ".tbn")) return strThumb + ".tbn";
      return String.Empty;
    }

    static public string ConvertToLargeCoverArt(string smallArt)
    {
      if (smallArt == null) return String.Empty;
      if (smallArt.Length == 0) return String.Empty;
      if (smallArt == String.Empty) return smallArt;
      return smallArt.Replace(".jpg", "L.jpg");
    }

    static public string GetCoverArtName(string strFolder, string strFileName)
    {
      if (strFolder == null) return String.Empty;
      if (strFolder.Length == 0) return String.Empty;

      if (strFileName == null) return String.Empty;
      if (strFileName.Length == 0) return String.Empty;

      string strThumb = Utils.GetCoverArt(strFolder, strFileName);
      if (strThumb == string.Empty)
      {
          strThumb = String.Format(@"{0}\{1}.jpg", strFolder, Utils.FilterFileName(strFileName));
      }
      return strThumb;
    }
    static public string GetLargeCoverArtName(string strFolder, string strFileName)
    {
      if (strFolder == null) return String.Empty;
      if (strFolder.Length == 0) return String.Empty;

      if (strFileName == null) return String.Empty;
      if (strFileName.Length == 0) return String.Empty;

      return Utils.GetCoverArtName(strFolder, strFileName+"L");
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

    static public void StopMCEServices()
    {
      bool ehRecvrExist = false;
      bool ehSchedExist = false;
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
      if ((ehRecvrExist && (ehRecvr.Status != ServiceControllerStatus.Stopped) && (ehRecvr.Status != ServiceControllerStatus.StopPending))
        || (ehSchedExist && (ehSched.Status != ServiceControllerStatus.Stopped) && (ehSched.Status != ServiceControllerStatus.StopPending)))
      {
        Log.Write("  Stopping Microsoft Media Center services");
        try
        {
          if ((ehRecvr.Status != ServiceControllerStatus.Stopped) && (ehRecvr.Status != ServiceControllerStatus.StopPending))
          {
            ehRecvr.Stop();
            restartMCEehRecvr = true;
          }
        }
        catch
        {
          Log.Write("Error stopping MCE service \"ehRecvr\"");
        }
        try
        {
          if ((ehSched.Status != ServiceControllerStatus.Stopped) && (ehSched.Status != ServiceControllerStatus.StopPending))
          {
            ehSched.Stop();
            restartMCEehSched = true;
          }
        }
        catch
        {
          Log.Write("Error stopping MCE service \"ehSched\"");
        }
      }
    }

    static public void RestartMCEServices()
    {
      if (restartMCEehRecvr || restartMCEehSched)
      {
        Log.Write("Restarting MCE Services");

        try
        {
          if (restartMCEehRecvr)
            ehRecvr.Start();
        }
        catch (Exception ex)
        {
          if (ehRecvr.Status != ServiceControllerStatus.Running)
            Log.Write("Error starting MCE service \"ehRecvr\" {0}", ex.ToString());
        }

        try
        {
          if (restartMCEehSched)
            ehSched.Start();
        }
        catch (Exception ex)
        {
          if (ehSched.Status != ServiceControllerStatus.Running)
            Log.Write("Error starting MCE service \"ehSched\" {0}", ex.ToString());
        }
      }
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

    public static void ExportEmbeddedResource(string resourceName, string path)
    {
      ExportEmbeddedResource(Assembly.GetCallingAssembly(), resourceName, path);
    }

    public static void ExportEmbeddedResource(Assembly resourceAssembly, string resourceName, string path)
    {
      try
      {
        using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(resourceName))
        {
          byte[] buffer = new byte[resourceStream.Length];

          if (resourceStream.Read(buffer, 0, buffer.Length) == buffer.Length)
          {
            using (Stream destinationStream = File.Create(path))
              destinationStream.Write(buffer, 0, buffer.Length);
          }
        }
      }
      catch (Exception e)
      {
        Log.Write("Util.ExportEmbeddedResource: {0}", e.Message);
      }
    }

    public static string ReplaceTag(string line, string tag, string value, string empty)
    {
      if (line == null) return String.Empty;
      if (line.Length == 0) return String.Empty;
      if (tag == null) return line;
      if (tag.Length == 0) return line;

      Regex r = new Regex(String.Format(@"\[[^%]*{0}[^\]]*[\]]", tag));
      if (value == empty)
      {
        Match match = r.Match(line);
        if (match != null && match.Length > 0)
        {
          line = line.Remove(match.Index, match.Length);
        }
      }
      else
      {
        Match match = r.Match(line);
        if (match != null && match.Length > 0)
        {
          line = line.Remove(match.Index, match.Length);
          string m = match.Value.Substring(1, match.Value.Length - 2);
          line = line.Insert(match.Index, m);
        }
      }
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

    static public void DeleteOldTimeShiftFiles(string path)
    {
      if (path == null) return;
      if (path == String.Empty) return;
      // Remove any trailing slashes
      path = Utils.RemoveTrailingSlash(path);


      // clean the TempDVR\ folder
      string directory = String.Empty;
      string[] files;
      try
      {
        directory = String.Format(@"{0}\TempDVR", path);
        if (System.IO.Directory.Exists(directory))
        {
          files = System.IO.Directory.GetFiles(directory, "*.tmp");
          foreach (string fileName in files)
          {
            try
            {
              System.IO.File.Delete(fileName);
            }
            catch (Exception) { }
          }
        }
      }
      catch (Exception) { }

      // clean the TempSBE\ folder
      try
      {
        directory = String.Format(@"{0}\TempSBE", path);
        if (System.IO.Directory.Exists(directory))
        {
          files = System.IO.Directory.GetFiles(directory, "*.tmp");
          foreach (string fileName in files)
          {
            try
            {
              System.IO.File.Delete(fileName);
            }
            catch (Exception) { }
          }
        }
      }
      catch (Exception) { }

      // delete *.tv
      try
      {
        directory = String.Format(@"{0}", path);
        if (System.IO.Directory.Exists(directory))
        {
          files = System.IO.Directory.GetFiles(directory, "*.tv");
          foreach (string fileName in files)
          {
            try
            {
              System.IO.File.Delete(fileName);
            }
            catch (Exception) { }
          }
        }
      }
      catch (Exception) { }
    }//void DeleteOldTimeShiftFiles(string path)

    static public void DeleteRecording(string recordingFilename)
    {
      Utils.FileDelete(recordingFilename);

      int pos = recordingFilename.LastIndexOf(@"\");
      if (pos < 0) return;
      string path = recordingFilename.Substring(0, pos);
      string filename = recordingFilename.Substring(pos + 1);
      pos = filename.LastIndexOf(".");
      if (pos >= 0)
        filename = filename.Substring(0, pos);
      filename = filename.ToLower();
      string[] files;
      try
      {
        files = System.IO.Directory.GetFiles(path);
        foreach (string fileName in files)
        {
          try
          {
            if (fileName.ToLower().IndexOf(filename) >= 0)
            {
              if (fileName.ToLower().IndexOf(".sbe") >= 0)
              {
                System.IO.File.Delete(fileName);
              }
            }
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }

    static public bool HibernateSystem(bool forceShutDown)
    {
      Log.Write("Utils: Hibernate system");
      return (SetSuspendState(PowerState.Hibernate, forceShutDown));
    }

    static public bool SuspendSystem(bool forceShutDown)
    {
      Log.Write("Utils: Suspend system");
      return (SetSuspendState(PowerState.Suspend, forceShutDown));
    }

    static private bool SetSuspendState(PowerState state, bool forceShutDown)
    {
      g_Player.Stop();
      AutoPlay.StopListening();
      GUIWindowManager.Dispose();
      GUITextureManager.Dispose();
      GUIFontManager.Dispose();

      return (Application.SetSuspendState(state, forceShutDown, false));
    }

    static public string EncryptPin(string code)
    {
      string result = string.Empty;
      foreach (char c in code)
        try
        {
          result += crypt[(int)c - 48];
        }
        catch { }
      return result;

    }

    static public string DecryptPin(string code)
    {
      string result = string.Empty;
      foreach (char c in code)
      {
        try
        {
          for (int i = 0; i < crypt.Length; i++)
            if (crypt[i] == c)
              result += (i).ToString();
        }
        catch { }
      }
      return result;
    }

  }
}
