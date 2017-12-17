/*
 *  Copyright (C) 2005-2013 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#pragma warning(disable:4995)
#include "StdAfx.h"

#include <winsock2.h>
#include <ws2tcpip.h>
#include <commdlg.h>
#include <bdatypes.h>
#include <sbe.h>
#include <time.h>
#include <streams.h>
#include <initguid.h>
#include <shlobj.h>
#include <tchar.h>
#include "tsreader.h"
#include "audiopin.h"
#include "videopin.h"
#include "subtitlepin.h"
#include "tsfileSeek.h"
#include "memoryreader.h"
#include "version.h"
#include "..\..\shared\DebugSettings.h"
#include <cassert>
#include <queue>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

//These are global variables, and can be shared between multiple TsReader instances !
DWORD m_tGTStartTime = 0;
long m_instanceCount = 0;
CCritSec m_instanceLock;


DEFINE_MP_DEBUG_SETTING(DoNotAllowSlowMotionDuringZapping)

//-------------------- Async logging methods -------------------------------------------------

//These are global variables, and can be shared between multiple TsReader instances !
WORD logFileParsed = -1;
WORD logFileDate = -1;

CTsReaderFilter* instanceID = 0;

CCritSec m_qLock;
CCritSec m_logLock;
CCritSec m_logFileLock;
std::queue<std::wstring> m_logQueue;
BOOL m_bLoggerRunning = false;
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
    _stprintf_s(logFolder, MAX_PATH, _T("%s\\Team Mediaportal\\MediaPortal\\log"), folder);

    //Read log folder path from registry (or write default path into registry if key doesn't exist)
    LPCTSTR logFolderC = logFolder;    
    LPCTSTR logFolderPath = _T("LogFolderPath");
    result = LogReadRegistryKeyString(hKey, logFolderPath, logFolderC);
    
    if (result == ERROR_SUCCESS)
    {
      //Get full log file path
      _stprintf_s(dest, MAX_PATH, _T("%s\\TsReader.%s"), logFolderC, name);
    }
  }
    
  if (result != ERROR_SUCCESS)
  {
    //Fall back to default log folder path
    TCHAR folder[MAX_PATH];
    SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
    //Get full log file path
    _stprintf_s(dest, MAX_PATH, _T("%s\\Team Mediaportal\\MediaPortal\\log\\TsReader.%s"), folder, name);
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


wstring GetLogLine()
{
  CAutoLock lock(&m_qLock);
  if ( m_logQueue.size() == 0 )
  {
    return L"";
  }
  wstring ret = m_logQueue.front();
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
        wstring line = GetLogLine();
        while (!line.empty())
        {
          fwprintf_s(fp, L"%s", line.c_str());
          line = GetLogLine();
        }
        fclose(fp);
      }
      else //discard data
      {
        wstring line = GetLogLine();
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
  CAutoLock logLock(&m_logLock);
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


void LogDebug(const wchar_t *fmt, ...) 
{
  CAutoLock logLock(&m_logLock);
  
  if (!m_hLogger) {
    m_bLoggerRunning = true;
    StartLogger();
  }

  wchar_t buffer[2000]; 
  int tmp;
  va_list ap;
  va_start(ap,fmt);
  tmp = vswprintf_s(buffer, fmt, ap);
  va_end(ap); 

  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  wchar_t msg[5000];
  swprintf_s(msg, 5000,L"[%04.4d-%02.2d-%02.2d %02.2d:%02.2d:%02.2d,%03.3d] [%p] [%4x] - %s\n",
    systemTime.wYear, systemTime.wMonth, systemTime.wDay,
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond, systemTime.wMilliseconds,
	(void*)instanceID,
    GetCurrentThreadId(),
    buffer);
  CAutoLock l(&m_qLock);
  if (m_logQueue.size() < 2000) 
  {
    m_logQueue.push((wstring)msg);
  }
};

void LogDebug(const char *fmt, ...)
{
  char logbuffer[2000]; 
  wchar_t logbufferw[2000];

	va_list ap;
	va_start(ap,fmt);
	vsprintf_s(logbuffer, fmt, ap);
	va_end(ap); 

	MultiByteToWideChar(CP_ACP, 0, logbuffer, -1,logbufferw, sizeof(logbuffer)/sizeof(wchar_t));
	LogDebug(L"%s", logbufferw);
};

//------------------------------------------------------------------------------------



const AMOVIESETUP_MEDIATYPE acceptAudioPinTypes =
{
  &MEDIATYPE_Audio,             // major type
  &MEDIASUBTYPE_MPEG1Audio      // minor type
};
const AMOVIESETUP_MEDIATYPE acceptVideoPinTypes =
{
  &MEDIATYPE_Video,             // major type
  &MEDIASUBTYPE_MPEG2_VIDEO     // minor type
};

const AMOVIESETUP_MEDIATYPE acceptSubtitlePinTypes =
{
  &MEDIATYPE_Stream,            // major type
  &MEDIASUBTYPE_MPEG2_TRANSPORT // minor type
};

const AMOVIESETUP_PIN audioVideoPin[] =
{
  {L"Audio",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptAudioPinTypes},
  {L"Video",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptVideoPinTypes},
  {L"Subtitle",FALSE,TRUE,FALSE,FALSE,&CLSID_NULL,NULL,1,&acceptSubtitlePinTypes}
};

const AMOVIESETUP_FILTER TSReader =
{
  &CLSID_TSReader,L"MediaPortal File Reader",MERIT_NORMAL+1000,3,audioVideoPin,CLSID_LegacyAmFilterCategory
};

CFactoryTemplate g_Templates[] =
{
  {L"MediaPortal File Reader",&CLSID_TSReader,CTsReaderFilter::CreateInstance,NULL,&TSReader},
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);


CUnknown * WINAPI CTsReaderFilter::CreateInstance(LPUNKNOWN punk, HRESULT *phr)
{
  ASSERT(phr);
  CTsReaderFilter *pNewObject = new CTsReaderFilter(punk, phr);

  if (pNewObject == NULL)
  {
    if (phr)
      *phr = E_OUTOFMEMORY;
  }
  return pNewObject;
}


// Constructor
CTsReaderFilter::CTsReaderFilter(IUnknown *pUnk, HRESULT *phr):
  CSource(NAME("CTsReaderFilter"), pUnk, CLSID_TSReader),
  m_pAudioPin(NULL),
  m_pVideoPin(NULL),
  m_pSubtitlePin(NULL),
  m_demultiplexer( m_duration, *this),
  m_rtspClient(m_buffer),
  m_pDVBSubtitle(NULL),
  m_pCallback(NULL),
  m_pRequestAudioCallback(NULL)
{
  { // Scope for CAutoLock
    CAutoLock lock(&m_instanceLock);  
    if (m_instanceCount == 0)
    {
      //Initialise m_tGTStartTime for GET_TIME_NOW() macro.
      //The macro is used to avoid having to handle timeGetTime()
      //rollover issues in the body of the code
      m_tGTStartTime = (timeGetTime() - 0x40000000); 
    }
  }

  // Set timer resolution to 1 ms (if possible)
  TIMECAPS tc; 
  dwResolution = 0; 
  if (timeGetDevCaps(&tc, sizeof(TIMECAPS)) == MMSYSERR_NOERROR)
  {
    dwResolution = min(max(tc.wPeriodMin, 1), tc.wPeriodMax);
    if (dwResolution)
    {
      timeBeginPeriod(dwResolution);
    }
  }

  instanceID = this;  
  
  // use the following line if you are having trouble setting breakpoints
  // #pragma comment( lib, "strmbasd" )

  LogDebug("------------- v%d.%d.%d.%d ------------- instanceCount:%d", TSREADER_MAJOR_VERSION, TSREADER_MID_VERSION, TSREADER_VERSION, TSREADER_POINT_VERSION, m_instanceCount);
  
  m_fileReader=NULL;
  m_fileDuration=NULL;
  Compensation=CRefTime(0L);
  
  LogDebug("CTsReaderFilter::ctor");
  m_pAudioPin = new CAudioPin(GetOwner(), this, phr,&m_section);
  m_pVideoPin = new CVideoPin(GetOwner(), this, phr,&m_section);
  m_pSubtitlePin = new CSubtitlePin(GetOwner(), this, phr,&m_section);

  if (m_pAudioPin == NULL || m_pVideoPin == NULL || m_pSubtitlePin == NULL)
  {
    *phr = E_OUTOFMEMORY;
    return;
  }
  wcscpy(m_fileName,L"");
  m_dwGraphRegister = 0;

  //Read (and create if needed) debug registry settings
  HKEY key;
  m_bDisableVidSizeRebuildMPEG2 = false;
  m_bDisableVidSizeRebuildH264 = false;
  m_bDisableAddPMT = false;
  m_bForceFFDShowSyncFix = false;
  m_bUseFPSfromDTSPTS = true;
  m_regInitialBuffDelay = INITIAL_BUFF_DELAY;
  m_bEnableBufferLogging = false;
  m_bSubPinConnectAlways = false;
  m_regAudioDelay = AUDIO_DELAY; 
  m_regSlowPlayInPPM = SLOW_PLAY_PPM; 
  m_AutoSpeedAdjust = 2; //enable both speed up and slow down
  if (ERROR_SUCCESS==RegCreateKeyEx(HKEY_CURRENT_USER, _T("Software\\Team MediaPortal\\TsReader"), 0, NULL, 
                                    REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &key, NULL))
  {
    DWORD keyValue = 0;
    LPCTSTR disableVidSizeRebuildMPEG2 = _T("DisableVidSizeRebuildMPEG2");
    ReadRegistryKeyDword(key, disableVidSizeRebuildMPEG2, keyValue);
    if (keyValue)
    {
      LogDebug("--- DisableVidSizeRebuildMPEG2 = yes");
      m_bDisableVidSizeRebuildMPEG2 = true;
    }

    keyValue = 0;
    LPCTSTR disableVidSizeRebuildH264 = _T("DisableVidSizeRebuildH264");
    ReadRegistryKeyDword(key, disableVidSizeRebuildH264, keyValue);
    if (keyValue)
    {
      LogDebug("--- DisableVidSizeRebuildH264 = yes");
      m_bDisableVidSizeRebuildH264 = true;
    }

    keyValue = 0;
    LPCTSTR disableAddPMT = _T("DisableAddPMT");
    ReadRegistryKeyDword(key, disableAddPMT, keyValue);
    if (keyValue)
    {
      LogDebug("--- DisableAddPMT = yes");
      m_bDisableAddPMT = true;
    }

    keyValue = 0;
    LPCTSTR forceFFDShowSyncFix_RRK = _T("ForceFFDShowSyncFix");
    ReadRegistryKeyDword(key, forceFFDShowSyncFix_RRK, keyValue);
    if (keyValue)
    {
      LogDebug("--- ForceFFDShowSyncFix = yes");
      m_bForceFFDShowSyncFix = true;
    }
    
    keyValue = m_bUseFPSfromDTSPTS ? 1 : 0;
    LPCTSTR useFPSfromDTSPTS_RRK = _T("UseFPSfromDTSPTS");
    ReadRegistryKeyDword(key, useFPSfromDTSPTS_RRK, keyValue);
    if (keyValue)
    {
      LogDebug("--- UseFPSfromDTSPTS = yes");
      m_bUseFPSfromDTSPTS = true;
    }
    else
    {
      m_bUseFPSfromDTSPTS = false;
    }

    keyValue = (DWORD)m_regInitialBuffDelay;
    LPCTSTR initialBuffDelay_RRK = _T("BufferingDelayInMilliSeconds");
    ReadRegistryKeyDword(key, initialBuffDelay_RRK, keyValue);
    if ((keyValue >= 0) && (keyValue <= 10000))
    {
      m_regInitialBuffDelay = (LONG)keyValue;
      LogDebug("--- Buffering delay = %d ms", m_regInitialBuffDelay);
    }
    else
    {
      m_regInitialBuffDelay = INITIAL_BUFF_DELAY;
      LogDebug("--- Buffering delay = %d ms (default value, allowed range is %d - %d)", m_regInitialBuffDelay, 0, 10000);
    }
    
    keyValue = 0;
    LPCTSTR enableBufferLogging = _T("EnableBufferLogging");
    ReadRegistryKeyDword(key, enableBufferLogging, keyValue);
    if (keyValue)
    {
      LogDebug("--- EnableBufferLogging = yes");
      m_bEnableBufferLogging = true;
    }

    keyValue = 0;
    LPCTSTR subConnectAlways = _T("SubPinConnectAlways");
    ReadRegistryKeyDword(key, subConnectAlways, keyValue);
    if (keyValue)
    {
      LogDebug("--- SubPinConnectAlways = yes");
      m_bSubPinConnectAlways = true;
    }

    keyValue = (DWORD)(m_regAudioDelay/10000);
    LPCTSTR regAudioDelay_RRK = _T("AudioDelayInMilliSeconds");
    ReadRegistryKeyDword(key, regAudioDelay_RRK, keyValue);
    if ((keyValue >= 0) && (keyValue <= 500))
    {
      m_regAudioDelay = (REFERENCE_TIME)(keyValue*10000);
      LogDebug("--- Audio delay = %d ms", (m_regAudioDelay/10000));
    }
    else
    {
      m_regAudioDelay = AUDIO_DELAY;
      LogDebug("--- Audio delay = %d ms (default value, allowed range is %d - %d)", (m_regAudioDelay/10000), 0, 500);
    }

    keyValue = (DWORD)m_regSlowPlayInPPM;
    LPCTSTR regSlowPlayInPPM_RRK = _T("SlowPlayInPPM");
    ReadRegistryKeyDword(key, regSlowPlayInPPM_RRK, keyValue);
    if ((keyValue >= 0) && (keyValue <= SPEED_ADJ_LIMIT))
    {
      m_regSlowPlayInPPM = (REFERENCE_TIME)keyValue;
      LogDebug("--- Slow Play = %d PPM", m_regSlowPlayInPPM);
    }
    else
    {
      m_regSlowPlayInPPM = SLOW_PLAY_PPM;
      LogDebug("--- Slow Play = %d PPM (default value, allowed range is %d - %d)", m_regSlowPlayInPPM, 0, SPEED_ADJ_LIMIT);
    }

    keyValue = (DWORD)m_AutoSpeedAdjust;
    LPCTSTR autoSpeedAdjust_RRK = _T("EnableAutoSpeedAdjust");
    ReadRegistryKeyDword(key, autoSpeedAdjust_RRK, keyValue);
    m_AutoSpeedAdjust = (int)keyValue;
    LogDebug("--- AutoSpeedAdjust = %d", m_AutoSpeedAdjust);
    

    keyValue = m_rtspClient.m_regRtspGenericTimeout;
    LPCTSTR rtspGenericTimeout_RRK = _T("RtspGenericTimeoutInMilliSeconds");
    ReadRegistryKeyDword(key, rtspGenericTimeout_RRK, keyValue);
    if ((keyValue >= 100) && (keyValue <= 2000))
    {
      m_rtspClient.m_regRtspGenericTimeout = keyValue;
      LogDebug("--- RTSP generic timeout = %d ms", m_rtspClient.m_regRtspGenericTimeout);
    }
    else
    {
      m_rtspClient.m_regRtspGenericTimeout = TIMEOUT_GENERIC_RTSP_RESPONSE;
      LogDebug("--- RTSP generic timeout = %d ms (default value, allowed range is %d - %d)", m_rtspClient.m_regRtspGenericTimeout, 100, 2000);
    }

    keyValue = m_rtspClient.m_regRtspFileTimeout;
    LPCTSTR rtspFileTimeout_RRK = _T("RtspFileTimeoutInMilliSeconds");
    ReadRegistryKeyDword(key, rtspFileTimeout_RRK, keyValue);
    if ((keyValue >= 100) && (keyValue <= 10000))
    {
      m_rtspClient.m_regRtspFileTimeout = keyValue;
      LogDebug("--- RTSP file timeout = %d ms", m_rtspClient.m_regRtspFileTimeout);
    }
    else
    {
      m_rtspClient.m_regRtspFileTimeout = TIMEOUT_FILE_ACTION_RTSP_RESPONSE;
      LogDebug("--- RTSP file timeout = %d ms (default value, allowed range is %d - %d)", m_rtspClient.m_regRtspFileTimeout, 100, 10000);
    }


    RegCloseKey(key);
  }

  //Read LAV filter registry settings
  m_regLAV_AutoAVSync = 0;
  if (ERROR_SUCCESS==RegOpenKeyEx(HKEY_CURRENT_USER, _T("Software\\LAV\\Audio"), 0, KEY_READ | KEY_WOW64_32KEY, &key))
  {
    DWORD keyValue = (DWORD)m_regLAV_AutoAVSync;
    LPCTSTR regLAV_AutoAVSync_RRK = _T("AutoAVSync");
    if (ERROR_SUCCESS==ReadOnlyRegistryKeyDword(key, regLAV_AutoAVSync_RRK, keyValue))
    {
      m_regLAV_AutoAVSync = keyValue;
      LogDebug("--- LAV_AutoAVSync = %d", m_regLAV_AutoAVSync);      
    }

    RegCloseKey(key);
  }
  
  // Set default filtering mode (normal), if not overriden externally (see ITSReader::SetRelaxedMode)
  m_demultiplexer.m_DisableDiscontinuitiesFiltering = false;
  
  if(!DoNotAllowSlowMotionDuringZapping())
  {
    LogDebug("--- Slow motion video allowed during zapping = yes");
    m_EnableSlowMotionOnZapping = true;
  }
  else
  {
    LogDebug("--- Slow motion video allowed during zapping = no");
    m_EnableSlowMotionOnZapping = false;
  }
  
  LogDebug("Wait for seeking to eof - false - constructor");
  m_bWaitForSeek=false;
  m_isUNCfile = false;
  m_bLiveTv = false;
  m_bTimeShifting = false;
  m_RandomCompensation = 0; 
  m_TotalDeltaCompensation = 0;    
  m_bAnalog = false;
  m_bStopping = false;
  m_bOnZap = false;
  m_bZapinProgress = false;
  m_bDurationThreadBusy = false;
  m_bPauseOnClockTooFast = false;
  m_bForcePosnUpdate = false;
  SetMediaPosnUpdate(0) ;
  m_bStoppedForUnexpectedSeek=false ;
  m_bForceSeekOnStop=false ;
  m_bForceSeekAfterRateChange=false ;
  m_bSeekAfterRcDone=false ;
  m_videoDecoderCLSID=GUID_NULL;
  m_subtitleCLSID=GUID_NULL;
  m_bFastSyncFFDShow=false;
  m_ShowBufferAudio = INIT_SHOWBUFFERAUDIO;
  m_ShowBufferVideo = INIT_SHOWBUFFERVIDEO;
  m_bMPARinGraph = false;
  m_bEVRhasConnected = false;
  m_MPmainThreadID = GetCurrentThreadId() ;
  m_lastPauseRun = GET_TIME_NOW();

  LogDebug("CTsReaderFilter::Start demux thread");
  if (m_demultiplexer.StartThread() != S_OK)
  {
    *phr = E_OUTOFMEMORY;
    return;
  }
  
  LogDebug("CTsReaderFilter::Start duration thread");
  if (StartThread() != S_OK)
  {
    *phr = E_OUTOFMEMORY;
    return;
  }

  LogDebug("CTsReaderFilter::timeGetTime():0x%x, m_tGTStartTime:0x%x, GET_TIME_NOW:0x%x, timer res:%d ms", timeGetTime(), m_tGTStartTime, GET_TIME_NOW(), dwResolution);
}

CTsReaderFilter::~CTsReaderFilter()
{
  LogDebug("CTsReaderFilter::dtor");

  // Reset timer resolution (if we managed to set it originally)
  if (dwResolution)
  {
    timeEndPeriod(dwResolution);
  }

  //stop duration thread
  StopThread(5000);
  
  //stop demux flush/read ahead thread
  m_demultiplexer.m_bShuttingDown = true;
  m_demultiplexer.StopThread(5000);
  
  HRESULT hr = m_pAudioPin->Disconnect();
  delete m_pAudioPin;

  hr=m_pVideoPin->Disconnect();
  delete m_pVideoPin;

  hr=m_pSubtitlePin->Disconnect();
  delete m_pSubtitlePin;
  ReleaseSubtitleFilter();

  if (m_fileReader != NULL)
    delete m_fileReader;
  if (m_fileDuration != NULL)
    delete m_fileDuration;
  LogDebug("CTsReaderFilter::dtor - finished");
  StopLogger();
}

STDMETHODIMP CTsReaderFilter::NonDelegatingQueryInterface(REFIID riid, void ** ppv)
{
  if (riid == IID_IMediaSeeking)
  {
    LogDebug("filt:IID_IMediaSeeking()");
    if (m_pAudioPin->IsConnected())
      return m_pAudioPin->NonDelegatingQueryInterface(riid, ppv);

    if (m_pVideoPin->IsConnected())
      return m_pVideoPin->NonDelegatingQueryInterface(riid, ppv);
  }
  if (riid == IID_IAMFilterMiscFlags)
  {
    return GetInterface((IAMFilterMiscFlags*)this, ppv);
  }
  if (riid == IID_IFileSourceFilter)
  {
    return GetInterface((IFileSourceFilter*)this, ppv);
  }
  if (riid == IID_IAMStreamSelect)
  {
    return GetInterface((IAMStreamSelect*)this, ppv);
  }
    if (riid == IID_ITeletextSource)
  {
    //LogDebug("filt:IID_ITeletextSource()");
    return GetInterface((ITeletextSource*)this, ppv);
  }
  if (riid == IID_ISubtitleStream)
  {
    //LogDebug("filt:IID_ISubtitleStream()");
    HRESULT hr =  GetInterface((ISubtitleStream*)this, ppv);
    if(SUCCEEDED(hr))
    {
      LogDebug("SUCCESS",hr);
    }
    else
    {
      LogDebug("FAILED",hr);
    }
    return hr;
  }
  if ( riid == IID_ITSReader )
  {
    return GetInterface((ITSReader*)this, ppv);
  }
  if ( riid == IID_IAudioStream )
  {
    return GetInterface((IAudioStream*)this, ppv);
  }
  return CSource::NonDelegatingQueryInterface(riid, ppv);
}

CBasePin * CTsReaderFilter::GetPin(int n)
{
  if (n == 0)
  {
    return m_pAudioPin;
  }
  else  if (n == 1)
  {
    return m_pVideoPin;
  }
  else if (n == 2)
  {
    return m_pSubtitlePin;
  }
  return NULL;
}


int CTsReaderFilter::GetPinCount()
{
  return 3;
}

void CTsReaderFilter::OnMediaTypeChanged(int mediaTypes)
{
  if ( m_pCallback ) m_pCallback->OnMediaTypeChanged(mediaTypes);
  
  if (GetAudioPin() && (mediaTypes & AUDIO_CHANGE))
  {
    GetAudioPin()->SetDiscontinuity(true);
    GetAudioPin()->SetAddPMT();
  }
  if (GetVideoPin() && (mediaTypes & VIDEO_CHANGE))
  {
    GetVideoPin()->SetDiscontinuity(true);
    GetVideoPin()->SetAddPMT();
  }
}

void CTsReaderFilter::OnRequestAudioChange()
{
  if ( m_pRequestAudioCallback ) m_pRequestAudioCallback->OnRequestAudioChange();
}

bool CTsReaderFilter::CheckAudioCallback()
{
  return (m_pRequestAudioCallback != NULL);
}

bool CTsReaderFilter::CheckCallback()
{
  return (m_pCallback != NULL);
}


void CTsReaderFilter::OnVideoFormatChanged(int streamType,int width,int height,int aspectRatioX,int aspectRatioY,int bitrate,int isInterlaced)
{
  if ( m_pCallback )
    m_pCallback->OnVideoFormatChanged(streamType,width,height,aspectRatioX,aspectRatioY,bitrate,isInterlaced);
}

void CTsReaderFilter::OnBitRateChanged(int bitrate)
{
  if ( m_pCallback)
    m_pCallback->OnBitRateChanged(bitrate);
}

STDMETHODIMP CTsReaderFilter::SetGraphCallback(ITSReaderCallback* pCallback)
{
  LogDebug("CALLBACK SET");
  m_pCallback = pCallback;
  return S_OK;
}

STDMETHODIMP CTsReaderFilter::SetRequestAudioChangeCallback(ITSReaderAudioChange* pCallback)
{
  LogDebug("SetRequestAudioChangeCallback SET");
  m_pRequestAudioCallback = pCallback;
  return S_OK;
}


STDMETHODIMP CTsReaderFilter::SetRelaxedMode(BOOL relaxedReading)
{
  LogDebug("SetRelaxedMode");
  if (relaxedReading == FALSE)
  {
    LogDebug("Normal discontinuities filtering");
    m_demultiplexer.m_DisableDiscontinuitiesFiltering = false;
  }
  else
  {
    LogDebug("Relaxed discontinuities filtering");
    m_demultiplexer.m_DisableDiscontinuitiesFiltering = true;
  }
  return S_OK;
}


double CTsReaderFilter::DurationUpdate()
{
  double duration = ((double)m_duration.Duration().Millisecs())/1000.0f;

  if (m_fileDuration!=NULL) //Local or UNC path file
  {
    //Update file duration
    m_duration.SetFileReader(m_fileDuration);
    m_duration.UpdateDuration(false, false);
    m_duration.CloseBufferFiles();
    duration = ((double)m_duration.Duration().Millisecs())/1000.0f;
  }
  else //RTSP
  {      
    //Update RTSP duration
    m_rtspClient.UpdateDuration();
    CPcr pcrStart, pcrEnd, pcrMax ;
    duration = m_rtspClient.Duration() / 1000.0f ;
    double start = m_duration.StartPcr().ToClock() ;
    double end = m_duration.EndPcr().ToClock() ; 
  
    // EndPcr is continuously increasing ( until ~26 hours for rollover that will fail ! )
    // So, we refer duration to End, and just update start.
    end = (double)(GET_TIME_NOW()-m_tickCount)/1000.0 ;
    start  = end - duration;
    if (start<0) 
      start=0 ;
  
    //set the duration
    pcrStart.FromClock(start) ;
    pcrStart.IsValid = true ;
    pcrEnd.FromClock(end);
    pcrEnd.IsValid = true ;
    m_duration.Set( pcrStart, pcrEnd, pcrMax); 
  }
   
  // Is graph running?
  if ((State() != State_Stopped) && !m_bStopping)
  {
    //yes, then send a EC_LENGTH_CHANGED event to the graph
    NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
  }
  m_bRecording = true; //Force duration thread to update soon...
  LogDebug("CTsReaderFilter::DurationUpdate() - StartPCR %f, EndPcr %f, Duration %f", m_duration.StartPcr().ToClock(),m_duration.EndPcr().ToClock(),(float)duration) ;
  
  return duration;
}

void STDMETHODCALLTYPE CTsReaderFilter::OnZapping(int info)
{
  LogDebug("OnZapping() - info 0x%x", info);
  // Theoretically a new PAT ( equal to PAT+1 modulo 16 ) will be issued by TsWriter.
  if (info == 0x80)
  {
    m_bOnZap = true ;
    m_bZapinProgress = true;
    m_demultiplexer.RequestNewPat();
    m_bAnalog = false;    

    if (m_bTimeShifting)
    {
      DurationUpdate();
    }
  }
  else
  {
  // Analog or card assigment failure
    if (info <= 0)							
    {
      m_demultiplexer.ClearRequestNewPat();
      m_bAnalog = true;
    }
  }
  
  return;
}

void STDMETHODCALLTYPE CTsReaderFilter::OnGraphRebuild(int info)
{
  LogDebug("CTsReaderFilter::OnGraphRebuild %d",info);
  m_demultiplexer.SetMediaChanging(false);
}


STDMETHODIMP CTsReaderFilter::GetState(DWORD dwMilliSecsTimeout, FILTER_STATE *pState)
{
  if (pState==NULL) 
  {
    LogDebug("CTsReaderFilter::GetState(), null pointer");
    return E_POINTER;
  }

   
  *pState = m_State;
  if (m_State == State_Paused)
  {    
    double playRate = 1.0;
    if (m_pAudioPin->IsConnected())
    {
      if (FAILED(m_pAudioPin->GetRate(&playRate)))
      {
        playRate = 1.0;
      }
    }

    bool isAVReady =  m_bStreamCompensated
              && (!m_bMPARinGraph || (GET_TIME_NOW() > m_demultiplexer.m_targetAVready) || m_audioReady);
    
    //FFWD is more responsive if we return VFW_S_CANT_CUE when rate != 1.0
    if (isAVReady || (playRate != 1.0) || m_demultiplexer.EndOfFile())
    {
      LogDebug("CTsReaderFilter::GetState(), VFW_S_CANT_CUE, m_audioReady %d",m_audioReady);
      return VFW_S_CANT_CUE;
    }
    else
    {
      //Stall for a while...
      //LogDebug("CTsReaderFilter::GetState(), wait isAVReady, loop %d", loopCnt);
      return VFW_S_STATE_INTERMEDIATE;
    }
  }
  else
  {
    //LogDebug("CTsReaderFilter::GetState(), %d", m_State);
    return S_OK;
  }
}


STDMETHODIMP CTsReaderFilter::Run(REFERENCE_TIME tStart)
{
  CRefTime runTime=tStart;
  double msec=(double)runTime.Millisecs();
  msec/=1000.0;
  LogDebug("CTsReaderFilter::Run(%05.2f) state %d seeking %d", msec, m_State, IsSeeking());
  
  m_RandomCompensation = 0;

  if (m_bStreamCompensated && m_bLiveTv)
  {
    m_bZapinProgress = false;
    LogDebug("CTsReaderFilter::Run() - Elapsed time from pause to Audio/Video ( total zapping time ) : %d mS",GET_TIME_NOW()-m_lastPauseRun);
  }
  
  m_lastPauseRun = GET_TIME_NOW();
 
  CAutoLock cObjectLock(m_pLock);
 
  //Wait for seeking to finish
  while (IsSeeking()) 
  {
    Sleep(1);
  }

  m_bSeekAfterRcDone = false;


  if(m_pSubtitlePin) m_pSubtitlePin->SetRunningStatus(true);
	
	//are we using RTSP or local file
  if (m_fileDuration==NULL)
  {
    //using RTSP, if its streaming is paused then
    //stop pausing and continue streaming
    LogDebug("CTsReaderFilter::Run() - continue RTSP");
    m_buffer.Run(true); //Just in case...
    m_rtspClient.Continue();
  }

  m_demultiplexer.m_LastDataFromRtsp=GET_TIME_NOW();
  //Set our StreamTime Reference offset to zero
  SetMediaPosnUpdate(m_MediaPos) ;   // reset offset.

  HRESULT hr= CSource::Run(tStart);
  
  m_bPauseOnClockTooFast=false ;

  m_ShowBufferVideo = INIT_SHOWBUFFERVIDEO;
  m_ShowBufferAudio = INIT_SHOWBUFFERAUDIO;
  
  LogDebug("CTsReaderFilter::Run(%05.2f) state %d -->done",msec,m_State);
  return hr;
}

STDMETHODIMP CTsReaderFilter::Stop()
{
  LogDebug("CTsReaderFilter::Stop(), state %d", m_State);

  CAutoLock cObjectLock(m_pLock);

  HRESULT hr = S_OK;
  
  //guarantees that audio/video/subtitle pins dont block in the fillbuffer() method
  m_bStopping = true;
  //Block duration file read updates
  m_updateThreadDuration.StopUpdate(true);
  
  //Cancel all pending IO operations, to avoid possible hangs if network connection is lost and to speed up the 'stop'
  m_fileReader->SetStopping(true);
  if (m_fileDuration != NULL)
  {
    m_fileDuration->SetStopping(true);
  }

  int i=0;
  //Wait for output pin data sample delivery and seeking to finish - timeout after 100 loop iterations in case pin delivery threads are stalled
  while ((i < 100) && (GetAudioPin()->IsInFillBuffer() || GetVideoPin()->IsInFillBuffer() || GetSubtitlePin()->IsInFillBuffer() || IsSeeking()) )
  {
    Sleep(5);
    i++;
  }
  if (i >= 100)
  {
    LogDebug("CTsReaderFilter: Stop: InFillBuffer() wait timeout");
  }

  if (m_pSubtitlePin)
  {
    m_pSubtitlePin->SetRunningStatus(false);
  }

  { //Context for CAutoLock
    //LogDebug("CTsReaderFilter::Stop()  -stop source pre-lock RAL, state %d", m_State);
    CAutoLock rLock (&m_ReadAheadLock);
    //LogDebug("CTsReaderFilter::Stop()  -stop source pre-lock DTL, state %d", m_State);
    CAutoLock dLock (&m_DurationThreadLock);
    //LogDebug("CTsReaderFilter::Stop()  -stop source, state %d", m_State);
    //stop filter
    hr = CSource::Stop();
    WakeThread(); //Encourage duration thread to see 'stopped' state
  }
  //LogDebug("CTsReaderFilter::Stop()  -stop source done, state %d", m_State);


  //are we using rtsp?
  if (m_fileDuration == NULL)
  {
    if (!m_demultiplexer.IsMediaChanging())
    {
      //yep then pause streaming
      LogDebug("CTsReaderFilter::Stop()  -- pause RTSP");
      m_buffer.Run(false);
      PauseRtspStreaming();
    }
    else
    {
      LogDebug("CTsReaderFilter::Stop()  -- Media changing - continue RTSP");
    }
  }
  
  if (m_bStreamCompensated)
  {
    //m_demultiplexer.Flush(true) ;
    //Flushing is delegated
    m_demultiplexer.DelegatedFlush(true, true);
  }
    
  LogDebug("CTsReaderFilter::Stop() done, state %d", m_State);
  //reset values
  m_bSeekAfterRcDone = false;
  m_bStopping = false;
  m_updateThreadDuration.StopUpdate(false);
  m_fileReader->SetStopping(false);
  if (m_fileDuration != NULL)
  {
    m_fileDuration->SetStopping(false);
  }
  m_bStoppedForUnexpectedSeek=true ;
  return hr;
}

bool CTsReaderFilter::IsTimeShifting()
{
  return m_bTimeShifting;
}

bool CTsReaderFilter::IsRTSP()
{
  return (m_fileDuration == NULL);
}

bool CTsReaderFilter::IsUNCfile()
{
  return m_isUNCfile;
}

bool CTsReaderFilter::IsLiveTV()
{
  return m_bLiveTv;
}


STDMETHODIMP CTsReaderFilter::Pause()
{
  m_ShowBufferVideo = INIT_SHOWBUFFERVIDEO;
  m_ShowBufferAudio = INIT_SHOWBUFFERAUDIO;

  LogDebug("CTsReaderFilter::Pause() - IsTimeShifting = %d - state = %d", IsTimeShifting(), m_State);
  HRESULT hr = S_FALSE;
  
  { //Set scope for lock
    CAutoLock cObjectLock(m_pLock);
    
    FILTER_STATE old_State = m_State;
      
    if (m_State == State_Running)
    {
      m_lastPauseRun = GET_TIME_NOW();
      m_RandomCompensation = 0;
    }
    m_demultiplexer.m_bAudioSampleLate=false;
  
    //pause filter - this will update m_State
    hr=CSource::Pause();
  
    if (!m_bPauseOnClockTooFast)
    {
      CheckForMPAR();
      
      //are we using rtsp?
      if (m_fileDuration==NULL)
      {
        //yes, are we busy seeking?
        if (!IsSeeking()) //When seeking, RTSP pause/play is handled in Seek() so don't do anything here
        {
          if (old_State == State_Stopped) //Transition to 'Pause' from 'Stopped'
          {
            if (m_demultiplexer.IsMediaChanging())
            {
              //Paused or stopped due to graph rebuild, so just continue streaming (no seeking should have occured since the last pause)
              LogDebug("CTsReaderFilter::Pause(), Media changing - continue RTSP");
              m_buffer.Run(true);
              m_rtspClient.Continue();
            }
            else if (m_rtspClient.IsPaused())  //Start streaming
            {
              double startTime=m_seekTime.Millisecs();
              startTime/=1000.0;
      
              //clear RTSP buffer
              LogDebug("CTsReaderFilter::Pause() - start RTSP from %f", startTime);
              m_buffer.Clear();
                                
              //start streaming
              m_buffer.Run(true);
              
              if (m_bTimeShifting && m_bZapinProgress)
              {
                startTime = DurationUpdate(); //Force play to start to end of timeshift file when zapping
              }
              
              m_rtspClient.Play(startTime, ((double)m_duration.Duration().Millisecs())/1000.0);
              
              LogDebug("CTsReaderFilter::Pause() - play RTSP, Timeshift %d, Recording %d, StartPCR %f, EndPcr %f, Duration %f",m_bTimeShifting,m_bRecording,m_duration.StartPcr().ToClock(),m_duration.EndPcr().ToClock(),(float)m_duration.Duration().Millisecs()/1000.0f) ;
            }                
            m_bRecording = true; //Force a duration update soon...
          }
          else if (old_State == State_Running && !m_demultiplexer.IsMediaChanging())
          {
            //we are transitioning from 'Run' to 'Paused' state and media is not changing
            PauseRtspStreaming();
          }
        }
      }
      m_demultiplexer.m_LastDataFromRtsp = GET_TIME_NOW() ;
    }
  }
    
  LogDebug("CTsReaderFilter::Pause() - END - state = %d", m_State);
  SetMediaPosnUpdate(m_MediaPos) ;   // reset offset.
  
  m_bForcePosnUpdate = true;
  WakeThread();
  
  return hr;
}

STDMETHODIMP CTsReaderFilter::GetDuration(REFERENCE_TIME *dur)
{
  if(!dur)
    return E_INVALIDARG;

  CAutoLock lock (&m_CritSecDuration);
  *dur = (REFERENCE_TIME)m_duration.Duration();

  return NOERROR;
}

STDMETHODIMP CTsReaderFilter::Load(LPCOLESTR pszFileName,const AM_MEDIA_TYPE *pmt)
{
  LogDebug("CTsReaderFilter::Load()");
  //clean up any old file readers
  if (m_fileReader != NULL)
    delete m_fileReader;
  if (m_fileDuration != NULL)
    delete m_fileDuration;
  m_buffer.Run(false);
  m_rtspClient.Stop();
  m_fileReader = NULL;
  m_fileDuration = NULL;
  m_seekTime = CRefTime(0L);
  m_absSeekTime = CRefTime(0L);
  m_bWaitForSeek=false;
  m_bRecording=false ;
  m_isUNCfile = false;
  m_updateThreadDuration.StopUpdate(false);
  DWORD startTimeout = 10000;
  m_bOnZap = false;
  m_bZapinProgress = false;

  wcscpy(m_fileName, pszFileName);
  char url[MAX_PATH];
  WideCharToMultiByte(CP_ACP, 0 ,m_fileName, -1, url, MAX_PATH, 0, 0);
  //check file type
  int length=strlen(url);
  if ((length > 5) && (_strcmpi(&url[length-4], ".tsp") == 0))
  {
    // .tsp file
    m_bTimeShifting = true;
    m_bLiveTv = true;

    FILE* fd = fopen(url, "rb");
    if (fd == NULL) return E_FAIL;
    fread(url, 1, 100, fd);
    int bytesRead = fread(url, 1, sizeof(url), fd);
    if (bytesRead >= 0) url[bytesRead] = 0;
    fclose(fd);

    LogDebug("open rtsp:%s", url);
    if ( !m_rtspClient.OpenStream(url)) return E_FAIL;

    m_buffer.Clear();
    m_buffer.Run(true);
    m_rtspClient.Play(0.0,0.0);
    m_tickCount = GET_TIME_NOW();
    m_fileReader = new CMemoryReader(m_buffer);
    m_demultiplexer.SetFileReader(m_fileReader);
    if (!m_demultiplexer.Start(startTimeout))
    {
      LogDebug("CTsReaderFilter::Load(), close rtsp:%s", url);
      m_buffer.Run(false);
      m_rtspClient.Stop();
      return E_FAIL;
    }

    // Pause. This will result in faster startup and channel change times,
    // because we don't have to SETUP a whole new session with the server.
    //m_rtspClient.Pause();

    //Note - calling m_rtspClient.OpenStream() above also updates the RTSP duration (in the RTSPClient)
    m_tickCount = GET_TIME_NOW()-m_rtspClient.Duration();   // Will be ready to update "virtual end Pcr" on recording in progress.

    double duration = m_rtspClient.Duration() / 1000.0f;
    CPcr pcrstart, pcrEnd, pcrMax;
    pcrstart = m_duration.StartPcr();
    duration += pcrstart.ToClock();
    pcrEnd.FromClock(duration);
    pcrstart.IsValid=true ;
    pcrEnd.IsValid=true ;
    m_duration.Set(pcrstart, pcrEnd, pcrMax);    //Load()
  }
  else if ((length > 7) && (_strnicmp(url, "rtsp://",7) == 0))
  {
    //rtsp:// stream
    //open stream
    LogDebug("CTsReaderFilter::Load(), open rtsp:%s", url);
    if ( !m_rtspClient.OpenStream(url)) return E_FAIL;

    m_bTimeShifting = true;
    m_bLiveTv = true;

    //are we playing a recording via RTSP
    if (strstr(url, "/stream") == NULL)
    {
      //yes, then we're not timeshifting
      m_bTimeShifting = false;
      m_bLiveTv = false;
    }

    //play
    m_buffer.Clear();
    m_buffer.Run(true);
    m_rtspClient.Play(0.0,0.0);
    m_fileReader = new CMemoryReader(m_buffer);

    //get audio /video pids
    m_demultiplexer.SetFileReader(m_fileReader);
    if (!m_demultiplexer.Start(startTimeout))
    {
      // stop streaming
      LogDebug("CTsReaderFilter::Load(), close rtsp:%s", url);
      m_buffer.Run(false);
      m_rtspClient.Stop();
      return E_FAIL;
    }
    

    // Pause. This will result in faster startup and channel change times,
    // because we don't have to SETUP a whole new session with the server.
    //m_rtspClient.Pause();

    //Update the duration of the stream
    //Note - calling m_rtspClient.OpenStream() above also updates the RTSP duration (in the RTSPClient)
    m_tickCount = GET_TIME_NOW()-m_rtspClient.Duration();
    double duration = m_rtspClient.Duration() / 1000.0f;
    CPcr pcrstart, pcrEnd, pcrMax;
    pcrstart = m_duration.StartPcr();
    duration += pcrstart.ToClock();
    pcrEnd.FromClock(duration);
    pcrstart.IsValid=true ;
    pcrEnd.IsValid=true ;
    m_duration.Set(pcrstart, pcrEnd, pcrMax);    //Load()
  }
  else
  {
    if ((length > 2) && (_strnicmp(url, "\\\\",2) == 0))
    {
      m_isUNCfile = true;
    }

    if ((length < 9) || (_strcmpi(&url[length-9], ".tsbuffer") != 0))
    {
      //local .ts file
      m_bTimeShifting = false;
      m_bLiveTv = false ;
      m_fileReader = new FileReader();
      m_fileDuration = new FileReader();
      startTimeout = m_isUNCfile ? 4000 : 2000;
    }
    else
    {
      //local timeshift buffer file file
      m_bTimeShifting = true;
      m_bLiveTv = true;
      //MultiFileReader::MultiFileReader(BOOL useFileNext, BOOL useDummyWrites, CCritSec* pFilterLock, BOOL useRandomAccess, BOOL extraLogging):
      m_fileReader   = new MultiFileReader(FALSE, m_isUNCfile, NULL, TRUE, m_bEnableBufferLogging);
      m_fileDuration = new MultiFileReader(FALSE, m_isUNCfile, NULL, TRUE, m_bEnableBufferLogging);
    }    

    //open file
    m_fileReader->SetFileName(m_fileName);
    m_fileReader->OpenFile();

    m_fileDuration->SetFileName(m_fileName);
    m_fileDuration->OpenFile();

    //detect audio/video pids
    m_demultiplexer.SetFileReader(m_fileReader);
    if (!m_demultiplexer.Start(startTimeout))
    {
      m_fileReader->CloseFile();
      m_fileDuration->CloseFile();
      return E_FAIL;
    }

    //get file duration
    m_duration.SetFileReader(m_fileDuration);
    m_duration.UpdateDuration(true, false);
    m_duration.CloseBufferFiles();
    m_bRecording = true; //Force duration thread to update

    float milli = (float)m_duration.Duration().Millisecs();
    milli /= 1000.0;
    LogDebug("CTsReaderFilter::Load(), duration - start:%x end:%x %f",
      (DWORD)m_duration.StartPcr().PcrReferenceBase, (DWORD) m_duration.EndPcr().PcrReferenceBase, milli);
    //m_fileReader->SetFilePointer(0LL, FILE_BEGIN);
  }

  if (length > 0)
  {
    LogDebug("CTsReaderFilter::Load() succeeded, file/stream: %s, isTimeshift:%d, isUNC:%d", url, m_bTimeShifting, m_isUNCfile);
  }

  //AddGraphToRot(GetFilterGraph());
  
  m_bEVRhasConnected = false;

  return S_OK;
}


STDMETHODIMP CTsReaderFilter::GetCurFile(LPOLESTR * ppszFileName,AM_MEDIA_TYPE *pmt)//
{
  CheckPointer(ppszFileName, E_POINTER);
  *ppszFileName = NULL;

  if (lstrlenW(m_fileName) > 0)
  {
    *ppszFileName = (LPOLESTR)QzTaskMemAlloc(sizeof(WCHAR) * (1 + lstrlenW(m_fileName)));
    wcscpy(*ppszFileName, m_fileName);
  }
  if(pmt)
  {
    ZeroMemory(pmt, sizeof(*pmt));
    pmt->majortype = MEDIATYPE_Stream;
    pmt->subtype = MEDIASUBTYPE_MPEG2_PROGRAM;
  }
  return S_OK;
}


double CTsReaderFilter::UpdateDuration()
{
  return 0;
}

// IAMFilterMiscFlags
ULONG CTsReaderFilter::GetMiscFlags()
{
  return AM_FILTER_MISC_FLAGS_IS_SOURCE;
}


CDeMultiplexer& CTsReaderFilter::GetDemultiplexer()
{
  return m_demultiplexer;
}

double CTsReaderFilter::GetStartTime()
{
  CAutoLock lock (&m_CritSecDuration);
  return 0;
}

///Seeks to the specified seekTime - returns 'true' if EOF is reached
bool CTsReaderFilter::Seek(CRefTime& seekTime)
{
  //are we playing a rtsp:// stream?
  if (m_fileDuration != NULL)
  {
    //no, do a seek in the local file
    double startTime = m_seekTime.Millisecs();
    double duration = m_duration.Duration().Millisecs();

    startTime /= 1000.0f;
    duration /= 1000.0f;

    LogDebug("CTsReaderFilter::  Seek-> %f/%f", startTime, duration);
    CTsFileSeek seek(m_duration);
    seek.SetFileReader(m_fileReader);
    BOOL useFileNext = m_fileReader->GetFileNext();
    m_fileReader->SetStopping(true); //Stop outstanding IO etc 
    m_fileReader->SetStopping(false);    
    bool eof = seek.Seek(seekTime);
    m_fileReader->SetFileNext(useFileNext);
    m_bRecording = true; // force a duration update soon..
    return eof;
  }
  else
  {
    //yes, we're playing a RTSP stream
    double startTime = m_seekTime.Millisecs();
    startTime /= 1000.0;
    if (m_bTimeShifting && m_bZapinProgress)
    {
      startTime = DurationUpdate(); //Force start to end of timeshift file when zapping
    }
    double milli = m_duration.Duration().Millisecs();
    milli /= 1000.0;

    //if (m_bLiveTv) startTime += 10.0; // If liveTv, it's a seek to end, force end of buffer.

    LogDebug("CTsReaderFilter:: Rtsp Seek->start client from %f/ %f",startTime,milli);

    // The RTSP server seems to ignore PLAY commands if the stream is already
    // playing, so we need to be PAUSE'd here.
    PauseRtspStreaming();

    m_buffer.Clear();
    m_buffer.Run(true);
    
    //start rtsp stream from the seek-time
  	if (m_rtspClient.Play(startTime, ((double)m_duration.Duration().Millisecs())/1000.0))
  	{
      int loop = 0;
      while (m_buffer.Size() == 0 && loop++ <= 50 ) // lets exit the loop if no data received for 5 secs.
      {
        LogDebug("CTsReaderFilter:: Rtsp Seek-->buffer empty, sleep(100ms)");
        Sleep(100);
      }
     
  	  if (loop >=50)
  	  {
        LogDebug("CTsReaderFilter:: Rtsp Seek->Play() aborted, buffer empty");
  	  }
  	}
  	else
  	{
      LogDebug("CTsReaderFilter:: Rtsp Seek->Play() failed");
  	}
    m_bRecording = true; // force a duration update soon..
  }
  return false;
}

bool CTsReaderFilter::IsFilterRunning()
{
  if (m_fileDuration != NULL) return true;
  return m_buffer.IsRunning();
}


HRESULT CTsReaderFilter::SeekPreStart(CRefTime& rtAbsSeek)
{  
  bool doSeek = true;

  REFERENCE_TIME MediaTime;
  GetMediaPosition(&MediaTime);
  
  //Are we seeking to the same place in the file ?
  bool isSamePosn = false;  
  if (MediaTime == rtAbsSeek.m_time)
  {
    //LogDebug("CTsReaderFilter::--SeekPreStart() isSamePosn"); 
    isSamePosn = true;
  }

  SetMediaPosnUpdate(rtAbsSeek.m_time) ;

  //Note that the seek timestamp (m_rtStart) is done in the range
  //from earliest - latest from GetAvailable()
  //We however would like the seek timestamp to be in the range 0-fileduration
  CRefTime rtSeek = rtAbsSeek;
  float seekTime = (float)rtSeek.Millisecs();
  seekTime /= 1000.0f;
  
  //get the earliest timestamp available in the file
  float earliesTimeStamp = 0;
  earliesTimeStamp = (float)(m_duration.StartPcr().ToClock() - m_duration.FirstStartPcr().ToClock());

  if (earliesTimeStamp < 0) earliesTimeStamp = 0;

  //correct the seek time
  seekTime -= earliesTimeStamp;
  if (seekTime < 0) seekTime = 0;

  seekTime *= 1000.0f;
  rtSeek = CRefTime((LONG)seekTime);
  // Now rtSeek contains the "relative" position from "0 to buffer/file duration"

  // Should we really seek ?
  
  // Because all skips generated after "Stop()" cause a lot of problem
  // This remove all these stupid skips. 
  if(m_State == State_Stopped)
  {   
    if ((m_bStoppedForUnexpectedSeek || (m_absSeekTime==rtAbsSeek)) && !m_bForceSeekOnStop && !m_bForceSeekAfterRateChange)
    {
      m_bStoppedForUnexpectedSeek=false ;
      m_seekTime = rtSeek ;
      m_absSeekTime = rtAbsSeek ;
      LogDebug("CTsReaderFilter::--SeekPreStart() End - Stopped"); 
      SetSeeking(false);
      return S_OK;
    }
  }

  if ((isSamePosn && !m_bForceSeekAfterRateChange) || (m_demultiplexer.IsMediaChanging() && !m_bOnZap && !m_bForceSeekOnStop && !m_bForceSeekAfterRateChange))  
  {
    doSeek = false;
    LogDebug("CTsReaderFilter::--SeekPreStart()-- No new seek %f ( Abs %f / %f ) - isSamePosn: %d, OnZap: %d, Force %d, Media changing: %d", 
		(float)rtSeek.Millisecs()/1000.0f, (float)rtAbsSeek.Millisecs()/1000.0f, (float)m_duration.EndPcr().ToClock(),isSamePosn,m_bOnZap,m_bForceSeekOnStop, m_demultiplexer.IsMediaChanging());
    m_bForceSeekOnStop = false ;
    SetSeeking(false);
    return S_OK;
  }

  LogDebug("CTsReaderFilter::--SeekPreStart()-- LiveTv : %d, TimeShifting: %d %3.3f ( Abs %f / %f )- isSamePosn: %d, OnZap: %d, Force %d, ForceRC %d, Media changing %d",
	m_bLiveTv,m_bTimeShifting,(float)rtSeek.Millisecs()/1000.0,(float)rtAbsSeek.Millisecs()/1000.0f, (float)m_duration.EndPcr().ToClock(),isSamePosn,m_bOnZap,m_bForceSeekOnStop,m_bForceSeekAfterRateChange,m_demultiplexer.IsMediaChanging());

  m_bForceSeekOnStop = false ;
  
  if (m_bForceSeekAfterRateChange)
  {
    m_bSeekAfterRcDone = true;
    m_bForceSeekAfterRateChange = false ; 
  }

  if (m_bTimeShifting)
  {
    LONG duration = m_duration.Duration().Millisecs() ;
    LONG seekTime = rtSeek.Millisecs() ;
    if (seekTime + 200 > duration) // End of timeshift buffer requested.
    {
      if (m_bLiveTv && !m_bAnalog  && (m_fileDuration != NULL)) 
      {
        doSeek=false ; // Live & not analog & not RTSP do not seek
      }
      m_bLiveTv=true ;
    }
    else
    {
      m_bLiveTv=false ;
    }

    LogDebug("Zap to File Seek : %d mS ( %f / %f ) LiveTv : %d, Seek : %d",GET_TIME_NOW()-m_lastPauseRun, (float)seekTime/1000.0f, (float)duration/1000.0f, m_bLiveTv, doSeek);
  }

  m_seekTime=rtSeek ;
  m_absSeekTime = rtAbsSeek ;

  if (!doSeek && !m_bOnZap) 
  {
    SetSeeking(false);
    LogDebug("CTsReaderFilter::--SeekPreStart() End - 0"); 
    return S_OK;
  }

  SetSeeking(true); //Just in case...normally set already by calling method

  m_demultiplexer.CallTeletextEventCallback(TELETEXT_EVENT_SEEK_START,TELETEXT_EVENTVALUE_NONE);

  // Stop threads ////

  if (GetAudioPin()->IsConnected())
  {
    //deliver a begin-flush to the codec filter so it stops asking for data
    GetAudioPin()->DeliverBeginFlush();
    GetAudioPin()->Stop();
  }
  if (GetVideoPin()->IsConnected())
  {
    //deliver a begin-flush to the codec filter so it stops asking for data
    GetVideoPin()->DeliverBeginFlush();
    GetVideoPin()->Stop();
  }

  if (!m_bOnZap || !m_demultiplexer.IsNewPatReady() || m_bAnalog) // On zapping, new PAT has occured, we should not flush to avoid loosing data.
  {                                                               //             new PAT has not occured, we should flush to avoid restart with old data.							
    //Flushing is delegated
    m_demultiplexer.DelegatedFlush(true, true);
  }
  else
  {
    m_bStreamCompensated=false ;
  }

  m_bOnZap=false ;

  //do the seek...
  if (doSeek && !m_demultiplexer.IsMediaChanging()&& !m_demultiplexer.IsAudioChanging()) 
  {
    if (!m_bTimeShifting) //It's a recording
    {
      //Check how close we are to end-of-file
      LONG minMsFromEOF = MIN_AUD_BUFF_TIME + m_regInitialBuffDelay + 3000;      
      if ((rtSeek.Millisecs() + minMsFromEOF) > m_duration.Duration().Millisecs())
      {
        //Too close to end-of-file, seek to an earlier position
        REFERENCE_TIME rollBackTime = minMsFromEOF * 10000; //hns units    
        if ((rtSeek.m_time - rollBackTime) > 0)
        {
          rtSeek.m_time -= rollBackTime;
        }
        else //very short file, so just seek to the beginning
        {
          rtSeek.m_time = 0;
        }
      }
    }

    //LogDebug("CTsReaderFilter::--SeekPreStart() Do Seek");
    for(int i(0) ; i < 4 ; i++)
    {
      bool eof = Seek(rtSeek);
      if (eof)
      {
        REFERENCE_TIME rollBackTime = m_bTimeShifting ? 5000000 : 30000000;  // 0.5s/3s      
        //reached end-of-file, try to seek to an earlier position
        if ((rtSeek.m_time - rollBackTime) > 0)
        {
          rtSeek.m_time -= rollBackTime;
        }
        else //very short file, so just seek to the beginning
        {
          rtSeek.m_time = 0;
          Seek(rtSeek);
          break;
        }
      }
      else
      {
        break; //we've succeeded
      }
    }
  }
      
  if (m_fileDuration != NULL)
  {
    if (rtSeek >= m_duration.Duration())
    {
      rtSeek=m_duration.Duration();
    }
  }

  if (GetVideoPin()->IsConnected())
  {      
    GetVideoPin()->DeliverEndFlush();
    GetVideoPin()->SetStart(m_absSeekTime) ;
    GetVideoPin()->Run();
  }

  if (GetSubtitlePin()->IsConnected())
  {
    // Update m_rtStart in case of has not seeked yet
    GetSubtitlePin()->SetStart(m_absSeekTime) ;
  }

  m_demultiplexer.CallTeletextEventCallback(TELETEXT_EVENT_SEEK_END,TELETEXT_EVENTVALUE_NONE);

  if (m_pDVBSubtitle)
  {
    m_pDVBSubtitle->SetFirstPcr(m_duration.FirstStartPcr().PcrReferenceBase);
    m_pDVBSubtitle->SeekDone(m_absSeekTime);
  }

  if (GetAudioPin()->IsConnected())
  {
    // deliver a end-flush to the codec filter so it will start asking for data again
    GetAudioPin()->DeliverEndFlush();
    GetAudioPin()->SetStart(m_absSeekTime) ;
    // and restart the thread
    GetAudioPin()->Run();
  }     
          
  LogDebug("CTsReaderFilter::--SeekPreStart() End 1"); 
  SetSeeking(false); //Unblock the pins - allow sample delivery to downstream      
        
  return S_OK;
}

// When a IMediaSeeking.SetPositions() is done on one of the output pins the output pin will do:
//  SeekStart() ->indicates to any other output pins we're busy seeking
//  Seek()      ->Does the seeking
//  SeekDone()  ->indicates that seeking has finished
// This prevents the situation where multiple outputpins are seeking in the file at the same time

///Returns the audio output pin
CAudioPin* CTsReaderFilter::GetAudioPin()
{
  return m_pAudioPin;
}
///Returns the video output pin
CVideoPin* CTsReaderFilter::GetVideoPin()
{
  return m_pVideoPin;
}
///Returns the subtitle output pin
CSubtitlePin* CTsReaderFilter::GetSubtitlePin()
{
  return m_pSubtitlePin;
}

IDVBSubtitle* CTsReaderFilter::GetSubtitleFilter()
{
  return m_pDVBSubtitle;
}

void CTsReaderFilter::ReleaseSubtitleFilter()
{
  if (m_pDVBSubtitle)
  {
    m_pDVBSubtitle->Release();
    m_pDVBSubtitle = NULL;
  }
}

//**************************************************************************************************************
/// This method is running in its own thread
/// Every second it will check the stream or local file and determine the total duration of the file/stream
/// The duration can/will grow if we are playing a timeshifting buffer/stream
//  If the duration has changed it will update m_duration and send a EC_LENGTH_CHANGED event
//  to the graph.
void CTsReaderFilter::ThreadProc()
{
  LogDebug("CTsReaderFilter::ThreadProc start(), threadID:0x%x", GetCurrentThreadId());

  int  durationUpdateLoop = 1;
  long Old_rtspDuration = -1 ;
  long PauseDuration =0;
  DWORD timeNow = GET_TIME_NOW();
  DWORD  lastPosnTime = timeNow;
  DWORD  lastDataLowTime = timeNow;
  DWORD  lastSlowPlayTime = timeNow;
  DWORD  lastDurUpdate = 0;
  DWORD  lastDurTime = timeNow - 2000;
  DWORD  pauseWaitTime = 1000;
  long   underRunLimit = 10;
  bool   longPause = true;
  int    isLiveCount = 2;
  CPcr   pcrStartLast, pcrEndLast, pcrMaxLast;
  
  REFERENCE_TIME  playSpeedAdjustInPPM = 0;
  
  DWORD fileReadLatency = 0;
  
  pcrStartLast.Reset();
  pcrEndLast.Reset();
  pcrMaxLast.Reset();

  ::SetThreadPriority(GetCurrentThread(),THREAD_PRIORITY_BELOW_NORMAL);
  do
  {
    //if demuxer reached the end of the file, we can skip the loop
    //since we're no longer playing
    if (m_demultiplexer.EndOfFile() || (State() == State_Stopped))
    {
      lastDurUpdate = 0;
      durationUpdateLoop = 1;
      if (m_bDurationThreadBusy)
      {
        LogDebug("CTsReaderFilter:: DurationThread -> idle");
      }
      m_bDurationThreadBusy = false;
      continue;
    }
    else
    {   
      if (!m_bDurationThreadBusy)
      {
        LogDebug("CTsReaderFilter:: DurationThread -> busy");
      }
      m_bDurationThreadBusy = true;
    }

    timeNow = GET_TIME_NOW();

    if (((m_MediaPos/10000)-m_absSeekTime.Millisecs()) < (10*1000))
    {
      //Shorter delay at start of play
      pauseWaitTime = 500;
      underRunLimit = 10;
      longPause = true;
    }
    else if (m_isUNCfile)
    {
      pauseWaitTime = 3000;
      underRunLimit = 10;
      longPause = false;
    }
    else
    {
      pauseWaitTime = 3000;
      underRunLimit = 10;
      longPause = false;
    }

    //Buffer underrun handling for timeshifting
    if (State() != State_Running)
    {
      lastDataLowTime = timeNow;
      _InterlockedAnd(&m_demultiplexer.m_AVDataLowCount, 0);
      _InterlockedAnd(&m_demultiplexer.m_AudioDataLowPauseTime, 0) ;
    }
    else if (m_demultiplexer.m_AVDataLowCount > underRunLimit)
    {      
      if (timeNow < (lastDataLowTime + pauseWaitTime))
      {
        //LogDebug("CTsReaderFilter:: Timeshift buffer underrun, rendering will be paused");
        m_bRenderingClockTooFast=true;
        if (timeNow < (lastDataLowTime + (pauseWaitTime/2))) //Reached trigger point in a short time
        {
          BufferingPause(true, m_demultiplexer.m_AudioDataLowPauseTime); //Force longer pause      
        }
        else
        {
          BufferingPause(longPause, m_demultiplexer.m_AudioDataLowPauseTime); //Pause for a short time         
        }
        _InterlockedAnd(&m_demultiplexer.m_AVDataLowCount, 0);
        _InterlockedAnd(&m_demultiplexer.m_AudioDataLowPauseTime, 0) ;
        m_bRenderingClockTooFast=false ;
        m_demultiplexer.m_bAudioSampleLate=false;
      }
      else
      {
        lastDataLowTime = timeNow;
        _InterlockedAnd(&m_demultiplexer.m_AVDataLowCount, 0);
        _InterlockedAnd(&m_demultiplexer.m_AudioDataLowPauseTime, 0) ;
        m_demultiplexer.m_bAudioSampleLate=false;
      }
    }
    
 
    //Update stream position - minimum 50ms between updates, nominally DUR_LOOP_TIMEOUT ms between loop iterations
    if ((State() != State_Stopped) && (((timeNow - 50) > lastPosnTime) || m_bForcePosnUpdate))
    {      
      lastPosnTime = timeNow;
      
      //Apply 'play speed' compensation adjustment when running
      if (!m_bForcePosnUpdate && (State() == State_Running))
      {
        //playSpeedAdjustInPPM (in hns units) is added to the main 'Compensation' variable,
        //to push timestamps on outgoing samples into the future(+) or past(-).
        //Since this happens approx every 0.1 sec, playSpeedAdjustInPPM 
        //is effectively in parts-per-million (micro-seconds per second)
        
        if (m_AutoSpeedAdjust > 0)
        {
          if ((timeNow - 1000) > lastSlowPlayTime) //run every 1 seconds
          {
            lastSlowPlayTime = timeNow;      
            double presToRef = GetAudioPin()->GetAudioPresToRefDiff(); //In seconds   
                    
            if (presToRef < -0.02) //slow down play
            {
              //Calculate the playSpeedAdjustInPPM value to compensate for the difference over the next 60 seconds
              //This assumes the nominal 'DeltaCompensation()' update rate is 10 per second
              playSpeedAdjustInPPM = (REFERENCE_TIME)(presToRef * ((double)(-1000*1000*DUR_LOOP_TIMEOUT)/(60.0*100.0)));              
              if (playSpeedAdjustInPPM > SPEED_ADJ_LIMIT)
              {
                playSpeedAdjustInPPM = SPEED_ADJ_LIMIT;
              }
              
              if (presToRef < -0.15)
              {
                if (!m_demultiplexer.m_bAudioSampleLate) 
                {
                  //Re-adjust the audio pin m_fAFTMeanRef value
                  m_lastPauseRun = timeNow;
                  LogDebug("CTsReaderFilter:: DurationThread : Audio to render late= %03.3f, adjusting audio pin m_fAFTMeanRef", (float)presToRef) ;
                }
                //We have lost a substantial amount of buffered data,
                //so we may need to pause playback to recover quickly
                _InterlockedExchange(&m_demultiplexer.m_AudioDataLowPauseTime, (long)((presToRef-0.02) * -1000.0));
                m_demultiplexer.m_bAudioSampleLate=true;
              }
            }
            else if ((presToRef > 0.02) && (m_AutoSpeedAdjust > 1)) //speed up play
            {
              //Calculate the playSpeedAdjustInPPM value to compensate for the difference over the next 60 seconds
              //This assumes the nominal 'DeltaCompensation()' update rate is 10 per second
              playSpeedAdjustInPPM = (REFERENCE_TIME)(presToRef * ((double)(-1000*1000*DUR_LOOP_TIMEOUT)/(60.0*100.0)));              
              if (playSpeedAdjustInPPM < -SPEED_ADJ_LIMIT)
              {
                playSpeedAdjustInPPM = -SPEED_ADJ_LIMIT;
              }
              
              if (presToRef > 0.15)
              {
                //We have gained a substantial amount of buffered data,
                //so re-adjust the audio pin m_fAFTMeanRef value
                m_lastPauseRun = timeNow;
                LogDebug("CTsReaderFilter:: DurationThread : Audio to render early= %03.3f, adjusting audio pin m_fAFTMeanRef", (float)presToRef) ;
              }
            }
            else  //We are within the +/-20ms 'dead band' so don't adjust
            {
              playSpeedAdjustInPPM = 0;
            }
          }
        }
        else //Use static value from registry (can only slow down)
        {
          playSpeedAdjustInPPM = (REFERENCE_TIME)((double)(m_regSlowPlayInPPM * DUR_LOOP_TIMEOUT)/100.0);
        }
                
        DeltaCompensation(playSpeedAdjustInPPM);
      }
      
      IMediaSeeking * ptrMediaPos = NULL;
      if (SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void**)&ptrMediaPos)))
      {
        LONGLONG currentPos;
        if (SUCCEEDED(ptrMediaPos->GetCurrentPosition(&currentPos)))
        {
          SetMediaPosnUpdate(currentPos);
          if (m_bForcePosnUpdate)
          {
            LogDebug("CTsReaderFilter:: ForcePosnUpdate: %.3f s", (float)currentPos/10000000.0f);
          }
          m_bForcePosnUpdate = false;
        }
        ptrMediaPos->Release();     
      }
    }
     
    //Execute this loop approx every second
    if (IsFilterRunning() && (State() != State_Stopped) && !m_bStopping && ((timeNow - 1000) > lastDurTime) )
    {
      lastDurTime = timeNow;
      
      //are we playing an RTSP stream?
      if (m_fileDuration!=NULL)
      {
        if (m_bTimeShifting)
        {
          isLiveCount = 2;
        }      
        //no, then get the duration from the local file
        if (m_bStreamCompensated) //Normal play started
        {          
          if((durationUpdateLoop == 2) || m_bRecording || (State()==State_Paused))
          {
            m_updateThreadDuration.SetFileReader(m_fileDuration);
            m_updateThreadDuration.SetVideoPid(m_duration.GetPid());
            { //Context for CAutoLock
              CAutoLock lock (&m_DurationThreadLock);              
              DWORD readFileTime = GET_TIME_NOW();              
              m_updateThreadDuration.UpdateDuration(false, true);
              m_updateThreadDuration.CloseBufferFiles();              
              fileReadLatency = GET_TIME_NOW() - readFileTime;    
            }
            m_bRecording = false;
            //LogDebug("CTsReaderFilter:: UpdThread duration = %.3f s", (float)m_updateThreadDuration.Duration().Millisecs()/1000.0f);
                             
            //did we find a duration?
            if (m_updateThreadDuration.Duration().Millisecs()>1)
            {
              if (m_updateThreadDuration.GetPid() > 0) //in case the PCR pid has changed
              {
                m_duration.SetVideoPid(m_updateThreadDuration.GetPid());
              }

              //yes, is it different then the one we determined last time?
              if (m_updateThreadDuration.StartPcr().PcrReferenceBase!=pcrStartLast.PcrReferenceBase ||
                  m_updateThreadDuration.EndPcr().PcrReferenceBase!=pcrEndLast.PcrReferenceBase)
              {
                //yes, then update it - we must be timeshifting or playing an in-progress recording
                m_duration.Set(m_updateThreadDuration.StartPcr(), m_updateThreadDuration.EndPcr(), m_updateThreadDuration.MaxPcr());  // Local file
                
                if (pcrStartLast.IsValid && pcrEndLast.IsValid)
                {
                  isLiveCount = 2;
                }
      
                // Is graph running?
                if ((State() != State_Stopped) && !m_bStopping)
                {
                  //LogDebug("CTsReaderFilter:: UpdThread EC_LENGTH_CHANGED 1");
                  //yes, then send a EC_LENGTH_CHANGED event to the graph
                  NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
                  //LogDebug("CTsReaderFilter:: UpdThread EC_LENGTH_CHANGED = %.3f s", (float)m_updateThreadDuration.Duration().Millisecs()/1000.0f);
                }
              }
              else //real duration is the same
              {
                if (isLiveCount > 0)
                {
                  isLiveCount--;
                }
                
                //Predicted duration might be incorrect, so we need to check/correct it
                if (m_updateThreadDuration.StartPcr().PcrReferenceBase!=m_duration.StartPcr().PcrReferenceBase ||
                    m_updateThreadDuration.EndPcr().PcrReferenceBase!=m_duration.EndPcr().PcrReferenceBase)
                {
                  //yes, then correct m_duration
                  m_duration.Set(m_updateThreadDuration.StartPcr(), m_updateThreadDuration.EndPcr(), m_updateThreadDuration.MaxPcr());  // Local file
                  
                  LogDebug("CTsReaderFilter::Duration - correction to predicted duration: %03.3f s", (float)m_updateThreadDuration.Duration().Millisecs()/1000.0f);
                            
                  // Is graph running?
                  if ((State() != State_Stopped) && !m_bStopping)
                  {
                    //yes, then send a EC_LENGTH_CHANGED event to the graph
                    NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
                  }
                }
              }

              lastDurUpdate = GET_TIME_NOW();
              pcrEndLast = m_updateThreadDuration.EndPcr();
              pcrStartLast = m_updateThreadDuration.StartPcr();   
              pcrMaxLast = m_updateThreadDuration.MaxPcr();          
            }
            else
            {
              lastDurUpdate = 0;
              m_bRecording = true; //We missed an update - force update next time
              // LogDebug("CTsReaderFilter::Duration, Update missed");
            }
            
          }
          else if (isLiveCount > 0 && lastDurUpdate > 0) // Live file - use prediction between actual file duration updates
          {           
            CPcr pcrStart, pcrEnd ;
            double start = pcrStartLast.ToClock() ;
            double end = pcrEndLast.ToClock() ; 

            end += fmin(3.5, ((double)(GET_TIME_NOW() - lastDurUpdate)/1000.0));

            //LogDebug("CTsReaderFilter::Duration, predicted start = %f, predicted end = %f", (float)start, (float)end);
            
            //set the duration
            pcrStart.FromClock(start) ;
            pcrStart.IsValid = true ;
            pcrEnd.FromClock(end);
            pcrEnd.IsValid = true ;
            m_duration.Set( pcrStart, pcrEnd, pcrMaxLast); // Continuous update
  
            // Is graph running?
            if ((State() != State_Stopped) && !m_bStopping)
            {
              //yes, then send a EC_LENGTH_CHANGED event to the graph
              NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
              // LogDebug("CTsReaderFilter::Duration, predicted end = %f", (float)end);
            }
          }
          
          if (m_bLiveTv && (State() == State_Paused))
          {
            // After 10 secs Pause, for sure, liveTv is cancelled.
            PauseDuration++ ;
            if (PauseDuration > 10)
            {
              m_bLiveTv=false;
              LogDebug("CTsReaderFilter, Live Tv is paused for more than 10 secs => m_bLiveTv=false.");
            }
          }
          else
          {
            PauseDuration=0 ;
          }
        }
        else
        {
          m_bRecording = true; //Force duration update next time m_bStreamCompensated is true
          lastDurUpdate = 0;
        }
      }
      else
      {
        // we are not playing a local file
        // we are playing a (RTSP) stream?
        if(m_bTimeShifting || m_bRecording)
        {
          if(durationUpdateLoop == 0)
          {
          	Old_rtspDuration = m_rtspClient.Duration();
            m_rtspClient.UpdateDuration();
          }
    	
          CPcr pcrStart, pcrEnd, pcrMax ;
          double duration = m_rtspClient.Duration() / 1000.0f ;
          double start = m_duration.StartPcr().ToClock() ;
          double end = m_duration.EndPcr().ToClock() ; 
          
      	  if (m_bTimeShifting)
          {
            // EndPcr is continuously increasing ( until ~26 hours for rollover that will fail ! )
            // So, we refer duration to End, and just update start.
            end = (double)(GET_TIME_NOW()-m_tickCount)/1000.0 ;
            if(durationUpdateLoop == 0)
            {
              start  = end - duration;
              if (start<0) start=0 ;
            }
  				}
  				else
  				{
            end = start + duration ;
  					if (Old_rtspDuration!=m_rtspClient.Duration())  // recording alive, continue to increase every second.
  					{
              end += (double)(durationUpdateLoop % 4) ;
  					}
            else
            {
              m_bRecording = false;
            }
  				}           
          //set the duration
          pcrStart.FromClock(start) ;
          pcrStart.IsValid = true ;
          pcrEnd.FromClock(end);
          pcrEnd.IsValid = true ;
          m_duration.Set( pcrStart, pcrEnd, pcrMax);          // Continuous update
  
  //          LogDebug("Start : %f, End : %f",(float)m_duration.StartPcr().ToClock(),(float)m_duration.EndPcr().ToClock()) ;
  
          // Is graph running?
          if ((State() != State_Stopped) && !m_bStopping)
          {
            //yes, then send a EC_LENGTH_CHANGED event to the graph
            NotifyEvent(EC_LENGTH_CHANGED, NULL, NULL);
          }
        }
      }
      
      durationUpdateLoop = (durationUpdateLoop + 1) % 4;
      
      if (durationUpdateLoop==0)
      {
        CRefTime firstAudio, lastAudio;
        CRefTime firstVideo, lastVideo, zeroVideo;
        int cntA = m_demultiplexer.GetAudioBufferPts(firstAudio, lastAudio);
        int cntV = m_demultiplexer.GetVideoBufferPts(firstVideo, lastVideo, zeroVideo);
        long rtspBuffSize = m_demultiplexer.GetRTSPBufferSize();
                
        if ((cntA > AUD_BUF_SIZE_LOG_LIM) || (cntV > VID_BUF_SIZE_LOG_LIM) || m_bEnableBufferLogging)
        {
          LogDebug("Buffers : A/V = %d/%d, RTSP = %d, MaxReadLat: %d ms, AveReadLat: %03.3f ms, DurReadLat: %d ms, A last: %03.3f, V Last: %03.3f, Comp: %.3f s, AudMean: %.3f s, AudDelta: %.3f s, SPPM: %d", 
          cntA, cntV, rtspBuffSize, 
          m_demultiplexer.GetMaxFileReadLatency(), m_demultiplexer.GetAveFileReadLatency(), fileReadLatency,
          (float)lastAudio.Millisecs()/1000.0f, (float)lastVideo.Millisecs()/1000.0f, 
          (float)Compensation.m_time/10000000, (float)GetAudioPin()->GetAudToPresMeanDelta(), 
          (float)GetAudioPin()->GetAudioPresToRefDiff(), playSpeedAdjustInPPM);
        }
      }
                        
    }

    
    Sleep(1);
  }
  while (!ThreadIsStopping(DUR_LOOP_TIMEOUT)) ;
  LogDebug("CTsReaderFilter::ThreadProc stopped()");
}


// Adds a filter graph to the Running Object Table
HRESULT CTsReaderFilter::AddGraphToRot(IUnknown *pUnkGraph)
{
  CComPtr <IMoniker>              pMoniker;
  CComPtr <IRunningObjectTable>   pROT;
  WCHAR wsz[128];
  HRESULT hr;

  if (m_dwGraphRegister!=0) return S_OK;
  if (FAILED(GetRunningObjectTable(0, &pROT)))
      return E_FAIL;

  swprintf_s(wsz, 128, L"FilterGraph %08x pid %08x\0", (DWORD_PTR) pUnkGraph, GetCurrentProcessId());
  hr = CreateItemMoniker(L"!", wsz, &pMoniker);
  if (SUCCEEDED(hr))
  {
    hr = pROT->Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, pUnkGraph, pMoniker, &m_dwGraphRegister);
  }
  return hr;
}


// Removes a filter graph from the Running Object Table
void CTsReaderFilter::RemoveGraphFromRot()
{
  if (m_dwGraphRegister==0) return;
  CComPtr <IRunningObjectTable> pROT;

  if (SUCCEEDED(GetRunningObjectTable(0, &pROT)))
      pROT->Revoke(m_dwGraphRegister);
}

/// method which implements IAMStreamSelect.Count
/// returns the number of audio streams available
STDMETHODIMP CTsReaderFilter::Count(DWORD* streamCount)
{
  *streamCount=m_demultiplexer.GetAudioStreamCount();
  return S_OK;
}

/// method which implements IAMStreamSelect.Enable
/// Sets the current audio stream to use
STDMETHODIMP CTsReaderFilter::Enable(long index, DWORD flags)
{
  bool res = m_demultiplexer.SetAudioStream((int)index);
  return S_OK;
}

/// method which implements IAMStreamSelect.Info
/// returns an array of all audio streams available
STDMETHODIMP CTsReaderFilter::Info( long lIndex,AM_MEDIA_TYPE **ppmt,DWORD *pdwFlags, LCID *plcid, DWORD *pdwGroup, WCHAR **ppszName, IUnknown **ppObject, IUnknown **ppUnk)
{
  if (pdwFlags)
  {
  int audioIndex = 0;
  m_demultiplexer.GetAudioStream(audioIndex);

    //if (m_demultiplexer.GetAudioStream()==(int)lIndex)
  if (audioIndex==(int)lIndex)
      *pdwFlags=AMSTREAMSELECTINFO_EXCLUSIVE;
    else
      *pdwFlags=0;
  }
  if (plcid) *plcid=0;
  if (pdwGroup) *pdwGroup=1;
  if (ppObject) *ppObject=NULL;
  if (ppUnk) *ppUnk=NULL;
  if (ppszName)
  {
    char szName[20];
    m_demultiplexer.GetAudioStreamInfo((int)lIndex,szName);
    *ppszName = (WCHAR *)CoTaskMemAlloc(20);
    MultiByteToWideChar(CP_ACP,0,szName,-1,*ppszName,20);
  }
  if (ppmt)
  {
    CMediaType mediaType;
    m_demultiplexer.GetAudioStreamType((int)lIndex,mediaType, GetAudioPin()->GetPMTiPosition());
    AM_MEDIA_TYPE* mType=(AM_MEDIA_TYPE*)(&mediaType);
    *ppmt=(AM_MEDIA_TYPE*)CoTaskMemAlloc(sizeof(AM_MEDIA_TYPE));
    memcpy(*ppmt, mType,sizeof(AM_MEDIA_TYPE));

    (*ppmt)->pbFormat=(BYTE*)CoTaskMemAlloc(mediaType.FormatLength());
    memcpy((*ppmt)->pbFormat,mType->pbFormat,mediaType.FormatLength());
  }
  return S_OK;
}

// IAudioStream methods
/*
STDMETHODIMP CTsReaderFilter::SetAudioStream(__int32 stream)
{
  return m_demultiplexer.SetAudioStream(stream);
}
*/
STDMETHODIMP CTsReaderFilter::GetAudioStream(__int32 &stream)
{
  return m_demultiplexer.GetAudioStream(stream);
}

// ITeletextSource methods
STDMETHODIMP CTsReaderFilter::SetTeletextTSPacketCallBack ( int (CALLBACK *pPacketCallback)(byte*, int))
{
  LogDebug("Setting Teletext TS packet callback");
  m_demultiplexer.SetTeletextPacketCallback(pPacketCallback);
  return S_OK;
}

STDMETHODIMP CTsReaderFilter::SetTeletextServiceInfoCallback( int (CALLBACK *pSICallback)(int,byte,byte,byte,byte) )
{
  LogDebug("Setting Teletext Service Info callback");
  m_demultiplexer.SetTeletextServiceInfoCallback(pSICallback);
  return S_OK;
}

STDMETHODIMP CTsReaderFilter::SetTeletextEventCallback( int (CALLBACK *pEventCallback)(int ecode,DWORD64 ev) )
{
  LogDebug("Setting Teletext Event callback");
  m_demultiplexer.SetTeletextEventCallback(pEventCallback);
  return S_OK;
}

// ISubtitleStream methods
STDMETHODIMP CTsReaderFilter::SetSubtitleStream(__int32 stream)
{
  return m_demultiplexer.SetSubtitleStream(stream);
}

STDMETHODIMP CTsReaderFilter::GetSubtitleStreamLanguage(__int32 stream,char* szLanguage)
{
  return m_demultiplexer.GetSubtitleStreamLanguage( stream, szLanguage );
}

STDMETHODIMP CTsReaderFilter::GetSubtitleStreamType(__int32 stream, int &type)
{
  return m_demultiplexer.GetSubtitleStreamType(stream, type);
}

STDMETHODIMP CTsReaderFilter::GetSubtitleStreamCount(__int32 &count)
{
  return m_demultiplexer.GetSubtitleStreamCount(count);
}

STDMETHODIMP CTsReaderFilter::GetCurrentSubtitleStream(__int32 &stream)
{
  return m_demultiplexer.GetCurrentSubtitleStream(stream);
}

STDMETHODIMP CTsReaderFilter::SetSubtitleResetCallback( int (CALLBACK *pSubUpdateCallback)(int c, void* opts, int* select)){
  //LogDebug("CTsReaderFilter SetSubtitleResetCallback");
  return m_demultiplexer.SetSubtitleResetCallback( pSubUpdateCallback );
}


HRESULT CTsReaderFilter::GetSubInfoFromPin(IPin* pPin)
{
  if (!pPin) 
  {
    m_subtitleCLSID = GUID_NULL;
    m_pDVBSubtitle = NULL;
    return S_FALSE;
  }
  CLSID clsid=GUID_NULL;
  PIN_INFO pi;
  HRESULT fhr = S_FALSE;
  
  if (SUCCEEDED(pPin->QueryPinInfo(&pi))) 
  {
    if (pi.pFilter) // IBaseFilter pointer
    {
      pi.pFilter->GetClassID(&clsid);
      m_subtitleCLSID = clsid;
 
      FILTER_INFO filterInfo;
      if (pi.pFilter->QueryFilterInfo(&filterInfo) == S_OK)
      {
        if (clsid == CLSID_DVBSub3)
        {
          fhr = pi.pFilter->QueryInterface( IID_IDVBSubtitle3, ( void**)&m_pDVBSubtitle );
          assert( fhr == S_OK);
          LogDebug("DVBSub3 interface OK");
          m_pDVBSubtitle->Test(1);
        }
        filterInfo.pGraph->Release();
      }

      pi.pFilter->Release();
    }
  }
  
  if (fhr != S_OK)
  {
    m_pDVBSubtitle = NULL;
    return S_FALSE;
  }
  return S_OK;
}


CTsDuration& CTsReaderFilter::GetDuration()
{
  return m_duration;
}

bool CTsReaderFilter::IsStreaming()
{
  return (m_fileDuration==NULL);
}

bool CTsReaderFilter::SetSeeking(bool onOff)
{
  CAutoLock lock (&m_sectionSeeking);
  
  //LogDebug("CTsReaderFilter: SetSeeking :%d", onOff);
  
  if (m_bWaitForSeek == onOff)
  {
    return false;
  }
  
  m_bWaitForSeek = onOff;
  
  if (m_bWaitForSeek)
  {
    int i = 0;
    //Wait for output pin data sample delivery to stop - timeout after 100 loop iterations in case pin delivery threads are stalled
    while ((i < 100) && (GetAudioPin()->IsInFillBuffer() || GetVideoPin()->IsInFillBuffer() || GetSubtitlePin()->IsInFillBuffer()) )
    {
      Sleep(5);
      i++;
    }
    if (i >= 100)
    {
      LogDebug("CTsReaderFilter: SetSeeking: InFillBuffer() wait timeout, %d %d %d", GetAudioPin()->IsInFillBuffer(), GetVideoPin()->IsInFillBuffer(), GetSubtitlePin()->IsInFillBuffer());
    }
  }
  return true; //state changed
}

bool CTsReaderFilter::IsSeeking()
{
  return m_bWaitForSeek;
}

bool CTsReaderFilter::IsStopping()
{
  return m_bStopping;
}

void CTsReaderFilter::SetMediaPosnUpdate(REFERENCE_TIME MediaPos)
{
  {
    CAutoLock cObjectLock(&m_GetTimeLock);
    m_MediaPos = MediaPos ;
    // m_BaseTime = (REFERENCE_TIME)GET_TIME_NOW() * 10000 ; 
    if (m_pClock)
    {
      m_pClock->GetTime(&m_BaseTime);
    }
    else
    {
      LogDebug("SetMediaPosnUpdate : m_pClock invalid");
      m_BaseTime = 0;
    }
    m_LastTime=m_BaseTime ;
  }
  //LogDebug("SetMediaPosnUpdate : %f %f",(float)MediaPos/10000,(float)m_LastTime/10000) ; 
}

void CTsReaderFilter::SetMediaPosition(REFERENCE_TIME MediaPos)
{
  //Empty method kept for backward compatibility (MP player code calls this)
  //Functionality now internal to TsReader
}

void CTsReaderFilter::BufferingPause(bool longPause, long extraSleep)
{
  // Must be called from CTsReaderFilter::ThreadProc() to allow TsReader to "Pause" itself without deadlock issue.

    if (m_bPauseOnClockTooFast)
      return ;                  // Do not re-enter !
      
    //Don't pause within 2s after a seek
    if (((m_MediaPos/10000)-m_absSeekTime.Millisecs()) < (2*1000))
    {
      return ;                  
    }

    DWORD sleepTime = 195 + (DWORD)(min(2000, max(0, extraSleep))); //Pause length in ms
    DWORD minDelayTime = 5000; //Min time between pauses in ms
    if (longPause)
    {
      minDelayTime = 500 ; //Shorter time between pauses at start of play   
    }          
    
    //Don't pause too soon after last time
    if ((GET_TIME_NOW()- m_lastPauseRun) < minDelayTime)
    {
      return ;                  
    }

    if (State() == State_Running)
    {
      m_bPauseOnClockTooFast=true ;
      IMediaControl * ptrMediaCtrl;
      if (SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaControl, (void**)&ptrMediaCtrl)))
      {
        if (m_State == State_Running)
        {
          int ACnt, VCnt;
          m_demultiplexer.GetBufferCounts(&ACnt, &VCnt);
          LogDebug("Pause %d mS renderer clock to match provider/RTSP clock, A/V = %d/%d ", sleepTime, ACnt, VCnt) ; 
          ptrMediaCtrl->Pause() ;         
          Sleep(sleepTime) ;
          //m_demultiplexer.ReadAheadFromFile(); //File read prefetch
          m_demultiplexer.m_bReadAheadFromFile = true;
          m_demultiplexer.WakeThread(); //File read prefetch
          if (m_State != State_Stopped)
          {
            ptrMediaCtrl->Run() ;
          }
        }
        ptrMediaCtrl->Release() ;
      }
      else
      {
        LogDebug("Pause failed...") ; 
      }
      m_bPauseOnClockTooFast=false ;
    }
}

void CTsReaderFilter::DeltaCompensation(REFERENCE_TIME deltaComp)
{
  if (m_bStreamCompensated)    
  {
    CAutoLock cObjectLock(&m_GetCompLock);
    Compensation.m_time -= deltaComp ; // positive deltaComp pushes timestamps into the future
    m_TotalDeltaCompensation += deltaComp;
  }
  //LogDebug("DeltaCompensation : %.3f s, %.3f s",(float)deltaComp/10000000,(float)Compensation.m_time/10000000) ; 
}

REFERENCE_TIME CTsReaderFilter::GetTotalDeltaComp()
{
  CAutoLock cObjectLock(&m_GetCompLock);
  return m_TotalDeltaCompensation;
}

void CTsReaderFilter::ClearTotalDeltaComp()
{
  CAutoLock cObjectLock(&m_GetCompLock);
  m_TotalDeltaCompensation = 0;
}

void CTsReaderFilter::SetCompensation(CRefTime newComp)
{
  {
    CAutoLock cObjectLock(&m_GetCompLock);
    Compensation = newComp ;
    m_TotalDeltaCompensation = 0;
  }
  //LogDebug("SetMediaPosnUpdate : %f %f",(float)MediaPos/10000,(float)m_LastTime/10000) ; 
}

CRefTime CTsReaderFilter::GetCompensation()
{
  {
    CAutoLock cObjectLock(&m_GetCompLock);
    return Compensation;
  }
}

void CTsReaderFilter::GetMediaPosition(REFERENCE_TIME *pMediaPos)
{
  CAutoLock cObjectLock(&m_GetTimeLock);
  REFERENCE_TIME Time=0 ;
  if (State() == State_Running)
  {
    //m_LastTime = (REFERENCE_TIME)GET_TIME_NOW() * 10000 ; 
    if (m_pClock)
    {
      m_pClock->GetTime(&m_LastTime);
    }
    else
    {
      LogDebug("GetMediaPosition : m_pClock invalid");
    }
  }
  *pMediaPos = (m_MediaPos + m_LastTime - m_BaseTime) ;
  return ; 
}

//----------------------------------------------------
// Derived from FFDShow code
CLSID CTsReaderFilter::GetCLSIDFromPin(IPin* pPin)
{
  if (!pPin) 
  {
    return GUID_NULL;
  }
  CLSID clsid=GUID_NULL;
  PIN_INFO pi;
  if (SUCCEEDED(pPin->QueryPinInfo(&pi))) 
  {
    if (pi.pFilter) // IBaseFilter pointer
    {
      pi.pFilter->GetClassID(&clsid);
      pi.pFilter->Release();
    }
  }
  return clsid;
}

void CTsReaderFilter::ReadRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  USES_CONVERSION;
  DWORD dwSize = sizeof(DWORD);
  DWORD dwType = REG_DWORD;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)&data, &dwSize);
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      LogDebug("Create default value for: %s", T2A(lpSubKey));
      WriteRegistryKeyDword(hKey, lpSubKey, data);
    }
    else
    {
      LogDebug("Fa�led to create default value for: %s", T2A(lpSubKey));
    }
  }
}

void CTsReaderFilter::WriteRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{  
  USES_CONVERSION;
  DWORD dwSize = sizeof(DWORD);
  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_DWORD, (LPBYTE)&data, dwSize);
  if (result == ERROR_SUCCESS) 
  {
    LogDebug("Success writing to Registry: %s", T2A(lpSubKey));
  } 
  else 
  {
    LogDebug("Error writing to Registry - subkey: %s error: %d", T2A(lpSubKey), result);
  }
}

LONG CTsReaderFilter::ReadOnlyRegistryKeyDword(HKEY hKey, LPCTSTR& lpSubKey, DWORD& data)
{
  USES_CONVERSION;
  DWORD dwSize = sizeof(DWORD);
  DWORD dwType = REG_DWORD;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)&data, &dwSize);
  return error;
}

void CTsReaderFilter::ReadRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{
  USES_CONVERSION;

  DWORD dwSize = MAX_REG_LENGTH;
  DWORD dwType = REG_SZ;
  LONG error = RegQueryValueEx(hKey, lpSubKey, NULL, &dwType, (PBYTE)data, &dwSize);
  
  if (error != ERROR_SUCCESS)
  {
    if (error == ERROR_FILE_NOT_FOUND)
    {
      LogDebug("   create default value for %s", T2A(lpSubKey));
      WriteRegistryKeyString(hKey, lpSubKey, data);
    }
    else if (error == ERROR_MORE_DATA)
      LogDebug("   too much data, corrupted registry setting(?):  %s", T2A(lpSubKey));
    else
      LogDebug("   error: %d subkey: %s", error, T2A(lpSubKey));
  }
}

void CTsReaderFilter::WriteRegistryKeyString(HKEY hKey, LPCTSTR& lpSubKey, LPCTSTR& data)
{  
  USES_CONVERSION;

  LONG result = RegSetValueEx(hKey, lpSubKey, 0, REG_SZ, (LPBYTE)data, _tcslen(data) * sizeof(TCHAR));
  if (result == ERROR_SUCCESS) 
    LogDebug("Success writing to Registry: %s", T2A(lpSubKey));
  else 
    LogDebug("Error writing to Registry - subkey: %s error: %d", T2A(lpSubKey), result);
}


void CTsReaderFilter::SetErrorAbort()
{
  m_demultiplexer.SetEndOfFile(true);

  if (GetAudioPin()->IsConnected())
  {
    GetAudioPin()->DeliverEndOfStream();
  }

  if (GetVideoPin()->IsConnected())
  {
    GetVideoPin()->DeliverEndOfStream();
  }
  
  NotifyEvent(EC_ERRORABORT, 0x88780078, NULL); // forces MP player to abort..."No sound driver is available for use"   
}

void CTsReaderFilter::CheckForMPAR()
{
  CComPtr<IBaseFilter> pBaseFilter;

  if (GetFilterGraph() && SUCCEEDED(GetFilterGraph()->FindFilterByName(L"MediaPortal - Audio Renderer", &pBaseFilter)))
  {
    m_bMPARinGraph = true;
    LogDebug("MPAR found");
  }  
  else if (GetFilterGraph() && SUCCEEDED(GetFilterGraph()->FindFilterByName(L"ReClock Audio Renderer", &pBaseFilter)))
  {
    m_bMPARinGraph = true;
    LogDebug("ReClock found");
  }
  else
  {
    m_bMPARinGraph = false;
    LogDebug("MPAR/Reclock not found");
  }
}

void CTsReaderFilter::PauseRtspStreaming()
{
  if (m_rtspClient.IsPaused())
  {
    return;
  }

  //query the current position, so it can resume on un-pause at this position
  //can be required in multiseat with rtsp when changing audio streams 
  IMediaSeeking * ptrMediaPos;
  if (SUCCEEDED(GetFilterGraph()->QueryInterface(IID_IMediaSeeking, (void**)&ptrMediaPos)))
  {
    ptrMediaPos->GetCurrentPosition(&m_seekTime.m_time);
    ptrMediaPos->Release();
  }
  //pause the streaming
  LogDebug("CTsReaderFilter::PauseRtspStreaming() - pause RTSP at %f", (m_seekTime.Millisecs() / 1000.0f));
  m_rtspClient.Pause();
}


////////////////////////////////////////////////////////////////////////
//
// Exported entry points for registration and unregistration
// (in this case they only call through to default implementations).
//
////////////////////////////////////////////////////////////////////////

//
// DllRegisterSever
//
// Handle the registration of this filter
//
STDAPI DllRegisterServer()
{
  return AMovieDllRegisterServer2( TRUE );
} 


//
// DllUnregisterServer
//
STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2( FALSE );
} 


//
// DllEntryPoint
//
extern "C" BOOL WINAPI DllEntryPoint(HINSTANCE, ULONG, LPVOID);

BOOL APIENTRY DllMain(HANDLE hModule,
                      DWORD  dwReason,
                      LPVOID lpReserved)
{
  return DllEntryPoint((HINSTANCE)(hModule), dwReason, lpReserved);
}

