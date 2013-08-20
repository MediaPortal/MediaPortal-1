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

CHttpDownloadResponse::CHttpDownloadResponse(void)
  : CDownloadResponse()
{
  this->headers = new CHttpHeaderCollection();
  this->supportedRanges = true;
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
  return this->supportedRanges;
}

/* set methods */

void CHttpDownloadResponse::SetRangesSupported(bool rangesSupported)
{
  this->supportedRanges = rangesSupported;
}

/* other methods */

CDownloadResponse *CHttpDownloadResponse::Clone(void)
{
  CHttpDownloadResponse *result = new CHttpDownloadResponse();
  if (result != NULL)
  {
    if (!this->CloneInternal(result))
    {
      FREE_MEM_CLASS(result);
    }
  }
  return result;
}

bool CHttpDownloadResponse::CloneInternal(CHttpDownloadResponse *clonedResponse)
{
  bool result = __super::CloneInternal(clonedResponse);
  if (result)
  {
    clonedResponse->headers->Clear();
    result &= clonedResponse->headers->Append(this->headers);
  }
  return result;
}