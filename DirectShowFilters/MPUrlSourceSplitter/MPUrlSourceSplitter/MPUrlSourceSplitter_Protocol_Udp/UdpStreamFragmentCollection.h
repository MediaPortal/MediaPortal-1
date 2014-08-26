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

#ifndef __UDP_STREAM_FRAGMENT_COLLECTION_DEFINED
#define __UDP_STREAM_FRAGMENT_COLLECTION_DEFINED

#include "CacheFileItemCollection.h"
#include "UdpStreamFragment.h"

class CUdpStreamFragmentCollection : public CCacheFileItemCollection
{
public:
  CUdpStreamFragmentCollection(HRESULT *result);
  virtual ~CUdpStreamFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CUdpStreamFragment *GetItem(unsigned int index);

  /* set methods */

  /* other methods */

  // adds UDP stream fragment to collection
  // @param item : the reference to UDP stream fragment to add
  // @return : true if successful, false otherwise
  virtual bool Add(CUdpStreamFragment *item);

  // gets index of UDP stream fragment where position is between start position and end position
  // @param position : the position between start position and end position
  // @return : index of UDP stream fragment or UINT_MAX if not exists
  unsigned int GetStreamFragmentIndexBetweenPositions(int64_t position);

  // returns indexes where UDP stream fragment have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => UDP stream fragment have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => UDP stream fragment have to be placed on end
  // startIndex == endIndex => UDP stream fragment with same start position exists in collection (index of UDP stream fragment is startIndex)
  // UDP stream fragment have to be placed between startIndex and endIndex
  // @param position : the start position to compare
  // @param startIndex : reference to variable which holds start index where UDP stream fragment have to be placed
  // @param endIndex : reference to variable which holds end index where UDP stream fragment have to be placed
  // @return : true if successful, false otherwise
  bool GetItemInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex);

protected:

  /* methods */
};

#endif