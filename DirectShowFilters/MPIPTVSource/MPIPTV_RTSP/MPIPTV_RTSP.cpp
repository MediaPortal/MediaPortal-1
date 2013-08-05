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
#define METHOD_CLOSE_SINKS_NAME                                         _T("CloseSinks()")
#define METHOD_TEARDOWN_MEDIA_SESSION_NAME                              _T("TeardownMediaSession()")
#define METHOD_RTSP_TEARDOWN_SESSION_WORKER_NAME                        _T("RtspTearDownSessionWorker()")

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
  this->rtspSession = NULL;
  this->rtspSchedulerThreadHandle = NULL;
  this->rtspSchedulerThreadId = 0;
  this->rtspThreadShouldExit = 0;
  this->rtspSource = NULL;
  this->rtspUdpSink = NULL;
  this->rtspUdpGroupsock = NULL;
  this->rtspUdpSinkMaxPayloadSize = RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT;
  this->rtspUdpPortRangeStart = RTSP_UDP_PORT_RANGE_START_DEFAULT;
  this->rtspUdpPortRangeEnd = RTSP_UDP_PORT_RANGE_END_DEFAULT;
  this->rtspTeardownRequestMaximumCount = RTSP_TEARDOWN_REQUEST_MAXIMUM_COUNT_DEFAULT;
  this->rtspTeardownRequestTimeout = RTSP_TEARDOWN_REQUEST_TIMEOUT_DEFAULT;
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
  this->rtspUdpSinkMaxPayloadSize = this->configurationParameters->GetValueUnsignedInt(CONFIGURATION_RTSP_UDP_SINK_MAX_PAYLOAD_SIZE, true, RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT);
  this->rtspUdpPortRangeStart = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_UDP_PORT_RANGE_START, true, RTSP_UDP_PORT_RANGE_START_DEFAULT);
  this->rtspUdpPortRangeEnd = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_UDP_PORT_RANGE_END, true, RTSP_UDP_PORT_RANGE_END_DEFAULT);
  this->rtspTeardownRequestMaximumCount = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_TEARDOWN_REQUEST_MAXIMUM_COUNT, true, RTSP_TEARDOWN_REQUEST_MAXIMUM_COUNT_DEFAULT);
  this->rtspTeardownRequestTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_TEARDOWN_REQUEST_TIMEOUT, true, RTSP_TEARDOWN_REQUEST_TIMEOUT_DEFAULT);
  this->openConnetionMaximumAttempts = this->configurationParameters->GetValueLong(CONFIGURATION_RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS, true, RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
  this->rtspUdpSinkMaxPayloadSize = (this->rtspUdpSinkMaxPayloadSize < 0) ? RTSP_UDP_SINK_MAX_PAYLOAD_SIZE_DEFAULT : this->rtspUdpSinkMaxPayloadSize;
  this->rtspUdpPortRangeStart = (this->rtspUdpPortRangeStart <= 1024) ? RTSP_UDP_PORT_RANGE_START_DEFAULT : this->rtspUdpPortRangeStart;
  this->rtspUdpPortRangeEnd = (this->rtspUdpPortRangeEnd < this->rtspUdpPortRangeStart) ? min(65535, this->rtspUdpPortRangeStart + 1000) : min(65535, this->rtspUdpPortRangeEnd);
  this->rtspTeardownRequestMaximumCount = (this->rtspTeardownRequestMaximumCount <= 0) ? RTSP_TEARDOWN_REQUEST_MAXIMUM_COUNT_DEFAULT : this->rtspTeardownRequestMaximumCount;
  this->rtspTeardownRequestTimeout = (this->rtspTeardownRequestTimeout < 0) ? RTSP_TEARDOWN_REQUEST_TIMEOUT_DEFAULT : this->rtspTeardownRequestTimeout;
  this->openConnetionMaximumAttempts = (this->openConnetionMaximumAttempts < 0) ? RTSP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT : this->openConnetionMaximumAttempts;

  this->rtspScheduler = RtspTaskScheduler::createNew();
  this->rtspEnvironment = BasicUsageEnvironment::createNew(*this->rtspScheduler);

  result |= (this->rtspScheduler == NULL);
  result |= (this->rtspEnvironment == NULL);

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
  int result = STATUS_OK;

  this->rtspClient = RTSPClient::createNew(*this->rtspEnvironment);
  result |= (this->rtspClient == NULL);

  if (result == STATUS_OK)
  {
    // RTSPClient works with char, not with TCHAR
    char *tempRtspUrl = ConvertToMultiByte(this->rtspUrl);
    result |= (tempRtspUrl == NULL);
    if (result == STATUS_OK)
    {
      char* optionsResult = this->rtspClient->sendOptionsCmd(tempRtspUrl, NULL, NULL, NULL, this->receiveDataTimeout / 2000);
      result |= (optionsResult == NULL);

      if (result != STATUS_OK)
      {
        TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("error occured while sending OPTIONS command"));
        this->LogRtspMessage(LOGGER_ERROR, message);
        FREE_MEM(message);
      }
      else
      {
        TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("OPTIONS result"));
        this->LogFullRtspMessage(LOGGER_VERBOSE, message, optionsResult);
        FREE_MEM(message);

        char *describeResult = this->rtspClient->describeURL(tempRtspUrl, NULL, FALSE, this->receiveDataTimeout / 2000);
        result |= (describeResult == NULL);

        if (result != STATUS_OK)
        {
          TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("error occured while sending DESCRIBE command"));
          this->LogRtspMessage(LOGGER_ERROR, message);
          FREE_MEM(message);
        }
        else
        {
          TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("DESCRIBE result"));
          this->LogFullRtspMessage(LOGGER_VERBOSE, message, describeResult);
          FREE_MEM(message);

          this->rtspSession = MediaSession::createNew(*this->rtspEnvironment, describeResult);
          result |= (this->rtspSession == NULL);

          if (result != STATUS_OK)
          {
            TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("error occured while creating new session"));
            this->LogRtspMessage(LOGGER_ERROR, message);
            FREE_MEM(message);
          }
          else
          {
            result |= (!this->rtspSession->hasSubsessions());
            if (result != STATUS_OK)
            {
              this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("session doesn't have subsessions"));
            }
            else
            {
              // Then, setup the "RTPSource"s for the session:
              MediaSubsessionIterator iter(*this->rtspSession);
              MediaSubsession *subsession = NULL;

              while ((result == STATUS_OK) && ((subsession = iter.next()) != NULL))
              {
                char *tempSubSessionName = (char *)subsession->mediumName();
                char *tempSubSessionCodecName = (char *)subsession->codecName();
#ifdef _MBCS
                TCHAR *subSessionName = ConvertToMultiByteA(tempSubSessionName);
                TCHAR *subSessionCodecName = ConvertToMultiByteA(tempSubSessionCodecName);
#else
                TCHAR *subSessionName = ConvertToUnicodeA(tempSubSessionName);
                TCHAR *subSessionCodecName = ConvertToUnicodeA(tempSubSessionCodecName);
#endif
                if (!subsession->initiate())
                {
                  result = STATUS_ERROR;
                  TCHAR *message = FormatString(_T("%s: %s: unable to create receiver for subsession '%s', codec '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName, subSessionCodecName);
                  this->LogRtspMessage(LOGGER_ERROR, message);
                  FREE_MEM(message);
                }
                else
                {
                  this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: created receiver for subsession '%s', codec '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName, subSessionCodecName);

                  // set session ID, doesn't matter what
                  subsession->sessionId = tempSubSessionName;

                  if (subsession->rtpSource() != NULL)
                  {
                    // because we're saving the incoming data, rather than playing
                    // it in real time, allow an especially large time threshold
                    // (1 second) for reordering misordered incoming packets:
                    unsigned const thresh = 1000000; // 1 second
                    subsession->rtpSource()->setPacketReorderingThresholdTime(thresh);

                    // set the RTP source's OS socket buffer size as appropriate
                    int socketNum = subsession->rtpSource()->RTPgs()->socketNum();
                    unsigned int currentBufferSize = getReceiveBufferSize(*this->rtspEnvironment, socketNum);

                    if (this->defaultBufferSize > currentBufferSize)
                    {
                      setReceiveBufferTo(*this->rtspEnvironment, socketNum, this->defaultBufferSize);
                      unsigned setBufferSize = getReceiveBufferSize(*this->rtspEnvironment, socketNum);

                      if (setBufferSize == this->defaultBufferSize)
                      {
                        this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: set buffer size for subsession '%s' successful, previous size: %i, requested size: %i, current size: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName, currentBufferSize, this->defaultBufferSize, setBufferSize);
                      }
                      else
                      {
                        result = STATUS_ERROR;
                        this->logger.Log(LOGGER_ERROR, _T("%s: %s: set buffer size for subsession '%s' failed, previous size: %i, requested size: %i, current size: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName, currentBufferSize, this->defaultBufferSize, setBufferSize);
                      }
                    }

                    if (_tcsncicmp(subSessionName, _T("audio"), 5) == 0)
                    {
                      // audio
                      this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: audio subsession '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName);
                      result |= (!rtspClient->setupMediaSubsession(*subsession));

                      if (result != STATUS_OK)
                      {
                        // error occured
                        TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("error while setup subsession"));
                        this->LogRtspMessage(LOGGER_ERROR, message);
                        FREE_MEM(message);
                      }
                      else
                      {
                        this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("subsession audio codec not supported"));
                      }
                    }
                    else if (_tcsncicmp(subSessionName, _T("video"), 5) == 0)
                    {
                      // video
                      this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: video subsession '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName);
                      result |= (!rtspClient->setupMediaSubsession(*subsession));

                      if (result != STATUS_OK)
                      {
                        // error occured
                        TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("error while setup subsession"));
                        this->LogRtspMessage(LOGGER_ERROR, message);
                        FREE_MEM(message);
                      }
                      else
                      {
                        if (_tcsncicmp(subSessionCodecName, _T("MP2T"), 4) == 0)
                        {
                          // MPEG2 Transport Stream
                          // set new RTSP source
                          this->rtspSource = subsession->rtpSource();

                          if (subsession->rtcpInstance() != NULL)
                          {
                            this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("set subsession 'Bye' handler"));
                            subsession->rtcpInstance()->setByeHandler(SubsessionByeHandler, this);
                          }
                        }
                        else if (_tcsncicmp(subSessionCodecName, _T("H264"), 4) == 0)
                        {
                          // H264 codec, HD TV
                          this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("H264 not supported"));
                          result = STATUS_ERROR;
                        }
                        else
                        {
                          // SD TV
                          this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("other subsession video codec than MP2T not supported"));
                          result = STATUS_ERROR;
                        }
                      }
                    }
                    else
                    {
                      this->logger.Log(LOGGER_WARNING, _T("%s: %s: unknown subsession '%s', ignored"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName);
                    }
                  }
                  else
                  {
                    this->logger.Log(LOGGER_WARNING, _T("%s: %s: subsession '%s' doesn't have RTP source"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, subSessionName);
                  }
                }

                // free subsession name and codec name
                FREE_MEM(subSessionName);
                FREE_MEM(subSessionCodecName);
              }

              // we should have some RTSP source
              result |= (this->rtspSource == NULL);

              if (result == STATUS_OK)
              {
                result |= (!this->rtspClient->playMediaSession(*this->rtspSession));

                if (result != STATUS_OK)
                {
                  // error occured
                  TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("error while playing session"));
                  this->LogRtspMessage(LOGGER_ERROR, message);
                  FREE_MEM(message);
                }
                else
                {
                  // create UDP socket and start playing
                  struct in_addr destinationAddress;
                  destinationAddress.s_addr = our_inet_addr("127.0.0.1");

                  unsigned int port = this->rtspUdpPortRangeStart;
                  do
                  {
                    this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: UDP port %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, port);

                    // special construction force not reuse same UDP port
                    {
                      NoReuse noReuse;
                      this->rtspUdpGroupsock = new Groupsock(*this->rtspEnvironment, destinationAddress, port, 1);
                    }

                    if (this->rtspUdpGroupsock->socketNum() == (-1))
                    {
                      this->logger.Log(LOGGER_ERROR, _T("%s: %s: UDP port %u occupied, trying another port"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, port);
                      port++;
                      delete this->rtspUdpGroupsock;
                      this->rtspUdpGroupsock = NULL;
                    }
                  }
                  while ((this->rtspUdpGroupsock == NULL) && (port <= this->rtspUdpPortRangeEnd));

                  result |= (this->rtspUdpGroupsock == NULL);
                  if (result != STATUS_OK)
                  {
                    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("cannot create UDP sink, no free port"));
                  }
                  else
                  {
                    this->rtspUdpSink = BasicUDPSink::createNew(*this->rtspEnvironment, this->rtspUdpGroupsock, this->rtspUdpSinkMaxPayloadSize);
                    result |= (this->rtspUdpSink == NULL);

                    if (result != STATUS_OK)
                    {
                      TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("cannot create UDP sink"));
                      this->LogRtspMessage(LOGGER_ERROR, message);
                      FREE_MEM(message);
                    }
                    else
                    {
                      if (this->rtspUdpSink->startPlaying(*this->rtspSource, NULL, NULL))
                      {
                        this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("playing started"));

                        // now create UDP connection
                        TCHAR *url = FormatString(_T("udp://@127.0.0.1:%u"), port);
                        result |= (url == NULL);

                        if (result == STATUS_OK)
                        {
                          // parse UDP url
                          // ParseURL calls ClearSession and IsConnected must return FALSE
                          // in another case will be current RTSP connection closed
                          result = this->CMPIPTV_UDP::ParseUrl(url, NULL);

                          if (result == STATUS_OK)
                          {
                            // connect to UDP url
                            result = this->CMPIPTV_UDP::OpenConnection();
                          }
                        }
                        FREE_MEM(url);
                      }
                      else
                      {
                        result = STATUS_ERROR;
                        TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("error occured while starting playing"));
                        this->LogRtspMessage(LOGGER_ERROR, message);
                        FREE_MEM(message);
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }

      if (optionsResult != NULL)
      {
        delete[] optionsResult;
        optionsResult = NULL;
      }
    }
    FREE_MEM(tempRtspUrl);
  }

  if (result == STATUS_OK)
  {
    // start winsock worker thread
    this->rtspSchedulerThreadHandle = CreateThread( 
      NULL,                                   // default security attributes
      0,                                      // use default stack size  
      &CMPIPTV_RTSP::RtspSchedulerWorker,     // thread function name
      this,                                   // argument to thread function 
      0,                                      // use default creation flags 
      &this->rtspSchedulerThreadId);          // returns the thread identifier

    if (this->rtspSchedulerThreadHandle == NULL)
    {
      // thread not created
      result = STATUS_ERROR;
      this->logger.Log(LOGGER_ERROR, _T("%s: %s: cannot create RTSP scheduler thread, error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, GetLastError());
    }
  }

  if (result != STATUS_OK)
  {
    // if failed opening connection, than close connection
    this->CloseConnection();
  }

  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  return (result == STATUS_OK) ? STATUS_OK : STATUS_ERROR;
}

int CMPIPTV_RTSP::IsConnected(void)
{
  if (this->rtspUdpGroupsock != NULL)
  {
    // receiving data, return status of UDP connection
    return this->CMPIPTV_UDP::IsConnected();
  }
  else
  {
    if (!this->CMPIPTV_UDP::IsConnected())
    {
      return FALSE;
    }
    else
    {
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
  }
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
    this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("waiting RTSP worker thread to exit"));
    if (WaitForSingleObject(this->rtspSchedulerThreadHandle, 1000) == WAIT_TIMEOUT)
    {
      // thread didn't exit, kill it now
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("RTSP worker thread didn't exit, terminating thread"));
      TerminateThread(this->rtspSchedulerThreadHandle, 0);
    }
  }

  this->rtspSchedulerThreadHandle = NULL;
  this->rtspThreadShouldExit = 0;

  // this->rtspSource is part of subsession, which is removed with this->rtspSession
  this->rtspSource = NULL;

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

char *CMPIPTV_RTSP::GetLastRtspMessageA(void)
{
  return ConvertToMultiByteA(this->rtspEnvironment->getResultMsg());
}

wchar_t *CMPIPTV_RTSP::GetLastRtspMessageW(void)
{
  return ConvertToUnicodeA(this->rtspEnvironment->getResultMsg());
}

void CMPIPTV_RTSP::LogRtspMessage(unsigned int loggerLevel, const TCHAR* messagePrefix)
{
  char *lastRtspMessage = this->GetLastRtspMessageA();

  this->LogFullRtspMessage(loggerLevel, messagePrefix, lastRtspMessage);

  FREE_MEM(lastRtspMessage);
}

void CMPIPTV_RTSP::LogFullRtspMessage(unsigned int loggerLevel, const TCHAR* messagePrefix, const char *message)
{
#ifdef _MBCS
  TCHAR *convertedRtspMessage = ConvertToMultiByteA(message);
#else
  TCHAR *convertedRtspMessage = ConvertToUnicodeA(message);
#endif

  this->logger.Log(loggerLevel, _T("%s%s %s"), messagePrefix, (convertedRtspMessage == NULL) ? _T(",") : _T(":"), (convertedRtspMessage == NULL) ? _T("unable to get message") : convertedRtspMessage);
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

void CMPIPTV_RTSP::CloseSinks(void)
{
  // free UDP sink
  if (this->rtspUdpSink != NULL)
  {
    this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_SINKS_NAME, _T("closing UDP sink"));
    this->rtspUdpSink->stopPlaying();
    Medium::close(this->rtspUdpSink);
    this->rtspUdpSink = NULL;
  }

  // free UDP socket
  if (this->rtspUdpGroupsock != NULL)
  {
    this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_SINKS_NAME, _T("closing UDP socket"));
    delete this->rtspUdpGroupsock;
    this->rtspUdpGroupsock = NULL;
  }
}

bool CMPIPTV_RTSP::TeardownMediaSession(bool forceTeardown)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME);
  bool result = (((this->rtspClient == NULL) && (this->rtspSession == NULL)) || ((this->rtspClient != NULL) && (this->rtspSession != NULL)));

  if (result && (this->rtspClient != NULL) && (this->rtspSession != NULL))
  {
    unsigned int requestCount = 0;
    do
    {
      HANDLE rtspTearDownSessionThreadHandle = NULL;
      DWORD rtspTearDownSessionThreadId = 0;

      // start RTSP teardown session worker thread
      rtspTearDownSessionThreadHandle = CreateThread( 
        NULL,                                       // default security attributes
        0,                                          // use default stack size  
        &CMPIPTV_RTSP::RtspTearDownSessionWorker,   // thread function name
        this,                                       // argument to thread function 
        0,                                          // use default creation flags 
        &rtspTearDownSessionThreadId);              // returns the thread identifier

      if (rtspTearDownSessionThreadHandle == NULL)
      {
        // thread not created
        // just log error message and try again
        this->logger.Log(LOGGER_ERROR, _T("%s: %s: cannot create RTSP teardown worker thread, error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, GetLastError());
      }
      else
      {
        this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("waiting RTSP teardown worker thread to exit"));
        if (WaitForSingleObject(rtspTearDownSessionThreadHandle, this->rtspTeardownRequestTimeout) == WAIT_TIMEOUT)
        {
          // thread didn't exit, kill it now
          this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("RTSP teardown worker thread didn't exit, terminating thread"));
          TerminateThread(rtspTearDownSessionThreadHandle, 0);
        }
        else
        {
          // thread terminated
          // set result and break
          result &= this->rtspTearDownSessionWorkerResult;
          break;
        }
      }

      requestCount++;

      // give another threads chance to work
      Sleep(1);

      result &= (requestCount <= this->rtspTeardownRequestMaximumCount);
    } while (result);
  }

  if (!result && (this->rtspClient != NULL) && (this->rtspSession != NULL))
  {
    TCHAR *message = FormatString(METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("cannot teardown media session"));
    this->LogRtspMessage(LOGGER_ERROR, message);
    FREE_MEM(message);
  }

  if (forceTeardown || result)
  {
    // close all media sinks after teardown and before closing RTSP session and client
    this->CloseSinks();

    if (this->rtspSession != NULL)
    {
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("closing RTSP session"));
      Medium::close(this->rtspSession);
    }

    if (this->rtspClient != NULL)
    {
      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME, _T("closing RTSP client"));
      Medium::close(this->rtspClient);
    }

    this->rtspSession = NULL;
    this->rtspClient = NULL;

    result = true;
  }

  this->logger.Log(LOGGER_INFO, (result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_TEARDOWN_MEDIA_SESSION_NAME);
  return result;
}

DWORD WINAPI CMPIPTV_RTSP::RtspTearDownSessionWorker(LPVOID lpParam)
{
  CMPIPTV_RTSP *caller = (CMPIPTV_RTSP*)lpParam;
  caller->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RTSP_TEARDOWN_SESSION_WORKER_NAME);
  caller->rtspTearDownSessionWorkerResult = false;

  caller->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RTSP_TEARDOWN_SESSION_WORKER_NAME, _T("sending TEARDOWN request"));
  caller->rtspTearDownSessionWorkerResult = (caller->rtspClient->teardownMediaSession(*caller->rtspSession) == 1);

  caller->logger.Log(LOGGER_INFO, (caller->rtspTearDownSessionWorkerResult) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RTSP_TEARDOWN_SESSION_WORKER_NAME);
  return S_OK;
}

unsigned int CMPIPTV_RTSP::GetOpenConnectionMaximumAttempts(void)
{
  return this->openConnetionMaximumAttempts;
}
