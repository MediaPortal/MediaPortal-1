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
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      public string szCSDVersion;
      public short wServicePackMajor;
      public short wServicePackMinor;
      public short wSuiteMask;
      public byte wProductType;
      public byte wReserved;
    }

    [DllImport("kernel32.dll")]
    private static extern bool GetVersionEx(ref OSVERSIONINFOEX osVersionInfo);

    [DllImport("kernel32.dll")]
    private static extern bool GetProductInfo(
[In] int dwOSMajorVersion,
[In] int dwOSMinorVersion,
[In] int dwSpMajorVersion,
[In] int dwSpMinorVersion,
[Out] out int pdwReturnedProductType);

    [DllImport("user32.dll")]
    private static extern bool GetSystemMetrics([In] int nIndex);

    [DllImport("kernel32.dll")]
    private static extern bool VerifyVersionInfo(ref OSVERSIONINFOEX osVersionInfo, [In] uint dwTypeMask, [In] UInt64 dwlConditionMask);

    [DllImport("kernel32.dll")]
    private static extern ulong VerSetConditionMask([In] ulong dwlConditionMask, [In] uint dwTypeBitMask,
[In] byte dwConditionMask);

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
      /// Windows 8
      ///</summary>
      Windows81,
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
      /// Windows 2012 Server
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
    /// Returns the product type of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system product type.</returns>
    public static string GetOSProductType()
    {
      OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
      osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
      if (!GetVersionEx(ref osVersionInfo)) return string.Empty;

      switch (OSMajorVersion)
      {
        case 4:
          if (OSProductType == NT_WORKSTATION)
          {
            // Windows NT 4.0 Workstation
            return " Workstation";
          }
          if (OSProductType == NT_SERVER)
          {
            // Windows NT 4.0 Server
            return " Server";
          }
          return string.Empty;
        case 5:
          if (GetSystemMetrics(SM_MEDIACENTER))
          {
            return " Media Center";
          }
          if (GetSystemMetrics(SM_TABLETPC))
          {
            return " Tablet PC";
          }
          if (OSProductType == NT_WORKSTATION)
          {
            if ((osVersionInfo.wSuiteMask & VER_SUITE_EMBEDDEDNT) == VER_SUITE_EMBEDDEDNT)
            {
              //Windows XP Embedded
              return " Embedded";
            }
            return (osVersionInfo.wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL ? " Home" : " Professional";
            // Windows XP / Windows 2000 Professional
          }
          if (OSProductType == NT_SERVER || OSProductType == NT_DOMAIN_CONTROLLER)
          {
            if (OSMinorVersion == 0)
            {
              if ((osVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
              {
                // Windows 2000 Datacenter Server
                return " Datacenter Server";
              }
              if ((osVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
              {
                // Windows 2000 Advanced Server
                return " Advanced Server";
              }
              // Windows 2000 Server
              return " Server";
            }
            if (OSMinorVersion == 2)
            {
              if ((osVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
              {
                // Windows Server 2003 Datacenter Edition
                return " Datacenter Edition";
              }
              if ((osVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
              {
                // Windows Server 2003 Enterprise Edition
                return " Enterprise Edition";
              }
              if ((osVersionInfo.wSuiteMask & VER_SUITE_STORAGE_SERVER) == VER_SUITE_STORAGE_SERVER)
              {
                // Windows Server 2003 Storage Edition
                return " Storage Edition";
              }
              if ((osVersionInfo.wSuiteMask & VER_SUITE_COMPUTE_SERV) == VER_SUITE_COMPUTE_SERV)
              {
                // Windows Server 2003 Compute Cluster Edition
                return " Compute Cluster Edition";
              }
              if ((osVersionInfo.wSuiteMask & VER_SUITE_BLADE) == VER_SUITE_BLADE)
              {
                // Windows Server 2003 Web Edition
                return " Web Edition";
              }
              // Windows Server 2003 Standard Edition
              return " Standard Edition";
            }
          }
          break;
        case 6:
          int strProductType;
          GetProductInfo(osVersionInfo.dwMajorVersion, osVersionInfo.dwMinorVersion, 0, 0, out strProductType);
          switch (strProductType)
          {
            case PRODUCT_ULTIMATE:
            case PRODUCT_ULTIMATE_E:
            case PRODUCT_ULTIMATE_N:
              return "Ultimate Edition";
            case PRODUCT_PROFESSIONAL:
            case PRODUCT_PROFESSIONAL_E:
            case PRODUCT_PROFESSIONAL_N:
              return "Professional";
            case PRODUCT_HOME_PREMIUM:
            case PRODUCT_HOME_PREMIUM_E:
            case PRODUCT_HOME_PREMIUM_N:
              return "Home Premium Edition";
            case PRODUCT_HOME_BASIC:
            case PRODUCT_HOME_BASIC_E:
            case PRODUCT_HOME_BASIC_N:
              return "Home Basic Edition";
            case PRODUCT_ENTERPRISE:
            case PRODUCT_ENTERPRISE_E:
            case PRODUCT_ENTERPRISE_N:
            case PRODUCT_ENTERPRISE_SERVER_V:
              return "Enterprise Edition";
            case PRODUCT_BUSINESS:
            case PRODUCT_BUSINESS_N:
              return "Business Edition";
            case PRODUCT_STARTER:
            case PRODUCT_STARTER_E:
            case PRODUCT_STARTER_N:
              return "Starter Edition";
            case PRODUCT_CLUSTER_SERVER:
              return "Cluster Server Edition";
            case PRODUCT_DATACENTER_SERVER:
            case PRODUCT_DATACENTER_SERVER_V:
              return "Datacenter Edition";
            case PRODUCT_DATACENTER_SERVER_CORE:
            case PRODUCT_DATACENTER_SERVER_CORE_V:
              return "Datacenter Edition (core installation)";
            case PRODUCT_ENTERPRISE_SERVER:
              return "Enterprise Edition";
            case PRODUCT_ENTERPRISE_SERVER_CORE:
            case PRODUCT_ENTERPRISE_SERVER_CORE_V:
              return "Enterprise Edition (core installation)";
            case PRODUCT_ENTERPRISE_SERVER_IA64:
              return "Enterprise Edition for Itanium-based Systems";
            case PRODUCT_SMALLBUSINESS_SERVER:
              return "Small Business Server";
            //case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
            // return "Small Business Server Premium Edition";
            case PRODUCT_SERVER_FOR_SMALLBUSINESS:
            case PRODUCT_SERVER_FOR_SMALLBUSINESS_V:
              return "Windows Essential Server Solutions";
            case PRODUCT_STANDARD_SERVER:
            case PRODUCT_STANDARD_SERVER_V:
              return "Standard Edition";
            case PRODUCT_STANDARD_SERVER_CORE:
            case PRODUCT_STANDARD_SERVER_CORE_V:
              return "Standard Edition (core installation)";
            case PRODUCT_WEB_SERVER:
            case PRODUCT_WEB_SERVER_CORE:
              return "Web Server Edition";
            case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT:
            case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING:
            case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY:
              return "Windows Essential Business Server ";
            case PRODUCT_STORAGE_ENTERPRISE_SERVER:
            case PRODUCT_STORAGE_EXPRESS_SERVER:
            case PRODUCT_STORAGE_STANDARD_SERVER:
            case PRODUCT_STORAGE_WORKGROUP_SERVER:
              return "Storage Server";
          }
          break;
      }
      return string.Empty;
    }

    /// <summary>
    /// Returns the service pack information of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system service pack information.</returns>
    public static string GetOSServicePack()
    {
      OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
      osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
      return !GetVersionEx(ref osVersionInfo) ? string.Empty : osVersionInfo.szCSDVersion;
    }

    /// <summary>
    /// Returns the name of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system name.</returns>
    public static string GetOSNameString()
    {
      OperatingSystem osInfo = Environment.OSVersion;
      string osName = "UNKNOWN";

      switch (osInfo.Platform)
      {
        case PlatformID.Win32Windows:
          {
            switch (OSMinorVersion)
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
            switch (OSMajorVersion)
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
                  switch (OSMinorVersion)
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
                  switch (OSMinorVersion)
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
                    case 3:
                      osName = OSProductType == NT_WORKSTATION ? "Windows 81" : "Windows 2012";
                      break;
                  }
                  break;
                }
            }
            break;
          }
      }

      if (!IsOSAsReported(OSMajorVersion, OSMinorVersion, OSBuildVersion, OSProductType, (short)OSServicePackMajor))
      {
        osName = "Compatibilty Mode: " + osName;
      }

      return osName;
    }

    /// <summary>
    /// Returns the name of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system name.</returns>
    public static OSList GetOSName()
    {
      switch (OsVersionInt())
      {
        case 51:
          return OSList.WindowsXp;
        case 52:
          if (OSProductType == NT_WORKSTATION)
          {
            return OSList.WindowsXp64;
          }
          return GetSystemMetrics(SM_SERVERR2) ? OSList.Windows2003R2 : OSList.Windows2003;
        case 60:
          return OSProductType == NT_WORKSTATION ? OSList.WindowsVista : OSList.Windows2008;
        case 61:
          return OSProductType == NT_WORKSTATION ? OSList.Windows7 : OSList.Windows2008R2;
        case 62:
          return OSProductType == NT_WORKSTATION ? OSList.Windows8 : OSList.Windows2012;
        case 63:
          return OSProductType == NT_WORKSTATION ? OSList.Windows81 : OSList.Windows2012;
      }
      return OSList.Windows2000andPrevious;
    }

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
      return GetOSNameString() + servicePack + " [" + OSVersion + "]";
    }

    /// <summary>
    /// Return a value that indicate if the OS is blocked, supported, or officially unsupported
    /// </summary>
    public static OsSupport GetOSSupported()
    {

      if (VerifyDesktopOSMinRequirement(5, 1, 2600, NT_WORKSTATION, 3))
      { // XP SP3
        return OsSupport.NotSupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 0, 6000, NT_WORKSTATION, 2))
      { // Vista SP2
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 1, 7600, NT_WORKSTATION, 0))
      { // Win7 RTM
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 2, 9200, NT_WORKSTATION, 0))
      { // Windows 8 RTM
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 3, 9431, NT_WORKSTATION, 0))
      { // Windows 8.1 Preview
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 3, 9600, NT_WORKSTATION, 0))
      { // Windows 8.1 RTM
        return OsSupport.FullySupported;
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
      return VerifyVersionGreaterEqual(5, 1);
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
      return VerifyVersionGreaterEqual(6, 0);
    }

    /// <summary>
    /// Return if running on Windows7 or later
    /// </summary>
    /// <returns>true means Windows7 or later</returns>
    /// <returns>false means Vista or previous</returns>
    public static bool Win7OrLater()
    {
      return VerifyVersionGreaterEqual(6, 1);
    }

    /// <summary>
    /// Return if running on Windows8 or later
    /// </summary>
    /// <returns>true means Windows8 or later</returns>
    /// <returns>false means Win7 or previous</returns>
    public static bool Win8OrLater()
    {
      return VerifyVersionGreaterEqual(6, 2);
    }

    /// <summary>
    /// Return if running on Windows8.1 or later
    /// </summary>
    /// <returns>true means Windows8.1 or later</returns>
    /// <returns>false means Win8 or previous</returns>
    public static bool Win81OrLater()
    {
      return VerifyVersionGreaterEqual(6, 3);
    }

    /// <summary>
    /// Return a numeric value rappresenting OS version
    /// </summary>
    /// <returns>(OSMajorVersion * 10 + OSMinorVersion)</returns>
    public static int OsVersionInt()
    {
      return (OSMajorVersion * 10 + OSMinorVersion);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the full version of the operating system running on this computer.
    /// </summary>
    public static string OSVersion
    {
      get { return Environment.OSVersion.Version.ToString(); }
    }

    /// <summary>
    /// Gets the major version of the operating system running on this computer.
    /// </summary>
    public static int OSMajorVersion
    {
      get { return Environment.OSVersion.Version.Major; }
    }

    /// <summary>
    /// Gets the minor version of the operating system running on this computer.
    /// </summary>
    public static int OSMinorVersion
    {
      get { return Environment.OSVersion.Version.Minor; }
    }

    /// <summary>
    /// Gets the build version of the operating system running on this computer.
    /// </summary>
    public static int OSBuildVersion
    {
      get { return Environment.OSVersion.Version.Build; }
    }

    /// <summary>
    /// Gets the revision version of the operating system running on this computer.
    /// </summary>
    public static int OSRevisionVersion
    {
      get { return Environment.OSVersion.Version.Revision; }
    }

    /// <summary>
    /// Gets the main version of the service pack running on this computer.
    /// </summary>
    public static int OSServicePackMajor
    {
      get
      {
        OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
        if (!GetVersionEx(ref osVersionInfo)) return -1;
        return osVersionInfo.wServicePackMajor;
      }
    }

    /// <summary>
    /// Gets the main version of the service pack running on this computer.
    /// </summary>
    public static int OSServicePackMinor
    {
      get
      {
        OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
        return !GetVersionEx(ref osVersionInfo) ? -1 : osVersionInfo.wServicePackMinor;
      }
    }

    /// <summary>
    /// Gets the string description of the service pack running on this computer.
    /// </summary>
    public static string OSServicePackDesc
    {
      get
      {
        OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
        return !GetVersionEx(ref osVersionInfo) ? String.Empty : osVersionInfo.szCSDVersion;
      }
    }

    /// <summary>
    /// Gets the product type of the operating system running on this computer.
    /// </summary>
    public static byte OSProductType
    {
      get
      {
        OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));
        if (!GetVersionEx(ref osVersionInfo)) return 0x0;
        return osVersionInfo.wProductType;
      }
    }

    #endregion

    #region private methods

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
        dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)),
        dwMajorVersion = majorVersion,
        dwMinorVersion = minorVersion
      };
      condition = VerSetConditionMask(condition, VER_MAJORVERSION, VER_GREATER_EQUAL);
      condition = VerSetConditionMask(condition, VER_MINORVERSION, VER_GREATER_EQUAL);
      return VerifyVersionInfo(ref osVersionInfo, VER_MAJORVERSION | VER_MINORVERSION, condition);
    }

    /// <summary>
    /// Checks OS for required service pack and build version
    /// </summary>
    /// <param name="majorVersion">Major OS version</param>
    /// <param name="minorVersion">Minor OS version</param>
    /// <param name="buildVersion">OS Build Version</param>
    /// <param name="productType">OS Product Type</param>
    /// <param name="servicePack">Minimum Major Service PackVersion</param>
    /// <returns>True if Major / Minor OS versions match and service pack / build version are >= parameters</returns>
    private static bool VerifyDesktopOSMinRequirement(int majorVersion, int minorVersion, int buildVersion, byte productType, short servicePack)
    {
      ulong condition = 0;
      var osVersionInfo = new OSVERSIONINFOEX
      {
        dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)),
        dwMajorVersion = majorVersion,
        dwMinorVersion = minorVersion,
        dwBuildNumber = buildVersion,
        wProductType = productType,
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
    /// Checks whether the OS version reported via GetVersionEx matches that of VerifyVersionInfo
    /// When running in compatibility mode GetVersionEx can return the value of the
    /// compatibility setting rather than the actual OS
    /// </summary>
    /// <param name="majorVersion">Reported OS Major Version</param>
    /// <param name="minorVersion">Reported OS Minor Version</param>
    /// <param name="buildVersion">Reported OS Build Version</param>
    /// <param name="productType">Reported OS Product Type</param>
    /// <param name="servicePack">Reported OS Major Service Pack Version</param>
    /// <returns>True if actual OS matches reported one</returns>
    private static bool IsOSAsReported(int majorVersion, int minorVersion, int buildVersion, byte productType, short servicePack)
    {
      ulong condition = 0;
      var osVersionInfo = new OSVERSIONINFOEX
      {
        dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX)),
        dwMajorVersion = majorVersion,
        dwMinorVersion = minorVersion,
        dwBuildNumber = buildVersion,
        wProductType = productType,
        wServicePackMajor = servicePack
      };
      condition = VerSetConditionMask(condition, VER_MAJORVERSION, VER_EQUAL);
      condition = VerSetConditionMask(condition, VER_MINORVERSION, VER_EQUAL);
      condition = VerSetConditionMask(condition, VER_PRODUCT_TYPE, VER_EQUAL);
      condition = VerSetConditionMask(condition, VER_SERVICEPACKMAJOR, VER_EQUAL);
      condition = VerSetConditionMask(condition, VER_BUILDVERSION, VER_EQUAL);
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