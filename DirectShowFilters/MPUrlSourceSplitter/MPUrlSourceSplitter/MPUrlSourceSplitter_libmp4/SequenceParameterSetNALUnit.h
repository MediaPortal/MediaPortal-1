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

#ifndef __SEQUENCE_PARAMETER_SET_NAL_UNIT_DEFINED
#define __SEQUENCE_PARAMETER_SET_NAL_UNIT_DEFINED

#include <stdint.h>

class CSequenceParameterSetNALUnit
{
public:
  // creates a new instance of CSequenceParameterSetNALUnit class
  CSequenceParameterSetNALUnit(void);

  // destructor
  ~CSequenceParameterSetNALUnit(void);

  /* get methods */

  // gets the length of buffer
  // @return : length of buffer
  uint16_t GetLength(void);

  // gets buffer
  // @return : buffer or NULL if error (if length is not zero)
  const uint8_t *GetBuffer(void);

  /* set methods */

  // sets buffer
  // @param buffer : the buffer containg data to set
  // @param length : the length of buffer
  // @result : true if successful, false otherwise
  bool SetBuffer(const uint8_t *buffer, uint16_t length);

  /* other methods */

private:

  // stores length of buffer
  uint16_t length;

  // stores buffer data
  uint8_t *buffer;
};

#endif