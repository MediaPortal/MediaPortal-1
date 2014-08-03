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

#ifndef __SEGMENT_RUN_TABLE_BOX_DEFINED
#define __SEGMENT_RUN_TABLE_BOX_DEFINED

#include "FullBox.h"

#include "QualitySegmentUrlModifierCollection.h"
#include "SegmentRunEntryCollection.h"

#define SEGMENT_RUN_TABLE_BOX_TYPE                                            L"asrt"

class CSegmentRunTableBox :
  public CFullBox
{
public:
  // initializes a new instance of CSegmentRunTableBox class
  CSegmentRunTableBox(HRESULT *result);

  // destructor
  virtual ~CSegmentRunTableBox(void);

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const uint8_t *buffer, uint32_t length);

  // gets box data in human readable format
  // @param indent : string to insert before each line
  // @return : box data in human readable format or NULL if error
  virtual wchar_t *GetParsedHumanReadable(const wchar_t *indent);

  // gets quality segment url modifiers table
  // @return : quality segment url modifiers table
  virtual CQualitySegmentUrlModifierCollection *GetQualitySegmentUrlModifiers(void);

  // gets segment run entry table
  // @return : segment run entry table
  virtual CSegmentRunEntryCollection *GetSegmentRunEntryTable(void);

protected:
  CQualitySegmentUrlModifierCollection *qualitySegmentUrlModifiers;
  CSegmentRunEntryCollection *segmentRunEntryTable;

  // parses data in buffer
  // @param buffer : buffer with box data for parsing
  // @param length : the length of data in buffer
  // @param processAdditionalBoxes : specifies if additional boxes have to be processed
  // @return : true if parsed successfully, false otherwise
  virtual bool ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes);
};

#endif