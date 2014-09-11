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

#define TS_PACKET_SIZE                                                188

#define TS_PACKET_HEADER_LENGTH                                       4

#define TS_PACKET_SYNC_BYTE                                           0x47
//#define     PID_PAT                                                   0x0000
#define TS_PACKET_PID_NULL                                            0x1FFF
#define TS_PACKET_MAX_RESERVED_PID                                    0x000F

#define TS_PACKET_FIND_RESULT_NOT_FOUND                               -1
#define TS_PACKET_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER              -2
#define TS_PACKET_FIND_RESULT_NOT_ENOUGH_MEMORY                       -3
#define TS_PACKET_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS               -4

#define TS_PACKET_MINIMUM_CHECKED_UNSPECIFIED                         0
#define TS_PACKET_MINIMUM_CHECKED                                     7

#define TS_PACKET_HEADER_SYNC_BYTE_MASK                               0xFF000000
#define TS_PACKET_HEADER_TRANSPORT_ERROR_INDICATOR_MASK               0x00800000
#define TS_PACKET_HEADER_PAYLOAD_UNIT_START_MASK                      0x00400000
#define TS_PACKET_HEADER_TRANSPORT_PRIORITY_MASK                      0x00200000
#define TS_PACKET_HEADER_PID_MASK                                     0x001FFF00
#define TS_PACKET_HEADER_TRANSPORT_SCRAMBLING_MASK                    0x000000C0
#define TS_PACKET_HEADER_ADAPTATION_FIELD_MASK                        0x00000030
#define TS_PACKET_HEADER_CONTINUITY_COUNTER_MASK                      0x0000000F

#define TS_PACKET_HEADER_SYNC_BYTE                                    0x47000000

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

  /* set methods */

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

protected:
  // holds TS packet header (first 4 bytes)
  uint32_t header;

  /* methods */
};

#endif