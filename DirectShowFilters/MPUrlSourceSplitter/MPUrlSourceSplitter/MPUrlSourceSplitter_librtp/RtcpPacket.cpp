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
  this->packetType = UINT_MAX;
  this->packetValue = UINT_MAX;
  this->payload = NULL;
  this->payloadLength = 0;
}

CRtcpPacket::~CRtcpPacket(void)
{
}

/* get methods */

unsigned int CRtcpPacket::GetPacketValue(void)
{
  return this->packetValue;
}

unsigned int CRtcpPacket::GetPacketType(void)
{
  return this->packetType;
}

const unsigned char *CRtcpPacket::GetPayload(void)
{
  return this->payload;
}

unsigned int CRtcpPacket::GetPayloadLength(void)
{
  return this->payloadLength;
}

/* set methods */

/* other methods */

void CRtcpPacket::Clear(void)
{
  __super::Clear();

  this->packetType = UINT_MAX;
  this->packetValue = UINT_MAX;
  FREE_MEM(this->payload);
  this->payloadLength = 0;
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

    this->packetValue = RBE8(buffer, position) & 0x1F;
    position++;

    RBE8INC(buffer, position, this->packetType);
    RBE16INC(buffer, position, this->size);
    this->size = (this->size + 1) * 4;

    // do some base checks
    result &= (this->version == RTCP_PACKET_VERSION);
    result &= (length >= this->size);

    if (result)
    {
      if ((this->flags & FLAG_RTCP_PACKET_PADDING) != 0)
      {
        this->paddingLength = RBE8(buffer, this->size - 1) & 0xFF;
        result &= (this->size >= (position + this->paddingLength));
      }
    }

    if (result)
    {
      // from position to this->size - this->paddingLength is payload
      this->payloadLength = this->size - this->paddingLength - position;
      if (this->payloadLength > 0)
      {
        this->payload = ALLOC_MEM_SET(this->payload, unsigned char, this->payloadLength, 0);
        result &= (this->payload != NULL);

        if (result)
        {
          memcpy(this->payload, buffer + position, this->payloadLength);
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

