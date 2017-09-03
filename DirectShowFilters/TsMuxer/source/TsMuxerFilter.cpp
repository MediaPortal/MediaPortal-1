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
#include "TsMuxerFilter.h"
#include <cstddef>    // NULL
#include <sstream>
#include "..\..\shared\TimeUtils.h"


extern void LogDebug(const wchar_t* fmt, ...);
extern bool TsMuxerDumpInput();
extern bool TsMuxerDumpOutput();

CTsMuxerFilter::CTsMuxerFilter(IStreamMultiplexer* multiplexer,
                                const wchar_t* debugPath,
                                LPUNKNOWN unk,
                                CCritSec* filterLock,
                                CCritSec& receiveLock,
                                HRESULT* hr)
  : CBaseFilter(NAME("MediaPortal TS Muxer"), unk, filterLock, CLSID_TS_MUXER),
    m_receiveLock(receiveLock)
{
  LogDebug(L"filter: constructor");
  if (multiplexer == NULL)
  {
    LogDebug(L"filter: multiplexer not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_multiplexer = multiplexer;

  m_outputPin = new CTsOutputPin(this, filterLock, hr);
  if (m_outputPin == NULL || !SUCCEEDED(*hr))
  {
    if (SUCCEEDED(*hr))
    {
      *hr = E_OUTOFMEMORY;
    }
    LogDebug(L"filter: failed to allocate output pin, hr = 0x%x", *hr);
    return;
  }

  *hr = AddPin();

  if (debugPath != NULL)
  {
    m_debugPath = debugPath;
  }
  m_inputPinDebugMask = 0;
  m_isOutputDebugEnabled = false;

  LogDebug(L"filter: completed, hr = 0x%x", *hr);
}

CTsMuxerFilter::~CTsMuxerFilter()
{
  LogDebug(L"filter: destructor");
  CAutoLock pinLock(&m_inputPinsLock);
  vector<CMuxInputPin*>::iterator it = m_inputPins.begin();
  for ( ; it != m_inputPins.end(); it++)
  {
    CMuxInputPin* inputPin = *it;
    if (inputPin != NULL)
    {
      delete inputPin;
      *it = NULL;
    }
  }
  m_inputPins.clear();

  if (m_outputPin != NULL)
  {
    delete m_outputPin;
    m_outputPin = NULL;
  }
  LogDebug(L"filter: completed");
}

CBasePin* CTsMuxerFilter::GetPin(int n)
{
  if (n == 0)
  {
    return m_outputPin;
  }
  n--;

  CAutoLock pinLock(&m_inputPinsLock);
  if (n < 0 || n >= (int)m_inputPins.size())
  {
    return NULL;
  }
  return m_inputPins[n];
}

HRESULT CTsMuxerFilter::AddPin()
{
  CAutoLock pinLock(&m_inputPinsLock);
  LogDebug(L"filter: add pin, current pin count = %llu",
            (unsigned long long)m_inputPins.size());

  // If any pin is unconnected or the filter is running then don't add a new pin.
  if (!IsStopped())
  {
    LogDebug(L"filter: can't add pin unless filter is stopped");
    return VFW_E_NOT_STOPPED;
  }
  unsigned char pinIndex = 0;
  vector<CMuxInputPin*>::const_iterator it = m_inputPins.begin();
  for ( ; it != m_inputPins.end(); it++)
  {
    if (!(*it)->IsConnected())
    {
      LogDebug(L"filter: pin %hhu is available", pinIndex);
      return S_FALSE;
    }
    pinIndex++;
  }

  // We have a maximum pin count.
  if (pinIndex >= 254)
  {
    LogDebug(L"filter: failed to add pin, limit reached");
    return E_FAIL;
  }

  LogDebug(L"filter: adding pin %hhu", pinIndex + 1);
  HRESULT hr = S_OK;
  CMuxInputPin* inputPin = new CMuxInputPin(pinIndex,
                                            m_multiplexer,
                                            this,
                                            m_pLock,
                                            m_receiveLock,
                                            &hr);
  if (inputPin == NULL || FAILED(hr))
  {
    if (SUCCEEDED(hr))
    {
      hr = E_OUTOFMEMORY;
    }
    LogDebug(L"filter: failed to add pin, hr = 0x%x", hr);
    return hr;
  }
  m_inputPins.push_back(inputPin);
  IncrementPinVersion();
  return S_OK;
}

int CTsMuxerFilter::GetPinCount()
{
  CAutoLock pinLock(&m_inputPinsLock);
  return 1 + m_inputPins.size();
}

HRESULT CTsMuxerFilter::Deliver(const unsigned char* data, long dataLength)
{
  return m_outputPin->Deliver(data, dataLength);
}

STDMETHODIMP CTsMuxerFilter::Pause()
{
  LogDebug(L"filter: pause");
  CAutoLock filterLock(m_pLock);
  LogDebug(L"filter: pause filter...");
  HRESULT hr = CBaseFilter::Pause();
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

STDMETHODIMP CTsMuxerFilter::Run(REFERENCE_TIME startTime)
{
  LogDebug(L"filter: run");
  CAutoLock filterLock(m_pLock);

  LogDebug(L"filter: start stream monitor thread...");
  m_streamingMonitorThreadContext.m_filter = this;
  {
    CAutoLock pinLock(&m_inputPinsLock);
    for (vector<CMuxInputPin*>::const_iterator it = m_inputPins.begin(); it != m_inputPins.end(); it++)
    {
      CMuxInputPin* pin = *it;
      m_streamingMonitorThreadContext.m_pinStates[pin->GetId()] = pin->IsConnected() == TRUE;
    }
  }
  if (!m_streamingMonitorThread.Start(STREAM_IDLE_TIMEOUT,
                                      &CTsMuxerFilter::StreamingMonitorThreadFunction,
                                      (void*)&m_streamingMonitorThreadContext))
  {
    return E_FAIL;
  }

  // Configure dumping.
  long inputPinDebugMask = m_inputPinDebugMask;
  bool isOutputDebugEnabled = m_isOutputDebugEnabled;
  if (TsMuxerDumpInput())
  {
    inputPinDebugMask = 0xffffffff;
  }
  if (TsMuxerDumpOutput())
  {
    isOutputDebugEnabled = true;
  }
  if (inputPinDebugMask != 0 || isOutputDebugEnabled)
  {
    LogDebug(L"filter: configure dumping, input mask = 0x%08x, output = %d...",
              inputPinDebugMask, isOutputDebugEnabled);
    if (inputPinDebugMask != 0)
    {
      wstringstream fileName;
      CAutoLock pinLock(&m_inputPinsLock);
      vector<CMuxInputPin*>::const_iterator it = m_inputPins.begin();
      long mask = 1;
      for ( ; it != m_inputPins.end(); it++)
      {
        if ((inputPinDebugMask & mask) != 0 || inputPinDebugMask == 0xffffffff)
        {
          fileName.str(wstring());
          fileName << m_debugPath << L"\\ts_muxer_input_" << (*it)->GetId() << L"_dump.dmp";
          wstring tempFileName = fileName.str();
          (*it)->StartDumping(tempFileName.c_str());
        }
        mask = mask << 1;
      }
    }
    if (isOutputDebugEnabled)
    {
      wstring fileName(m_debugPath);
      fileName += L"\\ts_muxer_output_dump.ts";
      m_outputPin->StartDumping(fileName.c_str());
    }
  }

  LogDebug(L"filter: reset multiplexer...");
  HRESULT hr = m_multiplexer->Reset();

  LogDebug(L"filter: start filter...");
  hr |= CBaseFilter::Run(startTime);
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

STDMETHODIMP CTsMuxerFilter::Stop()
{
  LogDebug(L"filter: stop");
  CAutoLock filterLock(m_pLock);
  LogDebug(L"filter: stop receiving...");
  CAutoLock receiveLock(&m_receiveLock);

  LogDebug(L"filter: stop stream monitor thread...");
  m_streamingMonitorThread.Stop();

  CAutoLock pinLock(&m_inputPinsLock);
  vector<CMuxInputPin*>::const_iterator it = m_inputPins.begin();
  for ( ; it != m_inputPins.end(); it++)
  {
    (*it)->StopDumping();
  }
  m_outputPin->StopDumping();

  LogDebug(L"filter: stop filter...");
  HRESULT hr = CBaseFilter::Stop();
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

HRESULT CTsMuxerFilter::SetDumpFilePath(const wchar_t* path)
{
  if (path == NULL)
  {
    return E_INVALIDARG;
  }
  CAutoLock filterLock(m_pLock);
  m_debugPath = path;
  return S_OK;
}

void CTsMuxerFilter::DumpInput(long mask)
{
  CAutoLock filterLock(m_pLock);
  m_inputPinDebugMask = mask;
}

void CTsMuxerFilter::DumpOutput(bool enable)
{
  CAutoLock filterLock(m_pLock);
  m_isOutputDebugEnabled = enable;
}

bool __cdecl CTsMuxerFilter::StreamingMonitorThreadFunction(void* arg)
{
  CThreadContext* context = (CThreadContext*)arg;
  if (context == NULL)
  {
    LogDebug(L"filter: streaming monitor thread context not provided");
    return false;
  }
  CTsMuxerFilter* filter = context->m_filter;
  IStreamMultiplexer* muxer = filter->m_multiplexer;
  if (muxer != NULL && muxer->IsStarted())
  {
    CAutoLock pinLock(&(filter->m_inputPinsLock));
    for (vector<CMuxInputPin*>::const_iterator it = filter->m_inputPins.begin(); it != filter->m_inputPins.end(); it++)
    {
      CMuxInputPin* pin = *it;
      unsigned char pinId = pin->GetId();
      bool wasReceiving = context->m_pinStates[pinId];
      bool isReceiving = true;
      if (
        pin->IsConnected() != TRUE ||
        pin->GetReceiveTime() == NOT_RECEIVING ||
        CTimeUtils::ElapsedMillis(pin->GetReceiveTime()) >= STREAM_IDLE_TIMEOUT
      )
      {
        isReceiving = false;
      }
      if (wasReceiving != isReceiving)
      {
        LogDebug(L"filter: pin %hhu changed receiving state, %d => %d",
                  pinId, wasReceiving, isReceiving);
        context->m_pinStates[pinId] = isReceiving;
      }
    }
  }
  return true;
}