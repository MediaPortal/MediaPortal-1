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

#ifndef __HTTP_DOWNLOAD_REQUEST_DEFINED
#define __HTTP_DOWNLOAD_REQUEST_DEFINED

#include "DownloadRequest.h"
#include "HttpHeaderCollection.h"

#include <stdint.h>

class CHttpDownloadRequest : public CDownloadRequest
{
public:
  CHttpDownloadRequest(void);
  virtual ~CHttpDownloadRequest(void);

  /* get methods */

  // gets start position
  // @return : start position
  virtual uint64_t GetStartPosition(void);

  // gets end position
  // @return : end position
  virtual uint64_t GetEndPosition(void);

  // gets cookie to send (overrides default cookies)
  // @return : cookie or NULL if error or not specified
  virtual const wchar_t *GetCookie(void);

  // gets referer
  // @return : referer or NULL if error or not specified
  virtual const wchar_t *GetReferer(void);

  // gets user agent
  // @return : user agent or NULL if error or not specified
  virtual const wchar_t *GetUserAgent(void);

  // gets HTTP version
  // @return : HTTP version
  virtual int GetHttpVersion(void);

  // gets ignore content length
  // @return : ignore content length
  virtual bool GetIgnoreContentLength(void);

  // gets additional headers to sent to server
  // @return : collection of additional headers
  virtual CHttpHeaderCollection *GetHeaders(void);

  /* set methods */

  // sets start position
  // @param startPosition : start position to set
  virtual void SetStartPosition(uint64_t startPosition);

  // sets end position
  // @param endPosition : end position to set
  virtual void SetEndPosition(uint64_t endPosition);

  // sets cookie to send (overrides default cookies)
  // @param cookie : cookie to set
  // @return : true if successful, false otherwise
  virtual bool SetCookie(const wchar_t *cookie);

  // sets referer
  // @param referer : referer to set
  // @return : true if successful, false otherwise
  virtual bool SetReferer(const wchar_t *referer);

  // sets user agent
  // @param userAgent : user agent to set
  // @return : true if successful, false otherwise
  virtual bool SetUserAgent(const wchar_t *userAgent);

  // sets HTTP version
  // @param httpVersion : HTTP version to set
  virtual void SetHttpVersion(int httpVersion);

  // sets ignore content length
  // @param ignoreContentLength : ignore content length to set
  virtual void SetIgnoreContentLength(bool ignoreContentLength);

  /* other methods */

  // deeply clones current instance
  // @result : deep clone of current instance or NULL if error
  virtual CDownloadRequest *Clone(void);

protected:

  // ranges start position and end position
  uint64_t startPosition;
  uint64_t endPosition;

  // referer header in HTTP request
  wchar_t *referer;

  // user agent header in HTTP request
  wchar_t *userAgent;

  // cookie header in HTTP request
  wchar_t *cookie;

  // the HTTP protocol version
  int httpVersion;

  // specifies if CURL have to ignore content length
  bool ignoreContentLength;

  // holds collection of additional headers to set to server
  CHttpHeaderCollection *headers;

  // deeply clones current instance to cloned request
  // @param  clonedRequest : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpDownloadRequest *clonedRequest);
};

#endif