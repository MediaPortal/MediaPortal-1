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

#ifndef __RTSP_DOWNLOAD_REQUEST_DEFINED
#define __RTSP_DOWNLOAD_REQUEST_DEFINED

#include "DownloadRequest.h"

class CRtspDownloadRequest : public CDownloadRequest
{
public:
  CRtspDownloadRequest(void);
  virtual ~CRtspDownloadRequest(void);

  /* get methods */

  // gets RTSP start time in ms
  // @return : RTSP start time in ms
  uint64_t GetStartTime(void);

  /* set methods */

  // sets RTSP start time
  // @return : RTSP start time in ms to set
  void SetStartTime(uint64_t startTime);

  /* other methods */

  // deeply clones current instance
  // @result : deep clone of current instance or NULL if error
  virtual CRtspDownloadRequest *Clone(void);

protected:

  // RTSP protocol specific variables

  // holds start time
  uint64_t startTime;

  // deeply clones current instance to cloned request
  // @param  clonedRequest : cloned request to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CRtspDownloadRequest *clonedRequest);
};

#endif