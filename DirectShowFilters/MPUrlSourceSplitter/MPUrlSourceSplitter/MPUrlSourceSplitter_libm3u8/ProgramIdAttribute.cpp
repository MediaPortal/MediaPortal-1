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

#include "ProgramIdAttribute.h"

CProgramIdAttribute::CProgramIdAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->programId = PROGRAM_ID_NOT_SPECIFIED;
}

CProgramIdAttribute::~CProgramIdAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

void CProgramIdAttribute::Clear(void)
{
  __super::Clear();

  this->programId = PROGRAM_ID_NOT_SPECIFIED;
}

bool CProgramIdAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if ((version == PLAYLIST_VERSION_01) || (version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05))
    {
      this->programId = CAttribute::GetDecimalInteger(value);
      result &= (this->programId != PROGRAM_ID_NOT_SPECIFIED);
    }
    else
    {
      result = false;
    }
  }

  return result;
}

/* protected methods */

CAttribute *CProgramIdAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CProgramIdAttribute *attribute = new CProgramIdAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CProgramIdAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CProgramIdAttribute *programId = dynamic_cast<CProgramIdAttribute *>(attribute);
  result &= (programId != NULL);

  if (result)
  {
    programId->programId = this->programId;
  }

  return result;
}