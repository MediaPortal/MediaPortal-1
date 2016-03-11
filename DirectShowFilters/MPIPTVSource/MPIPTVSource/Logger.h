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

#ifndef __LOGGER_DEFINED
#define __LOGGER_DEFINED

#include "MPIPTVSourceExports.h"

#include <tchar.h>

#define LOGGER_NONE                 0
#define LOGGER_ERROR                1
#define LOGGER_WARNING              2
#define LOGGER_INFO                 3
#define LOGGER_VERBOSE              4
#define LOGGER_DATA                 5

#define MPIPTVSOURCE_LOG_FILE       _T("log\\MPIPTVSource.log")
#define MPIPTVSOURCE_LOG_FILE_BAK   _T("log\\MPIPTVSource.bak")

class MPIPTVSOURCE_API CLogger
{
public:
  CLogger();
  ~CLogger(void);

  // log message to log file
  // @param logLevel : the log level of message
  // @param format : the formating string
  void Log(unsigned int logLevel, const TCHAR *format, ...);

  // the logger identifier
  GUID loggerInstance;
private:
  HANDLE mutex;

  static TCHAR *GetLogLevel(unsigned int level);

  DWORD maxLogSize;
  unsigned int allowedLogVerbosity;
};

#endif
