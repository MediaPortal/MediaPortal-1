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

#include <WinInet.h>
#include <stdio.h>

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Httpd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Http"
#endif

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Http(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Http *pClass = (CMPUrlSourceSplitter_Protocol_Http *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Protocol_Http::CMPUrlSourceSplitter_Protocol_Http(CLogger *logger, CParameterCollection *configuration)
{
  this->configurationParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

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
  
  this->receiveDataTimeout = HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->streamLength = 0;
  this->setLength = false;
  this->streamTime = 0;
  this->endStreamTime = 0;
  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
  this->internalExitRequest = false;
  this->wholeStreamDownloaded = false;
  this->receivedData = NULL;
  this->mainCurlInstance = NULL;
  this->supressData = false;
  this->currentCookies = NULL;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Protocol_Http::~CMPUrlSourceSplitter_Protocol_Http()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->configurationParameters);
  FREE_MEM_CLASS(this->currentCookies);

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

bool CMPUrlSourceSplitter_Protocol_Http::IsConnected(void)
{
  return ((this->mainCurlInstance != NULL) || (this->wholeStreamDownloaded));
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Http::ReceiveData(CReceiveData *receiveData)
{
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
    long responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
    if ((responseCode >= 0) && ((responseCode < 200) || (responseCode >= 400)))
    {
      // response code 200 - 299 = OK
      // response code 300 - 399 = redirect (OK)
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: error response code: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, responseCode);
    }
    else if ((responseCode >= 200) && (responseCode < 400))
    {
      if (!this->setLength)
      {
        double streamSize = this->mainCurlInstance->GetDownloadContentLength();
        if ((streamSize > 0) && (this->streamTime < streamSize))
        {
          LONGLONG total = LONGLONG(streamSize);
          this->streamLength = total;
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, total);
          receiveData->GetTotalLength()->SetTotalLength(total, false);
          this->setLength = true;
        }
        else
        {
          if (this->streamLength == 0)
          {
            // stream length not set
            // just make guess
            this->streamLength = LONGLONG(MINIMUM_RECEIVED_DATA_FOR_SPLITTER);
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
            receiveData->GetTotalLength()->SetTotalLength(this->streamLength, true);
          }
          else if ((this->streamTime > (this->streamLength * 3 / 4)))
          {
            // it is time to adjust stream length, we are approaching to end but still we don't know total length
            this->streamLength = this->streamTime * 2;
            this->logger->Log(LOGGER_VERBOSE, L"%s: %s: adjusting quess total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
            receiveData->GetTotalLength()->SetTotalLength(this->streamLength, true);
          }
        }
      }
    }

    {
      CLockMutex lockData(this->lockCurlMutex, INFINITE);

      FREE_MEM_CLASS(this->currentCookies);
      this->currentCookies = this->mainCurlInstance->GetCurrentCookies();

      unsigned int bytesRead = this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();
      if (bytesRead != 0)
      {
        unsigned int bufferSize = this->receivedData->GetBufferSize();
        unsigned int freeSpace = this->receivedData->GetBufferFreeSpace();
        unsigned int newBufferSize = max(bufferSize * 2, bufferSize + bytesRead);

        if (freeSpace < bytesRead)
        {
          this->logger->Log(LOGGER_INFO, L"%s: %s: not enough free space in buffer for received data, buffer size: %d, free size: %d, received data: %d, new buffer size: %d", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, bytesRead, newBufferSize);
          if (!this->receivedData->ResizeBuffer(newBufferSize))
          {
            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, L"resizing of buffer unsuccessful");
            // error
            bytesRead = 0;
          }
        }

        if (bytesRead != 0)
        {
          this->receivedData->AddToBufferWithResize(this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData());
          this->mainCurlInstance->GetHttpDownloadResponse()->GetReceivedData()->RemoveFromBufferAndMove(bytesRead);
        }
      }
    }

    unsigned int bufferOccupiedSpace = this->receivedData->GetBufferOccupiedSpace();
    if (bufferOccupiedSpace > 0)
    {
      // create media packet
      // set values of media packet

      CMediaPacket *mediaPacket = new CMediaPacket();
      if (mediaPacket->GetBuffer()->AddToBufferWithResize(this->receivedData) == bufferOccupiedSpace)
      {
        mediaPacket->SetStart(this->streamTime);
        mediaPacket->SetEnd(this->streamTime + bufferOccupiedSpace - 1);

        if (!receiveData->GetMediaPacketCollection()->Add(mediaPacket))
        {
          FREE_MEM_CLASS(mediaPacket);
        }
        else
        {
          this->streamTime += bufferOccupiedSpace;
          this->receivedData->RemoveFromBufferAndMove(bufferOccupiedSpace);
        }
      }
      else
      {
        FREE_MEM_CLASS(mediaPacket);
      }
    }

    if (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA)
    {
      // all data received, we're not receiving data

      if (this->mainCurlInstance->GetHttpDownloadResponse()->GetResultCode() == CURLE_OK)
      {
        // whole stream downloaded
        this->wholeStreamDownloaded = true;

        if (!this->setLength)
        {
          this->streamLength = this->streamTime;
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: setting total length: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->streamLength);
          receiveData->GetTotalLength()->SetTotalLength(this->streamLength, false);
          this->setLength = true;
        }

        // notify filter the we reached end of stream
        int64_t streamTime = this->streamTime;
        this->streamTime = this->streamLength;
        receiveData->GetEndOfStreamReached()->SetStreamPosition(max(0, streamTime - 1));
      }
      else
      {
        // error while receiving data, stops receiving data
        // this clear CURL instance and buffer, it leads to IsConnected() false result and connection will be reopened by ParserHoster
        this->StopReceivingData();
      }
    }
  }

  return S_OK;
}

CParameterCollection *CMPUrlSourceSplitter_Protocol_Http::GetConnectionParameters(void)
{
  CParameterCollection *result = new CParameterCollection();
  bool retval = (result != NULL);

  if (retval)
  {
    // add configuration parameters
    retval &= result->Append(this->configurationParameters);
  }

  if (retval)
  {
    // add current cookies
    if ((this->currentCookies != NULL) && (this->currentCookies->Count() != 0))
    {
      // first add count of cookies
      wchar_t *cookiesCountValue = FormatString(L"%u", this->currentCookies->Count());
      retval &= (cookiesCountValue != NULL);

      if (retval)
      {
        CParameter *cookiesCount = new CParameter(PARAMETER_NAME_HTTP_COOKIES_COUNT, cookiesCountValue);
        retval &= (cookiesCount != NULL);

        if (retval)
        {
          retval &= result->Add(cookiesCount);
        }

        if (!retval)
        {
          FREE_MEM_CLASS(cookiesCount);
        }
      }

      if (retval)
      {
        for (unsigned int i = 0; (retval && (i < this->currentCookies->Count())); i++)
        {
          CParameter *cookie = this->currentCookies->GetItem(i);
          wchar_t *name = FormatString(HTTP_COOKIE_FORMAT_PARAMETER_NAME, i);
          retval &= (name != NULL);

          if (retval)
          {
            CParameter *cookieToAdd = new CParameter(name, cookie->GetValue());
            retval = (cookieToAdd != NULL);

            if (retval)
            {
              retval &= result->Add(cookieToAdd);
            }

            if (!retval)
            {
              FREE_MEM_CLASS(cookieToAdd);
            }
          }

          FREE_MEM(name);
        }
      }
      FREE_MEM(cookiesCountValue);
    }
  }

  if (!retval)
  {
    FREE_MEM_CLASS(result);
  }
  
  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, this->configurationParameters);

  if (SUCCEEDED(result) && (parameters != NULL))
  {
    this->configurationParameters->Append(parameters);
  }

  unsigned int finishTime = UINT_MAX;
  if (SUCCEEDED(result))
  {
    finishTime = this->configurationParameters->GetValueUnsignedInt(PARAMETER_NAME_FINISH_TIME, true, UINT_MAX);
    if (finishTime != UINT_MAX)
    {
      unsigned int currentTime = GetTickCount();
      this->logger->Log(LOGGER_VERBOSE, L"%s: %s: finish time specified, current time: %u, finish time: %u, diff: %u (ms)", PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, currentTime, finishTime, finishTime - currentTime);
    }
  }

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  this->wholeStreamDownloaded = false;

  if (SUCCEEDED(result) && (this->mainCurlInstance == NULL))
  {
    this->mainCurlInstance = new CHttpCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
    CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);
  }

  if (SUCCEEDED(result) && (this->receivedData == NULL))
  {
    this->receivedData = new CLinearBuffer();
    result = (this->receivedData == NULL) ? E_POINTER : result;
  }

  if (SUCCEEDED(result))
  {
    result = (this->receivedData->InitializeBuffer(MINIMUM_RECEIVED_DATA_FOR_SPLITTER)) ? result : E_FAIL;
  }

  if (SUCCEEDED(result))
  {
    // set finish time, all methods must return before finish time
    this->mainCurlInstance->SetFinishTime(finishTime);

    this->mainCurlInstance->SetReceivedDataTimeout(this->receiveDataTimeout);
    this->mainCurlInstance->SetNetworkInterfaceName(this->configurationParameters->GetValue(PARAMETER_NAME_INTERFACE, true, NULL));

    if (this->currentCookies != NULL)
    {
      result = (this->mainCurlInstance->SetCurrentCookies(this->currentCookies)) ? S_OK : E_FAIL;
    }

    CHttpDownloadRequest *request = new CHttpDownloadRequest();
    CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      request->SetCookie(this->configurationParameters->GetValue(PARAMETER_NAME_HTTP_COOKIE, true, NULL));
      request->SetEndPosition(this->endStreamTime);
      request->SetHttpVersion(this->configurationParameters->GetValueLong(PARAMETER_NAME_HTTP_VERSION, true, HTTP_VERSION_DEFAULT));
      request->SetIgnoreContentLength((this->configurationParameters->GetValueLong(PARAMETER_NAME_HTTP_IGNORE_CONTENT_LENGTH, true, HTTP_IGNORE_CONTENT_LENGTH_DEFAULT) == 1L));
      request->SetReferer(this->configurationParameters->GetValue(PARAMETER_NAME_HTTP_REFERER, true, NULL));
      request->SetStartPosition(this->streamTime);
      request->SetUrl(this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL));
      request->SetUserAgent(this->configurationParameters->GetValue(PARAMETER_NAME_HTTP_USER_AGENT, true, NULL));

      result = (this->mainCurlInstance->Initialize(request)) ? S_OK : E_FAIL;
    }
    FREE_MEM_CLASS(request);

    if (SUCCEEDED(result))
    {
      // all parameters set
      // start receiving data

      result = (this->mainCurlInstance->StartReceivingData()) ? S_OK : E_FAIL;
    }

    if (SUCCEEDED(result))
    {
      // wait for HTTP status code

      long responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
      while (responseCode == 0)
      {
        responseCode = this->mainCurlInstance->GetHttpDownloadResponse()->GetResponseCode();
        if ((responseCode != 0) && ((responseCode < 200) || (responseCode >= 400)))
        {
          // response code 200 - 299 = OK
          // response code 300 - 399 = redirect (OK)
          this->logger->Log(LOGGER_VERBOSE, L"%s: %s: HTTP status code: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, responseCode);
          result = E_FAIL;
        }

        if ((responseCode == 0) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
        {
          // we received data too fast
          result = E_FAIL;
          break;
        }

        // wait some time
        Sleep(10);
      }
    }
  }

  if (FAILED(result))
  {
    this->StopReceivingData();
  }

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->receivedData);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::QueryStreamProgress(LONGLONG *total, LONGLONG *current)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, total);
  CHECK_POINTER_DEFAULT_HRESULT(result, current);

  if (result == S_OK)
  {
    *total = this->streamLength;
    *current = this->streamTime;
  }

  return result;
}
  
HRESULT CMPUrlSourceSplitter_Protocol_Http::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);

  if (result == S_OK)
  {
    // lock access to stream
    CLockMutex lock(this->lockMutex, INFINITE);

    availableLength->SetQueryResult(S_OK);
    availableLength->SetAvailableLength(((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetHttpDownloadResponse()->GetRangesSupported())) ? this->streamLength : this->streamTime);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }
 
  this->internalExitRequest = false;
  this->streamLength = 0;
  this->setLength = false;
  this->streamTime = 0;
  this->endStreamTime = 0;
  this->wholeStreamDownloaded = false;
  this->receiveDataTimeout = HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT;

  FREE_MEM_CLASS(this->receivedData);
  FREE_MEM_CLASS(this->currentCookies);

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

int64_t CMPUrlSourceSplitter_Protocol_Http::GetDuration(void)
{
  return DURATION_UNSPECIFIED;
}

void CMPUrlSourceSplitter_Protocol_Http::ReportStreamTime(uint64_t streamTime)
{
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Http::GetSeekingCapabilities(void)
{
  unsigned int result = SEEKING_METHOD_NONE;
  {
    // lock access to stream
    CLockMutex lock(this->lockMutex, INFINITE);

    result = ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetHttpDownloadResponse()->GetRangesSupported())) ? SEEKING_METHOD_POSITION : SEEKING_METHOD_NONE;
  }
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Http::SeekToTime(int64_t time)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu, to time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Http::SeekToPosition(int64_t start, int64_t end)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from position: %llu, to position: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, start, end);

  int64_t result = -1;

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  if (start >= this->streamLength)
  {
    result = -2;
  }
  else if (this->internalExitRequest)
  {
    // there is pending request exit request
    // set stream time to new value
    this->streamTime = start;
    this->endStreamTime = end;

    // connection should be reopened automatically
    result = start;
  }
  else
  {
    // only way how to "request" curl to interrupt transfer is set internalExitRequest to true
    this->internalExitRequest = true;

    // set stream time to new value
    this->streamTime = start;
    this->endStreamTime = end;

    // connection should be reopened automatically
    result = start;
  }

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, result);
  return result;
}

void CMPUrlSourceSplitter_Protocol_Http::SetSupressData(bool supressData)
{
  this->supressData = supressData;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Protocol_Http::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Http::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Http::Initialize(PluginConfiguration *configuration)
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

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_HTTP_RECEIVE_DATA_TIMEOUT, true, HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

  return S_OK;
}

// other methods