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

#ifndef __CANONICAL_END_POINT_SOURCE_DESCRIPTION_ITEM_DEFINED
#define __CANONICAL_END_POINT_SOURCE_DESCRIPTION_ITEM_DEFINED

#include "SourceDescriptionItem.h"

#define CANONICAL_END_POINT_SOURCE_DESCRIPTION_ITEM_TYPE              0x01

class CCanonicalEndPointSourceDescriptionItem : public CSourceDescriptionItem
{
public:
  // initializes a new instance of CCanonicalEndPointSourceDescriptionItem class
  CCanonicalEndPointSourceDescriptionItem(void);
  virtual ~CCanonicalEndPointSourceDescriptionItem(void);

  /* get methods */

  // gets canonical end point source description item type
  // @return : canonical end point source description item type
  virtual unsigned int GetType(void);

  // gets canonical end point source description item size
  // @return : canonical end point source description item size
  virtual unsigned int GetSize(void);

  // get whole canonical end point source description item into buffer
  // @param buffer : the buffer to store canonical end point source description item
  // @param length : the length of buffer
  // @return : true if successful, false otherwise
  virtual bool GetSourceDescriptionItem(unsigned char *buffer, unsigned int length);

  // gets canonical end-point name
  // @return : canonical end-point name or NULL if error
  virtual const wchar_t *GetEndPointName(void);

  /* set methods */

  // sets canonical end-point name
  // @param endPointName : the canonical end-point name to set
  // @return : true if successful, false otherwise
  virtual bool SetEndPointName(const wchar_t *endPointName);

  /* other methods */

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with source description item data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // holds end-point name
  wchar_t *endPointName;
};

#endif