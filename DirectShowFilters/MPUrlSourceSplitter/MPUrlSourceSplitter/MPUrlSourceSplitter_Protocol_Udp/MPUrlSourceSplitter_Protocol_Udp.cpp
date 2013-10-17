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

#include "MPUrlSourceSplitter_Protocol_Udp.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "MPUrlSourceSplitter_Protocol_Udp_Parameters.h"
#include "Parameters.h"

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Udpd"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Udp"
#endif

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_Udp(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Protocol_Udp *pClass = (CMPUrlSourceSplitter_Protocol_Udp *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Protocol_Udp::CMPUrlSourceSplitter_Protocol_Udp(CLogger *logger, CParameterCollection *configuration)
{
  this->configurationParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_UDP, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_UDP);
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
  
  this->receiveDataTimeout = UDP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  this->lockCurlMutex = CreateMutex(NULL, FALSE, NULL);
  this->internalExitRequest = false;
  this->wholeStreamDownloaded = false;
  this->mainCurlInstance = NULL;
  this->supressData = false;
  this->isConnected = false;
  this->streamLength = 0;
  this->bytePosition = 0;
  this->setLength = false;
  this->setEndOfStream = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Protocol_Udp::~CMPUrlSourceSplitter_Protocol_Udp()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  FREE_MEM_CLASS(this->mainCurlInstance);
  FREE_MEM_CLASS(this->configurationParameters);

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

bool CMPUrlSourceSplitter_Protocol_Udp::IsConnected(void)
{
  return ((this->isConnected) || (this->mainCurlInstance != NULL) || (this->wholeStreamDownloaded));
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::ParseUrl(const CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Udp::ReceiveData(CReceiveData *receiveData)
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

  if (this->IsConnected())
  {
    if (!this->wholeStreamDownloaded)
    {
      if (SUCCEEDED(result) && (this->mainCurlInstance != NULL))
      {
        if (this->mainCurlInstance->GetUdpDownloadResponse()->GetResultCode() == CURLE_OK)
        {
          CLockMutex curlLock(this->lockCurlMutex, INFINITE);

          unsigned int bufferSize = this->mainCurlInstance->GetUdpDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace();

          if (bufferSize > 0)
          {
            // create media packet
            // set values of media packet
            CMediaPacket *mediaPacket = new CMediaPacket();
            mediaPacket->GetBuffer()->InitializeBuffer(bufferSize);
            mediaPacket->GetBuffer()->AddToBufferWithResize(this->mainCurlInstance->GetUdpDownloadResponse()->GetReceivedData());
            mediaPacket->SetStart(this->bytePosition);
            mediaPacket->SetEnd(this->bytePosition + bufferSize - 1);

            if (!receiveData->GetMediaPacketCollection()->Add(mediaPacket))
            {
              FREE_MEM_CLASS(mediaPacket);
            }
            this->bytePosition += bufferSize;
          }

          this->mainCurlInstance->GetUdpDownloadResponse()->GetReceivedData()->ClearBuffer();
        }
      }

      // adjust total length (if necessary)
      if (SUCCEEDED(result) && (!this->setLength) && (this->bytePosition != 0))
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

      if ((this->mainCurlInstance != NULL) && (this->mainCurlInstance->GetCurlState() == CURL_STATE_RECEIVED_ALL_DATA))
      {
        if (this->mainCurlInstance->GetDownloadResponse()->GetResultCode() == CURLE_OK)
        {
          if (this->mainCurlInstance->GetDownloadResponse()->GetReceivedData()->GetBufferOccupiedSpace() == 0)
          {
            // all data read from CURL instance, received all data
            this->logger->Log(LOGGER_INFO, L"%s: %s: all data received", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
            FREE_MEM_CLASS(this->mainCurlInstance);
            
            // whole stream downloaded
            this->wholeStreamDownloaded = true;
            this->isConnected = false;

            // we can set total length
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
        else
        {
          // error occured while downloading
          this->logger->Log(LOGGER_ERROR, L"%s: %s: error while receiving data: %d, restarting download", PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->mainCurlInstance->GetDownloadResponse()->GetResultCode());
          FREE_MEM_CLASS(this->mainCurlInstance);

          this->isConnected = false;
        }
      }

      if (SUCCEEDED(result) && (this->isConnected) && (this->mainCurlInstance == NULL))
      {
        FREE_MEM_CLASS(this->mainCurlInstance);
        // no CURL instance exists, we finished or not started download
        // start another download

        // create CURL instance
        this->mainCurlInstance = new CUdpCurlInstance(this->logger, this->lockCurlMutex, PROTOCOL_IMPLEMENTATION_NAME, L"Main");
        CHECK_POINTER_HRESULT(result, this->mainCurlInstance, result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          CUdpDownloadRequest *request = new CUdpDownloadRequest();
          CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

          if (SUCCEEDED(result))
          {
            request->SetUrl(this->configurationParameters->GetValue(PARAMETER_NAME_URL, true, NULL));

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

  return result;
}

CParameterCollection *CMPUrlSourceSplitter_Protocol_Udp::GetConnectionParameters(void)
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

unsigned int CMPUrlSourceSplitter_Protocol_Udp::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::StartReceivingData(CParameterCollection *parameters)
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

HRESULT CMPUrlSourceSplitter_Protocol_Udp::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);

  // lock access to stream
  CLockMutex lock(this->lockMutex, INFINITE);

  FREE_MEM_CLASS(this->mainCurlInstance);

  this->isConnected = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::QueryStreamProgress(LONGLONG *total, LONGLONG *current)
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
  
HRESULT CMPUrlSourceSplitter_Protocol_Udp::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
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

HRESULT CMPUrlSourceSplitter_Protocol_Udp::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  this->internalExitRequest = false;
  this->streamLength = 0;
  this->bytePosition = 0;
  this->wholeStreamDownloaded = false;
  this->receiveDataTimeout = UDP_RECEIVE_DATA_TIMEOUT_DEFAULT;
  this->isConnected = false;
  this->setLength = false;
  this->setEndOfStream = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

int64_t CMPUrlSourceSplitter_Protocol_Udp::GetDuration(void)
{
  return DURATION_LIVE_STREAM;
}

void CMPUrlSourceSplitter_Protocol_Udp::ReportStreamTime(uint64_t streamTime)
{
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_Udp::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_NONE;
}

int64_t CMPUrlSourceSplitter_Protocol_Udp::SeekToTime(int64_t time)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_Udp::SeekToPosition(int64_t start, int64_t end)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from position: %llu, to position: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, start, end);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_POSITION_NAME, result);
  return result;
}

void CMPUrlSourceSplitter_Protocol_Udp::SetSupressData(bool supressData)
{
  this->supressData = supressData;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Protocol_Udp::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_Udp::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_Udp::Initialize(PluginConfiguration *configuration)
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

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_UDP_RECEIVE_DATA_TIMEOUT, true, UDP_RECEIVE_DATA_TIMEOUT_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? UDP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

  return S_OK;
}

// other methods
