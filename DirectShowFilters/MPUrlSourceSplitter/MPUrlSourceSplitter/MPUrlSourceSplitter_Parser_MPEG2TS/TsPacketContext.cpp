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
   this->tsPacketIndex = TS_PACKET_INDEX_NOT_SET;
   this->sectionPayloadCount = 0;
}

CTsPacketContext::~CTsPacketContext(void)
{
 }

/* get methods */

unsigned int CTsPacketContext::GetTsPacketIndex(void)
{
  return this->tsPacketIndex;
}

unsigned int CTsPacketContext::GetSectionPayloadCount(void)
{
  return this->sectionPayloadCount;
}

/* set methods */

void CTsPacketContext::SetTsPacketIndex(unsigned int tsPacketIndex)
{
  this->tsPacketIndex = tsPacketIndex;
}

void CTsPacketContext::SetSectionPayloadCount(unsigned int sectionPayloadCount)
{
  this->sectionPayloadCount = sectionPayloadCount;
}

/* other methods */

/* protected methods */