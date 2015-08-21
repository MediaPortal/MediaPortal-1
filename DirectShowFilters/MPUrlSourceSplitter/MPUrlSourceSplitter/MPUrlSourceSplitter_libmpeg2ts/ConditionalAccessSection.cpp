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

#include "ConditionalAccessSection.h"
#include "BufferHelper.h"
#include "DescriptorFactory.h"

CConditionalAccessSection::CConditionalAccessSection(HRESULT *result)
  : CSection(result)
{
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->descriptors = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->descriptors = new CDescriptorCollection(result);

    CHECK_POINTER_HRESULT(*result, this->descriptors, *result, E_OUTOFMEMORY);
  }
}

CConditionalAccessSection::~CConditionalAccessSection()
{
  FREE_MEM_CLASS(this->descriptors);
}

/* get methods */

unsigned int CConditionalAccessSection::GetVersion(void)
{
  return ((this->reservedVersionNumberCurrentNextIndicator >> CONDITIONAL_ACCESS_SECTION_VERSION_NUMBER_SHIFT) & CONDITIONAL_ACCESS_SECTION_VERSION_NUMBER_MASK);
}

unsigned int CConditionalAccessSection::GetSectionNumber(void)
{
  return this->sectionNumber;
}

unsigned int CConditionalAccessSection::GetLastSectionNumber(void)
{
  return this->lastSectionNumber;
}

CDescriptorCollection *CConditionalAccessSection::GetDescriptors(void)
{
  return this->descriptors;
}

/* set methods */

/* other methods */

bool CConditionalAccessSection::IsCurrentNextIndicator(void)
{
  return (((this->reservedVersionNumberCurrentNextIndicator >> CONDITIONAL_ACCESS_SECTION_CURRENT_NEXT_INDICATOR_SHIFT) & CONDITIONAL_ACCESS_SECTION_CURRENT_NEXT_INDICATOR_MASK) != 0);
}

HRESULT CConditionalAccessSection::Parse(CSectionPayload *sectionPayload)
{
  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->descriptors->Clear();

  HRESULT result = __super::Parse(sectionPayload);

  // S_OK is successfull, S_FALSE if more section payloads are needed to complete section, error code otherwise
  if (result == S_OK)
  {
    CHECK_CONDITION_HRESULT(result, this->GetTableId() == CONDITIONAL_ACCESS_SECTION_TABLE_ID, result, E_FAIL);

    if (SUCCEEDED(result))
    {
      unsigned int position = SECTION_HEADER_SIZE;

      position += 2;

      RBE8INC(this->payload, position, this->reservedVersionNumberCurrentNextIndicator);
      RBE8INC(this->payload, position, this->sectionNumber);
      RBE8INC(this->payload, position, this->lastSectionNumber);

      // parse descriptors (if needed)
      if (SUCCEEDED(result) && (position < (this->GetSectionSize() - SECTION_CRC32_SIZE)))
      {
        CDescriptorFactory *factory = new CDescriptorFactory(&result);
        CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

        uint16_t processed = 0;
        uint16_t descriptorSize = this->GetSectionSize() - position - SECTION_CRC32_SIZE;

        while (SUCCEEDED(result) && (processed < descriptorSize))
        {
          CDescriptor *descriptor = factory->CreateDescriptor(this->payload + position + processed, descriptorSize - processed);
          CHECK_POINTER_HRESULT(result, descriptor, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->descriptors->Add(descriptor), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(descriptor));

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), processed += descriptor->GetDescriptorSize());
        }

        position += processed;
        FREE_MEM_CLASS(factory);
      }
    }

  }

  return result;
}

void CConditionalAccessSection::Clear(void)
{
  __super::Clear();

  this->reservedVersionNumberCurrentNextIndicator = 0;
  this->sectionNumber = 0;
  this->lastSectionNumber = 0;
  this->descriptors->Clear();
}

/* protected methods */

CSection *CConditionalAccessSection::CreateItem(void)
{
  HRESULT result = S_OK;
  CConditionalAccessSection *section = new CConditionalAccessSection(&result);
  CHECK_POINTER_HRESULT(result, section, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(section));
  return section;
}

bool CConditionalAccessSection::InternalClone(CSection *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CConditionalAccessSection *section = dynamic_cast<CConditionalAccessSection *>(item);
    result &= (section != NULL);

    if (result)
    {
      section->reservedVersionNumberCurrentNextIndicator = this->reservedVersionNumberCurrentNextIndicator;
      section->sectionNumber = this->sectionNumber;
      section->lastSectionNumber = this->lastSectionNumber;

      result &= section->descriptors->Append(this->descriptors);
    }
  }

  return result;
}

unsigned int CConditionalAccessSection::GetSectionCalculatedSize(void)
{
  unsigned int result = __super::GetSectionCalculatedSize();

  if (result != 0)
  {
    result += 5;

    for (unsigned int i = 0; i < this->GetDescriptors()->Count(); i++)
    {
      CDescriptor *descriptor = this->GetDescriptors()->GetItem(i);

      result += descriptor->GetDescriptorSize();
    }
  }

  return result;
}

unsigned int CConditionalAccessSection::GetSectionInternal(void)
{
  unsigned int position = __super::GetSectionInternal();

  if (position != 0)
  {
    WBE8INC(this->payload, position, this->reservedVersionNumberCurrentNextIndicator);
    WBE8INC(this->payload, position, this->sectionNumber);
    WBE8INC(this->payload, position, this->lastSectionNumber);

    for (unsigned int i = 0; i < this->GetDescriptors()->Count(); i++)
    {
      CDescriptor *descriptor = this->GetDescriptors()->GetItem(i);

      WBE8INC(this->payload, position, descriptor->GetTag());
      WBE8INC(this->payload, position, descriptor->GetPayloadSize());

      if (descriptor->GetPayloadSize() != 0)
      {
        memcpy(this->payload + position, descriptor->GetPayload(), descriptor->GetPayloadSize());
        position += descriptor->GetPayloadSize();
      }
    }
  }

  return position;
}

bool CConditionalAccessSection::CheckTableId(void)
{
  return (this->GetTableId() == CONDITIONAL_ACCESS_SECTION_TABLE_ID);
}