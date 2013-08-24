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

#include "SourceDescriptionItem.h"
#include "BufferHelper.h"

#include <stdint.h>

CSourceDescriptionItem::CSourceDescriptionItem(void)
{
  this->type = UINT_MAX;
  this->payload = NULL;
  this->payloadSize = 0;
}

CSourceDescriptionItem::~CSourceDescriptionItem(void)
{
  FREE_MEM(this->payload);
}

/* get methods */

unsigned int CSourceDescriptionItem::GetSize(void)
{
  return SOURCE_DESCRIPTION_ITEM_HEADER;
}

bool CSourceDescriptionItem::GetSourceDescriptionItem(unsigned char *buffer, unsigned int length)
{
  unsigned int size = this->GetSize();
  bool result = ((buffer != NULL) && (length >= size));

  if (result)
  {
    // write source description item header
    unsigned int position = 0;

    WBE8INC(buffer, position, this->GetType());
    WBE8INC(buffer, position, (size - SOURCE_DESCRIPTION_ITEM_HEADER));
  }

  return result;
}

/* set methods */

/* other methods */

void CSourceDescriptionItem::Clear(void)
{
  this->type = UINT_MAX;
  FREE_MEM(this->payload);
  this->payloadSize = 0;
}

bool CSourceDescriptionItem::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = ((buffer != NULL) && (length >= SOURCE_DESCRIPTION_ITEM_HEADER));

  if (result)
  {
    this->Clear();

    unsigned int position = 0;
    unsigned int size = 0;

    RBE8INC(buffer, position, this->type);
    RBE8INC(buffer, position, size);

    // in size is not included two already read bytes
    this->payloadSize = size;
    size += SOURCE_DESCRIPTION_ITEM_HEADER;

    result &= (length >= size);

    if ((result) && (this->payloadSize != 0))
    {
      this->payload = ALLOC_MEM_SET(this->payload, unsigned char, this->payloadSize, 0);
      result &= (this->payload != NULL);

      if (result)
      {
        memcpy(this->payload, buffer + position, this->payloadSize);
      }
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}
