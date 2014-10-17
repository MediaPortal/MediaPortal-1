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

#include "MshsManifestCustomAttributeBox.h"
#include "BoxCollection.h"
#include "BufferHelper.h"
#include "BoxConstants.h"

CMshsManifestCustomAttributeBox::CMshsManifestCustomAttributeBox(HRESULT *result)
  : CBox(result)
{
  this->name = NULL;
  this->value = NULL;
  this->type = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MSHS_MANIFEST_CUSTOM_ATTRIBUTE_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CMshsManifestCustomAttributeBox::~CMshsManifestCustomAttributeBox(void)
{
}

/* get methods */

const wchar_t *CMshsManifestCustomAttributeBox::GetName(void)
{
  return this->name;
}

const wchar_t *CMshsManifestCustomAttributeBox::GetValue(void)
{
  return this->value;
}

/* set methods */

bool CMshsManifestCustomAttributeBox::SetName(const wchar_t *name)
{
  SET_STRING_RETURN_WITH_NULL(this->name, name);
}

bool CMshsManifestCustomAttributeBox::SetValue(const wchar_t *value)
{
  SET_STRING_RETURN_WITH_NULL(this->value, value);
}

/* other methods */

wchar_t *CMshsManifestCustomAttributeBox::GetParsedHumanReadable(const wchar_t *indent)
{
  return NULL;
}

/* protected methods */

uint64_t CMshsManifestCustomAttributeBox::GetBoxSize(void)
{
  uint64_t result = 8;
  result += (this->name != NULL) ? (wcslen(this->name) * sizeof(wchar_t)) : 0;
  result += (this->value != NULL) ? (wcslen(this->value) * sizeof(wchar_t)) : 0;

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CMshsManifestCustomAttributeBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->name);
  FREE_MEM(this->value);

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MSHS_MANIFEST_CUSTOM_ATTRIBUTE_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is MSHS manifest custom attribute box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_OUTOFMEMORY;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC_DEFINE(buffer, position, nameLength, uint32_t);

        // check if we have enough data in buffer for name
        CHECK_CONDITION_HRESULT(continueParsing, (position + nameLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (nameLength != 0))
        {
          this->name = ALLOC_MEM_SET(this->name, wchar_t, (nameLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->name, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->name, buffer + position, nameLength * sizeof(wchar_t));
            position += nameLength * sizeof(wchar_t);
          }
        }

        RBE32INC_DEFINE(buffer, position, valueLength, uint32_t);

        // check if we have enough data in buffer for name
        CHECK_CONDITION_HRESULT(continueParsing, (position + valueLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (valueLength != 0))
        {
          this->value = ALLOC_MEM_SET(this->value, wchar_t, (valueLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->value, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->value, buffer + position, valueLength * sizeof(wchar_t));
            position += valueLength * sizeof(wchar_t);
          }
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

uint32_t CMshsManifestCustomAttributeBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    unsigned int nameLength = (this->name != NULL) ? wcslen(this->name) : 0;
    unsigned int valueLength = (this->value != NULL) ? wcslen(this->value) : 0;

    WBE32INC(buffer, result, nameLength);

    if (nameLength > 0)
    {
      memcpy(buffer + result, this->name, nameLength * sizeof(wchar_t));
      result += nameLength * sizeof(wchar_t);
    }

    WBE32INC(buffer, result, valueLength);

    if (valueLength > 0)
    {
      memcpy(buffer + result, this->value, valueLength * sizeof(wchar_t));
      result += valueLength * sizeof(wchar_t);
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}