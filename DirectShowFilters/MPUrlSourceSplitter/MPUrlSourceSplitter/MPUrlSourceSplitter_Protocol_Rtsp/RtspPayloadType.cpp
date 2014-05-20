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

CRtspPayloadType::CRtspPayloadType(void)
  : CPayloadType()
{
  this->flags = RTSP_PAYLOAD_TYPE_FLAG_NONE;
  this->streamInputFormat = NULL;
}

CRtspPayloadType::~CRtspPayloadType(void)
{
  FREE_MEM(this->streamInputFormat);
}

/* get methods */

unsigned int CRtspPayloadType::GetFlags(void)
{
  return this->flags;
}

const wchar_t *CRtspPayloadType::GetStreamInputFormat(void)
{
  return this->streamInputFormat;
}

/* set methods */

void CRtspPayloadType::SetFlags(unsigned int flags)
{
  this->flags = flags;
}

bool CRtspPayloadType::SetStreamInputFormat(const wchar_t *streamInputFormat)
{
  SET_STRING_RETURN_WITH_NULL(this->streamInputFormat, streamInputFormat);
}

/* other methods */

bool CRtspPayloadType::IsSetFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}

CRtspPayloadType *CRtspPayloadType::Clone(void)
{
  CRtspPayloadType *clone = new CRtspPayloadType();
  bool result = (clone != NULL);

  if (result)
  {
    result &= this->CloneInternal(clone);
  }

  CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(clone));
  return clone;
}

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

bool CRtspPayloadType::CloneInternal(CRtspPayloadType *payloadType)
{
  bool result = __super::CloneInternal(payloadType);

  if (result)
  {
    payloadType->flags = this->flags;

    SET_STRING_AND_RESULT_WITH_NULL(payloadType->streamInputFormat, this->streamInputFormat, result);
  }

  return result;
}