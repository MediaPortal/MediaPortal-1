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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_DEFINED

#include "Logger.h"
#include "ProtocolPlugin.h"
#include "RtmpCurlInstance.h"
#include "RtmpStreamFragmentCollection.h"
#include "CacheFile.h"

#define PROTOCOL_NAME                                                         L"RTMP"

#define TOTAL_SUPPORTED_PROTOCOLS                                             6
wchar_t *SUPPORTED_PROTOCOLS[TOTAL_SUPPORTED_PROTOCOLS] =                     { L"RTMP", L"RTMPT", L"RTMPE", L"RTMPTE", L"RTMPS", L"RTMPTS" };

#define MINIMUM_RECEIVED_DATA_FOR_SPLITTER                                    1 * 1024 * 1024

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_NONE                        PROTOCOL_PLUGIN_FLAG_NONE

// only closes curl instance (stop receive data in curl instance), but stays in memory
#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE         (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 0))
// stop receiving data flag cannot be set without close curl instance flag
// specifies that after closing curl instance is called StopReceivingData() method
#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA         (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 1))

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_FIRST_TIMESTAMP         (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 2))
#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SKIP_HEADER_AND_META        (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 3))

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_VIDEO_CORRECTION        (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 4))
#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_AUDIO_CORRECTION        (1 << (PROTOCOL_PLUGIN_FLAG_LAST + 5))

#define MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_LAST                        (PROTOCOL_PLUGIN_FLAG_LAST + 6)

class CMPUrlSourceSplitter_Protocol_Rtmp : public CProtocolPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Protocol_Rtmp class
  CMPUrlSourceSplitter_Protocol_Rtmp(HRESULT *result, CLogger *logger, CParameterCollection *configuration);

  // destructor
  ~CMPUrlSourceSplitter_Protocol_Rtmp(void);

  // IProtocol interface

  // gets connection state
  // @return : one of protocol connection state values
  ProtocolConnectionState GetConnectionState(void);

  // parse given url to internal variables for specified protocol
  // errors should be logged to log file
  // @param parameters : the url and connection parameters
  // @return : S_OK if successfull
  HRESULT ParseUrl(const CParameterCollection *parameters);

  // receives data and process stream package request
  // the method can't block call (method is called within thread which can be terminated anytime)
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful, error code only in case when error is not related to processing request
  HRESULT ReceiveData(CStreamPackage *streamPackage);

  // gets current connection parameters (can be different as supplied connection parameters)
  // @param parameters : the reference to parameter collection to be filled with connection parameters
  // @return : S_OK if successful, error code otherwise
  HRESULT GetConnectionParameters(CParameterCollection *parameters);

  // ISimpleProtocol interface

  // gets timeout (in ms) for opening connection
  // @return : timeout (in ms) for opening connection
  unsigned int GetOpenConnectionTimeout(void);

  // gets sleep time (in ms) for opening connection
  // some protocols may need some sleep before loading (e.g. multicast UDP protocol needs some time between unsubscribing and subscribing in multicast groups)
  // @return : sleep time (in ms) for opening connection
  unsigned int GetOpenConnectionSleepTime(void);

  // gets total timeout (in ms) for re-opening connection (opening connection after lost connection)
  // re-open connection total timeout should be much more greater (e.g. 3 - 5 times) in order to allow more opening requests
  // @return : total timeout (in ms) for re-opening connection
  unsigned int GetTotalReopenConnectionTimeout(void);

  // starts receiving data from specified url and configuration parameters
  // @param parameters : the url and parameters used for connection
  // @return : S_OK if url is loaded, false otherwise
  HRESULT StartReceivingData(CParameterCollection *parameters);

  // request protocol implementation to cancel the stream reading operation
  // @return : S_OK if successful
  HRESULT StopReceivingData(void);

  // retrieves the progress of the stream reading operation
  // @param streamProgress : reference to instance of class that receives the stream progress
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_INVALIDARG if stream ID is unknown, E_UNEXPECTED if unexpected error
  HRESULT QueryStreamProgress(CStreamProgress *streamProgress);
  
  // clears current session
  void ClearSession(void);

  // gets duration of stream in ms
  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
  int64_t GetDuration(void);

  // gets information about streams
  // receiving data is disabled until protocol reports valid stream count (at least one)
  // @return : S_OK if successful, E_STREAM_COUNT_UNKNOWN if stream count is unknown, error code otherwise
  HRESULT GetStreamInformation(CStreamInformationCollection *streams);

  // ISeeking interface

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  unsigned int GetSeekingCapabilities(void);

  // request protocol implementation to receive data from specified time (in ms) for specified stream
  // this method is called with same time for each stream in protocols with multiple streams
  // @param streamId : the stream ID to receive data from specified time
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  int64_t SeekToTime(unsigned int streamId, int64_t time);

  // CPlugin implementation

  // return reference to null-terminated string which represents plugin name
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual const wchar_t *GetName(void);

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  virtual GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

protected:
  // holds connection state
  ProtocolConnectionState connectionState;

  // mutex for locking access to file, buffer, ...
  HANDLE lockMutex;
  // mutex for locking access to internal buffer of CURL instance
  HANDLE lockCurlMutex;

  // main instance of CURL
  CRtmpCurlInstance *mainCurlInstance;

  // holds RTMP stream fragments
  CRtmpStreamFragmentCollection *streamFragments;
  // holds cache file
  CCacheFile *cacheFile;

  // holds which fragment is currently downloading (UINT_MAX means none)
  unsigned int streamFragmentDownloading;
  // holds which fragment is currently processed
  unsigned int streamFragmentProcessing;
  // holds which fragment have to be downloaded
  // (UINT_MAX means next fragment, always reset after started download of fragment)
  unsigned int streamFragmentToDownload;

  // holds last store time of storing stream fragments to file
  unsigned int lastStoreTime;

  // the lenght of stream
  int64_t streamLength;

  // holds video timestamp correction (calculated after seek)
  unsigned int videoTimestampCorrection;
  // holds audio timestamp correction (calculated after seek)
  unsigned int audioTimestampCorrection;

  // holds last FLV packet timestamp
  unsigned int lastFlvPacketTimestamp;
  // holds last cumulated FLV packet timestamp
  int64_t lastCumulatedFlvTimestamp;
  // holds header and meta packet size
  unsigned headerAndMetaPacketSize;

  /* methods */

  // gets store file path based on configuration
  // creates folder structure if not created
  // @param extension : the extension of store file
  // @return : store file or NULL if error
  wchar_t *GetStoreFile(const wchar_t *extension);

  // gets byte position in buffer
  // it is always reset on seek
  // @return : byte position in buffer
  int64_t GetBytePosition(void);

  // recalculate stream fragments start positions based on previous stream fragments
  // @param streamFragments : the collection of stream fragments to recalculate
  // @param startIndex : the index of first stream fragment to recalculate start position
  void RecalculateStreamFragmentStartPosition(CRtmpStreamFragmentCollection *streamFragments, unsigned int startIndex);

  // gets FLV packet timestamp based on current FLV packet timestamp
  // @param currentFlvPacketTimestamp : current FLV packet timestamp to get FLV packet timestamp
  // @return : FLV packet timestamp
  int64_t GetFlvPacketTimestamp(unsigned int currentFlvPacketTimestamp);
};

#endif
