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

#include "RtspRequestHeader.h"

CRtspRequestHeader::CRtspRequestHeader(void)
  : CHttpHeader()
{
  this->requestHeader = NULL;
}

CRtspRequestHeader::~CRtspRequestHeader(void)
{
  FREE_MEM(this->requestHeader);
}

/* get methods */

const wchar_t *CRtspRequestHeader::GetRequestHeader(void)
{
  FREE_MEM(this->requestHeader);
  this->requestHeader = FormatString(RTSP_REQUEST_HEADER_FORMAT, this->GetName(), this->GetValue());
  return this->requestHeader;
}

/* set methods */

/* other methods */

CRtspRequestHeader *CRtspRequestHeader::Clone(void)
{
  return (CRtspRequestHeader *)__super::Clone();
}

bool CRtspRequestHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);

  return result;
}

CHttpHeader *CRtspRequestHeader::GetNewHeader(void)
{
  return new CRtspRequestHeader();
}