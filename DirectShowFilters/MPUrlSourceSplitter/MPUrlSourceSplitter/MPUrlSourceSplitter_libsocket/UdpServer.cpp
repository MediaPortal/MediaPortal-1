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

#include "UdpServer.h"
#include "IpAddressCollection.h"
#include "Dns.h"
#include "UdpSocketContext.h"

CUdpServer::CUdpServer(HRESULT *result)
  : CSimpleServer(result)
{
  this->flags |= UDP_SERVER_FLAG_SERVER;
}

CUdpServer::~CUdpServer(void)
{
  this->StopListening();
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CUdpServer::Initialize(int family, WORD port, CNetworkInterfaceCollection *networkInterfaces)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->servers);
  CHECK_POINTER_DEFAULT_HRESULT(result, networkInterfaces);

  if (SUCCEEDED(result))
  {
    this->servers->Clear();

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
            CHECK_POINTER_HRESULT(result, nic, result, E_OUTOFMEMORY);

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = ipAddr->SetPort(port) ? result : E_INVALIDARG);

            if (SUCCEEDED(result))
            {
              ipAddr->SetSockType(SOCK_DGRAM);
              ipAddr->SetProtocol(IPPROTO_UDP);

              CUdpSocketContext *server = new CUdpSocketContext(&result);
              CHECK_POINTER_HRESULT(result, server, result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->SetIpAddress(ipAddr), result);

              CHECK_CONDITION_HRESULT(result, this->servers->Add(server), result, E_OUTOFMEMORY);

              CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(server));
            }

            FREE_MEM_CLASS(ipAddr);
          }
        }
      }
    }
  }

  return result;
}

HRESULT CUdpServer::StartListening(void)
{
  HRESULT result = __super::StartListening();
  CHECK_POINTER_HRESULT(result, this->servers, result, E_POINTER);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->servers->Count())); i++)
  {
    CUdpSocketContext *server = dynamic_cast<CUdpSocketContext *>(this->servers->GetItem(i));

    // create server socket
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->CreateSocket(), result);

    // set non-blocking mode
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->SetBlockingMode(false), result);

    // bind socket to local address and port
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->Bind(), result);
  }

  return result;
}

HRESULT CUdpServer::StopListening(void)
{
  return __super::StopListening();
}
