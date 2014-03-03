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

#define METHOD_FILL_BUFFER_FOR_PROCESSING_NAME                          L"FillBufferForProcessing()"

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Rtsp(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Rtsp *pClass = (CMPUrlSourceSplitter_Protocol_Rtsp *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Protocol_Rtsp::CMPUrlSourceSplitter_Protocol_Rtsp(CLogger *logger, CParameterCollection *configuration)
{
  this->configurationParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

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
  
  this->receiveDataTimeout = RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->streamTime = 0;
  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
  this->internalExitRequest = false;
  this->wholeStreamDownloaded = false;
  this->mainCurlInstance = NULL;
  this->supressData = false;
  this->isConnected = false;
  this->streamTracks = new CRtspStreamTrackCollection();
  this->lastStoreTime = 0;
  this->liveStream = false;
  this->seekingActive = false;
  this->sessionDescription = NULL;
  this->reportedStreamTime = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Protocol_Rtsp::~CMPUrlSourceSplitter_Protocol_Rtsp()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->configurationParameters);
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

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
  FREE_MEM_CLASS(this->logger);
}

// IProtocol interface

bool CMPUrlSourceSplitter_Protocol_Rtsp::IsConnected(void)
{
  return ((this->isConnected) || (this->mainCurlInstance != NULL) || (this->wholeStreamDownloaded));
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ParseUrl(const CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  this->ClearSession();

  if (SUCCEEDED(result))
  {
    this->configurationParameters->Clear();
    ALLOC_MEM_DEFINE_SET(protocolConfiguration, ProtocolPluginConfiguration, 1, 0);
    if (protocolConfiguration != NULL)
    {
      protocolConfiguration->configuration = (CParameterCollection *)parameters;
    }
    this->Initialize(protocolConfiguration);
    FREE_MEM(protocolConfiguration);
  }

  const wchar_t *url = this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL);
  if (SUCCEEDED(result))
  {
    result = (url == NULL) ? E_OUTOFMEMORY : S_OK;
  }

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

CompareDataResult CMPUrlSourceSplitter_Protocol_Rtsp::CompareData(CRtspStreamTrack *track, unsigned int fragmentIndex, unsigned int fragmentReceivedDataPosition, CRtpPacket *rtpPacket, unsigned int rtpPacketPosition, bool onlyInFragmentReceivedDataPosition)
{
  CompareDataResult result;
  result.position = -1;
  result.size = 0;

  CLinearBuffer *fragmentLinearBuffer = this->FillBufferForProcessing(track->GetStreamFragments(), fragmentIndex, track->GetStoreFilePath());

  unsigned int fragmentBufferPosition = fragmentReceivedDataPosition;
  unsigned int fragmentBufferSize = fragmentLinearBuffer->GetBufferOccupiedSpace();

  const unsigned char *rtpPacketBuffer = rtpPacket->GetPayload() + rtpPacketPosition;
  unsigned int rtpPacketBufferSize = rtpPacket->GetPayloadSize() - rtpPacketPosition;

  ALLOC_MEM_DEFINE_SET(fragmentBuffer, unsigned char, fragmentBufferSize, 0);

  if (((fragmentBuffer != NULL) && (rtpPacketBuffer != NULL)))
  {
    fragmentLinearBuffer->CopyFromBuffer(fragmentBuffer, fragmentBufferSize, 0);

    unsigned char *start = fragmentBuffer + fragmentBufferPosition;

    while (start != NULL)
    {
      fragmentBufferPosition = start - fragmentBuffer;

      unsigned int currentSize = min(fragmentBufferSize - fragmentBufferPosition, rtpPacketBufferSize);
      if ((currentSize >= MINIMUM_RTP_PACKET_OVERLAP) && (memcmp(rtpPacketBuffer, fragmentBuffer + fragmentBufferPosition, currentSize) == 0))
      {
        result.position = fragmentBufferPosition;
        result.size = currentSize;

        break;
      }

      fragmentBufferPosition++;
      start = (onlyInFragmentReceivedDataPosition) ? NULL : (unsigned char *)memchr(fragmentBuffer + fragmentBufferPosition, *rtpPacketBuffer, fragmentBufferSize - fragmentBufferPosition);
    }
  }

  FREE_MEM(fragmentBuffer);
  FREE_MEM_CLASS(fragmentLinearBuffer);

  return result;
}

CompareDataResult CMPUrlSourceSplitter_Protocol_Rtsp::CompareDataReversed(CRtspStreamTrack *track, unsigned int fragmentIndex, unsigned int fragmentReceivedDataPosition, CRtpPacket *rtpPacket, unsigned int rtpPacketPosition, bool onlyInFragmentReceivedDataPosition)
{
  CompareDataResult result;
  result.position = -1;
  result.size = 0;

  CLinearBuffer *fragmentLinearBuffer = this->FillBufferForProcessing(track->GetStreamFragments(), fragmentIndex, track->GetStoreFilePath());

  unsigned int fragmentBufferSize = fragmentLinearBuffer->GetBufferOccupiedSpace();
  ALLOC_MEM_DEFINE_SET(fragmentBuffer, unsigned char, fragmentBufferSize, 0);
  ALLOC_MEM_DEFINE_SET(reversedFragmentBuffer, unsigned char, fragmentBufferSize, 0);

  unsigned int rtpPacketBufferSize = rtpPacket->GetPayloadSize() - rtpPacketPosition;
  ALLOC_MEM_DEFINE_SET(rtpPacketBuffer, unsigned char, rtpPacketBufferSize, 0);

  unsigned int reversedFragmentBufferPosition = fragmentBufferSize - 1 - fragmentReceivedDataPosition;
  unsigned int reversedFragmentBufferSize = reversedFragmentBufferPosition + 1;

  if ((fragmentBuffer != NULL) && (reversedFragmentBuffer != NULL) && (rtpPacketBuffer != NULL))
  {
    fragmentLinearBuffer->CopyFromBuffer(fragmentBuffer, fragmentBufferSize, 0);

    for (unsigned int i = 0; i < fragmentBufferSize; i++)
    {
      reversedFragmentBuffer[i] = fragmentBuffer[fragmentBufferSize - 1 - i];
    }
    const unsigned char *rtpPacketBufferTemp = rtpPacket->GetPayload();
    for (unsigned int i = 0; i < rtpPacketBufferSize; i++)
    {
      rtpPacketBuffer[i] = rtpPacketBufferTemp[rtpPacketBufferSize - 1 - i];
    }

    unsigned char *start = NULL;

    do
    {
      start = NULL;

      // find start
      while (reversedFragmentBufferPosition > 0)
      {
        if (reversedFragmentBuffer[reversedFragmentBufferPosition] == rtpPacketBuffer[0])
        {
          start = reversedFragmentBuffer + reversedFragmentBufferPosition;
          break;
        }

        reversedFragmentBufferPosition--;
      }

      if (onlyInFragmentReceivedDataPosition)
      {
        start = ((reversedFragmentBufferSize - reversedFragmentBufferPosition) <= rtpPacketBufferSize) ? start : NULL;
      }

      if (start != NULL)
      {
        // compare data
        unsigned int currentSize = min(reversedFragmentBufferSize - reversedFragmentBufferPosition, rtpPacketBufferSize);
        if ((currentSize >= MINIMUM_RTP_PACKET_OVERLAP) && (memcmp(rtpPacketBuffer, start, currentSize) == 0))
        {
          // we must revert position to original position in fragment
          result.position = reversedFragmentBufferSize - reversedFragmentBufferPosition - currentSize;
          result.size = currentSize;
        }

        if (reversedFragmentBufferPosition > 0)
        {
          reversedFragmentBufferPosition--;
        }
      }
    }
    while (start != NULL);
  }

  FREE_MEM(fragmentBuffer);
  FREE_MEM(reversedFragmentBuffer);
  FREE_MEM(rtpPacketBuffer);
  FREE_MEM_CLASS(fragmentLinearBuffer);

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ReceiveData(CReceiveData *receiveData)
{
  HRESULT result = S_OK;

  CLockMutex lock(this->lockMutex, INFINITE);

  if (this->internalExitRequest)
  {
    // there is internal exit request pending == changed timestamp
    // close connection
    this->StopReceivingData();

    // reopen connection
    // StartReceivingData() reset wholeStreamDownloaded
    this->StartReceivingData(NULL);

    this->internalExitRequest = false;
  }

  if ((this->IsConnected()) && (!this->wholeStreamDownloaded))
  {
    {
      // adjust total length (if necessary)
      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        if (!track->IsSetStreamLength())
        {
          if (track->GetStreamLength() == 0)
          {
            // stream length not set
            // just make guess

            track->SetStreamLength(int64_t(MINIMUM_RECEIVED_DATA_FOR_SPLITTER));
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());

            if (i == 0)
            {
              receiveData->GetTotalLength()->SetTotalLength(track->GetStreamLength(), true);
            }
          }
          else if ((track->GetBytePosition() > (track->GetStreamLength() * 3 / 4)))
          {
            // it is time to adjust stream length, we are approaching to end but still we don't know total length
            track->SetStreamLength(track->GetBytePosition() * 2);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting quess total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());

            if (i == 0)
            {
              receiveData->GetTotalLength()->SetTotalLength(track->GetStreamLength(), true);
            }
          }
        }
      }

      if (SUCCEEDED(result) && (this->mainCurlInstance != NULL))
      {
        CRtpPacketCollection *rtpPackets = new CRtpPacketCollection();
        CHECK_POINTER_HRESULT(result, rtpPackets, result, E_OUTOFMEMORY);

        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
        {
          CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
          CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

          {
            CLockMutex lockData(this->lockCurlMutex, INFINITE);

            rtpPackets->Append(track->GetRtpPackets());
            track->GetRtpPackets()->Clear();
          }

          if (rtpPackets->Count() != 0)
          {
            // split or add received RTP packets to stream fragments
            CRtspStreamFragment *currentDownloadingFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());

            for (unsigned int j = 0; (SUCCEEDED(result) && (currentDownloadingFragment != NULL) && (j < rtpPackets->Count())); j++)
            {
              CRtpPacket *rtpPacket = rtpPackets->GetItem(j);

              // check before creating new fragment or adding payload data, if current RTP packet is not in any of downloaded stream fragments
              // we must check content of RTP packet within all received data - there is no other option
              // timestamps are not same, timestamp differencies are not same, RTP packets are not same

              // check first incoming RTP packet if it is in any of downloaded stream fragments
              // if yes, then just skip it, but remember in which fragment and on which position we found it (it made easier to check next RTP packets)
              // do same with next incoming RTP packet until we receive RTP packet which we don't have

              bool foundSameData = false;
              bool foundPartialData = false;
              unsigned int partialRtpPacketStartPosition = 0;
              unsigned int partialRtpPacketSize = rtpPacket->GetPayloadSize();

              for (unsigned int k = streamTrack->GetLastRtpPacketStreamFragmentIndex(); ((!foundSameData) && (!foundPartialData) && (k < streamTrack->GetStreamFragments()->Count())); k++)
              {
                CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(k);

                // we can check RTP packet only in downloaded stream fragments
                if (fragment->IsDownloaded())
                {
                  CompareDataResult compareResult = this->CompareData(streamTrack, k, (k == streamTrack->GetLastRtpPacketStreamFragmentIndex()) ? streamTrack->GetLastRtpPacketFragmentReceivedDataPosition() : 0, rtpPacket, 0, false);

                  if (compareResult.position != (-1))
                  {
                    if (fragment->GetLength() <= ((unsigned int)compareResult.position + compareResult.size))
                    {
                      // we have overlapped RTP packet with current and next fragment

                      unsigned int restRtpPacketSize = rtpPacket->GetPayloadSize() - compareResult.size;

                      // find in which fragment is end of current RTP packet
                      int fragmentIndex = k + 1;

                      streamTrack->SetLastRtpPacketStreamFragmentIndex(fragmentIndex);
                      streamTrack->SetLastRtpPacketFragmentReceivedDataPosition(0);

                      while (restRtpPacketSize > 0)
                      {
                        CRtspStreamFragment *nextFragment = streamTrack->GetStreamFragments()->GetItem(fragmentIndex);

                        if (nextFragment->IsDownloaded())
                        {
                          streamTrack->SetLastRtpPacketStreamFragmentIndex(fragmentIndex);
                          streamTrack->SetLastRtpPacketFragmentReceivedDataPosition(min(restRtpPacketSize, nextFragment->GetLength()));

                          restRtpPacketSize -= min(restRtpPacketSize, nextFragment->GetLength());
                        }
                        else
                        {
                          // we have overlapped RTP packet, but some data are new

                          partialRtpPacketStartPosition = rtpPacket->GetPayloadSize() - restRtpPacketSize;
                          partialRtpPacketSize = rtpPacket->GetPayloadSize() - partialRtpPacketStartPosition;

                          streamTrack->SetLastRtpPacketStreamFragmentIndex(fragmentIndex);
                          streamTrack->SetLastRtpPacketFragmentReceivedDataPosition(0);

                          foundPartialData = true;
                          break;
                        }

                        fragmentIndex++;
                      }
                    }
                    else
                    {
                      // remember stream fragment index and fragment received data position to avoid going through all stream fragments again
                      streamTrack->SetLastRtpPacketStreamFragmentIndex(k);
                      streamTrack->SetLastRtpPacketFragmentReceivedDataPosition((unsigned int)compareResult.position + compareResult.size);
                    }

                    foundSameData = !foundPartialData;
                  }
                }
                else
                {
                  // remember stream fragment index and fragment received data position
                  streamTrack->SetLastRtpPacketStreamFragmentIndex(k);
                  streamTrack->SetLastRtpPacketFragmentReceivedDataPosition(0);

                  if (k >= streamTrack->GetStreamFragmentDownloading())
                  {
                    // it's not necessary to continue in search, we found gap where current RTP packet will go
                    break;
                  }
                }
              }

              if ((!foundSameData) || (foundPartialData))
              {
                foundSameData = false;

                // if not found same data, we must store it
                // in another case, go to next RTP packet

                uint64_t timestamp = streamTrack->GetFragmentTimestamp(rtpPacket->GetTimestamp(), track->GetStatistics()->GetClockFrequency(), currentDownloadingFragment->GetFragmentStartTimestamp());

                // if timestamp is more than one second after last stream fragment, then create new stream fragment
                // in case of live stream accept stream fragment of any length (except zero) - we don't need to split stream into fragments, because we can't seek

                // first check if we don't have next stream fragment to current downloading fragment
                CRtspStreamFragment *nextFragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading() + 1);
                // it return NULL if such fragment doesn't exist

                if (nextFragment != NULL)
                {
                  // next fragment exists
                  // if next fragment is downloaded then check if RTP packet doesn't exist in next fragment (exactly on start of fragment)

                  if (nextFragment->IsDownloaded())
                  {
                    // we need to search for end of RTP packet, because RTP packet can overlap fragment, but only with end part, not with start part

                    CompareDataResult compareResult = this->CompareDataReversed(streamTrack, streamTrack->GetStreamFragmentDownloading() + 1, 0, rtpPacket, partialRtpPacketStartPosition, true);

                    if (compareResult.position == 0)
                    {
                      // we found at least some data in next packet
                      // we are receiving data, which we already have
                      // it depends on returned size, if we discard partial data or whole RTP packet

                      partialRtpPacketSize -= min(partialRtpPacketSize, compareResult.size);

                      streamTrack->SetLastRtpPacketStreamFragmentIndex(streamTrack->GetStreamFragmentDownloading() + 1);
                      streamTrack->SetLastRtpPacketFragmentReceivedDataPosition(compareResult.size);

                      foundSameData = (partialRtpPacketSize == 0);
                      foundPartialData = (partialRtpPacketSize != 0);
                    }

                    if (foundSameData)
                    {
                      // currently processing RTP packet is already downloaded
                      // we are finished with downloading current fragment

                      if (currentDownloadingFragment->GetReceivedData()->GetBufferOccupiedSpace() != 0)
                      {
                        currentDownloadingFragment->SetFragmentEndTimestamp(nextFragment->GetFragmentStartTimestamp() - 1);
                        currentDownloadingFragment->SetDownloaded(true);
                      }
                      else
                      {
                        // current fragment is not necessary to hold, because it is empty

                        if (streamTrack->GetStreamFragmentProcessing() >= streamTrack->GetStreamFragmentDownloading())
                        {
                          streamTrack->SetStreamFragmentProcessing(streamTrack->GetStreamFragmentProcessing() - 1);
                        }

                        streamTrack->GetStreamFragments()->Remove(streamTrack->GetStreamFragmentDownloading());
                        streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() - 1);
                      }

                      unsigned int fragmentToDownload = streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(streamTrack->GetStreamFragmentDownloading());
                      fragmentToDownload = (fragmentToDownload == UINT_MAX) ? streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) : fragmentToDownload;

                      streamTrack->SetStreamFragmentDownloading(UINT_MAX);
                      streamTrack->SetStreamFragmentToDownload(fragmentToDownload);

                      // we are also finished with processing of current RTSP track
                      break;
                    }
                  }
                }

                if ((!foundSameData) || (foundPartialData))
                {
                  if (currentDownloadingFragment->GetLength() != 0)
                  {
                    // we don't allow fragments with zero length
                    if (this->liveStream || ((timestamp - currentDownloadingFragment->GetFragmentStartTimestamp()) > 1000))
                    {
                      if ((nextFragment == NULL) ||
                        (nextFragment->IsDownloaded() && (timestamp < nextFragment->GetFragmentStartTimestamp())))
                      {
                        currentDownloadingFragment->SetFragmentEndTimestamp(timestamp - 1);

                        nextFragment = new CRtspStreamFragment(timestamp);
                        CHECK_POINTER_HRESULT(result, nextFragment, result, E_OUTOFMEMORY);

                        CHECK_CONDITION_HRESULT(result, streamTrack->GetStreamFragments()->Insert(streamTrack->GetStreamFragmentDownloading() + 1, nextFragment), result, E_OUTOFMEMORY);

                        if (SUCCEEDED(result))
                        {
                          currentDownloadingFragment->SetDownloaded(true);
                          streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
                        }
                        else
                        {
                          FREE_MEM_CLASS(nextFragment);
                        }

                        currentDownloadingFragment = nextFragment;
                      }
                      else if (!nextFragment->IsDownloaded())
                      {
                        // next fragment exists
                        // if next fragment is not downloaded, just use it
                        // if next fragment is downloaded then add RTP packet to current fragment

                        currentDownloadingFragment->SetFragmentEndTimestamp(timestamp - 1);

                        // next fragment is not downloaded, just use it
                        nextFragment->SetFragmentStartTimestamp(timestamp);
                        nextFragment->GetReceivedData()->ClearBuffer();

                        currentDownloadingFragment->SetDownloaded(true);
                        streamTrack->SetStreamFragmentDownloading(streamTrack->GetStreamFragmentDownloading() + 1);
                        currentDownloadingFragment = nextFragment;
                      }
                    }
                  }

                  // add RTP packet to current downloading stream fragment
                  CHECK_CONDITION_HRESULT(result, currentDownloadingFragment->GetReceivedData()->AddToBufferWithResize(rtpPacket->GetPayload() + partialRtpPacketStartPosition, partialRtpPacketSize, currentDownloadingFragment->GetReceivedData()->GetBufferSize() * 2) == partialRtpPacketSize, result, E_OUTOFMEMORY);

                  if (SUCCEEDED(result))
                  {
                    if (streamTrack->GetLastRtpPacketStreamFragmentIndex() <= streamTrack->GetStreamFragmentDownloading())
                    {
                      // remember stream fragment index and fragment received data position
                      streamTrack->SetLastRtpPacketStreamFragmentIndex(streamTrack->GetStreamFragmentDownloading());
                      streamTrack->SetLastRtpPacketFragmentReceivedDataPosition(currentDownloadingFragment->GetLength());
                    }
                    else
                    {
                      // we are finished with processing RTP packets, because last RTP packet stream fragment is bigger than currently downloading fragment
                      // it means that we found some overlapped data in RTP packet and fragment after currently downloading fragment

                      currentDownloadingFragment->SetFragmentEndTimestamp(nextFragment->GetFragmentStartTimestamp() - 1);
                      currentDownloadingFragment->SetDownloaded(true);

                      unsigned int fragmentToDownload = streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(streamTrack->GetStreamFragmentDownloading());
                      fragmentToDownload = (fragmentToDownload == UINT_MAX) ? streamTrack->GetStreamFragments()->GetFirstNotDownloadedStreamFragment(0) : fragmentToDownload;

                      streamTrack->SetStreamFragmentDownloading(UINT_MAX);
                      streamTrack->SetStreamFragmentToDownload(fragmentToDownload);

                      // we are also finished with processing of current RTSP track
                      break;
                    }
                  }
                }
              }
            }
          }

          {
            CLockMutex lockData(this->lockCurlMutex, INFINITE);

            if (track->IsEndOfStream() && (track->GetRtpPackets()->Count() == 0))
            {
              // mark currently downloading stream fragment as downloaded
              CRtspStreamFragment *fragment = streamTrack->GetStreamFragments()->GetItem(streamTrack->GetStreamFragmentDownloading());
              if (fragment != NULL)
              {
                fragment->SetDownloaded(true);
              }

              streamTrack->SetStreamFragmentDownloading(UINT_MAX);
            }
          }

          rtpPackets->Clear();
        }

        FREE_MEM_CLASS(rtpPackets);
      }

      // for now we support only one RTSP track to receive data
      // we don't mux data to MPEG2-TS format

      for (unsigned int i = 0; (SUCCEEDED(result) && (!this->supressData) && (i < this->streamTracks->Count())); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        if (track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count())
        {
          CLinearBuffer *bufferForProcessing = NULL;

          while ((bufferForProcessing = this->FillBufferForProcessing(track->GetStreamFragments(), track->GetStreamFragmentProcessing(), track->GetStoreFilePath())) != NULL)
          {
            unsigned int bufferOccupiedSpace = bufferForProcessing->GetBufferOccupiedSpace();

            if (bufferOccupiedSpace > 0)
            {
              // create media packet
              // set values of media packet

              CMediaPacket *mediaPacket = new CMediaPacket();
              CHECK_POINTER_HRESULT(result, mediaPacket, result, E_OUTOFMEMORY);
              CHECK_CONDITION_HRESULT(result, mediaPacket->GetBuffer()->AddToBufferWithResize(bufferForProcessing) == bufferOccupiedSpace, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                mediaPacket->SetStart(track->GetBytePosition());
                mediaPacket->SetEnd(track->GetBytePosition() + bufferOccupiedSpace - 1);

                if (i == 0)
                {
                  result = receiveData->GetMediaPacketCollection()->Add(mediaPacket) ? result : E_OUTOFMEMORY;
                }
              }

              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaPacket));

              if (i != 0)
              {
                FREE_MEM_CLASS(mediaPacket);
              }
            }

            if (SUCCEEDED(result))
            {
              track->SetBytePosition(track->GetBytePosition() + bufferOccupiedSpace);
              track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() + 1);

              // check if fragment is downloaded
              // if fragment is not downloaded, then schedule it for download
              if (track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count())
              {
                CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());

                if ((!fragment->IsDownloaded()) && (track->GetStreamFragmentProcessing() != track->GetStreamFragmentDownloading()))
                {
                  // fragment is not downloaded and also is not downloading currently
                  track->SetStreamFragmentDownloading(UINT_MAX);
                  track->SetStreamFragmentToDownload(track->GetStreamFragmentProcessing());
                }
              }
            }

            FREE_MEM_CLASS(bufferForProcessing);
          }
        }

        // we are not seeking now
        this->seekingActive = false;
      }
    }

    if (SUCCEEDED(result) && (this->streamTracks->Count() != 0))
    {
      bool allStreamFragmentsDownloaded = true;

      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        if ((track->GetStreamFragmentProcessing() < track->GetStreamFragments()->Count()) ||
          (track->GetStreamFragmentToDownload() != UINT_MAX) ||
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
        this->wholeStreamDownloaded = true;
        this->isConnected = false;

        this->mainCurlInstance->StopReceivingData();
      }

      for (unsigned int i = 0; ((!this->seekingActive) && (i < this->streamTracks->Count())); i++)
      {
        // we are not seeking, so we can set total length
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        if (track->GetStreamFragmentProcessing() >= track->GetStreamFragments()->Count())
        {
          // all stream fragments processed
          // set stream length and report end of stream

          if (!track->IsSetStreamLength())
          {
            track->SetStreamLength(track->GetBytePosition());
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i, track->GetStreamLength());
            track->SetStreamLengthFlag(true);

            if (i == 0)
            {
              receiveData->GetTotalLength()->SetTotalLength(track->GetStreamLength(), false);
            }
          }

          if (!track->IsSetEndOfStream())
          {
            // notify filter the we reached end of stream
            track->SetEndOfStreamFlag(true);

            if (i == 0)
            {
              receiveData->GetEndOfStreamReached()->SetStreamPosition(max(0, track->GetBytePosition() - 1));
            }
          }
        }
      }
    }

    if ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    {
      // all data received, we're not receiving data
      if (this->mainCurlInstance->GetDownloadResponse()->GetResultCode() == CURLE_OK)
      {
        bool receivedAllData = true;

        for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
        {
          CRtspTrack *track = this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->GetItem(i);
          CRtspStreamTrack *streamTrack = this->streamTracks->GetItem(i);

          if (track->GetRtpPackets()->Count() == 0)
          {
            // all data read from CURL instance for track i, received all data
            this->logger->Log(LOGGER_INFO, L"%s: %s: track %u, all data received", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, i);
          }
          else
          {
            receivedAllData = false;
          }
        }

        CHECK_CONDITION_EXECUTE(receivedAllData, this->mainCurlInstance->StopReceivingData());
      }
      else
      {
        // error occured while downloading
        // download stream fragment again or download scheduled stream fragment

        this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: %d, restarting download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetResultCode());

        if (this->liveStream)
        {
          // we can't restart download in live stream
          this->streamTracks->Clear();
        }
        else
        {
          for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
          {
            CRtspStreamTrack *track = this->streamTracks->GetItem(i);

            track->SetStreamFragmentToDownload(track->GetStreamFragmentDownloading());
            track->SetStreamFragmentDownloading(UINT_MAX);
          }
        }

        FREE_MEM_CLASS(this->mainCurlInstance);
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

    if (SUCCEEDED(result) && (this->isConnected) && ((this->mainCurlInstance == NULL) || ((startFragmentDownload) && (!anyTrackDownloading))))
    {
      uint64_t startTime = UINT64_MAX;

      // if seeking to empty place (no downloaded fragment before) then use fragment time
      // if seeking to fill gap (one or more downloaded fragment(s) before) then use previous fragment time, in this case MUST fragments and RTP packets overlap
      // if fragments and RTP packets are not overlapped then decrease seek time

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);
        unsigned int fragmentToDownload = track->GetStreamFragmentToDownload();

        if (fragmentToDownload != UINT_MAX)
        {
          CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(fragmentToDownload);

          if (fragmentToDownload > 0)
          {
            CRtspStreamFragment *previousFragment = track->GetStreamFragments()->GetItem(fragmentToDownload - 1);

            if (previousFragment->IsDownloaded())
            {
              startTime = min(previousFragment->GetFragmentStartTimestamp(), startTime);
            }
          }
          
          startTime = min(fragment->GetFragmentStartTimestamp(), startTime);
        }
      }

      startTime = (startTime != UINT64_MAX) ? startTime: 0;

      FREE_MEM_CLASS(this->mainCurlInstance);
      FREE_MEM_CLASS(this->sessionDescription);

      // new connection will be created with new RTP packet timestamps
      // clear set RTP packet timestamp flags
      for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
      {
        CRtspStreamTrack *track = this->streamTracks->GetItem(i);

        track->SetFirstRtpPacketTimestampFlag(false);

        // searching for RTP packets will be started from first stream fragment
        track->SetLastRtpPacketStreamFragmentIndex(0);
        track->SetLastRtpPacketFragmentReceivedDataPosition(0);

        // clear all not downloaded stream fragments
        for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
        {
          CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);

          if (!fragment->IsDownloaded())
          {
            fragment->GetReceivedData()->ClearBuffer();
          }
        }
      }

      this->mainCurlInstance = new CRtspCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // set connection priorities
        this->mainCurlInstance->SetMulticastPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_MULTICAST_PREFERENCE, true, RTSP_MULTICAST_PREFERENCE_DEFAULT));
        this->mainCurlInstance->SetSameConnectionTcpPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_SAME_CONNECTION_TCP_PREFERENCE, true, RTSP_SAME_CONNECTION_TCP_PREFERENCE_DEFAULT));
        this->mainCurlInstance->SetUdpPreference(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_UDP_PREFERENCE, true, RTSP_UDP_PREFERENCE_DEFAULT));

        // set ports
        this->mainCurlInstance->SetRtspClientPortMin(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MIN, true, RTSP_CLIENT_PORT_MIN_DEFAULT));
        this->mainCurlInstance->SetRtspClientPortMax(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTSP_CLIENT_PORT_MAX, true, RTSP_CLIENT_PORT_MAX_DEFAULT));

        this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
        this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
        this->mainCurlInstance->SetIgnoreRtpPayloadType(this->configurationParameters->GetValueBool(PARAMETER_NAME_RTSP_IGNORE_RTP_PAYLOAD_TYPE, true, RTSP_IGNORE_RTP_PAYLOAD_TYPE_DEFAULT));
      }

      if (SUCCEEDED(result))
      {
        CRtspDownloadRequest *request = new CRtspDownloadRequest();
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          request->SetStartTime(startTime);
          request->SetUrl(this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL));

          result = (this->mainCurlInstance->Initialize(request)) ? S_OK : E_FAIL;
        }
        FREE_MEM_CLASS(request);

        if (SUCCEEDED(result))
        {
          if (this->sessionDescription == NULL)
          {
            const wchar_t *rawSDP = this->mainCurlInstance->GetRtspDownloadResponse()->GetRawSessionDescription();
            unsigned int rawSDPlength = wcslen(rawSDP);

            this->sessionDescription = new CSessionDescription();
            CHECK_POINTER_HRESULT(result, this->sessionDescription, result, E_OUTOFMEMORY);
            CHECK_CONDITION_HRESULT(result, this->sessionDescription->Parse(rawSDP, rawSDPlength), result, E_FAIL);
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
            this->liveStream = true;

            if (nptAttribute != NULL)
            {
              if (nptAttribute->IsSetStartTime() && nptAttribute->IsSetEndTime() && (nptAttribute->GetEndTime() >= nptAttribute->GetStartTime()))
              {
                // not live stream
                this->liveStream = false;
              }
            }

            // if in configuration is specified live stream flag, then use it and ignore normal play time attribute
            this->liveStream = this->configurationParameters->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, this->liveStream);
          }

          // all parameters set
          // start receiving data

          CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->StartReceivingData(), result, E_FAIL);
        }
      }

      if (SUCCEEDED(result))
      {
        // create same count of RTSP stream tracks as RTSP tracks in curl instance
        CLockMutex(this->lockCurlMutex, INFINITE);

        if (this->streamTracks->Count() != this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())
        {
          this->streamTracks->Clear();

          for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->mainCurlInstance->GetRtspDownloadResponse()->GetRtspTracks()->Count())); i++)
          {
            CRtspStreamTrack *track = new CRtspStreamTrack();
            CHECK_POINTER_HRESULT(result, track, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, this->streamTracks->Add(track), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(track));
          }

          // for each stream track add first stream fragment
          for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->streamTracks->Count())); i++)
          {
            CRtspStreamTrack *track = this->streamTracks->GetItem(i);
            CRtspStreamFragment *fragment = new CRtspStreamFragment(0);
            CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, track->GetStreamFragments()->Add(fragment), result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              fragment->SetFragmentStartTimestamp(this->reportedStreamTime);
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

          for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
          {
            CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);

            track->SetStreamFragmentDownloading(track->GetStreamFragmentToDownload());
          }

          // we are downloading stream fragment, so reset scheduled download
          track->SetStreamFragmentToDownload(UINT_MAX);
        }
      }

      if (FAILED(result))
      {
        // clear connected state to call StartReceiveData() method in parser hoster, after first fail we need to reopen connection until timeout (GetReceiveDataTimeout()) comes
        // free RTSP curl instance
        // in next run we try to open connection
        this->isConnected = false;
        FREE_MEM_CLASS(this->mainCurlInstance);
        result = S_OK;
      }
    }

    // store stream fragments to temporary file
    if (this->wholeStreamDownloaded || ((GetTickCount() - this->lastStoreTime) > 1000))
    {
      this->lastStoreTime = GetTickCount();

      if ((!this->liveStream) && (this->streamTracks->Count() > 0))
      {
        for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
        {
          CRtspStreamTrack *track = this->streamTracks->GetItem(i);

          if (track->GetStoreFilePath() == NULL)
          {
            wchar_t *storeFilePath = this->GetStoreFile(i);
            CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, track->SetStoreFilePath(storeFilePath));
            FREE_MEM(storeFilePath);
          }

          // store all stream fragments (which are not stored) to file
          if (track->GetStoreFilePath() != NULL)
          {
            LARGE_INTEGER size;
            size.QuadPart = 0;

            // open or create file
            HANDLE hTempFile = CreateFile(track->GetStoreFilePath(), FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

            if (hTempFile != INVALID_HANDLE_VALUE)
            {
              if (!GetFileSizeEx(hTempFile, &size))
              {
                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"error while getting size");
                // error occured while getting file size
                size.QuadPart = -1;
              }

              if (size.QuadPart >= 0)
              {
                for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
                {
                  CRtspStreamFragment *streamFragment = track->GetStreamFragments()->GetItem(j);

                  if ((!streamFragment->IsStoredToFile()) && (streamFragment->IsDownloaded()))
                  {
                    // if stream fragment is not stored to file
                    // store it to file
                    unsigned int length = streamFragment->GetLength();

                    ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
                    if (streamFragment->GetReceivedData()->CopyFromBuffer(buffer, length) == length)
                    {
                      DWORD written = 0;
                      if (WriteFile(hTempFile, buffer, length, &written, NULL))
                      {
                        if (length == written)
                        {
                          // mark as stored
                          streamFragment->SetStoredToFile(size.QuadPart);
                          size.QuadPart += length;
                        }
                      }
                    }
                    FREE_MEM(buffer);
                  }
                }
              }

              CloseHandle(hTempFile);
              hTempFile = INVALID_HANDLE_VALUE;
            }
          }
        }
      }

      // in case of live stream remove all downloaded and processed stream fragments before reported stream time
      if (this->liveStream)
      {
        for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
        {
          CRtspStreamTrack *track = this->streamTracks->GetItem(i);

          while (track->GetStreamFragmentProcessing() != 0)
          {
            CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(0);

            if (fragment->IsDownloaded() && (fragment->GetFragmentStartTimestamp() < this->reportedStreamTime))
            {
              // remove fragment
              track->GetStreamFragments()->Remove(0);

              track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() - 1);

              if (track->GetStreamFragmentDownloading() != UINT_MAX)
              {
                track->SetStreamFragmentDownloading(track->GetStreamFragmentDownloading() - 1);
              }

              if (track->GetStreamFragmentToDownload() != UINT_MAX)
              {
                track->SetStreamFragmentToDownload(track->GetStreamFragmentToDownload() - 1);
              }

              if (track->GetLastRtpPacketStreamFragmentIndex() > 0)
              {
                track->SetLastRtpPacketStreamFragmentIndex(track->GetLastRtpPacketStreamFragmentIndex() - 1);
              }
              else
              {
                track->SetLastRtpPacketStreamFragmentIndex(0);
                track->SetLastRtpPacketFragmentReceivedDataPosition(0);
              }
            }
            else
            {
              break;
            }
          }
        }
      }
    }
  }

  return result;
}

CParameterCollection *CMPUrlSourceSplitter_Protocol_Rtsp::GetConnectionParameters(void)
{
  CParameterCollection *result = new CParameterCollection();
  bool retval = (result != NULL);

  if (retval)
  {
    // add configuration parameters
    retval &= result->Append(this->configurationParameters);
  }

  if (!retval)
  {
    FREE_MEM_CLASS(result);
  }
  
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

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configurationParameters);

  if (SUCCEEDED(result) && (parameters != NULL))
  {
    this->configurationParameters->Append(parameters);
  }

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  this->wholeStreamDownloaded = false;

  if (FAILED(result))
  {
    this->StopReceivingData();
  }

  this->isConnected = SUCCEEDED(result);

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);

  this->isConnected = false;
  for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
  {
    CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    track->SetStreamFragmentDownloading(UINT_MAX);
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::QueryStreamProgress(LONGLONG *total, LONGLONG *current)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, total);
  CHECK_POINTER_DEFAULT_HRESULT(result, current);

  if (SUCCEEDED(result))
  {
    CRtspStreamTrack *track = (this->streamTracks->Count() > 0) ? (this->streamTracks->GetItem(0)) : NULL;

    if (track != NULL)
    {
      *total = (track->GetStreamLength() == 0) ? 1 : track->GetStreamLength();
      *current = (track->GetStreamLength() == 0) ? 0 : track->GetBytePosition();

      if (!track->IsSetStreamLength())
      {
        result = VFW_S_ESTIMATED;
      }
    }
    else
    {
      *total = 1;
      *current = 0;

      result = VFW_S_ESTIMATED;
    }
  }

  return result;
}
  
HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);

  if (SUCCEEDED(result))
  {
    availableLength->SetQueryResult(S_OK);

    CRtspStreamTrack *track = (this->streamTracks->Count() > 0) ? (this->streamTracks->GetItem(0)) : NULL;

    if (track != NULL)
    {
      if (!track->IsSetStreamLength())
      {
        availableLength->SetAvailableLength(track->GetBytePosition());
      }
      else
      {
        availableLength->SetAvailableLength(track->GetStreamLength());
      }
    }
    else
    {
      availableLength->SetAvailableLength(0);
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  this->internalExitRequest = false;
  this->streamTime = 0;
  this->wholeStreamDownloaded = false;
  this->receiveDataTimeout = RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->isConnected = false;
  this->streamTracks->Clear();
  this->liveStream = false;
  this->seekingActive = false;
  FREE_MEM_CLASS(this->sessionDescription);
  this->reportedStreamTime = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtsp::GetDuration(void)
{
  int64_t result = DURATION_LIVE_STREAM;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    if ((!this->liveStream) && (this->sessionDescription != NULL))
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

void CMPUrlSourceSplitter_Protocol_Rtsp::ReportStreamTime(uint64_t streamTime)
{
  this->reportedStreamTime = streamTime;
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Rtsp::GetSeekingCapabilities(void)
{
  unsigned int result = SEEKING_METHOD_NONE;
  {
    // lock access to stream
    CLockMutex lock(this->lockMutex, INFINITE);

    result = SEEKING_METHOD_TIME;
  }
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtsp::SeekToTime(int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  // find fragment to process for each track
  for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
  {
    CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    for (unsigned int j = 0; j < track->GetStreamFragments()->Count(); j++)
    {
      CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(j);

      if ((fragment->GetFragmentStartTimestamp() <= (uint64_t)time) &&
        (fragment->GetFragmentEndTimestamp() >= (uint64_t)time))
      {
        track->SetStreamFragmentProcessing(j);
        result = (result == -1) ? fragment->GetFragmentStartTimestamp() : (min((uint64_t)result, fragment->GetFragmentStartTimestamp()));

        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: track %u, timestamp %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, i, fragment->GetFragmentStartTimestamp());
        break;
      }
    }

    track->SetBytePosition(0);
    track->SetStreamLength(0);
    track->SetStreamLengthFlag(false);
    track->SetEndOfStreamFlag(false);
  }

  this->seekingActive = true;
  // reset whole stream downloaded, but IsConnected() must return true to avoid calling StartReceivingData()
  this->isConnected = true;
  this->wholeStreamDownloaded = false;

  // in all tracks is set stream fragment to process
  // in all tracks exists at least one stream fragment and cover all possible timestamps (0 - UINT64_MAX)

  bool downloadedFragment = true;
  for (unsigned int i = 0; i < this->streamTracks->Count(); i++)
  {
    CRtspStreamTrack *track = this->streamTracks->GetItem(i);

    CRtspStreamFragment *processingFragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());

    if (!processingFragment->IsDownloaded())
    {
      // stream fragment is not downloaded, it is gap
      // split stream fragment

      downloadedFragment = false;
      result = time;

      // create new fragment within found fragment
      CRtspStreamFragment *fragment = new CRtspStreamFragment(time);
      result = (fragment != NULL) ? result : (-1);

      if (result != (-1))
      {
        track->SetStreamFragmentProcessing(track->GetStreamFragmentProcessing() + 1);
        fragment->SetFragmentStartTimestamp(time);
        fragment->SetFragmentEndTimestamp(processingFragment->GetFragmentEndTimestamp());

        result = track->GetStreamFragments()->Insert(track->GetStreamFragmentProcessing(), fragment) ? result : (-1);
      }

      if (result != (-1))
      {
        // correct previous fragment end timestamp
        if (track->GetStreamFragmentProcessing() > 0)
        {
          CRtspStreamFragment *fragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing());
          CRtspStreamFragment *previousFragment = track->GetStreamFragments()->GetItem(track->GetStreamFragmentProcessing() - 1);

          previousFragment->SetFragmentEndTimestamp(max(fragment->GetFragmentStartTimestamp(), 1) - 1);
        }

        // force to download missing fragment
        track->SetStreamFragmentToDownload(track->GetStreamFragmentProcessing());
      }
    }
  }

  if (!downloadedFragment)
  {
    // close connection
    this->StopReceivingData();

    // reopen connection
    // StartReceivingData() reset wholeStreamDownloaded
    this->isConnected = SUCCEEDED(this->StartReceivingData(NULL));
  }

  if (!this->IsConnected())
  {
    result = -1;
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtsp::SeekToPosition(int64_t start, int64_t end)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from position: %llu, to position: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, start, end);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, result);
  return result;
}

void CMPUrlSourceSplitter_Protocol_Rtsp::SetSupressData(bool supressData)
{
  this->supressData = supressData;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Protocol_Rtsp::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Rtsp::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtsp::Initialize(PluginConfiguration *configuration)
{
  if (configuration == NULL)
  {
    return E_POINTER;
  }

  ProtocolPluginConfiguration *protocolConfiguration = (ProtocolPluginConfiguration *)configuration;
  this->logger->SetParameters(protocolConfiguration->configuration);

  if (this->lockMutex == NULL)
  {
    return E_FAIL;
  }

  this->configurationParameters->Clear();
  if (protocolConfiguration->configuration != NULL)
  {
    this->configurationParameters->Append(protocolConfiguration->configuration);
  }
  this->configurationParameters->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_RTSP_RECEIVE_DATA_TIMEOUT, true, RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT);
  this->liveStream = this->configurationParameters->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTSP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

  return S_OK;
}

// other methods

wchar_t *CMPUrlSourceSplitter_Protocol_Rtsp::GetStoreFile(unsigned int trackId)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configurationParameters->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_rtsp_%s_track_%02u.temp", folder, guid, trackId);
    }
    FREE_MEM(guid);
  }

  return result;
}

CLinearBuffer *CMPUrlSourceSplitter_Protocol_Rtsp::FillBufferForProcessing(CRtspStreamFragmentCollection *fragments, unsigned int streamFragmentProcessing, const wchar_t *storeFile)
{
  CLinearBuffer *result = NULL;

  if (fragments != NULL)
  {
    if (streamFragmentProcessing < fragments->Count())
    {
      CRtspStreamFragment *fragment = fragments->GetItem(streamFragmentProcessing);

      if (fragment->IsDownloaded())
      {
        // fragment is downloaded
        // fragment can be stored in memory or in store file

        // temporary buffer for data (from store file or from memory)
        unsigned int bufferLength = fragment->GetLength();
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferLength, 0);

        if (buffer != NULL)
        {
          if ((fragment->IsStoredToFile()) && (storeFile != NULL))
          {
            // segment and fragment is stored into file and store file is specified

            LARGE_INTEGER size;
            size.QuadPart = 0;

            // open or create file
            HANDLE hTempFile = CreateFile(storeFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

            if (hTempFile != INVALID_HANDLE_VALUE)
            {
              bool error = false;

              LONG distanceToMoveLow = (LONG)(fragment->GetStoreFilePosition());
              LONG distanceToMoveHigh = (LONG)(fragment->GetStoreFilePosition() >> 32);
              LONG distanceToMoveHighResult = distanceToMoveHigh;
              DWORD setFileResult = SetFilePointer(hTempFile, distanceToMoveLow, &distanceToMoveHighResult, FILE_BEGIN);
              if (setFileResult == INVALID_SET_FILE_POINTER)
              {
                DWORD lastError = GetLastError();
                if (lastError != NO_ERROR)
                {
                  this->logger->Log(LOGGER_ERROR, L"%s: %s: error occured while setting position: %lu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_FOR_PROCESSING_NAME, lastError);
                  error = true;
                }
              }

              if (!error)
              {
                DWORD read = 0;
                if (ReadFile(hTempFile, buffer, bufferLength, &read, NULL) == 0)
                {
                  this->logger->Log(LOGGER_ERROR, L"%s: %s: error occured reading file: %lu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_FOR_PROCESSING_NAME, GetLastError());
                  FREE_MEM(buffer);
                }
                else if (read != bufferLength)
                {
                  this->logger->Log(LOGGER_WARNING, L"%s: %s: readed data length not same as requested, requested: %u, readed: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_FOR_PROCESSING_NAME, bufferLength, read);
                  FREE_MEM(buffer);
                }
              }

              CloseHandle(hTempFile);
              hTempFile = INVALID_HANDLE_VALUE;
            }
          }
          else if (!fragment->IsStoredToFile())
          {
            // fragment is stored in memory
            if (fragment->GetReceivedData()->CopyFromBuffer(buffer, bufferLength) != bufferLength)
            {
              // error occured while copying data
              FREE_MEM(buffer);
            }
          }
        }

        if (buffer != NULL)
        {
          // all data are read
          bool correct = false;
          result = new CLinearBuffer();
          if (result != NULL)
          {
            if (result->InitializeBuffer(bufferLength))
            {
              if (result->AddToBuffer(buffer, bufferLength) == bufferLength)
              {
                // everything correct, data copied successfully
                correct = true;
              }
            }
          }

          if (!correct)
          {
            FREE_MEM_CLASS(result);
          }
        }

        // clean-up buffer
        FREE_MEM(buffer);
      }
    }
  }

  return result;
}