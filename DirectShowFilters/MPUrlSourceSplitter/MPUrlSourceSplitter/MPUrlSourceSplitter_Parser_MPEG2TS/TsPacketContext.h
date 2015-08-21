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

#ifndef __TS_PACKET_CONTEXT_DEFINED
#define __TS_PACKET_CONTEXT_DEFINED

#include "Flags.h"

#define TS_PACKET_CONTEXT_FLAG_NONE                                     FLAGS_NONE

#define TS_PACKET_CONTEXT_FLAG_LAST                                     (FLAGS_LAST + 0)

#define TS_PACKET_INDEX_NOT_SET                                         UINT_MAX

class CTsPacketContext : public CFlags
{
public:
  CTsPacketContext(HRESULT *result);
  virtual ~CTsPacketContext(void);

  /* get methods */

  // gets TS packet index
  // @return : TS packet index or TS_PACKET_INDEX_NOT_SET is not set
  unsigned int GetTsPacketIndex(void);

  // gets section payload count in TS packet
  // @return : section payload count
  unsigned int GetSectionPayloadCount(void);

  /* set methods */

  // sets TS packet index
  // @param tsPacketIndex : the TS packet index to set
  void SetTsPacketIndex(unsigned int tsPacketIndex);

  // sets section payload count in TS packet
  // @param sectionPayloadCount : the section payload count to set
  void SetSectionPayloadCount(unsigned int sectionPayloadCount);

  /* other methods */

protected:
  // holds TS packet index
  unsigned int tsPacketIndex;
  // holds section payload count
  unsigned int sectionPayloadCount;

  /* methods */
};

#endif