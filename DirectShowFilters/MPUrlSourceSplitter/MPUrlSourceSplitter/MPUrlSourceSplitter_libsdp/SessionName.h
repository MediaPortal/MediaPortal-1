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

#ifndef __SESSION_NAME_DEFINED
#define __SESSION_NAME_DEFINED

#include "SessionTag.h"

#define TAG_SESSION_NAME                                    L"s"

class CSessionName : public CSessionTag
{
public:
  // initializes a new instance of CSessionName class
  CSessionName(HRESULT *result);
  ~CSessionName(void);

  /* get methods */

  // gets session name
  // @return : session name
  const wchar_t *GetSessionName(void);

  /* set methods */

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds session name
  wchar_t *sessionName;
};

#endif