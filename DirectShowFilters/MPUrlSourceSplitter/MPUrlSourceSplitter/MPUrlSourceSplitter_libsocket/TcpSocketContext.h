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

#pragma once

#ifndef __TCP_SOCKET_CONTEXT_DEFINED
#define __TCP_SOCKET_CONTEXT_DEFINED

#include "SocketContext.h"
#include "SocketContextCollection.h"

#define TCP_SOCKET_CONTEXT_FLAG_NONE                                          SOCKET_CONTEXT_FLAG_NONE

#define TCP_SOCKET_CONTEXT_FLAG_LAST                                          (SOCKET_CONTEXT_FLAG_LAST + 0)


class CTcpSocketContext : public CSocketContext
{
public:
  CTcpSocketContext(HRESULT *result);
  CTcpSocketContext(HRESULT *result, SOCKET socket);
  virtual ~CTcpSocketContext(void);

  /* get methods */

  // gets maximum length of the queue of pending connections
  // @return : maximum length of the queue of pending connections
  virtual int GetConnections(void);

  // gets accepted incoming connections
  // @return : accepted incoming connections
  virtual CSocketContextCollection *GetAcceptedConnections(void);

  /* set methods */

  /* other methods */

  // creates and places a socket in a state in which it is listening for an incoming connection
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Listen(void);
  
  // accepts incoming socket connection
  // @param acceptedContext : reference to new created accepted socket context
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Accept(CSocketContext **acceptedContext);

  // initializes server socket context with maximum length of the queue of pending connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(void);

  // initializes server socket context
  // @param connections : the maximum length of the queue of pending connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int connections);

  // tests if there is some pending incoming connection
  // @return : S_OK if no connection request, S_FALSE if there is incoming connection request, error code otherwise (can be system or WSA)
  virtual HRESULT IsPendingIncomingConnection(void);

  // accepts incoming socket connection
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT AcceptPendingIncomingConnection(void);

protected:

  // holds maximum length of the queue of pending connections
  int connections;

  // holds accepted incoming connections
  CSocketContextCollection *acceptedConnections;
};

#endif