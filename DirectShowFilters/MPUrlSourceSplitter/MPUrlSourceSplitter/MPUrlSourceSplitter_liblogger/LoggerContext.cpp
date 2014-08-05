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

#include "LoggerContext.h"
#include "Logger.h"

CLoggerContext::CLoggerContext(HRESULT *result, GUID guid)
  : CFlags()
{
  this->messages = NULL;
  this->loggerFile = NULL;
  this->referenceCount = 0;
  this->mutex = NULL;
  this->loggerGUID = guid;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->mutex = CreateMutex(NULL, FALSE, NULL);
    this->messages = new CParameterCollection(result);

    CHECK_POINTER_HRESULT(*result, this->mutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->messages, *result, E_OUTOFMEMORY);
  }
}

CLoggerContext::~CLoggerContext(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->loggerFile, this->loggerFile->RemoveReference());
  FREE_MEM_CLASS(this->messages);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }
  this->mutex = NULL;
}

/* get methods */

CParameterCollection *CLoggerContext::GetMessages(void)
{
  return this->messages;
}

CLoggerFile *CLoggerContext::GetLoggerFile(void)
{
  return this->loggerFile;
}

HANDLE CLoggerContext::GetMutex(void)
{
  return this->mutex;
}

GUID CLoggerContext::GetLoggerGUID(void)
{
  return this->loggerGUID;
}

/* set methods */

void CLoggerContext::SetLoggerGUID(GUID loggerGUID)
{
  this->loggerGUID = loggerGUID;
}

void CLoggerContext::SetAllowedLogVerbosity(unsigned int allowedLogVerbosity)
{
  this->flags &= ~(LOGGER_CONTEXT_FLAG_VERBOSITY_ERROR | LOGGER_CONTEXT_FLAG_VERBOSITY_WARNING | LOGGER_CONTEXT_FLAG_VERBOSITY_INFO | LOGGER_CONTEXT_FLAG_VERBOSITY_VERBOSE);
  switch (allowedLogVerbosity)
  {
  case LOGGER_ERROR:
    this->flags |= LOGGER_CONTEXT_FLAG_VERBOSITY_ERROR;
    break;
  case LOGGER_WARNING:
    this->flags |= LOGGER_CONTEXT_FLAG_VERBOSITY_ERROR | LOGGER_CONTEXT_FLAG_VERBOSITY_WARNING;
    break;
  case LOGGER_INFO:
    this->flags |= LOGGER_CONTEXT_FLAG_VERBOSITY_ERROR | LOGGER_CONTEXT_FLAG_VERBOSITY_WARNING | LOGGER_CONTEXT_FLAG_VERBOSITY_INFO;
    break;
  case LOGGER_VERBOSE:
    this->flags |= LOGGER_CONTEXT_FLAG_VERBOSITY_ERROR | LOGGER_CONTEXT_FLAG_VERBOSITY_WARNING | LOGGER_CONTEXT_FLAG_VERBOSITY_INFO | LOGGER_CONTEXT_FLAG_VERBOSITY_VERBOSE;
    break;
  }
}

/* other methods */

bool CLoggerContext::IsAllowedLogVerbosity(unsigned int logVerbosity)
{
  switch (logVerbosity)
  {
  case LOGGER_ERROR:
    return this->IsSetFlags(LOGGER_CONTEXT_FLAG_VERBOSITY_ERROR);
  case LOGGER_WARNING:
    return this->IsSetFlags(LOGGER_CONTEXT_FLAG_VERBOSITY_WARNING);
  case LOGGER_INFO:
    return this->IsSetFlags(LOGGER_CONTEXT_FLAG_VERBOSITY_INFO);
  case LOGGER_VERBOSE:
    return this->IsSetFlags(LOGGER_CONTEXT_FLAG_VERBOSITY_VERBOSE);
  }

  return false;
}

bool CLoggerContext::IsFree(void)
{
  return (IsEqualGUID(this->loggerGUID, GUID_NULL) != 0);
}

unsigned int CLoggerContext::AddReference(void)
{
  this->referenceCount++;

  return this->referenceCount;
}

unsigned int CLoggerContext::RemoveReference(void)
{
  this->referenceCount--;

  return this->referenceCount;
}

void CLoggerContext::AddLoggerFileReference(CLoggerFile *loggerFile)
{
  // for sure, remove current logger file reference
  this->RemoveLoggerFileReference();

  // set new logger file, add logger file reference
  this->loggerFile = loggerFile;
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->loggerFile, this->loggerFile->AddReference());
}

void CLoggerContext::RemoveLoggerFileReference(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->loggerFile, this->loggerFile->RemoveReference());
  this->loggerFile = NULL;
}

void CLoggerContext::Clear(void)
{
  this->RemoveLoggerFileReference();

  this->flags = LOGGER_CONTEXT_FLAG_NONE;
  this->messages->Clear();
  this->loggerGUID = GUID_NULL;
  this->referenceCount = 0;
}