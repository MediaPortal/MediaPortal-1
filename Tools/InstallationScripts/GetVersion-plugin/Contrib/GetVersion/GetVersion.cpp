#include <windows.h>
#include "..\ExDll\exdll.h"
#include "shlwapi.h"
#include <stdio.h>

/**
 GetVersion.cpp - Windows version info plugin for NSIS by Afrow UK
 Based on example script by Microsoft at:
  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/getting_the_system_version.asp
*/

#define ERROR_STRING TEXT("GetVersion error")
#define SM_SERVERR2 89
typedef void (WINAPI *PGNSI)(LPSYSTEM_INFO);
typedef BOOL (WINAPI *PGPI)(DWORD, DWORD, DWORD, DWORD, PDWORD);

HINSTANCE g_hInstance;

#ifndef VER_PLATFORM_WIN32_CE
#define VER_PLATFORM_WIN32_CE 3
#endif

#ifndef VER_SUITE_STORAGE_SERVER
#define VER_SUITE_STORAGE_SERVER 0x00002000
#endif
#ifndef VER_SUITE_COMPUTE_SERVER
#define VER_SUITE_COMPUTE_SERVER 0x00004000
#endif

#define PRODUCT_UNDEFINED                       0x00000000

#define PRODUCT_ULTIMATE                        0x00000001
#define PRODUCT_HOME_BASIC                      0x00000002
#define PRODUCT_HOME_PREMIUM                    0x00000003
#define PRODUCT_ENTERPRISE                      0x00000004
#define PRODUCT_HOME_BASIC_N                    0x00000005
#define PRODUCT_BUSINESS                        0x00000006
#define PRODUCT_STANDARD_SERVER                 0x00000007
#define PRODUCT_DATACENTER_SERVER               0x00000008
#define PRODUCT_SMALLBUSINESS_SERVER            0x00000009
#define PRODUCT_ENTERPRISE_SERVER               0x0000000A
#define PRODUCT_STARTER                         0x0000000B
#define PRODUCT_DATACENTER_SERVER_CORE          0x0000000C
#define PRODUCT_STANDARD_SERVER_CORE            0x0000000D
#define PRODUCT_ENTERPRISE_SERVER_CORE          0x0000000E
#define PRODUCT_ENTERPRISE_SERVER_IA64          0x0000000F
#define PRODUCT_BUSINESS_N                      0x00000010
#define PRODUCT_WEB_SERVER                      0x00000011
#define PRODUCT_CLUSTER_SERVER                  0x00000012
#define PRODUCT_HOME_SERVER                     0x00000013
#define PRODUCT_STORAGE_EXPRESS_SERVER          0x00000014
#define PRODUCT_STORAGE_STANDARD_SERVER         0x00000015
#define PRODUCT_STORAGE_WORKGROUP_SERVER        0x00000016
#define PRODUCT_STORAGE_ENTERPRISE_SERVER       0x00000017
#define PRODUCT_SERVER_FOR_SMALLBUSINESS        0x00000018
#define PRODUCT_SMALLBUSINESS_SERVER_PREMIUM    0x00000019

#define PRODUCT_UNLICENSED                      0xABCDABCD

BOOL callGetVersion(LPOSVERSIONINFOEXA posvi, BOOL *pbOsVersionInfoEx)
{
  // Try calling GetVersionEx using the OSVERSIONINFOEX structure.
  // If that fails, try using the OSVERSIONINFO structure.

  posvi->dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);

  if (!(*pbOsVersionInfoEx = GetVersionEx((LPOSVERSIONINFOA)posvi)))
  {
    posvi->dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
    if (!GetVersionEx((LPOSVERSIONINFOA)posvi))
    {
      pushstring(ERROR_STRING);
      return FALSE;
    }
  }
  return TRUE;
}

extern "C"
void __declspec(dllexport) WindowsName(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    char *szOsName = NULL;
    PGNSI pGNSI;
    SYSTEM_INFO si;
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    switch (osvi.dwPlatformId)
    {
      // Test for the Windows NT product family.
      case VER_PLATFORM_WIN32_NT:

        // Test for the specific product.
        if (osvi.dwMajorVersion == 6 && osvi.dwMinorVersion == 0)
        {
          if (osvi.wProductType == VER_NT_WORKSTATION)
              szOsName = "Vista";
          else szOsName = "Server Longhorn";
        }
        else if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 2)
        {

          // Use GetProcAddress to avoid load issues on Windows 2000
          pGNSI = (PGNSI)GetProcAddress(GetModuleHandle("kernel32.dll"), "GetNativeSystemInfo");

          if (NULL != pGNSI)
            pGNSI(&si);

          if (GetSystemMetrics(SM_SERVERR2))
            szOsName = "Server 2003 R2";
          else if (osvi.wProductType == VER_NT_WORKSTATION && si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
            szOsName = "XP x64";
          else szOsName = "Server 2003";

        }
        else if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 1)
          szOsName = "XP";

        else if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 0)
          szOsName = "2000";

        else if (osvi.dwMajorVersion <= 4)
          szOsName = "NT";

      break;

      // Test for Windows CE.
      case VER_PLATFORM_WIN32_CE:
        szOsName = "CE";
      break;
      
      // Test for the Windows Me/98/95.
      case VER_PLATFORM_WIN32_WINDOWS:

      if (osvi.dwMajorVersion == 4 && osvi.dwMinorVersion == 0)
      {
        szOsName = "95";
        if (osvi.szCSDVersion[1]=='C' || osvi.szCSDVersion[1]=='B')
          lstrcat(szOsName, " OSR2");
      } 

      if (osvi.dwMajorVersion == 4 && osvi.dwMinorVersion == 10)
      {
        szOsName = "98";
        if (osvi.szCSDVersion[1] == 'A')
          lstrcat(szOsName, " SE");
      } 

      if (osvi.dwMajorVersion == 4 && osvi.dwMinorVersion == 90)
      {
        szOsName = "ME";
      } 
      break;

      case VER_PLATFORM_WIN32s:

      szOsName = "Win32s";
      break;
    }
    pushstring(szOsName);
  }
}

extern "C"
void __declspec(dllexport) WindowsType(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    char *szOsType = NULL;
    PGNSI pGNSI;
    SYSTEM_INFO si;
    PGPI pGPI;
    DWORD dwType;
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    //ZeroMemory(&si, sizeof(SYSTEM_INFO));

    pGNSI = (PGNSI)GetProcAddress(GetModuleHandle(TEXT("kernel32.dll")), "GetNativeSystemInfo");
    if (pGNSI)
      pGNSI(&si);
    else GetSystemInfo(&si);

    if (osvi.dwPlatformId == VER_PLATFORM_WIN32_NT)
    {
      if (osvi.dwMajorVersion == 6 && osvi.dwMinorVersion == 0)
      {
        pGPI = (PGPI)GetProcAddress(GetModuleHandle(TEXT("kernel32.dll")), "GetProductInfo");
        pGPI(6, 0, 0, 0, &dwType);

        switch(dwType)
        {
          case PRODUCT_ULTIMATE:
            szOsType = "Ultimate Edition";
            break;
          case PRODUCT_HOME_PREMIUM:
            szOsType = "Home Premium Edition";
            break;
          case PRODUCT_HOME_BASIC:
            szOsType = "Home Basic Edition";
            break;
          case PRODUCT_ENTERPRISE:
            szOsType = "Enterprise Edition";
            break;
          case PRODUCT_BUSINESS:
            szOsType = "Business Edition";
            break;
          case PRODUCT_STARTER:
            szOsType = "Starter Edition";
            break;
        }
      }
      else if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 1)
      {
        HKEY hKeyMCE, hKeyTPCE;
        DWORD dwBufLen;
        LONG lRetMCE, lRetTPCE;
        DWORD dwInstMCE = 0, dwInstTPCE = 0;

        lRetMCE  = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SYSTEM\\WPA\\MediaCenter", 0, KEY_QUERY_VALUE, &hKeyMCE);
        lRetTPCE = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SYSTEM\\WPA\\TabletPC"   , 0, KEY_QUERY_VALUE, &hKeyTPCE);

        if (lRetMCE != ERROR_SUCCESS)
        {
          dwBufLen = sizeof(DWORD);
          lRetMCE = RegQueryValueEx(hKeyMCE, "Installed", NULL, NULL, (LPBYTE)dwInstMCE, &dwBufLen);
        }
        if (lRetTPCE != ERROR_SUCCESS)
        {
          dwBufLen = sizeof(DWORD);
          lRetTPCE = RegQueryValueEx(hKeyTPCE, "Installed", NULL, NULL, (LPBYTE)dwInstTPCE, &dwBufLen);
        }

        if (lRetMCE == ERROR_SUCCESS)
          RegCloseKey(hKeyMCE);
        if (lRetTPCE == ERROR_SUCCESS)
          RegCloseKey(hKeyTPCE);

        if (dwInstMCE == 0 && dwInstTPCE == 0)
        {
          if (osvi.wSuiteMask & VER_SUITE_PERSONAL)
            szOsType = "Home Edition";
          else if (osvi.wSuiteMask & VER_SUITE_EMBEDDEDNT)
            szOsType = "Embedded";
          else if (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
            szOsType = "Professional x64 Edition";
          else
            szOsType = "Professional";
        }
        else if (dwInstMCE == 1)
          szOsType = "Media Center Edition";
        else if (dwInstTPCE == 1)
          szOsType = "Tablet PC Edition";
        else
          szOsType = "";
      }
      else if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 0)
        szOsType = "Professional";
      else if (osvi.dwMajorVersion == 4)
        szOsType = "Workstation 4.0";
    }

/*    switch (osvi.dwPlatformId)
    {
      // Test for the Windows NT product family.
      case VER_PLATFORM_WIN32_NT:

        // Use GetProcAddress to avoid load issues on Windows 2000
        pGNSI = (PGNSI)GetProcAddress(GetModuleHandle("kernel32.dll"), "GetNativeSystemInfo");

        if (NULL != pGNSI)
          pGNSI(&si);

        if (osvi.wProductType == VER_NT_WORKSTATION)
        {
          if (osvi.dwMajorVersion == 4)
            szOsType = "Workstation 4.0";
          else if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 1)
          {
            HKEY hKeyMCE, hKeyTPCE;
            DWORD dwBufLen;
            LONG lRetMCE, lRetTPCE;
            DWORD dwInstMCE = 0, dwInstTPCE = 0;

            lRetMCE  = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SYSTEM\\WPA\\MediaCenter", 0, KEY_QUERY_VALUE, &hKeyMCE);
            lRetTPCE = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SYSTEM\\WPA\\TabletPC"   , 0, KEY_QUERY_VALUE, &hKeyTPCE);

            if (lRetMCE != ERROR_SUCCESS)
            {
              dwBufLen = sizeof(DWORD);
              lRetMCE = RegQueryValueEx(hKeyMCE, "Installed", NULL, NULL, (LPBYTE)dwInstMCE, &dwBufLen);
            }
            if (lRetTPCE != ERROR_SUCCESS)
            {
              dwBufLen = sizeof(DWORD);
              lRetTPCE = RegQueryValueEx(hKeyTPCE, "Installed", NULL, NULL, (LPBYTE)dwInstTPCE, &dwBufLen);
            }

            if (lRetMCE == ERROR_SUCCESS)
              RegCloseKey(hKeyMCE);
            if (lRetTPCE == ERROR_SUCCESS)
              RegCloseKey(hKeyTPCE);

            if (dwInstMCE == 0 && dwInstTPCE == 0)
            {
              if (osvi.wSuiteMask & VER_SUITE_PERSONAL)
                szOsType = "Home Edition";
              else if (osvi.wSuiteMask & VER_SUITE_EMBEDDEDNT)
                szOsType = "Embedded";
              else if (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
                szOsType = "Professional x64 Edition";
              else
                szOsType = "Professional";
            }
            else if (dwInstMCE == 1)
              szOsType = "Media Center Edition";
            else if (dwInstTPCE == 1)
              szOsType = "Tablet PC Edition";
            else
              szOsType = "";
          }
          else if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion == 0)
            szOsType = "Professional";
          else if (osvi.dwMajorVersion == 6)
          {
            if (osvi.wSuiteMask & VER_SUITE_PERSONAL)
              szOsType = "Home Premium/Basic";
          }
          else szOsType = "";
        }
        else szOsType = "";

      break;
    }*/
    pushstring(szOsType);

  }
}

extern "C"
void __declspec(dllexport) WindowsVersion(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    char szOsVer[16];
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    wsprintf(szOsVer, "%i.%i", osvi.dwMajorVersion, osvi.dwMinorVersion);
    pushstring(szOsVer);
  }
}

extern "C"
void __declspec(dllexport) WindowsPlatformId(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    char szOsPId[16];
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    wsprintf(szOsPId, "%i", osvi.dwPlatformId);
    pushstring(szOsPId);
  }
}

extern "C"
void __declspec(dllexport) WindowsPlatformArchitecture(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    char *szOsPlatArch = NULL;
    PGNSI pGNSI;
    SYSTEM_INFO si;
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    switch (osvi.dwPlatformId)
    {
      // Test for the Windows NT product family.
      case VER_PLATFORM_WIN32_NT:

        // Use GetProcAddress to avoid load issues on Windows 2000
        pGNSI = (PGNSI)GetProcAddress(GetModuleHandle("kernel32.dll"), "GetNativeSystemInfo");

        if (NULL != pGNSI)
        {
          pGNSI(&si);

          if (si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 || si.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
            szOsPlatArch = "64";
          else
            szOsPlatArch = "32";
        }
        else
          szOsPlatArch = "32";

        pushstring(szOsPlatArch);
    }
  }
}

extern "C"
void __declspec(dllexport) WindowsServerName(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    char *szOsServName = NULL;
    PGNSI pGNSI;
    SYSTEM_INFO si;
    PGPI pGPI;
    DWORD dwType;
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    switch (osvi.dwPlatformId)
    {
      // Test for the Windows NT product family.
      case VER_PLATFORM_WIN32_NT:

      // Test for specific product on Windows NT 4.0 SP6 and later.
      if (bOsVersionInfoEx)
      {
        // Test for the workstation type.
        if (osvi.wProductType == VER_NT_WORKSTATION)
        { 
          // Test for the server type.
          if (osvi.wProductType == VER_NT_SERVER || 
              osvi.wProductType == VER_NT_DOMAIN_CONTROLLER)
          {
            if (osvi.dwMajorVersion==5 && osvi.dwMinorVersion==2)
            {
              // Use GetProcAddress to avoid load issues on Windows 2000
              pGNSI = (PGNSI)GetProcAddress(GetModuleHandle("kernel32.dll"), "GetNativeSystemInfo");
              if (pGNSI)
                pGNSI(&si);

              if (si.wProcessorArchitecture==PROCESSOR_ARCHITECTURE_IA64)
              {
                if (osvi.wSuiteMask & VER_SUITE_DATACENTER)
                  szOsServName = "Datacenter Edition for Itanium-based Systems";
                else if (osvi.wSuiteMask & VER_SUITE_ENTERPRISE)
                  szOsServName = "Enterprise Edition for Itanium-based Systems";
              }

              else if (si.wProcessorArchitecture==PROCESSOR_ARCHITECTURE_AMD64)
              {
                if (osvi.wSuiteMask & VER_SUITE_DATACENTER)
                  szOsServName = "Datacenter x64 Edition";
                else if (osvi.wSuiteMask & VER_SUITE_ENTERPRISE)
                  szOsServName = "Enterprise x64 Edition";
                else szOsServName = "Standard x64 Edition";
              }

              else
              {
                if (osvi.wSuiteMask & VER_SUITE_DATACENTER)
                  szOsServName = "Datacenter Edition";
                else if (osvi.wSuiteMask & VER_SUITE_ENTERPRISE)
                  szOsServName = "Enterprise Edition";
                else if (osvi.wSuiteMask == VER_SUITE_BLADE)
                  szOsServName = "Enterprise Edition";
                else if (osvi.wSuiteMask == VER_SUITE_STORAGE_SERVER)
                  szOsServName = "Storage Server 2003";
                else if (osvi.wSuiteMask == VER_SUITE_COMPUTE_SERVER)
                  szOsServName = "Server 2003";
                else if (osvi.wSuiteMask == VER_SUITE_SMALLBUSINESS)
                  szOsServName = "Small Business Server";
                else szOsServName = "Standard Edition";
              }
            }
            else if (osvi.dwMajorVersion==5 && osvi.dwMinorVersion==0)
            {
              if (osvi.wSuiteMask & VER_SUITE_DATACENTER)
                szOsServName = "Datacenter Server";
              else if (osvi.wSuiteMask & VER_SUITE_ENTERPRISE)
                szOsServName = "Advanced Server";
              else szOsServName = "Server";
            }
            else if (osvi.dwMajorVersion==6 && osvi.dwMinorVersion==0)
            {
              if (!osvi.wSuiteMask & VER_NT_WORKSTATION)
                szOsServName = "Server 2008";
              else
              {
                pGPI = (PGPI)GetProcAddress(GetModuleHandle(TEXT("kernel32.dll")), "GetProductInfo");
                pGPI(6, 0, 0, 0, &dwType);

                switch(dwType)
                {
                  case PRODUCT_DATACENTER_SERVER:
                  case PRODUCT_DATACENTER_SERVER_CORE:
                    szOsServName = "Datacenter Edition";
                    break;
                  case PRODUCT_ENTERPRISE_SERVER:
                  case PRODUCT_ENTERPRISE_SERVER_CORE:
                    szOsServName = "Enterprise Edition";
                    break;
                  case PRODUCT_STANDARD_SERVER:
                  case PRODUCT_STANDARD_SERVER_CORE:
                    szOsServName = "Standard Edition";
                    break;
                  case PRODUCT_ENTERPRISE_SERVER_IA64:
                    szOsServName = "Enterprise Edition for Itanium-based Systems";
                    break;
                  case PRODUCT_SMALLBUSINESS_SERVER:
                    szOsServName = "Small Business Server";
                    break;
                  case PRODUCT_SMALLBUSINESS_SERVER_PREMIUM:
                    szOsServName = "Small Business Server Premium Edition";
                    break;
                  case PRODUCT_CLUSTER_SERVER:
                    szOsServName = "Cluster Server Edition";
                    break;
                  case PRODUCT_WEB_SERVER:
                    szOsServName = "Web Server Edition";
                    break;
                }
              }
            }
            else  // Windows NT 4.0 
            {
              if (osvi.wSuiteMask & VER_SUITE_ENTERPRISE)
                szOsServName = "Server 4.0 Enterprise Edition";
              else szOsServName = "Server 4.0";
            }
          }
        }

      // Test for specific product on Windows NT 4.0 SP5 and earlier
      }
      else  
      {
        HKEY hKey;
        char szProductType[128];
        DWORD dwBufLen;
        LONG lRet;

        lRet = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\ProductOptions", 0, KEY_QUERY_VALUE, &hKey);
        if (lRet != ERROR_SUCCESS)
          szOsServName = ERROR_STRING;
        else
        {
          dwBufLen = sizeof(szProductType);
          lRet = RegQueryValueEx(hKey, "ProductType", NULL, NULL, (LPBYTE)szProductType, &dwBufLen);
          if (lRet != ERROR_SUCCESS)
            szOsServName = ERROR_STRING;
          else
          {
            RegCloseKey(hKey);

            if (lstrcmpi("WINNT", szProductType) == 0)
              szOsServName = "Workstation";
            else if (lstrcmpi("LANMANNT", szProductType) == 0)
              szOsServName = "Server";
            else if (lstrcmpi("SERVERNT", szProductType) == 0)
              szOsServName = "Advanced Server";
          }
        }
      }
    }
    pushstring(szOsServName);
  }
}

extern "C"
void __declspec(dllexport) WindowsServicePack(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    char *szOsServPack = NULL;
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    switch (osvi.dwPlatformId)
    {
      // Test for the Windows NT product family.
      case VER_PLATFORM_WIN32_NT:

      // Display service pack (if any) and build number.
      if (osvi.dwMajorVersion == 4 && 
          lstrcmpi(osvi.szCSDVersion, "Service Pack 6") == 0)
      { 
        HKEY hKey;
        LONG lRet;

        // Test for SP6 versus SP6a.
        lRet = RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Hotfix\\Q246009", 0, KEY_QUERY_VALUE, &hKey);
        if (lRet == ERROR_SUCCESS)
          szOsServPack = "Service Pack 6a";
        else // Windows NT 4.0 prior to SP6a
          szOsServPack = osvi.szCSDVersion;

        RegCloseKey(hKey);
      }
      else // not Windows NT 4.0
      {
        szOsServPack = osvi.szCSDVersion;
      }
      break;
    }
    pushstring(szOsServPack);
  }
}

extern "C"
void __declspec(dllexport) WindowsServicePackBuild(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    OSVERSIONINFOEX osvi;
    BOOL bOsVersionInfoEx;

    if (!callGetVersion(&osvi, &bOsVersionInfoEx))
    {
      pushstring("");
      return;
    }

    char szServPackBuild[16];
    wsprintf(szServPackBuild, "%i", osvi.dwBuildNumber & 0xFFFF);
    pushstring(szServPackBuild);
  }
}

/*extern "C"
void __declspec(dllexport) IEVersion(HWND hWndParent, int string_size, 
                                      char *variables, stack_t **stacktop,
                                      extra_parameters *extra)
{
  EXDLL_INIT();
  {
    HINSTANCE hBrowser;

    //Load the DLL.
    hBrowser = LoadLibrary(TEXT("shdocvw.dll"));

    if (hBrowser) 
    {
      HRESULT  hr = S_OK;
      DLLGETVERSIONPROC pDllGetVersion;

      pDllGetVersion =
        (DLLGETVERSIONPROC)GetProcAddress(hBrowser,
          TEXT("DllGetVersion"));

      if (pDllGetVersion) 
      {
        char buf[32];

        DLLVERSIONINFO dvi;

        dvi.cbSize = sizeof(dvi);
        hr = (*pDllGetVersion)(&dvi);

        if (SUCCEEDED(hr)) 
        {
          wsprintf(buf, "%i", dvi.dwBuildNumber);
          pushstring(buf);
          wsprintf(buf, "%i.%i", dvi.dwMajorVersion, dvi.dwMinorVersion);
          pushstring(buf);
        }

      } 
      else
        //If GetProcAddress failed, there is a problem 
        // with the DLL.
        hr = E_FAIL;

      FreeLibrary(hBrowser);
    }
  }
}*/

BOOL WINAPI DllMain(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
  g_hInstance = (HINSTANCE)hInst;
  return TRUE;
}