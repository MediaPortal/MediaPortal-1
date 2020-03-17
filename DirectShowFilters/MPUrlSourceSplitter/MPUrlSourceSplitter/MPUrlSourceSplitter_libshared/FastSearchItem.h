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

#ifndef __FAST_SEARCH_ITEM_DEFINED
#define __FAST_SEARCH_ITEM_DEFINED

#include "Flags.h"

#define FAST_SEARCH_ITEM_FLAG_NONE                                    FLAGS_NONE

#define FAST_SEARCH_ITEM_FLAG_LAST                                    (FLAGS_LAST + 0)

class CFastSearchItemCollection;

class CFastSearchItem : public CFlags
{
public:
  CFastSearchItem(HRESULT *result);
  virtual ~CFastSearchItem(void);

  /* get methods */

  /* set methods */

  // set owner of this item
  // @param owner : the owner of item to set
  void SetOwner(CFastSearchItemCollection *owner);

  /* other methods */

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  virtual CFastSearchItem *Clone(void);

protected:
  // holds owner
  CFastSearchItemCollection *owner;

  /* methods */

  // gets new instance of fast search item
  // @return : new fast search item instance or NULL if error
  virtual CFastSearchItem *CreateItem(void) = 0;

  // deeply clones current instance
  // @param item : the fast search item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CFastSearchItem *item);
};

#endif