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
#define AFFECTED_INDEX_DOWNLOADED_NOT_PROCESSED_ADD                   (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_DOWNLOADED_NOT_PROCESSED_INC                   (1 << (FLAGS_LAST + 2))

CMshsStreamFragmentCollection::CMshsStreamFragmentCollection(HRESULT *result)
  : CStreamFragmentCollection(result)
{
  this->indexDownloadedNotProcessed = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexDownloadedNotProcessed = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexDownloadedNotProcessed, *result, E_OUTOFMEMORY);
  }
}

CMshsStreamFragmentCollection::~CMshsStreamFragmentCollection(void)
{
  FREE_MEM_CLASS(this->indexDownloadedNotProcessed);
}

/* get methods */

CMshsStreamFragment *CMshsStreamFragmentCollection::GetItem(unsigned int index)
{
  return (CMshsStreamFragment *)__super::GetItem(index);
}

HRESULT CMshsStreamFragmentCollection::GetReadyForProcessingStreamFragments(CIndexedMshsStreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexDownloadedNotProcessed->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexDownloadedNotProcessed->Count())); i++)
  {
    unsigned int index = this->indexDownloadedNotProcessed->GetItem(i);

    CIndexedMshsStreamFragment *item = new CIndexedMshsStreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

/* set methods */

/* other methods */

bool CMshsStreamFragmentCollection::HasReadyForProcessingStreamFragments(void)
{
  return (this->indexDownloadedNotProcessed->Count() != 0);
}

/* index methods */

bool CMshsStreamFragmentCollection::InsertIndexes(unsigned int itemIndex)
{
  uint32_t flags = AFFECTED_INDEX_NONE;
  bool result = __super::InsertIndexes(itemIndex);

  unsigned int indexDownloadedNotProcessedItemIndex = UINT_MAX;

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

      result &= this->indexDownloadedNotProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexDownloadedNotProcessedItemIndex = min(endIndex, this->indexDownloadedNotProcessed->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexDownloadedNotProcessed->Increase(indexDownloadedNotProcessedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_DOWNLOADED_NOT_PROCESSED_INC);

        if (item->IsDownloaded() && (!item->IsProcessed()))
        {
          result &= this->indexDownloadedNotProcessed->Insert(indexDownloadedNotProcessedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_DOWNLOADED_NOT_PROCESSED_ADD);
        }
      }
    }
  }

  if (!result)
  {
    // revert first index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_DOWNLOADED_NOT_PROCESSED_ADD), this->indexDownloadedNotProcessed->Remove(indexDownloadedNotProcessedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_DOWNLOADED_NOT_PROCESSED_INC), this->indexDownloadedNotProcessed->Decrease(indexDownloadedNotProcessedItemIndex, 1));

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
  this->indexDownloadedNotProcessed->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexDownloadedNotProcessed->Count());

  this->indexDownloadedNotProcessed->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexDownloadedNotProcessed->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexDownloadedNotProcessed->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexDownloadedNotProcessed->Decrease(indexStart, count);
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
      bool test = item->IsDownloaded() && (!item->IsProcessed());
      unsigned int indexIndex = this->indexDownloadedNotProcessed->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexDownloadedNotProcessed->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexDownloadedNotProcessed->Insert(min(endIndex, this->indexDownloadedNotProcessed->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexDownloadedNotProcessed->Remove(indexIndex, count);
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
    result &= this->indexDownloadedNotProcessed->EnsureEnoughSpace(this->indexDownloadedNotProcessed->Count() + addingCount);
  }

  return result;
}

void CMshsStreamFragmentCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexDownloadedNotProcessed->Clear();
}

/* protected methods */