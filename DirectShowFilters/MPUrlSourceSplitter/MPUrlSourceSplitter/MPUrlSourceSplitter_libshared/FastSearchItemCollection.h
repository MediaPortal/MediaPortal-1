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

#ifndef __FAST_SEARCH_ITEM_COLLECTION_DEFINED
#define __FAST_SEARCH_ITEM_COLLECTION_DEFINED

#include "Collection.h"
#include "IndexCollection.h"
#include "FastSearchItem.h"
#include "IndexedFastSearchItemCollection.h"

class CFastSearchItemCollection : public CCollection<CFastSearchItem>
{
public:
  // create new instance of CFastSearchItemCollection class
  CFastSearchItemCollection(HRESULT *result);
  virtual ~CFastSearchItemCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CFastSearchItem *GetItem(unsigned int index);

  /* set methods */

  /* other methods */

  // add item to collection
  // @param item : the reference to item to add
  // @return : true if successful, false otherwise
  virtual bool Add(CFastSearchItem *item);

  // insert item to collection
  // @param position : zero-based position to insert new item
  // @param item : item to insert
  // @return : true if successful, false otherwise
  virtual bool Insert(unsigned int position, CFastSearchItem *item);

  // clear collection of items
  virtual void Clear(void);

  // removes count of items from collection from specified index
  // @param index : the index of item to start removing
  // @param count : the count of items to remove
  // @return : true if removed, false otherwise
  virtual bool Remove(unsigned int index, unsigned int count);

  /* index methods */

  // insert item with specified item index to indexes
  // @param itemIndex : the item index in collection to insert into indexes
  // @return : true if successful, false otherwise
  virtual bool InsertIndexes(unsigned int itemIndex);

  // removes item with specified item index from indexes
  // @param itemIndex : the item index in collection to remove from indexes
  virtual void RemoveIndexes(unsigned int itemIndex);

  // removes items from fast search collection indexes
  // items don't exist when calling this method
  // this method MUST NOT call any other overloaded method, it is used in InsertIndexes() method when inserting to indexes failed
  // @param startIndex : the start index of items to remove from indexes
  // @param count : the count of items to remove from indexes
  virtual void RemoveIndexes(unsigned int startIndex, unsigned int count);

  // updates fast search collection indexes
  // @param itemIndex : index of item to update indexes
  // @returm : true if successful, false otherwise
  virtual bool UpdateIndexes(unsigned int itemIndex);

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

  /* methods */

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  virtual CFastSearchItem *Clone(CFastSearchItem *item);
};

#endif