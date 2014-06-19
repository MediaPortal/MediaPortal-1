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

#ifndef __MULTICAST_UDP_SERVER_DEFINED
#define __MULTICAST_UDP_SERVER_DEFINED

#include "UdpServer.h"

#define MULTICAST_UDP_SERVER_FLAG_NONE                                        UDP_SERVER_FLAG_NONE

#define MULTICAST_UDP_SERVER_FLAG_SERVER                                      (1 << (UDP_SERVER_FLAG_LAST + 0))

#define MULTICAST_UDP_SERVER_FLAG_LAST                                        (UDP_SERVER_FLAG_LAST + 1)


class CMulticastUdpServer : public CUdpServer
{
public:
  CMulticastUdpServer(HRESULT *result);
  virtual ~CMulticastUdpServer(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // initializes multicast UDP server on specified interfaces
  // @param family : socket family (AF_INET, AF_INET6, ...)
  // @param multicastAddress : multicast address to bind server
  // @param multicastPort : multicast port to bind server
  // @param sourceAddress : the address of source (can be NULL if not specified)
  // @param sourcePort : the port of source (ignored if sourceAddress is NULL)
  // @param networkInterfaces : network interfaces to initialize multicast UDP server
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int family, const wchar_t *multicastAddress, WORD multicastPort, const wchar_t *sourceAddress, WORD sourcePort, CNetworkInterfaceCollection *networkInterfaces);

  // initializes multicast UDP server on specified interfaces
  // @param family : socket family (AF_INET, AF_INET6, ...)
  // @param multicastAddress : multicast IP address to bind server
  // @param sourceAddress : the IP address of source (can be NULL if not specified)
  // @param networkInterfaces : network interfaces to initialize multicast UDP server
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT Initialize(int family, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterfaceCollection *networkInterfaces);

  // starts listening to incoming connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT StartListening(void);

  // stops listening to incoming connections
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT StopListening(void);

protected:
};

#endif