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

#include "RtspTrackStatistics.h"

CRtspTrackStatistics::CRtspTrackStatistics()
{
  this->lastTime = 0;
  this->lastRtpPacketTimestamp = 0;
  this->clockFrequency = 0;

  this->setSequenceNumber = false;
  this->cycles = 0;
  this->lastSequenceNumber = 0;
  this->firstSequenceNumber = 0;
  this->receivedPacketCount = 0;

  this->lastExpectedSequenceNumber = 0;
  this->lastReceivedPacketCount = 0;
}

CRtspTrackStatistics::~CRtspTrackStatistics()
{
}

/* get methods */

unsigned int CRtspTrackStatistics::GetFractionLost()
{
  unsigned int extendedSequenceNumber = (this->cycles << 16) + this->lastSequenceNumber;
  unsigned int expectedSequenceNumber = extendedSequenceNumber - this->firstSequenceNumber + 1;

  uint64_t expectedDifference = (expectedSequenceNumber < this->lastExpectedSequenceNumber) ? 0x0000000100000000 : 0;
  expectedDifference += (uint64_t)expectedSequenceNumber - (uint64_t)this->lastExpectedSequenceNumber;

  uint64_t receivedPacketDifference = (this->receivedPacketCount < this->lastReceivedPacketCount) ? 0x0000000100000000 : 0;
  receivedPacketDifference += (uint64_t)this->receivedPacketCount - (uint64_t)this->lastReceivedPacketCount;

  unsigned int fraction = 0;

  if ((expectedDifference != 0) && (receivedPacketDifference > expectedDifference))
  {
    fraction = (unsigned int)(receivedPacketDifference - expectedDifference);
    fraction /= (unsigned int)expectedDifference;
  }

  this->lastExpectedSequenceNumber = expectedSequenceNumber;
  this->lastReceivedPacketCount = this->receivedPacketCount;

  return fraction;
}

unsigned int CRtspTrackStatistics::GetCumulativePacketLostCount()
{
  unsigned int extendedSequenceNumber = (this->cycles << 16) + this->lastSequenceNumber;
  unsigned int expectedSequenceNumber = extendedSequenceNumber - this->firstSequenceNumber + 1;

  int64_t lostCount = (int64_t)expectedSequenceNumber - (int64_t)this->receivedPacketCount;

  // cumulative packet lost is only 24 bits number
  // the maximum value is 0x0000000000FFFFFF, the minimum value is 0xFFFFFFFFFF800000
  lostCount = min(lostCount, 0x0000000000FFFFFF);
  lostCount = max(lostCount, 0xFFFFFFFFFF800000);
  lostCount &= 0x0000000000FFFFFF;

  return (unsigned int)lostCount;
}

unsigned int CRtspTrackStatistics::GetExtendedHighestSequenceNumberReceived()
{
  return ((this->cycles << 16) | this->lastSequenceNumber);
}

unsigned int CRtspTrackStatistics::GetJitter()
{
  return (this->jitter >> 4);
}

unsigned int CRtspTrackStatistics::GetClockFrequency(void)
{
  return this->clockFrequency;
}

/* set methods */

void CRtspTrackStatistics::SetClockFrequency(unsigned int clockFrequency)
{
  this->clockFrequency = clockFrequency;
}

/* other methods */

void CRtspTrackStatistics::AdjustJitter(unsigned int currentTime, unsigned int rtpPacketTimestamp)
{
  // jitter is calculated as integer approximation as described in RFC 3550, appendix A.8
  if (this->clockFrequency != 0)
  {
    // there can be overflow in times
    uint64_t calculatedRtpTimestampDifference = (currentTime < this->lastTime) ? 0x0000000100000000 : 0;
    calculatedRtpTimestampDifference += (uint64_t)currentTime - (uint64_t)this->lastTime;
    calculatedRtpTimestampDifference *= this->clockFrequency;
    // calculated RTP timestamp difference can be greater than UINT_MAX

    // there can be overflow in RTP packet timestamps
    uint64_t rtpTimestampDifference = (rtpPacketTimestamp < this->lastRtpPacketTimestamp) ? 0x0000000100000000 : 0;
    rtpTimestampDifference += (uint64_t)rtpPacketTimestamp - (uint64_t)this->lastRtpPacketTimestamp;
    // RTP timestamp difference can be at maximum UINT_MAX

    // jitter difference can be lower or greater than zero, so we use int64_t
    int64_t jitterDifference = (calculatedRtpTimestampDifference < rtpTimestampDifference) ? (int64_t)(rtpTimestampDifference - calculatedRtpTimestampDifference) : (int64_t)(calculatedRtpTimestampDifference - rtpPacketTimestamp);
    // jitter difference is now still greater or equal to zero
    // maximum current jitter value must be lower by 8, because we need to add 8 to current jitter
    jitterDifference -= (int64_t)((min(this->jitter, 0xFFFFFFFF - 0x00000008) + 8) >> 4);
    // jitter difference cannot be lower than -1/16 of current jitter
    // jitter difference must be lower or equal to current jitter
    jitterDifference = min(jitterDifference, (int64_t)this->jitter);   // lower or equal to current jitter

    this->jitter = (unsigned int)((int64_t)this->jitter - jitterDifference);
  }
}

void CRtspTrackStatistics::AdjustExpectedAndLostPacketCount(unsigned int sequenceNumber)
{
  if (!this->setSequenceNumber)
  {
    this->cycles = 0;
    this->lastSequenceNumber = 0;
    this->firstSequenceNumber = sequenceNumber;
    this->setSequenceNumber = true;
    this->receivedPacketCount = 0;
    this->lastReceivedPacketCount = 0;
  }
  else
  {
    CHECK_CONDITION_EXECUTE(sequenceNumber < this->lastSequenceNumber, this->cycles++);

    if (this->cycles > 0x0000FFFF)
    {
      // cycles cannot be greater than 0x0000FFFF - lower 16 bits are going to upper 16 bits of extended highest sequence number
      this->cycles &= 0x0000FFFF;
      this->firstSequenceNumber = sequenceNumber;
      this->receivedPacketCount = 0;
      this->lastReceivedPacketCount = 0;
    }

    this->lastSequenceNumber = sequenceNumber;
  }

  this->receivedPacketCount++;
}