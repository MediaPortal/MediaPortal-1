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

#include "RtspRefererRequestHeader.h"

CRtspRefererRequestHeader::CRtspRefererRequestHeader(void)
  : CRtspRequestHeader()
{
}

CRtspRefererRequestHeader::~CRtspRefererRequestHeader(void)
{
}

/* get methods */

const wchar_t *CRtspRefererRequestHeader::GetName(void)
{
  return RTSP_REFERER_REQUEST_HEADER_NAME;
}

const wchar_t *CRtspRefererRequestHeader::GetReferer(void)
{
  return this->GetValue();
}

/* set methods */

bool CRtspRefererRequestHeader::SetName(const wchar_t *name)
{
  // we never set name
  return false;
}

bool CRtspRefererRequestHeader::SetReferer(const wchar_t *referer)
{
  return __super::SetValue(referer);
}

/* other methods */

CRtspRefererRequestHeader *CRtspRefererRequestHeader::Clone(void)
{
  return (CRtspRefererRequestHeader *)__super::Clone();
}

bool CRtspRefererRequestHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  return __super::CloneInternal(clonedHeader);
}

CHttpHeader *CRtspRefererRequestHeader::GetNewHeader(void)
{
  return new CRtspRefererRequestHeader();
}