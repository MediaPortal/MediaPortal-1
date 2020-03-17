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

#include "RtmpDownloadResponse.h"

CRtmpDownloadResponse::CRtmpDownloadResponse(HRESULT *result)
  : CDownloadResponse(result)
{
  this->duration = RTMP_DURATION_UNSPECIFIED;
}

CRtmpDownloadResponse::~CRtmpDownloadResponse(void)
{
}

/* get methods */

uint64_t CRtmpDownloadResponse::GetDuration(void)
{
  return this->duration;
}

/* set methods */

void CRtmpDownloadResponse::SetDuration(uint64_t duration)
{
  this->duration = duration;
}

/* other methods */

/* protected methods */

CDownloadResponse *CRtmpDownloadResponse::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CRtmpDownloadResponse *response = new CRtmpDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

bool CRtmpDownloadResponse::CloneInternal(CDownloadResponse *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CRtmpDownloadResponse *response = dynamic_cast<CRtmpDownloadResponse *>(clone);

    response->duration = this->duration;
  }

  return result;
}