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

#ifndef __FRAGMENTED_INDEX_DEFINED
#define __FRAGMENTED_INDEX_DEFINED

#include <stdint.h>

class CFragmentedIndex
{
public:
  // creates a new instance of CFragmentedIndex class
  CFragmentedIndex(void);

  // destructor
  ~CFragmentedIndex(void);

  /* get methods */

  // gets timestamp
  // @return : timestamp
  uint64_t GetTimestamp(void);

  // gets duration
  // @return : duration
  uint64_t GetDuration(void);

  /* set methods */

  // sets timestamp
  // @param timestamp : timestamp to set
  void SetTimestamp(uint64_t timestamp);

  // sets duration
  // @param duration : duration to set
  void SetDuration(uint64_t duration);

  /* other methods */

private:

  // stores timestamp
  uint64_t timestamp;

  // stores duration
  uint64_t duration;
};

#endif