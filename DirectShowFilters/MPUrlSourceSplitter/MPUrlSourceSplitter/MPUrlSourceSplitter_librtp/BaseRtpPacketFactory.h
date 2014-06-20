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

#ifndef __BASE_RTP_PACKET_FACTORY_DEFINED
#define __BASE_RTP_PACKET_FACTORY_DEFINED

#include "BaseRtpPacket.h"

#define CREATE_SPECIFIC_PACKET(packetType, buffer, length, continueParsing, result, position)                       \
                                                                                                                    \
if (SUCCEEDED(continueParsing) && (result == NULL))                                                                 \
{                                                                                                                   \
  packetType *packet = new packetType(&continueParsing);                                                            \
  CHECK_POINTER_HRESULT(continueParsing, packet, continueParsing, E_OUTOFMEMORY);                                   \
                                                                                                                    \
  if (SUCCEEDED(continueParsing))                                                                                   \
  {                                                                                                                 \
    if (packet->Parse(buffer, length))                                                                              \
    {                                                                                                               \
      position = packet->GetSize();                                                                                 \
      result = packet;                                                                                              \
    }                                                                                                               \
  }                                                                                                                 \
                                                                                                                    \
  if ((FAILED(continueParsing)) || (result == NULL))                                                                \
  {                                                                                                                 \
    FREE_MEM_CLASS(packet);                                                                                         \
    position = 0;                                                                                                   \
  }                                                                                                                 \
}


class CBaseRtpPacketFactory
{
public:
  // initializes a new instance of CBaseRtpPacketFactory class
  CBaseRtpPacketFactory(void);
  virtual ~CBaseRtpPacketFactory(void);

  // creates base RTP packet from buffer
  // @param buffer : buffer with base RTP packet data for parsing
  // @param length : the length of data in buffer
  // @param position : pointer to position after parsing
  // @return : base RTP packet or NULL if error
  virtual CBaseRtpPacket *CreateBaseRtpPacket(const unsigned char *buffer, unsigned int length, unsigned int *position);
};

#endif