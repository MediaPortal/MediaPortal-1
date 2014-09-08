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

#ifndef __MSHS_STREAM_FRAGMENT_COLLECTION_DEFINED
#define __MSHS_STREAM_FRAGMENT_COLLECTION_DEFINED

#include "CacheFileItemCollection.h"
#include "MshsStreamFragment.h"
#include "IndexedMshsStreamFragmentCollection.h"

#define SEGMENT_FRAGMENT_INDEX_NOT_SET                                UINT_MAX

class CMshsStreamFragmentCollection : public CCacheFileItemCollection
{
public:
  CMshsStreamFragmentCollection(HRESULT *result);
  ~CMshsStreamFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CMshsStreamFragment *GetItem(unsigned int index);

  // gets index of stream fragment where position is between start position and end position
  // @param position : the position between start position and end position
  // @return : index of stream fragment or UINT_MAX if not exists
  unsigned int GetStreamFragmentIndexBetweenPositions(int64_t position);

  // returns indexes where stream fragment have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => stream fragment have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => strem fragment have to be placed on end
  // startIndex == endIndex => stream fragment with same start position exists in collection (index of stream fragment is startIndex)
  // strem fragment have to be placed between startIndex and endIndex
  // @param position : the start position to compare
  // @param startIndex : reference to variable which holds start index where strem fragment have to be placed
  // @param endIndex : reference to variable which holds end index where stream fragment have to be placed
  // @return : true if successful, false otherwise
  bool GetStreamFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex);

  // gets start index of stream fragment to start searching for specific position
  // @return : start index of stream fragment to start searching for specific position or SEGMENT_FRAGMENT_INDEX_NOT_SET if not set
  unsigned int GetStartSearchingIndex(void);

  // gets count of stream fragments to search for specific position
  // @return : count of stream fragments to search for specific position
  unsigned int GetSearchCount(void);

  // gets collection of indexed stream fragments which are ready for processing
  // @param collection : the collection to fill in indexed stream fragment
  // @return : S_OK if successful, error code otherwise
  HRESULT GetReadyForProcessingStreamFragments(CIndexedMshsStreamFragmentCollection *collection);

  // gets first not ready for processing and not downloaded stream fragment index
  // @param itemIndex : item index to start searching
  // @return : index of first not ready for processing and not downloaded stream fragment or UINT_MAX if not exists
  unsigned int GetFirstNotReadyForProcessingNotDownloadedStreamFragmentIndex(unsigned int itemIndex);

  /* set methods */

  // sets start index of stream fragment to start searching for specific position
  // @param startSearchingIndex : start index of stream fragment to start searching for specific position or SEGMENT_FRAGMENT_INDEX_NOT_SET if not set
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

  // tests if in collection are some ready for processing stream fragments
  // @return : true if in collection are some ready for processing stream fragments, false otherwise
  bool HasReadyForProcessingStreamFragments(void);

  /* index methods */

  // insert item with specified item index to indexes
  // @param itemIndex : the item index in collection to insert into indexes
  // @return : true if successful, false otherwise
  virtual bool InsertIndexes(unsigned int itemIndex);

  // removes items from indexes
  // @param startIndex : the start index of items to remove from indexes
  // @param count : the count of items to remove from indexes
  virtual void RemoveIndexes(unsigned int startIndex, unsigned int count);

  // updates indexes by using specified item
  // @param itemIndex : index of item to update indexes
  // @param count : the count of items to updates indexes
  // @retur : true if successful, false otherwise
  virtual bool UpdateIndexes(unsigned int itemIndex, unsigned int count);

  // ensures that in internal buffer of indexes is enough space
  // each index must check against its count of items and add addingCount
  // if in internal buffer of indexes is not enough space, method tries to allocate enough space in index
  // @param addingCount : the count of added index items
  // @return : true if in internal buffer of indexes is enough space, false otherwise
  virtual bool EnsureEnoughSpaceIndexes(unsigned int addingCount);

  // clears all indexes to default state
  virtual void ClearIndexes(void);

protected:
  // holds start index of stream fragment to start searching for specific position
  unsigned int startSearchingIndex;
  // holds stream fragments count to search for specific position
  unsigned int searchCount;

  // we need to maintain several indexes
  // first index : item->IsReadyForProcessing()
  // second index : (!item->IsReadyForProcessing()) && (!item->IsDownloaded())

  CIndexCollection *indexReadyForProcessing;
  CIndexCollection *indexNotReadyForProcessingNotDownloaded;
};

#endif