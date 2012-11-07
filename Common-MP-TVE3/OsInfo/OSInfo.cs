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
using System.Runtime.InteropServices;

namespace OSInfo
{
  /// <summary>
  /// OSInfo Class
  /// </summary>
  public class OSInfo
  {

    #region win32 API definitions

    [StructLayout(LayoutKind.Sequential)]
    private struct OSVERSIONINFOEX
    {
      public int dwOSVersionInfoSize;
      public int dwMajorVersion;
      public int dwMinorVersion;
      public int dwBuildNumber;
      public int dwPlatformId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szCSDVersion;
      public short wServicePackMajor;
      public short wServicePackMinor;
      public short wSuiteMask;
      public byte wProductType;
      public byte wReserved;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

    [DllImport("kernel32.dll")]
    private static extern bool VerifyVersionInfo(ref OSVERSIONINFOEX osVersionInfo, [In] uint dwTypeMask, [In] UInt64 dwlConditionMask);

    [DllImport("kernel32.dll")]
    private static extern ulong VerSetConditionMask([In] ulong dwlConditionMask, [In] uint dwTypeBitMask,
                                                    [In] byte dwConditionMask);

    [DllImport("kernel32.dll")]
    private static extern bool GetProductInfo(
      [In] int dwOSMajorVersion,
      [In] int dwOSMinorVersion,
      [In] int dwSpMajorVersion,
      [In] int dwSpMinorVersion,
      [Out] out int pdwReturnedProductType);

    [DllImport("user32.dll")]
    private static extern bool GetSystemMetrics([In] int nIndex);

    #endregion

    #region Private Constants

    //wProductType ( http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx )
    private const int NT_WORKSTATION = 1;
    private const int NT_DOMAIN_CONTROLLER = 2;
    private const int NT_SERVER = 3;

    //SuiteMask ( http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx )
    private const int VER_SUITE_SMALLBUSINESS = 0x00000001;
    private const int VER_SUITE_ENTERPRISE = 0x00000002;
    private const int VER_SUITE_BACKOFFICE = 0x00000004;
    private const int VER_SUITE_TERMINAL = 0x00000010;
    private const int VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x00000020;
    private const int VER_SUITE_EMBEDDEDNT = 0x00000040;
    private const int VER_SUITE_DATACENTER = 0x00000080;
    private const int VER_SUITE_SINGLEUSERTS = 0x00000100;
    private const int VER_SUITE_PERSONAL = 0x00000200;
    private const int VER_SUITE_BLADE = 0x00000400;
    private const int VER_SUITE_STORAGE_SERVER = 0x00002000;
    private const int VER_SUITE_COMPUTE_SERV = 0x00004000;
    private const int VER_SUITE_WH_SERVER = 0x00008000;

    //ProductType ( http://msdn.microsoft.com/en-us/library/ms724358(VS.85).aspx )
    private const int PRODUCT_BUSINESS = 0x00000006;
    private const int PRODUCT_BUSINESS_N = 0x00000010;
    private const int PRODUCT_CLUSTER_SERVER = 0x00000012;
    private const int PRODUCT_DATACENTER_SERVER = 0x00000008;
    private const int PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
    private const int PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027;
    private const int PRODUCT_DATACENTER_SERVER_V = 0x00000025;
    private const int PRODUCT_ENTERPRISE = 0x00000004;
    private const int PRODUCT_ENTERPRISE_E = 0x00000046;
    private const int PRODUCT_ENTERPRISE_N = 0x0000001B;
    private const int PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
    private const int PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
    private const int PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
    private const int PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
    private const int PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
    private const int PRODUCT_HOME_BASIC = 0x00000002;
    private const int PRODUCT_HOME_BASIC_E = 0x00000043;
    private const int PRODUCT_HOME_BASIC_N = 0x00000005;
    private const int PRODUCT_HOME_PREMIUM = 0x00000003;
    private const int PRODUCT_HOME_PREMIUM_E = 0x00000044;
    private const int PRODUCT_HOME_PREMIUM_N = 0x0000001A;
    private const int PRODUCT_HYPERV = 0x0000002A;
    private const int PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
    private const int PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
    private const int PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
    private const int PRODUCT_PROFESSIONAL = 0x00000030;
    private const int PRODUCT_PROFESSIONAL_E = 0x00000045;
    private const int PRODUCT_PROFESSIONAL_N = 0x00000031;
    private const int PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
    private const int PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
    private const int PRODUCT_SERVER_FOUNDATION = 0x00000021;
    private const int PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
    private const int PRODUCT_STANDARD_SERVER = 0x00000007;
    private const int PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
    private const int PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
    private const int PRODUCT_STANDARD_SERVER_V = 0x00000024;
    private const int PRODUCT_STARTER = 0x0000000B;
    private const int PRODUCT_STARTER_E = 0x00000042;
    private const int PRODUCT_STARTER_N = 0x0000002F;
    private const int PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
    private const int PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
    private const int PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
    private const int PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
    private const int PRODUCT_UNDEFINED = 0x00000000;
    private const int PRODUCT_ULTIMATE = 0x00000001;
    private const int PRODUCT_ULTIMATE_E = 0x00000047;
    private const int PRODUCT_ULTIMATE_N = 0x0000001C;
    private const int PRODUCT_WEB_SERVER = 0x00000011;
    private const int PRODUCT_WEB_SERVER_CORE = 0x0000001D;

    //Type bitmask ( http://msdn.microsoft.com/en-gb/library/ms725494(vs.85).aspx )
    private const int VER_MINORVERSION = 0x0000001;
    private const int VER_MAJORVERSION = 0x0000002;
    private const int VER_BUILDVERSION = 0x0000004;
    private const int VER_PLATFORMID = 0x0000008;
    private const int VER_SERVICEPACKMINOR = 0x0000010;
    private const int VER_SERVICEPACKMAJOR = 0x0000020;
    private const int VER_SUITENAME = 0x0000040;
    private const int VER_PRODUCT_TYPE = 0x0000080;

    //Condition bitmask ( http://msdn.microsoft.com/en-gb/library/ms725494(vs.85).aspx )
    private const int VER_EQUAL = 1;
    private const int VER_GREATER = 2;
    private const int VER_GREATER_EQUAL = 3;
    private const int VER_LESS = 4;
    private const int VER_LESS_EQUAL = 5;
    private const int VER_AND = 6; // only for wSuiteMask
    private const int VER_OR = 7; // only for wSuiteMask

    //sysMetrics ( http://msdn.microsoft.com/en-us/library/ms724385(VS.85).aspx )
    private const int SM_TABLETPC = 86;
    private const int SM_MEDIACENTER = 87;
    private const int SM_STARTER = 88;
    private const int SM_SERVERR2 = 89;

    #endregion

    #region enums

    /// <summary>
    /// List of all operating systems
    /// </summary>
    public enum OSList
    {
      ///<summary>
      /// Windows 95/98, NT4.0, 2000
      ///</summary>
      Windows2000andPrevious,
      ///<summary>
      /// Windows XP x86
      ///</summary>
      WindowsXp,
      ///<summary>
      /// Windows XP x64
      ///</summary>
      WindowsXp64,
      ///<summary>
      /// Windows Vista
      ///</summary>
      WindowsVista,
      ///<summary>
      /// Windows 7
      ///</summary>
      Windows7,
      ///<summary>
      /// Windows 8
      ///</summary>
      Windows8,
      ///<summary>
      /// Windows 2003 Server
      ///</summary>
      Windows2003,
      ///<summary>
      /// Windows 2003 R2 Server
      ///</summary>
      Windows2003R2,
      ///<summary>
      /// Windows 2008 Server
      ///</summary>
      Windows2008,
      ///<summary>
      /// Windows 2008 R2 Server
      ///</summary>
      Windows2008R2,
      ///<summary>
      /// Windows 8
      ///</summary>
      Windows2012
    }

    /// <summary>
    /// List of available status of current OS
    /// </summary>
    public enum OsSupport
    {
      /// <summary>
      /// Blocked: will cause an immediate exit of the program
      /// </summary>
      Blocked = 0,
      /// <summary>
      /// FullySupported: self explanatory
      /// </summary>
      FullySupported = 1,
      /// <summary>
      /// NotSupported: officially not supported, will log/display a warning
      /// </summary>
      NotSupported = 2
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Return a full version string, es.: "Windows XP ( Servicepack 2 ) [5.1.0000]"
    /// </summary>
    /// <returns>A string rappresenting a fully displayable version</returns>
    public static string GetOSDisplayVersion()
    {
      string servicePack = GetOSServicePack();
      if (!string.IsNullOrEmpty(servicePack))
      {
        servicePack = " ( " + servicePack + " )";
      }
      return GetOSNameString() + servicePack + " [" + Environment.OSVersion.Version + "]";
    }

    /// <summary>
    /// Return a value that indicate if the OS is blocked, supported, or officially unsupported
    /// </summary>
    public static OsSupport GetOSSupported()
    {

      if (VerifyDesktopOSMinRequirement(5, 1, 3, 2600))
      { // XP SP3
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 0, 2, 6000))
      { // Vista SP2
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 1, 0, 7600))
      { // Win7 RTM
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 2, 0, 9200))
      { // Windows 8 RTM
        return OsSupport.NotSupported;
      }
      if (IsServer())
      { // any server OS
        return OsSupport.NotSupported;
      }

      return OsSupport.Blocked;
    }

    /// <summary>
    /// Return if running on XP or later
    /// </summary>
    /// <returns>true means XP or later</returns>
    /// <returns>false means 2000 or previous</returns>
    public static bool XpOrLater()
    {
      return VerifyVersionGreaterEqual(5,1);
    }

    /// <summary>
    /// Return if running on XP 64 or later
    /// </summary>
    /// <returns>true means XP 64 or later</returns>
    /// <returns>false means XP or previous</returns>
    public static bool Xp64OrLater()
    {
      return VerifyVersionGreaterEqual(5, 2);
    }

    /// <summary>
    /// Return if running on Vista or later
    /// </summary>
    /// <returns>true means Vista or later</returns>
    /// <returns>false means Xp or previous</returns>
    public static bool VistaOrLater()
    {
      return VerifyVersionGreaterEqual(6,0);
    }

    /// <summary>
    /// Return if running on Windows7 or later
    /// </summary>
    /// <returns>true means Windows7 or later</returns>
    /// <returns>false means Vista or previous</returns>
    public static bool Win7OrLater()
    {
      return VerifyVersionGreaterEqual(6,1);
    }

    /// <summary>
    /// Return if running on Windows8 or later
    /// </summary>
    /// <returns>true means Windows8 or later</returns>
    /// <returns>false means Win7 or previous</returns>
    public static bool Win8OrLater()
    {
      return VerifyVersionGreaterEqual(6,2);
    }

    #endregion
    
    #region Properties

    /// <summary>
    /// Gets the main version of the service pack running on this computer.
    /// </summary>
    public static int OSServicePackMinor
    {
      get
      {
        OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));
        return !GetVersionEx(ref osVersionInfo) ? -1 : osVersionInfo.wServicePackMinor;
      }
    }

    /// <summary>
    /// Gets the product type of the operating system running on this computer.
    /// </summary>
    private static byte OSProductType
    {
      get
      {
        OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));
        if (!GetVersionEx(ref osVersionInfo)) return 0x0;
        return osVersionInfo.wProductType;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Returns the service pack information of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system service pack information.</returns>
    private static string GetOSServicePack()
    {
      OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
      osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
      return !GetVersionEx(ref osVersionInfo) ? string.Empty : osVersionInfo.szCSDVersion;
    }

    /// <summary>
    /// Returns the name of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system name.</returns>
    private static string GetOSNameString()
    {
      OperatingSystem osInfo = Environment.OSVersion;
      string osName = "UNKNOWN";

      switch (osInfo.Platform)
      {
        case PlatformID.Win32Windows:
          {
            switch (Environment.OSVersion.Version.Minor)
            {
              case 0:
                {
                  osName = "Windows 95";
                  break;
                }
              case 10:
                {
                  osName = osInfo.Version.Revision.ToString() == "2222A" ? "Windows 98 Second Edition" : "Windows 98";
                  break;
                }
              case 90:
                {
                  osName = "Windows Me";
                  break;
                }
            }
            break;
          }

        case PlatformID.Win32NT:
          {
            switch (Environment.OSVersion.Version.Major)
            {
              case 3:
                {
                  osName = "Windows NT 3.51";
                  break;
                }
              case 4:
                {
                  osName = "Windows NT 4.0";
                  break;
                }
              case 5:
                {
                  switch (Environment.OSVersion.Version.Minor)
                  {
                    case 0:
                      osName = "Windows 2000";
                      break;
                    case 1:
                      osName = "Windows XP";
                      break;
                    case 2:
                      if (OSProductType == NT_WORKSTATION)
                      {
                        osName = "WindowsXP x64";
                      }
                      else
                      {
                        osName = GetSystemMetrics(SM_SERVERR2) ? "Windows Server 2003 R2" : "Windows Server 2003";
                      }


                      break;
                  }
                  break;
                }
              case 6:
                {
                  switch (Environment.OSVersion.Version.Minor)
                  {
                    case 0:
                      osName = OSProductType == NT_WORKSTATION ? "Windows Vista" : "Windows 2008";
                      break;
                    case 1:
                      osName = OSProductType == NT_WORKSTATION ? "Windows 7" : "Windows 2008 R2";
                      break;
                    case 2:
                      osName = OSProductType == NT_WORKSTATION ? "Windows 8" : "Windows 2012";
                      break;
                  }
                  break;
                }
            }
            break;
          }
      }

      return osName;
    }

    /// <summary>
    /// Checks if OS is later then major / minor version
    /// </summary>
    /// <param name="majorVersion">Major OS version</param>
    /// <param name="minorVersion">Minor OS version</param>
    /// <returns>True if OS is later than version supplied as parameters</returns>
    private static bool VerifyVersionGreaterEqual(int majorVersion, int minorVersion)
    {
      ulong condition = 0;
      var osVersionInfo = new OSVERSIONINFOEX
                            {
                              dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX)),
                              dwMajorVersion = majorVersion,
                              dwMinorVersion =  minorVersion
                            };
      condition = VerSetConditionMask(condition, VER_MAJORVERSION,VER_GREATER_EQUAL);
      return VerifyVersionInfo(ref osVersionInfo, VER_MAJORVERSION, condition);
    }

    /// <summary>
    /// Checks Desktop OS for required service pack and build version
    /// </summary>
    /// <param name="majorVersion">Major OS version</param>
    /// <param name="minorVersion">Minor OS version</param>
    /// <param name="servicePack">Minimum Service Pack</param>
    /// <param name="buildVersion">Minimum </param>
    /// <returns>True if Major / Minor OS versions match and service pack / build version are >= parameters</returns>
    private static bool VerifyDesktopOSMinRequirement(int majorVersion, int minorVersion, short servicePack, int buildVersion)
    {
      ulong condition = 0;
      var osVersionInfo = new OSVERSIONINFOEX
      {
        dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)),
        dwMajorVersion = majorVersion,
        dwMinorVersion = minorVersion,
        dwBuildNumber = buildVersion,
        wProductType = NT_WORKSTATION,
        wServicePackMajor = servicePack
      };
      condition = VerSetConditionMask(condition, VER_MAJORVERSION, VER_EQUAL);
      condition = VerSetConditionMask(condition, VER_MINORVERSION, VER_EQUAL);
      condition = VerSetConditionMask(condition, VER_PRODUCT_TYPE, VER_EQUAL);
      condition = VerSetConditionMask(condition, VER_SERVICEPACKMAJOR, VER_GREATER_EQUAL);
      condition = VerSetConditionMask(condition, VER_BUILDVERSION, VER_GREATER_EQUAL);
      return VerifyVersionInfo(ref osVersionInfo, VER_MAJORVERSION | VER_MINORVERSION | VER_PRODUCT_TYPE | 
                                                  VER_SERVICEPACKMAJOR | VER_BUILDVERSION, condition);
    }

    /// <summary>
    /// Identifies if OS is a Windows Server OS
    /// </summary>
    /// <returns>True if OS is a Windows Server OS</returns>
    private static bool IsServer()
    {
      ulong condition = 0;
      var osVersionInfo = new OSVERSIONINFOEX
      {
        dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)),
        wProductType = NT_WORKSTATION // note the check is that this is not equal as per MS documenation
      };
      condition = VerSetConditionMask(condition, VER_PRODUCT_TYPE, VER_EQUAL);
      return !VerifyVersionInfo(ref osVersionInfo, VER_PRODUCT_TYPE, condition);
    }

    #endregion

  }
}