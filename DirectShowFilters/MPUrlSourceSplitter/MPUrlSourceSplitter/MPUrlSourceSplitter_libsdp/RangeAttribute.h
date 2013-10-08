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

#ifndef __RANGE_ATTRIBUTE_DEFINED
#define __RANGE_ATTRIBUTE_DEFINED

#include "Attribute.h"

#define TAG_ATTRIBUTE_RANGE                                 L"range"

#define TAG_ATTRIBUTE_INSTANCE_RANGE                        L"a_range"

class CRangeAttribute : public CAttribute
{
public:
  // initializes a new instance of CRangeAttribute class
  CRangeAttribute(void);
  virtual ~CRangeAttribute(void);

  /* get methods */

  // gets range specification
  // @return : range specification
  virtual const wchar_t *GetRangeSpecification(void);

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

  // holds control URL
  wchar_t *rangeSpecification;
};

#endif