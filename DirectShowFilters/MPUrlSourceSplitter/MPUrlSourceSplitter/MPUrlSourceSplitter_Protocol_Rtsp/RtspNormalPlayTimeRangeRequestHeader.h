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

#define NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_NONE               RTSP_REQUEST_HEADER_FLAG_NONE

#define NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_START              (1 << (RTSP_REQUEST_HEADER_FLAG_LAST + 0))
#define NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_END                (1 << (RTSP_REQUEST_HEADER_FLAG_LAST + 1))

#define NORMAL_PLAY_TIME_RANGE_REQUEST_HEADER_FLAG_LAST               (RTSP_REQUEST_HEADER_FLAG_LAST + 2)

#define TIME_UNSPECIFIED                                              UINT64_MAX

#define NORMAL_PLAY_TIME_END_VALUE_FORMAT                             L"npt=-%llu.%03llu"
#define NORMAL_PLAY_TIME_START_VALUE_FORMAT                           L"npt=%llu.%03llu-"
#define NORMAL_PLAY_TIME_START_END_VALUE_FORMAT                       L"npt=%llu.%03llu-%llu.%03llu"

class CRtspNormalPlayTimeRangeRequestHeader : public CRtspRangeRequestHeader
{
public:
  CRtspNormalPlayTimeRangeRequestHeader(HRESULT *result);
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

  /* other methods */

  // tests if for normal play time is set start time
  // @return : true if set, false otherwise
  virtual bool IsSetStart(void);

  // tests if for normal play time is set end time
  // @return : true if set, false otherwise
  virtual bool IsSetEnd(void);

protected:

  uint64_t startTime;
  uint64_t endTime;

  /* methods */

  // deeply clones current instance to cloned header
  // @param  clone : cloned header to hold clone of current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CHttpHeader *clone);

  // returns new RTSP request header object to be used in cloning
  // @return : RTSP request header object or NULL if error
  virtual CHttpHeader *CreateHeader(void);
};

#endif