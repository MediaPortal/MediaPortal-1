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

#include "RtspCurlInstance.h"
#include "InterleavedDataPacket.h"
#include "ControlAttribute.h"
#include "formatUrl.h"
#include "RtpPacket.h"
#include "RtcpPacket.h"
#include "BaseRtpPacketFactory.h"
#include "LockMutex.h"
#include "BufferHelper.h"

#include "UdpServer.h"
#include "TcpServer.h"
#include "NetworkInterface.h"
#include "NetworkInterfaceCollection.h"
#include "UdpSocketContext.h"
#include "TcpSocketContext.h"

#include "RtspOptionsRequest.h"
#include "RtspDescribeRequest.h"
#include "RtspSetupRequest.h"
#include "RtspPlayRequest.h"
#include "RtspTeardownRequest.h"

#include "RtspResponse.h"
#include "RtspPublicResponseHeader.h"
#include "RtspContentTypeResponseHeader.h"
#include "RtspTransportResponseHeader.h"
#include "RtspContentBaseResponseHeader.h"
#include "RtspContentLocationResponseHeader.h"

#include "SenderReportRtcpPacket.h"
#include "ReceiverReportRtcpPacket.h"
#include "SourceDescriptionRtcpPacket.h"

#include "CanonicalEndPointSourceDescriptionItem.h"

#include "hex.h"

CRtspCurlInstance::CRtspCurlInstance(CLogger *logger, HANDLE mutex, const wchar_t *protocolName, const wchar_t *instanceName)
  : CCurlInstance(logger, mutex, protocolName, instanceName)
{
  this->rtspDownloadRequest = dynamic_cast<CRtspDownloadRequest *>(this->downloadRequest);
  this->rtspDownloadResponse = dynamic_cast<CRtspDownloadResponse *>(this->downloadResponse);

  this->lastCommand = UINT_MAX;
  this->requestCommand = UINT_MAX;
  this->requestCommandFinished = false;
  this->lastSequenceNumber = 1;
  this->sameConnectionTcpPreference = RTSP_SAME_CONNECTION_TCP_PREFERENCE_DEFAULT;
  this->multicastPreference = RTSP_MULTICAST_PREFERENCE_DEFAULT;
  this->udpPreference = RTSP_UDP_PREFERENCE_DEFAULT;
  this->tcpPreference = RTSP_TCP_PREFERENCE_DEFAULT;
  this->rtspClientPort = RTSP_CLIENT_PORT_DEFAULT;
  this->sessionId = NULL;

  this->SetWriteCallback(CRtspCurlInstance::CurlReceiveDataCallback, this);
}

CRtspCurlInstance::~CRtspCurlInstance(void)
{
  this->StopReceivingData();

  FREE_MEM(this->sessionId);
}

CURLcode CRtspCurlInstance::SendAndReceive(CRtspRequest *request, CURLcode errorCode, const wchar_t *rtspMethodName, const wchar_t *functionName)
{
  if (errorCode == CURLE_OK)
  {
    // check sequence number
    unsigned int requestSequenceNumber = request->GetSequenceNumber();

    if ((errorCode == CURLE_OK) && (requestSequenceNumber == RTSP_SEQUENCE_NUMBER_UNSPECIFIED))
    {
      // bad sequence number
      this->logger->Log(LOGGER_ERROR, L"%s: %s: not specified request sequence number", this->protocolName, functionName);
      errorCode = CURLE_RTSP_CSEQ_ERROR;
    }
  }

  if (errorCode == CURLE_OK)
  {
    if (errorCode == CURLE_OK)
    {
      // send RTSP request
      {
        CLockMutex lock(this->mutex, INFINITE);

        this->rtspDownloadResponse->ClearRtspRequestAndResponse();
        this->rtspDownloadResponse->SetRtspRequest(request);
        this->rtspDownloadResponse->SetResponseCode(RTSP_STATUS_CODE_UNSPECIFIED);
        this->rtspDownloadResponse->SetResultCode(CURLE_AGAIN);
      }
      errorCode = CURLE_AGAIN;
    }

    // wait for response
    while (errorCode == CURLE_AGAIN)
    {
      // give chance other threads to do something useful
      Sleep(1);

      {
        CLockMutex lock(this->mutex, INFINITE);

        errorCode = this->rtspDownloadResponse->GetResultCode();
      }
    }

    if (errorCode != CURLE_OK)
    {
      wchar_t *error = FormatString(L"error while sending RTSP %s", rtspMethodName);
      if (error != NULL)
      {
        this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, functionName, error, errorCode);
      }
      this->logger->Log(LOGGER_ERROR, L"%s: %s: error while sending RTSP %s: 0x%08X", this->protocolName, functionName, rtspMethodName, this->rtspDownloadResponse->GetResponseCode());
      FREE_MEM(error);
    }

    {
      CLockMutex lock(this->mutex, INFINITE);

      if ((errorCode == CURLE_OK) && (this->rtspDownloadResponse->GetRtspResponse()->IsEmpty()))
      {
        // this should not happen, we should always have response or error (maybe timeout which is also error)
        this->logger->Log(LOGGER_ERROR, L"%s: %s: no response for RTSP %s request", this->protocolName, functionName, rtspMethodName);
        errorCode = CURLE_RECV_ERROR;
      }

      if (errorCode == CURLE_OK)
      {
        // check existence of session ID or check session ID value (if exists)

        if (this->sessionId != NULL)
        {
          // check session ID if same
          errorCode = (CompareWithNull(this->sessionId, this->rtspDownloadResponse->GetRtspResponse()->GetSessionId()) == 0) ? errorCode : CURLE_RECV_ERROR;

          CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->logger->Log(LOGGER_ERROR, L"%s: %s: no session ID or bad session ID for RTSP %s request", this->protocolName, functionName, rtspMethodName));
        }
        else if (this->rtspDownloadResponse->GetRtspResponse()->GetSessionId() != NULL)
        {
          // new session ID, our session ID is NULL
          this->sessionId = Duplicate(this->rtspDownloadResponse->GetRtspResponse()->GetSessionId());
          errorCode = (this->sessionId != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

          CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->logger->Log(LOGGER_ERROR, L"%s: %s: cannot store session ID for RTSP %s request", this->protocolName, functionName, rtspMethodName));
        }
      }

      if (errorCode == CURLE_OK)
      {
        // check sequence number
        unsigned int requestSequenceNumber = request->GetSequenceNumber();
        unsigned int responseSequenceNumber = this->rtspDownloadResponse->GetRtspResponse()->GetSequenceNumber();

        if ((errorCode == CURLE_OK) && (responseSequenceNumber == RTSP_SEQUENCE_NUMBER_UNSPECIFIED))
        {
          // bad sequence number
          this->logger->Log(LOGGER_ERROR, L"%s: %s: not specified response sequence number", this->protocolName, functionName);
          errorCode = CURLE_RTSP_CSEQ_ERROR;
        }

        if ((errorCode == CURLE_OK) && (requestSequenceNumber != responseSequenceNumber))
        {
          // sequence numbers not equal
          this->logger->Log(LOGGER_ERROR, L"%s: %s: request (%u) and response (%u) sequence numbers not equal", this->protocolName, functionName, requestSequenceNumber, responseSequenceNumber);
          errorCode = CURLE_RTSP_CSEQ_ERROR;
        }
      }

      if ((errorCode == CURLE_OK) && (!this->rtspDownloadResponse->GetRtspResponse()->IsSuccess()))
      {
        CRtspResponse *response = this->rtspDownloadResponse->GetRtspResponse();
        this->logger->Log(LOGGER_ERROR, L"%s: %s: RTSP %s response status code not success: %u (%s)", this->protocolName, functionName, rtspMethodName, response->GetStatusCode(), (response->GetStatusReason() != NULL) ? response->GetStatusReason() : L"");
        errorCode = CURLE_RECV_ERROR;
      }
    }
  }

  return errorCode;
}

bool CRtspCurlInstance::Initialize(CDownloadRequest *downloadRequest)
{
  DWORD startTicks = GetTickCount();
  DWORD endTicks = startTicks + this->GetReceiveDataTimeout();

  bool result = __super::Initialize(downloadRequest);
  this->state = CURL_STATE_CREATED;

  this->lastSequenceNumber = 1;
  this->requestCommandFinished = false;
  this->rtspDownloadRequest = dynamic_cast<CRtspDownloadRequest  *>(this->downloadRequest);
  this->rtspDownloadResponse = dynamic_cast<CRtspDownloadResponse *>(this->downloadResponse);
  result &= (this->rtspDownloadRequest != NULL) && (this->rtspDownloadResponse != NULL);
  result &= ((this->rtspClientPort >= RTSP_CLIENT_PORT_MIN) && (this->rtspClientPort <= RTSP_CLIENT_PORT_MAX));

  if (result)
  {
    result &= (this->rtspDownloadResponse->GetRtspTracks() != NULL);
  }

  if (result)
  {
    // check tranport prefereces
    // they must have different values

    result &= (this->sameConnectionTcpPreference != this->multicastPreference);
    result &= (this->sameConnectionTcpPreference != this->udpPreference);
    result &= (this->sameConnectionTcpPreference != this->tcpPreference);
    result &= (this->multicastPreference != this->udpPreference);
    result &= (this->multicastPreference != this->tcpPreference);
    result &= (this->udpPreference != this->tcpPreference);
  }

  if (result)
  {
    this->rtspDownloadResponse->GetRtspTracks()->Clear();

    // create receiving data thread
    // this add current CURL handle to multi handle
    // we avoid problem closing opened socket and creating new one
    result &= this->StartReceivingData();
  }

  if (result)
  {
    CURLcode errorCode = CURLE_OK;

    if (errorCode == CURLE_OK)
    {
      // we have own RTSP implementation, using CURL only to connect, send and receive data

      CHECK_CONDITION_EXECUTE_RESULT(errorCode == CURLE_OK, curl_easy_setopt(this->curl, CURLOPT_CONNECT_ONLY, 1L), errorCode);
      CHECK_CONDITION_EXECUTE_RESULT(errorCode == CURLE_OK, curl_easy_perform(this->curl), errorCode);

      // if no error, we successfully connected to remote server

      if (errorCode == CURLE_OK)
      {
        CRtspOptionsRequest *options = new CRtspOptionsRequest();
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (options != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (options->SetUri(this->rtspDownloadRequest->GetUrl())) ? errorCode : CURLE_OUT_OF_MEMORY);
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, options->SetTimeout(this->GetReceiveDataTimeout()));
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, options->SetSequenceNumber(this->lastSequenceNumber++));

        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = this->SendAndReceive(options, errorCode, L"OPTIONS", METHOD_INITIALIZE_NAME));

        CRtspResponse *response = NULL;
        if (errorCode == CURLE_OK)
        {
          // check OPTIONS required methods
          // we need DESCRIBE, SETUP, PLAY and TEARDOWN methods
          
          {
            CLockMutex lock(this->mutex, INFINITE);

            response = this->rtspDownloadResponse->GetRtspResponse()->Clone();
            CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (response != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
          }

          if (errorCode == CURLE_OK)
          {
            CRtspPublicResponseHeader *publicHeader = (CRtspPublicResponseHeader *)response->GetResponseHeaders()->GetRtspHeader(RTSP_PUBLIC_RESPONSE_HEADER_TYPE);
            if (publicHeader == NULL)
            {
              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"no PUBLIC RTSP OPTIONS response header");
              errorCode = CURLE_FAILED_INIT;
            }

            if (errorCode == CURLE_OK)
            {
              if (!publicHeader->IsSetFlag(FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_DESCRIBE | FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_SETUP | FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_PLAY | FLAG_RTSP_PUBLIC_RESPONSE_HEADER_METHOD_TEARDOWN))
              {
                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"not all required methods (DESCRIBE, SETUP, PLAY and TEARDOWN) defined in PUBLIC RTSP OPTIONS response header");
                errorCode = CURLE_FAILED_INIT;
              }
            }
          }
        }

        FREE_MEM_CLASS(response);
        FREE_MEM_CLASS(options);
      }
      CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while sending RTSP OPTIONS", errorCode));
    }
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, this->lastCommand = CURL_RTSPREQ_OPTIONS);

    if (errorCode == CURLE_OK)
    {
      if (errorCode == CURLE_OK)
      {
        CRtspDescribeRequest *describe = new CRtspDescribeRequest();
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (describe != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (describe->SetUri(this->rtspDownloadRequest->GetUrl())) ? errorCode : CURLE_OUT_OF_MEMORY);
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, describe->SetTimeout(this->GetReceiveDataTimeout()));
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, describe->SetSequenceNumber(this->lastSequenceNumber++));

        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = this->SendAndReceive(describe, errorCode, L"DESCRIBE", METHOD_INITIALIZE_NAME));

        CRtspResponse *response = NULL;
        if (errorCode == CURLE_OK)
        {
          // check content length
          {
            CLockMutex lock(this->mutex, INFINITE);

            response = this->rtspDownloadResponse->GetRtspResponse()->Clone();
            CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (response != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
          }

          if (errorCode == CURLE_OK)
          {
            if (response->GetContentLength() == 0)
            {
              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"RTSP DESCRIBE response content length is zero");
              errorCode = CURLE_FAILED_INIT;
            }

            // check content type
            CRtspResponseHeader *contentTypeHeader = response->GetResponseHeaders()->GetRtspHeader(RTSP_CONTENT_TYPE_RESPONSE_HEADER_TYPE);
            if (contentTypeHeader == NULL)
            {
              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"RTSP DESCRIBE response content type not found");
              errorCode = CURLE_FAILED_INIT;
            }
            else
            {
              if (wcscmp(RTSP_DESCRIBE_CONTENT_TYPE, contentTypeHeader->GetValue()) != 0)
              {
                this->logger->Log(LOGGER_ERROR, L"%s: %s: content type header '%s' not allowed, required: '%s'", this->protocolName, METHOD_INITIALIZE_NAME, contentTypeHeader->GetValue(), RTSP_DESCRIBE_CONTENT_TYPE);
                errorCode = CURLE_FAILED_INIT;
              }
            }
          }
        }

        if (errorCode == CURLE_OK)
        {
          // check DESCRIBE response, it have to be valid SDP

          unsigned int bufferSize = response->GetContentLength() + 1;
          ALLOC_MEM_DEFINE_SET(buffer, char, bufferSize, 0);
          errorCode = (buffer != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

          if (errorCode == CURLE_OK)
          {
            memcpy(buffer, response->GetContent(), bufferSize - 1);

            // try to log response
            wchar_t *responseW = ConvertToUnicodeA(buffer);
            errorCode = (responseW != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

            if (errorCode == CURLE_OK)
            {
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: RTSP DESCRIBE response: %s", this->protocolName, METHOD_INITIALIZE_NAME, responseW);

              errorCode = this->rtspDownloadResponse->SetRawSessionDescription(responseW) ? errorCode : CURLE_FAILED_INIT;
              errorCode = this->rtspDownloadResponse->ParseRawSessionDescription() ? errorCode : CURLE_FAILED_INIT;
            }
            FREE_MEM(responseW);
          }
          FREE_MEM(buffer);
        }

        if (errorCode == CURLE_OK)
        {
          // check content base or content location headers
          CRtspContentBaseResponseHeader *contentBase = (CRtspContentBaseResponseHeader *)response->GetResponseHeaders()->GetRtspHeader(RTSP_CONTENT_BASE_RESPONSE_HEADER_TYPE);
          CRtspContentLocationResponseHeader *contentLocation = (CRtspContentLocationResponseHeader *)response->GetResponseHeaders()->GetRtspHeader(RTSP_CONTENT_LOCATION_RESPONSE_HEADER_TYPE);

          CHECK_CONDITION_NOT_NULL_EXECUTE(contentBase, this->rtspDownloadResponse->SetContentBaseUrl(contentBase->GetUri()));
          CHECK_CONDITION_NOT_NULL_EXECUTE(contentLocation, this->rtspDownloadResponse->SetContentLocationUrl(contentLocation->GetUri()));
        }

        FREE_MEM_CLASS(response);
        FREE_MEM_CLASS(describe);
      }
      CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while sending RTSP DESCRIBE", errorCode));
    }
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, this->lastCommand = CURL_RTSPREQ_DESCRIBE);

    // clear buffer
    this->rtspDownloadResponse->GetReceivedData()->ClearBuffer();

    // RTSP setup depends on:
    // 1. transport preference (same connection, multicast, udp, tcp)
    // 2. SDP session tags (session control, media control, ...)

    unsigned int transportPreference = min(this->sameConnectionTcpPreference, this->multicastPreference);
    transportPreference = min(transportPreference, this->udpPreference);
    transportPreference = min(transportPreference, this->tcpPreference);

    bool negotiatedTransport = false;

    while ((!negotiatedTransport) && (errorCode == CURLE_OK))
    {
      CURLcode error = CURLE_OK;

      if (this->sameConnectionTcpPreference == transportPreference)
      {
        // same connection UDP or TCP transports are very similar
        unsigned int interleavedChannel = 0;

        this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"trying to negotiate TCP transport on same connection");

        // for each media description create and negotiate UDP or TCP transport
        CURLcode error = (this->rtspDownloadResponse->GetSessionDescription()->GetMediaDescriptions()->Count() != 0) ? CURLE_OK : CURLE_FAILED_INIT;

        for (unsigned int i = 0; ((error == CURLE_OK) && (i < this->rtspDownloadResponse->GetSessionDescription()->GetMediaDescriptions()->Count())); i++)
        {
          CMediaDescription *mediaDescription = this->rtspDownloadResponse->GetSessionDescription()->GetMediaDescriptions()->GetItem(i);

          //bool supportedPayloadType = false;
          //for (unsigned int j = 0; ((error == CURLE_OK) && (!supportedPayloadType) && (j < TOTAL_SUPPORTED_PAYLOAD_TYPES)); j++)
          //{
          //  SupportedPayloadType payloadType = SUPPORTED_PAYLOAD_TYPES[j];

          //  if (mediaDescription->GetMediaFormats()->Count() == 1)
          //  {
          //    // there must be only one media format
          //    CMediaFormat *mediaFormat = mediaDescription->GetMediaFormats()->GetItem(0);

          //    if ((mediaFormat->GetPayloadType() == payloadType.payloadType) ||
          //        (CompareWithNull(mediaFormat->GetName(), payloadType.name) == 0))
          //    {
          //      supportedPayloadType = true;
          //    }
          //    else
          //    {
          //      this->logger->Log(LOGGER_WARNING, L"%s: %s: media description format not supported: %d, %s", this->protocolName, METHOD_INITIALIZE_NAME, mediaFormat->GetPayloadType(), (mediaFormat->GetName() == NULL) ? L"unknown" : mediaFormat->GetName());
          //    }
          //  }
          //  else
          //  {
          //    this->logger->Log(LOGGER_ERROR, L"%s: %s: media description specify more media formats as allowed: '%s'", this->protocolName, METHOD_INITIALIZE_NAME, mediaDescription->GetTagContent());
          //  }
          //}

          //if (!supportedPayloadType)
          //{
          //  continue;
          //}

          // find control attribute
          CAttributeCollection *attributes = mediaDescription->GetAttributes();
          CControlAttribute *control = NULL;
          for (unsigned int i = 0; ((control == NULL) && (i < attributes->Count())); i++)
          {
            CAttribute *attribute = attributes->GetItem(i);

            if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_CONTROL))
            {
              control = dynamic_cast<CControlAttribute *>(attribute);
            }
          }
          error = (control != NULL) ? error : CURLE_FAILED_INIT;

          if (error == CURLE_OK)
          {
            // control attribute exists
            // it can be asterisk ('*') - this means stream URI
            // it can be relative or absolute URL

            const wchar_t *controlUrl = control->IsAsterisk() ? this->GetBaseUrl() : control->GetControlUrl();
            wchar_t *streamUrl = NULL;
            // this controlUrl can be relative or absolute
            if (IsAbsoluteUrl(controlUrl))
            {
              streamUrl = Duplicate(controlUrl);
            }
            else
            {
              streamUrl = FormatAbsoluteUrl(this->GetBaseUrl(), control->GetControlUrl());
            }

            error = (streamUrl != NULL) ? error : CURLE_OUT_OF_MEMORY;

            if (error == CURLE_OK)
            {
              CRtspSetupRequest *setup = new CRtspSetupRequest();
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (setup != NULL) ? error : CURLE_OUT_OF_MEMORY);
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (setup->GetTransportRequestHeader() != NULL) ? error : CURLE_OUT_OF_MEMORY);
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (setup->SetUri(streamUrl)) ? error : CURLE_OUT_OF_MEMORY);
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, setup->SetTimeout(this->GetReceiveDataTimeout()));
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, setup->SetSequenceNumber(this->lastSequenceNumber++));
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = setup->SetSessionId(this->sessionId) ? error : CURLE_OUT_OF_MEMORY);

              if (errorCode == CURLE_OK)
              {
                // setup transort header
                CRtspTransportRequestHeader *header = setup->GetTransportRequestHeader();

                header->SetTransportProtocol(RTSP_TRANSPORT_REQUEST_HEADER_PROTOCOL_RTP);
                header->SetProfile(RTSP_TRANSPORT_REQUEST_HEADER_PROFILE_AVP);
                header->SetLowerTransport(RTSP_TRANSPORT_REQUEST_HEADER_LOWER_TRANSPORT_TCP);
                header->SetMinInterleavedChannel(interleavedChannel);
                header->SetMaxInterleavedChannel(interleavedChannel + 1);
                header->SetFlags(FLAG_RTSP_TRANSPORT_REQUEST_HEADER_LOWER_TRANSPORT_TCP | FLAG_RTSP_TRANSPORT_REQUEST_HEADER_INTERLEAVED | FLAG_RTSP_TRANSPORT_REQUEST_HEADER_TRANSPORT_PROTOCOL_RTP | FLAG_RTSP_TRANSPORT_REQUEST_HEADER_PROFILE_AVP);
              }

              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = this->SendAndReceive(setup, error, L"SETUP", METHOD_INITIALIZE_NAME));

              FREE_MEM_CLASS(setup);
            }

            CHECK_CONDITION_EXECUTE(error != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while sending same connection TCP RTSP SETUP", error));

            CRtspResponse *response = NULL;
            if (error == CURLE_OK)
            {
              {
                CLockMutex lock(this->mutex, INFINITE);

                response = this->rtspDownloadResponse->GetRtspResponse()->Clone();
                CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (response != NULL) ? error : CURLE_OUT_OF_MEMORY);
              }

              if (error == CURLE_OK)
              {
                CRtspTransportResponseHeader *transport = (CRtspTransportResponseHeader *)response->GetResponseHeaders()->GetRtspHeader(RTSP_TRANSPORT_RESPONSE_HEADER_TYPE);

                if (transport == NULL)
                {
                  // no transport header in RTSP response
                  this->logger->Log(LOGGER_ERROR, L"%s: %s: no transport header in response", this->protocolName, METHOD_INITIALIZE_NAME);
                  error = CURLE_FAILED_INIT;
                }

                if ((error == CURLE_OK) && (!transport->IsLowerTransportTCP()))
                {
                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_INITIALIZE_NAME, L"no TCP transport header");
                  error = CURLE_FAILED_INIT;
                }

                if (error == CURLE_OK)
                {
                  if (!transport->IsInterleaved())
                  {
                    // client and server ports are specified, not interleaved transport
                    // only interleaved transport supported

                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_INITIALIZE_NAME, L"same connection transport not supported by server");
                    error = CURLE_FAILED_INIT;
                  }
                  else if (transport->IsInterleaved() && (!transport->IsClientPort()) && (!transport->IsServerPort()))
                  {
                    // if not set client and server ports, then transport must be interleaved
                  }
                  else
                  {
                    // bad or not implemented transport
                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_INITIALIZE_NAME, L"bad or not implemented transport");
                    error = CURLE_FAILED_INIT;
                  }
                }

                if (error == CURLE_OK)
                {
                  CRtspTrack *track = new CRtspTrack();
                  error = (track != NULL) ? error : CURLE_OUT_OF_MEMORY;

                  CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (track->SetTrackUrl(streamUrl)) ? error : CURLE_OUT_OF_MEMORY);
                  CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (track->SetTransportResponseHeader(transport)) ? error : CURLE_OUT_OF_MEMORY);

                  if (error == CURLE_OK)
                  {
                    long serverPort = 0;
                    long clientPort = 0;

                    CHECK_CONDITION_EXECUTE_RESULT(error == CURLE_OK, curl_easy_getinfo(this->curl, CURLINFO_PRIMARY_PORT, &serverPort), error);
                    CHECK_CONDITION_EXECUTE_RESULT(error == CURLE_OK, curl_easy_getinfo(this->curl, CURLINFO_LOCAL_PORT, &clientPort), error);

                    if (error == CURLE_OK)
                    {
                      track->SetClientDataPort(clientPort);
                      track->SetClientControlPort(clientPort);
                      track->SetServerDataPort(serverPort);
                      track->SetServerControlPort(serverPort);

                      track->SetLastReceiverReportTime(GetTickCount());
                      track->SetReceiverReportInterval(RECEIVER_REPORT_MIN_TIME);
                    }
                  }

                  CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = this->rtspDownloadResponse->GetRtspTracks()->Add(track) ? error : CURLE_OUT_OF_MEMORY);
                  CHECK_CONDITION_EXECUTE(error != CURLE_OK, FREE_MEM_CLASS(track));
                }
              }
            }

            FREE_MEM_CLASS(response);
            FREE_MEM(streamUrl);
          }

          interleavedChannel += 2;
        }

        CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (this->rtspDownloadResponse->GetRtspTracks()->Count() != 0) ? error : CURLE_FAILED_INIT);
        CHECK_CONDITION_EXECUTE(error == CURLE_OK, negotiatedTransport = true);
      }
      
      if (this->multicastPreference == transportPreference)
      {
        this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"trying to negotiate multicast transport");

        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"multicast transport not implemented");
      }

      if ((this->udpPreference == transportPreference) || (this->tcpPreference == transportPreference))
      {
        // UDP or TCP transports are very similar

        this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, (this->udpPreference == transportPreference) ? L"trying to negotiate udp transport" : L"trying to negotiate tcp transport");

        // for each media description create and negotiate UDP or TCP transport
        unsigned int clientPort = this->rtspClientPort;
        CURLcode error = (this->rtspDownloadResponse->GetSessionDescription()->GetMediaDescriptions()->Count() != 0) ? CURLE_OK : CURLE_FAILED_INIT;

        for (unsigned int i = 0; ((error == CURLE_OK) && (i < this->rtspDownloadResponse->GetSessionDescription()->GetMediaDescriptions()->Count())); i++)
        {
          CMediaDescription *mediaDescription = this->rtspDownloadResponse->GetSessionDescription()->GetMediaDescriptions()->GetItem(i);

          //bool supportedPayloadType = false;
          //for (unsigned int j = 0; ((error == CURLE_OK) && (!supportedPayloadType) && (j < TOTAL_SUPPORTED_PAYLOAD_TYPES)); j++)
          //{
          //  SupportedPayloadType payloadType = SUPPORTED_PAYLOAD_TYPES[j];

          //  if (mediaDescription->GetMediaFormats()->Count() == 1)
          //  {
          //    // there must be only one media format
          //    CMediaFormat *mediaFormat = mediaDescription->GetMediaFormats()->GetItem(0);

          //    if ((mediaFormat->GetPayloadType() == payloadType.payloadType) ||
          //        (CompareWithNull(mediaFormat->GetName(), payloadType.name) == 0))
          //    {
          //      supportedPayloadType = true;
          //    }
          //    else
          //    {
          //      this->logger->Log(LOGGER_WARNING, L"%s: %s: media description format not supported: %d, %s", this->protocolName, METHOD_INITIALIZE_NAME, mediaFormat->GetPayloadType(), (mediaFormat->GetName() == NULL) ? L"unknown" : mediaFormat->GetName());
          //    }
          //  }
          //  else
          //  {
          //    this->logger->Log(LOGGER_ERROR, L"%s: %s: media description specify more media formats as allowed: '%s'", this->protocolName, METHOD_INITIALIZE_NAME, mediaDescription->GetTagContent());
          //  }
          //}

          //if (!supportedPayloadType)
          //{
          //  continue;
          //}

          // find control attribute
          CAttributeCollection *attributes = mediaDescription->GetAttributes();
          CControlAttribute *control = NULL;
          for (unsigned int i = 0; ((control == NULL) && (i < attributes->Count())); i++)
          {
            CAttribute *attribute = attributes->GetItem(i);

            if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_CONTROL))
            {
              control = dynamic_cast<CControlAttribute *>(attribute);
            }
          }
          error = (control != NULL) ? error : CURLE_FAILED_INIT;

          if (error == CURLE_OK)
          {
            // control attribute exists
            // it can be asterisk ('*') - this means stream URI
            // it can be relative or absolute URL

            const wchar_t *controlUrl = control->IsAsterisk() ? this->GetBaseUrl() : control->GetControlUrl();
            wchar_t *streamUrl = NULL;
            // this controlUrl can be relative or absolute
            if (IsAbsoluteUrl(controlUrl))
            {
              streamUrl = Duplicate(controlUrl);
            }
            else
            {
              streamUrl = FormatAbsoluteUrl(this->GetBaseUrl(), control->GetControlUrl());
            }

            error = (streamUrl != NULL) ? error : CURLE_OUT_OF_MEMORY;

            CSimpleServer *dataServer = NULL;
            CSimpleServer *controlServer = NULL;

            do
            {
              error = CURLE_OK;

              if (this->udpPreference == transportPreference)
              {
                dataServer = new CUdpServer();
                controlServer = new CUdpServer();
              }
              else
              {
                // TCP transport not tried !!!

                //dataServer = new CTcpServer();
                //controlServer = new CTcpServer();
              }

              error = ((dataServer != NULL) && (controlServer != NULL)) ? error : CURLE_OUT_OF_MEMORY;

              if (error == CURLE_OK)
              {
                error = SUCCEEDED(dataServer->Initialize(AF_UNSPEC, clientPort)) ? error : CURLE_FAILED_INIT;
                error = SUCCEEDED(controlServer->Initialize(AF_UNSPEC, clientPort + 1)) ? error : CURLE_FAILED_INIT;
              }

              if (error == CURLE_OK)
              {
                error = SUCCEEDED(dataServer->StartListening()) ? error : CURLE_FAILED_INIT;
                error = SUCCEEDED(controlServer->StartListening()) ? error : CURLE_FAILED_INIT;
              }

              if (error != CURLE_OK)
              {
                FREE_MEM_CLASS(dataServer);
                FREE_MEM_CLASS(controlServer);

                clientPort++;
              }
            }
            while ((error != CURLE_OK) && (clientPort < RTSP_CLIENT_PORT_MAX));

            if (error == CURLE_OK)
            {
              CRtspSetupRequest *setup = new CRtspSetupRequest();
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (setup != NULL) ? error : CURLE_OUT_OF_MEMORY);
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (setup->GetTransportRequestHeader() != NULL) ? error : CURLE_OUT_OF_MEMORY);
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (setup->SetUri(streamUrl)) ? error : CURLE_OUT_OF_MEMORY);
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, setup->SetTimeout(this->GetReceiveDataTimeout()));
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, setup->SetSequenceNumber(this->lastSequenceNumber++));
              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = setup->SetSessionId(this->sessionId) ? error : CURLE_OUT_OF_MEMORY);

              if (error == CURLE_OK)
              {
                // setup transort header
                CRtspTransportRequestHeader *header = setup->GetTransportRequestHeader();

                header->SetTransportProtocol(RTSP_TRANSPORT_REQUEST_HEADER_PROTOCOL_RTP);
                header->SetProfile(RTSP_TRANSPORT_REQUEST_HEADER_PROFILE_AVP);
                header->SetMinClientPort(clientPort);
                header->SetMaxClientPort(clientPort + 1);
                header->SetFlags(FLAG_RTSP_TRANSPORT_REQUEST_HEADER_UNICAST | FLAG_RTSP_TRANSPORT_REQUEST_HEADER_TRANSPORT_PROTOCOL_RTP | FLAG_RTSP_TRANSPORT_REQUEST_HEADER_PROFILE_AVP | FLAG_RTSP_TRANSPORT_REQUEST_HEADER_CLIENT_PORT);

                if (this->tcpPreference == transportPreference)
                {
                  header->SetLowerTransport(RTSP_TRANSPORT_REQUEST_HEADER_LOWER_TRANSPORT_TCP);
                  header->SetFlags(header->GetFlags() | FLAG_RTSP_TRANSPORT_REQUEST_HEADER_LOWER_TRANSPORT_TCP);
                }
              }

              CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = this->SendAndReceive(setup, error, L"SETUP", METHOD_INITIALIZE_NAME));

              FREE_MEM_CLASS(setup);
            }

            CHECK_CONDITION_EXECUTE(error != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, (this->udpPreference == transportPreference) ? L"error while sending UDP RTSP SETUP" : L"error while sending TCP RTSP SETUP", error));

            CRtspResponse *response = NULL;
            if (error == CURLE_OK)
            {
              {
                CLockMutex lock(this->mutex, INFINITE);

                response = this->rtspDownloadResponse->GetRtspResponse()->Clone();
                CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (response != NULL) ? error : CURLE_OUT_OF_MEMORY);
              }

              if (error == CURLE_OK)
              {
                CRtspTransportResponseHeader *transport = (CRtspTransportResponseHeader *)response->GetResponseHeaders()->GetRtspHeader(RTSP_TRANSPORT_RESPONSE_HEADER_TYPE);

                if (transport == NULL)
                {
                  // no transport header in RTSP response
                  this->logger->Log(LOGGER_ERROR, L"%s: %s: no transport header in response", this->protocolName, METHOD_INITIALIZE_NAME);
                  error = CURLE_FAILED_INIT;
                }

                if (error == CURLE_OK)
                {
                  if (this->udpPreference == transportPreference)
                  {
                    if (!(transport->IsLowerTransportUDP() || (transport->GetLowerTransport() == NULL)))
                    {
                      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_INITIALIZE_NAME, L"no UDP transport header");
                      error = CURLE_FAILED_INIT;
                    }
                  }
                  else
                  {
                    if (!transport->IsLowerTransportTCP())
                    {
                      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_INITIALIZE_NAME, L"no TCP transport header");
                      error = CURLE_FAILED_INIT;
                    }
                  }
                }

                if (error == CURLE_OK)
                {
                  CRtspTrack *track = new CRtspTrack();
                  error = (track != NULL) ? error : CURLE_OUT_OF_MEMORY;

                  CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (track->SetTrackUrl(streamUrl)) ? error : CURLE_OUT_OF_MEMORY);
                  CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (track->SetTransportResponseHeader(transport)) ? error : CURLE_OUT_OF_MEMORY);

                  if (error == CURLE_OK)
                  {
                    if ((!transport->IsInterleaved()) && transport->IsClientPort() && transport->IsServerPort())
                    {
                      // client and server ports are specified, not interleaved transport

                      if ((transport->GetMinClientPort() == clientPort) && (transport->GetMaxClientPort() == (clientPort + 1)))
                      {
                        track->SetClientDataPort(transport->GetMinClientPort());
                        track->SetClientControlPort(transport->GetMaxClientPort());
                        track->SetServerDataPort(transport->GetMinServerPort());
                        track->SetServerControlPort(transport->GetMaxServerPort());

                        track->SetLastReceiverReportTime(GetTickCount());
                        track->SetReceiverReportInterval(RECEIVER_REPORT_MIN_TIME);

                        // set data and control server to track
                        track->SetDataServer(dataServer);
                        track->SetControlServer(controlServer);

                        // in case of another error will be data server and control server destroyed
                        // we must avoid this, because data server and control server are destroyed with track instance
                        dataServer = NULL;
                        controlServer = NULL;
                      }
                      else
                      {
                        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_INITIALIZE_NAME, L"client ports not same as requested");
                        error = CURLE_FAILED_INIT;
                      }
                    }
                    else if (transport->IsInterleaved() && (!transport->IsClientPort()) && (!transport->IsServerPort()))
                    {
                      // if not set client and server ports, then transport must be interleaved
                      long serverPort = 0;
                      long clientPort = 0;

                      CHECK_CONDITION_EXECUTE_RESULT(error == CURLE_OK, curl_easy_getinfo(this->curl, CURLINFO_PRIMARY_PORT, &serverPort), error);
                      CHECK_CONDITION_EXECUTE_RESULT(error == CURLE_OK, curl_easy_getinfo(this->curl, CURLINFO_LOCAL_PORT, &clientPort), error);

                      if (error == CURLE_OK)
                      {
                        track->SetClientDataPort(clientPort);
                        track->SetClientControlPort(clientPort);
                        track->SetServerDataPort(serverPort);
                        track->SetServerControlPort(serverPort);

                        track->SetLastReceiverReportTime(GetTickCount());
                        track->SetReceiverReportInterval(RECEIVER_REPORT_MIN_TIME);
                      }

                      // data and control servers are not needed, destroy them
                      FREE_MEM_CLASS(dataServer);
                      FREE_MEM_CLASS(controlServer);
                    }
                    else
                    {
                      // bad or not implemented transport
                      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_INITIALIZE_NAME, L"bad or not implemented transport");
                      error = CURLE_FAILED_INIT;
                    }
                  }

                  CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = this->rtspDownloadResponse->GetRtspTracks()->Add(track) ? error : CURLE_OUT_OF_MEMORY);
                  CHECK_CONDITION_EXECUTE(error != CURLE_OK, FREE_MEM_CLASS(track));
                }

                clientPort += 2;
              }
            }

            if (error != CURLE_OK)
            {
              FREE_MEM_CLASS(dataServer);
              FREE_MEM_CLASS(controlServer);
            }

            FREE_MEM_CLASS(response);
            FREE_MEM(streamUrl);
          }
        }

        CHECK_CONDITION_EXECUTE(error == CURLE_OK, error = (this->rtspDownloadResponse->GetRtspTracks()->Count() != 0) ? error : CURLE_FAILED_INIT);
        CHECK_CONDITION_EXECUTE(error == CURLE_OK, negotiatedTransport = true);
      }

      if ((!negotiatedTransport) && (errorCode == CURLE_OK))
      {
        this->rtspDownloadResponse->GetRtspTracks()->Clear();

        // select next transport by preference
        unsigned int temp = UINT_MAX;

        CHECK_CONDITION_EXECUTE_RESULT(this->sameConnectionTcpPreference > transportPreference, min(temp, this->sameConnectionTcpPreference), temp);
        CHECK_CONDITION_EXECUTE_RESULT(this->multicastPreference > transportPreference, min(temp, this->multicastPreference), temp);
        CHECK_CONDITION_EXECUTE_RESULT(this->udpPreference > transportPreference, min(temp, this->udpPreference), temp);
        CHECK_CONDITION_EXECUTE_RESULT(this->tcpPreference > transportPreference, min(temp, this->tcpPreference), temp);

        transportPreference = temp;

        // if transport preference is UINT_MAX, then no other transport is available and no negotiated transport exists
        // in that case we are finished with error
        errorCode = (transportPreference != UINT_MAX) ? errorCode : CURLE_FAILED_INIT;

        if (errorCode != CURLE_OK)
        {
          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"can't negotiate any transport");
        }
      }
    }
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, this->lastCommand = CURL_RTSPREQ_SETUP);
    
    if (errorCode == CURLE_OK)
    {
      errorCode = (this->rtspDownloadResponse->GetRtspTracks()->Count() != 0) ? errorCode : CURLE_FAILED_INIT;
      CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, this->protocolName, METHOD_INITIALIZE_NAME, L"no RTSP track to play"));
    }

    if (errorCode == CURLE_OK)
    {
      if (errorCode == CURLE_OK)
      {
        CRtspPlayRequest *play = new CRtspPlayRequest();
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (play != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (play->SetUri(this->rtspDownloadRequest->GetUrl())) ? errorCode : CURLE_OUT_OF_MEMORY);
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, play->SetTimeout(this->GetReceiveDataTimeout()));
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, play->SetSequenceNumber(this->lastSequenceNumber++));
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = play->SetSessionId(this->sessionId) ? errorCode : CURLE_OUT_OF_MEMORY);
        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = play->SetStartTime(0) ? errorCode : CURLE_FAILED_INIT);

        CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = this->SendAndReceive(play, errorCode, L"PLAY", METHOD_INITIALIZE_NAME));

        FREE_MEM_CLASS(play);
      }
      CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_INITIALIZE_NAME, L"error while sending RTSP PLAY", errorCode));
    }
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, this->lastCommand = CURL_RTSPREQ_PLAY);

    result &= (errorCode == CURLE_OK);
  }

  this->state = (result) ? CURL_STATE_INITIALIZED : CURL_STATE_CREATED;
  return result;
}

CRtspDownloadResponse *CRtspCurlInstance::GetRtspDownloadResponse(void)
{
  return this->rtspDownloadResponse;
}

CDownloadResponse *CRtspCurlInstance::GetNewDownloadResponse(void)
{
  return new CRtspDownloadResponse();
}

bool CRtspCurlInstance::ProcessReceivedBaseRtpPackets(CRtspTrack *track, unsigned int clientPort, CBaseRtpPacketCollection *packets)
{
  // only one thread can work with RTSP tracks and responses in one time
  CLockMutex lock(this->mutex, INFINITE);

  bool result = ((track != NULL) && (packets != NULL) && (this->rtspDownloadResponse != NULL));

  if (result)
  {
    result &= (packets->Count() != 0);
    result &= (this->rtspDownloadResponse->GetRtspTracks()->Count() != 0);
  }

  if (result)
  {
    bool isDataPort = track->IsClientDataPort(clientPort);
    bool isControlPort = track->IsClientControlPort(clientPort);

    result &= (isDataPort || isControlPort);

    for (unsigned int i = 0; (result && (i < packets->Count())); i++)
    {
      CBaseRtpPacket *packet = packets->GetItem(i);
      CRtpPacket *rtpPacket = dynamic_cast<CRtpPacket *>(packet);
      CRtcpPacket *rtcpPacket = dynamic_cast<CRtcpPacket *>(packet);

      result &= ((rtpPacket != NULL) || (rtcpPacket != NULL));
      result &= ((isDataPort && (rtpPacket != NULL)) || (rtpPacket == NULL));
      result &= ((isControlPort && (rtcpPacket != NULL)) || (rtcpPacket == NULL));

      if (rtcpPacket != NULL)
      {
        // handle control packet

        CSenderReportRtcpPacket *senderReport = dynamic_cast<CSenderReportRtcpPacket *>(rtcpPacket);

        if (senderReport != NULL)
        {
          track->SetSenderSynchronizationSourceIdentifier(senderReport->GetSenderSynchronizationSourceIdentifier());
        }
      }
      else
      {
        // in case of RTP packet
        // add payload to track received data
        result &= (track->GetDownloadResponse()->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize(), track->GetDownloadResponse()->GetReceivedData()->GetBufferSize() * 2) == rtpPacket->GetPayloadSize());
      }
    }
  }

  return result;
}

const wchar_t *CRtspCurlInstance::GetBaseUrl(void)
{
  // Implementations should look for a base URL in the following order:
  // 1.     The RTSP Content-Base field
  // 2.     The RTSP Content-Location field
  // 3.     The RTSP request URL 

  const wchar_t *result = NULL;

  if ((result == NULL) && (this->rtspDownloadResponse != NULL) && (this->rtspDownloadResponse->GetContentBaseUrl() != NULL))
  {
    result = this->rtspDownloadResponse->GetContentBaseUrl();
  }

  if ((result == NULL) && (this->rtspDownloadResponse != NULL) && (this->rtspDownloadResponse->GetContentLocationUrl() != NULL))
  {
    result = this->rtspDownloadResponse->GetContentLocationUrl();
  }

  if ((result == NULL) && (this->rtspDownloadRequest != NULL))
  {
    result = this->rtspDownloadRequest->GetUrl();
  }

  return result;
}

DWORD CRtspCurlInstance::CurlWorker(void)
{
  this->logger->Log(LOGGER_INFO, L"%s: %s: Start, url: '%s'", this->protocolName, METHOD_CURL_WORKER_NAME, this->downloadRequest->GetUrl());
  this->startReceivingTicks = GetTickCount();
  this->stopReceivingTicks = 0;
  this->totalReceivedBytes = 0;

  CURLcode errorCode = CURLE_OK;
  ALLOC_MEM_DEFINE_SET(buffer, unsigned char, BUFFER_LENGTH_DEFAULT, 0);
  CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (buffer != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
  int readData = 0;

  DWORD startTicks = 0;
  DWORD endTicks = 0;

  // RTSP request and response
  CRtspRequest *request = NULL;
  CRtspResponse *response = NULL;

  while (!this->curlWorkerShouldExit)
  {
    // RTSP request and response must be NULL to start with next request/response pair

    if ((request == NULL) && (response == NULL))
    {
      CLockMutex lock(this->mutex, 10);

      if (lock.IsLocked())
      {
        if ((this->rtspDownloadResponse->GetRtspRequest() != NULL) && (this->rtspDownloadResponse->GetRtspResponse() == NULL) && (this->rtspDownloadResponse->GetResultCode() == CURLE_AGAIN))
        {
          // it is prepared new RTSP request
          // we must send it to remote server and create new response

          request = this->rtspDownloadResponse->GetRtspRequest()->Clone();
          response = new CRtspResponse();

          CHECK_CONDITION_EXECUTE(request == NULL, this->rtspDownloadResponse->SetResultCode(CURLE_OUT_OF_MEMORY));
          CHECK_CONDITION_EXECUTE(response == NULL, this->rtspDownloadResponse->SetResultCode(CURLE_OUT_OF_MEMORY));

          if (this->rtspDownloadResponse->GetResultCode() == CURLE_AGAIN)
          {
            // everything is OK, we can continue
            startTicks = GetTickCount();
            endTicks = startTicks + request->GetTimeout();

            const wchar_t *requestW = request->GetRequest();
            CHECK_CONDITION_EXECUTE(requestW != NULL, this->logger->Log(LOGGER_VERBOSE, L"%s: %s: RTSP request:\n%s", protocolName, METHOD_CURL_WORKER_NAME, requestW));

            char *requestA = ConvertToMultiByte(requestW);
            unsigned int requestLength = (requestA != NULL) ? strlen(requestA) : 0;
            CHECK_CONDITION_EXECUTE(requestA == NULL, this->rtspDownloadResponse->SetResultCode(CURLE_OUT_OF_MEMORY));

            if (this->rtspDownloadResponse->GetResultCode() == CURLE_AGAIN)
            {
              // send data
              unsigned int receivedData = 0;
              CURLcode error = curl_easy_send(this->curl, requestA, requestLength, &receivedData);
              this->rtspDownloadResponse->SetResultCode((error == CURLE_OK) ? CURLE_AGAIN : error);
              FREE_MEM(requestA);
            }
          }
          
          if (this->rtspDownloadResponse->GetResultCode() != CURLE_AGAIN)
          {
            // some problem occured
            FREE_MEM_CLASS(request);
            FREE_MEM_CLASS(response);
          }
        }
      }
    }

    if ((startTicks != 0) && (endTicks != 0) && (GetTickCount() > endTicks))
    {
      CLockMutex lock(this->mutex, INFINITE);

      if (this->rtspDownloadResponse->GetResultCode() == CURLE_AGAIN)
      {
        // timeout occured
        this->rtspDownloadResponse->SetResultCode(CURLE_OPERATION_TIMEDOUT);
      }

      startTicks = 0;
      endTicks = 0;
      FREE_MEM_CLASS(request);
      FREE_MEM_CLASS(response);
    }

    int readData = this->ReadData(buffer, BUFFER_LENGTH_DEFAULT);

    if (readData < 0)
    {
      CLockMutex lock(this->mutex, INFINITE);

      // error occured
      this->rtspDownloadResponse->SetResultCode((CURLcode)(-readData));

      FREE_MEM_CLASS(request);
      FREE_MEM_CLASS(response);
    }
    else if (readData >= 0)
    {
      CLockMutex lock(this->mutex, INFINITE);
      CURLcode error = CURLE_OK;

      if (readData > 0)
      {
        error = (this->rtspDownloadResponse->GetReceivedData()->AddToBufferWithResize(buffer, readData, this->rtspDownloadResponse->GetReceivedData()->GetBufferSize() * 2) == readData) ? CURLE_OK : CURLE_OUT_OF_MEMORY;
        CHECK_CONDITION_EXECUTE(error != CURLE_OK, this->rtspDownloadResponse->SetResultCode(error));
      }

      bool possibleInterleavedPacket = false;

      if ((request != NULL) && (response != NULL))
      {
        if ((error == CURLE_OK) && (this->rtspDownloadResponse->GetResultCode() == CURLE_AGAIN))
        {
          // we have pending RTSP request
          // response can be for that RTSP request or data can be interleaved packet

          unsigned int bufferSize = this->rtspDownloadResponse->GetReceivedData()->GetBufferOccupiedSpace();

          if (bufferSize > 0)
          {
            ALLOC_MEM_DEFINE_SET(temp, unsigned char, (bufferSize + 1), 0);

            if (temp == NULL)
            {
              error = CURLE_OUT_OF_MEMORY;
              this->rtspDownloadResponse->SetResultCode(error);
            }

            if (error == CURLE_OK)
            {
              this->rtspDownloadResponse->GetReceivedData()->CopyFromBuffer(temp, bufferSize, 0, 0);
              HRESULT parseResult = response->Parse(temp, bufferSize);

              if (parseResult > 0)
              {
                // received valid RTSP response
                char *responseA = SubstringA((char *)temp, 0, parseResult);
                if (responseA != NULL)
                {
                  wchar_t *responseW = ConvertToUnicodeA(responseA);
                  if (responseW != NULL)
                  {
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: RTSP response:\n%s", protocolName, METHOD_CURL_WORKER_NAME, responseW);
                  }
                  FREE_MEM(responseW);
                }
                FREE_MEM(responseA);

                this->rtspDownloadResponse->SetRtspResponse(response);
                this->rtspDownloadResponse->SetResultCode(CURLE_OK);
                this->rtspDownloadResponse->SetResponseCode(response->GetStatusCode());
                this->rtspDownloadResponse->GetReceivedData()->RemoveFromBufferAndMove(parseResult);
              }
              else if (parseResult == 0)
              {
                // no RTSP response in buffer => possible interleaved packet
                possibleInterleavedPacket = true;
              }
              else if (parseResult == HRESULT_FROM_WIN32(ERROR_MORE_DATA))
              {
                // wait for more data
              }
              else if (FAILED(parseResult))
              {
                // error

                wchar_t *responseW = ConvertToUnicodeA((char *)temp);
                if (responseW != NULL)
                {
                  this->logger->Log(LOGGER_WARNING, L"%s: %s: RTSP response:\n%s", protocolName, METHOD_CURL_WORKER_NAME, responseW);
                }
                FREE_MEM(responseW);

                this->rtspDownloadResponse->SetResultCode(CURLE_RECV_ERROR);
                this->rtspDownloadResponse->SetResponseCode(parseResult);
              }
            }
            FREE_MEM(temp);
          }
        }

        if (this->rtspDownloadResponse->GetResultCode() != CURLE_AGAIN)
        {
          // request finished (successfully or not, doesn't matter)

          startTicks = 0;
          endTicks = 0;
          FREE_MEM_CLASS(request);
          FREE_MEM_CLASS(response);
        }
      }

      if ((errorCode == CURLE_OK) && (possibleInterleavedPacket || (request == NULL) && (response == NULL)))
      {
        if (this->rtspDownloadResponse->GetRtspTracks()->Count() == 1)
        {
          // there must be only one RTSP track in case of interleaved transport
          CRtspTrack *track = this->rtspDownloadResponse->GetRtspTracks()->GetItem(0);

          CInterleavedDataPacket *packet = new CInterleavedDataPacket();
          CHECK_CONDITION_EXECUTE(packet == NULL, errorCode = CURLE_OUT_OF_MEMORY);

          unsigned int bufferLength = this->rtspDownloadResponse->GetReceivedData()->GetBufferOccupiedSpace();
          ALLOC_MEM_DEFINE_SET(temp, unsigned char, bufferLength, 0);
          CHECK_CONDITION_EXECUTE(temp == NULL, errorCode = CURLE_OUT_OF_MEMORY);

          if (errorCode == CURLE_OK)
          {
            this->rtspDownloadResponse->GetReceivedData()->CopyFromBuffer(temp, bufferLength, 0, 0);

            HRESULT parseResult = packet->Parse(temp, bufferLength);

            if (parseResult > 0)
            {
              // for current case it doesn't depend on ports (client or server, whole communication is on same pair)
              errorCode = this->ProcessReceivedBaseRtpPackets(track, track->GetClientControlPort(), packet->GetBaseRtpPackets()) ? errorCode : CURLE_RECV_ERROR;

              this->rtspDownloadResponse->GetReceivedData()->RemoveFromBufferAndMove(parseResult);
            }
            else if (parseResult == 0)
            {
              // not interleaved packet and also not RTSP response
              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_CURL_WORKER_NAME, L"not interleaved packet and also not RTSP response");
              errorCode = CURLE_RECV_ERROR;
            }
            else if (parseResult == HRESULT_FROM_WIN32(ERROR_MORE_DATA))
            {
              // wait for more data
            }
            else if (FAILED(parseResult))
            {
              // error
              this->logger->Log(LOGGER_ERROR, L"%s: %s: error while parsing interleaved packet data: 0x%08X", protocolName, METHOD_CURL_WORKER_NAME, parseResult);
              errorCode = CURLE_RECV_ERROR;
            }
          }

          FREE_MEM(temp);
          FREE_MEM_CLASS(packet);
        }
      }
    }

    if (errorCode == CURLE_OK)
    {
      // only one thread can work with RTSP tracks and responses in one time
      CLockMutex lock(this->mutex, INFINITE);

      for (unsigned int i = 0; ((errorCode == CURLE_OK) && (i < this->rtspDownloadResponse->GetRtspTracks()->Count())); i++)
      {
        CRtspTrack *track = this->rtspDownloadResponse->GetRtspTracks()->GetItem(i);

        CSimpleServer *dataServer = track->GetDataServer(); 
        CSimpleServer *controlServer = track->GetControlServer(); 

        if ((dataServer != NULL) && (controlServer != NULL))
        {
          if (dataServer->IsServerType(SERVER_TYPE_UDP) && controlServer->IsServerType(SERVER_TYPE_UDP))
          {
            for (unsigned int k = 0; k < 2; k++)
            {
              CUdpServer *server = (k == 0) ? (CUdpServer *)dataServer : (CUdpServer *)controlServer;

              for (unsigned int j = 0; ((errorCode == CURLE_OK) && (j < server->GetServers()->Count())); j++)
              {
                CUdpSocketContext *udpContext = (CUdpSocketContext *)(server->GetServers()->GetItem(j));

                unsigned int pendingIncomingDataLength = 0;
                unsigned int interleavedPacketLength = 0;
                HRESULT result = S_OK;

                do
                {
                  result = udpContext->GetPendingIncomingDataLength(&pendingIncomingDataLength);

                  if (SUCCEEDED(result) && (pendingIncomingDataLength != 0))
                  {
                    // allocate buffer and receive data
                    // increase pending incoming data by 4 (interleaved packet header)
                    interleavedPacketLength = pendingIncomingDataLength + 4;
                    CHECK_CONDITION_EXECUTE(interleavedPacketLength > INTERLEAVED_MAX_PACKET_LENGTH, result = E_OUTOFMEMORY);

                    if (SUCCEEDED(result))
                    {
                      ALLOC_MEM_DEFINE_SET(buffer, unsigned char, interleavedPacketLength, 0);
                      CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

                      unsigned int receivedLength = 0;
                      CIpAddress *sender = NULL;

                      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = udpContext->Receive((char *)(buffer + 4), pendingIncomingDataLength, &receivedLength, &sender));
                      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (pendingIncomingDataLength == receivedLength) ? result : E_NOT_VALID_STATE);
                      CHECK_POINTER_HRESULT(result, sender, result, E_OUTOFMEMORY);

                      // for created server it can't be interleaved transport
                      // create interleaved packet, parse it and process base RTP packet collection

                      if (SUCCEEDED(result))
                      {
                        buffer[0] = INTERLEAVED_PACKET_HEADER_IDENTIFIER;   // interleaved packet identifier
                        buffer[1] = 0x00;                                   // interleaved packet channnel (value doesn't matter)
                        WBE16(buffer, 2, pendingIncomingDataLength);        // interleaved packet length (without header length)
                      }

                      CInterleavedDataPacket *packet = new CInterleavedDataPacket();
                      CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

                      if (SUCCEEDED(result))
                      {
                        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = packet->Parse(buffer, interleavedPacketLength) ? result : E_FAIL);
                        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->ProcessReceivedBaseRtpPackets(track, udpContext->GetIpAddress()->GetPort(), packet->GetBaseRtpPackets()) ? result : E_FAIL);

                        if (FAILED(result))
                        {
                          // error while processing received packet, dump it for further analysis

                          // every byte is in HEX encoding plus space
                          // every 32 bytes is new line
                          // add one character for null terminating character
                          unsigned int dumpPacketLength = interleavedPacketLength * 3 + ((interleavedPacketLength / 32) + 1) * 2 + 1;
                          ALLOC_MEM_DEFINE_SET(dumpPacket, wchar_t, dumpPacketLength, 0);
                          
                          if (dumpPacket != NULL)
                          {
                            unsigned int outputPosition = 0;
                            for (unsigned int i = 0; i < interleavedPacketLength; i++)
                            {
                              dumpPacket[outputPosition++] = get_charW(buffer[i] >> 4);
                              dumpPacket[outputPosition++] = get_charW(buffer[i] & 0x0F);
                              dumpPacket[outputPosition++] = L' ';
                              
                              if ((i % 32) == 0x1F)
                              {
                                dumpPacket[outputPosition++] = L'\r';
                                dumpPacket[outputPosition++] = L'\n';
                              }
                            }
                          }

                          this->logger->Log(LOGGER_ERROR, L"%s: %s: error while processing packet, track url: '%s', IP: %s, server IP: %s, length: %u\n%s",
                            protocolName, METHOD_CURL_WORKER_NAME,
                            track->GetTrackUrl(),
                            udpContext->GetIpAddress()->GetAddressString(),
                            sender->GetAddressString(),
                            interleavedPacketLength,
                            dumpPacket);

                          FREE_MEM(dumpPacket);
                        }
                      }

                      FREE_MEM_CLASS(packet);
                      FREE_MEM(buffer);
                      FREE_MEM_CLASS(sender);
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
          }
        }
      }
    }

    if (errorCode == CURLE_OK)
    {
      // only one thread can work with RTSP tracks and responses in one time
      CLockMutex lock(this->mutex, INFINITE);

      for (unsigned int i = 0; ((errorCode == CURLE_OK) && (i < this->rtspDownloadResponse->GetRtspTracks()->Count())); i++)
      {
        CRtspTrack *track = this->rtspDownloadResponse->GetRtspTracks()->GetItem(i);

        // check last receiver report time and interval
        // if needed, send receiver report for track and set last receiver report
        if ((GetTickCount() - track->GetLastReceiverReportTime()) > track->GetReceiverReportInterval())
        {
          CReceiverReportRtcpPacket *receiverReport = new CReceiverReportRtcpPacket();
          CSourceDescriptionRtcpPacket *sourceDescription = new CSourceDescriptionRtcpPacket();

          errorCode = (receiverReport != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;
          errorCode = (sourceDescription != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;
          // we must have sender SSRC to send report
          errorCode = track->IsSetSenderSynchronizationSourceIdentifier() ? errorCode : CURLE_SEND_ERROR;

          if (errorCode == CURLE_OK)
          {
            // fill receiver report RTCP packet with data
            receiverReport->SetSenderSynchronizationSourceIdentifier(track->GetSynchronizationSourceIdentifier());

            CReportBlock *reportBlock = new CReportBlock();
            errorCode = (receiverReport != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

            if (errorCode == CURLE_OK)
            {
              // set SSRC of server track
              reportBlock->SetSynchronizationSourceIdentifier(track->GetSenderSynchronizationSourceIdentifier());

              // set statistics of track

              errorCode = receiverReport->GetReportBlocks()->Add(reportBlock) ? errorCode : CURLE_OUT_OF_MEMORY;
            }

            if (errorCode != CURLE_OK)
            {
              FREE_MEM_CLASS(reportBlock);
            }
          }

          if (errorCode == CURLE_OK)
          {
            // fill source description RTCP packet with data
            CSourceDescriptionChunk *chunk = new CSourceDescriptionChunk();
            errorCode = (chunk != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

            if (errorCode == CURLE_OK)
            {
              chunk->SetIdentifier(track->GetSynchronizationSourceIdentifier());

              CCanonicalEndPointSourceDescriptionItem *endPoint = new CCanonicalEndPointSourceDescriptionItem();
              errorCode = (chunk != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

              if (errorCode == CURLE_OK)
              {
                ALLOC_MEM_DEFINE_SET(computerName, wchar_t, (MAX_COMPUTERNAME_LENGTH + 1), 0);
                errorCode = (computerName != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

                if (errorCode == CURLE_OK)
                {
                  DWORD length = MAX_COMPUTERNAME_LENGTH + 1;
                  errorCode = GetComputerName(computerName, &length) ? errorCode : CURLE_OUT_OF_MEMORY;

                  if (errorCode == CURLE_OK)
                  {
                    errorCode = endPoint->SetEndPointName(computerName) ? errorCode : CURLE_OUT_OF_MEMORY;
                    errorCode = chunk->GetItems()->Add(endPoint) ? errorCode : CURLE_OUT_OF_MEMORY;
                  }
                }
                FREE_MEM(computerName);
              }

              if (errorCode != CURLE_OK)
              {
                FREE_MEM_CLASS(endPoint);
              }

              errorCode = sourceDescription->GetChunks()->Add(chunk) ? errorCode : CURLE_OUT_OF_MEMORY;
            }

            if (errorCode != CURLE_OK)
            {
              FREE_MEM_CLASS(chunk);
            }
          }

          if (errorCode == CURLE_OK)
          {
            unsigned int receiverReportSize = receiverReport->GetSize();
            unsigned int sourceDescriptionSize = sourceDescription->GetSize();

            // allocate four more bytes - in case that we need to create interlaved packet
            ALLOC_MEM_DEFINE_SET(reportBuffer, unsigned char, (receiverReportSize + sourceDescriptionSize + 4), 0);
            errorCode = (reportBuffer != NULL) ? errorCode : CURLE_OUT_OF_MEMORY;

            CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = receiverReport->GetPacket(reportBuffer + 4, receiverReportSize) ? errorCode : CURLE_SEND_ERROR);
            CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = sourceDescription->GetPacket(reportBuffer + 4 + receiverReportSize, sourceDescriptionSize) ? errorCode : CURLE_SEND_ERROR);

            if (errorCode == CURLE_OK)
            {
              // send report buffer to remote server
              CSimpleServer *controlServer = track->GetControlServer(); 

              if (controlServer != NULL)
              {
                // track has control server

                CUdpServer *udpControlServer = dynamic_cast<CUdpServer *>(controlServer);
                CTcpServer *tcpControlServer = dynamic_cast<CTcpServer *>(controlServer);

                if (udpControlServer != NULL)
                {
                  CUdpSocketContext *udpContext = NULL;

                  // get control UDP socket context with set last sender IP address
                  for (unsigned int i = 0; i < udpControlServer->GetServers()->Count(); i++)
                  {
                    CUdpSocketContext *server = (CUdpSocketContext *)udpControlServer->GetServers()->GetItem(i);

                    if (server->GetLastSenderIpAddress() != NULL)
                    {
                      udpContext = server;
                      break;
                    }
                  }

                  errorCode = (udpContext != NULL) ? errorCode : CURLE_SEND_ERROR;

                  if (errorCode == CURLE_OK)
                  {
                    unsigned int sentLength = 0;
                    errorCode = SUCCEEDED(udpContext->Send((const char *)(reportBuffer + 4), receiverReportSize + sourceDescriptionSize, &sentLength)) ? errorCode : CURLE_SEND_ERROR;
                    errorCode = (sentLength == (receiverReportSize + sourceDescriptionSize)) ? errorCode : CURLE_SEND_ERROR;
                  }
                }

                if (tcpControlServer != NULL)
                {
                  // not implemented
                  errorCode = CURLE_SEND_ERROR;
                }
              }
              else
              {
                // track hasn't control server = it's interleaved transport
                // in that case we send receiver report packet through CURL instance

                // but first we need to create interleaved packet
                reportBuffer[0] = INTERLEAVED_PACKET_HEADER_IDENTIFIER;                             // interleaved packet identifier
                reportBuffer[1] = track->GetTransportResponseHeader()->GetMaxInterleavedChannel();  // interleaved packet channnel (specified in transport header)
                WBE16(reportBuffer, 2, (receiverReportSize + sourceDescriptionSize));               // interleaved packet length (without header length)

                errorCode = this->SendData(reportBuffer, receiverReportSize + sourceDescriptionSize + 4, this->GetReceiveDataTimeout());
              }

              track->SetLastReceiverReportTime(GetTickCount());
            }

            FREE_MEM(reportBuffer);
          }

          FREE_MEM_CLASS(receiverReport);
          FREE_MEM_CLASS(sourceDescription);
        }
      }

      errorCode = CURLE_OK;
    }

    Sleep(1);
  }

  FREE_MEM(buffer);

  CHECK_CONDITION_EXECUTE(errorCode != CURLE_OK, this->ReportCurlErrorMessage(LOGGER_ERROR, this->protocolName, METHOD_CURL_WORKER_NAME, L"error while sending, receiving or processing data", errorCode));
  this->rtspDownloadResponse->SetResultCode(errorCode);

  unsigned int count = 0;
  {
    CLockMutex lock(this->mutex, INFINITE);

    for (unsigned int i = 0; (i < this->rtspDownloadResponse->GetRtspTracks()->Count()); i++)
    {
      CRtspTrack *track = this->rtspDownloadResponse->GetRtspTracks()->GetItem(i);

      CSimpleServer *dataServer = track->GetDataServer(); 
      CSimpleServer *controlServer = track->GetControlServer(); 

      if ((dataServer != NULL) && (controlServer != NULL))
      {
        for (unsigned int k = 0; k < 2; k++)
        {
          CSimpleServer *server = (k == 0) ? dataServer : controlServer;

          for (unsigned int j = 0; (j < server->GetServers()->Count()); j++)
          {
            CSocketContext *context = server->GetServers()->GetItem(j);

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: address: %s, received bytes: %lld, sent bytes: %lld", this->protocolName, METHOD_CURL_WORKER_NAME, (context->GetIpAddress()->GetAddressString() == NULL) ? L"unknown" : context->GetIpAddress()->GetAddressString(), context->GetReceivedDataLength(), context->GetSentDataLength());
          }
        }
      }

      /*if (track->GetDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() != 0)
      {
        unsigned int length = track->GetDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);

        track->GetDownloadResponse()->GetReceivedData()->CopyFromBuffer(buffer, length, 0, 0);
        wchar_t *file = FormatString(L"X:\\temp2\\output_%08u.dat", count++);
        FILE *stream = _wfopen(file, L"wb");
        fwrite(buffer, sizeof(unsigned char), length, stream);
        fclose(stream);

        FREE_MEM(buffer);
      }*/
    }
  }

  this->state = CURL_STATE_RECEIVED_ALL_DATA;
  this->stopReceivingTicks = GetTickCount();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, this->protocolName, METHOD_CURL_WORKER_NAME);
  return S_OK;
}

unsigned int CRtspCurlInstance::GetSameConnectionTcpPreference(void)
{
  return this->sameConnectionTcpPreference;
}

unsigned int CRtspCurlInstance::GetMulticastPreference(void)
{
  return this->multicastPreference;
}

unsigned int CRtspCurlInstance::GetUdpPreference(void)
{
  return this->udpPreference;
}

unsigned int CRtspCurlInstance::GetTcpPreference(void)
{
  return this->tcpPreference;
}

unsigned int CRtspCurlInstance::GetRtspClientPort(void)
{
  return this->rtspClientPort;
}

void CRtspCurlInstance::SetSameConnectionTcpPreference(unsigned int preference)
{
  this->sameConnectionTcpPreference = preference;
}

void CRtspCurlInstance::SetMulticastPreference(unsigned int preference)
{
  this->multicastPreference = preference;
}

void CRtspCurlInstance::SetUdpPreference(unsigned int preference)
{
  this->udpPreference = preference;
}

void CRtspCurlInstance::SetTcpPreference(unsigned int preference)
{
  this->tcpPreference = preference;
}

void CRtspCurlInstance::SetRtspClientPort(unsigned int clientPort)
{
  this->rtspClientPort = clientPort;
}

bool CRtspCurlInstance::StopReceivingData(void)
{
  if ((this->lastCommand == CURL_RTSPREQ_SETUP) || (this->lastCommand == CURL_RTSPREQ_PLAY))
  {
    // create and send TEARDOWN request for stream
    CURLcode errorCode = CURLE_OK;

    CRtspTeardownRequest *teardown = new CRtspTeardownRequest();
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (teardown != NULL) ? errorCode : CURLE_OUT_OF_MEMORY);
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = (teardown->SetUri(this->rtspDownloadRequest->GetUrl())) ? errorCode : CURLE_OUT_OF_MEMORY);
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, teardown->SetTimeout(this->GetReceiveDataTimeout()));
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, teardown->SetSequenceNumber(this->lastSequenceNumber++));
    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = teardown->SetSessionId(this->sessionId) ? errorCode : CURLE_OUT_OF_MEMORY);

    // session ID is no longer required
    // clear it to avoid error in processing response
    FREE_MEM(this->sessionId);

    CHECK_CONDITION_EXECUTE(errorCode == CURLE_OK, errorCode = this->SendAndReceive(teardown, errorCode, L"TEARDOWN", L"StopReceivingData()"));
    FREE_MEM_CLASS(teardown);
  }

  return __super::StopReceivingData();
}