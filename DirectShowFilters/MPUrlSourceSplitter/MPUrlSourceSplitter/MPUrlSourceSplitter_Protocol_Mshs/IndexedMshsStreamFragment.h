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

#ifndef __INDEXED_MSHS_STREAM_FRAGMENT_DEFINED
#define __INDEXED_MSHS_STREAM_FRAGMENT_DEFINED

#include "IndexedCacheFileItem.h"
#include "MshsStreamFragment.h"

#define INDEXED_MSHS_STREAM_FRAGMENT_FLAG_NONE                        INDEXED_CACHE_FILE_ITEM_FLAG_NONE

#define INDEXED_MSHS_STREAM_FRAGMENT_FLAG_LAST                        (INDEXED_CACHE_FILE_ITEM_FLAG_LAST + 0)

class CIndexedMshsStreamFragment : public CIndexedCacheFileItem
{
public:
  CIndexedMshsStreamFragment(HRESULT *result, CMshsStreamFragment *item, unsigned int index);
  virtual ~CIndexedMshsStreamFragment(void);

  /* get methods */

  // gets MSHS stream fragment
  // @return : MSHS stream fragment
  virtual CMshsStreamFragment *GetItem(void);

  /* set methods */

  /* other methods */

protected:

  /* methods */
};

#endif