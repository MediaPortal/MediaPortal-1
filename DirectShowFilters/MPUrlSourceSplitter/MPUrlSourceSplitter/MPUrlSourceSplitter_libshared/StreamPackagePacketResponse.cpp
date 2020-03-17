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

#include "StreamPackagePacketResponse.h"

CStreamPackagePacketResponse::CStreamPackagePacketResponse(HRESULT *result)
  : CStreamPackageResponse(result)
{
  this->mediaPacket = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->mediaPacket = new CMediaPacket(result);
    CHECK_POINTER_HRESULT(*result, this->mediaPacket, *result, E_OUTOFMEMORY);
  }
}

CStreamPackagePacketResponse::~CStreamPackagePacketResponse(void)
{
  FREE_MEM_CLASS(this->mediaPacket);
}

/* get methods */

CMediaPacket *CStreamPackagePacketResponse::GetMediaPacket(void)
{
  return this->mediaPacket;
}

/* set methods */

/* other methods */

/* protected methods */

CStreamPackageResponse *CStreamPackagePacketResponse::CreatePackageResponse(void)
{
  HRESULT result = S_OK;
  CStreamPackagePacketResponse *response = new CStreamPackagePacketResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

bool CStreamPackagePacketResponse::InternalClone(CStreamPackageResponse *item)
{
  bool result = __super::InternalClone(item);

  if (result)
  {
    CStreamPackagePacketResponse *response = dynamic_cast<CStreamPackagePacketResponse *>(item);
    result &= (response != NULL);

    if (result)
    {
      FREE_MEM_CLASS(response->mediaPacket);
      response->mediaPacket = (CMediaPacket *)((this->mediaPacket != NULL) ? this->mediaPacket->Clone() : NULL);

      result &= ((this->mediaPacket == NULL) || ((this->mediaPacket != NULL) && (response->mediaPacket != NULL)));
    }
  }

  return result;
}