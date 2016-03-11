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

#ifndef __MPIPTV_UDP_DEFINE_DEFINED
#define __MPIPTV_UDP_DEFINE_DEFINED

#include "MPIPTV_UDP_Exports.h"
#include "Logger.h"
#include "ProtocolInterface.h"
#include "LinearBuffer.h"

#include <WinSock2.h>
#include <Ws2tcpip.h>

// we should get data in two seconds
#define UDP_RECEIVE_DATA_TIMEOUT_DEFAULT                    2000
#define UDP_INTERNAL_BUFFER_MULTIPLIER_DEFAULT              8
#define UDP_INTERNAL_BUFFER_MAX_MULTIPLIER_DEFAULT          1024
#define UDP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT        3

#define CONFIGURATION_SECTION_UDP                           _T("UDP")

#define CONFIGURATION_UDP_RECEIVE_DATA_TIMEOUT              _T("UdpReceiveDataTimeout")
#define CONFIGURATION_UDP_INTERNAL_BUFFER_MULTIPLIER        _T("UdpInternalBufferMultiplier")
#define CONFIGURATION_UDP_INTERNAL_BUFFER_MAX_MULTIPLIER    _T("UdpInternalBufferMaxMultiplier")
#define CONFIGURATION_UDP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS  _T("UdpOpenConnectionMaximumAttempts")

// returns protocol class instance
PIProtocol CreateProtocolInstance(void);

// destroys protocol class instance
void DestroyProtocolInstance(PIProtocol pProtocol);

// This class is exported from the MPIPTV_UDP.dll
class MPIPTV_UDP_API CMPIPTV_UDP : public IProtocol
{
public:
  // constructor
  // create instance of CMPITV_UDP class
  CMPIPTV_UDP(void);

  // destructor
  ~CMPIPTV_UDP(void);

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

protected:
  CLogger logger;
  SOCKET m_socket;

  HANDLE lockMutex;

  TCHAR *localAddress;
  WORD localPort;
  TCHAR *sourceAddress;
  WORD sourcePort;

  unsigned int packetSize;         // size of one packet in buffer
  char *receiveBuffer;    // internal receive buffer - must be long enough to not lost data (especially for UDP)
  LinearBuffer buffer;    // internal buffer which is used for storing data and sending to MediaPortal

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

  // specifies if input packets have to be dumped to file
  bool dumpInputPackets;

  // chains of ADDRINFO structures for remote and local server
  ADDRINFOT *local;
  ADDRINFOT *source;

  // variables for current values
  ADDRINFOT *currentLocalAddr;
  ADDRINFOT *currentSourceAddr;

  // holds open connection maximum attempts
  unsigned int openConnetionMaximumAttempts;
};

#endif
