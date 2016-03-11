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

#region Usings

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace TvEngine.PowerScheduler.Interfaces
{
  /// <summary>
  /// Provides static methods for managing system power states, events and settings
  /// </summary>
  public static class PowerManager
  {
    #region Variables

    #endregion

    #region System power structures and enumerations

    /// <summary>
    /// The thread's execution requirements
    /// </summary>
    [Flags]
    enum ExecutionState : uint
    {
      /// <summary>
      /// Some error.
      /// </summary>
      Error = 0,

      /// <summary>
      /// System is required, do not hibernate.
      /// </summary>
      ES_SYSTEM_REQUIRED = 0x00000001,

      /// <summary>
      /// Display is required, do not hibernate.
      /// </summary>
      ES_DISPLAY_REQUIRED = 0x00000002,

      /// <summary>
      /// User is active, do not hibernate.
      /// </summary>
      ES_USER_PRESENT = 0x00000004,

      /// <summary>
      /// Enables away mode.
      /// </summary>
      ES_AWAYMODE_REQUIRED = 0x00000040,

      /// <summary>
      /// Use together with the above options to report a
      /// state until explicitly changed.
      /// </summary>
      ES_CONTINUOUS = 0x80000000
    }

    /// <summary>
    /// Flags to register for power setting change notification
    /// </summary>
    const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
    const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;

    /// <summary>
    /// Constants for power notifications
    /// </summary>
    public const int WM_POWERBROADCAST = 0x0218;
    public const int PBT_APMQUERYSUSPEND = 0x0000;
    public const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
    public const int PBT_APMSUSPEND = 0x0004;
    public const int PBT_APMRESUMECRITICAL = 0x0006;
    public const int PBT_APMRESUMESUSPEND = 0x0007;
    public const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    public const int PBT_POWERSETTINGCHANGE = 0x8013;
    public const int BROADCAST_QUERY_DENY = 0x424D5144;

    /// <summary>
    /// This structure is sent when the PBT_POWERSETTINGSCHANGE message is sent.
    /// It describes the power setting that has changed and contains data about the change
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct POWERBROADCAST_SETTING
    {
      public Guid PowerSetting;
      public uint DataLength;
      public byte Data;
    }

    /// <summary>
    /// Guid to register for power setting change notification
    /// </summary>
    public static Guid GUID_SYSTEM_AWAYMODE = new Guid("98a7f580-01f7-48aa-9c0f-44352c29e5C0");

    /// <summary>
    /// System power setting guids
    /// </summary>
    static Guid NO_SUBGROUP_GUID = new Guid("fea3413e-7e05-4911-9a71-700331f1c294");
    static Guid GUID_LOCK_CONSOLE_ON_WAKE = new Guid("0e796bdb-100d-47d6-a2d5-f7d2daa51f51");
    static Guid GUID_SUB_SLEEP = new Guid("238c9fa8-0aad-41ed-83f4-97be242c8f20");
    static Guid GUID_ALLOW_AWAY_MODE = new Guid("25dfa149-5dd1-4736-b5ab-e8a37b5b8187");
    static Guid GUID_SLEEP_AFTER = new Guid("29f6c1db-86da-48c5-9fdb-f2b67b1f44da");
    static Guid GUID_ALLOW_HYBRID_SLEEP = new Guid("94ac6d29-73ce-41a6-809f-6363ba21b47e");
    static Guid GUID_HIBERNATE_AFTER = new Guid("9d7815a6-7ee4-497e-8888-515a05f02364");
    static Guid GUID_ALLOW_RTC_WAKE = new Guid("bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d");
    static Guid GUID_SUB_POWER_BUTTONS_AND_LID = new Guid("4f971e89-eebd-4455-a8de-9e59040e7347");
    static Guid GUID_LID_CLOSE_ACTION = new Guid("5ca83367-6e45-459f-a27b-476b1d01c936");
    static Guid GUID_POWER_BUTTON_ACTION = new Guid("7648efa3-dd9c-4e3e-b566-50f929386280");
    static Guid GUID_SLEEP_BUTTON_ACTION = new Guid("96996bc0-ad50-47ec-923b-6f41874dd9eb");
    static Guid GUID_SUB_MULTIMEDIA = new Guid("9596fb26-9850-41fd-ac3e-f7c3c00afd4b");
    static Guid GUID_WHEN_SHARING_MEDIA = new Guid("03680956-93bc-4294-bba6-4e0f09bb717f");

    /// <summary>
    /// Contains information about the power capabilities of the system.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEM_POWER_CAPABILITIES
    {
      [MarshalAs(UnmanagedType.I1)]
      public bool PowerButtonPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool SleepButtonPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool LidPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS1;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS2;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS3;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS4;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS5;
      [MarshalAs(UnmanagedType.I1)]
      public bool HiberFilePresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool FullWake;
      [MarshalAs(UnmanagedType.I1)]
      public bool VideoDimPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool ApmPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool UpsPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool ThermalControl;
      [MarshalAs(UnmanagedType.I1)]
      public bool ProcessorThrottle;
      public byte ProcessorMinimumThrottle;
      public byte ProcessorMaximumThrottle;
      [MarshalAs(UnmanagedType.I1)]
      public bool FastSystemS4;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public byte[] spare2;
      [MarshalAs(UnmanagedType.I1)]
      public bool DiskSpinDown;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public byte[] spare3;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemBatteriesPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool BatteriesAreShortTerm;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public BATTERY_REPORTING_SCALE[] BatteryScale;
      public SYSTEM_POWER_STATE AcOnlineWake;
      public SYSTEM_POWER_STATE SoftLidWake;
      public SYSTEM_POWER_STATE RtcWake;
      public SYSTEM_POWER_STATE MinimumDeviceWakeState;
      public SYSTEM_POWER_STATE DefaultLowLatencyWake;
    }

    /// <summary>
    /// Contains the granularity of the battery capacity that is reported by IOCTL_BATTERY_QUERY_STATUS.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct BATTERY_REPORTING_SCALE
    {
      public UInt32 Granularity;
      public UInt32 Capacity;
    }

    /// <summary>
    /// Contains information about the power status of the system.
    /// </summary>
    struct SYSTEM_POWER_STATUS
    {
      public ACLineStatus ACLineStatus;
      public BatteryFlag BatteryFlag;
      public Byte BatteryLifePercent;
      public Byte Reserved1;
      public Int32 BatteryLifeTime;
      public Int32 BatteryFullLifeTime;
    }
    
    /// <summary>
    /// The AC power status.
    /// </summary>
    enum ACLineStatus : byte
    {
      Offline = 0,
      Online = 1,
      Unknown = 255
    }

    /// <summary>
    /// The battery charge status.
    /// </summary>
    enum BatteryFlag : byte
    {
      High = 1,
      Low = 2,
      Critical = 4,
      Charging = 8,
      NoSystemBattery = 128,
      Unknown = 255
    }

    /// <summary>
    /// Contains information used to set the system power state.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    struct POWER_ACTION_POLICY
    {
      public POWER_ACTION Action;
      public PowerActionFlags Flags;
      public PowerActionEventCode EventCode;
    }
    
    /// <summary>
    /// Defines values that are used to specify system power action types.
    /// </summary>
    enum POWER_ACTION : uint
    {
      PowerActionNone = 0,       // No system power action.
      PowerActionReserved,       // Reserved; do not use.
      PowerActionSleep,          // Sleep.
      PowerActionHibernate,      // Hibernate.
      PowerActionShutdown,       // Shutdown.
      PowerActionShutdownReset,  // Shutdown and reset.
      PowerActionShutdownOff,    // Shutdown and power off.
      PowerActionWarmEject,      // Warm eject.
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    enum PowerActionFlags : uint
    {
      POWER_ACTION_QUERY_ALLOWED  = 0x00000001, // Broadcasts a PBT_APMQUERYSUSPEND event to each application to request permission to suspend operation.
      POWER_ACTION_UI_ALLOWED     = 0x00000002, // Applications can prompt the user for directions on how to prepare for suspension. Sets bit 0 in the Flags parameter passed in the lParam parameter of WM_POWERBROADCAST.
      POWER_ACTION_OVERRIDE_APPS  = 0x00000004, // Ignores applications that do not respond to the PBT_APMQUERYSUSPEND event broadcast in the WM_POWERBROADCAST message.
      POWER_ACTION_LIGHTEST_FIRST = 0x10000000, // Uses the first lightest available sleep state.
      POWER_ACTION_LOCK_CONSOLE   = 0x20000000, // Requires entry of the system password upon resume from one of the system standby states.
      POWER_ACTION_DISABLE_WAKES  = 0x40000000, // Disables all wake events.
      POWER_ACTION_CRITICAL       = 0x80000000, // Forces a critical suspension.
    }

    [Flags]
    enum PowerActionEventCode : uint
    {
      POWER_LEVEL_USER_NOTIFY_TEXT  = 0x00000001, // User notified using the UI.
      POWER_LEVEL_USER_NOTIFY_SOUND = 0x00000002, // User notified using sound.
      POWER_LEVEL_USER_NOTIFY_EXEC  = 0x00000004, // Specifies a program to be executed.
      POWER_USER_NOTIFY_BUTTON      = 0x00000008, // Indicates that the power action is in response to a user power button press.
      POWER_USER_NOTIFY_SHUTDOWN    = 0x00000010, // Indicates a power action of shutdown/off.
      POWER_FORCE_TRIGGER_RESET     = 0x80000000, // Clears a user power button press.
    }

    /// <summary>
    /// The global flags constants are used to enable or disable user power policy options
    /// </summary>
    [Flags]
    enum GlobalFlags : uint
    {
      EnableMultiBatteryDisplay = 0x02,   // Enables or disables multiple battery display in the system Power Meter.
      EnablePasswordLogon = 0x04,         // Enables or disables requiring password logon when the system resumes from standby or hibernate.
      EnableSysTrayBatteryMeter = 0x01,   // Enables or disables the battery meter icon in the system tray. When this flag is cleared, the battery meter icon is not displayed.
      EnableVideoDimDisplay = 0x10,       // Enables or disables support for dimming the video display when the system changes from running on AC power to running on battery power.
      EnableWakeOnRing = 0x08,            // Enables or disables wake on ring support.
    }

    /// <summary>
    /// Contains power policy settings that are unique to each power scheme.
    /// </summary>
    struct POWER_POLICY
    {
      public USER_POWER_POLICY user;
      public MACHINE_POWER_POLICY mach;
    }

    /// <summary>
    /// Contains power policy settings that are unique to each power scheme for a user.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct USER_POWER_POLICY {
      public uint Revision;
      public POWER_ACTION_POLICY IdleAc;
      public POWER_ACTION_POLICY IdleDc;
      public uint IdleTimeoutAc;
      public uint IdleTimeoutDc;
      public byte IdleSensitivityAc;
      public byte IdleSensitivityDc;
      public byte ThrottlePolicyAc;
      public byte ThrottlePolicyDc;
      public SYSTEM_POWER_STATE MaxSleepAc;
      public SYSTEM_POWER_STATE MaxSleepDc;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
      public uint[] Reserved;
      public uint VideoTimeoutAc;
      public uint VideoTimeoutDc;
      public uint SpindownTimeoutAc;
      public uint SpindownTimeoutDc;
      [MarshalAs(UnmanagedType.I1)]
      public bool OptimizeForPowerAc;
      [MarshalAs(UnmanagedType.I1)]
      public bool OptimizeForPowerDc;
      public byte FanThrottleToleranceAc;
      public byte FanThrottleToleranceDc;
      public byte ForcedThrottleAc;
      public byte ForcedThrottleDc;
    }

    /// <summary>
    /// Contains computer power policy settings that are unique to each power scheme on the computer.
    /// </summary>
    struct MACHINE_POWER_POLICY {
      public uint Revision;
      public SYSTEM_POWER_STATE MinSleepAc;
      public SYSTEM_POWER_STATE MinSleepDc;
      public SYSTEM_POWER_STATE ReducedLatencySleepAc;
      public SYSTEM_POWER_STATE ReducedLatencySleepDc;
      public uint DozeTimeoutAc;
      public uint DozeTimeoutDc;
      public uint DozeS4TimeoutAc;
      public uint DozeS4TimeoutDc;
      public byte MinThrottleAc;
      public byte MinThrottleDc;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
      public byte[] pad1;
      public POWER_ACTION_POLICY OverThrottledAc;
      public POWER_ACTION_POLICY OverThrottledDc;
    }

    /// <summary>
    /// Contains global power policy settings that apply to all power schemes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GLOBAL_POWER_POLICY
    {
      public GLOBAL_USER_POWER_POLICY user;
      public GLOBAL_MACHINE_POWER_POLICY mach;
    }

    /// <summary>
    /// Contains global user power policy settings that apply to all power schemes for a user.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GLOBAL_USER_POWER_POLICY
    {
      public const int NUM_DISCHARGE_POLICIES = 4;

      public uint Revision;
      public POWER_ACTION_POLICY PowerButtonAc;
      public POWER_ACTION_POLICY PowerButtonDc;
      public POWER_ACTION_POLICY SleepButtonAc;
      public POWER_ACTION_POLICY SleepButtonDc;
      public POWER_ACTION_POLICY LidCloseAc;
      public POWER_ACTION_POLICY LidCloseDc;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = NUM_DISCHARGE_POLICIES)]
      public SYSTEM_POWER_LEVEL[] DischargePolicy;
      public GlobalFlags GlobalFlags;
    }

    /// <summary>
    /// Contains information about system battery drain policy settings.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SYSTEM_POWER_LEVEL
    {
      public bool Enable;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public byte[] Spare;
      public uint BatteryLevel;
      public POWER_ACTION_POLICY PowerPolicy;
      public SYSTEM_POWER_STATE MinSystemState;
    }
    
    /// <summary>
    /// Contains global computer power policy settings that apply to all power schemes for all users.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GLOBAL_MACHINE_POWER_POLICY
    {
      public uint Revision;
      public SYSTEM_POWER_STATE LidOpenWakeAc;
      public SYSTEM_POWER_STATE LidOpenWakeDc;
      public uint BroadcastCapacityResolution;
    }

    /// <summary>
    /// Defines values that are used to specify system power states.
    /// </summary>
    enum SYSTEM_POWER_STATE
    {
      PowerSystemUnspecified = 0,
      PowerSystemWorking = 1,
      PowerSystemSleeping1 = 2,
      PowerSystemSleeping2 = 3,
      PowerSystemSleeping3 = 4,
      PowerSystemHibernate = 5,
      PowerSystemShutdown = 6,
      PowerSystemMaximum = 7
    }
    
    #endregion

    #region Public methods

    /// <summary>
    /// Power setting type (index to PowerSettings)
    /// </summary>
    public enum SystemPowerSettingType
    {
      LOCK_CONSOLE_ON_WAKE,
      ALLOW_AWAY_MODE,
      STANDBYIDLE,
      ALLOW_HYBRID_SLEEP,
      HIBERNATE_AFTER,
      ALLOW_RTC_WAKE,
      LID_CLOSE_ACTION,
      POWER_BUTTON_ACTION,
      SLEEP_BUTTON_ACTION,
      WHEN_SHARING_MEDIA
    }
    
    /// <summary>
    /// Reset the system idle timeout to prevent standby
    /// </summary>
    public static void ResetIdleTimer()
    {
      // ES_SYSTEM_REQUIRED without ES_CONTINUOS resets the idle timeout
      SetThreadExecutionState(ExecutionState.ES_SYSTEM_REQUIRED);
    }

    /// <summary>
    /// Sets thread execution state to allow / prevent standby (always must be called by one and the same thread)
    /// </summary>
    /// <param name="awayModeRequired">Enable/disable away mode</param>
    public static void SetStandbyMode(StandbyMode standbyMode)
    {
      switch (standbyMode)
      {
        case StandbyMode.StandbyAllowed:
          SetThreadExecutionState(ExecutionState.ES_CONTINUOUS);
          break;
        case StandbyMode.StandbyPrevented:
          SetThreadExecutionState(ExecutionState.ES_SYSTEM_REQUIRED | ExecutionState.ES_CONTINUOUS);
          break;
        case StandbyMode.AwayModeRequested:
          if (Environment.OSVersion.Version.Major >= 6)
            SetThreadExecutionState(ExecutionState.ES_SYSTEM_REQUIRED | ExecutionState.ES_AWAYMODE_REQUIRED | ExecutionState.ES_CONTINUOUS);
          else
            SetThreadExecutionState(ExecutionState.ES_SYSTEM_REQUIRED | ExecutionState.ES_CONTINUOUS);
          break;
      }
    }

    /// <summary>
    /// Get power setting value for active power status (OS independent)
    /// </summary>
    /// <param name="settingType"></param>
    /// <returns>Returns value of power setting requested. Returns 0 on errors.</returns>
    public static int GetActivePowerSetting(SystemPowerSettingType settingType)
    {
      if (Environment.OSVersion.Version.Major >= 6)
        return GetSystemPowerSetting(settingType, ACPowerPluggedIn);
      else
        return GetPowerPolicySetting(settingType, ACPowerPluggedIn);
    }

    /// <summary>
    /// Get power setting value for indicated power status (OS independent)
    /// </summary>
    /// <param name="settingType"></param>
    /// <param name="AC"></param>
    /// <returns>Returns value of power setting requested. Returns 0 on errors.</returns>
    public static int GetPowerSetting(SystemPowerSettingType settingType, bool AC)
    {
      if (Environment.OSVersion.Version.Major >= 6)
        return GetSystemPowerSetting(settingType, AC);
      else
        return GetPowerPolicySetting(settingType, AC);
    }

    /// <summary>
    /// Set power setting value (OS independent)
    /// </summary>
    /// <param name="settingType"></param>
    /// <param name="AC"></param>
    /// <param name="value"></param>
    public static void SetPowerSetting(SystemPowerSettingType settingType, bool AC, uint value)
    {
      if (Environment.OSVersion.Version.Major >= 6)
        SetSystemPowerSetting(settingType, AC, value);
      else
        SetPowerPolicySetting(settingType, AC, value);
    }

    /// <summary>
    ///  Checks if the given hostname/IP address is the local host
    /// </summary>
    /// <param name="serverName">hostname/IP address to check</param>
    /// <returns>is this name/address local?</returns>
    public static bool IsLocal(string serverName)
    {
      foreach (string name in new string[] { "localhost", "127.0.0.1", Dns.GetHostName() })
      {
        if (serverName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
          return true;
      }
      IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
      foreach (IPAddress address in hostEntry.AddressList)
      {
        if (address.ToString().Equals(serverName, StringComparison.CurrentCultureIgnoreCase))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Get "powercfg /requests" output (only on Win7 or higher)
    /// </summary>
    /// <returns>Output of "powercfg /requests" command</returns>
    public static string GetPowerCfgRequests(bool showTvService)
    {
      // Windows XP / 2003
      if (Environment.OSVersion.Version.Major < 6)
        return null;
      // Vista
      if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 0)
        return null;

      string output, requests = string.Empty;

      // Disable WOW64 redirection for this thread on 64-bit systems to call the 64-bit powercfg
      IntPtr oldValue = IntPtr.Zero;
      if (IsOS64Bit())
      {
        Wow64DisableWow64FsRedirection(ref oldValue);
      }

      // Run "powercfg /requests"
      using (Process p = new Process())
      {
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.StandardOutputEncoding = Encoding.ASCII;
        p.StartInfo.FileName = @"powercfg.exe";
        p.StartInfo.Arguments = "/requests";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        p.StartInfo.Verb = "runas";

        p.Start();
        output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
      }

      // Re-enable WOW64 redirection
      if (IsOS64Bit())
      {
        Wow64RevertWow64FsRedirection(oldValue);
      }

      // Process the output
      using (StringReader reader = new StringReader(output))
      {
        string line;

        // Skip lines until "SYSTEM:"
        while (true)
        {
          line = reader.ReadLine();
          if (line == null || line.StartsWith("SYSTEM:"))
            break;
        }

        // Save lines of interest
        while (line != null)
        {
          line = reader.ReadLine();
          if (line.StartsWith("AWAYMODE:"))
            break;
          if (!line.StartsWith("[DRIVER]") && !line.StartsWith("[PROCESS]") && !line.StartsWith("[SERVICE]"))
            continue;
          if (!showTvService && line.IndexOf("TvService.exe") >= 0)
            continue;

          if (string.IsNullOrEmpty(requests))
            requests = line;
          else
            requests += Environment.NewLine + line;
        }
      }
      return requests;
    }

    #endregion

    #region Public properties

    /// <summary>
    /// Check if running on AC Power
    /// </summary>
    /// <returns>Returns true if running on AC Power</returns>
    public static bool RunningOnAC
    {
      get
      {
        SYSTEM_POWER_STATUS sps = new SYSTEM_POWER_STATUS();
        try
        {
          GetSystemPowerStatus(ref sps);
          return (sps.ACLineStatus != ACLineStatus.Offline);
        }
        catch (Exception)
        {
          return true; // Fallback: running on AC power
        }
      }
    }

    /// <summary>
    /// Check DC Power source
    /// </summary>
    /// <returns>Returns true if there is a DC Power source</returns>
    public static bool HasDCPowerSource
    {
      get
      {
        SYSTEM_POWER_STATUS sps = new SYSTEM_POWER_STATUS();
        try
        {
          GetSystemPowerStatus(ref sps);
          return (((byte)sps.BatteryFlag & (byte)BatteryFlag.NoSystemBattery) == 0);
        }
        catch (Exception)
        {
          return false; // Fallback: no DC power source
        }
      }
    }

    /// <summary>
    /// Check system lid
    /// </summary>
    /// <returns>Returns true if lid is present</returns>
    public static bool HasLid
    {
      get
      {
        SYSTEM_POWER_CAPABILITIES spc = new SYSTEM_POWER_CAPABILITIES();
        try
        {
          GetPwrCapabilities(ref spc);
          return (spc.LidPresent);
        }
        catch (Exception)
        {
          return false;  // Fallback: No lid
        }
      }
    }

    /// <summary>
    /// Check if system can hibernate
    /// </summary>
    /// <returns>Returns true if lid is present</returns>
    public static bool CanHibernate
    {
      get
      {
        SYSTEM_POWER_CAPABILITIES spc = new SYSTEM_POWER_CAPABILITIES();
        try
        {
          GetPwrCapabilities(ref spc);
          return (spc.SystemS4 && spc.HiberFilePresent);
        }
        catch (Exception)
        {
          return false;  // Fallback: Cannot hibernate
        }
      }
    }

    #endregion

    #region System power management functions imports

    /// <summary>
    /// Enables an application to inform the system that it is in use, thereby preventing the system
    /// from entering sleep or turning off the display while the application is running.
    /// </summary>
    /// <param name="esFlags">The thread's execution requirements
    /// /// </param>
    /// <returns>
    /// If the function succeeds, the return value is the previous thread execution state.
    /// If the function fails, the return value is NULL.
    /// </returns>
    [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "SetThreadExecutionState")]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    /// <summary>
    /// Retrieves the power status of the system.
    /// </summary>
    /// <param name="__out  LPSYSTEM_POWER_STATUS lpSystemPowerStatus">A pointer to a SYSTEM_POWER_STATUS structure that receives status information.</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemPowerStatus(ref SYSTEM_POWER_STATUS lpSystemPowerStatus);

    /// <summary>
    /// Retrieves information about the system power capabilities.
    /// </summary>
    /// <param name="__out  PSYSTEM_POWER_CAPABILITIES lpSystemPowerCapabilities"></param>
    /// <returns></returns>
    [DllImport("powrprof.dll", SetLastError = true)]
    static extern bool GetPwrCapabilities(ref SYSTEM_POWER_CAPABILITIES lpSystemPowerCapabilities);

    /// <summary>
    /// Retrieves the active power scheme and returns a GUID that identifies the scheme.
    /// </summary>
    /// <param name="__in_opt  HKEY UserRootPowerKey">This parameter is reserved for future use and must be set to NULL.</param>
    /// <param name="__out     GUID **ActivePolicyGuid">A pointer that receives a pointer to a GUID structure. Use the LocalFree function to free this memory.</param>
    /// <returns></returns>
    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern UInt32 PowerGetActiveScheme(IntPtr RootPowerKey, ref IntPtr activePolicyGuid);

    /// <summary>
    /// Retrieves the AC index of the specified power setting.
    /// </summary>
    /// <param name="rootPowerKey"></param>
    /// <param name="schemeGuid"></param>
    /// <param name="subgroupOfPowerSettingsGuid"></param>
    /// <param name="powerSettingGuid"></param>
    /// <param name="valueIndex"></param>
    /// <returns></returns>
    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern UInt32 PowerReadACValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
      ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, ref UInt32 AcValueIndex);

    /// <summary>
    /// Writes the AC index of the specified power setting.
    /// </summary>
    /// <param name="rootPowerKey"></param>
    /// <param name="schemeGuid"></param>
    /// <param name="subgroupOfPowerSettingsGuid"></param>
    /// <param name="powerSettingGuid"></param>
    /// <param name="valueIndex"></param>
    /// <returns></returns>
    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern UInt32 PowerWriteACValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
      ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, UInt32 AcValueIndex);

    /// <summary>
    /// Retrieves the DC index of the specified power setting.
    /// </summary>
    /// <param name="rootPowerKey"></param>
    /// <param name="schemeGuid"></param>
    /// <param name="subgroupOfPowerSettingsGuid"></param>
    /// <param name="powerSettingGuid"></param>
    /// <param name="valueIndex"></param>
    /// <returns></returns>
    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern UInt32 PowerReadDCValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
      ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, ref UInt32 DcValueIndex);

    /// <summary>
    /// Writes the DC index of the specified power setting.
    /// </summary>
    /// <param name="__in_opt  HKEY RootPowerKey"></param>
    /// <param name="__in      const GUID *SchemeGuid"></param>
    /// <param name="__in_opt  const GUID *SubGroupOfPowerSettingsGuid"></param>
    /// <param name="__in_opt  const GUID *PowerSettingGuid"></param>
    /// <param name="__in      DWORD DcValueIndex"></param>
    /// <returns></returns>
    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern UInt32 PowerWriteDCValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
      ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, UInt32 DcValueIndex);

    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern bool GetCurrentPowerPolicies(ref GLOBAL_POWER_POLICY pGlobalPowerPolicy, ref POWER_POLICY pPowerPolicy);

    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern bool GetActivePwrScheme(ref UInt32 puiID);

    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern bool SetActivePwrScheme(uint uiID, ref GLOBAL_POWER_POLICY lpGlobalPowerPolicy, ref POWER_POLICY lpPowerPolicy);

    /// <summary>
    /// Disables file system redirection for the calling thread. File system redirection is enabled by default.
    /// </summary>
    /// <param name="OldValue">The WOW64 file system redirection value.</param>
    /// <returns>If the function succeeds, the return value is a nonzero value.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern UInt32 Wow64DisableWow64FsRedirection(ref IntPtr OldValue);

    /// <summary>
    /// Restores file system redirection for the calling thread.
    /// </summary>
    /// <param name="OldValue">The WOW64 file system redirection value.</param>
    /// <returns>If the function succeeds, the return value is a nonzero value.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern UInt32 Wow64RevertWow64FsRedirection(IntPtr OldValue);

    /// <summary>
    /// Loads the specified module into the address space of the calling process.
    /// </summary>
    /// <param name="libraryName">The name of the module.</param>
    /// <returns>If the function succeeds, the return value is a handle to the module.</returns>
    [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    private extern static IntPtr LoadLibrary(string libraryName);

    /// <summary>
    /// Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
    /// </summary>
    /// <param name="hwnd">A handle to the DLL module that contains the function or variable.</param>
    /// <param name="procedureName">The function or variable name, or the function's ordinal value.</param>
    /// <returns>If the function succeeds, the return value is the address of the exported function or variable.</returns>
    [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    private extern static IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

    #endregion

    #region Private power management helper methods

    /// <summary>
    /// System power setting structure
    /// </summary>
    struct SystemPowerSetting
    {
      public Guid settingGuid;
      public Guid subgroupGuid;
    }

    /// <summary>
    /// Array of system power settings (index is SystemPowerSettingsType)
    /// </summary>
    static SystemPowerSetting[] SystemPowerSettings = new SystemPowerSetting[]
    {
      new SystemPowerSetting  // LOCK_CONSOLE_ON_WAKE
      {       
        settingGuid = GUID_LOCK_CONSOLE_ON_WAKE,
        subgroupGuid = NO_SUBGROUP_GUID,
      },
      new SystemPowerSetting  // ALLOW_AWAY_MODE
      {       
        settingGuid = GUID_ALLOW_AWAY_MODE,
        subgroupGuid = GUID_SUB_SLEEP,
      },
      new SystemPowerSetting  // SLEEP_AFTER
      {      
        settingGuid = GUID_SLEEP_AFTER,
        subgroupGuid = GUID_SUB_SLEEP,
      },
      new SystemPowerSetting  // ALLOW_HYBRID_SLEEP
      {      
        settingGuid = GUID_ALLOW_HYBRID_SLEEP,
        subgroupGuid = GUID_SUB_SLEEP,
      },
      new SystemPowerSetting  // HIBERNATE_AFTER
      {      
        settingGuid = GUID_HIBERNATE_AFTER,
        subgroupGuid = GUID_SUB_SLEEP,
      },
      new SystemPowerSetting  // ALLOW_RTC_WAKE
      {      
        settingGuid = GUID_ALLOW_RTC_WAKE,
        subgroupGuid = GUID_SUB_SLEEP,
      },
      new SystemPowerSetting  // LID_CLOSE_ACTION
      {      
        settingGuid = GUID_LID_CLOSE_ACTION,
        subgroupGuid = GUID_SUB_POWER_BUTTONS_AND_LID,
      },
      new SystemPowerSetting  // POWER_BUTTON_ACTION
      {      
        settingGuid = GUID_POWER_BUTTON_ACTION,
        subgroupGuid = GUID_SUB_POWER_BUTTONS_AND_LID,
      },
      new SystemPowerSetting  // SLEEP_BUTTON_ACTION
      {      
        settingGuid = GUID_SLEEP_BUTTON_ACTION,
        subgroupGuid = GUID_SUB_POWER_BUTTONS_AND_LID,
      },
      new SystemPowerSetting  // WHEN_SHARING_MEDIA
      {      
        settingGuid = GUID_WHEN_SHARING_MEDIA,
        subgroupGuid = GUID_SUB_MULTIMEDIA,
      },
    };

    /// <summary>
    /// Check AC power status
    /// </summary>
    /// <returns>Returns true if AC power is plugged in</returns>
    private static bool ACPowerPluggedIn
    {
      get
      {
        SYSTEM_POWER_STATUS sps = new SYSTEM_POWER_STATUS();
        try
        {
          GetSystemPowerStatus(ref sps);
          return (sps.ACLineStatus == ACLineStatus.Online);
        }
        catch (Exception)
        {
          return true;  // Fallback: AC power is plugged in
        }
      }
    }

    /// <summary>
    /// Get system power setting for indicated power status (Vista / Windows 7)
    /// </summary>
    /// <param name="settingType"></param>
    /// <returns>Returns value of power setting requested. Returns -1 on errors.</returns>
    private static int GetSystemPowerSetting(SystemPowerSettingType settingType, bool AC)
    {
      try
      {
        IntPtr ptr = IntPtr.Zero;
        Guid scheme;
        UInt32 value = 0;
        UInt32 success;

        PowerGetActiveScheme(IntPtr.Zero, ref ptr);
        scheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
        SystemPowerSetting ps = SystemPowerSettings[(int)settingType];
        if (AC)
          success = PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref ps.subgroupGuid, ref ps.settingGuid, ref value);
        else
          success = PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref ps.subgroupGuid, ref ps.settingGuid, ref value);
        if (success == 0)
          return (int)value;
        return -1;
      }
      catch (Exception)
      {
        return -1;
      }
    }

    /// <summary>
    /// Set system power setting value for indicated power status
    /// </summary>
    /// <param name="settingType"></param>
    /// <param name="AC"></param>
    /// <param name="value"></param>
    private static void SetSystemPowerSetting(SystemPowerSettingType settingType, bool AC, uint value)
    {
      try
      {
        IntPtr ptr = IntPtr.Zero;
        PowerGetActiveScheme(IntPtr.Zero, ref ptr);
        Guid scheme = (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));

        SystemPowerSetting ps = SystemPowerSettings[(int)settingType];
        if (AC)
          PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref ps.subgroupGuid, ref ps.settingGuid, value);
        else
          PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref ps.subgroupGuid, ref ps.settingGuid, value);
      }
      catch (Exception) { }
    }

    /// <summary>
    /// Get power policy setting for indicated power status (Windows XP)
    /// </summary>
    /// <param name="settingType">Returns value of power setting requested. Returns -1 on errors.</param>
    /// <returns></returns>
    private static int GetPowerPolicySetting(SystemPowerSettingType settingType, bool AC)
    {
      try
      {
        GLOBAL_POWER_POLICY gpp = new GLOBAL_POWER_POLICY();
        POWER_POLICY pp = new POWER_POLICY();
        POWER_ACTION action;
        PowerActionEventCode eventCode;

        if (!GetCurrentPowerPolicies(ref gpp, ref pp))
        {
          return -1;
        }

        switch (settingType)
        {
          case SystemPowerSettingType.STANDBYIDLE:
            if (AC)
            {
              if (pp.user.IdleAc.Action == POWER_ACTION.PowerActionSleep)
                return (int)pp.user.IdleTimeoutAc;
            }
            else
            {
              if (pp.user.IdleDc.Action == POWER_ACTION.PowerActionSleep)
                return (int)pp.user.IdleTimeoutDc;
            }
            return 0;

          case SystemPowerSettingType.HIBERNATE_AFTER:
            if (AC)
            {
              if (pp.user.IdleAc.Action == POWER_ACTION.PowerActionHibernate)
                return (int)pp.user.IdleTimeoutAc;
              if (pp.mach.DozeS4TimeoutAc != 0)
                return (int)(pp.mach.DozeS4TimeoutAc + pp.user.IdleTimeoutAc);
            }
            else
            {
              if (pp.user.IdleDc.Action == POWER_ACTION.PowerActionHibernate)
                return (int)pp.user.IdleTimeoutDc;
              if (pp.mach.DozeS4TimeoutDc != 0)
                return (int)(pp.mach.DozeS4TimeoutDc + pp.user.IdleTimeoutDc);
            }
            return 0;

          case SystemPowerSettingType.LID_CLOSE_ACTION:
            if (AC)
            {
              action = gpp.user.LidCloseAc.Action;
              eventCode = gpp.user.LidCloseAc.EventCode;
            }
            else
            {
              action = gpp.user.LidCloseDc.Action;
              eventCode = gpp.user.LidCloseDc.EventCode;
            }
            switch (action)
            {
              case POWER_ACTION.PowerActionSleep:
                return 1; // Sleep
              case POWER_ACTION.PowerActionHibernate:
                return 2; // Hibernate
              default:
                if ((eventCode & PowerActionEventCode.POWER_USER_NOTIFY_SHUTDOWN) != 0)
                  return 3; // Shutdown
                if ((eventCode & PowerActionEventCode.POWER_USER_NOTIFY_BUTTON) != 0)
                  return 4; // Ask User
                return 0; //Do nothing
            }

          case SystemPowerSettingType.POWER_BUTTON_ACTION:
            if (AC)
            {
              action = gpp.user.PowerButtonAc.Action;
              eventCode = gpp.user.PowerButtonAc.EventCode;
            }
            else
            {
              action = gpp.user.PowerButtonDc.Action;
              eventCode = gpp.user.PowerButtonDc.EventCode;
            }
            switch (action)
            {
              case POWER_ACTION.PowerActionSleep:
                return 1; // Sleep
              case POWER_ACTION.PowerActionHibernate:
                return 2; // Hibernate
              default:
                if ((eventCode & PowerActionEventCode.POWER_USER_NOTIFY_SHUTDOWN) != 0)
                  return 3; // Shutdown
                if ((eventCode & PowerActionEventCode.POWER_USER_NOTIFY_BUTTON) != 0)
                  return 4; // Ask User
                return 0; //Do nothing
            }

          case SystemPowerSettingType.SLEEP_BUTTON_ACTION:
            if (AC)
            {
              action = gpp.user.SleepButtonAc.Action;
              eventCode = gpp.user.SleepButtonAc.EventCode;
            }
            else
            {
              action = gpp.user.SleepButtonDc.Action;
              eventCode = gpp.user.SleepButtonDc.EventCode;
            }
            switch (action)
            {
              case POWER_ACTION.PowerActionSleep:
                return 1; // Sleep
              case POWER_ACTION.PowerActionHibernate:
                return 2; // Hibernate
              default:
                if ((eventCode & PowerActionEventCode.POWER_USER_NOTIFY_SHUTDOWN) != 0)
                  return 3; // Shutdown
                if ((eventCode & PowerActionEventCode.POWER_USER_NOTIFY_BUTTON) != 0)
                  return 4; // Ask User
                return 0; //Do nothing
            }

          default:
            return 0;
        }
      }
      catch (Exception)
      {
        return -1;
      }
    }

    /// <summary>
    /// Set power policy setting value for indicated power status (Windows XP)
    /// </summary>
    /// <param name="settingType"></param>
    /// <param name="AC"></param>
    /// <param name="value"></param>
    private static void SetPowerPolicySetting(SystemPowerSettingType settingType, bool AC, uint value)
    {
      try
      {
        GLOBAL_POWER_POLICY gpp = new GLOBAL_POWER_POLICY();
        POWER_POLICY pp = new POWER_POLICY();
        UInt32 index = 0;
        POWER_ACTION action;
        PowerActionEventCode eventCode;

        if (!GetCurrentPowerPolicies(ref gpp, ref pp))
          return;

        GetActivePwrScheme(ref index);

        switch (settingType)
        {
          case SystemPowerSettingType.STANDBYIDLE:
            if (AC)
            {
              pp.user.IdleTimeoutAc = value;
              if (value == 0 && pp.mach.DozeS4TimeoutAc != 0)
                pp.user.IdleAc.Action = POWER_ACTION.PowerActionHibernate;
              else
                pp.user.IdleAc.Action = POWER_ACTION.PowerActionSleep;
            }
            else
            {
              pp.user.IdleTimeoutDc = value;
              if (value == 0 && pp.mach.DozeS4TimeoutDc != 0)
                pp.user.IdleDc.Action = POWER_ACTION.PowerActionHibernate;
              else
                pp.user.IdleDc.Action = POWER_ACTION.PowerActionSleep;
            }
            break;

          case SystemPowerSettingType.HIBERNATE_AFTER:
            if (AC)
            {
              if (pp.user.IdleTimeoutAc == 0)
              {
                pp.user.IdleTimeoutAc = value;
                pp.mach.DozeS4TimeoutAc = 0;
                pp.user.IdleAc.Action = POWER_ACTION.PowerActionHibernate;
              }
              else
              {
                if (value > pp.user.IdleTimeoutAc)
                  pp.mach.DozeS4TimeoutAc = value - pp.user.IdleTimeoutAc;
                else
                  pp.mach.DozeS4TimeoutAc = value;
                pp.user.IdleAc.Action = POWER_ACTION.PowerActionSleep;
              }
            }
            else
            {
              if (pp.user.IdleTimeoutDc == 0)
              {
                pp.user.IdleTimeoutDc = value;
                pp.mach.DozeS4TimeoutDc = 0;
                pp.user.IdleDc.Action = POWER_ACTION.PowerActionHibernate;
              }
              else
              {
                if (value > pp.user.IdleTimeoutDc)
                  pp.mach.DozeS4TimeoutDc = value - pp.user.IdleTimeoutDc;
                else
                  pp.mach.DozeS4TimeoutDc = value;
                pp.user.IdleDc.Action = POWER_ACTION.PowerActionSleep;
              }
            }
            break;

          case SystemPowerSettingType.LID_CLOSE_ACTION:
            eventCode = 0;
            switch (value)
            {
              case 0: // Do nothing
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_FORCE_TRIGGER_RESET;
                break;
              case 1: // Sleep
                action = POWER_ACTION.PowerActionSleep;
                break;
              case 2: // Hibernate
                action = POWER_ACTION.PowerActionHibernate;
                break;
              case 3: // Shutdown
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_USER_NOTIFY_SHUTDOWN;
                break;
              case 4: // Ask User
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_USER_NOTIFY_BUTTON;
                break;
              default:
                return;
            }
            if (AC)
            {
                gpp.user.LidCloseAc.Action = action;
                gpp.user.LidCloseAc.EventCode = eventCode;
            }
            else
            {
              gpp.user.LidCloseDc.Action = action;
              gpp.user.LidCloseDc.EventCode = eventCode;
            }
            break;

          case SystemPowerSettingType.POWER_BUTTON_ACTION:
            eventCode = 0;
            switch (value)
            {
              case 0: // Do nothing
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_FORCE_TRIGGER_RESET;
                break;
              case 1: // Sleep
                action = POWER_ACTION.PowerActionSleep;
                break;
              case 2: // Hibernate
                action = POWER_ACTION.PowerActionHibernate;
                break;
              case 3: // Shutdown
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_USER_NOTIFY_SHUTDOWN;
                break;
              case 4: // Ask User
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_USER_NOTIFY_BUTTON;
                break;
              default:
                return;
            }
            if (AC)
            {
                gpp.user.PowerButtonAc.Action = action;
                gpp.user.PowerButtonAc.EventCode = eventCode;
            }
            else
            {
              gpp.user.PowerButtonDc.Action = action;
              gpp.user.PowerButtonDc.EventCode = eventCode;
            }
            break;

          case SystemPowerSettingType.SLEEP_BUTTON_ACTION:
            eventCode = 0;
            switch (value)
            {
              case 0: // Do nothing
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_FORCE_TRIGGER_RESET;
                break;
              case 1: // Sleep
                action = POWER_ACTION.PowerActionSleep;
                break;
              case 2: // Hibernate
                action = POWER_ACTION.PowerActionHibernate;
                break;
              case 3: // Shutdown
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_USER_NOTIFY_SHUTDOWN;
                break;
              case 4: // Ask User
                action = POWER_ACTION.PowerActionNone;
                eventCode = PowerActionEventCode.POWER_USER_NOTIFY_BUTTON;
                break;
              default:
                return;
            }
            if (AC)
            {
              gpp.user.SleepButtonAc.Action = action;
              gpp.user.SleepButtonAc.EventCode = eventCode;
            }
            else
            {
              gpp.user.SleepButtonDc.Action = action;
              gpp.user.SleepButtonDc.EventCode = eventCode;
            }
            break;

          default:
            return;
        }
        SetActivePwrScheme(index, ref gpp, ref pp);
      }
      catch (Exception) { }
    }

    private delegate bool IsWow64ProcessDelegate([In] IntPtr handle, [Out] out bool isWow64Process);

    /// <summary>
    /// Checks if OS is 64 bit
    /// </summary>
    /// <returns>Returns true if 64-bit OS</returns>
    private static bool IsOS64Bit()
    {
      return (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()));
    }

    private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
    {
      IntPtr handle = LoadLibrary("kernel32");

      if (handle != IntPtr.Zero)
      {
        IntPtr fnPtr = GetProcAddress(handle, "IsWow64Process");
        if (fnPtr != IntPtr.Zero)
        {
          return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer((IntPtr)fnPtr, typeof(IsWow64ProcessDelegate));
        }
      }
      return null;
    }

    private static bool Is32BitProcessOn64BitProcessor()
    {
      IsWow64ProcessDelegate fnDelegate = GetIsWow64ProcessDelegate();

      if (fnDelegate == null)
        return false;

      bool isWow64;
      bool retVal = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, out isWow64);

      if (retVal == false)
        return false;
      else
        return isWow64;
    }

    #endregion

  }
}