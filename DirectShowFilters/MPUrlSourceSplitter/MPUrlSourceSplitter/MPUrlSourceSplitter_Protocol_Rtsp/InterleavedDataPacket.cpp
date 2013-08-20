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

#include "InterleavedDataPacket.h"
#include "BaseRtpPacketFactory.h"

CInterleavedDataPacket::CInterleavedDataPacket(void)
{
  this->channelIdentifier = CHANNEL_IDENTIFIER_UNSPECIFED;
  this->packetSize = 0;
  this->baseRtpPackets = new CBaseRtpPacketCollection();
}

CInterleavedDataPacket::~CInterleavedDataPacket(void)
{
  FREE_MEM_CLASS(this->baseRtpPackets);
}

/* get methods */

uint32_t CInterleavedDataPacket::GetPacketSize(void)
{
  return this->packetSize;
}

uint32_t CInterleavedDataPacket::GetChannelIdentifier(void)
{
  return this->channelIdentifier;
}

CBaseRtpPacketCollection *CInterleavedDataPacket::GetBaseRtpPackets(void)
{
  return this->baseRtpPackets;
}

/* set methods */

/* other methods */

HRESULT CInterleavedDataPacket::Parse(const uint8_t *buffer, uint32_t bufferSize)
{
  this->Clear();

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->baseRtpPackets);
  CHECK_CONDITION_HRESULT(result, bufferSize >= DATA_SECTION_OFFSET, result, HRESULT_FROM_WIN32(ERROR_MORE_DATA));

  if (SUCCEEDED(result))
  {
    if ((buffer[0] == INTERLEAVED_PACKET_HEADER_IDENTIFIER))
    {
      this->channelIdentifier = buffer[1];

      this->packetSize = buffer[2];
      this->packetSize <<= 8;
      this->packetSize |= buffer[3];
      this->packetSize += DATA_SECTION_OFFSET;

      CHECK_CONDITION_HRESULT(result, this->packetSize <= bufferSize, result, HRESULT_FROM_WIN32(ERROR_MORE_DATA));

      if (SUCCEEDED(result))
      {
        CBaseRtpPacketFactory *factory = new CBaseRtpPacketFactory();
        CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

        unsigned int processed = DATA_SECTION_OFFSET;
        unsigned int position = 0;

        while (SUCCEEDED(result) && (processed < this->packetSize))
        {
          CBaseRtpPacket *packet = factory->CreateBaseRtpPacket(buffer + processed, this->packetSize - processed, &position);
          CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->baseRtpPackets->Add(packet)) ? result : E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), processed += position);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
        }

        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = processed);

        FREE_MEM_CLASS(factory);
      }
    }
  }

  if (FAILED(result))
  {
    this->Clear();
  }

  return result;
}

void CInterleavedDataPacket::Clear(void)
{
  this->channelIdentifier = CHANNEL_IDENTIFIER_UNSPECIFED;
  this->packetSize = 0;
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->baseRtpPackets, this->baseRtpPackets->Clear());
}