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

#ifndef __SERIALIZABLE_DEFINED
#define __SERIALIZABLE_DEFINED

#include <stdint.h>

class CSerializable
{
public:
  // creates new instance of CSerializable class
  CSerializable(void);

  // destructor
  virtual ~CSerializable(void);

  /* get methods */

  // gets necessary buffer length for serializing instance
  // @return : necessary size for buffer
  virtual uint32_t GetSerializeSize(void);

  // gets necessary buffer length for serializing string
  // @param input : string to serialize
  // @return : necessary size for buffer
  virtual uint32_t GetSerializeStringSize(const wchar_t *input);

  /* set methods */

  /* other methods */

  // serialize instance into buffer, buffer must be allocated before and must have necessary size
  // @param buffer : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Serialize(uint8_t *buffer);

  // serialize string into buffer
  // @param buffer : buffer which stores serialized string
  // @param input : string to serialize to buffer
  // @return : true if successful, false otherwise
  virtual bool SerializeString(uint8_t *buffer, const wchar_t *input);

  // deserializes instance
  // @param : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Deserialize(const uint8_t *buffer);

  // deserializes string from buffer
  // @param buffer : buffer which stores serialized string
  // @param output : place where deserialized string will be placed
  // @return : true if successful, false otherwise
  virtual bool DeserializeString(const uint8_t *buffer, wchar_t **output);

};

#endif