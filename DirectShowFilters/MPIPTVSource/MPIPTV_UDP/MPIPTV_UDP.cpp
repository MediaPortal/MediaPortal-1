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

#include "MPIPTV_UDP.h"
#include "Network.h"
#include "Utilities.h"

#include <WinInet.h>
#include <stdio.h>

// protocol implementation name
#define PROTOCOL_IMPLEMENTATION_NAME                                    _T("CMPIPTV_UDP")

PIProtocol CreateProtocolInstance(void)
{
  return new CMPIPTV_UDP;
}

void DestroyProtocolInstance(PIProtocol pProtocol)
{
  if (pProtocol != NULL)
  {
    CMPIPTV_UDP *pClass = (CMPIPTV_UDP *)pProtocol;
    delete pClass;
  }
}

CMPIPTV_UDP::CMPIPTV_UDP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);

  this->packetSize = 0;
  this->lockMutex = NULL;

  this->buffer.DeleteBuffer();
  this->receiveBuffer = NULL;

  this->m_socket = NULL;
  this->localAddress = NULL;
  this->localPort = 0;
  this->sourceAddress = NULL;
  this->sourcePort = 0;
  this->configurationParameters = new CParameterCollection();
  this->loadParameters = new CParameterCollection();
  this->receiveDataTimeout = 0;
  this->openConnetionMaximumAttempts = UDP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT;

  this->defaultBufferSize = 0;
  this->maxBufferSize = 0;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CONSTRUCTOR_NAME);
}

CMPIPTV_UDP::~CMPIPTV_UDP()
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }

  FREE_MEM(this->localAddress);
  FREE_MEM(this->sourceAddress);
  FREE_MEM(this->receiveBuffer);
  this->buffer.DeleteBuffer();
  delete this->configurationParameters;
  delete this->loadParameters;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_DESTRUCTOR_NAME);
}

int CMPIPTV_UDP::Initialize(HANDLE lockMutex, CParameterCollection *configuration)
{
  this->configurationParameters->Clear();
  if (configuration != NULL)
  {
    this->configurationParameters->Append(configuration);
  }
  this->configurationParameters->LogCollection(&this->logger, LOGGER_VERBOSE, PROTOCOL_IMPLEMENTATION_NAME, METHOD_INITIALIZE_NAME);

  long iptvBufferSize = this->configurationParameters->GetValueLong(CONFIGURATION_IPTV_BUFFER_SIZE, true, IPTV_BUFFER_SIZE_DEFAULT);
  long defaultMultiplier = this->configurationParameters->GetValueLong(CONFIGURATION_UDP_INTERNAL_BUFFER_MULTIPLIER, true, UDP_INTERNAL_BUFFER_MULTIPLIER_DEFAULT);
  long maxMultiplier = this->configurationParameters->GetValueLong(CONFIGURATION_UDP_INTERNAL_BUFFER_MAX_MULTIPLIER, true, UDP_INTERNAL_BUFFER_MAX_MULTIPLIER_DEFAULT);
  this->receiveDataTimeout = this->configurationParameters->GetValueLong(CONFIGURATION_UDP_RECEIVE_DATA_TIMEOUT, true, UDP_RECEIVE_DATA_TIMEOUT_DEFAULT);
  this->openConnetionMaximumAttempts = this->configurationParameters->GetValueLong(CONFIGURATION_UDP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS, true, UDP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT);
  this->dumpInputPackets = this->configurationParameters->GetValueBool(CONFIGURATION_DUMP_INPUT_PACKETS, true, DUMP_INPUT_PACKETS_DEFAULT);

  iptvBufferSize = (iptvBufferSize <= 0) ? IPTV_BUFFER_SIZE_DEFAULT : iptvBufferSize;
  defaultMultiplier = (defaultMultiplier <= 0) ? UDP_INTERNAL_BUFFER_MULTIPLIER_DEFAULT : defaultMultiplier;
  maxMultiplier = (maxMultiplier < defaultMultiplier) ? defaultMultiplier : maxMultiplier;
  this->receiveDataTimeout = (this->receiveDataTimeout < 0) ? UDP_RECEIVE_DATA_TIMEOUT_DEFAULT : this->receiveDataTimeout;
  this->openConnetionMaximumAttempts = (this->openConnetionMaximumAttempts < 0) ? UDP_OPEN_CONNECTION_MAXIMUM_ATTEMPTS_DEFAULT : this->openConnetionMaximumAttempts;

  this->defaultBufferSize = defaultMultiplier * iptvBufferSize;
  this->maxBufferSize = maxMultiplier * iptvBufferSize;

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

TCHAR *CMPIPTV_UDP::GetProtocolName(void)
{
  return Duplicate(CONFIGURATION_SECTION_UDP);
}

int CMPIPTV_UDP::ClearSession(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  if (this->IsConnected())
  {
    this->CloseConnection();
  }
  this->loadParameters->Clear();

  FREE_MEM(this->localAddress);
  FREE_MEM(this->sourceAddress);

  this->buffer.DeleteBuffer();
  this->buffer.InitializeBuffer(this->defaultBufferSize);
  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLEAR_SESSION_NAME);

  return STATUS_OK;
}

int CMPIPTV_UDP::ParseUrl(const TCHAR *url, const CParameterCollection *parameters)
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

      if (_tcsncicmp(urlComponents->lpszScheme, _T("UDP"), urlComponents->dwSchemeLength) != 0)
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
      this->localAddress = ALLOC_MEM_SET(this->localAddress, TCHAR, length, 0);
      if (this->localAddress == NULL) 
      {
        this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'localAddress'"));
        result = STATUS_ERROR;
      }
    }

    if (result == STATUS_OK)
    {
      _tcsncat_s(this->localAddress, length, urlComponents->lpszHostName, urlComponents->dwHostNameLength);

      if (urlComponents->dwUserNameLength > 0) 
      {
        length = urlComponents->dwUserNameLength + 1;
        this->sourceAddress = ALLOC_MEM_SET(this->sourceAddress, TCHAR, length, 0);
        if (this->sourceAddress == NULL) 
        {
          this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, _T("cannot allocate memory for 'sourceAddress'"));
          result = STATUS_ERROR;
        }
        
        if (result == STATUS_OK)
        {
          _tcsncat_s(this->sourceAddress, length, urlComponents->lpszUserName, urlComponents->dwUserNameLength);
        }
      }
      if (urlComponents->dwPasswordLength > 0)
      {
        // its port for remote address
        length = urlComponents->dwPasswordLength + 1;
        this->sourcePort = _tstoi(urlComponents->lpszPassword);
      }

      this->localPort = urlComponents->nPort;

      if (result == STATUS_OK)
      {
        this->logger.Log(LOGGER_INFO, _T("%s: %s: local address: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->localAddress);
        this->logger.Log(LOGGER_INFO, _T("%s: %s: local port: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->localPort);
        this->logger.Log(LOGGER_INFO, _T("%s: %s: source address: %s"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->sourceAddress);
        this->logger.Log(LOGGER_INFO, _T("%s: %s: source port: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME, this->sourcePort);
      }
    }
  }
  FREE_MEM(urlComponents);
  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_PARSE_URL_NAME);
  return result;
}

int CMPIPTV_UDP::OpenConnection(void)
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
    hints.ai_socktype = SOCK_DGRAM;
    hints.ai_protocol = IPPROTO_UDP;

    // chains of ADDRINFO structures for remote and local server
    local = NULL;
    source = NULL;
    ADDRINFOT *networkInterface = NULL;

    // variables for cycles
    currentLocalAddr = NULL;
    currentSourceAddr = NULL;

    ADDRINFOT *localAddr = NULL;
    ADDRINFOT *sourceAddr = NULL;

    if (this->sourceAddress)
    {
      // in url was specified remote address
      int errorCode = GetIpAddress(this->sourceAddress, this->sourcePort, &source, &hints);
      if (errorCode != 0)
      {
        this->logger.Log(LOGGER_ERROR, _T("%s: %s: getaddrinfo() for source server error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, errorCode);
        result = STATUS_ERROR;
      }
    }

    if (result == STATUS_OK)
    {
      // set passive flag and resolve local address
      hints.ai_flags = AI_PASSIVE;

      int errorCode = GetIpAddress(this->localAddress, this->localPort, &local, &hints);
      if (errorCode != 0)
      {
        this->logger.Log(LOGGER_ERROR, _T("%s: %s: getaddrinfo() for local server error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, errorCode);
        result = STATUS_ERROR;
      }
    }

    if (result == STATUS_OK)
    {
      CParameterCollection *parameters = new CParameterCollection();
      parameters->Append(this->configurationParameters);
      parameters->Append(this->loadParameters);

      bool connected = false;
      for (localAddr = local; localAddr != NULL; localAddr = localAddr->ai_next)
      {
        // set connected to TRUE
        connected = true;

        // for each local address until we find working address

        if (parameters != NULL)
        {
          // we have set some parameters
          // get interface parameter
          PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
          if (networkInterfaceParameter != NULL)
          {
            TCHAR *networkInterfaceAddress = GetInterfaceAddress(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, networkInterfaceParameter->GetValue(), localAddr->ai_family);
            int errorCode = GetIpAddress(networkInterfaceAddress, 0, &networkInterface, &hints);
            if (errorCode != 0)
            {
              this->logger.Log(LOGGER_ERROR, _T("%s: %s: getaddrinfo() for network interface error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, errorCode);
              connected = FALSE;
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
          this->m_socket = CreateSocket(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, localAddr->ai_family, SOCK_DGRAM, 0);
          if (this->m_socket == INVALID_SOCKET)
          {
            this->logger.Log(LOGGER_ERROR, _T("%s: %s: socket() error: %i"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, WSAGetLastError());
            connected = FALSE;
          }
        }

        // setup socket
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
            if (connected)
            {
              this->logger.Log(LOGGER_VERBOSE, _T("%s: %s: receive buffer set to %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->defaultBufferSize);
            }
          }

          if (connected)
          {
            // set socket to non-blocking mode
            connected &= (SetBlockingMode(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->m_socket, false) == 0);
            if (connected)
            {
              this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("socket set to non-blocking"));
            }
          }
        } 

        if (connected)
        {
          // socket is configured, now bind
          if (SockAddrIsMulticast(localAddr) && (sizeof(struct sockaddr_storage) >= localAddr->ai_addrlen))
          {
            struct sockaddr_in6 dumb;
            memset(&dumb, 0, sizeof(sockaddr_in6));

            int dumbLength = 0;
            switch(localAddr->ai_family)
            {
            case AF_INET:
              dumbLength = sizeof(sockaddr_in);
              ((struct sockaddr_in*)&dumb)->sin_family = localAddr->ai_addr->sa_family;
              ((struct sockaddr_in*)&dumb)->sin_port =  ((struct sockaddr_in *)(localAddr->ai_addr))->sin_port;
              if (networkInterface != NULL)
              {
                ((struct sockaddr_in*)&dumb)->sin_addr = ((struct sockaddr_in *)(networkInterface->ai_addr))->sin_addr;
              }
              break;
            case AF_INET6:
              dumbLength = sizeof(sockaddr_in6);
              dumb.sin6_family = localAddr->ai_addr->sa_family;
              dumb.sin6_port =  ((struct sockaddr_in6 *)(localAddr->ai_addr))->sin6_port;
              break;
            }

            connected &= (Bind(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->m_socket, (struct sockaddr *)&dumb, dumbLength) == 0);
          }
          else
          {
            connected &= (Bind(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, this->m_socket, localAddr->ai_addr, localAddr->ai_addrlen) == 0);
          }

          if (connected)
          {
            this->logger.Log(LOGGER_VERBOSE, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("binded to local address"));
          }
        }

        if (connected)
        {
          if (SockAddrIsMulticast(localAddr))
          {
            this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME, _T("multicast"));

            // if local address is multicast address there is need to join to multicast group
            // method of joining to multicast group depends on source address

            if (this->sourceAddress)
            {
              // we are working with remote server
              for (sourceAddr = source; sourceAddr != NULL; sourceAddr = sourceAddr->ai_next)
              {
                if ((sourceAddr->ai_family != localAddr->ai_family)
                  || (sourceAddr->ai_socktype != localAddr->ai_socktype)
                  || (sourceAddr->ai_protocol != localAddr->ai_protocol))
                {
                  // remote and local server have to be same in family, socket type and protocol
                  connected = false;
                  continue;
                }

                // SubscribeToMulticastGroup returns 0 if success
                if (SubscribeToMulticastGroup(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, this->m_socket, sourceAddr->ai_addr, sourceAddr->ai_addrlen, localAddr->ai_addr, localAddr->ai_addrlen, parameters))
                {
                  // join to multicast group failed
                  connected = false;
                  continue;
                }
                else
                {
                  // successfully joined to multicast group
                  connected = true;
                  break;
                }
              }
            }
            else
            {
              // we are working on local server
              // SubscribeToMulticastGroup returns 0 if success
              if (SubscribeToMulticastGroup(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, this->m_socket, NULL, 0, localAddr->ai_addr, localAddr->ai_addrlen, parameters))
              {
                // join to multicast group failed
                connected = false;
              }
            }
          }
        }

        // free used resources
        if (networkInterface != NULL)
        {
          FreeAddrInfo(networkInterface);
          networkInterface = NULL;
        }

        if (connected)
        {
          // successfully connected, break cycle
          break;
        }
        else
        {
          // close connection and try another local address
          this->CloseConnection();
        }
      }

      delete parameters;

      if (!connected)
      {
        if (local != NULL)
        {
          FreeAddrInfo(local);
        }
        local = NULL;
        if (source != NULL)
        {
          FreeAddrInfo(source);
        }
        source = NULL;
        this->CloseConnection();
        result = STATUS_ERROR;
      }
      else
      {
        // leave local and source
        // when closing connection, these structures will be removed
        currentLocalAddr = localAddr;
        currentSourceAddr = sourceAddr;
      }
    }
  }
  this->logger.Log(LOGGER_INFO, (result == STATUS_OK) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_OPEN_CONNECTION_NAME);
  return result;
}

int CMPIPTV_UDP::IsConnected(void)
{
  return (this->m_socket != NULL);
}

void CMPIPTV_UDP::CloseConnection(void)
{
  this->logger.Log(LOGGER_INFO, METHOD_START_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);

  if (this->currentLocalAddr != NULL)
  {
    // local address is set
    // if local address is multicast, then remove from multicast group
    if (SockAddrIsMulticast(this->currentLocalAddr))
    {
      CParameterCollection *parameters = new CParameterCollection();
      parameters->Append(this->configurationParameters);
      parameters->Append(this->loadParameters);

      this->logger.Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("unsubscribe from multicast group"));
      if (UnsubscribeFromMulticastGroup(
        &this->logger,
        PROTOCOL_IMPLEMENTATION_NAME,
        this->m_socket,
        (this->currentSourceAddr == NULL) ? NULL : this->currentSourceAddr->ai_addr,
        (this->currentSourceAddr == NULL) ? 0 : this->currentSourceAddr->ai_addrlen,
        this->currentLocalAddr->ai_addr,
        this->currentLocalAddr->ai_addrlen,
        parameters))
      {
        this->logger.Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME, _T("unsubscribe from multicast group failed"));
      }

      delete parameters;
    }

    if (this->local != NULL)
    {
      FreeAddrInfo(local);
    }
    if (this->source != NULL)
    {
      FreeAddrInfo(source);
    }

    this->local = NULL;
    this->source = NULL;
    this->currentLocalAddr = NULL;
    this->currentSourceAddr = NULL;
  }

  if (this->m_socket != NULL)
  {
    closesocket(this->m_socket);
    WSACleanup();
  }

  this->m_socket = NULL;

  this->logger.Log(LOGGER_INFO, METHOD_END_FORMAT, PROTOCOL_IMPLEMENTATION_NAME, METHOD_CLOSE_CONNECTION_NAME);
}

void CMPIPTV_UDP::GetSafeBufferSizes(HANDLE lockMutex, unsigned int *freeSpace, unsigned int *occupiedSpace, unsigned int *bufferSize)
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

void CMPIPTV_UDP::ReceiveData(bool *shouldExit)
{
  if (this->IsConnected())
  {
    unsigned int freeSpace = 0;
    unsigned int occupiedSpace = 0;
    unsigned int bufferSize = 0;

    this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
    this->logger.Log(LOGGER_DATA, _T("%s: %s: start, buffer size: %u, free buffer size: %u, occupied buffer size: %u, packet size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace, this->packetSize);

    if (freeSpace >= this->packetSize)
    {
      // there is some place in buffer
      // and probably (!) is there sufficient place for received data
      // probably = received data length may variate
      // for unreliable protocols (e.g. UDP) are data lost if received data are longer than buffer

      int length = 0;
      unsigned int written = 0;

      WaitForSingleObject(this->lockMutex, INFINITE);
      do
      {
        length = GetDataFromSocket(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, this->m_socket, this->receiveBuffer, freeSpace);

        if (length > 0)
        {
          if (this->dumpInputPackets)
          {
            DumpInputPacket(this->GetInstanceId(), length, this->receiveBuffer);
          }

          this->buffer.AddToBuffer(this->receiveBuffer, (unsigned int)length);
          written += (unsigned int)length;
          freeSpace -= (unsigned int)length;

          if ((unsigned int)length > this->packetSize)
          {
            // set the maximum packet size (received until now)
            this->packetSize = length;
          }
        }
      }
      while ((length > 0) && (freeSpace >= packetSize) && (!(*shouldExit)));
      this->GetSafeBufferSizes(this->lockMutex, &freeSpace, &occupiedSpace, &bufferSize);
      ReleaseMutex(this->lockMutex);

      if (length == SOCKET_ERROR)
      {
        // socket error occured => close connection
        this->CloseConnection();
      }
      else if (written == 0)
      {
        // no data received
        // sleep some time
      }
      else
      {
        // all data already written
        this->logger.Log(LOGGER_DATA, _T("%s: %s: end, buffer size: %u, free buffer size: %u, occupied buffer size: %u, packet size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace, this->packetSize);
      }
    }
    else
    {
      // buffer free space to low
      this->logger.Log(LOGGER_WARNING, _T("%s: %s: no data received, buffer free space too small, buffer size: %u, free buffer size: %u, occupied buffer size: %u, packet size: %u"), PROTOCOL_IMPLEMENTATION_NAME, METHOD_RECEIVE_DATA_NAME, bufferSize, freeSpace, occupiedSpace, this->packetSize);
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

unsigned int CMPIPTV_UDP::FillBuffer(IMediaSample *pSamp, char *pData, long cbData)
{
  return FillBufferStandard(&this->logger, PROTOCOL_IMPLEMENTATION_NAME, METHOD_FILL_BUFFER_NAME, this->lockMutex, &this->buffer, pSamp, pData, cbData);
 }

unsigned int CMPIPTV_UDP::GetReceiveDataTimeout(void)
{
  return this->receiveDataTimeout;
}

GUID CMPIPTV_UDP::GetInstanceId(void)
{
  return this->logger.loggerInstance;
}

unsigned int CMPIPTV_UDP::GetOpenConnectionMaximumAttempts(void)
{
  return this->openConnetionMaximumAttempts;
}
