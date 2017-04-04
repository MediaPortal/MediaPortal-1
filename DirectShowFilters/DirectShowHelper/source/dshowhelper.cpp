// Copyright (C) 2005-2012 Team MediaPortal
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

#pragma warning(disable: 4995 4996)

#include "StdAfx.h"
#include <streams.h>
#include <atlbase.h>
#include <shlobj.h>
#include <d3dx9.h>
#include <vmr9.h>
#include <evr.h>
#include <sbe.h>
#include <dvdmedia.h>
#include <map>

#include "dshowhelper.h"
#include "evrcustomPresenter.h"
#include "dx9allocatorpresenter.h"
#include "madPresenter.h"

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

using namespace std;

HMODULE m_hModuleDXVA2    = NULL;
HMODULE m_hModuleEVR      = NULL;
HMODULE m_hModuleMFPLAT   = NULL;
HMODULE m_hModuleDWMAPI   = NULL;
HMODULE m_hModuleAVRT     = NULL;
HMODULE m_hModuleW7Helper = NULL;

TDXVA2CreateDirect3DDeviceManager9* m_pDXVA2CreateDirect3DDeviceManager9 = NULL;
TMFCreateVideoSampleFromSurface*    m_pMFCreateVideoSampleFromSurface    = NULL;
TMFCreateVideoMediaType*            m_pMFCreateVideoMediaType            = NULL;
TMFCreateMediaType*                 m_pMFCreateMediaType                 = NULL;

// Vista / Windows 7 only
TDwmEnableMMCSS*                    m_pDwmEnableMMCSS   = NULL;
TDwmFlush*                          m_pDwmFlush                = NULL;
TDwmSetPresentParameters*           m_pDwmSetPresentParameters = NULL;
TDwmIsCompositionEnabled*           m_pDwmIsCompositionEnabled = NULL;
TDwmSetDxFrameDuration*             m_pDwmSetDxFrameDuration   = NULL;
TDwmGetCompositionTimingInfo*       m_pDwmGetCompositionTimingInfo   = NULL;
 
TW7GetRefreshRate*                  m_pW7GetRefreshRate = NULL;

TAvSetMmThreadCharacteristicsW*     m_pAvSetMmThreadCharacteristicsW = NULL;
TAvSetMmThreadPriority*             m_pAvSetMmThreadPriority = NULL;
TAvRevertMmThreadCharacteristics*   m_pAvRevertMmThreadCharacteristics = NULL;



BOOL m_bEVRLoaded    = false;
TCHAR* m_RenderPrefix = _T("vmr9");

LPDIRECT3DDEVICE9         m_pDevice       = NULL;
CVMR9AllocatorPresenter*  m_vmr9Presenter = NULL;
MPEVRCustomPresenter*     m_evrPresenter  = NULL;
MPMadPresenter*           m_madPresenter  = NULL;
IBaseFilter*              m_pVMR9Filter   = NULL;
IVMRSurfaceAllocator9*    m_allocator     = NULL;
LONG                      m_iRecordingId  = 0;
int                       m_pRefCount = 0;

map<int,IStreamBufferRecordControl*> m_mapRecordControl;
typedef map<int,IStreamBufferRecordControl*>::iterator imapRecordControl;

#define MY_USER_ID  0x6ABE51
#define MY_USER_ID2 0x6ABE52

#define IsInterlaced(x)  ((x) & AMINTERLACE_IsInterlaced)
#define IsSingleField(x) ((x) & AMINTERLACE_1FieldPerSample)
#define IsField1First(x) ((x) & AMINTERLACE_Field1First)

#define INIT_GUID(name, l, w1, w2, b1, b2, b3, b4, b5, b6, b7, b8) \
  const GUID name \
  = { l, w1, w2, { b1, b2,  b3,  b4,  b5,  b6,  b7,  b8 } }

//#define DEFAULT_PRESENTER

INIT_GUID(bobDxvaGuid,  0x335aa36e, 0x7884, 0x43a4, 0x9c, 0x91, 0x7f, 0x87, 0xfa, 0xf3, 0xe3, 0x7e);
INIT_GUID(clsidTeeSink, 0x0A4252A0, 0x7E70, 0x11D0, 0xA5, 0xD6, 0x28, 0xDB, 0x04, 0xC1, 0x00, 0x00);


HRESULT __fastcall UnicodeToAnsi(LPCOLESTR pszW, LPSTR* ppszA)
{
  ULONG cbAnsi;
  ULONG cCharacters;
  DWORD dwError;

  // If input is null then just return the same.
  if (pszW == NULL)
  {
    *ppszA = NULL;
    return NOERROR;
  }

  cCharacters = (ULONG)wcslen(pszW)+1;
  // Determine number of bytes to be allocated for ANSI string. An
  // ANSI string can have at most 2 bytes per character (for Double
  // Byte Character Strings.)
  cbAnsi = cCharacters*2;

  // Use of the OLE allocator is not required because the resultant
  // ANSI  string will never be passed to another COM component. You
  // can use your own allocator.
  *ppszA = (LPSTR) CoTaskMemAlloc(cbAnsi);
  if (NULL == *ppszA)
    return E_OUTOFMEMORY;

  // Convert to ANSI.
  if (0 == WideCharToMultiByte(CP_ACP, 0, pszW, cCharacters, *ppszA,
    cbAnsi, NULL, NULL))
  {
    dwError = GetLastError();
    CoTaskMemFree(*ppszA);
    *ppszA = NULL;
    return HRESULT_FROM_WIN32(dwError);
  }
  return NOERROR;
}

//-------------------- Async logging methods -------------------------------------------------

WORD logFileParsed = -1;
WORD logFileDate = -1;
MPEVRCustomPresenter* instanceID = 0;

CCritSec m_qLock;
CCritSec m_logFileLock;
std::queue<std::string> m_logQueue;
BOOL m_bLoggerRunning;
HANDLE m_hLogger = NULL;
CAMEvent m_EndLoggingEvent;



LONG LogWriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{  
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_SZ, (LPBYTE)data, _tcslen(data) * sizeof(TCHAR));
  
  return result;
}

LONG LogReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{
  DWORD dwSize = MAX_PATH * sizeof(TCHAR);
  DWORD dwType = REG_SZ;
  LONG result = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)data, &dwSize);
  
  if (result != ERROR_SUCCESS)
  {
    if (result == ERROR_FILE_NOT_FOUND)
    {
      //create default value
      result = LogWriteRegistryKeyString(hKey, lpSubKey, data);
    }
  }
  
  return result;
}

void LogPath(TCHAR* dest, TCHAR* name)
{
  CAutoLock lock(&m_logFileLock); 
  HKEY hKey;
  //Try to read logging folder path from registry
  LONG result = RegCreateKeyEx(HKEY_CURRENT_USER, _T("Software\\Team MediaPortal\\Client Common"), 0, NULL, 
                                    REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, NULL);                                   
  if (result == ERROR_SUCCESS)
  {
    //Get default log folder path
    TCHAR folder[MAX_PATH];
    SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
    TCHAR logFolder[MAX_PATH];
    _stprintf(logFolder, _T("%s\\Team Mediaportal\\MediaPortal\\log"), folder);

    //Read log folder path from registry (or write default path into registry if key doesn't exist)
    LPCTSTR logFolderC = logFolder;    
    LPCTSTR logFolderPath = _T("LogFolderPath");
    result = LogReadRegistryKeyString(hKey, logFolderPath, logFolderC);
    
    if (result == ERROR_SUCCESS)
    {
      //Get full log file path
      _stprintf(dest, _T("%s\\%s.%s"), logFolderC, m_RenderPrefix, name);
    }
  }
    
  if (result != ERROR_SUCCESS)
  {
    //Fall back to default log folder path
    TCHAR folder[MAX_PATH];
    SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
    //Get full log file path
    _stprintf(dest, _T("%s\\Team Mediaportal\\MediaPortal\\log\\%s.%s"), folder, m_RenderPrefix, name);
  }
}


void LogRotate()
{   
  CAutoLock lock(&m_logFileLock);
    
  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));
    
  try
  {
    // Get the last file write date
    WIN32_FILE_ATTRIBUTE_DATA fileInformation; 
    if (GetFileAttributesEx(fileName, GetFileExInfoStandard, &fileInformation))
    {  
      // Convert the write time to local time.
      SYSTEMTIME stUTC, fileTime;
      if (FileTimeToSystemTime(&fileInformation.ftLastWriteTime, &stUTC))
      {
        if (SystemTimeToTzSpecificLocalTime(NULL, &stUTC, &fileTime))
        {
          logFileDate = fileTime.wDay;
        
          SYSTEMTIME systemTime;
          GetLocalTime(&systemTime);
          
          if(fileTime.wDay == systemTime.wDay)
          {
            //file date is today - no rotation needed
            return;
          }
        } 
      }   
    }
  }  
  catch (...) {}
  
  TCHAR bakFileName[MAX_PATH];
  LogPath(bakFileName, _T("bak"));
  _tremove(bakFileName);
  _trename(fileName, bakFileName);
}



string GetLogLine()
{
  CAutoLock lock(&m_qLock);
  if ( m_logQueue.size() == 0 )
  {
    return "";
  }
  string ret = m_logQueue.front();
  m_logQueue.pop();
  return ret;
}


UINT CALLBACK LogThread(void* param)
{
  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));
  while ( m_bLoggerRunning || (m_logQueue.size() > 0) ) 
  {
    if ( m_logQueue.size() > 0 ) 
    {
      SYSTEMTIME systemTime;
      GetLocalTime(&systemTime);
      if(logFileParsed != systemTime.wDay)
      {
        LogRotate();
        logFileParsed=systemTime.wDay;
        LogPath(fileName, _T("log"));
      }

      CAutoLock lock(&m_logFileLock);
      FILE* fp = _tfopen(fileName, _T("a+"));
      if (fp!=NULL)
      {
        SYSTEMTIME systemTime;
        GetLocalTime(&systemTime);
        string line = GetLogLine();
        while (!line.empty())
        {
          fprintf(fp, "%s", line.c_str());
          line = GetLogLine();
        }
        fclose(fp);
      }
      else //discard data
      {
        string line = GetLogLine();
        while (!line.empty())
        {
          line = GetLogLine();
        }
      }
    }
    if (m_bLoggerRunning)
    {
      m_EndLoggingEvent.Wait(1000); //Sleep for 1000ms, unless thread is ending
    }
    else
    {
      Sleep(1);
    }
  }
  return 0;
}


void StartLogger()
{
  UINT id;
  m_hLogger = (HANDLE)_beginthreadex(NULL, 0, LogThread, 0, 0, &id);
  SetThreadPriority(m_hLogger, THREAD_PRIORITY_BELOW_NORMAL);
}


void StopLogger()
{
  if (m_hLogger)
  {
    m_bLoggerRunning = FALSE;
    m_EndLoggingEvent.Set();
    WaitForSingleObject(m_hLogger, INFINITE);	
    m_EndLoggingEvent.Reset();
    m_hLogger = NULL;
    logFileParsed = -1;
    logFileDate = -1;
    instanceID = 0;
  }
}


void Log(const char *fmt, ...) 
{
  static CCritSec lock;
  va_list ap;
  va_start(ap,fmt);

  CAutoLock logLock(&lock);
  if (!m_hLogger) {
    m_bLoggerRunning = true;
    StartLogger();
  }
  char buffer[1000]; 
  int tmp;
  va_start(ap,fmt);
  tmp = vsprintf(buffer, fmt, ap);
  va_end(ap); 

  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  char msg[5000];
  sprintf_s(msg, 5000,"[%04.4d-%02.2d-%02.2d %02.2d:%02.2d:%02.2d,%03.3d] [%8x] [%4x] - %s\n",
    systemTime.wYear, systemTime.wMonth, systemTime.wDay,
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond, systemTime.wMilliseconds, 
    instanceID,
    GetCurrentThreadId(),
    buffer);
  CAutoLock l(&m_qLock);
  if (m_logQueue.size() < 2000) 
  {
    m_logQueue.push((string)msg);
  }
};


VMR9_SampleFormat ConvertInterlaceFlags(DWORD dwInterlaceFlags)
{
  if (IsInterlaced(dwInterlaceFlags)) {
    if (IsSingleField(dwInterlaceFlags)) {
      if (IsField1First(dwInterlaceFlags)) {
        return VMR9_SampleFieldSingleEven;
      }
      else {
        return VMR9_SampleFieldSingleOdd;
      }
    }
    else {
      if (IsField1First(dwInterlaceFlags)) {
        return VMR9_SampleFieldInterleavedEvenFirst;
      }
      else {
        return VMR9_SampleFieldInterleavedOddFirst;
      }
    }
  }
  else {
    return VMR9_SampleProgressiveFrame;  // Not interlaced.
  }
}


BOOL Vmr9Init(IVMR9Callback* callback, DWORD dwD3DDevice, IBaseFilter* vmr9Filter,DWORD monitor)
{
  HRESULT hr;
  m_pDevice = (LPDIRECT3DDEVICE9)(dwD3DDevice);
  m_pVMR9Filter=vmr9Filter;

  Log("Vmr9Init");

  CComQIPtr<IVMRFilterConfig9> pConfig = m_pVMR9Filter;
  if(!pConfig)
  {
    return FALSE;
  }

  hr = pConfig->SetRenderingMode(VMR9Mode_Renderless);
  if(FAILED(hr))
  {
    Log("Vmr9:Init() SetRenderingMode() failed 0x:%x", hr);
    return FALSE;
  }

  hr = pConfig->SetNumberOfStreams(4);
  if(FAILED(hr))
  {
    Log("Vmr9:Init() SetNumberOfStreams() failed 0x:%x", hr);
    return FALSE;
  }

  CComQIPtr<IVMRSurfaceAllocatorNotify9> pSAN = m_pVMR9Filter;
  if(!pSAN)
  {
    return FALSE;
  }

  m_vmr9Presenter = new CVMR9AllocatorPresenter(m_pDevice, callback,(HMONITOR)monitor) ;
  m_vmr9Presenter->QueryInterface(IID_IVMRSurfaceAllocator9,(void**)&m_allocator);

  hr =  pSAN->AdviseSurfaceAllocator(MY_USER_ID, m_allocator);
  if(FAILED(hr))
  {
    Log("Vmr9:Init() AdviseSurfaceAllocator() failed 0x:%x",hr);
    return FALSE;
  }

  hr = m_allocator->AdviseNotify(pSAN);
  if (FAILED(hr))
  {
    Log("Vmr9:Init() AdviseNotify() failed 0x:%x",hr);
    return FALSE;
  }
  return TRUE;
}


void Vmr9Deinit()
{
  Log("Vmr9Deinit enter");
  try
  {
    int hr;
    if (m_allocator!=NULL)
    {
      m_allocator->Release();
      m_allocator=NULL;
    }
    if (m_vmr9Presenter!=NULL)
    {
      hr=m_vmr9Presenter->Release();
      m_vmr9Presenter = NULL;
    }
    m_pVMR9Filter   = NULL;
    m_pDevice       = NULL;
  }
  catch(...)
  {
    Log("Vmr9Deinit:exception");
  }
  Log("Vmr9Deinit exit");
}

// http://msdn.microsoft.com/en-us/library/ms725491(VS.85).aspx
BOOL IsWin7()
{
   OSVERSIONINFOEX osvi;
   DWORDLONG dwlConditionMask = 0;
   int op = VER_GREATER_EQUAL;

   // Initialize the OSVERSIONINFOEX structure.
   ZeroMemory(&osvi, sizeof(OSVERSIONINFOEX));
   osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFOEX);
   osvi.dwMajorVersion = 6;
   osvi.dwMinorVersion = 1;
   osvi.wServicePackMajor = 0;
   osvi.wServicePackMinor = 0;

   // Initialize the condition mask.
   VER_SET_CONDITION(dwlConditionMask, VER_MAJORVERSION, op);
   VER_SET_CONDITION(dwlConditionMask, VER_MINORVERSION, op);
   VER_SET_CONDITION(dwlConditionMask, VER_SERVICEPACKMAJOR, op);
   VER_SET_CONDITION(dwlConditionMask, VER_SERVICEPACKMINOR, op);

   // Perform the test.
   return VerifyVersionInfo(
     &osvi,
     VER_MAJORVERSION | VER_MINORVERSION | VER_SERVICEPACKMAJOR | VER_SERVICEPACKMINOR,
     dwlConditionMask);
}

void UnloadEVR()
{
  Log("Unloading EVR libraries");
  if (m_hModuleDXVA2 != NULL)
  {
    Log("Freeing library DXVA2.dll");
    if (!FreeLibrary(m_hModuleDXVA2))
    {
      Log("DXVA2.dll could not be unloaded!");
    }
    m_hModuleDXVA2 = NULL;
  }
  if (m_hModuleEVR != NULL)
  {
    Log("Freeing lib: EVR.dll");
    if (!FreeLibrary(m_hModuleEVR))
    {
      Log("EVR.dll could not be unloaded");
    }
    m_hModuleEVR = NULL;
  }
  if (m_hModuleMFPLAT != NULL)
  {
    Log("Freeing lib: MFPLAT.dll");
    if (!FreeLibrary(m_hModuleMFPLAT))
    {
      Log("MFPLAT.dll could not be unloaded");
    }
    m_hModuleMFPLAT = NULL;
  }
  if (m_hModuleDWMAPI != NULL)
  {
    Log("Freeing lib: DWMAPI.dll");
    if (!FreeLibrary(m_hModuleDWMAPI))
    {
      Log("DWMAPI.dll could not be unloaded");
    }
    m_hModuleDWMAPI = NULL;
  }
  if (m_hModuleAVRT != NULL)
  {
    Log("Freeing lib: avrt.dll");
    if (!FreeLibrary(m_hModuleAVRT))
    {
      Log("avrt.dll could not be unloaded");
    }
    m_hModuleAVRT = NULL;
  }
  if (m_hModuleW7Helper != NULL)
  {
    Log("Freeing lib: Win7RefreshRateHelper.dll");
    if (!FreeLibrary(m_hModuleW7Helper))
    {
      Log("Win7RefreshRateHelper.dll could not be unloaded");
    }
    m_hModuleW7Helper = NULL;
  }
}


bool LoadEVR()
{
  Log("============================================================");
  Log("Loading EVR libraries");
  TCHAR systemFolder[MAX_PATH];
  TCHAR DLLFileName[MAX_PATH];
  GetSystemDirectory(systemFolder,sizeof(systemFolder));
  _stprintf(DLLFileName, _T("%s\\dxva2.dll"), systemFolder);
  m_hModuleDXVA2=LoadLibrary(DLLFileName);
  if (m_hModuleDXVA2 != NULL)
  {
    Log("Found dxva2.dll");
    m_pDXVA2CreateDirect3DDeviceManager9 = (TDXVA2CreateDirect3DDeviceManager9*)GetProcAddress(m_hModuleDXVA2,"DXVA2CreateDirect3DDeviceManager9");
    if (m_pDXVA2CreateDirect3DDeviceManager9 != NULL)
    {
      Log("Found method DXVA2CreateDirect3DDeviceManager9");
      _stprintf(DLLFileName, _T("%s\\evr.dll"), systemFolder);
      m_hModuleEVR = LoadLibrary(DLLFileName);
      m_pMFCreateVideoSampleFromSurface = (TMFCreateVideoSampleFromSurface*)GetProcAddress(m_hModuleEVR,"MFCreateVideoSampleFromSurface");

      if (m_pMFCreateVideoSampleFromSurface)
      {
        Log("Found method MFCreateVideoSampleFromSurface");
        m_pMFCreateVideoMediaType = (TMFCreateVideoMediaType*)GetProcAddress(m_hModuleEVR,"MFCreateVideoMediaType");
        if(m_pMFCreateVideoMediaType)
        {
          Log("Found method MFCreateVideoMediaType");
          _stprintf(DLLFileName, _T("%s\\mfplat.dll"), systemFolder);
          m_hModuleMFPLAT = LoadLibrary(DLLFileName);
          m_pMFCreateMediaType = (TMFCreateMediaType*)GetProcAddress(m_hModuleMFPLAT,"MFCreateMediaType");
          if (m_pMFCreateMediaType)
          {
            Log("Found method MFCreateMediaType");
            Log("Successfully loaded EVR dlls");
            
            _stprintf(DLLFileName, _T("%s\\dwmapi.dll"), systemFolder);
            m_hModuleDWMAPI = LoadLibrary(DLLFileName);
            // Vista / Windows 7 only, allowed to return NULL. Remember to check agains NULL when using
            if (m_hModuleDWMAPI)
            {
              Log("Successfully loaded DWM dll");
              m_pDwmEnableMMCSS              = (TDwmEnableMMCSS*)GetProcAddress(m_hModuleDWMAPI,"DwmEnableMMCSS");
              m_pDwmFlush                    = (TDwmFlush*)GetProcAddress(m_hModuleDWMAPI,"DwmFlush");
              m_pDwmSetPresentParameters     = (TDwmSetPresentParameters*)GetProcAddress(m_hModuleDWMAPI,"DwmSetPresentParameters");
              m_pDwmIsCompositionEnabled     = (TDwmIsCompositionEnabled*)GetProcAddress(m_hModuleDWMAPI,"DwmIsCompositionEnabled");
              m_pDwmSetDxFrameDuration       = (TDwmSetDxFrameDuration*)GetProcAddress(m_hModuleDWMAPI,"DwmSetDxFrameDuration");
              m_pDwmGetCompositionTimingInfo = (TDwmGetCompositionTimingInfo*)GetProcAddress(m_hModuleDWMAPI,"DwmGetCompositionTimingInfo");
            }


            _stprintf(DLLFileName, _T("%s\\avrt.dll"), systemFolder);
            m_hModuleAVRT = LoadLibrary(DLLFileName);
            // Vista / Windows 7 only, allowed to return NULL. Remember to check agains NULL when using
            if (m_hModuleAVRT)
            {
              Log("Successfully loaded AVRT dll");
              m_pAvSetMmThreadCharacteristicsW   = (TAvSetMmThreadCharacteristicsW*)GetProcAddress(m_hModuleAVRT,"AvSetMmThreadCharacteristicsW");
              m_pAvSetMmThreadPriority           = (TAvSetMmThreadPriority*)GetProcAddress(m_hModuleAVRT,"AvSetMmThreadPriority");
              m_pAvRevertMmThreadCharacteristics = (TAvRevertMmThreadCharacteristics*)GetProcAddress(m_hModuleAVRT,"AvRevertMmThreadCharacteristics");
            }



            if (IsWin7())
            {
              _stprintf(DLLFileName, _T("Win7RefreshRateHelper.dll"));
              m_hModuleW7Helper = LoadLibrary(DLLFileName);
              if (m_hModuleW7Helper)
              {
                Log("Successfully loaded Win7RefreshRateHelper.dll");
                m_pW7GetRefreshRate = (TW7GetRefreshRate*)GetProcAddress(m_hModuleW7Helper,"W7GetRefreshRate");
                if (m_pW7GetRefreshRate)
                {
                  Log("  W7GetRefreshRate() found");
                }
                else
                {
                  Log("  W7GetRefreshRate() not found");
                }
              }
            }
            return TRUE;
          }
        }
      }
    }
  }
  Log("Could not find all dependencies for EVR!");
  UnloadEVR();
  return FALSE;
}


void DsDumpGraph(IFilterGraph* vmr9Filter)
{
  return;
  IEnumFilters* pEnum;
  vmr9Filter->EnumFilters(&pEnum);
  Log("---------------------DUMPGRAPH----------------");
  //	HRESULT hr;
  IBaseFilter* pFilter;
  ULONG cFetched;
  do {
    pEnum->Next(1, &pFilter, &cFetched);
    if (cFetched > 0 )
    {
      FILTER_INFO info;
      pFilter->QueryFilterInfo(&info);
      LPSTR astr;
      UnicodeToAnsi(info.achName, &astr);
      Log("Found filter %p: %s", pFilter, astr);
      CoTaskMemFree(astr);
      IEnumPins* pEnumPins;
      pFilter->EnumPins(&pEnumPins);
      IPin* pPins, *pConnectedPin;
      ULONG pinsFetched;
      do 
      {
        pEnumPins->Next(1, &pPins, &pinsFetched);
        if (pinsFetched > 0)
        {
          PIN_INFO pinInfo;
          pPins->QueryPinInfo(&pinInfo);
          UnicodeToAnsi(pinInfo.achName, &astr);
          Log("Found pin: %s", astr);
          CoTaskMemFree(astr);

          if (SUCCEEDED(pPins->ConnectedTo(&pConnectedPin))) 
          {
            pConnectedPin->QueryPinInfo(&pinInfo);
            UnicodeToAnsi(pinInfo.achName, &astr);
            Log("Connected to pin: %s", astr);
            CoTaskMemFree(astr);
            pinInfo.pFilter->QueryFilterInfo(&info);
            UnicodeToAnsi(info.achName, &astr);
            Log("\tFrom Filter: %p: %s", pinInfo.pFilter, astr);
            CoTaskMemFree(astr);
          } 
          else 
          {
            Log("Could not get connected pin!");
          }
        }
      } 
      while (pinsFetched > 0);
    }
  } 
  while (cFetched > 0);

  pEnum->Release();
  Log("---------------------/DUMPGRAPH----------------");
  //DumpGraph(vmr9Filter, 0);
}


//avoid dependency into MFGetService, aparently only availabe on vista
HRESULT MyGetService(IUnknown* punkObject, REFGUID guidService, REFIID riid, LPVOID* ppvObject)
{
  if (ppvObject == NULL)
  {
    return E_POINTER;
  }
  HRESULT hr;
  IMFGetService* pGetService;
  hr = punkObject->QueryInterface(__uuidof(IMFGetService), (void**)&pGetService);
  if (SUCCEEDED(hr)) 
  {
    hr = pGetService->GetService(guidService, riid, ppvObject);
    SAFE_RELEASE(pGetService);
  }
  return hr;
}


BOOL EvrInit(IVMR9Callback* callback, DWORD dwD3DDevice, IBaseFilter** evrFilter, DWORD monitor, int monitorIdx, bool disVsyncCorr, bool disMparCorr)
{
  HRESULT hr;
  m_RenderPrefix = _T("evr");
  // Make sure that we aren't trying to load the DLLs for second time
  if (!m_bEVRLoaded)
  {
    m_bEVRLoaded = LoadEVR();

    if (!m_bEVRLoaded) 
    {
      Log("EVR libraries are not loaded. Cannot init EVR");
      return FALSE;
    }
  }

  m_pDevice = (LPDIRECT3DDEVICE9)(dwD3DDevice);

#ifndef DEFAULT_PRESENTER

  m_evrPresenter = new MPEVRCustomPresenter(callback, m_pDevice, (HMONITOR)monitor, evrFilter, IsWin7(), monitorIdx, disVsyncCorr, disMparCorr);
  m_pVMR9Filter = (*evrFilter);

  CComQIPtr<IMFVideoRenderer> pRenderer = m_pVMR9Filter;
  if (!pRenderer) 
  {
    Log("Could not get IMFVideoRenderer");
    return FALSE;
  }

  hr = pRenderer->InitializeRenderer(NULL, m_evrPresenter);
  
  if (FAILED(hr))
  {
    Log("InitializeRenderer failed: 0x%x", hr);
    return FALSE;
  }
  pRenderer.Release();

#else
  CComPtr<IMFVideoDisplayControl> videoControl;
  MFGetService(evrFilter, MR_VIDEO_RENDER_SERVICE, IID_IMFVideoDisplayControl, (LPVOID*)&videoControl);
  videoControl->SetVideoWindow((HWND)monitor);
#endif

  CComQIPtr<IEVRFilterConfig> pConfig = m_pVMR9Filter;
  if (!pConfig) 
  {
    Log("Could not get IEVRFilterConfig  interface" );
    return FALSE;
  }

  hr = pConfig->SetNumberOfStreams(3);
  if (FAILED(hr))
  {
    Log("EVR:Init() SetNumberOfStreams() failed 0x:%x",hr);
    return FALSE;
  }
  pConfig.Release();

  return TRUE;
}


void EvrDeinit()
{
  try
  {
    if (m_evrPresenter)
    {
      m_evrPresenter->ReleaseCallback();
    }
    m_evrPresenter = NULL;
    m_pVMR9Filter = NULL;
    
    // Do not unload DLLs when playback stops
    // 1) it generates randomly a access violation inside EVR.DLL
    // 2) it gives a small performance boost since most of the time we are going to play a 2nd video as well
    /*
    if (m_bEVRLoaded)
    {
      UnloadEVR();
      m_bEVRLoaded = FALSE;
    }
    */
  }
  catch(...)
  {
    Log("EvrDeinit:exception");
  }
  //StopLogger();
}


void EVRNotifyRateChange(double pRate)
{
  if (m_evrPresenter)
  {
    m_evrPresenter->NotifyRateChange(pRate);
  }
}


void EVRNotifyDVDMenuState(bool pIsInMenu)
{
  if (m_evrPresenter)
  {
    m_evrPresenter->NotifyDVDMenuState(pIsInMenu);
  }
}


void EVRDrawStats(bool enable)
{
  m_evrPresenter->EnableDrawStats(enable);
}


void EVRResetStatCounters(bool enable)
{
  m_evrPresenter->ResetEVRStatCounters();
}

// Get video FPS - returns video FPS from a source selected by 'fpsSource'
double EVRGetVideoFPS(int fpsSource)
{
  double videoFPS = -1.0;
  
  if (m_evrPresenter)
  {
    videoFPS = m_evrPresenter->GetVideoFramePeriod((FPS_SOURCE_METHOD)fpsSource); // frame period in seconds
    
    if (videoFPS > 0.0)
      videoFPS = 1.0 / videoFPS; // Convert valid frame period into FPS
  }
  return videoFPS;
}

void EVRUpdateDisplayFPS()
{
  m_evrPresenter->UpdateDisplayFPS();
}

// Get display FPS - returns display FPS
double EVRGetDisplayFPS()
{
  double displayFPS = -1.0;
  
  if (m_evrPresenter)
  {
    displayFPS = m_evrPresenter->GetDisplayCycle(); // display frame period in milliseconds
    
    if (displayFPS > 0.0)
      displayFPS = 1000.0 / displayFPS; // Convert period into FPS
  }
  return displayFPS;
}

BOOL MadInit(IVMR9Callback* callback, DWORD width, DWORD height, DWORD dwD3DDevice, OAHWND parent, IBaseFilter** madFilter, IMediaControl* pMediaControl)
{
  m_RenderPrefix = _T("mad");

  m_pDevice = reinterpret_cast<LPDIRECT3DDEVICE9>(dwD3DDevice);

  Log("MPMadDshow::MadInit");

  m_madPresenter = new MPMadPresenter(callback, width, height, parent, m_pDevice, pMediaControl);

  Com::SmartPtr<IUnknown> pRenderer;
  m_madPresenter->CreateRenderer(&pRenderer);
  m_pVMR9Filter = m_madPresenter->Initialize();
  m_pVMR9Filter = Com::SmartQIPtr<IBaseFilter>(pRenderer).Detach();
  
  // madVR supports calling IVideoWindow::put_Owner before the pins are connected
  //if (Com::SmartQIPtr<IVideoWindow> pVW = pCAP)
  //    pVW->put_Owner((OAHWND)CDSPlayer::GetDShWnd());

  *madFilter = m_pVMR9Filter;

  if (!madFilter)
    return FALSE;

  return TRUE;
}

void MadDeinit()
{
  try
  {
    Log("MPMadDshow::MadDeinit shutdown start");
    //CAutoLock lock(&m_madPresenter->m_dsLock);
    //m_madPresenter->m_dsLock.Lock();
    m_madPresenter->m_pShutdown = true;
    Sleep(100);
    m_madPresenter->Shutdown();
    m_pVMR9Filter = nullptr;
    //m_madPresenter->m_dsLock.Unlock();
    Log("MPMadDshow::MadDeinit shutdown done");
  }
  catch(...)
  {
  }
}

void MadStopping()
{
  try
  {
    Log("MPMadDshow::MadStopping start");
    //CAutoLock lock(&m_madPresenter->m_dsLock);
    //m_madPresenter->m_dsLock.Lock();
    m_madPresenter->m_pShutdown = true;
    Sleep(100);
    m_madPresenter->Stopping();
    //m_madPresenter->m_dsLock.Unlock();
    Log("MPMadDshow::MadStopping done");
  }
  catch (...)
  {
  }
}

void MadVrPaused(bool paused)
{
  m_madPresenter->SetMadVrPaused(paused);
}

void MadVrRepeatFrameSend()
{
  m_madPresenter->RepeatFrame();
}

void MadVr3DRight(uint16_t x, uint16_t y, DWORD width, DWORD height)
{
  m_madPresenter->MadVr3DSizeRight(x, y, width, height);
}

void MadVr3DLeft(uint16_t x, uint16_t y, DWORD width, DWORD height)
{
  m_madPresenter->MadVr3DSizeLeft(x, y, width, height);
}

void MadVrScreenResizeForce(uint16_t x, uint16_t y, DWORD width, DWORD height, BOOL displayChange)
{
  m_madPresenter->MadVrScreenResize(x, y, width, height, displayChange);
}

void MadVr3DEnable(bool Enable)
{
  m_madPresenter->MadVr3D(Enable);
}

void Vmr9SetDeinterlaceMode(int mode)
{
  //0=None
  //1=Bob
  //2=Weave
  //3=Best
  Log("vmr9:SetDeinterlace() SetDeinterlaceMode(%d)",mode);
  CComQIPtr<IVMRDeinterlaceControl9> pDeint = m_pVMR9Filter;
  VMR9VideoDesc VideoDesc; 
  DWORD dwNumModes = 0;
  GUID deintMode;
  int hr; 
  if (mode == 0)
  {
    //off
    hr = pDeint->SetDeinterlaceMode(0xFFFFFFFF,(LPGUID)&GUID_NULL);
    if (FAILED(hr))
    {
      Log("vmr9:SetDeinterlace() failed hr:0x%x", hr);
    }
    hr = pDeint->GetDeinterlaceMode(0,&deintMode);
    if (!FAILED(hr))
    {
      Log("vmr9:GetDeinterlaceMode() failed hr:0x%x", hr);
    }
    Log("vmr9:SetDeinterlace() deinterlace mode OFF: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
      deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

    return ;
  }
  if (mode == 1)
  {
    //BOB
    hr = pDeint->SetDeinterlaceMode(0xFFFFFFFF,(LPGUID)&bobDxvaGuid);
    if (FAILED(hr))
    {
      Log("vmr9:SetDeinterlace() failed hr:0x%x", hr);
    }
    hr = pDeint->GetDeinterlaceMode(0,&deintMode);
    if (FAILED(hr))
    {
      Log("vmr9:GetDeinterlaceMode() failed hr:0x%x", hr);
    }
    Log("vmr9:SetDeinterlace() deinterlace mode BOB: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
      deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

    return ;
  }
  if (mode == 2)
  {
    //WEAVE
    hr = pDeint->SetDeinterlaceMode(0xFFFFFFFF,(LPGUID)&GUID_NULL);
    if (FAILED(hr))
    {
      Log("vmr9:SetDeinterlace() failed hr:0x%x", hr);
    }
    hr = pDeint->GetDeinterlaceMode(0,&deintMode);
    if (!FAILED(hr))
    {
      Log("vmr9:GetDeinterlaceMode() failed hr:0x%x", hr);
    }
    Log("vmr9:SetDeinterlace() deinterlace mode WEAVE: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
      deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1], deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5], deintMode.Data4[6], deintMode.Data4[7]);

    return ;
  }

  AM_MEDIA_TYPE pmt;
  ULONG fetched;
  IPin* pins[10];
  CComPtr<IEnumPins> pinEnum;
  hr=m_pVMR9Filter->EnumPins(&pinEnum);
  pinEnum->Reset();
  pinEnum->Next(1,&pins[0],&fetched);
  hr=pins[0]->ConnectionMediaType(&pmt);
  pins[0]->Release();

  VIDEOINFOHEADER2* vidInfo2 =(VIDEOINFOHEADER2*)pmt.pbFormat;
  if (vidInfo2 == NULL)
  {
    Log("vmr9:SetDeinterlace() VMR9 not connected");
    return ;
  }
  if ((pmt.formattype != FORMAT_VideoInfo2) || (pmt.cbFormat< sizeof(VIDEOINFOHEADER2)))
  {
    Log("vmr9:SetDeinterlace() not using VIDEOINFOHEADER2");

    return ;
  }

  Log("vmr9:SetDeinterlace() resolution:%dx%d planes:%d bitcount:%d fmt:%d %c%c%c%c",
    vidInfo2->bmiHeader.biWidth,vidInfo2->bmiHeader.biHeight,
    vidInfo2->bmiHeader.biPlanes,
    vidInfo2->bmiHeader.biBitCount,
    vidInfo2->bmiHeader.biCompression,
    (char)(vidInfo2->bmiHeader.biCompression&0xff),
    (char)((vidInfo2->bmiHeader.biCompression>>8)&0xff),
    (char)((vidInfo2->bmiHeader.biCompression>>16)&0xff),
    (char)((vidInfo2->bmiHeader.biCompression>>24)&0xff)
    );
  char major[128];
  char subtype[128];
  strcpy(major,"unknown");
  sprintf(subtype, "unknown (0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x)",
    pmt.subtype.Data1,pmt.subtype.Data2,pmt.subtype.Data3, pmt.subtype.Data4[0],
    pmt.subtype.Data4[1], pmt.subtype.Data4[2], pmt.subtype.Data4[3], pmt.subtype.Data4[4],
    pmt.subtype.Data4[5],  pmt.subtype.Data4[6], pmt.subtype.Data4[7]);

  if (pmt.majortype == MEDIATYPE_AnalogVideo)
  {
    strcpy(major, "Analog video");
  }
  if (pmt.majortype == MEDIATYPE_Video)
  {
    strcpy(major, "video");
  }
  if (pmt.majortype == MEDIATYPE_Stream)
  {
    strcpy(major, "stream");
  }

  if (pmt.subtype == MEDIASUBTYPE_MPEG2_VIDEO)
  {
    strcpy(subtype, "mpeg2 video");
  }
  if (pmt.subtype == MEDIASUBTYPE_MPEG1System)
  {
    strcpy(subtype, "mpeg1 system");
  }
  if (pmt.subtype == MEDIASUBTYPE_MPEG1VideoCD)
  {
    strcpy(subtype, "mpeg1 videocd");
  }

  if (pmt.subtype == MEDIASUBTYPE_MPEG1Packet)
  {
    strcpy(subtype, "mpeg1 packet");
  }
  if (pmt.subtype == MEDIASUBTYPE_MPEG1Payload )
  {
    strcpy(subtype, "mpeg1 payload");
  }
  if (pmt.subtype == MEDIASUBTYPE_ATSC_SI)
  {
    strcpy(subtype, "ATSC SI");
  }
  if (pmt.subtype == MEDIASUBTYPE_DVB_SI)
  {
    strcpy(subtype, "DVB SI");
  }
  if (pmt.subtype == MEDIASUBTYPE_MPEG2DATA)
  {
    strcpy(subtype, "MPEG2 Data");
  }
  if (pmt.subtype == MEDIASUBTYPE_MPEG2_TRANSPORT)
  {
    strcpy(subtype, "MPEG2 Transport");
  }
  if (pmt.subtype == MEDIASUBTYPE_MPEG2_PROGRAM)
  {
    strcpy(subtype, "MPEG2 Program");
  }

  if (pmt.subtype == MEDIASUBTYPE_CLPL)
  {
    strcpy(subtype, "MEDIASUBTYPE_CLPL");
  }
  if (pmt.subtype == MEDIASUBTYPE_YUYV)
  {
    strcpy(subtype, "MEDIASUBTYPE_YUYV");
  }
  if (pmt.subtype == MEDIASUBTYPE_IYUV)
  {
    strcpy(subtype, "MEDIASUBTYPE_IYUV");
  }
  if (pmt.subtype == MEDIASUBTYPE_YVU9)
  {
    strcpy(subtype, "MEDIASUBTYPE_YVU9");
  }
  if (pmt.subtype == MEDIASUBTYPE_Y411)
  {
    strcpy(subtype, "MEDIASUBTYPE_Y411");
  }
  if (pmt.subtype == MEDIASUBTYPE_Y41P)
  {
    strcpy(subtype, "MEDIASUBTYPE_Y41P");
  }
  if (pmt.subtype == MEDIASUBTYPE_YUY2)
  {
    strcpy(subtype, "MEDIASUBTYPE_YUY2");
  }
  if (pmt.subtype == MEDIASUBTYPE_YVYU)
  {
    strcpy(subtype, "MEDIASUBTYPE_YVYU");
  }
  if (pmt.subtype == MEDIASUBTYPE_UYVY)
  {
    strcpy(subtype, "MEDIASUBTYPE_UYVY");
  }
  if (pmt.subtype == MEDIASUBTYPE_Y211)
  {
    strcpy(subtype, "MEDIASUBTYPE_Y211");
  }
  if (pmt.subtype == MEDIASUBTYPE_RGB565)
  {
    strcpy(subtype, "MEDIASUBTYPE_RGB565");
  }
  if (pmt.subtype == MEDIASUBTYPE_RGB32)
  {
    strcpy(subtype, "MEDIASUBTYPE_RGB32");
  }
  if (pmt.subtype == MEDIASUBTYPE_ARGB32)
  {
    strcpy(subtype, "MEDIASUBTYPE_ARGB32");
  }
  if (pmt.subtype == MEDIASUBTYPE_RGB555)
  {
    strcpy(subtype, "MEDIASUBTYPE_RGB555");
  }
  if (pmt.subtype == MEDIASUBTYPE_RGB24)
  {
    strcpy(subtype, "MEDIASUBTYPE_RGB24");
  }
  if (pmt.subtype == MEDIASUBTYPE_AYUV)
  {
    strcpy(subtype, "MEDIASUBTYPE_AYUV");
  }
  if (pmt.subtype == MEDIASUBTYPE_YV12)
  {
    strcpy(subtype, "MEDIASUBTYPE_YV12");
  }
  if (pmt.subtype == MEDIASUBTYPE_NV12)
  {
    strcpy(subtype, "MEDIASUBTYPE_NV12");
  }

  Log("vmr9:SetDeinterlace() major:%s subtype:%s", major,subtype);
  VideoDesc.dwSize = sizeof(VMR9VideoDesc);
  VideoDesc.dwFourCC = vidInfo2->bmiHeader.biCompression;
  VideoDesc.dwSampleWidth = vidInfo2->bmiHeader.biWidth;
  VideoDesc.dwSampleHeight = vidInfo2->bmiHeader.biHeight;
  VideoDesc.SampleFormat = ConvertInterlaceFlags(vidInfo2->dwInterlaceFlags);
  VideoDesc.InputSampleFreq.dwDenominator = (DWORD)vidInfo2->AvgTimePerFrame;
  VideoDesc.InputSampleFreq.dwNumerator = 10000000;
  VideoDesc.OutputFrameFreq.dwDenominator = (DWORD)vidInfo2->AvgTimePerFrame;
  VideoDesc.OutputFrameFreq.dwNumerator = VideoDesc.InputSampleFreq.dwNumerator;
  if (VideoDesc.SampleFormat != VMR9_SampleProgressiveFrame)
  {
    VideoDesc.OutputFrameFreq.dwNumerator=2*VideoDesc.InputSampleFreq.dwNumerator;
  }

  // Fill in the VideoDesc structure (not shown).
  hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, NULL);
  if (SUCCEEDED(hr) && (dwNumModes != 0))
  {
    // Allocate an array for the GUIDs that identify the modes.
    GUID *pModes = new GUID[dwNumModes];
    if (pModes)
    {
      Log("vmr9:SetDeinterlace() found %d deinterlacing modes", dwNumModes);
      // Fill the array.
      hr = pDeint->GetNumberOfDeinterlaceModes(&VideoDesc, &dwNumModes, pModes);
      if (SUCCEEDED(hr))
      {
        // Loop through each item and get the capabilities.
        for (DWORD i = 0; i < dwNumModes; i++)
        {
          hr = pDeint->SetDeinterlaceMode(0xFFFFFFFF,&pModes[0]);
          if (SUCCEEDED(hr)) 
          {
            Log("vmr9:SetDeinterlace() set deinterlace mode:%d",i);
            pDeint->GetDeinterlaceMode(0,&deintMode);
            if (deintMode.Data1 == pModes[0].Data1 && deintMode.Data2 == pModes[0].Data2 &&
              deintMode.Data3 == pModes[0].Data3 && deintMode.Data4 == pModes[0].Data4)
            {
              Log("vmr9:SetDeinterlace() succeeded");
            }
            else
              Log("vmr9:SetDeinterlace() deinterlace mode set to: 0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x-0x%x",
                deintMode.Data1,deintMode.Data2,deintMode.Data3, deintMode.Data4[0], deintMode.Data4[1],
                deintMode.Data4[2], deintMode.Data4[3], deintMode.Data4[4], deintMode.Data4[5],
                deintMode.Data4[6], deintMode.Data4[7]);
            break;
          }
          else
            Log("vmr9:SetDeinterlace() deinterlace mode:%d failed 0x:%x", i, hr);
        }
      }
      delete [] pModes;
    }
  }
}


void Vmr9SetDeinterlacePrefs(DWORD dwMethod)
{
  HRESULT hr;
  CComQIPtr<IVMRDeinterlaceControl9> pDeint=m_pVMR9Filter;
  switch (dwMethod)
  {
  case 1:
    Log("vmr9:SetDeinterlace() preference to BOB");
    hr = pDeint->SetDeinterlacePrefs(DeinterlacePref9_BOB);
    break;
  case 2:
    Log("vmr9:SetDeinterlace() preference to Weave");
    hr = pDeint->SetDeinterlacePrefs(DeinterlacePref9_Weave);
    break;
  case 3:
    Log("vmr9:SetDeinterlace() preference to NextBest");
    hr = pDeint->SetDeinterlacePrefs(DeinterlacePref9_NextBest );
    break;
  }
}


bool DvrMsCreate(LONG *id, IBaseFilter* streamBufferSink, LPCWSTR strPath, DWORD dwRecordingType)
{
  *id=-1;
  try
  {
    Log("CStreamBufferRecorder::Create() Record");
    int hr;
    IUnknown* pRecUnk;
    CComQIPtr<IStreamBufferSink> sink=streamBufferSink;
    if (!sink)
    {
      Log("cannot get IStreamBufferSink:%x");
      return false;
    }
    hr=sink->CreateRecorder( strPath,dwRecordingType,&pRecUnk);
    if (FAILED(hr))
    {
      Log("CStreamBufferRecorder::Create() create recorder failed:%x", hr);
      return false;
    }

    if (pRecUnk == NULL)
    {
      Log("CStreamBufferRecorder::Create() cannot get recorder failed");
      return false;
    }
    IStreamBufferRecordControl* pRecord;
    pRecUnk->QueryInterface(__uuidof(IStreamBufferRecordControl),(void**)&pRecord);
    if (pRecord == NULL)
    {
      Log("CStreamBufferRecorder::Create() cannot get IStreamBufferRecordControl");
      return false;
    }
    *id = m_iRecordingId;
    Log("CStreamBufferRecorder::Create() recorder created id:%d", *id);
    m_mapRecordControl[m_iRecordingId]=pRecord;
    m_iRecordingId++;

    Log("CStreamBufferRecorder::Create() done %x", pRecord);	
  }
  catch(...)
  {
    Log("CStreamBufferRecorder::Create() done exception");	
  }
  return TRUE;
}


void DvrMsStart(LONG id, ULONG startTime)
{
  imapRecordControl it = m_mapRecordControl.find(id);
  if (it == m_mapRecordControl.end())
  {
    return;
  }
  try
  {
    Log("CStreamBufferRecorder::Start():time:-%d secs in past id:%d", startTime,id);
    if (it->second == NULL)
    {
      Log("CStreamBufferRecorder::Start() recorded=null");
      return ;
    }
    Log("CStreamBufferRecorder::Start(1) %x",m_mapRecordControl[id]);
    REFERENCE_TIME timeStart = startTime;
    timeStart *= UNITS;
    timeStart *= -1LL;

    HRESULT hr = it->second->Start(&timeStart);
    if (FAILED(hr))
    {
      Log("CStreamBufferRecorder::Start() start at -%d sec failed:%X", startTime, hr);
      if (startTime != 0)
      {
        timeStart = 0;
        Log("CStreamBufferRecorder::Start() start at 0 sec");
        HRESULT hr = it->second->Start(&timeStart);
        if (FAILED(hr))
        {
          Log("CStreamBufferRecorder::Start() start failed:%x", hr);
          return ;
        }
      }
    }

    Log("CStreamBufferRecorder::Start(2)");
    HRESULT hrOut;
    BOOL started,stopped;
    it->second->GetRecordingStatus(&hrOut, &started, &stopped);
    timeStart /= UNITS;
    Log("CStreamBufferRecorder::Start() start status:%x started:%d stopped:%d at:%d secs", 
      hrOut,started,stopped, (long)timeStart);
    Log("CStreamBufferRecorder::Start() done");
  }
  catch(...)
  {
    Log("CStreamBufferRecorder::Start() exception");
  }
}


void DvrMsStop(LONG id)
{
  imapRecordControl it = m_mapRecordControl.find(id);
  if (it == m_mapRecordControl.end())
  {
    return;
  }
  try
  {
    Log("CStreamBufferRecorder::Stop()");
    if (it->second != NULL)
    {
      HRESULT hr = it->second->Stop(0);
      if (FAILED(hr))
      {
        Log("CStreamBufferRecorder::Stop() failed:%x", hr);
        hr = it->second->Stop(0);
        if (FAILED(hr))
        {
          Log("CStreamBufferRecorder::Stop() failed 2nd time :%x", hr);
          return ;
        }
      }
      for (int x=0; x < 10; ++x)
      {
        HRESULT hrOut;
        BOOL started,stopped;
        it->second->GetRecordingStatus(&hrOut, &started, &stopped);
        Log("CStreamBufferRecorder::Stop() status:%d %x started:%d stopped:%d", x, hrOut, started, stopped);
        if (stopped != 0)
        {
          break;
        }
        Sleep(100);
      }
      while (it->second->Release() > 0);
      m_mapRecordControl.erase(it);

    }
    Log("CStreamBufferRecorder::Stop() done");
  }
  catch(...)
  {
    Log("CStreamBufferRecorder::Stop() exception");
  }

}


HRESULT CreateKernelFilter(const GUID &guidCategory, LPCOLESTR szName, GUID	clsid, IBaseFilter **ppFilter)
{
  HRESULT hr;
  ICreateDevEnum *pDevEnum = NULL;
  IEnumMoniker *pEnum = NULL;
  if (!szName || !ppFilter) 
  {
    return E_POINTER;
  }

  // Create the system device enumerator.
  hr = CoCreateInstance(CLSID_SystemDeviceEnum, NULL, CLSCTX_INPROC, IID_ICreateDevEnum, (void**)&pDevEnum);
  if (FAILED(hr))
  {
    return hr;
  }

  // Create a class enumerator for the specified category.
  hr = pDevEnum->CreateClassEnumerator(guidCategory, &pEnum, 0);
  pDevEnum->Release();
  if (hr != S_OK) // S_FALSE means the category is empty.
  {
    return E_FAIL;
  }

  // Enumerate devices within this category.
  bool bFound = false;
  IMoniker *pMoniker;
  while (!bFound && (S_OK == pEnum->Next(1, &pMoniker, 0)))
  {
    IPropertyBag *pBag = NULL;
    hr = pMoniker->BindToStorage(0, 0, IID_IPropertyBag, (void **)&pBag);
    if (FAILED(hr))
    {
      pMoniker->Release();
      continue; // Maybe the next one will work.
    }

    // Check the friendly name.
    VARIANT var;
    VariantInit(&var);
    hr = pBag->Read(L"FriendlyName", &var, NULL);
    if (SUCCEEDED(hr) )
    {
      bool ok=true;
      for (DWORD x=0; x <(DWORD)wcslen(szName);x++)
      {
        if (var.bstrVal[x]!=szName[x])
        {
          ok=false;
        }
      }
      if (ok)
      {
        // This is the right filter.
        hr = pMoniker->BindToObject(0, 0, IID_IBaseFilter,(void**)ppFilter);
        bFound = true;
      }
    }
    VariantClear(&var);
    pBag->Release();
    pMoniker->Release();
  }
  pEnum->Release();
  return (bFound ? hr : E_FAIL);
}


void AddTeeSinkNameToGraph(IGraphBuilder* pGraph, LPCOLESTR szName)
{
  IBaseFilter* pKernelTee = NULL;
  HRESULT hr = CreateKernelFilter(AM_KSCATEGORY_SPLITTER, szName, clsidTeeSink, &pKernelTee);
  if (SUCCEEDED(hr))
  {
    pGraph->AddFilter(pKernelTee, L"Kernel Tee");
    pKernelTee->Release();
  }
}


void AddTeeSinkToGraph(IGraphBuilder* pGraph)
{
  AddTeeSinkNameToGraph(pGraph, OLESTR("Tee"));
}


void AddWstCodecToGraph(IGraphBuilder* pGraph)
{
  IBaseFilter* pWstCodec = NULL;
  HRESULT hr = CreateKernelFilter(AM_KSCATEGORY_VBICODEC, OLESTR("WST"), CLSID_WSTDecoder,&pWstCodec);
  if (SUCCEEDED(hr))
  {
    pGraph->AddFilter(pWstCodec, L"WST Codec");
    pWstCodec->Release();
  }
}

