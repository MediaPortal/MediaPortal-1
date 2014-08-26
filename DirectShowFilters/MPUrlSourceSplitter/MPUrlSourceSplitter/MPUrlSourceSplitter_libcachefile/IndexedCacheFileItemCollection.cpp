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

#include "IndexedCacheFileItemCollection.h"

CIndexedCacheFileItemCollection::CIndexedCacheFileItemCollection(HRESULT *result)
  : CIndexedFastSearchItemCollection(result)
{
}

CIndexedCacheFileItemCollection::~CIndexedCacheFileItemCollection(void)
{
}

/* get methods */

CIndexedCacheFileItem *CIndexedCacheFileItemCollection::GetItem(unsigned int index)
{
  return (CIndexedCacheFileItem *)__super::GetItem(index);
}

/* set methods */

/* other methods */

/* protected methods */

CIndexedFastSearchItem *CIndexedCacheFileItemCollection::Clone(CIndexedFastSearchItem *item)
{
  return NULL;
}