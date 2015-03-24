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

#pragma once

#ifndef __OUTPUT_PIN_ALLOCATOR_DEFINED
#define __OUTPUT_PIN_ALLOCATOR_DEFINED

#include <streams.h>

#define ALLOCATOR_ALIGNMENT_REQUIRED                                  1
#define ALLOCATOR_PREFIX_REQUIRED                                     0

class COutputPinAllocator : public CBaseAllocator
{
public:
  // initializes a new instance of COutputPinAllocator class
  // @param pUnk : pointer to the owner of this object. If the object is aggregated, pass a pointer to the aggregating object's IUnknown interface. Otherwise, set this parameter to NULL.
  // @param hr : pointer to an HRESULT value. Set the value to S_OK before creating the object. If the constructor fails, the value is set to an error code.
  COutputPinAllocator(LPUNKNOWN pUnk, HRESULT *hr);
  ~COutputPinAllocator(void);

  // override to free the memory when decommit completes
  // we actually do nothing, and save the memory until deletion
  void Free(void);

  // called from the destructor (and from Alloc if changing size or count) to actually free up the memory
  void FreeAllSamples(void);

  // overriden to allocate the memory when commit called
  HRESULT Alloc(void);

  // specifies the number of buffers to allocate and the size of each buffer
  // @param pRequest : pointer to an ALLOCATOR_PROPERTIES structure that contains the buffer requirements
  // @param pActual : pointer to an ALLOCATOR_PROPERTIES structure that receives the actual buffer properties
  // @return :
  // S_OK if successful
  // E_POINTER if NULL pointer argument
  // VFW_E_ALREADY_COMMITTED if cannot change allocated memory while the filter is active
  // VFW_E_BADALIGN if an invalid alignment was specified
  // VFW_E_BUFFERS_OUTSTANDING if one or more buffers are still active
  // error code otherwise
  STDMETHODIMP SetProperties(__in ALLOCATOR_PROPERTIES* pRequest, __out ALLOCATOR_PROPERTIES* pActual);

protected:
};

#endif