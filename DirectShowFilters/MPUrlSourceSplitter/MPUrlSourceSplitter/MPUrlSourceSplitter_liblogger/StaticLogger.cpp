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

#include "StaticLogger.h"
#include "LockMutex.h"

#include <process.h>
#include <stdio.h>
#include <assert.h>

CStaticLogger::CStaticLogger(HRESULT *result)
{
  this->loggerWorkerThread = NULL;
  this->loggerWorkerShouldExit = false;
  this->loggerContexts = NULL;
  this->mutex = NULL;
  this->registeredModules = NULL;
  this->loggerFiles = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->loggerContexts = new CLoggerContextCollection(result);
    this->loggerFiles = new CLoggerFileCollection(result);
    this->mutex = CreateMutex(NULL, FALSE, NULL);
    this->registeredModules = new CParameterCollection(result);

    CHECK_POINTER_HRESULT(*result, this->loggerContexts, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->loggerFiles, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->mutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->registeredModules, *result, E_OUTOFMEMORY);
  }
}

CStaticLogger::~CStaticLogger(void)
{
  this->DestroyLoggerWorker();
  this->Flush();

  FREE_MEM_CLASS(this->loggerContexts);
  FREE_MEM_CLASS(this->loggerFiles);
  FREE_MEM_CLASS(this->registeredModules);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }
  this->mutex = NULL;
}

/* get methods */

CLoggerContextCollection *CStaticLogger::GetLoggerContexts(void)
{
  return this->loggerContexts;
}

unsigned int CStaticLogger::GetLoggerContext(GUID guid, unsigned int maxLogSize, unsigned int allowedLogVerbosity, const wchar_t *logFile)
{
  unsigned int contextHandle = LOGGER_CONTEXT_INVALID_HANDLE;

  // we must be sure, that Flush() isn't working
  LOCK_MUTEX(this->mutex, INFINITE)

  HRESULT result = S_OK;
  CLoggerContext *context = NULL;
  CLoggerContext *firstFreeContext = NULL;
  unsigned int firstFreeContextHandle = LOGGER_CONTEXT_INVALID_HANDLE;

  for (unsigned int i = 0; i < this->loggerContexts->Count(); i++)
  {
    CLoggerContext *temp = this->loggerContexts->GetItem(i);

    if ((firstFreeContext == NULL) && (temp->IsFree()))
    {
      firstFreeContext = temp;
      firstFreeContextHandle = i;
    }
    
    if (IsEqualGUID(temp->GetLoggerGUID(), guid) != 0)
    {
      context = temp;
      contextHandle = i;
      break;
    }
  }

  if ((context == NULL) && (firstFreeContext != NULL))
  {
    context = firstFreeContext;
    contextHandle = firstFreeContextHandle;

    context->SetLoggerGUID(guid);
  }

  if (context == NULL)
  {
    // context is NULL for new instance of filter's logger
    
    context = new CLoggerContext(&result, guid);
    CHECK_POINTER_HRESULT(result, context, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, this->loggerContexts->Add(context), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(context));
    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), contextHandle = this->loggerContexts->Count() - 1);
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(context, context->SetAllowedLogVerbosity(allowedLogVerbosity));

  if (SUCCEEDED(result))
  {
    if ((context != NULL) && (context->GetLoggerFile() != NULL) && (logFile == NULL))
    {
      context->RemoveLoggerFileReference();
    }
    else if ((context != NULL) && (context->GetLoggerFile() == NULL) && (logFile != NULL))
    {
      // find or create logger file for logger context
      CLoggerFile *loggerFile = NULL;

      for (unsigned int i = 0; i < this->loggerFiles->Count(); i++)
      {
        CLoggerFile *temp = this->loggerFiles->GetItem(i);

        if (_wcsicmp(temp->GetLogFile(), logFile) == 0)
        {
          loggerFile = temp;
          break;
        }
      }

      if (loggerFile == NULL)
      {
        loggerFile = new CLoggerFile(&result, logFile, maxLogSize);
        CHECK_POINTER_HRESULT(result, loggerFile, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->loggerFiles->Add(loggerFile), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(loggerFile));
      }

      CHECK_CONDITION_NOT_NULL_EXECUTE(loggerFile, context->AddLoggerFileReference(loggerFile));
    }

    if (FAILED(result))
    {
      // context is already created
      context->Clear();
      contextHandle = LOGGER_CONTEXT_INVALID_HANDLE;
    }
  }

  UNLOCK_MUTEX(this->mutex)

  return contextHandle;
}

/* set methods */

/* other methods */

void CStaticLogger::LogMessage(unsigned int context, unsigned int logLevel, const wchar_t *message)
{
  if (context != LOGGER_CONTEXT_INVALID_HANDLE)
  {
    CLoggerContext *cnt = this->loggerContexts->GetItem(context);

    if ((cnt != NULL) && cnt->IsAllowedLogVerbosity(logLevel))
    {
      LOCK_MUTEX(cnt->GetMutex(), INFINITE)

      cnt->GetMessages()->Add(L"", message);

      UNLOCK_MUTEX(cnt->GetMutex())
    }
  }
  else
  {
    for (unsigned int i = 0; i < this->loggerContexts->Count(); i++)
    {
      CLoggerContext *cnt = this->loggerContexts->GetItem(i);

      if ((!cnt->IsFree()) && cnt->IsAllowedLogVerbosity(logLevel))
      {
        LOCK_MUTEX(cnt->GetMutex(), INFINITE)

        cnt->GetMessages()->Add(L"", message);

        UNLOCK_MUTEX(cnt->GetMutex())
      }
    }
  }
}

bool CStaticLogger::RegisterModule(const wchar_t *moduleFileName)
{
  bool result = true;

  if (!this->registeredModules->Contains(moduleFileName, false))
  {
    result &= this->registeredModules->Add(moduleFileName, L"");
  }

  return true;
}

void CStaticLogger::UnregisterModule(const wchar_t *moduleFileName)
{
  this->registeredModules->Remove(moduleFileName, false);
}

bool CStaticLogger::IsRegisteredModule(const wchar_t *moduleFileName)
{
  return this->registeredModules->Contains(moduleFileName, false);
}

bool CStaticLogger::AddLoggerContextReference(unsigned int context)
{
  CLoggerContext *cnt = this->loggerContexts->GetItem(context);

  if (cnt != NULL)
  {
    if (SUCCEEDED(this->CreateLoggerWorker()))
    {
      cnt->AddReference();
      return true;
    }
  }

  return false;
}

bool CStaticLogger::RemoveLoggerContextReference(unsigned int context)
{
  CLoggerContext *cnt = this->loggerContexts->GetItem(context);

  if ((cnt != NULL) && (!cnt->IsFree()))
  {
    if (cnt->RemoveReference() == 0)
    {
      // no logger has reference to this context
      this->FlushContext(context);
      cnt->Clear();

      // check if there is any used context
      // in another case, destroy logger worker
      bool allContextFree = true;
      for (unsigned int i = 0; i < this->loggerContexts->Count(); i++)
      {
        CLoggerContext *ctx = this->loggerContexts->GetItem(i);

        allContextFree &= ctx->IsFree();
      }

      CHECK_CONDITION_EXECUTE(allContextFree, this->DestroyLoggerWorker());
    }

    return true;
  }

  return false;
}

bool CStaticLogger::RemoveLoggerFileReference(unsigned int context)
{
  CLoggerContext *cnt = this->loggerContexts->GetItem(context);

  if ((cnt != NULL) && (!cnt->IsFree()))
  {
    cnt->RemoveLoggerFileReference();
    return true;
  }

  return false;
}

/* protected methods */

unsigned int WINAPI CStaticLogger::LoggerWorker(LPVOID lpParam)
{
  CStaticLogger *caller = (CStaticLogger *)lpParam;

  unsigned int lastFlushTime = 0;
  while (true)
  {
    if (caller->loggerWorkerShouldExit || ((GetTickCount() - lastFlushTime) > 10000))
    {
      lastFlushTime = GetTickCount();

      LOCK_MUTEX(caller->mutex, 10)

      caller->Flush();

      UNLOCK_MUTEX(caller->mutex)
    }

    if (caller->loggerWorkerShouldExit)
    {
      break;
    }

    // give chance to other threads to do something useful
    Sleep(20);
  }

  // _endthreadex should be called automatically, but for sure
  _endthreadex(0);

  return S_OK;
}

HRESULT CStaticLogger::CreateLoggerWorker(void)
{
  HRESULT result = S_OK;

  if (this->loggerWorkerThread == NULL)
  {
    this->loggerWorkerThread = (HANDLE)_beginthreadex(NULL, 0, &CStaticLogger::LoggerWorker, this, 0, NULL);
  }

  if (this->loggerWorkerThread == NULL)
  {
    // thread not created
    result = HRESULT_FROM_WIN32(GetLastError());
  }

  return result;
}

HRESULT CStaticLogger::DestroyLoggerWorker(void)
{
  HRESULT result = S_OK;

  // wait for the receive data worker thread to exit      
  if (this->loggerWorkerThread != NULL)
  {
    this->loggerWorkerShouldExit = true;
    if (WaitForSingleObject(this->loggerWorkerThread, INFINITE) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      TerminateThread(this->loggerWorkerThread, 0);
    }
    CloseHandle(this->loggerWorkerThread);
  }

  this->loggerWorkerShouldExit = false;
  this->loggerWorkerThread = NULL;
  
  return result;
}

void CStaticLogger::Flush(void)
{
  LOCK_MUTEX(this->mutex, INFINITE)

  HRESULT result = S_OK;
  unsigned int contextCount = this->loggerContexts->Count();

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < contextCount)); i++)
  {
    result = this->FlushContext(i);
  }

  UNLOCK_MUTEX(this->mutex)
}

HRESULT CStaticLogger::FlushContext(unsigned int contextHandle)
{
  HRESULT result = S_OK;
  LOCK_MUTEX(this->mutex, INFINITE)
  
  CParameterCollection *temporaryMessages = new CParameterCollection(&result);
  CLoggerContext *context = this->loggerContexts->GetItem(contextHandle);

  CHECK_POINTER_HRESULT(result, temporaryMessages, result, E_OUTOFMEMORY);
  CHECK_POINTER_HRESULT(result, context, result, E_INVALIDARG);

  if (SUCCEEDED(result) && (context->GetLoggerFile() != NULL))
  {
    // in rare circumstances can be called Flush() method with LogMessage() method simultaneously
    // in case that there is no space in internal memory for new message, then internal memory must be resized
    // in rare case we can get another pointer to internal memory, which can lead to crash
    // we need to copy all messages from context to temporary collection, process temporary collection and remove all processed messages from context

    temporaryMessages->Clear();

    LOCK_MUTEX(context->GetMutex(), INFINITE)

    temporaryMessages->Append(context->GetMessages());

    UNLOCK_MUTEX(context->GetMutex())

    unsigned int messagesCount = temporaryMessages->Count();

    if (messagesCount > 0)
    {
      if (context->GetLoggerFile()->GetLogFile() != NULL)
      {
        HANDLE hLogFile = CreateFile(context->GetLoggerFile()->GetLogFile(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
        if (hLogFile != INVALID_HANDLE_VALUE)
        {
          // move to end of log file
          LARGE_INTEGER distanceToMove;
          LARGE_INTEGER size;

          distanceToMove.QuadPart = 0;
          if (!GetFileSizeEx(hLogFile, &size))
          {
            // error occured while getting file size
            size.QuadPart = 0;
          }

          SetFilePointerEx(hLogFile, distanceToMove, NULL, FILE_END);

          unsigned int bufferSize = context->GetLoggerFile()->GetMaxLogSize();
          unsigned int bufferOccupied = 0;

          ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
          if (buffer != NULL)
          {
            for (unsigned int j = 0; j < messagesCount; j++)
            {
              const wchar_t *message = temporaryMessages->GetItem(j)->GetValue();
              unsigned int messageSize = wcslen(message) * sizeof(wchar_t);

              if (((size.LowPart + bufferOccupied + messageSize) > bufferSize) && (context->GetLoggerFile()->GetLogBackupFile() != NULL))
              {
                // write data to log file
                DWORD written = 0;
                WriteFile(hLogFile, buffer, bufferOccupied, &written, NULL);

                size.QuadPart = 0;

                CloseHandle(hLogFile);
                hLogFile = INVALID_HANDLE_VALUE;
                bufferOccupied = 0;
                memset(buffer, 0, bufferSize);

                // log file exceedes maximum log size
                DeleteFile(context->GetLoggerFile()->GetLogBackupFile());
                MoveFile(context->GetLoggerFile()->GetLogFile(), context->GetLoggerFile()->GetLogBackupFile());

                hLogFile = CreateFile(context->GetLoggerFile()->GetLogFile(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
                if (hLogFile == INVALID_HANDLE_VALUE)
                {
                  messagesCount = j;
                  break;
                }
              }

              memcpy(buffer + bufferOccupied, (unsigned char *)message, messageSize);
              bufferOccupied += messageSize;
            }

            if (bufferOccupied != 0)
            {
              // write data to log file
              DWORD written = 0;
              WriteFile(hLogFile, buffer, bufferOccupied, &written, NULL);
            }
          }
          FREE_MEM(buffer);

          CHECK_CONDITION_EXECUTE(hLogFile != INVALID_HANDLE_VALUE, CloseHandle(hLogFile));
          hLogFile = INVALID_HANDLE_VALUE;
        }
      }

      LOCK_MUTEX(context->GetMutex(), INFINITE)

      context->GetMessages()->CCollection::Remove(0, messagesCount);

      UNLOCK_MUTEX(context->GetMutex())
    }
  }

  FREE_MEM_CLASS(temporaryMessages);

  UNLOCK_MUTEX(this->mutex)
  return result;
}