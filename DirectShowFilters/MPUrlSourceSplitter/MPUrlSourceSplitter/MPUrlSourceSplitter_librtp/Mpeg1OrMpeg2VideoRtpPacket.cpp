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

#include "StdAfx.h"

#include "Mpeg1OrMpeg2VideoRtpPacket.h"
#include "BufferHelper.h"

#include <stdint.h>

CMpeg1OrMpeg2VideoRtpPacket::CMpeg1OrMpeg2VideoRtpPacket(HRESULT *result)
  : CRtpPacket(result)
{
  this->pictureType = UINT_MAX;
  this->temporalReference = UINT_MAX;
  this->lastByte = UINT_MAX;
}

CMpeg1OrMpeg2VideoRtpPacket::~CMpeg1OrMpeg2VideoRtpPacket(void)
{
}

/* get methods */

unsigned int CMpeg1OrMpeg2VideoRtpPacket::GetSize(void)
{
  return (__super::GetSize() + MPEG1_OR_MPEG2_VIDEO_PAYLOAD_HEADER_LENGTH);
}

unsigned int CMpeg1OrMpeg2VideoRtpPacket::GetTemporalReference(void)
{
  return this->temporalReference;
}

unsigned int CMpeg1OrMpeg2VideoRtpPacket::GetPictureType(void)
{
  return this->pictureType;
}

bool CMpeg1OrMpeg2VideoRtpPacket::GetFullPelBackwardVector(void)
{
  bool result = false;

  if ((this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_I) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_P) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_B) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_D))
  {
    result = ((this->lastByte & 0x00000080) != 0);
  }

  return result;
}

unsigned int CMpeg1OrMpeg2VideoRtpPacket::GetBackwardFCode(void)
{
  unsigned int result = UINT_MAX;

  if ((this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_I) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_P) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_B) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_D))
  {
    result = (this->lastByte & 0x00000070);
    result >>= 4;
  }

  return result;
}

bool CMpeg1OrMpeg2VideoRtpPacket::GetFullPelForwardVector(void)
{
  bool result = false;

  if ((this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_I) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_P) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_B) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_D))
  {
    result = ((this->lastByte & 0x00000008) != 0);
  }

  return result;
}

unsigned int CMpeg1OrMpeg2VideoRtpPacket::GetForwardFCode(void)
{
  unsigned int result = UINT_MAX;

  if ((this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_I) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_P) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_B) ||
      (this->pictureType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_PICTURE_TYPE_D))
  {
    result = (this->lastByte & 0x00000007);
  }

  return result;
}

const unsigned char *CMpeg1OrMpeg2VideoRtpPacket::GetPayload(void)
{
  return (this->payload + MPEG1_OR_MPEG2_VIDEO_PAYLOAD_HEADER_LENGTH);
}

unsigned int CMpeg1OrMpeg2VideoRtpPacket::GetPayloadSize(void)
{
  return (this->payloadSize - MPEG1_OR_MPEG2_VIDEO_PAYLOAD_HEADER_LENGTH);
}

/* set methods */

/* other methods */

bool CMpeg1OrMpeg2VideoRtpPacket::IsMpeg2HeaderExtension(void)
{
  return this->IsSetFlags(MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_MPEG2_HEADER_EXTENSION);
}

bool CMpeg1OrMpeg2VideoRtpPacket::IsActiveNBit(void)
{
  return this->IsSetFlags(MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_ACTIVE_N_BIT);
}

bool CMpeg1OrMpeg2VideoRtpPacket::IsNewPictureHeader(void)
{
  return this->IsSetFlags(MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_NEW_PICTURE_HEADER);
}

bool CMpeg1OrMpeg2VideoRtpPacket::IsSequenceHeader(void)
{
  return this->IsSetFlags(MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_SEQUENCE_HEADER);
}

bool CMpeg1OrMpeg2VideoRtpPacket::IsBeginingOfSlice(void)
{
  return this->IsSetFlags(MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_BEGINING_OF_SLICE);
}

bool CMpeg1OrMpeg2VideoRtpPacket::IsEndOfSlice(void)
{
  return this->IsSetFlags(MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_END_OF_SLICE);
}

void CMpeg1OrMpeg2VideoRtpPacket::Clear(void)
{
  __super::Clear();

  this->pictureType = UINT_MAX;
  this->temporalReference = UINT_MAX;
  this->lastByte = UINT_MAX;
}

bool CMpeg1OrMpeg2VideoRtpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);

  if (result)
  {
    // payload type should be MPEG1_OR_MPEG2_VIDEO_PAYLOAD_TYPE_DEFAULT
    // maybe later will be implemented collection of accepted payload types (if necessary)

    result &= (this->payloadType == MPEG1_OR_MPEG2_VIDEO_PAYLOAD_TYPE_DEFAULT);
    result &= (this->payloadSize >= MPEG1_OR_MPEG2_VIDEO_PAYLOAD_HEADER_LENGTH);

    if (result)
    {
      // parse first MPEG1_OR_MPEG2_VIDEO_PAYLOAD_HEADER_LENGTH bytes

      unsigned int position = 0;
      RBE32INC_DEFINE(this->payload, position, temp, unsigned int);

      this->flags |= ((temp & 0x04000000) != 0) ? MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_MPEG2_HEADER_EXTENSION : RTP_PACKET_FLAG_NONE;
      this->temporalReference = temp & 0x03FF0000;
      this->temporalReference >>= 16;

      this->flags |= ((temp & 0x00008000) != 0) ? MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_ACTIVE_N_BIT : RTP_PACKET_FLAG_NONE;
      this->flags |= ((temp & 0x00004000) != 0) ? MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_NEW_PICTURE_HEADER : RTP_PACKET_FLAG_NONE;
      this->flags |= ((temp & 0x00002000) != 0) ? MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_SEQUENCE_HEADER : RTP_PACKET_FLAG_NONE;
      this->flags |= ((temp & 0x00001000) != 0) ? MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_BEGINING_OF_SLICE : RTP_PACKET_FLAG_NONE;
      this->flags |= ((temp & 0x00000800) != 0) ? MPEG1_OR_MPEG2_VIDEO_RTP_PACKET_FLAG_PAYLOAD_END_OF_SLICE : RTP_PACKET_FLAG_NONE;

      this->pictureType = temp & 0x00000700;
      this->pictureType >>= 8;

      this->lastByte = temp & 0x000000FF;
    }
  }

  return result;
}

/* protected methods */

CRtpPacket *CMpeg1OrMpeg2VideoRtpPacket::CreateRtpPacket(void)
{
  HRESULT result = S_OK;
  CMpeg1OrMpeg2VideoRtpPacket *packet = new CMpeg1OrMpeg2VideoRtpPacket(&result);
  CHECK_POINTER_HRESULT(result, packet, result, E_OUTOFMEMORY);

  CHECK_CONDITION_EXECUTE(FAILED(result), FREE_MEM_CLASS(packet));
  return packet;
}

bool CMpeg1OrMpeg2VideoRtpPacket::CloneInternal(CRtpPacket *rtpPacket)
{
  bool result = __super::CloneInternal(rtpPacket);
  CMpeg1OrMpeg2VideoRtpPacket *mpeg1OrMpeg2VideoRtpPacket = dynamic_cast<CMpeg1OrMpeg2VideoRtpPacket *>(rtpPacket);
  result &= (mpeg1OrMpeg2VideoRtpPacket != NULL);

  if (result)
  {
    mpeg1OrMpeg2VideoRtpPacket->temporalReference = this->temporalReference;
    mpeg1OrMpeg2VideoRtpPacket->pictureType = this->pictureType;
    mpeg1OrMpeg2VideoRtpPacket->lastByte = this->lastByte;
  }

  return result;
}