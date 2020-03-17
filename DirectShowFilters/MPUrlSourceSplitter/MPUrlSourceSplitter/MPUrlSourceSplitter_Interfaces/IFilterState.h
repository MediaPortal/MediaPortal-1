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

#ifndef __IFILTER_STATE_DEFINED
#define __IFILTER_STATE_DEFINED

#include <streams.h>

// {420E98EF-0338-472F-B77B-C5BA8997ED10}
DEFINE_GUID(IID_IFilterState, 0x420E98EF, 0x0338, 0x472F, 0xB7, 0x7B, 0xC5, 0xBA, 0x89, 0x97, 0xED, 0x10);

// provides interface for filter state
MIDL_INTERFACE("420E98EF-0338-472F-B77B-C5BA8997ED10")
IFilterState : public IUnknown
{
public:
  // tests if filter is ready to connect output pins
  // @param ready : reference to variable that holds ready state
  // @return : S_OK if successful
  virtual STDMETHODIMP IsFilterReadyToConnectPins(bool *ready) = 0;
  
  // get cache file name
  // @param path : reference to string which will hold path to cache file name
  // @return : S_OK if successful (*path can be NULL), E_POINTER if path is NULL
  virtual STDMETHODIMP GetCacheFileName(wchar_t **path) = 0;
};

#endif
