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

#include "ProgramSpecificInformationPacket.h"
#include "TsPacketConstants.h"
#include "BufferHelper.h"
#include "Section.h"

CProgramSpecificInformationPacket::CProgramSpecificInformationPacket(HRESULT *result, uint16_t pid, uint8_t tableId)
  : CTsPacket(result)
{
  this->pid = pid;
  this->sectionPayloads = NULL;
  this->tableId = tableId;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    unsigned int header = this->GetHeader();

    header &= ~(TS_PACKET_HEADER_PID_MASK << TS_PACKET_HEADER_PID_SHIFT);
    header |= ((this->pid & TS_PACKET_HEADER_PID_MASK) << TS_PACKET_HEADER_PID_SHIFT);

    this->SetHeader(header);

    this->sectionPayloads = new CSectionPayloadCollection(result);

    CHECK_POINTER_HRESULT(*result, this->sectionPayloads, *result, E_OUTOFMEMORY);
  }
}

CProgramSpecificInformationPacket::CProgramSpecificInformationPacket(HRESULT *result, uint16_t pid, uint8_t tableId, bool reference)
  : CTsPacket(result, reference)
{
  this->pid = pid;
  this->sectionPayloads = NULL;
  this->tableId = tableId;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    unsigned int header = this->GetHeader();

    header &= ~(TS_PACKET_HEADER_PID_MASK << TS_PACKET_HEADER_PID_SHIFT);
    header |= ((this->pid & TS_PACKET_HEADER_PID_MASK) << TS_PACKET_HEADER_PID_SHIFT);

    this->SetHeader(header);

    this->sectionPayloads = new CSectionPayloadCollection(result);

    CHECK_POINTER_HRESULT(*result, this->sectionPayloads, *result, E_OUTOFMEMORY);
  }
}

CProgramSpecificInformationPacket::~CProgramSpecificInformationPacket(void)
{
  FREE_MEM_CLASS(this->sectionPayloads);
}

/* get methods */

CSectionPayloadCollection *CProgramSpecificInformationPacket::GetSectionPayloads(void)
{
  return this->sectionPayloads;
}

uint8_t CProgramSpecificInformationPacket::GetTableId(void)
{
  return this->tableId;
}

/* set methods */

/* other methods */

bool CProgramSpecificInformationPacket::Parse(const unsigned char *buffer, uint32_t length)
{
  this->sectionPayloads->Clear();

  if (__super::Parse(buffer, length, true))
  {
    HRESULT result = S_OK;

    CHECK_CONDITION_HRESULT(result, this->GetPID() == this->pid, result, E_FAIL);
    CHECK_CONDITION_HRESULT(result, __super::Parse(buffer, length, false), result, E_FAIL);

    if (SUCCEEDED(result))
    {
      // it is specified PSI packet
      // if IsPayloadUnitStart() is true, than in this packet starts at least one section

      unsigned int position = TS_PACKET_HEADER_LENGTH + this->GetAdaptationFieldSize();

      if ((this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_ADAPTATION_FIELD) ||
          (this->GetAdaptationFieldControl() == TS_PACKET_ADAPTATION_FIELD_CONTROL_ADAPTATION_FIELD_WITH_PAYLOAD))
      {
        position++;
      }
      
      if (this->IsPayloadUnitStart())
      {
        // there are at least two section payloads

        RBE8INC_DEFINE(buffer, position, pointerField, unsigned int);

        unsigned int payloadPosition = 1;
        const unsigned char *payload = this->GetPayload();
        unsigned int payloadSize = this->GetPayloadSize();

        if (pointerField != 0)
        {
            CSectionPayload *previousPayload = new CSectionPayload(&result, payload + payloadPosition, pointerField, false);
            CHECK_POINTER_HRESULT(result, previousPayload, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->sectionPayloads->Add(previousPayload), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(previousPayload));

            payloadPosition += pointerField;
        }

        // there can be one or more section payloads
        while (payloadPosition < payloadSize)
        {
          if (CSection::GetTableId(payload + payloadPosition, payloadSize - payloadPosition) == this->GetTableId())
          {
            unsigned int sectionPayloadSize = CSection::GetSectionSize(payload + payloadPosition, payloadSize - payloadPosition);
            
            // we have only (payloadSize - payloadPosition) bytes available

            sectionPayloadSize = min(sectionPayloadSize, (payloadSize - payloadPosition));

            CSectionPayload *sectionPayload = new CSectionPayload(&result, payload + payloadPosition, sectionPayloadSize, true);
            CHECK_POINTER_HRESULT(result, sectionPayload, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->sectionPayloads->Add(sectionPayload), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sectionPayload));

            payloadPosition += sectionPayloadSize;
          }
          else
          {
            // no section, we can finish
            break;
          }
        }
      }
      else
      {
        CSectionPayload *payload = new CSectionPayload(&result, this->GetPayload(), this->GetPayloadSize(), false);
        CHECK_POINTER_HRESULT(result, payload, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->sectionPayloads->Add(payload), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payload));
      }
    }

    this->flags &= ~TS_PACKET_FLAG_PARSED;
    this->flags |= SUCCEEDED(result) ? TS_PACKET_FLAG_PARSED : TS_PACKET_FLAG_NONE;
  }

  return this->IsSetFlags(TS_PACKET_FLAG_PARSED);
}

HRESULT CProgramSpecificInformationPacket::ParseSectionData(const uint8_t *sectionData, unsigned int sectionDataSize, bool sectionStart, bool fillStuffingBytes, unsigned int *processedDataSize)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, sectionData);
  CHECK_POINTER_DEFAULT_HRESULT(result, processedDataSize);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->GetPayload());
  CHECK_CONDITION_HRESULT(result, (!sectionStart) || (sectionStart && this->IsPayloadUnitStart()), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    // if IsPayloadUnitStart() is true, than in this packet starts at least one section
    // in that case we must set pointer field only if PROGRAM_SPECIFIC_INFORMATION_PACKET_FLAG_WRITTEN_POINTER_FIELD is not set; otherwise we just left pointer field intact

    unsigned int dataSize = this->GetPayloadSize();
    unsigned int position = 0;
    uint8_t *payload = (uint8_t *)this->GetPayload();
    unsigned int pointerField = 0;

    for (unsigned int i = 0; i < this->sectionPayloads->Count(); i++)
    {
      CSectionPayload *sectionPayload = this->sectionPayloads->GetItem(i);

      pointerField += sectionPayload->GetPayloadSize();
    }

    if (sectionStart && (!this->IsSetFlags(PROGRAM_SPECIFIC_INFORMATION_PACKET_FLAG_WRITTEN_POINTER_FIELD)))
    {
      // one byte for pointer field
      WBE8(payload, position, pointerField);
      this->flags |= PROGRAM_SPECIFIC_INFORMATION_PACKET_FLAG_WRITTEN_POINTER_FIELD;
    }

    position += pointerField;

    if (this->IsPayloadUnitStart())
    {
      position++;
    }

    unsigned int stuffingSize = ((dataSize - position) > sectionDataSize) ? (dataSize - position - sectionDataSize) : 0;

    // copy or fill data in packet
    unsigned int copyDataSize = min((dataSize - position), sectionDataSize);

    if (copyDataSize > 0)
    {
      memcpy(payload + position, sectionData, copyDataSize);

      CSectionPayload *payloadSection = new CSectionPayload(&result, payload + position, copyDataSize, sectionStart);
      CHECK_POINTER_HRESULT(result, payloadSection, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, this->sectionPayloads->Add(payloadSection), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payloadSection));

      position += copyDataSize;
      *processedDataSize = copyDataSize;
    }

    if (fillStuffingBytes && SUCCEEDED(result) && (stuffingSize > 0))
    {
      memset(payload + position, TS_PACKET_STUFFING_BYTE, stuffingSize);
      position += stuffingSize;
    }
  }

  return result;
}

bool CProgramSpecificInformationPacket::IsWrittenPointerField(void)
{
  return this->IsSetFlags(PROGRAM_SPECIFIC_INFORMATION_PACKET_FLAG_WRITTEN_POINTER_FIELD);
}

/* static methods */

/* protected methods */

CTsPacket *CProgramSpecificInformationPacket::CreateItem(void)
{
  HRESULT result = S_OK;
  CProgramSpecificInformationPacket *packet = new CProgramSpecificInformationPacket(&result, this->pid, this->tableId, this->IsSetFlags(TS_PACKET_FLAG_REFERENCE));
  CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
  return packet;
}

bool CProgramSpecificInformationPacket::InternalClone(CTsPacket *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CProgramSpecificInformationPacket *packet = dynamic_cast<CProgramSpecificInformationPacket *>(item);
    result &= (packet != NULL);

    if (result)
    {
      packet->pid = this->pid;

      result &= packet->sectionPayloads->Append(this->sectionPayloads);
    }
  }

  return result;
}