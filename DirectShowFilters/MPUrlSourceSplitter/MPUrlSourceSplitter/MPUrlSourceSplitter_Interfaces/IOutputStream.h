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

#ifndef __IOUTPUT_STREAM_DEFINED
#define __IOUTPUT_STREAM_DEFINED

#include "StreamReceiveData.h"

#include <stdint.h>

#define METHOD_PUSH_STREAM_RECEIVE_DATA_NAME                          L"PushStreamReceiveData()"

// defines interface for stream output
struct IOutputStream
{
  // notifies output stream about stream count
  // @param streamCount : the stream count
  // @param liveStream : true if stream(s) are live, false otherwise
  // @return : S_OK if successful, false otherwise
  virtual HRESULT SetStreamCount(unsigned int streamCount, bool liveStream) = 0;

  // pushes stream received data to filter
  // @param streamId : the stream ID to push stream received data
  // @param streamReceivedData : the stream received data to push to filter
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT PushStreamReceiveData(unsigned int streamId, CStreamReceiveData *streamReceiveData) = 0;
};

#endif