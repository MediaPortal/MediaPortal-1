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

#ifndef __UDP_DOWNLOAD_RESPONSE_DEFINED
#define __UDP_DOWNLOAD_RESPONE_DEFINED

#include "DownloadResponse.h"

#define UDP_DOWNLOAD_RESPONSE_FLAG_NONE                               DOWNLOAD_RESPONSE_FLAG_NONE

#define UDP_DOWNLOAD_RESPONSE_FLAG_LAST                               (DOWNLOAD_RESPONSE_FLAG_LAST + 1)


class CUdpDownloadResponse : public CDownloadResponse
{
public:
  CUdpDownloadResponse(HRESULT *result);
  virtual ~CUdpDownloadResponse(void);

  /* get methods */

  /* set methods */

  /* other methods */

protected:

  /* methods */

  // creates download response
  // @return : download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // deeply clones current instance to cloned request
  // @param  clone : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadResponse *clone);
};

#endif