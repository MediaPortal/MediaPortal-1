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

#include "StreamPackageResponse.h"

CStreamPackageResponse::CStreamPackageResponse(HRESULT *result)
  : CFlags()
{
}

CStreamPackageResponse::~CStreamPackageResponse(void)
{
}

/* get methods */

/* set methods */

void CStreamPackageResponse::SetDiscontinuity(bool discontinuity)
{
  this->flags &= ~STREAM_PACKAGE_RESPONSE_FLAG_DISCONTINUITY;
  this->flags |= discontinuity ? STREAM_PACKAGE_RESPONSE_FLAG_DISCONTINUITY : STREAM_PACKAGE_RESPONSE_FLAG_NONE;
}

void CStreamPackageResponse::SetNoMoreDataAvailable(bool noMoreDataAvailable)
{
  this->flags &= ~STREAM_PACKAGE_RESPONSE_FLAG_NO_MORE_DATA_AVAILABLE;
  this->flags |= noMoreDataAvailable ? STREAM_PACKAGE_RESPONSE_FLAG_NO_MORE_DATA_AVAILABLE : STREAM_PACKAGE_RESPONSE_FLAG_NONE;
}

void CStreamPackageResponse::SetConnectionLostCannotReopen(bool connectionLostCannotReopen)
{
  this->flags &= ~STREAM_PACKAGE_RESPONSE_FLAG_CONNECTION_LOST_CANNOT_REOPEN;
  this->flags |= connectionLostCannotReopen ? STREAM_PACKAGE_RESPONSE_FLAG_CONNECTION_LOST_CANNOT_REOPEN : STREAM_PACKAGE_RESPONSE_FLAG_NONE;
}

/* other methods */

bool CStreamPackageResponse::IsDiscontinuity(void)
{
  return this->IsSetFlags(STREAM_PACKAGE_RESPONSE_FLAG_DISCONTINUITY);
}

bool CStreamPackageResponse::IsNoMoreDataAvailable(void)
{
  return this->IsSetFlags(STREAM_PACKAGE_RESPONSE_FLAG_NO_MORE_DATA_AVAILABLE);
}

bool CStreamPackageResponse::IsConnectionLostCannotReopen(void)
{
  return this->IsSetFlags(STREAM_PACKAGE_RESPONSE_FLAG_CONNECTION_LOST_CANNOT_REOPEN);
}

CStreamPackageResponse *CStreamPackageResponse::Clone(void)
{
  CStreamPackageResponse *result = this->CreatePackageResponse();

  if (result != NULL)
  {
    if (!this->InternalClone(result))
    {
      FREE_MEM_CLASS(result);
    }
  }

  return result;
}

/* protected methods */

bool CStreamPackageResponse::InternalClone(CStreamPackageResponse *item)
{
  bool result = (item != NULL);

  if (result)
  {
    item->flags = this->flags;
  }

  return result;
}