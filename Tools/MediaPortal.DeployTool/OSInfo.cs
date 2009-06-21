using System;
using System.Runtime.InteropServices;

namespace OSInfo
{
  /// <summary>
  /// OSInfo Class
  /// </summary>
  public class OSInfo
  {
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

    #region Private Constants
    private const int VER_NT_WORKSTATION = 1;
    private const int VER_NT_DOMAIN_CONTROLLER = 2;
    private const int VER_NT_SERVER = 3;
    private const int VER_SUITE_SMALLBUSINESS = 1;
    private const int VER_SUITE_ENTERPRISE = 2;
    private const int VER_SUITE_TERMINAL = 16;
    private const int VER_SUITE_DATACENTER = 128;
    private const int VER_SUITE_SINGLEUSERTS = 256;
    private const int VER_SUITE_PERSONAL = 512;
    private const int VER_SUITE_BLADE = 1024;
    private const int VER_SUITE_WH_SERVER = 32768;
    #endregion

    #region Operating System enum
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
      /// Windows 2003 Server
      ///</summary>
      Windows2003,
      ///<summary>
      /// Windows 2008 Server
      ///</summary>
      Windows2008
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
          if (OSProductType == VER_NT_WORKSTATION)
          {
            // Windows NT 4.0 Workstation
            return " Workstation";
          }
          if (OSProductType == VER_NT_SERVER)
          {
            // Windows NT 4.0 Server
            return " Server";
          }
          return string.Empty;
        case 5:
          if (OSProductType == VER_NT_WORKSTATION)
          {
            if ((osVersionInfo.wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
            {
              // Windows XP Home Edition
              return " Home Edition";
            }
            // Windows XP / Windows 2000 Professional
            return " Professional";
          }
          if (OSProductType == VER_NT_SERVER)
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
          if (OSProductType == VER_NT_WORKSTATION)
          {
            if (OSMinorVersion == 0)
            {
              if ((osVersionInfo.wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
              {
                // Windows Vista Home Basic / Home Premium
                return " Home Basic/Premium";
              }
              // Windows Vista Business / Enterprise
              return " Business/Enterprise";
            }
            if (OSMinorVersion == 1)
            {
              return "Windows 7";
            }
          }
          if (OSProductType == VER_NT_SERVER)
          {
            if (OSMinorVersion == 0)
            {
              if ((osVersionInfo.wSuiteMask & VER_SUITE_DATACENTER) == VER_SUITE_DATACENTER)
              {
                // Windows 2008 Datacenter Server
                return " Datacenter Server";
              }
              if ((osVersionInfo.wSuiteMask & VER_SUITE_ENTERPRISE) == VER_SUITE_ENTERPRISE)
              {
                // Windows 2008 Advanced Server
                return " Advanced Server";
              }
              if ((osVersionInfo.wSuiteMask & VER_SUITE_WH_SERVER) == VER_SUITE_WH_SERVER)
              {
                return " Home Server";
              }
              // Windows 2008 Server
              return " Server";
            }
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
                      osName = OSProductType == VER_NT_SERVER ? "Windows Server 2003" : "Windows XP x64";
                      break;
                  }
                  break;
                }
              case 6:
                {
                  switch (OSMinorVersion)
                  {
                    case 0:
                      osName = OSProductType == VER_NT_SERVER ? "Windows 2008" : "Windows Vista";
                      break;
                    case 1:
                      osName = "Windows7";
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
    /// Returns the name of the operating system running on this computer.
    /// </summary>
    /// <returns>A string containing the the operating system name.</returns>
    public static OSList GetOSName()
    {
      int osVer = (OSMajorVersion * 10) + OSMinorVersion;

      switch (osVer)
      {
        case 51:
          return OSList.WindowsXp;
        case 52:
          return OSProductType == VER_NT_SERVER ? OSList.Windows2003 : OSList.WindowsXp64;
        case 60:
          return OSProductType == VER_NT_SERVER ? OSList.Windows2008 : OSList.WindowsVista;
        case 61:
          return OSList.Windows7;
      }
      return OSList.Windows2000andPrevious;
    }

    /// <summary>
    /// Return a value that indicate if the OS is blocked, supported, or officially unsupported
    /// </summary>
    /// <returns>0 to block installation/usage</returns>
    /// <returns>1 if fully supported</returns>
    /// <returns>2 if not officially supported</returns>
    public static int GetOSSupported()
    {
      switch (GetOSName())
      {
        case OSList.WindowsXp:
          if (OSServicePackMajor < 2)
          {
            return 0;
          }
          return OSServicePackMinor == 0 ? 1 : 2;
        case OSList.WindowsVista:
          if (OSServicePackMajor < 1)
          {
            return 0;
          }
          return OSServicePackMinor == 0 ? 1 : 2;
        case OSList.Windows7:
          return OSBuildVersion >= 7301 ? 1 : 2;
        case OSList.Windows2003:
        case OSList.Windows2008:
          return 2;
        default:
          return 0;
      }
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the full version of the operating system running on this computer.
    /// </summary>
    public static string OSVersion
    {
      get
      {
        return Environment.OSVersion.Version.ToString();
      }
    }

    /// <summary>
    /// Gets the major version of the operating system running on this computer.
    /// </summary>
    public static int OSMajorVersion
    {
      get
      {
        return Environment.OSVersion.Version.Major;
      }
    }

    /// <summary>
    /// Gets the minor version of the operating system running on this computer.
    /// </summary>
    public static int OSMinorVersion
    {
      get
      {
        return Environment.OSVersion.Version.Minor;
      }
    }

    /// <summary>
    /// Gets the build version of the operating system running on this computer.
    /// </summary>
    public static int OSBuildVersion
    {
      get
      {
        return Environment.OSVersion.Version.Build;
      }
    }

    /// <summary>
    /// Gets the revision version of the operating system running on this computer.
    /// </summary>
    public static int OSRevisionVersion
    {
      get
      {
        return Environment.OSVersion.Version.Revision;
      }
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
        if (!GetVersionEx(ref osVersionInfo)) return -1;
        return osVersionInfo.wServicePackMinor;
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
  }
}
