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

#include "stdafx.h"

#include "FilterProgramNumber.h"

CFilterProgramNumber::CFilterProgramNumber(HRESULT *result, uint16_t programNumber)
{
  this->leaveProgramElements = NULL;
  this->programNumber = programNumber;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->leaveProgramElements = new CProgramElementCollection(result);

    CHECK_POINTER_HRESULT(*result, this->leaveProgramElements, *result, E_OUTOFMEMORY);
  }
}

CFilterProgramNumber::~CFilterProgramNumber()
{
  FREE_MEM_CLASS(this->leaveProgramElements);
}

/* get methods */

uint16_t CFilterProgramNumber::GetProgramNumber(void)
{
  return this->programNumber;
}

CProgramElementCollection *CFilterProgramNumber::GetLeaveProgramElements(void)
{
  return this->leaveProgramElements;
}

/* set methods */

/* other methods */

/* protected methods */
