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


CMulticastUdpRawServer::CMulticastUdpRawServer(HRESULT *result)
  : CMulticastUdpServer(result)
{
}

CMulticastUdpRawServer::~CMulticastUdpRawServer(void)
{
  this->StopListening();
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CMulticastUdpRawServer::Initialize(int family, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterfaceCollection *networkInterfaces, CIpv4Header *header)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->servers);
  CHECK_POINTER_DEFAULT_HRESULT(result, networkInterfaces);
  CHECK_POINTER_DEFAULT_HRESULT(result, header);

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
              ipAddr->SetSockType(SOCK_RAW);
              ipAddr->SetProtocol(header->GetProtocol());

              CMulticastUdpRawSocketContext *server = new CMulticastUdpRawSocketContext(&result, multicastAddress, sourceAddress, nic, header);
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

