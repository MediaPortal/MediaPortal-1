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

#include "SimpleServer.h"
#include "IpAddressCollection.h"
#include "Dns.h"

CSimpleServer::CSimpleServer(HRESULT *result)
  : CFlags()
{
  this->sockets = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->sockets = new CSocketContextCollection(result);
    CHECK_POINTER_HRESULT(*result, this->sockets, *result, E_OUTOFMEMORY);
  }
}

CSimpleServer::~CSimpleServer(void)
{
  this->StopListening();

  FREE_MEM_CLASS(this->sockets);
}

/* get methods */

CSocketContextCollection *CSimpleServer::GetSockets(void)
{
  return this->sockets;
}

/* set methods */

/* other methods */

HRESULT CSimpleServer::Initialize(int family, WORD port)
{
  HRESULT result = S_OK;
  CNetworkInterfaceCollection *interfaces = new CNetworkInterfaceCollection(&result);
  CHECK_POINTER_HRESULT(result, interfaces, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), CNetworkInterface::GetAllNetworkInterfaces(interfaces, family), result);
  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->Initialize(family, port, interfaces), result);

  FREE_MEM_CLASS(interfaces);
    
  return result;
}

HRESULT CSimpleServer::Initialize(int family, WORD port, CNetworkInterfaceCollection *networkInterfaces)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->sockets);
  CHECK_POINTER_DEFAULT_HRESULT(result, networkInterfaces);

  return result;
}

HRESULT CSimpleServer::StartListening(void)
{
  return S_OK;
}

HRESULT CSimpleServer::StopListening(void)
{
  this->sockets->Clear();

  return S_OK;
}

HRESULT CSimpleServer::MaintainConnections(void)
{
  return S_OK;
}