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

#include "RTSPClient.hh"
#include "BasicUsageEnvironment.hh"
#include "MPEG2TransportStreamFromESSource.hh"
#include "Groupsock.hh"
#include "BasicUDPSink.hh"
#include "RtspTaskScheduler.h"

// we should get data in ten seconds
#define RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT                   10000
#define RTSP_PORT_DEFAULT                                   554
#define RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT              12288
#define RTSP_UDP_PORT_RANGE_START_DEFAULT                   45000
#define RTSP_UDP_PORT_RANGE_END_DEFAULT                     46000
#define RTSP_TEARDOWN_REQUEST_MAXIMUM_COUNT_DEFAULT         5
#define RTSP_TEARDOWN_REQUEST_TIMEOUT_DEFAULT               100
#define RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT       3

#define CONFIGURATION_SECTION_RTSP                          _T("RTSP")

#define CONFIGURATION_RTSP_RECEIVE_DATA_TIMEOUT             _T("RtspReceiveDataTimeout")
#define CONFIGURATION_RTSP_UDP_SINK_MAX_PAYLOAD_SIZE        _T("RtspUdpSinkMaxPayloadSize")
#define CONFIGURATION_RTSP_UDP_PORT_RANGE_START             _T("RtspUdpPortRangeStart")
#define CONFIGURATION_RTSP_UDP_PORT_RANGE_END               _T("RtspUdpPortRangeEnd")
#define CONFIGURATION_RTSP_TEARDOWN_REQUEST_MAXIMUM_COUNT   _T("RtspTeardownRequestMaximumCount")
#define CONFIGURATION_RTSP_TEARDOWN_REQUEST_TIMEOUT         _T("RtspTeardownRequestTimeout")
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
  TaskScheduler* rtspScheduler;
  UsageEnvironment* rtspEnvironment; 
  RTSPClient* rtspClient;
  MediaSession* rtspSession;
  FramedSource *rtspSource;
  Groupsock *rtspUdpGroupsock;
  MediaSink *rtspUdpSink;
  unsigned int rtspUdpSinkMaxPayloadSize;
  unsigned int rtspUdpPortRangeStart;
  unsigned int rtspUdpPortRangeEnd;

  // variable for signaling exit for rtspScheduler
  char rtspThreadShouldExit;
  // variables for RTSP scheduler thread
  DWORD rtspSchedulerThreadId;
  HANDLE rtspSchedulerThreadHandle;

  // RTSP scheduler worker method
  // @param lpParam : reference to instance of CMPIPTV_RTSP class
  static DWORD WINAPI RtspSchedulerWorker(LPVOID lpParam);

  // RTSP subsession 'Bye' handler
  static void SubsessionByeHandler(void *lpCMPIPTV_RTSP);

  // close all media sinks
  void CloseSinks();

  // tear down media session
  // @param forceTeardown : if true than session and client will be deleted in any case
  // @result : true if succesfull, false otherwise
  bool TeardownMediaSession(bool forceTeardown);

  // result of RtspTearDownSessionWorker()
  bool rtspTearDownSessionWorkerResult;

  // RTSP tear down session worker method
  // @param lpParam : reference to instance of CMPIPTV_RTSP class
  static DWORD WINAPI RtspTearDownSessionWorker(LPVOID lpParam);

  // RTSP tear down request maximum count
  unsigned int rtspTeardownRequestMaximumCount;
  // RTSP tear down request timeout (in ms)
  unsigned int rtspTeardownRequestTimeout;

  // log RTSP message
  void LogRtspMessage(unsigned int loggerLevel, const TCHAR *messagePrefix);

  // log RTSP full message (not from environment)
  void LogFullRtspMessage(unsigned int loggerLevel, const TCHAR *messagePrefix, const char *rtspMessage);

  // get last RTSP message
  // caller have to free result from memory
  // @result : reference to null-terminated string or NULL if error
  char *GetLastRtspMessageA(void);

  // get last RTSP message
  // caller have to free result from memory
  // @result : reference to null-terminated string or NULL if error
  wchar_t *GetLastRtspMessageW(void);

#ifdef _MBCS
#define GetLastRtspMessage GetLastRtspMessageA
#else
#define GetLastRtspMessage GetLastRtspMessageW
#endif

};

#endif
