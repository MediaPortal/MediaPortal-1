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

CRtpPacket::CRtpPacket(HRESULT *result)
  : CBaseRtpPacket(result)
{
  this->contributeSourceIdentifiers = NULL;
  this->payloadType = UINT_MAX;
  this->sequenceNumber = UINT_MAX;
  this->synchronizationSourceIdentifier = UINT_MAX;
  this->timestamp = UINT_MAX;
  this->profileSpecificExtensionHeaderId = UINT_MAX;
  this->extensionHeaderLength = UINT_MAX;
  this->extensionHeader = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->contributeSourceIdentifiers = new CContributeSourceIdentifierCollection(result);
    CHECK_POINTER_HRESULT(*result, this->contributeSourceIdentifiers, *result, E_OUTOFMEMORY);
  }
}

CRtpPacket::~CRtpPacket(void)
{
  FREE_MEM(this->extensionHeader);
  FREE_MEM_CLASS(this->contributeSourceIdentifiers);
}

/* get methods */

unsigned int CRtpPacket::GetSize(void)
{
  unsigned int size = RTP_PACKET_HEADER_SIZE;
  size += 4 * this->contributeSourceIdentifiers->Count();

  if (this->IsExtended())
  {
    size += 4 + this->extensionHeaderLength;
  }

  size += this->GetPayloadSize();
  size += this->paddingSize;

  return size;
}

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

unsigned int CRtpPacket::GetPayloadSize(void)
{
  return this->payloadSize;
}

/* set methods */

/* other methods */

bool CRtpPacket::IsExtended(void)
{
  return this->IsSetFlags(RTP_PACKET_FLAG_EXTENSION_HEADER);
}

bool CRtpPacket::IsMarked(void)
{
  return this->IsSetFlags(RTP_PACKET_FLAG_MARKER);
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

    this->flags |= ((RBE8(buffer, position) & 0x10) != 0) ? RTP_PACKET_FLAG_EXTENSION_HEADER : RTP_PACKET_FLAG_NONE;

    unsigned int csrcIdentifierCount = RBE8(buffer, position) & 0x0F;
    position++;

    this->flags |= ((RBE8(buffer, position) & 0x80) != 0) ? RTP_PACKET_FLAG_MARKER : RTP_PACKET_FLAG_NONE;
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

    if (result && ((this->flags & RTP_PACKET_FLAG_EXTENSION_HEADER) != 0))
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
        this->payloadSize = length - position;
        this->payload = ALLOC_MEM_SET(this->payload, unsigned char, this->payloadSize, 0);
        result &= (this->payload != NULL);

        if (result)
        {
          memcpy(this->payload, buffer + position, this->payloadSize);
          position += this->payloadSize;

          if (this->IsPadded())
          {
            this->paddingSize = RBE8(this->payload, this->payloadSize - 1) & 0xFF;
          }
        }
      }
    }

    if (result)
    {
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

  FREE_MEM(this->extensionHeader);
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->contributeSourceIdentifiers, this->contributeSourceIdentifiers->Clear());

  this->payloadType = UINT_MAX;
  this->sequenceNumber = UINT_MAX;
  this->synchronizationSourceIdentifier = UINT_MAX;
  this->timestamp = UINT_MAX;
  this->profileSpecificExtensionHeaderId = UINT_MAX;
  this->extensionHeaderLength = UINT_MAX;
}

CRtpPacket *CRtpPacket::Clone(void)
{
  CRtpPacket *result = this->CreateRtpPacket();

  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
}

/* protected methods */

CRtpPacket *CRtpPacket::CreateRtpPacket(void)
{
  HRESULT result = S_OK;
  CRtpPacket *packet = new CRtpPacket(&result);
  CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
  return packet;
}

bool CRtpPacket::CloneInternal(CRtpPacket *rtpPacket)
{
  bool result = true;

  rtpPacket->baseType = this->baseType;
  rtpPacket->extensionHeaderLength = this->extensionHeaderLength;
  rtpPacket->flags = this->flags;
  rtpPacket->paddingSize = this->paddingSize;
  rtpPacket->payloadSize = this->payloadSize;
  rtpPacket->payloadType = this->payloadType;
  rtpPacket->profileSpecificExtensionHeaderId = this->profileSpecificExtensionHeaderId;
  rtpPacket->sequenceNumber = this->sequenceNumber;
  rtpPacket->synchronizationSourceIdentifier = this->synchronizationSourceIdentifier;
  rtpPacket->timestamp = this->timestamp;
  rtpPacket->version = this->version;

  result &= rtpPacket->contributeSourceIdentifiers->Append(this->contributeSourceIdentifiers);

  if (this->IsExtended())
  {
    rtpPacket->extensionHeader = ALLOC_MEM_SET(rtpPacket->extensionHeader, unsigned char, rtpPacket->extensionHeaderLength, 0);
    result &= (rtpPacket->extensionHeader != NULL);

    CHECK_CONDITION_EXECUTE(result, memcpy(rtpPacket->extensionHeader, this->extensionHeader, rtpPacket->extensionHeaderLength));
  }
  if (rtpPacket->payloadSize != 0)
  {
    rtpPacket->payload = ALLOC_MEM_SET(rtpPacket->payload, unsigned char, rtpPacket->payloadSize, 0);
    result &= (rtpPacket->payload != NULL);

    CHECK_CONDITION_EXECUTE(result, memcpy(rtpPacket->payload, this->payload, rtpPacket->payloadSize));
  }

  return result;
}