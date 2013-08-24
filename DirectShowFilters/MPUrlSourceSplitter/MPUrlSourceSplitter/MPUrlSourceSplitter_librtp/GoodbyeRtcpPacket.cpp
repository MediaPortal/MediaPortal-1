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

#include "GoodbyeRtcpPacket.h"
#include "BufferHelper.h"

#include <stdint.h>

CGoodbyeRtcpPacket::CGoodbyeRtcpPacket(void)
  : CRtcpPacket()
{
  this->reason = NULL;
  this->senderSynchronizationSourceIdentifiers = new CIdentifierCollection();

  this->packetType = GOODBYE_RTCP_PACKET_TYPE;
}

CGoodbyeRtcpPacket::~CGoodbyeRtcpPacket(void)
{
  FREE_MEM(this->reason);
  FREE_MEM_CLASS(this->senderSynchronizationSourceIdentifiers);
}

/* get methods */

unsigned int CGoodbyeRtcpPacket::GetPacketValue(void)
{
  return this->GetSenderSynchronizationSourceIdentifiers()->Count();
}

unsigned int CGoodbyeRtcpPacket::GetPacketType(void)
{
  return GOODBYE_RTCP_PACKET_TYPE;
}

CIdentifierCollection *CGoodbyeRtcpPacket::GetSenderSynchronizationSourceIdentifiers(void)
{
  return this->senderSynchronizationSourceIdentifiers;
}

const wchar_t *CGoodbyeRtcpPacket::GetReason(void)
{
  return this->reason;
}

unsigned int CGoodbyeRtcpPacket::GetSize(void)
{
  unsigned int size = __super::GetSize() + GOODBYE_RTCP_PACKET_HEADER_SIZE + 4 * this->GetSenderSynchronizationSourceIdentifiers()->Count();

  if (this->HasReason())
  {
    char *temp = ConvertUnicodeToUtf8(this->GetReason());
    if (temp != NULL)
    {
      size += 1 + min(0xFF, strlen(temp));
    }
    FREE_MEM(temp);
  }

  if ((size % 4) != 0)
  {
    size += (4 - (size % 4));
  }

  return size;
}

bool CGoodbyeRtcpPacket::GetPacket(unsigned char *buffer, unsigned int length)
{
  bool result = __super::GetPacket(buffer, length);

  if (result)
  {
    unsigned int position = __super::GetSize();

    for (unsigned int i = 0; i < this->GetSenderSynchronizationSourceIdentifiers()->Count(); i++)
    {
      CIdentifier *identifier = this->GetSenderSynchronizationSourceIdentifiers()->GetItem(i);

      WBE32INC(buffer, position, identifier->GetIdentifier());
    }

    if (this->HasReason())
    {
      unsigned int size = 0;
      char *temp = ConvertUnicodeToUtf8(this->GetReason());
      if (temp != NULL)
      {
        size = 1 + min(0xFF, strlen(temp));

        WBE8INC(buffer, position, (size - 1));
        if (size > 1)
        {
          memcpy(buffer + position, temp, size - 1);
          position += size - 1;
        }
      }
      FREE_MEM(temp);

      if ((size % 4) != 0)
      {
        size = (4 - (size % 4));
        memset(buffer + position, 0, size);
        position += size;
      }
    }
  }

  return result;
}

/* set methods */

bool CGoodbyeRtcpPacket::SetReason(const wchar_t *reason)
{
  this->flags &= ~FLAG_GOODBYE_RTCP_PACKET_REASON;
  SET_STRING_RESULT_WITH_NULL_DEFINE(this->reason, reason, result);

  if (result && (this->reason != NULL))
  {
    this->flags |= FLAG_GOODBYE_RTCP_PACKET_REASON;
  }

  return result;
}

/* other methods */

bool CGoodbyeRtcpPacket::HasReason(void)
{
  return ((this->flags & FLAG_GOODBYE_RTCP_PACKET_REASON) != 0);
}

void CGoodbyeRtcpPacket::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->reason);
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->senderSynchronizationSourceIdentifiers, this->senderSynchronizationSourceIdentifiers->Clear());

  this->packetType = GOODBYE_RTCP_PACKET_TYPE;
}

bool CGoodbyeRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = (this->senderSynchronizationSourceIdentifiers != NULL);
  result &= __super::Parse(buffer, length);
  result &= (this->packetType == GOODBYE_RTCP_PACKET_TYPE);
  result &= (this->payloadSize >= GOODBYE_RTCP_PACKET_HEADER_SIZE);

  // we must have enough bytes for SSRC/CSRC and optionally for reason
  // in this->packetValue is count of SSRC/CSRC (each of 4 bytes)
  result &= (this->payloadSize >= (this->packetValue * 4));

  if (result)
  {
    // goodbye RTCP packet header is at least GOODBYE_RTCP_PACKET_HEADER_SIZE long
    unsigned int position = 0;

    for (unsigned int i = 0; (result && (i < this->packetValue)); i++)
    {
      unsigned int temp = 0;

      RBE32INC(this->payload, position, temp);

      CIdentifier *identifier = new CIdentifier();
      result &= (identifier != NULL);

      if (result)
      {
        identifier->SetIdentifier(temp);
        result &= this->senderSynchronizationSourceIdentifiers->Add(identifier);
      }

      if (!result)
      {
        FREE_MEM_CLASS(identifier);
      }
    }

    if (result && (position < this->payloadSize))
    {
      // there are still some data, it have to be reason
      RBE8INC_DEFINE(this->payload, position, reasonLength, unsigned int);
      result &= (reasonLength != 0);
      result &= ((position + reasonLength) <= this->payloadSize);

      if (result)
      {
        ALLOC_MEM_DEFINE_SET(temp, char, (reasonLength + 1), 0);
        result &= (temp != NULL);

        if (result)
        {
          memcpy(temp, this->payload + position, reasonLength);
          this->reason = ConvertUtf8ToUnicode(temp);
          result &= (this->reason != NULL);

          if (result)
          {
            this->flags |= FLAG_GOODBYE_RTCP_PACKET_REASON;
          }
        }

        FREE_MEM(temp);
      }
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}