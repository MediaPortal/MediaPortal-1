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

#include "RtpPacket.h"
#include "BufferHelper.h"

#include <stdint.h>

CRtpPacket::CRtpPacket(void)
  : CBaseRtpPacket()
{
  this->payload = NULL;
  this->payloadLength = 0;

  this->contributeSourceIdentifiers = new CContributeSourceIdentifierCollection();
  this->payloadType = UINT_MAX;
  this->sequenceNumber = UINT_MAX;
  this->synchronizationSourceIdentifier = UINT_MAX;
  this->timestamp = UINT_MAX;
  this->profileSpecificExtensionHeaderId = UINT_MAX;
  this->extensionHeaderLength = UINT_MAX;
  this->extensionHeader = NULL;
}

CRtpPacket::~CRtpPacket(void)
{
  FREE_MEM(this->payload);
  FREE_MEM(this->extensionHeader);
  FREE_MEM_CLASS(this->contributeSourceIdentifiers);
}

/* get methods */

unsigned int CRtpPacket::GetPayloadType(void)
{
  return this->payloadType;
}

unsigned int CRtpPacket::GetSequenceNumber(void)
{
  return this->sequenceNumber;
}

unsigned int CRtpPacket::GetTimestamp(void)
{
  return this->timestamp;
}

unsigned int CRtpPacket::GetSynchronizationSourceIdentifier(void)
{
  return this->synchronizationSourceIdentifier;
}

const unsigned char *CRtpPacket::GetPayload(void)
{
  return this->payload;
}

unsigned int CRtpPacket::GetPayloadLength(void)
{
  return this->payloadLength;
}

/* set methods */

/* other methods */

bool CRtpPacket::IsExtended(void)
{
  return ((this->flags & FLAG_RTP_PACKET_EXTENSION_HEADER) != 0);
}

bool CRtpPacket::IsMarked(void)
{
  return ((this->flags & FLAG_RTP_PACKET_MARKER) != 0);
}

bool CRtpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = ((buffer != NULL) && (length >= RTP_PACKET_HEADER_SIZE) && (this->contributeSourceIdentifiers != NULL));

  if (result)
  {
    result &= __super::Parse(buffer, length);
  }

  if (result)
  {
    // RTP packet header is at least RTP_PACKET_HEADER_SIZE long

    // parse extension header flag and CSRC identifier count from first byte
    unsigned int position = 0;

    this->flags |= ((RBE8(buffer, position) & 0x10) != 0) ? FLAG_RTP_PACKET_EXTENSION_HEADER : FLAG_RTP_PACKET_NONE;

    unsigned int csrcIdentifierCount = RBE8(buffer, position) & 0x0F;
    position++;

    this->flags |= ((RBE8(buffer, position) & 0x80) != 0) ? FLAG_RTP_PACKET_MARKER : FLAG_RTP_PACKET_NONE;
    this->payloadType = RBE8(buffer, position) & 0x7F;
    position++;

    RBE16INC(buffer, position, this->sequenceNumber);
    RBE32INC(buffer, position, this->timestamp);
    RBE32INC(buffer, position, this->synchronizationSourceIdentifier);

    // do some base checks
    result &= (this->version == RTP_PACKET_VERSION);

    // each contribution source identifier is 32-bit number,
    // so data needs to be (RTP_PACKET_HEADER_SIZE + 4 * csrcIdentifierCount) bytes long
    result &= (length >= (RTP_PACKET_HEADER_SIZE + 4 * csrcIdentifierCount));

    if (result)
    {
      for (unsigned int i = 0; (result && (i < csrcIdentifierCount)); i++)
      {
        CContributeSourceIdentifier *identifier = new CContributeSourceIdentifier();
        result &= (identifier != NULL);

        if (result)
        {
          RBE32INC_DEFINE(buffer, position, temp, unsigned int);
          identifier->SetIdentifier(temp);

          result &= this->contributeSourceIdentifiers->Add(identifier);
        }

        if (!result)
        {
          FREE_MEM_CLASS(identifier);
        }
      }
    }

    if (result && ((this->flags & FLAG_RTP_PACKET_EXTENSION_HEADER) != 0))
    {
      // extension header
      RBE16INC(buffer, position, this->profileSpecificExtensionHeaderId);
      RBE16INC(buffer, position, this->extensionHeaderLength);

      // extension header length is in 32-bit words
      this->extensionHeaderLength *= 4;

      // the length of buffer must be at least (RTP_PACKET_HEADER_SIZE + 4 * csrcIdentifierCount + 4 + this->extensionHeaderLength)

      result &= (length >= (RTP_PACKET_HEADER_SIZE + 4 * csrcIdentifierCount + 4 + this->extensionHeaderLength));

      if (result)
      {
        this->extensionHeader = ALLOC_MEM_SET(this->extensionHeader, unsigned char, this->extensionHeaderLength, 0);
        result &= (this->extensionHeader != NULL);

        if (result)
        {
          memcpy(this->extensionHeader, buffer + position, this->extensionHeaderLength);
          position += this->extensionHeaderLength;

          result &= (position <= length);
        }
      }
    }

    if (result)
    {
      if (position < length)
      {
        // there are still some data in packet
        this->payloadLength = length - position;
        this->payload = ALLOC_MEM_SET(this->payload, unsigned char, this->payloadLength, 0);
        result &= (this->payload != NULL);

        if (result)
        {
          memcpy(this->payload, buffer + position, this->payloadLength);
          position += this->payloadLength;

          if ((this->flags & FLAG_RTP_PACKET_PADDING) != 0)
          {
            this->paddingLength = RBE8(this->payload, this->payloadLength - 1) & 0xFF;
          }
        }
      }
    }

    if (result)
    {
      this->size = position;
      this->baseType = RTP_PACKET_BASE_TYPE;
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}

void CRtpPacket::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->payload);
  FREE_MEM(this->extensionHeader);
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->contributeSourceIdentifiers, this->contributeSourceIdentifiers->Clear());

  this->payloadLength = 0;

  this->payloadType = UINT_MAX;
  this->sequenceNumber = UINT_MAX;
  this->synchronizationSourceIdentifier = UINT_MAX;
  this->timestamp = UINT_MAX;
  this->profileSpecificExtensionHeaderId = UINT_MAX;
  this->extensionHeaderLength = UINT_MAX;
}