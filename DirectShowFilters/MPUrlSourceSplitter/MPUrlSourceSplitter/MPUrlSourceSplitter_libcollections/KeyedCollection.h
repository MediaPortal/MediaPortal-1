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

#ifndef __KEYED_COLLECTION_DEFINED
#define __KEYED_COLLECTION_DEFINED

#include "Collection.h"

template <class TItem, class TItemKey> class CKeyedCollection : public CCollection<TItem>
{
public:
  // create new instance of CKeyedCollection class
  CKeyedCollection();

  virtual ~CKeyedCollection(void);

  // test if item exists in collection
  // @param key : item key to find
  // @param context : the reference to user defined context
  // @return : true if item exists, false otherwise
  virtual bool Contains(TItemKey key, void *context);

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual TItem *GetItem(unsigned int index);

  // get the item from collection with specified key
  // @param key : item key to find
  // @param context : the reference to user defined context
  // @return : the reference to item or NULL if not find
  virtual TItem *GetItem(TItemKey key, void *context);

  // remove item with specified index from collection
  // @param index : the index of item to remove
  // @return : true if removed, false otherwise
  virtual bool Remove(unsigned int index);

  // removes count of items from collection from specified index
  // @param index : the index of item to start removing
  // @param count : the count of items to remove
  // @return : true if removed, false otherwise
  virtual bool Remove(unsigned int index, unsigned int count);

  // remove item with specified key from collection
  // @param key : key of item to remove
  // @param context : the reference to user defined context
  // @return : true if removed, false otherwise
  virtual bool Remove(TItemKey key, void *context);

  // get item index of item with specified key
  // @param key : the key of item to find
  // @param context : reference to user defined context
  // @return : the index of item or UINT_MAX if not found
  virtual unsigned int GetItemIndex(TItemKey key, void *context);

  // updates item in collection identified by key and context with item value
  // if item is not in collection then item is added to collection
  // @param key : the key of item to update
  // @param context : reference to user defined context (used to find item to update)
  // @param item : new item value
  // @return : true if successfully updated or added, false otherwise
  virtual bool Update(TItemKey key, void *context, TItem *item);

protected:

  // compare two items
  // @param firstItem : the first item to compare
  // @param secondItem : the second item to compare
  // @param context : the reference to user defined context
  // @return : 0 if items are equal, lower than zero if firstItem is lower than secondItem, greater than zero if firstItem is greater than secondItem
  virtual int CompareItems(TItem *firstItem, TItem *secondItem, void *context);

  // compare two item keys
  // @param firstKey : the first item key to compare
  // @param secondKey : the second item key to compare
  // @param context : the reference to user defined context
  // @return : 0 if keys are equal, lower than zero if firstKey is lower than secondKey, greater than zero if firstKey is greater than secondKey
  virtual int CompareItemKeys(TItemKey firstKey, TItemKey secondKey, void *context) = 0;

  // gets key for item
  // @param item : the item to get key
  // @return : the key of item
  virtual TItemKey GetKey(TItem *item) = 0;
};

// implementation

template <class TItem, class TItemKey> CKeyedCollection<TItem, TItemKey>::CKeyedCollection()
  : CCollection()
{
}

template <class TItem, class TItemKey> CKeyedCollection<TItem, TItemKey>::~CKeyedCollection(void)
{
}

template <class TItem, class TItemKey> bool CKeyedCollection<TItem, TItemKey>::Contains(TItemKey key, void *context)
{
  return (this->GetItem(key, context) != NULL);
}

template <class TItem, class TItemKey> TItem *CKeyedCollection<TItem, TItemKey>::GetItem(unsigned int index)
{
  return __super::GetItem(index);
}

template <class TItem, class TItemKey> TItem *CKeyedCollection<TItem, TItemKey>::GetItem(TItemKey key, void *context)
{
  TItem *result = NULL;
  unsigned int index = this->GetItemIndex(key, context);

  if (index != UINT_MAX)
  {
    result = *(this->items + index);
  }

  return result;
}

template <class TItem, class TItemKey> int CKeyedCollection<TItem, TItemKey>::CompareItems(TItem *firstItem, TItem *secondItem, void *context)
{
  TItemKey firstItemKey = this->GetKey(firstItem);
  TItemKey secondItemKey = this->GetKey(secondItem);

  int result = this->CompareItemKeys(firstItemKey, secondItemKey, context);

  return result;
}

template <class TItem, class TItemKey> unsigned int CKeyedCollection<TItem, TItemKey>::GetItemIndex(TItemKey key, void *context)
{
  unsigned int result = UINT_MAX;

  for(unsigned int i = 0; i < this->itemCount; i++)
  {
    TItem *item = *(this->items + i);
    TItemKey itemKey = this->GetKey(item);

    if (this->CompareItemKeys(key, itemKey, context) == 0)
    {
      result = i;
      break;
    }
  }

  return result;
}

template <class TItem, class TItemKey> bool CKeyedCollection<TItem, TItemKey>::Remove(unsigned int index)
{
  return __super::Remove(index);
}

template <class TItem, class TItemKey> bool CKeyedCollection<TItem, TItemKey>::Remove(unsigned int index, unsigned int count)
{
  return __super::Remove(index, count);
}

template <class TItem, class TItemKey> bool CKeyedCollection<TItem, TItemKey>::Remove(TItemKey key, void *context)
{
  unsigned int index = this->GetItemIndex(key, context);

  if (index != UINT_MAX)
  {
    return this->Remove(index);
  }
  else
  {
    return false;
  }
}

template <class TItem, class TItemKey> bool CKeyedCollection<TItem, TItemKey>::Update(TItemKey key, void *context, TItem *item)
{
  unsigned int index = this->GetItemIndex(key, context);
  bool result = true;

  if (index != UINT_MAX)
  {
    result = this->Remove(index);
  }
  
  if (result)
  {
    result = this->Add(item);
  }

  return result;
}

#endif

