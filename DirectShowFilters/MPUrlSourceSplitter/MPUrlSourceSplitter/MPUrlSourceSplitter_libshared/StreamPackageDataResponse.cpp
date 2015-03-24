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

#include "StreamPackageDataResponse.h"

CStreamPackageDataResponse::CStreamPackageDataResponse(HRESULT *result)
  : CStreamPackageResponse(result)
{
  this->buffer = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->buffer = new CLinearBuffer(result);
    CHECK_POINTER_HRESULT(*result, this->buffer, *result, E_OUTOFMEMORY);
  }
}

CStreamPackageDataResponse::~CStreamPackageDataResponse(void)
{
  FREE_MEM_CLASS(this->buffer);
}

/* get methods */

CLinearBuffer *CStreamPackageDataResponse::GetBuffer(void)
{
  return this->buffer;
}

/* set methods */

/* other methods */

/* protected methods */

CStreamPackageResponse *CStreamPackageDataResponse::CreatePackageResponse(void)
{
  HRESULT result = S_OK;
  CStreamPackageDataResponse *response = new CStreamPackageDataResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

bool CStreamPackageDataResponse::InternalClone(CStreamPackageResponse *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(item);
    result &= (response != NULL);

    if (result)
    {
      result &= (response->buffer->AddToBufferWithResize(this->buffer) == this->buffer->GetBufferOccupiedSpace());
    }
  }

  return result;
}