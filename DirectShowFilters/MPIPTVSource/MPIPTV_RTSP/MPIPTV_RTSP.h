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
#include "BasicUsageEnvironment.hh"
#include "MPEG2TransportStreamFromESSource.hh"
#include "Groupsock.hh"
#include "BasicUDPSink.hh"
#include "RtspTaskScheduler.h"

// we should get data in ten seconds
#define RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT                   10000
#define RTSP_PORT_DEFAULT                                   554
#define RTSP_MAX_RESPONSE_BYTE_COUNT                        4096
#define RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT            0
#define RTSP_RTP_CLIENT_PORT_RANGE_END_DEFAULT              0
#define RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT              12288
#define RTSP_UDP_PORT_RANGE_START_DEFAULT                   45000
#define RTSP_UDP_PORT_RANGE_END_DEFAULT                     46000
#define RTSP_COMMAND_RESPONSE_TIMEOUT_DEFAULT               100
#define RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT       3

#define CONFIGURATION_SECTION_RTSP                          _T("RTSP")

#define CONFIGURATION_RTSP_RECEIVE_DATA_TIMEOUT             _T("RtspReceiveDataTimeout")
#define CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_START      _T("RtspRtpClientPortRangeStart")
#define CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_END        _T("RtspRtpClientPortRangeEnd")
#define CONFIGURATION_RTSP_UDP_SINK_MAX_PAYLOAD_SIZE        _T("RtspUdpSinkMaxPayloadSize")
#define CONFIGURATION_RTSP_UDP_PORT_RANGE_START             _T("RtspUdpPortRangeStart")
#define CONFIGURATION_RTSP_UDP_PORT_RANGE_END               _T("RtspUdpPortRangeEnd")
#define CONFIGURATION_RTSP_COMMAND_RESPONSE_TIMEOUT         _T("RtspCommandResponseTimeout")
#define CONFIGURATION_RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS _T("RtspOpenConnectionMaximumAttempts")

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

  // RTSP variables
  TaskScheduler *rtspScheduler;
  UsageEnvironment *rtspEnvironment; 
  MPRTSPClient *rtspClient;
  HANDLE rtspResponseEvent;
  int rtspResponseResultCode;
  char rtspResponseResultString[RTSP_MAX_RESPONSE_BYTE_COUNT];
  MediaSession *rtspSession;
  unsigned int rtspRtpClientPortRangeStart;
  unsigned int rtspRtpClientPortRangeEnd;
  Groupsock *rtspUdpGroupsock;
  MediaSink *rtspUdpSink;
  unsigned int rtspUdpSinkMaxPayloadSize;
  unsigned int rtspUdpPortRangeStart;
  unsigned int rtspUdpPortRangeEnd;
  unsigned int rtspCommandResponseTimeout;

  // variable for signaling exit for rtspScheduler
  char rtspThreadShouldExit;
  // variables for RTSP scheduler thread
  DWORD rtspSchedulerThreadId;
  HANDLE rtspSchedulerThreadHandle;

  int SendRtspCommand(const TCHAR *method, const TCHAR *command, MediaSubsession *subsession = NULL);

  // RTSP request asynchronous response handler.
  static void OnRtspResponseReceived(RTSPClient *client, int resultCode, char *resultString);

  // log RTSP message
  void LogRtspMessage(unsigned int loggerLevel, const TCHAR *method, const TCHAR *message);

  // RTSP scheduler worker method
  // @param lpParam : reference to instance of CMPIPTV_RTSP class
  static DWORD WINAPI RtspSchedulerWorker(LPVOID lpParam);

  // RTSP subsession 'Bye' handler
  static void SubsessionByeHandler(void *lpCMPIPTV_RTSP);

  // tear down media session
  // @param forceTeardown : if true than session and client will be deleted in any case
  // @result : true if succesful, false otherwise
  bool TeardownMediaSession(bool forceTeardown);

};

#endif
