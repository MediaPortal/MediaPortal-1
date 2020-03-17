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

#ifndef __M3U8_SEGMENT_FRAGMENT_COLLECTION_DEFINED
#define __M3U8_SEGMENT_FRAGMENT_COLLECTION_DEFINED

#include "StreamFragmentCollection.h"
#include "M3u8StreamFragment.h"
#include "IndexedM3u8StreamFragmentCollection.h"

class CM3u8StreamFragmentCollection : public CStreamFragmentCollection
{
public:
  CM3u8StreamFragmentCollection(HRESULT *result);
  ~CM3u8StreamFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CM3u8StreamFragment *GetItem(unsigned int index);

  // gets collection of indexed stream fragments which are encrypted
  // @param collection : the collection to fill in indexed stream fragment
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT GetEncryptedStreamFragments(CIndexedM3u8StreamFragmentCollection *collection);

  // gets collection of indexed stream fragments which are decrypted
  // @param collection : the collection to fill in indexed stream fragment
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT GetDecryptedStreamFragments(CIndexedM3u8StreamFragmentCollection *collection);

  /* set methods */

  /* other methods */

  /* other methods */

  // tests if in collection are some ecnrypted stream fragments
  // @return : true if in collection are some encrypted stream fragments, false otherwise
  bool HasEncryptedStreamFragments(void);

  // tests if in collection are some decrypted stream fragments
  // @return : true if in collection are some decrypted stream fragments, false otherwise
  bool HasDecryptedStreamFragments(void);

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

  // we need to maintain several indexes
  // first index : item->IsEncrypted()
  // second index : item->IsDecrypted()

  CIndexCollection *indexEncrypted;
  CIndexCollection *indexDecrypted;

  /* methods */
};

#endif