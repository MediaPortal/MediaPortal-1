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

#define OUTPUT_PIN_FLAG_NONE                                          0x00000000
#define OUTPUT_PIN_FLAG_CONTAINER_MPEG_TS                             0x00000001
#define OUTPUT_PIN_FLAG_CONTAINER_MPEG                                0x00000002
#define OUTPUT_PIN_FLAG_CONTAINER_WTV                                 0x00000004
#define OUTPUT_PIN_FLAG_CONTAINER_ASF                                 0x00000008
#define OUTPUT_PIN_FLAG_CONTAINER_OGG                                 0x00000010
#define OUTPUT_PIN_FLAG_CONTAINER_MATROSKA                            0x00000020
#define OUTPUT_PIN_FLAG_CONTAINER_AVI                                 0x00000040
#define OUTPUT_PIN_FLAG_CONTAINER_MP4                                 0x00000080
#define OUTPUT_PIN_FLAG_HAS_ACCESS_UNIT_DELIMITERS                    0x00000100
#define OUTPUT_PIN_FLAG_PGS_DROP_STATE                                0x00000200

class CMPUrlSourceSplitterOutputPin
  : public CBaseOutputPin
  , protected CAMThread
{
public:
  CMPUrlSourceSplitterOutputPin(CMediaTypeCollection *mediaTypes, LPCWSTR pName, CBaseFilter *pFilter, CCritSec *pLock, HRESULT *phr, const wchar_t *containerFormat);
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

  // tests if container is MPEG TS
  // @return : true if container if MPEG TS, false otherwise
  bool IsContainerMpegTs(void);

  // tests if container is MPEG
  // @return : true if container if MPEG, false otherwise
  bool IsContainerMpeg(void);

  // tests if container is WTV
  // @return : true if container if WTV, false otherwise
  bool IsContainerWtv(void);

  // tests if container is ASF
  // @return : true if container if ASF, false otherwise
  bool IsContainerAsf(void);

  // tests if container is OGG
  // @return : true if container if OGG, false otherwise
  bool IsContainerOgg(void);

  // tests if container is MATROSKA
  // @return : true if container if MATROSKA, false otherwise
  bool IsContainerMatroska(void);

  // tests if container is AVI
  // @return : true if container if AVI, false otherwise
  bool IsContainerAvi(void);

  // tests if container is MP4
  // @return : true if container if MP4, false otherwise
  bool IsContainerMp4(void);

  // tests if has access unit delimiters (for H264)
  // @return : true if has access unit delimiters, false otherwise
  bool HasAccessUnitDelimiters(void);

  // tests if PGS drop state flag is set
  // @return : true if PGS drop state flag is set, false otherwise
  bool IsPGSDropState(void);

  // tests if specific combination of flags is set
  // @param flags : the combination of flags to test
  // @return : true if specific combination of flags is set, false otherwise
  bool IsFlags(unsigned int flags);
 
protected:
  enum { CMD_EXIT, CMD_BEGIN_FLUSH, CMD_END_FLUSH, CMD_PLAY, CMD_PAUSE };

  // holds various flags
  unsigned int flags;

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

  // media type sub type for parser
  GUID mediaTypeSubType;

  /* data for H264 Annex B stream */

  // holds data for parsing of H264 Annex B stream
  COutputPinPacket *h264Buffer;
  COutputPinPacketCollection *h264PacketCollection;

  /* methods */

  DWORD ThreadProc();

  // parses output pin packet
  // @return : S_OK if successful, error code otherwise
  HRESULT Parse(GUID subType, COutputPinPacket *packet);

  // sets if has access unit delimiters (for H264)
  // @param hasAccessUnitDelimiters : true if has access unit delimiters, false otherwise
  void SetHasAccessUnitDelimiters(bool hasAccessUnitDelimiters);

  // sets PGS drop state
  // @param pgsDropState : true if PGS drop state, false otherwise
  void SetPGSDropState(bool pgsDropState);
};

#endif