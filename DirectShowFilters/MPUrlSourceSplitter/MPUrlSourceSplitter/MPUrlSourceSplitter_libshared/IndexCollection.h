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

#ifndef __INDEX_COLLECTION_DEFINED
#define __INDEX_COLLECTION_DEFINED

class CIndexCollection
{
public:
  CIndexCollection(HRESULT *result);
  virtual ~CIndexCollection(void);

  /* get methods */

  // gets the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or UINT_MAX if not found
  virtual unsigned int GetItem(unsigned int index);

  // gets the item index in collection
  // @param item : the item to find in collection
  // @return : the index of item or UINT_MAX if not found
  virtual unsigned int GetItemIndex(unsigned int item);

  /* set methods */

  /* other methods */

  // adds item to collection
  // @param item : the reference to item to add
  // @return : true if successful, false otherwise
  virtual bool Add(unsigned int item);

  // inserts item to collection
  // @param position : zero-based position to insert new item
  // @param item : item to insert
  // @result : true if successful, false otherwise
  virtual bool Insert(unsigned int position, unsigned int item);

  // inserts items to collection
  // @param position : zero-based position to insert new items
  // @param item : item to insert
  // @param count : the count of items to insert
  // @result : true if successful, false otherwise
  virtual bool Insert(unsigned int position, unsigned int item, unsigned int count);

  // clears collection of items
  virtual void Clear(void);

  // gets count of items in collection
  // @return : count of items in collection
  virtual unsigned int Count(void);

  // removes item with specified index from collection
  // @param index : the index of item to remove
  // @return : true if removed, false otherwise
  virtual bool Remove(unsigned int index);

  // removes count of items from collection from specified index
  // @param index : the index of item to start removing
  // @param count : the count of items to remove
  // @return : true if removed, false otherwise
  virtual bool Remove(unsigned int index, unsigned int count);

  // ensures that in internal buffer is enough space
  // if in internal buffer is not enough space, method tries to allocate enough space
  // @param requestedCount : the requested count of items
  // @return : true if in internal buffer is enough space, false otherwise
  virtual bool EnsureEnoughSpace(unsigned int requestedCount);

  // returns indexes where item have to be placed
  // startIndex == UINT_MAX && endIndex == 0 => item have to be placed on beginning
  // startIndex == Count() - 1 && endIndex == UINT_MAX => item have to be placed on end
  // startIndex == endIndex => item with same value exists in collection (index of item is startIndex)
  // item have to be placed between startIndex and endIndex
  // @param position : the start position to compare
  // @param startIndex : reference to variable which holds start index where item have to be placed
  // @param endIndex : reference to variable which holds end index where item have to be placed
  // @return : true if successful, false otherwise
  virtual bool GetItemInsertPosition(unsigned int value, unsigned int *startIndex, unsigned int *endIndex);

  // tests if index contains specified index
  // @param index : the index to find in index
  // @return : true if collection contains index, false otherwise
  virtual bool Contains(unsigned int index);

  // increases all values in index starting from specified index by one
  // @param index : the item index of to start increasing
  virtual void Increase(unsigned int index);

  // decreases all values in index starting from specified index by count
  // @param index : the item index of to start decreasing
  // @param count : the count to decrease each index value
  virtual void Decrease(unsigned int index, unsigned int count);

protected:
  // pointer to array of pointers to items
  unsigned int *items;
  // count of items in collection
  unsigned int itemCount;
  // maximum count of items to store in collection
  unsigned int itemMaximumCount;

  /* methods */
};

#endif