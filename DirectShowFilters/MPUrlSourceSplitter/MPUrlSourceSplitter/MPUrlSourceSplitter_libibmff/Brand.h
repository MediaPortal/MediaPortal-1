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

#ifndef __BRAND_DEFINED
#define __BRAND_DEFINED

#include <stdint.h>

class CBrand
{
public:
  // initializes a new instance of CBrand class
  CBrand(HRESULT *result);

  // destructor
  ~CBrand(void);

  /* get methods */

  // gets brand as unsigned integer
  // @return : brand as unsigned integer
  uint32_t GetBrand(void);

  // gets brand as string
  // @return : brand as string
  const wchar_t *GetBrandString(void);

  /* set methods */

  // sets brand as unsigned integer
  // @param brand : brand as unsigned integer to set
  // @return : true if successful, false otherwise
  bool SetBrand(uint32_t brand);

  // sets brand as string
  // @param brandString : brand as string to set
  // @return : true if successful, false otherwise
  bool SetBrandString(const wchar_t *brandString);

  /* other methods */

protected:
  // holds brand
  uint32_t brand;
  // hodls brand string
  wchar_t *brandString;

  // converts brand unsigned integer to string
  // @return : true if successful, false otherwise
  bool ConvertBrandToString(void);

  // converts brand string to unsigned integer
  // @return : true if successful, false otherwise
  bool ConvertBrandToUnsignedInteger(void);
};

#endif