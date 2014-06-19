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
#include "Flags.h"

#define SIMPLE_SERVER_FLAG_NONE                                               FLAGS_NONE

#define SIMPLE_SERVER_FLAG_LAST                                               (FLAGS_LAST + 0)

class CSimpleServer : public CFlags
{
public:
  CSimpleServer(HRESULT *result);
  virtual ~CSimpleServer(void);

  /* get methods */

  // gets all servers
  // @return : all servers
  virtual CSocketContextCollection *GetServers(void);

  /* set methods */

  /* other methods */

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

  /* methods */
};

#endif