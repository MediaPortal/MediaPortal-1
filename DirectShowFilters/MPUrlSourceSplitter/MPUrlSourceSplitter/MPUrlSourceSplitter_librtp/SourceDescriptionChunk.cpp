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

#include "SourceDescriptionChunk.h"
#include "BufferHelper.h"
#include "SourceDescriptionItemFactory.h"
#include "NullSourceDescriptionItem.h"

#include <stdint.h>

CSourceDescriptionChunk::CSourceDescriptionChunk(void)
{
  this->identifier = UINT_MAX;
  this->size = UINT_MAX;
  this->items = new CSourceDescriptionItemCollection();
}

CSourceDescriptionChunk::~CSourceDescriptionChunk(void)
{
  FREE_MEM_CLASS(this->items);
}

/* get methods */

unsigned int CSourceDescriptionChunk::GetIdentifier(void)
{
  return this->identifier;
}

unsigned int CSourceDescriptionChunk::GetSize(void)
{
  return this->size;
}

CSourceDescriptionItemCollection *CSourceDescriptionChunk::GetItems(void)
{
  return this->items;
}

/* set methods */

/* other methods */

void CSourceDescriptionChunk::Clear(void)
{
  this->identifier = UINT_MAX;
  this->size = UINT_MAX;
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->items, this->items->Clear());
}

bool CSourceDescriptionChunk::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = ((buffer != NULL) && (length >= SOURCE_DESCRIPTION_CHUNK_MIN_SIZE) && (this->items != NULL));

  if (result)
  {
    this->Clear();

    unsigned int position = 0;

    RBE32INC(buffer, position, this->identifier);

    // parse source description items
    // NULL source description item must be found - it is end of list
    CSourceDescriptionItemFactory *factory = new CSourceDescriptionItemFactory();
    result &= (factory != NULL);

    bool nullSourceDescriptionItemFound = false;
    unsigned int tempPosition = 0;
    while (result && (!nullSourceDescriptionItemFound) && (position < length))
    {
      CSourceDescriptionItem *item = factory->CreateSourceDescriptionItem(buffer + position, length - position, &tempPosition);
      result &= (item != NULL);

      if (result)
      {
        result &= this->items->Add(item);
      }

      if (result)
      {
        position += item->GetSize();

        nullSourceDescriptionItemFound = (item->GetType() == NULL_SOURCE_DESCRIPTION_ITEM_TYPE);
      }

      if (!result)
      {
        FREE_MEM_CLASS(item);
      }
    }

    FREE_MEM_CLASS(factory);

    if (result)
    {
      // each source description chunk must be aligned to 32-bit boundary
      this->size = position;
      if ((this->size % 4) != 0)
      {
        this->size += (4 - (this->size % 4));
      }
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}