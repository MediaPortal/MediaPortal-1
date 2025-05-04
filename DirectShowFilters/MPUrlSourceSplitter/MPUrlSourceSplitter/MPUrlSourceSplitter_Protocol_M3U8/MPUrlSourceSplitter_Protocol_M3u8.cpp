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

#include "MPUrlSourceSplitter_Protocol_M3u8.h"
#include "MPUrlSourceSplitter_Protocol_M3u8_Parameters.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "Parameters.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"
#include "MediaPlaylistFactory.h"
#include "compress_zlib.h"
#include "base64.h"
#include "formatUrl.h"
#include "M3u8DecryptionPluginConfiguration.h"

#include <WinInet.h>
#include <stdio.h>
#include <Shlwapi.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_M3u8d"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_M3u8"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_M3u8(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_M3u8 *protocol = (CMPUrlSourceSplitter_Protocol_M3u8 *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_M3u8::CMPUrlSourceSplitter_Protocol_M3u8(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
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
  this->streamFragmentZeroTimestamp = 0;
  this->lastMediaPlaylistUpdateTime = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;
  this->decryptionHoster = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->streamFragments = new CM3u8StreamFragmentCollection(result);
    this->decryptionHoster = new CM3u8DecryptionHoster(result, logger, configuration);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->decryptionHoster, *result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(*result), this->decryptionHoster->LoadPlugins(), *result);

    // create CURL instance
    this->mainCurlInstance = new CM3u8CurlInstance(result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
    CHECK_POINTER_HRESULT(*result, this->mainCurlInstance, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8);
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

CMPUrlSourceSplitter_Protocol_M3u8::~CMPUrlSourceSplitter_Protocol_M3u8()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  // because stream fragments can be changed in decryption hoster, collection of stream fragments have to be released before decryption hoster
  // in other case it can lead to access violation exception, due to virtual function table is allocated in memory space of decryption hoster
  // same for CURL instance
  FREE_MEM_CLASS(this->streamFragments);
  FREE_MEM_CLASS(this->mainCurlInstance);

  FREE_MEM_CLASS(this->decryptionHoster);

  FREE_MEM_CLASS(this->cacheFile);

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

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_M3u8::GetConnectionState(void)
{
  return this->mainCurlInstance->GetConnectionState();
}

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::ReceiveData(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    LOCK_MUTEX(this->lockMutex, INFINITE)
      
    if (SUCCEEDED(result) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_UPDATE_STREAM_FRAGMENTS)) && (this->mainCurlInstance->IsLockedCurlInstanceByOwner(this)) && (this->mainCurlInstance->GetConnectionState() == Opening))
    {
      // in case of updating stream fragments (live stream) we don't set Opened state
      // Opened state will be set after receiving all data to reset protocol hoster timeout
      // after reseting protocol timeout will be connection closed and new stream fragment(s) will be donwloaded

      unsigned int bufferSize = 0;
      {
        // only check received data length to not block Load() method
        LOCK_MUTEX(this->lockCurlMutex, INFINITE)

        bufferSize = this->mainCurlInstance->GetM3u8DownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();

        UNLOCK_MUTEX(this->lockCurlMutex)
      }

      if (bufferSize > 0)
      {
        this->mainCurlInstance->SetConnectionState(Opened);
      }
    }

    if (SUCCEEDED(result) && (this->streamFragments->HasEncryptedStreamFragments()))
    {
      CM3u8DecryptionContext *decryptionContext = new CM3u8DecryptionContext(&result);
      CHECK_CONDITION_HRESULT(result, decryptionContext, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        decryptionContext->SetCurlInstance(this->mainCurlInstance);
        decryptionContext->SetStreamFragmentDownloading(this->streamFragmentDownloading);
        decryptionContext->SetStreamFragmentProcessing(this->streamFragmentProcessing);
        decryptionContext->SetStreamFragmentToDownload(this->streamFragmentToDownload);
        decryptionContext->SetStreamFragments(this->streamFragments);
        decryptionContext->SetPlaylistUrl(this->configuration->GetValue(PARAMETER_NAME_M3U8_PLAYLIST_URL, true, NULL));
        decryptionContext->SetConfiguration(this->configuration);
      }

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->decryptionHoster->DecryptStreamFragments(decryptionContext), result);
      CHECK_CONDITION_EXECUTE(FAILED(result), this->logger->Log(LOGGER_ERROR, L"%s: %s: decryption of stream fragments failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, result));
      FREE_MEM_CLASS(decryptionContext);
    }

    if (SUCCEEDED(result) && (this->streamFragments->HasDecryptedStreamFragments()))
    {
      // some stream fragments are decrypted

      CIndexedM3u8StreamFragmentCollection *indexedDecryptedStreamFragments = new CIndexedM3u8StreamFragmentCollection(&result);
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->streamFragments->GetDecryptedStreamFragments(indexedDecryptedStreamFragments));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < indexedDecryptedStreamFragments->Count())); i++)
      {
        CIndexedM3u8StreamFragment *indexedDecryptedStreamFragment = indexedDecryptedStreamFragments->GetItem(i);
        CM3u8StreamFragment *currentDecryptingFragment = indexedDecryptedStreamFragment->GetItem();

        // we can process stream fragment
        // after processing we mark fragment as downloaded = it is ready for filter

        if (SUCCEEDED(result))
        {
          currentDecryptingFragment->SetLoadedToMemoryTime(GetTickCount(), UINT_MAX);
          currentDecryptingFragment->SetDecrypted(false, UINT_MAX);
          currentDecryptingFragment->SetProcessed(true, UINT_MAX);

          this->streamFragments->UpdateIndexes(indexedDecryptedStreamFragment->GetItemIndex(), 1);

          // recalculate start position of all processed stream fragments until first not processed stream fragment
          this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(indexedDecryptedStreamFragment->GetItemIndex());
        }
      }

      FREE_MEM_CLASS(indexedDecryptedStreamFragments);
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance->GetConnectionState() == Initializing) && (!this->IsWholeStreamDownloaded()) && (!this->mainCurlInstance->IsLockedCurlInstance()) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_UPDATE_STREAM_FRAGMENTS)))
    {
      if (this->streamFragments->Count() == 0)
      {
        unsigned char *decoded = NULL;
        unsigned int decodedLength = 0;

        const char *encoded = ConvertToMultiByteW(this->configuration->GetValue(PARAMETER_NAME_M3U8_PLAYLIST_CONTENT, true, NULL));
        CHECK_POINTER_HRESULT(result, encoded, result, E_CONVERT_STRING_ERROR);

        if (SUCCEEDED(result))
        {
          // media playlist is BASE64 encoded
          result = base64_decode(encoded, &decoded, &decodedLength);

          if (SUCCEEDED(result))
          {
            // media playlist is compressed

            wchar_t *decompressed = NULL;
            unsigned int decompressedLength = 0;

            result = decompress_zlib(decoded, decodedLength, (uint8_t **)&decompressed, &decompressedLength);

            if (SUCCEEDED(result))
            {
              CMediaPlaylistFactory *factory = new CMediaPlaylistFactory(&result);
              CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                CMediaPlaylist *tempPlaylist = factory->CreateMediaPlaylist(&result, decompressed, wcslen(decompressed));

                if (SUCCEEDED(result))
                {
                  const wchar_t *baseUrl = this->configuration->GetValue(PARAMETER_NAME_M3U8_PLAYLIST_URL, true, NULL);

                  CM3u8StreamFragmentCollection *parsedStreamFragments = this->GetStreamFragmentsFromMediaPlaylist(baseUrl, tempPlaylist, this->streamFragments);
                  CHECK_POINTER_HRESULT(result, parsedStreamFragments, result, E_M3U8_CANNOT_GET_STREAM_FRAGMENTS_FROM_MEDIA_PLAYLIST);

                  CHECK_CONDITION_HRESULT(result, this->streamFragments->Append(parsedStreamFragments), result, E_OUTOFMEMORY);

                  // check last stream fragment for end of stream flag
                  CM3u8StreamFragment *fragment = this->streamFragments->GetItem(this->streamFragments->Count() - 1);

                  this->flags |= ((fragment != NULL) && fragment->IsEndOfStream()) ? PROTOCOL_PLUGIN_FLAG_NONE : PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED;

                  FREE_MEM_CLASS(parsedStreamFragments);
                }

                FREE_MEM_CLASS(tempPlaylist);
              }

              FREE_MEM_CLASS(factory);
            }

            FREE_MEM(decompressed);
          }
        }

        FREE_MEM(decoded);

        this->streamFragmentProcessing = 0;

        if (SUCCEEDED(result) && this->IsLiveStreamDetected())
        {
          if (SUCCEEDED(result))
          {
            // set start searching index to current processing stream fragment
            this->streamFragments->SetStartSearchingIndex(this->streamFragmentProcessing);
            // set count of fragments to search for specific position
            unsigned int firstNotDownloadedFragmentIndex = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentProcessing);
            this->streamFragments->SetSearchCount(((firstNotDownloadedFragmentIndex == UINT_MAX) ? this->streamFragments->Count() : firstNotDownloadedFragmentIndex) - this->streamFragmentProcessing);

            this->streamFragmentToDownload = 0;
          }
        }

        if (this->streamFragments->Count() != 0)
        {
          this->streamFragments->GetItem(0)->SetFragmentStartPosition(0);

          // set stream fragment zero timestamp as first steram fragment timestamp
          this->streamFragmentZeroTimestamp = this->streamFragments->GetItem(0)->GetFragmentTimestamp();
        }
      }

      // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
      this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentProcessing) : this->streamFragmentToDownload;
      // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
      this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0) : this->streamFragmentToDownload;
      // fragment to download still can be UINT_MAX = no fragment to download

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
        if (this->IsDumpInputData() || this->IsDumpOutputData())
        {
          wchar_t *storeFilePath = this->GetDumpFile();
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
          FREE_MEM(storeFilePath);

          this->mainCurlInstance->SetDumpInputData(this->IsDumpInputData());
          this->mainCurlInstance->SetDumpOutputData(this->IsDumpOutputData());
        }
      }

      if (SUCCEEDED(result) && (this->streamFragmentToDownload != UINT_MAX))
      {
        CM3u8DownloadRequest *request = new CM3u8DownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

          CM3u8StreamFragment *fragment = this->streamFragments->GetItem(this->streamFragmentToDownload);
          if (fragment == NULL)
          {
            result = E_M3U8_NOT_VALID_ITEM_FOUND;
          }
          else
          {
            // clear fragment buffer
            fragment->GetBuffer()->ClearBuffer();

            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: starting receiving data for fragment: %u, timestamp: %lld, original timestamp: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragment(), fragment->GetFragmentTimestamp() - this->streamFragmentZeroTimestamp, fragment->GetFragmentTimestamp());

            CHECK_CONDITION_HRESULT(result, request->SetUrl(fragment->GetUri()), result, E_OUTOFMEMORY);
            CHECK_CONDITION_HRESULT(result, request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL)), result, E_OUTOFMEMORY);
            request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_HTTP_VERSION, true, HTTP_VERSION_DEFAULT));
            request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
            CHECK_CONDITION_HRESULT(result, request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL)), result, E_OUTOFMEMORY);
            CHECK_CONDITION_HRESULT(result, request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL)), result, E_OUTOFMEMORY);

            if (this->configuration->GetValueBool(PARAMETER_NAME_HTTP_SERVER_AUTHENTICATE, true, HTTP_SERVER_AUTHENTICATE_DEFAULT))
            {
              const wchar_t *serverUserName = this->configuration->GetValue(PARAMETER_NAME_HTTP_SERVER_USER_NAME, true, NULL);
              const wchar_t *serverPassword = this->configuration->GetValue(PARAMETER_NAME_HTTP_SERVER_PASSWORD, true, NULL);

              CHECK_POINTER_HRESULT(result, serverUserName, result, E_AUTH_NO_SERVER_USER_NAME);
              CHECK_POINTER_HRESULT(result, serverUserName, result, E_AUTH_NO_SERVER_PASSWORD);

              CHECK_CONDITION_HRESULT(result, request->SetAuthentication(true, serverUserName, serverPassword), result, E_OUTOFMEMORY);
            }

            if (this->configuration->GetValueBool(PARAMETER_NAME_HTTP_PROXY_SERVER_AUTHENTICATE, true, HTTP_PROXY_SERVER_AUTHENTICATE_DEFAULT))
            {
              const wchar_t *proxyServer = this->configuration->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER, true, NULL);
              const wchar_t *proxyServerUserName = this->configuration->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER_USER_NAME, true, NULL);
              const wchar_t *proxyServerPassword = this->configuration->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER_PASSWORD, true, NULL);
              unsigned short proxyServerPort = (unsigned short)this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_PROXY_SERVER_PORT, true, HTTP_PROXY_SERVER_PORT_DEFAULT);
              unsigned int proxyServerType = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_PROXY_SERVER_TYPE, true, HTTP_PROXY_SERVER_TYPE_DEFAULT);

              CHECK_POINTER_HRESULT(result, proxyServer, result, E_AUTH_NO_PROXY_SERVER);
              CHECK_POINTER_HRESULT(result, proxyServerUserName, result, E_AUTH_NO_SERVER_USER_NAME);
              CHECK_POINTER_HRESULT(result, proxyServerPassword, result, E_AUTH_NO_SERVER_PASSWORD);

              CHECK_CONDITION_HRESULT(result, request->SetProxyAuthentication(true, proxyServer, proxyServerPort, proxyServerType, proxyServerUserName, proxyServerPassword), result, E_OUTOFMEMORY);
            }

            if ((fragment->GetByteRangeOffset() != UINT_MAX) && (fragment->GetByteRangeLength() != UINT_MAX))
            {
              request->SetStartPosition((uint64_t)fragment->GetByteRangeOffset());
              request->SetEndPosition((uint64_t)(fragment->GetByteRangeOffset() + fragment->GetByteRangeLength() - 1));
            }

            if (SUCCEEDED(this->mainCurlInstance->LockCurlInstance(this)))
            {
              // apply cookies

              unsigned int cookiesCount = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_COOKIES_COUNT, true, 0);

              if (cookiesCount != 0)
              {
                CParameterCollection *cookies = new CParameterCollection(&result);
                CHECK_POINTER_HRESULT(result, cookies, result, E_OUTOFMEMORY);

                for (unsigned int i = 0; (SUCCEEDED(result) && (i < cookiesCount)); i++)
                {
                  wchar_t *httpCookieName = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
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
                  wchar_t *httpCookieName = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
                  CHECK_POINTER_HRESULT(result, httpCookieName, result, E_OUTOFMEMORY);

                  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->configuration->Remove(httpCookieName, true));
                  FREE_MEM(httpCookieName);
                }

                this->configuration->Remove(PARAMETER_NAME_HTTP_COOKIES_COUNT, true);
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
        }

        FREE_MEM_CLASS(request);
      }
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance->GetConnectionState() == Initializing) && (!this->IsWholeStreamDownloaded()) && (!this->mainCurlInstance->IsLockedCurlInstance()) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_UPDATE_STREAM_FRAGMENTS)))
    {
      // wait time between updating media playlist should be half of time between two stream fragments
      unsigned int waitTime = UINT_MAX;

      CM3u8StreamFragment *lastFragment = this->streamFragments->GetItem(this->streamFragments->Count() - 1);
      CM3u8StreamFragment *previousFragment = this->streamFragments->GetItem(this->streamFragments->Count() - 2);

      if ((lastFragment != NULL) && (previousFragment != NULL))
      {
        waitTime = ((unsigned int)min(INT32_MAX, lastFragment->GetFragmentTimestamp() - previousFragment->GetFragmentTimestamp())) / 2;
      }

      if (waitTime < (GetTickCount() - this->lastMediaPlaylistUpdateTime))
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

        const wchar_t *mediaPlaylistUrl = this->configuration->GetValue(PARAMETER_NAME_M3U8_PLAYLIST_URL, true, NULL);
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: starting downloading media playlist, url: '%s'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, mediaPlaylistUrl);

        if (SUCCEEDED(result))
        {
          if (this->IsDumpInputData() || this->IsDumpOutputData())
          {
            wchar_t *storeFilePath = this->GetDumpFile();
            CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
            FREE_MEM(storeFilePath);

            this->mainCurlInstance->SetDumpInputData(this->IsDumpInputData());
            this->mainCurlInstance->SetDumpOutputData(this->IsDumpOutputData());
          }
        }

        if (SUCCEEDED(result))
        {
          CM3u8DownloadRequest *request = new CM3u8DownloadRequest(&result);
          CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            // set finish time, all methods must return before finish time
            request->SetFinishTime(finishTime);
            request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER));
            request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

            CHECK_CONDITION_HRESULT(result, request->SetUrl(mediaPlaylistUrl), result, E_OUTOFMEMORY);
            CHECK_CONDITION_HRESULT(result, request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL)), result, E_OUTOFMEMORY);
            request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_HTTP_VERSION, true, HTTP_VERSION_DEFAULT));
            request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
            CHECK_CONDITION_HRESULT(result, request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL)), result, E_OUTOFMEMORY);
            CHECK_CONDITION_HRESULT(result, request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL)), result, E_OUTOFMEMORY);

            if (this->configuration->GetValueBool(PARAMETER_NAME_HTTP_SERVER_AUTHENTICATE, true, HTTP_SERVER_AUTHENTICATE_DEFAULT))
            {
              const wchar_t *serverUserName = this->configuration->GetValue(PARAMETER_NAME_HTTP_SERVER_USER_NAME, true, NULL);
              const wchar_t *serverPassword = this->configuration->GetValue(PARAMETER_NAME_HTTP_SERVER_PASSWORD, true, NULL);

              CHECK_POINTER_HRESULT(result, serverUserName, result, E_AUTH_NO_SERVER_USER_NAME);
              CHECK_POINTER_HRESULT(result, serverUserName, result, E_AUTH_NO_SERVER_PASSWORD);

              CHECK_CONDITION_HRESULT(result, request->SetAuthentication(true, serverUserName, serverPassword), result, E_OUTOFMEMORY);
            }

            if (this->configuration->GetValueBool(PARAMETER_NAME_HTTP_PROXY_SERVER_AUTHENTICATE, true, HTTP_PROXY_SERVER_AUTHENTICATE_DEFAULT))
            {
              const wchar_t *proxyServer = this->configuration->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER, true, NULL);
              const wchar_t *proxyServerUserName = this->configuration->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER_USER_NAME, true, NULL);
              const wchar_t *proxyServerPassword = this->configuration->GetValue(PARAMETER_NAME_HTTP_PROXY_SERVER_PASSWORD, true, NULL);
              unsigned short proxyServerPort = (unsigned short)this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_PROXY_SERVER_PORT, true, HTTP_PROXY_SERVER_PORT_DEFAULT);
              unsigned int proxyServerType = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_PROXY_SERVER_TYPE, true, HTTP_PROXY_SERVER_TYPE_DEFAULT);

              CHECK_POINTER_HRESULT(result, proxyServer, result, E_AUTH_NO_PROXY_SERVER);
              CHECK_POINTER_HRESULT(result, proxyServerUserName, result, E_AUTH_NO_SERVER_USER_NAME);
              CHECK_POINTER_HRESULT(result, proxyServerPassword, result, E_AUTH_NO_SERVER_PASSWORD);

              CHECK_CONDITION_HRESULT(result, request->SetProxyAuthentication(true, proxyServer, proxyServerPort, proxyServerType, proxyServerUserName, proxyServerPassword), result, E_OUTOFMEMORY);
            }

            if (SUCCEEDED(this->mainCurlInstance->LockCurlInstance(this)))
            {
              if (SUCCEEDED(this->mainCurlInstance->Initialize(request)))
              {
                // all parameters set
                // start receiving data

                if (SUCCEEDED(this->mainCurlInstance->StartReceivingData()))
                {
                  this->mainCurlInstance->SetConnectionState(Opening);
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
    }

    if (SUCCEEDED(result) && (!this->IsWholeStreamDownloaded()) && (this->mainCurlInstance->IsLockedCurlInstanceByOwner(this)) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    {
      if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_UPDATE_STREAM_FRAGMENTS))
      {
        // media playlist received, check for error
        this->lastMediaPlaylistUpdateTime = GetTickCount();

        if (SUCCEEDED(this->mainCurlInstance->GetM3u8DownloadResponse()->GetResultError()))
        {
          // successfully downloaded media playlist
          unsigned int mediaPlaylistSize = this->mainCurlInstance->GetM3u8DownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();

          if (this->mainCurlInstance->GetConnectionState() == Opening)
          {
            if (mediaPlaylistSize > 0)
            {
              // we ignore zero length media playlist, in that case we request media playlist again

              ALLOC_MEM_DEFINE_SET(mediaPlaylistBuffer, unsigned char, mediaPlaylistSize + 1, 0);
              CHECK_CONDITION_HRESULT(result, mediaPlaylistBuffer, result, E_OUTOFMEMORY);

              if (SUCCEEDED(result))
              {
                this->mainCurlInstance->GetM3u8DownloadResponse()->GetReceivedData()->CopyFromBuffer(mediaPlaylistBuffer, mediaPlaylistSize);

                CMediaPlaylistFactory *factory = new CMediaPlaylistFactory(&result);
                CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

                wchar_t *tempMediaPlaylistBuffer = ConvertToUnicodeA((char *)mediaPlaylistBuffer);
                CHECK_POINTER_HRESULT(result, factory, result, E_OUTOFMEMORY);

                if (SUCCEEDED(result))
                {
                  CMediaPlaylist *tempPlaylist = factory->CreateMediaPlaylist(&result, tempMediaPlaylistBuffer, wcslen(tempMediaPlaylistBuffer));

                  if (SUCCEEDED(result))
                  {
                    const wchar_t *baseUrl = this->configuration->GetValue(PARAMETER_NAME_M3U8_PLAYLIST_URL, true, NULL);

                    CM3u8StreamFragmentCollection *parsedStreamFragments = this->GetStreamFragmentsFromMediaPlaylist(baseUrl, tempPlaylist, this->streamFragments);
                    CHECK_POINTER_HRESULT(result, parsedStreamFragments, result, E_M3U8_CANNOT_GET_STREAM_FRAGMENTS_FROM_MEDIA_PLAYLIST);

                    CHECK_CONDITION_HRESULT(result, this->streamFragments->Append(parsedStreamFragments), result, E_OUTOFMEMORY);

                    for (unsigned int i = 0; (SUCCEEDED(result) && (i < parsedStreamFragments->Count())); i++)
                    {
                      this->mainCurlInstance->SetConnectionState(Opened);
                      CM3u8StreamFragment *fragment = parsedStreamFragments->GetItem(i);

                      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->logger->Log(LOGGER_VERBOSE, L"%s: %s: added new stream fragment, fragment %u, timestamp: %lld, duration: %u (ms)", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragment(), fragment->GetFragmentTimestamp(), fragment->GetDuration()));
                    }

                    FREE_MEM_CLASS(parsedStreamFragments);
                  }

                  FREE_MEM_CLASS(tempPlaylist);
                }

                FREE_MEM(tempMediaPlaylistBuffer);
                FREE_MEM_CLASS(factory);
              }

              FREE_MEM(mediaPlaylistBuffer);
            }

            if (this->mainCurlInstance->GetConnectionState() == Opening)
            {
              // no new stream fragment, download playlist again
              // stop receiving data and restart downloading of stream fragments
              this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
            }
          }
          else if (this->mainCurlInstance->GetConnectionState() == Opened)
          {
            // new stream fragments were successfully added
            // protocol hoster timeout was reseted

            // just download new stream fragments
            this->flags &= ~MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_UPDATE_STREAM_FRAGMENTS;

            // stop receiving data and restart downloading of stream fragments
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
          }
          else
          {
            // stop receiving data and restart downloading of stream fragments
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
          }
        }
        else
        {
          // stop receiving data and restart downloading of stream fragments
          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
        }
      }
      else
      {
        // all data received, we're not receiving data
        // check end of stream or error on M3U8 connection

        if (SUCCEEDED(this->mainCurlInstance->GetM3u8DownloadResponse()->GetResultError()))
        {
          CM3u8StreamFragment *currentDownloadingFragment = this->streamFragments->GetItem(this->streamFragmentDownloading);

          CHECK_CONDITION_HRESULT(result, currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetM3u8DownloadResponse()->GetReceivedData()) == this->mainCurlInstance->GetM3u8DownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace(), result, E_OUTOFMEMORY);

          // segment fragment is by default encrypted, until decryptor sets state to decrypted
          if (SUCCEEDED(result))
          {
            currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
            currentDownloadingFragment->SetEncrypted(true, UINT_MAX);

            this->streamFragments->UpdateIndexes(this->streamFragmentDownloading, 1);
          }

          if (SUCCEEDED(result))
          {
            if (!this->IsLiveStreamDetected())
            {
              // check if we downloaded last stream fragment - then we set end of stream reached - it doesn't mean that we have all data (no gaps)

              if ((!this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED)) && (this->streamFragments->Count() != 0))
              {
                // not set end of stream, at least one stream fragment
                CM3u8StreamFragment *lastFragment = this->streamFragments->GetItem(this->streamFragments->Count() - 1);

                if (lastFragment->IsDownloaded())
                {
                  this->logger->Log(LOGGER_INFO, L"%s: %s: end of stream reached", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
                  this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

                  // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
                  this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentDownloading) : this->streamFragmentToDownload;
                  // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
                  this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0) : this->streamFragmentToDownload;
                  // fragment to download still can be UINT_MAX = no fragment to download
                }
              }

              // request to download next segment fragment after current downloaded fragment
              this->streamFragmentToDownload = (this->streamFragmentToDownload != UINT_MAX) ? this->streamFragmentToDownload : this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentDownloading + 1);
              this->streamFragmentDownloading = UINT_MAX;

              if (this->streamFragmentToDownload == UINT_MAX)
              {
                // no stream fragment to download, we have all data
                this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
              }

              this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE;
              this->flags |= (this->streamFragmentToDownload != UINT_MAX) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA : MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_NONE;
            }
            else
            {
              // live stream, check if we downloaded last stream fragment
              // if yes, then download new playlist and get new stream fragments or check if stream fragment has end of stream flag set

              if ((!this->IsSetFlags(PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED)) && (this->streamFragments->Count() != 0))
              {
                // at least one stream fragment
                CM3u8StreamFragment *lastFragment = this->streamFragments->GetItem(this->streamFragments->Count() - 1);

                if (lastFragment->IsDownloaded())
                {
                  if (lastFragment->IsEndOfStream())
                  {
                    this->logger->Log(LOGGER_INFO, L"%s: %s: live stream, downloaded last stream fragment, end of stream reached", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
                    this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
                  }
                  else
                  {
                    this->logger->Log(LOGGER_INFO, L"%s: %s: live stream, downloaded last stream fragment, requesting media playlist for update of stream fragments", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
                    this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_UPDATE_STREAM_FRAGMENTS | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
                  }

                  this->streamFragmentToDownload = UINT_MAX;
                  this->streamFragmentDownloading = UINT_MAX;
                }
                else
                {
                  // request to download next segment fragment after current downloaded fragment
                  this->streamFragmentToDownload = (this->streamFragmentToDownload != UINT_MAX) ? this->streamFragmentToDownload : this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentDownloading + 1);
                  this->streamFragmentDownloading = UINT_MAX;

                  CHECK_CONDITION_HRESULT(result, this->streamFragmentToDownload != UINT_MAX, result, E_M3U8_NO_STREAM_FRAGMENT_TO_DOWNLOAD);

                  this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE;
                  this->flags |= (this->streamFragmentToDownload != UINT_MAX) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA : MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_NONE;
                }
              }
            }
          }
        }
        else
        {
          // error occured while receiving data

          if (this->IsLiveStreamDetected())
          {
            // download stream fragment again or download scheduled stream fragment
            CM3u8StreamFragment *fragment = (this->streamFragmentDownloading > 0) ? this->streamFragments->GetItem(this->streamFragmentDownloading - 1) : NULL;

            if (fragment != NULL)
            {
              // we report discontinuity (if possible)
              fragment->SetDiscontinuity(true, this->streamFragmentDownloading - 1);
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, timestamp: %llu, position: %lld, size: %u, current position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragmentTimestamp(), fragment->GetFragmentStartPosition(), fragment->GetLength(), this->GetBytePosition());
            }

            this->streamFragmentToDownload = this->streamFragmentDownloading;
            this->streamFragmentDownloading = UINT_MAX;

            // error while receiving data, stop receiving data
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
          }
          else
          {
            // we can re-open connection exactly on lost position (time)

            // download stream fragment again or download scheduled stream fragment
            this->streamFragmentToDownload = this->streamFragmentDownloading;
            this->streamFragmentDownloading = UINT_MAX;

            // error while receiving data, stop receiving data
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
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
      // stream length can be set only in case when all fragments are processed

      bool allFragmentsProcessed = true;

      for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
      {
        CM3u8StreamFragment *fragment = this->streamFragments->GetItem(i);

        allFragmentsProcessed &= fragment->IsProcessed();
      }

      if (allFragmentsProcessed)
      {
        this->streamLength = this->GetBytePosition();
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);

        this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
        this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

        CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE);
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

        int64_t startPosition = dataRequest->GetStart() + foundDataLength;
        unsigned int fragmentIndex = this->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);

        while (fragmentIndex != UINT_MAX)
        {
          this->streamFragmentProcessing = fragmentIndex;

          // get stream fragment
          CM3u8StreamFragment *streamFragment = this->streamFragments->GetItem(fragmentIndex);
          CM3u8StreamFragment *startSearchingStreamFragment = this->streamFragments->GetItem(this->streamFragments->GetStartSearchingIndex());

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
            }
            else if (this->IsEndOfStreamReached() && ((dataRequest->GetStart() + dataRequest->GetLength()) >= this->streamLength))
            {
              // we are not receiving more data, complete request
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetLength(), this->streamLength);

              dataResponse->SetNoMoreDataAvailable(true);
            }

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
            CM3u8StreamFragment *fragment = this->streamFragments->GetItem(fragmentIndex);

            if ((!this->IsLiveStreamDetected()) && (fragment == NULL))
            {
              // bad, no such fragment exists, we don't have data
              // if live stream, we must update segment and fragments

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', not found stream fragment", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              streamPackage->SetCompleted(E_NO_MORE_DATA_AVAILABLE);
            }

            if ((!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_UPDATE_STREAM_FRAGMENTS)) && (fragment != NULL) && (!fragment->IsDownloaded()) && (fragmentIndex != this->streamFragmentDownloading) && ((this->mainCurlInstance->GetConnectionState() == None) || (this->mainCurlInstance->GetConnectionState() == Opened)))
            {
              // fragment is not downloaded and also is not downloading currently
              this->streamFragmentDownloading = UINT_MAX;
              this->streamFragmentToDownload = fragmentIndex;

              this->logger->Log(LOGGER_ERROR, L"%s: %s: request '%u', requesting data from '%lld' to '%lld', stream fragment not downloaded and not downloading, scheduled for download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, dataRequest->GetId(), dataRequest->GetStart(), dataRequest->GetStart() + dataRequest->GetLength());

              if (this->mainCurlInstance->GetConnectionState() == Opened)
              {
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
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

    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE) && (this->mainCurlInstance->IsLockedCurlInstanceByOwner(this) || (!this->mainCurlInstance->IsLockedCurlInstance())))
    {
      HRESULT res = S_OK;
      this->mainCurlInstance->SetConnectionState(Closing);

      if (this->mainCurlInstance != NULL)
      {
        res = this->mainCurlInstance->StopReceivingData();

        CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_INFO, L"%s: %s: closing connection failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, res));
        CHECK_CONDITION_EXECUTE(SUCCEEDED(res), this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->ClearBuffer());
      }

      if ((res == S_OK) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA)))
      {
        // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
        this->StopReceivingData();
      }

      if (res == S_OK)
      {
        this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"connection closed successfully");
        this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA);

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
        unsigned int fragmentRemoveStart = (this->streamFragments->GetStartSearchingIndex() == 0) ? 1 : 0;
        unsigned int fragmentRemoveCount = 0;

        if (this->IsDownloading() && (this->reportedStreamPosition > 0))
        {
          // in case of downloading stream remove all downloaded and processed stream fragments before reported stream position
          // leave at least 3 stream fragments (one is start searching stream fragment, the last two are needed to compute waiting time for playlist)

          while (((fragmentRemoveStart + fragmentRemoveCount) < this->streamFragmentProcessing) && ((fragmentRemoveStart + fragmentRemoveCount + 3) < this->streamFragments->Count()))
          {
            CM3u8StreamFragment *fragment = this->streamFragments->GetItem(fragmentRemoveStart + fragmentRemoveCount);

            if (((fragmentRemoveStart + fragmentRemoveCount) != this->streamFragments->GetStartSearchingIndex()) && fragment->IsProcessed() && ((fragment->GetFragmentStartPosition() + (int64_t)fragment->GetLength()) < (int64_t)this->reportedStreamPosition))
            {
              // fragment will be removed
              fragmentRemoveCount++;
            }
            else
            {
              break;
            }
          }
        }
        else if (this->IsLiveStream() && (this->reportedStreamTime > 0))
        {
          // in case of live stream remove all downloaded and processed stream fragments before reported stream time
          // leave at least 3 stream fragments (one is start searching stream fragment, the last two are needed to compute waiting time for playlist)

          while (((fragmentRemoveStart + fragmentRemoveCount) < this->streamFragmentProcessing) && ((fragmentRemoveStart + fragmentRemoveCount + 3) < this->streamFragments->Count()))
          {
            CM3u8StreamFragment *fragment = this->streamFragments->GetItem(fragmentRemoveStart + fragmentRemoveCount);

            if (((fragmentRemoveStart + fragmentRemoveCount) != this->streamFragments->GetStartSearchingIndex()) && fragment->IsProcessed() && ((fragment->GetFragmentTimestamp() - this->streamFragmentZeroTimestamp) < (int64_t)this->reportedStreamTime))
            {
              // fragment will be removed
              fragmentRemoveCount++;
            }
            else
            {
              break;
            }
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

        if (this->cacheFile->GetCacheFile() == NULL)
        {
          wchar_t *storeFilePath = this->GetCacheFile(NULL);
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }

        // store all stream fragments (which are not stored) to file
        if ((this->cacheFile->GetCacheFile() != NULL) && (this->streamFragments->Count() != 0) && (this->streamFragments->GetLoadedToMemorySize() > CACHE_FILE_RELOAD_SIZE))
        {
          this->cacheFile->StoreItems(this->streamFragments, this->lastStoreTime, false, this->IsWholeStreamDownloaded());
        }
      }
    }

    UNLOCK_MUTEX(this->lockMutex)
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_M3u8::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER);
}

unsigned int CMPUrlSourceSplitter_Protocol_M3u8::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_SLEEP_TIME, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_SPLITTER);
}

unsigned int CMPUrlSourceSplitter_Protocol_M3u8::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER);
}

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::StartReceivingData(CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  LOCK_MUTEX(this->lockMutex, INFINITE)

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->mainCurlInstance->StopReceivingData());
  this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA);

  this->mainCurlInstance->SetConnectionState(None);

  this->streamFragmentDownloading = UINT_MAX;

  UNLOCK_MUTEX(this->lockMutex)
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;

  {
    LOCK_MUTEX(this->lockMutex, INFINITE)

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

    UNLOCK_MUTEX(this->lockMutex)
  }

  return result;
}

void CMPUrlSourceSplitter_Protocol_M3u8::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  __super::ClearSession();

  this->decryptionHoster->ClearSession();
  this->streamLength = 0;
  this->mainCurlInstance->ClearSession();

  this->cacheFile->Clear();
  this->streamFragments->Clear();
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;
  this->streamFragmentZeroTimestamp = 0;
  this->lastMediaPlaylistUpdateTime = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_M3u8::GetDuration(void)
{
  if (this->IsLiveStream())
  {
    return DURATION_LIVE_STREAM;
  }
  else
  {
    CM3u8StreamFragment *lastFragment = this->streamFragments->GetItem(this->streamFragments->Count() - 1);

    return (lastFragment != NULL) ? (lastFragment->GetFragmentTimestamp() + lastFragment->GetDuration()) : DURATION_UNSPECIFIED;
  }
}

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::GetStreamInformation(CStreamInformationCollection *streams)
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

unsigned int CMPUrlSourceSplitter_Protocol_M3u8::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_TIME;
}

int64_t CMPUrlSourceSplitter_Protocol_M3u8::SeekToTime(unsigned int streamId, int64_t time)
{
  int64_t result = -1;

  LOCK_MUTEX(this->lockMutex, INFINITE)

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  // find stream fragment to process
  // TO DO: implement better and faster seeking algorithm
  for (unsigned int i = 0; i < this->streamFragments->Count(); i++)
  {
    // first M3U8 stream fragment has always zero timestamp
    CM3u8StreamFragment *fragment = this->streamFragments->GetItem(i);
    CM3u8StreamFragment *nextFragment = this->streamFragments->GetItem(i + 1);

    int64_t fragmentTime = fragment->GetFragmentTimestamp() - this->streamFragmentZeroTimestamp;
    int64_t nextFragmentTime = (nextFragment == NULL) ? INT64_MAX : (nextFragment->GetFragmentTimestamp() - this->streamFragmentZeroTimestamp);

    if ((fragmentTime <= time) && (nextFragmentTime >= time))
    {
      this->streamFragmentProcessing = i;

      result = (result == -1) ? fragmentTime : (min(result, fragmentTime));

      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: time %lld, fragment: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, fragmentTime, fragment->GetFragment());
      break;
    }
  }

  if (result != (-1))
  {
    this->flags &= ~PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
    this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

    // is set stream fragment to process
    // exists at least one stream fragment and cover all possible timestamps (0 - UINT64_MAX)

    this->streamLength = 0;

    CM3u8StreamFragment *previousFragment = (this->streamFragmentProcessing != 0) ? this->streamFragments->GetItem(this->streamFragmentProcessing - 1) : NULL;
    CM3u8StreamFragment *processingFragment = this->streamFragments->GetItem(this->streamFragmentProcessing);

    if (!processingFragment->IsDownloaded())
    {
      // stream fragment is not downloaded, force its download
      // split stream fragment

      // force to download missing fragment

      processingFragment->SetFragmentStartPosition(0);
      this->streamFragmentToDownload = this->streamFragmentProcessing;

      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_M3U8_FLAG_STOP_RECEIVING_DATA;
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

  UNLOCK_MUTEX(this->lockMutex)
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_M3u8::GetName(void)
{
  return PROTOCOL_NAME;
}

HRESULT CMPUrlSourceSplitter_Protocol_M3u8::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = __super::Initialize(configuration);
  CProtocolPluginConfiguration *protocolConfiguration = (CProtocolPluginConfiguration *)configuration;
  CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_INVALIDARG);
  CHECK_POINTER_HRESULT(result, this->lockMutex, result, E_NOT_VALID_STATE);

  CHECK_HRESULT_EXECUTE(result, this->decryptionHoster->InitializePlugins(protocolConfiguration->GetConfiguration()));

  if (SUCCEEDED(result))
  {
    CParameterCollection *cookies = new CParameterCollection(&result);
    CHECK_POINTER_HRESULT(result, cookies, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      unsigned int currentCookiesCount = this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_COOKIES_COUNT, true, 0);
      if (currentCookiesCount != 0)
      {
        for (unsigned int i = 0; (SUCCEEDED(result) & (i < currentCookiesCount)); i++)
        {
          wchar_t *cookieName = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
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

const wchar_t *CMPUrlSourceSplitter_Protocol_M3u8::GetModuleName(void)
{
  return PROTOCOL_IMPLEMENTATION_NAME;
}

const wchar_t *CMPUrlSourceSplitter_Protocol_M3u8::GetStoreFileNamePart(void)
{
  return PROTOCOL_STORE_FILE_NAME_PART;
}

int64_t CMPUrlSourceSplitter_Protocol_M3u8::GetBytePosition(void)
{
  int64_t result = 0;

  {
    LOCK_MUTEX(this->lockMutex, INFINITE)

    unsigned int first = this->streamFragments->GetStartSearchingIndex();
    unsigned int count = this->streamFragments->GetSearchCount();

    if (count != 0)
    {
      CM3u8StreamFragment *firstFragment = this->streamFragments->GetItem(first);
      CM3u8StreamFragment *lastFragment = this->streamFragments->GetItem(first + count - 1);

      result = lastFragment->GetFragmentStartPosition() + (int64_t)lastFragment->GetLength() - firstFragment->GetFragmentStartPosition();
    }

    UNLOCK_MUTEX(this->lockMutex)
  }

  return result;
}

CM3u8StreamFragmentCollection *CMPUrlSourceSplitter_Protocol_M3u8::GetStreamFragmentsFromMediaPlaylist(const wchar_t *baseUrl, CMediaPlaylist *playlist, CM3u8StreamFragmentCollection *currentStreamFragments)
{
  HRESULT result = S_OK;
  CM3u8StreamFragmentCollection *streamFragments = NULL;
  CHECK_POINTER_DEFAULT_HRESULT(result, baseUrl);
  CHECK_POINTER_DEFAULT_HRESULT(result, playlist);
  CHECK_POINTER_DEFAULT_HRESULT(result, currentStreamFragments);

  if (SUCCEEDED(result))
  {
    streamFragments = new CM3u8StreamFragmentCollection(&result);
    CHECK_POINTER_HRESULT(result, streamFragments, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      CM3u8StreamFragment *lastStreamFragment = currentStreamFragments->GetItem(currentStreamFragments->Count() - 1);
      CM3u8Fragment *firstFragment = playlist->GetFragments()->GetItem(0);

      unsigned int sequenceNumber = 1;

      if (lastStreamFragment != NULL)
      {
        sequenceNumber = lastStreamFragment->GetFragment() + 1;
      }
      else if (firstFragment != NULL)
      {
        sequenceNumber = firstFragment->GetSequenceNumber();
      }

      CM3u8FragmentCollection* fragments = playlist->GetFragments();
      if (lastStreamFragment == NULL && fragments->Count() > 3)
      {
        time_t nowt = std::time(0);
        tm* now = gmtime(&nowt);
        time_t nowinutc = mktime(now);

        unsigned int lastValidDateEarlierThanNowIndexPlus1 = 0;
        for (unsigned int i = fragments->Count() - 3; i > 0; i--)
        {
          if (fragments->GetItem(i - 1)->GetIsTimeValid())
          {
            time_t t_frag = fragments->GetItem(i - 1)->GetTimeInUTC();
            if (t_frag <= nowinutc)
            {
              lastValidDateEarlierThanNowIndexPlus1 = i;
              break;
            }
          }
        }

        if (lastValidDateEarlierThanNowIndexPlus1 > 0)
        {
          CM3u8Fragment* fragment = fragments->GetItem(lastValidDateEarlierThanNowIndexPlus1 - 1);
          double nextstarttime = fragment->GetTimeInUTC() + (double)fragment->GetDuration() / 1000;

          while (nextstarttime < nowinutc && lastValidDateEarlierThanNowIndexPlus1 < fragments->Count() - 3)
          {
            lastValidDateEarlierThanNowIndexPlus1++;
            fragment = fragments->GetItem(lastValidDateEarlierThanNowIndexPlus1 - 1);
            nextstarttime += (double)fragment->GetDuration() / 1000;
          };

          if (lastValidDateEarlierThanNowIndexPlus1 <= fragments->Count() - 3)
          {
            sequenceNumber = fragments->GetItem(lastValidDateEarlierThanNowIndexPlus1 - 1)->GetSequenceNumber();
          }

        }
      }


      int64_t timestamp = (lastStreamFragment != NULL) ? (lastStreamFragment->GetFragmentTimestamp() + lastStreamFragment->GetDuration()) : 0;

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < playlist->GetFragments()->Count())); i++)
      {
        CM3u8Fragment *frag = playlist->GetFragments()->GetItem(i);

        if (frag->GetSequenceNumber() >= sequenceNumber)
        {
          wchar_t *uri = FormatAbsoluteUrl(baseUrl, frag->GetUri());
          CHECK_POINTER_HRESULT(result, uri, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            CM3u8StreamFragment *fragment = new CM3u8StreamFragment(&result, uri, frag->GetSequenceNumber(), timestamp, frag->GetDuration(), (frag->GetOffset() != OFFSET_NOT_SPECIFED) ? frag->GetOffset() : UINT_MAX, (frag->GetLength() != LENGTH_NOT_SPECIFIED) ? frag->GetLength() : UINT_MAX, frag->GetFragmentEncryption());
            CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

            if (SUCCEEDED(result))
            {
              fragment->SetDiscontinuity(frag->IsDiscontinuity(), UINT_MAX);
              fragment->SetEndOfStream(frag->IsEndOfStream());
            }

            CHECK_CONDITION_HRESULT(result, streamFragments->Add(fragment), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
          }

          FREE_MEM(uri);

          timestamp += frag->GetDuration();
        }
      }
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(streamFragments));
  return streamFragments;
}