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

#ifndef __DISCONTINUITY_PARSER_DEFINED
#define __DISCONTINUITY_PARSER_DEFINED

#include "Parser.h"
#include "PidCounterCollection.h"

#define DISCONTINUITY_PARSER_FLAG_NONE                                PARSER_FLAG_NONE

#define DISCONTINUITY_PARSER_FLAG_DISCONTINUITY                       (1 << (PARSER_FLAG_LAST + 0))

#define DISCONTINUITY_PARSER_FLAG_LAST                                (PARSER_FLAG_LAST + 1)

#define DISCONTINUITY_PID_NOT_SPECIFIED                               0xFFFF

class CDiscontinuityParser : public CParser
{
public:
  CDiscontinuityParser(HRESULT *result);
  virtual ~CDiscontinuityParser(void);

  /* get methods */

  // gets last discontinuity PID
  // @return : last discontinuity PID or DISCONTINUITY_PID_NOT_SPECIFIED if none
  unsigned int GetLastDiscontinuityPid(void);

  // gets counter value for GetLastDiscontinuityPid() when discontinuity in counter occurred
  // @return : counter value or PID_COUNTER_NOT_SPECIFIED if none
  uint8_t GetLastDiscontinuityCounter(void);

  // gets expected counter value for GetLastDiscontinuityPid() when discontinuity in counter occurred
  // @return : expected counter value or PID_COUNTER_NOT_SPECIFIED if none
  uint8_t GetLastExpectedCounter(void);

  /* set methods */

  /* other methods */

  // tests if discontinuity occurred
  // @return : true if discontinuity occurred, false otherwise
  bool IsDiscontinuity(void);

  // parses input MPEG2 TS packet
  // @param packet : the MPEG2 TS packet to parse
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT Parse(CTsPacket *packet);

  // clears instance to its default state
  virtual void Clear(void);

protected:

  // holds current and last PID counter for each PID
  CPidCounterCollection *pidCounters;

  unsigned int lastDiscontinuityPid;
  uint8_t lastDiscontinuityCounter;
  uint8_t lastExpectedCounter;

  /* methods */
};

#endif