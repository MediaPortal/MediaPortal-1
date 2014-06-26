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

#ifndef __ATTRIBUTE_DEFINED
#define __ATTRIBUTE_DEFINED

#include "SessionTag.h"

#define TAG_ATTRIBUTE                                                 L"a"

#define TAG_ATTRIBUTE_UNSPECIFIED                                     L"au"

#define ATTRIBUTE_FLAG_NONE                                           SESSION_TAG_FLAG_NONE

#define ATTRIBUTE_FLAG_LAST                                           (SESSION_TAG_FLAG_LAST + 0)

class CAttribute : public CSessionTag
{
public:
  // intializes a new instance of CAttribute class
  CAttribute(HRESULT *result);
  virtual  ~CAttribute(void);

  /* get methods */

  // gets attribute
  // @return : attribute
  virtual const wchar_t *GetAttribute(void);

  // gets attribute value
  // @ return : attribute value or NULL if not specified
  virtual const wchar_t *GetValue(void);

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

  // holds attribute
  wchar_t *attribute;

  // holds attribute value
  wchar_t *value;
};

#endif