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

#include "CacheFileItemCollection.h"

#define AFFECTED_INDEX_NONE                                                                 FLAGS_NONE

#define AFFECTED_INDEX_BASE                                                                 (1 << (FLAGS_LAST + 0))
#define AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_STORED_TO_FILE_LOADED_TO_MEMORY_ADD             (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_STORED_TO_FILE_LOADED_TO_MEMORY_INC             (1 << (FLAGS_LAST + 2))
#define AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_NOT_STORED_TO_FILE_LOADED_TO_MEMORY_ADD         (1 << (FLAGS_LAST + 3))
#define AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_NOT_STORED_TO_FILE_LOADED_TO_MEMORY_INC         (1 << (FLAGS_LAST + 4))

CCacheFileItemCollection::CCacheFileItemCollection(HRESULT *result)
  : CFastSearchItemCollection(result)
{
  this->indexCleanUpFromMemoryStoredToFileLoadedToMemory = NULL;
  this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexCleanUpFromMemoryStoredToFileLoadedToMemory = new CIndexCollection(result);
    this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexCleanUpFromMemoryStoredToFileLoadedToMemory, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory, *result, E_OUTOFMEMORY);
  }
}

CCacheFileItemCollection::~CCacheFileItemCollection(void)
{
  FREE_MEM_CLASS(this->indexCleanUpFromMemoryStoredToFileLoadedToMemory);
  FREE_MEM_CLASS(this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory);
}

/* get methods */

CCacheFileItem *CCacheFileItemCollection::GetItem(unsigned int index)
{
  return (CCacheFileItem *)__super::GetItem(index);
}

HRESULT CCacheFileItemCollection::GetCleanUpFromMemoryStoredToFileLoadedToMemoryItems(CIndexedCacheFileItemCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Count())); i++)
  {
    unsigned int index = this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->GetItem(i);

    CIndexedCacheFileItem *item = new CIndexedCacheFileItem(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CCacheFileItemCollection::GetCleanUpFromMemoryNotStoredToFileLoadedToMemoryItems(CIndexedCacheFileItemCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Count())); i++)
  {
    unsigned int index = this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->GetItem(i);

    CIndexedCacheFileItem *item = new CIndexedCacheFileItem(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

/* set methods */

/* other methods */

/* index methods */

bool CCacheFileItemCollection::InsertIndexes(unsigned int itemIndex)
{
  uint32_t flags = AFFECTED_INDEX_NONE;
  bool result = __super::InsertIndexes(itemIndex);

  unsigned int indexCleanUpFromMemoryStoredToFileLoadedToMemoryItemIndex = UINT_MAX;
  unsigned int indexCleanUpFromMemoryNotStoredToFileLoadedToMemoryItemIndex = UINT_MAX;
  unsigned int indexNotDownloadedItemIndex = UINT_MAX;

  if (result)
  {
    flags |= AFFECTED_INDEX_BASE;

    // we must check if some index needs to be updated
    // in case of error we must revert all updated indexes
    CCacheFileItem *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexCleanUpFromMemoryStoredToFileLoadedToMemoryItemIndex = min(endIndex, this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Increase(indexCleanUpFromMemoryStoredToFileLoadedToMemoryItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_STORED_TO_FILE_LOADED_TO_MEMORY_INC);

        if ((!item->IsNoCleanUpFromMemory()) && (item->IsStoredToFile()) && (item->IsLoadedToMemory()))
        {
          result &= this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Insert(indexCleanUpFromMemoryStoredToFileLoadedToMemoryItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_STORED_TO_FILE_LOADED_TO_MEMORY_ADD);
        }
      }
    }

    // second index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexCleanUpFromMemoryNotStoredToFileLoadedToMemoryItemIndex = min(endIndex, this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Increase(indexCleanUpFromMemoryNotStoredToFileLoadedToMemoryItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_NOT_STORED_TO_FILE_LOADED_TO_MEMORY_INC);

        if ((!item->IsNoCleanUpFromMemory()) && (!item->IsStoredToFile()) && (item->IsLoadedToMemory()))
        {
          result &= this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Insert(indexCleanUpFromMemoryNotStoredToFileLoadedToMemoryItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_NOT_STORED_TO_FILE_LOADED_TO_MEMORY_ADD);
        }
      }
    }
  }

  if (!result)
  {
    // revert first index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_STORED_TO_FILE_LOADED_TO_MEMORY_ADD), this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Remove(indexCleanUpFromMemoryStoredToFileLoadedToMemoryItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_STORED_TO_FILE_LOADED_TO_MEMORY_INC), this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Decrease(indexCleanUpFromMemoryStoredToFileLoadedToMemoryItemIndex, 1));

    // revert seconds index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_NOT_STORED_TO_FILE_LOADED_TO_MEMORY_ADD), this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Remove(indexCleanUpFromMemoryNotStoredToFileLoadedToMemoryItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_CLEAN_UP_FROM_MEMORY_NOT_STORED_TO_FILE_LOADED_TO_MEMORY_INC), this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Decrease(indexCleanUpFromMemoryNotStoredToFileLoadedToMemoryItemIndex, 1));

    // revert base indexes
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BASE), __super::RemoveIndexes(itemIndex, 1));
  }


  return result;
}

void CCacheFileItemCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
{
  __super::RemoveIndexes(startIndex, count);

  unsigned int start = 0;
  unsigned int end = 0;

  unsigned int indexStart = 0;
  unsigned int indexCount = 0;

  // first index
  this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Count());

  this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Decrease(indexStart, count);

  // second index
  this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Count());

  this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Decrease(indexStart, count);
}

bool CCacheFileItemCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  bool result = __super::UpdateIndexes(itemIndex, count);

  if (result)
  {
    CCacheFileItem *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      bool test = (!item->IsNoCleanUpFromMemory()) && (item->IsStoredToFile()) && (item->IsLoadedToMemory());
      unsigned int indexIndex = this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Insert(min(endIndex, this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Remove(indexIndex, count);
      }
    }

    // second index
    if (result)
    {
      bool test = (!item->IsNoCleanUpFromMemory()) && (!item->IsStoredToFile()) && (item->IsLoadedToMemory());
      unsigned int indexIndex = this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Insert(min(endIndex, this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Remove(indexIndex, count);
      }
    }
  }

  return result;
}

bool CCacheFileItemCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  bool result = __super::EnsureEnoughSpaceIndexes(addingCount);

  if (result)
  {
    result &= this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->EnsureEnoughSpace(this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Count() + addingCount);
    result &= this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->EnsureEnoughSpace(this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Count() + addingCount);
  }

  return result;
}

void CCacheFileItemCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexCleanUpFromMemoryStoredToFileLoadedToMemory->Clear();
  this->indexCleanUpFromMemoryNotStoredToFileLoadedToMemory->Clear();
}

/* protected methods */
