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

#ifndef __RECEIVER_REPORT_RTCP_PACKET_DEFINED
#define __RECEIVER_REPORT_RTCP_PACKET_DEFINED

/* receiver report RTCP packet

+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|         bit / byte          | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 - 31 |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              0              |  VV   | P |        PV         |                 PT                  | length  |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              4              |                                 SSRC of sender                                |
+-----------------------------+-------------------------------------------------------------------------------+
| REPORT BLOCK X (24 bytes), X = 1 .. PV                                                                      |
+-----------------------------+-------------------------------------------------------------------------------+
|       8 + (X - 1) * 24      |                                      SSRC                                     |
+-----------------------------+-------------------------------+-----------------------------------------------+
|      12 + (X - 1) * 24      |         fraction lost         |       cumulative number of packets lost       |
+-----------------------------+-------------------------------+-----------------------------------------------+
|      16 + (X - 1) * 24      |                   extended highest sequence number received                   |
+-----------------------------+-------------------------------------------------------------------------------+
|      20 + (X - 1) * 24      |                              interarrival jitter                              |
+-----------------------------+-------------------------------------------------------------------------------+
|      24 + (X - 1) * 24      |                                 last SR (LSR)                                 |
+-----------------------------+-------------------------------------------------------------------------------+
|      28 + (X - 1) * 24      |                          delay since last SR (DLSR)                           |
+-----------------------------+-------------------------------------------------------------------------------+
|          8 + X * 24         |                          profile-specific extensions                          |
+-----------------------------+-------------------------------------------------------------------------------+

VV, P, PV, PT and length : same as for RTCP packet (RtcpPacket.h)

PV: packet value, 5 bits, reception report count, the number of reception report blocks contained in this packet,  value of zero is valid
PT: packet type, 8 bits, constant value RECEIVER_REPORT_RTCP_PACKET_TYPE

SSRC of sender: synchronization source identifier, 32 bits, synchronization source identifier for the originator of this SR (sender report) packet

REPORT BLOCK:

SSRC: synchronization source identifier, 32 bits
  The SSRC identifier of the source to which the information in this reception report block pertains.

fraction lost: 8 bits
cumulative number of packets lost: 24 bits
extended highest sequence number received: 32 bits
interarrival jitter: 32 bits 
last SR timestamp (LSR): 32 bits
delay since last SR (DLSR): 32 bits

*/

#include "RtcpPacket.h"
#include "ReportBlockCollection.h"

#include <stdint.h>

#define RECEIVER_REPORT_RTCP_PACKET_HEADER_SIZE                         4               // length of the receiver report RTCP header (until first block) in bytes
#define RECEIVER_REPORT_RTCP_PACKET_TYPE                                0xC9            // receiver report RTCP packet type

#define RECEIVER_REPORT_REPORT_BLOCK_SIZE                               24

#define RECEIVER_REPORT_RTCP_PACKET_FLAG_NONE                           RTCP_PACKET_FLAG_NONE

#define RECEIVER_REPORT_RTCP_PACKET_FLAG_PADDING                        RTCP_PACKET_FLAG_PADDING
#define RECEIVER_REPORT_RTCP_PACKET_FLAG_PROFILE_EXTENSIONS             (1 << (RTCP_PACKET_FLAG_LAST + 0))

#define RECEIVER_REPORT_RTCP_PACKET_FLAG_LAST                           (RTCP_PACKET_FLAG_LAST + 1)

class CReceiverReportRtcpPacket : public CRtcpPacket
{
public:
  // initializes a new instance of CReceiverReportRtcpPacket class
  CReceiverReportRtcpPacket(HRESULT *result);
  virtual ~CReceiverReportRtcpPacket(void);

  /* get methods */

  // gets packet value
  // @return : packet value
  virtual unsigned int GetPacketValue(void);

  // gets packet type
  // @return : packet type
  virtual unsigned int GetPacketType(void);

  // gets synchronization source identifier of sender
  // @return : synchronization source identifier of sender
  virtual unsigned int GetSenderSynchronizationSourceIdentifier(void);

  virtual const unsigned char *GetProfileSpecificExtensions(void);

  virtual unsigned int GetProfileSpecificExtensionsLength(void);

  virtual CReportBlockCollection *GetReportBlocks(void);

  // gets RTCP packet size
  // @return : RTCP packet size
  virtual unsigned int GetSize(void);

  // gets RTCP packet content
  // @param buffer : the buffer to store RTCP packet content
  // @param length : the length of buffer to store RTCP packet
  // @return : true if successful, false otherwise
  virtual bool GetPacket(unsigned char *buffer, unsigned int length);

  /* set methods */

  // sets SSRC of receiver report RTCP packet
  // @param senderSynchronizationSourceIdentifier : SSRC of receiver report RTCP packet
  virtual void SetSenderSynchronizationSourceIdentifier(unsigned int senderSynchronizationSourceIdentifier);

  /* other methods */

  // tests if receiver report packet has profile specific extensions part
  // @return : true if receiver report packet has profile specific extensions part, false otherwise
  virtual bool HasProfileSpecificExtensions(void);

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with packet data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // receiver report RTCP packet

  unsigned int senderSynchronizationSourceIdentifier;

  unsigned char *profileSpecificExtensions;
  unsigned int profileSpecificExtensionsLength;

  CReportBlockCollection *reportBlocks;
};

#endif