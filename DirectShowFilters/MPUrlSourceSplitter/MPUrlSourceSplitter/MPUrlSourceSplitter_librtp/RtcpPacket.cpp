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
  this->payloadSize = 0;
}

CRtcpPacket::~CRtcpPacket(void)
{
  FREE_MEM(this->payload);
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

unsigned int CRtcpPacket::GetPayloadSize(void)
{
  return this->payloadSize;
}

unsigned int CRtcpPacket::GetPacketSize(void)
{
  // for creating packets we assume that derived RTCP packets has payload length zero and payload is NULL
  return (this->GetPayloadSize() + RTCP_PACKET_HEADER_SIZE);
}

bool CRtcpPacket::GetPacket(unsigned char *buffer, unsigned int length)
{
  unsigned int size = this->GetPacketSize();
  bool result = ((buffer != NULL) && (length >= size));

  if (result)
  {
    // write RTCP packet header
    unsigned int position = 0;

    unsigned int header = (RTCP_PACKET_VERSION << 6) + this->GetPacketValue();
    WBE8INC(buffer, position, header);
    WBE8INC(buffer, position, this->GetPacketType());
    WBE16INC(buffer, position, ((size / 4) - 1));

    CHECK_CONDITION_NOT_NULL_EXECUTE(this->GetPayload(), memcpy(buffer + position, this->GetPayload(), this->GetPayloadSize()));
  }

  return result;
}

/* set methods */

void CRtcpPacket::SetPacketValue(unsigned int packetValue)
{
  this->packetValue = packetValue;
}

void CRtcpPacket::SetPacketType(unsigned int packetType)
{
  this->packetType = packetType;
}

bool CRtcpPacket::SetPayload(const unsigned char *payload, unsigned int payloadLength)
{
  bool result = true;
  FREE_MEM(this->payload);
  this->payloadSize = payloadLength;

  // payload length + RTCP_PACKET_HEADER_SIZE must be multiple of four
  result &= (((this->payloadSize + RTCP_PACKET_HEADER_SIZE) % 4) == 0);

  if (this->payloadSize != 0)
  {
    result &= (payload != NULL);

    if (result)
    {
      this->payload = ALLOC_MEM_SET(this->payload, unsigned char, this->payloadSize, 0);
      result = (this->payload != NULL);

      if (result)
      {
        memcpy(this->payload, payload, this->payloadSize);
      }
    }
  }

  return result;
}

/* other methods */

void CRtcpPacket::Clear(void)
{
  __super::Clear();

  this->packetType = UINT_MAX;
  this->packetValue = UINT_MAX;
  FREE_MEM(this->payload);
  this->payloadSize = 0;
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
      this->payloadSize = this->size - this->paddingLength - position;
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

