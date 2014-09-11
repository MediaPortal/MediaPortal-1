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

#include "StreamFragmentCollection.h"
#include "AfhsSegmentFragment.h"
#include "IndexedAfhsSegmentFragmentCollection.h"

class CAfhsSegmentFragmentCollection : public CStreamFragmentCollection
{
public:
  CAfhsSegmentFragmentCollection(HRESULT *result);
  ~CAfhsSegmentFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CAfhsSegmentFragment *GetItem(unsigned int index);

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

  // sets default base url for all segments and fragments
  // @param baseUrl : default base url to set
  // @return : true if successful, false otherwise
  bool SetBaseUrl(const wchar_t *baseUrl);

  // sets extra parameters for all segments and fragments
  // @param segmentFragmentUrlExtraParameters : segment and fragment URL extra parameters to set
  // @return : true if successful, false otherwise
  bool SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters);

  /* other methods */

  // tests if in collection are some ecnrypted segment fragments
  // @return : true if in collection are some encrypted segment fragments, false otherwise
  bool HasEncryptedSegmentFragments(void);

  // tests if in collection are some decrypted segment fragments
  // @return : true if in collection are some decrypted segment fragments, false otherwise
  bool HasDecryptedSegmentFragments(void);

  // recalculate decrypted segment fragments start positions based on previous stream fragments
  // @param startIndex : the index of first stream fragment to recalculate start position
  //void RecalculateDecryptedSegmentFragmentStartPosition(unsigned int startIndex);

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