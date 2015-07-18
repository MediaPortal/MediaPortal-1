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
#include "Section.h"

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

  /* set methods */

  // sets section as known section
  // @param section : the section to set as known
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetKnownSection(CSection *section) = 0;

  /* other methods */

  // clears current parser context instance to default state
  virtual void Clear(void);

  // check if section is known
  // @param section : the section to check
  // @return : true if section is known, false otherwise
  virtual bool IsKnownSection(CSection *section) = 0;

protected:
  // holds parser
  CParser *parser;

  /* methods */
};

#endif
