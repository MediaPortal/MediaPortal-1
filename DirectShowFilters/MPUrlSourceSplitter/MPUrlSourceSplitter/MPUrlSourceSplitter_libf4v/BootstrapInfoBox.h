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

#ifndef __BOOTSTRAP_INFO_BOX_DEFINED
#define __BOOTSTRAP_INFO_BOX_DEFINED

#include "FullBox.h"

#include "BootstrapInfoServerEntryCollection.h"
#include "BootstrapInfoQualityEntryCollection.h"
#include "SegmentRunTableBoxCollection.h"
#include "FragmentRunTableBoxCollection.h"

#define BOOTSTRAP_INFO_BOX_TYPE                                               L"abst"

#define PROFILE_NAMED_ACCESS                                                  0
#define PROFILE_RANGE_ACCESS                                                  1

class CBootstrapInfoBox :
  public CFullBox
{
public:
  // initializes a new instance of CBootstrapInfoBox class
  CBootstrapInfoBox(HRESULT *result);

  // destructor
  virtual ~CBootstrapInfoBox(void);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const uint8_t *buffer, uint32_t length);

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

  // gets the version number of the bootstrap information
  // when IsUpdate field is true, bootstrapinfo version indicates the version number that is being updated
  // @return : the version number of the bootstrap information
  virtual uint32_t GetBootstrapInfoVersion(void);

  // gets profile
  // @return : profile value (PROFILE_NAMED_ACCESS or PROFILE_RANGE_ACCESS)
  virtual uint8_t GetProfile(void);

  // gets if the media presentation is live or not
  // @return : true if the media presentation is live, false otherwise
  virtual bool IsLive(void);

  // gets if bootstrap info box is an update to previous bootstrap info box or not
  // @return : true if this table is an update, false otherwise
  virtual bool IsUpdate(void);

  // gets the number of time units per second
  // @return : the number of time units per second
  virtual uint32_t GetTimeScale(void);

  // gets the timestamp in TimeScale units of the latest available fragment in the media presentation
  // return : the timestamp in TimeScale units
  virtual uint64_t GetCurrentMediaTime(void);

  // the offset of the CurrentMediaTime from the SMPTE time code, converted to milliseconds
  // this offset is not in TimeScale units
  // this field is zero when not used
  // the server uses the SMPTE time code modulo 24 hours to make the offset positive
  // @return : the offset of the CurrentMediaTime from the SMPTE time code
  virtual uint64_t GetSmpteTimeCodeOffset(void);

  // gets movie identifier
  // @return : movie identifier
  virtual const wchar_t *GetMovieIdentifier(void);

  // gets server entry table (server URLs in descending order of preference)
  // @return : server entry table
  virtual CBootstrapInfoServerEntryCollection *GetServerEntryTable(void);

  // gets quality entry table
  // @return : quality entry table
  virtual CBootstrapInfoQualityEntryCollection *GetQualityEntryTable(void);

  // gets string holding Digital Rights Management metadata
  // encrypted files use this metadata to get the necessary keys and licenses for decryption and playback
  // @return : string holding Digital Rights Management metadata
  virtual const wchar_t *GetDrmData(void);

  // gets string that holds metadata
  // @return : string that holds metadata
  virtual const wchar_t *GetMetaData(void);

  // gets segment run table
  // @return : segment run table
  virtual CSegmentRunTableBoxCollection *GetSegmentRunTable(void);

  // gets fragment run table
  // @return : fragment run table
  virtual CFragmentRunTableBoxCollection *GetFragmentRunTable(void);

protected:

  uint32_t bootstrapInfoVersion;

  // only lower 6 bits used
  uint8_t profile;

  bool live;

  bool update;

  uint32_t timeScale;

  uint64_t currentMediaTime;

  uint64_t smpteTimeCodeOffset;

  wchar_t *movieIdentifier;

  CBootstrapInfoServerEntryCollection *serverEntryTable;

  CBootstrapInfoQualityEntryCollection *qualityEntryTable;

  wchar_t *drmData;

  wchar_t *metaData;

  CSegmentRunTableBoxCollection *segmentRunTable;

  CFragmentRunTableBoxCollection *fragmentRunTable;

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes);
};

#endif