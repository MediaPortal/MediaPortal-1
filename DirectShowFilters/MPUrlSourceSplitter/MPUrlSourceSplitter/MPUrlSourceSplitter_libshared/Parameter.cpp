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

CParameter::CParameter(const wchar_t *name, const wchar_t *value)
{
  bool result = true;
  this->name = NULL;
  this->value = NULL;

  SET_STRING_AND_RESULT(this->name, name, result);
  SET_STRING_AND_RESULT(this->value, value, result);

  if (!result)
  {
    this->Clear();
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

unsigned int CParameter::GetNameLength(void)
{
  return (this->name == NULL) ? UINT_MAX : wcslen(this->name);
}

const wchar_t *CParameter::GetValue(void)
{
  return this->value;
}

unsigned int CParameter::GetValueLength(void)
{
  return (this->value == NULL) ? UINT_MAX : wcslen(this->value);
}

CParameter *CParameter::Clone(void)
{
  CParameter *clone = new CParameter(this->name, this->value);

  return clone;
}
