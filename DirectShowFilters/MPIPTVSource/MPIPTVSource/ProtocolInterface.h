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

#ifndef __PROTOCOLINTERFACE_DEFINED
#define __PROTOCOLINTERFACE_DEFINED

#include <streams.h>

#include "MPIPTVSourceExports.h"
#include "ParameterCollection.h"

// logging constants

// methods' names
#define METHOD_CONSTRUCTOR_NAME                                         _T("ctor()")
#define METHOD_DESTRUCTOR_NAME                                          _T("dtor()")
#define METHOD_CLEAR_SESSION_NAME                                       _T("ClearSession()")
#define METHOD_INITIALIZE_NAME                                          _T("Initialize()")
#define METHOD_PARSE_URL_NAME                                           _T("ParseUrl()")
#define METHOD_OPEN_CONNECTION_NAME                                     _T("OpenConnection()")
#define METHOD_CLOSE_CONNECTION_NAME                                    _T("CloseConnection()")
#define METHOD_RECEIVE_DATA_NAME                                        _T("ReceiveData()")
#define METHOD_FILL_BUFFER_NAME                                         _T("FillBuffer()")

// methods' common string formats
#define METHOD_START_FORMAT                                             _T("%s: %s: Start")
#define METHOD_END_FORMAT                                               _T("%s: %s: End")
#define METHOD_END_FAIL_FORMAT                                          _T("%s: %s: End, Fail")
#define METHOD_MESSAGE_FORMAT                                           _T("%s: %s: %s")

// return values of protocol methods
// no error
#define STATUS_OK                                       0
// error
#define STATUS_ERROR                                    -1
// error, do not retry call method
#define STATUS_ERROR_NO_RETRY                           -2

// MPEG2 Transport Stream constants
#define     PID_COUNT                                   0x2000
#define     DVB_PACKET_SIZE                             188

#define     SYNC_BYTE                                                 0x47
#define     PID_PAT                                                   0x0000
#define     PID_NULL                                                  0x1FFF
#define     MAX_RESERVED_PID                                          0x000F

// defines interface for stream protocol implementation
// each stream protocol implementation will be in separate library
struct IProtocol
{
public:
  // return reference to null-terminated string which represents protocol name
  // function have to allocate enough memory for protocol name string
  // errors should be logged to log file and returned NULL
  // @return : reference to null-terminated string
  virtual TCHAR *GetProtocolName(void) = 0;

  // initialize protocol implementation with configuration parameters
  // @param lockMutex : the mutex to lock access to buffer
  // @param : the reference to configuration parameters
  // @return : STATUS_OK if successfull
  virtual int Initialize(HANDLE lockMutex, CParameterCollection *configuration) = 0;

  // clear current session before running ParseUrl() method
  // @return : STATUS_OK if successfull
  virtual int ClearSession(void) = 0;

  // parse given url to internal variables for specified protocol
  // errors should be logged to log file
  // @param url : the url to parse
  // @param parameters : the reference to collection of parameters
  // @return : STATUS_OK if successfull
  virtual int ParseUrl(const TCHAR *url, const CParameterCollection *parameters) = 0;

  // open connection
  // errors should be logged to log file
  // @return : STATUS_OK if successfull
  virtual int OpenConnection(void) = 0;

  // test if connection is opened
  // @return : true if connected, false otherwise
  virtual int IsConnected(void) = 0;

  // close connection
  // errors should be logged to log file
  virtual void CloseConnection(void) = 0;

  // safe get buffer sizes
  // implementation must lock before getting sizes and release lock after
  // if any of parameters freeSpace, occupiedSpace and bufferSize is NULL, then its value is not required
  // @param lockMutex : the mutex to lock access to buffer
  // @param freeSpace : the free space in buffer
  // @param occupiedSpace : the occupied space in buffer
  // @param bufferSize : the size of buffer
  virtual void GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize) = 0;

  // receive data and stores them into internal buffer
  // @param shouldExit : the reference to variable specifying if method have to be finished immediately
  virtual void ReceiveData(bool *shouldExit) = 0;

  // fill media sample with buffer data
  // @param pSamp :
  // @param pData :
  // @param cbData :
  // @return : count of bytes written to media sample
  virtual unsigned int FillBuffer(IMediaSample *pSamp, char *pData, long cbData) = 0;

  // get timeout (in ms) for receiving data
  // @return : timeout (in ms) for receiving data
  virtual unsigned int GetReceiveDataTimeout(void) = 0;

  // get protocol instance ID
  // @return : GUID, which represents instance identifier
  virtual GUID GetInstanceId(void) = 0;

  // get protocol maximum open connection attempts
  // @return : maximum attempts of opening connections
  virtual unsigned int GetOpenConnectionMaximumAttempts(void) = 0;
};

typedef IProtocol* PIProtocol;

extern "C"
{
  PIProtocol CreateProtocolInstance(CParameterCollection *configuration);
  typedef PIProtocol (*CREATEPROTOCOLINSTANCE)(void);

  void DestroyProtocolInstance(PIProtocol pProtocol);
  typedef void (*DESTROYPROTOCOLINSTANCE)(PIProtocol);
}


#endif
