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
#include "ParameterCollection.h"

class CStaticLogger
{
public:
  CStaticLogger(HRESULT *result);
  ~CStaticLogger(void);

  /* get methods */

  // gets static logger context collection
  // @return : static logger context collection
  CStaticLoggerContextCollection *GetLoggerContexts(void);

  // gets static logger context for specified logger
  // @return : static logger context or NULL if not found
  CStaticLoggerContext *GetLoggerContext(CLogger *logger);

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
  // @param logger : the logger
  // @param logLevel : the level of message
  // @param message : the message to log to file
  void LogMessage(CLogger *logger, unsigned int logLevel, const wchar_t *message);

  // logs message to specified logger context
  // @param context : the context to log message
  // @param logLevel : the level of message
  // @param message : the message to log to file
  void LogMessage(CStaticLoggerContext *context, unsigned int logLevel, const wchar_t *message);

  void Add(void);
  void Remove(void);
  void Flush(void);

  // registers module with specified file name
  // @param moduleFileName : the full path to module file to register
  // @return : true if successful, false otherwise
  bool RegisterModule(const wchar_t *moduleFileName);

  // unregisters module with specified file name
  // @param moduleFileName : the full path to module file to unregister
  void UnregisterModule(const wchar_t *moduleFileName);

  // tests if module with specified file name is registered
  // @param moduleFileName : the full path to module file to test
  // @return : true if module is registered, false otherwise
  bool IsRegisteredModule(const wchar_t *moduleFileName);

protected:
  CStaticLoggerContextCollection *loggerContexts;
  HANDLE mutex;
  unsigned int referencies;

  // holds registered modules
  CParameterCollection *registeredModules;

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