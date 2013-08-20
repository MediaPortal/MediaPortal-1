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

#ifndef __VISUAL_SAMPLE_ENTRY_BOX_DEFINED
#define __VISUAL_SAMPLE_ENTRY_BOX_DEFINED

#include "SampleEntryBox.h"
#include "FixedPointNumber.h"

class CVisualSampleEntryBox :
  public CSampleEntryBox
{
public:
  // initializes a new instance of CVisualSampleEntryBox class
  CVisualSampleEntryBox(void);

  // destructor
  virtual ~CVisualSampleEntryBox(void);

  /* get methods */

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  // gets video coding name
  // @return : coding name
  virtual const wchar_t *GetCodingName(void);

  // gets the maximum visual width of the stream described by this sample description, in pixels
  // @return : the maximum visual width of the stream described by this sample description, in pixels
  virtual uint16_t GetWidth(void);

  // gets the maximum visual height of the stream described by this sample description, in pixels
  // @return : the maximum visual height of the stream described by this sample description, in pixels
  virtual uint16_t GetHeight(void);

  // gets the resolution of the image in pixels-per-inch
  // @return : the resolution of the image in pixels-per-inch
  virtual CFixedPointNumber *GetHorizontalResolution(void);

  // gets the resolution of the image in pixels-per-inch
  // @return : the resolution of the image in pixels-per-inch
  virtual CFixedPointNumber *GetVerticalResolution(void);

  // gets how many frames of compressed video are stored in each sample
  // the default is 1, for one frame per sample; it may be more than 1 for multiple frames per sample
  // @return : how many frames of compressed video are stored in each sample
  virtual uint16_t GetFrameCount(void);

  // gets name, for informative purposes
  // @return : name, for informative purposes
  virtual const wchar_t *GetCompressorName(void);

  // gets one of the following values 0x0018 – images are in colour with no alpha
  // @return : one of the following values 0x0018 
  virtual uint16_t GetDepth(void);

  /* set methods */

  // sets video coding name
  // @param codingName : video coding name to set
  // @return : true if successful, false otherwise
  virtual bool SetCodingName(const wchar_t *codingName);

  // sets the maximum visual width of the stream described by this sample description, in pixels
  // @param width : the maximum visual width of the stream described by this sample description to set, in pixels
  virtual void SetWidth(uint16_t width);

  // sets the maximum visual height of the stream described by this sample description, in pixels
  // @param height : the maximum visual height of the stream described by this sample description to set, in pixels
  virtual void SetHeight(uint16_t height);

  // sets how many frames of compressed video are stored in each sample
  // the default is 1, for one frame per sample; it may be more than 1 for multiple frames per sample
  // @param frameCount : how many frames of compressed video are stored in each sample to set
  virtual void SetFrameCount(uint16_t frameCount);

  // sets name, for informative purposes
  // @param compressorName : name to set, for informative purposes
  // @return : true if successful, false otherwise
  virtual bool SetCompressorName(const wchar_t *compressorName);

  // sets one of the following values 0x0018 – images are in colour with no alpha
  // @param depth : one of the following values 0x0018 
  virtual void SetDepth(uint16_t depth);

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

  // the maximum visual width of the stream described by this sample description, in pixels
  uint16_t width;

  // the maximum visual height of the stream described by this sample description, in pixels
  uint16_t height;

  // the resolution of the image in pixels-per-inch, as a fixed 16.16 number
  CFixedPointNumber *horizontalResolution;

  // the resolution of the image in pixels-per-inch, as a fixed 16.16 number
  CFixedPointNumber *verticalResolution;

  // how many frames of compressed video are stored in each sample
  // the default is 1, for one frame per sample; it may be more than 1 for multiple frames per sample
  uint16_t frameCount;

  // name, for informative purposes
  // it is formatted in a fixed 32-byte field, with the first byte set to the number of bytes to be displayed,
  // followed by that number of bytes of displayable data, and then padding to complete 32 bytes total
  // (including the size byte)
  // the field may be set to 0
  wchar_t *compressorName;

  // one of the following values 0x0018 – images are in colour with no alpha
  uint16_t depth;

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