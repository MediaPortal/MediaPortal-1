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

#include "FragmentedIndexTrackBox.h"
#include "BoxCollection.h"

CFragmentedIndexTrackBox::CFragmentedIndexTrackBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->trackId = 0;
  this->fragmentedIndexes = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(FRAGMENTED_INDEX_TRACK_BOX_TYPE);
    this->fragmentedIndexes = new CFragmentedIndexCollection(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->fragmentedIndexes, *result, E_OUTOFMEMORY);
  }
}

CFragmentedIndexTrackBox::~CFragmentedIndexTrackBox(void)
{
  FREE_MEM_CLASS(this->fragmentedIndexes);
}

/* get methods */

bool CFragmentedIndexTrackBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint32_t CFragmentedIndexTrackBox::GetTrackId(void)
{
  return this->trackId;
}

CFragmentedIndexCollection *CFragmentedIndexTrackBox::GetFragmentedIndexes(void)
{
  return this->fragmentedIndexes;
}

/* set methods */

void CFragmentedIndexTrackBox::SetTrackId(uint32_t trackId)
{
  this->trackId = trackId;
}

/* other methods */

bool CFragmentedIndexTrackBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CFragmentedIndexTrackBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s"
      ,
      
      previousResult
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CFragmentedIndexTrackBox::GetBoxSize(void)
{
  uint64_t result = (this->fragmentedIndexes != NULL) ? (8 + this->fragmentedIndexes->Count() * 16) : 0;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CFragmentedIndexTrackBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->fragmentedIndexes->Clear();

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, FRAGMENTED_INDEX_TRACK_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is media data box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->trackId);
        RBE32INC_DEFINE(buffer, position, fragmentIndexCount, uint32_t);

        CHECK_CONDITION_HRESULT(continueParsing, this->fragmentedIndexes->EnsureEnoughSpace(fragmentIndexCount), continueParsing, E_OUTOFMEMORY);

        for (uint32_t i = 0; (SUCCEEDED(continueParsing) && (i < fragmentIndexCount)); i++)
        {
          CFragmentedIndex *index = new CFragmentedIndex(&continueParsing);
          CHECK_POINTER_HRESULT(continueParsing, index, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            index->SetTimestamp(RBE64(buffer, position));
            position += 8;
            index->SetDuration(RBE64(buffer, position));
            position += 8;
          }

          CHECK_CONDITION_HRESULT(continueParsing, this->fragmentedIndexes->Add(index), continueParsing, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(index));
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

uint32_t CFragmentedIndexTrackBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetTrackId());
    WBE32INC(buffer, result, this->GetFragmentedIndexes()->Count());

    for (unsigned int i = 0; i < this->GetFragmentedIndexes()->Count(); i++)
    {
      CFragmentedIndex *index = this->GetFragmentedIndexes()->GetItem(i);

      WBE64INC(buffer, result, index->GetTimestamp());
      WBE64INC(buffer, result, index->GetDuration());
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}