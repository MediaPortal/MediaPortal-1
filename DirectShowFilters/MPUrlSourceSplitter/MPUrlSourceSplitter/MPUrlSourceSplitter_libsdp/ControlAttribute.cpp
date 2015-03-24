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

#include "ControlAttribute.h"

CControlAttribute::CControlAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->controlUrl = NULL;
}

CControlAttribute::~CControlAttribute(void)
{
  FREE_MEM(this->controlUrl);
}

/* get methods */

const wchar_t *CControlAttribute::GetControlUrl(void)
{
  return this->controlUrl;
}

/* set methods */

/* other methods */

bool CControlAttribute::IsAsterisk(void)
{
  return this->IsSetFlags(CONTROL_ATTRIBUTE_FLAG_ASTERISK);
}

unsigned int CControlAttribute::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_ATTRIBUTE) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;

    result = (wcscmp(this->attribute, TAG_ATTRIBUTE_CONTROL) == 0) ? result : 0;
    result = (this->value != NULL) ? result : 0;
  }

  if (result != 0)
  {
    FREE_MEM(this->instanceTag);
    this->instanceTag = Duplicate(TAG_ATTRIBUTE_INSTANCE_CONTROL);
    result = (this->instanceTag != NULL) ? result : 0;

    this->controlUrl = Duplicate(this->value);
    result = (this->controlUrl != NULL) ? result : 0;

    if (result != 0)
    {
      unsigned int valueLength = wcslen(this->value);
      result = (valueLength != 0) ? result : 0;
    }
  }

  if (result != 0)
  {
    // set flags
    if (wcscmp(this->value, CONTROL_ATTRIBUTE_ASTERISK) == 0)
    {
      this->flags |= CONTROL_ATTRIBUTE_FLAG_ASTERISK;
    }
  }

  return result;
}

void CControlAttribute::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->controlUrl);
}