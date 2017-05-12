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

// protocol implementation name
#define PROTOCOL_IMPLEMENTATION_NAME                                    _T("CMPIPTV_RTSP")

#define METHOD_LIVE555_WORKER_NAME                                      _T("Live555Worker()")
#define METHOD_START_OPEN_CONNECTION_NAME                               _T("StartOpenConnection()")
#define METHOD_SETUP_RTSP_SESSION_NAME                                  _T("SetupRtspSession()")
#define METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME                          _T("SetupLocalUdpConnection()")
#define METHOD_CLEAN_UP_LIVE555_NAME                                    _T("CleanUpLive555()")
#define METHOD_RTSP_SESSION_BYE_HANDLER_NAME                            _T("RtspSessionByeHandler()")

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
  this->rtspClient = NULL;
  this->openConnectionResultEvent = NULL;
  this->rtspSession = NULL;
  this->isRtspSessionSetup = false;
  this->rtspSessionTimeout = 0;
  this->openConnectionTimeout = RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT;
  this->openConnetionMaximumAttempts = RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT;
  this->openConnectionResultEvent = NULL;
  this->sendRtspCommandOptions = RTSP_SEND_COMMAND_OPTIONS_DEFAULT;
  this->sendRtspCommandDescribe = RTSP_SEND_COMMAND_DESCRIBE_DEFAULT;
  this->keepAliveWithOptions = RTSP_KEEP_ALIVE_WITH_OPTIONS_DEFAULT;

  this->live555Scheduler = NULL;
  this->live555Environment = NULL;
  this->live555WorkerThreadHandle = NULL;
  this->live555WorkerThreadId = 0;
  this->live555WorkerThreadShouldExit = 0;

  this->rtpSource = NULL;
  this->rtpClientPortRangeStart = RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT;
  this->rtpClientPortRangeEnd = RTSP_RTP_CLIENT_PORT_RANGE_END_DEFAULT;

  this->udpUrl = NULL;
  this->udpSink = NULL;
  this->udpGroupsock = NULL;
  this->udpSinkMaxPayloadSize = RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT;
  this->udpPortRangeStart = RTSP_UDP_PORT_RANGE_START_DEFAULT;
  this->udpPortRangeEnd = RTSP_UDP_PORT_RANGE_END_DEFAULT;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTV_RTSP::~CMPIPTV_RTSP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  if (this->openConnectionResultEvent != NULL)
  {
    CloseHandle(this->openConnectionResultEvent);
    this->openConnectionResultEvent = NULL;
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
  this->rtpClientPortRangeStart = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_START, true, RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT);
  this->rtpClientPortRangeEnd = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_RTP_CLIENT_PORT_RANGE_END, true, RTSP_RTP_CLIENT_PORT_RANGE_END_DEFAULT);
  this->udpSinkMaxPayloadSize = this->configurationParameters->GetValueUnsignedInt(CONFIGURATION_RTSP_UDP_SINK_MAX_PAYLOAD_SIZE, true, RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT);
  this->udpPortRangeStart = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_UDP_PORT_RANGE_START, true, RTSP_UDP_PORT_RANGE_START_DEFAULT);
  this->udpPortRangeEnd = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_UDP_PORT_RANGE_END, true, RTSP_UDP_PORT_RANGE_END_DEFAULT);
  this->openConnectionTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_OPEN_CONNECTION_TIMEOUT, true, RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT);
  this->openConnetionMaximumAttempts = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS, true, RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT);
  this->sendRtspCommandOptions = this->configurationParameters->GetValueBool(CONFIGURATION_RTSP_SEND_COMMAND_OPTIONS, true, RTSP_SEND_COMMAND_OPTIONS_DEFAULT);
  this->sendRtspCommandDescribe = this->configurationParameters->GetValueBool(CONFIGURATION_RTSP_SEND_COMMAND_DESCRIBE, true, RTSP_SEND_COMMAND_DESCRIBE_DEFAULT);
  this->keepAliveWithOptions = this->configurationParameters->GetValueBool(CONFIGURATION_RTSP_KEEP_ALIVE_WITH_OPTIONS, true, RTSP_KEEP_ALIVE_WITH_OPTIONS_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
  this->rtpClientPortRangeStart = (this->rtpClientPortRangeStart < 0) ? RTSP_RTP_CLIENT_PORT_RANGE_START_DEFAULT : this->rtpClientPortRangeStart & ~1;
  this->rtpClientPortRangeEnd = (this->rtpClientPortRangeEnd < this->rtpClientPortRangeStart) ? min(65535, this->rtpClientPortRangeStart + 1000) : min(65535, this->rtpClientPortRangeEnd);
  this->udpSinkMaxPayloadSize = (this->udpSinkMaxPayloadSize < 0) ? RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT : this->udpSinkMaxPayloadSize;
  this->udpPortRangeStart = (this->udpPortRangeStart <= 1024) ? RTSP_UDP_PORT_RANGE_START_DEFAULT : this->udpPortRangeStart;
  this->udpPortRangeEnd = (this->udpPortRangeEnd < this->udpPortRangeStart) ? min(65535, this->udpPortRangeStart + 1000) : min(65535, this->udpPortRangeEnd);
  this->openConnectionTimeout = (this->openConnectionTimeout < 0) ? RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT : this->openConnectionTimeout;
  this->openConnetionMaximumAttempts = (this->openConnetionMaximumAttempts < 0) ? RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT : this->openConnetionMaximumAttempts;

  this->openConnectionResultEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
  result |= (this->openConnectionResultEvent == NULL);

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

  // All ParseUrl() implementations call ClearSession(). We invoke the UDP
  // implementation of ParseUrl() at the end of OpenConnection(). If we're not
  // careful that would cause us to free the RTSP URL, which in turn would
  // prevent reconnection attempts if our UDP connection attempt failed for any
  // reason.
  if (this->udpUrl == NULL)
  {
    FREE_MEM(this->rtspUrl);
  }

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
  int result = STATUS_OK;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);

  ResetEvent(this->openConnectionResultEvent);

  // All LIVE555 interaction has to be done from one thread.
  this->live555WorkerThreadShouldExit = 0;
  this->live555WorkerThreadHandle = CreateThread( 
    NULL,                                   // default security attributes
    0,                                      // use default stack size  
    &CMPIPTV_RTSP::Live555Worker,           // thread function name
    this,                                   // argument to thread function 
    0,                                      // use default creation flags 
    &this->live555WorkerThreadId);          // returns the thread identifier
  if (this->live555WorkerThreadHandle == NULL)
  {
    this->logger.Log(LOGGER_ERROR, _T("%s: %s: failed to create LIVE555 worker thread, error = %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, GetLastError());
    result = STATUS_ERROR;
  }

  if (result == STATUS_OK && WaitForSingleObject(this->openConnectionResultEvent, this->openConnectionTimeout) == WAIT_TIMEOUT)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("timed out"));
    this->CloseConnection();
    result = STATUS_ERROR;
  }
  else if (this->udpUrl == NULL)
  {
    result = STATUS_ERROR;
  }
  else
  {
    // The RTSP connection is open. Now create a UDP connection to receive the
    // data/RTP stream.
    result = this->CMPIPTV_UDP::ParseUrl(this->udpUrl, NULL);
    FREE_MEM(this->udpUrl);
    if (result == STATUS_OK)
    {
      result = this->CMPIPTV_UDP::OpenConnection();
    }
    if (result != STATUS_OK)
    {
      this->CloseConnection();
    }
  }

  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  return result;
}

int CMPIPTV_RTSP::IsConnected(void)
{
  if (this->udpSink != NULL)
  {
    return this->CMPIPTV_UDP::IsConnected();
  }
  return FALSE;
}

void CMPIPTV_RTSP::CloseConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);

  // close base UDP connection
  this->CMPIPTV_UDP::CloseConnection();

  // close the RTSP connection and stop the LIVE555 thread
  if (this->live555WorkerThreadHandle != NULL)
  {
    this->live555WorkerThreadShouldExit = 1;
    this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("wait for LIVE555 worker thread to exit"));
    if (WaitForSingleObject(this->live555WorkerThreadHandle, 3000) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("RTSP worker thread didn't exit, terminating thread"));
      TerminateThread(this->live555WorkerThreadHandle, 0);
    }
    this->live555WorkerThreadHandle = NULL;
  }
  this->live555WorkerThreadShouldExit = 0;

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

void CMPIPTV_RTSP::LogLive555Message(unsigned int loggerLevel, const TCHAR *method, const TCHAR *message)
{
  const char *lastLive555Message = this->live555Environment->getResultMsg();

#ifdef _MBCS
  TCHAR *convertedLive555Message = ConvertToMultiByteA(lastLive555Message);
#else
  TCHAR *convertedLive555Message = ConvertToUnicodeA(lastLive555Message);
#endif

  this->logger.Log(loggerLevel, _T("%s: %s: %s, %s"), PROTOCOL_IMPLEMENTATION_NAME, method, message, (convertedLive555Message == NULL) ? _T("unable to get LIVE555 message") : convertedLive555Message);

  FREE_MEM(convertedLive555Message);
}

DWORD WINAPI CMPIPTV_RTSP::Live555Worker(LPVOID lpParam)
{
  CMPIPTV_RTSP *protocolInstance = (CMPIPTV_RTSP*)lpParam;
  protocolInstance->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_LIVE555_WORKER_NAME);

  if (protocolInstance->StartOpenConnection() != STATUS_OK)
  {
    protocolInstance->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME);
    protocolInstance->CleanUpLive555();
    SetEvent(protocolInstance->openConnectionResultEvent);
    protocolInstance->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_LIVE555_WORKER_NAME);
    return S_OK;
  }

  bool openConnectionError = false;
  clock_t previousKeepAlive = 0;
  clock_t startWaitingForTeardownResponse = 0;
  while (true)
  {
    if (protocolInstance->live555WorkerThreadShouldExit == 1)
    {
      if (protocolInstance->udpSink == NULL)
      {
        openConnectionError = true;
      }
      if (!protocolInstance->isRtspSessionSetup)
      {
        // No need to send TEARDOWN. Stop immediately.
        break;
      }
      protocolInstance->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_LIVE555_WORKER_NAME, _T("send TEARDOWN command"));
      protocolInstance->isRtspSessionSetup = false;
      protocolInstance->live555WorkerThreadShouldExit = 0;
      startWaitingForTeardownResponse = clock();
      protocolInstance->rtspClient->sendTeardownCommand(*protocolInstance->rtspSession, &CMPIPTV_RTSP::OnTeardownResponseReceived);
    }
    else if (startWaitingForTeardownResponse != 0 && ElapsedMillis(startWaitingForTeardownResponse) > 500)
    {
      protocolInstance->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_LIVE555_WORKER_NAME, _T("TEARDOWN command response timed out"));
      break;
    }

    if (
      protocolInstance->keepAliveWithOptions &&
      protocolInstance->udpSink != NULL &&
      protocolInstance->rtspSessionTimeout > 0 &&
      startWaitingForTeardownResponse == 0 &&
      (
        previousKeepAlive == 0 ||
        (ElapsedMillis(previousKeepAlive) / 1000) > (protocolInstance->rtspSessionTimeout * 3 / 4)
      )
    )
    {
      previousKeepAlive = clock();
      protocolInstance->rtspClient->sendOptionsCommand(NULL);
    }

    protocolInstance->live555Environment->taskScheduler().doEventLoop();
  }

  protocolInstance->CleanUpLive555();
  if (openConnectionError)
  {
    SetEvent(protocolInstance->openConnectionResultEvent);
  }
  protocolInstance->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_LIVE555_WORKER_NAME);
  return S_OK;
}

int CMPIPTV_RTSP::StartOpenConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME);
  
  this->live555Scheduler = MPTaskScheduler::createNew();
  if (this->live555Scheduler == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME, _T("failed to create LIVE555 task scheduler"));
    return STATUS_ERROR;
  }

  this->live555Environment = BasicUsageEnvironment::createNew(*this->live555Scheduler);
  if (this->live555Environment == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME, _T("failed to create LIVE555 usage environment"));
    return STATUS_ERROR;
  }

  char *tempRtspUrl = ConvertToMultiByte(this->rtspUrl);
  if (tempRtspUrl == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME, _T("failed to convert URL"));
    return STATUS_ERROR;
  }

  this->rtspClient = MPRTSPClient::createNew(this, *this->live555Environment, tempRtspUrl);
  FREE_MEM(tempRtspUrl);
  if (this->rtspClient == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME, _T("failed to create RTSP client"));
    return STATUS_ERROR;
  }

  if (this->sendRtspCommandOptions)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME, _T("send OPTIONS command"));
    this->rtspClient->sendOptionsCommand(&CMPIPTV_RTSP::OnOptionsResponseReceived);
  }
  else if (this->sendRtspCommandDescribe)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME, _T("send DESCRIBE command"));
    this->rtspClient->sendDescribeCommand(&CMPIPTV_RTSP::OnDescribeResponseReceived);
  }
  else
  {
    OnDescribeResponseReceived(this->rtspClient, 0, NULL);
  }

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_OPEN_CONNECTION_NAME);
  return STATUS_OK;
}

void CMPIPTV_RTSP::OnGenericResponseReceived(const TCHAR *command, RTSPClient *client, int resultCode, char *resultString)
{
#ifdef _MBCS
  TCHAR *convertedResultString = ConvertToMultiByteA(resultString);
#else
  TCHAR *convertedResultString = ConvertToUnicodeA(resultString);
#endif
  if (resultCode != 0)
  {
    this->logger.Log(LOGGER_ERROR, _T("%s: %s: %s command failed, code = %i, response = %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, command, resultCode, (convertedResultString == NULL) ? _T("[no result string or conversion failed]") : convertedResultString);
  }
  else
  {
    this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: %s command succeeded, response = %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, command, (convertedResultString == NULL) ? _T("[no result string or conversion failed]") : convertedResultString);
  }
  FREE_MEM(convertedResultString);
}

void CMPIPTV_RTSP::OnOptionsResponseReceived(RTSPClient *client, int resultCode, char *resultString)
{
  CMPIPTV_RTSP* protocolInstance = (CMPIPTV_RTSP*)((MPRTSPClient*)client)->Context();
  if (protocolInstance != NULL && protocolInstance->live555WorkerThreadShouldExit == 0)
  {
    protocolInstance->OnGenericResponseReceived(_T("OPTIONS"), client, resultCode, resultString);
    if (resultCode != 0)
    {
      protocolInstance->live555WorkerThreadShouldExit = 1;
    }
    else if (protocolInstance->sendRtspCommandDescribe)
    {
      protocolInstance->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("send DESCRIBE command"));
      client->sendDescribeCommand(&CMPIPTV_RTSP::OnDescribeResponseReceived);
    }
    else
    {
      OnDescribeResponseReceived(client, 0, NULL);
    }
  }
  if (resultString != NULL)
  {
    delete[] resultString;
  }
}

void CMPIPTV_RTSP::OnDescribeResponseReceived(RTSPClient *client, int resultCode, char *resultString)
{
  CMPIPTV_RTSP* protocolInstance = (CMPIPTV_RTSP*)((MPRTSPClient*)client)->Context();
  if (protocolInstance != NULL && protocolInstance->live555WorkerThreadShouldExit == 0)
  {
    if (protocolInstance->sendRtspCommandDescribe)
    {
      protocolInstance->OnGenericResponseReceived(_T("DESCRIBE"), client, resultCode, resultString);
    }

    if (resultCode != 0)
    {
      protocolInstance->live555WorkerThreadShouldExit = 1;
    }
    else if (protocolInstance->sendRtspCommandDescribe)
    {
      protocolInstance->rtspSession = MediaSession::createNew(*protocolInstance->live555Environment, resultString);
    }
    else
    {
      // Use a generic SDP description for an MPEG 2 transport stream. Note that
      // the control attribute is also included for compatibility. If we don't
      // include the control attribute, the LIVE555-generated URL in the SETUP
      // command will include a trailing slash. That breaks compatibility with
      // the Triax TSS 400, and perhaps other servers too.
      char *sdpDescription = FormatString("v=0\r\nt=0 0\r\na=type:broadcast\r\na=recvonly\r\nm=video 0 RTP/AVP 33\r\na=control:%s", client->url());
      if (sdpDescription == NULL)
      {
        protocolInstance->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("failed to allocate SDP description"));
        protocolInstance->live555WorkerThreadShouldExit = 1;
      }
      else
      {
        protocolInstance->rtspSession = MediaSession::createNew(*protocolInstance->live555Environment, sdpDescription);
        FREE_MEM(sdpDescription);
      }
    }
  }
  if (resultString != NULL)
  {
    delete[] resultString;
  }
  if (protocolInstance->live555WorkerThreadShouldExit == 0)
  {
    protocolInstance->SetupRtspSession();
  }
}

void CMPIPTV_RTSP::SetupRtspSession(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME);

  if (this->rtspSession == NULL || !this->rtspSession->hasSubsessions())
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, this->rtspSession == NULL ? _T("failed to create session") : _T("session doesn't have sub-sessions"));
    this->live555WorkerThreadShouldExit = 1;
    this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME);
    return;
  }

  // Only one sub-session expected/supported.
  MediaSubsessionIterator iter(*this->rtspSession);
  MediaSubsession *subsession = NULL;
  FramedSource *rtpSource = NULL;
  while ((subsession = iter.next()) != NULL)
  {
    // We only support MPEG 2 transport streams.
#ifdef _MBCS
    TCHAR *subSessionName = ConvertToMultiByteA(subsession->mediumName());
    TCHAR *subSessionCodecName = ConvertToMultiByteA(subsession->codecName());
#else
    TCHAR *subSessionName = ConvertToUnicodeA(subsession->mediumName());
    TCHAR *subSessionCodecName = ConvertToUnicodeA(subsession->codecName());
#endif
    if (_tcsncicmp(subSessionName, _T("video"), 5) != 0 || _tcsncicmp(subSessionCodecName, _T("MP2T"), 4) != 0)
    {
      this->logger.Log(LOGGER_ERROR, _T("%s: %s: sub-session medium or codec not supported, medium = %s, codec = %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, subSessionName, subSessionCodecName);
      FREE_MEM(subSessionName);
      FREE_MEM(subSessionCodecName);
      continue;
    }

    // If a preferred RTP port range is configured, try to find a free pair of
    // ports in the range. The first port is used for RTP; the second port is
    // used for RTCP. Once we find one free port, we assume the next one is
    // also free. If we don't find a free port in the range, LIVE555 will
    // choose a random port.
    if (this->rtpClientPortRangeStart > 0)
    {
      struct in_addr destinationAddress;
      destinationAddress.s_addr = our_inet_addr("127.0.0.1");
      unsigned int port = this->rtpClientPortRangeStart;
      Groupsock *groupsock = NULL;
      do
      {
        // don't try to use ports that are already being used
        {
          NoReuse noReuse(*this->live555Environment);
          groupsock = new Groupsock(*this->live555Environment, destinationAddress, port, 1);
        }

        if (groupsock != NULL)
        {
          int socketNumber = groupsock->socketNum();
          delete groupsock;
          if (socketNumber != -1)
          {
            this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: set RTP client port = %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, port);
            subsession->setClientPortNum(port);
            break;
          }
        }
        this->logger.Log(LOGGER_WARNING, _T("%s: %s: RTP client port %u occupied, trying next even port"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, port);
        port += 2;
      }
      while (port <= this->rtpClientPortRangeEnd);
    }

    if (!subsession->initiate() || subsession->rtpSource() == NULL)
    {
      TCHAR *message = FormatString(_T("failed to create receiver for sub-session, medium = %s, codec = %s"), subSessionName, subSessionCodecName);
      this->LogLive555Message(LOGGER_ERROR, METHOD_SETUP_RTSP_SESSION_NAME, message);
      FREE_MEM(message);
      FREE_MEM(subSessionName);
      FREE_MEM(subSessionCodecName);
      continue;
    }

    this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: created receiver for sub-session, medium = %s, codec = %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, subSessionName, subSessionCodecName);
    FREE_MEM(subSessionName);
    FREE_MEM(subSessionCodecName);

    // Set the session ID. It can be anything.
    subsession->setSessionId(subsession->mediumName());

    // Allow an especially large time threshold for reordering incoming data,
    // because we're saving it rather than playing it in real time.
    subsession->rtpSource()->setPacketReorderingThresholdTime(1000000); // 1 second

    // set the RTP source's OS socket buffer size as appropriate
    int socketNum = subsession->rtpSource()->RTPgs()->socketNum();
    unsigned int currentBufferSize = getReceiveBufferSize(*this->live555Environment, socketNum);
    if (this->defaultBufferSize > currentBufferSize)
    {
      setReceiveBufferTo(*this->live555Environment, socketNum, this->defaultBufferSize);
      unsigned setBufferSize = getReceiveBufferSize(*this->live555Environment, socketNum);
      if (setBufferSize == this->defaultBufferSize)
      {
        this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: set buffer size, previous size = %i, requested size = %i, current size = %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, subSessionName, currentBufferSize, this->defaultBufferSize, setBufferSize);
      }
      else
      {
        this->logger.Log(LOGGER_WARNING, _T("%s: %s: failed to set buffer size for sub-session, previous size = %i, requested size = %i, current size = %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, subSessionName, currentBufferSize, this->defaultBufferSize, setBufferSize);
      }
    }

    this->rtpSource = subsession->rtpSource();
    if (subsession->rtcpInstance() != NULL)
    {
      this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, _T("set bye handler"));
      subsession->rtcpInstance()->setByeHandler(&CMPIPTV_RTSP::RtspSessionByeHandler, this);
    }

    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME, _T("send SETUP command"));
    this->rtspClient->sendSetupCommand(*subsession, &CMPIPTV_RTSP::OnSetupResponseReceived);
    this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME);
    return;
  }

  this->live555WorkerThreadShouldExit = 1;
  this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_RTSP_SESSION_NAME);
}

void CMPIPTV_RTSP::OnSetupResponseReceived(RTSPClient *client, int resultCode, char *resultString)
{
  CMPIPTV_RTSP* protocolInstance = (CMPIPTV_RTSP*)((MPRTSPClient*)client)->Context();
  if (protocolInstance != NULL && protocolInstance->live555WorkerThreadShouldExit == 0)
  {
    protocolInstance->OnGenericResponseReceived(_T("SETUP"), client, resultCode, resultString);
    if (resultCode != 0)
    {
      protocolInstance->live555WorkerThreadShouldExit = 1;
    }
    else
    {
      protocolInstance->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("send PLAY command"));
      protocolInstance->isRtspSessionSetup = true;
      protocolInstance->rtspSessionTimeout = client->sessionTimeoutParameter();
      client->sendPlayCommand(*protocolInstance->rtspSession, &CMPIPTV_RTSP::OnPlayResponseReceived);
    }
  }
  if (resultString != NULL)
  {
    delete[] resultString;
  }
}

void CMPIPTV_RTSP::OnPlayResponseReceived(RTSPClient *client, int resultCode, char *resultString)
{
  CMPIPTV_RTSP* protocolInstance = (CMPIPTV_RTSP*)((MPRTSPClient*)client)->Context();
  if (protocolInstance != NULL && protocolInstance->live555WorkerThreadShouldExit == 0)
  {
    protocolInstance->OnGenericResponseReceived(_T("PLAY"), client, resultCode, resultString);
    if (resultCode != 0)
    {
      protocolInstance->live555WorkerThreadShouldExit = 1;
    }
    else
    {
      protocolInstance->SetupLocalUdpConnection();
    }
  }
  if (resultString != NULL)
  {
    delete[] resultString;
  }
}

void CMPIPTV_RTSP::SetupLocalUdpConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME);

  // create UDP socket and start playing
  struct in_addr destinationAddress;
  destinationAddress.s_addr = our_inet_addr("127.0.0.1");

  unsigned int port = this->udpPortRangeStart;
  do
  {
    // don't try to use ports that are already being used
    {
      NoReuse noReuse(*this->live555Environment);
      this->udpGroupsock = new Groupsock(*this->live555Environment, destinationAddress, port, 1);
    }

    if (this->udpGroupsock != NULL && this->udpGroupsock->socketNum() != -1)
    {
      this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: set UDP port = %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME, port);
      break;
    }

    this->logger.Log(LOGGER_WARNING, _T("%s: %s: UDP port %u occupied, trying next port"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME, port);
    port++;
    if (this->udpGroupsock != NULL)
    {
      delete this->udpGroupsock;
      this->udpGroupsock = NULL;
    }
    if (port > this->udpPortRangeEnd)
    {
      this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME, _T("failed to create UDP socket, no free port"));
      this->live555WorkerThreadShouldExit = 1;
      this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME);
      return;
    }
  }
  while (true);

  this->udpSink = BasicUDPSink::createNew(*this->live555Environment, this->udpGroupsock, this->udpSinkMaxPayloadSize);
  if (this->udpSink == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME, _T("failed to create UDP sink"));
    this->live555WorkerThreadShouldExit = 1;
    this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME);
    return;
  }

  if (!this->udpSink->startPlaying(*rtpSource, NULL, NULL))
  {
    this->LogLive555Message(LOGGER_ERROR, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME, _T("failed to start UDP sink"));
    this->live555WorkerThreadShouldExit = 1;
    this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME);
    return;
  }

  this->udpUrl = FormatString(_T("udp://@127.0.0.1:%u"), port);
  SetEvent(this->openConnectionResultEvent);
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SETUP_LOCAL_UDP_CONNECTION_NAME);
}

void CMPIPTV_RTSP::OnTeardownResponseReceived(RTSPClient *client, int resultCode, char *resultString)
{
  CMPIPTV_RTSP* protocolInstance = (CMPIPTV_RTSP*)((MPRTSPClient*)client)->Context();
  if (protocolInstance != NULL && protocolInstance->live555WorkerThreadShouldExit == 0)
  {
    protocolInstance->OnGenericResponseReceived(_T("TEARDOWN"), client, resultCode, resultString);
    protocolInstance->live555WorkerThreadShouldExit = 1;
  }
  if (resultString != NULL)
  {
    delete[] resultString;
  }
}

void CMPIPTV_RTSP::CleanUpLive555(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME);

  // close all media sinks before closing the RTSP session and client
  if (this->udpSink != NULL)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME, _T("closing UDP sink"));
    this->udpSink->stopPlaying();
    Medium::close(this->udpSink);
    this->udpSink = NULL;
  }

  if (this->udpGroupsock != NULL)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME, _T("closing UDP socket"));
    delete this->udpGroupsock;
    this->udpGroupsock = NULL;
  }

  if (this->rtspSession != NULL)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME, _T("closing RTSP session"));
    Medium::close(this->rtspSession);
    this->rtspSession = NULL;
  }

  if (this->rtspClient != NULL)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME, _T("closing RTSP client"));
    Medium::close(this->rtspClient);
    this->rtspClient = NULL;
  }

  if (this->live555Environment != NULL)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME, _T("closing LIVE555 environment"));
    this->live555Environment->reclaim();
    this->live555Environment = NULL;
  }

  if (this->live555Scheduler != NULL)
  {
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME, _T("closing LIVE555 scheduler"));
    delete this->live555Scheduler;
    this->live555Scheduler = NULL;
  }

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAN_UP_LIVE555_NAME);
}

void CMPIPTV_RTSP::RtspSessionByeHandler(void *lpCMPIPTV_RTSP)
{
  CMPIPTV_RTSP *protocolInstance = (CMPIPTV_RTSP *)lpCMPIPTV_RTSP;

  protocolInstance->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RTSP_SESSION_BYE_HANDLER_NAME);

  protocolInstance->live555WorkerThreadShouldExit = 1;

  protocolInstance->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RTSP_SESSION_BYE_HANDLER_NAME);
}
