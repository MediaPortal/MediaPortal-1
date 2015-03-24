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

CProgramSpecificInformationPacket::CProgramSpecificInformationPacket(HRESULT *result, uint16_t pid)
  : CTsPacket(result)
{
  this->pid = pid;
  this->sectionPayloads = NULL;

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

CProgramSpecificInformationPacket::CProgramSpecificInformationPacket(HRESULT *result, uint16_t pid, bool reference)
  : CTsPacket(result, reference)
{
  this->pid = pid;
  this->sectionPayloads = NULL;

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
        RBE8INC_DEFINE(buffer, position, pointerField, unsigned int);

        if (pointerField != 0)
        {
          // split payload into two section payloads
          CSectionPayload *previousPayload = new CSectionPayload(&result, this->GetPayload() + 1, pointerField, false);
          CHECK_POINTER_HRESULT(result, previousPayload, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->sectionPayloads->Add(previousPayload), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(previousPayload));
        }

        CSectionPayload *payload = new CSectionPayload(&result, this->GetPayload() + 1 + pointerField, this->GetPayloadSize() - pointerField - 1, true);
        CHECK_POINTER_HRESULT(result, payload, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->sectionPayloads->Add(payload), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payload));
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

unsigned int CProgramSpecificInformationPacket::ParseSectionData(const uint8_t *sectionData, unsigned int sectionDataSize)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, sectionData);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->GetPayload());

  unsigned int processed = 0;

  if (SUCCEEDED(result))
  {
    // we allow only one section in PSI packet
    this->sectionPayloads->Clear();

    // if IsPayloadUnitStart() is true, than in this packet starts at least one section
    // in that case we must set pointer field

    unsigned int dataSize = this->GetPayloadSize();
    unsigned int position = 0;
    uint8_t *payload = (uint8_t *)this->GetPayload();
    unsigned int pointerField = 0;

    if (this->IsPayloadUnitStart())
    {
      // one byte for pointer field
      WBE8INC(payload, position, pointerField);
      dataSize--;
    }

    unsigned int stuffingSize = (dataSize > sectionDataSize) ? (dataSize - sectionDataSize) : 0;

    // copy or fill data in packet
    unsigned int copyDataSize = min(dataSize, sectionDataSize);
    memcpy(payload + position, sectionData, copyDataSize);
    position += copyDataSize;
    processed += copyDataSize;

    if (stuffingSize > 0)
    {
      memset(payload + position, TS_PACKET_STUFFING_BYTE, stuffingSize);
      position += stuffingSize;
    }

    CSectionPayload *payloadSection = new CSectionPayload(&result, this->GetPayload() + 1 + pointerField, this->GetPayloadSize() - pointerField - 1, this->IsPayloadUnitStart());
    CHECK_POINTER_HRESULT(result, payloadSection, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, this->sectionPayloads->Add(payloadSection), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payloadSection));
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), processed = 0);
  return processed;
}

/* static methods */

CTsPacketCollection *CProgramSpecificInformationPacket::SplitSectionInProgramSpecificInformationPackets(CSection *section, unsigned int packetPID, unsigned int continuityCounter)
{
  HRESULT result = S_OK;
  CTsPacketCollection *packets = new CTsPacketCollection(&result);
  CHECK_POINTER_HRESULT(result, packets, result, E_OUTOFMEMORY);
  CHECK_POINTER_DEFAULT_HRESULT(result, section);
  CHECK_CONDITION_HRESULT(result, packetPID < TS_PACKET_PID_NULL, result, E_INVALIDARG);
  CHECK_CONDITION_HRESULT(result, continuityCounter <= TS_PACKET_MAXIMUM_CONTINUITY_COUNTER, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    unsigned int sectionSize = section->GetSectionSize();
    const uint8_t *sectionData = section->GetSection();
    unsigned int processed = 0;

    while (SUCCEEDED(result) && (processed < sectionSize))
    {
      CProgramSpecificInformationPacket *psiPacket = new CProgramSpecificInformationPacket(&result, packetPID);
      CHECK_POINTER_HRESULT(result, psiPacket, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        psiPacket->SetAdaptationFieldControl(TS_PACKET_ADAPTATION_FIELD_CONTROL_ONLY_PAYLOAD);
        psiPacket->SetPayloadUnitStart(processed == 0);
        psiPacket->SetContinuityCounter(continuityCounter);

        continuityCounter++;
        continuityCounter &= TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK;

        unsigned int parsed = psiPacket->ParseSectionData(sectionData + processed, sectionSize - processed);
        CHECK_CONDITION_HRESULT(result, parsed != 0, result, E_FAIL);

        processed += parsed;
      }

      CHECK_CONDITION_HRESULT(result, packets->Add(psiPacket), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(psiPacket));
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packets));
  return packets;
}

/* protected methods */

CTsPacket *CProgramSpecificInformationPacket::CreateItem(void)
{
  HRESULT result = S_OK;
  CProgramSpecificInformationPacket *packet = new CProgramSpecificInformationPacket(&result, this->pid, this->IsSetFlags(TS_PACKET_FLAG_REFERENCE));
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