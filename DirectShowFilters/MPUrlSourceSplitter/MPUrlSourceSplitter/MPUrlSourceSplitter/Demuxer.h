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
#include "IDemuxerOwner.h"
#include "OutputPinPacketCollection.h"
#include "CacheFile.h"
#include "Flags.h"

#define DEMUXER_FLAG_NONE                                             FLAGS_NONE

// specifies if filter created demuxer successfully
#define DEMUXER_FLAG_CREATED_DEMUXER                                  (1 << (FLAGS_LAST + 0))
// specifies if create demuxer worker finished its work
#define DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED                   (1 << (FLAGS_LAST + 1))
// specifies that end of stream output packet is queued into output packet collection
#define DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED               (1 << (FLAGS_LAST + 2))
#define DEMUXER_FLAG_PENDING_DISCONTINUITY                            (1 << (FLAGS_LAST + 3))
#define DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT                (1 << (FLAGS_LAST + 4))

// disable demuxing, but it's not guaranteed that we are in DemuxingWorker() method
// e.g. we can be in DemuxerReadPosition() until is demuxing allowed or is requested to return to demuxing worker
#define DEMUXER_FLAG_DISABLE_DEMUXING                                 (1 << (FLAGS_LAST + 5))
// disable demuxing, it's guaranteed that we are in DemuxingWorker(), in that case can be FFmpeg confused
// demuxing worker notifies that thread is in DemuxingWorker() method with clearing DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER
// flag and setting DEMUXER_FLAG_DISABLE_DEMUXING flag
#define DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER  (1 << (FLAGS_LAST + 6))
// disable demuxing, it's guaranteed that we are in DemuxingWorker(), but we safely return (we read data if they were requested)
// demuxing worker notifies that thread is in DemuxingWorker() method with clearing DEMUXER_FLAG_DISABLE_DEMUXING_WITH_SAFE_RETURN_TO_DEMUXING_WORKER
// flag and setting DEMUXER_FLAG_DISABLE_DEMUXING flag
#define DEMUXER_FLAG_DISABLE_DEMUXING_WITH_SAFE_RETURN_TO_DEMUXING_WORKER  (1 << (FLAGS_LAST + 7))
// disable any reading (in seek or demuxing methods)
#define DEMUXER_FLAG_DISABLE_READING                                  (1 << (FLAGS_LAST + 8))

#define DEMUXER_FLAG_LAST                                             (FLAGS_NONE + 9)

#define METHOD_DEMUXER_MESSAGE_FORMAT                                 L"%s: %s: stream %u, %s"
#define METHOD_DEMUXER_START_FORMAT                                   L"%s: %s: stream %u, Start"
#define METHOD_DEMUXER_END_FORMAT                                     L"%s: %s: stream %u, End"
#define METHOD_DEMUXER_END_FAIL_FORMAT                                L"%s: %s: stream %u, End, Fail"
#define METHOD_DEMUXER_END_FAIL_HRESULT_FORMAT                        L"%s: %s: stream %u, End, Fail, result: 0x%08X"

#define METHOD_CREATE_CREATE_DEMUXER_WORKER_NAME                      L"CreateCreateDemuxerWorker()"
#define METHOD_DESTROY_CREATE_DEMUXER_WORKER_NAME                     L"DestroyCreateDemuxerWorker()"
#define METHOD_CREATE_DEMUXER_WORKER_NAME                             L"CreateDemuxerWorker()"

#define METHOD_CREATE_DEMUXING_WORKER_NAME                            L"CreateDemuxingWorker()"
#define METHOD_DESTROY_DEMUXING_WORKER_NAME                           L"DestroyDemuxingWorker()"
#define METHOD_DEMUXING_WORKER_NAME                                   L"DemuxingWorker()"

#define METHOD_GET_NEXT_PACKET_INTERNAL_NAME                          L"GetNextPacketInternal()"

#define METHOD_DEMUXER_READ_POSITION_NAME                             L"DemuxerReadPosition()"

class CDemuxer : public CFlags
{
public:
  enum { CMD_EXIT, CMD_PAUSE, CMD_DEMUX, CMD_CREATE_DEMUXER };

  // initializes a new instance of CDemuxer class
  // @param logger : logger for logging purposes
  // @param filter : filter
  // @param configuration : the configuration for filter/splitter
  // @param result : reference for variable, which holds result
  CDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration);
  virtual ~CDemuxer(void);

  /* get methods */

  // gets duration for stream
  virtual int64_t GetDuration(void) = 0;

  // gets output pin packet
  // @param packet : output pin packet to get
  // @return : S_OK if successful, S_FALSE if no packet, error code otherwise
  virtual HRESULT GetOutputPinPacket(COutputPinPacket *packet);

  // gets demuxer ID
  // @return : demuxer ID
  virtual unsigned int GetDemuxerId(void);

  // gets associated filter instance
  // @return : filter instance
  virtual IDemuxerOwner *GetDemuxerOwner(void);

  // gets create demuxer error (error which occurred while creating demuxer and demuxer worker stopped its work)
  // @return : create demuxer error code
  virtual HRESULT GetCreateDemuxerError(void);

  /* set methods */

  // sets demuxer ID
  // @param demuxerId : the demuxer ID to set
  virtual void SetDemuxerId(unsigned int demuxerId);

  // sets pause, seek or stop request flag
  // @param pauseSeekStopRequest : true if pause, seek or stop, false otherwise
  virtual void SetPauseSeekStopRequest(bool pauseSeekStopRequest);

  /* other methods */

  // tests if filter has created demuxer successfully
  // @return : true if filter created demuxer, false otherwise
  virtual bool IsCreatedDemuxer(void);

  // tests if create demuxer worker finished its work
  // @return : true if create demuxer worker finished its work, false otherwise
  virtual bool IsCreateDemuxerWorkerFinished(void);

  // tests if has started creating demuxer
  // @return : true if started creating demuxer, false otherwise
  virtual bool HasStartedCreatingDemuxer(void);

  // tests if end of stream output packet is queued into output packet collection
  // return : true if end of stream output packet is queued into output packet collection, false otherwise
  virtual bool IsEndOfStreamOutputPacketQueued(void);

  // starts creating demuxer
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT StartCreatingDemuxer(void);

  // starts demuxing (demuxer MUST be created successfully)
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT StartDemuxing(void);

  // gets position for specified stream time (in ms)
  // @param streamTime : the stream time (in ms) to get position in stream
  // @return : the position in stream
  virtual uint64_t GetPositionForStreamTime(uint64_t streamTime) = 0;

protected:
  // configuration passed from filter
  CParameterCollection *configuration;
  // holds logger for logging purposes
  // its only reference to logger instance, it is not destroyed in ~CDemuxer()
  CLogger *logger;
  // holds filter instance
  IDemuxerOwner *filter;

  // each demuxer has its own ID
  unsigned int demuxerId;

  // holds demuxing worker handle
  HANDLE demuxingWorkerThread;
  // specifies if demuxing worker should exit
  volatile bool demuxingWorkerShouldExit;

  // holds starting position to read data for demuxerContext (for splitter)
  int64_t demuxerContextBufferPosition;
  unsigned int demuxerContextRequestId;

  // collection of output (not necessary demuxed - if demuxing is not needed) packets
  COutputPinPacketCollection *outputPacketCollection;
  // mutex for output packets
  HANDLE outputPacketMutex;

  // holds create demuxer thread handle
  HANDLE createDemuxerWorkerThread;
  // specifies if demuxer worker should exit
  volatile bool createDemuxerWorkerShouldExit;
  // holds create demuxer error
  HRESULT createDemuxerError;

  /* methods */

  virtual HRESULT DemuxerReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags);

  /* create demuxer worker methods */

  // demuxer worker thread method
  static unsigned int WINAPI CreateDemuxerWorker(LPVOID lpParam);

  // creates create demuxer worker
  // @return : S_OK if successful
  virtual HRESULT CreateCreateDemuxerWorker(void);

  // destroys create demuxer worker
  // @return : S_OK if successful
  virtual HRESULT DestroyCreateDemuxerWorker(void);

  // creates demuxer
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT CreateDemuxerInternal(void) = 0;

  // cleans up demuxer
  virtual void CleanupDemuxerInternal(void) = 0;

  /* demuxing worker */

  // demuxing worker thread method
  static unsigned int WINAPI DemuxingWorker(LPVOID lpParam);

  // creates demuxing worker
  // @return : S_OK if successful
  virtual HRESULT CreateDemuxingWorker(void);

  // destroys demuxing worker
  // @return : S_OK if successful
  virtual HRESULT DestroyDemuxingWorker(void);

  // demuxing worker internal method executed from DemuxingWorker() method
  virtual void DemuxingWorkerInternal(void) = 0;

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  virtual HRESULT GetNextPacketInternal(COutputPinPacket *packet) = 0;
};


//#include "FFmpegContext.h"
////#include "MediaPacketCollection.h"
////#include "IPacketDemuxer.h"
//#include "CacheFile.h"
////#include "PacketInputFormat.h"
////#include "StreamInformation.h"
//
//#define STANDARD_DEMUXER_FLAG_NONE                                    DEMUXER_FLAG_NONE
//
////#define STANDARD_DEMUXER_FLAG_FLV                                     (1 << (DEMUXER_FLAG_LAST + 0))
////#define STANDARD_DEMUXER_FLAG_ASF                                     (1 << (DEMUXER_FLAG_LAST + 1))
////#define STANDARD_DEMUXER_FLAG_MP4                                     (1 << (DEMUXER_FLAG_LAST + 2))
////#define STANDARD_DEMUXER_FLAG_MATROSKA                                (1 << (DEMUXER_FLAG_LAST + 3))
////#define STANDARD_DEMUXER_FLAG_OGG                                     (1 << (DEMUXER_FLAG_LAST + 4))
////#define STANDARD_DEMUXER_FLAG_AVI                                     (1 << (DEMUXER_FLAG_LAST + 5))
////#define STANDARD_DEMUXER_FLAG_MPEG_TS                                 (1 << (DEMUXER_FLAG_LAST + 6))
////#define STANDARD_DEMUXER_FLAG_MPEG_PS                                 (1 << (DEMUXER_FLAG_LAST + 7))
////#define STANDARD_DEMUXER_FLAG_RM                                      (1 << (DEMUXER_FLAG_LAST + 8))
////
////#define STANDARD_DEMUXER_FLAG_ALL_CONTAINERS                          (DEMUXER_FLAG_RM | DEMUXER_FLAG_MPEG_PS | DEMUXER_FLAG_MPEG_TS | DEMUXER_FLAG_AVI | DEMUXER_FLAG_OGG | DEMUXER_FLAG_MATROSKA | DEMUXER_FLAG_MP4 | DEMUXER_FLAG_ASF | DEMUXER_FLAG_FLV)
////
////#define DEMUXER_FLAG_VC1_SEEN_TIMESTAMP                               (1 << (FLAGS_LAST + 9))
////#define DEMUXER_FLAG_VC1_CORRECTION                                   (1 << (FLAGS_LAST + 10))
//
////// specifies if filter created demuxer successfully
////#define DEMUXER_FLAG_CREATED_DEMUXER                                  (1 << (FLAGS_LAST + 11))
////// specifies if create demuxer worker finished its work
////#define DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED                   (1 << (FLAGS_LAST + 12))
////// specifies if real demuxing is needed (in another case are input media packets moved to output packets)
////#define DEMUXER_FLAG_REAL_DEMUXING_NEEDED                             (1 << (FLAGS_LAST + 13))
////// specifies that received media packets contains stream in container (e.g. avi, mkv, flv, ...)
////#define DEMUXER_FLAG_STREAM_IN_CONTAINER                              (1 << (FLAGS_LAST + 14))
////// specifies that received media packets contains demuxed packets ready for output pins
////#define DEMUXER_FLAG_STREAM_IN_PACKETS                                (1 << (FLAGS_LAST + 15))
////// specifies that end of stream output packet is queued into output packet collection
////#define DEMUXER_FLAG_END_OF_STREAM_OUTPUT_PACKET_QUEUED               (1 << (FLAGS_LAST + 16))
////#define DEMUXER_FLAG_PENDING_DISCONTINUITY                            (1 << (FLAGS_LAST + 17))
////#define DEMUXER_FLAG_PENDING_DISCONTINUITY_WITH_REPORT                (1 << (FLAGS_LAST + 18))
////
////// disable demuxing, but it's not guaranteed that we are in DemuxingWorker() method
////// e.g. we can be in DemuxerReadPosition() until is demuxing allowed or is requested to return to demuxing worker
////#define DEMUXER_FLAG_DISABLE_DEMUXING                                 (1 << (FLAGS_LAST + 19))
////// disable demuxing, it's guaranteed that we are in DemuxingWorker(), in that case can be FFmpeg confused
////// demuxing worker notifies that thread is in DemuxingWorker() method with clearing DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER
////// flag and setting DEMUXER_FLAG_DISABLE_DEMUXING flag
////#define DEMUXER_FLAG_DISABLE_DEMUXING_WITH_RETURN_TO_DEMUXING_WORKER  (1 << (FLAGS_LAST + 20))
////// disable demuxing, it's guaranteed that we are in DemuxingWorker(), but we safely return (we read data if they were requested)
////// demuxing worker notifies that thread is in DemuxingWorker() method with clearing DEMUXER_FLAG_DISABLE_DEMUXING_WITH_SAFE_RETURN_TO_DEMUXING_WORKER
////// flag and setting DEMUXER_FLAG_DISABLE_DEMUXING flag
////#define DEMUXER_FLAG_DISABLE_DEMUXING_WITH_SAFE_RETURN_TO_DEMUXING_WORKER  (1 << (FLAGS_LAST + 21))
////// disable any reading (in seek or demuxing methods)
////#define DEMUXER_FLAG_DISABLE_READING                                  (1 << (FLAGS_LAST + 22))
//
//#define STANDARD_DEMUXER_FLAG_NONE                                    (DEMUXER_FLAG_LAST + 0)
//
//#define NO_SUBTITLE_PID                                               DWORD_MAX
//#define FORCED_SUBTITLE_PID                                           (NO_SUBTITLE_PID - 1)
//#define FORCED_SUB_STRING                                             L"Forced Subtitles (auto)"
//#define ACTIVE_STREAM_NOT_SPECIFIED                                   -1
//
//static const AVRational AV_RATIONAL_TIMEBASE = {1, AV_TIME_BASE};
//
////struct FlvTimestamp
////{
////  int64_t lastPacketStart;
////  int64_t lastPacketStop;
////  // value for decreasing timestamp in FLV video (we assume that all incoming streams are in sync)
////  int64_t decreaseTimestamp;
////  // decrease timestamp value must be recalculated, because it was changed by another stream
////  bool needRecalculate;
////};
//
////#define FLV_TIMESTAMP_MAX                                             1024
//
//class CStandardDemuxer : public CDemuxer, public IFFmpegLog //, public IPacketDemuxer, public IFFmpegLog
//{
//public:
//  //enum { CMD_EXIT, CMD_PAUSE, CMD_DEMUX, CMD_CREATE_DEMUXER };
//
//  // initializes a new instance of CStandardDemuxer class
//  // @param logger : logger for logging purposes
//  // @param filter : filter
//  // @param configuration : the configuration for filter/splitter
//  // @param result : reference for variable, which holds result
//  CStandardDemuxer(HRESULT *result, CLogger *logger, IDemuxerOwner *filter, CParameterCollection *configuration);
//  virtual ~CStandardDemuxer(void);
//
//  //// IPacketDemuxer interface
//
//  //// gets next available media packet
//  //// @param mediaPacket : reference to variable to store to reference to media packet
//  //// @param flags : the flags
//  //// @return : 
//  //// S_OK     = media packet returned
//  //// S_FALSE  = no media packet available
//  //// negative values are error
//  //HRESULT GetNextMediaPacket(CMediaPacket **mediaPacket, uint64_t flags);
//
//  //// reads data from stream from specified position into buffer
//  //// @param position : the position in stream to start reading data
//  //// @param buffer : the buffer to store data
//  //// @param length : the size of requested data
//  //// @param flags : the flags
//  //// @return : the length of read data, negative values are errors
//  //int StreamReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags);
//
//  // IFFmpegLog interface
//
//  virtual bool FFmpegLog(CFFmpegLogger *ffmpegLogger, CFFmpegContext *context, void *ffmpegPtr, int ffmpegLogLevel, const char *ffmpegFormat, va_list ffmpegList);
//  
//  /* get methods */
//
//  //// gets stream collection of specified type
//  //// @return : stream collection of specified type
//  virtual CStreamCollection *GetStreams(CStream::StreamType type);
//
//  //// gets duration for stream
//  //int64_t GetDuration(void);
//
//  // gets container format
//  // @return : container format or NULL if error
//  virtual const wchar_t *GetContainerFormat(void);
//
//  //// gets output pin packet
//  //// @param packet : output pin packet to get
//  //// @return : S_OK if successful, S_FALSE if no packet, error code otherwise
//  //HRESULT GetOutputPinPacket(COutputPinPacket *packet);
//
//  //// gets demuxer ID
//  //// @return : demuxer ID
//  //unsigned int GetDemuxerId(void);
//
//  //// gets associated filter instance
//  //// @return : filter instance
//  //IDemuxerOwner *GetDemuxerOwner(void);
//
//  //// gets create demuxer error (error which occurred while creating demuxer and demuxer worker stopped its work)
//  //// @return : create demuxer error code
//  //HRESULT GetCreateDemuxerError(void);
//
//  // gets active stream for specific stream type
//  // @param streamType : the type of stream to set active stream (Video, Audio, Subpic, Unknown)
//  // @return : the active stream or NULL if no stream is active for type of stream
//  virtual CStream *GetActiveStream(CStream::StreamType streamType);
//
//  /* set methods */
//
//  // sets active stream for specific stream type
//  // @param streamType : the type of stream to set active stream (Video, Audio, Subpic, Unknown)
//  // @param activeStreamId : the ID of active stream or ACTIVE_STREAM_NOT_SPECIFIED if no stream is specified for type of stream
//  virtual void SetActiveStream(CStream::StreamType streamType, int activeStreamId);
//
//  //// sets demuxer ID
//  //// @param demuxerId : the demuxer ID to set
//  //void SetDemuxerId(unsigned int demuxerId);
//
//  //// sets pause, seek or stop request flag
//  //// @param pauseSeekStopRequest : true if pause, seek or stop, false otherwise
//  //void SetPauseSeekStopRequest(bool pauseSeekStopRequest);
//
//  //// sets real demuxing needed flag
//  //// @param realDemuxingNeeded : true if real demuxing is needed, false otherwise
//  //void SetRealDemuxingNeeded(bool realDemuxingNeeded);
//
//  //// sets stream information to demuxer
//  //// @param streamInformation : the stream information reported by parser or protocol
//  //// @return : S_OK if successful, error code otherwise
//  virtual HRESULT SetStreamInformation(CStreamInformation *streamInformation);
//
//  /* other methods */
//
//  //bool IsFlv(void);
//  //bool IsAsf(void);
//  //bool IsMp4(void);
//  //bool IsMatroska(void);
//  //bool IsOgg(void);
//  //bool IsAvi(void);
//  //bool IsMpegTs(void);
//  //bool IsMpegPs(void);
//  //bool IsRm(void);
//  //bool IsVc1SeenTimestamp(void);
//  //bool IsVc1Correction(void);
//
//  //// tests if filter has created demuxer successfully
//  //// @return : true if filter created demuxer, false otherwise
//  //bool IsCreatedDemuxer(void);
//
//  //// tests if create demuxer worker finished its work
//  //// @return : true if create demuxer worker finished its work, false otherwise
//  //bool IsCreateDemuxerWorkerFinished(void);
//
//  //// tests if real demuxing is needed
//  //// @return : true if real demuxing is needed, false otherwise
//  //bool IsRealDemuxingNeeded(void);
//
//  //// tests if has started creating demuxer
//  //// @return : true if started creating demuxer, false otherwise
//  //bool HasStartedCreatingDemuxer(void);
//
//  //// tests if end of stream output packet is queued into output packet collection
//  //// return : true if end of stream output packet is queued into output packet collection, false otherwise
//  //bool IsEndOfStreamOutputPacketQueued(void);
//
//  //// starts creating demuxer
//  //// @return : S_OK if successful, error code otherwise
//  //HRESULT StartCreatingDemuxer(void);
//
//  //// starts demuxing (demuxer MUST be created successfully)
//  //// @return : S_OK if successful, error code otherwise
//  //HRESULT StartDemuxing(void);
//
//  // selects best video stream
//  // @return : the best video stream or NULL if error
//  virtual CStream *SelectVideoStream(void);
//
//  // selects best audio stream
//  // @return : the best audio stream or NULL if error
//  virtual CStream *SelectAudioStream(void);
//
//  // seeks to specified time
//  // @param time : the time to seek in stream
//  // @return : S_OK if successful, false otherwise
//  virtual HRESULT Seek(REFERENCE_TIME time);
//
//  //// gets position for specified stream time (in ms)
//  //// @param streamTime : the stream time (in ms) to get position in stream
//  //// @return : the position in stream
//  //uint64_t GetPositionForStreamTime(uint64_t streamTime);
//
//protected:
//  //// configuration passed from filter
//  //CParameterCollection *configuration;
//  //// holds logger for logging purposes
//  //// its only reference to logger instance, it is not destroyed in ~CDemuxer()
//  //CLogger *logger;
//  //// holds filter instance
//  //IDemuxerOwner *filter;
//  //// holds FFMpeg context
//  //CFFmpegContext *ffmpegContext;
//
//  //// each demuxer has its own ID
//  //unsigned int demuxerId;
//
//  //// holds stream input format (if specified)
//  //wchar_t *streamInputFormat;
//  //// holds special packet input format (only in case of DEMUXER_FLAG_STREAM_IN_PACKETS set flag)
//  //CPacketInputFormat *packetInputFormat;
//
//  //// holds streams collection for each type of stream
//  //CStreamCollection *streams[CStream::Unknown];
//  //// holds active stream index for each group (type of stream), default is ACTIVE_STREAM_NOT_SPECIFIED
//  //int activeStream[CStream::Unknown];
//
//  //AVFormatContext *formatContext;
//
//  //// holds container format in human-readable form
//  //wchar_t *containerFormat;
//
//  ///*FlvTimestamp *flvTimestamps;
//  //bool dontChangeTimestamps;*/
//
//  //// holds demuxing worker handle
//  //HANDLE demuxingWorkerThread;
//  //// specifies if demuxing worker should exit
//  //volatile bool demuxingWorkerShouldExit;
//
//  //// holds starting position to read data for demuxerContext (for splitter)
//  //int64_t demuxerContextBufferPosition;
//  //unsigned int demuxerContextRequestId;
//  //// AVIOContext for demuxer (splitter)
//  //AVIOContext *demuxerContext;
//
//  //// collection of output (not necessary demuxed - if demuxing is not needed) packets
//  //COutputPinPacketCollection *outputPacketCollection;
//  //// mutex for output packets
//  //HANDLE outputPacketMutex;
//
//  //// holds create demuxer thread handle
//  //HANDLE createDemuxerWorkerThread;
//  //// specifies if demuxer worker should exit
//  //volatile bool createDemuxerWorkerShouldExit;
//  //// holds create demuxer error
//  //HRESULT createDemuxerError;
//
//  ///* methods */
//
//  //// cleans format context
//  //void CleanupFormatContext(void);
//
//  //static int InitParser(AVFormatContext *formatContext, AVStream *stream);
//  //void UpdateParserFlags(AVStream *stream);
//
//  //REFERENCE_TIME ConvertTimestampToRT(int64_t pts, int num, int den, int64_t starttime);
//  //int64_t ConvertRTToTimestamp(REFERENCE_TIME timestamp, int num, int den, int64_t starttime);
//
//  //HRESULT SeekByPosition(REFERENCE_TIME time, int flags);
//  //HRESULT SeekByTime(REFERENCE_TIME time, int flags);
//  //HRESULT SeekBySequenceReading(REFERENCE_TIME time, int flags);
//
//  ///* create demuxer worker methods */
//
//  //// demuxer worker thread method
//  //static unsigned int WINAPI CreateDemuxerWorker(LPVOID lpParam);
//
//  //// creates create demuxer worker
//  //// @return : S_OK if successful
//  //HRESULT CreateCreateDemuxerWorker(void);
//
//  //// destroys create demuxer worker
//  //// @return : S_OK if successful
//  //HRESULT DestroyCreateDemuxerWorker(void);
//
//  ///* demuxer (AVIOContext from ffmpeg) read and seek methods */
//
//  //static int DemuxerRead(void *opaque, uint8_t *buf, int buf_size);
//  //static int64_t DemuxerSeek(void *opaque, int64_t offset, int whence);
//  //HRESULT DemuxerReadPosition(int64_t position, uint8_t *buffer, int length, uint64_t flags);
//
//  ///* demuxing worker */
//
//  //// demuxing worker thread method
//  //static unsigned int WINAPI DemuxingWorker(LPVOID lpParam);
//
//  //// creates demuxing worker
//  //// @return : S_OK if successful
//  //HRESULT CreateDemuxingWorker(void);
//
//  //// destroys demuxing worker
//  //// @return : S_OK if successful
//  //HRESULT DestroyDemuxingWorker(void);
//
//  //// gets next output pin packet
//  //// @param packet : pointer to output packet
//  //// @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
//  //HRESULT GetNextPacketInternal(COutputPinPacket *packet);
//
//  //// opens stream
//  //// @param demuxerContext : demuxer context
//  //// @return : S_OK if successful, error code otherwise
//  //HRESULT OpenStream(AVIOContext *demuxerContext);
//
//  //// initializes format context
//  //// @return : S_OK if successful, error code otherwise
//  //HRESULT InitFormatContext();
//};

#endif