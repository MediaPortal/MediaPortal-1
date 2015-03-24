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

#ifndef __PROGRAM_ELEMENT_DEFINED
#define __PROGRAM_ELEMENT_DEFINED

#define PROGRAM_ELEMENT_PID_NOT_SET                                   UINT_MAX

class CProgramElement
{
public:
  CProgramElement(HRESULT *result, unsigned int pid);
  ~CProgramElement(void);

  /* get methods */

  // gets program element PID
  // @return : program element PID or PROGRAM_ELEMENT_PID_NOT_SET if not set
  unsigned int GetPID(void);

  /* set methods */

  // sets program element PID
  // @param pid : program element PID to set
  void SetPID(unsigned int pid);

  /* other methods */

protected:
  // holds program element PID
  unsigned int pid;

  /* methods */
};

#endif