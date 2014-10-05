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

#ifndef __RTP_PACKET_DEFINED
#define __RTP_PACKET_DEFINED

/* RTP packet header

+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|         bit / byte          | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 - 31 |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              0              | V | V | P | X | C | C | C | C | M | T |  T |  T |  T |  T |  T |  T |   SN    |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              4              |                                   timestamp                                   |
+-----------------------------+-------------------------------------------------------------------------------+
|              8              |                                SSRC identifier                                |
+-----------------------------+-------------------------------------------------------------------------------+
|             12              |                               CSRC identifiers                                |
+-----------------------------+---------------------------------------------------------------------+---------+
|      12 + X * 4 * CCCC      |                profile specific extension header ID                 |   EHL   |
+-----------------------------+---------------------------------------------------------------------+---------+
|      16 + X * 4 * CCCC      |                               extension header                                |
+-----------------------------+-------------------------------------------------------------------------------+
|  16 + X * 4 * (CCCC + EHL)  |                                     data                                      |
+-----------------------------+-------------------------------------------------------------------------------+

VV: version, 2 bits, indicates the version of the protocol, current version is 2
P: padding, 1 bit, used to indicate if there are extra padding bytes at the end of the RTP packet.
   A padding might be used to fill up a block of certain size, for example as required by an encryption algorithm.
X: extension, 1 bit, indicates presence of an extension header between standard header and payload data.
   This is application or profile specific.
CCCC: CSRC count, 4 bits, contains the number of CSRC identifiers (defined below) that follow the fixed header
M: marker, 1 bit, used at the application level and defined by a profile.
   If it is set, it means that the current data has some special relevance for the application.
TTTTTTT: payload type, 7 bits, indicates the format of the payload and determines its interpretation by the application.
         This is specified by an RTP profile.
SN: sequence number, 16 bits, the sequence number is incremented by one for each RTP data packet sent and is to be used by the receiver to detect
    packet loss and to restore packet sequence. The RTP does not specify any action on packet loss; it is left to the application to take
    appropriate action. For example, video applications may play the last known frame in place of the missing frame. According to RFC 3550,
    the initial value of the sequence number should be random to make known-plaintext attacks on encryption more difficult. RTP provides no guarantee
    of delivery, but the presence of sequence numbers makes it possible to detect missing packets.
timestamp: timestamp, 32 bits, used to enable the receiver to play back the received samples at appropriate intervals. When several media streams
           are present, the timestamps are independent in each stream, and may not be relied upon for media synchronization. The granularity
           of the timing is application specific. For example, an audio application that samples data once every 125 us (8 kHz, a common sample
           rate in digital telephony) could use that value as its clock resolution. The clock granularity is one of the details that is specified
           in the RTP profile for an application.
SSRC identifier: synchronization source identifier, 32 bits, synchronization source identifier uniquely identifies the source of a stream.
                 The synchronization sources within the same RTP session will be unique.
CSRC identifiers: contributing source IDs enumerate contributing sources to a stream which has been generated from multiple sources
Extension header: optional, the first 32-bit word contains a profile-specific identifier (16 bits) and a length specifier (16 bits) that indicates
                  the length of the extension (EHL = extension header length) in 32-bit units, excluding the 32 bits of the extension header
*/

#include "BaseRtpPacket.h"
#include "ContributeSourceIdentifierCollection.h"

#define RTP_PACKET_BASE_TYPE                                            0x00000001

#define RTP_PACKET_HEADER_SIZE                                          12              // length of the RTP header in bytes
#define RTP_PACKET_VERSION                                              2               // RTP packet version
#define RTP_PACKET_MAXIMUM_PADDING_LENGTH                               255             // maximum RTP packet padding length
#define RTP_PACKET_MAXIMUM_PROFILE_SPECIFIC_EXTENSION_HEADER_ID         0xFFFF          // maximum profile specific extension header ID
#define RTP_PACKET_MAXIMUM_EXTENSION_HEADER_LENGTH_IN_32BIT_WORDS       0xFFFF          // maximum extension header length in 32bit words
#define RTP_PACKET_MAXIMUM_PAYLOAD_TYPE                                 0x7F            // maximum RTP packet payload type
#define RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER                              0xFFFF          // maximum RTP packet sequence number
#define RTP_PACKET_MAXIMUM_CONTRIBUTING_SOURCE_ID_COUNT                 0x0F            // maximum RTP packet contributing source ID count

#define RTP_PACKET_FLAG_NONE                                            BASE_RTP_PACKET_FLAG_NONE

#define RTP_PACKET_FLAG_EXTENSION_HEADER                                (1 << (BASE_RTP_PACKET_FLAG_LAST + 0))
#define RTP_PACKET_FLAG_MARKER                                          (1 << (BASE_RTP_PACKET_FLAG_LAST + 1))

#define RTP_PACKET_FLAG_LAST                                            (BASE_RTP_PACKET_FLAG_LAST + 2)

class CRtpPacket : public CBaseRtpPacket
{
public:
  // initializes a new instance of CRtpPacket class
  CRtpPacket(HRESULT *result);
  virtual ~CRtpPacket(void);

  /* get methods */

  // gets packet size
  // @return : packet size or UINT_MAX if error
  virtual unsigned int GetSize(void);

  // gets payload type
  // @return : payload type or UINT_MAX if error
  virtual unsigned int GetPayloadType(void);

  // gets sequence number
  // @return : sequence number or UINT_MAX if error
  virtual unsigned int GetSequenceNumber(void);

  // gets timestamp
  // @return : timestamp (no error value)
  virtual unsigned int GetTimestamp(void);

  // gets synchronization source identifier
  // @return : synchronization source identifier of UINT_MAX if error
  virtual unsigned int GetSynchronizationSourceIdentifier(void);

  // gets payload data
  // @return : payload data or NULL if error
  virtual const unsigned char *GetPayload(void);

  // gets payload size
  // @return : payload size
  virtual unsigned int GetPayloadSize(void);

  /* set methods */

  // sets timestamp
  // @param timestamp : the timestamp to set
  virtual void SetTimestamp(unsigned int timestamp);

  /* other methods */

  // tests if packet has extension header between standard header and payload data
  // @return : true if packet has extension header, false otherwise
  virtual bool IsExtended(void);

  // tests if packet has set marker bit
  // @return : true if packet has marker bit set, false otherwise
  virtual bool IsMarked(void);

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with RTP packet data for parsing (there must be only one RTP packet)
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  virtual CRtpPacket *Clone(void);

protected:

  // RTP packet header

  unsigned int payloadType;
  unsigned int sequenceNumber;
  unsigned int timestamp;
  unsigned int synchronizationSourceIdentifier;
  CContributeSourceIdentifierCollection *contributeSourceIdentifiers;
  unsigned int profileSpecificExtensionHeaderId;

  unsigned char *extensionHeader;
  unsigned int extensionHeaderLength;

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