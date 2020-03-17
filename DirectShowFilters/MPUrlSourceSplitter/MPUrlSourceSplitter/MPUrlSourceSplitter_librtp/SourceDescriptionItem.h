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

#ifndef __SOURCE_DESCRIPTION_ITEM_DEFINED
#define __SOURCE_DESCRIPTION_ITEM_DEFINED

#define SOURCE_DESCRIPTION_ITEM_HEADER                                2       // source descripition item header size

class CSourceDescriptionItem
{
public:
  // initialize a new instance of CSourceDescriptionItem class
  CSourceDescriptionItem(HRESULT *result);
  virtual ~CSourceDescriptionItem(void);

  /* get methods */

  // gets source description item type
  // @return : source description item type or UINT_MAX if not specified or error
  virtual unsigned int GetType(void) = 0;

  // gets source description item size
  // @return : source description item size
  virtual unsigned int GetSize(void);

  // get whole source description item into buffer
  // @param buffer : the buffer to store source description item
  // @param length : the length of buffer
  // @return : true if successful, false otherwise
  virtual bool GetSourceDescriptionItem(unsigned char *buffer, unsigned int length);

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

  // holds source description item type
  unsigned int type;

  // holds payload
  unsigned char *payload;

  // holds payload size
  unsigned int payloadSize;
};

#endif