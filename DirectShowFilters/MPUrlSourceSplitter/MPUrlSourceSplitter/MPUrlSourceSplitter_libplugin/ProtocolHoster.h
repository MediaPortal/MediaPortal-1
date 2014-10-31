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

#ifndef __PROTOCOL_HOSTER_DEFINED
#define __PROTOCOL_HOSTER_DEFINED

#include "Hoster.h"
#include "ProtocolPlugin.h"
#include "StreamPackageCollection.h"
#include "IDemuxerOwner.h"

#define MODULE_PROTOCOL_HOSTER_NAME                                   L"ProtocolHoster"

#define METHOD_RECEIVE_DATA_WORKER_NAME                               L"ReceiveDataWorker()"
#define METHOD_CREATE_RECEIVE_DATA_WORKER_NAME                        L"CreateReceiveDataWorker()"
#define METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME                       L"DestroyReceiveDataWorker()"

#define PROTOCOL_HOSTER_FLAG_NONE                                     HOSTER_FLAG_NONE

#define PROTOCOL_HOSTER_FLAG_LAST                                     (HOSTER_FLAG_LAST + 0)

class CProtocolHoster : public CHoster, virtual public IProtocol, virtual public IDemuxerOwner
{
public:
  CProtocolHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  ~CProtocolHoster(void);

  // IProtocol interface implementation

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

  // ISimpleProtocol interface implementation

  // gets timeout (in ms) for opening connection (first opening connection when specified new URL)
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

  // reports actual stream time to protocol
  // @param streamTime : the actual stream time in ms to report to protocol
  // @param streamPosition : the actual stream position (related to stream time) to report to protocol
  void ReportStreamTime(uint64_t streamTime, uint64_t streamPosition);

  // gets information about streams
  // receiving data is disabled until protocol reports valid stream count (at least one)
  // @return : S_OK if successful, E_STREAM_COUNT_UNKNOWN if stream count is unknown, error code otherwise
  HRESULT GetStreamInformation(CStreamInformationCollection *streams);

  // ISeeking interface implementation

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  unsigned int GetSeekingCapabilities(void);

  // request protocol implementation to receive data from specified time (in ms) for specified stream
  // this method is called with same time for each stream in protocols with multiple streams
  // @param streamId : the stream ID to receive data from specified time
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  int64_t SeekToTime(unsigned int streamId, int64_t time);

  // set pause, seek or stop mode
  // in such mode are reading operations disabled
  // @param pauseSeekStopMode : one of PAUSE_SEEK_STOP_MODE values
  void SetPauseSeekStopMode(unsigned int pauseSeekStopMode);

  // IDemuxerOwner interface implementation

  // process stream package request
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful, error code only in case when error is not related to processing request
  HRESULT ProcessStreamPackage(CStreamPackage *streamPackage);

  // other methods
 
  // gets active protocol
  // @return : active protocol or NULL if none
  CProtocolPlugin *GetActiveProtocol(void);

  // loads plugins from directory
  // @return : S_OK if successful, E_NO_PROTOCOL_LOADED if no protocol loaded, error code otherwise
  virtual HRESULT LoadPlugins(void);

  // tests if stream is specified as live stream by configuration
  // @return : true if stream is specified as live stream, false otherwise
  virtual bool IsLiveStreamSpecified(void);

  // tests if stream is detected as live stream
  // @return : true if stream is detected as live stream
  virtual bool IsLiveStreamDetected(void);

  // tests if stream is specified or detected as live stream
  // @return : true if stream is specified or detected as live stream
  virtual bool IsLiveStream(void);

  // tests if stream length was set
  // @return : true if stream length was set, false otherwise
  virtual bool IsSetStreamLength(void);

  // tests if stream length is estimated
  // @return : true if stream length is estimated, false otherwise
  virtual bool IsStreamLengthEstimated(void);

  // tests if whole stream is downloaded (no gaps)
  // @return : true if whole stream is downloaded
  virtual bool IsWholeStreamDownloaded(void);

  // tests if end of stream is reached (but it can be with gaps)
  // @return : true if end of stream reached, false otherwise
  virtual bool IsEndOfStreamReached(void);

  // tests if connection was lost and can't be opened again
  // @return : true if connection was lost and can't be opened again, false otherwise
  virtual bool IsConnectionLostCannotReopen(void);

protected:
  // stores active protocol
  CProtocolPlugin *activeProtocol;

  // holds stream packages to be processed
  CStreamPackageCollection *streamPackages;

  // mutex for locking access to file, buffer, ...
  HANDLE mutex;

  // holds protocol error
  HRESULT protocolError;

  /* received data worker */

  HANDLE receiveDataWorkerThread;
  volatile bool receiveDataWorkerShouldExit;

  // holds pause, seek or stop mode
  volatile unsigned int pauseSeekStopMode;
  // specifies if we are in StartReceiveData() method (it is required to set correct parameters to StartReceivingData() of protocol
  volatile bool startReceivingData;
  // holds finish time for protocol (protocol must return from StartReceivingData() to this time)
  // it is acquired be GetTickCount() (time in ms)
  volatile unsigned int finishTime;

  /* methods */

  // creates hoster plugin metadata
  // @param result : the reference to result
  // @param logger : the reference to logger
  // @param configuration : the reference to configuration
  // @param hosterName : the hoster name
  // @param pluginLibraryFileName : the plugin library file name
  // @return : hoster plugin metadata instance
  virtual CHosterPluginMetadata *CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);

  // creates plugin configuration
  // @param result : the reference to result
  // @param configuration : the collection of parameters
  // @return : plugin configuration instance
  virtual CPluginConfiguration *CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration);

  /* receive data worker */

  // creates receive data worker
  // @return : S_OK if successful
  HRESULT CreateReceiveDataWorker(void);

  // destroys receive data worker
  // @return : S_OK if successful
  HRESULT DestroyReceiveDataWorker(void);

  static unsigned int WINAPI ReceiveDataWorker(LPVOID lpParam);
};

#endif