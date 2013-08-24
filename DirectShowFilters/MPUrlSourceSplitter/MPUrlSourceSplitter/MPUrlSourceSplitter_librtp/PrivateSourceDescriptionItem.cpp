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

#include "PrivateSourceDescriptionItem.h"

CPrivateSourceDescriptionItem::CPrivateSourceDescriptionItem(void)
  : CSourceDescriptionItem()
{
  this->type = PRIVATE_SOURCE_DESCRIPTION_ITEM_TYPE;
}

CPrivateSourceDescriptionItem::~CPrivateSourceDescriptionItem(void)
{
}

/* get methods */

unsigned int CPrivateSourceDescriptionItem::GetType(void)
{
  return PRIVATE_SOURCE_DESCRIPTION_ITEM_TYPE;
}

unsigned int CPrivateSourceDescriptionItem::GetSize(void)
{
  return __super::GetSize();
}

bool CPrivateSourceDescriptionItem::GetSourceDescriptionItem(unsigned char *buffer, unsigned int length)
{
  return __super::GetSourceDescriptionItem(buffer, length);
}

/* set methods */

/* other methods */

void CPrivateSourceDescriptionItem::Clear(void)
{
  __super::Clear();

  this->type = PRIVATE_SOURCE_DESCRIPTION_ITEM_TYPE;
}

bool CPrivateSourceDescriptionItem::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->type == PRIVATE_SOURCE_DESCRIPTION_ITEM_TYPE);
  result &= (this->payloadSize != 0);

  if (!result)
  {
    this->Clear();
  }

  return result;
}