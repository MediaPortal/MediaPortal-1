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

#include "RtspResponseHeader.h"

CRtspResponseHeader::CRtspResponseHeader(void)
  : CHttpHeader()
{
  this->responseHeaderType = RESPONSE_HEADER_TYPE_UNSPECIFIED;
}

CRtspResponseHeader::~CRtspResponseHeader(void)
{
  FREE_MEM(this->responseHeaderType);
}

/* get methods */

const wchar_t *CRtspResponseHeader::GetResponseHeaderType(void)
{
  return this->responseHeaderType;
}

/* set methods */

/* other methods */

bool CRtspResponseHeader::IsResponseHeaderType(const wchar_t *responseHeaderType)
{
  return (CompareWithNullInvariant(this->responseHeaderType, responseHeaderType) == 0);
}

CRtspResponseHeader *CRtspResponseHeader::Clone(void)
{
  return (CRtspResponseHeader *)__super::Clone();
}

bool CRtspResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspResponseHeader *header = dynamic_cast<CRtspResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_RESULT_WITH_NULL(header->responseHeaderType, this->responseHeaderType, result);
  }

  return result;
}

CHttpHeader *CRtspResponseHeader::GetNewHeader(void)
{
  return new CRtspResponseHeader();
}