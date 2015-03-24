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

#include "Parameter.h"

CParameter::CParameter(HRESULT *result, const wchar_t *name, const wchar_t *value)
{
  this->name = NULL;
  this->value = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, name);
    CHECK_POINTER_DEFAULT_HRESULT(*result, value);

    if (SUCCEEDED(*result))
    {
      this->name = Duplicate(name);
      this->value = Duplicate(value);

      CHECK_POINTER_HRESULT(*result, this->name, *result, E_OUTOFMEMORY);
      CHECK_POINTER_HRESULT(*result, this->value, *result, E_OUTOFMEMORY);
    }
  }
}

CParameter::~CParameter(void)
{
  this->Clear();
}

void CParameter::Clear(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->value);
}

bool CParameter::IsValid(void)
{
  return ((this->name != NULL) && (this->value != NULL));
}

const wchar_t *CParameter::GetName(void)
{
  return this->name;
}

const wchar_t *CParameter::GetValue(void)
{
  return this->value;
}

CParameter *CParameter::Clone(void)
{
  HRESULT result = S_OK;
  CParameter *clone = new CParameter(&result, this->name, this->value);
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}
