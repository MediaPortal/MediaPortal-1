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

#ifndef __SAMPLE_ENTRY_BOX_FACTORY_DEFINED
#define __SAMPLE_ENTRY_BOX_FACTORY_DEFINED

#include "BoxFactory.h"

#define CREATE_SPECIFIC_BOX_WITHOUT_HEADER_TYPE(box, boxType, buffer, length, continueParsing, result)\
                                                                                                      \
if (continueParsing && (result == NULL))                                                              \
{                                                                                                     \
  boxType *specificBox = new boxType();                                                               \
  continueParsing &= (specificBox != NULL);                                                           \
                                                                                                      \
  if (continueParsing)                                                                                \
  {                                                                                                   \
    continueParsing &= specificBox->Parse(buffer, length);                                            \
    if (continueParsing)                                                                              \
    {                                                                                                 \
      result = specificBox;                                                                           \
    }                                                                                                 \
  }                                                                                                   \
                                                                                                      \
  if (!continueParsing)                                                                               \
  {                                                                                                   \
    FREE_MEM_CLASS(specificBox);                                                                      \
  }                                                                                                   \
}


class CSampleEntryBoxFactory :
  public CBoxFactory
{
public:
  // initializes a new instance of CSampleEntryBoxFactory class
  CSampleEntryBoxFactory(void);

  // destructor
  virtual ~CSampleEntryBoxFactory(void);

  // creates box based on type
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param : the type of handler from handler box
  // @return : box or NULL if error
  virtual CBox *CreateBox(const uint8_t *buffer, uint32_t length, uint32_t handlerType);

protected:

  // creates box based on type
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : always NULL
  virtual CBox *CreateBox(const uint8_t *buffer, uint32_t length);

};

#endif