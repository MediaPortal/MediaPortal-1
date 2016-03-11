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

// MPIPTV_RTP.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#include "MPIPTV_RTP.h"
#include "Network.h"
#include "Utilities.h"

#include <stdio.h>

// protocol implementation name
#define PROTOCOL_IMPLEMENTATION_NAME                                    _T("CMPIPTV_RTP")

PIProtocol CreateProtocolInstance(void)
{
  return new CMPIPTV_RTP;
}

void DestroyProtocolInstance(PIProtocol pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPIPTV_RTP *pClass = (CMPIPTV_RTP *)pProtocol;
    delete pClass;
  }
}

CMPIPTV_RTP::CMPIPTV_RTP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  this->rtpFailPackets = 0;
  this->switchedToUdp = false;
  this->maxFailedPackets = 0;
  this->rtpHandler = NULL;
  this->openConnetionMaximumAttempts = RTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTV_RTP::~CMPIPTV_RTP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->rtpHandler != NULL)
  {
    delete this->rtpHandler;
  }
  this->rtpHandler = NULL;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
}

int CMPIPTV_RTP::Initialize(HANDLE lockMutex, CParameterCollection *configuration)
{
  if (configuration != NULL)
  {
    CParameterCollection *udpParameters = GetConfiguration(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME, CONFIGURATION_SECTION_UDP);
    configuration->Append(udpParameters);
    delete udpParameters;
  }

  int result = this->CMPIPTV_UDP::Initialize(lockMutex, configuration);

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_RTP_RECEIVE_DATA_TIMEOUT, true, RTP_RECEIVE_DATA_TIMEOUT_DEFAULT);
  this->maxFailedPackets = this->configurationParameters->GetValueLong(CONFIGURATION_RTP_MAX_FAILED_PACKETS, true, RTP_MAX_FAILED_PACKETS_DEFAULT);
  this->openConnetionMaximumAttempts = this->configurationParameters->GetValueLong(CONFIGURATION_RTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS, true, RTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT);

  this->maxFailedPackets = (this->maxFailedPackets <= 0) ? RTP_MAX_FAILED_PACKETS_DEFAULT : this->maxFailedPackets;
  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
  this->openConnetionMaximumAttempts = (this->openConnetionMaximumAttempts < 0) ? RTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT : this->openConnetionMaximumAttempts;

  return result;
}

TCHAR *CMPIPTV_RTP::GetProtocolName(void)
{
  return Duplicate(CONFIGURATION_SECTION_RTP);
}

int CMPIPTV_RTP::ClearSession(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  if (this->rtpHandler != NULL)
  {
    delete this->rtpHandler;
  }
  this->rtpHandler = NULL;
  this->rtpFailPackets = 0;
  this->switchedToUdp = false;

  this->CMPIPTV_UDP::ClearSession();
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  return STATUS_OK;
}

int CMPIPTV_RTP::ParseUrl(const TCHAR *url, const CParameterCollection *parameters)
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

      if (_tcsncicmp(urlComponents->lpszScheme, _T("RTP"), urlComponents->dwSchemeLength) != 0)
      {
        // not supported protocol
        this->logger.Log(LOGGER_INFO, _T("%s: %s: unsupported protocol '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
        result = STATUS_ERROR;
      }
    }
    FREE_MEM(protocol);
      
    if (result == STATUS_OK)
    {
      length = urlComponents->dwHostNameLength + 1;
      this->localAddress = ALLOC_MEM_SET(this->localAddress, TCHAR, length, 0);
      if (this->localAddress == NULL) 
      {
        this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'localAddress'"));
        result = STATUS_ERROR;
      }
    }

    if (result == STATUS_OK)
    {
      _tcsncat_s(this->localAddress, length, urlComponents->lpszHostName, urlComponents->dwHostNameLength);

      if (urlComponents->dwUserNameLength > 0) 
      {
        length = urlComponents->dwUserNameLength + 1;
        this->sourceAddress = ALLOC_MEM_SET(this->sourceAddress, TCHAR, length, 0);
        if (this->sourceAddress == NULL) 
        {
          this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'sourceAddress'"));
          result = STATUS_ERROR;
        }
        
        if (result == STATUS_OK)
        {
          _tcsncat_s(this->sourceAddress, length, urlComponents->lpszUserName, urlComponents->dwUserNameLength);
        }
      }
      if (urlComponents->dwPasswordLength > 0) 
      {
        // its port for remote address
        length = urlComponents->dwPasswordLength + 1;
        this->sourcePort = _tstoi(urlComponents->lpszPassword);
      }

      this->localPort = urlComponents->nPort;

      this->logger.Log(LOGGER_INFO, _T("%s: %s: local address: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->localAddress);
      this->logger.Log(LOGGER_INFO, _T("%s: %s: local port: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->localPort);
      this->logger.Log(LOGGER_INFO, _T("%s: %s: source address: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->sourceAddress);
      this->logger.Log(LOGGER_INFO, _T("%s: %s: source port: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->sourcePort);
    }
  }
  FREE_MEM(urlComponents);
  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  return result;
}

int CMPIPTV_RTP::OpenConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);

  this->rtpHandler = new RtpSource(&this->logger);
  int retval = this->CMPIPTV_UDP::OpenConnection();

  this->logger.Log(LOGGER_INFO, (retval == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  return retval;
}

int CMPIPTV_RTP::IsConnected(void)
{
  return this->CMPIPTV_UDP::IsConnected();
}

void CMPIPTV_RTP::CloseConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);

  this->CMPIPTV_UDP::CloseConnection();

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);
}

void CMPIPTV_RTP::GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize)
{
  this->CMPIPTV_UDP::GetSafeBufferSizes(lockMutex, freeSpace, occupiedSpace, bufferSize);
}

void CMPIPTV_RTP::ReceiveData(bool *shouldExit)
{
  if (!this->switchedToUdp)
  {
    // if not switched to UDP
    // parse RTP protocol packets

    if (this->IsConnected())
    {
      unsigned int freeSpace = 0;
      unsigned int occupiedSpace = 0;
      unsigned int bufferSize = 0;

      this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
      this->logger.Log(LOGGER_DATA, _T("%s: %s: buffer size: %u, free buffer size: %u, occupied buffer size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace);

      if (freeSpace >= this->packetSize)
      {
        // there is some place in buffer
        // and probably (!) is there sufficient place for received data
        // probably = received data length may variate

        int length = 0;

        WaitForSingleObject(this->lockMutex, INFINITE);
        do
        {
          length = GetDataFromSocket(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->m_socket, this->receiveBuffer, freeSpace);

          if (length == SOCKET_ERROR)
          {
            // socket error occured => close connection
            this->CloseConnection();
          }
          else if (length == 0)
          {
            // no data received
            // sleep some time
          }
          else
          {
            if (this->dumpInputPackets)
            {
              DumpInputPacket(this->GetInstanceId(), length, this->receiveBuffer);
            }

            bool isRtpPacket = this->rtpHandler->IsRtpPacket(this->receiveBuffer, (unsigned int)length);
            if (!isRtpPacket) 
            {
              // this is not a rtp packet
              // if there are RTP_MAX_FAIL_PACKETS then consider this stream as udp instead
              this->rtpFailPackets++;
              this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: invalid rtp packet received, failed packets: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->rtpFailPackets);

              if (this->rtpFailPackets >= this->maxFailedPackets) 
              {
                // this is not a rtp stream
                // consider it as raw udp

                this->logger.Log(LOGGER_INFO, _T("%s: %s: switching back to raw udp"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
                this->switchedToUdp = true;
                // Skip this packet
              }

              // This is still not a correct rtp packet -> skip it
            } 
            else
            {
              this->rtpFailPackets = 0;
            }

            if ((!this->switchedToUdp) && (isRtpPacket))
            {
              // now we can be sure, that this is a rtp packet -> process it
              if (!this->rtpHandler->ProcessPacket(this->receiveBuffer, (unsigned int)length))
              {
                this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, _T("ProcessPacket() failed"));

                // close connection
                // new connection will try to open in next run
                this->CloseConnection();
              }
              else
              {
                unsigned int firstSequenceNumber;
                unsigned int lastSequenceNumber;
                unsigned int rtpPacketBytes = this->rtpHandler->GetAndRemovePacketData(this->receiveBuffer, this->defaultBufferSize, &firstSequenceNumber, &lastSequenceNumber, true);

                if (rtpPacketBytes > 0)
                {
                  if (rtpPacketBytes > this->packetSize)
                  {
                    this->packetSize = rtpPacketBytes;
                  }

                  this->buffer.AddToBuffer(this->receiveBuffer, rtpPacketBytes);
                  freeSpace -= rtpPacketBytes;
                }
              }
            }
          }
        }
        while ((length > 0) && (freeSpace >= this->packetSize) && (!this->switchedToUdp) && (!(*shouldExit)));
        this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
        this->logger.Log(LOGGER_DATA, _T("%s: %s: buffer size: %u, free buffer size: %u, occupied buffer size: %u, packet size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace, this->packetSize);
        ReleaseMutex(this->lockMutex);
      }
      else
      {
        // buffer free space to low
        this->logger.Log(LOGGER_WARNING, _T("%s: %s: no data received, buffer free space too small, buffer size: %u, free buffer size: %u, occupied buffer size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace);
        this->buffer.SafeResizeBuffer(this->lockMutex, min(bufferSize * 2, this->maxBufferSize), true, &this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
      }
    }
    else
    {
      this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, _T("socket connection not opened, opening new one"));
      // re-open connection if previous is lost
      if (this->OpenConnection() != STATUS_OK)
      {
        this->CloseConnection();
      }
    }
  }
  else
  {
    // just call base method
    this->CMPIPTV_UDP::ReceiveData(shouldExit);
  }
}

unsigned int CMPIPTV_RTP::FillBuffer(IMediaSample *pSamp, char *pData, long cbData)
{
  return this->CMPIPTV_UDP::FillBuffer(pSamp, pData, cbData);
}

unsigned int CMPIPTV_RTP::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

unsigned int CMPIPTV_RTP::GetOpenConnectionMaximumAttempts(void)
{
  return this->openConnetionMaximumAttempts;
}
