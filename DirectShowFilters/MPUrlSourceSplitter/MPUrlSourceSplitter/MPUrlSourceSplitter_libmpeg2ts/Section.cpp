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

#include "Section.h"
#include "BufferHelper.h"
#include "ErrorCodes.h"
#include "Crc32.h"
#include "TsPacketConstants.h"

CSection::CSection(HRESULT *result)
  : CFlags()
{
  this->flags |= SECTION_FLAG_CHECK_CRC32 | SECTION_FLAG_EMPTY_SECTION;
  this->payload = NULL;
  this->currentPayloadSize = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->payload = ALLOC_MEM_SET(this->payload, uint8_t, SECTION_MAX_SIZE, 0);

    CHECK_POINTER_HRESULT(*result, this->payload, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      this->currentPayloadSize = SECTION_HEADER_SIZE;
      WBE24(this->payload, 0, (SECTION_HEADER_RESERVED_MASK << SECTION_HEADER_RESERVED_SHIFT));
    }
  }
}

CSection::~CSection(void)
{
  FREE_MEM(this->payload);
}

/* get methods */

unsigned int CSection::GetTableId(void)
{
  unsigned int header = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;

  return ((header >> SECTION_HEADER_TABLE_ID_SHIFT) & SECTION_HEADER_TABLE_ID_MASK);
}

unsigned int CSection::GetReserved(void)
{
  unsigned int header = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;

  return ((header >> SECTION_HEADER_RESERVED_SHIFT) & SECTION_HEADER_RESERVED_MASK);
}

unsigned int CSection::GetSectionSize(void)
{
  unsigned int header = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;
  header = ((header >> SECTION_HEADER_SECTION_LENGTH_SHIFT) & SECTION_HEADER_SECTION_LENGTH_MASK);

  header += (header != 0) ? SECTION_HEADER_SIZE : 0;

  return (header == 0) ? this->GetSectionCalculatedSize() : header;
}

unsigned int CSection::GetSectionPayloadSize(void)
{
  unsigned int header = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;
  header = ((header >> SECTION_HEADER_SECTION_LENGTH_SHIFT) & SECTION_HEADER_SECTION_LENGTH_MASK);
  header = (header == 0) ? this->GetSectionCalculatedSize() : header;

  header -= ((header != 0) && this->IsSetFlags(SECTION_FLAG_CHECK_CRC32)) ? SECTION_CRC32_SIZE : 0;

  return header;
}

const uint8_t *CSection::GetSection(void)
{
  unsigned int size = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;
  size = ((size >> SECTION_HEADER_SECTION_LENGTH_SHIFT) & SECTION_HEADER_SECTION_LENGTH_MASK);

  size += (size != 0) ? SECTION_HEADER_SIZE : 0;

  if (size == 0)
  {
    // header size is still zero, so section size is not written into payload

    size = this->GetSectionInternal();

    if (size != 0)
    {
      this->currentPayloadSize = size;

      if (this->IsSetFlags(SECTION_FLAG_CHECK_CRC32))
      {
        size = this->CalculateSectionCrc32();
      }

      if (size != 0)
      {
        this->currentPayloadSize = size;
      }
    }
  }

  return (size != 0) ? this->payload : NULL;
}

/* set methods */

void CSection::SetTableId(unsigned int tableId)
{
  unsigned int header = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;
  header &= ~(SECTION_HEADER_TABLE_ID_MASK << SECTION_HEADER_TABLE_ID_SHIFT);
  header |= ((tableId & SECTION_HEADER_TABLE_ID_MASK) << SECTION_HEADER_TABLE_ID_SHIFT);

  WBE24(this->payload, 0, header);
}

/* other methods */

bool CSection::IsSectionEmpty(void)
{
  return this->IsSetFlags(SECTION_FLAG_EMPTY_SECTION);
}

bool CSection::IsSectionIncomplete(void)
{
  return this->IsSetFlags(SECTION_FLAG_INCOMPLETE_SECTION);
}

bool CSection::IsSectionComplete(void)
{
  return this->IsSetFlags(SECTION_FLAG_COMPLETE_SECTION);
}

bool CSection::IsSectionSyntaxIndicator(void)
{
  unsigned int header = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;

  return (((header >> SECTION_HEADER_SECTION_SYNTAX_INDICATOR_SHIFT) & SECTION_HEADER_SECTION_SYNTAX_INDICATOR_MASK) != 0);
}

bool CSection::IsPrivateIndicator(void)
{
  unsigned int header = (this->currentPayloadSize >= SECTION_HEADER_SIZE) ? RBE24(this->payload, 0) : 0;

  return (((header >> SECTION_HEADER_PRIVATE_INDICATOR_SHIFT) & SECTION_HEADER_PRIVATE_INDICATOR_MASK) != 0);
}

HRESULT CSection::Parse(CProgramSpecificInformationPacket *psiPacket, unsigned int startFromSectionPayload)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, psiPacket);

  if (SUCCEEDED(result))
  {
    if (this->IsSetFlags(SECTION_FLAG_EMPTY_SECTION) && (!psiPacket->IsPayloadUnitStart()))
    {
      result = E_MPEG2TS_EMPTY_SECTION_AND_PSI_PACKET_WITHOUT_NEW_SECTION;
    }

    if (this->IsSetFlags(SECTION_FLAG_INCOMPLETE_SECTION))
    {
      // check PSI packet for section end

      for (unsigned int i = startFromSectionPayload; i < psiPacket->GetSectionPayloads()->Count(); i++)
      {
        CSectionPayload *sectionPayload = psiPacket->GetSectionPayloads()->GetItem(i);

        if (sectionPayload->IsPayloadUnitStart())
        {
          // started new section, following data are for new section
          // check size of section
          if ((this->currentPayloadSize >= SECTION_HEADER_SIZE) && this->IsSetFlags(SECTION_FLAG_INCOMPLETE_SECTION) && (this->GetSectionSize() <= this->currentPayloadSize))
          {
            this->flags &= ~SECTION_FLAG_INCOMPLETE_SECTION;
            this->flags |= SECTION_FLAG_COMPLETE_SECTION;
          }
          else
          {
            result = E_MPEG2TS_INCOMPLETE_SECTION;
          }
          break;
        }

        memcpy(this->payload + this->currentPayloadSize, sectionPayload->GetPayload(), sectionPayload->GetPayloadSize());
        this->currentPayloadSize += sectionPayload->GetPayloadSize();
      }
    }

    if (SUCCEEDED(result) && this->IsSetFlags(SECTION_FLAG_EMPTY_SECTION) && psiPacket->IsPayloadUnitStart())
    {
      // there is at least one section in PSI packet
      // current section instance is empty, we can read PSI section

      CSectionPayload *sectionPayload = NULL;
      for (unsigned int i = startFromSectionPayload; i < psiPacket->GetSectionPayloads()->Count(); i++)
      {
        CSectionPayload *temp = psiPacket->GetSectionPayloads()->GetItem(i);

        if (temp->IsPayloadUnitStart())
        {
          sectionPayload = temp;
          break;
        }
      }

      this->currentPayloadSize = 0;

      memcpy(this->payload + this->currentPayloadSize, sectionPayload->GetPayload(), sectionPayload->GetPayloadSize());
      this->currentPayloadSize += sectionPayload->GetPayloadSize();

      this->flags &= ~SECTION_FLAG_EMPTY_SECTION;
      this->flags |= SECTION_FLAG_INCOMPLETE_SECTION;
    }

    // check if we have enough data in section
    if (SUCCEEDED(result) && (this->currentPayloadSize >= SECTION_HEADER_SIZE) && this->IsSetFlags(SECTION_FLAG_INCOMPLETE_SECTION) && (this->GetSectionSize() <= this->currentPayloadSize))
    {
      this->flags &= ~SECTION_FLAG_INCOMPLETE_SECTION;
      this->flags |= SECTION_FLAG_COMPLETE_SECTION;
    }

    if (SUCCEEDED(result) && this->IsSetFlags(SECTION_FLAG_COMPLETE_SECTION | SECTION_FLAG_CHECK_CRC32))
    {
      // complete section, check CRC32
      CHECK_CONDITION_HRESULT(result, this->GetSectionSize() >= (SECTION_HEADER_SIZE + SECTION_CRC32_SIZE), result, E_MPEG2TS_SECTION_INVALID_CRC32);

      if (SUCCEEDED(result))
      {
        uint32_t sectionCrc32 = RBE32(this->payload, this->GetSectionSize() - SECTION_CRC32_SIZE);
        uint32_t sectionCalculatedCrc32 = CCrc32::Calculate(this->payload, this->GetSectionSize() - SECTION_CRC32_SIZE);

        CHECK_CONDITION_HRESULT(result, sectionCrc32 == sectionCalculatedCrc32, result, E_MPEG2TS_SECTION_INVALID_CRC32);
      }
    }

    if (SUCCEEDED(result))
    {
      result = this->IsSetFlags(SECTION_FLAG_INCOMPLETE_SECTION) ? S_FALSE : S_OK;
    }
  }

  return result;
}

void CSection::Clear(void)
{
  this->flags = SECTION_FLAG_NONE;
  this->flags |= SECTION_FLAG_CHECK_CRC32 | SECTION_FLAG_EMPTY_SECTION;
  this->currentPayloadSize = SECTION_HEADER_SIZE;
  WBE24(this->payload, 0, (SECTION_HEADER_RESERVED_MASK << SECTION_HEADER_RESERVED_SHIFT));
}

CSection *CSection::Clone(void)
{
  CSection *result = this->CreateItem();

  if (result != NULL)
  {
    if (!this->InternalClone(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
}

void CSection::ResetSize(void)
{
  if (this->currentPayloadSize >= SECTION_HEADER_SIZE)
  {
    unsigned int header =  RBE24(this->payload, 0);
    header &= ~(SECTION_HEADER_SECTION_LENGTH_MASK << SECTION_HEADER_SECTION_LENGTH_SHIFT);

    WBE24(this->payload, 0, header);
  }  
}

/* protected methods */

bool CSection::InternalClone(CSection *item)
{
  bool result = (item != NULL);

  if (result)
  {
    item->flags = this->flags;
    item->currentPayloadSize = this->currentPayloadSize;

    // item->payload is always created, just copy data
    memcpy(item->payload, this->payload, SECTION_MAX_SIZE);
  }

  return result;
}

unsigned int CSection::GetSectionCalculatedSize(void)
{
  unsigned int result = SECTION_HEADER_SIZE;
  result += this->IsSetFlags(SECTION_FLAG_CHECK_CRC32) ? SECTION_CRC32_SIZE : 0;

  return result;
}

unsigned int CSection::GetSectionInternal(void)
{
  unsigned int position = 0;

  // write header, optional CRC32 can be added after last section data

  unsigned int sectionSize = this->GetSectionSize() - SECTION_HEADER_SIZE;

  unsigned int header = ((this->GetTableId() & SECTION_HEADER_TABLE_ID_MASK) << SECTION_HEADER_TABLE_ID_SHIFT);
  header |= this->IsSectionSyntaxIndicator() ? (1 << SECTION_HEADER_SECTION_SYNTAX_INDICATOR_SHIFT) : 0;
  header |= this->IsPrivateIndicator() ? (1 << SECTION_HEADER_PRIVATE_INDICATOR_SHIFT) : 0;
  header |= ((this->GetReserved() & SECTION_HEADER_RESERVED_MASK) << SECTION_HEADER_RESERVED_SHIFT);
  header |= ((sectionSize & SECTION_HEADER_SECTION_LENGTH_MASK) << SECTION_HEADER_SECTION_LENGTH_SHIFT);

  WBE24INC(this->payload, position, header);

  return position;
}

unsigned int CSection::CalculateSectionCrc32(void)
{
  unsigned int position = this->GetSectionSize() - SECTION_CRC32_SIZE;
  uint32_t sectionCalculatedCrc32 = CCrc32::Calculate(this->payload, position);

  WBE32INC(this->payload, position, sectionCalculatedCrc32);

  return position;
}