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

#ifndef __MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_DEFINED
#define __MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_DEFINED

/*

   MPEG Video-specific header

   This header shall be attached to each RTP packet after the RTP fixed
   header.

    0                   1                   2                   3
    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |    MBZ  |T|         TR        | |N|S|B|E|  P  | | BFC | | FFC |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                                   AN              FBV     FFV 

        MBZ: Unused. Must be set to zero in current
           specification. This space is reserved for future use.

        T: MPEG-2 (Two) specific header extension present (1 bit).
           Set to 1 when the MPEG-2 video-specific header extension (see
           Section 3.4.1) follows this header. This extension may be
           needed for improved error resilience; however, its inclusion
           in an RTP packet is optional. (See Appendix 1.)

        TR: Temporal-Reference (10 bits). The temporal reference of
           the current picture within the current GOP. This value ranges
           from 0-1023 and is constant for all RTP packets of a given
           picture.

        AN: Active N bit for error resilience (1 bit). Set to 1 when
           the following bit (N) is used to signal changes in the
           picture header information for MPEG-2 payloads. It must be
           set to 0 for MPEG-1 payloads or when N bit is not used.

        N: New picture header (1 bit). Used for MPEG-2 payloads when
           the previous bit (AN) is set to 1. Otherwise, it must be set
           to zero. Set to 1 when the information contained in the
           previously transmitted Picture Headers can't be used to
           reconstruct a header for the current picture. This happens
           when the current picture is encoded using a different set of
           parameters than the previous pictures of the same type. The N
           bit must be constant for all RTP packets that belong to the
           same picture so that receipt of any packet from a picture
           allows detecting whether information necessary for
           reconstruction was contained in that picture (N = 1) or a
           previous one (N = 0).

        S: Sequence-header-present (1 bit). Normally 0 and set to 1 at
           the occurrence of each MPEG sequence header.  Used to detect
           presence of sequence header in RTP packet.

        B: Beginning-of-slice (BS) (1 bit). Set when the start of the
           packet payload is a slice start code, or when a slice start
           code is preceded only by one or more of a
           Video_Sequence_Header, GOP_header and/or Picture_Header.

        E: End-of-slice (ES) (1 bit). Set when the last byte of the
           payload is the end of an MPEG slice.

        P: Picture-Type (3 bits). I (1), P (2), B (3) or D (4). This
           value is constant for each RTP packet of a given picture.
           Value 000B is forbidden and 101B - 111B are reserved to
           support future extensions to the MPEG ES specification.

        FBV: full_pel_backward_vector
        BFC: backward_f_code
        FFV: full_pel_forward_vector
        FFC: forward_f_code
           Obtained from the most recent picture header, and are
           constant for each RTP packet of a given picture. For I frames
           none of these values are present in the picture header and
           they must be set to zero in the RTP header.  For P frames
           only the last two values are present and FBV and BFC must be
           set to zero in the RTP header. For B frames all the four
           values are present. 
*/

#include "RtpPacket.h"

#define MPEG1_OR_MPEG2_VIDEO_PAYLOAD_TYPE_DEFAULT                     32

#define MPEG1_OR_MPEG2_VIDEO_PAYLOAD_HEADER_LENGTH                    4
#define MPEG1_OR_MPEG2_VIDEO_PAYLOAD_MPEG2_EXTENSION_HEADER_LENGTH    4

#define FLAG_MPEG1_OR_MPEG2_VIDEO_PAYLOAD_MPEG2_HEADER_EXTENSION      0x00000008
#define FLAG_MPEG1_OR_MPEG2_VIDEO_PAYLOAD_ACTIVE_N_BIT                0x00000010
#define FLAG_MPEG1_OR_MPEG2_VIDEO_PAYLOAD_NEW_PICTURE_HEADER          0x00000020
#define FLAG_MPEG1_OR_MPEG2_VIDEO_PAYLOAD_SEQUENCE_HEADER             0x00000040
#define FLAG_MPEG1_OR_MPEG2_VIDEO_PAYLOAD_BEGINING_OF_SLICE           0x00000080
#define FLAG_MPEG1_OR_MPEG2_VIDEO_PAYLOAD_END_OF_SLICE                0x00000100

#define MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_I                   0x00000001
#define MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_P                   0x00000002
#define MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_B                   0x00000003
#define MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_D                   0x00000004

class CMpeg1OrMpeg2VideoRtpPacket : public CRtpPacket
{
public:
  // initializes a new instance of CMpeg1OrMpeg2VideoRtpPacket class
  CMpeg1OrMpeg2VideoRtpPacket(void);
  virtual ~CMpeg1OrMpeg2VideoRtpPacket(void);

  /* get methods */

  // gets packet size
  // @return : packet size or UINT_MAX if error
  virtual unsigned int GetSize(void);

  virtual unsigned int GetTemporalReference(void);
  virtual unsigned int GetPictureType(void);

  virtual bool GetFullPelBackwardVector(void);
  virtual unsigned int GetBackwardFCode(void);
  virtual bool GetFullPelForwardVector(void);
  virtual unsigned int GetForwardFCode(void);

  // gets payload data
  // @return : payload data or NULL if error
  virtual const unsigned char *GetPayload(void);

  // gets payload size
  // @return : payload size
  virtual unsigned int GetPayloadSize(void);

  /* set methods */

  /* other methods */

  virtual bool IsMpeg2HeaderExtension(void);
  virtual bool IsActiveNBit(void);
  virtual bool IsNewPictureHeader(void);
  virtual bool IsSequenceHeader(void);
  virtual bool IsBeginingOfSlice(void);
  virtual bool IsEndOfSlice(void);

  // sets current instance to default state
  virtual void Clear(void);

  // parses data in buffer
  // @param buffer : buffer with RTP packet data for parsing (there must be only one RTP packet)
  // @param length : the length of data in buffer
  // @return : true if successfully parsed, false otherwise
  virtual bool Parse(const unsigned char *buffer, unsigned int length);

protected:

  // holds temporal reference
  unsigned int temporalReference;

  // holds picture type
  unsigned int pictureType;

  // holds last byte value (necessary if picture type is not from specified picture types)
  unsigned int lastByte;

  /* methods */

  // creates RTP packet instance for cloning
  // @return : new RTP packet instance or NULL if error
  virtual CRtpPacket *CreateRtpPacket(void);

  // deeply clones current instance to specified RTP packet
  // @param rtpPacket : the RTP packet to clone current instance
  // @result : true if successful, false otherwise
  virtual bool CloneInternal(CRtpPacket *rtpPacket);
};

#endif