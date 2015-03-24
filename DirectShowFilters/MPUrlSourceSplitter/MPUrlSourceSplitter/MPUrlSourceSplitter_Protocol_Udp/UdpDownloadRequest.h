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

#ifndef __UDP_DOWNLOAD_REQUEST_DEFINED
#define __UDP_DOWNLOAD_REQUEST_DEFINED

#include "DownloadRequest.h"

#define UDP_DOWNLOAD_REQUEST_FLAG_NONE                                DOWNLOAD_REQUEST_FLAG_NONE

#define UDP_DOWNLOAD_REQUEST_FLAG_LAST                                (DOWNLOAD_REQUEST_FLAG_LAST + 0)


class CUdpDownloadRequest : public CDownloadRequest
{
public:
  CUdpDownloadRequest(HRESULT *result);
  virtual ~CUdpDownloadRequest(void);

  /* get methods */

  // gets check interval for incoming data (in ms)
  // @return : check interval for incoming data (in ms)
  virtual unsigned int GetCheckInterval(void);

  /* set methods */

  // sets receive data check interval (in ms)
  // @param checkInterval : the check interval for received data (in ms)
  virtual void SetCheckInterval(unsigned int checkInterval);

  /* other methods */

protected:

  // holds check interval for incoming data
  unsigned int checkInterval;

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