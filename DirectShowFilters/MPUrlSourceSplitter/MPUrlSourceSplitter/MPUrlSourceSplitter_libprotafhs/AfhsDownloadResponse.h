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

#ifndef __AFHS_DOWNLOAD_RESPONSE_DEFINED
#define __AFHS_DOWNLOAD_RESPONE_DEFINED

#include "HttpDownloadResponse.h"

#define AFHS_DOWNLOAD_RESPONSE_FLAG_NONE                              HTTP_DOWNLOAD_RESPONSE_FLAG_NONE

#define AFHS_DOWNLOAD_RESPONSE_FLAG_LAST                              (HTTP_DOWNLOAD_RESPONSE_FLAG_LAST + 0)

class CAfhsDownloadResponse : public CHttpDownloadResponse
{
public:
  CAfhsDownloadResponse(HRESULT *result);
  virtual ~CAfhsDownloadResponse(void);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // creates download response
  // @return : download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // deeply clones current instance to cloned response
  // @param  clone : cloned response to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadResponse *clone);
};

#endif