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

#include "SegmentRunEntryCollection.h"

CSegmentRunEntryCollection::CSegmentRunEntryCollection(HRESULT *result)
  : CCollection(result)
{
}

CSegmentRunEntryCollection::~CSegmentRunEntryCollection(void)
{
}

/* get methods */

unsigned int CSegmentRunEntryCollection::GetFragmentRunEntrySegmentIndex(uint32_t fragmentRunEntryIndex)
{
  unsigned int first = 0;
  unsigned int last = this->Count() - 1;
  bool result = false;
  unsigned int index = UINT_MAX;

  while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
  {
    // compute middle index
    unsigned int middle = (first + last) / 2;

    // get segment run entry at middle index
    CSegmentRunEntry *segmentRunEntry = this->GetItem(middle);

    // compare segment run entry cumulated fragment count to fragment run entry index
    if (fragmentRunEntryIndex > segmentRunEntry->GetCumulatedFragmentCount())
    {
      // fragment run entry index is bigger than segment run entry cumulated fragment count
      // search in top half
      first = middle + 1;
    }
    else if (fragmentRunEntryIndex < segmentRunEntry->GetCumulatedFragmentCount()) 
    {
      // fragment run entry index is lower than segment run entry cumulated fragment count
      // search in bottom half
      last = middle - 1;
    }
    else
    {
      // we found segment run entry with same cumulated fragment count as fragment run entry index
      index = middle;
      result = true;
      break;
    }
  }

  if (!result)
  {
    // we don't found segment run entry
    // it means that segment run entry with 'fragment run entry index' belongs between first and last
    index = last;
    result = true;
  }

  return result ? index : UINT_MAX;
}

/* set methods */

/* other methods */

/* protected methods */

CSegmentRunEntry *CSegmentRunEntryCollection::Clone(CSegmentRunEntry *item)
{
  return NULL;
}