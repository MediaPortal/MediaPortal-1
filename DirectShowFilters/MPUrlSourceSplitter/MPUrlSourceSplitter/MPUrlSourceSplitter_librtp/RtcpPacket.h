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

#ifndef __RTCP_PACKET_DEFINED
#define __RTCP_PACKET_DEFINED

/* RTCP packet header

+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|         bit / byte          | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 - 31 |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              0              |  VV   | P |        PV         |                 PT                  | length  |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+

VV: version, 2 bits, indicates the version of the protocol, current version is 2
P: padding, 1 bit, used to indicate if there are extra padding bytes at the end of the RTCP packet

From RFC 3550:
      If the padding bit is set, this individual RTCP packet contains
      some additional padding octets at the end which are not part of
      the control information but are included in the length field.  The
      last octet of the padding is a count of how many padding octets
      should be ignored, including itself (it will be a multiple of
      four).  Padding may be needed by some encryption algorithms with
      fixed block sizes.  In a compound RTCP packet, padding is only
      required on one individual packet because the compound packet is
      encrypted as a whole for the method in Section 9.1.  Thus, padding
      MUST only be added to the last individual packet, and if padding
      is added to that packet, the padding bit MUST be set only on that
      packet.  This convention aids the header validity checks described
      in Appendix A.2 and allows detection of packets from some early
      implementations that incorrectly set the padding bit on the first
      individual packet and add padding to the last individual packet. 

PV: packet value, 5 bits, meaning of PV is different for each type of RTCP packet

PT: packet type, 8 bits

length: length, 16 bits

From RFC 3550:
      The length of this RTCP packet in 32-bit words minus one,
      including the header and any padding.  (The offset of one makes
      zero a valid length and avoids a possible infinite loop in
      scanning a compound RTCP packet, while counting 32-bit words
      avoids a validity check for a multiple of 4.) 

*/

#include "BaseRtpPacket.h"

#define RTCP_PACKET_BASE_TYPE                                           0x00000002

#define RTCP_PACKET_HEADER_SIZE                                         4               // length of the RTCP header in bytes
#define RTCP_PACKET_VERSION                                             2               // RTCP packet version

#define RTCP_PACKET_FLAG_NONE                                           BASE_RTP_PACKET_FLAG_NONE

#define RTCP_PACKET_FLAG_PADDING                                        BASE_RTP_PACKET_FLAG_PADDING

#define RTCP_PACKET_FLAG_LAST                                           (BASE_RTP_PACKET_FLAG_LAST + 0)

class CRtcpPacket : public CBaseRtpPacket
{
public:
  // initializes a new instance of CRtcpPacket
  CRtcpPacket(HRESULT *result);
  virtual ~CRtcpPacket(void);

  /* get methods */

  // gets packet value
  // @return : packet value
  virtual unsigned int GetPacketValue(void) = 0;

  // gets packet type
  // @return : packet type
  virtual unsigned int GetPacketType(void) = 0;

  // gets RTCP packet size
  // @return : RTCP packet size
  virtual unsigned int GetSize(void);

  // gets RTCP packet content
  // for creating packets is assumed that derived RTCP packets has payload length zero and payload is NULL
  // @param buffer : the buffer to store RTCP packet content
  // @param length : the length of buffer to store RTCP packet
  // @return : true if successful, false otherwise
  virtual bool GetPacket(unsigned char *buffer, unsigned int length);

  /* set methods */

  /* other methods */

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with packet data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // RTCP packet header
  unsigned int packetValue;
  unsigned int packetType;

  // payload length + RTCP_PACKET_HEADER_SIZE must be multiple of four

  /* get methods */

  /* set methods */

  /* other methods */
};

#endif