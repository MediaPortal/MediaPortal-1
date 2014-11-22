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
#include "formatUrl.h"
#include "ErrorCodes.h"
#include "Utilities.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "MovieFragmentBox.h"
#include "TrackFragmentBox.h"
#include "MovieHeaderBox.h"
#include "MovieExtendsBox.h"
#include "TrackExtendsBox.h"
#include "TrackHeaderBox.h"
#include "MediaBox.h"
#include "MediaHeaderBox.h"
#include "MediaInformationBox.h"
#include "VideoMediaHeaderBox.h"
#include "DataReferenceBox.h"
#include "DataEntryUrlBox.h"
#include "SampleDescriptionBox.h"
#include "VisualSampleEntryBox.h"
#include "AVCConfigurationBox.h"
#include "SequenceParameterSetNALUnit.h"
#include "PictureParameterSetNALUnit.h"
#include "TimeToSampleBox.h"
#include "SampleToChunkBox.h"
#include "ChunkOffsetBox.h"
#include "SoundMediaHeaderBox.h"
#include "AudioSampleEntryBox.h"
#include "ESDBox.h"
#include "TrackRunBox.h"
#include "BoxConstants.h"

#include "base64.h"
#include "compress_zlib.h"
#include "conversions.h"

#include <Shlwapi.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Mshsd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Mshs"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Mshs(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Mshs *protocol = (CMPUrlSourceSplitter_Protocol_Mshs *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Mshs::CMPUrlSourceSplitter_Protocol_Mshs(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->lockCurlMutex = NULL;
  this->lockMutex = NULL;
  this->mainCurlInstance = NULL;
  this->streamLength = 0;
  this->streamFragments = NULL;
  this->cacheFile = NULL;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;
  this->streamingMediaBox = NULL;
  this->videoTrackFragmentHeaderBox = NULL;
  this->audioTrackFragmentHeaderBox = NULL;
  this->lastTrackID = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;
  this->fragmentedIndexBox = NULL;
  this->fragmentedIndexBoxSize = 0;
  this->reconstructedHeaderSize = 0;
  
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->streamFragments = new CMshsStreamFragmentCollection(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);

    // create CURL instance
    this->mainCurlInstance = new CMshsCurlInstance(result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
    CHECK_POINTER_HRESULT(*result, this->mainCurlInstance, *result, E_OUTOFMEMORY);

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

    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Protocol_Mshs::~CMPUrlSourceSplitter_Protocol_Mshs()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->streamFragments);
  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->streamingMediaBox);
  FREE_MEM_CLASS(this->audioTrackFragmentHeaderBox);
  FREE_MEM_CLASS(this->videoTrackFragmentHeaderBox);
  FREE_MEM_CLASS(this->fragmentedIndexBox);

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

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Mshs::GetConnectionState(void)
{
  return this->mainCurlInstance->GetConnectionState();
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::ReceiveData(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    CLockMutex lock(this->lockMutex, INFINITE);

    if (SUCCEEDED(result) && (this->mainCurlInstance->IsLockedCurlInstanceByOwner(this)) && (this->mainCurlInstance->GetConnectionState() == Opening))
    {
      unsigned int bufferSize = 0;
      {
        // only check received data length to not block Load() method
        CLockMutex lockData(this->lockCurlMutex, INFINITE);

        bufferSize = this->mainCurlInstance->GetMshsDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
      }

      if (bufferSize > 0)
      {
        this->mainCurlInstance->SetConnectionState(Opened);
      }
    }

    if (SUCCEEDED(result) && (this->streamFragments->HasReadyForProcessingStreamFragments()))
    {
      // some segment fragments are ready for processing
      CIndexedMshsStreamFragmentCollection *indexedReadyForProcessingStreamFragments = new CIndexedMshsStreamFragmentCollection(&result);
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->streamFragments->GetReadyForProcessingStreamFragments(indexedReadyForProcessingStreamFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedReadyForProcessingStreamFragments->Count())); i++)
      {
        CIndexedMshsStreamFragment *indexedReadyForProcessingStreamFragment = indexedReadyForProcessingStreamFragments->GetItem(i);
        CMshsStreamFragment *currentProcessingFragment = indexedReadyForProcessingStreamFragment->GetItem();

        // we can process stream fragment
        // after processing we mark fragment as downloaded = it is ready for filter

        if (SUCCEEDED(result) && (this->videoTrackFragmentHeaderBox == NULL) && currentProcessingFragment->IsVideo())
        {
          this->videoTrackFragmentHeaderBox = new CTrackFragmentHeaderBox(&result);
          CHECK_POINTER_HRESULT(result, this->videoTrackFragmentHeaderBox, result, E_OUTOFMEMORY);

          result = this->GetTrackFragmentHeaderBox(this->videoTrackFragmentHeaderBox, currentProcessingFragment, ++this->lastTrackID);

          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->videoTrackFragmentHeaderBox));
        }

        if (SUCCEEDED(result) && (this->audioTrackFragmentHeaderBox == NULL) && currentProcessingFragment->IsAudio())
        {
          this->audioTrackFragmentHeaderBox = new CTrackFragmentHeaderBox(&result);
          CHECK_POINTER_HRESULT(result, this->audioTrackFragmentHeaderBox, result, E_OUTOFMEMORY);

          result = this->GetTrackFragmentHeaderBox(this->audioTrackFragmentHeaderBox, currentProcessingFragment, ++this->lastTrackID);

          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(this->audioTrackFragmentHeaderBox));
        }

        if (SUCCEEDED(result) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_RECONSTRUCTED_HEADER)) && (this->videoTrackFragmentHeaderBox != NULL) && (this->audioTrackFragmentHeaderBox != NULL))
        {
          // for reconstruction of stream we need first video and first audio fragment
          // we need to reconstruct file header to be able to play fragmented video

          CLinearBuffer *fileHeader = new CLinearBuffer(&result);
          CHECK_POINTER_HRESULT(result, fileHeader, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, fileHeader->InitializeBuffer(MINIMUM_RECEIVED_DATA_FOR_SPLITTER), result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            /*wchar_t *videoData = this->videoTrackFragmentHeaderBox->GetParsedHumanReadable(L"");
            wchar_t *audioData = this->audioTrackFragmentHeaderBox->GetParsedHumanReadable(L"");

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: video track fragment header box:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, videoData);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: audio track fragment header box:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, audioData);

            FREE_MEM(videoData);
            FREE_MEM(audioData);*/

            // create file type box
            CFileTypeBox *fileTypeBox = new CFileTypeBox(&result);
            CHECK_POINTER_HRESULT(result, fileTypeBox, result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->GetFileTypeBox(fileTypeBox));
            CHECK_CONDITION_HRESULT(result, this->AddBoxIntoBuffer(fileTypeBox, fileHeader), result, E_OUTOFMEMORY);

            FREE_MEM_CLASS(fileTypeBox);

            // create movie box
            CMovieBox *movieBox = new CMovieBox(&result);
            CHECK_POINTER_HRESULT(result, movieBox, result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->GetMovieBox(movieBox, this->streamingMediaBox, this->videoTrackFragmentHeaderBox, this->audioTrackFragmentHeaderBox));
            CHECK_CONDITION_HRESULT(result, this->AddBoxIntoBuffer(movieBox, fileHeader), result, E_OUTOFMEMORY);

            FREE_MEM_CLASS(movieBox);
          }

          // if reconstructing is correct, put all data to first stream fragment
          if (SUCCEEDED(result))
          {
            this->reconstructedHeaderSize = fileHeader->GetBufferOccupiedSpace();

            CMshsStreamFragment *fragment = this->streamFragments->GetItem(0);

            CHECK_CONDITION_HRESULT(result, fileHeader->AddToBufferWithResize(fragment->GetBuffer()) == fragment->GetBuffer()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              fragment->SetFlags(fragment->GetFlags() | MSHS_STREAM_FRAGMENT_FLAG_CONTAINS_RECONSTRUCTED_HEADER);
              fragment->GetBuffer()->DeleteBuffer();

              CHECK_CONDITION_HRESULT(result, fragment->GetBuffer()->InitializeBuffer(fileHeader->GetBufferOccupiedSpace()), result, E_OUTOFMEMORY);
              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), fragment->GetBuffer()->AddToBufferWithResize(fileHeader));
            }
          }

          if (SUCCEEDED(result))
          {
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_RECONSTRUCTED_HEADER;

            // break current loop
            // in next run we finally process stream fragments which are ready for processing
            break;
          }
        }

        if (SUCCEEDED(result) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_RECONSTRUCTED_HEADER)) && (this->videoTrackFragmentHeaderBox != NULL) && (this->audioTrackFragmentHeaderBox != NULL))
        {
          // common processing of received stream fragments

          // process stream fragment data
          // in track fragment header box we need to set track ID
          // in track run box we need to set flags and data offset

          unsigned int fragmentSize = currentProcessingFragment->GetBuffer()->GetBufferOccupiedSpace();
          unsigned int fragmentPosition = 0;

          CLinearBuffer *processedFragment = new CLinearBuffer(&result, fragmentSize);
          CHECK_POINTER_HRESULT(result, processedFragment, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, fragmentSize != 0, result, E_MSHS_INVALID_FRAGMENT_SIZE);

          if (SUCCEEDED(result))
          {
            ALLOC_MEM_DEFINE_SET(fragmentBuffer, uint8_t, fragmentSize, 0);
            CHECK_POINTER_HRESULT(result, fragmentBuffer, result, E_OUTOFMEMORY);

            CBoxFactory *boxFactory = new CBoxFactory(&result);
            CHECK_POINTER_HRESULT(result, boxFactory, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              currentProcessingFragment->GetBuffer()->CopyFromBuffer(fragmentBuffer, fragmentSize);

              unsigned int movieFragmentBoxSize = 0;
              CTrackRunBox *trackRunBox = NULL;

              while (SUCCEEDED(result) && (fragmentPosition < fragmentSize))
              {
                CBox *box = boxFactory->CreateBox(fragmentBuffer + fragmentPosition, fragmentSize - fragmentPosition);
                CHECK_POINTER_HRESULT(result, box, result, E_MSHS_INVALID_BOX);
                HRESULT res = S_OK;

                if (SUCCEEDED(result))
                {
                  if (box->IsType(MOVIE_FRAGMENT_BOX_TYPE))
                  {
                    fragmentPosition += (unsigned int)box->GetSize();

                    // movie fragment box, it is root box for ther boxes
                    CMovieFragmentBox *movieFragmentBox = dynamic_cast<CMovieFragmentBox *>(box);

                    // reset its size to get correct size after changes
                    movieFragmentBox->ResetSize();

                    for (unsigned int j = 0; (SUCCEEDED(result) && (j < movieFragmentBox->GetBoxes()->Count())); j++)
                    {
                      CBox *innerMovieFragmentBox = movieFragmentBox->GetBoxes()->GetItem(j);
                      // reset its size to get correct size after changes
                      innerMovieFragmentBox->ResetSize();

                      if (innerMovieFragmentBox->IsType(TRACK_FRAGMENT_BOX_TYPE))
                      {
                        CTrackFragmentBox *trackFragmentBox = dynamic_cast<CTrackFragmentBox *>(innerMovieFragmentBox);

                        for (unsigned int k = 0; (SUCCEEDED(result) && (k < trackFragmentBox->GetBoxes()->Count())); k++)
                        {
                          CBox *innerTrackFragmentBox = trackFragmentBox->GetBoxes()->GetItem(k);
                          innerTrackFragmentBox->ResetSize();

                          if (innerTrackFragmentBox->IsType(TRACK_FRAGMENT_HEADER_BOX_TYPE))
                          {
                            // replace track ID in track fragment header with track ID in corresponding track fragment header (video, audio)
                            // remove base data offset flag from track fragment header box
                            CTrackFragmentHeaderBox *trackFragmentHeaderBox = dynamic_cast<CTrackFragmentHeaderBox *>(innerTrackFragmentBox);

                            trackFragmentHeaderBox->SetBoxFlags(trackFragmentHeaderBox->GetBoxFlags() & (~FLAGS_BASE_DATA_OFFSET_PRESENT));
                            trackFragmentHeaderBox->SetTrackId((currentProcessingFragment->IsVideo()) ? this->videoTrackFragmentHeaderBox->GetTrackId() : this->audioTrackFragmentHeaderBox->GetTrackId());
                          }

                          if (innerTrackFragmentBox->IsType(TRACK_RUN_BOX_TYPE))
                          {
                            trackRunBox = dynamic_cast<CTrackRunBox *>(innerTrackFragmentBox);

                            // add to flags that we specify data offset present
                            // we can't now set data offset, because not all boxes are processed

                            trackRunBox->SetBoxFlags(trackRunBox->GetBoxFlags() | FLAGS_DATA_OFFSET_PRESENT);
                          }
                        }
                      }
                    }
                    
                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), movieFragmentBoxSize = (unsigned int)movieFragmentBox->GetSize());
                    CHECK_CONDITION_HRESULT(result, movieFragmentBoxSize != 0, result, E_MSHS_INVALID_MOVIE_FRAGMENT_BOX);
                    
                    if (SUCCEEDED(result) && (movieFragmentBoxSize != 0) && (trackRunBox != NULL))
                    {
                      trackRunBox->SetDataOffset(movieFragmentBoxSize + BOX_HEADER_LENGTH);
                    }

                    CHECK_CONDITION_HRESULT(result, this->AddBoxIntoBuffer(box, processedFragment), result, E_OUTOFMEMORY);
                  }
                  else
                  {
                    // add data directly into processed fragment
                    CHECK_CONDITION_HRESULT(result, processedFragment->AddToBufferWithResize(fragmentBuffer + fragmentPosition, (unsigned int)box->GetSize()) == (unsigned int)box->GetSize(), result, E_OUTOFMEMORY);
                    fragmentPosition += (unsigned int)box->GetSize();
                  }
                }
                
                FREE_MEM_CLASS(box);
              }
            }

            FREE_MEM_CLASS(boxFactory);
            FREE_MEM(fragmentBuffer);
          }

          if (SUCCEEDED(result))
          {
            // we have processed fragment, store it to stream fragment
            currentProcessingFragment->GetBuffer()->DeleteBuffer();
            CHECK_CONDITION_HRESULT(result, currentProcessingFragment->GetBuffer()->InitializeBuffer(processedFragment->GetBufferOccupiedSpace()), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), currentProcessingFragment->GetBuffer()->AddToBufferWithResize(processedFragment));
          }

          FREE_MEM_CLASS(processedFragment);

          // update current processing stream fragment flags and indexes
          currentProcessingFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
          currentProcessingFragment->SetProcessed(true, UINT_MAX);

          this->streamFragments->UpdateIndexes(indexedReadyForProcessingStreamFragment->GetItemIndex(), 1);

          // recalculate start position of all processed stream fragments until first not processed stream fragment
          this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(indexedReadyForProcessingStreamFragment->GetItemIndex());
        }
      }

      FREE_MEM_CLASS(indexedReadyForProcessingStreamFragments);

      // check if we downloaded last segment fragment - then we set end of stream reached - it doesn't mean that we have all data (no gaps)

      if (SUCCEEDED(result) && (!this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED)) && (this->streamFragments->Count() != 0))
      {
        // not set end of stream, at least one stream fragment
        CMshsStreamFragment *lastFragment = this->streamFragments->GetItem(this->streamFragments->Count() - 1);

        if (lastFragment->IsDownloaded())
        {
          this->logger->Log(LOGGER_INFO, L"%s: %s: end of stream reached", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
          this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

          if (this->streamFragmentToDownload == UINT_MAX)
          {
            // no segment fragment to download, we have all data
            this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
          }
        }
      }
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance->GetConnectionState() == Initializing) && (!this->IsWholeStreamDownloaded()) && (!this->mainCurlInstance->IsLockedCurlInstance()))
    {
      if (this->streamFragments->Count() == 0)
      {
        char *encoded = ConvertToMultiByteW(this->configuration->GetValue(PARAMETER_NAME_MSHS_MANIFEST, true, NULL));
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
            FREE_MEM_CLASS(this->streamingMediaBox);

            this->streamingMediaBox = new CMshsManifestSmoothStreamingMediaBox(&result);
            CHECK_POINTER_HRESULT(result, this->streamingMediaBox, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              CHECK_CONDITION_HRESULT(result, this->streamingMediaBox->Parse(decompressedManifest, decompressedLength), result, E_MSHS_CANNOT_PARSE_STREAMING_MEDIA_BOX);

              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->GetStreamFragmentsFromManifest(this->streamFragments, this->streamingMediaBox));
            }

            CHECK_CONDITION_EXECUTE(FAILED(result), this->streamingMediaBox);
          }
          else
          {
            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot decompress manifest");
          }

          FREE_MEM(decompressedManifest);
        }
        else
        {
          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot decode manifest");
        }

        FREE_MEM(compressedManifestDecoded);
        FREE_MEM(encoded);

        // for reconstruction of stream we need first video and first audio fragment
        // check if first video stream fragment and first audio stream fragment are downloaded
        // in another case schedule download of what is missing (prefer video)

        if (SUCCEEDED(result) && (this->videoTrackFragmentHeaderBox == NULL) && (this->streamFragmentToDownload == UINT_MAX))
        {
          unsigned int i = 0;
          while (i < this->streamFragments->Count())
          {
            CMshsStreamFragment *fragment = this->streamFragments->GetItem(i);

            if (fragment->IsVideo())
            {
              this->streamFragmentToDownload = i;
              break;
            }

            i++;
          }
        }
      }
      
      // if not set stream fragment to download, then set stream fragment to download (get next not downloaded stream fragment after current processed stream fragment)
      this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentProcessing) : this->streamFragmentToDownload;
      // if not set stream fragment to download, then set stream fragment to download (get next not downloaded stream fragment from first stream fragment)
      this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0) : this->streamFragmentToDownload;
      // stream fragment to download still can be UINT_MAX = no stream fragment to download

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

      if (SUCCEEDED(result))
      {
        if (this->configuration->GetValueBool(PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA, true, PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA_DEFAULT))
        {
          wchar_t *storeFilePath = this->GetDumpFile();
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }
      }

      if (SUCCEEDED(result) && (this->streamFragmentToDownload != UINT_MAX))
      {
        CMshsDownloadRequest *request = new CMshsDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MSHS_OPEN_CONNECTION_TIMEOUT, true, MSHS_OPEN_CONNECTION_TIMEOUT_DEFAULT));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

          CMshsStreamFragment *fragment = this->streamFragments->GetItem(this->streamFragmentToDownload);
          // clear fragment buffer
          fragment->GetBuffer()->ClearBuffer();

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: starting receiving data, timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragmentTimestamp());

          CHECK_CONDITION_HRESULT(result, request->SetUrl(fragment->GetUrl()), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_MSHS_COOKIE, true, NULL)), result, E_OUTOFMEMORY);
          request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_MSHS_VERSION, true, HTTP_VERSION_DEFAULT));
          request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_MSHS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
          CHECK_CONDITION_HRESULT(result, request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_MSHS_REFERER, true, NULL)), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_MSHS_USER_AGENT, true, NULL)), result, E_OUTOFMEMORY);

          if (SUCCEEDED(this->mainCurlInstance->LockCurlInstance(this)))
          {
            // apply cookies

            unsigned int cookiesCount = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MSHS_COOKIES_COUNT, true, 0);

            if (cookiesCount != 0)
            {
              CParameterCollection *cookies = new CParameterCollection(&result);
              CHECK_POINTER_HRESULT(result, cookies, result, E_OUTOFMEMORY);

              for (unsigned int i = 0; (SUCCEEDED(result) && (i < cookiesCount)); i++)
              {
                wchar_t *httpCookieName = FormatString(MSHS_COOKIE_FORMAT_PARAMETER_NAME, i);
                CHECK_POINTER_HRESULT(result, httpCookieName, result, E_OUTOFMEMORY);

                if (SUCCEEDED(result))
                {
                  const wchar_t *cookieValue = this->configuration->GetValue(httpCookieName, true, NULL);
                  CHECK_POINTER_HRESULT(result, cookieValue, result, E_OUTOFMEMORY);

                  CHECK_CONDITION_HRESULT(result, cookies->Add(L"", cookieValue), result, E_OUTOFMEMORY);
                }

                FREE_MEM(httpCookieName);
              }

              CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->AddCookies(cookies), result, E_OUTOFMEMORY);
              FREE_MEM_CLASS(cookies);

              // clear set cookies to avoid adding same cookies
              for (unsigned int i = 0; (SUCCEEDED(result) && (i < cookiesCount)); i++)
              {
                wchar_t *httpCookieName = FormatString(MSHS_COOKIE_FORMAT_PARAMETER_NAME, i);
                CHECK_POINTER_HRESULT(result, httpCookieName, result, E_OUTOFMEMORY);

                CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->configuration->Remove(httpCookieName, true));
                FREE_MEM(httpCookieName);
              }

              this->configuration->Remove(PARAMETER_NAME_MSHS_COOKIES_COUNT, true);
            }

            if (SUCCEEDED(this->mainCurlInstance->Initialize(request)))
            {
              // all parameters set
              // start receiving data

              if (SUCCEEDED(this->mainCurlInstance->StartReceivingData()))
              {
                this->mainCurlInstance->SetConnectionState(Opening);

                this->streamFragmentDownloading = this->streamFragmentToDownload;
                this->streamFragmentToDownload = UINT_MAX;
              }
              else
              {
                this->mainCurlInstance->SetConnectionState(OpeningFailed);

                // we must unlock CURL instance, because we don't use it more
                this->mainCurlInstance->UnlockCurlInstance(this);
              }
            }
            else
            {
              this->mainCurlInstance->SetConnectionState(InitializeFailed);

              // we must unlock CURL instance, because we don't use it more
              this->mainCurlInstance->UnlockCurlInstance(this);
            }
          }
          else
          {
            this->mainCurlInstance->SetConnectionState(InitializeFailed);
            this->logger->Log(LOGGER_WARNING, L"%s: %s: cannot lock CURL instance, owner: 0x%p, lock count: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetOwner(), this->mainCurlInstance->GetOwnerLockCount());
          }
        }

        FREE_MEM_CLASS(request);
      }
    }

    if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()) && (this->mainCurlInstance->IsLockedCurlInstanceByOwner(this)) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    {
      // all data received, we're not receiving data
      // check end of stream or error on MSHS connection

      if (SUCCEEDED(this->mainCurlInstance->GetMshsDownloadResponse()->GetResultError()))
      {
        CMshsStreamFragment *currentDownloadingFragment = this->streamFragments->GetItem(this->streamFragmentDownloading);

        CHECK_CONDITION_HRESULT(result, currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetMshsDownloadResponse()->GetReceivedData()) == this->mainCurlInstance->GetMshsDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), currentDownloadingFragment->SetDownloaded(true, this->streamFragmentDownloading));
        
        // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentDownloading + 1) : this->streamFragmentToDownload;
        // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
        this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0) : this->streamFragmentToDownload;
        // fragment to download still can be UINT_MAX = no fragment to download

        // we are not downloading any stream fragment
        this->streamFragmentDownloading = UINT_MAX;

        this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE;
        this->flags |= (this->streamFragmentToDownload != UINT_MAX) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA : (PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED);
      }
      else
      {
        // error occured while receiving data
        // we can re-open connection exactly on lost position (time)

        // download stream fragment again or download scheduled stream fragment
        this->streamFragmentToDownload = this->streamFragmentDownloading;
        this->streamFragmentDownloading = UINT_MAX;

        // error while receiving data, stop receiving data
        this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA;
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
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting guess total length: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
      }
      else if ((this->GetBytePosition() > (this->streamLength * 3 / 4)))
      {
        // it is time to adjust stream length, we are approaching to end but still we don't know total length
        this->streamLength = this->GetBytePosition() * 2;
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting guess total length: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
      }
    }

    if ((!this->IsSetStreamLength()) && (this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen()))
    {
      // reached end of stream, set stream length

      this->streamLength = this->GetBytePosition();
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);

      this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
      this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
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
        // first try to find starting segment fragment (segment fragment which have first data)
        unsigned int foundDataLength = dataResponse->GetBuffer()->GetBufferOccupiedSpace();

        int64_t startPosition = this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_SKIP_HEADER) ? this->reconstructedHeaderSize : 0;
        startPosition += dataRequest->GetStart() + foundDataLength;

        if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_SEND_FRAGMENTED_INDEX) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_FRAGMENTED_INDEX_SENT)))
        {
          // we need to send fragmented index (because seek happen), but it hasn't been send yet
          // add fragmented index box into response

          ALLOC_MEM_DEFINE_SET(buffer, uint8_t, this->fragmentedIndexBoxSize, 0);
          CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->fragmentedIndexBox->GetBox(buffer, this->fragmentedIndexBoxSize), result, E_OUTOFMEMORY);
          dataResponse->GetBuffer()->AddToBuffer(buffer, this->fragmentedIndexBoxSize);
          FREE_MEM(buffer);

          foundDataLength += this->fragmentedIndexBoxSize;
          startPosition += this->fragmentedIndexBoxSize;

          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_FRAGMENTED_INDEX_SENT;
        }

        if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_FRAGMENTED_INDEX_SENT))
        {
          startPosition -= this->fragmentedIndexBoxSize;
        }

        unsigned int fragmentIndex = this->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);

        while (fragmentIndex != UINT_MAX)
        {
          this->streamFragmentProcessing = fragmentIndex;

          // get stream fragment
          CMshsStreamFragment *streamFragment = this->streamFragments->GetItem(fragmentIndex);
          CMshsStreamFragment *startSearchingStreamFragment = this->streamFragments->GetItem(this->streamFragments->GetStartSearchingIndex());

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
            // do not find any more stream fragments for this request because we have enough data
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
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

              dataResponse->SetConnectionLostCannotReopen(true);
            }

            if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= this->streamLength))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

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
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

              dataResponse->SetConnectionLostCannotReopen(true);
              streamPackage->SetCompleted(S_OK);
            }
            else if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= this->streamLength))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

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
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

              dataResponse->SetConnectionLostCannotReopen(true);
              streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_CONNECTION_LOST_CANNOT_REOPEN);
            }
            else if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= this->streamLength))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

              dataResponse->SetNoMoreDataAvailable(true);
              streamPackage->SetCompleted((dataResponse->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_NO_MORE_DATA_AVAILABLE);
            }
            //else if (this->IsLiveStreamDetected() && (this->connectionState != Opened))
            //{
            //  // we have live stream, we are missing data and we have not opened connection
            //  // we lost some data, report discontinuity

            //  dataResponse->SetDiscontinuity(true);
            //  streamPackage->SetCompleted(S_OK);
            //}
          }

          if (streamPackage->GetState() == CStreamPackage::Waiting)
          {
            // no seeking by position is available
            // requested position is probably in stream fragment on the end of searchable stream fragments, between this->streamFragments->GetStartSearchingIndex() and this->streamFragments->GetSearchCount()
            // check if fragment is downloading
            // if fragment is not downloading, then schedule it for download

            unsigned int fragmentIndex = this->streamFragments->GetStartSearchingIndex() + this->streamFragments->GetSearchCount();
            CMshsStreamFragment *fragment = this->streamFragments->GetItem(fragmentIndex);

            if ((!this->IsLiveStreamDetected()) && (fragment == NULL))
            {
              // bad, no such fragment exists, we don't have data
              // if live stream, we must update stream fragments

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', not found stream fragment", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
            }

            if ((fragment != NULL) && (!fragment->IsDownloaded()) && (fragmentIndex != this->streamFragmentDownloading) && (fragmentIndex != this->streamFragmentToDownload) && ((this->mainCurlInstance->GetConnectionState() == None) || (this->mainCurlInstance->GetConnectionState() == Opened)))
            {
              // fragment is not downloaded and also is not downloading currently
              this->streamFragmentDownloading = UINT_MAX;
              this->streamFragmentToDownload = fragmentIndex;

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', stream fragment not downloaded and not downloading, scheduled for download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              if (this->mainCurlInstance->GetConnectionState() == Opened)
              {
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA;
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

    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE) && (this->mainCurlInstance->IsLockedCurlInstanceByOwner(this) || (!this->mainCurlInstance->IsLockedCurlInstance())))
    {
      HRESULT res = S_OK;
      this->mainCurlInstance->SetConnectionState(Closing);

      if (this->mainCurlInstance != NULL)
      {
        res = this->mainCurlInstance->StopReceivingData();

        CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_INFO, L"%s: %s: closing connection failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, res));
        CHECK_CONDITION_EXECUTE(SUCCEEDED(res), this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->ClearBuffer());
      }

      if ((res == S_OK) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA)))
      {
        // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
        this->StopReceivingData();
      }

      if (res == S_OK)
      {
        this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"connection closed successfully");
        this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA);

        // if CURL instance is locked, then we unlock CURL instance
        this->mainCurlInstance->UnlockCurlInstance(this);
      }
    }

    // store stream fragments to temporary file
    if ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
    {
      this->lastStoreTime = GetTickCount();
      if (this->currentProcessedSize != 0)
      {
        this->lastProcessedSize = this->currentProcessedSize;
      }
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
            CMshsStreamFragment *fragment = this->streamFragments->GetItem(fragmentRemoveStart + fragmentRemoveCount);

            if (((fragmentRemoveStart + fragmentRemoveCount) != this->streamFragments->GetStartSearchingIndex()) && fragment->IsProcessed() && (fragment->GetFragmentTimestamp()  < (int64_t)this->reportedStreamTime))
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
          wchar_t *storeFilePath = this->GetStoreFile();
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }

        // store all segment fragments (which are not stored) to file
        if ((this->cacheFile->GetCacheFile() != NULL) && (this->streamFragments->Count() != 0) && (this->streamFragments->GetLoadedToMemorySize() > CACHE_FILE_RELOAD_SIZE))
        {
          this->cacheFile->StoreItems(this->streamFragments, this->lastStoreTime, false, this->IsWholeStreamDownloaded());
        }
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Mshs::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MSHS_OPEN_CONNECTION_TIMEOUT, true, MSHS_OPEN_CONNECTION_TIMEOUT_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Mshs::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MSHS_OPEN_CONNECTION_SLEEP_TIME, true, MSHS_OPEN_CONNECTION_SLEEP_TIME_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Mshs::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MSHS_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, MSHS_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT);
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configuration);

  if (SUCCEEDED(result) && (parameters != NULL))
  {
    this->configuration->Append(parameters);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), this->StopReceivingData());

  this->mainCurlInstance->SetConnectionState(SUCCEEDED(result) ? Initializing : None);

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->mainCurlInstance->StopReceivingData());
  this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA);

  this->mainCurlInstance->SetConnectionState(None);

  this->streamFragmentDownloading = UINT_MAX;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

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
  }

  return result;
}

void CMPUrlSourceSplitter_Protocol_Mshs::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  __super::ClearSession();
 
  this->streamLength = 0;
  this->mainCurlInstance->SetConnectionState(None);
  this->cacheFile->Clear();
  this->streamFragments->Clear();
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;
  FREE_MEM_CLASS(this->streamingMediaBox);
  FREE_MEM_CLASS(this->audioTrackFragmentHeaderBox);
  FREE_MEM_CLASS(this->videoTrackFragmentHeaderBox);
  FREE_MEM_CLASS(this->fragmentedIndexBox);
  this->lastTrackID = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;
  this->fragmentedIndexBoxSize = 0;
  this->reconstructedHeaderSize = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_Mshs::GetDuration(void)
{
  return this->IsLiveStream() ? DURATION_LIVE_STREAM : DURATION_UNSPECIFIED;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetStreamInformation(CStreamInformationCollection *streams)
{
  // MSHS protocol has always one stream (container)
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

unsigned int CMPUrlSourceSplitter_Protocol_Mshs::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_Mshs::SeekToTime(unsigned int streamId, int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  // MSHS protocol can seek to ms
  // time is in ms

  int64_t streamingMediaTime = time * this->streamingMediaBox->GetTimeScale() / 1000;

  // find stream fragment to process
  // TO DO: implement better and faster seeking algorithm
  for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
  {
    // first MSHS stream fragment has always zero timestamp
    // seeking is always on video stream, we need to find video fragment

    CMshsStreamFragment *fragment = this->streamFragments->GetItem(i);
    CMshsStreamFragment *nextFragment = this->streamFragments->GetItem(i + 1);

    int64_t fragmentTime = fragment->GetFragmentTimestamp();
    int64_t nextFragmentTime = (nextFragment == NULL) ? INT64_MAX : nextFragment->GetFragmentTimestamp();

    if ((fragment->IsVideo()) && ((fragmentTime >= streamingMediaTime) || ((fragmentTime <= streamingMediaTime) && (nextFragmentTime >= streamingMediaTime))))
    {
      this->streamFragmentProcessing = i;

      result = (result == -1) ? fragmentTime : (min(result, fragmentTime));

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: time %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, fragmentTime);
      break;
    }
  }

  if (result != (-1))
  {
    this->flags &= ~(PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_SKIP_HEADER | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_SEND_FRAGMENTED_INDEX | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_FRAGMENTED_INDEX_SENT);
    this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

    // is set stream fragment to process
    // exists at least one stream fragment and cover all possible timestamps (0 - UINT64_MAX)

    this->streamLength = 0;

    CMshsStreamFragment *previousFragment = (this->streamFragmentProcessing != 0) ? this->streamFragments->GetItem(this->streamFragmentProcessing - 1) : NULL;
    CMshsStreamFragment *processingFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);

    if (processingFragment->ContainsReconstructedHeader())
    {
      // we must skip reconstructed header, FFmpeg can't handle them again
      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_SKIP_HEADER;
    }

    HRESULT res = S_OK;

    // create fragmented index box, which is required in seeking
    FREE_MEM_CLASS(this->fragmentedIndexBox);
    this->fragmentedIndexBoxSize = 0;

    this->fragmentedIndexBox = new CFragmentedIndexBox(&res);
    CHECK_POINTER_HRESULT(res, this->fragmentedIndexBox, res, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(res), res = this->GetFragmentedIndexBox(this->fragmentedIndexBox, this->streamFragments, this->videoTrackFragmentHeaderBox->GetTrackId(), this->audioTrackFragmentHeaderBox->GetTrackId(), processingFragment->IsVideo() ? this->streamFragmentProcessing : UINT_MAX, processingFragment->IsAudio() ? this->streamFragmentProcessing : UINT_MAX));

    if (FAILED(res))
    {
      FREE_MEM_CLASS(this->fragmentedIndexBox);
      result = -1;
    }
    
    if (result != (-1))
    {
      this->fragmentedIndexBoxSize = (unsigned int)this->fragmentedIndexBox->GetSize();
      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_SEND_FRAGMENTED_INDEX;

      if (!processingFragment->IsDownloaded())
      {
        // stream fragment is not downloaded, force its download
        // force to download missing fragment

        processingFragment->SetFragmentStartPosition(0);
        this->streamFragmentToDownload = this->streamFragmentProcessing;

        this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_MSHS_FLAG_STOP_RECEIVING_DATA;
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
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Mshs::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Mshs::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::Initialize(CPluginConfiguration *configuration)
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

  if (SUCCEEDED(result))
  {
    CParameterCollection *cookies = new CParameterCollection(&result);
    CHECK_POINTER_HRESULT(result, cookies, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      unsigned int currentCookiesCount = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_MSHS_COOKIES_COUNT, true, 0);
      if (currentCookiesCount != 0)
      {
        for (unsigned int i = 0; (SUCCEEDED(result) & (i < currentCookiesCount)); i++)
        {
          wchar_t *cookieName = FormatString(MSHS_COOKIE_FORMAT_PARAMETER_NAME, i);
          CHECK_POINTER_HRESULT(result, cookieName, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            const wchar_t *curlCookieValue = this->configuration->GetValue(cookieName, true, NULL);
            CHECK_POINTER_HRESULT(result, curlCookieValue, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, cookies->Add(L"", curlCookieValue), result, E_OUTOFMEMORY);
          }

          FREE_MEM(cookieName);
        }
      }
    }

    FREE_MEM_CLASS(cookies);
  }

  return result;
}

/* protected methods */

wchar_t *CMPUrlSourceSplitter_Protocol_Mshs::GetStoreFile(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

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

wchar_t *CMPUrlSourceSplitter_Protocol_Mshs::GetDumpFile(void)
{
  wchar_t *result = NULL;
  wchar_t *folder = Duplicate(this->configuration->GetValue(PARAMETER_NAME_LOG_FILE_NAME, true, NULL));

  if (folder != NULL)
  {
    PathRemoveFileSpec(folder);

    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%s\\mpurlsourcesplitter_protocol_mshs_%s.dump", folder, guid);
    }
    FREE_MEM(guid);
  }

  FREE_MEM(folder);

  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Mshs::GetBytePosition(void)
{
  int64_t result = 0;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    unsigned int first = this->streamFragments->GetStartSearchingIndex();
    unsigned int count = this->streamFragments->GetSearchCount();

    if (count != 0)
    {
      CMshsStreamFragment *firstFragment = this->streamFragments->GetItem(first);
      CMshsStreamFragment *lastFragment = this->streamFragments->GetItem(first + count - 1);

      result = lastFragment->GetFragmentStartPosition() + (int64_t)lastFragment->GetLength() - firstFragment->GetFragmentStartPosition();
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetStreamFragmentsFromManifest(CMshsStreamFragmentCollection *streamFragments, CMshsManifestSmoothStreamingMediaBox *manifest)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamFragments);
  CHECK_POINTER_DEFAULT_HRESULT(result, manifest);

  CHECK_CONDITION_HRESULT(result, manifest->GetProtections()->Count() == 0, result, E_DRM_PROTECTED);

  if (SUCCEEDED(result))
  {
    uint32_t videoIndex = 0;
    uint32_t audioIndex = 0;
    int64_t lastTimestamp = 0;
    uint32_t maxVideoIndex = 0;
    uint32_t maxAudioIndex = 0;

    // get maximum video and audio indexes
    for (unsigned int i = 0; i < manifest->GetStreams()->Count(); i++)
    {
      CMshsManifestStreamBox *stream = manifest->GetStreams()->GetItem(i);

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
    CMshsManifestTrackBox *videoTrack = NULL;
    CMshsManifestTrackBox *audioTrack = NULL;
    const wchar_t *baseUrl = this->configuration->GetValue(PARAMETER_NAME_MSHS_BASE_URL, true, NULL);

    while (SUCCEEDED(result) && ((videoIndex < maxVideoIndex) || (audioIndex < maxAudioIndex)))
    {
      // there is still some fragment to add to stream fragments
      // choose fragment which is nearest to last timestamp

      CMshsManifestStreamFragmentBox *videoFragment = NULL;
      CMshsManifestStreamFragmentBox *audioFragment = NULL;

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < manifest->GetStreams()->Count())); i++)
      {
        CMshsManifestStreamBox *stream = manifest->GetStreams()->GetItem(i);

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
      int64_t fragmentTime = 0;
      unsigned int fragmentType = MSHS_STREAM_FRAGMENT_FLAG_NONE;

      CHECK_CONDITION_HRESULT(result, (videoFragment != NULL) || (audioFragment != NULL), result, E_MSHS_NO_VIDEO_AND_AUDIO_FRAGMENT);

      if (SUCCEEDED(result))
      {
        if ((videoFragment != NULL) && (audioFragment != NULL))
        {
          int64_t videoDiff = (int64_t)videoFragment->GetFragmentTime() - lastTimestamp;
          int64_t audioDiff = (int64_t)audioFragment->GetFragmentTime() - lastTimestamp;

          if (videoDiff <= audioDiff)
          {
            fragmentTime = (int64_t)videoFragment->GetFragmentTime();
            fragmentType = MSHS_STREAM_FRAGMENT_FLAG_VIDEO;
            url = this->FormatUrl(baseUrl, videoUrlPattern, videoTrack, videoFragment);
            videoIndex++;
          }
          else if (audioDiff < videoDiff)
          {
            fragmentTime = (int64_t)audioFragment->GetFragmentTime();
            fragmentType = MSHS_STREAM_FRAGMENT_FLAG_AUDIO;
            url = this->FormatUrl(baseUrl, audioUrlPattern, audioTrack, audioFragment);
            audioIndex++;
          }
        }
        else if (videoFragment != NULL)
        {
          fragmentTime = (int64_t)videoFragment->GetFragmentTime();
          fragmentType = MSHS_STREAM_FRAGMENT_FLAG_VIDEO;
          url = this->FormatUrl(baseUrl, videoUrlPattern, videoTrack, videoFragment);
          videoIndex++;
        }
        else if (audioFragment != NULL)
        {
          fragmentTime = (int64_t)audioFragment->GetFragmentTime();
          fragmentType = MSHS_STREAM_FRAGMENT_FLAG_AUDIO;
          url = this->FormatUrl(baseUrl, audioUrlPattern, audioTrack, audioFragment);
          audioIndex++;
        }
      }

      CHECK_POINTER_HRESULT(result, url, result, E_MSHS_NO_FRAGMENT_URL);

      if (SUCCEEDED(result))
      {
        lastTimestamp = fragmentTime;

        CMshsStreamFragment *streamFragment = new CMshsStreamFragment(&result, fragmentTime, url, fragmentType);
        CHECK_POINTER_HRESULT(result, streamFragment, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, streamFragments->Add(streamFragment), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamFragment));
      }

      FREE_MEM(url);
    }
  }

  return result;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Mshs::FormatUrl(const wchar_t *baseUrl, const wchar_t *urlPattern, CMshsManifestTrackBox *track, CMshsManifestStreamFragmentBox *fragment)
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

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetTrackFragmentHeaderBox(CTrackFragmentHeaderBox *trackFragmentHeaderBox, CMshsStreamFragment *fragment, unsigned int trackID)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, trackFragmentHeaderBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, fragment);
  CHECK_CONDITION_HRESULT(result, fragment->GetBuffer()->GetBufferOccupiedSpace() > 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    unsigned int bufferSize = fragment->GetBuffer()->GetBufferOccupiedSpace();
    ALLOC_MEM_DEFINE_SET(buffer, uint8_t, bufferSize, 0);
    CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      fragment->GetBuffer()->CopyFromBuffer(buffer, bufferSize);

      CMovieFragmentBox *movieFragmentBox = new CMovieFragmentBox(&result);
      CHECK_POINTER_HRESULT(result, movieFragmentBox, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, movieFragmentBox->Parse(buffer, bufferSize), result, E_MSHS_CANNOT_PARSE_MOVIE_FRAGMENT_BOX);

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < movieFragmentBox->GetBoxes()->Count())); i++)
      {
        CBox *box = movieFragmentBox->GetBoxes()->GetItem(i);

        if (box->IsType(TRACK_FRAGMENT_BOX_TYPE))
        {
          for (unsigned int j = 0; (SUCCEEDED(result) && (j < box->GetBoxes()->Count())); j++)
          {
            CBox *subBox = box->GetBoxes()->GetItem(j);

            if (subBox->IsType(TRACK_FRAGMENT_HEADER_BOX_TYPE))
            {
              // we found track fragment header box
              // we can't return reference because movie fragment box is container and will be destroyed
              // we can save track fragment header box into buffer and then create track fragment header box from buffer

              uint32_t subBoxSize = (uint32_t)subBox->GetSize();
              CHECK_CONDITION_HRESULT(result, subBoxSize > 0, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                ALLOC_MEM_DEFINE_SET(trackFragmentHeaderBoxBuffer, uint8_t, subBoxSize, 0);
                CHECK_POINTER_HRESULT(result, trackFragmentHeaderBoxBuffer, result, E_OUTOFMEMORY);

                CHECK_CONDITION_HRESULT(result, subBox->GetBox(trackFragmentHeaderBoxBuffer, subBoxSize), result, E_MSHS_CANNOT_GET_TRACK_HEADER_FRAGMENT_BOX_FROM_MOVIE_FRAGMENT_BOX);
                CHECK_CONDITION_HRESULT(result, trackFragmentHeaderBox->Parse(trackFragmentHeaderBoxBuffer, subBoxSize), result, E_MSHS_CANNOT_PARSE_TRACK_FRAGMENT_HEADER_BOX);

                if (SUCCEEDED(result) && (trackID != UINT_MAX))
                {
                  trackFragmentHeaderBox->SetTrackId(trackID);
                }

                FREE_MEM(trackFragmentHeaderBoxBuffer);
              }
            }
          }
        }
      }

      FREE_MEM_CLASS(movieFragmentBox);
    }

    FREE_MEM(buffer);
  }

  return result;
}

bool CMPUrlSourceSplitter_Protocol_Mshs::AddBoxIntoBuffer(CBox *box, CLinearBuffer *buffer)
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

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetFileTypeBox(CFileTypeBox *fileTypeBox)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, fileTypeBox);

  if (SUCCEEDED(result))
  {
    fileTypeBox->SetMinorVersion(512);

    CHECK_CONDITION_HRESULT(result, fileTypeBox->GetMajorBrand()->SetBrandString(L"isml"), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, fileTypeBox->GetCompatibleBrands()->AddBrand(L"piff"), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, fileTypeBox->GetCompatibleBrands()->AddBrand(L"iso2"), result, E_OUTOFMEMORY);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetMovieBox(CMovieBox *movieBox, CMshsManifestSmoothStreamingMediaBox *media, CTrackFragmentHeaderBox *videoFragmentHeaderBox, CTrackFragmentHeaderBox *audioFragmentHeaderBox)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, movieBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, media);
  CHECK_POINTER_DEFAULT_HRESULT(result, videoFragmentHeaderBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, audioFragmentHeaderBox);

  if (SUCCEEDED(result))
  {
    // add movie header box
    CMovieHeaderBox *movieHeaderBox = new CMovieHeaderBox(&result);
    CHECK_POINTER_HRESULT(result, movieHeaderBox, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // set time scale by manifest
      movieHeaderBox->SetTimeScale((uint32_t)media->GetTimeScale());

      // set next track ID to last used track ID + 1
      movieHeaderBox->SetNextTrackId(this->lastTrackID + 1);

      movieHeaderBox->GetRate()->SetIntegerPart(1);
      movieHeaderBox->GetVolume()->SetIntegerPart(1);
    }

    CHECK_CONDITION_HRESULT(result, movieBox->GetBoxes()->Add(movieHeaderBox), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(movieHeaderBox));
  }

  if (SUCCEEDED(result))
  {
    unsigned int videoStreamIndex = 0;
    unsigned int audioStreamIndex = 0;
    unsigned int trackIndex = 0;

    for (unsigned int i = 0; i < media->GetStreams()->Count(); i++)
    {
      CMshsManifestStreamBox *stream = media->GetStreams()->GetItem(i);

      if (stream->IsVideo())
      {
        videoStreamIndex = i;
        break;
      }
    }

    for (unsigned int i = 0; i < media->GetStreams()->Count(); i++)
    {
      CMshsManifestStreamBox *stream = media->GetStreams()->GetItem(i);

      if (stream->IsAudio())
      {
        audioStreamIndex = i;
        break;
      }
    }

    if (SUCCEEDED(result))
    {
      // add track box (video or audio - depends on track ID)
      CTrackBox *trackBox = new CTrackBox(&result);
      CHECK_POINTER_HRESULT(result, trackBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        result = (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
          this->GetVideoTrackBox(trackBox, media, videoStreamIndex, trackIndex, videoFragmentHeaderBox) : this->GetAudioTrackBox(trackBox, media, audioStreamIndex, trackIndex, audioFragmentHeaderBox);
      }

      CHECK_CONDITION_HRESULT(result, movieBox->GetBoxes()->Add(trackBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(trackBox));
    }

    if (SUCCEEDED(result))
    {
      // add track box (video or audio - depends on track ID)
      CTrackBox *trackBox = new CTrackBox(&result);
      CHECK_POINTER_HRESULT(result, trackBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        result = (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
          this->GetAudioTrackBox(trackBox, media, audioStreamIndex, trackIndex, audioFragmentHeaderBox) : this->GetVideoTrackBox(trackBox, media, videoStreamIndex, trackIndex, videoFragmentHeaderBox);
      }

      CHECK_CONDITION_HRESULT(result, movieBox->GetBoxes()->Add(trackBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(trackBox));
    }
  }

  if (SUCCEEDED(result))
  {
    // add movie extends box
    CMovieExtendsBox *movieExtendsBox = new CMovieExtendsBox(&result);
    CHECK_POINTER_HRESULT(result, movieExtendsBox, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // add track extends box (video or audio - depends on track ID)
      CTrackExtendsBox *trackExtendsBox = new CTrackExtendsBox(&result);
      CHECK_POINTER_HRESULT(result, trackExtendsBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        trackExtendsBox->SetTrackId(
          (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
          videoFragmentHeaderBox->GetTrackId() : audioFragmentHeaderBox->GetTrackId());

        trackExtendsBox->SetDefaultSampleDescriptionIndex(1);
      }

      CHECK_CONDITION_HRESULT(result, movieExtendsBox->GetBoxes()->Add(trackExtendsBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(trackExtendsBox));
    }

    if (SUCCEEDED(result))
    {
      // add track extends box (video or audio - depends on track ID)
      CTrackExtendsBox *trackExtendsBox = new CTrackExtendsBox(&result);
      CHECK_POINTER_HRESULT(result, trackExtendsBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        trackExtendsBox->SetTrackId(
          (videoFragmentHeaderBox->GetTrackId() < audioFragmentHeaderBox->GetTrackId()) ? 
          audioFragmentHeaderBox->GetTrackId() : videoFragmentHeaderBox->GetTrackId());

        trackExtendsBox->SetDefaultSampleDescriptionIndex(1);
      }

      CHECK_CONDITION_HRESULT(result, movieExtendsBox->GetBoxes()->Add(trackExtendsBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(trackExtendsBox));
    }

    CHECK_CONDITION_HRESULT(result, movieBox->GetBoxes()->Add(movieExtendsBox), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(movieExtendsBox));
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetVideoTrackBox(CTrackBox *videoTrackBox, CMshsManifestSmoothStreamingMediaBox *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, videoTrackBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, media);
  CHECK_POINTER_DEFAULT_HRESULT(result, fragmentHeaderBox);

  if (SUCCEEDED(result))
  {
    CMshsManifestStreamBox *stream = media->GetStreams()->GetItem(streamIndex);
    CMshsManifestTrackBox *track = stream->GetTracks()->GetItem(trackIndex);

    // add track header box
    CTrackHeaderBox *trackHeaderBox = new CTrackHeaderBox(&result);
    CHECK_POINTER_HRESULT(result, trackHeaderBox, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // set flags, track ID, duration, width and height
      // set version to 1 (uint(64))
      trackHeaderBox->SetBoxFlags(0x0000000F);
      trackHeaderBox->SetTrackId(fragmentHeaderBox->GetTrackId());
      trackHeaderBox->SetDuration(media->GetDuration());
      trackHeaderBox->SetVersion(1);

      trackHeaderBox->GetWidth()->SetIntegerPart(track->GetMaxWidth());
      trackHeaderBox->GetHeight()->SetIntegerPart(track->GetMaxHeight());
    }

    CHECK_CONDITION_HRESULT(result, videoTrackBox->GetBoxes()->Add(trackHeaderBox), result, E_OUTOFMEMORY);
    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(trackHeaderBox));
    
    if (SUCCEEDED(result))
    {
      // add media box
      CMediaBox *mediaBox = new CMediaBox(&result);
      CHECK_POINTER_HRESULT(result, mediaBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // add media header box
        CMediaHeaderBox *mediaHeaderBox = new CMediaHeaderBox(&result);
        CHECK_POINTER_HRESULT(result, mediaHeaderBox, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // set version (1 = uint(64)), time scale from manifest, duration
          mediaHeaderBox->SetVersion(1);
          mediaHeaderBox->SetTimeScale((uint32_t)media->GetTimeScale());
          mediaHeaderBox->SetDuration(media->GetDuration());
        }

        CHECK_CONDITION_HRESULT(result, mediaBox->GetBoxes()->Add(mediaHeaderBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaHeaderBox));
      }

      if (SUCCEEDED(result))
      {
        // add handler box
        CHandlerBox *handlerBox = new CHandlerBox(&result);
        CHECK_POINTER_HRESULT(result, handlerBox, result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), handlerBox->SetHandlerType(HANDLER_TYPE_VIDEO));
        CHECK_CONDITION_HRESULT(result, handlerBox->SetName(L"VideoHandler"), result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, mediaBox->GetBoxes()->Add(handlerBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(handlerBox));
      }

      if (SUCCEEDED(result))
      {
        // add media information box
        CMediaInformationBox *mediaInformationBox = new CMediaInformationBox(&result, HANDLER_TYPE_VIDEO);
        CHECK_POINTER_HRESULT(result, mediaInformationBox, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // add video media header box
          CVideoMediaHeaderBox *videoMediaHeaderBox = new CVideoMediaHeaderBox(&result);
          CHECK_POINTER_HRESULT(result, videoMediaHeaderBox, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            videoMediaHeaderBox->SetBoxFlags(0x00000001);
          }

          CHECK_CONDITION_HRESULT(result, mediaInformationBox->GetBoxes()->Add(videoMediaHeaderBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(videoMediaHeaderBox));
        }

        if (SUCCEEDED(result))
        {
          // add data information box

          CDataInformationBox *dataInformationBox = new CDataInformationBox(&result);
          CHECK_POINTER_HRESULT(result, dataInformationBox, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            // add data reference box
            CDataReferenceBox *dataReferenceBox = new CDataReferenceBox(&result);
            CHECK_POINTER_HRESULT(result, dataReferenceBox, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              // add data entry url box
              CDataEntryUrlBox *dataEntryUrlBox = new CDataEntryUrlBox(&result);
              CHECK_POINTER_HRESULT(result, dataEntryUrlBox, result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), dataEntryUrlBox->SetSelfContained(true));

              CHECK_CONDITION_HRESULT(result, dataReferenceBox->GetDataEntryBoxCollection()->Add(dataEntryUrlBox), result, E_OUTOFMEMORY);
              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dataEntryUrlBox));
            }

            CHECK_CONDITION_HRESULT(result, dataInformationBox->GetBoxes()->Add(dataReferenceBox), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dataReferenceBox));
          }

          CHECK_CONDITION_HRESULT(result, mediaInformationBox->GetBoxes()->Add(dataInformationBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dataInformationBox));
        }

        if (SUCCEEDED(result))
        {
          // add samle table box
          CSampleTableBox *sampleTableBox = new CSampleTableBox(&result, HANDLER_TYPE_VIDEO);
          CHECK_POINTER_HRESULT(result, sampleTableBox, result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->GetVideoSampleTableBox(sampleTableBox, media, streamIndex, trackIndex, fragmentHeaderBox));

          CHECK_CONDITION_HRESULT(result, mediaInformationBox->GetBoxes()->Add(sampleTableBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sampleTableBox));
        }

        CHECK_CONDITION_HRESULT(result, mediaBox->GetBoxes()->Add(mediaInformationBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaInformationBox));
      }
      
      CHECK_CONDITION_HRESULT(result, videoTrackBox->GetBoxes()->Add(mediaBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaBox));
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetAudioTrackBox(CTrackBox *audioTrackBox, CMshsManifestSmoothStreamingMediaBox *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, audioTrackBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, media);
  CHECK_POINTER_DEFAULT_HRESULT(result, fragmentHeaderBox);

  if (SUCCEEDED(result))
  {
    if (SUCCEEDED(result))
    {
      // add track header box
      CTrackHeaderBox *trackHeaderBox = new CTrackHeaderBox(&result);
      CHECK_POINTER_HRESULT(result, trackHeaderBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // set flags, track ID, duration, width and height
        // set version to 1 (uint(64))
        trackHeaderBox->SetBoxFlags(0x0000000F);
        trackHeaderBox->SetTrackId(fragmentHeaderBox->GetTrackId());
        trackHeaderBox->SetDuration(media->GetDuration());
        trackHeaderBox->SetVersion(1);

        trackHeaderBox->SetAlternateGroup(1);
        trackHeaderBox->GetVolume()->SetIntegerPart(1);
      }

      CHECK_CONDITION_HRESULT(result, audioTrackBox->GetBoxes()->Add(trackHeaderBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(trackHeaderBox));
    }

    if (SUCCEEDED(result))
    {
      // add media box
      CMediaBox *mediaBox = new CMediaBox(&result);
      CHECK_POINTER_HRESULT(result, mediaBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // add media header box
        CMediaHeaderBox *mediaHeaderBox = new CMediaHeaderBox(&result);
        CHECK_POINTER_HRESULT(result, mediaHeaderBox, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // set version (1 = uint(64)), time scale from manifest, duration
          mediaHeaderBox->SetVersion(1);
          mediaHeaderBox->SetTimeScale((uint32_t)media->GetTimeScale());
          mediaHeaderBox->SetDuration(media->GetDuration());
        }

        CHECK_CONDITION_HRESULT(result, mediaBox->GetBoxes()->Add(mediaHeaderBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaHeaderBox));
      }

      if (SUCCEEDED(result))
      {
        // add handler box
        CHandlerBox *handlerBox = new CHandlerBox(&result);
        CHECK_POINTER_HRESULT(result, handlerBox, result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(result), handlerBox->SetHandlerType(HANDLER_TYPE_AUDIO));
        CHECK_CONDITION_HRESULT(result, handlerBox->SetName(L"SoundHandler"), result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, mediaBox->GetBoxes()->Add(handlerBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(handlerBox));
      }

      if (SUCCEEDED(result))
      {
        // add media information box
        CMediaInformationBox *mediaInformationBox = new CMediaInformationBox(&result, HANDLER_TYPE_AUDIO);
        CHECK_POINTER_HRESULT(result, mediaInformationBox, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // add sound media header box
          CSoundMediaHeaderBox *soundMediaHeaderBox = new CSoundMediaHeaderBox(&result);
          CHECK_POINTER_HRESULT(result, soundMediaHeaderBox, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, mediaInformationBox->GetBoxes()->Add(soundMediaHeaderBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(soundMediaHeaderBox));
        }

        if (SUCCEEDED(result))
        {
          // add data information box
          CDataInformationBox *dataInformationBox = new CDataInformationBox(&result);
          CHECK_POINTER_HRESULT(result, dataInformationBox, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            // add data reference box
            CDataReferenceBox *dataReferenceBox = new CDataReferenceBox(&result);
            CHECK_POINTER_HRESULT(result, dataReferenceBox, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              // add data entry url box
              CDataEntryUrlBox *dataEntryUrlBox = new CDataEntryUrlBox(&result);
              CHECK_POINTER_HRESULT(result, dataEntryUrlBox, result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), dataEntryUrlBox->SetSelfContained(true));

              CHECK_CONDITION_HRESULT(result, dataReferenceBox->GetDataEntryBoxCollection()->Add(dataEntryUrlBox), result, E_OUTOFMEMORY);
              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dataEntryUrlBox));
            }

            CHECK_CONDITION_HRESULT(result, dataInformationBox->GetBoxes()->Add(dataReferenceBox), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dataReferenceBox));
          }

          CHECK_CONDITION_HRESULT(result, mediaInformationBox->GetBoxes()->Add(dataInformationBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(dataInformationBox));
        }

        if (SUCCEEDED(result))
        {
          // add samle table box
          CSampleTableBox *sampleTableBox = new CSampleTableBox(&result, HANDLER_TYPE_AUDIO);
          CHECK_POINTER_HRESULT(result, sampleTableBox, result, E_OUTOFMEMORY);

          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->GetAudioSampleTableBox(sampleTableBox, media, streamIndex, trackIndex, fragmentHeaderBox));

          CHECK_CONDITION_HRESULT(result, mediaInformationBox->GetBoxes()->Add(sampleTableBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sampleTableBox));
        }

        CHECK_CONDITION_HRESULT(result, mediaBox->GetBoxes()->Add(mediaInformationBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaInformationBox));
      }

      CHECK_CONDITION_HRESULT(result, audioTrackBox->GetBoxes()->Add(mediaBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(mediaBox));
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetVideoSampleTableBox(CSampleTableBox *sampleTableBox, CMshsManifestSmoothStreamingMediaBox *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, sampleTableBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, media);
  CHECK_POINTER_DEFAULT_HRESULT(result, fragmentHeaderBox);

  if (SUCCEEDED(result))
  {
    CMshsManifestStreamBox *stream = media->GetStreams()->GetItem(streamIndex);
    CMshsManifestTrackBox *track = stream->GetTracks()->GetItem(trackIndex);

    if (SUCCEEDED(result))
    {
      // add sample description box
      CSampleDescriptionBox *sampleDescriptionBox = new CSampleDescriptionBox(&result, HANDLER_TYPE_VIDEO);
      CHECK_POINTER_HRESULT(result, sampleDescriptionBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // add visual sample entry
        CVisualSampleEntryBox *visualSampleEntryBox = new CVisualSampleEntryBox(&result);
        CHECK_POINTER_HRESULT(result, visualSampleEntryBox, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          CHECK_CONDITION_HRESULT(result, visualSampleEntryBox->SetCodingName(L"avc1"), result, E_OUTOFMEMORY);
          visualSampleEntryBox->SetDataReferenceIndex(1);
          visualSampleEntryBox->GetHorizontalResolution()->SetIntegerPart(72);
          visualSampleEntryBox->GetVerticalResolution()->SetIntegerPart(72);
          visualSampleEntryBox->SetFrameCount(1);
          CHECK_CONDITION_HRESULT(result, visualSampleEntryBox->SetCompressorName(L""), result, E_OUTOFMEMORY);
          visualSampleEntryBox->SetDepth(24);
          visualSampleEntryBox->SetWidth((uint16_t)track->GetMaxWidth());
          visualSampleEntryBox->SetHeight((uint16_t)track->GetMaxHeight());
        }

        if (SUCCEEDED(result))
        {
          // add AVC configuration box
          CAVCConfigurationBox *avcConfigurationBox = new CAVCConfigurationBox(&result);
          CHECK_POINTER_HRESULT(result, avcConfigurationBox, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            char *codecPrivateData = ConvertToMultiByte(track->GetCodecPrivateData());
            CHECK_POINTER_HRESULT(result, codecPrivateData, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              const char *spsStart = strstr(codecPrivateData, "00000001");
              CHECK_POINTER_HRESULT(result, spsStart, result, E_MSHS_NOT_FOUND_SPS_START);

              if (SUCCEEDED(result))
              {
                spsStart += 8;

                const char *ppsStart = strstr(spsStart, "00000001");
                CHECK_POINTER_HRESULT(result, ppsStart, result, E_MSHS_NOT_FOUND_PPS_START);

                if (SUCCEEDED(result))
                {
                  ppsStart += 8;
                  unsigned int ppsLength = strlen(ppsStart);
                  unsigned int spsLength = strlen(spsStart) - ppsLength - 8;

                  // we have SPS start and PPS start
                  // parse data to AVC configuration box
                  ALLOC_MEM_DEFINE_SET(sps, char, (spsLength + 1), 0);
                  CHECK_POINTER_HRESULT(result, sps, result, E_OUTOFMEMORY);

                  if (SUCCEEDED(result))
                  {
                    memcpy(sps, spsStart, spsLength);

                    uint8_t *convertedSps = HexToDecA(sps);
                    uint8_t *convertedPps = HexToDecA(ppsStart);

                    CHECK_POINTER_HRESULT(result, convertedSps, result, E_MSHS_CANNOT_CONVERT_SPS);
                    CHECK_POINTER_HRESULT(result, convertedPps, result, E_MSHS_CANNOT_CONVERT_PPS);

                    if (SUCCEEDED(result))
                    {
                      avcConfigurationBox->GetAVCDecoderConfiguration()->SetConfigurationVersion(1);
                      avcConfigurationBox->GetAVCDecoderConfiguration()->SetAvcProfileIndication(convertedSps[1]);
                      avcConfigurationBox->GetAVCDecoderConfiguration()->SetProfileCompatibility(convertedSps[2]);
                      avcConfigurationBox->GetAVCDecoderConfiguration()->SetAvcLevelIndication(convertedSps[3]);
                      avcConfigurationBox->GetAVCDecoderConfiguration()->SetLengthSizeMinusOne(3);
                    }

                    if (SUCCEEDED(result))
                    {
                      CSequenceParameterSetNALUnit *spsUnit = new CSequenceParameterSetNALUnit(&result);
                      CHECK_POINTER_HRESULT(result, spsUnit, result, E_OUTOFMEMORY);

                      CHECK_CONDITION_HRESULT(result, spsUnit->SetBuffer(convertedSps, spsLength / 2), result, E_OUTOFMEMORY);

                      CHECK_CONDITION_HRESULT(result, avcConfigurationBox->GetAVCDecoderConfiguration()->GetSequenceParameterSetNALUnits()->Add(spsUnit), result, E_OUTOFMEMORY);
                      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(spsUnit));
                    }

                    if (SUCCEEDED(result))
                    {
                      CPictureParameterSetNALUnit *ppsUnit = new CPictureParameterSetNALUnit(&result);
                      CHECK_POINTER_HRESULT(result, ppsUnit, result, E_OUTOFMEMORY);

                      CHECK_CONDITION_HRESULT(result, ppsUnit->SetBuffer(convertedPps, ppsLength / 2), result, E_OUTOFMEMORY);

                      CHECK_CONDITION_HRESULT(result, avcConfigurationBox->GetAVCDecoderConfiguration()->GetPictureParameterSetNALUnits()->Add(ppsUnit), result, E_OUTOFMEMORY);
                      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(ppsUnit));
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

          CHECK_CONDITION_HRESULT(result, visualSampleEntryBox->GetBoxes()->Add(avcConfigurationBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(avcConfigurationBox));
        }

        CHECK_CONDITION_HRESULT(result, sampleDescriptionBox->GetSampleEntries()->Add(visualSampleEntryBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(visualSampleEntryBox));
      }

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(sampleDescriptionBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sampleDescriptionBox));
    }

    if (SUCCEEDED(result))
    {
      // add time to sample box
      CTimeToSampleBox *timeToSampleBox = new CTimeToSampleBox(&result);
      CHECK_POINTER_HRESULT(result, timeToSampleBox, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(timeToSampleBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(timeToSampleBox));
    }

    if (SUCCEEDED(result))
    {
      // add sample to chunk box
      CSampleToChunkBox *sampleToChunkBox = new CSampleToChunkBox(&result);
      CHECK_POINTER_HRESULT(result, sampleToChunkBox, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(sampleToChunkBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sampleToChunkBox));
    }

    if (SUCCEEDED(result))
    {
      // add chunk offset box
      CChunkOffsetBox *chunkOffsetBox = new CChunkOffsetBox(&result);
      CHECK_POINTER_HRESULT(result, chunkOffsetBox, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(chunkOffsetBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(chunkOffsetBox));
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetAudioSampleTableBox(CSampleTableBox *sampleTableBox, CMshsManifestSmoothStreamingMediaBox *media, unsigned int streamIndex, unsigned int trackIndex, CTrackFragmentHeaderBox *fragmentHeaderBox)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, sampleTableBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, media);
  CHECK_POINTER_DEFAULT_HRESULT(result, fragmentHeaderBox);

  if (SUCCEEDED(result))
  {
    CMshsManifestStreamBox *stream = media->GetStreams()->GetItem(streamIndex);
    CMshsManifestTrackBox *track = stream->GetTracks()->GetItem(trackIndex);

    if (SUCCEEDED(result))
    {
      // add sample description box
      CSampleDescriptionBox *sampleDescriptionBox = new CSampleDescriptionBox(&result, HANDLER_TYPE_AUDIO);
      CHECK_POINTER_HRESULT(result, sampleDescriptionBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        // add audio sample entry
        CAudioSampleEntryBox *audioSampleEntryBox = new CAudioSampleEntryBox(&result);
        CHECK_POINTER_HRESULT(result, audioSampleEntryBox, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          CHECK_CONDITION_HRESULT(result, audioSampleEntryBox->SetCodingName(L"mp4a"), result, E_OUTOFMEMORY);
          audioSampleEntryBox->SetChannelCount(track->GetChannels());
          audioSampleEntryBox->SetSampleSize(track->GetBitsPerSample());
          audioSampleEntryBox->GetSampleRate()->SetIntegerPart(track->GetSamplingRate());
        }

        if (SUCCEEDED(result))
        {
          // add ESD box
          CESDBox *esdBox = new CESDBox(&result);
          CHECK_POINTER_HRESULT(result, esdBox, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            uint32_t length = (track->GetCodecPrivateData() != NULL) ? wcslen(track->GetCodecPrivateData()) : 0;

            esdBox->SetTrackId(fragmentHeaderBox->GetTrackId());
            esdBox->SetCodecTag(CODEC_TAG_AAC);
            esdBox->SetMaxBitrate(128000);

            if (length > 0)
            {
              uint8_t *convertedCodecPrivateData = HexToDecW(track->GetCodecPrivateData());
              CHECK_POINTER_HRESULT(result, convertedCodecPrivateData, result, E_OUTOFMEMORY);

              CHECK_CONDITION_HRESULT(result, esdBox->SetCodecPrivateData(convertedCodecPrivateData, length / 2), result, E_OUTOFMEMORY);
              
              FREE_MEM(convertedCodecPrivateData);
            }
          }

          CHECK_CONDITION_HRESULT(result, audioSampleEntryBox->GetBoxes()->Add(esdBox), result, E_OUTOFMEMORY);
          CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(esdBox));
        }
        
        CHECK_CONDITION_HRESULT(result, sampleDescriptionBox->GetSampleEntries()->Add(audioSampleEntryBox), result, E_OUTOFMEMORY);
        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(audioSampleEntryBox));
      }

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(sampleDescriptionBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sampleDescriptionBox));
    }

    if (SUCCEEDED(result))
    {
      // add time to sample box
      CTimeToSampleBox *timeToSampleBox = new CTimeToSampleBox(&result);
      CHECK_POINTER_HRESULT(result, timeToSampleBox, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(timeToSampleBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(timeToSampleBox));
    }

    if (SUCCEEDED(result))
    {
      // add sample to chunk box
      CSampleToChunkBox *sampleToChunkBox = new CSampleToChunkBox(&result);
      CHECK_POINTER_HRESULT(result, sampleToChunkBox, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(sampleToChunkBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(sampleToChunkBox));
    }

    if (SUCCEEDED(result))
    {
      // add chunk offset box
      CChunkOffsetBox *chunkOffsetBox = new CChunkOffsetBox(&result);
      CHECK_POINTER_HRESULT(result, chunkOffsetBox, result, E_OUTOFMEMORY);

      CHECK_CONDITION_HRESULT(result, sampleTableBox->GetBoxes()->Add(chunkOffsetBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(chunkOffsetBox));
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetFragmentedIndexBox(CFragmentedIndexBox *fragmentedIndexBox, CMshsStreamFragmentCollection *streamFragments, uint32_t videoTrackId, uint32_t audioTrackId, unsigned int videoFragmentIndex, unsigned int audioFragmentIndex)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, fragmentedIndexBox);
  
  if (SUCCEEDED(result))
  {
    if (SUCCEEDED(result))
    {
      // add fragmented index track box (video or audio - depends on track ID)
      CFragmentedIndexTrackBox *fragmentedIndexTrackBox = new CFragmentedIndexTrackBox(&result);
      CHECK_POINTER_HRESULT(result, fragmentedIndexTrackBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        result =
          (videoTrackId < audioTrackId) ? 
          this->GetVideoFragmentedIndexTrackBox(fragmentedIndexTrackBox, streamFragments, videoTrackId, audioTrackId, videoFragmentIndex, audioFragmentIndex) : this->GetAudioFragmentedIndexTrackBox(fragmentedIndexTrackBox, streamFragments, videoTrackId, audioTrackId, videoFragmentIndex, audioFragmentIndex);
      }
      
      CHECK_CONDITION_HRESULT(result, fragmentedIndexBox->GetBoxes()->Add(fragmentedIndexTrackBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragmentedIndexTrackBox));
    }

    if (SUCCEEDED(result))
    {
      // add fragmented index track box (video or audio - depends on track ID)
      CFragmentedIndexTrackBox *fragmentedIndexTrackBox = new CFragmentedIndexTrackBox(&result);
      CHECK_POINTER_HRESULT(result, fragmentedIndexTrackBox, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        result =
          (videoTrackId < audioTrackId) ? 
          this->GetAudioFragmentedIndexTrackBox(fragmentedIndexTrackBox, streamFragments, videoTrackId, audioTrackId, videoFragmentIndex, audioFragmentIndex) : this->GetVideoFragmentedIndexTrackBox(fragmentedIndexTrackBox, streamFragments, videoTrackId, audioTrackId, videoFragmentIndex, audioFragmentIndex);
      }
      
      CHECK_CONDITION_HRESULT(result, fragmentedIndexBox->GetBoxes()->Add(fragmentedIndexTrackBox), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragmentedIndexTrackBox));
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetVideoFragmentedIndexTrackBox(CFragmentedIndexTrackBox *fragmentedIndexTrackBox, CMshsStreamFragmentCollection *streamFragments, uint32_t videoTrackId, uint32_t audioTrackId, unsigned int videoFragmentIndex, unsigned int audioFragmentIndex)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, fragmentedIndexTrackBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, streamFragments);
  CHECK_CONDITION_HRESULT(result, (videoFragmentIndex != UINT_MAX) || (audioFragmentIndex != UINT_MAX), result, E_INVALIDARG);
  
  if (SUCCEEDED(result))
  {
    if (videoFragmentIndex == UINT_MAX)
    {
      // we don't have index of video fragment in streamFragments collection
      // but video fragment will be very close (in most cases it is next fragment)

      for (unsigned int i = (audioFragmentIndex + 1); (SUCCEEDED(result) && (i < streamFragments->Count())); i++)
      {
        CMshsStreamFragment *fragment = streamFragments->GetItem(i);

        if (fragment->IsVideo())
        {
          videoFragmentIndex = i;
          break;
        }
      }
    }

    CHECK_CONDITION_HRESULT(result, videoFragmentIndex != UINT_MAX, result, E_MSHS_CANNOT_GET_VIDEO_FRAGMENT_INDEX);

    if (SUCCEEDED(result))
    {
      CMshsStreamFragment *fragment = streamFragments->GetItem(videoFragmentIndex);

      fragmentedIndexTrackBox->SetTrackId(videoTrackId);

      CFragmentedIndex *index = new CFragmentedIndex(&result);
      CHECK_POINTER_HRESULT(result, index, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        index->SetTimestamp((uint64_t)fragment->GetFragmentTimestamp());
        // duration is ignored in FFmpeg
        index->SetDuration(0);
      }

      CHECK_CONDITION_HRESULT(result, fragmentedIndexTrackBox->GetFragmentedIndexes()->Add(index), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(index));
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Mshs::GetAudioFragmentedIndexTrackBox(CFragmentedIndexTrackBox *fragmentedIndexTrackBox, CMshsStreamFragmentCollection *streamFragments, uint32_t videoTrackId, uint32_t audioTrackId, unsigned int videoFragmentIndex, unsigned int audioFragmentIndex)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, fragmentedIndexTrackBox);
  CHECK_POINTER_DEFAULT_HRESULT(result, streamFragments);
  CHECK_CONDITION_HRESULT(result, (videoFragmentIndex != UINT_MAX) || (audioFragmentIndex != UINT_MAX), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    if (audioFragmentIndex == UINT_MAX)
    {
      // we don't have index of audio fragment in streamFragments collection
      // but audio fragment will be very close (in most cases it is next fragment)

      for (unsigned int i = (videoFragmentIndex + 1); (SUCCEEDED(result) && (i < streamFragments->Count())); i++)
      {
        CMshsStreamFragment *fragment = streamFragments->GetItem(i);

        if (fragment->IsAudio())
        {
          audioFragmentIndex = i;
          break;
        }
      }
    }

    CHECK_CONDITION_HRESULT(result, audioFragmentIndex != UINT_MAX, result, E_MSHS_CANNOT_GET_AUDIO_FRAGMENT_INDEX);

    if (SUCCEEDED(result))
    {
      CMshsStreamFragment *fragment = streamFragments->GetItem(audioFragmentIndex);

      fragmentedIndexTrackBox->SetTrackId(audioTrackId);

      CFragmentedIndex *index = new CFragmentedIndex(&result);
      CHECK_POINTER_HRESULT(result, index, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        index->SetTimestamp((uint64_t)fragment->GetFragmentTimestamp());
        // duration is ignored in FFmpeg
        index->SetDuration(0);
      }

      CHECK_CONDITION_HRESULT(result, fragmentedIndexTrackBox->GetFragmentedIndexes()->Add(index), result, E_OUTOFMEMORY);
      CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(index));
    }
  }

  return result;
}