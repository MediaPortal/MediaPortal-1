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
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections;
using System.Management;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Windows.Forms;

using Microsoft.Win32;

using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using MediaPortal.Configuration;

namespace MediaPortal.Util
{
  public struct LanguageInfo
  {
    public string EnglishName;
    public string TwoLetterISO;
  };


  /// <summary>
  /// Common functions for general usage
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

    [DllImport("mpr.dll")]
    static extern int WNetAddConnection2A(ref NetResource pstNetRes, string psPassword, string psUsername, int piFlags);

    private const int CONNECT_UPDATE_PROFILE = 0x00000001;
    private const int RESOURCETYPE_DISK = 0x1;

    [StructLayout(LayoutKind.Sequential)]
    private struct NetResource
    {
      public int Scope;
      public int Type;
      public int DisplayType;
      public int Usage;
      public string LocalName;
      public string RemoteName;
      public string Comment;
      public string Provider;
    }

    public delegate void UtilEventHandler(Process proc, bool waitForExit);
    public static event UtilEventHandler OnStartExternal = null;	// Event: Start external process / waeberd & mPod
    public static event UtilEventHandler OnStopExternal = null;		// Event: Stop external process	/ waeberd & mPod
    static ArrayList m_AudioExtensions = new ArrayList();
    static ArrayList m_VideoExtensions = new ArrayList();
    static ArrayList m_PictureExtensions = new ArrayList();

    static string[] _artistNamePrefixes;

    static bool m_bHideExtensions = false;
    static bool enableGuiSounds;

    static char[] crypt = new char[10] { 'G', 'D', 'J', 'S', 'I', 'B', 'T', 'P', 'W', 'Q' };


    // singleton. Dont allow any instance of this class
    private Utils()
    {
    }

    static Utils()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        m_bHideExtensions = xmlreader.GetValueAsBool("general", "hideextensions", true);
        string artistNamePrefixes = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");
        _artistNamePrefixes = artistNamePrefixes.Split(',');

        string strTmp = xmlreader.GetValueAsString("music", "extensions", ".mp3,.wma,.ogg,.flac,.wav,.cda,.m3u,.pls,.b4s,.m4a,.m4p,.mp4,.wpl,.wv,.ape,.mpc");
        Tokens tok = new Tokens(strTmp, new[] { ',' });
        foreach (string extension in tok)
        {
          m_AudioExtensions.Add(extension.ToLower());
        }

        strTmp = xmlreader.GetValueAsString("movies", "extensions", ".avi,.mpg,.mpeg,.mp4,.divx,.ogm,.mkv,.wmv,.qt,.rm,.mov,.mts,.sbe,.dvr-ms,.ts,.dat,.ifo,.iso");
        tok = new Tokens(strTmp, new[] { ',' });
        foreach (string extension in tok)
        {
          m_VideoExtensions.Add(extension.ToLower());
        }

        strTmp = xmlreader.GetValueAsString("pictures", "extensions", ".jpg,.jpeg,.gif,.bmp,.png");
        tok = new Tokens(strTmp, new[] { ',' });
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

    public static string GetDriveSerial(string drive)
    {
      if (drive == null) return string.Empty;
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

    public static string GetDriveName(string drive)
    {
      if (drive == null) return string.Empty;
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

    public static int getDriveType(string drive)
    {
      if (drive == null) return 2;
      if ((GetDriveType(drive) & 5) == 5) return 5;//cd
      if ((GetDriveType(drive) & 3) == 3) return 3;//fixed
      if ((GetDriveType(drive) & 2) == 2) return 2;//removable
      if ((GetDriveType(drive) & 4) == 4) return 4;//remote disk
      if ((GetDriveType(drive) & 6) == 6) return 6;//ram disk
      return 0;
    }

    public static string GetUNCPath(string sFilePath)
    {
      if (sFilePath.StartsWith("\\\\"))
        return sFilePath;

      try
      {
        ManagementObject mo = new ManagementObject();
        mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", sFilePath));

        //DriveType 4 = Network Drive
        if (Convert.ToUInt32(mo["DriveType"]) == 4)
          return Convert.ToString(mo["ProviderName"]);
      }
      catch (Exception) { }
      return sFilePath;
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

    public static bool IsLiveTv(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (strPath.ToLower().IndexOf("live.tv") >= 0) return true;
        if (strPath.ToLower().IndexOf("live.ts") >= 0) return true;
        if (strPath.ToLower().IndexOf("ts.tsbuffer") >= 0) return true;
      }
      catch (Exception) { }
      return false;
    }

    public static bool IsRTSP(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (strPath.ToLower().IndexOf("rtsp:") >= 0) return true;
      }
      catch (Exception) { }
      return false;
    }

    public static bool IsLiveRadio(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (strPath.ToLower().IndexOf("radio.ts") >= 0) return true;
      }
      catch (Exception) { }
      return false;

    }

    public static bool IsVideo(string strPath)
    {
      if (strPath == null) return false;
      if (IsLastFMStream(strPath)) return false;
      if (strPath.ToLower().StartsWith("rtsp:")) return true;
      if (strPath.ToLower().StartsWith("mms:")
        && strPath.ToLower().EndsWith(".ymvp")) return true;
      try
      {
        if (!Path.HasExtension(strPath))
          return false;
        if (IsPlayList(strPath))
          return false;
        string extensionFile = Path.GetExtension(strPath).ToLower();
        switch (extensionFile)
        {
          case ".tv":
          case ".ts":
          case ".sbe":
          case ".dvr-ms":
            return true;
        }
        if (VirtualDirectory.IsImageFile(extensionFile.ToLower()))
          return true;
        foreach (string extension in m_VideoExtensions)
        {
          if (extension == extensionFile)
            return true;
        }
      }
      catch (Exception)
      {
      }
      return false;
    }

    public static bool IsLastFMStream(string aPath)
    {
      try
      {
        if (aPath.StartsWith(@"http://"))
        {
          if (aPath.IndexOf(@"/last.mp3?") > 0)
            return true;
          if (aPath.Contains(@"last.fm/"))
            return true;
        }
      }
      catch (Exception ex)
      {
        Log.Warn("Util: Error in IsLastFMStream - {0}", ex.Message);
      }
      return false;
    }

    public static bool IsAVStream(string strPath)
    {
      if (strPath == null) return false;
      if (strPath.ToLower().IndexOf("http:") >= 0) return true;
      if (strPath.ToLower().IndexOf("https:") >= 0) return true;
      if (strPath.ToLower().IndexOf("mms:") >= 0) return true;
      return false;
    }

    public static bool IsAudio(string strPath)
    {
      if (strPath == null) return false;
      if (IsLastFMStream(strPath)) return true;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        if (IsPlayList(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLower();
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

    public static bool IsPicture(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        if (IsPlayList(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLower();
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

    public static bool IsPlayList(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLower();
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
    public static bool IsProgram(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLower();
        if (extensionFile == ".exe") return true;
      }
      catch (Exception)
      {
      }
      return false;
    }
    public static bool IsShortcut(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLower();
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
          if (IsNetwork(item.Path))
          {
            item.IconImage = "defaultNetwork.png";
            item.IconImageBig = "defaultNetworkBig.png";
          }
          else
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
      string strThumb = string.Empty;

      if (!item.IsFolder || (item.IsFolder && VirtualDirectory.IsImageFile(Path.GetExtension(item.Path).ToLower())))
      {
        if (IsPicture(item.Path)) return;

        strThumb = Path.ChangeExtension(item.Path, ".jpg");

        if (!File.Exists(strThumb))
        {
          strThumb = Path.ChangeExtension(item.Path, ".tbn");
          if (!File.Exists(strThumb))
          {
            strThumb = Path.ChangeExtension(item.Path, ".png");
            if (!File.Exists(strThumb))
            {
              strThumb = GetThumb(item.Path);
              if (!File.Exists(strThumb))
              {
                bool createVideoThumbs = false;
                using (Profile.Settings xmlreader = new Profile.MPSettings())
                {
                  createVideoThumbs = xmlreader.GetValueAsBool("thumbnails", "tvrecordedondemand", true);
                }

                if (IsVideo(item.Path) && !VirtualDirectory.IsImageFile(Path.GetExtension(item.Path).ToLower()))
                {
                  strThumb = String.Format(@"{0}\{1}.jpg", Thumbs.Videos, EncryptLine(item.Path));
                  if (File.Exists(strThumb))
                  {
                    try
                    {
                      File.SetLastAccessTime(strThumb, DateTime.Now); //useful for later creating a process plugin that deletes old thumbnails
                      if (!File.Exists(ConvertToLargeCoverArt(strThumb)))
                      {
                        File.Copy(strThumb, ConvertToLargeCoverArt(strThumb));
                        File.SetLastAccessTime(ConvertToLargeCoverArt(strThumb), DateTime.Now);
                      }
                    }
                    catch (Exception ex)
                    {
                      Log.Error(ex);
                    }
                    item.IconImage = strThumb;
                    string strLargeThumb = ConvertToLargeCoverArt(strThumb);
                    if (File.Exists(strLargeThumb))
                    {
                      item.ThumbnailImage = strLargeThumb;
                      item.IconImageBig = strLargeThumb;
                    }
                    else
                    {
                      item.ThumbnailImage = strThumb;
                      item.IconImageBig = strThumb;
                    }

                  }
                  else if (createVideoThumbs)
                  {
                    Thread extractVideoThumbThread = new Thread(GetVideoThumb)
                                                       {
                                                         Name = "ExtractVideoThumb",
                                                         IsBackground = true,
                                                         Priority = ThreadPriority.Lowest
                                                       };
                    extractVideoThumbThread.Start(item);
                  }
                }

              }
              return;
            }
          }
        }

        // now strThumb exists
        item.ThumbnailImage = strThumb;
        item.IconImage = strThumb;
        item.IconImageBig = strThumb;
      }
      else
      {
        if (item.Label != "..")
        {
          strThumb = item.Path + @"\folder.jpg";
          if (File.Exists(strThumb))
          {
            item.ThumbnailImage = strThumb;
            item.IconImage = strThumb;
            item.IconImageBig = strThumb;
          }
        }
      }
      if (!string.IsNullOrEmpty(strThumb))
      {
        strThumb = ConvertToLargeCoverArt(strThumb);
        if (File.Exists(strThumb))
          item.ThumbnailImage = strThumb;
      }
    }

    public static void GetVideoThumb(object i)
    {
      GUIListItem item = (GUIListItem)i;
      string path = item.Path;
      string strThumb = String.Format(@"{0}\{1}.jpg", Thumbs.Videos, EncryptLine(path));
      if (File.Exists(strThumb))
      {
        return;
      }

      // Do not try to create thumbnails for DVDs
      if (path.Contains("VIDEO_TS\\VIDEO_TS.IFO"))
      {
        return;
      }

      Image thumb = null;
      try
      {
        bool success = VideoThumbCreator.CreateVideoThumb(path, strThumb, true, false);
        if (!success)
        {
          //Failed due to incompatible format or no write permissions on folder. Try querying Explorer for thumb.
          Log.Warn("Failed to extract thumb for {0}, trying another method.", path);
          if (Environment.OSVersion.Version.Major >= 6)
          {
            thumb = VistaToolbelt.Shell.ThumbnailGenerator.GenerateThumbnail(path); //only works for Vista/7
          }
          else
          {
            using (ThumbnailExtractor extractor = new ThumbnailExtractor())
            {
              thumb = extractor.GetThumbnail(path); //works on XP but not too well threaded
            }
          }
          if (thumb != null)
            if (Picture.CreateThumbnail(thumb, strThumb, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false))
              SetThumbnails(ref item);
        }
        else
          SetThumbnails(ref item);
      }
      catch (COMException comex)
      {
        if (comex.ErrorCode == unchecked((int)0x8004B200))
        {
          Log.Warn("Could not create thumbnail for {0} [Unknown error 0x8004B200]", path);
        }
        else
        {
          Log.Error("Could not create thumbnail for {0}", path);
          Log.Error(comex);
        }
      }
      catch (Exception ex)
      {
        Log.Error("Could not create thumbnail for {0}", path);
        Log.Error(ex);
      }
      finally
      {
        if (thumb != null)
          thumb.Dispose();
      }
    }

    public static string SecondsToShortHMSString(int lSeconds)
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

    public static string SecondsToHMSString(TimeSpan timespan)
    {
      return SecondsToHMSString(timespan.Seconds);
    }

    public static string SecondsToHMSString(int lSeconds)
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


    public static string GetNamedMonth(string aTwoLetterMonth)
    {
      string readableMonth = "";
      switch (aTwoLetterMonth)
      {
        case "01":
          readableMonth = GUILocalizeStrings.Get(21); //January
          break;
        case "02":
          readableMonth = GUILocalizeStrings.Get(22); //February
          break;
        case "03":
          readableMonth = GUILocalizeStrings.Get(23); //March
          break;
        case "04":
          readableMonth = GUILocalizeStrings.Get(24); //April
          break;
        case "05":
          readableMonth = GUILocalizeStrings.Get(25); //May
          break;
        case "06":
          readableMonth = GUILocalizeStrings.Get(26); //June
          break;
        case "07":
          readableMonth = GUILocalizeStrings.Get(27); //July
          break;
        case "08":
          readableMonth = GUILocalizeStrings.Get(28); //August
          break;
        case "09":
          readableMonth = GUILocalizeStrings.Get(29); //September
          break;
        case "10":
          readableMonth = GUILocalizeStrings.Get(30); //October          
          break;
        case "11":
          readableMonth = GUILocalizeStrings.Get(31); //November
          break;
        case "12":
          readableMonth = GUILocalizeStrings.Get(32); //December
          break;
      }
      return readableMonth;
    }

    public static string GetNamedDate(DateTime aDateTime)
    {
      DateTime now = DateTime.Now;
      if (aDateTime.Date == now.Date) // Today
      {
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6030), aDateTime.ToString("HH:mm"));
      }
      else if (aDateTime.Date == now.Date.AddDays(1)) // Tomorrow
      {
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6031), aDateTime.ToString("HH:mm"));
      }
      else if (aDateTime.Date.AddDays(1) == now.Date) // Yesterday
      {
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6040), aDateTime.ToString("HH:mm"));
      }
      else if (aDateTime.Date.AddDays(2) == now.Date) // Two days ago
      {
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6041), aDateTime.ToString("HH:mm"));
      }

      return String.Format("{0} {1}", aDateTime.ToShortDateString(), aDateTime.ToString("HH:mm"));
    }

    public static string GetShortDayString(DateTime dt)
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
      return string.Empty;
    }

    public static string SecondsToHMString(int lSeconds)
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

    public static long GetUnixTime(DateTime desiredTime_)
    {
      TimeSpan ts = (desiredTime_ - new DateTime(1970, 1, 1, 0, 0, 0));

      return (long)ts.TotalSeconds;
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
      strFileName = Path.Combine(strBasePath, strFileName);
    }

    public static string stripHTMLtags(string strHTML)
    {
      if (strHTML == null) return string.Empty;
      if (strHTML.Length == 0) return string.Empty;
      string stripped = Regex.Replace(strHTML, @"<(.|\n)*?>", string.Empty);
      return stripped.Trim();
    }

    public static bool IsNetwork(string strPath)
    {
      if (strPath == null) return false;
      if (strPath.Length < 2) return false;
      if (strPath.StartsWith(@"\\")) return true;
      string strDrive = strPath.Substring(0, 2);
      if (getDriveType(strDrive) == 4) return true;
      return false;
    }

    public static bool IsPersistentNetwork(string strPath)
    {
      //IsNetwork doesn't work correctly, when the drive is disconnected (for whatever reason)
      try
      {
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(string.Format(@"Network\{0}", strPath.Substring(0, 1)));
        return (regKey != null);
      }
      catch (Exception)
      {
      }
      return false;
    }

    public static bool TryReconnectNetwork(string strPath)
    {
      string driveLetter = strPath.Substring(0, 1);

      try
      {
        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(string.Format(@"Network\{0}", driveLetter));
        if (regKey == null)
          return false;

        //we can't restore drives with stored auth data, so check it
        object userName = regKey.GetValue("UserName", null);
        if (userName != null)
        {
          switch (regKey.GetValueKind("UserName"))
          {
            case RegistryValueKind.DWord:
              if ((int)userName != 0)
                return false;

              break;
            case RegistryValueKind.String:
              if (userName.ToString() != string.Empty)
                return false;

              break;
          }
        }

        object remotePath = regKey.GetValue("RemotePath", null);
        if (remotePath == null)
          return false;

        NetResource netRes = new NetResource();

        netRes.Scope = 2;
        netRes.Type = RESOURCETYPE_DISK;
        netRes.DisplayType = 3;
        netRes.Usage = 1;
        netRes.RemoteName = remotePath.ToString();
        netRes.LocalName = driveLetter + ":";

        WNetAddConnection2A(ref netRes, null, null, CONNECT_UPDATE_PROFILE);
      }
      catch (Exception exp)
      {
        Log.Error("Could not reconnect network drive '{0}{1}'. Error: {2}", driveLetter, ":", exp.Message);

        return false;
      }

      return true;
    }

    public static string[] GetDirectories(string path)
    {
      string[] dirs = null;

      try
      {
        dirs = Directory.GetDirectories(path);
      }
      catch (DirectoryNotFoundException)
      {
        if (Utils.IsPersistentNetwork(path))
        {
          if (Utils.TryReconnectNetwork(path))
          {
            try
            {
              dirs = Directory.GetDirectories(path);
            }
            catch (Exception exp)
            {
              Log.Error("Could not open directory '{0}'. Error: {1}", path, exp.Message);
            }
          }
        }
      }
      catch (Exception exp)
      {
        Log.Error("Could not open directory '{0}'. Error: {1}", path, exp.Message);
      }

      return dirs;
    }

    public static string[] GetFiles(string path)
    {
      string[] files = null;

      try
      {
        files = Directory.GetFiles(path);
      }
      catch (DirectoryNotFoundException)
      {
        if (Utils.IsPersistentNetwork(path))
        {
          if (Utils.TryReconnectNetwork(path))
          {
            try
            {
              files = Directory.GetFiles(path);
            }
            catch (Exception exp)
            {
              Log.Error("Could not open directory '{0}'. Error: {1}", path, exp.Message);
            }
          }
        }
      }
      catch (Exception exp)
      {
        Log.Error("Could not open directory '{0}'. Error: {1}", path, exp.Message);
      }

      return files;
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

    public static string GetObjectCountLabel(int iTotalItems)
    {
      string strObjects = string.Empty;

      if (iTotalItems == 1)
        strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(1062)); //Object
      else
        strObjects = String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632)); //Objects

      return strObjects;
    }

    public static string GetSongCountLabel(int iTotalItems, int iTotalSeconds)
    {
      string strObjects = string.Empty;

      if (iTotalItems == 1)
        strObjects = String.Format("{0} {1}, {2}", iTotalItems, GUILocalizeStrings.Get(179),
          MediaPortal.Util.Utils.SecondsToHMSString(iTotalSeconds)); //Song
      else
        strObjects = String.Format("{0} {1}, {2}", iTotalItems, GUILocalizeStrings.Get(1052),
          MediaPortal.Util.Utils.SecondsToHMSString(iTotalSeconds)); //Songs

      return strObjects;
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
        string[] pattern = StackExpression();

        // Strip the extensions and make everything lowercase
        string strFileName1 = Path.GetFileNameWithoutExtension(strFile1).ToLower();
        string strFileName2 = Path.GetFileNameWithoutExtension(strFile2).ToLower();

        // Check all the patterns
        for (int i = 0; i < pattern.Length; i++)
        {
          // See if we can find the special patterns in both filenames
          if (Regex.IsMatch(strFileName1, pattern[i], RegexOptions.IgnoreCase) && Regex.IsMatch(strFileName2, pattern[i], RegexOptions.IgnoreCase))
          {
            // Both strings had the special pattern. Now see if the filenames are the same.
            // Do this by removing the special pattern and compare the remains.
            if (Regex.Replace(strFileName1, pattern[i], "", RegexOptions.IgnoreCase)
              == Regex.Replace(strFileName2, pattern[i], "", RegexOptions.IgnoreCase))
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

    public static void RemoveStackEndings(ref string strFileName)
    {

      if (strFileName == null) return;
      string[] pattern = StackExpression();
      for (int i = 0; i < pattern.Length; i++)
      {
        // See if we can find the special patterns in both filenames
        if (Regex.IsMatch(strFileName, pattern[i], RegexOptions.IgnoreCase))
        {
          strFileName = Regex.Replace(strFileName, pattern[i], "", RegexOptions.IgnoreCase);
        }
      }
    }

    public static string[] StackExpression()
    {
      // Patterns that are used for matching
      // 1st pattern matches [x-y] for example [1-2] which is disc 1 of 2 total
      // 2nd pattern matches ?cd?## and ?disc?## for example -cd2 which is cd 2.
      //     ? is -_+ or space (second ? is optional), ## is 1 or 2 digits
      //
      // Chemelli: added "+" as separator to allow IMDB scripts usage of this function
      //
      string[] pattern = {"\\[[0-9]{1,2}-[0-9]{1,2}\\]",
                          "[-_+ ]\\({0,1}(cd|dis[ck]|part|dvd)[-_+ ]{0,1}[0-9]{1,2}\\){0,1}"};
      return pattern;
    }

    public static string GetThumb(string strLine)
    {
      if (string.IsNullOrEmpty(strLine))
        return "000";
      try
      {
        if (String.Compare(Strings.Unknown, strLine, true) == 0) return "";
        CRCTool crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
        ulong dwcrc = crc.calc(strLine);
        string strRet = Path.GetFullPath(String.Format("{0}{1}.jpg", Config.GetFolder(Config.Dir.Thumbs), dwcrc));
        return strRet;
      }
      catch (Exception)
      {
      }
      return "000";
    }

    public static string SplitFilename(string strFileNameAndPath)
    {
      string path = string.Empty;
      string singlefilename = string.Empty;
      Split(strFileNameAndPath, out path, out singlefilename);
      return singlefilename;
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
        IntPtr fHandle = CreateFile(strDrive, FileAccess.Read, FileShare.ReadWrite, 0, FileMode.Open, 0x80, IntPtr.Zero);
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

    public static void EjectCDROM()
    {
      mciSendString("set cdaudio door open", null, 0, IntPtr.Zero);
    }

    public static Process StartProcess(ProcessStartInfo procStartInfo, bool bWaitForExit)
    {
      Process proc = new Process();
      proc.StartInfo = procStartInfo;
      try
      {
        Log.Info("Start process {0} {1}", procStartInfo.FileName, procStartInfo.Arguments);
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
        Log.Info(ErrorString);
      }
      return proc;
    }

    public static Process StartProcess(string strProgram, string strParams, bool bWaitForExit, bool bMinimized)
    {
      if (strProgram == null) return null;
      if (strProgram.Length == 0) return null;

      string strWorkingDir = Path.GetFullPath(strProgram);
      string strFileName = Path.GetFileName(strProgram);
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

    private static void OutputDataHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
    {
      if (!String.IsNullOrEmpty(outLine.Data))
      {
        Log.Debug("Util: StdOut - {0}", outLine.Data);
      }
    }

    private static void ErrorDataHandler(object sendingProcess,
        DataReceivedEventArgs errLine)
    {
      if (!String.IsNullOrEmpty(errLine.Data))
      {
        Log.Debug("Util: StdErr - {0}", errLine.Data);
      }
    }


    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool StartProcess(string aAppName, string aArguments, string aWorkingDir, int aExpectedTimeoutMs, bool aLowerPriority, ProcessFailedConditions aFailConditions)
    {
      bool success = false;
      Process ExternalProc = new Process();
      ProcessStartInfo ProcOptions = new ProcessStartInfo(aAppName, aArguments);

      ProcOptions.UseShellExecute = false;                                       // Important for WorkingDirectory behaviour
      ProcOptions.RedirectStandardError = true;                                  // .NET bug? Some stdout reader abort to early without that!
      ProcOptions.RedirectStandardOutput = true;                                 // The precious data we're after
      //ProcOptions.StandardOutputEncoding = Encoding.GetEncoding("ISO-8859-1"); // the output contains "Umlaute", etc.
      //ProcOptions.StandardErrorEncoding = Encoding.GetEncoding("ISO-8859-1");
      ProcOptions.WorkingDirectory = aWorkingDir;                                // set the dir because the binary might depend on cygwin.dll
      ProcOptions.CreateNoWindow = true;                                         // Do not spawn a "Dos-Box"      
      ProcOptions.ErrorDialog = false;                                           // Do not open an error box on failure        

      ExternalProc.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);
      ExternalProc.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataHandler);
      ExternalProc.EnableRaisingEvents = true;                                   // We want to know when and why the process died        
      ExternalProc.StartInfo = ProcOptions;
      if (File.Exists(ProcOptions.FileName))
      {
        try
        {
          ExternalProc.Start();
          if (aLowerPriority)
          {
            try
            {
              ExternalProc.PriorityClass = ProcessPriorityClass.BelowNormal;            // Execute all processes in the background so movies, etc stay fluent
            }
            catch (Exception ex2)
            {
              Log.Error("Util: Error setting process priority for {0}: {1}", aAppName, ex2.Message);
            }
          }
          // Read in asynchronous  mode to avoid deadlocks (if error stream is full)
          // http://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.redirectstandarderror.aspx
          ExternalProc.BeginErrorReadLine();
          ExternalProc.BeginOutputReadLine();

          // wait this many seconds until the process has to be finished
          ExternalProc.WaitForExit(aExpectedTimeoutMs);

          success = (ExternalProc.HasExited && ExternalProc.ExitCode == aFailConditions.SuccessExitCode);

          ExternalProc.OutputDataReceived -= new DataReceivedEventHandler(OutputDataHandler);
          ExternalProc.ErrorDataReceived -= new DataReceivedEventHandler(ErrorDataHandler);
        }
        catch (Exception ex)
        {
          Log.Error("Util: Error executing {0}: {1}", aAppName, ex.Message);
        }
      }
      else
        Log.Warn("Util: Could not start {0} because it doesn't exist!", ProcOptions.FileName);

      return success;
    }

    public static void KillProcess(string aProcessName)
    {
      try
      {
        Process[] leftovers = System.Diagnostics.Process.GetProcessesByName(aProcessName);
        foreach (Process termProc in leftovers)
        {
          try
          {
            Log.Warn("Util: Killing process: {0}", termProc.ProcessName);
            termProc.Kill();
          }
          catch (Exception exk)
          {
            Log.Error("Util: Error stopping processes - {0})", exk.ToString());
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Util: Error getting processes by name for {0} - {1})", aProcessName, ex.ToString());
      }
    }

    public class ProcessFailedConditions
    {
      public List<string> CriticalOutputLines = new List<string>();
      public int SuccessExitCode = 0;

      public void AddCriticalOutString(string aFailureString)
      {
        CriticalOutputLines.Add(aFailureString);
      }
    }

    public static bool PlayDVD()
    {
      using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
      {
        string strPath = xmlreader.GetValueAsString("dvdplayer", "path", "");
        string strParams = xmlreader.GetValueAsString("dvdplayer", "arguments", "");
        bool bInternal = xmlreader.GetValueAsBool("dvdplayer", "internal", true);
        if (bInternal) return false;

        if (strPath != "")
        {
          if (File.Exists(strPath))
          {
            Process dvdplayer = new Process();

            string strWorkingDir = Path.GetFullPath(strPath);
            string strFileName = Path.GetFileName(strPath);
            strWorkingDir = strWorkingDir.Substring(0, strWorkingDir.Length - (strFileName.Length + 1));
            dvdplayer.StartInfo.FileName = strFileName;
            dvdplayer.StartInfo.WorkingDirectory = strWorkingDir;

            if (strParams.Length > 0)
            {
              dvdplayer.StartInfo.Arguments = strParams;
            }
            Log.Info("start process {0} {1}", strPath, dvdplayer.StartInfo.Arguments);

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
            Log.Info("{0} done", strPath);
          }
          else
          {
            Log.Info("file {0} does not exists", strPath);
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Use external player for movie playback (configured by the user)
    /// </summary>
    /// <param name="strFile">Full path to the video file</param>
    /// <returns>Whether the player was successfully used</returns>
    public static bool PlayMovie(string strFile)
    {
      if (String.IsNullOrEmpty(strFile)) return false;

      try
      {
        string extension = Path.GetExtension(strFile).ToLower();
        if (strFile.ToLower().IndexOf("live.ts") >= 0) return false;
        if (strFile.ToLower().IndexOf("live.tv") >= 0) return false;
        if (extension.Equals(".sbe")) return false;
        if (extension.Equals(".dvr-ms")) return false;
        if (extension.Equals(".radio")) return false;
        if (strFile.IndexOf("record0.") > 0 || strFile.IndexOf("record1.") > 0 ||
          strFile.IndexOf("record2.") > 0 || strFile.IndexOf("record3.") > 0 ||
          strFile.IndexOf("record4.") > 0 || strFile.IndexOf("record5.") > 0) return false;

        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
        {
          //using external player checking is now g_player side 
          //bool bInternal = xmlreader.GetValueAsBool("movieplayer", "internal", true);
          //if (bInternal) return false;
          string strPath = xmlreader.GetValueAsString("movieplayer", "path", "");
          string strParams = xmlreader.GetValueAsString("movieplayer", "arguments", "");
          if (extension.ToLower() == ".ifo" || extension.ToLower() == ".vob")
          {
            strPath = xmlreader.GetValueAsString("dvdplayer", "path", "");
            strParams = xmlreader.GetValueAsString("dvdplayer", "arguments", "");
          }
          if (strPath != "")
          {
            if (File.Exists(strPath))
            {
              if (strParams.IndexOf("%filename%") >= 0)
                strParams = strParams.Replace("%filename%", "\"" + strFile + "\"");

              Process movieplayer = new Process();
              string strWorkingDir = Path.GetFullPath(strPath);
              string strFileName = Path.GetFileName(strPath);
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
              Log.Info("start process {0} {1}", strPath, movieplayer.StartInfo.Arguments);
              if (OnStartExternal != null)
              {
                OnStartExternal(movieplayer, true);		// Event: Starting external process
              }
              AutoPlay.StopListening();
              movieplayer.Start();
              movieplayer.WaitForExit();
              AutoPlay.StartListening();
              if (OnStopExternal != null)
              {
                OnStopExternal(movieplayer, true);		// Event: External process stopped
              }
              Log.Debug("Util: External player stopped on {0}", strPath);
              return true;
            }
            else
            {
              Log.Warn("Util: External player {0} does not exists", strPath);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Warn("Util: Error using external player - {0}", ex.ToString());
      }
      return false;
    }

    public static DateTime longtodate(long ldate)
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

    public static long datetolong(DateTime dt)
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

    public static string MakeFileName(string strText)
    {
      if (strText == null) return string.Empty;
      if (strText.Length == 0) return string.Empty;

      string strFName = strText.Replace(':', '_');
      strFName = strFName.Replace('/', '_');
      strFName = strFName.Replace('\\', '_');
      strFName = strFName.Replace('*', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_');
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');

      bool unclean = true;
      char[] invalids = Path.GetInvalidFileNameChars();
      while (unclean)
      {
        unclean = false;

        char[] filechars = strFName.ToCharArray();

        foreach (char c in filechars)
        {
          if (!unclean)
            foreach (char i in invalids)
            {
              if (c == i)
              {
                unclean = true;
                //Log.Warn("Utils: *** File name {1} still contains invalid chars - {0}", Convert.ToString(c), strFName);
                strFName = strFName.Replace(c, '_');
                break;
              }
            }
        }

      }
      return strFName;
    }

    public static string MakeDirectoryPath(string strText)
    {
      if (strText == null) return string.Empty;
      if (strText.Length == 0) return string.Empty;

      string strFName = strText.Replace(':', '_');
      strFName = strFName.Replace('/', '_');
      strFName = strFName.Replace('*', '_');
      strFName = strFName.Replace('?', '_');
      strFName = strFName.Replace('\"', '_');
      strFName = strFName.Replace('<', '_');
      strFName = strFName.Replace('>', '_');
      strFName = strFName.Replace('|', '_');

      return strFName;
    }

    public static bool FileDelete(string strFile)
    {
      if (String.IsNullOrEmpty(strFile)) return true;
      try
      {
        if (!File.Exists(strFile))
          return true;
        File.Delete(strFile);
        return true;
      }
      catch (Exception) { }
      return false;
    }

    public static bool DirectoryDelete(string aDirectory)
    {
      return DirectoryDelete(aDirectory, false);
    }

    public static bool DirectoryDelete(string aDirectory, bool aRecursive)
    {
      if (String.IsNullOrEmpty(aDirectory)) return false;
      try
      {
        Directory.Delete(aDirectory, aRecursive);
        return true;
      }
      catch (Exception) { }
      return false;
    }

    public static void DownLoadImage(string strURL, string strFile, System.Drawing.Imaging.ImageFormat imageFormat)
    {
      if (string.IsNullOrEmpty(strURL) || string.IsNullOrEmpty(strFile))
        return;

      using (WebClient client = new WebClient())
      {
        try
        {
          string extensionURL = Path.GetExtension(strURL);
          string extensionFile = Path.GetExtension(strFile);
          if (extensionURL.Length > 0 && extensionFile.Length > 0)
          {
            extensionURL = extensionURL.ToLower();
            extensionFile = extensionFile.ToLower();
            string strLogo = Path.ChangeExtension(strFile, extensionURL);
            client.Proxy.Credentials = CredentialCache.DefaultCredentials;
            client.DownloadFile(strURL, strLogo);
            if (extensionURL != extensionFile)
            {
              using (Image imgSrc = Image.FromFile(strLogo))
              {
                imgSrc.Save(strFile, imageFormat);
              }
              Utils.FileDelete(strLogo);
            }
          }
        }
        catch (Exception ex)
        {
          Log.Info("Utils: DownLoadImage {1} failed: {0}", ex.Message, strURL);
        }
      }
    }

    public static void DownLoadAndCacheImage(string strURL, string strFile)
    {
      if (strURL == null) return;
      if (strURL.Length == 0) return;
      if (strFile == null) return;
      if (strFile.Length == 0) return;
      string url = String.Format("mpcache-{0}", EncryptLine(strURL));

      string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), url);
      if (file != string.Empty && File.Exists(file))
      {
        try
        {
          File.Copy(file, strFile, true);
        }
        catch (Exception)
        {
        }
        return;
      }
      DownLoadImage(strURL, file);
      if (File.Exists(file))
      {
        try
        {
          //file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), url);
          //Util.Picture.CreateThumbnail(file, strFile, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0);
          //string strFileL = ConvertToLargeCoverArt(strFile);
          //Util.Picture.CreateThumbnail(file, strFileL, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0);
          File.Copy(file, strFile, true);
        }
        catch (Exception ex)
        {
          Log.Warn("Util: error after downloading thumbnail {0} - {1}", strFile, ex.Message);
        }
      }

    }

    public static void DownLoadImage(string strURL, string strFile)
    {
      if (string.IsNullOrEmpty(strURL) || string.IsNullOrEmpty(strFile))
        return;

      try
      {
        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(strURL);
        wr.Timeout = 5000;
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // wr.Proxy = WebProxy.GetDefaultProxy();
          wr.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) { }
        HttpWebResponse ws = (HttpWebResponse)wr.GetResponse();

        using (Stream str = ws.GetResponseStream())
        {
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
          using (FileStream fstr = new FileStream(strFile, FileMode.OpenOrCreate, FileAccess.Write))
          {
            fstr.Write(inBuf, 0, bytesRead);
            str.Close();
            fstr.Close();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Info("Utils: DownLoadImage {1} failed:{0}", ex.Message, strURL);
      }
    }


    public static string RemoveTrailingSlash(string strLine)
    {
      if (strLine == null) return string.Empty;
      if (strLine.Length == 0) return string.Empty;
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

    public static string GetFilename(string strPath)
    {
      return GetFilename(strPath, false);
    }

    public static string GetFilename(string strPath, bool withoutExtension)
    {
      if (strPath == null) return string.Empty;
      if (strPath.Length == 0) return string.Empty;
      try
      {
        if (m_bHideExtensions || withoutExtension)
          return Path.GetFileNameWithoutExtension(strPath);
        else
          return Path.GetFileName(strPath);
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
      if (!File.Exists(sSoundFile))
      {
        string strSkin = GUIGraphicsContext.Skin;
        if (File.Exists(strSkin + "\\sounds\\" + sSoundFile))
        {
          sSoundFile = strSkin + "\\sounds\\" + sSoundFile;
        }
        else if (File.Exists(strSkin + "\\" + sSoundFile + ".wav"))
        {
          sSoundFile = strSkin + "\\" + sSoundFile + ".wav";
        }
        else
        {
          Log.Info(@"Cannot find sound:{0}\sounds\{1} ", strSkin, sSoundFile);
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
    public static int GetNextForwardSpeed(int iCurSpeed)
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

    public static int GetNextRewindSpeed(int iCurSpeed)
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

    public static string FilterFileName(string strName)
    {
      if (strName == null) return string.Empty;
      if (strName.Length == 0) return string.Empty;
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

    public static string EncryptLine(string strLine)
    {
      if (strLine == null) return string.Empty;
      if (strLine.Length == 0) return string.Empty;
      if (String.Compare(Strings.Unknown, strLine, true) == 0) return string.Empty;
      CRCTool crc = new CRCTool();
      crc.Init(CRCTool.CRCCode.CRC32);
      ulong dwcrc = crc.calc(strLine);
      string strRet = String.Format("{0}", dwcrc);
      return strRet;
    }

    public static string TryEverythingToGetFolderThumbByFilename(string aSongPath, bool aPreferOriginalShareFile)
    {
      string strThumb = string.Empty;

      strThumb = GetLocalFolderThumb(aSongPath);
      if (File.Exists(strThumb) && !aPreferOriginalShareFile)
      {
        return strThumb;
      }
      else
      {
        // nothing locally - try the share itself
        string strRemoteFolderThumb = string.Empty;
        strRemoteFolderThumb = GetFolderThumb(aSongPath);

        if (File.Exists(strRemoteFolderThumb))
          return strRemoteFolderThumb;
        else
        {
          // last chance - maybe some other program left useable images
          string searchPath = Path.GetDirectoryName(aSongPath);
          if (!Directory.Exists(searchPath))
          {
            return string.Empty;
          }
          string[] imageFiles = Directory.GetFiles(searchPath, @"*.png", SearchOption.TopDirectoryOnly);
          if (imageFiles.Length < 1) // WMP leaves files like AlbumArt_{18289833-9C5D-4D3F-9971-F3F9EBDC03E7}_Large.jpg
            imageFiles = Directory.GetFiles(searchPath, @"*Large.jpg", SearchOption.TopDirectoryOnly);
          if (imageFiles.Length < 1)
            imageFiles = Directory.GetFiles(searchPath, @"*.jpg", SearchOption.TopDirectoryOnly);

          long maxSize = 0;
          for (int i = 0; i < imageFiles.Length; i++)
          {
            try
            {
              // get the largest pic available
              FileInfo fi = new FileInfo(imageFiles[i]);
              if (fi.Length > maxSize)
              {
                strRemoteFolderThumb = imageFiles[i];
                maxSize = fi.Length;
              }
              // prefer the front cover over booklets or cd prints
              if (imageFiles[i].ToLowerInvariant().Contains(@"front") || imageFiles[i].ToLowerInvariant().Contains(@"vorne"))
              {
                strRemoteFolderThumb = imageFiles[i];
                break;
              }
            }
            catch (Exception)
            {
            }
          }
          if (File.Exists(strRemoteFolderThumb))
            return strRemoteFolderThumb;
          // we came through here without finding anything so fallback to the cache finally
          if (aPreferOriginalShareFile && File.Exists(strThumb))
            return strThumb;
        }
      }
      return string.Empty;
    }

    public static string GetAlbumThumbName(string ArtistName, string AlbumName)
    {
      if (string.IsNullOrEmpty(ArtistName) || string.IsNullOrEmpty(AlbumName))
        return string.Empty;

      RemoveStackEndings(ref AlbumName);

      string name = string.Format("{0}-{1}", ArtistName, AlbumName);
      return GetCoverArtName(Thumbs.MusicAlbum, name);
    }

    public static string GetFolderThumb(string strFile)
    {
      if (string.IsNullOrEmpty(strFile))
        return string.Empty;

      string strPath, strFileName;
      Utils.Split(strFile, out strPath, out strFileName);
      string strFolderJpg = String.Format(@"{0}\folder.jpg", strPath);

      return strFolderJpg;
    }

    public static string GetFolderThumbForDir(string strPath)
    {
      if (string.IsNullOrEmpty(strPath))
        return string.Empty;

      string strFolderJpg = String.Format(@"{0}\folder.jpg", strPath);

      return strFolderJpg;
    }

    public static string GetLocalFolderThumb(string strFile)
    {
      if (string.IsNullOrEmpty(strFile))
        return string.Empty;

      string strPath, strFileName;
      Utils.Split(strFile, out strPath, out strFileName);
      string strFolderJpg = String.Format(@"{0}\{1}{2}", Thumbs.MusicFolder, EncryptLine(strPath), GetThumbExtension());

      return strFolderJpg;
    }

    public static string GetLocalFolderThumbForDir(string strDirPath)
    {
      if (string.IsNullOrEmpty(strDirPath))
        return string.Empty;

      string strFolderJpg = String.Format(@"{0}\{1}{2}", Thumbs.MusicFolder, EncryptLine(strDirPath), GetThumbExtension());

      return strFolderJpg;
    }

    public static string GetCoverArt(string strFolder, string strFileName)
    {
      if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
        return string.Empty;

      string strThumb = String.Format(@"{0}\{1}", strFolder, Utils.MakeFileName(strFileName));

      if (File.Exists(strThumb + ".png"))
        return strThumb + ".png";
      else if (File.Exists(strThumb + ".jpg"))
        return strThumb + ".jpg";
      else if (File.Exists(strThumb + ".gif"))
        return strThumb + ".gif";
      else if (File.Exists(strThumb + ".tbn"))
        return strThumb + ".tbn";
      return string.Empty;
    }

    public static string ConvertToLargeCoverArt(string smallArt)
    {
      if (smallArt == null) return string.Empty;
      if (smallArt.Length == 0) return string.Empty;
      if (smallArt == string.Empty) return smallArt;

      string smallExt = GetThumbExtension();
      string LargeExt = String.Format(@"L{0}", GetThumbExtension());

      return smallArt.Replace(smallExt, LargeExt);
    }

    public static string GetCoverArtName(string strFolder, string strFileName)
    {
      if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
        return string.Empty;

      return string.Format(@"{0}\{1}{2}", strFolder, Utils.MakeFileName(strFileName), GetThumbExtension());
    }

    public static string GetLargeCoverArtName(string strFolder, string strFileName)
    {
      if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
        return string.Empty;

      return Utils.GetCoverArtName(strFolder, strFileName + "L");
    }

    private static void AddPicture(Graphics g, string strFileName, int x, int y, int w, int h)
    {
      Image img = null;
      try
      {
        // Add a thumbnail of the specified picture file to the image referenced by g, draw it at the given location and size.
        //try
        //{
        //  img = ImageFast.FromFile(strFileName);
        //  using (FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.ReadWrite))
        //  {
        //    img = Image.FromStream(fs, true, true);
        //  }
        //}
        //catch (ArgumentException)
        //{
        try
        {
          img = Image.FromFile(strFileName);

          if (img != null)
            g.DrawImage(img, x, y, w, h);
        }
        catch (OutOfMemoryException)
        {
          Log.Warn("Utils: Damaged picture file found: {0}. Try to repair or delete this file please!", strFileName);
        }
        catch (Exception ex)
        {
          Log.Info("Utils: An exception occured adding an image to the folder preview thumb: {0}", ex.Message);
        }
        //}
      }
      finally
      {
        if (img != null)
          img.Dispose();
        if (MediaPortal.Player.g_Player.Playing)
          Thread.Sleep(50);
        else
          Thread.Sleep(10);
      }
    }

    public static bool CreateFolderPreviewThumb(List<string> aPictureList, string aThumbPath)
    {
      bool result = false;
      Stopwatch benchClock = new Stopwatch();
      benchClock.Start();

      if (aPictureList.Count > 0)
      {
        try
        {
          string currentSkin = GUIGraphicsContext.Skin;

          // when launched by configuration exe this might be the case
          if (string.IsNullOrEmpty(currentSkin))
          {
            using (Profile.Settings xmlreader = new Profile.MPSettings())
            {
              currentSkin = Config.Dir.Config + @"\skin\" + xmlreader.GetValueAsString("skin", "name", "Blue3");
            }
          }

          string defaultBackground = currentSkin + @"\media\previewbackground.png";

          if (File.Exists(defaultBackground))
          {
            using (FileStream fs = new FileStream(defaultBackground, FileMode.Open, FileAccess.ReadWrite))
            {
              using (Image imgFolder = Image.FromStream(fs, true, false))
              {
                int width = imgFolder.Width;
                int height = imgFolder.Height;

                int thumbnailWidth = 256;
                int thumbnailHeight = 256;
                // draw a fullsize thumb if only 1 pic is available
                if (aPictureList.Count == 1)
                {
                  thumbnailWidth = (width - 20);
                  thumbnailHeight = (height - 20);
                }
                else
                {
                  thumbnailWidth = (width - 30) / 2;
                  thumbnailHeight = (height - 30) / 2;
                }

                using (Bitmap bmp = new Bitmap(width, height))
                {
                  using (Graphics g = Graphics.FromImage(bmp))
                  {
                    g.CompositingQuality = Thumbs.Compositing;
                    g.InterpolationMode = Thumbs.Interpolation;
                    g.SmoothingMode = Thumbs.Smoothing;

                    g.DrawImage(imgFolder, 0, 0, width, height);
                    int x, y, w, h;
                    x = 0;
                    y = 0;
                    w = thumbnailWidth;
                    h = thumbnailHeight;
                    //Load first of 4 images for the folder thumb.                  
                    try
                    {
                      AddPicture(g, (string)aPictureList[0], x + 10, y + 10, w, h);

                      //If exists load second of 4 images for the folder thumb.
                      if (aPictureList.Count > 1)
                      {
                        AddPicture(g, (string)aPictureList[1], x + thumbnailWidth + 20, y + 10, w, h);
                      }

                      //If exists load third of 4 images for the folder thumb.
                      if (aPictureList.Count > 2)
                      {
                        AddPicture(g, (string)aPictureList[2], x + 10, y + thumbnailHeight + 20, w, h);
                      }

                      //If exists load fourth of 4 images for the folder thumb.
                      if (aPictureList.Count > 3)
                      {
                        AddPicture(g, (string)aPictureList[3], x + thumbnailWidth + 20, y + thumbnailHeight + 20, w, h);
                      }
                    }
                    catch (Exception ex)
                    {
                      Log.Error("Utils: An exception occured creating folder preview thumb: {0}", ex.Message);
                    }
                  }//using (Graphics g = Graphics.FromImage(bmp) )

                  try
                  {
                    if (File.Exists(aThumbPath))
                      FileDelete(aThumbPath);

                    string tmpFile = Path.Combine(Path.GetTempPath(), "folderpreview.jpg");
                    if (File.Exists(tmpFile))
                      FileDelete(tmpFile);

                    bmp.Save(tmpFile, Thumbs.ThumbCodecInfo, Thumbs.ThumbEncoderParams);

                    // we do not want a folderL.jpg
                    if (aThumbPath.ToLowerInvariant().Contains(@"folder.jpg"))
                    {
                      Picture.CreateThumbnail(tmpFile, aThumbPath, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false);
                    }
                    else
                      if (Picture.CreateThumbnail(tmpFile, aThumbPath, (int)Thumbs.ThumbResolution, (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
                      {
                        aThumbPath = Util.Utils.ConvertToLargeCoverArt(aThumbPath);
                        Picture.CreateThumbnail(tmpFile, aThumbPath, (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, 0, false);
                      }

                    if (MediaPortal.Player.g_Player.Playing)
                      Thread.Sleep(100);
                    else
                      Thread.Sleep(10);

                    if (File.Exists(aThumbPath))
                      result = true;
                  }
                  catch (Exception ex2)
                  {
                    Log.Error("Utils: An exception occured saving folder preview thumb: {0} - {1}", aThumbPath, ex2.Message);
                  }
                }//using (Bitmap bmp = new Bitmap(210,210))
              }
            }
          }
          else
            Log.Warn("Utils: Your skin does not supply previewbackground.png to create folder preview thumbs!");

        }
        catch (Exception exm)
        {
          Log.Error("Utils: An error occured creating folder preview thumbs: {0}", exm.Message);
        }

        benchClock.Stop();
        Log.Debug("Utils: CreateFolderPreviewThumb for {0} took {1} ms", aThumbPath, benchClock.ElapsedMilliseconds);
      }  //if (pictureList.Count>0)
      else
        result = false;

      return result;
    }

    public static string GetThumbExtension()
    {
      if (Thumbs.ThumbFormat == ImageFormat.Jpeg)
        return ".jpg";
      else if (Thumbs.ThumbFormat == ImageFormat.Png)
        return ".png";
      else if (Thumbs.ThumbFormat == ImageFormat.Gif)
        return ".gif";
      else if (Thumbs.ThumbFormat == ImageFormat.Icon)
        return ".ico";
      else if (Thumbs.ThumbFormat == ImageFormat.Bmp)
        return ".bmp";

      return ".jpg";
    }

    /// <summary>
    /// Move the Prefix of an artist to the end of the string for better sorting
    /// i.e. "The Rolling Stones" -> "Rolling Stones, The" 
    /// </summary>
    /// <param name="artistName"></param>
    /// <param name="appendPrefix"></param>
    /// <returns></returns>
    public static bool StripArtistNamePrefix(ref string artistName, bool appendPrefix)
    {
      string temp = artistName.ToLower();

      foreach (string s in _artistNamePrefixes)
      {
        if (s.Length == 0)
          continue;

        string prefix = s;
        prefix = prefix.Trim().ToLower();
        int pos = temp.IndexOf(prefix + " ");
        if (pos == 0)
        {
          string tempName = artistName.Substring(prefix.Length).Trim();

          if (appendPrefix)
            artistName = string.Format("{0}, {1}", tempName, artistName.Substring(0, prefix.Length));
          else
            artistName = tempName;

          return true;
        }
      }

      return false;
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
        if (!Directory.Exists(strDir))
          return;
        strFiles = Directory.GetFiles(strDir, strPattern);
        foreach (string strFile in strFiles)
        {
          try
          {
            File.Delete(strFile);
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }

    public static bool UsingTvServer
    {
      get
      {
        try
        {
          return File.Exists(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll");
        }
        catch (Exception)
        {
          return false;
        }
      }
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
        Log.Info("Util.ExportEmbeddedResource: {0}", e.Message);
      }
    }

    public static string ReplaceTag(string line, string tag, string value, string empty)
    {
      if (line == null) return string.Empty;
      if (line.Length == 0) return string.Empty;
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
      string driveName = string.Empty;

      if (drive.StartsWith(@"\\"))
        // We've got unc notation
        driveName = drive;
      else
        // We've got a drove letter only
        driveName = drive[0] + @":\";

      GetDiskFreeSpaceEx(
         driveName,
         out freeBytesAvailable,
         out totalNumberOfBytes,
         out totalNumberOfFreeBytes);
      return freeBytesAvailable;
    }

    public static void DeleteOldTimeShiftFiles(string path)
    {
      if (path == null) return;
      if (path == string.Empty) return;
      // Remove any trailing slashes
      path = Utils.RemoveTrailingSlash(path);


      // clean the TempDVR\ folder
      string directory = string.Empty;
      string[] files;
      try
      {
        directory = String.Format(@"{0}\TempDVR", path);
        if (Directory.Exists(directory))
        {
          files = Directory.GetFiles(directory, "*.tmp");
          foreach (string fileName in files)
          {
            try
            {
              File.Delete(fileName);
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
        if (Directory.Exists(directory))
        {
          files = Directory.GetFiles(directory, "*.tmp");
          foreach (string fileName in files)
          {
            try
            {
              File.Delete(fileName);
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
        if (Directory.Exists(directory))
        {
          files = Directory.GetFiles(directory, "*.tv");
          foreach (string fileName in files)
          {
            try
            {
              File.Delete(fileName);
            }
            catch (Exception) { }
          }
        }
      }
      catch (Exception) { }
    }//void DeleteOldTimeShiftFiles(string path)

    public static void DeleteRecording(string recordingFilename)
    {
      Utils.FileDelete(recordingFilename);

      int pos = recordingFilename.LastIndexOf(@"\");
      if (pos < 0)
        return;
      string path = Path.GetDirectoryName(recordingFilename);
      string filename = Path.GetFileNameWithoutExtension(recordingFilename);

      filename = filename.ToLower();
      string[] files;
      try
      {
        files = Directory.GetFiles(path);
        foreach (string fileName in files)
        {
          try
          {
            if (fileName.ToLower().IndexOf(filename) >= 0)
            {
              //delete all Timeshift buffer files
              if (fileName.ToLower().IndexOf(".sbe") >= 0)
              {
                File.Delete(fileName);
              }
              //delete Thumbnails
              if (fileName.ToLower().IndexOf(".jpg") >= 0)
              {
                File.Delete(fileName);
              }
              //delete comskip txt file
              if (fileName.ToLower().IndexOf(".txt") >= 0)
              {
                File.Delete(fileName);
              }
              //delete Matroska tag file
              if (fileName.ToLower().IndexOf(".xml") >= 0)
              {
                File.Delete(fileName);
              }
            }
          }
          catch (Exception) { }
        }
      }
      catch (Exception) { }
    }

    /// <summary>
    /// Please use WindowsController.ExitWindows
    /// </summary>
    /// <param name="forceShutDown"></param>
    /// <returns></returns>
    public static void HibernateSystem(bool forceShutDown)
    {
      Log.Info("Utils: Hibernate system");
      WindowsController.ExitWindows(RestartOptions.Hibernate, forceShutDown);
    }

    /// <summary>
    /// Please use WindowsController.ExitWindows
    /// </summary>
    /// <param name="forceShutDown"></param>
    /// <returns></returns>
    public static void SuspendSystem(bool forceShutDown)
    {
      Log.Info("Utils: Suspend system");
      WindowsController.ExitWindows(RestartOptions.Suspend, forceShutDown);
    }

    public static string EncryptPin(string code)
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

    public static string DecryptPin(string code)
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

    public static string GetAspectRatioLocalizedString(MediaPortal.GUI.Library.Geometry.Type aspectRatioType)
    {
      switch (aspectRatioType)
      {
        case MediaPortal.GUI.Library.Geometry.Type.Stretch:
          return GUILocalizeStrings.Get(942);

        case MediaPortal.GUI.Library.Geometry.Type.Normal:
          return GUILocalizeStrings.Get(943);

        case MediaPortal.GUI.Library.Geometry.Type.Original:
          return GUILocalizeStrings.Get(944);

        case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
          return GUILocalizeStrings.Get(945);

        case MediaPortal.GUI.Library.Geometry.Type.NonLinearStretch:
          return GUILocalizeStrings.Get(946);

        case MediaPortal.GUI.Library.Geometry.Type.Zoom:
          return GUILocalizeStrings.Get(947);

        case MediaPortal.GUI.Library.Geometry.Type.Zoom14to9:
          return GUILocalizeStrings.Get(1190);

        default:
          return GUILocalizeStrings.Get(943);
      }
    }

    public static string GetAspectRatio(MediaPortal.GUI.Library.Geometry.Type aspectRatioType)
    {
      switch (aspectRatioType)
      {
        case MediaPortal.GUI.Library.Geometry.Type.Stretch:
          return "Stretch";

        case MediaPortal.GUI.Library.Geometry.Type.Normal:
          return "Normal";

        case MediaPortal.GUI.Library.Geometry.Type.Original:
          return "Original";

        case MediaPortal.GUI.Library.Geometry.Type.LetterBox43:
          return "Letterbox 4:3";

        case MediaPortal.GUI.Library.Geometry.Type.NonLinearStretch:
          return "Non-linear Stretch";

        case MediaPortal.GUI.Library.Geometry.Type.Zoom:
          return "Zoom";

        case MediaPortal.GUI.Library.Geometry.Type.Zoom14to9:
          return "Zoom 14:9";

        default:
          return "Normal";
      }
    }

    public static MediaPortal.GUI.Library.Geometry.Type GetAspectRatio(string aspectRatioText)
    {
      switch (aspectRatioText)
      {
        case "Stretch":
          return MediaPortal.GUI.Library.Geometry.Type.Stretch;

        case "Normal":
          return MediaPortal.GUI.Library.Geometry.Type.Normal;

        case "Original":
          return MediaPortal.GUI.Library.Geometry.Type.Original;

        case "Letterbox 4:3":
          return MediaPortal.GUI.Library.Geometry.Type.LetterBox43;

        case "Non-linear Stretch":
          return MediaPortal.GUI.Library.Geometry.Type.NonLinearStretch;

        case "Zoom":
          return MediaPortal.GUI.Library.Geometry.Type.Zoom;

        case "Zoom 14:9":
          return MediaPortal.GUI.Library.Geometry.Type.Zoom14to9;

        default:
          return MediaPortal.GUI.Library.Geometry.Type.Normal;
      }
    }

    public static MediaPortal.GUI.Library.Geometry.Type GetAspectRatioByLangID(int languageID)
    {
      switch (languageID)
      {
        case 942:
          return MediaPortal.GUI.Library.Geometry.Type.Stretch;

        case 943:
          return MediaPortal.GUI.Library.Geometry.Type.Normal;

        case 944:
          return MediaPortal.GUI.Library.Geometry.Type.Original;

        case 945:
          return MediaPortal.GUI.Library.Geometry.Type.LetterBox43;

        case 946:
          return MediaPortal.GUI.Library.Geometry.Type.NonLinearStretch;

        case 947:
          return MediaPortal.GUI.Library.Geometry.Type.Zoom;

        case 1190:
          return MediaPortal.GUI.Library.Geometry.Type.Zoom14to9;

        default:
          return MediaPortal.GUI.Library.Geometry.Type.Normal;
      }
    }

    public static string TranslateLanguageString(string language)
    {
      switch (language.ToLower())
      {
        case "undetermined":
          return GUILocalizeStrings.Get(2599);
        case "english":
          return GUILocalizeStrings.Get(2600);
        case "german":
          return GUILocalizeStrings.Get(2601);
        case "french":
          return GUILocalizeStrings.Get(2602);
        case "dutch":
          return GUILocalizeStrings.Get(2603);
        case "norwegian":
          return GUILocalizeStrings.Get(2604);
        case "italian":
          return GUILocalizeStrings.Get(2605);
        case "swedish":
          return GUILocalizeStrings.Get(2606);
        case "spanish":
          return GUILocalizeStrings.Get(2607);
        case "portuguese":
          return GUILocalizeStrings.Get(2608);
        case "danish":
          return GUILocalizeStrings.Get(2609);
        case "polish":
          return GUILocalizeStrings.Get(2610);
        case "czech":
          return GUILocalizeStrings.Get(2611);
        case "hungarian":
          return GUILocalizeStrings.Get(2612);
        default:
          return language;
      }
    }


    public static string GetCultureRegionLanguage()
    {
      string strLongLanguage = CultureInfo.CurrentCulture.EnglishName;
      int iTrimIndex = strLongLanguage.IndexOf(" ", 0, strLongLanguage.Length);
      string strShortLanguage = strLongLanguage.Substring(0, iTrimIndex);

      foreach (CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
      {
        if (cultureInformation.EnglishName.ToLower().IndexOf(strShortLanguage.ToLower()) != -1)
        {
          return cultureInformation.EnglishName;
        }
      }
      return "English";
    }

    public static void PopulateLanguagesToComboBox(ComboBox comboBox, string defaultLanguage)
    {
      comboBox.Items.Clear();

      foreach (CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
      {
        comboBox.Items.Add(cultureInformation.EnglishName);
        if (String.Compare(cultureInformation.TwoLetterISOLanguageName, defaultLanguage, true) == 0)
        {
          comboBox.Text = cultureInformation.EnglishName;
        }
      }
    }
  }
}
