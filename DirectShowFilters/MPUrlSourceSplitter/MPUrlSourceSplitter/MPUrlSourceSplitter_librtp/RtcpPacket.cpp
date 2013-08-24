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

#include "RtcpPacket.h"
#include "BufferHelper.h"

#include <stdint.h>

CRtcpPacket::CRtcpPacket(void)
  : CBaseRtpPacket()
{
  this->packetType = 0;
  this->packetValue = 0;
}

CRtcpPacket::~CRtcpPacket(void)
{
}

/* get methods */

unsigned int CRtcpPacket::GetSize(void)
{
  // for creating packets we assume that derived RTCP packets has payload length zero and payload is NULL, same with padding
  // but padding can be set after parsing
  return (this->paddingSize + RTCP_PACKET_HEADER_SIZE);
}

bool CRtcpPacket::GetPacket(unsigned char *buffer, unsigned int length)
{
  unsigned int size = this->GetSize();
  bool result = ((buffer != NULL) && (length >= size));

  if (result)
  {
    // write RTCP packet header
    unsigned int position = 0;

    unsigned int header = (RTCP_PACKET_VERSION << 6) | (this->GetPacketValue() & 0x0000001F);
    WBE8INC(buffer, position, header);
    WBE8INC(buffer, position, this->GetPacketType());
    WBE16INC(buffer, position, ((size / 4) - 1));

    CHECK_CONDITION_NOT_NULL_EXECUTE(this->payload, memcpy(buffer + position, this->payload, this->payloadSize));
  }

  return result;
}

/* set methods */

/* other methods */

void CRtcpPacket::Clear(void)
{
  __super::Clear();

  this->packetType = 0;
  this->packetValue = 0;
}

bool CRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = ((buffer != NULL) && (length >= RTCP_PACKET_HEADER_SIZE));

  if (result)
  {
    result &= __super::Parse(buffer, length);
  }

  if (result)
  {
    // RTCP packet header is at least RTCP_PACKET_HEADER_SIZE long

    // parse packet value from first byte
    unsigned int position = 0;
    unsigned int size = 0;

    this->packetValue = RBE8(buffer, position) & 0x1F;
    position++;

    RBE8INC(buffer, position, this->packetType);
    RBE16INC(buffer, position, size);
    size = (size + 1) * 4;

    // do some base checks
    result &= (this->version == RTCP_PACKET_VERSION);
    result &= (length >= size);

    if (result)
    {
      if ((this->flags & FLAG_RTCP_PACKET_PADDING) != 0)
      {
        this->paddingSize = RBE8(buffer, size - 1) & 0xFF;
        result &= (size >= (position + this->paddingSize));
      }
    }

    if (result)
    {
      // from position to size - this->paddingSize is payload
      this->payloadSize = size - this->paddingSize - position;
      if (this->payloadSize > 0)
      {
        this->payload = ALLOC_MEM_SET(this->payload, unsigned char, this->payloadSize, 0);
        result &= (this->payload != NULL);

        if (result)
        {
          memcpy(this->payload, buffer + position, this->payloadSize);
        }
      }
    }

    if (result)
    {
      this->baseType = RTCP_PACKET_BASE_TYPE;
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}

/* protected methods */

/* get methods */

/* set methods */

/* other methods */