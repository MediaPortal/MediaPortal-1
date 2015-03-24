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

#ifndef __FREE_SPACE_DEFINED
#define __FREE_SPACE_DEFINED

#include <stdint.h>

class CFreeSpace
{
public:
  CFreeSpace(HRESULT *result);
  ~CFreeSpace(void);

  /* get methods */

  // gets start byte of free space
  // @return : start byte of free space
  int64_t GetStart(void);

  // gets length of free space
  // @return : length of free space
  int64_t GetLength(void);

  /* set methods */

  // sets start byte of free space
  // @param start : the start byte of free space to set
  void SetStart(int64_t start);

  // sets length free space
  // @param length : the length of free space to set
  void SetLength(int64_t length);

  /* other methods */

protected:
  // holds start byte of free space
  int64_t start;

  // holds length of free space
  int64_t length;
};

#endif