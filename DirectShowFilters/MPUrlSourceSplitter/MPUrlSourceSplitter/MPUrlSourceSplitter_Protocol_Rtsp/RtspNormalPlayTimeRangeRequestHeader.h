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

#ifndef __RTSP_NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_DEFINED
#define __RTSP_NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_DEFINED

#include "RtspRangeRequestHeader.h"

#define FLAG_NORMAL_PLAY_TIME_NONE                                    0x00000000
#define FLAG_NORMAL_PLAY_TIME_START                                   0x00000001
#define FLAG_NORMAL_PLAY_TIME_END                                     0x00000002

#define TIME_UNSPECIFIED                                              UINT64_MAX

#define NORMAL_PLAY_TIME_END_VALUE_FORMAT                             L"npt=-%llu.%03llu"
#define NORMAL_PLAY_TIME_START_VALUE_FORMAT                           L"npt=%llu.%03llu-"
#define NORMAL_PLAY_TIME_START_END_VALUE_FORMAT                       L"npt=%llu.%03llu-%llu.%03llu"

class CRtspNormalPlayTimeRangeRequestHeader : public CRtspRangeRequestHeader
{
public:
  CRtspNormalPlayTimeRangeRequestHeader(void);
  virtual ~CRtspNormalPlayTimeRangeRequestHeader(void);

  /* get methods */

  // gets RTSP header value
  // @return : RTSP header value
  virtual const wchar_t *GetValue(void);

  // gets start time in ms
  // @return : start time in ms or TIME_UNSPECIFIED if not specified
  virtual uint64_t GetStartTime(void);

  // gets end time in ms
  // @return : end time in ms or TIME_UNSPECIFIED if not specified
  virtual uint64_t GetEndTime(void);

  // gets flags
  // @return : flags
  virtual unsigned int GetFlags(void);

  /* set methods */

  // sets RTSP header value
  // @param value : RTSP header value to set
  // @return : true if successful, false otherwise
  virtual bool SetValue(const wchar_t *value);

  // sets start time in ms
  // @param startTime : the start time in ms to set
  virtual void SetStartTime(uint64_t startTime);

  // sets end time in ms
  // @param endTime : the end time in ms to set
  virtual void SetEndTime(uint64_t endTime);

  // sets flags
  // @param flags : the flags to set
  virtual void SetFlags(unsigned int flags);

  /* other methods */

  // tests if for normal play time is set start time
  // @return : true if set, false otherwise
  virtual bool IsSetStart(void);

  // tests if for normal play time is set end time
  // @return : true if set, false otherwise
  virtual bool IsSetEnd(void);

  // tests if flag is set
  // @param flag : the flag to test
  // @return : true if flag is set, false otherwise
  virtual bool IsSetFlag(unsigned int flag);

  // deep clones of current instance
  // @return : deep clone of current instance or NULL if error
  virtual CRtspNormalPlayTimeRangeRequestHeader *Clone(void);

protected:

  unsigned int flags;

  uint64_t startTime;
  uint64_t endTime;

  // deeply clones current instance to cloned header
  // @param  clonedHeader : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clonedHeader);

  // returns new RTSP request header object to be used in cloning
  // @return : RTSP request header object or NULL if error
  virtual CHttpHeader *GetNewHeader(void);
};

#endif