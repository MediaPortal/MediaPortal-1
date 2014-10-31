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

#include "ForcedAttribute.h"

CForcedAttribute::CForcedAttribute(HRESULT *result)
  : CAttribute(result)
{
}

CForcedAttribute::~CForcedAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CForcedAttribute::Parse(const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(name, value);

  if (result)
  {
    wchar_t *defaultValue = CAttribute::GetEnumeratedString(value);
    result &= (defaultValue != NULL);

    if (result)
    {
      this->flags |= (wcscmp(defaultValue, FORCED_YES) == 0) ? FORCED_ATTRIBUTE_FLAG_YES : FORCED_ATTRIBUTE_FLAG_NONE;
      this->flags |= (wcscmp(defaultValue, FORCED_NO) == 0) ? FORCED_ATTRIBUTE_FLAG_NO : FORCED_ATTRIBUTE_FLAG_NONE;
    }

    FREE_MEM(defaultValue);
  }

  return result;
}

/* protected methods */

CAttribute *CForcedAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CForcedAttribute *attribute = new CForcedAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CForcedAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CForcedAttribute *forced = dynamic_cast<CForcedAttribute *>(attribute);
  result &= (forced != NULL);

  return result;
}