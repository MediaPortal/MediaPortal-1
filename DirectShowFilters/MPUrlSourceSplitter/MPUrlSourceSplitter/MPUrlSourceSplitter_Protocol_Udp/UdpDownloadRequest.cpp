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

#include "UdpDownloadRequest.h"
#include "MPUrlSourceSplitter_Protocol_Udp_Parameters.h"

CUdpDownloadRequest::CUdpDownloadRequest(HRESULT *result)
  : CDownloadRequest(result)
{
  this->checkInterval = 0;
  this->ipv4Dscp = 0;
  this->ipv4Ecn = 0;
  this->ipv4Identification = 0;
  this->ipv4Flags = 0;
  this->ipv4Ttl = 0;
  this->ipv4Protocol = 0;
  this->ipv4Options = NULL;
  this->ipv4OptionsLength = 0;
}

CUdpDownloadRequest::~CUdpDownloadRequest(void)
{
  FREE_MEM(this->ipv4Options);
}

/* get methods */

unsigned int CUdpDownloadRequest::GetCheckInterval(void)
{
  return this->checkInterval;
}

uint8_t CUdpDownloadRequest::GetIpv4Dscp(void)
{
  return this->ipv4Dscp;
}

uint8_t CUdpDownloadRequest::GetIpv4Ecn(void)
{
  return this->ipv4Ecn;
}

uint16_t CUdpDownloadRequest::GetIpv4Identification(void)
{
  return this->ipv4Identification;
}

uint8_t CUdpDownloadRequest::GetIpv4Flags(void)
{
  return this->ipv4Flags;
}

uint8_t CUdpDownloadRequest::GetIpv4Ttl(void)
{
  return this->ipv4Ttl;
}

uint8_t CUdpDownloadRequest::GetIpv4Protocol(void)
{
  return this->ipv4Protocol;
}

uint8_t *CUdpDownloadRequest::GetIpv4Options(void)
{
  return this->ipv4Options;
}

uint8_t CUdpDownloadRequest::GetIpv4OptionsLength(void)
{
  return this->ipv4OptionsLength;
}

/* set methods */

void CUdpDownloadRequest::SetCheckInterval(unsigned int checkInterval)
{
  this->checkInterval = checkInterval;
}

void CUdpDownloadRequest::SetIpv4Dscp(uint8_t dscp)
{
  this->ipv4Dscp = dscp & UDP_IPV4_DSCP_MAX;
  this->flags |= UDP_DOWNLOAD_REQUEST_FLAG_IPV4_DSCP;
}

void CUdpDownloadRequest::SetIpv4Ecn(uint8_t ecn)
{
  this->ipv4Ecn = ecn & UDP_IPV4_ECN_MAX;
  this->flags |= UDP_DOWNLOAD_REQUEST_FLAG_IPV4_ECN;
}

void CUdpDownloadRequest::SetIpv4Identification(uint16_t identification)
{
  this->ipv4Identification = identification;
  this->flags |= UDP_DOWNLOAD_REQUEST_FLAG_IPV4_IDENTIFICATION;
}

void CUdpDownloadRequest::SetIpv4Flags(uint8_t flags)
{
  this->ipv4Flags = flags & UDP_IPV4_FLAGS_MAX;
  this->flags |= UDP_DOWNLOAD_REQUEST_FLAG_IPV4_FLAGS;
}

void CUdpDownloadRequest::SetIpv4Ttl(uint8_t ttl)
{
  this->ipv4Ttl = ttl;
  this->flags |= UDP_DOWNLOAD_REQUEST_FLAG_IPV4_TTL;
}

void CUdpDownloadRequest::SetIpv4Protocol(uint8_t protocol)
{
  this->ipv4Protocol = protocol;
  this->flags |= UDP_DOWNLOAD_REQUEST_FLAG_IPV4_PROTOCOL;
}

bool CUdpDownloadRequest::SetIpv4Options(uint8_t *options, uint8_t optionsLength)
{
  FREE_MEM(this->ipv4Options);
  this->ipv4OptionsLength = 0;

  bool result = true;
  this->flags &= ~UDP_DOWNLOAD_REQUEST_FLAG_IPV4_OPTIONS;

  if (options != NULL)
  {
    this->ipv4Options = ALLOC_MEM_SET(this->ipv4Options, uint8_t, optionsLength, 0);
    result &= (this->ipv4Options != NULL);

    if (result)
    {
      memcpy(this->ipv4Options, options, optionsLength);
      this->ipv4OptionsLength = optionsLength;
      this->flags |= UDP_DOWNLOAD_REQUEST_FLAG_IPV4_OPTIONS;
    }
  }

  return result;
}

/* other methods */

bool CUdpDownloadRequest::IsRawSocket(void)
{
  return this->IsSetAnyOfFlags(
    UDP_DOWNLOAD_REQUEST_FLAG_IPV4_DSCP | 
    UDP_DOWNLOAD_REQUEST_FLAG_IPV4_ECN | 
    UDP_DOWNLOAD_REQUEST_FLAG_IPV4_FLAGS | 
    UDP_DOWNLOAD_REQUEST_FLAG_IPV4_IDENTIFICATION |
    UDP_DOWNLOAD_REQUEST_FLAG_IPV4_OPTIONS |
    UDP_DOWNLOAD_REQUEST_FLAG_IPV4_PROTOCOL |
    UDP_DOWNLOAD_REQUEST_FLAG_IPV4_TTL);
}

/* protected methods */

CDownloadRequest *CUdpDownloadRequest::CreateDownloadRequest(void)
{
  HRESULT result = S_OK;
  CUdpDownloadRequest *request = new CUdpDownloadRequest(&result);
  CHECK_POINTER_HRESULT(result, request, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(request));
  return request;
}

bool CUdpDownloadRequest::CloneInternal(CDownloadRequest *clone)
{
  bool result = __super::CloneInternal(clone);

  if (result)
  {
    CUdpDownloadRequest *request = dynamic_cast<CUdpDownloadRequest *>(clone);
    result &= (request != NULL);

    if (result)
    {
      request->checkInterval = this->checkInterval;
      request->ipv4Dscp = this->ipv4Dscp;
      request->ipv4Ecn = this->ipv4Ecn;
      request->ipv4Identification = this->ipv4Identification;
      request->ipv4Flags = this->ipv4Flags;
      request->ipv4Ttl = this->ipv4Ttl;
      request->ipv4Protocol = this->ipv4Protocol;

      if (this->ipv4Options != NULL)
      {
        request->ipv4Options = ALLOC_MEM_SET(request->ipv4Options, uint8_t, this->ipv4OptionsLength, 0);
        result &= (request->ipv4Options != NULL);

        CHECK_CONDITION_EXECUTE(result, memcpy(request->ipv4Options, this->ipv4Options, this->ipv4OptionsLength));
      }

      request->ipv4OptionsLength = this->ipv4OptionsLength;
    }
  }

  return result;
}