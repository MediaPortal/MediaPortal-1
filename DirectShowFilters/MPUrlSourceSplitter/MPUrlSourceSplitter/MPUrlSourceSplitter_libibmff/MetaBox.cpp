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

#include "MetaBox.h"
#include "BoxCollection.h"

CMetaBox::CMetaBox(HRESULT *result)
  : CFullBox(result)
{
  this->type = NULL;
  this->handlerBox = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->type = Duplicate(META_BOX_TYPE);
    this->handlerBox = new CHandlerBox(result);

    CHECK_POINTER_HRESULT(*result, this->type, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->handlerBox, *result, E_OUTOFMEMORY);
  }
}

CMetaBox::~CMetaBox(void)
{
  FREE_MEM_CLASS(this->handlerBox);
}

/* get methods */

CHandlerBox *CMetaBox::GetHandlerBox(void)
{
  return this->handlerBox;
}

/* set methods */

/* other methods */

wchar_t *CMetaBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    wchar_t *tempIndent = FormatString(L"%s\t", indent);

    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sHandler box:%s" \
      L"%s"
      ,
      
      previousResult,
      indent, (this->GetHandlerBox() == NULL) ? L"" : L"\n",
      (this->GetHandlerBox() == NULL) ? L"" : this->GetHandlerBox()->GetParsedHumanReadable(tempIndent)
      );

    FREE_MEM(tempIndent);
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CMetaBox::GetBoxSize(void)
{
  uint64_t result = this->GetHandlerBox()->GetSize();

  if (result != 0)
  {
    uint64_t boxSize = __super::GetBoxSize();
    result = (boxSize != 0) ? (result + boxSize) : 0; 
  }

  return result;
}

bool CMetaBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  FREE_MEM_CLASS(this->handlerBox);

  if (__super::ParseInternal(buffer, length, false))
  {
    this->flags &= ~BOX_FLAG_PARSED;
    this->flags |= (wcscmp(this->type, META_BOX_TYPE) == 0) ? BOX_FLAG_PARSED : BOX_FLAG_NONE;

    if (this->IsSetFlags(BOX_FLAG_PARSED))
    {
      // box is meta box, parse all values
      uint32_t position = this->HasExtendedHeader() ? FULL_BOX_HEADER_LENGTH_SIZE64 : FULL_BOX_HEADER_LENGTH;
      HRESULT continueParsing = (this->GetSize() <= (uint64_t)length) ? S_OK : E_NOT_VALID_STATE;

      if (SUCCEEDED(continueParsing))
      {
        this->handlerBox = new CHandlerBox(&continueParsing);
        CHECK_POINTER_HRESULT(continueParsing, this->handlerBox, continueParsing, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(continueParsing, this->handlerBox->Parse(buffer + position, (uint32_t)this->GetSize() - position), continueParsing, E_FAIL);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(continueParsing), position += (uint32_t)this->handlerBox->GetSize());

        CHECK_CONDITION_EXECUTE(FAILED(continueParsing), FREE_MEM_CLASS(this->handlerBox));
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

uint32_t CMetaBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    result = this->GetHandlerBox()->GetBox(buffer + result, length - result) ? (result + (uint32_t)this->GetHandlerBox()->GetSize()) : 0;

    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}