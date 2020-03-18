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

#ifndef __CRASH_REPORT
#define __CRASH_REPORT

#include "Flags.h"
#include "Version.h"
#include "ParameterCollection.h"

#define HUNDRED_NANOSECONDS_IN_DAY                                    (SECONDS_IN_DAY * 10000000ULL)

#define CRASH_REPORT_FLAG_NONE                                        FLAGS_NONE

#define CRASH_REPORT_FLAG_ENABLED                                     (1 << (FLAGS_LAST + 0))
#define CRASH_REPORT_FLAG_SEND_CRASH_ENABLED                          (1 << (FLAGS_LAST + 1))

#define CRASH_REPORT_FLAG_LAST                                        (FLAGS_LAST + 2)

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

  // handles exception
  // @param exceptionInfo : the exception info to handle
  // @return : S_OK if successful, error code otherwise
  HRESULT HandleException(struct _EXCEPTION_POINTERS *exceptionInfo);

protected:

  unsigned int crashReportMode;
  unsigned int maximumDumpFiles;
  unsigned int maximumRetainDays;
  wchar_t *userName;
 
  /* methods */

};

#endif