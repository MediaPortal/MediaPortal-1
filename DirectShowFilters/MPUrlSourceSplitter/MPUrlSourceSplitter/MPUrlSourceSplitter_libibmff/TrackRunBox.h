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

#ifndef __TRACK_RUN_BOX_DEFINED
#define __TRACK_RUN_BOX_DEFINED

#include "FullBox.h"
#include "SampleCollection.h"

#define TRACK_RUN_BOX_TYPE                                            L"trun"

#define FLAGS_DATA_OFFSET_PRESENT                                     0x00000001
#define FLAGS_FIRST_SAMPLE_FLAGS_PRESENT                              0x00000004
#define FLAGS_SAMPLE_DURATION_PRESENT                                 0x00000100
#define FLAGS_SAMPLE_SIZE_PRESENT                                     0x00000200
#define FLAGS_SAMPLE_FLAGS_PRESENT                                    0x00000400
#define FLAGS_SAMPLE_COMPOSITION_TIME_OFFSETS_PRESENT                 0x00000800

#define TRACK_RUN_BOX_FLAG_NONE                                       FULL_BOX_FLAG_NONE

#define TRACK_RUN_BOX_FLAG_LAST                                       (FULL_BOX_FLAG_LAST + 0)

class CTrackRunBox :
  public CFullBox
{
public:
  // initializes a new instance of CTrackRunBox class
  CTrackRunBox(HRESULT *result);

  // destructor
  virtual ~CTrackRunBox(void);

  /* get methods */

  // gets addition to the implicit or explicit data offset established in the track fragment header
  // value valid only if IsDataOffsetPresent() is true
  // @return : addition to the implicit or explicit data offset established in the track fragment header
  virtual int32_t GetDataOffset(void);

  // gets set of flags for the first sample only of this run
  // value valid only if IsFirstDataSampleFlagsPresent() is true
  // @return : set of flags for the first sample only of this run
  virtual uint32_t GetFirstSampleFlags(void);

  // gets samples
  // valid values are only those for which Is...Present() is true
  // @return : samples
  virtual CSampleCollection *GetSamples(void);

  /* set methods */

  // sets addition to the implicit or explicit data offset established in the track fragment header
  // value valid only if IsDataOffsetPresent() is true
  // @param dataOffset : addition to the implicit or explicit data offset established in the track fragment header to set
  virtual void SetDataOffset(int32_t dataOffset);

  // sets set of flags for the first sample only of this run
  // value valid only if IsFirstDataSampleFlagsPresent() is true
  // @param firstSampleFlags : set of flags for the first sample only of this run to set
  virtual void SetFirstSampleFlags(uint32_t firstSampleFlags);

  /* other methods */

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

  // tests if data offset is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsDataOffsetPresent(void);

  // tests if first data sample flags is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsFirstDataSampleFlagsPresent(void);

  // tests if sample duration is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsSampleDurationPresent(void);

  // tests if sample size is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsSampleSizePresent(void);

  // tests if sample flags is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsSampleFlagsPresent(void);

  // tests if sample composition time offsets is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsSampleCompositionTimeOffsetsPresent(void);

protected:

  /*

  Within the Track Fragment Box, there are zero or more Track Run Boxes. If the duration-is-empty flag is set in
  the tf_flags, there are no track runs. A track run documents a contiguous set of samples for a track.

  The number of optional fields is determined from the number of bits set in the lower byte of the flags, and the
  size of a record from the bits set in the second byte of the flags. This procedure shall be followed, to allow for
  new fields to be defined.

  If the data-offset is not present, then the data for this run starts immediately after the data of the previous run,
  or at the base-data-offset defined by the track fragment header if this is the first run in a track fragment, If the
  data-offset is present, it is relative to the base-data-offset established in the track fragment header.

  The following flags are defined:

  0x000001:
    data-offset-present

  0x000004:
    first-sample-flags-present; this over-rides the default flags for the first sample only. This
    makes it possible to record a group of frames where the first is a key and the rest are difference
    frames, without supplying explicit flags for every sample. If this flag and field are used, sample-flags
    shall not be present.

  0x000100:
    sample-duration-present: indicates that each sample has its own duration, otherwise the default is used.

  0x000200:
    sample-size-present: each sample has its own size, otherwise the default is used.

  0x000400:
    sample-flags-present; each sample has its own flags, otherwise the default is used

  0x000800:
    sample-composition-time-offsets-present; each sample has a composition time offset (e.g. as
    used for I/P/B video in MPEG).

  */

  // added to the implicit or explicit data offset established in the track fragment header
  int32_t dataOffset;

  // a set of flags for the first sample only of this run
  uint32_t firstSampleFlags;

  CSampleCollection *samples;

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