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

#include "RtspServerResponseHeader.h"

CRtspServerResponseHeader::CRtspServerResponseHeader(void)
  : CRtspResponseHeader()
{
  this->server = NULL;
}

CRtspServerResponseHeader::~CRtspServerResponseHeader(void)
{
  FREE_MEM(this->server);
}

/* get methods */

const wchar_t *CRtspServerResponseHeader::GetServer(void)
{
  return this->server;
}

/* set methods */

/* other methods */

CRtspServerResponseHeader *CRtspServerResponseHeader::Clone(void)
{
  return (CRtspServerResponseHeader *)__super::Clone();
}

bool CRtspServerResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspServerResponseHeader *header = dynamic_cast<CRtspServerResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_RESULT_WITH_NULL(header->server, this->server, result);
  }

  return result;
}

CHttpHeader *CRtspServerResponseHeader::GetNewHeader(void)
{
  return new CRtspServerResponseHeader();
}

bool CRtspServerResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_SERVER_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      this->server = Duplicate(this->value);
      result &= (this->server != NULL);
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_SERVER_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}