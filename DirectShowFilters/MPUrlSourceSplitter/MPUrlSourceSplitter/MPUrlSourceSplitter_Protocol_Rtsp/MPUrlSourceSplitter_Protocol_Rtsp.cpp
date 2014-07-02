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
  this->receiveDataTimeout = RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT;
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

    if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()))
    {
      if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && (this->connectionState == Opened))
      {
        if (SUCCEEDED(result))
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
                  DWORD ticks = GetTickCount();
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

                int64_t timestamp = streamTrack->GetRtpPacketTimestamp(rtpPacket->GetTimestamp(), true) + streamTrack->GetRtpTimestampCorrection();

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

                    currentDownloadingFragment->SetDownloaded(true);

                    // recalculate start position of all downloaded stream fragments until first not downloaded stream fragment
                    this->RecalculateStreamFragmentStartPosition(streamTrack->GetStreamFragments(), streamTrack->GetStreamFragmentDownloading());

                    if (nextFragment->IsDownloaded())
                    {
                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, found next RTSP stream fragment with lower timestamp as receiving stream fragment, stopping downloading fragment, current fragment timestamp: %lld, receiving fragment timestamp: %lld, next fragment timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), timestamp, nextFragment->GetFragmentRtpTimestamp());

                      currentDownloadingFragment = NULL;
                      streamTrack->SetStreamFragmentDownloading(UINT_MAX);
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
                    currentDownloadingFragment->SetDownloaded(true);

                    // recalculate start position of all downloaded stream fragments until first not downloaded stream fragment
                    this->RecalculateStreamFragmentStartPosition(streamTrack->GetStreamFragments(), streamTrack->GetStreamFragmentDownloading());

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

            if (streamTrack->GetStreamFragmentDownloading() != UINT_MAX)
            {
              CLockMutex lockData(this->lockCurlMutex, INFINITE);

              if (track->IsEndOfStream() && (track->GetRtpPackets()->Count() == 0))
              {
                // mark currently downloading stream fragment as downloaded
                CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());
                if (fragment != NULL)
                {
                  fragment->SetDownloaded(true);

                  // recalculate start position of all downloaded stream fragments until first not downloaded stream fragment
                  this->RecalculateStreamFragmentStartPosition(streamTrack->GetStreamFragments(), streamTrack->GetStreamFragmentDownloading());
                }

                streamTrack->SetStreamFragmentDownloading(UINT_MAX);
              }
            }

            rtpPackets->Clear();
          }

          FREE_MEM_CLASS(rtpPackets);
        }
      }

      bool allTracksSetFirstRtpPacketTimestamp = (this->streamTracks->Count() != 0);

      if (SUCCEEDED(result))
      {
        for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
        {
          CRtspStreamTrack *track = this->streamTracks->GetItem(i);

          allTracksSetFirstRtpPacketTimestamp &= track->IsSetFirstRtpPacketTimestamp();
        }
      }

      if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()) && (this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
      {
        // all data received, we're not receiving data
        if (this->mainCurlInstance->GetDownloadResponse()->GetResultError() == CURLE_OK)
        {
          bool receivedAllData = true;

          for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
          {
            CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
            CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

            if ((track->GetRtpPackets()->Count() == 0) && (!streamTrack->IsReceivedAllData()))
            {
              // all data read from CURL instance for track i, received all data
              this->logger->Log(LOGGER_INFO, L"%s: %s: track %u, all data received", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i);
              streamTrack->SetReceivedAllDataFlag(true);
            }
            else
            {
              receivedAllData = false;
            }
          }

          if (receivedAllData)
          {
            this->mainCurlInstance->StopReceivingData();

            // check last fragment of each track
            // if all are downloaded, then we reached end of stream - this doesn't mean that there is not gap
            for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
            {
              CRtspStreamTrack *track = this->streamTracks->GetItem(i);

              if (track->GetStreamFragments()->Count() != 0)
              {
                CRtspStreamFragment *lastFragment = track->GetStreamFragments()->GetItem(track->GetStreamFragments()->Count() - 1);

                receivedAllData &= lastFragment->IsDownloaded();
              }
            }

            if (receivedAllData)
            {
              // set end of stream reached and also set stream total length
              this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

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
            }
          }
        }
        else
        {
          // error occured while downloading
          // download stream fragment again or download scheduled stream fragment

          this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: %d, restarting download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetResultError());

          for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
          {
            CRtspStreamTrack *track = this->streamTracks->GetItem(i);

            track->SetStreamFragmentToDownload(track->GetStreamFragmentDownloading());
            track->SetStreamFragmentDownloading(UINT_MAX);
          }

          FREE_MEM_CLASS(this->mainCurlInstance);
        }
      }

      if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()) && (this->streamTracks->Count() != 0))
      {
        bool allStreamFragmentsDownloaded = true;

        for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
        {
          CRtspStreamTrack *track = this->streamTracks->GetItem(i);

          if ((track->GetStreamFragmentToDownload() != UINT_MAX) ||
            (track->GetStreamFragmentDownloading() != UINT_MAX) ||
            (track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) != UINT_MAX))
          {
            allStreamFragmentsDownloaded = false;
            break;
          }
        }

        if (allStreamFragmentsDownloaded)
        {
          // all fragments downloaded and processed
          // whole stream downloaded
          this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;        
          this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

          this->mainCurlInstance->StopReceivingData();

          for (unsigned int i = 0; (i < this->streamTracks->Count()); i++)
          {
            CRtspStreamTrack *track = this->streamTracks->GetItem(i);

            // all stream fragments processed
            // set stream length and report end of stream

            if (!track->IsSetStreamLength())
            {
              track->SetStreamLength(track->GetBytePosition());
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
              track->SetStreamLengthFlag(true);
            }

            if (!track->IsSetEndOfStream())
            {
              // notify filter the we reached end of stream
              track->SetEndOfStreamFlag(true);
            }
          }
        }
        else
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

                track->SetStreamLength(int64_t(MINIMUM_RECEIVED_DATA_FOR_SPLITTER));
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
              }
              else if ((track->GetBytePosition() > (track->GetStreamLength() * 3 / 4)))
              {
                // it is time to adjust stream length, we are approaching to end but still we don't know total length
                track->SetStreamLength(track->GetBytePosition() * 2);
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
              }
            }
          }
        }
      }

      // check if there isn't some fragment to download
      bool anyTrackDownloading = false;
      bool startFragmentDownload = false;

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        anyTrackDownloading |= (track->GetStreamFragmentDownloading() != UINT_MAX);

        if (track->GetStreamFragmentDownloading() == UINT_MAX)
        {
          unsigned int fragmentToDownload = UINT_MAX;

          // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
          fragmentToDownload = (track->GetStreamFragmentToDownload() == UINT_MAX) ? track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(track->GetStreamFragmentProcessing()) : track->GetStreamFragmentToDownload();
          // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
          fragmentToDownload = (fragmentToDownload == UINT_MAX) ? track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) : fragmentToDownload;
          // fragment to download still can be UINT_MAX = no fragment to download

          track->SetStreamFragmentToDownload(fragmentToDownload);
          startFragmentDownload |= (fragmentToDownload != UINT_MAX);
        }
      }

      if (SUCCEEDED(result) && ((this->mainCurlInstance == NULL) || ((startFragmentDownload) && (!anyTrackDownloading))))
      {
        int64_t startTime = INT64_MAX;

        // if seeking to empty place (no downloaded fragment before) then use fragment time
        // if seeking to fill gap (one or more downloaded fragment(s) before) then use previous fragment time
        // if seeking in live stream (e.g. lost connection) then use zero time

        for (unsigned int i = 0; (SUCCEEDED(result) && (!this->IsLiveStream()) && (i < this->streamTracks->Count())); i++)
        {
          CRtspStreamTrack *track = this->streamTracks->GetItem(i);
          unsigned int fragmentToDownload = track->GetStreamFragmentToDownload();

          if (fragmentToDownload != UINT_MAX)
          {
            CRtspStreamFragment *firstFragment = track->GetStreamFragments()->GetItem(0);
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

          if (this->configuration->GetValueBool(PARAMETER_NAME_DUMP_INPUT_RAW_DATA, true, PARAMETER_NAME_DUMP_INPUT_RAW_DATA_DEFAULT))
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
            request->SetReceivedDataTimeout(this->receiveDataTimeout);
            request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

            result = this->mainCurlInstance->Initialize(request);
          }
          FREE_MEM_CLASS(request);

          CHECK_CONDITION_EXECUTE(FAILED(result), this->connectionState = InitializeFailed);

          if (SUCCEEDED(result))
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

              // if in configuration is specified live stream flag, then use it and ignore normal play time attribute
              //this->liveStream = this->configurationParameters->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, this->liveStream);
            }

            // all parameters set
            // start receiving data

            CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->mainCurlInstance->StartReceivingData(), result);
          }

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->connectionState = Opening);
        }

        if (SUCCEEDED(result))
        {
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
                unsigned int firstNotDownloadedFragmentIndex = track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(track->GetStreamFragmentProcessing());
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

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->connectionState = Opened);
        }
      }
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

    if ((streamPackage->GetState() == CStreamPackage::Waiting) || (streamPackage->GetState() == CStreamPackage::WaitingIgnoreTimeout))
    {
      // in Waiting or WaitingIgnoreTimeout state can be request only if request and response are correctly set
      CStreamPackageDataRequest *dataRequest = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CStreamPackagePacketRequest *packetRequest = dynamic_cast<CStreamPackagePacketRequest *>(streamPackage->GetRequest());

      if (dataRequest != NULL)
      {
        CStreamPackageDataResponse *dataResponse = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

        CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(dataRequest->GetStreamId());

        // clear response buffer
        dataResponse->GetBuffer()->ClearBuffer();

        // first try to find starting media packet (packet which have first data)
        unsigned int packetIndex = UINT_MAX;
        unsigned int foundDataLength = 0;

        int64_t startPosition = dataRequest->GetStart();
        packetIndex = streamTrack->GetStreamFragments()->GetStreamFragmentIndexBetweenPositions(startPosition);

        while (packetIndex != UINT_MAX)
        {
          streamTrack->SetStreamFragmentProcessing(packetIndex);

          // get stream fragment
          CRtspStreamFragment *streamFragment = streamTrack->GetStreamFragments()->GetItem(packetIndex);

          int64_t streamFragmentRelativeStart = streamFragment->GetFragmentStartPosition() - streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragments()->GetStartSearchingIndex())->GetFragmentStartPosition();

          // set copy data start and copy data length
          unsigned int copyDataStart = (startPosition > streamFragmentRelativeStart) ? (unsigned int)(startPosition - streamFragmentRelativeStart) : 0;
          unsigned int copyDataLength = min(streamFragment->GetLength() - copyDataStart, dataRequest->GetLength() - foundDataLength);

          // copy data from media packet to response buffer
          if (streamTrack->GetCacheFile()->LoadItems(streamTrack->GetStreamFragments(), packetIndex, true, false))
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

          if (foundDataLength < dataRequest->GetLength())
          {
            // find another stream fragment after end of this stream fragment
            startPosition += copyDataLength;

            packetIndex = streamTrack->GetStreamFragments()->GetStreamFragmentIndexBetweenPositions(startPosition);

            if (packetIndex != UINT_MAX)
            {
              streamTrack->SetStreamFragmentProcessing(packetIndex);
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
            unsigned int currentTime = GetTickCount();

            if ((streamPackage->GetState() == CStreamPackage::Waiting) && ((currentTime - dataRequest->GetStartTime()) > this->GetReceiveDataTimeout()))
            {
              // request timeouted
              // finish request with error to avoid freeze
              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', stream '%u', timeout, current time: %u, request start time: %u, specified timeout: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStreamId(), currentTime, dataRequest->GetStartTime(), this->GetReceiveDataTimeout());

              streamPackage->SetCompleted(VFW_E_TIMEOUT);
            }
            else if (this->IsLiveStream() && (!this->IsWholeStreamDownloaded()) && ((currentTime - streamTrack->GetLastReceiveDataTime()) > this->GetReceiveDataTimeout()))
            {
              // we don't receive data from protocol at least for specified timeout or request timeouted
              // finish request with error to avoid freeze
              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', stream '%u', doesn't receive data for specified time, current time: %u, last received data time: %u, specified timeout: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStreamId(), currentTime, streamTrack->GetLastReceiveDataTime(), this->GetReceiveDataTimeout());

              streamPackage->SetCompleted(VFW_E_TIMEOUT);
            }
            else if ((!this->IsWholeStreamDownloaded()) && streamTrack->IsSetStreamLength() && ((dataRequest->GetStart() + dataRequest->GetLength()) < streamTrack->GetStreamLength()))
            {
              // we are receiving data, wait for all requested data
            }
            else if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= streamTrack->GetStreamLength()))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', stream '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStreamId(), dataRequest->GetStart(), dataRequest->GetLength(), streamTrack->GetStreamLength());

              streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_NO_MORE_DATA_AVAILABLE);
            }
          }

          //  if (streamPackage->GetState() == CStreamPackage::Waiting)
          //  {
          //    //if ((request->GetStart() <= this->currentStreamPosition) && ((request->GetStart() + request->GetLength()) > this->currentStreamPosition))
          //    //{
          //    //  // current stream position is within current request
          //    //  // we are receiving data, do nothing, just wait for all data
          //    //  streamPackage->SetWaitingIgnoreTimeout();
          //    //}
          //  }

          //  if (streamPackage->GetState() == CStreamPackage::Waiting)
          //  {
          //    // no seeking by position is available
          //    // check request against current stream position, if we can receive requested data

          //    //if ((request->GetStart() + request->GetLength()) <= this->currentStreamPosition)
          //    //{
          //    //  // it's bad, current stream position is after requested data and we can't seek
          //    //  this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', before current stream position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetStart() + request->GetLength(), this->currentStreamPosition);

          //    //  streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
          //    //}
          //  }

          if (streamPackage->GetState() == CStreamPackage::Waiting)
          {
            // no seeking by position is available
            // requested position is probably in stream fragment on the end of searchable stream fragments, between streamTrack->GetStreamFragments()->GetStartSearchingIndex() and streamTrack->GetStreamFragments()->GetSearchCount()
            // check if fragment is downloading
            // if fragment is not downloading, then schedule it for download

            unsigned int fragmentIndex = streamTrack->GetStreamFragments()->GetStartSearchingIndex() + streamTrack->GetStreamFragments()->GetSearchCount();
            CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(fragmentIndex);

            if ((fragment != NULL) && (!fragment->IsDownloaded()) && (fragmentIndex != streamTrack->GetStreamFragmentDownloading()))
            {
              // fragment is not downloaded and also is not downloading currently
              streamTrack->SetStreamFragmentDownloading(UINT_MAX);
              streamTrack->SetStreamFragmentToDownload(fragmentIndex);
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
          // clone media packet from stream fragments
          if (streamTrack->GetCacheFile()->LoadItems(streamTrack->GetStreamFragments(), streamTrack->GetStreamFragmentProcessing(), true, false))
          {
            int64_t pts = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentProcessing())->GetFragmentRtpTimestamp();
            pts = streamTrack->GetRtpPacketTimestampInDshowTimeBaseUnits(pts);

            packetResponse->GetMediaPacket()->SetPresentationTimestamp(pts);
            packetResponse->GetMediaPacket()->SetPresentationTimestampTicksPerSecond(DSHOW_TIME_BASE);

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
          unsigned int currentTime = GetTickCount();

          if ((streamPackage->GetState() == CStreamPackage::Waiting) && ((currentTime - packetRequest->GetStartTime()) > this->GetReceiveDataTimeout()))
          {
            // request timeouted
            // finish request with error to avoid freeze
            this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', stream '%u', timeout, current time: %u, request start time: %u, specified timeout: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, packetRequest->GetId(), packetRequest->GetStreamId(), currentTime, packetRequest->GetStartTime(), this->GetReceiveDataTimeout());

            streamPackage->SetCompleted(VFW_E_TIMEOUT);
          }
          else if (streamTrack->IsSetEndOfStream() && (streamTrack->GetStreamFragmentProcessing() >= streamTrack->GetStreamFragments()->Count()))
          {
            // we are not receiving more data, complete request
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', stream '%u'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, packetRequest->GetId(), packetRequest->GetStreamId());

            streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
          }
        }
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

              if (((fragmentRemoveStart + fragmentRemoveCount) != track->GetStreamFragments()->GetStartSearchingIndex()) && fragment->IsDownloaded() && (fragment->GetFragmentRtpTimestamp() < (int64_t)reportedStreamTimeRtpTimestamp))
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
          if ((track->GetCacheFile()->GetCacheFile() != NULL) && (track->GetStreamFragments()->Count() != 0))
          {
            track->GetCacheFile()->StoreItems(track->GetStreamFragments(), this->lastStoreTime, this->IsWholeStreamDownloaded());
          }
        }
      }
    }



    // RTSP can have several streams
    //if ((this->IsConnected()) && (!this->wholeStreamDownloaded) && (receiveData->SetStreamCount(this->streamTracks->Count())))
    //{
    //  receiveData->SetLiveStream(this->liveStream);

    //  if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && (this->streamTracks->Count() != 0))
    //  {
    //    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
    //    {
    //      CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
    //      CStreamReceiveData *stream = receiveData->GetStreams()->GetItem(i);

    //      stream->SetStreamInputFormat(track->GetPayloadType()->GetStreamInputFormat());
    //      stream->SetContainer(track->GetPayloadType()->IsSetFlags(RTSP_PAYLOAD_TYPE_FLAG_CONTAINER));
    //      stream->SetPackets(track->GetPayloadType()->IsSetFlags(RTSP_PAYLOAD_TYPE_FLAG_PACKETS));
    //    }
    //  }

    //  if (SUCCEEDED(result) && (this->mainCurlInstance != NULL))
    //  {
    //    CRtpPacketCollection *rtpPackets = new CRtpPacketCollection();
    //    CHECK_POINTER_HRESULT(result, rtpPackets, result, E_OUTOFMEMORY);

    //    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
    //    {
    //      CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
    //      CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

    //      {
    //        // copy RTP packets to avoid blocking of RTSP download instance
    //        CLockMutex lockData(this->lockCurlMutex, INFINITE);

    //        rtpPackets->Append(track->GetRtpPackets());
    //        track->GetRtpPackets()->Clear();
    //      }

    //      if (rtpPackets->Count() != 0)
    //      {
    //        // split or add received RTP packets to stream fragments
    //        CRtspStreamFragment *currentDownloadingFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());

    //        for (unsigned int j = 0; (SUCCEEDED(result) && (currentDownloadingFragment != NULL) && (j < rtpPackets->Count())); j++)
    //        {
    //          CRtpPacket *rtpPacket = rtpPackets->GetItem(j);

    //          // set first RTP packet timestamp (if not already set)
    //          // timestamp is needed to compute (almost - after seeking we only assume that timestamps are correct) correct timestamps of fragments
    //          if (!streamTrack->IsSetFirstRtpPacketTimestamp())
    //          {
    //            DWORD ticks = GetTickCount();
    //            streamTrack->SetFirstRtpPacketTimestamp(rtpPacket->GetTimestamp(), true, ticks);

    //            // change current downloading fragment RTP timestamp to correct timestamp based on time from start of stream (first time set ticks to track)
    //            if (this->liveStream)
    //            {
    //              int64_t timestamp = ((int64_t)ticks - (int64_t)streamTrack->GetFirstRtpPacketTicks()) * streamTrack->GetClockFrequency() / 1000;

    //              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, fragment timestamp changing from %lld to %lld, ticks: %u, track ticks: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), timestamp, ticks, streamTrack->GetFirstRtpPacketTicks());
    //              currentDownloadingFragment->SetFragmentRtpTimestamp(timestamp, false);
    //            }

    //            // compute correction for actual stream track
    //            int64_t correction = currentDownloadingFragment->GetFragmentRtpTimestamp() - (int64_t)rtpPacket->GetTimestamp();

    //            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, fragment timestamp: %lld, first RTP packet timestamp: %u, RTP: %u, ticks: %u, track ticks: %u, correction: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), rtpPacket->GetTimestamp(), rtpPacket->GetSequenceNumber(), ticks, streamTrack->GetFirstRtpPacketTicks(), correction);
    //            streamTrack->SetRtpTimestampCorrection(correction);
    //          }

    //          int64_t timestamp = streamTrack->GetRtpPacketTimestamp(rtpPacket->GetTimestamp(), true) + streamTrack->GetRtpTimestampCorrection();

    //          if (!currentDownloadingFragment->IsSetFragmentRtpTimestamp())
    //          {
    //            currentDownloadingFragment->SetFragmentRtpTimestamp(timestamp);
    //          }

    //          CRtspStreamFragment *nextFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading() + 1);

    //          // nextFragment can be NULL in case that we are on the end of collection

    //          if (nextFragment != NULL)
    //          {
    //            if (timestamp >= nextFragment->GetFragmentRtpTimestamp())
    //            {
    //              // our RTP packet timestamp is greater than or equal to next fragment timestamp
    //              // this means that we are receiving data, which we already have - in case if next fragment is downloaded

    //              currentDownloadingFragment->SetDownloaded(true);

    //              if (nextFragment->IsDownloaded())
    //              {
    //                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, found next RTSP stream fragment with lower timestamp as receiving stream fragment, stopping downloading fragment, receiving fragment timestamp: %lld, next fragment timestamp: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), nextFragment->GetFragmentRtpTimestamp());

    //                currentDownloadingFragment = NULL;
    //                streamTrack->SetStreamFragmentDownloading(UINT_MAX);
    //              }
    //              else
    //              {
    //                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, found next not downloaded RTSP stream fragment with lower timestamp as receiving stream fragment, continuing downloading fragment, receiving fragment timestamp: %lld, next fragment timestamp: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), nextFragment->GetFragmentRtpTimestamp());

    //                nextFragment->SetFragmentRtpTimestamp(timestamp);

    //                currentDownloadingFragment = nextFragment;
    //                streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
    //              }
    //            }
    //          }

    //          if ((currentDownloadingFragment != NULL) && (timestamp != currentDownloadingFragment->GetFragmentRtpTimestamp()))
    //          {
    //            CRtspStreamFragment *nextFragment = new CRtspStreamFragment(timestamp, true);
    //            CHECK_POINTER_HRESULT(result, nextFragment, result, E_OUTOFMEMORY);

    //            if (SUCCEEDED(result))
    //            {
    //              result = streamTrack->GetStreamFragments()->Insert(streamTrack->GetStreamFragmentDownloading() + 1, nextFragment) ? result : E_OUTOFMEMORY;
    //            }

    //            if (SUCCEEDED(result))
    //            {
    //              currentDownloadingFragment->SetDownloaded(true);
    //              streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
    //            }
    //            else
    //            {
    //              FREE_MEM_CLASS(nextFragment);
    //            }

    //            currentDownloadingFragment = nextFragment;
    //          }

    //          // add RTP packet to current downloading stream fragment
    //          if (SUCCEEDED(result) && (currentDownloadingFragment != NULL))
    //          {
    //            result = (currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize(), currentDownloadingFragment->GetBuffer()->GetBufferSize() * 2) == rtpPacket->GetPayloadSize()) ? result : E_OUTOFMEMORY;
    //          }
    //        }
    //      }

    //      if (streamTrack->GetStreamFragmentDownloading() != UINT_MAX)
    //      {
    //        CLockMutex lockData(this->lockCurlMutex, INFINITE);

    //        if (track->IsEndOfStream() && (track->GetRtpPackets()->Count() == 0))
    //        {
    //          // mark currently downloading stream fragment as downloaded
    //          CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());
    //          if (fragment != NULL)
    //          {
    //            fragment->SetDownloaded(true);
    //          }

    //          streamTrack->SetStreamFragmentDownloading(UINT_MAX);
    //        }
    //      }

    //      rtpPackets->Clear();
    //    }

    //    FREE_MEM_CLASS(rtpPackets);
    //  }

    //  bool allTracksSetFirstRtpPacketTimestamp = (this->streamTracks->Count() != 0);

    //  if (SUCCEEDED(result))
    //  {
    //    for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
    //    {
    //      CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //      allTracksSetFirstRtpPacketTimestamp &= track->IsSetFirstRtpPacketTimestamp();
    //    }
    //  }

    //  for (unsigned int i = 0; (SUCCEEDED(result) && (allTracksSetFirstRtpPacketTimestamp) && (i < this->streamTracks->Count())); i++)
    //  {
    //    CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //    if ((!track->IsSetSupressData()) && (track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count()))
    //    {
    //      bool loadStreamFragmentToMemory = true;

    //      while (track->GetCacheFile()->LoadItems(track->GetStreamFragments(), track->GetStreamFragmentProcessing(), loadStreamFragmentToMemory, true))
    //      {
    //        // do not load next stream fragment from cache file - in another case it leads to situation that whole cache file will be loaded to memory
    //        loadStreamFragmentToMemory = false;

    //        CLinearBuffer *bufferForProcessing = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing())->GetBuffer();
    //        unsigned int bufferOccupiedSpace = bufferForProcessing->GetBufferOccupiedSpace();

    //        if (bufferOccupiedSpace > 0)
    //        {
    //          // create media packet
    //          // set values of media packet

    //          CMediaPacket *mediaPacket = new CMediaPacket();
    //          CHECK_POINTER_HRESULT(result, mediaPacket, result, E_OUTOFMEMORY);
    //          CHECK_CONDITION_HRESULT(result, mediaPacket->GetBuffer()->AddToBufferWithResize(bufferForProcessing) == bufferOccupiedSpace, result, E_OUTOFMEMORY);

    //          if (SUCCEEDED(result))
    //          {
    //            mediaPacket->SetStart(track->GetBytePosition());
    //            mediaPacket->SetEnd(track->GetBytePosition() + bufferOccupiedSpace - 1);

    //            int64_t pts = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing())->GetFragmentRtpTimestamp();
    //            pts = track->GetRtpPacketTimestampInDshowTimeBaseUnits(pts);

    //            mediaPacket->SetPresentationTimestamp(pts);
    //            mediaPacket->SetPresentationTimestampTicksPerSecond(DSHOW_TIME_BASE);

    //            result = receiveData->GetStreams()->GetItem(i)->GetMediaPacketCollection()->Add(mediaPacket) ? result : E_OUTOFMEMORY;
    //          }

    //          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaPacket));
    //        }

    //        if (SUCCEEDED(result))
    //        {
    //          track->SetBytePosition(track->GetBytePosition() + bufferOccupiedSpace);
    //          track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() + 1);

    //          // check if fragment is downloaded
    //          // if fragment is not downloaded, then schedule it for download
    //          if (track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count())
    //          {
    //            CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());

    //            if ((!fragment->IsDownloaded()) && (track->GetStreamFragmentProcessing() != track->GetStreamFragmentDownloading()))
    //            {
    //              // fragment is not downloaded and also is not downloading currently
    //              track->SetStreamFragmentDownloading(UINT_MAX);
    //              track->SetStreamFragmentToDownload(track->GetStreamFragmentProcessing());
    //            }
    //          }
    //        }
    //      }
    //    }

    //    // adjust total length (if necessary)
    //    if (!track->IsSetStreamLength())
    //    {
    //      if (track->GetStreamLength() == 0)
    //      {
    //        // stream length not set
    //        // just make guess

    //        track->SetStreamLength(int64_t(MINIMUM_RECEIVED_DATA_FOR_SPLITTER));
    //        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());

    //        receiveData->GetStreams()->GetItem(i)->GetTotalLength()->SetTotalLength(track->GetStreamLength(), true);
    //      }
    //      else if ((track->GetBytePosition() > (track->GetStreamLength() * 3 / 4)))
    //      {
    //        // it is time to adjust stream length, we are approaching to end but still we don't know total length
    //        track->SetStreamLength(track->GetBytePosition() * 2);
    //        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());

    //        receiveData->GetStreams()->GetItem(i)->GetTotalLength()->SetTotalLength(track->GetStreamLength(), true);
    //      }
    //    }
    //  }

    //  if ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    //  {
    //    // all data received, we're not receiving data
    //    if (this->mainCurlInstance->GetDownloadResponse()->GetResultCode() == CURLE_OK)
    //    {
    //      bool receivedAllData = true;

    //      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
    //      {
    //        CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
    //        CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

    //        if ((track->GetRtpPackets()->Count() == 0) && (!streamTrack->IsReceivedAllData()))
    //        {
    //          // all data read from CURL instance for track i, received all data
    //          this->logger->Log(LOGGER_INFO, L"%s: %s: track %u, all data received", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i);
    //          streamTrack->SetReceivedAllDataFlag(true);
    //        }
    //        else
    //        {
    //          receivedAllData = false;
    //        }
    //      }

    //      CHECK_CONDITION_EXECUTE(receivedAllData, this->mainCurlInstance->StopReceivingData());
    //    }
    //    else
    //    {
    //      // error occured while downloading
    //      // download stream fragment again or download scheduled stream fragment

    //      this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: %d, restarting download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetResultCode());

    //      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
    //      {
    //        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //        track->SetStreamFragmentToDownload(track->GetStreamFragmentDownloading());
    //        track->SetStreamFragmentDownloading(UINT_MAX);
    //      }

    //      FREE_MEM_CLASS(this->mainCurlInstance);
    //    }
    //  }

    //  if (SUCCEEDED(result) && (this->streamTracks->Count() != 0))
    //  {
    //    bool allStreamFragmentsDownloaded = true;

    //    for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
    //    {
    //      CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //      if ((track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count()) ||
    //        (track->GetStreamFragmentToDownload() != UINT_MAX) ||
    //        (track->GetStreamFragmentDownloading() != UINT_MAX) ||
    //        (track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) != UINT_MAX))
    //      {
    //        allStreamFragmentsDownloaded = false;
    //        break;
    //      }
    //    }

    //    if (allStreamFragmentsDownloaded)
    //    {
    //      // all fragments downloaded and processed
    //      // whole stream downloaded
    //      this->wholeStreamDownloaded = true;
    //      this->isConnected = false;

    //      this->mainCurlInstance->StopReceivingData();

    //      for (unsigned int i = 0; (i < this->streamTracks->Count()); i++)
    //      {
    //        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //        if (track->GetStreamFragmentProcessing() >= track->GetStreamFragments()->Count())
    //        {
    //          // we are not seeking, so we can set total length

    //          // all stream fragments processed
    //          // set stream length and report end of stream

    //          if (!track->IsSetStreamLength())
    //          {
    //            track->SetStreamLength(track->GetBytePosition());
    //            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
    //            track->SetStreamLengthFlag(true);

    //            receiveData->GetStreams()->GetItem(i)->GetTotalLength()->SetTotalLength(track->GetStreamLength(), false);
    //          }

    //          if (!track->IsSetEndOfStream())
    //          {
    //            // notify filter the we reached end of stream
    //            track->SetEndOfStreamFlag(true);

    //            receiveData->GetStreams()->GetItem(i)->GetEndOfStreamReached()->SetStreamPosition(max(0, track->GetBytePosition() - 1));
    //          }
    //        }
    //      }
    //    }      
    //  }

    //  // check if there isn't some fragment to download
    //  bool anyTrackDownloading = false;
    //  bool startFragmentDownload = false;

    //  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
    //  {
    //    CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //    anyTrackDownloading |= (track->GetStreamFragmentDownloading() != UINT_MAX);

    //    if (track->GetStreamFragmentDownloading() == UINT_MAX)
    //    {
    //      unsigned int fragmentToDownload = UINT_MAX;

    //      // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
    //      fragmentToDownload = (track->GetStreamFragmentToDownload() == UINT_MAX) ? track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(track->GetStreamFragmentProcessing()) : track->GetStreamFragmentToDownload();
    //      // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
    //      fragmentToDownload = (fragmentToDownload == UINT_MAX) ? track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) : fragmentToDownload;
    //      // fragment to download still can be UINT_MAX = no fragment to download

    //      track->SetStreamFragmentToDownload(fragmentToDownload);
    //      startFragmentDownload |= (fragmentToDownload != UINT_MAX);
    //    }
    //  }

    //  if (SUCCEEDED(result) && (this->isConnected) && ((this->mainCurlInstance == NULL) || ((startFragmentDownload) && (!anyTrackDownloading))))
    //  {
    //    int64_t startTime = INT64_MAX;

    //    // if seeking to empty place (no downloaded fragment before) then use fragment time
    //    // if seeking to fill gap (one or more downloaded fragment(s) before) then use previous fragment time
    //    // if seeking in live stream (e.g. lost connection) then use zero time

    //    for (unsigned int i = 0; (SUCCEEDED(result) && (!this->liveStream) && (i < this->streamTracks->Count())); i++)
    //    {
    //      CRtspStreamTrack *track = this->streamTracks->GetItem(i);
    //      unsigned int fragmentToDownload = track->GetStreamFragmentToDownload();

    //      if (fragmentToDownload != UINT_MAX)
    //      {
    //        CRtspStreamFragment *firstFragment = track->GetStreamFragments()->GetItem(0);
    //        CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(fragmentToDownload);

    //        int64_t fragmentRtpTimestamp = INT64_MAX;

    //        if (fragmentToDownload > 0)
    //        {
    //          CRtspStreamFragment *previousFragment = track->GetStreamFragments()->GetItem(fragmentToDownload - 1);

    //          if (previousFragment->IsDownloaded())
    //          {
    //            fragmentRtpTimestamp = min(previousFragment->GetFragmentRtpTimestamp(), fragmentRtpTimestamp);
    //          }
    //        }

    //        fragmentRtpTimestamp = min(fragment->GetFragmentRtpTimestamp(), fragmentRtpTimestamp);
    //        fragmentRtpTimestamp *= 1000;
    //        fragmentRtpTimestamp /= track->GetClockFrequency();

    //        startTime = min(startTime, fragmentRtpTimestamp);
    //      }
    //    }

    //    startTime = (startTime != INT64_MAX) ? startTime: 0;

    //    FREE_MEM_CLASS(this->mainCurlInstance);
    //    FREE_MEM_CLASS(this->sessionDescription);

    //    // new connection will be created with new RTP packet timestamps
    //    // clear set RTP packet timestamp flags
    //    for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
    //    {
    //      CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //      track->SetFirstRtpPacketTimestamp(UINT_MAX, false, 0);
    //      track->SetReceivedAllDataFlag(false);

    //      // clear all not downloaded stream fragments
    //      // recalculate stream fragments timestams for not downloaded stream fragments
    //      for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
    //      {
    //        CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);

    //        if (!fragment->IsDownloaded())
    //        {
    //          fragment->GetBuffer()->ClearBuffer();

    //          fragment->SetFragmentRtpTimestamp(fragment->GetFragmentRtpTimestamp(), false);
    //        }
    //      }
    //    }

    //    this->mainCurlInstance = new CRtspCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
    //    CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

    //    if (SUCCEEDED(result))
    //    {
    //      // set connection priorities
    //      this->mainCurlInstance->SetMulticastPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_MULTICAST_PREFERENCE, true, RTSP_MULTICAST_PREFERENCE_DEFAULT));
    //      this->mainCurlInstance->SetSameConnectionTcpPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_SAME_CONNECTION_TCP_PREFERENCE, true, RTSP_SAME_CONNECTION_TCP_PREFERENCE_DEFAULT));
    //      this->mainCurlInstance->SetUdpPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_UDP_PREFERENCE, true, RTSP_UDP_PREFERENCE_DEFAULT));

    //      // set ports
    //      this->mainCurlInstance->SetRtspClientPortMin(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MIN, true, RTSP_CLIENT_PORT_MIN_DEFAULT));
    //      this->mainCurlInstance->SetRtspClientPortMax(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MAX, true, RTSP_CLIENT_PORT_MAX_DEFAULT));

    //      this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
    //      this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
    //      this->mainCurlInstance->SetIgnoreRtpPayloadType(this->configurationParameters->GetValueBool(PARAMETER_NAME_RTSP_IGNORE_RTP_PAYLOAD_TYPE, true, RTSP_IGNORE_RTP_PAYLOAD_TYPE_DEFAULT));
    //    }

    //    if (SUCCEEDED(result))
    //    {
    //      CRtspDownloadRequest *request = new CRtspDownloadRequest();
    //      CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

    //      if (SUCCEEDED(result))
    //      {
    //        request->SetStartTime(startTime);
    //        request->SetUrl(this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL));

    //        result = (this->mainCurlInstance->Initialize(request)) ? S_OK : E_FAIL;
    //      }
    //      FREE_MEM_CLASS(request);

    //      if (SUCCEEDED(result))
    //      {
    //        if (this->sessionDescription == NULL)
    //        {
    //          const wchar_t *rawSDP = this->mainCurlInstance->GetRtspDownloadResponse()->GetRawSessionDescription();
    //          unsigned int rawSDPlength = wcslen(rawSDP);

    //          this->sessionDescription = new CSessionDescription();
    //          CHECK_POINTER_HRESULT(result, this->sessionDescription, result, E_OUTOFMEMORY);
    //          CHECK_CONDITION_HRESULT(result, this->sessionDescription->Parse(rawSDP, rawSDPlength), result, E_FAIL);
    //        }

    //        if (SUCCEEDED(result))
    //        {
    //          // check for normal play time range attribute in session description
    //          // if normal play time range attribute has start and end time, then session is not live session

    //          CNormalPlayTimeRangeAttribute *nptAttribute = NULL;
    //          for (unsigned int i = 0; i < this->sessionDescription->GetAttributes()->Count(); i++)
    //          {
    //            CAttribute *attribute = this->sessionDescription->GetAttributes()->GetItem(i);

    //            if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_NORMAL_PLAY_TIME_RANGE))
    //            {
    //              nptAttribute = dynamic_cast<CNormalPlayTimeRangeAttribute *>(attribute);
    //              break;
    //            }
    //          }

    //          // RTSP is by default for live streams
    //          this->liveStream = true;

    //          if (nptAttribute != NULL)
    //          {
    //            if (nptAttribute->IsSetStartTime() && nptAttribute->IsSetEndTime() && (nptAttribute->GetEndTime() >= nptAttribute->GetStartTime()))
    //            {
    //              // not live stream
    //              this->liveStream = false;
    //            }
    //          }

    //          // if in configuration is specified live stream flag, then use it and ignore normal play time attribute
    //          this->liveStream = this->configurationParameters->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, this->liveStream);
    //        }

    //        // all parameters set
    //        // start receiving data

    //        CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->StartReceivingData(), result, E_FAIL);
    //      }
    //    }

    //    if (SUCCEEDED(result))
    //    {
    //      // create same count of RTSP stream tracks as RTSP tracks in CURL instance
    //      CLockMutex(this->lockCurlMutex, INFINITE);

    //      if (this->streamTracks->Count() != this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())
    //      {
    //        this->streamTracks->Clear();

    //        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
    //        {
    //          CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
    //          CRtspStreamTrack *streamTrack = new CRtspStreamTrack();
    //          CHECK_POINTER_HRESULT(result, streamTrack, result, E_OUTOFMEMORY);

    //          streamTrack->SetClockFrequency(track->GetStatistics()->GetClockFrequency());
    //          streamTrack->SetReceivedAllDataFlag(false);

    //          CHECK_CONDITION_HRESULT(result, this->streamTracks->Add(streamTrack), result, E_OUTOFMEMORY);
    //          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamTrack));
    //        }

    //        // for each stream track add first stream fragment
    //        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
    //        {
    //          CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //          CRtspStreamFragment *fragment = new CRtspStreamFragment();
    //          CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

    //          CHECK_CONDITION_HRESULT(result, track->GetStreamFragments()->Add(fragment), result, E_OUTOFMEMORY);

    //          if (SUCCEEDED(result))
    //          {
    //            track->SetStreamFragmentToDownload(0);
    //          }
    //          else
    //          {
    //            FREE_MEM_CLASS(fragment);
    //          }
    //        }
    //      }

    //      // set stream fragment downloading for each track (based on start time)
    //      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
    //      {
    //        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //        track->SetStreamFragmentDownloading(track->GetStreamFragmentToDownload());

    //        // we are downloading stream fragment, so reset scheduled download
    //        track->SetStreamFragmentToDownload(UINT_MAX);
    //      }
    //    }

    //    if (FAILED(result))
    //    {
    //      // clear connected state to call StartReceiveData() method in parser hoster, after first fail we need to reopen connection until timeout (GetReceiveDataTimeout()) comes
    //      // free RTSP CURL instance
    //      // in next run we try to open connection
    //      this->isConnected = false;
    //      FREE_MEM_CLASS(this->mainCurlInstance);
    //      result = S_OK;
    //    }
    //  }

    //  // store stream fragments to temporary file
    //  if (this->wholeStreamDownloaded || ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT))
    //  {
    //    this->lastStoreTime = GetTickCount();

    //    if (this->streamTracks->Count() > 0)
    //    {
    //      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
    //      {
    //        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    //        // in case of live stream remove all downloaded and processed stream fragments before reported stream time
    //        if ((this->liveStream) && (this->reportedStreamTime > 0))
    //        {
    //          int64_t reportedStreamTimeRtpTimestamp = (int64_t)this->reportedStreamTime * track->GetClockFrequency() / 1000;

    //          unsigned int fragmentRemoveCount = 0;
    //          while (fragmentRemoveCount < track->GetStreamFragmentProcessing())
    //          {
    //            CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(fragmentRemoveCount);

    //            if (fragment->IsDownloaded() && (fragment->GetFragmentRtpTimestamp() < (int64_t)reportedStreamTimeRtpTimestamp))
    //            {
    //              // fragment will be removed
    //              fragmentRemoveCount++;
    //            }
    //            else
    //            {
    //              break;
    //            }
    //          }

    //          if ((fragmentRemoveCount > 0) && (track->GetCacheFile()->RemoveItems(track->GetStreamFragments(), 0, fragmentRemoveCount)))
    //          {
    //            track->GetStreamFragments()->Remove(0, fragmentRemoveCount);

    //            track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() - fragmentRemoveCount);

    //            if (track->GetStreamFragmentDownloading() != UINT_MAX)
    //            {
    //              track->SetStreamFragmentDownloading(track->GetStreamFragmentDownloading() - fragmentRemoveCount);
    //            }

    //            if (track->GetStreamFragmentToDownload() != UINT_MAX)
    //            {
    //              track->SetStreamFragmentToDownload(track->GetStreamFragmentToDownload() - fragmentRemoveCount);
    //            }
    //          }
    //        }

    //        if (track->GetCacheFile()->GetCacheFile() == NULL)
    //        {
    //          wchar_t *storeFilePath = this->GetStoreFile(i);
    //          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, track->GetCacheFile()->SetCacheFile(storeFilePath));
    //          FREE_MEM(storeFilePath);
    //        }

    //        // store all stream fragments (which are not stored) to file
    //        if ((track->GetCacheFile()->GetCacheFile() != NULL) && (track->GetStreamFragments()->Count() != 0))
    //        {
    //          track->GetCacheFile()->StoreItems(track->GetStreamFragments(), this->lastStoreTime, this->wholeStreamDownloaded);
    //        }
    //      }
    //    }
    //  }
    //}
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

unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
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

  this->connectionState = SUCCEEDED(result) ? Opening : None;

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);

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

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  HRESULT result = __super::ClearSession();
 
  this->receiveDataTimeout = RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->connectionState = None;
  this->streamTracks->Clear();

  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

  FREE_MEM_CLASS(this->sessionDescription);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return result;
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

    this->flags &= ~(PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED | PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED);
    this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

    // in all tracks is set stream fragment to process
    // in all tracks exists at least one stream fragment and cover all possible RTP timestamps (0 - INT64_MAX)

    bool downloadedFragment = true;
    for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
    {
      CRtspStreamTrack *track = this->streamTracks->GetItem(i);

      track->SetStreamLength(0);
      track->SetStreamLengthFlag(false);
      track->SetEndOfStreamFlag(false);

      CRtspStreamFragment *processingFragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());

      if (!processingFragment->IsDownloaded())
      {
        track->SetFirstRtpPacketTimestamp(0, false, 0);
        track->SetRtpTimestampCorrection(0);

        // stream fragment is not downloaded, it is gap
        // split stream fragment

        downloadedFragment = false;
        result = time;

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
      unsigned int firstNotDownloadedFragmentIndex = track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(track->GetStreamFragmentProcessing());
      track->GetStreamFragments()->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? track->GetStreamFragments()->Count() : firstNotDownloadedFragmentIndex) - track->GetStreamFragmentProcessing());
    }

    if (!downloadedFragment)
    {
      // close connection
      this->StopReceivingData();

      HRESULT res = S_OK;
      CParameterCollection *temp = new CParameterCollection(&res);
      CHECK_POINTER_HRESULT(res, temp, res, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(res), this->StartReceivingData(temp), res);
      FREE_MEM_CLASS(temp);

      result = SUCCEEDED(res) ? result : res;
    }
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
    this->logger->SetParameters(protocolConfiguration->GetConfiguration());
    this->configuration->Clear();

    CHECK_CONDITION_HRESULT(result, this->configuration->Append(protocolConfiguration->GetConfiguration()), result, E_OUTOFMEMORY);

    this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

    this->receiveDataTimeout = this->configuration->GetValueLong(PARAMETER_NAME_RTSP_RECEIVE_DATA_TIMEOUT, true, RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT);
    this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

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

void CMPUrlSourceSplitter_Protocol_Rtsp::RecalculateStreamFragmentStartPosition(CRtspStreamFragmentCollection *streamFragments, unsigned int startIndex)
{
  for (unsigned int i = startIndex; i < streamFragments->Count(); i++)
  {
    CRtspStreamFragment *fragment = streamFragments->GetItem(i);
    CRtspStreamFragment *previousFragment = (i > 0) ? streamFragments->GetItem(i - 1) : NULL;

    if (fragment->IsDownloaded())
    {
      if ((previousFragment != NULL) && (previousFragment->IsDownloaded()))
      {
        fragment->SetFragmentStartPosition(previousFragment->GetFragmentStartPosition() + previousFragment->GetLength());
      }

      if (i >= (streamFragments->GetStartSearchingIndex() + streamFragments->GetSearchCount()))
      {
        streamFragments->SetSearchCount(i + 1 - streamFragments->GetStartSearchingIndex());
      }
    }
    else
    {
      // we found not downloaded stream fragment, stop recalculating start positions
      break;
    }
  }
}

//CMPUrlSourceSplitter_Protocol_Rtsp::CMPUrlSourceSplitter_Protocol_Rtsp(CLogger *logger, CParameterCollection *configuration)
//{
//  this->configurationParameters = new CParameterCollection();
//  if (configuration != NULL)
//  {
//    this->configurationParameters->Append(configuration);
//  }
//
//  this->logger = new CLogger(logger);
//  this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);
//
//  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_RTSP);
//  if (version != NULL)
//  {
//    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
//  }
//  FREE_MEM(version);
//
//  version = CCurlInstance::GetCurlVersion();
//  if (version != NULL)
//  {
//    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
//  }
//  FREE_MEM(version);
//  
//  this->receiveDataTimeout = RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT;
//  this->streamTime = 0;
//  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
//  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
//  this->internalExitRequest = false;
//  this->wholeStreamDownloaded = false;
//  this->mainCurlInstance = NULL;
//  this->isConnected = false;
//  this->streamTracks = new CRtspStreamTrackCollection();
//  this->lastStoreTime = 0;
//  this->liveStream = false;
//  this->sessionDescription = NULL;
//  this->reportedStreamTime = 0;
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
//}
//
//CMPUrlSourceSplitter_Protocol_Rtsp::~CMPUrlSourceSplitter_Protocol_Rtsp()
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//
//  if (this->IsConnected())
//  {
//    this->StopReceivingData();
//  }
//
//  FREE_MEM_CLASS(this->mainCurlInstance);
//  FREE_MEM_CLASS(this->configurationParameters);
//  FREE_MEM_CLASS(this->streamTracks);
//  FREE_MEM_CLASS(this->sessionDescription);
//
//  if (this->lockMutex != NULL)
//  {
//    CloseHandle(this->lockMutex);
//    this->lockMutex = NULL;
//  }
//
//  if (this->lockCurlMutex != NULL)
//  {
//    CloseHandle(this->lockCurlMutex);
//    this->lockCurlMutex = NULL;
//  }
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//  FREE_MEM_CLASS(this->logger);
//}
//
//// IProtocol interface
//
//bool CMPUrlSourceSplitter_Protocol_Rtsp::IsConnected(void)
//{
//  return ((this->isConnected) || (this->mainCurlInstance != NULL) || (this->wholeStreamDownloaded));
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ParseUrl(const CParameterCollection *parameters)
//{
//  HRESULT result = S_OK;
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
//  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);
//
//  this->ClearSession();
//
//  if (SUCCEEDED(result))
//  {
//    this->configurationParameters->Clear();
//    ALLOC_MEM_DEFINE_SET(protocolConfiguration, ProtocolPluginConfiguration, 1, 0);
//    if (protocolConfiguration != NULL)
//    {
//      protocolConfiguration->configuration = (CParameterCollection *)parameters;
//    }
//    this->Initialize(protocolConfiguration);
//    FREE_MEM(protocolConfiguration);
//  }
//
//  const wchar_t *url = this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL);
//  if (SUCCEEDED(result))
//  {
//    result = (url == NULL) ? E_OUTOFMEMORY : S_OK;
//  }
//
//  if (SUCCEEDED(result))
//  {
//    ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
//    if (urlComponents == NULL)
//    {
//      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'url components'");
//      result = E_OUTOFMEMORY;
//    }
//
//    if (SUCCEEDED(result))
//    {
//      ZeroURL(urlComponents);
//      urlComponents->dwStructSize = sizeof(URL_COMPONENTS);
//
//      this->logger->Log(LOGGER_INFO, L"%s: %s: url: %s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, url);
//
//      if (!InternetCrackUrl(url, 0, 0, urlComponents))
//      {
//        this->logger->Log(LOGGER_ERROR, L"%s: %s: InternetCrackUrl() error: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, GetLastError());
//        result = E_FAIL;
//      }
//    }
//
//    if (SUCCEEDED(result))
//    {
//      int length = urlComponents->dwSchemeLength + 1;
//      ALLOC_MEM_DEFINE_SET(protocol, wchar_t, length, 0);
//      if (protocol == NULL) 
//      {
//        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for 'protocol'");
//        result = E_OUTOFMEMORY;
//      }
//
//      if (SUCCEEDED(result))
//      {
//        wcsncat_s(protocol, length, urlComponents->lpszScheme, urlComponents->dwSchemeLength);
//
//        bool supportedProtocol = false;
//        for (int i = 0; i < TOTAL_SUPPORTED_PROTOCOLS; i++)
//        {
//          if (_wcsnicmp(urlComponents->lpszScheme, SUPPORTED_PROTOCOLS[i], urlComponents->dwSchemeLength) == 0)
//          {
//            supportedProtocol = true;
//            break;
//          }
//        }
//
//        if (!supportedProtocol)
//        {
//          // not supported protocol
//          this->logger->Log(LOGGER_INFO, L"%s: %s: unsupported protocol '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
//          result = E_FAIL;
//        }
//      }
//      FREE_MEM(protocol);
//    }
//
//    FREE_MEM(urlComponents);
//  }
//
//  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ReceiveData(CReceiveData *receiveData)
//{
//  HRESULT result = S_OK;
//
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  if (this->internalExitRequest)
//  {
//    // there is internal exit request pending == changed timestamp
//    // close connection
//    this->StopReceivingData();
//
//    // reopen connection
//    // StartReceivingData() reset wholeStreamDownloaded
//    this->StartReceivingData(NULL);
//
//    this->internalExitRequest = false;
//  }
//
//  // RTSP can have several streams
//  if ((this->IsConnected()) && (!this->wholeStreamDownloaded) && (receiveData->SetStreamCount(this->streamTracks->Count())))
//  {
//    receiveData->SetLiveStream(this->liveStream);
//
//    if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && (this->streamTracks->Count() != 0))
//    {
//      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
//      {
//        CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
//        CStreamReceiveData *stream = receiveData->GetStreams()->GetItem(i);
//
//        stream->SetStreamInputFormat(track->GetPayloadType()->GetStreamInputFormat());
//        stream->SetContainer(track->GetPayloadType()->IsSetFlags(RTSP_PAYLOAD_TYPE_FLAG_CONTAINER));
//        stream->SetPackets(track->GetPayloadType()->IsSetFlags(RTSP_PAYLOAD_TYPE_FLAG_PACKETS));
//      }
//    }
//
//    if (SUCCEEDED(result) && (this->mainCurlInstance != NULL))
//    {
//      CRtpPacketCollection *rtpPackets = new CRtpPacketCollection();
//      CHECK_POINTER_HRESULT(result, rtpPackets, result, E_OUTOFMEMORY);
//
//      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
//      {
//        CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
//        CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);
//
//        {
//          // copy RTP packets to avoid blocking of RTSP download instance
//          CLockMutex lockData(this->lockCurlMutex, INFINITE);
//
//          rtpPackets->Append(track->GetRtpPackets());
//          track->GetRtpPackets()->Clear();
//        }
//
//        if (rtpPackets->Count() != 0)
//        {
//          // split or add received RTP packets to stream fragments
//          CRtspStreamFragment *currentDownloadingFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());
//
//          for (unsigned int j = 0; (SUCCEEDED(result) && (currentDownloadingFragment != NULL) && (j < rtpPackets->Count())); j++)
//          {
//            CRtpPacket *rtpPacket = rtpPackets->GetItem(j);
//
//            // set first RTP packet timestamp (if not already set)
//            // timestamp is needed to compute (almost - after seeking we only assume that timestamps are correct) correct timestamps of fragments
//            if (!streamTrack->IsSetFirstRtpPacketTimestamp())
//            {
//              DWORD ticks = GetTickCount();
//              streamTrack->SetFirstRtpPacketTimestamp(rtpPacket->GetTimestamp(), true, ticks);
//
//              // change current downloading fragment RTP timestamp to correct timestamp based on time from start of stream (first time set ticks to track)
//              if (this->liveStream)
//              {
//                int64_t timestamp = ((int64_t)ticks - (int64_t)streamTrack->GetFirstRtpPacketTicks()) * streamTrack->GetClockFrequency() / 1000;
//
//                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, fragment timestamp changing from %lld to %lld, ticks: %u, track ticks: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), timestamp, ticks, streamTrack->GetFirstRtpPacketTicks());
//                currentDownloadingFragment->SetFragmentRtpTimestamp(timestamp, false);
//              }
//
//              // compute correction for actual stream track
//              int64_t correction = currentDownloadingFragment->GetFragmentRtpTimestamp() - (int64_t)rtpPacket->GetTimestamp();
//
//              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, fragment timestamp: %lld, first RTP packet timestamp: %u, RTP: %u, ticks: %u, track ticks: %u, correction: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), rtpPacket->GetTimestamp(), rtpPacket->GetSequenceNumber(), ticks, streamTrack->GetFirstRtpPacketTicks(), correction);
//              streamTrack->SetRtpTimestampCorrection(correction);
//            }
//
//            int64_t timestamp = streamTrack->GetRtpPacketTimestamp(rtpPacket->GetTimestamp(), true) + streamTrack->GetRtpTimestampCorrection();
//
//            if (!currentDownloadingFragment->IsSetFragmentRtpTimestamp())
//            {
//              currentDownloadingFragment->SetFragmentRtpTimestamp(timestamp);
//            }
//
//            CRtspStreamFragment *nextFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading() + 1);
//
//            // nextFragment can be NULL in case that we are on the end of collection
//
//            if (nextFragment != NULL)
//            {
//              if (timestamp >= nextFragment->GetFragmentRtpTimestamp())
//              {
//                // our RTP packet timestamp is greater than or equal to next fragment timestamp
//                // this means that we are receiving data, which we already have - in case if next fragment is downloaded
//
//                currentDownloadingFragment->SetDownloaded(true);
//
//                if (nextFragment->IsDownloaded())
//                {
//                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, found next RTSP stream fragment with lower timestamp as receiving stream fragment, stopping downloading fragment, receiving fragment timestamp: %lld, next fragment timestamp: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), nextFragment->GetFragmentRtpTimestamp());
//
//                  currentDownloadingFragment = NULL;
//                  streamTrack->SetStreamFragmentDownloading(UINT_MAX);
//                }
//                else
//                {
//                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, found next not downloaded RTSP stream fragment with lower timestamp as receiving stream fragment, continuing downloading fragment, receiving fragment timestamp: %lld, next fragment timestamp: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, currentDownloadingFragment->GetFragmentRtpTimestamp(), nextFragment->GetFragmentRtpTimestamp());
//
//                  nextFragment->SetFragmentRtpTimestamp(timestamp);
//
//                  currentDownloadingFragment = nextFragment;
//                  streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
//                }
//              }
//            }
//
//            if ((currentDownloadingFragment != NULL) && (timestamp != currentDownloadingFragment->GetFragmentRtpTimestamp()))
//            {
//              CRtspStreamFragment *nextFragment = new CRtspStreamFragment(timestamp, true);
//              CHECK_POINTER_HRESULT(result, nextFragment, result, E_OUTOFMEMORY);
//
//              if (SUCCEEDED(result))
//              {
//                result = streamTrack->GetStreamFragments()->Insert(streamTrack->GetStreamFragmentDownloading() + 1, nextFragment) ? result : E_OUTOFMEMORY;
//              }
//
//              if (SUCCEEDED(result))
//              {
//                currentDownloadingFragment->SetDownloaded(true);
//                streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
//              }
//              else
//              {
//                FREE_MEM_CLASS(nextFragment);
//              }
//
//              currentDownloadingFragment = nextFragment;
//            }
//
//            // add RTP packet to current downloading stream fragment
//            if (SUCCEEDED(result) && (currentDownloadingFragment != NULL))
//            {
//              result = (currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(rtpPacket->GetPayload(), rtpPacket->GetPayloadSize(), currentDownloadingFragment->GetBuffer()->GetBufferSize() * 2) == rtpPacket->GetPayloadSize()) ? result : E_OUTOFMEMORY;
//            }
//          }
//        }
//
//        if (streamTrack->GetStreamFragmentDownloading() != UINT_MAX)
//        {
//          CLockMutex lockData(this->lockCurlMutex, INFINITE);
//
//          if (track->IsEndOfStream() && (track->GetRtpPackets()->Count() == 0))
//          {
//            // mark currently downloading stream fragment as downloaded
//            CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());
//            if (fragment != NULL)
//            {
//              fragment->SetDownloaded(true);
//            }
//
//            streamTrack->SetStreamFragmentDownloading(UINT_MAX);
//          }
//        }
//
//        rtpPackets->Clear();
//      }
//
//      FREE_MEM_CLASS(rtpPackets);
//    }
//
//    bool allTracksSetFirstRtpPacketTimestamp = (this->streamTracks->Count() != 0);
//
//    if (SUCCEEDED(result))
//    {
//      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//      {
//        CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//        allTracksSetFirstRtpPacketTimestamp &= track->IsSetFirstRtpPacketTimestamp();
//      }
//    }
//
//    for (unsigned int i = 0; (SUCCEEDED(result) && (allTracksSetFirstRtpPacketTimestamp) && (i < this->streamTracks->Count())); i++)
//    {
//      CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//      if ((!track->IsSetSupressData()) && (track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count()))
//      {
//        bool loadStreamFragmentToMemory = true;
//
//        while (track->GetCacheFile()->LoadItems(track->GetStreamFragments(), track->GetStreamFragmentProcessing(), loadStreamFragmentToMemory, true))
//        {
//          // do not load next stream fragment from cache file - in another case it leads to situation that whole cache file will be loaded to memory
//          loadStreamFragmentToMemory = false;
//
//          CLinearBuffer *bufferForProcessing = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing())->GetBuffer();
//          unsigned int bufferOccupiedSpace = bufferForProcessing->GetBufferOccupiedSpace();
//
//          if (bufferOccupiedSpace > 0)
//          {
//            // create media packet
//            // set values of media packet
//
//            CMediaPacket *mediaPacket = new CMediaPacket();
//            CHECK_POINTER_HRESULT(result, mediaPacket, result, E_OUTOFMEMORY);
//            CHECK_CONDITION_HRESULT(result, mediaPacket->GetBuffer()->AddToBufferWithResize(bufferForProcessing) == bufferOccupiedSpace, result, E_OUTOFMEMORY);
//
//            if (SUCCEEDED(result))
//            {
//              mediaPacket->SetStart(track->GetBytePosition());
//              mediaPacket->SetEnd(track->GetBytePosition() + bufferOccupiedSpace - 1);
//
//              int64_t pts = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing())->GetFragmentRtpTimestamp();
//              pts = track->GetRtpPacketTimestampInDshowTimeBaseUnits(pts);
//
//              mediaPacket->SetPresentationTimestamp(pts);
//              mediaPacket->SetPresentationTimestampTicksPerSecond(DSHOW_TIME_BASE);
//
//              result = receiveData->GetStreams()->GetItem(i)->GetMediaPacketCollection()->Add(mediaPacket) ? result : E_OUTOFMEMORY;
//            }
//
//            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaPacket));
//          }
//
//          if (SUCCEEDED(result))
//          {
//            track->SetBytePosition(track->GetBytePosition() + bufferOccupiedSpace);
//            track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() + 1);
//
//            // check if fragment is downloaded
//            // if fragment is not downloaded, then schedule it for download
//            if (track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count())
//            {
//              CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());
//
//              if ((!fragment->IsDownloaded()) && (track->GetStreamFragmentProcessing() != track->GetStreamFragmentDownloading()))
//              {
//                // fragment is not downloaded and also is not downloading currently
//                track->SetStreamFragmentDownloading(UINT_MAX);
//                track->SetStreamFragmentToDownload(track->GetStreamFragmentProcessing());
//              }
//            }
//          }
//        }
//      }
//
//      // adjust total length (if necessary)
//      if (!track->IsSetStreamLength())
//      {
//        if (track->GetStreamLength() == 0)
//        {
//          // stream length not set
//          // just make guess
//
//          track->SetStreamLength(int64_t(MINIMUM_RECEIVED_DATA_FOR_SPLITTER));
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
//
//          receiveData->GetStreams()->GetItem(i)->GetTotalLength()->SetTotalLength(track->GetStreamLength(), true);
//        }
//        else if ((track->GetBytePosition() > (track->GetStreamLength() * 3 / 4)))
//        {
//          // it is time to adjust stream length, we are approaching to end but still we don't know total length
//          track->SetStreamLength(track->GetBytePosition() * 2);
//          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
//
//          receiveData->GetStreams()->GetItem(i)->GetTotalLength()->SetTotalLength(track->GetStreamLength(), true);
//        }
//      }
//    }
//
//    if ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
//    {
//      // all data received, we're not receiving data
//      if (this->mainCurlInstance->GetDownloadResponse()->GetResultCode() == CURLE_OK)
//      {
//        bool receivedAllData = true;
//
//        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
//        {
//          CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
//          CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);
//
//          if ((track->GetRtpPackets()->Count() == 0) && (!streamTrack->IsReceivedAllData()))
//          {
//            // all data read from CURL instance for track i, received all data
//            this->logger->Log(LOGGER_INFO, L"%s: %s: track %u, all data received", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i);
//            streamTrack->SetReceivedAllDataFlag(true);
//          }
//          else
//          {
//            receivedAllData = false;
//          }
//        }
//
//        CHECK_CONDITION_EXECUTE(receivedAllData, this->mainCurlInstance->StopReceivingData());
//      }
//      else
//      {
//        // error occured while downloading
//        // download stream fragment again or download scheduled stream fragment
//
//        this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: %d, restarting download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetResultCode());
//
//        for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//        {
//          CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//          track->SetStreamFragmentToDownload(track->GetStreamFragmentDownloading());
//          track->SetStreamFragmentDownloading(UINT_MAX);
//        }
//
//        FREE_MEM_CLASS(this->mainCurlInstance);
//      }
//    }
//
//    if (SUCCEEDED(result) && (this->streamTracks->Count() != 0))
//    {
//      bool allStreamFragmentsDownloaded = true;
//
//      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//      {
//        CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//        if ((track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count()) ||
//          (track->GetStreamFragmentToDownload() != UINT_MAX) ||
//          (track->GetStreamFragmentDownloading() != UINT_MAX) ||
//          (track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) != UINT_MAX))
//        {
//          allStreamFragmentsDownloaded = false;
//          break;
//        }
//      }
//
//      if (allStreamFragmentsDownloaded)
//      {
//        // all fragments downloaded and processed
//        // whole stream downloaded
//        this->wholeStreamDownloaded = true;
//        this->isConnected = false;
//
//        this->mainCurlInstance->StopReceivingData();
//
//        for (unsigned int i = 0; (i < this->streamTracks->Count()); i++)
//        {
//          CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//          if (track->GetStreamFragmentProcessing() >= track->GetStreamFragments()->Count())
//          {
//            // we are not seeking, so we can set total length
//
//            // all stream fragments processed
//            // set stream length and report end of stream
//
//            if (!track->IsSetStreamLength())
//            {
//              track->SetStreamLength(track->GetBytePosition());
//              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
//              track->SetStreamLengthFlag(true);
//
//              receiveData->GetStreams()->GetItem(i)->GetTotalLength()->SetTotalLength(track->GetStreamLength(), false);
//            }
//
//            if (!track->IsSetEndOfStream())
//            {
//              // notify filter the we reached end of stream
//              track->SetEndOfStreamFlag(true);
//
//              receiveData->GetStreams()->GetItem(i)->GetEndOfStreamReached()->SetStreamPosition(max(0, track->GetBytePosition() - 1));
//            }
//          }
//        }
//      }      
//    }
//
//    // check if there isn't some fragment to download
//    bool anyTrackDownloading = false;
//    bool startFragmentDownload = false;
//
//    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
//    {
//      CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//      
//      anyTrackDownloading |= (track->GetStreamFragmentDownloading() != UINT_MAX);
//
//      if (track->GetStreamFragmentDownloading() == UINT_MAX)
//      {
//        unsigned int fragmentToDownload = UINT_MAX;
//
//        // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
//        fragmentToDownload = (track->GetStreamFragmentToDownload() == UINT_MAX) ? track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(track->GetStreamFragmentProcessing()) : track->GetStreamFragmentToDownload();
//        // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
//        fragmentToDownload = (fragmentToDownload == UINT_MAX) ? track->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) : fragmentToDownload;
//        // fragment to download still can be UINT_MAX = no fragment to download
//
//        track->SetStreamFragmentToDownload(fragmentToDownload);
//        startFragmentDownload |= (fragmentToDownload != UINT_MAX);
//      }
//    }
//
//    if (SUCCEEDED(result) && (this->isConnected) && ((this->mainCurlInstance == NULL) || ((startFragmentDownload) && (!anyTrackDownloading))))
//    {
//      int64_t startTime = INT64_MAX;
//
//      // if seeking to empty place (no downloaded fragment before) then use fragment time
//      // if seeking to fill gap (one or more downloaded fragment(s) before) then use previous fragment time
//      // if seeking in live stream (e.g. lost connection) then use zero time
//
//      for (unsigned int i = 0; (SUCCEEDED(result) && (!this->liveStream) && (i < this->streamTracks->Count())); i++)
//      {
//        CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//        unsigned int fragmentToDownload = track->GetStreamFragmentToDownload();
//
//        if (fragmentToDownload != UINT_MAX)
//        {
//          CRtspStreamFragment *firstFragment = track->GetStreamFragments()->GetItem(0);
//          CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(fragmentToDownload);
//
//          int64_t fragmentRtpTimestamp = INT64_MAX;
//
//          if (fragmentToDownload > 0)
//          {
//            CRtspStreamFragment *previousFragment = track->GetStreamFragments()->GetItem(fragmentToDownload - 1);
//
//            if (previousFragment->IsDownloaded())
//            {
//              fragmentRtpTimestamp = min(previousFragment->GetFragmentRtpTimestamp(), fragmentRtpTimestamp);
//            }
//          }
//          
//          fragmentRtpTimestamp = min(fragment->GetFragmentRtpTimestamp(), fragmentRtpTimestamp);
//          fragmentRtpTimestamp *= 1000;
//          fragmentRtpTimestamp /= track->GetClockFrequency();
//
//          startTime = min(startTime, fragmentRtpTimestamp);
//        }
//      }
//
//      startTime = (startTime != INT64_MAX) ? startTime: 0;
//
//      FREE_MEM_CLASS(this->mainCurlInstance);
//      FREE_MEM_CLASS(this->sessionDescription);
//
//      // new connection will be created with new RTP packet timestamps
//      // clear set RTP packet timestamp flags
//      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//      {
//        CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//        track->SetFirstRtpPacketTimestamp(UINT_MAX, false, 0);
//        track->SetReceivedAllDataFlag(false);
//
//        // clear all not downloaded stream fragments
//        // recalculate stream fragments timestams for not downloaded stream fragments
//        for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
//        {
//          CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);
//
//          if (!fragment->IsDownloaded())
//          {
//            fragment->GetBuffer()->ClearBuffer();
//
//            fragment->SetFragmentRtpTimestamp(fragment->GetFragmentRtpTimestamp(), false);
//          }
//        }
//      }
//
//      this->mainCurlInstance = new CRtspCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
//      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);
//
//      if (SUCCEEDED(result))
//      {
//        // set connection priorities
//        this->mainCurlInstance->SetMulticastPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_MULTICAST_PREFERENCE, true, RTSP_MULTICAST_PREFERENCE_DEFAULT));
//        this->mainCurlInstance->SetSameConnectionTcpPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_SAME_CONNECTION_TCP_PREFERENCE, true, RTSP_SAME_CONNECTION_TCP_PREFERENCE_DEFAULT));
//        this->mainCurlInstance->SetUdpPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_UDP_PREFERENCE, true, RTSP_UDP_PREFERENCE_DEFAULT));
//
//        // set ports
//        this->mainCurlInstance->SetRtspClientPortMin(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MIN, true, RTSP_CLIENT_PORT_MIN_DEFAULT));
//        this->mainCurlInstance->SetRtspClientPortMax(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MAX, true, RTSP_CLIENT_PORT_MAX_DEFAULT));
//
//        this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
//        this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
//        this->mainCurlInstance->SetIgnoreRtpPayloadType(this->configurationParameters->GetValueBool(PARAMETER_NAME_RTSP_IGNORE_RTP_PAYLOAD_TYPE, true, RTSP_IGNORE_RTP_PAYLOAD_TYPE_DEFAULT));
//      }
//
//      if (SUCCEEDED(result))
//      {
//        CRtspDownloadRequest *request = new CRtspDownloadRequest();
//        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);
//
//        if (SUCCEEDED(result))
//        {
//          request->SetStartTime(startTime);
//          request->SetUrl(this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
//
//          result = (this->mainCurlInstance->Initialize(request)) ? S_OK : E_FAIL;
//        }
//        FREE_MEM_CLASS(request);
//
//        if (SUCCEEDED(result))
//        {
//          if (this->sessionDescription == NULL)
//          {
//            const wchar_t *rawSDP = this->mainCurlInstance->GetRtspDownloadResponse()->GetRawSessionDescription();
//            unsigned int rawSDPlength = wcslen(rawSDP);
//
//            this->sessionDescription = new CSessionDescription();
//            CHECK_POINTER_HRESULT(result, this->sessionDescription, result, E_OUTOFMEMORY);
//            CHECK_CONDITION_HRESULT(result, this->sessionDescription->Parse(rawSDP, rawSDPlength), result, E_FAIL);
//          }
//
//          if (SUCCEEDED(result))
//          {
//            // check for normal play time range attribute in session description
//            // if normal play time range attribute has start and end time, then session is not live session
//
//            CNormalPlayTimeRangeAttribute *nptAttribute = NULL;
//            for (unsigned int i = 0; i < this->sessionDescription->GetAttributes()->Count(); i++)
//            {
//              CAttribute *attribute = this->sessionDescription->GetAttributes()->GetItem(i);
//
//              if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_NORMAL_PLAY_TIME_RANGE))
//              {
//                nptAttribute = dynamic_cast<CNormalPlayTimeRangeAttribute *>(attribute);
//                break;
//              }
//            }
//
//            // RTSP is by default for live streams
//            this->liveStream = true;
//
//            if (nptAttribute != NULL)
//            {
//              if (nptAttribute->IsSetStartTime() && nptAttribute->IsSetEndTime() && (nptAttribute->GetEndTime() >= nptAttribute->GetStartTime()))
//              {
//                // not live stream
//                this->liveStream = false;
//              }
//            }
//
//            // if in configuration is specified live stream flag, then use it and ignore normal play time attribute
//            this->liveStream = this->configurationParameters->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, this->liveStream);
//          }
//
//          // all parameters set
//          // start receiving data
//
//          CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->StartReceivingData(), result, E_FAIL);
//        }
//      }
//
//      if (SUCCEEDED(result))
//      {
//        // create same count of RTSP stream tracks as RTSP tracks in CURL instance
//        CLockMutex(this->lockCurlMutex, INFINITE);
//
//        if (this->streamTracks->Count() != this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())
//        {
//          this->streamTracks->Clear();
//
//          for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
//          {
//            CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
//            CRtspStreamTrack *streamTrack = new CRtspStreamTrack();
//            CHECK_POINTER_HRESULT(result, streamTrack, result, E_OUTOFMEMORY);
//
//            streamTrack->SetClockFrequency(track->GetStatistics()->GetClockFrequency());
//            streamTrack->SetReceivedAllDataFlag(false);
//
//            CHECK_CONDITION_HRESULT(result, this->streamTracks->Add(streamTrack), result, E_OUTOFMEMORY);
//            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamTrack));
//          }
//
//          // for each stream track add first stream fragment
//          for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
//          {
//            CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//            
//            CRtspStreamFragment *fragment = new CRtspStreamFragment();
//            CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);
//
//            CHECK_CONDITION_HRESULT(result, track->GetStreamFragments()->Add(fragment), result, E_OUTOFMEMORY);
//
//            if (SUCCEEDED(result))
//            {
//              track->SetStreamFragmentToDownload(0);
//            }
//            else
//            {
//              FREE_MEM_CLASS(fragment);
//            }
//          }
//        }
//
//        // set stream fragment downloading for each track (based on start time)
//        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
//        {
//          CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//          track->SetStreamFragmentDownloading(track->GetStreamFragmentToDownload());
//
//          // we are downloading stream fragment, so reset scheduled download
//          track->SetStreamFragmentToDownload(UINT_MAX);
//        }
//      }
//
//      if (FAILED(result))
//      {
//        // clear connected state to call StartReceiveData() method in parser hoster, after first fail we need to reopen connection until timeout (GetReceiveDataTimeout()) comes
//        // free RTSP CURL instance
//        // in next run we try to open connection
//        this->isConnected = false;
//        FREE_MEM_CLASS(this->mainCurlInstance);
//        result = S_OK;
//      }
//    }
//
//    // store stream fragments to temporary file
//    if (this->wholeStreamDownloaded || ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT))
//    {
//      this->lastStoreTime = GetTickCount();
//
//      if (this->streamTracks->Count() > 0)
//      {
//        for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//        {
//          CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//          // in case of live stream remove all downloaded and processed stream fragments before reported stream time
//          if ((this->liveStream) && (this->reportedStreamTime > 0))
//          {
//            int64_t reportedStreamTimeRtpTimestamp = (int64_t)this->reportedStreamTime * track->GetClockFrequency() / 1000;
//
//            unsigned int fragmentRemoveCount = 0;
//            while (fragmentRemoveCount < track->GetStreamFragmentProcessing())
//            {
//              CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(fragmentRemoveCount);
//
//              if (fragment->IsDownloaded() && (fragment->GetFragmentRtpTimestamp() < (int64_t)reportedStreamTimeRtpTimestamp))
//              {
//                // fragment will be removed
//                fragmentRemoveCount++;
//              }
//              else
//              {
//                break;
//              }
//            }
//
//            if ((fragmentRemoveCount > 0) && (track->GetCacheFile()->RemoveItems(track->GetStreamFragments(), 0, fragmentRemoveCount)))
//            {
//              track->GetStreamFragments()->Remove(0, fragmentRemoveCount);
//
//              track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() - fragmentRemoveCount);
//
//              if (track->GetStreamFragmentDownloading() != UINT_MAX)
//              {
//                track->SetStreamFragmentDownloading(track->GetStreamFragmentDownloading() - fragmentRemoveCount);
//              }
//
//              if (track->GetStreamFragmentToDownload() != UINT_MAX)
//              {
//                track->SetStreamFragmentToDownload(track->GetStreamFragmentToDownload() - fragmentRemoveCount);
//              }
//            }
//          }
//          
//          if (track->GetCacheFile()->GetCacheFile() == NULL)
//          {
//            wchar_t *storeFilePath = this->GetStoreFile(i);
//            CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, track->GetCacheFile()->SetCacheFile(storeFilePath));
//            FREE_MEM(storeFilePath);
//          }
//
//          // store all stream fragments (which are not stored) to file
//          if ((track->GetCacheFile()->GetCacheFile() != NULL) && (track->GetStreamFragments()->Count() != 0))
//          {
//            track->GetCacheFile()->StoreItems(track->GetStreamFragments(), this->lastStoreTime, this->wholeStreamDownloaded);
//          }
//        }
//      }
//    }
//  }
//
//  return result;
//}
//
//CParameterCollection *CMPUrlSourceSplitter_Protocol_Rtsp::GetConnectionParameters(void)
//{
//  CParameterCollection *result = new CParameterCollection();
//  bool retval = (result != NULL);
//
//  if (retval)
//  {
//    // add configuration parameters
//    retval &= result->Append(this->configurationParameters);
//  }
//
//  if (!retval)
//  {
//    FREE_MEM_CLASS(result);
//  }
//  
//  return result;
//}
//
//// ISimpleProtocol interface
//
//unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetReceiveDataTimeout(void)
//{
//  return this->receiveDataTimeout;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::StartReceivingData(CParameterCollection *parameters)
//{
//  HRESULT result = S_OK;
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);
//
//  CHECK_POINTER_DEFAULT_HRESULT(result, this->configurationParameters);
//
//  if (SUCCEEDED(result) && (parameters != NULL))
//  {
//    this->configurationParameters->Append(parameters);
//  }
//
//  // lock access to stream
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  this->wholeStreamDownloaded = false;
//
//  if (FAILED(result))
//  {
//    this->StopReceivingData();
//  }
//
//  this->isConnected = SUCCEEDED(result);
//
//  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::StopReceivingData(void)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
//
//  // lock access to stream
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  FREE_MEM_CLASS(this->mainCurlInstance);
//
//  this->isConnected = false;
//  for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//  {
//    CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//    track->SetStreamFragmentDownloading(UINT_MAX);
//  }
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
//  return S_OK;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::QueryStreamProgress(CStreamProgress *streamProgress)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
//  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() < this->streamTracks->Count(), result, E_INVALIDARG);
//
//  if (SUCCEEDED(result))
//  {
//    CRtspStreamTrack *track = this->streamTracks->GetItem(streamProgress->GetStreamId());
//
//    streamProgress->SetTotalLength((track->GetStreamLength() == 0) ? 1 : track->GetStreamLength());
//    streamProgress->SetCurrentLength((track->GetStreamLength() == 0) ? 0 : track->GetBytePosition());
//
//    if (!track->IsSetStreamLength())
//    {
//      result = VFW_S_ESTIMATED;
//    }
//  }
//
//  return result;
//}
//  
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);
//  CHECK_CONDITION_HRESULT(result, availableLength->GetStreamId() < this->streamTracks->Count(), result, E_INVALIDARG);
//
//  if (SUCCEEDED(result))
//  {
//    CRtspStreamTrack *track = this->streamTracks->GetItem(availableLength->GetStreamId());
//
//    availableLength->SetAvailableLength((track->IsSetStreamLength()) ? track->GetStreamLength() : track->GetBytePosition());
//  }
//
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ClearSession(void)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//
//  if (this->IsConnected())
//  {
//    this->StopReceivingData();
//  }
//
//  this->internalExitRequest = false;
//  this->streamTime = 0;
//  this->wholeStreamDownloaded = false;
//  this->receiveDataTimeout = RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT;
//  this->isConnected = false;
//  this->streamTracks->Clear();
//  this->liveStream = false;
//  FREE_MEM_CLASS(this->sessionDescription);
//  this->reportedStreamTime = 0;
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//  return S_OK;
//}
//
//int64_t CMPUrlSourceSplitter_Protocol_Rtsp::GetDuration(void)
//{
//  int64_t result = DURATION_LIVE_STREAM;
//
//  {
//    CLockMutex lock(this->lockMutex, INFINITE);
//
//    if ((!this->liveStream) && (this->sessionDescription != NULL))
//    {
//      CNormalPlayTimeRangeAttribute *nptAttribute = NULL;
//      for (unsigned int i = 0; i < this->sessionDescription->GetAttributes()->Count(); i++)
//      {
//        CAttribute *attribute = this->sessionDescription->GetAttributes()->GetItem(i);
//
//        if (attribute->IsInstanceTag(TAG_ATTRIBUTE_INSTANCE_NORMAL_PLAY_TIME_RANGE))
//        {
//          nptAttribute = dynamic_cast<CNormalPlayTimeRangeAttribute *>(attribute);
//          break;
//        }
//      }
//
//      if (nptAttribute != NULL)
//      {
//        if (nptAttribute->IsSetStartTime() && nptAttribute->IsSetEndTime() && (nptAttribute->GetEndTime() >= nptAttribute->GetStartTime()))
//        {
//          result = nptAttribute->GetEndTime() - nptAttribute->GetStartTime();
//        }
//      }
//    }
//  }
//
//  return result;
//}
//
//void CMPUrlSourceSplitter_Protocol_Rtsp::ReportStreamTime(uint64_t streamTime)
//{
//  this->reportedStreamTime = streamTime;
//}
//
//// ISeeking interface
//
//unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetSeekingCapabilities(void)
//{
//  unsigned int result = SEEKING_METHOD_NONE;
//  {
//    // lock access to stream
//    CLockMutex lock(this->lockMutex, INFINITE);
//
//    result = SEEKING_METHOD_TIME;
//  }
//  return result;
//}
//
//int64_t CMPUrlSourceSplitter_Protocol_Rtsp::SeekToTime(unsigned int streamId, int64_t time)
//{
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
//  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);
//
//  int64_t result = -1;
//
//  // find fragment to process for each track
//  // TO DO: implement better and faster seeking algorithm
//  for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//  {
//    CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//    for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
//    {
//      // first RTSP stream fragment has always zero timestamp
//      CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);
//      CRtspStreamFragment *nextFragment = track->GetStreamFragments()->GetItem(j + 1);
//
//      // calculate fragment time in ms
//      int64_t fragmentTime = fragment->GetFragmentRtpTimestamp() * 1000 / track->GetClockFrequency();
//      int64_t nextFragmentTime = (nextFragment == NULL) ? INT64_MAX : (nextFragment->GetFragmentRtpTimestamp() * 1000 / track->GetClockFrequency());
//
//      if ((fragmentTime <= time) && (nextFragmentTime >= time))
//      {
//        if (i >= streamId)
//        {
//          // set processing fragment only on equal to or greater than specified stream
//          // in other case we will send same data again
//          track->SetStreamFragmentProcessing(j);
//        }
//
//        result = (result == -1) ? fragmentTime : (min(result, fragmentTime));
//
//        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, time %lld, fragment: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, i, fragmentTime, j);
//        break;
//      }
//    }
//  }
//
//  if ((result != (-1)) && (streamId == 0))
//  {
//    // first seek is always on stream with ID zero
//
//    // reset whole stream downloaded, but IsConnected() must return true to avoid calling StartReceivingData()
//    this->isConnected = true;
//    this->wholeStreamDownloaded = false;
//
//    // in all tracks is set stream fragment to process
//    // in all tracks exists at least one stream fragment and cover all possible RTP timestamps (0 - INT64_MAX)
//
//    bool downloadedFragment = true;
//    for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//    {
//      CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//      track->SetBytePosition(0);
//      track->SetStreamLength(0);
//      track->SetStreamLengthFlag(false);
//      track->SetEndOfStreamFlag(false);
//
//      CRtspStreamFragment *processingFragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());
//
//      if (!processingFragment->IsDownloaded())
//      {
//        track->SetFirstRtpPacketTimestamp(0, false, 0);
//        track->SetRtpTimestampCorrection(0);
//
//        // stream fragment is not downloaded, it is gap
//        // split stream fragment
//
//        downloadedFragment = false;
//        result = time;
//
//        // create new fragment within found fragment
//        // calculate fragment RTP timestamp
//
//        // first fragment RTP timestamp is always zero, no need to correct timestamp
//        int64_t timestamp = time * track->GetClockFrequency() / 1000;
//        CRtspStreamFragment *fragment = new CRtspStreamFragment(timestamp, false);
//        result = (fragment != NULL) ? result : (-1);
//
//        if (result != (-1))
//        {
//          track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() + 1);
//
//          result = track->GetStreamFragments()->Insert(track->GetStreamFragmentProcessing(), fragment) ? result : (-1);
//        }
//
//        if (result != (-1))
//        {
//          // force to download missing fragment
//          track->SetStreamFragmentToDownload(track->GetStreamFragmentProcessing());
//        }
//      }
//    }
//
//    if (!downloadedFragment)
//    {
//      // close connection
//      this->StopReceivingData();
//
//      // reopen connection
//      // StartReceivingData() reset wholeStreamDownloaded
//      this->isConnected = SUCCEEDED(this->StartReceivingData(NULL));
//    }
//
//    if (!this->IsConnected())
//    {
//      result = -1;
//    }
//  }
//
//  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
//  return result;
//}
//
//void CMPUrlSourceSplitter_Protocol_Rtsp::SetSupressData(unsigned int streamId, bool supressData)
//{
//  // if supress data then disable all streams to send data to filter, otherwise allow only requested stream to send data to filter
//
//  if (supressData && (streamId == 0))
//  {
//    // supressing data come from all streams, but is relevant only on first stream (other streams will be supressed too)
//    for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
//    {
//      CRtspStreamTrack *track = this->streamTracks->GetItem(i);
//
//      track->SetSupressDataFlag(true);
//    }
//  }
//  else if (!supressData)
//  {
//    CRtspStreamTrack *track = this->streamTracks->GetItem(streamId);
//
//    track->SetSupressDataFlag(false);
//  }
//}
//
//// IPlugin interface
//
//const wchar_t *CMPUrlSourceSplitter_Protocol_Rtsp::GetName(void)
//{
//  return PROTOCOL_NAME;
//}
//
//GUID CMPUrlSourceSplitter_Protocol_Rtsp::GetInstanceId(void)
//{
//  return this->logger->GetLoggerInstanceId();
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::Initialize(PluginConfiguration *configuration)
//{
//  if (configuration == NULL)
//  {
//    return E_POINTER;
//  }
//
//  ProtocolPluginConfiguration *protocolConfiguration = (ProtocolPluginConfiguration *)configuration;
//  this->logger->SetParameters(protocolConfiguration->configuration);
//
//  if (this->lockMutex == NULL)
//  {
//    return E_FAIL;
//  }
//
//  this->configurationParameters->Clear();
//  if (protocolConfiguration->configuration != NULL)
//  {
//    this->configurationParameters->Append(protocolConfiguration->configuration);
//  }
//  this->configurationParameters->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
//
//  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_RTSP_RECEIVE_DATA_TIMEOUT, true, RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT);
//  this->liveStream = this->configurationParameters->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT);
//
//  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
//
//  return S_OK;
//}
//
//// other methods
//
//wchar_t *CMPUrlSourceSplitter_Protocol_Rtsp::GetStoreFile(unsigned int trackId)
//{
//  wchar_t *result = NULL;
//  const wchar_t *folder = this->configurationParameters->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);
//
//  if (folder != NULL)
//  {
//    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
//    if (guid != NULL)
//    {
//      result = FormatString(L"%smpurlsourcesplitter_protocol_rtsp_%s_track_%02u.temp", folder, guid, trackId);
//    }
//    FREE_MEM(guid);
//  }
//
//  return result;
//}