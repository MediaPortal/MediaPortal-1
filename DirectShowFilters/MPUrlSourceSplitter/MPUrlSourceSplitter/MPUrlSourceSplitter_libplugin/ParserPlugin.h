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

#ifndef __PARSER_PLUGIN_DEFINED
#define __PARSER_PLUGIN_DEFINED

#include "Plugin.h"
#include "IDemuxerOwner.h"
#include "ProtocolHoster.h"
#include "IProtocol.h"
#include "DumpFile.h"

#define METHOD_GET_PARSER_RESULT_NAME                                 L"GetParserResult()"

#define PARSER_PLUGIN_FLAG_NONE                                       PLUGIN_FLAG_NONE

#define PARSER_PLUGIN_FLAG_DUMP_INPUT_DATA                            (1 << (PLUGIN_FLAG_LAST + 0))
#define PARSER_PLUGIN_FLAG_DUMP_OUTPUT_DATA                           (1 << (PLUGIN_FLAG_LAST + 1))
#define PARSER_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED                      (1 << (PLUGIN_FLAG_LAST + 2))
#define PARSER_PLUGIN_FLAG_LIVE_STREAM_DETECTED                       (1 << (PLUGIN_FLAG_LAST + 3))
#define PARSER_PLUGIN_FLAG_SET_STREAM_LENGTH                          (1 << (PLUGIN_FLAG_LAST + 4))
#define PARSER_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED                    (1 << (PLUGIN_FLAG_LAST + 5))
#define PARSER_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED                    (1 << (PLUGIN_FLAG_LAST + 6))
#define PARSER_PLUGIN_FLAG_END_OF_STREAM_REACHED                      (1 << (PLUGIN_FLAG_LAST + 7))
#define PARSER_PLUGIN_FLAG_CONNECTION_LOST_CANNOT_REOPEN              (1 << (PLUGIN_FLAG_LAST + 8))

#define PARSER_PLUGIN_FLAG_LAST                                       (PLUGIN_FLAG_LAST + 9)

#define PARSER_RESULT_PENDING                                         1
#define PARSER_RESULT_NOT_KNOWN                                       2
#define PARSER_RESULT_KNOWN                                           S_OK
#define PARSER_RESULT_DRM_PROTECTED                                   E_DRM_PROTECTED

class CParserPlugin : public CPlugin, virtual public IDemuxerOwner, virtual public IProtocol
{
public:
  CParserPlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CParserPlugin(void);

  enum Action
  {
    ParseStream,
    GetNewConnection
  };

  // CPlugin

  // get plugin instance ID
  // @return : GUID, which represents instance identifier or GUID_NULL if error
  virtual GUID GetInstanceId(void);

  // initialize plugin implementation with configuration parameters
  // @param configuration : the reference to additional configuration parameters (created by plugin's hoster class)
  // @return : S_OK if successfull, error code otherwise
  virtual HRESULT Initialize(CPluginConfiguration *configuration);

  // IDemuxerOwner interface

  // gets duration of stream in ms
  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
  virtual int64_t GetDuration(void);

  // process stream package request
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful, error code only in case when error is not related to processing request
  virtual HRESULT ProcessStreamPackage(CStreamPackage *streamPackage);

  // retrieves the progress of the stream reading operation
  // @param streamProgress : reference to instance of class that receives the stream progress
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_INVALIDARG if stream ID is unknown, E_UNEXPECTED if unexpected error
  virtual HRESULT QueryStreamProgress(CStreamProgress *streamProgress);

  // IProtocol interface

  // gets current connection parameters (can be different as supplied connection parameters)
  // @param parameters : the reference to parameter collection to be filled with connection parameters
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT GetConnectionParameters(CParameterCollection *parameters);

  // gets connection state
  // @return : one of protocol connection state values
  ProtocolConnectionState GetConnectionState(void);

  // parse given url to internal variables for specified protocol
  // errors should be logged to log file
  // @param parameters : the url and connection parameters
  // @return : S_OK if successfull
  virtual HRESULT ParseUrl(const CParameterCollection *parameters);

  // receives data and process stream package request
  // the method can't block call (method is called within thread which can be terminated anytime)
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful, error code only in case when error is not related to processing request
  virtual HRESULT ReceiveData(CStreamPackage *streamPackage);

  // ISimpleProtocol interface

  // gets timeout (in ms) for opening connection
  // @return : timeout (in ms) for opening connection
  virtual unsigned int GetOpenConnectionTimeout(void);

  // gets sleep time (in ms) for opening connection
  // some protocols may need some sleep before loading (e.g. multicast UDP protocol needs some time between unsubscribing and subscribing in multicast groups)
  // @return : sleep time (in ms) for opening connection
  virtual unsigned int GetOpenConnectionSleepTime(void);

  // gets total timeout (in ms) for re-opening connection (opening connection after lost connection)
  // re-open connection total timeout should be much more greater (e.g. 3 - 5 times) in order to allow more opening requests
  // @return : total timeout (in ms) for re-opening connection
  virtual unsigned int GetTotalReopenConnectionTimeout(void);

  // starts receiving data from specified url and configuration parameters
  // @param parameters : the url and parameters used for connection
  // @return : S_OK if url is loaded, error code otherwise
  virtual HRESULT StartReceivingData(CParameterCollection *parameters);

  // request protocol implementation to cancel the stream reading operation
  // @return : S_OK if successful
  virtual HRESULT StopReceivingData(void);

  // clears current session
  virtual void ClearSession(void);

  // reports actual stream time to protocol
  // @param streamTime : the actual stream time in ms to report to protocol
  // @param streamPosition : the actual stream position (related to stream time) to report to protocol
  virtual void ReportStreamTime(uint64_t streamTime, uint64_t streamPosition);

  // gets information about streams
  // receiving data is disabled until protocol reports valid stream count (at least one)
  // @return : S_OK if successful, E_STREAM_COUNT_UNKNOWN if stream count is unknown, error code otherwise
  virtual HRESULT GetStreamInformation(CStreamInformationCollection *streams);

  // ISeeking interface

  // gets seeking capabilities of protocol
  // @return : bitwise combination of SEEKING_METHOD flags
  virtual unsigned int GetSeekingCapabilities(void);

  // request protocol implementation to receive data from specified time (in ms) for specified stream
  // this method is called with same time for each stream in protocols with multiple streams
  // @param streamId : the stream ID to receive data from specified time
  // @param time : the requested time (zero is start of stream)
  // @return : time (in ms) where seek finished or lower than zero if error
  virtual int64_t SeekToTime(unsigned int streamId, int64_t time);

  // set pause, seek or stop mode
  // in such mode are reading operations disabled
  // @param pauseSeekStopMode : one of PAUSE_SEEK_STOP_MODE values
  virtual void SetPauseSeekStopMode(unsigned int pauseSeekStopMode);

  /* get methods */

  // gets parser result about current stream
  // @return : one of PARSER_RESULT values
  virtual HRESULT GetParserResult(void);

  // gets parser score if parser result is PARSER_RESULT_KNOWN
  // @return : parser score (parser with highest score is set as active parser)
  virtual unsigned int GetParserScore(void) = 0;

  // gets parser action after parser recognizes stream
  // @return : one of Action values
  virtual Action GetAction(void) = 0;

  /* set methods */

  // sets current connection url and parameters
  // @param parameters : the collection of url and connection parameters
  // @return : S_OK if successful
  virtual HRESULT SetConnectionParameters(const CParameterCollection *parameters);

  /* other methods */

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
  // holds logger instance
  CLogger *logger;
  // holds configuration
  CParameterCollection *configuration;
  // holds protocol hoster - only reference, do not cleanup !
  CProtocolHoster *protocolHoster;
  // holds parser result
  HRESULT parserResult;
  // holds connection parameters
  CParameterCollection *connectionParameters;
  // holds reported stream time and position
  uint64_t reportedStreamTime;
  uint64_t reportedStreamPosition;
  // holds dump file
  CDumpFile *dumpFile;

  /* methods */

  // gets store file name
  // @param extension : the extension of store file
  // @return : store file name or NULL if error
  virtual wchar_t *GetStoreFile(const wchar_t *extension) = 0;

};

#endif