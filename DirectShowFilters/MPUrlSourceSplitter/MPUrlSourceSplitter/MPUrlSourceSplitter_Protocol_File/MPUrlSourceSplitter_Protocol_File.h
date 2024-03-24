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

#ifndef __MP_URL_SOURCE_SPLITTER_PROTOCOL_FILE_DEFINED
#define __MP_URL_SOURCE_SPLITTER_PROTOCOL_FILE_DEFINED

#include "Logger.h"
#include "ProtocolPlugin.h"
#include "LinearBuffer.h"

#include <stdio.h>

#define DEFAULT_BUFFER_SIZE                                       64 * 1024

#define PROTOCOL_NAME                                             L"FILE"

#define PROTOCOL_STORE_FILE_NAME_PART                                         L"mpurlsourcesplitter_protocol_file"

#define TOTAL_SUPPORTED_PROTOCOLS                                 1
wchar_t *SUPPORTED_PROTOCOLS[TOTAL_SUPPORTED_PROTOCOLS] = { L"FILE" };

class CMPUrlSourceSplitter_Protocol_File : public CProtocolPlugin
{
public:
  // constructor
  // create instance of CMPUrlSourceSplitter_Protocol_File class
  CMPUrlSourceSplitter_Protocol_File(HRESULT* result, CLogger *logger, CParameterCollection *configuration);

  // destructor
  ~CMPUrlSourceSplitter_Protocol_File(void);

  // IProtocol interface

  // gets connection state
  // @return : one of protocol connection state values
  ProtocolConnectionState GetConnectionState(void);

  // parse given url to internal variables for specified protocol
  // errors should be logged to log file
  // @param parameters : the url and connection parameters
  // @return : S_OK if successfull
  HRESULT ParseUrl(const CParameterCollection *parameters);

  // receives data and stores them into receive data parameter
  // the method should fill receiveData parameter with relevant data and finish
  // the method can't block call (method is called within thread which can be terminated anytime)
  // @param receiveData : received data
  // @return: S_OK if successful, error code otherwise
  HRESULT ReceiveData(CStreamPackage* streamPackage);

  // gets current connection parameters (can be different as supplied connection parameters)
  // @return : current connection parameters or NULL if error
  HRESULT GetConnectionParameters(CParameterCollection* parameters);

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
  
  // clear current session
  void ClearSession(void);

  // gets duration of stream in ms
  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
  int64_t GetDuration(void);

  // gets information about streams
  // receiving data is disabled until protocol reports valid stream count (at least one)
  // @return : S_OK if successful, E_STREAM_COUNT_UNKNOWN if stream count is unknown, error code otherwise
  HRESULT GetStreamInformation(CStreamInformationCollection* streams);

  // ISeeking interface

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  unsigned int GetSeekingCapabilities(void);

  // request protocol implementation to receive data from specified time (in ms)
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  int64_t SeekToTime(unsigned int streamId, int64_t time);

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
  HRESULT Initialize(CPluginConfiguration *configuration);

protected:

  // holds file path
  wchar_t *filePath;
  // holds file
  FILE *fileStream;

  // holds receive data timeout
  unsigned int receiveDataTimeout;

  // the lenght of file
  LONGLONG fileLength;

  // holds if length of stream was set
  bool setLength;

  // stream time
  int64_t streamTime;

  // mutex for locking access to file, buffer, ...
  HANDLE lockMutex;

  // specifies if filter requested supressing data
  bool supressData;

  // get module name for Initialize() method
  // @return : module name
  virtual const wchar_t* GetModuleName(void);

  // gets store file name part
  // @return : store file name part or NULL if error
  const wchar_t* GetStoreFileNamePart(void);
};

#endif
