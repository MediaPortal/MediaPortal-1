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

#ifndef __IPROTOCOL_DEFINED
#define __IPROTOCOL_DEFINED

#include "ISimpleProtocol.h"
#include "StreamPackage.h"

#include <streams.h>

#define METHOD_PARSE_URL_NAME                                                 L"ParseUrl()"
#define METHOD_RECEIVE_DATA_NAME                                              L"ReceiveData()"

enum ProtocolConnectionState
{
  None,
  Initializing,
  Initialized,
  InitializeFailed,
  Opening,
  OpeningFailed,
  Opened,
  Closing
};

// defines interface for stream protocol implementation
struct IProtocol : virtual public ISimpleProtocol
{
public:
  // gets connection state
  // @return : one of protocol connection state values
  virtual ProtocolConnectionState GetConnectionState(void) = 0;

  // parse given url to internal variables for specified protocol
  // errors should be logged to log file
  // @param parameters : the url and connection parameters
  // @return : S_OK if successfull
  virtual HRESULT ParseUrl(const CParameterCollection *parameters) = 0;

  // receives data and process stream package request
  // the method can't block call (method is called within thread which can be terminated anytime)
  // @param streamPackage : the stream package request to process
  // @return : S_OK if successful (long sleep), S_FALSE if successful (short sleep), error code only in case when error is not related to processing request
  virtual HRESULT ReceiveData(CStreamPackage *streamPackage) = 0;

  // gets current connection parameters (can be different as supplied connection parameters)
  // @param parameters : the reference to parameter collection to be filled with connection parameters
  // @return : S_OK if successful, error code otherwise
  virtual HRESULT GetConnectionParameters(CParameterCollection *parameters) = 0;
};

#endif