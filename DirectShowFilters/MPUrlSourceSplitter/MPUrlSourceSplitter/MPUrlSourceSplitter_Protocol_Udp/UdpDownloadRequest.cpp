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

#include "UdpDownloadRequest.h"

CUdpDownloadRequest::CUdpDownloadRequest(HRESULT *result)
  : CDownloadRequest(result)
{
  this->checkInterval = 0;
}

CUdpDownloadRequest::~CUdpDownloadRequest(void)
{
}

/* get methods */

unsigned int CUdpDownloadRequest::GetCheckInterval(void)
{
  return this->checkInterval;
}

/* set methods */

void CUdpDownloadRequest::SetCheckInterval(unsigned int checkInterval)
{
  this->checkInterval = checkInterval;
}

/* other methods */

/* protected methods */

CDownloadRequest *CUdpDownloadRequest::CreateDownloadRequest(void)
{
  HRESULT result = S_OK;
  CUdpDownloadRequest *request = new CUdpDownloadRequest(&result);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}

bool CUdpDownloadRequest::CloneInternal(CDownloadRequest *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CUdpDownloadRequest *request = dynamic_cast<CUdpDownloadRequest *>(clone);
    result &= (request != NULL);

    if (result)
    {
      request->checkInterval = this->checkInterval;
    }
  }

  return result;
}