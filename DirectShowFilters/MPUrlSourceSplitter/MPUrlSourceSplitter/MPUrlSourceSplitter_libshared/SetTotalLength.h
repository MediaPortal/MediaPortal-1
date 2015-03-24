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

#ifndef __SET_TOTAL_LENGTH_DEFINED
#define __SET_TOTAL_LENGTH_DEFINED

#include <stdint.h>

class CSetTotalLength
{
public:
  CSetTotalLength(void);
  ~CSetTotalLength(void);

  /* get methods */

  // gets total length
  // @return : total length
  int64_t GetTotalLength(void);

  // tests if total length is guess
  // @return : true if total length is guess, false otherwise
  bool IsEstimate(void);

  /* set methods */

  // sets total length
  // @param totalLength : total length to set
  void SetTotalLength(int64_t totalLength);

  // sets total length
  // @param totalLength : total length to set
  // @param estimate : true if total length is guess, false otherwise
  void SetTotalLength(int64_t totalLength, bool estimate);

  // sets if total length is guess
  // @param estimate : true if total length is guess, false otherwise
  void SetEstimate(bool estimate);

  /* other methods */

  // tests if total length was set
  // @return : true if total length was set, false otherwise
  bool IsSet(void);

  // clears current instance to default state
  void Clear(void);

private:

  // holds total length
  int64_t total;

  // specifies if total length is guess
  bool estimate;

  // specifies if value was set
  bool setValue;
};

#endif