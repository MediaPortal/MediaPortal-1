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

#ifndef __CONDITIONAL_ACCESS_PARSER_DEFINED
#define __CONDITIONAL_ACCESS_PARSER_DEFINED

#include "SectionPayloadParser.h"
#include "ConditionalAccessSection.h"

#define CONDITIONAL_ACCESS_PARSER_FLAG_NONE                           PARSER_FLAG_NONE

#define CONDITIONAL_ACCESS_PARSER_FLAG_SECTION_FOUND                  (1 << (PARSER_FLAG_LAST + 0))

#define CONDITIONAL_ACCESS_PARSER_FLAG_LAST                           (PARSER_FLAG_LAST + 1)

#define CONDITIONAL_ACCESS_PARSER_PSI_PACKET_PID                      0x0001

class CConditionalAccessParser : public CSectionPayloadParser
{
public:
  CConditionalAccessParser(HRESULT *result);
  virtual ~CConditionalAccessParser(void);

  /* get methods */

  // gets current conditional access section
  // @return : current conditional access section
  CConditionalAccessSection *GetConditionalAccessSection(void);

  // gets conditional access section parse result
  // @return :
  //  S_OK                                                        : complete conditional access section
  //  S_FALSE                                                     : incomplete conditional access section
  //  E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION  : section is empty and PSI packet with section data
  //  E_MPEG2TS_INCOMPLETE_SECTION                                : section is incomplete
  //  E_MPEG2TS_SECTION_INVALID_CRC32                             : invalid section CRC32 (corrupted section)
  //  other error code                                            : another error
  HRESULT GetConditionalAccessSectionParseResult(void);

  /* set methods */

  /* other methods */

  // tests if conditional access section is found
  // @return : true if section is found, false otherwise
  bool IsSectionFound(void);

  // parses section payload for section
  // @param sectionPayload : the section payload to parse
  // @return :
  //  S_OK                                                        : complete conditional access section
  //  S_FALSE                                                     : incomplete conditional access section
  //  E_FAIL                                                      : not PSI packet or PSI packet PID not CONDITIONAL_ACCESS_PARSER_PSI_PACKET_PID
  //  E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION  : section is empty and PSI packet with section data
  //  E_MPEG2TS_INCOMPLETE_SECTION                                : section is incomplete
  //  E_MPEG2TS_SECTION_INVALID_CRC32                             : invalid section CRC32 (corrupted section)
  //  other error code                                            : another error
  virtual HRESULT Parse(CSectionPayload *sectionPayload);

  // clears instance to its default state
  virtual void Clear(void);

protected:

  // holds last conditional access section parse result
  HRESULT conditionalAccessSectionResult;

  // holds current (maybe incomplete) conditional access section
  CConditionalAccessSection *currentSection;

  /* methods */
};

#endif
