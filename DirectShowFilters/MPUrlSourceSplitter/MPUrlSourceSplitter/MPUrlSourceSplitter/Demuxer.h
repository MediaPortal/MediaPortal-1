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

#ifndef __DEMUXER_DEFINED
#define __DEMUXER_DEFINED

#include "Logger.h"
#include "StreamCollection.h"
#include "OutputPinPacket.h"
#include "IFilter.h"

#define FLAG_DEMUXER_NONE                                             0x00000000
#define FLAG_DEMUXER_FLV                                              0x00000001
#define FLAG_DEMUXER_ASF                                              0x00000002
#define FLAG_DEMUXER_MP4                                              0x00000004
#define FLAG_DEMUXER_MATROSKA                                         0x00000008
#define FLAG_DEMUXER_OGG                                              0x00000010
#define FLAG_DEMUXER_AVI                                              0x00000020
#define FLAG_DEMUXER_MPEG_TS                                          0x00000040
#define FLAG_DEMUXER_MPEG_PS                                          0x00000080
#define FLAG_DEMUXER_EVO                                              0x00000100
#define FLAG_DEMUXER_RM                                               0x00000200
#define FLAG_DEMUXER_VC1_SEEN_TIMESTAMP                               0x00000400
#define FLAG_DEMUXER_VC1_CORRECTION                                   0x00000800

#define NO_SUBTITLE_PID                                               DWORD_MAX
#define FORCED_SUBTITLE_PID                                           (NO_SUBTITLE_PID - 1)
#define FORCED_SUB_STRING                                             L"Forced Subtitles (auto)"
#define ACTIVE_STREAM_NOT_SPECIFIED                                   -1

static const AVRational AV_RATIONAL_TIMEBASE = {1, AV_TIME_BASE};

//struct FlvTimestamp
//{
//  int64_t lastPacketStart;
//  int64_t lastPacketStop;
//  // value for decreasing timestamp in FLV video (we assume that all incoming streams are in sync)
//  int64_t decreaseTimestamp;
//  // decrease timestamp value must be recalculated, because it was changed by another stream
//  bool needRecalculate;
//};

#define FLV_TIMESTAMP_MAX                                             1024

class CDemuxer
{
public:
  enum StreamType { Video, Audio, Subpic, Unknown };

  // initializes a new instance of CDemuxer class
  // @param logger : logger for logging purposes
  // @param filter : filter
  // @param result : reference for variable, which holds result
  CDemuxer(CLogger *logger, IFilter *filter, HRESULT *result);
  ~CDemuxer(void);

  /* get methods */

  // gets stream collection of specified type
  // @return : stream collection of specified type
  CStreamCollection *GetStreams(StreamType type);

  // gets duration for stream
  int64_t GetDuration(void);

  // gets container format
  // @return : container format or NULL if error
  const wchar_t *GetContainerFormat(void);

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  HRESULT GetNextPacket(COutputPinPacket *packet);

  /* set methods */

  // sets active stream for specific stream type
  // @param streamType : the type of stream to set active stream (Video, Audio, Subpic, Unknown)
  // @param activeStreamId : the ID of active stream
  void SetActiveStream(StreamType streamType, int activeStreamId);

  /* other methods */

  // opens stream
  // @param demuxerContext : demuxer context
  // @param streamUrl : the stream url to open
  // @return : S_OK if successful, error code otherwise
  HRESULT OpenStream(AVIOContext *demuxerContext, const wchar_t *streamUrl);

  // tests if specific combination of flags is set
  // @param flags : the set of flags to test
  // @return : true if set of flags is set, false otherwise
  bool IsSetFlags(unsigned int flags);

  bool IsFlv(void);
  bool IsAsf(void);
  bool IsMp4(void);
  bool IsMatroska(void);
  bool IsOgg(void);
  bool IsAvi(void);
  bool IsMpegTs(void);
  bool IsMpegPs(void);
  bool IsEvo(void);
  bool IsRm(void);
  bool IsVc1SeenTimestamp(void);
  bool IsVc1Correction(void);

  // selects best video stream
  // @return : the best video stream or NULL if error
  CStream *SelectVideoStream(void);

  // selects best audio stream
  // @return : the best audio stream or NULL if error
  CStream *SelectAudioStream(void);

  // seeks to specified time
  // @param time : the time to seek in stream
  // @return : S_OK if successful, false otherwise
  HRESULT Seek(REFERENCE_TIME time);

protected:

  // holds various flags
  unsigned int flags;

  // holds logger for logging purposes
  // its only reference to logger instance, it is not destroyed in ~CDemuxer()
  CLogger *logger;

  // holds filter instance
  IFilter *filter;

  // holds streams collection for each type of stream
  CStreamCollection *streams[CDemuxer::Unknown];
  // holds active stream index for each group (type of stream), default is ACTIVE_STREAM_NOT_SPECIFIED
  int activeStream[CDemuxer::Unknown];

  AVFormatContext *formatContext;

  // holds input format in human-readable form
  wchar_t *inputFormat;

  // holds parse type for each stream
  enum AVStreamParseType *streamParseType;

  /*FlvTimestamp *flvTimestamps;
  bool dontChangeTimestamps;*/

  /* methods */

  // initializes format context
  // @param streamUrl : the stream url to open
  // @return : S_OK if successful, error code otherwise
  HRESULT InitFormatContext(const wchar_t *streamUrl);

  // cleans format context
  void CleanupFormatContext(void);

  static int InitParser(AVFormatContext *formatContext, AVStream *stream);
  void UpdateParserFlags(AVStream *stream);

  REFERENCE_TIME ConvertTimestampToRT(int64_t pts, int num, int den, int64_t starttime);
  int64_t ConvertRTToTimestamp(REFERENCE_TIME timestamp, int num, int den, int64_t starttime);

  HRESULT SeekByPosition(REFERENCE_TIME time, int flags);
  HRESULT SeekByTime(REFERENCE_TIME time, int flags);
  HRESULT SeekBySequenceReading(REFERENCE_TIME time, int flags);
};

#endif