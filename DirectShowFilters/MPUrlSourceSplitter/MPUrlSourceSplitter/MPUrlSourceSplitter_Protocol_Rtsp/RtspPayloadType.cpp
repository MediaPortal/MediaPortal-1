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

#include "RtspPayloadType.h"

CRtspPayloadType::CRtspPayloadType(HRESULT *result)
  : CPayloadType(result)
{
  this->streamInputFormat = NULL;
}

CRtspPayloadType::~CRtspPayloadType(void)
{
  FREE_MEM(this->streamInputFormat);
}

/* get methods */

const wchar_t *CRtspPayloadType::GetStreamInputFormat(void)
{
  return this->streamInputFormat;
}

/* set methods */

bool CRtspPayloadType::SetStreamInputFormat(const wchar_t *streamInputFormat)
{
  SET_STRING_RETURN_WITH_NULL(this->streamInputFormat, streamInputFormat);
}

/* other methods */

bool CRtspPayloadType::CopyFromPayloadType(CRtspPayloadType *payloadType)
{
  bool result = (payloadType != NULL);

  if (result)
  {
    FREE_MEM(this->streamInputFormat);

    result = payloadType->CloneInternal(this);
  }

  return result;
}

/* protected methods */

CPayloadType *CRtspPayloadType::CreatePayloadType(void)
{
  HRESULT result = S_OK;
  CRtspPayloadType *payloadType = new CRtspPayloadType(&result);
  CHECK_POINTER_HRESULT(result, payloadType, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payloadType));
  return payloadType;
}

bool CRtspPayloadType::CloneInternal(CPayloadType *payloadType)
{
  bool result = __super::CloneInternal(payloadType);

  if (result)
  {
    CRtspPayloadType *rtspPayloadType = dynamic_cast<CRtspPayloadType *>(payloadType);

    SET_STRING_AND_RESULT_WITH_NULL(rtspPayloadType->streamInputFormat, this->streamInputFormat, result);
  }

  return result;
}