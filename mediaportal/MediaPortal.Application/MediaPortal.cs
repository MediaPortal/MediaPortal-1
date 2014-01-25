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

#region usings

using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using MediaPortal;
using MediaPortal.Common.Utils;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Pictures;
using MediaPortal.InputDevices;
using MediaPortal.IR;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.Properties;
using MediaPortal.RedEyeIR;
using MediaPortal.Ripper;
using MediaPortal.SerialIR;
using MediaPortal.Util;
using MediaPortal.Services;
using MediaPortal.Visualization;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.Win32;
using Action = MediaPortal.GUI.Library.Action;

#endregion

// ReSharper disable EmptyNamespace
namespace MediaPortal {}
// ReSharper restore EmptyNamespace

/// <summary>
/// 
/// </summary>
public class MediaPortalApp : D3D, IRender
{
  #region vars
  
  private static bool           _useRestartOptions;
  private static bool           _isWinScreenSaverInUse;
  private static bool           _mpCrashed;
  private static bool           _skinOverrideNoTheme;
  private static bool           _waitForTvServer;
  private static bool           _isRendering;
  #if !DEBUG
  private static bool           _avoidVersionChecking;
  #endif
  private readonly bool         _useScreenSaver = true;
  private readonly bool         _useIdlePluginScreen;
  private bool                  _idlePluginActive = false;
  private bool                  _changeScreen;
  private readonly bool         _useIdleblankScreen;
  private int                   _idlePluginWindowId;
  private readonly bool         _showLastActiveModule;
  private readonly bool         _stopOnLostAudioRenderer; 
  private bool                  _playingState;
  private bool                  _showStats;
  private bool                  _showStatsPrevious;
  private bool                  _restoreTopMost;
  private bool                  _startWithBasicHome;
  private bool                  _useOnlyOneHome;
  private bool                  _suspended;
  private bool                  _ignoreContextMenuAction;
  private bool                  _supportsFiltering;
  private bool                  _supportsAlphaBlend;
  private bool                  _mouseClickFired;
  private bool                  _useLongDateFormat;
  private static int            _startupDelay;
  private readonly int          _timeScreenSaver;
  private readonly int          _suspendGracePeriodSec;
  private int                   _xpos;
  private int                   _frameCount;
  private int                   _errorCounter;
  private int                   _anisotropy;
  private static string         _alternateConfig;
  private static string         _safePluginsList;
  private string                _dateFormat;
  private string                _outdatedSkinName;
  private static DateTime       _lastOnresume;
  private DateTime              _updateTimer;
  private DateTime              _lastContextMenuAction;
  private Point                 _lastCursorPosition;
  private SerialUIR             _serialuirdevice;
  private USBUIRT               _usbuirtdevice;
  private WinLirc               _winlircdevice;
  private RedEye                _redeyedevice;
  private MouseEventArgs        _lastMouseClickEvent;
  private readonly Rectangle[]  _region;
  private static RestartOptions _restartOptions;
  private IntPtr                _deviceNotificationHandle;
  private IntPtr                _displayStatusHandle;
  private IntPtr                _userPresenceHandle;
  private IntPtr                _awayModeHandle;

  // ReSharper disable InconsistentNaming
  private const int WM_SYSCOMMAND            = 0x0112; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int SC_MINIMIZE              = 0xF020; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int SC_MONITORPOWER          = 0xF170; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int SC_SCREENSAVE            = 0xF140; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  private const int WM_ENDSESSION            = 0x0016; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa376889(v=vs.85).aspx
  private const int WM_DEVICECHANGE          = 0x0219; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363480(v=vs.85).aspx
  private const int DBT_DEVICEARRIVAL        = 0x8000; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363211(v=vs.85).aspx
  private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363211(v=vs.85).aspx
  private const int WM_QUERYENDSESSION       = 0x0011; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa376890(v=vs.85).aspx
  private const int WM_ACTIVATE              = 0x0006; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WA_INACTIVE              = 0;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WA_ACTIVE                = 1;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WA_CLICKACTIVE           = 2;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  private const int WM_SIZING                = 0x0214; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_LEFT                = 1;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_RIGHT               = 2;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_TOP                 = 3;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_TOPLEFT             = 4;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_TOPRIGHT            = 5;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_BOTTOM              = 6;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_BOTTOMLEFT          = 7;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WMSZ_BOTTOMRIGHT         = 8;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632647(v=vs.85).aspx
  private const int WM_SIZE                  = 0x0005; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632646(v=vs.85).aspx
  private const int SIZE_RESTORED            = 0;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632646(v=vs.85).aspx
  private const int SIZE_MINIMIZED           = 1;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632646(v=vs.85).aspx
  private const int SIZE_MAXIMIZED           = 2;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632646(v=vs.85).aspx
  private const int SIZE_MAXSHOW             = 3;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632646(v=vs.85).aspx
  private const int SIZE_MAXHIDE             = 4;      // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632646(v=vs.85).aspx
  private const int WM_GETMINMAXINFO         = 0x0024; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632626(v=vs.85).aspx
  private const int WM_MOVING                = 0x0216; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632632(v=vs.85).aspx
  private const int WM_ENTERSIZEMOVE         = 0x0231; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632622(v=vs.85).aspx
  private const int WM_EXITSIZEMOVE          = 0x0232; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632623(v=vs.85).aspx
  private const int WM_DISPLAYCHANGE         = 0x007E; // http://msdn.microsoft.com/en-us/library/windows/desktop/dd145210(v=vs.85).aspx
  private const int WM_POWERBROADCAST        = 0x0218; //http://msdn.microsoft.com/en-us/library/windows/desktop/aa373247(v=vs.85).aspx
  private const int PBT_APMSUSPEND           = 0x0004; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372721(v=vs.85).aspx
  private const int PBT_APMRESUMECRITICAL    = 0x0006; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372719(v=vs.85).aspx
  private const int PBT_APMRESUMESUSPEND     = 0x0007; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372720(v=vs.85).aspx
  private const int PBT_APMRESUMEAUTOMATIC   = 0x0012; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372718(v=vs.85).aspx
  private const int PBT_POWERSETTINGCHANGE   = 0x8013; // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372718(v=vs.85).aspx
  private const int SPI_GETSCREENSAVEACTIVE  = 0x0010; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  private const int SPI_SETSCREENSAVEACTIVE  = 0x0011; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  private const int SPIF_SENDCHANGE          = 0x0002; // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  private const int D3DERR_DEVICEHUNG        = -2005530508; // http://msdn.microsoft.com/en-us/library/windows/desktop/bb172554(v=vs.85).aspx
  private const int D3DERR_DEVICEREMOVED     = -2005530512; // http://msdn.microsoft.com/en-us/library/windows/desktop/bb172554(v=vs.85).aspx
  private const int D3DERR_INVALIDCALL       = -2005530516; // http://msdn.microsoft.com/en-us/library/windows/desktop/bb172554(v=vs.85).aspx

  private const int DEVICE_NOTIFY_WINDOW_HANDLE         = 0;
  private const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
  private const int DBT_DEVTYP_DEVICEINTERFACE          = 5;

  private static readonly Guid KSCATEGORY_RENDER  = new Guid("{65E8773E-8F56-11D0-A3B9-00A0C9223196}");
  private static readonly Guid RDP_REMOTE_AUDIO = new Guid("{E6327CAD-DCEC-4949-AE8A-991E976A79D2}");
  
  // http://msdn.microsoft.com/en-us/library/windows/desktop/hh448380(v=vs.85).aspx
  private static Guid GUID_MONITOR_POWER_ON             = new Guid("02731015-4510-4526-99e6-e5a17ebd1aea"); 
  private static Guid GUID_SESSION_DISPLAY_STATUS       = new Guid("2b84c20e-ad23-4ddf-93db-05ffbd7efca5");
  private static Guid GUID_SESSION_USER_PRESENCE        = new Guid("{3c0f4548-c03f-4c4d-b9f2-237ede686376}");
  private static Guid GUID_SYSTEM_AWAYMODE              = new Guid("98a7f580-01f7-48aa-9c0f-44352c29e5c0");
  // ReSharper restore InconsistentNaming

  private const string MPMutex     = "{E0151CBA-7F81-41df-9849-F5298A779EB3}";
  #pragma warning disable 169
  private const string ConfigMutex = "{0BFD648F-A59F-482A-961B-337D70968611}";
  #pragma warning restore 169

  private ShellNotifications Notifications = new ShellNotifications();

  #endregion

  #region enumns

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa373208(v=vs.85).aspx
  [FlagsAttribute]
  // ReSharper disable InconsistentNaming
  public enum EXECUTION_STATE : uint
  {
    ES_SYSTEM_REQUIRED   = 0x00000001,
    ES_DISPLAY_REQUIRED  = 0x00000002,
    ES_USER_PRESENT      = 0x00000004, // Legacy flag, should not be used.
    ES_AWAYMODE_REQUIRED = 0x00000040,
    ES_CONTINUOUS        = 0x80000000,
  }
  // ReSharper restore InconsistentNaming

  //http://msdn.microsoft.com/en-us/library/windows/desktop/aa363480(v=vs.85).aspx
  // ReSharper disable InconsistentNaming
  // ReSharper disable UnusedMember.Local
  private enum DBT_EVENT
  {
    DBT_CONFIGCHANGECANCELED    = 0x0019,
    DBT_CONFIGCHANGED           = 0x0018,
    DBT_CUSTOMEVENT             = 0x8006,
    DBT_DEVICEARRIVAL           = 0x8000,
    DBT_DEVICEQUERYREMOVE       = 0x8001,
    DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
    DBT_DEVICEREMOVECOMPLETE    = 0x8004,
    DBT_DEVICEREMOVEPENDING     = 0x8003,
    DBT_DEVICETYPESPECIFIC      = 0x8005,
    DBT_DEVNODES_CHANGED        = 0x0007,
    DBT_QUERYCHANGECONFIG       = 0x0017,
    DBT_USERDEFINED             = 0xFFFF
  }
  // ReSharper restore UnusedMember.Local
  // ReSharper restore InconsistentNaming

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa373247(v=vs.85).aspx
  // ReSharper disable InconsistentNaming
  // ReSharper disable UnusedMember.Local
  private enum PBT_EVENT
  {
    PBT_APMPOWERSTATUSCHANGE  = 0x000A,
    PBT_APMRESUMEAUTOMATIC    = 0x0012,
    PBT_APMRESUMESUSPEND      = 0x0007,
    PBT_APMSUSPEND            = 0x0004,
    PBT_POWERSETTINGCHANGE    = 0x8013,
    // XP only
    PBT_APMBATTERYLOW         = 0x0009,
    PBT_APMOEMEVENT           = 0x000B,
    PBT_APMQUERYSUSPEND       = 0x0000,
    PBT_APMQUERYSUSPENDFAILED = 0x0002,
    PBT_APMRESUMECRITICAL     = 0x0006
  }
  // ReSharper restore UnusedMember.Local
  // ReSharper restore InconsistentNaming

  // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646274(v=vs.85).aspx
  // ReSharper disable InconsistentNaming
  // ReSharper disable UnusedMember.Local
  private enum WA_EVENT
  {
    WA_ACTIVE      = 1,
    WA_CLICKACTIVE = 2,
    WA_INACTIVE    = 0
  }
  // ReSharper restore InconsistentNaming
  // ReSharper restore UnusedMember.Local

  // ReSharper disable InconsistentNaming
  // ReSharper disable UnusedMember.Local
  //http://msdn.microsoft.com/en-us/library/windows/desktop/aa363246(v=vs.85).aspx
  private enum DBT_DEV_TYPE
  {
    DBT_DEVTYP_DEVICEINTERFACE = 0x00000005,
    DBT_DEVTYP_HANDLE          = 0x00000006,
    DBT_DEVTYP_OEM             = 0x00000000,
    DBT_DEVTYP_PORT            = 0x00000003,
    DBT_DEVTYP_VOLUME          = 0x00000002
  }
  // ReSharper restore InconsistentNaming
  // ReSharper restore UnusedMember.Local

  // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646360(v=vs.85).aspx
  // ReSharper disable InconsistentNaming
  // ReSharper disable UnusedMember.Local
  private enum SYSCOMMAND
  {
    SC_CLOSE        = 0xF060,
    SC_CONTEXTHELP  = 0xF180,
    SC_DEFAULT      = 0xF160,
    SC_HOTKEY       = 0xF150,
    SC_HSCROLL      = 0xF080,
    SCF_ISSECURE    = 0x0001,
    SC_KEYMENU      = 0xF100,
    SC_MAXIMIZE     = 0xF030,
    SC_MINIMIZE     = 0xF020,
    SC_MONITORPOWER = 0xF170,
    SC_MOUSEMENU    = 0xF090,
    SC_MOVE         = 0xF010,
    SC_NEXTWINDOW   = 0xF040,
    SC_PREVWINDOW   = 0xF050,
    SC_RESTORE      = 0xF120,
    SC_SCREENSAVE   = 0xF140,
    SC_SIZE         = 0xF000,
    SC_TASKLIST     = 0xF130,
    SC_VSCROLL      = 0xF070
  }
  // ReSharper restore InconsistentNaming
  // ReSharper restore UnusedMember.Local

  #endregion

  #region structs

  // ReSharper disable InconsistentNaming
  // ReSharper disable NotAccessedField.Local
  #pragma warning disable 169, 649
  private struct POINTAPI
  {
    public int x;
    public int y;
  }

  // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632605(v=vs.85).aspx
  private struct MINMAXINFO
  {
    public POINTAPI ptReserved;
    public POINTAPI ptMaxSize;
    public POINTAPI ptMaxPosition;
    public POINTAPI ptMinTrackSize;
    public POINTAPI ptMaxTrackSize;
  }
  #pragma warning restore 169, 649
  // ReSharper restore NotAccessedField.Local

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363244(v=vs.85).aspx
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct DEV_BROADCAST_DEVICEINTERFACE
  {
    public int    dbcc_size;
    public int    dbcc_devicetype;
    public int    dbcc_reserved;
    public Guid   dbcc_classguid;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
    public string dbcc_name;
  }


  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363246(v=vs.85).aspx
  [StructLayout(LayoutKind.Sequential)]
  public struct DEV_BROADCAST_HDR
  {
    public int dbcc_size;
    public int dbcc_devicetype;
    public int dbcc_reserved;
  }

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363245(v=vs.85).aspx
  [StructLayout(LayoutKind.Sequential)]
  public struct DEV_BROADCAST_HANDLE
  {
    public int    dbch_size;
    public int    dbch_devicetype;
    public int    dbch_reserved;
    public IntPtr dbch_handle;
    public IntPtr dbch_hdevnotify;
    public Guid   dbch_eventguid;
    public long   dbch_nameoffset;
    public byte   dbch_data;
    public byte   dbch_data1;
  }

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa372723(v=vs.85).aspx
  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  internal struct POWERBROADCAST_SETTING
  {
    public Guid PowerSetting;
    public uint DataLength;
    public byte Data;
  }
  // ReSharper restore InconsistentNaming

  #endregion

  #region imports

  // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  [DllImport("user32")]
  private static extern bool SystemParametersInfo(int uAction, int uParam, ref bool lpvParam, int fuWinIni);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/ms724947(v=vs.85).aspx
  [DllImport("user32")]
  private static extern bool SystemParametersInfo(int uAction, int uParam, int lpvParam, int fuWinIni);

  //http://msdn.microsoft.com/en-us/library/windows/desktop/ms686234(v=vs.85).aspx
  [DllImport("kernel32")]
  private static extern bool SetProcessWorkingSetSize(IntPtr handle, int minSize, int maxSize);

  // http://msdn.microsoft.com/en-us/library/ms648415(v=vs.85).aspx
  [DllImport("User32.dll", CharSet = CharSet.Auto)]
  private static extern void DisableProcessWindowsGhosting();

  // http://msdn.microsoft.com/en-us/library/windows/desktop/bb773640(v=vs.85).aspx
  [DllImport("shlwapi.dll")]
  private static extern bool PathIsNetworkPath(string path);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363431(v=vs.85).aspx
  [DllImport("user32.dll", SetLastError = true)]
  private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, uint flags);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363475(v=vs.85).aspx
  [DllImport("user32.dll", CharSet = CharSet.Auto)]
  private static extern uint UnregisterDeviceNotification(IntPtr hHandle);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa373208(v=vs.85).aspx
  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa373196(v=vs.85).aspx
  [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
  private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid powerSettingGuid, Int32 flags);

  // http://msdn.microsoft.com/en-us/library/windows/desktop/aa373237(v=vs.85).aspx
  [DllImport(@"User32", EntryPoint = "UnregisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
  private static extern bool UnregisterPowerSettingNotification(IntPtr handle);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool SetProcessDPIAware();

  #endregion

  #region main()

  [STAThread]
  public static void Main(string[] args)
  {
    Thread.CurrentThread.Name = "MPMain";

    if (Environment.OSVersion.Version.Major >= 6)
    {
      SetProcessDPIAware();
    }

    #if !DEBUG
    // TODO: work on the handlers to take over more Watchdog capabilities, current use for Area51 builds as needed only
    //AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    //Application.ThreadException += OnThreadException;
    #endif  

    SkinOverride         = string.Empty;
    WindowedOverride     = false;
    FullscreenOverride   = false;
    ScreenNumberOverride = -1;

    if (args.Length > 0)
    {
      foreach (string arg in args)
      {
        if (arg == "/fullscreen")
        {
          FullscreenOverride = true;
        }

        if (arg == "/windowed")
        {
          WindowedOverride = true;
        }

        if (arg.StartsWith("/fullscreen="))
        {
          string argValue = arg.Remove(0, 12); // remove /?= from the argument  
          FullscreenOverride |= argValue != "no";
          WindowedOverride |= argValue.Equals("no");
        }

        if (arg == "/crashtest")
        {
          _mpCrashed = true;
        }

        if (arg.StartsWith("/screen="))
        {
          string screenarg = arg.Remove(0, 8); // remove /?= from the argument          
          if (!int.TryParse(screenarg, out ScreenNumberOverride))
          {
            ScreenNumberOverride = -1;
          }
        }

        if (arg.StartsWith("/skin="))
        {
          string skinOverrideArg = arg.Remove(0, 6); // remove /?= from the argument
          SkinOverride = skinOverrideArg;
        }

        if (arg.StartsWith("/config="))
        {
          _alternateConfig = arg.Remove(0, 8); // remove /?= from the argument
          if (!Path.IsPathRooted(_alternateConfig))
          {
            _alternateConfig = Config.GetFile(Config.Dir.Config, _alternateConfig);
          }
        }

        if (arg.StartsWith("/safelist="))
        {
          _safePluginsList = arg.Remove(0, 10); // remove /?= from the argument
        }

        if (arg == "/NoTheme")
        {
          _skinOverrideNoTheme = true;
        }

        #if !DEBUG
        _avoidVersionChecking = arg.ToLowerInvariant() == "/avoidversioncheck";
        #endif
      }
    }

    // check if MediaPotal is already running
    using (var processLock = new ProcessLock(MPMutex))
    {
      if (processLock.AlreadyExists)
      {
        Log.Warn("Main: MediaPortal is already running");
        Win32API.ActivatePreviousInstance();
      }
    }
   
    if (string.IsNullOrEmpty(_alternateConfig))
    {
      Log.BackupLogFiles();
    }
    else
    {
      if (File.Exists(_alternateConfig))
      {
        try
        {
          MPSettings.ConfigPathName = _alternateConfig;
          Log.BackupLogFiles();
          Log.Info("Using alternate configuration file: {0}", MPSettings.ConfigPathName);
        }
        catch (Exception ex)
        {
          Log.BackupLogFiles();
          Log.Error("Failed to change to alternate configuration file:");
          Log.Error(ex);
        }
      }
      else
      {
        Log.BackupLogFiles();
        Log.Info("Alternative configuration file was specified but the file was not found: '{0}'", _alternateConfig);
        Log.Info("Using default configuration file instead.");
      }
    }

    if (!Config.DirsFileUpdateDetected)
    {
      // check if Mediaportal has been configured
      var fi = new FileInfo(MPSettings.ConfigPathName);
      if (!File.Exists(MPSettings.ConfigPathName) || (fi.Length < 10000))
      {
        // no, then start configuration.exe in wizard form
        Log.Info("MediaPortal.xml not found. Launching configuration tool and exiting...");
        try
        {
          Process.Start(Config.GetFile(Config.Dir.Base, "configuration.exe"), @"/wizard");
        }
        // ReSharper disable EmptyGeneralCatchClause
        catch {} // no exception logging needed, since MP is now closed
        // ReSharper restore EmptyGeneralCatchClause
        return;
      }

      // TODO: check if config is valid. If you create a config file > 10000 bytes full of spaces MP will crash as Utils.dll does nearly no error checking

      using (Settings xmlreader = new MPSettings())
      {
        string threadPriority = xmlreader.GetValueAsString("general", "ThreadPriority", "Normal");
        switch (threadPriority)
        {
          case "AboveNormal":
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            break;
          case "High":
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            break;
          case "BelowNormal":
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            break;
        }
        _startupDelay    = xmlreader.GetValueAsBool("general", "delay startup", false) ? xmlreader.GetValueAsInt("general", "delay", 0): 0;
        _waitForTvServer = xmlreader.GetValueAsBool("general", "wait for tvserver", false);
      }

      #if !DEBUG
      bool watchdogEnabled;
      bool restartOnError;
      int restartDelay;
      using (Settings xmlreader = new MPSettings())
      {
        watchdogEnabled = xmlreader.GetValueAsBool("general", "watchdogEnabled", true);
        restartOnError  = xmlreader.GetValueAsBool("general", "restartOnError", false);
        restartDelay    = xmlreader.GetValueAsInt("general", "restart delay", 10);        
      }

      AddExceptionHandler();
      if (watchdogEnabled)
      {
        using (var sw = new StreamWriter(Config.GetFile(Config.Dir.Config, "mediaportal.running"), false))
        {
          sw.WriteLine("running");
          sw.Close();
        }

        Log.Info("Main: Starting MPWatchDog");
        string cmdargs = "-watchdog";
        if (restartOnError)
        {
          cmdargs += " -restartMP " + restartDelay.ToString(CultureInfo.InvariantCulture);
        }
        var mpWatchDog = new Process
                           {
                             StartInfo =
                               {
                                 ErrorDialog      = true,
                                 UseShellExecute  = true,
                                 WorkingDirectory = Application.StartupPath,
                                 FileName         = "WatchDog.exe",
                                 Arguments        = cmdargs
                               }
                           };
        mpWatchDog.Start();
      }
      #endif

      // Log MediaPortal version build and operating system level
      FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);

      Log.Info("Main: MediaPortal v" + versionInfo.FileVersion + " is starting up on " + OSInfo.OSInfo.GetOSDisplayVersion());

      #if DEBUG
      Log.Info("Debug Build: " + Application.ProductVersion);
      #else
      Log.Info("Build: " + Application.ProductVersion);
      #endif

      // setting minimum worker threads
      int minWorker, minIOC;
      ThreadPool.GetMinThreads(out minWorker, out minIOC);
      ThreadPool.SetMinThreads(minWorker * 2, minIOC * 1);
      ThreadPool.GetMinThreads(out minWorker, out minIOC);
      Log.Info("Main: Minimum number of worker threads to {0}/{1}", minWorker, minIOC);

      // Check for unsupported operating systems
      OSPrerequisites.OSPrerequisites.OsCheck(false);

      // Log last install of WindowsUpdate patches
      string lastSuccessTime = "NEVER !!!";
      UIntPtr res;

      int options = Convert.ToInt32(Reg.RegistryRights.ReadKey);
      if (OSInfo.OSInfo.Xp64OrLater())
      {
        options = options | Convert.ToInt32(Reg.RegWow64Options.KEY_WOW64_64KEY);
      }
      var rKey = new UIntPtr(Convert.ToUInt32(Reg.RegistryRoot.HKLM));
      int lastError;
      int retval = Reg.RegOpenKeyEx(rKey, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\Results\\Install", 0, options, out res);
      if (retval == 0)
      {
        uint tKey;
        uint lKey = 100;
        var sKey = new StringBuilder((int)lKey);
        retval = Reg.RegQueryValueEx(res, "LastSuccessTime", 0, out tKey, sKey, ref lKey);
        if (retval == 0)
        {
          lastSuccessTime = sKey.ToString();
        }
        else
        {
          lastError = Marshal.GetLastWin32Error();
          Log.Debug("RegQueryValueEx retval=<{0}>, lastError=<{1}>", retval, lastError);
        }
      }
      else
      {
        lastError = Marshal.GetLastWin32Error();
        Log.Debug("RegOpenKeyEx retval=<{0}>, lastError=<{1}>", retval, lastError);
      }
      Log.Info("Main: Last install from WindowsUpdate is dated {0}", lastSuccessTime);

      Log.Debug("Disabling process window ghosting");
      DisableProcessWindowsGhosting();

      // Start MediaPortal
      Log.Info("Main: Using Directories:");
      foreach (Config.Dir option in Enum.GetValues(typeof (Config.Dir)))
      {
        Log.Info("{0} - {1}", option, Config.GetFolder(option));
      }

      var mpFi = new FileInfo(Assembly.GetExecutingAssembly().Location);
      Log.Info("Main: Assembly creation time: {0} (UTC)", mpFi.LastWriteTimeUtc.ToUniversalTime());

      #pragma warning disable 168
      using (var processLock = new ProcessLock(MPMutex))
      #pragma warning restore 168
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Set current directory
        string applicationPath = Application.ExecutablePath;
        applicationPath = Path.GetFullPath(applicationPath);
        applicationPath = Path.GetDirectoryName(applicationPath);
        if (!String.IsNullOrEmpty(applicationPath))
        {
          Directory.SetCurrentDirectory(applicationPath);
          Log.Info("Main: Set current directory to: {0}", applicationPath);
        }
        else
        {
          Log.Error("Main: Cannot set current directory to {0}", applicationPath);
        }

        // log  about available displays
        foreach (var screen in Screen.AllScreens)
        {
          Log.Debug("Display: {0} - IsPrimary: {1} - BitsPerPixel: {2} - Bounds: {3}x{4} @ {5},{6} - WorkingArea: {7}x{8} @ {9},{10}",
            GetCleanDisplayName(screen), screen.Primary, screen.BitsPerPixel,
            screen.Bounds.Width, screen.Bounds.Height, screen.Bounds.X, screen.Bounds.Y,
            screen.WorkingArea.Width, screen.WorkingArea.Height, screen.WorkingArea.X, screen.WorkingArea.Y);
        }

        // Localization strings for new splash screen and for MediaPortal itself
        LoadLanguageString();

        // Initialize the skin and theme prior to beginning the splash screen thread.  This provides for the splash screen to be used in a theme.
        string skin;
        try
        {
          using (Settings xmlreader = new MPSettings())
          {
            skin = string.IsNullOrEmpty(SkinOverride) ? xmlreader.GetValueAsString("skin", "name", "Default") : SkinOverride;
          }
        }
        catch (Exception)
        {
           skin = "Default";
        }

        Config.SkinName = skin;
        GUIGraphicsContext.Skin = skin;
        SkinSettings.NoTheme = _skinOverrideNoTheme;
        SkinSettings.Load();

        // Send a message that the skin has changed.
        var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SKIN_CHANGED, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msg);

        Log.Info("Main: Skin is {0} using theme {1}", skin, GUIThemeManager.CurrentTheme);

        // Start Splash Screen
        string version = ConfigurationManager.AppSettings["version"];
        SplashScreen = new SplashScreen {Version = version};

        #if !DEBUG
        SplashScreen.Run();
        #endif

        
        if (_waitForTvServer)
        {
          Log.Debug("Main: Wait for TV service requested");
          ServiceController ctrl;
          try
          {
            ctrl = new ServiceController("TVService");
          }
          catch (Exception)
          {
            ctrl = null;
            Log.Debug("Main: Create ServiceController for TV service failed - proceeding...");
          }

          if (ctrl != null)
          {
            //Sanity check for existance of TV service
            ServiceControllerStatus status = ServiceControllerStatus.Stopped;
            try
            {
              status = ctrl.Status;
            }
            catch (Exception)
            {
              Log.Debug("Main: Failed to retrieve TV service status");
              ctrl.Close();
              ctrl = null;
            }
          }

          if (ctrl != null)
          {
            Log.Debug("Main: TV service found. Checking status...");
            UpdateSplashScreenMessage(GUILocalizeStrings.Get(60)); // Waiting for startup of TV service...
            if (ctrl.Status == ServiceControllerStatus.StartPending || ctrl.Status == ServiceControllerStatus.Stopped)
            {
              if (ctrl.Status == ServiceControllerStatus.StartPending)
              {
                Log.Info("Main: TV service start is pending. Waiting...");
              }

              if (ctrl.Status == ServiceControllerStatus.Stopped)
              {
                Log.Info("Main: TV service is stopped, so we try start it...");
                try
                {
                  ctrl.Start();
                }
                catch (Exception)
                {
                  Log.Info("TvService seems to be already starting up.");
                }
              }

              try
              {
                ctrl.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 45));
              }
              // ReSharper disable EmptyGeneralCatchClause
              catch (Exception) {}
              // ReSharper restore EmptyGeneralCatchClause
              
              if (ctrl.Status == ServiceControllerStatus.Running)
              {
                Log.Info("Main: The TV service has started successfully.");
              }
              else
              {
                Log.Info("Main: Startup of the TV service failed - current status: {0}", ctrl.Status.ToString());
              }
            }
            Log.Info("Main: TV service is in status {0} - proceeding...", ctrl.Status.ToString());
            ctrl.Close();
          }
        }


        if (_startupDelay > 0)
        {
          Log.Info("Main: Waiting {0} second(s) before startup", _startupDelay);
          for (int i = _startupDelay; i > 0; i--)
          {
            UpdateSplashScreenMessage(String.Format(GUILocalizeStrings.Get(61), i.ToString(CultureInfo.InvariantCulture)));
            Thread.Sleep(1000);
          }
        }

        Log.Debug("Main: Checking prerequisites");
        try
        {
          // check if DirectX 9.0c if installed
          Log.Debug("Main: Verifying DirectX 9");
          if (!DirectXCheck.IsInstalled())
          {
            DisableSplashScreen();
            string strLine = "Please install a newer DirectX 9.0c redist!\r\n";
            strLine = strLine + "MediaPortal cannot run without DirectX 9.0c redist (August 2008)\r\n";
            strLine = strLine + "http://install.team-mediaportal.com/DirectX";
            // ReSharper disable LocalizableElement
            MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // ReSharper restore LocalizableElement
            return;
          }


          #if !DEBUG
          // Check TvPlugin version
          string mpExe    = Assembly.GetExecutingAssembly().Location;
          string tvPlugin = Config.GetFolder(Config.Dir.Plugins) + "\\Windows\\TvPlugin.dll";
          if (File.Exists(tvPlugin) && !_avoidVersionChecking)
          {
            string tvPluginVersion = FileVersionInfo.GetVersionInfo(tvPlugin).ProductVersion;
            string mpVersion       = FileVersionInfo.GetVersionInfo(mpExe).ProductVersion;

            if (mpVersion != tvPluginVersion)
            {
              string strLine = "TvPlugin and MediaPortal don't have the same version.\r\n";
              strLine       += "Please update the older component to the same version as the newer one.\r\n";
              strLine       += "MediaPortal Version: " + mpVersion + "\r\n";
              strLine       += "TvPlugin    Version: " + tvPluginVersion;
              DisableSplashScreen();
              // ReSharper disable LocalizableElement
              MessageBox.Show(strLine, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
              // ReSharper restore LocalizableElement
              Log.Info(strLine);
              return;
            }
          }
          #endif

        }
        // ReSharper disable EmptyGeneralCatchClause
        catch (Exception) {}
        // ReSharper restore EmptyGeneralCatchClause

        try
        {
          UpdateSplashScreenMessage(GUILocalizeStrings.Get(62));
          Log.Debug("Main: Initializing DirectX");

          var app = new MediaPortalApp();
          if (app.Init())
          {
            try
            {
              Log.Info("Main: Running");
              GUIGraphicsContext.BlankScreen = false;
              Application.Run(app);
              app.Focus();
            }
            catch (Exception ex)
            {
              Log.Error(ex);
              Log.Error("MediaPortal stopped due to an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
              _mpCrashed = true;
            }
            app.OnExit();
          }

        }
        catch (Exception ex)
        {
          Log.Error(ex);
          Log.Error("MediaPortal stopped due to an exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
          _mpCrashed = true;
        }

        DisableSplashScreen();
        
        Settings.SaveCache();

        // only re-show the task bar if MP is the one that has hidden it.
        using (Settings xmlreader = new MPSettings())
        {
          if (xmlreader.GetValueAsBool("general", "hidetaskbar", false))
          {
            HideTaskBar(false);
          }
        }

        if (_useRestartOptions)
        {
          Log.Info("Main: Exiting Windows - {0}", _restartOptions);
          if (File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
          {
            File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
          }
          WindowsController.ExitWindows(_restartOptions, false);
        }
        else
        {
          if (!_mpCrashed && File.Exists(Config.GetFile(Config.Dir.Config, "mediaportal.running")))
          {
            File.Delete(Config.GetFile(Config.Dir.Config, "mediaportal.running"));
          }
        }
      }
    }
    else
    {
      DisableSplashScreen();
      string msg = "The file MediaPortalDirs.xml has been changed by a recent update in the MediaPortal application directory.\n\n";
      msg       += "You have to open the file ";
      msg       += Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Team MediaPortal\MediaPortalDirs.xml";
      msg       += " with an editor, update it with all changes and SAVE it at least once to start up MediaPortal successfully after this update.\n\n";
      msg       += "If you are not using windows user profiles for MediaPortal's configuration management, ";
      msg       += "just delete the whole directory mentioned above and reconfigure MediaPortal.";
      msg       += "\n\n\n";
      msg       += "Do you want to open your local file now?";
      Log.Error(msg);
      
      // ReSharper disable LocalizableElement
      DialogResult result = MessageBox.Show(msg, "MediaPortal - Update Conflict", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);
      // ReSharper restore LocalizableElement
      try
      {
        if (result == DialogResult.Yes)
        {
          Process.Start("notepad.exe",
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                        @"\Team MediaPortal\MediaPortalDirs.xml");
        }
      }
      catch (Exception)
      {
        // ReSharper disable LocalizableElement
        MessageBox.Show(
          "Error opening file " + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
          @"\Team MediaPortal\MediaPortalDirs.xml using notepad.exe", "Error", MessageBoxButtons.OK,
          MessageBoxIcon.Error);
        // ReSharper restore LocalizableElement
      }
    }
    Environment.Exit(0);
  }


  /// <summary>
  /// Disables the Splash Screen
  /// </summary>
  private static void DisableSplashScreen()
  {
    if (SplashScreen != null)
    {
      SplashScreen.Stop();
      SplashScreen = null;
    }
  }

  #if !DEBUG
  private static UnhandledExceptionLogger _logger;

  /// <remark>This method is only used in release builds.</remark>
  private static void AddExceptionHandler()
  {
    _logger = new UnhandledExceptionLogger();
    AppDomain current = AppDomain.CurrentDomain;
    current.UnhandledException += _logger.LogCrash;
  }
  #endif

  #endregion

  #region remote callbacks

  /// <summary>
  /// 
  /// </summary>
  /// <param name="command"></param>
  private static void OnRemoteCommand(object command)
  {
    GUIGraphicsContext.OnAction(new Action((Action.ActionType)command, 0, 0));
  }

  #endregion

  #region ctor

  /// <summary>
  /// 
  /// </summary>
  public MediaPortalApp()
  {
    // init attributes
    _xpos                  = 50;
    _lastOnresume          = DateTime.Now;
    _updateTimer           = DateTime.MinValue;
    _lastContextMenuAction = DateTime.MaxValue;
    _region                = new Rectangle[1];
    _restartOptions        = RestartOptions.Reboot;

    int screenNumber;
    using (Settings xmlreader = new MPSettings())
    {
      _suspendGracePeriodSec      = xmlreader.GetValueAsInt("general", "suspendgraceperiod", 5);
      _useScreenSaver             = xmlreader.GetValueAsBool("general", "IdleTimer", true);
      _timeScreenSaver            = xmlreader.GetValueAsInt("general", "IdleTimeValue", 300);
      _useIdleblankScreen         = xmlreader.GetValueAsBool("general", "IdleBlanking", false);
      _useIdlePluginScreen        = xmlreader.GetValueAsBool("general", "IdlePlugin", false);
      _idlePluginWindowId         = xmlreader.GetValueAsInt("general", "IdlePluginWindow", 0);
      _showLastActiveModule       = xmlreader.GetValueAsBool("general", "showlastactivemodule", false);
      screenNumber                = xmlreader.GetValueAsInt("screenselector", "screennumber", 0);
      _stopOnLostAudioRenderer    = xmlreader.GetValueAsBool("general", "stoponaudioremoval", true);
    }

    if (ScreenNumberOverride >= 0)
    {
      screenNumber = ScreenNumberOverride;
    }
    if (screenNumber < 0 || screenNumber >= Screen.AllScreens.Length)
    {
      screenNumber = 0;
    }

    GUIGraphicsContext.currentScreen = Screen.AllScreens[screenNumber];

    Log.Info("Main: MP is using screen: {0} (Position: {1},{2} Dimensions: {3}x{4})",
             GetCleanDisplayName(GUIGraphicsContext.currentScreen),
             GUIGraphicsContext.currentScreen.Bounds.Location.X, GUIGraphicsContext.currentScreen.Bounds.Location.Y,
             GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);

    // move form to selected screen
    Location = new Point(Location.X + GUIGraphicsContext.currentScreen.Bounds.X, Location.Y + GUIGraphicsContext.currentScreen.Bounds.Y);

    // temporarily set new client size for initialization purposes
    ClientSize = new Size(0, 0);

    // check if MediaPortal is already running...
    Log.Info("Main: Checking for running MediaPortal instance");
    Log.Info(@"Main: Deleting old log\capture.log");
    Utils.FileDelete(Config.GetFile(Config.Dir.Log, "capture.log"));

    
    // ReSharper disable LocalizableElement
    Text = "MediaPortal";
    // ReSharper restore LocalizableElement
    GUIGraphicsContext.form = this;
    GUIGraphicsContext.graphics = null;
    GUIGraphicsContext.RenderGUI = this;
    GUIGraphicsContext.Render3DMode = GUIGraphicsContext.eRender3DMode.None;

    using (Settings xmlreader = new MPSettings())
    {
      AutoHideMouse = xmlreader.GetValueAsBool("general", "autohidemouse", true);
      GUIGraphicsContext.MouseSupport = xmlreader.GetValueAsBool("gui", "mousesupport", false);
      GUIGraphicsContext.AllowRememberLastFocusedItem = xmlreader.GetValueAsBool("gui", "allowRememberLastFocusedItem", false);
      GUIGraphicsContext.DBLClickAsRightClick = xmlreader.GetValueAsBool("general", "dblclickasrightclick", false);
      MinimizeOnStartup = xmlreader.GetValueAsBool("general", "minimizeonstartup", false);
      MinimizeOnGuiExit = xmlreader.GetValueAsBool("general", "minimizeonexit", false);
      MinimizeOnFocusLoss = xmlreader.GetValueAsBool("general", "minimizeonfocusloss", false);
    }

    SetStyle(ControlStyles.Opaque, true);
    SetStyle(ControlStyles.UserPaint, true);
    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
    SetStyle(ControlStyles.DoubleBuffer, false);

    Activated  += MediaPortalAppActivated;
    Deactivate += MediaPortalAppDeactivate;
    
    Log.Info("Main: Checking skin version");
    CheckSkinVersion();
    using (Settings xmlreader = new MPSettings())
    {
      var startFullscreen = !WindowedOverride && (FullscreenOverride || xmlreader.GetValueAsBool("general", "startfullscreen", false));
      Windowed = !startFullscreen;
    }

    DoStartupJobs();
  }

  /// <summary>
  /// 
  /// </summary>
  private void DoStartupJobs()
  {
    Log.Debug("Main: DoStartupJobs()");
    FilterChecker.CheckInstalledVersions();

    Version aParamVersion;
    // 6.5.2600.3243 = KB941568,   6.5.2600.3024 = KB927544
    if (!FilterChecker.CheckFileVersion(Environment.SystemDirectory + "\\quartz.dll", "6.5.2600.3024", out aParamVersion))
    {
      string errorMsg = string.Format("Your version {0} of quartz.dll has too many bugs! \nPlease check our Wiki's requirements page.", aParamVersion);
      Log.Info("Util: quartz.dll error - {0}", errorMsg);
      // ReSharper disable LocalizableElement
      if (MessageBox.Show(errorMsg, "Core DirectShow component (quartz.dll) is outdated!", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
      // ReSharper restore LocalizableElement
      {
        Process.Start(@"http://wiki.team-mediaportal.com/GeneralRequirements");
      }
    }

    GUIWindowManager.OnNewAction += OnAction;
    GUIWindowManager.Receivers += OnMessage;
    GUIWindowManager.Callbacks += MPProcess;

    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STARTING;

    Utils.OnStartExternal += OnStartExternal;
    Utils.OnStopExternal += OnStopExternal;

    // register the playlistplayer for thread messages (like playback stopped,ended)
    Log.Info("Main: Init playlist player");
    g_Player.Factory = new PlayerFactory();
    PlaylistPlayer.Init();

    // Only load the USBUIRT device if it has been enabled in the configuration
    using (Settings xmlreader = new MPSettings())
    {
      bool inputEnabled = xmlreader.GetValueAsBool("USBUIRT", "internal", false);
      bool outputEnabled = xmlreader.GetValueAsBool("USBUIRT", "external", false);
      if (inputEnabled || outputEnabled)
      {
        Log.Info("Main: Creating the USBUIRT device");
        _usbuirtdevice = USBUIRT.Create(OnRemoteCommand);
        Log.Info("Main: Creating the USBUIRT device done");
      }

      // Load Winlirc if enabled.
      bool winlircInputEnabled = xmlreader.GetValueAsString("WINLIRC", "enabled", "false") == "true";
      if (winlircInputEnabled)
      {
        Log.Info("Main: Creating the WINLIRC device");
        _winlircdevice = new WinLirc();
        Log.Info("Main: Creating the WINLIRC device done");
      }

      // Load RedEye if enabled.
      bool redeyeInputEnabled = xmlreader.GetValueAsString("RedEye", "internal", "false") == "true";
      if (redeyeInputEnabled)
      {
        Log.Info("Main: Creating the REDEYE device");
        _redeyedevice = RedEye.Create(OnRemoteCommand);
        Log.Info("Main: Creating the RedEye device done");
      }

      // load SerialUIR if enabled
      inputEnabled = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";
      if (inputEnabled)
      {
        Log.Info("Main: Creating the SerialUIR device");
        _serialuirdevice = SerialUIR.Create(OnRemoteCommand);
        Log.Info("Main: Creating the SerialUIR device done");
      }
    }

    // registers the player for video window size notifications
    Log.Info("Main: Init players");
    g_Player.Init();

    GUIGraphicsContext.ActiveForm = Handle;

    var doc = new XmlDocument();
    try
    {
      doc.Load("mediaportal.exe.config");
      XmlNode node = doc.SelectSingleNode("/configuration/appStart/ClientApplicationInfo/appFolderName");

      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }

      node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/baseDir");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }

      node = doc.SelectSingleNode("/configuration/appUpdater/UpdaterConfiguration/application/client/tempDir");
      if (node != null)
      {
        node.InnerText = Directory.GetCurrentDirectory();
      }
      doc.Save("MediaPortal.exe.config");
    }
    // ReSharper disable EmptyGeneralCatchClause
    catch { }
    // ReSharper restore EmptyGeneralCatchClause

    Thumbs.CreateFolders();

    GUIGraphicsContext.ResetLastActivity();
  }

  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private static void MediaPortalAppDeactivate(object sender, EventArgs e)
  {
    GUIGraphicsContext.HasFocus = false;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  private static void MediaPortalAppActivated(object sender, EventArgs e)
  {
    GUIGraphicsContext.HasFocus = true;
  }

  #endregion

  #region RenderStats() method
  
  /// <summary>
  /// 
  /// </summary>
  private void RenderStats()
  {
    try
    {
      UpdateStats();

      if (GUIGraphicsContext.IsEvr && g_Player.HasVideo && GUIGraphicsContext.Vmr9Active)
      {
        if (_showStats != _showStatsPrevious)
        {
          // notify EVR presenter only when the setting changes
          if (VMR9Util.g_vmr9 == null)
          {
            return;
          }
          VMR9Util.g_vmr9.EnableEVRStatsDrawing(_showStats);
        }
        // EVR presenter will draw the stats internally
        _showStatsPrevious = _showStats;
        return;
      }
      _showStatsPrevious = false;

      if (_showStats)
      {
        GetStats();
        GUIFont font = GUIFontManager.GetFont(0);
        if (font != null)
        {
          GUIGraphicsContext.SetScalingResolution(0, 0, false);
          // '\n' doesn't work with the DirectX9 Ex device, so the string is split into two
          font.DrawText(80, 40, 0xffffffff, FrameStatsLine1, GUIControl.Alignment.ALIGN_LEFT, -1);
          font.DrawText(80, 55, 0xffffffff, FrameStatsLine2, GUIControl.Alignment.ALIGN_LEFT, -1);

          _region[0].X = _xpos;
          _region[0].Y = 0;
          _region[0].Width = 4;
          _region[0].Height = GUIGraphicsContext.Height;

          GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.FromArgb(255, 255, 255, 255), 1.0f, 0, _region);

          float fStep = (GUIGraphicsContext.Width - 100);
          fStep /= (2f * 16f);
          fStep /= GUIGraphicsContext.CurrentFPS;
          _frameCount++;
          if (_frameCount >= (int)fStep)
          {
            _frameCount = 0;
            _xpos += 12;
            if (_xpos > GUIGraphicsContext.Width - 50)
            {
              _xpos = 50;
            }
          }
        }
      }
    }
    // ReSharper disable EmptyGeneralCatchClause
    catch {} // Intentionally left blank - if stats rendering fails it is not a critical issue
    // ReSharper restore EmptyGeneralCatchClause
  }

  #endregion

  #region PreProcessMessage() and WndProc()

  /// <summary>
  /// Find the Greatest Common Divisor
  /// </summary>
  /// <param name="a">Number a</param>
  /// <param name="b">Number b</param>
  /// <returns>The greatest common Divisor</returns>
  private static long GCD(long a, long b)
  {
    while (b != 0)
    {
      long tmp = b;
      b = a % b;
      a = tmp;
    }
    return a;
  }
  

  /// <summary>
  /// Message Pump
  /// </summary>
  /// <param name="msg"></param>
  protected override void WndProc(ref Message msg)
  {
    try
    {
      switch (msg.Msg)
      {
        case (int)ShellNotifications.WmShnotify:
          NotifyInfos info = new NotifyInfos((ShellNotifications.SHCNE)(int)msg.LParam);
          
          if (Notifications.NotificationReceipt(msg.WParam, msg.LParam, ref info))
          {
            if (info.Notification == ShellNotifications.SHCNE.SHCNE_MEDIAINSERTED)
            {
              string path = info.Item1;

              if (Utils.IsRemovable(path))
              {
                string driveName = Utils.GetDriveName(info.Item1);
                GUIMessage gmsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ADD_REMOVABLE_DRIVE, 0, 0, 0, 0, 0, 0);
                gmsg.Label = info.Item1;
                gmsg.Label2 = String.Format("({0}) {1}", path, driveName);
                GUIGraphicsContext.SendMessage(gmsg);
              }
            }

            if (info.Notification == ShellNotifications.SHCNE.SHCNE_MEDIAREMOVED)
            {
              string path = info.Item1;

              if (Utils.IsRemovable(path))
              {
                string driveName = Utils.GetDriveName(info.Item1);
                GUIMessage gmsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE, 0, 0, 0, 0, 0, 0);
                gmsg.Label = info.Item1;
                gmsg.Label2 = String.Format("({0}) {1}", path, driveName);
                GUIGraphicsContext.SendMessage(gmsg);
              }
            }
          }
          return;

        // power management
        case WM_POWERBROADCAST:
          OnPowerBroadcast(ref msg);
          break;

        // set maximum and minimum form size in windowed mode
        case WM_GETMINMAXINFO:
          OnGetMinMaxInfo(ref msg);
          PluginManager.WndProc(ref msg);
          break;

        case WM_ENTERSIZEMOVE:
          Log.Debug("Main: WM_ENTERSIZEMOVE");
          PluginManager.WndProc(ref msg);
          break;

        case WM_EXITSIZEMOVE:
          Log.Debug("Main: WM_EXITSIZEMOVE");
          PluginManager.WndProc(ref msg);
          break;

        // only allow window to be moved inside a valid working area
        case WM_MOVING:
          OnMoving(ref msg);
          PluginManager.WndProc(ref msg);
          break;
        
        // verify window size in case it was not resized by the user
        case WM_SIZE:
          OnSize(ref msg);
          PluginManager.WndProc(ref msg);
          break;

        // aspect ratio save window resizing
        case WM_SIZING:
          OnSizing(ref msg);
          PluginManager.WndProc(ref msg);
          break;
        
        // handle display changes
        case WM_DISPLAYCHANGE:
          OnDisplayChange(ref msg);
          PluginManager.WndProc(ref msg);
          break;
        
        // handle device changes
        case WM_DEVICECHANGE:
          OnDeviceChange(ref msg);
          PluginManager.WndProc(ref msg);
          break;

        case WM_QUERYENDSESSION:
          Log.Debug("Main: WM_QUERYENDSESSION");
          PluginManager.WndProc(ref msg);
          base.WndProc(ref msg);
          ShuttingDown = true;
          msg.Result = (IntPtr)1;
          break;

        case WM_ENDSESSION:
          Log.Info("Main: WM_ENDESSION");
          PluginManager.WndProc(ref msg);
          base.WndProc(ref msg);
          Application.ExitThread();
          Application.Exit();
          msg.Result = (IntPtr)0;
          break;
        
        // handle activation and deactivation requests
        case WM_ACTIVATE:
          OnActivate(ref msg);
          PluginManager.WndProc(ref msg);
          break;

        // handle system commands
        case WM_SYSCOMMAND:
          // do not continue with rest of method in case we aborted screen saver or display powering off
          if (!OnSysCommand(ref msg))
          {
            return;
          }
          PluginManager.WndProc(ref msg);
          break;

        // handle default commands needed for plugins
        default:
          PluginManager.WndProc(ref msg);
          break;
      }

      // TODO: extract to method and change to correct code
      // forward message to input devices
      Action action;
      char key;
      Keys keyCode;
      if (InputDevices.WndProc(ref msg, out action, out key, out keyCode))
      {
        if (msg.Result.ToInt32() != 1)
        {
          msg.Result = new IntPtr(0);
        }

        if (action != null && action.wID != Action.ActionType.ACTION_INVALID)
        {
          Log.Info("Main: Incoming action: {0}", action.wID);
          if (ActionTranslator.GetActionDetail(GUIWindowManager.ActiveWindowEx, action) && (action.SoundFileName.Length > 0 && !g_Player.Playing))
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.ResetLastActivity();
          GUIGraphicsContext.OnAction(action);
        }

        if (keyCode != Keys.A)
        {
          Log.Info("Main: Incoming Keycode: {0}", keyCode.ToString());
          var ke = new KeyEventArgs(keyCode);
          OnKeyDown(ke);
          return; // abort WndProc()
        }

        if (key != 0)
        {
          Log.Info("Main: Incoming Key: {0}", key);
          var e = new KeyPressEventArgs(key);
          OnKeyPress(e);
          return; // abort WndProc()
        }

        return; // abort WndProc()
      }

      // forward message to player
      g_Player.WndProc(ref msg);

      // forward message to form class
      base.WndProc(ref msg);
    }

    catch (Exception ex)
    {
      Log.Error(ex);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private bool OnSysCommand(ref Message msg)
  {
    Log.Debug("Main: WM_SYSCOMMAND ({0})", Enum.GetName(typeof(SYSCOMMAND), msg.WParam.ToInt32() & 0xFFF0));
    bool result = true;
    switch (msg.WParam.ToInt32() & 0xFFF0)
    {
      // user clicked on minimize button
      case SC_MINIMIZE:
        Log.Debug("Main: SC_MINIMIZE");
        MinimizeToTray();
        break;

      // Windows is requesting to turn off the display
      case SC_MONITORPOWER:
        int displayState = msg.LParam.ToInt32();
        Log.Debug("Main: SC_MONITORPOWER {0}", displayState);
        if ((GUIGraphicsContext.IsFullScreenVideo && !g_Player.Paused) || GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW && displayState != -1)
        {
          PluginManager.WndProc(ref msg);
          Log.Info("Main: Active player - resetting idle timer for display to be turned off");
          SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
          msg.Result = (IntPtr)1;
          result = false;
        }
        else
        {
          msg.Result = (IntPtr)0;
        }
        break;

      // Windows is requesting to start the screen saver
      case SC_SCREENSAVE:
        Log.Debug("Main: SC_SCREENSAVE");
        if ((GUIGraphicsContext.IsFullScreenVideo && !g_Player.Paused) || GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_SLIDESHOW)
        {
          PluginManager.WndProc(ref msg);
          Log.Info("Main: Active player - resetting idle timer for screen save to be turned on");
          SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
          msg.Result = (IntPtr)1;
          result = false;
        }
        else
        {
          msg.Result = (IntPtr)0;
        }
        break;
    }
    return result;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnPowerBroadcast(ref Message msg)
  {
    Log.Debug("Main: WM_POWERBROADCAST ({0})", Enum.GetName(typeof(PBT_EVENT), msg.WParam.ToInt32()));
    switch (msg.WParam.ToInt32())
    {
      case PBT_APMSUSPEND:
        Log.Info("Main: Suspending operation");
        PluginManager.WndProc(ref msg);
        OnSuspend();
        break;

      // When resuming from hibernation, the OS always assume that a user is present. This is by design of Windows.
      case PBT_APMRESUMEAUTOMATIC:
        Log.Info("Main: Resuming operation");
        OnResumeAutomatic();
        PluginManager.WndProc(ref msg);
        break;

      // only for Windows XP
      case PBT_APMRESUMECRITICAL:
        Log.Info("Main: Resuming operation after a forced suspend");
        OnResumeAutomatic();
        OnResumeSuspend();
        PluginManager.WndProc(ref msg);
        break;

      case PBT_APMRESUMESUSPEND:
        OnResumeSuspend();
        PluginManager.WndProc(ref msg);
        break;

      case PBT_POWERSETTINGCHANGE:
        var ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(msg.LParam, typeof(POWERBROADCAST_SETTING));

        if (ps.PowerSetting == GUID_SYSTEM_AWAYMODE && ps.DataLength == Marshal.SizeOf(typeof(Int32)))
        {
          switch (ps.Data)
          {
            case 0:
              Log.Info("Main: The computer is exiting away mode");
              IsInAwayMode = false;
              break;
            case 1:
              Log.Info("Main: The computer is entering away mode");
              IsInAwayMode = true;
              break;
          }
        }
        // GUID_SESSION_DISPLAY_STATUS is only provided on Win8 and above
        else if ((ps.PowerSetting == GUID_MONITOR_POWER_ON || ps.PowerSetting == GUID_SESSION_DISPLAY_STATUS) && ps.DataLength == Marshal.SizeOf(typeof(Int32)))
        {
          switch (ps.Data)
          {
            case 0:
              Log.Info("Main: The display is off");
              IsDisplayTurnedOn = false;
              break;
            case 1:
              Log.Info("Main: The display is on");
              IsDisplayTurnedOn = true;
              ShowMouseCursor(false);
              break;
            case 2:
              Log.Info("Main: The display is dimmed");
              IsDisplayTurnedOn = true;
              break;
          }
        }
        // GUIT_SESSION_USER_PRESENCE is only provide on Win8 and above
        else if (ps.PowerSetting == GUID_SESSION_USER_PRESENCE && ps.DataLength == Marshal.SizeOf(typeof(Int32)))
        {
          switch (ps.Data)
          {
            case 0:
              Log.Info("Main: User is providing input to the session");
              IsUserPresent = true;
              ShowMouseCursor(false);
              break;
            case 2:
              Log.Info("Main: The user activity timeout has elapsed with no interaction from the user");
              IsUserPresent = false;
              break;
          }
        }
        PluginManager.WndProc(ref msg);
        break;
    }
    msg.Result = (IntPtr)1;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnActivate(ref Message msg)
  {
    int loword = unchecked((short) msg.WParam);
    Log.Debug("Main: WM_ACTIVATE ({0})", Enum.GetName(typeof(WA_EVENT), loword));
    switch (loword)
    {
      case WA_INACTIVE:
        Log.Info("Main: Deactivation request received");
        if (RefreshRateChanger.RefreshRateChangeRunning)
        {
          Log.Info("Main: Refresh rate changer running. Ignoring deactivation request");
          RefreshRateChanger.RefreshRateChangeRunning = false;
        }
        else
        {
          MinimizeToTray();
        }
        break;

      case WA_ACTIVE:
      case WA_CLICKACTIVE:
        Log.Info("Main: Activation request received");
        RestoreFromTray();
        break;
    }
    msg.Result = (IntPtr)0;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnDeviceChange(ref Message msg)
  {
    Log.Debug("Main: WM_DEVICECHANGE (Event: {0})", Enum.GetName(typeof(DBT_EVENT), msg.WParam.ToInt32()));
    RemovableDriveHelper.HandleDeviceChangedMessage(msg);

    // process additional data if available
    if (msg.LParam.ToInt32() != 0)
    {
      var hdr = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(msg.LParam, typeof(DEV_BROADCAST_HDR));
      if (hdr.dbcc_devicetype != DBT_DEVTYP_DEVICEINTERFACE)
      {
        Log.Debug("Main: Device type is {0}", Enum.GetName(typeof(DBT_DEV_TYPE), hdr.dbcc_devicetype));        
      }
      else
      {
        var deviceInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(msg.LParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

        // get friendly device name
        string deviceName = String.Empty;
        string[] values = deviceInterface.dbcc_name.Split('#');
        if (values.Length >= 3)
        {
          string deviceType = values[0].Substring(values[0].IndexOf(@"?\", StringComparison.Ordinal) + 2);
          string deviceInstanceID = values[1];
          string deviceUniqueID = values[2];
          string regPath = @"SYSTEM\CurrentControlSet\Enum\" + deviceType + "\\" + deviceInstanceID + "\\" + deviceUniqueID;
          RegistryKey regKey = Registry.LocalMachine.OpenSubKey(regPath);
          if (regKey != null)
          {
            // use the friendly name if it exists
            object result = regKey.GetValue("FriendlyName");
            if (result != null)
            {
              deviceName = result.ToString();
            }
              // if not use the device description's last part
            else
            {
              result = regKey.GetValue("DeviceDesc");
              if (result != null)
              {
                deviceName = result.ToString().Contains(@"%;") ? result.ToString().Substring(result.ToString().IndexOf(@"%;", StringComparison.Ordinal) + 2) : result.ToString();
              }
            }
          }
        }
        Log.Debug("Main: Device type is {0} - Name: {1}", Enum.GetName(typeof(DBT_DEV_TYPE), hdr.dbcc_devicetype), deviceName);

        // special chanding for audio renderer
        if (deviceInterface.dbcc_classguid == KSCATEGORY_RENDER || deviceInterface.dbcc_classguid == RDP_REMOTE_AUDIO)
        {
          switch (msg.WParam.ToInt32())
          {
            case DBT_DEVICEREMOVECOMPLETE:
              Log.Info("Main: Audio Renderer {0} removed", deviceName);
              if (_stopOnLostAudioRenderer)
              {
                g_Player.Stop();
                while (GUIGraphicsContext.IsPlaying)
                {
                  Thread.Sleep(100);
                }
              }
              try
              {
                VolumeHandler.Dispose();
                #pragma warning disable 168
                VolumeHandler vh = VolumeHandler.Instance;
                #pragma warning restore 168
              }
              catch (Exception exception)
              {
                Log.Warn("Main: Could not initialize volume handler: ", exception.Message);
              }
              break;

            case DBT_DEVICEARRIVAL:
              Log.Info("Main: Audio Renderer {0} connected", deviceName);
              if (_stopOnLostAudioRenderer)
              {
                g_Player.Stop();
                while (GUIGraphicsContext.IsPlaying)
                {
                  Thread.Sleep(100);
                }
              }
              try
              {
                VolumeHandler.Dispose();
                #pragma warning disable 168
                VolumeHandler vh = VolumeHandler.Instance;
                #pragma warning restore 168
              }
              catch (Exception exception)
              {
                Log.Warn("Main: Could not initialize volume handler: ", exception.Message);
              }
              break;
          }
        }
      }
    }
    msg.Result = (IntPtr)1;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnDisplayChange(ref Message msg)
  {
    Log.Debug("Main: WM_DISPLAYCHANGE");
    if (VMR9Util.g_vmr9 != null && GUIGraphicsContext.Vmr9Active && GUIGraphicsContext.IsEvr)
    {
      VMR9Util.g_vmr9.UpdateEVRDisplayFPS(); // Update FPS
    }
    Screen screen = Screen.FromControl(this);
    Rectangle currentBounds = GUIGraphicsContext.currentScreen.Bounds;
    Rectangle newBounds = screen.Bounds;
    if (Created && !Equals(screen, GUIGraphicsContext.currentScreen) || !Equals(currentBounds.Size, newBounds.Size))
    {
      Log.Info("Main: Screen MP OnDisplayChange is displayed on changed from {0} to {1}", GetCleanDisplayName(GUIGraphicsContext.currentScreen), GetCleanDisplayName(screen));
      if (screen.Bounds != GUIGraphicsContext.currentScreen.Bounds)
      {
        Log.Info("Main: OnDisplayChange Bounds of display changed from {0}x{1} to {2}x{3}", currentBounds.Width, currentBounds.Height, newBounds.Width, newBounds.Height);
      }
      if (!Equals(currentBounds.Size, newBounds.Size))
      {
        // Check if start screen is equal to device screen and check if current screen bond differ from current detected screen bond then recreate swap chain.
        Log.Debug("Main: Screen MP OnDisplayChange current screen detected                                {0}", GetCleanDisplayName(screen));
        Log.Debug("Main: Screen MP OnDisplayChange current screen                                         {0}", GetCleanDisplayName(GUIGraphicsContext.currentScreen));
        Log.Debug("Main: Screen MP OnDisplayChange start screen                                           {0}", GetCleanDisplayName(GUIGraphicsContext.currentStartScreen));
        Log.Debug("Main: Screen MP OnDisplayChange change current screen {0} with current detected screen {1}", GetCleanDisplayName(GUIGraphicsContext.currentScreen), GetCleanDisplayName(screen));
        GUIGraphicsContext.currentScreen = screen;
        Log.Debug("Main: Screen MP OnDisplayChange set current screen bounds {0} to Bounds {1}",
          GUIGraphicsContext.currentScreen.Bounds, Bounds);
        Bounds = screen.Bounds;
        Log.Debug("Main: Screen MP OnDisplayChange recreate swap chain");
        RecreateSwapChain();
        _changeScreen = true;
      }
      // Restore original Start Screen in case of change from RDP Session
      if (!Equals(screen, GUIGraphicsContext.currentStartScreen))
      {
        foreach (GraphicsAdapterInfo adapterInfo in _enumerationSettings.AdapterInfoList)
        {
          var hMon = Manager.GetAdapterMonitor(adapterInfo.AdapterOrdinal);
          var info = new MonitorInformation();
          info.Size = (uint)Marshal.SizeOf(info);
          GetMonitorInfo(hMon, ref info);
          var rect = Screen.FromRectangle(info.MonitorRectangle).Bounds;
          if (Equals(Manager.Adapters[GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal].Information.DeviceName, GetCleanDisplayName(GUIGraphicsContext.currentStartScreen)) && rect.Equals(screen.Bounds))
          {
            GUIGraphicsContext.currentScreen = GUIGraphicsContext.currentStartScreen;
            break;
          }
          GUIGraphicsContext.currentScreen = screen;
        }
      }
    }

    if (!Windowed)
    {
      SetBounds(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y, GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
    }

    // needed to avoid cursor show when MP windows change (for ex when refesh rate is working)
    _moveMouseCursorPositionRefresh = D3D._lastCursorPosition;

    msg.Result = (IntPtr)1;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnGetMinMaxInfo(ref Message msg)
  {
    var mmi = (MINMAXINFO)Marshal.PtrToStructure(msg.LParam, typeof(MINMAXINFO));
    Log.Debug("Main: WM_GETMINMAXINFO Start (MaxSize: {0}x{1} - MaxPostion: {2},{3} - MinTrackSize: {4}x{5} - MaxTrackSize: {6}x{7})",
              mmi.ptMaxSize.x, mmi.ptMaxSize.y, mmi.ptMaxPosition.x, mmi.ptMaxPosition.y, mmi.ptMinTrackSize.x, mmi.ptMinTrackSize.y, mmi.ptMaxTrackSize.x, mmi.ptMaxTrackSize.y);

    // do not continue if form is not created yet
    if (!Created)
    {
      Log.Debug("Main: Form not created yet - ignoring message");
      return;
    }

    if (Windowed && Screen.PrimaryScreen.WorkingArea.Width == 0 && Screen.PrimaryScreen.WorkingArea.Height == 0)
    {
      Log.Debug("Main: Desktop is not visible - ignoring message");
      return;
    }

    // check if display changes in case no DISPLAYCHANGE message is send by Windows
    Screen screen = Screen.FromControl(this);
    Rectangle currentBounds = GUIGraphicsContext.currentScreen.Bounds;
    Rectangle newBounds = screen.Bounds;
    string adapterOrdinalScreenName = Manager.Adapters[GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal].Information.DeviceName;

    if ((!Equals(screen, GUIGraphicsContext.currentScreen) || (!Equals(GetCleanDisplayName(GUIGraphicsContext.currentScreen), GetCleanDisplayName(screen)))) && !_firstLoadedScreen)
    {
      Log.Info("Main: Screen MP OnGetMinMaxInfo is displayed on changed from {0} to {1}", GetCleanDisplayName(GUIGraphicsContext.currentScreen), GetCleanDisplayName(screen));
      if (screen.Bounds != GUIGraphicsContext.currentScreen.Bounds)
      {
        Log.Info("Main: OnGetMinMaxInfo Bounds of display changed from {0}x{1} @ {2},{3} to {4}x{5} @ {6},{7}",
                 currentBounds.Width, currentBounds.Height, currentBounds.X, currentBounds.Y, newBounds.Width, newBounds.Height, newBounds.X, newBounds.Y);
      }
      _changeScreen = true;
    }

    if (!Equals(currentBounds.Size, newBounds.Size) && !_firstLoadedScreen)
    {
      // Check if start screen is equal to device screen and check if current screen bond differ from current detected screen bond then recreate swap chain.
      Log.Debug("Main: Screen MP OnGetMinMaxInfo Information.DeviceName Manager.Adapters                {0}", adapterOrdinalScreenName);
      Log.Debug("Main: Screen MP OnGetMinMaxInfo current screen detected                                {0}", GetCleanDisplayName(screen));
      Log.Debug("Main: Screen MP OnGetMinMaxInfo current screen                                         {0}", GetCleanDisplayName(GUIGraphicsContext.currentScreen));
      Log.Debug("Main: Screen MP OnGetMinMaxInfo start screen                                           {0}", GetCleanDisplayName(GUIGraphicsContext.currentStartScreen));
      Log.Debug("Main: Screen MP OnGetMinMaxInfo change current screen {0} with current detected screen {1}", GetCleanDisplayName(GUIGraphicsContext.currentScreen), GetCleanDisplayName(screen));
      GUIGraphicsContext.currentScreen = screen;
      Log.Debug("Main: Screen MP OnGetMinMaxInfo set current screen bounds {0} to Bounds {1}", GUIGraphicsContext.currentScreen.Bounds, Bounds);
      Bounds = screen.Bounds;
      Log.Debug("Main: Screen MP OnGetMinMaxInfo recreate swap chain");
      RecreateSwapChain();
      _changeScreen = true;

      if (!Windowed)
      {
        SetBounds(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y, GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height);
      }
    }

    if (_changeScreen)
    {
      Log.Debug("Main: Screen MP OnGetMinMaxInfo (changeScreen) change current screen {0} with current detected screen {1}", GetCleanDisplayName(GUIGraphicsContext.currentScreen), GetCleanDisplayName(screen));
      GUIGraphicsContext.currentScreen = screen;
      _changeScreen = false;
    }

    // calculate form dimension limits based on primary screen.
    if (Windowed)
    {
      double ratio         = Math.Min((double)Screen.PrimaryScreen.WorkingArea.Width / Width, (double)Screen.PrimaryScreen.WorkingArea.Height / Height);
      mmi.ptMaxSize.x      = (int)(Width * ratio);
      mmi.ptMaxSize.y      = (int)(Height * ratio);
      mmi.ptMaxPosition.x  = Screen.PrimaryScreen.WorkingArea.Left;
      mmi.ptMaxPosition.y  = Screen.PrimaryScreen.WorkingArea.Top;
      mmi.ptMinTrackSize.x = GUIGraphicsContext.SkinSize.Width / 3;
      mmi.ptMinTrackSize.y = GUIGraphicsContext.SkinSize.Height / 3;
      mmi.ptMaxTrackSize.x = GUIGraphicsContext.currentScreen.WorkingArea.Right - GUIGraphicsContext.currentScreen.WorkingArea.Left;
      mmi.ptMaxTrackSize.y = GUIGraphicsContext.currentScreen.WorkingArea.Bottom - GUIGraphicsContext.currentScreen.WorkingArea.Top;
      Marshal.StructureToPtr(mmi, msg.LParam, true);
      msg.Result = (IntPtr)0;
    }
    else
    {
      mmi.ptMaxSize.x      = Screen.PrimaryScreen.Bounds.Width;
      mmi.ptMaxSize.y      = Screen.PrimaryScreen.Bounds.Height;
      mmi.ptMaxPosition.x  = Screen.PrimaryScreen.Bounds.X;
      mmi.ptMaxPosition.y  = Screen.PrimaryScreen.Bounds.Y;
      mmi.ptMinTrackSize.x = GUIGraphicsContext.currentScreen.Bounds.Width;
      mmi.ptMinTrackSize.y = GUIGraphicsContext.currentScreen.Bounds.Height;
      mmi.ptMaxTrackSize.x = GUIGraphicsContext.currentScreen.Bounds.Width;
      mmi.ptMaxTrackSize.y = GUIGraphicsContext.currentScreen.Bounds.Height;
      Marshal.StructureToPtr(mmi, msg.LParam, true);
      msg.Result = (IntPtr)0;

      // force form dimensions to screen size to compensate for HDMI hot plug problems (e.g. WM_DiSPLAYCHANGE reported 1920x1080 but system is still in 1024x768 mode).
      //Bounds = GUIGraphicsContext.currentScreen.Bounds;
    }
    Log.Debug("Main: WM_GETMINMAXINFO End (MaxSize: {0}x{1} - MaxPostion: {2},{3} - MinTrackSize: {4}x{5} - MaxTrackSize: {6}x{7})",
          mmi.ptMaxSize.x, mmi.ptMaxSize.y, mmi.ptMaxPosition.x, mmi.ptMaxPosition.y, mmi.ptMinTrackSize.x, mmi.ptMinTrackSize.y, mmi.ptMaxTrackSize.x, mmi.ptMaxTrackSize.y);
    // needed to avoid cursor show when MP windows change (for ex when refesh rate is working)
    _moveMouseCursorPositionRefresh = D3D._lastCursorPosition;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnSizing(ref Message msg)
  {
    Log.Debug("Main: WM_SIZING");
    var rc           = (RECT) Marshal.PtrToStructure(msg.LParam, typeof (RECT));
    var border       = new Size(Width - ClientSize.Width, Height - ClientSize.Height);
    int width        = rc.right - rc.left - border.Width;
    int height       = rc.bottom - rc.top - border.Height;
    long gcd         = GCD(GUIGraphicsContext.SkinSize.Width, GUIGraphicsContext.SkinSize.Height);
    double ratioX    = (double) GUIGraphicsContext.SkinSize.Width/gcd;
    double ratioY    = (double) GUIGraphicsContext.SkinSize.Height/gcd;

    switch (msg.WParam.ToInt32())
    {
      // adjust height by overriding bottom
      case WMSZ_LEFT:
      case WMSZ_RIGHT:
      case WMSZ_BOTTOMRIGHT:
        rc.bottom = rc.top + border.Height + (int)(ratioY * width / ratioX);
        break;
      // adjust width by overriding right
      case WMSZ_TOP:
      case WMSZ_BOTTOM:
        rc.right = rc.left + border.Width + (int)(ratioX * height / ratioY);
        break;
      // adjust width by overriding left
      case WMSZ_TOPLEFT:
      case WMSZ_BOTTOMLEFT:
        rc.left = rc.right - border.Width - (int)(ratioX * height / ratioY);
        break;
      // adjust height by overriding top
      case WMSZ_TOPRIGHT:
        rc.top = rc.bottom - border.Height - (int)(ratioY * width / ratioX);
        break;
    }

    Size maxClientSize = CalcMaxClientArea();
    if (rc.right - rc.left - border.Width > maxClientSize.Width)
    {
      Log.Debug("Main: Cannot resize beyond maximum aspect ratio safe size (Reqested Size:{0}x{1})", rc.right - rc.left, rc.bottom, rc.top);
      rc = LastRect;
    }

    // only redraw if rectangle size changed
    if (((rc.right - rc.left) != (LastRect.right - LastRect.left)) || ((rc.bottom - rc.top) != (LastRect.bottom - LastRect.top)))
    {
      Log.Info("Main: Aspect ratio safe resizing from {0}x{1} to {2}x{3} (new Client Size {4}x{5})",
               LastRect.right - LastRect.left, LastRect.bottom - LastRect.top,
               rc.right - rc.left, rc.bottom - rc.top,
               rc.right - rc.left - border.Width, rc.bottom - rc.top - border.Height);
      OnPaintEvent();
    }

    // snapback cursor to window border if needed
    Point pos = Cursor.Position;
    if (pos.X > rc.right)
    {
      pos.X = rc.right;
    }
    else if (pos.X < rc.left)
    {
      pos.X = rc.left;
    }

    if (pos.Y > rc.bottom)
    {
      pos.Y = rc.bottom;
    }
    else if (pos.Y < rc.top)
    {
      pos.Y = rc.top;
    }
    Cursor.Position = pos;

    Marshal.StructureToPtr(rc, msg.LParam, false);
    LastRect = rc;
    msg.Result = (IntPtr)1;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnSize(ref Message msg)
  {
    int x = unchecked((short)msg.LParam);
    int y = unchecked((short)((uint)msg.LParam >> 16));
    switch (msg.WParam.ToInt32())
    {
      case SIZE_RESTORED:
        Log.Debug("Main: WM_SIZE (SIZE_RESTORED: {0}x{1})", x, y);
  
        // do not continue if form is not created yet
        if (!Created)
        {
          Log.Debug("Main: Form not created yet - ignoring message");
          return;
        }

        if (Windowed && Screen.PrimaryScreen.WorkingArea.Width == 0 && Screen.PrimaryScreen.WorkingArea.Height == 0)
        {
          Log.Debug("Main: Desktop is not visible - ignoring message");
          return;
        }

        if (Windowed)
        {
          Size maxClientSize = CalcMaxClientArea();
          if (x > maxClientSize.Width || y > maxClientSize.Height)
          {
            Log.Debug("Main: Requested client size {0}x{1} is larger than the maximum aspect ratio safe client size of {2}x{3} - overriding", 
              x, y, maxClientSize.Width, maxClientSize.Height);
            ClientSize = maxClientSize;
            break;
          }

          var border = new Size(Width - ClientSize.Width, Height - ClientSize.Height);
          var height = (int)((double)x * GUIGraphicsContext.SkinSize.Height / GUIGraphicsContext.SkinSize.Width);
          if (height != y && Windowed)
          {
            Log.Info("Main: Overriding size from {0}x{1} to {2}x{3} (Skin resized to {4}x{5})", 
              x + border.Width, y + border.Height, x + border.Width, height + border.Height, x, height);
            ClientSize = new Size(x, height);
          }
          // send new resolution to VisualizationWindow so the Winproc can work with it 
          if (VisualizationBase.VisualizationWindow != null)
          {
            VisualizationBase.VisualizationWindow.Height = ClientRectangle.Width;
            VisualizationBase.VisualizationWindow.Width = ClientRectangle.Height;
          }
        }
        else
        {
          // force form dimensions to screen size to compensate for HDMI hot plug problems (e.g. WM_DiSPLAYCHANGE reported 1920x1080 but system is still in 1024x768 mode).
          if (Bounds != GUIGraphicsContext.currentScreen.Bounds)
          {
            Log.Debug("Main: Setting full screen bonds to: {0}x{1} @ {2},{3}",
                      GUIGraphicsContext.currentScreen.Bounds.Width, GUIGraphicsContext.currentScreen.Bounds.Height, GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y);
            Bounds = GUIGraphicsContext.currentScreen.Bounds;
          }
        }
        break;

      case SIZE_MINIMIZED:
        Log.Debug("Main: WM_SIZE (SIZE_MINIMIZED: {0}x{1})", x, y);
        break;
      
      case SIZE_MAXIMIZED:
        Log.Debug("Main: WM_SIZE (SIZE_MAXIMIZED: {0}x{1})", x, y);
        if (VisualizationBase.VisualizationWindow != null)
        {
          VisualizationBase.VisualizationWindow.Height = ClientRectangle.Width;
          VisualizationBase.VisualizationWindow.Width = ClientRectangle.Height;
        }
        break;
      
      case SIZE_MAXSHOW:
        Log.Debug("Main: WM_SIZE (SIZE_MAXSHOW: {0}x{1})", x, y);
        break;
      
      case SIZE_MAXHIDE:
        Log.Debug("Main: WM_SIZE (SIZE_MAXHIDE: {0}x{1})", x, y);
        break;
    }
    msg.Result = (IntPtr)0;
  }

  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="msg"></param>
  private void OnMoving(ref Message msg)
  {
    var rc = (RECT)Marshal.PtrToStructure(msg.LParam, typeof(RECT));
    Log.Debug("Main: WM_MOVING (TopLeft: {0},{1} - BottomRight: {2},{3})", rc.left, rc.top, rc.right, rc.bottom);
    msg.Result = (IntPtr)1;
  }


  /// <summary>
  /// 
  /// </summary>
  private static void ReOpenDBs()
  {
    string dbPath = FolderSettings.DatabaseName;
    if (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: reopen FolderDatabase3 sqllite database.");
      FolderSettings.ReOpen();
    }
    dbPath = MediaPortal.Picture.Database.PictureDatabase.DatabaseName;
    if (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: reopen PictureDatabase sqllite database.");
      MediaPortal.Picture.Database.PictureDatabase.ReOpen();
    }

    dbPath = MediaPortal.Video.Database.VideoDatabase.DatabaseName;
    if (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: reopen VideoDatabaseV5.db3 sqllite database.");
      MediaPortal.Video.Database.VideoDatabase.ReOpen();
    }
    else
    {
      Log.Info("Main: VideoDatabaseV5.db3 sqllite database disk cache activated.");
      MediaPortal.Video.Database.VideoDatabase.RevertFlushTransactionsToDisk();
    }

    dbPath = MediaPortal.Music.Database.MusicDatabase.Instance.DatabaseName;
    if (string.IsNullOrEmpty(dbPath) || PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: reopen MusicDatabase.db3 sqllite database.");
      MediaPortal.Music.Database.MusicDatabase.ReOpen();
    }
  }


  /// <summary>
  /// 
  /// </summary>
  private static void DisposeDBs()
  {
    string dbPath = FolderSettings.DatabaseName;
    if (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: disposing FolderDatabase3 sqllite database.");
      FolderSettings.Dispose();
    }

    dbPath = MediaPortal.Picture.Database.PictureDatabase.DatabaseName;
    if (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: disposing PictureDatabase sqllite database.");
      MediaPortal.Picture.Database.PictureDatabase.Dispose();
    }

    dbPath = MediaPortal.Video.Database.VideoDatabase.DatabaseName;
    if (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: disposing VideoDatabaseV5.db3 sqllite database.");
      MediaPortal.Video.Database.VideoDatabase.Dispose();
    }
    else
    {
      Log.Info("Main: VideoDatabaseV5.db3 sqllite database cache flushed to disk.");
      MediaPortal.Video.Database.VideoDatabase.FlushTransactionsToDisk();
    }

    dbPath = MediaPortal.Music.Database.MusicDatabase.Instance.DatabaseName;
    if (!string.IsNullOrEmpty(dbPath) && PathIsNetworkPath(dbPath))
    {
      Log.Info("Main: disposing MusicDatabase db3 sqllite database.");
      MediaPortal.Music.Database.MusicDatabase.Dispose();
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  private static bool Currentmodulefullscreen()
  {
    bool currentmodulefullscreen = (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                                    GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC ||
                                    GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
                                    GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT);
    return currentmodulefullscreen;
  }

  /// <summary>
  /// 
  /// </summary>  
  private void OnSuspend()
  {
    _ignoreContextMenuAction = true;
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.SUSPENDING; // this will close all open dialogs      

    // stop playback
    Log.Debug("Main: OnSuspend - stopping playback");
    if (GUIGraphicsContext.IsPlaying)
    {
      Currentmodulefullscreen();
      g_Player.Stop();
      while (GUIGraphicsContext.IsPlaying)
      {
        Thread.Sleep(100);
      }
    }
    SaveLastActiveModule();

    Log.Debug("Main: OnSuspend - stopping input devices");
    InputDevices.Stop();

    Log.Debug("Main: OnSuspend - stopping AutoPlay");
    AutoPlay.StopListening();
      
    // un-mute volume in case we are suspending in away mode
    if (IsInAwayMode && VolumeHandler.Instance.IsMuted)
    {
      Log.Debug("Main: OnSuspend - unmute volume");
      VolumeHandler.Instance.UnMute();
    }
    VolumeHandler.Dispose();

    // we only dispose the DB connection if the DB path is remote.      
    Log.Debug("Main: OnSuspend - dispose DB connection");
    DisposeDBs();

    _suspended = true;
    Log.Info("Main: OnSuspend - Done");
  }

  /// <summary>
  /// This event is delivered every time the system resumes and does not indicate whether a user is present
  /// </summary>
  private void OnResumeAutomatic()
  {
    // delay resuming as configured
    using (Settings xmlreader = new MPSettings())
    {
      int waitOnResume = xmlreader.GetValueAsBool("general", "delay resume", false) ? xmlreader.GetValueAsInt("general", "delay", 0) : 0;
      if (waitOnResume > 0)
      {
        Log.Info("Main: OnResumeAutomatic - waiting on resume {0} secs", waitOnResume);
        Thread.Sleep(waitOnResume * 1000);
      }
    }

    Log.Debug("Main: OnResumeAutomatic - reopen Database");
    ReOpenDBs();

    Log.Info("Main: OnResumeAutomatic - Done");
  }

  /// <summary>
  /// This event is sent after the  PBT_APMRESUMEAUTOMATIC event if the system has resumed operation due to user activity.
  /// </summary>
  private void OnResumeSuspend()
  {
    // avoid screen saver after standby
    GUIGraphicsContext.ResetLastActivity();
    _ignoreContextMenuAction = false;

    // Systems without DirectX9Ex have lost graphics device in suspend/hibernate cycle
    if (!GUIGraphicsContext.IsDirectX9ExUsed())
    {
      Log.Debug("Main: OnResumeSuspend - set GUIGraphicsContext.State.LOST");
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
    }
    RecoverDevice();

    if (GUIGraphicsContext.IsDirectX9ExUsed())
    {
      Log.Debug("Main: OnResumeSuspend - set GUIGraphicsContext.State.RUNNING");
      GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;
    }

    Log.Debug("Main: OnResumeSuspend - Init Input Devices");
    InputDevices.Init();

    Log.Debug("Main: OnResumeSuspend - Autoplay start listening");
    AutoPlay.StartListening();

    Log.Debug("Main: OnResumeSuspend - Initializing volume handler");
    try
    {
#pragma warning disable 168
      VolumeHandler vh = VolumeHandler.Instance;
#pragma warning restore 168
    }
    catch (Exception exception)
    {
      Log.Warn("Main: OnResumeSuspend - Could not initialize volume handler: ", exception.Message);
    }

    _suspended = false;
    _lastOnresume = DateTime.Now;
    Log.Info("Main: OnResumeSuspend - Done");
  }

  #endregion

  #region process

  /// <summary>
  /// Process() gets called when a dialog is presented.
  /// It contains the message loop 
  /// </summary>
  public void MPProcess()
  {
    if (!_suspended && AppActive)
    {
      try
      {
        g_Player.Process();
        HandleMessage();
        FrameMove();
        FullRender();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }
  }

  #endregion

  #region RenderFrame()

  /// <summary>
  /// 
  /// </summary>
  /// <param name="timePassed"></param>
  public void RenderFrame(float timePassed)
  {
    if (!_suspended && AppActive)
    {
      try
      {
        CreateStateBlock();
        GUILayerManager.Render(timePassed);
        RenderStats();
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        Log.Error("RenderFrame exception {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      }
    }
  }

  #endregion

  #region Onstartup() / OnExit()

  /// <summary>
  /// OnStartup() gets called just before the application starts
  /// </summary>
  protected override void OnStartup()
  {
    Log.Info("Main: Starting up");

  // Register shell notifications to wndproc
    Notifications.RegisterChangeNotify(this.Handle, ShellNotifications.CSIDL.CSIDL_DESKTOP, true);

    // Initializing input devices...
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(63));
    Log.Info("Main: Initializing Input Devices");
    InputDevices.Init();

    // Starting plugins...
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(64));
    PluginManager.LoadProcessPlugins();
    PluginManager.StartProcessPlugins();

    using (Settings xmlreader = new MPSettings())
    {
      _dateFormat = xmlreader.GetValueAsString("home", "dateformat", "<Day> <Month> <DD>");
    }

    // Asynchronously pre-initialize the music engine if we're using the BassMusicPlayer
    if (BassMusicPlayer.IsDefaultMusicPlayer)
    {
      BassMusicPlayer.CreatePlayerAsync();
    }
    try
    {
      GUIPropertyManager.SetProperty("#date", GetDate());
      GUIPropertyManager.SetProperty("#time", GetTime());
      GUIPropertyManager.SetProperty("#Day", GetDay()); // 01
      GUIPropertyManager.SetProperty("#SDOW", GetShortDayOfWeek()); // Sun
      GUIPropertyManager.SetProperty("#DOW", GetDayOfWeek()); // Sunday
      GUIPropertyManager.SetProperty("#Month", GetMonth()); // 01
      GUIPropertyManager.SetProperty("#SMOY", GetShortMonthOfYear()); // Jan
      GUIPropertyManager.SetProperty("#MOY", GetMonthOfYear()); // January
      GUIPropertyManager.SetProperty("#SY", GetShortYear()); // 80
      GUIPropertyManager.SetProperty("#Year", GetYear()); // 1980

      // TODO: remove internal screen saver, there is no need for it anymore as MP bugs have been fixed
      // disable screen saver when MP running and internal selected
      if (_useScreenSaver)
      {
        SystemParametersInfo(SPI_GETSCREENSAVEACTIVE, 0, ref _isWinScreenSaverInUse, 0);
        if (_isWinScreenSaverInUse)
        {
          SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 0, 0, SPIF_SENDCHANGE);
        }
      }

      GlobalServiceProvider.Add<IVideoThumbBlacklist>(new MediaPortal.Video.Database.VideoThumbBlacklistDBImpl());
      Utils.CheckThumbExtractorVersion();
    }
    catch (Exception ex)
    {
      Log.Error("MediaPortalApp: Error setting date and time properties - {0}", ex.Message);
    }

    if (_outdatedSkinName != null || PluginManager.IncompatiblePluginAssemblies.Count > 0 || PluginManager.IncompatiblePlugins.Count > 0)
    {
      GUIWindowManager.SendThreadCallback(ShowStartupWarningDialogs, 0, 0, null);
    }
    Log.Debug("Main: Auto play start listening");
    AutoPlay.StartListening();

    Log.Info("Main: Initializing volume handler");
    #pragma warning disable 168
    VolumeHandler vh = VolumeHandler.Instance;
    #pragma warning restore 168

    // register for device change notifications
    RegisterForDeviceNotifications();

    // register for power settings notifications
    Log.Debug("Main: Register for Power Settings Notifications");
    if (OSInfo.OSInfo.Win8OrLater())
    {
      _displayStatusHandle = RegisterPowerSettingNotification(Handle, ref GUID_SESSION_DISPLAY_STATUS, DEVICE_NOTIFY_WINDOW_HANDLE);
      if (_displayStatusHandle == IntPtr.Zero)
      {
        Log.Warn("Main: Could not register for power settings notification GUID_SESSION_DISPLAY_STATUS");
      }

      _userPresenceHandle = RegisterPowerSettingNotification(Handle, ref GUID_SESSION_USER_PRESENCE, DEVICE_NOTIFY_WINDOW_HANDLE);
      if (_userPresenceHandle == IntPtr.Zero)
      {
        Log.Warn("Main: Could not register for power settings notification GUID_SESSION_USER_PRESENCE");
      }
    } 
    else if (OSInfo.OSInfo.VistaOrLater())
    {
      _displayStatusHandle = RegisterPowerSettingNotification(Handle, ref GUID_MONITOR_POWER_ON, DEVICE_NOTIFY_WINDOW_HANDLE);
      if (_displayStatusHandle == IntPtr.Zero)
      {
        Log.Warn("Main: Could not register for power settings notification GUID_MONITOR_POWER_ON");
      }
    }

    if (OSInfo.OSInfo.VistaOrLater())
    {
      _awayModeHandle = RegisterPowerSettingNotification(Handle, ref GUID_SYSTEM_AWAYMODE, DEVICE_NOTIFY_WINDOW_HANDLE);
      if (_awayModeHandle == IntPtr.Zero)
      {
        Log.Warn("Main: Could not register for power settings notification GUID_SYSTEM_AWAYMODE");
      }
    }
  }

  private void RegisterForDeviceNotifications()
  {
    Log.Debug("Main: Registering for Device Notifications");
    var devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
    int size = Marshal.SizeOf(devBroadcastDeviceInterface);
    devBroadcastDeviceInterface.dbcc_size = size;
    devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
    IntPtr devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);
    Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);
    _deviceNotificationHandle = RegisterDeviceNotification(Handle, devBroadcastDeviceInterfaceBuffer,
                                                           DEVICE_NOTIFY_WINDOW_HANDLE |
                                                           DEVICE_NOTIFY_ALL_INTERFACE_CLASSES);
    if (_deviceNotificationHandle == IntPtr.Zero)
    {
      Log.Warn("Main: Could not register for device notifications");
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="param1"></param>
  /// <param name="param2"></param>
  /// <param name="data"></param>
  /// <returns></returns>
  private int ShowStartupWarningDialogs(int param1, int param2, object data)
  {
    // If skin is outdated it may not have a skin file for this dialog but user may choose to use it anyway
    // So show incompatible plugins dialog first (possibly using default skin)
    if (PluginManager.IncompatiblePluginAssemblies.Count > 0 || PluginManager.IncompatiblePlugins.Count > 0)
    {
      var dlg = (GUIDialogIncompatiblePlugins)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_INCOMPATIBLE_PLUGINS);
      dlg.DoModal(GUIWindowManager.ActiveWindow);
    }

    if (_outdatedSkinName != null)
    {
      var dlg = (GUIDialogOldSkin)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OLD_SKIN);
      dlg.UserSkin = _outdatedSkinName;
      dlg.DoModal(GUIWindowManager.ActiveWindow);
    }

    return 0;
  }


  /// <summary>
  /// Load string_xx.xml based on config
  /// </summary>
  private static void LoadLanguageString()
  {
    string mylang;
    try
    {
      using (Settings xmlreader = new MPSettings())
      {
        mylang = xmlreader.GetValueAsString("gui", "language", "English");
      }
    }
    catch
    {
      Log.Warn("Load language file failed, fall back to \"English\"");
      mylang = "English";
    }
    Log.Info("Loading selected language: " + mylang);

    try
    {
      GUILocalizeStrings.Load(mylang);
    }
    catch (Exception ex)
    {
      // ReSharper disable LocalizableElement
      MessageBox.Show(String.Format("Failed to load your language! Aborting startup...\n\n{0}\nstack:{1}", ex.Message, ex.StackTrace),
        "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
      // ReSharper restore LocalizableElement
      Application.Exit();
    }
  }


  /// <summary>
  /// saves last active module.
  /// </summary>
  private void SaveLastActiveModule()
  {
    // persist the currently selected module to XML for later use.
    Log.Debug("Main: SaveLastActiveModule - enabled {0}", _showLastActiveModule);
    bool currentmodulefullscreen = Currentmodulefullscreen();
    string currentmodulefullscreenstate = GUIPropertyManager.GetProperty("#currentmodulefullscreenstate");
    string currentmoduleid = GUIPropertyManager.GetProperty("#currentmoduleid");

    if (_showLastActiveModule && !Utils.IsGUISettingsWindow(GUIWindowManager.GetPreviousActiveWindow()))
    {
      using (Settings xmlreader = new MPSettings())
      {
        if (currentmodulefullscreen)
        {
          currentmoduleid = Convert.ToString(GUIWindowManager.GetPreviousActiveWindow());
        }
        
        if (!currentmodulefullscreen && currentmodulefullscreenstate == "True")
        {
          currentmodulefullscreen = true;
        }

        if (currentmoduleid.Length == 0)
        {
          currentmoduleid = "0";
        }

        string section;
        switch (GUIWindowManager.ActiveWindow)
        {
          case (int)GUIWindow.Window.WINDOW_PICTURES:
            section = "pictures";
            break;

          case (int)GUIWindow.Window.WINDOW_MUSIC:
            section = "music";
             break;

          case (int)GUIWindow.Window.WINDOW_VIDEOS:
            section = "movies";
            break;

          default:
            section = "";
            break;
        }

        bool rememberLastFolder = xmlreader.GetValueAsBool(section, "rememberlastfolder", false);
        string lastFolder = xmlreader.GetValueAsString(section, "lastfolder", "");

        var virtualDir = new VirtualDirectory();
        virtualDir.LoadSettings(section);

        int pincode;
        bool lastFolderPinProtected = virtualDir.IsProtectedShare(lastFolder, out pincode);
        if (rememberLastFolder && lastFolderPinProtected)
        {
          lastFolder = "root";
          xmlreader.SetValue(section, "lastfolder", lastFolder);
          Log.Debug("Main: reverting to root folder, pin protected folder was open, SaveLastFolder {0}", lastFolder);
        }

        xmlreader.SetValue("general", "lastactivemodule", currentmoduleid);
        xmlreader.SetValueAsBool("general", "lastactivemodulefullscreen", currentmodulefullscreen);
        Log.Debug("Main: SaveLastActiveModule - module {0}", currentmoduleid);
        Log.Debug("Main: SaveLastActiveModule - fullscreen {0}", currentmodulefullscreen);
      }
    }
  }


  /// <summary>
  /// OnExit() Gets called just b4 application stops
  /// </summary>
  protected override void OnExit()
  {
    SaveLastActiveModule();

    Log.Info("Main: Exiting");

    if (_usbuirtdevice != null)
    {
      _usbuirtdevice.Close();
    }

    if (_serialuirdevice != null)
    {
      _serialuirdevice.Close();
    }

    if (_redeyedevice != null)
    {
      _redeyedevice.Close();
    }

    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;

    g_Player.Stop();
    InputDevices.Stop();
    AutoPlay.StopListening();
    PluginManager.Stop();
    GUIWaitCursor.Dispose();
    GUIFontManager.ReleaseUnmanagedResources();
    GUIFontManager.Dispose();
    GUITextureManager.Dispose();
    GUIWindowManager.Clear();
    GUILocalizeStrings.Dispose();
    TexturePacker.Cleanup();
    VolumeHandler.Dispose();

    if (_isWinScreenSaverInUse)
    {
      SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 1, 0, SPIF_SENDCHANGE);
    }
    UnregisterForDeviceNotification();
    UnregisterForPowerSettingNotitication();
    Notifications.UnregisterChangeNotify();
  }


  /// <summary>
  /// 
  /// </summary>
  private void UnregisterForPowerSettingNotitication()
  {
    if (_displayStatusHandle != IntPtr.Zero)
    {
      UnregisterPowerSettingNotification(_displayStatusHandle);
      _displayStatusHandle = IntPtr.Zero;
    }

    if (_userPresenceHandle != IntPtr.Zero)
    {
      UnregisterPowerSettingNotification(_userPresenceHandle);
      _userPresenceHandle = IntPtr.Zero;
    }

    if (_awayModeHandle != IntPtr.Zero)
    {
      UnregisterPowerSettingNotification(_awayModeHandle);
      _awayModeHandle = IntPtr.Zero;
    }
  }


  /// <summary>
  /// 
  /// </summary>
  private void UnregisterForDeviceNotification()
  {
    if (_deviceNotificationHandle != IntPtr.Zero)
    {
      UnregisterDeviceNotification(_deviceNotificationHandle);
      _deviceNotificationHandle = IntPtr.Zero;
    }
  }


  /// <summary>
  /// 
  /// </summary>
  protected override void InitializeDeviceObjects()
  {
    Log.Debug("Main: InitializeDeviceObjects()");
    GUIWindowManager.Clear();
    GUIWaitCursor.Dispose();
    GUITextureManager.Dispose();
 
    // Loading keymap.xml
    Log.Info("Startup: Load keymap.xml");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(65));
    ActionTranslator.Load();
    GUIGraphicsContext.ActiveForm = Handle;

    // Caching Graphics
    Log.Info("Startup: Caching Graphics");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(67));
    try
    {
      GUITextureManager.Init();
    }
    catch (Exception exs)
    {
      // ReSharper disable LocalizableElement
      MessageBox.Show(String.Format("Failed to load your skin! Aborting startup...\n\n{0}", exs.Message), "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
      // ReSharper restore LocalizableElement
      Close();
    }
    Utils.FileExistsInCache(Config.GetSubFolder(Config.Dir.Skin, "") + "dummy.png");
    Utils.FileExistsInCache(Config.GetSubFolder(Config.Dir.Thumbs, "") + "dummy.png");
    Utils.FileExistsInCache(Thumbs.Videos + "\\dummy.png");
    Utils.FileExistsInCache(Thumbs.MusicFolder + "\\dummy.png");

    // Resize Form
    if (Windowed)
    {
      Log.Debug("Startup: Resizing form to Skin Dimensions");
      ClientSize      = CalcMaxClientArea();
      LastRect.top    = Location.Y;
      LastRect.left   = Location.X;
      LastRect.bottom = Size.Height;
      LastRect.right  = Size.Width;
      Location = new Point(GUIGraphicsContext.currentScreen.Bounds.X, GUIGraphicsContext.currentScreen.Bounds.Y);
    }
    else
    {
      Log.Debug("Startup: Resizing form to Screen Dimensions");
      ClientSize = new Size(GUIGraphicsContext.currentScreen.Bounds.Right, GUIGraphicsContext.currentScreen.Bounds.Bottom);
    }

    // Loading Fonts
    Log.Info("Startup: Loading Fonts");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(68));
    GUIFontManager.LoadFonts(GUIGraphicsContext.GetThemedSkinFile(@"\fonts.xml"));
    GUIFontManager.InitializeDeviceObjects();

    // Loading window plugins
    Log.Info("Startup: Loading and Starting Window Plugins");
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(70));
    if (!string.IsNullOrEmpty(_safePluginsList))
    {
      PluginManager.LoadWhiteList(_safePluginsList);
    }
    PluginManager.LoadWindowPlugins();
    PluginManager.CheckExternalPlayersCompatibility();
    
    // Initialize window manager
    UpdateSplashScreenMessage(GUILocalizeStrings.Get(71));
    Log.Info("Startup: Initialize Window Manager...");
    GUIGraphicsContext.Load();
    GUIWindowManager.Initialize();

    using (Settings xmlreader = new MPSettings())
    {
      _useLongDateFormat = xmlreader.GetValueAsBool("home", "LongTimeFormat", false);
      _startWithBasicHome = xmlreader.GetValueAsBool("gui", "startbasichome", false);
      _useOnlyOneHome = xmlreader.GetValueAsBool("gui", "useonlyonehome", false);
    }

    Log.Info("Startup: Starting Window Manager");
    GUIWindowManager.PreInit();
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.RUNNING;

    Log.Info("Startup: Activating Window Manager");
    if ((_startWithBasicHome) && (File.Exists(GUIGraphicsContext.GetThemedSkinFile(@"\basichome.xml"))))
    {
      GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_SECOND_HOME);
    }
    else
    {
      GUIWindowManager.ActivateWindow(GUIWindowManager.ActiveWindow);
    }
   
    // setting D3D9 helper variables
    if (GUIGraphicsContext.DX9Device != null)
    {
      _anisotropy = GUIGraphicsContext.DX9Device.DeviceCaps.MaxAnisotropy;
      _supportsFiltering = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
                                                     GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
                                                     GUIGraphicsContext.DX9Device.DisplayMode.Format,
                                                     Usage.RenderTarget | Usage.QueryFilter, ResourceType.Textures,
                                                     Format.A8R8G8B8);
      _supportsAlphaBlend = Manager.CheckDeviceFormat(GUIGraphicsContext.DX9Device.DeviceCaps.AdapterOrdinal,
                                                      GUIGraphicsContext.DX9Device.DeviceCaps.DeviceType,
                                                      GUIGraphicsContext.DX9Device.DisplayMode.Format,
                                                      Usage.RenderTarget | Usage.QueryPostPixelShaderBlending,
                                                      ResourceType.Surface,
                                                      Format.A8R8G8B8);
      Log.Info("Main: Video memory left: {0} MB", (uint)GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1048576);
    }

    // ReSharper disable ObjectCreationAsStatement
    new GUILayerRenderer();
    // ReSharper restore ObjectCreationAsStatement

    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
  }


  /// <summary>
  /// Updates the splash screen to display the given string. 
  /// This method checks whether the splash screen exists.
  /// </summary>
  /// <param name="aSplashLine"></param>
  private static void UpdateSplashScreenMessage(string aSplashLine)
  {
    try
    {
      if (SplashScreen != null)
      {
        SplashScreen.SetInformation(aSplashLine);
      }
    }
    catch (Exception ex)
    {
      Log.Error("Main: Could not update splashscreen - {0}", ex.Message);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  protected override void OnDeviceLost(object sender, EventArgs e)
  {
    Log.Warn("Main: OnDeviceLost()");
    if (!Created)
    {
      Log.Debug("Main: Form not created yet - ignoring Event");
      return;
    }
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
    RecoverDevice();
    base.OnDeviceLost(sender, e);
  }

  #endregion

  #region Render()

  /// <summary>
  /// 
  /// </summary>
  /// <param name="timePassed"></param>
  protected override void Render(float timePassed)
  {
    if (!_suspended && AppActive && !_isRendering && GUIGraphicsContext.CurrentState != GUIGraphicsContext.State.LOST &&
        GUIGraphicsContext.DX9Device != null)
    {
      if (GUIGraphicsContext.InVmr9Render)
      {
        Log.Error("Main: MediaPortal.Render() called while VMR9 render - {0} / {1}", GUIGraphicsContext.Vmr9Active,
                  GUIGraphicsContext.Vmr9FPS);
        return;
      }
      if (GUIGraphicsContext.Vmr9Active)
      {
        Log.Error("Main: MediaPortal.Render() called while VMR9 active");
        return;
      }

      // render frame
      try
      {
        _isRendering = true;
        Frames++;
        lock (GUIGraphicsContext.RenderModeSwitch)
        {
          if (GUIGraphicsContext.BlankScreen || GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.None ||
              GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySideTo2D ||
              GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottomTo2D)
          {
            // clear the surface
            GUIGraphicsContext.DX9Device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            GUIGraphicsContext.DX9Device.BeginScene();
            CreateStateBlock();
            GUIGraphicsContext.SetScalingResolution(0, 0, false);
            // ask the layer manager to render all layers
            GUILayerManager.Render(timePassed);
            RenderStats();
            GUIFontManager.Present();
            GUIGraphicsContext.DX9Device.EndScene();
            //d3ErrInvalidCallCounter = 0;
            try
            {
              // Show the frame on the primary surface.
              GUIGraphicsContext.DX9Device.Present(); //SLOW
            }
            catch (DeviceLostException ex)
            {
              Log.Error("Main: Device lost - {0}", ex.ToString());
              if (!RefreshRateChanger.RefreshRateChangePending)
              {
                g_Player.Stop();
              }
              GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
            }
          }
          else if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide ||
                   GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.TopAndBottom)
          {
            // 3D output either SBS or TAB

            Surface old = GUIGraphicsContext.DX9Device.GetRenderTarget(0);
            Surface backbuffer = GUIGraphicsContext.DX9Device.GetBackBuffer(0, 0, BackBufferType.Mono);

            // create texture/surface for preparation for 3D output if they don't exist

            if (GUIGraphicsContext.Auto3DTexture == null)
              GUIGraphicsContext.Auto3DTexture = new Texture(GUIGraphicsContext.DX9Device,
                                                             backbuffer.Description.Width,
                                                             backbuffer.Description.Height, 0, Usage.RenderTarget,
                                                             backbuffer.Description.Format, Pool.Default);

            if (GUIGraphicsContext.Auto3DSurface == null)
              GUIGraphicsContext.Auto3DSurface = GUIGraphicsContext.Auto3DTexture.GetSurfaceLevel(0);

            if (GUIGraphicsContext.Render3DMode == GUIGraphicsContext.eRender3DMode.SideBySide)
            {
              // left half (or right if switched)

              PlaneScene.RenderFor3DMode(GUIGraphicsContext.Switch3DSides ? GUIGraphicsContext.eRender3DModeHalf.SBSRight : GUIGraphicsContext.eRender3DModeHalf.SBSLeft,
                              timePassed, backbuffer, GUIGraphicsContext.Auto3DSurface,
                              new Rectangle(0, 0, backbuffer.Description.Width / 2, backbuffer.Description.Height));

              // right half (or right if switched)

              PlaneScene.RenderFor3DMode(GUIGraphicsContext.Switch3DSides ? GUIGraphicsContext.eRender3DModeHalf.SBSLeft : GUIGraphicsContext.eRender3DModeHalf.SBSRight,
                              timePassed, backbuffer, GUIGraphicsContext.Auto3DSurface,
                              new Rectangle(backbuffer.Description.Width / 2, 0, backbuffer.Description.Width / 2, backbuffer.Description.Height));
            }
            else
            {
              // upper half (or lower if switched)
              PlaneScene.RenderFor3DMode(GUIGraphicsContext.Switch3DSides ? GUIGraphicsContext.eRender3DModeHalf.TABBottom : GUIGraphicsContext.eRender3DModeHalf.TABTop, 
                              timePassed, backbuffer, GUIGraphicsContext.Auto3DSurface,
                              new Rectangle(0, 0, backbuffer.Description.Width, backbuffer.Description.Height/2));

              // lower half (or upper if switched)
              PlaneScene.RenderFor3DMode(GUIGraphicsContext.Switch3DSides ? GUIGraphicsContext.eRender3DModeHalf.TABTop : GUIGraphicsContext.eRender3DModeHalf.TABBottom, 
                              timePassed, backbuffer, GUIGraphicsContext.Auto3DSurface,
                              new Rectangle(0, backbuffer.Description.Height/2, backbuffer.Description.Width, backbuffer.Description.Height/2));
            }

            GUIGraphicsContext.DX9Device.Present();
            backbuffer.Dispose();
          }
        }
      }
      catch (DirectXException dex)
      {
        switch (dex.ErrorCode)
        {
          case D3DERR_INVALIDCALL:
            _errorCounter++;
            if (AdapterInfo.AdapterOrdinal > -1 && Manager.Adapters.Count > AdapterInfo.AdapterOrdinal)
            {
              double refreshRate = Manager.Adapters[AdapterInfo.AdapterOrdinal].CurrentDisplayMode.RefreshRate;
              if (refreshRate > 0 && _errorCounter > 5*refreshRate) // why 5 * refreshRate???
              {
                _errorCounter = 0; //reset counter
                Log.Info("Main: D3DERR_INVALIDCALL - {0}", dex.ToString());
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
              }
            }
            break;

          case D3DERR_DEVICEHUNG:
          case D3DERR_DEVICEREMOVED:
            Log.Info("Main: GPU_HUNG - {0}", dex.ToString());
            if (!RefreshRateChanger.RefreshRateChangePending)
            {
              g_Player.Stop();
            }
            GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.LOST;
            break;

          default:
            Log.Error(dex);
            break;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      finally
      {
        _isRendering = false;
      }
    }
  }

  #endregion

  #region OnProcess()

  /// <summary>
  /// 
  /// </summary>
  protected override void OnProcess()
  {
    // Set the date & time
    if (DateTime.Now.Second != _updateTimer.Second)
    {
      _updateTimer = DateTime.Now;
      GUIPropertyManager.SetProperty("#date", GetDate());
      GUIPropertyManager.SetProperty("#time", GetTime());
    }

    g_Player.Process();
    RecoverDevice();

    if (g_Player.Playing)
    {
      _playingState = true;
      if (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO)
      {
        GUIGraphicsContext.IsFullScreenVideo = true;
      }

      GUIGraphicsContext.IsPlaying = true;
      GUIGraphicsContext.IsPlayingVideo = (g_Player.IsVideo || g_Player.IsTV);

      if (g_Player.Paused)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_pause.png");
      }
      else if (g_Player.Speed > 1)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_fastforward.png");
      }
      else if (g_Player.Speed < 1)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_rewind.png");
      }
      else if (g_Player.Playing)
      {
        GUIPropertyManager.SetProperty("#playlogo", "logo_play.png");
      }

      if (g_Player.IsTV && !g_Player.IsTVRecording)
      {
        GUIPropertyManager.SetProperty("#currentplaytime", GUIPropertyManager.GetProperty("#TV.Record.current"));
        GUIPropertyManager.SetProperty("#shortcurrentplaytime", GUIPropertyManager.GetProperty("#TV.Record.current"));
      }
      else
      {
        GUIPropertyManager.SetProperty("#currentplaytime", Utils.SecondsToHMSString((int) g_Player.CurrentPosition));
        GUIPropertyManager.SetProperty("#currentremaining",
                                       Utils.SecondsToHMSString((int) (g_Player.Duration - g_Player.CurrentPosition)));
        GUIPropertyManager.SetProperty("#shortcurrentremaining",
                                       Utils.SecondsToShortHMSString(
                                         (int) (g_Player.Duration - g_Player.CurrentPosition)));
        GUIPropertyManager.SetProperty("#shortcurrentplaytime",
                                       Utils.SecondsToShortHMSString((int) g_Player.CurrentPosition));
      }

      if (g_Player.Duration > 0)
      {
        GUIPropertyManager.SetProperty("#duration", Utils.SecondsToHMSString((int) g_Player.Duration));
        GUIPropertyManager.SetProperty("#shortduration", Utils.SecondsToShortHMSString((int) g_Player.Duration));
        double percent = 100*g_Player.CurrentPosition/g_Player.Duration;
        GUIPropertyManager.SetProperty("#percentage", percent.ToString(CultureInfo.CurrentCulture));

        // Set comskip or chapter markers
        string strJumpPoints = string.Empty;
        string strChapters = string.Empty;
        if (((g_Player.IsTV && g_Player.IsTVRecording) || g_Player.HasVideo) && g_Player.HasChapters)
        {
          if (g_Player.JumpPoints != null)
          {
            // Set the marker start to indicate the start of commercials
            foreach (double jump in g_Player.JumpPoints)
            {
              double jumpPercent = jump/g_Player.Duration*100.0d;
              strJumpPoints += String.Format("{0:0.00}", jumpPercent) + " ";
            }
            // Set the marker end to indicate the end of commercials
            foreach (double chapter in g_Player.Chapters)
            {
              double chapterPercent = chapter/g_Player.Duration*100.0d;
              strChapters += String.Format("{0:0.00}", chapterPercent) + " ";
            }
          }
          else
          {
            // Set a fixed size marker at the start of each chapter
            double markerWidth = 0.7d;
            foreach (double chapter in g_Player.Chapters)
            {
              double chapterPercent = chapter/g_Player.Duration*100.0d;
              strChapters += String.Format("{0:0.00}", chapterPercent) + " ";
              chapterPercent = (chapterPercent >= markerWidth) ? chapterPercent - markerWidth : 0.0d;
              strJumpPoints += String.Format("{0:0.00}", chapterPercent) + " ";
            }
          }
        }
        GUIPropertyManager.SetProperty("#chapters", strChapters);
        GUIPropertyManager.SetProperty("#jumppoints", strJumpPoints);
      }
      else
      {
        GUIPropertyManager.SetProperty("#duration", string.Empty);
        GUIPropertyManager.SetProperty("#shortduration", string.Empty);
        GUIPropertyManager.SetProperty("#percentage", "0,0");

        GUIPropertyManager.SetProperty("#chapters", string.Empty);
        GUIPropertyManager.SetProperty("#jumppoints", string.Empty);
      }

      GUIPropertyManager.SetProperty("#playspeed", g_Player.Speed.ToString(CultureInfo.InvariantCulture));
    }
    else
    {
      GUIGraphicsContext.IsPlaying = false;
      if (_playingState)
      {
        GUIPropertyManager.RemovePlayerProperties();
        _playingState = false;
      }
    }
  }

  #endregion

  #region FrameMove()
  
  /// <summary>
  /// 
  /// </summary>
  protected override void FrameMove()
  {
    // we are suspended/hibernated
    if (_suspended)
    {
      return;
    }
    
    #if !DEBUG
    try
    {
    #endif
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.STOPPING)
      {
        Log.Info("Main: Stopping FrameMove");
        Close();
        return;
      }

      try
      {
        GUIWindowManager.DispatchThreadMessages();
        GUIWindowManager.ProcessWindows();
      }
      catch (FileNotFoundException ex)
      {
        Log.Error(ex);
        // ReSharper disable LocalizableElement
        MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
        // ReSharper restore LocalizableElement
        Close();
      }
      if (_useScreenSaver)
      {
        if ((GUIGraphicsContext.IsFullScreenVideo && g_Player.Paused == false) || GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_SLIDESHOW || GUIWindowManager.ActiveWindow == _idlePluginWindowId)
        {
          GUIGraphicsContext.ResetLastActivity();
        }
        if (!GUIGraphicsContext.BlankScreen && !Windowed)
        {
          TimeSpan ts = DateTime.Now - GUIGraphicsContext.LastActivity;
          if (ts.TotalSeconds >= _timeScreenSaver)
          {
            if (_useIdleblankScreen)
            {
              if (!GUIGraphicsContext.BlankScreen)
              {
                Log.Debug("Main: Idle timer is blanking the screen after {0} seconds of inactivity", ts.TotalSeconds.ToString("n0"));
              }
              GUIGraphicsContext.BlankScreen = true;
            }
            else if (_useIdlePluginScreen)
            {
              if (_idlePluginWindowId != 0)
              {
                if (!_idlePluginActive)
                {
                  _idlePluginActive = true;
                  Log.Debug("Main: Idle timer is setting the screensaver application after {0} seconds of inactivity", ts.TotalSeconds.ToString("n0"));
                  GUIWindowManager.ActivateWindow(_idlePluginWindowId);

                }
              }
              else
              {
                Log.Debug("Main: Idle timer tried to set plugin screen saver but window id was 0");
              }
            }
            else
            {
              // Slower rendering will have an impact on scrolling labels or list items
              // As long as we're e.g. listening to music on "Playing Now" screen
              // we might not want to slow things down here.
              // This feature is mainly intended to save energy on idle 24/7 rigs.
              if (GUIWindowManager.ActiveWindow != (int) GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
              {
                if (!GUIGraphicsContext.SaveRenderCycles)
                {
                  Log.Debug("Main: Idle timer is entering power save mode after {0} seconds of inactivity", ts.TotalSeconds.ToString("n0"));
                }
                GUIGraphicsContext.SaveRenderCycles = true;
              }
            }
          }
        }
      }

    #if !DEBUG
    }
    catch (Exception ex)
    {
      Log.Error(ex);
    }
    #endif
  }

  #endregion

  #region Handle messages, keypresses, mouse moves etc

  /// <summary>
  /// 
  /// </summary>
  /// <param name="action"></param>
  private void OnAction(Action action)
  {
    try
    {
      // hack/fix for lastactivemodulefullscreen
      // when recovering from hibernation/standby after closing with remote control somehow a F9 (keycode 120) onkeydown event is thrown from outside
      // we are currently filtering it away.
      // sometimes more than one F9 keydown event fired.
      // if these events are not filtered away the F9 context menu is shown on the restored/shown module.
      if ((action.wID == Action.ActionType.ACTION_CONTEXT_MENU || _suspended) && (_showLastActiveModule))
      {
        if (_ignoreContextMenuAction)
        {
          _ignoreContextMenuAction = false;
          _lastContextMenuAction = DateTime.Now;
          return;
        }
        
        if (_lastContextMenuAction != DateTime.MaxValue)
        {
          TimeSpan ts = _lastContextMenuAction - DateTime.Now;
          if (ts.TotalMilliseconds > -100)
          {
            _ignoreContextMenuAction = false;
            _lastContextMenuAction = DateTime.Now;
            return;
          }
        }
        _lastContextMenuAction = DateTime.Now;
      }

      GUIWindow window;
      if (action.IsUserAction())
      {
        GUIGraphicsContext.ResetLastActivity();
      }

      // needed to avoid cursor show when MP windows change (for ex when refesh rate is working / BassVis)
      _moveMouseCursorPositionRefresh = D3D._lastCursorPosition;

      switch (action.wID)
      {
        // record current tv program
        case Action.ActionType.ACTION_RECORD:
          if ((GUIGraphicsContext.IsTvWindow(GUIWindowManager.ActiveWindowEx) &&
               GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_TVGUIDE) &&
              (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_DIALOG_TVGUIDE))
          {
            GUIWindow tvHome = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            if (tvHome != null && tvHome.GetID != GUIWindowManager.ActiveWindow)
            {
              tvHome.OnAction(action);
              return;
            }
          }
          break;

        // TV: zap to previous channel
        case Action.ActionType.ACTION_PREV_CHANNEL:
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        // TV: zap to next channel
        case Action.ActionType.ACTION_NEXT_CHANNEL:
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        // TV: zap to last channel viewed
        case Action.ActionType.ACTION_LAST_VIEWED_CHANNEL: // mPod
          if (!GUIWindowManager.IsRouted)
          {
            window = GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_TV);
            window.OnAction(action);
            return;
          }
          break;

        // toggle between windowed and fullscreen mode
        case Action.ActionType.ACTION_TOGGLE_WINDOWED_FULLSCREEN:
          ToggleFullscreen();
          return;

        // mute or unmute audio
        case Action.ActionType.ACTION_VOLUME_MUTE:
          VolumeHandler.Instance.IsMuted = !VolumeHandler.Instance.IsMuted;
          break;

        // decrease volume 
        case Action.ActionType.ACTION_VOLUME_DOWN:
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Previous;
          break;

        // increase volume 
        case Action.ActionType.ACTION_VOLUME_UP:
          VolumeHandler.Instance.Volume = VolumeHandler.Instance.Next;
          break;

        // toggle live tv in background
        case Action.ActionType.ACTION_BACKGROUND_TOGGLE:
          // show livetv or video as background instead of the static GUI background
          // toggle livetv/video in background on/off
          if (GUIGraphicsContext.ShowBackground)
          {
            Log.Info("Main: Using live TV as background");
            // if on, but we're not playing any video or watching tv
            if (GUIGraphicsContext.Vmr9Active)
            {
              GUIGraphicsContext.ShowBackground = false;
            }
            else
            {
              // show warning message
              var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SHOW_WARNING, 0, 0, 0, 0, 0, 0) {Param1 = 727, Param2 = 728, Param3 = 729};
              GUIWindowManager.SendMessage(msg);
              return;
            }
          }
          else
          {
            Log.Info("Main: Using GUI as background");
            GUIGraphicsContext.ShowBackground = true;
          }
          break;

        // switch between several home windows
        case Action.ActionType.ACTION_SWITCH_HOME:
          GUIWindow.Window newHome = _startWithBasicHome
                                       ? GUIWindow.Window.WINDOW_SECOND_HOME
                                       : GUIWindow.Window.WINDOW_HOME;
          // do we prefer to use only one home screen?
          if (_useOnlyOneHome)
          {
            // skip if we are already in there
            if (GUIWindowManager.ActiveWindow == (int)newHome)
            {
              break;
            }
          }
          // we like both 
          else
          {
            // if already in one home switch to the other
            switch (GUIWindowManager.ActiveWindow)
            {
              case (int)GUIWindow.Window.WINDOW_HOME:
                newHome = GUIWindow.Window.WINDOW_SECOND_HOME;
                break;

              case (int)GUIWindow.Window.WINDOW_SECOND_HOME:
                newHome = GUIWindow.Window.WINDOW_HOME;
                break;
            }
          }
          var homeMsg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW, 0, 0, 0, (int)newHome, 0, null);
          GUIWindowManager.SendThreadMessage(homeMsg);
          // Stop Video for MyPictures when going to home
          if (g_Player.IsPicture)
          {
            GUISlideShow._slideDirection = 0;
            g_Player.Stop();
          }
          break;

        case Action.ActionType.ACTION_MPRESTORE:
          Log.Info("Main: Restore MP by action");
          RestoreFromTray();
          if ((g_Player.IsVideo || g_Player.IsTV || g_Player.IsDVD) && Volume > 0)
          {
            g_Player.Volume = Volume;
            g_Player.ContinueGraph();
            if (g_Player.Paused && !GUIGraphicsContext.IsVMR9Exclusive)
            {
              g_Player.Pause();
            }
          }
          break;

        // reboot pc
        case Action.ActionType.ACTION_POWER_OFF:
        case Action.ActionType.ACTION_SUSPEND:
        case Action.ActionType.ACTION_HIBERNATE:
        case Action.ActionType.ACTION_REBOOT:
          // reboot
          Log.Info("Main: Reboot requested");
          bool okToChangePowermode = (Math.Abs(action.fAmount1 - 1) < float.Epsilon);

          if (!okToChangePowermode)
          {
            okToChangePowermode = PromptUserBeforeChangingPowermode(action);
          }

          if (okToChangePowermode)
          {
            switch (action.wID)
            {
              case Action.ActionType.ACTION_REBOOT:
                _restartOptions = RestartOptions.Reboot;
                _useRestartOptions = true;
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                break;

              case Action.ActionType.ACTION_POWER_OFF:
                _restartOptions = RestartOptions.PowerOff;
                _useRestartOptions = true;
                GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                ShuttingDown = true;
                break;

              case Action.ActionType.ACTION_SUSPEND:
                if (IsSuspendOrHibernationAllowed())
                {
                  _restartOptions = RestartOptions.Suspend;
                  Utils.SuspendSystem(false);
                }
                else
                {
                  Log.Info("Main: SUSPEND ignored since suspend graceperiod of {0} sec. is violated.", _suspendGracePeriodSec); 
                }
                break;

              case Action.ActionType.ACTION_HIBERNATE:
                if (IsSuspendOrHibernationAllowed())
                {
                  _restartOptions = RestartOptions.Hibernate;
                  Utils.HibernateSystem(false);
                }
                else
                {
                  Log.Info("Main: HIBERNATE ignored since hibernate graceperiod of {0} sec. is violated.", _suspendGracePeriodSec);
                }
                break;
            }
          }
          break;

        // eject cd
        case Action.ActionType.ACTION_EJECTCD:
          Utils.EjectCDROM();
          break;

        // shutdown pc
        case Action.ActionType.ACTION_SHUTDOWN:
          Log.Info("Main: Shutdown dialog");
          var dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
          if (dlg != null)
          {
            dlg.Reset();
            dlg.SetHeading(GUILocalizeStrings.Get(498)); //Menu
            dlg.AddLocalizedString(1057); //Exit MediaPortal
            dlg.AddLocalizedString(1058); //Restart MediaPortal
            dlg.AddLocalizedString(1032); //Suspend
            dlg.AddLocalizedString(1049); //Hibernate
            dlg.AddLocalizedString(1031); //Reboot
            dlg.AddLocalizedString(1030); //PowerOff
            dlg.DoModal(GUIWindowManager.ActiveWindow);

            if (dlg.SelectedId >= 0)
            {
              switch (dlg.SelectedId)
              {
                case 1057:
                  ExitMP();
                  return;

                case 1058:
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  Utils.RestartMePo();
                  break;

                case 1030:
                  _restartOptions = RestartOptions.PowerOff;
                  _useRestartOptions = true;
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  ShuttingDown = true;
                  break;

                case 1031:
                  _restartOptions = RestartOptions.Reboot;
                  _useRestartOptions = true;
                  GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
                  ShuttingDown = true;
                  break;

                case 1032:
                  _restartOptions = RestartOptions.Suspend;
                  Utils.SuspendSystem(false);
                  break;

                case 1049:
                  _restartOptions = RestartOptions.Hibernate;
                  Utils.HibernateSystem(false);
                  break;
              }
            }
            else
            {
              GUIWindow win = GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_HOME);
              if (win != null)
              {
                win.OnAction(new Action(Action.ActionType.ACTION_MOVE_LEFT, 0, 0));
              }
            }
          }
          break;

        // exit Mediaportal
        case Action.ActionType.ACTION_EXIT:
          ExitMP();
          break;

        // stop radio
        case Action.ActionType.ACTION_STOP:
          break;

        // Take Screen shot
        case Action.ActionType.ACTION_TAKE_SCREENSHOT:
          try
          {
            string directory = string.Format("{0}\\MediaPortal Screenshots\\{1:0000}-{2:00}-{3:00}",
                                             Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                                             DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (!Directory.Exists(directory))
            {
              Log.Info("Main: Taking screenshot - Creating directory: {0}", directory);
              Directory.CreateDirectory(directory);
            }

            string fileName = string.Format("{0}\\{1:00}-{2:00}-{3:00}", directory, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            Log.Info("Main: Taking screenshot - Target: {0}.png", fileName);
            Surface backbuffer = GUIGraphicsContext.DX9Device.GetBackBuffer(0, 0, BackBufferType.Mono);
            SurfaceLoader.Save(fileName + ".png", ImageFileFormat.Png, backbuffer);
            backbuffer.Dispose();
            Log.Info("Main: Taking screenshot done");
          }
          catch (Exception ex)
          {
            Log.Info("Main: Error taking screenshot: {0}", ex.Message);
          }
          break;

        case Action.ActionType.ACTION_SHOW_GUI:
          // can we handle the switch to fullscreen?
          if (!GUIGraphicsContext.IsFullScreenVideo && g_Player.ShowFullScreenWindow())
          {
            return;
          }
          break;
      }

      if (g_Player.Playing)
      {
        string activeWindowName;
        GUIWindow.Window activeWindow;

        switch (action.wID)
        {
          // show DVD menu
          case Action.ActionType.ACTION_DVD_MENU:
            if (g_Player.IsDVD)
            {
              g_Player.OnAction(action);
              return;
            }
            break;

          // DVD: goto previous chapter
          // play previous item from playlist;
          case Action.ActionType.ACTION_PREV_ITEM:
          case Action.ActionType.ACTION_PREV_CHAPTER:
            if (g_Player.IsDVD || g_Player.HasChapters)
            {
              action = new Action(Action.ActionType.ACTION_PREV_CHAPTER, 0, 0);
              g_Player.OnAction(action);
              break;
            }
            // When MyPictures Plugin shows the pictures/videos we don't want to change music track
            activeWindowName = GUIWindowManager.ActiveWindow.ToString(CultureInfo.InvariantCulture);
            activeWindow = (GUIWindow.Window) Enum.Parse(typeof(GUIWindow.Window), activeWindowName);
            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key) &&
                (activeWindow != GUIWindow.Window.WINDOW_SLIDESHOW && !g_Player.IsPicture))
            {
              PlaylistPlayer.PlayPrevious();
            }
            break;

          // play next item from playlist;
          // DVD: goto next chapter
          case Action.ActionType.ACTION_NEXT_CHAPTER:
          case Action.ActionType.ACTION_NEXT_ITEM:
            if (g_Player.IsDVD || g_Player.HasChapters)
            {
              action = new Action(Action.ActionType.ACTION_NEXT_CHAPTER, 0, 0);
              g_Player.OnAction(action);
              break;
            }
            // When MyPictures Plugin shows the pictures/videos we don't want to change music track
            activeWindowName = GUIWindowManager.ActiveWindow.ToString(CultureInfo.InvariantCulture);
            activeWindow = (GUIWindow.Window) Enum.Parse(typeof(GUIWindow.Window), activeWindowName);
            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key) && (activeWindow != GUIWindow.Window.WINDOW_SLIDESHOW && !g_Player.IsPicture))
            {
              PlaylistPlayer.PlayNext();
            }
            break;

          // stop playback
          case Action.ActionType.ACTION_STOP:
            // When MyPictures Plugin shows the pictures we want to stop the slide show only, not the player
            activeWindowName = GUIWindowManager.ActiveWindow.ToString(CultureInfo.InvariantCulture);
            activeWindow = (GUIWindow.Window)Enum.Parse(typeof(GUIWindow.Window), activeWindowName);
            if ((activeWindow == GUIWindow.Window.WINDOW_SLIDESHOW) || (activeWindow == GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO && g_Player.IsPicture) && g_Player.Playing)
            {
              break;
            }

            if (!g_Player.IsTV || !GUIGraphicsContext.IsFullScreenVideo)
            {
              Log.Info("Main: Stopping media");
              if (g_Player.IsPicture)
              {
                GUISlideShow._slideDirection = 0;
              }
              g_Player.Stop();
            }
            break;

          // Jump to Music Now Playing
          case Action.ActionType.ACTION_JUMP_MUSIC_NOW_PLAYING:
            if (g_Player.IsMusic)
            {
              if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW)
              {
                GUIWindowManager.ShowPreviousWindow();
            }
              else
              {
                GUIWindowManager.ActivateWindow((int) GUIWindow.Window.WINDOW_MUSIC_PLAYING_NOW);
              }
            }
            break;

          // play music
          // resume playback
          case Action.ActionType.ACTION_PLAY:
          case Action.ActionType.ACTION_MUSIC_PLAY:
            // Don't start playing from the beginning if we press play to return to normal speed
            if (g_Player.IsMusic && g_Player.Speed != 1 &&
                (GUIWindowManager.ActiveWindow != (int) GUIWindow.Window.WINDOW_MUSIC_FILES &&
                 GUIWindowManager.ActiveWindow != (int) GUIWindow.Window.WINDOW_MUSIC_GENRE))
            {
              g_Player.Speed = 1;
              break;
            }

            g_Player.StepNow();
            g_Player.Speed = 1;

            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
            break;

          // pause (or resume playback)
          case Action.ActionType.ACTION_PAUSE:
            // When MyPictures Plugin shows the pictures we want to pause the slide show only, not the player
            activeWindowName = GUIWindowManager.ActiveWindow.ToString(CultureInfo.InvariantCulture);
            activeWindow = (GUIWindow.Window)Enum.Parse(typeof(GUIWindow.Window), activeWindowName);
            if ((activeWindow == GUIWindow.Window.WINDOW_SLIDESHOW) ||
                (activeWindow == GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO && g_Player.IsPicture) && g_Player.Playing && !g_Player.IsVideo)
            {
              break;
            }
            g_Player.Pause();

            break;

          // fast forward...
          case Action.ActionType.ACTION_FORWARD:
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }
              g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
              break;
            }

          // Decide if we want to have CD style of FF or Skip steps
          case Action.ActionType.ACTION_MUSIC_FORWARD:
            // When MyPictures Plugin shows the pictures/videos we don't want to change music track
            activeWindowName = GUIWindowManager.ActiveWindow.ToString(CultureInfo.InvariantCulture);
            activeWindow = (GUIWindow.Window) Enum.Parse(typeof(GUIWindow.Window), activeWindowName);
            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key) && (activeWindow != GUIWindow.Window.WINDOW_SLIDESHOW && !g_Player.IsPicture || g_Player.IsVideo))
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }
              if (!MediaPortal.MusicPlayer.BASS.Config.UseSkipSteps)
              {
                g_Player.Speed = Utils.GetNextForwardSpeed(g_Player.Speed);
              }
            }
            break;
 
          // fast rewind...
          case Action.ActionType.ACTION_REWIND:
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }
              g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
              break;
            }

          // Decide if we want to have CD style of Rew or Skip steps
          case Action.ActionType.ACTION_MUSIC_REWIND:
            // When MyPictures Plugin shows the pictures/videos we don't want to change music track
            activeWindowName = GUIWindowManager.ActiveWindow.ToString(CultureInfo.InvariantCulture);
            activeWindow = (GUIWindow.Window) Enum.Parse(typeof(GUIWindow.Window), activeWindowName);
            if (!ActionTranslator.HasKeyMapped(GUIWindowManager.ActiveWindowEx, action.m_key) && (activeWindow != GUIWindow.Window.WINDOW_SLIDESHOW && !g_Player.IsPicture || g_Player.IsVideo))
            {
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }
              if (!MediaPortal.MusicPlayer.BASS.Config.UseSkipSteps)
              {
                g_Player.Speed = Utils.GetNextRewindSpeed(g_Player.Speed);
              }
            }
            break;
         }
      }
      GUIWindowManager.OnAction(action);
    }
    catch (FileNotFoundException ex)
    {
      Log.Error(ex);
      // ReSharper disable LocalizableElement
      MessageBox.Show("File not found:" + ex.FileName, "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
      // ReSharper restore LocalizableElement
      Close();
    }
    catch (Exception ex)
    {
      Log.Error(ex);
      Log.Error("Exception: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
      #if !DEBUG
      throw new Exception("exception occurred", ex);
      #endif
    }
  }


  /// <summary>
  /// 
  /// </summary>
  private void ExitMP()
  {
    Log.Info("Main: Exit requested");

    if (MinimizeOnGuiExit && !ShuttingDown)
    {
      Log.Info("Main: Minimizing to tray on GUI exit");
      ExitToTray = true;
      MinimizeToTray();
      return;
    }
    GUIGraphicsContext.CurrentState = GUIGraphicsContext.State.STOPPING;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="action"></param>
  /// <returns></returns>
  private static bool PromptUserBeforeChangingPowermode(Action action)
  {
    var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_ASKYESNO, 0, 0, 0, 0, 0, 0);
    switch (action.wID)
    {
      case Action.ActionType.ACTION_REBOOT:
        msg.Param1 = 630;
        break;

      case Action.ActionType.ACTION_POWER_OFF:
        msg.Param1 = 1600;
        break;

      case Action.ActionType.ACTION_SUSPEND:
        msg.Param1 = 1601;
        break;

      case Action.ActionType.ACTION_HIBERNATE:
        msg.Param1 = 1602;
        break;
    }
    msg.Param2 = 0;
    msg.Param3 = 0;
    GUIWindowManager.SendMessage(msg);

    return (msg.Param1 == 1);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  private bool IsSuspendOrHibernationAllowed()
  {
    TimeSpan ts = DateTime.Now - _lastOnresume;
    return (ts.TotalSeconds > _suspendGracePeriodSec);
  }

  #region keypress handlers
  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void KeyPressEvent(KeyPressEventArgs e)
  {
    GUIGraphicsContext.BlankScreen = false;
    _idlePluginActive = false;
    var key = new Key(e.KeyChar, 0);
    var action = new Action();
    if (GUIWindowManager.IsRouted || GUIWindowManager.ActiveWindowEx == (int)GUIWindow.Window.WINDOW_TV_SEARCH)
    // is a dialog open or maybe the tv schedule search (GUISMSInputControl)?
    {
      GUIGraphicsContext.ResetLastActivity();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD) &&
          (GUIWindowManager.ActiveWindowEx != (int)GUIWindow.Window.WINDOW_TV_SEARCH))
      {
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }
        GUIGraphicsContext.OnAction(action);
      }
      else
      {
        action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
        GUIGraphicsContext.OnAction(action);
      }
      return;
    }

    if (key.KeyChar == '!')
    {
      _showStats = !_showStats;
    }

    if (key.KeyChar == '|' && g_Player.Playing == false)
    {
      g_Player.Play("rtsp://localhost/stream0");
      g_Player.ShowFullScreenWindow();
      return;
    }

    if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
    {
      if (action.ShouldDisableScreenSaver)
      {
        GUIGraphicsContext.ResetLastActivity();
      }
      if (action.SoundFileName.Length > 0 && !g_Player.Playing)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
    else
    {
      GUIGraphicsContext.ResetLastActivity();
    }

    action = new Action(key, Action.ActionType.ACTION_KEY_PRESSED, 0, 0);
    GUIGraphicsContext.OnAction(action);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void KeyDownEvent(KeyEventArgs e)
  {
    if (!_suspended && AppActive)
    {
      GUIGraphicsContext.ResetLastActivity();
      var key = new Key(0, (int) e.KeyCode);
      var action = new Action();
      if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
      {
        if (action.SoundFileName.Length > 0 && !g_Player.Playing)
        {
          Utils.PlaySound(action.SoundFileName, false, true);
        }
        GUIGraphicsContext.OnAction(action);
      }
    }
  }

  #endregion

  #region mouse event handlers
  

  /// <summary>
  /// 
  /// </summary>
  /// <param name="location"></param>
  /// <returns></returns>
  private Point ScaleCursorPosition(Point location)
  {
    var point = new Point
    {
      X = (int)Math.Round(location.X * (float)GUIGraphicsContext.Width / ClientSize.Width),
      Y = (int)Math.Round(location.Y * (float)GUIGraphicsContext.Height / ClientSize.Height)
    };
    return point;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void OnMouseWheel(MouseEventArgs e)
  {
    //Update Timer
    MouseTimeOutTimer = DateTime.Now;

    if (e.Delta > 0)
    {
      base.MouseMoveEvent(e);
      Point p = ScaleCursorPosition(e.Location);
      var action = new Action(Action.ActionType.ACTION_MOVE_UP, p.X, p.Y) {MouseButton = e.Button};
      GUIGraphicsContext.ResetLastActivity(); 
      GUIGraphicsContext.OnAction(action);
      base.MouseMoveEvent(e);
    }
    else if (e.Delta < 0)
    {
      base.MouseMoveEvent(e);
      Point p = ScaleCursorPosition(e.Location);
      var action = new Action(Action.ActionType.ACTION_MOVE_DOWN, p.X, p.Y) {MouseButton = e.Button};
      GUIGraphicsContext.ResetLastActivity();
      GUIGraphicsContext.OnAction(action);
      base.MouseMoveEvent(e);
    }
    base.OnMouseWheel(e);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void MouseMoveEvent(MouseEventArgs e)
  {
    Cursor current = Cursor.Current;
    // Disable first mouse action when mouse was hidden
    if (!MouseCursor || current == null)
    {
      _moveMouseCursorPosition = Cursor.Position;
      base.MouseMoveEvent(e);
    }
    else
    {
      if (_lastCursorPosition != Cursor.Position)
      {
       bool cursorMovedFarEnough = (Math.Abs(_lastCursorPosition.X - Cursor.Position.X) > 10) || (Math.Abs(_lastCursorPosition.Y - Cursor.Position.Y) > 10);
        if (cursorMovedFarEnough)
        {
          GUIGraphicsContext.ResetLastActivity();
          if (GUIGraphicsContext.DBLClickAsRightClick && _mouseClickFired)
          {
            CheckSingleClick(e);
          }
        }
        _lastCursorPosition = Cursor.Position;
        if (GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow) != null)
        {
          Point p = ScaleCursorPosition(e.Location);
          var action = new Action(Action.ActionType.ACTION_MOUSE_MOVE, p.X, p.Y) {MouseButton = e.Button};
          GUIGraphicsContext.OnAction(action);
          if (MouseCursor && current != null)
          {
            MouseTimeOutTimer = DateTime.Now;
            Cursor.Show();
          }
        }
      }
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void MouseDoubleClickEvent(MouseEventArgs e)
  {
    if (GUIGraphicsContext.DBLClickAsRightClick)
    {
      return;
    }

    GUIGraphicsContext.ResetLastActivity();

    // Disable first mouse action when mouse was hidden
    if (!MouseCursor)
    {
      base.MouseClickEvent(e);
    }
    else
    {
      _lastCursorPosition = Cursor.Position;
      Point p = ScaleCursorPosition(e.Location);

      var actionMove = new Action(Action.ActionType.ACTION_MOUSE_MOVE, p.X, p.Y);
      GUIGraphicsContext.OnAction(actionMove);

      var action = new Action(Action.ActionType.ACTION_MOUSE_DOUBLECLICK, p.X, p.Y) {MouseButton = e.Button, SoundFileName = "click.wav"};
      if (action.SoundFileName.Length > 0 && !g_Player.Playing)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="e"></param>
  protected override void MouseClickEvent(MouseEventArgs e)
  {
    GUIGraphicsContext.ResetLastActivity();

    // Click event
    MouseTimeOutTimer = DateTime.Now;

    // Disable first mouse action when mouse was hidden
    Cursor current = Cursor.Current;
    if (!MouseCursor || current == null)
    {
      base.MouseClickEvent(e);
    }
    else
    {
      Action action;
      bool mouseButtonRightClick = false;
      _lastCursorPosition = Cursor.Position;
      Point p = ScaleCursorPosition(e.Location);

      var actionMove = new Action(Action.ActionType.ACTION_MOUSE_MOVE, p.X, p.Y);
      GUIGraphicsContext.OnAction(actionMove);

      if (MouseCursor && current != null)
      {
        MouseTimeOutTimer = DateTime.Now;
        Cursor.Show();
      }

      if (e.Button == MouseButtons.Left)
      {
        if (GUIGraphicsContext.DBLClickAsRightClick)
        {
          _mouseClickFired = false;
          if (e.Clicks < 2)
          {
            _lastMouseClickEvent = e;
            _mouseClickFired = true;
            return;
          }
          // Double click used as right click
          _lastMouseClickEvent = null;

          mouseButtonRightClick = true;
        }
        else
        {
          action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, p.X, p.Y) {MouseButton = e.Button, SoundFileName = "click.wav"};
          if (action.SoundFileName.Length > 0 && !g_Player.Playing)
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
          return;
        }
      }

      // right mouse button=back
      if ((e.Button == MouseButtons.Right) || mouseButtonRightClick)
      {
        GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
        if ((window.GetFocusControlId() != -1) || GUIGraphicsContext.IsFullScreenVideo ||
            (GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_SLIDESHOW))
        {
          // Get context menu
          action = new Action(Action.ActionType.ACTION_CONTEXT_MENU, p.X, p.Y) {MouseButton = e.Button, SoundFileName = "click.wav"};
          if (action.SoundFileName.Length > 0 && !g_Player.Playing)
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
        }
        else
        {
          var key = new Key(0, (int) Keys.Escape);
          action = new Action();
          if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
          {
            if (action.SoundFileName.Length > 0 && !g_Player.Playing)
            {
              Utils.PlaySound(action.SoundFileName, false, true);
            }
            GUIGraphicsContext.OnAction(action);
            return;
          }
        }
      }

      // middle mouse button=Y
      if (e.Button == MouseButtons.Middle)
      {
        var key = new Key('y', 0);
        action = new Action();
        if (ActionTranslator.GetAction(GUIWindowManager.ActiveWindowEx, key, ref action))
        {
          if (action.SoundFileName.Length > 0 && !g_Player.Playing)
          {
            Utils.PlaySound(action.SoundFileName, false, true);
          }
          GUIGraphicsContext.OnAction(action);
        }
      }
    }
  }

  
  /// <summary>
  /// 
  /// </summary>
  private void CheckSingleClick(MouseEventArgs e)
  {
    // Check for touchscreen users and TVGuide items
    if (GUIWindowManager.ActiveWindow == (int)GUIWindow.Window.WINDOW_TVGUIDE)
    {
      GUIWindow window = GUIWindowManager.GetWindow(GUIWindowManager.ActiveWindow);
      if ((window.GetFocusControlId() == 1) && (GUIWindowManager.RoutedWindow == -1))
      {
        // Don't send single click (only the mouse move event is send)
        _mouseClickFired = false;
        return;
      }
    }

    if (_mouseClickFired)
    {
      _mouseClickFired = false;
      Point p = ScaleCursorPosition(e.Location);
      var action = new Action(Action.ActionType.ACTION_MOUSE_CLICK, p.X, p.Y) {MouseButton = _lastMouseClickEvent.Button, SoundFileName = "click.wav"};
      if (action.SoundFileName.Length > 0 && !g_Player.Playing)
      {
        Utils.PlaySound(action.SoundFileName, false, true);
      }
      GUIGraphicsContext.OnAction(action);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  protected override void NotifyIconRestore(Object sender, EventArgs e)
  {
    RestoreFromTray();
  }

  #endregion


  /// <summary>
  /// 
  /// </summary>
  /// <param name="message"></param>
  private void OnMessage(GUIMessage message)
  {
    if (!_suspended && !ExitToTray && !IsVisible)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW:
        case GUIMessage.MessageType.GUI_MSG_GETFOCUS:
          Log.Debug("Main: Setting focus");
          if (Volume > 0 && (g_Player.IsVideo || g_Player.IsTV))
          {
            g_Player.Volume = Volume;
            if (g_Player.Paused)
            {
              g_Player.Pause();
            }
          }
          RestoreFromTray();
          break;
      }
    }

    if (!_suspended && AppActive)
    {
      switch (message.Message)
      {
        case GUIMessage.MessageType.GUI_MSG_RESTART_REMOTE_CONTROLS:
          Log.Info("Main: Restart remote controls");
          InputDevices.Stop();
          InputDevices.Init();
          break;

        case GUIMessage.MessageType.GUI_MSG_GOTO_WINDOW:
          GUIWindowManager.ActivateWindow(message.Param1);
          GUIGraphicsContext.IsFullScreenVideo = GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_TVFULLSCREEN ||
                                                 GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_TELETEXT ||
                                                 GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_VIDEO ||
                                                 GUIWindowManager.ActiveWindow == (int) GUIWindow.Window.WINDOW_FULLSCREEN_MUSIC;
          break;

        case GUIMessage.MessageType.GUI_MSG_CD_INSERTED:
          AutoPlay.ExamineCD(message.Label);
          break;

        case GUIMessage.MessageType.GUI_MSG_VOLUME_INSERTED:
          AutoPlay.ExamineVolume(message.Label);
          break;

        case GUIMessage.MessageType.GUI_MSG_TUNE_EXTERNAL_CHANNEL:
          double retNum;
          bool bIsInteger = Double.TryParse(message.Label, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
          try
          {
            if (bIsInteger)
            {
              _usbuirtdevice.ChangeTunerChannel(message.Label);
            }
          }
          // ReSharper disable EmptyGeneralCatchClause
          catch {}
          // ReSharper restore EmptyGeneralCatchClause

          try
          {
            _winlircdevice.ChangeTunerChannel(message.Label);
          }
          // ReSharper disable EmptyGeneralCatchClause
          catch { }
          // ReSharper restore EmptyGeneralCatchClause
          
          try
          {
            if (bIsInteger)
            {
              _redeyedevice.ChangeTunerChannel(message.Label);
            }
          }
          // ReSharper disable EmptyGeneralCatchClause
          catch {}
          // ReSharper restore EmptyGeneralCatchClause
          break;

        case GUIMessage.MessageType.GUI_MSG_SWITCH_FULL_WINDOWED:
          Log.Info("Main: GUI_MSG_SWITCH_FULL_WINDOWED message is obsolete.");
          break;

        case GUIMessage.MessageType.GUI_MSG_GETFOCUS:
          Log.Debug("Main: Setting focus");
          if (WindowState != FormWindowState.Minimized)
          {
            Activate();
          }
          else
          {
            if (Volume > 0 && (g_Player.IsVideo || g_Player.IsTV))
            {
              g_Player.Volume = Volume;
              if (g_Player.Paused)
              {
                g_Player.Pause();
              }
            }
            RestoreFromTray();
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_CODEC_MISSING:
          var dlgOk = (GUIDialogOK) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_OK);
          dlgOk.SetHeading(string.Empty);
          dlgOk.SetLine(1, message.Label);
          dlgOk.SetLine(2, string.Empty);
          dlgOk.SetLine(3, message.Label2);
          dlgOk.SetLine(4, message.Label3);
          dlgOk.DoModal(GUIWindowManager.ActiveWindow);
          break;

        case GUIMessage.MessageType.GUI_MSG_REFRESHRATE_CHANGED:
          var dlgNotify = (GUIDialogNotify) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
          if (dlgNotify != null)
          {
            dlgNotify.Reset();
            dlgNotify.ClearAll();
            dlgNotify.SetHeading(message.Label);
            dlgNotify.SetText(message.Label2);
            dlgNotify.TimeOut = message.Param1;
            dlgNotify.DoModal(GUIWindowManager.ActiveWindow);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_ENDED:
        case GUIMessage.MessageType.GUI_MSG_PLAYBACK_STOPPED:
          // reset idle timer for consistent timing after end 0f playback
          SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
          break;

        case GUIMessage.MessageType.GUI_MSG_ADD_REMOVABLE_DRIVE:
          if (!Utils.IsRemovable(message.Label))
          {
            VirtualDirectories.Instance.Movies.AddRemovableDrive(message.Label, message.Label2);
            VirtualDirectories.Instance.Music.AddRemovableDrive(message.Label, message.Label2);
            VirtualDirectories.Instance.Pictures.AddRemovableDrive(message.Label, message.Label2);
          }
          break;

        case GUIMessage.MessageType.GUI_MSG_REMOVE_REMOVABLE_DRIVE:
          if (!Utils.IsRemovable(message.Label))
          {
            VirtualDirectories.Instance.Movies.Remove(message.Label);
            VirtualDirectories.Instance.Music.Remove(message.Label);
            VirtualDirectories.Instance.Pictures.Remove(message.Label);
          }
          break;
      }
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  // ReSharper disable UnusedMember.Local
  // ReSharper disable UnusedParameter.Local
  private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
  // ReSharper restore UnusedParameter.Local
  // ReSharper restore UnusedMember.Local
  {
    try
    {
      var ex = (Exception)e.ExceptionObject;
      MessageBox.Show(string.Format("{0}\n\n{1}\n{2}", Resources.UnhandledException, ex.Message, ex.StackTrace), Resources.FatalError, MessageBoxButtons.OK, MessageBoxIcon.Stop);
    }
    finally
    {
      Application.Exit();
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  // ReSharper disable UnusedMember.Local
  // ReSharper disable UnusedParameter.Local
  private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
  // ReSharper restore UnusedParameter.Local
  // ReSharper restore UnusedMember.Local
  {
    var result = DialogResult.Abort;
    try
    {
      result = MessageBox.Show(string.Format("{0}\n\n{1}{2}", Resources.UnhandledException, e.Exception.Message, e.Exception.StackTrace), Resources.ApplicationError, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
    }
    finally
    {
      if (result == DialogResult.Abort)
      {
        Application.Exit();
      }
    }
  }


  #endregion

  #region External process start / stop handling
  
  /// <summary>
  /// 
  /// </summary>
  /// <param name="proc"></param>
  /// <param name="waitForExit"></param>
  public void OnStartExternal(Process proc, bool waitForExit)
  {
    if (TopMost && waitForExit)
    {
      TopMost = false;
      _restoreTopMost = true;
      MinimizeToTray();
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="proc"></param>
  /// <param name="waitForExit"></param>
  public void OnStopExternal(Process proc, bool waitForExit)
  {
    if (_restoreTopMost)
    {
      RestoreFromTray();
      TopMost = true;
      _restoreTopMost = false;
    }
  }

  #endregion
  
  #region helper funcs

  /// <summary>
  /// 
  /// </summary>
  private void CreateStateBlock()
  {
    DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_CULLMODE, (int)D3DCULL.D3DCULL_NONE);
    DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_LIGHTING, 0);
    DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_ZENABLE, 1);
    DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_FOGENABLE, 0);
    DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_FILLMODE, (int)D3DFILLMODE.D3DFILL_SOLID);
    DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_SRCBLEND, (int)D3DBLEND.D3DBLEND_SRCALPHA);
    DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_DESTBLEND, (int)D3DBLEND.D3DBLEND_INVSRCALPHA);

    DXNative.FontEngineSetTextureStageState(0, (int)D3DTEXTURESTAGESTATETYPE.D3DTSS_COLOROP, (int)D3DTEXTUREOP.D3DTOP_MODULATE);
    DXNative.FontEngineSetTextureStageState(0, (int)D3DTEXTURESTAGESTATETYPE.D3DTSS_COLORARG1, (int)D3DTA.D3DTA_TEXTURE);
    DXNative.FontEngineSetTextureStageState(0, (int)D3DTEXTURESTAGESTATETYPE.D3DTSS_COLORARG2, (int)D3DTA.D3DTA_DIFFUSE);
    DXNative.FontEngineSetTextureStageState(0, (int)D3DTEXTURESTAGESTATETYPE.D3DTSS_ALPHAOP, (int)D3DTEXTUREOP.D3DTOP_MODULATE);
    DXNative.FontEngineSetTextureStageState(0, (int)D3DTEXTURESTAGESTATETYPE.D3DTSS_ALPHAARG1, (int)D3DTA.D3DTA_TEXTURE);
    DXNative.FontEngineSetTextureStageState(0, (int)D3DTEXTURESTAGESTATETYPE.D3DTSS_ALPHAARG2, (int)D3DTA.D3DTA_DIFFUSE);

    if (_supportsFiltering)
    {
      DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MINFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
      DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MAGFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
      DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MIPFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
      DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MAXANISOTROPY, (uint)_anisotropy);

      DXNative.FontEngineSetSamplerState(1, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MINFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
      DXNative.FontEngineSetSamplerState(1, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MAGFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
      DXNative.FontEngineSetSamplerState(1, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MIPFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_LINEAR);
      DXNative.FontEngineSetSamplerState(1, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MAXANISOTROPY, (uint)_anisotropy);
    }
    else
    {
      DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MINFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_POINT);
      DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MAGFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_POINT);
      DXNative.FontEngineSetSamplerState(0, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MIPFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_POINT);

      DXNative.FontEngineSetSamplerState(1, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MINFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_POINT);
      DXNative.FontEngineSetSamplerState(1, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MAGFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_POINT);
      DXNative.FontEngineSetSamplerState(1, (int)D3DSAMPLERSTATETYPE.D3DSAMP_MIPFILTER, (int)D3DTEXTUREFILTERTYPE.D3DTEXF_POINT);
    }
    if (_supportsAlphaBlend)
    {
      DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_ALPHATESTENABLE, 1);
      DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_ALPHAREF, 1);
      DXNative.FontEngineSetRenderState((int)D3DRENDERSTATETYPE.D3DRS_ALPHAFUNC, (int)D3DCMPFUNC.D3DCMP_GREATEREQUAL);
    }
  }


  /// <summary>
  /// Get the current date from the system and localize it based on the user preferences.
  /// </summary>
  /// <returns>A string containing the localized version of the date.</returns>
  protected string GetDate()
  {
    string dateString = _dateFormat;
    if (!string.IsNullOrEmpty(dateString))
    {
      DateTime cur = DateTime.Now;
      string day;
      switch (cur.DayOfWeek)
      {
        case DayOfWeek.Monday:
          day = GUILocalizeStrings.Get(11);
          break;
        case DayOfWeek.Tuesday:
          day = GUILocalizeStrings.Get(12);
          break;
        case DayOfWeek.Wednesday:
          day = GUILocalizeStrings.Get(13);
          break;
        case DayOfWeek.Thursday:
          day = GUILocalizeStrings.Get(14);
          break;
        case DayOfWeek.Friday:
          day = GUILocalizeStrings.Get(15);
          break;
        case DayOfWeek.Saturday:
          day = GUILocalizeStrings.Get(16);
          break;
        default:
          day = GUILocalizeStrings.Get(17);
          break;
      }

      string month;
      switch (cur.Month)
      {
        case 1:
          month = GUILocalizeStrings.Get(21);
          break;
        case 2:
          month = GUILocalizeStrings.Get(22);
          break;
        case 3:
          month = GUILocalizeStrings.Get(23);
          break;
        case 4:
          month = GUILocalizeStrings.Get(24);
          break;
        case 5:
          month = GUILocalizeStrings.Get(25);
          break;
        case 6:
          month = GUILocalizeStrings.Get(26);
          break;
        case 7:
          month = GUILocalizeStrings.Get(27);
          break;
        case 8:
          month = GUILocalizeStrings.Get(28);
          break;
        case 9:
          month = GUILocalizeStrings.Get(29);
          break;
        case 10:
          month = GUILocalizeStrings.Get(30);
          break;
        case 11:
          month = GUILocalizeStrings.Get(31);
          break;
        default:
          month = GUILocalizeStrings.Get(32);
          break;
      }

      dateString = Utils.ReplaceTag(dateString, "<Day>", day, "unknown");
      dateString = Utils.ReplaceTag(dateString, "<DD>", cur.Day.ToString(CultureInfo.InvariantCulture), "unknown");
      dateString = Utils.ReplaceTag(dateString, "<Month>", month, "unknown");
      dateString = Utils.ReplaceTag(dateString, "<MM>", cur.Month.ToString(CultureInfo.InvariantCulture), "unknown");
      dateString = Utils.ReplaceTag(dateString, "<Year>", cur.Year.ToString(CultureInfo.InvariantCulture), "unknown");
      dateString = Utils.ReplaceTag(dateString, "<YY>", (cur.Year - 2000).ToString("00"), "unknown");
      GUIPropertyManager.SetProperty("#date", dateString);
      return dateString;
    }
    return string.Empty;
  }


  /// <summary>
  /// Get the current time from the system. Set the format in the Home plugin's config
  /// </summary>
  /// <returns>A string containing the current time.</returns>
  protected string GetTime()
  {
    return DateTime.Now.ToString(_useLongDateFormat 
      ? Thread.CurrentThread.CurrentCulture.DateTimeFormat.LongTimePattern 
      : Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetDay()
  {
    DateTime cur = DateTime.Now;
    return String.Format("{0}", cur.Day);
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetShortDayOfWeek()
  {
    DateTime cur = DateTime.Now;
    string ddd;
    switch (cur.DayOfWeek)
    {
      case DayOfWeek.Monday:
        ddd = GUILocalizeStrings.Get(657);
        break;
      case DayOfWeek.Tuesday:
        ddd = GUILocalizeStrings.Get(658);
        break;
      case DayOfWeek.Wednesday:
        ddd = GUILocalizeStrings.Get(659);
        break;
      case DayOfWeek.Thursday:
        ddd = GUILocalizeStrings.Get(660);
        break;
      case DayOfWeek.Friday:
        ddd = GUILocalizeStrings.Get(661);
        break;
      case DayOfWeek.Saturday:
        ddd = GUILocalizeStrings.Get(662);
        break;
      default:
        ddd = GUILocalizeStrings.Get(663);
        break;
    }
    return ddd;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetDayOfWeek()
  {
    DateTime cur = DateTime.Now;
    string dddd;
    switch (cur.DayOfWeek)
    {
      case DayOfWeek.Monday:
        dddd = GUILocalizeStrings.Get(11);
        break;
      case DayOfWeek.Tuesday:
        dddd = GUILocalizeStrings.Get(12);
        break;
      case DayOfWeek.Wednesday:
        dddd = GUILocalizeStrings.Get(13);
        break;
      case DayOfWeek.Thursday:
        dddd = GUILocalizeStrings.Get(14);
        break;
      case DayOfWeek.Friday:
        dddd = GUILocalizeStrings.Get(15);
        break;
      case DayOfWeek.Saturday:
        dddd = GUILocalizeStrings.Get(16);
        break;
      default:
        dddd = GUILocalizeStrings.Get(17);
        break;
    }
    return dddd;
  }

  protected string GetMonth()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("MM");
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetShortMonthOfYear()
  {
    string smoy = GetMonthOfYear();
    smoy = smoy.Substring(0, 3);
    return smoy;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetMonthOfYear()
  {
    DateTime cur = DateTime.Now;
    string mmmm;
    switch (cur.Month)
    {
      case 1:
        mmmm = GUILocalizeStrings.Get(21);
        break;
      case 2:
        mmmm = GUILocalizeStrings.Get(22);
        break;
      case 3:
        mmmm = GUILocalizeStrings.Get(23);
        break;
      case 4:
        mmmm = GUILocalizeStrings.Get(24);
        break;
      case 5:
        mmmm = GUILocalizeStrings.Get(25);
        break;
      case 6:
        mmmm = GUILocalizeStrings.Get(26);
        break;
      case 7:
        mmmm = GUILocalizeStrings.Get(27);
        break;
      case 8:
        mmmm = GUILocalizeStrings.Get(28);
        break;
      case 9:
        mmmm = GUILocalizeStrings.Get(29);
        break;
      case 10:
        mmmm = GUILocalizeStrings.Get(30);
        break;
      case 11:
        mmmm = GUILocalizeStrings.Get(31);
        break;
      default:
        mmmm = GUILocalizeStrings.Get(32);
        break;
    }
    return mmmm;
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetShortYear()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("yy");
  }


  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  protected string GetYear()
  {
    DateTime cur = DateTime.Now;
    return cur.ToString("yyyy");
  }

  /// <summary>
  /// 
  /// </summary>
  protected void CheckSkinVersion()
  {
    bool ignoreErrors;
    using (Settings xmlreader = new MPSettings())
    {
      ignoreErrors = xmlreader.GetValueAsBool("general", "dontshowskinversion", false);
    }

    if (!ignoreErrors)
    {
      Version versionSkin = null;
      string filename = GUIGraphicsContext.GetThemedSkinFile(@"\references.xml");
      if (File.Exists(filename))
      {
        var doc = new XmlDocument();
        doc.Load(filename);
        XmlNode node = doc.SelectSingleNode("/controls/skin/version");
        if (node != null)
        {
          versionSkin = new Version(node.InnerText);
        }
      }

      if (CompatibilityManager.SkinVersion != versionSkin)
      {
        _outdatedSkinName = GUIGraphicsContext.SkinName;
        float screenHeight = GUIGraphicsContext.currentScreen.Bounds.Height;
        float screenWidth = GUIGraphicsContext.currentScreen.Bounds.Width;
        float screenRatio = (screenWidth/screenHeight);
        GUIGraphicsContext.Skin = screenRatio > 1.5 ? "Titan" : "Default";
        Config.SkinName = GUIGraphicsContext.SkinName;
        SkinSettings.Load();

        // Send a message that the skin has changed.
        var msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_SKIN_CHANGED, 0, 0, 0, 0, 0, null);
        GUIGraphicsContext.SendMessage(msg);

        Log.Info("Main: User skin is not compatible, using skin {0} with theme {1}", GUIGraphicsContext.SkinName, GUIThemeManager.CurrentTheme);
      }
    }
  }


  /// <summary>
  /// XP returns random garbage in a name of a display in .NET
  /// </summary>
  /// <param name="screen"></param>
  /// <returns></returns>
  protected static string GetCleanDisplayName(Screen screen)
  {
    if (OSInfo.OSInfo.VistaOrLater())
    {
      return screen.DeviceName;
    }

    int length = screen.DeviceName.IndexOf("\0", StringComparison.Ordinal);
    string deviceName = length == -1 ? screen.DeviceName : screen.DeviceName.Substring(0, length);
    return deviceName;
  }

  #endregion

  #region registry helper function

  /// <summary>
  /// 
  /// </summary>
  /// <param name="hklm"></param>
  /// <param name="key"></param>
  /// <param name="value"></param>
  /// <param name="iValue"></param>
  public static void SetDWORDRegKey(RegistryKey hklm, string key, string value, Int32 iValue)
  {
    try
    {
      using (RegistryKey subkey = hklm.CreateSubKey(key))
      {
        if (subkey != null)
        {
          subkey.SetValue(value, iValue);
        }
      }
    }
    catch (SecurityException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
    catch (UnauthorizedAccessException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
  }


  /// <summary>
  /// 
  /// </summary>
  /// <param name="hklm"></param>
  /// <param name="key"></param>
  /// <param name="name"></param>
  /// <param name="value"></param>
  public static void SetREGSZRegKey(RegistryKey hklm, string key, string name, string value)
  {
    try
    {
      using (RegistryKey subkey = hklm.CreateSubKey(key))
      {
        if (subkey != null)
        {
          subkey.SetValue(name, value);
        }
      }
    }
    catch (SecurityException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
    catch (UnauthorizedAccessException)
    {
      Log.Error(@"User does not have sufficient rights to modify registry key HKLM\{0}", key);
    }
  }

  #endregion
}
