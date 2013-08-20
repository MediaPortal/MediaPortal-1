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

#ifndef __BOX_DEFINED
#define __BOX_DEFINED

#include <stdint.h>

class CBoxCollection;

class CBox
{
public:
  // initializes a new instance of CBox class
  CBox(void);

  // destructor
  virtual ~CBox(void);

  /* get methods */

  // gets box size
  // @return : box size
  virtual uint64_t GetSize(void);

  // gets box type
  // @return : box type or NULL if error
  virtual const wchar_t *GetType(void);

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  // gets additional boxes stored in this box
  // @return : additional boxes stored in this box
  virtual CBoxCollection *GetBoxes(void);

  /* set methods */

  /* other methods */

  // tests if instance has valid box
  virtual bool IsBox(void);

  // gets if box buffer is successfully parsed
  // @return : true if successfully parsed, false otherwise
  virtual bool IsParsed(void);

  // tests if box size is bigger than UINT_MAX
  // @return : true if box size is bigger than UINT_MAX, false otherwise
  virtual bool IsBigSize(void);

  // tests if box size if unspecified (box content extends to the end of the file)
  // @return : true if box size is unspecifed, false otherwise
  virtual bool IsSizeUnspecifed(void);

  // tests if box has extended header (extra 8 bytes for int(64) size)
  // @return : true if box has extended header, false otherwise
  virtual bool HasExtendedHeader(void);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const unsigned char *buffer, uint32_t length);

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

  // tests if box is specified type
  // @param type : the requested box type
  // @return : true if box is specified type, false otherwise
  virtual bool IsType(const wchar_t *type);

  // reset box size to acquire correct box size
  virtual void ResetSize(void);

protected:
  // stores the length of box
  uint64_t length;
  // stores if data were successfully parsed
  bool parsed;
  // stores box type
  wchar_t *type;
  // stores if box has extended header
  bool hasExtendedHeader;
  // stores if box has unspecified size
  bool hasUnspecifiedSize;

  // stores additional boxes stored in this box
  CBoxCollection *boxes;

  // gets whole box size
  // method is called to determine whole box size for storing box into buffer
  // @return : size of box 
  virtual uint64_t GetBoxSize(void);

  // gets Unicode string from buffer from specified position
  // @param buffer : the buffer to read UTF-8 string
  // @param length : the length of buffer
  // @param startPosition : the position within buffer to start reading UTF-8 string
  // @param output : reference to Unicode buffer where result will be stored
  // @param positionAfterString : reference to variable where will be stored position after null terminating character of UTF-8 string
  // @return : S_OK if successful, E_POINTER if buffer, output or positionAfterString is NULL, HRESULT_FROM_WIN32(ERROR_INVALID_DATA) if not enough data in buffer, E_OUTOFMEMORY if not enough memory for results
  HRESULT GetString(const uint8_t *buffer, uint32_t length, uint32_t startPosition, wchar_t **output, uint32_t *positionAfterString);

  // gets Unicode string from buffer from specified position
  // @param buffer : the buffer to read UTF-8 string
  // @param length : the length of buffer
  // @param startPosition : the position within buffer to start reading UTF-8 string
  // @param output : reference to Unicode buffer where result will be stored
  // @param positionAfterString : reference to variable where will be stored position after null terminating character of UTF-8 string
  // @param maxLength : the maximum length of string
  // @return : S_OK if successful, E_POINTER if buffer, output or positionAfterString is NULL, HRESULT_FROM_WIN32(ERROR_INVALID_DATA) if not enough data in buffer, E_OUTOFMEMORY if not enough memory for results
  HRESULT GetString(const uint8_t *buffer, uint32_t length, uint32_t startPosition, wchar_t **output, uint32_t *positionAfterString, uint32_t maxLength);

  // sets Unicode string into buffer
  // @param buffer : the buffer to write UTF-8 string
  // @param length : the length of buffer
  // @param input : reference to Unicode string which will be stored into buffer
  // @return : number of bytes written into buffer (including NULL terminating character), 0 is error when input is not NULL
  uint32_t SetString(uint8_t *buffer, uint32_t length, const wchar_t *input);

  // gets Unicode string necessary size to store into buffer
  // @param input : reference to Unicode string which will be stored into buffer
  // @return : number of bytes necessary in buffer (including NULL terminating character), 0 is error when input is not NULL
  uint32_t GetStringSize(const wchar_t *input);

  // process remaining data in box as boxes
  // @param buffer : the buffer to process
  // @param length : the length of buffer
  // @param position : the position within buffer to start processing
  // @return : true if successful, false otherwise
  virtual bool ProcessAdditionalBoxes(const uint8_t *buffer, uint32_t length, uint32_t position);

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

  // get remaining boxes into buffer
  // @param buffer : the buffer to store remaining boxes
  // @param length : the length of buffer
  // @return : number of bytes stored into buffer, 0 is error only if there are boxes
  virtual uint32_t GetAdditionalBoxes(uint8_t *buffer, uint32_t length);
};

#endif