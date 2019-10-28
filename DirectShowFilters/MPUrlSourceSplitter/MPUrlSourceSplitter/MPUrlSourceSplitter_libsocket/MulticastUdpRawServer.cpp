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

#include "MulticastUdpRawServer.h"
#include "Dns.h"
#include "MulticastUdpRawSocketContext.h"
#include "Ipv4Header_Constants.h"


CMulticastUdpRawServer::CMulticastUdpRawServer(HRESULT *result)
  : CMulticastUdpServer(result)
{
  this->igmpSockets = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->igmpSockets = new CSocketContextCollection(result);
    CHECK_POINTER_HRESULT(*result, this->igmpSockets, *result, E_OUTOFMEMORY);
  }
}

CMulticastUdpRawServer::~CMulticastUdpRawServer(void)
{
  this->StopListening();

  FREE_MEM_CLASS(this->igmpSockets);
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CMulticastUdpRawServer::Initialize(int family, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterfaceCollection *networkInterfaces, CIpv4Header *header)
{
  HRESULT result = __super::Initialize(family, multicastAddress, sourceAddress, networkInterfaces);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->igmpSockets);
  CHECK_POINTER_DEFAULT_HRESULT(result, header);

  if (SUCCEEDED(result))
  {
    this->igmpSockets->Clear();

    for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->sockets->Count())); i++)
    {
      CMulticastUdpSocketContext *socket = dynamic_cast<CMulticastUdpSocketContext *>(this->sockets->GetItem(i));
      CMulticastUdpRawSocketContext *igmpSocket = new CMulticastUdpRawSocketContext(&result, socket->GetMulticastAddress(), socket->GetSourceAddress(), socket->GetNetworkInterface(), header);
      CHECK_POINTER_HRESULT(result, igmpSocket, result, E_OUTOFMEMORY);

      CIpAddress *ipAddr = socket->GetIpAddress()->Clone();
      CHECK_POINTER_HRESULT(result, ipAddr, result, E_OUTOFMEMORY);

      if (SUCCEEDED(result))
      {
        ipAddr->SetSockType(SOCK_RAW);
        ipAddr->SetProtocol(IPV4_HEADER_IGMP_PROTOCOL);

        CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), igmpSocket->SetIpAddress(ipAddr), result);

        CHECK_CONDITION_HRESULT(result, this->igmpSockets->Add(igmpSocket), result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(igmpSocket));
      }

      FREE_MEM_CLASS(ipAddr);
    }
  }

  return result;
}

HRESULT CMulticastUdpRawServer::StartListening(void)
{
  // we ignore base StartListening, we do whole procedure by its own
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, this->sockets, result, E_POINTER);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->sockets->Count())); i++)
  {
    CMulticastUdpSocketContext *socket = dynamic_cast<CMulticastUdpSocketContext *>(this->sockets->GetItem(i));
    CMulticastUdpRawSocketContext *igmpSocket = dynamic_cast<CMulticastUdpRawSocketContext *>(this->igmpSockets->GetItem(i));

    // create server socket
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->CreateSocket(), result);
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), igmpSocket->CreateSocket(), result);

    // set non-blocking mode
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->SetBlockingMode(false), result);
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), igmpSocket->SetBlockingMode(false), result);

    // set reuse address
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->SetReuseAddress(true), result);
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), igmpSocket->SetReuseAddress(true), result);

    // bind socket to local address and port
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->Bind(), result);
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), igmpSocket->Bind(), result);

    // subscribe to multicast group
    //CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), socket->SubscribeToMulticastGroup(), result);
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), igmpSocket->SubscribeToMulticastGroup(), result);
  }

  return result;
}

HRESULT CMulticastUdpRawServer::StopListening(void)
{
  this->igmpSockets->Clear();

  return __super::StopListening();
}