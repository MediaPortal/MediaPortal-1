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

#ifndef __BASE_RTP_PACKET_DEFINED
#define __BASE_RTP_PACKET_DEFINED

#include "Flags.h"

#define BASE_RTP_PACKET_HEADER_SIZE                                     1               // length of the header in bytes

#define BASE_RTP_PACKET_FLAG_NONE                                       FLAGS_NONE

#define BASE_RTP_PACKET_FLAG_PADDING                                    (1 << (FLAGS_LAST + 0))

#define BASE_RTP_PACKET_FLAG_LAST                                       (FLAGS_NONE + 1)

// base RTP packet is simple base class for RTP packets and RTCP packets
// RTP and RTCP packets have similar header but totally different meanings of each value
// that's why RTCP packets have each own branch from  RTP packets
class CBaseRtpPacket : public CFlags
{
public:
  // intializes a new instance of CBaseRtpPacket
  CBaseRtpPacket(HRESULT *result);
  virtual ~CBaseRtpPacket(void);

  /* get methods */

  // gets the version of base RTP packet
  // @return : version of base RTP packet or UINT_MAX if error
  virtual unsigned int GetVersion(void);

  // gets packet base type (RTP or RTCP packet)
  // @return : base type of packet or UINT_MAX if not specified
  virtual unsigned int GetBaseType(void);

  // gets packet size
  // @return : packet size or UINT_MAX if error
  virtual unsigned int GetSize(void);

  /* set methods */

  /* other methods */

  // tests if packet is padded with extra padding bytes
  // @return : true if packet is padded, false otherwise
  virtual bool IsPadded(void);

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with packet data for parsing
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:
  // holds packet version
  unsigned int version;

  // holds packet base type
  unsigned int baseType;

  // holds padding size
  unsigned int paddingSize;

  // holds payload size and payload
  unsigned char *payload;
  unsigned int payloadSize;
};

#endif