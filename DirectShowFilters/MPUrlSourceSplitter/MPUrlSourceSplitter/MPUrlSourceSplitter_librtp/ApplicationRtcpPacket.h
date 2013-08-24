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

#ifndef __APPLICATION_RTCP_PACKET_DEFINED
#define __APPLICATION_RTCP_PACKET_DEFINED

/* application RTCP packet

+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|         bit / byte          | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 - 31 |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              0              |  VV   | P |        PV         |                 PT                  | length  |
+-----------------------------+---+---+---+---+---+---+---+---+---+---+----+----+----+----+----+----+---------+
|              4              |                                    SSRC/CSRC                                  |
+-----------------------------+-------------------------------------------------------------------------------+
|              8              |                                   name (ASCII)                                |
+-----------------------------+-------------------------------------------------------------------------------+
|             12              |                            application depended data                          |
+-----------------------------+-------------------------------------------------------------------------------+

VV, P, PV, PT and length : same as for RTCP packet (RtcpPacket.h)

PV: packet value, 5 bits, subtype, may be used as a subtype to allow a set of APP packets to be defined under
  one unique name, or for any application-dependent data
PT: packet type, 8 bits, constant value APPLICATION_RTCP_PACKET_TYPE

SSRC/CSRC: synchronization source identifier or contribution source, 32 bits

name: 4 octets
  A name chosen by the person defining the set of APP packets to be unique with respect to other APP packets
  this application might receive. The application creator might choose to use the application name, and then
  coordinate the allocation of subtype values to others who want to define new packet types for the application.
  Alternatively, it is RECOMMENDED that others choose a name based on the entity they represent, then coordinate
  the use of the name within that entity. The name is interpreted as a sequence of four ASCII characters, with
  uppercase and lowercase characters treated as distinct.

application-dependent data: variable length
  Application-dependent data may or may not appear in an APP packet. It is interpreted by the application and
  not RTP itself. It MUST be a multiple of 32 bits long. 

*/

#include "RtcpPacket.h"

#define APPLICATION_RTCP_PACKET_HEADER_SIZE                             8               // length of the application RTCP header in bytes
#define APPLICATION_RTCP_PACKET_TYPE                                    0xCC            // application RTCP packet type

class CApplicationRtcpPacket : public CRtcpPacket
{
public:
  // initializes a new instance of CApplicationRtcpPacket
  CApplicationRtcpPacket(void);
  virtual ~CApplicationRtcpPacket(void);

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

  // gets name
  // @return : name
  virtual const wchar_t *GetName(void);

  // gets application subtype
  // @return : application subtype
  virtual unsigned int GetApplicationSubtype(void);

  // gets application data
  // @return : application data or NULL if not specified
  virtual unsigned char *GetApplicationData(void);

  // gets application data size
  // @return : application data size
  virtual unsigned int GetApplicationDataSize(void);

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

  // sets application subtype
  // @param applicationSubtype : application subtype to set
  virtual void SetApplicationSubtype(unsigned int applicationSubtype);

  // sets name
  // @param name : the name to set
  // @return : true if successful, false otherwise
  virtual bool SetName(const wchar_t *name);

  // sets application data
  // @param applicationData : the application data to set, can be NULL if applicationDataSize is zero
  // @param applicationDataSize : the size of application data, it MUST be a multiple of 32 bits
  // @return : true if successful, false otherwise
  virtual bool SetApplicationData(unsigned char *applicationData, unsigned int applicationDataSize);

  /* other methods */

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with packet data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  unsigned int senderSynchronizationSourceIdentifier;

  wchar_t *name;
  unsigned char *applicationData;
  unsigned int applicationDataSize;
};

#endif