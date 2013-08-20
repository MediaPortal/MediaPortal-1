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

#include "SenderReportRtcpPacket.h"
#include "BufferHelper.h"

CSenderReportRtcpPacket::CSenderReportRtcpPacket(void)
  : CRtcpPacket()
{
  this->senderSynchronizationSourceIdentifier = UINT_MAX;
  this->ntpTimestamp = UINT64_MAX;
  this->rtpTimestamp = UINT_MAX;
  this->senderPacketCount = UINT_MAX;
  this->senderOctetCount = UINT_MAX;

  this->profileSpecificExtensions = NULL;
  this->profileSpecificExtensionsLength = 0;
  this->reportBlocks = new CReportBlockCollection();
}

CSenderReportRtcpPacket::~CSenderReportRtcpPacket(void)
{
  FREE_MEM(this->profileSpecificExtensions);
  FREE_MEM_CLASS(this->reportBlocks);
}

/* get methods */

unsigned int CSenderReportRtcpPacket::GetSenderSynchronizationSourceIdentifier(void)
{
  return this->senderSynchronizationSourceIdentifier;
}

uint64_t CSenderReportRtcpPacket::GetNtpTimestamp(void)
{
  return this->ntpTimestamp;
}

unsigned int CSenderReportRtcpPacket::GetRtpTimestamp(void)
{
  return this->rtpTimestamp;
}

unsigned int CSenderReportRtcpPacket::GetSenderPacketCount(void)
{
  return this->senderPacketCount;
}

unsigned int CSenderReportRtcpPacket::GetSenderOctetCount(void)
{
  return this->senderOctetCount;
}

const unsigned char *CSenderReportRtcpPacket::GetProfileSpecificExtensions(void)
{
  return this->profileSpecificExtensions;
}

unsigned int CSenderReportRtcpPacket::GetProfileSpecificExtensionsLength(void)
{
  return this->profileSpecificExtensionsLength;
}

CReportBlockCollection *CSenderReportRtcpPacket::GetReportBlocks(void)
{
  return this->reportBlocks;
}

/* set methods */

/* other methods */

bool CSenderReportRtcpPacket::HasProfileSpecificExtensions(void)
{
  return ((this->flags & FLAG_SENDER_REPORT_RTCP_PACKET_PROFILE_EXTENSIONS) != 0);
}

void CSenderReportRtcpPacket::Clear(void)
{
  __super::Clear();

  this->senderSynchronizationSourceIdentifier = UINT_MAX;
  this->ntpTimestamp = UINT64_MAX;
  this->rtpTimestamp = UINT_MAX;
  this->senderPacketCount = UINT_MAX;
  this->senderOctetCount = UINT_MAX;

  FREE_MEM(this->profileSpecificExtensions);
  this->profileSpecificExtensionsLength = 0;

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->reportBlocks, this->reportBlocks->Clear());
}

bool CSenderReportRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->reportBlocks != NULL);
  result &= (this->packetType == SENDER_REPORT_RTCP_PACKET_TYPE);
  result &= (this->payloadLength >= SENDER_REPORT_RTCP_PACKET_HEADER_SIZE);

  if (result)
  {
    // sender report RTCP packet header is at least SENDER_REPORT_RTCP_PACKET_HEADER_SIZE long
    unsigned int position = 0;

    RBE32INC(this->payload, position, this->senderSynchronizationSourceIdentifier);
    RBE64INC(this->payload, position, this->ntpTimestamp);
    RBE32INC(this->payload, position, this->rtpTimestamp);
    RBE32INC(this->payload, position, this->senderPacketCount);
    RBE32INC(this->payload, position, this->senderOctetCount);

    // without padding we must have enough bytes for report blocks

    unsigned int reportBlockAndProfileSpecificExtensions = this->payloadLength - position;
    unsigned int reportBlockCount = reportBlockAndProfileSpecificExtensions / SENDER_REPORT_REPORT_BLOCK_SIZE;
    result &= (this->packetValue == reportBlockCount);

    if (result)
    {
      // parse report blocks

      for (unsigned int i = 0; (result && (i < reportBlockCount)); i++)
      {
        CReportBlock *reportBlock = new CReportBlock();
        result &= (reportBlock != NULL);

        if (result)
        {
          reportBlock->SetSynchronizationSourceIdentifier(RBE32(this->payload, position));
          reportBlock->SetFractionLost(RBE8(this->payload, (position + 4)));
          reportBlock->SetCumulativeNumberOfPacketsLost(RBE8(this->payload, (position + 5)));
          reportBlock->SetExtendedHighestSequenceNumberReceived(RBE8(this->payload, (position + 8)));
          reportBlock->SetInterarrivalJitter(RBE8(this->payload, (position + 12)));
          reportBlock->SetLastSenderReport(RBE8(this->payload, (position + 16)));
          reportBlock->SetDelaySinceLastSenderReport(RBE8(this->payload, (position + 20)));

          result &= this->reportBlocks->Add(reportBlock);
        }

        if (!result)
        {
          FREE_MEM_CLASS(reportBlock);
        }

        position += SENDER_REPORT_REPORT_BLOCK_SIZE;
      }
    }

    if (result)
    {
      this->profileSpecificExtensionsLength = reportBlockAndProfileSpecificExtensions % SENDER_REPORT_REPORT_BLOCK_SIZE;
      if (this->profileSpecificExtensionsLength != 0)
      {
        this->profileSpecificExtensions = ALLOC_MEM_SET(this->profileSpecificExtensions, unsigned char, this->profileSpecificExtensionsLength, 0);
        result &= (this->profileSpecificExtensions != NULL);

        if (result)
        {
          memcpy(this->profileSpecificExtensions, buffer + position, this->profileSpecificExtensionsLength);
        }
      }
    }
  }

  if (!result)
  {
    this->Clear();
  }

  return result;
}