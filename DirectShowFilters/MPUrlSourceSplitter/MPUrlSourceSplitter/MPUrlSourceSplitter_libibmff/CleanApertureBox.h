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

#ifndef __CLEAN_APERTURE_BOX_DEFINED
#define __CLEAN_APERTURE_BOX_DEFINED

#include "Box.h"

#define CLEAN_APERTURE_BOX_TYPE                                               L"clap"

class CCleanApertureBox :
  public CBox
{
public:
  // initializes a new instance of CCleanApertureBox class
  CCleanApertureBox(void);

  // destructor
  virtual ~CCleanApertureBox(void);

  /* get methods */

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  // fractional number which defines the exact clean aperture width, in counted pixels, of the video image
  virtual uint32_t GetCleanApertureWidthN(void);

  // fractional number which defines the exact clean aperture width, in counted pixels, of the video image
  virtual uint32_t GetCleanApertureWidthD(void);

  // fractional number which defines the exact clean aperture height, in counted pixels, of the video image
  virtual uint32_t GetCleanApertureHeightN(void);

  // fractional number which defines the exact clean aperture height, in counted pixels, of the video image
  virtual uint32_t GetCleanApertureHeightD(void);

  // fractional number which defines the horizontal offset of clean aperture centre minus (width-1)/2
  // typically 0
  virtual uint32_t GetHorizontalOffsetN(void);

  // fractional number which defines the horizontal offset of clean aperture centre minus (width-1)/2
  // typically 0
  virtual uint32_t GetHorizontalOffsetD(void);

  // fractional number which defines the vertical offset of clean aperture centre minus (height-1)/2
  // typically 0
  virtual uint32_t GetVerticalOffsetN(void);

  // fractional number which defines the vertical offset of clean aperture centre minus (height-1)/2
  // typically 0
  virtual uint32_t GetVerticalOffsetD(void);

  /* set methods */

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

  // fractional number which defines the exact clean aperture width, in counted pixels, of the video image
  uint32_t cleanApertureWidthN;

  // fractional number which defines the exact clean aperture width, in counted pixels, of the video image
  uint32_t cleanApertureWidthD;

  // fractional number which defines the exact clean aperture height, in counted pixels, of the video image
  uint32_t cleanApertureHeightN;

  // fractional number which defines the exact clean aperture height, in counted pixels, of the video image
  uint32_t cleanApertureHeightD;

  // fractional number which defines the horizontal offset of clean aperture centre minus (width-1)/2
  // typically 0
  uint32_t horizOffN;

  // fractional number which defines the horizontal offset of clean aperture centre minus (width-1)/2
  // typically 0
  uint32_t horizOffD;

  // fractional number which defines the vertical offset of clean aperture centre minus (height-1)/2
  // typically 0
  uint32_t vertOffN;

  // fractional number which defines the vertical offset of clean aperture centre minus (height-1)/2
  // typically 0
  uint32_t vertOffD;

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