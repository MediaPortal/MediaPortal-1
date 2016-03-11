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

#ifndef __RTPSOURCE_DEFINED
#define __RTPSOURCE_DEFINED

#include "MPIPTV_RTP_Exports.h"
#include "Logger.h"
#include "RtpPacket.h"

// maximum difference between sequence numbers
// if difference between sequence numbers is bigger, then sequence numbers are in direction as they arrive
#define RTP_MAXIMUM_DIFFERENCE_SEQUENCE_NUMBER              ((RTP_PACKET_MAXIMUM_SEQUENCE_NUMBER + 1) / 4)

class MPIPTV_RTP_API RtpSource 
{

protected:
  CLogger *logger;
  RtpPacket *firstRtpPacket;

  // get internal reference to RTP packet
  // @param index : position of RTP packet from first RTP packet
  // @return : pointer to RTP packet or NULL if error
  RtpPacket *GetInternalRtpPacket(int index);

  // get internal reference to RTP packet with specified sequence number
  // @param sequenceNumber : the sequence number of packet to get
  // @return : pointer to RTP packet or NULL if error
  RtpPacket *GetInternalRtpPacket(unsigned int sequenceNumber);

  // remove internal RTP packet
  // @param rtpPacket : reference to internal RTP packet
  // @return : true if successful, false otherwise
  bool RemoveInternalRtpPacket(RtpPacket *rtpPacket);

public:
  RtpSource(CLogger *logger);
  ~RtpSource();

  // get packets from the buffer
  // @param buffer : buffer to copy packets data
  // @param length : the length of buffer
  // @param firstSequenceNumber: the first RTP packet sequence number
  // @param lastSequenceNumber: the last RTP packet sequence number
  // @param getUncontinousPackets : specifies if uncontinous packets (packets which sequence number do not follow) have to be copied or copying stops on first uncontinous packet
  // @return : the length of copied data, UINT_MAX if error 
  unsigned int GetPacketData(char* buffer, unsigned int length, unsigned int *firstSequenceNumber, unsigned int *lastSequenceNumber, bool getUncontinousPackets);

  // get packets from the buffer and remove RTP packets
  // @param buffer : buffer to copy packets data
  // @param length : the length of buffer
  // @param firstSequenceNumber: the first RTP packet sequence number
  // @param lastSequenceNumber: the last RTP packet sequence number
  // @param getUncontinousPackets : specifies if uncontinous packets (packets which sequence number do not follow) have to be copied or copying stops on first uncontinous packet
  // @return : the length of copied data, UINT_MAX if error 
  unsigned int GetAndRemovePacketData(char* buffer, unsigned int length, unsigned int *firstSequenceNumber, unsigned int *lastSequenceNumber, bool getUncontinousPackets);

  // check if in buffer is a valid RTP packet
  // @param buffer : buffer containing RTP packet
  // @param length : the length of buffer
  // @return : true if successful, false otherwise
  bool IsRtpPacket(char *buffer, unsigned int length);

  // process the RTP packet and add it to list
  // only valid RTP packets are added to list
  // @param buffer : buffer containing RTP packet
  // @param length : the length of buffer
  // @return : true if successful, false otherwise
  bool ProcessPacket(char *buffer, unsigned int length);

  // add one RTP packet to correct place in chain of RTP packets
  // @param rtpPacket : the RTP packet to add
  // @return : true if successful, false otherwise
  bool AddPacket(RtpPacket *rtpPacket);

  // remove RTP packet
  // @param index : position of RTP packet from first RTP packet
  // @return : true if successful, false otherwise
  bool RemoveRtpPacket(int index);

  // remove RTP packet with specifes sequence number
  // @param sequenceNumber : the sequence number of packet to be removed
  // @return : true if successful, false otherwise
  bool RemoveRtpPacket(unsigned int sequenceNumber);

  // get RTP packet
  // RTP packet is deep clone
  // @param index : position of RTP packet from first RTP packet
  // @return : pointer to RTP packet or NULL if error
  RtpPacket *GetRtpPacket(int index);

  // get RTP packet with specified sequence number
  // RTP packet is deep clone
  // @param sequenceNumber : the sequence number of packet to get
  // @return : pointer to RTP packet or NULL if error
  RtpPacket *GetRtpPacket(unsigned int sequenceNumber);

  // check if sequence numbers are continous (if second is right first sequence number)
  // @param firstSequenceNumber : first sequence number
  // @param secondSequenceNumber : second sequence number
  // @return : true if sequence is continous
  bool IsSequenceContinuous(unsigned int firstSequenceNumber, unsigned int secondSequenceNumber);

  // check if RTP packet is between previous and next packet
  // @param currentPacket : the RTP packet to check
  // @param previousPacket : the previous RTP packet
  // @param nextPacket : the next RTP packet
  // @return : true if packet is between previous packet and next packet
  bool IsPacketBetween(RtpPacket *currentPacket, RtpPacket *previousPacket, RtpPacket *nextPacket);

  // compute difference between sequence numbers of two RTP packets
  // @param firstPacket : the first RTP packet
  // @param secondPacket : the second RTP packet
  // @return : difference between sequence numbers of packets or INT_MAX if error, if difference is positive then second packet is after first packet
  int PacketSequenceNumberDifference(RtpPacket *firstPacket, RtpPacket *secondPacket);

  // get packet count in source
  // @return : the count of packet in source or UINT_MAX if error
  unsigned int GetPacketCount(void);
};

#endif
