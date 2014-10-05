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

#include "SectionPayload.h"

CSectionPayload::CSectionPayload(HRESULT *result, const uint8_t *payload, uint32_t payloadSize, bool payloadUnitStart)
  : CFlags()
{
  this->payload = NULL;
  this->payloadSize = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, payload);
    CHECK_CONDITION_HRESULT(*result, payloadSize > 0, *result, E_INVALIDARG);

    if (SUCCEEDED(*result))
    {
      this->payload = payload;
      this->payloadSize = payloadSize;
      this->flags |= payloadUnitStart ? SECTION_PAYLOAD_FLAG_PAYLOAD_UNIT_START : SECTION_PAYLOAD_FLAG_NONE;
    }
  }
}

CSectionPayload::~CSectionPayload(void)
{
  // payload is only reference, do not free memory
}

/* get methods */

const uint8_t *CSectionPayload::GetPayload(void)
{
  return this->payload;
}

uint32_t CSectionPayload::GetPayloadSize(void)
{
  return this->payloadSize;
}

/* set methods */

/* other methods */

bool CSectionPayload::IsPayloadUnitStart(void)
{
  return this->IsSetFlags(SECTION_PAYLOAD_FLAG_PAYLOAD_UNIT_START);
}

CSectionPayload *CSectionPayload::Clone(void)
{
  HRESULT result = S_OK;
  CSectionPayload *sectionPayload = new CSectionPayload(&result, this->payload, this->payloadSize, this->IsPayloadUnitStart());
  CHECK_POINTER_HRESULT(result, sectionPayload, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sectionPayload));
  return sectionPayload;
}

/* protected methods */
