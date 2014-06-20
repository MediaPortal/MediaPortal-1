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

#ifndef __FLAGS_DEFINED
#define __FLAGS_DEFINED

#include <stdint.h>

#define FLAGS_NONE                                                    0x0000000000000000

#define FLAGS_LAST                                                    0

class CFlags
{
public:
  CFlags(void);
  virtual ~CFlags(void);

  /* get methods */

  // gets combination of set flags
  // @return : combination of set flags
  virtual uint64_t GetFlags(void);

  /* set methods */

  // sets combination of flags
  // @param flags : the combination of flags to set
  virtual void SetFlags(uint64_t flags);

  /* other methods */

  // tests if specific combination of flags is set
  // @param flags : the set of flags to test
  // @return : true if set of flags is set, false otherwise
  virtual bool IsSetFlags(uint64_t flags);

protected:
  // holds various flags
  uint64_t flags;

  /* methods */
};

#endif