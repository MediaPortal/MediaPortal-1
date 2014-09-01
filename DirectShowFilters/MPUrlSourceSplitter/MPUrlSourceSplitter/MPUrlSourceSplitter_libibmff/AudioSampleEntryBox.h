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

#ifndef __AUDIO_SAMPLE_ENTRY_BOX_DEFINED
#define __AUDIO_SAMPLE_ENTRY_BOX_DEFINED

#include "SampleEntryBox.h"
#include "FixedPointNumber.h"

#define AUDIO_SAMPLE_ENTRY_BOX_FLAG_NONE                              SAMPLE_ENTRY_BOX_FLAG_NONE

#define AUDIO_SAMPLE_ENTRY_BOX_FLAG_LAST                              (SAMPLE_ENTRY_BOX_FLAG_LAST + 0)

class CAudioSampleEntryBox :
  public CSampleEntryBox
{
public:
  // initializes a new instance of CAudioSampleEntryBox class
  CAudioSampleEntryBox(HRESULT *result);

  // destructor
  virtual ~CAudioSampleEntryBox(void);

  /* get methods */

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  // gets audio coding name
  // @return : coding name
  virtual const wchar_t *GetCodingName(void);

  // gets channel count
  // @return : 1 (mono) or 2 (stereo)
  virtual uint16_t GetChannelCount(void);

  // gets sample size
  // @return : in bits, and takes the default value of 16
  virtual uint16_t GetSampleSize(void);

  // gets the sampling rate
  // @return : the sampling rate expressed as a 16.16 fixed-point number (hi.lo)
  virtual CFixedPointNumber *GetSampleRate(void);

  /* set methods */

  // sets audio coding name
  // @param codingName : audio coding name to set
  // @return : true if successful, false otherwise
  virtual bool SetCodingName(const wchar_t *codingName);

  // sets channel count
  // @param channelCount : channel count to set
  virtual void SetChannelCount(uint16_t channelCount);

  // sets sample size
  // @param sampleSize : the sample size (in bits) to set
  virtual void SetSampleSize(uint16_t sampleSize);

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

  // either 1 (mono) or 2 (stereo)
  uint16_t channelCount;

  // in bits, and takes the default value of 16
  uint16_t sampleSize;

  // the sampling rate expressed as a 16.16 fixed-point number (hi.lo)
  CFixedPointNumber *sampleRate;

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