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

#include "VisualSampleEntryBox.h"
#include "BoxCollection.h"

CVisualSampleEntryBox::CVisualSampleEntryBox(HRESULT *result)
  : CSampleEntryBox(result)
{
  this->width = 0;
  this->height = 0;
  this->horizontalResolution = NULL;
  this->verticalResolution = NULL;
  this->frameCount = 0;
  this->compressorName = NULL;
  this->depth = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->horizontalResolution = new CFixedPointNumber(result, 16, 16);
    this->verticalResolution = new CFixedPointNumber(result, 16, 16);

    CHECK_POINTER_HRESULT(*result, this->horizontalResolution, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->verticalResolution, *result, E_OUTOFMEMORY);
  }
}

CVisualSampleEntryBox::~CVisualSampleEntryBox(void)
{
  FREE_MEM_CLASS(this->horizontalResolution);
  FREE_MEM_CLASS(this->verticalResolution);
  FREE_MEM(this->compressorName);
}

/* get methods */

const wchar_t *CVisualSampleEntryBox::GetCodingName(void)
{
  return this->GetType();
}

uint16_t CVisualSampleEntryBox::GetWidth(void)
{
  return this->width;
}

uint16_t CVisualSampleEntryBox::GetHeight(void)
{
  return this->height;
}

CFixedPointNumber *CVisualSampleEntryBox::GetHorizontalResolution(void)
{
  return this->horizontalResolution;
}

CFixedPointNumber *CVisualSampleEntryBox::GetVerticalResolution(void)
{
  return this->verticalResolution;
}

uint16_t CVisualSampleEntryBox::GetFrameCount(void)
{
  return this->frameCount;
}

const wchar_t *CVisualSampleEntryBox::GetCompressorName(void)
{
  return this->compressorName;
}

uint16_t CVisualSampleEntryBox::GetDepth(void)
{
  return this->depth;
}

/* set methods */

bool CVisualSampleEntryBox::SetCodingName(const wchar_t *codingName)
{
  SET_STRING_RETURN(this->type, codingName);
}

void CVisualSampleEntryBox::SetWidth(uint16_t width)
{
  this->width = width;
}

void CVisualSampleEntryBox::SetHeight(uint16_t height)
{
  this->height = height;
}

void CVisualSampleEntryBox::SetFrameCount(uint16_t frameCount)
{
  this->frameCount = frameCount;
}

bool CVisualSampleEntryBox::SetCompressorName(const wchar_t *compressorName)
{
  SET_STRING_RETURN(this->compressorName, compressorName);
}

void CVisualSampleEntryBox::SetDepth(uint16_t depth)
{
  this->depth = depth;
}

/* other methods */

wchar_t *CVisualSampleEntryBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sCoding name: '%s'\n" \
      L"%sWidth: %u\n" \
      L"%sHeight: %u\n" \
      L"%sHorizontal resolution: %u.%u\n" \
      L"%sVertical resolution: %u.%u\n" \
      L"%sFrame count: %u\n" \
      L"%sCompressor: '%s'\n" \
      L"%sDepth: %u"
      ,
      
      previousResult,
      indent, this->GetCodingName(),
      indent, this->GetWidth(),
      indent, this->GetHeight(),
      indent, this->GetHorizontalResolution()->GetIntegerPart(), this->GetHorizontalResolution()->GetFractionPart(),
      indent, this->GetVerticalResolution()->GetIntegerPart(), this->GetVerticalResolution()->GetFractionPart(),
      indent, this->GetFrameCount(),
      indent, this->GetCompressorName(),
      indent, this->GetDepth()
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CVisualSampleEntryBox::GetBoxSize(void)
{
  uint64_t result = 70;

  char *utf8String = ConvertUnicodeToUtf8(this->GetCompressorName());
  result = (utf8String != NULL) ? result : 0;

  if (result != 0)
  {
    unsigned int compressorNameLength = strlen(utf8String);
    // compressor name length can be at max 31
    result = (compressorNameLength < 32) ? result : 0;
  }
  FREE_MEM(utf8String);

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0;
  }

  return result;
}

bool CVisualSampleEntryBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM_CLASS(this->horizontalResolution);
  FREE_MEM_CLASS(this->verticalResolution);
  FREE_MEM(this->compressorName);

  if (__super::ParseInternal(buffer, length, false))
  {
    uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
    HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

    if (SUCCEEDED(continueParsing))
    {
      this->horizontalResolution = new CFixedPointNumber(&continueParsing, 16, 16);
      this->verticalResolution = new CFixedPointNumber(&continueParsing, 16, 16);

      CHECK_POINTER_HRESULT(continueParsing, this->horizontalResolution, continueParsing, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(continueParsing, this->verticalResolution, continueParsing, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(continueParsing))
    {
      // skip 16 reserved and pre-defined bytes
      position += 16;

      RBE16INC(buffer, position, this->width);
      RBE16INC(buffer, position, this->height);

      CHECK_CONDITION_HRESULT(continueParsing, this->horizontalResolution->SetNumber(RBE32(buffer, position)), continueParsing, E_OUTOFMEMORY);
      position += 4;

      CHECK_CONDITION_HRESULT(continueParsing, this->verticalResolution->SetNumber(RBE32(buffer, position)), continueParsing, E_OUTOFMEMORY);
      position += 4;

      // skip 4 reserved bytes
      position += 4;

      RBE16INC(buffer, position, this->frameCount);

      RBE8INC_DEFINE(buffer, position, compressorNameLength, uint8_t);

      uint32_t positionAfterString = 0;
      continueParsing = this->GetString(buffer, length, position, &this->compressorName, &positionAfterString);
      position += 31;

      if (SUCCEEDED(continueParsing))
      {
        RBE16INC(buffer, position, this->depth);

        // skip 2 pre-defined bytes
        position += 2;

        // optional clean aperture box

        // optional pixel aspect ratio box
      }
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

uint32_t CVisualSampleEntryBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    // skip 16 reserved and pre-defined bytes
    result += 16;

    WBE16INC(buffer, result, this->GetWidth());
    WBE16INC(buffer, result, this->GetHeight());

    WBE32INC(buffer, result, this->GetHorizontalResolution()->GetNumber());
    WBE32INC(buffer, result, this->GetVerticalResolution()->GetNumber());

    // skip 4 reserved bytes
    result += 4;

    WBE16INC(buffer, result, this->GetFrameCount());

    if (this->GetCompressorName() != NULL)
    {
      char *utf8String = ConvertUnicodeToUtf8(this->GetCompressorName());
      result = (utf8String != NULL) ? result : 0;

      if (result != 0)
      {
        unsigned int compressorNameLength = strlen(utf8String);
        // compressor name length can be at max 31
        result = (compressorNameLength < 32) ? result : 0;

        if (result != 0)
        {
          WBE8INC(buffer, result, compressorNameLength);

          for (unsigned int i = 0; i < compressorNameLength; i++)
          {
            WBE8INC(buffer, result, utf8String[i]);
          }

          result += (31 - compressorNameLength);
        }
      }
      FREE_MEM(utf8String);
    }
    else
    {
      result += 32;
    }

    if (result != 0)
    {
      WBE16INC(buffer, result, this->GetDepth());

      // skip 2 pre-defined bytes (0xFFFF)
      WBE16INC(buffer, result, 0xFFFF);

      // optional clean aperture box

      // optional pixel aspect ratio box
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}