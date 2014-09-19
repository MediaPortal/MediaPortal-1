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

#include "Mpeg2tsStreamFragmentCollection.h"

#define AFFECTED_INDEX_NONE                                                                 FLAGS_NONE

#define AFFECTED_INDEX_BASE                                                                 (1 << (FLAGS_LAST + 0))
#define AFFECTED_INDEX_READY_FOR_ALIGN_ADD                                                  (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_READY_FOR_ALIGN_INC                                                  (1 << (FLAGS_LAST + 2))
#define AFFECTED_INDEX_ALIGNED_NOT_PARTIALLY_OR_FULL_PROCESSED_ADD                          (1 << (FLAGS_LAST + 3))
#define AFFECTED_INDEX_ALIGNED_NOT_PARTIALLY_OR_FULL_PROCESSED_INC                          (1 << (FLAGS_LAST + 4))
#define AFFECTED_INDEX_PARTIALLY_PROCESSED_ADD                                              (1 << (FLAGS_LAST + 5))
#define AFFECTED_INDEX_PARTIALLY_PROCESSED_INC                                              (1 << (FLAGS_LAST + 6))

CMpeg2tsStreamFragmentCollection::CMpeg2tsStreamFragmentCollection(HRESULT *result)
  : CStreamFragmentCollection(result)
{
  this->indexReadyForAlign = NULL;
  this->indexAlignedNotPartiallyOrFullProcessed = NULL;
  this->indexPartiallyProcessed = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexReadyForAlign = new CIndexCollection(result);
    this->indexAlignedNotPartiallyOrFullProcessed = new CIndexCollection(result);
    this->indexPartiallyProcessed = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexReadyForAlign, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexAlignedNotPartiallyOrFullProcessed, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexPartiallyProcessed, *result, E_OUTOFMEMORY);
  }
}

CMpeg2tsStreamFragmentCollection::~CMpeg2tsStreamFragmentCollection(void)
{
  FREE_MEM_CLASS(this->indexReadyForAlign);
  FREE_MEM_CLASS(this->indexAlignedNotPartiallyOrFullProcessed);
  FREE_MEM_CLASS(this->indexPartiallyProcessed);
}

/* get methods */

CMpeg2tsStreamFragment *CMpeg2tsStreamFragmentCollection::GetItem(unsigned int index)
{
  return (CMpeg2tsStreamFragment *)__super::GetItem(index);
}

HRESULT CMpeg2tsStreamFragmentCollection::GetReadyForAlignStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexReadyForAlign->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexReadyForAlign->Count())); i++)
  {
    unsigned int index = this->indexReadyForAlign->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetAlignedNotPartiallyOrFullProcessedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexAlignedNotPartiallyOrFullProcessed->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexAlignedNotPartiallyOrFullProcessed->Count())); i++)
  {
    unsigned int index = this->indexAlignedNotPartiallyOrFullProcessed->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetPartiallyProcessedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexReadyForAlign->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexPartiallyProcessed->Count())); i++)
  {
    unsigned int index = this->indexPartiallyProcessed->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

/* set methods */

/* other methods */

bool CMpeg2tsStreamFragmentCollection::HasReadyForAlignStreamFragments(void)
{
  return (this->indexReadyForAlign->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasAlignedNotPartiallyOrFullProcessed(void)
{
  return (this->indexAlignedNotPartiallyOrFullProcessed->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasPartiallyProcessed(void)
{
  return (this->indexPartiallyProcessed->Count() != 0);
}

/* index methods */

bool CMpeg2tsStreamFragmentCollection::InsertIndexes(unsigned int itemIndex)
{
  uint32_t flags = AFFECTED_INDEX_NONE;
  bool result = __super::InsertIndexes(itemIndex);

  unsigned int indexReadyForAlignItemIndex = UINT_MAX;
  unsigned int indexAlignedNotPartiallyOrFullProcessedItemIndex = UINT_MAX;
  unsigned int indexPartiallyProcessedItemIndex = UINT_MAX;

  if (result)
  {
    flags |= AFFECTED_INDEX_BASE;

    // we must check if some index needs to be updated
    // in case of error we must revert all updated indexes
    CMpeg2tsStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexReadyForAlign->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexReadyForAlignItemIndex = min(endIndex, this->indexReadyForAlign->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexReadyForAlign->Increase(indexReadyForAlignItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_READY_FOR_ALIGN_INC);

        if (item->IsReadyForAlign())
        {
          result &= this->indexReadyForAlign->Insert(indexReadyForAlignItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_READY_FOR_ALIGN_ADD);
        }
      }
    }

    // second index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexAlignedNotPartiallyOrFullProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexAlignedNotPartiallyOrFullProcessedItemIndex = min(endIndex, this->indexAlignedNotPartiallyOrFullProcessed->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexAlignedNotPartiallyOrFullProcessed->Increase(indexAlignedNotPartiallyOrFullProcessedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_NOT_PARTIALLY_OR_FULL_PROCESSED_INC);

        if (item->IsAligned() && (!(item->IsPartiallyProcessed() || item->IsProcessed())))
        {
          result &= this->indexAlignedNotPartiallyOrFullProcessed->Insert(indexAlignedNotPartiallyOrFullProcessedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_NOT_PARTIALLY_OR_FULL_PROCESSED_ADD);
        }
      }
    }

    // third index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexPartiallyProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexPartiallyProcessedItemIndex = min(endIndex, this->indexPartiallyProcessed->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexPartiallyProcessed->Increase(indexPartiallyProcessedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_PARTIALLY_PROCESSED_INC);

        if (item->IsPartiallyProcessed())
        {
          result &= this->indexPartiallyProcessed->Insert(indexPartiallyProcessedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_PARTIALLY_PROCESSED_ADD);
        }
      }
    }
  }

  if (!result)
  {
    // revert first index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_READY_FOR_ALIGN_ADD), this->indexReadyForAlign->Remove(indexReadyForAlignItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_READY_FOR_ALIGN_INC), this->indexReadyForAlign->Decrease(indexReadyForAlignItemIndex, 1));

    // revert second index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_NOT_PARTIALLY_OR_FULL_PROCESSED_ADD), this->indexAlignedNotPartiallyOrFullProcessed->Remove(indexAlignedNotPartiallyOrFullProcessedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_NOT_PARTIALLY_OR_FULL_PROCESSED_INC), this->indexAlignedNotPartiallyOrFullProcessed->Decrease(indexAlignedNotPartiallyOrFullProcessedItemIndex, 1));

    // revert third index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_PARTIALLY_PROCESSED_ADD), this->indexPartiallyProcessed->Remove(indexPartiallyProcessedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_PARTIALLY_PROCESSED_INC), this->indexPartiallyProcessed->Decrease(indexPartiallyProcessedItemIndex, 1));

    // revert base indexes
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BASE), __super::RemoveIndexes(itemIndex, 1));
  }


  return result;
}

void CMpeg2tsStreamFragmentCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
{
  __super::RemoveIndexes(startIndex, count);

  unsigned int start = 0;
  unsigned int end = 0;

  unsigned int indexStart = 0;
  unsigned int indexCount = 0;

  // first index
  this->indexReadyForAlign->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexReadyForAlign->Count());

  this->indexReadyForAlign->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexReadyForAlign->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexReadyForAlign->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexReadyForAlign->Decrease(indexStart, count);

  // second index
  this->indexAlignedNotPartiallyOrFullProcessed->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexAlignedNotPartiallyOrFullProcessed->Count());

  this->indexAlignedNotPartiallyOrFullProcessed->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexAlignedNotPartiallyOrFullProcessed->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexAlignedNotPartiallyOrFullProcessed->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexAlignedNotPartiallyOrFullProcessed->Decrease(indexStart, count);

  // this index
  this->indexPartiallyProcessed->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexPartiallyProcessed->Count());

  this->indexPartiallyProcessed->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexPartiallyProcessed->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexPartiallyProcessed->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexPartiallyProcessed->Decrease(indexStart, count);
}

bool CMpeg2tsStreamFragmentCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  bool result = __super::UpdateIndexes(itemIndex, count);

  if (result)
  {
    CMpeg2tsStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      bool test = item->IsReadyForAlign();
      unsigned int indexIndex = this->indexReadyForAlign->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexReadyForAlign->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexReadyForAlign->Insert(min(endIndex, this->indexReadyForAlign->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexReadyForAlign->Remove(indexIndex, count);
      }
    }

    // second index
    if (result)
    {
      bool test = item->IsAligned() && (!(item->IsPartiallyProcessed() || item->IsProcessed()));
      unsigned int indexIndex = this->indexAlignedNotPartiallyOrFullProcessed->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexAlignedNotPartiallyOrFullProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexAlignedNotPartiallyOrFullProcessed->Insert(min(endIndex, this->indexAlignedNotPartiallyOrFullProcessed->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexAlignedNotPartiallyOrFullProcessed->Remove(indexIndex, count);
      }
    }

    // this index
    if (result)
    {
      bool test = item->IsPartiallyProcessed();
      unsigned int indexIndex = this->indexPartiallyProcessed->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexPartiallyProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexPartiallyProcessed->Insert(min(endIndex, this->indexPartiallyProcessed->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexPartiallyProcessed->Remove(indexIndex, count);
      }
    }
  }

  return result;
}

bool CMpeg2tsStreamFragmentCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  bool result = __super::EnsureEnoughSpaceIndexes(addingCount);

  if (result)
  {
    result &= this->indexReadyForAlign->EnsureEnoughSpace(this->indexReadyForAlign->Count() + addingCount);
    result &= this->indexAlignedNotPartiallyOrFullProcessed->EnsureEnoughSpace(this->indexAlignedNotPartiallyOrFullProcessed->Count() + addingCount);
    result &= this->indexPartiallyProcessed->EnsureEnoughSpace(this->indexPartiallyProcessed->Count() + addingCount);
  }

  return result;
}

void CMpeg2tsStreamFragmentCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexReadyForAlign->Clear();
  this->indexAlignedNotPartiallyOrFullProcessed->Clear();
  this->indexPartiallyProcessed->Clear();
}


/* protected methods */
