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

#ifndef __FULL_BOX_DEFINED
#define __FULL_BOX_DEFINED

#include "box.h"

#define FULL_BOX_DATA_SIZE                                                    4

#define FULL_BOX_HEADER_LENGTH                                                BOX_HEADER_LENGTH + FULL_BOX_DATA_SIZE
#define FULL_BOX_HEADER_LENGTH_SIZE64                                         BOX_HEADER_LENGTH_SIZE64 + FULL_BOX_DATA_SIZE

#define FULL_BOX_FLAG_NONE                                                    BOX_FLAG_NONE

#define FULL_BOX_FLAG_LAST                                                    (BOX_FLAG_LAST + 0)

class CFullBox : public CBox
{
public:
  // initializes a new instance of CFullBox class
  CFullBox(HRESULT *result);

  // destructor
  virtual ~CFullBox(void);

  /* get methods */

  // gets version of this format of the box
  // @return : version of this format of the box
  virtual uint8_t GetVersion(void);

  // gets map of box flags (flags in box, they are differerent from flags from CBox class)
  // @return : map of box flags
  virtual uint32_t GetBoxFlags(void);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  /* set methods */

  // sets version of this format of the box
  // @param version : version of this format of the box to set
  virtual void SetVersion(uint8_t version);

  // sets map of box flags (flags in box, they are differerent from flags from CBox class)
  // @param flags : map of box flags to set
  virtual void SetBoxFlags(uint32_t flags);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:
  // stores version of this format of the box
  uint8_t version;
  // stores map of box flags (used only lower 24 bits)
  uint32_t boxFlags;

  // gets whole box size
  // method is called to determine whole box size for storing box into buffer
  // @return : size of box 
  virtual uint64_t GetBoxSize(void);
  
  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed (added to buffer)
  // @return : number of bytes stored into buffer, 0 if error
  virtual uint32_t GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes);
};

#endif