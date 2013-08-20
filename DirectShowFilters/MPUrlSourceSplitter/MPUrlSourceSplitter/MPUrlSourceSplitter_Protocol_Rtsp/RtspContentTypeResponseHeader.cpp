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

#include "RtspContentTypeResponseHeader.h"

CRtspContentTypeResponseHeader::CRtspContentTypeResponseHeader(void)
  : CRtspResponseHeader()
{
  this->contentType = NULL;
}

CRtspContentTypeResponseHeader::~CRtspContentTypeResponseHeader(void)
{
  FREE_MEM(this->contentType);
}

/* get methods */

const wchar_t *CRtspContentTypeResponseHeader::GetContentType(void)
{
  return this->contentType;
}

/* set methods */

/* other methods */

CRtspContentTypeResponseHeader *CRtspContentTypeResponseHeader::Clone(void)
{
  return (CRtspContentTypeResponseHeader *)__super::Clone();
}

bool CRtspContentTypeResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspContentTypeResponseHeader *header = dynamic_cast<CRtspContentTypeResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_RESULT_WITH_NULL(header->contentType, this->contentType, result);
  }

  return result;
}

CHttpHeader *CRtspContentTypeResponseHeader::GetNewHeader(void)
{
  return new CRtspContentTypeResponseHeader();
}

bool CRtspContentTypeResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_CONTENT_TYPE_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      this->contentType = Duplicate(this->value);
      result &= (this->contentType != NULL);
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_CONTENT_TYPE_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}