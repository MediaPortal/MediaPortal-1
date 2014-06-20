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

#ifndef __MPEG2_TRANSPORT_STREAM_RTP_PACKET_DEFINED
#define __MPEG2_TRANSPORT_STREAM_RTP_PACKET_DEFINED

#include "RtpPacket.h"

#define MPEG2_TRANPORT_STREAM_PAYLOAD_TYPE_DEFAULT                    33

class CMpeg2TransportStreamRtpPacket : public CRtpPacket
{
public:
  // initializes a new instance of CMpeg2TransportStreamRtpPacket class
  CMpeg2TransportStreamRtpPacket(HRESULT *result);
  virtual ~CMpeg2TransportStreamRtpPacket(void);

  /* get methods */

  /* set methods */

  /* other methods */

  // parses data in buffer
  // @param buffer : buffer with RTP packet data for parsing (there must be only one RTP packet)
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  /* methods */

  // creates RTP packet instance for cloning
  // @return : new RTP packet instance or NULL if error
  virtual CRtpPacket *CreateRtpPacket(void);

  // deeply clones current instance to specified RTP packet
  // @param rtpPacket : the RTP packet to clone current instance
  // @result : true if successful, false otherwise
  virtual bool CloneInternal(CRtpPacket *rtpPacket);
};

#endif