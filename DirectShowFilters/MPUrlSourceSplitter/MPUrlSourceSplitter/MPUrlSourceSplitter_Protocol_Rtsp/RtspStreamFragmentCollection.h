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

#ifndef __RTSP_STREAM_FRAGMENT_COLLECTION_DEFINED
#define __RTSP_STREAM_FRAGMENT_COLLECTION_DEFINED

#include "RtspStreamFragment.h"
#include "CacheFileItemCollection.h"

#define STREAM_FRAGMENT_INDEX_NOT_SET                                 UINT_MAX

class CRtspStreamFragmentCollection : public CCacheFileItemCollection
{
public:
  CRtspStreamFragmentCollection(HRESULT *result);
  virtual ~CRtspStreamFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CRtspStreamFragment *GetItem(unsigned int index);

  // gets index of stream fragment where position is between start position and end position
  // @param position : the position between start position and end position
  // @return : index of stream fragment or UINT_MAX if not exists
  unsigned int GetStreamFragmentIndexBetweenPositions(int64_t position);

  // returns indexes where stream fragment have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => stream fragment have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => stream fragment have to be placed on end
  // startIndex == endIndex => stream fragment with same start position exists in collection (index of stream fragment is startIndex)
  // stream fragment have to be placed between startIndex and endIndex
  // @param position : the start position to compare
  // @param startIndex : reference to variable which holds start index where stream fragment have to be placed
  // @param endIndex : reference to variable which holds end index where stream fragment have to be placed
  // @return : true if successful, false otherwise
  bool GetStreamFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex);

  // gets start index of stream fragment to start searching for specific position
  // @return : start index of stream fragment to start searching for specific position or STREAM_FRAGMENT_INDEX_NOT_SET if not set
  unsigned int GetStartSearchingIndex(void);

  // gets count of stream fragments to search for specific position
  // @return : count of stream fragments to search for specific position
  unsigned int GetSearchCount(void);

  /* set methods */

  // sets start index of stream fragment to start searching for specific position
  // @param startSearchingIndex : start index of stream fragment to start searching for specific position or STREAM_FRAGMENT_INDEX_NOT_SET if not set
  void SetStartSearchingIndex(unsigned int startSearchingIndex);

  // sets count of stream fragments to search for specific position
  // @param searchCount : count of stream fragments to search for specific position
  void SetSearchCount(unsigned int searchCount);

  /* other methods */

  // insert item to collection
  // @param position : zero-based position to insert new item
  // @param item : item to insert
  // @result : true if successful, false otherwise
  virtual bool Insert(unsigned int position, CCacheFileItem *item);

protected:

  // holds start index of stream fragment to start searching for specific position
  unsigned int startSearchingIndex;
  // holds stream fragments count to search for specific position
  unsigned int searchCount;
};

#endif