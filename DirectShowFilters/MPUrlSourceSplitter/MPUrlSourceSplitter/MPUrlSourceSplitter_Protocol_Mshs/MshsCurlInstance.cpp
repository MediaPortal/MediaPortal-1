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

#include "MshsCurlInstance.h"

#pragma warning(pop)

CMshsCurlInstance::CMshsCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CHttpCurlInstance(result, logger, mutex, protocolName, instanceName)
{
  this->owner = NULL;
  this->ownerLockCount = 0;
  this->connectionState = None;

  this->mshsDownloadRequest = dynamic_cast<CMshsDownloadRequest *>(this->downloadRequest);
  this->mshsDownloadResponse = dynamic_cast<CMshsDownloadResponse *>(this->downloadResponse);
}

CMshsCurlInstance::~CMshsCurlInstance(void)
{
  this->StopReceivingData();
}

/* get methods */


CMshsDownloadRequest *CMshsCurlInstance::GetMshsDownloadRequest(void)
{
  return this->mshsDownloadRequest;
}

CMshsDownloadResponse *CMshsCurlInstance::GetMshsDownloadResponse(void)
{
  return this->mshsDownloadResponse;
}

void *CMshsCurlInstance::GetOwner(void)
{
  return this->owner;
}

unsigned int CMshsCurlInstance::GetOwnerLockCount(void)
{
  return this->ownerLockCount;
}

ProtocolConnectionState CMshsCurlInstance::GetConnectionState(void)
{
  return this->connectionState;
}

/* set methods */

void CMshsCurlInstance::SetConnectionState(ProtocolConnectionState connectionState)
{
  this->connectionState = connectionState;
}

/* other methods */

HRESULT CMshsCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->mshsDownloadRequest = dynamic_cast<CMshsDownloadRequest *>(this->downloadRequest);
  this->mshsDownloadResponse = dynamic_cast<CMshsDownloadResponse *>(this->downloadResponse);
  CHECK_POINTER_HRESULT(result, this->mshsDownloadRequest, result, E_NOT_VALID_STATE);
  CHECK_POINTER_HRESULT(result, this->mshsDownloadResponse, result, E_NOT_VALID_STATE);

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

HRESULT CMshsCurlInstance::LockCurlInstance(void *owner)
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

HRESULT CMshsCurlInstance::UnlockCurlInstance(void *owner)
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

bool CMshsCurlInstance::IsLockedCurlInstance(void)
{
  return (this->ownerLockCount != 0);
}

bool CMshsCurlInstance::IsLockedCurlInstanceByOwner(void *owner)
{
  return (this->owner == owner);
}

/* protected methods */

CDownloadResponse *CMshsCurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CMshsDownloadResponse *response = new CMshsDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}