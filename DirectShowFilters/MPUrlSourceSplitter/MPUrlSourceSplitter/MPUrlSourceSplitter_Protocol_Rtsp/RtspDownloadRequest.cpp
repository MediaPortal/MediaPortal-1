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

CRtspDownloadRequest::CRtspDownloadRequest(HRESULT *result)
  : CDownloadRequest(result)
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

/* protected methods */

CDownloadRequest *CRtspDownloadRequest::CreateDownloadRequest(void)
{
  HRESULT result = S_OK;
  CRtspDownloadRequest *request = new CRtspDownloadRequest(&result);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}

bool CRtspDownloadRequest::CloneInternal(CDownloadRequest *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CRtspDownloadRequest *request = dynamic_cast<CRtspDownloadRequest *>(clone);

    request->startTime = this->startTime;
  }

  return result;
}