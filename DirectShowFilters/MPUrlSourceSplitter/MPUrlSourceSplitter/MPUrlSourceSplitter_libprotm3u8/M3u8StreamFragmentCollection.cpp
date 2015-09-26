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

#include "M3u8StreamFragmentCollection.h"

#define AFFECTED_INDEX_NONE                                                                 FLAGS_NONE

#define AFFECTED_INDEX_BASE                                                                 (1 << (FLAGS_LAST + 0))
#define AFFECTED_INDEX_ENCRYPTED_ADD                                                        (1 << (FLAGS_LAST + 1))
#define AFFECTED_INDEX_ENCRYPTED_INC                                                        (1 << (FLAGS_LAST + 2))
#define AFFECTED_INDEX_DECRYPTED_ADD                                                        (1 << (FLAGS_LAST + 3))
#define AFFECTED_INDEX_DECRYPTED_INC                                                        (1 << (FLAGS_LAST + 4))


CM3u8StreamFragmentCollection::CM3u8StreamFragmentCollection(HRESULT *result)
  : CStreamFragmentCollection(result)
{
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

CM3u8StreamFragmentCollection::~CM3u8StreamFragmentCollection(void)
{
  FREE_MEM_CLASS(this->indexEncrypted);
  FREE_MEM_CLASS(this->indexDecrypted);
}

/* get methods */

CM3u8StreamFragment *CM3u8StreamFragmentCollection::GetItem(unsigned int index)
{
  return (CM3u8StreamFragment *)__super::GetItem(index);
}

HRESULT CM3u8StreamFragmentCollection::GetEncryptedStreamFragments(CIndexedM3u8StreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexEncrypted->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexEncrypted->Count())); i++)
  {
    unsigned int index = this->indexEncrypted->GetItem(i);

    CIndexedM3u8StreamFragment *item = new CIndexedM3u8StreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

HRESULT CM3u8StreamFragmentCollection::GetDecryptedStreamFragments(CIndexedM3u8StreamFragmentCollection *collection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, collection);

  CHECK_CONDITION_HRESULT(result, collection->EnsureEnoughSpace(this->indexDecrypted->Count()), result, E_OUTOFMEMORY);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->indexDecrypted->Count())); i++)
  {
    unsigned int index = this->indexDecrypted->GetItem(i);

    CIndexedM3u8StreamFragment *item = new CIndexedM3u8StreamFragment(&result, this->GetItem(index), index);
    CHECK_POINTER_HRESULT(result, item, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, collection->Add(item), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(item));
  }

  return result;
}

/* set methods */

/* other methods */

bool CM3u8StreamFragmentCollection::HasEncryptedStreamFragments(void)
{
  return (this->indexEncrypted->Count() != 0);
}

bool CM3u8StreamFragmentCollection::HasDecryptedStreamFragments(void)
{
  return (this->indexDecrypted->Count() != 0);
}

/* index methods */

bool CM3u8StreamFragmentCollection::InsertIndexes(unsigned int itemIndex)
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
    CM3u8StreamFragment *item = this->GetItem(itemIndex);

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

void CM3u8StreamFragmentCollection::RemoveIndexes(unsigned int startIndex, unsigned int count)
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

bool CM3u8StreamFragmentCollection::UpdateIndexes(unsigned int itemIndex, unsigned int count)
{
  bool result = __super::UpdateIndexes(itemIndex, count);

  if (result)
  {
    CM3u8StreamFragment *item = this->GetItem(itemIndex);

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

bool CM3u8StreamFragmentCollection::EnsureEnoughSpaceIndexes(unsigned int addingCount)
{
  bool result = __super::EnsureEnoughSpaceIndexes(addingCount);

  if (result)
  {
    result &= this->indexEncrypted->EnsureEnoughSpace(this->indexEncrypted->Count() + addingCount);
    result &= this->indexDecrypted->EnsureEnoughSpace(this->indexDecrypted->Count() + addingCount);
  }

  return result;
}

void CM3u8StreamFragmentCollection::ClearIndexes(void)
{
  __super::ClearIndexes();

  this->indexEncrypted->Clear();
  this->indexDecrypted->Clear();
}

/* protected methods */