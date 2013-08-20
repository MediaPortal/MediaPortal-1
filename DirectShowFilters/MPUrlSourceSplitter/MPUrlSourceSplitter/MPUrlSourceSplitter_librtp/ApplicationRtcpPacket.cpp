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

#include "ApplicationRtcpPacket.h"
#include "BufferHelper.h"

#include <stdint.h>

CApplicationRtcpPacket::CApplicationRtcpPacket(void)
  : CRtcpPacket()
{
  this->identifier = 0;
  this->name = NULL;
  this->applicationData = NULL;
  this->applicationDataLength = 0;
}

CApplicationRtcpPacket::~CApplicationRtcpPacket(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->applicationData);
}

/* get methods */

unsigned int CApplicationRtcpPacket::GetIdentifier(void)
{
  return this->identifier;
}

const wchar_t *CApplicationRtcpPacket::GetName(void)
{
  return this->name;
}

unsigned char *CApplicationRtcpPacket::GetApplicationData(void)
{
  return this->applicationData;
}

unsigned int CApplicationRtcpPacket::GetApplicationDataLength(void)
{
  return this->applicationDataLength;
}

/* set methods */

/* other methods */

void CApplicationRtcpPacket::Clear(void)
{
  __super::Clear();

  this->identifier = 0;
  this->applicationDataLength = 0;
  FREE_MEM(this->name);
  FREE_MEM(this->applicationData);
}

bool CApplicationRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->packetType == APPLICATION_RTCP_PACKET_TYPE);
  result &= (this->payloadLength >= APPLICATION_RTCP_PACKET_HEADER_SIZE);

  if (result)
  {
    // application RTCP packet header is at least APPLICATION_RTCP_PACKET_HEADER_SIZE long
    unsigned int position = 0;

    RBE32INC(this->payload, position, this->identifier);
    
    ALLOC_MEM_DEFINE_SET(temp, char, 5, 0);
    result &= (temp != NULL);

    if (result)
    {
      memcpy(temp, this->payload + position, 4);
      this->name = ConvertUtf8ToUnicode(temp);
      result &= (this->name != NULL);

      position += 4;
    }

    FREE_MEM(temp);

    this->applicationDataLength = this->payloadLength - position;

    if (result && (this->applicationDataLength > 0))
    {
      // there are still some data, it have to be application data
      this->applicationData = ALLOC_MEM_SET(this->applicationData, unsigned char, this->applicationDataLength, 0);
      result &= (this->applicationData != NULL);

      if (result)
      {
        memcpy(this->applicationData, this->payload + position, this->applicationDataLength);
      }
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}