/**
*	TSThread.cpp
*  Copyright (C) 2004-2006 bear
*  Copyright (C) 2005      nate
*
*  This file is part of TSFileSource, a directshow push source filter that
*  provides an MPEG transport stream output.
*
*  TSFileSource is free software; you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation; either version 2 of the License, or
*  (at your option) any later version.
*
*  TSFileSource is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with TSFileSource; if not, write to the Free Software
*  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*
*  bear and nate can be reached on the forums at
*    http://forums.dvbowners.com/
*/

#include "StdAfx.h"

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "TSThread.h"
#include <process.h>

// For more details for memory leak detection see the alloctracing.h header
#include "..\..\alloctracing.h"

extern void LogDebug(const char *fmt, ...) ;

/*
void TSThreadThreadProc(void *pParam)
{
	((TSThread *)pParam)->InternalThreadProc();
}
*/

//////////////////////////////////////////////////////////////////////
// DWSource
//////////////////////////////////////////////////////////////////////

TSThread::TSThread()
{
	m_hStopEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
	m_hDoneEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
	m_hWakeEvent = CreateEvent(NULL, FALSE, FALSE, NULL); //Auto-reset
	m_threadHandle = INVALID_HANDLE_VALUE;
  m_bThreadRunning=FALSE;
}

TSThread::~TSThread()
{
	if (m_threadHandle != INVALID_HANDLE_VALUE)
	{
	  StopThread(5000);
	}
	CloseHandle(m_hStopEvent);
	CloseHandle(m_hDoneEvent);
	CloseHandle(m_hWakeEvent);
}


BOOL TSThread::IsThreadRunning()
{
  return m_bThreadRunning;
}

HRESULT TSThread::StartThread()
{
  CAutoLock tLock (&m_ThreadStateLock);
	ResetEvent(m_hStopEvent);

	m_threadHandle = (HANDLE)_beginthread(&TSThread::thread_function, 0, (void *) this);
	if (m_threadHandle == INVALID_HANDLE_VALUE)
	{
    LogDebug("TSThread::StartThread failed !!");
		return E_FAIL;
	}
	
	return S_OK;
}

HRESULT TSThread::StopThread(DWORD dwTimeoutMilliseconds)
{
  CAutoLock tLock (&m_ThreadStateLock);
	HRESULT hr = S_OK;
	
  if (m_threadHandle == INVALID_HANDLE_VALUE)
	{
	  return S_FALSE;
	}
	
	ResetEvent(m_hDoneEvent);
  m_bThreadRunning=FALSE; //Block wake events
	SetEvent(m_hStopEvent);
	DWORD result = WaitForSingleObject(m_hDoneEvent, dwTimeoutMilliseconds);

	if ((result == WAIT_TIMEOUT) && (m_threadHandle != INVALID_HANDLE_VALUE))
	{
	  try
	  {
  		TerminateThread(m_threadHandle, -1);
  		CloseHandle(m_threadHandle);
	  }
    catch(...) {}
    
		hr = S_FALSE;
	}
	else if (result != WAIT_OBJECT_0)
	{
		DWORD err = GetLastError();
		return HRESULT_FROM_WIN32(err);
	}

	m_threadHandle = INVALID_HANDLE_VALUE;

	return hr;
}

void TSThread::WakeThread()
{
  if (m_bThreadRunning)
  {
	  SetEvent(m_hWakeEvent);
	}  
}

BOOL TSThread::ThreadIsStopping(DWORD dwTimeoutMilliseconds)
{
  HANDLE hEvts[] = {m_hStopEvent, m_hWakeEvent};

	DWORD result = WaitForMultipleObjects(2, hEvts, FALSE, dwTimeoutMilliseconds);
  switch (result)
  {
    case WAIT_OBJECT_0 : //m_hStopEvent
	    return true;
      break;
    case WAIT_OBJECT_0 + 1 : //m_hWakeEvent
	    ResetEvent(m_hWakeEvent);
	    return false;
      break;
    case WAIT_TIMEOUT : //Timeout
	    return false;
      break;
    default : //Error conditions
	    return true;      
  }
}

void TSThread::InternalThreadProc()
{
	ResetEvent(m_hDoneEvent);
  m_bThreadRunning=TRUE;
	try
	{
		ThreadProc();
	}
	catch (LPWSTR pStr)
	{
		pStr = NULL;
	}
  m_bThreadRunning=FALSE;
	SetEvent(m_hDoneEvent);
}

void TSThread::thread_function(void* p)
{
	TSThread *thread = reinterpret_cast<TSThread *>(p);
	thread->InternalThreadProc();
	_endthread();
}