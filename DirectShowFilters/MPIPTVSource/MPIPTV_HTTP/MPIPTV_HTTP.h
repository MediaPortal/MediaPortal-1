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

#ifndef __MPIPTV_HTTP_DEFINE_DEFINED
#define __MPIPTV_HTTP_DEFINE_DEFINED

#include "MPIPTV_HTTP_Exports.h"
#include "Logger.h"
#include "ProtocolInterface.h"
#include "LinearBuffer.h"

#include <WinSock2.h>

// we should get data in twenty seconds
#define HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT                   20000
#define HTTP_INTERNAL_BUFFER_MULTIPLIER_DEFAULT             8
#define HTTP_INTERNAL_BUFFER_MAX_MULTIPLIER_DEFAULT         1024
#define HTTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT       3

#define CONFIGURATION_SECTION_HTTP                          _T("HTTP")

#define CONFIGURATION_HTTP_RECEIVE_DATA_TIMEOUT             _T("HttpReceiveDataTimeout")
#define CONFIGURATION_HTTP_INTERNAL_BUFFER_MULTIPLIER       _T("HttpInternalBufferMultiplier")
#define CONFIGURATION_HTTP_INTERNAL_BUFFER_MAX_MULTIPLIER   _T("HttpInternalBufferMaxMultiplier")
#define CONFIGURATION_HTTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS _T("HttpOpenConnectionMaximumAttempts")

#define HTTP_NON_STANDARD_REQUEST_FORMAT                    _T("GET %s HTTP/1.1\r\nHost: %s:%d\r\nUser-Agent: vlc/1.1.8\r\nRange: bytes=0-\r\n\r\n")
#define HTTP_STANDARD_REQUEST_FORMAT                        _T("GET %s HTTP/1.1\r\nHost: %s\r\nUser-Agent: vlc/1.1.8\r\nRange: bytes=0-\r\n\r\n")

// returns protocol class instance
PIProtocol CreateProtocolInstance(void);

// destroys protocol class instance
void DestroyProtocolInstance(PIProtocol pProtocol);

// This class is exported from the MPIPTV_HTTP.dll
class MPIPTV_HTTP_API CMPIPTV_HTTP : public IProtocol
{
public:
  // constructor
  // create instance of CMPITV_HTTP class
  CMPIPTV_HTTP(void);

  // destructor
  ~CMPIPTV_HTTP(void);

  /* IProtocol interface */
  TCHAR *GetProtocolName(void);
  int Initialize(HANDLE lockMutex, CParameterCollection *configuration);
  int ClearSession(void);
  int ParseUrl(const TCHAR *url, const CParameterCollection *parameters);
  int OpenConnection(void);
  int IsConnected(void);
  void CloseConnection(void);
  void GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize);
  void ReceiveData(bool *shouldExit);
  unsigned int FillBuffer(IMediaSample *pSamp, char *pData, long cbData);
  unsigned int GetReceiveDataTimeout(void);
  GUID GetInstanceId(void);
  unsigned int GetOpenConnectionMaximumAttempts(void);
  
  // gets internal receive buffer
  LinearBuffer *GetBuffer(void);

  // gets internal buffer for chunked encoding
  LinearBuffer *GetChunkedBuffer(void);

protected:
  CLogger logger;
  SOCKET m_socket;

  TCHAR *server;
  WORD serverPort;
  TCHAR *serverGetString;

  // some streams are provided in "chunks" - blocks of specified length (not necessary same)
  bool chunkedEncoding;

  // signalize that we received HTTP server response
  bool receivedHttpResponse;

  HANDLE lockMutex;

  char *receiveBuffer;            // internal receive buffer - must be long enough to not lost data (especially for UDP)
  LinearBuffer buffer;            // internal buffer which is used for storing data and sending to MediaPortal
  LinearBuffer chunkedBuffer;     // internal receive buffer for chunked data

  // holds various parameters supplied by TvService
  CParameterCollection *configurationParameters;
  // holds various parameters supplied by TvService when loading url
  CParameterCollection *loadParameters;

  // holds default buffer size
  unsigned int defaultBufferSize;

  // holds maximum buffer size
  unsigned int maxBufferSize;

  // holds receive data timeout
  unsigned int receiveDataTimeout;

  // holds open connection maximum attempts
  unsigned int openConnetionMaximumAttempts;

  // specifies if input packets have to be dumped to file
  bool dumpInputPackets;
};

#endif
