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

#include "RtspSessionRequestHeader.h"

CRtspSessionRequestHeader::CRtspSessionRequestHeader(HRESULT *result)
  : CRtspRequestHeader(result)
{
  this->sessionId = NULL;
}

CRtspSessionRequestHeader::~CRtspSessionRequestHeader(void)
{
  FREE_MEM(this->sessionId);
}

/* get methods */

const wchar_t *CRtspSessionRequestHeader::GetName(void)
{
  return RTSP_SESSION_REQUEST_HEADER_NAME;
}

const wchar_t *CRtspSessionRequestHeader::GetSessionId(void)
{
  return this->sessionId;
}

const wchar_t *CRtspSessionRequestHeader::GetValue(void)
{
  return this->GetSessionId();
}

/* set methods */

bool CRtspSessionRequestHeader::SetName(const wchar_t *name)
{
  // we never set name
  return false;
}

bool CRtspSessionRequestHeader::SetValue(const wchar_t *value)
{
  // we never set value
  return false;
}

bool CRtspSessionRequestHeader::SetSessionId(const wchar_t *sessionId)
{
  SET_STRING_RETURN_WITH_NULL(this->sessionId, sessionId);
}

/* other methods */

/* protected methods */

bool CRtspSessionRequestHeader::CloneInternal(CHttpHeader *clone)
{
  bool result = __super::CloneInternal(clone);
  CRtspSessionRequestHeader *header = dynamic_cast<CRtspSessionRequestHeader *>(clone);
  result &= (header != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(header->sessionId, this->sessionId, result);
  }

  return result;
}

CHttpHeader *CRtspSessionRequestHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspSessionRequestHeader *header = new CRtspSessionRequestHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}