/*
 *  Copyright (C) 2005-2013 Team MediaPortal
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
#include "TsWriterFilter.h"
#include <process.h>  // _beginthread()
#include <Windows.h>  // CloseHandle(), CreateEvent(), INVALID_HANDLE_VALUE, ResetEvent(), SetEvent(), WaitForSingleObject()
#include "..\..\shared\TimeUtils.h"


extern void LogDebug(const wchar_t* fmt, ...);
extern bool TsWriterDumpInput();

CTsWriterFilter::CTsWriterFilter(ITsAnalyser* analyser,
                                  const wchar_t* debugPath,
                                  LPUNKNOWN unk,
                                  CCritSec* filterLock,
                                  CCritSec& receiveLock,
                                  HRESULT* hr)
  : CBaseFilter(NAME("MediaPortal TS Writer"), unk, filterLock, CLSID_TS_WRITER),
    m_receiveLock(receiveLock)
{
  LogDebug(L"filter: constructor");
  if (analyser == NULL)
  {
    LogDebug(L"filter: analyser not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_analyser = analyser;

  m_inputPinOobSi = new CInputPinOobSi(analyser, this, filterLock, receiveLock, hr);
  if (m_inputPinOobSi == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"filter: failed to allocate TS input pin, hr = 0x%x", *hr);
    return;
  }

  m_inputPinTs = new CInputPinTs(analyser, this, filterLock, receiveLock, hr);
  if (m_inputPinTs == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"filter: failed to allocate TS input pin, hr = 0x%x", *hr);
    return;
  }

  m_streamingMonitorThread = INVALID_HANDLE_VALUE;
  m_streamingMonitorThreadStopEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  if (m_streamingMonitorThreadStopEvent == NULL)
  {
    *hr = GetLastError();
    LogDebug(L"filter: failed to create streaming monitor thread stop event, hr = 0x%x",
              *hr);
    return;
  }

  if (debugPath != NULL)
  {
    m_debugPath.str(debugPath);
  }
  m_isDebugEnabledOobSi = false;
  m_isDebugEnabledTs = false;

  LogDebug(L"filter: completed, hr = 0x%x", *hr);
}

CTsWriterFilter::~CTsWriterFilter()
{
  LogDebug(L"filter: destructor");

  if (m_inputPinOobSi != NULL)
  {
    delete m_inputPinOobSi;
    m_inputPinOobSi = NULL;
  }
  if (m_inputPinTs != NULL)
  {
    delete m_inputPinTs;
    m_inputPinTs = NULL;
  }

  if (m_streamingMonitorThreadStopEvent != NULL)
  {
    CloseHandle(m_streamingMonitorThreadStopEvent);
  }
  LogDebug(L"filter: completed");
}

CBasePin* CTsWriterFilter::GetPin(int n)
{
  if (n == 0)
  {
    return m_inputPinTs;
  }
  if (n == 1)
  {
    return m_inputPinOobSi;
  }
  return NULL;
}

int CTsWriterFilter::GetPinCount()
{
  return 2;
}

STDMETHODIMP CTsWriterFilter::Pause()
{
  LogDebug(L"filter: pause");
  CAutoLock filterLock(m_pLock);
  LogDebug(L"filter: pause filter...");
  HRESULT hr = CBaseFilter::Pause();
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

STDMETHODIMP CTsWriterFilter::Run(REFERENCE_TIME startTime)
{
  LogDebug(L"filter: run");
  CAutoLock filterLock(m_pLock);

  LogDebug(L"filter: start stream monitor thread...");
  ResetEvent(m_streamingMonitorThreadStopEvent);
  m_streamingMonitorThread = (HANDLE)_beginthread(&CTsWriterFilter::StreamingMonitorThreadFunction,
                                                  0,
                                                  (void*)this);
  if (m_streamingMonitorThread == INVALID_HANDLE_VALUE)
  {
    return E_OUTOFMEMORY;
  }

  // Configure dumping.
  bool isDebugEnabledOobSi = m_isDebugEnabledOobSi;
  bool isDebugEnabledTs = m_isDebugEnabledTs;
  if (TsWriterDumpInput())
  {
    isDebugEnabledOobSi = true;
    isDebugEnabledTs = true;
  }
  if (isDebugEnabledOobSi || isDebugEnabledTs)
  {
    LogDebug(L"filter: configure dumping, OOB SI = %d, TS = %d...",
              isDebugEnabledOobSi, isDebugEnabledTs);
    wstringstream fileName;
    if (isDebugEnabledOobSi)
    {
      fileName << m_debugPath.str() << L"\\ts_writer_oob_si_input_dump.ts";
      m_inputPinOobSi->StartDumping(fileName.str().c_str());
    }
    if (isDebugEnabledTs)
    {
      fileName.str(wstring());
      fileName << m_debugPath.str() << L"\\ts_writer_ts_input_dump.ts";
      m_inputPinTs->StartDumping(fileName.str().c_str());
    }
  }

  LogDebug(L"filter: start filter...");
  HRESULT hr = CBaseFilter::Run(startTime);
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

STDMETHODIMP CTsWriterFilter::Stop()
{
  LogDebug(L"filter: stop");
  CAutoLock filterLock(m_pLock);
  LogDebug(L"filter: stop receiving...");
  CAutoLock receiveLock(&m_receiveLock);

  LogDebug(L"filter: stop stream monitor thread...");
  SetEvent(m_streamingMonitorThreadStopEvent);
  WaitForSingleObject(m_streamingMonitorThread, INFINITE);
  m_streamingMonitorThread = INVALID_HANDLE_VALUE;

  m_inputPinOobSi->StopDumping();
  m_inputPinTs->StopDumping();

  LogDebug(L"filter: stop filter...");
  HRESULT hr = CBaseFilter::Stop();
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

void __cdecl CTsWriterFilter::StreamingMonitorThreadFunction(void* arg)
{
  LogDebug(L"filter: monitor thread started");
  CTsWriterFilter* filter = (CTsWriterFilter*)arg;
  ITsAnalyser* analyser = filter->m_analyser;
  bool isReceivingOobSi = filter->m_inputPinOobSi->IsConnected() == TRUE;
  bool isReceivingTs = filter->m_inputPinTs->IsConnected() == TRUE;
  while (true)
  {
    DWORD result = WaitForSingleObject(filter->m_streamingMonitorThreadStopEvent,
                                        STREAM_IDLE_TIMEOUT);
    if (result != WAIT_TIMEOUT)
    {
      // event was set
      break;
    }

    bool wasReceiving = isReceivingOobSi;
    bool isReceiving = true;
    if (
      filter->m_inputPinOobSi->IsConnected() != TRUE ||
      filter->m_inputPinOobSi->GetReceiveTime() == NOT_RECEIVING ||
      CTimeUtils::ElapsedMillis(filter->m_inputPinOobSi->GetReceiveTime()) >= STREAM_IDLE_TIMEOUT
    )
    {
      isReceiving = false;
    }
    if (wasReceiving != isReceiving)
    {
      LogDebug(L"filter: OOB SI pin changed receiving state, %d => %d",
                wasReceiving, isReceiving);
    }
    isReceivingOobSi = isReceiving;

    wasReceiving = isReceivingTs;
    isReceiving = true;
    if (
      filter->m_inputPinTs->IsConnected() != TRUE ||
      filter->m_inputPinTs->GetReceiveTime() == NOT_RECEIVING ||
      CTimeUtils::ElapsedMillis(filter->m_inputPinTs->GetReceiveTime()) >= STREAM_IDLE_TIMEOUT
    )
    {
      isReceiving = false;
    }
    if (wasReceiving != isReceiving)
    {
      LogDebug(L"filter: TS pin changed receiving state, %d => %d",
                wasReceiving, isReceiving);
    }
    isReceivingTs = isReceiving;
  }
  LogDebug(L"filter: monitor thread stopped");
}

STDMETHODIMP CTsWriterFilter::SetDumpFilePath(wchar_t* path)
{
  if (path == NULL)
  {
    return E_INVALIDARG;
  }
  CAutoLock filterLock(m_pLock);
  m_debugPath.str(path);
  return S_OK;
}

STDMETHODIMP CTsWriterFilter::DumpInput(bool enableTs, bool enableOobSi)
{
  CAutoLock filterLock(m_pLock);
  m_isDebugEnabledTs = enableTs;
  m_isDebugEnabledOobSi = enableOobSi;
  return S_OK;
}

void CTsWriterFilter::CheckSectionCrcs(bool enable)
{
  m_inputPinOobSi->CheckSectionCrcs(enable);
}