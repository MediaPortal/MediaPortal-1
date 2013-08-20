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

#include "BaseRtpPacketFactory.h"
#include "RtpPacket.h"
#include "SenderReportRtcpPacket.h"
#include "ReceiverReportRtcpPacket.h"
#include "SourceDescriptionRtcpPacket.h"
#include "GoodbyeRtcpPacket.h"
#include "ApplicationRtcpPacket.h"
#include "Mpeg1OrMpeg2VideoRtpPacket.h"
#include "MpegAudioRtpPacket.h"
#include "Mpeg2TransportStreamRtpPacket.h"

CBaseRtpPacketFactory::CBaseRtpPacketFactory(void)
{
}

CBaseRtpPacketFactory::~CBaseRtpPacketFactory(void)
{
}

CBaseRtpPacket *CBaseRtpPacketFactory::CreateBaseRtpPacket(const unsigned char *buffer, unsigned int length, unsigned int *position)
{
  CBaseRtpPacket *result = NULL;
  bool continueParsing = ((buffer != NULL) && (length > 0) && (position != NULL));

  if (continueParsing)
  {
    *position = 0;

    // create RTCP packets
    CREATE_SPECIFIC_PACKET(CSenderReportRtcpPacket, buffer, length, continueParsing, result, (*position));
    CREATE_SPECIFIC_PACKET(CReceiverReportRtcpPacket, buffer, length, continueParsing, result, (*position));
    CREATE_SPECIFIC_PACKET(CSourceDescriptionRtcpPacket, buffer, length, continueParsing, result, (*position));
    CREATE_SPECIFIC_PACKET(CGoodbyeRtcpPacket, buffer, length, continueParsing, result, (*position));
    CREATE_SPECIFIC_PACKET(CApplicationRtcpPacket, buffer, length, continueParsing, result, (*position));

    // create RTP packet
    CREATE_SPECIFIC_PACKET(CMpeg2TransportStreamRtpPacket, buffer, length, continueParsing, result, (*position));
    CREATE_SPECIFIC_PACKET(CMpeg1OrMpeg2VideoRtpPacket, buffer, length, continueParsing, result, (*position));
    CREATE_SPECIFIC_PACKET(CMpegAudioRtpPacket, buffer, length, continueParsing, result, (*position));
    CREATE_SPECIFIC_PACKET(CRtpPacket, buffer, length, continueParsing, result, (*position));

    // never return base RTP packet
    continueParsing &= (result != NULL);
  }

  if (!continueParsing)
  {
    FREE_MEM_CLASS(result);
    *position = 0;
  }

  return result;
}