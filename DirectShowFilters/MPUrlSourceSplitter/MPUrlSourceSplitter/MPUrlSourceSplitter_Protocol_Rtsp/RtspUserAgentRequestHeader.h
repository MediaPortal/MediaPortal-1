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

#ifndef __RTSP_USER_AGENT_REQUEST_HEADER_DEFINED
#define __RTSP_USER_AGENT_REQUEST_HEADER_DEFINED

#include "RtspRequestHeader.h"

#define RTSP_USER_AGENT_REQUEST_HEADER_NAME                           L"User-Agent"

class CRtspUserAgentRequestHeader : public CRtspRequestHeader
{
public:
  CRtspUserAgentRequestHeader(void);
  virtual ~CRtspUserAgentRequestHeader(void);

  /* get methods */

  // gets RTSP header name
  // @return : RTSP header name
  virtual const wchar_t *GetName(void);

  // gets user agent string
  // @return : user agent string or NULL if not specified
  virtual const wchar_t *GetUserAgent(void);

  /* set methods */

  // sets RTSP header name
  // @param name : RTSP header name to set
  // @return : true if successful, false otherwise
  virtual bool SetName(const wchar_t *name);

  // sets user agent string
  // @param userAgent : the user agent string to set
  // @return : true if successful, false otherwise
  virtual bool SetUserAgent(const wchar_t *userAgent);

  /* other methods */

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  virtual CRtspUserAgentRequestHeader *Clone(void);

protected:

  // deeply clones current instance to cloned header
  // @param  clonedHeader : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clonedHeader);

  // returns new RTSP request header object to be used in cloning
  // @return : RTSP request header object or NULL if error
  virtual CHttpHeader *GetNewHeader(void);
};

#endif