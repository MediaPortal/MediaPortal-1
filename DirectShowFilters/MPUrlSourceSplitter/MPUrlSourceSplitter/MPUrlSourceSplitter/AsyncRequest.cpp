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

#include "AsyncRequest.h"
#include "Utilities.h"

CAsyncRequest::CAsyncRequest(void)
{
  this->requestId = 0;
  this->position = 0;
  this->length = 0;
  this->buffer = NULL;
  this->requestState = CAsyncRequest::Created;
  this->errorCode = S_OK;
  this->userData = NULL;
}

CAsyncRequest::~CAsyncRequest(void)
{
}

HRESULT CAsyncRequest::Request(unsigned int requestId, int64_t position, LONG length, BYTE *buffer, DWORD_PTR userData)
{
  this->requestId = requestId;
  this->position = position;
  this->length = length;
  this->buffer = buffer;
  this->requestState = CAsyncRequest::Waiting;
  this->errorCode = S_OK;
  this->userData = userData;

  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->buffer);

  return result;
}

LONG CAsyncRequest::GetBufferLength(void)
{
  return this->length;
}

void CAsyncRequest::SetBufferLength(LONG length)
{
  this->length = length;
}

int64_t CAsyncRequest::GetStart(void)
{
  return this->position;
}

void CAsyncRequest::Complete(HRESULT errorCode)
{
  this->requestState = CAsyncRequest::Completed;
  this->errorCode = errorCode;
}

void CAsyncRequest::WaitAndIgnoreTimeout(void)
{
  this->requestState = CAsyncRequest::WaitingIgnoreTimeout;
}

CAsyncRequest::AsyncState CAsyncRequest::GetState(void)
{
  return this->requestState;
}

HRESULT CAsyncRequest::GetErrorCode(void)
{
  return this->errorCode;
}

DWORD_PTR CAsyncRequest::GetUserData(void)
{
  return this->userData;
}

unsigned int CAsyncRequest::GetRequestId(void)
{
  return this->requestId;
}

BYTE *CAsyncRequest::GetBuffer(void)
{
  return this->buffer;
}