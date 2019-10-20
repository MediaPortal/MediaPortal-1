// Copyright (C) 2005-2015 Team MediaPortal
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

#include "Logger.h"
#include <windows.h>
#include <shlobj.h>
#include <MMReg.h>
#include <audioclient.h>
#include "Globals.h"

Logger::Logger() :
  m_hLogger(NULL),
  m_repeat(0)
{
}

Logger::~Logger()
{
}

void Logger::LogPath(TCHAR* dest, TCHAR* name)
{
  TCHAR folder[MAX_PATH];
  SHGetSpecialFolderPath(NULL, folder, CSIDL_COMMON_APPDATA, FALSE);
  _stprintf(dest, _T("%s\\Team Mediaportal\\MediaPortal\\log\\AudioRenderer.%s"), folder, name);
}

void Logger::LogRotate()
{
  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));
  TCHAR bakFileName[MAX_PATH];
  LogPath(bakFileName, _T("bak"));
  _tremove(bakFileName);
  // ignore if rename fails
  (void)_trename(fileName, bakFileName);
}

string Logger::GetLogLine()
{
  CAutoLock lock(&m_qLock);
  if (m_logQueue.size() == 0)
    return "";

  string ret = m_logQueue.front();
  m_logQueue.pop();
  return ret;
}

unsigned int WINAPI Logger::ThreadEntryPoint(void* pParameter)
{
  return ((Logger*)pParameter)->ThreadProc();
}

DWORD Logger::ThreadProc()
{
  SetThreadName(0, "MPAR-LoggerThread");

  TCHAR fileName[MAX_PATH];
  LogPath(fileName, _T("log"));

  HANDLE handles[3];
  handles[0] = m_eLog;
  //handles[1] = m_hLogger;
  handles[1] = m_eStop;

  while (true)
  {
    DWORD result = WaitForMultipleObjects(2, handles, false, INFINITE);

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

void Logger::Start()
{
  CAutoLock lock(&m_threadLock);
  UINT id = 0;
  m_eStop.Reset();
  m_hLogger = (HANDLE)_beginthreadex(NULL, 0, ThreadEntryPoint, this, 0, &id);
  SetThreadPriority(m_hLogger, THREAD_PRIORITY_BELOW_NORMAL);
}

void Logger::Stop()
{
  CAutoLock lock(&m_threadLock);

  if (m_hLogger)
  {
    m_eStop.Set();
    WaitForSingleObject(m_hLogger, INFINITE);
    m_hLogger = NULL;
  }
}

void Logger::WriteLog(const char *fmt, ...)
{
  static CCritSec lock;
  va_list ap;
  va_start(ap, fmt);

  CAutoLock logLock(&lock);
  if (!m_hLogger)
    return;

  char buffer[MAX_LOG_LINE_LENGHT - LOG_LINE_RESERVED];
  buffer[0] = '\n';
  int ret;
  va_start(ap, fmt);
  ret = _vsnprintf(buffer, MAX_LOG_LINE_LENGHT - LOG_LINE_RESERVED, fmt, ap);
  va_end(ap);

  if (ret < 0)
    return;

  if (!strcmp(buffer, m_prevLogLine))
  {
    m_repeat++;
    return;
  }

#ifdef TRACELOGENTRY
  TRACE("%s\n", buffer);
#endif

  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);
  char msg[MAX_LOG_LINE_LENGHT];
  sprintf_s(msg, MAX_LOG_LINE_LENGHT, "%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%5x] %s\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
    systemTime.wMilliseconds,
    GetCurrentThreadId(),
    buffer);

  CAutoLock l(&m_qLock);

  if (m_repeat > 0)
  {
    char test[MAX_LOG_LINE_LENGHT];
    sprintf_s(test, MAX_LOG_LINE_LENGHT, "   line repeated %d times\n", m_repeat);
    m_logQueue.push(test);

    m_repeat = 0;
  }

  strncpy_s(m_prevLogLine, buffer, _TRUNCATE);

  m_logQueue.push((string)msg);
  m_eLog.Set();
}

const char* Logger::SubFormatToString(GUID subFormat)
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

void Logger::WriteWaveFormat(const WAVEFORMATEXTENSIBLE* pwfx, const char* text)
{
  if (pwfx)
  {
    if (pwfx->Format.wFormatTag == WAVE_FORMAT_EXTENSIBLE)
    {
      WriteLog("%s: %6dHz %2d (%2d)bits %2dch -- ch mask: %4d align: %2d avgbytes: %8d type: %s tag: %d", text, pwfx->Format.nSamplesPerSec,
        pwfx->Format.wBitsPerSample, pwfx->Samples.wValidBitsPerSample, pwfx->Format.nChannels, pwfx->dwChannelMask,
        pwfx->Format.nBlockAlign, pwfx->Format.nAvgBytesPerSec, SubFormatToString(pwfx->SubFormat), pwfx->Format.wFormatTag);
    }
    else
    {
      WriteLog("%s: %6dHz %2d %2dch -- align: %2d avgbytes: %8d tag: %d", text, pwfx->Format.nSamplesPerSec,
        pwfx->Format.wBitsPerSample, pwfx->Format.nChannels, pwfx->Format.nBlockAlign, pwfx->Format.nAvgBytesPerSec, pwfx->Format.wFormatTag);
    }
  }
}