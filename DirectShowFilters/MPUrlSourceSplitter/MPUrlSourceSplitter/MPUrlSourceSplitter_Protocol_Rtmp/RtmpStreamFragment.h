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

#ifndef __RTMP_STREAM_FRAGMENT_DEFINED
#define __RTMP_STREAM_FRAGMENT_DEFINED

#include "CacheFileItem.h"

#define RTMP_STREAM_FRAGMENT_FLAG_NONE                                CACHE_FILE_ITEM_FLAG_NONE

#define RTMP_STREAM_FRAGMENT_FLAG_DOWNLOADED                          (1 << (CACHE_FILE_ITEM_FLAG_LAST + 0))
#define RTMP_STREAM_FRAGMENT_FLAG_SEEKED                              (1 << (CACHE_FILE_ITEM_FLAG_LAST + 1))
#define RTMP_STREAM_FRAGMENT_FLAG_HAS_INCORRECT_TIMESTAMPS            (1 << (CACHE_FILE_ITEM_FLAG_LAST + 2))
#define RTMP_STREAM_FRAGMENT_FLAG_SET_START_TIMESTAMP                 (1 << (CACHE_FILE_ITEM_FLAG_LAST + 3))

class CRtmpStreamFragment : public CCacheFileItem
{
public:
  // creates new instance of CRtmpStreamFragment class with specified key frame timestamp
  CRtmpStreamFragment(void);
  virtual ~CRtmpStreamFragment(void);

  /* get methods */

  // gets fragment start timestamp in ms
  // @return : fragment start timestamp in ms
  uint64_t GetFragmentStartTimestamp(void);

  // gets fragment end timestamp in ms
  // @return : fragment end timestamp in ms
  uint64_t GetFragmentEndTimestamp(void);

  // gets packet correction (positive or negative)
  // @return : packet correction
  int GetPacketCorrection(void);

  /* set methods */

  // sets fragment start timestamp
  // @param fragmentStartTimestamp : fragment start timestamp in ms to set
  // @param setStartTimestamp : specifies if start timestamp is set by received data
  void SetFragmentStartTimestamp(uint64_t fragmentStartTimestamp, bool setStartTimestamp);

  // sets fragment end timestamp
  // @param fragmentEndTimestamp : fragment end timestamp in ms to set
  void SetFragmentEndTimestamp(uint64_t fragmentEndTimestamp);

  // sets if fragment is downloaded
  // @param downloaded : true if fragment is downloaded
  void SetDownloaded(bool downloaded);

  // sets if fragment is first fragment after seek (it can be incomplete)
  // @param seeked : true if fragment is first after seek, false otherwise
  void SetSeeked(bool seeked);

  // sets if fragment has incorrect timestamps (it happen after seeking)
  // @param hasIncorrectTimestamps : true if fragment has incorect timestamps, false otherwise
  void SetIncorrectTimestamps(bool hasIncorrectTimestamps);

  // sets packet correction (positive or negative)
  // @param packetCorrection : packet correction
  void SetPacketCorrection(int packetCorrection);

  /* other methods */

  // tests if segment and fragment is downloaded
  // @return : true if downloaded, false otherwise
  bool IsDownloaded(void);

  // tests if start timestamp was set by received data
  bool IsStartTimestampSet(void);

  // tests if fragment is first fragment after seek (it can be incomplete)
  // @return : true if fragment is first after seek, false otherwise
  bool IsSeeked(void);

  // tests if fragment has incorrect timestamps (it happen after seeking)
  // @return : true if fragment has incorect timestamps, false otherwise
  bool HasIncorrectTimestamps(void);

private:
  // stores fragment start timestamp
  uint64_t fragmentStartTimestamp;
  // stores fragment end timestamp
  uint64_t fragmentEndTimestamp;
  // holds packet correction
  int packetCorrection;

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