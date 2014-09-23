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
#define AFFECTED_INDEX_ALIGNED_NOT_DISCONTINUITY_PROCESSED_ADD                              (1 << (FLAGS_LAST + 3))
#define AFFECTED_INDEX_ALIGNED_NOT_DISCONTINUITY_PROCESSED_INC                              (1 << (FLAGS_LAST + 4))
#define AFFECTED_INDEX_ALIGNED_DISCONTINUITY_PROCESSED_NOT_PARTIALLY_OR_FULL_PROCESSED_ADD  (1 << (FLAGS_LAST + 5))
#define AFFECTED_INDEX_ALIGNED_DISCONTINUITY_PROCESSED_NOT_PARTIALLY_OR_FULL_PROCESSED_INC  (1 << (FLAGS_LAST + 6))
#define AFFECTED_INDEX_PARTIALLY_PROCESSED_ADD                                              (1 << (FLAGS_LAST + 7))
#define AFFECTED_INDEX_PARTIALLY_PROCESSED_INC                                              (1 << (FLAGS_LAST + 8))

CMpeg2tsStreamFragmentCollection::CMpeg2tsStreamFragmentCollection(HRESULT *result)
  : CStreamFragmentCollection(result)
{
  this->indexReadyForAlign = NULL;
  this->indexAlignedNotDiscontinuityProcessed = NULL;
  this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed = NULL;
  this->indexPartiallyProcessed = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexReadyForAlign = new CIndexCollection(result);
    this->indexAlignedNotDiscontinuityProcessed = new CIndexCollection(result);
    this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed = new CIndexCollection(result);
    this->indexPartiallyProcessed = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexReadyForAlign, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexAlignedNotDiscontinuityProcessed, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexPartiallyProcessed, *result, E_OUTOFMEMORY);
  }
}

CMpeg2tsStreamFragmentCollection::~CMpeg2tsStreamFragmentCollection(void)
{
  FREE_MEM_CLASS(this->indexReadyForAlign);
  FREE_MEM_CLASS(this->indexAlignedNotDiscontinuityProcessed);
  FREE_MEM_CLASS(this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed);
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

HRESULT CMpeg2tsStreamFragmentCollection::GetAlignedNotDiscontinuityProcessedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexAlignedNotDiscontinuityProcessed->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexAlignedNotDiscontinuityProcessed->Count())); i++)
  {
    unsigned int index = this->indexAlignedNotDiscontinuityProcessed->GetItem(i);

    CIndexedMpeg2tsStreamFragment *item = new CIndexedMpeg2tsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CMpeg2tsStreamFragmentCollection::GetAlignedDiscontinuityProcessedNotPartiallyOrFullProcessedStreamFragments(CIndexedMpeg2tsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count())); i++)
  {
    unsigned int index = this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->GetItem(i);

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

bool CMpeg2tsStreamFragmentCollection::HasAlignedNotDiscontinuityProcessed(void)
{
  return (this->indexAlignedNotDiscontinuityProcessed->Count() != 0);
}

bool CMpeg2tsStreamFragmentCollection::HasAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed(void)
{
  return (this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count() != 0);
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
  unsigned int indexAlignedNotDiscontinuityProcessedItemIndex = UINT_MAX;
  unsigned int indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessedItemIndex = UINT_MAX;
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

      result &= this->indexAlignedNotDiscontinuityProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexAlignedNotDiscontinuityProcessedItemIndex = min(endIndex, this->indexAlignedNotDiscontinuityProcessed->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexAlignedNotDiscontinuityProcessed->Increase(indexAlignedNotDiscontinuityProcessedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_NOT_DISCONTINUITY_PROCESSED_INC);

        if (item->IsAligned() && (!item->IsDiscontinuityProcessed()))
        {
          result &= this->indexAlignedNotDiscontinuityProcessed->Insert(indexAlignedNotDiscontinuityProcessedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_NOT_DISCONTINUITY_PROCESSED_ADD);
        }
      }
    }

    // third index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessedItemIndex = min(endIndex, this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Increase(indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_DISCONTINUITY_PROCESSED_NOT_PARTIALLY_OR_FULL_PROCESSED_INC);

        if (item->IsAligned() && item->IsDiscontinuityProcessed() && (!(item->IsPartiallyProcessed() || item->IsProcessed())))
        {
          result &= this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Insert(indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ALIGNED_DISCONTINUITY_PROCESSED_NOT_PARTIALLY_OR_FULL_PROCESSED_ADD);
        }
      }
    }

    // fourth index
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
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_NOT_DISCONTINUITY_PROCESSED_ADD), this->indexAlignedNotDiscontinuityProcessed->Remove(indexAlignedNotDiscontinuityProcessedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_NOT_DISCONTINUITY_PROCESSED_INC), this->indexAlignedNotDiscontinuityProcessed->Decrease(indexAlignedNotDiscontinuityProcessedItemIndex, 1));

    // revert third index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_DISCONTINUITY_PROCESSED_NOT_PARTIALLY_OR_FULL_PROCESSED_ADD), this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Remove(indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ALIGNED_DISCONTINUITY_PROCESSED_NOT_PARTIALLY_OR_FULL_PROCESSED_INC), this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Decrease(indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessedItemIndex, 1));

    // revert fourth index
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
  this->indexAlignedNotDiscontinuityProcessed->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexAlignedNotDiscontinuityProcessed->Count());

  this->indexAlignedNotDiscontinuityProcessed->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexAlignedNotDiscontinuityProcessed->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexAlignedNotDiscontinuityProcessed->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexAlignedNotDiscontinuityProcessed->Decrease(indexStart, count);

  // third index
  this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count());

  this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Decrease(indexStart, count);

  // fourth index
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
      bool test = item->IsAligned() && (!item->IsDiscontinuityProcessed());
      unsigned int indexIndex = this->indexAlignedNotDiscontinuityProcessed->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexAlignedNotDiscontinuityProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexAlignedNotDiscontinuityProcessed->Insert(min(endIndex, this->indexAlignedNotDiscontinuityProcessed->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexAlignedNotDiscontinuityProcessed->Remove(indexIndex, count);
      }
    }

    // third index
    if (result)
    {
      bool test = item->IsAligned() && item->IsDiscontinuityProcessed() && (!(item->IsPartiallyProcessed() || item->IsProcessed()));
      unsigned int indexIndex = this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Insert(min(endIndex, this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Remove(indexIndex, count);
      }
    }

    // fourth index
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
    result &= this->indexAlignedNotDiscontinuityProcessed->EnsureEnoughSpace(this->indexAlignedNotDiscontinuityProcessed->Count() + addingCount);
    result &= this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->EnsureEnoughSpace(this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Count() + addingCount);
    result &= this->indexPartiallyProcessed->EnsureEnoughSpace(this->indexPartiallyProcessed->Count() + addingCount);
  }

  return result;
}

void CMpeg2tsStreamFragmentCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexReadyForAlign->Clear();
  this->indexAlignedNotDiscontinuityProcessed->Clear();
  this->indexAlignedDiscontinuityProcessedNotPartiallyOrFullProcessed->Clear();
  this->indexPartiallyProcessed->Clear();
}


/* protected methods */
