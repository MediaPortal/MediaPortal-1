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

#include "OutputStreamHoster.h"
#include "IParserPlugin.h"
#include "ProtocolHoster.h"
#include "IParserOutputStream.h"
#include "ISimpleProtocol.h"

#define STATUS_NONE                                                           0
#define STATUS_RECEIVING_DATA                                                 1
#define STATUS_PARSER_PENDING                                                 2
#define STATUS_NEW_URL_SPECIFIED                                              3

#define MODULE_PARSER_HOSTER_NAME                                             L"ParserHoster"

#define METHOD_RECEIVE_DATA_WORKER_NAME                                       L"ReceiveDataWorker()"
#define METHOD_CREATE_RECEIVE_DATA_WORKER_NAME                                L"CreateReceiveDataWorker()"
#define METHOD_DESTROY_RECEIVE_DATA_WORKER_NAME                               L"DestroyReceiveDataWorker()"

struct ParserImplementation : public PluginImplementation
{
  ParseResult result;
};

class CParserHoster : public COutputStreamHoster, public ISimpleProtocol
{
public:
  CParserHoster(CLogger *logger, CParameterCollection *configuration, IParserOutputStream *parserOutputStream);
  ~CParserHoster(void);

  // ISimpleProtocol interface implementation

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

  // ISeeking interface implementation

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

  // IOutputStream interface implementation

  // sets total length of stream to output pin
  // @param total : total length of stream in bytes
  // @param estimate : specifies if length is estimate
  // @return : S_OK if successful
  HRESULT SetTotalLength(int64_t total, bool estimate);

  // pushes media packets to filter
  // @param mediaPackets : collection of media packets to push to filter
  // @return : S_OK if successful
  HRESULT PushMediaPackets(CMediaPacketCollection *mediaPackets);

  // notifies output stream that end of stream was reached
  // this method can be called only when protocol support SEEKING_METHOD_POSITION
  // @param streamPosition : the last valid stream position
  // @return : S_OK if successful
  HRESULT EndOfStreamReached(int64_t streamPosition);

  // gets parser hoster status
  // @return : one of STATUS_* values or error code if error
  HRESULT GetParserHosterStatus(void);

protected:
  // hoster methods

  // allocates memory for plugin implementations in specific hoster
  // @param maxPlugins : the maximum plugins for hoster
  // @return : allocated memory or NULL if error
  PluginImplementation *AllocatePluginsMemory(unsigned int maxPlugins);

  // gets plugins implementation at specified position
  // @param position : the plugin position
  // @return : reference to plugin implementation or NULL if error
  PluginImplementation *GetPluginImplementation(unsigned int position);

  // appends parser implementation to end of parsers implementations
  // @param plugin : reference to parser implementation structure
  // @return : true if successful, false otherwise (in that case MUST be called RemovePluginImplementation() method)
  bool AppendPluginImplementation(HINSTANCE hLibrary, DESTROYPLUGININSTANCE destroyPluginInstance, PIPlugin plugin);

  // removes last plugin implementation
  void RemovePluginImplementation(void); 

  // gets parser configuration for Initialize() method
  // @return : parser configuration
  PluginConfiguration *GetPluginConfiguration(void);

  // status of processing
  int status;
  HANDLE hReceiveDataWorkerThread;
  volatile bool receiveDataWorkerShouldExit;
  static unsigned int WINAPI ReceiveDataWorker(LPVOID lpParam);

  // creates receive data worker
  // @return : S_OK if successful
  HRESULT CreateReceiveDataWorker(void);

  // destroys receive data worker
  // @return : S_OK if successful
  HRESULT DestroyReceiveDataWorker(void);

  // hoster for all protocols
  CProtocolHoster *protocolHoster;

  // reference to parser output stream methods
  IParserOutputStream *parserOutputStream;

  // reference to parser plugin which return ParseResult::Known result
  IParserPlugin *parsingPlugin;

  // specifies if hoster have to parse media packets
  bool parseMediaPackets;

  // specifies if SetTotalLength() method was called
  bool setTotalLengthCalled;
  // holds last SetTotalLength() method call parameters
  int64_t total;
  bool estimate;

  // specifies if EndOfStreamReached() method was called
  bool endOfStreamReachedCalled;
  // holds last EndOfStreamReached() method call parameters
  int64_t streamPosition;

  // holds if data are supressed
  bool supressData;
  // specifies if we are in StartReceiveData() method (it is required to set correct parameters to StartReceivingData() of protocol
  volatile bool startReceivingData;
  // holds finish time for protocol (protocol must return from StartReceivingData() to this time)
  // it is acquired be GetTickCount() (time in ms)
  volatile unsigned int finishTime;

  // mutex for locking access to file, buffer, ...
  HANDLE lockMutex;
};

#endif