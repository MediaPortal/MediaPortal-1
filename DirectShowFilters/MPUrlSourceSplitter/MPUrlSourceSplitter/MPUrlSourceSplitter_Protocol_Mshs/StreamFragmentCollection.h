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

#ifndef __STREAM_FRAGMENT_COLLECTION_DEFINED
#define __STREAM_FRAGMENT_COLLECTION_DEFINED

#include "KeyedCollection.h"
#include "StreamFragment.h"

class CStreamFragmentCollection : public CKeyedCollection<CStreamFragment, const wchar_t *>
{
public:
  CStreamFragmentCollection(void);
  ~CStreamFragmentCollection(void);

  // get the stream fragment from collection with specified url
  // @param name : the URL of stream fragment to find
  // @param invariant : specifies if stream fragment URL shoud be find with invariant casing
  // @return : the reference to stream fragment or NULL if not find
  CStreamFragment *GetStreamFragment(const wchar_t *url, bool invariant);

protected:

  // compare two item keys
  // @param firstKey : the first item key to compare
  // @param secondKey : the second item key to compare
  // @param context : the reference to user defined context
  // @return : 0 if keys are equal, lower than zero if firstKey is lower than secondKey, greater than zero if firstKey is greater than secondKey
  int CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context);

  // gets key for item
  // @param item : the item to get key
  // @return : the key of item
  const wchar_t *GetKey(CStreamFragment *item);

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  CStreamFragment *Clone(CStreamFragment *item);
};

#endif