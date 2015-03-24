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

#ifndef __RTSP_SESSION_REQUEST_HEADER_DEFINED
#define __RTSP_SESSION_REQUEST_HEADER_DEFINED

#include "RtspRequestHeader.h"

#define RTSP_SESSION_REQUEST_HEADER_NAME                              L"Session"

#define RTSP_SESSION_REQUEST_HEADER_FLAG_NONE                         RTSP_REQUEST_HEADER_FLAG_NONE

#define RTSP_SESSION_REQUEST_HEADER_FLAG_LAST                         (RTSP_REQUEST_HEADER_FLAG_LAST + 0)

class CRtspSessionRequestHeader : public CRtspRequestHeader
{
public:
  CRtspSessionRequestHeader(HRESULT *result);
  virtual ~CRtspSessionRequestHeader(void);

  /* get methods */

  // gets RTSP header name
  // @return : RTSP header name
  virtual const wchar_t *GetName(void);

  // gets RTSP session ID
  // @return : session ID or NULL if error
  virtual const wchar_t *GetSessionId(void);

  // gets RTSP header value
  // @return : RTSP header value
  virtual const wchar_t *GetValue(void);

  /* set methods */

  // sets RTSP header name
  // @param name : RTSP header name to set
  // @return : true if successful, false otherwise
  virtual bool SetName(const wchar_t *name);

  // sets RTSP header value
  // @param value : RTSP header value to set
  // @return : true if successful, false otherwise
  virtual bool SetValue(const wchar_t *value);

  // sets RTSP session ID
  // @param sessionId : the session ID to set
  // @return : true if successful, false otherwise
  virtual bool SetSessionId(const wchar_t *sessionId);

  /* other methods */

protected:

  wchar_t *sessionId;

  /* methods */

  // deeply clones current instance to cloned header
  // @param  clone : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clone);

  // returns new RTSP request header object to be used in cloning
  // @return : RTSP request header object or NULL if error
  virtual CHttpHeader *CreateHeader(void);
};

#endif