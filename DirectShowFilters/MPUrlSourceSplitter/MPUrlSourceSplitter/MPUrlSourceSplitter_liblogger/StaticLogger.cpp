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

CStaticLogger::CStaticLogger(void)
{
  this->loggerContexts = new CStaticLoggerContextCollection();
  this->loggerWorkerThread = NULL;
  this->loggerWorkerShouldExit = false;

  this->mutex = CreateMutex(NULL, FALSE, NULL);
  this->referencies = 0;
}

CStaticLogger::~CStaticLogger(void)
{
  this->DestroyLoggerWorker();

  FREE_MEM_CLASS(this->loggerContexts);

  if (this->mutex != NULL)
  {
    CloseHandle(this->mutex);
  }
  this->mutex = NULL;
}

/* get methods */

CStaticLoggerContextCollection *CStaticLogger::GetLoggerContexts(void)
{
  return this->loggerContexts;
}

/* set methods */

/* other methods */

HANDLE CStaticLogger::Initialize(DWORD maxLogSize, unsigned int allowedLogVerbosity, const wchar_t *logFile, const wchar_t *logBackupFile, const wchar_t *globalMutexName)
{
  bool res = (this->loggerContexts != NULL)  && (logFile != NULL) && (logBackupFile != NULL) && (globalMutexName != NULL);
  HANDLE result = NULL;

  CStaticLoggerContext *foundContext = NULL;
  for (unsigned int i = 0; (res && (i < this->loggerContexts->Count())); i++)
  {
    CStaticLoggerContext *context = this->loggerContexts->GetItem(i);

    if (wcscmp(context->GetGlobalMutexName(), globalMutexName) == 0)
    {
      foundContext = context;
      break;
    }
  }

  if (res)
  {
    if (foundContext == NULL)
    {
      // create new static logger context and add it to collection
      foundContext = new CStaticLoggerContext();
      res &= (foundContext != NULL);

      CHECK_CONDITION_EXECUTE(res, res &= foundContext->Initialize(maxLogSize, allowedLogVerbosity, logFile, logBackupFile, globalMutexName));
      CHECK_CONDITION_EXECUTE(res, res &= this->loggerContexts->Add(foundContext));
      CHECK_CONDITION_EXECUTE(!res, FREE_MEM_CLASS(foundContext));
    }
    
    if (foundContext != NULL)
    {
      result = foundContext->GetMutex();
    }
  }

  return result;
}

void CStaticLogger::LogMessage(HANDLE mutex, unsigned int logLevel, const wchar_t *message)
{
  CStaticLoggerContext *context = NULL;

  for (unsigned int i = 0; i < this->loggerContexts->Count(); i++)
  {
    CStaticLoggerContext *temp = this->loggerContexts->GetItem(i);

    if (temp->GetMutex() == mutex)
    {
      context = temp;
      break;
    }
  }

  if (context != NULL)
  {
    if (logLevel <= context->GetAllowedLogVerbosity())
    {
      CLockMutex lock(context->GetMutex(), INFINITE);

      context->GetMessages()->Add(L"", message);
    }
  }
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

      {
        CLockMutex lock(caller->mutex, 10);

        if (lock.IsLocked())
        {
          caller->Flush();
        }
      }
    }

    if (caller->loggerWorkerShouldExit)
    {
      break;
    }

    // give chance to other threads to do something useful
    Sleep(1);
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

void CStaticLogger::Add(void)
{
  CLockMutex lock(this->mutex, INFINITE);

  if (this->referencies == 0)
  {
    this->CreateLoggerWorker();
  }

  this->referencies++;
}

void CStaticLogger::Remove(void)
{
  CLockMutex lock(this->mutex, INFINITE);

  this->referencies--;

  // the last reference is for FFmpeg logger
  if (this->referencies <= 1)
  {
    this->DestroyLoggerWorker();
    this->Flush();
  }
}

void CStaticLogger::Flush(void)
{
  CLockMutex lock(this->mutex, INFINITE);

  unsigned int contextCount = this->loggerContexts->Count();
  for (unsigned int i = 0; i < contextCount; i++)
  {
    CStaticLoggerContext *context = this->loggerContexts->GetItem(i);

    unsigned int messagesCount = 0;

    {
      CLockMutex lock(context->GetMutex(), INFINITE);

      messagesCount = context->GetMessages()->Count();
    }

    if (messagesCount > 0)
    {
      if (context->GetLogFile() != NULL)
      {
        HANDLE hLogFile = CreateFile(context->GetLogFile(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
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

          unsigned int bufferSize = context->GetMaxLogSize();
          unsigned int bufferOccupied = 0;

          ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
          if (buffer != NULL)
          {
            for (unsigned int j = 0; j < messagesCount; j++)
            {
              const wchar_t *message = context->GetMessages()->GetItem(j)->GetValue();
              unsigned int messageSize = wcslen(message) * sizeof(wchar_t);
              
              if (((size.LowPart + bufferOccupied + messageSize) > bufferSize) && (context->GetLogBackupFile() != NULL))
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
                DeleteFile(context->GetLogBackupFile());
                MoveFile(context->GetLogFile(), context->GetLogBackupFile());

                hLogFile = CreateFile(context->GetLogFile(), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_WRITE_THROUGH, NULL);
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

      {
        CLockMutex lock(context->GetMutex(), INFINITE);

        for (unsigned int j = 0; j < messagesCount; j++)
        {
          context->GetMessages()->Remove(0);
        }
      }
    }
  }
}