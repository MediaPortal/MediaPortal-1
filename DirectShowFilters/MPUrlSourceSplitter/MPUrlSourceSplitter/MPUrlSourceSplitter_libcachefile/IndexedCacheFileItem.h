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

#ifndef __INDEXED_CACHE_FILE_ITEM_DEFINED
#define __INDEXED_CACHE_FILE_ITEM_DEFINED

#include "IndexedFastSearchItem.h"
#include "CacheFileItem.h"

#define INDEXED_CACHE_FILE_ITEM_FLAG_NONE                             INDEXED_FAST_SEARCH_ITEM_FLAG_NONE

#define INDEXED_CACHE_FILE_ITEM_FLAG_LAST                             (INDEXED_FAST_SEARCH_ITEM_FLAG_LAST + 0)

class CIndexedCacheFileItem : public CIndexedFastSearchItem
{
public:
  CIndexedCacheFileItem(HRESULT *result, CCacheFileItem *item, unsigned int index);
  virtual ~CIndexedCacheFileItem(void);

  /* get methods */

  // gets cache file item
  // @return : cache file item
  virtual CCacheFileItem *GetItem(void);

  /* set methods */

  /* other methods */

protected:

  /* methods */
};

#endif