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

#ifndef __DESCRIPTOR_DEFINED
#define __DESCRIPTOR_DEFINED

#include "Flags.h"

#define DESCRIPTOR_FLAG_NONE                                          FLAGS_NONE

#define DESCRIPTOR_FLAG_PARSED                                        (1 << (FLAGS_LAST + 0))

#define DESCRIPTOR_FLAG_LAST                                          (FLAGS_LAST + 1)

#define DESCRIPTOR_HEADER_LENGTH                                      2

class CDescriptor : public CFlags
{
public:
  CDescriptor(HRESULT *result);
  virtual ~CDescriptor(void);

  /* get methods */

  // gets descriptor tag
  // @return : descriptor tag
  virtual uint8_t GetTag(void);

  // gets payload size
  // @return : payload size
  virtual uint8_t GetPayloadSize(void);

  // gets payload
  // @return : payload or NULL if payload size is 0
  virtual const uint8_t *GetPayload(void);

  // gets descriptor size
  // @return : descriptor size
  virtual uint8_t GetDescriptorSize(void);

  /* set methods */

  // sets payload and size
  // @param payload : the payload or NULL if payload size is 0
  // @param payloadSize : the payload size to set
  virtual bool SetPayload(const uint8_t *payload, unsigned int payloadSize);

  /* other methods */

  // tests if decriptor is successfully parsed
  // @return : true if successfully parsed, false otherwise
  virtual bool IsParsed(void);

  // parses data in buffer
  // @param buffer : buffer with descriptor data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const unsigned char *buffer, uint32_t length);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  virtual CDescriptor *Clone(void);

protected:
  // holds descriptor tag
  uint8_t tag;
  // holds descriptor payload size
  uint8_t payloadSize;
  // holds descriptor payload
  uint8_t *payload;

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

  // deeply clones current instance
  // @param descriptor : the descriptor instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CDescriptor *descriptor);
};

#endif
