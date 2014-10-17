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

#include "MediaDataBox.h"
#include "BoxCollection.h"

CMediaDataBox::CMediaDataBox(HRESULT *result)
  : CBox(result)
{
  this->payload = NULL;
  this->payloadSize = 0;
  this->type = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MEDIA_DATA_BOX_TYPE);
    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CMediaDataBox::~CMediaDataBox(void)
{
  FREE_MEM(this->payload);
}

/* get methods */

const uint8_t *CMediaDataBox::GetPayload(void)
{
  return this->payload;
}

uint64_t CMediaDataBox::GetPayloadSize(void)
{
  return this->payloadSize;
}

bool CMediaDataBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

bool CMediaDataBox::SetPayload(const uint8_t *buffer, uint32_t length)
{
  bool result = (buffer != NULL) || (length == 0);
  FREE_MEM(this->payload);
  this->payloadSize = 0;

  if (result)
  {
    if (length > 0)
    {
      this->payload = ALLOC_MEM_SET(this->payload, uint8_t, length, 0);
      result &= (this->payload != NULL);

      if (result)
      {
        memcpy(this->payload, buffer, length);
        this->payloadSize = length;
      }
    }
  }

  return result;
}

/* other methods */

wchar_t *CMediaDataBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sPayload size: %llu",
      
      previousResult,
      indent, this->payloadSize
      
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CMediaDataBox::GetBoxSize(void)
{
  uint64_t result = this->GetPayloadSize();

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CMediaDataBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->payload);
  this->payloadSize = 0;

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MEDIA_DATA_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is media data box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;

      this->payloadSize = this->GetSize() - position;
      HRESULT continueParsing = ((position + this->payloadSize) <= length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        this->payload = ALLOC_MEM_SET(this->payload, uint8_t, (uint32_t)this->payloadSize, 0);
        CHECK_POINTER_HRESULT(continueParsing, this->payload, continueParsing, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), memcpy(this->payload, buffer + position, (uint32_t)this->payloadSize));
        position += (uint32_t)this->payloadSize;
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

uint32_t CMediaDataBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    if (this->GetPayloadSize() > 0)
    {
      memcpy(buffer + result, this->GetPayload(), (uint32_t)this->GetPayloadSize());
      result += (uint32_t)this->GetPayloadSize();
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}