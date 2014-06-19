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

#include "StreamPackage.h"

CStreamPackage::CStreamPackage(HRESULT *result)
{
  this->state = Invalid;
  this->request = NULL;
  this->response = NULL;
  this->errorCode = S_OK;
}

CStreamPackage::~CStreamPackage(void)
{
  FREE_MEM_CLASS(this->request);
  FREE_MEM_CLASS(this->response);
}

/* get methods */

CStreamPackage::ProcessingState CStreamPackage::GetState(void)
{
  return this->state;
}

CStreamPackageRequest *CStreamPackage::GetRequest(void)
{
  return this->request;
}

CStreamPackageResponse *CStreamPackage::GetResponse(void)
{
  return this->response;
}

HRESULT CStreamPackage::GetError(void)
{
  return this->errorCode;
}

/* set methods */

void CStreamPackage::SetRequest(CStreamPackageRequest *request)
{
  FREE_MEM_CLASS(this->request);
  this->request = request;
  this->state = (this->request != NULL) ? Created : Invalid;
}

void CStreamPackage::SetResponse(CStreamPackageResponse *response)
{
  FREE_MEM_CLASS(this->response);
  this->response = response;
}

void CStreamPackage::SetCompleted(HRESULT error)
{
  CHECK_CONDITION_EXECUTE(this->state != Invalid, this->state = Completed);
  this->errorCode = error;
}

void CStreamPackage::SetWaiting(void)
{
  CHECK_CONDITION_EXECUTE(this->state != Invalid, this->state = Waiting);
}

void CStreamPackage::SetWaitingIgnoreTimeout(void)
{
  CHECK_CONDITION_EXECUTE(this->state != Invalid, this->state = WaitingIgnoreTimeout);
}

/* other methods */

void CStreamPackage::Clear(void)
{
  this->state = Invalid;
  FREE_MEM_CLASS(this->request);
  FREE_MEM_CLASS(this->response);
  this->errorCode = S_OK;
}

bool CStreamPackage::IsError(void)
{
  return SUCCEEDED(this->GetError());
}

CStreamPackage *CStreamPackage::Clone(void)
{
  HRESULT result = S_OK;
  CStreamPackage *clone = new CStreamPackage(&result);
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    clone->state = this->state;
    clone->request = (this->request != NULL) ? this->request->Clone() : NULL;
    clone->response = (this->response != NULL) ? this->response->Clone() : NULL;
    clone->errorCode = this->errorCode;

    CHECK_CONDITION_HRESULT(result, (this->request != NULL) && (clone->request == NULL), E_OUTOFMEMORY, result);
    CHECK_CONDITION_HRESULT(result, (this->response != NULL) && (clone->response == NULL), E_OUTOFMEMORY, result);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));

  return clone;
}

/* protected methods */