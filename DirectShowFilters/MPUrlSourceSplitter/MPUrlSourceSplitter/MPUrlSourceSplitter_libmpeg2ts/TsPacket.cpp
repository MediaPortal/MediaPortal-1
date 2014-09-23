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

#include "TsPacket.h"
#include "TsPacketConstants.h"
#include "BufferHelper.h"

CTsPacket::CTsPacket(HRESULT *result)
  : CFlags()
{
  this->header = 0;

  /*if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }*/
}

CTsPacket::~CTsPacket(void)
{
}

/* get methods */

unsigned int CTsPacket::GetPID(void)
{
  return ((this->header & TS_PACKET_HEADER_PID_MASK) >> 8);
}

unsigned int CTsPacket::GetTransportScramblingControl(void)
{
  return ((this->header & TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_MASK) >> 6);
}

unsigned int CTsPacket::GetAdaptationFieldControl(void)
{
  return ((this->header & TS_PACKET_HEADER_ADAPTATION_FIELD_MASK) >> 4);
}
  
unsigned int CTsPacket::GetContinuityCounter(void)
{
  return (this->header & TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK);
}

/* set methods */

/* other methods */

bool CTsPacket::IsParsed(void)
{
  return this->IsSetFlags(TS_PACKET_FLAG_PARSED);
}

bool CTsPacket::IsTransportErrorIndicator(void)
{
  return ((this->header & TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_MASK) != 0);
}

bool CTsPacket::IsPayloadUnitStart(void)
{
  return ((this->header & TS_PACKET_HEADER_PAYLOAD_UNIT_START_MASK) != 0);
}

bool CTsPacket::IsTransportPriority(void)
{
  return ((this->header & TS_PACKET_HEADER_TRANSPORT_PRIORITY_MASK) != 0);
}

bool CTsPacket::Parse(const unsigned char *buffer, uint32_t length)
{
  this->flags &= ~TS_PACKET_FLAG_PARSED;

  if ((buffer != NULL) && (length >= TS_PACKET_HEADER_LENGTH))
  {
    this->header = RBE32(buffer, 0);

    this->flags |= ((this->header & TS_PACKET_HEADER_SYNC_BYTE_MASK) == TS_PACKET_HEADER_SYNC_BYTE) ? TS_PACKET_FLAG_PARSED : TS_PACKET_FLAG_NONE;
  }

  return this->IsSetFlags(TS_PACKET_FLAG_PARSED);
}

/* static methods */

int CTsPacket::FindPacket(const unsigned char *buffer, unsigned int length, unsigned int minimumPacketsToCheck)
{
  int result = TS_PACKET_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER;

  if ((buffer != NULL) && (length >= TS_PACKET_HEADER_LENGTH))
  {
    result = TS_PACKET_FIND_RESULT_NOT_FOUND;
    minimumPacketsToCheck = (minimumPacketsToCheck == TS_PACKET_MINIMUM_CHECKED_UNSPECIFIED) ? TS_PACKET_MINIMUM_CHECKED : minimumPacketsToCheck;

    unsigned int firstPacketPosition = UINT_MAX;      // position of first MPEG2 TS packet
    unsigned int packetsChecked  = 0;                 // checked MPEG2 TS packets count
    unsigned int processedBytes = 0;                  // processed bytes for correct seek position value

    while ((processedBytes < length) && ((firstPacketPosition == UINT_MAX) || (packetsChecked <= minimumPacketsToCheck)))
    {
      // repeat until first TS packet is found and verified by at least (minimumPacketsToCheck + 1) another TS packets
      // try to find TS packets in buffer

      unsigned int i = 0;
      while (i < length)
      {
        // we have to check bytes in whole buffer

        if ((buffer[i] == TS_PACKET_SYNC_BYTE) && (firstPacketPosition == UINT_MAX))
        {
          // possible TS packet

          if ((i + TS_PACKET_SIZE) <= length)
          {
            // in buffer we have whole TS packet
            // remeber first TS packet position and skip to possible next packet

            firstPacketPosition = i;
            i += TS_PACKET_SIZE;
            continue;
          }
          else
          {
            // clear first TS packet position and go to next byte in buffer
            firstPacketPosition = UINT_MAX;
            packetsChecked = 0;
            i++;
            continue;
          }
        }
        else if ((buffer[i] == TS_PACKET_SYNC_BYTE) && (firstPacketPosition != UINT_MAX))
        {
          // possible next packet, continue
          packetsChecked++;
          i += TS_PACKET_SIZE;
          continue;
        }
        else if (firstPacketPosition != UINT_MAX)
        {
          // TS packet after first possible TS packet not found
          // first possible TS packet is not TS packet
          i = firstPacketPosition + 1;
          firstPacketPosition = UINT_MAX;
          packetsChecked = 0;
          continue;
        }

        // go to next byte in buffer
        i++;
      }

      if (firstPacketPosition == UINT_MAX)
      {
        processedBytes += length;
      }
      else if ((firstPacketPosition != UINT_MAX) && (packetsChecked <= minimumPacketsToCheck))
      {
        processedBytes += length;
        result = TS_PACKET_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS;
      }
      else if ((firstPacketPosition != UINT_MAX) && (packetsChecked > minimumPacketsToCheck))
      {
        result = firstPacketPosition;
      }
    }
  }

  return result;
}

int CTsPacket::FindPacket(CLinearBuffer *buffer, unsigned int minimumPacketsToCheck)
{
  int result = TS_PACKET_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER;

  if ((buffer != NULL) && (buffer->GetBufferOccupiedSpace() >= TS_PACKET_HEADER_LENGTH))
  {
    // at least size for MPEG2 TS packet header
    ALLOC_MEM_DEFINE_SET(buf, unsigned char, buffer->GetBufferOccupiedSpace(), 0);
    result = (buf != NULL) ? result : TS_PACKET_FIND_RESULT_NOT_ENOUGH_MEMORY;

    if (buf != NULL)
    {
      buffer->CopyFromBuffer(buf, buffer->GetBufferOccupiedSpace());
      result = CTsPacket::FindPacket(buf, buffer->GetBufferOccupiedSpace(), minimumPacketsToCheck);
    }
    FREE_MEM(buf);
  }

  return result;
}

HRESULT CTsPacket::FindPacketSequence(const unsigned char *buffer, unsigned int length, unsigned int *firstPacketPosition, unsigned int *packetSequenceLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_POINTER_DEFAULT_HRESULT(result, firstPacketPosition);
  CHECK_POINTER_DEFAULT_HRESULT(result, packetSequenceLength);

  if (SUCCEEDED(result))
  {
    *firstPacketPosition = UINT_MAX;
    *packetSequenceLength = 0;

    if (length >= TS_PACKET_HEADER_LENGTH)
    {
      unsigned int packetsChecked  = 0;                 // checked MPEG2 TS packets count
      unsigned int processedBytes = 0;                  // processed bytes for correct seek position value

      while ((processedBytes < length) && (*firstPacketPosition == UINT_MAX))
      {
        // try to find TS packets in buffer

        unsigned int i = 0;
        while (i < length)
        {
          // we have to check bytes in whole buffer

          if ((buffer[i] == TS_PACKET_SYNC_BYTE) && (*firstPacketPosition == UINT_MAX))
          {
            // possible TS packet

            if ((i + TS_PACKET_SIZE) <= length)
            {
              // in buffer we have whole TS packet
              // remeber first TS packet position and skip to possible next packet

              *firstPacketPosition = i;
              i += TS_PACKET_SIZE;
              continue;
            }
            else
            {
              // clear first TS packet position and go to next byte in buffer
              *firstPacketPosition = UINT_MAX;
              packetsChecked = 0;
              i++;
              continue;
            }
          }
          else if ((buffer[i] == TS_PACKET_SYNC_BYTE) && (*firstPacketPosition != UINT_MAX))
          {
            // possible next packet, continue
            packetsChecked++;
            i += TS_PACKET_SIZE;
            continue;
          }
          else if (*firstPacketPosition != UINT_MAX)
          {
            // TS packet after first possible TS packet not found
            // first possible TS packet is not TS packet
            i = *firstPacketPosition + 1;
            *firstPacketPosition = UINT_MAX;
            packetsChecked = 0;
            continue;
          }

          // go to next byte in buffer
          i++;
        }

        if (i == length)
        {
          // we are exactly on position of next MPEG2 TS packet, which starts exactly after buffer
          packetsChecked++;
        }

        if (*firstPacketPosition == UINT_MAX)
        {
          // not found any possible TS packet
          processedBytes += length;
        }
        else
        {
          // found possible TS packet, checked packets count is in packetsChecked
          processedBytes += length;
        }
      }

      if (*firstPacketPosition != UINT_MAX)
      {
        *packetSequenceLength = packetsChecked * TS_PACKET_SIZE;
      }
    }
  }

  return result;
}

HRESULT CTsPacket::FindPacketSequence(CLinearBuffer *buffer, unsigned int *firstPacketPosition, unsigned int *packetSequenceLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, buffer);
  CHECK_POINTER_DEFAULT_HRESULT(result, firstPacketPosition);
  CHECK_POINTER_DEFAULT_HRESULT(result, packetSequenceLength);

  if (SUCCEEDED(result))
  {
    if (buffer->GetBufferOccupiedSpace() >= TS_PACKET_HEADER_LENGTH)
    {
      // at least size for MPEG2 TS packet header
      ALLOC_MEM_DEFINE_SET(buf, unsigned char, buffer->GetBufferOccupiedSpace(), 0);
      result = (buf != NULL) ? result : TS_PACKET_FIND_RESULT_NOT_ENOUGH_MEMORY;

      if (buf != NULL)
      {
        buffer->CopyFromBuffer(buf, buffer->GetBufferOccupiedSpace());
        result = CTsPacket::FindPacketSequence(buf, buffer->GetBufferOccupiedSpace(), firstPacketPosition, packetSequenceLength);
      }
      FREE_MEM(buf);
    }
  }

  return result;
}

/* protected methods */
