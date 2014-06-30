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

#include "DownloadRequest.h"

CDownloadRequest::CDownloadRequest(HRESULT *result)
  : CFlags()
{
  this->url = NULL;
  this->receiveDataTimeout = UINT_MAX;
  this->networkInterfaceName = NULL;
  this->finishTime = FINISH_TIME_NOT_SPECIFIED;
}

CDownloadRequest::~CDownloadRequest(void)
{
  FREE_MEM(this->url);
  FREE_MEM(this->networkInterfaceName);
}

/* get methods */

const wchar_t *CDownloadRequest::GetUrl(void)
{
  return this->url;
}

unsigned int CDownloadRequest::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

const wchar_t *CDownloadRequest::GetNetworkInterfaceName(void)
{
  return this->networkInterfaceName;
}

unsigned int CDownloadRequest::GetFinishTime(void)
{
  return this->finishTime;
}

/* set methods */

bool CDownloadRequest::SetUrl(const wchar_t *url)
{
  SET_STRING_RETURN_WITH_NULL(this->url, url);
}

void CDownloadRequest::SetReceivedDataTimeout(unsigned int timeout)
{
  this->receiveDataTimeout = timeout;
}

bool CDownloadRequest::SetNetworkInterfaceName(const wchar_t *networkInterfaceName)
{
  SET_STRING_RETURN_WITH_NULL(this->networkInterfaceName, networkInterfaceName);
}

void CDownloadRequest::SetFinishTime(unsigned int finishTime)
{
  this->finishTime = finishTime;
}

/* other methods*/

CDownloadRequest *CDownloadRequest::Clone(void)
{
  HRESULT result = S_OK;
  CDownloadRequest *clone = this->CreateDownloadRequest();
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, this->CloneInternal(clone), result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}

/* protected methods */

CDownloadRequest *CDownloadRequest::CreateDownloadRequest(void)
{
  HRESULT result = S_OK;
  CDownloadRequest *request = new CDownloadRequest(&result);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}

bool CDownloadRequest::CloneInternal(CDownloadRequest *clone)
{
  bool result = (clone != NULL);

  if (result)
  {
    clone->flags = this->flags;
    clone->url = Duplicate(this->url);
    clone->finishTime = this->finishTime;
    clone->receiveDataTimeout = this->receiveDataTimeout;

    SET_STRING_AND_RESULT_WITH_NULL(clone->networkInterfaceName, this->networkInterfaceName, result);
    SET_STRING_AND_RESULT_WITH_NULL(clone->url, this->url, result);
  }

  return result;
}