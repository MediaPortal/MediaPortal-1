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

#ifndef __PARSER_DEFINED
#define __PARSER_DEFINED

#include "Flags.h"

#define PARSER_FLAG_NONE                                              FLAGS_NONE

#define PARSER_FLAG_LAST                                              (FLAGS_LAST + 0)

class CParser : public CFlags
{
public:
  CParser(HRESULT *result);
  virtual ~CParser(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // parses input data for MPEG2 TS packets
  // @param buffer : the buffer to parse
  // @param length : the length of buffer to parse
  // @result : positive values if successful (the length of processed data), error code otherwise
  virtual HRESULT Parse(unsigned char *buffer, unsigned int length) = 0;

  // clears instance to its default state
  virtual void Clear(void);

protected:

  /* methods */
};

#endif