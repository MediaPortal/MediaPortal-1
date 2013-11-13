#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

// http://www.codeproject.com/Articles/3054/Shell-Notifications-in-C

using System;
using System.Runtime.InteropServices;

namespace MediaPortal.Util
{
  /// <summary>
  /// Class that allow to receive shell notifications
  /// </summary>
  public class ShellNotifications
  {
    #region Properties and Constants

    private uint _notifyid;
    public const uint WmShnotify = 0x0401;
    public const int MaxPath = 260;
    
    public uint NotifyID
    {
      get { return (_notifyid); }
    }

    #endregion

    #region DllImports

    [DllImport("shell32.dll", EntryPoint = "#2", CharSet = CharSet.Auto)]
    private static extern uint SHChangeNotifyRegister(
      IntPtr hWnd,
      SHCNF fSources,
      SHCNE fEvents,
      uint wMsg,
      int cEntries,
      ref SHChangeNotifyEntry pFsne);

    [DllImport("shell32.dll", EntryPoint = "#4", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern Boolean SHChangeNotifyUnregister(
      UInt32 hNotify);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int SHGetFileInfoA(
      uint pidl,
      uint fileAttributes,
      out SHFILEINFO fi,
      uint fileInfo,
      SHGFI flags);

    [DllImport("Shell32.Dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern Boolean SHGetPathFromIDList(
      [In] IntPtr pidl,
      [In, Out, MarshalAs(UnmanagedType.LPTStr)] String pszPath);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern uint SHGetSpecialFolderLocation(
      IntPtr hWnd,
      CSIDL nFolder,
      out IntPtr pidl);
    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHChangeNotifyEntry
    {
      public IntPtr pIdl;
      [MarshalAs(UnmanagedType.Bool)]
      public Boolean Recursively;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SHFILEINFO
    {
      private IntPtr hIcon;
      private int iIcon;
      private uint dwAttributes;
      [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
      public string szDisplayName;
      [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
      private string szTypeName;

      public SHFILEINFO(bool b)
      {
        hIcon = IntPtr.Zero; 
        iIcon = 0; 
        dwAttributes = 0; 
        szDisplayName = ""; 
        szTypeName = "";
      }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct SHNOTIFYSTRUCT
    {
      public IntPtr dwItem1;
      public IntPtr dwItem2;
    };

    #endregion

    #region CSIDL Enum
    public enum CSIDL
    {
      /// <summary>
      /// Desktop
      /// </summary>
      CSIDL_DESKTOP = 0x0000,
      /// <summary>
      /// Internet Explorer (icon on desktop)
      /// </summary>
      CSIDL_INTERNET = 0x0001,
      /// <summary>
      /// Start Menu\Programs
      /// </summary>
      CSIDL_PROGRAMS = 0x0002,
      /// <summary>
      /// My Computer\Control Panel
      /// </summary>
      CSIDL_CONTROLS = 0x0003,
      /// <summary>
      /// My Computer\Printers
      /// </summary>
      CSIDL_PRINTERS = 0x0004,
      /// <summary>
      /// My Documents
      /// </summary>
      CSIDL_PERSONAL = 0x0005,
      /// <summary>
      /// user name\Favorites
      /// </summary>
      CSIDL_FAVORITES = 0x0006,
      /// <summary>
      /// Start Menu\Programs\Startup
      /// </summary>
      CSIDL_STARTUP = 0x0007,
      /// <summary>
      /// user name\Recent
      /// </summary>
      CSIDL_RECENT = 0x0008,
      /// <summary>
      /// user name\SendTo
      /// </summary>
      CSIDL_SENDTO = 0x0009,
      /// <summary>
      /// desktop\Recycle Bin
      /// </summary>
      CSIDL_BITBUCKET = 0x000a,
      /// <summary>
      /// user name\Start Menu
      /// </summary>
      CSIDL_STARTMENU = 0x000b,
      /// <summary>
      /// logical "My Documents" desktop icon
      /// </summary>
      CSIDL_MYDOCUMENTS = 0x000c,
      /// <summary>
      /// "My Music" folder
      /// </summary>
      CSIDL_MYMUSIC = 0x000d,
      /// <summary>
      /// "My Videos" folder
      /// </summary>
      CSIDL_MYVIDEO = 0x000e,
      /// <summary>
      /// user name\Desktop
      /// </summary>
      CSIDL_DESKTOPDIRECTORY = 0x0010,
      /// <summary>
      /// My Computer
      /// </summary>
      CSIDL_DRIVES = 0x0011,
      /// <summary>
      /// Network Neighborhood (My Network Places)
      /// </summary>
      CSIDL_NETWORK = 0x0012,
      /// <summary>
      /// user name>nethood
      /// </summary>
      CSIDL_NETHOOD = 0x0013,
      /// <summary>
      /// windows\fonts
      /// </summary>
      CSIDL_FONTS = 0x0014,
      CSIDL_TEMPLATES = 0x0015,
      /// <summary>
      /// All Users\Start Menu
      /// </summary>
      CSIDL_COMMON_STARTMENU = 0x0016,
      /// <summary>
      /// All Users\Start Menu\Programs
      /// </summary>
      CSIDL_COMMON_PROGRAMS = 0X0017,
      /// <summary>
      /// All Users\Startup
      /// </summary>
      CSIDL_COMMON_STARTUP = 0x0018,
      /// <summary>
      /// All Users\Desktop
      /// </summary>
      CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019,
      /// <summary>
      /// user name\Application Data
      /// </summary>
      CSIDL_APPDATA = 0x001a,
      /// <summary>
      /// user name\PrintHood
      /// </summary>
      CSIDL_PRINTHOOD = 0x001b,
      /// <summary>
      /// user name\Local Settings\Applicaiton Data (non roaming)
      /// </summary>
      CSIDL_LOCAL_APPDATA = 0x001c,
      /// <summary>
      /// non localized startup
      /// </summary>
      CSIDL_ALTSTARTUP = 0x001d,
      /// <summary>
      /// non localized common startup
      /// </summary>
      CSIDL_COMMON_ALTSTARTUP = 0x001e,
      CSIDL_COMMON_FAVORITES = 0x001f,
      CSIDL_INTERNET_CACHE = 0x0020,
      CSIDL_COOKIES = 0x0021,
      CSIDL_HISTORY = 0x0022,
      /// <summary>
      /// All Users\Application Data
      /// </summary>
      CSIDL_COMMON_APPDATA = 0x0023,
      /// <summary>
      /// GetWindowsDirectory()
      /// </summary>
      CSIDL_WINDOWS = 0x0024,
      /// <summary>
      /// GetSystemDirectory()
      /// </summary>
      CSIDL_SYSTEM = 0x0025,
      /// <summary>
      /// C:\Program Files
      /// </summary>
      CSIDL_PROGRAM_FILES = 0x0026,
      /// <summary>
      /// C:\Program Files\My Pictures
      /// </summary>
      CSIDL_MYPICTURES = 0x0027,
      /// <summary>
      /// USERPROFILE
      /// </summary>
      CSIDL_PROFILE = 0x0028,
      /// <summary>
      /// x86 system directory on RISC
      /// </summary>
      CSIDL_SYSTEMX86 = 0x0029,
      /// <summary>
      /// x86 C:\Program Files on RISC
      /// </summary>
      CSIDL_PROGRAM_FILESX86 = 0x002a,
      /// <summary>
      /// C:\Program Files\Common
      /// </summary>
      CSIDL_PROGRAM_FILES_COMMON = 0x002b,
      /// <summary>
      /// x86 Program Files\Common on RISC
      /// </summary>
      CSIDL_PROGRAM_FILES_COMMONX86 = 0x002c,
      /// <summary>
      /// All Users\Templates
      /// </summary>
      CSIDL_COMMON_TEMPLATES = 0x002d,
      /// <summary>
      /// All Users\Documents
      /// </summary>
      CSIDL_COMMON_DOCUMENTS = 0x002e,
      /// <summary>
      /// All Users\Start Menu\Programs\Administrative Tools
      /// </summary>
      CSIDL_COMMON_ADMINTOOLS = 0x002f,
      /// <summary>
      /// user name\Start Menu\Programs\Administrative Tools
      /// </summary>
      CSIDL_ADMINTOOLS = 0x0030,
      /// <summary>
      /// Network and Dial-up Connections
      /// </summary>
      CSIDL_CONNECTIONS = 0x0031,
      /// <summary>
      /// All Users\My Music
      /// </summary>
      CSIDL_COMMON_MUSIC = 0x0035,
      /// <summary>
      /// All Users\My Pictures
      /// </summary>
      CSIDL_COMMON_PICTURES = 0x0036,
      /// <summary>
      /// All Users\My Video
      /// </summary>
      CSIDL_COMMON_VIDEO = 0x0037,
      /// <summary>
      /// Resource Direcotry
      /// </summary>
      CSIDL_RESOURCES = 0x0038,
      /// <summary>
      /// Localized Resource Direcotry
      /// </summary>
      CSIDL_RESOURCES_LOCALIZED = 0x0039,
      /// <summary>
      /// Links to All Users OEM specific apps
      /// </summary>
      CSIDL_COMMON_OEM_LINKS = 0x003a,
      /// <summary>
      /// USERPROFILE\Local Settings\Application Data\Microsoft\CD Burning
      /// </summary>
      CSIDL_CDBURN_AREA = 0x003b,
      /// <summary>
      /// Computers Near Me (computered from Workgroup membership)
      /// </summary>
      CSIDL_COMPUTERSNEARME = 0x003d,
      /// <summary>
      /// combine with CSIDL_ value to force folder creation in SHGetFolderPath()
      /// </summary>
      CSIDL_FLAG_CREATE = 0x8000,
      /// <summary>
      /// combine with CSIDL_ value to return an unverified folder path
      /// </summary>
      CSIDL_FLAG_DONT_VERIFY = 0x4000,
      /// <summary>
      /// combine with CSIDL_ value to insure non-alias versions of the pidl
      /// </summary>
      CSIDL_FLAG_NO_ALIAS = 0x1000,
      /// <summary>
      /// combine with CSIDL_ value to indicate per-user init (eg. upgrade)
      /// </summary>
      CSIDL_FLAG_PER_USER_INIT = 0x0800,
      /// <summary>
      /// mask for all possible 
      /// </summary>
      CSIDL_FLAG_MASK = 0xFF00,
    }
    #endregion

    #region SHNCF Enum

    [Flags]
    public enum SHCNF
    {
      SHCNF_IDLIST = 0x0000,
      SHCNF_PATHA = 0x0001,
      SHCNF_PRINTERA = 0x0002,
      SHCNF_DWORD = 0x0003,
      SHCNF_PATHW = 0x0005,
      SHCNF_PRINTERW = 0x0006,
      SHCNF_TYPE = 0x00FF,
      SHCNF_FLUSH = 0x1000,
      SHCNF_FLUSHNOWAIT = 0x2000
    }
    #endregion

    #region SHCNE Enum

    [Flags]
    public enum SHCNE : uint
    {
      SHCNE_RENAMEITEM = 0x00000001,
      SHCNE_CREATE = 0x00000002,
      SHCNE_DELETE = 0x00000004,
      SHCNE_MKDIR = 0x00000008,
      SHCNE_RMDIR = 0x00000010,
      SHCNE_MEDIAINSERTED = 0x00000020,
      SHCNE_MEDIAREMOVED = 0x00000040,
      SHCNE_DRIVEREMOVED = 0x00000080,
      SHCNE_DRIVEADD = 0x00000100,
      SHCNE_NETSHARE = 0x00000200,
      SHCNE_NETUNSHARE = 0x00000400,
      SHCNE_ATTRIBUTES = 0x00000800,
      SHCNE_UPDATEDIR = 0x00001000,
      SHCNE_UPDATEITEM = 0x00002000,
      SHCNE_SERVERDISCONNECT = 0x00004000,
      SHCNE_UPDATEIMAGE = 0x00008000,
      SHCNE_DRIVEADDGUI = 0x00010000,
      SHCNE_RENAMEFOLDER = 0x00020000,
      SHCNE_FREESPACE = 0x00040000,
      SHCNE_EXTENDED_EVENT = 0x04000000,
      SHCNE_ASSOCCHANGED = 0x08000000,
      SHCNE_DISKEVENTS = 0x0002381F,
      SHCNE_GLOBALEVENTS = 0x0C0581E0,
      SHCNE_ALLEVENTS = 0x7FFFFFFF,
      SHCNE_INTERRUPT = 0x80000000,
    }
    #endregion

    #region SHGFI Enum
    public enum SHGFI : uint
    {
      SHGFI_ICON = 0x000000100,     // get icon
      SHGFI_DISPLAYNAME = 0x000000200,     // get display name
      SHGFI_TYPENAME = 0x000000400,     // get type name
      SHGFI_ATTRIBUTES = 0x000000800,     // get attributes
      SHGFI_ICONLOCATION = 0x000001000,     // get icon location
      SHGFI_EXETYPE = 0x000002000,     // return exe type
      SHGFI_SYSICONINDEX = 0x000004000,     // get system icon index
      SHGFI_LINKOVERLAY = 0x000008000,     // put a link overlay on icon
      SHGFI_SELECTED = 0x000010000,     // show icon in selected state
      SHGFI_ATTR_SPECIFIED = 0x000020000,     // get only specified attributes
      SHGFI_LARGEICON = 0x000000000,     // get large icon
      SHGFI_SMALLICON = 0x000000001,     // get small icon
      SHGFI_OPENICON = 0x000000002,     // get open icon
      SHGFI_SHELLICONSIZE = 0x000000004,     // get shell size icon
      SHGFI_PIDL = 0x000000008,     // pszPath is a pidl
      SHGFI_USEFILEATTRIBUTES = 0x000000010,     // use passed dwFileAttribute
      SHGFI_ADDOVERLAYS = 0x000000020,     // apply the appropriate overlays
      SHGFI_OVERLAYINDEX = 0x000000040,     // Get the index of the overlay
    }
    #endregion

    #region SHGetFolderLocationReturnValues Enum
    public enum SHGetFolderLocationReturnValues : uint
    {
      /// <summary>
      /// Success
      /// </summary>
      S_OK = 0x00000000,
      /// <summary>
      /// The CSIDL in nFolder is valid but the folder does not exist
      /// </summary>
      S_FALSE = 0x00000001,
      /// <summary>
      /// The CSIDL in nFolder is not valid
      /// </summary>
      E_INVALIDARG = 0x80070057
    }
    #endregion

    #region Register Functions

    /// <summary>
    /// Register a form handle
    /// This form will receive a WM_SHNOTIFY when a notification occures
    /// 
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="folderID">Root folder of the 'spy' (CSIDL_DESKTOP vor example)</param>
    /// <param name="recursively">Look recursively at modifications</param>
    /// <returns></returns>
    public ulong RegisterChangeNotify(IntPtr hWnd, CSIDL folderID, bool recursively)
    {
      if (_notifyid != 0)
      {
        return (0);
      }
      
      SHChangeNotifyEntry changeentry = new SHChangeNotifyEntry();
      changeentry.pIdl = GetPidlFromFolderID(hWnd, folderID);
      changeentry.Recursively = recursively;

      _notifyid = SHChangeNotifyRegister(
        hWnd,
        SHCNF.SHCNF_TYPE | SHCNF.SHCNF_IDLIST,
        //SHCNE.SHCNE_ALLEVENTS | SHCNE.SHCNE_INTERRUPT,
        SHCNE.SHCNE_MEDIAINSERTED | SHCNE.SHCNE_MEDIAREMOVED,
        WmShnotify,
        1,
        ref changeentry);
      return (_notifyid);
    }

    /// <summary>
    /// Unregister the form
    /// </summary>
    /// <returns>True if successfull</returns>
    public Boolean UnregisterChangeNotify()
    {
      if (_notifyid == 0)
      {
        return (false);
      }

      if (SHChangeNotifyUnregister(_notifyid))
      {
        _notifyid = 0;
        return (true);
      }
      return (false);
    }

    #endregion

    #region Pidl functions

    /// <summary>
    /// Get the path from a Pidl value
    /// </summary>
    /// <param name="Pidl">Pidl of the path</param>
    /// <returns>Path</returns>
    public static string GetPathFromPidl(IntPtr Pidl)
    {
      string path = new String('\0', MaxPath);
      return (SHGetPathFromIDList(Pidl, path) ? path.ToString().TrimEnd('\0') : "");
    }

    /// <summary>
    /// Do not work
    /// If someone has a solution please tell me (caudalth@etu.utc.fr)
    /// </summary>
    /// <param name="Pidl"></param>
    /// <returns></returns>
    public static string GetDisplayNameFromPidl(IntPtr Pidl)
    {
      SHFILEINFO fileinfo = new SHFILEINFO(true);
      SHGetFileInfoA(
        (uint)Pidl,
        0,
        out fileinfo,
        (uint)Marshal.SizeOf(fileinfo),
        SHGFI.SHGFI_PIDL | SHGFI.SHGFI_DISPLAYNAME);
      return (fileinfo.szDisplayName);
    }

    /// <summary>
    /// Get the Pidl from a special folder ID
    /// </summary>
    /// <param name="hWnd">Handle of the window</param>
    /// <param name="Id">ID of the special folder</param>
    /// <returns>Pidl of the special folder</returns>
    public static IntPtr GetPidlFromFolderID(IntPtr hWnd, CSIDL Id)
    {
      IntPtr pIdl = IntPtr.Zero;
      SHGetFolderLocationReturnValues res = (SHGetFolderLocationReturnValues)
        SHGetSpecialFolderLocation(
        hWnd,
        Id,
        out pIdl);
      return (pIdl);
    }

    #endregion

    #region Notification Function

    /// <summary>
    /// Message received from the WndProc of a registered form
    /// </summary>
    /// <param name="wParam"></param>
    /// <param name="lParam"></param>
    /// <param name="info"></param>
    public bool NotificationReceipt(IntPtr wParam, IntPtr lParam, ref NotifyInfos info)
    {
      SHNOTIFYSTRUCT shNotify = (SHNOTIFYSTRUCT)Marshal.PtrToStructure(wParam, typeof(SHNOTIFYSTRUCT));

      if (info.Notification == SHCNE.SHCNE_FREESPACE || info.Notification == SHCNE.SHCNE_UPDATEIMAGE)
      {
        return false;
      }

      info.Item1 = GetPathFromPidl(shNotify.dwItem1);
      info.Item2 = GetPathFromPidl(shNotify.dwItem2);
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Structure that contain informations about a notification
  /// </summary>
  public struct NotifyInfos
  {
    public NotifyInfos(ShellNotifications.SHCNE notification) { Notification = notification; Item1 = ""; Item2 = ""; }
    public ShellNotifications.SHCNE Notification;
    public string Item1;
    public string Item2;
  };
}
