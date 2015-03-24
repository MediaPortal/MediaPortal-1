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

#include "Flags.h"

#define FRAGMENT_RUN_ENTRY_FLAG_NONE                                  FLAGS_NONE

#define FRAGMENT_RUN_ENTRY_FLAG_END_OF_PRESENTATION                   (1 << (FLAGS_LAST + 0))
#define FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_FRAGMENT_NUMBERING      (1 << (FLAGS_LAST + 1))
#define FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_TIMESTAMPS              (1 << (FLAGS_LAST + 2))

#define FRAGMENT_RUN_ENTRY_FLAG_LAST                                  (FLAGS_LAST + 0)

class CFragmentRunEntry : public CFlags
{
public:
  // initializes a new instance of CFragmentRunEntry class
  CFragmentRunEntry(HRESULT *result, uint32_t firstFragment, uint64_t firstFragmentTimestamp, uint32_t fragmentDuration, uint32_t cumulatedFragmentCount);
  ~CFragmentRunEntry(void);

  /* get method */

  // gets first fragment
  // @return : first fragment
  uint32_t GetFirstFragment(void);

  // gets first fragment timestamp
  // @return : first fragment timestamp
  uint64_t GetFirstFragmentTimestamp(void);

  // gets fragment duration
  // @return : fragment duration
  uint32_t GetFragmentDuration(void);

  // gets cumulated fragment count
  // @return : cumulated fragment count
  uint32_t GetCumulatedFragmentCount(void);

  /* set methods */

  /* other methods */

  // tests if end of presentation flag is set
  // @return : true if flag is set, false otherwise
  bool IsEndOfPresentation(void);

  // tests if discontinuity in fragment numbering flag is set
  // @return : true if flag is set, false otherwise
  bool IsDiscontinuityFragmentNumbering(void);

  // tests if discontinuity in timestamps flag is set
  // @return : true if flag is set, false otherwise
  bool IsDiscontinuityTimestamps(void);

protected:
  // holds the identifying number of the first fragment in this run of fragments with the same duration
  uint32_t firstFragment;
  // holds the timestamp of the FirstFragment, in TimeScale units
  uint64_t firstFragmentTimestamp;
  // holds the duration, in TimeScale units, of each fragment in this run
  uint32_t fragmentDuration;
  // holds cumulated fragment count
  uint32_t cumulatedFragmentCount;

  /* methods */
};

#endif