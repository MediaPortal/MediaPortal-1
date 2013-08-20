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

#ifndef __FIXED_POINT_NUMBER_DEFINED
#define __FIXED_POINT_NUMBER_DEFINED

#include <stdint.h>

#define INTEGER_PART_SIZE_MAX                                                 32
#define FRACTION_PART_SIZE_MAX                                                32

class CFixedPointNumber
{
public:
  // initializes a new instance of CBox class
  // @param integerPartSize : size of integer part in bits (maximum allowed is INTEGER_PART_SIZE_MAX)
  // @param fractionPartSize : size of fraction part in bits (maximum allowed is FRACTION_PART_SIZE_MAX)
  CFixedPointNumber(uint8_t integerPartSize, uint8_t fractionPartSize);

  // destructor
  ~CFixedPointNumber(void);

  /* get methods */

  uint32_t GetIntegerPart(void);

  uint32_t GetFractionPart(void);

  uint64_t GetNumber(void);

  /* set methods */

  bool SetIntegerPart(uint32_t integerPart);

  bool SetFractionPart(uint32_t fractionPart);

  bool SetNumber(uint64_t number);

  /* other methods */

protected:
  // stores size of integer part in bits
  uint8_t integerPartSize;
  // stores size of fraction part in bits
  uint8_t fractionPartSize;
  // stores integer part
  uint32_t integerPart;
  // stores fraction part
  uint32_t fractionPart;

  // stores maximum number for integer part (2 ^ integerPartSize - 1)
  uint64_t integerPartMaximum;
  // stores maximum number for fraction part (2 ^ fractionPartSize - 1)
  uint64_t fractionPartMaximum;

  // gets maximum number which can be stored into 'size' bits
  // @param size : the count of bits for number
  // @return : maximum number
  uint64_t GetMaximum(uint8_t size);
};

#endif