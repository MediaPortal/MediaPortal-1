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
  this->senderSynchronizationSourceIdentifier = 0;
  this->name = NULL;
  this->applicationData = NULL;
  this->applicationDataSize = 0;

  this->packetType = APPLICATION_RTCP_PACKET_TYPE;
}

CApplicationRtcpPacket::~CApplicationRtcpPacket(void)
{
  FREE_MEM(this->name);
  FREE_MEM(this->applicationData);
}

/* get methods */

unsigned int CApplicationRtcpPacket::GetPacketValue(void)
{
  return this->GetApplicationSubtype();
}

unsigned int CApplicationRtcpPacket::GetPacketType(void)
{
  return APPLICATION_RTCP_PACKET_TYPE;
}

unsigned int CApplicationRtcpPacket::GetSenderSynchronizationSourceIdentifier(void)
{
  return this->senderSynchronizationSourceIdentifier;
}

const wchar_t *CApplicationRtcpPacket::GetName(void)
{
  return this->name;
}

unsigned int CApplicationRtcpPacket::GetApplicationSubtype(void)
{
  return this->packetValue;
}

unsigned char *CApplicationRtcpPacket::GetApplicationData(void)
{
  return this->applicationData;
}

unsigned int CApplicationRtcpPacket::GetApplicationDataSize(void)
{
  return this->applicationDataSize;
}

unsigned int CApplicationRtcpPacket::GetSize(void)
{
  return (__super::GetSize() + APPLICATION_RTCP_PACKET_HEADER_SIZE + this->GetApplicationDataSize());
}

bool CApplicationRtcpPacket::GetPacket(unsigned char *buffer, unsigned int length)
{
  bool result = __super::GetPacket(buffer, length);

  if (result)
  {
    unsigned int position = __super::GetSize();

    WBE32INC(buffer, position, this->GetSenderSynchronizationSourceIdentifier());

    // write first 4 characters of name
    char *temp = ConvertUnicodeToUtf8(this->GetName());
    if (temp != NULL)
    {
      unsigned int tempSize = strlen(temp);

      buffer[position] = (0 < tempSize) ? temp[0] : 0;
      buffer[position + 1] = (1 < tempSize) ? temp[1] : 0;
      buffer[position + 2] = (2 < tempSize) ? temp[2] : 0;
      buffer[position + 3] = (3 < tempSize) ? temp[3] : 0;
    }
    FREE_MEM(temp);
    position += 4;

    memcpy(buffer + position, this->GetApplicationData(), this->GetApplicationDataSize());
  }

  return result;
}

/* set methods */

void CApplicationRtcpPacket::SetSenderSynchronizationSourceIdentifier(unsigned int senderSynchronizationSourceIdentifier)
{
  this->senderSynchronizationSourceIdentifier = senderSynchronizationSourceIdentifier;
}

void CApplicationRtcpPacket::SetApplicationSubtype(unsigned int applicationSubtype)
{
  this->packetValue = applicationSubtype;
}

bool CApplicationRtcpPacket::SetName(const wchar_t *name)
{
  SET_STRING_RETURN_WITH_NULL(this->name, name);
}

bool CApplicationRtcpPacket::SetApplicationData(unsigned char *applicationData, unsigned int applicationDataSize)
{
  bool result = true;
  FREE_MEM(this->applicationData);
  this->applicationDataSize = applicationDataSize;

  if (this->applicationDataSize != 0)
  {
    result &= ((applicationData != NULL) && ((this->applicationDataSize % 4) == 0));

    if (result)
    {
      this->applicationData =  ALLOC_MEM_SET(this->applicationData, unsigned char, this->applicationDataSize, 0);
      result &= (this->applicationData != NULL);

      if (result)
      {
        memcpy(this->applicationData, applicationData, this->applicationDataSize);
      }
    }
  }

  if (!result)
  {
    FREE_MEM(this->applicationData);
    this->applicationDataSize = 0;
  }

  return result;
}

/* other methods */

void CApplicationRtcpPacket::Clear(void)
{
  __super::Clear();

  this->senderSynchronizationSourceIdentifier = 0;
  this->applicationDataSize = 0;
  FREE_MEM(this->name);
  FREE_MEM(this->applicationData);

  this->packetType = APPLICATION_RTCP_PACKET_TYPE;
}

bool CApplicationRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->packetType == APPLICATION_RTCP_PACKET_TYPE);
  result &= (this->payloadSize >= APPLICATION_RTCP_PACKET_HEADER_SIZE);

  if (result)
  {
    // application RTCP packet header is at least APPLICATION_RTCP_PACKET_HEADER_SIZE long
    unsigned int position = 0;

    RBE32INC(this->payload, position, this->senderSynchronizationSourceIdentifier);
    
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

    this->applicationDataSize = this->payloadSize - position;

    if (result && (this->applicationDataSize > 0))
    {
      // there are still some data, it have to be application data
      this->applicationData = ALLOC_MEM_SET(this->applicationData, unsigned char, this->applicationDataSize, 0);
      result &= (this->applicationData != NULL);

      if (result)
      {
        memcpy(this->applicationData, this->payload + position, this->applicationDataSize);
      }
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}