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

#include "M3u8DownloadResponse.h"

CM3u8DownloadResponse::CM3u8DownloadResponse(HRESULT *result)
  : CHttpDownloadResponse(result)
{
  /*if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }*/
}

CM3u8DownloadResponse::~CM3u8DownloadResponse(void)
{
}

/* get methods */

/* set methods */

/* other methods */

/* protected methods */

CDownloadResponse *CM3u8DownloadResponse::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CM3u8DownloadResponse *response = new CM3u8DownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

bool CM3u8DownloadResponse::CloneInternal(CDownloadResponse *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CM3u8DownloadResponse *response = dynamic_cast<CM3u8DownloadResponse *>(clone);
  }

  return result;
}