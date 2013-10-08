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

CPayloadType::CPayloadType(void)
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
  bool res = true;
  CPayloadType *result = new CPayloadType();

  if (result != NULL)
  {
    
    result->id = this->id;
    result->channels = this->channels;
    result->clockRate = this->clockRate;
    SET_STRING_AND_RESULT_WITH_NULL(result->encodingName, this->encodingName, res);
    result->mediaType = this->mediaType;
  }

  CHECK_CONDITION_EXECUTE(!res, FREE_MEM_CLASS(result));
  return result;
}