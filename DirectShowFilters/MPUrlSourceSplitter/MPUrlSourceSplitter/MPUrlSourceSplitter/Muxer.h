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

#ifndef __MUXER_DEFINED
#define __MUXER_DEFINED

#include "Logger.h"
#include "OutputPinPacketCollection.h"
#include "Flags.h"
#include "StreamCollection.h"

#define MUXER_FLAG_NONE                                               FLAGS_NONE

// specifies if create demuxer worker finished its work
//#define DEMUXER_FLAG_CREATE_DEMUXER_WORKER_FINISHED                   (1 << (FLAGS_LAST + 1))

#define MUXER_FLAG_END_OF_STREAM                                      (1 << (FLAGS_LAST + 0))


#define MUXER_FLAG_LAST                                               (FLAGS_NONE + 1)

#define METHOD_CREATE_MUXER_WORKER_NAME                               L"CreateMuxerWorker()"
#define METHOD_DESTROY_MUXER_WORKER_NAME                              L"DestroyMuxerWorker()"
#define METHOD_MUXER_WORKER_NAME                                      L"MuxerWorker()"

#define METHOD_QUEUE_END_OF_STREAM_NAME                               L"QueueEndOfStream()"

class CMuxer : public CFlags
{
public:
  // initializes a new instance of CMuxer class
  // @param logger : logger for logging purposes
  // @param configuration : the configuration for filter/splitter
  // @param result : reference for variable, which holds result
  CMuxer(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CMuxer(void);

  /* get methods */

  // gets output pin packet
  // @param packet : output pin packet to get
  // @return : S_OK if successful, S_FALSE if no packet, error code otherwise
  virtual HRESULT GetOutputPinPacket(COutputPinPacket *packet);

  // gets muxer error (error which occurred while muxing and muxer worker stopped its work)
  // @return : muxer error code
  virtual HRESULT GetMuxerError(void);

  /* set methods */

  // sets video streams to muxer (only reference)
  // @param streams : the collection of video streams
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetVideoStreams(CStreamCollection *streams) = 0;

  // sets audio streams to muxer (only reference)
  // @param streams : the collection of audio streams
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetAudioStreams(CStreamCollection *streams) = 0;

  // sets subtitle streams to muxer (only reference)
  // @param streams : the collection of subtitles streams
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT SetSubtitleStreams(CStreamCollection *streams) = 0;

  /* other methods */

  // tests if filter has created demuxer successfully
  // @return : true if filter created demuxer, false otherwise
  //virtual bool IsCreatedDemuxer(void);

  // tests if muxer worker finished its work
  // @return : true if muxer worker finished its work, false otherwise
  //virtual bool IsMuxerWorkerFinished(void);

  // tests if end of stream output packet is queued into output packet collection
  // return : true if end of stream output packet is queued into output packet collection, false otherwise
  //virtual bool IsEndOfStreamOutputPacketQueued(void);

  // starts creating demuxer
  // @return : S_OK if successful, error code otherwise
  //virtual HRESULT StartCreatingDemuxer(void);

  // starts muxing (muxer MUST be created successfully)
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT StartMuxer(void);

  // requests muxer to stop muxing and flush all internal data
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT BeginFlush(void);

  // requests muxer to continue muxing
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT EndFlush(void);

  // queues output pin packet
  // @param packet : the packet to queue to output pin
  // @param timeout : the timeout in ms to queue to output pin
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueuePacket(COutputPinPacket *packet, DWORD timeout);

  // queues end of stream
  // @param endOfStreamResult : S_OK if normal end of stream, error code otherwise
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueueEndOfStream(HRESULT endOfStreamResult);

  // tests if end of stream flag is set
  // @return : true if end of stream flag is set, false otherwise
  virtual bool IsEndOfStream(void);

protected:
  // configuration passed from filter
  CParameterCollection *configuration;
  // holds logger for logging purposes
  // its only reference to logger instance, it is not destroyed in ~CDemuxer()
  CLogger *logger;

  // holds muxer worker handle
  HANDLE muxerWorkerThread;
  // specifies if muxer worker should exit
  volatile bool muxerWorkerShouldExit;
  HRESULT muxerError;

  // specifies if flushing is active
  volatile bool flushing;

  // collection of muxer packets
  COutputPinPacketCollection *muxerPacketCollection;
  // lock mutex for access to muxer packet
  HANDLE muxerPacketMutex;

  // collection of output packets
  COutputPinPacketCollection *outputPacketCollection;
  // mutex for output packets
  HANDLE outputPacketMutex;

  /* methods */

  /* muxer worker methods */

  // muxer worker thread method
  static unsigned int WINAPI MuxerWorker(LPVOID lpParam);

  // creates muxer worker
  // @return : S_OK if successful
  virtual HRESULT CreateMuxerWorker(void);

  // destroys demuxer worker
  // @return : S_OK if successful
  virtual HRESULT DestroyMuxerWorker(void);

  // internal muxer worker method
  // @return : S_OK if successful
  virtual HRESULT MuxerWorkerInternal(void) = 0;

  // gets next output pin packet
  // @param packet : pointer to output packet
  // @return : S_OK if successful, S_FALSE if no output pin packet available, error code otherwise
  //virtual HRESULT GetNextPacketInternal(COutputPinPacket *packet) = 0;
};

#endif