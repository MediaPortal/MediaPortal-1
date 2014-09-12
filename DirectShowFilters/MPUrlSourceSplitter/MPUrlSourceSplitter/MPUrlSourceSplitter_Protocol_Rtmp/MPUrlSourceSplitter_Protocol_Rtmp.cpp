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

#include "MPUrlSourceSplitter_Protocol_Rtmp.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "FlvPacket.h"
#include "VersionInfo.h"
#include "MPUrlSourceSplitter_Protocol_Rtmp_Parameters.h"
#include "Parameters.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Rtmpd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Rtmp"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Rtmp(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Rtmp *protocol = (CMPUrlSourceSplitter_Protocol_Rtmp *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Rtmp::CMPUrlSourceSplitter_Protocol_Rtmp(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->lockCurlMutex = NULL;
  this->lockMutex = NULL;
  this->mainCurlInstance = NULL;
  this->streamLength = 0;
  this->connectionState = None;
  this->streamFragments = NULL;
  this->cacheFile = NULL;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  this->lastFlvPacketTimestamp = 0;
  this->lastCumulatedFlvTimestamp = 0;
  this->headerAndMetaPacketSize = 0;
  this->videoTimestampCorrection = 0;
  this->audioTimestampCorrection = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->streamFragments = new CRtmpStreamFragmentCollection(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP);
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

CMPUrlSourceSplitter_Protocol_Rtmp::~CMPUrlSourceSplitter_Protocol_Rtmp()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->streamFragments);

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

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Rtmp::GetConnectionState(void)
{
  return this->connectionState;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::ReceiveData(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->lockMutex, INFINITE);

    if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && ((this->connectionState == Opening) || (this->connectionState == Opened)))
    {
      unsigned char *buffer = NULL;
      unsigned int bufferSize = 0;

      {
        // copy packets to avoid blocking of RTMP download instance
        CLockMutex lockData(this->lockCurlMutex, INFINITE);

        bufferSize = this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
        buffer = ALLOC_MEM_SET(buffer, unsigned char, bufferSize, 0);
        CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->CopyFromBuffer(buffer, bufferSize) == bufferSize , result, E_OUTOFMEMORY);
      }

      if (SUCCEEDED(result) && (bufferSize != 0))
      {
        this->connectionState = Opened;

        // holds processed data size, which can be removed from CURL instance
        unsigned int processed = 0;

        // split or add received FLV packets to stream fragments
        CRtmpStreamFragment *currentDownloadingFragment = this->streamFragments->GetItem(this->streamFragmentDownloading);
        CFlvPacket *flvPacket = new CFlvPacket(&result);
        CHECK_POINTER_HRESULT(result, flvPacket, result, E_OUTOFMEMORY);
        int parsedPacket = FLV_PARSE_RESULT_OK;

        while (SUCCEEDED(result) && (currentDownloadingFragment != NULL) && (parsedPacket == FLV_PARSE_RESULT_OK) && (processed < bufferSize))
        {
          parsedPacket = flvPacket->ParsePacket(buffer + processed, bufferSize - processed);

          // FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_HEADER and FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_PACKET means that we must wait for more data
          if (parsedPacket == FLV_PARSE_RESULT_CHECK_SIZE_INCORRECT)
          {
            this->logger->Log(LOGGER_WARNING, L"%s: %s: malformed FLV packet detected, type: 0x%02X, size: %u, timestamp: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, flvPacket->GetType(), flvPacket->GetSize(), flvPacket->GetTimestamp());

            // we have received data, it seems that we have FLV packet, but with incorrect check size = malformed FLV packet
            int findResult = flvPacket->FindPacket(buffer + processed, bufferSize - processed, FLV_PACKET_MINIMUM_CHECKED_UNSPECIFIED);

            if (findResult >= 0)
            {
              // found sequence of correct FLV packets
              // just remove malformed data from buffer and continue
              processed += (unsigned int)findResult;
            }
            else
            {
              // error returned
              switch (findResult)
              {
              case FLV_FIND_RESULT_NOT_FOUND:
                // in case of small amount of data it is not relevant
                break;
              case FLV_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER:
                // too small amount of data
                break;
              case FLV_FIND_RESULT_NOT_ENOUGH_MEMORY:
                // bad - not enough memory
                result = E_OUTOFMEMORY;
                this->logger->Log(LOGGER_ERROR, L"%s: %s: malformed FLV packet detected, not enough memory, buffer size: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace());
                break;
              case FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS:
                // wait for more data
                break;
              }
            }
          }
          else if (parsedPacket == FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY)
          {
            this->logger->Log(LOGGER_ERROR, L"%s: %s: not enough memory for FLV packet, type: 0x%02X, size: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, flvPacket->GetType(), flvPacket->GetSize());
            result = E_OUTOFMEMORY;
          }
          else if (parsedPacket == FLV_PARSE_RESULT_OK)
          {
            // most common case

            if ((flvPacket->GetType() == FLV_PACKET_HEADER) || (flvPacket->GetType() == FLV_PACKET_META))
            {
              if (this->streamFragmentDownloading == 0)
              {
                // if not downloading first stream fragment and received header or meta packet
                // we don't need another header or meta packet, just ignore it

                // add FLV packet to current downloading stream fragment
                if (SUCCEEDED(result) && (currentDownloadingFragment != NULL))
                {
                  result = (currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(flvPacket->GetData(), flvPacket->GetSize(), currentDownloadingFragment->GetBuffer()->GetBufferSize() * 2) == flvPacket->GetSize()) ? result : E_OUTOFMEMORY;

                  currentDownloadingFragment->SetContainsHeaderOrMetaPacket(true);
                  this->headerAndMetaPacketSize += flvPacket->GetSize();
                }
              }
            }
            else
            {
              // set first timestamp (if not already set)
              // timestamp is needed to compute (almost - after seeking we only assume that timestamps are correct) correct timestamps of fragments
              if (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_FIRST_TIMESTAMP))
              {
                unsigned int ticks = GetTickCount();
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_FIRST_TIMESTAMP;
                this->lastFlvPacketTimestamp = 0;
                this->lastCumulatedFlvTimestamp = 0;

                // change current downloading fragment FLV timestamp to correct timestamp based on time from start of stream (first time set ticks to track)
                if (this->IsLiveStream())
                {
                  //int64_t timestamp = ((int64_t)ticks - (int64_t)streamTrack->GetFirstRtpPacketTicks()) * streamTrack->GetClockFrequency() / 1000;

                  //this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, fragment timestamp changing from %lld to %lld, ticks: %u, track ticks: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), timestamp, ticks, streamTrack->GetFirstRtpPacketTicks());
                  //currentDownloadingFragment->SetFragmentRtpTimestamp(timestamp, false);
                }
              }

              if ((flvPacket->GetType() == FLV_PACKET_VIDEO) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_VIDEO_CORRECTION)))
              {
                // fragment start timestamp should not be greater than UINT_MAX (UINT_MAX  in ms is approx. 49.7 days)
                this->videoTimestampCorrection = (unsigned int)currentDownloadingFragment->GetFragmentStartTimestamp() - flvPacket->GetTimestamp();

                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: fragment timestamp: %lld, first FLV video packet timestamp: %u, correction: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, currentDownloadingFragment->GetFragmentStartTimestamp(), flvPacket->GetTimestamp(), this->videoTimestampCorrection);
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_VIDEO_CORRECTION;
              }

              if ((flvPacket->GetType() == FLV_PACKET_AUDIO) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_AUDIO_CORRECTION)))
              {
                this->audioTimestampCorrection = (unsigned int)currentDownloadingFragment->GetFragmentStartTimestamp() - (int64_t)flvPacket->GetTimestamp();

                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: fragment timestamp: %lld, first FLV audio packet timestamp: %u, correction: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, currentDownloadingFragment->GetFragmentStartTimestamp(), flvPacket->GetTimestamp(), this->audioTimestampCorrection);
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_AUDIO_CORRECTION;
              }

              flvPacket->SetTimestamp(flvPacket->GetTimestamp() + ((flvPacket->GetType() == FLV_PACKET_VIDEO) ? this->videoTimestampCorrection : this->audioTimestampCorrection));

              int64_t timestamp = this->GetFlvPacketTimestamp(flvPacket->GetTimestamp());

              if (!currentDownloadingFragment->IsSetFragmentStartTimestamp())
              {
                currentDownloadingFragment->SetFragmentStartTimestamp(timestamp);
              }

              CRtmpStreamFragment *nextFragment = this->streamFragments->GetItem(this->streamFragmentDownloading + 1);

              // nextFragment can be NULL in case that we are on the end of collection

              if (nextFragment != NULL)
              {
                if (timestamp >= nextFragment->GetFragmentStartTimestamp())
                {
                  // our FLV packet timestamp is greater than or equal to next fragment timestamp
                  // this means that we are receiving data, which we already have - in case if next fragment is downloaded

                  currentDownloadingFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
                  currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
                  currentDownloadingFragment->SetProcessed(true, UINT_MAX);

                  this->streamFragments->UpdateIndexes(this->streamFragmentDownloading, 1);

                  // recalculate start position of all processed stream fragments until first not processed stream fragment
                  this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(this->streamFragmentDownloading);

                  if (nextFragment->IsDownloaded())
                  {
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found next FLV stream fragment with lower timestamp as receiving stream fragment, stopping downloading fragment, current fragment timestamp: %lld, receiving fragment timestamp: %lld, next fragment timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, currentDownloadingFragment->GetFragmentStartTimestamp(), timestamp, nextFragment->GetFragmentStartTimestamp());

                    currentDownloadingFragment->SetDiscontinuity(true, this->streamFragmentDownloading);
                    currentDownloadingFragment = NULL;

                    // request to download first not downloaded stream fragment after current downloaded fragment

                    this->streamFragmentToDownload = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentDownloading);
                    this->streamFragmentDownloading = UINT_MAX;

                    this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE;
                    this->flags |= (this->streamFragmentToDownload != UINT_MAX) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA : MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_NONE;
                  }
                  else
                  {
                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found next not downloaded FLV stream fragment with lower timestamp as receiving stream fragment, continuing downloading fragment, current fragment timestamp: %lld, receiving fragment timestamp: %lld, next fragment timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, currentDownloadingFragment->GetFragmentStartTimestamp(), timestamp, nextFragment->GetFragmentStartTimestamp());

                    nextFragment->SetFragmentStartTimestamp(timestamp);

                    currentDownloadingFragment = nextFragment;
                    this->streamFragmentDownloading++;
                  }
                }
              }

              if ((currentDownloadingFragment != NULL) && (currentDownloadingFragment->GetLength() != 0) && (timestamp != currentDownloadingFragment->GetFragmentStartTimestamp()))
              {
                CRtmpStreamFragment *nextFragment = new CRtmpStreamFragment(&result, timestamp, true);
                CHECK_POINTER_HRESULT(result, nextFragment, result, E_OUTOFMEMORY);

                if (SUCCEEDED(result))
                {
                  result = this->streamFragments->Insert(this->streamFragmentDownloading + 1, nextFragment) ? result : E_OUTOFMEMORY;
                }

                if (SUCCEEDED(result))
                {
                  currentDownloadingFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
                  currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
                  currentDownloadingFragment->SetProcessed(true, UINT_MAX);

                  this->streamFragments->UpdateIndexes(this->streamFragmentDownloading, 1);

                  // recalculate start position of all processed stream fragments until first not processed stream fragment
                  this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(this->streamFragmentDownloading);

                  this->streamFragmentDownloading++;
                }
                else
                {
                  FREE_MEM_CLASS(nextFragment);
                }

                currentDownloadingFragment = nextFragment;
              }

              // add FLV packet to current downloading stream fragment
              if (SUCCEEDED(result) && (currentDownloadingFragment != NULL))
              {
                result = (currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(flvPacket->GetData(), flvPacket->GetSize(), currentDownloadingFragment->GetBuffer()->GetBufferSize() * 2) == flvPacket->GetSize()) ? result : E_OUTOFMEMORY;
              }
            }

            processed += flvPacket->GetSize();
          }
          
          // clear FLV packet and continue with remaining data
          flvPacket->Clear();
        }

        FREE_MEM_CLASS(flvPacket);

        if (processed > 0)
        {
          // removed processed data from CURL instance
          CLockMutex lockData(this->lockCurlMutex, INFINITE);

          this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(processed);
        }
      }

      FREE_MEM(buffer);
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance == NULL) && (this->connectionState == Initializing) && (!this->IsWholeStreamDownloaded()))
    {
      this->connectionState = Initializing;

      int64_t startTime = INT64_MAX;

      // if seeking to empty place (no downloaded fragment before) then use fragment time
      // if seeking to fill gap (one or more downloaded fragment(s) before) then use previous fragment time
      // if seeking in live stream (e.g. lost connection) then use zero time

      if (!this->IsLiveStreamDetected())
      {
        // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentProcessing) : this->streamFragmentToDownload;
        // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0) : this->streamFragmentToDownload;
        // fragment to download still can be UINT_MAX = no fragment to download

        if (this->streamFragmentToDownload != UINT_MAX)
        {
          CRtmpStreamFragment *fragment = this->streamFragments->GetItem(this->streamFragmentToDownload);

          if (this->streamFragmentToDownload > 0)
          {
            CRtmpStreamFragment *previousFragment = this->streamFragments->GetItem(this->streamFragmentToDownload - 1);

            if (previousFragment->IsDownloaded())
            {
              startTime = previousFragment->GetFragmentStartTimestamp();
            }
          }

          startTime = min(startTime, fragment->GetFragmentStartTimestamp());
        }
      }

      startTime = (startTime != INT64_MAX) ? startTime: 0;

      FREE_MEM_CLASS(this->mainCurlInstance);

      // new connection will be created with new timestamps
      // clear set timestamp flag
      this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_FIRST_TIMESTAMP | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_VIDEO_CORRECTION | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SET_AUDIO_CORRECTION);
      this->lastFlvPacketTimestamp = 0;
      this->lastCumulatedFlvTimestamp = 0;
      this->videoTimestampCorrection = 0;
      this->audioTimestampCorrection = 0;

      // clear all not downloaded stream fragments
      // recalculate stream fragments timestams for not downloaded stream fragments

      for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
      {
        CRtmpStreamFragment *fragment = this->streamFragments->GetItem(i);

        if (!fragment->IsDownloaded())
        {
          fragment->GetBuffer()->ClearBuffer();

          fragment->SetFragmentStartTimestamp(fragment->GetFragmentStartTimestamp(), false);
        }
      }

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

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: starting receiving data from timestamp %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, startTime);

      // create CURL instance
      this->mainCurlInstance = new CRtmpCurlInstance(&result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        if (this->configuration->GetValueBool(PARAMETER_NAME_DUMP_INPUT_RAW_DATA, true, PARAMETER_NAME_DUMP_INPUT_RAW_DATA_DEFAULT))
        {
          wchar_t *storeFilePath = this->GetStoreFile(L"dump");
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }
      }

      if (SUCCEEDED(result))
      {
        CRtmpDownloadRequest *request = new CRtmpDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTMP_OPEN_CONNECTION_TIMEOUT, true, RTMP_OPEN_CONNECTION_TIMEOUT_DEFAULT));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

          request->SetUrl(this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL));
          request->SetRtmpApp(this->configuration->GetValue(PARAMETER_NAME_RTMP_APP, true, RTMP_APP_DEFAULT));
          request->SetRtmpArbitraryData(this->configuration->GetValue(PARAMETER_NAME_RTMP_ARBITRARY_DATA, true, NULL));
          request->SetRtmpBuffer(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTMP_BUFFER, true, RTMP_BUFFER_DEFAULT));
          request->SetRtmpFlashVersion(this->configuration->GetValue(PARAMETER_NAME_RTMP_FLASHVER, true, RTMP_FLASH_VER_DEFAULT));
          request->SetRtmpAuth(this->configuration->GetValue(PARAMETER_NAME_RTMP_AUTH, true, RTMP_AUTH_DEFAULT));
          request->SetRtmpJtv(this->configuration->GetValue(PARAMETER_NAME_RTMP_JTV, true, RTMP_JTV_DEFAULT));
          request->SetRtmpLive(this->configuration->GetValueBool(PARAMETER_NAME_RTMP_LIVE, true, RTMP_LIVE_DEFAULT));
          request->SetRtmpPageUrl(this->configuration->GetValue(PARAMETER_NAME_RTMP_PAGE_URL, true, RTMP_PAGE_URL_DEFAULT));
          request->SetRtmpPlaylist(this->configuration->GetValueBool(PARAMETER_NAME_RTMP_PLAYLIST, true, RTMP_PLAYLIST_DEFAULT));
          request->SetRtmpPlayPath(this->configuration->GetValue(PARAMETER_NAME_RTMP_PLAY_PATH, true, RTMP_PLAY_PATH_DEFAULT));
          request->SetRtmpStart(startTime);
          request->SetRtmpStop(RTMP_STOP_DEFAULT);
          request->SetRtmpSubscribe(this->configuration->GetValue(PARAMETER_NAME_RTMP_SUBSCRIBE, true, RTMP_SUBSCRIBE_DEFAULT));
          request->SetRtmpSwfUrl(this->configuration->GetValue(PARAMETER_NAME_RTMP_SWF_URL, true, RTMP_SWF_URL_DEFAULT));
          request->SetRtmpSwfVerify(this->configuration->GetValueBool(PARAMETER_NAME_RTMP_SWF_VERIFY, true, RTMP_SWF_VERIFY_DEFAULT));
          request->SetRtmpTcUrl(this->configuration->GetValue(PARAMETER_NAME_RTMP_TC_URL, true, RTMP_TC_URL_DEFAULT));
          request->SetRtmpToken(this->configuration->GetValue(PARAMETER_NAME_RTMP_TOKEN, true, RTMP_TOKEN_DEFAULT));

          if (SUCCEEDED(this->mainCurlInstance->Initialize(request)))
          {
            // all parameters set
            // start receiving data

            if (SUCCEEDED(this->mainCurlInstance->StartReceivingData()))
            {
              this->connectionState = Opening;

              if (this->streamFragments->Count() == 0)
              {
                // add first stream fragment
                CRtmpStreamFragment *fragment = new CRtmpStreamFragment(&result);
                CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

                fragment->SetFragmentStartPosition(0);
                CHECK_CONDITION_HRESULT(result, this->streamFragments->Insert(0, fragment), result, E_OUTOFMEMORY);

                if (SUCCEEDED(result))
                {
                  // set start searching index to current processing stream fragment
                  this->streamFragments->SetStartSearchingIndex(this->streamFragmentProcessing);
                  // set count of fragments to search for specific position
                  unsigned int firstNotDownloadedFragmentIndex = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentProcessing);
                  this->streamFragments->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? this->streamFragments->Count() : firstNotDownloadedFragmentIndex) - this->streamFragmentProcessing);

                  this->streamFragmentToDownload = 0;
                }
                else
                {
                  FREE_MEM_CLASS(fragment);
                }
              }

              this->streamFragmentDownloading = this->streamFragmentToDownload;
              this->streamFragmentToDownload = UINT_MAX;
            }
            else
            {
              this->connectionState = OpeningFailed;
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
      // check end of stream or error on RTMP connection

      if (SUCCEEDED(this->mainCurlInstance->GetDownloadResponse()->GetResultError()))
      {
        if (!this->IsLiveStreamDetected())
        {
          // check if we downloaded last stream fragment - then we set end of stream reached - it doesn't mean that we have all data (no gaps)

          if (this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
          {
            // all data processed, we can continue
            bool endOfStreamReached = true;

            if (this->streamFragmentDownloading != UINT_MAX)
            {
              // mark currently downloading stream fragment as downloaded
              CRtmpStreamFragment *fragment = this->streamFragments->GetItem(this->streamFragmentDownloading);

              if (fragment != NULL)
              {
                fragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
                fragment->SetDownloaded(true, UINT_MAX);
                fragment->SetProcessed(true, UINT_MAX);

                this->streamFragments->UpdateIndexes(this->streamFragmentDownloading, 1);

                // recalculate start position of all processed stream fragments until first not processed stream fragment
                this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(this->streamFragmentDownloading);
              }

              this->streamFragmentDownloading = UINT_MAX;
            }

            if ((!this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED)) && (this->streamFragments->Count() != 0))
            {
              // all data read from CURL instance, not set end of stream, at least one stream fragment
              CRtmpStreamFragment *lastFragment = this->streamFragments->GetItem(this->streamFragments->Count() - 1);

              endOfStreamReached &= lastFragment->IsDownloaded();
              if (lastFragment->IsDownloaded())
              {
                this->logger->Log(LOGGER_INFO, L"%s: %s: end of stream reached", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
              }
            }

            if (endOfStreamReached)
            {
              this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

              // check if there isn't some fragment to download
              bool startFragmentDownload = false;

              if (this->streamFragmentDownloading == UINT_MAX)
              {
                unsigned int fragmentToDownload = UINT_MAX;

                // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
                fragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentProcessing) : this->streamFragmentToDownload;
                // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
                fragmentToDownload = (fragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0) : fragmentToDownload;
                // fragment to download still can be UINT_MAX = no fragment to download

                this->streamFragmentToDownload = fragmentToDownload;
                startFragmentDownload |= (fragmentToDownload != UINT_MAX);
              }

              if (startFragmentDownload)
              {
                // we need to download at least one stream fragment
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA;
              }
              else
              {
                // no stream fragment to download, we have all data
                this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
              }
            }
          }
        }
        else
        {
          // whole stream downloaded
          this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
          this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"live stream, all data received");
        }
      }
      else
      {
        // error occured while receiving data

        if (this->IsLiveStreamDetected())
        {
          // download stream fragment again or download scheduled stream fragment
          CRtmpStreamFragment *fragment = (this->streamFragmentDownloading > 0) ? this->streamFragments->GetItem(this->streamFragmentDownloading - 1) : NULL;

          if (fragment != NULL)
          {
            // we report discontinuity (if possible)
            fragment->SetDiscontinuity(true, this->streamFragmentDownloading - 1);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, timestamp: %llu, position: %lld, size: %u, current position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragmentStartTimestamp(), fragment->GetFragmentStartPosition(), fragment->GetLength(), this->GetBytePosition());
          }

          this->streamFragmentToDownload = this->streamFragmentDownloading;
          this->streamFragmentDownloading = UINT_MAX;

          // error while receiving data, stop receiving data
          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA;
        }
        else
        {
          // we must set discontinuity flag, because we can't re-open connection exactly on lost position (time)

          // download stream fragment again or download scheduled stream fragment
          CRtmpStreamFragment *fragment = this->streamFragments->GetItem(this->streamFragmentDownloading);

          fragment->SetDiscontinuity(true, this->streamFragmentDownloading);

          this->streamFragmentToDownload = this->streamFragmentDownloading;
          this->streamFragmentDownloading = UINT_MAX;

          // error while receiving data, stop receiving data
          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA;
        }
      }
    }

    if ((!this->IsSetStreamLength()) && (!(this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen())))
    {
      // adjust total length (if necessary)
      if (this->streamLength == 0)
      {
        // stream length not set
        // just make guess

        this->streamLength = MINIMUM_RECEIVED_DATA_FOR_SPLITTER;
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
      }
      else if ((this->GetBytePosition() > (this->streamLength * 3 / 4)))
      {
        // it is time to adjust stream length, we are approaching to end but still we don't know total length
        this->streamLength = this->GetBytePosition() * 2;
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
      }
    }

    if ((!this->IsSetStreamLength()) && (this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen()))
    {
      // reached end of stream, set stream length

      this->streamLength = this->GetBytePosition();
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);

      this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
      this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

      CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE);
    }

    // process stream package (if valid)
    if (streamPackage->GetState() == CStreamPackage::Created)
    {
      HRESULT res = S_OK;
      // stream package is just created, it wasn't processed before
      CStreamPackageDataRequest *dataRequest = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CHECK_CONDITION_HRESULT(res, dataRequest != NULL, res, E_INVALID_STREAM_PACKAGE_REQUEST);

      if (SUCCEEDED(res))
      {
        // set start time of processing request
        // set Waiting state
        // set response

        CStreamPackageDataResponse *response = new CStreamPackageDataResponse(&res);
        CHECK_POINTER_HRESULT(res, response, res, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(res, dataRequest->GetStreamId() == 0, res, E_INVALID_STREAM_ID);

        // allocate memory for response
        CHECK_CONDITION_HRESULT(res, response->GetBuffer()->InitializeBuffer(dataRequest->GetLength()), res, E_OUTOFMEMORY);

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

    if (streamPackage->GetState() == CStreamPackage::Waiting)
    {
      // in Waiting or WaitingIgnoreTimeout state can be request only if request and response are correctly set
      CStreamPackageDataRequest *dataRequest = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());

      if (dataRequest != NULL)
      {
        CStreamPackageDataResponse *dataResponse = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

        // don not clear response buffer, we don't have to copy data again from start position
        // first try to find starting stream fragment (stream fragment which have first data)
        unsigned int foundDataLength = dataResponse->GetBuffer()->GetBufferOccupiedSpace();

        int64_t startPosition = this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SKIP_HEADER_AND_META) ? this->headerAndMetaPacketSize : 0;
        startPosition += dataRequest->GetStart() + foundDataLength;
        
        unsigned int fragmentIndex = this->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);

        while (fragmentIndex != UINT_MAX)
        {
          this->streamFragmentProcessing = fragmentIndex;

          // get stream fragment
          CRtmpStreamFragment *streamFragment = this->streamFragments->GetItem(fragmentIndex);
          CRtmpStreamFragment *startSearchingStreamFragment = this->streamFragments->GetItem(this->streamFragments->GetStartSearchingIndex());

          int64_t streamFragmentRelativeStart = streamFragment->GetFragmentStartPosition() - startSearchingStreamFragment->GetFragmentStartPosition();

          // set copy data start and copy data length
          unsigned int copyDataStart = (startPosition > streamFragmentRelativeStart) ? (unsigned int)(startPosition - streamFragmentRelativeStart) : 0;
          unsigned int copyDataLength = min(streamFragment->GetLength() - copyDataStart, dataRequest->GetLength() - foundDataLength);

          // copy data from stream fragment to response buffer
          if (this->cacheFile->LoadItems(this->streamFragments, fragmentIndex, true, UINT_MAX, (this->lastProcessedSize == 0) ? CACHE_FILE_RELOAD_SIZE : this->lastProcessedSize))
          {
            // memory is allocated while switching from Created to Waiting state, we can't have problem on next line
            dataResponse->GetBuffer()->AddToBufferWithResize(streamFragment->GetBuffer(), copyDataStart, copyDataLength);
          }
          else
          {
            // we can't copy data, try it later
            break;
          }

          // update length of data
          foundDataLength += copyDataLength;
          this->currentProcessedSize += copyDataLength;

          if ((streamFragment->IsDiscontinuity()) && ((dataRequest->GetStart() + dataRequest->GetLength()) >= (streamFragmentRelativeStart + streamFragment->GetLength())))
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, completing request, request '%u', start '%lld', size '%u', found: '%u', fragment start: %lld, fragment length: %u, start searching fragment start: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), foundDataLength, streamFragment->GetFragmentStartPosition(), streamFragment->GetLength(), startSearchingStreamFragment->GetFragmentStartPosition());

            dataResponse->SetDiscontinuity(true);
            break;
          }
          else if (foundDataLength < dataRequest->GetLength())
          {
            // find another stream fragment after end of this stream fragment
            startPosition += copyDataLength;

            // find another stream fragment after end of this stream fragment
            fragmentIndex = this->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);

            if (fragmentIndex != UINT_MAX)
            {
              this->streamFragmentProcessing = fragmentIndex;
            }
          }
          else
          {
            // do not find any more media packets for this request because we have enough data
            break;
          }
        }

        if (foundDataLength < dataRequest->GetLength())
        {
          // found data length is lower than requested
          // check request flags, maybe we can complete request

          if ((dataRequest->IsSetAnyNonZeroDataLength() && (foundDataLength > 0)) ||
            (dataRequest->IsSetAnyDataLength()))
          {
            // request can be completed with any length of available data
            streamPackage->SetCompleted(S_OK);
          }
          else
          {
            if (dataResponse->IsDiscontinuity())
            {
              streamPackage->SetCompleted(S_OK);
            }
            else if (this->IsConnectionLostCannotReopen())
            {
              // connection is lost and we cannot reopen it
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

              streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_CONNECTION_LOST_CANNOT_REOPEN);
            }
            else if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= this->streamLength))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

              streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_NO_MORE_DATA_AVAILABLE);
            }
            else if (this->IsLiveStreamDetected() && (this->connectionState != Opened))
            {
              // we have live stream, we are missing data and we have not opened connection
              // we lost some data, report discontinuity

              dataResponse->SetDiscontinuity(true);
              streamPackage->SetCompleted(S_OK);
            }
          }

          if (streamPackage->GetState() == CStreamPackage::Waiting)
          {
            // no seeking by position is available
            // requested position is probably in stream fragment on the end of searchable stream fragments, between this->streamFragments->GetStartSearchingIndex() and this->streamFragments->GetSearchCount()
            // check if fragment is downloading
            // if fragment is not downloading, then schedule it for download

            unsigned int fragmentIndex = this->streamFragments->GetStartSearchingIndex() + this->streamFragments->GetSearchCount();
            CRtmpStreamFragment *fragment = this->streamFragments->GetItem(fragmentIndex);

            if (fragment == NULL)
            {
              // bad, no such fragment exists, we don't have data

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', not found stream fragment", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
            }

            if ((fragment != NULL) && (!fragment->IsDownloaded()) && (fragmentIndex != this->streamFragmentDownloading) && (fragmentIndex != this->streamFragmentToDownload) && ((this->connectionState == None) || (this->connectionState == Opened)))
            {
              // fragment is not downloaded and also is not downloading currently
              this->streamFragmentDownloading = UINT_MAX;
              this->streamFragmentToDownload = fragmentIndex;

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', stream fragment not downloaded and not downloading, scheduled for download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              if (this->connectionState == Opened)
              {
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA;
              }
            }
          }
        }
        else if (foundDataLength == dataRequest->GetLength())
        {
          // found data length is equal than requested
          streamPackage->SetCompleted(S_OK);
        }
      }
    }

    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE))
    {
      HRESULT res = S_OK;
      this->connectionState = Closing;

      if (this->mainCurlInstance != NULL)
      {
        res = this->mainCurlInstance->StopReceivingData();

        CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_INFO, L"%s: %s: closing connection failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, res));
        CHECK_CONDITION_EXECUTE(SUCCEEDED(res), this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->ClearBuffer());
      }

      if ((res == S_OK) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA)))
      {
        // this clear CURL instance and buffer, it leads to GetConnectionState() to PROTOCOL_CONNECTION_STATE_NONE result and connection will be reopened by ProtocolHoster,
        this->StopReceivingData();
      }

      if (res == S_OK)
      {
        this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"connection closed successfully");
        this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA);
      }
    }

    // store stream fragments to temporary file
    if ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
    {
      this->lastStoreTime = GetTickCount();

      this->lastProcessedSize = this->currentProcessedSize;
      this->currentProcessedSize = 0;

      if (this->streamFragments->Count() > 0)
      {
        // in case of live stream remove all downloaded and processed stream fragments before reported stream time
        if ((this->IsLiveStream()) && (this->reportedStreamTime > 0))
        {
          unsigned int fragmentRemoveStart = (this->streamFragments->GetStartSearchingIndex() == 0) ? 1 : 0;
          unsigned int fragmentRemoveCount = 0;

          while ((fragmentRemoveStart + fragmentRemoveCount) < this->streamFragmentProcessing)
          {
            CRtmpStreamFragment *fragment = this->streamFragments->GetItem(fragmentRemoveStart + fragmentRemoveCount);

            if (((fragmentRemoveStart + fragmentRemoveCount) != this->streamFragments->GetStartSearchingIndex()) && fragment->IsProcessed() && (fragment->GetFragmentStartTimestamp() < (int64_t)this->reportedStreamTime))
            {
              // fragment will be removed
              fragmentRemoveCount++;
            }
            else
            {
              break;
            }
          }

          if ((fragmentRemoveCount > 0) && (this->cacheFile->RemoveItems(this->streamFragments, fragmentRemoveStart, fragmentRemoveCount)))
          {
            unsigned int startSearchIndex = (fragmentRemoveCount > this->streamFragments->GetStartSearchingIndex()) ? 0 : (this->streamFragments->GetStartSearchingIndex() - fragmentRemoveCount);
            unsigned int searchCountDecrease = (fragmentRemoveCount > this->streamFragments->GetStartSearchingIndex()) ? (fragmentRemoveCount - this->streamFragments->GetStartSearchingIndex()) : 0;

            this->streamFragments->SetStartSearchingIndex(startSearchIndex);
            this->streamFragments->SetSearchCount(this->streamFragments->GetSearchCount() - searchCountDecrease);

            this->streamFragments->Remove(fragmentRemoveStart, fragmentRemoveCount);

            this->streamFragmentProcessing -= fragmentRemoveCount;

            if (this->streamFragmentDownloading != UINT_MAX)
            {
              this->streamFragmentDownloading -= fragmentRemoveCount;
            }

            if (this->streamFragmentToDownload != UINT_MAX)
            {
              this->streamFragmentToDownload -= fragmentRemoveCount;
            }
          }
        }

        if (this->cacheFile->GetCacheFile() == NULL)
        {
          wchar_t *storeFilePath = this->GetStoreFile(L"temp");
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }

        // store all stream fragments (which are not stored) to file
        if ((this->cacheFile->GetCacheFile() != NULL) && (this->streamFragments->Count() != 0))
        {
          this->cacheFile->StoreItems(this->streamFragments, this->lastStoreTime, this->IsWholeStreamDownloaded());
        }
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Rtmp::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTMP_OPEN_CONNECTION_TIMEOUT, true, RTMP_OPEN_CONNECTION_TIMEOUT_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Rtmp::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTMP_OPEN_CONNECTION_SLEEP_TIME, true, RTMP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Rtmp::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTMP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, RTMP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT);
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configuration);

  if (SUCCEEDED(result) && (parameters != NULL))
  {
    this->configuration->Append(parameters);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->StopReceivingData());

  this->connectionState = SUCCEEDED(result) ? Initializing : None;

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);
  this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA);

  this->connectionState = None;

  this->streamFragmentDownloading = UINT_MAX;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    streamProgress->SetTotalLength((this->streamLength == 0) ? 1 : this->streamLength);
    streamProgress->SetCurrentLength((this->streamLength == 0) ? 0 : this->GetBytePosition());

    if (this->IsStreamLengthEstimated())
    {
      result = VFW_S_ESTIMATED;
    }
  }

  return result;
}

void CMPUrlSourceSplitter_Protocol_Rtmp::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  __super::ClearSession();
 
  this->streamLength = 0;
  this->connectionState = None;
  this->cacheFile->Clear();
  this->streamFragments->Clear();
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  this->lastFlvPacketTimestamp = 0;
  this->lastCumulatedFlvTimestamp = 0;
  this->headerAndMetaPacketSize = 0;
  this->videoTimestampCorrection = 0;
  this->audioTimestampCorrection = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_Rtmp::GetDuration(void)
{
  uint64_t duration = RTMP_DURATION_UNSPECIFIED;
  if ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetRtmpDownloadResponse()->GetDuration() != RTMP_DURATION_UNSPECIFIED))
  {
    duration = (this->mainCurlInstance->GetRtmpDownloadResponse()->GetDuration() != 0) ? this->mainCurlInstance->GetRtmpDownloadResponse()->GetDuration() : RTMP_DURATION_UNSPECIFIED;
  }

  return this->IsLiveStream() ? DURATION_LIVE_STREAM : ((duration != RTMP_DURATION_UNSPECIFIED) ? (int64_t)duration : DURATION_UNSPECIFIED);
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::GetStreamInformation(CStreamInformationCollection *streams)
{
  // RTMP protocol has always one stream (container)
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

unsigned int CMPUrlSourceSplitter_Protocol_Rtmp::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtmp::SeekToTime(unsigned int streamId, int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  // find fragment to process
  // TO DO: implement better and faster seeking algorithm
  for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
  {
    // first RTMP stream fragment has always zero timestamp
    CRtmpStreamFragment *fragment = this->streamFragments->GetItem(i);
    CRtmpStreamFragment *nextFragment = this->streamFragments->GetItem(i + 1);

    int64_t fragmentTime = fragment->GetFragmentStartTimestamp();
    int64_t nextFragmentTime = (nextFragment == NULL) ? INT64_MAX : nextFragment->GetFragmentStartTimestamp();

    if ((fragmentTime <= time) && (nextFragmentTime >= time))
    {
      this->streamFragmentProcessing = i;

      result = (result == -1) ? fragmentTime : (min(result, fragmentTime));

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: time %lld, fragment: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, fragmentTime, i);
      break;
    }
  }

  if (result != (-1))
  {
    this->flags &= ~(PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SKIP_HEADER_AND_META);
    this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

    // is set stream fragment to process
    // exists at least one stream fragment and cover all possible timestamps (0 - UINT64_MAX)

    this->streamLength = 0;

    CRtmpStreamFragment *previousFragment = (this->streamFragmentProcessing != 0) ? this->streamFragments->GetItem(this->streamFragmentProcessing - 1) : NULL;
    CRtmpStreamFragment *processingFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);

    if (processingFragment->ContainsHeaderOrMetaPacket())
    {
      // we must skip header and meta packet, FFmpeg can't handle them again
      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_SKIP_HEADER_AND_META;
    }

    if (!processingFragment->IsDownloaded())
    {
      // stream fragment is not downloaded, it is gap
      // split stream fragment

      CHECK_CONDITION_NOT_NULL_EXECUTE(previousFragment, previousFragment->SetDiscontinuity(true, this->streamFragmentProcessing - 1));
      result = time;

      // create new fragment within found fragment
      // calculate fragment start timestamp

      // first fragment start timestamp is always zero, no need to correct timestamp
      HRESULT res = S_OK;
      CRtmpStreamFragment *fragment = new CRtmpStreamFragment(&res, time, false);
      CHECK_POINTER_HRESULT(res, fragment, res, E_OUTOFMEMORY)

      result = SUCCEEDED(res) ? result : res;

      if (result >= 0)
      {
        fragment->SetFragmentStartPosition(0);
        this->streamFragmentProcessing = this->streamFragmentProcessing + 1;

        result = this->streamFragments->Insert(this->streamFragmentProcessing, fragment) ? result : E_OUTOFMEMORY;
      }

      if (result >= 0)
      {
        // force to download missing fragment
        this->streamFragmentToDownload = this->streamFragmentProcessing;
      }
      
      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTMP_FLAG_STOP_RECEIVING_DATA;
    }

    // set start searching index to current processing stream fragment
    this->streamFragments->SetStartSearchingIndex(this->streamFragmentProcessing);
    // set count of fragments to search for specific position
    unsigned int firstNotDownloadedFragmentIndex = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentProcessing);

    if (firstNotDownloadedFragmentIndex == UINT_MAX)
    {
      this->streamFragments->SetSearchCount(this->streamFragments->Count() - this->streamFragmentProcessing);
    }
    else
    {
      // clear end of stream reached flag, because we moved to place with gap between processing stream fragment and last stream fragment
      this->flags &= ~PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

      this->streamFragments->SetSearchCount(firstNotDownloadedFragmentIndex - this->streamFragmentProcessing);
    }
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Rtmp::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Rtmp::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::Initialize(CPluginConfiguration *configuration)
{
  CProtocolPluginConfiguration *protocolConfiguration = (CProtocolPluginConfiguration *)configuration;
  HRESULT result = ((this->lockMutex != NULL) && (this->configuration != NULL) && (this->logger != NULL)) ? S_OK : E_NOT_VALID_STATE;
  CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(protocolConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT) ? PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_SPECIFIED : PROTOCOL_PLUGIN_FLAG_NONE;
  }

  return result;
}

/* protected methods */

wchar_t *CMPUrlSourceSplitter_Protocol_Rtmp::GetStoreFile(const wchar_t *extension)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_rtmp_%s.%s", folder, guid, extension);
    }
    FREE_MEM(guid);
  }

  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtmp::GetBytePosition(void)
{
  int64_t result = 0;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    unsigned int first = this->streamFragments->GetStartSearchingIndex();
    unsigned int count = this->streamFragments->GetSearchCount();

    if (count != 0)
    {
      CRtmpStreamFragment *firstFragment = this->streamFragments->GetItem(first);
      CRtmpStreamFragment *lastFragment = this->streamFragments->GetItem(first + count - 1);

      result = (unsigned int)(lastFragment->GetFragmentStartPosition() + lastFragment->GetLength() - firstFragment->GetFragmentStartPosition());
    }
  }

  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtmp::GetFlvPacketTimestamp(unsigned int currentFlvPacketTimestamp)
{
  int64_t difference = ((currentFlvPacketTimestamp < this->lastFlvPacketTimestamp) ? 0x0000000100000000 : 0);
  difference += currentFlvPacketTimestamp;
  difference -= this->lastFlvPacketTimestamp;

  if (currentFlvPacketTimestamp < this->lastFlvPacketTimestamp)
  {
    // try to identify if overflow occured or FLV timestamp is only slightly decreased
    uint64_t diff = this->lastFlvPacketTimestamp - currentFlvPacketTimestamp;

    // on this place is difference always greater than or equal to zero, we can safely cast it to uint64_t
    if (diff < (uint64_t)difference)
    {
      // FLV timestamp decrease is more probable than overflow
      difference -= 0x0000000100000000;
    }
  }

  int64_t result = this->lastCumulatedFlvTimestamp + difference;

  this->lastCumulatedFlvTimestamp += difference;
  this->lastFlvPacketTimestamp = currentFlvPacketTimestamp;

  return result;
}