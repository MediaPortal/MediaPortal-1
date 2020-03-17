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

#ifndef __OUTPUT_PIN_PACKET_DEFINED
#define __OUTPUT_PIN_PACKET_DEFINED

#include "CacheFileItem.h"

#define OUTPUT_PIN_PACKET_FLAG_NONE                                   CACHE_FILE_ITEM_FLAG_NONE

#define OUTPUT_PIN_PACKET_FLAG_DISCONTINUITY                          (1 << (CACHE_FILE_ITEM_FLAG_LAST + 0))
#define OUTPUT_PIN_PACKET_FLAG_SYNC_POINT                             (1 << (CACHE_FILE_ITEM_FLAG_LAST + 1))
#define OUTPUT_PIN_PACKET_FLAG_END_OF_STREAM                          (1 << (CACHE_FILE_ITEM_FLAG_LAST + 2))
#define OUTPUT_PIN_PACKET_FLAG_PACKET_H264_ANNEXB                     (1 << (CACHE_FILE_ITEM_FLAG_LAST + 3))
#define OUTPUT_PIN_PACKET_FLAG_PACKET_PARSED                          (1 << (CACHE_FILE_ITEM_FLAG_LAST + 4))
#define OUTPUT_PIN_PACKET_FLAG_PACKET_MOV_TEXT                        (1 << (CACHE_FILE_ITEM_FLAG_LAST + 5))
#define OUTPUT_PIN_PACKET_FLAG_PACKET_FORCED_SUBTITLE                 (1 << (CACHE_FILE_ITEM_FLAG_LAST + 6))

#define OUTPUT_PIN_PACKET_FLAG_LAST                                   (CACHE_FILE_ITEM_FLAG_LAST + 7)

#define DEMUXER_ID_UNSPECIFIED                                        UINT_MAX
#define STREAM_PID_UNSPECIFIED                                        UINT_MAX

class COutputPinPacket : public CCacheFileItem
{
public:
  // initializes a new instance of COutputPinPacket class
  COutputPinPacket(HRESULT *result);
  virtual ~COutputPinPacket(void);

  /* get methods */

  // gets start time of packet
  // @ return : packet start time or INVALID_TIME
  REFERENCE_TIME GetStartTime(void);

  // gets end time of packet
  // @ return : packet end time or INVALID_TIME
  REFERENCE_TIME GetEndTime(void);

  // gets media type
  // @return : packet media type
  AM_MEDIA_TYPE *GetMediaType(void);

  // gets demuxer ID
  // it is specified for splitter, which needs to identify output pin for specific output packet
  // @return : demuxer ID or DEMUXER_ID_UNSPECIFIED if not specified
  unsigned int GetDemuxerId(void);

  // gets stream PID
  // it is specified for splitter, which needs to identify output pin for specific output packet
  // @return : stream PID or STREAM_PID_UNSPECIFIED if not specified
  unsigned int GetStreamPid(void);

  // gets end of stream result
  // @return : S_OK if normal end of stream, error code otherwise
  HRESULT GetEndOfStreamResult(void);

  /* set methods */

  // sets discontinuity flag
  // @param discontinuity : true if discontinued, false otherwise
  void SetDiscontinuity(bool discontinuity);

  // sets sync point flag
  // @param syncPoint : true if sync point, false otherwise
  void SetSyncPoint(bool syncPoint);

  // sets end of stream
  // @param endOfStream : true if packet signalize end of stream, false otherwise
  void SetEndOfStream(bool endOfStream, HRESULT endOfStreamResult);

  // sets packet start time
  // @param startTime : packet start time to set
  void SetStartTime(REFERENCE_TIME startTime);

  // sets packet end time
  // @param endTime : packet end time to set
  void SetEndTime(REFERENCE_TIME endTime);

  // sets media type (only reference)
  // @param mediaType : the media type to set
  void SetMediaType(AM_MEDIA_TYPE *mediaType);

  // sets demuxer ID
  // @param demuxerId : the demuxer ID to set
  void SetDemuxerId(unsigned int demuxerId);

  // sets stream PID
  // @param streamPid : the stream PID to set
  void SetStreamPid(unsigned int streamPid);

  /* other methods */

  // tests if discontinuity flag is set
  // @return : true if discontinuity flag is set, false otherwise
  bool IsDiscontinuity(void);

  // tests if sync point flag is set
  // @return : true if sync point flag is set, false otherwise
  bool IsSyncPoint(void);

  // tests if packet signalize end of stream
  // @return : true if packet signalize end of stream, false otherwise
  bool IsEndOfStream(void);

  // tests if packet is parsed
  // @return : true if packet is parsed, false otherwise
  bool IsPacketParsed(void);

  // tests if packet is H264 Annex B
  // @return : true if packet is H264 Annex B, false otherwise
  bool IsH264AnnexB(void);

  // tests if packet has set OUTPUT_PIN_PACKET_FLAG_PACKET_MOV_TEXT flags
  // @return : true if packet has set OUTPUT_PIN_PACKET_FLAG_PACKET_MOV_TEXT flag, false otherwise
  bool IsPacketMovText(void);

  static const REFERENCE_TIME INVALID_TIME = _I64_MIN;

protected:
  // holds start time of packet
  REFERENCE_TIME startTime;

  // holds end time of packet
  REFERENCE_TIME endTime;

  // holds media type
  AM_MEDIA_TYPE *mediaType;

  // holds demuxer ID for splitter (it specifies to which output pin goes this packet)
  unsigned int demuxerId;

  // holds stream PID for splitter (it specifies to which output pin goes this packet)
  unsigned int streamPid;

  // holds end of stream result (S_OK if normal end of stream, error code otherwise)
  HRESULT endOfStreamResult;

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