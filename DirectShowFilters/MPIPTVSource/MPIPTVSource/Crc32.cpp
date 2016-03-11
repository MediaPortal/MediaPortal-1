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


Crc32::Crc32(void)
{
  // create CRC32 table
  this->Crc32Table = ALLOC_MEM_SET(this->Crc32Table, unsigned int, 256, 0);
  if (this->Crc32Table != NULL)
  {
    for (unsigned int i = 0; i < 256; i++)
    {
      unsigned int k = 0;
      for (unsigned int j = (i << 24) | 0x00800000; j != 0x80000000; j <<= 1)
      {
        k = (k << 1) ^ ((((k ^ j) & 0x80000000) != 0) ? (unsigned int)0x04C11DB7 : 0);
      }
      this->Crc32Table[i] = k;
    }
  }
}

Crc32::~Crc32(void)
{
  FREE_MEM(this->Crc32Table);
}

unsigned int Crc32::Compute(const char *data, unsigned int length)
{
  unsigned int result = 0xFFFFFFFF;
  if (data != NULL)
  {
    for (unsigned int i = 0; i < length; i++)
    {
      result = (result << 8) ^ this->Crc32Table[((result >> 24) ^ (unsigned char)data[i]) & 0xFF];
    }
  }

  return result;
}