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

#ifndef __REPEAT_INTERVAL_DEFINED
#define __REPEAT_INTERVAL_DEFINED

class CRepeatInterval
{
public:
  // initializes a new instance of CRepeatInterval class
  CRepeatInterval(void);
  ~CRepeatInterval(void);

  /* get methods */

  // gets repeat interval in seconds
  // @return : repeat interval in seconds
  unsigned int GetRepeatInterval(void);

  // gets active duration in seconds
  // @return : active duration in seconds
  unsigned int GetActiveDuration(void);

  // gets offset from start time in seconds
  // @return : offset from start time in seconds
  unsigned int GetOffset(void);

  /* set methods */

  /* other methods */

protected:

  // holds repeat interval (max interval is UINT_MAX in seconds = 49710 days)
  unsigned int repeatInterval;

  // holds active duration
  unsigned int activeDuration;

  // holds offset from start time
  unsigned int offset;
};

#endif