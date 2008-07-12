 ==============================================================

 GetVersion.dll v0.9 (5kB) by Afrow UK

  Last build: 12th October 2007

  C++ NSIS plugin that gets Windows & IE version information.

 --------------------------------------------------------------

 Place GetVersion.dll in your NSIS\Plugins folder or
 simply extract all files in the Zip to NSIS\

 See Examples\GetVersion\Example.nsi for examples of use.

 ==============================================================
 The Functions:

  GetVersion::WindowsName
   Pop $R0

   Gets name of Windows. This includes:
    Vista
    Server Longhorn
    Server 2003
    Server 2003 R2
    XP
    XP x64
    2000
    CE
    NT
    ME
    98
    98 SE
    95
    95 OSR2
    Win32s

  ---------------------------

  GetVersion::WindowsType
   Pop $R0

   Gets type of Windows OS.
   For Windows NT:
    Workstation 4.0
   For Windows XP:
    Home Edition
    Professional
    Professional x64 Edition
    Media Center Edition
    Tablet PC Edition
    (or empty string)
   For Windows Vista:
    Ultimate Edition
    Home Premium Edition
    Home Basic Edition
    Enterprise Edition
    Business Edition
    Starter Edition
    (or empty string)

  ---------------------------

  GetVersion::WindowsVersion
   Pop $R0

   Gets the Windows version x.x (e.g. 5.1).

  ---------------------------

  GetVersion::WindowsServerName
   Pop $R0

   Gets the installed server name. This includes:
    Server
    Server 4.0
    Server 4.0 Enterprise Edition
    Workstation
    Storage Server 2003
    Server 2003
    Server 2008
    Cluster Server Edition
    Datacenter Edition
    Datacenter Edition for Itanium-based Systems
    Datacenter x64 Edition
    Enterprise Edition
    Enterprise Edition for Itanium-based Systems
    Enterprise x64 Edition
    Advanced Server
    Small Business Server
    Small Business Server Premium Edition
    Standard Edition
    Web Server Edition
    (or empty string)

  ---------------------------

  GetVersion::WindowsServicePack
   Pop $R0

   Gets the installed service pack name (e.g. Service Pack 2).

  ---------------------------

  GetVersion::WindowsServicePackBuild
   Pop $R0

   Gets the installed service pack build number (e.g. 2600).

  ---------------------------

  GetVersion::WindowsPlatformId
   Pop $R0

   Gets the platform Id of the installed Windows
   (e.g. 1, 2, 3).

  ---------------------------

  GetVersion::WindowsPlatformArchitecture
   Pop $R0

   Gets the architecture of the installed Windows
   (e.g. 32, 64).

 ==============================================================
 Change Log:

  v0.9
   * Major code clean up.
   * All functions now return an empty string if GetVersionEx API call fails.
   * Added Windows types and server names for Vista.

  v0.8
   * Fixed WindowsType.
   * Removed function to get IE version.

  v0.7
   * WindowsName now returns simple names (not Windows #).

  v0.6
   * Added support for Windows CE.

  v0.5
   * Added support for Windows XP Media Center Edition (in
     WindowsType).
   * Added support for Windows XP Tablet PC Edition (in
     WindowsType).

  v0.4
   * Added WindowsPlatformId.
   * Added WindowsPlatformArchitecture.

  v0.3
   * Added support for Windows Vista and Longhorn Server.

  v0.2
   * Added support for Windows x64.
   * No support added for Windows Vista as yet (waiting for
     Microsoft to update their page for it!)

  v0.1
   * First version.