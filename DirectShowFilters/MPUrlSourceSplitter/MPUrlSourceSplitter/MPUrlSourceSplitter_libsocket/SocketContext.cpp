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

#include "StdAfx.h"

#include "SocketContext.h"

CSocketContext::CSocketContext(void)
{
  this->internalSocket = INVALID_SOCKET;
  this->wsaInitialized = false;

  WORD versionRequested;
  WSADATA wsaData;

  /* Use the MAKEWORD(lowbyte, highbyte) macro declared in Windef.h */
  versionRequested = MAKEWORD(2, 2);

  this->wsaInitialized = (SUCCEEDED(HRESULT_FROM_WIN32(WSAStartup(versionRequested, &wsaData))));
  this->ipAddress = NULL;
  this->receivedDataLength = 0;
  this->sentDataLength = 0;
}

CSocketContext::CSocketContext(SOCKET socket)
{
  this->internalSocket = socket;
  this->wsaInitialized = false;

  this->ipAddress = NULL;
  this->receivedDataLength = 0;
  this->sentDataLength = 0;
}

CSocketContext::~CSocketContext(void)
{
  this->CloseSocket();
  FREE_MEM_CLASS(this->ipAddress);

  if (this->wsaInitialized)
  {
    WSACleanup();
  }
}

/* get methods */

CIpAddress *CSocketContext::GetIpAddress(void)
{
  return this->ipAddress;
}

HRESULT CSocketContext::GetOption(int level, int optionName, char *optionValue, int *optionLength)
{
  HRESULT result = S_OK;

  if (getsockopt(this->internalSocket, level, optionName, optionValue, optionLength) == SOCKET_ERROR)
  {
    result = HRESULT_FROM_WIN32(WSAGetLastError());
  }

  return result;
}

HRESULT CSocketContext::GetPendingIncomingDataLength(unsigned int *incomingDataLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, incomingDataLength);

  if (SUCCEEDED(result))
  {
    unsigned int state = SOCKET_STATE_UNDEFINED;
    result = this->Select(true, false, 0, &state);

    CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (state == SOCKET_STATE_READABLE) ? result : E_NOT_VALID_STATE);

    if (FAILED(result))
    {
      result = (result == HRESULT_FROM_WIN32(WSAETIMEDOUT)) ? S_OK : result;
      CHECK_CONDITION_EXECUTE(SUCCEEDED(result), (*incomingDataLength) = 0);
    }
    else
    {
      // if socket is closed, then readable state is signaled
      // Receive() method returns 0 received bytes when connection is closed
      ALLOC_MEM_DEFINE_SET(buffer, char, BUFFER_REQUEST_SIZE_DEFAULT, 0);
      CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        result = this->Receive(buffer, BUFFER_REQUEST_SIZE_DEFAULT, MSG_PEEK, incomingDataLength);
      }

      FREE_MEM(buffer);
    }
  }

  return result;
}

uint64_t CSocketContext::GetReceivedDataLength(void)
{
  return this->receivedDataLength;
}

uint64_t CSocketContext::GetSentDataLength(void)
{
  return this->sentDataLength;
}

/* set methods */

HRESULT CSocketContext::SetIpAddress(CIpAddress *address)
{
  HRESULT result = S_OK;
  FREE_MEM_CLASS(this->ipAddress);

  if (address != NULL)
  {
    this->ipAddress = address->Clone();
    CHECK_POINTER_HRESULT(result, this->ipAddress, result, E_OUTOFMEMORY);
  }
  
  return result;
}

HRESULT CSocketContext::SetOption(int level, int optionName, const char *optionValue, int optionLength)
{
  HRESULT result = S_OK;

  if (setsockopt(this->internalSocket, level, optionName, optionValue, optionLength) == SOCKET_ERROR)
  {
    result = HRESULT_FROM_WIN32(WSAGetLastError());
  }

  return result;
}

HRESULT CSocketContext::SetBlockingMode(bool blocking)
{
  HRESULT result = S_OK;
  unsigned long nonblocking = (blocking == 0);

  if (ioctlsocket(this->internalSocket, FIONBIO, &nonblocking) == SOCKET_ERROR) 
  {
    result = HRESULT_FROM_WIN32(WSAGetLastError());
  }

  return result;
}

/* other methods */

HRESULT CSocketContext::CreateSocket(void)
{
  HRESULT result = (this->GetIpAddress() != NULL) ? S_OK : E_POINTER;
  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->CloseSocket(), result);

  if (SUCCEEDED(result))
  {
    this->internalSocket = socket(this->GetIpAddress()->GetFamily(), this->GetIpAddress()->GetSockType(), this->GetIpAddress()->GetProtocol());

    if (this->internalSocket == INVALID_SOCKET)
    {
      result = HRESULT_FROM_WIN32(WSAGetLastError());
    }
    else
    {
      // set socket buffer size
      DWORD dw = BUFFER_LENGTH_DEFAULT;
      int dwLen = sizeof(dw);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(SOL_SOCKET, SO_RCVBUF, (const char*)&dw, dwLen), result);
      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(SOL_SOCKET, SO_SNDBUF, (const char*)&dw, dwLen), result);
    }
  }

  return result;
}

HRESULT CSocketContext::CreateSocket(CIpAddress *address)
{
  HRESULT result = ((this->GetIpAddress() != NULL) || (this->GetIpAddress() == NULL) && (address != NULL)) ? S_OK : E_INVALIDARG;
  
  if (SUCCEEDED(result) && (address != NULL))
  {
    result = this->SetIpAddress(address);
    CHECK_POINTER_HRESULT(result, this->GetIpAddress(), result, E_OUTOFMEMORY);
  }

  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->CreateSocket(), result);

  return result;
}

HRESULT CSocketContext::CloseSocket(void)
{
  HRESULT result = S_OK;

  if (this->internalSocket != INVALID_SOCKET)
  {
    int err = closesocket(this->internalSocket);
    this->internalSocket = INVALID_SOCKET;
  }

  return result;
}

HRESULT CSocketContext::IsClosed(void)
{
  unsigned int state = SOCKET_STATE_UNDEFINED;
  HRESULT result = this->Select(true, false, 0, &state);
  result = (result == HRESULT_FROM_WIN32(WSAETIMEDOUT)) ? S_FALSE : result;

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (state == SOCKET_STATE_READABLE) ? result : E_NOT_VALID_STATE);

  if (SUCCEEDED(result))
  {
    // if socket is closed, then readable state is signaled
    // Receive() method returns 0 received bytes when connection is closed
    ALLOC_MEM_DEFINE_SET(buffer, char, BUFFER_REQUEST_SIZE_DEFAULT, 0);
    CHECK_POINTER_HRESULT(result, buffer, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      unsigned int receivedLength = 0;
      result = this->Receive(buffer, BUFFER_REQUEST_SIZE_DEFAULT, MSG_PEEK, &receivedLength);

      if (SUCCEEDED(result))
      {
        result = (receivedLength == 0) ? S_OK : S_FALSE;
      }
    }

    FREE_MEM(buffer);
  }

  return result;
}

HRESULT CSocketContext::Bind(void)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, this->GetIpAddress(), result, E_POINTER);

  if (SUCCEEDED(result))
  {
    if (bind(this->internalSocket, this->GetIpAddress()->GetAddressIP(), this->GetIpAddress()->GetAddressLength()) == SOCKET_ERROR)
    {
      result = HRESULT_FROM_WIN32(WSAGetLastError());
    }
  }

  return result;
}

HRESULT CSocketContext::Send(const char *buffer, unsigned int length, unsigned int *sentLength)
{
  return this->Send(buffer, length, 0, sentLength);
}

HRESULT CSocketContext::Send(const char *buffer, unsigned int length, int flags, unsigned int *sentLength)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, buffer, result, E_INVALIDARG);
  CHECK_POINTER_HRESULT(result, sentLength, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    *sentLength = send(this->internalSocket, buffer, length, flags);
    if ((*sentLength) == SOCKET_ERROR)
    {
      result = HRESULT_FROM_WIN32(WSAGetLastError());
      *sentLength = 0;
    }
    else
    {
      this->sentDataLength += *sentLength;
    }
  }

  return result;
}

HRESULT CSocketContext::Receive(char *buffer, unsigned int length, unsigned int *receivedLength)
{
  return this->Receive(buffer, length, 0, receivedLength);
}

HRESULT CSocketContext::Receive(char *buffer, unsigned int length, int flags, unsigned int *receivedLength)
{
  return this->Receive(buffer, length, flags, receivedLength, NULL);
}

HRESULT CSocketContext::Receive(char *buffer, unsigned int length, unsigned int *receivedLength, CIpAddress **sender)
{
  return this->Receive(buffer, length, 0, receivedLength, sender);
}

HRESULT CSocketContext::Receive(char *buffer, unsigned int length, int flags, unsigned int *receivedLength, CIpAddress **sender)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, buffer, result, E_INVALIDARG);
  CHECK_POINTER_HRESULT(result, receivedLength, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    SOCKADDR_STORAGE sockAddr;
    int sockAddrLength = sizeof(SOCKADDR_STORAGE);

    *receivedLength = recvfrom(this->internalSocket, buffer, length, flags, (struct sockaddr *)&sockAddr, &sockAddrLength);
    if ((*receivedLength) == SOCKET_ERROR)
    {
      int lastError = WSAGetLastError();

      result = (lastError == WSAEWOULDBLOCK) ? S_OK : HRESULT_FROM_WIN32(lastError);
      *receivedLength = 0;
    }
    else if ((flags & MSG_PEEK) == 0)
    {
      // not checking incoming data length
      this->receivedDataLength += *receivedLength;

      if (sender != NULL)
      {
        *sender = new CIpAddress(&sockAddr, sockAddrLength);
        CHECK_POINTER_HRESULT(result, (*sender), result, E_OUTOFMEMORY);

        if (SUCCEEDED(result))
        {
          (*sender)->SetProtocol(this->GetIpAddress()->GetProtocol());
          (*sender)->SetSockType(this->GetIpAddress()->GetSockType());
        }
      }
    }
  }

  return result;
}

HRESULT CSocketContext::Select(bool read, bool write, unsigned int timeout, unsigned int *state)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, state, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    fd_set readFD;
    fd_set writeFD;
    fd_set exceptFD;

    FD_ZERO(&readFD);
    FD_ZERO(&writeFD);
    FD_ZERO(&exceptFD);

    if (read)
    {
      // want to read from socket
      FD_SET(this->internalSocket, &readFD);
    }
    if (write)
    {
      // want to write to socket
      FD_SET(this->internalSocket, &writeFD);
    }
    // we want to receive errors
    FD_SET(this->internalSocket, &exceptFD);

    timeval sendTimeout;
    sendTimeout.tv_sec = timeout;
    sendTimeout.tv_usec = 0;

    int selectResult = select(0, &readFD, &writeFD, &exceptFD, &sendTimeout);
    if (selectResult == 0)
    {
      // timeout occured
      result = HRESULT_FROM_WIN32(WSAETIMEDOUT);
    }
    else if (selectResult == SOCKET_ERROR)
    {
      // socket error occured
      result = HRESULT_FROM_WIN32(WSAGetLastError());
    }
    else
    {
      // result is 0, correct return
      if (FD_ISSET(this->internalSocket, &exceptFD))
      {
        // error occured on socket, select function was successful
        int err;
        int errlen = sizeof(err);

        result = HRESULT_FROM_WIN32(this->GetOption(SOL_SOCKET, SO_ERROR, (char *)&err, &errlen));
        if (SUCCEEDED(result))
        {
          result = HRESULT_FROM_WIN32(err);
        }
      }

      if (SUCCEEDED(result))
      {
        *state = SOCKET_STATE_UNDEFINED;

        if (read && (FD_ISSET(this->internalSocket, &readFD) != 0))
        {
          (*state) |= SOCKET_STATE_READABLE;
        }

        if (write && (FD_ISSET(this->internalSocket, &writeFD) != 0))
        {
          (*state) |= SOCKET_STATE_WRITABLE;
        }

      }
    }
  }

  return result;
}