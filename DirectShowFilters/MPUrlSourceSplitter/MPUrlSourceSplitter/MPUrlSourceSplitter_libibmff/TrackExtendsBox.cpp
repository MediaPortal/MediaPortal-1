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

#include "TrackExtendsBox.h"
#include "BoxCollection.h"

CTrackExtendsBox::CTrackExtendsBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->trackId = 0;
  this->defaultSampleDescriptionIndex = 0;
  this->defaultSampleDuration = 0;
  this->defaultSampleFlags = 0;
  this->defaultSampleSize = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(TRACK_EXTENDS_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CTrackExtendsBox::~CTrackExtendsBox(void)
{
}

/* get methods */

bool CTrackExtendsBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint32_t CTrackExtendsBox::GetTrackId(void)
{
  return this->trackId;
}

uint32_t CTrackExtendsBox::GetDefaultSampleDescriptionIndex(void)
{
  return this->defaultSampleDescriptionIndex;
}

uint32_t CTrackExtendsBox::GetDefaultSampleDuration(void)
{
  return this->defaultSampleDuration;
}

uint32_t CTrackExtendsBox::GetDefaultSampleSize(void)
{
  return this->defaultSampleSize;
}

uint32_t CTrackExtendsBox::GetDefaultSampleFlags(void)
{
  return this->defaultSampleFlags;
}

/* set methods */

void CTrackExtendsBox::SetTrackId(uint32_t trackId)
{
  this->trackId = trackId;
}

void CTrackExtendsBox::SetDefaultSampleDescriptionIndex(uint32_t defaultSampleDescriptionIndex)
{
  this->defaultSampleDescriptionIndex = defaultSampleDescriptionIndex;
}

void CTrackExtendsBox::SetDefaultSampleDuration(uint32_t defaultSampleDuration)
{
  this->defaultSampleDuration = defaultSampleDuration;
}

void CTrackExtendsBox::SetDefaultSampleSize(uint32_t defaultSampleSize)
{
  this->defaultSampleSize = defaultSampleSize;
}

void CTrackExtendsBox::SetDefaultSampleFlags(uint32_t defaultSampleFlags)
{
  this->defaultSampleFlags = defaultSampleFlags;
}

/* other methods */

wchar_t *CTrackExtendsBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sTrack ID: %u\n" \
      L"%sDefault sample description index: %u\n" \
      L"%sDefault sample duration: %u\n" \
      L"%sDefault sample size: %u\n" \
      L"%sDefault sample flags: 0x%08X"
      ,
      
      previousResult,
      indent, this->GetTrackId(),
      indent, this->GetDefaultSampleDescriptionIndex(),
      indent, this->GetDefaultSampleDuration(),
      indent, this->GetDefaultSampleSize(),
      indent, this->GetDefaultSampleFlags()
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CTrackExtendsBox::GetBoxSize(void)
{
  uint64_t result = 20;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CTrackExtendsBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, TRACK_EXTENDS_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is track extends box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->trackId);
        RBE32INC(buffer, position, this->defaultSampleDescriptionIndex);
        RBE32INC(buffer, position, this->defaultSampleDuration);
        RBE32INC(buffer, position, this->defaultSampleSize);
        RBE32INC(buffer, position, this->defaultSampleFlags);
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

uint32_t CTrackExtendsBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetTrackId());
    WBE32INC(buffer, result, this->GetDefaultSampleDescriptionIndex());
    WBE32INC(buffer, result, this->GetDefaultSampleDuration());
    WBE32INC(buffer, result, this->GetDefaultSampleSize());
    WBE32INC(buffer, result, this->GetDefaultSampleFlags());

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}