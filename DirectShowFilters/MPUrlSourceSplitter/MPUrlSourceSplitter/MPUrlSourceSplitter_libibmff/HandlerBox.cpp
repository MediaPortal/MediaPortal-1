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

#include "HandlerBox.h"
#include "BoxCollection.h"

CHandlerBox::CHandlerBox(void)
  : CFullBox()
{
  this->type = Duplicate(HANDLER_BOX_TYPE);
  this->name = Duplicate(L"");
  this->handlerType = 0;
}

CHandlerBox::~CHandlerBox(void)
{
  FREE_MEM(this->name);
}

/* get methods */

bool CHandlerBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

uint32_t CHandlerBox::GetHandlerType(void)
{
  return this->handlerType;
}

const wchar_t *CHandlerBox::GetName(void)
{
  return this->name;
}

/* set methods */

void CHandlerBox::SetHandlerType(uint32_t handlerType)
{
  this->handlerType = handlerType;
}

bool CHandlerBox::SetName(const wchar_t *name)
{
  SET_STRING_RETURN(this->name, name);
}

/* other methods */

bool CHandlerBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CHandlerBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sHandler type: 0x%08X\n" \
      L"%sName: '%s'" 
      ,
      
      previousResult,
      indent, this->GetHandlerType(),
      indent, this->GetName()
      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CHandlerBox::GetBoxSize(void)
{
  uint64_t result = this->GetStringSize(this->GetName());

  if (result != 0)
  {
    result += 20;
  }

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CHandlerBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->name);
  this->handlerType = 0;

  // in bad case we don't have objects, but still it can be valid box
  bool result = __super::ParseInternal(buffer, length, false);

  if (result)
  {
    if (wcscmp(this->type, HANDLER_BOX_TYPE) != 0)
    {
      // incorect box type
      this->parsed = false;
    }
    else
    {
      // box is file type box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      bool continueParsing = (this->GetSize() <= (uint64_t)length);
      
      if (continueParsing)
      {
        // pre-defined field uint32_t
        position += 4;

        // handler type uint32_t
        RBE32INC(buffer, position, this->handlerType);
        
        // reserved, 3 x uint32_t
        position += 12;
      }

      if (continueParsing)
      {
        uint32_t positionAfter = position;
        continueParsing &= SUCCEEDED(this->GetString(buffer, length, position, &this->name, &positionAfter));

        if (continueParsing)
        {
          position = positionAfter;
        }
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

uint32_t CHandlerBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    // pre-defined field uint32_t
    result += 4;

    // handler type uint32_t
    WBE32INC(buffer, result, this->GetHandlerType());

    // reserved, 3 x uint32_t
    result += 12;

    uint32_t res = this->SetString(buffer + result, length - result, this->GetName());
    result = (res != 0) ? (result + res) : 0;

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}