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

#ifndef __ITEM_COLLECTION_DEFINED
#define __ITEM_COLLECTION_DEFINED

#include "Collection.h"
#include "Item.h"

class CVersionTag;

class CItemCollection : public CCollection<CItem>
{
public:
  CItemCollection(HRESULT *result);
  virtual ~CItemCollection(void);

  /* get methods */

  // gets version tag from collection of items
  // @return : version tag or NULL if not found
  virtual CVersionTag *GetVersion(void);

  /* set methods */

  /* other methods */

protected:

  // clones specified item
  // @param item : the item to clone
  // @return : deep clone of item or NULL if not implemented
  virtual CItem *Clone(CItem *item);
};

#endif