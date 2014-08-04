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

#ifndef __LOGGER_CONTEXT_DEFINED
#define __LOGGER_CONTEXT_DEFINED

#include "Flags.h"
#include "ParameterCollection.h"
#include "LoggerFile.h"

#define LOGGER_CONTEXT_FLAG_NONE                                      FLAGS_NONE

#define LOGGER_CONTEXT_FLAG_VERBOSITY_ERROR                           (1 << (FLAGS_LAST + 0))
#define LOGGER_CONTEXT_FLAG_VERBOSITY_WARNING                         (1 << (FLAGS_LAST + 1))
#define LOGGER_CONTEXT_FLAG_VERBOSITY_INFO                            (1 << (FLAGS_LAST + 2))
#define LOGGER_CONTEXT_FLAG_VERBOSITY_VERBOSE                         (1 << (FLAGS_LAST + 3))

#define LOGGER_CONTEXT_FLAG_LAST                                      (FLAGS_LAST + 4)

class CLoggerContext : public CFlags
{
public:
  CLoggerContext(HRESULT *result, GUID guid);
  ~CLoggerContext(void);

  /* get methods */

  // gets messages to be written to file
  // @return : messages to be written to file
  CParameterCollection *GetMessages(void);

  // gets logger file associated with logger context
  // @return : logger file instance or NULL if not set
  CLoggerFile *GetLoggerFile(void);

  // gets logger context mutex for locking
  // @return : logger context mutex
  HANDLE GetMutex(void);

  // get logger GUID
  // @return : logger GUID or GUID_NULL if not set
  GUID GetLoggerGUID(void);

  /* set methods */

  // sets logger GUID
  // @param loggerGUID : the logger GUID to set
  void SetLoggerGUID(GUID loggerGUID);

  // sets allowed log verbosity
  // @param allowedLogVerbosity : the allowed log verbosity to set
  void SetAllowedLogVerbosity(unsigned int allowedLogVerbosity);

  /* other methods */

  // tests if specified log verbosity is allowed to log
  // @return : true if allowed, false otherwise
  bool IsAllowedLogVerbosity(unsigned int logVerbosity);

  // tests if logger context if free (no reference, can be used)
  // @return : true if logger context is free, false otherwise
  bool IsFree(void);

  // adds reference to logger context
  // @return : current reference count
  unsigned int AddReference(void);

  // removes reference to logger context
  // @return : current reference count
  unsigned int RemoveReference(void);

  // add logger file to logger context and increase logger file reference
  // @param loggerFile : the logger file to add
  void AddLoggerFileReference(CLoggerFile *loggerFile);

  // removes reference to logger file
  void RemoveLoggerFileReference(void);

  // clears logger context to default state
  void Clear(void);

protected:
  // holds mutex for locking logger context
  HANDLE mutex;
  // holds logger messages to be written to log file
  CParameterCollection *messages;
  // holds logger file
  CLoggerFile *loggerFile;
  // holds logger GUID
  GUID loggerGUID;
  // holds instance reference count
  unsigned int referenceCount;
};

#endif