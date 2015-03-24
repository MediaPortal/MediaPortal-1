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

#ifndef __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_DEFINED
#define __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_DEFINED

#include "Parser.h"
#include "TransportStreamProgramMapSection.h"
#include "ProgramAssociationSection.h"

#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_NONE                 PARSER_FLAG_NONE

#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_SECTION_FOUND        (1 << (PARSER_FLAG_LAST + 0))

#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_FLAG_LAST                 (PARSER_FLAG_LAST + 1)

#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_PID_NOT_DEFINED           0xFFFF

class CTransportStreamProgramMapParser : public CParser
{
public:
  CTransportStreamProgramMapParser(HRESULT *result, uint16_t pid);
  virtual ~CTransportStreamProgramMapParser(void);

  /* get methods */

  // gets current transport stream program map section
  // @return : current transport stream program map section
  CTransportStreamProgramMapSection *GetTransportStreamProgramMapSection(void);

  // gets program association section parse result
  // @return :
  //  S_OK                                                        : complete program association section
  //  S_FALSE                                                     : incomplete program association section
  //  E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION  : section is empty and PSI packet with section data
  //  E_MPEG2TS_INCOMPLETE_SECTION                                : section is incomplete
  //  E_MPEG2TS_SECTION_INVALID_CRC32                             : invalid section CRC32 (corrupted section)
  //  other error code                                            : another error
  HRESULT GetTransportStreamProgramMapSectionParseResult(void);

  // gets transport stream program map section PSI packet PID
  // @return : transport stream program map section PSI packet PID
  uint16_t GetTransportStreamProgramMapSectionPID(void);

  /* set methods */

  /* other methods */

  // tests if transport stream program map section is found
  // @return : true if section is found, false otherwise
  bool IsSectionFound(void);

  // parses input MPEG2 TS packet
  // @param packet : the MPEG2 TS packet to parse
  // @return :
  //  S_OK                                                        : complete program association section
  //  S_FALSE                                                     : incomplete program association section
  //  E_FAIL                                                      : not PSI packet or PSI packet PID not transport stream program map section PID
  //  E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION  : section is empty and PSI packet with section data
  //  E_MPEG2TS_INCOMPLETE_SECTION                                : section is incomplete
  //  E_MPEG2TS_SECTION_INVALID_CRC32                             : invalid section CRC32 (corrupted section)
  //  other error code                                            : another error
  virtual HRESULT Parse(CTsPacket *packet);

  // clears instance to its default state
  virtual void Clear(void);

protected:
  // holds transport stream program map section PID
  uint16_t transportStreamProgramMapSectionPID;
  // holds last transport stream program map section parse result
  HRESULT transportStreamProgramMapSectionResult;
  // holds current (maybe incomplete) transport stream program map section
  CTransportStreamProgramMapSection *currentSection;

  /* methods */
};

#endif