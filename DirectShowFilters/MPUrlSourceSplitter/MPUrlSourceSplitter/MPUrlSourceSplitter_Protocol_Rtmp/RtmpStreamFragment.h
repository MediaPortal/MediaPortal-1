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
#define RTMP_STREAM_FRAGMENT_FLAG_SET_TIMESTAMP                       (1 << (CACHE_FILE_ITEM_FLAG_LAST + 1))
#define RTMP_STREAM_FRAGMENT_FLAG_DISCONTINUITY                       (1 << (CACHE_FILE_ITEM_FLAG_LAST + 2))
#define RTMP_STREAM_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET      (1 << (CACHE_FILE_ITEM_FLAG_LAST + 3))

#define RTMP_STREAM_FRAGMENT_FLAG_LAST                                (CACHE_FILE_ITEM_FLAG_LAST + 4)

#define RTMP_STREAM_FRAGMENT_START_POSITION_NOT_SET                   -1

class CRtmpStreamFragment : public CCacheFileItem
{
public:
  // creates new instance of CRtmpStreamFragment class
  CRtmpStreamFragment(HRESULT *result);
  CRtmpStreamFragment(HRESULT *result, int64_t fragmentStartTimestamp, bool setStartTimestampFlag);

  virtual ~CRtmpStreamFragment(void);

  /* get methods */

  // gets fragment start timestamp
  // @return : fragment start timestamp
  int64_t GetFragmentStartTimestamp(void);

  // gets fragment start position within stream
  // @return : fragment start position within stream or RTMP_STREAM_FRAGMENT_START_POSITION_NOT_SET if not set
  int64_t GetFragmentStartPosition(void);

  /* set methods */

  // sets if segment and fragment is downloaded
  // @param downloaded : true if segment and fragment is downloaded
  void SetDownloaded(bool downloaded);

  // sets fragment start timestamp
  // @param fragmentStartTimestamp : the fragment start timestamp to set
  void SetFragmentStartTimestamp(int64_t fragmentStartTimestamp);

  // sets fragment start timestamp with specified flag
  // @param fragmentStartTimestamp : the fragment start timestamp to set
  // @param setStartTimestampFlag : fragment has set start timestamp flag
  void SetFragmentStartTimestamp(int64_t fragmentStartTimestamp, bool setStartTimestampFlag);

  // sets fragment start position
  // @param fragmentStartPosition : fragment start position to set
  void SetFragmentStartPosition(int64_t fragmentStartPosition);

  // sets discontinuity
  // @param discontinuity : true if discontinuity after data, false otherwise
  void SetDiscontinuity(bool discontinuity);

  // sets if fragment contains header or meta packet
  // @param containsHeaderOrMetaPacket : true if fragment contains header or meta packet, false otherwise
  void SetContainsHeaderOrMetaPacket(bool containsHeaderOrMetaPacket);

  /* other methods */

  // tests if discontinuity is set
  // @return : true if discontinuity is set, false otherwise
  bool IsDiscontinuity(void);

  // tests if fragment is downloaded
  // @return : true if downloaded, false otherwise
  bool IsDownloaded(void);

  // tests if fragment has set start timestamp
  // @return : true if fragment has set start timestamp, false otherwise
  bool IsSetFragmentStartTimestamp(void);

  // tests if fragment has set start position
  // @return : true if fragment has set start position, false otherwise
  bool IsSetFragmentStartPosition(void);

  // tests if fragment contains header or meta packet
  // @return : true if fragment contains header or meta packet, false otherwise
  bool ContainsHeaderOrMetaPacket(void);

private:

  // holds fragment start timestamp
  int64_t fragmentStartTimestamp;

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