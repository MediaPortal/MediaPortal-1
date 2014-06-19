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
  this->end = 0;
  this->presentationTimestamp = MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED;
  this->presentationTimestampTicksPerSecond = 0;
}

CMediaPacket::~CMediaPacket(void)
{
}

/* get methods */

int64_t CMediaPacket::GetStart(void)
{
  return this->start;
}

int64_t CMediaPacket::GetEnd(void)
{
  return this->end;
}

int64_t CMediaPacket::GetPresentationTimestamp(void)
{
  return this->presentationTimestamp;
}

int64_t CMediaPacket::GetPresentationTimestampInDirectShowTimeUnits(void)
{
  int64_t result = MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED;

  if (this->GetPresentationTimestamp() != MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED)
  {
    if (this->GetPresentationTimestampTicksPerSecond() == DSHOW_TIME_BASE)
    {
      result = this->GetPresentationTimestamp();
    }
    else
    {
      // there can be a problem in multiplication and dividing of big numbers
      // we can't use 128-bit numbers, so we need to use greatest common divisor method

      unsigned int gcd = GreatestCommonDivisor(DSHOW_TIME_BASE, this->GetPresentationTimestampTicksPerSecond());
      unsigned int numerator = DSHOW_TIME_BASE / gcd;
      unsigned int denumerator = this->GetPresentationTimestampTicksPerSecond() / gcd;

      result = this->GetPresentationTimestamp() * numerator / denumerator;
    }
  }

  return result;
}

unsigned int CMediaPacket::GetPresentationTimestampTicksPerSecond(void)
{
  return this->presentationTimestampTicksPerSecond;
}

/* set methods */

void CMediaPacket::SetStart(int64_t position)
{
  this->start = position;
}

void CMediaPacket::SetEnd(int64_t position)
{
  this->end = position;
}

void CMediaPacket::SetPresentationTimestamp(int64_t presentationTimestamp)
{
  this->presentationTimestamp = presentationTimestamp;
}

void CMediaPacket::SetPresentationTimestampTicksPerSecond(unsigned int presentationTimestampTicksPerSecond)
{
  this->presentationTimestampTicksPerSecond = presentationTimestampTicksPerSecond;
}

/* other methods */

CMediaPacket *CMediaPacket::CreateMediaPacketBasedOnPacket(int64_t start, int64_t end)
{
  HRESULT result = S_OK;
  CMediaPacket *mediaPacket = new CMediaPacket(&result);
  CHECK_POINTER_HRESULT(result, mediaPacket, result, E_OUTOFMEMORY);

  unsigned char *buffer = NULL;
  bool success = (SUCCEEDED(result) && (start >= this->start) && (end >= start) && (this->GetBuffer() != NULL));

  if (success)
  {
    // initialize new media packet start, end and length
    unsigned int length = (unsigned int)(end - start + 1);

    // initialize new media packet data
    mediaPacket->SetStart(start);
    mediaPacket->SetEnd(end);
    if (success)
    {
      success = (mediaPacket->GetBuffer()->InitializeBuffer(length));
    }

    if (success)
    {
      // create temporary buffer and copy data from unprocessed media packet
      buffer = ALLOC_MEM_SET(buffer, unsigned char, length, 0);
      success = (buffer != NULL);
    }

    if (success)
    {
      success = (this->GetBuffer()->CopyFromBuffer(buffer, length, (unsigned int)(start - this->start)) == length);
    }

    if (success)
    {
      // add data from temporary buffer to first part media packet
      success = (mediaPacket->GetBuffer()->AddToBuffer(buffer, length) == length);

      // remove temporary buffer
      FREE_MEM(buffer);
    }
  }

  if (!success)
  {
    FREE_MEM_CLASS(mediaPacket);
  }

  return mediaPacket;
}

/* protected methods */

CCacheFileItem *CMediaPacket::CreateItem(void)
{
  HRESULT result = S_OK;
  CMediaPacket *item = new CMediaPacket(&result);
  CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));

  return item;
}

bool CMediaPacket::InternalClone(CCacheFileItem *item)
{
  bool result = __super::InternalClone(item);
  
  if (result)
  {
    CMediaPacket *mediaPacket = dynamic_cast<CMediaPacket *>(item);
    result &= (mediaPacket != NULL);

    if (result)
    {
      mediaPacket->start = this->start;
      mediaPacket->end = this->end;
      mediaPacket->presentationTimestamp = this->presentationTimestamp;
      mediaPacket->presentationTimestampTicksPerSecond = this->presentationTimestampTicksPerSecond;
    }
  }

  return result;
}