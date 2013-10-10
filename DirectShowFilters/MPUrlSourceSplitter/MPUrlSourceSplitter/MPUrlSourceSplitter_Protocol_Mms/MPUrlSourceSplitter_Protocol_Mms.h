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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_MMS_DEFINED

#include "Logger.h"
#include "IProtocolPlugin.h"
#include "HttpCurlInstance.h"
#include "MMSContext.h"
#include "MMSChunk.h"

#include <WinSock2.h>

// see Ref 2.2.1.8
#define USERAGENT_NAME                                              L"User-Agent"
#define USERAGENT_VALUE                                             L"NSPlayer/7.10.0.3059"
// see Ref 2.2.1.4.33
// the guid value can be changed to any valid value.
#define CLIENTGUID_FORMAT_VALUE                                     L"xClientGUID=%s"

#define PROTOCOL_NAME                                               L"MMS"

#define METHOD_GET_MMS_HEADER_DATA_NAME                             L"GetMmsHeaderData()"
#define METHOD_GET_MMS_CHUNK_NAME                                   L"GetMmsChunk()"
#define METHOD_PARSE_MMS_ASF_HEADER_NAME                            L"ParseMmsAsfHeader()"

#define TOTAL_SUPPORTED_PROTOCOLS                                 1
wchar_t *SUPPORTED_PROTOCOLS[TOTAL_SUPPORTED_PROTOCOLS] = { L"MMS" };

#define MINIMUM_RECEIVED_DATA_FOR_SPLITTER                          1 * 1024 * 1024

#define CHUNK_HEADER_LENGTH                                         4   // 2bytes chunk type and 2bytes chunk length
#define EXT_HEADER_LENGTH                                           8   // 4bytes sequence, 2bytes useless and 2bytes chunk length

class CMPUrlSourceSplitter_Protocol_Mms : public IProtocolPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Protocol_Mms class
  CMPUrlSourceSplitter_Protocol_Mms(CLogger *logger, CParameterCollection *configuration);

  // destructor
  ~CMPUrlSourceSplitter_Protocol_Mms(void);

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

  // holds if length of stream was set
  bool setLength;

  // specifies if length can be set
  bool lengthCanBeSet;

  // stream time
  int64_t streamTime;

  // specifies position in buffer
  // it is always reset on seek
  int64_t bytePosition;

  // mutex for locking access to file, buffer, ...
  HANDLE lockMutex;

  // mutex for locking access to internal buffer of CURL instance
  HANDLE lockCurlMutex;

  // main instance of CURL
  CHttpCurlInstance *mainCurlInstance;

  // internal variable for requests to interrupt transfers
  bool internalExitRequest;
  // specifies if whole stream is downloaded
  bool wholeStreamDownloaded;
  // specifies if seeking (cleared when first data arrive)
  bool seekingActive;
  // specifies if filter requested supressing data
  bool supressData;

  // specifies request sequencenumber
  unsigned int sequenceNumber;

  // specifies if receiving data or issuing commands
  bool receivingData;

  // specifies if stream end (CHUNK_TYPE_END) was logged
  bool streamEndedLogged;

  // specifies is ASF header have to be send to filter or not (after seek)
  bool sendAsfHeader;

  // holds MMS context
  MMSContext *mmsContext;

  // GUID for client requests
  GUID clientGuid;

  // gets MMS header data
  // @param mmsContext : reference to MMS context
  // @param mmsChunk : reference to already acquired MMS chunk (can be NULL)
  // @return : S_OK if successfull, error code otherwise
  HRESULT GetMmsHeaderData(MMSContext *mmsContext, MMSChunk *mmsChunk);

  // gets MMS chunk data
  // @param mmsContext : reference to MMS context
  // @param mmsChunk : reference to MMS chunk
  // @return : S_OK if successful or error code
  HRESULT GetMmsChunk(MMSContext *mmsContext, MMSChunk *mmsChunk);

  // parses MMS ASF header
  // @param mmsContext : reference to MMS context
  // @param mmsChunk : reference to MMS chunk
  // @return : S_OK if successful or error code
  HRESULT ParseMmsAsfHeader(MMSContext *mmsContext, MMSChunk *mmsChunk);
};

#endif
