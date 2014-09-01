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

#include "HintMediaHeaderBox.h"
#include "BoxCollection.h"

CHintMediaHeaderBox::CHintMediaHeaderBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->averageBitrate = 0;
  this->averagePDUSize = 0;
  this->maxBitrate = 0;
  this->maxPDUSize = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(HINT_MEDIA_HEADER_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CHintMediaHeaderBox::~CHintMediaHeaderBox(void)
{
}

/* get methods */

bool CHintMediaHeaderBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint16_t CHintMediaHeaderBox::GetMaxPDUSize(void)
{
  return this->maxPDUSize;
}

uint16_t CHintMediaHeaderBox::GetAveragePDUSize(void)
{
  return this->averagePDUSize;
}

uint32_t CHintMediaHeaderBox::GetMaxBitrate(void)
{
  return this->maxBitrate;
}

uint32_t CHintMediaHeaderBox::GetAverageBitrate(void)
{
  return this->averageBitrate;
}

/* set methods */

void CHintMediaHeaderBox::SetMaxPDUSize(uint16_t maxPDUSize)
{
  this->maxPDUSize = maxPDUSize;
}

void CHintMediaHeaderBox::SetAveragePDUSize(uint16_t averagePDUSize)
{
  this->averagePDUSize = averagePDUSize;
}

void CHintMediaHeaderBox::SetMaxBitrate(uint32_t maxBitrate)
{
  this->maxBitrate = maxBitrate;
}

void CHintMediaHeaderBox::SetAverageBitrate(uint32_t averageBitrate)
{
  this->averageBitrate = averageBitrate;
}

/* other methods */

bool CHintMediaHeaderBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CHintMediaHeaderBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sMax PDU size: %u\n" \
      L"%sAverage PDU size: %u\n" \
      L"%sMax bitrate: %u\n" \
      L"%sAverage bitrate: %u"
      ,
      
      previousResult,
      indent, this->GetMaxPDUSize(),
      indent, this->GetAveragePDUSize(),
      indent, this->GetMaxBitrate(),
      indent, this->GetAverageBitrate()

      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CHintMediaHeaderBox::GetBoxSize(void)
{
  uint64_t result = 16;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CHintMediaHeaderBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, HINT_MEDIA_HEADER_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is media data box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE16INC(buffer, position, this->maxPDUSize);
        RBE16INC(buffer, position, this->averagePDUSize);
        RBE32INC(buffer, position, this->maxBitrate);
        RBE32INC(buffer, position, this->averageBitrate);

        // skip 4 reserved bytes
        position += 4;
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

uint32_t CHintMediaHeaderBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE16INC(buffer, result, this->GetMaxPDUSize());
    WBE16INC(buffer, result, this->GetAveragePDUSize());
    WBE32INC(buffer, result, this->GetMaxBitrate());
    WBE32INC(buffer, result, this->GetAverageBitrate());

    // skip 4 reserved bytes
    result += 4;

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}