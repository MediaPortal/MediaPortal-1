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

#include "PhoneNumberSourceDescriptionItem.h"

CPhoneNumberSourceDescriptionItem::CPhoneNumberSourceDescriptionItem(HRESULT *result)
  : CSourceDescriptionItem(result)
{
  this->phoneNumber = NULL;
  this->type = PHONE_NUMBER_SOURCE_DESCRIPTION_ITEM_TYPE;
}

CPhoneNumberSourceDescriptionItem::~CPhoneNumberSourceDescriptionItem(void)
{
  FREE_MEM(this->phoneNumber);
}

/* get methods */

unsigned int CPhoneNumberSourceDescriptionItem::GetType(void)
{
  return PHONE_NUMBER_SOURCE_DESCRIPTION_ITEM_TYPE;
}

unsigned int CPhoneNumberSourceDescriptionItem::GetSize(void)
{
  unsigned int size = __super::GetSize();

  // it is in UTF-8 encoded string (without NULL terminating character)
  char *result = ConvertUnicodeToUtf8(this->GetPhoneNumber());
  size += (result != NULL) ? strlen(result) : 0;

  FREE_MEM(result);
  return size;
}

bool CPhoneNumberSourceDescriptionItem::GetSourceDescriptionItem(unsigned char *buffer, unsigned int length)
{
  bool result = __super::GetSourceDescriptionItem(buffer, length);

  if (result)
  {
    unsigned int position = __super::GetSize();
    char *converted = ConvertUnicodeToUtf8(this->GetPhoneNumber());
    result &= (converted != NULL);

    if (result)
    {
      memcpy(buffer + position, converted, strlen(converted));
    }

    FREE_MEM(converted);
  }

  return result;
}

const wchar_t *CPhoneNumberSourceDescriptionItem::GetPhoneNumber(void)
{
  return this->phoneNumber;
}

/* set methods */

bool CPhoneNumberSourceDescriptionItem::SetPhoneNumber(const wchar_t *phoneNumber)
{
  SET_STRING_RETURN_WITH_NULL(this->phoneNumber, phoneNumber);
}

/* other methods */

void CPhoneNumberSourceDescriptionItem::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->phoneNumber);
  this->type = PHONE_NUMBER_SOURCE_DESCRIPTION_ITEM_TYPE;
}

bool CPhoneNumberSourceDescriptionItem::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->type == PHONE_NUMBER_SOURCE_DESCRIPTION_ITEM_TYPE);
  result &= (this->payloadSize != 0);

  if (result)
  {
    // in payload is in UTF-8 encoded string (without NULL terminating character)

    ALLOC_MEM_DEFINE_SET(temp, char, this->payloadSize + 1, 0);
    result &= (temp != NULL);

    if (result)
    {
      memcpy(temp, this->payload, this->payloadSize);
      this->phoneNumber = ConvertUtf8ToUnicode(temp);
      result &= (this->phoneNumber != NULL);
    }

    FREE_MEM(temp);
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}