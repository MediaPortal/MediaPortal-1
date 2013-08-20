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

#ifndef __TRACK_EXTENDS_BOX_DEFINED
#define __TRACK_EXTENDS_BOX_DEFINED

#include "FullBox.h"

#define TRACK_EXTENDS_BOX_TYPE                                                L"trex"

class CTrackExtendsBox :
  public CFullBox
{
public:
  // initializes a new instance of CTrackExtendsBox class
  CTrackExtendsBox(void);

  // destructor
  virtual ~CTrackExtendsBox(void);

  /* get methods */

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  // the track; this shall be the track ID of a track in the Movie Box
  virtual uint32_t GetTrackId(void);

  // default sample description index for track fragments
  virtual uint32_t GetDefaultSampleDescriptionIndex(void);

  // default sample duration for track fragments
  virtual uint32_t GetDefaultSampleDuration(void);

  // default sample size for track fragments
  virtual uint32_t GetDefaultSampleSize(void);

  // default sample flags for track fragments
  virtual uint32_t GetDefaultSampleFlags(void);

  /* set methods */

  // sets track ID of a track in the Movie Box
  // @param trackId : track ID of a track in the Movie Box to set
  virtual void SetTrackId(uint32_t trackId);

  // sets default sample description index for track fragments
  // @param defaultSampleDescriptionIndex : default sample description index for track fragments to set
  virtual void SetDefaultSampleDescriptionIndex(uint32_t defaultSampleDescriptionIndex);

  // sets default sample duration for track fragments
  // @param defaultSampleDuration : default sample duration for track fragments to set
  virtual void SetDefaultSampleDuration(uint32_t defaultSampleDuration);

  // sets default sample size for track fragments
  // @param defaultSampleSize : default sample size for track fragments to set
  virtual void SetDefaultSampleSize(uint32_t defaultSampleSize);

  // sets default sample flags for track fragments
  // @param defaultSampleFlags : default sample flags for track fragments to set
  virtual void SetDefaultSampleFlags(uint32_t defaultSampleFlags);

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

  /*

  The sample flags field in sample fragments (defaultSampleFlags here and in a Track Fragment Header
  Box, and sampleFlags and firstSampleFlags in a Track Fragment Run Box) is coded as a 32-bit
  value. It has the following structure:

  bit(6) reserved=0;
  unsigned int(2) sample_depends_on;
  unsigned int(2) sample_is_depended_on;
  unsigned int(2) sample_has_redundancy;
  bit(3) sample_padding_value;
  bit(1) sample_is_difference_sample;
  // i.e. when 1 signals a non-key or non-sync sample
  unsigned int(16) sample_degradation_priority;

  The sample_depends_on, sample_is_depended_on and sample_has_redundancy values are defined
  as documented in the Independent and Disposable Samples Box.

  The sample_padding_value is defined as for the padding bits table. The
  sample_degradation_priority is defined as for the degradation priority table.

  */

  // the track; this shall be the track ID of a track in the Movie Box
  uint32_t trackId;

  // default sample description index for track fragments
  uint32_t defaultSampleDescriptionIndex;

  // default sample duration for track fragments
  uint32_t defaultSampleDuration;

  // default sample size for track fragments
  uint32_t defaultSampleSize;

  // default sample flags for track fragments
  uint32_t defaultSampleFlags;

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