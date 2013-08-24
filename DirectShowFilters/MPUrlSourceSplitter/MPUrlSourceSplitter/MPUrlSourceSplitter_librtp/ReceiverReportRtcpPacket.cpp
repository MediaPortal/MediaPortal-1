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

#include "ReceiverReportRtcpPacket.h"
#include "BufferHelper.h"

CReceiverReportRtcpPacket::CReceiverReportRtcpPacket(void)
  : CRtcpPacket()
{
  this->senderSynchronizationSourceIdentifier = 0;
  this->profileSpecificExtensions = NULL;
  this->profileSpecificExtensionsLength = 0;
  this->reportBlocks = new CReportBlockCollection();
}

CReceiverReportRtcpPacket::~CReceiverReportRtcpPacket(void)
{
  FREE_MEM(this->profileSpecificExtensions);
  FREE_MEM_CLASS(this->reportBlocks);
}

/* get methods */

unsigned int CReceiverReportRtcpPacket::GetPacketValue(void)
{
  return this->reportBlocks->Count();
}

unsigned int CReceiverReportRtcpPacket::GetPacketType(void)
{
  return RECEIVER_REPORT_RTCP_PACKET_TYPE;
}

unsigned int CReceiverReportRtcpPacket::GetSenderSynchronizationSourceIdentifier(void)
{
  return this->senderSynchronizationSourceIdentifier;
}

const unsigned char *CReceiverReportRtcpPacket::GetProfileSpecificExtensions(void)
{
  return this->profileSpecificExtensions;
}

unsigned int CReceiverReportRtcpPacket::GetProfileSpecificExtensionsLength(void)
{
  return this->profileSpecificExtensionsLength;
}

CReportBlockCollection *CReceiverReportRtcpPacket::GetReportBlocks(void)
{
  return this->reportBlocks;
}

unsigned int CReceiverReportRtcpPacket::GetSize(void)
{
  // receiver report packet has RTCP_PACKET_HEADER_SIZE + SSRC + RECEIVER_REPORT_REPORT_BLOCK_SIZE * count of blocks
  return (__super::GetSize() + 4 + RECEIVER_REPORT_REPORT_BLOCK_SIZE * this->GetReportBlocks()->Count());
}

bool CReceiverReportRtcpPacket::GetPacket(unsigned char *buffer, unsigned int length)
{
  bool result = __super::GetPacket(buffer, length);

  if (result)
  {
    unsigned int position = __super::GetSize();

    WBE32INC(buffer, position, this->GetSenderSynchronizationSourceIdentifier());
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

void CReceiverReportRtcpPacket::SetSenderSynchronizationSourceIdentifier(unsigned int senderSynchronizationSourceIdentifier)
{
  this->senderSynchronizationSourceIdentifier = senderSynchronizationSourceIdentifier;
}

/* other methods */

bool CReceiverReportRtcpPacket::HasProfileSpecificExtensions(void)
{
  return ((this->flags & FLAG_RECEIVER_REPORT_RTCP_PACKET_PROFILE_EXTENSIONS) != 0);
}

void CReceiverReportRtcpPacket::Clear(void)
{
  __super::Clear();

  this->senderSynchronizationSourceIdentifier = 0;
  FREE_MEM(this->profileSpecificExtensions);
  this->profileSpecificExtensionsLength = 0;
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->reportBlocks, this->reportBlocks->Clear());
}

bool CReceiverReportRtcpPacket::Parse(const unsigned char *buffer, unsigned int length)
{
  bool result = __super::Parse(buffer, length);
  result &= (this->reportBlocks != NULL);
  result &= (this->packetType == RECEIVER_REPORT_RTCP_PACKET_TYPE);
  result &= (this->payloadSize >= RECEIVER_REPORT_RTCP_PACKET_HEADER_SIZE);

  if (result)
  {
    // receiver report RTCP packet header is at least RECEIVER_REPORT_RTCP_PACKET_HEADER_SIZE long
    unsigned int position = 0;

    RBE32INC(this->payload, position, this->senderSynchronizationSourceIdentifier);

    // without padding we must have enough bytes for report blocks

    unsigned int reportBlockAndProfileSpecificExtensions = this->payloadSize - RECEIVER_REPORT_RTCP_PACKET_HEADER_SIZE;
    unsigned int reportBlockCount = reportBlockAndProfileSpecificExtensions / RECEIVER_REPORT_REPORT_BLOCK_SIZE;
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

        position += RECEIVER_REPORT_REPORT_BLOCK_SIZE;
      }
    }

    if (result)
    {
      this->profileSpecificExtensionsLength = reportBlockAndProfileSpecificExtensions % RECEIVER_REPORT_REPORT_BLOCK_SIZE;
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