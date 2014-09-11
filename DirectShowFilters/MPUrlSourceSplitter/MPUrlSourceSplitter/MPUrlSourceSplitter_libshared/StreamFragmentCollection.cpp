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

#include "StreamFragmentCollection.h"

#define AFFECTED_INDEX_NONE                                                                 FLAGS_NONE

#define AFFECTED_INDEX_BASE                                                                 (1 << (FLAGS_LAST + 0))
#define AFFECTED_INDEX_NOT_DOWNLOADED_ADD                                                   (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_NOT_DOWNLOADED_INC                                                   (1 << (FLAGS_LAST + 2))


CStreamFragmentCollection::CStreamFragmentCollection(HRESULT *result)
  : CCacheFileItemCollection(result)
{
  this->startSearchingIndex = 0;
  this->searchCount = 0;
  this->indexNotDownloaded = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexNotDownloaded = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexNotDownloaded, *result, E_OUTOFMEMORY);
  }
}

CStreamFragmentCollection::~CStreamFragmentCollection(void)
{
  FREE_MEM_CLASS(this->indexNotDownloaded);
}

/* get methods */

CStreamFragment *CStreamFragmentCollection::GetItem(unsigned int index)
{
  return (CStreamFragment *)__super::GetItem(index);
}

HRESULT CStreamFragmentCollection::GetNotDownloadedStreamFragments(CIndexedStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexNotDownloaded->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexNotDownloaded->Count())); i++)
  {
    unsigned int index = this->indexNotDownloaded->GetItem(i);

    CIndexedStreamFragment *item = new CIndexedStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

unsigned int CStreamFragmentCollection::GetFirstNotDownloadedStreamFragmentIndex(unsigned int itemIndex)
{
  unsigned int result = UINT_MAX;

  // get position to insert item index
  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->indexNotDownloaded->GetItemInsertPosition(itemIndex, &startIndex, &endIndex))
  {
    result = this->indexNotDownloaded->GetItem(endIndex);
  }

  return result;
}

unsigned int CStreamFragmentCollection::GetStreamFragmentIndexBetweenPositions(int64_t position)
{
  unsigned int index = UINT_MAX;

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  CStreamFragment *zeroPositionFragment = this->GetItem(this->startSearchingIndex);

  if (zeroPositionFragment != NULL)
  {
    position += zeroPositionFragment->GetFragmentStartPosition();

    if (this->GetStreamFragmentInsertPosition(position, &startIndex, &endIndex))
    {
      if (startIndex != UINT_MAX)
      {
        // if requested position is somewhere after start of stream fragments
        CStreamFragment *streamFragment = this->GetItem(startIndex);
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

bool CStreamFragmentCollection::GetStreamFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex)
{
  bool result = ((startIndex != NULL) && (endIndex != NULL));

  if (result)
  {
    result = ((this->startSearchingIndex != STREAM_FRAGMENT_INDEX_NOT_SET) && (this->searchCount > 0));

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
        CStreamFragment *streamFragment = this->GetItem(middle);

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

unsigned int CStreamFragmentCollection::GetStartSearchingIndex(void)
{
  return this->startSearchingIndex;
}

unsigned int CStreamFragmentCollection::GetSearchCount(void)
{
  return this->searchCount;
}

unsigned int CStreamFragmentCollection::GetFirstNotDownloadedStreamFragmentIndexAfterStartPosition(int64_t position)
{
  unsigned int result = UINT_MAX;

  // get position to insert item index
  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  if (this->GetStreamFragmentInsertPosition(position, &startIndex, &endIndex))
  {
    if (this->indexNotDownloaded->GetItemInsertPosition(endIndex, &startIndex, &endIndex))
    {
      result = this->indexNotDownloaded->GetItem(endIndex);
    }
  }

  return result;
}

/* set methods */

void CStreamFragmentCollection::SetStartSearchingIndex(unsigned int startSearchingIndex)
{
  this->startSearchingIndex = startSearchingIndex;
}

void CStreamFragmentCollection::SetSearchCount(unsigned int searchCount)
{
  this->searchCount = searchCount;
}

/* other methods */

bool CStreamFragmentCollection::Add(CFastSearchItem *item)
{
  bool result = (item != NULL);

  CHECK_CONDITION_EXECUTE(result, result &= this->EnsureEnoughSpace(this->Count() + 1));

  if (result)
  {
    CStreamFragment *fragment = dynamic_cast<CStreamFragment *>(item);
    result &= (fragment != NULL);

    if (result)
    {
      if (fragment->GetFragmentStartPosition() == STREAM_FRAGMENT_START_POSITION_NOT_SET)
      {
        result &= __super::Insert(this->Count(), item);
      }
      else
      {
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        if (this->GetStreamFragmentInsertPosition(fragment->GetFragmentStartPosition(), &startIndex, &endIndex))
        {
          // check if media packet exists in collection
          result &= (startIndex != endIndex);

          if (result)
          {
            result &= __super::Insert(min(endIndex, this->Count()), item);
          }
        }
      }
    }
  }

  return result;
}

bool CStreamFragmentCollection::Insert(unsigned int position, CFastSearchItem *item)
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

void CStreamFragmentCollection::RecalculateProcessedStreamFragmentStartPosition(unsigned int startIndex)
{
  for (unsigned int i = startIndex; i < this->Count(); i++)
  {
    CStreamFragment *fragment = this->GetItem(i);
    CStreamFragment *previousFragment = (i > 0) ? this->GetItem(i - 1) : NULL;

    if (fragment->IsDownloaded())
    {
      if ((previousFragment != NULL) && (previousFragment->IsProcessed()))
      {
        fragment->SetFragmentStartPosition(previousFragment->GetFragmentStartPosition() + previousFragment->GetLength());
      }

      if (i == (this->GetStartSearchingIndex() + this->GetSearchCount()))
      {
        this->SetSearchCount(this->GetSearchCount() + 1);
      }
    }
    else
    {
      // we found not downloaded stream fragment, stop recalculating start positions
      break;
    }
  }
}

/* index methods */

bool CStreamFragmentCollection::InsertIndexes(unsigned int itemIndex)
{
  uint32_t flags = AFFECTED_INDEX_NONE;
  bool result = __super::InsertIndexes(itemIndex);

  unsigned int indexNotDownloadedItemIndex = UINT_MAX;

  if (result)
  {
    flags |= AFFECTED_INDEX_BASE;

    // we must check if some index needs to be updated
    // in case of error we must revert all updated indexes
    CStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexNotDownloaded->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexNotDownloadedItemIndex = min(endIndex, this->indexNotDownloaded->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexNotDownloaded->Increase(indexNotDownloadedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_NOT_DOWNLOADED_INC);

        if (!item->IsDownloaded())
        {
          result &= this->indexNotDownloaded->Insert(indexNotDownloadedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_NOT_DOWNLOADED_ADD);
        }
      }
    }
  }

  if (!result)
  {
    // revert first index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_NOT_DOWNLOADED_ADD), this->indexNotDownloaded->Remove(indexNotDownloadedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_NOT_DOWNLOADED_INC), this->indexNotDownloaded->Decrease(indexNotDownloadedItemIndex, 1));

    // revert base indexes
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BASE), __super::RemoveIndexes(itemIndex, 1));
  }


  return result;
}

void CStreamFragmentCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
{
  __super::RemoveIndexes(startIndex, count);

  unsigned int start = 0;
  unsigned int end = 0;

  unsigned int indexStart = 0;
  unsigned int indexCount = 0;

  // first index
  this->indexNotDownloaded->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexNotDownloaded->Count());

  this->indexNotDownloaded->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexNotDownloaded->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexNotDownloaded->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexNotDownloaded->Decrease(indexStart, count);
}

bool CStreamFragmentCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  bool result = __super::UpdateIndexes(itemIndex, count);

  if (result)
  {
    CStreamFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      bool test = (!item->IsDownloaded());
      unsigned int indexIndex = this->indexNotDownloaded->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexNotDownloaded->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexNotDownloaded->Insert(min(endIndex, this->indexNotDownloaded->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexNotDownloaded->Remove(indexIndex, count);
      }
    }
  }

  return result;
}

bool CStreamFragmentCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  bool result = __super::EnsureEnoughSpaceIndexes(addingCount);

  if (result)
  {
    result &= this->indexNotDownloaded->EnsureEnoughSpace(this->indexNotDownloaded->Count() + addingCount);
  }

  return result;
}

void CStreamFragmentCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexNotDownloaded->Clear();
}

/* protected methods */
