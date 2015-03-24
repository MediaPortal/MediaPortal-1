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

#include "StreamPackagePacketRequest.h"

CStreamPackagePacketRequest::CStreamPackagePacketRequest(HRESULT *result)
  : CStreamPackageRequest(result)
{
}

CStreamPackagePacketRequest::~CStreamPackagePacketRequest(void)
{
}

/* get methods */

/* set methods */

void CStreamPackagePacketRequest::SetResetPacketCounter(bool resetPacketCounter)
{
  this->flags &= ~STREAM_PACKAGE_PACKET_REQUEST_FLAG_RESET_PACKET_COUNTER;
  this->flags |= (resetPacketCounter) ? STREAM_PACKAGE_PACKET_REQUEST_FLAG_RESET_PACKET_COUNTER : STREAM_PACKAGE_PACKET_REQUEST_FLAG_NONE;
}

/* other methods */

bool CStreamPackagePacketRequest::IsResetPacketCounter(void)
{
  return this->IsSetFlags(STREAM_PACKAGE_PACKET_REQUEST_FLAG_RESET_PACKET_COUNTER);
}

/* protected methods */

CStreamPackageRequest *CStreamPackagePacketRequest::CreatePackageRequest(void)
{
  HRESULT result = S_OK;
  CStreamPackagePacketRequest *packageRequest = new CStreamPackagePacketRequest(&result);
  CHECK_POINTER_HRESULT(result, packageRequest, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packageRequest));
  return packageRequest;
}

bool CStreamPackagePacketRequest::InternalClone(CStreamPackageRequest *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CStreamPackagePacketRequest *clone = dynamic_cast<CStreamPackagePacketRequest *>(item);
    result &= (clone != NULL);

    if (result)
    {
    }
  }

  return result;
}