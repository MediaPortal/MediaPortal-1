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

#include "StaticLoggerContext.h"
#include "Logger.h"

CStaticLoggerContext::CStaticLoggerContext(void)
{
  this->allowedLogVerbosity = LOGGER_NONE;
  this->globalMutexName = NULL;
  this->logBackupFile = NULL;
  this->logFile = NULL;
  this->maxLogSize = 0;
  this->mutex = NULL;
  this->messages = NULL;
}

CStaticLoggerContext::~CStaticLoggerContext(void)
{
  FREE_MEM_CLASS(this->messages);
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

HANDLE CStaticLoggerContext::GetMutex(void)
{
  return this->mutex;
}

DWORD CStaticLoggerContext::GetMaxLogSize(void)
{
  return this->maxLogSize;
}

unsigned int CStaticLoggerContext::GetAllowedLogVerbosity(void)
{
  return this->allowedLogVerbosity;
}

const wchar_t *CStaticLoggerContext::GetLogFile(void)
{
  return this->logFile;
}

const wchar_t *CStaticLoggerContext::GetLogBackupFile(void)
{
  return this->logBackupFile;
}

const wchar_t *CStaticLoggerContext::GetGlobalMutexName(void)
{
  return this->globalMutexName;
}

CParameterCollection *CStaticLoggerContext::GetMessages(void)
{
  return this->messages;
}

/* set methods */

/* other methods */

bool CStaticLoggerContext::Initialize(DWORD maxLogSize, unsigned int allowedLogVerbosity, const wchar_t *logFile, const wchar_t *logBackupFile, const wchar_t *globalMutexName)
{
  bool result = (this->mutex == NULL) && (this->logFile == NULL) && (this->logBackupFile == NULL) && (this->globalMutexName == NULL) && (this->messages == NULL);

  if (result)
  {
    this->messages = new CParameterCollection();
    result &= (this->messages != NULL);

    SET_STRING_AND_RESULT_WITH_NULL(this->logFile, logFile, result);
    SET_STRING_AND_RESULT_WITH_NULL(this->logBackupFile, logBackupFile, result);
    SET_STRING_AND_RESULT_WITH_NULL(this->globalMutexName, globalMutexName, result);
    this->allowedLogVerbosity = allowedLogVerbosity;
    this->maxLogSize = maxLogSize;

    if (result)
    {
      // create mutex, can return NULL
      this->mutex = CreateMutex(NULL, false, this->globalMutexName);
    }

    result &= (this->mutex != NULL);
  }

  return result;
}
