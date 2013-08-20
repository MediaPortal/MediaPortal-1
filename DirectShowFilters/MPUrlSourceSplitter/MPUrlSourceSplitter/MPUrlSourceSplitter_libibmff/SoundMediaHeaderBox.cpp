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

#include "SoundMediaHeaderBox.h"
#include "BoxCollection.h"

CSoundMediaHeaderBox::CSoundMediaHeaderBox(void)
  : CFullBox()
{
  this->type = Duplicate(SOUND_MEDIA_HEADER_BOX_TYPE);
  this->balance = new CFixedPointNumber(8, 8);
}

CSoundMediaHeaderBox::~CSoundMediaHeaderBox(void)
{
  FREE_MEM_CLASS(this->balance);
}

/* get methods */

bool CSoundMediaHeaderBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

CFixedPointNumber *CSoundMediaHeaderBox::GetBalance(void)
{
  return this->balance;
}

/* set methods */

/* other methods */

bool CSoundMediaHeaderBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CSoundMediaHeaderBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sBalance: %u.%u"
      ,
      
      previousResult,
      indent, this->GetBalance()->GetIntegerPart(), this->GetBalance()->GetFractionPart()

      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CSoundMediaHeaderBox::GetBoxSize(void)
{
  uint64_t result = 4;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CSoundMediaHeaderBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM_CLASS(this->balance);
  this->balance = new CFixedPointNumber(8, 8);
  bool result (this->balance != NULL);

  result &= __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, SOUND_MEDIA_HEADER_BOX_TYPE) != 0)
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
        continueParsing &= this->balance->SetNumber(RBE16(buffer, position));
        position += 2;

        // skip 2 reserved bytes
        position += 2;
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

uint32_t CSoundMediaHeaderBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE16INC(buffer, result, this->GetBalance()->GetNumber());

    // skip 2 reserved bytes
    result += 2;

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}