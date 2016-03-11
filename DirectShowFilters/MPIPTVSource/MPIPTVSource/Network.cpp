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

#include "Network.h"
#include "Logger.h"
#include "Utilities.h"
#include "ProtocolInterface.h"

#include <stdio.h>
#include <IPHlpApi.h>

#define METHOD_GET_INTERFACE_ADDRESS_NAME                                         _T("GetInterfaceAddress()")
#define METHOD_GET_INTERFACE_ID_NAME                                              _T("GetInterfaceId()")
#define METHOD_JOIN_MULTICAST_GROUP_IPV4_NAME                                     _T("JoinMulticastGroupIPv4()")
#define METHOD_JOIN_MULTICAST_GROUP_IPV6_NAME                                     _T("JoinMulticastGroupIPv6()")
#define METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME                                  _T("SubscribeToMulticastGroup()")
#define METHOD_LEAVE_MULTICAST_GROUP_IPV4_NAME                                    _T("LeaveMulticastGroupIPv4()")
#define METHOD_LEAVE_MULTICAST_GROUP_IPV6_NAME                                    _T("LeaveMulticastGroupIPv6()")
#define METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME                              _T("UnsubscribeFromMulticastGroup()")

MPIPTVSOURCE_API int GetIpAddress(const TCHAR *serverName, WORD port, ADDRINFOT **address, const ADDRINFOT *pHints)
{
  ADDRINFOT hints;
  ADDRINFOT *result = NULL;

  TCHAR service[6];
  _sntprintf_s(service, 6 * sizeof(TCHAR), 6, _T("%d"), port);

  // setup the hints address info structure
  // which is passed to the getaddrinfo() function
  ZeroMemory(&hints, sizeof(hints));
  if (pHints != NULL)
  {
    const int safe_flags =
      AI_PASSIVE |
      AI_CANONNAME |
      AI_NUMERICHOST |
      AI_NUMERICSERV |
#ifdef AI_ALL
      AI_ALL |
#endif
#ifdef AI_ADDRCONFIG
      AI_ADDRCONFIG |
#endif
#ifdef AI_V4MAPPED
      AI_V4MAPPED |
#endif
      0;

    hints.ai_family = pHints->ai_family;
    hints.ai_socktype = pHints->ai_socktype;
    hints.ai_protocol = pHints->ai_protocol;

    // unfortunately, some flags change the layout of struct addrinfo, so
    // they cannot be copied blindly from p_hints to &hints. Therefore, we
    // only copy flags that we know for sure are "safe".
    hints.ai_flags = pHints->ai_flags & safe_flags;
  }

  // we only ever use port *numbers*
  hints.ai_flags |= AI_NUMERICSERV;

  int retval = 0;
  if ((hints.ai_flags & AI_NUMERICHOST) == 0)
  {
    hints.ai_flags |= AI_NUMERICHOST;
    retval = GetAddrInfo(serverName, service, &hints, &result);
    if (retval != 0)
    {
      hints.ai_flags &= ~AI_NUMERICHOST;
    }
  } 

  retval = GetAddrInfo(serverName, service, &hints, &result);
  if (retval == 0)
  {
    *address = result;
  }
  return retval;
}

MPIPTVSOURCE_API bool SockAddrIsMulticast(const ADDRINFOT *addr)
{
  switch (addr->ai_addr->sa_family)
  {
  case AF_INET:
    {
      const struct sockaddr_in *v4 = (const struct sockaddr_in *)addr->ai_addr;
      return (IN_MULTICAST(ntohl (v4->sin_addr.s_addr)) != 0);
    }
  case AF_INET6:
    {
      const struct sockaddr_in6 *v6 = (const struct sockaddr_in6 *)addr->ai_addr;
      return (IN6_IS_ADDR_MULTICAST(&v6->sin6_addr) != 0);
    }
  }

  return false;
}

MPIPTVSOURCE_API TCHAR *GetInterfaceAddress(CLogger *logger, const TCHAR *protocolName, const TCHAR *interfaceName, int family)
{
  logger->Log(LOGGER_INFO, METHOD_START_FORMAT, protocolName, METHOD_GET_INTERFACE_ADDRESS_NAME);

  ULONG bufferLen = 0;
  ULONG flags = GAA_FLAG_INCLUDE_PREFIX;
  DWORD retval = 0;
  PIP_ADAPTER_ADDRESSES pAddresses = NULL;
  PIP_ADAPTER_ADDRESSES pCurrAddresses = NULL;
  PIP_ADAPTER_UNICAST_ADDRESS pUnicast = NULL;
  TCHAR *result = NULL;
  BOOL error = FALSE;

  if (GetAdaptersAddresses(family, flags, NULL, pAddresses, &bufferLen) == ERROR_BUFFER_OVERFLOW)
  {
    pAddresses = (PIP_ADAPTER_ADDRESSES)CoTaskMemAlloc(bufferLen);
    if (pAddresses == NULL)
    {
      logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_GET_INTERFACE_ADDRESS_NAME, _T("not enough memory for interface addresses"));
      error = TRUE;
    }
    else
    {
      retval = GetAdaptersAddresses(family, flags, NULL, pAddresses, &bufferLen);
    }
  }

  if (!error)
  {
    error |= (retval != NO_ERROR);

    if (!error)
    {
      // if successful, output some information from the data we received
      pCurrAddresses = pAddresses;
      while (pCurrAddresses != NULL)
      {
        error = FALSE;

        // unify interface friendly name
#ifdef _MBCS
        TCHAR *friendlyName = ConvertToMultiByteW(pCurrAddresses->FriendlyName);
#else
        TCHAR *friendlyName = ConvertToUnicodeW(pCurrAddresses->FriendlyName);
#endif
        if (friendlyName == NULL)
        {
          logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_GET_INTERFACE_ADDRESS_NAME, _T("cannot convert interface friendly name"));
          error = TRUE;
        }

        if (!error)
        {
          if (_tcsicmp(friendlyName, interfaceName) == 0)
          {
            // if we found correct interface by name
            pUnicast = pCurrAddresses->FirstUnicastAddress;

            // interface should have only one unicast address
            if (pUnicast != NULL)
            {
              // allocate enough memory for 256 characters
              DWORD resultLength = 256;

              result = ALLOC_MEM_SET(result, TCHAR, resultLength, 0);
              if (result == NULL)
              {
                logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_GET_INTERFACE_ADDRESS_NAME, _T("not enough memory for result"));
                error = TRUE;
              }

              if (!error)
              {
                if (WSAAddressToString(pUnicast->Address.lpSockaddr, pUnicast->Address.iSockaddrLength, NULL, result, &resultLength) != 0)
                {
                  logger->Log(LOGGER_WARNING, _T("%s: %s: cannot acquire address for interface '%s', error: %d"), protocolName, METHOD_GET_INTERFACE_ADDRESS_NAME, friendlyName, WSAGetLastError());
                  FREE_MEM(result);
                  error = TRUE;
                }
                else
                {
                  // interface address successfully found
                  pCurrAddresses = NULL;
                }
              }
            }
          }
        }

        FREE_MEM(friendlyName);
        if (pCurrAddresses != NULL)
        {
          pCurrAddresses = pCurrAddresses->Next;
        }
      }
    }
    else
    {
      logger->Log(LOGGER_ERROR, _T("%s: %s: GetAdaptersAddresses() returned error: %d"), protocolName, METHOD_GET_INTERFACE_ADDRESS_NAME, retval);
    }
  }

  FREE_MEM(pAddresses);
  logger->Log(LOGGER_INFO, (result != NULL) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_GET_INTERFACE_ADDRESS_NAME);
  return result;
}

MPIPTVSOURCE_API ULONG GetInterfaceId(CLogger *logger, const TCHAR *protocolName, const TCHAR *interfaceName, int family)
{
  logger->Log(LOGGER_INFO, METHOD_START_FORMAT, protocolName, METHOD_GET_INTERFACE_ID_NAME);

  ULONG bufferLen = 0;
  ULONG flags = GAA_FLAG_INCLUDE_PREFIX;
  DWORD retval = 0;
  PIP_ADAPTER_ADDRESSES pAddresses = NULL;
  PIP_ADAPTER_ADDRESSES pCurrAddresses = NULL;
  PIP_ADAPTER_UNICAST_ADDRESS pUnicast = NULL;
  ULONG result = 0;
  bool error = false;

  if (GetAdaptersAddresses(family, flags, NULL, pAddresses, &bufferLen) == ERROR_BUFFER_OVERFLOW)
  {
    pAddresses = (PIP_ADAPTER_ADDRESSES)CoTaskMemAlloc(bufferLen);
    if (pAddresses == NULL)
    {
      logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_GET_INTERFACE_ID_NAME, _T("not enough memory for interface addresses"));
      error = true;
    }
    else
    {
      retval = GetAdaptersAddresses(family, flags, NULL, pAddresses, &bufferLen);
      if (retval != NO_ERROR)
      {
        logger->Log(LOGGER_VERBOSE, _T("%s: %s: GetAdaptersAddresses() error: %u"), protocolName, METHOD_GET_INTERFACE_ID_NAME, retval);
        error = true;
      }
    }
  }

  if (!error)
  {
    // if successful, output some information from the data we received
    pCurrAddresses = pAddresses;
    while (pCurrAddresses != NULL)
    {
      result = 0;
      // unify interface friendly name
#ifdef _MBCS
      TCHAR *friendlyName = ConvertToMultiByteW(pCurrAddresses->FriendlyName);
#else
      TCHAR *friendlyName = ConvertToUnicodeW(pCurrAddresses->FriendlyName);
#endif
      if (friendlyName == NULL)
      {
        logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_GET_INTERFACE_ID_NAME, _T("cannot convert interface friendly name"));
      }
        
      if (friendlyName != NULL)
      {
        if (_tcsicmp(friendlyName, interfaceName) == 0)
        {
          // if we found correct interface by name

          switch(family)
          {
          case AF_INET:
            result = pCurrAddresses->IfIndex;
            break;
          case AF_INET6:
            result = pCurrAddresses->Ipv6IfIndex;
            break;
          }
        }
      }

      FREE_MEM(friendlyName);
      pCurrAddresses = (result != 0) ? NULL : pCurrAddresses->Next;
    }
  }

  FREE_MEM(pAddresses);
  logger->Log(LOGGER_INFO, _T("%s: %s: interface ID: %u"), protocolName, METHOD_GET_INTERFACE_ID_NAME, result);
  logger->Log(LOGGER_INFO, (result != 0) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_GET_INTERFACE_ID_NAME);
  return result;
}

MPIPTVSOURCE_API int JoinMulticastGroupIPv4(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in *source, const struct sockaddr_in *local, CParameterCollection *parameters)
{
  int result = 0;
  ADDRINFOT *networkInterface = NULL;
  ADDRINFOT hints;
  hints.ai_family = local->sin_family;
  hints.ai_socktype = SOCK_DGRAM;
  hints.ai_protocol = IPPROTO_UDP;
  hints.ai_flags = AI_PASSIVE;

  if (parameters != NULL)
  {
    // we have set some parameters
    // get interface parameter
    PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
    if (networkInterfaceParameter != NULL)
    {
      TCHAR *networkInterfaceAddress = GetInterfaceAddress(logger, protocolName, networkInterfaceParameter->GetValue(), local->sin_family);
      int errorCode = GetIpAddress(networkInterfaceAddress, 0, &networkInterface, &hints);
      if (errorCode != 0)
      {
        logger->Log(LOGGER_ERROR, _T("%s: %s: getaddrinfo() for network interface error: %d"), protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV4_NAME, errorCode);
        result = -1;
      }
      if (result == 0)
      {
        logger->Log(LOGGER_INFO, _T("%s: %s: try to bind on interface '%s', address: %s"), protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV4_NAME, networkInterfaceParameter->GetValue(), networkInterfaceAddress);
      }
      FREE_MEM(networkInterfaceAddress);
    }
  }

  if (result == 0)
  {
    union
    {
      struct ip_mreq gr4;
      struct ip_mreq_source gsr4;
    } opt;
    int cmd;
    struct in_addr id;

    if (networkInterface != NULL)
    {
      id = ((sockaddr_in *)networkInterface->ai_addr)->sin_addr;
    }
    else
    {
      id.s_addr = INADDR_ANY;
    }
    socklen_t optlen;

    memset(&opt, 0, sizeof(opt));
    if (source != NULL)
    {
      cmd = IP_ADD_SOURCE_MEMBERSHIP;
      opt.gsr4.imr_multiaddr = local->sin_addr;
      opt.gsr4.imr_sourceaddr = source->sin_addr;
      opt.gsr4.imr_interface = id;
      optlen = sizeof (opt.gsr4);
    }
    else
    {
      cmd = IP_ADD_MEMBERSHIP;
      opt.gr4.imr_multiaddr = local->sin_addr;
      opt.gr4.imr_interface = id;
      optlen = sizeof(opt.gr4);
    }

    logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV4_NAME,  (source != NULL) ? _T("IP_ADD_SOURCE_MEMBERSHIP multicast request") : _T("IP_ADD_MEMBERSHIP multicast request"));
    
    result = (SetSocketOption(logger, protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV4_NAME, (source != NULL) ? _T("IP_ADD_SOURCE_MEMBERSHIP") : _T("IP_ADD_MEMBERSHIP"), m_socket, IPPROTO_IP, cmd, (char *)&opt, optlen) == 0) ? 0 : (-1);
  }

  logger->Log(LOGGER_INFO, (result == 0) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV4_NAME);
  return result;
}

MPIPTVSOURCE_API int JoinMulticastGroupIPv6(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in6 *local, CParameterCollection *parameters)
{
  int result = 0;
  struct ipv6_mreq gr6;
  memset(&gr6, 0, sizeof(gr6));
  gr6.ipv6mr_interface = local->sin6_scope_id;
  memcpy(&gr6.ipv6mr_multiaddr, &local->sin6_addr, 16);

  if (parameters != NULL)
  {
    // we have set some parameters
    // get interface parameter
    PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
    if (networkInterfaceParameter != NULL)
    {
      ULONG interfaceId = GetInterfaceId(logger, protocolName, networkInterfaceParameter->GetValue(), local->sin6_family);
      result = (interfaceId != 0) ? 0 : (-1);
      if (result == 0)
      {
        gr6.ipv6mr_interface = interfaceId;
        logger->Log(LOGGER_INFO, _T("%s: %s: try to bind on interface '%s', id: %u"), protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV6_NAME, networkInterfaceParameter->GetValue(), gr6.ipv6mr_interface);
      }
      else
      {
        logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV6_NAME, _T("cannot get interface ID"));
      }
    }
  }

  logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV6_NAME, _T("IPV6_JOIN_GROUP multicast request"));
  result = (SetSocketOption(logger, protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV6_NAME, _T("IPV6_JOIN_GROUP"), m_socket, IPPROTO_IPV6, IPV6_JOIN_GROUP, (char *)&gr6, sizeof(gr6)) == 0) ? 0 : (-1);

  logger->Log(LOGGER_INFO, (result == 0) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_JOIN_MULTICAST_GROUP_IPV6_NAME);
  return result;
}

MPIPTVSOURCE_API int SubscribeToMulticastGroup(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr *source, socklen_t sourceLen, const struct sockaddr *local, socklen_t localLen, CParameterCollection *parameters)
{
  int result = 0;
  int level;
  ULONG iid = 0;

  if (parameters != NULL)
  {
    // we have set some parameters
    // get interface parameter
    PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
    if (networkInterfaceParameter != NULL)
    {
      ULONG interfaceId = GetInterfaceId(logger, protocolName, networkInterfaceParameter->GetValue(), local->sa_family);
      result = (interfaceId != 0) ? 0 :(-1);

      if (result == 0)
      {
        iid = interfaceId;
        logger->Log(LOGGER_INFO, _T("%s: %s: try to bind on interface '%s', id: %u"), protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, networkInterfaceParameter->GetValue(), iid);
      }
      else
      {
        logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, _T("cannot get interface ID"));
      }
    }
  }

  if (result == 0)
  {
    switch (local->sa_family)
    {
    case AF_INET6:
      level = IPPROTO_IPV6;
      break;
    case AF_INET:
      level = IPPROTO_IP;
      break;
    default:
      logger->Log(LOGGER_ERROR, _T("%s: %s: not supported internet family: %d"), protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, local->sa_family);
      result = -1;
    }
  }

  if (result == 0)
  {
    if (source != NULL)
    {
      switch (source->sa_family)
      {
      case AF_INET6:
        if (memcmp(&((const struct sockaddr_in6 *)source)->sin6_addr, &in6addr_any, sizeof(in6addr_any)) == 0)
        {
          source = NULL;
        }
        break;
      case AF_INET:
        if (((const struct sockaddr_in *)source)->sin_addr.s_addr == INADDR_ANY)
        {
          source = NULL;
        }
        break;
      }
    }

    union
    {
      struct group_req gr;
      struct group_source_req gsr;
    } opt;
    socklen_t optlen;

    memset(&opt, 0, sizeof (opt));
    if (source != NULL)
    {
      if ((localLen > sizeof (opt.gsr.gsr_group)) || (sourceLen > sizeof (opt.gsr.gsr_source)))
      {
        result = -1;
      }

      if (result == 0)
      {
        opt.gsr.gsr_interface = iid;
        memcpy(&opt.gsr.gsr_source, source, sourceLen);
        memcpy(&opt.gsr.gsr_group, local, localLen);
        optlen = sizeof(opt.gsr);
      }
    }
    else
    {
      if (localLen > sizeof (opt.gr.gr_group))
      {
        result = -1;
      }

      if (result == 0)
      {
        opt.gr.gr_interface = iid;
        memcpy(&opt.gr.gr_group, local, localLen);
        optlen = sizeof(opt.gr);
      }
    }

    if (result == 0)
    {
      logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, (source != NULL) ? _T("multicast source group join request") : _T("multicast group join request"));

      result = (SetSocketOption(logger, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, (source != NULL) ? _T("MCAST_JOIN_SOURCE_GROUP") : _T("MCAST_JOIN_GROUP"), m_socket, level, (source != NULL) ? MCAST_JOIN_SOURCE_GROUP : MCAST_JOIN_GROUP, (char *)&opt, optlen)  == 0) ? 0 : (-1);

      if (result != 0)
      {
        result = 0;
        // subscribe to multicast group was not successful
        logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, (source != NULL) ? _T("multicast source group join request not successful") : _T("multicast group join request not successful"));

        // fallback to IPv-specific APIs
        if ((source != NULL) && (source->sa_family != local->sa_family))
        {
          logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, _T("source and local internet address family not same"));
          result = -1;
        }

        if (result == 0)
        {
          switch (local->sa_family)
          {
          case AF_INET:
            if ((localLen < sizeof (struct sockaddr_in)) || ((source != NULL) && (sourceLen < sizeof (struct sockaddr_in))))
            {
              result = -1;
            }

            if (result == 0)
            {
              result = JoinMulticastGroupIPv4(logger, protocolName, m_socket, (const struct sockaddr_in *)source, (const struct sockaddr_in *)local, parameters);
            }
            break;
          case AF_INET6:
            if ((localLen < sizeof (struct sockaddr_in6)) || ((source != NULL) && (sourceLen < sizeof (struct sockaddr_in6))))
            {
              result = -1;
            }

            if (result == 0)
            {
              result = JoinMulticastGroupIPv6(logger, protocolName, m_socket, (const struct sockaddr_in6 *)local, parameters);
            }
            break;
          }

          if (result != 0)
          {
            logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, _T("multicast group join error"));
            if (source != NULL)
            {
              logger->Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME, _T("trying another join method"));
              result = SubscribeToMulticastGroup(logger, protocolName, m_socket, NULL, 0, local, localLen, parameters);
            }
          }
        }
      }
    }
  }
  logger->Log(LOGGER_INFO, (result == 0) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_SUBSCRIBE_TO_MULTICAST_GROUP_NAME);
  return result;
}

MPIPTVSOURCE_API int LeaveMulticastGroupIPv4(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in *source, const struct sockaddr_in *local, CParameterCollection *parameters)
{
  ADDRINFOT *networkInterface = NULL;
  ADDRINFOT hints;
  hints.ai_family = local->sin_family;
  hints.ai_socktype = SOCK_DGRAM;
  hints.ai_protocol = IPPROTO_UDP;
  hints.ai_flags = AI_PASSIVE;
  int result = 0;

  if (parameters != NULL)
  {
    // we have set some parameters
    // get interface parameter
    PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
    if (networkInterfaceParameter != NULL)
    {
      TCHAR *networkInterfaceAddress = GetInterfaceAddress(logger, protocolName, networkInterfaceParameter->GetValue(), local->sin_family);
      int errorCode = GetIpAddress(networkInterfaceAddress, 0, &networkInterface, &hints);
      if (errorCode != 0)
      {
        logger->Log(LOGGER_ERROR, _T("%s: %s: getaddrinfo() for network interface error: %d"), protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV4_NAME, errorCode);
        result = -1;
      }
      if (result == 0)
      {
        logger->Log(LOGGER_INFO, _T("%s: %s: try to bind on interface '%s', address: %s"), protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV4_NAME, networkInterfaceParameter->GetValue(), networkInterfaceAddress);
      }
      FREE_MEM(networkInterfaceAddress);
    }
  }

  if (result == 0)
  {
    union
    {
      struct ip_mreq gr4;
      struct ip_mreq_source gsr4;
    } opt;
    int cmd;
    struct in_addr id;

    if (networkInterface != NULL)
    {
      id = ((sockaddr_in *)networkInterface->ai_addr)->sin_addr;
    }
    else
    {
      id.s_addr = INADDR_ANY;
    }
    socklen_t optlen;

    memset(&opt, 0, sizeof(opt));
    if (source != NULL)
    {
      cmd = IP_DROP_SOURCE_MEMBERSHIP;
      opt.gsr4.imr_multiaddr = local->sin_addr;
      opt.gsr4.imr_sourceaddr = source->sin_addr;
      opt.gsr4.imr_interface = id;
      optlen = sizeof (opt.gsr4);
    }
    else
    {
      cmd = IP_DROP_MEMBERSHIP;
      opt.gr4.imr_multiaddr = local->sin_addr;
      opt.gr4.imr_interface = id;
      optlen = sizeof(opt.gr4);
    }

    logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV4_NAME, (source != NULL) ? _T("IP_DROP_SOURCE_MEMBERSHIP multicast request") : _T("IP_DROP_MEMBERSHIP multicast request"));
    result = (SetSocketOption(logger, protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV4_NAME, (source != NULL) ? _T("IP_DROP_SOURCE_MEMBERSHIP") : _T("IP_DROP_MEMBERSHIP"), m_socket, IPPROTO_IP, cmd, (char *)&opt, optlen) == 0) ? 0 : (-1);
  }

  if (networkInterface != NULL)
  {
    FreeAddrInfo(networkInterface);
    networkInterface = NULL;
  }

  logger->Log(LOGGER_INFO, (result == 0) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV4_NAME);
  return result;
}

MPIPTVSOURCE_API int LeaveMulticastGroupIPv6(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr_in6 *local, CParameterCollection *parameters)
{
  int result = 0;
  struct ipv6_mreq gr6;
  memset(&gr6, 0, sizeof(gr6));
  gr6.ipv6mr_interface = local->sin6_scope_id;
  memcpy(&gr6.ipv6mr_multiaddr, &local->sin6_addr, 16);

  if (parameters != NULL)
  {
    // we have set some parameters
    // get interface parameter
    PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
    if (networkInterfaceParameter != NULL)
    {
      ULONG interfaceId = GetInterfaceId(logger, protocolName, networkInterfaceParameter->GetValue(), local->sin6_family);
      result = (interfaceId != 0) ? 0 : (-1);

      if (result == 0)
      {
        gr6.ipv6mr_interface = interfaceId;
        logger->Log(LOGGER_INFO, _T("%s: %s: try to bind on interface '%s', id: %u"), protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV6_NAME, networkInterfaceParameter->GetValue(), gr6.ipv6mr_interface);
      }
      else
      {
        logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV6_NAME, _T("cannot get interface ID"));
      }
    }
  }

  if (result == 0)
  {
    logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV6_NAME, _T("IPV6_LEAVE_GROUP multicast request"), protocolName);
    result = (SetSocketOption(logger, protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV6_NAME, _T("IPV6_LEAVE_GROUP"), m_socket, IPPROTO_IPV6, IPV6_LEAVE_GROUP, (char *)&gr6, sizeof(gr6)) == 0) ? 0 : (-1);
  }

  logger->Log(LOGGER_INFO, (result == 0) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_LEAVE_MULTICAST_GROUP_IPV6_NAME);
  return result;
}

MPIPTVSOURCE_API int UnsubscribeFromMulticastGroup(CLogger *logger, const TCHAR *protocolName, SOCKET m_socket, const struct sockaddr *source, socklen_t sourceLen, const struct sockaddr *local, socklen_t localLen, CParameterCollection *parameters)
{
  int result = 0;
  int level;
  ULONG iid = 0;

  if (parameters != NULL)
  {
    // we have set some parameters
    // get interface parameter
    PCParameter networkInterfaceParameter = parameters->GetParameter(INTERFACE_PARAMETER_NAME, true);
    if (networkInterfaceParameter != NULL)
    {
      ULONG interfaceId = GetInterfaceId(logger, protocolName, networkInterfaceParameter->GetValue(), local->sa_family);
      result = (interfaceId != 0) ? 0 : (-1);

      if (result == 0)
      {
        iid = interfaceId;
        logger->Log(LOGGER_INFO, _T("%s: %s: try to bind on interface '%s', id: %u"), protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, networkInterfaceParameter->GetValue(), iid);
      }
      else
      {
        logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, _T("cannot get interface ID"));
      }
    }
  }

  if (result == 0)
  {
    switch (local->sa_family)
    {
    case AF_INET6:
      level = IPPROTO_IPV6;
      break;
    case AF_INET:
      level = IPPROTO_IP;
      break;
    default:
      logger->Log(LOGGER_ERROR, _T("%s: %s: not supported internet family: %d"), protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, local->sa_family);
      result = -1;
    }
  }

  if (result == 0)
  {
    if (source != NULL)
    {
      switch (source->sa_family)
      {
      case AF_INET6:
        if (memcmp(&((const struct sockaddr_in6 *)source)->sin6_addr, &in6addr_any, sizeof(in6addr_any)) == 0)
        {
          source = NULL;
        }
        break;
      case AF_INET:
        if (((const struct sockaddr_in *)source)->sin_addr.s_addr == INADDR_ANY)
        {
          source = NULL;
        }
        break;
      }
    }

    union
    {
      struct group_req gr;
      struct group_source_req gsr;
    } opt;
    socklen_t optlen;

    memset(&opt, 0, sizeof (opt));
    if (source != NULL)
    {
      if ((localLen > sizeof (opt.gsr.gsr_group)) || (sourceLen > sizeof (opt.gsr.gsr_source)))
      {
        result = -1;
      }

      if (result == 0)
      {
        opt.gsr.gsr_interface = iid;
        memcpy(&opt.gsr.gsr_source, source, sourceLen);
        memcpy(&opt.gsr.gsr_group, local, localLen);
        optlen = sizeof(opt.gsr);
      }
    }
    else
    {
      if (localLen > sizeof (opt.gr.gr_group))
      {
        result = -1;
      }

      if (result == 0)
      {
        opt.gr.gr_interface = iid;
        memcpy(&opt.gr.gr_group, local, localLen);
        optlen = sizeof(opt.gr);
      }
    }

    if (result == 0)
    {
      logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, (source != NULL) ? _T("multicast source group leave request") : _T("multicast group leave request"));
      result = (SetSocketOption(logger, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, (source != NULL) ? _T("MCAST_LEAVE_SOURCE_GROUP") : _T("MCAST_LEAVE_GROUP"), m_socket, level, (source != NULL) ? MCAST_LEAVE_SOURCE_GROUP : MCAST_LEAVE_GROUP, (char *)&opt, optlen) == 0) ? 0 : (-1);

      if (result != 0)
      {
        result = 0;
        logger->Log(LOGGER_INFO, METHOD_MESSAGE_FORMAT, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, (source != NULL) ? _T("multicast source group leave request not successful") : _T("multicast group leave request not successful"));

        // fallback to IPv-specific APIs
        if ((source != NULL) && (source->sa_family != local->sa_family))
        {
          logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, _T("source and local internet address family not same"));
          result = -1;
        }

        if (result == 0)
        {
          switch (local->sa_family)
          {
          case AF_INET:
            if ((localLen < sizeof (struct sockaddr_in)) || ((source != NULL) && (sourceLen < sizeof (struct sockaddr_in))))
            {
              result = -1;
            }

            if (result == 0)
            {
              result = LeaveMulticastGroupIPv4(logger, protocolName, m_socket, (const struct sockaddr_in *)source, (const struct sockaddr_in *)local, parameters);
            }
            break;
          case AF_INET6:
            if ((localLen < sizeof (struct sockaddr_in6)) || ((source != NULL) && (sourceLen < sizeof (struct sockaddr_in6))))
            {
              result = -1;
            }

            if (result == 0)
            {
              result = LeaveMulticastGroupIPv6(logger, protocolName, m_socket, (const struct sockaddr_in6 *)local, parameters);
            }
            break;
          }

          if (result != 0)
          {
            logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, _T("multicast group leave error"));
            if (source != NULL)
            {
              logger->Log(LOGGER_WARNING, METHOD_MESSAGE_FORMAT, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME, _T("trying another leave method"));
              result = UnsubscribeFromMulticastGroup(logger, protocolName, m_socket, NULL, 0, local, localLen, parameters);
            }
          }
        }
      }
    }
  }

  logger->Log(LOGGER_INFO, (result == 0) ? METHOD_END_FORMAT : METHOD_END_FAIL_FORMAT, protocolName, METHOD_UNSUBSCRIBE_FROM_MULTICAST_GROUP_NAME);
  return result;
}

MPIPTVSOURCE_API void ZeroURL(URL_COMPONENTS *url) 
{
  url->lpszScheme = NULL;
  url->lpszExtraInfo = NULL;
  url->lpszHostName = NULL;
  url->lpszPassword = NULL;
  url->lpszUrlPath = NULL;
  url->lpszUserName = NULL;

  url->dwSchemeLength = 1;
  url->dwHostNameLength = 1;
  url->dwUrlPathLength = 1;
  url->dwExtraInfoLength = 1;
  url->dwPasswordLength = 1;
  url->dwUrlPathLength = 1;
  url->dwUserNameLength = 1;
}

MPIPTVSOURCE_API SOCKET CreateSocket(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, int family, int type, int protocol)
{
  SOCKET m_socket = socket(family, type, protocol);
  if (m_socket != INVALID_SOCKET)
  {
    logger->Log(LOGGER_VERBOSE, _T("%s: %s: socket: %u"), protocolName, functionName, m_socket);
    switch(family)
    {
    case AF_INET:
      logger->Log(LOGGER_INFO, _T("%s: %s: internet family: %s"), protocolName, functionName, _T("AF_INET"));
      break;
    case AF_INET6:
      logger->Log(LOGGER_INFO, _T("%s: %s: internet family: %s"), protocolName,functionName, _T("AF_INET6"));
      break;
    default:
      logger->Log(LOGGER_INFO, _T("%s: %s: internet family: %d"), protocolName, functionName, family);
      break;
    }
  }
  else
  {
    logger->Log(LOGGER_ERROR, METHOD_MESSAGE_FORMAT, protocolName, functionName, _T("invalid socket"));
  }

  return m_socket;
}

MPIPTVSOURCE_API int SetSocketOption(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *optionName, SOCKET s, int level, int optname, const char *optval, int optlen)
{
  int result = 0;
  if (setsockopt(s, level, optname, optval, optlen) == SOCKET_ERROR)
  {
    result = WSAGetLastError();
    logger->Log(LOGGER_ERROR, _T("%s: %s: setsockopt(%s) error: %d"), protocolName, functionName, optionName, result);
  }

  return result;
}

MPIPTVSOURCE_API int GetSocketOption(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *optionName, SOCKET s, int level, int optname, char *optval, int *optlen)
{
  int result = 0;
  if (getsockopt(s, level, optname, optval, optlen) == SOCKET_ERROR)
  {
    result = WSAGetLastError();
    logger->Log(LOGGER_ERROR, _T("%s: %s: getsockopt(%s) error: %d"), protocolName, functionName, optionName, result);
  }

  return result;
}

MPIPTVSOURCE_API int SetAndCheckSocketOption(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *optionName, SOCKET s, int level, int optname, const char *optval, int optlen)
{
  int result = SetSocketOption(logger, protocolName, functionName, optionName, s, level, optname, optval, optlen);

  if (result == 0)
  {
    // option correctly set, now check it
    int valLen = optlen;
    ALLOC_MEM_DEFINE_SET(val, char, optlen, 0);
    result = (val == NULL) ? E_OUTOFMEMORY : 0;
    if (result == 0)
    {
      result = GetSocketOption(logger, protocolName, functionName, optionName, s, level, optname, val, &valLen);
      if (result == 0)
      {
        // successfully retrieved value
        if (optlen == valLen)
        {
          result = (memcmp(optval, val, optlen) == 0) ? 0 : (-1);
        }
      }
    }

    FREE_MEM(val);
  }

  return result;
}

MPIPTVSOURCE_API int SetBlockingMode(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, bool blocking)
{
  int result = 0;
  unsigned long nonblocking = (blocking == 0);

  if (ioctlsocket(s, FIONBIO, &nonblocking) == SOCKET_ERROR) 
  {
    result = WSAGetLastError();
    logger->Log(LOGGER_ERROR, _T("%s: %s: ioctlsocket(FIONBIO) error: %d"), protocolName, functionName, result);
  }

  return result;
}

MPIPTVSOURCE_API int Bind(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const sockaddr *name, int namelen)
{
  int result = 0;

  if (bind(s, name, namelen) == SOCKET_ERROR)
  {
    result = WSAGetLastError();
    logger->Log(LOGGER_ERROR, _T("%s: %s: bind() error: %d"), protocolName, functionName, result);
  }

  return result;
}

MPIPTVSOURCE_API int Connect(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const sockaddr *name, int namelen, unsigned int timeout, bool read, bool write)
{
  int result = 0;

  if (connect(s, name, namelen) == SOCKET_ERROR)
  {
    result = ProcessError(logger, protocolName, functionName, _T("Connect()"), s, timeout, read, write);
  }

  return result;
}

MPIPTVSOURCE_API int ProcessError(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, const TCHAR *networkFunctionName, SOCKET s, unsigned int timeout, bool read, bool write)
{
  int result = 0;
  int error = WSAGetLastError();

  if (error == WSAEWOULDBLOCK)
  {
    fd_set readFD;
    fd_set writeFD;
    fd_set exceptFD;

    FD_ZERO(&readFD);
    FD_ZERO(&writeFD);
    FD_ZERO(&exceptFD);

    if (read)
    {
      // want to read from socket
      FD_SET(s, &readFD);
    }
    if (write)
    {
      // want to write to socket
      FD_SET(s, &writeFD);
    }
    // we want to receive errors
    FD_SET(s, &exceptFD);

    timeval sendTimeout;
    sendTimeout.tv_sec = timeout;
    sendTimeout.tv_usec = 0;

    int selectResult = select(0, &readFD, &writeFD, &exceptFD, (timeout == 0) ? NULL : &sendTimeout);
    if (selectResult == 0)
    {
      // timeout occured
      result = WSAETIMEDOUT;
      logger->Log(LOGGER_ERROR, _T("%s: %s: %s timeout"), protocolName, functionName, networkFunctionName);
    }
    else if (selectResult == SOCKET_ERROR)
    {
      // socket error occured
      result = WSAGetLastError();
      logger->Log(LOGGER_ERROR, _T("%s: %s: %s error: %d"), protocolName, functionName, networkFunctionName, result);
    }

    if (result == 0)
    {
      if (FD_ISSET(s, &exceptFD))
      {
        // error occured on socket, select function was successful
        int err;
        int errlen = sizeof(err);

        if (GetSocketOption(logger, protocolName, functionName, _T("SO_ERROR"), s, SOL_SOCKET, SO_ERROR, (char *)&err, &errlen) == 0)
        {
          // successfully get error
          result = err;
          logger->Log(LOGGER_ERROR, _T("%s: %s: %s socket error: %d"), protocolName, functionName, networkFunctionName, err);
        }
        else
        {
          // error occured while getting error
          result = WSAGetLastError();
          logger->Log(LOGGER_ERROR, _T("%s: %s: %s error occured while getting error: %d"), protocolName, functionName, networkFunctionName, result);
        }
      }

      if (result == 0)
      {
        if (read && (FD_ISSET(s, &readFD) == 0))
        {
          result |= 0x01;
          // socket is not in readable state => error
          logger->Log(LOGGER_ERROR, _T("%s: %s: %s socket not in readable state"), protocolName, functionName, networkFunctionName);
        }

        if (write && (FD_ISSET(s, &writeFD) == 0))
        {
          result |= 0x02;
          // socket is not in writable state => error
          logger->Log(LOGGER_ERROR, _T("%s: %s: %s socket not in writable state"), protocolName, functionName, networkFunctionName);
        }

        result = (-result);
      }
    }
  }
  else
  {
    // another error then WSAEWOULDBLOCK
    result = error;
  }

  return result;
}

MPIPTVSOURCE_API int Send(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const TCHAR *buffer, int length, int flags, unsigned int timeout)
{
  int result = 0;

  if (send(s, buffer, length, 0) == SOCKET_ERROR)
  {
    result = ProcessError(logger, protocolName, functionName, _T("Send()"), s, timeout, 0, 0);
  }

  return result;
}

MPIPTVSOURCE_API int SendString(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, const TCHAR *buffer, int flags, unsigned int timeout)
{
  // first convert buffer to multi-byte character
  char *tempBuffer = ConvertToMultiByte(buffer);
  int result = (tempBuffer == NULL) ? E_OUTOFMEMORY : 0;

  if (result == 0)
  {
    size_t length = strlen(tempBuffer);
    result = Send(logger, protocolName, functionName, s, tempBuffer, length, flags, timeout);
  }

  FREE_MEM(tempBuffer);

  return result;
}

MPIPTVSOURCE_API int GetDataFromSocket(CLogger *logger, const TCHAR *protocolName, const TCHAR *functionName, SOCKET s, char *buffer, unsigned int length)
{
  SOCKADDR_STORAGE safrom;
  int fromlen;
  fromlen = sizeof(safrom);

  int retval = recvfrom(s, buffer, length, 0, (SOCKADDR *)&safrom, &fromlen);

  // in case of WSAEWOULDBLOCK error (no data at socket)
  // we should sleep some time
  int recvfromError = 0;
  if (retval == SOCKET_ERROR)
  {
    // socket error occured => ignore packet
    recvfromError = WSAGetLastError();

    if (recvfromError == WSAEMSGSIZE)
    {
      // no enough free space in buffer
      logger->Log(LOGGER_WARNING, _T("%s: %s: recvfrom error: WSAEMSGSIZE"), protocolName, functionName);
      retval = 0;
    }
    else if (recvfromError != WSAEWOULDBLOCK)
    {
      // WSAEWOULDBLOCK = no data available
      logger->Log(LOGGER_ERROR, _T("%s: %s: recvfrom error: %d"), protocolName, functionName, recvfromError);
      // some error occured in socket
      // close connection
      // new connection will try to open in next run
    }
    else
    {
      // no error, no data
      retval = 0;
    }
  }

  return retval;
}

MPIPTVSOURCE_API int DumpInputPacket(GUID protocolInstance, unsigned int length, char *packet)
{
  int result = 0;

  TCHAR *folder = GetTvServerFolder();
  TCHAR *guid = ConvertGuidToString(protocolInstance);
  if ((folder != NULL) && (guid != NULL) && (length > 0) && (packet != NULL))
  {
    TCHAR *lengthFileName = FormatString(_T("%slog\\mpiptv_input_dump_length_%s.txt"), folder, guid);
    TCHAR *dumpFileName = FormatString(_T("%slog\\mpiptv_input_dump_%s.ts"), folder, guid);
    if ((dumpFileName != NULL) && (lengthFileName != NULL))
    {
      // we have raw TS file path
      FILE *dumpStream = NULL;
      FILE *lengthStream = NULL;

      result = _tfopen_s(&dumpStream, dumpFileName, _T("ab"));
      if (result == 0)
      {
        result = _tfopen_s(&lengthStream, lengthFileName, _T("a"));
        if (result == 0)
        {
          if (fwrite(packet, sizeof(char), length, dumpStream) != length)
          {
            result = -1;
          }

          if (_ftprintf(lengthStream, _T("%u\n"), length) < 0)
          {
            result = -1;
          }

          fclose(lengthStream);
        }

        fclose(dumpStream);
      }
    }
    else
    {
      result = -1;
    }
    FREE_MEM(lengthFileName);
    FREE_MEM(dumpFileName);
  }
  else
  {
    result = -1;
  }

  FREE_MEM(folder);
  FREE_MEM(guid);

  return result;
}