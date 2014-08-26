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

#ifndef __RTSP_STREAM_FRAGMENT_DEFINED
#define __RTSP_STREAM_FRAGMENT_DEFINED

#include "CacheFileItem.h"
#include "RtpPacket.h"

#define RTSP_STREAM_FRAGMENT_FLAG_NONE                                CACHE_FILE_ITEM_FLAG_NONE

#define RTSP_STREAM_FRAGMENT_FLAG_SET_RTP_TIMESTAMP                   (1 << (CACHE_FILE_ITEM_FLAG_LAST + 0))

#define RTSP_STREAM_FRAGMENT_FLAG_LAST                                (CACHE_FILE_ITEM_FLAG_LAST + 1)

#define RTSP_STREAM_FRAGMENT_START_POSITION_NOT_SET                   -1

class CRtspStreamFragment : public CCacheFileItem
{
public:
  // initializes a new instance of CRtspStreamFragment class
  CRtspStreamFragment(HRESULT *result);
  CRtspStreamFragment(HRESULT *result, int64_t fragmentRtpTimestamp, bool setRtpTimestampFlag);

  // destructor
  virtual ~CRtspStreamFragment(void);

  /* get methods */

  // gets fragment RTP timestamp
  // @return : fragment RTP timestamp
  int64_t GetFragmentRtpTimestamp(void);

  // gets fragment start position within stream
  // @return : fragment start position within stream or RTSP_STREAM_FRAGMENT_START_POSITION_NOT_SET if not set
  int64_t GetFragmentStartPosition(void);

  /* set methods */

  // sets fragment RTP timestamp
  // @param fragmentRtpTimestamp : the fragment RTP timestamp to set
  void SetFragmentRtpTimestamp(int64_t fragmentRtpTimestamp);

  // sets fragment RTP timestamp with specified flag
  // @param fragmentRtpTimestamp : the fragment RTP timestamp to set
  // @param setRtpTimestampFlag : fragment has set RTP timestamp flag
  void SetFragmentRtpTimestamp(int64_t fragmentRtpTimestamp, bool setRtpTimestampFlag);

  // sets fragment start position
  // @param fragmentStartPosition : fragment start position to set
  void SetFragmentStartPosition(int64_t fragmentStartPosition);

  /* other methods */

  // tests if fragment has set RTP timestamp
  // @return : true if fragment has set RTP timestamp, false otherwise
  virtual bool IsSetFragmentRtpTimestamp(void);

  // tests if fragment has set start position
  // @return : true if fragment has set start position, false otherwise
  virtual bool IsSetFragmentStartPosition(void);

protected:
  /* timestamps are with sign, sometimes are timestamps negative */

  // holds fragment RTP timestamp
  int64_t fragmentRtpTimestamp;

  // holds fragment start position within stream
  int64_t fragmentStartPosition;

  /* methods */

  // gets new instance of RTSP stream fragment
  // @return : new RTSP stream fragment instance or NULL if error
  virtual CCacheFileItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the cache file item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CCacheFileItem *item);
};

#endif