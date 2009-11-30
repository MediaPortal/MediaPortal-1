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

#include <winsock2.h>
#include <ws2tcpip.h>
#include <streams.h>
#include "TSThread.h"
#include <process.h>

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
	m_hStopEvent = CreateEvent(NULL, TRUE, TRUE, NULL);
	m_hDoneEvent = CreateEvent(NULL, TRUE, TRUE, NULL);
	m_threadHandle = INVALID_HANDLE_VALUE;
  m_bThreadRunning=FALSE;
}

TSThread::~TSThread()
{
	StopThread();
	CloseHandle(m_hStopEvent);
	CloseHandle(m_hDoneEvent);
}


BOOL TSThread::IsThreadRunning()
{
  return m_bThreadRunning;
}

HRESULT TSThread::StartThread()
{
	ResetEvent(m_hStopEvent);
	unsigned long m_threadHandle = _beginthread(&TSThread::thread_function, 0, (void *) this);
	if (m_threadHandle == (unsigned long)INVALID_HANDLE_VALUE)
		return E_FAIL;

	return S_OK;
}

HRESULT TSThread::StopThread(DWORD dwTimeoutMilliseconds)
{
	HRESULT hr = S_OK;

	SetEvent(m_hStopEvent);
	DWORD result = WaitForSingleObject(m_hDoneEvent, dwTimeoutMilliseconds);

	if ((result == WAIT_TIMEOUT) && (m_threadHandle != INVALID_HANDLE_VALUE))
	{
		TerminateThread(m_threadHandle, -1);
		CloseHandle(m_threadHandle);
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

BOOL TSThread::ThreadIsStopping(DWORD dwTimeoutMilliseconds)
{
	DWORD result = WaitForSingleObject(m_hStopEvent, dwTimeoutMilliseconds);
	return (result != WAIT_TIMEOUT);
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
	SetEvent(m_hDoneEvent);
  m_bThreadRunning=FALSE;
}

void TSThread::thread_function(void* p)
{
	TSThread *thread = reinterpret_cast<TSThread *>(p);
	thread->InternalThreadProc();
}