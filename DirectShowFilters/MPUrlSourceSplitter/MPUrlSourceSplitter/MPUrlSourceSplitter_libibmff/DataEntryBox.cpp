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

#include "DataEntryBox.h"
#include "BoxCollection.h"

CDataEntryBox::CDataEntryBox(HRESULT *result)
  : CFullBox(result)
{
}

CDataEntryBox::~CDataEntryBox(void)
{
}

/* get methods */

bool CDataEntryBox::GetBox(uint8_t *buffer, uint32_t length)
{
  return (this->GetBoxInternal(buffer, length, true) != 0);
}

bool CDataEntryBox::IsSelfContained(void)
{
  return ((this->flags & FLAGS_SELF_CONTAINED) != 0);
}

/* set methods */

void CDataEntryBox::SetSelfContained(bool selfContained)
{
  this->flags = (selfContained) ? (this->flags | FLAGS_SELF_CONTAINED) : (this->flags & (~FLAGS_SELF_CONTAINED));
}

/* other methods */

bool CDataEntryBox::Parse(const uint8_t *buffer, uint32_t length)
{
  return this->ParseInternal(buffer, length, true);
}

wchar_t *CDataEntryBox::GetParsedHumanReadable(const wchar_t *indent)
{
  wchar_t *result = NULL;
  wchar_t *previousResult = __super::GetParsedHumanReadable(indent);

  if ((previousResult != NULL) && (this->IsParsed()))
  {
    // prepare finally human readable representation
    result = FormatString(
      L"%s\n" \
      L"%sSelf contained: %s"
      ,
      
      previousResult,
      indent, this->IsSelfContained() ? L"true" : L"false"

      );
  }

  FREE_MEM(previousResult);

  return result;
}

uint64_t CDataEntryBox::GetBoxSize(void)
{
  return __super::GetBoxSize();
}

bool CDataEntryBox::ParseInternal(const unsigned char *buffer, uint32_t length, bool processAdditionalBoxes)
{
  return __super::ParseInternal(buffer, length, processAdditionalBoxes);
}

uint32_t CDataEntryBox::GetBoxInternal(uint8_t *buffer, uint32_t length, bool processAdditionalBoxes)
{
  uint32_t result = __super::GetBoxInternal(buffer, length, false);

  if (result != 0)
  {
    if ((result != 0) && processAdditionalBoxes && (this->GetBoxes()->Count() != 0))
    {
      uint32_t boxSizes = this->GetAdditionalBoxes(buffer + result, length - result);
      result = (boxSizes != 0) ? (result + boxSizes) : 0;
    }
  }

  return result;
}