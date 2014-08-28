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

#include "MPUrlSourceSplitter_Protocol_Afhs.h"
#include "MPUrlSourceSplitter_Protocol_Afhs_Parameters.h"
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
#include "AfhsDecryptionContext.h"

#include "base64.h"

#include "Box.h"
#include "BootstrapInfoBox.h"
#include "MediaDataBox.h"

#include "FlvPacket.h"
#include "F4MManifest.h"

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Afhsd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                          L"MPUrlSourceSplitter_Protocol_Afhs"
#endif

#define METHOD_GET_SEGMENTS_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX_NAME            L"GetSegmentsFragmentsFromBootstrapInfoBox()"

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Afhs(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Afhs *protocol = (CMPUrlSourceSplitter_Protocol_Afhs *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Afhs::CMPUrlSourceSplitter_Protocol_Afhs(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->lockCurlMutex = NULL;
  this->lockMutex = NULL;
  this->mainCurlInstance = NULL;
  this->streamLength = 0;
  this->segmentFragments = NULL;
  this->cacheFile = NULL;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->segmentFragmentDownloading = UINT_MAX;
  this->segmentFragmentProcessing = UINT_MAX;
  this->segmentFragmentToDownload = UINT_MAX;
  this->decryptionHoster = NULL;
  this->manifest = NULL;
  this->headerAndMetaPacketSize = 0;
  this->segmentFragmentZeroTimestamp = 0;
  
  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->segmentFragments = new CAfhsSegmentFragmentCollection(result);
    this->decryptionHoster = new CAfhsDecryptionHoster(result, logger, configuration);
    this->manifest = new CF4MManifest(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->segmentFragments, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->decryptionHoster, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->manifest, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(*result), this->decryptionHoster->LoadPlugins(), *result);

    // create CURL instance
    this->mainCurlInstance = new CAfhsCurlInstance(result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
    CHECK_POINTER_HRESULT(*result, this->mainCurlInstance, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS);
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

CMPUrlSourceSplitter_Protocol_Afhs::~CMPUrlSourceSplitter_Protocol_Afhs()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  // because segments and fragments can be changed in decryption hoster, collection of segments and fragments have to be released before decryption hoster
  // in other case it can lead to access violation exception, due to virtual function table is allocated in memory space of decryption hoster
  FREE_MEM_CLASS(this->segmentFragments);

  FREE_MEM_CLASS(this->decryptionHoster);
  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->cacheFile);
  FREE_MEM_CLASS(this->manifest);

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

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Afhs::GetConnectionState(void)
{
  return this->mainCurlInstance->GetConnectionState();
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::ReceiveData(CStreamPackage *streamPackage)
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

        bufferSize = this->mainCurlInstance->GetAfhsDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
      }

      if (bufferSize > 0)
      {
        this->mainCurlInstance->SetConnectionState(Opened);
      }
    }

    if (SUCCEEDED(result) && (this->segmentFragments->HasEncryptedSegmentFragments()))
    {
      CAfhsDecryptionContext *decryptionContext = new CAfhsDecryptionContext(&result);
      CHECK_CONDITION_HRESULT(result, decryptionContext, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        decryptionContext->SetCurlInstance(this->mainCurlInstance);
        decryptionContext->SetSegmentFragmentDownloading(this->segmentFragmentDownloading);
        decryptionContext->SetSegmentFragmentProcessing(this->segmentFragmentProcessing);
        decryptionContext->SetSegmentFragmentToDownload(this->segmentFragmentToDownload);
        decryptionContext->SetSegmentsFragments(this->segmentFragments);
        decryptionContext->SetManifestUrl(this->configuration->GetValue(PARAMETER_NAME_AFHS_MANIFEST_URL, true, NULL));
        decryptionContext->SetConfiguration(this->configuration);
      }

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->decryptionHoster->DecryptSegmentFragments(decryptionContext), result);
      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: decryption of segment fragments failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, result));
      FREE_MEM_CLASS(decryptionContext);
    }

    if (SUCCEEDED(result) && (this->segmentFragments->HasDecryptedSegmentFragments()))
    {
      // some segment fragments are decrypted

      CIndexedAfhsSegmentFragmentCollection *indexedDecryptedSegmentFragments = new CIndexedAfhsSegmentFragmentCollection(&result);
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->segmentFragments->GetDecryptedStreamFragments(indexedDecryptedSegmentFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedDecryptedSegmentFragments->Count())); i++)
      {
        CIndexedAfhsSegmentFragment *indexedDecryptedSegmentFragment = indexedDecryptedSegmentFragments->GetItem(i);
        CAfhsSegmentFragment *currentDecryptingFragment = indexedDecryptedSegmentFragment->GetItem();

        // we can process segment and fragment
        // after processing we mark fragment as downloaded = it is ready for filter

        unsigned int bufferSize = currentDecryptingFragment->GetBuffer()->GetBufferOccupiedSpace();
        unsigned int processed = 0;

        CHECK_CONDITION_HRESULT(result, bufferSize != 0, result, E_AFHS_DECRYPTED_DATA_SIZE_ZERO);

        if (SUCCEEDED(result))
        {
          ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferSize, 0);
          CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            currentDecryptingFragment->GetBuffer()->CopyFromBuffer(buffer, bufferSize);
            currentDecryptingFragment->GetBuffer()->ClearBuffer();

            if (SUCCEEDED(result) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_ADDED_HEADER_AND_META)))
            {
              // we must add FLV file header

              CHECK_CONDITION_HRESULT(result, currentDecryptingFragment->GetBuffer()->AddToBufferWithResize(FLV_FILE_HEADER, FLV_FILE_HEADER_LENGTH) == FLV_FILE_HEADER_LENGTH, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                char *mediaMetadataBase64Encoded = ConvertToMultiByteW(this->configuration->GetValue(PARAMETER_NAME_AFHS_MEDIA_METADATA, true, NULL));
                if (mediaMetadataBase64Encoded != NULL)
                {
                  // metadata can be in connection parameters, but it is optional
                  // metadata is BASE64 encoded

                  unsigned char *metadata = NULL;
                  unsigned int metadataLength = 0;

                  CHECK_CONDITION_HRESULT(result, SUCCEEDED(base64_decode(mediaMetadataBase64Encoded, &metadata, &metadataLength)), result, E_AFHS_CANNOT_DECODE_METADATA);

                  if (SUCCEEDED(result))
                  {
                    // create FLV packet from metadata and add its content to buffer for processing
                    CFlvPacket *metadataFlvPacket = new CFlvPacket(&result);
                    CHECK_POINTER_HRESULT(result, metadataFlvPacket, result, E_OUTOFMEMORY);

                    CHECK_CONDITION_HRESULT(result, metadataFlvPacket->CreatePacket(FLV_PACKET_META, metadata, metadataLength, (unsigned int)currentDecryptingFragment->GetFragmentTimestamp(), false), result, E_AFHS_CANNOT_CREATE_METADATA_FLV_PACKET);
                    CHECK_CONDITION_HRESULT(result, currentDecryptingFragment->GetBuffer()->AddToBufferWithResize(metadataFlvPacket->GetData(), metadataFlvPacket->GetSize()) == metadataFlvPacket->GetSize(), result, E_OUTOFMEMORY);

                    if (SUCCEEDED(result))
                    {
                      this->headerAndMetaPacketSize = FLV_FILE_HEADER_LENGTH + metadataFlvPacket->GetSize();
                      currentDecryptingFragment->SetContainsHeaderOrMetaPacket(true);
                    }

                    FREE_MEM_CLASS(metadataFlvPacket);
                  }

                  FREE_MEM(metadata);
                }

                FREE_MEM(mediaMetadataBase64Encoded);
              }

              this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_ADDED_HEADER_AND_META;
            }

            CBox *box = new CBox(&result);
            CHECK_POINTER_HRESULT(result, box, result, E_OUTOFMEMORY);

            while (SUCCEEDED(result) && (processed < bufferSize))
            {
              CHECK_CONDITION_HRESULT(result, box->Parse(buffer + processed, bufferSize - processed), result, E_AFHS_CANNOT_PARSE_BOX);
              CHECK_CONDITION_HRESULT(result, box->GetSize() != 0, result, E_AFHS_BOX_SIZE_ZERO);

              if (SUCCEEDED(result) && (wcscmp(box->GetType(), MEDIA_DATA_BOX_TYPE) == 0))
              {
                CMediaDataBox *mediaBox = new CMediaDataBox(&result);
                CHECK_POINTER_HRESULT(result, mediaBox, result, E_OUTOFMEMORY);

                CHECK_CONDITION_HRESULT(result, mediaBox->Parse(buffer + processed, bufferSize - processed), result, E_AFHS_CANNOT_PARSE_MEDIA_DATA_BOX);

                if (SUCCEEDED(result))
                {
                  currentDecryptingFragment->GetBuffer()->AddToBufferWithResize(mediaBox->GetPayload(), (unsigned int)mediaBox->GetPayloadSize());
                  processed += (unsigned int)box->GetSize();
                }

                FREE_MEM_CLASS(mediaBox);
              }
            }

            FREE_MEM_CLASS(box);
          }

          FREE_MEM(buffer);
        }

        if (SUCCEEDED(result))
        {
          currentDecryptingFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
          currentDecryptingFragment->SetDecrypted(false, UINT_MAX);
          currentDecryptingFragment->SetDownloaded(true, UINT_MAX);

          this->segmentFragments->UpdateIndexes(indexedDecryptedSegmentFragment->GetItemIndex(), 1);

          // recalculate start position of all downloaded segment fragments until first not downloaded segment fragment
          this->RecalculateSegmentFragmentStartPosition(this->segmentFragments, indexedDecryptedSegmentFragment->GetItemIndex());
        }

        if (SUCCEEDED(result))
        {
          if (!this->IsLiveStreamDetected())
          {
            // check if we downloaded last segment fragment - then we set end of stream reached - it doesn't mean that we have all data (no gaps)

            if ((!this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED)) && (this->segmentFragments->Count() != 0))
            {
              // not set end of stream, at least one segment fragment
              CAfhsSegmentFragment *lastFragment = this->segmentFragments->GetItem(this->segmentFragments->Count() - 1);

              if (lastFragment->IsDownloaded())
              {
                this->logger->Log(LOGGER_INFO, L"%s: %s: end of stream reached", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
                this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

                unsigned int fragmentToDownload = UINT_MAX;

                // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
                fragmentToDownload = (this->segmentFragmentToDownload == UINT_MAX) ? this->segmentFragments->GetFirstNotDownloadedItemIndex(indexedDecryptedSegmentFragment->GetItemIndex()) : this->segmentFragmentToDownload;
                // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
                fragmentToDownload = (fragmentToDownload == UINT_MAX) ? this->segmentFragments->GetFirstNotDownloadedItemIndex(0) : fragmentToDownload;
                // fragment to download still can be UINT_MAX = no fragment to download
  
                this->segmentFragmentToDownload = fragmentToDownload;

                if (this->segmentFragmentToDownload == UINT_MAX)
                {
                  // no segment fragment to download, we have all data
                  this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
                }
              }
            }
          }
          else
          {
            // live stream, check if we downloaded last segment fragment
            // if yes, then download bootstrap info and get new segment fragments

            if (this->segmentFragments->Count() != 0)
            {
              // at least one segment fragment
              CAfhsSegmentFragment *lastFragment = this->segmentFragments->GetItem(this->segmentFragments->Count() - 1);

              if (lastFragment->IsDownloaded())
              {
                this->logger->Log(LOGGER_INFO, L"%s: %s: live stream, downloaded last segment fragment, requesting bootstrap info for update of segment fragments", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_UPDATE_SEGMENT_FRAGMENTS | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA;
              }
            }
          }
        }
      }

      FREE_MEM_CLASS(indexedDecryptedSegmentFragments);
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance->GetConnectionState() == Initializing) && (!this->IsWholeStreamDownloaded()) && (!this->mainCurlInstance->IsLockedCurlInstance()) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_UPDATE_SEGMENT_FRAGMENTS)))
    {
      if (this->segmentFragments->Count() == 0)
      {
        CBootstrapInfoBox *bootstrapInfoBox = new CBootstrapInfoBox(&result);
        CHECK_POINTER_HRESULT(result, bootstrapInfoBox, result, E_OUTOFMEMORY);

        char *bootstrapInfoBase64Encoded = ConvertToMultiByteW(this->configuration->GetValue(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO, true, NULL));
        CHECK_POINTER_HRESULT(result, bootstrapInfoBase64Encoded, result, E_CONVERT_STRING_ERROR);

        if (SUCCEEDED(result))
        {
          // bootstrap info is BASE64 encoded
          unsigned char *bootstrapInfo = NULL;
          unsigned int bootstrapInfoLength = 0;

          result = base64_decode(bootstrapInfoBase64Encoded, &bootstrapInfo, &bootstrapInfoLength);
          CHECK_CONDITION_HRESULT(result, bootstrapInfoBox->Parse(bootstrapInfo, bootstrapInfoLength), result, E_AFHS_CANNOT_PARSE_BOOTSTRAP_INFO_BOX);
        }

        // we must have F4M manifest, we can't be here without it

        if (SUCCEEDED(result))
        {
          // we have bootstrap info box successfully parsed
          this->flags |= bootstrapInfoBox->IsLive() ? PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED : PROTOCOL_PLUGIN_FLAG_NONE;

          uint64_t currentMediaTime = (bootstrapInfoBox->GetCurrentMediaTime() > 0) ? (bootstrapInfoBox->GetCurrentMediaTime() - 1): 0;

          CAfhsSegmentFragmentCollection *parsedSegmentsFragments = this->GetSegmentsFragmentsFromBootstrapInfoBox(bootstrapInfoBox, false, this->IsLiveStreamDetected() ? currentMediaTime : 0);
          CHECK_POINTER_HRESULT(result, parsedSegmentsFragments, result, E_AFHS_CANNOT_GET_SEGMENT_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX);

          CHECK_CONDITION_HRESULT(result, this->segmentFragments->Append(parsedSegmentsFragments), result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, this->segmentFragments->SetBaseUrl(parsedSegmentsFragments->GetBaseUrl()), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, this->segmentFragments->SetSegmentFragmentUrlExtraParameters(parsedSegmentsFragments->GetSegmentFragmentUrlExtraParameters()), result, E_OUTOFMEMORY);

          this->segmentFragmentProcessing = 0;

          if (SUCCEEDED(result) && this->IsLiveStreamDetected())
          {
            // in case of live stream check current media time and choose right segment and fragment
            // this download one fragment before current media time

            // find segment and fragment to process
            result = E_AFHS_NOT_FOUND_SEGMENT_FRAGMENT_IN_LIVE_STREAM;

            for (unsigned int i = 0; i < this->segmentFragments->Count(); i++)
            {
              CAfhsSegmentFragment *segFrag = this->segmentFragments->GetItem(i);

              if (segFrag->GetFragmentTimestamp() <= (int64_t)currentMediaTime)
              {
                this->segmentFragmentProcessing = i;
                result = S_OK;
              }
            }

            // the segments and fragments before this->segmentFragmentProcessing are not needed
            if (SUCCEEDED(result) && (this->segmentFragmentProcessing > 0))
            {
              this->segmentFragments->Remove(0, this->segmentFragmentProcessing - 1);

              this->segmentFragmentProcessing = 0;
            }
          }

          if (SUCCEEDED(result))
          {
            // set start searching index to current processing segment fragment
            this->segmentFragments->SetStartSearchingIndex(this->segmentFragmentProcessing);
            // set count of fragments to search for specific position
            unsigned int firstNotDownloadedFragmentIndex = this->segmentFragments->GetFirstNotDownloadedItemIndex(this->segmentFragmentProcessing);
            this->segmentFragments->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? this->segmentFragments->Count() : firstNotDownloadedFragmentIndex) - this->segmentFragmentProcessing);

            this->segmentFragmentToDownload = 0;
          }
        }

        FREE_MEM_CLASS(bootstrapInfoBox);
        FREE_MEM(bootstrapInfoBase64Encoded);

        // set segment fragment zero timestamp as first segment fragment timestamp
        this->segmentFragmentZeroTimestamp = this->segmentFragments->GetItem(0)->GetFragmentTimestamp();
      }

      // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
      this->segmentFragmentToDownload = (this->segmentFragmentToDownload == UINT_MAX) ? this->segmentFragments->GetFirstNotDownloadedItemIndex(this->segmentFragmentProcessing) : this->segmentFragmentToDownload;
      // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
      this->segmentFragmentToDownload = (this->segmentFragmentToDownload == UINT_MAX) ? this->segmentFragments->GetFirstNotDownloadedItemIndex(0) : this->segmentFragmentToDownload;
      // fragment to download still can be UINT_MAX = no fragment to download

      //// clear all not downloaded segment fragments
      //CIndexedCacheFileItemCollection *notDownloadedIndexedItems = new CIndexedCacheFileItemCollection(&result);
      //CHECK_CONDITION_HRESULT(result, notDownloadedIndexedItems, result, E_OUTOFMEMORY);

      //CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->segmentFragments->GetNotDownloadedItems(notDownloadedIndexedItems));

      //for (unsigned int i = 0; (SUCCEEDED(result) && (i < notDownloadedIndexedItems->Count())); i++)
      //{
      //  CCacheFileItem *notDownloadedItem = notDownloadedIndexedItems->GetItem(i)->GetItem();

      //  notDownloadedItem->GetBuffer()->ClearBuffer();
      //}

      //FREE_MEM_CLASS(notDownloadedIndexedItems);

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
        if (this->configuration->GetValueBool(PARAMETER_NAME_DUMP_INPUT_RAW_DATA, true, PARAMETER_NAME_DUMP_INPUT_RAW_DATA_DEFAULT))
        {
          wchar_t *storeFilePath = this->GetStoreFile(L"dump");
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }
      }

      if (SUCCEEDED(result) && (this->segmentFragmentToDownload != UINT_MAX))
      {
        CAfhsDownloadRequest *request = new CAfhsDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_AFHS_OPEN_CONNECTION_TIMEOUT, true, AFHS_OPEN_CONNECTION_TIMEOUT_DEFAULT));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

          CAfhsSegmentFragment *fragment = this->segmentFragments->GetItem(this->segmentFragmentToDownload);
          // clear fragment buffer
          fragment->GetBuffer()->ClearBuffer();

          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: starting receiving data for segment: %u, fragment: %u, timestamp: %lld, original timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetSegment(), fragment->GetFragment(), fragment->GetFragmentTimestamp() - this->segmentFragmentZeroTimestamp, fragment->GetFragmentTimestamp());

          wchar_t *url = this->segmentFragments->GetSegmentFragmentUrl(fragment);
          CHECK_POINTER_HRESULT(result, url, result, E_OUTOFMEMORY);

          CHECK_CONDITION_HRESULT(result, request->SetUrl(url), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_AFHS_COOKIE, true, NULL)), result, E_OUTOFMEMORY);
          request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_AFHS_VERSION, true, HTTP_VERSION_DEFAULT));
          request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
          CHECK_CONDITION_HRESULT(result, request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_AFHS_REFERER, true, NULL)), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_AFHS_USER_AGENT, true, NULL)), result, E_OUTOFMEMORY);

          FREE_MEM(url);
          
          if (SUCCEEDED(this->mainCurlInstance->LockCurlInstance(this)))
          {
            if (SUCCEEDED(this->mainCurlInstance->Initialize(request)))
            {
              // all parameters set
              // start receiving data

              if (SUCCEEDED(this->mainCurlInstance->StartReceivingData()))
              {
                this->mainCurlInstance->SetConnectionState(Opening);

                this->segmentFragmentDownloading = this->segmentFragmentToDownload;
                this->segmentFragmentToDownload = UINT_MAX;
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

    if (SUCCEEDED(result) && (this->mainCurlInstance->GetConnectionState() == Initializing) && (!this->IsWholeStreamDownloaded()) && (!this->mainCurlInstance->IsLockedCurlInstance()) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_UPDATE_SEGMENT_FRAGMENTS)))
    {
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

      const wchar_t *bootstrapInfoUrl = this->configuration->GetValue(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO_URL, true, NULL);
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: starting downloading bootstrap info, url: '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bootstrapInfoUrl);

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
        CAfhsDownloadRequest *request = new CAfhsDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_AFHS_OPEN_CONNECTION_TIMEOUT, true, AFHS_OPEN_CONNECTION_TIMEOUT_DEFAULT));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

          CHECK_CONDITION_HRESULT(result, request->SetUrl(bootstrapInfoUrl), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_AFHS_COOKIE, true, NULL)), result, E_OUTOFMEMORY);
          request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_AFHS_VERSION, true, HTTP_VERSION_DEFAULT));
          request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
          CHECK_CONDITION_HRESULT(result, request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_AFHS_REFERER, true, NULL)), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_AFHS_USER_AGENT, true, NULL)), result, E_OUTOFMEMORY);

          if (SUCCEEDED(this->mainCurlInstance->LockCurlInstance(this)))
          {
            if (SUCCEEDED(this->mainCurlInstance->Initialize(request)))
            {
              // all parameters set
              // start receiving data

              if (SUCCEEDED(this->mainCurlInstance->StartReceivingData()))
              {
                this->mainCurlInstance->SetConnectionState(Opening);

                this->segmentFragmentDownloading = this->segmentFragmentToDownload;
                this->segmentFragmentToDownload = UINT_MAX;
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
      if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_UPDATE_SEGMENT_FRAGMENTS))
      {
        // bootstrap info received, check for error
        
        if (SUCCEEDED(this->mainCurlInstance->GetAfhsDownloadResponse()->GetResultError()))
        {
          // successfully downloaded bootstrap info
          unsigned int bootstrapInfoSize = this->mainCurlInstance->GetAfhsDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
          bool addedNewSegmentFragments = false;
          
          if (bootstrapInfoSize > 0)
          {
            // we ignore zero length bootstrap info, in that case we request bootstrap info again
            
            ALLOC_MEM_DEFINE_SET(bootstrapInfoBuffer, unsigned char, bootstrapInfoSize, 0);
            CHECK_CONDITION_HRESULT(result, bootstrapInfoBuffer, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              this->mainCurlInstance->GetAfhsDownloadResponse()->GetReceivedData()->CopyFromBuffer(bootstrapInfoBuffer, bootstrapInfoSize);

              CBootstrapInfoBox *bootstrapInfoBox = new CBootstrapInfoBox(&result);
              CHECK_CONDITION_HRESULT(result, bootstrapInfoBox, result, E_OUTOFMEMORY);

              CHECK_CONDITION_HRESULT(result, bootstrapInfoBox->Parse(bootstrapInfoBuffer, bootstrapInfoSize), result, E_AFHS_CANNOT_PARSE_BOOTSTRAP_INFO_BOX);

              if (SUCCEEDED(result))
              {
                CAfhsSegmentFragment *lastSegmentFragment = this->segmentFragments->GetItem(this->segmentFragments->Count() - 1);
                CAfhsSegmentFragmentCollection *updateSegmentsFragments = this->GetSegmentsFragmentsFromBootstrapInfoBox(bootstrapInfoBox, false, lastSegmentFragment->GetFragmentTimestamp());

                CHECK_POINTER_HRESULT(result, updateSegmentsFragments, result, E_AFHS_CANNOT_GET_SEGMENT_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX);
                
                for (unsigned int i = 0; (SUCCEEDED(result) && (i < updateSegmentsFragments->Count())); i++)
                {
                  CAfhsSegmentFragment *parsedSegmentFragment = updateSegmentsFragments->GetItem(i);
                  if (parsedSegmentFragment->GetFragment() > lastSegmentFragment->GetFragment())
                  {
                    addedNewSegmentFragments = true;

                    // new segment fragment, add it to be downloaded
                    CAfhsSegmentFragment *clone = new CAfhsSegmentFragment(&result, parsedSegmentFragment->GetSegment(), parsedSegmentFragment->GetFragment(), parsedSegmentFragment->GetFragmentTimestamp());
                    CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

                    CHECK_CONDITION_HRESULT(result, this->segmentFragments->Add(clone), result, E_OUTOFMEMORY);
                    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));

                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->logger->Log(LOGGER_VERBOSE, L"%s: %s: added new segment and fragment, segment %d, fragment %d, timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, clone->GetSegment(), clone->GetFragment(), clone->GetFragmentTimestamp()));
                  }
                }

                FREE_MEM_CLASS(updateSegmentsFragments);
              }

              FREE_MEM_CLASS(bootstrapInfoBox);
            }

            FREE_MEM(bootstrapInfoBuffer);
          }
          
          CHECK_CONDITION_EXECUTE(addedNewSegmentFragments, this->flags &= ~MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_UPDATE_SEGMENT_FRAGMENTS);
        }

        // stop receiving data and restart downloading of segment fragments
        this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA;
      }
      else
      {
        // all data received, we're not receiving data
        // check end of stream or error on AFHS connection

        if (SUCCEEDED(this->mainCurlInstance->GetAfhsDownloadResponse()->GetResultError()))
        {
          CAfhsSegmentFragment *currentDownloadingFragment = this->segmentFragments->GetItem(this->segmentFragmentDownloading);

          CHECK_CONDITION_HRESULT(result, currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetAfhsDownloadResponse()->GetReceivedData()) == this->mainCurlInstance->GetAfhsDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);
          // segment fragment is by default encrypted, until decryptor sets state to decrypted
          CHECK_CONDITION_EXECUTE(SUCCEEDED(result), currentDownloadingFragment->SetEncrypted(true, this->segmentFragmentDownloading));

          // request to download next segment fragment after current downloaded fragment
          this->segmentFragmentToDownload = this->segmentFragments->GetFirstNotDownloadedItemIndex(this->segmentFragmentDownloading + 1);
          this->segmentFragmentDownloading = UINT_MAX;

          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE;
          this->flags |= (this->segmentFragmentToDownload != UINT_MAX) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA : MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_NONE;
        }
        else
        {
          // error occured while receiving data

          if (this->IsLiveStreamDetected())
          {
            // download segment fragment again or download scheduled segment fragment
            CAfhsSegmentFragment *fragment = (this->segmentFragmentDownloading > 0) ? this->segmentFragments->GetItem(this->segmentFragmentDownloading - 1) : NULL;

            if (fragment != NULL)
            {
              // we report discontinuity (if possible)
              fragment->SetDiscontinuity(true, this->segmentFragmentDownloading - 1);
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, timestamp: %llu, position: %lld, size: %u, current position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragmentTimestamp(), fragment->GetFragmentStartPosition(), fragment->GetLength(), this->GetBytePosition());
            }

            this->segmentFragmentToDownload = this->segmentFragmentDownloading;
            this->segmentFragmentDownloading = UINT_MAX;

            // error while receiving data, stop receiving data
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA;
          }
          else
          {
            // we can re-open connection exactly on lost position (time)

            // download segment fragment again or download scheduled stream fragment
            this->segmentFragmentToDownload = this->segmentFragmentDownloading;
            this->segmentFragmentDownloading = UINT_MAX;

            // error while receiving data, stop receiving data
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA;
          }
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

      CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE);
    }

    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE) && (this->mainCurlInstance->IsLockedCurlInstanceByOwner(this) || (!this->mainCurlInstance->IsLockedCurlInstance())))
    {
      HRESULT res = S_OK;
      this->mainCurlInstance->SetConnectionState(Closing);

      if (this->mainCurlInstance != NULL)
      {
        res = this->mainCurlInstance->StopReceivingData();

        CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_INFO, L"%s: %s: closing connection failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, res));
        CHECK_CONDITION_EXECUTE(SUCCEEDED(res), this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->ClearBuffer());
      }

      if ((res == S_OK) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA)))
      {
        // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
        this->StopReceivingData();
      }

      if (res == S_OK)
      {
        this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"connection closed successfully");
        this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA);

        // if CURL instance is locked, then we unlock CURL instance
        this->mainCurlInstance->UnlockCurlInstance(this);
      }
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

        int64_t startPosition = ((foundDataLength == 0) && this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_SKIP_HEADER_AND_META)) ? this->headerAndMetaPacketSize : 0;
        startPosition += dataRequest->GetStart() + foundDataLength;
        
        unsigned int fragmentIndex = this->segmentFragments->GetSegmentFragmentIndexBetweenPositions(startPosition);

        while (fragmentIndex != UINT_MAX)
        {
          this->segmentFragmentProcessing = fragmentIndex;

          // get segment fragment
          CAfhsSegmentFragment *segmentFragment = this->segmentFragments->GetItem(fragmentIndex);
          CAfhsSegmentFragment *startSearchingSegmentFragment = this->segmentFragments->GetItem(this->segmentFragments->GetStartSearchingIndex());

          int64_t streamFragmentRelativeStart = segmentFragment->GetFragmentStartPosition() - startSearchingSegmentFragment->GetFragmentStartPosition();

          // set copy data start and copy data length
          unsigned int copyDataStart = (startPosition > streamFragmentRelativeStart) ? (unsigned int)(startPosition - streamFragmentRelativeStart) : 0;
          unsigned int copyDataLength = min(segmentFragment->GetLength() - copyDataStart, dataRequest->GetLength() - foundDataLength);

          // copy data from segment fragment to response buffer
          if (this->cacheFile->LoadItems(this->segmentFragments, fragmentIndex, true))
          {
            // memory is allocated while switching from Created to Waiting state, we can't have problem on next line
            dataResponse->GetBuffer()->AddToBufferWithResize(segmentFragment->GetBuffer(), copyDataStart, copyDataLength);
          }
          else
          {
            // we can't copy data, try it later
            break;
          }

          // update length of data
          foundDataLength += copyDataLength;

          if ((segmentFragment->IsDiscontinuity()) && ((dataRequest->GetStart() + dataRequest->GetLength()) >= (streamFragmentRelativeStart + segmentFragment->GetLength())))
          {
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, completing request, request '%u', start '%lld', size '%u', found: '%u', fragment start: %lld, fragment length: %u, start searching fragment start: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), foundDataLength, segmentFragment->GetFragmentStartPosition(), segmentFragment->GetLength(), startSearchingSegmentFragment->GetFragmentStartPosition());

            dataResponse->SetDiscontinuity(true);
            break;
          }
          else if (foundDataLength < dataRequest->GetLength())
          {
            // find another segment fragment after end of this segment fragment
            startPosition += copyDataLength;

            // find another segment fragment after end of this segment fragment
            fragmentIndex = this->segmentFragments->GetSegmentFragmentIndexBetweenPositions(startPosition);

            if (fragmentIndex != UINT_MAX)
            {
              this->segmentFragmentProcessing = fragmentIndex;
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
            // requested position is probably in segment fragment on the end of searchable segment fragments, between this->segmentFragments->GetStartSearchingIndex() and this->segmentFragments->GetSearchCount()
            // check if fragment is downloading
            // if fragment is not downloading, then schedule it for download

            unsigned int fragmentIndex = this->segmentFragments->GetStartSearchingIndex() + this->segmentFragments->GetSearchCount();
            CAfhsSegmentFragment *fragment = this->segmentFragments->GetItem(fragmentIndex);

            if ((!this->IsLiveStreamDetected()) && (fragment == NULL))
            {
              // bad, no such fragment exists, we don't have data
              // if live stream, we must update segment and fragments

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', not found stream fragment", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
            }

            if ((fragment != NULL) && (!fragment->IsDownloaded()) && (!fragment->IsEncrypted()) && (!fragment->IsDecrypted()) && (fragmentIndex != this->segmentFragmentDownloading) && ((this->mainCurlInstance->GetConnectionState() == None) || (this->mainCurlInstance->GetConnectionState() == Opened)))
            {
              // fragment is not downloaded and also is not downloading currently
              this->segmentFragmentDownloading = UINT_MAX;
              this->segmentFragmentToDownload = fragmentIndex;

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', segment fragment not downloaded, not downloading and not decrypting, scheduled for download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              if (this->mainCurlInstance->GetConnectionState() == Opened)
              {
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA;
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

    // store stream fragments to temporary file
    if ((GetTickCount() - this->lastStoreTime) > CACHE_FILE_LOAD_TO_MEMORY_TIME_SPAN_DEFAULT)
    {
      this->lastStoreTime = GetTickCount();

      if (this->segmentFragments->Count() > 0)
      {
        // in case of live stream remove all downloaded and processed segment fragments before reported stream time
        if ((this->IsLiveStream()) && (this->reportedStreamTime > 0))
        {
          unsigned int fragmentRemoveStart = (this->segmentFragments->GetStartSearchingIndex() == 0) ? 1 : 0;
          unsigned int fragmentRemoveCount = 0;

          while ((fragmentRemoveStart + fragmentRemoveCount) < this->segmentFragmentProcessing)
          {
            CAfhsSegmentFragment *fragment = this->segmentFragments->GetItem(fragmentRemoveStart + fragmentRemoveCount);

            if (((fragmentRemoveStart + fragmentRemoveCount) != this->segmentFragments->GetStartSearchingIndex()) && fragment->IsDownloaded() && ((fragment->GetFragmentTimestamp() - this->segmentFragmentZeroTimestamp)  < (int64_t)this->reportedStreamTime))
            {
              // fragment will be removed
              fragmentRemoveCount++;
            }
            else
            {
              break;
            }
          }

          if ((fragmentRemoveCount > 0) && (this->cacheFile->RemoveItems(this->segmentFragments, fragmentRemoveStart, fragmentRemoveCount)))
          {
            unsigned int startSearchIndex = (fragmentRemoveCount > this->segmentFragments->GetStartSearchingIndex()) ? 0 : (this->segmentFragments->GetStartSearchingIndex() - fragmentRemoveCount);
            unsigned int searchCountDecrease = (fragmentRemoveCount > this->segmentFragments->GetStartSearchingIndex()) ? (fragmentRemoveCount - this->segmentFragments->GetStartSearchingIndex()) : 0;

            this->segmentFragments->SetStartSearchingIndex(startSearchIndex);
            this->segmentFragments->SetSearchCount(this->segmentFragments->GetSearchCount() - searchCountDecrease);

            this->segmentFragments->Remove(fragmentRemoveStart, fragmentRemoveCount);

            this->segmentFragmentProcessing -= fragmentRemoveCount;

            if (this->segmentFragmentDownloading != UINT_MAX)
            {
              this->segmentFragmentDownloading -= fragmentRemoveCount;
            }

            if (this->segmentFragmentToDownload != UINT_MAX)
            {
              this->segmentFragmentToDownload -= fragmentRemoveCount;
            }
          }
        }

        if (this->cacheFile->GetCacheFile() == NULL)
        {
          wchar_t *storeFilePath = this->GetStoreFile(L"temp");
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }

        // store all segment fragments (which are not stored) to file
        if ((this->cacheFile->GetCacheFile() != NULL) && (this->segmentFragments->Count() != 0))
        {
          this->cacheFile->StoreItems(this->segmentFragments, this->lastStoreTime, this->IsWholeStreamDownloaded());
        }
      }
    }
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Afhs::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_AFHS_OPEN_CONNECTION_TIMEOUT, true, AFHS_OPEN_CONNECTION_TIMEOUT_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Afhs::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_AFHS_OPEN_CONNECTION_SLEEP_TIME, true, AFHS_OPEN_CONNECTION_SLEEP_TIME_DEFAULT);
}

unsigned int CMPUrlSourceSplitter_Protocol_Afhs::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_AFHS_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, AFHS_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT);
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::StartReceivingData(CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->mainCurlInstance->StopReceivingData());
  this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA);

  this->mainCurlInstance->SetConnectionState(None);

  this->segmentFragmentDownloading = UINT_MAX;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::QueryStreamProgress(CStreamProgress *streamProgress)
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

void CMPUrlSourceSplitter_Protocol_Afhs::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  __super::ClearSession();
 
  this->decryptionHoster->ClearSession();
  this->streamLength = 0;
  this->mainCurlInstance->SetConnectionState(None);
  this->cacheFile->Clear();
  this->segmentFragments->Clear();
  this->manifest->Clear();
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->segmentFragmentDownloading = UINT_MAX;
  this->segmentFragmentProcessing = UINT_MAX;
  this->segmentFragmentToDownload = UINT_MAX;
  this->headerAndMetaPacketSize = 0;
  this->segmentFragmentZeroTimestamp = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_Afhs::GetDuration(void)
{
  return this->IsLiveStream() ? DURATION_LIVE_STREAM : DURATION_UNSPECIFIED;
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::GetStreamInformation(CStreamInformationCollection *streams)
{
  // AFHS protocol has always one stream (container)
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

unsigned int CMPUrlSourceSplitter_Protocol_Afhs::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_Afhs::SeekToTime(unsigned int streamId, int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  // find segment fragment to process
  // TO DO: implement better and faster seeking algorithm
  for (unsigned int i = 0; i < this->segmentFragments->Count(); i++)
  {
    // first AFHS segment fragment has always zero timestamp
    CAfhsSegmentFragment *fragment = this->segmentFragments->GetItem(i);
    CAfhsSegmentFragment *nextFragment = this->segmentFragments->GetItem(i + 1);

    int64_t fragmentTime = fragment->GetFragmentTimestamp() - this->segmentFragmentZeroTimestamp;
    int64_t nextFragmentTime = (nextFragment == NULL) ? INT64_MAX : (nextFragment->GetFragmentTimestamp() - this->segmentFragmentZeroTimestamp);

    if ((fragmentTime <= time) && (nextFragmentTime >= time))
    {
      this->segmentFragmentProcessing = i;

      result = (result == -1) ? fragmentTime : (min(result, fragmentTime));

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: time %lld, segment: %u, fragment: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, fragmentTime, fragment->GetSegment(), fragment->GetFragment());
      break;
    }
  }

  if (result != (-1))
  {
    this->flags &= ~(PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_SKIP_HEADER_AND_META);
    this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

    // is set segment fragment to process
    // exists at least one segment fragment and cover all possible timestamps (0 - UINT64_MAX)

    this->streamLength = 0;

    CAfhsSegmentFragment *previousFragment = (this->segmentFragmentProcessing != 0) ? this->segmentFragments->GetItem(this->segmentFragmentProcessing - 1) : NULL;
    CAfhsSegmentFragment *processingFragment = this->segmentFragments->GetItem(this->segmentFragmentProcessing);

    if (processingFragment->ContainsHeaderOrMetaPacket())
    {
      // we must skip header and meta packet, FFmpeg can't handle them again
      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_SKIP_HEADER_AND_META;
    }

    if (!processingFragment->IsDownloaded())
    {
      // stream fragment is not downloaded, force its download
      // split stream fragment

      // force to download missing fragment
      this->segmentFragmentToDownload = this->segmentFragmentProcessing;

      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS_FLAG_STOP_RECEIVING_DATA;
    }

    // set start searching index to current processing segment fragment
    this->segmentFragments->SetStartSearchingIndex(this->segmentFragmentProcessing);
    // set count of fragments to search for specific position
    unsigned int firstNotDownloadedFragmentIndex = this->segmentFragments->GetFirstNotDownloadedItemIndex(this->segmentFragmentProcessing);

    if (firstNotDownloadedFragmentIndex == UINT_MAX)
    {
      this->segmentFragments->SetSearchCount(this->segmentFragments->Count() - this->segmentFragmentProcessing);
    }
    else
    {
      // clear end of stream reached flag, because we moved to place with gap between processing stream fragment and last stream fragment
      this->flags &= ~PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

      this->segmentFragments->SetSearchCount(firstNotDownloadedFragmentIndex - this->segmentFragmentProcessing);
    }
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Afhs::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Afhs::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Afhs::Initialize(CPluginConfiguration *configuration)
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
      unsigned int currentCookiesCount = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_AFHS_COOKIES_COUNT, true, 0);
      if (currentCookiesCount != 0)
      {
        for (unsigned int i = 0; (SUCCEEDED(result) & (i < currentCookiesCount)); i++)
        {
          wchar_t *cookieName = FormatString(AFHS_COOKIE_FORMAT_PARAMETER_NAME, i);
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

wchar_t *CMPUrlSourceSplitter_Protocol_Afhs::GetStoreFile(const wchar_t *extension)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_afhs_%s.%s", folder, guid, extension);
    }
    FREE_MEM(guid);
  }

  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Afhs::GetBytePosition(void)
{
  int64_t result = 0;

  {
    CLockMutex lock(this->lockMutex, INFINITE);

    unsigned int first = this->segmentFragments->GetStartSearchingIndex();
    unsigned int count = this->segmentFragments->GetSearchCount();

    if (count != 0)
    {
      CAfhsSegmentFragment *firstFragment = this->segmentFragments->GetItem(first);
      CAfhsSegmentFragment *lastFragment = this->segmentFragments->GetItem(first + count - 1);

      result = (unsigned int)(lastFragment->GetFragmentStartPosition() + lastFragment->GetLength() - firstFragment->GetFragmentStartPosition());
    }
  }

  return result;
}

void CMPUrlSourceSplitter_Protocol_Afhs::RecalculateSegmentFragmentStartPosition(CAfhsSegmentFragmentCollection *segmentFragments, unsigned int startIndex)
{
  for (unsigned int i = startIndex; i < segmentFragments->Count(); i++)
  {
    CAfhsSegmentFragment *fragment = segmentFragments->GetItem(i);
    CAfhsSegmentFragment *previousFragment = (i > 0) ? segmentFragments->GetItem(i - 1) : NULL;

    if (fragment->IsDownloaded())
    {
      if ((previousFragment != NULL) && (previousFragment->IsDownloaded()))
      {
        fragment->SetFragmentStartPosition(previousFragment->GetFragmentStartPosition() + previousFragment->GetLength());
      }

      if (i == (segmentFragments->GetStartSearchingIndex() + segmentFragments->GetSearchCount()))
      {
        segmentFragments->SetSearchCount(segmentFragments->GetSearchCount() + 1);
      }
    }
    else
    {
      // we found not downloaded stream fragment, stop recalculating start positions
      break;
    }
  }
}

CAfhsSegmentFragmentCollection *CMPUrlSourceSplitter_Protocol_Afhs::GetSegmentsFragmentsFromBootstrapInfoBox(CBootstrapInfoBox *bootstrapInfoBox, bool logCollection, uint64_t lastSegmentFragmentTimestamp)
{
  HRESULT result = S_OK;
  CAfhsSegmentFragmentCollection *segmentsFragments = NULL;

  if (SUCCEEDED(result))
  {
    // now choose from bootstrap info -> QualityEntryTable highest quality (if exists) with segment run
    wchar_t *quality = NULL;
    CSegmentRunEntryCollection *segmentRunEntryTable = NULL;

    for (unsigned int i = 0; ((i <= bootstrapInfoBox->GetQualityEntryTable()->Count()) && (segmentRunEntryTable == NULL)); i++)
    {
      FREE_MEM(quality);

      // choose quality only for valid indexes, in another case is quality NULL
      if (i != bootstrapInfoBox->GetQualityEntryTable()->Count())
      {
        CBootstrapInfoQualityEntry *bootstrapInfoQualityEntry = bootstrapInfoBox->GetQualityEntryTable()->GetItem(0);
        quality = Duplicate(bootstrapInfoQualityEntry->GetQualityEntry());
      }

      // from segment run table choose segment with specifed quality (if exists) or segment with QualityEntryCount equal to zero
      for (unsigned int i = 0; i < bootstrapInfoBox->GetSegmentRunTable()->Count(); i++)
      {
        CSegmentRunTableBox *segmentRunTableBox = bootstrapInfoBox->GetSegmentRunTable()->GetItem(i);

        if (quality != NULL)
        {
          if (segmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(quality, false))
          {
            segmentRunEntryTable = segmentRunTableBox->GetSegmentRunEntryTable();
          }
        }
        else
        {
          if ((segmentRunTableBox->GetQualitySegmentUrlModifiers()->Count() == 0) || (segmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(L"", false)))
          {
            segmentRunEntryTable = segmentRunTableBox->GetSegmentRunEntryTable();
          }
        }
      }
    }

    if (segmentRunEntryTable == NULL)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_SEGMENTS_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX_NAME, L"cannot find any segment run table");
      result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
    }

    if (SUCCEEDED(result))
    {
      if (segmentRunEntryTable->Count() == 0)
      {
        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_SEGMENTS_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX_NAME, L"cannot find any segment run entry");
        result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
      }
    }

    // from fragment run table choose fragment with specifed quality (if exists) or fragment with QualityEntryCount equal to zero
    CFragmentRunEntryCollection *fragmentRunEntryTableTemp = NULL;
    unsigned int timeScale = 0;
    for (unsigned int i = 0; i < bootstrapInfoBox->GetFragmentRunTable()->Count(); i++)
    {
      CFragmentRunTableBox *fragmentRunTableBox = bootstrapInfoBox->GetFragmentRunTable()->GetItem(i);

      if (quality != NULL)
      {
        if (fragmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(quality, false))
        {
          fragmentRunEntryTableTemp = fragmentRunTableBox->GetFragmentRunEntryTable();
          timeScale = fragmentRunTableBox->GetTimeScale();
        }
      }
      else
      {
        if ((fragmentRunTableBox->GetQualitySegmentUrlModifiers()->Count() == 0) || (fragmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(L"", false)))
        {
          fragmentRunEntryTableTemp = fragmentRunTableBox->GetFragmentRunEntryTable();
          timeScale = fragmentRunTableBox->GetTimeScale();
        }
      }
    }

    if (fragmentRunEntryTableTemp == NULL)
    {
      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_SEGMENTS_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX_NAME, L"cannot find any fragment run table");
      result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
    }

    wchar_t *serverBaseUrl = Duplicate(this->configuration->GetValue(PARAMETER_NAME_AFHS_BASE_URL, true, L""));
    for (unsigned int i = 0; i < bootstrapInfoBox->GetServerEntryTable()->Count(); i++)
    {
      CBootstrapInfoServerEntry *serverEntry = bootstrapInfoBox->GetServerEntryTable()->GetItem(i);
      if (!IsNullOrEmptyOrWhitespace(serverEntry->GetServerEntry()))
      {
        FREE_MEM(serverBaseUrl);
        serverBaseUrl = Duplicate(serverEntry->GetServerEntry());
      }
    }
    CHECK_POINTER_HRESULT(result, serverBaseUrl, result, E_OUTOFMEMORY);

    wchar_t *mediaPartUrl = NULL;
    wchar_t *baseUrl = NULL;
    //wchar_t *movieIdentifierUrl = NULL;
    wchar_t *qualityUrl = NULL;

    if (SUCCEEDED(result))
    {
      mediaPartUrl = Duplicate(this->configuration->GetValue(PARAMETER_NAME_AFHS_MEDIA_PART_URL, true, L""));
      CHECK_POINTER_HRESULT(result, mediaPartUrl, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      baseUrl = FormatAbsoluteUrl(serverBaseUrl, mediaPartUrl);
      CHECK_POINTER_HRESULT(result, baseUrl, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      //movieIdentifierUrl = FormatAbsoluteUrl(baseUrl, this->bootstrapInfoBox->GetMovieIdentifier());
      //CHECK_POINTER_HRESULT(result, movieIdentifierUrl, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      //qualityUrl = FormatString(L"%s%s", movieIdentifierUrl, (quality == NULL) ? L"" : quality);
      qualityUrl = FormatAbsoluteUrl(baseUrl, (quality == NULL) ? L"" : quality);
      CHECK_POINTER_HRESULT(result, qualityUrl, result, E_OUTOFMEMORY);
    }

    if (SUCCEEDED(result))
    {
      segmentsFragments = new CAfhsSegmentFragmentCollection(&result);
      CHECK_POINTER_HRESULT(result, segmentsFragments, result, E_OUTOFMEMORY);
    }

    CHECK_CONDITION_HRESULT(result, segmentsFragments->SetBaseUrl(qualityUrl), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, segmentsFragments->SetSegmentFragmentUrlExtraParameters(this->configuration->GetValue(PARAMETER_NAME_AFHS_SEGMENT_FRAGMENT_URL_EXTRA_PARAMETERS, true, NULL)), result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      // convert segment run entry table and fragment run entry table to segments and fragments

      unsigned int segmentRunEntryIndex = 0;  // holds index in segmentRunEntryTable

      unsigned int firstSegmentInRun = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFirstSegment();
      unsigned int lastSegmentInRun = (((segmentRunEntryIndex + 1) < segmentRunEntryTable->Count()) ? segmentRunEntryTable->GetItem(segmentRunEntryIndex + 1)->GetFirstSegment() : UINT_MAX);
      unsigned int fragmentsPerSegment = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFragmentsPerSegment();   // if UINT_MAX, then current segment is forever
      unsigned int fragmentInSegment = 0;     // holds fragment in current segment, if it reach fragmentsPerSegment, the segment must be changed

      unsigned int segmentNumber = firstSegmentInRun;     // holds segment number for final segment and fragment

      for (unsigned int i = 0; i < fragmentRunEntryTableTemp->Count(); i++)
      {
        CFragmentRunEntry *fragmentRunEntryTemp = fragmentRunEntryTableTemp->GetItem(i);
        unsigned int nextItemIndex = i + 1;
        CFragmentRunEntry *fragmentRunEntryTempNext = NULL;

        for (unsigned int j = nextItemIndex; j < fragmentRunEntryTableTemp->Count(); j++)
        {
          CFragmentRunEntry *temp = fragmentRunEntryTableTemp->GetItem(nextItemIndex);
          if (temp->GetFirstFragment() != 0)
          {
            fragmentRunEntryTempNext = temp;
            break;
          }
          else
          {
            nextItemIndex++;
          }
        }

        uint64_t fragmentTimestamp = fragmentRunEntryTemp->GetFirstFragmentTimestamp();
        unsigned int lastFragment = (fragmentRunEntryTempNext == NULL) ? (fragmentRunEntryTemp->GetFirstFragment() + 1) : fragmentRunEntryTempNext->GetFirstFragment();
        uint64_t lastFragmentTimestamp = (fragmentRunEntryTempNext == NULL) ? (fragmentRunEntryTemp->GetFirstFragmentTimestamp() + fragmentRunEntryTemp->GetFragmentDuration() - 1) : (fragmentRunEntryTempNext->GetFirstFragmentTimestamp() - 1);

        // in some special live session can be some fragments with zero duration
        // these fragments can get HTTP 503 error, which disables playback
        // just skip these fragments and continue with next fragment
        if ((lastFragmentTimestamp >= lastSegmentFragmentTimestamp) && (fragmentRunEntryTemp->GetFragmentDuration() != 0))
        {
          // current fragment run entry has at least some timestamps greater than requested segment and fragment timestamp (lastSegmentFragmentTimestamp)
          unsigned int fragmentIndex = (unsigned int)((lastFragmentTimestamp - fragmentRunEntryTemp->GetFirstFragmentTimestamp()) / fragmentRunEntryTemp->GetFragmentDuration());

          fragmentTimestamp += fragmentIndex * fragmentRunEntryTemp->GetFragmentDuration();
          fragmentIndex += fragmentRunEntryTemp->GetFirstFragment();

          while (SUCCEEDED(result) && (fragmentIndex < lastFragment))
          {
            CAfhsSegmentFragment *segFrag = new CAfhsSegmentFragment(&result, segmentNumber, fragmentIndex, fragmentTimestamp * 1000 / timeScale);
            CHECK_POINTER_HRESULT(result, segFrag, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, segmentsFragments->Add(segFrag), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(segFrag));

            fragmentTimestamp += fragmentRunEntryTemp->GetFragmentDuration();
            fragmentIndex++;
          }
        }

        // update segment information if necessary
        unsigned int fragmentCount = lastFragment - fragmentRunEntryTemp->GetFirstFragment();
        while (fragmentCount != 0)
        {
          unsigned int fragmentsToEndOfSegment = min(fragmentsPerSegment - fragmentInSegment, fragmentCount);
          fragmentCount -= fragmentsToEndOfSegment;
          fragmentInSegment += fragmentsToEndOfSegment;

          if (fragmentInSegment == fragmentsPerSegment)
          {
            // we reached end of segment, update segment information
            if ((++segmentNumber) >= lastSegmentInRun)
            {
              // we reached end of segment run, we must update firstSegmentInRun, lastSegmentInRun, fragmentsPerSegment
              segmentRunEntryIndex++;

              firstSegmentInRun = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFirstSegment();
              lastSegmentInRun = (((segmentRunEntryIndex + 1) < segmentRunEntryTable->Count()) ? segmentRunEntryTable->GetItem(segmentRunEntryIndex + 1)->GetFirstSegment() : UINT_MAX);
              fragmentsPerSegment = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFragmentsPerSegment();   // if UINT_MAX, then current segment is forever
            }

            fragmentInSegment = 0;
          }
        }
      }
    }

    FREE_MEM(serverBaseUrl);
    FREE_MEM(mediaPartUrl);
    FREE_MEM(baseUrl);
    //FREE_MEM(movieIdentifierUrl);
    FREE_MEM(qualityUrl);

    FREE_MEM(quality);

    if (SUCCEEDED(result))
    {
      result = (segmentsFragments->Count() > 0) ? result : E_FAIL;
    }

    if (SUCCEEDED(result) && (logCollection))
    {
      wchar_t *segmentFragmentLog = NULL;
      for (unsigned int i = 0; i < segmentsFragments->Count(); i++)
      {
        CAfhsSegmentFragment *segmentFragment = segmentsFragments->GetItem(i);

        wchar_t *temp = FormatString(L"%s%ssegment %u, fragment %u, timestamp: %llu", (i == 0) ? L"" : segmentFragmentLog, (i == 0) ? L"" : L"\n", segmentFragment->GetSegment(), segmentFragment->GetFragment(), segmentFragment->GetFragmentTimestamp());
        FREE_MEM(segmentFragmentLog);
        segmentFragmentLog = temp;
      }

      CHECK_CONDITION_NOT_NULL_EXECUTE(segmentFragmentLog, this->logger->Log(LOGGER_VERBOSE, L"%s: %s: segments and fragments:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_GET_SEGMENTS_FRAGMENTS_FROM_BOOTSTRAP_INFO_BOX_NAME, segmentFragmentLog));
      FREE_MEM(segmentFragmentLog);
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(segmentsFragments));

  return segmentsFragments;
}

/* old implementation */

//CMPUrlSourceSplitter_Protocol_Afhs::CMPUrlSourceSplitter_Protocol_Afhs(CLogger *logger, CParameterCollection *configuration)
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
//  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_AFHS);
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
//  this->receiveDataTimeout = AFHS_RECEIVE_DATA_TIMEOUT_DEFAULT;
//  this->streamLength = 0;
//  this->setLength = false;
//  this->setEndOfStream = false;
//  this->streamTime = 0;
//  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
//  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
//  this->wholeStreamDownloaded = false;
//  this->mainCurlInstance = NULL;
//  this->bootstrapInfoCurlInstance = NULL;
//  this->bytePosition = 0;
//  this->seekingActive = false;
//  this->supressData = false;
//  this->bufferForProcessing = NULL;
//  this->bootstrapInfoBox = NULL;
//  this->segmentsFragments = NULL;
//  this->live = false;
//  this->lastBootstrapInfoRequestTime = 0;
//  this->storeFilePath = NULL;
//  this->lastStoreTime = 0;
//  this->isConnected = false;
//  this->segmentFragmentDownloading = UINT_MAX;
//  this->segmentFragmentProcessing = 0;
//  this->segmentFragmentToDownload = UINT_MAX;
//  this->canCallProcessSegmentsAndFragments = true;
//  this->manifest = NULL;
//  this->cookies = NULL;
//  this->addedHeader = false;
//  this->zeroTimeSegmentFragmentTimestamp = UINT64_MAX;
//
//  this->decryptionHoster = new CAfhsDecryptionHoster(this->logger, this->configurationParameters);
//  if (this->decryptionHoster != NULL)
//  {
//    this->decryptionHoster->LoadPlugins();
//  }
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
//}
//
//CMPUrlSourceSplitter_Protocol_Afhs::~CMPUrlSourceSplitter_Protocol_Afhs()
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//
//  if (this->IsConnected())
//  {
//    this->StopReceivingData();
//  }
//
//  // because segments and fragments can be changed in decryption hoster, collection of segments and fragments
//  // have to be released before decryption hoster
//  FREE_MEM_CLASS(this->segmentsFragments);
//  FREE_MEM_CLASS(this->mainCurlInstance);
//  FREE_MEM_CLASS(this->bootstrapInfoCurlInstance);
//
//  if (this->decryptionHoster != NULL)
//  {
//    this->decryptionHoster->RemoveAllPlugins();
//    FREE_MEM_CLASS(this->decryptionHoster);
//  }
//
//  if (this->storeFilePath != NULL)
//  {
//    DeleteFile(this->storeFilePath);
//  }
//
//  FREE_MEM_CLASS(this->bufferForProcessing);
//  FREE_MEM_CLASS(this->configurationParameters);
//
//  if (this->lockMutex != NULL)
//  {
//    CloseHandle(this->lockMutex);
//    this->lockMutex = NULL;
//  }
//  if (this->lockCurlMutex != NULL)
//  {
//    CloseHandle(this->lockCurlMutex);
//    this->lockCurlMutex = NULL;
//  }
//
//  FREE_MEM_CLASS(this->manifest);
//  FREE_MEM_CLASS(this->bootstrapInfoBox);
//  FREE_MEM(this->storeFilePath);
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
//
//  FREE_MEM_CLASS(this->logger);
//}
//
//// IProtocol interface
//
//bool CMPUrlSourceSplitter_Protocol_Afhs::IsConnected(void)
//{
//  return ((this->isConnected) || (this->wholeStreamDownloaded));
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::ParseUrl(const CParameterCollection *parameters)
//{
//  HRESULT result = S_OK;
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
//  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);
//
//  this->ClearSession();
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
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::ReceiveData(CReceiveData *receiveData)
//{
//  HRESULT result = S_OK;
//
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  // AFHS has always one stream
//  if (this->IsConnected() && (receiveData->SetStreamCount(1)))
//  {
//    if (!this->wholeStreamDownloaded)
//    {
//      bool forceDownload = false;
//
//      if (SUCCEEDED(result) && (this->canCallProcessSegmentsAndFragments) && (this->segmentsFragments->GetFirstNotProcessedSegmentFragment(0) != UINT_MAX))
//      {
//        CAfhsDecryptionContext *context = new CAfhsDecryptionContext();
//        if (context != NULL)
//        {
//          context->SetSegmentFragmentDownloading(this->segmentFragmentDownloading);
//          context->SetSegmentFragmentProcessing(this->segmentFragmentProcessing);
//          context->SetSegmentFragmentToDownload(this->segmentFragmentToDownload);
//          context->SetSegmentsFragments(this->segmentsFragments);
//          context->SetManifestUrl(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_MANIFEST_URL, true, NULL));
//          context->SetManifestContent(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, true, NULL));
//          context->SetForceDownload(false);
//
//          result = this->decryptionHoster->ProcessSegmentsAndFragments(context);
//
//          if (SUCCEEDED(result))
//          {
//            this->segmentFragmentDownloading = context->GetSegmentFragmentDownloading();
//            this->segmentFragmentProcessing = context->GetSegmentFragmentProcessing();
//            this->segmentFragmentToDownload = context->GetSegmentFragmentToDownload();
//            forceDownload = context->GetForceDownload();
//          }
//
//          if (result == S_FALSE)
//          {
//            this->canCallProcessSegmentsAndFragments = false;
//          }
//        }
//        FREE_MEM_CLASS(context);
//      }
//
//      if (SUCCEEDED(result) && (!this->supressData) && (this->segmentFragmentProcessing < this->segmentsFragments->Count()))
//      {
//        CLinearBuffer *bufferForBoxProcessing = this->FillBufferForProcessing(this->segmentsFragments, this->segmentFragmentProcessing, this->storeFilePath);
//        if (bufferForBoxProcessing != NULL)
//        {
//          // buffer successfully filled
//
//          bool continueProcessing = false;
//          do
//          {
//            continueProcessing = false;
//            CBox *box = new CBox();
//            if (box != NULL)
//            {
//              unsigned int length = bufferForBoxProcessing->GetBufferOccupiedSpace();
//              if (length > 0)
//              {
//                ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
//                if (buffer != NULL)
//                {
//                  bufferForBoxProcessing->CopyFromBuffer(buffer, length);
//                  if (box->Parse(buffer, length))
//                  {
//                    unsigned int boxSize = (unsigned int)box->GetSize();
//                    if (length >= boxSize)
//                    {
//                      continueProcessing = true;
//
//                      if (wcscmp(box->GetType(), MEDIA_DATA_BOX_TYPE) == 0)
//                      {
//                        CMediaDataBox *mediaBox = new CMediaDataBox();
//                        if (mediaBox != NULL)
//                        {
//                          continueProcessing &= mediaBox->Parse(buffer, length);
//
//                          if (continueProcessing)
//                          {
//                            unsigned int payloadSize = (unsigned int)mediaBox->GetPayloadSize();
//                            continueProcessing &= (this->bufferForProcessing->AddToBufferWithResize(mediaBox->GetPayload(), payloadSize) == payloadSize);
//                          }
//                        }
//                        FREE_MEM_CLASS(mediaBox);
//                      }
//
//                      //if (wcscmp(box->GetType(), BOOTSTRAP_INFO_BOX_TYPE) == 0)
//                      //{
//                      //  CBootstrapInfoBox *bootstrapInfoBox = new CBootstrapInfoBox();
//                      //  if (bootstrapInfoBox != NULL)
//                      //  {
//                      //    continueProcessing &= bootstrapInfoBox->Parse(buffer, length);
//
//                      //    if (continueProcessing)
//                      //    {
//                      //      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: bootstrap info box:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bootstrapInfoBox->GetParsedHumanReadable(L""));
//                      //    }
//
//                      //    // ignore errors while processing bootstrap info boxes
//                      //    continueProcessing = true;
//                      //  }
//                      //  FREE_MEM_CLASS(bootstrapInfoBox);
//                      //}
//
//                      if (continueProcessing)
//                      {
//                        bufferForBoxProcessing->RemoveFromBuffer(boxSize);
//                        continueProcessing = true;
//                      }
//                    }
//                  }
//                }
//                FREE_MEM(buffer);
//              }
//            }
//            FREE_MEM_CLASS(box);
//          } while (continueProcessing);
//
//          if (bufferForBoxProcessing->GetBufferOccupiedSpace() == 0)
//          {
//            // all data are processed
//            continueProcessing = true;
//          }
//
//          if (continueProcessing)
//          {
//            this->segmentFragmentProcessing++;
//
//            // check if segment and fragment is downloaded
//            // if segment and fragment is not downloaded, then schedule it for download
//
//            if (this->segmentFragmentProcessing < this->segmentsFragments->Count())
//            {
//              CSegmentFragment *segmentFragment = this->segmentsFragments->GetItem(this->segmentFragmentProcessing);
//              if ((!segmentFragment->IsDownloaded()) && (this->segmentFragmentProcessing != this->segmentFragmentDownloading))
//              {
//                // segment and fragment is not downloaded and also is not downloading currently
//                this->segmentFragmentToDownload = this->segmentFragmentProcessing;
//              }
//            }
//          }
//        }
//        FREE_MEM_CLASS(bufferForBoxProcessing);
//      }
//
//      unsigned int lastFlvPacketTimestamp = 0;
//
//      if (SUCCEEDED(result) && (!this->supressData) && (this->bufferForProcessing != NULL))
//      {
//        CFlvPacket *flvPacket = new CFlvPacket();
//        if (flvPacket != NULL)
//        {
//          while (SUCCEEDED(result) && (flvPacket->ParsePacket(this->bufferForProcessing) == FLV_PARSE_RESULT_OK))
//          {
//            // FLV packet parsed correctly
//            // push FLV packet to filter
//
//            //if ((flvPacket->GetType() != FLV_PACKET_HEADER) && (this->firstTimestamp == (-1)))
//            //{
//            //  this->firstTimestamp = flvPacket->GetTimestamp();
//            //  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: set first timestamp: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->firstTimestamp);
//            //}
//
//            //if ((flvPacket->GetType() == FLV_PACKET_VIDEO) && (this->firstVideoTimestamp == (-1)))
//            //{
//            //  this->firstVideoTimestamp = flvPacket->GetTimestamp();
//            //  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: set first video timestamp: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->firstVideoTimestamp);
//            //}
//
//            //if ((flvPacket->GetType() == FLV_PACKET_VIDEO) && (this->firstVideoTimestamp != (-1)) && (this->firstTimestamp != (-1)))
//            //{
//            //  // correction of video timestamps
//            //  flvPacket->SetTimestamp(flvPacket->GetTimestamp() + this->firstTimestamp - this->firstVideoTimestamp);
//            //}
//
//            if ((flvPacket->GetType() == FLV_PACKET_AUDIO) ||
//              (flvPacket->GetType() == FLV_PACKET_HEADER) ||
//              (flvPacket->GetType() == FLV_PACKET_META) ||
//              (flvPacket->GetType() == FLV_PACKET_VIDEO))
//            {
//              // do nothing, known packet types
//              CHECK_CONDITION_HRESULT(result, !flvPacket->IsEncrypted(), result, E_DRM_PROTECTED);
//            }
//            else
//            {
//              this->logger->Log(LOGGER_WARNING, L"%s: %s: unknown FLV packet: %d, size: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, flvPacket->GetType(), flvPacket->GetSize());
//              result = E_UNKNOWN_STREAM_TYPE;
//            }
//
//            if ((flvPacket->GetType() != FLV_PACKET_HEADER) || (!this->seekingActive))
//            {
//              lastFlvPacketTimestamp = flvPacket->GetTimestamp();
//
//              // create media packet
//              // set values of media packet
//              CMediaPacket *mediaPacket = new CMediaPacket();
//              mediaPacket->GetBuffer()->InitializeBuffer(flvPacket->GetSize());
//              mediaPacket->GetBuffer()->AddToBuffer(flvPacket->GetData(), flvPacket->GetSize());
//              mediaPacket->SetStart(this->bytePosition);
//              mediaPacket->SetEnd(this->bytePosition + flvPacket->GetSize() - 1);
//
//              if (!receiveData->GetStreams()->GetItem(0)->GetMediaPacketCollection()->Add(mediaPacket))
//              {
//                FREE_MEM_CLASS(mediaPacket);
//              }
//              this->bytePosition += flvPacket->GetSize();
//            }
//            // we are definitely not seeking
//            this->seekingActive = false;
//            this->bufferForProcessing->RemoveFromBufferAndMove(flvPacket->GetSize());
//
//            flvPacket->Clear();
//          }
//
//          FREE_MEM_CLASS(flvPacket);
//        }
//      }
//
//      if (SUCCEEDED(result))
//      {
//        if ((!this->setLength) && (this->bytePosition != 0))
//        {
//          // adjust total length if not already set
//          if ((!this->live) && (this->manifest->GetDuration()->GetDuration() != F4M_DURATION_NOT_SPECIFIED))
//          {
//            if (lastFlvPacketTimestamp != 0)
//            {
//              // specified duration in manifest
//              this->streamLength = this->bytePosition * this->manifest->GetDuration()->GetDuration() / lastFlvPacketTimestamp;
//              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length (by time): %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//              receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, true);
//            }
//          }
//          else
//          {
//            if (this->streamLength == 0)
//            {
//              // error occured or stream duration is not set
//              // just make guess
//              this->streamLength = LONGLONG(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
//              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//              receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, true);
//            }
//            else if ((this->bytePosition > (this->streamLength * 3 / 4)))
//            {
//              // it is time to adjust stream length, we are approaching to end but still we don't know total length
//              this->streamLength = this->bytePosition * 2;
//              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: adjusting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//              receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, true);
//            }
//          }
//        }
//      }
//
//      if (SUCCEEDED(result) && (this->segmentFragmentProcessing >= this->segmentsFragments->Count()) &&
//        (this->segmentFragmentToDownload == UINT_MAX) &&
//        (this->segmentFragmentDownloading == UINT_MAX) &&
//        (!this->live))
//      {
//        // all segments and fragments downloaded and processed
//        // whole stream downloaded
//        this->wholeStreamDownloaded = true;
//        this->isConnected = false;
//        FREE_MEM_CLASS(this->mainCurlInstance);
//      }
//
//      if (SUCCEEDED(result) && (this->segmentFragmentProcessing >= this->segmentsFragments->Count()) && (!this->live))
//      {
//        // all segments and fragments processed
//        // set stream length and report end of stream
//        if (!this->seekingActive)
//        {
//          // we are not seeking, so we can set total length
//          if (!this->setLength)
//          {
//            this->streamLength = this->bytePosition;
//            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//            receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, false);
//            this->setLength = true;
//          }
//
//          if (!this->setEndOfStream)
//          {
//            // notify filter the we reached end of stream
//            receiveData->GetStreams()->GetItem(0)->GetEndOfStreamReached()->SetStreamPosition(max(0, this->bytePosition - 1));
//            this->setEndOfStream = true;
//          }
//        }
//      }
//
//      if (SUCCEEDED(result) && (this->segmentFragmentDownloading != UINT_MAX) && (this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
//      {
//        // we received data, remember cookies and continue
//        CParameterCollection *mainCurlCookies = this->mainCurlInstance->GetCurrentCookies();
//        if (mainCurlCookies != NULL)
//        {
//          // NULL means error, so at least leave current cookies
//          FREE_MEM_CLASS(this->cookies);
//          this->cookies = mainCurlCookies;
//        }
//
//        if (this->mainCurlInstance->GetHttpDownloadResponse()->GetResultCode() == CURLE_OK)
//        {
//          // in CURL instance is HTTP download response, all data are stored in CURL instance
//          // all data received, we're not receiving data
//          CSegmentFragment *segmentFragment = this->segmentsFragments->GetItem(this->segmentFragmentDownloading);
//          if (segmentFragment->SetHttpDownloadResponse(this->mainCurlInstance->GetHttpDownloadResponse()))
//          {
//            segmentFragment->SetDownloaded(true);
//            this->canCallProcessSegmentsAndFragments = true;
//            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: received all data for url '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetHttpDownloadRequest()->GetUrl());
//
//            // if it is live session, then download first not downloaded segment and fragment after currently processed segment and fragment
//            // can return UINT_MAX if segment and fragment for download doesn't exist (in that case wait for update of bootstrap info)
//            this->segmentFragmentToDownload = (this->live) ? this->segmentsFragments->GetFirstNotDownloadedSegmentFragment(this->segmentFragmentProcessing) : this->segmentFragmentToDownload;
//            // if not set segment and fragment to download, then set segment and fragment to download (get next not downloaded segment and fragment after current downloaded segment and fragment)
//            this->segmentFragmentToDownload = ((!this->live) && (this->segmentFragmentToDownload == UINT_MAX)) ? this->segmentsFragments->GetFirstNotDownloadedSegmentFragment(this->segmentFragmentDownloading) : this->segmentFragmentToDownload;
//            // if not set segment and fragment to download, then set segment and fragment to download (get next not downloaded segment and fragment from first segment and fragment)
//            this->segmentFragmentToDownload = ((!this->live) && (this->segmentFragmentToDownload == UINT_MAX)) ? this->segmentsFragments->GetFirstNotDownloadedSegmentFragment(0) : this->segmentFragmentToDownload;
//
//            // segment and fragment to download still can be UINT_MAX = no segment and fragment to download
//            this->segmentFragmentDownloading = UINT_MAX;
//
//            // do not delete CURL instance
//            // preserve all cookies, referer, user agent
//          }
//        }
//        else
//        {
//          // error occured while downloading
//          // download segment and fragment again or download scheduled segment and fragment
//          this->segmentFragmentToDownload = (this->segmentFragmentToDownload != UINT_MAX) ? this->segmentFragmentDownloading : this->segmentFragmentToDownload;
//          this->segmentFragmentDownloading = UINT_MAX;
//
//          // do not delete CURL instance
//          // preserve all cookies, referer, user agent
//          this->isConnected = false;
//        }
//      }
//
//      if (SUCCEEDED(result) && (this->segmentFragmentDownloading == UINT_MAX) && (this->isConnected) && (forceDownload || (this->segmentsFragments->GetFirstNotProcessedSegmentFragment(0) == UINT_MAX)))
//      {
//        // do not start any download until all downloaded segments and fragments processed
//        // no CURL instance exists, we finished download
//        // start another download
//
//        // if it is live session, then download first not downloaded segment and fragment after currently processed segment and fragment
//        // can return UINT_MAX if segment and fragment for download doesn't exist (in that case wait for update of bootstrap info)
//        this->segmentFragmentToDownload = (this->live) ? this->segmentsFragments->GetFirstNotDownloadedSegmentFragment(this->segmentFragmentProcessing) : this->segmentFragmentToDownload;
//        // if not set segment and fragment to download, then set segment and fragment to download (get next not downloaded segment and fragment after current processed segment and fragment)
//        this->segmentFragmentToDownload = ((!this->live) && (this->segmentFragmentToDownload == UINT_MAX)) ? this->segmentsFragments->GetFirstNotDownloadedSegmentFragment(this->segmentFragmentProcessing) : this->segmentFragmentToDownload;
//        // if not set segment and fragment to download, then set segment and fragment to download (get next not downloaded segment and fragment from first segment and fragment)
//        this->segmentFragmentToDownload = ((!this->live) && (this->segmentFragmentToDownload == UINT_MAX)) ? this->segmentsFragments->GetFirstNotDownloadedSegmentFragment(0) : this->segmentFragmentToDownload;
//        // segment and fragment to download still can be UINT_MAX = no segment and fragment to download
//
//        if (SUCCEEDED(result) && (this->segmentFragmentToDownload != UINT_MAX))
//        {
//          // there is specified segment and fragment to download
//
//          CSegmentFragment *segmentFragment = this->segmentsFragments->GetItem(this->segmentFragmentToDownload);
//          if (segmentFragment != NULL)
//          {
//            HRESULT result = S_OK;
//
//            if (SUCCEEDED(result))
//            {
//              // we need to download for another url
//              if (this->mainCurlInstance == NULL)
//              {
//                this->mainCurlInstance = new CHttpCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
//              }
//              CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_POINTER);
//
//              if (SUCCEEDED(result))
//              {
//                this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
//                this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
//
//                // set current cookies (passed from HTTP CURL manifest instance, bootstrap info or from last download)
//                result = (this->mainCurlInstance->SetCurrentCookies(this->cookies)) ? result : E_FAIL;
//
//                if (segmentFragment->GetHttpDownloadRequest() == NULL)
//                {
//                  result = (segmentFragment->CreateHttpDownloadRequest()) ? result : E_OUTOFMEMORY;
//
//                  if (SUCCEEDED(result))
//                  {
//                    wchar_t *url = this->segmentsFragments->GetSegmentFragmentUrl(segmentFragment);
//                    CHECK_POINTER_HRESULT(result, url, result, E_OUTOFMEMORY);
//
//                    if (SUCCEEDED(result))
//                    {
//                      result = (segmentFragment->GetHttpDownloadRequest()->SetUrl(url)) ? result : E_OUTOFMEMORY;
//
//                      result = (segmentFragment->GetHttpDownloadRequest()->SetReferer(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_REFERER, true, NULL))) ? result : E_OUTOFMEMORY;
//                      result = (segmentFragment->GetHttpDownloadRequest()->SetUserAgent(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_USER_AGENT, true, NULL))) ? result : E_OUTOFMEMORY;
//                      result = (segmentFragment->GetHttpDownloadRequest()->SetCookie(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_COOKIE, true, NULL))) ? result : E_OUTOFMEMORY;
//
//                      segmentFragment->GetHttpDownloadRequest()->SetHttpVersion(this->configurationParameters->GetValueLong(PARAMETER_NAME_AFHS_VERSION, true, HTTP_VERSION_DEFAULT));
//                      segmentFragment->GetHttpDownloadRequest()->SetIgnoreContentLength((this->configurationParameters->GetValueLong(PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
//                    }
//                    FREE_MEM(url);
//                  }
//                }
//
//                if (segmentFragment->GetHttpDownloadResponse() == NULL)
//                {
//                  result = (segmentFragment->CreateHttpDownloadResponse()) ? result : E_OUTOFMEMORY;
//                }
//
//                if (SUCCEEDED(result))
//                {
//                  result = (this->mainCurlInstance->Initialize(segmentFragment->GetHttpDownloadRequest())) ? result : E_FAIL;
//                }
//
//                if (SUCCEEDED(result))
//                {
//                  // all parameters set
//                  // start receiving data
//
//                  result = (this->mainCurlInstance->StartReceivingData()) ? S_OK : E_FAIL;
//
//                  if (SUCCEEDED(result))
//                  {
//                    this->segmentFragmentDownloading = this->segmentFragmentToDownload;
//                    this->segmentFragmentToDownload = UINT_MAX;
//                  }
//                }
//              }
//            }
//          }
//        }
//      }
//
//      if (SUCCEEDED(result) && (this->live))
//      {
//        // in case of live stream we need to download again manifest and parse bootstrap info for new information about stream
//
//        if ((this->bootstrapInfoCurlInstance != NULL) && (this->bootstrapInfoCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
//        {
//          // we received data, remember cookies and continue
//          CParameterCollection *bootstrapInfoCookies = this->bootstrapInfoCurlInstance->GetCurrentCookies();
//          if (bootstrapInfoCookies != NULL)
//          {
//            // NULL means error, so at least leave current cookies
//            FREE_MEM_CLASS(this->cookies);
//            this->cookies = bootstrapInfoCookies;
//          }
//
//          if (this->bootstrapInfoCurlInstance->GetHttpDownloadResponse()->GetResultCode() == CURLE_OK)
//          {
//            unsigned int length = this->bootstrapInfoCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
//            bool continueWithBootstrapInfo = (length > 1);
//
//            if (continueWithBootstrapInfo)
//            {
//              ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
//              continueWithBootstrapInfo &= (buffer != NULL);
//
//              if (continueWithBootstrapInfo)
//              {
//                this->bootstrapInfoCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->CopyFromBuffer(buffer, length);
//
//                CBootstrapInfoBox *bootstrapInfoBox = new CBootstrapInfoBox();
//                continueWithBootstrapInfo &= (bootstrapInfoBox != NULL);
//
//                if (continueWithBootstrapInfo)
//                {
//                  if (bootstrapInfoBox->Parse(buffer, length))
//                  {
//                    //this->logger->Log(LOGGER_VERBOSE, L"%s: %s: new bootstrap info box:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bootstrapInfoBox->GetParsedHumanReadable(L""));
//                    CSegmentFragment *lastSegmentFragment = this->segmentsFragments->GetItem(this->segmentsFragments->Count() - 1);
//
//                    CSegmentFragmentCollection *updateSegmentsFragments = this->GetSegmentsFragmentsFromBootstrapInfoBox(
//                      this->logger,
//                      METHOD_RECEIVE_DATA_NAME,
//                      this->configurationParameters,
//                      bootstrapInfoBox,
//                      false,
//                      lastSegmentFragment->GetFragmentTimestamp());
//                    continueWithBootstrapInfo &= (updateSegmentsFragments != NULL);
//
//                    if (continueWithBootstrapInfo)
//                    {
//                      CSegmentFragment *lastSegmentFragment = this->segmentsFragments->GetItem(this->segmentsFragments->Count() - 1);
//
//                      for (unsigned int i = 0; i < updateSegmentsFragments->Count(); i++)
//                      {
//                        CSegmentFragment *parsedSegmentFragment = updateSegmentsFragments->GetItem(i);
//                        if (parsedSegmentFragment->GetFragment() > lastSegmentFragment->GetFragment())
//                        {
//                          // new segment fragment, add it to be downloaded
//                          CSegmentFragment *clone = new CSegmentFragment(
//                            parsedSegmentFragment->GetSegment(),
//                            parsedSegmentFragment->GetFragment(),
//                            parsedSegmentFragment->GetFragmentTimestamp());
//                          continueWithBootstrapInfo &= (clone != NULL);
//
//                          if (continueWithBootstrapInfo)
//                          {
//                            continueWithBootstrapInfo &= this->segmentsFragments->Add(clone);
//                            if (continueWithBootstrapInfo)
//                            {
//                              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: added new segment and fragment, segment %d, fragment %d, timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, clone->GetSegment(), clone->GetFragment(), clone->GetFragmentTimestamp());
//                            }
//                          }
//
//                          if (!continueWithBootstrapInfo)
//                          {
//                            FREE_MEM_CLASS(clone);
//                          }
//                        }
//                      }
//                    }
//                    else
//                    {
//                      this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"cannot create segments and fragments to download");
//                    }
//
//                    FREE_MEM_CLASS(updateSegmentsFragments);
//                  }
//                  else
//                  {
//                    this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"cannot parse new bootstrap info box");
//                  }
//                }
//                else
//                {
//                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"not enough memory for new bootstrap info box");
//                }
//
//                FREE_MEM_CLASS(bootstrapInfoBox);
//              }
//              else
//              {
//                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"not enough memory for new bootstrap info data");
//              }
//
//              FREE_MEM(buffer);
//            }
//            else
//            {
//              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"too short data downloaded for new bootstrap info");
//            }
//
//            if (continueWithBootstrapInfo)
//            {
//              // everything correct, download next bootstrap info after LAST_REQUEST_BOOTSTRAP_INFO_DELAY seconds
//              this->lastBootstrapInfoRequestTime = GetTickCount();
//            }
//          }
//          else
//          {
//            // error occured while downloading bootstrap info
//            // download bootstrap info again
//            this->lastBootstrapInfoRequestTime = 0;
//          }
//
//          FREE_MEM_CLASS(this->bootstrapInfoCurlInstance);
//        }
//
//        // because of cookies, we cannot download bootstrap info before download in mainCurlInstance is finished
//        if ((this->segmentFragmentDownloading == UINT_MAX) && (this->segmentFragmentToDownload == UINT_MAX) && (this->bootstrapInfoCurlInstance == NULL) && (GetTickCount() > (this->lastBootstrapInfoRequestTime + LAST_REQUEST_BOOTSTRAP_INFO_DELAY)))
//        {
//          // no segment and fragment is downloading
//          // no segment and fragment is scheduled for download
//          // bootstrap info CURL instance doesn't exist
//          // it's time to update bootstrap info
//          const wchar_t *url = this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO_URL, true, NULL);
//          if (url != NULL)
//          {
//            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: live streaming, requesting bootstrap info, url: '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, url);
//
//            this->bootstrapInfoCurlInstance = new CHttpCurlInstance(this->logger, NULL, PROTOCOL_IMPLEMENTATION_NAME, L"BootstrapInfo");
//            bool continueWithBootstrapInfo = (this->bootstrapInfoCurlInstance != NULL);
//
//            if (continueWithBootstrapInfo)
//            {
//              this->bootstrapInfoCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
//              this->bootstrapInfoCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));
//
//              continueWithBootstrapInfo &= this->bootstrapInfoCurlInstance->SetCurrentCookies(this->cookies);
//            }
//
//            if (continueWithBootstrapInfo)
//            {
//              CHttpDownloadRequest *request = new CHttpDownloadRequest();
//              if (request != NULL)
//              {
//                request->SetUrl(url);
//                request->SetReferer(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_REFERER, true, NULL));
//                request->SetUserAgent(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_USER_AGENT, true, NULL));
//                request->SetCookie(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_COOKIE, true, NULL));
//                request->SetHttpVersion(this->configurationParameters->GetValueLong(PARAMETER_NAME_AFHS_VERSION, true, HTTP_VERSION_DEFAULT));
//                request->SetIgnoreContentLength((this->configurationParameters->GetValueLong(PARAMETER_NAME_AFHS_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
//
//                continueWithBootstrapInfo &= this->bootstrapInfoCurlInstance->Initialize(request);
//              }
//              FREE_MEM_CLASS(request);
//            }
//
//            if (continueWithBootstrapInfo)
//            {
//              continueWithBootstrapInfo &= this->bootstrapInfoCurlInstance->StartReceivingData();
//              if (!continueWithBootstrapInfo)
//              {
//                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"cannot start receiving data of new bootstrap info");
//              }
//            }
//            else
//            {
//              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"cannot initialize new bootstrap info download");
//            }
//
//            if (!continueWithBootstrapInfo)
//            {
//              FREE_MEM_CLASS(this->bootstrapInfoCurlInstance);
//            }
//          }
//        }
//      }
//    }
//    else
//    {
//      // set total length (if not set earlier)
//      if (!this->setLength)
//      {
//        this->streamLength = this->bytePosition;
//        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
//        receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->streamLength, false);
//        this->setLength = true;
//      }
//    }
//  }
//
//  // store segments and fragments to temporary file (only when not live session)
//  // remove not needed segments and fragments (in case of live session)
//  if (SUCCEEDED(result) && ((GetTickCount() - this->lastStoreTime) > 1000))
//  {
//    this->lastStoreTime = GetTickCount();
//
//    if ((!this->live) && (this->segmentsFragments->Count() > 0))
//    {
//      // store all segments and fragments (which are not stored) to file
//      if (this->storeFilePath == NULL)
//      {
//        this->storeFilePath = this->GetStoreFile();
//      }
//
//      if (this->storeFilePath != NULL)
//      {
//        LARGE_INTEGER size;
//        size.QuadPart = 0;
//
//        // open or create file
//        HANDLE hTempFile = CreateFile(this->storeFilePath, FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
//
//        if (hTempFile != INVALID_HANDLE_VALUE)
//        {
//          if (!GetFileSizeEx(hTempFile, &size))
//          {
//            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"error while getting size");
//            // error occured while getting file size
//            size.QuadPart = -1;
//          }
//
//          if (size.QuadPart >= 0)
//          {
//            unsigned int i = 0;
//            while (i < this->segmentsFragments->Count())
//            {
//              CSegmentFragment *segmentFragment = this->segmentsFragments->GetItem(i);
//
//              if ((!segmentFragment->IsStoredToFile()) && (segmentFragment->IsProcessed()))
//              {
//                // if segment and fragment is not stored to file
//                // store it to file
//                unsigned int length = segmentFragment->GetLength();
//
//                ALLOC_MEM_DEFINE_SET(buffer, unsigned char, length, 0);
//                if (segmentFragment->GetHttpDownloadResponse()->GetReceivedData()->CopyFromBuffer(buffer, length) == length)
//                {
//                  DWORD written = 0;
//                  if (WriteFile(hTempFile, buffer, length, &written, NULL))
//                  {
//                    if (length == written)
//                    {
//                      // mark as stored
//                      segmentFragment->SetStoredToFile(size.QuadPart);
//                      size.QuadPart += length;
//
//                      // after storing segment and fragment to file some data are not necessary to hold
//                      segmentFragment->FreeHttpDownloadRequest();
//                      segmentFragment->FreeHttpDownloadResponse();
//                    }
//                  }
//                }
//                FREE_MEM(buffer);
//              }
//
//              i++;
//            }
//          }
//
//          CloseHandle(hTempFile);
//          hTempFile = INVALID_HANDLE_VALUE;
//        }
//      }
//    }
//
//    if (this->live)
//    {
//      // in case of live stream remove all downloaded segments and fragments before reported stream time
//      if ((this->segmentsFragments->Count() > 0) && (this->segmentFragmentProcessing != UINT_MAX))
//      {
//        // leave last segment and fragment in collection in order to not add downloaded and processed segments and fragments
//        while ((this->segmentFragmentProcessing > 0) && (this->segmentsFragments->Count() > 1))
//        {
//          CSegmentFragment *segmentFragment = this->segmentsFragments->GetItem(0);
//
//          if (segmentFragment->IsDownloaded() && segmentFragment->IsProcessed() && ((segmentFragment->GetFragmentTimestamp() - this->zeroTimeSegmentFragmentTimestamp) < this->reportedStreamTime))
//          {
//            this->segmentsFragments->Remove(0);
//            this->segmentFragmentProcessing--;
//
//            if (this->segmentFragmentDownloading != UINT_MAX)
//            {
//              this->segmentFragmentDownloading--;
//            }
//            if (this->segmentFragmentToDownload != UINT_MAX)
//            {
//              this->segmentFragmentToDownload--;
//            }
//          }
//          else
//          {
//            break;
//          }
//        }
//      }
//    }
//  }
//
//  return result;
//}
//
//CParameterCollection *CMPUrlSourceSplitter_Protocol_Afhs::GetConnectionParameters(void)
//{
//  CParameterCollection *result = new CParameterCollection();
//
//  if (result != NULL)
//  {
//    if (!result->Append(this->configurationParameters))
//    {
//      FREE_MEM_CLASS(result);
//    }
//  }
//  
//  return result;
//}
//
//// ISimpleProtocol interface
//
//unsigned int CMPUrlSourceSplitter_Protocol_Afhs::GetReceiveDataTimeout(void)
//{
//  return this->receiveDataTimeout;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::StartReceivingData(CParameterCollection *parameters)
//{
//  HRESULT result = S_OK;
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);
//
//  CHECK_POINTER_DEFAULT_HRESULT(result, this->configurationParameters);
//
//  // lock access to stream
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  this->wholeStreamDownloaded = false;
//
//  if (this->cookies == NULL)
//  {
//    this->cookies = new CParameterCollection();
//    CHECK_POINTER_HRESULT(result, this->cookies, result, E_OUTOFMEMORY);
//
//    if (SUCCEEDED(result))
//    {
//      unsigned int currentCookiesCount = this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_AFHS_COOKIES_COUNT, true, 0);
//      if (currentCookiesCount != 0)
//      {
//        for (unsigned int i = 0; (SUCCEEDED(result) & (i < currentCookiesCount)); i++)
//        {
//          wchar_t *cookieName = FormatString(AFHS_COOKIE_FORMAT_PARAMETER_NAME, i);
//          CHECK_POINTER_HRESULT(result, cookieName, result, E_OUTOFMEMORY);
//
//          if (SUCCEEDED(result))
//          {
//            const wchar_t *curlCookieValue = this->configurationParameters->GetValue(cookieName, true, NULL);
//            CHECK_POINTER_HRESULT(result, curlCookieValue, result, E_OUTOFMEMORY);
//
//            if (SUCCEEDED(result))
//            {
//              CParameter *curlCookie = new CParameter(L"", curlCookieValue);
//              CHECK_POINTER_HRESULT(result, curlCookie, result, E_OUTOFMEMORY);
//
//              if (SUCCEEDED(result))
//              {
//                result = (this->cookies->Add(curlCookie)) ? result : E_FAIL;
//              }
//
//              if (FAILED(result))
//              {
//                FREE_MEM_CLASS(curlCookie);
//              }
//            }
//          }
//
//          FREE_MEM(cookieName);
//        }
//      }
//    }
//  }
//
//  if (this->segmentsFragments == NULL)
//  {
//    char *bootstrapInfoBase64Encoded = ConvertToMultiByteW(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_BOOTSTRAP_INFO, true, NULL));
//    CHECK_POINTER_HRESULT(result, bootstrapInfoBase64Encoded, result, E_POINTER);
//
//    if (SUCCEEDED(result))
//    {
//      // bootstrap info is BASE64 encoded
//      unsigned char *bootstrapInfo = NULL;
//      unsigned int bootstrapInfoLength = 0;
//
//      result = base64_decode(bootstrapInfoBase64Encoded, &bootstrapInfo, &bootstrapInfoLength);
//
//      if (SUCCEEDED(result))
//      {
//        FREE_MEM_CLASS(this->bootstrapInfoBox);
//        this->bootstrapInfoBox = new CBootstrapInfoBox();
//        CHECK_POINTER_HRESULT(result, this->bootstrapInfoBox, result, E_OUTOFMEMORY);
//
//        if (SUCCEEDED(result))
//        {
//          result = (this->bootstrapInfoBox->Parse(bootstrapInfo, bootstrapInfoLength)) ? result : HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//
//          /*if (SUCCEEDED(result))
//          {
//            wchar_t *parsedBootstrapInfoBox = this->bootstrapInfoBox->GetParsedHumanReadable(L"");
//            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: parsed bootstrap info:\n%s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, parsedBootstrapInfoBox);
//            FREE_MEM(parsedBootstrapInfoBox);
//          }*/
//
//          if (FAILED(result))
//          {
//            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot parse bootstrap info box");
//          }
//        }
//      }
//      else
//      {
//        this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, L"cannot decode bootstrap info");
//      }
//    }
//
//    // check if we have manifest
//    CHECK_POINTER_HRESULT(result, this->manifest, result, E_FAIL);
//
//    if (SUCCEEDED(result))
//    {
//      // we have bootstrap info box successfully parsed
//      this->live = this->bootstrapInfoBox->IsLive();
//      uint64_t currentMediaTime = (this->bootstrapInfoBox->GetCurrentMediaTime() > 0) ? (this->bootstrapInfoBox->GetCurrentMediaTime() - 1): 0;
//
//      FREE_MEM_CLASS(this->segmentsFragments);
//      this->segmentsFragments = this->GetSegmentsFragmentsFromBootstrapInfoBox(
//        this->logger,
//        METHOD_START_RECEIVING_DATA_NAME,
//        this->configurationParameters,
//        this->bootstrapInfoBox,
//        false,
//        this->live ? currentMediaTime : 0);
//      CHECK_POINTER_HRESULT(result, this->segmentsFragments, result, E_POINTER);
//
//      if (this->live)
//      {
//        // in case of live stream check current media time and choose right segment and fragment
//        // this download one fragment before current media time
//
//        // find segment and fragment to process
//        result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//        if (this->segmentsFragments != NULL)
//        {
//          for (unsigned int i = 0; i < this->segmentsFragments->Count(); i++)
//          {
//            CSegmentFragment *segFrag = this->segmentsFragments->GetItem(i);
//
//            if (segFrag->GetFragmentTimestamp() <= currentMediaTime)
//            {
//              this->segmentFragmentProcessing = i;
//              result = S_OK;
//            }
//          }
//        }
//      }
//    }
//  }
//
//  if (this->zeroTimeSegmentFragmentTimestamp == UINT64_MAX)
//  {
//    // zero time timestamp is not set
//    // we are starting receiving data, we must set timestamp for zero time (because we are in zero time)
//
//    CSegmentFragment *segFrag = this->segmentsFragments->GetItem(this->segmentFragmentProcessing);
//
//    this->zeroTimeSegmentFragmentTimestamp = segFrag->GetFragmentTimestamp();
//  }
//
//  if (SUCCEEDED(result) && (this->bufferForProcessing == NULL))
//  {
//    this->bufferForProcessing = new CLinearBuffer();
//    CHECK_POINTER_HRESULT(result, this->bufferForProcessing, result, E_OUTOFMEMORY);
//
//    if (SUCCEEDED(result))
//    {
//      result = (this->bufferForProcessing->InitializeBuffer(MINIMUM_RECEIVED_DATA_FOR_SPLITTER)) ? result : E_FAIL;
//
//      if ((SUCCEEDED(result)) && (!this->addedHeader))
//      {
//        this->bufferForProcessing->AddToBuffer(FLV_FILE_HEADER, FLV_FILE_HEADER_LENGTH);
//
//        char *mediaMetadataBase64Encoded = ConvertToMultiByteW(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_MEDIA_METADATA, true, NULL));
//        if (mediaMetadataBase64Encoded != NULL)
//        {
//          // metadata can be in connection parameters, but it is optional
//          // metadata is BASE64 encoded
//          unsigned char *metadata = NULL;
//          unsigned int metadataLength = 0;
//          result = base64_decode(mediaMetadataBase64Encoded, &metadata, &metadataLength);
//
//          if (SUCCEEDED(result))
//          {
//            // create FLV packet from metadata and add its content to buffer for processing
//            CFlvPacket *metadataFlvPacket = new CFlvPacket();
//            CHECK_POINTER_HRESULT(result, metadataFlvPacket, result, E_OUTOFMEMORY);
//
//            if (SUCCEEDED(result))
//            {
//              result = metadataFlvPacket->CreatePacket(FLV_PACKET_META, metadata, metadataLength, (unsigned int)this->segmentsFragments->GetItem(0)->GetFragmentTimestamp(), false) ? result : E_FAIL;
//
//              if (SUCCEEDED(result))
//              {
//                result = (this->bufferForProcessing->AddToBufferWithResize(metadataFlvPacket->GetData(), metadataFlvPacket->GetSize()) == metadataFlvPacket->GetSize()) ? result : E_FAIL;
//
//                if (FAILED(result))
//                {
//                  this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"cannot add FLV metadata packet to buffer");
//                }
//              }
//              else
//              {
//                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"cannot create FLV metadata packet");
//              }
//            }
//            else
//            {
//              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"not enough memory for FLV metadata packet");
//            }
//            FREE_MEM_CLASS(metadataFlvPacket);
//          }
//          else
//          {
//            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"cannot decode metadata");
//          }
//          FREE_MEM(metadata);
//        }
//        FREE_MEM(mediaMetadataBase64Encoded);
//
//        if (SUCCEEDED(result))
//        {
//          addedHeader = true;
//        }
//      }
//    }
//  }
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
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::StopReceivingData(void)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
//
//  // lock access to stream
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  FREE_MEM_CLASS(this->mainCurlInstance);
//  FREE_MEM_CLASS(this->bootstrapInfoCurlInstance);
//
//  FREE_MEM_CLASS(this->bufferForProcessing);
//  this->isConnected = false;
//  this->segmentFragmentDownloading = UINT_MAX;
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
//  return S_OK;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::QueryStreamProgress(CStreamProgress *streamProgress)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
//  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);
//
//  if (SUCCEEDED(result))
//  {
//    streamProgress->SetTotalLength((this->streamLength == 0) ? 1 : this->streamLength);
//    streamProgress->SetCurrentLength((this->streamLength == 0) ? 0 : this->bytePosition);
//
//    if (!this->setLength)
//    {
//      result = VFW_S_ESTIMATED;
//    }
//  }
//
//  return result;
//}
//  
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
//{
//  HRESULT result = S_OK;
//  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);
//  CHECK_CONDITION_HRESULT(result, availableLength->GetStreamId() == 0, result, E_INVALIDARG);
//
//  if (SUCCEEDED(result))
//  {
//    availableLength->SetAvailableLength((this->setLength) ? this->streamLength : this->bytePosition);
//  }
//
//  return result;
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::ClearSession(void)
//{
//  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//
//  if (this->IsConnected())
//  {
//    this->StopReceivingData();
//  }
//
//  // clear all decryption plugins session
//  this->decryptionHoster->ClearSession();
//
//  if (this->storeFilePath != NULL)
//  {
//    DeleteFile(this->storeFilePath);
//  }
//
//  FREE_MEM_CLASS(this->bufferForProcessing);
// 
//  this->streamLength = 0;
//  this->setLength = false;
//  this->setEndOfStream = false;
//  this->streamTime = 0;
//  this->wholeStreamDownloaded = false;
//  this->receiveDataTimeout = AFHS_RECEIVE_DATA_TIMEOUT_DEFAULT;
//  this->bytePosition = 0;
//  FREE_MEM_CLASS(this->bootstrapInfoBox);
//  FREE_MEM_CLASS(this->segmentsFragments);
//  this->live = false;
//  this->lastBootstrapInfoRequestTime = 0;
//  FREE_MEM(this->storeFilePath);
//  this->isConnected = false;
//  this->segmentFragmentDownloading = UINT_MAX;
//  this->segmentFragmentProcessing = 0;
//  this->segmentFragmentToDownload = UINT_MAX;
//  this->canCallProcessSegmentsAndFragments = true;
//  this->addedHeader = false;
//  FREE_MEM_CLASS(this->manifest);
//  FREE_MEM_CLASS(this->cookies);
//  this->seekingActive = false;
//  this->zeroTimeSegmentFragmentTimestamp = UINT64_MAX;
//
//  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
//  return S_OK;
//}
//
//int64_t CMPUrlSourceSplitter_Protocol_Afhs::GetDuration(void)
//{
//  return DURATION_UNSPECIFIED;
//}
//
//void CMPUrlSourceSplitter_Protocol_Afhs::ReportStreamTime(uint64_t streamTime)
//{
//  this->reportedStreamTime = streamTime;
//}
//
//// ISeeking interface
//
//unsigned int CMPUrlSourceSplitter_Protocol_Afhs::GetSeekingCapabilities(void)
//{
//  return SEEKING_METHOD_TIME;
//}
//
//int64_t CMPUrlSourceSplitter_Protocol_Afhs::SeekToTime(int64_t time)
//{
//  CLockMutex lock(this->lockMutex, INFINITE);
//
//  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
//  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);
//
//  int64_t result = -1;
//
//  // AFHS protocol can seek to ms
//  // time is in ms
//
//  // find segment and fragment to process
//  if (this->segmentsFragments != NULL)
//  {
//    for (unsigned int i = 0; i < this->segmentsFragments->Count(); i++)
//    {
//      CSegmentFragment *segFrag = this->segmentsFragments->GetItem(i);
//
//      if (segFrag->GetFragmentTimestamp() <= (uint64_t)time)
//      {
//        this->segmentFragmentProcessing = i;
//        result = segFrag->GetFragmentTimestamp();
//      }
//    }
//  }
//
//  this->seekingActive = true;
//  this->bytePosition = 0;
//  this->streamLength = 0;
//  this->setLength = false;
//  this->setEndOfStream = false;
//  // reset whole stream downloaded, but IsConnected() must return true to avoid calling StartReceivingData()
//  this->isConnected = true;
//  this->wholeStreamDownloaded = false;
//
//  // in this->segmentFragmentProcessing is id of segment and fragment to process
//  CSegmentFragment *segFrag = this->segmentsFragments->GetItem(this->segmentFragmentProcessing);
//
//  if (segFrag != NULL)
//  {
//    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: segment %d, fragment %d, timestamp %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME,
//      segFrag->GetSegment(), segFrag->GetFragment(), segFrag->GetFragmentTimestamp());
//
//    if (!segFrag->IsDownloaded())
//    {
//      // close connection
//      this->StopReceivingData();
//
//      // clear segment and fragment to download
//      this->segmentFragmentToDownload = UINT_MAX;
//
//      // reopen connection
//      // StartReceivingData() reset wholeStreamDownloaded
//      this->isConnected = SUCCEEDED(this->StartReceivingData(NULL));
//    }
//  }
//  else
//  {
//    this->isConnected = false;
//  }
//
//  if (!this->IsConnected())
//  {
//    result = -1;
//  }
//  else
//  {
//    this->streamTime = result;
//  }
//
//  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
//  return result;
//}
//
//void CMPUrlSourceSplitter_Protocol_Afhs::SetSupressData(bool supressData)
//{
//  this->supressData = supressData;
//}
//
//// IPlugin interface
//
//const wchar_t *CMPUrlSourceSplitter_Protocol_Afhs::GetName(void)
//{
//  return PROTOCOL_NAME;
//}
//
//GUID CMPUrlSourceSplitter_Protocol_Afhs::GetInstanceId(void)
//{
//  return this->logger->GetLoggerInstanceId();
//}
//
//HRESULT CMPUrlSourceSplitter_Protocol_Afhs::Initialize(PluginConfiguration *configuration)
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
//  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_AFHS_RECEIVE_DATA_TIMEOUT, true, AFHS_RECEIVE_DATA_TIMEOUT_DEFAULT);
//
//  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? AFHS_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
//
//  HRESULT result = S_OK;
//  FREE_MEM_CLASS(this->manifest);
//  char *manifestContent = ConvertToMultiByteW(this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, true, NULL));
//  CHECK_POINTER_HRESULT(result, (((manifestContent != NULL) && (this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, true, NULL) != NULL)) || (this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, true, NULL) == NULL)), result, E_CONVERT_STRING_ERROR);
//
//  if (SUCCEEDED(result) && (this->configurationParameters->GetValue(PARAMETER_NAME_AFHS_MANIFEST_CONTENT, true, NULL) != NULL))
//  {
//    this->manifest = new CF4MManifest();
//    CHECK_POINTER_HRESULT(result, this->manifest, result, E_OUTOFMEMORY);
//
//    if (SUCCEEDED(result))
//    {
//      result = (this->manifest->Parse(manifestContent)) ? result : E_FAIL;
//    }
//  }
//  FREE_MEM(manifestContent);
//
//  if (FAILED(result))
//  {
//    FREE_MEM_CLASS(this->manifest);
//  }
//
//  if (SUCCEEDED(result))
//  {
//    ALLOC_MEM_DEFINE_SET(decryptionPluginConfiguration, AfhsDecryptionPluginConfiguration, 1, 0);
//    CHECK_POINTER_HRESULT(result, decryptionPluginConfiguration, result, E_OUTOFMEMORY);
//
//    if (SUCCEEDED(result))
//    {
//      decryptionPluginConfiguration->configuration = this->configurationParameters;
//      result = this->decryptionHoster->Initialize(decryptionPluginConfiguration);
//    }
//
//    FREE_MEM(decryptionPluginConfiguration);
//  }
//
//  return result;
//}
//
//// other methods
//
//CSegmentFragmentCollection *CMPUrlSourceSplitter_Protocol_Afhs::GetSegmentsFragmentsFromBootstrapInfoBox(CLogger *logger, const wchar_t *methodName, CParameterCollection *configurationParameters, CBootstrapInfoBox *bootstrapInfoBox, bool logCollection, uint64_t lastSegmentFragmentTimestamp)
//{
//  HRESULT result = S_OK;
//  CSegmentFragmentCollection *segmentsFragments = NULL;
//
//  if (SUCCEEDED(result))
//  {
//    // now choose from bootstrap info -> QualityEntryTable highest quality (if exists) with segment run
//    wchar_t *quality = NULL;
//    CSegmentRunEntryCollection *segmentRunEntryTable = NULL;
//
//    for (unsigned int i = 0; ((i <= bootstrapInfoBox->GetQualityEntryTable()->Count()) && (segmentRunEntryTable == NULL)); i++)
//    {
//      FREE_MEM(quality);
//
//      // choose quality only for valid indexes, in another case is quality NULL
//      if (i != bootstrapInfoBox->GetQualityEntryTable()->Count())
//      {
//        CBootstrapInfoQualityEntry *bootstrapInfoQualityEntry = bootstrapInfoBox->GetQualityEntryTable()->GetItem(0);
//        quality = Duplicate(bootstrapInfoQualityEntry->GetQualityEntry());
//      }
//
//      // from segment run table choose segment with specifed quality (if exists) or segment with QualityEntryCount equal to zero
//      for (unsigned int i = 0; i < bootstrapInfoBox->GetSegmentRunTable()->Count(); i++)
//      {
//        CSegmentRunTableBox *segmentRunTableBox = bootstrapInfoBox->GetSegmentRunTable()->GetItem(i);
//
//        if (quality != NULL)
//        {
//          if (segmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(quality, false))
//          {
//            segmentRunEntryTable = segmentRunTableBox->GetSegmentRunEntryTable();
//          }
//        }
//        else
//        {
//          if ((segmentRunTableBox->GetQualitySegmentUrlModifiers()->Count() == 0) || (segmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(L"", false)))
//          {
//            segmentRunEntryTable = segmentRunTableBox->GetSegmentRunEntryTable();
//          }
//        }
//      }
//    }
//
//    if (segmentRunEntryTable == NULL)
//    {
//      logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, methodName, L"cannot find any segment run table");
//      result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//    }
//
//    if (SUCCEEDED(result))
//    {
//      if (segmentRunEntryTable->Count() == 0)
//      {
//        logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, methodName, L"cannot find any segment run entry");
//        result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//      }
//    }
//
//    // from fragment run table choose fragment with specifed quality (if exists) or fragment with QualityEntryCount equal to zero
//    CFragmentRunEntryCollection *fragmentRunEntryTableTemp = NULL;
//    unsigned int timeScale = 0;
//    for (unsigned int i = 0; i < bootstrapInfoBox->GetFragmentRunTable()->Count(); i++)
//    {
//      CFragmentRunTableBox *fragmentRunTableBox = bootstrapInfoBox->GetFragmentRunTable()->GetItem(i);
//
//      if (quality != NULL)
//      {
//        if (fragmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(quality, false))
//        {
//          fragmentRunEntryTableTemp = fragmentRunTableBox->GetFragmentRunEntryTable();
//          timeScale = fragmentRunTableBox->GetTimeScale();
//        }
//      }
//      else
//      {
//        if ((fragmentRunTableBox->GetQualitySegmentUrlModifiers()->Count() == 0) || (fragmentRunTableBox->GetQualitySegmentUrlModifiers()->Contains(L"", false)))
//        {
//          fragmentRunEntryTableTemp = fragmentRunTableBox->GetFragmentRunEntryTable();
//          timeScale = fragmentRunTableBox->GetTimeScale();
//        }
//      }
//    }
//
//    if (fragmentRunEntryTableTemp == NULL)
//    {
//      logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, methodName, L"cannot find any fragment run table");
//      result = HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
//    }
//
//    wchar_t *serverBaseUrl = Duplicate(configurationParameters->GetValue(PARAMETER_NAME_AFHS_BASE_URL, true, L""));
//    for (unsigned int i = 0; i < bootstrapInfoBox->GetServerEntryTable()->Count(); i++)
//    {
//      CBootstrapInfoServerEntry *serverEntry = bootstrapInfoBox->GetServerEntryTable()->GetItem(i);
//      if (!IsNullOrEmptyOrWhitespace(serverEntry->GetServerEntry()))
//      {
//        FREE_MEM(serverBaseUrl);
//        serverBaseUrl = Duplicate(serverEntry->GetServerEntry());
//      }
//    }
//    CHECK_POINTER_HRESULT(result, serverBaseUrl, result, E_OUTOFMEMORY);
//
//    wchar_t *mediaPartUrl = NULL;
//    wchar_t *baseUrl = NULL;
//    //wchar_t *movieIdentifierUrl = NULL;
//    wchar_t *qualityUrl = NULL;
//
//    if (SUCCEEDED(result))
//    {
//      mediaPartUrl = Duplicate(configurationParameters->GetValue(PARAMETER_NAME_AFHS_MEDIA_PART_URL, true, L""));
//      CHECK_POINTER_HRESULT(result, mediaPartUrl, result, E_OUTOFMEMORY);
//    }
//
//    if (SUCCEEDED(result))
//    {
//      baseUrl = FormatAbsoluteUrl(serverBaseUrl, mediaPartUrl);
//      CHECK_POINTER_HRESULT(result, baseUrl, result, E_OUTOFMEMORY);
//    }
//    
//    if (SUCCEEDED(result))
//    {
//      //movieIdentifierUrl = FormatAbsoluteUrl(baseUrl, this->bootstrapInfoBox->GetMovieIdentifier());
//      //CHECK_POINTER_HRESULT(result, movieIdentifierUrl, result, E_OUTOFMEMORY);
//    }
//        
//    if (SUCCEEDED(result))
//    {
//      //qualityUrl = FormatString(L"%s%s", movieIdentifierUrl, (quality == NULL) ? L"" : quality);
//      qualityUrl = FormatAbsoluteUrl(baseUrl, (quality == NULL) ? L"" : quality);
//      CHECK_POINTER_HRESULT(result, qualityUrl, result, E_OUTOFMEMORY);
//    }
//          
//    if (SUCCEEDED(result))
//    {
//      segmentsFragments = new CSegmentFragmentCollection();
//      CHECK_POINTER_HRESULT(result, segmentsFragments, result, E_OUTOFMEMORY);
//    }
//
//    if (SUCCEEDED(result))
//    {
//      result = (segmentsFragments->SetBaseUrl(qualityUrl)) ? result : E_OUTOFMEMORY;
//      result = (segmentsFragments->SetSegmentFragmentUrlExtraParameters(configurationParameters->GetValue(PARAMETER_NAME_AFHS_SEGMENT_FRAGMENT_URL_EXTRA_PARAMETERS, true, NULL))) ? result : E_OUTOFMEMORY;
//    }
//            
//    if (SUCCEEDED(result))
//    {
//      // convert segment run entry table and fragment run entry table to segments and fragments
//
//      unsigned int segmentRunEntryIndex = 0;  // holds index in segmentRunEntryTable
//
//      unsigned int firstSegmentInRun = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFirstSegment();
//      unsigned int lastSegmentInRun = (((segmentRunEntryIndex + 1) < segmentRunEntryTable->Count()) ? segmentRunEntryTable->GetItem(segmentRunEntryIndex + 1)->GetFirstSegment() : UINT_MAX);
//      unsigned int fragmentsPerSegment = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFragmentsPerSegment();   // if UINT_MAX, then current segment is forever
//      unsigned int fragmentInSegment = 0;     // holds fragment in current segment, if it reach fragmentsPerSegment, the segment must be changed
//
//      unsigned int segmentNumber = firstSegmentInRun;     // holds segment number for final segment and fragment
//
//      for (unsigned int i = 0; i < fragmentRunEntryTableTemp->Count(); i++)
//      {
//        CFragmentRunEntry *fragmentRunEntryTemp = fragmentRunEntryTableTemp->GetItem(i);
//        unsigned int nextItemIndex = i + 1;
//        CFragmentRunEntry *fragmentRunEntryTempNext = NULL;
//
//        for (unsigned int j = nextItemIndex; j < fragmentRunEntryTableTemp->Count(); j++)
//        {
//          CFragmentRunEntry *temp = fragmentRunEntryTableTemp->GetItem(nextItemIndex);
//          if (temp->GetFirstFragment() != 0)
//          {
//            fragmentRunEntryTempNext = temp;
//            break;
//          }
//          else
//          {
//            nextItemIndex++;
//          }
//        }
//
//        uint64_t fragmentTimestamp = fragmentRunEntryTemp->GetFirstFragmentTimestamp();
//        unsigned int lastFragment = (fragmentRunEntryTempNext == NULL) ? (fragmentRunEntryTemp->GetFirstFragment() + 1) : fragmentRunEntryTempNext->GetFirstFragment();
//        uint64_t lastFragmentTimestamp = (fragmentRunEntryTempNext == NULL) ? (fragmentRunEntryTemp->GetFirstFragmentTimestamp() + fragmentRunEntryTemp->GetFragmentDuration() - 1) : (fragmentRunEntryTempNext->GetFirstFragmentTimestamp() - 1);
//
//        // in some special live session can be some fragments with zero duration
//        // these fragments can get HTTP 503 error, which disables playback
//        // just skip these fragments and continue with next fragment
//        if ((lastFragmentTimestamp >= lastSegmentFragmentTimestamp) && (fragmentRunEntryTemp->GetFragmentDuration() != 0))
//        {
//          // current fragment run entry has at least some timestamps greater than requested segment and fragment timestamp (lastSegmentFragmentTimestamp)
//          unsigned int fragmentIndex = (unsigned int)((lastFragmentTimestamp - fragmentRunEntryTemp->GetFirstFragmentTimestamp()) / fragmentRunEntryTemp->GetFragmentDuration());
//
//          fragmentTimestamp += fragmentIndex * fragmentRunEntryTemp->GetFragmentDuration();
//          fragmentIndex += fragmentRunEntryTemp->GetFirstFragment();
//
//          while (SUCCEEDED(result) && (fragmentIndex < lastFragment))
//          {
//            CSegmentFragment *segFrag = new CSegmentFragment(segmentNumber, fragmentIndex, fragmentTimestamp * 1000 / timeScale);
//            CHECK_POINTER_HRESULT(result, segFrag, result, E_OUTOFMEMORY);
//
//            if (SUCCEEDED(result))
//            {
//              result = (segmentsFragments->Add(segFrag)) ? result : E_OUTOFMEMORY;
//            }
//
//            if (FAILED(result))
//            {
//              FREE_MEM_CLASS(segFrag);
//            }
//
//            fragmentTimestamp += fragmentRunEntryTemp->GetFragmentDuration();
//            fragmentIndex++;
//          }
//        }
//        
//        // update segment information if necessary
//        unsigned int fragmentCount = lastFragment - fragmentRunEntryTemp->GetFirstFragment();
//        while (fragmentCount != 0)
//        {
//          unsigned int fragmentsToEndOfSegment = min(fragmentsPerSegment - fragmentInSegment, fragmentCount);
//          fragmentCount -= fragmentsToEndOfSegment;
//          fragmentInSegment += fragmentsToEndOfSegment;
//
//          if (fragmentInSegment == fragmentsPerSegment)
//          {
//            // we reached end of segment, update segment information
//            if ((++segmentNumber) >= lastSegmentInRun)
//            {
//              // we reached end of segment run, we must update firstSegmentInRun, lastSegmentInRun, fragmentsPerSegment
//              segmentRunEntryIndex++;
//
//              firstSegmentInRun = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFirstSegment();
//              lastSegmentInRun = (((segmentRunEntryIndex + 1) < segmentRunEntryTable->Count()) ? segmentRunEntryTable->GetItem(segmentRunEntryIndex + 1)->GetFirstSegment() : UINT_MAX);
//              fragmentsPerSegment = segmentRunEntryTable->GetItem(segmentRunEntryIndex)->GetFragmentsPerSegment();   // if UINT_MAX, then current segment is forever
//            }
//
//            fragmentInSegment = 0;
//          }
//        }
//      }
//    }
//
//    FREE_MEM(serverBaseUrl);
//    FREE_MEM(mediaPartUrl);
//    FREE_MEM(baseUrl);
//    //FREE_MEM(movieIdentifierUrl);
//    FREE_MEM(qualityUrl);
//
//    FREE_MEM(quality);
//
//    if (SUCCEEDED(result))
//    {
//      result = (segmentsFragments->Count() > 0) ? result : E_FAIL;
//    }
//
//    if (SUCCEEDED(result) && (logCollection))
//    {
//      wchar_t *segmentFragmentLog = NULL;
//      for (unsigned int i = 0; i < segmentsFragments->Count(); i++)
//      {
//        CSegmentFragment *segmentFragment = segmentsFragments->GetItem(i);
//
//        wchar_t *temp = FormatString(L"%s%ssegment %u, fragment %u, timestamp: %llu", (i == 0) ? L"" : segmentFragmentLog, (i == 0) ? L"" : L"\n", segmentFragment->GetSegment(), segmentFragment->GetFragment(), segmentFragment->GetFragmentTimestamp());
//        FREE_MEM(segmentFragmentLog);
//        segmentFragmentLog = temp;
//      }
//
//      if (segmentFragmentLog != NULL)
//      {
//        logger->Log(LOGGER_VERBOSE, L"%s: %s: segments and fragments:\n%s", PROTOCOL_IMPLEMENTATION_NAME, methodName, segmentFragmentLog);
//      }
//
//      FREE_MEM(segmentFragmentLog);
//    }
//  }
//
//  if (FAILED(result))
//  {
//    FREE_MEM_CLASS(segmentsFragments);
//  }
//
//  return segmentsFragments;
//}
//
//wchar_t *CMPUrlSourceSplitter_Protocol_Afhs::GetStoreFile(void)
//{
//  wchar_t *result = NULL;
//  const wchar_t *folder = this->configurationParameters->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);
//
//  if (folder != NULL)
//  {
//    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
//    if (guid != NULL)
//    {
//      result = FormatString(L"%smpurlsourcesplitter_protocol_afhs_%s.temp", folder, guid);
//    }
//    FREE_MEM(guid);
//  }
//
//  return result;
//}
//
//CLinearBuffer *CMPUrlSourceSplitter_Protocol_Afhs::FillBufferForProcessing(CSegmentFragmentCollection *segmentsFragments, unsigned int segmentFragmentProcessing, wchar_t *storeFile)
//{
//  CLinearBuffer *result = NULL;
//
//  if (segmentsFragments != NULL)
//  {
//    if (segmentFragmentProcessing < segmentsFragments->Count())
//    {
//      CSegmentFragment *segmentFragment = segmentsFragments->GetItem(segmentFragmentProcessing);
//
//      if (segmentFragment->IsProcessed())
//      {
//        // segment and fragment is downloaded and processed
//        // segment and fragment can be stored in memory or in store file
//
//        // temporary buffer for data (from store file or from memory)
//        unsigned int bufferLength = segmentFragment->GetLength();
//        ALLOC_MEM_DEFINE_SET(buffer, unsigned char, bufferLength, 0);
//
//        if (buffer != NULL)
//        {
//          if ((segmentFragment->IsStoredToFile()) && (storeFile != NULL))
//          {
//            // segment and fragment is stored into file and store file is specified
//
//            LARGE_INTEGER size;
//            size.QuadPart = 0;
//
//            // open or create file
//            HANDLE hTempFile = CreateFile(storeFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
//
//            if (hTempFile != INVALID_HANDLE_VALUE)
//            {
//              bool error = false;
//
//              LONG distanceToMoveLow = (LONG)(segmentFragment->GetStoreFilePosition());
//              LONG distanceToMoveHigh = (LONG)(segmentFragment->GetStoreFilePosition() >> 32);
//              LONG distanceToMoveHighResult = distanceToMoveHigh;
//              DWORD setFileResult = SetFilePointer(hTempFile, distanceToMoveLow, &distanceToMoveHighResult, FILE_BEGIN);
//              if (setFileResult == INVALID_SET_FILE_POINTER)
//              {
//                DWORD lastError = GetLastError();
//                if (lastError != NO_ERROR)
//                {
//                  this->logger->Log(LOGGER_ERROR, L"%s: %s: error occured while setting position: %lu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_FOR_PROCESSING_NAME, lastError);
//                  error = true;
//                }
//              }
//
//              if (!error)
//              {
//                DWORD read = 0;
//                if (ReadFile(hTempFile, buffer, bufferLength, &read, NULL) == 0)
//                {
//                  this->logger->Log(LOGGER_ERROR, L"%s: %s: error occured reading file: %lu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_FOR_PROCESSING_NAME, GetLastError());
//                  FREE_MEM(buffer);
//                }
//                else if (read != bufferLength)
//                {
//                  this->logger->Log(LOGGER_WARNING, L"%s: %s: readed data length not same as requested, requested: %u, readed: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_FOR_PROCESSING_NAME, bufferLength, read);
//                  FREE_MEM(buffer);
//                }
//              }
//
//              CloseHandle(hTempFile);
//              hTempFile = INVALID_HANDLE_VALUE;
//            }
//          }
//          else if (!segmentFragment->IsStoredToFile())
//          {
//            // segment and fragment is stored in memory
//            if (segmentFragment->GetHttpDownloadResponse()->GetReceivedData()->CopyFromBuffer(buffer, bufferLength) != bufferLength)
//            {
//              // error occured while copying data
//              FREE_MEM(buffer);
//            }
//          }
//        }
//
//        if (buffer != NULL)
//        {
//          // all data are read
//          bool correct = false;
//          result = new CLinearBuffer();
//          if (result != NULL)
//          {
//            if (result->InitializeBuffer(bufferLength))
//            {
//              if (result->AddToBuffer(buffer, bufferLength) == bufferLength)
//              {
//                // everything correct, data copied successfully
//                correct = true;
//              }
//            }
//          }
//
//          if (!correct)
//          {
//            FREE_MEM_CLASS(result);
//          }
//        }
//
//        // clean-up buffer
//        FREE_MEM(buffer);
//      }
//    }
//  }
//
//  return result;
//}