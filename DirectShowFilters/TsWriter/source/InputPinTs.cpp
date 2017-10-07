/*
 *  Copyright (C) 2006-2008 Team MediaPortal
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
#include "InputPinTs.h"


extern void LogDebug(const wchar_t* fmt, ...);

CInputPinTs::CInputPinTs(ITsAnalyser* analyser,
                          CBaseFilter* filter,
                          CCritSec* filterLock,
                          CCritSec& receiveLock,
                          HRESULT* hr)
  : CRenderedInputPin(NAME("TS Input"), filter, filterLock, hr, L"TS Input"),
    m_receiveLock(receiveLock)
{
  if (analyser == NULL)
  {
    LogDebug(L"TS input: analyser not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_analyser = analyser;
  m_receiveTime = NOT_RECEIVING;
  m_isDumpEnabled = false;
}

CInputPinTs::~CInputPinTs()
{
  StopDumping();
}

HRESULT CInputPinTs::BreakConnect()
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"TS input: break connect");
  m_receiveTime = NOT_RECEIVING;
  return CRenderedInputPin::BreakConnect();
}

HRESULT CInputPinTs::CheckMediaType(const CMediaType* mediaType)
{
  /*const GUID& mt = mediaType->majortype;
  const GUID& st = mediaType->subtype;
  LogDebug(L"TS input: check media type, major type = %08x-%04hx-%04hx-%02hhx%02hhx-%02hhx%02hhx%02hhx%02hhx%02hhx%02hhx, sub type = %08x-%04hx-%04hx-%02hhx%02hhx-%02hhx%02hhx%02hhx%02hhx%02hhx%02hhx",
            mt.Data1, mt.Data2, mt.Data3, mt.Data4[0], mt.Data4[1],
            mt.Data4[2], mt.Data4[3], mt.Data4[4], mt.Data4[5], mt.Data4[6],
            mt.Data4[7], st.Data1, st.Data2, st.Data3, st.Data4[0],
            st.Data4[1], st.Data4[2], st.Data4[3], st.Data4[4], st.Data4[5],
            st.Data4[6], st.Data4[7]);*/
  for (unsigned char i = 0; i < INPUT_MEDIA_TYPE_COUNT_TS; i++)
  {
    if (
      *INPUT_MEDIA_TYPES_TS[i].clsMajorType == mediaType->majortype &&
      *INPUT_MEDIA_TYPES_TS[i].clsMinorType == mediaType->subtype
    )
    {
      return S_OK;
    }
  }
  return VFW_E_TYPE_NOT_ACCEPTED;
}

HRESULT CInputPinTs::CompleteConnect(IPin* receivePin)
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"TS input: complete connect");
  CPacketSync::Reset();
  return CRenderedInputPin::CompleteConnect(receivePin);
}

HRESULT CInputPinTs::GetMediaType(int position, CMediaType* mediaType)
{
  if (position < 0)
  {
    return E_INVALIDARG;
  }
  if (position >= INPUT_MEDIA_TYPE_COUNT_TS)
  {
    return VFW_S_NO_MORE_ITEMS;
  }

  mediaType->ResetFormatBuffer();
  mediaType->formattype = FORMAT_None;
  mediaType->InitMediaType();
  mediaType->majortype = *INPUT_MEDIA_TYPES_TS[position].clsMajorType;
  mediaType->subtype = *INPUT_MEDIA_TYPES_TS[position].clsMinorType;
  return S_OK;
}

STDMETHODIMP CInputPinTs::Receive(IMediaSample* sample)
{
  try
  {
    if (sample == NULL)
    {
      LogDebug(L"TS input: received NULL sample");
      return E_INVALIDARG;
    }

    CAutoLock lock(&m_receiveLock);
    if (IsStopped())
    {
      LogDebug(L"TS input: received sample when stopped");
      return VFW_E_NOT_RUNNING;
    }

    if (m_bAtEndOfStream)
    {
      LogDebug(L"TS input: received sample after end of stream");
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
      LogDebug(L"TS input: failed to get sample pointer, hr = 0x%x", hr);
      return E_POINTER;
    }

    if (m_receiveTime == NOT_RECEIVING)
    {
      LogDebug(L"TS input: stream started");
    }
    m_receiveTime = clock();

    if (m_isDumpEnabled)
    {
      CAutoLock lock(&m_dumpLock);
      if (m_dumpFileWriter.IsFileOpen())
      {
        LogDebug(L"TS input: dumping %ld bytes", sampleLength);
        m_dumpFileWriter.Write(data, sampleLength);
      }
    }

    OnRawData(data, sampleLength);
    return S_OK;
  }
  catch (...)
  {
    LogDebug(L"TS input: unhandled exception in Receive()");
    return E_FAIL;
  }
}

STDMETHODIMP CInputPinTs::ReceiveCanBlock()
{
  return S_FALSE;
}

HRESULT CInputPinTs::Run(REFERENCE_TIME startTime)
{
  m_receiveTime = NOT_RECEIVING;
  return CRenderedInputPin::Run(startTime);
}

void CInputPinTs::OnTsPacket(const unsigned char* tsPacket)
{
  m_analyser->AnalyseTsPacket(tsPacket);
}

clock_t CInputPinTs::GetReceiveTime()
{
  return m_receiveTime;
}

HRESULT CInputPinTs::StartDumping(const wchar_t* fileName)
{
  if (!m_isDumpEnabled)
  {
    LogDebug(L"TS input: start dumping, file name = %s",
              fileName == NULL ? L"" : fileName);
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.OpenFile(fileName);
    if (FAILED(hr))
    {
      LogDebug(L"TS input: failed to open dump file, hr = 0x%x, path/name = %s",
                hr, fileName == NULL ? L"" : fileName);
      return hr;
    }
    m_isDumpEnabled = true;
  }
  return S_OK;
}

HRESULT CInputPinTs::StopDumping()
{
  if (m_isDumpEnabled)
  {
    LogDebug(L"TS input: stop dumping");
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.CloseFile();
    if (FAILED(hr))
    {
      LogDebug(L"TS input: failed to close dump file, hr = 0x%x", hr);
      return hr;
    }
    m_isDumpEnabled = false;
  }
  return S_OK;
}