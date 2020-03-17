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

#define HTTP_DOWNLOAD_REQUEST_FLAG_NONE                               DOWNLOAD_REQUEST_FLAG_NONE

#define HTTP_DOWNLOAD_REQUEST_FLAG_IGNORE_CONTENT_LENGTH              (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 0))
#define HTTP_DOWNLOAD_REQUEST_FLAG_AUTHENTICATE                       (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 1))
#define HTTP_DOWNLOAD_REQUEST_FLAG_PROXY_AUTHENTICATE                 (1 << (DOWNLOAD_REQUEST_FLAG_LAST + 2))

#define HTTP_DOWNLOAD_REQUEST_FLAG_LAST                               (DOWNLOAD_REQUEST_FLAG_LAST + 3)

// proxy types
#define HTTP_PROXY_TYPE_NONE                                          0
#define HTTP_PROXY_TYPE_HTTP                                          1
#define HTTP_PROXY_TYPE_HTTP_1_0                                      2
#define HTTP_PROXY_TYPE_SOCKS4                                        3
#define HTTP_PROXY_TYPE_SOCKS5                                        4
#define HTTP_PROXY_TYPE_SOCKS4A                                       5
#define HTTP_PROXY_TYPE_SOCKS5_HOSTNAME                               6

class CHttpDownloadRequest : public CDownloadRequest
{
public:
  CHttpDownloadRequest(HRESULT *result);
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
  virtual unsigned int GetHttpVersion(void);

  // gets ignore content length
  // @return : ignore content length
  virtual bool GetIgnoreContentLength(void);

  // gets additional headers to sent to server
  // @return : collection of additional headers
  virtual CHttpHeaderCollection *GetHeaders(void);

  // gets remote server user name
  // @return : remote server user name
  virtual const wchar_t *GetServerUserName(void);

  // gets remote server password
  // @return : remote server password
  virtual const wchar_t *GetServerPassword(void);

  // gets proxy server
  // @return : proxy server
  virtual const wchar_t *GetProxyServer(void);

  // gets proxy server port
  // @return : proxy server port
  virtual unsigned short GetProxyServerPort(void);

  // gets proxy server user name
  // @return : proxy server user name
  virtual const wchar_t *GetProxyServerUserName(void);

  // gets proxy server password
  // @return : proxy server password
  virtual const wchar_t *GetProxyServerPassword(void);

  // gets proxy server type
  // @return : proxy server type
  virtual unsigned int GetProxyServerType(void);

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
  virtual void SetHttpVersion(unsigned int httpVersion);

  // sets ignore content length
  // @param ignoreContentLength : ignore content length to set
  virtual void SetIgnoreContentLength(bool ignoreContentLength);

  // sets remote server authentication
  // @param authenticate : true if authentication is enabled, false otherwise
  // @param serverUserName : server user name
  // @param serverPassword : server password
  // @return : true if successful, false otherwise
  virtual bool SetAuthentication(bool authenticate, const wchar_t *serverUserName, const wchar_t *serverPassword);

  // sets proxy authentication
  // @param authenticate : true if proxy authentication is enabled, false otherwise
  // @param proxyServer : proxy server hostname or IP address
  // @param proxyServerPort : proxy server port
  // @param proxyServerType : proxy server type (one of HTTP_PROXY_TYPE values)
  // @param proxyServerUserName : proxy server user name
  // @param proxyServerPassword : proxy server password
  // @return : true if successful, false otherwise
  virtual bool SetProxyAuthentication(bool authenticate, const wchar_t *proxyServer, unsigned short proxyServerPort, unsigned int proxyServerType, const wchar_t *proxyServerUserName, const wchar_t *proxyServerPassword);

  /* other methods */

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
  unsigned int httpVersion;

  // holds collection of additional headers to set to server
  CHttpHeaderCollection *headers;

  /* authentication */

  wchar_t *serverUserName;
  wchar_t *serverPassword;

  /* proxy server and authentication */

  wchar_t *proxyServer;
  unsigned short proxyServerPort;
  wchar_t *proxyServerUserName;
  wchar_t *proxyServerPassword;
  unsigned int proxyServerType;

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