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

#ifndef __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_DEFINED
#define __TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_DEFINED

#include "ParserContext.h"
#include "TransportStreamProgramMapParser.h"
#include "ProgramElementCollection.h"
#include "TransportStreamProgramMapParserKnownSectionContextCollection.h"

#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_NONE                       PARSER_CONTEXT_FLAG_NONE

#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_FILTER_PROGRAM_ELEMENTS    (1 << (PARSER_CONTEXT_FLAG_LAST + 0))

#define TRANSPORT_STREAM_PROGRAM_MAP_PARSER_CONTEXT_FLAG_LAST                       (PARSER_CONTEXT_FLAG_LAST + 1)

class CTransportStreamProgramMapParserContext : public CParserContext
{
public:
  CTransportStreamProgramMapParserContext(HRESULT *result, uint16_t pid);
  virtual ~CTransportStreamProgramMapParserContext(void);

  /* get methods */

  // gets parser associated with parser context
  // @return : parser or NULL if no parser
  virtual CTransportStreamProgramMapParser *GetParser(void);

  // gets collection of program elements to leave in transport stream program map
  // @return : collection of program elements to leave in transport stream program map
  virtual CProgramElementCollection *GetLeaveProgramElements(void);

  /* set methods */

  // sets section as known section
  // @param section : the section to set as known
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetKnownSection(CSection *section);

  // sets filter program elements flag
  // @param filterProgramElements : true if filter program elements, false otherwise
  virtual void SetFilterProgramElements(bool filterProgramElements);

  /* other methods */

  // clears current parser context instance to default state
  virtual void Clear(void);

  // tests if filter program elements flag is set
  // @return : true if filter program elements flag is set, false otherwise
  virtual bool IsFilterProgramElements(void);

  // check if section is known
  // @param section : the section to check
  // @return : true if section is known, false otherwise
  virtual bool IsKnownSection(CSection *section);

protected:
  // holds program elements to leave in transport stream program map
  CProgramElementCollection *leaveProgramElements;
  // holds known sections
  CTransportStreamProgramMapParserKnownSectionContextCollection *knownSections;

  /* methods */
};

#endif
