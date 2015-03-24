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

#ifndef __PROGRAM_ASSOCIATION_PARSER_DEFINED
#define __PROGRAM_ASSOCIATION_PARSER_DEFINED

#include "Parser.h"
#include "ProgramAssociationSection.h"

#define PROGRAM_ASSOCIATION_PARSER_FLAG_NONE                          PARSER_FLAG_NONE

#define PROGRAM_ASSOCIATION_PARSER_FLAG_SECTION_FOUND                 (1 << (PARSER_FLAG_LAST + 0))

#define PROGRAM_ASSOCIATION_PARSER_FLAG_LAST                          (PARSER_FLAG_LAST + 1)

#define PROGRAM_ASSOCIATION_PARSER_PSI_PACKET_PID                     0x0000

class CProgramAssociationParser : public CParser
{
public:
  CProgramAssociationParser(HRESULT *result);
  virtual ~CProgramAssociationParser(void);

  /* get methods */

  // gets current program association section
  // @return : current program association section
  CProgramAssociationSection *GetProgramAssociationSection(void);

  // gets program association section parse result
  // @return :
  //  S_OK                                                        : complete program association section
  //  S_FALSE                                                     : incomplete program association section
  //  E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION  : section is empty and PSI packet with section data
  //  E_MPEG2TS_INCOMPLETE_SECTION                                : section is incomplete
  //  E_MPEG2TS_SECTION_INVALID_CRC32                             : invalid section CRC32 (corrupted section)
  //  other error code                                            : another error
  HRESULT GetProgramAssociationSectionParseResult(void);

  /* set methods */

  /* other methods */

  // tests if program association section is found
  // @return : true if section is found, false otherwise
  bool IsSectionFound(void);

  // parses input MPEG2 TS packet
  // @param packet : the MPEG2 TS packet to parse
  // @return :
  //  S_OK                                                        : complete program association section
  //  S_FALSE                                                     : incomplete program association section
  //  E_FAIL                                                      : not PSI packet or PSI packet PID not PROGRAM_ASSOCIATION_PARSER_PSI_PACKET_PID
  //  E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION  : section is empty and PSI packet with section data
  //  E_MPEG2TS_INCOMPLETE_SECTION                                : section is incomplete
  //  E_MPEG2TS_SECTION_INVALID_CRC32                             : invalid section CRC32 (corrupted section)
  //  other error code                                            : another error
  virtual HRESULT Parse(CTsPacket *packet);

  // clears instance to its default state
  virtual void Clear(void);

protected:

  // holds last program association section parse result
  HRESULT programAssociationSectionResult;

  // holds current (maybe incomplete) program association section
  CProgramAssociationSection *currentSection;

  /* methods */
};

#endif