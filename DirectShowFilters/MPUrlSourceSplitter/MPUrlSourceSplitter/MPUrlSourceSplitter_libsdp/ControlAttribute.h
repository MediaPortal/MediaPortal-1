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

#ifndef __CONTROL_ATTRIBUTE_DEFINED
#define __CONTROL_ATTRIBUTE_DEFINED

#include "Attribute.h"

#define TAG_ATTRIBUTE_CONTROL                               L"control"

#define TAG_ATTRIBUTE_INSTANCE_CONTROL                      L"a_control"

#define FLAG_CONTROL_ATTRIBUTE_NONE                         0x00000000
#define FLAG_CONTROL_ATTRIBUTE_ASTERISK                     0x00000001

#define CONTROL_ATTRIBUTE_ASTERISK                          L"*"

class CControlAttribute : public CAttribute
{
public:
  // initializes a new instance of CControlAttribute class
  CControlAttribute(void);
  virtual ~CControlAttribute(void);

  /* get methods */

  // gets control URL
  // @return : control URL
  virtual const wchar_t *GetControlUrl(void);

  /* set methods */

  /* other methods */

  // tests if control attribute is asterisk ('*')
  // @return : true if control attribute is asterisk, false otherwise
  virtual bool IsAsterisk(void);

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds various flags
  unsigned int flags;

  // holds control URL
  wchar_t *controlUrl;
};

#endif