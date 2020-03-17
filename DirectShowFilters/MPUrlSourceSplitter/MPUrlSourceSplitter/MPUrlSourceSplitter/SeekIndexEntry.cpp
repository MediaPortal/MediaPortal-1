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

#include "SeekIndexEntry.h"

CSeekIndexEntry::CSeekIndexEntry(int64_t position, int64_t timestamp)
{
  this->position = position;
  this->timestamp = timestamp;
}

CSeekIndexEntry::~CSeekIndexEntry(void)
{
}

/* get methods */

int64_t CSeekIndexEntry::GetPosition(void)
{
  return this->position;
}

int64_t CSeekIndexEntry::GetTimestamp(void)
{
  return this->timestamp;
}

/* set methods */

void CSeekIndexEntry::SetPosition(int64_t position)
{
  this->position = position;
}

void CSeekIndexEntry::SetTimestamp(int64_t timestamp)
{
  this->timestamp = timestamp;
}

/* other methods */