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

#ifndef __MEDIA_PACKET_DEFINED
#define __MEDIA_PACKET_DEFINED

#include "CacheFileItem.h"

#define MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED                 INT64_MIN

// DirectShow times are in 100ns units
#ifndef DSHOW_TIME_BASE
#define DSHOW_TIME_BASE                                               10000000
#endif

#define MEDIA_PACKET_FLAG_NONE                                        CACHE_FILE_ITEM_FLAG_NONE

#define MEDIA_PACKET_FLAG_LAST                                        (CACHE_FILE_ITEM_FLAG_LAST + 1)

// CMediaPacket class is wrapper for IMediaSample interface
// this class doesn't implement all methods of IMediaSample interface
class CMediaPacket : public CCacheFileItem
{
public:
  CMediaPacket(HRESULT *result);
  virtual ~CMediaPacket();

  /* get methods */

  // gets the stream position where this packet starts
  // @return : the stream position where this packet starts
  int64_t GetStart(void);

  // gets presentation timestamp in 100ns (DSHOW_TIME_BASE ticks per second) units
  // @return : presentation timestamp in in 100ns (DSHOW_TIME_BASE ticks per second) units or MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED if not defined
  int64_t GetPresentationTimestamp(void);

  /* set methods */

  // sets the stream position where this packet starts
  // @param position : the stream position where this packet starts
  void SetStart(int64_t position);

  // sets presentation timestamp (in DSHOW_TIME_BASE units)
  // @param presentationTimestamp : the presentation timestamp in ticks per second to set or MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED if not defined
  void SetPresentationTimestamp(int64_t presentationTimestamp);

  // sets presentation timestamp in specified timestamp units
  // the presentation timestamp is converted to DSHOW_TIME_BASE units
  // @param presentationTimestamp : the presentation timestamp in ticks per second to set or MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED if not defined
  // @param presentationTimestampTicksPerSecond : presentation timestamp ticks per second to set
  void SetPresentationTimestamp(int64_t presentationTimestamp, unsigned int presentationTimestampTicksPerSecond);

  /* other methods */

protected:
  // start sample - byte position
  int64_t start;
  // holds presentation timestamp
  int64_t presentationTimestamp;

  /* methods */

  // gets new instance of cache file item
  // @return : new cache file item instance or NULL if error
  virtual CFastSearchItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the cache file item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CFastSearchItem *item);
};

#endif

