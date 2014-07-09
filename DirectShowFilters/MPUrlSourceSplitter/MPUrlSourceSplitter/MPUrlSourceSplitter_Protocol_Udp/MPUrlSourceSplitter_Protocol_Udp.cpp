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

#pragma warning(push)
// disable warning: 'INT8_MIN' : macro redefinition
// warning is caused by stdint.h and intsafe.h, which both define same macro
#pragma warning(disable:4005)

#include "MPUrlSourceSplitter_Protocol_Udp.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "MPUrlSourceSplitter_Protocol_Udp_Parameters.h"
#include "Parameters.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Udpd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Udp"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Udp(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Udp *protocol = (CMPUrlSourceSplitter_Protocol_Udp *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Udp::CMPUrlSourceSplitter_Protocol_Udp(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->lockCurlMutex = NULL;
  this->lockMutex = NULL;
  this->mainCurlInstance = NULL;
  this->streamLength = 0;
  this->connectionState = None;
  this->mediaPackets = NULL;
  this->cacheFile = NULL;
  this->currentStreamPosition = 0;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->lastReceiveDataTime = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->mediaPackets = new CMediaPacketCollection(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->mediaPackets, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_UDP, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_UDP);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    version = CCurlInstance::GetCurlVersion();
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Protocol_Udp::~CMPUrlSourceSplitter_Protocol_Udp()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->mediaPackets);

  if (this->lockMutex != NULL)
  {
    CloseHandle(this->lockMutex);
    this->lockMutex = NULL;
  }

  if (this->lockCurlMutex != NULL)
  {
    CloseHandle(this->lockCurlMutex);
    this->lockCurlMutex = NULL;
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Udp::GetConnectionState(void)
{
  return this->connectionState;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::ParseUrl(const CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  this->ClearSession();

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CProtocolPluginConfiguration *protocolConfiguration = new CProtocolPluginConfiguration(&result, (CParameterCollection *)parameters);
    CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->Initialize(protocolConfiguration));
    FREE_MEM_CLASS(protocolConfiguration);
  }

  const wchar_t *url = this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL);
  CHECK_POINTER_HRESULT(result, url, result, E_URL_NOT_SPECIFIED);

  if (SUCCEEDED(result))
  {
    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
    if (urlComponents == NULL)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'url components'");
      result = E_OUTOFMEMORY;
    }

    if (SUCCEEDED(result))
    {
      ZeroURL(urlComponents);
      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

      this->logger->Log(LOGGER_INFO, L"%s: %s: url: %s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, url);

      if (!InternetCrackUrl(url, 0, 0, urlComponents))
      {
        this->logger->Log(LOGGER_ERROR, L"%s: %s: InternetCrackUrl() error: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, GetLastError());
        result = E_FAIL;
      }
    }

    if (SUCCEEDED(result))
    {
      int length = urlComponents->dwSchemeLength + 1;
      ALLOC_MEM_DEFINE_SET(protocol, wchar_t, length, 0);
      if (protocol == NULL) 
      {
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'protocol'");
        result = E_OUTOFMEMORY;
      }

      if (SUCCEEDED(result))
      {
        wcsncat_s(protocol, length, urlComponents->lpszScheme, urlComponents->dwSchemeLength);

        bool supportedProtocol = false;
        for (int i = 0; i < TOTAL_SUPPORTED_PROTOCOLS; i++)
        {
          if (_wcsnicmp(urlComponents->lpszScheme, SUPPORTED_PROTOCOLS[i], urlComponents->dwSchemeLength) == 0)
          {
            supportedProtocol = true;
            break;
          }
        }

        if (!supportedProtocol)
        {
          // not supported protocol
          this->logger->Log(LOGGER_INFO, L"%s: %s: unsupported protocol '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
          result = E_FAIL;
        }
      }
      FREE_MEM(protocol);
    }

    FREE_MEM(urlComponents);
  }

  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::ReceiveData(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->lockMutex, INFINITE);

    if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && ((this->connectionState == Opening) || (this->connectionState == Opened)))
    {
      {
        CLockMutex lockData(this->lockCurlMutex, INFINITE);

        unsigned int bytesRead = this->mainCurlInstance->GetUdpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
        if (bytesRead > 0)
        {
          this->connectionState = Opened;
          this->lastReceiveDataTime = GetTickCount();

          CMediaPacket *mediaPacket = new CMediaPacket(&result);
          CHECK_POINTER_HRESULT(result, mediaPacket, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, mediaPacket->GetBuffer()->InitializeBuffer(bytesRead), result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            mediaPacket->SetStart(this->currentStreamPosition);
            mediaPacket->SetEnd(this->currentStreamPosition + bytesRead - 1);
            mediaPacket->SetLoadedToMemoryTime(GetTickCount());

            mediaPacket->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetUdpDownloadResponse()->GetReceivedData(), 0, bytesRead);
          }

          CHECK_CONDITION_HRESULT(result, this->mediaPackets->Add(mediaPacket), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaPacket));

          if (SUCCEEDED(result))
          {
            this->currentStreamPosition += (int64_t)bytesRead;
            this->mainCurlInstance->GetUdpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(bytesRead);
          }

          if (SUCCEEDED(result) && (!this->IsSetStreamLength()))
          {
            // UDP/RTP is always live stream, content length is not specified
            this->flags |= PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED;

            if (this->streamLength == 0)
            {
              // stream length not set
              // just make guess
              this->streamLength = LONGLONG(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
            }
            else if ((this->currentStreamPosition > (this->streamLength * 3 / 4)))
            {
              // it is time to adjust stream length, we are approaching to end but still we don't know total length
              this->streamLength = this->currentStreamPosition * 2;
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: adjusting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
            }
          }
        }
      }
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance == NULL) && (!this->IsWholeStreamDownloaded()))
    {
      this->connectionState = Initializing;

      unsigned int finishTime = UINT_MAX;
      if (SUCCEEDED(result))
      {
        finishTime = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, UINT_MAX);
        if (finishTime != UINT_MAX)
        {
          unsigned int currentTime = GetTickCount();
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: finish time specified, current time: %u, finish time: %u, diff: %u (ms)", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, currentTime, finishTime, finishTime - currentTime);
          this->configuration->Remove(PARAMETER_NAME_FINISH_TIME, true);
        }
      }

      this->mainCurlInstance = new CUdpCurlInstance(&result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        if (this->configuration->GetValueBool(PARAMETER_NAME_DUMP_INPUT_RAW_DATA, true, PARAMETER_NAME_DUMP_INPUT_RAW_DATA_DEFAULT))
        {
          wchar_t *storeFilePath = this->GetStoreFile(L"dump");
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }

        CUdpDownloadRequest *request = new CUdpDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          request->SetUrl(this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL));

          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_UDP_OPEN_CONNECTION_TIMEOUT, true, UDP_OPEN_CONNECTION_TIMEOUT_DEFAULT));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
          request->SetCheckInterval(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_UDP_RECEIVE_DATA_CHECK_INTERVAL, true, UDP_RECEIVE_DATA_CHECK_INTERVAL_DEFAULT));

          if (SUCCEEDED(this->mainCurlInstance->Initialize(request)))
          {
            // all parameters set
            // start receiving data

            if (SUCCEEDED(this->mainCurlInstance->StartReceivingData()))
            {
              this->connectionState = Opening;
            }
          }
          else
          {
            this->connectionState = InitializeFailed;
          }
        }
        FREE_MEM_CLASS(request);
      }
    }

    if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()) && (this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    {
      // all data received, we're not receiving data
      // check end of stream or error on HTTP connection

      if (SUCCEEDED(this->mainCurlInstance->GetUdpDownloadResponse()->GetResultError()))
      {
        // UDP/RTP is always live stream

        // whole stream downloaded
        this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
        this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"live stream, all data received");
      }
      else
      {
        // check if all data removed from CURL instance
        if (this->mainCurlInstance->GetUdpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
        {
          // error while receiving data, stops receiving data
          // this clear CURL instance and buffer, it leads to GetConnectionState() to PROTOCOL_CONNECTION_STATE_NONE result and connection will be reopened by ProtocolHoster
          this->StopReceivingData();

          if (this->mediaPackets->Count() != 0)
          {
            CMediaPacket *lastMediaPacket = this->mediaPackets->GetItem(this->mediaPackets->Count() - 1);

            lastMediaPacket->SetDiscontinuity(true);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, start '%lld', size '%u', current stream position: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, lastMediaPacket->GetStart(), lastMediaPacket->GetLength(), this->currentStreamPosition);
          }
        }
      }
    }

    if ((!this->IsSetStreamLength()) && (this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen()))
    {
      // reached end of stream, set stream length

      // get last media packet to get total length
      CMediaPacket *mediaPacket = (this->mediaPackets->Count() != 0) ? this->mediaPackets->GetItem(this->mediaPackets->Count() - 1) : NULL;

      this->streamLength = (mediaPacket != NULL) ? (mediaPacket->GetEnd() + 1) : 0;
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);

      this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
      this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

      // set current stream position to stream length to get correct result in QueryStreamProgress() method
      this->currentStreamPosition = this->streamLength;

      FREE_MEM_CLASS(this->mainCurlInstance);
    }

    // process stream package (if valid)
    if (streamPackage->GetState() == CStreamPackage::Created)
    {
      HRESULT res = S_OK;
      // stream package is just created, it wasn't processed before
      CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CHECK_POINTER_HRESULT(res, request, res, E_INVALID_STREAM_PACKAGE_REQUEST);

      if (SUCCEEDED(res))
      {
        // set start time of processing request
        // set Waiting state
        // set response

        CStreamPackageDataResponse *response = new CStreamPackageDataResponse(&res);
        CHECK_POINTER_HRESULT(res, response, res, E_OUTOFMEMORY);

        // allocate memory for response
        CHECK_CONDITION_HRESULT(res, response->GetBuffer()->InitializeBuffer(request->GetLength()), res, E_OUTOFMEMORY);

        if (SUCCEEDED(res))
        {
          streamPackage->GetRequest()->SetStartTime(GetTickCount());
          streamPackage->SetWaiting();
          streamPackage->SetResponse(response);
        }

        CHECK_CONDITION_EXECUTE(FAILED(res), FREE_MEM_CLASS(response));
      }

      CHECK_CONDITION_EXECUTE(FAILED(res), streamPackage->SetCompleted(res));
    }

    if ((streamPackage->GetState() == CStreamPackage::Waiting) || (streamPackage->GetState() == CStreamPackage::WaitingIgnoreTimeout))
    {
      // in Waiting or WaitingIgnoreTimeout state can be request only if request and response are correctly set
      CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

      // clear response buffer
      response->GetBuffer()->ClearBuffer();

      // first try to find starting media packet (packet which have first data)
      unsigned int packetIndex = UINT_MAX;
      unsigned int foundDataLength = 0;

      int64_t startPosition = request->GetStart();
      packetIndex = this->mediaPackets->GetMediaPacketIndexBetweenPositions(startPosition);

      while (packetIndex != UINT_MAX)
      {
        // get media packet
        CMediaPacket *mediaPacket = this->mediaPackets->GetItem(packetIndex);

        // set copy data start and copy data length
        unsigned int copyDataStart = (startPosition > mediaPacket->GetStart()) ? (unsigned int)(startPosition - mediaPacket->GetStart()) : 0;
        unsigned int copyDataLength = min(mediaPacket->GetLength() - copyDataStart, request->GetLength() - foundDataLength);

        // copy data from media packet to response buffer
        if (this->cacheFile->LoadItems(this->mediaPackets, packetIndex, true, false))
        {
          // memory is allocated while switching from Created to Waiting state, we can't have problem on next line
          response->GetBuffer()->AddToBufferWithResize(mediaPacket->GetBuffer(), copyDataStart, copyDataLength);
        }
        else
        {
          // we can't copy data, try it later
          break;
        }

        // update length of data
        foundDataLength += copyDataLength;

        if (mediaPacket->IsDiscontinuity())
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, completing request, request '%u', start '%lld', size '%u', found: '%u', current stream position: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), foundDataLength, this->currentStreamPosition);

          response->SetDiscontinuity(true);
        }

        if ((!mediaPacket->IsDiscontinuity()) && (foundDataLength < request->GetLength()))
        {
          // find another media packet after end of this media packet
          startPosition += copyDataLength;

          packetIndex = this->mediaPackets->GetMediaPacketIndexBetweenPositions(startPosition);
        }
        else
        {
          // do not find any more media packets for this request because we have enough data
          break;
        }
      }

      if (foundDataLength < request->GetLength())
      {
        // found data length is lower than requested
        // check request flags, maybe we can complete request

        if ((request->IsSetAnyNonZeroDataLength() && (foundDataLength > 0)) ||
          (request->IsSetAnyDataLength()))
        {
          // request can be completed with any length of available data
          streamPackage->SetCompleted(S_OK);
        }
        else
        {
          if (response->IsDiscontinuity())
          {
            streamPackage->SetCompleted(S_OK);
          }
          else if (this->IsConnectionLostCannotReopen())
          {
            // connection is lost and we cannot reopen it
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            streamPackage->SetCompleted((response->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_CONNECTION_LOST_CANNOT_REOPEN);
          }
          else if (this->IsEndOfStreamReached() && ((request->GetStart() + request->GetLength()) >= this->streamLength))
          {
            // we are not receiving more data, complete request
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            streamPackage->SetCompleted((response->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_NO_MORE_DATA_AVAILABLE);
          }
          else if (this->IsLiveStreamDetected() && (this->connectionState != Opened))
          {
            // we have live stream, we are missing data and we have not opened connection
            // we lost some data, report discontinuity

            response->SetDiscontinuity(true);
            streamPackage->SetCompleted(S_OK);
          }
        }

        if (streamPackage->GetState() == CStreamPackage::Waiting)
        {
          // no seeking by position is available
          // check request against current stream position, if we can receive requested data

          if ((request->GetStart() + request->GetLength()) <= this->currentStreamPosition)
          {
            // it's bad, current stream position is after requested data and we can't seek
            this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', before current stream position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetStart() + request->GetLength(), this->currentStreamPosition);

            streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
          }
        }
      }
      else if (foundDataLength == request->GetLength())
      {
        // found data length is equal than requested
        streamPackage->SetCompleted(S_OK);
      }
    }

    // store media packets to temporary file
    if ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
    {
      this->lastStoreTime = GetTickCount();

      if (this->cacheFile->GetCacheFile() == NULL)
      {
        wchar_t *storeFilePath = this->GetStoreFile(L"temp");
        CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
        FREE_MEM(storeFilePath);
      }

      // in case of live stream remove all downloaded and processed media packets
      // processed media packet means that all data from media packet were requested
      if (this->IsLiveStream() && (this->reportedStreamTime > 0) && (this->reportedStreamPosition > 0) && (this->pauseSeekStopMode == PAUSE_SEEK_STOP_MODE_NONE))
      {
        // remove used media packets
        // in case of live stream they will not be needed (after created demuxer and started playing)
        // in case of seeking based on position there can be serious problem, because position in data is not related to play time - switching audio stream will not work

        if (this->mediaPackets->Count() > 0)
        {
          unsigned int mediaPacketRemoveCount = 0;

          while (mediaPacketRemoveCount < this->mediaPackets->Count())
          {
            CMediaPacket *mediaPacket = this->mediaPackets->GetItem(mediaPacketRemoveCount);

            if ((int64_t)this->reportedStreamPosition <= mediaPacket->GetEnd())
            {
              // reported stream position is before media packet end = not whole media packet is processed
              break;
            }

            mediaPacketRemoveCount++;
          }

          if ((mediaPacketRemoveCount > 0) && (this->cacheFile->RemoveItems(this->mediaPackets, 0, mediaPacketRemoveCount)))
          {
            this->mediaPackets->Remove(0, mediaPacketRemoveCount);
          }
        }
      }

      // store all media packets (which are not stored) to file
      if ((this->cacheFile->GetCacheFile() != NULL) && (this->mediaPackets->Count() != 0))
      {
        this->cacheFile->StoreItems(this->mediaPackets, this->lastStoreTime, false);
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Udp::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_UDP_OPEN_CONNECTION_TIMEOUT, true, UDP_OPEN_CONNECTION_TIMEOUT_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Udp::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_UDP_OPEN_CONNECTION_SLEEP_TIME, true, UDP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Udp::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, UDP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT);
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configuration);

  if (SUCCEEDED(result) && (parameters != NULL))
  {
    this->configuration->Append(parameters);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->StopReceivingData());

  this->connectionState = SUCCEEDED(result) ? Opening : None;

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);

  this->connectionState = None;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    streamProgress->SetTotalLength(this->streamLength);
    streamProgress->SetCurrentLength(this->currentStreamPosition);

    if (this->IsStreamLengthEstimated())
    {
      result = VFW_S_ESTIMATED;
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  HRESULT result = __super::ClearSession();
 
  this->streamLength = 0;
  this->connectionState = None;
  this->cacheFile->Clear();
  this->mediaPackets->Clear();
  this->currentStreamPosition = 0;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->lastReceiveDataTime = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Udp::GetDuration(void)
{
  return this->IsLiveStream() ? DURATION_LIVE_STREAM : DURATION_UNSPECIFIED;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::GetStreamInformation(CStreamInformationCollection *streams)
{
  // UDP protocol has always one stream (container)
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streams);

  if (SUCCEEDED(result))
  {
    CStreamInformation *streamInfo = new CStreamInformation(&result);
    CHECK_POINTER_HRESULT(result, streamInfo, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      streamInfo->SetContainer(true);
    }

    CHECK_CONDITION_HRESULT(result, streams->Add(streamInfo), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamInfo));
  }

  return result;
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Udp::GetSeekingCapabilities(void)
{
  unsigned int result = SEEKING_METHOD_NONE;

  {
    // lock access to stream
    CLockMutex lock(this->lockMutex, INFINITE);
    
    result = (this->IsWholeStreamDownloaded()) ? SEEKING_METHOD_POSITION : SEEKING_METHOD_NONE;
  }

  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Udp::SeekToTime(unsigned int streamId, int64_t time)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu, to time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = E_SEEK_METHOD_NOT_SUPPORTED;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Udp::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Udp::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::Initialize(CPluginConfiguration *configuration)
{
  CProtocolPluginConfiguration *protocolConfiguration = (CProtocolPluginConfiguration *)configuration;
  HRESULT result = ((this->lockMutex != NULL) && (this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->logger->SetParameters(protocolConfiguration->GetConfiguration());
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(protocolConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT) ? PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED : PROTOCOL_PLUGIN_FLAG_NONE;
  }

  return result;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Udp::GetStoreFile(const wchar_t *extension)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_udp_%s.%s", folder, guid, extension);
    }
    FREE_MEM(guid);
  }

  return result;
}