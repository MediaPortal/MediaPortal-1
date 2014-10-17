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

#include "DumpBox.h"
#include "BoxConstants.h"
#include "BoxCollection.h"
#include "BufferHelper.h"

CDumpBox::CDumpBox(HRESULT *result)
  : CBox(result)
{
  this->payload = NULL;
  this->payloadSize = 0;
  this->type = NULL;
  memset(&this->time, 0, sizeof(SYSTEMTIME));

  /*if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }*/
}

CDumpBox::~CDumpBox(void)
{
  FREE_MEM(this->payload);
}

/* get methods */

const uint8_t *CDumpBox::GetPayload(void)
{
  return this->payload;
}

uint64_t CDumpBox::GetPayloadSize(void)
{
  return this->payloadSize;
}

bool CDumpBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

SYSTEMTIME CDumpBox::GetTime(void)
{
  return this->time;
}

/* set methods */

bool CDumpBox::SetPayload(const uint8_t *buffer, uint32_t length)
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

void CDumpBox::SetTime(SYSTEMTIME time)
{
  this->time = time;
}

void CDumpBox::SetTimeWithLocalTime(void)
{
  GetLocalTime(&this->time);
}

/* other methods */

wchar_t *CDumpBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sFlags: 0x%08X\n" \
      L"%sDay: %hu\n" \
      L"%sMonth: %hu\n" \
      L"%sYear: %hu\n" \
      L"%sHour: %hu\n" \
      L"%sMinute: %hu\n" \
      L"%sSecond: %hu\n" \
      L"%sMilliseconds: %hu\n" \
      L"%sDayOfWeek: %hu\n" \
      L"%sPayload size: %llu",
      
      previousResult,
      indent, this->flags,
      indent, this->time.wDay,
      indent, this->time.wMonth,
      indent, this->time.wYear,
      indent, this->time.wHour,
      indent, this->time.wMinute,
      indent, this->time.wSecond,
      indent, this->time.wMilliseconds,
      indent, this->time.wDayOfWeek,
      indent, this->payloadSize
      );
  }

  FREE_MEM(previousResult);

  return result;
}

/* protected methods */

uint64_t CDumpBox::GetBoxSize(void)
{
  uint64_t result = 8 + sizeof(SYSTEMTIME) + this->payloadSize;

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CDumpBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  return this->ParseInternal(buffer, length, processAdditionalBoxes, true);;
}

uint32_t CDumpBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->flags);

    memcpy(buffer + result, &this->time, sizeof(SYSTEMTIME));
    result += sizeof(SYSTEMTIME);

    WBE32INC(buffer, result, this->payloadSize);

    if (this->payloadSize > 0)
    {
      memcpy(buffer + result, this->payload, this->payloadSize);
      result += this->payloadSize;
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}

bool CDumpBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes, bool checkType)
{
  FREE_MEM(this->payload);
  this->payloadSize = 0;

  if (__super::ParseInternal(buffer, length, false))
  {
    // don't know box type, don't check box type
   
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is dump box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->flags);

        memcpy(&this->time, buffer + position, sizeof(SYSTEMTIME));
        position += sizeof(SYSTEMTIME);

        RBE32INC(buffer, position, this->payloadSize);

        this->payload = ALLOC_MEM_SET(this->payload, uint8_t, this->payloadSize, 0);
        CHECK_POINTER_HRESULT(continueParsing, this->payload, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing))
        {
          memcpy(this->payload, buffer + position, this->payloadSize);
          position += this->payloadSize;
        }
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