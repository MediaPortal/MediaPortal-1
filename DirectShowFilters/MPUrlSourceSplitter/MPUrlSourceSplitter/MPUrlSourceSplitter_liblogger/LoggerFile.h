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

#ifndef __LOGGER_FILE_DEFINED
#define __LOGGER_FILE_DEFINED

class CLoggerFile
{
public:
  CLoggerFile(HRESULT *result, const wchar_t *logFile, unsigned int maxLogSize);
  ~CLoggerFile(void);

  /* get methods */

  // gets log file name (with full path)
  // @return : log file name
  const wchar_t *GetLogFile(void);

  // gets backup log file name (with full path)
  // @return : backup log file name
  const wchar_t *GetLogBackupFile(void);

  // gets log global mutex name
  // @return : log global mutex name or NULL if error
  const wchar_t *GetLogGlobalMutexName(void);

  // gets log mutex to lock access to log file
  // @return : log mutex or NULL if error
  HANDLE GetLogMutex(void);

  // gets maximum log file size
  // @return : maximum log file size
  unsigned int GetMaxLogSize(void);

  /* set methods */

  /* other methods */

  // adds reference to logger file
  void AddReference(void);

  // removes reference to logger file
  void RemoveReference(void);

protected:
  // mutex for accessing log file
  HANDLE mutex;
  // max log file size
  unsigned int maxLogSize;
  // log file name (with full path)
  wchar_t *logFile;
  // backup log file name (with full path)
  wchar_t *logBackupFile;
  // holds global mutex name
  wchar_t *globalMutexName;
  // holds instance reference count
  unsigned int referenceCount;

  /* methods */

  // creates global mutex name for current log file name
  // @return : S_OK if successfully created, error code otherwise
  HRESULT CreateGlobalMutexName(void);
};

#endif