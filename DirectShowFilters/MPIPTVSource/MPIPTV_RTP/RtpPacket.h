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

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the MPIPTV_RTP_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// MPIPTV_RTP_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.

// RTP packet header
/*

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
           of the timing is application specific. For example, an audio application that samples data once every 125 ?s (8 kHz, a common sample
           rate in digital telephony) could use that value as its clock resolution. The clock granularity is one of the details that is specified
           in the RTP profile for an application.
SSRC identifier: synchronization source identifier, 32 bits, synchronization source identifier uniquely identifies the source of a stream.
                 The synchronization sources within the same RTP session will be unique.
CSRC identifiers: contributing source IDs enumerate contributing sources to a stream which has been generated from multiple sources
Extension header: optional, the first 32-bit word contains a profile-specific identifier (16 bits) and a length specifier (16 bits) that indicates
                  the length of the extension (EHL = extension header length) in 32-bit units, excluding the 32 bits of the extension header
*/

#pragma once

#ifndef __RTP_PACKET_DEFINE_DEFINED
#define __RTP_PACKET_DEFINE_DEFINED

#include "MPIPTV_RTP_Exports.h"

// length of the RTP header in byte
#define RTP_HEADER_SIZE                                                 12
// RTP packet version
#define RTP_PACKET_VERSION                                              2
// maximum RTP packet padding length
#define RTP_PACKET_MAXIMUM_PADDING_LENGTH                               255
// maximum profile specific extension header ID
#define RTP_PACKET_MAXIMUM_PROFILE_SPECIFIC_EXTENSION_HEADER_ID         0xFFFF
// maximum extension header length in 32bit words
#define RTP_PACKET_MAXIMUM_EXTENSION_HEADER_LENGTH_IN_32BIT_WORDS       0xFFFF
// maximum RTP packet payload type
#define RTP_PACKET_MAXIMUM_PAYLOAD_TYPE                                 0x7F
// maximum RTP packet sequence number
#define RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER                              0xFFFF
// maximum RTP packet contributing source ID count
#define RTP_PACKET_MAXIMUM_CONTRIBUTING_SOURCE_ID_COUNT                 0x0F

class MPIPTV_RTP_API RtpPacket
{
private:
  char *data;
  unsigned int length;
  RtpPacket *nextPacket;
  RtpPacket *previousPacket;

public:
  RtpPacket(const char *buffer, unsigned int length, RtpPacket *previousPacket, RtpPacket *nextPacket);
  ~RtpPacket(void);

  // RTP packet header methods

  // get RTP packet version
  // @return : version or UINT_MAX if error
  unsigned int GetVersion(void);

  // set RTP packet version
  // @param version : the RTP packet version
  // @return : true if successful, false otherwise
  bool SetVersion(unsigned int version);

  // get RTP packet padding bit
  // @return : padding bit or UINT_MAX if error
  unsigned int GetPadding(void);

  // set RTP packet padding with resizing packet
  // @param length : the request length of padding, if zero than padding is removed from packet
  // @return : true if successful, false otherwise
  bool SetPadding(unsigned int length);

  // get RTP packet padding length
  // @return : padding length or UINT_MAX if error
  unsigned int GetPaddingLength(void);

  // check if RTP packet has padding
  // @return : true if successful, false otherwise
  bool IsPadding(void);

  // get RTP packet extension header bit
  // @return : extension header bit or UINT_MAX if error
  unsigned int GetExtensionHeader(void);

  // get RTP packet profile specific extension header ID
  // @return : profile specific extension header ID or UINT_MAX if error
  unsigned int GetProfileSpecificExtensionHeaderId(void);

  // get RTP packet extension header length
  // @return : extension header length or UINT_MAX if error
  unsigned int GetExtensionHeaderLength(void);

  // get RTP packet extension header full length (with profile-specific identifier and length specifier)
  // @return : extension header length or UINT_MAX if error
  unsigned int GetExtensionHeaderFullLength(void);

  // set RTP packet extension header
  // @param profileSpecificExtensionHeaderId : the profile specific extension header ID
  // @param extensionHeader : the buffer with extension header data
  // @param length : the length of extension header buffer
  // @return : true if successful, false otherwise
  bool SetExtensionHeader(unsigned int profileSpecificExtensionHeaderId, char *extensionHeader, unsigned int length);

  // check if RTP packet has extension header
  // @return : true if successful, false otherwise
  bool IsExtensionHeader(void);

  // get RTP packet contributing source ID count
  // @return : count or UINT_MAX if error
  unsigned int GetContributingSourceIdCount(void);

  // set RTP packet contributing source IDs
  // @param contributingSources : pointer to array of contributing source IDs
  // @param length : the length of array (count of contributing sources)
  // @return : true if successful, false otherwise
  bool SetContributingSourceId(unsigned int *contributingSources, unsigned int length);

  // get RTP packet marker bit
  // @return : marker bit or UINT_MAX if error
  unsigned int GetMarker(void);

  // set RTP packet marker bit
  // @return : true if successful, false otherwise
  bool SetMarker(void);

  // clear RTP packet marker bit
  // @return : true if successful, false otherwise
  bool ClearMarker(void);

  // check if RTP packet has marker
  // @return : true if successful, false otherwise
  bool IsMarker(void);

  // get RTP packet payload type
  // @return : payload type or UINT_MAX if error
  unsigned int GetPayloadType(void);

  // set RTP packet payload type
  // @param payloadType : the RTP packet payload type
  // @return : true if successful, false otherwise
  bool SetPayloadType(unsigned int payloadType);

  // get RTP packet sequence number
  // @return : sequence number or UINT_MAX if error
  unsigned int GetSequenceNumber(void);

  // set RTP packet sequence number
  // @param sequenceNumber : the RTP packet sequence number
  // @return : true if successful, false otherwise
  bool SetSequenceNumber(unsigned int sequenceNumber);

  // get RTP packet timestamp
  // @return : timestamp or UINT_MAX if error
  unsigned int GetTimestamp(void);

  // set RTP packet timestamp
  // @param timestamp : the RTP packet timestamp
  // @return : true if successful, false otherwise
  bool SetTimestamp(unsigned int timestamp);

  // get RTP packet source identifier
  // @return : source identifier of UINT_MAX if error
  unsigned int GetSourceIdentifier(void);

  // set RTP packet source identifier
  // @param sourceIdentifier : the RTP packet source identifier
  // @return : true if successful, false otherwise
  bool SetSourceIdentifier(unsigned int sourceIdentifier);

  // data methods

  // copy RTP packet extension header data
  // @param buffer : pointer to buffer to copy data
  // @param length : the length of buffer
  // @return : the length of copied extension header data or UINT_MAX if error
  unsigned int GetExtensionHeaderData(char *buffer, unsigned int length);

  // get RTP packet data length
  // @return : length of data in RTP packet or UINT_MAX if error
  unsigned int GetDataLength(void);

  // copy RTP packet data
  // @param buffer : pointer to buffer to copy data
  // @param length : the length of buffer
  // @return : the length of copied data, UINT_MAX if error
  unsigned int GetData(char *buffer, unsigned int length);

  // set RTP packet data
  // @param buffer : pointer to buffer containing data
  // @param length : the length of buffer
  // @return : true if successful, false otherwise
  bool SetData(char *buffer, unsigned int length);

  // copy whole RTP packet
  // @param buffer : pointer to buffer to copy data
  // @param length : the length of buffer
  // @return : the length of copied data, UINT_MAX if error
  unsigned int GetPacketData(char *buffer, unsigned int length);

  // RTP packet methods

  // deep clone of RTP packet without reference to previous and next packet
  // @return : the pointer to clone of RTP packet
  RtpPacket *Clone(void);

  // get RTP packet length
  // @return : RTP packet length
  unsigned int GetPacketLength(void);

  // checks if this is a valid RTP packet
  // @return : true if successful, false otherwise
  bool IsRtpPacket(void);

  // RTP packet link methods

  // get next RTP packet
  // @return : next RTP packet (NULL if next packet doesn't exist)
  RtpPacket *GetNextPacket(void);

  // set next RTP packet
  // @param rtpPacket : the pointer to next RTP packet
  void SetNextPacket(RtpPacket *rtpPacket);

  // get previous RTP packet
  // @return : previous RTP packet (NULL if previous packet doesn't exist)
  RtpPacket *GetPreviousPacket(void);

  // set previous RTP packet
  // @param rtpPacket : the pointer to previous RTP packet
  void SetPreviousPacket(RtpPacket *rtpPacket);
};

#endif
