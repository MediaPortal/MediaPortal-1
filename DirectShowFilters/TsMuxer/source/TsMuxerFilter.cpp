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
#include <map>
#include <process.h>
#include <stdio.h>


extern void LogDebug(const wchar_t* fmt, ...);
extern bool TsMuxerDumpInput();
extern bool TsMuxerDumpOutput();

CTsMuxerFilter::CTsMuxerFilter(IStreamMultiplexer* multiplexer, wchar_t* debugPath, LPUNKNOWN unk, CCritSec* filterLock, CCritSec* receiveLock, HRESULT* hr)
  : CBaseFilter(NAME("MediaPortal TS Muxer"), unk, filterLock, CLSID_TS_MUXER)
{
  LogDebug(L"filter: constructor");
  m_multiplexer = multiplexer;
  m_receiveLock = receiveLock;
  m_outputPin = new CTsOutputPin(this, filterLock, hr);
  if (m_outputPin == NULL)
  {
    *hr = E_OUTOFMEMORY;
    return;
  }
  m_streamingMonitorThread = INVALID_HANDLE_VALUE;
  m_streamingMonitorThreadStopEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
  if (m_streamingMonitorThreadStopEvent == NULL)
  {
    *hr = GetLastError();
  }

  *hr = AddPin();

  if (debugPath != NULL)
  {
    wcscpy(m_debugPath, debugPath);
  }
  m_inputPinDebugMask = 0;
  m_isOutputDebugEnabled = false;

  LogDebug(L"filter: completed, hr = 0x%x", *hr);
}

CTsMuxerFilter::~CTsMuxerFilter(void)
{
  LogDebug(L"filter: destructor");
  CAutoLock pinLock(&m_inputPinsLock);
  vector<CMuxInputPin*>::iterator it = m_inputPins.begin();
  while (it != m_inputPins.end())
  {
    if (*it != NULL)
    {
      delete *it;
    }
    it++;
  }
  m_inputPins.clear();

  if (m_outputPin != NULL)
  {
    delete m_outputPin;
    m_outputPin = NULL;
  }

  if (m_streamingMonitorThreadStopEvent != NULL)
  {
    CloseHandle(m_streamingMonitorThreadStopEvent);
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
  LogDebug(L"filter: add pin, current pin count = %d", m_inputPins.size());

  // If any pin is unconnected or the filter is running then don't add a new pin.
  if (!IsStopped())
  {
    LogDebug(L"filter: can't add pin unless filter is stopped");
    return VFW_E_NOT_STOPPED;
  }
  byte pinIndex = 0;
  vector<CMuxInputPin*>::iterator it = m_inputPins.begin();
  while (it != m_inputPins.end())
  {
    if (!(*it)->IsConnected())
    {
      LogDebug(L"filter: pin %d is available", pinIndex);
      return S_FALSE;
    }
    pinIndex++;
    it++;
  }

  LogDebug(L"filter: adding pin %d", pinIndex + 1);
  HRESULT hr = S_OK;
  CMuxInputPin* inputPin = new CMuxInputPin(pinIndex, m_multiplexer, this, m_pLock, m_receiveLock, &hr);
  if (inputPin == NULL || !SUCCEEDED(hr))
  {
    if (SUCCEEDED(hr))
    {
      hr = E_OUTOFMEMORY;
    }
    LogDebug(L"filter: failed to add pin, hr = 0x%x", hr);
    return hr;
  }
  m_inputPins.push_back(inputPin);
  return S_OK;
}

int CTsMuxerFilter::GetPinCount()
{
  CAutoLock pinLock(&m_inputPinsLock);
  return 1 + m_inputPins.size();
}

HRESULT CTsMuxerFilter::Deliver(PBYTE data, long dataLength)
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
  ResetEvent(m_streamingMonitorThreadStopEvent);
  m_streamingMonitorThread = (HANDLE)_beginthread(&CTsMuxerFilter::StreamingMonitorThreadFunction, 0, (void*)this);
  if (m_streamingMonitorThread == INVALID_HANDLE_VALUE)
  {
    return E_POINTER;
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
    wchar_t fileName[MAX_PATH];
    if (inputPinDebugMask != 0)
    {
      CAutoLock pinLock(&m_inputPinsLock);
      vector<CMuxInputPin*>::iterator it = m_inputPins.begin();
      long mask = 1;
      while (it != m_inputPins.end())
      {
        if ((inputPinDebugMask & mask) != 0 || inputPinDebugMask == 0xffffffff)
        {
          swprintf_s(fileName, L"%s\\ts_muxer_input_%d_dump.dmp", m_debugPath, (*it)->GetId());
          (*it)->StartDumping(fileName);
        }
        mask = mask << 1;
        it++;
      }
    }
    if (isOutputDebugEnabled)
    {
      swprintf_s(fileName, L"%s\\ts_muxer_output_dump.ts", m_debugPath);
      m_outputPin->StartDumping(fileName);
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
  CAutoLock receiveLock(m_receiveLock);

  LogDebug(L"filter: stop stream monitor thread...");
  SetEvent(m_streamingMonitorThreadStopEvent);
  WaitForSingleObject(m_streamingMonitorThread, INFINITE);
  m_streamingMonitorThread = INVALID_HANDLE_VALUE;

  CAutoLock pinLock(&m_inputPinsLock);
  vector<CMuxInputPin*>::iterator it = m_inputPins.begin();
  while (it != m_inputPins.end())
  {
    (*it)->StopDumping();
    it++;
  }
  m_outputPin->StopDumping();

  LogDebug(L"filter: stop filter...");
  HRESULT hr = CBaseFilter::Stop();
  LogDebug(L"filter: completed, hr = 0x%x", hr);
  return hr;
}

void __cdecl CTsMuxerFilter::StreamingMonitorThreadFunction(void* arg)
{
  LogDebug(L"filter: monitor thread started");
  CTsMuxerFilter* filter = (CTsMuxerFilter*)arg;
  IStreamMultiplexer* muxer = filter->m_multiplexer;
  map<byte, BOOL> pinStates;
  bool isFirst = true;
  while (true)
  {
    DWORD result = WaitForSingleObject(filter->m_streamingMonitorThreadStopEvent, STREAM_IDLE_TIMEOUT);
    if (result != WAIT_TIMEOUT)
    {
      // event was set
      break;
    }

    if (muxer != NULL && muxer->IsStarted())
    {
      CAutoLock pinLock(&(filter->m_inputPinsLock));
      vector<CMuxInputPin*>::iterator it = filter->m_inputPins.begin();
      if (isFirst)
      {
        while (it != filter->m_inputPins.end())
        {
          CMuxInputPin* pin = *it;
          pinStates[pin->GetId()] = pin->IsConnected();
          it++;
        }
        isFirst = false;
      }
      else
      {
        while (it != filter->m_inputPins.end())
        {
          CMuxInputPin* pin = *it;
          byte pinId = pin->GetId();
          BOOL wasReceiving = pinStates[pinId];
          BOOL isReceiving = TRUE;
          if (pin->IsConnected() != TRUE || pin->GetReceiveTickCount() == NOT_RECEIVING || GetTickCount() - pin->GetReceiveTickCount() >= STREAM_IDLE_TIMEOUT)
          {
            isReceiving = FALSE;
          }
          if (wasReceiving != isReceiving)
          {
            LogDebug(L"filter: pin %d changed receiving state, %d => %d", pinId, wasReceiving, isReceiving);
          }
          pinStates[pinId] = isReceiving;
          it++;
        }
      }
    }
  }
  LogDebug(L"filter: monitor thread stopped");
}

STDMETHODIMP CTsMuxerFilter::SetDumpFilePath(wchar_t* path)
{
  if (path == NULL)
  {
    return E_POINTER;
  }
  CAutoLock filterLock(m_pLock);
  wcscpy(m_debugPath, path);
  return S_OK;
}

STDMETHODIMP CTsMuxerFilter::DumpInput(long mask)
{
  CAutoLock filterLock(m_pLock);
  m_inputPinDebugMask = mask;
  return S_OK;
}

STDMETHODIMP CTsMuxerFilter::DumpOutput(bool enable)
{
  CAutoLock filterLock(m_pLock);
  m_isOutputDebugEnabled = enable;
  return S_OK;
}