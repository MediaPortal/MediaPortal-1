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

#include "TsPacketContext.h"

CTsPacketContext::CTsPacketContext(HRESULT *result)
  : CFlags()
{
  this->sectionContext = NULL;
  this->tsPacketIndex = TS_PACKET_INDEX_NOT_SET;
}

CTsPacketContext::~CTsPacketContext(void)
{
  CHECK_CONDITION_EXECUTE(this->IsSectionContextOwner(), FREE_MEM_CLASS(this->sectionContext));
}

/* get methods */

unsigned int CTsPacketContext::GetTsPacketIndex(void)
{
  return this->tsPacketIndex;
}

CSectionContext *CTsPacketContext::GetSectionContext(void)
{
  return this->sectionContext;
}

/* set methods */

void CTsPacketContext::SetTsPacketIndex(unsigned int tsPacketIndex)
{
  this->tsPacketIndex = tsPacketIndex;
}

bool CTsPacketContext::SetSectionContext(CSectionContext *sectionContext)
{
  CHECK_CONDITION_EXECUTE(this->IsSectionContextOwner(), FREE_MEM_CLASS(this->sectionContext));
  this->sectionContext = sectionContext;

  return true;
}

void CTsPacketContext::SetSectionContextOwner(bool isSectionOwner)
{
  this->flags &= ~TS_PACKET_CONTEXT_FLAG_SECTION_CONTEXT_OWNER;
  this->flags |= isSectionOwner ? TS_PACKET_CONTEXT_FLAG_SECTION_CONTEXT_OWNER : TS_PACKET_CONTEXT_FLAG_NONE;
}

/* other methods */

bool CTsPacketContext::IsSectionContextOwner(void)
{
  return this->IsSetFlags(TS_PACKET_CONTEXT_FLAG_SECTION_CONTEXT_OWNER);
}

/* protected methods */