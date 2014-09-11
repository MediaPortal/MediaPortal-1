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

#include "StreamFragment.h"

#define RTMP_STREAM_FRAGMENT_FLAG_NONE                                STREAM_FRAGMENT_FLAG_NONE

#define RTMP_STREAM_FRAGMENT_FLAG_SET_TIMESTAMP                       (1 << (STREAM_FRAGMENT_FLAG_LAST + 0))
#define RTMP_STREAM_FRAGMENT_FLAG_CONTAINS_HEADER_OR_META_PACKET      (1 << (STREAM_FRAGMENT_FLAG_LAST + 1))

#define RTMP_STREAM_FRAGMENT_FLAG_LAST                                (STREAM_FRAGMENT_FLAG_LAST + 2)

class CRtmpStreamFragment : public CStreamFragment
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

  /* set methods */

  // sets fragment start timestamp
  // @param fragmentStartTimestamp : the fragment start timestamp to set
  void SetFragmentStartTimestamp(int64_t fragmentStartTimestamp);

  // sets fragment start timestamp with specified flag
  // @param fragmentStartTimestamp : the fragment start timestamp to set
  // @param setStartTimestampFlag : fragment has set start timestamp flag
  void SetFragmentStartTimestamp(int64_t fragmentStartTimestamp, bool setStartTimestampFlag);

  // sets if fragment contains header or meta packet
  // @param containsHeaderOrMetaPacket : true if fragment contains header or meta packet, false otherwise
  void SetContainsHeaderOrMetaPacket(bool containsHeaderOrMetaPacket);

  /* other methods */

  // tests if fragment has set start timestamp
  // @return : true if fragment has set start timestamp, false otherwise
  bool IsSetFragmentStartTimestamp(void);

  // tests if fragment contains header or meta packet
  // @return : true if fragment contains header or meta packet, false otherwise
  bool ContainsHeaderOrMetaPacket(void);

private:
  // holds fragment start timestamp
  int64_t fragmentStartTimestamp;

  /* methods */

  // gets new instance of RTMP stream fragment
  // @return : new RTMP stream fragment instance or NULL if error
  virtual CFastSearchItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the cache file item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CFastSearchItem *item);
};

#endif