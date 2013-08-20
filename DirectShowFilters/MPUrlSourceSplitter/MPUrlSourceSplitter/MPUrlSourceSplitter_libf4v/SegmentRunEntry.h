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

#ifndef __SEGMENT_RUN_ENTRY_DEFINED
#define __SEGMENT_RUN_ENTRY_DEFINED

#include <stdint.h>

class CSegmentRunEntry
{
public:
  // initializes a new instance of CSegmentRunEntry class
  CSegmentRunEntry(uint32_t firstSegment, uint32_t fragmentsPerSegment);

  ~CSegmentRunEntry(void);

  // gets first segment
  // @return : first segment
  uint32_t GetFirstSegment(void);

  // gets fragments per segment
  // @return : fragments per segment
  uint32_t GetFragmentsPerSegment(void);

private:
  // stores the identifying number of the first segment in the run of segments containing the same number of fragments
  uint32_t firstSegment;
  // stores the number of fragments in each segment in this run
  uint32_t fragmentsPerSegment;
};

#endif