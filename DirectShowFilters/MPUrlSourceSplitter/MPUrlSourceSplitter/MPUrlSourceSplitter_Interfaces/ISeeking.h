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

#define METHOD_SEEK_TO_POSITION_NAME                                          L"SeekToPosition()"
#define METHOD_SEEK_TO_TIME_NAME                                              L"SeekToTime()"

// defines interface for seeking
struct ISeeking
{
  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  virtual unsigned int GetSeekingCapabilities(void) = 0;

  // request protocol implementation to receive data from specified time (in ms)
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  virtual int64_t SeekToTime(int64_t time) = 0;

  // request protocol implementation to receive data from specified position to specified position
  // @param start : the requested start position (zero is start of stream)
  // @param end : the requested end position, if end position is lower or equal to start position than end position is not specified
  // @return : position where seek finished or lower than zero if error
  virtual int64_t SeekToPosition(int64_t start, int64_t end) = 0;

  // sets if protocol implementation have to supress sending data to filter
  // @param supressData : true if protocol have to supress sending data to filter, false otherwise
  virtual void SetSupressData(bool supressData) = 0;
};

#endif