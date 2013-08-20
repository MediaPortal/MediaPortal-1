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

#ifndef __MSHS_CUSTOM_ATTRIBUTE_DEFINED
#define __MSHS_CUSTOM_ATTRIBUTE_DEFINED

#include "Serializable.h"

class CMSHSCustomAttribute : public CSerializable
{
public:
  // creats new instance of CMSHSCustomAttribute class
  CMSHSCustomAttribute(void);

  // desctructor
  ~CMSHSCustomAttribute(void);

  /* get methods */

  // gets custom attribute name
  // @return : custom attribute name
  const wchar_t *GetName(void);

  // gets custom attribute value
  // @return : custom attribute value
  const wchar_t *GetValue(void);

  /* set methods */

  // sets custom attribute name
  // @param name : custom attribute name to set
  bool SetName(const wchar_t *name);

  // sets custom attribute value
  // @param value : custom attribute value to set
  bool SetValue(const wchar_t *value);

  /* other methods */

  // gets necessary buffer length for serializing instance
  // @return : necessary size for buffer
  virtual uint32_t GetSerializeSize(void);

  // serialize instance into buffer, buffer must be allocated before and must have necessary size
  // @param buffer : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Serialize(uint8_t *buffer);

  // deserializes instance
  // @param : buffer which stores serialized instance
  // @return : true if successful, false otherwise
  virtual bool Deserialize(const uint8_t *buffer);

private:

  wchar_t *name;

  wchar_t *value;

};

#endif