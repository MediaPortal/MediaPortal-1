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

#include "EmailSourceDescriptionItem.h"

CEmailSourceDescriptionItem::CEmailSourceDescriptionItem(void)
  : CSourceDescriptionItem()
{
  this->email = NULL;
}

CEmailSourceDescriptionItem::~CEmailSourceDescriptionItem(void)
{
  FREE_MEM(this->email);
}

/* get methods */

const wchar_t *CEmailSourceDescriptionItem::GetEmail(void)
{
  return this->email;
}

/* set methods */

/* other methods */

void CEmailSourceDescriptionItem::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->email);
}

bool CEmailSourceDescriptionItem::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->type == EMAIL_SOURCE_DESCRIPTION_ITEM_TYPE);
  result &= (this->payloadSize != 0);

  if (result)
  {
    // in payload is in UTF-8 encoded string (without NULL terminating character)

    ALLOC_MEM_DEFINE_SET(temp, char, this->payloadSize + 1, 0);
    result &= (temp != NULL);

    if (result)
    {
      memcpy(temp, this->payload, this->payloadSize);
      this->email = ConvertUtf8ToUnicode(temp);
      result &= (this->email != NULL);
    }

    FREE_MEM(temp);
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}