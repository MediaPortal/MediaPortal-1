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

#include "SourceDescriptionRtcpPacket.h"

CSourceDescriptionRtcpPacket::CSourceDescriptionRtcpPacket(void)
  : CRtcpPacket()
{
  this->chunks = new CSourceDescriptionChunkCollection();
}

CSourceDescriptionRtcpPacket::~CSourceDescriptionRtcpPacket(void)
{
  FREE_MEM_CLASS(this->chunks);
}

/* get methods */

/* set methods */

/* other methods */

void CSourceDescriptionRtcpPacket::Clear(void)
{
  __super::Clear();

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->chunks, this->chunks->Clear());
}

bool CSourceDescriptionRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = (this->chunks != NULL);
  result &= __super::Parse(buffer, length);
  result &= (this->packetType == SOURCE_DESCRIPTION_RTCP_PACKET_TYPE);
  result &= (this->payloadLength >= SOURCE_DESCRIPTION_RTCP_PACKET_HEADER_SIZE);

  if (result)
  {
    // source description RTCP packet header is at least SOURCE_DESCRIPTION_RTCP_PACKET_HEADER_SIZE long
    unsigned int position = 0;

    // parse chunks
    while (result && (position < this->payloadLength))
    {
      CSourceDescriptionChunk *chunk = new CSourceDescriptionChunk();
      result &= (chunk != NULL);

      if (result)
      {
        result &= chunk->Parse(this->payload + position, this->payloadLength - position);
      }

      if (result)
      {
        result &= (this->chunks->Add(chunk));
      }

      if (result)
      {
        position += chunk->GetSize();
      }

      if (!result)
      {
        FREE_MEM_CLASS(chunk);
      }
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}