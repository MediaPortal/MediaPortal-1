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

#include "ProgramAssociationSection.h"
#include "BufferHelper.h"

CProgramAssociationSection::CProgramAssociationSection(HRESULT *result)
  : CSection(result)
{
  this->programs = NULL;
  this->transportStreamId = 0;
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->programs = new CProgramAssociationSectionProgramCollection(result);

    CHECK_POINTER_HRESULT(*result, this->programs, *result, E_OUTOFMEMORY);
  }
}

CProgramAssociationSection::~CProgramAssociationSection(void)
{
  FREE_MEM_CLASS(this->programs);
}

/* get methods */

unsigned int CProgramAssociationSection::GetTransportStreamId(void)
{
  return this->transportStreamId;
}

unsigned int CProgramAssociationSection::GetVersion(void)
{
  return ((this->reservedVersionNumberCurrentNextIndicator >> PROGRAM_ASSOCIATION_SECTION_VERSION_NUMBER_SHIFT) & PROGRAM_ASSOCIATION_SECTION_VERSION_NUMBER_MASK);
}

unsigned int CProgramAssociationSection::GetSectionNumber(void)
{
  return this->sectionNumber;
}

unsigned int CProgramAssociationSection::GetLastSectionNumber(void)
{
  return this->lastSectionNumber;
}

CProgramAssociationSectionProgramCollection *CProgramAssociationSection::GetPrograms(void)
{
  return this->programs;
}

/* set methods */

void CProgramAssociationSection::SetTransportStreamId(unsigned int transportStreamId)
{
  this->transportStreamId = (uint16_t)transportStreamId;
}

void CProgramAssociationSection::SetVersion(unsigned int version)
{
  this->reservedVersionNumberCurrentNextIndicator &= ~(PROGRAM_ASSOCIATION_SECTION_VERSION_NUMBER_MASK << PROGRAM_ASSOCIATION_SECTION_VERSION_NUMBER_SHIFT);
  this->reservedVersionNumberCurrentNextIndicator |= ((version & PROGRAM_ASSOCIATION_SECTION_VERSION_NUMBER_MASK) << PROGRAM_ASSOCIATION_SECTION_VERSION_NUMBER_SHIFT);
}

void CProgramAssociationSection::SetSectionNumber(unsigned int sectionNumber)
{
  this->sectionNumber = (uint8_t)sectionNumber;
}

void CProgramAssociationSection::SetLastSectionNumber(unsigned int lastSectionNumber)
{
  this->lastSectionNumber = (uint8_t)lastSectionNumber;
}

/* other methods */

bool CProgramAssociationSection::IsCurrentNextIndicator(void)
{
  return (((this->reservedVersionNumberCurrentNextIndicator >> PROGRAM_ASSOCIATION_SECTION_CURRENT_NEXT_INDICATOR_SHIFT) & PROGRAM_ASSOCIATION_SECTION_CURRENT_NEXT_INDICATOR_MASK) != 0);
}

HRESULT CProgramAssociationSection::Parse(CProgramSpecificInformationPacket *psiPacket, unsigned int startFromSectionPayload)
{
  this->transportStreamId = 0;
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->programs->Clear();

  HRESULT result = __super::Parse(psiPacket, startFromSectionPayload);

  // S_OK is successfull, S_FALSE if more PSI packets are needed to complete section, error code otherwise
  if (result == S_OK)
  {
    CHECK_CONDITION_HRESULT(result, this->GetTableId() == PROGRAM_ASSOCIATION_SECTION_TABLE_ID, result, E_FAIL);

    if (SUCCEEDED(result))
    {
      unsigned int position = SECTION_HEADER_SIZE;

      RBE16INC(this->payload, position, this->transportStreamId);
      RBE8INC(this->payload, position, this->reservedVersionNumberCurrentNextIndicator);
      RBE8INC(this->payload, position, this->sectionNumber);
      RBE8INC(this->payload, position, this->lastSectionNumber);

      unsigned int programCount = (this->GetSectionPayloadSize() - PROGRAM_ASSOCIATION_SECTION_HEADER_SIZE) / PROGRAM_ASSOCIATION_SECTION_PROGRAM_SIZE;
      CHECK_CONDITION_HRESULT(result, this->programs->EnsureEnoughSpace(programCount), result, E_OUTOFMEMORY);

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < programCount)); i++)
      {
        CProgramAssociationSectionProgram *program = new CProgramAssociationSectionProgram(&result);
        CHECK_POINTER_HRESULT(result, program, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          RBE16INC_DEFINE(this->payload, position, programNumber, uint16_t);
          RBE16INC_DEFINE(this->payload, position, programMapPID, uint16_t);

          program->SetProgramNumber(programNumber);
          program->SetProgramMapPID(programMapPID & PROGRAM_ASSOCIATION_SECTION_PROGRAM_MAP_PID_MASK);
        }

        CHECK_CONDITION_HRESULT(result, this->programs->Add(program), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(program));
      }
    }
  }

  return result;
}

void CProgramAssociationSection::Clear(void)
{
  __super::Clear();

  this->transportStreamId = 0;
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->programs->Clear();
}

/* protected methods */

CSection *CProgramAssociationSection::CreateItem(void)
{
  HRESULT result = S_OK;
  CProgramAssociationSection *section = new CProgramAssociationSection(&result);
  CHECK_POINTER_HRESULT(result, section, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(section));
  return section;
}

bool CProgramAssociationSection::InternalClone(CSection *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CProgramAssociationSection *section = dynamic_cast<CProgramAssociationSection *>(item);
    result &= (section != NULL);

    if (result)
    {
      section->transportStreamId = this->transportStreamId;
      section->reservedVersionNumberCurrentNextIndicator = this->reservedVersionNumberCurrentNextIndicator;
      section->sectionNumber = this->sectionNumber;
      section->lastSectionNumber = this->lastSectionNumber;

      result &= section->programs->Append(this->programs);
    }
  }

  return result;
}

unsigned int CProgramAssociationSection::GetSectionCalculatedSize(void)
{
  unsigned int result = __super::GetSectionCalculatedSize();

  CHECK_CONDITION_EXECUTE(result != 0, result += 5 + this->GetPrograms()->Count() * 4);

  return result;
}

unsigned int CProgramAssociationSection::GetSectionInternal(void)
{
  unsigned int position = __super::GetSectionInternal();

  if (position != 0)
  {
    WBE16INC(this->payload, position, this->transportStreamId);
    WBE8INC(this->payload, position, this->reservedVersionNumberCurrentNextIndicator);
    WBE8INC(this->payload, position, this->sectionNumber);
    WBE8INC(this->payload, position, this->lastSectionNumber);

    for (unsigned int i = 0; i < this->GetPrograms()->Count(); i++)
    {
      CProgramAssociationSectionProgram *program = this->GetPrograms()->GetItem(i);

      WBE16INC(this->payload, position, program->GetProgramNumber());
      WBE16INC(this->payload, position, (program->GetProgramMapPID() | (~PROGRAM_ASSOCIATION_SECTION_PROGRAM_MAP_PID_MASK)));
    }
  }

  return position;
}

bool CProgramAssociationSection::CheckTableId(void)
{
  return (this->GetTableId() == PROGRAM_ASSOCIATION_SECTION_TABLE_ID);
}