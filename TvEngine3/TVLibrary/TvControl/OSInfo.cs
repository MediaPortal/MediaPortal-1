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
    #endregion

    #region Operating System enum
    /// <summary>
    /// List of all operating systems
    /// </summary>
    public enum OSList
    {
      ///<summary>
      /// Windows2000 and previous
      ///</summary>
      Windows2000andPrevious,
      ///<summary>
      /// Clients
      ///</summary>
      WindowsXp,
      WindowsXp64,
      WindowsVista,
      Windows7,
      ///<summary>
      /// Servers
      ///</summary>
      Windows2003,
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
      OperatingSystem osInfo = Environment.OSVersion;

      osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFOEX));

      if (!GetVersionEx(ref osVersionInfo))
      {
        return string.Empty;
      }

      switch (osInfo.Version.Major)
      {
        case 4:
          if (osVersionInfo.wProductType == VER_NT_WORKSTATION)
          {
            // Windows NT 4.0 Workstation
            return " Workstation";
          }
          if (osVersionInfo.wProductType == VER_NT_SERVER)
          {
            // Windows NT 4.0 Server
            return " Server";
          }
          return string.Empty;
        case 5:
          if (osVersionInfo.wProductType == VER_NT_WORKSTATION)
          {
            if ((osVersionInfo.wSuiteMask & VER_SUITE_PERSONAL) == VER_SUITE_PERSONAL)
            {
              // Windows XP Home Edition
              return " Home Edition";
            }
            // Windows XP / Windows 2000 Professional
            return " Professional";
          }
          if (osVersionInfo.wProductType == VER_NT_SERVER)
          {
            if (osInfo.Version.Minor == 0)
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
            if (osInfo.Version.Minor == 2)
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
      OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
      OperatingSystem osInfo = Environment.OSVersion;
      string osName = "UNKNOWN";

      switch (osInfo.Platform)
      {
        case PlatformID.Win32Windows:
          {
            switch (osInfo.Version.Minor)
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
            switch (osInfo.Version.Major)
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
                  switch (osInfo.Version.Minor)
                  {
                    case 0:
                      osName = "Windows 2000";
                      break;
                    case 1:
                      osName = "Windows XP";
                      break;
                    case 2:
                      osName = osVersionInfo.wProductType == VER_NT_WORKSTATION ? "Windows Server 2003" : "Windows XP x64";
                      break;
                  }
                  break;
                }
              case 6:
                {
                  switch (osInfo.Version.Minor)
                  {
                    case 0:
                      osName = osVersionInfo.wProductType == VER_NT_WORKSTATION ? "Windows Vista" : "Windows 2008";
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
      OSVERSIONINFOEX osVersionInfo = new OSVERSIONINFOEX();
      int osVer = (osVersionInfo.dwMajorVersion * 10) + osVersionInfo.dwMinorVersion;

      switch (osVer)
      {
        case 51:
          return OSList.WindowsXp;
        case 52:
          return osVersionInfo.wProductType == VER_NT_SERVER ? OSList.Windows2003 : OSList.WindowsXp;
        case 60:
          return osVersionInfo.wProductType == VER_NT_SERVER ? OSList.Windows2008 : OSList.WindowsVista;
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
          if (OSMajorVersion < 2)
          {
            return 0;
          }
          return OSServicePackMinor == 0 ? 1 : 2;
        case OSList.WindowsVista:
          if (OSMajorVersion < 1)
          {
            return 0;
          }
          return OSServicePackMinor == 0 ? 1 : 2;
        case OSList.Windows2003:
        case OSList.Windows2008:
        case OSList.Windows7:
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
        return Environment.OSVersion.VersionString;
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
        return osVersionInfo.wServicePackMinor;
      }
    }
    #endregion
  }
}
