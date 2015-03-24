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

#ifndef __DATA_ENTRY_BOX_DEFINED
#define __DATA_ENTRY_BOX_DEFINED

#include "FullBox.h"

// means that the media data is in the same file as the Movie Box containing this data reference
#define FLAGS_SELF_CONTAINED                                          0x00000001

#define DATA_ENTRY_BOX_FLAG_NONE                                      FULL_BOX_FLAG_NONE

#define DATA_ENTRY_BOX_FLAG_LAST                                      (FULL_BOX_FLAG_LAST + 0)

class CDataEntryBox :
  public CFullBox
{
public:
  // initializes a new instance of CDataEntryBox class
  CDataEntryBox(HRESULT *result);

  // destructor
  virtual ~CDataEntryBox(void);

  /* get methods */

  // tests if the media data is in the same file as the Movie Box containing this data reference
  // @return : true if the media data is in the same file as the Movie Box containing this data reference, false otherwise
  virtual bool IsSelfContained(void);

  /* set methods */

  // sets if the media data is in the same file as the Movie Box containing this data reference
  // @param selfContained : true if the media data is in the same file as the Movie Box containing this data reference, false otherwise
  virtual void SetSelfContained(bool selfContained);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:

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