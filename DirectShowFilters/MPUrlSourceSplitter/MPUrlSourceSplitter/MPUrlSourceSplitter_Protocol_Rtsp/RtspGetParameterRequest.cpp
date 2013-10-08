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

#include "RtspGetParameterRequest.h"

CRtspGetParameterRequest::CRtspGetParameterRequest(void)
  : CRtspRequest()
{
}

CRtspGetParameterRequest::CRtspGetParameterRequest(bool createDefaultHeaders)
  : CRtspRequest(createDefaultHeaders)
{
}


CRtspGetParameterRequest::~CRtspGetParameterRequest(void)
{
}

/* get methods */

const wchar_t *CRtspGetParameterRequest::GetMethod(void)
{
  return RTSP_GET_PARAMETER_METHOD;
}

/* set methods */

/* other methods */

CRtspGetParameterRequest *CRtspGetParameterRequest::Clone(void)
{
  return (CRtspGetParameterRequest *)__super::Clone();
}

bool CRtspGetParameterRequest::CloneInternal(CRtspRequest *clonedRequest)
{
  return __super::CloneInternal(clonedRequest);
}

CRtspRequest *CRtspGetParameterRequest::GetNewRequest(void)
{
  return new CRtspGetParameterRequest(false);
}