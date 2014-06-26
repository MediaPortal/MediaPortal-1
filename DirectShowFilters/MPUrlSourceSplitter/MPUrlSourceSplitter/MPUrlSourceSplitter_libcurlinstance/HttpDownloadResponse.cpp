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

#include "HttpDownloadResponse.h"

CHttpDownloadResponse::CHttpDownloadResponse(HRESULT *result)
  : CDownloadResponse(result)
{
  this->headers = NULL;
  this->responseCode = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->headers = new CHttpHeaderCollection(result);
    CHECK_POINTER_HRESULT(*result, this->headers, *result, E_OUTOFMEMORY);
  }
}

CHttpDownloadResponse::~CHttpDownloadResponse(void)
{
  FREE_MEM_CLASS(this->headers);
}

/* get methods */

CHttpHeaderCollection *CHttpDownloadResponse::GetHeaders(void)
{
  return this->headers;
}

bool CHttpDownloadResponse::GetRangesSupported(void)
{
  return this->IsSetFlags(HTTP_DOWNLOAD_RESPONSE_FLAG_RANGES_SUPPORTED);
}

long CHttpDownloadResponse::GetResponseCode(void)
{
  return this->responseCode;
}

/* set methods */

void CHttpDownloadResponse::SetRangesSupported(bool rangesSupported)
{
  this->flags &= ~HTTP_DOWNLOAD_RESPONSE_FLAG_RANGES_SUPPORTED;
  this->flags = (rangesSupported) ? HTTP_DOWNLOAD_RESPONSE_FLAG_RANGES_SUPPORTED : HTTP_DOWNLOAD_RESPONSE_FLAG_NONE;
}

void CHttpDownloadResponse::SetResponseCode(long responseCode)
{
  this->responseCode = responseCode;
}

/* other methods */

/* protected methods */

CDownloadResponse *CHttpDownloadResponse::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CHttpDownloadResponse *response = new CHttpDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

bool CHttpDownloadResponse::CloneInternal(CDownloadResponse *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CHttpDownloadResponse *response = dynamic_cast<CHttpDownloadResponse *>(clone);

    response->responseCode = this->responseCode;
    response->headers->Clear();
    result &= response->headers->Append(this->headers);
  }
  return result;
}