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

#include "stdafx.h"

#include "MPIPTV_RTSP.h"
#include "Network.h"
#include "Utilities.h"

#include "GroupsockHelper.hh"

#include <time.h>

// protocol implementation name
#define PROTOCOL_IMPLEMENTATION_NAME                                    _T("CMPIPTV_RTSP")

#define METHOD_RTSP_SCHEDULER_WORKER_NAME                               _T("RtspSchedulerWorker()")
#define METHOD_SUBSESSION_BYE_HANDLER_NAME                              _T("SubsessionByeHandler()")
#define METHOD_TEARDOWN_MEDIA_SESSION_NAME                              _T("TeardownMediaSession()")

PIProtocol CreateProtocolInstance(void)
{
  return new CMPIPTV_RTSP;
}

void DestroyProtocolInstance(PIProtocol pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPIPTV_RTSP *pClass = (CMPIPTV_RTSP *)pProtocol;
    delete pClass;
  }
}

CMPIPTV_RTSP::CMPIPTV_RTSP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  this->rtspUrl = NULL;
  this->rtspScheduler = NULL;
  this->rtspEnvironment = NULL;
  this->rtspClient = NULL;
  this->rtspResponseEvent = NULL;
  this->rtspSession = NULL;
  this->isRtspSessionSetup = false;
  this->rtspSchedulerThreadHandle = NULL;
  this->rtspSchedulerThreadId = 0;
  this->rtspThreadShouldExit = 0;
  this->rtspRtpClientPortRangeStart = RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT;
  this->rtspRtpClientPortRangeEnd = RTSP_RTP_CLIENT_PORT_RANGE_END_DEFAULT;
  this->rtspUdpSink = NULL;
  this->rtspUdpGroupsock = NULL;
  this->rtspUdpSinkMaxPayloadSize = RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT;
  this->rtspUdpPortRangeStart = RTSP_UDP_PORT_RANGE_START_DEFAULT;
  this->rtspUdpPortRangeEnd = RTSP_UDP_PORT_RANGE_END_DEFAULT;
  this->rtspCommandResponseTimeout = RTSP_COMMAND_RESPONSE_TIMEOUT_DEFAULT;
  this->openConnetionMaximumAttempts = RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTV_RTSP::~CMPIPTV_RTSP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  if (this->rtspEnvironment != NULL)
  {
    // release RTSP environment
    this->rtspEnvironment->reclaim();
    this->rtspEnvironment = NULL;
  }

  if (this->rtspScheduler != NULL)
  {
    delete this->rtspScheduler;
    this->rtspScheduler = NULL;
  }

  if (this->rtspResponseEvent != NULL)
  {
    CloseHandle(this->rtspResponseEvent);
    this->rtspResponseEvent = NULL;
  }

  FREE_MEM(this->rtspUrl);

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
}

int CMPIPTV_RTSP::Initialize(HANDLE lockMutex, CParameterCollection *configuration)
{
  if (configuration != NULL)
  {
    CParameterCollection *vlcParameters = GetConfiguration(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME, CONFIGURATION_SECTION_UDP);
    configuration->Append(vlcParameters);
    delete vlcParameters;
  }

  int result = this->CMPIPTV_UDP::Initialize(lockMutex, configuration);

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_RECEIVE_DATA_TIMEOUT, true, RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT);
  this->rtspRtpClientPortRangeStart = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_START, true, RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT);
  this->rtspRtpClientPortRangeEnd = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_END, true, RTSP_RTP_CLIENT_PORT_RANGE_END_DEFAULT);
  this->rtspUdpSinkMaxPayloadSize = this->configurationParameters->GetValueUnsignedInt(CONFIGURATION_RTSP_UDP_SINK_MAX_PAYLOAD_SIZE, true, RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT);
  this->rtspUdpPortRangeStart = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_UDP_PORT_RANGE_START, true, RTSP_UDP_PORT_RANGE_START_DEFAULT);
  this->rtspUdpPortRangeEnd = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_UDP_PORT_RANGE_END, true, RTSP_UDP_PORT_RANGE_END_DEFAULT);
  this->rtspCommandResponseTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_COMMAND_RESPONSE_TIMEOUT, true, RTSP_COMMAND_RESPONSE_TIMEOUT_DEFAULT);
  this->openConnetionMaximumAttempts = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS, true, RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
  this->rtspRtpClientPortRangeStart = (this->rtspRtpClientPortRangeStart < 0) ? RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT : this->rtspRtpClientPortRangeStart & ~1;
  this->rtspRtpClientPortRangeEnd = (this->rtspRtpClientPortRangeEnd < this->rtspRtpClientPortRangeStart) ? min(65535, this->rtspRtpClientPortRangeStart + 1000) : min(65535, this->rtspRtpClientPortRangeEnd);
  this->rtspUdpSinkMaxPayloadSize = (this->rtspUdpSinkMaxPayloadSize < 0) ? RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT : this->rtspUdpSinkMaxPayloadSize;
  this->rtspUdpPortRangeStart = (this->rtspUdpPortRangeStart <= 1024) ? RTSP_UDP_PORT_RANGE_START_DEFAULT : this->rtspUdpPortRangeStart;
  this->rtspUdpPortRangeEnd = (this->rtspUdpPortRangeEnd < this->rtspUdpPortRangeStart) ? min(65535, this->rtspUdpPortRangeStart + 1000) : min(65535, this->rtspUdpPortRangeEnd);
  this->rtspCommandResponseTimeout = (this->rtspCommandResponseTimeout < 0) ? RTSP_COMMAND_RESPONSE_TIMEOUT_DEFAULT : this->rtspCommandResponseTimeout;
  this->openConnetionMaximumAttempts = (this->openConnetionMaximumAttempts < 0) ? RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT : this->openConnetionMaximumAttempts;

  this->rtspScheduler = RtspTaskScheduler::createNew();
  if (this->rtspScheduler != NULL)
  {
    this->rtspEnvironment = BasicUsageEnvironment::createNew(*this->rtspScheduler);
  }

  result |= (this->rtspScheduler == NULL);
  result |= (this->rtspEnvironment == NULL);

  this->rtspResponseEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
  result |= (this->rtspResponseEvent == NULL);

  return (result == STATUS_OK) ? STATUS_OK : STATUS_ERROR;
}

TCHAR *CMPIPTV_RTSP::GetProtocolName(void)
{
  return Duplicate(CONFIGURATION_SECTION_RTSP);
}

int CMPIPTV_RTSP::ClearSession(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  FREE_MEM(this->rtspUrl);

  this->CMPIPTV_UDP::ClearSession();

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  return STATUS_OK;
}

int CMPIPTV_RTSP::ParseUrl(const TCHAR *url, const CParameterCollection *parameters)
{
  int result = STATUS_OK;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  this->ClearSession();
  this->loadParameters->Append((CParameterCollection *)parameters);
  this->loadParameters->LogCollection(&this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
  if (urlComponents == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'url components'"));
    result = STATUS_ERROR;
  }

  if (result == STATUS_OK)
  {
    ZeroURL(urlComponents);
    urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

    this->logger.Log(LOGGER_INFO, _T("%s: %s: url: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, url);

    if (!InternetCrackUrl(url, 0, 0, urlComponents))
    {
      this->logger.Log(LOGGER_ERROR, _T("%s: %s: InternetCrackUrl() error: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, GetLastError());
      result = STATUS_ERROR;
    }
  }

  if (result == STATUS_OK)
  {
    int length = urlComponents->dwSchemeLength + 1;
    ALLOC_MEM_DEFINE_SET(protocol, TCHAR, length, 0);
    if (protocol == NULL) 
    {
      this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'protocol'"));
      result = STATUS_ERROR;
    }

    if (result == STATUS_OK)
    {
      _tcsncat_s(protocol, length, urlComponents->lpszScheme, urlComponents->dwSchemeLength);

      if (_tcsncicmp(urlComponents->lpszScheme, _T("RTSP"), urlComponents->dwSchemeLength) != 0)
      {
        // not supported protocol
        this->logger.Log(LOGGER_INFO, _T("%s: %s: unsupported protocol '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
        result = STATUS_ERROR;
      }
    }
    FREE_MEM(protocol);

    if (result == STATUS_OK)
    {
      length = _tcslen(url) + 1;
      this->rtspUrl = ALLOC_MEM_SET(this->rtspUrl, TCHAR, length, 0);
      if (this->rtspUrl == NULL)
      {
        this->logger.Log(LOG_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for RTSP url"));
        result = STATUS_ERROR;
      }
    }

    if (result == STATUS_OK)
    {
      _tcscpy_s(this->rtspUrl, length, url);

      this->logger.Log(LOGGER_INFO, _T("%s: %s: RTSP url: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->rtspUrl);
    }
  }
  FREE_MEM(urlComponents);
  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  return result;
}

int CMPIPTV_RTSP::OpenConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  this->isRtspSessionSetup = false;

  // LIVE555 works with char, not with TCHAR
  char *tempRtspUrl = ConvertToMultiByte(this->rtspUrl);
  if (tempRtspUrl == NULL)
  {
    return STATUS_ERROR;
  }

  // start LIVE555 worker thread
  this->rtspSchedulerThreadHandle = CreateThread( 
    NULL,                                   // default security attributes
    0,                                      // use default stack size  
    &CMPIPTV_RTSP::RtspSchedulerWorker,     // thread function name
    this,                                   // argument to thread function 
    0,                                      // use default creation flags 
    &this->rtspSchedulerThreadId);          // returns the thread identifier
  if (this->rtspSchedulerThreadHandle == NULL)
  {
    this->logger.Log(LOGGER_ERROR, _T("%s: %s: failed to create RTSP scheduler thread, error = %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, GetLastError());
    return STATUS_ERROR;
  }

  this->rtspClient = MPRTSPClient::createNew(this, *this->rtspEnvironment, tempRtspUrl);
  FREE_MEM(tempRtspUrl);
  if (
    this->rtspClient == NULL ||
    SendRtspCommand(METHOD_OPEN_CONNECTION_NAME, _T("OPTIONS")) != STATUS_OK ||
    SendRtspCommand(METHOD_OPEN_CONNECTION_NAME, _T("DESCRIBE")) != STATUS_OK
  )
  {
    CloseConnection();
    return STATUS_ERROR;
  }

  this->rtspSession = MediaSession::createNew(*this->rtspEnvironment, this->rtspResponseResultString);
  if (this->rtspSession == NULL || !this->rtspSession->hasSubsessions())
  {
    this->LogRtspMessage(LOGGER_ERROR, METHOD_OPEN_CONNECTION_NAME, this->rtspSession == NULL ? _T("failed to create session") : _T("session doesn't have sub-sessions"));
    CloseConnection();
    return STATUS_ERROR;
  }

  // Setup the RTP source for the session. Only one sub-session expected/supported.
  MediaSubsessionIterator iter(*this->rtspSession);
  MediaSubsession *subsession = NULL;
  FramedSource *rtspSource = NULL;
  while ((subsession = iter.next()) != NULL)
  {
#ifdef _MBCS
    TCHAR *subSessionName = ConvertToMultiByteA(subsession->mediumName());
    TCHAR *subSessionCodecName = ConvertToMultiByteA(subsession->codecName());
#else
    TCHAR *subSessionName = ConvertToUnicodeA(subsession->mediumName());
    TCHAR *subSessionCodecName = ConvertToUnicodeA(subsession->codecName());
#endif

    if (_tcsncicmp(subSessionName, _T("video"), 5) != 0 || _tcsncicmp(subSessionCodecName, _T("MP2T"), 4) != 0)
    {
      TCHAR *message = FormatString(_T("sub-session medium or codec not supported, medium = %s, codec = %s"), subSessionName, subSessionCodecName);
      this->LogRtspMessage(LOGGER_ERROR, METHOD_OPEN_CONNECTION_NAME, message);
      FREE_MEM(message);
      FREE_MEM(subSessionName);
      FREE_MEM(subSessionCodecName);
      continue;
    }

    // If a client port is configured, find a free pair of ports in the range.
    // The first port is used for RTP; the second port is used for RTCP. Once
    // we find one free port, we assume the next one is also free.
    if (this->rtspRtpClientPortRangeStart > 0)
    {
      struct in_addr destinationAddress;
      destinationAddress.s_addr = our_inet_addr("127.0.0.1");
      unsigned int port = this->rtspRtpClientPortRangeStart;
      Groupsock *groupsock = NULL;
      do
      {
        this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: RTP client port %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, port);

        // special construction force not reuse same UDP port
        {
          NoReuse noReuse(*this->rtspEnvironment);
          groupsock = new Groupsock(*this->rtspEnvironment, destinationAddress, port, 1);
        }

        if (groupsock == NULL || groupsock->socketNum() == -1)
        {
          this->logger.Log(LOGGER_WARNING, _T("%s: %s: RTP client port %u occupied, trying next even port"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, port);
          port += 2;
          if (groupsock != NULL)
          {
            delete groupsock;
            groupsock = NULL;
          }
        }
      }
      while ((groupsock == NULL) && (port <= this->rtspRtpClientPortRangeEnd));
      // Did we find a free port? If not, we fall back to a random port chosen
      // by LIVE555.
      if (groupsock != NULL)
      {
        delete groupsock;
        groupsock = NULL;
        subsession->setClientPortNum(port);
      }
    }

    if (!subsession->initiate() || subsession->rtpSource() == NULL)
    {
      TCHAR *message = FormatString(_T("failed to create receiver for sub-session, medium = %s, codec = %s"), subSessionName, subSessionCodecName);
      this->LogRtspMessage(LOGGER_ERROR, METHOD_OPEN_CONNECTION_NAME, message);
      FREE_MEM(message);
      FREE_MEM(subSessionName);
      FREE_MEM(subSessionCodecName);
      continue;
    }

    this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: created receiver for sub-session, medium = %s, codec = %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName, subSessionCodecName);
    FREE_MEM(subSessionName);
    FREE_MEM(subSessionCodecName);

    // set session ID, doesn't matter what
    subsession->setSessionId(subsession->mediumName());

    // because we're saving the incoming data, rather than playing
    // it in real time, allow an especially large time threshold
    // for reordering misordered incoming packets:
    subsession->rtpSource()->setPacketReorderingThresholdTime(1000000); // 1 second

    // set the RTP source's OS socket buffer size as appropriate
    int socketNum = subsession->rtpSource()->RTPgs()->socketNum();
    unsigned int currentBufferSize = getReceiveBufferSize(*this->rtspEnvironment, socketNum);
    if (this->defaultBufferSize > currentBufferSize)
    {
      setReceiveBufferTo(*this->rtspEnvironment, socketNum, this->defaultBufferSize);
      unsigned setBufferSize = getReceiveBufferSize(*this->rtspEnvironment, socketNum);
      if (setBufferSize == this->defaultBufferSize)
      {
        this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: set buffer size for sub-session, previous size = %i, requested size = %i, current size = %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName, currentBufferSize, this->defaultBufferSize, setBufferSize);
      }
      else
      {
        this->logger.Log(LOGGER_WARNING, _T("%s: %s: failed to set buffer size for sub-session, previous size = %i, requested size = %i, current size = %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName, currentBufferSize, this->defaultBufferSize, setBufferSize);
      }
    }

    if (SendRtspCommand(METHOD_OPEN_CONNECTION_NAME, _T("SETUP"), subsession) != STATUS_OK)
    {
      CloseConnection();
      return STATUS_ERROR;
    }
    rtspSource = subsession->rtpSource();
    break;
  }

  // If we don't have an RTSP source then we can't continue.
  if (rtspSource == NULL)
  {
    CloseConnection();
    return STATUS_ERROR;
  }

  this->isRtspSessionSetup = true;
  if (SendRtspCommand(METHOD_OPEN_CONNECTION_NAME, _T("PLAY")) != STATUS_OK)
  {
    CloseConnection();
    return STATUS_ERROR;
  }

  // create UDP socket and start playing
  struct in_addr destinationAddress;
  destinationAddress.s_addr = our_inet_addr("127.0.0.1");

  unsigned int port = this->rtspUdpPortRangeStart;
  do
  {
    this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: UDP port %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, port);

    // special construction force not reuse same UDP port
    {
      NoReuse noReuse(*this->rtspEnvironment);
      this->rtspUdpGroupsock = new Groupsock(*this->rtspEnvironment, destinationAddress, port, 1);
    }

    if (this->rtspUdpGroupsock == NULL || this->rtspUdpGroupsock->socketNum() == -1)
    {
      this->logger.Log(LOGGER_WARNING, _T("%s: %s: UDP port %u occupied, trying another port"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, port);
      port++;
      if (this->rtspUdpGroupsock != NULL)
      {
        delete this->rtspUdpGroupsock;
        this->rtspUdpGroupsock = NULL;
      }
    }
  }
  while ((this->rtspUdpGroupsock == NULL) && (port <= this->rtspUdpPortRangeEnd));

  if (this->rtspUdpGroupsock == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("failed to create UDP socket, no free port"));
    CloseConnection();
    return STATUS_ERROR;
  }

  this->rtspUdpSink = BasicUDPSink::createNew(*this->rtspEnvironment, this->rtspUdpGroupsock, this->rtspUdpSinkMaxPayloadSize);
  if (this->rtspUdpSink == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("failed to create UDP sink"));
    CloseConnection();
    return STATUS_ERROR;
  }

  if (!this->rtspUdpSink->startPlaying(*rtspSource, NULL, NULL))
  {
    this->LogRtspMessage(LOGGER_ERROR, METHOD_OPEN_CONNECTION_NAME, _T("failed to start UDP sink"));
    CloseConnection();
    return STATUS_ERROR;
  }

  this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("streaming started"));

  // create a UDP connection to the local stream
  TCHAR *url = FormatString(_T("udp://@127.0.0.1:%u"), port);
  if (
    url == NULL ||
    this->CMPIPTV_UDP::ParseUrl(url, NULL) != STATUS_OK ||
    this->CMPIPTV_UDP::OpenConnection() != STATUS_OK
  )
  {
    FREE_MEM(url);
    this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
    CloseConnection();
    return STATUS_ERROR;
  }

  FREE_MEM(url);
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  return STATUS_OK;
}

int CMPIPTV_RTSP::IsConnected(void)
{
  if (this->rtspUdpGroupsock != NULL)
  {
    // receiving data, return status of UDP connection
    return this->CMPIPTV_UDP::IsConnected();
  }

  if (!this->CMPIPTV_UDP::IsConnected())
  {
    return FALSE;
  }

  // we have UDP connection but RTSP connection is closed
  // if buffer is not occupied, close UDP connection
  unsigned int occupiedSpace = 0;
  this->GetSafeBufferSizes(this->lockMutex, NULL, &occupiedSpace, NULL);

  if (occupiedSpace == 0)
  {
    this->CloseConnection();
    return FALSE;
  }

  // we are still connected when RTSP connection is closed, but UDP connection is opened and we have data in buffer
  return TRUE;
}

void CMPIPTV_RTSP::CloseConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);

  // close base UDP connection
  this->CMPIPTV_UDP::CloseConnection();

  // tear down media session
  // rtspClient and rtspSession are freed while tear down, also rtspUdpSink and rtspUdpGroupsock are freed
  this->TeardownMediaSession(TRUE);

  // stop RTSP scheduler
  this->rtspThreadShouldExit = 1;

  // wait for the RTSP worker thread to exit      
  if (this->rtspSchedulerThreadHandle != NULL)
  {
    this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("wait for RTSP worker thread to exit"));
    if (WaitForSingleObject(this->rtspSchedulerThreadHandle, 1000) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("RTSP worker thread didn't exit, terminating thread"));
      TerminateThread(this->rtspSchedulerThreadHandle, 0);
    }
  }

  this->rtspSchedulerThreadHandle = NULL;
  this->rtspThreadShouldExit = 0;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);
}

void CMPIPTV_RTSP::GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize)
{
  WaitForSingleObject(lockMutex, INFINITE);
  if (freeSpace != NULL)
  {
    *freeSpace = this->buffer.GetBufferFreeSpace();
  }
  if (occupiedSpace != NULL)
  {
    *occupiedSpace = this->buffer.GetBufferOccupiedSpace();
  }
  if (bufferSize != NULL)
  {
    *bufferSize = this->buffer.GetBufferSize();
  }
  ReleaseMutex(lockMutex);
}

void CMPIPTV_RTSP::ReceiveData(bool *shouldExit)
{
  this->CMPIPTV_UDP::ReceiveData(shouldExit);
}

unsigned int CMPIPTV_RTSP::FillBuffer(IMediaSample *pSamp, char *pData, long cbData)
{
  return this->CMPIPTV_UDP::FillBuffer(pSamp, pData, cbData);
}

unsigned int CMPIPTV_RTSP::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

GUID CMPIPTV_RTSP::GetInstanceId(void)
{
  return this->logger.loggerInstance;
}

unsigned int CMPIPTV_RTSP::GetOpenConnectionMaximumAttempts(void)
{
  return this->openConnetionMaximumAttempts;
}

int CMPIPTV_RTSP::SendRtspCommand(const TCHAR *method, const TCHAR *command, MediaSubsession *subsession)
{
  this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: send %s command"), PROTOCOL_IMPLEMENTATION_NAME, method, command);
  ResetEvent(this->rtspResponseEvent);
  if (_tcscmp(command, _T("OPTIONS")) == 0)
  {
    this->rtspClient->sendOptionsCommand(&CMPIPTV_RTSP::OnRtspResponseReceived);
  }
  else if (_tcscmp(command, _T("DESCRIBE")) == 0)
  {
    this->rtspClient->sendDescribeCommand(&CMPIPTV_RTSP::OnRtspResponseReceived);
  }
  else if (_tcscmp(command, _T("SETUP")) == 0)
  {
    this->rtspClient->sendSetupCommand(*subsession, &CMPIPTV_RTSP::OnRtspResponseReceived);
  }
  else if (_tcscmp(command, _T("PLAY")) == 0)
  {
    this->rtspClient->sendPlayCommand(*this->rtspSession, &CMPIPTV_RTSP::OnRtspResponseReceived);
  }
  else if (_tcscmp(command, _T("TEARDOWN")) == 0)
  {
    this->rtspClient->sendTeardownCommand(*this->rtspSession, &CMPIPTV_RTSP::OnRtspResponseReceived);
  }
  else
  {
    return STATUS_ERROR;
  }

  if (WaitForSingleObject(this->rtspResponseEvent, this->rtspCommandResponseTimeout) == WAIT_TIMEOUT)
  {
    this->logger.Log(LOGGER_ERROR, _T("%s: %s: %s command timed out"), PROTOCOL_IMPLEMENTATION_NAME, method, command);
    return STATUS_ERROR;
  }
  if (this->rtspResponseResultCode != 0)
  {
#ifdef _MBCS
    TCHAR *convertedRtspResponse = ConvertToMultiByteA(&this->rtspResponseResultString[0]);
#else
    TCHAR *convertedRtspResponse = ConvertToUnicodeA(&this->rtspResponseResultString[0]);
#endif
    this->logger.Log(LOGGER_ERROR, _T("%s: %s: %s command failed, code = %i, response = %s"), PROTOCOL_IMPLEMENTATION_NAME, method, command, this->rtspResponseResultCode, (convertedRtspResponse == NULL) ? _T("unable to get message") : convertedRtspResponse);
    FREE_MEM(convertedRtspResponse);
    return STATUS_ERROR;
  }

#ifdef _MBCS
  TCHAR *convertedRtspResponse = ConvertToMultiByteA(&this->rtspResponseResultString[0]);
#else
  TCHAR *convertedRtspResponse = ConvertToUnicodeA(&this->rtspResponseResultString[0]);
#endif
  this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: %s command succeeded, response = %s"), PROTOCOL_IMPLEMENTATION_NAME, method, command, (convertedRtspResponse == NULL) ? _T("unable to get message") : convertedRtspResponse);
  FREE_MEM(convertedRtspResponse);
  return STATUS_OK;
}

void CMPIPTV_RTSP::OnRtspResponseReceived(RTSPClient *client, int resultCode, char *resultString)
{
  CMPIPTV_RTSP* rtspPluginInstance = (CMPIPTV_RTSP*)((MPRTSPClient*)client)->Context();
  if (rtspPluginInstance == NULL)
  {
    // This can happen if the client attempted to send a command, the command
    // was assumed to have timed out after some time, and the client was
    // subsequently destroyed as a result. This is the response to the command.
    // We can't do anything more than clean up.
    if (resultString != NULL)
    {
      delete[] resultString;
    }
    return;
  }

  rtspPluginInstance->rtspResponseResultCode = resultCode;
  if (resultString == NULL)
  {
    rtspPluginInstance->rtspResponseResultString[0] = NULL;
  }
  else
  {
    strncpy(rtspPluginInstance->rtspResponseResultString, resultString, RTSP_MAX_RESPONSE_BYTE_COUNT);
    rtspPluginInstance->rtspResponseResultString[RTSP_MAX_RESPONSE_BYTE_COUNT - 1] = NULL;
    delete[] resultString;
  }
  SetEvent(rtspPluginInstance->rtspResponseEvent);
}

void CMPIPTV_RTSP::LogRtspMessage(unsigned int loggerLevel, const TCHAR *method, const TCHAR *message)
{
  const char *lastRtspMessage = this->rtspEnvironment->getResultMsg();

#ifdef _MBCS
  TCHAR *convertedRtspMessage = ConvertToMultiByteA(lastRtspMessage);
#else
  TCHAR *convertedRtspMessage = ConvertToUnicodeA(lastRtspMessage);
#endif

  this->logger.Log(loggerLevel, _T("%s: %s: %s, %s"), PROTOCOL_IMPLEMENTATION_NAME, method, message, (convertedRtspMessage == NULL) ? _T("unable to get message") : convertedRtspMessage);

  FREE_MEM(convertedRtspMessage);
}

DWORD WINAPI CMPIPTV_RTSP::RtspSchedulerWorker(LPVOID lpParam)
{
  CMPIPTV_RTSP *caller = (CMPIPTV_RTSP*)lpParam;
  caller->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RTSP_SCHEDULER_WORKER_NAME);

  if (caller->rtspScheduler != NULL)
  {
    caller->rtspScheduler->doEventLoop(&caller->rtspThreadShouldExit);
  }

  caller->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RTSP_SCHEDULER_WORKER_NAME);
  return S_OK;
}

void CMPIPTV_RTSP::SubsessionByeHandler(void *lpCMPIPTV_RTSP)
{
  CMPIPTV_RTSP *instance = (CMPIPTV_RTSP *)lpCMPIPTV_RTSP;

  instance->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SUBSESSION_BYE_HANDLER_NAME);

  instance->TeardownMediaSession(FALSE);

  instance->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SUBSESSION_BYE_HANDLER_NAME);
}

bool CMPIPTV_RTSP::TeardownMediaSession(bool forceTeardown)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME);
  bool result = true;

  if (this->rtspClient != NULL && this->rtspSession != NULL && this->isRtspSessionSetup)
  {
    result = SendRtspCommand(METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("TEARDOWN")) == STATUS_OK;
    this->isRtspSessionSetup = !result;
  }

  if (forceTeardown || result)
  {
    // close all media sinks after teardown and before closing RTSP session and client
    if (this->rtspUdpSink != NULL)
    {
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("closing UDP sink"));
      this->rtspUdpSink->stopPlaying();
      Medium::close(this->rtspUdpSink);
      this->rtspUdpSink = NULL;
    }

    if (this->rtspUdpGroupsock != NULL)
    {
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("closing UDP socket"));
      delete this->rtspUdpGroupsock;
      this->rtspUdpGroupsock = NULL;
    }

    if (this->rtspSession != NULL)
    {
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("closing RTSP session"));
      Medium::close(this->rtspSession);
      this->rtspSession = NULL;
    }

    if (this->rtspClient != NULL)
    {
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("closing RTSP client"));
      Medium::close(this->rtspClient);
      this->rtspClient = NULL;
    }
  }

  this->logger.Log(LOGGER_INFO, (result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME);
  return result;
}
