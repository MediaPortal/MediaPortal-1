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

#include "MPUrlSourceSplitter_Protocol_File.h"
#include "Utilities.h"
#include "LockMutex.h"
#include "VersionInfo.h"
#include "MPUrlSourceSplitter_Protocol_File_Parameters.h"
#include "Parameters.h"
#include "ProtocolPluginConfiguration.h"

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Filed"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_File"
#endif

CPlugin *CreatePlugin(HRESULT* result, CLogger* logger, CParameterCollection* configuration)
{
  return new CMPUrlSourceSplitter_Protocol_File(result, logger, configuration);
}

void DestroyPlugin(CPlugin *plugin)
{ 
  if (plugin != NULL)
  {
    CMPUrlSourceSplitter_Protocol_File *pClass = (CMPUrlSourceSplitter_Protocol_File *)plugin;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Protocol_File::CMPUrlSourceSplitter_Protocol_File(HRESULT* result, CLogger *logger, CParameterCollection *configuration)
  : CProtocolPlugin(result, logger, configuration)
{
  this->filePath = NULL;
  this->fileStream = NULL;

  this->receiveDataTimeout = 0;
  this->fileLength = 0;
  this->setLength = false;
  this->streamTime = 0;
  this->lockMutex = NULL;
  this->supressData = false;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->logger->Log(LOGGER_INFO, METHOD_CONSTRUCTOR_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, this);

    wchar_t* version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_FILE, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_FILE);
    if (version != NULL)
    {
      this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
    }
    FREE_MEM(version);

    this->lockMutex = CreateMutex(NULL, FALSE, NULL);
    this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
  }
}

CMPUrlSourceSplitter_Protocol_File::~CMPUrlSourceSplitter_Protocol_File()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->lockMutex != NULL)
  {
    CloseHandle(this->lockMutex);
    this->lockMutex = NULL;
  }

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->logger, this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME));
}

// IProtocol interface

ProtocolConnectionState CMPUrlSourceSplitter_Protocol_File::GetConnectionState(void)
{
  if (this->fileStream != NULL)
    return Opened;
  else
    return None;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::ParseUrl(const CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);
  this->ClearSession();

  if (SUCCEEDED(result))
  {
    this->configuration->Clear();

    CProtocolPluginConfiguration* protocolConfiguration = new CProtocolPluginConfiguration(&result, (CParameterCollection*)parameters);
    CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_OUTOFMEMORY);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->Initialize(protocolConfiguration));
    FREE_MEM_CLASS(protocolConfiguration);
  }

  const wchar_t *url = this->configuration->GetValue(PARAMETER_NAME_URL, true, NULL);
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

      if (SUCCEEDED(result))
      {
        // convert url to Unicode (if needed), because CoInternetParseUrl() works in Unicode

        size_t urlLength = wcslen(url) + 1;
        wchar_t *parseUrl = ConvertToUnicode(url);
        if (parseUrl == NULL)
        {
          this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot convert url to wide character url");
          result = E_OUTOFMEMORY;
        }

        if (SUCCEEDED(result))
        {
          // parsed file path should be shorter than wide character url
          ALLOC_MEM_DEFINE_SET(parsedFilePath, wchar_t, urlLength, 0);
          if (parsedFilePath == NULL)
          {
            this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for parsed file path");
            result = E_OUTOFMEMORY;
          }

          if (SUCCEEDED(result))
          {
            DWORD stored = 0;
            HRESULT error = CoInternetParseUrl(parseUrl, PARSE_PATH_FROM_URL, 0, parsedFilePath, urlLength, &stored, 0);
            if (error == E_POINTER)
            {
              // not enough memory in buffer, in stored is required buffer size
              FREE_MEM(parsedFilePath);
              parsedFilePath = ALLOC_MEM_SET(parsedFilePath, wchar_t, stored, 0);
              if (parsedFilePath == NULL)
              {
                this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot allocate memory for parsed file path");
                result = E_OUTOFMEMORY;
              }

              if (SUCCEEDED(result))
              {
                stored = 0;
                error = CoInternetParseUrl(parseUrl, PARSE_PATH_FROM_URL, 0, parsedFilePath, stored, &stored, 0);
                if (error != S_OK)
                {
                  this->logger->Log(LOGGER_ERROR, L"%s: %s: error occured while parsing file url, error: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, error);
                  result = error;
                }
              }
            }
            else if (error != S_OK)
            {
              // error occured
              this->logger->Log(LOGGER_ERROR, L"%s: %s: error occured while parsing file url, error: %u", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, error);
              result = error;
            }
          }

          if (SUCCEEDED(result))
          {
            // if we are here, then file url was successfully parsed
            // now store parsed url into this->filePath

            this->filePath = ConvertToUnicodeW(parsedFilePath);
            if (this->filePath == NULL)
            {
              this->logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, L"cannot convert from Unicode file path to file path");
              result = E_OUTOFMEMORY;
            }
          }

          if (SUCCEEDED(result))
          {
            this->logger->Log(LOGGER_INFO, L"%s: %s: file path: %s", PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->filePath);
          }
          FREE_MEM(parsedFilePath);
        }
        FREE_MEM(parseUrl);
      }
      FREE_MEM(protocol);
    }

    FREE_MEM(urlComponents);
  }
  
  this->logger->Log(LOGGER_INFO, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  return result;
}

// this method implementation can block call for very long time
// reading from file is not implemented as asynchronous
HRESULT CMPUrlSourceSplitter_Protocol_File::ReceiveData(CStreamPackage* streamPackage)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamPackage);
  if (SUCCEEDED(result))
  {
    LOCK_MUTEX(this->lockMutex, INFINITE);

    /*

    this should never happen, because supression of data can occure only when seeking by time

    */

    // file has always one stream
    if (!this->supressData)
    {
      if (this->GetConnectionState() == Opened)
      {
        if (!this->IsWholeStreamDownloaded())
        {
          if (!this->setLength)
          {
            receiveData->GetStreams()->GetItem(0)->GetTotalLength()->SetTotalLength(this->fileLength, false);
            this->setLength = true;
          }

          if (!feof(this->fileStream))
          {
            unsigned int bytesToRead = DEFAULT_BUFFER_SIZE; // 32 kB

            ALLOC_MEM_DEFINE_SET(receiveBuffer, unsigned char, bytesToRead, 0);
            unsigned int bytesRead = fread_s(receiveBuffer, bytesToRead, sizeof(char), bytesToRead, this->fileStream);
            if (bytesRead != 0)
            {
              // create media packet
              // set values of media packet
              CMediaPacket* mediaPacket = new CMediaPacket();
              mediaPacket->GetBuffer()->InitializeBuffer(bytesRead);
              mediaPacket->GetBuffer()->AddToBuffer(receiveBuffer, bytesRead);

              mediaPacket->SetStart(this->streamTime);
              mediaPacket->SetEnd(this->streamTime + bytesRead - 1);

              if (!receiveData->GetStreams()->GetItem(0)->GetMediaPacketCollection()->Add(mediaPacket))
              {
                FREE_MEM_CLASS(mediaPacket);
              }
              
              this->streamTime += bytesRead;
            }
            FREE_MEM(receiveBuffer);
          }
          else
          {
            this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

            // notify filter the we reached end of stream
            int64_t streamTime = this->streamTime;
            this->streamTime = this->fileLength;

            receiveData->GetStreams()->GetItem(0)->GetEndOfStreamReached()->SetStreamPosition(max(0, streamTime - 1));
          }
        }
      }
    }
    UNLOCK_MUTEX(this->lockMutex)
  }

  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::GetConnectionParameters(CParameterCollection* parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, parameters);

  CHECK_CONDITION_HRESULT(result, parameters->Append(this->configuration), result, E_OUTOFMEMORY);

  return result;
}

// ISimpleProtocol interface

unsigned int CMPUrlSourceSplitter_Protocol_File::GetOpenConnectionTimeout(void)
{
  return this->receiveDataTimeout;
}

unsigned int CMPUrlSourceSplitter_Protocol_File::GetOpenConnectionSleepTime(void)
{
  return 0;
}

unsigned int CMPUrlSourceSplitter_Protocol_File::GetTotalReopenConnectionTimeout(void)
{
  return 0;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->filePath);

  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  this->flags &= ~(PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED);

  if ((result == S_OK) && (this->fileStream == NULL))
  {
    if (_wfopen_s(&this->fileStream, this->filePath, L"rb") != 0)
    {
      this->logger->Log(LOGGER_ERROR, L"%s: %s: error occured while opening file, error: %i", PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, errno);
      result = E_FAIL;
    }

    if (result == S_OK)
    {
      LARGE_INTEGER size;
      size.QuadPart = 0;

      // open or create file
      HANDLE hLogFile = CreateFile(this->filePath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);

      if (hLogFile != INVALID_HANDLE_VALUE)
      {
        if (!GetFileSizeEx(hLogFile, &size))
        {
          // error occured while getting file size
          size.QuadPart = 0;
        }

        CloseHandle(hLogFile);
        hLogFile = INVALID_HANDLE_VALUE;

        this->fileLength = size.QuadPart;
      }
    }
  }

  this->logger->Log(SUCCEEDED(result) ? LOGGER_INFO : LOGGER_ERROR, SUCCEEDED(result) ? METHOD_END_FORMAT : METHOD_END_FAIL_HRESULT_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME, result);
  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::StopReceivingData(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  LOCK_MUTEX(this->lockMutex, INFINITE);

  // close connection and set that whole stream downloaded
  if (this->fileStream != NULL)
  {
    fclose(this->fileStream);
  }
  this->fileStream = NULL;

  FREE_MEM(this->filePath);

  this->flags |= PROTOCOL_PLUGIN_FLAG_WHOLE_STREAM_DOWNLOADED | PROTOCOL_PLUGIN_FLAG_END_OF_STREAM_REACHED;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  UNLOCK_MUTEX(this->lockMutex);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::QueryStreamProgress(CStreamProgress *streamProgress)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streamProgress);
  CHECK_CONDITION_HRESULT(result, streamProgress->GetStreamId() == 0, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    streamProgress->SetTotalLength(this->fileLength);
    streamProgress->SetCurrentLength(this->streamTime);
  }

  return result;
}
  
void CMPUrlSourceSplitter_Protocol_File::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->GetConnectionState() == Opened)
  {
    this->StopReceivingData();
  }

  this->fileLength = 0;
  this->setLength = false;
  this->streamTime = 0;
  this->configuration->Clear();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
}

int64_t CMPUrlSourceSplitter_Protocol_File::GetDuration(void)
{
  return DURATION_UNSPECIFIED;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::GetStreamInformation(CStreamInformationCollection* streams)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, streams);

  if (SUCCEEDED(result))
  {
    CStreamInformation* streamInfo = new CStreamInformation(&result);
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

unsigned int CMPUrlSourceSplitter_Protocol_File::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_POSITION;
}

int64_t CMPUrlSourceSplitter_Protocol_File::SeekToTime(unsigned int streamId, int64_t time)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu, to time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

void CMPUrlSourceSplitter_Protocol_File::SetSupressData(bool supressData)
{
  this->supressData = supressData;
}

// IPlugin interface

const wchar_t *CMPUrlSourceSplitter_Protocol_File::GetName(void)
{
  return PROTOCOL_NAME;
}

GUID CMPUrlSourceSplitter_Protocol_File::GetInstanceId(void)
{
  return this->logger->GetLoggerInstanceId();
}

HRESULT CMPUrlSourceSplitter_Protocol_File::Initialize(CPluginConfiguration *configuration)
{
  HRESULT result = __super::Initialize(configuration);

  CPluginConfiguration *protocolConfiguration = (CProtocolPluginConfiguration *)configuration;
  CHECK_POINTER_HRESULT(result, protocolConfiguration, result, E_INVALIDARG);
  CHECK_POINTER_HRESULT(result, this->lockMutex, result, E_NOT_VALID_STATE);

  this->logger->SetParameters(protocolConfiguration->GetConfiguration());

  if (this->lockMutex == NULL)
  {
    return E_FAIL;
  }

  this->configuration->LogCollection(this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

  this->receiveDataTimeout = this->configuration->GetValueLong(PARAMETER_NAME_FILE_RECEIVE_DATA_TIMEOUT, true, FILE_RECEIVE_DATA_TIMEOUT_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? FILE_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

  return S_OK;
}

const wchar_t* CMPUrlSourceSplitter_Protocol_File::GetModuleName(void)
{
  return PROTOCOL_IMPLEMENTATION_NAME;
}

const wchar_t* CMPUrlSourceSplitter_Protocol_File::GetStoreFileNamePart(void)
{
  return PROTOCOL_STORE_FILE_NAME_PART;
}