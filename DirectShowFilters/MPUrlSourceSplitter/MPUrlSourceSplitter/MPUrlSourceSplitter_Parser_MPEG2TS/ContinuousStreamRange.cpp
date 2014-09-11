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

#include "ContinuousStreamRange.h"
#include "TsPacket.h"

CContinuousStreamRange::CContinuousStreamRange(HRESULT *result)
  : CFlags()
{
  this->filterStartPosition = 0;
  this->protocolStartPosition = 0;
  this->streamLength = 0;

  /*if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }*/
}

CContinuousStreamRange::~CContinuousStreamRange(void)
{
}

/* get methods */

int64_t CContinuousStreamRange::GetFilterStartPosition(void)
{
  return this->filterStartPosition;
}

int64_t CContinuousStreamRange::GetProtocolStartPosition(void)
{
  return this->protocolStartPosition;
}

int64_t CContinuousStreamRange::GetStreamLength(void)
{
  return this->streamLength;
}

int64_t CContinuousStreamRange::GetLastPacketAvailableLength(void)
{
  int64_t result = this->streamLength % TS_PACKET_SIZE;
  return (result == 0) ? TS_PACKET_SIZE : result;
}

int64_t CContinuousStreamRange::GetLastPacketMissingLength(void)
{
  int64_t result = TS_PACKET_SIZE - (this->streamLength % TS_PACKET_SIZE);
  return (result == TS_PACKET_SIZE) ? 0 : result;
}

/* set methods */

void CContinuousStreamRange::SetFilterStartPosition(int64_t filterStartPosition)
{
  this->filterStartPosition = filterStartPosition;
}

void CContinuousStreamRange::SetProtocolStartPosition(int64_t protocolStartPosition)
{
  this->protocolStartPosition = protocolStartPosition;
}

void CContinuousStreamRange::SetStreamLength(int64_t streamLength)
{
  this->streamLength = streamLength;
}

/* other methods */

/* protected methods */