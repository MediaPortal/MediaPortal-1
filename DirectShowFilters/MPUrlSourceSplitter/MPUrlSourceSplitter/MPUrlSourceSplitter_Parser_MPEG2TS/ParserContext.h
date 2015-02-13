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

#ifndef __PARSER_CONTEXT_DEFINED
#define __PARSER_CONTEXT_DEFINED

#include "Flags.h"
#include "Parser.h"
#include "SectionContext.h"

#define PARSER_CONTEXT_FLAG_NONE                                      FLAGS_NONE

#define PARSER_CONTEXT_FLAG_LAST                                      (FLAGS_LAST + 0)

class CParserContext : public CFlags
{
public:
  CParserContext(HRESULT *reslt);
  virtual ~CParserContext(void);

  /* get methods */

  // gets parser associated with parser context
  // @return : parser or NULL if no parser
  virtual CParser *GetParser(void);

  // gets section context associated with parser context
  // @return : section context or NULL if no section context
  virtual CSectionContext *GetSectionContext(void);

  /* set methods */

  /* other methods */

  // clears current parser context instance to default state
  virtual void Clear(void);

  // free section context from using
  // it is not released from memory
  virtual void FreeSectionContext(void);

  // creates new section context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CreateSectionContext(void) = 0;

protected:
  // holds parser
  CParser *parser;
  // holds section context
  CSectionContext *sectionContext;

  /* methods */
};

#endif
