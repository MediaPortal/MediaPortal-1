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

#include "RtspRtpInfoResponseHeader.h"
#include "conversions.h"

CRtspRtpInfoResponseHeader::CRtspRtpInfoResponseHeader(HRESULT *result)
  : CRtspResponseHeader(result)
{
  this->rtpInfoTracks = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->rtpInfoTracks = new CRtpInfoTrackCollection(result);
    CHECK_POINTER_HRESULT(*result, this->rtpInfoTracks, *result, E_OUTOFMEMORY);
  }
}

CRtspRtpInfoResponseHeader::~CRtspRtpInfoResponseHeader(void)
{
  FREE_MEM_CLASS(this->rtpInfoTracks);
}

/* get methods */

CRtpInfoTrackCollection *CRtspRtpInfoResponseHeader::GetRtpInfoTracks(void)
{
  return this->rtpInfoTracks;
}

/* set methods */

/* other methods */

bool CRtspRtpInfoResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  HRESULT result = __super::Parse(header, length) ? S_OK : E_FAIL;

  if (SUCCEEDED(result))
  {
    CHECK_CONDITION_HRESULT(result, _wcsicmp(this->name, RTSP_RTP_INFO_RESPONSE_HEADER_TYPE) == 0, result, E_FAIL);

    if (SUCCEEDED(result))
    {
      // find first separator

      unsigned int position = 0;
      unsigned int valueLength = this->GetValueLength();

      while (SUCCEEDED(result) && (position < valueLength))
      {
        // try to find separator
        int index = IndexOf(this->value + position, valueLength - position, RTSP_RTP_INFO_RESPONSE_HEADER_TRACK_SEPARATOR, RTSP_RTP_INFO_RESPONSE_HEADER_TRACK_SEPARATOR_LENGTH);
        unsigned int tempLength = (index == (-1)) ? (valueLength - position) : (index);

        CRtpInfoTrack *rtpInfoTrack = new CRtpInfoTrack(&result);
        CHECK_POINTER_HRESULT(result, rtpInfoTrack, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, rtpInfoTrack->Parse(this->value + position, tempLength), result, E_FAIL);

        CHECK_CONDITION_HRESULT(result, this->rtpInfoTracks->Add(rtpInfoTrack), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(rtpInfoTrack));

        position += tempLength + RTSP_RTP_INFO_RESPONSE_HEADER_TRACK_SEPARATOR_LENGTH;
      }
    }
  }

  if (SUCCEEDED(result))
  {
    SET_STRING_HRESULT_WITH_NULL(this->responseHeaderType, RTSP_RTP_INFO_RESPONSE_HEADER_TYPE, result);
  }

  return SUCCEEDED(result);
}

/* protected methods */

bool CRtspRtpInfoResponseHeader::CloneInternal(CHttpHeader *clone)
{
  bool result = __super::CloneInternal(clone);
  CRtspRtpInfoResponseHeader *header = dynamic_cast<CRtspRtpInfoResponseHeader *>(clone);
  result &= (header != NULL);

  if (result)
  {
    result &= header->rtpInfoTracks->Append(this->rtpInfoTracks);
  }

  return result;
}

CHttpHeader *CRtspRtpInfoResponseHeader::CreateHeader(void)
{
  HRESULT result = S_OK;
  CRtspRtpInfoResponseHeader *header = new CRtspRtpInfoResponseHeader(&result);
  CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
  return header;
}

