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

#ifndef __COLLECTION_DEFINED
#define __COLLECTION_DEFINED

template <class TItem, class TItemKey> class CCollection
{
public:
  enum FreeItemMethod
  {
    Delete,
    FreeMem
  };

  // create new instance of CCollection class
  // @param freeItemMethod : method of removing items from memory
  CCollection(FreeItemMethod freeItemMethod);

  virtual ~CCollection(void);

  // add item to collection
  // @param item : the reference to item to add
  // @return : true if successful, false otherwise
  virtual bool Add(TItem *item);

  // append collection of items
  // @param collection : the reference to collection to add
  void Append(CCollection<TItem, TItemKey> *collection);

  // clear collection of items
  void Clear(void);

  // test if item exists in collection
  // @param key : item key to find
  // @param context : the reference to user defined context
  // @return : true if item exists, false otherwise
  bool Contains(TItemKey key, void *context);

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  TItem *GetItem(unsigned int index);

  // get the item from collection with specified key
  // @param key : item key to find
  // @param context : the reference to user defined context
  // @return : the reference to item or NULL if not find
  TItem *GetItem(TItemKey key, void *context);

  // get count of items in collection
  // @return : count of items in collection
  unsigned int Count(void);

  // remove item with specified index from collection
  // @param index : the index of item to remove
  // @return : true if removed, false otherwise
  bool Remove(unsigned int index);

  // remove item with specified key from collection
  // @param key : key of item to remove
  // @param context : the reference to user defined context
  // @return : true if removed, false otherwise
  bool Remove(TItemKey key, void *context);

  // get item index of item with specified key
  // @param key : the key of item to find
  // @param context : reference to user defined context
  // @return : the index of item or UINT_MAX if not found
  virtual unsigned int GetItemIndex(TItemKey key, void *context);

protected:
  // pointer to array of pointers to items
  TItem **items;

  // count of items in collection
  unsigned int itemCount;

  // maximum count of items to store in collection
  unsigned int itemMaximumCount;

  // method of removing items from memory
  FreeItemMethod freeItemMethod;

  // delete item from memory
  // @param item : item to delete from memory
  virtual void FreeItem(TItem *item);

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
  // caller is responsible of deleting item key using FreeKey() method
  // @param item : the item to get key
  // @return : the key of item
  virtual TItemKey GetKey(TItem *item) = 0;

  // frees item key
  // @param key : the item to free
  virtual void FreeKey(TItemKey key) = 0;

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  virtual TItem *Clone(TItem *item) = 0;

  // ensures that in internal buffer is enough space
  // if in internal buffer is not enough space, method tries to allocate enough space
  // @param requestedCount : the requested count of items
  // @return : true if in internal buffer is enough space, false otherwise
  bool EnsureEnoughSpace(unsigned int requestedCount);
};

// implementation

template <class TItem, class TItemKey> CCollection<TItem, TItemKey>::CCollection(FreeItemMethod freeItemMethod)
{
  this->itemCount = 0;
  this->itemMaximumCount = 16;
  this->freeItemMethod = freeItemMethod;
  this->items = ALLOC_MEM_SET(this->items, TItem *, this->itemMaximumCount, 0);
}

template <class TItem, class TItemKey> CCollection<TItem, TItemKey>::~CCollection(void)
{
  this->Clear();

  FREE_MEM(this->items);
}

template <class TItem, class TItemKey> void CCollection<TItem, TItemKey>::FreeItem(TItem *item)
{
  switch (this->freeItemMethod)
  {
  case Delete:
    delete item;
    break;
  case FreeMem:
    FREE_MEM(item);
    break;
  default:
    delete item;
    break;
  }
}

template <class TItem, class TItemKey> void CCollection<TItem, TItemKey>::Clear(void)
{
  // call destructors of all items
  for(unsigned int i = 0; i < this->itemCount; i++)
  {
    this->FreeItem(*(this->items + i));
  }

  // set used items to 0
  this->itemCount = 0;
}

template <class TItem, class TItemKey> bool CCollection<TItem, TItemKey>::EnsureEnoughSpace(unsigned int requestedCount)
{
  if (requestedCount >= this->itemMaximumCount)
  {
    // there is need to enlarge array of items
    this->itemMaximumCount = requestedCount;

    TItem **itemArray = REALLOC_MEM(this->items, TItem *, this->itemMaximumCount);

    if (itemArray == NULL)
    {
      return false;
    }

    this->items = itemArray;
  }

  return true;
}

template <class TItem, class TItemKey> bool CCollection<TItem, TItemKey>::Add(TItem *item)
{
  if (item == NULL)
  {
    return false;
  }

  if (!this->EnsureEnoughSpace(this->Count() + 1))
  {
    return false;
  }

  *(this->items + this->itemCount++) = item;
  return true;
}


template <class TItem, class TItemKey> void CCollection<TItem, TItemKey>::Append(CCollection<TItem, TItemKey> *collection)
{
  if (collection != NULL)
  {
    unsigned int count = collection->Count();
    for (unsigned int i = 0; i < count; i++)
    {
      this->Add(this->Clone(collection->GetItem(i)));
    }
  }
}

template <class TItem, class TItemKey> unsigned int CCollection<TItem, TItemKey>::Count(void)
{
  return this->itemCount;
}

template <class TItem, class TItemKey> bool CCollection<TItem, TItemKey>::Contains(TItemKey key, void *context)
{
  return (this->GetItem(key, context) != NULL);
}

template <class TItem, class TItemKey> TItem *CCollection<TItem, TItemKey>::GetItem(unsigned int index)
{
  TItem *result = NULL;
  if (index <= this->itemCount)
  {
    result = *(this->items + index);
  }
  return result;
}

template <class TItem, class TItemKey> TItem *CCollection<TItem, TItemKey>::GetItem(TItemKey key, void *context)
{
  TItem *result = NULL;
  unsigned int index = this->GetItemIndex(key, context);

  if (index != UINT_MAX)
  {
    result = *(this->items + index);
  }

  return result;
}

template <class TItem, class TItemKey> int CCollection<TItem, TItemKey>::CompareItems(TItem *firstItem, TItem *secondItem, void *context)
{
  TItemKey firstItemKey = this->GetKey(firstItem);
  TItemKey secondItemKey = this->GetKey(secondItem);

  int result = this->CompareItemKeys(firstItemKey, secondItemKey, context);

  this->FreeKey(firstItemKey);
  this->FreeKey(secondItemKey);

  return result;
}

template <class TItem, class TItemKey> unsigned int CCollection<TItem, TItemKey>::GetItemIndex(TItemKey key, void *context)
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

    this->FreeKey(itemKey);
  }

  return result;
}

template <class TItem, class TItemKey> bool CCollection<TItem, TItemKey>::Remove(unsigned int index)
{
  bool result = false;

  if ((index >= 0) && (index < this->itemCount))
  {
    // delete item on specified index
    this->FreeItem(*(this->items + index));
    // move rest of items
    for (unsigned int i = (index + 1); i < this->itemCount; i++)
    {
      *(this->items + i - 1) = *(this->items + i);
    }

    this->itemCount--;
    result = true;
  }

  return result;
}

template <class TItem, class TItemKey> bool CCollection<TItem, TItemKey>::Remove(TItemKey key, void *context)
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

#endif

