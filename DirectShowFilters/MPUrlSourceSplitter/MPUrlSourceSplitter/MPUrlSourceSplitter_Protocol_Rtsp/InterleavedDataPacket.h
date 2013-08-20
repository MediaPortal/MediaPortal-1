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

#ifndef __INTERLEAVED_DATA_PACKET_DEFINED
#define __INTERLEAVED_DATA_PACKET_DEFINED

#include "BaseRtpPacketCollection.h"

#define DATA_SECTION_OFFSET                                 4
#define INTERLEAVED_PACKET_HEADER_IDENTIFIER                0x24

#define INTERLEAVED_MAX_PACKET_LENGTH                       UINT16_MAX
#define CHANNEL_IDENTIFIER_UNSPECIFED                       UINT_MAX

class CInterleavedDataPacket
{
public:
  // initializes a new instance of CInterleavedDataPacket class
  CInterleavedDataPacket(void);
  ~CInterleavedDataPacket(void);

  /* get methods */

  // gets whole packet size
  // @return : whole packet size
  uint32_t GetPacketSize(void);

  // gets channel identifier of interleaved packet
  // @return : channel identifier or CHANNEL_IDENTIFIER_UNSPECIFED if not specified
  uint32_t GetChannelIdentifier(void);

  // gets base RTP packets
  // @return : base RTP packets
  CBaseRtpPacketCollection *GetBaseRtpPackets(void);

  /* set methods */

  /* other methods */

  // parses buffer for interleaved packet
  // @param buffer : the buffer to parse for interleaved packet
  // @param bufferSize : the size of buffer to parse
  // @return :
  // 1. size of RTSP response if successful (zero for no RTSP response in buffer)
  // 2. HRESULT_FROM_WIN32(ERROR_MORE_DATA) if not enough data
  // 3. HRESULT_FROM_WIN32(ERROR_INVALID_DATA) if invalid data in buffer
  // 4. error code otherwise
  HRESULT Parse(const uint8_t *buffer, uint32_t bufferSize);

  // clears current instance for further use
  void Clear(void);

protected:

  // holds whole packet size
  uint32_t packetSize;

  // holds channel identifier
  uint32_t channelIdentifier;

  // holds base RTP packets
  CBaseRtpPacketCollection *baseRtpPackets;
};

#endif