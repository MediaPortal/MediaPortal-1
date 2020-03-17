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

#pragma once

#ifndef __SEEK_INDEX_ENTRY_COLLECTION_DEFINED
#define __SEEK_INDEX_ENTRY_COLLECTION_DEFINED

#include "Collection.h"
#include "SeekIndexEntry.h"

class CSeekIndexEntryCollection : public CCollection<CSeekIndexEntry>
{
public:
  CSeekIndexEntryCollection(HRESULT *result);
  virtual ~CSeekIndexEntryCollection(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // adds seek index entry to collection on correct place
  // @param position : the position of seek index entry
  // @param timestamp : the timestamp of seek index entry
  // @return : S_OK if successful, E_SEEK_INDEX_ENTRY_EXISTS if index entry with same timestamp exists in collection, error code otherwise
  HRESULT AddSeekIndexEntry(int64_t position, int64_t timestamp);

  // finds seek index entry with specified timestamp and search flags
  // @param timestamp : the timestamp to find
  // @param backwards : true if search backwards, false otherwise
  // @return : index of seek index entry or E_NOT_FOUND_SEEK_INDEX_ENTRY if not found
  HRESULT FindSeekIndexEntry(int64_t timestamp, bool backwards);

protected:

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CSeekIndexEntry *Clone(CSeekIndexEntry *item);
};

#endif