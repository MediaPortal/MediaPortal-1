/*
 *  Copyright (C) 2006-2008 Team MediaPortal
 *  http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#include "..\shared\Thread.h"
#include <cstddef>    // NULL
#include <process.h>  // _beginthread()
#include <Windows.h>  // CloseHandle(), CreateEvent(), GetLastError(), INVALID_HANDLE_VALUE, SetEvent(), WaitForSingleObject()
#include "..\shared\EnterCriticalSection.h"


extern void LogDebug(const wchar_t* fmt, ...);

CThread::CThread()
{
  m_thread = INVALID_HANDLE_VALUE;
  m_wakeEvent = NULL;
  m_stopSignal = true;
  m_frequency = INFINITE;
  m_function = NULL;
  m_context = NULL;
}

CThread::~CThread()
{
  Stop();
}

bool CThread::Start(unsigned long frequency, bool (*function)(void*), void* context)
{
  LogDebug(L"thread: start");
  CEnterCriticalSection lock(m_section);
  m_wakeEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  if (m_wakeEvent == NULL)
  {
    LogDebug(L"thread: failed to create wake event, error = %lu",
              GetLastError());
    return false;
  }

  m_wakeCount = 0;
  m_stopSignal = false;
  m_frequency = frequency;
  m_function = function;
  m_context = context;

  m_thread = (HANDLE)_beginthread(&CThread::ThreadFunction, 0, (void*)this);
  if (m_thread == INVALID_HANDLE_VALUE)
  {
    LogDebug(L"thread: failed to start thread execution");
    m_stopSignal = true;
    CloseHandle(m_wakeEvent);
    m_wakeEvent = NULL;
    return false;
  }

  LogDebug(L"thread: started");
  return true;
}

bool CThread::IsRunning()
{
  return m_thread != INVALID_HANDLE_VALUE;
}

bool CThread::Wake()
{
  CEnterCriticalSection lock(m_section);
  if (m_wakeEvent == NULL)
  {
    LogDebug(L"thread: wake event is NULL");
    return false;
  }
  if (SetEvent(m_wakeEvent) != TRUE)
  {
    LogDebug(L"thread: failed to set wake event, error = %lu",
              GetLastError());
    return false;
  }
  m_wakeCount++;
  return true;
}

void CThread::Stop()
{
  CEnterCriticalSection lock(m_section);
  if (m_stopSignal || m_thread == INVALID_HANDLE_VALUE)
  {
    return;
  }

  LogDebug(L"thread: stop");
  m_stopSignal = true;
  if (Wake())
  {
    WaitForSingleObject(m_thread, INFINITE);
    LogDebug(L"thread: stopped");
  }
  if (m_wakeEvent != NULL)
  {
    CloseHandle(m_wakeEvent);
    m_wakeEvent = NULL;
  }
  m_thread = INVALID_HANDLE_VALUE;
}

void __cdecl CThread::ThreadFunction(void* arg)
{
  unsigned long threadId = GetCurrentThreadId();
  LogDebug(L"thread: running, ID = %lu", threadId);
  CThread* thread = (CThread*)arg;
  unsigned long loopCount = 0;
  unsigned long signalledCount = 0;
  while (true)
  {
    loopCount++;

    DWORD result = WaitForSingleObject(thread->m_wakeEvent,
                                        thread->m_frequency);
    if (result == 0)
    {
      signalledCount++;
    }
    if (loopCount == 0)
    {
      signalledCount = 0;
      thread->m_wakeCount = 0;
    }

    if (thread->m_stopSignal)
    {
      LogDebug(L"thread: stopping, ID = %lu, loop count = %lu, wake count = %lu, signalled count = %lu",
                threadId, loopCount, thread->m_wakeCount, signalledCount);
      return;
    }

    if (!(thread->m_function)(thread->m_context))
    {
      LogDebug(L"thread: finishing, ID = %lu, loop count = %lu, wake count = %lu, signalled count = %lu",
                threadId, loopCount, thread->m_wakeCount, signalledCount);
      CEnterCriticalSection lock(thread->m_section);
      thread->m_stopSignal = true;
      if (thread->m_wakeEvent != NULL)
      {
        CloseHandle(thread->m_wakeEvent);
        thread->m_wakeEvent = NULL;
      }
      LogDebug(L"thread: finished");
      return;
    }
  }
}