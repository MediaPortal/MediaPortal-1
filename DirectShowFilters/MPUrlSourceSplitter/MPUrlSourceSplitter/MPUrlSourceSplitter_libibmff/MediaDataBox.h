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

#ifndef __MEDIA_DATA_BOX_DEFINED
#define __MEDIA_DATA_BOX_DEFINED

#include "box.h"

#define MEDIA_DATA_BOX_TYPE                                           L"mdat"

#define MEDIA_DATA_BOX_FLAG_NONE                                      BOX_FLAG_NONE

#define MEDIA_DATA_BOX_FLAG_LAST                                      (BOX_FLAG_LAST + 3)

class CMediaDataBox :
  public CBox
{
public:
  // initializes a new instance of CMediaDataBox class
  CMediaDataBox(HRESULT *result);

  // destructor
  virtual ~CMediaDataBox(void);

  /* get methods */

  // gets payload data of media data box
  // @return : payload data or NULL if error
  virtual const uint8_t *GetPayload(void);

  // gets payload size
  // @return : payload size
  virtual uint64_t GetPayloadSize(void);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  /* set methods */

  // sets payload data of media data box
  // @param buffer : buffer with payload data
  // @param length : buffer size (payload data size)
  // @return : true if successfull, false otherwise
  virtual bool SetPayload(const uint8_t *buffer, uint32_t length);

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const uint8_t *buffer, uint32_t length);

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:
  // stores playload
  uint8_t *payload;
  // stores payload size
  uint64_t payloadSize;

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