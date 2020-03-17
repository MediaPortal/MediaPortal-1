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

#ifndef __INDEXED_FAST_SEARCH_ITEM_DEFINED
#define __INDEXED_FAST_SEARCH_ITEM_DEFINED

#include "Flags.h"
#include "FastSearchItem.h"

#define INDEXED_FAST_SEARCH_ITEM_FLAG_NONE                            FLAGS_NONE

#define INDEXED_FAST_SEARCH_ITEM_FLAG_LAST                            (FLAGS_LAST + 0)

#define ITEM_INDEX_NOT_SET                                            UINT_MAX

class CIndexedFastSearchItem : public CFlags
{
public:
  CIndexedFastSearchItem(HRESULT *result, CFastSearchItem *item, unsigned int index);
  virtual ~CIndexedFastSearchItem(void);

  /* get methods */

  // gets item
  // @return : item
  virtual CFastSearchItem *GetItem(void);

  // gets item index in original collection
  // @return : item index in original collection
  virtual unsigned int GetItemIndex(void);

  /* set methods */

  /* other methods */

protected:
  // holds reference to item (only reference, not deep clone)
  CFastSearchItem *item;
  // holds index of item in original collection
  unsigned int index;

  /* methods */
};

#endif