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

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "AfhsCurlInstance.h"

#pragma warning(pop)

CAfhsCurlInstance::CAfhsCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CHttpCurlInstance(result, logger, mutex, protocolName, instanceName)
{
  this->owner = NULL;
  this->ownerLockCount = 0;
  this->connectionState = None;

  this->afhsDownloadRequest = dynamic_cast<CAfhsDownloadRequest *>(this->downloadRequest);
  this->afhsDownloadResponse = dynamic_cast<CAfhsDownloadResponse *>(this->downloadResponse);
}

CAfhsCurlInstance::~CAfhsCurlInstance(void)
{
  this->StopReceivingData();
}

/* get methods */


CAfhsDownloadRequest *CAfhsCurlInstance::GetAfhsDownloadRequest(void)
{
  return this->afhsDownloadRequest;
}

CAfhsDownloadResponse *CAfhsCurlInstance::GetAfhsDownloadResponse(void)
{
  return this->afhsDownloadResponse;
}

void *CAfhsCurlInstance::GetOwner(void)
{
  return this->owner;
}

unsigned int CAfhsCurlInstance::GetOwnerLockCount(void)
{
  return this->ownerLockCount;
}

ProtocolConnectionState CAfhsCurlInstance::GetConnectionState(void)
{
  return this->connectionState;
}

/* set methods */

void CAfhsCurlInstance::SetConnectionState(ProtocolConnectionState connectionState)
{
  this->connectionState = connectionState;
}

/* other methods */

HRESULT CAfhsCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->afhsDownloadRequest = dynamic_cast<CAfhsDownloadRequest *>(this->downloadRequest);
  this->afhsDownloadResponse = dynamic_cast<CAfhsDownloadResponse *>(this->downloadResponse);
  CHECK_POINTER_HRESULT(result, this->afhsDownloadRequest, result, E_NOT_VALID_STATE);
  CHECK_POINTER_HRESULT(result, this->afhsDownloadResponse, result, E_NOT_VALID_STATE);

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

HRESULT CAfhsCurlInstance::LockCurlInstance(void *owner)
{
  HRESULT result = E_FAIL;

  if ((this->ownerLockCount == 0) || (this->owner == owner))
  {
    result = (this->ownerLockCount == 0) ? S_OK : S_FALSE;
    this->ownerLockCount++;
  }

  // remember owner
  if (this->ownerLockCount > 0)
  {
    this->owner = owner;
  }

  return result;
}

HRESULT CAfhsCurlInstance::UnlockCurlInstance(void *owner)
{
  HRESULT result = E_FAIL;

  if ((this->ownerLockCount > 0) && (this->owner == owner))
  {
    result = (this->ownerLockCount == 1) ? S_OK : S_FALSE;
    this->ownerLockCount--;
  }

  // reset owner if finally unlocked
  if (this->ownerLockCount == 0)
  {
    // finally unlocked
    this->owner = NULL;
  }

  return result;
}

bool CAfhsCurlInstance::IsLockedCurlInstance(void)
{
  return (this->ownerLockCount != 0);
}

bool CAfhsCurlInstance::IsLockedCurlInstanceByOwner(void *owner)
{
  return (this->owner == owner);
}

/* protected methods */

CDownloadResponse *CAfhsCurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CAfhsDownloadResponse *response = new CAfhsDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}