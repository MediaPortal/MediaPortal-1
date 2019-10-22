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

#include "Ipv4Header.h"
#include "Ipv4Header_Constants.h"

CIpv4Header::CIpv4Header(HRESULT *result)
  : CFlags()
{
  this->flags |= IPV4_HEADER_FLAG_IDENTIFICATION_RANDOM;

  this->dscp = IPV4_HEADER_DSCP_DEFAULT;
  this->ecn = IPV4_HEADER_ECN_DEFAULT;
  this->identification = (uint16_t)GetTickCount();
  this->ttl = IPV4_HEADER_TTL_DEFAULT;
  this->protocol = IPV4_HEADER_PROTOCOL_DEFAULT;
  this->options = NULL;
  this->optionsLength = 0;
}

CIpv4Header::~CIpv4Header()
{
  FREE_MEM(this->options);
}

/* get methods */

uint8_t CIpv4Header::GetDscp(void)
{
  return this->dscp;
}

uint8_t CIpv4Header::GetEcn(void)
{
  return this->ecn;
}

uint16_t CIpv4Header::GetIdentification(void)
{
  return this->identification;
}

uint8_t CIpv4Header::GetTtl(void)
{
  return this->ttl;
}

uint8_t CIpv4Header::GetProtocol(void)
{
  return this->protocol;
}

uint8_t *CIpv4Header::GetOptions(void)
{
  return this->options;
}

uint8_t CIpv4Header::GetOptionsLength(void)
{
  return this->optionsLength;
}

/* set methods */

void CIpv4Header::SetDscp(uint8_t dscp)
{
  this->dscp = dscp & IPV4_HEADER_DSCP_MAX;
}

void CIpv4Header::SetEcn(uint8_t ecn)
{
  this->ecn = ecn & IPV4_HEADER_ECN_MAX;
}

void CIpv4Header::SetIdentification(uint16_t identification)
{
  this->flags &= ~IPV4_HEADER_FLAG_IDENTIFICATION_RANDOM;
  this->flags |= IPV4_HEADER_FLAG_IDENTIFICATION_SPECIFIC;
  this->identification = identification;
}

void CIpv4Header::SetDontFragment(bool dontFragment)
{
  this->flags &= ~IPV4_HEADER_FLAG_DONT_FRAGMENT;
  this->flags |= dontFragment ? IPV4_HEADER_FLAG_DONT_FRAGMENT : IPV4_HEADER_FLAG_NONE;
}

void CIpv4Header::SetMoreFragments(bool moreFragments)
{
  this->flags &= ~IPV4_HEADER_FLAG_MORE_FRAGMENTS;
  this->flags |= moreFragments ? IPV4_HEADER_FLAG_MORE_FRAGMENTS : IPV4_HEADER_FLAG_NONE;
}

void CIpv4Header::SetTtl(uint8_t ttl)
{
  this->ttl = ttl;
}

void CIpv4Header::SetProtocol(uint8_t protocol)
{
  this->protocol = protocol;
}

bool CIpv4Header::SetOptions(uint8_t *options, uint8_t optionsLength)
{
  FREE_MEM(this->options);
  this->optionsLength = 0;

  bool result = true;

  if (options != NULL)
  {
    this->options = ALLOC_MEM_SET(this->options, uint8_t, optionsLength, 0);
    result &= (this->options != NULL);

    if (result)
    {
      memcpy(this->options, options, optionsLength);
      this->optionsLength = optionsLength;
    }
  }

  return result;
}

/* other methods */

bool CIpv4Header::IsDontFragment(void)
{
  return this->IsSetFlags(IPV4_HEADER_FLAG_DONT_FRAGMENT);
}

bool CIpv4Header::IsMoreFragments(void)
{
  return this->IsSetFlags(IPV4_HEADER_FLAG_MORE_FRAGMENTS);
}

CIpv4Header *CIpv4Header::Clone(void)
{
  HRESULT result = S_OK;
  CIpv4Header *clone = new CIpv4Header(&result);
  CHECK_POINTER_HRESULT(result, clone, result, E_OUTOFMEMORY);

  clone->flags = this->flags;
  clone->dscp = this->dscp;
  clone->ecn = this->ecn;
  clone->identification = this->identification;
  clone->ttl = this->ttl;
  clone->protocol = this->protocol;
  clone->options = NULL;
  clone->optionsLength = 0;

  if (SUCCEEDED(result))
  {
    clone->options = ALLOC_MEM_SET(clone->options, uint8_t, this->optionsLength, 0);
    CHECK_POINTER_HRESULT(result, clone->options, result, E_OUTOFMEMORY);

    if (SUCCEEDED(result))
    {
      memcpy(clone->options, this->options, this->optionsLength);
      clone->optionsLength = this->optionsLength;
    }
  }

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(clone));
  return clone;
}

/* protected methods */

