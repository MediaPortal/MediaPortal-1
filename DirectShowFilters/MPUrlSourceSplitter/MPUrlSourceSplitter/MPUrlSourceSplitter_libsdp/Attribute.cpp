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

#include "Attribute.h"

CAttribute::CAttribute(HRESULT *result)
  : CSessionTag(result)
{
  this->attribute = NULL;
  this->value = NULL;
}

CAttribute::~CAttribute(void)
{
  FREE_MEM(this->attribute);
  FREE_MEM(this->value);
}

/* get methods */

const wchar_t *CAttribute::GetAttribute(void)
{
  return this->attribute;
}

const wchar_t *CAttribute::GetValue(void)
{
  return this->value;
}

/* set methods */

/* other methods */

void CAttribute::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->attribute);
  FREE_MEM(this->value);
}

unsigned int CAttribute::Parse(const wchar_t *buffer, unsigned int length)
{
  unsigned int tempResult = __super::Parse(buffer, length);
  unsigned int result = (tempResult > SESSION_TAG_SIZE) ? tempResult : 0;

  if (result != 0)
  {
    // successful parsing of session tag
    // compare it to our session tag
    result = (wcscmp(this->originalTag, TAG_ATTRIBUTE) == 0) ? result : 0;
    result = (this->tagContent != NULL) ? result : 0;
  }

  if (result != 0)
  {
    this->instanceTag = Duplicate(TAG_ATTRIBUTE_UNSPECIFIED);
    result = (this->instanceTag != NULL) ? result : 0;
    unsigned int tagContentLength = result - SESSION_TAG_SIZE - 1;

    if (result != 0)
    {
      unsigned int position = 0;
      int index = IndexOf(this->tagContent, tagContentLength, L":", 1);

      if (index == (-1))
      {
        // value is not specified
        this->attribute = Duplicate(this->tagContent);
        result = (this->attribute != NULL) ? result : 0;
      }
      else
      {
        // value is specified
        this->attribute = Substring(this->tagContent, 0, index);
        result = (this->attribute != NULL) ? result : 0;

        this->value = Substring(this->tagContent, index + 1, tagContentLength - index - 1);
        result = (this->value != NULL) ? result : 0;
      }
    }
  }

  return result;
}