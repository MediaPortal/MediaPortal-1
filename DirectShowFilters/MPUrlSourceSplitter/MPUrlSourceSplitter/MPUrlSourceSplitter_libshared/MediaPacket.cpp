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

#include "MediaPacket.h"
#include "Utilities.h"

CMediaPacket::CMediaPacket(HRESULT *result)
  : CCacheFileItem(result)
{
  this->start = 0;
  this->presentationTimestamp = MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED;
}

CMediaPacket::~CMediaPacket(void)
{
}

/* get methods */

int64_t CMediaPacket::GetStart(void)
{
  return this->start;
}

int64_t CMediaPacket::GetPresentationTimestamp(void)
{
  return this->presentationTimestamp;
}

/* set methods */

void CMediaPacket::SetStart(int64_t position)
{
  this->start = position;
}

void CMediaPacket::SetPresentationTimestamp(int64_t presentationTimestamp)
{
  this->presentationTimestamp = presentationTimestamp;
}

void CMediaPacket::SetPresentationTimestamp(int64_t presentationTimestamp, unsigned int presentationTimestampTicksPerSecond)
{
  if (presentationTimestampTicksPerSecond == DSHOW_TIME_BASE)
  {
    this->presentationTimestamp = presentationTimestamp;
  }
  else
  {
    // there can be a problem in multiplication and dividing of big numbers
    // we can't use 128-bit numbers, so we need to use greatest common divisor method

    unsigned int gcd = GreatestCommonDivisor(DSHOW_TIME_BASE, presentationTimestampTicksPerSecond);
    unsigned int numerator = DSHOW_TIME_BASE / gcd;
    unsigned int denumerator = presentationTimestampTicksPerSecond / gcd;

    this->presentationTimestamp = presentationTimestamp * numerator / denumerator;
  }
}

/* other methods */

/* protected methods */

CFastSearchItem *CMediaPacket::CreateItem(void)
{
  HRESULT result = S_OK;
  CMediaPacket *item = new CMediaPacket(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));

  return item;
}

bool CMediaPacket::InternalClone(CFastSearchItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CMediaPacket *mediaPacket = dynamic_cast<CMediaPacket *>(item);
    result &= (mediaPacket != NULL);

    if (result)
    {
      mediaPacket->start = this->start;
      mediaPacket->presentationTimestamp = this->presentationTimestamp;
    }
  }

  return result;
}