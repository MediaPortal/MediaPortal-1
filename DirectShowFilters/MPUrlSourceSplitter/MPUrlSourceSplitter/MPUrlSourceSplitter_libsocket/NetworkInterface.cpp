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

#include "NetworkInterface.h"
#include "NetworkInterfaceCollection.h"

#include <winsock2.h>
#include <IPHlpApi.h>

CNetworkInterface::CNetworkInterface(HRESULT *result)
{
  this->adapterName = NULL;
  this->friendlyName = NULL;
  this->description = NULL;
  this->dnsSuffix = NULL;
  this->flags = 0;
  this->ipv4Index = 0;
  this->ipv6Index = 0;
  this->maximumTransmissionUnit = 0;
  this->operationalStatus = IfOperStatusUnknown;

  this->unicastAddresses = NULL;
  this->anycastAddresses = NULL;
  this->multicastAddresses = NULL;
  this->dnsServerAddresses = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->unicastAddresses = new CIpAddressCollection(result);
    this->anycastAddresses = new CIpAddressCollection(result);
    this->multicastAddresses = new CIpAddressCollection(result);
    this->dnsServerAddresses = new CIpAddressCollection(result);

    CHECK_POINTER_HRESULT(*result, this->unicastAddresses, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->anycastAddresses, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->multicastAddresses, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->dnsServerAddresses, *result, E_OUTOFMEMORY);
  }
}

CNetworkInterface::~CNetworkInterface(void)
{
  FREE_MEM(this->adapterName);
  FREE_MEM(this->friendlyName);
  FREE_MEM(this->description);
  FREE_MEM(this->dnsSuffix);
  FREE_MEM_CLASS(this->unicastAddresses);
  FREE_MEM_CLASS(this->anycastAddresses);
  FREE_MEM_CLASS(this->multicastAddresses);
  FREE_MEM_CLASS(this->dnsServerAddresses);
}

/* get methods */

const wchar_t *CNetworkInterface::GetAdapterName(void)
{
  return this->adapterName;
}

const wchar_t *CNetworkInterface::GetFriendlyName(void)
{
  return this->friendlyName;
}

const wchar_t *CNetworkInterface::GetDnsSuffix(void)
{
  return this->dnsSuffix;
}

const wchar_t *CNetworkInterface::GetDescription(void)
{
  return this->description;
}

unsigned int CNetworkInterface::GetMaximumTransmissionUnit(void)
{
  return this->maximumTransmissionUnit;
}

unsigned int CNetworkInterface::GetIpv4Index(void)
{
  return this->ipv4Index;
}

unsigned int CNetworkInterface::GetIpv6Index(void)
{
  return this->ipv6Index;
}

CIpAddressCollection *CNetworkInterface::GetUnicastAddresses(void)
{
  return this->unicastAddresses;
}

CIpAddressCollection *CNetworkInterface::GetAnycastAddresses(void)
{
  return this->anycastAddresses;
}

CIpAddressCollection *CNetworkInterface::GetMulticastAddresses(void)
{
  return this->multicastAddresses;
}

CIpAddressCollection *CNetworkInterface::GetDnsServerAddresses(void)
{
  return this->dnsServerAddresses;
}

IF_OPER_STATUS CNetworkInterface::GetOperationalStatus(void)
{
  return this->operationalStatus;
}

/* set methods */

bool CNetworkInterface::SetAdapterName(const wchar_t *adapterName)
{
  SET_STRING_RETURN_WITH_NULL(this->adapterName, adapterName);
}

bool CNetworkInterface::SetFriendlyName(const wchar_t *friendlyName)
{
  SET_STRING_RETURN_WITH_NULL(this->friendlyName, friendlyName);
}

bool CNetworkInterface::SetDnsSuffix(const wchar_t *dnsSuffix)
{
  SET_STRING_RETURN_WITH_NULL(this->dnsSuffix, dnsSuffix);
}

bool CNetworkInterface::SetDescription(const wchar_t *description)
{
  SET_STRING_RETURN_WITH_NULL(this->description, description);
}

void CNetworkInterface::SetMaximumTransmissionUnit(unsigned int maximumTransmissionUnit)
{
  this->maximumTransmissionUnit = maximumTransmissionUnit;
}

void CNetworkInterface::SetIpv4Index(unsigned int ipv4Index)
{
  this->ipv4Index = ipv4Index;
}

void CNetworkInterface::SetIpv6Index(unsigned int ipv6Index)
{
  this->ipv6Index = ipv6Index;
}

void CNetworkInterface::SetOperationalStatus(IF_OPER_STATUS operationalStatus)
{
  this->operationalStatus = operationalStatus;
}

/* other methods */

CNetworkInterface *CNetworkInterface::Clone(void)
{
  HRESULT result = S_OK;
  CNetworkInterface *clone = new CNetworkInterface(&result);
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  if (SUCCEEDED(result))
  {
    clone->flags = this->flags;
    clone->ipv4Index = this->ipv4Index;
    clone->ipv6Index = this->ipv6Index;
    clone->maximumTransmissionUnit = this->maximumTransmissionUnit;
    clone->operationalStatus = this->operationalStatus;

    SET_STRING_HRESULT_WITH_NULL(clone->adapterName, this->adapterName, result);
    SET_STRING_HRESULT_WITH_NULL(clone->description, this->description, result);
    SET_STRING_HRESULT_WITH_NULL(clone->dnsSuffix, this->dnsSuffix, result);
    SET_STRING_HRESULT_WITH_NULL(clone->friendlyName, this->friendlyName, result);

    CHECK_CONDITION_HRESULT(result, clone->anycastAddresses->Append(this->anycastAddresses), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, clone->dnsServerAddresses->Append(this->dnsServerAddresses), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, clone->multicastAddresses->Append(this->multicastAddresses), result, E_OUTOFMEMORY);
    CHECK_CONDITION_HRESULT(result, clone->unicastAddresses->Append(this->unicastAddresses), result, E_OUTOFMEMORY);
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}

HRESULT CNetworkInterface::GetAllNetworkInterfaces(CNetworkInterfaceCollection *interfaces)
{
  return CNetworkInterface::GetAllNetworkInterfaces(interfaces, AF_UNSPEC);
}

HRESULT CNetworkInterface::GetAllNetworkInterfaces(CNetworkInterfaceCollection *interfaces, int family)
{
  HRESULT result = S_OK;
  CHECK_POINTER_DEFAULT_HRESULT(result, interfaces);

  if (SUCCEEDED(result))
  {
    ULONG bufferLen = 0;
    ULONG flags = GAA_FLAG_INCLUDE_PREFIX;
    DWORD retval = 0;

    PIP_ADAPTER_ADDRESSES addresses = NULL;
    PIP_ADAPTER_ADDRESSES pCurrAddresses = NULL;
    PIP_ADAPTER_UNICAST_ADDRESS pUnicast = NULL;

    if (GetAdaptersAddresses(family, flags, NULL, addresses, &bufferLen) == ERROR_BUFFER_OVERFLOW)
    {
      addresses = (PIP_ADAPTER_ADDRESSES)CoTaskMemAlloc(bufferLen);
      CHECK_POINTER_HRESULT(result, addresses, result, E_OUTOFMEMORY);

      CHECK_CONDITION_EXECUTE_RESULT(SUCCEEDED(result), HRESULT_FROM_WIN32(GetAdaptersAddresses(family, flags, NULL, addresses, &bufferLen)), result);
    }

    if (SUCCEEDED(result))
    {
      for (PIP_ADAPTER_ADDRESSES address = addresses; (SUCCEEDED(result) && (address != NULL)); address = address->Next)
      {
        CNetworkInterface *nic = new CNetworkInterface(&result);
        CHECK_POINTER_HRESULT(result, nic, result, E_OUTOFMEMORY);
        
        if (SUCCEEDED(result))
        {
          nic->SetIpv4Index(address->IfIndex);

          wchar_t *temp = ConvertToUnicodeA(address->AdapterName);
          result = nic->SetAdapterName(temp) ? result : E_OUTOFMEMORY;
          FREE_MEM(temp);

          for (PIP_ADAPTER_UNICAST_ADDRESS unicastAddress = address->FirstUnicastAddress; (SUCCEEDED(result) && (unicastAddress != NULL)); unicastAddress = unicastAddress->Next)
          {
            CIpAddress *ipAddr = new CIpAddress(&result, unicastAddress->Address.lpSockaddr, unicastAddress->Address.iSockaddrLength);
            CHECK_POINTER_HRESULT(result, ipAddr, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, nic->GetUnicastAddresses()->Add(ipAddr), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(ipAddr));
          }

          for (PIP_ADAPTER_ANYCAST_ADDRESS anycastAddress = address->FirstAnycastAddress; (SUCCEEDED(result) && (anycastAddress != NULL)); anycastAddress = anycastAddress->Next)
          {
            CIpAddress *ipAddr = new CIpAddress(&result, anycastAddress->Address.lpSockaddr, anycastAddress->Address.iSockaddrLength);
            CHECK_POINTER_HRESULT(result, ipAddr, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, nic->GetAnycastAddresses()->Add(ipAddr), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(ipAddr));
          }

          for (PIP_ADAPTER_MULTICAST_ADDRESS multicastAddress = address->FirstMulticastAddress; (SUCCEEDED(result) && (multicastAddress != NULL)); multicastAddress = multicastAddress->Next)
          {
            CIpAddress *ipAddr = new CIpAddress(&result, multicastAddress->Address.lpSockaddr, multicastAddress->Address.iSockaddrLength);
            CHECK_POINTER_HRESULT(result, ipAddr, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, nic->GetMulticastAddresses()->Add(ipAddr), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(ipAddr));
          }

          for (PIP_ADAPTER_DNS_SERVER_ADDRESS dnsServerAddress = address->FirstDnsServerAddress; (SUCCEEDED(result) && (dnsServerAddress != NULL)); dnsServerAddress = dnsServerAddress->Next)
          {
            CIpAddress *ipAddr = new CIpAddress(&result, dnsServerAddress->Address.lpSockaddr, dnsServerAddress->Address.iSockaddrLength);
            CHECK_POINTER_HRESULT(result, ipAddr, result, E_OUTOFMEMORY);

            CHECK_CONDITION_HRESULT(result, nic->GetDnsServerAddresses()->Add(ipAddr), result, E_OUTOFMEMORY);
            CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(ipAddr));
          }

          CHECK_CONDITION_HRESULT(result, nic->SetDnsSuffix(address->DnsSuffix), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, nic->SetDescription(address->Description), result, E_OUTOFMEMORY);
          CHECK_CONDITION_HRESULT(result, nic->SetFriendlyName(address->FriendlyName), result, E_OUTOFMEMORY);

          nic->SetMaximumTransmissionUnit(address->Mtu);

          nic->SetOperationalStatus(address->OperStatus);
          nic->SetIpv6Index(address->Ipv6IfIndex);

          CHECK_CONDITION_HRESULT(result, interfaces->Add(nic), result, E_OUTOFMEMORY);
        }

        CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(nic));
      }
    }

    FREE_MEM(addresses);
  }

  return result;
}