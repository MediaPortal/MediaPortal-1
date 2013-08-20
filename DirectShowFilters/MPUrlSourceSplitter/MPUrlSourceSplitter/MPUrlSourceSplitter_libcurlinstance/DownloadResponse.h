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

#ifndef __DOWNLOAD_RESPONSE_DEFINED
#define __DOWNLOAD_RESPONSE_DEFINED

#include "LinearBuffer.h"

#include <curl/curl.h>

class CDownloadResponse
{
public:
  CDownloadResponse(void);
  virtual ~CDownloadResponse(void);

  /* get methods */

  // gets received data
  // @return : received data
  virtual CLinearBuffer *GetReceivedData(void);

  // gets CURL result code
  // @return : CURL result code
  virtual CURLcode GetResultCode(void);

  // gets response code
  // @return : response code
  virtual long GetResponseCode(void);

  /* set methods */

  // sets CURL result code
  // @param resultCode : CURL result code to set
  virtual void SetResultCode(CURLcode resultCode);

  // sets response code
  // @param responseCode : response code to set
  virtual void SetResponseCode(long responseCode);

  /* other methods */

  // deeply clones current instance
  // @result : deep clone of current instance or NULL if error
  virtual CDownloadResponse *Clone(void);

protected:

  // holds received data
  CLinearBuffer *receivedData;

  // holds CURL result code
  CURLcode resultCode;

  // holds response code
  long responseCode;

  // deeply clones current instance to cloned response
  // @param  clonedResponse : cloned response to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadResponse *clonedResponse);
};

#endif