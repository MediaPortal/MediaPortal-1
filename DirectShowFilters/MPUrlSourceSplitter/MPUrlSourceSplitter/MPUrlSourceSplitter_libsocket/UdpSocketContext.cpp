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

#include "UdpSocketContext.h"

CUdpSocketContext::CUdpSocketContext(HRESULT *result)
  : CSocketContext(result)
{
  this->lastSenderIpAddress = NULL;
}

CUdpSocketContext::CUdpSocketContext(HRESULT *result, SOCKET socket)
  : CSocketContext(result, socket)
{
  this->lastSenderIpAddress = NULL;
}

CUdpSocketContext::~CUdpSocketContext(void)
{
  FREE_MEM_CLASS(this->lastSenderIpAddress);
}

/* get methods */

CIpAddress *CUdpSocketContext::GetLastSenderIpAddress(void)
{
  return this->lastSenderIpAddress;
}

/* set methods */

bool CUdpSocketContext::SetLastSenderIpAddress(CIpAddress *sender)
{
  FREE_MEM_CLASS(this->lastSenderIpAddress);
  this->lastSenderIpAddress = (sender != NULL) ? (sender->Clone()) : NULL;

  return (((this->lastSenderIpAddress == NULL) && (sender == NULL)) || ((this->lastSenderIpAddress != NULL) && (sender != NULL)));
}

/* other methods */

HRESULT CUdpSocketContext::Send(const char *buffer, unsigned int length, unsigned int *sentLength)
{
  return this->Send(buffer, length, sentLength, this->GetLastSenderIpAddress());
}

HRESULT CUdpSocketContext::Send(const char *buffer, unsigned int length, int flags, unsigned int *sentLength)
{
  return this->Send(buffer, length, flags, sentLength, this->GetLastSenderIpAddress());
}

HRESULT CUdpSocketContext::Send(const char *buffer, unsigned int length, unsigned int *sentLength, CIpAddress *client)
{
  return this->Send(buffer, length, 0, sentLength, client);
}

HRESULT CUdpSocketContext::Send(const char *buffer, unsigned int length, int flags, unsigned int *sentLength, CIpAddress *client)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, buffer, result, E_INVALIDARG);
  CHECK_POINTER_HRESULT(result, sentLength, result, E_INVALIDARG);
  CHECK_POINTER_HRESULT(result, client, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    *sentLength = sendto(this->internalSocket, buffer, length, flags, client->GetAddressIP(), client->GetAddressLength());
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

HRESULT CUdpSocketContext::Receive(char *buffer, unsigned int length, unsigned int *receivedLength)
{
  return this->Receive(buffer, length, 0, receivedLength);
}

HRESULT CUdpSocketContext::Receive(char *buffer, unsigned int length, int flags, unsigned int *receivedLength)
{
  return this->Receive(buffer, length, flags, receivedLength, NULL);
}

HRESULT CUdpSocketContext::Receive(char *buffer, unsigned int length, unsigned int *receivedLength, CIpAddress **sender)
{
  return this->Receive(buffer, length, 0, receivedLength, sender);
}

HRESULT CUdpSocketContext::Receive(char *buffer, unsigned int length, int flags, unsigned int *receivedLength, CIpAddress **sender)
{
  HRESULT result = S_OK;

  CIpAddress *ipAddress = NULL;
  result = __super::Receive(buffer, length, flags, receivedLength, &ipAddress);

  if (SUCCEEDED(result))
  {
    if ((*receivedLength) != 0)
    {
      // we received some data, remember sender
      result = this->SetLastSenderIpAddress(ipAddress) ? result : E_OUTOFMEMORY;
    }

    if (SUCCEEDED(result) && (sender != NULL))
    {
      *sender = ipAddress->Clone();
      CHECK_POINTER_HRESULT(result, (*sender), result, E_OUTOFMEMORY);
    }
  }

  FREE_MEM_CLASS(ipAddress);
  return result;
}