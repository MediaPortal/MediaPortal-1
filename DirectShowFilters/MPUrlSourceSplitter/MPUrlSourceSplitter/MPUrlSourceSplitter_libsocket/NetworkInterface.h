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

#ifndef __NETWORK_INTERFACE_DEFINED
#define __NETWORK_INTERFACE_DEFINED

class CNetworkInterfaceCollection;

#include "IpAddressCollection.h"

#include <IPHlpApi.h>

class CNetworkInterface
{
public:
  CNetworkInterface(HRESULT *result);
  virtual ~CNetworkInterface(void);

  /* get methods */

  // gets adapter name
  // @return : adapter name or NULL if error
  virtual const wchar_t *GetAdapterName(void);

  // gets user friendly name of network interface
  // @return : user friendly name of network interface or NULL if error
  virtual const wchar_t *GetFriendlyName(void);

  // gets the domain name system (DNS) suffix associated with this network interface
  // @return : the domain name system (DNS) suffix associated with this network interface or NULL if error
  virtual const wchar_t *GetDnsSuffix(void);

  // gets description for the network interface
  // @return : description for the network interface or NULL if error
  virtual const wchar_t *GetDescription(void);

  // gets maximum transmission unit (MTU) size, in bytes
  // @return : maximum transmission unit (MTU) size, in bytes
  virtual unsigned int GetMaximumTransmissionUnit(void);

  // gets index of the IPv4 interface
  // @return : index of the IPv4 interface or zero if IPv4 not exists
  virtual unsigned int GetIpv4Index(void);

  // gets index of the IPv6 interface
  // @return : index of the IPv6 interface or zero if IPv6 not exists
  virtual unsigned int GetIpv6Index(void);

  // gets unicast addresses
  // @return : unicast addresses or NULL if error
  virtual CIpAddressCollection *GetUnicastAddresses(void);

  // gets anycast addresses
  // @return : anycast addresses or NULL if error
  virtual CIpAddressCollection *GetAnycastAddresses(void);

  // gets multicast addresses
  // @return : multicast addresses or NULL if error
  virtual CIpAddressCollection *GetMulticastAddresses(void);

  // gets DNS server addresses
  // @return : DNS server addresses or NULL if error
  virtual CIpAddressCollection *GetDnsServerAddresses(void);

  // gets operation status of network interface
  // @return : operation status of network interface
  virtual IF_OPER_STATUS GetOperationalStatus(void);

  /* set methods */

  // sets adapter name
  // @param adapterName : adapter name to set
  // @return : true if successful, false otherwise
  virtual bool SetAdapterName(const wchar_t *adapterName);

  // sets user friendly name of adapter
  // @param friendlyName : user friendly name to set
  // @return : true if successful, false otherwise
  virtual bool SetFriendlyName(const wchar_t *friendlyName);

  // sets the domain name system (DNS) suffix associated with this network interface
  // @param dnsSuffix : the domain name system (DNS) suffix associated with this network interface to set
  // @return : true if successful, false otherwise
  virtual bool SetDnsSuffix(const wchar_t *dnsSuffix);

  // sets description for the network interface
  // @param description : description for the network interface to set
  // @return : true if successful, false otherwise
  virtual bool SetDescription(const wchar_t *description);

  // sets maximum transmission unit (MTU) size, in bytes
  // @param maximumTransmissionUnit : maximum transmission unit (MTU) size to set, in bytes
  virtual void SetMaximumTransmissionUnit(unsigned int maximumTransmissionUnit);

  // sets index of the IPv4 interface
  // @param ipv4Index : index of the IPv4 interface to set
  virtual void SetIpv4Index(unsigned int ipv4Index);

  // sets index of the IPv6 interface
  // @param ipv6Index : index of the IPv6 interface to set
  virtual void SetIpv6Index(unsigned int ipv6Index);

  // sets network interface operation status
  // @param operationalStatus : operational status to set
  virtual void SetOperationalStatus(IF_OPER_STATUS operationalStatus);

  /* other methods */

  virtual CNetworkInterface *Clone(void);

  // gets all network interfaces
  // @param interfaces : the collection to add network interfaces
  // @return : S_OK if successful, error code otherwise
  static HRESULT GetAllNetworkInterfaces(CNetworkInterfaceCollection *interfaces);

  // gets all network interfaces
  // @param interfaces : the collection to add network interfaces
  // @param family : address family to get network interfaces
  // @return : S_OK if successful, error code otherwise
  static HRESULT GetAllNetworkInterfaces(CNetworkInterfaceCollection *interfaces, int family);

protected:

  wchar_t *adapterName;
  wchar_t *friendlyName;

  wchar_t *dnsSuffix;
  wchar_t *description;

  unsigned int flags;
  unsigned int maximumTransmissionUnit;

  unsigned int ipv4Index;
  unsigned int ipv6Index;

  CIpAddressCollection *unicastAddresses;
  CIpAddressCollection *anycastAddresses;
  CIpAddressCollection *multicastAddresses;
  CIpAddressCollection *dnsServerAddresses;

  IF_OPER_STATUS operationalStatus;

  /* methods */
};

#endif