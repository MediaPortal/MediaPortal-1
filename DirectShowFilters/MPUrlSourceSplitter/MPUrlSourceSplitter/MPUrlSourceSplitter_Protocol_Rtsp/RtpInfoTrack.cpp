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

#include "RtpInfoTrack.h"
#include "conversions.h"

CRtpInfoTrack::CRtpInfoTrack(HRESULT *result)
  : CFlags()
{
  this->url = NULL;
  this->sequenceNumber = 0;
  this->rtpTimestamp = 0;
}

CRtpInfoTrack::~CRtpInfoTrack(void)
{
  FREE_MEM(this->url);
}

/* get methods */

const wchar_t *CRtpInfoTrack::GetUrl(void)
{
  return this->url;
}

unsigned int CRtpInfoTrack::GetSequenceNumber(void)
{
  return this->sequenceNumber;
}

unsigned int CRtpInfoTrack::GetRtpTimestamp(void)
{
  return this->rtpTimestamp;
}

/* set methods */

/* other methods */

bool CRtpInfoTrack::IsUrl(void)
{
  return this->IsSetFlags(RTP_INFO_TRACK_FLAG_URL);
}

bool CRtpInfoTrack::IsSequenceNumber(void)
{
  return this->IsSetFlags(RTP_INFO_TRACK_FLAG_SEQUENCE_NUMBER);
}

bool CRtpInfoTrack::IsRtpTimestamp(void)
{
  return this->IsSetFlags(RTP_INFO_TRACK_FLAG_RTP_TIMESTAMP);
}

CRtpInfoTrack *CRtpInfoTrack::Clone(void)
{
  HRESULT result = S_OK;
  CRtpInfoTrack *rtpInfoTrack = new CRtpInfoTrack(&result);
  CHECK_POINTER_HRESULT(result, rtpInfoTrack, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    rtpInfoTrack->flags = this->flags;
    rtpInfoTrack->sequenceNumber = this->sequenceNumber;
    rtpInfoTrack->rtpTimestamp = this->rtpTimestamp;

    SET_STRING_AND_RESULT_WITH_NULL(rtpInfoTrack->url, this->url, result);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(rtpInfoTrack));
  return rtpInfoTrack;
}

bool CRtpInfoTrack::Parse(const wchar_t *rtpInfoTrack, unsigned int length)
{
  bool result = (rtpInfoTrack != NULL) && (length > 0);

  if (result)
  {
    // find first separator
    unsigned int position = SkipBlanks(rtpInfoTrack) - rtpInfoTrack;

    while (result && (position < length))
    {
      // try to find separator
      int index = IndexOf(rtpInfoTrack + position, length - position, RTP_INFO_PARAMETER_SEPARATOR, RTP_INFO_PARAMETER_SEPARATOR_LENGTH);
      unsigned int tempLength = (index == (-1)) ? (length - position) : (index);

      if (wcsncmp(rtpInfoTrack + position, RTP_INFO_TRACK_PARAMETER_URL, RTP_INFO_TRACK_PARAMETER_URL_LENGTH) == 0)
      {
        int index2 = IndexOf(rtpInfoTrack + position, tempLength, RTP_INFO_PARAMETER_VALUE_SEPARATOR, RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH);
        result &= (index2 > 0);

        if (result)
        {
          this->url = Substring(rtpInfoTrack, position + index2 + RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH, tempLength - index2 - RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH);
          result &= (this->url != NULL);

          if (result)
          {
            this->flags |= RTP_INFO_TRACK_FLAG_URL;
          }
        }
      }
      else if (wcsncmp(rtpInfoTrack + position, RTP_INFO_TRACK_PARAMETER_SEQUENCE_NUMBER, RTP_INFO_TRACK_PARAMETER_SEQUENCE_NUMBER_LENGTH) == 0)
      {
        int index2 = IndexOf(rtpInfoTrack + position, tempLength, RTP_INFO_PARAMETER_VALUE_SEPARATOR, RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH);
        result &= (index2 > 0);

        if (result)
        {
          this->sequenceNumber = GetValueUint(rtpInfoTrack + position + index2 + RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

          this->flags |= RTP_INFO_TRACK_FLAG_SEQUENCE_NUMBER;
        }
      }
      else if (wcsncmp(rtpInfoTrack + position, RTP_INFO_TRACK_PARAMETER_RTP_TIMESTAMP, RTP_INFO_TRACK_PARAMETER_RTP_TIMESTAMP_LENGTH) == 0)
      {
        int index2 = IndexOf(rtpInfoTrack + position, tempLength, RTP_INFO_PARAMETER_VALUE_SEPARATOR, RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH);
        result &= (index2 > 0);

        if (result)
        {
          this->rtpTimestamp = GetValueUint(rtpInfoTrack + position + index2 + RTP_INFO_PARAMETER_VALUE_SEPARATOR_LENGTH, 0);

          this->flags |= RTP_INFO_TRACK_FLAG_RTP_TIMESTAMP;
        }
      }
      else
      {
        // unknown parameter, ignore
      }

      position += tempLength + RTP_INFO_PARAMETER_SEPARATOR_LENGTH;
    }
  }

  return result;
}