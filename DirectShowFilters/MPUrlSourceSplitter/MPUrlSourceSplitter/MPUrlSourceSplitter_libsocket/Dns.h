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

#ifndef __DNS_DEFINED
#define __DNS_DEFINED

#include "IpAddressCollection.h"

class CDns
{
public:

  /* get methods */

  /* set methods */

  /* other methods */

  // gets IP address collection for specified host name
  // @param hostName : pointer to a NULL-terminated Unicode string that contains a host (node) name or a numeric host address string; for the Internet protocol, the numeric host address string is a dotted-decimal IPv4 address or an IPv6 hex address
  // @param port : port number
  // @param family : the address family (e.g. AF_INET, AF_INET6, ...)
  // @param type : the socket type (e.g. SOCK_DGRAM, SOCK_STREAM, ...)
  // @param protocol : the protocol type (e.g. IPPROTO_UDP, IPRPOTO_TCP, ...)
  // @param flags : flags that indicate options to be used
  // @param collection : the collection to add IP addresses
  // @return : S_OK if successful, error code otherwise
  static HRESULT GetIpAddresses(const wchar_t *hostName, WORD port, int family, int type, int protocol, unsigned int flags, CIpAddressCollection *collection);

protected:
  CDns(void);
  ~CDns(void);

};

#endif