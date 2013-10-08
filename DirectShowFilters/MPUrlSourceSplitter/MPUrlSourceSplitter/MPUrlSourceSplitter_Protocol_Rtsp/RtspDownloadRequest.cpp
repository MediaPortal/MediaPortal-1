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

#include "RtspDownloadRequest.h"

CRtspDownloadRequest::CRtspDownloadRequest(void)
  : CDownloadRequest()
{
  this->startTime = 0;
}

CRtspDownloadRequest::~CRtspDownloadRequest(void)
{
}

/* get methods */

uint64_t CRtspDownloadRequest::GetStartTime(void)
{
  return this->startTime;
}

/* set methods */

void CRtspDownloadRequest::SetStartTime(uint64_t startTime)
{
  this->startTime = startTime;
}

/* other methods */

CRtspDownloadRequest *CRtspDownloadRequest::Clone(void)
{
  CRtspDownloadRequest *result = new CRtspDownloadRequest();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CRtspDownloadRequest::CloneInternal(CRtspDownloadRequest *clonedRequest)
{
  bool result = __super::CloneInternal(clonedRequest);

  if (result)
  {
    clonedRequest->startTime = this->startTime;
  }

  return result;
}