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
#include "ErrorCodes.h"

CUdpCurlInstance::CUdpCurlInstance(HRESULT *result, CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(result, logger, mutex, protocolName, instanceName)
{
  this->localAddress = NULL;
  this->sourceAddress = NULL;
  this->localPort = PORT_UNSPECIFIED;
  this->sourcePort = PORT_UNSPECIFIED;

  this->udpDownloadRequest = dynamic_cast<CUdpDownloadRequest *>(this->downloadRequest);
  this->udpDownloadResponse = dynamic_cast<CUdpDownloadResponse *>(this->downloadResponse);

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
  }
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

HRESULT CUdpCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  HRESULT result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->udpDownloadRequest = dynamic_cast<CUdpDownloadRequest  *>(this->downloadRequest);
  this->udpDownloadResponse = dynamic_cast<CUdpDownloadResponse *>(this->downloadResponse);
  CHECK_POINTER_HRESULT(result, this->udpDownloadRequest, result, E_NOT_VALID_STATE);
  CHECK_POINTER_HRESULT(result, this->udpDownloadResponse, result, E_NOT_VALID_STATE);

  if (SUCCEEDED(result))
  {
    unsigned int endTicks = (this->downloadRequest->GetFinishTime() == FINISH_TIME_NOT_SPECIFIED) ? (GetTickCount() + this->downloadRequest->GetReceiveDataTimeout()) : this->downloadRequest->GetFinishTime();

    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
    CHECK_POINTER_HRESULT(result, urlComponents, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      ZeroURL(urlComponents);
      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

      CHECK_CONDITION_HRESULT(result, InternetCrackUrl(downloadRequest->GetUrl(), 0, 0, urlComponents) == TRUE, result, E_FAIL);

      if (SUCCEEDED(result))
      {
        this->localAddress = Substring(urlComponents->lpszHostName, 0, urlComponents->dwHostNameLength);
        CHECK_POINTER_HRESULT(result, this->localAddress, result, E_OUTOFMEMORY);

        this->localPort = urlComponents->nPort;

        if (urlComponents->dwUserNameLength > 0)
        {
          this->sourceAddress = Substring(urlComponents->lpszUserName, 0, urlComponents->dwUserNameLength);
          CHECK_POINTER_HRESULT(result, this->sourceAddress, result, E_OUTOFMEMORY);
        }

        if (urlComponents->dwPasswordLength > 0)
        {
          // its port for source address
          this->sourcePort = GetValueUnsignedInt(urlComponents->lpszPassword, PORT_UNSPECIFIED);
        }

        if (SUCCEEDED(result))
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

  this->state = SUCCEEDED(result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

/* protected methods */

CDownloadResponse *CUdpCurlInstance::CreateDownloadResponse(void)
{
  HRESULT result = S_OK;
  CUdpDownloadResponse *response = new CUdpDownloadResponse(&result);
  CHECK_POINTER_HRESULT(result, response, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(response));
  return response;
}

unsigned int CUdpCurlInstance::CurlWorker(void)
{
  this->logger->Log(LOGGER_INFO, L"%s: %s: Start, url: '%s'", this->protocolName, METHOD_CURL_WORKER_NAME, this->downloadRequest->GetUrl());
  this->startReceivingTicks = GetTickCount();
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;

  HRESULT result = S_OK;
  ALLOC_MEM_DEFINE_SET(buffer, unsigned char, BUFFER_LENGTH_DEFAULT, 0);
  CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

  // local address can be address of some network interface, can be localhost or can be mutlicast address
  // check if local address is mutlicast or unicast address

  CIpAddressCollection *localIpAddresses = new CIpAddressCollection(&result);
  CIpAddressCollection *sourceIpAddresses = new CIpAddressCollection(&result);
  CRtpPacket *rtpPacket = new CRtpPacket(&result);

  CHECK_POINTER_HRESULT(result, localIpAddresses, result, E_OUTOFMEMORY);
  CHECK_POINTER_HRESULT(result, sourceIpAddresses, result, E_OUTOFMEMORY);
  CHECK_POINTER_HRESULT(result, rtpPacket, result, E_OUTOFMEMORY);

  // local address collection must contain only one IP address - local unicast IP address or multicast address
  CUdpServer *server = NULL;

  if (SUCCEEDED(result))
  {
    result = CDns::GetIpAddresses(this->localAddress, this->localPort, AF_UNSPEC, SOCK_DGRAM, IPPROTO_UDP, 0, localIpAddresses);
    CHECK_CONDITION_HRESULT(result, localIpAddresses->Count() == 1, result, E_FAIL);

    if (SUCCEEDED(result) && (this->sourceAddress != NULL))
    {
      result = CDns::GetIpAddresses(this->sourceAddress, this->sourcePort, AF_UNSPEC, SOCK_DGRAM, IPPROTO_UDP, 0, sourceIpAddresses);
      CHECK_CONDITION_HRESULT(result, sourceIpAddresses->Count() == 1, result, E_FAIL);
    }

    if (SUCCEEDED(result))
    {
      CIpAddress *localIpAddress = localIpAddresses->GetItem(0);

      server = localIpAddress->IsMulticast() ? new CMulticastUdpServer(&result) : new CUdpServer(&result);
      CHECK_POINTER_HRESULT(result, server, result, E_OUTOFMEMORY);

      CNetworkInterfaceCollection *interfaces = new CNetworkInterfaceCollection(&result);
      CHECK_POINTER_HRESULT(result, interfaces, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), CNetworkInterface::GetAllNetworkInterfaces(interfaces, AF_UNSPEC), result);

      if (SUCCEEDED(result) && (localIpAddress->IsMulticast()))
      {
        CMulticastUdpServer *multicastServer = dynamic_cast<CMulticastUdpServer *>(server);

        result = multicastServer->Initialize(AF_UNSPEC, localIpAddress, (this->sourceAddress != NULL) ? sourceIpAddresses->GetItem(0) : NULL, interfaces);
      }
      else if (SUCCEEDED(result) && (!localIpAddress->IsMulticast()))
      {
        // if not multicast address, then binding to local address
        // we need to find correct network interface (with same IP address) and bind to it

        if (SUCCEEDED(result))
        {
          unsigned int i = 0;
          while (SUCCEEDED(result) && (i < interfaces->Count()))
          {
            CNetworkInterface *networkInterface = interfaces->GetItem(i);

            unsigned int j = 0;
            while (SUCCEEDED(result) && (j < networkInterface->GetUnicastAddresses()->Count()))
            {
              CIpAddress *ipAddress = networkInterface->GetUnicastAddresses()->GetItem(j);

              if (SUCCEEDED(result))
              {
                ipAddress->SetPort(localIpAddress->GetPort());

                if ((ipAddress->GetAddressLength() == localIpAddress->GetAddressLength()) &&
                  (memcmp(ipAddress->GetAddress(), localIpAddress->GetAddress(), ipAddress->GetAddressLength()) == 0))
                {
                  j++;
                }
                else
                {
                  networkInterface->GetUnicastAddresses()->Remove(j);
                }
              }
            }

            if (SUCCEEDED(result))
            {
              if (networkInterface->GetUnicastAddresses()->Count() != 0)
              {
                i++;
              }
              else
              {
                interfaces->Remove(i);
              }
            }
          }
        }

        CHECK_CONDITION_HRESULT(result, interfaces->Count() != 0, result, E_FAIL);
        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->Initialize(AF_UNSPEC, this->localPort, interfaces), result);
      }
      else
      {
        result = E_FAIL;
      }

      FREE_MEM_CLASS(interfaces);
      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->StartListening(), result);

      while (!this->curlWorkerShouldExit)
      {
        if (SUCCEEDED(result))
        {
          // only one thread can work with UDP data in one time
          CLockMutex lock(this->mutex, INFINITE);

          for (unsigned int i = 0; (SUCCEEDED(result) && (i < server->GetServers()->Count())); i++)
          {
            CUdpSocketContext *udpContext = (CUdpSocketContext *)(server->GetServers()->GetItem(i));

            unsigned int pendingIncomingDataLength = 0;
            HRESULT res = S_OK;
            do
            {
              res = udpContext->GetPendingIncomingDataLength(&pendingIncomingDataLength);

              if (SUCCEEDED(res) && (pendingIncomingDataLength != 0))
              {
                // allocate buffer and receive data
                unsigned int receivedLength = 0;

                CHECK_CONDITION_EXECUTE(SUCCEEDED(res), res = udpContext->Receive((char *)buffer, pendingIncomingDataLength, &receivedLength));
                CHECK_CONDITION_HRESULT(res, pendingIncomingDataLength == receivedLength, res, E_NOT_VALID_STATE);

                if (SUCCEEDED(res))
                {
                  CDumpBox *dumpBox = NULL;
                  CHECK_CONDITION_NOT_NULL_EXECUTE(this->dumpFile->GetDumpFile(), dumpBox = this->CreateDumpBox());

                  if (dumpBox != NULL)
                  {
                    dumpBox->SetTimeWithLocalTime();
                    dumpBox->SetPayload(buffer, receivedLength);
                  }

                  CHECK_CONDITION_EXECUTE((dumpBox != NULL) && (!this->dumpFile->AddDumpBox(dumpBox)), FREE_MEM_CLASS(dumpBox));

                  if (this->IsSetFlags(UDP_CURL_INSTANCE_FLAG_TRANSPORT_RTP))
                  {
                    rtpPacket->Clear();

                    if (rtpPacket->Parse(buffer, receivedLength))
                    {
                      CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize()) == rtpPacket->GetPayloadSize(), res, E_OUTOFMEMORY);
                    }
                  }
                  else if (this->IsSetFlags(UDP_CURL_INSTANCE_FLAG_TRANSPORT_UDP))
                  {
                    CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(buffer, pendingIncomingDataLength) == pendingIncomingDataLength, res, E_OUTOFMEMORY);
                  }
                  else
                  {
                    // transport type is not resolved, try first RTP
                    rtpPacket->Clear();

                    if (rtpPacket->Parse(buffer, receivedLength))
                    {
                      CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize()) == rtpPacket->GetPayloadSize(), res, E_OUTOFMEMORY);
                      this->flags |= UDP_CURL_INSTANCE_FLAG_TRANSPORT_RTP;
                    }
                    else
                    {
                      CHECK_CONDITION_HRESULT(res, this->udpDownloadResponse->GetReceivedData()->AddToBufferWithResize(buffer, pendingIncomingDataLength) == pendingIncomingDataLength, res, E_OUTOFMEMORY);
                      this->flags |= UDP_CURL_INSTANCE_FLAG_TRANSPORT_UDP;
                    }
                  }
                }
              }
            }
            while (SUCCEEDED(res) && (pendingIncomingDataLength != 0));

            if (FAILED(res))
            {
              this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, res);
              result = res;
            }
          }
        }

        if (FAILED(result) && (this->state != CURL_STATE_RECEIVED_ALL_DATA))
        {
          // we have some error, we can't do more
          // report error code and wait for destroying CURL instance

          CLockMutex lock(this->mutex, INFINITE);

          this->udpDownloadResponse->SetResultError(result);
          this->state = CURL_STATE_RECEIVED_ALL_DATA;
        }

        Sleep(1);
      }
    }
  }

  FREE_MEM_CLASS(localIpAddresses);
  FREE_MEM_CLASS(sourceIpAddresses);
  FREE_MEM_CLASS(rtpPacket);
  FREE_MEM(buffer);

  CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: error while sending, receiving or processing data: 0x%08X", this->protocolName, METHOD_CURL_WORKER_NAME, result));
  this->udpDownloadResponse->SetResultError(result);

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