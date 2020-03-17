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

#include "RangeAttribute.h"

CRangeAttribute::CRangeAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->rangeSpecification = NULL;
}

CRangeAttribute::~CRangeAttribute(void)
{
  FREE_MEM(this->rangeSpecification);
}

/* get methods */

const wchar_t *CRangeAttribute::GetRangeSpecification(void)
{
  return this->rangeSpecification;
}

/* set methods */

/* other methods */

unsigned int CRangeAttribute::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_ATTRIBUTE) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;

    result = (wcscmp(this->attribute, TAG_ATTRIBUTE_RANGE) == 0) ? result : 0;
    result = (this->value != NULL) ? result : 0;
  }

  if (result != 0)
  {
    FREE_MEM(this->instanceTag);
    this->instanceTag = Duplicate(TAG_ATTRIBUTE_INSTANCE_RANGE);
    result = (this->instanceTag != NULL) ? result : 0;

    this->rangeSpecification = Duplicate(this->value);
    result = (this->rangeSpecification != NULL) ? result : 0;

    if (result != 0)
    {
      unsigned int valueLength = wcslen(this->value);
      result = (valueLength != 0) ? result : 0;
    }
  }

  return result;
}

void CRangeAttribute::Clear(void)
{
  FREE_MEM(this->rangeSpecification);
}