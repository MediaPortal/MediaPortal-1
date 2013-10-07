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

// TODO debug
#include <shlobj.h>
#include <iostream>

extern void LogDebug(const wchar_t* fmt, ...) ;

CTsOutputPin::CTsOutputPin(CBaseFilter* filter, CCritSec* filterLock, HRESULT* hr)
  : CBaseOutputPin(NAME("TS Output"), filter, filterLock, hr, L"TS Output")
{
  m_isConnected = false;

  // TODO debug
  /*wchar_t appDataFolderName[MAX_PATH];
  ::SHGetSpecialFolderPathW(NULL, appDataFolderName, CSIDL_COMMON_APPDATA, FALSE);
  wchar_t tsFileName[MAX_PATH];
  swprintf_s(tsFileName, L"%s\\Team MediaPortal\\MediaPortal TV Server\\timeshiftbuffer\\test.ts", appDataFolderName);
  m_debugOutFile.open(tsFileName, std::ios::out | std::ios::binary);*/
}

HRESULT CTsOutputPin::BreakConnect()
{
  m_isConnected = false;
  return CBaseOutputPin::BreakConnect();
}

HRESULT CTsOutputPin::CheckConnect(IPin* receivePin)
{
  return CBaseOutputPin::CheckConnect(receivePin);
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

HRESULT CTsOutputPin::CompleteConnect(IPin* receivePin)
{
  m_isConnected = true;
  return CBaseOutputPin::CompleteConnect(receivePin);
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
    LogDebug(L"CTsOutputPin: failed to set allocator properties, hr = 0x%x", hr);
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
  // TODO debug
  /*if (m_debugBytes > 0 && m_debugOutFile.is_open())
  {
    m_debugOutFile.write((char*)data, dataLength);
    //m_debugBytes -= m_debugBytes;
    if (m_debugBytes <= 0)
    {
      m_debugOutFile.close();
    }
  }*/

  IMediaSample* sample;
  HRESULT hr = GetDeliveryBuffer(&sample, NULL, NULL, 0);
  hr |= sample->SetActualDataLength(dataLength);
  PBYTE sampleBuffer;
  hr |= sample->GetPointer(&sampleBuffer);
  memcpy(sampleBuffer, data, dataLength);
  sample->Release();

  hr |= CBaseOutputPin::Deliver(sample);
  if (hr != 0)
  {
    LogDebug(L"CTsOutputPin: failed to deliver, hr = 0x%x", hr);
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

bool CTsOutputPin::IsConnected()
{
  return m_isConnected;
}