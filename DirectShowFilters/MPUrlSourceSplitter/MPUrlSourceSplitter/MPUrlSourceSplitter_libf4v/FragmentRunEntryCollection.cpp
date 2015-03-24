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

#include "FragmentRunEntryCollection.h"

CFragmentRunEntryCollection::CFragmentRunEntryCollection(HRESULT *result)
  : CCollection(result)
{
}

CFragmentRunEntryCollection::~CFragmentRunEntryCollection(void)
{
}

/* get methods */

unsigned int CFragmentRunEntryCollection::GetFragmentRunEntryIndex(uint64_t timestamp)
{
  unsigned int first = 0;
  unsigned int last = this->Count() - 1;
  bool result = false;
  unsigned int index = UINT_MAX;

  while ((first <= last) && (first != UINT_MAX) && (last != UINT_MAX))
  {
    // compute middle index
    unsigned int middle = (first + last) / 2;

    // get fragment run entry at middle index
    CFragmentRunEntry *fragmentRunEntry = this->GetItem(middle);

    // compare fragment run entry first fragment timestamp to timestamp
    if (timestamp > fragmentRunEntry->GetFirstFragmentTimestamp())
    {
      // timestamp is bigger than fragment run entry first fragment timestamp 
      // search in top half
      first = middle + 1;
    }
    else if (timestamp < fragmentRunEntry->GetFirstFragmentTimestamp()) 
    {
      // timestamp is lower than fragment run entry first fragment timestamp 
      // search in bottom half
      last = middle - 1;
    }
    else
    {
      // we found fragment run entry with same first fragment timestamp as timestamp
      index = middle;
      result = true;
      break;
    }
  }

  if (!result)
  {
    // we don't found fragment run entry
    // it means that fragment run entry with 'timestamp' belongs between first and last
    index = last;
    result = true;
  }

  return result ? index : UINT_MAX;
}

/* set methods */

/* other methods */

/* protected methods */

CFragmentRunEntry *CFragmentRunEntryCollection::Clone(CFragmentRunEntry *item)
{
  return NULL;
}