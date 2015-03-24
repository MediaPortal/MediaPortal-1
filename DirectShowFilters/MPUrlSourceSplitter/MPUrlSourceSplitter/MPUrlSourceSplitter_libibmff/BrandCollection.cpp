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

#include "BrandCollection.h"

CBrandCollection::CBrandCollection(HRESULT *result)
  : CKeyedCollection(result)
{
}

CBrandCollection::~CBrandCollection(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CBrandCollection::AddBrand(const wchar_t *brandString)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, brandString);

  if (SUCCEEDED(result))
  {
    CBrand *brand = new CBrand(&result);
    CHECK_POINTER_HRESULT(result, brand, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, brand->SetBrandString(brandString), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, this->Add(brand), result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(brand));
  }

  return SUCCEEDED(result);
}

/* protected methods */

int CBrandCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
{
  bool invariant = (*(bool *)context);

  if (invariant)
  {
    return _wcsicmp(firstKey, secondKey);
  }
  else
  {
    return wcscmp(firstKey, secondKey);
  }
}

const wchar_t *CBrandCollection::GetKey(CBrand *item)
{
  return item->GetBrandString();
}

CBrand *CBrandCollection::Clone(CBrand *item)
{
  return NULL;
}