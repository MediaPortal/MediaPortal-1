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

#ifndef __END_OF_STREAM_REACHED_DEFINED
#define __END_OF_STREAM_REACHED_DEFINED

#include <stdint.h>

class CEndOfStreamReached
{
public:
  CEndOfStreamReached(void);
  ~CEndOfStreamReached(void);

  /* get methods */

  // gets end stream position
  // @return : end stream position
  int64_t GetStreamPosition(void);

  /* set methods */

  // sets end stream position
  // @param streamPosition : end stream position to set
  void SetStreamPosition(int64_t streamPosition);

  /* other methods */

  // tests if end of stream was set
  // @return : true if end of stream was set, false otherwise
  bool IsSet(void);

  // clears current instance to default state
  void Clear(void);

private:

  // holds end stream position
  int64_t streamPosition;

  // specifies if value was set
  bool setValue;
};

#endif