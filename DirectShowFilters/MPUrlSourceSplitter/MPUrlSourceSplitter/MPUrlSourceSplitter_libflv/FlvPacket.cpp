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

#include "FlvPacket.h"

CFlvPacket::CFlvPacket(HRESULT *result)
{
  this->packet = NULL;
  this->size = 0;
  this->type = FLV_PACKET_NONE;
  this->encrypted = false;
}

CFlvPacket::~CFlvPacket(void)
{
  FREE_MEM(this->packet);
}

bool CFlvPacket::IsValid(void)
{
  return ((this->packet != NULL) && (this->size != 0) && (this->type != FLV_PACKET_NONE));
}

const unsigned char *CFlvPacket::GetData(void)
{
  return this->packet;
}

unsigned int CFlvPacket::GetSize(void)
{
  return this->size;
}

unsigned int CFlvPacket::GetType(void)
{
  return this->type;
}

int CFlvPacket::ParsePacket(const unsigned char *buffer, unsigned int length)
{
  int result = FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_HEADER;
  this->Clear();

  if ((buffer != NULL) && (length >= FLV_PACKET_HEADER_LENGTH))
  {
    // at least size for FLV header
    this->packet = ALLOC_MEM_SET(this->packet, unsigned char, FLV_PACKET_HEADER_LENGTH, 0);
    result = (this->packet != NULL) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY;

    if (this->packet != NULL)
    {
      memcpy(this->packet, buffer, FLV_PACKET_HEADER_LENGTH);

      // copied 13 bytes, check first 3 bytes
      if (strncmp("FLV", (char *)this->packet, 3) == 0)
      {
        this->size = FLV_PACKET_HEADER_LENGTH;
        this->type = FLV_PACKET_HEADER;
        result = FLV_PARSE_RESULT_OK;
      }
      else
      {
        // we got first 13 bytes to analyze
        this->type = (*this->packet) & FLV_PACKET_TYPE_MASK;
        this->encrypted = ((*this->packet) & FLV_PACKET_ENCRYPTED_MASK) != 0;

        this->size = ((unsigned char)this->packet[1]) << 8;
        this->size += ((unsigned char)this->packet[2]);
        this->size <<= 8;
        this->size += ((unsigned char)this->packet[3]) + 0x0F;

        result = (length >= this->size) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_PACKET;

        if (length >= this->size)
        {
          FREE_MEM(this->packet);
          this->packet = ALLOC_MEM_SET(this->packet, unsigned char, this->size, 0);
          result = (this->packet != NULL) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY;

          if (this->packet != NULL)
          {
            memcpy(this->packet, buffer, this->size);

            unsigned int checkSize = ((unsigned char)this->packet[this->size - 4]) << 8;
            checkSize += ((unsigned char)this->packet[this->size - 3]);
            checkSize <<= 8;
            checkSize += ((unsigned char)this->packet[this->size - 2]);
            checkSize <<= 8;
            checkSize += ((unsigned char)this->packet[this->size - 1]) + 4;

            // FLV packet has correct size return FLV_PARSE_RESULT_OK
            // FLV packet has incorrect size return FLV_PARSE_RESULT_CHECK_SIZE_INCORRECT
            result = (this->size == checkSize) ? FLV_PARSE_RESULT_OK : FLV_PARSE_RESULT_CHECK_SIZE_INCORRECT;
          }
        }
      }
    }
  }

  if (result != FLV_PARSE_RESULT_OK)
  {
    this->Clear();
  }

  return result;
}

unsigned int CFlvPacket::GetPossiblePacketSize(const unsigned char *buffer, unsigned int length)
{
  unsigned int result = UINT_MAX;

  if ((buffer != NULL) && (length >= FLV_PACKET_HEADER_LENGTH))
  {
    // enough data for FLV header packet, check first 3 bytes
    if (strncmp("FLV", (char *)buffer, 3) == 0)
    {
      result = FLV_PACKET_HEADER_LENGTH;
    }
    else
    {
      // we got at least FLV_PACKET_HEADER_LENGTH bytes to analyze

      result = ((unsigned char)buffer[1]) << 8;
      result += ((unsigned char)buffer[2]);
      result <<= 8;
      result += ((unsigned char)buffer[3]) + 0x0F;
    }
  }

  return result;
}

int CFlvPacket::ParsePacket(CLinearBuffer *buffer)
{
  int result = FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_HEADER;
  this->Clear();

  if ((buffer != NULL) && (buffer->GetBufferOccupiedSpace() >= FLV_PACKET_HEADER_LENGTH))
  {
    unsigned int possibleSize = UINT_MAX;
    // first get possible FLV packet size
    ALLOC_MEM_DEFINE_SET(sizeBuffer, unsigned char, FLV_PACKET_HEADER_LENGTH, 0);
    result = (sizeBuffer != NULL) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY;

    if (sizeBuffer != NULL)
    {
      buffer->CopyFromBuffer(sizeBuffer, FLV_PACKET_HEADER_LENGTH);
      possibleSize = this->GetPossiblePacketSize(sizeBuffer, FLV_PACKET_HEADER_LENGTH);
    }
    FREE_MEM(sizeBuffer);

    result = (possibleSize != UINT_MAX) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY;

    if (possibleSize != UINT_MAX)
    {
      // at least size for FLV header
      ALLOC_MEM_DEFINE_SET(buf, unsigned char, possibleSize, 0);
      result = (buf != NULL) ? result : FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY;

      if (buf != NULL)
      {
        buffer->CopyFromBuffer(buf, possibleSize);
        result = this->ParsePacket(buf, possibleSize);
      }
      FREE_MEM(buf);
    }
  }

  if (result != FLV_PARSE_RESULT_OK)
  {
    this->Clear();
  }

  return result;
}

bool CFlvPacket::CreatePacket(unsigned int packetType, const unsigned char *buffer, unsigned int length, unsigned int timestamp, bool encrypted)
{
  bool result = false;
  this->Clear();

  if ((buffer != NULL) && ((packetType == FLV_PACKET_AUDIO) || (packetType == FLV_PACKET_VIDEO) || (packetType == FLV_PACKET_META)))
  {
    this->type = packetType;
    this->encrypted = encrypted;
    this->size = length + 0x0F;
    this->packet = ALLOC_MEM_SET(this->packet, unsigned char, this->size, 0);
    result = (this->packet != NULL);

    if (result)
    {
      this->packet[0] = (unsigned char)packetType;
      this->packet[0] |= (this->encrypted) ? (unsigned char)FLV_PACKET_ENCRYPTED_MASK : (unsigned char)0;
      
      this->packet[1] = (unsigned char)((length & 0x00FF0000) >> 16);
      this->packet[2] = (unsigned char)((length & 0x0000FF00) >> 8);
      this->packet[3] = (unsigned char)(length & 0x000000FF);

      this->packet[4] = (unsigned char)((timestamp & 0x00FF0000) >> 16);
      this->packet[5] = (unsigned char)((timestamp & 0x0000FF00) >> 8);
      this->packet[6] = (unsigned char)(timestamp & 0x000000FF);
      this->packet[7] = (unsigned char)((timestamp & 0xFF000000) >> 24);

      memcpy(this->packet + 11, buffer, length);

      unsigned int checkSize = this->size - 0x04;
      this->packet[this->size - 4] = (unsigned char)((checkSize & 0xFF000000) >> 24);
      this->packet[this->size - 3] = (unsigned char)((checkSize & 0x00FF0000) >> 16);
      this->packet[this->size - 2] = (unsigned char)((checkSize & 0x0000FF00) >> 8);
      this->packet[this->size - 1] = (unsigned char)((checkSize & 0x000000FF));
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}

bool CFlvPacket::IsEncrypted(void)
{
  return this->encrypted;
}

unsigned int CFlvPacket::GetTimestamp(void)
{
  unsigned int result = 0;

  if (this->IsValid() && (this->type != FLV_PACKET_HEADER))
  {
    result = ((unsigned char)this->packet[7]) << 8;
    result += ((unsigned char)this->packet[4]);
    result <<= 8;
    result += ((unsigned char)this->packet[5]);
    result <<= 8;
    result += ((unsigned char)this->packet[6]);
  }

  return result;
}

void CFlvPacket::SetTimestamp(unsigned int timestamp)
{
  if (this->IsValid() && (this->type != FLV_PACKET_HEADER))
  {
    this->packet[6] = (unsigned char)(timestamp & 0xFF);
    timestamp >>= 8;
    this->packet[5] = (unsigned char)(timestamp & 0xFF);
    timestamp >>= 8;
    this->packet[4] = (unsigned char)(timestamp & 0xFF);
    timestamp >>= 8;
    this->packet[7] = (unsigned char)(timestamp & 0xFF);
  }
}

unsigned int CFlvPacket::GetCodecId(void)
{
  unsigned int codecId = UINT_MAX;

  if (this->IsValid() && (this->type == FLV_PACKET_VIDEO))
  {
    codecId =  this->packet[11] & FLV_VIDEO_CODECID_MASK;
  }

  return codecId;
}

unsigned int CFlvPacket::GetFrameType(void)
{
  unsigned int frameType = UINT_MAX;

  if (this->IsValid() && (this->type == FLV_PACKET_VIDEO))
  {
    frameType =  this->packet[11] & FLV_VIDEO_FRAMETYPE_MASK;
  }

  return frameType;
}

void CFlvPacket::SetCodecId(unsigned int codecId)
{
  if (this->IsValid() && (this->type == FLV_PACKET_VIDEO))
  {
    this->packet[11] = this->packet[11] & (~FLV_VIDEO_CODECID_MASK) | codecId;
  }
}

void CFlvPacket::SetFrameType(unsigned int frameType)
{
  if (this->IsValid() && (this->type == FLV_PACKET_VIDEO))
  {
    this->packet[11] = this->packet[11] & (~FLV_VIDEO_FRAMETYPE_MASK) | frameType;
  }
}

void CFlvPacket::Clear(void)
{
  FREE_MEM(this->packet);
  this->size = 0;
  this->type = FLV_PACKET_NONE;
  this->encrypted = false;
}

bool CFlvPacket::IsKeyFrame(void)
{
  return ((this->type == FLV_PACKET_VIDEO) && (this->GetFrameType() == FLV_FRAME_KEY));
}

int CFlvPacket::FindPacket(const unsigned char *buffer, unsigned int length, unsigned int minimumFlvPacketsToCheck)
{
  int result = FLV_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER;

  if ((buffer != NULL) && (length >= FLV_PACKET_HEADER_LENGTH))
  {
    result = FLV_FIND_RESULT_NOT_FOUND;
    minimumFlvPacketsToCheck = (minimumFlvPacketsToCheck == FLV_PACKET_MINIMUM_CHECKED_UNSPECIFIED) ? FLV_PACKET_MINIMUM_CHECKED : minimumFlvPacketsToCheck;

    unsigned int firstFlvPacketPosition = UINT_MAX;   // position of first FLV packet
    unsigned int packetsChecked  = 0;                 // checked FLV packets count
    unsigned int processedBytes = 0;                  // processed bytes for correct seek position value

    while ((processedBytes < length) && ((firstFlvPacketPosition == UINT_MAX) || (packetsChecked <= minimumFlvPacketsToCheck)))
    {
      // repeat until first FLV packet is found and verified by at least (minimumFlvPacketsToCheck + 1) another FLV packet

      // try to find flv packets in buffer

      unsigned int i = 0;
      unsigned int flvPacketLength = 0;

      while (i < length)
      {
        // we have to check bytes in whole buffer

        if (((buffer[i] == FLV_PACKET_AUDIO) || (buffer[i] == FLV_PACKET_VIDEO) || (buffer[i] == FLV_PACKET_META)) && (firstFlvPacketPosition == UINT_MAX))
        {
          flvPacketLength = 0;
          // possible audio, video or meta tag

          if ((i + 3) < length)
          {
            // in buffer have to be at least 3 next bytes for FLV packet length
            // remember FLV packet length and possible first FLV packet postion
            flvPacketLength = (buffer[i + 1] << 8 | buffer[i + 2]) << 8 | buffer[i + 3];
            if (flvPacketLength > (length - i))
            {
              // FLV packet length has wrong value, it's after valid data
              firstFlvPacketPosition = UINT_MAX;
              packetsChecked = 0;
              i++;
              continue;
            }
            // the FLV packet length is in valid range
            // remeber first FLV packet position and skip to possible next packet
            firstFlvPacketPosition = i;
            i += flvPacketLength + 15;
            continue;
          }
          else
          {
            // clear first FLV packet position and go to next byte in buffer
            firstFlvPacketPosition = UINT_MAX;
            packetsChecked = 0;
            i++;
            continue;
          }
        }
        else if (((buffer[i] == FLV_PACKET_AUDIO) || (buffer[i] == FLV_PACKET_VIDEO) || (buffer[i] == FLV_PACKET_META)) && (firstFlvPacketPosition != UINT_MAX))
        {
          // possible next packet, verify
          unsigned int previousLength = UINT_MAX;
          unsigned int nextLength = UINT_MAX;

          if (i >= 3)
          {
            // valid range for previous FLV packet length
            previousLength = (buffer[i - 3] << 8 | buffer[i - 2]) << 8 | buffer[i - 1];
          }

          if ((i + 3) < length)
          {
            // valid range for previous FLV packet length
            nextLength = (buffer[i + 1] << 8 | buffer[i + 2]) << 8 | buffer[i + 3];
          }

          if ((previousLength != UINT_MAX) && (nextLength != UINT_MAX))
          {
            if (previousLength == (flvPacketLength + 11))
            {
              // correct value of previous FLV packet length
              // skip to next possible FLV packet
              packetsChecked++;
              i += nextLength + 15;
              flvPacketLength = nextLength;
              continue;
            }
          }

          // bad FLV packet
          i = firstFlvPacketPosition + 1;
          firstFlvPacketPosition = UINT_MAX;
          packetsChecked = 0;
          continue;
        }
        else if (firstFlvPacketPosition != UINT_MAX)
        {
          // FLV packet after first FLV packet not found
          // first FLV packet is not FLV packet
          i = firstFlvPacketPosition + 1;
          firstFlvPacketPosition = UINT_MAX;
          packetsChecked = 0;
          continue;
        }

        // go to next byte in buffer
        i++;
      }

      if (firstFlvPacketPosition == UINT_MAX)
      {
        processedBytes += length;
      }
      else if ((firstFlvPacketPosition != UINT_MAX) && (packetsChecked <= minimumFlvPacketsToCheck))
      {
        processedBytes += length;
        result = FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS;
      }
      else if ((firstFlvPacketPosition != UINT_MAX) && (packetsChecked > minimumFlvPacketsToCheck))
      {
        result = firstFlvPacketPosition;
      }
    }
  }

  return result;
}

int CFlvPacket::FindPacket(CLinearBuffer *buffer, unsigned int minimumFlvPacketsToCheck)
{
  int result = FLV_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER;

  if ((buffer != NULL) && (buffer->GetBufferOccupiedSpace() >= FLV_PACKET_HEADER_LENGTH))
  {
    // at least size for FLV header
    ALLOC_MEM_DEFINE_SET(buf, unsigned char, buffer->GetBufferOccupiedSpace(), 0);
    result = (buf != NULL) ? result : FLV_FIND_RESULT_NOT_ENOUGH_MEMORY;

    if (buf != NULL)
    {
      buffer->CopyFromBuffer(buf, buffer->GetBufferOccupiedSpace());
      result = this->FindPacket(buf, buffer->GetBufferOccupiedSpace(), minimumFlvPacketsToCheck);
    }
    FREE_MEM(buf);
  }

  return result;
}