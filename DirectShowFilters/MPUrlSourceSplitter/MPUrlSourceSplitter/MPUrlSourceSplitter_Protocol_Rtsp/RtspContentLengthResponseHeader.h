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

#ifndef __RTSP_CONTENT_LENGTH_RESPONSE_HEADER_DEFINED
#define __RTSP_CONTENT_LENGTH_RESPONSE_HEADER_DEFINED

#include "RtspResponseHeader.h"

#define RTSP_CONTENT_LENGTH_RESPONSE_HEADER_TYPE                      L"Content-Length"

#define RTSP_CONTENT_LENGTH_UNSPECIFIED                               UINT_MAX

class CRtspContentLengthResponseHeader : public CRtspResponseHeader
{
public:
  CRtspContentLengthResponseHeader(HRESULT *result);
  virtual ~CRtspContentLengthResponseHeader(void);

  /* get methods */

  // gets content length
  // @return : content length or RTSP_CONTENT_LENGTH_UNSPECIFIED if not specified
  virtual unsigned int GetContentLength(void);

  /* set methods */

  /* other methods */

  // parses header and stores name and value to internal variables
  // @param header : header to parse
  // @param length : the length of header
  // @return : true if successful, false otherwise
  virtual bool Parse(const wchar_t *header, unsigned int length);

protected:

  unsigned int contentLength;

  // deeply clones current instance to cloned header
  // @param  clone : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clone);

  // returns new header object to be used in cloning
  // @return : header object or NULL if error
  virtual CHttpHeader *CreateHeader(void);
};

#endif