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

#ifndef __TAG_COLLECTION_DEFINED
#define __TAG_COLLECTION_DEFINED

#include "Collection.h"
#include "Tag.h"
#include "MediaSequenceTag.h"
#include "EndListTag.h"

class CTagCollection : public CCollection<CTag>
{
public:
  CTagCollection(HRESULT *result);
  virtual ~CTagCollection(void);

  /* get methods */

  // gets end list tag from collection of items
  // @return : end list tag or NULL if not found
  virtual CEndListTag *GetEndList(void);

  // gets media sequence tag from collection of items
  // @return : media sequence tag or NULL if not found
  virtual CMediaSequenceTag *GetMediaSequence(void);

  /* set methods */

  /* other methods */

protected:

  // clones specified tag
  // @param item : the tag to clone
  // @return : deep clone of tag or NULL if not implemented
  virtual CTag *Clone(CTag *tag);
};

#endif