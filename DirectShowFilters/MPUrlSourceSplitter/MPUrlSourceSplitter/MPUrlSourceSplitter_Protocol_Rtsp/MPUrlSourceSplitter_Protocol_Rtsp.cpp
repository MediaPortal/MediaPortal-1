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

#include "MPUrlSourceSplitter_Protocol_Rtsp.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "MPUrlSourceSplitter_Protocol_Rtsp_Parameters.h"
#include "Parameters.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "StreamPackagePacketRequest.h"
#include "StreamPackagePacketResponse.h"

#include "NormalPlayTimeRangeAttribute.h"

#include <WinInet.h>
#include <stdio.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Rtspd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Rtsp"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Rtsp(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Rtsp *protocol = (CMPUrlSourceSplitter_Protocol_Rtsp *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Rtsp::CMPUrlSourceSplitter_Protocol_Rtsp(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->lockMutex = NULL;
  this->lockCurlMutex = NULL;
  this->mainCurlInstance = NULL;
  this->streamTracks = NULL;
  this->lastStoreTime = 0;
  this->sessionDescription = NULL;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->streamTracks = new CRtspStreamTrackCollection(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streamTracks, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP);
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

CMPUrlSourceSplitter_Protocol_Rtsp::~CMPUrlSourceSplitter_Protocol_Rtsp()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->streamTracks);
  FREE_MEM_CLASS(this->sessionDescription);

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

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Rtsp::GetConnectionState(void)
{
  return this->connectionState;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ReceiveData(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->lockMutex, INFINITE);

    if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && ((this->connectionState == Opening) || (this->connectionState == Opened)))
    {
      CRtpPacketCollection *rtpPackets = new CRtpPacketCollection(&result);
      CHECK_POINTER_HRESULT(result, rtpPackets, result, E_OUTOFMEMORY);

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
      {
        CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
        CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

        {
          // copy RTP packets to avoid blocking of RTSP download instance
          CLockMutex lockData(this->lockCurlMutex, INFINITE);

          rtpPackets->Append(track->GetRtpPackets());
          track->GetRtpPackets()->Clear();
        }

        if (rtpPackets->Count() != 0)
        {
          this->connectionState = Opened;
          streamTrack->SetLastReceiveDataTime(GetTickCount());

          // split or add received RTP packets to stream fragments
          CRtspStreamFragment *currentDownloadingFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());

          for (unsigned int j = 0; (SUCCEEDED(result) && (currentDownloadingFragment != NULL) && (j < rtpPackets->Count())); j++)
          {
            CRtpPacket *rtpPacket = rtpPackets->GetItem(j);

            // set first RTP packet timestamp (if not already set)
            // timestamp is needed to compute (almost - after seeking we only assume that timestamps are correct) correct timestamps of fragments
            if (!streamTrack->IsSetFirstRtpPacketTimestamp())
            {
              unsigned int ticks = GetTickCount();
              streamTrack->SetFirstRtpPacketTimestamp(rtpPacket->GetTimestamp(), true, ticks);

              // change current downloading fragment RTP timestamp to correct timestamp based on time from start of stream (first time set ticks to track)
              if (this->IsLiveStream())
              {
                int64_t timestamp = ((int64_t)ticks - (int64_t)streamTrack->GetFirstRtpPacketTicks()) * streamTrack->GetClockFrequency() / 1000;

                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, fragment timestamp changing from %lld to %lld, ticks: %u, track ticks: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), timestamp, ticks, streamTrack->GetFirstRtpPacketTicks());
                currentDownloadingFragment->SetFragmentRtpTimestamp(timestamp, false);
              }

              // compute correction for actual stream track
              int64_t correction = currentDownloadingFragment->GetFragmentRtpTimestamp() - (int64_t)rtpPacket->GetTimestamp();

              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, fragment timestamp: %lld, first RTP packet timestamp: %u, RTP: %u, ticks: %u, track ticks: %u, correction: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), rtpPacket->GetTimestamp(), rtpPacket->GetSequenceNumber(), ticks, streamTrack->GetFirstRtpPacketTicks(), correction);
              streamTrack->SetRtpTimestampCorrection(correction);
            }

            int64_t timestamp = streamTrack->GetRtpPacketTimestamp(rtpPacket->GetTimestamp()) + streamTrack->GetRtpTimestampCorrection();

            if (!currentDownloadingFragment->IsSetFragmentRtpTimestamp())
            {
              currentDownloadingFragment->SetFragmentRtpTimestamp(timestamp);
            }

            CRtspStreamFragment *nextFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading() + 1);

            // nextFragment can be NULL in case that we are on the end of collection

            if (nextFragment != NULL)
            {
              if (timestamp >= nextFragment->GetFragmentRtpTimestamp())
              {
                // our RTP packet timestamp is greater than or equal to next fragment timestamp
                // this means that we are receiving data, which we already have - in case if next fragment is downloaded

                currentDownloadingFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
                currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
                currentDownloadingFragment->SetProcessed(true, UINT_MAX);

                streamTrack->GetStreamFragments()->UpdateIndexes(streamTrack->GetStreamFragmentDownloading(), 1);

                // recalculate start position of all processed stream fragments until first not processed stream fragment
                streamTrack->GetStreamFragments()->RecalculateProcessedStreamFragmentStartPosition(streamTrack->GetStreamFragmentDownloading());

                if (nextFragment->IsDownloaded())
                {
                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, found next RTSP stream fragment with lower timestamp as receiving stream fragment, stopping downloading fragment, current fragment timestamp: %lld, receiving fragment timestamp: %lld, next fragment timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), timestamp, nextFragment->GetFragmentRtpTimestamp());

                  currentDownloadingFragment->SetDiscontinuity(true, streamTrack->GetStreamFragmentDownloading());
                  currentDownloadingFragment = NULL;

                  // request to download first not downloaded stream fragment after current downloaded fragment
                  // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
                  streamTrack->SetStreamFragmentToDownload((streamTrack->GetStreamFragmentToDownload() == UINT_MAX) ? streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragmentIndex(streamTrack->GetStreamFragmentDownloading()) : streamTrack->GetStreamFragmentToDownload());
                  // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
                  streamTrack->SetStreamFragmentToDownload((streamTrack->GetStreamFragmentToDownload() == UINT_MAX) ? streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragmentIndex(0) : streamTrack->GetStreamFragmentToDownload());
                  // fragment to download still can be UINT_MAX = no fragment to download

                  this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE;
                  this->flags |= (streamTrack->GetStreamFragmentToDownload() != UINT_MAX) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA : (PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED);
                }
                else
                {
                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, found next not downloaded RTSP stream fragment with lower timestamp as receiving stream fragment, continuing downloading fragment, current fragment timestamp: %lld, receiving fragment timestamp: %lld, next fragment timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), timestamp, nextFragment->GetFragmentRtpTimestamp());

                  nextFragment->SetFragmentRtpTimestamp(timestamp);

                  currentDownloadingFragment = nextFragment;
                  streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
                }
              }
            }

            if ((currentDownloadingFragment != NULL) && (timestamp != currentDownloadingFragment->GetFragmentRtpTimestamp()))
            {
              CRtspStreamFragment *nextFragment = new CRtspStreamFragment(&result, timestamp, true);
              CHECK_POINTER_HRESULT(result, nextFragment, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                result = streamTrack->GetStreamFragments()->Insert(streamTrack->GetStreamFragmentDownloading() + 1, nextFragment) ? result : E_OUTOFMEMORY;
              }

              if (SUCCEEDED(result))
              {
                currentDownloadingFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
                currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
                currentDownloadingFragment->SetProcessed(true, UINT_MAX);

                streamTrack->GetStreamFragments()->UpdateIndexes(streamTrack->GetStreamFragmentDownloading(), 1);

                // recalculate start position of all processed stream fragments until first not processed stream fragment
                streamTrack->GetStreamFragments()->RecalculateProcessedStreamFragmentStartPosition(streamTrack->GetStreamFragmentDownloading());

                streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
              }
              else
              {
                FREE_MEM_CLASS(nextFragment);
              }

              currentDownloadingFragment = nextFragment;
            }

            // add RTP packet to current downloading stream fragment
            if (SUCCEEDED(result) && (currentDownloadingFragment != NULL))
            {
              result = (currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize(), currentDownloadingFragment->GetBuffer()->GetBufferSize() * 2) == rtpPacket->GetPayloadSize()) ? result : E_OUTOFMEMORY;
            }
          }
        }

        rtpPackets->Clear();
      }

      FREE_MEM_CLASS(rtpPackets);
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance == NULL) && (this->connectionState == Initializing) && (!this->IsWholeStreamDownloaded()))
    {
      this->connectionState = Initializing;

      int64_t startTime = INT64_MAX;

      // if seeking to empty place (no downloaded fragment before) then use fragment time
      // if seeking to fill gap (one or more downloaded fragment(s) before) then use previous fragment time
      // if seeking in live stream (e.g. lost connection) then use zero time

      for (unsigned int i = 0; (SUCCEEDED(result) && (!this->IsLiveStreamDetected()) && (i < this->streamTracks->Count())); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);
        unsigned int fragmentToDownload = track->GetStreamFragmentToDownload();

        if (fragmentToDownload != UINT_MAX)
        {
          CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(fragmentToDownload);

          int64_t fragmentRtpTimestamp = INT64_MAX;

          if (fragmentToDownload > 0)
          {
            CRtspStreamFragment *previousFragment = track->GetStreamFragments()->GetItem(fragmentToDownload - 1);

            if (previousFragment->IsDownloaded())
            {
              fragmentRtpTimestamp = min(previousFragment->GetFragmentRtpTimestamp(), fragmentRtpTimestamp);
            }
          }

          fragmentRtpTimestamp = min(fragment->GetFragmentRtpTimestamp(), fragmentRtpTimestamp);
          fragmentRtpTimestamp *= 1000;
          fragmentRtpTimestamp /= track->GetClockFrequency();

          startTime = min(startTime, fragmentRtpTimestamp);
        }
      }

      startTime = (startTime != INT64_MAX) ? startTime: 0;

      FREE_MEM_CLASS(this->mainCurlInstance);
      FREE_MEM_CLASS(this->sessionDescription);

      // new connection will be created with new RTP packet timestamps
      // clear set RTP packet timestamp flags
      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        track->SetFirstRtpPacketTimestamp(UINT_MAX, false, 0);
        track->SetReceivedAllDataFlag(false);

        // clear all not downloaded stream fragments
        // recalculate stream fragments timestams for not downloaded stream fragments
        for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
        {
          CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);

          if (!fragment->IsDownloaded())
          {
            fragment->GetBuffer()->ClearBuffer();

            fragment->SetFragmentRtpTimestamp(fragment->GetFragmentRtpTimestamp(), false);
          }
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

      this->mainCurlInstance = new CRtspCurlInstance(&result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // set connection priorities
        this->mainCurlInstance->SetMulticastPreference(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_MULTICAST_PREFERENCE, true, RTSP_MULTICAST_PREFERENCE_DEFAULT));
        this->mainCurlInstance->SetSameConnectionTcpPreference(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_SAME_CONNECTION_TCP_PREFERENCE, true, RTSP_SAME_CONNECTION_TCP_PREFERENCE_DEFAULT));
        this->mainCurlInstance->SetUdpPreference(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_UDP_PREFERENCE, true, RTSP_UDP_PREFERENCE_DEFAULT));

        // set ports
        this->mainCurlInstance->SetRtspClientPortMin(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MIN, true, RTSP_CLIENT_PORT_MIN_DEFAULT));
        this->mainCurlInstance->SetRtspClientPortMax(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MAX, true, RTSP_CLIENT_PORT_MAX_DEFAULT));

        this->mainCurlInstance->SetIgnoreRtpPayloadType(this->configuration->GetValueBool(PARAMETER_NAME_RTSP_IGNORE_RTP_PAYLOAD_TYPE, true, RTSP_IGNORE_RTP_PAYLOAD_TYPE_DEFAULT));

        if (this->configuration->GetValueBool(PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA, true, PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA_DEFAULT))
        {
          wchar_t *storeFilePath = this->GetStoreFile(0, L"dump");
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }
      }

      if (SUCCEEDED(result))
      {
        CRtspDownloadRequest *request = new CRtspDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          request->SetStartTime(startTime);
          request->SetUrl(this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL));

          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_OPEN_CONNECTION_TIMEOUT, true, RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

          if (SUCCEEDED(this->mainCurlInstance->Initialize(request)))
          {
            if (this->sessionDescription == NULL)
            {
              const wchar_t *rawSDP = this->mainCurlInstance->GetRtspDownloadResponse()->GetRawSessionDescription();
              unsigned int rawSDPlength = wcslen(rawSDP);

              this->sessionDescription = new CSessionDescription(&result);
              CHECK_POINTER_HRESULT(result, this->sessionDescription, result, E_OUTOFMEMORY);
              CHECK_CONDITION_HRESULT(result, this->sessionDescription->Parse(rawSDP, rawSDPlength), result, E_RTSP_SESSION_DESCRIPTION_PARSE_ERROR);
            }

            if (SUCCEEDED(result))
            {
              // check for normal play time range attribute in session description
              // if normal play time range attribute has start and end time, then session is not live session

              CNormalPlayTimeRangeAttribute *nptAttribute = NULL;
              for (unsigned int i = 0; i < this->sessionDescription->GetAttributes()->Count(); i++)
              {
                CAttribute *attribute = this->sessionDescription->GetAttributes()->GetItem(i);

                if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_NORMAL_PLAY_TIME_RANGE))
                {
                  nptAttribute = dynamic_cast<CNormalPlayTimeRangeAttribute *>(attribute);
                  break;
                }
              }

              // RTSP is by default for live streams
              this->flags |= PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED;

              if (nptAttribute != NULL)
              {
                if (nptAttribute->IsSetStartTime() && nptAttribute->IsSetEndTime() && (nptAttribute->GetEndTime() >= nptAttribute->GetStartTime()))
                {
                  // not live stream
                  this->flags &= ~PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED;
                }
              }
            }

            // all parameters set
            // start receiving data

            if (SUCCEEDED(this->mainCurlInstance->StartReceivingData()))
            {
              this->connectionState = Opening;

              // create same count of RTSP stream tracks as RTSP tracks in CURL instance
              CLockMutex(this->lockCurlMutex, INFINITE);

              if (this->streamTracks->Count() != this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())
              {
                this->streamTracks->Clear();

                for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
                {
                  CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
                  CRtspStreamTrack *streamTrack = new CRtspStreamTrack(&result);
                  CHECK_POINTER_HRESULT(result, streamTrack, result, E_OUTOFMEMORY);

                  streamTrack->SetLastReceiveDataTime(GetTickCount());
                  streamTrack->SetClockFrequency(track->GetStatistics()->GetClockFrequency());
                  streamTrack->SetReceivedAllDataFlag(false);

                  CHECK_CONDITION_HRESULT(result, this->streamTracks->Add(streamTrack), result, E_OUTOFMEMORY);
                  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamTrack));
                }

                // for each stream track add first stream fragment
                for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
                {
                  CRtspStreamTrack *track = this->streamTracks->GetItem(i);

                  CRtspStreamFragment *fragment = new CRtspStreamFragment(&result);
                  CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

                  fragment->SetFragmentStartPosition(0);
                  CHECK_CONDITION_HRESULT(result, track->GetStreamFragments()->Insert(0, fragment), result, E_OUTOFMEMORY);

                  if (SUCCEEDED(result))
                  {
                    // set start searching index to current processing stream fragment
                    track->GetStreamFragments()->SetStartSearchingIndex(track->GetStreamFragmentProcessing());
                    // set count of fragments to search for specific position
                    unsigned int firstNotDownloadedFragmentIndex = track->GetStreamFragments()->GetFirstNotDownloadedStreamFragmentIndex(track->GetStreamFragmentProcessing());
                    track->GetStreamFragments()->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? track->GetStreamFragments()->Count() : firstNotDownloadedFragmentIndex) - track->GetStreamFragmentProcessing());

                    track->SetStreamFragmentToDownload(0);
                  }
                  else
                  {
                    FREE_MEM_CLASS(fragment);
                  }
                }
              }

              // set stream fragment downloading for each track (based on start time)
              for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
              {
                CRtspStreamTrack *track = this->streamTracks->GetItem(i);

                track->SetStreamFragmentDownloading(track->GetStreamFragmentToDownload());

                // we are downloading stream fragment, so reset scheduled download
                track->SetStreamFragmentToDownload(UINT_MAX);
              }
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

    if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()) && (!(this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE))) && (!(this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA))) && (this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    {
      // all data received, we're not receiving data
      // check end of stream or error on RTSP connection

      if (SUCCEEDED(this->mainCurlInstance->GetDownloadResponse()->GetResultError()))
      {
        if (!this->IsLiveStreamDetected())
        {
          // very rare, but possible
          // check if we downloaded last stream fragment - then we set end of stream reached - it doesn't mean that we have all data (no gaps)

          bool allEndOfStreamReached = true;
          for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
          {
            CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
            CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

            allEndOfStreamReached &= (track->GetRtpPackets()->Count() == 0);
            if (track->GetRtpPackets()->Count() == 0)
            {
              if (track->IsEndOfStream() && (streamTrack->GetStreamFragmentDownloading() != UINT_MAX))
              {
                // mark currently downloading stream fragment as downloaded
                CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());
                if (fragment != NULL)
                {
                  fragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
                  fragment->SetDownloaded(true, UINT_MAX);
                  fragment->SetProcessed(true, UINT_MAX);

                  streamTrack->GetStreamFragments()->UpdateIndexes(streamTrack->GetStreamFragmentDownloading(), 1);

                  // recalculate start position of all processed stream fragments until first not processed stream fragment
                  streamTrack->GetStreamFragments()->RecalculateProcessedStreamFragmentStartPosition(streamTrack->GetStreamFragmentDownloading());
                }

                streamTrack->SetStreamFragmentDownloading(UINT_MAX);
              }

              if ((!streamTrack->IsSetEndOfStream()) && (streamTrack->GetStreamFragments()->Count() != 0))
              {
                // all data read from CURL instance for track i, not set end of stream, at least one stream fragment
                CRtspStreamFragment *lastFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragments()->Count() - 1);

                allEndOfStreamReached &= lastFragment->IsDownloaded();
                if (lastFragment->IsDownloaded())
                {
                  this->logger->Log(LOGGER_INFO, L"%s: %s: track %u, end of stream reached", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i);
                  streamTrack->SetEndOfStreamFlag(true);
                }
              }
            }
          }

          if (allEndOfStreamReached)
          {
            this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

            // check if there isn't some fragment to download
            bool anyTrackDownloading = false;
            bool startFragmentDownload = false;

            for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
            {
              CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

              anyTrackDownloading |= (streamTrack->GetStreamFragmentDownloading() != UINT_MAX);

              if (streamTrack->GetStreamFragmentDownloading() == UINT_MAX)
              {
                unsigned int fragmentToDownload = UINT_MAX;

                // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
                fragmentToDownload = (streamTrack->GetStreamFragmentToDownload() == UINT_MAX) ? streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragmentIndex(streamTrack->GetStreamFragmentProcessing()) : streamTrack->GetStreamFragmentToDownload();
                // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
                fragmentToDownload = (fragmentToDownload == UINT_MAX) ? streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragmentIndex(0) : fragmentToDownload;
                // fragment to download still can be UINT_MAX = no fragment to download

                streamTrack->SetStreamFragmentToDownload(fragmentToDownload);
                startFragmentDownload |= (fragmentToDownload != UINT_MAX);
              }
            }

            if ((!anyTrackDownloading) && (startFragmentDownload))
            {
              // no track is downloading, but we need to download at least one stream fragment
              this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA;
            }
            else if (!(anyTrackDownloading || startFragmentDownload))
            {
              // no track is downloading, no stream fragment to download, we have all data
              this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
            }
          }
        }
        else
        {
          // more common case, whole stream downloaded

          bool allEndOfStreamReached = true;
          for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
          {
            CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
            CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

            allEndOfStreamReached &= (track->GetRtpPackets()->Count() == 0);
            if (track->GetRtpPackets()->Count() == 0)
            {
              if (track->IsEndOfStream() && (streamTrack->GetStreamFragmentDownloading() != UINT_MAX))
              {
                // mark currently downloading stream fragment as downloaded
                CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());
                if (fragment != NULL)
                {
                  fragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
                  fragment->SetDownloaded(true, UINT_MAX);
                  fragment->SetProcessed(true, UINT_MAX);

                  streamTrack->GetStreamFragments()->UpdateIndexes(streamTrack->GetStreamFragmentDownloading(), 1);

                  // recalculate start position of all processed stream fragments until first not processed stream fragment
                  streamTrack->GetStreamFragments()->RecalculateProcessedStreamFragmentStartPosition(streamTrack->GetStreamFragmentDownloading());
                }

                streamTrack->SetStreamFragmentDownloading(UINT_MAX);
              }

              if ((!streamTrack->IsSetEndOfStream()) && (streamTrack->GetStreamFragments()->Count() != 0))
              {
                // all data read from CURL instance for track i, not set end of stream, at least one stream fragment
                CRtspStreamFragment *lastFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragments()->Count() - 1);

                allEndOfStreamReached &= lastFragment->IsDownloaded();
                if (lastFragment->IsDownloaded())
                {
                  this->logger->Log(LOGGER_INFO, L"%s: %s: track %u, end of stream reached", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i);
                  streamTrack->SetEndOfStreamFlag(true);
                }
              }
            }
          }

          if (allEndOfStreamReached)
          {
            this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
            this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"live stream, all data received");
          }
        }
      }
      else
      {
        // error occured while receiving data
        
        if (this->IsLiveStreamDetected())
        {
          // more common case

          // download stream fragment again or download scheduled stream fragment
          for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
          {
            CRtspStreamTrack *track = this->streamTracks->GetItem(i);

            CRtspStreamFragment *fragment = (track->GetStreamFragmentDownloading() > 0) ? track->GetStreamFragments()->GetItem(track->GetStreamFragmentDownloading() - 1) : NULL;

            if (fragment != NULL)
            {
              // we report discontinuity (if possible)
              fragment->SetDiscontinuity(true, track->GetStreamFragmentDownloading());
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, RTP timestamp: %lld, position: %lld, size: %u, current track position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragmentRtpTimestamp(), fragment->GetFragmentStartPosition(), fragment->GetLength(), track->GetBytePosition());
            }

            track->SetStreamFragmentToDownload(track->GetStreamFragmentDownloading());
            track->SetStreamFragmentDownloading(UINT_MAX);
          }

          // error while receiving data, stop receiving data
          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA;
        }
        else
        {
          // we must set discontinuity flag, because we can't re-open connection exactly on lost position (time)

          // download stream fragment again or download scheduled stream fragment
          for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
          {
            CRtspStreamTrack *track = this->streamTracks->GetItem(i);
            CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentDownloading());

            fragment->SetDiscontinuity(true, track->GetStreamFragmentDownloading());

            track->SetStreamFragmentToDownload(track->GetStreamFragmentDownloading());
            track->SetStreamFragmentDownloading(UINT_MAX);
          }

          // error while receiving data, stop receiving data
          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA;
        }
      }
    }

    if ((!this->IsSetStreamLength()) && (!(this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen())))
    {
      for (unsigned int i = 0; (i < this->streamTracks->Count()); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        // adjust total length (if necessary)
        if (!track->IsSetStreamLength())
        {
          if (track->GetStreamLength() == 0)
          {
            // stream length not set
            // just make guess

            track->SetStreamLength((int64_t)MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting guess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
          }
          else if ((track->GetBytePosition() > (track->GetStreamLength() * 3 / 4)))
          {
            // it is time to adjust stream length, we are approaching to end but still we don't know total length
            track->SetStreamLength(track->GetBytePosition() * 2);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting guess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
          }
        }
      }
    }

    if ((!this->IsSetStreamLength()) && (this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen()))
    {
      // reached end of stream, set stream length

      for (unsigned int i = 0; (i < this->streamTracks->Count()); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        if (!track->IsSetStreamLength())
        {
          track->SetStreamLength(track->GetBytePosition());
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
          track->SetStreamLengthFlag(true);
        }

        if (!track->IsSetEndOfStream())
        {
          track->SetEndOfStreamFlag(true);
        }
      }

      this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
      this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

      CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE);
    }

    // process stream package (if valid)
    if (streamPackage->GetState() == CStreamPackage::Created)
    {
      HRESULT res = S_OK;
      // stream package is just created, it wasn't processed before
      CStreamPackageDataRequest *dataRequest = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CStreamPackagePacketRequest *packetRequest = dynamic_cast<CStreamPackagePacketRequest *>(streamPackage->GetRequest());

      CHECK_CONDITION_HRESULT(res, (dataRequest != NULL) || (packetRequest != NULL), res, E_INVALID_STREAM_PACKAGE_REQUEST);

      if (SUCCEEDED(res) && (dataRequest != NULL))
      {
        // set start time of processing request
        // set Waiting state
        // set response

        CStreamPackageDataResponse *response = new CStreamPackageDataResponse(&res);
        CHECK_POINTER_HRESULT(res, response, res, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(res, dataRequest->GetStreamId() < this->streamTracks->Count(), res, E_INVALID_STREAM_ID);

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
      else if (SUCCEEDED(res) && (packetRequest != NULL))
      {
        CStreamPackagePacketResponse *response = new CStreamPackagePacketResponse(&res);
        CHECK_POINTER_HRESULT(res, response, res, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(res, packetRequest->GetStreamId() < this->streamTracks->Count(), res, E_INVALID_STREAM_ID);

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
      CStreamPackagePacketRequest *packetRequest = dynamic_cast<CStreamPackagePacketRequest *>(streamPackage->GetRequest());

      if (dataRequest != NULL)
      {
        CStreamPackageDataResponse *dataResponse = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

        CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(dataRequest->GetStreamId());

        // don not clear response buffer, we don't have to copy data again from start position
        // first try to find starting stream fragment (stream fragment which have first data)
        unsigned int foundDataLength = dataResponse->GetBuffer()->GetBufferOccupiedSpace();

        int64_t startPosition = dataRequest->GetStart() + foundDataLength;
        unsigned int fragmentIndex = streamTrack->GetStreamFragments()->GetStreamFragmentIndexBetweenPositions(startPosition);

        while (fragmentIndex != UINT_MAX)
        {
          streamTrack->SetStreamFragmentProcessing(fragmentIndex);

          // get stream fragment
          CRtspStreamFragment *streamFragment = streamTrack->GetStreamFragments()->GetItem(fragmentIndex);
          CRtspStreamFragment *startSearchingStreamFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragments()->GetStartSearchingIndex());

          int64_t streamFragmentRelativeStart = streamFragment->GetFragmentStartPosition() - startSearchingStreamFragment->GetFragmentStartPosition();

          // set copy data start and copy data length
          unsigned int copyDataStart = (startPosition > streamFragmentRelativeStart) ? (unsigned int)(startPosition - streamFragmentRelativeStart) : 0;
          unsigned int copyDataLength = min(streamFragment->GetLength() - copyDataStart, dataRequest->GetLength() - foundDataLength);

          // copy data from stream fragment to response buffer
          if (streamTrack->GetCacheFile()->LoadItems(streamTrack->GetStreamFragments(), fragmentIndex, true, UINT_MAX, (streamTrack->GetLastProcessedSize() == 0) ? CACHE_FILE_RELOAD_SIZE : streamTrack->GetLastProcessedSize()))
          {
            // memory is allocated while switching from Created to Waiting state, we can't have problem on next line
            dataResponse->GetBuffer()->AddToBufferWithResize(streamFragment->GetBuffer(), copyDataStart, copyDataLength);

            // update fragment loaded to memory time to avoid its freeing from memory
            streamFragment->SetLoadedToMemoryTime(GetTickCount(), fragmentIndex);
          }
          else
          {
            // we can't copy data, try it later
            break;
          }

          // update length of data
          foundDataLength += copyDataLength;
          streamTrack->SetCurrentProcessedSize(streamTrack->GetCurrentProcessedSize() + copyDataLength);

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
            fragmentIndex = streamTrack->GetStreamFragments()->GetStreamFragmentIndexBetweenPositions(startPosition);

            if (fragmentIndex != UINT_MAX)
            {
              streamTrack->SetStreamFragmentProcessing(fragmentIndex);
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

          if ((dataRequest->IsSetAnyNonZeroDataLength() || dataRequest->IsSetAnyDataLength()) && (foundDataLength > 0))
          {
            // set connection lost and no more data available flags
            if (this->IsConnectionLostCannotReopen())
            {
              // connection is lost and we cannot reopen it
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), streamTrack->GetStreamLength());

              dataResponse->SetConnectionLostCannotReopen(true);
            }

            if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= streamTrack->GetStreamLength()))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), streamTrack->GetStreamLength());

              dataResponse->SetNoMoreDataAvailable(true);
            }

            // request can be completed with any length of available data
            streamPackage->SetCompleted(S_OK);
          }
          else if (dataRequest->IsSetAnyDataLength() && (foundDataLength == 0))
          {
            // no data available, check end of stream and connection lost

            if (this->IsConnectionLostCannotReopen())
            {
              // connection is lost and we cannot reopen it
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), streamTrack->GetStreamLength());

              dataResponse->SetConnectionLostCannotReopen(true);
              streamPackage->SetCompleted(S_OK);
            }
            else if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= streamTrack->GetStreamLength()))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), streamTrack->GetStreamLength());

              dataResponse->SetNoMoreDataAvailable(true);
              streamPackage->SetCompleted(S_OK);
            }
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
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), streamTrack->GetStreamLength());

              dataResponse->SetConnectionLostCannotReopen(true);
              streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_CONNECTION_LOST_CANNOT_REOPEN);
            }
            else if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= streamTrack->GetStreamLength()))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), streamTrack->GetStreamLength());

              dataResponse->SetNoMoreDataAvailable(true);
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
            // requested position is probably in stream fragment on the end of searchable stream fragments, between streamTrack->GetStreamFragments()->GetStartSearchingIndex() and streamTrack->GetStreamFragments()->GetSearchCount()
            // check if fragment is downloading
            // if fragment is not downloading, then schedule it for download

            unsigned int fragmentIndex = streamTrack->GetStreamFragments()->GetStartSearchingIndex() + streamTrack->GetStreamFragments()->GetSearchCount();
            CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(fragmentIndex);

            if (fragment == NULL)
            {
              // bad, no such fragment exists, we don't have data

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', not found stream fragment", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              dataResponse->SetNoMoreDataAvailable(true);
              streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
            }

            if ((fragment != NULL) && (!fragment->IsDownloaded()) && (fragmentIndex != streamTrack->GetStreamFragmentDownloading()) && ((this->connectionState == None) || (this->connectionState == Opened)))
            {
              // fragment is not downloaded and also is not downloading currently
              streamTrack->SetStreamFragmentDownloading(UINT_MAX);
              streamTrack->SetStreamFragmentToDownload(fragmentIndex);

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', stream fragment not downloaded and not downloading, scheduled for download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              if (this->connectionState == Opened)
              {
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA;
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
      else if (packetRequest != NULL)
      {
        CStreamPackagePacketResponse *packetResponse = dynamic_cast<CStreamPackagePacketResponse *>(streamPackage->GetResponse());
        CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(packetRequest->GetStreamId());

        if (packetRequest->IsResetPacketCounter())
        {
          streamTrack->SetStreamFragmentProcessing(streamTrack->GetStreamFragments()->GetStartSearchingIndex()); 
        }

        CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentProcessing());

        if ((fragment != NULL) && fragment->IsDownloaded())
        {
          // set discontinuity flag (if set on stream fragment)
          packetResponse->SetDiscontinuity(fragment->IsDiscontinuity());

          // clone media packet from stream fragments
          if (streamTrack->GetCacheFile()->LoadItems(streamTrack->GetStreamFragments(), streamTrack->GetStreamFragmentProcessing(), true, false))
          {
            int64_t pts = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentProcessing())->GetFragmentRtpTimestamp();
            pts = streamTrack->GetRtpPacketTimestampInDshowTimeBaseUnits(pts);

            packetResponse->GetMediaPacket()->SetPresentationTimestamp(pts);

            if (packetResponse->GetMediaPacket()->GetBuffer()->InitializeBuffer(fragment->GetBuffer()->GetBufferOccupiedSpace()))
            {
              packetResponse->GetMediaPacket()->GetBuffer()->AddToBufferWithResize(fragment->GetBuffer());

              streamTrack->SetStreamFragmentProcessing(streamTrack->GetStreamFragmentProcessing() + 1);
              streamPackage->SetCompleted(S_OK);
            }
            else
            {
              streamPackage->SetCompleted(E_OUTOFMEMORY);
            }
          }
        }

        if (streamPackage->GetState() == CStreamPackage::Waiting)
        {
          if (packetResponse->IsDiscontinuity())
          {
            streamPackage->SetCompleted(S_OK);
          }
          else if (this->IsConnectionLostCannotReopen())
          {
            // connection is lost and we cannot reopen it
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', stream '%u'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, packetRequest->GetId(), packetRequest->GetStreamId());

            packetResponse->SetConnectionLostCannotReopen(true);
            streamPackage->SetCompleted(E_CONNECTION_LOST_CANNOT_REOPEN);
          }
          else if (this->IsEndOfStreamReached() && (streamTrack->GetStreamFragmentProcessing() >= streamTrack->GetStreamFragments()->Count()))
          {
            // we are not receiving more data, complete request
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', stream '%u'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, packetRequest->GetId(), packetRequest->GetStreamId());

            packetResponse->SetNoMoreDataAvailable(true);
            streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
          }
          else if (this->IsLiveStreamDetected() && (this->connectionState != Opened))
          {
            // we have live stream, we are missing data and we have not opened connection
            // we lost some data, report discontinuity

            packetResponse->SetDiscontinuity(true);
            streamPackage->SetCompleted(S_OK);
          }
        }
      }
    }

    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE))
    {
      HRESULT res = S_OK;
      this->connectionState = Closing;

      if (this->mainCurlInstance != NULL)
      {
        res = this->mainCurlInstance->StopReceivingDataAsync(false);

        CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_INFO, L"%s: %s: closing connection failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, res));
      }

      if ((res == S_OK) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA)))
      {
        // this clear CURL instance and buffer, it leads to GetConnectionState() to PROTOCOL_CONNECTION_STATE_NONE result and connection will be reopened by ProtocolHoster,
        // it also reset each stream track downloading fragment
        this->StopReceivingData();
      }

      if (res == S_OK)
      {
        this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"connection closed successfully");
        this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA);
      }
    }
    
    // store stream fragments to temporary file
    if ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
    {
      this->lastStoreTime = GetTickCount();

      if (this->streamTracks->Count() > 0)
      {
        for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
        {
          CRtspStreamTrack *track = this->streamTracks->GetItem(i);

          if (track->GetCurrentProcessedSize() != 0)
          {
            track->SetLastProcessedSize(track->GetCurrentProcessedSize());
          }
          track->SetCurrentProcessedSize(0);

          // in case of live stream remove all downloaded and processed stream fragments before reported stream time
          if ((this->IsLiveStream()) && (this->reportedStreamTime > 0))
          {
            int64_t reportedStreamTimeRtpTimestamp = (int64_t)this->reportedStreamTime * track->GetClockFrequency() / 1000;

            // we must preserve stream fragment which is on start searching index position
            // this fragment is used as zero position fragment in searching for specific position

            unsigned int fragmentRemoveStart = (track->GetStreamFragments()->GetStartSearchingIndex() == 0) ? 1 : 0;
            unsigned int fragmentRemoveCount = 0;

            while ((fragmentRemoveStart + fragmentRemoveCount) < track->GetStreamFragmentProcessing())
            {
              CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(fragmentRemoveStart + fragmentRemoveCount);

              if (((fragmentRemoveStart + fragmentRemoveCount) != track->GetStreamFragments()->GetStartSearchingIndex()) && fragment->IsProcessed() && (fragment->GetFragmentRtpTimestamp() < (int64_t)reportedStreamTimeRtpTimestamp))
              {
                // fragment will be removed
                fragmentRemoveCount++;
              }
              else
              {
                break;
              }
            }

            if ((fragmentRemoveCount > 0) && (track->GetCacheFile()->RemoveItems(track->GetStreamFragments(), fragmentRemoveStart, fragmentRemoveCount)))
            {
              unsigned int startSearchIndex = (fragmentRemoveCount > track->GetStreamFragments()->GetStartSearchingIndex()) ? 0 : (track->GetStreamFragments()->GetStartSearchingIndex() - fragmentRemoveCount);
              unsigned int searchCountDecrease = (fragmentRemoveCount > track->GetStreamFragments()->GetStartSearchingIndex()) ? (fragmentRemoveCount - track->GetStreamFragments()->GetStartSearchingIndex()) : 0;

              track->GetStreamFragments()->SetStartSearchingIndex(startSearchIndex);
              track->GetStreamFragments()->SetSearchCount(track->GetStreamFragments()->GetSearchCount() - searchCountDecrease);

              track->GetStreamFragments()->Remove(fragmentRemoveStart, fragmentRemoveCount);

              track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() - fragmentRemoveCount);

              if (track->GetStreamFragmentDownloading() != UINT_MAX)
              {
                track->SetStreamFragmentDownloading(track->GetStreamFragmentDownloading() - fragmentRemoveCount);
              }

              if (track->GetStreamFragmentToDownload() != UINT_MAX)
              {
                track->SetStreamFragmentToDownload(track->GetStreamFragmentToDownload() - fragmentRemoveCount);
              }
            }
          }

          if (track->GetCacheFile()->GetCacheFile() == NULL)
          {
            wchar_t *storeFilePath = this->GetStoreFile(i, L"temp");
            CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, track->GetCacheFile()->SetCacheFile(storeFilePath));
            FREE_MEM(storeFilePath);
          }

          // store all stream fragments (which are not stored) to file
          if ((track->GetCacheFile()->GetCacheFile() != NULL) && (track->GetStreamFragments()->Count() != 0) && (track->GetStreamFragments()->GetLoadedToMemorySize() > CACHE_FILE_RELOAD_SIZE))
          {
            track->GetCacheFile()->StoreItems(track->GetStreamFragments(), this->lastStoreTime, false, this->IsWholeStreamDownloaded());
          }
        }
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_OPEN_CONNECTION_TIMEOUT, true, RTSP_OPEN_CONNECTION_TIMEOUT_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_OPEN_CONNECTION_SLEEP_TIME, true, RTSP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_RTSP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, RTSP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT);
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::StartReceivingData(CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);
  this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA);

  this->connectionState = None;

  for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
  {
    CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    track->SetStreamFragmentDownloading(UINT_MAX);
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
    CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() < this->streamTracks->Count(), result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      CRtspStreamTrack *track = this->streamTracks->GetItem(streamProgress->GetStreamId());

      streamProgress->SetTotalLength((track->GetStreamLength() == 0) ? 1 : track->GetStreamLength());
      streamProgress->SetCurrentLength((track->GetStreamLength() == 0) ? 0 : track->GetBytePosition());

      if (!track->IsSetStreamLength())
      {
        result = VFW_S_ESTIMATED;
      }
    }
  }

  return result;
}

void CMPUrlSourceSplitter_Protocol_Rtsp::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  __super::ClearSession();
 
  this->connectionState = None;
  this->streamTracks->Clear();

  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

  FREE_MEM_CLASS(this->sessionDescription);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_Rtsp::GetDuration(void)
{
  int64_t result = DURATION_LIVE_STREAM;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    if ((!this->IsLiveStream()) && (this->sessionDescription != NULL))
    {
      CNormalPlayTimeRangeAttribute *nptAttribute = NULL;
      for (unsigned int i = 0; i < this->sessionDescription->GetAttributes()->Count(); i++)
      {
        CAttribute *attribute = this->sessionDescription->GetAttributes()->GetItem(i);

        if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_NORMAL_PLAY_TIME_RANGE))
        {
          nptAttribute = dynamic_cast<CNormalPlayTimeRangeAttribute *>(attribute);
          break;
        }
      }

      if (nptAttribute != NULL)
      {
        if (nptAttribute->IsSetStartTime() && nptAttribute->IsSetEndTime() && (nptAttribute->GetEndTime() >= nptAttribute->GetStartTime()))
        {
          result = nptAttribute->GetEndTime() - nptAttribute->GetStartTime();
        }
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::GetStreamInformation(CStreamInformationCollection *streams)
{
  // RTSP protocol can have multiple streams
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streams);
  CHECK_CONDITION_HRESULT(result, this->streamTracks->Count() != 0, result, E_STREAM_COUNT_UNKNOWN);

  if (SUCCEEDED(result))
  {
    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
    {
      CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);

      CStreamInformation *streamInfo = new CStreamInformation(&result);
      CHECK_POINTER_HRESULT(result, streamInfo, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        streamInfo->SetStreamInputFormat(track->GetPayloadType()->GetStreamInputFormat());
        streamInfo->SetContainer(track->GetPayloadType()->IsSetFlags(RTSP_PAYLOAD_TYPE_FLAG_CONTAINER));
        streamInfo->SetPackets(track->GetPayloadType()->IsSetFlags(RTSP_PAYLOAD_TYPE_FLAG_PACKETS));
      }

      CHECK_CONDITION_HRESULT(result, streams->Add(streamInfo), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamInfo));
    }
  }

  return result;
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtsp::SeekToTime(unsigned int streamId, int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  // find fragment to process for each track
  // TO DO: implement better and faster seeking algorithm
  for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
  {
    CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
    {
      // first RTSP stream fragment has always zero timestamp
      CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);
      CRtspStreamFragment *nextFragment = track->GetStreamFragments()->GetItem(j + 1);

      // calculate fragment time in ms
      int64_t fragmentTime = fragment->GetFragmentRtpTimestamp() * 1000 / track->GetClockFrequency();
      int64_t nextFragmentTime = (nextFragment == NULL) ? INT64_MAX : (nextFragment->GetFragmentRtpTimestamp() * 1000 / track->GetClockFrequency());

      if ((fragmentTime <= time) && (nextFragmentTime >= time))
      {
        if (i >= streamId)
        {
          // set processing fragment only on equal to or greater than specified stream
          // in other case we will send same data again
          track->SetStreamFragmentProcessing(j);
        }

        result = (result == -1) ? fragmentTime : (min(result, fragmentTime));

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, time %lld, fragment: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, i, fragmentTime, j);
        break;
      }
    }
  }

  if ((result != (-1)) && (streamId == 0))
  {
    // first seek is always on stream with ID zero

    this->flags &= ~PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
    this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

    // in all tracks is set stream fragment to process
    // in all tracks exists at least one stream fragment and cover all possible RTP timestamps (0 - INT64_MAX)

    bool downloadedFragment = true;
    for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
    {
      CRtspStreamTrack *track = this->streamTracks->GetItem(i);

      track->SetStreamLength(0);
      track->SetStreamLengthFlag(false);

      CRtspStreamFragment *previousFragment = (track->GetStreamFragmentProcessing() != 0) ? track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing() - 1) : NULL;
      CRtspStreamFragment *processingFragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());

      if (!processingFragment->IsDownloaded())
      {
        track->SetFirstRtpPacketTimestamp(0, false, 0);
        track->SetRtpTimestampCorrection(0);

        // stream fragment is not downloaded, it is gap
        // split stream fragment

        downloadedFragment = false;
        result = time;
        CHECK_CONDITION_NOT_NULL_EXECUTE(previousFragment, previousFragment->SetDiscontinuity(true, track->GetStreamFragmentProcessing() - 1));

        // create new fragment within found fragment
        // calculate fragment RTP timestamp

        // first fragment RTP timestamp is always zero, no need to correct timestamp
        int64_t timestamp = time * track->GetClockFrequency() / 1000;
        HRESULT res = S_OK;
        CRtspStreamFragment *fragment = new CRtspStreamFragment(&res, timestamp, false);
        CHECK_POINTER_HRESULT(res, fragment, res, E_OUTOFMEMORY)

        result = SUCCEEDED(res) ? result : res;

        if (result >= 0)
        {
          fragment->SetFragmentStartPosition(0);
          track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() + 1);

          result = track->GetStreamFragments()->Insert(track->GetStreamFragmentProcessing(), fragment) ? result : E_OUTOFMEMORY;
        }

        if (result >= 0)
        {
          // force to download missing fragment
          track->SetStreamFragmentToDownload(track->GetStreamFragmentProcessing());
        }
      }

      // set start searching index to current processing stream fragment
      track->GetStreamFragments()->SetStartSearchingIndex(track->GetStreamFragmentProcessing());
      // set count of fragments to search for specific position
      unsigned int firstNotDownloadedFragmentIndex = track->GetStreamFragments()->GetFirstNotDownloadedStreamFragmentIndex(track->GetStreamFragmentProcessing());

      if (firstNotDownloadedFragmentIndex == UINT_MAX)
      {
        track->GetStreamFragments()->SetSearchCount(track->GetStreamFragments()->Count() - track->GetStreamFragmentProcessing());
      }
      else
      {
        // clear end of stream reached flag, because we moved to place with gap between processing stream fragment and last stream fragment
        this->flags &= ~PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

        track->SetEndOfStreamFlag(false);
        track->GetStreamFragments()->SetSearchCount(firstNotDownloadedFragmentIndex - track->GetStreamFragmentProcessing());
      }
    }

    CHECK_CONDITION_EXECUTE(!downloadedFragment, this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP_FLAG_STOP_RECEIVING_DATA);
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Rtsp::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Rtsp::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::Initialize(CPluginConfiguration *configuration)
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

wchar_t *CMPUrlSourceSplitter_Protocol_Rtsp::GetStoreFile(unsigned int trackId, const wchar_t *extension)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_rtsp_%s_track_%02u.%s", folder, guid, trackId, extension);
    }
    FREE_MEM(guid);
  }

  return result;
}