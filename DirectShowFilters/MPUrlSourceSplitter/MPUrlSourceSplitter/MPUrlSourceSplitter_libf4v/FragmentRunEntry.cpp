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

#include "FragmentRunEntry.h"

CFragmentRunEntry::CFragmentRunEntry(HRESULT *result, uint32_t firstFragment, uint64_t firstFragmentTimestamp, uint32_t fragmentDuration, uint32_t cumulatedFragmentCount)
  : CFlags()
{
  this->firstFragment = firstFragment;
  this->firstFragmentTimestamp = firstFragmentTimestamp;
  this->fragmentDuration = fragmentDuration;
  this->flags = FRAGMENT_RUN_ENTRY_FLAG_NONE;
  this->cumulatedFragmentCount = cumulatedFragmentCount;
}

CFragmentRunEntry::~CFragmentRunEntry(void)
{
}

/* get methods */

uint32_t CFragmentRunEntry::GetFirstFragment(void)
{
  return this->firstFragment;
}

uint64_t CFragmentRunEntry::GetFirstFragmentTimestamp(void)
{
  return this->firstFragmentTimestamp;
}

uint32_t CFragmentRunEntry::GetFragmentDuration(void)
{
  return this->fragmentDuration;
}

uint32_t CFragmentRunEntry::GetCumulatedFragmentCount(void)
{
  return this->cumulatedFragmentCount;
}

/* set methods */

/* other methods */

bool CFragmentRunEntry::IsEndOfPresentation(void)
{
  return this->IsSetFlags(FRAGMENT_RUN_ENTRY_FLAG_END_OF_PRESENTATION);
}

bool CFragmentRunEntry::IsDiscontinuityFragmentNumbering(void)
{
  return this->IsSetFlags(FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_FRAGMENT_NUMBERING);
}

bool CFragmentRunEntry::IsDiscontinuityTimestamps(void)
{
  return this->IsSetFlags(FRAGMENT_RUN_ENTRY_FLAG_DISCONTINUITY_TIMESTAMPS);
}

/* protected methods */