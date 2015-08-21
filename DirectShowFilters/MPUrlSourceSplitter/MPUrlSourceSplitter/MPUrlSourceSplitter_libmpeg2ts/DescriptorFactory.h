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

#ifndef __DESCRIPTOR_FACTORY_DEFINED
#define __DESCRIPTOR_FACTORY_DEFINED

#include "Descriptor.h"

#define CREATE_SPECIFIC_DESCRIPTOR(descriptor, specificDescriptorTag, specificDescriptorType, buffer, length, continueParsing, result)              \
                                                                                                                                                    \
if (SUCCEEDED(continueParsing) && (result == NULL) && (descriptor->GetTag() == specificDescriptorTag))                                              \
{                                                                                                                                                   \
  specificDescriptorType *specificDescriptor = new specificDescriptorType(&continueParsing);                                                        \
  CHECK_POINTER_HRESULT(continueParsing, specificDescriptor, continueParsing, E_OUTOFMEMORY);                                                       \
                                                                                                                                                    \
  CHECK_CONDITION_HRESULT(continueParsing, specificDescriptor->Parse(buffer, length), continueParsing, E_FAIL);                                     \
  CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), result = specificDescriptor);                                                                 \
                                                                                                                                                    \
  CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(specificDescriptor));                                                             \
}

class CDescriptorFactory
{
public:
  CDescriptorFactory(HRESULT *result);
  virtual ~CDescriptorFactory(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // creates descriptor based on type
  // @param buffer : buffer with descriptor data for parsing
  // @param length : the length of data in buffer
  // @return : box or NULL if error
  virtual CDescriptor *CreateDescriptor(const uint8_t *buffer, uint32_t length);

protected:

  /* methods */

};

#endif
