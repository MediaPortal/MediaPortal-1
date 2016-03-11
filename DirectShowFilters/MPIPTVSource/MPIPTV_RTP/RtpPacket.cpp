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

#include "stdafx.h"

#include "RtpPacket.h"
#include "Utilities.h"

RtpPacket::RtpPacket(const char *buffer, unsigned int length, RtpPacket *previousPacket, RtpPacket *nextPacket)
{
  bool result = false;
  this->data = NULL;
  this->length = 0;

  this->length = max(RTP_HEADER_SIZE, length);
  this->data = ALLOC_MEM_SET(this->data, char, this->length, 0);
  if (this->data != NULL)
  {
    result = true;
    this->nextPacket = nextPacket;
    this->previousPacket = previousPacket;
    if ((buffer != NULL) && (length >= RTP_HEADER_SIZE))
    {
      // copy only if is supplied buffer with at least RTP header
      memcpy(this->data, buffer, length);
    }
    else
    {
      // default initialization of RTP packet
      result &= this->SetVersion(RTP_PACKET_VERSION);
      SYSTEMTIME time;
      GetSystemTime(&time);
      unsigned int sequenceNumber = (time.wDay + time.wDayOfWeek + time.wHour + time.wMilliseconds + time.wMinute + time.wMonth + time.wSecond + time.wYear) & 0x0000FFFF;
      result &= this->SetSequenceNumber(sequenceNumber);
    }
  }

  if (!result)
  {
    // error occured
    FREE_MEM(this->data);
    this->length = 0;
  }
}

RtpPacket::~RtpPacket(void)
{
  FREE_MEM(this->data);
  this->length = 0;
}

unsigned int RtpPacket::GetPacketLength(void)
{
  return this->length;
}

// RTP packet header methods

unsigned int RtpPacket::GetVersion(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)((*this->data) & 0xC0)) >> 6;
  }

  return result;
}

bool RtpPacket::SetVersion(unsigned int version)
{
  bool result = false;

  if ((this->length >= RTP_HEADER_SIZE) && (version == RTP_PACKET_VERSION))
  {
    *this->data = ((*this->data) & 0x3F) | ((char)(version << 6));
    result = true;
  }

  return result;
}

unsigned int RtpPacket::GetPadding(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)((*this->data) & 0x20)) >> 5;
  }

  return result;
}

bool RtpPacket::SetPadding(unsigned int length)
{
  bool result = false;

  if (length <= RTP_PACKET_MAXIMUM_PADDING_LENGTH)
  {
    // get current padding length
    unsigned int currentPadding = this->GetPaddingLength();
    if (currentPadding != UINT_MAX)
    {
      // no error occured
      unsigned int resizedLength = this->length - currentPadding + length;
      unsigned int rawPacketLength = this->length - currentPadding;

      ALLOC_MEM_DEFINE_SET(buffer, char, resizedLength, 0);
      if (buffer != NULL)
      {
        // copy raw data from old RTP packet (except padding) to new RTP packet, padding is on the end
        memcpy(buffer, this->data, rawPacketLength);
        // release old RTP packet buffer, set new RTP packet buffer
        FREE_MEM(this->data);
        this->data = buffer;
        this->length = resizedLength;

        if (length > 0)
        {
          // set padding length (set last byte to padding length)
          *(this->data + this->length - 1) = (char)length;
        }

        // clear padding flag
        *this->data = (*this->data) & 0xDF;
        // set padding flag (if necessary)
        *this->data |= (length > 0) ? 0x20 : 0x00;
        
        result = true;
      }
    }
  }

  return result;
}

unsigned int RtpPacket::GetPaddingLength(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = (this->IsPadding()) ? ((unsigned int)(*(this->data + this->length - 1))) : 0;
  }

  return result;
}

bool RtpPacket::IsPadding(void)
{
  return (this->GetPadding() == 0x00000001);
}

unsigned int RtpPacket::GetExtensionHeader(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)((*this->data) & 0x10)) >> 4;
  }

  return result;
}

unsigned int RtpPacket::GetProfileSpecificExtensionHeaderId(void)
{
  unsigned int result = UINT_MAX;
  
  // extension header is after contributing source IDs
  if (this->GetContributingSourceIdCount() != UINT_MAX)
  {
    unsigned int contributingSourceIdLength = 4 * this->GetContributingSourceIdCount();

    if (this->IsExtensionHeader())
    {
      if (this->length >= (RTP_HEADER_SIZE + contributingSourceIdLength + 4))
      {
        result = ((unsigned int)(*(this->data + RTP_HEADER_SIZE + contributingSourceIdLength))) << 8;
        result |= ((unsigned int)(*(this->data + RTP_HEADER_SIZE + contributingSourceIdLength + 1)));
      }
    }
    else
    {
      result = 0;
    }
  }

  return result;
}

unsigned int RtpPacket::GetExtensionHeaderLength(void)
{
  unsigned int result = UINT_MAX;

  // extension header is after contributing source IDs
  if (this->GetContributingSourceIdCount() != UINT_MAX)
  {
    unsigned int contributingSourceIdLength = 4 * this->GetContributingSourceIdCount();

    if (this->IsExtensionHeader())
    {
      if (this->length >= (RTP_HEADER_SIZE + contributingSourceIdLength + 4))
      {
        result = ((unsigned int)(*(this->data + RTP_HEADER_SIZE + contributingSourceIdLength + 2))) << 8;
        result |= ((unsigned int)(*(this->data + RTP_HEADER_SIZE + contributingSourceIdLength + 3)));
        result *= 4;
      }
    }
    else
    {
      result = 0;
    }
  }

  return result;
}

unsigned int RtpPacket::GetExtensionHeaderFullLength(void)
{
  unsigned int result = UINT_MAX;

  if (this->IsExtensionHeader())
  {
    result = this->GetExtensionHeaderLength();
    if (result != UINT_MAX)
    {
      result += 4;
    }
  }
  else
  {
    result = 0;
  }

  return result;
}

bool RtpPacket::SetExtensionHeader(unsigned int profileSpecificExtensionHeaderId, char *extensionHeader, unsigned int length)
{
  bool result = false;

  // get current padding length
  unsigned int currentExtensionHeaderLength = this->GetExtensionHeaderFullLength();
  unsigned int extensionHeaderLengthIn32bitWords = length / 32;
  unsigned int paddingLength = this->GetPaddingLength();
  unsigned int rawDataLength = this->GetDataLength();

  if ((currentExtensionHeaderLength != UINT_MAX) &&
      ((extensionHeaderLengthIn32bitWords * 32) == length) &&
      (extensionHeader != NULL) &&
      (profileSpecificExtensionHeaderId <= RTP_PACKET_MAXIMUM_PROFILE_SPECIFIC_EXTENSION_HEADER_ID) &&
      (extensionHeaderLengthIn32bitWords <= RTP_PACKET_MAXIMUM_EXTENSION_HEADER_LENGTH_IN_32BIT_WORDS) &&
      (this->GetContributingSourceIdCount() != UINT_MAX) &&
      (this->GetDataLength() != UINT_MAX) &&
      (paddingLength != UINT_MAX) &&
      (rawDataLength != UINT_MAX))
  {
    unsigned int contributeSourceIdLength = this->GetContributingSourceIdCount() * 4;
    // 4 bytes are for profile specific extension header id and extension header length
    unsigned int resizedLength = this->length - currentExtensionHeaderLength + length + 4;

    ALLOC_MEM_DEFINE_SET(buffer, char, resizedLength, 0);
    if (buffer != NULL)
    {
      // copy header from old RTP packet (except old extension header) to new RTP packet
      memcpy(buffer, this->data, RTP_HEADER_SIZE + contributeSourceIdLength);
      char *newDataStart = buffer + RTP_HEADER_SIZE + contributeSourceIdLength;
      newDataStart += (length > 0) ? (4 + length) : 0;
      if (paddingLength > 0)
      {
        // copy padding data if necessary
        char *paddingStart = newDataStart + rawDataLength;
        char *currentPaddingStart = this->data + RTP_HEADER_SIZE + contributeSourceIdLength;
        currentPaddingStart += currentExtensionHeaderLength;
        memcpy(paddingStart, currentPaddingStart, paddingLength);
      }

      // copy RTP packet data from old RTP packet to new RTP packet
      if (this->GetData(newDataStart, rawDataLength) != UINT_MAX)
      {
        // release old RTP packet buffer, set new RTP packet buffer
        FREE_MEM(this->data);
        this->data = buffer;
        this->length = resizedLength;

        if (length > 0)
        {
          // copy extension header data
          memcpy(this->data + RTP_HEADER_SIZE + contributeSourceIdLength + 4, extensionHeader, length);
          // set profile specific extension header ID
          *(this->data + RTP_HEADER_SIZE + contributeSourceIdLength) = (char)((profileSpecificExtensionHeaderId >> 8) & 0x000000FF);
          *(this->data + RTP_HEADER_SIZE + contributeSourceIdLength + 1) = (char)(profileSpecificExtensionHeaderId & 0x000000FF);
          // set extension header length
          *(this->data + RTP_HEADER_SIZE + contributeSourceIdLength + 2) = (char)((extensionHeaderLengthIn32bitWords >> 8) & 0x000000FF);
          *(this->data + RTP_HEADER_SIZE + contributeSourceIdLength + 3) = (char)(extensionHeaderLengthIn32bitWords & 0x000000FF);
        }

        // clear extension header flag
        *this->data = (*this->data) & 0xEF;
        // set extension header flag (if necessary)
        *this->data |= (length > 0) ? 0x10 : 0x00;

        result = true;
      }
      else
      {
        // error occured while copying RTP packet data
        FREE_MEM(buffer);
      }
    }
  }

  return result;
}

unsigned int RtpPacket::GetExtensionHeaderData(char *buffer, unsigned int length)
{
  unsigned int result = UINT_MAX;

  unsigned int contributingSourceIdCount = this->GetContributingSourceIdCount();

  if (this->IsExtensionHeader() && (contributingSourceIdCount != UINT_MAX))
  {
    unsigned int extensionHeaderLength = this->GetExtensionHeaderLength();

    if ((extensionHeaderLength != UINT_MAX) && (extensionHeaderLength <= length) && (buffer != NULL))
    {
      char *extensionHeaderStart = this->data + RTP_HEADER_SIZE + contributingSourceIdCount * 4 + 4;
      memcpy(buffer, extensionHeaderStart, extensionHeaderLength);
      result = extensionHeaderLength;
    }
  }

  return result;
}

bool RtpPacket::IsExtensionHeader(void)
{
  return (this->GetExtensionHeader() == 0x00000001);
}

unsigned int RtpPacket::GetContributingSourceIdCount(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)((*this->data) & 0x0F));
  }

  return result;
}

bool RtpPacket::SetContributingSourceId(unsigned int *contributingSources, unsigned int length)
{
  bool result = false;

  unsigned int currentContributingSourceIdCount = this->GetContributingSourceIdCount();

  if ((currentContributingSourceIdCount != UINT_MAX) &&
      (length <= RTP_PACKET_MAXIMUM_CONTRIBUTING_SOURCE_ID_COUNT))
  {
    result = true;

    unsigned int resizedLength = this->length - currentContributingSourceIdCount * 4 + length * 4;
    if (resizedLength != this->length)
    {
      // resize and copy necessary data
      ALLOC_MEM_DEFINE_SET(buffer, char, resizedLength, 0);
      result &= (buffer != NULL);
      if (result)
      {
        // copy header data until contributing sources IDs
        memcpy(buffer, this->data, RTP_HEADER_SIZE);
        // copy remaining data after contributing source IDs
        memcpy(buffer + RTP_HEADER_SIZE + length * 4, this->data + RTP_HEADER_SIZE + currentContributingSourceIdCount * 4, this->length - RTP_HEADER_SIZE - currentContributingSourceIdCount * 4);

        FREE_MEM(this->data);
        this->data = buffer;
        this->length = resizedLength;
      }
    }

    if (result)
    {
      // set contribute source id count
      *this->data &= 0xF0;
      *this->data |= (char)(length & 0x0000000F);

      if (length > 0)
      {
        // copy contributing source IDs
        memcpy(this->data + RTP_HEADER_SIZE, contributingSources, length * 4);
      }
    }
  }

  return result;
}

unsigned int RtpPacket::GetDataLength(void)
{
  unsigned int result = UINT_MAX;

  unsigned int contributingSourceIdCount = this->GetContributingSourceIdCount();
  unsigned int extensionHeaderLength = this->GetExtensionHeaderFullLength();
  unsigned int paddingLength = this->GetPaddingLength();

  if ((contributingSourceIdCount != UINT_MAX) &&
      (extensionHeaderLength != UINT_MAX) &&
      (paddingLength != UINT_MAX))
  {
    // RTP packet data starts after RTP packet header and extension header
    // RTP packet data ends before padding
    if (this->length >= (RTP_HEADER_SIZE + contributingSourceIdCount * 4 + extensionHeaderLength + paddingLength))
    {
      result = this->length - RTP_HEADER_SIZE - contributingSourceIdCount * 4 - extensionHeaderLength - paddingLength;
    }
  }

  return result;
}

unsigned int RtpPacket::GetData(char *buffer, unsigned int length)
{
  unsigned int result = UINT_MAX;

  unsigned int rawDataLength = this->GetDataLength();
  unsigned int contributingSourceIdCount = this->GetContributingSourceIdCount();
  unsigned int extensionHeaderLength = this->GetExtensionHeaderFullLength();
  unsigned int paddingLength = this->GetPaddingLength();

  if ((rawDataLength != UINT_MAX) &&
      (rawDataLength <= length) &&
      (contributingSourceIdCount != UINT_MAX) &&
      (extensionHeaderLength != UINT_MAX) &&
      (paddingLength != UINT_MAX))
  {
    char *dataStart = this->data + RTP_HEADER_SIZE + contributingSourceIdCount * 4 + extensionHeaderLength;
    if (rawDataLength > 0)
    {
      memcpy(buffer, dataStart, rawDataLength);
    }
    result = rawDataLength;
  }

  return result;
}

bool RtpPacket::SetData(char *buffer, unsigned int length)
{
  bool result = false;

  unsigned int currentDataLength = this->GetDataLength();
  unsigned int contributingSourceIdCount = this->GetContributingSourceIdCount();
  unsigned int extensionHeaderLength = this->GetExtensionHeaderFullLength();
  unsigned int paddingLength = this->GetPaddingLength();

  if ((currentDataLength != UINT_MAX) &&
      (contributingSourceIdCount != UINT_MAX) &&
      (extensionHeaderLength != UINT_MAX) &&
      (paddingLength != UINT_MAX))
  {
    result = true;
    unsigned int resizedLength = this->length - currentDataLength + length;
    if (this->length != resizedLength)
    {
      // resize and copy necessary data
      ALLOC_MEM_DEFINE_SET(tempBuffer, char, resizedLength, 0);
      result &= (tempBuffer != NULL);
      if (result)
      {
        // copy all data until RTP packet data
        memcpy(tempBuffer, this->data, RTP_HEADER_SIZE + contributingSourceIdCount * 4 + extensionHeaderLength);
        // copy remaining data after RTP data (padding data)
        memcpy(tempBuffer + RTP_HEADER_SIZE + contributingSourceIdCount * 4 + extensionHeaderLength + length, this->data + RTP_HEADER_SIZE + contributingSourceIdCount * 4 + extensionHeaderLength + currentDataLength, paddingLength);

        FREE_MEM(this->data);
        this->data = tempBuffer;
        this->length = resizedLength;
      }

      if ((result) && (length > 0))
      {
        // copy RTP packet data
        memcpy(this->data + RTP_HEADER_SIZE + contributingSourceIdCount * 4 + extensionHeaderLength, buffer, length);
      }
    }
  }

  return result;
}

unsigned int RtpPacket::GetMarker(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)((*(this->data + 1)) & 0x80)) >> 7;
  }

  return result;
}

bool RtpPacket::SetMarker(void)
{
  bool result = false;

  if (this->length >= RTP_HEADER_SIZE)
  {
    *(this->data + 1) |= 0x80;
    result = true;
  }

  return result;
}

bool RtpPacket::ClearMarker(void)
{
  bool result = false;

  if (this->length >= RTP_HEADER_SIZE)
  {
    *(this->data + 1) &= 0x7F;
    result = true;
  }

  return result;
}

bool RtpPacket::IsMarker(void)
{
  return (this->GetMarker() == 0x00000001);
}

unsigned int RtpPacket::GetPayloadType(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = (unsigned int)((*(this->data + 1)) & 0x7F);
  }

  return result;
}

bool RtpPacket::SetPayloadType(unsigned int payloadType)
{
  bool result = false;

  if ((this->length >= RTP_HEADER_SIZE) && (payloadType <= RTP_PACKET_MAXIMUM_PAYLOAD_TYPE))
  {
    *(this->data + 1) &= 0x80;
    *(this->data + 1) |= ((char)payloadType & 0x7F);
    result = true;
  }

  return result;
}

unsigned int RtpPacket::GetSequenceNumber(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)(*(this->data + 2)) & 0x000000FF) << 8;
    result |= (unsigned int)(*(this->data + 3)) & 0x000000FF;
  }

  return result;
}

bool RtpPacket::SetSequenceNumber(unsigned int sequenceNumber)
{
  bool result = false;

  if ((this->length >= RTP_HEADER_SIZE) && (sequenceNumber <= RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER))
  {
    *(this->data + 2) = (char)(sequenceNumber >> 8);
    *(this->data + 3) = (char)(sequenceNumber);
    result = true;
  }

  return result;
}

unsigned int RtpPacket::GetTimestamp(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)(*(this->data + 4)) & 0x000000FF) << 24;
    result |= ((unsigned int)(*(this->data + 5)) & 0x000000FF) << 16;
    result |= ((unsigned int)(*(this->data + 6)) & 0x000000FF) << 8;
    result |= ((unsigned int)(*(this->data + 7)) & 0x000000FF);
  }

  return result;
}

bool RtpPacket::SetTimestamp(unsigned int timestamp)
{
  bool result = false;

  if (this->length >= RTP_HEADER_SIZE)
  {
    *(this->data + 4) = (char)((timestamp & 0xFF000000) >> 24);
    *(this->data + 5) = (char)((timestamp & 0x00FF0000) >> 16);
    *(this->data + 6) = (char)((timestamp & 0x0000FF00) >> 8);
    *(this->data + 7) = (char)((timestamp & 0x000000FF));
    result = true;
  }

  return result;
}

unsigned int RtpPacket::GetSourceIdentifier(void)
{
  unsigned int result = UINT_MAX;

  if (this->length >= RTP_HEADER_SIZE)
  {
    result = ((unsigned int)(*(this->data + 8)) & 0x000000FF) << 24;
    result |= ((unsigned int)(*(this->data + 9)) & 0x000000FF) << 16;
    result |= ((unsigned int)(*(this->data + 10)) & 0x000000FF) << 8;
    result |= ((unsigned int)(*(this->data + 11)) & 0x000000FF);
  }

  return result;
}

bool RtpPacket::SetSourceIdentifier(unsigned int sourceIdentifier)
{
  bool result = false;

  if (this->length >= RTP_HEADER_SIZE)
  {
    *(this->data + 4) = (char)((sourceIdentifier & 0xFF000000) >> 24);
    *(this->data + 5) = (char)((sourceIdentifier & 0x00FF0000) >> 16);
    *(this->data + 6) = (char)((sourceIdentifier & 0x0000FF00) >> 8);
    *(this->data + 7) = (char)((sourceIdentifier & 0x000000FF));
    result = true;
  }

  return result;
}

unsigned int RtpPacket::GetPacketData(char *buffer, unsigned int length)
{
  unsigned int result = UINT_MAX;

  if ((this->length >= RTP_HEADER_SIZE) &&
      (buffer != NULL) &&
      (this->length <= length))
  {
    memcpy(buffer, this->data, this->length);
    result = this->length;
  }

  return result;
}

RtpPacket *RtpPacket::Clone(void)
{
  return new RtpPacket(this->data, this->length, NULL, NULL);
}

bool RtpPacket::IsRtpPacket()
{
  bool result = false;

  if ((this->GetVersion() == RTP_PACKET_VERSION) &&
      (this->GetPaddingLength() != UINT_MAX) &&
      (this->GetContributingSourceIdCount() != UINT_MAX) &&
      (this->GetPayloadType() != UINT_MAX) &&
      (this->GetSequenceNumber() != UINT_MAX) &&
      (this->GetTimestamp() != UINT_MAX) &&
      (this->GetSourceIdentifier() != UINT_MAX) &&
      (this->GetExtensionHeaderLength() != UINT_MAX))
  {
    result = true;
    if (this->IsExtensionHeader())
    {
      result &= (this->GetProfileSpecificExtensionHeaderId() != UINT_MAX);
    }

    if (result)
    {
      unsigned int packetLength = RTP_HEADER_SIZE + this->GetContributingSourceIdCount() * 4 + this->GetPaddingLength() + this->GetDataLength();
      packetLength += (this->IsExtensionHeader()) ? (4 + this->GetExtensionHeaderLength()) : 0;

      result = (this->length == packetLength);
    }
  }

  return result;
}

RtpPacket *RtpPacket::GetNextPacket(void)
{
  return this->nextPacket;
}

void RtpPacket::SetNextPacket(RtpPacket *rtpPacket)
{
  this->nextPacket = rtpPacket;
}

RtpPacket *RtpPacket::GetPreviousPacket(void)
{
  return this->previousPacket;
}

void RtpPacket::SetPreviousPacket(RtpPacket *rtpPacket)
{
  this->previousPacket = rtpPacket;
}
