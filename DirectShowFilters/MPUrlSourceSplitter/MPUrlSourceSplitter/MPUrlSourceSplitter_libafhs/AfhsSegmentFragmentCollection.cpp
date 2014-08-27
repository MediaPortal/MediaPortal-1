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

#include "AfhsSegmentFragmentCollection.h"

#define AFFECTED_INDEX_NONE                                                                 FLAGS_NONE

#define AFFECTED_INDEX_BASE                                                                 (1 << (FLAGS_LAST + 0))
#define AFFECTED_INDEX_ENCRYPTED_ADD                                                        (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_ENCRYPTED_INC                                                        (1 << (FLAGS_LAST + 2))
#define AFFECTED_INDEX_DECRYPTED_ADD                                                        (1 << (FLAGS_LAST + 3))
#define AFFECTED_INDEX_DECRYPTED_INC                                                        (1 << (FLAGS_LAST + 4))

CAfhsSegmentFragmentCollection::CAfhsSegmentFragmentCollection(HRESULT *result)
  : CCacheFileItemCollection(result)
{
  this->startSearchingIndex = 0;
  this->searchCount = 0;

  this->defaultBaseUrl = NULL;
  this->segmentFragmentUrlExtraParameters = NULL;
  this->indexEncrypted = NULL;
  this->indexDecrypted = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->indexEncrypted = new CIndexCollection(result);
    this->indexDecrypted = new CIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->indexEncrypted, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->indexDecrypted, *result, E_OUTOFMEMORY);
  }
}

CAfhsSegmentFragmentCollection::~CAfhsSegmentFragmentCollection(void)
{
  FREE_MEM(this->defaultBaseUrl);
  FREE_MEM(this->segmentFragmentUrlExtraParameters);
  FREE_MEM_CLASS(this->indexEncrypted);
  FREE_MEM_CLASS(this->indexDecrypted);
}

/* get methods */

CAfhsSegmentFragment *CAfhsSegmentFragmentCollection::GetItem(unsigned int index)
{
  return (CAfhsSegmentFragment *)__super::GetItem(index);
}

unsigned int CAfhsSegmentFragmentCollection::GetSegmentFragmentIndexBetweenPositions(int64_t position)
{
  unsigned int index = UINT_MAX;

  unsigned int startIndex = 0;
  unsigned int endIndex = 0;

  CAfhsSegmentFragment *zeroPositionFragment = this->GetItem(this->startSearchingIndex);

  if (zeroPositionFragment != NULL)
  {
    position += zeroPositionFragment->GetFragmentStartPosition();

    if (this->GetSegmentFragmentInsertPosition(position, &startIndex, &endIndex))
    {
      if (startIndex != UINT_MAX)
      {
        // if requested position is somewhere after start of segment fragments
        CAfhsSegmentFragment *segmentFragment = this->GetItem(startIndex);
        int64_t positionStart = segmentFragment->GetFragmentStartPosition();
        int64_t positionEnd = positionStart + segmentFragment->GetLength();

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

bool CAfhsSegmentFragmentCollection::GetSegmentFragmentInsertPosition(int64_t position, unsigned int *startIndex, unsigned int *endIndex)
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

        // get segment fragment at middle index
        CAfhsSegmentFragment *segmentFragment = this->GetItem(middle);

        // compare segment fragment start to position
        if (position > segmentFragment->GetFragmentStartPosition())
        {
          // position is bigger than segment fragment start time
          // search in top half
          first = middle + 1;
        }
        else if (position < segmentFragment->GetFragmentStartPosition()) 
        {
          // position is lower than segment fragment start time
          // search in bottom half
          last = middle - 1;
        }
        else
        {
          // we found segment fragment with same starting time as position
          *startIndex = middle;
          *endIndex = middle;
          result = true;
          break;
        }
      }

      if (!result)
      {
        // we don't found segment fragment
        // it means that segment fragment with 'position' belongs between first and last
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

unsigned int CAfhsSegmentFragmentCollection::GetStartSearchingIndex(void)
{
  return this->startSearchingIndex;
}

unsigned int CAfhsSegmentFragmentCollection::GetSearchCount(void)
{
  return this->searchCount;
}

wchar_t *CAfhsSegmentFragmentCollection::GetSegmentFragmentUrl(CAfhsSegmentFragment *segmentFragment)
{
  return FormatString(L"%sSeg%d-Frag%d%s", this->defaultBaseUrl, segmentFragment->GetSegment(), segmentFragment->GetFragment(), (this->segmentFragmentUrlExtraParameters == NULL) ? L"" : this->segmentFragmentUrlExtraParameters);
}

const wchar_t *CAfhsSegmentFragmentCollection::GetBaseUrl(void)
{
  return this->defaultBaseUrl;
}

const wchar_t *CAfhsSegmentFragmentCollection::GetSegmentFragmentUrlExtraParameters(void)
{
  return this->segmentFragmentUrlExtraParameters;
}

HRESULT CAfhsSegmentFragmentCollection::GetEncryptedStreamFragments(CIndexedAfhsSegmentFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexEncrypted->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexEncrypted->Count())); i++)
  {
    unsigned int index = this->indexEncrypted->GetItem(i);

    CIndexedAfhsSegmentFragment *item = new CIndexedAfhsSegmentFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CAfhsSegmentFragmentCollection::GetDecryptedStreamFragments(CIndexedAfhsSegmentFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexDecrypted->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexDecrypted->Count())); i++)
  {
    unsigned int index = this->indexDecrypted->GetItem(i);

    CIndexedAfhsSegmentFragment *item = new CIndexedAfhsSegmentFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

/* set methods */

void CAfhsSegmentFragmentCollection::SetStartSearchingIndex(unsigned int startSearchingIndex)
{
  this->startSearchingIndex = startSearchingIndex;
}

void CAfhsSegmentFragmentCollection::SetSearchCount(unsigned int searchCount)
{
  this->searchCount = searchCount;
}

bool CAfhsSegmentFragmentCollection::SetBaseUrl(const wchar_t *baseUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->defaultBaseUrl, baseUrl);
}

bool CAfhsSegmentFragmentCollection::SetSegmentFragmentUrlExtraParameters(const wchar_t *segmentFragmentUrlExtraParameters)
{
  SET_STRING_RETURN_WITH_NULL(this->segmentFragmentUrlExtraParameters, segmentFragmentUrlExtraParameters);
}

/* other methods */

bool CAfhsSegmentFragmentCollection::Insert(unsigned int position, CCacheFileItem *item)
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

bool CAfhsSegmentFragmentCollection::HasEncryptedSegmentFragments(void)
{
  return (this->indexEncrypted->Count() != 0);
}

bool CAfhsSegmentFragmentCollection::HasDecryptedSegmentFragments(void)
{
  return (this->indexDecrypted->Count() != 0);
}

/* index methods */

bool CAfhsSegmentFragmentCollection::InsertIndexes(unsigned int itemIndex)
{
  uint32_t flags = AFFECTED_INDEX_NONE;
  bool result = __super::InsertIndexes(itemIndex);

  unsigned int indexEncryptedItemIndex = UINT_MAX;
  unsigned int indexDecryptedItemIndex = UINT_MAX;

  if (result)
  {
    flags |= AFFECTED_INDEX_BASE;

    // we must check if some index needs to be updated
    // in case of error we must revert all updated indexes
    CAfhsSegmentFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexEncrypted->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexEncryptedItemIndex = min(endIndex, this->indexEncrypted->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexEncrypted->Increase(indexEncryptedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ENCRYPTED_INC);

        if (item->IsEncrypted())
        {
          result &= this->indexEncrypted->Insert(indexEncryptedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_ENCRYPTED_ADD);
        }
      }
    }

    // second index
    if (result)
    {
      // get position to insert item index
      unsigned int startIndex = 0;
      unsigned int endIndex = 0;

      result &= this->indexDecrypted->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

      if (result)
      {
        indexDecryptedItemIndex = min(endIndex, this->indexDecrypted->Count());

        // update (increase) values in index
        CHECK_CONDITION_EXECUTE(result, this->indexDecrypted->Increase(indexDecryptedItemIndex));
        CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_DECRYPTED_INC);

        if (item->IsDecrypted())
        {
          result &= this->indexDecrypted->Insert(indexDecryptedItemIndex, itemIndex);
          CHECK_CONDITION_EXECUTE(result, flags |= AFFECTED_INDEX_DECRYPTED_ADD);
        }
      }
    }
  }

  if (!result)
  {
    // revert first index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ENCRYPTED_ADD), this->indexEncrypted->Remove(indexEncryptedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_ENCRYPTED_INC), this->indexEncrypted->Decrease(indexEncryptedItemIndex, 1));

    // revert seconds index
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_DECRYPTED_ADD), this->indexDecrypted->Remove(indexDecryptedItemIndex));
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_DECRYPTED_INC), this->indexDecrypted->Decrease(indexDecryptedItemIndex, 1));

    // revert base indexes
    CHECK_CONDITION_EXECUTE(CFlags::IsSetFlags(flags, AFFECTED_INDEX_BASE), __super::RemoveIndexes(itemIndex, 1));
  }


  return result;
}

void CAfhsSegmentFragmentCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
{
  __super::RemoveIndexes(startIndex, count);

  unsigned int start = 0;
  unsigned int end = 0;

  unsigned int indexStart = 0;
  unsigned int indexCount = 0;

  // first index
  this->indexEncrypted->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexEncrypted->Count());

  this->indexEncrypted->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexEncrypted->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexEncrypted->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexEncrypted->Decrease(indexStart, count);

  // second index
  this->indexDecrypted->GetItemInsertPosition(startIndex, &start, &end);
  indexStart = min(end, this->indexDecrypted->Count());

  this->indexDecrypted->GetItemInsertPosition(startIndex + count, &start, &end);
  indexCount = min(end, this->indexDecrypted->Count()) - indexStart;

  CHECK_CONDITION_EXECUTE(indexCount > 0, this->indexDecrypted->Remove(indexStart, indexCount));
  // update (decrease) values in index
  this->indexDecrypted->Decrease(indexStart, count);
}

bool CAfhsSegmentFragmentCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  bool result = __super::UpdateIndexes(itemIndex, count);

  if (result)
  {
    CAfhsSegmentFragment *item = this->GetItem(itemIndex);

    // first index
    if (result)
    {
      bool test = item->IsEncrypted();
      unsigned int indexIndex = this->indexEncrypted->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexEncrypted->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexEncrypted->Insert(min(endIndex, this->indexEncrypted->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexEncrypted->Remove(indexIndex, count);
      }
    }

    // second index
    if (result)
    {
      bool test = item->IsDecrypted();
      unsigned int indexIndex = this->indexDecrypted->GetItemIndex(itemIndex);

      if (result && test && (indexIndex == UINT_MAX))
      {
        // get position to insert item index
        unsigned int startIndex = 0;
        unsigned int endIndex = 0;

        result &= this->indexDecrypted->GetItemInsertPosition(itemIndex, &startIndex, &endIndex);

        // insert into index
        CHECK_CONDITION_EXECUTE(result, result &= this->indexDecrypted->Insert(min(endIndex, this->indexDecrypted->Count()), itemIndex, count));
      }
      else if (result && (!test) && (indexIndex != UINT_MAX))
      {
        // remove from index
        this->indexDecrypted->Remove(indexIndex, count);
      }
    }
  }

  return result;
}

bool CAfhsSegmentFragmentCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  bool result = __super::EnsureEnoughSpaceIndexes(addingCount);

  if (result)
  {
    result &= this->indexEncrypted->EnsureEnoughSpace(this->indexEncrypted->Count() + addingCount);
    result &= this->indexDecrypted->EnsureEnoughSpace(this->indexDecrypted->Count() + addingCount);
  }

  return result;
}

void CAfhsSegmentFragmentCollection::ClearIndexes(void)
{
  this->indexEncrypted->Clear();
  this->indexDecrypted->Clear();
}

/* protected methods */