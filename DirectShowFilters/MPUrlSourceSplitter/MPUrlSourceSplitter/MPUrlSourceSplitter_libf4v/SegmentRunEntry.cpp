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

#include "SegmentRunEntry.h"

CSegmentRunEntry::CSegmentRunEntry(HRESULT *result, uint32_t firstSegment, uint32_t fragmentsPerSegment)
{
  this->firstSegment = firstSegment;
  this->fragmentsPerSegment = fragmentsPerSegment;
}

CSegmentRunEntry::~CSegmentRunEntry(void)
{
}

uint32_t CSegmentRunEntry::GetFirstSegment(void)
{
  return this->firstSegment;
}

uint32_t CSegmentRunEntry::GetFragmentsPerSegment(void)
{
  return this->fragmentsPerSegment;
}