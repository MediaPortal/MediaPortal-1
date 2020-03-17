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

#include "IpAddress.h"

CIpAddress::CIpAddress()
{
  this->flags = IP_ADDRESS_FLAG_NONE;
  this->addr = NULL;
  this->canonicalName = NULL;
  this->family = AF_UNSPEC;
  this->length = 0;
  this->protocol = 0;
  this->socktype = 0;
  this->addressString = NULL;
}

CIpAddress::CIpAddress(HRESULT *result, const ADDRINFOW *addrInfo, const wchar_t *canonicalName)
{
  this->flags = IP_ADDRESS_FLAG_NONE;
  this->addr = NULL;
  this->canonicalName = NULL;
  this->family = AF_UNSPEC;
  this->length = 0;
  this->protocol = 0;
  this->socktype = 0;
  this->addressString = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, addrInfo);

    if (SUCCEEDED(*result))
    {
      this->flags = addrInfo->ai_flags;

      this->family = addrInfo->ai_family;
      this->length = addrInfo->ai_addrlen;
      this->protocol = addrInfo->ai_protocol;
      this->socktype = addrInfo->ai_socktype;

      SET_STRING_HRESULT_WITH_NULL(this->canonicalName, ((addrInfo->ai_canonname != NULL) ? addrInfo->ai_canonname : canonicalName), *result);

      if (this->length != 0)
      {
        this->addr = ALLOC_MEM_SET(this->addr, SOCKADDR_STORAGE, 1, 0);
        CHECK_POINTER_HRESULT(*result, this->addr, *result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), memcpy(this->addr, addrInfo->ai_addr, this->length));
      }
    }

    if (SUCCEEDED(*result))
    {
      if (this->IsIP())
      {
        DWORD addressStringLength = 128;
        this->addressString = ALLOC_MEM_SET(this->addressString, wchar_t, addressStringLength, 0);

        if (this->addressString != NULL)
        {
          if (FAILED(WSAAddressToString((LPSOCKADDR)this->GetAddressIP(), this->length, NULL, this->addressString, &addressStringLength)))
          {
            FREE_MEM(this->addressString);
          }
        }
      }
    }
  }
}

CIpAddress::CIpAddress(HRESULT *result, const SOCKADDR_STORAGE *addr, unsigned int length)
{
  this->flags = IP_ADDRESS_FLAG_NONE;
  this->addr = NULL;
  this->canonicalName = NULL;
  this->family = AF_UNSPEC;
  this->length = 0;
  this->protocol = 0;
  this->socktype = 0;
  this->addressString = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, addr);
    CHECK_CONDITION_HRESULT(*result, length > 0, *result, E_INVALIDARG);

    if (SUCCEEDED(*result))
    {
      this->family = addr->ss_family;
      this->length = length;

      if (this->length != 0)
      {
        this->addr = ALLOC_MEM_SET(this->addr, SOCKADDR_STORAGE, 1, 0);
        CHECK_POINTER_HRESULT(*result, this->addr, *result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), memcpy(this->addr, addr, this->length));
      }
    }

    if (SUCCEEDED(*result))
    {
      if (this->IsIP())
      {
        DWORD addressStringLength = 128;
        this->addressString = ALLOC_MEM_SET(this->addressString, wchar_t, addressStringLength, 0);

        if (this->addressString != NULL)
        {
          if (FAILED(WSAAddressToString((LPSOCKADDR)this->GetAddressIP(), this->length, NULL, this->addressString, &addressStringLength)))
          {
            FREE_MEM(this->addressString);
          }
        }
      }
    }
  }
}

CIpAddress::CIpAddress(HRESULT *result, const struct sockaddr *addr, unsigned int length)
{
  this->flags = IP_ADDRESS_FLAG_NONE;
  this->addr = NULL;
  this->canonicalName = NULL;
  this->family = AF_UNSPEC;
  this->length = 0;
  this->protocol = 0;
  this->socktype = 0;
  this->addressString = NULL;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    CHECK_POINTER_DEFAULT_HRESULT(*result, addr);
    CHECK_CONDITION_HRESULT(*result, length > 0, *result, E_INVALIDARG);

    if (SUCCEEDED(*result))
    {
      this->family = addr->sa_family;
      this->length = length;

      if (this->length != 0)
      {
        this->addr = ALLOC_MEM_SET(this->addr, SOCKADDR_STORAGE, 1, 0);
        CHECK_POINTER_HRESULT(*result, this->addr, *result, E_OUTOFMEMORY);

        CHECK_CONDITION_EXECUTE(SUCCEEDED(*result), memcpy(this->addr, addr, this->length));
      }
    }

    if (SUCCEEDED(*result))
    {
      if (this->IsIP())
      {
        DWORD addressStringLength = 128;
        this->addressString = ALLOC_MEM_SET(this->addressString, wchar_t, addressStringLength, 0);

        if (this->addressString != NULL)
        {
          if (FAILED(WSAAddressToString((LPSOCKADDR)this->GetAddressIP(), this->length, NULL, this->addressString, &addressStringLength)))
          {
            FREE_MEM(this->addressString);
          }
        }
      }
    }
  }
}

CIpAddress::~CIpAddress(void)
{
  FREE_MEM(this->addr);
  FREE_MEM(this->canonicalName);
  FREE_MEM(this->addressString);
}

/* get methods */

int CIpAddress::GetFamily(void)
{
  return this->family;
}

int CIpAddress::GetSockType(void)
{
  return this->socktype;
}

int CIpAddress::GetProtocol(void)
{
  return this->protocol;
}

const wchar_t *CIpAddress::GetCanonicalName(void)
{
  return this->canonicalName;
}

unsigned int CIpAddress::GetAddressLength(void)
{
  return this->length;
}

const SOCKADDR_STORAGE *CIpAddress::GetAddress(void)
{
  return this->addr;
}

const struct sockaddr *CIpAddress::GetAddressIP(void)
{
  return (this->IsIP()) ? (struct sockaddr *)this->addr : NULL;
}

const struct sockaddr_in *CIpAddress::GetAddressIPv4(void)
{
  return (this->IsIPv4()) ? (struct sockaddr_in *)this->addr : NULL;
}

const struct sockaddr_in6 *CIpAddress::GetAddressIPv6(void)
{
  return (this->IsIPv6()) ? (struct sockaddr_in6 *)this->addr : NULL;
}

unsigned short CIpAddress::GetPort(void)
{
  unsigned short result = 0;

  if (this->IsIPv4())
  {
    result = ntohs(((struct sockaddr_in *)this->addr)->sin_port);
  }
  else if (this->IsIPv6())
  {
    result = ntohs(((struct sockaddr_in6 *)this->addr)->sin6_port);
  }

  return result;
}

const wchar_t *CIpAddress::GetAddressString(void)
{
  return this->addressString;
}

/* set methods */

void CIpAddress::SetSockType(int sockType)
{
  this->socktype = sockType;
}

void CIpAddress::SetProtocol(int protocol)
{
  this->protocol = protocol;
}
  
bool CIpAddress::SetPort(unsigned short port)
{
  bool result = false;

  if (this->IsIPv4())
  {
    ((struct sockaddr_in *)this->addr)->sin_port = htons(port);
    result = true;
  }
  else if (this->IsIPv6())
  {
    ((struct sockaddr_in6 *)this->addr)->sin6_port = htons(port);
    result = true;
  }

  return result;
}

/* other methods */

bool CIpAddress::IsIP(void)
{
  return (this->IsIPv4() || this->IsIPv6());
}

bool CIpAddress::IsIPv4(void)
{
  return ((this->family == AF_INET) && (this->length == sizeof(struct sockaddr_in)));
}

bool CIpAddress::IsIPv6(void)
{
  return ((this->family == AF_INET6) && (this->length == sizeof(struct sockaddr_in6)));
}

bool CIpAddress::IsMulticast(void)
{
  bool result = false;

  if (this->IsIP())
  {
    switch (this->GetFamily())
    {
    case AF_INET:
      {
        const struct sockaddr_in *ipv4 = this->GetAddressIPv4();
        result = (IN_MULTICAST(ntohl(ipv4->sin_addr.s_addr)) != 0);
      }
      break;
    case AF_INET6:
      {
        const struct sockaddr_in6 *ipv6 = this->GetAddressIPv6();
        result = (IN6_IS_ADDR_MULTICAST(&ipv6->sin6_addr) != 0);
      }
      break;
    }
  }

  return result;
}

CIpAddress *CIpAddress::Clone(void)
{
  HRESULT result = S_OK;
  CIpAddress *clone = new CIpAddress(&result, this->GetAddress(), this->GetAddressLength());
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);
  
  if (SUCCEEDED(result))
  {
    clone->flags = this->flags;

    SET_STRING_HRESULT_WITH_NULL(clone->canonicalName, this->canonicalName, result);

    clone->protocol = this->protocol;
    clone->socktype = this->socktype;
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}
