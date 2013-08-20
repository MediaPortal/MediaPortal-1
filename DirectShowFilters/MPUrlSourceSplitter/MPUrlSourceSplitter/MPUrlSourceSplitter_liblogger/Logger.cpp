/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "stdafx.h"

#include "Logger.h"
#include "Parameters.h"
#include "LockMutex.h"

#include <stdio.h>
#include <assert.h>

CLogger::CLogger(CParameterCollection *configuration)
{
  assert(configuration != NULL);

  this->globalMutexName = NULL;
  this->logFile = NULL;
  this->logBackupFile = NULL;
  this->mutex = NULL;

  this->SetParameters(configuration);

  if (CoCreateGuid(&this->loggerInstance) != S_OK)
  {
    this->loggerInstance = GUID_NULL;
  }
}

CLogger::CLogger(CLogger *logger)
{
  assert(logger != NULL);

  this->globalMutexName = Duplicate(logger->globalMutexName);
  this->logFile = Duplicate(logger->logFile);
  this->logBackupFile = Duplicate(logger->logBackupFile);
  this->allowedLogVerbosity = logger->allowedLogVerbosity;
  this->maxLogSize = logger->maxLogSize;

  // create mutex, can return NULL
  this->mutex = CreateMutex(NULL, false, this->globalMutexName);

  if (CoCreateGuid(&this->loggerInstance) != S_OK)
  {
    this->loggerInstance = GUID_NULL;
  }
}

CLogger::~CLogger(void)
{
  FREE_MEM(this->globalMutexName);
  FREE_MEM(this->logFile);
  FREE_MEM(this->logBackupFile);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }
}

void CLogger::SetParameters(CParameterCollection *configuration)
{
  if (configuration != NULL)
  {
    // set maximum log size
    this->maxLogSize = configuration->GetValueLong(PARAMETER_NAME_LOG_MAX_SIZE, true, LOG_MAX_SIZE_DEFAULT);
    this->allowedLogVerbosity = configuration->GetValueLong(PARAMETER_NAME_LOG_VERBOSITY, true, LOG_VERBOSITY_DEFAULT);

    // check value
    this->maxLogSize = (this->maxLogSize <= 0) ? LOG_MAX_SIZE_DEFAULT : this->maxLogSize;
    this->allowedLogVerbosity = (this->allowedLogVerbosity < 0) ? LOG_VERBOSITY_DEFAULT : this->allowedLogVerbosity;

    const wchar_t *logFile = configuration->GetValue(PARAMETER_NAME_LOG_FILE_NAME, true, NULL);
    const wchar_t *logBackupFile = configuration->GetValue(PARAMETER_NAME_LOG_BACKUP_FILE_NAME, true, NULL);
    const wchar_t *logGlobalMutexName = configuration->GetValue(PARAMETER_NAME_LOG_GLOBAL_MUTEX_NAME, true, NULL);

    CHECK_CONDITION_EXECUTE(logFile != NULL, SET_STRING(this->logFile, logFile));
    CHECK_CONDITION_EXECUTE(logBackupFile != NULL, SET_STRING(this->logBackupFile, logBackupFile));
    CHECK_CONDITION_EXECUTE(logGlobalMutexName != NULL, SET_STRING(this->globalMutexName, logGlobalMutexName));

    assert(this->logFile != NULL);
    assert(this->logBackupFile != NULL);
    assert(this->globalMutexName != NULL);
  }

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }

  if (this->globalMutexName != NULL)
  {
    // create mutex, can return NULL
    this->mutex = CreateMutex(NULL, false, globalMutexName);
  }
}

void CLogger::Log(unsigned int level, const wchar_t *format, ...)
{
  va_list vl;
  va_start(vl, format);

  this->Log(level, format, vl);

  va_end(vl);
}

const wchar_t *CLogger::GetLogLevel(unsigned int level)
{
  switch(level)
  {
  case LOGGER_NONE:
    return L"         ";
  case LOGGER_ERROR:
    return L"[Error]  ";
  case LOGGER_WARNING:
    return L"[Warning]";
  case LOGGER_INFO:
    return L"[Info]   ";
  case LOGGER_VERBOSE:
    return L"[Verbose]";
  case LOGGER_DATA:
    return L"[Data]   ";
  default:
    return L"         ";
  }
}

void CLogger::Log(unsigned int level, const wchar_t *format, va_list vl)
{
  if (level <= this->allowedLogVerbosity)
  {
    CLockMutex lock(this->mutex, INFINITE);

    wchar_t *logRow = this->GetLogMessage(level, format, vl);

    if (logRow != NULL)
    {
      this->LogMessage(level, logRow);
      FREE_MEM(logRow);      
    }
  }
}

void CLogger::LogMessage(unsigned int logLevel, const wchar_t *message)
{
  if (logLevel <= this->allowedLogVerbosity)
  {
    if (message != NULL)
    {
      // now we have log row
      if (this->logFile != NULL)
      {
        CLockMutex lock(this->mutex, INFINITE);

        LARGE_INTEGER size;
        size.QuadPart = 0;

        // open or create file
        HANDLE hLogFile = CreateFile(this->logFile, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

        if (hLogFile != INVALID_HANDLE_VALUE)
        {
          if (!GetFileSizeEx(hLogFile, &size))
          {
            // error occured while getting file size
            size.QuadPart = 0;
          }

          CloseHandle(hLogFile);
          hLogFile = INVALID_HANDLE_VALUE;
        }

        if (((size.LowPart + wcslen(message)) > this->maxLogSize) && (this->logBackupFile != NULL) )
        {
          // log file exceedes maximum log size
          DeleteFile(this->logBackupFile);
          MoveFile(this->logFile, this->logBackupFile);
        }

        hLogFile = CreateFile(this->logFile, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
        if (hLogFile != INVALID_HANDLE_VALUE)
        {
          // move to end of log file
          LARGE_INTEGER distanceToMove;
          distanceToMove.QuadPart = 0;
          SetFilePointerEx(hLogFile, distanceToMove, NULL, FILE_END);

          // write data to log file
          DWORD written = 0;
          WriteFile(hLogFile, message, wcslen(message) * sizeof(wchar_t), &written, NULL);

          CloseHandle(hLogFile);
          hLogFile = INVALID_HANDLE_VALUE;
        }
      }
    }
  }
}

wchar_t *CLogger::GetLogMessage(unsigned int level, const wchar_t *format, va_list vl)
{
  SYSTEMTIME systemTime;
  GetLocalTime(&systemTime);

  int length = _vscwprintf(format, vl) + 1;
  ALLOC_MEM_DEFINE_SET(buffer, wchar_t, length, 0);
  if (buffer != NULL)
  {
    vswprintf_s(buffer, length, format, vl);
  }

  wchar_t *guid = ConvertGuidToString(this->loggerInstance);

  wchar_t *logRow = FormatString(L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\n",
    systemTime.wDay, systemTime.wMonth, systemTime.wYear,
    systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
    systemTime.wMilliseconds,
    GetCurrentThreadId(),
    guid,
    CLogger::GetLogLevel(level),
    buffer);

  FREE_MEM(guid);
  FREE_MEM(buffer);

  return logRow;
}

//void CLogger::SetAllowedLogVerbosity(unsigned int allowedLogVerbosity)
//{
//  this->allowedLogVerbosity = allowedLogVerbosity;
//}

GUID CLogger::GetLoggerInstanceId(void)
{
  return this->loggerInstance;
}