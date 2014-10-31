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

#include "AudioAttribute.h"

CAudioAttribute::CAudioAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->audioGroupId = NULL;
}

CAudioAttribute::~CAudioAttribute(void)
{
  FREE_MEM(this->audioGroupId);
}

/* get methods */

/* set methods */

/* other methods */

void CAudioAttribute::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->audioGroupId);
}

bool CAudioAttribute::Parse(const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(name, value);

  if (result)
  {
    this->audioGroupId = CAttribute::GetQuotedString(value);
    result &= (this->audioGroupId != NULL);
  }

  return result;
}

/* protected methods */

CAttribute *CAudioAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CAudioAttribute *attribute = new CAudioAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CAudioAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CAudioAttribute *audio = dynamic_cast<CAudioAttribute *>(attribute);
  result &= (audio != NULL);

  if (result)
  {
    SET_STRING_AND_RESULT_WITH_NULL(audio->audioGroupId, this->audioGroupId, result);
  }

  return result;
}