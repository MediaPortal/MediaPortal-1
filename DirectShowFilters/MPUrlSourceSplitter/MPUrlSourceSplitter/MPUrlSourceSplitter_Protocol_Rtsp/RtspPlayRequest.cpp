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

#include "RtspPlayRequest.h"
#include "RtspNormalPlayTimeRangeRequestHeader.h"

CRtspPlayRequest::CRtspPlayRequest(void)
  : CRtspRequest()
{
}

CRtspPlayRequest::CRtspPlayRequest(bool createDefaultHeaders)
  : CRtspRequest(createDefaultHeaders)
{
}

CRtspPlayRequest::~CRtspPlayRequest(void)
{
}

/* get methods */

const wchar_t *CRtspPlayRequest::GetMethod(void)
{
  return RTSP_PLAY_METHOD;
}

/* set methods */

bool CRtspPlayRequest::SetStartTime(uint64_t startTime)
{
  bool result = (this->requestHeaders != NULL);

  if (result)
  {
    unsigned int index = this->requestHeaders->GetRtspHeaderIndex(RTSP_RANGE_REQUEST_HEADER_NAME, false);

    if (index != UINT_MAX)
    {
      if (startTime == TIME_UNSPECIFIED)
      {
        // start time is not specified, but in header can be specified end time
        // check for end time and if not specified than remove header

        CRtspNormalPlayTimeRangeRequestHeader *header = dynamic_cast<CRtspNormalPlayTimeRangeRequestHeader *>(this->requestHeaders->GetItem(index));
        result &= (header != NULL);

        if (result)
        {
          if (header->IsSetFlag(FLAG_NORMAL_PLAY_TIME_END))
          {
            header->SetStartTime(startTime);
            header->SetFlags(header->GetFlags() & (~FLAG_NORMAL_PLAY_TIME_START));
          }
          else
          {
            result &= this->requestHeaders->Remove(index);
          }
        }
      }
    }
    else if (startTime != TIME_UNSPECIFIED)
    {
      // create new normal play time request header
      CRtspNormalPlayTimeRangeRequestHeader *header = new CRtspNormalPlayTimeRangeRequestHeader();
      result &= (header != NULL);

      CHECK_CONDITION_EXECUTE(result, header->SetStartTime(startTime));
      CHECK_CONDITION_EXECUTE(result, header->SetFlags(FLAG_NORMAL_PLAY_TIME_START));
      CHECK_CONDITION_EXECUTE(result, result &= this->requestHeaders->Add(header));
      CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(header));
    }
  }

  return result;
}

bool CRtspPlayRequest::SetEndTime(uint64_t endTime)
{
  bool result = (this->requestHeaders != NULL);

  if (result)
  {
    unsigned int index = this->requestHeaders->GetRtspHeaderIndex(RTSP_RANGE_REQUEST_HEADER_NAME, false);

    if (index != UINT_MAX)
    {
      if (endTime == TIME_UNSPECIFIED)
      {
        // end time is not specified, but in header can be specified start time
        // check for start time and if not specified than remove header

        CRtspNormalPlayTimeRangeRequestHeader *header = dynamic_cast<CRtspNormalPlayTimeRangeRequestHeader *>(this->requestHeaders->GetItem(index));
        result &= (header != NULL);

        if (result)
        {
          if (header->IsSetFlag(FLAG_NORMAL_PLAY_TIME_START))
          {
            header->SetEndTime(endTime);
            header->SetFlags(header->GetFlags() & (~FLAG_NORMAL_PLAY_TIME_END));
          }
          else
          {
            result &= this->requestHeaders->Remove(index);
          }
        }
      }
    }
    else if (endTime != TIME_UNSPECIFIED)
    {
      // create new normal play time request header
      CRtspNormalPlayTimeRangeRequestHeader *header = new CRtspNormalPlayTimeRangeRequestHeader();
      result &= (header != NULL);

      CHECK_CONDITION_EXECUTE(result, header->SetEndTime(endTime));
      CHECK_CONDITION_EXECUTE(result, header->SetFlags(FLAG_NORMAL_PLAY_TIME_END));
      CHECK_CONDITION_EXECUTE(result, result &= this->requestHeaders->Add(header));
      CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(header));
    }
  }

  return result;
}

/* other methods */

CRtspPlayRequest *CRtspPlayRequest::Clone(void)
{
  return (CRtspPlayRequest *)__super::Clone();
}

bool CRtspPlayRequest::CloneInternal(CRtspRequest *clonedRequest)
{
  return __super::CloneInternal(clonedRequest);
}

CRtspRequest *CRtspPlayRequest::GetNewRequest(void)
{
  return new CRtspPlayRequest(false);
}