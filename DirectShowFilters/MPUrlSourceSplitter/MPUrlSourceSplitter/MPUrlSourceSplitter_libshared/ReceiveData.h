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

#ifndef __RECEIVE_DATA_DEFINED
#define __RECEIVE_DATA_DEFINED

#include "StreamReceiveDataColletion.h"

#define RECEIVE_DATA_FLAG_NONE                                        0x00000000
// specifies that stream count was set
#define RECEIVE_DATA_FLAG_SET_STREAM_COUNT                            0x00000001
// specifies that streams are in live streams
#define RECEIVE_DATA_FLAG_LIVE_STREAM                                 0x00000002

class CReceiveData
{
public:
  CReceiveData(void);
  ~CReceiveData(void);

  /* get methods */

  // gets received streams
  // @return : received streams collection
  CStreamReceiveDataColletion *GetStreams(void);

  /* set methods */

  // sets stream count
  // @param streamCount : the stream count to set
  // @return : true if successful, false otherwise
  bool SetStreamCount(unsigned int streamCount);

  // sets live stream flag
  // @param liveStream : true if live stream, false otherwise
  void SetLiveStream(bool liveStream);

  /* other methods */

  // tests if stream count was set
  // @return : true if stream count was set, false otherwise
  bool IsSetStreamCount(void);

  // tests if streams are in live streams
  // @return : true if streams are in live streams, false otherwise
  bool IsLiveStream(void);

  // tests if specific combination of flags is set
  // @param flags : the set of flags to test
  // @return : true if set of flags is set, false otherwise
  bool IsSetFlags(unsigned int flags);

  // clears current instance to default state
  void Clear(void);

private:

  // holds various flags
  unsigned int flags;

  // holds streams data
  CStreamReceiveDataColletion *streams;
};

#endif