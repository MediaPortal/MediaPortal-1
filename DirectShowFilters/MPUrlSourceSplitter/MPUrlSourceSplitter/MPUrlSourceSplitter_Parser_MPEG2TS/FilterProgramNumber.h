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

#ifndef __FILTER_PROGRAM_NUMBER_DEFINED
#define __FILTER_PROGRAM_NUMBER_DEFINED

#include "ProgramElementCollection.h"

class CFilterProgramNumber
{
public:
  CFilterProgramNumber(HRESULT *result, uint16_t programNumber);
  ~CFilterProgramNumber();

  /* get methods */

  // gets program number to filter program elements
  // @return : program number to filter program elements
  uint16_t GetProgramNumber(void);

  // gets collection of program elements to leave in transport stream program map
  // @return : collection of program elements to leave in transport stream program map
  CProgramElementCollection *GetLeaveProgramElements(void);

  /* set methods */

  /* other methods */

protected:
  // holds program number
  uint16_t programNumber;
  // holds program elements to leave in transport stream program map
  CProgramElementCollection *leaveProgramElements;

  /* methods */
};

#endif