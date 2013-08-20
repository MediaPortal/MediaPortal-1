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

#ifndef __SIMPLE_SERVER_DEFINED
#define __SIMPLE_SERVER_DEFINED

#include "SocketContextCollection.h"
#include "NetworkInterfaceCollection.h"

#define SERVER_TYPE_UNSPECIFIED                                       UINT_MAX

class CSimpleServer
{
public:
  CSimpleServer(void);
  virtual ~CSimpleServer(void);

  /* get methods */

  // gets all servers
  // @return : all servers
  virtual CSocketContextCollection *GetServers(void);

  // gets server type
  // @return : server type or SERVER_TYPE_UNSPECIFIED if not specified
  virtual unsigned int GetServerType(void);

  /* set methods */

  /* other methods */

  // tests if server type is requested type
  // @param serverType : the requested server type
  // @return : true if server type is requested type, false otherwise
  virtual bool IsServerType(unsigned int serverType);

  // initializes simple server on all network interfaces
  // @param family : socket family (AF_INET, AF_INET6, ...)
  // @param port : port to bind server
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int family, WORD port);

  // initializes simple server on specified interfaces
  // @param family : socket family (AF_INET, AF_INET6, ...)
  // @param port : port to bind server
  // @param networkInterfaces : network interfaces to initialize simple server
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int family, WORD port, CNetworkInterfaceCollection *networkInterfaces);

  // starts listening to incoming connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT StartListening(void);

  // stops listening to incoming connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT StopListening(void);

protected:

  // holds socket contexts
  CSocketContextCollection *servers;

  // holds server type
  unsigned int serverType;
};

#endif