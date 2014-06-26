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

#ifndef __NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_DEFINED
#define __NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_DEFINED

#include "RangeAttribute.h"

#include <stdint.h>

#define TAG_ATTRIBUTE_INSTANCE_NORMAL_PLAY_TIME_RANGE                 L"a_range_npt"

#define NORMAL_PLAY_TIME_IDENTIFIER                                   L"npt="
#define NORMAL_PLAY_TIME_IDENTIFIER_LENGTH                            4

#define NORMAL_PLAY_TIME_SEPARATOR                                    L"-"
#define NORMAL_PLAY_TIME_SEPARATOR_LENGTH                             1

#define NORMAL_PLAY_TIME_DECIMAL_SEPARATOR                            L"."
#define NORMAL_PLAY_TIME_DECIMAL_SEPARATOR_LENGTH                     1

#define NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_NONE                    ATTRIBUTE_FLAG_NONE

#define NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_START_TIME              (1 << (ATTRIBUTE_FLAG_LAST + 0))
#define NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_END_TIME                (1 << (ATTRIBUTE_FLAG_LAST + 1))

#define NORMAL_PLAY_TIME_RANGE_ATTRIBUTE_FLAG_LAST                                    (ATTRIBUTE_FLAG_LAST + 2)

class CNormalPlayTimeRangeAttribute : public CRangeAttribute
{
public:
  // initializes a new instance of CNormalPlayTimeRangeAttribute class
  CNormalPlayTimeRangeAttribute(HRESULT *result);
  virtual ~CNormalPlayTimeRangeAttribute(void);

  /* get methods */

  // gets start time
  // @return : start time in ms
  virtual uint64_t GetStartTime(void);

  // gets end time
  // @return : end time in ms
  virtual uint64_t GetEndTime(void);

  /* set methods */

  /* other methods */

  // tests if start time is set
  // @return : true if start time is set, false otherwise
  virtual bool IsSetStartTime(void);

  // tests if end time is set
  // @return : true if end time is set, false otherwise
  virtual bool IsSetEndTime(void);

  // parses data in buffer
  // @param buffer : buffer with session tag data for parsing
  // @param length : the length of data in buffer
  // @return : return position in buffer after processing or 0 if not processed
  virtual unsigned int Parse(const wchar_t *buffer, unsigned int length);

  // clears current instance
  virtual void Clear(void);

protected:

  // holds start time
  uint64_t startTime;
  // holds end time
  uint64_t endTime;
};

#endif