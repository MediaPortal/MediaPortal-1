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

#ifndef __MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_DEFINED
#define __MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_DEFINED

#include "Logger.h"
#include "ParameterCollection.h"
#include "OutputPinPacketCollection.h"
#include "MediaTypeCollection.h"
#include "CacheFile.h"
#include "DumpFile.h"
#include "Flags.h"

#include <streams.h>

#define OUTPUT_PIN_BUFFERS_RECOMMENDED                                32
#define OUTPUT_PIN_BUFFERS_LENGTH_RECOMMENDED                         524288

#define MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_NONE                   FLAGS_NONE

#define MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_END_OF_STREAM          (1 << (FLAGS_LAST + 0))
#define MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_DUMP_DATA              (1 << (FLAGS_LAST + 1))

#define MP_URL_SOURCE_SPLITTER_OUTPUT_PIN_FLAG_LAST                   (FLAGS_LAST + 2)

#define METHOD_GET_MEDIA_TYPE_NAME                                    L"GetMediaType()"
#define METHOD_CONNECT_NAME                                           L"Connect()"
#define METHOD_DECIDE_ALLOCATOR_NAME                                  L"DecideAllocator()"
#define METHOD_DECIDE_BUFFER_SIZE_NAME                                L"DecideBufferSize()"
#define METHOD_ACTIVE_NAME                                            L"Active()"
#define METHOD_INACTIVE_NAME                                          L"Inactive()"
#define METHOD_CHECK_MEDIA_TYPE_NAME                                  L"CheckMediaType()"

#define METHOD_THREAD_PROC_NAME                                       L"ThreadProc()"
#define METHOD_QUEUE_END_OF_STREAM_NAME                               L"QueueEndOfStream()"

#define METHOD_PIN_MESSAGE_FORMAT                                     L"%s: %s: pin '%s', %s"
#define METHOD_PIN_START_FORMAT                                       L"%s: %s: pin '%s', Start"
#define METHOD_PIN_END_FORMAT                                         L"%s: %s: pin '%s', End"
#define METHOD_PIN_END_FAIL_RESULT_FORMAT                             L"%s: %s: pin '%s', End, Fail, result: 0x%08X"

class CMPUrlSourceSplitterOutputPin : public CBaseOutputPin, public CFlags, protected CAMThread
{
public:
  CMPUrlSourceSplitterOutputPin(LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, CLogger *logger, CParameterCollection *parameters, CMediaTypeCollection *mediaTypes);
  virtual ~CMPUrlSourceSplitterOutputPin();

  DECLARE_IUNKNOWN;
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // CBasePin

  // determines if the pin accepts a specific media type
  // @param pmt : pointer to a CMediaType object that contains the proposed media type
  // @return : S_OK if media type is acceptable, error code otherwise
  virtual HRESULT CheckMediaType(const CMediaType* pmt);

  // retrieves a preferred media type, by index value
  // @param iPosition : zero-based index value
  // @param pMediaType : pointer to a CMediaType object that receives the media type
  // @return :
  // S_OK if successful
  // VFW_S_NO_MORE_ITEMS if index out of range
  // E_INVALIDARD if index lower than zero
  // error code otherwise
  virtual HRESULT GetMediaType(int iPosition, CMediaType* pMediaType);

  // connects the pin to another pin
  // @param pReceivePin : pointer to the receiving pin's IPin interface
  // @param pMediaType : pointer to an AM_MEDIA_TYPE structure that specifies the media type for the connection
  virtual STDMETHODIMP Connect(IPin *pReceivePin, const AM_MEDIA_TYPE *pMediaType);

  // notifies the pin that a quality change is requested
  // @param pSender : pointer to the IBaseFilter interface of the filter that delivered the quality-control message
  // @param q : specifies a Quality structure that contains the quality-control message
  // @return : always E_NOTIMPL
  virtual STDMETHODIMP Notify(IBaseFilter* pSender, Quality q);

  // CBaseOutputPin

  // selects a memory allocator
  // @param pPin : pointer to the input pin's IMemInputPin interface
  // @param pAlloc : address of a variable that receives a pointer to the allocator's IMemAllocator interface
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT DecideAllocator(IMemInputPin * pPin, IMemAllocator ** pAlloc);

  // sets the buffer requirements
  // @param pAlloc : pointer to the allocator's IMemAllocator interface
  // @param pProperties : pointer to an ALLOCATOR_PROPERTIES structure that contains the input pin's buffer requirements. If the input pin does not have any requirements, the caller should zero out the members of this structure before calling the method.
  virtual HRESULT DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pProperties);

  // notifies the pin that the filter is now active
  // @return : S_OK if successful, VFW_E_NO_ALLOCATOR if no allocator is available, error code otherwise
  virtual HRESULT Active();

  // notifies the pin that the filter is no longer active
  // @return : S_OK if successful, VFW_E_NO_ALLOCATOR if no allocator is available, error code otherwise
  virtual HRESULT Inactive();

  // Requests the connected input pin to begin a flush operation
  // @return : S_OK if successful, VFW_E_NOT_CONNECTED if pin is not connected, error code otherwise
  virtual HRESULT DeliverBeginFlush();

  // requests the connected input pin to end a flush operation
  // @return : S_OK if successful, VFW_E_NOT_CONNECTED if pin is not connected, error code otherwise
  virtual HRESULT DeliverEndFlush();

  // queues output pin packet
  // @param packet : the packet to queue to output pin
  // @param timeout : the timeout in ms to queue to output pin
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueuePacket(COutputPinPacket *packet, DWORD timeout);

  // queues end of stream
  // @param endOfStreamResult : S_OK if normal end of stream, error code otherwise
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  virtual HRESULT QueueEndOfStream(HRESULT endOfStreamResult);

  // delivers a new-segment notification to the connected input pin
  // @param tStart : starting media position of the segment, in 100-nanosecond units
  // @param tStop : end media position of the segment, in 100-nanosecond units
  // @param dRate : rate at which this segment should be processed, as a percentage of the original rate
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);

  /* get methods */

  // gets demuxer ID
  // it is specified for splitter, which needs to identify output pin for specific output packet
  // @return : demuxer ID or DEMUXER_ID_UNSPECIFIED if not specified
  virtual unsigned int GetDemuxerId(void);

  // gets stream PID
  // it is specified for splitter, which needs to identify output pin for specific output packet
  // @return : stream PID or STREAM_PID_UNSPECIFIED if not specified
  virtual unsigned int GetStreamPid(void);

  /* set methods */

  // sets demuxer ID
  // @param demuxerId : the demuxer ID to set
  virtual void SetDemuxerId(unsigned int demuxerId);

  // sets stream PID
  // @param streamPid : the stream PID to set
  virtual void SetStreamPid(unsigned int streamPid);

  // sets new media types for output pin
  // @param mediaTypes : the media types to set
  // @return : true if successful, false otherwise
  virtual bool SetNewMediaTypes(CMediaTypeCollection *mediaTypes);

  /* other methods */

  // requests thread with CMD_PLAY command
  // @return : S_OK if successful, false otherwise
  virtual HRESULT DeliverPlay();

  // requests thread with CMD_APUSE command
  // @return : S_OK if successful, false otherwise
  virtual HRESULT DeliverPause();

  // sends specified media type with next queued packet
  // @param mediaType : media type to send
  // @return : S_OK if successful, false otherwise
  virtual HRESULT SendMediaType(CMediaType *mediaType);

  // tests if end of stream flag is set
  // @return : true if end of stream flag is set, false otherwise
  virtual bool IsEndOfStream(void);

protected:
  enum { CMD_EXIT, CMD_BEGIN_FLUSH, CMD_END_FLUSH, CMD_PLAY, CMD_PAUSE };

  // lock mutex for access to media packets
  HANDLE mediaPacketsLock;

  // holds media packets ready to send through pin
  COutputPinPacketCollection *mediaPackets;

  // holds media types associated with output pin
  CMediaTypeCollection *mediaTypes;

  // holds logger instance
  CLogger *logger;
  // holds configuration parameters
  CParameterCollection *parameters;

  // holds demuxer ID
  // it is specified for splitter, which needs to identify output pin for specific demuxer
  unsigned int demuxerId;

  // holds stream PID
  // it is specified for splitter, which needs to identify output pin for specific output packet
  unsigned int streamPid;

  // specifies if flushing is active
  volatile bool flushing;

  // holds media type to send with next queued packet
  CMediaType *mediaTypeToSend;

  // media type sub type for parser
  GUID mediaTypeSubType;

  // holds last store time to cache file
  unsigned int lastStoreTime;
  // holds cache file
  CCacheFile *cacheFile;

  /* statistical data */

  uint64_t outputPinDataLength;

  // holds dump file
  CDumpFile *dumpFile;

  // holds media packet processed from last store time
  unsigned int mediaPacketProcessed;

  /* methods */

  virtual DWORD ThreadProc();

  // creates dump box for dump file
  // @return : dump box or NULL if error
  virtual CDumpBox *CreateDumpBox(void);

  // gets store file name
  // @return : store file name or NULL if error
  wchar_t *GetStoreFile(void);

  // gets dump file name
  // @return : dump file name or NULL if error
  wchar_t *GetDumpFile(void);
};

#endif