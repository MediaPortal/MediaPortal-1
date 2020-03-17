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

#ifndef __SESSION_TAG_FACTORY_DEFINED
#define __SESSION_TAG_FACTORY_DEFINED

#include "SessionTag.h"

#define CREATE_SPECIFIC_SESSION_TAG(sessionTag, tag, sessionTagType, buffer, length, continueParsing, result, position)       \
                                                                                                                    \
if (SUCCEEDED(continueParsing) && (result == NULL) && (wcscmp(sessionTag->GetOriginalTag(), tag) == 0))             \
{                                                                                                                   \
  sessionTagType *specificTag = new sessionTagType(&continueParsing);                                               \
  CHECK_POINTER_HRESULT(continueParsing, specificTag, continueParsing, E_OUTOFMEMORY);                              \
                                                                                                                    \
  if (SUCCEEDED(continueParsing))                                                                                   \
  {                                                                                                                 \
    unsigned int tempPosition = specificTag->Parse(buffer, length);                                                 \
                                                                                                                    \
    if (tempPosition != 0)                                                                                          \
    {                                                                                                               \
      position = tempPosition;                                                                                      \
      result = specificTag;                                                                                         \
    }                                                                                                               \
  }                                                                                                                 \
                                                                                                                    \
  if (result == NULL)                                                                                               \
  {                                                                                                                 \
    FREE_MEM_CLASS(specificTag);                                                                                    \
    position = 0;                                                                                                   \
  }                                                                                                                 \
}

class CSessionTagFactory
{
public:
  // initializes a new instance of CSessionTagFactory class
  CSessionTagFactory(void);

  // destructor
  virtual ~CSessionTagFactory(void);

  // creates session tag from buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @param position : pointer to position after parsing
  // @return : session tag or NULL if error
  virtual CSessionTag *CreateSessionTag(const wchar_t *buffer, unsigned int length, unsigned int *position);
};

#endif