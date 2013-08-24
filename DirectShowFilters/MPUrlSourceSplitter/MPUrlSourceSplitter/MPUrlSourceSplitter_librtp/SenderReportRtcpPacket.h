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

#ifndef __SENDER_REPORT_RTCP_PACKET_DEFINED
#define __SENDER_REPORT_RTCP_PACKET_DEFINED

/* sender report RTCP packet

+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|         bit / byte          | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 - 31 |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              0              |  VV   | P |        PV         |                 PT                  | length  |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              4              |                                 SSRC of sender                                |
+-----------------------------+-------------------------------------------------------------------------------+
|              8              |                      NTP timestamp, most significant word                     |
+-----------------------------+-------------------------------------------------------------------------------+
|             12              |                      NTP timestamp, least significant word                    |
+-----------------------------+-------------------------------------------------------------------------------+
|             16              |                                 RTP timestamp                                 |
+-----------------------------+-------------------------------------------------------------------------------+
|             20              |                             sender's packet count                             |
+-----------------------------+-------------------------------------------------------------------------------+
|             24              |                              sender's octet count                             |
+-----------------------------+-------------------------------------------------------------------------------+
| REPORT BLOCK X (24 bytes), X = 1 .. PV                                                                      |
+-----------------------------+-------------------------------------------------------------------------------+
|      28 + (X - 1) * 24      |                                      SSRC                                     |
+-----------------------------+-------------------------------+-----------------------------------------------+
|      32 + (X - 1) * 24      |         fraction lost         |       cumulative number of packets lost       |
+-----------------------------+-------------------------------+-----------------------------------------------+
|      36 + (X - 1) * 24      |                   extended highest sequence number received                   |
+-----------------------------+-------------------------------------------------------------------------------+
|      40 + (X - 1) * 24      |                              interarrival jitter                              |
+-----------------------------+-------------------------------------------------------------------------------+
|      44 + (X - 1) * 24      |                                 last SR (LSR)                                 |
+-----------------------------+-------------------------------------------------------------------------------+
|      48 + (X - 1) * 24      |                          delay since last SR (DLSR)                           |
+-----------------------------+-------------------------------------------------------------------------------+
|         28 + X * 24         |                          profile-specific extensions                          |
+-----------------------------+-------------------------------------------------------------------------------+

VV, P, PV, PT and length : same as for RTCP packet (RtcpPacket.h)

PV: packet value, 5 bits, reception report count, the number of reception report blocks contained in this packet,  value of zero is valid
PT: packet type, 8 bits, constant value SENDER_REPORT_RTCP_PACKET_TYPE

SSRC of sender: synchronization source identifier, 32 bits, synchronization source identifier for the originator of this SR (sender report) packet
NTP timestamp: 64 bits
RTP timestamp: 32 bits

sender's packet count: 32 bits
  The total number of RTP data packets transmitted by the sender since starting transmission up until the time this SR packet was
  generated.  The count SHOULD be reset if the sender changes its SSRC identifier. 

sender's octet count: 32 bits
  The total number of payload octets (i.e., not including header or padding) transmitted in RTP data packets by the sender since
  starting transmission up until the time this SR packet was generated.  The count SHOULD be reset if the sender changes its
  SSRC identifier.  This field can be used to estimate the average payload data rate. 

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

#define SENDER_REPORT_RTCP_PACKET_HEADER_SIZE                           24              // length of the sender report RTCP header (until first block) in bytes
#define SENDER_REPORT_RTCP_PACKET_TYPE                                  0xC8            // sender report RTCP packet type

#define SENDER_REPORT_REPORT_BLOCK_SIZE                                 24

#define FLAG_SENDER_REPORT_RTCP_PACKET_NONE                             FLAG_RTCP_PACKET_NONE
#define FLAG_SENDER_REPORT_RTCP_PACKET_PADDING                          FLAG_RTCP_PACKET_PADDING
#define FLAG_SENDER_REPORT_RTCP_PACKET_PROFILE_EXTENSIONS               0x00000002

class CSenderReportRtcpPacket : public CRtcpPacket
{
public:
  // initializes a new instance of CSenderReportRtcpPacket class
  CSenderReportRtcpPacket(void);
  virtual ~CSenderReportRtcpPacket(void);

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

  virtual uint64_t GetNtpTimestamp(void);
  virtual unsigned int GetRtpTimestamp(void);
  virtual unsigned int GetSenderPacketCount(void);
  virtual unsigned int GetSenderOctetCount(void);

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

  // sets synchronization source identifier of sender
  // @param senderSynchronizationSourceIdentifier : synchronization source identifier of sender to set
  virtual void SetSenderSynchronizationSourceIdentifier(unsigned int senderSynchronizationSourceIdentifier);

  virtual void SetNtpTimestamp(uint64_t ntpTimestamp);
  virtual void SetRtpTimestamp(unsigned int rtpTimestamp);
  virtual void SetSenderPacketCount(unsigned int senderPacketCount);
  virtual void SetSenderOctetCount(unsigned int senderOctetCount);

  /* other methods */

  // tests if sender report packet has profile specific extensions part
  // @return : true if sender report packet has profile specific extensions part, false otherwise
  virtual bool HasProfileSpecificExtensions(void);

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with packet data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // sender report RTCP packet

  unsigned int senderSynchronizationSourceIdentifier;
  uint64_t ntpTimestamp;
  unsigned int rtpTimestamp;
  unsigned int senderPacketCount;
  unsigned int senderOctetCount;

  unsigned char *profileSpecificExtensions;
  unsigned int profileSpecificExtensionsLength;

  CReportBlockCollection *reportBlocks;
};

#endif