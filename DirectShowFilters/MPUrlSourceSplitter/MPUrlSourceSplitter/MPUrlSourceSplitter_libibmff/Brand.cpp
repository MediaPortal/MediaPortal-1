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

#include "StdAfx.h"

#include "Brand.h"

CBrand::CBrand(void)
{
  this->brand = 0;
  this->brandString = NULL;
}

CBrand::~CBrand(void)
{
  FREE_MEM(this->brandString);
}

/* get methods */

uint32_t CBrand::GetBrand(void)
{
  return this->brand;
}

const wchar_t *CBrand::GetBrandString(void)
{
  return this->brandString;
}

/* set methods */

bool CBrand::SetBrand(uint32_t brand)
{
  this->brand = brand;
  return this->ConvertBrandToString();
}

bool CBrand::SetBrandString(const wchar_t *brandString)
{
  FREE_MEM(this->brandString);
  this->brandString = Duplicate(brandString);
  return this->ConvertBrandToUnsignedInteger();
}

/* other methods */

bool CBrand::ConvertBrandToString(void)
{
  ALLOC_MEM_DEFINE_SET(buffer, char, 5, 0);
  bool result = (buffer != NULL);

  if (result)
  {
    FREE_MEM(this->brandString);

    buffer[0] = (this->brand & 0xFF000000) >> 24;
    buffer[1] = (this->brand & 0x00FF0000) >> 16;
    buffer[2] = (this->brand & 0x0000FF00) >> 8;
    buffer[3] = (this->brand & 0x000000FF);

    this->brandString = ConvertToUnicodeA(buffer);
    result &= (this->brandString != NULL);
  }

  FREE_MEM(buffer);

  return result;
}

bool CBrand::ConvertBrandToUnsignedInteger(void)
{
  char *buffer = ConvertToMultiByteW(this->brandString);
  bool result = (buffer != NULL);

  if (result)
  {
    uint32_t length = strlen(buffer);
    this->brand = 0;

    for (uint32_t i = 0; i < 4; i++)
    {
      this->brand <<= 8;
      this->brand += (i < length) ? (uint8_t)buffer[i] : 0;
    }
  }

  FREE_MEM(buffer);

  return result;
}
