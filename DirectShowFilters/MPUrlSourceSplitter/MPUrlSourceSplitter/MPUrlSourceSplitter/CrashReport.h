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

#pragma once

#ifndef __CRASH_REPORT_
#define __CRASH_REPORT

#include "Flags.h"
#include "ParameterCollection.h"
#include "CrashRpt.h"

#define UNIX_TIMESTAMP_2000_01_01                                     946684800
#define SECONDS_IN_DAY                                                86400
#define HUNDRED_NANOSECONDS_IN_DAY                                    864000000000

#define CRASHRPT_FILE_NAME                                            L"crashrpt.dll"
#define DBGHELP_FILE_NAME                                             L"dbghelp.dll"
#define SENDRPT_FILE_NAME                                             L"sendrpt.exe"

#define CRASH_REPORT_FLAG_NONE                                        FLAGS_NONE

#define CRASH_REPORT_FLAG_ENABLED                                     (1 << (FLAGS_LAST + 0))
#define CRASH_REPORT_FLAG_SEND_CRASH_ENABLED                          (1 << (FLAGS_LAST + 1))

#define CRASH_REPORT_FLAG_LAST                                        (FLAGS_LAST + 2)

#pragma warning(push)
// disable warning: 'ApplicationInfo': was declared deprecated
// disable warning: 'HandlerSettings': was declared deprecated
#pragma warning(disable:4996)

struct FileNameAndCreationDateTime
{
  wchar_t *fileName;
  ULARGE_INTEGER creationDateTime;
};

class CCrashReport : public CFlags
{
public:
  CCrashReport(HRESULT *result);
  ~CCrashReport();

  /* get methods */

  /* set methods */

  /* other methods */

  // changes crash report parameters
  // @param configuration : the collection of parameters to change crash report
  // @return : S_OK if successful, error code otherwise
  HRESULT ChangeParameters(CParameterCollection *configuration);

  // tests if crash reporting is enabled
  // @return : true if crash reporting is enabled, false otherwise
  bool IsCrashReportingEnabled(void);

  // tests if sending crash report is enabled
  // @return : true if sending crash report is enabled, false otherwise
  bool IsSendingCrashReportEnabled(void);

protected:

  // holds crash reporting instance
  crash_rpt::CrashRpt *crashReporting;

  // holds filter path, dbghelp path and sendrpt path for crash reporting service
  wchar_t *filterPath;
  wchar_t *dbgHelpPath;
  wchar_t *sendrptPath;

  ApplicationInfo *applicationInfo;
  HandlerSettings *handlerSettings;

  unsigned int crashReportMode;
  unsigned int maximumDumpFiles;
  unsigned int maximumRetainDays;
  wchar_t *userName;
 
  /* methods */

  // exception handler for crash reporting
  static crash_rpt::CrashProcessingCallbackResult CALLBACK HandleException(crash_rpt::CrashProcessingCallbackStage stage, crash_rpt::ExceptionInfo* exceptionInfo, LPVOID userData);

  // gets module handle by address
  // @param lpAddress : the address to get module
  // @return : handle to module or NULL if not found
  static HMODULE GetModuleHandleByAddress(LPVOID lpAddress);
};

#pragma warning(pop)

#endif