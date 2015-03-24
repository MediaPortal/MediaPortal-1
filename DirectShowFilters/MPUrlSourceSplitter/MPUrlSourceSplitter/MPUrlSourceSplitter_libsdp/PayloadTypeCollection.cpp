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

#include "PayloadTypeCollection.h"

CPayloadTypeCollection::CPayloadTypeCollection(HRESULT *result)
  : CKeyedCollection(result)
{
}

CPayloadTypeCollection::~CPayloadTypeCollection(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CPayloadTypeCollection::AddPayloadType(unsigned int id, const wchar_t *encodingName, CPayloadType::MediaType mediaType, unsigned int clockRate, unsigned int channels)
{
  HRESULT result = S_OK;
  CPayloadType *payloadType = new CPayloadType(&result);
  CHECK_CONDITION_HRESULT(result, payloadType, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    payloadType->SetId(id);
    CHECK_CONDITION_HRESULT(result, payloadType->SetEncodingName(encodingName), result, E_OUTOFMEMORY);
    payloadType->SetMediaType(mediaType);
    payloadType->SetClockRate(clockRate);
    payloadType->SetChannels(channels);

    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->Add(payloadType), result);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payloadType));
  return SUCCEEDED(result);
}

/* protected methods */

int CPayloadTypeCollection::CompareItemKeys(const wchar_t *firstKey, const wchar_t *secondKey, void *context)
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

const wchar_t *CPayloadTypeCollection::GetKey(CPayloadType *item)
{
  return item->GetEncodingName();
}

CPayloadType *CPayloadTypeCollection::Clone(CPayloadType *item)
{
  return item->Clone();
}
