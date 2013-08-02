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

#ifndef __IMPIPTVCONNECTINFO_DEFINED
#define __IMPIPTVCONNECTINFO_DEFINED

DEFINE_GUID(IID_MPIPTVConnectInfo, 0xA1ACE92B, 0xADF9, 0x42CE, 0xB2, 0x9A, 0x7F, 0x2F, 0xF4, 0x1A, 0xEB, 0xB0);

// provides interface for specifying additional parameters
MIDL_INTERFACE("A1ACE92B-ADF9-42CE-B29A-7F2FF41AEBB0")
IMPIPTVConnectInfo : public IUnknown
{
public:
  // this method parses additional parameters sent to filter
  // additional parameters have to be set before calling Load method
  // parameters are in form: param1=value1|...|paramN=valueN
  // if in value is need to write pipe character, then pipe character have to be doubled
  virtual STDMETHODIMP SetConnectInfo(LPCOLESTR pszConnectInfo) = 0;
};


#endif
