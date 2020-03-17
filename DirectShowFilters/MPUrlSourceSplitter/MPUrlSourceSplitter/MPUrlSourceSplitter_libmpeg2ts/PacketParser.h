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

#ifndef __PACKET_PARSER_DEFINED
#define __PACKET_PARSER_DEFINED

#include "Parser.h"
#include "TsPacket.h"

#define PACKET_PARSER_FLAG_NONE                                       PARSER_FLAG_NONE

#define PACKET_PARSER_FLAG_LAST                                       (PARSER_FLAG_LAST + 0)

class CPacketParser : public CParser
{
public:
  CPacketParser(HRESULT *result);
  virtual ~CPacketParser();

  /* get methods */

  /* set methods */

  /* other methods */

  // parses input MPEG2 TS packet
  // @param packet : the MPEG2 TS packet to parse
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT Parse(CTsPacket *packet);

protected:

  /* methods */
};

#endif

