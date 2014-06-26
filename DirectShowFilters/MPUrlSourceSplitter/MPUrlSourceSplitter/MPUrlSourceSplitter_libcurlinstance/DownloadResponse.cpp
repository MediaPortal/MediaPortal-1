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

#include "DownloadResponse.h"

CDownloadResponse::CDownloadResponse(HRESULT *result)
  : CFlags()
{
  this->resultError = S_OK;
  this->receivedData = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->receivedData = new CLinearBuffer(result);
    CHECK_POINTER_HRESULT(*result, this->receivedData, *result, E_OUTOFMEMORY);
  }
}

CDownloadResponse::~CDownloadResponse(void)
{
  FREE_MEM_CLASS(this->receivedData);
}

/* get methods */

CLinearBuffer *CDownloadResponse::GetReceivedData(void)
{
  return this->receivedData;
}

HRESULT CDownloadResponse::GetResultError(void)
{
  return this->resultError;
}

/* set methods */

void CDownloadResponse::SetResultError(HRESULT resultError)
{
  this->resultError = resultError;
}

/* other methods */

CDownloadResponse *CDownloadResponse::Clone(void)
{
  HRESULT result = S_OK;
  CDownloadResponse *clone = this->CreateDownloadResponse();
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  CHECK_CONDITION_HRESULT(result, this->CloneInternal(clone), result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}

/* protected methods */

CDownloadResponse *CDownloadResponse::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CDownloadResponse *response = new CDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

bool CDownloadResponse::CloneInternal(CDownloadResponse *clone)
{
  bool result = (clone != NULL);

  if (result)
  {
    clone->flags = this->flags;
    clone->resultError = this->resultError;
    FREE_MEM_CLASS(clone->receivedData);
    clone->receivedData = this->receivedData->Clone();

    result &= (clone->receivedData != NULL);
  }

  return result;
}