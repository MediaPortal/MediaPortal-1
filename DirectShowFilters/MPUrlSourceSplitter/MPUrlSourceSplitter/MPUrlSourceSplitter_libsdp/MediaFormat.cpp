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

#include "MediaFormat.h"

CMediaFormat::CMediaFormat(void)
{
  this->channels = MEDIA_FORMAT_CHANNELS_UNSPECIFIED;
  this->clockRate = MEDIA_FORMAT_CLOCK_RATE_UNSPECIFIED;
  this->payloadType = MEDIA_FORMAT_PAYLOAD_TYPE_UNSPECIFIED;
  this->name = NULL;
  this->type = NULL;
}

CMediaFormat::~CMediaFormat(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->type);
}

/* get methods */

unsigned int CMediaFormat::GetPayloadType(void)
{
  return this->payloadType;
}

const wchar_t *CMediaFormat::GetName(void)
{
  return this->name;
}

const wchar_t *CMediaFormat::GetType(void)
{
  return this->type;
}

unsigned int CMediaFormat::GetChannels(void)
{
  return this->channels;
}

unsigned int CMediaFormat::GetClockRate(void)
{
  return this->clockRate;
}

/* set methods */

void CMediaFormat::SetPayloadType(unsigned int payloadType)
{
  this->payloadType = payloadType;
}

bool CMediaFormat::SetName(const wchar_t *name)
{
  SET_STRING_RETURN_WITH_NULL(this->name, name);
}

bool CMediaFormat::SetType(const wchar_t *type)
{
  SET_STRING_RETURN_WITH_NULL(this->type, type);
}

void CMediaFormat::SetChannels(unsigned int channels)
{
  this->channels = channels;
}

void CMediaFormat::SetClockRate(unsigned int clockRate)
{
  this->clockRate = clockRate;
}

/* other methods */