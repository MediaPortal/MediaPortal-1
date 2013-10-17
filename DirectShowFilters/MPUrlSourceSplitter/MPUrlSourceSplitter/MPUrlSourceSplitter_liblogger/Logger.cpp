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
#include "StaticLogger.h"

#include <assert.h>
#include <stdio.h>

CLogger::CLogger(CStaticLogger *staticLogger, CParameterCollection *configuration)
{
  assert(staticLogger != NULL);
  assert(configuration != NULL);

  this->staticLogger = staticLogger;
  this->mutex = NULL;
  this->allowedLogVerbosity = LOGGER_NONE;

  this->SetParameters(configuration);

  if (CoCreateGuid(&this->loggerInstance) != S_OK)
  {
    this->loggerInstance = GUID_NULL;
  }

  this->staticLogger->Add();
}

CLogger::CLogger(CLogger *logger)
{
  assert(logger != NULL);

  this->staticLogger = logger->staticLogger;
  this->mutex = logger->mutex;
  this->allowedLogVerbosity = logger->allowedLogVerbosity;

  if (CoCreateGuid(&this->loggerInstance) != S_OK)
  {
    this->loggerInstance = GUID_NULL;
  }

  this->staticLogger->Add();
}

CLogger::~CLogger(void)
{
  this->staticLogger->Remove();
}

void CLogger::SetParameters(CParameterCollection *configuration)
{
  if (configuration != NULL)
  {
    if (this->mutex == NULL)
    {
      // set maximum log size
      DWORD maxLogSize = configuration->GetValueLong(PARAMETER_NAME_LOG_MAX_SIZE, true, LOG_MAX_SIZE_DEFAULT);
      this->allowedLogVerbosity = configuration->GetValueLong(PARAMETER_NAME_LOG_VERBOSITY, true, LOG_VERBOSITY_DEFAULT);

      // check value
      maxLogSize = (maxLogSize <= 0) ? LOG_MAX_SIZE_DEFAULT : maxLogSize;
      this->allowedLogVerbosity = (this->allowedLogVerbosity < 0) ? LOG_VERBOSITY_DEFAULT : this->allowedLogVerbosity;

      const wchar_t *logFile = configuration->GetValue(PARAMETER_NAME_LOG_FILE_NAME, true, NULL);
      const wchar_t *logBackupFile = configuration->GetValue(PARAMETER_NAME_LOG_BACKUP_FILE_NAME, true, NULL);
      const wchar_t *logGlobalMutexName = configuration->GetValue(PARAMETER_NAME_LOG_GLOBAL_MUTEX_NAME, true, NULL);

      assert(logFile != NULL);
      assert(logBackupFile != NULL);
      assert(logGlobalMutexName != NULL);

      this->mutex = this->staticLogger->Initialize(maxLogSize, this->allowedLogVerbosity, logFile, logBackupFile, logGlobalMutexName);
    }
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
  default:
    return L"         ";
  }
}

void CLogger::Log(unsigned int level, const wchar_t *format, va_list vl)
{
  if (level <= this->allowedLogVerbosity)
  {
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
    if ((message != NULL) && (this->mutex != NULL))
    {
      this->staticLogger->LogMessage(this->mutex, logLevel, message);
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
    systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
    systemTime.wMilliseconds,
    GetCurrentThreadId(),
    guid,
    CLogger::GetLogLevel(level),
    buffer);

  FREE_MEM(guid);
  FREE_MEM(buffer);

  return logRow;
}

GUID CLogger::GetLoggerInstanceId(void)
{
  return this->loggerInstance;
}