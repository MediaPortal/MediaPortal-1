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

#ifndef __TCP_SERVER_DEFINED
#define __TCP_SERVER_DEFINED

#include "SimpleServer.h"

#define TCP_SERVER_FLAG_NONE                                                  SIMPLE_SERVER_FLAG_NONE

#define TCP_SERVER_FLAG_SERVER                                                (1 << (SIMPLE_SERVER_FLAG_LAST + 2))

#define TCP_SERVER_FLAG_LAST                                                  (SIMPLE_SERVER_FLAG_LAST + 2)


class CTcpServer : public CSimpleServer
{
public:
  CTcpServer(HRESULT *result);
  virtual ~CTcpServer(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // initializes TCP server on specified interfaces
  // @param family : socket family (AF_INET, AF_INET6, ...)
  // @param port : port to bind server
  // @param networkInterfaces : network interfaces to initialize TCP server
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int family, WORD port, CNetworkInterfaceCollection *networkInterfaces);

  // initializes TCP server on all network interfaces
  // @param family : socket family (AF_INET, AF_INET6, ...)
  // @param port : port to bind server
  // @param connections : the maximum length of the queue of pending connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int family, WORD port, int connections);

  // initializes TCP server on specified interfaces
  // @param family : socket family (AF_INET, AF_INET6, ...)
  // @param port : port to bind server
  // @param connections : the maximum length of the queue of pending connections
  // @param networkInterfaces : network interfaces to initialize TCP server
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int family, WORD port, int connections, CNetworkInterfaceCollection *networkInterfaces);

  // starts listening to incoming connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT StartListening(void);

  // stops listening to incoming connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT StopListening(void);

protected:
};

#endif