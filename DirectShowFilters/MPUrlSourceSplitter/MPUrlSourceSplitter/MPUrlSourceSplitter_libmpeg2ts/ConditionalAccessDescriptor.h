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

#ifndef __CONDITIONAL_ACCESS_DESCRIPTOR_DEFINED
#define __CONDITIONAL_ACCESS_DESCRIPTOR_DEFINED

#include "Descriptor.h"

#define CONDITIONAL_ACCESS_DESCRIPTOR_FLAG_NONE                       DESCRIPTOR_FLAG_NONE

#define CONDITIONAL_ACCESS_DESCRIPTOR_FLAG_LAST                       (DESCRIPTOR_FLAG_LAST + 0)

#define TAG_CONDITIONAL_ACCESS_DESCRIPTOR                             0x09

#define CONDITIONAL_ACCESS_DESCRIPTOR_SYSTEM_ID_MASK                  0xFFFF
#define CONDITIONAL_ACCESS_DESCRIPTOR_SYSTEM_ID_SHIFT                 0

#define CONDITIONAL_ACCESS_DESCRIPTOR_PID_MASK                        0x1FFF
#define CONDITIONAL_ACCESS_DESCRIPTOR_PID_SHIFT                       0

#define CONDITIONAL_ACCESS_DESCRIPTOR_HEADER_LENGTH                   4

class CConditionalAccessDescriptor : public CDescriptor
{
public:
  CConditionalAccessDescriptor(HRESULT *result);
  virtual ~CConditionalAccessDescriptor(void);

  /* get methods */

  // gets CA system ID
  // @return : CA system ID
  uint16_t GetSystemId(void);

  // gets CA PID
  // @return : CA PID
  uint16_t GetPID(void);

  // gets CA private data size
  // @return : private data size
  uint8_t GetPrivateDataSize(void);

  // gets CA private data
  // @return : CA private data
  const uint8_t *GetPrivateData(void);

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // parses data in buffer
  // @param buffer : buffer with descriptor data for parsing
  // @param length : the length of data in buffer
  // @param onlyHeader : only header of descriptor is parsed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool onlyHeader);

  // gets new instance of descriptor
  // @return : new descriptor instance or NULL if error
  virtual CDescriptor *CreateDescriptor(void);
};

#endif
