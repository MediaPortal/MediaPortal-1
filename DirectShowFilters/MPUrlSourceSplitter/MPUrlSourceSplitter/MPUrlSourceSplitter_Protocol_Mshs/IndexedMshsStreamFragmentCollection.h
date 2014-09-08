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

#ifndef __INDEXED_MSHS_STREAM_FRAGMENT_COLLECTION_DEFINED
#define __INDEXED_MSHS_STREAM_FRAGMENT_COLLECTION_DEFINED

#include "IndexedCacheFileItemCollection.h"
#include "IndexedMshsStreamFragment.h"

class CIndexedMshsStreamFragmentCollection : public CIndexedCacheFileItemCollection
{
public:
  CIndexedMshsStreamFragmentCollection(HRESULT *result);
  virtual ~CIndexedMshsStreamFragmentCollection(void);

  /* get methods */

  // get the item from collection with specified index
  // @param index : the index of item to find
  // @return : the reference to item or NULL if not find
  virtual CIndexedMshsStreamFragment *GetItem(unsigned int index);

  /* set methods */

  /* other methods */

protected:

  /* methods */
};

#endif