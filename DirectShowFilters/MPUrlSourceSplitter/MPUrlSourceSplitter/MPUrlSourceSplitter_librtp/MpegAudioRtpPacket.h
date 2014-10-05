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

#ifndef __MPEG_AUDIO_RTP_PACKET_DEFINED
#define __MPEG_AUDIO_RTP_PACKET_DEFINED

/*

   MPEG Audio-specific header

   This header shall be attached to each RTP packet at the start of the
   payload and after any RTP headers for an MPEG1/2 Audio payload type.

    0                   1                   2                   3
    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |             MBZ               |          Frag_offset          |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

           Frag_offset: Byte offset into the audio frame for the data
                        in this packet. 
*/

#include "RtpPacket.h"

#define MPEG_AUDIO_PAYLOAD_TYPE_DEFAULT                               14

#define MPEG_AUDIO_PAYLOAD_HEADER_LENGTH                              4

class CMpegAudioRtpPacket : public CRtpPacket
{
public:
  CMpegAudioRtpPacket(HRESULT *result);
  virtual ~CMpegAudioRtpPacket(void);

  /* get methods */

  // gets packet size
  // @return : packet size or UINT_MAX if error
  virtual unsigned int GetSize(void);

  // gets payload data
  // @return : payload data or NULL if error
  virtual const unsigned char *GetPayload(void);

  // gets payload size
  // @return : payload size
  virtual unsigned int GetPayloadSize(void);

  /* set methods */

  /* other methods */

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with RTP packet data for parsing (there must be only one RTP packet)
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // holds frag offset
  unsigned int fragOffset;

  /* methods */

  // creates RTP packet instance for cloning
  // @return : new RTP packet instance or NULL if error
  virtual CRtpPacket *CreateRtpPacket(void);

  // deeply clones current instance to specified RTP packet
  // @param rtpPacket : the RTP packet to clone current instance
  // @return : true if successful, false otherwise
  virtual bool CloneInternal(CRtpPacket *rtpPacket);
};

#endif