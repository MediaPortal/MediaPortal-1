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

#include "SampleEntryBox.h"
#include "BoxCollection.h"

CSampleEntryBox::CSampleEntryBox(HRESULT *result)
  : CBox(result)
{
  this->dataReferenceIndex = 0;
}

CSampleEntryBox::~CSampleEntryBox(void)
{
}

/* get methods */

bool CSampleEntryBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint16_t CSampleEntryBox::GetDataReferenceIndex(void)
{
  return this->dataReferenceIndex;
}

/* set methods */

void CSampleEntryBox::SetDataReferenceIndex(uint16_t dataReferenceIndex)
{
  this->dataReferenceIndex = dataReferenceIndex;
}

/* other methods */

bool CSampleEntryBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CSampleEntryBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sData reference index: %u"
      ,
      
      previousResult,
      indent, this->GetDataReferenceIndex()
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CSampleEntryBox::GetBoxSize(void)
{
  return (8 + __super::GetBoxSize());
}

bool CSampleEntryBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
    HRESULT continueParsing = ((position + SAMPLE_ENTRY_BOX_DATA_SIZE) <= min(length, (uint32_t)this->GetSize())) ? S_OK : E_NOT_VALID_STATE;

    if (SUCCEEDED(continueParsing))
    {
      // skip 6 x uint(8) reserved
      position += 6;

      RBE16INC(buffer, position, this->dataReferenceIndex);
    }

    if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
    {
      this->ProcessAdditionalBoxes(buffer, length, position);
    }

    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t CSampleEntryBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    // skip 6 x uint(8) reserved
    result += 6;

    WBE16INC(buffer, result, this->GetDataReferenceIndex());

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}