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

#ifndef __FRAGMENT_RUN_ENTRY_DEFINED
#define __FRAGMENT_RUN_ENTRY_DEFINED

#include <stdint.h>

#define DISCONTINUITY_INDICATOR_END_OF_PRESENTATION                           0
#define DISCONTINUITY_INDICATOR_FRAGMENT_NUMBERING                            1
#define DISCONTINUITY_INDICATOR_TIMESTAMPS                                    2
#define DISCONTINUITY_INDICATOR_FRAGMENT_NUMBERING_AND_TIMESTAMPS             3

#define DISCONTINUITY_INDICATOR_NOT_AVAILABLE                                 UINT_MAX

class CFragmentRunEntry
{
public:
  // initializes a new instance of CFragmentRunEntry class
  CFragmentRunEntry(uint32_t firstFragment, uint64_t firstFragmentTimestamp, uint32_t fragmentDuration, uint32_t discontinuityIndicator);
  ~CFragmentRunEntry(void);

  // gets first fragment
  // @return : first fragment
  uint32_t GetFirstFragment(void);

  // gets first fragment timestamp
  // @return : first fragment timestamp
  uint64_t GetFirstFragmentTimestamp(void);

  // gets fragment duration
  // @return : fragment duration
  uint32_t GetFragmentDuration(void);

  // gets discontinuity indicator
  // @return : discontinuity indicator or DISCONTINUITY_INDICATOR_NOT_AVAILABLE if not available
  uint32_t GetDiscontinuityIndicator(void);

private:
  // stores the identifying number of the first fragment in this run of fragments with the same duration
  uint32_t firstFragment;
  // stores the timestamp of the FirstFragment, in TimeScale units
  uint64_t firstFragmentTimestamp;
  // stores the duration, in TimeScale units, of each fragment in this run
  uint32_t fragmentDuration;
  // indicates discontinuities in timestamps, fragment numbers, or both
  // this field is also used to identify the end of a (live) presentation
  uint32_t discontinuityIndicator;
};

#endif