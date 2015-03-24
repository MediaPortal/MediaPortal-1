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

#include "RtspUserAgentRequestHeader.h"

CRtspUserAgentRequestHeader::CRtspUserAgentRequestHeader(HRESULT *result)
  : CRtspRequestHeader(result)
{
}

CRtspUserAgentRequestHeader::~CRtspUserAgentRequestHeader(void)
{
}

/* get methods */

const wchar_t *CRtspUserAgentRequestHeader::GetName(void)
{
  return RTSP_USER_AGENT_REQUEST_HEADER_NAME;
}

const wchar_t *CRtspUserAgentRequestHeader::GetUserAgent(void)
{
  return this->GetValue();
}

/* set methods */

bool CRtspUserAgentRequestHeader::SetName(const wchar_t *name)
{
  // we never set name
  return false;
}

bool CRtspUserAgentRequestHeader::SetUserAgent(const wchar_t *userAgent)
{
  return __super::SetValue(userAgent);
}

/* other methods */

/* protected methods */

bool CRtspUserAgentRequestHeader::CloneInternal(CHttpHeader *clone)
{
  return __super::CloneInternal(clone);
}

CHttpHeader *CRtspUserAgentRequestHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspUserAgentRequestHeader *header = new CRtspUserAgentRequestHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}