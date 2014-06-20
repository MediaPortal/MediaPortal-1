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

CSourceDescriptionChunk::CSourceDescriptionChunk(HRESULT *result)
{
  this->identifier = 0;
  this->items = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->items = new CSourceDescriptionItemCollection(result);
    CHECK_POINTER_HRESULT(*result, this->items, *result, E_OUTOFMEMORY);
  }
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
  // each source description chunk must be aligned to 32-bit boundary
  unsigned int size = 4;
  bool foundNullSourceDescriptionItem = false;
  for (unsigned int i = 0; i < this->GetItems()->Count(); i++)
  {
    CSourceDescriptionItem *item = this->GetItems()->GetItem(i);

    foundNullSourceDescriptionItem |= (item->GetType() == NULL_SOURCE_DESCRIPTION_ITEM_TYPE);

    size += item->GetSize();
  }

  // each chunk must be ended with NULL source description item
  // if not found, add one NULL source description item
  if (!foundNullSourceDescriptionItem)
  {
    size += NULL_SOURCE_DESCRIPTION_ITEM_HEADER;
  }

  if ((size % 4) != 0)
  {
    size += (4 - (size % 4));
  }

  if (size < SOURCE_DESCRIPTION_CHUNK_MIN_SIZE)
  {
    size = SOURCE_DESCRIPTION_CHUNK_MIN_SIZE;
  }

  return size;
}

CSourceDescriptionItemCollection *CSourceDescriptionChunk::GetItems(void)
{
  return this->items;
}

bool CSourceDescriptionChunk::GetChunk(unsigned char *buffer, unsigned int length)
{
  unsigned int size = this->GetSize();
  bool result = ((buffer != NULL) && (length >= size));

  if (result)
  {
    unsigned int position = 0;

    WBE32INC(buffer, position, this->identifier);
    for (unsigned int i = 0; (result && (i < this->GetItems()->Count())); i++)
    {
      CSourceDescriptionItem *item = this->GetItems()->GetItem(i);

      result &= item->GetSourceDescriptionItem(buffer + position, length - position);
      position += item->GetSize();
    }

    if (position < size)
    {
      // each source description chunk must be aligned to 32-bit boundary
      // everything between current position and chunk size must be filled with zeros (padding)
      memset(buffer + position, 0, size - position);
    }
  }

  return result;
}

/* set methods */

void CSourceDescriptionChunk::SetIdentifier(unsigned int identifier)
{
  this->identifier = identifier;
}

/* other methods */

void CSourceDescriptionChunk::Clear(void)
{
  this->identifier = 0;
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

    // by specification each chunk must be ended with NULL source description item
    result &= nullSourceDescriptionItemFound;

    FREE_MEM_CLASS(factory);
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}