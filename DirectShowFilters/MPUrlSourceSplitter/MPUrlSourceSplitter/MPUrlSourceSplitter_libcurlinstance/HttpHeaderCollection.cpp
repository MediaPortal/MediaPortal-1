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

#include "HttpHeaderCollection.h"

CHttpHeaderCollection::CHttpHeaderCollection(HRESULT *result)
  : CKeyedCollection(result)
{
}

CHttpHeaderCollection::~CHttpHeaderCollection(void)
{
}

/* get methods */

CHttpHeader *CHttpHeaderCollection::GetHeader(const wchar_t *name, bool invariant)
{
  return this->GetItem(name, (void *)&invariant);
}

/* set methods */

/* other methods */

bool CHttpHeaderCollection::Add(const wchar_t *name, const wchar_t *value)
{
  HRESULT result = S_OK;
  CHttpHeader *httpHeader = new CHttpHeader(&result);
  CHECK_POINTER_HRESULT(result, httpHeader, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, httpHeader->SetName(name), result, E_OUTOFMEMORY);
  CHECK_CONDITION_HRESULT(result, httpHeader->SetValue(value), result, E_OUTOFMEMORY);
  CHECK_CONDITION_HRESULT(result, __super::Add(httpHeader), result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(httpHeader));

  return SUCCEEDED(result);
}

bool CHttpHeaderCollection::Add(const wchar_t *header)
{
  HRESULT result = (header != NULL) ? S_OK : E_POINTER;

  if (SUCCEEDED(result))
  {
    CHttpHeader *httpHeader = new CHttpHeader(&result);
    CHECK_POINTER_HRESULT(result, httpHeader, result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(result, httpHeader->Parse(header), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, __super::Add(httpHeader), result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(httpHeader));
  }

  return SUCCEEDED(result);
}

int CHttpHeaderCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
{
  bool invariant = (*(bool *)context);

  return (invariant) ? CompareWithNullInvariant(firstKey, secondKey) : CompareWithNull(firstKey, secondKey);
}

const wchar_t *CHttpHeaderCollection::GetKey(CHttpHeader *item)
{
  return item->GetName();
}

CHttpHeader *CHttpHeaderCollection::Clone(CHttpHeader *item)
{
  return item->Clone();
}
