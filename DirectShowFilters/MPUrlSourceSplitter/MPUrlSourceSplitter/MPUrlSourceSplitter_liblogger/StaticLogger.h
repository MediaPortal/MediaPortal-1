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

#ifndef __STATIC_LOGGER_DEFINED
#define __STATIC_LOGGER_DEFINED

#include "StaticLoggerContextCollection.h"

class CStaticLogger
{
public:
  CStaticLogger(void);
  ~CStaticLogger(void);

  /* get methods */

  // gets static logger context collection
  // @return : static logger context collection
  CStaticLoggerContextCollection *GetLoggerContexts(void);

  /* set methods */

  /* other methods */

  // initializes static logger with new logger instance
  // new static logger configuration is created in case that global mutex name is not known
  // @param maxLogSize : maximum log file size
  // @param allowedLogVerbosity : allowed log verbosity
  // @param logFile : log file name with full path
  // @param logBackupFile : backup log file name with full path
  // @param globalMutexName : global mutex name
  // @return : mutex to logger instance or NULL if error
  HANDLE Initialize(DWORD maxLogSize, unsigned int allowedLogVerbosity, const wchar_t *logFile, const wchar_t *logBackupFile, const wchar_t *globalMutexName);

  // logs message to log file
  // @param mutex : the mutex to identify logger
  // @param logLevel : the level of message
  // @param message : the message to log to file
  void LogMessage(HANDLE mutex, unsigned int logLevel, const wchar_t *message);

  void Add(void);
  void Remove(void);
  void Flush(void);

protected:
  CStaticLoggerContextCollection *loggerContexts;
  HANDLE mutex;
  unsigned int referencies;

  /* logger worker thread */

  // holds logger worker thread handle
  HANDLE loggerWorkerThread;
  // specifies if logger worker thread should exit
  volatile bool loggerWorkerShouldExit;

  /* logger worker thread methods */

  // logger worker thread method
  static unsigned int WINAPI LoggerWorker(LPVOID lpParam);

  // creates logger worker thread
  // @return : S_OK if successful
  HRESULT CreateLoggerWorker(void);

  // destroys logger worker
  // @return : S_OK if successful
  HRESULT DestroyLoggerWorker(void);
};

#endif