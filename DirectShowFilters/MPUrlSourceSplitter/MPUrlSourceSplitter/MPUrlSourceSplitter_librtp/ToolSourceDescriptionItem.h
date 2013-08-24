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

#ifndef __TOOL_SOURCE_DESCRIPTION_ITEM_DEFINED
#define __TOOL_SOURCE_DESCRIPTION_ITEM_DEFINED

#include "SourceDescriptionItem.h"

#define TOOL_SOURCE_DESCRIPTION_ITEM_TYPE                             0x06

class CToolSourceDescriptionItem : public CSourceDescriptionItem
{
public:
  // initializes a new instance of CToolSourceDescriptionItem class
  CToolSourceDescriptionItem(void);
  virtual ~CToolSourceDescriptionItem(void);

  /* get methods */

  // gets tool source description item type
  // @return : tool source description item type
  virtual unsigned int GetType(void);

  // gets tool source description item size
  // @return : tool source description item size
  virtual unsigned int GetSize(void);

  // get whole tool source description item into buffer
  // @param buffer : the buffer to store tool source description item
  // @param length : the length of buffer
  // @return : true if successful, false otherwise
  virtual bool GetSourceDescriptionItem(unsigned char *buffer, unsigned int length);

  // gets tool
  // @return : tool or NULL if error
  virtual const wchar_t *GetTool(void);

  /* set methods */

  // sets tool
  // @param : the tool to set
  // @return : true if successful, false otherwise
  virtual bool SetTool(const wchar_t *tool);

  /* other methods */

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with source description item data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // holds tool
  wchar_t *tool;
};

#endif