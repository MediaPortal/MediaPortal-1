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

#include "PayloadType.h"

CPayloadType::CPayloadType(HRESULT *result)
  : CFlags()
{
  this->channels = PAYLOAD_TYPE_CHANNELS_VARIABLE;
  this->clockRate = PAYLOAD_TYPE_CLOCK_RATE_VARIABLE;
  this->encodingName = NULL;
  this->id = PAYLOAD_TYPE_ID_DYNAMIC;
  this->mediaType = Unknown;
}

CPayloadType::~CPayloadType(void)
{
  FREE_MEM(this->encodingName);
}

/* get methods */

unsigned int CPayloadType::GetId(void)
{
  return this->id;
}

const wchar_t *CPayloadType::GetEncodingName(void)
{
  return this->encodingName;
}

CPayloadType::MediaType CPayloadType::GetMediaType(void)
{
  return this->mediaType;
}

unsigned int CPayloadType::GetClockRate(void)
{
  return this->clockRate;
}

unsigned int CPayloadType::GetChannels(void)
{
  return this->channels;
}

/* set methods */

void CPayloadType::SetId(unsigned int id)
{
  this->id = id;
}

bool CPayloadType::SetEncodingName(const wchar_t *encodingName)
{
  SET_STRING_RETURN_WITH_NULL(this->encodingName, encodingName);
}

void CPayloadType::SetMediaType(CPayloadType::MediaType mediaType)
{
  this->mediaType = mediaType;
}

void CPayloadType::SetClockRate(unsigned int clockRate)
{
  this->clockRate = clockRate;
}

void CPayloadType::SetChannels(unsigned int channels)
{
  this->channels = channels;
}

/* other methods */

CPayloadType *CPayloadType::Clone(void)
{
  HRESULT result = S_OK;
  CPayloadType *clone = this->CreatePayloadType();
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, this->CloneInternal(clone), result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}

/* protected methods */

CPayloadType *CPayloadType::CreatePayloadType(void)
{
  HRESULT result = S_OK;
  CPayloadType *payloadType = new CPayloadType(&result);
  CHECK_POINTER_HRESULT(result, payloadType, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payloadType));
  return payloadType;
}

bool CPayloadType::CloneInternal(CPayloadType *payloadType)
{
  bool result = (payloadType != NULL);

  if (result)
  {
    payloadType->flags = this->flags;
    payloadType->id = this->id;
    payloadType->channels = this->channels;
    payloadType->clockRate = this->clockRate;
    SET_STRING_AND_RESULT_WITH_NULL(payloadType->encodingName, this->encodingName, result);
    payloadType->mediaType = this->mediaType;
  }

  return result;
}