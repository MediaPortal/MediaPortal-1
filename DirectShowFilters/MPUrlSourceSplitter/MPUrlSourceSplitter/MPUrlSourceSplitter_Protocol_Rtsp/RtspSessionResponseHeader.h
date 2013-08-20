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

#ifndef __RTSP_SESSION_RESPONSE_HEADER_DEFINED
#define __RTSP_SESSION_RESPONSE_HEADER_DEFINED

#include "RtspResponseHeader.h"

#define RTSP_SESSION_RESPONSE_HEADER_TYPE                                 L"Session"

#define RTSP_SESSION_RESPONSE_TIMEOUT_DEFAULT                             60

#define RTSP_SESSION_RESPONSE_HEADER_PARAMETER_TIMEOUT                    L"timeout"
#define RTSP_SESSION_RESPONSE_HEADER_PARAMETER_TIMEOUT_LENGTH             7

#define RTSP_SESSION_RESPONSE_HEADER_SEPARATOR                            L";"
#define RTSP_SESSION_RESPONSE_HEADER_SEPARATOR_LENGTH                     1

#define RTSP_SESSION_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR            L"="
#define RTSP_SESSION_RESPONSE_HEADER_PARAMETER_VALUE_SEPARATOR_LENGTH     1

class CRtspSessionResponseHeader : public CRtspResponseHeader
{
public:
  CRtspSessionResponseHeader(void);
  virtual ~CRtspSessionResponseHeader(void);

  /* get methods */

  // gets session ID
  // @return : session ID or NULL if error
  virtual const wchar_t *GetSessionId(void);

  // gets session timeout
  // @return : session timeout
  virtual unsigned int GetTimeout(void);

  /* set methods */

  /* other methods */

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  virtual CRtspSessionResponseHeader *Clone(void);

  // parses header and stores name and value to internal variables
  // @param header : header to parse
  // @param length : the length of header
  // @return : true if successful, false otherwise
  virtual bool Parse(const wchar_t *header, unsigned int length);

protected:

  wchar_t *sessionId;
  unsigned int timeout;

  // deeply clones current instance to cloned header
  // @param  clonedHeader : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clonedHeader);

  // returns new header object to be used in cloning
  // @return : header object or NULL if error
  virtual CHttpHeader *GetNewHeader(void);
};

#endif