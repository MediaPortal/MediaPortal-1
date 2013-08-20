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

#include "ReceiveData.h"

CReceiveData::CReceiveData(void)
{
  this->endOfStreamReached = new CEndOfStreamReached();
  this->totalLength = new CSetTotalLength();
  this->mediaPackets = new CMediaPacketCollection();
}

CReceiveData::~CReceiveData(void)
{
  FREE_MEM_CLASS(this->endOfStreamReached);
  FREE_MEM_CLASS(this->totalLength);
  FREE_MEM_CLASS(this->mediaPackets);
}

/* get methods */

CSetTotalLength *CReceiveData::GetTotalLength(void)
{
  return this->totalLength;
}

CMediaPacketCollection *CReceiveData::GetMediaPacketCollection(void)
{
  return this->mediaPackets;
}

CEndOfStreamReached *CReceiveData::GetEndOfStreamReached(void)
{
  return this->endOfStreamReached;
}

/* set methods */

/* other methods */

void CReceiveData::Clear(void)
{
  this->endOfStreamReached->Clear();
  this->mediaPackets->Clear();
  this->totalLength->Clear();
}