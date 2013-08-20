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

CFragmentedIndexTrackBox::CFragmentedIndexTrackBox(void)
  : CFullBox()
{
  this->type = Duplicate(FRAGMENTED_INDEX_TRACK_BOX_TYPE);
  this->trackId = 0;
  this->fragmentedIndexes = new CFragmentedIndexCollection();
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
  FREE_MEM_CLASS(this->fragmentedIndexes);
  this->fragmentedIndexes = new CFragmentedIndexCollection();

  // in bad case we don't have objects, but still it can be valid box
  bool result = ((this->fragmentedIndexes != NULL) && __super::ParseInternal(buffer, length, false));

  if (result)
  {
    if (wcscmp(this->type, FRAGMENTED_INDEX_TRACK_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);

      if (continueParsing)
      {
        RBE32INC(buffer, position, this->trackId);
        RBE32INC_DEFINE(buffer, position, fragmentIndexCount, uint32_t);

        for (uint32_t i = 0; (continueParsing && (i < fragmentIndexCount)); i++)
        {
          CFragmentedIndex *index = new CFragmentedIndex();

          if (continueParsing)
          {
            index->SetTimestamp(RBE64(buffer, position));
            position += 8;
            index->SetDuration(RBE64(buffer, position));
            position += 8;

            continueParsing &= this->fragmentedIndexes->Add(index);
          }

          if (!continueParsing)
          {
            FREE_MEM_CLASS(index);
          }
        }
      }

      if (continueParsing && processAdditionalBoxes)
      {
        this->ProcessAdditionalBoxes(buffer, length, position);
      }

      this->parsed = continueParsing;
    }
  }

  result = this->parsed;

  return result;
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