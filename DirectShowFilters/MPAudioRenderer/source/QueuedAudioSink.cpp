// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#include "stdafx.h"
#include "QueuedAudioSink.h"

#define END_OF_STREAM_FLUSH_TIMEOUT (5000)

CQueuedAudioSink::CQueuedAudioSink(void)
: m_hThread(NULL)
{
  //memset(m_hEvents, 0, sizeof(m_hEvents));
  m_hStopThreadEvent = CreateEvent(0, TRUE, FALSE, 0);
  m_hInputSamplesAvailableEvent = CreateEvent(0, TRUE, FALSE, 0);
  //m_hInputQueueEmptyEvent = CreateEvent(0, FALSE, FALSE, 0);
}

CQueuedAudioSink::~CQueuedAudioSink(void)
{
  if (m_hStopThreadEvent)
    CloseHandle(m_hStopThreadEvent);
  if (m_hInputSamplesAvailableEvent)
    CloseHandle(m_hInputSamplesAvailableEvent);
  //if (m_hInputQueueEmptyEvent)
  //  CloseHandle(m_hInputQueueEmptyEvent);
}

// Control
HRESULT CQueuedAudioSink::Start()
{
  HRESULT hr = CBaseAudioSink::Start();
  if (FAILED(hr))
    return hr;

  if (!m_hThread)
  {
    ResetEvent(m_hStopThreadEvent);
    m_hThread = CreateThread(0, 0, CQueuedAudioSink::ThreadEntryPoint, (LPVOID)this, 0, NULL);
  }

  if (!m_hThread)
    return HRESULT_FROM_WIN32(GetLastError());

  return S_OK;
}

HRESULT CQueuedAudioSink::BeginStop()
{
  SetEvent(m_hStopThreadEvent);
  return CBaseAudioSink::BeginStop();
}

HRESULT CQueuedAudioSink::EndStop()
{
  if (m_hThread)
  {
    WaitForSingleObject(m_hThread, INFINITE); //perhaps a reasonable timeout is needed
    CloseHandle(m_hThread);
    m_hThread = NULL;
    ResetEvent(m_hStopThreadEvent);
  }

  return CBaseAudioSink::EndStop();
}

// Processing
HRESULT CQueuedAudioSink::PutSample(IMediaSample *pSample)
{
  CAutoLock queueLock(&m_InputQueueLock);
  m_InputQueue.push(pSample);
  SetEvent(m_hInputSamplesAvailableEvent);
  //if(m_hInputQueueEmptyEvent)
  //  ResetEvent(m_hInputQueueEmptyEvent);

  return S_OK;
}

HRESULT CQueuedAudioSink::EndOfStream()
{
  // Ensure all samples are processed:
  // wait until input queue is empty
  //if(m_hInputQueueEmptyEvent)
  //  WaitForSingleObject(m_hInputQueueEmptyEvent, END_OF_STREAM_FLUSH_TIMEOUT); // TODO make this depend on the amount of data in the queue

  // Call next filter only after processing the entire queue
  return CBaseAudioSink::EndOfStream();
}

HRESULT CQueuedAudioSink::BeginFlush()
{
  {
    CAutoLock queueLock(&m_InputQueueLock);
    ResetEvent(m_hInputSamplesAvailableEvent);
    while (!m_InputQueue.empty())
      m_InputQueue.pop();
    //SetEvent(m_hInputQueueEmptyEvent);
  }

  return CBaseAudioSink::BeginFlush();
}

//HRESULT CQueuedAudioSink::EndFlush()
//{
//  return CBaseAudioSink::EndFlush();
//}

// Queue services
HRESULT CQueuedAudioSink::WaitForSample(DWORD dwTimeout)
{
  HANDLE hEvents[2] = {m_hStopThreadEvent, m_hInputSamplesAvailableEvent};

  DWORD result = WaitForMultipleObjects(sizeof(hEvents)/sizeof(HANDLE), hEvents, FALSE, dwTimeout);
  switch(result)
  {
  case WAIT_OBJECT_0: // Stop Event
    return MPAR_S_THREAD_STOPPING;
  case WAIT_OBJECT_0 + 1:
    return S_OK;
  case WAIT_FAILED:
    return HRESULT_FROM_WIN32(GetLastError());
  default:
    return S_FALSE;
  }
}

// Get the next sample in the queue. If there is none, wait for at most
// dwTimeout milliseconds for one to become available before failing.
// Returns: S_FALSE if no sample available
// Threading: only one thread should be calling GetNextSample()
// but it can be different from the one calling PutSample()
HRESULT CQueuedAudioSink::GetNextSample(IMediaSample **pSample, DWORD dwTimeout)
{
  HRESULT hr = WaitForSample(dwTimeout);
  if (hr != S_OK)
    return hr;

  CAutoLock queueLock(&m_InputQueueLock);
  
  SAFE_RELEASE(*pSample);
  *pSample = m_InputQueue.front();
  if (*pSample)
    (*pSample)->AddRef();
  m_InputQueue.pop();
  if (m_InputQueue.empty())
    ResetEvent(m_hInputSamplesAvailableEvent);
  //if (m_InputQueue.empty())
  //  SetEvent(m_hInputQueueEmptyEvent);
  return S_OK;
}


DWORD WINAPI CQueuedAudioSink::ThreadEntryPoint(LPVOID lpParameter)
{
  return ((CQueuedAudioSink *)lpParameter)->ThreadProc();
}
