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

#ifndef __EMAIL_SOURCE_DESCRIPTION_ITEM_DEFINED
#define __EMAIL_SOURCE_DESCRIPTION_ITEM_DEFINED

#include "SourceDescriptionItem.h"

#define EMAIL_SOURCE_DESCRIPTION_ITEM_TYPE                            0x03

class CEmailSourceDescriptionItem : public CSourceDescriptionItem
{
public:
  // initializes a new instance of CEmailSourceDescriptionItem class
  CEmailSourceDescriptionItem(void);
  virtual ~CEmailSourceDescriptionItem(void);

  /* get methods */

  // gets e-mail
  // @return : e-mail or NULL if error
  const wchar_t *GetEmail(void);

  /* set methods */

  /* other methods */

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with source description item data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // holds e-mail
  wchar_t *email;

};

#endif