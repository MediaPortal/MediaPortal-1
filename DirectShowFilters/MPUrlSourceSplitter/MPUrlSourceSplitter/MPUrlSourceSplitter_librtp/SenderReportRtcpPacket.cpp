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
  this->senderSynchronizationSourceIdentifier = 0;
  this->ntpTimestamp = 0;
  this->rtpTimestamp = 0;
  this->senderPacketCount = 0;
  this->senderOctetCount = 0;

  this->profileSpecificExtensions = NULL;
  this->profileSpecificExtensionsLength = 0;
  this->reportBlocks = new CReportBlockCollection();

  this->packetType = SENDER_REPORT_RTCP_PACKET_TYPE;
}

CSenderReportRtcpPacket::~CSenderReportRtcpPacket(void)
{
  FREE_MEM(this->profileSpecificExtensions);
  FREE_MEM_CLASS(this->reportBlocks);
}

/* get methods */

unsigned int CSenderReportRtcpPacket::GetPacketValue(void)
{
  return this->GetReportBlocks()->Count();
}

unsigned int CSenderReportRtcpPacket::GetPacketType(void)
{
  return SENDER_REPORT_RTCP_PACKET_TYPE;
}

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

unsigned int CSenderReportRtcpPacket::GetSize(void)
{
  return (__super::GetSize() + SENDER_REPORT_RTCP_PACKET_HEADER_SIZE + this->GetReportBlocks()->Count() * SENDER_REPORT_REPORT_BLOCK_SIZE);
}

bool CSenderReportRtcpPacket::GetPacket(unsigned char *buffer, unsigned int length)
{
  bool result = __super::GetPacket(buffer, length);

  if (result)
  {
    unsigned int position = __super::GetSize();

    WBE32INC(buffer, position, this->GetSenderSynchronizationSourceIdentifier());
    WBE64INC(buffer, position, this->GetNtpTimestamp());
    WBE32INC(buffer, position, this->GetRtpTimestamp());
    WBE32INC(buffer, position, this->GetSenderPacketCount());
    WBE32INC(buffer, position, this->GetSenderOctetCount());

    for (unsigned int i = 0; i < this->GetReportBlocks()->Count(); i++)
    {
      CReportBlock *report = this->GetReportBlocks()->GetItem(i);

      WBE32INC(buffer, position, report->GetSynchronizationSourceIdentifier());
      WBE8INC(buffer, position, report->GetFractionLost());
      WBE24INC(buffer, position, report->GetCumulativeNumberOfPacketsLost());
      WBE32INC(buffer, position, report->GetExtendedHighestSequenceNumberReceived());
      WBE32INC(buffer, position, report->GetInterarrivalJitter());
      WBE32INC(buffer, position, report->GetLastSenderReport());
      WBE32INC(buffer, position, report->GetDelaySinceLastSenderReport());
    }
  }

  return result;
}

/* set methods */

void CSenderReportRtcpPacket::SetSenderSynchronizationSourceIdentifier(unsigned int senderSynchronizationSourceIdentifier)
{
  this->senderSynchronizationSourceIdentifier = senderSynchronizationSourceIdentifier;
}

void CSenderReportRtcpPacket::SetNtpTimestamp(uint64_t ntpTimestamp)
{
  this->ntpTimestamp = ntpTimestamp;
}

void CSenderReportRtcpPacket::SetRtpTimestamp(unsigned int rtpTimestamp)
{
  this->rtpTimestamp = rtpTimestamp;
}

void CSenderReportRtcpPacket::SetSenderPacketCount(unsigned int senderPacketCount)
{
  this->senderPacketCount = senderPacketCount;
}

void CSenderReportRtcpPacket::SetSenderOctetCount(unsigned int senderOctetCount)
{
  this->senderOctetCount = senderOctetCount;
}

/* other methods */

bool CSenderReportRtcpPacket::HasProfileSpecificExtensions(void)
{
  return ((this->flags & FLAG_SENDER_REPORT_RTCP_PACKET_PROFILE_EXTENSIONS) != 0);
}

void CSenderReportRtcpPacket::Clear(void)
{
  __super::Clear();

  this->senderSynchronizationSourceIdentifier = 0;
  this->ntpTimestamp = 0;
  this->rtpTimestamp = 0;
  this->senderPacketCount = 0;
  this->senderOctetCount = 0;

  FREE_MEM(this->profileSpecificExtensions);
  this->profileSpecificExtensionsLength = 0;

  CHECK_CONDITION_NOT_NULL_EXECUTE(this->reportBlocks, this->reportBlocks->Clear());

  this->packetType = SENDER_REPORT_RTCP_PACKET_TYPE;
}

bool CSenderReportRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->reportBlocks != NULL);
  result &= (this->packetType == SENDER_REPORT_RTCP_PACKET_TYPE);
  result &= (this->payloadSize >= SENDER_REPORT_RTCP_PACKET_HEADER_SIZE);

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

    unsigned int reportBlockAndProfileSpecificExtensions = this->payloadSize - position;
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
          reportBlock->SetCumulativeNumberOfPacketsLost(RBE24(this->payload, (position + 5)));
          reportBlock->SetExtendedHighestSequenceNumberReceived(RBE32(this->payload, (position + 8)));
          reportBlock->SetInterarrivalJitter(RBE32(this->payload, (position + 12)));
          reportBlock->SetLastSenderReport(RBE32(this->payload, (position + 16)));
          reportBlock->SetDelaySinceLastSenderReport(RBE32(this->payload, (position + 20)));

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