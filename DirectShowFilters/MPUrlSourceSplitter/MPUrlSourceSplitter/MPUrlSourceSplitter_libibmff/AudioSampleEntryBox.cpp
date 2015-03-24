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

#include "AudioSampleEntryBox.h"
#include "BoxCollection.h"

CAudioSampleEntryBox::CAudioSampleEntryBox(HRESULT *result)
  : CSampleEntryBox(result)
{
  this->channelCount = 0;
  this->sampleSize = 0;
  this->sampleRate = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->sampleRate = new CFixedPointNumber(result, 16, 16);

    CHECK_POINTER_HRESULT(*result, this->sampleRate, *result, E_OUTOFMEMORY);
  }
}

CAudioSampleEntryBox::~CAudioSampleEntryBox(void)
{
  FREE_MEM_CLASS(this->sampleRate);
}

/* get methods */

const wchar_t *CAudioSampleEntryBox::GetCodingName(void)
{
  return this->GetType();
}

uint16_t CAudioSampleEntryBox::GetChannelCount(void)
{
  return this->channelCount;
}

uint16_t CAudioSampleEntryBox::GetSampleSize(void)
{
  return this->sampleSize;
}

CFixedPointNumber *CAudioSampleEntryBox::GetSampleRate(void)
{
  return this->sampleRate;
}

/* set methods */

bool CAudioSampleEntryBox::SetCodingName(const wchar_t *codingName)
{
  if ((codingName != NULL) && (wcslen(codingName) == 4))
  {
    SET_STRING_RETURN(this->type, codingName);
  }

  return false;
}

void CAudioSampleEntryBox::SetChannelCount(uint16_t channelCount)
{
  this->channelCount = channelCount;
}

void CAudioSampleEntryBox::SetSampleSize(uint16_t sampleSize)
{
  this->sampleSize = sampleSize;
}

/* other methods */

wchar_t *CAudioSampleEntryBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sCoding name: '%s'\n" \
      L"%sChannel count: %u\n" \
      L"%sSample size: %u\n" \
      L"%sSample rate: %u.%u"
      ,
      
      previousResult,
      indent, this->GetCodingName(),
      indent, this->GetChannelCount(),
      indent, this->GetSampleSize(),
      indent, this->GetSampleRate()->GetIntegerPart(), this->GetSampleRate()->GetFractionPart()
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CAudioSampleEntryBox::GetBoxSize(void)
{
  uint64_t result = 20;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CAudioSampleEntryBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
    HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

    if (SUCCEEDED(continueParsing))
    {
      // skip 2 x uint(32) reserved
      position += 8;

      RBE16INC(buffer, position, this->channelCount);
      RBE16INC(buffer, position, this->sampleSize);

      // skip 1 x uint(16) pre-defined and 1 x uint(16) reserved
      position += 4;

      CHECK_CONDITION_HRESULT(continueParsing, this->sampleRate->SetNumber(RBE32(buffer, position)), continueParsing, E_OUTOFMEMORY);
      position += 4;
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

uint32_t CAudioSampleEntryBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    // skip 2 x uint(32) reserved
    result += 8;

    WBE16INC(buffer, result, this->GetChannelCount());
    WBE16INC(buffer, result, this->GetSampleSize());

    // skip 1 x uint(16) pre-defined and 1 x uint(16) reserved
    result += 4;

    WBE32INC(buffer, result, this->GetSampleRate()->GetNumber());

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}