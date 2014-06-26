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

#ifndef __RTSP_RESPONSE_HEADER_DEFINED
#define __RTSP_RESPONSE_HEADER_DEFINED

#include "HttpHeader.h"

#define RESPONSE_HEADER_TYPE_UNSPECIFIED                              NULL

#define RTSP_RESPONSE_HEADER_FLAG_NONE                                HTTP_HEADER_FLAG_NONE

#define RTSP_RESPONSE_HEADER_FLAG_LAST                                (HTTP_HEADER_FLAG_LAST + 0)

class CRtspResponseHeader : public CHttpHeader
{
public:
  CRtspResponseHeader(HRESULT *result);
  virtual ~CRtspResponseHeader(void);

  /* get methods */

  // gets response header type
  // @return : response header type or RESPONSE_HEADER_TYPE_UNSPECIFIED if not specified
  virtual const wchar_t *GetResponseHeaderType(void);

  /* set methods */

  /* other methods */

  // tests if instance response header type is same as specified response header type
  // @param responseHeaderType : response header type to test
  // @return : true if same, false otherwise
  virtual bool IsResponseHeaderType(const wchar_t *responseHeaderType);

protected:

  wchar_t *responseHeaderType;

  // deeply clones current instance to cloned header
  // @param  clone : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clone);

  // returns new header object to be used in cloning
  // @return : header object or NULL if error
  virtual CHttpHeader *CreateHeader(void);
};

#endif