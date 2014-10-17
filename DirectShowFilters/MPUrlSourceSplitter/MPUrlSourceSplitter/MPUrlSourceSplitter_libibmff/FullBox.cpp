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

#include "FullBox.h"
#include "BoxCollection.h"

CFullBox::CFullBox(HRESULT *result)
  : CBox(result)
{
  this->version = 0;
  this->boxFlags = 0;
}

CFullBox::~CFullBox(void)
{
}

/* get methods */

uint8_t CFullBox::GetVersion(void)
{
  return this->version;
}

uint32_t CFullBox::GetBoxFlags(void)
{
  return this->boxFlags;
}

bool CFullBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

void CFullBox::SetVersion(uint8_t version)
{
  this->version = version;
}

void CFullBox::SetBoxFlags(uint32_t flags)
{
  this->boxFlags = flags;
}

/* other methods */

wchar_t *CFullBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sVersion: %u\n" \
      L"%sBox flags: 0x%06X",
      
      previousResult,
      indent, this->GetVersion(),
      indent, this->GetFlags()
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CFullBox::GetBoxSize(void)
{
  uint64_t result = 4;

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CFullBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  this->version = 0;
  this->boxFlags = 0;

  if (__super::ParseInternal(buffer, length, false))
  {
    // box is full box, parse all values
    uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
    bool continueParsing = ((position + FULL_BOX_DATA_SIZE) <= min(length, (uint32_t)this->GetSize()));

    if (continueParsing)
    {
      RBE8INC(buffer, position, this->version);
      RBE24INC(buffer, position, this->boxFlags);
    }

    if (continueParsing && processAdditionalBoxes)
    {
      this->ProcessAdditionalBoxes(buffer, length, position);
    }

    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= continueParsing ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
  }

  return this->IsSetFlags(BOX_FLAG_PARSED);
}

uint32_t CFullBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE8INC(buffer, result, this->GetVersion());
    WBE24INC(buffer, result, this->GetBoxFlags());

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}