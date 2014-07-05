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

#define MEDIA_PACKET_FLAG_DISCONTINUITY                               (1 << (CACHE_FILE_ITEM_FLAG_LAST + 0))

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

  // gets the stream position where this packet ends
  // @return : the stream position where this packet ends
  int64_t GetEnd(void);

  // gets presentation timestamp
  // @return : presentation timestamp in ticks per second or MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED if not defined
  int64_t GetPresentationTimestamp(void);

  // gets presentation timestamp in 100ns (DSHOW_TIME_BASE ticks per second) units
  // @return : presentation timestamp in 100ns units or MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED if not defined
  int64_t GetPresentationTimestampInDirectShowTimeUnits(void);

  // gets presentation timestamp ticks per second
  // @return : presentation timestamp ticks per second
  unsigned int GetPresentationTimestampTicksPerSecond(void);

  /* set methods */

  // sets the stream position where this packet starts
  // @param position : the stream position where this packet starts
  void SetStart(int64_t position);

  // sets the stream position where this packet ends
  // @param position : the stream position where this packet ends
  void SetEnd(int64_t position);

  // sets presentation timestamp
  // @param presentationTimestamp : the presentation timestamp in ticks per second to set or MEDIA_PACKET_PRESENTATION_TIMESTAMP_UNDEFINED if not defined
  void SetPresentationTimestamp(int64_t presentationTimestamp);

  // sets presentation timestamp ticks per second
  // @param presentationTimestampTicksPerSecond : presentation timestamp ticks per second to set
  void SetPresentationTimestampTicksPerSecond(unsigned int presentationTimestampTicksPerSecond);

  // set discontinuity
  // @param discontinuity : true if discontinuity after data, false otherwise
  void SetDiscontinuity(bool discontinuity);

  /* other methods */

  // tests if discontinuity is set
  // @return : true if discontinuity is set, false otherwise
  bool IsDiscontinuity(void);

  // deeply clone current instance of media packet with specified position range to new media packet
  // @param start : start position of new media packet
  // @param end : end position of new media packet
  // @return : new media packet or NULL if error or media packet is stored to file
  CMediaPacket *CreateMediaPacketBasedOnPacket(int64_t start, int64_t end);

protected:
  // start sample - byte position
  int64_t start;
  // end sample - byte position
  int64_t end;

  // holds presentation timestamp
  int64_t presentationTimestamp;
  // holds presentation timestamp ticks per second
  unsigned int presentationTimestampTicksPerSecond;

  /* methods */

  // gets new instance of media packet
  // @return : new media packet instance or NULL if error
  virtual CCacheFileItem *CreateItem(void);

  // deeply clones current instance
  // @param item : the cache file item instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CCacheFileItem *item);
};

#endif

