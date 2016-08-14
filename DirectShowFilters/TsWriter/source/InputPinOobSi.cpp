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
#include "InputPinOobSi.h"
#include <algorithm>    // min()
#include <cstddef>      // NULL
#include <cstring>      // memcpy()

using namespace std;


extern void LogDebug(const wchar_t* fmt, ...);

CInputPinOobSi::CInputPinOobSi(ITsAnalyser* analyser,
                                CBaseFilter* filter,
                                CCritSec* filterLock,
                                CCritSec& receiveLock,
                                HRESULT* hr)
  : CRenderedInputPin(NAME("OOB SI Input"), filter, filterLock, hr, L"OOB SI Input"),
    m_receiveLock(receiveLock)
{
  if (analyser == NULL)
  {
    LogDebug(L"OOB SI input: analyser not supplied");
    *hr = E_INVALIDARG;
    return;
  }

  m_analyser = analyser;
  m_receiveTime = NOT_RECEIVING;
  m_isDumpEnabled = false;
  m_enableCrcCheck = true;
}

CInputPinOobSi::~CInputPinOobSi()
{
  StopDumping();
}

HRESULT CInputPinOobSi::BreakConnect()
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"OOB SI input: break connect");
  m_receiveTime = NOT_RECEIVING;
  return CRenderedInputPin::BreakConnect();
}

HRESULT CInputPinOobSi::CheckMediaType(const CMediaType* mediaType)
{
  for (unsigned char i = 0; i < INPUT_MEDIA_TYPE_COUNT_OOB_SI; i++)
  {
    if (
      *INPUT_MEDIA_TYPES_OOB_SI[i].clsMajorType == mediaType->majortype &&
      *INPUT_MEDIA_TYPES_OOB_SI[i].clsMinorType == mediaType->subtype
    )
    {
      return S_OK;
    }
  }
  return VFW_E_TYPE_NOT_ACCEPTED;
}

HRESULT CInputPinOobSi::CompleteConnect(IPin* receivePin)
{
  CAutoLock lock(&m_receiveLock);
  LogDebug(L"OOB SI input: complete connect");
  return CRenderedInputPin::CompleteConnect(receivePin);
}

HRESULT CInputPinOobSi::GetMediaType(int position, CMediaType* mediaType)
{
  if (position < 0)
  {
    return E_INVALIDARG;
  }
  if (position >= INPUT_MEDIA_TYPE_COUNT_OOB_SI)
  {
    return VFW_S_NO_MORE_ITEMS;
  }

  mediaType->ResetFormatBuffer();
  mediaType->formattype = FORMAT_None;
  mediaType->InitMediaType();
  mediaType->majortype = *INPUT_MEDIA_TYPES_OOB_SI[position].clsMajorType;
  mediaType->subtype = *INPUT_MEDIA_TYPES_OOB_SI[position].clsMinorType;
  return S_OK;
}

STDMETHODIMP CInputPinOobSi::Receive(IMediaSample* sample)
{
  try
  {
    if (sample == NULL)
    {
      LogDebug(L"OOB SI input: received NULL sample");
      return E_INVALIDARG;
    }

    CAutoLock lock(&m_receiveLock);
    if (IsStopped())
    {
      LogDebug(L"OOB SI input: received sample when stopped");
      return VFW_E_NOT_RUNNING;
    }

    if (m_bAtEndOfStream)
    {
      LogDebug(L"OOB SI input: received sample after end of stream");
      return VFW_E_SAMPLE_REJECTED_EOS;
    }

    long sampleLength = sample->GetActualDataLength();
    if (sampleLength <= 1)
    {
      return S_OK;
    }

    unsigned char* data = NULL;
    HRESULT hr = sample->GetPointer(&data);
    if (FAILED(hr) || data == NULL)
    {
      LogDebug(L"OOB SI input: failed to get sample pointer, hr = 0x%x", hr);
      return E_POINTER;
    }

    if (m_receiveTime == NOT_RECEIVING)
    {
      LogDebug(L"OOB SI input: stream started");
    }
    m_receiveTime = clock();

    if (m_isDumpEnabled)
    {
      CAutoLock lock(&m_dumpLock);
      if (!m_dumpFileWriter.IsFileInvalid())
      {
        LogDebug(L"OOB SI input: dumping %ld bytes", sampleLength);
        m_dumpFileWriter.Write((unsigned char*)&sampleLength, 4);
        m_dumpFileWriter.Write(data, sampleLength);
      }
    }
    
    CSection s;
    s.AppendData(data, min(sizeof(s.Data), sampleLength));
    if (!s.IsComplete())
    {
      LogDebug(L"OOB SI input: received incomplete section sample, sample length = %ld, section length = %d",
                sampleLength, s.section_length);
    }
    else if (m_enableCrcCheck && !s.IsValid())
    {
      LogDebug(L"OOB SI input: received invalid section");
    }
    else
    {
      m_analyser->AnalyseOobSiSection(s);
    }
    return S_OK;
  }
  catch (...)
  {
    LogDebug(L"OOB SI input: unhandled exception in Receive()");
    return E_FAIL;
  }
}

STDMETHODIMP CInputPinOobSi::ReceiveCanBlock()
{
  return S_FALSE;
}

HRESULT CInputPinOobSi::Run(REFERENCE_TIME startTime)
{
  m_receiveTime = NOT_RECEIVING;
  return CRenderedInputPin::Run(startTime);
}

clock_t CInputPinOobSi::GetReceiveTime()
{
  return m_receiveTime;
}

HRESULT CInputPinOobSi::StartDumping(const wchar_t* fileName)
{
  if (!m_isDumpEnabled)
  {
    LogDebug(L"OOB SI input: start dumping, file name = %s",
              fileName == NULL ? L"" : fileName);
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.OpenFile(fileName);
    if (FAILED(hr))
    {
      LogDebug(L"OOB SI input: failed to open dump file, hr = 0x%x, path/name = %s",
                hr, fileName == NULL ? L"" : fileName);
      return hr;
    }
    m_isDumpEnabled = true;
  }
  return S_OK;
}

HRESULT CInputPinOobSi::StopDumping()
{
  if (m_isDumpEnabled)
  {
    LogDebug(L"OOB SI input: stop dumping");
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.CloseFile();
    if (FAILED(hr))
    {
      LogDebug(L"OOB SI input: failed to close dump file, hr = 0x%x", hr);
      return hr;
    }
    m_isDumpEnabled = false;
  }
  return S_OK;
}

void CInputPinOobSi::CheckSectionCrcs(bool enable)
{
  m_enableCrcCheck = enable;
}