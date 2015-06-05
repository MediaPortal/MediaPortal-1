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
#include "TransportStreamProgramMapSectionContext.h"
#include "ProgramElementCollection.h"

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

  // gets section context associated with parser context
  // @return : section context or NULL if no section context
  virtual CTransportStreamProgramMapSectionContext *GetSectionContext(void);

  // gets collection of program elements to leave in transport stream program map
  // @return : collection of program elements to leave in transport stream program map
  virtual CProgramElementCollection *GetLeaveProgramElements(void);

  /* set methods */

  // sets filter program elements flag
  // @param filterProgramElements : true if filter program elements, false otherwise
  virtual void SetFilterProgramElements(bool filterProgramElements);

  /* other methods */

  // tests if filter program elements flag is set
  // @return : true if filter program elements flag is set, false otherwise
  virtual bool IsFilterProgramElements(void);

  // creates new section context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CreateSectionContext(void);

protected:
  // holds program elements to leave in transport stream program map
  CProgramElementCollection *leaveProgramElements;

  /* methods */
};

#endif
