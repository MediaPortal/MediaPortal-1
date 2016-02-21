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
  // holds crash reporting instance
  this->crashReporting = NULL;
  this->filterPath = NULL;
  this->dbgHelpPath = NULL;
  this->sendrptPath = NULL;
  this->applicationInfo = NULL;
  this->handlerSettings = NULL;

  this->crashReportMode = CRASH_REPORT_MODE_BASIC;
  this->maximumDumpFiles = PARAMETER_NAME_CRASH_REPORT_MAX_DUMP_FILES_DEFAULT;
  this->maximumRetainDays = PARAMETER_NAME_CRASH_REPORT_MAX_RETAIN_DAYS_DEFAULT;
  this->userName = NULL;
  this->flags |= CRASH_REPORT_FLAG_ENABLED | CRASH_REPORT_FLAG_SEND_CRASH_ENABLED;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    // create crash reporting instance

    // holds filter path, crashrpt path, dbghelp path and sendrpt path for crash reporting service
    this->filterPath = ALLOC_MEM_SET(this->filterPath, wchar_t, MAX_PATH, 0);
    ALLOC_MEM_DEFINE_SET(crashrptPath, wchar_t, MAX_PATH, 0);
    this->dbgHelpPath = ALLOC_MEM_SET(this->dbgHelpPath, wchar_t, MAX_PATH, 0);
    this->sendrptPath = ALLOC_MEM_SET(this->sendrptPath, wchar_t, MAX_PATH, 0);
    this->applicationInfo = ALLOC_MEM_SET(this->applicationInfo, ApplicationInfo, 1, 0);
    this->handlerSettings = ALLOC_MEM_SET(this->handlerSettings, HandlerSettings, 1, 0);

    CHECK_CONDITION_HRESULT(*result, this->filterPath, *result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(*result, crashrptPath, *result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(*result, this->dbgHelpPath, *result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(*result, this->sendrptPath, *result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(*result, this->applicationInfo, *result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(*result, this->handlerSettings, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(*result))
    {
      CHECK_CONDITION_HRESULT(*result, GetModuleFileName(GetModuleHandle(MODULE_FILE_NAME), this->filterPath, MAX_PATH) != 0, *result, E_CANNOT_GET_MODULE_FILE_NAME);

      if (SUCCEEDED(*result))
      {
        PathRemoveFileSpec(this->filterPath);

        wcscat_s(this->filterPath, MAX_PATH, L"\\");
        wcscpy_s(crashrptPath, MAX_PATH, this->filterPath);
        wcscpy_s(this->dbgHelpPath, MAX_PATH, this->filterPath);
        wcscpy_s(this->sendrptPath, MAX_PATH, this->filterPath);

        wcscat_s(crashrptPath, MAX_PATH, CRASHRPT_FILE_NAME);
        wcscat_s(this->dbgHelpPath, MAX_PATH, DBGHELP_FILE_NAME);
        wcscat_s(this->sendrptPath, MAX_PATH, SENDRPT_FILE_NAME);

        uint64_t buildDate = BUILD_INFO_MP_URL_SOURCE_SPLITTER - UNIX_TIMESTAMP_2000_01_01;
        buildDate /= SECONDS_IN_DAY;

        this->applicationInfo->ApplicationInfoSize = sizeof(ApplicationInfo);
        this->applicationInfo->ApplicationGUID = "44514f47-9a70-4e67-8f52-d2377e0517b8";
        this->applicationInfo->Prefix = "MPUrlSourceSplitter";
        this->applicationInfo->AppName = L"MediaPortal Url Source Splitter";
        this->applicationInfo->Company = L"Team MediaPortal";
        this->applicationInfo->V[0] = 2;
        this->applicationInfo->V[1] = 2;
        this->applicationInfo->V[2] = 11;
        this->applicationInfo->V[3] = (USHORT)buildDate;
        this->applicationInfo->Hotfix = 0;
        this->applicationInfo->PrivacyPolicyUrl = NULL;

        this->handlerSettings->HandlerSettingsSize = sizeof(HandlerSettings);
        this->handlerSettings->LeaveDumpFilesInTempFolder = FALSE;
        this->handlerSettings->OpenProblemInBrowser = FALSE;
        this->handlerSettings->UseWER = TRUE;
        this->handlerSettings->SubmitterID = 0;
        this->handlerSettings->SendAdditionalDataWithoutApproval = TRUE;
        this->handlerSettings->OverrideDefaultFullDumpType = TRUE;
        this->handlerSettings->FullDumpType = (MINIDUMP_TYPE)(MiniDumpNormal | MiniDumpIgnoreInaccessibleMemory);
        this->handlerSettings->LangFilePath = NULL;
        this->handlerSettings->SendRptPath = this->sendrptPath;
        this->handlerSettings->DbgHelpPath = this->dbgHelpPath;
        this->handlerSettings->CrashProcessingCallback = CCrashReport::HandleException;
        this->handlerSettings->CrashProcessingCallbackUserData = this;
        //handlerSettings->CustomDataCollectionSettings = NULL;

        this->crashReporting = new crash_rpt::CrashRpt(crashrptPath, this->applicationInfo, this->handlerSettings, TRUE);
        CHECK_CONDITION_HRESULT(*result, this->crashReporting, *result, E_OUTOFMEMORY);
        CHECK_CONDITION_HRESULT(*result, this->crashReporting->IsCrashHandlingEnabled(), *result, E_CANNOT_INITIALIZE_CRASH_REPORTING);

        FREE_MEM(crashrptPath);
      }
    }

    if (FAILED(*result))
    {
      FREE_MEM_CLASS(this->crashReporting);
      FREE_MEM(this->applicationInfo);
      FREE_MEM(this->handlerSettings);
      FREE_MEM(this->filterPath);
      FREE_MEM(this->dbgHelpPath);
      FREE_MEM(this->sendrptPath);
      FREE_MEM(this->userName);
    }
  }
}
#pragma warning(pop)

CCrashReport::~CCrashReport()
{
  FREE_MEM_CLASS(this->crashReporting);
  FREE_MEM(this->applicationInfo);
  FREE_MEM(this->handlerSettings);
  FREE_MEM(this->filterPath);
  FREE_MEM(this->dbgHelpPath);
  FREE_MEM(this->sendrptPath);
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

    switch (this->crashReportMode)
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
    }

    CHECK_CONDITION_HRESULT(result, this->crashReporting->InitCrashRpt(this->applicationInfo, this->handlerSettings, TRUE), result, E_CANNOT_INITIALIZE_CRASH_REPORTING);

    // remove dump files out of retain period

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < staticLogger->GetLoggerContexts()->Count())); i++)
    {
      CLoggerContext *context = staticLogger->GetLoggerContexts()->GetItem(i);

      if (context->GetLoggerFile() != NULL)
      {
        wchar_t *contextLogFilePath = Duplicate(context->GetLoggerFile()->GetLogFile());
        CHECK_POINTER_HRESULT(result, contextLogFilePath, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          PathRemoveFileSpec(contextLogFilePath);
          wchar_t *existingDumpFilePattern = FormatString(L"%s\\*.dmp", contextLogFilePath);
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

                  wchar_t *fileToDelete = FormatString(L"%s\\%s", contextLogFilePath, existingDumpFileInfo.cFileName);
                  CHECK_CONDITION_NOT_NULL_EXECUTE(fileToDelete, DeleteFile(fileToDelete));
                  FREE_MEM(fileToDelete);
                }
              } while (FindNextFile(existingDumpFileHandle, &existingDumpFileInfo));

              FindClose(existingDumpFileHandle);
            }
          }

          FREE_MEM(existingDumpFilePattern);
        }

        FREE_MEM(contextLogFilePath);
      }
    }
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

/* protected methods */

crash_rpt::CrashProcessingCallbackResult CALLBACK CCrashReport::HandleException(crash_rpt::CrashProcessingCallbackStage stage, crash_rpt::ExceptionInfo* exceptionInfo, LPVOID userData)
{
  crash_rpt::CrashProcessingCallbackResult result = crash_rpt::CrashProcessingCallbackResult::ContinueSearch;
  CCrashReport *caller = (CCrashReport *)userData;

  if (stage == crash_rpt::CrashProcessingCallbackStage::BeforeSendReport)
  {
    // we received some unhandled exception
    // flush logs and continue with processing exception

    // by ntstatus.h:

    //
    //  Values are 32 bit values laid out as follows:
    //
    //   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
    //   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
    //  +---+-+-+-----------------------+-------------------------------+
    //  |Sev|C|R|     Facility          |               Code            |
    //  +---+-+-+-----------------------+-------------------------------+
    //
    //  where
    //
    //      Sev - is the severity code
    //
    //          00 - Success
    //          01 - Informational
    //          10 - Warning
    //          11 - Error
    //
    //      C - is the Customer code flag (0 for Microsoft errors, 1 for custom errors)
    //
    //      R - is a reserved bit
    //
    //      Facility - is the facility code
    //
    //      Code - is the facility's status code
    //
    // we care only about errors
    if ((exceptionInfo != NULL) &&
        (exceptionInfo->ExceptionPointers != NULL) &&
        (exceptionInfo->ExceptionPointers->ExceptionRecord != NULL) &&
        ((exceptionInfo->ExceptionPointers->ExceptionRecord->ExceptionCode & 0xF0000000) == 0xC0000000) &&
        (staticLogger != NULL))
    {
      if (caller->IsCrashReportingEnabled())
      {
        HMODULE exceptionModule = GetModuleHandleByAddress(exceptionInfo->ExceptionPointers->ExceptionRecord->ExceptionAddress);

        if (exceptionModule != NULL)
        {
          ALLOC_MEM_DEFINE_SET(exceptionModuleFileName, wchar_t, MAX_PATH, 0);

          if (exceptionModuleFileName != NULL)
          {
            if (GetModuleFileName(exceptionModule, exceptionModuleFileName, MAX_PATH) != 0)
            {
              // we have exception module file name
              if (staticLogger->IsRegisteredModule(exceptionModuleFileName))
              {
                // exception occured in one of our registered modules
                // dump crash file

                SYSTEMTIME currentLocalTime;
                MINIDUMP_EXCEPTION_INFORMATION minidumpException;
                GetLocalTime(&currentLocalTime);

                // dump file will be created in location of first log file

                wchar_t *dumpFileName = NULL;
                wchar_t *guid = ConvertGuidToString(GUID_NULL);
                wchar_t *message = NULL;

                if (guid != NULL)
                {
                  for (unsigned int i = 0; i < staticLogger->GetLoggerContexts()->Count(); i++)
                  {
                    CLoggerContext *context = staticLogger->GetLoggerContexts()->GetItem(i);

                    if (context->GetLoggerFile() != NULL)
                    {
                      if (dumpFileName == NULL)
                      {
                        wchar_t *contextLogFilePath = Duplicate(context->GetLoggerFile()->GetLogFile());

                        if (contextLogFilePath != NULL)
                        {
                          PathRemoveFileSpec(contextLogFilePath);

                          // files with 'dmp' extension are known for Visual Studio

                          dumpFileName = FormatString(L"%s\\MPUrlSourceSplitter-%04.4d-%02.2d-%02.2d-%02.2d-%02.2d-%02.2d-%03.3d.dmp", contextLogFilePath,
                            currentLocalTime.wYear, currentLocalTime.wMonth, currentLocalTime.wDay,
                            currentLocalTime.wHour, currentLocalTime.wMinute, currentLocalTime.wSecond, currentLocalTime.wMilliseconds);

                          if (dumpFileName != NULL)
                          {
                            message = FormatString(L"%02.2d-%02.2d-%04.4d %02.2d:%02.2d:%02.2d.%03.3d [%4x] [%s] %s %s\r\n",
                              currentLocalTime.wDay, currentLocalTime.wMonth, currentLocalTime.wYear,
                              currentLocalTime.wHour, currentLocalTime.wMinute, currentLocalTime.wSecond,
                              currentLocalTime.wMilliseconds,
                              GetCurrentThreadId(),
                              guid,
                              L"[Error]  ",
                              dumpFileName);

                            if (message != NULL)
                            {
                              HANDLE dumpFile = CreateFile(dumpFileName, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, 0, CREATE_ALWAYS, 0, 0);

                              if (dumpFile != INVALID_HANDLE_VALUE)
                              {
                                minidumpException.ThreadId = GetCurrentThreadId();
                                minidumpException.ExceptionPointers = exceptionInfo->ExceptionPointers;
                                minidumpException.ClientPointers = TRUE;

                                MINIDUMP_TYPE miniDumpType = (MINIDUMP_TYPE)
                                  (MiniDumpWithFullMemory | MiniDumpWithDataSegs | MiniDumpIgnoreInaccessibleMemory);

                                if (MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), dumpFile, miniDumpType, &minidumpException, NULL, NULL) == TRUE)
                                {
                                  staticLogger->LogMessage(i, LOGGER_ERROR, message);
                                  staticLogger->Flush();

                                  // add user info to report (crash dump file name, )
                                  caller->crashReporting->AddUserInfoToReport(L"CrashDumpFileName", dumpFileName);

                                  // add log files to report (splitter or IPTV log files) 
                                  caller->crashReporting->AddFileToReport(context->GetLoggerFile()->GetLogFile(), NULL);
                                  caller->crashReporting->AddFileToReport(context->GetLoggerFile()->GetLogBackupFile(), NULL);

                                  // remove exceeding dump files from log folder

                                  wchar_t *existingDumpFilePattern = FormatString(L"%s\\*.dmp", contextLogFilePath);                                  
                                  WIN32_FIND_DATA existingDumpFileInfo;

                                  // newest dump files will be in beginning of array
                                  ALLOC_MEM_DEFINE_SET(preservingDumpFiles, FileNameAndCreationDateTime, caller->maximumDumpFiles, 0);

                                  if (existingDumpFilePattern != NULL)
                                  {
                                    HANDLE existingDumpFileHandle = FindFirstFile(existingDumpFilePattern, &existingDumpFileInfo);

                                    if (existingDumpFileHandle != INVALID_HANDLE_VALUE)
                                    {
                                      do
                                      {
                                        ULARGE_INTEGER creationTime;

                                        creationTime.LowPart = existingDumpFileInfo.ftLastWriteTime.dwLowDateTime;
                                        creationTime.HighPart = existingDumpFileInfo.ftLastWriteTime.dwHighDateTime;

                                        // find place for current dump file in preservingDumpFiles
                                        unsigned int insertIndex = 0;
                                        for (insertIndex = 0; insertIndex < caller->maximumDumpFiles; insertIndex++)
                                        {
                                          if ((*(preservingDumpFiles + insertIndex)).creationDateTime.QuadPart < creationTime.QuadPart)
                                          {
                                            break;
                                          }
                                        }

                                        if (insertIndex >= caller->maximumDumpFiles)
                                        {
                                          // delete file, surely exceeded

                                          wchar_t *fileToDelete = FormatString(L"%s\\%s", contextLogFilePath, existingDumpFileInfo.cFileName);
                                          CHECK_CONDITION_NOT_NULL_EXECUTE(fileToDelete, DeleteFile(fileToDelete));
                                          FREE_MEM(fileToDelete);
                                        }
                                        else
                                        {
                                          // we remove last item from array
                                          FileNameAndCreationDateTime *file = (FileNameAndCreationDateTime *)(preservingDumpFiles + (caller->maximumDumpFiles - 1));

                                          CHECK_CONDITION_NOT_NULL_EXECUTE(file->fileName, DeleteFile(file->fileName));
                                          FREE_MEM(file->fileName);

                                          // move everything from insertIndex till end of array
                                          if (insertIndex < (caller->maximumDumpFiles - 1))
                                          {
                                            memmove(preservingDumpFiles + insertIndex + 1, preservingDumpFiles + insertIndex, (caller->maximumDumpFiles - 1 - insertIndex) * sizeof(FileNameAndCreationDateTime));
                                          }

                                          // insert file in insertIndex
                                          FileNameAndCreationDateTime *insertFile = (FileNameAndCreationDateTime *)(preservingDumpFiles + insertIndex);

                                          insertFile->creationDateTime = creationTime;
                                          insertFile->fileName = FormatString(L"%s\\%s", contextLogFilePath, existingDumpFileInfo.cFileName);
                                        }
                                      } while (FindNextFile(existingDumpFileHandle, &existingDumpFileInfo));

                                      FindClose(existingDumpFileHandle);
                                    }
                                  }

                                  // free allocated memory for dump files
                                  for (unsigned int i = 0; i < caller->maximumDumpFiles; i++)
                                  {
                                    FileNameAndCreationDateTime *file = (FileNameAndCreationDateTime *)(preservingDumpFiles + i);

                                    FREE_MEM(file->fileName);
                                  }

                                  FREE_MEM(preservingDumpFiles);
                                  FREE_MEM(existingDumpFilePattern);
                                }

                                CloseHandle(dumpFile);
                              }
                              else
                              {
                                FREE_MEM(dumpFileName);
                                FREE_MEM(message);
                              }
                            }
                          }
                        }

                        FREE_MEM(contextLogFilePath);
                      }
                      else if (message != NULL)
                      {
                        staticLogger->LogMessage(i, LOGGER_ERROR, message);
                        staticLogger->Flush();
                      }
                    }
                  }
                }

                FREE_MEM(dumpFileName);
                FREE_MEM(guid);
                FREE_MEM(message);

                // add user name (if any)
                if (caller->userName != NULL)
                {
                  caller->crashReporting->AddUserInfoToReport(L"UserName", caller->userName);
                }
#ifdef _DEBUG
                result = crash_rpt::CrashProcessingCallbackResult::SkipSendReportReturnDefaultResult;
#else
                result = caller->IsSendingCrashReportEnabled() ? crash_rpt::CrashProcessingCallbackResult::DoDefaultActions : crash_rpt::CrashProcessingCallbackResult::SkipSendReportReturnDefaultResult;
#endif
              }
            }
          }

          FREE_MEM(exceptionModuleFileName);
        }
      }

      staticLogger->Flush();
    }
  }

  return result;
}

HMODULE CCrashReport::GetModuleHandleByAddress(LPVOID address)
{
  HMODULE *moduleArray = NULL;

  DWORD moduleArraySize = 0;
  DWORD moduleArraySizeNeeded = 0;

  if (EnumProcessModules(GetCurrentProcess(), moduleArray, moduleArraySize, &moduleArraySizeNeeded) == 0)
  {
    return NULL;
  }

  moduleArray = ALLOC_MEM_SET(moduleArray, HMODULE, (moduleArraySizeNeeded / sizeof(HMODULE)), 0);
  if (moduleArray != NULL)
  {
    moduleArraySize = moduleArraySizeNeeded;

    if (EnumProcessModules(GetCurrentProcess(), moduleArray, moduleArraySize, &moduleArraySizeNeeded) == 0)
    {
      return NULL;
    }
  }

  HMODULE result = NULL;
  unsigned int count = moduleArraySize / sizeof(HMODULE);
  for (unsigned int i = 0; i < count; i++)
  {
    MODULEINFO moduleInfo;

    if (GetModuleInformation(GetCurrentProcess(), moduleArray[i], &moduleInfo, sizeof(MODULEINFO)) == 0)
    {
      continue;
    }

    if (address < moduleInfo.lpBaseOfDll)
    {
      continue;
    }

    if ((ULONG_PTR)address >= ((ULONG_PTR)moduleInfo.lpBaseOfDll + moduleInfo.SizeOfImage))
    {
      continue;
    }

    result = (HMODULE)moduleInfo.lpBaseOfDll;
    break;
  }

  FREE_MEM(moduleArray);

  return result;
}