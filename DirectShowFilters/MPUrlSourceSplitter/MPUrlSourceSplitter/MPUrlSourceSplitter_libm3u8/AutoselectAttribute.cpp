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

#include "AutoselectAttribute.h"

CAutoselectAttribute::CAutoselectAttribute(HRESULT *result)
  : CAttribute(result)
{
}

CAutoselectAttribute::~CAutoselectAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CAutoselectAttribute::Parse(const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(name, value);

  if (result)
  {
    wchar_t *defaultValue = CAttribute::GetEnumeratedString(value);
    result &= (defaultValue != NULL);

    if (result)
    {
      this->flags |= (wcscmp(defaultValue, AUTOSELECT_YES) == 0) ? AUTOSELECT_ATTRIBUTE_FLAG_YES : AUTOSELECT_ATTRIBUTE_FLAG_NONE;
      this->flags |= (wcscmp(defaultValue, AUTOSELECT_NO) == 0) ? AUTOSELECT_ATTRIBUTE_FLAG_NO : AUTOSELECT_ATTRIBUTE_FLAG_NONE;
    }

    FREE_MEM(defaultValue);
  }

  return result;
}

/* protected methods */

CAttribute *CAutoselectAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CAutoselectAttribute *attribute = new CAutoselectAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CAutoselectAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CAutoselectAttribute *autoselect = dynamic_cast<CAutoselectAttribute *>(attribute);
  result &= (autoselect != NULL);

  return result;
}