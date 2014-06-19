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

#ifndef __ISEEKING_DEFINED
#define __ISEEKING_DEFINED

#include <stdint.h>

// define seeking capabilities of protocol
// protocol doesn't support seeking
#define SEEKING_METHOD_NONE                                                   0
// protocol supports seeking by position (in bytes)
#define SEEKING_METHOD_POSITION                                               1
// protocol supports seeking by time (in ms)
#define SEEKING_METHOD_TIME                                                   2

#define METHOD_SEEK_TO_TIME_NAME                                              L"SeekToTime()"

#ifndef PAUSE_SEEK_STOP_MODE_NONE

// enable all reading operations
#define PAUSE_SEEK_STOP_MODE_NONE                                             0
// demuxers are not allowed to read data (but data can be read in seek methods)
#define PAUSE_SEEK_STOP_MODE_DISABLE_DEMUXING                                 1
// demuxers are not allowed to read any data (all data request are not successfully processed)
#define PAUSE_SEEK_STOP_MODE_DISABLE_READING                                  2

#endif

// defines interface for seeking
struct ISeeking
{
  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  virtual unsigned int GetSeekingCapabilities(void) = 0;

  // request protocol implementation to receive data from specified time (in ms) for specified stream
  // this method is called with same time for each stream in protocols with multiple streams
  // @param streamId : the stream ID to receive data from specified time
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  virtual int64_t SeekToTime(unsigned int streamId, int64_t time) = 0;

  // set pause, seek or stop mode
  // in such mode are reading operations disabled
  // @param pauseSeekStopMode : one of PAUSE_SEEK_STOP_MODE values
  virtual void SetPauseSeekStopMode(unsigned int pauseSeekStopMode) = 0;
};

#endif