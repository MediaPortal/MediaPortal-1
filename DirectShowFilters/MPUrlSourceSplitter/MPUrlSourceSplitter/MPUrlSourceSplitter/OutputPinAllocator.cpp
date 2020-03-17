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

#include "OutputPinAllocator.h"
#include "OutputPinMediaSample.h"

COutputPinAllocator::COutputPinAllocator(LPUNKNOWN pUnk, HRESULT *hr)
  : CBaseAllocator(L"MediaPortal Url Source Splitter Output Pin allocator", pUnk, hr)
{
}

COutputPinAllocator::~COutputPinAllocator(void)
{
  this->Decommit();
  this->FreeAllSamples();
}

STDMETHODIMP COutputPinAllocator::SetProperties(__in ALLOCATOR_PROPERTIES* pRequest, __out ALLOCATOR_PROPERTIES* pActual)
{
  HRESULT result = S_OK;

  CHECK_POINTER_DEFAULT_HRESULT(result, pRequest);
  CHECK_POINTER_DEFAULT_HRESULT(result, pActual);

  if (SUCCEEDED(result))
  {
    CHECK_CONDITION_HRESULT(result, (pRequest->cbBuffer > 0), result, E_INVALIDARG);
    CHECK_CONDITION_HRESULT(result, (pRequest->cbAlign != ALLOCATOR_ALIGNMENT_REQUIRED), VFW_E_BADALIGN, result);
    CHECK_CONDITION_HRESULT(result, (pRequest->cbPrefix == ALLOCATOR_PREFIX_REQUIRED), result, VFW_E_BADALIGN);
  }

  if (SUCCEEDED(result))
  {
    result = __super::SetProperties(pRequest, pActual);
  }

  return result;
}

HRESULT COutputPinAllocator::Alloc(void)
{
  CAutoLock lck(this);

  /* Check he has called SetProperties */
  HRESULT result = CBaseAllocator::Alloc();

  if (SUCCEEDED(result) && (result != S_FALSE))
  {
    // everything is OK and the requirements haven't changed

    // if there are some media samples allocated, then free them
    if (this->m_lAllocated != 0)
    {
      this->FreeAllSamples();
    }

    // make sure we've got reasonable values
    CHECK_CONDITION_HRESULT(result, ((this->m_lSize < 0) || (this->m_lPrefix < 0) || (this->m_lCount < 0)), E_OUTOFMEMORY, result);

    COutputPinMediaSample *sample = NULL;
    for (this->m_lAllocated = 0; (SUCCEEDED(result) && (this->m_lAllocated < this->m_lCount)); this->m_lAllocated++)
    {
      sample = new COutputPinMediaSample(this, &result);
      CHECK_POINTER_HRESULT(result, sample, result, E_OUTOFMEMORY);

      // this not fail, sample is added to head of list and automatically linked to list
      this->m_lFree.Add(sample);
    }
  }

  return result;
}

void COutputPinAllocator::Free(void)
{
}

void COutputPinAllocator::FreeAllSamples(void)
{
  // should never be deleting this unless all buffers are freed
  ASSERT(this->m_lAllocated == this->m_lFree.GetCount());

  // free up all the media samples
  COutputPinMediaSample *sample = NULL;
  while (this->m_lFree.GetCount() != 0)
  {
    sample = static_cast<COutputPinMediaSample *>(this->m_lFree.RemoveHead());
    FREE_MEM_CLASS(sample);
  }

  this->m_lAllocated = 0;
}
