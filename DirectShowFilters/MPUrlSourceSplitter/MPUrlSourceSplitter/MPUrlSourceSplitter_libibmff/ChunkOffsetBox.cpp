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

#include "ChunkOffsetBox.h"
#include "BoxCollection.h"

CChunkOffsetBox::CChunkOffsetBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->chunkOffsets = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(CHUNK_OFFSET_BOX_TYPE);
    this->chunkOffsets = new CChunkOffsetCollection(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->chunkOffsets, *result, E_OUTOFMEMORY);
  }
}

CChunkOffsetBox::~CChunkOffsetBox(void)
{
  FREE_MEM_CLASS(this->chunkOffsets);
}

/* get methods */

CChunkOffsetCollection *CChunkOffsetBox::GetChunkOffsets(void)
{
  return this->chunkOffsets;
}

/* set methods */

/* other methods */

wchar_t *CChunkOffsetBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare sample entries collection
    wchar_t *offsets = NULL;
    wchar_t *tempIndent = FormatString(L"%s\t", indent);
    for (unsigned int i = 0; i < this->GetChunkOffsets()->Count(); i++)
    {
      CChunkOffset *chunkOffset = this->GetChunkOffsets()->GetItem(i);
      wchar_t *tempOffsets = FormatString(
        L"%s%s%s%llu",
        (i == 0) ? L"" : offsets,
        (i == 0) ? L"" : L"\n",
        tempIndent,
        chunkOffset->GetChunkOffset());
      FREE_MEM(offsets);

      offsets = tempOffsets;
    }

    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sChunk offsets:%s" \
      L"%s"
      ,
      
      previousResult,
      indent, (this->GetChunkOffsets()->Count() == 0) ? L"" : L"\n",
      (this->GetChunkOffsets()->Count() == 0) ? L"" : offsets

      );

    FREE_MEM(offsets);
    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CChunkOffsetBox::GetBoxSize(void)
{
  uint64_t result = 4 + this->GetChunkOffsets()->Count() * 4;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CChunkOffsetBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->chunkOffsets->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, CHUNK_OFFSET_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is chunk offset box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC_DEFINE(buffer, position, chunkOffsetCount, uint32_t);

        CHECK_CONDITION_HRESULT(continueParsing, this->chunkOffsets->EnsureEnoughSpace(chunkOffsetCount), continueParsing, E_OUTOFMEMORY);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < chunkOffsetCount)); i++)
        {
          CChunkOffset *chunkOffset = new CChunkOffset(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, chunkOffset, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            chunkOffset->SetChunkOffset(RBE32(buffer, position));
            position += 4;
          }

          CHECK_CONDITION_HRESULT(continueParsing, this->chunkOffsets->Add(chunkOffset), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(chunkOffset));
        }
      }

      if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }

      this->flags &= ~BOX_FLAG_PARSED;
      this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    }
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t CChunkOffsetBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetChunkOffsets()->Count());

    for (uint32_t i = 0; (i < this->GetChunkOffsets()->Count()); i++)
    {
      WBE32INC(buffer, result, this->GetChunkOffsets()->GetItem(i)->GetChunkOffset());
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}