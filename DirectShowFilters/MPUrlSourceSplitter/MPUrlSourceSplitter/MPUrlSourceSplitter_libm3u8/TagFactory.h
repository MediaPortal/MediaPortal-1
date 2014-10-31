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

#ifndef __TAG_FACTORY_DEFINED
#define __TAG_FACTORY_DEFINED

#include "Tag.h"

#define CREATE_SPECIFIC_TAG(tempTag, tagIdentifier, tagType, continueParsing, tag)                                  \
                                                                                                                    \
if (SUCCEEDED(continueParsing) && (tag == NULL) && (wcscmp(tempTag->GetTag(), tagIdentifier) == 0))                 \
{                                                                                                                   \
  tagType *specificTag = new tagType(&continueParsing);                                                             \
  CHECK_POINTER_HRESULT(continueParsing, specificTag, continueParsing, E_OUTOFMEMORY);                              \
                                                                                                                    \
  if (SUCCEEDED(continueParsing))                                                                                   \
  {                                                                                                                 \
    if (specificTag->ParseGeneralTag(tempTag))                                                                      \
    {                                                                                                               \
      tag = specificTag;                                                                                            \
    }                                                                                                               \
  }                                                                                                                 \
                                                                                                                    \
  if (tag == NULL)                                                                                                  \
  {                                                                                                                 \
    FREE_MEM_CLASS(specificTag);                                                                                    \
  }                                                                                                                 \
}

class CTagFactory
{
public:
  CTagFactory(HRESULT *result);
  ~CTagFactory(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // creates tag from general tag
  // @param result : reference to HRESULT variable holding error code if some error
  // @param generalTag : the general tag to create specific tag
  // @return : tag or NULL
  CTag *CreateTag(HRESULT *result, CGeneralTag *generalTag);
};

#endif