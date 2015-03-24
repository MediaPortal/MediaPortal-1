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

#ifndef __SOURCE_DESCRIPTION_RTCP_PACKET_DEFINED
#define __SOURCE_DESCRIPTION_RTCP_PACKET_DEFINED

/* source description RTCP packet

+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|         bit / byte          | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 - 31 |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              0              |  VV   | P |        PV         |                 PT                  | length  |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
| CHUNK X, X = 1 .. PV                                                                                        |
+-----------------------------+-------------------------------------------------------------------------------+
|                             |                                      SSRC                                     |
+-----------------------------+-------------------------------------------------------------------------------+
|                             |                                   SDES items                                  |
+-----------------------------+-------------------------------------------------------------------------------+

VV, P, PV, PT and length : same as for RTCP packet (RtcpPacket.h)

PV: packet value, 5 bits, the number of SSRC/CSRC chunks contained in this SDES packet, value of zero is valid but useless
PT: packet type, 8 bits, constant value SOURCE_DESCRIPTION_RTCP_PACKET_TYPE

SSRC of sender: synchronization source identifier, 32 bits, synchronization source identifier for the originator of this SR (sender report) packet

REPORT BLOCK:

SSRC: synchronization source identifier, 32 bits

  Each chunk consists of an SSRC/CSRC identifier followed by a list of zero or more items,
  which carry information about the SSRC/CSRC. Each chunk starts on a 32-bit boundary.
  Each item consists of an 8-bit type field, an 8-bit octet count describing the length of the
  text (thus, not including this two-octet header), and the text itself. Note that the text
  can be no longer than 255 octets, but this is consistent with the need to limit RTCP bandwidth consumption.

  The text is encoded according to the UTF-8 encoding specified in RFC 2279. US-ASCII is a subset of this encoding
  and requires no additional encoding. The presence of multi-octet encodings is indicated by setting
  the most significant bit of a character to a value of one.

  Items are contiguous, i.e., items are not individually padded to a 32-bit boundary. Text is not null
  terminated because some multi-octet encodings include null octets. The list of items in each chunk
  MUST be terminated by one or more null octets, the first of which is interpreted as an item type
  of zero to denote the end of the list. No length octet follows the null item type octet, but additional null
  octets MUST be included if needed to pad until the next 32-bit boundary. Note that this padding is separate
  from that indicated by the P bit in the RTCP header. A chunk with zero items (four null octets) is valid but useless.

*/

#include "RtcpPacket.h"
#include "SourceDescriptionChunkCollection.h"

#include <stdint.h>

#define SOURCE_DESCRIPTION_RTCP_PACKET_HEADER_SIZE                      0               // length of the source description RTCP header (until first block) in bytes
#define SOURCE_DESCRIPTION_RTCP_PACKET_TYPE                             0xCA            // source description RTCP packet type

class CSourceDescriptionRtcpPacket : public CRtcpPacket
{
public:
  // initializes a new instance of CSourceDescriptionRtcpPacket
  CSourceDescriptionRtcpPacket(HRESULT *result);
  virtual ~CSourceDescriptionRtcpPacket(void);

  /* get methods */

  // gets packet value
  // @return : packet value
  virtual unsigned int GetPacketValue(void);

  // gets packet type
  // @return : packet type
  virtual unsigned int GetPacketType(void);

  // gets RTCP packet size
  // @return : RTCP packet size
  virtual unsigned int GetSize(void);

  // gets RTCP packet content
  // @param buffer : the buffer to store RTCP packet content
  // @param length : the length of buffer to store RTCP packet
  // @return : true if successful, false otherwise
  virtual bool GetPacket(unsigned char *buffer, unsigned int length);

  // gets source description chunk collection
  // @return : source description chunk collection
  virtual CSourceDescriptionChunkCollection *GetChunks(void);

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

  // holds source description chunks
  CSourceDescriptionChunkCollection *chunks;
};

#endif