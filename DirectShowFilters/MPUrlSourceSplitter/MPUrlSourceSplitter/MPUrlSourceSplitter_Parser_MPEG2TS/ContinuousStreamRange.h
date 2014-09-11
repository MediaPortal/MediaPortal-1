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

#ifndef __CONTINUOUS_STREAM_RANGE_DEFINED
#define __CONTINUOUS_STREAM_RANGE_DEFINED

#include "Flags.h"

#define CONTINUOUS_STREAM_RANGE_FLAG_NONE                             FLAGS_NONE

#define CONTINUOUS_STREAM_RANGE_FLAG_LAST                             (FLAGS_LAST + 0)

class CContinuousStreamRange : public CFlags
{
public:
  CContinuousStreamRange(HRESULT *result);
  virtual ~CContinuousStreamRange(void);

  /* get methods */

  // gets filter start position
  // @return : filter start position
  int64_t GetFilterStartPosition(void);

  // gets protocol start position
  // @return : protocol start position
  int64_t GetProtocolStartPosition(void);

  // gets stream length
  // @return : stream length
  int64_t GetStreamLength(void);

  // gets last TS packet available length
  // @return : last TS packet available length
  int64_t GetLastPacketAvailableLength(void);

  // gets last TS packet missing length (length of missing data, if any)
  // @return : last TS packet missing data length
  int64_t GetLastPacketMissingLength(void);

  /* set methods */

  // sgets filter start position
  // @param filterStartPosition : filter start position
  void SetFilterStartPosition(int64_t filterStartPosition);

  // sets protocol start position
  // @param protocolStartPosition : protocol start position
  void SetProtocolStartPosition(int64_t protocolStartPosition);

  // sets stream length
  // param streamLength : stream length
  void SetStreamLength(int64_t streamLength);

  /* other methods */

protected:
  // holds filter start prosition
  int64_t filterStartPosition;
  // holds protocol start position
  int64_t protocolStartPosition;
  // holds stream length
  int64_t streamLength;

  /* methods */
};

#endif