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

#include "MPIPTV_HTTP.h"
#include "Network.h"
#include "Utilities.h"

#include <stdio.h>

// protocol implementation name
#define PROTOCOL_IMPLEMENTATION_NAME                                    _T("CMPIPTV_HTTP")

PIProtocol CreateProtocolInstance(void)
{
  return new CMPIPTV_HTTP;
}

void DestroyProtocolInstance(PIProtocol pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPIPTV_HTTP *pClass = (CMPIPTV_HTTP *)pProtocol;
    delete pClass;
  }
}

CMPIPTV_HTTP::CMPIPTV_HTTP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  this->lockMutex = NULL;

  this->buffer.DeleteBuffer();
  this->chunkedBuffer.DeleteBuffer();
  this->receiveBuffer = NULL;

  this->m_socket = NULL;
  this->server = NULL;
  this->serverPort = 0;
  this->serverGetString = NULL;
  this->chunkedEncoding = false;
  this->receivedHttpResponse = false;
  this->configurationParameters = new CParameterCollection();
  this->loadParameters = new CParameterCollection();
  this->receiveDataTimeout = 0;
  this->openConnetionMaximumAttempts = HTTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT;

  this->defaultBufferSize = 0;
  this->maxBufferSize = 0;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTV_HTTP::~CMPIPTV_HTTP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  FREE_MEM(this->server);
  FREE_MEM(this->serverGetString);
  this->buffer.DeleteBuffer();
  this->chunkedBuffer.DeleteBuffer();
  FREE_MEM(this->receiveBuffer);
  delete this->configurationParameters;
  delete this->loadParameters;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
}

int CMPIPTV_HTTP::Initialize(HANDLE lockMutex, CParameterCollection *configuration)
{
  this->configurationParameters->Clear();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }
  this->configurationParameters->LogCollection(&this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

  long iptvBufferSize = this->configurationParameters->GetValueLong(CONFIGURATION_IPTV_BUFFER_SIZE, true, IPTV_BUFFER_SIZE_DEFAULT);
  long defaultMultiplier = this->configurationParameters->GetValueLong(CONFIGURATION_HTTP_INTERNAL_BUFFER_MULTIPLIER, true, HTTP_INTERNAL_BUFFER_MULTIPLIER_DEFAULT);
  long maxMultiplier = this->configurationParameters->GetValueLong(CONFIGURATION_HTTP_INTERNAL_BUFFER_MAX_MULTIPLIER, true, HTTP_INTERNAL_BUFFER_MAX_MULTIPLIER_DEFAULT);
  this->receiveDataTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_HTTP_RECEIVE_DATA_TIMEOUT, true, HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT);
  this->openConnetionMaximumAttempts = this->configurationParameters->GetValueLong(CONFIGURATION_HTTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS, true, HTTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT);
  this->dumpInputPackets = this->configurationParameters->GetValueBool(CONFIGURATION_DUMP_INPUT_PACKETS, true, DUMP_INPUT_PACKETS_DEFAULT);

  iptvBufferSize = (iptvBufferSize <= 0) ? IPTV_BUFFER_SIZE_DEFAULT : iptvBufferSize;
  defaultMultiplier = (defaultMultiplier <= 0) ? HTTP_INTERNAL_BUFFER_MULTIPLIER_DEFAULT : defaultMultiplier;
  maxMultiplier = (maxMultiplier < defaultMultiplier) ? defaultMultiplier : maxMultiplier;
  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? HTTP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
  this->openConnetionMaximumAttempts = (this->openConnetionMaximumAttempts < 0) ? HTTP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT : this->openConnetionMaximumAttempts;

  this->defaultBufferSize = defaultMultiplier * iptvBufferSize;
  this->maxBufferSize = maxMultiplier * iptvBufferSize;

  this->lockMutex = lockMutex;
  if (this->lockMutex == NULL)
  {
    return STATUS_ERROR;
  }

  if (this->defaultBufferSize > 0)
  {
    this->receiveBuffer = ALLOC_MEM(char, this->defaultBufferSize);
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
    this->logger.Log(LOGGER_INFO, METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);
    return STATUS_ERROR;
  }

  return STATUS_OK;
}

TCHAR *CMPIPTV_HTTP::GetProtocolName(void)
{
  return Duplicate(CONFIGURATION_SECTION_HTTP);
}

int CMPIPTV_HTTP::ClearSession(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  this->buffer.DeleteBuffer();
  this->buffer.InitializeBuffer(this->defaultBufferSize);
  this->loadParameters->Clear();

  FREE_MEM(this->server);
  FREE_MEM(this->serverGetString);
  this->serverPort = 0;
  this->chunkedEncoding = false;
  this->receivedHttpResponse = false;
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  return STATUS_OK;
}

int CMPIPTV_HTTP::ParseUrl(const TCHAR *url, const CParameterCollection *parameters)
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

      if (_tcsncicmp(urlComponents->lpszScheme, _T("HTTP"), urlComponents->dwSchemeLength) != 0)
      {
        // not supported protocol
        this->logger.Log(LOGGER_INFO, _T("%s: %s: unsupported protocol '%s'"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, protocol);
        result = STATUS_ERROR;
      }
    }
    FREE_MEM(protocol);

    if (result == STATUS_OK)
    {
      length = urlComponents->dwHostNameLength + 1;
      this->server = ALLOC_MEM_SET(this->server, TCHAR, length, 0);
      if (this->server == NULL) 
      {
        this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'server'"));
        result = STATUS_ERROR;
      }
    }

    if (result == STATUS_OK)
    {
      _tcsncat_s(this->server, length, urlComponents->lpszHostName, urlComponents->dwHostNameLength);
      
      if ((urlComponents->dwUrlPathLength + urlComponents->dwExtraInfoLength) != 0)
      {
        length = urlComponents->dwUrlPathLength + urlComponents->dwExtraInfoLength + 1;
        this->serverGetString = ALLOC_MEM_SET(this->serverGetString, TCHAR, length, 0);
        if (this->serverGetString == NULL)
        {
          this->logger.Log(LOG_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'server get string'"));
          result = STATUS_ERROR;
        }
        else
        {
          _tcsncat_s(this->serverGetString, length, urlComponents->lpszUrlPath, urlComponents->dwUrlPathLength + urlComponents->dwExtraInfoLength);
        }
      }
    }

    if (result == STATUS_OK)
    {
      //_tcsncat_s(this->serverGetString, length, urlComponents->lpszUrlPath, urlComponents->dwUrlPathLength + urlComponents->dwExtraInfoLength);
      this->serverPort = urlComponents->nPort;

      this->logger.Log(LOGGER_INFO, _T("%s: %s: server address: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->server);
      this->logger.Log(LOGGER_INFO, _T("%s: %s: server port: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->serverPort);
      this->logger.Log(LOGGER_INFO, _T("%s: %s: server get string: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->serverGetString);
    }
  }

  FREE_MEM(urlComponents);
  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);

  return result;
}

int CMPIPTV_HTTP::OpenConnection(void)
{
  int result = STATUS_OK;
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);

  WSADATA wsaData;
  if (WSAStartup(MAKEWORD(2, 2), &wsaData) != NO_ERROR)
  {
    this->logger.Log(LOGGER_ERROR, _T("%s: %s: WSAStartup() error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, WSAGetLastError());
    result = STATUS_ERROR;
  }

  if (result == STATUS_OK)
  {
    ADDRINFOT hints;
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    // chain of ADDRINFO structures for server
    ADDRINFOT *server = NULL;
    ADDRINFOT *networkInterface = NULL;

    // variable for cycle
    ADDRINFOT *serverAddr = NULL;

    // set passive flag and resolve local address
    hints.ai_flags = AI_PASSIVE;

    int errorCode = GetIpAddress(this->server, this->serverPort, &server, &hints);
    if (errorCode != 0)
    {
      this->logger.Log(LOGGER_ERROR, _T("%s: %s: getaddrinfo() for server error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, errorCode);
      result = STATUS_ERROR;
    }

    CParameterCollection *parameters = new CParameterCollection();
    parameters->Append(this->configurationParameters);
    parameters->Append(this->loadParameters);

    bool connected = false;
    for(serverAddr = server; ((serverAddr != NULL) && (result == STATUS_OK)); serverAddr = serverAddr->ai_next)
    {
      // set connected to true
      connected = true;

      if (parameters != NULL)
      {
        // we have set some parameters
        // get interface parameter
        PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
        if (networkInterfaceParameter != NULL)
        {
          TCHAR *networkInterfaceAddress = GetInterfaceAddress(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, networkInterfaceParameter->GetValue(), serverAddr->ai_family);
          int errorCode = GetIpAddress(networkInterfaceAddress, 0, &networkInterface, &hints);
          if (errorCode != 0)
          {
            this->logger.Log(LOGGER_ERROR, _T("%s: %s: getaddrinfo() for network interface error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, errorCode);
            connected = false;
          }

          if (connected)
          {
            this->logger.Log(LOGGER_INFO, _T("%s: %s: try to bind on interface '%s', address: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, networkInterfaceParameter->GetValue(), networkInterfaceAddress);
          }
          FREE_MEM(networkInterfaceAddress);
        }
      }

      // create socket
      if (connected)
      {
        this->m_socket = CreateSocket(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, serverAddr->ai_family, SOCK_STREAM, IPPROTO_TCP);
        if (this->m_socket == INVALID_SOCKET)
        {
          this->logger.Log(LOGGER_ERROR, _T("%s: %s: socket() error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, WSAGetLastError());
          connected = false;
        }
      }

      if (connected)
      {
        DWORD dw = TRUE;
        int dwLen = sizeof(dw);

        connected &= (SetSocketOption(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("SO_REUSEADDR"), this->m_socket, SOL_SOCKET, SO_REUSEADDR, (const char*)&dw, dwLen) == 0);

        if (connected)
        {
          connected &= (GetSocketOption(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("SO_RCVBUF"), this->m_socket, SOL_SOCKET, SO_RCVBUF, (char *)&dw, &dwLen) == 0);
        }

        if (connected)
        {
          this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: socket receive buffer is: %d"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, dw);
          this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: trying to set receive buffer to %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->defaultBufferSize);

          // set socket buffer size
          dw = this->defaultBufferSize;
          dwLen = sizeof(dw);

          connected &= (SetAndCheckSocketOption(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("SO_RCVBUF"), this->m_socket, SOL_SOCKET, SO_RCVBUF, (const char*)&dw, dwLen) == 0);
        }

        if (connected)
        {
          // set socket to non-blocking mode
          connected &= (SetBlockingMode(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->m_socket, false) == 0);
        }

        if ((connected) && (networkInterface != NULL))
        {
          // is there request to bing to specific interface

          connected &= (Bind(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->m_socket, networkInterface->ai_addr, networkInterface->ai_addrlen) == 0);
        }
      }

      if (networkInterface != NULL)
      {
        FreeAddrInfo(networkInterface);
        networkInterface = NULL;
      }

      unsigned int receiveDataTimeout = this->GetReceiveDataTimeout() / 2000;

      if (this->m_socket != INVALID_SOCKET)
      {
        // connect to remote server
        connected &= (Connect(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->m_socket, serverAddr->ai_addr, serverAddr->ai_addrlen, receiveDataTimeout, false, true) == 0);
      }

      if (connected)
      {
        // successfuly connected, socket in writable state
        // send request

        TCHAR *tempBuffer = NULL;

        // get_string must be at least '/'
        if (this->serverGetString == NULL)
        {
          this->serverGetString = _T("/");
        }

        // create request string
        if ((this->serverPort != 80) && (this->serverPort != 443))
        {
          tempBuffer = FormatString(HTTP_NON_STANDARD_REQUEST_FORMAT, this->serverGetString, this->server, this->serverPort);
        }
        else
        {
          tempBuffer = FormatString(HTTP_STANDARD_REQUEST_FORMAT, this->serverGetString, this->server);
        }

        this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: request length: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _tcslen(tempBuffer));
        this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: request: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, tempBuffer);

        // send request
        connected &= (SendString(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->m_socket, tempBuffer, 0, receiveDataTimeout) == 0);

        FREE_MEM(tempBuffer);
      }

      if (connected)
      {
        // now should be connection successfuly opened and receiving data
        break;
      }
      else
      {
        // error occured, close connection
        this->CloseConnection();
      }
    }

    delete parameters;

    if (server != NULL)
    {
      FreeAddrInfo(server);
    }

    result = (connected) ? STATUS_OK : STATUS_ERROR;
  }

  if (result != STATUS_OK)
  {
    this->CloseConnection();
  }

  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  return result;
}

int CMPIPTV_HTTP::IsConnected(void)
{
  return (this->m_socket != NULL);
}

void CMPIPTV_HTTP::CloseConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);

  if (this->m_socket != NULL)
  {
    closesocket(this->m_socket);
    WSACleanup();
  }

  this->m_socket = NULL;
  this->chunkedEncoding = false;
  this->receivedHttpResponse = false;
  this->chunkedBuffer.DeleteBuffer();

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);
}

void CMPIPTV_HTTP::GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize)
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

// gets the line with '\n' and the terminating null character
TCHAR* GetLine(char *source)
{
  char* end = strchr(source, '\n');
  size_t length = (end == NULL) ? (strlen(source) + 1) : (((long)end - (long)source) + 2);
  ALLOC_MEM_DEFINE_SET(temp, char, length, 0);
  TCHAR *result = NULL;

  if (temp != NULL)
  {
    if (length > 1)
    {
      strncpy_s(temp, length, source, length - 1);
    }

#ifdef _MBCS
    result = ConvertToMultiByteA(temp);
#else
    result = ConvertToUnicodeA(temp);
#endif
  }

  FREE_MEM(temp);

  return result;
}

// gets chunk data from buffer
// data are returned to data variable
// length of data returned to length variable
// the whole chunk length returned to chunkLength variable
// method returns TRUE if chunk correctly get, FALSE otherwise
// 
// necessary space needed for data is created in this method
// this space after using have to be removed
bool GetChunkData(LinearBuffer *buffer, char **data, unsigned int *length, unsigned int *chunkLength)
{
  bool result = false;
  unsigned int firstEndLinePosition = buffer->GetFirstPosition(0, '\n');
  if (firstEndLinePosition != UINT_MAX)
  {
    // there is some chance for chunk data header
    unsigned int chunkHeaderDataLength = firstEndLinePosition + 2;
    // chunk hedaer data with terminating null character
    ALLOC_MEM_DEFINE_SET(chunkHeaderData, char, chunkHeaderDataLength, 0);

    if (chunkHeaderData != NULL)
    {
      // copy header data from buffer
      buffer->CopyFromBuffer(chunkHeaderData, chunkHeaderDataLength - 1, 0, 0);
      char *chunkHeaderEndPart = NULL;

      *length = strtol(chunkHeaderData, &chunkHeaderEndPart, 16);

      //if ((*length > 0) && (strlen(chunkHeaderEndPart) == 2))
      if (*length > 0)
      {
        // there valid length of chunk data and header is ended correctly with CRLF
        // the last characters are always CRLF

        // (chunkHeaderDataLength - 1) is the length of chunk data with terminating CRLF characters
        // the chunk data must be ended with CRLF
        // this size of chunk data is lenght + 2 (CRLF)
        // there is no need to termination NULL character, because in data can be NULL characters
        unsigned int chunkDataLength = (*length) + 2;

        // now is known whole chunk length
        *chunkLength = chunkHeaderDataLength - 1 + chunkDataLength;

        if (buffer->GetBufferOccupiedSpace() >= (*chunkLength))
        {
          // there is enough data to process whole chunk
          // allocate memory for chunk data
          *data = ALLOC_MEM(char, *length);
          if ((*data) != NULL)
          {
            buffer->CopyFromBuffer((*data), (*length), 0, chunkHeaderDataLength - 1);
            result = TRUE;
          }
        }
      }
      else if (*length == 0)
      {
        // there is valid length
        // zero length is signal for end of transmission
        result = TRUE;
      }
    }

    FREE_MEM(chunkHeaderData);
  }

  return result;
}

void CMPIPTV_HTTP::ReceiveData(bool *shouldExit)
{
  if (this->IsConnected())
  {
    unsigned int freeSpace = 0;
    unsigned int occupiedSpace = 0;
    unsigned int bufferSize = 0;

    this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);

    if (freeSpace >= 0)
    {
      // there is some place in buffer
      // and probably (!) is there sufficient place for received data
      // probably = received data length may variate

      int length = GetDataFromSocket(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->m_socket, this->receiveBuffer, this->defaultBufferSize);

      if (length == SOCKET_ERROR)
      {
        // socket error occured => close connection
        this->CloseConnection();
      }
      else if (length == 0)
      {
        // no data received
        // sleep some time
      }
      else
      {
        if (this->dumpInputPackets)
        {
          DumpInputPacket(this->GetInstanceId(), length, this->receiveBuffer);
        }

        // parse HTTP response
        // set end of string
        this->receiveBuffer[length + 1] = '\0';

        int httpReturnCode = 0;

        // set start to start of buffer
        char *start = this->receiveBuffer;

        // do not try to parse data if we received HTTP response earlier
        if (!(this->receivedHttpResponse))
        {
          if(!strncmp(this->receiveBuffer, "HTTP/1.", 7))
          {
            this->receivedHttpResponse = TRUE;
            httpReturnCode = atoi(&this->receiveBuffer[9]);
            this->logger.Log(LOGGER_INFO, _T("%s: %s: HTTP return code: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, httpReturnCode);

            TCHAR *line = NULL;
            int lineLength = 0;
            do
            {
              line = GetLine(start);
              lineLength = _tcslen(line);

              this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: response header line (length: %i): %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, lineLength, line);

              if ((httpReturnCode >= 300) && (httpReturnCode < 400))
              {
                // redirect
                if (_tcsstr(line, _T("Location: ")) != NULL)
                {
                  this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, _T("redirection detected"));
                  this->receivedHttpResponse = FALSE;

                  TCHAR *newUrl = NULL;
                  TCHAR *endOfLine = _tcschr(line, _T('\r'));
                  if (endOfLine != NULL)
                  {
                    int length = _tcslen(line) - _tcslen(endOfLine) - 9;
                    newUrl = ALLOC_MEM_SET(newUrl, TCHAR, length, 0);
                    if (newUrl != NULL)
                    {
                      this->logger.Log(LOGGER_INFO, _T("%s: %s: length: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, length);
                      if (length > 1)
                      {
                        _tcsncpy_s(newUrl, length, line + 10, length - 1);
                        this->logger.Log(LOGGER_INFO, _T("%s: %s: new url: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, newUrl);
                      }
                    }
                  }

                  // ClearSession() clears also loadParameters
                  CParameterCollection *parameters = new CParameterCollection();
                  parameters->Append(this->loadParameters);

                  // clear this session
                  this->ClearSession();

                  if (newUrl != NULL)
                  {
                    if (this->ParseUrl(newUrl, parameters) == STATUS_OK)
                    {
                      this->OpenConnection();
                    }
                  }

                  FREE_MEM(newUrl);
                  FREE_MEM(line);
                  delete parameters;
                  return;
                }
              }

              if (httpReturnCode == 200)
              {
                // OK
                if (_tcsstr(line, _T("Transfer-Encoding: chunked")) != NULL)
                {
                  this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, _T("chunked encoding"));

                  // chunked encoding :(
                  // we must initialize chunked buffer
                  this->chunkedEncoding = TRUE;
                  this->chunkedBuffer.DeleteBuffer();
                  this->chunkedBuffer.InitializeBuffer(this->defaultBufferSize);
                }
              }

              start += lineLength;
              FREE_MEM(line);
            } while (lineLength > 2);
          }
        }

        // if start moves then length must be shorter
        length -= ((long)start - (long)this->receiveBuffer);

        if (httpReturnCode >= 400)
        {
          this->logger.Log(LOGGER_ERROR, _T("%s: %s: server returned error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, httpReturnCode);

          // server returned error
          // close connection
          // new connection will try to open in next run
          this->CloseConnection();
        }
        else if (length > 0)
        {
          this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);

          while (((unsigned int)length > freeSpace) && (!(*shouldExit)))
          {
            // if length of data is greater than free space in buffer
            // must wait for free space
            this->logger.Log(LOGGER_WARNING, _T("%s: %s: data received, buffer free space too small, buffer size: %u, free buffer size: %u, occupied buffer size: %u, data length: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace, length);
            if (!this->buffer.SafeResizeBuffer(this->lockMutex, min(bufferSize * 2, this->maxBufferSize), true, &this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME))
            {
              // buffer cannot be resized
              // just wait
              Sleep(10);
              this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
            }
          }

          if (!(*shouldExit))
          {
            if (this->chunkedEncoding)
            {
              this->chunkedBuffer.AddToBuffer(start, length);

              char *chunkData = NULL;
              unsigned int chunkDataLength = 0;
              unsigned int chunkLength = 0;
              while (GetChunkData(&this->chunkedBuffer, &chunkData, &chunkDataLength, &chunkLength))
              {
                // valid chunk data returned
                this->logger.Log(LOGGER_DATA, _T("%s: %s: chunk data length: %u, chunk length: %u, chunk data: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, chunkDataLength, chunkLength, chunkData);

                if (chunkDataLength > 0)
                {
                  // remove from chunk buffer
                  WaitForSingleObject(this->lockMutex, INFINITE);
                  this->chunkedBuffer.RemoveFromBufferAndMove(chunkLength);
                  this->buffer.AddToBuffer(chunkData, chunkDataLength);
                  this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
                  ReleaseMutex(this->lockMutex);

                  FREE_MEM(chunkData);
                }
                else
                {
                  this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, _T("end of transmission, closing connection"));

                  // transmission ended, close connection
                  // connection be reopened in next run
                  this->CloseConnection();
                }
              }
            }
            else
            {
              // copy everything from start
              WaitForSingleObject(this->lockMutex, INFINITE);
              this->buffer.AddToBuffer(start, length);
              this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
              ReleaseMutex(this->lockMutex);
            }
          }
          this->logger.Log(LOGGER_DATA, _T("%s: %s: buffer size: %u, free buffer size: %u, occupied buffer size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace);
        }
      }
    }
    else
    {
      // buffer free space to low
      this->logger.Log(LOGGER_WARNING, _T("%s: %s: no data received, buffer free space too small, buffer size: %u, free buffer size: %u, occupied buffer size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace);
      this->buffer.SafeResizeBuffer(this->lockMutex, min(bufferSize * 2, this->maxBufferSize), true, &this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME);
    }
  }
  else
  {
    this->logger.Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, _T("socket connection not opened, opening new one"));
    // re-open connection if previous is lost
    if (this->OpenConnection() != STATUS_OK)
    {
      this->CloseConnection();
    }
  }
}

unsigned int CMPIPTV_HTTP::FillBuffer(IMediaSample *pSamp, char *pData, long cbData)
{
  WaitForSingleObject(lockMutex, INFINITE);

  unsigned int result = 0;
  unsigned int length = this->buffer.GetBufferOccupiedSpace();

  unsigned int start = 0;

  if (length >= DVB_PACKET_SIZE)
  {
    // find start of MPEG TS packet and check few other packets if they are aligned

    ALLOC_MEM_DEFINE_SET(temp, char, length, 0);
    if (temp != NULL)
    {
      this->buffer.CopyFromBuffer(temp, length, 0, 0);

      while (start < length)
      {
        unsigned int tempStart = start;
        while (((tempStart + DVB_PACKET_SIZE) < length) && (temp[tempStart] == SYNC_BYTE))
        {
          tempStart += DVB_PACKET_SIZE;
        }

        if ((length - tempStart) <= DVB_PACKET_SIZE)
        {
          // if difference is less or equal to DVB_PACKET_SIZE (188 bytes)
          // then we reached end of buffer and all packets were starting with SYNC_BYTE (0x47 byte)
          break;
        }

        start++;
      }

      this->buffer.RemoveFromBuffer(start);
    }
    FREE_MEM(temp);

    result = FillBufferStandard(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_NAME, this->lockMutex, &this->buffer, pSamp, pData, cbData);
  }

  ReleaseMutex(lockMutex);

  return result;
}

unsigned int CMPIPTV_HTTP::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

LinearBuffer *CMPIPTV_HTTP::GetBuffer(void)
{
  return &this->buffer;
}

LinearBuffer *CMPIPTV_HTTP::GetChunkedBuffer(void)
{
  return &this->chunkedBuffer;
}

GUID CMPIPTV_HTTP::GetInstanceId(void)
{
  return this->logger.loggerInstance;
}

unsigned int CMPIPTV_HTTP::GetOpenConnectionMaximumAttempts(void)
{
  return this->openConnetionMaximumAttempts;
}
