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

#ifndef __MPIPTV_RTP_DEFINE_DEFINED
#define __MPIPTV_RTP_DEFINE_DEFINED

#include "Logger.h"
#include "ProtocolInterface.h"
#include "LinearBuffer.h"
#include "MPIPTV_UDP.h"
#include "RtpSource.h"

//number of wrong (= wrong header) packets after the rtp-stream is considered as raw udp instead of rtp
#define RTP_MAX_FAILED_PACKETS_DEFAULT                      5

// we should get data in two seconds
#define RTP_RECEIVE_DATA_TIMEOUT_DEFAULT                    2000
#define RTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT        3

#define CONFIGURATION_SECTION_RTP                           _T("RTP")

#define CONFIGURATION_RTP_RECEIVE_DATA_TIMEOUT              _T("RtpReceiveDataTimeout")
#define CONFIGURATION_RTP_MAX_FAILED_PACKETS                _T("RtpMaxFailedPackets")
#define CONFIGURATION_RTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS  _T("RtpOpenConnectionMaximumAttempts")

// returns protocol class instance
PIProtocol CreateProtocolInstance(void);

// destroys protocol class instance
void DestroyProtocolInstance(PIProtocol pProtocol);

// This class is exported from the MPIPTV_RTP.dll
class MPIPTV_RTP_API CMPIPTV_RTP : public CMPIPTV_UDP
{
public:
  // constructor
  // create instance of CMPITV_RTP class
  CMPIPTV_RTP(void);

  // destructor
  ~CMPIPTV_RTP(void);

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
  unsigned int GetOpenConnectionMaximumAttempts(void);

protected:
  // handler of RTP protocol
  RtpSource *rtpHandler;

  // stores the number of failed RTP packets
  unsigned int rtpFailPackets;

  // hold maximum count of failed RTP packets
  unsigned maxFailedPackets;

  // specifies if RTP protocol is switched to UDP
  bool switchedToUdp;
};

#endif
