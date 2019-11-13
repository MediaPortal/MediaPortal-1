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

#ifndef __MULTICAST_UDP_RAW_SOCKET_CONTEXT_DEFINED
#define __MULTICAST_UDP_RAW_SOCKET_CONTEXT_DEFINED

#include "MulticastUdpSocketContext.h"
#include "NetworkInterface.h"
#include "Ipv4Header.h"

#define MULTICAST_UDP_RAW_SOCKET_CONTEXT_FLAG_NONE                            MULTICAST_UDP_SOCKET_CONTEXT_FLAG_NONE

#define MULTICAST_UDP_RAW_SOCKET_CONTEXT_FLAG_LAST                            (MULTICAST_UDP_SOCKET_CONTEXT_FLAG_LAST + 0)

#define IGMP_TYPE_MEMBERSHIP_QUERY                                            0x11
#define IGMP_TYPE_MEMBERSHIP_REPORT_V1                                        0x12
#define IGMP_TYPE_MEMBERSHIP_REPORT_V2                                        0x16
#define IGMP_TYPE_MEMBERSHIP_REPORT_V3                                        0x22
#define IGMP_TYPE_MEMBERSHIP_LEAVE                                            0x17

#define IGMP_PACKET_LENGTH_V2                                                 0x08

class CMulticastUdpRawSocketContext : public CMulticastUdpSocketContext
{
public:
  CMulticastUdpRawSocketContext(HRESULT *result, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterface *networkInterface, CIpv4Header *header);
  virtual ~CMulticastUdpRawSocketContext(void);

  /* get methods */

  // gets last IGMP packet ticks (in ms)
  // @return : last IGMP packet ticks (in ms)
  virtual DWORD GetLastIgmpPacket(void);

  /* set methods */

  /* other methods */

  // creates socket with specified family, type and protocol
  // @return : S_OK if successful, error code otherwise (can be system or WSA)
  virtual HRESULT CreateSocket(void);

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

protected:

  // holds requested IPV4 header
  CIpv4Header *header;

  // holds time when last IGMP packet was send
  DWORD lastIgmpPacket;

  /* methods */

  // calculates 1's complement checksum for payload
  // @param payload : the payload to calculate checksum
  // @param length : the length of payload to calculate checksum
  // @return : the checksum of payload
  static uint16_t CalculateChecksum(uint8_t *payload, uint16_t length);
};

#endif