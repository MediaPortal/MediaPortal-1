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

#ifndef __SOURCE_DESCRIPTION_ITEM_FACTORY_DEFINED
#define __SOURCE_DESCRIPTION_ITEM_FACTORY_DEFINED

#include "SourceDescriptionItem.h"

#define CREATE_SPECIFIC_SOURCE_DESCRIPTION_ITEM(sdesItemType, buffer, length, continueParsing, result, position)    \
                                                                                                                    \
if (SUCCEEDED(continueParsing) && (result == NULL))                                                                 \
{                                                                                                                   \
  sdesItemType *specificItem = new sdesItemType(&continueParsing);                                                  \
  CHECK_POINTER_HRESULT(continueParsing, specificItem, continueParsing, E_OUTOFMEMORY);                             \
                                                                                                                    \
  if (SUCCEEDED(continueParsing))                                                                                   \
  {                                                                                                                 \
    if (specificItem->Parse(buffer, length))                                                                        \
    {                                                                                                               \
      position = specificItem->GetSize();                                                                           \
      result = specificItem;                                                                                        \
    }                                                                                                               \
  }                                                                                                                 \
                                                                                                                    \
  if (result == NULL)                                                                                               \
  {                                                                                                                 \
    FREE_MEM_CLASS(specificItem);                                                                                   \
    position = 0;                                                                                                   \
  }                                                                                                                 \
}


class CSourceDescriptionItemFactory
{
public:
  // initializes a new instance of CSourceDescriptionItemFactory class
  CSourceDescriptionItemFactory(void);
  virtual ~CSourceDescriptionItemFactory(void);

  // creates source description item from buffer
  // @param buffer : buffer with source description item data for parsing
  // @param length : the length of data in buffer
  // @param position : pointer to position after parsing
  // @return : source description item or NULL if error
  virtual CSourceDescriptionItem *CreateSourceDescriptionItem(const unsigned char *buffer, unsigned int length, unsigned int *position);
};

#endif