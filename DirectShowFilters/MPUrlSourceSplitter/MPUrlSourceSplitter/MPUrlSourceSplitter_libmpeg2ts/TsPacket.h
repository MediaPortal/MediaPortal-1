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

#ifndef __TS_PACKET_DEFINED
#define __TS_PACKET_DEFINED

#include "Flags.h"
#include "LinearBuffer.h"

#define TS_PACKET_FLAG_NONE                                           FLAGS_NONE

#define TS_PACKET_FLAG_PARSED                                         (1 << (FLAGS_LAST + 0))

#define TS_PACKET_FLAG_LAST                                           (FLAGS_LAST + 1)

#define TS_PACKET_FIND_RESULT_NOT_FOUND                               -1
#define TS_PACKET_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER              -2
#define TS_PACKET_FIND_RESULT_NOT_ENOUGH_MEMORY                       -3
#define TS_PACKET_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS               -4

#define TS_PACKET_MINIMUM_CHECKED_UNSPECIFIED                         0
#define TS_PACKET_MINIMUM_CHECKED                                     7

#define TS_PACKET_HEADER_SYNC_BYTE_MASK                               0x000000FF
#define TS_PACKET_HEADER_SYNC_BYTE_SHIFT                              24

#define TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_MASK               0x00000001
#define TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_SHIFT              23

#define TS_PACKET_HEADER_PAYLOAD_UNIT_START_MASK                      0x00000001
#define TS_PACKET_HEADER_PAYLOAD_UNIT_START_SHIFT                     22

#define TS_PACKET_HEADER_TRANSPORT_PRIORITY_MASK                      0x00000001
#define TS_PACKET_HEADER_TRANSPORT_PRIORITY_SHIFT                     21

#define TS_PACKET_HEADER_PID_MASK                                     0x00001FFF
#define TS_PACKET_HEADER_PID_SHIFT                                    8

#define TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_MASK                    0x00000003
#define TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_SHIFT                   6

#define TS_PACKET_HEADER_ADAPTATION_FIELD_MASK                        0x00000003
#define TS_PACKET_HEADER_ADAPTATION_FIELD_SHIFT                       4

#define TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK                      0x0000000F
#define TS_PACKET_HEADER_CONTINUITY_COUNTER_SHIFT                     0

class CTsPacket : public CFlags
{
public:
  CTsPacket(HRESULT *result);
  virtual ~CTsPacket(void);

  /* get methods */

  // gets MPEG2 TS packet PID
  // @return : MPEG2 TS packet PID
  virtual unsigned int GetPID(void);

  // gets transport scrambling control
  // @return : transport scrambling control
  virtual unsigned int GetTransportScramblingControl(void);

  // gets adaptation field control
  // @return : adaptation field control
  virtual unsigned int GetAdaptationFieldControl(void);
  
  // gets continuity counter
  // @return : continuity counter
  virtual unsigned int GetContinuityCounter(void);

  // gets adaptation field size
  // @return : adaptation field size
  virtual unsigned int GetAdaptationFieldSize(void);

  // gets payload size
  // @return : payload size
  virtual unsigned int GetPayloadSize(void);

  // gets payload
  // @return : payload or NULL if payload size is 0
  virtual const uint8_t *GetPayload(void);

  // gets MPEG2 TS packet
  // @return : MPEG2 TS packet or NULL if error
  virtual const uint8_t *GetPacket(void);

  /* set methods */

  // sets MPEG2 TS packet PID
  // @param pid : the MPEG2 TS packet PID to set
  virtual void SetPID(unsigned int pid);

  // sets transport scrambling control
  // @param transportScramblingControl : transport scrambling control to set
  virtual void SetTransportScramblingControl(unsigned int transportScramblingControl);

  // sets adaptation field control
  // @param adaptationFieldControl : the adaptation field control to set
  virtual void SetAdaptationFieldControl(unsigned int adaptationFieldControl);
  
  // sets continuity counter
  // @param continuityCounter : the continuity counter to set
  virtual void SetContinuityCounter(unsigned int continuityCounter);

  // sets payload and size
  // @param payload : the payload or NULL if payload size is 0
  // @param payloadSize : the payload size to set
  virtual bool SetPayload(const uint8_t *payload, unsigned int payloadSize);

  // sets transport error indicator
  // @param transportErrorIndicator : the transport error indicator
  virtual void SetTransportErrorIndicator(bool transportErrorIndicator);

  // sets payload unit start
  // @param payloadUnitStart : the payload unit start to set
  virtual void SetPayloadUnitStart(bool payloadUnitStart);

  // sets transport priority
  // @param transportPriority : the transport priority to set
  virtual void SetTransportPriority(bool transportPriority);

  /* other methods */

  // tests if TS packet is successfully parsed
  // @return : true if successfully parsed, false otherwise
  virtual bool IsParsed(void);

  // tests if transport error indicator is set
  // @return : true if set, false otherwise
  virtual bool IsTransportErrorIndicator(void);

  // tests if payload unit start is set
  // @return : true if set, false otherwise
  virtual bool IsPayloadUnitStart(void);

  // tests if transport priority is set
  // @return : true if set, false otherwise
  virtual bool IsTransportPriority(void);

  // parses data in buffer
  // @param buffer : buffer with MPEG2 TS data for parsing
  // @param length : the length of data in buffer
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const unsigned char *buffer, uint32_t length);

  // deeply clones current instance
  // @return : deep clone of current instance or NULL if error
  virtual CTsPacket *Clone(void);

  /* static methods */

  // try to find MPEG2 TS packet in buffer
  // @param buffer : buffer to try to find TS packet
  // @param length : length of buffer
  // @param minimumPacketsToCheck : 
  //  minimum TS packets to check, if not found such sequence, than TS_PACKET_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS returned
  //  if TS_PACKET_MINIMUM_CHECKED_UNSPECIFIED passed, than TS_PACKET_MINIMUM_CHECKED is used
  // @return : equal or greater to zero is position of MPEG2 TS packet sequence in buffer, TS_PACKET_FIND_RESULT value if error
  static int FindPacket(const unsigned char *buffer, unsigned int length, unsigned int minimumPacketsToCheck);

  // try to find MPEG2 TS packet in buffer
  // @param buffer : linear buffer to parse
  // @param minimumPacketsToCheck : 
  //  minimum TS packets to check, if not found such sequence, than TS_PACKET_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS returned
  //  if TS_PACKET_MINIMUM_CHECKED_UNSPECIFIED passed, than TS_PACKET_MINIMUM_CHECKED is used
  // @return : equal or greater to zero is position of MPEG2 TS packet sequence in buffer, TS_PACKET_FIND_RESULT value if error
  static int FindPacket(CLinearBuffer *buffer, unsigned int minimumPacketsToCheck);

  // tries to find MPEG2 TS packet sequence in buffer
  // @param buffer : buffer to try to find TS packet sequence
  // @param length : length of buffer
  // @param firstPacketPosition : the first MPEG2 TS packet position in buffer
  // @param packetSequenceLength : the length in bytes of continuous MPEG2 TS packets
  // @return : S_OK if successful, E_POINTER if buffer, firstPacketPosition or packetSequenceLength is NULL, error code otherwise
  static HRESULT FindPacketSequence(const unsigned char *buffer, unsigned int length, unsigned int *firstPacketPosition, unsigned int *packetSequenceLength);

  // tries to find MPEG2 TS packet sequence in buffer
  // @param buffer : linear buffer to parse
  // @param firstPacketPosition : the first MPEG2 TS packet position in buffer
  // @param packetSequenceLength : the length in bytes of continuous MPEG2 TS packets
  // @return : S_OK if successful, E_POINTER if buffer, firstPacketPosition or packetSequenceLength is NULL, error code otherwise
  static HRESULT FindPacketSequence(CLinearBuffer *buffer, unsigned int *firstPacketPosition, unsigned int *packetSequenceLength);

  // creates MPEG2 TS NULL packet
  // @return : MPEG2 TS packet or NULL if error
  static CTsPacket *CreateNullPacket(void);

  // creates MPEG2 TS NULL packet with payload filled with specified data byte
  // @param dataByte : the data byte to fill payload of MPEG2 TS NULL packet
  // @return : MPEG2 TS packet or NULL if error
  static CTsPacket *CreateNullPacket(uint8_t dataByte);

  // creates MPEG2 TS NULL packet with specified payload
  // @param payload : the payload of MPEG2 TS NULL packet
  // @param payloadSize : the payload size, it MUST be TS_PACKET_MAXIMUM_PAYLOAD_SIZE
  // @return : MPEG2 TS packet or NULL if error
  static CTsPacket *CreateNullPacket(const uint8_t *payload, unsigned int payloadSize);

protected:
  // holds whole MPEG2 TS packet (if fully parsed) or only MPEG2 TS packet header (if parsed only header)
  uint8_t *packet;

  /* methods */

  // parses data in buffer
  // @param buffer : buffer with MPEG2 TS data for parsing
  // @param length : the length of data in buffer
  // @param onlyHeader : only header of MPEG2 TS packet is parsed
  // @return : true if parsed successfully, false otherwise
  virtual bool Parse(const unsigned char *buffer, uint32_t length, bool onlyHeader);

  // gets new instance of MPEG2 TS packet
  // @return : new MPEG2 TS packet instance or NULL if error
  virtual CTsPacket *CreateItem(void);

  // deeply clones current instance
  // @param item : the MPEG2 TS packet instance to clone
  // @return : true if successful, false otherwise
  virtual bool InternalClone(CTsPacket *item);
};

#endif