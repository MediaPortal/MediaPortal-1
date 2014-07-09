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

#ifndef __PARSER_HOSTER_DEFINED
#define __PARSER_HOSTER_DEFINED

#include "Hoster.h"
#include "ProtocolHoster.h"
#include "ParserPlugin.h"
#include "IDemuxerOwner.h"

#define MODULE_PARSER_HOSTER_NAME                                             L"ParserHoster"

class CParserHoster : public CHoster, virtual public IDemuxerOwner, virtual public ISimpleProtocol
{
public:
  CParserHoster(HRESULT *result, CLogger *logger, CParameterCollection *configuration);
  virtual ~CParserHoster(void);

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

  // ISimpleProtocol interface implementation

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
  // @return : S_OK if successfull
  HRESULT ClearSession(void);

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

  // IDemuxerOwner interface implementation

  // process stream package request
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful, error code only in case when error is not related to processing request
  HRESULT ProcessStreamPackage(CStreamPackage *streamPackage);

  /* other methods */

  // loads plugins from directory
  // @return : S_OK if successful, E_NO_PARSER_LOADED if no parser loaded, error code otherwise
  virtual HRESULT LoadPlugins(void);

protected:
  // hoster for all protocols
  CProtocolHoster *protocolHoster;

  // reference to active parser
  CParserPlugin *activeParser;

  /* methods */

  // creates hoster plugin metadata
  // @param result : the reference to result
  // @param logger : the reference to logger
  // @param configuration : the reference to configuration
  // @param hosterName : the hoster name
  // @param pluginLibraryFileName : the plugin library file name
  // @result : hoster plugin metadata instance
  virtual CHosterPluginMetadata *CreateHosterPluginMetadata(HRESULT *result, CLogger *logger, CParameterCollection *configuration, const wchar_t *hosterName, const wchar_t *pluginLibraryFileName);

  // creates plugin configuration
  // @param result : the reference to result
  // @param configuration : the collection of parameters
  // @result : plugin configuration instance
  virtual CPluginConfiguration *CreatePluginConfiguration(HRESULT *result, CParameterCollection *configuration);
};

#endif