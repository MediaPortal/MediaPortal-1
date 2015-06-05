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

#include "hex.h"

#include <assert.h>
#include <stdio.h>

CLogger::CLogger(HRESULT *result, CStaticLogger *staticLogger, CParameterCollection *configuration)
{
  this->staticLogger = NULL;
  this->allowedLogVerbosity = LOGGER_NONE;
  this->loggerInstance = GUID_NULL;
  this->context = LOGGER_CONTEXT_INVALID_HANDLE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, staticLogger);
    CHECK_POINTER_DEFAULT_HRESULT(*result, configuration);

    if (SUCCEEDED(*result))
    {
      this->staticLogger = staticLogger;

      CoCreateGuid(&this->loggerInstance);
      this->SetParameters(configuration);

      CHECK_CONDITION_HRESULT(*result, this->context != LOGGER_CONTEXT_INVALID_HANDLE, *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->staticLogger->AddLoggerContextReference(this->context), *result, E_NOT_VALID_STATE);
    }
  }
}

CLogger::CLogger(HRESULT *result, CLogger *logger)
{
  this->staticLogger = NULL;
  this->allowedLogVerbosity = LOGGER_NONE;
  this->loggerInstance = GUID_NULL;
  this->context = LOGGER_CONTEXT_INVALID_HANDLE;
  
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, logger);
    CHECK_CONDITION_HRESULT(*result, logger->context != LOGGER_CONTEXT_INVALID_HANDLE, *result, E_NOT_VALID_STATE);

    if (SUCCEEDED(*result))
    {
      this->staticLogger = logger->staticLogger;
      this->allowedLogVerbosity = logger->allowedLogVerbosity;
      this->context = logger->context;

      CoCreateGuid(&this->loggerInstance);
      CHECK_CONDITION_HRESULT(*result, this->staticLogger->AddLoggerContextReference(this->context), *result, E_NOT_VALID_STATE);
    }
  }
}

CLogger::~CLogger(void)
{
  CHECK_CONDITION_EXECUTE(this->context != LOGGER_CONTEXT_INVALID_HANDLE, this->staticLogger->RemoveLoggerContextReference(this->context));
}

/* get methods */

GUID CLogger::GetLoggerInstanceId(void)
{
  return this->loggerInstance;
}

/* set methods */

void CLogger::SetParameters(CParameterCollection *configuration)
{
  if (configuration != NULL)
  {
    // set maximum log size
    unsigned int maxLogSize = configuration->GetValueUnsignedInt(PARAMETER_NAME_LOG_MAX_SIZE, true, LOG_MAX_SIZE_DEFAULT);
    this->allowedLogVerbosity = configuration->GetValueLong(PARAMETER_NAME_LOG_VERBOSITY, true, LOG_VERBOSITY_DEFAULT);

    // check value
    maxLogSize = (maxLogSize <= 0) ? LOG_MAX_SIZE_DEFAULT : maxLogSize;
    this->allowedLogVerbosity = (this->allowedLogVerbosity < 0) ? LOG_VERBOSITY_DEFAULT : this->allowedLogVerbosity;

    const wchar_t *logFile = configuration->GetValue(PARAMETER_NAME_LOG_FILE_NAME, true, NULL);

    this->context = this->staticLogger->GetLoggerContext(this->loggerInstance, maxLogSize, this->allowedLogVerbosity, logFile);
  }
}

/* other methods */

void CLogger::Log(unsigned int level, const wchar_t *format, ...)
{
  va_list vl;
  va_start(vl, format);

  this->Log(level, NULL, 0, false, format, vl);

  va_end(vl);
}

void CLogger::LogBinary(unsigned int logLevel, const unsigned char *data, unsigned int size, const wchar_t *format, ...)
{
  va_list vl;
  va_start(vl, format);

  this->Log(logLevel, data, size, true, format, vl);

  va_end(vl);
}

bool CLogger::RegisterModule(const wchar_t *moduleFileName)
{
  return (this->staticLogger != NULL) ? this->staticLogger->RegisterModule(moduleFileName) : false;
}

void CLogger::UnregisterModule(const wchar_t *moduleFileName)
{
  if (this->staticLogger != NULL)
  {
    this->staticLogger->UnregisterModule(moduleFileName);
  }
}

void CLogger::Clear(void)
{
  CHECK_CONDITION_EXECUTE(this->context != LOGGER_CONTEXT_INVALID_HANDLE, this->staticLogger->RemoveLoggerFileReference(this->context));
}

/* protected methods */

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

wchar_t *CLogger::GetLogMessage(unsigned int level, const unsigned char *data, unsigned int size, bool dataSpecified, const wchar_t *format, va_list vl)
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
  wchar_t *logRow = NULL;

  if (dataSpecified)
  {
    wchar_t *dumpData = NULL;

    if ((data != NULL) && (size != 0))
    {
      // every byte is in HEX encoding plus space
      // every 32 bytes is new line
      // add one character for null terminating character
      unsigned int dumpDataLength = size * 3 + ((size / 32) + 1) * 2 + 1;
      dumpData = ALLOC_MEM_SET(dumpData, wchar_t, dumpDataLength, 0);

      if (dumpData != NULL)
      {
        unsigned int outputPosition = 0;
        for (unsigned int i = 0; i < size; i++)
        {
          dumpData[outputPosition++] = get_charW(data[i] >> 4);
          dumpData[outputPosition++] = get_charW(data[i] & 0x0F);
          dumpData[outputPosition++] = L' ';

          if (((i % 32) == 0x1F) && (i != (size - 1)))
          {
            dumpData[outputPosition++] = L'\r';
            dumpData[outputPosition++] = L'\n';
          }
        }
      }
    }
    
    logRow = FormatString(L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\nBinary data size: %u\r\n%s\r\n",
      systemTime.wDay, systemTime.wMonth, systemTime.wYear,
      systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
      systemTime.wMilliseconds,
      GetCurrentThreadId(),
      guid,
      CLogger::GetLogLevel(level),
      buffer,
      size,
      (dumpData == NULL) ? L"NULL" : dumpData);

    FREE_MEM(dumpData);
  }
  else
  {
    logRow = FormatString(L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\n",
      systemTime.wDay, systemTime.wMonth, systemTime.wYear,
      systemTime.wHour, systemTime.wMinute, systemTime.wSecond,
      systemTime.wMilliseconds,
      GetCurrentThreadId(),
      guid,
      CLogger::GetLogLevel(level),
      buffer);
  }

  FREE_MEM(guid);
  FREE_MEM(buffer);

  return logRow;
}

void CLogger::LogMessage(unsigned int logLevel, const wchar_t *message)
{
  if ((logLevel <= this->allowedLogVerbosity) && (message != NULL) && (this->context != LOGGER_CONTEXT_INVALID_HANDLE))
  {
    this->staticLogger->LogMessage(this->context, logLevel, message);
  }
}

void CLogger::Log(unsigned int level, const unsigned char *data, unsigned int size, bool dataSpecified, const wchar_t *format, va_list vl)
{
  if (level <= this->allowedLogVerbosity)
  {
    wchar_t *logRow = this->GetLogMessage(level, data, size, dataSpecified, format, vl);

    if (logRow != NULL)
    {
      this->LogMessage(level, logRow);
      FREE_MEM(logRow);      
    }
  }
}