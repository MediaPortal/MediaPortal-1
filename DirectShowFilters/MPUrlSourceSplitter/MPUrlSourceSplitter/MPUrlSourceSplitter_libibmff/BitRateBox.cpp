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

#include "BitRateBox.h"
#include "BoxCollection.h"

CBitrateBox::CBitrateBox(void)
  : CBox()
{
  this->type = Duplicate(BITRATE_BOX_TYPE);
  this->bufferSize = 0;
  this->maximumBitrate = 0;
  this->averageBitrate = 0;
}

CBitrateBox::~CBitrateBox(void)
{
}

/* get methods */

bool CBitrateBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint32_t CBitrateBox::GetBufferSize(void)
{
  return this->bufferSize;
}

uint32_t CBitrateBox::GetMaximumBitrate(void)
{
  return this->maximumBitrate;
}

uint32_t CBitrateBox::GetAverageBitrate(void)
{
  return this->averageBitrate;
}

/* set methods */

void CBitrateBox::SetBufferSize(uint32_t bufferSize)
{
  this->bufferSize = bufferSize;
}

void CBitrateBox::SetMaximumBitrate(uint32_t maximumBitrate)
{
  this->maximumBitrate = maximumBitrate;
}

void CBitrateBox::SetAverageBitrate(uint32_t averageBitrate)
{
  this->averageBitrate = averageBitrate;
}

/* other methods */

bool CBitrateBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CBitrateBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sBuffer size: %u\n" \
      L"%sMaximum bitrate: %u\n" \
      L"%sAverage bitrate: %u"
      ,
      
      previousResult,
      indent, this->GetBufferSize(),
      indent, this->GetMaximumBitrate(),
      indent, this->GetAverageBitrate()
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CBitrateBox::GetBoxSize(void)
{
  uint64_t result = 12;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CBitrateBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->bufferSize = 0;
  this->maximumBitrate = 0;
  this->averageBitrate = 0;

  bool result = __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, BITRATE_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);

      if (continueParsing)
      {
        RBE32INC(buffer, position, this->bufferSize);
        RBE32INC(buffer, position, this->maximumBitrate);
        RBE32INC(buffer, position, this->averageBitrate);
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

uint32_t CBitrateBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->GetBufferSize());
    WBE32INC(buffer, result, this->GetMaximumBitrate());
    WBE32INC(buffer, result, this->GetAverageBitrate());

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}