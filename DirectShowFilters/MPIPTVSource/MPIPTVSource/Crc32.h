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

#ifndef __CRC32_DEFINED
#define __CRC32_DEFINED

#include "MPIPTVSourceExports.h"

class MPIPTVSOURCE_API Crc32
{
public:
  Crc32(void);
  ~Crc32(void);

  // computes CRC32 of specified data
  // @param data : the pointer to data
  // @param length : the length of data
  // @return : the CRC32 of data
  unsigned int Compute(const char *data, unsigned int length);
private:
  // CRC32 table 
  unsigned int *Crc32Table;
};

#endif