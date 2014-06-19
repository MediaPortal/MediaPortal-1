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

#include "FreeSpaceCollection.h"

#include <assert.h>

CFreeSpaceCollection::CFreeSpaceCollection(HRESULT *result)
  : CCollection(result)
{
}

CFreeSpaceCollection::~CFreeSpaceCollection(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CFreeSpaceCollection::AddFreeSpace(int64_t start, int64_t length)
{
  bool result = ((start >= 0) && (length >= 0));

  if (result)
  {
    bool merged = false;

    for (unsigned int i = 0; i < this->Count(); i++)
    {
      CFreeSpace *freeSpace = this->GetItem(i);

      if ((freeSpace->GetStart() + freeSpace->GetLength()) == start)
      {
        // free space to be added on end of freeSpace
        freeSpace->SetLength(freeSpace->GetLength() + length);
        merged = true;
        break;
      }
      else if (freeSpace->GetStart() == (start + length))
      {
        // free space to be added on begin of freeSpace
        freeSpace->SetStart(start);
        freeSpace->SetLength(freeSpace->GetLength() + length);
        merged = true;
        break;
      }
    }

    if (result && (!merged))
    {
      // not merged free space, just add to free space
      HRESULT res = S_OK;
      CFreeSpace *freeSpace = new CFreeSpace(&res);
      result &= ((freeSpace != NULL) && SUCCEEDED(res));

      if (result)
      {
        freeSpace->SetStart(start);
        freeSpace->SetLength(length);

        result &= this->Add(freeSpace);
      }

      CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(freeSpace));
    }

    while (result && merged)
    {
      merged = false;
      unsigned int count = this->Count();

      for (unsigned int i = 0; (result && (i < count) && (!merged)); i++)
      {
        CFreeSpace *first = this->GetItem(i);

        for (unsigned int j = (i + 1); (result && (j < count) && (!merged)); j++)
        {
          CFreeSpace *second = this->GetItem(j);

          if ((second->GetStart() + second->GetLength()) == first->GetStart())
          {
            first->SetStart(second->GetStart());
            first->SetLength(first->GetLength() + second->GetLength());
            this->Remove(j);
            merged = true;
            break;
          }
          else if (second->GetStart() == (first->GetStart() + first->GetLength()))
          {
            first->SetLength(first->GetLength() + second->GetLength());
            this->Remove(j);
            merged = true;
            break;
          }
        }
      }
    }
  }

  return result;
}

bool CFreeSpaceCollection::RemoveFreeSpace(unsigned int index, int64_t length)
{
  bool result = ((index < this->Count()) && (length >= 0));

  if (result)
  {
    CFreeSpace *freeSpace = this->GetItem(index);
    result &= (freeSpace->GetLength() >= length);

    if (result)
    {
      if (freeSpace->GetLength() == length)
      {
        // the length is exactly same, remove free space item
        result &= this->Remove(index);
      }
      else
      {
        // change free space item start and length, but do not remove (still some free space)
        int64_t end = freeSpace->GetStart() + freeSpace->GetLength();
        freeSpace->SetLength(freeSpace->GetLength() - length);
        freeSpace->SetStart(end - freeSpace->GetLength());
      }
    }
  }

  return result;
}

bool CFreeSpaceCollection::GetItemInsertPosition(int64_t start, unsigned int *startIndex, unsigned int *endIndex)
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

        // get free space at middle index
        CFreeSpace *freeSpace = this->GetItem(middle);

        // free item key is start time

        // tests free space start byte value
        if (start > freeSpace->GetStart())
        {
          // key is bigger than free space start byte
          // search in top half
          first = middle + 1;
        }
        else if (start < freeSpace->GetStart()) 
        {
          // key is lower than free space start byte
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found free space with same starting byte as key
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found media packet
        // it means that free space with 'key' belongs between first and last
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

unsigned int CFreeSpaceCollection::FindSuitableFreeSpace(int64_t length)
{
  unsigned int index = FREE_SPACE_NOT_FOUND;
  int64_t minLength = INT64_MAX;

  // we assume that free space collection contains small count of free spaces
  for (unsigned int i = 0; i < this->Count(); i++)
  {
    CFreeSpace *freeSpace = this->GetItem(i);

    if ((freeSpace->GetLength() >= length) && (minLength > freeSpace->GetLength()))
    {
      // last stored length is greater then current free space length
      // we trying to find as closest free space length as possible

      index = i;
      minLength = freeSpace->GetLength();
    }
  }

  return index;
}

/* protected methods */

bool CFreeSpaceCollection::Add(CFreeSpace *item)
{
  if (item == NULL)
  {
    return false;
  }

  if (!this->EnsureEnoughSpace(this->Count() + 1))
  {
    return false;
  }

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->GetItemInsertPosition(item->GetStart(), &startIndex, &endIndex))
  {
    if (startIndex == endIndex)
    {
      // media packet exists in collection
      return false;
    }

    CFreeSpace *startFreeSpace = (startIndex == UINT_MAX) ? NULL : this->GetItem(startIndex);
    CFreeSpace *endFreeSpace = (endIndex == UINT_MAX) ? NULL : this->GetItem(endIndex);

    // everything after endIndex must be moved
    if (this->itemCount > 0)
    {
      for (unsigned int i = this->itemCount; i > endIndex; i--)
      {
        *(this->items + i) = *(this->items + i - 1);
      }
    }

    if (endIndex == UINT_MAX)
    {
      // the free space have to be added after all free spaces
      endIndex = this->itemCount;
    }

    // add new item to collection and increase item count
    *(this->items + endIndex) = item;
    this->itemCount++;
  }

  return true;
}

CFreeSpace *CFreeSpaceCollection::Clone(CFreeSpace *item)
{
  return NULL;
}