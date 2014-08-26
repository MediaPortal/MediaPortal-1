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

#include "FastSearchItemCollection.h"

CFastSearchItemCollection::CFastSearchItemCollection(HRESULT *result)
  : CCollection(result)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }
}

CFastSearchItemCollection::~CFastSearchItemCollection(void)
{
}

/* get methods */

CFastSearchItem *CFastSearchItemCollection::GetItem(unsigned int index)
{
  return (CFastSearchItem *)__super::GetItem(index);
}

/* set methods */

/* other methods */

bool CFastSearchItemCollection::Add(CFastSearchItem *item)
{
  // we are adding one item
  bool result = this->EnsureEnoughSpaceIndexes(1);

  if (result)
  {
    result &= __super::Add(item);

    if (result)
    {
      item->SetOwner(this);
      result &= this->InsertIndexes(this->itemCount - 1);

      // revert adding to collection
      CHECK_CONDITION_EXECUTE(!result, this->RemoveWithoutDestroyInstance(this->itemCount - 1));
    }
  }

  return result;
}

bool CFastSearchItemCollection::Insert(unsigned int position, CFastSearchItem *item)
{
  // we are inserting one item
  bool result = this->EnsureEnoughSpaceIndexes(1);

  if (result)
  {
    result &= __super::Insert(position, item);

    if (result)
    {
      item->SetOwner(this);
      result &= this->InsertIndexes(position);

      // revert inserting to collection
      CHECK_CONDITION_EXECUTE(!result, this->RemoveWithoutDestroyInstance(position));
    }
  }

  return result;
}

void CFastSearchItemCollection::Clear(void)
{
  // clear collection
  __super::Clear();
  // clear all indexes
  this->ClearIndexes();
}

bool CFastSearchItemCollection::Remove(unsigned int index, unsigned int count)
{
  if (__super::Remove(index, count))
  {
    this->RemoveIndexes(index, count);

    return true;
  }

  return false;
}

/* indexes methods */

bool CFastSearchItemCollection::InsertIndexes(unsigned int itemIndex)
{
  // item with specified index is already added to collection
  // it is also ensured that there is enough space in index

  return true;
}

void CFastSearchItemCollection::RemoveIndexes(unsigned int itemIndex)
{
  this->RemoveIndexes(itemIndex, 1);
}

void CFastSearchItemCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
{
  // item with specified index is already removed from collection
}

bool CFastSearchItemCollection::UpdateIndexes(unsigned int itemIndex)
{
  return this->UpdateIndexes(itemIndex, 1);
}

bool CFastSearchItemCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  return true;
}

bool CFastSearchItemCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  return true;
}

void CFastSearchItemCollection::ClearIndexes(void)
{
}

/* protected methods */

CFastSearchItem *CFastSearchItemCollection::Clone(CFastSearchItem *item)
{
  return item->Clone();
}