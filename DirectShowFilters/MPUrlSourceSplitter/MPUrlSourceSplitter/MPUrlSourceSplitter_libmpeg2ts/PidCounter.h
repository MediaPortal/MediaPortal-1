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

#ifndef __PID_COUNTER_DEFINED
#define __PID_COUNTER_DEFINED

#include <stdint.h>

#define PID_COUNTER_NOT_SPECIFIED                                     0xFF

class CPidCounter
{
public:
  CPidCounter(HRESULT *result);
  ~CPidCounter(void);

  /* get methods */

  // gets current counter
  // @return : current counter or PID_COUNTER_NOT_SPECIFIED if not specified
  uint8_t GetCurrentCounter(void);

  // gets previous counter
  // @return : previous counter or PID_COUNTER_NOT_SPECIFIED if not specified
  uint8_t GetPreviousCounter(void);

  // gets expected counter value based on current counter value
  // @return : expected counter calue or PID_COUNTER_NOT_SPECIFIED if not specified
  uint8_t GetExpectedCurrentCounter(void);

  // gets expected counter value based on current counter value
  // @return : expected counter calue or PID_COUNTER_NOT_SPECIFIED if not specified
  uint8_t GetExpectedPreviousCounter(void);

  /* set methods */

  // sets current counter
  // @param currentCounter : the current counter to set
  void SetCurrentCounter(uint8_t currentCounter);

  // sets previous counter
  // @param previousCounter : the previous counter to set
  void SetPreviousCounter(uint8_t previousCounter);

  /* other methods */

  // tests if current counter is set
  // @return : true if current counter is set, false otherwise
  bool IsSetCurrentCounter(void);

  // tests if previous counter is set
  // @return : true if previous counter is set, false otherwise
  bool IsSetPreviousCounter(void);

  // clears current instance to default state
  void Clear(void);

protected:

  // holds current and previous counter
  uint8_t currentCounter;
  uint8_t previousCounter;

  /* methods */
};

#endif