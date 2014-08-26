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

#include "MediaPacketCollection.h"

CMediaPacketCollection::CMediaPacketCollection(HRESULT *result)
  : CCacheFileItemCollection(result)
{
}

CMediaPacketCollection::~CMediaPacketCollection(void)
{
}

/* get methods */

CMediaPacket *CMediaPacketCollection::GetItem(unsigned int index)
{
  return (CMediaPacket *)__super::GetItem(index);
}

/* set methods */

/* other methods */

bool CMediaPacketCollection::Add(CMediaPacket *item)
{
  bool result = (item != NULL);

  CHECK_CONDITION_EXECUTE(result, result &= this->EnsureEnoughSpace(this->Count() + 1));

  if (result)
  {
    unsigned int startIndex = 0;
    unsigned int endIndex = 0;

    if (this->GetItemInsertPosition(item->GetStart(), &startIndex, &endIndex))
    {
      // check if media packet exists in collection
      result &= (startIndex != endIndex);

      if (result)
      {
        result &= __super::Insert(min(endIndex, this->Count()), item);
      }
    }
  }

  return result;
}

unsigned int CMediaPacketCollection::GetMediaPacketIndexBetweenPositions(int64_t position)
{
  unsigned int index = UINT_MAX;

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->GetItemInsertPosition(position, &startIndex, &endIndex))
  {
    if (startIndex != UINT_MAX)
    {
      // if requested position is somewhere after start of media packets
      CMediaPacket *mediaPacket = this->GetItem(startIndex);
      int64_t positionStart = mediaPacket->GetStart();
      int64_t positionEnd = positionStart + mediaPacket->GetLength();

      if ((position >= positionStart) && (position < positionEnd))
      {
        // we found media packet
        index = startIndex;
      }
    }
  }

  return index;
}

bool CMediaPacketCollection::GetItemInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = (this->Count() > 0);

    if (result)
    {
      unsigned int first = 0;
      unsigned int last = this->Count() - 1;
      result = false;

      while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
      {
        // compute middle index
        unsigned int middle = (first + last) / 2;

        // get media packet at middle index
        CMediaPacket *mediaPacket = this->GetItem(middle);

        // compare media packet start to position
        if (position > mediaPacket->GetStart())
        {
          // position is bigger than media packet start time
          // search in top half
          first = middle + 1;
        }
        else if (position < mediaPacket->GetStart()) 
        {
          // position is lower than media packet start time
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found media packet with same starting time as position
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found media packet
        // it means that media packet with 'position' belongs between first and last
        *startIndex = last;
        *endIndex = (first >= this->Count()) ? UINT_MAX : first;
        result = true;
      }
    }
    else
    {
      *startIndex = UINT_MAX;
      *endIndex = 0;
      result = true;
    }
  }

  return result;
}

bool CMediaPacketCollection::FindGapInMediaPackets(int64_t position, int64_t *startPosition, int64_t *endPosition)
{
  bool result = ((startPosition != NULL) && (endPosition != NULL));

  if (result)
  {
    // check media packets from supplied last valid stream position
    result = false;

    int64_t tempStartPosition = 0;
    int64_t tempEndPosition = 0;
    unsigned int mediaPacketIndex = this->GetMediaPacketIndexBetweenPositions(position);

    if (mediaPacketIndex != UINT_MAX)
    {
      CMediaPacket *mediaPacket = this->GetItem(mediaPacketIndex);
      tempStartPosition = mediaPacket->GetStart();
      tempEndPosition = tempStartPosition + mediaPacket->GetLength() - 1;
    }

    // because collection is sorted
    // then simple going through all media packets will reveal if there is some empty place
    while (mediaPacketIndex != UINT_MAX)
    {
      CMediaPacket *mediaPacket = this->GetItem(mediaPacketIndex);
      int64_t mediaPacketStart = mediaPacket->GetStart();

      if (tempStartPosition == mediaPacketStart)
      {
        // next start time is next to end of current media packet
        tempStartPosition = mediaPacketStart + mediaPacket->GetLength();
        mediaPacketIndex++;

        if (mediaPacketIndex >= this->Count())
        {
          // stop checking, all media packets checked
          tempEndPosition = tempStartPosition;
          mediaPacketIndex = UINT_MAX;
        }
      }
      else
      {
        // we found gap between media packets
        // set end time and stop checking media packets
        tempEndPosition = mediaPacketStart - 1;
        mediaPacketIndex = UINT_MAX;
        result = true;
      }
    }

    if (result)
    {
      *startPosition = tempStartPosition;
      *endPosition = tempEndPosition;
    }
  }

  return result;
}

/* protected methods */

bool CMediaPacketCollection::Insert(unsigned int position, CMediaPacket *item)
{
  return __super::Insert(position, item);
}