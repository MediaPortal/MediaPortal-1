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

#ifndef __MPIPTVGUIDS_DEFINED
#define __MPIPTVGUIDS_DEFINED
#endif

#ifndef __MPIPTVSOURCE_DEFINED
#define __MPIPTVSOURCE_DEFINED

#include "MPIPTVSourceStream.h"
#include "Logger.h"
#include "IMPIPTVConnectInfo.h"
#include "ParameterCollection.h"

#include <initguid.h>
#include <cguid.h>

// {D3DD4C59-D3A7-4b82-9727-7B9203EB67C0}
DEFINE_GUID(CLSID_MPIPTVSource, 
  0xd3dd4c59, 0xd3a7, 0x4b82, 0x97, 0x27, 0x7b, 0x92, 0x3, 0xeb, 0x67, 0xc0);

// This class is exported from the MPIPTVSource.ax
class CMPIPTVSource : public CSource, public IFileSourceFilter, public IMPIPTVConnectInfo
{

private:
  // Constructor is private because you have to use CreateInstance
  CMPIPTVSource(IUnknown *pUnk, HRESULT *phr);
  ~CMPIPTVSource();

  CMPIPTVSourceStream *m_stream;
  TCHAR* m_url;
  CParameterCollection *m_parameters;
  CParameterCollection *m_configuration;
  CLogger logger;

public:
  // IFileSourceFilter
  DECLARE_IUNKNOWN
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);
  STDMETHODIMP Load(LPCOLESTR pszFileName, const AM_MEDIA_TYPE* pmt);
  STDMETHODIMP GetCurFile(LPOLESTR* ppszFileName, AM_MEDIA_TYPE* pmt);
  static CUnknown * WINAPI CreateInstance(IUnknown *pUnk, HRESULT *phr);

  // IMPIPTVConnectInfo
  STDMETHODIMP SetConnectInfo(LPCOLESTR pszConnectInfo);
};

#endif
