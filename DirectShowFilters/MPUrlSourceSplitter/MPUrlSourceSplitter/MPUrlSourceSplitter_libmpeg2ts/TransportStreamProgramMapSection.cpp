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

#include "TransportStreamProgramMapSection.h"
#include "BufferHelper.h"

CTransportStreamProgramMapSection::CTransportStreamProgramMapSection(HRESULT *result)
  : CSection(result)
{
  this->programNumber = 0;
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->pcrPID = 0;
  this->programInfoSize = 0;
  this->programInfoDescriptor = NULL;
  this->programDefinitions = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->programDefinitions = new CProgramDefinitionCollection(result);

    CHECK_POINTER_HRESULT(*result, this->programDefinitions, *result, E_OUTOFMEMORY);
  }
}

CTransportStreamProgramMapSection::~CTransportStreamProgramMapSection(void)
{
  FREE_MEM(this->programInfoDescriptor);
  FREE_MEM_CLASS(this->programDefinitions);
}

/* get methods */

unsigned int CTransportStreamProgramMapSection::GetProgramNumber(void)
{
  return this->programNumber;
}

unsigned int CTransportStreamProgramMapSection::GetVersion(void)
{
  return ((this->reservedVersionNumberCurrentNextIndicator >> TRANSPORT_STREAM_PROGRAM_MAP_SECTION_VERSION_NUMBER_SHIFT) & TRANSPORT_STREAM_PROGRAM_MAP_SECTION_VERSION_NUMBER_MASK);
}

unsigned int CTransportStreamProgramMapSection::GetSectionNumber(void)
{
  return this->sectionNumber;
}

unsigned int CTransportStreamProgramMapSection::GetLastSectionNumber(void)
{
  return this->lastSectionNumber;
}

unsigned int CTransportStreamProgramMapSection::GetPcrPID(void)
{
  return ((this->pcrPID >> TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PCR_PID_SHIFT) & TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PCR_PID_MASK);
}

unsigned int CTransportStreamProgramMapSection::GetProgramInfoSize(void)
{
  return ((this->programInfoSize >> TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_INFO_SIZE_SHIFT) & TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_INFO_SIZE_MASK);
}

const uint8_t *CTransportStreamProgramMapSection::GetProgramInfoDescriptor(void)
{
  return this->programInfoDescriptor;
}

CProgramDefinitionCollection *CTransportStreamProgramMapSection::GetProgramDefinitions(void)
{
  return this->programDefinitions;
}

/* set methods */

void CTransportStreamProgramMapSection::SetProgramNumber(unsigned int programNumber)
{
  this->programNumber = programNumber;
}

/* other methods */

HRESULT CTransportStreamProgramMapSection::Parse(CProgramSpecificInformationPacket *psiPacket, unsigned int startFromSectionPayload)
{
  this->programNumber = 0;
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->pcrPID = 0;
  this->programInfoSize = 0;
  FREE_MEM(this->programInfoDescriptor);

  HRESULT result = __super::Parse(psiPacket, startFromSectionPayload);

  // S_OK is successfull, S_FALSE if more PSI packets are needed to complete section, error code otherwise
  if (result == S_OK)
  {
    CHECK_CONDITION_HRESULT(result, this->GetTableId() == TRANSPORT_STREAM_PROGRAM_MAP_SECTION_TABLE_ID, result, E_FAIL);

    if (SUCCEEDED(result))
    {
      unsigned int position = SECTION_HEADER_SIZE;

      RBE16INC(this->payload, position, this->programNumber);
      RBE8INC(this->payload, position, this->reservedVersionNumberCurrentNextIndicator);
      RBE8INC(this->payload, position, this->sectionNumber);
      RBE8INC(this->payload, position, this->lastSectionNumber);
      RBE16INC(this->payload, position, this->pcrPID);
      RBE16INC(this->payload, position, this->programInfoSize);

      if (this->GetProgramInfoSize() != 0)
      {
        this->programInfoDescriptor = ALLOC_MEM_SET(this->programInfoDescriptor, uint8_t, this->GetProgramInfoSize(), 0);
        CHECK_POINTER_HRESULT(result, this->programInfoDescriptor, result, E_OUTOFMEMORY);
        CHECK_CONDITION_HRESULT(result, (position + this->GetProgramInfoSize()) <= (this->GetSectionSize() - SECTION_CRC32_SIZE), result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), memcpy(this->programInfoDescriptor, this->payload + position, this->GetProgramInfoSize()));
        position += this->GetProgramInfoSize();
      }

      // program definition size is not constant (it depends on ES info size)
      while (SUCCEEDED(result) && (position < (this->GetSectionSize() - SECTION_CRC32_SIZE)))
      {
        RBE8INC_DEFINE(this->payload, position, streamType, uint8_t);
        RBE16INC_DEFINE(this->payload, position, elementaryPID, uint16_t);
        RBE16INC_DEFINE(this->payload, position, esInfoSize, uint16_t);

        elementaryPID = (elementaryPID >> TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ELEMENTARY_PID_SHIFT) & TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ELEMENTARY_PID_MASK;
        esInfoSize = (esInfoSize >> TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ES_INFO_LENGTH_SHIFT) & TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ES_INFO_LENGTH_MASK;

        if (esInfoSize != 0)
        {
          CHECK_CONDITION_HRESULT(result, (position + esInfoSize) <= (this->GetSectionSize() - SECTION_CRC32_SIZE), result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            CProgramDefinition *programDefinition = new CProgramDefinition(&result);
            CHECK_POINTER_HRESULT(result, programDefinition, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              programDefinition->SetStreamType(streamType);
              programDefinition->SetElementaryPID(elementaryPID);
              CHECK_CONDITION_HRESULT(result, programDefinition->SetEsInfoDescriptor(this->payload + position, esInfoSize), result, E_OUTOFMEMORY);
            }

            CHECK_CONDITION_HRESULT(result, this->programDefinitions->Add(programDefinition), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(programDefinition));
          }
        }

        position += esInfoSize;
      }
    }
  }

  return result;
}

void CTransportStreamProgramMapSection::Clear(void)
{
  __super::Clear();

  this->programNumber = 0;
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->pcrPID = 0;
  this->programInfoSize = 0;
  FREE_MEM(this->programInfoDescriptor);
  this->programDefinitions->Clear();
}

/* protected methods */

CSection *CTransportStreamProgramMapSection::CreateItem(void)
{
  HRESULT result = S_OK;
  CTransportStreamProgramMapSection *section = new CTransportStreamProgramMapSection(&result);
  CHECK_POINTER_HRESULT(result, section, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(section));
  return section;
}

bool CTransportStreamProgramMapSection::InternalClone(CSection *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CTransportStreamProgramMapSection *section = dynamic_cast<CTransportStreamProgramMapSection *>(item);
    result &= (section != NULL);

    if (result)
    {
      section->programNumber = this->programNumber;
      section->reservedVersionNumberCurrentNextIndicator = this->reservedVersionNumberCurrentNextIndicator;
      section->sectionNumber = this->sectionNumber;
      section->lastSectionNumber = this->lastSectionNumber;
      section->pcrPID = this->pcrPID;
      section->programInfoSize = this->programInfoSize;

      if (this->GetProgramInfoSize() != 0)
      {
        section->programInfoDescriptor = ALLOC_MEM_SET(section->programInfoDescriptor, uint8_t, this->GetProgramInfoSize(), 0);
        result &= (section->programInfoDescriptor != NULL);

        CHECK_CONDITION_EXECUTE(result, memcpy(section->programInfoDescriptor, this->programInfoDescriptor, this->GetProgramInfoSize()));
      }

      section->programDefinitions->Append(this->programDefinitions);
    }
  }

  return result;
}

unsigned int CTransportStreamProgramMapSection::GetSectionCalculatedSize(void)
{
  unsigned int result = __super::GetSectionCalculatedSize();

  if (result != 0)
  {
    result += 9 + this->GetProgramInfoSize();

    for (unsigned int i = 0; i < this->GetProgramDefinitions()->Count(); i++)
    {
      CProgramDefinition *programDefinition = this->GetProgramDefinitions()->GetItem(i);

      result += 5 + programDefinition->GetEsInfoSize();
    }
  }

  return result;
}

unsigned int CTransportStreamProgramMapSection::GetSectionInternal(void)
{
  unsigned int position = __super::GetSectionInternal();

  if (position != 0)
  {
    WBE16INC(this->payload, position, this->programNumber);
    WBE8INC(this->payload, position, this->reservedVersionNumberCurrentNextIndicator);
    WBE8INC(this->payload, position, this->sectionNumber);
    WBE8INC(this->payload, position, this->lastSectionNumber);
    WBE16INC(this->payload, position, this->pcrPID);
    WBE16INC(this->payload, position, this->programInfoSize);

    if (this->GetProgramInfoSize() != 0)
    {
      memcpy(this->payload + position, this->programInfoDescriptor, this->GetProgramInfoSize());
      position += this->GetProgramInfoSize();
    }

    // program definition size is not constant (it depends on ES info size)
    for (unsigned int i = 0; i < this->GetProgramDefinitions()->Count(); i++)
    {
      CProgramDefinition *programDefinition = this->GetProgramDefinitions()->GetItem(i); 

      WBE8INC(this->payload, position, programDefinition->GetStreamType());

      uint16_t elementaryPID = ((programDefinition->GetElementaryPID() & TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ELEMENTARY_PID_MASK) << TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ELEMENTARY_PID_SHIFT);
      elementaryPID |= ~TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ELEMENTARY_PID_MASK;

      uint16_t esInfoSize = ((programDefinition->GetEsInfoSize() & TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ES_INFO_LENGTH_MASK) << TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ES_INFO_LENGTH_SHIFT);
      esInfoSize |= ~TRANSPORT_STREAM_PROGRAM_MAP_SECTION_PROGRAM_DEFINITION_ES_INFO_LENGTH_MASK;

      WBE16INC(this->payload, position, elementaryPID);
      WBE16INC(this->payload, position, esInfoSize);

      if (programDefinition->GetEsInfoSize() != 0)
      {
        memcpy(this->payload + position, programDefinition->GetEsInfoDescriptor(), programDefinition->GetEsInfoSize());
        position += programDefinition->GetEsInfoSize();
      }
    }
  }

  return position;
}