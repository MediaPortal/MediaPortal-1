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

#include "RtspContentBaseResponseHeader.h"

CRtspContentBaseResponseHeader::CRtspContentBaseResponseHeader(void)
  : CRtspResponseHeader()
{
  this->uri = NULL;
}

CRtspContentBaseResponseHeader::~CRtspContentBaseResponseHeader(void)
{
  FREE_MEM(this->uri);
}

/* get methods */

const wchar_t *CRtspContentBaseResponseHeader::GetUri(void)
{
  return this->uri;
}

/* set methods */

/* other methods */

CRtspContentBaseResponseHeader *CRtspContentBaseResponseHeader::Clone(void)
{
  return (CRtspContentBaseResponseHeader *)__super::Clone();
}

bool CRtspContentBaseResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspContentBaseResponseHeader *header = dynamic_cast<CRtspContentBaseResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_RESULT_WITH_NULL(header->uri, this->uri, result);
  }

  return result;
}

CHttpHeader *CRtspContentBaseResponseHeader::GetNewHeader(void)
{
  return new CRtspContentBaseResponseHeader();
}

bool CRtspContentBaseResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_CONTENT_BASE_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      this->uri = Duplicate(this->value);
      result &= (this->uri != NULL);
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_CONTENT_BASE_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}