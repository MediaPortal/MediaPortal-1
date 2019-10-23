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

#include "MulticastUdpRawSocketContext.h"
#include "Ipv4Header_Constants.h"

CMulticastUdpRawSocketContext::CMulticastUdpRawSocketContext(HRESULT *result, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterface *networkInterface, CIpv4Header *header)
  : CMulticastUdpSocketContext(result, multicastAddress, sourceAddress, networkInterface)
{
  this->header = NULL;

  CHECK_POINTER_DEFAULT_HRESULT(*result, header);

  if (SUCCEEDED(*result))
  {
    this->header = header->Clone();
    CHECK_CONDITION_HRESULT(*result, this->header, *result, E_OUTOFMEMORY);
  }
}

CMulticastUdpRawSocketContext::CMulticastUdpRawSocketContext(HRESULT *result, CIpAddress *multicastAddress, CIpAddress *sourceAddress, CNetworkInterface *networkInterface, CIpv4Header *header, SOCKET socket)
  : CMulticastUdpSocketContext(result, multicastAddress, sourceAddress, networkInterface, socket)
{
  this->header = NULL;

  CHECK_POINTER_DEFAULT_HRESULT(*result, header);

  if (SUCCEEDED(*result))
  {
    this->header = header->Clone();
    CHECK_CONDITION_HRESULT(*result, this->header, *result, E_OUTOFMEMORY);
  }
}

CMulticastUdpRawSocketContext::~CMulticastUdpRawSocketContext()
{
  if (this->IsSetFlags(MULTICAST_UDP_SOCKET_CONTEXT_FLAG_SUBSCRIBED_TO_GROUP))
  {
    this->UnsubscribeFromMulticastGroup();
  }

  FREE_MEM_CLASS(this->header);
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CMulticastUdpRawSocketContext::CreateSocket(void)
{
  HRESULT result = (this->GetIpAddress() != NULL) ? S_OK : E_POINTER;
  CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->CloseSocket(), result);

  if (SUCCEEDED(result))
  {
    this->internalSocket = socket(this->GetIpAddress()->GetFamily(), this->GetIpAddress()->GetSockType(), this->GetIpAddress()->GetProtocol());

    if (this->internalSocket == INVALID_SOCKET)
    {
      result = HRESULT_FROM_WIN32(WSAGetLastError());
    }
    else
    {
      // set IP_HDRINCL
      DWORD dw = 1;
      int dwLen = sizeof(dw);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(IPPROTO_IP, IP_HDRINCL, (const char*)&dw, dwLen), result);
      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(SOL_SOCKET, SO_BROADCAST, (const char*)&dw, dwLen), result);

      // set socket buffer size
      dw = BUFFER_LENGTH_DEFAULT;

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(SOL_SOCKET, SO_RCVBUF, (const char*)&dw, dwLen), result);
      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), this->SetOption(SOL_SOCKET, SO_SNDBUF, (const char*)&dw, dwLen), result);
    }
  }

  return result;
}

HRESULT CMulticastUdpRawSocketContext::SubscribeToMulticastGroup(void)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->networkInterface);

  unsigned int interfaceId = 0;
  unsigned int level = 0;

  if (SUCCEEDED(result))
  {
    switch (this->multicastAddress->GetFamily())
    {
      case AF_INET:
        interfaceId = networkInterface->GetIpv4Index();
        level = IPPROTO_IP;
        break;
      // IPV6 is not supported
      /*case AF_INET6:
        interfaceId = networkInterface->GetIpv6Index();
        level = IPPROTO_IPV6;
        break;*/
      default:
        result = E_FAIL;
    }
  }

  // we support only IGMPv2 message
  // IGMPv2 message is 8 bytes long
  ALLOC_MEM_DEFINE_SET(igmpPacket, uint8_t, 8, 0);
  CHECK_POINTER_HRESULT(result, igmpPacket, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    // IGMPv2 type
    *igmpPacket = IGMP_TYPE_MEMBERSHIP_REPORT_V2;

    // IGMPv2 max response time
    *(igmpPacket + 1) = 0;

    // IGMPv2 checksum (skip)

    // IGMPv2 group address
    memcpy(igmpPacket + 4, &this->multicastAddress->GetAddressIPv4()->sin_addr.S_un.S_addr, 4);

    // calculate IGMPv2 payload checksum
    uint16_t checksum = CMulticastUdpRawSocketContext::CalculateChecksum(igmpPacket, IGMP_PACKET_LENGTH_V2);

    // update IGMPv2 checksum
    *(igmpPacket + 2) = ((checksum & 0xFF00) >> 8);
    *(igmpPacket + 3) = (checksum & 0x00FF);
  }

  // the length of IPv4 packet is 20 bytes + IPv4 options length + IGMP packet length
  uint16_t ipv4PacketHeaderLength = IPV4_HEADER_LENGTH_MIN + this->header->GetOptionsLength();
  uint16_t ipv4PacketLength = ipv4PacketHeaderLength + IGMP_PACKET_LENGTH_V2;

  ALLOC_MEM_DEFINE_SET(ipv4packet, uint8_t, ipv4PacketLength, 0);
  CHECK_POINTER_HRESULT(result, ipv4packet, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    uint8_t ihl = ipv4PacketHeaderLength / 4;

    // version field is always 4
    *(ipv4packet) = (0x40 + ihl);

    // DSCP and ECN fields
    *(ipv4packet + 1) = ((this->header->GetDscp() << 2) + this->header->GetEcn());

    // total length of IPV4 packet
    *(ipv4packet + 2) = (ipv4PacketLength >> 8);
    *(ipv4packet + 3) = (ipv4PacketLength & 0x00FF);

    // IPV4 packet identification
    *(ipv4packet + 4) = (this->header->GetIdentification() >> 8);
    *(ipv4packet + 5) = (this->header->GetIdentification() & 0x00FF);

    // IPV4 flags and fragment offset (always 0)
    *(ipv4packet + 6) |= this->header->IsDontFragment() ? 0x40 : 0x00;
    *(ipv4packet + 6) |= this->header->IsMoreFragments() ? 0x20 : 0x00;

    *(ipv4packet + 8) = this->header->GetTtl();
    *(ipv4packet + 9) = this->header->GetProtocol();

    // IPV4 source address
    memcpy(ipv4packet + 12, &this->ipAddress->GetAddressIPv4()->sin_addr.S_un.S_addr, 4);

    // IPV4 destination address
    memcpy(ipv4packet + 16, &this->multicastAddress->GetAddressIPv4()->sin_addr.S_un.S_addr, 4);

    // IPV4 options
    memcpy(ipv4packet + 20, this->header->GetOptions(), this->header->GetOptionsLength());

    // calculate IPv4 header checksum
    uint16_t checksum = CMulticastUdpRawSocketContext::CalculateChecksum(ipv4packet, ipv4PacketHeaderLength);

    // update IPv4 header checksum
    *(ipv4packet + 10) = ((checksum & 0xFF00) >> 8);
    *(ipv4packet + 11) = (checksum & 0x00FF);

    // add IGMPv2 payload
    memcpy(ipv4packet + ipv4PacketHeaderLength, igmpPacket, IGMP_PACKET_LENGTH_V2);
  }

  // try to send IPv4 packet
  if (SUCCEEDED(result))
  {
    int sent_length = sendto(this->internalSocket, (const char *)ipv4packet, ipv4PacketLength, 0, this->multicastAddress->GetAddressIP(), this->multicastAddress->GetAddressLength());

    if (sent_length == SOCKET_ERROR)
    {
      int res = WSAGetLastError();
      result = HRESULT_FROM_WIN32(res);
   } 
  }

  FREE_MEM(ipv4packet);
  FREE_MEM(igmpPacket);

  return result;

  

  

  //if (SUCCEEDED(result))
  //{
  //  union
  //  {
  //    struct group_req groupReq;
  //    struct group_source_req groupSourceReq;
  //  } socketOption;
  //  socklen_t socketOptionLength;

  //  memset(&socketOption, 0, sizeof(socketOption));

  //  if (this->sourceAddress != NULL)
  //  {
  //    if ((this->multicastAddress->GetAddressLength() > sizeof(socketOption.groupSourceReq.gsr_group)) || (this->sourceAddress->GetAddressLength() > sizeof(socketOption.groupSourceReq.gsr_source)))
  //    {
  //      result = E_FAIL;
  //    }

  //    if (SUCCEEDED(result))
  //    {
  //      socketOption.groupSourceReq.gsr_interface = interfaceId;
  //      memcpy(&socketOption.groupSourceReq.gsr_source, this->sourceAddress->GetAddress(), this->sourceAddress->GetAddressLength());
  //      memcpy(&socketOption.groupSourceReq.gsr_group, this->multicastAddress->GetAddress(), this->multicastAddress->GetAddressLength());
  //      socketOptionLength = sizeof(socketOption.groupSourceReq);
  //    }
  //  }
  //  else
  //  {
  //    if (this->multicastAddress->GetAddressLength() > sizeof(socketOption.groupReq.gr_group))
  //    {
  //      result = E_FAIL;
  //    }

  //    if (SUCCEEDED(result))
  //    {
  //      socketOption.groupReq.gr_interface = interfaceId;
  //      memcpy(&socketOption.groupReq.gr_group, this->multicastAddress->GetAddress(), this->multicastAddress->GetAddressLength());
  //      socketOptionLength = sizeof(socketOption.groupReq);
  //    }
  //  }

  //  if (SUCCEEDED(result))
  //  {
  //    if (FAILED(this->SetOption(level, (this->sourceAddress != NULL) ? MCAST_JOIN_SOURCE_GROUP : MCAST_JOIN_GROUP, (char *)&socketOption, socketOptionLength)))
  //    {
  //      // subscribe to multicast group was not successful, fallback to IPv-specific APIs

  //      if (this->multicastAddress->IsIPv4())
  //      {
  //        result = this->JoinMulticastGroupIPv4();
  //      }
  //      else if (this->multicastAddress->IsIPv6())
  //      {
  //        result = this->JoinMulticastGroupIPv6();
  //      }
  //      else
  //      {
  //        result = E_FAIL;
  //      }
  //    }
  //  }
  //}

  //this->flags |= SUCCEEDED(result) ? MULTICAST_UDP_SOCKET_CONTEXT_FLAG_SUBSCRIBED_TO_GROUP : MULTICAST_UDP_SOCKET_CONTEXT_FLAG_NONE;
  //return result;
}

HRESULT CMulticastUdpRawSocketContext::UnsubscribeFromMulticastGroup(void)
{
  return E_NOTIMPL;

  //HRESULT result = S_OK;
  //CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  //CHECK_POINTER_DEFAULT_HRESULT(result, this->networkInterface);

  //unsigned int interfaceId = 0;
  //unsigned int level = 0;

  //if (SUCCEEDED(result))
  //{
  //  switch (this->multicastAddress->GetFamily())
  //  {
  //  case AF_INET:
  //    interfaceId = networkInterface->GetIpv4Index();
  //    level = IPPROTO_IP;
  //    break;
  //  case AF_INET6:
  //    interfaceId = networkInterface->GetIpv6Index();
  //    level = IPPROTO_IPV6;
  //    break;
  //  default:
  //    result = E_FAIL;
  //  }
  //}

  //if (SUCCEEDED(result))
  //{
  //  union
  //  {
  //    struct group_req groupReq;
  //    struct group_source_req groupSourceReq;
  //  } socketOption;
  //  socklen_t socketOptionLength;

  //  memset(&socketOption, 0, sizeof(socketOption));

  //  if (this->sourceAddress != NULL)
  //  {
  //    if ((this->multicastAddress->GetAddressLength() > sizeof(socketOption.groupSourceReq.gsr_group)) || (this->sourceAddress->GetAddressLength() > sizeof(socketOption.groupSourceReq.gsr_source)))
  //    {
  //      result = E_FAIL;
  //    }

  //    if (SUCCEEDED(result))
  //    {
  //      socketOption.groupSourceReq.gsr_interface = interfaceId;
  //      memcpy(&socketOption.groupSourceReq.gsr_source, this->sourceAddress->GetAddress(), this->sourceAddress->GetAddressLength());
  //      memcpy(&socketOption.groupSourceReq.gsr_group, this->multicastAddress->GetAddress(), this->multicastAddress->GetAddressLength());
  //      socketOptionLength = sizeof(socketOption.groupSourceReq);
  //    }
  //  }
  //  else
  //  {
  //    if (this->multicastAddress->GetAddressLength() > sizeof(socketOption.groupReq.gr_group))
  //    {
  //      result = E_FAIL;
  //    }

  //    if (SUCCEEDED(result))
  //    {
  //      socketOption.groupReq.gr_interface = interfaceId;
  //      memcpy(&socketOption.groupReq.gr_group, this->multicastAddress->GetAddress(), this->multicastAddress->GetAddressLength());
  //      socketOptionLength = sizeof(socketOption.groupReq);
  //    }
  //  }

  //  if (SUCCEEDED(result))
  //  {
  //    if (FAILED(this->SetOption(level, (this->sourceAddress != NULL) ? MCAST_LEAVE_SOURCE_GROUP : MCAST_LEAVE_GROUP, (char *)&socketOption, socketOptionLength)))
  //    {
  //      // unsubscribe from multicast group was not successful, fallback to IPv-specific APIs

  //      if (this->multicastAddress->IsIPv4())
  //      {
  //        result = this->LeaveMulticastGroupIPv4();
  //      }
  //      else if (this->multicastAddress->IsIPv6())
  //      {
  //        result = this->LeaveMulticastGroupIPv6();
  //      }
  //      else
  //      {
  //        result = E_FAIL;
  //      }
  //    }
  //  }
  //}

  //this->flags &= ~MULTICAST_UDP_SOCKET_CONTEXT_FLAG_SUBSCRIBED_TO_GROUP;
  //this->flags |= FAILED(result) ? MULTICAST_UDP_SOCKET_CONTEXT_FLAG_SUBSCRIBED_TO_GROUP : MULTICAST_UDP_SOCKET_CONTEXT_FLAG_NONE;
  //return result;
}

HRESULT CMulticastUdpRawSocketContext::JoinMulticastGroupIPv4(void)
{
  return E_NOTIMPL;

  /*HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->networkInterface);

  CHECK_CONDITION_HRESULT(result, this->multicastAddress->IsIPv4(), result, E_INVALIDARG);
  CHECK_CONDITION_HRESULT(result, (this->sourceAddress == NULL) || ((this->sourceAddress != NULL) && (this->sourceAddress->IsIPv4())), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    union
    {
      struct ip_mreq gr4;
      struct ip_mreq_source gsr4;
    } socketOption;
    socklen_t socketOptionLength;

    CIpAddress *networkInterfaceIpAddress = NULL;
    for (unsigned int i = 0; i < networkInterface->GetUnicastAddresses()->Count(); i++)
    {
      CIpAddress *address = networkInterface->GetUnicastAddresses()->GetItem(i);

      if (address->IsIPv4())
      {
        networkInterfaceIpAddress = address;
        break;
      }
    }
    CHECK_POINTER_HRESULT(result, networkInterfaceIpAddress, result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      memset(&socketOption, 0, sizeof(socketOption));

      if (this->sourceAddress != NULL)
      {
        socketOption.gsr4.imr_multiaddr = this->multicastAddress->GetAddressIPv4()->sin_addr;
        socketOption.gsr4.imr_sourceaddr = this->sourceAddress->GetAddressIPv4()->sin_addr;
        socketOption.gsr4.imr_interface = networkInterfaceIpAddress->GetAddressIPv4()->sin_addr;
        socketOptionLength = sizeof(socketOption.gsr4);
      }
      else
      {
        socketOption.gr4.imr_multiaddr = this->multicastAddress->GetAddressIPv4()->sin_addr;
        socketOption.gr4.imr_interface = networkInterfaceIpAddress->GetAddressIPv4()->sin_addr;
        socketOptionLength = sizeof(socketOption.gr4);
      }

      result = this->SetOption(IPPROTO_IP, (this->sourceAddress != NULL) ? IP_ADD_SOURCE_MEMBERSHIP : IP_ADD_MEMBERSHIP, (char *)&socketOption, socketOptionLength);
    }
  }

  return result;*/
}

HRESULT CMulticastUdpRawSocketContext::JoinMulticastGroupIPv6(void)
{
  return E_NOTIMPL;

  /*HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->networkInterface);

  CHECK_CONDITION_HRESULT(result, this->multicastAddress->IsIPv6(), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    struct ipv6_mreq gr6;
    memset(&gr6, 0, sizeof(gr6));
    memcpy(&gr6.ipv6mr_multiaddr, &this->multicastAddress->GetAddressIPv6()->sin6_addr, 16);
    gr6.ipv6mr_interface = this->networkInterface->GetIpv6Index();

    result = this->SetOption(IPPROTO_IPV6, IPV6_JOIN_GROUP, (char *)&gr6, sizeof(gr6));
  }

  return result;*/
}

HRESULT CMulticastUdpRawSocketContext::LeaveMulticastGroupIPv4(void)
{
  return E_NOTIMPL;

  /*HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->networkInterface);

  CHECK_CONDITION_HRESULT(result, this->multicastAddress->IsIPv4(), result, E_INVALIDARG);
  CHECK_CONDITION_HRESULT(result, (this->sourceAddress == NULL) || ((this->sourceAddress != NULL) && (this->sourceAddress->IsIPv4())), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    union
    {
      struct ip_mreq gr4;
      struct ip_mreq_source gsr4;
    } socketOption;
    socklen_t socketOptionLength;

    CIpAddress *networkInterfaceIpAddress = NULL;
    for (unsigned int i = 0; i < networkInterface->GetUnicastAddresses()->Count(); i++)
    {
      CIpAddress *address = networkInterface->GetUnicastAddresses()->GetItem(i);

      if (address->IsIPv4())
      {
        networkInterfaceIpAddress = address;
        break;
      }
    }
    CHECK_POINTER_HRESULT(result, networkInterfaceIpAddress, result, E_INVALIDARG);

    if (SUCCEEDED(result))
    {
      memset(&socketOption, 0, sizeof(socketOption));

      if (this->sourceAddress != NULL)
      {
        socketOption.gsr4.imr_multiaddr = this->multicastAddress->GetAddressIPv4()->sin_addr;
        socketOption.gsr4.imr_sourceaddr = this->sourceAddress->GetAddressIPv4()->sin_addr;
        socketOption.gsr4.imr_interface = networkInterfaceIpAddress->GetAddressIPv4()->sin_addr;
        socketOptionLength = sizeof(socketOption.gsr4);
      }
      else
      {
        socketOption.gr4.imr_multiaddr = this->multicastAddress->GetAddressIPv4()->sin_addr;
        socketOption.gr4.imr_interface = networkInterfaceIpAddress->GetAddressIPv4()->sin_addr;
        socketOptionLength = sizeof(socketOption.gr4);
      }

      result = this->SetOption(IPPROTO_IP, (this->sourceAddress != NULL) ? IP_DROP_SOURCE_MEMBERSHIP : IP_DROP_MEMBERSHIP, (char *)&socketOption, socketOptionLength);
    }
  }

  return result;*/
}

HRESULT CMulticastUdpRawSocketContext::LeaveMulticastGroupIPv6(void)
{
  return E_NOTIMPL;

  /*HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  CHECK_POINTER_DEFAULT_HRESULT(result, this->networkInterface);

  CHECK_CONDITION_HRESULT(result, this->multicastAddress->IsIPv6(), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    struct ipv6_mreq gr6;
    memset(&gr6, 0, sizeof(gr6));
    memcpy(&gr6.ipv6mr_multiaddr, &this->multicastAddress->GetAddressIPv6()->sin6_addr, 16);
    gr6.ipv6mr_interface = this->networkInterface->GetIpv6Index();

    result = this->SetOption(IPPROTO_IPV6, IPV6_LEAVE_GROUP, (char *)&gr6, sizeof(gr6));
  }

  return result;*/
}

/* protected methods */

uint16_t CMulticastUdpRawSocketContext::CalculateChecksum(uint8_t *payload, uint16_t length)
{
  uint32_t result = 0;

  if ((payload != NULL) && (length > 0) && ((length % 2) == 0))
  {
    for (uint16_t i = 0; i < length; i++)
    {
      result += ((*(payload + i)) << 8) + (*(payload + i + 1));
      i++;
    }

    while (result > 0x0000FFFF)
    {
      uint16_t carry = (result >> 16);

      result &= 0x0000FFFF;
      result += carry;
    }
  }

  return (uint16_t)(~result);
}