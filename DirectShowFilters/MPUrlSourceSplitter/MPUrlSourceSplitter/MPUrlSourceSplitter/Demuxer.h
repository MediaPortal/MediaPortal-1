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
#include "OutputPinPacketCollection.h"
#include "IFilter.h"
#include "MediaPacketCollection.h"
#include "AsyncRequest.h"
#include "IOutputStream.h"
#include "IPacketDemuxer.h"
#include "CacheFile.h"
#include "PacketInputFormat.h"

#define DEMUXER_FLAG_NONE                                             0x00000000
#define DEMUXER_FLAG_FLV                                              0x00000001
#define DEMUXER_FLAG_ASF                                              0x00000002
#define DEMUXER_FLAG_MP4                                              0x00000004
#define DEMUXER_FLAG_MATROSKA                                         0x00000008
#define DEMUXER_FLAG_OGG                                              0x00000010
#define DEMUXER_FLAG_AVI                                              0x00000020
#define DEMUXER_FLAG_MPEG_TS                                          0x00000040
#define DEMUXER_FLAG_MPEG_PS                                          0x00000080
#define DEMUXER_FLAG_RM                                               0x00000200

#define DEMUXER_FLAG_ALL_CONTAINERS                                   (DEMUXER_FLAG_RM | DEMUXER_FLAG_MPEG_PS | DEMUXER_FLAG_MPEG_TS | DEMUXER_FLAG_AVI | DEMUXER_FLAG_OGG | DEMUXER_FLAG_MATROSKA | DEMUXER_FLAG_MP4 | DEMUXER_FLAG_ASF | DEMUXER_FLAG_FLV)

#define DEMUXER_FLAG_VC1_SEEN_TIMESTAMP                               0x00000400
#define DEMUXER_FLAG_VC1_CORRECTION                                   0x00000800

#define DEMUXER_FLAG_ESTIMATE_TOTAL_LENGTH                            0x00001000
// specifies if all data are received (all data from stream has been received - it doesn't mean that has been stored to file)
#define DEMUXER_FLAG_ALL_DATA_RECEIVED                                0x00002000
// specifies if total length data are received (that means that we received end of stream, but not all data - there can be still gaps in stream)
#define DEMUXER_FLAG_TOTAL_LENGTH_RECEIVED                            0x00004000
// specifies if demuxer is processing live stream or not
#define DEMUXER_FLAG_LIVE_STREAM                                      0x00008000
// specifies if filter created demuxer successfully
#define DEMUXER_FLAG_CREATED_DEMUXER                                  0x00010000
// specifies if create demuxer worker finished its work
#define DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED                   0x00020000
// specifies if real demuxing is needed (in another case are input media packets moved to output packets)
#define DEMUXER_FLAG_REAL_DEMUXING_NEEDED                             0x00040000
// specifies that received media packets contains stream in container (e.g. avi, mkv, flv, ...)
#define DEMUXER_FLAG_STREAM_IN_CONTAINER                              0x00080000
// specifies that received media packets contains demuxed packets ready for output pins
#define DEMUXER_FLAG_STREAM_IN_PACKETS                                0x00100000
// specifies that end of stream output packet is queued into output packet collection
#define DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED               0x00200000

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

//#define FLV_TIMESTAMP_MAX                                             1024

#define DEMUXER_STORE_FILE_RELOAD_SIZE                                1048576

// disabled everything - internal demuxer and also reading in seek methods
#define PAUSE_SEEK_STOP_REQUEST_DISABLE_ALL                           2
// disabled only internal demuxer, reading from seek methods is allowed
#define PAUSE_SEEK_STOP_REQUEST_DISABLE_DEMUXING                      1
// internal demuxing and reading from seek methods is allowed
#define PAUSE_SEEK_STOP_REQUEST_NONE                                  0

class CDemuxer : public IOutputStream, public IPacketDemuxer
{
public:
  enum { CMD_EXIT, CMD_PAUSE, CMD_DEMUX, CMD_CREATE_DEMUXER };

  // initializes a new instance of CDemuxer class
  // @param logger : logger for logging purposes
  // @param filter : filter
  // @param configuration : the configuration for filter/splitter
  // @param result : reference for variable, which holds result
  CDemuxer(CLogger *logger, IFilter *filter, CParameterCollection *configuration, HRESULT *result);
  ~CDemuxer(void);

  // IOutputStream interface

  // notifies output stream about stream count
  // @param streamCount : the stream count
  // @param liveStream : true if stream(s) are live, false otherwise
  // @return : S_OK if successful, false otherwise
  HRESULT SetStreamCount(unsigned int streamCount, bool liveStream);

  // pushes stream received data to filter
  // @param streamId : the stream ID to push stream received data
  // @param streamReceivedData : the stream received data to push to filter
  // @return : S_OK if successful, error code otherwise
  HRESULT PushStreamReceiveData(unsigned int streamId, CStreamReceiveData *streamReceiveData);

  // IPacketDemuxer methods

  // gets next available media packet
  // @param mediaPacket : reference to variable to store to reference to media packet
  // @return : 
  // S_OK     = media packet returned
  // S_FALSE  = no media packet available
  // negative values are error
  HRESULT GetNextMediaPacket(CMediaPacket **mediaPacket);

  // reads data from stream from specified position into buffer
  // @param position : the position in stream to start reading data
  // @param buffer : the buffer to store data
  // @param length : the size of requested data
  // @return : the length of read data, negative values are errors
  int StreamRead(int64_t position, uint8_t *buffer, int length);
  
  /* get methods */

  // gets stream collection of specified type
  // @return : stream collection of specified type
  CStreamCollection *GetStreams(CStream::StreamType type);

  // gets duration for stream
  int64_t GetDuration(void);

  // gets container format
  // @return : container format or NULL if error
  const wchar_t *GetContainerFormat(void);

  // gets output pin packet
  // @param packet : output pin packet to get
  // @return : S_OK if successful, S_FALSE if no packet, error code otherwise
  HRESULT GetOutputPinPacket(COutputPinPacket *packet);

  // gets parser stream ID
  // @return : parser stream ID
  unsigned int GetParserStreamId(void);

  // gets associated filter instance
  // @return : filter instance
  IFilter *GetFilter(void);

  // gets cache file path
  // @return : cache file path or NULL if not set
  const wchar_t *GetCacheFilePath(void);

  /* set methods */

  // sets active stream for specific stream type
  // @param streamType : the type of stream to set active stream (Video, Audio, Subpic, Unknown)
  // @param activeStreamId : the ID of active stream or ACTIVE_STREAM_NOT_SPECIFIED if no stream is specified for type of stream
  void SetActiveStream(CStream::StreamType streamType, int activeStreamId);

  // sets parser stream ID
  // @param parserStreamId : the parser stream ID to set
  void SetParserStreamId(unsigned int parserStreamId);

  // sets pause, seek or stop request flag
  // @param pauseSeekStopRequest : true if pause, seek or stop, false otherwise
  void SetPauseSeekStopRequest(bool pauseSeekStopRequest);

  // sets live stream flag
  // @param liveStream : true if processed stream is live stream, false otherwise
  void SetLiveStream(bool liveStream);

  // sets real demuxing needed flag
  // @param realDemuxingNeeded : true if real demuxing is needed, false otherwise
  void SetRealDemuxingNeeded(bool realDemuxingNeeded);

  /* other methods */

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
  bool IsRm(void);
  bool IsVc1SeenTimestamp(void);
  bool IsVc1Correction(void);

  // tests if all data were received
  // @return : true if all data received, false otherwise
  bool IsAllDataReceived(void);

  // tests if total length is received
  // @return : true if total length is received, false otherwise
  bool IsTotalLengthReceived(void);

  // tests if processing live stream
  // @return : true if processing live stream, false otherwise
  bool IsLiveStream(void);

  // tests if filter has created demuxer successfully
  // @return : true if filter created demuxer, false otherwise
  bool IsCreatedDemuxer(void);

  // tests if total length is estimation only
  // @return : true if total length is estimation, false otherwise
  bool IsEstimateTotalLength(void);

  // tests if create demuxer worker finished its work
  // @return : true if create demuxer worker finished its work, false otherwise
  bool IsCreateDemuxerWorkerFinished(void);

  // tests if real demuxing is needed
  // @return : true if real demuxing is needed, false otherwise
  bool IsRealDemuxingNeeded(void);

  // tests if has started creating demuxer
  // @return : true if started creating demuxer, false otherwise
  bool HasStartedCreatingDemuxer(void);

  // tests if end of stream output packet is queued into output packet collection
  // return : true if end of stream output packet is queued into output packet collection, false otherwise
  bool IsEndOfStreamOutputPacketQueued(void);

  // starts creating demuxer
  // @return : S_OK if successful, error code otherwise
  HRESULT StartCreatingDemuxer(void);

  // starts demuxing (demuxer MUST be created successfully)
  // @return : S_OK if successful, error code otherwise
  HRESULT StartDemuxing(void);

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

  // reports actual stream time in ms to demuxer
  // @param streamTime : the actual stream time in ms to report
  void ReportStreamTime(uint64_t streamTime);

protected:

  // holds various flags
  unsigned int flags;

  // configuration passed from filter
  CParameterCollection *configuration;

  // holds logger for logging purposes
  // its only reference to logger instance, it is not destroyed in ~CDemuxer()
  CLogger *logger;

  // holds filter instance
  IFilter *filter;

  // each demuxer has its own stream ID (it is complete stream coming from parser before demuxing)
  unsigned int parserStreamId;

  // holds stream input format (if specified)
  wchar_t *streamInputFormat;

  // holds special packet input format (only in case of DEMUXER_FLAG_STREAM_IN_PACKETS set flag)
  CPacketInputFormat *packetInputFormat;

  // holds media packet collection cache file
  CCacheFile *mediaPacketCollectionCacheFile;

  // holds streams collection for each type of stream
  CStreamCollection *streams[CStream::Unknown];
  // holds active stream index for each group (type of stream), default is ACTIVE_STREAM_NOT_SPECIFIED
  int activeStream[CStream::Unknown];

  AVFormatContext *formatContext;

  // holds container format in human-readable form
  wchar_t *containerFormat;

  // holds parse type for each stream
  enum AVStreamParseType *streamParseType;

  /*FlvTimestamp *flvTimestamps;
  bool dontChangeTimestamps;*/

  // holds if filter want to call CAMThread::CallWorker() with CMD_PAUSE, CMD_SEEK, CMD_STOP values
  // in that case demuxer do not demux input stream
  volatile unsigned int pauseSeekStopRequest;

  // holds demuxing worker handle
  HANDLE demuxingWorkerThread;
  // specifies if demuxing worker should exit
  volatile bool demuxingWorkerShouldExit;

  // holds starting position to read data for demuxerContext (for splitter)
  LONGLONG demuxerContextBufferPosition;
  // AVIOContext for demuxer (splitter)
  AVIOContext *demuxerContext;

  // collection of input media packets
  CMediaPacketCollection *mediaPacketCollection;
  // mutex for accessing media packets
  HANDLE mediaPacketMutex;

  // collection of output (not necessary demuxed - if demuxing is not needed) packets
  COutputPinPacketCollection *outputPacketCollection;
  // mutex for output packets
  HANDLE outputPacketMutex;

  // holds last received media packet time
  DWORD lastReceivedMediaPacketTime;

  // holds total length in bytes of stream from protocol
  // it can be estimation, it depends on FLAG_DEMUXER_ESTIMATE_TOTAL_LENGTH flag
  int64_t totalLength;

  // holds create demuxer thread handle
  HANDLE createDemuxerWorkerThread;
  // specifies if demuxer worker should exit
  volatile bool createDemuxerWorkerShouldExit;

  // holds demuxer read request
  CAsyncRequest *demuxerReadRequest;

  // holds demuxer read request worker thread handle
  HANDLE demuxerReadRequestWorkerThread;

  // mutex for accessing demuxer requests
  HANDLE demuxerReadRequestMutex;

  // specifies if demuxer read request worker should exit
  volatile bool demuxerReadRequestWorkerShouldExit;

  // demuxer read request ID for async requests
  unsigned int demuxerReadRequestId;

  // holds last read media packet
  unsigned int lastMediaPacket;

  // holds last reported stream time from filter
  uint64_t streamTime;

  /* methods */

  // cleans format context
  void CleanupFormatContext(void);

  static int InitParser(AVFormatContext *formatContext, AVStream *stream);
  void UpdateParserFlags(AVStream *stream);

  REFERENCE_TIME ConvertTimestampToRT(int64_t pts, int num, int den, int64_t starttime);
  int64_t ConvertRTToTimestamp(REFERENCE_TIME timestamp, int num, int den, int64_t starttime);

  HRESULT SeekByPosition(REFERENCE_TIME time, int flags);
  HRESULT SeekByTime(REFERENCE_TIME time, int flags);
  HRESULT SeekBySequenceReading(REFERENCE_TIME time, int flags);

  /* create demuxer worker methods */

  // demuxer worker thread method
  static unsigned int WINAPI CreateDemuxerWorker(LPVOID lpParam);

  // creates create demuxer worker
  // @return : S_OK if successful
  HRESULT CreateCreateDemuxerWorker(void);

  // destroys create demuxer worker
  // @return : S_OK if successful
  HRESULT DestroyCreateDemuxerWorker(void);

  /* demuxer (AVIOContext from ffmpeg) read and seek methods */

  static int DemuxerRead(void *opaque, uint8_t *buf, int buf_size);
  static int64_t DemuxerSeek(void *opaque, int64_t offset, int whence);
  int DemuxerReadPosition(int64_t position, uint8_t *buffer, int length);

  /* demuxer read request worker */

  // demuxer read request worker thread method
  static unsigned int WINAPI DemuxerReadRequestWorker(LPVOID lpParam);

  // creates demuxer read request worker
  // @return : S_OK if successful
  HRESULT CreateDemuxerReadRequestWorker(void);

  // destroys demuxer read request worker
  // @return : S_OK if successful
  HRESULT DestroyDemuxerReadRequestWorker(void);

  /* demuxing worker */

  // demuxing worker thread method
  static unsigned int WINAPI DemuxingWorker(LPVOID lpParam);

  // creates demuxing worker
  // @return : S_OK if successful
  HRESULT CreateDemuxingWorker(void);

  // destroys demuxing worker
  // @return : S_OK if successful
  HRESULT DestroyDemuxingWorker(void);

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  HRESULT GetNextPacketInternal(COutputPinPacket *packet);

  // check demuxer read request and media packet values agains not valid values
  // @param request : demuxer read request
  // @param mediaPacket : media packet
  // @param mediaPacketDataStart : the reference to variable that holds data start within media packet (if successful)
  // @param mediaPacketDataLength : the reference to variable that holds data length within media packet (if successful)
  // @param startPosition : start position of data
  // @return : S_OK if successful, error code otherwise
  HRESULT CheckValues(CAsyncRequest *request, CMediaPacket *mediaPacket, unsigned int *mediaPacketDataStart, unsigned int *mediaPacketDataLength, int64_t startPosition);

  // gets total length of stream in bytes
  // @param totalLength : reference to total length variable
  // @return : S_OK if success, VFW_S_ESTIMATED if total length is not surely known, error code if error
  HRESULT GetTotalLength(int64_t *totalLength);

  // gets available length of stream in bytes
  // @param availableLength : reference to available length variable
  // @return : S_OK if success, error code if error
  HRESULT GetAvailableLength(int64_t *availableLength);

  // retrieves the total length of the stream
  // @param total : pointer to a variable that receives the length of the stream, in bytes
  // @param available : pointer to a variable that receives the portion of the stream that is currently available, in bytes
  // @return : S_OK if success, VFW_S_ESTIMATED if values are estimates, E_UNEXPECTED if error
  HRESULT Length(int64_t *total, int64_t *available);

  // opens stream
  // @param demuxerContext : demuxer context
  // @return : S_OK if successful, error code otherwise
  HRESULT OpenStream(AVIOContext *demuxerContext);

  // initializes format context
  // @return : S_OK if successful, error code otherwise
  HRESULT InitFormatContext();

  // gets cache file path based on configuration for media packet collection
  // creates folder structure if not created
  // @return : cache file or NULL if error
  wchar_t *GetMediaPacketCollectionCacheFilePath(void);
};

#endif