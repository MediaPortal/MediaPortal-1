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

#include "PreciseAttribute.h"

CPreciseAttribute::CPreciseAttribute(HRESULT *result)
  : CAttribute(result)
{
}

CPreciseAttribute::~CPreciseAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CPreciseAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
    {
      wchar_t *precise = CAttribute::GetEnumeratedString(value);
      result &= (precise != NULL);

      if (result)
      {
        this->flags |= (wcscmp(precise, PRECISE_YES) == 0) ? PRECISE_ATTRIBUTE_FLAG_YES : PRECISE_ATTRIBUTE_FLAG_NONE;
        this->flags |= (wcscmp(precise, PRECISE_NO) == 0) ? PRECISE_ATTRIBUTE_FLAG_NO : PRECISE_ATTRIBUTE_FLAG_NONE;
      }

      FREE_MEM(precise);
    }
    else
    {
      result = false;
    }
  }

  return result;
}

/* protected methods */

CAttribute *CPreciseAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CPreciseAttribute *attribute = new CPreciseAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CPreciseAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CPreciseAttribute *precise = dynamic_cast<CPreciseAttribute *>(attribute);
  result &= (precise != NULL);

  return result;
}