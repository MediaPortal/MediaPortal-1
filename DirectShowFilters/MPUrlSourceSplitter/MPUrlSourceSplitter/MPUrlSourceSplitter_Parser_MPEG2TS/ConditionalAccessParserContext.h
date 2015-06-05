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

#ifndef __CONDITIONAL_ACCESS_PARSER_CONTEXT_DEFINED
#define __CONDITIONAL_ACCESS_PARSER_CONTEXT_DEFINED

#include "ParserContext.h"
#include "ConditionalAccessParser.h"
#include "ConditionalAccessSectionContext.h"

#define CONDITIONAL_ACCESS_PARSER_CONTEXT_FLAG_NONE                   PARSER_CONTEXT_FLAG_NONE

#define CONDITIONAL_ACCESS_PARSER_CONTEXT_FLAG_LAST                   (PARSER_CONTEXT_FLAG_LAST + 0)

class CConditionalAccessParserContext : public CParserContext
{
public:
  CConditionalAccessParserContext(HRESULT *result);
  virtual ~CConditionalAccessParserContext(void);

  /* get methods */

  // gets parser associated with parser context
  // @return : parser or NULL if no parser
  virtual CConditionalAccessParser *GetParser(void);

  // gets section context associated with parser context
  // @return : section context or NULL if no section context
  virtual CConditionalAccessSectionContext *GetSectionContext(void);

  /* set methods */

  /* other methods */

  // creates new section context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CreateSectionContext(void);

protected:

  /* methods */
};

#endif
