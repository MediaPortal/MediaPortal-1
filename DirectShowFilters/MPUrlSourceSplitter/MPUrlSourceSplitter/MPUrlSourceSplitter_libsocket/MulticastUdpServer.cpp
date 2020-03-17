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

#include "MulticastUdpServer.h"
#include "Dns.h"
#include "MulticastUdpSocketContext.h"

CMulticastUdpServer::CMulticastUdpServer(HRESULT *result)
  : CUdpServer(result)
{
  this->flags |= MULTICAST_UDP_SERVER_FLAG_SERVER;
  this->flags &= ~UDP_SERVER_FLAG_SERVER;
}

CMulticastUdpServer::~CMulticastUdpServer(void)
{
  this->StopListening();
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CMulticastUdpServer::Initialize(int family, const wchar_t *multicastAddress, WORD multicastPort, const wchar_t *sourceAddress, WORD sourcePort, CNetworkInterfaceCollection *networkInterfaces)
{
  HRESULT result = S_OK;

  CIpAddressCollection *multicastAddresses = new CIpAddressCollection(&result);
  CIpAddressCollection *sourceAddresses = new CIpAddressCollection(&result);
  CHECK_POINTER_HRESULT(result, multicastAddresses, result, E_OUTOFMEMORY);
  CHECK_POINTER_HRESULT(result, sourceAddresses, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = CDns::GetIpAddresses(multicastAddress, multicastPort, family, SOCK_DGRAM, IPPROTO_UDP, 0, multicastAddresses));
  CHECK_CONDITION_EXECUTE(SUCCEEDED(result) && (sourceAddress != NULL), result = CDns::GetIpAddresses(sourceAddress, sourcePort, family, SOCK_DGRAM, IPPROTO_UDP, 0, multicastAddresses));

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = (multicastAddresses->Count() == 1) ? result : E_INVALIDARG);
  CHECK_CONDITION_EXECUTE(SUCCEEDED(result) && (sourceAddress != NULL), result = (sourceAddresses->Count() == 1) ? result : E_INVALIDARG);

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->Initialize(family, multicastAddresses->GetItem(0), (sourceAddress != NULL) ? sourceAddresses->GetItem(0) : NULL, networkInterfaces));

  FREE_MEM_CLASS(multicastAddresses);
  FREE_MEM_CLASS(sourceAddresses);

  return result;
}

HRESULT CMulticastUdpServer::Initialize(int family, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterfaceCollection *networkInterfaces)
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

            CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = ipAddr->SetPort(multicastAddress->GetPort()) ? result : E_INVALIDARG);

            if (SUCCEEDED(result) && (ipAddr->GetFamily() == multicastAddress->GetFamily()))
            {
              ipAddr->SetSockType(SOCK_DGRAM);
              ipAddr->SetProtocol(IPPROTO_UDP);

              CMulticastUdpSocketContext *server = new CMulticastUdpSocketContext(&result, multicastAddress, sourceAddress, nic);
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

HRESULT CMulticastUdpServer::StartListening(void)
{
  // we ignore base StartListening, we do whole procedure by its own
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, this->servers, result, E_POINTER);

  for (unsigned int i = 0; (SUCCEEDED(result) && (i < this->servers->Count())); i++)
  {
    CMulticastUdpSocketContext *server = dynamic_cast<CMulticastUdpSocketContext *>(this->servers->GetItem(i));

    // create server socket
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->CreateSocket(), result);

    // set non-blocking mode
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->SetBlockingMode(false), result);

    // set reuse address
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->SetReuseAddress(true), result);

    // bind socket to local address and port
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->Bind(), result);

    // subscribe to multicast group
    CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), server->SubscribeToMulticastGroup(), result);
  }

  return result;
}

HRESULT CMulticastUdpServer::StopListening(void)
{
  return __super::StopListening();
}