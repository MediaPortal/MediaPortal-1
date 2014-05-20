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

CRtspRtpInfoResponseHeader::CRtspRtpInfoResponseHeader(void)
  : CRtspResponseHeader()
{
  this->rtpInfoTracks = new CRtpInfoTrackCollection();
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

CRtspRtpInfoResponseHeader *CRtspRtpInfoResponseHeader::Clone(void)
{
  return (CRtspRtpInfoResponseHeader *)__super::Clone();
}

bool CRtspRtpInfoResponseHeader::CloneInternal(CHttpHeader *clonedHeader)
{
  bool result = __super::CloneInternal(clonedHeader);
  CRtspRtpInfoResponseHeader *header = dynamic_cast<CRtspRtpInfoResponseHeader *>(clonedHeader);
  result &= (header != NULL);

  if (result)
  {
    result &= header->rtpInfoTracks->Append(this->rtpInfoTracks);
  }

  return result;
}

CHttpHeader *CRtspRtpInfoResponseHeader::GetNewHeader(void)
{
  return new CRtspRtpInfoResponseHeader();
}

bool CRtspRtpInfoResponseHeader::Parse(const wchar_t *header, unsigned int length)
{
  bool result = __super::Parse(header, length);

  if (result)
  {
    result &= (_wcsicmp(this->name, RTSP_RTP_INFO_RESPONSE_HEADER_TYPE) == 0);

    if (result)
    {
      // find first separator

      unsigned int position = 0;
      unsigned int valueLength = this->GetValueLength();

      while (result && (position < valueLength))
      {
        // try to find separator
        int index = IndexOf(this->value + position, valueLength - position, RTSP_RTP_INFO_RESPONSE_HEADER_TRACK_SEPARATOR, RTSP_RTP_INFO_RESPONSE_HEADER_TRACK_SEPARATOR_LENGTH);
        unsigned int tempLength = (index == (-1)) ? (valueLength - position) : (index);

        CRtpInfoTrack *rtpInfoTrack = new CRtpInfoTrack();
        result &= (rtpInfoTrack != NULL);

        if (result)
        {
          result &= rtpInfoTrack->Parse(this->value + position, tempLength);
        }

        CHECK_CONDITION_EXECUTE(result, result &= this->rtpInfoTracks->Add(rtpInfoTrack));
        CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(rtpInfoTrack));

        position += tempLength + RTSP_RTP_INFO_RESPONSE_HEADER_TRACK_SEPARATOR_LENGTH;
      }
    }
  }

  if (result)
  {
    this->responseHeaderType = Duplicate(RTSP_RTP_INFO_RESPONSE_HEADER_TYPE);
    result &= (this->responseHeaderType != NULL);
  }

  return result;
}