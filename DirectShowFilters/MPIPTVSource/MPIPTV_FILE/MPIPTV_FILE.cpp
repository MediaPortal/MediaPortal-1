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

#include "MPIPTV_FILE.h"
#include "Network.h"
#include "Utilities.h"

#include <WinInet.h>
#include <stdio.h>

// protocol implementation name
#define PROTOCOL_IMPLEMENTATION_NAME                                    _T("CMPIPTV_FILE")

PIProtocol CreateProtocolInstance(void)
{
  return new CMPIPTV_FILE;
}

void DestroyProtocolInstance(PIProtocol pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPIPTV_FILE *pClass = (CMPIPTV_FILE *)pProtocol;
    delete pClass;
  }
}

CMPIPTV_FILE::CMPIPTV_FILE()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  this->lockMutex = NULL;

  this->buffer.DeleteBuffer();
  this->receiveBuffer = NULL;

  this->filePath = NULL;
  this->fileStream = NULL;

  this->configurationParameters = new CParameterCollection();
  this->loadParameters = new CParameterCollection();
  this->receiveDataTimeout = 0;
  this->repeatCount = 0;
  this->repeatLimit = REPEAT_FOREVER;
  this->openConnetionMaximumAttempts = FILE_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT;
  this->endOfFileReachedLogged = 0;

  this->defaultBufferSize = 0;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTV_FILE::~CMPIPTV_FILE()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  this->buffer.DeleteBuffer();
  FREE_MEM(this->receiveBuffer);
  delete this->configurationParameters;
  delete this->loadParameters;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
}

int CMPIPTV_FILE::ClearSession(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  this->buffer.ClearBuffer();
  this->loadParameters->Clear();
  this->repeatCount = 0;
  this->endOfFileReachedLogged = 0;
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  return STATUS_OK;
}

int CMPIPTV_FILE::Initialize(HANDLE lockMutex, CParameterCollection *configuration)
{
  this->configurationParameters->Clear();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }
  this->configurationParameters->LogCollection(&this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

  long iptvBufferSize = this->configurationParameters->GetValueLong(CONFIGURATION_IPTV_BUFFER_SIZE, true, IPTV_BUFFER_SIZE_DEFAULT);
  long defaultMultiplier = this->configurationParameters->GetValueLong(CONFIGURATION_FILE_INTERNAL_BUFFER_MULTIPLIER, true, FILE_INTERNAL_BUFFER_MULTIPLIER_DEFAULT);
  this->receiveDataTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_FILE_RECEIVE_DATA_TIMEOUT, true, FILE_RECEIVE_DATA_TIMEOUT_DEFAULT);
  this->repeatLimit = this->configurationParameters->GetValueLong(CONFIGURATION_FILE_REPEAT_LIMIT, true, REPEAT_LIMIT_DEFAULT);
  this->openConnetionMaximumAttempts = this->configurationParameters->GetValueLong(CONFIGURATION_FILE_OPEN_CONNECTION_MAXIMUM_ATTEMPTS, true, FILE_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT);
  this->dumpInputPackets = this->configurationParameters->GetValueBool(CONFIGURATION_DUMP_INPUT_PACKETS, true, DUMP_INPUT_PACKETS_DEFAULT);

  iptvBufferSize = (iptvBufferSize <= 0) ? IPTV_BUFFER_SIZE_DEFAULT : iptvBufferSize;
  defaultMultiplier = (defaultMultiplier <= 0) ? FILE_INTERNAL_BUFFER_MULTIPLIER_DEFAULT : defaultMultiplier;
  this->repeatLimit  = (this->repeatLimit  < 0) ? REPEAT_LIMIT_DEFAULT : this->repeatLimit;
  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? FILE_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
  this->openConnetionMaximumAttempts = (this->openConnetionMaximumAttempts < 0) ? FILE_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT : this->openConnetionMaximumAttempts;

  this->defaultBufferSize = defaultMultiplier * iptvBufferSize;

  this->lockMutex = lockMutex;
  if (this->lockMutex == NULL)
  {
    return STATUS_ERROR;
  }

  if (this->defaultBufferSize > 0)
  {
    this->receiveBuffer = ALLOC_MEM_SET(this->receiveBuffer, char, this->defaultBufferSize, 0);
    if (this->receiveBuffer == NULL)
    {
      this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME, _T("cannot initialize internal buffer"));
      this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
      return STATUS_ERROR;
    }
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME, _T("internal buffer initialized"));

    // initialize internal buffer
    this->buffer.InitializeBuffer(this->defaultBufferSize);
    this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME, _T("internal linear buffer initialized"));
  }
  else
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME, _T("not valid size of IPTV buffer"));
    return STATUS_ERROR;
  }

  return STATUS_OK;
}

TCHAR *CMPIPTV_FILE::GetProtocolName(void)
{
  return Duplicate(CONFIGURATION_SECTION_FILE);
}

int CMPIPTV_FILE::ParseUrl(const TCHAR *url, const CParameterCollection *parameters)
{
  int result = STATUS_OK;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  this->ClearSession();
  this->loadParameters->Append((CParameterCollection *)parameters);
  this->loadParameters->LogCollection(&this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  ALLOC_MEM_DEFINE_SET(urlComponents, URL_COMPONENTS, 1, 0);
  if (urlComponents == NULL)
  {
    this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'url components'"));
    result = STATUS_ERROR;
  }

  if (result == STATUS_OK)
  {
    ZeroURL(urlComponents);
    urlComponents->dwStructSize = sizeof(URL_COMPONENTS);

    this->logger.Log(LOGGER_INFO, _T("%s: %s: url: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, url);

    if (!InternetCrackUrl(url, 0, 0, urlComponents))
    {
      this->logger.Log(LOGGER_ERROR, _T("%s: %s: InternetCrackUrl() error: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, GetLastError());
      result = STATUS_ERROR;
    }
  }

  if (result == STATUS_OK)
  {
    int length = urlComponents->dwSchemeLength + 1;
    ALLOC_MEM_DEFINE_SET(protocol, TCHAR, length, 0);
    if (protocol == NULL) 
    {
      this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'protocol'"));
      result = STATUS_ERROR;
    }

    if (result == STATUS_OK)
    {
      _tcsncat_s(protocol, length, urlComponents->lpszScheme, urlComponents->dwSchemeLength);

      if (_tcsncicmp(urlComponents->lpszScheme, _T("FILE"), urlComponents->dwSchemeLength) != 0)
      {
        // not supported protocol
        this->logger.Log(LOGGER_INFO, _T("%s: %s: unsupported protocol '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
        result = STATUS_ERROR;
      }
    }

    if (result == STATUS_OK)
    {
      // convert url to Unicode (if needed), because CoInternetParseUrl() works in Unicode

      size_t urlLength = _tcslen(url) + 1;
      wchar_t *parseUrl = ConvertToUnicode(url);
      if (parseUrl == NULL)
      {
        this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot convert url to wide character url"));
        result = STATUS_ERROR;
      }

      if (result == STATUS_OK)
      {
        // parsed file path should be shorter than wide character url
        ALLOC_MEM_DEFINE_SET(parsedFilePath, wchar_t, urlLength, 0);
        if (parsedFilePath == NULL)
        {
          this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for parsed file path"));
          result = STATUS_ERROR;
        }

        if (result == STATUS_OK)
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
              this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for parsed file path"));
              result = STATUS_ERROR;
            }

            if (result == STATUS_OK)
            {
              stored = 0;
              error = CoInternetParseUrl(parseUrl, PARSE_PATH_FROM_URL, 0, parsedFilePath, stored, &stored, 0);
              if (error != S_OK)
              {
                this->logger.Log(LOGGER_ERROR, _T("%s: %s: error occured while parsing file url, error: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, error);
                result = STATUS_ERROR;
              }
            }
          }
          else if (error != S_OK)
          {
            // error occured
            this->logger.Log(LOGGER_ERROR, _T("%s: %s: error occured while parsing file url, error: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, error);
            result = STATUS_ERROR;
          }
        }

        if (result == STATUS_OK)
        {
          // if we are here, then file url was successfully parsed
          // now store parsed url into this->filePath

#ifdef _MBCS
          this->filePath = ConvertToMultiByteW(parsedFilePath);
#else
          this->filePath = ConvertToUnicodeW(parsedFilePath);
#endif
          if (this->filePath == NULL)
          {
            this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot convert from Unicode file path to file path"));
            result = STATUS_ERROR;
          }
        }

        if (result == STATUS_OK)
        {
          this->logger.Log(LOGGER_INFO, _T("%s: %s: file path: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->filePath);
        }
        FREE_MEM(parsedFilePath);
      }
      FREE_MEM(parseUrl);
    }
    FREE_MEM(protocol);
  }

  FREE_MEM(urlComponents);
  
  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  return result;
}

int CMPIPTV_FILE::OpenConnection(void)
{
  int result = (this->filePath != NULL) ? STATUS_OK : STATUS_ERROR;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);

  if ((result == STATUS_OK) && (this->fileStream == NULL))
  {
    if (_tfopen_s(&this->fileStream, this->filePath, _T("rb")) != 0)
    {
      this->logger.Log(LOGGER_ERROR, _T("%s: %s: error occured while opening file, error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, errno);
      result = STATUS_ERROR;
    }
  }

  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  return result;
}

int CMPIPTV_FILE::IsConnected(void)
{
  return (this->fileStream != NULL);
}

void CMPIPTV_FILE::CloseConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);

  if (this->fileStream != NULL)
  {
    fclose(this->fileStream);
  }
  this->fileStream = NULL;

  FREE_MEM(this->filePath);
  
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);
}

void CMPIPTV_FILE::GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize)
{
  WaitForSingleObject(lockMutex, INFINITE);
  if (freeSpace != NULL)
  {
    *freeSpace = this->buffer.GetBufferFreeSpace();
  }
  if (occupiedSpace != NULL)
  {
    *occupiedSpace = this->buffer.GetBufferOccupiedSpace();
  }
  if (bufferSize != NULL)
  {
    *bufferSize = this->buffer.GetBufferSize();
  }
  ReleaseMutex(lockMutex);
}

void CMPIPTV_FILE::ReceiveData(bool *shouldExit)
{
  if (this->IsConnected())
  {
    unsigned int freeSpace = 0;
    unsigned int occupiedSpace = 0;
    unsigned int bufferSize = 0;

    this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
    this->logger.Log(LOGGER_DATA, _T("%s: %s: start, buffer size: %u, free buffer size: %u, occupied buffer size: %u, packet size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace, DVB_PACKET_SIZE);

    if (freeSpace >= DVB_PACKET_SIZE)
    {
      // there is some place in buffer

      // number of packets * this->packetSize = bytes to read from file
      unsigned int bytesToRead = (freeSpace / DVB_PACKET_SIZE) * DVB_PACKET_SIZE;

      WaitForSingleObject(this->lockMutex, INFINITE);
      size_t bytesRead = fread_s(this->receiveBuffer, bufferSize, sizeof(char), bytesToRead, this->fileStream);
      if (bytesRead != 0)
      {
        if (this->dumpInputPackets)
        {
          DumpInputPacket(this->GetInstanceId(), bytesRead, this->receiveBuffer);
        }
        this->buffer.AddToBuffer(this->receiveBuffer, bytesRead);
      }
      if (feof(this->fileStream))
      {
        if (!this->endOfFileReachedLogged)
        {
          this->logger.Log(LOGGER_INFO, _T("%s: %s: end of file reached, play count: %u, play limit: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->repeatCount + 1, this->repeatLimit);
          this->endOfFileReachedLogged = 1;
        }
        if ((this->repeatLimit == REPEAT_FOREVER) || (this->repeatCount < (this->repeatLimit - 1)))
        {
          // if end of file reached, then rewind file
          rewind(this->fileStream);
          this->repeatCount++;
          this->endOfFileReachedLogged = 0;
        }
      }
      this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
      ReleaseMutex(this->lockMutex);

      // all data already written
      this->logger.Log(LOGGER_DATA, _T("%s: %s: end, buffer size: %u, free buffer size: %u, occupied buffer size: %u, packet size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace, DVB_PACKET_SIZE);
    }
  }
  else
  {
    this->logger.Log(LOGGER_WARNING, _T("%s: %s: file not opened, opening file"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
    // re-open connection if previous is lost
    if (this->OpenConnection() != STATUS_OK)
    {
      this->CloseConnection();
    }
  }
}

unsigned int CMPIPTV_FILE::FillBuffer(IMediaSample *pSamp, char *pData, long cbData)
{
  return FillBufferStandard(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_NAME, this->lockMutex, &this->buffer, pSamp, pData, cbData);
}

unsigned int CMPIPTV_FILE::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

GUID CMPIPTV_FILE::GetInstanceId(void)
{
  return this->logger.loggerInstance;
}

unsigned int CMPIPTV_FILE::GetOpenConnectionMaximumAttempts(void)
{
  return this->openConnetionMaximumAttempts;
}
