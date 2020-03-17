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

#ifndef __ATTRIBUTE_FACTORY_DEFINED
#define __ATTRIBUTE_FACTORY_DEFINED

#include "Attribute.h"

#define CREATE_SPECIFIC_ATTRIBUTE(attribute, name, attributeType, continueParsing, result, version)                 \
                                                                                                                    \
if (SUCCEEDED(continueParsing) && (result == NULL) && (wcscmp(attribute->GetName(), name) == 0))                    \
{                                                                                                                   \
  attributeType *specificAttribute = new attributeType(&continueParsing);                                           \
  CHECK_POINTER_HRESULT(continueParsing, specificAttribute, continueParsing, E_OUTOFMEMORY);                        \
                                                                                                                    \
  if (SUCCEEDED(continueParsing))                                                                                   \
  {                                                                                                                 \
    if (specificAttribute->Parse(version, attribute->GetName(), attribute->GetValue()))                             \
    {                                                                                                               \
      result = specificAttribute;                                                                                   \
    }                                                                                                               \
  }                                                                                                                 \
                                                                                                                    \
  if (result == NULL)                                                                                               \
  {                                                                                                                 \
    FREE_MEM_CLASS(specificAttribute);                                                                              \
  }                                                                                                                 \
}


class CAttributeFactory
{
public:
  CAttributeFactory(HRESULT *result);
  virtual ~CAttributeFactory(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // creates attribute from buffer
  // @param version : the playlist version
  // @param buffer : buffer with attribute data for parsing
  // @param length : the length of data in buffer
  // @return : attribute or NULL if error
  virtual CAttribute *CreateAttribute(unsigned int version, const wchar_t *buffer, unsigned int length);

protected:

  /* methods */
};

#endif