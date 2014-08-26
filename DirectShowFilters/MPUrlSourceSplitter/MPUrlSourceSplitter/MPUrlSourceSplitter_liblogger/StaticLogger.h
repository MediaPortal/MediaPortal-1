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

#include "LoggerFileCollection.h"
#include "LoggerContextCollection.h"
#include "ParameterCollection.h"

#define LOGGER_CONTEXT_INVALID_HANDLE                                 UINT_MAX

class CStaticLogger
{
public:
  CStaticLogger(HRESULT *result);
  ~CStaticLogger(void);

  /* get methods */

  // gets logger context collection
  // @return : logger context collection
  CLoggerContextCollection *GetLoggerContexts(void);

  // gets logger context handle for specified parameters
  // @param guid : the logger GUID
  // @param maxLogSize : the logger max log size
  // @param allowedLogVerbosity : the logger allowed log verbosity
  // @param logFile : the logger log file
  // @return : logger context handle or LOGGER_CONTEXT_INVALID_HANDLE if none
  unsigned int GetLoggerContext(GUID guid, unsigned int maxLogSize, unsigned int allowedLogVerbosity, const wchar_t *logFile);

  /* set methods */

  /* other methods */

  // logs message to specified logger context
  // @param context : the context to log message (LOGGER_CONTEXT_INVALID_HANDLE for all contexts)
  // @param logLevel : the level of message
  // @param message : the message to log to file
  void LogMessage(unsigned int context, unsigned int logLevel, const wchar_t *message);

  // flushes all logger contexts to their logger files
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

  // adds reference to logger context
  // @param context : the logger context handle to add reference
  // @return : true if successful, false otherwise
  bool AddLoggerContextReference(unsigned int context);

  // removes reference to logger context
  // @param context : the logger context handle to remove reference
  // @return : true if successful, false otherwise
  bool RemoveLoggerContextReference(unsigned int context);

  // removes reference to logger file for specified context
  // @param context : the logger context handle to remove logger file reference
  // @return : true if successful, false otherwise
  bool RemoveLoggerFileReference(unsigned int context);

protected:
  // holds logger contexts
  CLoggerContextCollection *loggerContexts;
  // holds logger files
  CLoggerFileCollection *loggerFiles;

  HANDLE mutex;

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

  // flushes specified logger context to its logger file
  // @param contextHandle : the logger context handle to flush
  HRESULT FlushContext(unsigned int contextHandle);
};

#endif