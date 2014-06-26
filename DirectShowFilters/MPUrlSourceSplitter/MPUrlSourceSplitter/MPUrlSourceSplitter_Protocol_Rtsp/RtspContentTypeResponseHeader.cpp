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

CRtspContentTypeResponseHeader::CRtspContentTypeResponseHeader(HRESULT *result)
  : CRtspResponseHeader(result)
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

/* protected methods */

bool CRtspContentTypeResponseHeader::CloneInternal(CHttpHeader *clone)
{
  bool result = __super::CloneInternal(clone);
  CRtspContentTypeResponseHeader *header = dynamic_cast<CRtspContentTypeResponseHeader *>(clone);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_RESULT_WITH_NULL(header->contentType, this->contentType, result);
  }

  return result;
}

CHttpHeader *CRtspContentTypeResponseHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspContentTypeResponseHeader *header = new CRtspContentTypeResponseHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}