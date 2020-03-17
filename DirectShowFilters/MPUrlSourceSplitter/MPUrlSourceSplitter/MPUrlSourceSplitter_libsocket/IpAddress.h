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

#ifndef __IP_ADDRESS_DEFINED
#define __IP_ADDRESS_DEFINED

#include <Ws2tcpip.h>

#define IP_ADDRESS_FLAG_NONE                                          0

class CIpAddress
{
public:
  // initializes a new instance of CIpAddress class with specified ADDRINFOW structure
  CIpAddress(HRESULT *result, const ADDRINFOW *addrInfo, const wchar_t *canonicalName);

  // initializes a new instance of CIpAddress class with specified SOCKADDR_STORAGE structure and length
  CIpAddress(HRESULT *result, const SOCKADDR_STORAGE *addr, unsigned int length);

  // initializes a new instance of CIpAddress class with specified sockaddr structure and length
  CIpAddress(HRESULT *result, const struct sockaddr *addr, unsigned int length);

  ~CIpAddress(void);

  /* get methods */

  // gets address family (e.g. AF_INET, AF_INET6, ...)
  // @return : address family or 0 if not specified
  int GetFamily(void);

  // gets socket type (e.g. SOCK_STREAM, SOCK_DGRAM, ...)
  // @return : socket type or 0 if not specified
  int GetSockType(void);

  // gets protocol (e.g. IPPROTO_TCP, IPPROTO_UDP, ...)
  // @return : protocol or 0 if not specified
  int GetProtocol(void);

  // gets canonical name
  // @return : canonical name or NULL if not specified
  const wchar_t *GetCanonicalName(void);

  // gets address length
  // @return : length of address structure
  unsigned int GetAddressLength(void);

  // gets address structure
  // @return : address structure or NULL if error
  const SOCKADDR_STORAGE *GetAddress(void);

  // gets IP address
  // @return : IP address in sockaddr structure or NULL if error or address not IP
  const struct sockaddr *GetAddressIP(void);

  // gets IPv4 address
  // @return : IPv4 address in sockaddr_in structure or NULL if error or address not IPv4
  const struct sockaddr_in *GetAddressIPv4(void);

  // gets IPv6 address
  // @return : IPv6 address in sockaddr_in6 structure or NULL if error or address not IPv6
  const struct sockaddr_in6 *GetAddressIPv6(void);

  // gets port of IP address
  // @return : port of IP address or 0 if not specified or error
  unsigned short GetPort(void);

  // gets human-readable address
  // @return : human-readable address string or NULL if error
  const wchar_t *GetAddressString(void);

  /* set methods */

  // sets sock type (e.g. SOCK_STREAM, SOCK_DGRAM, ...)
  // @param sockType : the sock type to set
  void SetSockType(int sockType);

  // sets protocol (e.g. IPPROTO_TCP, IPPROTO_UDP, ...)
  // @param protocol : the protocol to set
  void SetProtocol(int protocol);

  // set port to IP address
  // @return : true if successful, false otherwise
  bool SetPort(unsigned short port);

  /* other methods */

  // tests if IP address is IP address
  // @return : true if IP address is IP, false otherwise
  bool IsIP(void);

  // tests if IP address is IPv4 address
  // @return : true if IP address is IPv4, false otherwise
  bool IsIPv4(void);

  // tests if IP address is IPv6 address
  // @return : true if IP address is IPv6, false otherwise
  bool IsIPv6(void);

  // tests if IP address is multicast address
  // @return : true if address if multicast, false otherwise
  bool IsMulticast(void);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  CIpAddress *Clone(void);

protected:

  // holds ai_flags from ADDRINFOW structure
  int flags;

  // holds ai_family from ADDRINFOW structure
  int family;

  // holds ai_socktype from ADDRINFOW structure
  int socktype;

  // holds ai_protocol from ADDRINFOW structure
  int protocol;

  // holds canonical name for the host from ADDRINFOW structure
  wchar_t *canonicalName;

  // holds length of ai_addr (ai_addrlen) from ADDRINFOW structure
  unsigned int length;

  // holds ai_addr from ADDRINFOW structure
  SOCKADDR_STORAGE *addr;

  // holds human-readable address
  wchar_t *addressString;

  /* methods */

  // initializes a new instance of CIpAddress class
  CIpAddress();
};

#endif