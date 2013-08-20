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

CHttpHeaderCollection::CHttpHeaderCollection(void)
  : CKeyedCollection()
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
  CHttpHeader *httpHeader = new CHttpHeader();
  bool continueAdding = (httpHeader != NULL);
  if (continueAdding)
  {
    continueAdding &= httpHeader->SetName(name);
    continueAdding &= httpHeader->SetValue(value);
  }
  if (continueAdding)
  {
    continueAdding &= __super::Add(httpHeader);
  }

  if (!continueAdding)
  {
    FREE_MEM_CLASS(httpHeader);
  }
  return continueAdding;
}

bool CHttpHeaderCollection::Add(const wchar_t *header)
{
  bool result = (header != NULL);

  if (result)
  {
    CHttpHeader *httpHeader = new CHttpHeader();
    result &= (httpHeader != NULL);

    CHECK_CONDITION_EXECUTE_RESULT(result, httpHeader->Parse(header), result);
    CHECK_CONDITION_EXECUTE_RESULT(result, __super::Add(httpHeader), result);

    if (!result)
    {
      FREE_MEM_CLASS(httpHeader);
    }
  }

  return result;
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
