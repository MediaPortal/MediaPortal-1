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

#ifndef __HANDLER_BOX_DEFINED
#define __HANDLER_BOX_DEFINED

#include "FullBox.h"

#define HANDLER_BOX_TYPE                                              L"hdlr"

#define HANDLER_TYPE_VIDEO                                            0x76696465
#define HANDLER_TYPE_AUDIO                                            0x736F756E
#define HANDLER_TYPE_HINT                                             0x68696E74
#define HANDLER_TYPE_METADATA                                         0x6D657461

#define HANDLER_BOX_FLAG_NONE                                         FULL_BOX_FLAG_NONE

#define HANDLER_BOX_FLAG_LAST                                         (FULL_BOX_FLAG_LAST + 0)

class CHandlerBox :
  public CFullBox
{
public:
  // initializes a new instance of CHandlerBox class
  CHandlerBox(HRESULT *result);

  // destructor
  virtual ~CHandlerBox(void);

  /* get methods */

  // gets handler type
  // when present in a media box, is an integer containing one of the following values, or a value from a derived specification:
  // ‘vide’ Video track
  // ‘soun’ Audio track
  // ‘hint’ Hint track
  // ‘meta’ Timed Metadata track
  // when present in a meta box, contains an appropriate value to indicate the format of the meta box contents
  // the value ‘null’ can be used in the primary meta box to indicate that it is merely being used to hold resources
  // @return : handler type
  virtual uint32_t GetHandlerType(void);

  // gets human-readable name for the track type
  // @return : human-readable name for the track type
  virtual const wchar_t *GetName(void);

  /* set methods */

  // sets handler type
  // when present in a media box, is an integer containing one of the following values, or a value from a derived specification:
  // ‘vide’ Video track
  // ‘soun’ Audio track
  // ‘hint’ Hint track
  // ‘meta’ Timed Metadata track
  // when present in a meta box, contains an appropriate value to indicate the format of the meta box contents
  // the value ‘null’ can be used in the primary meta box to indicate that it is merely being used to hold resources
  // @param handlerType : handler type to set
  virtual void SetHandlerType(uint32_t handlerType);

  // sets human-readable name for the track type
  // @param name : human-readable name for the track type to set
  // @return : true if successful, false otherwise
  virtual bool SetName(const wchar_t *name);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:

  // when present in a media box, is an integer containing one of the following values, or a value from a derived specification:
  //  ‘vide’ Video track
  // ‘soun’ Audio track
  // ‘hint’ Hint track
  // ‘meta’ Timed Metadata track
  // when present in a meta box, contains an appropriate value to indicate the format of the meta box contents
  // the value ‘null’ can be used in the primary meta box to indicate that it is merely being used to hold resources
  uint32_t handlerType;

  wchar_t *name;

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