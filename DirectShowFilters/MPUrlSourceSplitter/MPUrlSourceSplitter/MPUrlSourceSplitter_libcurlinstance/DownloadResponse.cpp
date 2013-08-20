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

CDownloadResponse::CDownloadResponse(void)
{
  this->receivedData = new CLinearBuffer();
  this->resultCode = CURLE_OK;
  this->responseCode = 0;
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

CURLcode CDownloadResponse::GetResultCode(void)
{
  return this->resultCode;
}

long CDownloadResponse::GetResponseCode(void)
{
  return this->responseCode;
}

/* set methods */

void CDownloadResponse::SetResultCode(CURLcode resultCode)
{
  this->resultCode = resultCode;
}

void CDownloadResponse::SetResponseCode(long responseCode)
{
  this->responseCode = responseCode;
}

/* other methods */

CDownloadResponse *CDownloadResponse::Clone(void)
{
  CDownloadResponse *result = new CDownloadResponse();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CDownloadResponse::CloneInternal(CDownloadResponse *clonedResponse)
{
  clonedResponse->resultCode = this->resultCode;
  clonedResponse->responseCode = this->responseCode;
  FREE_MEM_CLASS(clonedResponse->receivedData);
  clonedResponse->receivedData = this->receivedData->Clone();

  return (clonedResponse->receivedData != NULL);
}