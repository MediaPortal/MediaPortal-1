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

#ifndef __SESSION_TIME_DEFINED
#define __SESSION_TIME_DEFINED

#include "RepeatIntervalCollection.h"

#include <stdint.h>

class CSessionTime
{
public:
  // initializes a new instance of CSessionTime class
  CSessionTime(HRESULT *result);
  ~CSessionTime(void);

  /* get methods */

  // gets start time (decimal representation of Network Time Protocol)
  // @return : start time
  uint64_t GetStartTime(void);

  // gets stop time (decimal representation of Network Time Protocol)
  // @return : stop time
  uint64_t GetStopTime(void);

  // gets repeat intervals
  // @return : repeat intervals
  CRepeatIntervalCollection *GetRepeatIntervals(void);

  /* set methods */

  /* other methods */

protected:

  // holds start time
  uint64_t startTime;

  // holds stop time
  uint64_t stopTime;

  // holds repeat intervals
  CRepeatIntervalCollection *repeatIntervals;
};

#endif