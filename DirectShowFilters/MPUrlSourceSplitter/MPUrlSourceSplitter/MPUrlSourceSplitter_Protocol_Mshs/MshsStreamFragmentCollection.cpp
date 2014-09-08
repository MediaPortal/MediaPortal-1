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

#include "MshsStreamFragmentCollection.h"

#define AFFECTED_INDEX_NONE                                           FLAGS_NONE

#define AFFECTED_INDEX_BASE                                           (1 << (FLAGS_LAST + 0))
#define AFFECTED_INDEX_READY_FOR_PROCESSING_ADD                       (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_READY_FOR_PROCESSING_INC                       (1 << (FLAGS_LAST + 2))
#define AFFECTED_INDEX_NOT_READY_FOR_PROCESSING_NOT_DOWNLOADED_ADD    (1 << (FLAGS_LAST + 3))
#define AFFECTED_INDEX_NOT_READY_FOR_PROCESSING_NOT_DOWNLOADED_INC    (1 << (FLAGS_LAST + 4))

CMshsStreamFragmentCollection::CMshsStreamFragmentCollection(HRESULT *result)
  : CCacheFileItemCollection(result)
{
  this->startSearchingIndex = 0;
  this->searchCount = 0;
  this->indexReadyForProcessing = NULL;
  this->indexNotReadyForProcessingNotDownloaded = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexReadyForProcessing = new CIndexCollection(result);
    this->indexNotReadyForProcessingNotDownloaded = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexReadyForProcessing, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexNotReadyForProcessingNotDownloaded, *result, E_OUTOFMEMORY);
  }
}

CMshsStreamFragmentCollection::~CMshsStreamFragmentCollection(void)
{
  FREE_MEM_CLASS(this->indexReadyForProcessing);
  FREE_MEM_CLASS(this->indexNotReadyForProcessingNotDownloaded);
}

/* get methods */

CMshsStreamFragment *CMshsStreamFragmentCollection::GetItem(unsigned int index)
{
  return (CMshsStreamFragment *)__super::GetItem(index);
}

unsigned int CMshsStreamFragmentCollection::GetStreamFragmentIndexBetweenPositions(int64_t position)
{
  unsigned int index = UINT_MAX;

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  CMshsStreamFragment *zeroPositionFragment = this->GetItem(this->startSearchingIndex);

  if (zeroPositionFragment != NULL)
  {
    position += zeroPositionFragment->GetFragmentStartPosition();

    if (this->GetStreamFragmentInsertPosition(position, &startIndex, &endIndex))
    {
      if (startIndex != UINT_MAX)
      {
        // if requested position is somewhere after start of stream fragments
        CMshsStreamFragment *streamFragment = this->GetItem(startIndex);
        int64_t positionStart = streamFragment->GetFragmentStartPosition();
        int64_t positionEnd = positionStart + streamFragment->GetLength();

        if ((position >= positionStart) && (position < positionEnd))
        {
          // we found segment fragment
          index = startIndex;
        }
      }
    }
  }

  return index;
}

bool CMshsStreamFragmentCollection::GetStreamFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = ((this->startSearchingIndex != SEGMENT_FRAGMENT_INDEX_NOT_SET) && (this->searchCount > 0));

    if (result)
    {
      unsigned int first = this->startSearchingIndex;
      unsigned int last = this->startSearchingIndex + this->searchCount - 1;
      result = false;

      while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
      {
        // compute middle index
        unsigned int middle = (first + last) / 2;

        // get stream fragment at middle index
        CMshsStreamFragment *streamFragment = this->GetItem(middle);

        // compare stream fragment start to position
        if (position > streamFragment->GetFragmentStartPosition())
        {
          // position is bigger than stream fragment start time
          // search in top half
          first = middle + 1;
        }
        else if (position < streamFragment->GetFragmentStartPosition()) 
        {
          // position is lower than stream fragment start time
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found stream fragment with same starting time as position
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found stream fragment
        // it means that stream fragment with 'position' belongs between first and last
        *startIndex = last;
        *endIndex = (first >= (this->startSearchingIndex + this->searchCount)) ? UINT_MAX : first;
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

unsigned int CMshsStreamFragmentCollection::GetStartSearchingIndex(void)
{
  return this->startSearchingIndex;
}

unsigned int CMshsStreamFragmentCollection::GetSearchCount(void)
{
  return this->searchCount;
}

HRESULT CMshsStreamFragmentCollection::GetReadyForProcessingStreamFragments(CIndexedMshsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexReadyForProcessing->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexReadyForProcessing->Count())); i++)
  {
    unsigned int index = this->indexReadyForProcessing->GetItem(i);

    CIndexedMshsStreamFragment *item = new CIndexedMshsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

unsigned int CMshsStreamFragmentCollection::GetFirstNotReadyForProcessingNotDownloadedStreamFragmentIndex(unsigned int itemIndex)
{
  unsigned int result = UINT_MAX;

  // get position to insert item index
  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->indexNotReadyForProcessingNotDownloaded->GetItemInsertPosition(itemIndex, &startIndex, &endIndex))
  {
    result = this->indexNotReadyForProcessingNotDownloaded->GetItem(endIndex);
  }

  return result;
}

/* set methods */

void CMshsStreamFragmentCollection::SetStartSearchingIndex(unsigned int startSearchingIndex)
{
  this->startSearchingIndex = startSearchingIndex;
}

void CMshsStreamFragmentCollection::SetSearchCount(unsigned int searchCount)
{
  this->searchCount = searchCount;
}

/* other methods */

bool CMshsStreamFragmentCollection::Insert(unsigned int position, CCacheFileItem *item)
{
  bool result = __super::Insert(position, item);

  if (result)
  {
    if (position <= this->startSearchingIndex)
    {
      this->startSearchingIndex++;
    }
  }

  return result;
}

bool CMshsStreamFragmentCollection::HasReadyForProcessingStreamFragments(void)
{
  return (this->indexReadyForProcessing->Count() != 0);
}

/* index methods */

bool CMshsStreamFragmentCollection::InsertIndexes(unsigned int itemIndex)
{
  uint32_t flags = AFFECTED_INDEX_NONE;
  bool result = __super::InsertIndexes(itemIndex);

  unsigned int indexReadyForProcessingItemIndex = UINT_MAX;
  unsigned int indexNotReadyForProcessingNotDownloadedItemIndex = UINT_MAX;

  if (result)
  {
    flags |= AFFECTED_INDEX_BASE;

    // we must check if some index needs to be updated
    // in case of error we must revert all updated indexes
    CMshsStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexReadyForProcessing->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexReadyForProcessingItemIndex = min(endIndex, this->indexReadyForProcessing->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexReadyForProcessing->Increase(indexReadyForProcessingItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_READY_FOR_PROCESSING_INC);

        if (item->IsReadyForProcessing())
        {
          result &= this->indexReadyForProcessing->Insert(indexReadyForProcessingItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_READY_FOR_PROCESSING_ADD);
        }
      }
    }

    // second index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexNotReadyForProcessingNotDownloaded->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexNotReadyForProcessingNotDownloadedItemIndex = min(endIndex, this->indexNotReadyForProcessingNotDownloaded->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexNotReadyForProcessingNotDownloaded->Increase(indexNotReadyForProcessingNotDownloadedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_NOT_READY_FOR_PROCESSING_NOT_DOWNLOADED_INC);

        if ((!item->IsReadyForProcessing()) && (!item->IsDownloaded()))
        {
          result &= this->indexNotReadyForProcessingNotDownloaded->Insert(indexNotReadyForProcessingNotDownloadedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_NOT_READY_FOR_PROCESSING_NOT_DOWNLOADED_ADD);
        }
      }
    }
  }

  if (!result)
  {
    // revert first index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_READY_FOR_PROCESSING_ADD), this->indexReadyForProcessing->Remove(indexReadyForProcessingItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_READY_FOR_PROCESSING_INC), this->indexReadyForProcessing->Decrease(indexReadyForProcessingItemIndex, 1));

    // revert second index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_NOT_READY_FOR_PROCESSING_NOT_DOWNLOADED_ADD), this->indexNotReadyForProcessingNotDownloaded->Remove(indexNotReadyForProcessingNotDownloadedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_NOT_READY_FOR_PROCESSING_NOT_DOWNLOADED_INC), this->indexNotReadyForProcessingNotDownloaded->Decrease(indexNotReadyForProcessingNotDownloadedItemIndex, 1));

    // revert base indexes
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BASE), __super::RemoveIndexes(itemIndex, 1));
  }

  return result;
}

void CMshsStreamFragmentCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
{
  __super::RemoveIndexes(startIndex, count);

  unsigned int start = 0;
  unsigned int end = 0;

  unsigned int indexStart = 0;
  unsigned int indexCount = 0;

  // first index
  this->indexReadyForProcessing->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexReadyForProcessing->Count());

  this->indexReadyForProcessing->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexReadyForProcessing->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexReadyForProcessing->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexReadyForProcessing->Decrease(indexStart, count);

  // second index
  this->indexNotReadyForProcessingNotDownloaded->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexNotReadyForProcessingNotDownloaded->Count());

  this->indexNotReadyForProcessingNotDownloaded->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexNotReadyForProcessingNotDownloaded->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexNotReadyForProcessingNotDownloaded->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexNotReadyForProcessingNotDownloaded->Decrease(indexStart, count);
}

bool CMshsStreamFragmentCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  bool result = __super::UpdateIndexes(itemIndex, count);

  if (result)
  {
    CMshsStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      bool test = item->IsReadyForProcessing();
      unsigned int indexIndex = this->indexReadyForProcessing->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexReadyForProcessing->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexReadyForProcessing->Insert(min(endIndex, this->indexReadyForProcessing->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexReadyForProcessing->Remove(indexIndex, count);
      }
    }

    // second index
    if (result)
    {
      bool test = (!item->IsReadyForProcessing()) && (!item->IsDownloaded());
      unsigned int indexIndex = this->indexNotReadyForProcessingNotDownloaded->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexNotReadyForProcessingNotDownloaded->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexNotReadyForProcessingNotDownloaded->Insert(min(endIndex, this->indexNotReadyForProcessingNotDownloaded->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexNotReadyForProcessingNotDownloaded->Remove(indexIndex, count);
      }
    }
  }

  return result;
}

bool CMshsStreamFragmentCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  bool result = __super::EnsureEnoughSpaceIndexes(addingCount);

  if (result)
  {
    result &= this->indexReadyForProcessing->EnsureEnoughSpace(this->indexReadyForProcessing->Count() + addingCount);
    result &= this->indexNotReadyForProcessingNotDownloaded->EnsureEnoughSpace(this->indexNotReadyForProcessingNotDownloaded->Count() + addingCount);
  }

  return result;
}

void CMshsStreamFragmentCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexReadyForProcessing->Clear();
  this->indexNotReadyForProcessingNotDownloaded->Clear();
}

/* protected methods */