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

#include "IFilter.h"
#include "OutputPinPacketCollection.h"
#include "MediaTypeCollection.h"

#include <streams.h>

#define OUTPUT_PIN_BUFFERS_RECOMMENDED                                32
#define OUTPUT_PIN_BUFFERS_LENGTH_RECOMMENDED                         524288

class CMPUrlSourceSplitterOutputPin
  : public CBaseOutputPin
  , protected CAMThread
{
public:
  CMPUrlSourceSplitterOutputPin(CMediaTypeCollection *mediaTypes, LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr);
  ~CMPUrlSourceSplitterOutputPin();

  DECLARE_IUNKNOWN;
  STDMETHODIMP NonDelegatingQueryInterface(REFIID riid, void** ppv);

  // CBasePin

  // determines if the pin accepts a specific media type
  // @param pmt : pointer to a CMediaType object that contains the proposed media type
  // @return : S_OK if media type is acceptable, error code otherwise
  HRESULT CheckMediaType(const CMediaType* pmt);

  // retrieves a preferred media type, by index value
  // @param iPosition : zero-based index value
  // @param pMediaType : pointer to a CMediaType object that receives the media type
  // @return :
  // S_OK if successful
  // VFW_S_NO_MORE_ITEMS if index out of range
  // E_INVALIDARD if index lower than zero
  // error code otherwise
  HRESULT GetMediaType(int iPosition, CMediaType* pMediaType);

  // connects the pin to another pin
  // @param pReceivePin : pointer to the receiving pin's IPin interface
  // @param pMediaType : pointer to an AM_MEDIA_TYPE structure that specifies the media type for the connection
  STDMETHODIMP Connect(IPin *pReceivePin, const AM_MEDIA_TYPE *pMediaType);

  // notifies the pin that a quality change is requested
  // @param pSender : pointer to the IBaseFilter interface of the filter that delivered the quality-control message
  // @param q : specifies a Quality structure that contains the quality-control message
  // @return : always E_NOTIMPL
  STDMETHODIMP Notify(IBaseFilter* pSender, Quality q);

  // CBaseOutputPin

  // selects a memory allocator
  // @param pPin : pointer to the input pin's IMemInputPin interface
  // @param pAlloc : address of a variable that receives a pointer to the allocator's IMemAllocator interface
  // @return : S_OK if successful, error code otherwise
  HRESULT DecideAllocator(IMemInputPin * pPin, IMemAllocator ** pAlloc);

  // sets the buffer requirements
  // @param pAlloc : pointer to the allocator's IMemAllocator interface
  // @param pProperties : pointer to an ALLOCATOR_PROPERTIES structure that contains the input pin's buffer requirements. If the input pin does not have any requirements, the caller should zero out the members of this structure before calling the method.
  HRESULT DecideBufferSize(IMemAllocator* pAlloc, ALLOCATOR_PROPERTIES* pProperties);

  // notifies the pin that the filter is now active
  // @return : S_OK if successful, VFW_E_NO_ALLOCATOR if no allocator is available, error code otherwise
  HRESULT Active();

  // notifies the pin that the filter is no longer active
  // @return : S_OK if successful, VFW_E_NO_ALLOCATOR if no allocator is available, error code otherwise
  HRESULT Inactive();

  // Requests the connected input pin to begin a flush operation
  // @return : S_OK if successful, VFW_E_NOT_CONNECTED if pin is not connected, error code otherwise
  HRESULT DeliverBeginFlush();

  // requests the connected input pin to end a flush operation
  // @return : S_OK if successful, VFW_E_NOT_CONNECTED if pin is not connected, error code otherwise
  HRESULT DeliverEndFlush();

  // queues output pin packet
  // @param packet : the packet to queue to output pin
  // @param timeout : the timeout in ms to queue to output pin
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  HRESULT QueuePacket(COutputPinPacket *packet, DWORD timeout);

  // queues end of stream
  // @return : S_OK if successful, VFW_E_TIMEOUT if timeout occured, error code otherwise
  HRESULT QueueEndOfStream(void);

  // delivers a new-segment notification to the connected input pin
  // @param tStart : starting media position of the segment, in 100-nanosecond units
  // @param tStop : end media position of the segment, in 100-nanosecond units
  // @param dRate : rate at which this segment should be processed, as a percentage of the original rate
  // @return : S_OK if successful, error code otherwise
  HRESULT DeliverNewSegment(REFERENCE_TIME tStart, REFERENCE_TIME tStop, double dRate);

  /* get methods */

  // gets stream PID
  // it is specified for splitter, which needs to identify output pin for specific output packet
  // @return : stream PID or STREAM_PID_UNSPECIFIED if not specified
  unsigned int GetStreamPid(void);

  /* set methods */

  // sets stream PID
  // @param streamPid : the stream PID to set
  void SetStreamPid(unsigned int streamPid);

  // sets new media types for output pin
  // @param mediaTypes : the media types to set
  // @return : true if successful, false otherwise
  bool SetNewMediaTypes(CMediaTypeCollection *mediaTypes);

  /* other methods */

  // requests thread with CMD_PLAY command
  // @return : S_OK if successful, false otherwise
  HRESULT DeliverPlay();

  // requests thread with CMD_APUSE command
  // @return : S_OK if successful, false otherwise
  HRESULT DeliverPause();

  // sends specified media type with next queued packet
  // @param mediaType : media type to send
  // @return : S_OK if successful, false otherwise
  HRESULT SendMediaType(CMediaType *mediaType);
 
protected:
  enum { CMD_EXIT, CMD_BEGIN_FLUSH, CMD_END_FLUSH, CMD_PLAY, CMD_PAUSE };

  DWORD ThreadProc();

  // lock mutex for access to media packets
  HANDLE mediaPacketsLock;

  // holds media packets ready to send through pin
  COutputPinPacketCollection *mediaPackets;

  // holds media types associated with output pin
  CMediaTypeCollection *mediaTypes;

  // holds filter reference
  IFilter *filter;

  // holds stream PID
  // it is specified for splitter, which needs to identify output pin for specific output packet
  unsigned int streamPid;

  // specifies if flushing is active
  volatile bool flushing;

  // holds media type to send with next queued packet
  CMediaType *mediaTypeToSend;
};

#endif