// Copyright (C) 2005-2016 Team MediaPortal
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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using WUApiLib;

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
      public SuiteFlags wSuiteMask;
      public NTProductType wProductType;
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
    private static extern bool VerifyVersionInfo(ref OSVERSIONINFOEX osVersionInfo, [In] VersionTypeFlags dwTypeMask,
      [In] ulong dwlConditionMask);

    [DllImport("kernel32.dll")]
    private static extern ulong VerSetConditionMask([In] ulong dwlConditionMask, [In] VersionTypeFlags dwTypeBitMask,
      [In] ConditionType dwConditionMask);

    #endregion

    #region Private Constants

    //sysMetrics ( http://msdn.microsoft.com/en-us/library/ms724385(VS.85).aspx )
    private const int SM_TABLETPC = 86;
    private const int SM_MEDIACENTER = 87;
    private const int SM_STARTER = 88;
    private const int SM_SERVERR2 = 89;

    #endregion

    #region enums

    //wProductType ( http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx )
    public enum NTProductType : byte
    {
      [Description("Workstation")] NT_WORKSTATION = 1,
      [Description("Domain Controller")] NT_DOMAIN_CONTROLLER = 2,
      [Description("Server")] NT_SERVER = 3
    }

    //SuiteMask ( http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx )
    [Flags]
    private enum SuiteFlags : ushort
    {
      [Description("(Microsoft Small Business Server was once installed)")] VER_SUITE_SMALLBUSINESS = 0x00000001,
      [Description("Enterprise")] VER_SUITE_ENTERPRISE = 0x00000002,
      [Description("(with Microsoft BackOffice components)")] VER_SUITE_BACKOFFICE = 0x00000004,
      [Description("Microsoft Office Communications Server")] VER_SUITE_COMMUNICATIONS = 0x00000008,
      [Description("(with Terminal Services installed)")] VER_SUITE_TERMINAL = 0x00000010,
      [Description("(Microsoft Small Business Server is installed with the restrictive client license in force)")] VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x00000020,
      [Description("Windows XP Embedded")] VER_SUITE_EMBEDDEDNT = 0x00000040,
      [Description("Datacenter")] VER_SUITE_DATACENTER = 0x00000080,
      [Description("(only one Remote Desktop interactive session is supported)")] VER_SUITE_SINGLEUSERTS = 0x00000100,
      [Description("Home")] VER_SUITE_PERSONAL = 0x00000200,
      [Description("Web Edition")] VER_SUITE_BLADE = 0x00000400,
      [Description("Embedded restricted")] VER_SUITE_EMBEDDED_RESTRICTED = 0x00000800,
      VER_SUITE_SECURITY_APPLIANCE = 0x00001000,
      [Description("Storage Server")] VER_SUITE_STORAGE_SERVER = 0x00002000,
      [Description("Compute Cluster Edition")] VER_SUITE_COMPUTE_SERV = 0x00004000,
      [Description("Home Server")] VER_SUITE_WH_SERVER = 0x00008000
    }

    //ProductType ( http://msdn.microsoft.com/en-us/library/ms724358(VS.85).aspx )
    private enum ProductType
    {
      [Description("undefined")] PRODUCT_UNDEFINED = 0x00000000,
      [Description("Ultimate")] PRODUCT_ULTIMATE = 0x00000001,
      [Description("Home Basic")] PRODUCT_HOME_BASIC = 0x00000002,
      [Description("Home Premium")] PRODUCT_HOME_PREMIUM = 0x00000003,
      [Description("Enterprise")] PRODUCT_ENTERPRISE = 0x00000004,
      [Description("Home Basic N")] PRODUCT_HOME_BASIC_N = 0x00000005,
      [Description("Business")] PRODUCT_BUSINESS = 0x00000006,
      [Description("Server Standard")] PRODUCT_STANDARD_SERVER = 0x00000007,
      [Description("Server Datacenter (full installation)")] PRODUCT_DATACENTER_SERVER = 0x00000008,
      [Description("Small Business Server")] PRODUCT_SMALLBUSINESS_SERVER = 0x00000009,
      [Description("Server Enterprise (full installation)")] PRODUCT_ENTERPRISE_SERVER = 0x0000000A,
      [Description("Starter")] PRODUCT_STARTER = 0x0000000B,
      [Description("Server Datacenter (core installation)")] PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C,
      [Description("Server Standard (core installation)")] PRODUCT_STANDARD_SERVER_CORE = 0x0000000D,
      [Description("Server Enterprise (core installation)")] PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E,
      [Description("Server Enterprise for Itanium-based Systems")] PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F,
      [Description("Business N")] PRODUCT_BUSINESS_N = 0x00000010,
      [Description("Web Server (full installation)")] PRODUCT_WEB_SERVER = 0x00000011,
      [Description("HPC (Cluster Server) Edition")] PRODUCT_CLUSTER_SERVER = 0x00000012,
      //[Description("Storage Server 2008 R2 Essentials")] - too specific
      [Description("Storage Server Essentials")] PRODUCT_HOME_SERVER = 0x00000013,
      [Description("Storage Server Express")] PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014,
      [Description("Storage Server Standard")] PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015,
      [Description("Storage Server Workgroup")] PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016,
      [Description("Storage Server Enterprise")] PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017,
      //[Description("Server 2008 for Windows Essential Server Solutions")]
      [Description("Server for Windows Essential Server Solutions")] PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018,
      [Description("Small Business Server Premium")] PRODUCT_SMALLBUSINESS_SERVER_PREMIUM = 0x00000019,
      [Description("Home Premium N")] PRODUCT_HOME_PREMIUM_N = 0x0000001A,
      [Description("Enterprise N")] PRODUCT_ENTERPRISE_N = 0x0000001B,
      [Description("Ultimate N")] PRODUCT_ULTIMATE_N = 0x0000001C,
      [Description("Web Server (core installation)")] PRODUCT_WEB_SERVER_CORE = 0x0000001D,
      [Description("Essential Business Server Management Server")] PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E,
      [Description("Essential Business Server Security Server")] PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F,
      [Description("Essential Business Server Messaging Server")] PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020,
      [Description("Server Foundation")] PRODUCT_SERVER_FOUNDATION = 0x00000021,
      //[Description("Home Server 2011")] - too specific
      [Description("Home Server")] PRODUCT_HOME_PREMIUM_SERVER = 0x00000022,
      //[Description("Server 2008 without Hyper-V for Windows Essential Server Solutions")]
      [Description("Server without Hyper-V for Windows Essential Server Solutions")] PRODUCT_SERVER_FOR_SMALLBUSINESS_V
      = 0x00000023,
      [Description("Server Standard without Hyper-V")] PRODUCT_STANDARD_SERVER_V = 0x00000024,
      [Description("Server Datacenter without Hyper-V (full installation)")] PRODUCT_DATACENTER_SERVER_V = 0x00000025,
      [Description("Server Enterprise without Hyper-V (full installation)")] PRODUCT_ENTERPRISE_SERVER_V = 0x00000026,

      [Description("Server Datacenter without Hyper-V (core installation)")] PRODUCT_DATACENTER_SERVER_CORE_V =
        0x00000027,
      [Description("Server Standard without Hyper-V (core installation)")] PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028,

      [Description("Server Enterprise without Hyper-V (core installation)")] PRODUCT_ENTERPRISE_SERVER_CORE_V =
        0x00000029,
      [Description("Hyper-V Server")] PRODUCT_HYPERV = 0x0000002A,
      [Description("Storage Server Express (core installation)")] PRODUCT_STORAGE_EXPRESS_SERVER_CORE = 0x0000002B,
      [Description("Storage Server Standard (core installation)")] PRODUCT_STORAGE_STANDARD_SERVER_CORE = 0x0000002C,
      [Description("Storage Server Workgroup (core installation)")] PRODUCT_STORAGE_WORKGROUP_SERVER_CORE = 0x0000002D,
      [Description("Storage Server Enterprise (core installation)")] PRODUCT_STORAGE_ENTERPRISE_SERVER_CORE = 0x0000002E,
      [Description("Starter N")] PRODUCT_STARTER_N = 0x0000002F,
      [Description("Professional")] PRODUCT_PROFESSIONAL = 0x00000030,
      [Description("Professional N")] PRODUCT_PROFESSIONAL_N = 0x00000031,
      //[Description("Small Business Server 2011 Essentials")] - obviously wrong
      [Description("Server Essentials")] PRODUCT_SB_SOLUTION_SERVER = 0x00000032,
      [Description("Server For SB Solutions")] PRODUCT_SERVER_FOR_SB_SOLUTIONS = 0x00000033,
      [Description("Server Solutions Premium ")] PRODUCT_STANDARD_SERVER_SOLUTIONS = 0x00000034,
      [Description("Server Solutions Premium (core installation)")] PRODUCT_STANDARD_SERVER_SOLUTIONS_CORE = 0x00000035,
      [Description("Server For SB Solutions EM")] PRODUCT_SB_SOLUTION_SERVER_EM = 0x00000036,
      [Description("Server For SB Solutions EM")] PRODUCT_SERVER_FOR_SB_SOLUTIONS_EM = 0x00000037,
      PRODUCT_SOLUTION_EMBEDDEDSERVER = 0x00000038,
      PRODUCT_SOLUTION_EMBEDDEDSERVER_CORE = 0x00000039,
      PRODUCT_PROFESSIONAL_EMBEDDED = 0x0000003A,
      [Description("Essential Server Solution Management")] PRODUCT_ESSENTIALBUSINESS_SERVER_MGMT = 0x0000003B,
      [Description("Essential Server Solution Additional")] PRODUCT_ESSENTIALBUSINESS_SERVER_ADDL = 0x0000003C,
      [Description("Essential Server Solution Management SVC")] PRODUCT_ESSENTIALBUSINESS_SERVER_MGMTSVC = 0x0000003D,
      [Description("Essential Server Solution Additional SVC")] PRODUCT_ESSENTIALBUSINESS_SERVER_ADDLSVC = 0x0000003E,

      [Description("Small Business Server Premium (core installation)")] PRODUCT_SMALLBUSINESS_SERVER_PREMIUM_CORE =
        0x0000003F,
      [Description("Server Hyper Core V")] PRODUCT_CLUSTER_SERVER_V = 0x00000040,
      [Description("Embedded")] PRODUCT_EMBEDDED = 0x00000041,
      [Description("Starter E")] PRODUCT_STARTER_E = 0x00000042,
      [Description("Home Basic E")] PRODUCT_HOME_BASIC_E = 0x00000043,
      [Description("Home Premium E")] PRODUCT_HOME_PREMIUM_E = 0x00000044,
      [Description("Professional E")] PRODUCT_PROFESSIONAL_E = 0x00000045,
      [Description("Enterprise E")] PRODUCT_ENTERPRISE_E = 0x00000046,
      [Description("Ultimate E")] PRODUCT_ULTIMATE_E = 0x00000047,
      [Description("Enterprise (evaluation installation)")] PRODUCT_ENTERPRISE_EVALUATION = 0x00000048,
      [Description("MultiPoint Server Standard (full installation)")] PRODUCT_MULTIPOINT_STANDARD_SERVER = 0x0000004C,
      [Description("MultiPoint Server Premium (full installation)")] PRODUCT_MULTIPOINT_PREMIUM_SERVER = 0x0000004D,
      [Description("Server Standard (evaluation installation)")] PRODUCT_STANDARD_EVALUATION_SERVER = 0x0000004F,
      [Description("Server Datacenter (evaluation installation)")] PRODUCT_DATACENTER_EVALUATION_SERVER = 0x00000050,
      [Description("Enterprise N (evaluation installation)")] PRODUCT_ENTERPRISE_N_EVALUATION = 0x00000054,
      PRODUCT_EMBEDDED_AUTOMOTIVE = 0x00000055,
      PRODUCT_EMBEDDED_INDUSTRY_A = 0x00000056,
      PRODUCT_THINPC = 0x00000057,
      PRODUCT_EMBEDDED_A = 0x00000058,
      PRODUCT_EMBEDDED_INDUSTRY = 0x00000059,
      PRODUCT_EMBEDDED_E = 0x0000005A,
      PRODUCT_EMBEDDED_INDUSTRY_E = 0x0000005B,
      PRODUCT_EMBEDDED_INDUSTRY_A_E = 0x0000005C,

      [Description("Storage Server Workgroup (evaluation installation)")] PRODUCT_STORAGE_WORKGROUP_EVALUATION_SERVER =
        0x0000005F,

      [Description("Storage Server Standard (evaluation installation)")] PRODUCT_STORAGE_STANDARD_EVALUATION_SERVER =
        0x00000060,
      PRODUCT_CORE_ARM = 0x00000061,
      [Description("Home N")] PRODUCT_CORE_N = 0x00000062,
      [Description("Home China")] PRODUCT_CORE_COUNTRYSPECIFIC = 0x00000063,
      [Description("Home Single Language")] PRODUCT_CORE_SINGLELANGUAGE = 0x00000064,
      [Description("Home")] PRODUCT_CORE = 0x00000065,
      [Description("Professional with Media Center")] PRODUCT_PROFESSIONAL_WMC = 0x00000067,

      [Description("Mobile")] // Windows 10 mobile
      PRODUCT_MOBILE_CORE = 0x00000068,
      PRODUCT_EMBEDDED_INDUSTRY_EVAL = 0x00000069,
      PRODUCT_EMBEDDED_INDUSTRY_E_EVAL = 0x0000006A,
      [Description("Embedded (evaluation installation)")] PRODUCT_EMBEDDED_EVAL = 0x0000006B,
      [Description("Embedded E (evaluation installation)")] PRODUCT_EMBEDDED_E_EVAL = 0x0000006C,
      PRODUCT_NANO_SERVER = 0x0000006D,
      PRODUCT_CLOUD_STORAGE_SERVER = 0x0000006E,
      PRODUCT_CORE_CONNECTED = 0x0000006F,
      PRODUCT_PROFESSIONAL_STUDENT = 0x00000070,
      PRODUCT_CORE_CONNECTED_N = 0x00000071,
      PRODUCT_PROFESSIONAL_STUDENT_N = 0x00000072,
      PRODUCT_CORE_CONNECTED_SINGLELANGUAGE = 0x00000073,
      PRODUCT_CORE_CONNECTED_COUNTRYSPECIFIC = 0x00000074,
      PRODUCT_CONNECTED_CAR = 0x00000075,
      PRODUCT_INDUSTRY_HANDHELD = 0x00000076,
      PRODUCT_PPI_PRO = 0x00000077,
      PRODUCT_ARM64_SERVER = 0x00000078,

      [Description("Education")] //Windows 10 Education"
      PRODUCT_EDUCATION = 0x00000079,

      [Description("Education N")] //Windows 10 Education N
      PRODUCT_EDUCATION_N = 0x0000007A,

      [Description("IoT Core")] //Windows 10 IoT Core
      PRODUCT_IOTUAP = 0x0000007B,
      PRODUCT_CLOUD_HOST_INFRASTRUCTURE_SERVER = 0x0000007C,
      [Description("Enterprise 2015 LTSB")] PRODUCT_ENTERPRISE_S = 0x0000007D,
      [Description("Enterprise 2015 LTSB N")] PRODUCT_ENTERPRISE_S_N = 0x0000007E,
      [Description("Professional 2015 LTSB")] PRODUCT_PROFESSIONAL_S = 0x0000007F,
      [Description("Professional 2015 LTSB N")] PRODUCT_PROFESSIONAL_S_N = 0x00000080,
      [Description("Enterprise 2015 LTSB (evaluation installation)")] PRODUCT_ENTERPRISE_S_EVALUATION = 0x00000081,
      [Description("Enterprise 2015 LTSB N (evaluation installation)")] PRODUCT_ENTERPRISE_S_N_EVALUATION = 0x00000082,

      [Description("Mobile Enterprise")] //Windows 10 Mobile Enterprise
      PRODUCT_MOBILE_ENTERPRISE = 0x00000085,
      [Description("not licensed!")] PRODUCT_UNLICENSED = -1412584499 //  0xABCDABCD
    }

    //Type bitmask ( http://msdn.microsoft.com/en-gb/library/ms725494(vs.85).aspx )
    [Flags]
    private enum VersionTypeFlags : uint
    {
      VER_MINORVERSION = 0x0000001,
      VER_MAJORVERSION = 0x0000002,
      VER_BUILDVERSION = 0x0000004,
      VER_PLATFORMID = 0x0000008,
      VER_SERVICEPACKMINOR = 0x0000010,
      VER_SERVICEPACKMAJOR = 0x0000020,
      VER_SUITENAME = 0x0000040,
      VER_PRODUCT_TYPE = 0x0000080
    }

    //Condition bitmask ( http://msdn.microsoft.com/en-gb/library/ms725494(vs.85).aspx )
    private enum ConditionType : byte
    {
      VER_EQUAL = 1,
      VER_GREATER = 2,
      VER_GREATER_EQUAL = 3,
      VER_LESS = 4,
      VER_LESS_EQUAL = 5,
      VER_AND = 6, // only for wSuiteMask
      VER_OR = 7 // only for wSuiteMask
    }

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
      /// Windows 81
      ///</summary>
      Windows81,

      ///<summary>
      /// Windows 10
      ///</summary>
      Windows10,

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
      Windows2012,

      ///<summary>
      /// Windows 2012 R2 Server
      ///</summary>
      Windows2012R2,

      ///<summary>
      /// Windows 2016 Server
      ///</summary>
      Windows2016
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
      var osVersionInfo = new OSVERSIONINFOEX {dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX))};
      if (!GetVersionEx(ref osVersionInfo)) return string.Empty;

      switch (OSMajorVersion)
      {
          #region VERSION 4

        case 4:
          if (OSProductType == NTProductType.NT_WORKSTATION)
            // Windows NT 4.0 Workstation
            return "Workstation";
          else if (OSProductType == NTProductType.NT_SERVER)
          {
            if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_ENTERPRISE))
              // Windows NT 4.0 Server Enterprise
              return "Enterprise Server";
            else
            // Windows NT 4.0 Server
              return "Standard Server";
          }
          return string.Empty;

          #endregion

          #region VERSION 5

        case 5:
          if (GetSystemMetrics(SM_MEDIACENTER))
            return "Media Center";
          else if (GetSystemMetrics(SM_TABLETPC))
            return "Tablet PC";
          else if (OSProductType == NTProductType.NT_WORKSTATION)
          {
            if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_EMBEDDEDNT))
            {
              //Windows XP Embedded
              return "Embedded";
            }
            else if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_PERSONAL))
            {
              // Windows XP Home Edition
              return "Home";
            }
            else
            {
              // Windows XP / Windows 2000 Professional
              return "Professional";
            }
          }
          else if (OSProductType == NTProductType.NT_SERVER || OSProductType == NTProductType.NT_DOMAIN_CONTROLLER)
          {
            if (OSMinorVersion == 0)
            {
              if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_DATACENTER))
                // Windows 2000 Datacenter Server
                return "Datacenter Server";
              else if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_ENTERPRISE))
                // Windows 2000 Advanced Server
                return "Advanced Server";
              else
              // Windows 2000 Server
                return "Server";
            }
            else if (OSMinorVersion == 2)
            {
              if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_DATACENTER))
                // Windows Server 2003 Datacenter Edition
                return "Datacenter Edition";
              else if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_ENTERPRISE))
                // Windows Server 2003 Enterprise Edition
                return "Enterprise Edition";
              else if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_STORAGE_SERVER))
                // Windows Server 2003 Storage Edition
                return "Storage Edition";
              else if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_COMPUTE_SERV))
                // Windows Server 2003 Compute Cluster Edition
                return "Compute Cluster Edition";
              else if (osVersionInfo.wSuiteMask.HasFlag(SuiteFlags.VER_SUITE_BLADE))
                // Windows Server 2003 Web Edition
                return "Web Edition";
              else
              // Windows Server 2003 Standard Edition
                return "Standard Edition";
            }
          }
          break;

          #endregion

          #region VERSION 6 and 10

        case 6:
        case 10:
          int intProductType;
          GetProductInfo(osVersionInfo.dwMajorVersion, osVersionInfo.dwMinorVersion, 0, 0, out intProductType);
          return ((ProductType) intProductType).DescriptionAttr();

          #endregion
      }
      return string.Empty;
    }

    /// <summary>
    /// Returns the service pack information of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system service pack information.</returns>
    public static string GetOSServicePack()
    {
      var osVersionInfo = new OSVERSIONINFOEX {dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX))};
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
              osName = "Windows 95";
              break;
            case 10:
              osName = osInfo.Version.Revision == 2222 ? "Windows 98 Second Edition" : "Windows 98";
              break;
            case 90:
              osName = "Windows Me";
              break;
          }
          break;
        }
        case PlatformID.Win32NT:
        {
          switch (OSMajorVersion)
          {
            case 3:
              osName = "Windows NT 3.51";
              break;
            case 4:
              osName = "Windows NT 4.0";
              break;
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
                  if (OSProductType == NTProductType.NT_WORKSTATION)
                    osName = "WindowsXP x64";
                  else
                    osName = GetSystemMetrics(SM_SERVERR2) ? "Windows Server 2003 R2" : "Windows Server 2003";
                  break;
              }
              break;
            }
            case 6:
            {
              switch (OSMinorVersion)
              {
                case 0:
                  osName = OSProductType == NTProductType.NT_WORKSTATION ? "Windows Vista" : "Windows 2008";
                  break;
                case 1:
                  osName = OSProductType == NTProductType.NT_WORKSTATION ? "Windows 7" : "Windows 2008 R2";
                  break;
                case 2:
                  osName = OSProductType == NTProductType.NT_WORKSTATION ? "Windows 8" : "Windows 2012";
                  break;
                case 3:
                  osName = OSProductType == NTProductType.NT_WORKSTATION ? "Windows 8.1" : "Windows 2012 R2";
                  break;
              }
              break;
            }
            case 10:
            {
              switch (OSMinorVersion)
              {
                case 0:
                  osName = OSProductType == NTProductType.NT_WORKSTATION ? "Windows 10" : "Windows 2016";
                  break;
              }
              break;
            }
          }
          break;
        }
      }
      if (
        !IsOSAsReported(OSMajorVersion, OSMinorVersion, OSBuildVersion, (byte) OSProductType, (short) OSServicePackMajor))
        osName = "Compatibilty Mode: " + osName;

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
          if (OSProductType == NTProductType.NT_WORKSTATION)
            return OSList.WindowsXp64;
          else
            return GetSystemMetrics(SM_SERVERR2) ? OSList.Windows2003R2 : OSList.Windows2003;
        case 60:
          return OSProductType == NTProductType.NT_WORKSTATION ? OSList.WindowsVista : OSList.Windows2008;
        case 61:
          return OSProductType == NTProductType.NT_WORKSTATION ? OSList.Windows7 : OSList.Windows2008R2;
        case 62:
          return OSProductType == NTProductType.NT_WORKSTATION ? OSList.Windows8 : OSList.Windows2012;
        case 63:
          return OSProductType == NTProductType.NT_WORKSTATION ? OSList.Windows81 : OSList.Windows2012R2;
        case 10:
          return OSProductType == NTProductType.NT_WORKSTATION ? OSList.Windows10 : OSList.Windows2016;
      }
      return OSList.Windows2000andPrevious;
    }

    /// <summary>
    /// Return a full version string, e.g. "Windows XP ( Servicepack 2 ) [5.1.0000]"
    /// </summary>
    /// <returns>A string rappresenting a fully displayable version</returns>
    public static string GetOSDisplayVersion()
    {
      string servicePack = GetOSServicePack();
      if (!string.IsNullOrEmpty(servicePack))
        servicePack = " (" + servicePack + ")";
      return string.Format("{0} {1}{2} {3} [{4}]", GetOSNameString(), GetOSProductType(), servicePack,
        GetOSx64orx32String(), OSVersion);
    }

    /// <summary>
    /// Returns a string containing description of OS "bitness"
    /// </summary>
    /// <returns>string containing either x32 or x64</returns>
    public static string GetOSx64orx32String()
    {
      return Environment.Is64BitOperatingSystem ? "x64" : "x32";
    }

    /// <summary>
    /// Return a value that indicate if the OS is blocked, supported, or officially unsupported
    /// </summary>
    public static OsSupport GetOSSupported()
    {
      if (VerifyDesktopOSMinRequirement(5, 1, 2600, (byte) NTProductType.NT_WORKSTATION, 3))
      {
        // XP SP3
        return OsSupport.NotSupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 0, 6000, (byte) NTProductType.NT_WORKSTATION, 2))
      {
        // Vista SP2
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 1, 7600, (byte) NTProductType.NT_WORKSTATION, 0))
      {
        // Win7 RTM
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 2, 9200, (byte) NTProductType.NT_WORKSTATION, 0))
      {
        // Windows 8 RTM
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 3, 9431, (byte) NTProductType.NT_WORKSTATION, 0))
      {
        // Windows 8.1 Preview
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(6, 3, 9600, (byte) NTProductType.NT_WORKSTATION, 0))
      {
        // Windows 8.1 RTM
        return OsSupport.FullySupported;
      }
      if (VerifyDesktopOSMinRequirement(10, 0, 10240, (byte) NTProductType.NT_WORKSTATION, 0))
      {
        // Windows 10 RTM
        return OsSupport.FullySupported;
      }
      if (IsServer())
      {
        // any server OS
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
    /// Return if running on Windows10 or later
    /// </summary>
    /// <returns>true means Windows 10 or later</returns>
    /// <returns>false means Win 8.1 or previous</returns>
    public static bool Win10OrLater()
    {
      return VerifyVersionGreaterEqual(10, 0);
    }

    /// <summary>
    /// Return a numeric value rappresenting OS version
    /// </summary>
    /// <returns>(OSMajorVersion * 10 + OSMinorVersion)</returns>
    public static int OsVersionInt()
    {
      return OSMajorVersion < 10 ? OSMajorVersion*10 + OSMinorVersion : OSMajorVersion + OSMinorVersion;
    }

    /// <summary>
    /// Return string with installed Windows Media Player version
    /// </summary>
    /// <returns>Version of WMP.dll, or null if no VersionInfo, or "not installed" when DLL not found</returns>
    public static string GetWMPVersion()
    {
      string pathToWmp = Environment.SystemDirectory + "\\wmp.dll";
      return File.Exists(pathToWmp) ? FileVersionInfo.GetVersionInfo(pathToWmp).ProductVersion : "not installed";
    }

    /// <summary>
    /// Return DateTime of last installed Windows Update (excluding Security Essentials definition updates)
    /// </summary>
    /// <returns>DateTime (in UTC), or DateTime.MinValue if not found</returns>
    public static DateTime GetLastInstalledWindowsUpdateTimestamp()
    {
      var session = new UpdateSession();
      var updateSearcher = session.CreateUpdateSearcher();
      if (updateSearcher.ClientApplicationID != null)
      {
        updateSearcher.Online = false;
        int count = updateSearcher.GetTotalHistoryCount();
        var history = updateSearcher.QueryHistory(0, count);
        for (int i = 0; i < count; ++i)
        {
          if ((history[i].ResultCode == OperationResultCode.orcSucceeded) &&
              (!history[i].Title.Contains("Security Essentials")))
            return history[i].Date;
        }
      }
      return DateTime.MinValue;
    }

    /// <summary>
    /// Return string with last installed Windows Update timestamp (excluding Security Essentials definition updates)
    /// </summary>
    /// <returns>timestamp (in UTC) as string, or "NEVER !!!" if not found</returns>
    public static string GetLastInstalledWindowsUpdateTimestampAsString()
    {
      DateTime dt = GetLastInstalledWindowsUpdateTimestamp();
      return "Last install from WindowsUpdate is dated " + ((dt == DateTime.MinValue)
        ? "NEVER !!!"
        : TimeZone.CurrentTimeZone.ToLocalTime(dt).ToString("yyyy-MM-dd HH:mm:ss"));
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
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));
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
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));
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
        osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX));
        return !GetVersionEx(ref osVersionInfo) ? String.Empty : osVersionInfo.szCSDVersion;
      }
    }

    /// <summary>
    /// Gets the product type of the operating system running on this computer.
    /// </summary>
    public static NTProductType OSProductType
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
        dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX)),
        dwMajorVersion = majorVersion,
        dwMinorVersion = minorVersion
      };
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_MAJORVERSION, ConditionType.VER_GREATER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_MINORVERSION, ConditionType.VER_GREATER_EQUAL);
      return VerifyVersionInfo(ref osVersionInfo, VersionTypeFlags.VER_MAJORVERSION | VersionTypeFlags.VER_MINORVERSION,
        condition);
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
    private static bool VerifyDesktopOSMinRequirement(int majorVersion, int minorVersion, int buildVersion,
      byte productType, short servicePack)
    {
      ulong condition = 0;
      var osVersionInfo = new OSVERSIONINFOEX
      {
        dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX)),
        dwMajorVersion = majorVersion,
        dwMinorVersion = minorVersion,
        dwBuildNumber = buildVersion,
        wProductType = (NTProductType) productType,
        wServicePackMajor = servicePack
      };
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_MAJORVERSION, ConditionType.VER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_MINORVERSION, ConditionType.VER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_PRODUCT_TYPE, ConditionType.VER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_SERVICEPACKMAJOR, ConditionType.VER_GREATER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_BUILDVERSION, ConditionType.VER_GREATER_EQUAL);
      return VerifyVersionInfo(ref osVersionInfo,
        VersionTypeFlags.VER_MAJORVERSION | VersionTypeFlags.VER_MINORVERSION |
        VersionTypeFlags.VER_PRODUCT_TYPE | VersionTypeFlags.VER_SERVICEPACKMAJOR |
        VersionTypeFlags.VER_BUILDVERSION, condition);
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
    private static bool IsOSAsReported(int majorVersion, int minorVersion, int buildVersion, byte productType,
      short servicePack)
    {
      ulong condition = 0;
      var osVersionInfo = new OSVERSIONINFOEX
      {
        dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX)),
        dwMajorVersion = majorVersion,
        dwMinorVersion = minorVersion,
        dwBuildNumber = buildVersion,
        wProductType = (NTProductType) productType,
        wServicePackMajor = servicePack
      };
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_MAJORVERSION, ConditionType.VER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_MINORVERSION, ConditionType.VER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_PRODUCT_TYPE, ConditionType.VER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_SERVICEPACKMAJOR, ConditionType.VER_EQUAL);
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_BUILDVERSION, ConditionType.VER_EQUAL);
      return VerifyVersionInfo(ref osVersionInfo,
        VersionTypeFlags.VER_MAJORVERSION | VersionTypeFlags.VER_MINORVERSION |
        VersionTypeFlags.VER_PRODUCT_TYPE | VersionTypeFlags.VER_SERVICEPACKMAJOR |
        VersionTypeFlags.VER_BUILDVERSION, condition);
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
        dwOSVersionInfoSize = Marshal.SizeOf(typeof (OSVERSIONINFOEX)),
        wProductType = NTProductType.NT_WORKSTATION // note the check is that this is not equal as per MS documenation
      };
      condition = VerSetConditionMask(condition, VersionTypeFlags.VER_PRODUCT_TYPE, ConditionType.VER_EQUAL);
      return !VerifyVersionInfo(ref osVersionInfo, VersionTypeFlags.VER_PRODUCT_TYPE, condition);
    }

    #endregion
  }

  public static class GenericDescriptionExtension
  {
    public static string DescriptionAttr<T>(this T source)
    {
      var fi = source.GetType().GetField(source.ToString());
      var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
      return attributes.Length > 0 ? attributes[0].Description : source.ToString();
    }
  }
}