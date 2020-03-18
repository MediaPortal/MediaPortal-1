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

#include "CrashReport.h"
#include "VersionInfo.h"
#include "ErrorCodes.h"
#include "StaticLogger.h"
#include "Logger.h"
#include "Hoster.h"
#include "Parameters.h"

#include <dbghelp.h>
#include <Shlwapi.h>
#include <Psapi.h>

extern "C++" CStaticLogger *staticLogger;

#pragma warning(push)
// disable warning: 'ApplicationInfo': was declared deprecated
// disable warning: 'HandlerSettings': was declared deprecated
#pragma warning(disable:4996)

CCrashReport::CCrashReport(HRESULT *result)
  : CFlags()
{
  this->crashReportMode = CRASH_REPORT_MODE_BASIC;
  this->maximumDumpFiles = PARAMETER_NAME_CRASH_REPORT_MAX_DUMP_FILES_DEFAULT;
  this->maximumRetainDays = PARAMETER_NAME_CRASH_REPORT_MAX_RETAIN_DAYS_DEFAULT;
  this->userName = NULL;
  this->flags |= CRASH_REPORT_FLAG_ENABLED | CRASH_REPORT_FLAG_SEND_CRASH_ENABLED;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    // create crash reporting instance

    if (FAILED(*result))
    {
      FREE_MEM(this->userName);
    }
  }
}
#pragma warning(pop)

CCrashReport::~CCrashReport()
{
  FREE_MEM(this->userName);
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CCrashReport::ChangeParameters(CParameterCollection *configuration)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, configuration);

  if (SUCCEEDED(result))
  {
    this->flags &= ~(CRASH_REPORT_FLAG_ENABLED | CRASH_REPORT_FLAG_SEND_CRASH_ENABLED);
    this->flags |= configuration->GetValueBool(PARAMETER_NAME_CRASH_REPORT, true, PARAMETER_NAME_CRASH_REPORT_DEFAULT) ? CRASH_REPORT_FLAG_ENABLED : CRASH_REPORT_FLAG_NONE;
    this->flags |= configuration->GetValueBool(PARAMETER_NAME_CRASH_REPORT_SEND_CRASH, true, PARAMETER_NAME_CRASH_REPORT_SEND_CRASH_DEFAULT) ? CRASH_REPORT_FLAG_SEND_CRASH_ENABLED : CRASH_REPORT_FLAG_NONE;

    this->crashReportMode = configuration->GetValueUnsignedInt(PARAMETER_NAME_CRASH_REPORT_MODE, true, this->crashReportMode);
    this->maximumDumpFiles = configuration->GetValueUnsignedInt(PARAMETER_NAME_CRASH_REPORT_MAX_DUMP_FILES, true, this->maximumDumpFiles);
    this->maximumRetainDays = configuration->GetValueUnsignedInt(PARAMETER_NAME_CRASH_REPORT_MAX_RETAIN_DAYS, true, this->maximumDumpFiles);

    const wchar_t *configurationUserName = configuration->GetValue(PARAMETER_NAME_CRASH_REPORT_USER_NAME, true, NULL);
    if (configurationUserName != NULL)
    {
      SET_STRING_HRESULT_WITH_NULL(this->userName, configurationUserName, result);
    }

    /*switch (this->crashReportMode)
    {
      case CRASH_REPORT_MODE_BASIC:
      {
        this->handlerSettings->FullDumpType = (MINIDUMP_TYPE)(MiniDumpNormal | MiniDumpIgnoreInaccessibleMemory);
      }
      break;
      case CRASH_REPORT_MODE_FULL:
      {
        this->handlerSettings->FullDumpType = (MINIDUMP_TYPE)(MiniDumpWithFullMemory | MiniDumpWithDataSegs | MiniDumpIgnoreInaccessibleMemory);
      }
      break;
      default:
      {
        this->handlerSettings->FullDumpType = (MINIDUMP_TYPE)(MiniDumpNormal | MiniDumpIgnoreInaccessibleMemory);
      }
      break;
    }*/

    // remove dump files out of retain period
    wchar_t *existingDumpFilePattern = GetCrashFilePath(L"*.dmp");
    CHECK_POINTER_HRESULT(result, existingDumpFilePattern, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      WIN32_FIND_DATA existingDumpFileInfo;

      HANDLE existingDumpFileHandle = FindFirstFile(existingDumpFilePattern, &existingDumpFileInfo);

      if (existingDumpFileHandle != INVALID_HANDLE_VALUE)
      {
        // last write file time is in UTC
        // current system time is in UTC, decrease current system time by retain days
        SYSTEMTIME currentSystemTime;
        FILETIME currentSystemFileTime;

        GetSystemTime(&currentSystemTime);
        CHECK_CONDITION_HRESULT(result, SystemTimeToFileTime(&currentSystemTime, &currentSystemFileTime) != 0, result, E_FAIL);

        ULARGE_INTEGER retainFileTime;
        retainFileTime.LowPart = currentSystemFileTime.dwLowDateTime;
        retainFileTime.HighPart = currentSystemFileTime.dwHighDateTime;

        retainFileTime.QuadPart -= ((ULONGLONG)HUNDRED_NANOSECONDS_IN_DAY) * ((ULONGLONG)this->maximumRetainDays);

        do
        {
          ULARGE_INTEGER fileTime;
          fileTime.LowPart = existingDumpFileInfo.ftLastWriteTime.dwLowDateTime;
          fileTime.HighPart = existingDumpFileInfo.ftLastWriteTime.dwHighDateTime;

          if (fileTime.QuadPart < retainFileTime.QuadPart)
          {
            // file is older than specified retain interval, delete file

            wchar_t *fileToDelete = GetCrashFilePath(existingDumpFileInfo.cFileName);
            CHECK_CONDITION_NOT_NULL_EXECUTE(fileToDelete, DeleteFile(fileToDelete));
            FREE_MEM(fileToDelete);
          }
        } while (FindNextFile(existingDumpFileHandle, &existingDumpFileInfo));

        FindClose(existingDumpFileHandle);
      }
    }

    FREE_MEM(existingDumpFilePattern);
  }

  return result;
}

bool CCrashReport::IsCrashReportingEnabled(void)
{
  return this->IsSetFlags(CRASH_REPORT_FLAG_ENABLED);
}

bool CCrashReport::IsSendingCrashReportEnabled(void)
{
  return this->IsSetFlags(CRASH_REPORT_FLAG_SEND_CRASH_ENABLED);
}

HRESULT CCrashReport::HandleException(struct _EXCEPTION_POINTERS *exceptionInfo)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, exceptionInfo);

  // we received some unhandled exception
  // flush logs and continue with processing exception

  // in rare cases can be static logger flushing log messages to log files
  // in that case is internal mutex locked and it will not be unlocked (because of crash)
  // if Flush(10) fails, we assume that mutex is locked and we can't add messages to static logger, also we can't flush log messages to log files
  bool canFlush = staticLogger->Flush(10);

  if (this->IsCrashReportingEnabled())
  {
    HRESULT res = S_OK;

    // exception occured in one of our registered modules
    // dump crash file

    SYSTEMTIME currentLocalTime;
    MINIDUMP_EXCEPTION_INFORMATION minidumpException;
    GetLocalTime(&currentLocalTime);

    // dump file will be created in location of crash folder

    wchar_t *dumpFileName = FormatString(L"MPUrlSourceSplitter-%04.4d-%02.2d-%02.2d-%02.2d-%02.2d-%02.2d-%03.3d.dmp",
      currentLocalTime.wYear, currentLocalTime.wMonth, currentLocalTime.wDay,
      currentLocalTime.wHour, currentLocalTime.wMinute, currentLocalTime.wSecond, currentLocalTime.wMilliseconds);
    CHECK_POINTER_HRESULT(res, dumpFileName, res, E_OUTOFMEMORY);

    wchar_t *fullDumpFileName = NULL;
    wchar_t *guid = ConvertGuidToString(GUID_NULL);
    wchar_t *message = NULL;

    CHECK_POINTER_HRESULT(res, guid, res, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(SUCCEEDED(res), fullDumpFileName = GetCrashFilePath(dumpFileName));
    CHECK_POINTER_HRESULT(res, fullDumpFileName, res, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(res), message = FormatString(L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\n",
      currentLocalTime.wDay, currentLocalTime.wMonth, currentLocalTime.wYear,
      currentLocalTime.wHour, currentLocalTime.wMinute, currentLocalTime.wSecond,
      currentLocalTime.wMilliseconds,
      GetCurrentThreadId(),
      guid,
      L"[Error]  ",
      fullDumpFileName));
    CHECK_POINTER_HRESULT(res, message, res, E_OUTOFMEMORY);

    if (SUCCEEDED(res))
    {
      HANDLE dumpFile = CreateFile(fullDumpFileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, CREATE_ALWAYS, 0, 0);
      CHECK_CONDITION_HRESULT(res, dumpFile != INVALID_HANDLE_VALUE, res, E_FAIL);

      if (SUCCEEDED(res))
      {
        minidumpException.ThreadId = GetCurrentThreadId();
        minidumpException.ExceptionPointers = exceptionInfo;
        minidumpException.ClientPointers = TRUE;

        MINIDUMP_TYPE miniDumpType = (MINIDUMP_TYPE)
          (MiniDumpWithFullMemory | MiniDumpWithDataSegs | MiniDumpIgnoreInaccessibleMemory);

        CHECK_CONDITION_HRESULT(res, MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), dumpFile, miniDumpType, &minidumpException, NULL, NULL) == TRUE, res, E_FAIL);

        CloseHandle(dumpFile);
      }
    }

    // dump logs, add files to report
    for (unsigned int i = 0; (SUCCEEDED(res) && (i < staticLogger->GetLoggerContexts()->Count())); i++)
    {
      CLoggerContext *context = staticLogger->GetLoggerContexts()->GetItem(i);

      wchar_t *loggerContextLogFile = FormatString(L"LoggerContext%02uLogFile", i);
      wchar_t *loggerContextLogBackupFile = FormatString(L"LoggerContext%02uLogBackupFile", i);

      if (context->GetLoggerFile() != NULL)
      {
        if (canFlush)
        {
          staticLogger->LogMessage(i, LOGGER_ERROR, message);
          staticLogger->Flush();
        }
      }

      FREE_MEM(loggerContextLogFile);
      FREE_MEM(loggerContextLogBackupFile);
    }

    // remove exceeding dump files from crash folder
    if (SUCCEEDED(res))
    {
      wchar_t *existingDumpFilePattern = GetCrashFilePath(L"*.dmp");
      WIN32_FIND_DATA existingDumpFileInfo;

      // newest dump files will be in beginning of array
      ALLOC_MEM_DEFINE_SET(preservingDumpFiles, FileNameAndCreationDateTime, this->maximumDumpFiles, 0);
      CHECK_POINTER_HRESULT(res, existingDumpFilePattern, res, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(res, preservingDumpFiles, res, E_OUTOFMEMORY);

      if (SUCCEEDED(res))
      {
        HANDLE existingDumpFileHandle = FindFirstFile(existingDumpFilePattern, &existingDumpFileInfo);
        CHECK_CONDITION_HRESULT(res, existingDumpFileHandle != INVALID_HANDLE_VALUE, res, E_FAIL);

        if (SUCCEEDED(res))
        {
          do
          {
            ULARGE_INTEGER creationTime;

            creationTime.LowPart = existingDumpFileInfo.ftLastWriteTime.dwLowDateTime;
            creationTime.HighPart = existingDumpFileInfo.ftLastWriteTime.dwHighDateTime;

            // find place for current dump file in preservingDumpFiles
            unsigned int insertIndex = 0;
            for (insertIndex = 0; insertIndex < this->maximumDumpFiles; insertIndex++)
            {
              if ((*(preservingDumpFiles + insertIndex)).creationDateTime.QuadPart < creationTime.QuadPart)
              {
                break;
              }
            }

            if (insertIndex >= this->maximumDumpFiles)
            {
              // delete file, surely exceeded

              wchar_t *fileToDelete = GetCrashFilePath(existingDumpFileInfo.cFileName);
              CHECK_CONDITION_NOT_NULL_EXECUTE(fileToDelete, DeleteFile(fileToDelete));
              FREE_MEM(fileToDelete);
            }
            else
            {
              // we remove last item from array
              FileNameAndCreationDateTime *file = (FileNameAndCreationDateTime *)(preservingDumpFiles + (this->maximumDumpFiles - 1));

              CHECK_CONDITION_NOT_NULL_EXECUTE(file->fileName, DeleteFile(file->fileName));
              FREE_MEM(file->fileName);

              // move everything from insertIndex till end of array
              if (insertIndex < (this->maximumDumpFiles - 1))
              {
                memmove(preservingDumpFiles + insertIndex + 1, preservingDumpFiles + insertIndex, (this->maximumDumpFiles - 1 - insertIndex) * sizeof(FileNameAndCreationDateTime));
              }

              // insert file in insertIndex
              FileNameAndCreationDateTime *insertFile = (FileNameAndCreationDateTime *)(preservingDumpFiles + insertIndex);

              insertFile->creationDateTime = creationTime;
              insertFile->fileName = GetCrashFilePath(existingDumpFileInfo.cFileName);
            }
          } while (FindNextFile(existingDumpFileHandle, &existingDumpFileInfo));

          FindClose(existingDumpFileHandle);
        }
      }

      // free allocated memory for dump files
      for (unsigned int i = 0; i < this->maximumDumpFiles; i++)
      {
        FileNameAndCreationDateTime *file = (FileNameAndCreationDateTime *)(preservingDumpFiles + i);

        FREE_MEM(file->fileName);
      }

      FREE_MEM(existingDumpFilePattern);
      FREE_MEM(preservingDumpFiles);
    }

    FREE_MEM(dumpFileName);
    FREE_MEM(fullDumpFileName);
    FREE_MEM(guid);
    FREE_MEM(message);
  }

  return result;
}

/* protected methods */