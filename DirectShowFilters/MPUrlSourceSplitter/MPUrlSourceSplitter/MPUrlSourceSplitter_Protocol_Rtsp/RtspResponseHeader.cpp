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

CRtspResponseHeader::CRtspResponseHeader(HRESULT *result)
  : CHttpHeader(result)
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

/* protected methods */

bool CRtspResponseHeader::CloneInternal(CHttpHeader *clone)
{
  bool result = __super::CloneInternal(clone);
  CRtspResponseHeader *header = dynamic_cast<CRtspResponseHeader *>(clone);
  result &= (header != NULL);

  if (result)
  {
    header->flags = this->flags;

    SET_STRING_RESULT_WITH_NULL(header->responseHeaderType, this->responseHeaderType, result);
  }

  return result;
}

CHttpHeader *CRtspResponseHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspResponseHeader *header = new CRtspResponseHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}