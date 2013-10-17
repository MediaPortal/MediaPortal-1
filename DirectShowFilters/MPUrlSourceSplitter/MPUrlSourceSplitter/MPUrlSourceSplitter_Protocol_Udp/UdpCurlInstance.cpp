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

#include "StdAfx.h"

#include "UdpCurlInstance.h"
#include "UdpServer.h"
#include "UdpSocketContext.h"
#include "MulticastUdpServer.h"
#include "LockMutex.h"
#include "conversions.h"
#include "Dns.h"
#include "IpAddress.h"
#include "RtpPacket.h"

CUdpCurlInstance::CUdpCurlInstance(CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(logger, mutex, protocolName, instanceName)
{
  this->udpDownloadRequest = dynamic_cast<CUdpDownloadRequest *>(this->downloadRequest);
  this->udpDownloadResponse = dynamic_cast<CUdpDownloadResponse *>(this->downloadResponse);

  this->localAddress = NULL;
  this->sourceAddress = NULL;
  this->localPort = PORT_UNSPECIFIED;
  this->sourcePort = PORT_UNSPECIFIED;

  this->flags = UDP_CURL_INSTANCE_FLAG_NONE;
}

CUdpCurlInstance::~CUdpCurlInstance(void)
{
  this->StopReceivingData();

  FREE_MEM(this->localAddress);
  FREE_MEM(this->sourceAddress);
}

/* get methods */

CUdpDownloadResponse *CUdpCurlInstance::GetUdpDownloadResponse(void)
{
  return this->udpDownloadResponse;
}

/* set methods */

/* other methods */

bool CUdpCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  unsigned int endTicks = (this->finishTime == FINISH_TIME_NOT_SPECIFIED) ? (GetTickCount() + this->GetReceiveDataTimeout()) : this->finishTime;

  bool result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->udpDownloadRequest = dynamic_cast<CUdpDownloadRequest  *>(this->downloadRequest);
  this->udpDownloadResponse = dynamic_cast<CUdpDownloadResponse *>(this->downloadResponse);
  result &= (this->udpDownloadRequest != NULL) && (this->udpDownloadResponse != NULL);

  if (result)
  {
    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
    result &= (urlComponents != NULL);

    if (result)
    {
      ZeroURL(urlComponents);
      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

      result &= (InternetCrackUrl(downloadRequest->GetUrl(), 0, 0, urlComponents) == TRUE);

      if (SUCCEEDED(result))
      {
        this->localAddress = Substring(urlComponents->lpszHostName, 0, urlComponents->dwHostNameLength);
        result &= (this->localAddress != NULL);

        this->localPort = urlComponents->nPort;

        if (urlComponents->dwUserNameLength > 0)
        {
          this->sourceAddress = Substring(urlComponents->lpszUserName, 0, urlComponents->dwUserNameLength);
          result &= (this->sourceAddress != NULL);
        }

        if (urlComponents->dwPasswordLength > 0)
        {
          // its port for source address
          this->sourcePort = GetValueUnsignedInt(urlComponents->lpszPassword, PORT_UNSPECIFIED);
        }

        if (result)
        {
          this->logger->Log(LOGGER_INFO, L"%s: %s: local address '%s', local port %u, source address '%s', source port %u", this->protocolName, METHOD_INITIALIZE_NAME,
            (this->localAddress == NULL) ? L"NULL" : this->localAddress,
            this->localPort,
            (this->sourceAddress == NULL) ? L"NULL" : this->sourceAddress,
            this->sourcePort);
        }
      }
    }

    FREE_MEM(urlComponents);
  }

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

/* protected methods */

CDownloadResponse *CUdpCurlInstance::GetNewDownloadResponse(void)
{
  return new CUdpDownloadResponse();
}

unsigned int CUdpCurlInstance::CurlWorker(void)
{
  this->logger->Log(LOGGER_INFO, L"%s: %s: Start, url: '%s'", this->protocolName, METHOD_CURL_WORKER_NAME, this->downloadRequest->GetUrl());
  this->startReceivingTicks = GetTickCount();
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;

  CURLcode errorCode = CURLE_OK;
  ALLOC_MEM_DEFINE_SET(buffer, unsigned char, BUFFER_LENGTH_DEFAULT, 0);
  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (buffer != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);

  // local address can be address of some network interface, can be localhost or can be mutlicast address
  // check if local address is mutlicast or unicast address

  CIpAddressCollection *localIpAddresses = new CIpAddressCollection();
  CIpAddressCollection *sourceIpAddresses = new CIpAddressCollection();
  CRtpPacket *rtpPacket = new CRtpPacket();

  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (localIpAddresses != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (sourceIpAddresses != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (rtpPacket != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);

  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = SUCCEEDED(CDns::GetIpAddresses(this->localAddress, this->localPort, AF_UNSPEC, SOCK_DGRAM, IPPROTO_UDP, 0, localIpAddresses)) ? errorCode : CURLE_FAILED_INIT);
  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK && (this->sourceAddress != NULL), errorCode = SUCCEEDED(CDns::GetIpAddresses(this->sourceAddress, this->sourcePort, AF_UNSPEC, SOCK_DGRAM, IPPROTO_UDP, 0, sourceIpAddresses)) ? errorCode : CURLE_FAILED_INIT);

  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (localIpAddresses->Count() == 1) ? errorCode : CURLE_FAILED_INIT);
  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK && (this->sourceAddress != NULL), errorCode = (sourceIpAddresses->Count() == 1) ? errorCode : CURLE_FAILED_INIT);

  // local address collection must contain only one IP address - local unicast IP address or multicast address
  CUdpServer *server = NULL;

  if (errorCode == CURLE_OK)
  {
    server = localIpAddresses->GetItem(0)->IsMulticast() ? new CMulticastUdpServer() : new CUdpServer();
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (server != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);

    if ((errorCode == CURLE_OK) && (localIpAddresses->GetItem(0)->IsMulticast()))
    {
      CMulticastUdpServer *multicastServer = dynamic_cast<CMulticastUdpServer *>(server);

      CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = SUCCEEDED(multicastServer->Initialize(AF_UNSPEC, localIpAddresses->GetItem(0), (this->sourceAddress != NULL) ? sourceIpAddresses->GetItem(0) : NULL, this->networkInterfaces)) ? errorCode : CURLE_FAILED_INIT);
    }
    else if ((errorCode == CURLE_OK) && (!localIpAddresses->GetItem(0)->IsMulticast()))
    {
      CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = SUCCEEDED(server->Initialize(AF_UNSPEC, this->localPort, this->networkInterfaces)) ? errorCode : CURLE_FAILED_INIT);
    }
    else
    {
      errorCode = CURLE_FAILED_INIT;
    }
    
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = SUCCEEDED(server->StartListening()) ? errorCode : CURLE_FAILED_INIT);

    while (!this->curlWorkerShouldExit)
    {
      if (errorCode == CURLE_OK)
      {
        // only one thread can work with UDP data in one time
        CLockMutex lock(this->mutex, INFINITE);

        for (unsigned int i = 0; ((errorCode == CURLE_OK) && (i < server->GetServers()->Count())); i++)
        {
          CUdpSocketContext *udpContext = (CUdpSocketContext *)(server->GetServers()->GetItem(i));

          unsigned int pendingIncomingDataLength = 0;
          HRESULT result = S_OK;
          do
          {
            result = udpContext->GetPendingIncomingDataLength(&pendingIncomingDataLength);

            if (SUCCEEDED(result) && (pendingIncomingDataLength != 0))
            {
              // allocate buffer and receive data
              unsigned int receivedLength = 0;

              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = udpContext->Receive((char *)buffer, pendingIncomingDataLength, &receivedLength));
              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (pendingIncomingDataLength == receivedLength) ? result : E_NOT_VALID_STATE);

              if (SUCCEEDED(result))
              {
                if (this->IsSetFlags(UDP_CURL_INSTANCE_FLAG_TRANSPORT_RTP))
                {
                  rtpPacket->Clear();

                  if (rtpPacket->Parse(buffer, receivedLength))
                  {
                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize()) == rtpPacket->GetPayloadSize()) ? result : E_OUTOFMEMORY);
                  }
                }
                else if (this->IsSetFlags(UDP_CURL_INSTANCE_FLAG_TRANSPORT_UDP))
                {
                  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(buffer, pendingIncomingDataLength) == pendingIncomingDataLength) ? result : E_OUTOFMEMORY);
                }
                else
                {
                  // transport type is not resolved, try first RTP
                  rtpPacket->Clear();

                  if (rtpPacket->Parse(buffer, receivedLength))
                  {
                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize()) == rtpPacket->GetPayloadSize()) ? result : E_OUTOFMEMORY);
                    this->flags |= UDP_CURL_INSTANCE_FLAG_TRANSPORT_RTP;
                  }
                  else
                  {
                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(buffer, pendingIncomingDataLength) == pendingIncomingDataLength) ? result : E_OUTOFMEMORY);
                    this->flags |= UDP_CURL_INSTANCE_FLAG_TRANSPORT_UDP;
                  }
                }
              }
            }
          }
          while (SUCCEEDED(result) && (pendingIncomingDataLength != 0));

          if (FAILED(result))
          {
            this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, result);
            errorCode = CURLE_READ_ERROR;
          }
        }
      }



      //    if (endOfStreamReached && (this->state != CURL_STATE_RECEIVED_ALL_DATA) && (rtspRequest == NULL))
      //    {
      //      // we have end of stream on all tracks, we can't do more
      //      // report error code and wait for destroying CURL instance

      //      CLockMutex lock(this->mutex, INFINITE);

      //      this->rtspDownloadResponse->SetResultCode(errorCode);
      //      this->state = CURL_STATE_RECEIVED_ALL_DATA;
      //    }
      //  }

      if ((errorCode != CURLE_OK) && (this->state != CURL_STATE_RECEIVED_ALL_DATA))
      {
        // we have some error, we can't do more
        // report error code and wait for destroying CURL instance

        CLockMutex lock(this->mutex, INFINITE);

        this->udpDownloadResponse->SetResultCode(errorCode);
        this->state = CURL_STATE_RECEIVED_ALL_DATA;
      }

      Sleep(1);
    }
  }

  FREE_MEM_CLASS(localIpAddresses);
  FREE_MEM_CLASS(sourceIpAddresses);
  FREE_MEM_CLASS(rtpPacket);
  FREE_MEM(buffer);

  CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_CURL_WORKER_NAME, L"error while sending, receiving or processing data", errorCode));
  this->udpDownloadResponse->SetResultCode(errorCode);

  unsigned int count = 0;
  {
    CLockMutex lock(this->mutex, INFINITE);

    for (unsigned int i = 0; ((server != NULL) && (i < server->GetServers()->Count())); i++)
    {
      CSocketContext *context = server->GetServers()->GetItem(i);

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: address: %s, received bytes: %lld, sent bytes: %lld", this->protocolName, METHOD_CURL_WORKER_NAME, (context->GetIpAddress()->GetAddressString() == NULL) ? L"unknown" : context->GetIpAddress()->GetAddressString(), context->GetReceivedDataLength(), context->GetSentDataLength());
    }
  }

  this->state = CURL_STATE_RECEIVED_ALL_DATA;
  this->stopReceivingTicks = GetTickCount();

  FREE_MEM_CLASS(server);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->protocolName, METHOD_CURL_WORKER_NAME);
  return S_OK;
}

bool CUdpCurlInstance::IsSetFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}