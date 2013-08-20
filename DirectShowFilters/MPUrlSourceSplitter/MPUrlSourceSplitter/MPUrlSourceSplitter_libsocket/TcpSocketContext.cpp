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

#include "TcpSocketContext.h"

CTcpSocketContext::CTcpSocketContext(void)
  : CSocketContext()
{
  this->connections = 0;
  this->acceptedConnections = new CSocketContextCollection();
}

CTcpSocketContext::CTcpSocketContext(SOCKET socket)
  : CSocketContext(socket)
{
  this->connections = 0;
  this->acceptedConnections = new CSocketContextCollection();
}

CTcpSocketContext::~CTcpSocketContext(void)
{
  FREE_MEM_CLASS(this->acceptedConnections);
}

/* get methods */

int CTcpSocketContext::GetConnections(void)
{
  return this->connections;
}

CSocketContextCollection *CTcpSocketContext::GetAcceptedConnections(void)
{
  return this->acceptedConnections;
}

/* set methods */

/* other methods */

HRESULT CTcpSocketContext::Listen()
{
  HRESULT result = S_OK;

  if (listen(this->internalSocket, this->GetConnections()) == SOCKET_ERROR)
  {
    result = WSAGetLastError();
  }

  return result;
}

HRESULT CTcpSocketContext::Accept(CSocketContext **acceptedContext)
{
  HRESULT result = S_OK;
  CHECK_POINTER_HRESULT(result, acceptedContext, result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    SOCKET acceptedSocket = accept(this->internalSocket, NULL, NULL);
    if (acceptedSocket == INVALID_SOCKET)
    {
      result = HRESULT_FROM_WIN32(WSAGetLastError());
    }

    if (SUCCEEDED(result))
    {
      *acceptedContext = new CSocketContext(acceptedSocket);
      CHECK_POINTER_HRESULT(result, (*acceptedContext), result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), (*acceptedContext)->SetIpAddress(this->GetIpAddress()), result);
    }
  }

  return result;
}

HRESULT CTcpSocketContext::Initialize()
{
  return this->Initialize(SOMAXCONN);
}

HRESULT CTcpSocketContext::Initialize(int connections)
{
  this->connections = connections;

  return S_OK;
}

HRESULT CTcpSocketContext::IsPendingIncomingConnection(void)
{
  HRESULT result = S_OK;

  unsigned int state = SOCKET_STATE_UNDEFINED;
  result = this->Select(true, false, 0, &state);
  result = (result == HRESULT_FROM_WIN32(WSAETIMEDOUT)) ? S_OK : result;

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = ((state & SOCKET_STATE_READABLE) != 0) ? S_FALSE : S_OK);

  return result;
}

HRESULT CTcpSocketContext::AcceptPendingIncomingConnection(void)
{
  CSocketContext *acceptedSocket = NULL;
  HRESULT result = this->Accept(&acceptedSocket);

  CHECK_CONDITION_EXECUTE(SUCCEEDED(result), result = this->acceptedConnections->Add(acceptedSocket) ? result : E_OUTOFMEMORY);
  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(acceptedSocket));

  return result;
}