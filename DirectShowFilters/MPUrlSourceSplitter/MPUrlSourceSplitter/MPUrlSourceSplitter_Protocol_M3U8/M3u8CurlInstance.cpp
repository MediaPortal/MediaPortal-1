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

#include "M3u8CurlInstance.h"
#include "M3u8DumpBox.h"

#pragma warning(pop)

CM3u8CurlInstance::CM3u8CurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CHttpCurlInstance(result, logger, mutex, protocolName, instanceName)
{
  this->owner = NULL;
  this->ownerLockCount = 0;
  this->connectionState = None;

  this->m3u8DownloadRequest = dynamic_cast<CM3u8DownloadRequest *>(this->downloadRequest);
  this->m3u8DownloadResponse = dynamic_cast<CM3u8DownloadResponse *>(this->downloadResponse);
}

CM3u8CurlInstance::~CM3u8CurlInstance(void)
{
  this->StopReceivingData();
}

/* get methods */


CM3u8DownloadRequest *CM3u8CurlInstance::GetM3u8DownloadRequest(void)
{
  return this->m3u8DownloadRequest;
}

CM3u8DownloadResponse *CM3u8CurlInstance::GetM3u8DownloadResponse(void)
{
  return this->m3u8DownloadResponse;
}

void *CM3u8CurlInstance::GetOwner(void)
{
  return this->owner;
}

unsigned int CM3u8CurlInstance::GetOwnerLockCount(void)
{
  return this->ownerLockCount;
}

ProtocolConnectionState CM3u8CurlInstance::GetConnectionState(void)
{
  return this->connectionState;
}

/* set methods */

void CM3u8CurlInstance::SetConnectionState(ProtocolConnectionState connectionState)
{
  this->connectionState = connectionState;
}

/* other methods */

HRESULT CM3u8CurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->m3u8DownloadRequest = dynamic_cast<CM3u8DownloadRequest *>(this->downloadRequest);
  this->m3u8DownloadResponse = dynamic_cast<CM3u8DownloadResponse *>(this->downloadResponse);
  CHECK_POINTER_HRESULT(result, this->m3u8DownloadRequest, result, E_NOT_VALID_STATE);
  CHECK_POINTER_HRESULT(result, this->m3u8DownloadResponse, result, E_NOT_VALID_STATE);

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

HRESULT CM3u8CurlInstance::LockCurlInstance(void *owner)
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

HRESULT CM3u8CurlInstance::UnlockCurlInstance(void *owner)
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

bool CM3u8CurlInstance::IsLockedCurlInstance(void)
{
  return (this->ownerLockCount != 0);
}

bool CM3u8CurlInstance::IsLockedCurlInstanceByOwner(void *owner)
{
  return (this->owner == owner);
}

/* protected methods */

CDownloadResponse *CM3u8CurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CM3u8DownloadResponse *response = new CM3u8DownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

CDumpBox *CM3u8CurlInstance::CreateDumpBox(void)
{
  HRESULT result = S_OK;
  CM3u8DumpBox *box = new CM3u8DumpBox(&result);
  CHECK_POINTER_HRESULT(result, box, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(box));
  return box;
}