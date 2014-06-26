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

#include "SessionTime.h"

CSessionTime::CSessionTime(HRESULT *result)
{
  this->startTime = 0;
  this->stopTime = 0;
  this->repeatIntervals = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->repeatIntervals = new CRepeatIntervalCollection(result);
    CHECK_POINTER_HRESULT(*result, this->repeatIntervals, *result, E_OUTOFMEMORY);
  }
}

CSessionTime::~CSessionTime(void)
{
  FREE_MEM_CLASS(this->repeatIntervals);
}

/* get methods */

uint64_t CSessionTime::GetStartTime(void)
{
  return this->startTime;
}

uint64_t CSessionTime::GetStopTime(void)
{
  return this->stopTime;
}

CRepeatIntervalCollection *CSessionTime::GetRepeatIntervals(void)
{
  return this->repeatIntervals;
}

/* set methods */

/* other methods */