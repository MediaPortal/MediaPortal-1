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

#endif