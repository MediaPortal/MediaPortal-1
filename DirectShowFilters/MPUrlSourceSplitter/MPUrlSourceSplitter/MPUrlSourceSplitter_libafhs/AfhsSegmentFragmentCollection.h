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

#ifndef __AFHS_SEGMENT_FRAGMENT_COLLECTION_DEFINED
#define __AFHS_SEGMENT_FRAGMENT_COLLECTION_DEFINED

#include "CacheFileItemCollection.h"
#include "AfhsSegmentFragment.h"
#include "IndexedAfhsSegmentFragmentCollection.h"

#define SEGMENT_FRAGMENT_INDEX_NOT_SET                                UINT_MAX

class CAfhsSegmentFragmentCollection : public CCacheFileItemCollection
{
public:
  CAfhsSegmentFragmentCollection(HRESULT *result);
  ~CAfhsSegmentFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CAfhsSegmentFragment *GetItem(unsigned int index);

  // gets index of stream segment where position is between start position and end position
  // @param position : the position between start position and end position
  // @return : index of segment fragment or UINT_MAX if not exists
  unsigned int GetSegmentFragmentIndexBetweenPositions(int64_t position);

  // returns indexes where segment fragment have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => segment fragment have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => segment fragment have to be placed on end
  // startIndex == endIndex => segment fragment with same start position exists in collection (index of segment fragment is startIndex)
  // segment fragment have to be placed between startIndex and endIndex
  // @param position : the start position to compare
  // @param startIndex : reference to variable which holds start index where segment fragment have to be placed
  // @param endIndex : reference to variable which holds end index where segment fragment have to be placed
  // @return : true if successful, false otherwise
  bool GetSegmentFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex);

  // gets start index of segment fragment to start searching for specific position
  // @return : start index of segment fragment to start searching for specific position or SEGMENT_FRAGMENT_INDEX_NOT_SET if not set
  unsigned int GetStartSearchingIndex(void);

  // gets count of segment fragments to search for specific position
  // @return : count of segment fragments to search for specific position
  unsigned int GetSearchCount(void);

  // gets default url for segment and fragment
  // @param segmentFragment : the segment and fragment to get default url
  // @return : default url for segment and fragment or NULL if error
  wchar_t *GetSegmentFragmentUrl(CAfhsSegmentFragment *segmentFragment);

  // gets default base url for all segments and fragment
  // @return : default base url for all segments and fragments or NULL if error
  const wchar_t *GetBaseUrl(void);

  // gets extra parameters for all segments and fragment
  // @return : extra parameters for all segments and fragments or NULL if error
  const wchar_t *GetSegmentFragmentUrlExtraParameters(void);

  // gets collection of indexed segment fragments which are encrypted
  // @param collection : the collection to fill in indexed segment fragment
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT GetEncryptedStreamFragments(CIndexedAfhsSegmentFragmentCollection *collection);

  // gets collection of indexed segment fragments which are decrypted
  // @param collection : the collection to fill in indexed segment fragment
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT GetDecryptedStreamFragments(CIndexedAfhsSegmentFragmentCollection *collection);

  /* set methods */

  // sets start index of segment fragment to start searching for specific position
  // @param startSearchingIndex : start index of segment fragment to start searching for specific position or SEGMENT_FRAGMENT_INDEX_NOT_SET if not set
  void SetStartSearchingIndex(unsigned int startSearchingIndex);

  // sets count of segment fragments to search for specific position
  // @param searchCount : count of segment fragments to search for specific position
  void SetSearchCount(unsigned int searchCount);

  // sets default base url for all segments and fragments
  // @param baseUrl : default base url to set
  // @return : true if successful, false otherwise
  bool SetBaseUrl(const wchar_t *baseUrl);

  // sets extra parameters for all segments and fragments
  // @param segmentFragmentUrlExtraParameters : segment and fragment URL extra parameters to set
  // @return : true if successful, false otherwise
  bool SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters);

  /* other methods */

  // insert item to collection
  // @param position : zero-based position to insert new item
  // @param item : item to insert
  // @result : true if successful, false otherwise
  virtual bool Insert(unsigned int position, CCacheFileItem *item);

  // tests if in collection are some ecnrypted segment fragments
  // @return : true if in collection are some encrypted segment fragments, false otherwise
  bool HasEncryptedSegmentFragments(void);

  // tests if in collection are some decrypted segment fragments
  // @return : true if in collection are some decrypted segment fragments, false otherwise
  bool HasDecryptedSegmentFragments(void);

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
  // holds start index of segment fragment to start searching for specific position
  unsigned int startSearchingIndex;
  // holds segment fragments count to search for specific position
  unsigned int searchCount;
  // holds default base url for all segments and fragments
  wchar_t *defaultBaseUrl;
  // holds extra parameters which are added to all segments and fragments
  wchar_t *segmentFragmentUrlExtraParameters;

  // we need to maintain several indexes
  // first index : item->IsEncrypted()
  // second index : item->IsDecrypted()

  CIndexCollection *indexEncrypted;
  CIndexCollection *indexDecrypted;
};

#endif