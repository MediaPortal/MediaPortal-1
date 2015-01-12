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

#include "StdAfx.h"

#include "LoggerFile.h"

#include <Shlwapi.h>

#define GLOBAL_MUTEX_NAME_PREFIX                                      L"Global\\"

CLoggerFile::CLoggerFile(HRESULT *result, const wchar_t *logFile, unsigned int maxLogSize)
{
  this->mutex = NULL;
  this->logFile = NULL;
  this->logBackupFile = NULL;
  this->maxLogSize = maxLogSize;
  this->globalMutexName = NULL;
  this->referenceCount = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    wchar_t *backupFile = NULL;
    CHECK_POINTER_DEFAULT_HRESULT(*result, logFile);

    SET_STRING_HRESULT_WITH_NULL(this->logFile, logFile, *result);
    SET_STRING_HRESULT_WITH_NULL(backupFile, logFile, *result);

    if (SUCCEEDED(*result))
    {
      PathRemoveExtension(backupFile);
      this->logBackupFile = FormatString(L"%s.bak", backupFile);
      CHECK_POINTER_HRESULT(*result, this->logBackupFile, *result, E_OUTOFMEMORY);
    }

    // create global mutex name based on log file
    CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), *result = this->CreateGlobalMutexName());

    // create mutex, can return NULL
    CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), this->mutex = CreateMutex(NULL, FALSE, this->globalMutexName));
    CHECK_POINTER_HRESULT(*result, this->mutex, *result, E_OUTOFMEMORY);

    // TO DO:
    // creating mutex fail if mutex is created by service and it is requested by user application
    // it is returned access denied error code (5)

    FREE_MEM(backupFile);
  }
}

CLoggerFile::~CLoggerFile(void)
{
  FREE_MEM(this->globalMutexName);
  FREE_MEM(this->logBackupFile);
  FREE_MEM(this->logFile);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }
  this->mutex = NULL;
}

/* get methods */

const wchar_t *CLoggerFile::GetLogFile(void)
{
  return this->logFile;
}

const wchar_t *CLoggerFile::GetLogBackupFile(void)
{
  return this->logBackupFile;
}

const wchar_t *CLoggerFile::GetLogGlobalMutexName(void)
{
  return this->globalMutexName;
}

HANDLE CLoggerFile::GetLogMutex(void)
{
  return this->mutex;
}

unsigned int CLoggerFile::GetMaxLogSize(void)
{
  return this->maxLogSize;
}

/* set methods */

/* other methods */

void CLoggerFile::AddReference(void)
{
  this->referenceCount++;
}

void CLoggerFile::RemoveReference(void)
{
  this->referenceCount--;
}

/* protected methods */

HRESULT CLoggerFile::CreateGlobalMutexName(void)
{
  // global mutex name will consists from "Global\" and [A-Za-z0-9] characters from log file name, up to MAX_PATH count of characters
  HRESULT result = S_OK;
  this->globalMutexName = ALLOC_MEM_SET(this->globalMutexName, wchar_t, MAX_PATH, 0);
  CHECK_POINTER_HRESULT(result, this->globalMutexName, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    unsigned int processed = wcslen(GLOBAL_MUTEX_NAME_PREFIX);
    unsigned int logFileLength = wcslen(this->logFile);

    wcscpy_s(this->globalMutexName, MAX_PATH, GLOBAL_MUTEX_NAME_PREFIX);

    for (unsigned int i = 0; (SUCCEEDED(result) && (processed < MAX_PATH) && (i < logFileLength)); i++)
    {
      if (((this->logFile[i] >= 'A') && (this->logFile[i] <= 'Z')) ||
          ((this->logFile[i] >= 'a') && (this->logFile[i] <= 'z')) ||
          ((this->logFile[i] >= '0') && (this->logFile[i] <= '9')))
      {
        this->globalMutexName[processed++] = this->logFile[i];
      }
    }

    // sanity
    this->globalMutexName[processed] = '\0';
  }

  return result;
}