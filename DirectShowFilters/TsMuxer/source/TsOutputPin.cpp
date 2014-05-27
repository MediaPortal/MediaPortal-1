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
#include "TsOutPutPin.h"


extern void LogDebug(const wchar_t* fmt, ...);

CTsOutputPin::CTsOutputPin(CBaseFilter* filter, CCritSec* filterLock, HRESULT* hr)
  : CBaseOutputPin(NAME("TS Output"), filter, filterLock, hr, L"TS Output")
{
  m_isDumpEnabled = false;
}

CTsOutputPin::~CTsOutputPin(void)
{
  StopDumping();
}

HRESULT CTsOutputPin::CheckMediaType(const CMediaType* mediaType)
{
  /*for (int i = 0; i < OUTPUT_MEDIA_TYPE_COUNT; i++)
  {
    if (*OUTPUT_MEDIA_TYPES[i].clsMajorType == mediaType->majortype &&
      *OUTPUT_MEDIA_TYPES[i].clsMinorType == mediaType->subtype)
    {
      return S_OK;
    }
  }
  return VFW_E_TYPE_NOT_ACCEPTED;*/
  return S_OK;
}

HRESULT CTsOutputPin::DecideBufferSize(IMemAllocator* allocator, ALLOCATOR_PROPERTIES* properties)
{
  CheckPointer(allocator, E_POINTER);
  CheckPointer(properties, E_POINTER);

  if (properties->cBuffers == 0)
  {
    properties->cBuffers = 30;
  }

  properties->cbBuffer = 256000;

  ALLOCATOR_PROPERTIES actualProperties;
  HRESULT hr = allocator->SetProperties(properties, &actualProperties);
  if (FAILED(hr))
  {
    LogDebug(L"TS output: failed to set allocator properties, hr = 0x%x", hr);
    return hr;
  }

  if (actualProperties.cbBuffer < properties->cbBuffer)
  {
    return E_FAIL;
  }
  return S_OK;
}

HRESULT CTsOutputPin::Deliver(PBYTE data, long dataLength)
{
  if (m_isDumpEnabled)
  {
    CAutoLock lock(&m_dumpLock);
    if (!m_dumpFileWriter.IsFileInvalid())
    {
      LogDebug(L"TS output: dumping %d bytes", dataLength);
      m_dumpFileWriter.Write(data, dataLength);
    }
  }

  IMediaSample* sample;
  HRESULT hr = GetDeliveryBuffer(&sample, NULL, NULL, 0);
  if (!SUCCEEDED(hr))
  {
    LogDebug(L"TS output: failed to get delivery buffer, hr = 0x%x", hr);
    return hr;
  }
  hr = sample->SetActualDataLength(dataLength);
  if (!SUCCEEDED(hr))
  {
    LogDebug(L"TS output: failed to set actual data length, hr = 0x%x", hr);
    return hr;
  }
  PBYTE sampleBuffer;
  hr = sample->GetPointer(&sampleBuffer);
  if (!SUCCEEDED(hr))
  {
    LogDebug(L"TS output: failed to get sample pointer, hr = 0x%x", hr);
    return hr;
  }
  memcpy(sampleBuffer, data, dataLength);

  hr = CBaseOutputPin::Deliver(sample);
  sample->Release();
  if (hr != S_OK)
  {
    LogDebug(L"TS output: failed to deliver sample, hr = 0x%x", hr);
  }
  return hr;
}

HRESULT CTsOutputPin::DeliverEndOfStream()
{
  return S_OK;
}

HRESULT CTsOutputPin::GetMediaType(int position, CMediaType* mediaType)
{
  if (position < 0)
  {
    return E_INVALIDARG;
  }
  if (position >= OUTPUT_MEDIA_TYPE_COUNT)
  {
    return VFW_S_NO_MORE_ITEMS;
  }

  mediaType->ResetFormatBuffer();
  mediaType->formattype = FORMAT_None;
  mediaType->InitMediaType();
  mediaType->majortype = *OUTPUT_MEDIA_TYPES[position].clsMajorType;
  mediaType->subtype = *OUTPUT_MEDIA_TYPES[position].clsMinorType;
  return S_OK;
}

HRESULT CTsOutputPin::StartDumping(wchar_t* fileName)
{
  if (!m_isDumpEnabled)
  {
    LogDebug(L"TS output: start dumping");
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.SetFileName(fileName);
    if (!SUCCEEDED(hr))
    {
      LogDebug(L"TS output: failed to set dump file name, path/name = %s, hr = 0x%x", fileName, hr);
      return hr;
    }
    hr = m_dumpFileWriter.OpenFile();
    if (!SUCCEEDED(hr))
    {
      LogDebug(L"TS output: failed to open dump file, hr = 0x%x", hr);
      return hr;
    }
    m_isDumpEnabled = true;
  }
  return S_OK;
}

HRESULT CTsOutputPin::StopDumping()
{
  if (m_isDumpEnabled)
  {
    LogDebug(L"TS output: stop dumping");
    CAutoLock lock(&m_dumpLock);
    HRESULT hr = m_dumpFileWriter.CloseFile();
    if (!SUCCEEDED(hr))
    {
      LogDebug(L"TS output: failed to close dump file, hr = 0x%x", hr);
      return hr;
    }
    m_isDumpEnabled = false;
  }
  return S_OK;
}