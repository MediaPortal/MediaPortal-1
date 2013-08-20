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

#include "RtspContentLengthResponseHeader.h"
#include "conversions.h"

CRtspContentLengthResponseHeader::CRtspContentLengthResponseHeader(void)
  : CRtspResponseHeader()
{
  this->contentLength = RTSP_CONTENT_LENGTH_UNSPECIFIED;
}

CRtspContentLengthResponseHeader::~CRtspContentLengthResponseHeader(void)
{
}

/* get methods */

unsigned int CRtspContentLengthResponseHeader::GetContentLength(void)
{
  return this->contentLength;
}

/* set methods */

/* other methods */

CRtspContentLengthResponseHeader *CRtspContentLengthResponseHeader::Clone(void)
{
  return (CRtspContentLengthResponseHeader *)__super::Clone();
}

bool CRtspContentLengthResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspContentLengthResponseHeader *header = dynamic_cast<CRtspContentLengthResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    header->contentLength = this->contentLength;
  }

  return result;
}

CHttpHeader *CRtspContentLengthResponseHeader::GetNewHeader(void)
{
  return new CRtspContentLengthResponseHeader();
}

bool CRtspContentLengthResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_CONTENT_LENGTH_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      this->contentLength = GetValueUnsignedInt(this->value, RTSP_CONTENT_LENGTH_UNSPECIFIED);
      result &= (this->contentLength != RTSP_CONTENT_LENGTH_UNSPECIFIED);
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_CONTENT_LENGTH_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}