// Copyright (C) 2005-2010 Team MediaPortal
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

#include <atlbase.h>
#include <shlobj.h>
#include <queue>

#include "MpAudioRenderer.h"

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
  {L"Input", 
  TRUE, 
  FALSE, 
  FALSE, 
  FALSE, 
  &CLSID_NULL, 
  NULL, 
  4, 
  sudPinTypesIn},
};

const AMOVIESETUP_FILTER sudFilter[] =
{
	{&__uuidof(CMPAudioRenderer), 
  L"MediaPortal - Audio Renderer", 
  0x30000000, 
  NULL,
  sudpPins},
};

CFactoryTemplate g_Templates[] =
{
  {sudFilter[0].strName, 
  &__uuidof(CMPAudioRenderer), 
  CMPAudioRenderer::CreateInstance, 
  NULL, 
  &sudFilter[0]},
};

int g_cTemplates = 1;

STDAPI DllRegisterServer()
{
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

void LogPath(char* dest, char* name)
{
  TCHAR folder[MAX_PATH];
  SHGetSpecialFolderPath(NULL,folder,CSIDL_COMMON_APPDATA,FALSE);
  sprintf(dest,"%s\\Team Mediaportal\\MediaPortal\\log\\AudioRenderer.%s",folder,name);
}


void LogRotate()
{
  TCHAR fileName[MAX_PATH];
  LogPath(fileName, "log");
  TCHAR bakFileName[MAX_PATH];
  LogPath(bakFileName, "bak");
  remove(bakFileName);
  // ignore if rename fails 
  (void)rename(fileName, bakFileName);
}


CCritSec m_qLock;
std::queue<std::string> m_logQueue;
BOOL m_bLoggerRunning;
HANDLE m_hLogger = NULL;

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
  SetThreadName(-1, "LoggerThread");

  TCHAR fileName[MAX_PATH];
  LogPath(fileName, "log");
  while ( m_bLoggerRunning ) {
    if ( m_logQueue.size() > 0 ) {
      FILE* fp = fopen(fileName,"a+");
      if (fp != NULL)
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
    }
    Sleep(1000);
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
  {
    m_bLoggerRunning = true;
    StartLogger();
  }
  char buffer[1000]; 
  int ret;
  va_start(ap, fmt);
  ret = vsprintf(buffer, fmt, ap);
  va_end(ap); 

  if (ret < 0)
    return;

  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  char msg[500];
  sprintf_s(msg, 500,"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%5x] %s\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
    systemTime.wMilliseconds,
    GetCurrentThreadId(),
    buffer);
  CAutoLock l(&m_qLock);
  m_logQueue.push((string)msg);
};

HRESULT __fastcall UnicodeToAnsi(LPCOLESTR pszW, LPSTR* ppszA)
{
  ULONG cbAnsi;
  ULONG cCharacters;

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
    DWORD dwError = GetLastError();
    CoTaskMemFree(*ppszA);
    *ppszA = NULL;
    return HRESULT_FROM_WIN32(dwError);
  }
  return NOERROR;
}

void LogWaveFormat(const WAVEFORMATEX* pwfx, const char *text)
{
  if (pwfx)
  {
    char type = 'u';
      
    if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
    {
      WAVEFORMATEXTENSIBLE* tmp = (WAVEFORMATEXTENSIBLE*)pwfx;

      if (tmp->SubFormat == KSDATAFORMAT_SUBTYPE_PCM)
        type = 'i';
      else if (tmp->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
        type = 'f';

      Log("EXTENSIBLE - %s: %6dHz %2d%c (%2d)bits %2dch -- ch mask: %4d align: %2d avgbytes: %8d", text, pwfx->nSamplesPerSec, 
        pwfx->wBitsPerSample, type, tmp->Samples.wValidBitsPerSample, pwfx->nChannels, tmp->dwChannelMask, pwfx->nBlockAlign, pwfx->nAvgBytesPerSec);
    }
    else
    {
      if (pwfx->wFormatTag == WAVE_FORMAT_PCM)
        type = 'i';
      else if (pwfx->wFormatTag == WAVE_FORMAT_IEEE_FLOAT)
        type = 'f';

      Log("EX         - %s: %6dHz %2d%cbits %2dch -- align: %2d avgbytes: %8d", text, pwfx->nSamplesPerSec, pwfx->wBitsPerSample,
        type, pwfx->nChannels, pwfx->nBlockAlign, pwfx->nAvgBytesPerSec);
    }
  }

  /*if (pwfx)
  {
    Log("WAVEFORMATEX - %s", text);
    Log("  nAvgBytesPerSec     %d", pwfx->nAvgBytesPerSec);
    Log("  nBlockAlign         %d", pwfx->nBlockAlign);
    Log("  nChannels           %d", pwfx->nChannels);
    Log("  nSamplesPerSec      %d", pwfx->nSamplesPerSec);
    Log("  wBitsPerSample      %d", pwfx->wBitsPerSample);
    Log("  wFormatTag          %d", pwfx->wFormatTag);

    if (pwfx->wFormatTag == WAVE_FORMAT_EXTENSIBLE)
    {
      WAVEFORMATEXTENSIBLE* tmp = (WAVEFORMATEXTENSIBLE*)pwfx;
      Log("  WAVE_FORMAT_EXTENSIBLE");
      Log("  dwChannelMask       %d", tmp->dwChannelMask);
      Log("  wValidBitsPerSample %d", tmp->Samples.wValidBitsPerSample);
      if (tmp->SubFormat == KSDATAFORMAT_SUBTYPE_PCM)
        Log("  SubFormat           PCM");
      else if (tmp->SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT)
        Log("  SubFormat           FLOAT");

      LPOLESTR str;
      LPSTR astr;
      str = (LPOLESTR)CoTaskMemAlloc(400);
      if (str)
      {
        StringFromGUID2(tmp->SubFormat, str, 200);
        UnicodeToAnsi(str, &astr);
        Log("  GUID          %s", astr);
        CoTaskMemFree(str);
      }
    }
    else if (pwfx->wFormatTag == WAVE_FORMAT_PCM)
      Log("  SubFormat           PCM");
    else if (pwfx->wFormatTag == WAVE_FORMAT_IEEE_FLOAT)
      Log("  SubFormat           FLOAT");
  }*/
}

HRESULT CopyWaveFormatEx(WAVEFORMATEX **dst, const WAVEFORMATEX *src)
{
  if (!src)
    return S_OK;

  if (!dst)
    return E_POINTER;

  int	size = sizeof(WAVEFORMATEX) + src->cbSize;

  *dst = (WAVEFORMATEX *)new BYTE[size];

  if (!*dst)
    return E_OUTOFMEMORY;

  memcpy(*dst, src, size);

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