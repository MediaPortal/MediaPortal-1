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

#include "RtspSetupRequest.h"

CRtspSetupRequest::CRtspSetupRequest(HRESULT *result)
  : CRtspRequest(result)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CRtspTransportRequestHeader *header = new CRtspTransportRequestHeader(result);
    CHECK_POINTER_HRESULT(*result, header, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*result, this->requestHeaders->Add(header), *result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(header));
  }
}

CRtspSetupRequest::CRtspSetupRequest(HRESULT *result, bool createDefaultHeaders)
  : CRtspRequest(result, createDefaultHeaders)
{
  if (createDefaultHeaders && (result != NULL) && (SUCCEEDED(*result)))
  {
    CRtspTransportRequestHeader *header = new CRtspTransportRequestHeader(result);
    CHECK_POINTER_HRESULT(*result, header, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*result, this->requestHeaders->Add(header), *result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(*result), FREE_MEM_CLASS(header));
  }
}

CRtspSetupRequest::~CRtspSetupRequest(void)
{
}

/* get methods */

const wchar_t *CRtspSetupRequest::GetMethod(void)
{
  return RTSP_SETUP_METHOD;
}

CRtspTransportRequestHeader *CRtspSetupRequest::GetTransportRequestHeader(void)
{
  return dynamic_cast<CRtspTransportRequestHeader *>(this->requestHeaders->GetRtspHeader(RTSP_TRANSPORT_REQUEST_HEADER_NAME, false));
}

/* set methods */

/* other methods */

/* protected methods */

bool CRtspSetupRequest::CloneInternal(CRtspRequest *clone)
{
  return __super::CloneInternal(clone);
}

CRtspRequest *CRtspSetupRequest::CreateRequest(void)
{
  HRESULT result = S_OK;
  CRtspSetupRequest *request = new CRtspSetupRequest(&result, false);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}