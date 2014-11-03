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

// {6E33E032-E321-4392-ABA4-82C03AC3DC20}
DEFINE_GUID(IID_IFilterStateEx, 0x6e33e032, 0xe321, 0x4392, 0xab, 0xa4, 0x82, 0xc0, 0x3a, 0xc3, 0xdc, 0x20);

// provides interface for filter state
MIDL_INTERFACE("6E33E032-E321-4392-ABA4-82C03AC3DC20")
IFilterStateEx : virtual public IFilterState
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
};

#endif
