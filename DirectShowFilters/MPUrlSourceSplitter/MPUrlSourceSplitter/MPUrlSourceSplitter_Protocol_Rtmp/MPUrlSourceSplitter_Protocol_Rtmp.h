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
#include "IProtocolPlugin.h"
#include "LinearBuffer.h"
#include "RtmpCurlInstance.h"
#include "RtmpStreamFragmentCollection.h"

#include <WinSock2.h>

#define PROTOCOL_NAME                                                         L"RTMP"

#define TOTAL_SUPPORTED_PROTOCOLS                                             6
wchar_t *SUPPORTED_PROTOCOLS[TOTAL_SUPPORTED_PROTOCOLS] =                     { L"RTMP", L"RTMPT", L"RTMPE", L"RTMPTE", L"RTMPS", L"RTMPTS" };

#define MINIMUM_RECEIVED_DATA_FOR_SPLITTER                                    1 * 1024 * 1024
#define BUFFER_FOR_PROCESSING_SIZE_DEFAULT                                    256 * 1024

class CMPUrlSourceSplitter_Protocol_Rtmp : public IProtocolPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Protocol_Rtmp class
  CMPUrlSourceSplitter_Protocol_Rtmp(CLogger *logger, CParameterCollection *configuration);

  // destructor
  ~CMPUrlSourceSplitter_Protocol_Rtmp(void);

  // IProtocol interface

  // test if connection is opened
  // @return : true if connected, false otherwise
  bool IsConnected(void);

  // parse given url to internal variables for specified protocol
  // errors should be logged to log file
  // @param parameters : the url and connection parameters
  // @return : S_OK if successfull
  HRESULT ParseUrl(const CParameterCollection *parameters);

  // receives data and stores them into receive data parameter
  // the method should fill receiveData parameter with relevant data and finish
  // the method can't block call (method is called within thread which can be terminated anytime)
  // @param receiveData : received data
  // @result: S_OK if successful, error code otherwise
  HRESULT ReceiveData(CReceiveData *receiveData);

  // gets current connection parameters (can be different as supplied connection parameters)
  // @return : current connection parameters or NULL if error
  CParameterCollection *GetConnectionParameters(void);

  // ISimpleProtocol interface

  // get timeout (in ms) for receiving data
  // @return : timeout (in ms) for receiving data
  unsigned int GetReceiveDataTimeout(void);

  // starts receiving data from specified url and configuration parameters
  // @param parameters : the url and parameters used for connection
  // @return : S_OK if url is loaded, false otherwise
  HRESULT StartReceivingData(CParameterCollection *parameters);

  // request protocol implementation to cancel the stream reading operation
  // @return : S_OK if successful
  HRESULT StopReceivingData(void);

  // retrieves the progress of the stream reading operation
  // @param total : reference to a variable that receives the length of the entire stream, in bytes
  // @param current : reference to a variable that receives the length of the downloaded portion of the stream, in bytes
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_UNEXPECTED if unexpected error
  HRESULT QueryStreamProgress(LONGLONG *total, LONGLONG *current);
  
  // retrieves available lenght of stream
  // @param available : reference to instance of class that receives the available length of stream, in bytes
  // @return : S_OK if successful, other error codes if error
  HRESULT QueryStreamAvailableLength(CStreamAvailableLength *availableLength);

  // clear current session
  // @return : S_OK if successfull
  HRESULT ClearSession(void);

  // gets duration of stream in ms
  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
  int64_t GetDuration(void);

  // reports actual stream time to protocol
  // @param streamTime : the actual stream time in ms to report to protocol
  void ReportStreamTime(uint64_t streamTime);

  // ISeeking interface

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  unsigned int GetSeekingCapabilities(void);

  // request protocol implementation to receive data from specified time (in ms)
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  int64_t SeekToTime(int64_t time);

  // request protocol implementation to receive data from specified position to specified position
  // @param start : the requested start position (zero is start of stream)
  // @param end : the requested end position, if end position is lower or equal to start position than end position is not specified
  // @return : position where seek finished or lower than zero if error
  int64_t SeekToPosition(int64_t start, int64_t end);

  // sets if protocol implementation have to supress sending data to filter
  // @param supressData : true if protocol have to supress sending data to filter, false otherwise
  void SetSupressData(bool supressData);

  // IPlugin interface

  // return reference to null-terminated string which represents plugin name
  // function have to allocate enough memory for plugin name string
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  const wchar_t *GetName(void);

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull
  HRESULT Initialize(PluginConfiguration *configuration);

protected:
  CLogger *logger;

  // holds various parameters supplied by caller
  CParameterCollection *configurationParameters;

  // holds receive data timeout
  unsigned int receiveDataTimeout;

  // the lenght of stream
  LONGLONG streamLength;

  // the duration of stream
  // mostly we get only stream duration instead of stream length
  double streamDuration;

  // holds if length of stream was set
  bool setLength;
  // holds if end of stream was set
  bool setEndOfStream;

  // specifies position in buffer
  // it is always reset on seek
  int64_t bytePosition;

  // mutex for locking access to file, buffer, ...
  HANDLE lockMutex;

  // mutex for locking access to internal buffer of CURL instance
  HANDLE lockCurlMutex;

  // main instance of CURL
  CRtmpCurlInstance *mainCurlInstance;

  // specifies if whole stream is downloaded
  bool wholeStreamDownloaded;
  // specifies if seeking (cleared when first data arrive)
  bool seekingActive;
  // specifies if filter requested supressing data
  bool supressData;
  // specifies if working with live stream
  bool liveStream;

  // holds first FLV packet timestamp for correction of video packet timestamps
  int firstTimestamp;

  // holds first video FLV packet timestamp for correction of video packet timestamps
  int firstVideoTimestamp;

  // gets store file path based on configuration
  // creates folder structure if not created
  // @return : store file or NULL if error
  wchar_t *GetStoreFile(void);

  // holds store file path
  wchar_t *storeFilePath;
  // holds last store time of storing stream fragments to file
  DWORD lastStoreTime;

  // specifies if we are still connected
  bool isConnected;

  // holds RTMP stream fragment
  CRtmpStreamFragmentCollection *rtmpStreamFragments;

  // holds which fragment is currently downloading (UINT_MAX means none)
  unsigned int streamFragmentDownloading;
  // holds which fragment is currently processed
  unsigned int streamFragmentProcessing;
  // holds which fragment have to be downloaded
  // (UINT_MAX means next fragment, always reset after started download of fragment)
  unsigned int streamFragmentToDownload;

  // fills buffer for processing with segment and fragment data (stored in memory or in store file)
  // @param fragments : stream fragments collection
  // @param streamFragmentProcessing : fragment to get data
  // @param storeFile : the name of store file
  // @return : buffer for processing with filled data, NULL otherwise
  CLinearBuffer *FillBufferForProcessing(CRtmpStreamFragmentCollection *fragments, unsigned int streamFragmentProcessing, wchar_t *storeFile);

  // specifies if FLV packets until key frame with timestamp higher than specified is received
  unsigned int ignoreKeyFrameTimestamp;

  // holds additional correction after seeking to known start timestamp
  int additionalCorrection;

  // holds duration in ms of RTMP stream (used in total length guess)
  // RTMP_DURATION_UNSPECIFIED if not specified
  uint64_t duration;

  // holds filter actual stream time
  uint64_t reportedStreamTime;
};

#endif
