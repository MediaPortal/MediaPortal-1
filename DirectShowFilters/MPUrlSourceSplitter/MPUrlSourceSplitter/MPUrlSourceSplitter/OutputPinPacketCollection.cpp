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

#include "OutputPinPacketCollection.h"

COutputPinPacketCollection::COutputPinPacketCollection(void)
  : CKeyedCollection()
{
}

COutputPinPacketCollection::~COutputPinPacketCollection(void)
{
}

int COutputPinPacketCollection::CompareItemKeys(REFERENCE_TIME firstKey, REFERENCE_TIME secondKey, void *context)
{
  if (firstKey < secondKey)
  {
    return (-1);
  }
  else if (firstKey == secondKey)
  {
    return 0;
  }
  else
  {
    return 1;
  }
}

int64_t COutputPinPacketCollection::GetKey(COutputPinPacket *item)
{
  return item->GetStartTime();
}

COutputPinPacket *COutputPinPacketCollection::Clone(COutputPinPacket *item)
{
  return NULL;
}