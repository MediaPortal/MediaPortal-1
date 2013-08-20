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

#ifndef __PROTOCOL_VERSION_DEFINED
#define __PROTOCOL_VERSION_DEFINED

#include "SessionTag.h"

#define TAG_PROTOCOL_VERSION                                L"v"

#define PROTOCOL_VERSION_DEFAULT                            0

class CProtocolVersion : public CSessionTag
{
public:
  // initializes a new instance of CProtocolVersion class
  CProtocolVersion(void);
  virtual ~CProtocolVersion(void);

  /* get methods */

  // gets protocol version
  // @return : protocol version
  virtual unsigned int GetProtocolVersion(void);

  /* set methods */

  // sets protocol version
  // @param protocolVersion : the protocol version to set
  virtual void SetProtocolVersion(unsigned int protocolVersion);

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds protocol version
  unsigned int protocolVersion;
};

#endif