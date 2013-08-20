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

#pragma once

#ifndef __MEDIA_PACKET_COLLECTION_DEFINED
#define __MEDIA_PACKET_COLLECTION_DEFINED

#include "KeyedCollection.h"
#include "MediaPacket.h"

class CMediaPacketCollection : public CKeyedCollection<CMediaPacket, int64_t>
{
public:
  CMediaPacketCollection(void);
  ~CMediaPacketCollection(void);

  // gets index of media packet where position is between start position and end position
  // @param position : the position between start position and end position
  // @return : index of media packet or UINT_MAX if not exists
  unsigned int GetMediaPacketIndexBetweenPositions(int64_t position);

  // get item index of item with specified start position
  // @param key : the key of item to find
  // @param context : reference to user defined context
  // @return : the index of item or UINT_MAX if not found
  unsigned int GetItemIndex(int64_t key, void *context);

  // add item to collection
  // @param item : the reference to item to add
  // @return : true if successful, false otherwise
  bool Add(CMediaPacket *item);

  // returns indexes where item have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => item have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => item have to be placed on end
  // startIndex == endIndex => item with same key exists in collection (index of item is startIndex)
  // item have to be placed between startIndex and endIndex
  // @param key : the item key to compare
  // @param context : the reference to user defined context
  // @param startIndex : reference to variable which holds start index where item have to be placed
  // @param endIndex : reference to variable which holds end index where item have to be placed
  // @return : true if successful, false otherwise
  bool GetItemInsertPosition(int64_t key, void *context, unsigned int *startIndex, unsigned int *endIndex);

  // gets overlapped region between specified packet and consolidated space
  // @param packet : packet to get overlapped region
  // @return : media packet which holds overlapping region without data or NULL if error
  //           if media packet start and end are zero then no overlapping region exists
  CMediaPacket *GetOverlappedRegion(CMediaPacket *packet);

  // clear collection of items
  virtual void Clear(void);

protected:

  CMediaPacketCollection(bool consolidateSpace);

  // compare two item keys
  // @param firstKey : the first item key to compare
  // @param secondKey : the second item key to compare
  // @param context : the reference to user defined context
  // @return : 0 if keys are equal, lower than zero if firstKey is lower than secondKey, greater than zero if firstKey is greater than secondKey
  int CompareItemKeys(int64_t firstKey, int64_t secondKey, void *context);

  // gets key for item
  // @param item : the item to get key
  // @return : the key of item
  int64_t GetKey(CMediaPacket *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CMediaPacket *Clone(CMediaPacket *item);

  // holds consolidated media packets space
  CMediaPacketCollection *consolidatedMediaPackets;

  // adds media packet to consolidated media packets space
  // @param packet : packet to add
  void AddPacketToConsolidatedMediaPackets(CMediaPacket *packet);
};

#endif