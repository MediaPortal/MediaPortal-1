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

#ifndef __STATIC_LOGGER_CONTEXT_DEFINED
#define __STATIC_LOGGER_CONTEXT_DEFINED

#include "ParameterCollection.h"

class CStaticLoggerContext
{
public:
  CStaticLoggerContext(void);
  ~CStaticLoggerContext(void);

  /* get methods */

  // gets mutex for accessing log file
  // @return : mutex for accessing log file
  HANDLE GetMutex(void);

  // gets maximum log file size
  // @return : maximum log file size
  DWORD GetMaxLogSize(void);

  // gets allowed verbosity (messages with higher verbosity are not logged)
  // @return : allowed verbosity
  unsigned int GetAllowedLogVerbosity(void);

  // gets log file name (with full path)
  // @return : log file name
  const wchar_t *GetLogFile(void);

  // gets backup log file name (with full path)
  // @return : backup log file name
  const wchar_t *GetLogBackupFile(void);

  // gets global mutex name
  // @return : global mutex name
  const wchar_t *GetGlobalMutexName(void);

  // gets messages to be written to file
  // @return : messages to be written to file
  CParameterCollection *GetMessages(void);

  /* set methods */

  /* other methods */

  // initializes current instance with new data
  // @param maxLogSize : maximum log file size
  // @param allowedLogVerbosity : allowed log verbosity
  // @param logFile : log file name with full path
  // @param logBackupFile : backup log file name with full path
  // @param globalMutexName : global mutex name
  // @return : true if successful, false otherwise
  bool Initialize(DWORD maxLogSize, unsigned int allowedLogVerbosity, const wchar_t *logFile, const wchar_t *logBackupFile, const wchar_t *globalMutexName);

protected:
  // mutex for accessing log file
  HANDLE mutex;
  // max log file size
  DWORD maxLogSize;
  // allowed verbosity (messages with higher verbosity are not logged)
  unsigned int allowedLogVerbosity;
  // log file name (with full path)
  wchar_t *logFile;
  // backup log file name (with full path)
  wchar_t *logBackupFile;
  // global mutex name
  wchar_t *globalMutexName;
  // holds logger messages to be written to log file
  CParameterCollection *messages;
};

#endif