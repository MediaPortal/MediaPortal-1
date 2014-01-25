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
using System.Drawing.Drawing2D;
using System.Linq;
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
using System.Xml;
using MediaPortal.ExtensionMethods;
using MediaPortal.Player;
using MediaPortal.Profile;
using Microsoft.Win32;
using MediaPortal.GUI.Library;
using MediaPortal.Ripper;
using MediaPortal.Configuration;
using MediaPortal.Services;

namespace MediaPortal.Util
{
  public struct LanguageInfo
  {
    public string EnglishName;
    public string TwoLetterISO;
  } ;


  /// <summary>
  /// Common functions for general usage
  /// </summary>
  public class Utils
  {
    [DllImport("User32")]
    public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

    [DllImport("User32")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("User32")]
    public static extern int EnumWindows(IECallBack x, int y);

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

    [DllImport("mpr.dll")]
    private static extern int WNetAddConnection2A(ref NetResource pstNetRes, string psPassword, string psUsername,
                                                  int piFlags);


    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;

    private const int CONNECT_UPDATE_PROFILE = 0x00000001;
    private const int RESOURCETYPE_DISK = 0x1;

    private const int FileLookUpCacheThreadScanningIntervalMSecs = 5000;

    private static CRCTool crc = null;

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

    public delegate bool IECallBack(int hwnd, int lParam);

    public delegate void UtilEventHandler(Process proc, bool waitForExit);

    public static event UtilEventHandler OnStartExternal = null; // Event: Start external process / waeberd & mPod
    public static event UtilEventHandler OnStopExternal = null; // Event: Stop external process	/ waeberd & mPod
    private static HashSet<string> m_AudioExtensions = new HashSet<string>();
    private static HashSet<string> m_VideoExtensions = new HashSet<string>();
    private static HashSet<string> m_PictureExtensions = new HashSet<string>();
    private static HashSet<string> m_ImageExtensions = new HashSet<string>();

    private static string[] _artistNamePrefixes;
    protected static string _artistPrefixes;
    
    private static bool m_bHideExtensions = false;
    private static bool enableGuiSounds;

    private static char[] crypt = new char[10] {'G', 'D', 'J', 'S', 'I', 'B', 'T', 'P', 'W', 'Q'};


    // singleton. Dont allow any instance of this class
    private Utils() {}

    public static string AudioExtensionsDefault =
        ".asx,.dts," +
        // Playlists
        ".m3u,.pls,.b4s,.wpl,.cue," +
        // Bass Standard
        ".mod,.mo3,.s3m,.xm,.it,.mtm,.umx,.mdz,.s3z,.itz,.xmz," +
        ".mp3,.ogg,.wav,.mp2,.mp1,.aiff,.m2a,.mpa,.m1a,.swa,.aif,.mp3pro," +
        // BassCD
        ".cda," +
        // BassAac
        ".aac,.mp4,.m4a,.m4b,.m4p," +
        // BassAc3
        ".ac3," +
        // BassAlac
        //".m4a,.aac,.mp4," +
        // BassApe
        ".ape,.apl," +
        // BassFlac
        ".flac," +
        // BassMidi
        ".midi,.mid,.rmi,.kar," +
        // BassMpc
        ".mpc,.mpp,.mp+," +
        // BassOfr
        ".ofr,.ofs," +
        // BassOpus
        ".opus," +
        // BassSpx
        ".spx," +
        // BassTta
        ".tta," +
        // BassWma
        // .wmv,
        ".wma," +
        // BassWv
        ".wv";
    
    public static string VideoExtensionsDefault =
      ".avi,.bdmv,.mpg,.mpeg,.mp4,.divx,.ogm,.mkv,.wmv,.qt,.rm,.mov,.mts,.m2ts,.sbe,.dvr-ms,.ts,.dat,.ifo,.flv,.m4v,.3gp,.wtv,.ogv";

    public static string PictureExtensionsDefault = ".jpg,.jpeg,.gif,.bmp,.png";
    public static string ImageExtensionsDefault = ".cue,.bin,.iso,.ccd,.bwt,.mds,.cdi,.nrg,.pdi,.b5t,.img";

    static Utils()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        m_bHideExtensions = xmlreader.GetValueAsBool("gui", "hideextensions", true);
        string artistNamePrefixes = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");
        _artistNamePrefixes = artistNamePrefixes.Split(',');
        _artistPrefixes = xmlreader.GetValueAsString("musicfiles", "artistprefixes", "The, Les, Die");

        string strTmp = xmlreader.GetValueAsString("music", "extensions", AudioExtensionsDefault);
        Tokens tok = new Tokens(strTmp, new[] {','});
        foreach (string extension in tok)
        {
          m_AudioExtensions.Add(extension.ToLowerInvariant().Trim());
        }

        strTmp = xmlreader.GetValueAsString("movies", "extensions", VideoExtensionsDefault);
        tok = new Tokens(strTmp, new[] {','});
        foreach (string extension in tok)
        {
          m_VideoExtensions.Add(extension.ToLowerInvariant().Trim());
        }

        strTmp = xmlreader.GetValueAsString("pictures", "extensions", PictureExtensionsDefault);
        tok = new Tokens(strTmp, new[] {','});
        foreach (string extension in tok)
        {
          m_PictureExtensions.Add(extension.ToLowerInvariant().Trim());
        }

        if (xmlreader.GetValueAsBool("daemon", "enabled", false))
        {
          strTmp = xmlreader.GetValueAsString("daemon", "extensions", ImageExtensionsDefault);
          tok = new Tokens(strTmp, new[] {','});
          foreach (string extension in tok)
          {
            m_ImageExtensions.Add(extension.ToLowerInvariant().Trim());
          }
        }

        enableGuiSounds = xmlreader.GetValueAsBool("gui", "enableguisounds", true);
      }
    }

    public static ArrayList VideoExtensions
    {
      get
      {
        ArrayList t = new ArrayList();
        t.AddRange(m_VideoExtensions.ToArray());
        //
        // Added images extensions in order to trigger autoplay as video
        // no more need to add them manually to Videos extensions (mantis #2749)
        //
        if (m_ImageExtensions.Count != 0)
        {
          t.AddRange(m_ImageExtensions.ToArray());
        }
        return t;
      }
    }

    public static ArrayList AudioExtensions
    {
      get { return new ArrayList(m_AudioExtensions.ToArray()); }
    }

    public static ArrayList PictureExtensions
    {
      get { return new ArrayList(m_PictureExtensions.ToArray()); }
    }

    public static ArrayList ImageExtensions
    {
      get { return new ArrayList(m_ImageExtensions.ToArray()); }
    }

    public static string GetDriveSerial(string drive)
    {
      if (drive == null) return string.Empty;
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
      if (drive == null) return string.Empty;
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
      catch (Exception) {}
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

    private static char[] sizes = new char[] {'K', 'M', 'G', 'T'};
    // converts bytes to bytes / 1024^n
    private static Func<long, int, double> sizeConverter = (size, expo) => size / Math.Pow(1024, expo);

    public static string GetSize(long dwFileSize)
    {
      int i = sizes.Length;
      double beautySize = 0.0D;

      // get the highest power of 1024 that yields a value above 1
      while ((beautySize = sizeConverter(dwFileSize, i)) < 1 && i > 1)
        i--;

      // we force close enough values to the next higher, so we get 0.99 GB instead of 1010 MB for instance
      if (beautySize >= 1000 && i < sizes.Length)
        beautySize = sizeConverter(dwFileSize, ++i);

      return string.Format("{0:f} {1}B", beautySize, sizes[i - 1]);
    }

    /// <summary>
    /// Returns whether a file is TV
    /// Will also return true is file is Radio as both share same format
    /// </summary>
    /// <param name="strPath">path to file</param>
    /// <returns>Whether file is TV</returns>
    public static bool IsLiveTv(string strPath)
    {
      if (strPath == null) return false;

      Match ex = Regex.Match(strPath, @"(live\d+-\d+\.ts(\.tsbuffer(\d+\.ts)?)?)$");
      return ex.Success;
    }

    public static bool IsRTSP(string strPath)
    {
      if (strPath == null) return false;

      return strPath.Contains("rtsp:");
    }

    /// <summary>
    /// Should return whether a file is radio but is bugged
    /// because file format for radio is the same as TV
    /// </summary>
    /// <param name="strPath">path to file</param>
    /// <returns>Whether file is radio</returns>
    public static bool IsLiveRadio(string strPath)
    {
      if (strPath == null) return false;
      //
      // Bugged implementation: files are named "live3-0.ts.tsbuffer" as for LiveTv
      //
      return strPath.Contains("live-radio.ts");
    }

    /// <summary>
    /// This returns whether a file is video or not
    /// There is an issue in the logic for multi-seat radio 
    /// => if (strPath.ToLowerInvariant().StartsWith("rtsp:")) return true;
    /// means this will incorrectly return true for multi-seat radio
    /// </summary>
    /// <param name="strPath">path to file</param>
    /// <returns>Whether file is a video file</returns>
    public static bool IsVideo(string strPath)
    {
      if (strPath == null) return false;
      if (IsLastFMStream(strPath)) return false;
      if (strPath.ToLowerInvariant().StartsWith("rtsp:")) return true;
      if (strPath.ToLowerInvariant().StartsWith("mms:")
          && strPath.ToLowerInvariant().EndsWith(".ymvp")) return true;
      try
      {
        if (!Path.HasExtension(strPath))
          return false;
        string extensionFile = Path.GetExtension(strPath).ToLowerInvariant();
        if (IsPlayListExtension(extensionFile))
          return false;

        if (extensionFile == ".ts")
        {
          // Forced check to avoid users messed configuration ( .ts remove from Videos extensions list)
          return true;
        }
        if (extensionFile == ".bdmv")
        {
          // Forced check to avoid users messed configuration ( .bdmv remove from Videos extensions list)
          return true;
        }
        if (VirtualDirectory.IsImageFile(extensionFile.ToLowerInvariant()))
          return true;
        return m_VideoExtensions.Contains(extensionFile, StringComparer.InvariantCultureIgnoreCase);
      }
      catch (Exception) {}
      return false;
    }

    public static bool IsLastFMStream(string aPath)
    {
      try
      {
        if (aPath.StartsWith(@"http://play.last.fm"))
        {
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
      if (strPath.StartsWith("http:")) return true;
      if (strPath.StartsWith("https:")) return true;
      if (strPath.StartsWith("mms:")) return true;
      return false;
    }

    public static bool IsRemoteUrl(string strPath)
    {
      Uri playbackUri = null;
      if (Uri.TryCreate(strPath, UriKind.Absolute, out playbackUri) && playbackUri.Scheme != "file")
      {
        return true;
      }
      return false;
    }

    public static bool IsAudio(string strPath)
    {
      if (strPath == null) return false;
      if (IsLastFMStream(strPath)) return true;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLowerInvariant();
        if (IsPlayListExtension(extensionFile)) return false;
        return m_AudioExtensions.Contains(extensionFile);
      }
      catch (Exception) {}
      return false;
    }

    public static bool IsPicture(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        if (IsPlayList(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLowerInvariant();
        return m_PictureExtensions.Contains(extensionFile);
      }
      catch (Exception) {}
      return false;
    }

    private static bool IsPlayListExtension(string extensionFile)
    {
      if (extensionFile == ".m3u") return true;
      if (extensionFile == ".pls") return true;
      if (extensionFile == ".b4s") return true;
      if (extensionFile == ".wpl") return true;
      return false;
    }

    public static bool IsPlayList(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLowerInvariant();
        return IsPlayListExtension(extensionFile);
      }
      catch (Exception) {}
      return false;
    }

    public static bool IsProgram(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLowerInvariant();
        if (extensionFile == ".exe") return true;
      }
      catch (Exception) {}
      return false;
    }

    public static bool IsShortcut(string strPath)
    {
      if (strPath == null) return false;
      try
      {
        if (!Path.HasExtension(strPath)) return false;
        string extensionFile = Path.GetExtension(strPath).ToLowerInvariant();
        if (extensionFile == ".lnk") return true;
      }
      catch (Exception) {}
      return false;
    }

    public static void SetDefaultIcons(GUIListItem item)
    {
      if (item == null)
      {
        return;
      }
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
          else if (item.Path.Length <= 3)
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
      if (item == null || String.IsNullOrEmpty(item.Path))
      {
        //Disable verbose logging
        //Log.Debug("SetThumbnails: nothing to do.");
        return;
      }

      string strThumb = string.Empty;
      string strThumbFolder = string.Empty;

      if (!item.IsFolder || (item.IsFolder && VirtualDirectory.IsImageFile(Path.GetExtension(item.Path).ToLowerInvariant())))
      {
        if (!IsVideo(item.Path))
        {
          //Disable verbose logging
          //Log.Debug("SetThumbnails: nothing to do.");
          return;
        }

        string[] thumbs = {
                            Util.Utils.GetVideosThumbPathname(item.Path),
                            Path.ChangeExtension(item.Path, ".jpg"),
                            Path.ChangeExtension(item.Path, ".tbn"),
                            Path.ChangeExtension(item.Path, ".png")
                          };

        bool foundVideoThumb = false;
        foreach (string s in thumbs)
        {
          string thumbStr = s;
          if (FileExistsInCache(thumbStr))
          {
            strThumb = thumbStr;
            foundVideoThumb = true;
            break;
          }
        }
        //
        bool createVideoThumbs;
        bool getItemThumb = true;
          
        using (Settings xmlreader = new MPSettings())
        {
          createVideoThumbs = xmlreader.GetValueAsBool("thumbnails", "videoondemand", true);
          //
          //Get movies shares and check for video thumb create
          //
          const int maximumShares = 128;
          for (int index = 0; index < maximumShares; index++)
          {
            // Get share dir
            string sharePath = String.Format("sharepath{0}", index);
            string shareDir = xmlreader.GetValueAsString("movies", sharePath, "");
            // Get item dir
            string itemDir = string.Empty;
            if (!item.IsRemote)
            {
              itemDir = (GetParentDirectory(item.Path));
            }
            // Check if share dir correspond to item dir
            if (AreEqual(shareDir, itemDir))
            {
              string thumbsCreate = String.Format("videothumbscreate{0}", index);
              getItemThumb = xmlreader.GetValueAsBool("movies", thumbsCreate, true);
              break;
            }
          }
        }

        if (createVideoThumbs && !foundVideoThumb && getItemThumb)
        {
          if (Path.IsPathRooted(item.Path) && IsVideo(item.Path) &&
              !VirtualDirectory.IsImageFile(Path.GetExtension(item.Path).ToLowerInvariant()))
          {
            Log.Debug("SetThumbnails: Thumbs for video (" + GetFilename(item.Path) +
                      ") not found. Creating a new video thumb...");
            // creating and starting a thread for potentially every file in a list is very expensive, we should use the threadpool
            //Thread extractVideoThumbThread = new Thread(GetVideoThumb)
            //{
            //  Name = "ExtractVideoThumb",
            //  IsBackground = true,
            //  Priority = ThreadPriority.Lowest
            //};
            //extractVideoThumbThread.Start(item);
            ThreadPool.QueueUserWorkItem(GetVideoThumb, item);
          }
          return;
        }
        if (item.ThumbnailImage == string.Empty && FileExistsInCache(strThumb))
        {
          item.ThumbnailImage = strThumb;
          item.IconImage = strThumb;
          item.IconImageBig = strThumb;
        }
        else
        {
          if (FileExistsInCache(strThumb))
          {
            strThumb = item.ThumbnailImage;
          }
        }
      }
      else
      {
        if (item.Label != "..")
        {
          strThumb = item.Path + @"\folder.jpg";
          strThumbFolder = strThumb;
          if (FileExistsInCache(strThumb))
          {
            item.ThumbnailImage = strThumb;
            item.IconImage = strThumb;
            item.IconImageBig = strThumb;
          }
        }
      }
      if (!string.IsNullOrEmpty(strThumb) && !strThumb.Equals(strThumbFolder))
      {
        strThumb = ConvertToLargeCoverArt(strThumb);
        if (FileExistsInCache(strThumb) && strThumb != item.ThumbnailImage)
        {
          item.ThumbnailImage = strThumb;
          item.IconImageBig = strThumb;
        }
      }
    }

    /// <summary>
    /// Function to check share path and selected item path.
    /// Item path can have deeper subdir level but must begin
    /// with share path to return TRUE, selected item extra 
    /// subdir levels will be ignored
    /// </summary>
    /// <param name="dir1">share path</param>
    /// <param name="dir2">selected item path</param>
    /// <returns>true: paths are equal, false: paths do not match</returns>
    public static bool AreEqual(string dir1, string dir2)
    {
      if (dir1 == string.Empty | dir2 == string.Empty)
        return false;

      try
      {
        DirectoryInfo parent1 = new DirectoryInfo(dir1);
        DirectoryInfo parent2 = new DirectoryInfo(dir2);

        // Build a list of parents
        List<string> folder1Parents = new List<string>();
        List<string> folder2Parents = new List<string>();

        while (parent1 != null)
        {
          folder1Parents.Add(parent1.Name);
          parent1 = parent1.Parent;
        }

        while (parent2 != null)
        {
          folder2Parents.Add(parent2.Name);
          parent2 = parent2.Parent;
        }
        // Share path can't be deeper than item path
        if (folder1Parents.Count > folder2Parents.Count)
        {
          return false;
        }
        // Remove extra subdirs from item path
        if (folder2Parents.Count > folder1Parents.Count)
        {
          int diff = folder2Parents.Count - folder1Parents.Count;
          for (int i = 0; i < diff; i++)
          {
            folder2Parents.RemoveAt(0);
          }
        }

        bool equal = true;
        // Final check
        for (int i = 0; i < folder1Parents.Count; i++)
        {
          if (folder1Parents[i] != folder2Parents[i])
          {
            equal = false;
            break;
          }
        }
        return equal;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public static bool IsFolderDedicatedMovieFolder(string directory)
    {
      using (MPSettings xmlreader = new MPSettings())
      {
        const int maximumShares = 128;

        for (int index = 0; index < maximumShares; index++)
        {
          // Get share dir
          string sharePath = String.Format("sharepath{0}", index);
          string shareDir = xmlreader.GetValueAsString("movies", sharePath, "");
          // Get item dir
          string itemDir = string.Empty;
          itemDir = (GetParentDirectory(directory));
          
          // Check if share dir correspond to item dir
          if (AreEqual(shareDir, itemDir))
          {
            string eachFolderIsMovie = String.Format("eachfolderismovie{0}", index);
            bool folderMovie = xmlreader.GetValueAsBool("movies", eachFolderIsMovie, false);
            return folderMovie;
          }
        }
      }
      return false;
    }

    public static void GetVideoThumb(object i)
    {
      Thread.CurrentThread.Name = "GetVideoThumb Thumbnail";
      GUIListItem item = (GUIListItem)i;
      string path = item.Path;
      string strThumb = Util.Utils.GetVideosThumbPathname(path);
      string strThumbLarge = Util.Utils.GetVideosThumbPathname(path);
      if (FileExistsInCache(strThumb))
      {
        return;
      }

      // Do not try to create thumbnails for DVDs/BDs
      if (path.Contains("VIDEO_TS\\VIDEO_TS.IFO") || path.Contains("BDMV\\index.bdmv"))
      {
        return;
      }

      IVideoThumbBlacklist blacklist = GlobalServiceProvider.Get<IVideoThumbBlacklist>();
      if (blacklist != null && blacklist.Contains(path))
      {
        Log.Debug("Skipped creating thumbnail for {0}, it has been blacklisted because last attempt failed", path);
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
          if (OSInfo.OSInfo.VistaOrLater())
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
          {
            if (Picture.CreateThumbnail(thumb, strThumb, (int) Thumbs.ThumbLargeResolution,
                                        (int) Thumbs.ThumbLargeResolution, 0, false))
            {
              if (Picture.CreateThumbnail(thumb, strThumbLarge, (int)Thumbs.ThumbLargeResolution,
                                        (int)Thumbs.ThumbLargeResolution, 0, false))
              {
                item.ThumbnailImage = strThumbLarge;
                item.IconImage = strThumb;
                SetThumbnails(ref item);
              }
            }
          }
        }
        else
        {
          SetThumbnails(ref item);
        }
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
          thumb.SafeDispose();
        if (!FileExistsInCache(strThumb) && blacklist != null)
        {
          blacklist.Add(path);
        }
      }
    }

    public static void CheckThumbExtractorVersion()
    {
      try
      {
        using (Profile.Settings xmlreader = new Profile.MPSettings())
        {
          if (!xmlreader.GetValueAsBool("thumbnails", "videoondemand", true))
            return;

          string lastVersion = xmlreader.GetValueAsString("thumbnails", "extractorversion", "");
          string newVersion = VideoThumbCreator.GetThumbExtractorVersion();
          if (newVersion != lastVersion)
          {
            IVideoThumbBlacklist blacklist = GlobalServiceProvider.Get<IVideoThumbBlacklist>();
            if (blacklist != null)
            {
              blacklist.Clear();
            }
            xmlreader.SetValue("thumbnails", "extractorversion", newVersion);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("CheckThumbExtractorVersion failed:");
        Log.Error(ex);
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
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6030),
                             aDateTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
      }
      else if (aDateTime.Date == now.Date.AddDays(1)) // Tomorrow
      {
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6031), aDateTime.ToString("t"),
                             CultureInfo.CurrentCulture.DateTimeFormat);
      }
      else if (aDateTime.Date.AddDays(1) == now.Date) // Yesterday
      {
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6040), aDateTime.ToString("t"),
                             CultureInfo.CurrentCulture.DateTimeFormat);
      }
      else if (aDateTime.Date.AddDays(2) == now.Date) // Two days ago
      {
        return String.Format("{0} {1}", GUILocalizeStrings.Get(6041), aDateTime.ToString("t"),
                             CultureInfo.CurrentCulture.DateTimeFormat);
      }
      return String.Format("{0} {1}",
                           Utils.GetShortDayString(aDateTime),
                           aDateTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
    }

    public static string GetNamedDateStartEnd(DateTime startDateTime, DateTime endDateTime)
    {
      return String.Format("{0}-{1}", GetNamedDate(startDateTime), endDateTime.ToString("t"),
                           CultureInfo.CurrentCulture.DateTimeFormat);
    }

    public static string GetShortDayString(DateTime dt)
    {
      try
      {
        string day;
        switch (dt.DayOfWeek)
        {
          case DayOfWeek.Monday:
            day = GUILocalizeStrings.Get(657);
            break;
          case DayOfWeek.Tuesday:
            day = GUILocalizeStrings.Get(658);
            break;
          case DayOfWeek.Wednesday:
            day = GUILocalizeStrings.Get(659);
            break;
          case DayOfWeek.Thursday:
            day = GUILocalizeStrings.Get(660);
            break;
          case DayOfWeek.Friday:
            day = GUILocalizeStrings.Get(661);
            break;
          case DayOfWeek.Saturday:
            day = GUILocalizeStrings.Get(662);
            break;
          default:
            day = GUILocalizeStrings.Get(663);
            break;
        }
        return String.Format("{0} {1}-{2}", day, dt.Day, dt.Month);
      }
      catch (Exception) {}
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
      catch (Exception) {}
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
      if (String.IsNullOrEmpty(strFile)) return false;
      if (strFile.StartsWith("cdda:")) return true;
      string extension = Path.GetExtension(strFile).ToLowerInvariant();
      if (extension.Equals(".cda")) return true;
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

    public static bool IsUsbHdd(string path)
    {
      if (path == null) return false;
      if (path.Length < 2) return false;
      List<string> usbHdds = new List<string>();
      usbHdds = GetAvailableUsbHardDisks();
      string strDrive = path.Substring(0, 2);
      if (usbHdds.Contains(strDrive)) return true;
      return false;
    }

    public static bool IsRemovableUsbDisk(string path)
    {
      if (path == null) return false;
      if (path.Length < 2) return false;
      List<string> usbDisks = new List<string>();
      usbDisks = GetRemovableUsbDisks();
      string strDrive = path.Substring(0, 2);
      if (usbDisks.Contains(strDrive)) return true;
      return false;
    }

    // Check if filename is from mounted ISO image
    public static bool IsISOImage(string fileName)
    {
      string extension = Path.GetExtension(fileName).ToLowerInvariant();
      if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName) || (extension == ".tsbuffer" || extension == ".ts"))
        return false;

      string vDrive = DaemonTools.GetVirtualDrive();
      string bDrive = Path.GetPathRoot(fileName);

      if (vDrive == Util.Utils.RemoveTrailingSlash(bDrive))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Check if mounted image is BluRay. Use after mounting image!!!
    /// Returns changed filename as index.bdmv with full path if ISO is BluRay.
    /// </summary>
    /// <param name="bdIsoFilename"></param>
    /// <param name="fileName"></param>
    /// <returns>true/false and full index.bdmv path as filename</returns>
    public static bool IsBDImage(string bdIsoFilename, ref string fileName)
    {
      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(bdIsoFilename)))
      {
        string drive = DaemonTools.GetVirtualDrive();
        string driverLetter = drive.Substring(0, 1);
        string bdFilename = String.Format(@"{0}:\BDMV\index.bdmv", driverLetter);

        if (File.Exists(bdFilename))
        {
          fileName = bdFilename;
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Check if mounted image is BluRay. Use after mounting image!!!
    /// </summary>
    /// <param name="bdIsoFilename"></param>
    /// <returns></returns>
    public static bool IsBDImage(string bdIsoFilename)
    {
      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(bdIsoFilename)))
      {
        string drive = DaemonTools.GetVirtualDrive();
        string driverLetter = drive.Substring(0, 1);
        string fileName = String.Format(@"{0}:\BDMV\index.bdmv", driverLetter);

        if (File.Exists(fileName))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Check if mounted image is DVD. Use after mounting image!!!
    /// </summary>
    /// <param name="dvdIsoFilename"></param>
    /// <returns>true/false</returns>
    public static bool IsDVDImage(string dvdIsoFilename)
    {
      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(dvdIsoFilename)))
      {
        string drive = DaemonTools.GetVirtualDrive();
        string driverLetter = drive.Substring(0, 1);
        string fileName = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter);

        if (File.Exists(fileName))
        {
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Check if mounted image is DVD and returns full path video_ts.ifo as filename. Use after mounting image!!!
    /// </summary>
    /// <param name="dvdIsoFilename"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static bool IsDVDImage(string dvdIsoFilename, ref string fileName)
    {
      if (VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(dvdIsoFilename)))
      {
        string drive = DaemonTools.GetVirtualDrive();
        string driverLetter = drive.Substring(0, 1);
        string dvdFileName = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", driverLetter);

        if (File.Exists(dvdFileName))
        {
          fileName = dvdFileName;
          return true;
        }
      }
      return false;
    }

    public static string GetObjectCountLabel(int iTotalItems)
    {
      return iTotalItems.ToString();

      // Old code with embedded labels, see mantis #2833
      //return iTotalItems == 1
      //              ? String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(1062))
      //              : String.Format("{0} {1}", iTotalItems, GUILocalizeStrings.Get(632));
    }

    public static string GetSongCountLabel(int iTotalItems, int iTotalSeconds)
    {
      return String.Format("{0}, {1}", iTotalItems, SecondsToHMSString(iTotalSeconds));

      // Old code with embedded labels, see mantis #2833
      //return iTotalItems == 1
      //               ? String.Format("{0} {1}, {2}", iTotalItems, GUILocalizeStrings.Get(179), SecondsToHMSString(iTotalSeconds))
      //               : String.Format("{0} {1}, {2}", iTotalItems, GUILocalizeStrings.Get(1052), SecondsToHMSString(iTotalSeconds));
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
        var stackReg = StackExpression();

        // Strip the extensions and make everything lowercase
        string strFileName1 = Path.GetFileNameWithoutExtension(strFile1).ToLowerInvariant();
        string strFileName2 = Path.GetFileNameWithoutExtension(strFile2).ToLowerInvariant();

        // Check all the patterns
        for (int i = 0; i < stackReg.Length; i++)
        {
          // See if we can find the special patterns in both filenames
          //if (Regex.IsMatch(strFileName1, pattern[i], RegexOptions.IgnoreCase) &&
          //    Regex.IsMatch(strFileName2, pattern[i], RegexOptions.IgnoreCase))
          if (stackReg[i].IsMatch(strFileName1) && stackReg[i].IsMatch(strFileName2))
          {
            // Both strings had the special pattern. Now see if the filenames are the same.
            // Do this by removing the special pattern and compare the remains.
            //if (Regex.Replace(strFileName1, pattern[i], "", RegexOptions.IgnoreCase)
            //    == Regex.Replace(strFileName2, pattern[i], "", RegexOptions.IgnoreCase))
            if (stackReg[i].Replace(strFileName1, "") == stackReg[i].Replace(strFileName2, ""))
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

    public static bool PathShouldStack(string strPath1, string strPath2)
    {
      if (strPath1 == null) return false;
      if (strPath2 == null) return false;
      try
      {
        var stackReg = StackExpression();

        // Check all the patterns
        for (int i = 0; i < stackReg.Length; i++)
        {
          // See if we can find the special patterns in both paths
          if (stackReg[i].IsMatch(strPath1) && stackReg[i].IsMatch(strPath2))
          {
            // Both strings had the special pattern. Now see if the paths are the same.
            // Do this by removing the special pattern and compare the remains.
            if (stackReg[i].Replace(strPath1, "") == stackReg[i].Replace(strPath2, ""))
            {
              // It was a match so stack it
              return true;
            }
          }
        }
      }
      catch (Exception) { }

      // No matches were found, so no stacking
      return false;
    }

    public static void RemoveStackEndings(ref string strFileName)
    {
      if (strFileName == null) return;
      var stackReg = StackExpression();
      for (int i = 0; i < stackReg.Length; i++)
      {
        // See if we can find the special patterns in both filenames
        //if (Regex.IsMatch(strFileName, pattern[i], RegexOptions.IgnoreCase))
        if (stackReg[i].IsMatch(strFileName))
        {
          strFileName = stackReg[i].Replace(strFileName, "");
          //Regex.Replace(strFileName, pattern[i], "", RegexOptions.IgnoreCase);
        }
      }
    }

    private static Regex[] StackRegExpressions = null;

    public static Regex[] StackExpression()
    {
      // Patterns that are used for matching
      // 1st pattern matches [x-y] for example [1-2] which is disc 1 of 2 total
      // 2nd pattern matches ?cd?## and ?disc?## for example -cd2 which is cd 2.
      //     ? is -_+ or space (second ? is optional), ## is 1 or 2 digits
      //
      // Chemelli: added "+" as separator to allow IMDB scripts usage of this function
      //
      if (StackRegExpressions != null) return StackRegExpressions;
      string[] pattern = {
                           "\\s*\\[(?<digit>[0-9]{1,2})-[0-9]{1,2}\\]",
                           "\\s*[-_+ ]\\({0,1}(cd|dis[ck]|part|dvd)[-_+ ]{0,1}(?<digit>[0-9]{1,2})\\){0,1}"
                         };

      StackRegExpressions = new Regex[]
                              {
                                new Regex(pattern[0], RegexOptions.Compiled | RegexOptions.IgnoreCase),
                                new Regex(pattern[1], RegexOptions.Compiled | RegexOptions.IgnoreCase)
                              };
      return StackRegExpressions;
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
      catch (Exception) {}
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
      catch (Exception) {}

      return result;
    }

    public static void EjectCDROM()
    {
      EjectCDROM(string.Empty);
    }
    
    public static void CloseCDROM(string driveLetter)
    {
      mciSendString(string.Format("set CDAudio!{0} door closed", driveLetter), null, 127, IntPtr.Zero);
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
        string ErrorString =
          String.Format(
            "Utils: Error starting process!\n  filename: {0}\n  arguments: {1}\n  WorkingDirectory: {2}\n  stack: {3} {4} {5}",
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

    public static Process StartProcess(string strProgram, string strParams, bool bWaitForExit, bool bHidden)
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

      if (bHidden)
      {
        // A hidden window can process messages from the system or from other windows, but it cannot process input from the user or display output.
        // Set to hidden to avoid losing focus.
        procInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        procInfo.CreateNoWindow = true;
        procInfo.FileName = strProgram;
        procInfo.UseShellExecute = false;
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
    public static bool StartProcess(string aAppName, string aArguments, string aWorkingDir, int aExpectedTimeoutMs,
                                    bool aLowerPriority, ProcessFailedConditions aFailConditions)
    {
      bool success = false;
      Process ExternalProc = new Process();
      ProcessStartInfo ProcOptions = new ProcessStartInfo(aAppName, aArguments);

      ProcOptions.UseShellExecute = false; // Important for WorkingDirectory behaviour
      ProcOptions.RedirectStandardError = true; // .NET bug? Some stdout reader abort to early without that!
      ProcOptions.RedirectStandardOutput = true; // The precious data we're after
      //ProcOptions.StandardOutputEncoding = Encoding.GetEncoding("ISO-8859-1"); // the output contains "Umlaute", etc.
      //ProcOptions.StandardErrorEncoding = Encoding.GetEncoding("ISO-8859-1");
      ProcOptions.WorkingDirectory = aWorkingDir; // set the dir because the binary might depend on cygwin.dll
      ProcOptions.CreateNoWindow = true; // Do not spawn a "Dos-Box"      
      ProcOptions.ErrorDialog = false; // Do not open an error box on failure        

      ExternalProc.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);
      ExternalProc.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataHandler);
      ExternalProc.EnableRaisingEvents = true; // We want to know when and why the process died        
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
              ExternalProc.PriorityClass = ProcessPriorityClass.BelowNormal;
              // Execute all processes in the background so movies, etc stay fluent
            }
            catch (Exception ex2)
            {
              Log.Warn("Util: Error setting process priority for {0}: {1}", aAppName, ex2.Message);
            }
          }
          // Read in asynchronous  mode to avoid deadlocks (if error stream is full)
          // http://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.redirectstandarderror.aspx
          ExternalProc.BeginErrorReadLine();
          ExternalProc.BeginOutputReadLine();

          // wait this many seconds until the process has to be finished
          ExternalProc.WaitForExit(aExpectedTimeoutMs);

          success = (ExternalProc.HasExited && ExternalProc.ExitCode == aFailConditions.SuccessExitCode);

          if (!success)
          {
            Log.Warn("Util: Error executing {0}: return code {1}", aAppName, ExternalProc.ExitCode);
          }

          ExternalProc.OutputDataReceived -= new DataReceivedEventHandler(OutputDataHandler);
          ExternalProc.ErrorDataReceived -= new DataReceivedEventHandler(ErrorDataHandler);
        }
        catch (Exception ex)
        {
          Log.Warn("Util: Error executing {0}: {1}", aAppName, ex.Message);
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

    /// <summary>
    /// Checks whether a process is currently running
    /// </summary>
    /// <param name="aShouldExit">Indicate that a windows application should be closed gracefully. If it does not respond in 10 seconds a kill is performed</param>
    /// <returns>If the given process is still present.</returns>
    public static bool CheckForRunningProcess(string aProcessName, bool aShouldExit)
    {
      bool mpRunning = false;
      string processName = aProcessName;
      foreach (Process process in Process.GetProcesses())
      {
        if (process.ProcessName.Equals(processName))
        {
          if (!aShouldExit)
          {
            mpRunning = true;
            break;
          }
          else
          {
            try
            {
              // Kill the MediaPortal process by finding window and sending ALT+F4 to it.
              IECallBack ewp = new IECallBack(EnumWindowCallBack);
              EnumWindows(ewp, 0);
              process.CloseMainWindow();
              // Wait for the process to die, we wait for a maximum of 10 seconds
              if (!process.WaitForExit(10000))
              {
                process.Kill();
              }
            }
            catch (Exception)
            {
              try
              {
                process.Kill();
              }
              catch (Exception) {}
            }

            mpRunning = CheckForRunningProcess(aProcessName, false);
            break;
          }
        }
      }
      return mpRunning;
    }

    private static bool EnumWindowCallBack(int hwnd, int lParam)
    {
      IntPtr windowHandle = (IntPtr)hwnd;
      StringBuilder sb = new StringBuilder(1024);
      GetWindowText((int)windowHandle, sb, sb.Capacity);
      string window = sb.ToString().ToLowerInvariant();
      if (window.IndexOf("mediaportal") >= 0 || window.IndexOf("media portal") >= 0)
      {
        ShowWindow(windowHandle, SW_SHOWNORMAL);
      }
      return true;
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
              OnStartExternal(dvdplayer, true); // Event: Starting external process
            }
            dvdplayer.Start();
            dvdplayer.WaitForExit();
            if (OnStopExternal != null)
            {
              OnStopExternal(dvdplayer, true); // Event: External process stopped
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
        string extension = Path.GetExtension(strFile).ToLowerInvariant();
        if (IsLiveTv(strFile)) return false;
        if (extension.Equals(".sbe")) return false;
        if (extension.Equals(".dvr-ms")) return false;
        if (extension.Equals(".radio")) return false;
        Match regMatch = Regex.Match(strFile, @"record[0-9]\.");
        if (regMatch.Success)
        {
          return false;
        }
        using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.MPSettings())
        {
          //using external player checking is now g_player side 
          //bool bInternal = xmlreader.GetValueAsBool("movieplayer", "internal", true);
          //if (bInternal) return false;
          string strPath = xmlreader.GetValueAsString("movieplayer", "path", "");
          string strParams = xmlreader.GetValueAsString("movieplayer", "arguments", "");
          if (extension.ToLowerInvariant() == ".ifo" || extension.ToLowerInvariant() == ".vob" || extension.ToLowerInvariant() == ".bdmv")
          {
            strPath = xmlreader.GetValueAsString("dvdplayer", "path", "");
            strParams = xmlreader.GetValueAsString("dvdplayer", "arguments", "");
          }
          if (strPath != "")
          {
            if (File.Exists(strPath))
            {
              // %root% argument handling (TMT can only play BD/DVD/VCD images using root directory)
              // other video files will go to the player with full path
              if (strParams.IndexOf("%root%") >= 0)
              {
                DirectoryInfo dirInfo = new DirectoryInfo(strFile);

                if (dirInfo.Parent != null)
                {
                  string dirLvl = dirInfo.Parent.ToString();

                  // BluRay, DVD, VCD, HDDVD
                  if (dirLvl.Equals("bdmv", StringComparison.OrdinalIgnoreCase) ||
                      dirLvl.Equals("video_ts", StringComparison.OrdinalIgnoreCase) ||
                      dirLvl.Equals("vcd", StringComparison.OrdinalIgnoreCase) ||
                      dirLvl.Equals("hddvd_ts", StringComparison.OrdinalIgnoreCase))
                  {
                    dirInfo = new DirectoryInfo(dirInfo.Parent.FullName);
                    if (dirInfo.Parent != null)
                      strFile = dirInfo.Parent.FullName;
                  }
                  strParams = strParams.Replace("%root%", "\"" + strFile + "\"");
                }
              }
              // %filename% argument handling
              else if (strParams.IndexOf("%filename%") >= 0)
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
                OnStartExternal(movieplayer, true); // Event: Starting external process
              }
              AutoPlay.StopListening();
              movieplayer.Start();
              movieplayer.WaitForExit();
              AutoPlay.StartListening();
              if (OnStopExternal != null)
              {
                OnStopExternal(movieplayer, true); // Event: External process stopped
              }
              Log.Debug("Util: External player stopped on {0}", strPath);
              if (IsISOImage(strFile))
              {
                if (!String.IsNullOrEmpty(DaemonTools.GetVirtualDrive()) &&
                    (g_Player.IsBDDirectory(DaemonTools.GetVirtualDrive()) ||
                    g_Player.IsDvdDirectory(DaemonTools.GetVirtualDrive())))
                {
                  DaemonTools.UnMount();
                }
              }
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
      if (String.IsNullOrEmpty(strFile)) return false;
      try
      {
        if (!File.Exists(strFile))
        {
          return false;
        }
        File.Delete(strFile);
        Log.Debug("Util: FileDelete {0} successful.", strFile);
        return true;
      }
      catch (Exception ex)
      {
        Log.Error("Util: FileDelete {0} error: {1}", strFile, ex.Message);
      }
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
      catch (Exception) {}
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
            extensionURL = extensionURL.ToLowerInvariant();
            extensionFile = extensionFile.ToLowerInvariant();
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
          Log.Error("Utils: DownLoadImage {1} failed: {0}", ex.Message, strURL);
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
          Log.Debug("Util DownLoadAndCacheImage: Copying previously cached image {0} to {1}", file, strFile);
        }
        catch (Exception ex)
        {
          Log.Error("Util DownLoadAndCacheImage: error copying cached image {0} to {1} - {2}", file, strFile, ex.Message);
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
          Log.Debug("Util DownLoadAndCacheImage: Copying downloaded image {0} to {1}", file, strFile);
        }
        catch (Exception ex)
        {
          Log.Error("Util DownLoadAndCacheImage: error copying downloaded image {0} to {1} - {2}", file, strFile, ex.Message);
        }
      }
    }

    /// <summary>
    /// This method will not check if downloaded image exists in cache.
    /// Existing cached image will be overwritten with new one.
    /// </summary>
    /// <param name="strURL"></param>
    /// <param name="strFile"></param>
    public static void DownLoadAndOverwriteCachedImage(string strURL, string strFile)
    {
      if (strURL == null) return;
      if (strURL.Length == 0) return;
      if (strFile == null) return;
      if (strFile.Length == 0) return;
      string url = String.Format("mpcache-{0}", EncryptLine(strURL));

      string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), url);
      FileDelete(file);
      DownLoadImage(strURL, file);
      
      if (File.Exists(file))
      {
        try
        {
          File.Copy(file, strFile, true);
        }
        catch (Exception ex)
        {
          Log.Error("Util DownLoadAndOverwriteCachedImage: error copying downloaded image {0} to {1} - {2}", file, strFile, ex.Message);
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
        wr.Timeout = 20000;
        try
        {
          // Use the current user in case an NTLM Proxy or similar is used.
          // wr.Proxy = WebProxy.GetDefaultProxy();
          wr.Proxy.Credentials = CredentialCache.DefaultCredentials;
        }
        catch (Exception) {}
        HttpWebResponse ws = (HttpWebResponse)wr.GetResponse();
        try
        {
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
        finally
        {
          if (ws != null)
          {
            ws.Close();
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
      catch (Exception) {}
      return strPath;
    }

    public static string GetFileNameWithExtension(string strPath)
    {
      if (string.IsNullOrEmpty(strPath)) return string.Empty;
      try
      {
        return Path.GetFileName(strPath);
      }
      catch { }
      return strPath;
    }

    public static string GetFileExtension(string strPath)
    {
      if (string.IsNullOrEmpty(strPath)) return string.Empty;
      try
      {
        return Path.GetExtension(strPath);
      }
      catch { }
      return string.Empty;
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
      if (!Util.Utils.FileExistsInCache(sSoundFile))
      {
        if (Util.Utils.FileExistsInCache(GUIGraphicsContext.GetThemedSkinFile("\\sounds\\" + sSoundFile)))
        {
          sSoundFile = GUIGraphicsContext.GetThemedSkinFile("\\sounds\\" + sSoundFile);
        }
        else if (Util.Utils.FileExistsInCache(GUIGraphicsContext.GetThemedSkinFile("\\" + sSoundFile + ".wav")))
        {
          sSoundFile = GUIGraphicsContext.GetThemedSkinFile("\\" + sSoundFile + ".wav");
        }
        else
        {
          Log.Info(@"Cannot find sound:{0} ", GUIGraphicsContext.GetThemedSkinFile("\\sounds\\" + sSoundFile));
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
        sndPlaySoundA(null, Snd_Options); // terminate a currently active sound output
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
      if (crc == null)
      {
        crc = new CRCTool();
        crc.Init(CRCTool.CRCCode.CRC32);
      }
      ulong dwcrc = crc.calc(strLine);
      return dwcrc.ToString();
      //string strRet = String.Format("{0}", dwcrc);
      //return strRet;
    }

    public static string TryEverythingToGetFolderThumbByFilename(string aSongPath, bool aPreferOriginalShareFile)
    {
      string strThumb = string.Empty;

      strThumb = GetLocalFolderThumb(aSongPath);
      if (FileExistsInCache(strThumb) && !aPreferOriginalShareFile)
      {
        return strThumb;
      }
      else
      {
        // nothing locally - try the share itself
        string strRemoteFolderThumb = string.Empty;
        strRemoteFolderThumb = GetFolderThumb(aSongPath);

        if (FileExistsInCache(strRemoteFolderThumb))
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
              if (imageFiles[i].ToLowerInvariant().Contains(@"front") ||
                  imageFiles[i].ToLowerInvariant().Contains(@"vorne"))
              {
                strRemoteFolderThumb = imageFiles[i];
                break;
              }
            }
            catch (Exception) {}
          }
          if (FileExistsInCache(strRemoteFolderThumb))
          {
            return strRemoteFolderThumb;
          }
          // we came through here without finding anything so fallback to the cache finally
          if (aPreferOriginalShareFile && FileExistsInCache(strThumb))
          {
            return strThumb;
          }
        }
      }
      return string.Empty;
    }

    public static string GetAlbumThumbName(string ArtistName, string AlbumName)
    {
      if (string.IsNullOrEmpty(ArtistName) || string.IsNullOrEmpty(AlbumName))
        return string.Empty;

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

      string strFolderJpg = String.Format(@"{0}\{1}{2}", Thumbs.MusicFolder, EncryptLine(strDirPath),
                                          GetThumbExtension());

      return strFolderJpg;
    }

    public static void GetLocalItemPictureFolderThumb(List<string> _thumbnailFolderItem)
    {
      string thumbPicturesPath = Path.GetFullPath(Config.GetFolder(Config.Dir.Thumbs)) + @"\Pictures";
      try
      {
        VirtualDirectory dir = new VirtualDirectory();
        dir.SetExtensions(Util.Utils.PictureExtensions);
        List<GUIListItem> items = dir.GetDirectoryUnProtectedExt(thumbPicturesPath, true, true);
        foreach (GUIListItem item in items)
        {
          if (!item.IsFolder)
          {
            _thumbnailFolderItem.Add(item.Path);
          }
        }
      }
      catch (Exception e)
      {
        Log.Info("Exception counting files:{0}", e);
        // Ignore
      }
    }

    public struct FileLookUpItem
    {
      private string _filename;
      private bool _exists;

      public string Filename
      {
        get { return _filename; }
        set { _filename = value; }
      }

      public bool Exists
      {
        get { return _exists; }
        set { _exists = value; }
      }
    }

    private static bool? _fileLookUpCacheEnabled = null;

    private static Dictionary<string, FileLookUpItem> _fileLookUpCache = new Dictionary<string, FileLookUpItem>();
    private static Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

    private static HashSet<string> _foldersLookedUp = new HashSet<string>();
    private static HashSet<string> _fileExistsCacheQueue = new HashSet<string>();

    private static Thread _fileSystemManagerThread = null;
    private static Thread _fileExistsCacheThread = null;

    private static DateTime _lastTimeFolderWasAdded = DateTime.MinValue;

    private static ManualResetEvent _fileExistsCacheThreadEvt = new ManualResetEvent(false);

    private static object _fileLookUpCacheLock = new object();
    private static object _foldersLookedUpLock = new object();
    private static object _watchersLock = new object();
    private static object _fileExistsCacheLock = new object();

    private static void UpdateLookUpCacheItem(FileLookUpItem fileLookUpItem, string key)
    {
      lock (_fileLookUpCacheLock)
      {
        //Log.Debug("UpdateLookUpCacheItem : {0}", key);
        _fileLookUpCache[key] = fileLookUpItem; // we never remove anything, so this is safe
      }
    }

    private static void UpdateFileNameForCache(ref string filename)
    {
      if (string.IsNullOrEmpty(filename)) return;
      filename = filename.ToLowerInvariant();
    }

    private static IEnumerable<string> DirSearch(string sDir)
    {
      var files = new List<string>();
      try
      {
        // Let network drives get looked up lazily
        // It makes for much better performance
        if (!IsNetwork(sDir))
        {
          files.AddRange(NativeFileSystemOperations.GetFiles(sDir));
        }
        AddFoldersLookedUp(sDir);
      }
      catch (Exception)
      {
        AddFoldersLookedUp(sDir); //lets tag the invalid folder as have looked-up
        //Log.Error("DirSearch failed in dir={0}, with err={1}", sDir, e.Message);        
        //ignore
      }
      return files;
    }

    private static string GetParentDirectory(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        return string.Empty;
      }
      return Path.GetDirectoryName(path[path.Length - 1] == '\\' ? path.Substring(0, path.Length - 1) : path);
    }

    private static bool HasFolderBeenScanned(string dir)
    {
      // eg. takes care of this sequence
      // 1:         \\thumbs\tv\recorded
      // 2:					\\thumbs\tv         

      //or 
      // eg. takes care of this sequence
      // 1:         \\thumbs\tv
      // 2:         \\thumbs\tv          
      string dirCopy = dir;

      HashSet<string> foldersLookedUpCopy = null;

      lock (_foldersLookedUpLock)
      {
        foldersLookedUpCopy = new HashSet<string>(_foldersLookedUp);
      }

      bool hasFolderBeenScanned = foldersLookedUpCopy.Any(s => s == dirCopy);
      return hasFolderBeenScanned;
    }

    /// <summary>
    /// Determines if a folder already has a watcher on it
    /// </summary>
    /// <param name="dir">directory to test for watcher</param>
    /// <returns></returns>
    private static bool IsFolderAlreadyWatched(string dir)
    {
      bool isFolderAlreadyWatched = false;

      HashSet<string> _watcherKeyCopy;

      _watcherKeyCopy = new HashSet<string>(_watchers.Keys);
      if (!_watcherKeyCopy.Contains(dir))
      {
        foreach (string path in _watcherKeyCopy)
        {
          if (dir.Length >= path.Length)
          {
            if (dir.StartsWith(path))
            {
              isFolderAlreadyWatched = true;
              break;
            }
          }
        }
      }
      else
      {
        isFolderAlreadyWatched = true;
      }
      return isFolderAlreadyWatched;
    }


    private static void AddFoldersLookedUp(string dir)
    {
      if (!string.IsNullOrEmpty(dir))
      {
        lock (_foldersLookedUpLock)
        {
          //Log.Debug("AddFoldersLookedUp {0}", dir);
          //make sure we don't add the same one again
          if (!_foldersLookedUp.Contains(dir))
          {
            _foldersLookedUp.Add(dir);
            _lastTimeFolderWasAdded = DateTime.Now;
          }
        }
      }
    }

    private static void fileSystemWatcher_Error(object sender, ErrorEventArgs e)
    {
      Exception watchException = e.GetException();
      FileSystemWatcher watcher = sender as FileSystemWatcher;

      if (watcher != null)
      {
        Log.Debug("fileSystemWatcher_Error path {0} exception={1}", watcher.Path, watchException);

        string path = watcher.Path;
        if (watchException is InternalBufferOverflowException)
        {
          // buffer overflow ex.
          // remove watcher, and re-add with 4k bigger buffer.
          int buffersize = watcher.InternalBufferSize;
          RemoveWatcher(path);
          AddWatcher(path, (buffersize + 4096));
        }
        else
        {
          RemoveWatcher(path);
          RemoveFoldersFromCache(path);
        }
      }
    }

    private static void fileSystemWatcher_Created(object sender, FileSystemEventArgs e)
    {
      FileSystemWatcher watcher = sender as FileSystemWatcher;
      if (!e.FullPath.ToLowerInvariant().Contains("db3-journal"))
      {
        if (watcher != null)
        {
          Log.Debug("fileSystemWatcher_Created file {0}", e.FullPath);
          DoInsertExistingFileIntoCache(e.FullPath);
        }
      }
    }

    private static void fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
      FileSystemWatcher watcher = sender as FileSystemWatcher;

      if (!e.FullPath.ToLowerInvariant().Contains("db3-journal"))
      {
        if (watcher != null)
        {
          Log.Debug("fileSystemWatcher_Deleted file {0}", e.FullPath);
          DoInsertNonExistingFileIntoCache(e.FullPath);
        }
      }
    }

    private static string GetDirectoryName(string f)
    {
      try
      {
        int posOfDirSep = f.LastIndexOf(Path.DirectorySeparatorChar);
        if (posOfDirSep >= 0)
          return f.Substring(0, posOfDirSep);
        else return string.Empty;
      }
      catch (Exception)
      {
        return string.Empty;
      }
    }

    public static bool FileExistsInCache(string filename)
    {
      bool found = false;

      if (IsFileExistsCacheEnabled())
      {
        SetupFileSystemManagerThread();
        SetupFileExistsCacheThread();
        try
        {
          string path = GetDirectoryName(filename);
          if (path.Length > 0)
          {
            path = path.ToLowerInvariant();
            if (!HasFolderBeenScanned(path))
            {
              lock (_fileExistsCacheLock)
              {
                _fileExistsCacheQueue.Add(path);
                _fileExistsCacheThreadEvt.Set();
              }
            }
            /*else
            {
              Log.Debug("FileExistsInCache: already pre-scanned dir : {0} .. skipping", path);
            }*/
          }
        }
        catch (ArgumentException)
        {
          //ignore  
        }
        found = DoFileExistsInCache(filename);
      }
      else
      {
        found = File.Exists(filename);
      }
      return found;
    }

    public static bool IsFileExistsCacheEnabled()
    {
      if (!_fileLookUpCacheEnabled.HasValue)
      {
        using (Settings xmlreader = new MPSettings())
        {
          _fileLookUpCacheEnabled = xmlreader.GetValueAsBool("gui", "fileexistscache", false);
        }
      }
      return (_fileLookUpCacheEnabled.HasValue && _fileLookUpCacheEnabled.Value);
    }

    private static void FileExistsCacheThread()
    {
      while (true)
      {
        HashSet<string> fileExistsCacheQueueCopy = null;
        lock (_fileExistsCacheLock)
        {
          fileExistsCacheQueueCopy = new HashSet<string>(_fileExistsCacheQueue);
        }

        int items = fileExistsCacheQueueCopy.Count;
        if (items > 0)
        {
          Log.Debug("FileExistsCacheThread: new items found waiting for caching: {0}", items);

          foreach (string path in fileExistsCacheQueueCopy)
          {
            InsertFilesIntoCacheAsynch(path);
            lock (_fileExistsCacheLock)
            {
              _fileExistsCacheQueue.Remove(path);
            }
          }
        }

        bool isQueueEmpty = false;
        lock (_fileExistsCacheLock)
        {
          isQueueEmpty = (_fileExistsCacheQueue.Count == 0);
        }

        if (isQueueEmpty)
        {
          Log.Debug("FileExistsCacheThread: no more items to cache, suspending thread.: {0}", items);
          _fileExistsCacheThreadEvt.Reset();
        }

        _fileExistsCacheThreadEvt.WaitOne();
      }
    }

    private static void FileSystemWatchManagerThread()
    {
      while (true)
      {
        lock (_watchersLock)
        {
          if (_lastTimeFolderWasAdded != DateTime.MinValue)
          {
            DateTime now = DateTime.Now;
            TimeSpan ts = now - _lastTimeFolderWasAdded;

            //Log.Debug("_lastTimeFolderWasAdded: {0}", _lastTimeFolderWasAdded);
            //Log.Debug("ts.TotalSeconds: {0}", ts.TotalSeconds);

            if (ts.TotalSeconds > 5)
            {
              Log.Debug("FileSystemWatchManagerThread : updating watchers");
              HashSet<string> folders = GetUniqueTopLevelFolders();

              foreach (string dir in folders)
              {
                if (!string.IsNullOrEmpty(dir))
                {
                  if (!IsFolderAlreadyWatched(dir))
                  {
                    UpdateWatchers(dir);
                  }
                }
              }
              _lastTimeFolderWasAdded = DateTime.MinValue;
              Log.Debug("FileLookUpCacheThread items : {0}", _fileLookUpCache.Count);
            }


            /*string[] keyCopy = _watchers.Keys.ToArray();
            foreach (string key in keyCopy)
            {
              FileSystemWatcher fsw = null;
              if (_watchers.TryGetValue(key, out fsw))
              {
                Log.Debug("FileSystemWatcher : {0}", fsw.Path);
              }
            }*/
          }
        }
        Thread.Sleep(FileLookUpCacheThreadScanningIntervalMSecs);
      }
    }

    private static HashSet<string> GetUniqueTopLevelFolders()
    {
      HashSet<string> uniqueTopLevelFolders = new HashSet<string>();

      HashSet<string> foldersLookedUpCopy = null;
      lock (_foldersLookedUpLock)
      {
        foldersLookedUpCopy = new HashSet<string>(_foldersLookedUp);
      }

      foreach (string dir in foldersLookedUpCopy)
      {
        if (string.IsNullOrEmpty(dir))
        {
          continue;
        }
        if (Path.IsPathRooted(dir))
        {
          string parentDir2Use = dir;
          string parentDir = dir;
          int nrOfFoldersOnParentDirOld = 0;
          while (true)
          {
            parentDir = GetParentDirectory(parentDir);

            if (string.IsNullOrEmpty(parentDir))
            {
              break;
            }

            bool isPathRoot = IsPathRoot(parentDir);

            if (isPathRoot)
            {
              break;
            }

            if (!uniqueTopLevelFolders.Contains(parentDir))
            {
              int nrOfFoldersOnParentDir = foldersLookedUpCopy.Count(fli => fli.StartsWith(parentDir));

              if (nrOfFoldersOnParentDir == 1)
              {
                break;
              }

              if (nrOfFoldersOnParentDirOld == nrOfFoldersOnParentDir)
              {
                break;
              }
              if (nrOfFoldersOnParentDir > 0)
              {
                parentDir2Use = parentDir;
              }
              nrOfFoldersOnParentDirOld = nrOfFoldersOnParentDir;
            }
            else
            {
              parentDir2Use = null;
              break;
            }
          }

          if (parentDir2Use != null && !uniqueTopLevelFolders.Contains(parentDir2Use))
          {
            string folderCompare = parentDir2Use;
            bool addFolder = true;
            if (uniqueTopLevelFolders.Count > 0)
            {
              while (true)
              {
                int itemsCount =
                  uniqueTopLevelFolders.Count(fli => fli.StartsWith(folderCompare));
                if (itemsCount > 0)
                {
                  addFolder = false;
                  break;
                }

                folderCompare = GetParentDirectory(folderCompare);

                if (string.IsNullOrEmpty(folderCompare))
                {
                  break;
                }

                bool isPathRoot = IsPathRoot(folderCompare);
                if (isPathRoot)
                {
                  break;
                }
              }
            }
            if (addFolder)
            {
              uniqueTopLevelFolders.Add(parentDir2Use);
            }
          }
        }
      }

      return uniqueTopLevelFolders;
    }

    private static bool IsPathRoot(string dir)
    {
      bool isPathRoot = false;
      string root = Path.GetPathRoot(dir);
      if (!string.IsNullOrEmpty(root))
      {
        isPathRoot = (dir.Length == root.Length);
      }
      return isPathRoot;
    }

    private static void UpdateWatchers(string dir4Watcher)
    {
      int dir4WatcherLen = dir4Watcher.Length;

      string[] keyCopy = _watchers.Keys.ToArray();
      foreach (string key in keyCopy)
      {
        FileSystemWatcher fsw = null;
        if (_watchers.TryGetValue(key, out fsw))
        {
          string path = fsw.Path;
          int pathLen = path.Length;
          if ((pathLen > dir4WatcherLen) && path.StartsWith(dir4Watcher))
          {
            RemoveWatcher(path);
          }
        }
      }

      if (!_watchers.ContainsKey(dir4Watcher))
      {
        AddWatcher(dir4Watcher);
      }
    }

    /// <summary>
    /// taken from audioscrobbler plugin code to reverse where prefix has been swapped 
    /// eg. The Beatles => Beatles, The or Die Toten Hosen => Toten Hosen ,Die
    /// and will change back to the artist name
    /// </summary>
    /// <param name="aStrippedArtist">Value stored in database with prefix at the end</param>
    /// <returns>What should be actual string in tag</returns>
    public static string UndoArtistPrefix(string aStrippedArtist)
    {
      try
      {
        string[] allPrefixes = null;
        allPrefixes = _artistPrefixes.Split(',');
        if (allPrefixes.Length > 0)
        {
          for (int i = 0; i < allPrefixes.Length; i++)
          {
            string cpyPrefix = allPrefixes[i];
            if (!aStrippedArtist.ToLowerInvariant().EndsWith(cpyPrefix.ToLowerInvariant())) continue;
            // strip the separating "," as well
            int prefixPos = aStrippedArtist.IndexOf(',');
            if (prefixPos <= 0) continue;
            aStrippedArtist = aStrippedArtist.Remove(prefixPos);
            cpyPrefix = cpyPrefix.Trim(new char[] { ' ', ',' });
            aStrippedArtist = cpyPrefix + " " + aStrippedArtist;
            // abort here since artists should only have one prefix stripped
            return aStrippedArtist;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("An error occured undoing prefix strip for artist: {0} - {1}", aStrippedArtist,
                  ex.Message);
      }

      return aStrippedArtist;
    }

    private static void AddWatcher(string dir, int buffersize)
    {
      AddWatcher(dir);
      FileSystemWatcher fsw = null;

      lock (_watchersLock)
      {
        if (_watchers.TryGetValue(dir, out fsw))
        {
          fsw.InternalBufferSize = buffersize;
        }
      }
    }

    private static void AddWatcher(string dir)
    {
      bool bAlreadyWatched = false;
      lock (_watchersLock)
      {
        bAlreadyWatched = IsFolderAlreadyWatched(dir);
      }
      // if this folders parent is already watched then that watcher already covers it.
      if (!bAlreadyWatched)
      {
        if (!Directory.Exists(dir))
        {
          return;
        }
        try
        {
          FileSystemWatcher fsw = new FileSystemWatcher(dir);
          // if this is changed then IsFolderAlreadyWatched needs to be modified.
          fsw.IncludeSubdirectories = true;
          fsw.EnableRaisingEvents = true;
          fsw.Created += new FileSystemEventHandler(fileSystemWatcher_Created);
          fsw.Deleted += new FileSystemEventHandler(fileSystemWatcher_Deleted);
          fsw.Error += new ErrorEventHandler(fileSystemWatcher_Error);

          fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;

          lock (_watchersLock)
          {
            if (!_watchers.ContainsKey(dir))
            {
              _watchers.Add(dir, fsw);
            }
          }
          Log.Debug("AddWatcher {0}", dir);
        }
        catch (Exception ex)
        {
          Log.Error("AddWatcher exception on dir={0}, ex={1}", dir, ex);
        }
      }
    }

    private static void RemoveWatcher(string dir)
    {
      try
      {
        Log.Debug("RemoveWatcher {0}", dir);
        FileSystemWatcher fsw = null;
        if (_watchers.TryGetValue(dir, out fsw))
        {
          fsw.EnableRaisingEvents = false;
          fsw.Created -= new FileSystemEventHandler(fileSystemWatcher_Created);
          fsw.Deleted -= new FileSystemEventHandler(fileSystemWatcher_Deleted);
          fsw.Error -= new ErrorEventHandler(fileSystemWatcher_Error);
          fsw.SafeDispose();
          lock (_watchersLock)
          {
            _watchers.Remove(dir);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("RemoveWatcher exception on dir={0}, ex={1}", dir, ex);
      }
    }

    private static void RemoveFoldersFromCache(string dir)
    {
      Dictionary<string, FileLookUpItem> fileLookUpCacheCopy = null;
      lock (_fileLookUpCacheLock)
      {
        fileLookUpCacheCopy = new Dictionary<string, FileLookUpItem>(_fileLookUpCache);
      }

      IEnumerable<KeyValuePair<string, FileLookUpItem>> filesWithinDir =
        fileLookUpCacheCopy.Where(fli => fli.Value.Filename.StartsWith(dir));

      foreach (KeyValuePair<string, FileLookUpItem> fli in filesWithinDir)
      {
        lock (_fileLookUpCacheLock)
        {
          _fileLookUpCache.Remove(fli.Key);
        }
      }

      lock (_foldersLookedUpLock)
      {
        Log.Debug("RemoveWatcher removing folders from cache={0}", dir);
        _foldersLookedUp.RemoveWhere(s => s.StartsWith(dir));
      }
    }

    private static void SetupFileSystemManagerThread()
    {
      if (_fileSystemManagerThread == null)
      {
        _fileSystemManagerThread = new Thread(FileSystemWatchManagerThread);
        _fileSystemManagerThread.Name = "FileSystemManager Thread";
        _fileSystemManagerThread.IsBackground = true;
        _fileSystemManagerThread.Priority = ThreadPriority.Lowest;
        _fileSystemManagerThread.Start();
      }
    }

    private static void SetupFileExistsCacheThread()
    {
      if (_fileExistsCacheThread == null)
      {
        _fileExistsCacheThread = new Thread(FileExistsCacheThread);
        _fileExistsCacheThread.Name = "FileExistsCache Thread";
        _fileExistsCacheThread.IsBackground = true;
        _fileExistsCacheThread.Priority = ThreadPriority.Lowest;
        _fileExistsCacheThread.Start();
      }
    }

    private static void InsertFilesIntoCacheAsynch(object oPath)
    {
      string path = oPath as string;
      if (!String.IsNullOrEmpty(path))
      {
        string dir2Lower = path.ToLowerInvariant();
        if (Path.IsPathRooted(dir2Lower))
        {
          if (!HasFolderBeenScanned(dir2Lower))
          {
            Log.Debug("InsertFilesIntoCacheAsynch: pre-scanning dir : {0}", path);
            IEnumerable<string> files = DirSearch(dir2Lower);
            foreach (string file in files)
            {
              DoInsertExistingFileIntoCache(file);
            }
            lock (_watchersLock)
            {
              UpdateWatchers(dir2Lower);
            }
          }
        }
        /*else
        {
          Log.Debug("InsertFilesIntoCacheAsynch: dir already pre-scanned : {0}", path);      
        }*/
      }
    }

    public static void DoInsertExistingFileIntoCache(string file)
    {
      UpdateFileNameForCache(ref file);

      FileLookUpItem fileLookUpItem = new FileLookUpItem();
      fileLookUpItem.Exists = true;
      fileLookUpItem.Filename = file;
      UpdateLookUpCacheItem(fileLookUpItem, file);
    }

    public static void DoInsertNonExistingFileIntoCache(string file)
    {
      UpdateFileNameForCache(ref file);

      FileLookUpItem fileLookUpItem = new FileLookUpItem();
      fileLookUpItem.Exists = false;
      fileLookUpItem.Filename = file;
      UpdateLookUpCacheItem(fileLookUpItem, file);
    }

    private static bool DoFileExistsInCache(string filename)
    {
      bool found = false;

      if (!string.IsNullOrEmpty(filename))
      {
        UpdateFileNameForCache(ref filename);
        FileLookUpItem fileLookUpItem = new FileLookUpItem();
        if (!_fileLookUpCache.TryGetValue(filename, out fileLookUpItem))
        {
          found = File.Exists(filename);
          if (found)
            DoInsertExistingFileIntoCache(filename);
          else
            DoInsertNonExistingFileIntoCache(filename);
        }
        else
        {
          found = fileLookUpItem.Exists;
        }
      }
      return found;
    }


    public static string GetCoverArtByThumbExtension(string strFolder, string strFileName)
    {
      if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
      {
        return string.Empty;
      }

      string strThumb = String.Format(@"{0}\{1}", strFolder, Utils.MakeFileName(strFileName));
      bool found = false;

      string lookForThumb = strThumb + Utils.GetThumbExtension();
      found = FileExistsInCache(lookForThumb);

      if (!found)
      {
        lookForThumb = string.Empty;
      }

      return lookForThumb;
    }

    public static string GetCoverArt(string strFolder, string strFileName)
    {
      if (string.IsNullOrEmpty(strFolder) || string.IsNullOrEmpty(strFileName))
      {
        return string.Empty;
      }

      string strThumb = String.Format(@"{0}\{1}", strFolder, Utils.MakeFileName(strFileName));
      bool found = false;

      string lookForThumb = strThumb + ".png";
      found = FileExistsInCache(lookForThumb);

      if (!found)
      {
        lookForThumb = strThumb + ".jpg";
        found = FileExistsInCache(lookForThumb);
      }

      if (!found)
      {
        lookForThumb = strThumb + ".gif";
        found = FileExistsInCache(lookForThumb);
      }

      if (!found)
      {
        lookForThumb = strThumb + ".tbn";
        found = FileExistsInCache(lookForThumb);
      }

      if (!found)
      {
        lookForThumb = string.Empty;
      }

      return lookForThumb;
    }

    public static string ConvertToLargeCoverArt(string smallArt)
    {
      if (smallArt == null) return string.Empty;
      if (smallArt.Length == 0) return string.Empty;
      if (smallArt == string.Empty) return smallArt;

      string smallExt = GetThumbExtension();
      string LargeExt = String.Format(@"L{0}", GetThumbExtension());
      string largeCoverArt = smallArt.Replace(smallExt, LargeExt);
      return largeCoverArt;
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
          try
          {
            using (FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
            {
              using (img = Image.FromStream(fs, true, false))
              {
                int iRotation = Util.Picture.GetRotateByExif(img);
                switch (iRotation)
                {
                  case 1:
                    img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                  case 2:
                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                  case 3:
                    img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                  default:
                    break;
                }
                if (img != null)
                  g.DrawImage(img, x, y, w, h);
              }
            }
          }
          catch
            (OutOfMemoryException)
          {
            Log.Warn("Utils: Damaged picture file found: {0}. Try to repair or delete this file please!",
                     strFileName);
          }
        }
        catch (Exception ex)
        {
          Log.Info("Utils: An exception occured adding an image to the folder preview thumb: {0}", ex.Message);
        }
      }
      finally
      {
        if (img != null)
          img.SafeDispose();
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
          string defaultBackground;
          string currentSkin = GUIGraphicsContext.Skin;

          // when launched by configuration exe this might be the case
          if (string.IsNullOrEmpty(currentSkin))
          {
            using (Profile.Settings xmlreader = new Profile.MPSettings())
            {
              currentSkin = Config.Dir.Config + @"\skin\" + xmlreader.GetValueAsString("skin", "name", "Default");
            }
            defaultBackground = currentSkin + @"\media\previewbackground.png";
          }
          else
          {
            defaultBackground = GUIGraphicsContext.GetThemedSkinFile(@"\media\previewbackground.png");
          }


          using (FileStream fs = new FileStream(defaultBackground, FileMode.Open, FileAccess.Read))
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
                } //using (Graphics g = Graphics.FromImage(bmp) )

                try
                {
                  string tmpFile = Path.GetTempFileName();
                  Log.Debug("Saving thumb!");
                  bmp.Save(tmpFile, Thumbs.ThumbCodecInfo, Thumbs.ThumbEncoderParams);

                  // we do not want a folderL.jpg
                  if (aThumbPath.ToLowerInvariant().Contains(@"folder.jpg"))
                  {
                    Picture.CreateThumbnail(tmpFile, aThumbPath, (int)Thumbs.ThumbLargeResolution,
                                            (int)Thumbs.ThumbLargeResolution, 0, false);
                    FileDelete(tmpFile);
                  }
                  else if (Picture.CreateThumbnail(tmpFile, aThumbPath, (int)Thumbs.ThumbResolution,
                                                   (int)Thumbs.ThumbResolution, 0, Thumbs.SpeedThumbsSmall))
                  {
                    aThumbPath = Util.Utils.ConvertToLargeCoverArt(aThumbPath);
                    Picture.CreateThumbnail(tmpFile, aThumbPath, (int)Thumbs.ThumbLargeResolution,
                                            (int)Thumbs.ThumbLargeResolution, 0, false);
                    FileDelete(tmpFile);
                  }

                  if (MediaPortal.Player.g_Player.Playing)
                    Thread.Sleep(100);
                  else
                    Thread.Sleep(10);

                  if (FileExistsInCache(aThumbPath))
                    result = true;
                }
                catch (Exception ex2)
                {
                  Log.Error("Utils: An exception occured saving folder preview thumb: {0} - {1}", aThumbPath,
                            ex2.Message);
                }
              } //using (Bitmap bmp = new Bitmap(210,210))
            }
          }
        }
        catch (FileNotFoundException)
        {
          Log.Warn("Utils: Your skin does not supply previewbackground.png to create folder preview thumbs!");
        }
        catch (Exception exm)
        {
          Log.Error("Utils: An error occured creating folder preview thumbs: {0}", exm.Message);
        }

        benchClock.Stop();
        Log.Debug("Utils: CreateFolderPreviewThumb for {0} took {1} ms", aThumbPath, benchClock.ElapsedMilliseconds);
      } //if (pictureList.Count>0)
      else
      {
        result = false;
      }
      return result;
    }

    public static Image ResizeImage(Image image, Size size, bool preserveAspectRatio)
    {
      int newWidth;
      int newHeight;
      if (preserveAspectRatio)
      {
        int originalWidth = image.Width;
        int originalHeight = image.Height;
        float percentWidth = (float)size.Width / (float)originalWidth;
        float percentHeight = (float)size.Height / (float)originalHeight;
        float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
        newWidth = (int)(originalWidth * percent);
        newHeight = (int)(originalHeight * percent);
      }
      else
      {
        newWidth = size.Width;
        newHeight = size.Height;
      }
      Image newImage = new Bitmap(newWidth, newHeight);
      using (Graphics graphicsHandle = Graphics.FromImage(newImage))
      {
        graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
      }
      return newImage;
    }

    public static bool CreateTileThumb(List<string> aPictureList, string aThumbPath, int PreviewColumns, int PreviewRows)
    {
      bool result = false;

      if (aPictureList.Count > 0)
      {
        try
        {
          string defaultBackground;
          string currentSkin = GUIGraphicsContext.Skin;

          // when launched by configuration exe this might be the case
          if (string.IsNullOrEmpty(currentSkin))
          {
            using (Profile.Settings xmlreader = new Profile.MPSettings())
            {
              currentSkin = Config.Dir.Config + @"\skin\" + xmlreader.GetValueAsString("skin", "name", "Default");
            }
            defaultBackground = currentSkin + @"\media\previewbackground.png";
          }
          else
          {
            defaultBackground = GUIGraphicsContext.GetThemedSkinFile(@"\media\previewbackground.png");
          }

          // Resize defaultBackground to keep ratio based on aPictureList ratio
          Image img = new Bitmap(aPictureList[0]);
          int widthPicture = img.Width;
          int heightPicture = img.Height;
          Image defaultBackgroundOriginal = Image.FromFile(defaultBackground);
          Image defaultBackgroundResized = ResizeImage(defaultBackgroundOriginal, new Size(widthPicture, heightPicture), false);

          using (Image imgFolder = defaultBackgroundResized)
          {
            int width = imgFolder.Width;
            int height = imgFolder.Height;

            int thumbnailWidth = 256;
            int thumbnailHeight = 256;
            // draw a fullsize thumb if only 1 pic is available
            switch (PreviewColumns)
            {
              case 1:
                thumbnailWidth = width;
                break;
              case 2:
                thumbnailWidth = width/2;
                break;
              case 3:
                thumbnailWidth = width/3;
                break;
            }
            switch (PreviewRows)
            {
              case 1:
                thumbnailHeight = height;
                break;
              case 2:
                thumbnailHeight = height/2;
                break;
              case 3:
                thumbnailHeight = height/3;
                break;
            }

            using (Bitmap bmp = new Bitmap(width, height))
            {
              using (Graphics g = Graphics.FromImage(bmp))
              {
                g.CompositingQuality = Thumbs.Compositing;
                g.InterpolationMode = Thumbs.Interpolation;
                g.SmoothingMode = Thumbs.Smoothing;

                g.DrawImage(imgFolder, 0, 0, width, height);
                int w, h;
                w = thumbnailWidth;
                h = thumbnailHeight;

                try
                {
                  if (PreviewColumns == 1 && PreviewRows == 1)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                  }
                  if (PreviewColumns == 1 && PreviewRows == 2)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], 0, h, w, h);
                  }
                  if (PreviewColumns == 2 && PreviewRows == 1)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], w, 0, w, h);
                  }
                  if (PreviewColumns == 2 && PreviewRows == 2)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], w, 0, w, h);
                    AddPicture(g, (string) aPictureList[2], 0, h, w, h);
                    AddPicture(g, (string) aPictureList[3], w, h, w, h);
                  }
                  if (PreviewColumns == 1 && PreviewRows == 3)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], 0, h, w, h);
                    AddPicture(g, (string) aPictureList[2], 0, 2*h, w, h);
                  }
                  if (PreviewColumns == 2 && PreviewRows == 3)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], w, 0, w, h);
                    AddPicture(g, (string) aPictureList[2], 0, h, w, h);
                    AddPicture(g, (string) aPictureList[3], w, h, w, h);
                    AddPicture(g, (string) aPictureList[4], 0, 2*h, w, h);
                    AddPicture(g, (string) aPictureList[5], w, 2*h, w, h);
                  }
                  if (PreviewColumns == 3 && PreviewRows == 3)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], w, 0, w, h);
                    AddPicture(g, (string) aPictureList[2], 2*w, 0, w, h);
                    AddPicture(g, (string) aPictureList[3], 0, h, w, h);
                    AddPicture(g, (string) aPictureList[4], w, h, w, h);
                    AddPicture(g, (string) aPictureList[5], 2*w, h, w, h);
                    AddPicture(g, (string) aPictureList[6], 0, 2*h, w, h);
                    AddPicture(g, (string) aPictureList[7], w, 2*h, w, h);
                    AddPicture(g, (string) aPictureList[8], 2*w, 2*h, w, h);
                  }
                  if (PreviewColumns == 3 && PreviewRows == 1)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], w, 0, w, h);
                    AddPicture(g, (string) aPictureList[2], 2*w, 0, w, h);
                  }
                  if (PreviewColumns == 3 && PreviewRows == 2)
                  {
                    AddPicture(g, (string) aPictureList[0], 0, 0, w, h);
                    AddPicture(g, (string) aPictureList[1], w, 0, w, h);
                    AddPicture(g, (string) aPictureList[2], 2*w, 0, w, h);
                    AddPicture(g, (string) aPictureList[3], 0, h, w, h);
                    AddPicture(g, (string) aPictureList[4], w, h, w, h);
                    AddPicture(g, (string) aPictureList[5], 2*w, h, w, h);
                  }
                }
                catch (Exception ex)
                {
                  Log.Error("Utils: An exception occured creating CreateTileThumb: {0}", ex.Message);
                }
              }

              try
              {
                string tmpFile = Path.GetTempFileName();
                bmp.Save(tmpFile, Thumbs.ThumbCodecInfo, Thumbs.ThumbEncoderParams);
                Log.Debug("CreateTileThumb: Saving thumb!");

                Picture.CreateThumbnail(tmpFile, aThumbPath, (int) Thumbs.ThumbLargeResolution,
                                        (int) Thumbs.ThumbLargeResolution, 0, false);
                FileDelete(tmpFile);

                if (defaultBackgroundResized != null)
                {
                  defaultBackgroundResized.Dispose();
                }
                if (defaultBackgroundOriginal != null)
                {
                  defaultBackgroundOriginal.Dispose();
                }
                if (img != null)
                {
                  img.Dispose();
                }

                if (aPictureList.Count > 0)
                {
                  string pictureListName = string.Empty;
                  try
                  {
                    for (int i = 0; i < (aPictureList.Count); i++)
                    {
                      pictureListName = aPictureList[i];
                      File.Delete(aPictureList[i]);
                    }
                  }
                  catch (FileNotFoundException)
                  {
                    Log.Debug("CreateTileThumb: {0} file not found.", pictureListName);
                  }
                }

                if (MediaPortal.Player.g_Player.Playing)
                  Thread.Sleep(100);
                else
                  Thread.Sleep(10);

                if (FileExistsInCache(aThumbPath))
                  result = true;
              }
              catch (Exception ex2)
              {
                Log.Error("Utils: An exception occured saving CreateTileThumb: {0} - {1}", aThumbPath,
                          ex2.Message);
              }
            }
          }
        }
        catch (FileNotFoundException)
        {
          Log.Warn("Utils: Your skin does not supply previewbackground.png to create CreateTileThumb!");
        }
        catch (Exception exm)
        {
          Log.Error("Utils: An error occured creating folder CreateTileThumb: {0}", exm.Message);
        }
      }
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
      string temp = artistName.ToLower(CultureInfo.CurrentCulture);

      foreach (string s in _artistNamePrefixes)
      {
        if (s.Length == 0)
          continue;

        string prefix = s;
        prefix = prefix.Trim().ToLower(CultureInfo.CurrentCulture);
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

    // Move the prefix of movie to the end of the string for better sorting
    public static bool StripMovieNamePrefix(ref string movieName, bool appendPrefix)
    {
      string[] movieNamePrefixes;
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        // Movie title prefix strip
        movieNamePrefixes = xmlreader.GetValueAsString("moviedatabase", "titleprefixes", "The, Les, Die").Split(',');
      }
      string temp = movieName.ToLower(CultureInfo.CurrentCulture);

      foreach (string s in movieNamePrefixes)
      {
        if (s.Length == 0)
          continue;

        string prefix = s;
        prefix = prefix.Trim().ToLower(CultureInfo.CurrentCulture);
        int pos = temp.IndexOf(prefix + " ");
        if (pos == 0)
        {
          string tempName = movieName.Substring(prefix.Length).Trim();

          if (appendPrefix)
            movieName = string.Format("{0}, {1}", tempName, movieName.Substring(0, prefix.Length));
          else
            movieName = tempName;

          return true;
        }
      }

      return false;
    }

    public static void DeleteFiles(string strDir, string strPattern)
    {
      DeleteFiles(strDir, strPattern, false);
    }

    public static void DeleteFiles(string strDir, string strPattern, bool recursive)
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
          catch (Exception) {}
        }
        if (recursive)
        {
          var subDirs = Directory.GetDirectories(strDir);
          foreach (string subDir in subDirs)
          {
            try
            {
              DeleteFiles(subDir, strPattern, true);
              Directory.Delete(subDir);
      }
      catch (Exception) {}
    }
        }
      }
      catch (Exception) {}
    }

    public static bool UsingTvServer
    {
      get
      {
        try
        {
          return (Util.Utils.FileExistsInCache(Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll"));
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
      catch (Exception) {}
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
            catch (Exception) {}
          }
        }
      }
      catch (Exception) {}

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
            catch (Exception) {}
          }
        }
      }
      catch (Exception) {}

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
            catch (Exception) {}
          }
        }
      }
      catch (Exception) {}
    }

    //void DeleteOldTimeShiftFiles(string path)

    public static void DeleteRecording(string recordingFilename)
    {
      Utils.FileDelete(recordingFilename);

      int pos = recordingFilename.LastIndexOf(@"\");
      if (pos < 0)
        return;
      string path = Path.GetDirectoryName(recordingFilename);
      string filename = Path.GetFileNameWithoutExtension(recordingFilename);

      filename = filename.ToLowerInvariant();
      string[] files;
      try
      {
        files = Directory.GetFiles(path);
        foreach (string fileName in files)
        {
          try
          {
            if (fileName.ToLowerInvariant().IndexOf(filename) >= 0)
            {
              //delete all Timeshift buffer files
              if (fileName.ToLowerInvariant().IndexOf(".sbe") >= 0)
              {
                File.Delete(fileName);
              }
              //delete Thumbnails
              if (fileName.ToLowerInvariant().IndexOf(".jpg") >= 0)
              {
                File.Delete(fileName);
              }
              //delete comskip txt file
              if (fileName.ToLowerInvariant().IndexOf(".txt") >= 0)
              {
                File.Delete(fileName);
              }
              //delete Matroska tag file
              if (fileName.ToLowerInvariant().IndexOf(".xml") >= 0)
              {
                File.Delete(fileName);
              }
            }
          }
          catch (Exception) {}
        }
      }
      catch (Exception) {}
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

    public static void RestartMePo()
    {
      File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
      Log.Info("Restarting - saving settings...");
      Settings.SaveCache();
      Process restartScript = new Process();
      restartScript.EnableRaisingEvents = false;
      restartScript.StartInfo.WorkingDirectory = Config.GetFolder(Config.Dir.Base);
      restartScript.StartInfo.FileName = Config.GetFile(Config.Dir.Base, @"restart.vbs");
      Log.Debug("Restarting - executing script {0}", restartScript.StartInfo.FileName);
      restartScript.Start();
      try
      {
        // Maybe the scripting host is not available therefore do not wait infinitely.
        if (!restartScript.HasExited)
        {
          restartScript.WaitForExit();
        }
      }
      catch (Exception ex)
      {
        Log.Error("Restarting - WaitForExit: {0}", ex.Message);
      }
    }

    public static string EncryptPin(string code)
    {
      string result = string.Empty;
      foreach (char c in code)
        try
        {
          result += crypt[(int)c - 48];
        }
        catch {}
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
        catch {}
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
      string languageTranslated = "";
      switch (language.ToLowerInvariant())
      {
        case "undetermined":
          languageTranslated = GUILocalizeStrings.Get(2599);
          break;
        case "english":
          languageTranslated = GUILocalizeStrings.Get(2600);
          break;
        case "german":
          languageTranslated = GUILocalizeStrings.Get(2601);
          break;
        case "french":
          languageTranslated = GUILocalizeStrings.Get(2602);
          break;
        case "dutch":
          languageTranslated = GUILocalizeStrings.Get(2603);
          break;
        case "norwegian":
          languageTranslated = GUILocalizeStrings.Get(2604);
          break;
        case "italian":
          languageTranslated = GUILocalizeStrings.Get(2605);
          break;
        case "swedish":
          languageTranslated = GUILocalizeStrings.Get(2606);
          break;
        case "spanish":
          languageTranslated = GUILocalizeStrings.Get(2607);
          break;
        case "portuguese":
          languageTranslated = GUILocalizeStrings.Get(2608);
          break;
        case "danish":
          languageTranslated = GUILocalizeStrings.Get(2609);
          break;
        case "polish":
          languageTranslated = GUILocalizeStrings.Get(2610);
          break;
        case "czech":
          languageTranslated = GUILocalizeStrings.Get(2611);
          break;
        case "hungarian":
          languageTranslated = GUILocalizeStrings.Get(2612);
          break;
        case "russian":
          languageTranslated = GUILocalizeStrings.Get(2613);
          break;
        case "ukrainian":
          languageTranslated = GUILocalizeStrings.Get(2614);
          break;
        case "deutsch":
          languageTranslated = GUILocalizeStrings.Get(2615);
          break;
        default:
          break;
      }
      if (languageTranslated.Equals(""))
        languageTranslated = language;
      return languageTranslated;
    }


    public static string GetCultureRegionLanguage()
    {
      string strLongLanguage = CultureInfo.CurrentCulture.EnglishName;
      int iTrimIndex = strLongLanguage.IndexOf(" ", 0, strLongLanguage.Length);
      string strShortLanguage = strLongLanguage.Substring(0, iTrimIndex);

      foreach (CultureInfo cultureInformation in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
      {
        if (cultureInformation.EnglishName.ToLower(CultureInfo.CurrentCulture).IndexOf(strShortLanguage.ToLower(CultureInfo.CurrentCulture)) != -1)
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

    /// <summary>
    /// Will load all files in path with specified extensions.
    /// </summary>
    /// <param name="path">Path where to get files from</param>
    /// <param name="extensions">Extension ("dll" for example). Multiple extensions should be in format "ext1|ext2|ext3"</param>
    /// <returns>String list of file names</returns>
    public static string[] GetFiles(string path, string extensions)
    {
      string[] allExtensions = extensions.Split('|');
      List<string> files = new List<string>();
      foreach (string sp in allExtensions)
      {
        string[] strFiles = System.IO.Directory.GetFiles(path, "*." + sp);
        foreach (string strFile in strFiles)
        {
          //this check is needed because GetFiles truncates extension to 3 letters, 
          //therefore "*.htm" would find both htm and html files, together with any other variant (htmshit for example)
          if (!strFile.ToLowerInvariant().EndsWith("." + sp))
          {
            continue;
          }
          files.Add(strFile);
        }
      }
      files.Sort();
      return files.ToArray();
    }

    /// <summary>
    /// Tags such as artist allow multiple values.  We expect these to be separated
    /// by ; but for formatting and easier searching we show this as |
    /// trim off start and end separators
    /// </summary>
    /// <param name="tagValue">The value stored in the tag</param>
    /// <param name="stripArtistPrefixes">Whether to strip prefixes, eg The Beatles => Beatles, The</param>
    /// <returns></returns>
    public static string FormatMultiItemMusicStringTrim(string tagValue, bool stripArtistPrefixes)
    {
      String formattedString = FormatMultiItemMusicString(tagValue, stripArtistPrefixes);
      char[] charsToTrim = { '|', ' ' };
      return formattedString.Trim(charsToTrim);
    }

    /// <summary>
    /// Tags such as artist allow multiple values.  We expect these to be separated
    /// by ; but for formatting and easier searching we show this as |
    /// </summary>
    /// <param name="tagValue">The value stored in the tag</param>
    /// <param name="stripArtistPrefixes">Whether to strip prefixes, eg The Beatles => Beatles, The</param>
    /// <returns></returns>
    public static string FormatMultiItemMusicString(string tagValue, bool stripArtistPrefixes)
    {
      string[] strSplit = tagValue.Split(new char[] { ';', '|' });
      // Can't use a simple String.Join as i need to trim all the elements 
      string strFormattedString = "| ";
      foreach (string strTmp in strSplit)
      {
        string s = strTmp.Trim();
        // Strip Artist / AlbumArtist but NOT Genres
        if (stripArtistPrefixes)
        {
          Utils.StripArtistNamePrefix(ref s, true);
        }
        strFormattedString += String.Format("{0} | ", s.Trim());
      }
      return strFormattedString;
    }

    public static bool IsGUISettingsWindow(int windowId)
    {
      if (windowId == (int)GUIWindow.Window.WINDOW_SETTINGS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_DVD ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_BLURAY ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_EXTENSIONS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_FOLDERS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GENERALMAIN ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GENERALMP ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GENERALSTARTUP ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GENERALRESUME ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GENERALREFRESHRATE ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_MOVIES ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_MUSIC ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_MUSICDATABASE ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_MUSICNOWPLAYING ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUIMAIN ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUISKIN ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUIGENERAL ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUIONSCREEN_DISPLAY ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUICONTROL ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUISKIPSTEPS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUITHUMBNAILS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_PICTURES ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_PICTURES_SLIDESHOW ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_PICTURESDATABASE ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_PLAYLIST ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_RECORDINGS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUISCREENSETUP ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GUISCREENSAVER ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_SORT_CHANNELS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_TV ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_TV_EPG ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_VIDEODATABASE ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_VIDEOOTHERSETTINGS ||
          windowId == (int)GUIWindow.Window.WINDOW_SETTINGS_GENERALVOLUME ||
        // Minidisplay (no enum values in GUIWindow)
          windowId == 9000 ||
          windowId == 9001 ||
          windowId == 9002 ||
          windowId == 9003 ||
          windowId == 9004 ||
          windowId == 9005 ||
          windowId == 9006)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Returns connected USB hard disk drives letters
    /// Works only from Vista and above
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAvailableUsbHardDisks()
    {
      List<string> disks = new List<string>();
      
      try
      {
        // browse all USB WMI physical disks
        foreach (ManagementObject drive in
          new ManagementObjectSearcher(
            "select DeviceID, Model from Win32_DiskDrive where InterfaceType='USB' AND MediaType LIKE '%hard disk%'").
            Get())
        {
          // associate physical disks with partitions
          ManagementObject partition = new ManagementObjectSearcher(String.Format(
            "associators of {{Win32_DiskDrive.DeviceID='{0}'}} where AssocClass = Win32_DiskDriveToDiskPartition",
            drive["DeviceID"])).First();

          if (partition != null)
          {
            // associate partitions with logical disks (drive letter volumes)
            ManagementObject logical = new ManagementObjectSearcher(String.Format(
              "associators of {{Win32_DiskPartition.DeviceID='{0}'}} where AssocClass = Win32_LogicalDiskToPartition",
              partition["DeviceID"])).First();

            if (logical != null)
            {
              disks.Add(logical["Name"].ToString());
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Utils: GetUsbHardDisks Error: {0}", ex.Message);
      }

      return disks;
    }

    /// <summary>
    /// Returns connected USB removable disk drives letters
    /// Works only from Vista and above
    /// </summary>
    /// <returns></returns>
    public static List<string> GetRemovableUsbDisks()
    {
      List<string> disks = new List<string>();

      try
      {
        // browse all USB WMI physical disks
        foreach (ManagementObject drive in
          new ManagementObjectSearcher(
            "select DeviceID, Model from Win32_DiskDrive where InterfaceType='USB' AND MediaType LIKE '%removable%' AND Model LIKE '%disk%'").
            Get())
        {
          // associate physical disks with partitions
          ManagementObject partition = new ManagementObjectSearcher(String.Format(
            "associators of {{Win32_DiskDrive.DeviceID='{0}'}} where AssocClass = Win32_DiskDriveToDiskPartition",
            drive["DeviceID"])).First();

          if (partition != null)
          {
            // associate partitions with logical disks (drive letter volumes)
            ManagementObject logical = new ManagementObjectSearcher(String.Format(
              "associators of {{Win32_DiskPartition.DeviceID='{0}'}} where AssocClass = Win32_LogicalDiskToPartition",
              partition["DeviceID"])).First();

            if (logical != null)
            {
              disks.Add(logical["Name"].ToString());
            }
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("Utils: GetUsbHardDisks Error: {0}", ex.Message);
      }

      return disks;
    }

    public static string GetTreePath(string filename, int depth, int step)
    {
      string basename = Path.GetFileNameWithoutExtension(filename) ?? "";
      string tree = "";
      int i = basename.Length;

      while ((i-=step)>=0 && depth-->0)
      {
        tree += basename.Substring(i, step) + @"\";
  }
      return tree;
    }

    public static bool CreatePath(string path)
    {
      var paths = new Stack<string>();

      path = Path.GetFullPath(path);
      var rootPath = Path.GetPathRoot(path);
      if (!Directory.Exists(rootPath))
        return false;

      while(!string.IsNullOrEmpty(path) && path != rootPath && !Directory.Exists(path))
      {
        paths.Push(path);
        path = Path.GetDirectoryName(path);
      }

      while(paths.Count>0)
      {
        Directory.CreateDirectory(paths.Pop());
      }

      return true;
    }

    public static string GetThumbnailPathname(string basePath, string file, string formatString)
    {
      file = EncryptLine(file);
      // TODO: define dept/step in constants or make it configurable
      var path = Path.Combine(basePath, GetTreePath(file,  1, 2));
      CreatePath(path);
      return Path.Combine(path, string.Format(formatString, file));
    }

    #region Thumbnail path helpers

    public static string GetPicturesThumbPathname(string file)
    {
      return GetThumbnailPathname(Thumbs.Pictures, file, "{0}.jpg");
    }

    public static string GetPicturesLargeThumbPathname(string file)
    {
      return GetThumbnailPathname(Thumbs.Pictures, file, "{0}L.jpg");
    }

    public static string GetVideosThumbPathname(string file)
    {
      return GetThumbnailPathname(Thumbs.Videos, file, "{0}.jpg");
    }

    public static string GetVideosLargeThumbPathname(string file)
    {
      return GetThumbnailPathname(Thumbs.Videos, file, "{0}L.jpg");
    }

    #endregion
  }

  public static class GenericExtensions
  {
    public static XmlNode SelectSingleNodeFast(this XmlNode node, string xpath)
    {
      // XmlNode.SelectSingleNode finds all occurances as oppossed to a single one, this causes huge perf issues (about 50% of control creation according to dotTrace)
      XmlNodeList nodes = node.SelectNodes(xpath);

      if (nodes == null)
        return null;

      IEnumerator enumerator = nodes.GetEnumerator();
      if (enumerator != null && enumerator.MoveNext())
      {
        return (XmlNode)enumerator.Current;
      }
      return null;
    }

    /// <summary>
    /// In simple cases this is much much faster than a Full XPATH query
    /// </summary>
    /// <param name="node"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static XmlNode SelectByNameFromChildren(this XmlNode node, string name)
    {
      if (node == null || string.IsNullOrEmpty(name) || !node.HasChildNodes)
        return null;
      foreach (XmlNode child in node.ChildNodes)
        if (name.Equals(child.Name, StringComparison.OrdinalIgnoreCase))
          return child;
      return null;
    }

    public static TResult TryGetOrAdd<TIn, TResult>(this Dictionary<TIn, TResult> cache, TIn arg,
                                                    Func<TIn, TResult> evaluator)
    {
      TResult res;
      if (cache.TryGetValue(arg, out res))
        return res;
      res = evaluator(arg);
      cache.Add(arg, res);
      return res;
    }

    public static IAsyncResult DoAsync<T>(this Action<T> action, T arg)
    {
      return action.BeginInvoke(arg, null, null);
    }

    public static void WaitForAll(this IEnumerable<IAsyncResult> operations)
    {
      while (operations.Any(o => !o.IsCompleted))
        System.Threading.Thread.Sleep(10);
    }

    public static IEnumerable<IAsyncResult> DoActionAsync<T>(this IEnumerable<T> source, Action<T> actionOnItem)
    {
      return source.Select(i => actionOnItem.DoAsync(i))
        .ToArray();
    }
  }
  
  public class StringLogicalComparer : IComparer, IComparer<string>
  {
    public static int Compare(string x, string y)
    {
      return CompareStrings(x, y);
    }

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int StrCmpLogicalW(string x, string y);

    private static int CompareStrings(string x, string y)
    {
      return StrCmpLogicalW(x, y);
    }

    private static int CompareObjects(object x, object y)
    {
      return StrCmpLogicalW((string) x, (string) y);
    }

    int IComparer<string>.Compare(string x, string y)
    {
      if (null == x && null == y) return 0;
      if (null == x) return -1;
      if (null == y) return 1;
      return Compare(x, y);
    }

    int IComparer.Compare(object x, object y)
    {
      if (null == x && null == y) return 0;
      if (null == x) return -1;
      if (null == y) return 1;
      return CompareObjects(x, y);
    }
  }
}