// Copyright (C) 2005-2012 Team MediaPortal
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
#include "Globals.h"
#include "QueuedAudioSink.h"

#include "alloctracing.h"

#define END_OF_STREAM_FLUSH_TIMEOUT (5000)

CQueuedAudioSink::CQueuedAudioSink(void) : 
  CBaseAudioSink(false),
  m_hThread(NULL),
  m_ThreadId(NULL)
{
  //memset(m_hEvents, 0, sizeof(m_hEvents));
  m_hStopThreadEvent = CreateEvent(0, TRUE, FALSE, 0);
  m_hInputAvailableEvent = CreateEvent(0, TRUE, FALSE, 0);
  m_hOOBCommandAvailableEvent = CreateEvent(0, TRUE, FALSE, 0);

  m_hEvents.push_back(m_hOOBCommandAvailableEvent);
  m_hEvents.push_back(m_hInputAvailableEvent);
  m_hEvents.push_back(m_hStopThreadEvent);

  m_dwWaitObjects.push_back(S_OK);
  m_dwWaitObjects.push_back(MPAR_S_OOB_COMMAND_AVAILABLE);
  m_dwWaitObjects.push_back(MPAR_S_THREAD_STOPPING);

  m_hCurrentSampleReleased = CreateEvent(0, FALSE, FALSE, 0);

  //m_hInputQueueEmptyEvent = CreateEvent(0, FALSE, FALSE, 0);
}

CQueuedAudioSink::~CQueuedAudioSink(void)
{
  if (m_hStopThreadEvent)
    CloseHandle(m_hStopThreadEvent);
  if (m_hInputAvailableEvent)
    CloseHandle(m_hInputAvailableEvent);
  if (m_hOOBCommandAvailableEvent)
    CloseHandle(m_hOOBCommandAvailableEvent);
  if (m_hCurrentSampleReleased)
    CloseHandle(m_hCurrentSampleReleased);

  //if (m_hInputQueueEmptyEvent)
  //  CloseHandle(m_hInputQueueEmptyEvent);
}

// Control
HRESULT CQueuedAudioSink::Start(REFERENCE_TIME rtStart)
{
  HRESULT hr = CBaseAudioSink::Start(rtStart);
  if (FAILED(hr))
    return hr;

  return StartThread();
}

HRESULT CQueuedAudioSink::Run(REFERENCE_TIME rtStart)
{
  HRESULT hr = S_OK;

  if (!m_hThread)
    hr = Start(rtStart);

  if (FAILED(hr))
    Log("QueuedAudioSink::Run - failed to start ThreadProc (0x%08x)", hr);

  PutOOBCommand(ASC_Resume);
  return CBaseAudioSink::Run(rtStart);
}

HRESULT CQueuedAudioSink::Pause()
{
  PutOOBCommand(ASC_Pause);
  return CBaseAudioSink::Pause();
}

HRESULT CQueuedAudioSink::BeginStop()
{
  SetEvent(m_hStopThreadEvent);
  return CBaseAudioSink::BeginStop();
}

HRESULT CQueuedAudioSink::EndStop()
{
  return CBaseAudioSink::EndStop();
}

// Processing
HRESULT CQueuedAudioSink::PutSample(IMediaSample *pSample)
{
  if (m_bFlushing)
    return S_OK;

  CAutoLock queueLock(&m_inputQueueLock);
  m_inputQueue.push_back(pSample);
  SetEvent(m_hInputAvailableEvent);
  //if(m_hInputQueueEmptyEvent)
  //  ResetEvent(m_hInputQueueEmptyEvent);

  return S_OK;
}

HRESULT CQueuedAudioSink::PutCommand(AudioSinkCommand nCommand)
{
  CAutoLock queueLock(&m_inputQueueLock);
  m_inputQueue.push_back(nCommand);
  SetEvent(m_hInputAvailableEvent);
  //if(m_hInputQueueEmptyEvent)
  //  ResetEvent(m_hInputQueueEmptyEvent);

  return S_OK;
}

HRESULT CQueuedAudioSink::PutOOBCommand(AudioSinkCommand nCommand)
{
  CAutoLock queueLock(&m_OOBInputQueueLock);
  m_OOBInputQueue.push(nCommand);
  SetEvent(m_hOOBCommandAvailableEvent);

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
  ResetEvent(m_hCurrentSampleReleased);

  // Request the derived class to release the current sample and stall threadproc
  PutOOBCommand(ASC_Flush);

  {
    CAutoLock queueLock(&m_inputQueueLock);
    ResetEvent(m_hInputAvailableEvent);
    while (!m_inputQueue.empty())
      m_inputQueue.erase(m_inputQueue.begin());
    //SetEvent(m_hInputQueueEmptyEvent);
  }

  return CBaseAudioSink::BeginFlush();
}

HRESULT CQueuedAudioSink::EndFlush()
{
  if (m_hThread)
    WaitForSingleObject(m_hCurrentSampleReleased, INFINITE);

  HRESULT hr = CBaseAudioSink::EndFlush();
  return hr;
}

// Queue services
HRESULT CQueuedAudioSink::WaitForEvents(DWORD dwTimeout, vector<HANDLE>* pEvents, vector<DWORD>* pWaitObjects)
{
  vector<HANDLE>* events = NULL;
  vector<DWORD>* waitObjects = NULL;

  bool useBaseEvents = !(pEvents && pWaitObjects);

  if (useBaseEvents)
  {
    events = &m_hEvents;
    waitObjects = &m_dwWaitObjects;
  }
  else
  {
    events = pEvents;
    waitObjects = pWaitObjects;
  }

  DWORD result = WaitForMultipleObjects(static_cast<DWORD>(events->size()), &(*events)[0], FALSE, dwTimeout);
  HRESULT hr = S_FALSE;

  if (result == WAIT_TIMEOUT)
    hr = MPAR_S_WAIT_TIMED_OUT;
  else if (result != WAIT_FAILED)
    hr = (*waitObjects)[result];

  return hr;
}

// Get the next sample in the queue. If there is none, wait for at most
// dwTimeout milliseconds for one to become available before failing.
// Returns: S_FALSE if no sample available
// Threading: only one thread should be calling GetNextSampleOrCommand()
// but it can be different from the one calling PutSample()/PutCommand()
HRESULT CQueuedAudioSink::GetNextSampleOrCommand(AudioSinkCommand* pCommand, IMediaSample** pSample, DWORD dwTimeout,
                                                  vector<HANDLE>* pHandles, vector<DWORD>* pWaitObjects, bool handleOOBOnly)
{
  HRESULT hr = WaitForEvents(dwTimeout, pHandles, pWaitObjects);

  if (hr == MPAR_S_WAIT_TIMED_OUT || hr == S_FALSE)
  {
    if (pSample && *pSample && !handleOOBOnly)
    {
      (*pSample)->Release();
      (*pSample) = NULL;
    }

    *pCommand = ASC_Nop;
    return WAIT_TIMEOUT;
  }
  
  {
    CAutoLock OOBQueueLock(&m_OOBInputQueueLock);
    if (!m_OOBInputQueue.empty())
    {
      TQueueEntry entry = m_OOBInputQueue.front();
      if (pCommand)
        *pCommand = entry.Command;
      
      m_OOBInputQueue.pop();

      if (m_OOBInputQueue.empty())
        ResetEvent(m_hOOBCommandAvailableEvent);

      return S_OK;
    }
    else if (handleOOBOnly)
    {
      *pCommand = ASC_Nop;
      return S_OK;
    }
  }

  if (hr != S_OK)
    return hr;

  if (pSample && *pSample)
    (*pSample)->Release();

  CAutoLock queueLock(&m_inputQueueLock);
  TQueueEntry entry = m_inputQueue.front();
  if (pSample)
    *pSample = entry.Sample.Detach();
  if (pCommand)
    *pCommand = entry.Command;

  m_inputQueue.erase(m_inputQueue.begin());
  if (m_inputQueue.empty())
    ResetEvent(m_hInputAvailableEvent);
  //if (m_InputQueue.empty())
  //  SetEvent(m_hInputQueueEmptyEvent);

  return S_OK;
}

DWORD WINAPI CQueuedAudioSink::ThreadEntryPoint(LPVOID lpParameter)
{
  return ((CQueuedAudioSink *)lpParameter)->ThreadProc();
}

HRESULT CQueuedAudioSink::CloseThread()
{
  CAutoLock lock(&m_csResources);

  if (m_hThread)
  {
    //WaitForSingleObject(m_hThread, INFINITE);
    CloseHandle(m_hThread);
    m_hThread = NULL;
    ResetEvent(m_hStopThreadEvent);
  }

  return S_OK;
}

HRESULT CQueuedAudioSink::StartThread()
{
  CAutoLock lock(&m_csResources);

  if (!m_hThread)
  {
    ResetEvent(m_hStopThreadEvent);
    m_hThread = CreateThread(0, 0, CQueuedAudioSink::ThreadEntryPoint, (LPVOID)this, 0, &m_ThreadId);
  }

  if (!m_hThread)
    return HRESULT_FROM_WIN32(GetLastError());

  return S_OK;
}