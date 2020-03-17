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

#pragma once

#ifndef __M3U8_DOWNLOAD_REQUEST_DEFINED
#define __M3U8_DOWNLOAD_REQUEST_DEFINED

#include "HttpDownloadRequest.h"

#define M3U8_DOWNLOAD_REQUEST_FLAG_NONE                               HTTP_DOWNLOAD_REQUEST_FLAG_NONE

#define M3U8_DOWNLOAD_REQUEST_FLAG_LAST                               (HTTP_DOWNLOAD_REQUEST_FLAG_LAST + 0)

class CM3u8DownloadRequest : public CHttpDownloadRequest
{
public:
  CM3u8DownloadRequest(HRESULT *result);
  virtual ~CM3u8DownloadRequest(void);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // creates empty download request
  // @return : download request or NULL if error
  virtual CDownloadRequest *CreateDownloadRequest(void);

  // deeply clones current instance to cloned request
  // @param  clone : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadRequest *clone);
};

#endif