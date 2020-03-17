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

#ifndef __OUTPUT_PIN_MEDIA_SAMPLE_DEFINED
#define __OUTPUT_PIN_MEDIA_SAMPLE_DEFINED

#include "IOutputPinMediaSample.h"

class COutputPinMediaSample : public CMediaSample
  , public IOutputPinMediaSample
{
public:
  // initializes a new instance of COutputPinMediaSample class
  // @param allocator : pointer to the CBaseAllocator object that created this sample
  // @param hr : ignored
  COutputPinMediaSample(CBaseAllocator *allocator, HRESULT *hr);
  ~COutputPinMediaSample(void);

  // IUnknown interface implementation
  STDMETHODIMP QueryInterface(REFIID riid, void **ppv);
  STDMETHODIMP_(ULONG) AddRef();
  STDMETHODIMP_(ULONG) Release();

  // IOutputPinMediaSample interface implementation

  // sets output packet to media sample
  // @param packet : the packet to set to media sample
  // @return : S_OK if successful, E_POINTER if packet is NULL, error code otherwise
  STDMETHODIMP SetPacket(COutputPinPacket *packet);

  /* get methods */

  /* set methods */

  /* other methods */

  static STDMETHODIMP SetPacket(IMediaSample *mediaSample, COutputPinPacket *packet);

protected:
  
};

#endif