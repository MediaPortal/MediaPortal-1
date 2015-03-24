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

#ifndef __HINT_MEDIA_HEADER_BOX_DEFINED
#define __HINT_MEDIA_HEADER_BOX_DEFINED

#include "FullBox.h"

#define HINT_MEDIA_HEADER_BOX_TYPE                                    L"hmhd"

#define HINT_MEDIA_HEADER_BOX_FLAG_NONE                               FULL_BOX_FLAG_NONE

#define HINT_MEDIA_HEADER_BOX_FLAG_LAST                               (FULL_BOX_FLAG_LAST + 0)

class CHintMediaHeaderBox :
  public CFullBox
{
public:
  // initializes a new instance of CHintMediaHeaderBox class
  CHintMediaHeaderBox(HRESULT *result);

  // destructor
  virtual ~CHintMediaHeaderBox(void);

  /* get methods */

  // gets the size in bytes of the largest PDU in this (hint) stream
  // @return : the size in bytes of the largest PDU in this (hint) stream
  virtual uint16_t GetMaxPDUSize(void);

  // gets the average size of a PDU over the entire presentation
  // @return : the average size of a PDU over the entire presentation
  virtual uint16_t GetAveragePDUSize(void);

  // gets the maximum rate in bits/second over any window of one second
  // @return : the maximum rate in bits/second over any window of one second
  virtual uint32_t GetMaxBitrate(void);

  // gets the average rate in bits/second over the entire presentation
  // @return : the average rate in bits/second over the entire presentation
  virtual uint32_t GetAverageBitrate(void);

  /* set methods */

  // sets the size in bytes of the largest PDU in this (hint) stream
  // @param maxPDUSize : the size in bytes of the largest PDU in this (hint) stream to set
  virtual void SetMaxPDUSize(uint16_t maxPDUSize);

  // sets the average size of a PDU over the entire presentation
  // @param averagePDUSize : the average size of a PDU over the entire presentation to set
  virtual void SetAveragePDUSize(uint16_t averagePDUSize);

  // sets the maximum rate in bits/second over any window of one second
  // @param maxBitrate : the maximum rate in bits/second over any window of one second to set
  virtual void SetMaxBitrate(uint32_t maxBitrate);

  // sets the average rate in bits/second over the entire presentation
  // @param averageBitrate : the average rate in bits/second over the entire presentation to set
  virtual void SetAverageBitrate(uint32_t averageBitrate);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

protected:

  // the size in bytes of the largest PDU in this (hint) stream
  uint16_t maxPDUSize;
  // the average size of a PDU over the entire presentation
  uint16_t averagePDUSize;
  // the maximum rate in bits/second over any window of one second
  uint32_t maxBitrate;
  // the average rate in bits/second over the entire presentation
  uint32_t averageBitrate;

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