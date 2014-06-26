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

#include "RtspSupportedPayloadTypeCollection.h"
#include "StreamReceiveData.h"

CRtspSupportedPayloadTypeCollection::CRtspSupportedPayloadTypeCollection(HRESULT *result)
  : CCollection(result)
{
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_CONDITION_HRESULT(*result, this->EnsureEnoughSpace(3), *result, E_OUTOFMEMORY);

    CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(33, L"MP2T", NULL, RTSP_PAYLOAD_TYPE_FLAG_CONTAINER), *result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(14, L"MPA", STREAM_INPUT_FORMAT_MP3, RTSP_PAYLOAD_TYPE_FLAG_PACKETS), *result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(*result, this->AddPayloadType(32, L"MPV", STREAM_INPUT_FORMAT_MPEGVIDEO, RTSP_PAYLOAD_TYPE_FLAG_PACKETS), *result, E_OUTOFMEMORY);
  }

  if (FAILED(*result))
  {
    this->Clear();
  }
}

CRtspSupportedPayloadTypeCollection::~CRtspSupportedPayloadTypeCollection(void)
{
}

CRtspPayloadType *CRtspSupportedPayloadTypeCollection::Clone(CRtspPayloadType *item)
{
  return (CRtspPayloadType *)item->Clone();
}

bool CRtspSupportedPayloadTypeCollection::AddPayloadType(unsigned int payloadType, const wchar_t *name, const wchar_t *streamInputFormat, uint64_t flags)
{
  HRESULT result = S_OK;
  CRtspPayloadType *payload = new CRtspPayloadType(&result);
  CHECK_POINTER_HRESULT(result, payload, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    payload->SetFlags(flags);
    payload->SetId(payloadType);

    CHECK_CONDITION_HRESULT(result, payload->SetEncodingName(name), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, payload->SetStreamInputFormat(streamInputFormat), result, E_OUTOFMEMORY);
  }

  CHECK_CONDITION_HRESULT(result, this->Add(payload), result, E_OUTOFMEMORY);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(payload));

  return SUCCEEDED(result);
}