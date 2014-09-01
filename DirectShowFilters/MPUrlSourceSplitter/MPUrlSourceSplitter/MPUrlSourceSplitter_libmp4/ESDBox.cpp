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

#include "ESDBox.h"
#include "BoxCollection.h"

CESDBox::CESDBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->trackId = 0;
  this->codecTag = 0;
  this->bufferSize = 0;
  this->maxBitrate = 0;
  this->averageBitrate = 0;
  this->codecPrivateData = NULL;
  this->codecPrivateDataLength = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(ESD_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CESDBox::~CESDBox(void)
{
  FREE_MEM(this->codecPrivateData);
}

/* get methods */

bool CESDBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint16_t CESDBox::GetTrackId(void)
{
  return this->trackId;
}

uint8_t CESDBox::GetCodecTag(void)
{
  return this->codecTag;
}

uint32_t CESDBox::GetBufferSize(void)
{
  return this->bufferSize;
}

uint32_t CESDBox::GetMaxBitrate(void)
{
  return this->maxBitrate;
}

uint32_t CESDBox::GetAverageBitrate(void)
{
  return this->averageBitrate;
}

const uint8_t *CESDBox::GetCodecPrivateData(void)
{
  return this->codecPrivateData;
}

uint32_t CESDBox::GetCodecPrivateDataLength(void)
{
  return this->codecPrivateDataLength;
}

/* set methods */

void CESDBox::SetTrackId(uint16_t trackId)
{
  this->trackId = trackId;
}

void CESDBox::SetCodecTag(uint8_t codecTag)
{
  this->codecTag = codecTag;
}

void CESDBox::SetBufferSize(uint32_t bufferSize)
{
  this->bufferSize = bufferSize;
}

void CESDBox::SetMaxBitrate(uint32_t maxBitrate)
{
  this->maxBitrate = maxBitrate;
}

void CESDBox::SetAverageBitrate(uint32_t averageBitrate)
{
  this->averageBitrate = averageBitrate;
}

bool CESDBox::SetCodecPrivateData(const uint8_t *data, uint32_t length)
{
  FREE_MEM(this->codecPrivateData);
  this->codecPrivateDataLength = 0;

  if ((data != NULL) && (length > 0))
  {
    this->codecPrivateData = ALLOC_MEM_SET(this->codecPrivateData, uint8_t, length, 0);
    if (this->codecPrivateData != NULL)
    {
      memcpy(this->codecPrivateData, data, length);
      this->codecPrivateDataLength = length;
    }
  }

  return ((this->codecPrivateData != NULL) || ((this->codecPrivateData == NULL) && (data == NULL)));
}

/* other methods */

bool CESDBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CESDBox::GetParsedHumanReadable(const wchar_t *indent)
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

uint64_t CESDBox::GetBoxSize(void)
{
  uint64_t result = 32;

  if (this->GetCodecPrivateDataLength() > 0)
  {
    result += this->GetCodecPrivateDataLength();
    result += 5;
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CESDBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  //if (__super::ParseInternal(buffer, length, false))
  //{
  //  this->flags &= ~BOX_FLAG_PARSED;
  //  this->flags |= (wcscmp(this->type, ESD_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

  //  if (this->IsSetFlags(BOX_FLAG_PARSED))
  //  {
  //    // box is media data box, parse all values
  //    uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
  //    HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

  //    if (SUCCEEDED(continueParsing) && processAdditionalBoxes)
  //    {
  //      this->ProcessAdditionalBoxes(buffer, length, position);
  //    }

  //    this->flags &= ~BOX_FLAG_PARSED;
  //    this->flags |= SUCCEEDED(continueParsing) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
  //  }
  //}

  //return this->IsSetFlags(BOX_FLAG_PARSED);

  return false;
}

uint32_t CESDBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    uint32_t decoderSpecificInfoLength = (this->GetCodecPrivateDataLength() != 0) ? (this->GetCodecPrivateDataLength() + 5) : 0;

    this->PutDescriptor(buffer + result, 0x03, 3 + 5 + 13 + decoderSpecificInfoLength + 5 + 1);
    result += 5;

    WBE16INC(buffer, result, this->GetTrackId());
    WBE8INC(buffer, result, 0);

    this->PutDescriptor(buffer + result, 0x04, 13 + decoderSpecificInfoLength);
    result += 5;

    WBE8INC(buffer, result, this->GetCodecTag());
    WBE8INC(buffer, result, 0x15);
    WBE8INC(buffer, result, (this->GetBufferSize() >> (3 + 16)));
    WBE16INC(buffer, result, ((this->GetBufferSize() >> 3) & 0x0000FFFF));
    WBE32INC(buffer, result, this->GetMaxBitrate());
    WBE32INC(buffer, result, this->GetAverageBitrate());

    if (this->GetCodecPrivateDataLength() > 0)
    {
      this->PutDescriptor(buffer + result, 0x05, this->GetCodecPrivateDataLength());
      result += 5;
      memcpy(buffer + result, this->GetCodecPrivateData(), this->GetCodecPrivateDataLength());
      result += this->GetCodecPrivateDataLength();
    }

    this->PutDescriptor(buffer + result, 0x06, 1);
    WBE8INC(buffer, result, 0x02);

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}

void CESDBox::PutDescriptor(uint8_t *buffer, uint8_t tag, uint32_t size)
{
  uint32_t position = 0;

  WBE8INC(buffer, position, tag);
  for (uint8_t i = 3; i > 0; i--)
  {
    WBE8INC(buffer, position, ((size >> (7*i)) | 0x80));
  }
  WBE8INC(buffer, position, (size & 0x0000007F));
}

