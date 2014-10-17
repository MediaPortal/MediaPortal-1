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

#include "MshsManifestStreamFragmentBox.h"
#include "BoxCollection.h"
#include "BufferHelper.h"
#include "BoxConstants.h"

CMshsManifestStreamFragmentBox::CMshsManifestStreamFragmentBox(HRESULT *result)
  : CBox(result)
{
  this->fragmentDuration = 0;
  this->fragmentNumber = 0;
  this->fragmentTime = 0;
  this->type = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(MSHS_MANIFEST_STREAM_FRAGMENT_BOX_TYPE);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
  }
}

CMshsManifestStreamFragmentBox::~CMshsManifestStreamFragmentBox(void)
{
}

/* get methods */

uint32_t CMshsManifestStreamFragmentBox::GetFragmentNumber(void)
{
  return this->fragmentNumber;
}

uint64_t CMshsManifestStreamFragmentBox::GetFragmentDuration(void)
{
  return this->fragmentDuration;
}

uint64_t CMshsManifestStreamFragmentBox::GetFragmentTime(void)
{
  return this->fragmentTime;
}

/* set methods */

void CMshsManifestStreamFragmentBox::SetFragmentNumber(uint32_t fragmentNumber)
{
  this->fragmentNumber = fragmentNumber;
}

void CMshsManifestStreamFragmentBox::SetFragmentDuration(uint64_t fragmentDuration)
{
  this->fragmentDuration = fragmentDuration;
}

void CMshsManifestStreamFragmentBox::SetFragmentTime(uint64_t fragmentTime)
{
  this->fragmentTime = fragmentTime;
}

/* other methods */

wchar_t *CMshsManifestStreamFragmentBox::GetParsedHumanReadable(const wchar_t *indent)
{
  return NULL;
}

/* protected methods */

uint64_t CMshsManifestStreamFragmentBox::GetBoxSize(void)
{
  uint64_t result = 20;

  uint64_t boxSize = __super::GetBoxSize();
  result = (boxSize != 0) ? (result + boxSize) : 0; 

  return result;
}

bool CMshsManifestStreamFragmentBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, MSHS_MANIFEST_STREAM_FRAGMENT_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;
    
    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is MSHS manifest protection box, parse all values
      uint32_t position = this->HasExtendedHeader() ? BOX_HEADER_LENGTH_SIZE64 : BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_OUTOFMEMORY;

      if (SUCCEEDED(continueParsing))
      {
        RBE32INC(buffer, position, this->fragmentNumber);
        RBE64INC(buffer, position, this->fragmentDuration);
        RBE64INC(buffer, position, this->fragmentTime);
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

uint32_t CMshsManifestStreamFragmentBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    WBE32INC(buffer, result, this->fragmentNumber);
    WBE64INC(buffer, result, this->fragmentDuration);
    WBE64INC(buffer, result, this->fragmentTime);

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}