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

#ifndef __TRACK_FRAGMENT_HEADER_BOX_DEFINED
#define __TRACK_FRAGMENT_HEADER_BOX_DEFINED

#include "FullBox.h"

#define TRACK_FRAGMENT_HEADER_BOX_TYPE                                        L"tfhd"

#define FLAGS_BASE_DATA_OFFSET_PRESENT                                        0x00000001
#define FLAGS_SAMPLE_DESCRIPTION_INDEX_PRESENT                                0x00000002
#define FLAGS_DEFAULT_SAMPLE_DURATION_PRESENT                                 0x00000008
#define FLAGS_DEFAULT_SAMPLE_SIZE_PRESENT                                     0x00000010
#define FLAGS_DEFAULT_SAMPLE_FLAGS_PRESENT                                    0x00000020
#define FLAGS_DURATION_IS_EMPTY                                               0x00010000

class CTrackFragmentHeaderBox :
  public CFullBox
{
public:
  // initializes a new instance of CTrackFragmentHeaderBox class
  CTrackFragmentHeaderBox(void);

  // destructor
  virtual ~CTrackFragmentHeaderBox(void);

  /* get methods */

  // gets whole box into buffer (buffer must be allocated before)
  // @param buffer : the buffer for box data
  // @param length : the length of buffer for data
  // @return : true if all data were successfully stored into buffer, false otherwise
  virtual bool GetBox(uint8_t *buffer, uint32_t length);

  // gets track ID
  // @return : track ID
  virtual uint32_t GetTrackId(void);

  // gets base offset to use when calculating data offsets
  // value valid only if IsBaseDataOffsetPresent() is true
  // @return : base offset to use when calculating data offsets
  virtual uint64_t GetBaseDataOffset(void);

  // gets sample description index
  // value valid only if IsSampleDescriptionIndexPresent() is true
  // @return : sample description index
  virtual uint32_t GetSampleDescriptionIndex(void);

  // gets default sample duration
  // value valid only if IsDefaultSampleDurationPresent() is true
  // @return : default sample duration
  virtual uint32_t GetDefaultSampleDuration(void);

  // gets default sample size
  // value valid only if IsDefaultSampleSizePresent() is true
  // @return : default sample size
  virtual uint32_t GetDefaultSampleSize(void);

  // gets default sample flags
  // value valid only if IsDefaultSampleFlagsPresent() is true
  // @return : default sample flags
  virtual uint32_t GetDefaultSampleFlags(void);

  /* set methods */

  // sets track ID
  // @param trackId : track ID to set
  virtual void SetTrackId(uint32_t trackId);

  // gets base offset to use when calculating data offsets
  // value valid only if IsBaseDataOffsetPresent() is true
  // @param baseDataOffset : base offset to set to use when calculating data offsets
  virtual void SetBaseDataOffset(uint64_t baseDataOffset);

  // gets sample description index
  // value valid only if IsSampleDescriptionIndexPresent() is true
  // @param sampleDescriptionIndex : sample description index to set
  virtual void SetSampleDescriptionIndex(uint32_t sampleDescriptionIndex);

  // gets default sample duration
  // value valid only if IsDefaultSampleDurationPresent() is true
  // @param defaultSampleDuration : default sample duration to set
  virtual void SetDefaultSampleDuration(uint32_t defaultSampleDuration);

  // gets default sample size
  // value valid only if IsDefaultSampleSizePresent() is true
  // @param defaultSampleSize : default sample size to set
  virtual void SetDefaultSampleSize(uint32_t defaultSampleSize);

  // gets default sample flags
  // value valid only if IsDefaultSampleFlagsPresent() is true
  // @param defaultSampleFlags : default sample flags to set
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

  // tests if base data offset is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsBaseDataOffsetPresent(void);

  // tests if sample description index is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsSampleDescriptionIndexPresent(void);

  // test if default sample duration is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsDefaultSampleDurationPresent(void);

  // tests if default sample size is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsDefaultSampleSizePresent(void);

  // tests if default sample flags is valid value
  // @return : true if valid value, false otherwise
  virtual bool IsDefaultSampleFlagsPresent(void);

  // tests if duration provided in either default-sample-duration or by the default-duration in the Track Extends Box, is empty
  // @return : true if duration is empty, false otherwise
  virtual bool IsDurationIsEmpty(void);

protected:

  /*
  The following flags are defined in the tf_flags:

  0x000001:
    base-data-offset-present: indicates the presence of the base-data-offset field. This provides
    an explicit anchor for the data offsets in each track run (see below). If not provided, the base-dataoffset
    for the first track in the movie fragment is the position of the first byte of the enclosing Movie
    Fragment Box, and for second and subsequent track fragments, the default is the end of the data
    defined by the preceding fragment. Fragments 'inheriting' their offset in this way must all use the same
    data-reference (i.e., the data for these tracks must be in the same file).

  0x000002:
    sample-description-index-present: indicates the presence of this field, which over-rides, in this
    fragment, the default set up in the Track Extends Box.

  0x000008:
    default-sample-duration-present

  0x000010:
    default-sample-size-present

  0x000020:
    default-sample-flags-present

  0x010000:
    duration-is-empty: this indicates that the duration provided in either default-sample-duration,
  or by the default-duration in the Track Extends Box, is empty, i.e. that there are no samples for this
  time interval. It is an error to make a presentation that has both edit lists in the Movie Box, and empty duration
  fragments.

  */

  uint32_t trackId;

  // all the following are optional fields

  // base offset to use when calculating data offsets
  uint64_t baseDataOffset;

  uint32_t sampleDescriptionIndex;
  uint32_t defaultSampleDuration;
  uint32_t defaultSampleSize;
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