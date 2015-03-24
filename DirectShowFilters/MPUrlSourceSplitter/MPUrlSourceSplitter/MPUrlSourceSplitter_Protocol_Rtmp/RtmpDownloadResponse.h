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

#ifndef __RTMP_DOWNLOAD_RESPONSE_DEFINED
#define __RTMP_DOWNLOAD_RESPONE_DEFINED

#include "DownloadResponse.h"

#define RTMP_DOWNLOAD_RESPONSE_FLAG_NONE                              DOWNLOAD_RESPONSE_FLAG_NONE

#define RTMP_DOWNLOAD_RESPONSE_FLAG_LAST                              (DOWNLOAD_RESPONSE_FLAG_LAST + 0)

#define RTMP_DURATION_UNSPECIFIED                                     UINT64_MAX

class CRtmpDownloadResponse : public CDownloadResponse
{
public:
  CRtmpDownloadResponse(HRESULT *result);
  virtual ~CRtmpDownloadResponse(void);

  /* get methods */

  // gets duration in ms of RTMP stream
  // @return : duration in ms of RTMP stream or RTMP_DURATION_UNSPECIFIED if duration of stream unspecified
  uint64_t GetDuration(void);

  /* set methods */

  // sets duration in ms of RTMP stream
  // @param duration : the duration in ms of RTMP stream
  void SetDuration(uint64_t duration);

  /* other methods */

protected:

  // holds duration in ms of RTMP steam, RTMP_DURATION_UNSPECIFIED if not specified
  uint64_t duration;

  /* methods */

  // creates download response
  // @return : download response or NULL if error
  virtual CDownloadResponse *CreateDownloadResponse(void);

  // deeply clones current instance to cloned request
  // @param  clone : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CDownloadResponse *clone);
};

#endif