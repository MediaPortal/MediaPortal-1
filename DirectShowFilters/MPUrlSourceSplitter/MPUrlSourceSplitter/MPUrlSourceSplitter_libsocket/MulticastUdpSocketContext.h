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

#ifndef __MULTICAST_UDP_SOCKET_CONTEXT_DEFINED
#define __MULTICAST_UDP_SOCKET_CONTEXT_DEFINED

#include "UdpSocketContext.h"
#include "NetworkInterface.h"

#define MULTICAST_UDP_SOCKET_CONTEXT_FLAG_NONE                                UDP_SOCKET_CONTEXT_FLAG_NONE

#define MULTICAST_UDP_SOCKET_CONTEXT_FLAG_SUBSCRIBED_TO_GROUP                 (1 << (UDP_SOCKET_CONTEXT_FLAG_LAST + 0))

#define MULTICAST_UDP_SOCKET_CONTEXT_FLAG_LAST                                (UDP_SOCKET_CONTEXT_FLAG_LAST + 1)

class CMulticastUdpSocketContext : public CUdpSocketContext
{
public:
  CMulticastUdpSocketContext(HRESULT *result, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterface *networkInterface);
  CMulticastUdpSocketContext(HRESULT *result, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterface *networkInterface, SOCKET socket);
  virtual ~CMulticastUdpSocketContext(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // subscribes to multicast group
  // @return : S_OK if successful, false otherwise
  virtual HRESULT SubscribeToMulticastGroup(void);

  // unsubscribes from multicast group
  // @return : S_OK if successful, false otherwise
  virtual HRESULT UnsubscribeFromMulticastGroup(void);

  // joins to multicast group (IPV4)
  // @return : S_OK if successful, false otherwise
  virtual HRESULT JoinMulticastGroupIPv4(void);

  // joins to multicast group (IPV6)
  // @return : S_OK if successful, false otherwise
  virtual HRESULT JoinMulticastGroupIPv6(void);

  // leaves multicast group (IPV4)
  // @return : S_OK if successful, false otherwise
  virtual HRESULT LeaveMulticastGroupIPv4(void);

  // leaves multicast group (IPV6)
  // @return : S_OK if successful, false otherwise
  virtual HRESULT LeaveMulticastGroupIPv6(void);

  // sets reusing address
  // @param reuseAddress : true if address can be reused, false otherwise
  // @return : S_OK if successful, false otherwise
  virtual HRESULT SetReuseAddress(bool reuseAddress);

protected:

  //bool subscribedToMulticastGroup;
  CIpAddress *multicastAddress;
  CIpAddress *sourceAddress;
  CNetworkInterface *networkInterface;
};

#endif