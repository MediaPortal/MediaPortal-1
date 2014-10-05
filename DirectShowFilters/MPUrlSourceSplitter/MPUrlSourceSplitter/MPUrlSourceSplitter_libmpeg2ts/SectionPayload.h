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

#ifndef __SECTION_PAYLOAD_DEFINED
#define __SECTION_PAYLOAD_DEFINED

#include "Flags.h"

#define SECTION_PAYLOAD_FLAG_NONE                                     FLAGS_NONE

#define SECTION_PAYLOAD_FLAG_PAYLOAD_UNIT_START                       (1 << (FLAGS_LAST + 0))

#define SECTION_PAYLOAD_FLAG_LAST                                     (FLAGS_LAST + 1)

class CSectionPayload : public CFlags
{
public:
  CSectionPayload(HRESULT *result, const uint8_t *payload, uint32_t payloadSize, bool payloadUnitStart);
  ~CSectionPayload(void);

  /* get methods */

  // gets payload
  // @return : payload
  const uint8_t *GetPayload(void);

  // gets payload size
  // @return : payload size
  uint32_t GetPayloadSize(void);

  /* set methods */

  /* other methods */

  // tests if payload unit start is set
  // @return : true if payload unit start is set, false otherwise
  bool IsPayloadUnitStart(void);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CSectionPayload *Clone(void);

protected:

  // holds reference to payload and payload size
  const uint8_t *payload;
  uint32_t payloadSize;

  /* methods */
};

#endif