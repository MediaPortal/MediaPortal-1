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

#include "FreeSpace.h"

CFreeSpace::CFreeSpace(HRESULT *result)
{
  this->start = 0;
  this->length = 0;
}

CFreeSpace::~CFreeSpace(void)
{
}

/* get methods */

int64_t CFreeSpace::GetStart(void)
{
  return this->start;
}

int64_t CFreeSpace::GetLength(void)
{
  return this->length;
}

/* set methods */

void CFreeSpace::SetStart(int64_t start)
{
  this->start = start;
}

void CFreeSpace::SetLength(int64_t length)
{
  this->length = length;
}

/* other methods */
