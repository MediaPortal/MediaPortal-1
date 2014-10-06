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

#pragma once

#include "stdafx.h"

#include "MpAudioRenderer.h"
#include "Settings.h"
#include "SettingsProp.h"

#include <shlobj.h>

#include "alloctracing.h"

using namespace std;

void SetThreadName(DWORD dwThreadID, char* threadName);

const AMOVIESETUP_MEDIATYPE sudPinTypesIn[] =
{
  {&MEDIATYPE_Audio, &MEDIASUBTYPE_PCM},
  {&MEDIATYPE_Audio, &MEDIASUBTYPE_IEEE_FLOAT},
  {&MEDIATYPE_Audio, &KSDATAFORMAT_SUBTYPE_PCM},
  {&MEDIATYPE_Audio, &KSDATAFORMAT_SUBTYPE_IEEE_FLOAT},
};

const AMOVIESETUP_PIN sudpPins[] =
{
  {
    L"Input",
    TRUE,
    FALSE,
    FALSE,
    FALSE,
    &CLSID_NULL,
    NULL,
    4,
    sudPinTypesIn
  }
};

const AMOVIESETUP_FILTER sudFilter[] =
{
  {
    &__uuidof(CMPAudioRenderer),
    L"MediaPortal - Audio Renderer",
    0x30000000,
    NULL,
    sudpPins,
    CLSID_AudioRendererCategory
  }
};

CFactoryTemplate g_Templates[] =
{
  {
    sudFilter[0].strName,
    &__uuidof(CMPAudioRenderer),
    CMPAudioRenderer::CreateInstance,
    NULL,
    &sudFilter[0]
  },
  {
    L"MediaPortal Audio Renderer Properties",
    &CLSID_MPARSettingsProp,
    CSettingsProp::CreateInstance,
    NULL,
    NULL
  }
};

int g_cTemplates = sizeof(g_Templates) / sizeof(g_Templates[0]);

STDAPI DllRegisterServer()
{
  // Create the initial settings - currently disabeld as it is breaking installer for some reason

  // AudioRendererSettings settings;
  return AMovieDllRegisterServer2(TRUE);
}

STDAPI DllUnregisterServer()
{
  return AMovieDllRegisterServer2(FALSE);
}

//
// TODO: this logging code is borrowed from dshowhelper.dll
// To be replaced when MP2 has generic C++ log framework available
//

const int MAX_LOG_LINE_LENGHT = 1000;
const int LOG_LINE_RESERVED = 32;

void LogPath(TCHAR* dest, TCHAR* name)
{
  TCHAR folder[MAX_PATH];
  SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  _stprintf(dest, _T("%s\\Team Mediaportal\\MediaPortal\\log\\AudioRenderer.%s"), folder, name);
}

void LogRotate()
{
  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));
  TCHAR bakFileName[MAX_PATH];
  LogPath(bakFileName, _T("bak"));
  _tremove(bakFileName);
  // ignore if rename fails 
  (void)_trename(fileName, bakFileName);
}

CCritSec m_qLock;
CCritSec m_threadLock;

std::queue<std::string> m_logQueue;
HANDLE m_hLogger = NULL;
CAMEvent m_eLog;
CAMEvent m_eStop;

string GetLogLine()
{
  CAutoLock lock(&m_qLock);
  if (m_logQueue.size() == 0)
    return "";

  string ret = m_logQueue.front();
  m_logQueue.pop();
  return ret;
}

UINT CALLBACK LogThread(void* param)
{
  SetThreadName(0, "LoggerThread");

  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));

  HANDLE handles[3];
  handles[0] = m_eLog;
  handles[1] = m_hLogger;
  handles[2] = m_eStop;

  while (true)
  {
    DWORD result = WaitForMultipleObjects(3, handles, false, INFINITE);
    
    if (result == WAIT_OBJECT_0)
    {
      FILE* pFile = _tfopen(fileName, _T("a+"));

      string line = GetLogLine();
      while (!line.empty())
      {
        if (pFile)
          fprintf(pFile, "%s", line.c_str());
        line = GetLogLine();
      }
      if (pFile)
        fclose(pFile);
    }
    else if (result == WAIT_FAILED)
    {
      DWORD error = GetLastError();
      FILE* pFile = _tfopen(fileName, _T("a+"));
      if (pFile)
      {
        fprintf(pFile, "LoggerThread - WaitForMultipleObjects failed, result: %d error: %d\n", result, error);
        fclose(pFile);
      }
    }
    else if (result == WAIT_OBJECT_0 + 2 || result == WAIT_OBJECT_0 + 1)
      return 0;
  }

  return 0;
}

void StartLogger()
{
  CAutoLock lock(&m_threadLock);
  UINT id = 0;
  m_hLogger = (HANDLE)_beginthreadex(NULL, 0, LogThread, 0, 0, &id);
  SetThreadPriority(m_hLogger, THREAD_PRIORITY_BELOW_NORMAL);
}

void StopLogger()
{
  CAutoLock lock(&m_threadLock);

  if (m_hLogger)
  {
    m_eStop.Set();
    WaitForSingleObject(m_hLogger, INFINITE);
    m_hLogger = NULL;
  }
}

void Log(const char *fmt, ...)
{
  static CCritSec lock;
  va_list ap;
  va_start(ap, fmt);

  CAutoLock logLock(&lock);
  if (!m_hLogger)
    return;

  char buffer[MAX_LOG_LINE_LENGHT - LOG_LINE_RESERVED]; 
  int ret;
  va_start(ap, fmt);
  ret = _vsnprintf(buffer, MAX_LOG_LINE_LENGHT - LOG_LINE_RESERVED, fmt, ap);
  va_end(ap); 

  if (ret < 0)
    return;

#ifdef TRACELOGENTRY
  TRACE("%s\n", buffer);
#endif

  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  char msg[MAX_LOG_LINE_LENGHT];
  sprintf_s(msg, MAX_LOG_LINE_LENGHT,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%5x] %s\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
    systemTime.wMilliseconds,
    GetCurrentThreadId(),
    buffer);

  CAutoLock l(&m_qLock);

  m_logQueue.push((string)msg);
  m_eLog.Set();
}

const char* SubFormatToString(GUID subFormat)
{
  if (subFormat == KSDATAFORMAT_SUBTYPE_PCM)
    return "PCM";
  else if (subFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
    return "Float";
  else if (subFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL)
    return "DD";
  else if (subFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL_PLUS)
    return "DD Plus";
  else if (subFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_MLP)
    return "True HD";
  else if (subFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DTS)
    return "DTS";
  else if (subFormat == KSDATAFORMAT_SUBTYPE_IEC61937_DTS_HD)
    return "DTS-HD";
  else if (subFormat == KSDATAFORMAT_SUBTYPE_IEC61937_WMA_PRO)
    return "Windows Media Audio (WMA) Pro";
  else 
    return "Unknown";
}

void LogWaveFormat(const WAVEFORMATEXTENSIBLE* pwfx, const char* text)
{
  if (pwfx)
  {
    if (pwfx->Format.wFormatTag == WAVE_FORMAT_EXTENSIBLE)
    {
      Log("%s: %6dHz %2d (%2d)bits %2dch -- ch mask: %4d align: %2d avgbytes: %8d type: %s tag: %d", text, pwfx->Format.nSamplesPerSec,
        pwfx->Format.wBitsPerSample, pwfx->Samples.wValidBitsPerSample, pwfx->Format.nChannels, pwfx->dwChannelMask,
        pwfx->Format.nBlockAlign, pwfx->Format.nAvgBytesPerSec, SubFormatToString(pwfx->SubFormat), pwfx->Format.wFormatTag);
    }
    else
    {
      Log("%s: %6dHz %2d %2dch -- align: %2d avgbytes: %8d tag: %d", text, pwfx->Format.nSamplesPerSec,
        pwfx->Format.wBitsPerSample, pwfx->Format.nChannels, pwfx->Format.nBlockAlign, pwfx->Format.nAvgBytesPerSec, pwfx->Format.wFormatTag);
    }
  }
}

HRESULT CopyWaveFormatEx(WAVEFORMATEXTENSIBLE** dst, const WAVEFORMATEXTENSIBLE* src)
{
  if (!src)
    return S_OK;

  if (!dst)
    return E_POINTER;

  int	size = sizeof(WAVEFORMATEXTENSIBLE) + src->Format.cbSize;

  *dst = (WAVEFORMATEXTENSIBLE *)new BYTE[size];

  if (!*dst)
    return E_OUTOFMEMORY;

  memcpy(*dst, src, size);

  return S_OK;
}

HRESULT ToWaveFormatExtensible(WAVEFORMATEXTENSIBLE** dst, WAVEFORMATEX* src)
{
  if (!src)
    return S_OK;

  if (!dst)
    return E_POINTER;

  int	size = sizeof(WAVEFORMATEXTENSIBLE);
  WAVEFORMATEXTENSIBLE *pwfe = (WAVEFORMATEXTENSIBLE *)new BYTE[size];
  
  if (!pwfe)
    return E_OUTOFMEMORY;

  //ASSERT(pwf->cbSize <= sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX));
  memcpy(pwfe, src, sizeof(WAVEFORMATEX));
  pwfe->Format.cbSize = sizeof(WAVEFORMATEXTENSIBLE) - sizeof(WAVEFORMATEX);
  switch(pwfe->Format.wFormatTag)
  {
  case WAVE_FORMAT_PCM:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_PCM;
    break;
  case WAVE_FORMAT_IEEE_FLOAT:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_IEEE_FLOAT;
    break;
  case WAVE_FORMAT_DOLBY_AC3_SPDIF:
    pwfe->SubFormat = KSDATAFORMAT_SUBTYPE_IEC61937_DOLBY_DIGITAL;
    break;
  default:
    delete pwfe;
    return VFW_E_TYPE_NOT_ACCEPTED;
  }
  if (pwfe->Format.nChannels >= 1 && pwfe->Format.nChannels <= 8)
  {
    pwfe->dwChannelMask = gdwDefaultChannelMask[pwfe->Format.nChannels];
    if (pwfe->dwChannelMask == 0)
    {
      delete pwfe;
      return VFW_E_TYPE_NOT_ACCEPTED;
    }
  }
  else
  {
    delete pwfe;
    return VFW_E_TYPE_NOT_ACCEPTED;
  }

  pwfe->Samples.wValidBitsPerSample = pwfe->Format.wBitsPerSample;
  pwfe->Format.wFormatTag = WAVE_FORMAT_EXTENSIBLE;
  *dst = pwfe;

  return S_OK;
}

// http://blogs.msdn.com/b/stevejs/archive/2005/12/19/505815.aspx

#include <windows.h>
#define MS_VC_EXCEPTION 0x406D1388
#pragma pack(push,8)
typedef struct tagTHREADNAME_INFO
{
   DWORD dwType; // Must be 0x1000.
   LPCSTR szName; // Pointer to name (in user addr space).
   DWORD dwThreadID; // Thread ID (-1=caller thread).
   DWORD dwFlags; // Reserved for future use, must be zero.
} THREADNAME_INFO;

#pragma pack(pop)

void SetThreadName(DWORD dwThreadID, char* threadName)
{
   Sleep(10);
   THREADNAME_INFO info;
   info.dwType = 0x1000;
   info.szName = threadName;
   info.dwThreadID = dwThreadID;
   info.dwFlags = 0;
   __try
   {
      RaiseException( MS_VC_EXCEPTION, 0, sizeof(info)/sizeof(ULONG_PTR), (ULONG_PTR*)&info );
   }
   __except(EXCEPTION_EXECUTE_HANDLER)
   {
   }
}