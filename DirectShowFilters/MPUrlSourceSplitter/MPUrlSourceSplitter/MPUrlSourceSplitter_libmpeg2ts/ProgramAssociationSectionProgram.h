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

#ifndef __PROGRAM_ASSOCIATION_SECTION_PROGRAM_DEFINED
#define __PROGRAM_ASSOCIATION_SECTION_PROGRAM_DEFINED

class CProgramAssociationSectionProgram
{
public:
  CProgramAssociationSectionProgram(HRESULT *result);
  ~CProgramAssociationSectionProgram(void);

  /* get methods */

  // gets program number
  // @return : program number
  unsigned int GetProgramNumber(void);

  // holds program map PID
  // @return : program map PID
  unsigned int GetProgramMapPID(void);

  /* set methods */

  // sets program number
  // @param programNumber : the program number to set
  void SetProgramNumber(unsigned int programNumber);

  // sets program map PID
  // @param programMapPID : the program map PID to set
  void SetProgramMapPID(unsigned int programMapPID);

  /* other methods */

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CProgramAssociationSectionProgram *Clone(void);

protected:

  // holds program number
  unsigned int programNumber;
  // holds network PID or program map PID
  unsigned int programMapPID;

  /* methods */
};

#endif