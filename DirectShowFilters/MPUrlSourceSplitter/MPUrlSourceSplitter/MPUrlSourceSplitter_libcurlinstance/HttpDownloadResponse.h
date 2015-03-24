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

#ifndef __HTTP_DOWNLOAD_RESPONSE_DEFINED
#define __HTTP_DOWNLOAD_RESPONSE_DEFINED

#include "DownloadResponse.h"
#include "HttpHeaderCollection.h"

#define HTTP_DOWNLOAD_RESPONSE_FLAG_NONE                              DOWNLOAD_RESPONSE_FLAG_NONE

#define HTTP_DOWNLOAD_RESPONSE_FLAG_RANGES_SUPPORTED                  (1 << (DOWNLOAD_RESPONSE_FLAG_LAST + 0))

#define HTTP_DOWNLOAD_RESPONSE_FLAG_LAST                              (DOWNLOAD_RESPONSE_FLAG_LAST + 1)

class CHttpDownloadResponse : public CDownloadResponse
{
public:
  CHttpDownloadResponse(HRESULT *result);
  virtual ~CHttpDownloadResponse(void);

  /* get methods */

  // gets response headers
  // @return : collection of headers
  virtual CHttpHeaderCollection *GetHeaders(void);

  // gets if ranges are supported
  // @return : true if ranges are supported, false otherwise
  virtual bool GetRangesSupported(void);

  // gets response code
  // @return : response code
  virtual long GetResponseCode(void);

  /* set methods */

  // sets if ranges are supported
  // @param rangesSupported : true if ranges are supported, false otherwise
  virtual void SetRangesSupported(bool rangesSupported);

  // sets response code
  // @param responseCode : response code to set
  virtual void SetResponseCode(long responseCode);

  /* other methods */

protected:
  // holds received headers
  CHttpHeaderCollection *headers;
  // holds HTTP response code
  long responseCode;

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