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

#include "MPUrlSourceSplitter_Protocol_Http.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "MPUrlSourceSplitter_Protocol_Http_Parameters.h"
#include "Parameters.h"
#include "ProtocolPluginConfiguration.h"
#include "ErrorCodes.h"
#include "StreamPackageDataRequest.h"
#include "StreamPackageDataResponse.h"

#include <WinInet.h>
#include <stdio.h>
#include <Shlwapi.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Httpd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Http"
#endif

CPlugin *CreatePlugin(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Http(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Http *protocol = (CMPUrlSourceSplitter_Protocol_Http *)plugin;

    delete protocol;
  }
}

CMPUrlSourceSplitter_Protocol_Http::CMPUrlSourceSplitter_Protocol_Http(HRESULT *result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->currentCookies = NULL;
  this->lockCurlMutex = NULL;
  this->lockMutex = NULL;
  this->mainCurlInstance = NULL;
  this->streamLength = 0;
  this->connectionState = None;
  this->streamFragments = NULL;
  this->cacheFile = NULL;
  this->currentStreamPosition = 0;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->lastReceiveDataTime = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
    this->cacheFile = new CCacheFile(result);
    this->streamFragments = new CHttpStreamFragmentCollection(result);

    CHECK_POINTER_HRESULT(*result, this->lockMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->lockCurlMutex, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->cacheFile, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->streamFragments, *result, E_OUTOFMEMORY);

    wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP);
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

CMPUrlSourceSplitter_Protocol_Http::~CMPUrlSourceSplitter_Protocol_Http()
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->currentCookies);
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

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_Http::GetConnectionState(void)
{
  return this->connectionState;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Http::ReceiveData(CStreamPackage *streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);

  if (SUCCEEDED(result))
  {
    LOCK_MUTEX(this->lockMutex, INFINITE)

    if (SUCCEEDED(result) && this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_REPORTED_STATUS_CODE) && (this->mainCurlInstance != NULL) && ((this->connectionState == Opening) || (this->connectionState == Opened)))
    {
      {
        LOCK_MUTEX(this->lockCurlMutex, INFINITE)

        FREE_MEM_CLASS(this->currentCookies);
        this->currentCookies = this->mainCurlInstance->GetCurrentCookies();

        unsigned int bytesRead = this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
        if (bytesRead > 0)
        {
          this->connectionState = Opened;
          this->lastReceiveDataTime = GetTickCount();

          CHttpStreamFragment *currentDownloadingFragment = this->streamFragments->GetItem(this->streamFragmentDownloading);
          CHECK_CONDITION_HRESULT(result, currentDownloadingFragment->GetBuffer()->InitializeBuffer(bytesRead), result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            currentDownloadingFragment->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData(), 0, bytesRead);
            this->currentStreamPosition += bytesRead;

            currentDownloadingFragment->SetLoadedToMemoryTime(this->lastReceiveDataTime, UINT_MAX);
            currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
            currentDownloadingFragment->SetProcessed(true, UINT_MAX);

            this->streamFragments->UpdateIndexes(this->streamFragmentDownloading, 1);

            this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(bytesRead);

            CHttpStreamFragment *nextFragment = this->streamFragments->GetItem(this->streamFragmentDownloading + 1);
            // nextFragment can be NULL in case that we are on the end of collection

            if ((nextFragment == NULL) || ((nextFragment != NULL) && (nextFragment->GetFragmentStartPosition() > this->currentStreamPosition)))
            {
              // next fragment doesn't exist or next fragment start is after current stream position

              // create new HTTP stream fragment
              CHttpStreamFragment *fragment = new CHttpStreamFragment(&result);
              CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE(SUCCEEDED(result), fragment->SetFragmentStartPosition(this->currentStreamPosition));

              CHECK_CONDITION_HRESULT(result, this->streamFragments->Insert(this->streamFragmentDownloading + 1, fragment), result, E_OUTOFMEMORY);

              this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(this->streamFragmentDownloading);

              if (SUCCEEDED(result))
              {
                this->streamFragmentDownloading++;
                this->streamFragments->SetSearchCount(this->streamFragments->Count() - 1);
              }

              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
            }
            else
            {
              this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(this->streamFragmentDownloading);

              this->streamFragmentToDownload = UINT_MAX;
              this->streamFragmentDownloading = UINT_MAX;

              // this clear CURL instance and buffer, set CURL instance state to CURL_STATE_RECEIVED_ALL_DATA
              // this forces to check end of stream and forces to find gap in stream
              this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE;
            }
          }
        }

        UNLOCK_MUTEX(this->lockCurlMutex)
      }
    }

    if (SUCCEEDED(result) && (this->mainCurlInstance == NULL) && (this->connectionState == Initializing) && (!this->IsWholeStreamDownloaded()))
    {
      this->connectionState = Initializing;

      // if not set fragment to download, then set fragment to download (get next not downloaded fragment after current processed fragment)
      this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndexAfterStartPosition(this->currentStreamPosition) : this->streamFragmentToDownload;
      // if not set fragment to download, then set fragment to download (get next not downloaded fragment from first fragment)
      this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0) : this->streamFragmentToDownload;
      // fragment to download still can be UINT_MAX = no fragment to download

      // clear all not downloaded stream fragments
      CIndexedStreamFragmentCollection *notDownloadedIndexedItems = new CIndexedStreamFragmentCollection(&result);
      CHECK_CONDITION_HRESULT(result, notDownloadedIndexedItems, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->streamFragments->GetNotDownloadedStreamFragments(notDownloadedIndexedItems));

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < notDownloadedIndexedItems->Count())); i++)
      {
        CCacheFileItem *notDownloadedItem = notDownloadedIndexedItems->GetItem(i)->GetItem();

        notDownloadedItem->GetBuffer()->ClearBuffer();
      }

      FREE_MEM_CLASS(notDownloadedIndexedItems);

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

      this->mainCurlInstance = new CHttpCurlInstance(&result, this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
      CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        if (this->currentCookies != NULL)
        {
          CHECK_CONDITION_HRESULT(result, this->mainCurlInstance->SetCurrentCookies(this->currentCookies), result, E_HTTP_CANNOT_SET_COOKIES);
        }

        if (this->configuration->GetValueBool(PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA, true, PARAMETER_NAME_DUMP_PROTOCOL_INPUT_DATA_DEFAULT))
        {
          wchar_t *storeFilePath = this->GetDumpFile();
          CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->mainCurlInstance->SetDumpFile(storeFilePath));
          FREE_MEM(storeFilePath);
        }
      }

      if (SUCCEEDED(result))
      {
        CHttpDownloadRequest *request = new CHttpDownloadRequest(&result);
        CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          CHttpStreamFragment *fragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? NULL : this->streamFragments->GetItem(this->streamFragmentToDownload);
          CHttpStreamFragment *nextFragment = (this->streamFragmentToDownload == UINT_MAX) ? NULL : this->streamFragments->GetItem(this->streamFragmentToDownload + 1);

          int64_t startStreamPosition = (fragmentToDownload != NULL) ? fragmentToDownload->GetFragmentStartPosition() : 0;
          int64_t endStreamPosition = (nextFragment != NULL) ? (nextFragment->GetFragmentStartPosition() - 1) : startStreamPosition;

          request->SetCookie(this->configuration->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL));
          request->SetHttpVersion(this->configuration->GetValueLong(PARAMETER_NAME_HTTP_VERSION, true, HTTP_VERSION_DEFAULT));
          request->SetIgnoreContentLength((this->configuration->GetValueLong(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
          request->SetReferer(this->configuration->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL));
          request->SetUrl(this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL));
          request->SetUserAgent(this->configuration->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL));
          request->SetStartPosition(this->IsLiveStreamDetected() ? 0 : startStreamPosition);
          request->SetEndPosition(this->IsLiveStreamDetected() ? 0 : endStreamPosition);

          // set finish time, all methods must return before finish time
          request->SetFinishTime(finishTime);
          request->SetReceivedDataTimeout(this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER));
          request->SetNetworkInterfaceName(this->configuration->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

          this->currentStreamPosition = startStreamPosition;

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
                CHttpStreamFragment *fragment = new CHttpStreamFragment(&result);
                CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

                CHECK_CONDITION_EXECUTE(SUCCEEDED(result), fragment->SetFragmentStartPosition(0));
                CHECK_CONDITION_HRESULT(result, this->streamFragments->Add(fragment), result, E_OUTOFMEMORY);

                CHECK_CONDITION_EXECUTE(SUCCEEDED(result), this->streamFragmentToDownload = 0);
                CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));
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

    if (SUCCEEDED(result) && (!this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_REPORTED_STATUS_CODE)) && (this->mainCurlInstance != NULL) && ((this->connectionState == Opening) || (this->connectionState == Opened)))
    {
      // check HTTP response code
      long responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();

      if (responseCode != 0)
      {
        // response code 200 - 299 = OK
        // response code 300 - 399 = redirect (OK)
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: HTTP status code: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, responseCode);

        if ((responseCode < 200) || (responseCode >= 400))
        {
          this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragmentDownloading : this->streamFragmentToDownload;
          this->streamFragmentDownloading = UINT_MAX;

          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA;
        }
        else
        {
          this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_REPORTED_STATUS_CODE;

          if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_SEEKING_SUPPORT_DETECTION))
          {
            this->flags |= (responseCode == 206) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;
            this->flags |= (this->mainCurlInstance->GetHttpDownloadResponse()->GetRangesSupported()) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;
          }

          this->flags |= (this->configuration->GetValueBool(PARAMETER_NAME_HTTP_SEEKING_SUPPORTED, true, HTTP_SEEKING_SUPPORTED_DEFAULT)) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;
        }
      }
    }

    if (SUCCEEDED(result) && this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_REPORTED_STATUS_CODE) && (!this->IsWholeStreamDownloaded()) && (this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
    {
      // all data received, we're not receiving data
      // check end of stream or error on HTTP connection

      if (SUCCEEDED(this->mainCurlInstance->GetHttpDownloadResponse()->GetResultError()))
      {
        // check if all data removed from CURL instance
        if (this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
        {
          // mark current downloading stream fragment as downloaded or remove it, if it has not any data
          CHttpStreamFragment *currentDownloadingFragment = this->streamFragments->GetItem(this->streamFragmentDownloading);

          if (currentDownloadingFragment != NULL)
          {
            if (currentDownloadingFragment->GetLength() == 0)
            {
              this->streamFragments->Remove(this->streamFragmentDownloading, 1);
            }
            else
            {
              currentDownloadingFragment->SetLoadedToMemoryTime(this->lastReceiveDataTime, UINT_MAX);
              currentDownloadingFragment->SetDownloaded(true, UINT_MAX);
              currentDownloadingFragment->SetProcessed(true, UINT_MAX);

              this->streamFragments->UpdateIndexes(this->streamFragmentDownloading, 1);
              this->streamFragments->RecalculateProcessedStreamFragmentStartPosition(this->streamFragmentDownloading);
            }
          }

          if (!this->IsLiveStreamDetected())
          {
            // check if all data removed from CURL instance
            if (this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
            {
              // check if we are not missing any data

              if (this->IsSetStreamLength() && (this->currentStreamPosition >= this->streamLength))
              {
                // stream length is set and our position is at least stream length, we reached end of stream
                this->flags |= PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
              }

              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: searching for gap in stream fragments, stream fragment count: %u, stream position: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamFragments->Count(), this->currentStreamPosition);
              this->streamFragmentToDownload = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(this->streamFragmentDownloading);

              if (this->streamFragmentToDownload == UINT_MAX)
              {
                this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"searching for gap in stream fragments from beginning");
                this->streamFragmentToDownload = this->streamFragments->GetFirstNotDownloadedStreamFragmentIndex(0);
              }

              if (this->streamFragmentToDownload == UINT_MAX)
              {
                // we didn't find gap after beggining of stream fragments, we have whole stream

                // whole stream downloaded
                this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;
                this->logger->Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"no gap found, all data received");

                this->currentStreamPosition = this->streamLength;

                this->streamFragmentDownloading = UINT_MAX;

                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE;
                this->flags &= ~MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA;
              }
              else 
              {
                this->streamFragmentDownloading = UINT_MAX;
                CHttpStreamFragment *fragmentToDownload = this->streamFragments->GetItem(this->streamFragmentToDownload);

                this->logger->Log(LOGGER_VERBOSE, L"%s: %s: found gap in stream fragments, start: %lld", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragmentToDownload->GetFragmentStartPosition());

                // stops receiving data
                // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
                this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA;
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
      }
      else
      {
        // check if all data removed from CURL instance
        if (this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
        {
          if (this->IsLiveStreamDetected())
          {
            if (this->streamFragments->Count() != 0)
            {
              CHttpStreamFragment *fragment = this->streamFragments->GetItem(this->streamFragmentDownloading);

              fragment->SetDiscontinuity(true, this->streamFragmentDownloading);
              this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, start '%lld', size '%u'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, fragment->GetFragmentStartPosition(), fragment->GetLength());
            }

            this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragmentDownloading : this->streamFragmentToDownload;
            this->streamFragmentDownloading = UINT_MAX;

            // error while receiving data, stops receiving data
            // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA;
          }
          else
          {
            // we don't need to set discontinuity flag, because we can re-open connection on lost postion

            // error while receiving data, stops receiving data
            // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
            this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA;

            // re-open connection at last known position
            this->streamFragmentToDownload = (this->streamFragmentToDownload == UINT_MAX) ? this->streamFragmentDownloading : this->streamFragmentToDownload;
            this->streamFragmentDownloading = UINT_MAX;
          }
        }
      }
    }

    if ((!this->IsSetStreamLength()) && (!(this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen())))
    {
      double streamSize = (this->mainCurlInstance != NULL) ? this->mainCurlInstance->GetDownloadContentLength() : 0;
      if (streamSize > 0)
      {
        LONGLONG total = LONGLONG(streamSize);
        this->streamLength = total;
        this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, total);

        this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
        this->flags &= ~(PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED | PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED);
      }
      else
      {
        // probably live stream, content length is not specified
        this->flags |= PROTOCOL_PLUGIN_FLAG_LIVE_STREAM_DETECTED;

        if (this->streamLength == 0)
        {
          // stream length not set
          // just make guess
          this->streamLength = LONGLONG(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting guess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
        }
        else if ((this->currentStreamPosition > (this->streamLength * 3 / 4)))
        {
          // it is time to adjust stream length, we are approaching to end but still we don't know total length
          this->streamLength = this->currentStreamPosition * 2;
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: adjusting guess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
        }
      }
    }

    if ((!this->IsSetStreamLength()) && (this->IsWholeStreamDownloaded() || this->IsEndOfStreamReached() || this->IsConnectionLostCannotReopen()))
    {
      // get last stream fragment to get total length
      CHttpStreamFragment *fragment = (this->streamFragments->Count() != 0) ? this->streamFragments->GetItem(this->streamFragments->Count() - 1) : NULL;

      this->streamLength = (fragment != NULL) ? (fragment->GetFragmentStartPosition() + (int64_t)fragment->GetLength()) : 0;
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);

      this->flags |= PROTOCOL_PLUGIN_FLAG_SET_STREAM_LENGTH;
      this->flags &= ~PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;

      // set current stream position to stream length to get correct result in QueryStreamProgress() method
      this->currentStreamPosition = this->streamLength;

      CHECK_CONDITION_NOT_NULL_EXECUTE(this->mainCurlInstance, this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE);
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

    if (streamPackage->GetState() == CStreamPackage::Waiting)
    {
      // in Waiting or WaitingIgnoreTimeout state can be request only if request and response are correctly set
      CStreamPackageDataRequest *request = dynamic_cast<CStreamPackageDataRequest *>(streamPackage->GetRequest());
      CStreamPackageDataResponse *response = dynamic_cast<CStreamPackageDataResponse *>(streamPackage->GetResponse());

      // don not clear response buffer, we don't have to copy data again from start position
      // first try to find starting stream fragment (packet which have first data)
      unsigned int foundDataLength = response->GetBuffer()->GetBufferOccupiedSpace();

      int64_t startPosition = request->GetStart() + foundDataLength;
      unsigned int fragmentIndex = this->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);

      while (fragmentIndex != UINT_MAX)
      {
        // get stream fragment
        CHttpStreamFragment *fragment = this->streamFragments->GetItem(fragmentIndex);

        // set copy data start and copy data length
        unsigned int copyDataStart = (startPosition > fragment->GetFragmentStartPosition()) ? (unsigned int)(startPosition - fragment->GetFragmentStartPosition()) : 0;
        unsigned int copyDataLength = min(fragment->GetLength() - copyDataStart, request->GetLength() - foundDataLength);

        // copy data from stream fragment to response buffer
        if (this->cacheFile->LoadItems(this->streamFragments, fragmentIndex, true, UINT_MAX, (this->lastProcessedSize == 0) ? CACHE_FILE_RELOAD_SIZE : this->lastProcessedSize))
        {
          // memory is allocated while switching from Created to Waiting state, we can't have problem on next line
          response->GetBuffer()->AddToBufferWithResize(fragment->GetBuffer(), copyDataStart, copyDataLength);
        }
        else
        {
          // we can't copy data, try it later
          break;
        }

        // update length of data
        foundDataLength += copyDataLength;
        this->currentProcessedSize += copyDataLength;

        if (fragment->IsDiscontinuity())
        {
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: discontinuity, completing request, request '%u', start '%lld', size '%u', found: '%u'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), foundDataLength);

          response->SetDiscontinuity(true);
        }

        if ((!fragment->IsDiscontinuity()) && (foundDataLength < request->GetLength()))
        {
          // find another media packet after end of this media packet
          startPosition += copyDataLength;

          fragmentIndex = this->streamFragments->GetStreamFragmentIndexBetweenPositions(startPosition);
        }
        else
        {
          // do not find any more stream fragments for this request because we have enough data
          break;
        }
      }

      if (foundDataLength < request->GetLength())
      {
        // found data length is lower than requested
        // check request flags, maybe we can complete request

        if ((request->IsSetAnyNonZeroDataLength() || request->IsSetAnyDataLength()) && (foundDataLength > 0))
        {
          // set connection lost and no more data available flags
          if (this->IsConnectionLostCannotReopen())
          {
            // connection is lost and we cannot reopen it
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            response->SetConnectionLostCannotReopen(true);
          }

          if (this->IsEndOfStreamReached() && ((request->GetStart() + request->GetLength()) >= this->streamLength))
          {
            // we are not receiving more data, complete request
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            response->SetNoMoreDataAvailable(true);
          }

          // request can be completed with any length of available data
          streamPackage->SetCompleted(S_OK);
        }
        else if (request->IsSetAnyDataLength() && (foundDataLength == 0))
        {
          // no data available, check end of stream and connection lost

          if (this->IsConnectionLostCannotReopen())
          {
            // connection is lost and we cannot reopen it
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: connection lost, no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            response->SetConnectionLostCannotReopen(true);
            streamPackage->SetCompleted(S_OK);
          }
          else if (this->IsEndOfStreamReached() && ((request->GetStart() + request->GetLength()) >= this->streamLength))
          {
            // we are not receiving more data, complete request
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            response->SetNoMoreDataAvailable(true);
            streamPackage->SetCompleted(S_OK);
          }
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

            response->SetConnectionLostCannotReopen(true);
            streamPackage->SetCompleted((response->GetBuffer()->GetBufferOccupiedSpace() != 0) ? S_OK : E_CONNECTION_LOST_CANNOT_REOPEN);
          }
          else if (this->IsEndOfStreamReached() && ((request->GetStart() + request->GetLength()) >= this->streamLength))
          {
            // we are not receiving more data, complete request
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: no more data available, request '%u', start '%lld', size '%u', stream length: '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), request->GetStart(), request->GetLength(), this->streamLength);

            response->SetNoMoreDataAvailable(true);
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
          if (this->GetSeekingCapabilities() & SEEKING_METHOD_POSITION)
          {
            if (SUCCEEDED(result))
            {
              // find start and end of gap
              // we are missing data without found data length

              int64_t requestStart = request->GetStart() + foundDataLength;
              unsigned int startIndex = 0;
              unsigned int endIndex = 0;

              if (this->streamFragments->GetStreamFragmentInsertPosition(requestStart, &startIndex, &endIndex))
              {
                // start and end index found successfully

                if (startIndex == endIndex)
                {
                  // we found stream fragment with exactly same start position as we requested

                  if ((this->streamFragmentDownloading != endIndex) && (this->streamFragmentToDownload != endIndex))
                  {
                    // in other case we are downloading found stream fragment

                    // stops receiving data
                    // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
                    this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA;

                    // re-open connection at requested stream fragment
                    this->streamFragmentToDownload = endIndex;
                    this->streamFragmentDownloading = UINT_MAX;

                    this->logger->Log(LOGGER_VERBOSE, L"%s: %s: request '%u', requesting data from '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), requestStart);
                  }
                }

                if (this->currentStreamPosition != requestStart)
                {
                  endIndex = min(endIndex, this->streamFragments->Count());

                  if ((this->streamFragmentDownloading != endIndex) && (this->streamFragmentToDownload != endIndex))
                  {
                    CHttpStreamFragment *fragment = new CHttpStreamFragment(&result);
                    CHECK_POINTER_HRESULT(result, fragment, result, E_OUTOFMEMORY);

                    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), fragment->SetFragmentStartPosition(requestStart));
                    CHECK_CONDITION_HRESULT(result, this->streamFragments->Insert(endIndex, fragment), result, E_OUTOFMEMORY);
                    CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(fragment));

                    if (SUCCEEDED(result))
                    {
                      this->streamFragments->SetSearchCount(this->streamFragments->Count() - 1);

                      // stops receiving data
                      // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
                      this->flags |= MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA;

                      // re-open connection at requested stream fragment
                      this->streamFragmentToDownload = endIndex;
                      this->streamFragmentDownloading = UINT_MAX;

                      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: request '%u', requesting data from '%lld'", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, request->GetId(), requestStart);
                    }
                  }
                }
              }
            }
          }
          else
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
      }
      else if (foundDataLength == request->GetLength())
      {
        // found data length is equal than requested
        streamPackage->SetCompleted(S_OK);
      }
    }

    if (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE))
    {
      HRESULT res = S_OK;
      this->connectionState = Closing;

      if (this->mainCurlInstance != NULL)
      {
        res = this->mainCurlInstance->StopReceivingData();

        CHECK_CONDITION_EXECUTE(FAILED(res), this->logger->Log(LOGGER_INFO, L"%s: %s: closing connection failed, error: 0x%08X", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, res));
        CHECK_CONDITION_EXECUTE(SUCCEEDED(res), this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->ClearBuffer());
      }

      if ((res == S_OK) && (this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA)))
      {
        // this clear CURL instance and buffer, it leads to GetConnectionState() to ProtocolConnectionState::None result and connection will be reopened by ProtocolHoster
        this->StopReceivingData();
      }

      if (res == S_OK)
      {
        this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"connection closed successfully");
        this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA);
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

      if (this->cacheFile->GetCacheFile() == NULL)
      {
        wchar_t *storeFilePath = this->GetStoreFile();
        CHECK_CONDITION_NOT_NULL_EXECUTE(storeFilePath, this->cacheFile->SetCacheFile(storeFilePath));
        FREE_MEM(storeFilePath);
      }

      // in case of live stream remove all downloaded and processed stream fragments before reported stream time
      if ((this->IsLiveStream()) && (this->reportedStreamTime > 0))
      {
        unsigned int fragmentRemoveStart = (this->streamFragments->GetStartSearchingIndex() == 0) ? 1 : 0;
        unsigned int fragmentRemoveCount = 0;

        while ((fragmentRemoveStart + fragmentRemoveCount) < this->streamFragments->Count())
        {
          CHttpStreamFragment *fragment = this->streamFragments->GetItem(fragmentRemoveStart + fragmentRemoveCount);

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

        if ((fragmentRemoveCount > 0) && (this->cacheFile->RemoveItems(this->streamFragments, fragmentRemoveStart, fragmentRemoveCount)))
        {
          unsigned int startSearchIndex = (fragmentRemoveCount > this->streamFragments->GetStartSearchingIndex()) ? 0 : (this->streamFragments->GetStartSearchingIndex() - fragmentRemoveCount);
          unsigned int searchCountDecrease = (fragmentRemoveCount > this->streamFragments->GetStartSearchingIndex()) ? (fragmentRemoveCount - this->streamFragments->GetStartSearchingIndex()) : 0;

          this->streamFragments->SetStartSearchingIndex(startSearchIndex);
          this->streamFragments->SetSearchCount(this->streamFragments->GetSearchCount() - searchCountDecrease);

          this->streamFragments->Remove(fragmentRemoveStart, fragmentRemoveCount);

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

      // store all stream fragments (which are not stored) to file
      if ((this->cacheFile->GetCacheFile() != NULL) && (this->streamFragments->Count() != 0) && (this->streamFragments->GetLoadedToMemorySize() > CACHE_FILE_RELOAD_SIZE))
      {
        this->cacheFile->StoreItems(this->streamFragments, this->lastStoreTime, false, this->IsWholeStreamDownloaded());
      }
    }

    UNLOCK_MUTEX(this->lockMutex)
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::GetConnectionParameters(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  // add configuration parameters
  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    // add current cookies
    if ((this->currentCookies != NULL) && (this->currentCookies->Count() != 0))
    {
      // first add count of cookies
      wchar_t *cookiesCountValue = FormatString(L"%u", this->currentCookies->Count());
      CHECK_POINTER_HRESULT(result, cookiesCountValue, result, E_OUTOFMEMORY); 

      CHECK_CONDITION_HRESULT(result, parameters->Add(PARAMETER_NAME_HTTP_COOKIES_COUNT, cookiesCountValue), result, E_OUTOFMEMORY);

      for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->currentCookies->Count())); i++)
      {
        CParameter *cookie = this->currentCookies->GetItem(i);
        wchar_t *name = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
        CHECK_POINTER_HRESULT(result, name, result, E_OUTOFMEMORY);

        CHECK_CONDITION_HRESULT(result, parameters->Add(name, cookie->GetValue()), result, E_OUTOFMEMORY);

        FREE_MEM(name);
      }

      FREE_MEM(cookiesCountValue);
    }
  }

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetOpenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER);
}

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetOpenConnectionSleepTime(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_OPEN_CONNECTION_SLEEP_TIME, true, this->IsIptv() ? HTTP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_IPTV : HTTP_OPEN_CONNECTION_SLEEP_TIME_DEFAULT_SPLITTER);
}

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetTotalReopenConnectionTimeout(void)
{
  return this->configuration->GetValueUnsignedInt(PARAMETER_NAME_HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT, true, this->IsIptv() ? HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_IPTV : HTTP_TOTAL_REOPEN_CONNECTION_TIMEOUT_DEFAULT_SPLITTER);
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::StartReceivingData(CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Http::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  LOCK_MUTEX(this->lockMutex, INFINITE)

  FREE_MEM_CLASS(this->mainCurlInstance);

  this->connectionState = None;
  this->flags &= ~(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_REPORTED_STATUS_CODE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_CLOSE_CURL_INSTANCE | MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_STOP_RECEIVING_DATA);

  this->streamFragmentDownloading = UINT_MAX;

  UNLOCK_MUTEX(this->lockMutex)
  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;

  {
    LOCK_MUTEX(this->lockMutex, INFINITE)

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

    UNLOCK_MUTEX(this->lockMutex)
  }

  return result;
}

void CMPUrlSourceSplitter_Protocol_Http::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  this->StopReceivingData();

  __super::ClearSession();
 
  this->streamLength = 0;
  this->connectionState = None;
  this->cacheFile->Clear();
  this->streamFragments->Clear();
  this->currentStreamPosition = 0;
  this->lastStoreTime = 0;
  this->flags |= PROTOCOL_PLUGIN_FLAG_STREAM_LENGTH_ESTIMATED;
  this->lastReceiveDataTime = 0;
  this->lastProcessedSize = 0;
  this->currentProcessedSize = 0;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentToDownload = UINT_MAX;

  FREE_MEM_CLASS(this->currentCookies);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_Http::GetDuration(void)
{
  return this->IsLiveStream() ? DURATION_LIVE_STREAM : DURATION_UNSPECIFIED;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::GetStreamInformation(CStreamInformationCollection *streams)
{
  // HTTP protocol has always one stream (container)
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

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetSeekingCapabilities(void)
{
  unsigned int result = SEEKING_METHOD_NONE;

  {
    // lock access to stream
    LOCK_MUTEX(this->lockMutex, INFINITE)
    
    result = (this->IsWholeStreamDownloaded() || this->IsSetFlags(MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_RANGES_SUPPORTED)) ? SEEKING_METHOD_POSITION : SEEKING_METHOD_NONE;

    UNLOCK_MUTEX(this->lockMutex)
  }

  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Http::SeekToTime(unsigned int streamId, int64_t time)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu, to time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = E_SEEK_METHOD_NOT_SUPPORTED;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

// CPlugin implementation

const wchar_t *CMPUrlSourceSplitter_Protocol_Http::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Http::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::Initialize(CPluginConfiguration *configuration)
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
    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_SPLITTER, true, PARAMETER_NAME_SPLITTER_DEFAULT) ? PLUGIN_FLAG_SPLITTER : PROTOCOL_PLUGIN_FLAG_NONE;
    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_IPTV, true, PARAMETER_NAME_IPTV_DEFAULT) ? PLUGIN_FLAG_IPTV : PROTOCOL_PLUGIN_FLAG_NONE;

    this->flags |= this->configuration->GetValueBool(PARAMETER_NAME_HTTP_SEEKING_SUPPORT_DETECTION, true, HTTP_SEEKING_SUPPORT_DETECTION_DEFAULT) ? MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_SEEKING_SUPPORT_DETECTION : MP_URL_SOURCE_SPLITTER_PROTOCOL_HTTP_FLAG_NONE;
  }

  return result;
}

/* protected methods */

wchar_t *CMPUrlSourceSplitter_Protocol_Http::GetStoreFile(void)
{
  wchar_t *result = NULL;
  const wchar_t *folder = this->configuration->GetValue(PARAMETER_NAME_CACHE_FOLDER, true, NULL);

  if (folder != NULL)
  {
    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%smpurlsourcesplitter_protocol_http_%s.temp", folder, guid);
    }
    FREE_MEM(guid);
  }

  return result;
}

wchar_t *CMPUrlSourceSplitter_Protocol_Http::GetDumpFile(void)
{
  wchar_t *result = NULL;
  wchar_t *folder = Duplicate(this->configuration->GetValue(PARAMETER_NAME_LOG_FILE_NAME, true, NULL));

  if (folder != NULL)
  {
    PathRemoveFileSpec(folder);

    wchar_t *guid = ConvertGuidToString(this->logger->GetLoggerInstanceId());
    if (guid != NULL)
    {
      result = FormatString(L"%s\\mpurlsourcesplitter_protocol_http_%s.dump", folder, guid);
    }
    FREE_MEM(guid);
  }

  FREE_MEM(folder);

  return result;
}