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
  this->packet = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->packet = ALLOC_MEM_SET(this->packet, uint8_t, TS_PACKET_SIZE, 0);

    CHECK_POINTER_HRESULT(*result, this->packet, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      unsigned int header = ((TS_PACKET_SYNC_BYTE & TS_PACKET_HEADER_SYNC_BYTE_MASK) << TS_PACKET_HEADER_SYNC_BYTE_SHIFT);

      WBE32(this->packet, 0, header);
    }
  }
}

CTsPacket::~CTsPacket(void)
{
  FREE_MEM(this->packet);
}

/* get methods */

unsigned int CTsPacket::GetPID(void)
{
  unsigned int header = RBE32(this->packet, 0);

  return (((header >> TS_PACKET_HEADER_PID_SHIFT) & TS_PACKET_HEADER_PID_MASK));
}

unsigned int CTsPacket::GetTransportScramblingControl(void)
{
  unsigned int header = RBE32(this->packet, 0);

  return (((header >> TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_SHIFT) & TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_MASK));
}

unsigned int CTsPacket::GetAdaptationFieldControl(void)
{
  unsigned int header = RBE32(this->packet, 0);

  return (((header >> TS_PACKET_HEADER_ADAPTATION_FIELD_SHIFT) & TS_PACKET_HEADER_ADAPTATION_FIELD_MASK));
}
  
unsigned int CTsPacket::GetContinuityCounter(void)
{
  unsigned int header = RBE32(this->packet, 0);

  return (((header >> TS_PACKET_HEADER_CONTINUITY_COUNTER_SHIFT) & TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK));
}

unsigned int CTsPacket::GetAdaptationFieldSize(void)
{
  unsigned int adaptationFieldSize = 0;

  if ((this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_ADAPTATION_FIELD) ||
    (this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ADAPTATION_FIELD_WITH_PAYLOAD))
  {
    // adaptation field in MPEG2 TS packet
    adaptationFieldSize = RBE8(this->packet, TS_PACKET_HEADER_LENGTH);
  }

  return adaptationFieldSize;
}

unsigned int CTsPacket::GetPayloadSize(void)
{
  unsigned int payloadSize = 0;

  if ((this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_PAYLOAD) ||
    (this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ADAPTATION_FIELD_WITH_PAYLOAD))
  {
    payloadSize = TS_PACKET_SIZE - this->GetAdaptationFieldSize() - TS_PACKET_HEADER_LENGTH;
  }

  return payloadSize;
}

const uint8_t *CTsPacket::GetPayload(void)
{
  const uint8_t *payload = NULL;

  if ((this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_PAYLOAD) ||
    (this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ADAPTATION_FIELD_WITH_PAYLOAD))
  {
    payload = this->packet + TS_PACKET_HEADER_LENGTH + this->GetAdaptationFieldSize();
  }

  return payload;
}

const uint8_t *CTsPacket::GetPacket(void)
{
  return this->packet;
}

/* set methods */

void CTsPacket::SetPID(unsigned int pid)
{
  unsigned int header = RBE32(this->packet, 0);

  header &= ~(TS_PACKET_HEADER_PID_MASK << TS_PACKET_HEADER_PID_SHIFT);
  header |= ((pid & TS_PACKET_HEADER_PID_MASK) << TS_PACKET_HEADER_PID_SHIFT);

  WBE32(this->packet, 0, header);
}

void CTsPacket::SetTransportScramblingControl(unsigned int transportScramblingControl)
{
  unsigned int header = RBE32(this->packet, 0);

  header &= ~(TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_MASK << TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_SHIFT);
  header |= ((transportScramblingControl & TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_MASK) << TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_SHIFT);

  WBE32(this->packet, 0, header);
}

void CTsPacket::SetAdaptationFieldControl(unsigned int adaptationFieldControl)
{
  unsigned int header = RBE32(this->packet, 0);

  header &= ~(TS_PACKET_HEADER_ADAPTATION_FIELD_MASK << TS_PACKET_HEADER_ADAPTATION_FIELD_SHIFT);
  header |= ((adaptationFieldControl & TS_PACKET_HEADER_ADAPTATION_FIELD_MASK) << TS_PACKET_HEADER_ADAPTATION_FIELD_SHIFT);

  WBE32(this->packet, 0, header);
}
  
void CTsPacket::SetContinuityCounter(unsigned int continuityCounter)
{
  unsigned int header = RBE32(this->packet, 0);

  header &= ~(TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK << TS_PACKET_HEADER_CONTINUITY_COUNTER_SHIFT);
  header |= ((continuityCounter & TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK) << TS_PACKET_HEADER_CONTINUITY_COUNTER_SHIFT);

  WBE32(this->packet, 0, header);
}

bool CTsPacket::SetPayload(const uint8_t *payload, unsigned int payloadSize)
{
  HRESULT result = S_OK;
  CHECK_CONDITION_HRESULT(result, payloadSize == this->GetPayloadSize(), result, E_INVALIDARG);

  if (payloadSize != 0)
  {
    CHECK_POINTER_DEFAULT_HRESULT(result, payload);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), memcpy((uint8_t *)this->GetPayload(), payload, payloadSize));
  }

  return SUCCEEDED(result);
}

void CTsPacket::SetTransportErrorIndicator(bool transportErrorIndicator)
{
  unsigned int header = RBE32(this->packet, 0);

  header &= ~(TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_MASK << TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_SHIFT);
  header |= transportErrorIndicator ? (TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_MASK << TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_SHIFT) : 0;

  WBE32(this->packet, 0, header);
}

void CTsPacket::SetPayloadUnitStart(bool payloadUnitStart)
{
  unsigned int header = RBE32(this->packet, 0);

  header &= ~(TS_PACKET_HEADER_PAYLOAD_UNIT_START_MASK << TS_PACKET_HEADER_PAYLOAD_UNIT_START_SHIFT);
  header |= payloadUnitStart ? (TS_PACKET_HEADER_PAYLOAD_UNIT_START_MASK << TS_PACKET_HEADER_PAYLOAD_UNIT_START_SHIFT) : 0;

  WBE32(this->packet, 0, header);
}

void CTsPacket::SetTransportPriority(bool transportPriority)
{
  unsigned int header = RBE32(this->packet, 0);

  header &= ~(TS_PACKET_HEADER_TRANSPORT_PRIORITY_MASK << TS_PACKET_HEADER_TRANSPORT_PRIORITY_SHIFT);
  header |= transportPriority ? (TS_PACKET_HEADER_TRANSPORT_PRIORITY_MASK << TS_PACKET_HEADER_TRANSPORT_PRIORITY_SHIFT) : 0;

  WBE32(this->packet, 0, header);
}

/* other methods */

bool CTsPacket::IsParsed(void)
{
  return this->IsSetFlags(TS_PACKET_FLAG_PARSED);
}

bool CTsPacket::IsTransportErrorIndicator(void)
{
  unsigned int header = RBE32(this->packet, 0);

  return (((header >> TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_SHIFT) & TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_MASK) != 0);
}

bool CTsPacket::IsPayloadUnitStart(void)
{
  unsigned int header = RBE32(this->packet, 0);

  return (((header >> TS_PACKET_HEADER_PAYLOAD_UNIT_START_SHIFT) & TS_PACKET_HEADER_PAYLOAD_UNIT_START_MASK) != 0);
}

bool CTsPacket::IsTransportPriority(void)
{
  unsigned int header = RBE32(this->packet, 0);

  return (((header >> TS_PACKET_HEADER_TRANSPORT_PRIORITY_SHIFT) & TS_PACKET_HEADER_TRANSPORT_PRIORITY_MASK) != 0);
}

bool CTsPacket::Parse(const unsigned char *buffer, uint32_t length)
{
  return this->Parse(buffer, length, false);
}

CTsPacket *CTsPacket::Clone(void)
{
  CTsPacket *result = this->CreateItem();

  if (result != NULL)
  {
    if (!this->InternalClone(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
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

CTsPacket *CTsPacket::CreateNullPacket(void)
{
  return CTsPacket::CreateNullPacket(TS_PACKET_NULL_PAYLOAD_BYTE);
}

CTsPacket *CTsPacket::CreateNullPacket(uint8_t dataByte)
{
  CTsPacket *packet = NULL;
  HRESULT result = S_OK;
  ALLOC_MEM_DEFINE_SET(payload, uint8_t, TS_PACKET_MAXIMUM_PAYLOAD_SIZE, dataByte);
  CHECK_POINTER_HRESULT(result, payload, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), packet = CTsPacket::CreateNullPacket(payload, TS_PACKET_MAXIMUM_PAYLOAD_SIZE));

  FREE_MEM(payload);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
  return packet;
}

CTsPacket *CTsPacket::CreateNullPacket(const uint8_t *payload, unsigned int payloadSize)
{
  CTsPacket *packet = NULL;
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, payload);
  CHECK_CONDITION_HRESULT(result, payloadSize == TS_PACKET_MAXIMUM_PAYLOAD_SIZE, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    packet = new CTsPacket(&result);
    CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      packet->SetPID(TS_PACKET_PID_NULL);
      packet->SetAdaptationFieldControl(TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_PAYLOAD);
      CHECK_CONDITION_HRESULT(result, packet->SetPayload(payload, payloadSize), result, E_OUTOFMEMORY);
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
  return packet;
}

/* protected methods */

bool CTsPacket::Parse(const unsigned char *buffer, uint32_t length, bool onlyHeader)
{
  this->flags &= ~TS_PACKET_FLAG_PARSED;

  if ((buffer != NULL) && (length >= TS_PACKET_HEADER_LENGTH))
  {
    memcpy(this->packet, buffer, TS_PACKET_HEADER_LENGTH);
    unsigned int header = RBE32(buffer, 0);
    
    this->flags |= (((header >> TS_PACKET_HEADER_SYNC_BYTE_SHIFT) & TS_PACKET_HEADER_SYNC_BYTE_MASK) == TS_PACKET_SYNC_BYTE) ? TS_PACKET_FLAG_PARSED : TS_PACKET_FLAG_NONE;

    if (this->IsSetFlags(TS_PACKET_FLAG_PARSED) && (!onlyHeader))
    {
      unsigned int position = TS_PACKET_HEADER_LENGTH;
      HRESULT result = S_OK;

      memcpy(this->packet, buffer, TS_PACKET_SIZE);

      if ((this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_ADAPTATION_FIELD) ||
        (this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ADAPTATION_FIELD_WITH_PAYLOAD))
      {
        // adaptation field in MPEG2 TS packet
        // check adaptation field size
        RBE8INC_DEFINE(buffer, position, adaptationFieldSize, unsigned int);
        CHECK_CONDITION_HRESULT(result, (position + adaptationFieldSize) <= TS_PACKET_SIZE, result, E_OUTOFMEMORY);
      }

      this->flags &= ~TS_PACKET_FLAG_PARSED;
      this->flags |= SUCCEEDED(result) ? TS_PACKET_FLAG_PARSED : TS_PACKET_FLAG_NONE;
    }
  }

  return this->IsSetFlags(TS_PACKET_FLAG_PARSED);
}

CTsPacket *CTsPacket::CreateItem(void)
{
  HRESULT result = S_OK;
  CTsPacket *packet = new CTsPacket(&result);
  CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
  return packet;
}

bool CTsPacket::InternalClone(CTsPacket *item)
{
  bool result = (item != NULL);

  if (result)
  {
    item->flags = this->flags;
    memcpy(item->packet, this->packet, TS_PACKET_SIZE);
  }

  return result;
}