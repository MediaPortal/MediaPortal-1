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

#ifndef __RTSP_REQUEST_DEFINED
#define __RTSP_REQUEST_DEFINED

#define RTSP_REQUEST_VERSION                                          L"1.0"
#define RTSP_REQUEST_LINE_FORMAT                                      L"%s %s RTSP/%s%s"      // Method SP Request-URI SP RTSP-Version CRLF
#define RTSP_CRLF                                                     L"\r\n"
#define RTSP_REQUEST_FORMAT                                           L"%s%s"           // Request-Line sequence number general-header request-header entity-header CRLF

#include "RtspRequestHeaderCollection.h"

class CRtspRequest
{
public:
  CRtspRequest(HRESULT *result);
  virtual ~CRtspRequest(void);

  /* get methods */

  // gets URI of RTSP request
  // @return : URI of RTSP request or NULL if error
  virtual const wchar_t *GetUri(void);

  // gets RTSP request version
  // @return : RTSP request version (default RTSP_REQUEST_VERSION)
  virtual const wchar_t *GetVersion(void);

  // gets RTSP request to send to remote server
  // @return : RTSP request to send to remote server or NULL if error
  virtual const wchar_t *GetRequest(void);

  // gets RTSP request method
  // @return : RTSP request method
  virtual const wchar_t *GetMethod(void) = 0;

  // gets RTSP request headers
  // @return : RTSP request headers
  virtual CRtspRequestHeaderCollection *GetRequestHeaders(void);

  // gets RTSP sequence number
  // @return : RTSP sequence number or RTSP_SEQUENCE_NUMBER_UNSPECIFIED if not specified
  virtual unsigned int GetSequenceNumber(void);

  // gets RTSP session ID
  // @return : session ID or NULL if not specified
  virtual const wchar_t *GetSessionId(void);

  // gets RTSP request timeout
  // @return : RTSP request timeout
  virtual unsigned int GetTimeout(void);

  /* set methods */

  // sets RTSP request URI
  // @param uri : RTSP request URI to set
  // @return : true if successful, false otherwise
  virtual bool SetUri(const wchar_t *uri);

  // sets RTSP request version
  // @param version : RTSP request version to set
  // @return : true if successful, false otherwise
  virtual bool SetVersion(const wchar_t *version);

  // sets RTSP sequence number
  // @param : RTSP sequence number to set
  virtual void SetSequenceNumber(unsigned int sequenceNumber);

  // sets timeout for RTSP request in ms
  // @param timeout : timeout in ms to set
  virtual void SetTimeout(unsigned int timeout);

  // sets RTSP session ID
  // @param sessionId : the session ID to set
  // @return : true if successful, false otherwise
  virtual bool SetSessionId(const wchar_t *sessionId);

  /* other methods */

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  virtual CRtspRequest *Clone(void);

protected:

  wchar_t *uri;

  wchar_t *version;

  wchar_t *request;

  unsigned int timeout;

  CRtspRequestHeaderCollection *requestHeaders;

  /* methods */

  CRtspRequest(HRESULT *result, bool createDefaultHeaders);

  // deeply clones current instance to cloned RTSP request
  // @param  clone : cloned RTSP request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CRtspRequest *clone);

  // returns new RTSP request object to be used in cloning
  // @return : RTSP request object or NULL if error
  virtual CRtspRequest *CreateRequest(void) = 0;
};

#endif