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
#include "ReceiveData.h"

#include <streams.h>

#define METHOD_PARSE_URL_NAME                                                 L"ParseUrl()"
#define METHOD_RECEIVE_DATA_NAME                                              L"ReceiveData()"

// defines interface for stream protocol implementation
struct IProtocol : public ISimpleProtocol
{
public:
  // test if connection is opened
  // @return : true if connected, false otherwise
  virtual bool IsConnected(void) = 0;

  // parse given url to internal variables for specified protocol
  // errors should be logged to log file
  // @param parameters : the url and connection parameters
  // @return : S_OK if successfull
  virtual HRESULT ParseUrl(const CParameterCollection *parameters) = 0;

  // receives data and stores them into receive data parameter
  // the method should fill receiveData parameter with relevant data and finish
  // the method can't block call (method is called within thread which can be terminated anytime)
  // @param receiveData : received data
  // @result: S_OK if successful, error code otherwise
  virtual HRESULT ReceiveData(CReceiveData *receiveData) = 0;

  // gets current connection parameters (can be different as supplied connection parameters)
  // @return : current connection parameters or NULL if error
  virtual CParameterCollection *GetConnectionParameters(void) = 0;
};

typedef IProtocol* PIProtocol;

#endif