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

  if (debugPath != NULL)
  {
    m_debugPath = debugPath;
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

  if (m_streamingMonitorThread.IsRunning())
  {
    LogDebug(L"filter: stream monitor already started...");
  }
  else
  {
    LogDebug(L"filter: start stream monitor thread...");
    m_streamingMonitorThreadContext.m_filter = this;
    m_streamingMonitorThreadContext.m_isReceivingOobSi = m_inputPinOobSi->IsConnected() == TRUE;
    m_streamingMonitorThreadContext.m_isReceivingTs = m_inputPinTs->IsConnected() == TRUE;
    if (!m_streamingMonitorThread.Start(STREAM_IDLE_TIMEOUT,
                                        &CTsWriterFilter::StreamingMonitorThreadFunction,
                                        (void*)&m_streamingMonitorThreadContext))
    {
      return E_FAIL;
    }
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
    if (isDebugEnabledOobSi)
    {
      wstring fileName(m_debugPath);
      fileName += L"\\ts_writer_oob_si_input_dump.ts";
      m_inputPinOobSi->StartDumping(fileName.c_str());
    }
    if (isDebugEnabledTs)
    {
      wstring fileName(m_debugPath);
      fileName += L"\\ts_writer_ts_input_dump.ts";
      m_inputPinTs->StartDumping(fileName.c_str());
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

  if (!m_streamingMonitorThread.IsRunning())
  {
    LogDebug(L"filter: stream monitor thread already stopped...");
  }
  else
  {
    LogDebug(L"filter: stop stream monitor thread...");
    m_streamingMonitorThread.Stop();
  }

  m_inputPinOobSi->StopDumping();
  m_inputPinTs->StopDumping();

  LogDebug(L"filter: stop filter...");
  HRESULT hr = CBaseFilter::Stop();
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

HRESULT CTsWriterFilter::SetDumpFilePath(const wchar_t* path)
{
  if (path == NULL)
  {
    return E_INVALIDARG;
  }
  CAutoLock filterLock(m_pLock);
  m_debugPath = path;
  return S_OK;
}

void CTsWriterFilter::DumpInput(bool enableTs, bool enableOobSi)
{
  CAutoLock filterLock(m_pLock);
  m_isDebugEnabledTs = enableTs;
  m_isDebugEnabledOobSi = enableOobSi;
}

void CTsWriterFilter::CheckSectionCrcs(bool enable)
{
  m_inputPinOobSi->CheckSectionCrcs(enable);
}

bool __cdecl CTsWriterFilter::StreamingMonitorThreadFunction(void* arg)
{
  CThreadContext* context = (CThreadContext*)arg;
  if (context == NULL)
  {
    LogDebug(L"filter: streaming monitor thread context not provided");
    return false;
  }
  CTsWriterFilter* filter = context->m_filter;

  bool wasReceiving = context->m_isReceivingOobSi;
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
  context->m_isReceivingOobSi = isReceiving;

  wasReceiving = context->m_isReceivingTs;
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
  context->m_isReceivingTs = isReceiving;
  return true;
}