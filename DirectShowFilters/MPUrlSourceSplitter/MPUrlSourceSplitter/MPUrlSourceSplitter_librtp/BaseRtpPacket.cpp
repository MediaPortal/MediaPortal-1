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

#include "BaseRtpPacket.h"
#include "BufferHelper.h"

#include <stdint.h>

CBaseRtpPacket::CBaseRtpPacket(void)
{
  this->flags = FLAG_BASE_RTP_PACKET_NONE;
  this->version = UINT_MAX;
  this->paddingSize = 0;
  this->baseType = UINT_MAX;
  this->payloadSize = 0;
  this->payload = NULL;
}

CBaseRtpPacket::~CBaseRtpPacket(void)
{
  FREE_MEM(this->payload);
}

/* get methods */

unsigned int CBaseRtpPacket::GetVersion(void)
{
  return this->version;
}

unsigned int CBaseRtpPacket::GetBaseType(void)
{
  return this->baseType;
}

unsigned int CBaseRtpPacket::GetSize(void)
{
  return BASE_RTP_PACKET_HEADER_SIZE;
}

/* set methods */

/* other methds */

bool CBaseRtpPacket::IsPadded(void)
{
  return ((this->flags & FLAG_BASE_RTP_PACKET_PADDING) != 0);
}

void CBaseRtpPacket::Clear(void)
{
  this->flags = FLAG_BASE_RTP_PACKET_NONE;
  this->version = UINT_MAX;
  this->paddingSize = 0;
  this->baseType = UINT_MAX;
  FREE_MEM(this->payload);
  this->payloadSize = 0;
}

bool CBaseRtpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = ((buffer != NULL) && (length >= BASE_RTP_PACKET_HEADER_SIZE));

  if (result)
  {
    // base packet header is at least BASE_RTP_PACKET_HEADER_SIZE long
    this->Clear();

    unsigned int position = 0;

    this->version = (RBE8(buffer, position) & 0xC0) >> 6;
    this->flags |= ((RBE8(buffer, position) & 0x20) != 0) ? FLAG_BASE_RTP_PACKET_PADDING : FLAG_BASE_RTP_PACKET_NONE;
    position++;

    // we don't set this->baseType because base RTP packet is mostly abstract class
  }

  return result;
}