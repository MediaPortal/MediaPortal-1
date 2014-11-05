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

#include "InitializationVectorAttribute.h"
#include "conversions.h"

CInitializationVectorAttribute::CInitializationVectorAttribute(HRESULT *result)
  : CAttribute(result)
{
  this->iv = NULL;
}

CInitializationVectorAttribute::~CInitializationVectorAttribute(void)
{
  FREE_MEM(this->iv);
}

/* get methods */

/* set methods */

/* other methods */

void CInitializationVectorAttribute::Clear(void)
{
  __super::Clear();

  FREE_MEM(this->iv);
}

bool CInitializationVectorAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if ((version == PLAYLIST_VERSION_02) || (version == PLAYLIST_VERSION_03) || (version == PLAYLIST_VERSION_04) || (version == PLAYLIST_VERSION_05))
    {
      // initialization vector is exactly 16 hexadecimal numbers
      result &= (wcslen(value) == INITIALIZATION_VECTOR_VALUE_LENGTH);

      if (result)
      {
        // check first two characters 
        result &= (_wcsnicmp(value, INITIALIZATION_VECTOR_VALUE_START, INITIALIZATION_VECTOR_VALUE_START_LENGTH) == 0);

        if (result)
        {
          // length is correct, starting characters are correct, we can parse
          this->iv = HexToDec(value + INITIALIZATION_VECTOR_VALUE_START_LENGTH);
          result &= (this->iv != NULL);
        }
      }
    }
    else
    {
      result = false;
    }
  }

  return result;
}

/* protected methods */

CAttribute *CInitializationVectorAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CInitializationVectorAttribute *attribute = new CInitializationVectorAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CInitializationVectorAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CInitializationVectorAttribute *initializationVector = dynamic_cast<CInitializationVectorAttribute *>(attribute);
  result &= (initializationVector != NULL);

  if (result)
  {
    if (this->iv != NULL)
    {
      initializationVector->iv = ALLOC_MEM_SET(initializationVector->iv, uint8_t, INITIALIZATION_VECTOR_LENGTH, 0);
      result &= (initializationVector->iv != NULL);

      CHECK_CONDITION_EXECUTE(result, memcpy(initializationVector->iv, this->iv, INITIALIZATION_VECTOR_LENGTH));
    }
  }

  return result;
}