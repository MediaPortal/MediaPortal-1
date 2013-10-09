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

#ifndef __ISIMPLE_PROTOCOL_DEFINED
#define __ISIMPLE_PROTOCOL_DEFINED

#include "ParameterCollection.h"
#include "StreamAvailableLength.h"
#include "ISeeking.h"

#include <streams.h>

#define METHOD_START_RECEIVING_DATA_NAME                                      L"StartReceivingData()"
#define METHOD_STOP_RECEIVING_DATA_NAME                                       L"StopReceivingData()"
#define METHOD_QUERY_STREAM_PROGRESS_NAME                                     L"QueryStreamProgress()"
#define METHOD_CLEAR_SESSION_NAME                                             L"ClearSession()"

#ifndef DURATION_UNSPECIFIED

#define DURATION_UNSPECIFIED                                                  -2
#define DURATION_LIVE_STREAM                                                  -1

#endif

// defines interface for simple stream protocol
struct ISimpleProtocol : public ISeeking
{
public:
  // get timeout (in ms) for receiving data
  // @return : timeout (in ms) for receiving data
  virtual unsigned int GetReceiveDataTimeout(void) = 0;

  // starts receiving data from specified url and configuration parameters
  // @param parameters : the url and parameters used for connection
  // @return : S_OK if url is loaded, false otherwise
  virtual HRESULT StartReceivingData(CParameterCollection *parameters) = 0;

  // request protocol implementation to cancel the stream reading operation
  // @return : S_OK if successful
  virtual HRESULT StopReceivingData(void) = 0;

  // retrieves the progress of the stream reading operation
  // @param total : reference to a variable that receives the length of the entire stream, in bytes
  // @param current : reference to a variable that receives the length of the downloaded portion of the stream, in bytes
  // @return : S_OK if successful, VFW_S_ESTIMATED if returned values are estimates, E_UNEXPECTED if unexpected error
  virtual HRESULT QueryStreamProgress(LONGLONG *total, LONGLONG *current) = 0;
  
  // retrieves available lenght of stream
  // @param available : reference to instance of class that receives the available length of stream, in bytes
  // @return : S_OK if successful, other error codes if error
  virtual HRESULT QueryStreamAvailableLength(CStreamAvailableLength *availableLength) = 0;

  // clear current session
  // @return : S_OK if successfull
  virtual HRESULT ClearSession(void) = 0;

  // gets duration of stream in ms
  // @return : stream duration in ms or DURATION_LIVE_STREAM in case of live stream or DURATION_UNSPECIFIED if duration is unknown
  virtual int64_t GetDuration(void) = 0;

  // reports actual stream time to protocol
  // @param streamTime : the actual stream time in ms to report to protocol
  virtual void ReportStreamTime(uint64_t streamTime) = 0;
};

typedef ISimpleProtocol* PISimpleProtocol;

#endif