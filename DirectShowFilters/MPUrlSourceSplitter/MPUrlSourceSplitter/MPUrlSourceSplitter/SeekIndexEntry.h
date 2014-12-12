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

#ifndef __SEEK_INDEX_ENTRY_DEFINED
#define __SEEK_INDEX_ENTRY_DEFINED

#include <stdint.h>

class CSeekIndexEntry
{
public:
  CSeekIndexEntry(int64_t position, int64_t timestamp);
  ~CSeekIndexEntry(void);

  /* get methods */

  // gets position
  // @return : position
  int64_t GetPosition(void);

  // gets timestamp
  // @return : timestamp
  int64_t GetTimestamp(void);

  /* set methods */

  // sets position
  // @param : position to set
  void SetPosition(int64_t position);

  // sets timestamp
  // @param timestamp: timestamp to set
  void SetTimestamp(int64_t timestamp);

  /* other methods */

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CSeekIndexEntry *Clone(void);

protected:
  // holds position
  int64_t position;
  // holds DTS
  int64_t timestamp;
};

#endif