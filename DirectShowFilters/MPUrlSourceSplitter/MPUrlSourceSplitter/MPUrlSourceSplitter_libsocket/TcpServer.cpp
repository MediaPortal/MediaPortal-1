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

#include "TcpServer.h"
#include "TcpSocketContext.h"
#include "IpAddressCollection.h"
#include "Dns.h"

CTcpServer::CTcpServer(HRESULT *result)
  : CSimpleServer(result)
{
  this->flags |= TCP_SERVER_FLAG_SERVER;
}

CTcpServer::~CTcpServer(void)
{
  this->StopListening();
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CTcpServer::Initialize(int family, WORD port, CNetworkInterfaceCollection *networkInterfaces)
{
  return this->Initialize(family, port, SOMAXCONN, networkInterfaces);
}

HRESULT CTcpServer::Initialize(int family, WORD port, int connections)
{
  HRESULT result = S_OK;
  CNetworkInterfaceCollection *interfaces = new CNetworkInterfaceCollection(&result);
  CHECK_POINTER_HRESULT(result, interfaces, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), CNetworkInterface::GetAllNetworkInterfaces(interfaces, family), result);
  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->Initialize(family, port, connections, interfaces), result);

  FREE_MEM_CLASS(interfaces);
    
  return result;
}

HRESULT CTcpServer::Initialize(int family, WORD port, int connections, CNetworkInterfaceCollection *networkInterfaces)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->sockets);
  CHECK_POINTER_DEFAULT_HRESULT(result, networkInterfaces);

  if (SUCCEEDED(result))
  {
    this->sockets->Clear();

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < networkInterfaces->Count())); i++)
    {
      CNetworkInterface *nic = networkInterfaces->GetItem(i);
      CHECK_POINTER_HRESULT(result, nic, result, E_POINTER);

      if (SUCCEEDED(result))
      {
        if (nic->GetOperationalStatus() == IfOperStatusUp)
        {
          for (unsigned int j = 0; (SUCCEEDED(result) && (j < nic->GetUnicastAddresses()->Count())); j++)
          {
            CIpAddress *ipAddr = nic->GetUnicastAddresses()->GetItem(j)->Clone();
            CHECK_POINTER_HRESULT(result, ipAddr, result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = ipAddr->SetPort(port) ? result : E_INVALIDARG);

            if (SUCCEEDED(result))
            {
              ipAddr->SetSockType(SOCK_STREAM);
              ipAddr->SetProtocol(IPPROTO_TCP);

              CTcpSocketContext *socket = new CTcpSocketContext(&result);
              CHECK_POINTER_HRESULT(result, socket, result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->SetIpAddress(ipAddr), result);
              CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->Initialize(connections), result);

              CHECK_CONDITION_HRESULT(result, this->sockets->Add(socket), result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(socket));
            }

            FREE_MEM_CLASS(ipAddr);
          }
        }
      }
    }
  }

  return result;
}

HRESULT CTcpServer::StartListening(void)
{
  HRESULT result = __super::StartListening();
  CHECK_POINTER_HRESULT(result, this->sockets, result, E_POINTER);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->sockets->Count())); i++)
  {
    CTcpSocketContext *socket = dynamic_cast<CTcpSocketContext *>(this->sockets->GetItem(i));

    // create server socket
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->CreateSocket(), result);

    // set non-blocking mode
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->SetBlockingMode(false), result);

    // bind socket to local address and port
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->Bind(), result);

    // listen on socket
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->Listen(), result);
  }

  return result;
}

HRESULT CTcpServer::StopListening(void)
{
  return __super::StopListening();
}
