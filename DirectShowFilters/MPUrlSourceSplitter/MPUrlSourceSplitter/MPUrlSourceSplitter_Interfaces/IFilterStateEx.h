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

#ifndef __IFILTER_EX_STATE_DEFINED
#define __IFILTER_EX_STATE_DEFINED

#include "IFilterState.h"
#include <streams.h>

// {505C28D8-01F4-41C7-BD51-013FA6DBBD39}
DEFINE_GUID(IID_IFilterStateEx, 0x505c28d8, 0x1f4, 0x41c7, 0xbd, 0x51, 0x1, 0x3f, 0xa6, 0xdb, 0xbd, 0x39);

// provides interface for filter state
MIDL_INTERFACE("505C28D8-01F4-41C7-BD51-013FA6DBBD39")
IFilterStateEx : public IFilterState
{
public:
  // gets filter version
  // @param version : reference to unsigned integer which will hold filter version
  // @return : S_OK if successful, error code otherwise
  virtual STDMETHODIMP GetVersion(unsigned int *version) = 0;

  // tests if error code is filter error
  // @param isFilterError : reference to variable that holds result of test
  // @param error : the error code to test
  // @return : S_OK if successful, error code otherwise
  virtual STDMETHODIMP IsFilterError(bool *isFilterError, HRESULT error) = 0;

  // gets error description for filter error
  // @param error : the error code to get description
  // @param description : reference to string which will hold description error
  virtual STDMETHODIMP GetErrorDescription(HRESULT error, wchar_t **description) = 0;

  // loads stream into filter
  // @param url : the formatted url to load stream
  // @return : S_OK if successfully loaded, S_FALSE if loading started, error code otherwise
  virtual STDMETHODIMP LoadAsync(const wchar_t *url) = 0;

  // tests if stream is opened
  // @param opened : reference to variable that holds stream state
  // @return : S_OK if successful
  virtual STDMETHODIMP IsStreamOpened(bool *opened) = 0;

  // tests if stream is IPTV compatible
  // @param compatible : reference to variable that holds result
  // @return : S_OK if successful
  virtual STDMETHODIMP IsStreamIptvCompatible(bool *compatible) = 0;
};

#endif
