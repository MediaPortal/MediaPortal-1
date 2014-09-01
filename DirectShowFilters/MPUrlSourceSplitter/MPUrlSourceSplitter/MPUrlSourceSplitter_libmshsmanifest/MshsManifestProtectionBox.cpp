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

#include "MshsManifestProtectionBox.h"
#include "BoxCollection.h"
#include "BufferHelper.h"
#include "BoxConstants.h"

CMshsManifestProtectionBox::CMshsManifestProtectionBox(HRESULT *result)
  : CBox(result)
{
  this->systemId = GUID_NULL;
  this->content = NULL;
  this->type = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MSHS_MANIFEST_PROTECTION_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CMshsManifestProtectionBox::~CMshsManifestProtectionBox(void)
{
  FREE_MEM(this->content);
}

/* get methods */

GUID CMshsManifestProtectionBox::GetSystemId(void)
{
  return this->systemId;
}

const wchar_t *CMshsManifestProtectionBox::GetContent(void)
{
  return this->content;
}

bool CMshsManifestProtectionBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

/* set methods */

void CMshsManifestProtectionBox::SetSystemId(GUID systemId)
{
  this->systemId = systemId;
}

bool CMshsManifestProtectionBox::SetContent(const wchar_t *content)
{
  SET_STRING_RETURN_WITH_NULL(this->content, content);
}

/* other methods */

bool CMshsManifestProtectionBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CMshsManifestProtectionBox::GetParsedHumanReadable(const wchar_t *indent)
{
  return NULL;
}

/* protected methods */

uint64_t CMshsManifestProtectionBox::GetBoxSize(void)
{
  uint64_t result = sizeof(GUID) + 4;
  result += (this->content != NULL) ? (wcslen(this->content) * sizeof(wchar_t)) : 0;

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CMshsManifestProtectionBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM(this->content);

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MSHS_MANIFEST_PROTECTION_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is MSHS manifest protection box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_OUTOFMEMORY;

      if (SUCCEEDED(continueParsing))
      {
        memcpy(&this->systemId, buffer + position, sizeof(GUID));
        position += sizeof(GUID);

        RBE32INC_DEFINE(buffer, position, contentLength, uint32_t);
        // check if we have enough data in buffer for content
        CHECK_CONDITION_HRESULT(continueParsing, (this->GetSize() + contentLength * sizeof(wchar_t)) <= length, continueParsing, E_OUTOFMEMORY);

        if (SUCCEEDED(continueParsing) && (contentLength != 0))
        {
          this->content = ALLOC_MEM_SET(this->content, wchar_t, (contentLength + 1), 0);
          CHECK_POINTER_HRESULT(continueParsing, this->content, continueParsing, E_OUTOFMEMORY);

          if (SUCCEEDED(continueParsing))
          {
            memcpy(this->content, buffer + position, contentLength * sizeof(wchar_t));
            position += contentLength * sizeof(wchar_t);
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

uint32_t CMshsManifestProtectionBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    memcpy(buffer + result, &this->systemId, sizeof(GUID));
    result += sizeof(GUID);

    unsigned int contentLength = (this->content != NULL) ? wcslen(this->content) : 0;

    WBE32INC(buffer, result, contentLength);

    if (contentLength > 0)
    {
      memcpy(buffer + result, this->content, contentLength * sizeof(wchar_t));
      result += contentLength * sizeof(wchar_t);
    }

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}