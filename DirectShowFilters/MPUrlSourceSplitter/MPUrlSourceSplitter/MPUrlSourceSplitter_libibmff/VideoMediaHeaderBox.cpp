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

#include "VideoMediaHeaderBox.h"
#include "BoxCollection.h"

CVideoMediaHeaderBox::CVideoMediaHeaderBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->graphicsMode = 0;
  this->colorRed = 0;
  this->colorGreen = 0;
  this->colorBlue = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(VIDEO_MEDIA_HEADER_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CVideoMediaHeaderBox::~CVideoMediaHeaderBox(void)
{
}

/* get methods */

uint16_t CVideoMediaHeaderBox::GetGraphicsMode(void)
{
  return this->graphicsMode;
}

uint16_t CVideoMediaHeaderBox::GetColorRed(void)
{
  return this->colorRed;
}

uint16_t CVideoMediaHeaderBox::GetColorGreen(void)
{
  return this->colorGreen;
}

uint16_t CVideoMediaHeaderBox::GetColorBlue(void)
{
  return this->colorBlue;
}

/* set methods */

void CVideoMediaHeaderBox::SetGraphicsMode(uint16_t graphicsMods)
{
  this->graphicsMode = graphicsMode;
}

void CVideoMediaHeaderBox::SetColorRed(uint16_t red)
{
  this->colorRed = red;
}

 void CVideoMediaHeaderBox::SetColorGreen(uint16_t green)
 {
   this->colorGreen = green;
 }

void CVideoMediaHeaderBox::SetColorBlue(uint16_t blue)
{
  this->colorBlue = blue;
}

/* other methods */

wchar_t *CVideoMediaHeaderBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sGraphics mode: %u\n" \
      L"%sColor red: %u\n" \
      L"%sColor green: %u\n" \
      L"%sColor blue: %u"
      ,
      
      previousResult,
      indent, this->GetGraphicsMode(),
      indent, this->GetColorRed(),
      indent, this->GetColorGreen(),
      indent, this->GetColorBlue()

      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CVideoMediaHeaderBox::GetBoxSize(void)
{
  uint64_t result = 8;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CVideoMediaHeaderBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, VIDEO_MEDIA_HEADER_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is video media header box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        RBE16INC(buffer, position, this->graphicsMode);
        RBE16INC(buffer, position, this->colorRed);
        RBE16INC(buffer, position, this->colorGreen);
        RBE16INC(buffer, position, this->colorBlue);
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

uint32_t CVideoMediaHeaderBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE16INC(buffer, result, this->GetGraphicsMode());
    WBE16INC(buffer, result, this->GetColorRed());
    WBE16INC(buffer, result, this->GetColorGreen());
    WBE16INC(buffer, result, this->GetColorBlue());

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}