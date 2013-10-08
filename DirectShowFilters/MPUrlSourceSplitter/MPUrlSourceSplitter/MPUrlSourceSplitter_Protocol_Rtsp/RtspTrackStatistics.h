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

#ifndef __RTSP_TRACK_STATISTICS_DEFINED
#define __RTSP_TRACK_STATISTICS_DEFINED

#define RTSP_TRACK_FLAG_NONE                                          0x00000000
#define RTSP_TRACK_FLAG_SET_SEQUENCE_NUMBER                           0x00000001

class CRtspTrackStatistics
{
public:
  // initializes a new instance of CRtspTrackStatistics class
  CRtspTrackStatistics(void);
  virtual ~CRtspTrackStatistics(void);

  /* get methods */

  // gets fraction lost
  // after call is fraction lost set to zero
  // @return : fraction lost
  unsigned int GetFractionLost(void);

  // gets cumulative packet lost count
  // @return : cumulative packet lost count
  unsigned int GetCumulativePacketLostCount(void);

  // gets extended highest sequence number
  // @return : extended highest sequence number
  unsigned int GetExtendedHighestSequenceNumberReceived(void);

  // gets jitter
  // @return : jitter
  unsigned int GetJitter(void);

  // gets clock frequency used to compute RTP timestamps
  // @return : clock frequency to compute RTP timestamps
  unsigned int GetClockFrequency(void);

  // gets last sender report timestamp
  // @return : last sender report timestamp
  unsigned int GetLastSenderReportTimestamp(void);

  // gets delay since last sender report
  // @param currentTime : the current time measured in client environment in ms
  // @return : delay since last sender report
  unsigned int GetDelaySinceLastSenderReport(unsigned int currentTime);

  // gets last received packet count
  // @return : last received packet count
  unsigned int GetLastReceivedPacketCount(void);

  // gets previous last received packet count (the state before current last received packet count)
  // @return : previous last received packet count
  unsigned int GetPreviousLastReceivedPacketCount(void);

  /* set methods */

  // sets clock frequency used to compute RTP timestamps
  // @param clockFrequency : clock frequency used to compute RTP timestamps to set
  void SetClockFrequency(unsigned int clockFrequency);

  /* other methods */

  // adjusts jitter in RTSP track statistics
  // @param currentTime : the current time measured in client environment in ms
  // @param rtpPacketTimestamp : RTP packet timestamp
  void AdjustJitter(unsigned int currentTime, unsigned int rtpPacketTimestamp);

  // adjusts expected and lost packet count
  // @param sequenceNumber : the sequence number of RTP packet
  void AdjustExpectedAndLostPacketCount(unsigned int sequenceNumber);

  // adjusts last sender report timestamp
  // @param ntpTimestamp : RTCP packet NTP timestamp
  // @param currentTime : the current time measured in client environment in ms
  void AdjustLastSenderReportTimestamp(uint64_t ntpTimestamp, unsigned int currentTime);

  // tests if first sequence number is set (tests if we received some RTP packet)
  // @return : true if first sequence number is set, false otherwise
  bool IsSetSequenceNumber(void);

  // tests if specific combination of flags is set
  // @param flags : the set of flags to test
  // @return : true if set of flags is set, false otherwise
  bool IsSetFlags(unsigned int flags);

protected:

  unsigned int flags;

  /* jitter */
  // jitter is calculated as integer approximation as described in RFC 3550, appendix A.8

  unsigned int jitter;
  unsigned int lastTime;
  unsigned int lastRtpPacketTimestamp;
  unsigned int clockFrequency;

  /* cumulative packet lost count, expected and lost packets, fraction lost */

  unsigned int cycles;
  unsigned int firstSequenceNumber;
  unsigned int lastSequenceNumber;
  unsigned int receivedPacketCount;

  unsigned int lastExpectedSequenceNumber;
  unsigned int lastReceivedPacketCount;
  unsigned int previousLastReceivedPacketCount;

  /* last sender report timestamp and time*/

  unsigned int lastSenderReportTimestamp;
  unsigned int lastSenderReportTime;
};

#endif