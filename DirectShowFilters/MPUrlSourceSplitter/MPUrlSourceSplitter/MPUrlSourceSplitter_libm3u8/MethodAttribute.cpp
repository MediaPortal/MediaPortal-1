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

#include "MethodAttribute.h"

CMethodAttribute::CMethodAttribute(HRESULT *result)
  : CAttribute(result)
{
}

CMethodAttribute::~CMethodAttribute(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CMethodAttribute::IsNone(void)
{
  return this->IsSetFlags(METHOD_ATTRIBUTE_FLAG_METHOD_NONE);
}

bool CMethodAttribute::IsAes128(void)
{
  return this->IsSetFlags(METHOD_ATTRIBUTE_FLAG_METHOD_AES_128);
}

bool CMethodAttribute::IsSampleAes(void)
{
  return this->IsSetFlags(METHOD_ATTRIBUTE_FLAG_METHOD_SAMPLE_AES);
}

bool CMethodAttribute::Parse(unsigned int version, const wchar_t *name, const wchar_t *value)
{
  bool result = __super::Parse(version, name, value);

  if (result)
  {
    if (version == PLAYLIST_VERSION_01)
    {
      wchar_t *method = CAttribute::GetEnumeratedString(this->value);
      result &= (method != NULL);

      if (result)
      {
        this->flags |= (wcscmp(method, METHOD_ATTRIBUTE_VALUE_NONE) == 0) ? METHOD_ATTRIBUTE_FLAG_METHOD_NONE : METHOD_ATTRIBUTE_FLAG_NONE;
        this->flags |= (wcscmp(method, METHOD_ATTRIBUTE_VALUE_AES_128) == 0) ? METHOD_ATTRIBUTE_FLAG_METHOD_AES_128 : METHOD_ATTRIBUTE_FLAG_NONE;
      }

      FREE_MEM(method);
    }
    else
    {
      result = false;
    }

    //this->flags |= (wcscmp(method, METHOD_ATTRIBUTE_VALUE_SAMPLE_AES) == 0) ? METHOD_ATTRIBUTE_FLAG_METHOD_SAMPLE_AES : METHOD_ATTRIBUTE_FLAG_NONE;
  }

  return result;
}

/* protected methods */

CAttribute *CMethodAttribute::CreateAttribute(void)
{
  HRESULT result = S_OK;
  CMethodAttribute *attribute = new CMethodAttribute(&result);
  CHECK_POINTER_HRESULT(result, attribute, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(attribute));
  return attribute;
}

bool CMethodAttribute::CloneInternal(CAttribute *attribute)
{
  bool result = __super::CloneInternal(attribute);
  CMethodAttribute *method = dynamic_cast<CMethodAttribute *>(attribute);
  result &= (method != NULL);

  return result;
}