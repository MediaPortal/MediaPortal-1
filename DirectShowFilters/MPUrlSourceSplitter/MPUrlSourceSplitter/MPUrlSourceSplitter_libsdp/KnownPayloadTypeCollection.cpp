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

#include "KnownPayloadTypeCollection.h"

CKnownPayloadTypeCollection::CKnownPayloadTypeCollection(HRESULT *result)
  : CPayloadTypeCollection(result)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_CONDITION_HRESULT(*result, this->EnsureEnoughSpace(KNOWN_PAYLOAD_TYPE_COUNT), *result, E_OUTOFMEMORY);

    if (SUCCEEDED(*result))
    {
      /* audio */
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(0, L"PCMU", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(3, L"GSM", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(4, L"G723", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(5, L"DVI4", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(6, L"DVI4", CPayloadType::Audio, 16000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(7, L"LPC", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(8, L"PCMA", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(9, L"G722", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(10, L"L16", CPayloadType::Audio, 44100, 2), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(11, L"L16", CPayloadType::Audio, 44100, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(12, L"QCELP", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(13, L"CN", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(14, L"MPA", CPayloadType::Audio, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(15, L"G728", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(16, L"DVI4", CPayloadType::Audio, 11025, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(17, L"DVI4", CPayloadType::Audio, 22050, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(18, L"G729", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"G726-40", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"G726-32", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"G726-24", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"G726-16", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"G729D", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"G729E", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"GSM-EFR", CPayloadType::Audio, 8000, 1), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"L8", CPayloadType::Audio, PAYLOAD_TYPE_CLOCK_RATE_VARIABLE, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"RED", CPayloadType::Audio, PAYLOAD_TYPE_CLOCK_RATE_VARIABLE, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"VDVI", CPayloadType::Audio, PAYLOAD_TYPE_CLOCK_RATE_VARIABLE, 1), *result, E_OUTOFMEMORY);

      /* video or combined */

      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(25, L"CelB", CPayloadType::Video, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(26, L"JPEG", CPayloadType::Video, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(28, L"nv", CPayloadType::Video, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(31, L"H261", CPayloadType::Video, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(32, L"MPV", CPayloadType::Video, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(33, L"MP2T", CPayloadType::Both, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(34, L"H263", CPayloadType::Video, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
      CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(PAYLOAD_TYPE_ID_DYNAMIC, L"H263-1998", CPayloadType::Video, 90000, PAYLOAD_TYPE_CHANNELS_VARIABLE), *result, E_OUTOFMEMORY);
    }

    CHECK_CONDITION_EXECUTE(FAILED(*result), this->Clear());
  }
}

CKnownPayloadTypeCollection::~CKnownPayloadTypeCollection(void)
{
}

/* get methods */

/* set methods */

/* other methods */