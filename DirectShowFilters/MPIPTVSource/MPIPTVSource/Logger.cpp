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
#include "Utilities.h"

#include <stdio.h>

CLogger::CLogger()
{
  // create mutex can return NULL
  this->mutex = CreateMutex(NULL, false, _T("Global\\MPIptvMutex"));
  if (CoCreateGuid(&this->loggerInstance) != S_OK)
  {
    this->loggerInstance = GUID_NULL;
  }

  this->maxLogSize = MAX_LOG_SIZE_DEFAULT;
  this->allowedLogVerbosity = LOG_VERBOSITY_DEFAULT;

  // set maximum log size
  CParameterCollection *parameters = GetConfiguration(NULL, NULL, NULL, CONFIGURATION_SECTION_MPIPTVSOURCE);
  if (parameters != NULL)
  {
    this->maxLogSize = parameters->GetValueLong(CONFIGURATION_MAX_LOG_SIZE, true, MAX_LOG_SIZE_DEFAULT);
    this->allowedLogVerbosity = parameters->GetValueLong(CONFIGURATION_LOG_VERBOSITY, true, LOG_VERBOSITY_DEFAULT);
    delete parameters;
  }

  // check value
  this->maxLogSize = (this->maxLogSize <= 0) ? MAX_LOG_SIZE_DEFAULT : this->maxLogSize;
  this->allowedLogVerbosity = (this->allowedLogVerbosity < 0) ? LOG_VERBOSITY_DEFAULT : this->allowedLogVerbosity;
}

CLogger::~CLogger(void)
{
  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }
}

void CLogger::Log(unsigned int level, const TCHAR *format, ...)
{
  if (level <= this->allowedLogVerbosity)
  {
    SYSTEMTIME systemTime;
    GetLocalTime(&systemTime);

    va_list ap;
    va_start(ap, format);

    int length = _vsctprintf(format, ap) + 1;
    ALLOC_MEM_DEFINE_SET(buffer, TCHAR, length, 0);
    if (buffer != NULL)
    {
      _vstprintf_s(buffer, length, format, ap);
    }
    va_end(ap);

    if (this->mutex != NULL)
    {
      // wait for mutex free
      WaitForSingleObject(this->mutex, INFINITE);
    }

    TCHAR *levelBuffer = this->GetLogLevel(level);
    TCHAR *guid = ConvertGuidToString(this->loggerInstance);

    TCHAR *logRow = FormatString(_T("%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\n"),
      systemTime.wDay, systemTime.wMonth, systemTime.wYear,
      systemTime.wHour,systemTime.wMinute,systemTime.wSecond,
      systemTime.wMilliseconds,
      GetCurrentThreadId(),
      guid,
      levelBuffer,
      buffer);

    if (logRow != NULL)
    {
      // now we have log row
      // get log file
      TCHAR *fileName = GetTvServerFilePath(MPIPTVSOURCE_LOG_FILE);

      if (fileName != NULL)
      {
        LARGE_INTEGER size;
        size.QuadPart = 0;

        // open or create file
        HANDLE hLogFile = CreateFile(fileName, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

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

        if ((size.LowPart + _tcslen(logRow)) > this->maxLogSize)
        {
          // log file exceedes maximum log size
          TCHAR *moveFileName = GetTvServerFilePath(MPIPTVSOURCE_LOG_FILE_BAK);
          if (moveFileName != NULL)
          {
            // remove previous backup file
            DeleteFile(moveFileName);
            MoveFile(fileName, moveFileName);

            FREE_MEM(moveFileName);
          }
        }

        hLogFile = CreateFile(fileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
        if (hLogFile != INVALID_HANDLE_VALUE)
        {
          // move to end of log file
          LARGE_INTEGER distanceToMove;
          distanceToMove.QuadPart = 0;
          SetFilePointerEx(hLogFile, distanceToMove, NULL, FILE_END);

          // write data to log file
          DWORD written = 0;
          //WriteFile(hLogFile, logRow, (logRowLength - 1) * sizeof(TCHAR), &written, NULL);
          WriteFile(hLogFile, logRow, _tcslen(logRow) * sizeof(TCHAR), &written, NULL);

          CloseHandle(hLogFile);
          hLogFile = INVALID_HANDLE_VALUE;
        }

        FREE_MEM(fileName);
      }

      FREE_MEM(logRow);
    }

    FREE_MEM(guid);
    FREE_MEM(levelBuffer);
    FREE_MEM(buffer);

    if (this->mutex != NULL)
    {
      // release mutex
      ReleaseMutex(this->mutex);
    }
  }
}

TCHAR *CLogger::GetLogLevel(unsigned int level)
{
  switch(level)
  {
  case LOGGER_NONE:
    return FormatString(_T("         "));
  case LOGGER_ERROR:
    return FormatString(_T("[Error]  "));
  case LOGGER_WARNING:
    return FormatString(_T("[Warning]"));
  case LOGGER_INFO:
    return FormatString(_T("[Info]   "));
  case LOGGER_VERBOSE:
    return FormatString(_T("[Verbose]"));
  case LOGGER_DATA:
    return FormatString(_T("[Data]   "));
  default:
    return FormatString(_T("         "));
  }
}
