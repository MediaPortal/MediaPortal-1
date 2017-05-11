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

#ifndef __MPIPTV_RTSP_DEFINE_DEFINED
#define __MPIPTV_RTSP_DEFINE_DEFINED

#include "MPIPTV_RTSP_Exports.h"
#include "MPIPTV_UDP.h"

#include "MPRTSPClient.h"
#include "MPTaskScheduler.h"
#include "BasicUsageEnvironment.hh"
#include "Groupsock.hh"
#include "BasicUDPSink.hh"
#include <ctime>

// we should get data in ten seconds
#define RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT                   10000
#define RTSP_PORT_DEFAULT                                   554
#define RTSP_MAX_RESPONSE_BYTE_COUNT                        4096
#define RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT            0
#define RTSP_RTP_CLIENT_PORT_RANGE_END_DEFAULT              0
#define RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT              12288
#define RTSP_UDP_PORT_RANGE_START_DEFAULT                   45000
#define RTSP_UDP_PORT_RANGE_END_DEFAULT                     46000
#define RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT                1000
#define RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT       3
#define RTSP_SEND_COMMAND_OPTIONS_DEFAULT                   true
#define RTSP_SEND_COMMAND_DESCRIBE_DEFAULT                  true
#define RTSP_KEEP_ALIVE_WITH_OPTIONS_DEFAULT                false

#define CONFIGURATION_SECTION_RTSP                          _T("RTSP")

#define CONFIGURATION_RTSP_RECEIVE_DATA_TIMEOUT             _T("RtspReceiveDataTimeout")
#define CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_START      _T("RtspRtpClientPortRangeStart")
#define CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_END        _T("RtspRtpClientPortRangeEnd")
#define CONFIGURATION_RTSP_UDP_SINK_MAX_PAYLOAD_SIZE        _T("RtspUdpSinkMaxPayloadSize")
#define CONFIGURATION_RTSP_UDP_PORT_RANGE_START             _T("RtspUdpPortRangeStart")
#define CONFIGURATION_RTSP_UDP_PORT_RANGE_END               _T("RtspUdpPortRangeEnd")
#define CONFIGURATION_RTSP_OPEN_CONNECTION_TIMEOUT          _T("RtspOpenConnectionTimeout")
#define CONFIGURATION_RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS _T("RtspOpenConnectionMaximumAttempts")
#define CONFIGURATION_RTSP_SEND_COMMAND_OPTIONS             _T("RtspSendCommandOptions")
#define CONFIGURATION_RTSP_SEND_COMMAND_DESCRIBE            _T("RtspSendCommandDescribe")
#define CONFIGURATION_RTSP_KEEP_ALIVE_WITH_OPTIONS          _T("RtspKeepAliveWithOptions")

// returns protocol class instance
PIProtocol CreateProtocolInstance(void);

// destroys protocol class instance
void DestroyProtocolInstance(PIProtocol pProtocol);

// This class is exported from the MPIPTV_RTSP.dll
class MPIPTV_RTSP_API CMPIPTV_RTSP : public CMPIPTV_UDP
{
public:
  // constructor
  // create instance of CMPIPTV_RTSP class
  CMPIPTV_RTSP(void);

  // destructor
  ~CMPIPTV_RTSP(void);

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
  TCHAR *rtspUrl;
  MPRTSPClient *rtspClient;
  MediaSession *rtspSession;
  bool isRtspSessionSetup;
  unsigned int rtspSessionTimeout;
  unsigned int openConnectionTimeout;
  HANDLE openConnectionResultEvent;
  bool sendRtspCommandOptions;
  bool sendRtspCommandDescribe;
  bool keepAliveWithOptions;

  FramedSource *rtpSource;
  unsigned int rtpClientPortRangeStart;
  unsigned int rtpClientPortRangeEnd;

  TCHAR *udpUrl;
  Groupsock *udpGroupsock;
  MediaSink *udpSink;
  unsigned int udpSinkMaxPayloadSize;
  unsigned int udpPortRangeStart;
  unsigned int udpPortRangeEnd;

  MPTaskScheduler *live555Scheduler;
  UsageEnvironment *live555Environment; 
  volatile char live555WorkerThreadShouldExit;
  DWORD live555WorkerThreadId;
  HANDLE live555WorkerThreadHandle;

  static unsigned long ElapsedMillis(clock_t start)
  {
    return (clock() - start) * 1000 / CLOCKS_PER_SEC;
  }

  // log the most recent LIVE555 message
  void LogLive555Message(unsigned int loggerLevel, const TCHAR *method, const TCHAR *message);

  // LIVE555 worker thread function, for all RTSP handling
  // @param lpParam : reference to instance of CMPIPTV_RTSP class
  static DWORD WINAPI Live555Worker(LPVOID lpParam);

  int StartOpenConnection(void);
  void OnGenericResponseReceived(const TCHAR *command, RTSPClient *client, int resultCode, char *resultString);
  static void OnOptionsResponseReceived(RTSPClient *client, int resultCode, char *resultString);
  static void OnDescribeResponseReceived(RTSPClient *client, int resultCode, char *resultString);
  void SetupRtspSession(void);
  static void OnSetupResponseReceived(RTSPClient *client, int resultCode, char *resultString);
  static void OnPlayResponseReceived(RTSPClient *client, int resultCode, char *resultString);
  void SetupLocalUdpConnection(void);

  static void OnTeardownResponseReceived(RTSPClient *client, int resultCode, char *resultString);
  void CleanUpLive555(void);

  static void RtspSessionByeHandler(void *lpCMPIPTV_RTSP);
};

#endif
