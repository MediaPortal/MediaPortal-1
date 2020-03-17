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

#include "StreamPackageDataRequest.h"

CStreamPackageDataRequest::CStreamPackageDataRequest(HRESULT *result)
  : CStreamPackageRequest(result)
{
  this->start = 0;
  this->length = 0;
}

CStreamPackageDataRequest::~CStreamPackageDataRequest(void)
{
}

/* get methods */

int64_t CStreamPackageDataRequest::GetStart(void)
{
  return this->start;
}

unsigned int CStreamPackageDataRequest::GetLength(void)
{
  return this->length;
}

/* set methods */

void CStreamPackageDataRequest::SetStart(int64_t start)
{
  this->start = start;
}

void CStreamPackageDataRequest::SetLength(unsigned int length)
{
  this->length = length;
}

void CStreamPackageDataRequest::SetAnyDataLength(bool anyDataLength)
{
  this->flags &= ~STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH;
  this->flags |= anyDataLength ? STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH : STREAM_PACKAGE_DATA_REQUEST_FLAG_NONE;
}

void CStreamPackageDataRequest::SetAnyNonZeroDataLength(bool anyNonZeroDataLength)
{
  this->flags &= ~STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH;
  this->flags |= anyNonZeroDataLength ? STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH : STREAM_PACKAGE_DATA_REQUEST_FLAG_NONE;
}

/* other methods */

bool CStreamPackageDataRequest::IsSetAnyDataLength(void)
{
  return this->IsSetFlags(STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_DATA_LENGTH);
}

bool CStreamPackageDataRequest::IsSetAnyNonZeroDataLength(void)
{
  return this->IsSetFlags(STREAM_PACKAGE_DATA_REQUEST_FLAG_ANY_NONZERO_DATA_LENGTH);
}

/* protected methods */

CStreamPackageRequest *CStreamPackageDataRequest::CreatePackageRequest(void)
{
  HRESULT result = S_OK;
  CStreamPackageDataRequest *packageRequest = new CStreamPackageDataRequest(&result);
  CHECK_POINTER_HRESULT(result, packageRequest, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packageRequest));
  return packageRequest;
}

bool CStreamPackageDataRequest::InternalClone(CStreamPackageRequest *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CStreamPackageDataRequest *clone = dynamic_cast<CStreamPackageDataRequest *>(item);
    result &= (clone != NULL);

    if (result)
    {
      clone->start = this->start;
      clone->length = this->length;
    }
  }

  return result;
}