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

#ifndef __SEGMENT_RUN_ENTRY_COLLECTION_DEFINED
#define __SEGMENT_RUN_ENTRY_COLLECTION_DEFINED

#include "Collection.h"
#include "SegmentRunEntry.h"

class CSegmentRunEntryCollection : public CCollection<CSegmentRunEntry>
{
public:
  CSegmentRunEntryCollection(HRESULT *result);
  ~CSegmentRunEntryCollection(void);

  /* get methods */

  // returns segment run entry index of fragment run entry index
  // @param fragmentRunEntryIndex : the fragment run entry index to compare
  // @return : segment run entry index or UINT_MAX if not found
  unsigned int GetFragmentRunEntrySegmentIndex(uint32_t fragmentRunEntryIndex);

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CSegmentRunEntry *Clone(CSegmentRunEntry *item);
};

#endif