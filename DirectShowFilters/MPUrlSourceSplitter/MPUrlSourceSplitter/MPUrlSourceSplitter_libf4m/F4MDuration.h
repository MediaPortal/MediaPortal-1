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

#ifndef __F4M_DURATION_DEFINED
#define __F4M_DURATION_DEFINED

#include <stdint.h>

#define F4M_DURATION_NOT_SPECIFIED                                            UINT64_MAX

class CF4MDuration
{
public:
  // initializes a new instance of CF4MDuration class
  CF4MDuration(void);

  // destructor
  ~CF4MDuration(void);

  /* get methods */

  // gets duration of media (in ms)
  // @return : media duration (in ms) or F4M_DURATION_NOT_SPECIFIED if not specified
  uint64_t GetDuration(void);

  /* set methods */

  // sets duration of media (in ms)
  // @param duration : duration of media (in ms) to set
  void SetDuration(uint64_t duration);

  /* other methods */

protected:

  // holds duration of media (in ms)
  uint64_t duration;
};

#endif