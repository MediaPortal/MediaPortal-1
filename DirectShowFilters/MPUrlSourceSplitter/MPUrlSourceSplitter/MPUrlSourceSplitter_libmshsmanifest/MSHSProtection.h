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

#ifndef __MSHS_PROTECTION_DEFINED
#define __MSHS_PROTECTION_DEFINED

#include "Serializable.h"

#include <stdint.h>

class CMSHSProtection :
  public CSerializable
{
public:
  // creats new instance of CMSHSProtection class
  CMSHSProtection(void);

  // desctructor
  ~CMSHSProtection(void);

  /* get methods */

  // gets system ID
  // UUID that uniquely identifies the Content Protection System
  // @return : system ID
  GUID GetSystemId(void);

  // gets protection content
  // opaque data that the Content Protection System identified in the GetSystemID() can use to enable playback
  // for authorized users, encoded using BASE64 encoding
  // @return : opaque protection content data
  const wchar_t *GetContent(void);

  /* set methods */

  // sets system ID - UUID that uniquely identifies the Content Protection System
  // @param systemId : UUID that uniquely identifies the Content Protection System
  void SetSystemId(GUID systemId);

  // sets protection content
  // @param content : opaque protection content data in BASE64 encoding
  bool SetContent(const wchar_t *content);

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

  GUID systemId;

  wchar_t *content;
};

#endif