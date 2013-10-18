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
#include "RtmpCurlInstance.h"
#include "ErrorCodes.h"

#include <WinInet.h>
#include <stdio.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Rtmpd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Rtmp"
#endif

#define METHOD_FILL_BUFFER_FOR_PROCESSING_NAME                                L"FillBufferForProcessing()"

void CorrectPreviousFragmentEndTimestamp(CRtmpStreamFragmentCollection *streamFragments, unsigned int currentFragment)
{
  // correct previous fragment end timestamp
  if (currentFragment > 0)
  {
    CRtmpStreamFragment *fragment = streamFragments->GetItem(currentFragment);
    CRtmpStreamFragment *previousFragment = streamFragments->GetItem(currentFragment - 1);

    previousFragment->SetFragmentEndTimestamp(max(fragment->GetFragmentStartTimestamp(), 1) - 1);
  }
}

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Rtmp(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Rtmp *pClass = (CMPUrlSourceSplitter_Protocol_Rtmp *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Protocol_Rtmp::CMPUrlSourceSplitter_Protocol_Rtmp(CLogger *logger, CParameterCollection *configuration)
{
  this->configurationParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

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
  
  this->receiveDataTimeout = RTMP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->streamLength = 0;
  this->setLength = false;
  this->setEndOfStream = false;
  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
  this->wholeStreamDownloaded = false;
  this->mainCurlInstance = NULL;
  this->streamDuration = 0;
  this->bytePosition = 0;
  this->seekingActive = false;
  this->supressData = false;
  this->storeFilePath = NULL;
  this->lastStoreTime = 0;
  this->isConnected = false;
  this->rtmpStreamFragments = NULL;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  this->ignoreKeyFrameTimestamp = 0;
  this->additionalCorrection = 0;
  this->duration = RTMP_DURATION_UNSPECIFIED;
  this->liveStream = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Protocol_Rtmp::~CMPUrlSourceSplitter_Protocol_Rtmp()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  if (this->storeFilePath != NULL)
  {
    DeleteFile(this->storeFilePath);
  }

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->configurationParameters);
  FREE_MEM(this->storeFilePath);
  FREE_MEM_CLASS(this->rtmpStreamFragments);

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

bool CMPUrlSourceSplitter_Protocol_Rtmp::IsConnected(void)
{
  return ((this->isConnected) || (this->mainCurlInstance != NULL) || (this->wholeStreamDownloaded));
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::ParseUrl(const CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  this->ClearSession();
  if (parameters != NULL)
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

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::ReceiveData(CReceiveData *receiveData)
{
  HRESULT result = S_OK;

  CLockMutex lock(this->lockMutex, INFINITE);

  if (this->IsConnected())
  {
    if (!this->wholeStreamDownloaded)
    {
      if (SUCCEEDED(result) && (this->mainCurlInstance != NULL))
      {
        if (this->mainCurlInstance->GetRtmpDownloadResponse()->GetResultCode() == CURLE_OK)
        {
          // everything correct, we can parse data
          CRtmpStreamFragment *fragment = this->rtmpStreamFragments->GetItem(this->streamFragmentDownloading);
          CFlvPacket *flvPacket = new CFlvPacket();

          int parsedPacket = FLV_PARSE_RESULT_OK;
          while (SUCCEEDED(result) && (this->streamFragmentDownloading != UINT_MAX) && (parsedPacket == FLV_PARSE_RESULT_OK))
          {
            if ((this->firstTimestamp == (-1)) || (this->firstVideoTimestamp == (-1)))
            {
              // we don't have video correction
              // we must find META, VIDEO and AUDIO packet and compare their timestamps

              unsigned int bufferSize = 0;
              unsigned int processed  = 0;
              uint8_t *buffer = NULL;

              int firstTimestamp = -1;
              int videoTimestamp = -1;
              int audioTimestamp = -1;

              {
                CLockMutex lockData(this->lockCurlMutex, INFINITE);

                bufferSize = this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
                buffer = ALLOC_MEM_SET(buffer, uint8_t, bufferSize, 0);
                CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

                if (SUCCEEDED(result))
                {
                  this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData()->CopyFromBuffer(buffer, bufferSize);
                }
              }

              while (SUCCEEDED(result) && (parsedPacket == FLV_PARSE_RESULT_OK))
              {
                parsedPacket = flvPacket->ParsePacket(buffer + processed, bufferSize - processed);

                if (((bufferSize - processed) != 0) && (parsedPacket == FLV_PARSE_RESULT_CHECK_SIZE_INCORRECT))
                {
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
                      this->logger->Log(LOGGER_ERROR, L"%s: %s: malformed FLV packet detected, not enough memory, buffer size: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace());
                      break;
                    case FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS:
                      // wait for more data
                      break;
                    default:
                      this->logger->Log(LOGGER_ERROR, L"%s: %s: malformed FLV packet detected, unknown find result: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, findResult);
                      result = E_FAIL;
                      break;
                    }
                  }
                }

                if (SUCCEEDED(result) && (parsedPacket == FLV_PARSE_RESULT_OK))
                {
                  // FLV packet parsed correctly

                  if ((flvPacket->GetType() != FLV_PACKET_HEADER) && (firstTimestamp == (-1)))
                  {
                    firstTimestamp = flvPacket->GetTimestamp();
                  }

                  if ((flvPacket->GetType() == FLV_PACKET_VIDEO) && (videoTimestamp == (-1)))
                  {
                    videoTimestamp = flvPacket->GetTimestamp();
                  }

                  if ((flvPacket->GetType() == FLV_PACKET_AUDIO) && (audioTimestamp == (-1)))
                  {
                    audioTimestamp = flvPacket->GetTimestamp();
                  }
                }

                if ((firstTimestamp != (-1)) && (videoTimestamp != (-1)) && (audioTimestamp != (-1)))
                {
                  // we have first FLV packet, video and audio timestamps
                  // stop searching and calculate video correction
                  break;
                }

                processed += flvPacket->GetSize();
                flvPacket->Clear();
              }

              if ((firstTimestamp != (-1)) && (videoTimestamp != (-1)) && (audioTimestamp != (-1)))
              {
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: first timestamp: %d, video timestamp: %d, audio timestamp: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, firstTimestamp, videoTimestamp, audioTimestamp);

                int diffFirstVideo = abs(firstTimestamp - videoTimestamp);
                int diffFirstAudio = abs(firstTimestamp - audioTimestamp);
                int diffVideoAudio = abs(videoTimestamp - audioTimestamp);

                // choose smallest difference
                int index = 0;                  // 0 = first - video, 1 = first - audio, 2 = video - audio
                int value = diffFirstVideo;

                if (diffFirstAudio < value)
                {
                  index = 1;
                  value = diffFirstAudio;
                }

                if (diffVideoAudio < value)
                {
                  index = 2;
                  value = diffVideoAudio;
                }

                // remember correct timestamp values
                if ((index == 0) || (index == 1))
                {
                  this->firstTimestamp = firstTimestamp;
                  this->firstVideoTimestamp = videoTimestamp;
                }
                else if (index == 2)
                {
                  this->firstTimestamp = videoTimestamp;
                  this->firstVideoTimestamp = videoTimestamp;
                }

                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: set first timestamp: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->firstTimestamp);
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: set first video timestamp: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->firstVideoTimestamp);
              }

              FREE_MEM(buffer);
            }

            if (SUCCEEDED(result) && (this->firstTimestamp != (-1)) && (this->firstVideoTimestamp != (-1)))
            {
              CLockMutex lockData(this->lockCurlMutex, INFINITE);

              parsedPacket = flvPacket->ParsePacket(this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData());

              if ((this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() != 0) && (parsedPacket == FLV_PARSE_RESULT_CHECK_SIZE_INCORRECT))
              {
                // we have received data, it seems that we have FLV packet, but with incorrect check size = malformed FLV packet

                int findResult = flvPacket->FindPacket(this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData(), FLV_PACKET_MINIMUM_CHECKED_UNSPECIFIED);

                if (findResult >= 0)
                {
                  // found sequence of correct FLV packets
                  // just remove malformed data from buffer and continue
                  this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove((unsigned int)findResult);
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
                    this->logger->Log(LOGGER_ERROR, L"%s: %s: malformed FLV packet detected, not enough memory, buffer size: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace());
                    break;
                  case FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS:
                    // wait for more data
                    break;
                  default:
                    this->logger->Log(LOGGER_ERROR, L"%s: %s: malformed FLV packet detected, unknown find result: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, findResult);
                    result = E_FAIL;
                    break;
                  }
                }
              }
            }

            if (SUCCEEDED(result) && (this->firstTimestamp != (-1)) && (this->firstVideoTimestamp != (-1)) && (parsedPacket == FLV_PARSE_RESULT_OK))
            {
              // FLV packet parsed correctly

              if (SUCCEEDED(result) && (fragment->IsSeeked()) && (fragment->IsStartTimestampSet()) && (fragment->GetBuffer()->GetBufferOccupiedSpace() == 0))
              {
                // seeked to fragment which was downloading, but interrupted
                // actually we are in fragment before
                // ignore everything to second key frame (first key frame is key frame for previous fragment)

                // activate ignoring packets
                if ((this->ignoreKeyFrameTimestamp == 0) && (flvPacket->IsKeyFrame()))
                {
                  this->ignoreKeyFrameTimestamp = flvPacket->GetTimestamp();
                }

                if ((this->ignoreKeyFrameTimestamp != 0) && (flvPacket->IsKeyFrame()) && (flvPacket->GetTimestamp() > this->ignoreKeyFrameTimestamp))
                {
                  // second key frame, accept
                  this->ignoreKeyFrameTimestamp = 0;

                  this->additionalCorrection = (unsigned int)fragment->GetFragmentStartTimestamp() + this->firstVideoTimestamp - flvPacket->GetTimestamp() - this->firstTimestamp;
                  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: set additional correction: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->additionalCorrection);

                  // we have correct first timestamp, stop ignoring data, set seeked to false
                  fragment->SetSeeked(false);

                  // we are on key fragment, we can set that fragment has not valid start timestamp
                  // it will be set few lines after
                  fragment->SetFragmentStartTimestamp(fragment->GetFragmentStartTimestamp(), false);
                }
              }

              if (SUCCEEDED(result) && (flvPacket->GetType() == FLV_PACKET_VIDEO) && (this->firstVideoTimestamp != (-1)) && (this->firstTimestamp != (-1)))
              {
                // correction of video timestamps
                flvPacket->SetTimestamp(flvPacket->GetTimestamp() + this->firstTimestamp - this->firstVideoTimestamp + this->additionalCorrection);
              }
              else if (SUCCEEDED(result) && (flvPacket->GetType() != FLV_PACKET_VIDEO) && (flvPacket->GetType() != FLV_PACKET_HEADER))
              {
                // correction of timestamps for another type of packets (META, AUDIO)
                flvPacket->SetTimestamp(flvPacket->GetTimestamp() + this->additionalCorrection);
              }

              if (SUCCEEDED(result) && (this->ignoreKeyFrameTimestamp == 0) &&
                (((flvPacket->GetType() == FLV_PACKET_META) && (!fragment->IsSeeked())) || (flvPacket->GetType() != FLV_PACKET_META)))
              {
                // no ignoring key frames and other data

                // check if packet is not key frame
                if (flvPacket->IsKeyFrame() && 
                  ((flvPacket->GetTimestamp() != fragment->GetFragmentStartTimestamp()) || (!fragment->IsStartTimestampSet())) &&
                  (this->firstTimestamp != (-1)) && 
                  (this->firstVideoTimestamp != (-1)))
                {
                  bool createNewFragment = false;
                  uint64_t fragmentEnd = 0;

                  if (fragment->IsStartTimestampSet())
                  {
                    fragmentEnd = fragment->GetFragmentEndTimestamp();

                    // mark current fragment as downloaded
                    fragment->SetFragmentEndTimestamp(max(1, flvPacket->GetTimestamp()) - 1);
                    fragment->SetDownloaded(true);

                    unsigned int downloadedFragment = this->streamFragmentDownloading++;
                    unsigned int position = this->rtmpStreamFragments->GetFragmentWithTimestamp(flvPacket->GetTimestamp(), this->streamFragmentDownloading);

                    if (position == UINT_MAX)
                    {
                      // there is no fragmet with FLV packet timestamp
                      // create new fragment
                      createNewFragment = true;
                    }
                    else
                    {
                      CRtmpStreamFragment *positionFragment = this->rtmpStreamFragments->GetItem(position);
                      if (positionFragment->IsSeeked())
                      {
                        // found fragment has start timestamp lower or equal to FLV packet timestamp
                        // found fragment has end timestamp higher or equal to FLV packet timestamp

                        // fragment is seeked = not completly downloaded and with incorrect timestamps
                        this->streamFragmentDownloading = position;

                        positionFragment->SetSeeked(false);
                        positionFragment->SetDownloaded(false);
                        positionFragment->SetStoredToFile(-1);
                        positionFragment->GetBuffer()->ClearBuffer();
                        positionFragment->SetFragmentStartTimestamp(flvPacket->GetTimestamp(), true);
                        positionFragment->SetIncorrectTimestamps(fragment->HasIncorrectTimestamps());

                        CorrectPreviousFragmentEndTimestamp(this->rtmpStreamFragments, this->streamFragmentDownloading);

                        fragment = positionFragment;
                      }
                      else
                      {
                        // found fragment has start timestamp lower or equal to FLV packet timestamp
                        // found fragment has end timestamp higher or equal to FLV packet timestamp

                        // between fragment and found fragment have to be seeked fragments
                        // we are downloading data which were already downloaded, but with incorrect timestamps

                        // remove all fragments between fragment and found fragment, rest of fragments until seeked or
                        // not downloaded fragment correct timestamps (also in FLV packets)

                        for (unsigned int i = 1; i < (position - downloadedFragment); i++)
                        {
                          this->rtmpStreamFragments->Remove(downloadedFragment + i);
                        }

                        // correct rest of fragments
                        int correction = flvPacket->GetTimestamp() - (unsigned int)this->rtmpStreamFragments->GetItem(downloadedFragment + 1)->GetFragmentStartTimestamp();

                        for (unsigned int i = (downloadedFragment + 1); i < this->rtmpStreamFragments->Count(); i++)
                        {
                          CRtmpStreamFragment *fragmentToCorrection = this->rtmpStreamFragments->GetItem(i);

                          if (!fragmentToCorrection->IsDownloaded())
                          {
                            // fragment is not downloaded
                            // fragment possibly has incorrect timestamp

                            fragmentToCorrection->SetFragmentStartTimestamp(fragmentToCorrection->GetFragmentStartTimestamp() + correction, true);
                            fragmentToCorrection->SetIncorrectTimestamps(fragment->HasIncorrectTimestamps());

                            CorrectPreviousFragmentEndTimestamp(this->rtmpStreamFragments, i);
                            break;
                          }

                          fragmentToCorrection->SetFragmentStartTimestamp(fragmentToCorrection->GetFragmentStartTimestamp() + correction, true);
                          fragmentToCorrection->SetIncorrectTimestamps(fragment->HasIncorrectTimestamps());

                          CorrectPreviousFragmentEndTimestamp(this->rtmpStreamFragments, i);

                          // correct timestamps in all FLV packets in fragment
                          fragmentToCorrection->SetPacketCorrection(fragmentToCorrection->GetPacketCorrection() + correction);
                        }

                        // we are finished with downloading of this block
                        FREE_MEM_CLASS(this->mainCurlInstance);
                        fragment = NULL;
                        this->streamFragmentDownloading = UINT_MAX;
                      }
                    }
                  }
                  else
                  {
                    // for current fragment is start timestamp set not by FLV packet
                    // or it has been cleared few lines above (handling seeking)
                    // correct it with right value
                    fragment->SetFragmentStartTimestamp(flvPacket->GetTimestamp(), true);
                    CorrectPreviousFragmentEndTimestamp(this->rtmpStreamFragments, this->streamFragmentDownloading);
                  }

                  if (SUCCEEDED(result) && (this->streamFragmentDownloading != UINT_MAX) && (createNewFragment))
                  {
                    // create new fragment with key frame
                    fragment = new CRtmpStreamFragment();
                    CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

                    if (SUCCEEDED(result))
                    {
                      CRtmpStreamFragment *previousFragment = this->rtmpStreamFragments->GetItem(max(this->streamFragmentDownloading, 1) - 1);
                      fragment->SetFragmentStartTimestamp(flvPacket->GetTimestamp(), true);
                      fragment->SetFragmentEndTimestamp(fragmentEnd);
                      fragment->SetIncorrectTimestamps(previousFragment->HasIncorrectTimestamps());

                      result = (this->rtmpStreamFragments->Insert(this->streamFragmentDownloading, fragment)) ? result : E_FAIL;

                      if (SUCCEEDED(result) && (this->setEndOfStream))
                      {
                        // end of stream was reached, increase this->streamFragmentProcessing
                        this->streamFragmentProcessing++;
                      }
                    }

                    if (FAILED(result))
                    {
                      FREE_MEM_CLASS(fragment);
                    }
                  }
                }

                if (fragment != NULL)
                {
                  // just add packet to fragment
                  result = (fragment->GetBuffer()->AddToBufferWithResize(flvPacket->GetData(), flvPacket->GetSize()) == flvPacket->GetSize()) ? result : E_OUTOFMEMORY;
                }
              }

              // downloading instance can be deleted
              if (this->mainCurlInstance != NULL)
              {
                CLockMutex lockData(this->lockCurlMutex, INFINITE);

                this->mainCurlInstance->GetRtmpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(flvPacket->GetSize());
              }
            }

            flvPacket->Clear();
          }

          FREE_MEM_CLASS(flvPacket);
        }
      }

      unsigned int lastFlvPacketTimestamp = 0;

      if (SUCCEEDED(result) && (!this->supressData) && (this->streamFragmentProcessing < this->rtmpStreamFragments->Count()))
      {
        CLinearBuffer *bufferForProcessing = this->FillBufferForProcessing(this->rtmpStreamFragments, this->streamFragmentProcessing, this->storeFilePath);

        if (SUCCEEDED(result) && (!this->supressData) && (bufferForProcessing != NULL))
        {
          CFlvPacket *flvPacket = new CFlvPacket();
          if (flvPacket != NULL)
          {
            while (SUCCEEDED(result) && (flvPacket->ParsePacket(bufferForProcessing) == FLV_PARSE_RESULT_OK))
            {
              // FLV packet parsed correctly
              // push FLV packet to filter

              if ((flvPacket->GetType() == FLV_PACKET_AUDIO) ||
                (flvPacket->GetType() == FLV_PACKET_HEADER) ||
                (flvPacket->GetType() == FLV_PACKET_META) ||
                (flvPacket->GetType() == FLV_PACKET_VIDEO))
              {
                // do nothing, known packet types
                CHECK_CONDITION_HRESULT(result, !flvPacket->IsEncrypted(), result, E_DRM_PROTECTED);
              }
              else
              {
                this->logger->Log(LOGGER_WARNING, L"%s: %s: unknown FLV packet: %d, size: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, flvPacket->GetType(), flvPacket->GetSize());
                result = E_UNKNOWN_STREAM_TYPE;
              }

              if ((flvPacket->GetType() != FLV_PACKET_HEADER) || (!this->seekingActive))
              {
                lastFlvPacketTimestamp = flvPacket->GetTimestamp();

                // create media packet
                // set values of media packet
                CMediaPacket *mediaPacket = new CMediaPacket();
                mediaPacket->GetBuffer()->InitializeBuffer(flvPacket->GetSize());
                mediaPacket->GetBuffer()->AddToBuffer(flvPacket->GetData(), flvPacket->GetSize());
                mediaPacket->SetStart(this->bytePosition);
                mediaPacket->SetEnd(this->bytePosition + flvPacket->GetSize() - 1);

                if (!receiveData->GetMediaPacketCollection()->Add(mediaPacket))
                {
                  FREE_MEM_CLASS(mediaPacket);
                }
                this->bytePosition += flvPacket->GetSize();
              }
              // we are definitely not seeking
              this->seekingActive = false;
              bufferForProcessing->RemoveFromBufferAndMove(flvPacket->GetSize());

              flvPacket->Clear();
            }

            FREE_MEM_CLASS(flvPacket);
          }

          result = (bufferForProcessing->GetBufferOccupiedSpace() == 0) ? result : E_FAIL;

          if (SUCCEEDED(result))
          {
            this->streamFragmentProcessing++;

            // check if fragment is downloaded
            // if fragment is not downloaded, then schedule it for download

            if (this->streamFragmentProcessing < this->rtmpStreamFragments->Count())
            {
              CRtmpStreamFragment *fragment = this->rtmpStreamFragments->GetItem(this->streamFragmentProcessing);
              if ((!fragment->IsDownloaded()) && (this->streamFragmentProcessing != this->streamFragmentDownloading))
              {
                // fragment is not downloaded and also is not downloading currently
                this->streamFragmentToDownload = this->streamFragmentProcessing;
              }
            }
          }
        }

        FREE_MEM_CLASS(bufferForProcessing);
      }

      if (SUCCEEDED(result) && (!this->setLength) && (this->bytePosition != 0))
      {
        if ((this->duration == RTMP_DURATION_UNSPECIFIED) && (this->mainCurlInstance != NULL))
        {
          this->duration = (this->mainCurlInstance->GetDuration() != 0) ? this->mainCurlInstance->GetDuration() : RTMP_DURATION_UNSPECIFIED;
        }

        if (this->duration != RTMP_DURATION_UNSPECIFIED)
        {
          if (lastFlvPacketTimestamp != 0)
          {
            // specified duration in RTMP connect response
            this->streamLength = this->bytePosition * this->duration / lastFlvPacketTimestamp;
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length (by time): %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
            receiveData->GetTotalLength()->SetTotalLength(this->streamLength, true);
          }
        }
        else
        {
          // adjust total length if not already set
          if (this->streamLength == 0)
          {
            // error occured or stream duration is not set
            // just make guess
            this->streamLength = LONGLONG(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
            receiveData->GetTotalLength()->SetTotalLength(this->streamLength, true);
          }
          else if ((this->bytePosition > (this->streamLength * 3 / 4)))
          {
            // it is time to adjust stream length, we are approaching to end but still we don't know total length
            this->streamLength = this->bytePosition * 2;
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: adjusting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
            receiveData->GetTotalLength()->SetTotalLength(this->streamLength, true);
          }
        }
      }

      if (SUCCEEDED(result))
      {
        if ((this->streamFragmentProcessing >= this->rtmpStreamFragments->Count()) &&
          (this->streamFragmentToDownload == UINT_MAX) &&
          (this->streamFragmentDownloading == UINT_MAX) &&
          (this->rtmpStreamFragments->GetFirstNotDownloadedStreamFragment(0) == UINT_MAX))
        {
          // all fragments downloaded and processed
          // whole stream downloaded
          this->wholeStreamDownloaded = true;
          this->isConnected = false;
        }

        if (this->streamFragmentProcessing >= this->rtmpStreamFragments->Count())
        {
          // all stream fragments processed
          // set stream length and report end of stream
          if (!this->seekingActive)
          {
            // we are not seeking, so we can set total length
            if (!this->setLength)
            {
              this->streamLength = this->bytePosition;
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
              receiveData->GetTotalLength()->SetTotalLength(this->streamLength, false);
              this->setLength = true;
            }

            if (!this->setEndOfStream)
            {
              // notify filter the we reached end of stream
              receiveData->GetEndOfStreamReached()->SetStreamPosition(max(0, this->bytePosition - 1));
              this->setEndOfStream = true;
            }
          }
        }
      }

      if ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
      {
        if (this->mainCurlInstance->GetDownloadResponse()->GetResultCode() == CURLE_OK)
        {
          if (this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
          {
            // all data read from CURL instance, received all data
            this->logger->Log(LOGGER_INFO, L"%s: %s: all data received", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
            FREE_MEM_CLASS(this->mainCurlInstance);

            // set end timestamp of last fragment
            CRtmpStreamFragment *fragment = this->rtmpStreamFragments->GetItem(this->rtmpStreamFragments->Count() - 1);
            CHECK_POINTER_HRESULT(result, fragment, result, E_FAIL);

            if (SUCCEEDED(result))
            {
              fragment->SetDownloaded(true);
            }
          }
        }
        else
        {
          // error occured while downloading
          // download fragment again or download scheduled fragment
          this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: %d, restarting download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetResultCode());
          this->streamFragmentToDownload = (this->streamFragmentToDownload != UINT_MAX) ? this->streamFragmentDownloading : this->streamFragmentToDownload;
          this->streamFragmentDownloading = UINT_MAX;
          FREE_MEM_CLASS(this->mainCurlInstance);
          this->isConnected = false;
        }
      }

      if (SUCCEEDED(result) && (this->isConnected) && ((this->mainCurlInstance == NULL) || (this->streamFragmentToDownload != UINT_MAX)))
      {
        FREE_MEM_CLASS(this->mainCurlInstance);
        // no CURL instance exists, we finished or not started download
        // start another download

        // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->rtmpStreamFragments->GetFirstNotDownloadedStreamFragment(this->streamFragmentProcessing) : this->streamFragmentToDownload;
        // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->rtmpStreamFragments->GetFirstNotDownloadedStreamFragment(0) : this->streamFragmentToDownload;
        // fragment to download still can be UINT_MAX = no fragment to download

        if (SUCCEEDED(result) && (this->streamFragmentToDownload != UINT_MAX))
        {
          // starting download from another point

          // clear additional correction, it will be set after analysing key frames
          this->additionalCorrection = 0;

          // there is specified fragment to download

          CRtmpStreamFragment *fragment = this->rtmpStreamFragments->GetItem(this->streamFragmentToDownload);
          CRtmpStreamFragment *previousFragment = this->rtmpStreamFragments->GetItem(max(this->streamFragmentToDownload, 1) - 1);
          CHECK_POINTER_HRESULT(result, fragment, result, E_POINTER);
          CHECK_POINTER_HRESULT(result, previousFragment, result, E_POINTER);

          if (SUCCEEDED(result))
          {
            // fragment was received, but receiving was interrupted
            // get previous fragment and start receiving data from previous fragment
            int64_t rtmpStart = fragment->IsStartTimestampSet() ? previousFragment->GetFragmentStartTimestamp() : fragment->GetFragmentStartTimestamp();

            // clean fragment buffer
            fragment->SetDownloaded(false);
            fragment->SetStoredToFile(-1);
            fragment->GetBuffer()->ClearBuffer();
            this->firstTimestamp = -1;
            this->firstVideoTimestamp = -1;

            fragment->SetSeeked(rtmpStart != 0);
            fragment->SetIncorrectTimestamps(((!fragment->IsStartTimestampSet()) && (rtmpStart != 0)) || (previousFragment->HasIncorrectTimestamps()));

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: starting receiving data from timestamp %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, rtmpStart);

            // create CURL instance
            this->mainCurlInstance = new CRtmpCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
            CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              CRtmpDownloadRequest *request = new CRtmpDownloadRequest();
              CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                request->SetUrl(this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
                request->SetRtmpApp(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_APP, true, RTMP_APP_DEFAULT));
                request->SetRtmpArbitraryData(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_ARBITRARY_DATA, true, NULL));
                request->SetRtmpBuffer(this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_RTMP_BUFFER, true, RTMP_BUFFER_DEFAULT));
                request->SetRtmpFlashVersion(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_FLASHVER, true, RTMP_FLASH_VER_DEFAULT));
                request->SetRtmpAuth(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_AUTH, true, RTMP_AUTH_DEFAULT));
                request->SetRtmpJtv(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_JTV, true, RTMP_JTV_DEFAULT));
                request->SetRtmpLive(this->configurationParameters->GetValueBool(PARAMETER_NAME_RTMP_LIVE, true, RTMP_LIVE_DEFAULT));
                request->SetRtmpPageUrl(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_PAGE_URL, true, RTMP_PAGE_URL_DEFAULT));
                request->SetRtmpPlaylist(this->configurationParameters->GetValueBool(PARAMETER_NAME_RTMP_PLAYLIST, true, RTMP_PLAYLIST_DEFAULT));
                request->SetRtmpPlayPath(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_PLAY_PATH, true, RTMP_PLAY_PATH_DEFAULT));
                request->SetRtmpStart(rtmpStart);
                request->SetRtmpStop(RTMP_STOP_DEFAULT);
                request->SetRtmpSubscribe(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_SUBSCRIBE, true, RTMP_SUBSCRIBE_DEFAULT));
                request->SetRtmpSwfUrl(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_SWF_URL, true, RTMP_SWF_URL_DEFAULT));
                request->SetRtmpSwfVerify(this->configurationParameters->GetValueBool(PARAMETER_NAME_RTMP_SWF_VERIFY, true, RTMP_SWF_VERIFY_DEFAULT));
                request->SetRtmpTcUrl(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_TC_URL, true, RTMP_TC_URL_DEFAULT));
                request->SetRtmpToken(this->configurationParameters->GetValue(PARAMETER_NAME_RTMP_TOKEN, true, RTMP_TOKEN_DEFAULT));

                this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
                this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
                result = (this->mainCurlInstance->Initialize(request)) ? S_OK : E_FAIL;
              }
              FREE_MEM_CLASS(request);
            }

            if (SUCCEEDED(result))
            {
              // all parameters set
              // start receiving data

              result = (this->mainCurlInstance->StartReceivingData()) ? S_OK : E_FAIL;

              if (SUCCEEDED(result))
              {
                this->streamFragmentDownloading = this->streamFragmentToDownload;
                this->streamFragmentToDownload = UINT_MAX;
              }
            }
          }
        }
      }
    }
    else
    {
      // set total length (if not set earlier)
      if (!this->setLength)
      {
        this->streamLength = this->bytePosition;
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
        receiveData->GetTotalLength()->SetTotalLength(this->streamLength, false);
        this->setLength = true;
      }
    }
  }

  // store stream fragments to temporary file
  if ((GetTickCount() - this->lastStoreTime) > 1000)
  {
    this->lastStoreTime = GetTickCount();

    if ((!this->liveStream) && (this->rtmpStreamFragments->Count() > 0))
    {
      // store all stream fragments (which are not stored) to file
      if (this->storeFilePath == NULL)
      {
        this->storeFilePath = this->GetStoreFile();
      }

      if (this->storeFilePath != NULL)
      {
        LARGE_INTEGER size;
        size.QuadPart = 0;

        // open or create file
        HANDLE hTempFile = CreateFile(this->storeFilePath, FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);

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
            unsigned int i = 0;
            while (i < this->rtmpStreamFragments->Count())
            {
              CRtmpStreamFragment *streamFragment = this->rtmpStreamFragments->GetItem(i);

              if ((!streamFragment->IsStoredToFile()) && (streamFragment->IsDownloaded()))
              {
                // if stream fragment is not stored to file
                // store it to file
                unsigned int length = streamFragment->GetLength();

                ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
                if (streamFragment->GetBuffer()->CopyFromBuffer(buffer, length) == length)
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

              i++;
            }
          }

          CloseHandle(hTempFile);
          hTempFile = INVALID_HANDLE_VALUE;
        }
      }
    }

    if (this->liveStream)
    {
      // in case of live stream remove all downloaded fragments before reported stream time
      if ((this->rtmpStreamFragments->Count() > 0) && (this->streamFragmentProcessing != UINT_MAX))
      {
        // leave last fragment in collection in order to not add downloaded and processed fragments
        while ((this->streamFragmentProcessing > 0) && (this->rtmpStreamFragments->Count() > 1))
        {
          CRtmpStreamFragment *fragment = this->rtmpStreamFragments->GetItem(0);

          if (fragment->IsDownloaded() && (fragment->GetFragmentStartTimestamp() < this->reportedStreamTime))
          {
            this->rtmpStreamFragments->Remove(0);
            this->streamFragmentProcessing--;

            if (this->streamFragmentDownloading != UINT_MAX)
            {
              this->streamFragmentDownloading--;
            }
            if (this->streamFragmentToDownload != UINT_MAX)
            {
              this->streamFragmentToDownload--;
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

  return result;
}

CParameterCollection *CMPUrlSourceSplitter_Protocol_Rtmp::GetConnectionParameters(void)
{
  CParameterCollection *result = new CParameterCollection();

  if (result != NULL)
  {
    if (!result->Append(this->configurationParameters))
    {
      FREE_MEM_CLASS(result);
    }
  }
  
  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Rtmp::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configurationParameters);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  this->wholeStreamDownloaded = false;

  if (this->rtmpStreamFragments == NULL)
  {
    this->rtmpStreamFragments = new CRtmpStreamFragmentCollection();
    CHECK_POINTER_HRESULT(result, this->rtmpStreamFragments, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      CRtmpStreamFragment *first = new CRtmpStreamFragment();
      CHECK_POINTER_HRESULT(result, first, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // first fragment is gap
        first->SetFragmentStartTimestamp(0, false);
        first->SetFragmentEndTimestamp(UINT64_MAX);

        result = (this->rtmpStreamFragments->Add(first)) ? result : E_OUTOFMEMORY;
      }

      if (FAILED(result))
      {
        FREE_MEM_CLASS(first);
      }
    }
  }

  if (FAILED(result))
  {
    this->StopReceivingData();
  }

  this->isConnected = SUCCEEDED(result);

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);
  
  this->isConnected = false;
  this->streamFragmentDownloading = UINT_MAX;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::QueryStreamProgress(LONGLONG *total, LONGLONG *current)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, total);
  CHECK_POINTER_DEFAULT_HRESULT(result, current);

  if (SUCCEEDED(result))
  {
    *total = (this->streamLength == 0) ? 1 : this->streamLength;
    *current = (this->streamLength == 0) ? 0 : this->bytePosition;

    if (!this->setLength)
    {
      result = VFW_S_ESTIMATED;
    }
  }

  return result;
}
  
HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);

  if (SUCCEEDED(result))
  {
    availableLength->SetQueryResult(S_OK);
    if (!this->setLength)
    {
      availableLength->SetAvailableLength(this->bytePosition);
    }
    else
    {
      availableLength->SetAvailableLength(this->streamLength);
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  if (this->storeFilePath != NULL)
  {
    DeleteFile(this->storeFilePath);
  }
 
  this->streamLength = 0;
  this->setLength = false;
  this->setEndOfStream = false;
  this->wholeStreamDownloaded = false;
  this->receiveDataTimeout = RTMP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->streamDuration = 0;
  this->bytePosition = 0;
  FREE_MEM_CLASS(this->rtmpStreamFragments);
  FREE_MEM(this->storeFilePath);
  this->isConnected = false;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  this->ignoreKeyFrameTimestamp = 0;
  this->additionalCorrection = 0;
  this->duration = UINT64_MAX;
  this->liveStream = false;
  this->seekingActive = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtmp::GetDuration(void)
{
  return (this->liveStream) ? DURATION_LIVE_STREAM : DURATION_UNSPECIFIED;
}

void CMPUrlSourceSplitter_Protocol_Rtmp::ReportStreamTime(uint64_t streamTime)
{
  this->reportedStreamTime = streamTime;
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Rtmp::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtmp::SeekToTime(int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  // RTMP protocol can seek to ms
  // time is in ms

  // 1 second back
  time = max(0, time - 1000);

  // find fragment to process
  this->streamFragmentProcessing = this->rtmpStreamFragments->Count();
  for (unsigned int i = 0; i < this->rtmpStreamFragments->Count(); i++)
  {
    CRtmpStreamFragment *segFrag = this->rtmpStreamFragments->GetItem(i);

    if ((segFrag->GetFragmentStartTimestamp() <= (uint64_t)time) &&
      (segFrag->GetFragmentEndTimestamp() >= (uint64_t)time))
    {
      this->streamFragmentProcessing = i;
      result = segFrag->GetFragmentStartTimestamp();
      break;
    }
  }

  this->seekingActive = true;
  this->bytePosition = 0;
  this->streamLength = 0;
  this->setLength = false;
  this->setEndOfStream = false;
  // reset whole stream downloaded, but IsConnected() must return true to avoid calling StartReceivingData()
  this->isConnected = true;
  this->wholeStreamDownloaded = false;

  // in this->streamFragmentProcessing is id of fragment to process
  // it must be set, because at least one stream fragment exists and cover all possible timestamps (0 - UINT64_MAX)
  CRtmpStreamFragment *segFrag = this->rtmpStreamFragments->GetItem(this->streamFragmentProcessing);

  if (!segFrag->IsDownloaded())
  {
    result = time;

    // stream fragment is not downloaded, it is gap
    // split stream fragment

    // close connection
    this->StopReceivingData();

    // create new fragment within found fragment
    CRtmpStreamFragment *fragment = new CRtmpStreamFragment();
    result = (fragment != NULL) ? result : (-1);

    if (result != (-1))
    {
      this->streamFragmentProcessing++;
      fragment->SetFragmentStartTimestamp(time, false);
      fragment->SetFragmentEndTimestamp(segFrag->GetFragmentEndTimestamp());

      result = this->rtmpStreamFragments->Insert(this->streamFragmentProcessing, fragment) ? result : (-1);

      if (result != (-1))
      {
        CorrectPreviousFragmentEndTimestamp(this->rtmpStreamFragments, this->streamFragmentProcessing);

        // clear fragment to download
        this->streamFragmentToDownload = UINT_MAX;

        // reopen connection
        // StartReceivingData() reset wholeStreamDownloaded
        this->isConnected = SUCCEEDED(this->StartReceivingData(NULL));
      }
    }
  }
  else
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: timestamp %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, segFrag->GetFragmentStartTimestamp());
  }

  if (!this->IsConnected())
  {
    result = -1;
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Rtmp::SeekToPosition(int64_t start, int64_t end)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu, to time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, start, end);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, result);
  return result;
}

void CMPUrlSourceSplitter_Protocol_Rtmp::SetSupressData(bool supressData)
{
  this->supressData = supressData;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Protocol_Rtmp::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Rtmp::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Rtmp::Initialize(PluginConfiguration *configuration)
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

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_RTMP_RECEIVE_DATA_TIMEOUT, true, RTMP_RECEIVE_DATA_TIMEOUT_DEFAULT);
  this->liveStream = this->configurationParameters->GetValueBool(PARAMETER_NAME_LIVE_STREAM, true, PARAMETER_NAME_LIVE_STREAM_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? RTMP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

  return S_OK;
}

// other methods

wchar_t *CMPUrlSourceSplitter_Protocol_Rtmp::GetStoreFile(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configurationParameters->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_rtmp_%s.temp", folder, guid);
    }
    FREE_MEM(guid);
  }

  return result;
}

CLinearBuffer *CMPUrlSourceSplitter_Protocol_Rtmp::FillBufferForProcessing(CRtmpStreamFragmentCollection *fragments, unsigned int streamFragmentProcessing, wchar_t *storeFile)
{
  CLinearBuffer *result = NULL;

  if (fragments != NULL)
  {
    if (streamFragmentProcessing < fragments->Count())
    {
      CRtmpStreamFragment *fragment = fragments->GetItem(streamFragmentProcessing);

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
            if (fragment->GetBuffer()->CopyFromBuffer(buffer, bufferLength) != bufferLength)
            {
              // error occured while copying data
              FREE_MEM(buffer);
            }
          }
        }

        if ((buffer != NULL) && (fragment->GetPacketCorrection() != 0))
        {
          // correct FLV packets timestamps
          CFlvPacket *flvPacket = new CFlvPacket();
          bool correct = (flvPacket != NULL);
          unsigned int position = 0;

          while (correct && (position < bufferLength))
          {
            correct = (flvPacket->ParsePacket(buffer + position, bufferLength - position) == FLV_PARSE_RESULT_OK);

            if (correct)
            {
              if (flvPacket->GetType() != FLV_PACKET_HEADER)
              {
                flvPacket->SetTimestamp(flvPacket->GetTimestamp() + fragment->GetPacketCorrection());

                memcpy(buffer + position, flvPacket->GetData(), flvPacket->GetSize());
              }
            }

            if (correct)
            {
              position += flvPacket->GetSize();
            }

            flvPacket->Clear();
          }

          FREE_MEM_CLASS(flvPacket);
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
