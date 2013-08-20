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

CRtspUserAgentRequestHeader::CRtspUserAgentRequestHeader(void)
  : CRtspRequestHeader()
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

CRtspUserAgentRequestHeader *CRtspUserAgentRequestHeader::Clone(void)
{
  return (CRtspUserAgentRequestHeader *)__super::Clone();
}

bool CRtspUserAgentRequestHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  return __super::CloneInternal(clonedHeader);
}

CHttpHeader *CRtspUserAgentRequestHeader::GetNewHeader(void)
{
  return new CRtspUserAgentRequestHeader();
}