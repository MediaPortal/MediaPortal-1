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

#pragma warning(pop)

// protocol implementation name
#ifdef _DEBUG
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_Filed"
#else
#define PROTOCOL_IMPLEMENTATION_NAME                                    L"MPUrlSourceSplitter_Protocol_File"
#endif

PIPlugin CreatePluginInstance(CLogger *logger, CParameterCollection *configuration)
{
  return new CMPUrlSourceSplitter_Protocol_File(logger, configuration);
}

void DestroyPluginInstance(PIPlugin pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPUrlSourceSplitter_Protocol_File *pClass = (CMPUrlSourceSplitter_Protocol_File *)pProtocol;
    delete pClass;
  }
}

CMPUrlSourceSplitter_Protocol_File::CMPUrlSourceSplitter_Protocol_File(CLogger *logger, CParameterCollection *configuration)
{
  this->configurationParameters = new CParameterCollection();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }

  this->logger = new CLogger(logger);
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  wchar_t *version = GetVersionInfo(COMMIT_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_FILE, DATE_INFO_MP_URL_SOURCE_SPLITTER_PROTOCOL_FILE);
  if (version != NULL)
  {
    this->logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME, version);
  }
  FREE_MEM(version);

  this->filePath = NULL;
  this->fileStream = NULL;

  this->receiveDataTimeout = 0;
  this->fileLength = 0;
  this->setLength = false;
  this->streamTime = 0;
  this->lockMutex = CreateMutex(NULL, FALSE, NULL);
  this->wholeStreamDownloaded = false;
  this->supressData = false;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPUrlSourceSplitter_Protocol_File::~CMPUrlSourceSplitter_Protocol_File()
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  FREE_MEM_CLASS(this->configurationParameters);

  if (this->lockMutex != NULL)
  {
    CloseHandle(this->lockMutex);
    this->lockMutex = NULL;
  }

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  delete this->logger;
  this->logger = NULL;
}

// IProtocol interface

bool CMPUrlSourceSplitter_Protocol_File::IsConnected(void)
{
  return ((this->fileStream != NULL) || (this->wholeStreamDownloaded));
}

HRESULT CMPUrlSourceSplitter_Protocol_File::ParseUrl(const CParameterCollection *parameters)
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
HRESULT CMPUrlSourceSplitter_Protocol_File::ReceiveData(CReceiveData *receiveData)
{
  CLockMutex lock(this->lockMutex, INFINITE);

  /*

  this should never happen, because supression of data can occure only when seeking by time

  */

  if (!this->supressData)
  {
    if (this->IsConnected())
    {
      if (!this->wholeStreamDownloaded)
      {
        if (!this->setLength)
        {
          receiveData->GetTotalLength()->SetTotalLength(this->fileLength, false);
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
            CMediaPacket *mediaPacket = new CMediaPacket();
            mediaPacket->GetBuffer()->InitializeBuffer(bytesRead);
            mediaPacket->GetBuffer()->AddToBuffer(receiveBuffer, bytesRead);

            mediaPacket->SetStart(this->streamTime);
            mediaPacket->SetEnd(this->streamTime + bytesRead - 1);

            if (!receiveData->GetMediaPacketCollection()->Add(mediaPacket))
            {
              FREE_MEM_CLASS(mediaPacket);
            }

            this->streamTime += bytesRead;
          }
          FREE_MEM(receiveBuffer);
        }
        else
        {
          this->wholeStreamDownloaded = true;

          // notify filter the we reached end of stream
          int64_t streamTime = this->streamTime;
          this->streamTime = this->fileLength;

          receiveData->GetEndOfStreamReached()->SetStreamPosition(max(0, streamTime - 1));
        }
      }
    }
  }

  return S_OK;
}

CParameterCollection *CMPUrlSourceSplitter_Protocol_File::GetConnectionParameters(void)
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

unsigned int CMPUrlSourceSplitter_Protocol_File::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::StartReceivingData(CParameterCollection *parameters)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->filePath);

  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_START_RECEIVING_DATA_NAME);

  this->wholeStreamDownloaded = false;

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
  CLockMutex lock(this->lockMutex, INFINITE);

  // close connection and set that whole stream downloaded
  if (this->fileStream != NULL)
  {
    fclose(this->fileStream);
  }
  this->fileStream = NULL;

  FREE_MEM(this->filePath);

  this->wholeStreamDownloaded = true;

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_STOP_RECEIVING_DATA_NAME);
  return S_OK;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::QueryStreamProgress(LONGLONG *total, LONGLONG *current)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, total);
  CHECK_POINTER_DEFAULT_HRESULT(result, current);

  if (result == S_OK)
  {
    *total = this->fileLength;
    *current = this->streamTime;
  }

  return result;
}
  
HRESULT CMPUrlSourceSplitter_Protocol_File::QueryStreamAvailableLength(CStreamAvailableLength *availableLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, availableLength);

  if (result == S_OK)
  {
    availableLength->SetQueryResult(S_OK);
    availableLength->SetAvailableLength(this->fileLength);
  }

  return result;
}

HRESULT CMPUrlSourceSplitter_Protocol_File::ClearSession(void)
{
  this->logger->Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->StopReceivingData();
  }

  this->fileLength = 0;
  this->setLength = false;
  this->streamTime = 0;
  this->wholeStreamDownloaded = false;
  this->configurationParameters->Clear();

  this->logger->Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);
  return S_OK;
}

int64_t CMPUrlSourceSplitter_Protocol_File::GetDuration(void)
{
  return DURATION_UNSPECIFIED;
}

void CMPUrlSourceSplitter_Protocol_File::ReportStreamTime(uint64_t streamTime)
{
}

// ISeeking interface

unsigned int CMPUrlSourceSplitter_Protocol_File::GetSeekingCapabilities(void)
{
  return SEEKING_METHOD_POSITION;
}

int64_t CMPUrlSourceSplitter_Protocol_File::SeekToTime(int64_t time)
{
  this->logger->Log(LOGGER_VERBOSE, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME);
  this->logger->Log(LOGGER_VERBOSE, L"%s: %s: from time: %llu, to time: %llu", PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, time);

  int64_t result = -1;

  this->logger->Log(LOGGER_VERBOSE, METHOD_END_INT64_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_SEEK_TO_TIME_NAME, result);
  return result;
}

int64_t CMPUrlSourceSplitter_Protocol_File::SeekToPosition(int64_t start, int64_t end)
{
  int64_t result = -1;

  if (this->IsConnected())
  {
    {
      // lock access to file
      CLockMutex lock(this->lockMutex, INFINITE);

      result = (_fseeki64(this->fileStream, start, SEEK_SET) == 0) ? start : -1;

      if (result >= 0)
      {
        this->wholeStreamDownloaded = false;
        this->streamTime = start;
      }
    }
  }

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

HRESULT CMPUrlSourceSplitter_Protocol_File::Initialize(PluginConfiguration *configuration)
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

  this->receiveDataTimeout = this->configurationParameters->GetValueLong(PARAMETER_NAME_FILE_RECEIVE_DATA_TIMEOUT, true, FILE_RECEIVE_DATA_TIMEOUT_DEFAULT);

  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? FILE_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;

  return S_OK;
}
