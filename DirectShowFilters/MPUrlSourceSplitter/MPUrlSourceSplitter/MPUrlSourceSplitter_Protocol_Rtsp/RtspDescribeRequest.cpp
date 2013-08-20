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

#include "RtspDescribeRequest.h"
#include "RtspAcceptApplicationSdpRequestHeader.h"

CRtspDescribeRequest::CRtspDescribeRequest(void)
  : CRtspRequest()
{
  CRtspAcceptApplicationSdpRequestHeader *header = new CRtspAcceptApplicationSdpRequestHeader();
  if (header != NULL)
  {
    if (!this->requestHeaders->Add(header))
    {
      FREE_MEM_CLASS(header);
    }
  }
}

CRtspDescribeRequest::CRtspDescribeRequest(bool createDefaultHeaders)
  : CRtspRequest(createDefaultHeaders)
{
  if (createDefaultHeaders)
  {
    CRtspAcceptApplicationSdpRequestHeader *header = new CRtspAcceptApplicationSdpRequestHeader();
    if (header != NULL)
    {
      if (!this->requestHeaders->Add(header))
      {
        FREE_MEM_CLASS(header);
      }
    }
  }
}

CRtspDescribeRequest::~CRtspDescribeRequest(void)
{
}

/* get methods */

const wchar_t *CRtspDescribeRequest::GetMethod(void)
{
  return RTSP_DESCRIBE_METHOD;
}

/* set methods */

/* other methods */

CRtspDescribeRequest *CRtspDescribeRequest::Clone(void)
{
  return (CRtspDescribeRequest *)__super::Clone();
}

bool CRtspDescribeRequest::CloneInternal(CRtspRequest *clonedRequest)
{
  return __super::CloneInternal(clonedRequest);
}

CRtspRequest *CRtspDescribeRequest::GetNewRequest(void)
{
  return new CRtspDescribeRequest(false);
}