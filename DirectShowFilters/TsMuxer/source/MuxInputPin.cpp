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
#include "MuxInputPin.h"


extern void LogDebug(const wchar_t* fmt, ...);

CMuxInputPin::CMuxInputPin(byte id, IStreamMultiplexer* multiplexer, CBaseFilter* filter, CCritSec* filterLock, CCritSec* receiveLock, HRESULT* hr)
  : CRenderedInputPin(NAME("Input"), filter, filterLock, hr, L"Input")
{
  m_pinId = id;
  m_streamType = STREAM_TYPE_UNKNOWN;
  m_receiveTickCount = NOT_RECEIVING;
  m_receiveLock = receiveLock;
  m_multiplexer = multiplexer;
  m_tsReceiveBufferOffset = 0;
}

CMuxInputPin::~CMuxInputPin(void)
{
  StopDumping();
}

HRESULT CMuxInputPin::BreakConnect()
{
  CAutoLock lock(m_receiveLock);
  LogDebug(L"input: pin %d break connect", m_pinId);
  m_streamType = STREAM_TYPE_UNKNOWN;
  m_receiveTickCount = NOT_RECEIVING;
  HRESULT hr = m_multiplexer->BreakConnect(this);
  if (SUCCEEDED(hr))
  {
    hr = CRenderedInputPin::BreakConnect();
  }
  return hr;
}

HRESULT CMuxInputPin::CheckMediaType(const CMediaType* mediaType)
{
  for (int i = 0; i < INPUT_MEDIA_TYPE_COUNT; i++)
  {
    if (*INPUT_MEDIA_TYPES[i].clsMajorType == mediaType->majortype &&
      *INPUT_MEDIA_TYPES[i].clsMinorType == mediaType->subtype)
    {
      return S_OK;
    }
  }
  return VFW_E_TYPE_NOT_ACCEPTED;
}

HRESULT CMuxInputPin::CompleteConnect(IPin* receivePin)
{
  CAutoLock lock(m_receiveLock);
  LogDebug(L"input: pin %d complete connect", m_pinId);
  HRESULT hr = m_multiplexer->CompleteConnect(this);
  if (SUCCEEDED(hr))
  {
    CPacketSync::Reset();
    m_tsReceiveBufferOffset = 0;
    hr = CRenderedInputPin::CompleteConnect(receivePin);
  }
  return hr;
}

HRESULT CMuxInputPin::GetMediaType(int position, CMediaType* mediaType)
{
  if (position < 0)
  {
    return E_INVALIDARG;
  }
  if (position >= INPUT_MEDIA_TYPE_COUNT)
  {
    return VFW_S_NO_MORE_ITEMS;
  }

  mediaType->ResetFormatBuffer();
  mediaType->formattype = FORMAT_None;
  mediaType->InitMediaType();
  mediaType->majortype = *INPUT_MEDIA_TYPES[position].clsMajorType;
  mediaType->subtype = *INPUT_MEDIA_TYPES[position].clsMinorType;
  return S_OK;
}

STDMETHODIMP CMuxInputPin::EndOfStream(void)
{
  CAutoLock lock(m_receiveLock);
  return CRenderedInputPin::EndOfStream();
}

STDMETHODIMP CMuxInputPin::NewSegment(REFERENCE_TIME startTime, REFERENCE_TIME stopTime, double rate)
{
  return S_OK;
}

STDMETHODIMP CMuxInputPin::Receive(IMediaSample* sample)
{
  try
  {
    if (sample == NULL)
    {
      LogDebug(L"input: pin %d received NULL sample", m_pinId);
      return E_POINTER;
    }

    CAutoLock lock(m_receiveLock);
    if (IsStopped())
    {
      LogDebug(L"input: pin %d received sample when stopped", m_pinId);
      return VFW_E_NOT_RUNNING;
    }

    if (m_bAtEndOfStream)
    {
      LogDebug(L"input: pin %d received sample after end of stream", m_pinId);
      return VFW_E_SAMPLE_REJECTED_EOS;
    }

    long sampleLength = sample->GetActualDataLength();
    if (sampleLength <= 0)
    {
      return S_OK;
    }

    PBYTE data = NULL;
    HRESULT hr = sample->GetPointer(&data);
    if (FAILED(hr))
    {
      LogDebug(L"input: pin %d failed to get sample pointer, hr = 0x%x", m_pinId, hr);
      return E_POINTER;
    }

    REFERENCE_TIME startTime;
    REFERENCE_TIME stopTime;
    hr = sample->GetTime(&startTime, &stopTime);
    if (FAILED(hr))   // Seems to mean that this sample is a continuation of a previous frame.
    {
      startTime = -1;
    }
    /*else if (m_receiveTickCount == NOT_RECEIVING)
    {
      LogDebug(L"input: pin %d sample size = %d, start time = %lld, stop time = %lld", m_pinId, sampleLength, startTime, stopTime);
    }*/

    if (m_receiveTickCount == NOT_RECEIVING)
    {
      LogDebug(L"input: pin %d stream started", m_pinId);
    }
    if (m_isDumpEnabled)
    {
      CAutoLock lock(&m_dumpLock);
      if (!m_dumpFileWriter.IsFileInvalid())
      {
        LogDebug(L"input: pin %d dumping %d bytes, start time = %lld, stop time = %lld", m_pinId, sampleLength, startTime, stopTime);
        m_dumpFileWriter.Write(data, sampleLength);
      }
    }
    m_receiveTickCount = GetTickCount();
    if (m_streamType == STREAM_TYPE_MPEG2_TRANSPORT_STREAM)
    {
      OnRawData(data, sampleLength);
      return S_OK;
    }
    else
    {
      return m_multiplexer->Receive(this, data, sampleLength, startTime);
    }
  }
  catch (...)
  {
    LogDebug(L"input: pin %d unhandled exception in Receive()", m_pinId);
    return E_FAIL;
  }
}

STDMETHODIMP CMuxInputPin::ReceiveCanBlock()
{
  return S_FALSE;
}

HRESULT CMuxInputPin::SetMediaType(const CMediaType* mediaType)
{
  CAutoLock lock(m_receiveLock);
  /*LogDebug(L"debug: pin %d set media type %08x-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x  %08x-%04x-%04x-%02x%02x-%02x%02x%02x%02x%02x%02x",
      m_pinId, mediaType->majortype.Data1, mediaType->majortype.Data2, mediaType->majortype.Data3,
      mediaType->majortype.Data4[0], mediaType->majortype.Data4[1], mediaType->majortype.Data4[2],
      mediaType->majortype.Data4[3], mediaType->majortype.Data4[4], mediaType->majortype.Data4[5],
      mediaType->majortype.Data4[6], mediaType->majortype.Data4[7],
      mediaType->subtype.Data1, mediaType->subtype.Data2, mediaType->subtype.Data3,
      mediaType->subtype.Data4[0], mediaType->subtype.Data4[1], mediaType->subtype.Data4[2],
      mediaType->subtype.Data4[3], mediaType->subtype.Data4[4], mediaType->subtype.Data4[5],
      mediaType->subtype.Data4[6], mediaType->subtype.Data4[7]);*/
  byte oldStreamType = m_streamType;
  for (int i = 0; i < INPUT_MEDIA_TYPE_COUNT; i++)
  {
    if (*INPUT_MEDIA_TYPES[i].clsMajorType == mediaType->majortype &&
      *INPUT_MEDIA_TYPES[i].clsMinorType == mediaType->subtype)
    {
      m_streamType = STREAM_TYPES[i];
      if (oldStreamType != m_streamType && oldStreamType != STREAM_TYPE_UNKNOWN)
      {
        LogDebug(L"input: pin %d change media type, %d => %d", m_pinId, oldStreamType, m_streamType);
        m_multiplexer->StreamTypeChange(this, oldStreamType, m_streamType);
        m_receiveTickCount = NOT_RECEIVING;
      }
      CPacketSync::Reset();
      m_tsReceiveBufferOffset = 0;
      return S_OK;
    }
  }
  return VFW_E_TYPE_NOT_ACCEPTED;
}

byte CMuxInputPin::GetId()
{
  return m_pinId;
}

byte CMuxInputPin::GetStreamType()
{
  return m_streamType;
}

DWORD CMuxInputPin::GetReceiveTickCount()
{
  return m_receiveTickCount;
}

HRESULT CMuxInputPin::StartDumping(wchar_t* fileName)
{
  if (!m_isDumpEnabled)
  {
    LogDebug(L"input: pin %d start dumping", m_pinId);
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.SetFileName(fileName);
    if (!SUCCEEDED(hr))
    {
      LogDebug(L"input: pin %d failed to set dump file name, path/name = %s, hr = 0x%x", m_pinId, fileName, hr);
      return hr;
    }
    hr = m_dumpFileWriter.OpenFile();
    if (!SUCCEEDED(hr))
    {
      LogDebug(L"input: pin %d failed to open dump file, hr = 0x%x", m_pinId, hr);
      return hr;
    }
    m_isDumpEnabled = true;
  }
  return S_OK;
}

HRESULT CMuxInputPin::StopDumping()
{
  if (m_isDumpEnabled)
  {
    LogDebug(L"input: pin %d stop dumping", m_pinId);
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.CloseFile();
    if (!SUCCEEDED(hr))
    {
      LogDebug(L"input: pin %d failed to close dump file, hr = 0x%x", m_pinId, hr);
      return hr;
    }
    m_isDumpEnabled = false;
  }
  return S_OK;
}

void CMuxInputPin::OnTsPacket(byte* tsPacket)
{
  memcpy(&m_tsReceiveBuffer[m_tsReceiveBufferOffset], tsPacket, TS_PACKET_LEN);
  m_tsReceiveBufferOffset += TS_PACKET_LEN;
  if (m_tsReceiveBufferOffset == RECEIVE_BUFFER_SIZE)
  {
    m_multiplexer->Receive(this, m_tsReceiveBuffer, RECEIVE_BUFFER_SIZE, -1);
    m_tsReceiveBufferOffset = 0;
  }
}