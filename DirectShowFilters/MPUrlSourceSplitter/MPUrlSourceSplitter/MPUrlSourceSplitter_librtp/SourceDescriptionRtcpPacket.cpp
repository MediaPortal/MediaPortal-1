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

CSourceDescriptionRtcpPacket::CSourceDescriptionRtcpPacket(HRESULT *result)
  : CRtcpPacket(result)
{
  this->chunks = NULL;

  this->packetType = SOURCE_DESCRIPTION_RTCP_PACKET_TYPE;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->chunks = new CSourceDescriptionChunkCollection(result);
    CHECK_POINTER_HRESULT(*result, this->chunks, *result, E_OUTOFMEMORY);
  }
}

CSourceDescriptionRtcpPacket::~CSourceDescriptionRtcpPacket(void)
{
  FREE_MEM_CLASS(this->chunks);
}

/* get methods */

unsigned int CSourceDescriptionRtcpPacket::GetPacketValue(void)
{
  return this->GetChunks()->Count();
}

unsigned int CSourceDescriptionRtcpPacket::GetPacketType(void)
{
  return SOURCE_DESCRIPTION_RTCP_PACKET_TYPE;
}

unsigned int CSourceDescriptionRtcpPacket::GetSize(void)
{
  unsigned int size = SOURCE_DESCRIPTION_RTCP_PACKET_HEADER_SIZE + __super::GetSize();

  for (unsigned int i = 0; i < this->GetChunks()->Count(); i++)
  {
    CSourceDescriptionChunk *chunk = this->GetChunks()->GetItem(i);

    size += chunk->GetSize();
  }

  return size;
}

bool CSourceDescriptionRtcpPacket::GetPacket(unsigned char *buffer, unsigned int length)
{
  bool result = __super::GetPacket(buffer, length);

  if (result)
  {
    unsigned int position = __super::GetSize();

    for (unsigned int i = 0; (result && (i < this->GetChunks()->Count())); i++)
    {
      CSourceDescriptionChunk *chunk = this->GetChunks()->GetItem(i);

      result &= chunk->GetChunk(buffer + position, length - position);
      position += chunk->GetSize();
    }
  }

  return result;
}

CSourceDescriptionChunkCollection *CSourceDescriptionRtcpPacket::GetChunks(void)
{
  return this->chunks;
}

/* set methods */

/* other methods */

void CSourceDescriptionRtcpPacket::Clear(void)
{
  __super::Clear();

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->chunks, this->chunks->Clear());

  this->packetType = SOURCE_DESCRIPTION_RTCP_PACKET_TYPE;
}

bool CSourceDescriptionRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = (this->chunks != NULL);
  result &= __super::Parse(buffer, length);
  result &= (this->packetType == SOURCE_DESCRIPTION_RTCP_PACKET_TYPE);
  result &= (this->payloadSize >= SOURCE_DESCRIPTION_RTCP_PACKET_HEADER_SIZE);

  if (result)
  {
    // source description RTCP packet header is at least SOURCE_DESCRIPTION_RTCP_PACKET_HEADER_SIZE long
    unsigned int position = 0;

    // parse chunks
    while (result && (position < this->payloadSize))
    {
      HRESULT res = S_OK;
      CSourceDescriptionChunk *chunk = new CSourceDescriptionChunk(&res);
      result &= (chunk != NULL);

      if (result)
      {
        result &= chunk->Parse(this->payload + position, this->payloadSize - position);
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