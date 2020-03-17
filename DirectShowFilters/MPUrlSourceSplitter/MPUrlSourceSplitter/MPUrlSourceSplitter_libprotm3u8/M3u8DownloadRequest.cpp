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

#include "M3u8DownloadRequest.h"

CM3u8DownloadRequest::CM3u8DownloadRequest(HRESULT *result)
  : CHttpDownloadRequest(result)
{
  /*if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }*/
}

CM3u8DownloadRequest::~CM3u8DownloadRequest(void)
{
}

/* get methods */

/* set methods */

/* other methods */

/* protected methods */

CDownloadRequest *CM3u8DownloadRequest::CreateDownloadRequest(void)
{
  HRESULT result = S_OK;
  CM3u8DownloadRequest *request = new CM3u8DownloadRequest(&result);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}

bool CM3u8DownloadRequest::CloneInternal(CDownloadRequest *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CM3u8DownloadRequest *request = dynamic_cast<CM3u8DownloadRequest *>(clone);
  }

  return result;
}