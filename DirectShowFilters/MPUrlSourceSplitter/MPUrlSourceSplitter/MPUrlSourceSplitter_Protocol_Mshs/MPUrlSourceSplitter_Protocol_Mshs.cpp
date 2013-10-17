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

#include "MPUrlSourceSplitter_Protocol_Mshs.h"
#include "MPUrlSourceSplitter_Protocol_Mshs_Parameters.h"
#include "Parameters.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"

#include "base64.h"
#include "compress_zlib.h"
#include "formatUrl.h"
#include "conversions.h"

#include "MovieFragmentBox.h"
#include "TrackFragmentBox.h"
#include "TrackFragmentHeaderBox.h"
#include "BoxCollection.h"
#include "MovieHeaderBox.h"
#include "BoxFactory.h"
#include "TrackRunBox.h"
#include "BoxConstants.h"

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Mshsd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Mshs"
#endif

#define METHOD_FILL_BUFFER_FOR_PROCESSING_NAME                                L"FillBufferForProcessing()"

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Mshs(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Mshs *pClass = (CMPUrlSourceSplitter_Protocol_Mshs *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Protocol_Mshs::CMPUrlSourceSplitter_Protocol_Mshs(CLogger *logger, CParameterCollection *configuration)
{
  this->configurationParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS);
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
  
  this->receiveDataTimeout = MSHS_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->streamLength = 0;
  this->setLength = false;
  this->setEndOfStream = false;
  this->streamTime = 0;
  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
  this->wholeStreamDownloaded = false;
  this->mainCurlInstance = NULL;
  this->bytePosition = 0;
  this->seekingActive = false;
  this->supressData = false;
  this->bufferForProcessing = NULL;
  this->streamFragments = NULL;
  this->videoTrackFragmentHeaderBox = NULL;
  this->audioTrackFragmentHeaderBox = NULL;
  this->reconstructedHeader = false;
  this->streamingMedia = NULL;
  this->storeFilePath = NULL;
  this->lastStoreTime = 0;
  this->isConnected = false;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  this->lastTrackID = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Protocol_Mshs::~CMPUrlSourceSplitter_Protocol_Mshs()
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

  FREE_MEM_CLASS(this->videoTrackFragmentHeaderBox);
  FREE_MEM_CLASS(this->audioTrackFragmentHeaderBox);
  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->bufferForProcessing);
  FREE_MEM_CLASS(this->configurationParameters);
  FREE_MEM_CLASS(this->streamFragments);
  FREE_MEM_CLASS(this->streamingMedia);
  FREE_MEM(this->storeFilePath);

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

bool CMPUrlSourceSplitter_Protocol_Mshs::IsConnected(void)
{
  return ((this->isConnected) || (this->wholeStreamDownloaded));
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::ReceiveData(CReceiveData *receiveData)
{
  HRESULT result = S_OK;

  CLockMutex lock(this->lockMutex, INFINITE);

  if (this->IsConnected())
  {
    if (!this->wholeStreamDownloaded)
    {
      if ((!this->reconstructedHeader) &&
        (this->videoTrackFragmentHeaderBox != NULL) &&
        (this->audioTrackFragmentHeaderBox != NULL))
      {
        // we need to reconstruct header
        CLinearBuffer *header = new CLinearBuffer();
        CHECK_POINTER_HRESULT(result, header, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          result = (header->InitializeBuffer(MINIMUM_RECEIVED_DATA_FOR_SPLITTER)) ? result : E_FAIL;
        }

        if (SUCCEEDED(result))
        {
          wchar_t *videoData = this->videoTrackFragmentHeaderBox->GetParsedHumanReadable(L"");
          wchar_t *audioData = this->audioTrackFragmentHeaderBox->GetParsedHumanReadable(L"");

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: video track fragment header box:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, videoData);
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: audio track fragment header box:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, audioData);

          FREE_MEM(videoData);
          FREE_MEM(audioData);

          // create file type box
          CFileTypeBox *fileTypeBox = this->CreateFileTypeBox();
          CHECK_POINTER_HRESULT(result, fileTypeBox, result, E_OUTOFMEMORY);
          if (SUCCEEDED(result))
          {
            result = (this->PutBoxIntoBuffer(fileTypeBox, header)) ? result : E_FAIL;
          }
          FREE_MEM_CLASS(fileTypeBox);

          // create movie box
          CMovieBox *movieBox = this->GetMovieBox(this->streamingMedia, this->videoTrackFragmentHeaderBox, this->audioTrackFragmentHeaderBox);
          CHECK_POINTER_HRESULT(result, movieBox, result, E_OUTOFMEMORY);
          if (SUCCEEDED(result))
          {
            result = (this->PutBoxIntoBuffer(movieBox, header)) ? result : E_FAIL;
          }
          FREE_MEM_CLASS(movieBox);

          // if reconstructing is correct, put all data to output buffer
          if (SUCCEEDED(result))
          {
            unsigned int length = header->GetBufferOccupiedSpace();
            result = (length != 0) ? result : E_FAIL;

            if (SUCCEEDED(result))
            {
              result = (this->bufferForProcessing->AddToBufferWithResize(header) == length) ? result : E_FAIL;
            }
          }
        }
        FREE_MEM_CLASS(header);

        if (SUCCEEDED(result))
        {
          // successfully reconstructed header
          // start processing of stream fragments from start
          this->reconstructedHeader = true;
          this->streamFragmentProcessing = 0;
        }
      }


      if (SUCCEEDED(result) && (this->streamFragmentDownloading != UINT_MAX) && (this->mainCurlInstance != NULL))
      {
        {
          CLockMutex lockData(this->lockCurlMutex, INFINITE);

          unsigned int bytesRead = this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
          if (bytesRead != 0)
          {
            CStreamFragment *streamFragment = this->streamFragments->GetItem(this->streamFragmentDownloading);
            CLinearBuffer *linearBuffer = streamFragment->GetBuffer();

            if (linearBuffer != NULL)
            {
              unsigned int bufferSize = linearBuffer->GetBufferSize();
              if (bufferSize == 0)
              {
                // initialize buffer
                linearBuffer->InitializeBuffer(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
                bufferSize = linearBuffer->GetBufferSize();
              }

              unsigned int freeSpace = linearBuffer->GetBufferFreeSpace();

              if (freeSpace < bytesRead)
              {
                unsigned int bufferNewSize = max(bufferSize * 2, bufferSize + bytesRead);
                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: buffer to small, buffer size: %d, new size: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, bufferNewSize);
                if (!linearBuffer->ResizeBuffer(bufferNewSize))
                {
                  this->logger->Log(LOGGER_WARNING, L"%s: %s: resizing buffer unsuccessful, dropping received data", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
                  // error
                  bytesRead = 0;
                }
              }

              if (bytesRead != 0)
              {
                linearBuffer->AddToBufferWithResize(this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData());
                this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(bytesRead);
              }
            }
          }
        }
      }

      if (SUCCEEDED(result) && this->seekingActive && (!this->supressData) && (this->reconstructedHeader))
      {
        CStreamFragment *streamFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);

        if (streamFragment != NULL)
        {
          // this should happen only once per seek
          // created fragmented index box
          CFragmentedIndexBox *fragmentedIndexBox = this->GetFragmentedIndexBox(this->streamingMedia, this->videoTrackFragmentHeaderBox->GetTrackId(), this->audioTrackFragmentHeaderBox->GetTrackId(), streamFragment->GetFragmentTime());

          if (fragmentedIndexBox != NULL)
          {
            // copy fragmented index box to processing
            if (this->bufferForProcessing != NULL)
            {
              this->PutBoxIntoBuffer(fragmentedIndexBox, this->bufferForProcessing);
            }
          }
          FREE_MEM_CLASS(fragmentedIndexBox);
        }
      }

      uint64_t lastTimestamp = 0;
      if (SUCCEEDED(result) && (!this->supressData) && (this->streamFragmentProcessing < this->streamFragments->Count()) && (this->reconstructedHeader))
      {
        CLinearBuffer *bufferForBoxProcessing = this->FillBufferForProcessing(this->streamFragments, this->streamFragmentProcessing, this->storeFilePath);
        if (bufferForBoxProcessing != NULL)
        {
          // buffer successfully filled

          bool continueProcessing = false;
          do
          {
            continueProcessing = false;
            unsigned int length = bufferForBoxProcessing->GetBufferOccupiedSpace();
            if (length > 0)
            {
              continueProcessing = (this->bufferForProcessing->AddToBufferWithResize(bufferForBoxProcessing) == length);
              CHECK_CONDITION_EXECUTE(continueProcessing, bufferForBoxProcessing->RemoveFromBufferAndMove(length));
            }
          } while (continueProcessing);

          if (bufferForBoxProcessing->GetBufferOccupiedSpace() == 0)
          {
            // all data are processed
            continueProcessing = true;
          }

          if (continueProcessing)
          {
            CStreamFragment *currentFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);
            lastTimestamp = currentFragment->GetFragmentTime() + currentFragment->GetFragmentDuration() - 1;

            this->streamFragmentProcessing++;

            // check if stream fragment is downloaded
            // if stream fragment is not downloaded, then schedule it for download

            if (this->streamFragmentProcessing < this->streamFragments->Count())
            {
              CStreamFragment *streamFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);
              if ((!streamFragment->GetDownloaded()) && (this->streamFragmentProcessing != this->streamFragmentDownloading))
              {
                // stream fragment is not downloaded and also is not downloading currently
                this->streamFragmentToDownload = this->streamFragmentProcessing;
              }
            }
          }
        }
        FREE_MEM_CLASS(bufferForBoxProcessing);
      }

      int64_t lastPosition = 0;
      if (SUCCEEDED(result) && (!this->supressData) && (this->bufferForProcessing != NULL))
      {
        unsigned int length = this->bufferForProcessing->GetBufferOccupiedSpace();

        if (length > 0)
        {
          this->seekingActive = false;

          CMediaPacket *mediaPacket = new CMediaPacket();
          if (mediaPacket != NULL)
          {
            if (mediaPacket->GetBuffer()->AddToBufferWithResize(this->bufferForProcessing) == length)
            {
              mediaPacket->SetStart(this->bytePosition);
              mediaPacket->SetEnd(this->bytePosition + length - 1);

              if (!receiveData->GetMediaPacketCollection()->Add(mediaPacket))
              {
                FREE_MEM_CLASS(mediaPacket);
              }
              else
              {
                this->bytePosition += length;
                lastPosition = this->bytePosition;
                this->bufferForProcessing->RemoveFromBufferAndMove(length);
              }
            }
            else
            {
              FREE_MEM_CLASS(mediaPacket);
            }
          }
        }
      }

      if (SUCCEEDED(result) && (!this->setLength) && (this->bytePosition != 0))
      {
        if ((this->streamingMedia->GetDuration() != 0) && (this->streamingMedia->GetTimeScale() != 0))
        {
          if ((lastPosition != 0))
          {
            this->streamLength = (lastTimestamp != 0) ? (lastPosition * this->streamingMedia->GetDuration() / lastTimestamp) : lastPosition;
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length (by time%s): %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, (lastTimestamp != 0) ? L"" : L" and last position", this->streamLength);
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

      if (SUCCEEDED(result) &&
        (this->streamFragmentProcessing >= this->streamFragments->Count()) &&
        (this->streamFragmentToDownload == UINT_MAX) &&
        (this->streamFragmentDownloading == UINT_MAX))
      {
        // all stream fragments downloaded and processed
        // whole stream downloaded
        this->wholeStreamDownloaded = true;
        this->isConnected = false;
        FREE_MEM_CLASS(this->mainCurlInstance);
      }

      if (SUCCEEDED(result) && (this->streamFragmentProcessing >= this->streamFragments->Count()))
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

      if (SUCCEEDED(result) && (this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
      {
        if (this->mainCurlInstance->GetHttpDownloadResponse()->GetResultCode() == CURLE_OK)
        {
          if (this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
          {
            // in CURL instance aren't received data, all data are stored in stream fragment
            // all data received, we're not receiving data
            CStreamFragment *streamFragment = this->streamFragments->GetItem(this->streamFragmentDownloading);
            streamFragment->SetDownloaded(true);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: received all data for url '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetHttpDownloadRequest()->GetUrl());

            // if not set stream fragment to download, then set stream fragment to download (get next not downloaded stream fragment after current downloaded stream fragment)
            this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->GetFirstNotDownloadedStreamFragment(this->streamFragmentDownloading) : this->streamFragmentToDownload;
            // if not set stream fragment to download, then set stream fragment to download (get next not downloaded stream fragment from first stream fragment)
            this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->GetFirstNotDownloadedStreamFragment(0) : this->streamFragmentToDownload;

            // stream fragment to download still can be UINT_MAX = no stream fragment to download
            this->streamFragmentDownloading = UINT_MAX;
            FREE_MEM_CLASS(this->mainCurlInstance);

            // process first video stream fragment or first audio stream fragment (if necessary)
            if ((this->videoTrackFragmentHeaderBox == NULL) && (streamFragment->GetFragmentType() == FRAGMENT_TYPE_VIDEO))
            {
              this->videoTrackFragmentHeaderBox = this->GetTrackFragmentHeaderBox(streamFragment->GetBuffer(), ++this->lastTrackID);
            }

            if ((this->audioTrackFragmentHeaderBox == NULL) && (streamFragment->GetFragmentType() == FRAGMENT_TYPE_AUDIO))
            {
              this->audioTrackFragmentHeaderBox = this->GetTrackFragmentHeaderBox(streamFragment->GetBuffer(), ++this->lastTrackID);
            }

            // process stream fragment data
            // in track fragment header box we need to set track ID
            // in track run box we need to set flasg and data offset

            unsigned int inputLength = streamFragment->GetBuffer()->GetBufferOccupiedSpace();
            unsigned int inputPosition = 0;

            unsigned int outputLength = 0;
            unsigned int outputPosition = 0;
            unsigned int movieFragmentSize  = 0;

            if (inputLength > 0)
            {
              // we must process data twice - first we get size for output buffer, second we put processed data to output buffer
              ALLOC_MEM_DEFINE_SET(inputBuffer, uint8_t, inputLength, 0);
              CHECK_POINTER_HRESULT(result, inputBuffer, result, E_OUTOFMEMORY);

              CBoxFactory *boxFactory = new CBoxFactory();
              CHECK_POINTER_HRESULT(result, boxFactory, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                result = (streamFragment->GetBuffer()->CopyFromBuffer(inputBuffer, inputLength) == inputLength) ? result : E_FAIL;
              }

              // first processing - get size for output buffer
              if (SUCCEEDED(result))
              {
                while (SUCCEEDED(result) && (inputPosition < inputLength))
                {
                  CBox *box = boxFactory->CreateBox(inputBuffer + inputPosition, inputLength - inputPosition);
                  CHECK_POINTER_HRESULT(result, box, result, E_FAIL);

                  if (SUCCEEDED(result))
                  {
                    inputPosition += (unsigned int)box->GetSize();

                    if (box->IsType(MOVIE_FRAGMENT_BOX_TYPE))
                    {
                      // movie fragment box, it is root box for ther boxes

                      CMovieFragmentBox *movieFragmentBox = dynamic_cast<CMovieFragmentBox *>(box);
                      // reset its size to get correct size after changes
                      movieFragmentBox->ResetSize();

                      for (unsigned int i = 0; (SUCCEEDED(result) && (i < movieFragmentBox->GetBoxes()->Count())); i++)
                      {
                        CBox *innerMovieFragmentBox = movieFragmentBox->GetBoxes()->GetItem(i);
                        // reset its size to get correct size after changes
                        innerMovieFragmentBox->ResetSize();

                        if (innerMovieFragmentBox->IsType(TRACK_FRAGMENT_BOX_TYPE))
                        {
                          CTrackFragmentBox *trackFragmentBox = dynamic_cast<CTrackFragmentBox *>(innerMovieFragmentBox);

                          for (unsigned int j = 0; (SUCCEEDED(result) && (j < trackFragmentBox->GetBoxes()->Count())); j++)
                          {
                            CBox *innerTrackFragmentBox = trackFragmentBox->GetBoxes()->GetItem(j);
                            innerTrackFragmentBox->ResetSize();

                            if (innerTrackFragmentBox->IsType(TRACK_FRAGMENT_HEADER_BOX_TYPE))
                            {
                              // replace track ID in track fragment header with track ID in corresponding track fragment header (video, audio)
                              // remove base data offset flag from track fragment header box
                              CTrackFragmentHeaderBox *trackFragmentHeaderBox = dynamic_cast<CTrackFragmentHeaderBox *>(innerTrackFragmentBox);

                              trackFragmentHeaderBox->SetFlags(trackFragmentHeaderBox->GetFlags() & (~FLAGS_BASE_DATA_OFFSET_PRESENT));
                              trackFragmentHeaderBox->SetTrackId((streamFragment->GetFragmentType() == FRAGMENT_TYPE_VIDEO) ? this->videoTrackFragmentHeaderBox->GetTrackId() : this->audioTrackFragmentHeaderBox->GetTrackId());
                            }

                            if (innerTrackFragmentBox->IsType(TRACK_RUN_BOX_TYPE))
                            {
                              CTrackRunBox *trackRunBox = dynamic_cast<CTrackRunBox *>(innerTrackFragmentBox);

                              // add to flags that we specify data offset present
                              // we can't now set data offset, because not all boxes are processed

                              trackRunBox->SetFlags(trackRunBox->GetFlags() | FLAGS_DATA_OFFSET_PRESENT);
                            }
                          }
                        }
                      }

                      movieFragmentSize = (unsigned int)movieFragmentBox->GetSize();
                    }

                    // increase output buffer length by required box size
                    outputLength += (unsigned int)box->GetSize();
                  }

                  FREE_MEM_CLASS(box);
                }
              }

              result = (outputLength != 0) ? result : E_FAIL;
              if (SUCCEEDED(result))
              {
                ALLOC_MEM_DEFINE_SET(outputBuffer, uint8_t, outputLength, 0);
                CHECK_POINTER_HRESULT(result, outputBuffer, result, E_OUTOFMEMORY);

                // second processing - process data and put processed data to output buffer
                if (SUCCEEDED(result))
                {
                  inputPosition = 0;

                  while (SUCCEEDED(result) && (inputPosition < inputLength))
                  {
                    CBox *box = boxFactory->CreateBox(inputBuffer + inputPosition, inputLength - inputPosition);
                    CHECK_POINTER_HRESULT(result, box, result, E_FAIL);

                    if (SUCCEEDED(result))
                    {
                      inputPosition += (unsigned int)box->GetSize();

                      if (box->IsType(MOVIE_FRAGMENT_BOX_TYPE))
                      {
                        // movie fragment box, it is root box for ther boxes

                        CMovieFragmentBox *movieFragmentBox = dynamic_cast<CMovieFragmentBox *>(box);
                        // reset its size to get correct size after changes
                        movieFragmentBox->ResetSize();

                        for (unsigned int i = 0; (SUCCEEDED(result) && (i < movieFragmentBox->GetBoxes()->Count())); i++)
                        {
                          CBox *innerMovieFragmentBox = movieFragmentBox->GetBoxes()->GetItem(i);
                          // reset its size to get correct size after changes
                          innerMovieFragmentBox->ResetSize();

                          if (innerMovieFragmentBox->IsType(TRACK_FRAGMENT_BOX_TYPE))
                          {
                            CTrackFragmentBox *trackFragmentBox = dynamic_cast<CTrackFragmentBox *>(innerMovieFragmentBox);

                            for (unsigned int j = 0; (SUCCEEDED(result) && (j < trackFragmentBox->GetBoxes()->Count())); j++)
                            {
                              CBox *innerTrackFragmentBox = trackFragmentBox->GetBoxes()->GetItem(j);
                              innerTrackFragmentBox->ResetSize();

                              if (innerTrackFragmentBox->IsType(TRACK_FRAGMENT_HEADER_BOX_TYPE))
                              {
                                // replace track ID in track fragment header with track ID in corresponding track fragment header (video, audio)
                                // remove base data offset flag from track fragment header box
                                CTrackFragmentHeaderBox *trackFragmentHeaderBox = dynamic_cast<CTrackFragmentHeaderBox *>(innerTrackFragmentBox);

                                trackFragmentHeaderBox->SetFlags(trackFragmentHeaderBox->GetFlags() & (~FLAGS_BASE_DATA_OFFSET_PRESENT));
                                trackFragmentHeaderBox->SetTrackId((streamFragment->GetFragmentType() == FRAGMENT_TYPE_VIDEO) ? this->videoTrackFragmentHeaderBox->GetTrackId() : this->audioTrackFragmentHeaderBox->GetTrackId());
                              }

                              if (innerTrackFragmentBox->IsType(TRACK_RUN_BOX_TYPE))
                              {
                                CTrackRunBox *trackRunBox = dynamic_cast<CTrackRunBox *>(innerTrackFragmentBox);

                                // add to flags that we specify data offset present
                                // set data offset to movieFragmentSize + BOX_HEADER_LENGTH (length and type of media data box)

                                trackRunBox->SetFlags(trackRunBox->GetFlags() | FLAGS_DATA_OFFSET_PRESENT);
                                trackRunBox->SetDataOffset(movieFragmentSize + BOX_HEADER_LENGTH);
                              }
                            }
                          }
                        }
                      }

                      // copy box to output buffer
                      result = (box->GetBox(outputBuffer + outputPosition, outputLength - outputPosition)) ? result : E_FAIL;
                      outputPosition += (unsigned int)box->GetSize();
                    }

                    FREE_MEM_CLASS(box);
                  }

                  if (SUCCEEDED(result))
                  {
                    // we have processed fragment, store it to stream fragment
                    streamFragment->GetBuffer()->DeleteBuffer();
                    result = (streamFragment->GetBuffer()->AddToBufferWithResize(outputBuffer, outputLength) == outputLength) ? result : E_OUTOFMEMORY;
                  }
                }

                FREE_MEM(outputBuffer);
              }

              FREE_MEM_CLASS(boxFactory);
              FREE_MEM(inputBuffer);
            }
          }
        }
        else
        {
          // error occured while downloading
          // download stream fragment again or download scheduled stream fragment
          this->streamFragmentToDownload = (this->streamFragmentToDownload != UINT_MAX) ? this->streamFragmentDownloading : this->streamFragmentToDownload;
          this->streamFragmentDownloading = UINT_MAX;
          FREE_MEM_CLASS(this->mainCurlInstance);
          this->isConnected = false;
        }
      }

      if (SUCCEEDED(result) && (this->isConnected) && (this->mainCurlInstance == NULL))
      {
        // no CURL instance exists, we finished download
        // start another one download

        // check if first video stream fragment and first audio stream fragment downloaded
        // in another case schedule download of what is missing (prefer video)
        if (this->videoTrackFragmentHeaderBox == NULL)
        {
          unsigned int i = 0;
          while (i < this->streamFragments->Count())
          {
            CStreamFragment *fragment = this->streamFragments->GetItem(i);

            if (fragment->GetFragmentType() == FRAGMENT_TYPE_VIDEO)
            {
              this->streamFragmentToDownload = i;
              break;
            }

            i++;
          }
        }

        if ((this->audioTrackFragmentHeaderBox == NULL) && (this->streamFragmentToDownload == UINT_MAX))
        {
          unsigned int i = 0;
          while (i < this->streamFragments->Count())
          {
            CStreamFragment *fragment = this->streamFragments->GetItem(i);

            if (fragment->GetFragmentType() == FRAGMENT_TYPE_AUDIO)
            {
              this->streamFragmentToDownload = i;
              break;
            }

            i++;
          }
        }

        // if not set stream fragment to download, then set stream fragment to download (get next not downloaded stream fragment after current processed stream fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->GetFirstNotDownloadedStreamFragment(this->streamFragmentProcessing) : this->streamFragmentToDownload;
        // if not set stream fragment to download, then set stream fragment to download (get next not downloaded stream fragment from first stream fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->GetFirstNotDownloadedStreamFragment(0) : this->streamFragmentToDownload;
        // stream fragment to download still can be UINT_MAX = no stream fragment to download

        if (this->streamFragmentToDownload != UINT_MAX)
        {
          // there is specified stream fragment to download

          CStreamFragment *streamFragment = this->streamFragments->GetItem(this->streamFragmentToDownload);
          if (streamFragment != NULL)
          {
            // clear stream fragment buffer
            // there can be some data from previous unfinished download
            streamFragment->GetBuffer()->ClearBuffer();

            if (SUCCEEDED(result))
            {
              // we need to download for another url
              this->mainCurlInstance = new CHttpCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
              CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                CHttpDownloadRequest *request = new CHttpDownloadRequest();
                CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

                if (SUCCEEDED(request))
                {
                  request->SetUrl(streamFragment->GetUrl());
                  request->SetReferer(this->configurationParameters->GetValue(PARAMETER_NAME_MSHS_REFERER, true, NULL));
                  request->SetUserAgent(this->configurationParameters->GetValue(PARAMETER_NAME_MSHS_USER_AGENT, true, NULL));
                  request->SetCookie(this->configurationParameters->GetValue(PARAMETER_NAME_MSHS_COOKIE, true, NULL));
                  request->SetHttpVersion(this->configurationParameters->GetValueLong(PARAMETER_NAME_MSHS_VERSION, true, HTTP_VERSION_DEFAULT));
                  request->SetIgnoreContentLength((this->configurationParameters->GetValueLong(PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));

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

  // store stream fragments to temporary file, do not store any data until video and audio track fragment header box are created
  if (((GetTickCount() - this->lastStoreTime) > 1000) && (this->videoTrackFragmentHeaderBox != NULL) && (this->audioTrackFragmentHeaderBox != NULL))
  {
    this->lastStoreTime = GetTickCount();

    if (this->streamFragments->Count() > 0)
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
            while (i < this->streamFragments->Count())
            {
              CStreamFragment *streamFragment = this->streamFragments->GetItem(i);

              if ((!streamFragment->IsStoredToFile()) && (streamFragment->GetDownloaded()))
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
  }

  return result;
}

CParameterCollection *CMPUrlSourceSplitter_Protocol_Mshs::GetConnectionParameters(void)
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

unsigned int CMPUrlSourceSplitter_Protocol_Mshs::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configurationParameters);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  this->wholeStreamDownloaded = false;

  if (this->streamFragments == NULL)
  {
    char *encoded = ConvertToMultiByteW(this->configurationParameters->GetValue(PARAMETER_NAME_MSHS_MANIFEST, true, NULL));
    unsigned char *compressedManifestDecoded = NULL;
    uint32_t compressedManifestDecodedLength = 0;
    result = base64_decode(encoded, &compressedManifestDecoded, &compressedManifestDecodedLength);

    if (SUCCEEDED(result))
    {
      // decompress manifest
      uint8_t *decompressedManifest = NULL;
      uint32_t decompressedLength = 0;

      result = decompress_zlib(compressedManifestDecoded, compressedManifestDecodedLength, &decompressedManifest, &decompressedLength);

      if (SUCCEEDED(result))
      {
        this->streamingMedia = new CMSHSSmoothStreamingMedia();
        CHECK_POINTER_HRESULT(result, this->streamingMedia, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          result = (this->streamingMedia->Deserialize(decompressedManifest)) ? S_OK : HRESULT_FROM_WIN32(ERROR_INVALID_DATA);

          if (SUCCEEDED(result))
          {
            FREE_MEM(this->streamFragments);
            this->streamFragments = this->GetStreamFragmentsFromManifest(
              this->logger,
              METHOD_START_RECEIVING_DATA_NAME,
              this->configurationParameters,
              this->streamingMedia,
              false);
            CHECK_POINTER_HRESULT(result, this->streamFragments, result, E_POINTER);
          }
          else
          {
            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot deserialize manifest");
          }
        }

        if (FAILED(result))
        {
          FREE_MEM_CLASS(this->streamingMedia);
        }
      }
      else
      {
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot decompress manifest");
      }
    }
    else
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot decode manifest");
    }
  }

  if (SUCCEEDED(result) && (this->bufferForProcessing == NULL))
  {
    this->bufferForProcessing = new CLinearBuffer();
    CHECK_POINTER_HRESULT(result, this->bufferForProcessing, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    result = (this->bufferForProcessing->InitializeBuffer(MINIMUM_RECEIVED_DATA_FOR_SPLITTER)) ? result : E_FAIL;
  }

  if (SUCCEEDED(result) && (this->videoTrackFragmentHeaderBox == NULL) && (this->audioTrackFragmentHeaderBox == NULL))
  {
    // start downloading first video and first audio stream fragments
    // both are needed for header reconstruction

    CStreamFragment *videoStreamFragment = NULL;
    CStreamFragment *audioStreamFragment = NULL;

    // find first not downloaded video and audio streams
    unsigned int i = 0;
    while (((videoStreamFragment == NULL) || (audioStreamFragment == NULL)) && (i < this->streamFragments->Count()))
    {
      CStreamFragment *fragment = this->streamFragments->GetItem(i);

      if ((videoStreamFragment == NULL) && (fragment->GetFragmentType() == FRAGMENT_TYPE_VIDEO))
      {
        videoStreamFragment = fragment;

        // process first video stream fragment, then audio stream fragment
        this->streamFragmentProcessing = i;
      }

      if ((audioStreamFragment == NULL) && (fragment->GetFragmentType() == FRAGMENT_TYPE_AUDIO))
      {
        audioStreamFragment = fragment;
      }

      i++;
    }

    CHECK_POINTER_HRESULT(result, videoStreamFragment, result, E_POINTER);
    CHECK_POINTER_HRESULT(result, audioStreamFragment, result, E_POINTER);

    if (FAILED(result))
    {
      this->streamFragmentProcessing = 0;
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

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);

  FREE_MEM_CLASS(this->bufferForProcessing);
  this->isConnected = false;
  this->streamFragmentDownloading = UINT_MAX;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::QueryStreamProgress(LONGLONG *total, LONGLONG *current)
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
  
HRESULT CMPUrlSourceSplitter_Protocol_Mshs::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
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

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::ClearSession(void)
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

  FREE_MEM_CLASS(this->bufferForProcessing);
  FREE_MEM_CLASS(this->streamingMedia);
 
  this->streamLength = 0;
  this->setLength = false;
  this->setEndOfStream = false;
  this->streamTime = 0;
  this->wholeStreamDownloaded = false;
  this->receiveDataTimeout = MSHS_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->bytePosition = 0;
  FREE_MEM(this->storeFilePath);
  this->isConnected = false;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  FREE_MEM_CLASS(this->videoTrackFragmentHeaderBox);
  FREE_MEM_CLASS(this->audioTrackFragmentHeaderBox);
  this->reconstructedHeader = false;
  this->lastTrackID = 0;
  this->seekingActive = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

int64_t CMPUrlSourceSplitter_Protocol_Mshs::GetDuration(void)
{
  return DURATION_UNSPECIFIED;
}

void CMPUrlSourceSplitter_Protocol_Mshs::ReportStreamTime(uint64_t streamTime)
{
  this->reportedStreamTime = streamTime;
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Mshs::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_Mshs::SeekToTime(int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  // MSHS protocol can seek to ms
  // time is in ms

  time = time * this->streamingMedia->GetTimeScale() / 1000;

  // find stream fragment to process
  if (this->streamFragments != NULL)
  {
    for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
    {
      CStreamFragment *fragment = this->streamFragments->GetItem(i);

      if ((fragment->GetFragmentType() == FRAGMENT_TYPE_VIDEO) && (fragment->GetFragmentTime() <= (uint64_t)time))
      {
        this->streamFragmentProcessing = i;
        result = fragment->GetFragmentTime();
      }
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

  // in this->sstreamFragmentProcessing is id of stream fragment to process
  CStreamFragment *streamFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);

  if (streamFragment != NULL)
  {
    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: url '%s', timestamp: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME,
      streamFragment->GetUrl(), streamFragment->GetFragmentTime());

    if (!streamFragment->GetDownloaded())
    {
      // close connection
      this->StopReceivingData();

      // clear stream fragment to download
      this->streamFragmentToDownload = UINT_MAX;

      // reopen connection
      // StartReceivingData() reset wholeStreamDownloaded
      this->isConnected = SUCCEEDED(this->StartReceivingData(NULL));
    }
  }
  else
  {
    this->isConnected = false;
  }

  if (!this->IsConnected())
  {
    result = -1;
  }
  else
  {
    this->streamTime = result;
  }

  if (result >= 0)
  {
    result = result * 1000 / this->streamingMedia->GetTimeScale();
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Mshs::SeekToPosition(int64_t start, int64_t end)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from position: %llu, to position: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, start, end);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, result);
  return result;
}

void CMPUrlSourceSplitter_Protocol_Mshs::SetSupressData(bool supressData)
{
  this->supressData = supressData;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Protocol_Mshs::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Mshs::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::Initialize(PluginConfiguration *configuration)
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

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_MSHS_RECEIVE_DATA_TIMEOUT, true, MSHS_RECEIVE_DATA_TIMEOUT_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? MSHS_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

  return S_OK;
}

// other methods

unsigned int CMPUrlSourceSplitter_Protocol_Mshs::GetFirstNotDownloadedStreamFragment(unsigned int start)
{
  unsigned int result = UINT_MAX;

  for (unsigned int i = start; i < this->streamFragments->Count(); i++)
  {
    if (!this->streamFragments->GetItem(i)->GetDownloaded())
    {
      result = i;
      break;
    }
  }

  return result;
}

CStreamFragmentCollection *CMPUrlSourceSplitter_Protocol_Mshs::GetStreamFragmentsFromManifest(CLogger *logger, const wchar_t *methodName, CParameterCollection *configurationParameters, CMSHSSmoothStreamingMedia *manifest, bool logCollection)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, logger);
  CHECK_POINTER_DEFAULT_HRESULT(result, methodName);
  CHECK_POINTER_DEFAULT_HRESULT(result, configurationParameters);
  CHECK_POINTER_DEFAULT_HRESULT(result, manifest);

  CStreamFragmentCollection *streamFragments = NULL;
  if (SUCCEEDED(result))
  {
    streamFragments = new CStreamFragmentCollection();
    CHECK_POINTER_HRESULT(result, streamFragments, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result))
  {
    uint32_t videoIndex = 0;
    uint32_t audioIndex = 0;
    uint64_t lastTimestamp = 0;
    uint32_t maxVideoIndex = 0;
    uint32_t maxAudioIndex = 0;

    // get maximum video and audio indexes
    for (unsigned int i = 0; i < manifest->GetStreams()->Count(); i++)
    {
      CMSHSStream *stream = manifest->GetStreams()->GetItem(i);

      if (stream->IsVideo())
      {
        maxVideoIndex = stream->GetStreamFragments()->Count();
      }
      else if (stream->IsAudio())
      {
        maxAudioIndex = stream->GetStreamFragments()->Count();
      }
    }

    const wchar_t *videoUrlPattern = NULL;
    const wchar_t *audioUrlPattern = NULL;
    CMSHSTrack *videoTrack = NULL;
    CMSHSTrack *audioTrack = NULL;
    const wchar_t *baseUrl = configurationParameters->GetValue(PARAMETER_NAME_MSHS_BASE_URL, true, NULL);

    while ((videoIndex < maxVideoIndex) || (audioIndex < maxAudioIndex))
    {
      // there is still some fragment to add to stream fragments
      // choose fragment which is nearest to last timestamp

      CMSHSStreamFragment *videoFragment = NULL;
      CMSHSStreamFragment *audioFragment = NULL;

      for (unsigned int i = 0; i < manifest->GetStreams()->Count(); i++)
      {
        CMSHSStream *stream = manifest->GetStreams()->GetItem(i);

        if (stream->IsVideo() && (videoIndex < maxVideoIndex))
        {
          videoTrack = stream->GetTracks()->GetItem(0);
          videoUrlPattern = stream->GetUrl();
          videoFragment = stream->GetStreamFragments()->GetItem(videoIndex);
        }
        else if (stream->IsAudio() && (audioIndex < maxAudioIndex))
        {
          audioTrack = stream->GetTracks()->GetItem(0);
          audioUrlPattern = stream->GetUrl();
          audioFragment = stream->GetStreamFragments()->GetItem(audioIndex);
        }
      }

      wchar_t *url = NULL;
      uint64_t fragmentTime = 0;
      uint64_t fragmentDuration = 0;
      unsigned int fragmentType = FRAGMENT_TYPE_UNSPECIFIED;

      if (SUCCEEDED(result))
      {
        if ((videoFragment != NULL) && (audioFragment != NULL))
        {
          uint64_t videoDiff = videoFragment->GetFragmentTime() - lastTimestamp;
          uint64_t audioDiff = audioFragment->GetFragmentTime() - lastTimestamp;

          if (videoDiff <= audioDiff)
          {
            fragmentTime = videoFragment->GetFragmentTime();
            fragmentDuration = videoFragment->GetFragmentDuration();
            fragmentType = FRAGMENT_TYPE_VIDEO;
            url = this->FormatUrl(baseUrl, videoUrlPattern, videoTrack, videoFragment);
            videoIndex++;
          }
          else if (audioDiff < videoDiff)
          {
            fragmentTime = audioFragment->GetFragmentTime();
            fragmentDuration = audioFragment->GetFragmentDuration();
            fragmentType = FRAGMENT_TYPE_AUDIO;
            url = this->FormatUrl(baseUrl, audioUrlPattern, audioTrack, audioFragment);
            audioIndex++;
          }
        }
        else if (videoFragment != NULL)
        {
          fragmentTime = videoFragment->GetFragmentTime();
          fragmentDuration = videoFragment->GetFragmentDuration();
          fragmentType = FRAGMENT_TYPE_VIDEO;
          url = this->FormatUrl(baseUrl, videoUrlPattern, videoTrack, videoFragment);
          videoIndex++;
        }
        else if (audioFragment != NULL)
        {
          fragmentTime = audioFragment->GetFragmentTime();
          fragmentDuration = audioFragment->GetFragmentDuration();
          fragmentType = FRAGMENT_TYPE_AUDIO;
          url = this->FormatUrl(baseUrl, audioUrlPattern, audioTrack, audioFragment);
          audioIndex++;
        }
        else
        {
          // bad case, this should not happen
          logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, methodName, L"no audio or video fragment to process");
          result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
        }
      }

      if (SUCCEEDED(result))
      {
        lastTimestamp = fragmentTime;

        //CStreamFragment *streamFragment = new CStreamFragment(url, fragmentDuration * 1000 / manifest->GetTimeScale(), fragmentTime * 1000 / manifest->GetTimeScale(), fragmentType);
        CStreamFragment *streamFragment = new CStreamFragment(url, fragmentDuration, fragmentTime, fragmentType);
        CHECK_POINTER_HRESULT(result, streamFragment, result, E_OUTOFMEMORY);

        // add stream fragment to stream fragments
        if (SUCCEEDED(result))
        {
          result = (streamFragments->Add(streamFragment)) ? S_OK : E_FAIL;
        }

        if (FAILED(result))
        {
          FREE_MEM_CLASS(streamFragment);
        }
      }

      FREE_MEM(url);
    }

    result = (streamFragments->Count() > 0) ? result : E_FAIL;

    if (SUCCEEDED(result) && (logCollection))
    {
      wchar_t *streamFragmentLog = NULL;
      for (unsigned int i = 0; i < streamFragments->Count(); i++)
      {
        CStreamFragment *streamFragment = streamFragments->GetItem(i);

        wchar_t *temp = FormatString(L"%s%surl '%s', timestamp: %llu", (i == 0) ? L"" : streamFragmentLog, (i == 0) ? L"" : L"\n", streamFragment->GetUrl(), streamFragment->GetFragmentTime());
        FREE_MEM(streamFragmentLog);
        streamFragmentLog = temp;
      }

      if (streamFragmentLog != NULL)
      {
        logger->Log(LOGGER_VERBOSE, L"%s: %s: stream fragments:\n%s", PROTOCOL_IMPLEMENTATION_NAME, methodName, streamFragmentLog);
      }

      FREE_MEM(streamFragmentLog);
    }
  }

  if (FAILED(result))
  {
    FREE_MEM_CLASS(streamFragments);
  }

  return streamFragments;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Mshs::FormatUrl(const wchar_t *baseUrl, const wchar_t *urlPattern, CMSHSTrack *track, CMSHSStreamFragment *fragment)
{
  wchar_t *result = NULL;

  if ((baseUrl != NULL) && (urlPattern != NULL) && (track != NULL) && (fragment != NULL))
  {
    // in url pattern replace {bitrate} or {Bitrate} with track bitrate bitrate
    // in url pattern replace {start time} or {Start time} with fragment time

    wchar_t *bitrate = FormatString(L"%u", track->GetBitrate());
    wchar_t *startTime = FormatString(L"%llu", fragment->GetFragmentTime());

    if ((bitrate != NULL) && (startTime != NULL))
    {
      wchar_t *replaced1 = ReplaceString(urlPattern, L"{bitrate}", bitrate);
      wchar_t *replaced2 = ReplaceString(replaced1, L"{Bitrate}", bitrate);

      wchar_t *replaced3 = ReplaceString(replaced2, L"{start time}", startTime);
      wchar_t *replaced4 = ReplaceString(replaced3, L"{Start time}", startTime);

      result = FormatAbsoluteUrl(baseUrl, replaced4);

      FREE_MEM(replaced1);
      FREE_MEM(replaced2);
      FREE_MEM(replaced3);
      FREE_MEM(replaced4);
    }

    FREE_MEM(bitrate);
    FREE_MEM(startTime);
  }

  return result;
}

CFileTypeBox *CMPUrlSourceSplitter_Protocol_Mshs::CreateFileTypeBox(void)
{
  bool continueCreating = true;
  CFileTypeBox *fileTypeBox = new CFileTypeBox();
  continueCreating &= (fileTypeBox != NULL);

  if (continueCreating)
  {
    continueCreating &= fileTypeBox->GetMajorBrand()->SetBrandString(L"isml");
  }

  if (continueCreating)
  {
    fileTypeBox->SetMinorVersion(512);

    CBrand *brand = new CBrand();
    continueCreating &= (brand != NULL);

    if (continueCreating)
    {
      continueCreating &= brand->SetBrandString(L"piff");
      if (continueCreating)
      {
        continueCreating &= fileTypeBox->GetCompatibleBrands()->Add(brand);
      }
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(brand);
    }

    if (continueCreating)
    {
      brand = new CBrand();
      continueCreating &= (brand != NULL);

      if (continueCreating)
      {
        continueCreating &= brand->SetBrandString(L"iso2");
        if (continueCreating)
        {
          continueCreating &= fileTypeBox->GetCompatibleBrands()->Add(brand);
        }
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(brand);
      }
    }
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(fileTypeBox);
  }

  return fileTypeBox;
}

CTrackFragmentHeaderBox *CMPUrlSourceSplitter_Protocol_Mshs::GetTrackFragmentHeaderBox(CLinearBuffer *buffer, unsigned int trackID)
{
  CTrackFragmentHeaderBox *result = NULL;
  unsigned int bytesRead = buffer->GetBufferOccupiedSpace();

  if (bytesRead != 0)
  {
    ALLOC_MEM_DEFINE_SET(tempBuffer, unsigned char, bytesRead, 0);

    if (tempBuffer != NULL)
    {
      buffer->CopyFromBuffer(tempBuffer, bytesRead);

      CMovieFragmentBox *movieFragmentBox = new CMovieFragmentBox();

      if (movieFragmentBox != NULL)
      {
        if (movieFragmentBox->Parse(tempBuffer, bytesRead))
        {
          for (unsigned int i = 0; ((result == NULL) && (i < movieFragmentBox->GetBoxes()->Count())); i++)
          {
            CBox *trackFragmentBox = movieFragmentBox->GetBoxes()->GetItem(i);

            if (trackFragmentBox->IsType(TRACK_FRAGMENT_BOX_TYPE))
            {
              for (unsigned int j = 0; ((result == NULL) && (j < trackFragmentBox->GetBoxes()->Count())); j++)
              {
                CBox *trackFragmentHeaderBox = trackFragmentBox->GetBoxes()->GetItem(j);

                if (trackFragmentHeaderBox->IsType(TRACK_FRAGMENT_HEADER_BOX_TYPE))
                {
                  // we found video track fragment header box
                  // we can't return reference because movie fragment box is container and will be destroyed
                  // we can save track fragment header box into buffer and then create track fragment header box from buffer

                  uint32_t trackFragmentHeaderBoxSize = (uint32_t)trackFragmentHeaderBox->GetSize();
                  if (trackFragmentHeaderBoxSize != 0)
                  {
                    ALLOC_MEM_DEFINE_SET(trackFragmentHeaderBoxBuffer, uint8_t, trackFragmentHeaderBoxSize, 0);
                    if (trackFragmentHeaderBoxBuffer != NULL)
                    {
                      if (trackFragmentHeaderBox->GetBox(trackFragmentHeaderBoxBuffer, trackFragmentHeaderBoxSize))
                      {
                        result = new CTrackFragmentHeaderBox();
                        if (result != NULL)
                        {
                          if (!result->Parse(trackFragmentHeaderBoxBuffer, trackFragmentHeaderBoxSize))
                          {
                            FREE_MEM_CLASS(result);
                          }
                        }

                        if ((result != NULL) && (trackID != UINT_MAX))
                        {
                          result->SetTrackId(trackID);
                        }
                      }
                    }
                    FREE_MEM(trackFragmentHeaderBoxBuffer);
                  }
                }
              }
            }
          }
        }
      }

      FREE_MEM_CLASS(movieFragmentBox);
    }

    FREE_MEM(tempBuffer);
  }

  return result;
}

bool CMPUrlSourceSplitter_Protocol_Mshs::PutBoxIntoBuffer(CBox *box, CLinearBuffer *buffer)
{
  bool result = false;

  if ((box != NULL) && (buffer != NULL))
  {
    // copy box to buffer
    uint32_t boxBufferLength = (uint32_t)box->GetSize();
    if (boxBufferLength != 0)
    {
      ALLOC_MEM_DEFINE_SET(boxBuffer, unsigned char, boxBufferLength, 0);

      if (boxBuffer != NULL)
      {
        if (box->GetBox(boxBuffer, boxBufferLength))
        {
          result = (buffer->AddToBufferWithResize(boxBuffer, boxBufferLength) == boxBufferLength);
        }
      }
      FREE_MEM(boxBuffer);
    }
  }

  return result;
}

CMovieBox *CMPUrlSourceSplitter_Protocol_Mshs::GetMovieBox(CMSHSSmoothStreamingMedia *media, CTrackFragmentHeaderBox *videoFragmentHeaderBox, CTrackFragmentHeaderBox *audioFragmentHeaderBox)
{
  CMovieBox *movieBox = NULL;
  bool continueCreating = ((media != NULL) && (videoFragmentHeaderBox != NULL) && (audioFragmentHeaderBox != NULL));

  if (continueCreating)
  {
    movieBox = new CMovieBox();
    continueCreating &= (movieBox != NULL);

    // add movie header box
    if (continueCreating)
    {
      CMovieHeaderBox *movieHeaderBox = new CMovieHeaderBox();
      continueCreating &= (movieHeaderBox != NULL);

      if (continueCreating)
      {
        // set time scale by manifest
        movieHeaderBox->SetTimeScale((uint32_t)media->GetTimeScale());

        // set next track ID to last used track ID + 1
        movieHeaderBox->SetNextTrackId(this->lastTrackID + 1);

        movieHeaderBox->GetRate()->SetIntegerPart(1);
        movieHeaderBox->GetVolume()->SetIntegerPart(1);

        if (continueCreating)
        {
          continueCreating &= movieBox->GetBoxes()->Add(movieHeaderBox);
        }
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(movieHeaderBox);
      }
    }

    unsigned int videoStreamIndex = 0;
    unsigned int audioStreamIndex = 0;
    unsigned int trackIndex = 0;

    for (unsigned int i = 0; i < media->GetStreams()->Count(); i++)
    {
      CMSHSStream *stream = media->GetStreams()->GetItem(i);

      if (stream->IsVideo())
      {
        videoStreamIndex = i;
        break;
      }
    }

    for (unsigned int i = 0; i < media->GetStreams()->Count(); i++)
    {
      CMSHSStream *stream = media->GetStreams()->GetItem(i);

      if (stream->IsAudio())
      {
        audioStreamIndex = i;
        break;
      }
    }

    // add track box (video or audio - depends on track ID)
    if (continueCreating)
    {
      CTrackBox *trackBox = 
        (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
        this->GetVideoTrackBox(media, videoStreamIndex, trackIndex, videoFragmentHeaderBox) : this->GetAudioTrackBox(media, audioStreamIndex, trackIndex, audioFragmentHeaderBox);
      continueCreating &= (trackBox != NULL);

      if (continueCreating)
      {
        continueCreating &= movieBox->GetBoxes()->Add(trackBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(trackBox);
      }
    }

    // add track box (video or audio - depends on track ID)
    if (continueCreating)
    {
      CTrackBox *trackBox = 
        (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
        this->GetAudioTrackBox(media, audioStreamIndex, trackIndex, audioFragmentHeaderBox) : this->GetVideoTrackBox(media, videoStreamIndex, trackIndex, videoFragmentHeaderBox);
      continueCreating &= (trackBox != NULL);

      if (continueCreating)
      {
        continueCreating &= movieBox->GetBoxes()->Add(trackBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(trackBox);
      }
    }

    // add movie extends box
    if (continueCreating)
    {
      CMovieExtendsBox *movieExtendsBox = new CMovieExtendsBox();
      continueCreating &= (movieExtendsBox != NULL);

      // add track extends box (video or audio - depends on track ID)
      if (continueCreating)
      {
        CTrackExtendsBox *trackExtendsBox = new CTrackExtendsBox();
        continueCreating &= (trackExtendsBox != NULL);

        if (continueCreating)
        {
          trackExtendsBox->SetTrackId(
            (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
            videoFragmentHeaderBox->GetTrackId() : audioFragmentHeaderBox->GetTrackId());

          trackExtendsBox->SetDefaultSampleDescriptionIndex(1);
        }

        if (continueCreating)
        {
          continueCreating &= movieExtendsBox->GetBoxes()->Add(trackExtendsBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(trackExtendsBox);
        }
      }

      // add track extends box (video or audio - depends on track ID)
      if (continueCreating)
      {
        CTrackExtendsBox *trackExtendsBox = new CTrackExtendsBox();
        continueCreating &= (trackExtendsBox != NULL);

        if (continueCreating)
        {
          trackExtendsBox->SetTrackId(
            (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
            audioFragmentHeaderBox->GetTrackId() : videoFragmentHeaderBox->GetTrackId());

          trackExtendsBox->SetDefaultSampleDescriptionIndex(1);
        }

        if (continueCreating)
        {
          continueCreating &= movieExtendsBox->GetBoxes()->Add(trackExtendsBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(trackExtendsBox);
        }
      }

      if (continueCreating)
      {
        continueCreating &= movieBox->GetBoxes()->Add(movieExtendsBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(movieExtendsBox);
      }
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(movieBox);
    }
  }

  return movieBox;
}

CTrackBox *CMPUrlSourceSplitter_Protocol_Mshs::GetVideoTrackBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  CTrackBox *trackBox = NULL;
  bool continueCreating = ((media != NULL) && (fragmentHeaderBox != NULL));

  if (continueCreating)
  {
    trackBox = new CTrackBox();
    continueCreating &= (trackBox != NULL);
  }

  CMSHSStream *stream = media->GetStreams()->GetItem(streamIndex);
  continueCreating &= (stream != NULL);

  CMSHSTrack *track = NULL;
  if (continueCreating)
  {
    track = stream->GetTracks()->GetItem(trackIndex);
  }
  continueCreating &= (track != NULL);

  // add track header box
  if (continueCreating)
  {
    CTrackHeaderBox *trackHeaderBox = new CTrackHeaderBox();
    continueCreating &= (trackHeaderBox != NULL);

    if (continueCreating)
    {
      // set flags, track ID, duration, width and height
      // set version to 1 (uint(64))
      trackHeaderBox->SetFlags(0x0000000F);
      trackHeaderBox->SetTrackId(fragmentHeaderBox->GetTrackId());
      trackHeaderBox->SetDuration(media->GetDuration());
      trackHeaderBox->SetVersion(1);

      trackHeaderBox->GetWidth()->SetIntegerPart(track->GetMaxWidth());
      trackHeaderBox->GetHeight()->SetIntegerPart(track->GetMaxHeight());
    }

    if (continueCreating)
    {
      continueCreating &= trackBox->GetBoxes()->Add(trackHeaderBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(trackHeaderBox);
    }
  }

  // add media box
  if (continueCreating)
  {
    CMediaBox *mediaBox = new CMediaBox();
    continueCreating &= (mediaBox != NULL);

    // add media header box
    if (continueCreating)
    {
      CMediaHeaderBox *mediaHeaderBox = new CMediaHeaderBox();
      continueCreating &= (mediaHeaderBox != NULL);

      if (continueCreating)
      {
        // set version (1 = uint(64)), time scale from manifest, duration
        mediaHeaderBox->SetVersion(1);
        mediaHeaderBox->SetTimeScale((uint32_t)media->GetTimeScale());
        mediaHeaderBox->SetDuration(media->GetDuration());
      }

      if (continueCreating)
      {
        continueCreating &= mediaBox->GetBoxes()->Add(mediaHeaderBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(mediaHeaderBox);
      }
    }

    // add handler box
    if (continueCreating)
    {
      CHandlerBox *handlerBox = this->GetHandlerBox(HANDLER_TYPE_VIDEO, L"VideoHandler");
      continueCreating &= (handlerBox != NULL);

      if (continueCreating)
      {
        continueCreating &= mediaBox->GetBoxes()->Add(handlerBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(handlerBox);
      }
    }

    // add media information box
    if (continueCreating)
    {
      CMediaInformationBox *mediaInformationBox = new CMediaInformationBox(HANDLER_TYPE_VIDEO);
      continueCreating &= (mediaInformationBox != NULL);

      // add video media header box
      if (continueCreating)
      {
        CVideoMediaHeaderBox *videoMediaHeaderBox = new CVideoMediaHeaderBox();
        continueCreating &= (videoMediaHeaderBox != NULL);

        if (continueCreating)
        {
          videoMediaHeaderBox->SetFlags(0x00000001);
        }

        if (continueCreating)
        {
          continueCreating &= mediaInformationBox->GetBoxes()->Add(videoMediaHeaderBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(videoMediaHeaderBox);
        }
      }

      // add data information box
      if (continueCreating)
      {
        CDataInformationBox *dataInformationBox = this->GetDataInformationBox();
        continueCreating &= (dataInformationBox != NULL);

        if (continueCreating)
        {
          continueCreating &= mediaInformationBox->GetBoxes()->Add(dataInformationBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(dataInformationBox);
        }
      }

      // add samle table box
      if (continueCreating)
      {
        CSampleTableBox *sampleTableBox = this->GetVideoSampleTableBox(media, streamIndex, trackIndex, fragmentHeaderBox);
        continueCreating &= (sampleTableBox != NULL);

        if (continueCreating)
        {
          continueCreating &= mediaInformationBox->GetBoxes()->Add(sampleTableBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(sampleTableBox);
        }
      }

      if (continueCreating)
      {
        continueCreating &= mediaBox->GetBoxes()->Add(mediaInformationBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(mediaInformationBox);
      }
    }

    if (continueCreating)
    {
      continueCreating &= trackBox->GetBoxes()->Add(mediaBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(mediaBox);
    }
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(trackBox);
  }

  return trackBox;
}

CTrackBox *CMPUrlSourceSplitter_Protocol_Mshs::GetAudioTrackBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  CTrackBox *trackBox = NULL;
  bool continueCreating = ((media != NULL) && (fragmentHeaderBox != NULL));

  if (continueCreating)
  {
    trackBox = new CTrackBox();
    continueCreating &= (trackBox != NULL);
  }

  // add track header box
  if (continueCreating)
  {
    CTrackHeaderBox *trackHeaderBox = new CTrackHeaderBox();
    continueCreating &= (trackHeaderBox != NULL);

    if (continueCreating)
    {
      // set flags, track ID, duration, width and height
      // set version to 1 (uint(64))
      trackHeaderBox->SetFlags(0x0000000F);
      trackHeaderBox->SetTrackId(fragmentHeaderBox->GetTrackId());
      trackHeaderBox->SetDuration(media->GetDuration());
      trackHeaderBox->SetVersion(1);
      trackHeaderBox->SetAlternateGroup(1);
      trackHeaderBox->GetVolume()->SetIntegerPart(1);
    }

    if (continueCreating)
    {
      continueCreating &= trackBox->GetBoxes()->Add(trackHeaderBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(trackHeaderBox);
    }
  }

  // add media box
  if (continueCreating)
  {
    CMediaBox *mediaBox = new CMediaBox();
    continueCreating &= (mediaBox != NULL);

    // add media header box
    if (continueCreating)
    {
      CMediaHeaderBox *mediaHeaderBox = new CMediaHeaderBox();
      continueCreating &= (mediaHeaderBox != NULL);

      if (continueCreating)
      {
        // set version (1 = uint(64)), time scale from manifest, duration
        mediaHeaderBox->SetVersion(1);
        mediaHeaderBox->SetTimeScale((uint32_t)media->GetTimeScale());
        mediaHeaderBox->SetDuration(media->GetDuration());
      }

      if (continueCreating)
      {
        continueCreating &= mediaBox->GetBoxes()->Add(mediaHeaderBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(mediaHeaderBox);
      }
    }

    // add handler box
    if (continueCreating)
    {
      CHandlerBox *handlerBox = this->GetHandlerBox(HANDLER_TYPE_AUDIO, L"SoundHandler");
      continueCreating &= (handlerBox != NULL);

      if (continueCreating)
      {
        continueCreating &= mediaBox->GetBoxes()->Add(handlerBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(handlerBox);
      }
    }

    // add media information box
    if (continueCreating)
    {
      CMediaInformationBox *mediaInformationBox = new CMediaInformationBox(HANDLER_TYPE_AUDIO);
      continueCreating &= (mediaInformationBox != NULL);

      // add sound media header box
      if (continueCreating)
      {
        CSoundMediaHeaderBox *soundMediaHeaderBox = new CSoundMediaHeaderBox();
        continueCreating &= (soundMediaHeaderBox != NULL);

        if (continueCreating)
        {
          continueCreating &= mediaInformationBox->GetBoxes()->Add(soundMediaHeaderBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(soundMediaHeaderBox);
        }
      }

      // add data information box
      if (continueCreating)
      {
        CDataInformationBox *dataInformationBox = this->GetDataInformationBox();
        continueCreating &= (dataInformationBox != NULL);

        if (continueCreating)
        {
          continueCreating &= mediaInformationBox->GetBoxes()->Add(dataInformationBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(dataInformationBox);
        }
      }

      // add samle table box
      if (continueCreating)
      {
        CSampleTableBox *sampleTableBox = this->GetAudioSampleTableBox(media, streamIndex, trackIndex, fragmentHeaderBox);
        continueCreating &= (sampleTableBox != NULL);

        if (continueCreating)
        {
          continueCreating &= mediaInformationBox->GetBoxes()->Add(sampleTableBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(sampleTableBox);
        }
      }

      if (continueCreating)
      {
        continueCreating &= mediaBox->GetBoxes()->Add(mediaInformationBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(mediaInformationBox);
      }
    }

    if (continueCreating)
    {
      continueCreating &= trackBox->GetBoxes()->Add(mediaBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(mediaBox);
    }
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(trackBox);
  }

  return trackBox;
}

CDataInformationBox *CMPUrlSourceSplitter_Protocol_Mshs::GetDataInformationBox(void)
{
  CDataInformationBox *dataInformationBox = new CDataInformationBox();
  bool continueCreating = (dataInformationBox != NULL);

  if (continueCreating)
  {
    // add data reference box
    if (continueCreating)
    {
      CDataReferenceBox *dataReferenceBox = new CDataReferenceBox();
      continueCreating &= (dataReferenceBox != NULL);

      if (continueCreating)
      {
        // add data entry url box
        if (continueCreating)
        {
          CDataEntryUrlBox *dataEntryUrlBox = new CDataEntryUrlBox();
          continueCreating &= (dataEntryUrlBox != NULL);

          if (continueCreating)
          {
            dataEntryUrlBox->SetSelfContained(true);
          }

          if (continueCreating)
          {
            continueCreating &= dataReferenceBox->GetDataEntryBoxCollection()->Add(dataEntryUrlBox);
          }

          if (!continueCreating)
          {
            FREE_MEM_CLASS(dataEntryUrlBox);
          }
        }
      }

      if (continueCreating)
      {
        continueCreating &= dataInformationBox->GetBoxes()->Add(dataReferenceBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(dataReferenceBox);
      }
    }
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(dataInformationBox);
  }

  return dataInformationBox;
}

CHandlerBox *CMPUrlSourceSplitter_Protocol_Mshs::GetHandlerBox(uint32_t handlerType, const wchar_t *handlerName)
{
  CHandlerBox *handlerBox = new CHandlerBox();
  bool continueCreating = (handlerBox != NULL);

  if (continueCreating)
  {
    handlerBox->SetHandlerType(handlerType);
    continueCreating &= handlerBox->SetName(handlerName);
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(handlerBox);
  }

  return handlerBox;
}

CSampleTableBox *CMPUrlSourceSplitter_Protocol_Mshs::GetVideoSampleTableBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  CSampleTableBox *sampleTableBox = new CSampleTableBox(HANDLER_TYPE_VIDEO);
  bool continueCreating = ((sampleTableBox != NULL) && (media != NULL));

  CMSHSStream *stream = media->GetStreams()->GetItem(streamIndex);
  continueCreating &= (stream != NULL);

  CMSHSTrack *track = NULL;
  if (continueCreating)
  {
    track = stream->GetTracks()->GetItem(trackIndex);
  }
  continueCreating &= (track != NULL);

  // add sample description box
  if (continueCreating)
  {
    CSampleDescriptionBox *sampleDescriptionBox = new CSampleDescriptionBox(HANDLER_TYPE_VIDEO);
    continueCreating &= (sampleDescriptionBox != NULL);

    // add visual sample entry
    if (continueCreating)
    {
      CVisualSampleEntryBox *visualSampleEntryBox = new CVisualSampleEntryBox();
      continueCreating &= (visualSampleEntryBox != NULL);

      if (continueCreating)
      {
        continueCreating &= visualSampleEntryBox->SetCodingName(L"avc1");
        visualSampleEntryBox->SetDataReferenceIndex(1);
        visualSampleEntryBox->GetHorizontalResolution()->SetIntegerPart(72);
        visualSampleEntryBox->GetVerticalResolution()->SetIntegerPart(72);
        visualSampleEntryBox->SetFrameCount(1);
        continueCreating &= visualSampleEntryBox->SetCompressorName(L"");
        visualSampleEntryBox->SetDepth(24);
        visualSampleEntryBox->SetWidth((uint16_t)track->GetMaxWidth());
        visualSampleEntryBox->SetHeight((uint16_t)track->GetMaxHeight());
      }

      // add AVC configuration box
      if (continueCreating)
      {
        CAVCConfigurationBox *avcConfigurationBox = new CAVCConfigurationBox();
        continueCreating &= (avcConfigurationBox != NULL);

        if (continueCreating)
        {
          char *codecPrivateData = ConvertToMultiByte(track->GetCodecPrivateData());
          continueCreating &= (codecPrivateData != NULL);

          if (continueCreating)
          {
            const char *spsStart = strstr(codecPrivateData, "00000001");
            continueCreating &= (spsStart != NULL);

            if (continueCreating)
            {
              spsStart += 8;

              const char *ppsStart = strstr(spsStart, "00000001");
              continueCreating &= (ppsStart != NULL);

              if (continueCreating)
              {
                ppsStart += 8;
                unsigned int ppsLength = strlen(ppsStart);
                unsigned int spsLength = strlen(spsStart) - ppsLength - 8;

                // we have SPS start and PPS start
                // parse data to AVC configuration box
                ALLOC_MEM_DEFINE_SET(sps, char, (spsLength + 1), 0);
                continueCreating &= (sps != NULL);

                if (continueCreating)
                {
                  memcpy(sps, spsStart, spsLength);

                  uint8_t *convertedSps = HexToDecA(sps);
                  uint8_t *convertedPps = HexToDecA(ppsStart);
                  continueCreating &= ((convertedSps != NULL) && (convertedPps != NULL));

                  if (continueCreating)
                  {
                    avcConfigurationBox->GetAVCDecoderConfiguration()->SetConfigurationVersion(1);
                    avcConfigurationBox->GetAVCDecoderConfiguration()->SetAvcProfileIndication(convertedSps[1]);
                    avcConfigurationBox->GetAVCDecoderConfiguration()->SetProfileCompatibility(convertedSps[2]);
                    avcConfigurationBox->GetAVCDecoderConfiguration()->SetAvcLevelIndication(convertedSps[3]);
                    avcConfigurationBox->GetAVCDecoderConfiguration()->SetLengthSizeMinusOne(3);
                  }

                  if (continueCreating)
                  {
                    CSequenceParameterSetNALUnit *spsUnit = new CSequenceParameterSetNALUnit();
                    continueCreating &= (spsUnit != NULL);

                    if (continueCreating)
                    {
                      continueCreating &= spsUnit->SetBuffer(convertedSps, spsLength / 2);
                    }

                    if (continueCreating)
                    {
                      continueCreating &= avcConfigurationBox->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Add(spsUnit);
                    }

                    if (!continueCreating)
                    {
                      FREE_MEM_CLASS(spsUnit);
                    }
                  }

                  if (continueCreating)
                  {
                    CPictureParameterSetNALUnit *ppsUnit = new CPictureParameterSetNALUnit();
                    continueCreating &= (ppsUnit != NULL);

                    if (continueCreating)
                    {
                      continueCreating &= ppsUnit->SetBuffer(convertedPps, ppsLength / 2);
                    }

                    if (continueCreating)
                    {
                      continueCreating &= avcConfigurationBox->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->Add(ppsUnit);
                    }

                    if (!continueCreating)
                    {
                      FREE_MEM_CLASS(ppsUnit);
                    }
                  }

                  FREE_MEM(convertedSps);
                  FREE_MEM(convertedPps);
                }
                FREE_MEM(sps);
              }
            }
          }
          FREE_MEM(codecPrivateData);
        }

        if (continueCreating)
        {
          continueCreating &= visualSampleEntryBox->GetBoxes()->Add(avcConfigurationBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(avcConfigurationBox);
        }
      }

      if (continueCreating)
      {
        continueCreating &= sampleDescriptionBox->GetSampleEntries()->Add(visualSampleEntryBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(visualSampleEntryBox);
      }
    }

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(sampleDescriptionBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(sampleDescriptionBox);
    }
  }

  // add time to sample box
  if (continueCreating)
  {
    CTimeToSampleBox *timeToSampleBox = new CTimeToSampleBox();
    continueCreating &= (timeToSampleBox != NULL);

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(timeToSampleBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(timeToSampleBox);
    }
  }

  // add sample to chunk box
  if (continueCreating)
  {
    CSampleToChunkBox *sampleToChunkBox = new CSampleToChunkBox();
    continueCreating &= (sampleToChunkBox != NULL);

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(sampleToChunkBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(sampleToChunkBox);
    }
  }

  // add chunk offset box
  if (continueCreating)
  {
    CChunkOffsetBox *chunkOffsetBox = new CChunkOffsetBox();
    continueCreating &= (chunkOffsetBox != NULL);

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(chunkOffsetBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(chunkOffsetBox);
    }
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(sampleTableBox);
  }

  return sampleTableBox;
}

CSampleTableBox *CMPUrlSourceSplitter_Protocol_Mshs::GetAudioSampleTableBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  CSampleTableBox *sampleTableBox = new CSampleTableBox(HANDLER_TYPE_AUDIO);
  bool continueCreating = ((sampleTableBox != NULL) && (media != NULL));

  CMSHSStream *stream = media->GetStreams()->GetItem(streamIndex);
  continueCreating &= (stream != NULL);

  CMSHSTrack *track = NULL;
  if (continueCreating)
  {
    track = stream->GetTracks()->GetItem(trackIndex);
  }
  continueCreating &= (track != NULL);

  // add sample description box
  if (continueCreating)
  {
    CSampleDescriptionBox *sampleDescriptionBox = new CSampleDescriptionBox(HANDLER_TYPE_AUDIO);
    continueCreating &= (sampleDescriptionBox != NULL);

    // add audio sample entry
    if (continueCreating)
    {
      CAudioSampleEntryBox *audioSampleEntryBox = new CAudioSampleEntryBox();
      continueCreating &= (audioSampleEntryBox != NULL);

      if (continueCreating)
      {
        continueCreating &= audioSampleEntryBox->SetCodingName(L"mp4a");
        audioSampleEntryBox->SetChannelCount(track->GetChannels());
        audioSampleEntryBox->SetSampleSize(track->GetBitsPerSample());
        audioSampleEntryBox->GetSampleRate()->SetIntegerPart(track->GetSamplingRate());
      }

      // add ESD box
      if (continueCreating)
      {
        CESDBox *esdBox = new CESDBox();
        continueCreating &= (esdBox != NULL);

        if (continueCreating)
        {
          uint32_t length = (track->GetCodecPrivateData() != NULL) ? wcslen(track->GetCodecPrivateData()) : 0;

          if (continueCreating)
          {
            esdBox->SetTrackId(fragmentHeaderBox->GetTrackId());
            esdBox->SetCodecTag(CODEC_TAG_AAC);
            esdBox->SetMaxBitrate(128000);

            if (length > 0)
            {
              uint8_t *convertedCodecPrivateData = HexToDecW(track->GetCodecPrivateData());
              continueCreating &= (convertedCodecPrivateData != NULL);

              if (continueCreating)
              {
                continueCreating &= esdBox->SetCodecPrivateData(convertedCodecPrivateData, length);
              }

              FREE_MEM(convertedCodecPrivateData);
            }
          }
        }

        if (continueCreating)
        {
          continueCreating &= audioSampleEntryBox->GetBoxes()->Add(esdBox);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(esdBox);
        }
      }

      if (continueCreating)
      {
        continueCreating &= sampleDescriptionBox->GetSampleEntries()->Add(audioSampleEntryBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(audioSampleEntryBox);
      }
    }

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(sampleDescriptionBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(sampleDescriptionBox);
    }
  }

  // add time to sample box
  if (continueCreating)
  {
    CTimeToSampleBox *timeToSampleBox = new CTimeToSampleBox();
    continueCreating &= (timeToSampleBox != NULL);

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(timeToSampleBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(timeToSampleBox);
    }
  }

  // add sample to chunk box
  if (continueCreating)
  {
    CSampleToChunkBox *sampleToChunkBox = new CSampleToChunkBox();
    continueCreating &= (sampleToChunkBox != NULL);

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(sampleToChunkBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(sampleToChunkBox);
    }
  }

  // add chunk offset box
  if (continueCreating)
  {
    CChunkOffsetBox *chunkOffsetBox = new CChunkOffsetBox();
    continueCreating &= (chunkOffsetBox != NULL);

    if (continueCreating)
    {
      continueCreating &= sampleTableBox->GetBoxes()->Add(chunkOffsetBox);
    }

    if (!continueCreating)
    {
      FREE_MEM_CLASS(chunkOffsetBox);
    }
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(sampleTableBox);
  }

  return sampleTableBox;
}

CFragmentedIndexBox *CMPUrlSourceSplitter_Protocol_Mshs::GetFragmentedIndexBox(CMSHSSmoothStreamingMedia *media, uint32_t videoTrackId, uint32_t audioTrackId, uint64_t timestamp)
{
  CFragmentedIndexBox *fragmentedIndexBox = NULL;
  bool continueCreating = (media != NULL);

  if (continueCreating)
  {
    fragmentedIndexBox = new CFragmentedIndexBox();
    continueCreating &= (fragmentedIndexBox != NULL);

    unsigned int videoStreamIndex = 0;
    unsigned int audioStreamIndex = 0;

    for (unsigned int i = 0; i < media->GetStreams()->Count(); i++)
    {
      CMSHSStream *stream = media->GetStreams()->GetItem(i);

      if (stream->IsVideo())
      {
        videoStreamIndex = i;
        break;
      }
    }

    for (unsigned int i = 0; i < media->GetStreams()->Count(); i++)
    {
      CMSHSStream *stream = media->GetStreams()->GetItem(i);

      if (stream->IsAudio())
      {
        audioStreamIndex = i;
        break;
      }
    }

    // add fragmented index track box (video or audio - depends on track ID)
    if (continueCreating)
    {
      CFragmentedIndexTrackBox *fragmentedIndexTrackBox = 
        (videoTrackId < audioTrackId) ? 
        this->GetFragmentedIndexTrackBox(media, videoStreamIndex, videoTrackId, timestamp) : this->GetFragmentedIndexTrackBox(media, audioStreamIndex, audioTrackId, timestamp);
      continueCreating &= (fragmentedIndexTrackBox != NULL);

      if (continueCreating)
      {
        continueCreating &= fragmentedIndexBox->GetBoxes()->Add(fragmentedIndexTrackBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(fragmentedIndexTrackBox);
      }
    }

    // add fragmented index track box (video or audio - depends on track ID)
    if (continueCreating)
    {
      CFragmentedIndexTrackBox *fragmentedIndexTrackBox = 
        (videoTrackId < audioTrackId) ? 
        this->GetFragmentedIndexTrackBox(media, audioStreamIndex, audioTrackId, timestamp) : this->GetFragmentedIndexTrackBox(media, videoStreamIndex, videoTrackId, timestamp);
      continueCreating &= (fragmentedIndexTrackBox != NULL);

      if (continueCreating)
      {
        continueCreating &= fragmentedIndexBox->GetBoxes()->Add(fragmentedIndexTrackBox);
      }

      if (!continueCreating)
      {
        FREE_MEM_CLASS(fragmentedIndexTrackBox);
      }
    }
  }

  return fragmentedIndexBox;
}

CFragmentedIndexTrackBox *CMPUrlSourceSplitter_Protocol_Mshs::GetFragmentedIndexTrackBox(CMSHSSmoothStreamingMedia *media, unsigned int streamIndex, uint32_t trackId, uint64_t timestamp)
{
  CFragmentedIndexTrackBox *fragmentedIndexTrackBox = NULL;
  bool continueCreating = (media != NULL);

  if (continueCreating)
  {
    fragmentedIndexTrackBox = new CFragmentedIndexTrackBox();
    continueCreating &= (fragmentedIndexTrackBox != NULL);
  }

  CMSHSStream *stream = media->GetStreams()->GetItem(streamIndex);
  continueCreating &= (stream != NULL);

  // add fragmented indexes for this track ID
  if (continueCreating)
  {
    fragmentedIndexTrackBox->SetTrackId(trackId);

    for (unsigned int i = 0; (continueCreating && (i < stream->GetStreamFragments()->Count())); i++)
    {
      CMSHSStreamFragment *streamFragment = stream->GetStreamFragments()->GetItem(i);

      if (streamFragment->GetFragmentTime() >= timestamp)
      {
        CFragmentedIndex *index = new CFragmentedIndex();
        continueCreating &= (index != NULL);

        if (continueCreating)
        {
          index->SetTimestamp(streamFragment->GetFragmentTime());
          index->SetDuration(streamFragment->GetFragmentDuration());
        }

        if (continueCreating)
        {
          continueCreating &= fragmentedIndexTrackBox->GetFragmentedIndexes()->Add(index);
        }

        if (!continueCreating)
        {
          FREE_MEM_CLASS(index);
        }

        break;
      }
    }
  }

  if (!continueCreating)
  {
    FREE_MEM_CLASS(fragmentedIndexTrackBox);
  }

  return fragmentedIndexTrackBox;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Mshs::GetStoreFile(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configurationParameters->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_mshs_%s.temp", folder, guid);
    }
    FREE_MEM(guid);
  }

  return result;
}

CLinearBuffer *CMPUrlSourceSplitter_Protocol_Mshs::FillBufferForProcessing(CStreamFragmentCollection *streamFragments, unsigned int streamFragmentProcessing, wchar_t *storeFile)
{
  CLinearBuffer *result = NULL;

  if (streamFragments != NULL)
  {
    if (streamFragmentProcessing < streamFragments->Count())
    {
      CStreamFragment *streamFragment = streamFragments->GetItem(streamFragmentProcessing);

      if (streamFragment->GetDownloaded())
      {
        // stream fragment is downloaded
        // stream fragment can be stored in memory or in store file

        // temporary buffer for data (from store file or from memory)
        unsigned int bufferLength = streamFragment->GetLength();
        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferLength, 0);

        if (buffer != NULL)
        {
          if ((streamFragment->IsStoredToFile()) && (storeFile != NULL))
          {
            // stream fragment is stored into file and store file is specified

            LARGE_INTEGER size;
            size.QuadPart = 0;

            // open or create file
            HANDLE hTempFile = CreateFile(storeFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

            if (hTempFile != INVALID_HANDLE_VALUE)
            {
              bool error = false;

              LONG distanceToMoveLow = (LONG)(streamFragment->GetStoreFilePosition());
              LONG distanceToMoveHigh = (LONG)(streamFragment->GetStoreFilePosition() >> 32);
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
          else if (!streamFragment->IsStoredToFile())
          {
            // stream fragment is stored in memory
            if (streamFragment->GetBuffer() != NULL)
            {
              if (streamFragment->GetBuffer()->CopyFromBuffer(buffer, bufferLength) != bufferLength)
              {
                // error occured while copying data
                FREE_MEM(buffer);
              }
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