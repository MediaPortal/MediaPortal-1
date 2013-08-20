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

#include "DownloadRequest.h"

CDownloadRequest::CDownloadRequest(void)
{
  this->url = NULL;
}

CDownloadRequest::~CDownloadRequest(void)
{
  FREE_MEM(this->url);
}

/* get methods */

const wchar_t *CDownloadRequest::GetUrl(void)
{
  return this->url;
}

/* set methods */

bool CDownloadRequest::SetUrl(const wchar_t *url)
{
  SET_STRING_RETURN_WITH_NULL(this->url, url);
}

/* other methods*/

CDownloadRequest *CDownloadRequest::Clone(void)
{
  CDownloadRequest *result = new CDownloadRequest();

  if (!this->CloneInternal(result))
  {
    FREE_MEM_CLASS(result);
  }

  return result;
}

bool CDownloadRequest::CloneInternal(CDownloadRequest *clonedRequest)
{
  bool result = false;
  if (clonedRequest != NULL)
  {
    clonedRequest->url = Duplicate(this->url);
    result = TEST_STRING_WITH_NULL(clonedRequest->url, this->url);
  }
  return result;
}