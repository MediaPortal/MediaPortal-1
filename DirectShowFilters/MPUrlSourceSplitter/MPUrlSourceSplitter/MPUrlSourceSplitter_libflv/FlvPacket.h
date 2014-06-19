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

#ifndef __FLV_PACKET_DEFINED
#define __FLV_PACKET_DEFINED

#include "LinearBuffer.h"

#define FLV_PACKET_NONE                                                       0x00
#define FLV_PACKET_AUDIO                                                      0x08
#define FLV_PACKET_VIDEO                                                      0x09
#define FLV_PACKET_META                                                       0x12
#define FLV_PACKET_HEADER                                                     0x1F

#define FLV_PACKET_HEADER_LENGTH                                              13

#define FLV_PACKET_TYPE_MASK                                                  0x1F
#define FLV_PACKET_ENCRYPTED_MASK                                             0x20

#define FLV_VIDEO_CODECID_MASK                                                0x0F
#define FLV_VIDEO_FRAMETYPE_MASK                                              0xF0

#define FLV_VIDEO_FRAMETYPE_OFFSET                                            4

#define FLV_FRAME_KEY                                                         (1 << FLV_VIDEO_FRAMETYPE_OFFSET)
#define FLV_FRAME_INTER                                                       (2 << FLV_VIDEO_FRAMETYPE_OFFSET)
#define FLV_FRAME_DISP_INTER                                                  (3 << FLV_VIDEO_FRAMETYPE_OFFSET)

#define FLV_CODECID_H263                                                      2
#define FLV_CODECID_SCREEN                                                    3
#define FLV_CODECID_VP6                                                       4
#define FLV_CODECID_VP6A                                                      5
#define FLV_CODECID_SCREEN2                                                   6
#define FLV_CODECID_H264                                                      7
#define FLV_CODECID_REALH263                                                  8
#define FLV_CODECID_MPEG4                                                     9

#define FLV_PARSE_RESULT_ERROR_COUNT                                          4

#define FLV_PARSE_RESULT_OK                                                   0

#define FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_HEADER                           -1
#define FLV_PARSE_RESULT_NOT_ENOUGH_DATA_FOR_PACKET                           -2
#define FLV_PARSE_RESULT_NOT_ENOUGH_MEMORY                                    -3
#define FLV_PARSE_RESULT_CHECK_SIZE_INCORRECT                                 -4

#define FLV_FIND_RESULT_ERROR_COUNT                                           4

#define FLV_FIND_RESULT_NOT_FOUND                                             -1
#define FLV_FIND_RESULT_NOT_ENOUGH_DATA_FOR_HEADER                            -2
#define FLV_FIND_RESULT_NOT_ENOUGH_MEMORY                                     -3
#define FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS                             -4

#define FLV_PACKET_MINIMUM_CHECKED_UNSPECIFIED                                 0
#define FLV_PACKET_MINIMUM_CHECKED                                             5

class CFlvPacket
{
public:
  // initializes a new instance of CFlvPacket class
  CFlvPacket(HRESULT *result);
  virtual ~CFlvPacket(void);

  // tests if current instance of CFlvPacket is valid
  // @return : true if valid, false otherwise
  virtual bool IsValid();

  // gets FLV packet type
  // @return : FLV packet type
  virtual unsigned int GetType(void);

  // gets FLV packet size
  // @return : FLV packet size
  virtual unsigned int GetSize(void);

  // gets FLV packet data
  // @return : FLV packet data
  virtual const unsigned char *GetData(void);

  // parses buffer for FLV packet
  // @param buffer : linear buffer to parse
  // @return : 0 if FLV packet found, FLV_PARSE_RESULT value otherwise
  virtual int ParsePacket(CLinearBuffer *buffer);

  // parses buffer for FLV packet
  // @param buffer : buffer to parse
  // @param length : length of buffer
  // @return : 0 if FLV packet found, FLV_PARSE_RESULT value otherwise
  virtual int ParsePacket(const unsigned char *buffer, unsigned int length);

  // creates FLV packet and fill it with data from buffer
  // @param packetType : type of FLV packet (FLV_PACKET_AUDIO, FLV_PACKET_VIDEO, FLV_PACKET_META)
  // @param buffer : data to fill FLV packet
  // @param length : the length of data buffer
  // @param timestamp : the timestamp of FLV packet
  // @param encrypted : specifies if FLV packet is encrypted
  // @return : true if FLV packet successfully created, false otherwise
  virtual bool CreatePacket(unsigned int packetType, const unsigned char *buffer, unsigned int length, unsigned int timestamp, bool encrypted);

  // tests if FLV packet is encrypted
  // @return : true if packet is encrypted, false otherwise
  virtual bool IsEncrypted(void);

  // gets FLV packet timestamp
  virtual unsigned int GetTimestamp(void);

  // sets FLV packet timestamp
  virtual void SetTimestamp(unsigned int timestamp);

  // gets codec ID for video packet
  // @return : codec ID or UINT_MAX if not valid
  virtual unsigned int GetCodecId(void);
  
  // sets codec ID for video packet
  // @param codecId : video packet codec ID to set
  virtual void SetCodecId(unsigned int codecId);

  // gets frame type for video packet
  // @return : frame type or UINT_MAX if not valid
  virtual unsigned int GetFrameType(void);

  // sets video packet frame type
  // @param frameType : video packet frame type to set
  virtual void SetFrameType(unsigned int frameType);

  // clears current instance
  virtual void Clear(void);

  // gets possible FLV packet size
  // @param buffer : buffer to parse
  // @param length : the length of buffer
  // @return : the possible size of FLV packet or UINT_MAX if error
  virtual unsigned int GetPossiblePacketSize(const unsigned char *buffer, unsigned int length);

  // tests if FLV packet is key frame
  virtual bool IsKeyFrame(void);

  // try to find FLV packet in buffer
  // @param buffer : buffer to try to find FLV packet
  // @param length : length of buffer
  // @param minimumFlvPacketsToCheck : 
  //  minimum FLV packets to check, if not found such sequence, than FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS returned
  //  if FLV_PACKET_MINIMUM_CHECKED_UNSPECIFIED passed, than FLV_PACKET_MINIMUM_CHECKED is used
  // @return : equal or greater to zero is position of FLV packet in buffer, FLV_FIND_RESULT value if error
  virtual int FindPacket(const unsigned char *buffer, unsigned int length, unsigned int minimumFlvPacketsToCheck);

  // try to find FLV packet in buffer
  // @param buffer : linear buffer to parse
  // @param minimumFlvPacketsToCheck : 
  //  minimum FLV packets to check, if not found such sequence, than FLV_FIND_RESULT_NOT_FOUND_MINIMUM_PACKETS returned
  //  if FLV_PACKET_MINIMUM_CHECKED_UNSPECIFIED passed, than FLV_PACKET_MINIMUM_CHECKED is used
  // @return : equal or greater to zero is position of FLV packet in buffer, FLV_FIND_RESULT value if error
  virtual int FindPacket(CLinearBuffer *buffer, unsigned int minimumFlvPacketsToCheck);

protected:
  // holds packet type (AUDIO, VIDEO, META or HEADER)
  unsigned int type;

  // holds FLV packet size
  unsigned int size;

  // holds FLV packet data
  unsigned char *packet;

  // hold if FLV packet is encrypted
  bool encrypted;
};

#endif