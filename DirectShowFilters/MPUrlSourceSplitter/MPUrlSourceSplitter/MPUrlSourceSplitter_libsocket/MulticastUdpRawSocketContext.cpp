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

#include "MulticastUdpRawSocketContext.h"

CMulticastUdpRawSocketContext::CMulticastUdpRawSocketContext(HRESULT *result, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterface *networkInterface)
  : CMulticastUdpSocketContext(result, multicastAddress, sourceAddress, networkInterface)
{
}

CMulticastUdpRawSocketContext::CMulticastUdpRawSocketContext(HRESULT *result, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterface *networkInterface, SOCKET socket)
  : CMulticastUdpSocketContext(result, multicastAddress, sourceAddress, networkInterface, socket)
{
}

CMulticastUdpRawSocketContext::~CMulticastUdpRawSocketContext()
{
  if (this->IsSetFlags(MULTICAST_UDP_SOCKET_CONTEXT_FLAG_SUBSCRIBED_TO_GROUP))
  {
    this->UnsubscribeFromMulticastGroup();
  }
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CMulticastUdpRawSocketContext::CreateSocket(void)
{
  HRESULT result = (this->GetIpAddress() != NULL) ? S_OK : E_POINTER;
  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->CloseSocket(), result);

  if (SUCCEEDED(result))
  {
    result = E_NOTIMPL;

  //  this->internalSocket = socket(this->GetIpAddress()->GetFamily(), this->GetIpAddress()->GetSockType(), this->GetIpAddress()->GetProtocol());

  //  if (this->internalSocket == INVALID_SOCKET)
  //  {
  //    result = HRESULT_FROM_WIN32(WSAGetLastError());
  //  }
  //  else
  //  {
  //    // set socket buffer size
  //    DWORD dw = BUFFER_LENGTH_DEFAULT;
  //    int dwLen = sizeof(dw);

  //    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(SOL_SOCKET, SO_RCVBUF, (const char*)&dw, dwLen), result);
  //    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(SOL_SOCKET, SO_SNDBUF, (const char*)&dw, dwLen), result);
  //  }
  }

  return result;
}
