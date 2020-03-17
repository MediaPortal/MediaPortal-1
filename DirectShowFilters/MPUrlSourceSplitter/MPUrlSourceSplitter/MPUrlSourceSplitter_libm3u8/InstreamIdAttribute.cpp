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

#include "InstreamIdAttribute.h"

CInstreamIdAttribute::CInstreamIdAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->instreamId = NULL;
}

CInstreamIdAttribute::~CInstreamIdAttribute(void)
{
  FREE_MEM(this->instreamId);
}

/* get methods */

/* set methods */

/* other methods */

void CInstreamIdAttribute::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->instreamId);
}

bool CInstreamIdAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if ((version == PLAYLIST_VERSION_06) || (version == PLAYLIST_VERSION_07))
    {
      this->instreamId = CAttribute::GetQuotedString(value);
      result &= (this->instreamId != NULL);
    }
    else
    {
      result = false;
    }
  }

  return result;
}

/* protected methods */

CAttribute *CInstreamIdAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CInstreamIdAttribute *attribute = new CInstreamIdAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CInstreamIdAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CInstreamIdAttribute *instreamId = dynamic_cast<CInstreamIdAttribute *>(attribute);
  result &= (instreamId != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(instreamId->instreamId, this->instreamId, result);
  }

  return result;
}