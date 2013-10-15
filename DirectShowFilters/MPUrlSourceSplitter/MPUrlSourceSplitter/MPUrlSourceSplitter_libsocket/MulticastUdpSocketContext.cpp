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

#include "MulticastUdpSocketContext.h"

CMulticastUdpSocketContext::CMulticastUdpSocketContext(CIpAddress *multicastAddress, CIpAddress *sourceAddress)
  : CUdpSocketContext()
{
  this->multicastAddress = (multicastAddress != NULL) ? multicastAddress->Clone() : NULL;
  this->sourceAddress = (sourceAddress != NULL) ? sourceAddress->Clone() : NULL;
}

CMulticastUdpSocketContext::CMulticastUdpSocketContext(CIpAddress *multicastAddress, CIpAddress *sourceAddress, SOCKET socket)
  : CUdpSocketContext(socket)
{
  this->multicastAddress = (multicastAddress != NULL) ? multicastAddress->Clone() : NULL;
  this->sourceAddress = (sourceAddress != NULL) ? sourceAddress->Clone() : NULL;
}


CMulticastUdpSocketContext::~CMulticastUdpSocketContext(void)
{
  FREE_MEM_CLASS(this->multicastAddress);
  FREE_MEM_CLASS(this->sourceAddress);
}

/* get methods */

/* set methods */

/* other methods */

HRESULT CMulticastUdpSocketContext::SubscribeToMulticastGroup(void)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  unsigned int interfaceId = 0;
  unsigned int level = 0;

  if (SUCCEEDED(result))
  {
    switch (this->multicastAddress->GetFamily())
    {
    case AF_INET:
      //interfaceId = (networkInterface != NULL) ? networkInterface->GetIpv4Index() : 0;
      level = IPPROTO_IP;
      break;
    case AF_INET6:
      //interfaceId = (networkInterface != NULL) ? networkInterface->GetIpv6Index() : 0;
      level = IPPROTO_IPV6;
      break;
    default:
      result = E_FAIL;
    }
  }

  if (SUCCEEDED(result))
  {
    union
    {
      struct group_req groupReq;
      struct group_source_req groupSourceReq;
    } socketOption;
    socklen_t socketOptionLength;

    memset(&socketOption, 0, sizeof(socketOption));

    if (this->sourceAddress != NULL)
    {
      if ((this->multicastAddress->GetAddressLength() > sizeof (socketOption.groupSourceReq.gsr_group)) || (this->sourceAddress->GetAddressLength() > sizeof (socketOption.groupSourceReq.gsr_source)))
      {
        result = E_FAIL;
      }

      if (SUCCEEDED(result))
      {
        socketOption.groupSourceReq.gsr_interface = interfaceId;
        memcpy(&socketOption.groupSourceReq.gsr_source, this->sourceAddress->GetAddress(), this->sourceAddress->GetAddressLength());
        memcpy(&socketOption.groupSourceReq.gsr_group, this->multicastAddress->GetAddress(), this->multicastAddress->GetAddressLength());
        socketOptionLength = sizeof(socketOption.groupSourceReq);
      }
    }
    else
    {
      if (this->multicastAddress->GetAddressLength() > sizeof (socketOption.groupReq.gr_group))
      {
        result = E_FAIL;
      }

      if (SUCCEEDED(result))
      {
        socketOption.groupReq.gr_interface = interfaceId;
        memcpy(&socketOption.groupReq.gr_group, this->multicastAddress->GetAddress(), this->multicastAddress->GetAddressLength());
        socketOptionLength = sizeof(socketOption.groupReq);
      }
    }

    if (SUCCEEDED(result))
    {
      if (FAILED(this->SetOption(level, (this->sourceAddress != NULL) ? MCAST_JOIN_SOURCE_GROUP : MCAST_JOIN_GROUP, (char *)&socketOption, socketOptionLength)))
      {
        // subscribe to multicast group was not successful, fallback to IPv-specific APIs

        if (this->multicastAddress->IsIPv4())
        {
          result = this->JoinMulticastGroupIPv4();
        }
        else if (this->multicastAddress->IsIPv6())
        {
          result = this->JoinMulticastGroupIPv6();
        }
        else
        {
          result = E_FAIL;
        }
      }
    }
  }

  return result;
}

HRESULT CMulticastUdpSocketContext::JoinMulticastGroupIPv4(void)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
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

    //struct in_addr id;

    /*if (networkInterface != NULL)
    {
      id = ((sockaddr_in *)networkInterface->ai_addr)->sin_addr;
    }
    else
    {
      id.s_addr = INADDR_ANY;
    }*/

    memset(&socketOption, 0, sizeof(socketOption));

    if (this->sourceAddress != NULL)
    {
      socketOption.gsr4.imr_multiaddr = this->multicastAddress->GetAddressIPv4()->sin_addr;
      socketOption.gsr4.imr_sourceaddr = this->sourceAddress->GetAddressIPv4()->sin_addr;
      //socketOption.gsr4.imr_interface = id;
      socketOptionLength = sizeof (socketOption.gsr4);
    }
    else
    {
      socketOption.gr4.imr_multiaddr = this->multicastAddress->GetAddressIPv4()->sin_addr;
      //socketOption.gr4.imr_interface = id;
      socketOptionLength = sizeof(socketOption.gr4);
    }

    result = this->SetOption(IPPROTO_IP, (this->sourceAddress != NULL) ? IP_ADD_SOURCE_MEMBERSHIP : IP_ADD_MEMBERSHIP, (char *)&socketOption, socketOptionLength);
  }

  return result;
}

HRESULT CMulticastUdpSocketContext::JoinMulticastGroupIPv6(void)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, this->multicastAddress);
  CHECK_CONDITION_HRESULT(result, this->multicastAddress->IsIPv6(), result, E_INVALIDARG);

  if (SUCCEEDED(result))
  {
    struct ipv6_mreq gr6;
    memset(&gr6, 0, sizeof(gr6));
    gr6.ipv6mr_interface = this->multicastAddress->GetAddressIPv6()->sin6_scope_id;
    memcpy(&gr6.ipv6mr_multiaddr, &this->multicastAddress->GetAddressIPv6()->sin6_addr, 16);

    result = this->SetOption(IPPROTO_IPV6, IPV6_JOIN_GROUP, (char *)&gr6, sizeof(gr6));
  }

  return result;
}