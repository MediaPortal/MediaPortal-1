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

#include "Crc32.h"

/* get methods */

/* set methods */

/* other methods */

/* static methods */

uint32_t CCrc32::Calculate(const uint8_t *data, uint32_t length)
{
  uint32_t result = 0xFFFFFFFF;

  if (data != NULL)
  {
    for (uint32_t i = 0; i < length; i++)
    {
      result = (result << 8) ^ CRC32_TABLE[((result >> 24) ^ data[i]) & 0xFF];
    }
  }

  return result;
}

/* protected methods */

CCrc32::CCrc32(void)
{
}

CCrc32::~CCrc32(void)
{
}
