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

#include "StdAfx.h"

#include "IndexCollection.h"

CIndexCollection::CIndexCollection(HRESULT *result)
{
  this->itemCount = 0;
  this->itemMaximumCount = 16;
  this->items = NULL;

  if ((result != NULL) && SUCCEEDED(*result))
  {
    this->items = ALLOC_MEM_SET(this->items, unsigned int, this->itemMaximumCount, 0);
    CHECK_POINTER_HRESULT(*result, this->items, *result, E_OUTOFMEMORY);
  }
}

CIndexCollection::~CIndexCollection(void)
{
  this->Clear();

  FREE_MEM(this->items);
}

/* get methods */

unsigned int CIndexCollection::GetItem(unsigned int index)
{
  unsigned int result = UINT_MAX;
  if (index < this->itemCount)
  {
    result = *(this->items + index);
  }
  return result;
}

unsigned int CIndexCollection::GetItemIndex(unsigned int item)
{
  unsigned int startIndex = 0;
  unsigned int endIndex = 0;
  unsigned int result = UINT_MAX;

  // check item position in index, also check if it not exists in collection (startIndex == endIndex)
  if (this->GetItemInsertPosition(item, &startIndex, &endIndex))
  {
    if (startIndex == endIndex)
    {
      // item exists in collection
      result = startIndex;
    }
  }

  return result;
}

/* set methods */

/* other methods */

bool CIndexCollection::Add(unsigned int item)
{
  if (this->EnsureEnoughSpace(this->itemCount + 1))
  {
    *(this->items + this->itemCount++) = item;
    return true;
  }

  return false;
}

bool CIndexCollection::Insert(unsigned int position, unsigned int item)
{
  //bool result = false;

  //if ((position >= 0) && (position <= this->itemCount))
  //{
  //  // ensure that enough space is in collection
  //  result = this->EnsureEnoughSpace(this->itemCount + 1);

  //  if (result)
  //  {
  //    // move everything after insert position
  //    unsigned int size = (this->itemCount - position) * sizeof(unsigned int);
  //    if (size > 0)
  //    {
  //      void *source = (void *)(this->items + position);
  //      void *destination = (void *)(this->items + position + 1);

  //      memmove(destination, source, size);
  //    }

  //    *(this->items + position) = item;
  //    this->itemCount++;
  //  }
  //}

  //return result;

  return this->Insert(position, item, 1);
}

bool CIndexCollection::Insert(unsigned int position, unsigned int item, unsigned int count)
{
  bool result = false;

  if ((position >= 0) && (position <= this->itemCount))
  {
    // ensure that enough space is in collection
    result = this->EnsureEnoughSpace(this->itemCount + count);

    if (result)
    {
      // move everything after insert position
      unsigned int size = (this->itemCount - position) * sizeof(unsigned int);
      if (size > 0)
      {
        void *source = (void *)(this->items + position);
        void *destination = (void *)(this->items + position + count);

        memmove(destination, source, size);
      }

      for (unsigned int i = position; i < (position + count); i++)
      {
        *(this->items + i) = item++;
      }
      this->itemCount += count;
    }
  }

  return result;
}

void CIndexCollection::Clear(void)
{
  // set used items to 0
  this->itemCount = 0;
}

unsigned int CIndexCollection::Count(void)
{
  return this->itemCount;
}

bool CIndexCollection::Remove(unsigned int index)
{
  return this->Remove(index, 1);
}

bool CIndexCollection::Remove(unsigned int index, unsigned int count)
{
  if ((count > 0) && ((index + count) <= this->itemCount))
  {
    // move rest of items
    unsigned int size = (this->itemCount - index - count) * sizeof(unsigned int);
    if (size > 0)
    {
      void *source = (void *)(this->items + index + count);
      void *destination = (void *)(this->items + index);

      memmove(destination, source, size);
    }

    this->itemCount -= count;
    return true;
  }

  return false;
}

bool CIndexCollection::EnsureEnoughSpace(unsigned int requestedCount)
{
  if (requestedCount > this->itemMaximumCount)
  {
    // there is need to enlarge array of items
    unsigned int *itemArray = REALLOC_MEM(this->items, unsigned int, requestedCount);

    if (itemArray == NULL)
    {
      return false;
    }

    this->items = itemArray;
    this->itemMaximumCount = requestedCount;
  }

  return true;
}

bool CIndexCollection::GetItemInsertPosition(unsigned int value, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = (this->Count() > 0);

    if (result)
    {
      unsigned int first = 0;
      unsigned int last = this->Count() - 1;
      result = false;

      while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
      {
        // compute middle index
        unsigned int middle = (first + last) / 2;

        // get item at middle index
        unsigned int item = this->GetItem(middle);

        // compare item value
        if (value > item)
        {
          // value is bigger than item
          // search in top half
          first = middle + 1;
        }
        else if (value < item) 
        {
          // value is lower than item
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found item with same value
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found value
        // it means that item with 'value' belongs between first and last
        *startIndex = last;
        *endIndex = (first >= this->Count()) ? UINT_MAX : first;
        result = true;
      }
    }
    else
    {
      *startIndex = UINT_MAX;
      *endIndex = 0;
      result = true;
    }
  }

  return result;
}

bool CIndexCollection::Contains(unsigned int index)
{
  return (this->GetItemIndex(index) != UINT_MAX);
}

void CIndexCollection::Increase(unsigned int index)
{
  for (unsigned int i = index; i < this->itemCount; i++)
  {
    (*(this->items + i))++;
  }
}

void CIndexCollection::Decrease(unsigned int index, unsigned int count)
{
  for (unsigned int i = index; i < this->itemCount; i++)
  {
    (*(this->items + i)) -= count;
  }
}

/* protected methods */