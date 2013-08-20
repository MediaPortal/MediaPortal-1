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

#pragma once

#ifndef __RECEIVE_DATA_DEFINED
#define __RECEIVE_DATA_DEFINED

#include "SetTotalLength.h"
#include "MediaPacketCollection.h"
#include "EndOfStreamReached.h"

class CReceiveData
{
public:
  CReceiveData(void);
  ~CReceiveData(void);

  /* get methods */

  // gets total length
  // @return : total length
  CSetTotalLength *GetTotalLength(void);

  // gets received media packets
  // @return : media packet collection
  CMediaPacketCollection *GetMediaPacketCollection(void);

  // gets end of stream reached
  // @return : end of stream reached
  CEndOfStreamReached *GetEndOfStreamReached(void);

  /* set methods */

  /* other methods */

  // clears current instance to default state
  void Clear(void);

private:

  // holds total length
  CSetTotalLength *totalLength;

  // holds received media packets
  CMediaPacketCollection *mediaPackets;

  // holds end of stream reached
  CEndOfStreamReached *endOfStreamReached;
};

#endif