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

#include "SeekIndexEntryCollection.h"
#include "ErrorCodes.h"

CSeekIndexEntryCollection::CSeekIndexEntryCollection(HRESULT *result)
  : CCollection(result)
{
}

CSeekIndexEntryCollection::~CSeekIndexEntryCollection(void)
{
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CSeekIndexEntryCollection::AddSeekIndexEntry(int64_t position, int64_t timestamp)
{
  HRESULT result = this->EnsureEnoughSpace(this->Count() + 1) ? S_OK : E_OUTOFMEMORY;

  if (SUCCEEDED(result))
  {
    result = this->FindSeekIndexEntry(timestamp, false);

    // seek index entry was possibly found, we need to check timestamp
    CHECK_CONDITION_HRESULT(result, this->GetItem((unsigned int)result)->GetTimestamp() != timestamp, result, E_SEEK_INDEX_ENTRY_EXISTS);
    CHECK_CONDITION_EXECUTE(result == E_NOT_FOUND_SEEK_INDEX_ENTRY, result = this->Count());

    if (SUCCEEDED(result))
    {
      CSeekIndexEntry *indexEntry = new CSeekIndexEntry(position, timestamp);
      CHECK_POINTER_HRESULT(result, indexEntry, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->Insert((unsigned int)result, indexEntry), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(indexEntry));
    }
  }

  return SUCCEEDED(result) ? S_OK : result;
}

HRESULT CSeekIndexEntryCollection::FindSeekIndexEntry(int64_t timestamp, bool backwards)
{
  HRESULT result = (this->Count() != 0) ? S_OK : E_NOT_FOUND_SEEK_INDEX_ENTRY;

  if (SUCCEEDED(result))
  {
    unsigned int first = 0;
    unsigned int last = this->Count() - 1;
    unsigned int middle = 0;

    // optimise searching seek index entries of end of collection
    if ((last != 0) && (this->GetItem(last)->GetTimestamp() < timestamp))
    {
      first = last - 1;
    }

    while ((last - first) > 1)
    {
      middle = (first + last) / 2;

      if (this->GetItem(middle)->GetTimestamp() >= timestamp)
      {
        last = middle;
      }

      if (this->GetItem(middle)->GetTimestamp() <= timestamp)
      {
        first = middle;
      }
    }

    if (backwards)
    {
      CHECK_CONDITION_HRESULT(result, this->GetItem(first)->GetTimestamp() <= timestamp, (HRESULT)first, E_NOT_FOUND_SEEK_INDEX_ENTRY);
    }
    else
    {
      CHECK_CONDITION_HRESULT(result, this->GetItem(last)->GetTimestamp() >= timestamp, (HRESULT)last, E_NOT_FOUND_SEEK_INDEX_ENTRY);
    }
  }

  return result;
}

/* protected methods */

CSeekIndexEntry *CSeekIndexEntryCollection::Clone(CSeekIndexEntry *item)
{
  return NULL;
}