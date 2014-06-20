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

#include "Mpeg2TransportStreamRtpPacket.h"

CMpeg2TransportStreamRtpPacket::CMpeg2TransportStreamRtpPacket(HRESULT *result)
  : CRtpPacket(result)
{
}

CMpeg2TransportStreamRtpPacket::~CMpeg2TransportStreamRtpPacket(void)
{
}

/* get methods */

/* set methods */

/* other methods */

bool CMpeg2TransportStreamRtpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);

  if (result)
  {
    // payload type should be MPEG2_TRANPORT_STREAM_PAYLOAD_TYPE_DEFAULT
    // maybe later will be implemented collection of accepted payload types (if necessary)

    result &= (this->payloadType == MPEG2_TRANPORT_STREAM_PAYLOAD_TYPE_DEFAULT);
  }

  return result;
}

/* protected methods */

CRtpPacket *CMpeg2TransportStreamRtpPacket::CreateRtpPacket(void)
{
  HRESULT result = S_OK;
  CMpeg2TransportStreamRtpPacket *packet = new CMpeg2TransportStreamRtpPacket(&result);
  CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
  return packet;
}

bool CMpeg2TransportStreamRtpPacket::CloneInternal(CRtpPacket *rtpPacket)
{
  bool result = __super::CloneInternal(rtpPacket);
  CMpeg2TransportStreamRtpPacket *mpeg2TransportStreamRtpPacket = dynamic_cast<CMpeg2TransportStreamRtpPacket *>(rtpPacket);
  result &= (mpeg2TransportStreamRtpPacket != NULL);

  return result;
}