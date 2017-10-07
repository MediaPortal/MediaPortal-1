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
#include <cstddef>      // NULL
#include <cstring>      // memcpy()
#include <cwchar>       // wcscmp()

using namespace std;


extern void LogDebug(const wchar_t* fmt, ...);

CMuxInputPin::CMuxInputPin(unsigned char id,
                            IStreamMultiplexer* multiplexer,
                            CBaseFilter* filter,
                            CCritSec* filterLock,
                            CCritSec& receiveLock,
                            HRESULT* hr)
  : CRenderedInputPin(NAME("Input"), filter, filterLock, hr, L"Input"),
    m_receiveLock(receiveLock)
{
  if (multiplexer == NULL)
  {
    LogDebug(L"input: pin %hhu multiplexer not supplied", id);
    *hr = E_INVALIDARG;
    return;
  }

  m_pinId = id;
  m_isRdsConnectionAllowed = false;
  m_streamType = STREAM_TYPE_UNKNOWN;
  m_receiveTime = NOT_RECEIVING;
  m_multiplexer = multiplexer;
  m_tsReceiveBufferOffset = 0;
  m_isDumpEnabled = false;
}

CMuxInputPin::~CMuxInputPin()
{
  StopDumping();
}

HRESULT CMuxInputPin::BreakConnect()
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"input: pin %hhu break connect", m_pinId);
  m_streamType = STREAM_TYPE_UNKNOWN;
  m_receiveTime = NOT_RECEIVING;
  HRESULT hr = m_multiplexer->BreakConnect(this);
  if (SUCCEEDED(hr))
  {
    hr = CRenderedInputPin::BreakConnect();
  }
  return hr;
}

HRESULT CMuxInputPin::CheckConnect(IPin* receivePin)
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"input: pin %hhu check connect", m_pinId);
  HRESULT hr = CRenderedInputPin::CheckConnect(receivePin);
  if (!SUCCEEDED(hr))
  {
    return hr;
  }

  m_isRdsConnectionAllowed = false;
  if (receivePin != NULL)
  {
    PIN_INFO pinInfo;
    hr = receivePin->QueryPinInfo(&pinInfo);
    if (SUCCEEDED(hr))
    {
      QueryPinInfoReleaseFilter(pinInfo);
      m_isRdsConnectionAllowed = wcscmp(L"RDSOutput", (wchar_t*)&(pinInfo.achName)) == 0;
    }
  }
  LogDebug(L"input: pin %hhu is RDS connection allowed = %d",
            m_pinId, m_isRdsConnectionAllowed);
  return S_OK;
}

HRESULT CMuxInputPin::CheckMediaType(const CMediaType* mediaType)
{
  /*const GUID& mt = mediaType->majortype;
  const GUID& st = mediaType->subtype;
  LogDebug(L"input: pin %hhu check media type, major type = %08x-%04hx-%04hx-%02hhx%02hhx-%02hhx%02hhx%02hhx%02hhx%02hhx%02hhx, sub type = %08x-%04hx-%04hx-%02hhx%02hhx-%02hhx%02hhx%02hhx%02hhx%02hhx%02hhx",
            m_pinId, mt.Data1, mt.Data2, mt.Data3, mt.Data4[0], mt.Data4[1],
            mt.Data4[2], mt.Data4[3], mt.Data4[4], mt.Data4[5], mt.Data4[6],
            mt.Data4[7], st.Data1, st.Data2, st.Data3, st.Data4[0],
            st.Data4[1], st.Data4[2], st.Data4[3], st.Data4[4], st.Data4[5],
            st.Data4[6], st.Data4[7]);*/
  unsigned char mediaTypeCount = INPUT_MEDIA_TYPE_COUNT;
  if (!m_isRdsConnectionAllowed)
  {
    mediaTypeCount -= INPUT_MEDIA_TYPE_COUNT_RDS;
  }
  for (unsigned char i = 0; i < mediaTypeCount; i++)
  {
    if (
      *INPUT_MEDIA_TYPES[i].clsMajorType == mediaType->majortype &&
      *INPUT_MEDIA_TYPES[i].clsMinorType == mediaType->subtype
    )
    {
      return S_OK;
    }
  }
  return VFW_E_TYPE_NOT_ACCEPTED;
}

HRESULT CMuxInputPin::CompleteConnect(IPin* receivePin)
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"input: pin %hhu complete connect", m_pinId);
  m_isRdsConnectionAllowed = false;
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

STDMETHODIMP CMuxInputPin::Receive(IMediaSample* sample)
{
  try
  {
    if (sample == NULL)
    {
      LogDebug(L"input: pin %hhu received NULL sample", m_pinId);
      return E_INVALIDARG;
    }

    CAutoLock lock(&m_receiveLock);
    if (IsStopped())
    {
      LogDebug(L"input: pin %hhu received sample when stopped", m_pinId);
      return VFW_E_NOT_RUNNING;
    }

    if (m_bAtEndOfStream)
    {
      LogDebug(L"input: pin %hhu received sample after end of stream", m_pinId);
      return VFW_E_SAMPLE_REJECTED_EOS;
    }

    long sampleLength = sample->GetActualDataLength();
    if (sampleLength <= 0)
    {
      return S_OK;
    }

    unsigned char* data = NULL;
    HRESULT hr = sample->GetPointer(&data);
    if (FAILED(hr) || data == NULL)
    {
      LogDebug(L"input: pin %hhu failed to get sample pointer, hr = 0x%x",
                m_pinId, hr);
      return E_POINTER;
    }

    REFERENCE_TIME startTime;
    REFERENCE_TIME stopTime;
    hr = sample->GetTime(&startTime, &stopTime);
    if (FAILED(hr))   // Seems to mean that this sample is a continuation of a previous frame.
    {
      startTime = -1;
    }
    /*else if (m_receiveTime == NOT_RECEIVING)
    {
      LogDebug(L"input: pin %hhu receive, sample length = %ld, start time = %lld, stop time = %lld",
                m_pinId, sampleLength, startTime, stopTime);
    }*/

    if (m_receiveTime == NOT_RECEIVING)
    {
      LogDebug(L"input: pin %hhu stream started", m_pinId);
    }
    m_receiveTime = clock();
    if (m_isDumpEnabled)
    {
      CAutoLock lock(&m_dumpLock);
      if (m_dumpFileWriter.IsFileOpen())
      {
        LogDebug(L"input: pin %hhu dumping %ld bytes, start time = %lld, stop time = %lld",
                  m_pinId, sampleLength, startTime, stopTime);
        m_dumpFileWriter.Write(data, sampleLength);
      }
    }
    if (m_streamType == STREAM_TYPE_MPEG2_TRANSPORT_STREAM)
    {
      OnRawData(data, sampleLength);
      return S_OK;
    }
    return m_multiplexer->Receive(this, data, sampleLength, startTime);
  }
  catch (...)
  {
    LogDebug(L"input: pin %hhu unhandled exception in Receive()", m_pinId);
    return E_FAIL;
  }
}

STDMETHODIMP CMuxInputPin::ReceiveCanBlock()
{
  return S_FALSE;
}

HRESULT CMuxInputPin::Run(REFERENCE_TIME startTime)
{
  m_receiveTime = NOT_RECEIVING;
  return CRenderedInputPin::Run(startTime);
}

HRESULT CMuxInputPin::SetMediaType(const CMediaType* mediaType)
{
  CAutoLock lock(&m_receiveLock);
  /*LogDebug(L"debug: pin %hhu set media type %08x-%04hx-%04hx-%02hhx%02hhx-%02hhx%02hhx%02hhx%02hhx%02hhx%02hhx  %08x-%04hx-%04hx-%02hx%02hx-%02hhx%02hhx%02hhx%02hhx%02hhx%02hhx",
      m_pinId, mediaType->majortype.Data1, mediaType->majortype.Data2,
      mediaType->majortype.Data3, mediaType->majortype.Data4[0],
      mediaType->majortype.Data4[1], mediaType->majortype.Data4[2],
      mediaType->majortype.Data4[3], mediaType->majortype.Data4[4],
      mediaType->majortype.Data4[5], mediaType->majortype.Data4[6],
      mediaType->majortype.Data4[7], mediaType->subtype.Data1,
      mediaType->subtype.Data2, mediaType->subtype.Data3,
      mediaType->subtype.Data4[0], mediaType->subtype.Data4[1],
      mediaType->subtype.Data4[2], mediaType->subtype.Data4[3],
      mediaType->subtype.Data4[4], mediaType->subtype.Data4[5],
      mediaType->subtype.Data4[6], mediaType->subtype.Data4[7]);*/
  unsigned char oldStreamType = m_streamType;
  for (unsigned char i = 0; i < INPUT_MEDIA_TYPE_COUNT; i++)
  {
    if (*INPUT_MEDIA_TYPES[i].clsMajorType == mediaType->majortype &&
      *INPUT_MEDIA_TYPES[i].clsMinorType == mediaType->subtype)
    {
      m_streamType = STREAM_TYPES[i];
      if (oldStreamType != m_streamType && oldStreamType != STREAM_TYPE_UNKNOWN)
      {
        LogDebug(L"input: pin %hhu change media type, %hhu => %hhu",
                  m_pinId, oldStreamType, m_streamType);
        m_multiplexer->StreamTypeChange(this, oldStreamType, m_streamType);
        m_receiveTime = NOT_RECEIVING;
      }
      CPacketSync::Reset();
      m_tsReceiveBufferOffset = 0;
      return CRenderedInputPin::SetMediaType(mediaType);
    }
  }
  return VFW_E_TYPE_NOT_ACCEPTED;
}

unsigned char CMuxInputPin::GetId() const
{
  return m_pinId;
}

unsigned char CMuxInputPin::GetStreamType() const
{
  return m_streamType;
}

clock_t CMuxInputPin::GetReceiveTime() const
{
  return m_receiveTime;
}

HRESULT CMuxInputPin::StartDumping(const wchar_t* fileName)
{
  if (!m_isDumpEnabled)
  {
    LogDebug(L"input: pin %hhu start dumping, file name = %s",
              m_pinId, fileName == NULL ? L"" : fileName);
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.OpenFile(fileName);
    if (FAILED(hr))
    {
      LogDebug(L"input: pin %hhu failed to open dump file, hr = 0x%x, path/name = %s",
                m_pinId, hr, fileName == NULL ? L"" : fileName);
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
    LogDebug(L"input: pin %hhu stop dumping", m_pinId);
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.CloseFile();
    if (FAILED(hr))
    {
      LogDebug(L"input: pin %hhu failed to close dump file, hr = 0x%x",
                m_pinId, hr);
      return hr;
    }
    m_isDumpEnabled = false;
  }
  return S_OK;
}

void CMuxInputPin::OnTsPacket(const unsigned char* tsPacket)
{
  memcpy(&m_tsReceiveBuffer[m_tsReceiveBufferOffset], tsPacket, TS_PACKET_LEN);
  m_tsReceiveBufferOffset += TS_PACKET_LEN;
  if (m_tsReceiveBufferOffset == RECEIVE_BUFFER_SIZE)
  {
    m_multiplexer->Receive(this, m_tsReceiveBuffer, RECEIVE_BUFFER_SIZE, -1);
    m_tsReceiveBufferOffset = 0;
  }
}