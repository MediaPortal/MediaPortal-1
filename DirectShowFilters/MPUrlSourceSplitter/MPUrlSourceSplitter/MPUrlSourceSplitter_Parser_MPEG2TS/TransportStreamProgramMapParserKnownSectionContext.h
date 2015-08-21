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

#ifndef __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_KNOWN_SECTION_CONTEXT_DEFINED
#define __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_KNOWN_SECTION_CONTEXT_DEFINED

class CTransportStreamProgramMapParserKnownSectionContext
{
public:
  CTransportStreamProgramMapParserKnownSectionContext(HRESULT *result, uint16_t programNumber, unsigned int crc32);
  ~CTransportStreamProgramMapParserKnownSectionContext();

  /* get methods */

  // gets program number
  // @return : program number
  uint16_t GetProgramNumber(void);

  // gets last transport stream program map section CRC32
  // @return : CRC32 or SECTION_CRC32_UNDEFINED if not set
  unsigned int GetCrc32(void);

  /* set methods */

  // sets last transport stream program map section CRC32
  // @param crc32 : CRC32 to set
  void SetCrc32(unsigned int crc32);

  /* other methods */

protected:
  // holds program number
  uint16_t programNumber;
  // holds last CRC32 for transport stream program map section with program number
  unsigned int crc32;

  /* methods */
};

#endif