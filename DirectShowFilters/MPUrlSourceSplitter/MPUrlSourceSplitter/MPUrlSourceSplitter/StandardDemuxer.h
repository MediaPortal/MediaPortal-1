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

#ifndef __STANDARD_DEMUXER_DEFINED
#define __STANDARD_DEMUXER_DEFINED

#include "Demuxer.h"
#include "FFmpegContext.h"
#include "CacheFile.h"
#include "StreamInformation.h"

#define STANDARD_DEMUXER_FLAG_NONE                                    DEMUXER_FLAG_NONE

#define STANDARD_DEMUXER_FLAG_FLV                                     (1 << (DEMUXER_FLAG_LAST + 0))
#define STANDARD_DEMUXER_FLAG_ASF                                     (1 << (DEMUXER_FLAG_LAST + 1))
#define STANDARD_DEMUXER_FLAG_MP4                                     (1 << (DEMUXER_FLAG_LAST + 2))
#define STANDARD_DEMUXER_FLAG_MATROSKA                                (1 << (DEMUXER_FLAG_LAST + 3))
#define STANDARD_DEMUXER_FLAG_OGG                                     (1 << (DEMUXER_FLAG_LAST + 4))
#define STANDARD_DEMUXER_FLAG_AVI                                     (1 << (DEMUXER_FLAG_LAST + 5))
#define STANDARD_DEMUXER_FLAG_MPEG_TS                                 (1 << (DEMUXER_FLAG_LAST + 6))
#define STANDARD_DEMUXER_FLAG_MPEG_PS                                 (1 << (DEMUXER_FLAG_LAST + 7))
#define STANDARD_DEMUXER_FLAG_RM                                      (1 << (DEMUXER_FLAG_LAST + 8))

#define STANDARD_DEMUXER_FLAG_ALL_CONTAINERS                          (STANDARD_DEMUXER_FLAG_RM | STANDARD_DEMUXER_FLAG_MPEG_PS | STANDARD_DEMUXER_FLAG_MPEG_TS | STANDARD_DEMUXER_FLAG_AVI | STANDARD_DEMUXER_FLAG_OGG | STANDARD_DEMUXER_FLAG_MATROSKA | STANDARD_DEMUXER_FLAG_MP4 | STANDARD_DEMUXER_FLAG_ASF | STANDARD_DEMUXER_FLAG_FLV)

#define STANDARD_DEMUXER_FLAG_VC1_SEEN_TIMESTAMP                      (1 << (DEMUXER_FLAG_LAST + 9))
#define STANDARD_DEMUXER_FLAG_VC1_CORRECTION                          (1 << (DEMUXER_FLAG_LAST + 10))

#define STANDARD_DEMUXER_FLAG_NONE                                    (DEMUXER_FLAG_LAST + 11)

#define NO_SUBTITLE_PID                                               DWORD_MAX
#define FORCED_SUBTITLE_PID                                           (NO_SUBTITLE_PID - 1)
#define FORCED_SUB_STRING                                             L"Forced Subtitles (auto)"
#define ACTIVE_STREAM_NOT_SPECIFIED                                   -1

#define METHOD_SEEK_NAME                                              L"Seek()"
#define METHOD_SEEK_BY_TIME_NAME                                      L"SeekByTime()"
#define METHOD_SEEK_BY_POSITION_NAME                                  L"SeekByPosition()"
#define METHOD_SEEK_BY_SEQUENCE_READING_NAME                          L"SeekBySequenceReading()"

static const AVRational AV_RATIONAL_TIMEBASE = {1, AV_TIME_BASE};

struct FlvSeekPosition
{
  int64_t time;
  int64_t position;
};

#define FLV_SEEK_UPPER_BOUNDARY                                       1
#define FLV_SEEK_LOWER_BOUNDARY                                       0
#define FLV_BOUNDARIES_COUNT                                          2
#define FLV_SEEKING_BUFFER_SIZE                                       32 * 1024   // size of buffer to read from stream
#define FLV_SEEKING_TOTAL_BUFFER_SIZE                                 32 * FLV_SEEKING_BUFFER_SIZE // total buffer size for reading from stream (1 MB)
#define FLV_NO_SEEK_DIFFERENCE_TIME                                   10000000    // time in DSHOW_TIME_BASE units between lower FLV seeking boundary time and seek pts to ignore seeking

#define DEMUXER_READ_BUFFER_SIZE								                      32768

#define countof(array) (sizeof(array) / sizeof(array[0]))

class CStandardDemuxer : public CDemuxer, public IFFmpegLog
{
public:
  // initializes a new instance of CStandardDemuxer class
  // @param logger : logger for logging purposes
  // @param filter : filter
  // @param configuration : the configuration for filter/splitter
  // @param result : reference for variable, which holds result
  CStandardDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration);
  virtual ~CStandardDemuxer(void);

  /* CDemuxer methods */

  // gets duration for stream
  virtual int64_t GetDuration(void);

  // gets position for specified stream time (in ms)
  // @param streamTime : the stream time (in ms) to get position in stream
  // @return : the position in stream
  virtual uint64_t GetPositionForStreamTime(uint64_t streamTime);

  // IFFmpegLog interface

  virtual bool FFmpegLog(CFFmpegLogger *ffmpegLogger, CFFmpegContext *context, void *ffmpegPtr, int ffmpegLogLevel, const char *ffmpegFormat, va_list ffmpegList);
  
  /* get methods */

  // gets stream collection of specified type
  // @return : stream collection of specified type
  virtual CStreamCollection *GetStreams(CStream::StreamType type);

  // gets container format
  // @return : container format or NULL if error
  virtual const wchar_t *GetContainerFormat(void);

  // gets active stream for specific stream type
  // @param streamType : the type of stream to set active stream (Video, Audio, Subpic, Unknown)
  // @return : the active stream or NULL if no stream is active for type of stream
  virtual CStream *GetActiveStream(CStream::StreamType streamType);

  /* set methods */

  // sets active stream for specific stream type
  // @param streamType : the type of stream to set active stream (Video, Audio, Subpic, Unknown)
  // @param activeStreamId : the ID of active stream or ACTIVE_STREAM_NOT_SPECIFIED if no stream is specified for type of stream
  virtual void SetActiveStream(CStream::StreamType streamType, int activeStreamId);

  // sets stream information to demuxer
  // @param streamInformation : the stream information reported by parser or protocol
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetStreamInformation(CStreamInformation *streamInformation) = 0;

  /* other methods */

  // selects best video stream
  // @return : the best video stream or NULL if error
  virtual CStream *SelectVideoStream(void);

  // selects best audio stream
  // @return : the best audio stream or NULL if error
  virtual CStream *SelectAudioStream(void);

  // seeks to specified time
  // @param time : the time to seek in stream
  // @return : S_OK if successful, false otherwise
  virtual HRESULT Seek(REFERENCE_TIME time);

protected:

  // holds container format in human-readable form
  wchar_t *containerFormat;
  AVFormatContext *formatContext;
  // AVIOContext for demuxer (splitter)
  AVIOContext *demuxerContext;
  // holds FFMpeg context
  CFFmpegContext *ffmpegContext;

  // holds streams collection for each type of stream
  CStreamCollection *streams[CStream::Unknown];
  // holds active stream index for each group (type of stream), default is ACTIVE_STREAM_NOT_SPECIFIED
  int activeStream[CStream::Unknown];

  /* methods */

  // creates demuxer
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CreateDemuxerInternal(void);

  // cleans up demuxer
  virtual void CleanupDemuxerInternal(void);

  // demuxing worker internal method executed from DemuxingWorker() method
  virtual void DemuxingWorkerInternal(void);

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  virtual HRESULT GetNextPacketInternal(COutputPinPacket *packet);

  REFERENCE_TIME ConvertTimestampToRT(int64_t pts, int num, int den, int64_t starttime);
  int64_t ConvertRTToTimestamp(REFERENCE_TIME timestamp, int num, int den, int64_t starttime);

  // cleans format context
  virtual void CleanupFormatContext(void);

  // gets AV packet PTS
  // @param stream : the AV stream
  // @param packet : the AV packet to get PTS
  // @return : the PTS of AV packet
  virtual int64_t GetPacketPts(AVStream *stream, AVPacket *packet) = 0;

  // gets AV packet DTS
  // @param stream : the AV stream
  // @param packet : the AV packet to get DTS
  // @return : the DTS of AV packet
  virtual int64_t GetPacketDts(AVStream *stream, AVPacket *packet) = 0;

  virtual bool IsFlv(void);
  virtual bool IsAsf(void);
  virtual bool IsMp4(void);
  virtual bool IsMatroska(void);
  virtual bool IsOgg(void);
  virtual bool IsAvi(void);
  virtual bool IsMpegTs(void);
  virtual bool IsMpegPs(void);
  virtual bool IsRm(void);
  virtual bool IsVc1SeenTimestamp(void);
  virtual bool IsVc1Correction(void);

  virtual HRESULT SeekByPosition(REFERENCE_TIME time, int flags);
  virtual HRESULT SeekByTime(REFERENCE_TIME time, int flags);
  virtual HRESULT SeekBySequenceReading(REFERENCE_TIME time, int flags);

  virtual int InitParser(AVFormatContext *formatContext, AVStream *stream);
  virtual void UpdateParserFlags(AVStream *stream);

  /* demuxer (AVIOContext from ffmpeg) read and seek methods */

  static int DemuxerRead(void *opaque, uint8_t *buf, int buf_size);
  static int64_t DemuxerSeek(void *opaque, int64_t offset, int whence);

  // opens stream
  // @param demuxerContext : demuxer context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT OpenStream(AVIOContext *demuxerContext) = 0;

  // initializes format context
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT InitFormatContext(void);
};

#endif