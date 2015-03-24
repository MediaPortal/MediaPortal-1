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

CRtspPlayRequest::CRtspPlayRequest(HRESULT *result)
  : CRtspRequest(result)
{
}

CRtspPlayRequest::CRtspPlayRequest(HRESULT *result, bool createDefaultHeaders)
  : CRtspRequest(result, createDefaultHeaders)
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
  HRESULT result = (this->requestHeaders != NULL) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    unsigned int index = this->requestHeaders->GetRtspHeaderIndex(RTSP_RANGE_REQUEST_HEADER_NAME, false);

    if (index != UINT_MAX)
    {
      if (startTime == TIME_UNSPECIFIED)
      {
        // start time is not specified, but in header can be specified end time
        // check for end time and if not specified than remove header

        CRtspNormalPlayTimeRangeRequestHeader *header = dynamic_cast<CRtspNormalPlayTimeRangeRequestHeader *>(this->requestHeaders->GetItem(index));
        CHECK_POINTER_HRESULT(result, header, result, E_FAIL);

        if (SUCCEEDED(result))
        {
          if (header->IsSetFlags(NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_END))
          {
            header->SetStartTime(startTime);
            header->SetFlags(header->GetFlags() & (~NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_START));
          }
          else
          {
            CHECK_CONDITION_HRESULT(result, this->requestHeaders->Remove(index), result, E_FAIL);
          }
        }
      }
    }
    else if (startTime != TIME_UNSPECIFIED)
    {
      // create new normal play time request header
      CRtspNormalPlayTimeRangeRequestHeader *header = new CRtspNormalPlayTimeRangeRequestHeader(&result);
      CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        header->SetStartTime(startTime);
        header->SetFlags(header->GetFlags() | NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_START);
      }

      CHECK_CONDITION_HRESULT(result, this->requestHeaders->Add(header), result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
    }
  }

  return SUCCEEDED(result);
}

bool CRtspPlayRequest::SetEndTime(uint64_t endTime)
{
  HRESULT result = (this->requestHeaders != NULL) ? S_OK : E_NOT_VALID_STATE;

  if (SUCCEEDED(result))
  {
    unsigned int index = this->requestHeaders->GetRtspHeaderIndex(RTSP_RANGE_REQUEST_HEADER_NAME, false);

    if (index != UINT_MAX)
    {
      if (endTime == TIME_UNSPECIFIED)
      {
        // end time is not specified, but in header can be specified start time
        // check for start time and if not specified than remove header

        CRtspNormalPlayTimeRangeRequestHeader *header = dynamic_cast<CRtspNormalPlayTimeRangeRequestHeader *>(this->requestHeaders->GetItem(index));
        CHECK_POINTER_HRESULT(result, header, result, E_FAIL);

        if (SUCCEEDED(result))
        {
          if (header->IsSetFlags(NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_START))
          {
            header->SetEndTime(endTime);
            header->SetFlags(header->GetFlags() & (~NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_END));
          }
          else
          {
            CHECK_CONDITION_HRESULT(result, this->requestHeaders->Remove(index), result, E_OUTOFMEMORY);
          }
        }
      }
    }
    else if (endTime != TIME_UNSPECIFIED)
    {
      // create new normal play time request header
      CRtspNormalPlayTimeRangeRequestHeader *header = new CRtspNormalPlayTimeRangeRequestHeader(&result);
      CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        header->SetEndTime(endTime);
        header->SetFlags(header->GetFlags() | NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_END);
      }

      CHECK_CONDITION_HRESULT(result, this->requestHeaders->Add(header), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(header));
    }
  }

  return SUCCEEDED(result);
}

/* other methods */

/* protected methods */

bool CRtspPlayRequest::CloneInternal(CRtspRequest *clone)
{
  return __super::CloneInternal(clone);
}

CRtspRequest *CRtspPlayRequest::CreateRequest(void)
{
  HRESULT result = S_OK;
  CRtspPlayRequest *request = new CRtspPlayRequest(&result, false);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}