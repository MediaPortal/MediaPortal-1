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

CSoundMediaHeaderBox::CSoundMediaHeaderBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->balance = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(SOUND_MEDIA_HEADER_BOX_TYPE);
    this->balance = new CFixedPointNumber(result, 8, 8);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->balance, *result, E_OUTOFMEMORY);
  }
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

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, SOUND_MEDIA_HEADER_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is media data box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        this->balance = new CFixedPointNumber(&continueParsing, 8, 8);
        CHECK_POINTER_HRESULT(continueParsing, this->balance, continueParsing, E_OUTOFMEMORY);
      }

      if (SUCCEEDED(continueParsing))
      {
        CHECK_CONDITION_HRESULT(continueParsing, this->balance->SetNumber(RBE16(buffer, position)), continueParsing, E_OUTOFMEMORY);
        position += 2;

        // skip 2 reserved bytes
        position += 2;
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