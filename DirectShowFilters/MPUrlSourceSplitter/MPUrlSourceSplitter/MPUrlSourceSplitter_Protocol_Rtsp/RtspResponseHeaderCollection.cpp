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

#include "RtspResponseHeaderCollection.h"

CRtspResponseHeaderCollection::CRtspResponseHeaderCollection(HRESULT *result)
  : CKeyedCollection(result)
{
}

CRtspResponseHeaderCollection::~CRtspResponseHeaderCollection(void)
{
}

/* get methods */

CRtspResponseHeader *CRtspResponseHeaderCollection::GetRtspHeader(const wchar_t *name, bool invariant)
{
  return this->GetItem(name, (void *)&invariant);
}

CRtspResponseHeader *CRtspResponseHeaderCollection::GetRtspHeader(const wchar_t *responseHeaderType)
{
  CRtspResponseHeader *result = NULL;

  for (unsigned int i = 0; i < this->Count(); i++)
  {
    CRtspResponseHeader *header = this->GetItem(i);

    if (header->IsResponseHeaderType(responseHeaderType))
    {
      result = header;
      break;
    }
  }

  return result;
}

/* set methods */

/* other methods */

int CRtspResponseHeaderCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
{
  bool invariant = (*(bool *)context);

  return (invariant) ? CompareWithNullInvariant(firstKey, secondKey) : CompareWithNull(firstKey, secondKey);
}

const wchar_t *CRtspResponseHeaderCollection::GetKey(CRtspResponseHeader *item)
{
  return item->GetName();
}

CRtspResponseHeader *CRtspResponseHeaderCollection::Clone(CRtspResponseHeader *item)
{
  return (CRtspResponseHeader *)item->Clone();
}
