/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "StdAfx.h"

#include "OutputPinMediaSample.h"

COutputPinMediaSample::COutputPinMediaSample(CBaseAllocator *allocator, HRESULT *hr)
  : CMediaSample(L"MediaPortal Url Source Splitter Output Pin media sample", allocator, hr)
{
  this->SetPointer(NULL, 0);
}

COutputPinMediaSample::~COutputPinMediaSample(void)
{
  // destroy buffer and set length to zero
  FREE_MEM(this->m_pBuffer);
  this->SetPointer(NULL, 0);
}

// IUnknown interface implementation

STDMETHODIMP COutputPinMediaSample::QueryInterface(REFIID riid, void **ppv)
{
  CheckPointer(ppv, E_POINTER);

  *ppv = NULL;

  return
    QI(IOutputPinMediaSample)
    CMediaSample::QueryInterface(riid, ppv);
}

STDMETHODIMP_(ULONG) COutputPinMediaSample::AddRef()
{
  return CMediaSample::AddRef();
}

STDMETHODIMP_(ULONG) COutputPinMediaSample::Release()
{
  // decrement our own private reference count
  LONG lRef;
  if (m_cRef == 1)
  {
    lRef = 0;
    m_cRef = 0;
  }
  else
  {
    lRef = InterlockedDecrement(&m_cRef);
  }
  ASSERT(lRef >= 0);

  if (lRef == 0)
  {
    // we released our final reference count
    // free all resources
    if (m_dwFlags & Sample_TypeChanged)
    {
      this->SetMediaType(NULL);
    }
    ASSERT(m_pMediaType == NULL);

    m_dwFlags = 0;
    m_dwTypeSpecificFlags = 0;
    m_dwStreamId = AM_STREAM_MEDIA;
    
    // destroy buffer and set length to zero
    FREE_MEM(this->m_pBuffer);
    this->SetPointer(NULL, 0);

    // this may cause us to be deleted
    // our refcount is reliably 0 thus no-one will mess with us
    this->m_pAllocator->ReleaseBuffer(this);
  }

  return (ULONG)lRef;
}

// IOutputPinMediaSample interface implementation

STDMETHODIMP COutputPinMediaSample::SetPacket(COutputPinPacket *packet)
{
  HRESULT result = S_OK;
  // destroy buffer and set length to zero
  FREE_MEM(this->m_pBuffer);
  this->SetPointer(NULL, 0);

  CHECK_POINTER_HRESULT(result, packet, result, E_POINTER);

  if (SUCCEEDED(result))
  {
    unsigned int bufferSize = packet->GetBuffer()->GetBufferSize();
    unsigned int bufferSizeOccupied = packet->GetBuffer()->GetBufferOccupiedSpace();

    this->m_pBuffer = ALLOC_MEM_SET(this->m_pBuffer, BYTE, bufferSize, 0);
    CHECK_POINTER_HRESULT(result, this->m_pBuffer, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      this->SetPointer(this->m_pBuffer, bufferSize);
      result = COutputPinMediaSample::SetPacket(this, packet);
    }
  }

  return result;
}

/* get methods */

/* set methods */

/* other methods */

STDMETHODIMP COutputPinMediaSample::SetPacket(IMediaSample *mediaSample, COutputPinPacket *packet)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, mediaSample);
  CHECK_POINTER_DEFAULT_HRESULT(result, packet);

  if (SUCCEEDED(result))
  {
    BYTE *buffer = NULL;

    result = mediaSample->GetPointer(&buffer);

    if (SUCCEEDED(result))
    {
      unsigned int bufferSizeOccupied = min((unsigned long)mediaSample->GetSize(), packet->GetBuffer()->GetBufferOccupiedSpace());

      packet->GetBuffer()->CopyFromBuffer(buffer, bufferSizeOccupied);

      bool timeValid = packet->GetStartTime() != COutputPinPacket::INVALID_TIME;
      REFERENCE_TIME rtStart = packet->GetStartTime();
      REFERENCE_TIME rtEnd = packet->GetEndTime();

      CHECK_HRESULT_EXECUTE(result, mediaSample->SetActualDataLength(bufferSizeOccupied));
      CHECK_HRESULT_EXECUTE(result, mediaSample->SetTime(timeValid ? &rtStart : NULL, timeValid ? &rtEnd : NULL));
      CHECK_HRESULT_EXECUTE(result, mediaSample->SetMediaTime(NULL, NULL));
      CHECK_HRESULT_EXECUTE(result, mediaSample->SetDiscontinuity(packet->IsDiscontinuity()));
      CHECK_HRESULT_EXECUTE(result, mediaSample->SetSyncPoint(packet->IsSyncPoint()));
      CHECK_HRESULT_EXECUTE(result, mediaSample->SetPreroll(timeValid && (packet->GetStartTime() < 0)));

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = bufferSizeOccupied);
    }
  }

  return result;
}