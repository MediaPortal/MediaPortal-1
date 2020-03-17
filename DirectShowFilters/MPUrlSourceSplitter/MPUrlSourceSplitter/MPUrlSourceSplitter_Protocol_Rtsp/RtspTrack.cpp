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

#include "RtspTrack.h"

CRtspTrack::CRtspTrack(HRESULT *result)
  : CFlags()
{
  this->serverControlPort = PORT_UNSPECIFIED;
  this->serverDataPort = PORT_UNSPECIFIED;
  this->clientControlPort = PORT_UNSPECIFIED;
  this->clientDataPort = PORT_UNSPECIFIED;
  this->trackUrl = NULL;
  this->dataServer = NULL;
  this->controlServer = NULL;
  this->transportResponseHeader = NULL;
  this->lastReceiverReportTime = 0;
  this->receiverReportInterval = 0;
  this->payloadType = NULL;
  this->rtpPackets = NULL;
  this->flags = RTSP_TRACK_FLAG_NONE;
  this->statistics = NULL;
  this->senderSynchronizationSourceIdentifier = 0;
  this->synchronizationSourceIdentifier = 0;
  this->startTime = 0;
  this->lastCumulatedRtpTimestamp = 0;
  this->firstRtpPacketTimestamp = 0;
  this->lastRtpPacketTimestamp = 0;
  this->trackEndRtpTimestamp = 0;

  if ((result != NULL) && (SUCCEEDED(*result)))
  {
    this->payloadType = new CRtspPayloadType(result);
    this->rtpPackets = new CRtpPacketCollection(result);
    this->statistics = new CRtspTrackStatistics(result);

    CHECK_POINTER_HRESULT(*result, this->payloadType, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->rtpPackets, *result, E_OUTOFMEMORY);
    CHECK_POINTER_HRESULT(*result, this->statistics, *result, E_OUTOFMEMORY);

    if (SUCCEEDED(*result))
    {
      // create GUID and set SSRC to its first 4 bytes (Data1)
      GUID guid;
      *result = CoCreateGuid(&guid);
      if (SUCCEEDED(*result))
      {
        this->synchronizationSourceIdentifier = (unsigned int)guid.Data1;
      }
    }
  }
}

CRtspTrack::~CRtspTrack(void)
{
  FREE_MEM(this->trackUrl);
  FREE_MEM_CLASS(this->dataServer);
  FREE_MEM_CLASS(this->controlServer);
  FREE_MEM_CLASS(this->transportResponseHeader);
  FREE_MEM_CLASS(this->statistics);
  FREE_MEM_CLASS(this->payloadType);
  FREE_MEM_CLASS(this->rtpPackets);
}

/* get methods */

unsigned int CRtspTrack::GetServerDataPort(void)
{
  return this->serverDataPort;
}

unsigned int CRtspTrack::GetServerControlPort(void)
{
  return this->serverControlPort;
}

unsigned int CRtspTrack::GetClientDataPort(void)
{
  return this->clientDataPort;
}

unsigned int CRtspTrack::GetClientControlPort(void)
{
  return this->clientControlPort;
}

const wchar_t *CRtspTrack::GetTrackUrl(void)
{
  return this->trackUrl;
}

CSimpleServer *CRtspTrack::GetDataServer(void)
{
  return this->dataServer;
}

CSimpleServer *CRtspTrack::GetControlServer(void)
{
  return this->controlServer;
}

CRtspTransportResponseHeader *CRtspTrack::GetTransportResponseHeader(void)
{
  return this->transportResponseHeader;
}

DWORD CRtspTrack::GetLastReceiverReportTime(void)
{
  return this->lastReceiverReportTime;
}

DWORD CRtspTrack::GetReceiverReportInterval(void)
{
  return this->receiverReportInterval;
}

unsigned int CRtspTrack::GetSynchronizationSourceIdentifier(void)
{
  return this->synchronizationSourceIdentifier;
}

unsigned int CRtspTrack::GetSenderSynchronizationSourceIdentifier(void)
{
  return this->senderSynchronizationSourceIdentifier;
}

CRtspTrackStatistics *CRtspTrack::GetStatistics(void)
{
  return this->statistics;
}

CRtpPacketCollection *CRtspTrack::GetRtpPackets(void)
{
  return this->rtpPackets;
}

CRtspPayloadType *CRtspTrack::GetPayloadType(void)
{
  return this->payloadType;
}

unsigned int CRtspTrack::GetRtpPacketTimestamp(unsigned int currentTime)
{
  if (!this->IsSetFlags(RTSP_TRACK_FLAG_SET_START_TIME))
  {
    this->startTime = currentTime;
    this->flags |= RTSP_TRACK_FLAG_SET_START_TIME;
  }

  // current time is always greater or equal to this->startTime
  uint64_t timestamp = currentTime - this->startTime;
  timestamp *= this->statistics->GetClockFrequency();
  timestamp /= 1000;

  // RTP timestamp can overlap through UINT_MAX
  timestamp &= 0x00000000FFFFFFFF;

  return (unsigned int)timestamp;
}

int64_t CRtspTrack::GetStreamRtpTimestamp(void)
{
  return (this->lastCumulatedRtpTimestamp - (int64_t)this->firstRtpPacketTimestamp);
}

int64_t CRtspTrack::GetTrackEndRtpTimestamp(void)
{
  return this->trackEndRtpTimestamp;
}

/* set methods */

void CRtspTrack::SetServerDataPort(unsigned int serverDataPort)
{
  this->serverDataPort = serverDataPort;
}

void CRtspTrack::SetServerControlPort(unsigned int serverControlPort)
{
  this->serverControlPort = serverControlPort;
}

void CRtspTrack::SetClientDataPort(unsigned int clientDataPort)
{
  this->clientDataPort = clientDataPort;
}

void CRtspTrack::SetClientControlPort(unsigned int clientControlPort)
{
  this->clientControlPort = clientControlPort;
}

bool CRtspTrack::SetTrackUrl(const wchar_t *trackUrl)
{
  SET_STRING_RETURN_WITH_NULL(this->trackUrl, trackUrl);
}

void CRtspTrack::SetDataServer(CSimpleServer *dataServer)
{
  FREE_MEM_CLASS(this->dataServer);
  this->dataServer = dataServer;
}

void CRtspTrack::SetControlServer(CSimpleServer *controlServer)
{
  FREE_MEM_CLASS(this->controlServer);
  this->controlServer = controlServer;
}

bool CRtspTrack::SetTransportResponseHeader(CRtspTransportResponseHeader *header)
{
  FREE_MEM_CLASS(this->transportResponseHeader);
  bool result = true;
  if (header != NULL)
  {
    this->transportResponseHeader = (CRtspTransportResponseHeader *)header->Clone();
    result &= (this->transportResponseHeader != NULL);
  }
  return result;
}

void CRtspTrack::SetLastReceiverReportTime(DWORD lastReceiverReportTime)
{
  this->lastReceiverReportTime = lastReceiverReportTime;
}

void CRtspTrack::SetReceiverReportInterval(DWORD receiverReportInterval)
{
  this->receiverReportInterval = receiverReportInterval;
}

void CRtspTrack::SetSynchronizationSourceIdentifier(unsigned int synchronizationSourceIdentifier)
{
  this->synchronizationSourceIdentifier = synchronizationSourceIdentifier;
}

void CRtspTrack::SetSenderSynchronizationSourceIdentifier(unsigned int senderSynchronizationSourceIdentifier)
{
  this->senderSynchronizationSourceIdentifier = senderSynchronizationSourceIdentifier;
  this->flags |= RTSP_TRACK_FLAG_SENDER_SYNCHRONIZATION_SOURCE_IDENTIFIER_SET;
}

void CRtspTrack::SetEndOfStream(bool endOfStream)
{
  this->flags &= ~RTSP_TRACK_FLAG_END_OF_STREAM;
  this->flags |= (endOfStream) ? RTSP_TRACK_FLAG_END_OF_STREAM : RTSP_TRACK_FLAG_NONE;
}

void CRtspTrack::SetTrackEndRtpTimestamp(int64_t trackEndRtpTimestamp)
{
  this->trackEndRtpTimestamp = trackEndRtpTimestamp;
}

/* other methods */

bool CRtspTrack::IsServerDataPort(unsigned int port)
{
  return (this->serverDataPort == port);
}

bool CRtspTrack::IsServerControlPort(unsigned int port)
{
  return (this->clientControlPort == port);
}

bool CRtspTrack::IsClientDataPort(unsigned int port)
{
  return (this->clientDataPort == port);
}

bool CRtspTrack::IsClientControlPort(unsigned int port)
{
  return (this->clientControlPort == port);
}

bool CRtspTrack::IsSetSenderSynchronizationSourceIdentifier(void)
{
  return this->IsSetFlags(RTSP_TRACK_FLAG_SENDER_SYNCHRONIZATION_SOURCE_IDENTIFIER_SET);
}

bool CRtspTrack::IsEndOfStream(void)
{
  return this->IsSetFlags(RTSP_TRACK_FLAG_END_OF_STREAM);
}

void CRtspTrack::UpdateRtpPacketTotalTimestamp(unsigned int currentRtpPacketTimestamp)
{
  if (!this->IsSetFlags(RTSP_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP))
  {
    this->flags |= RTSP_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP;
    this->firstRtpPacketTimestamp = currentRtpPacketTimestamp;
    this->lastRtpPacketTimestamp = currentRtpPacketTimestamp;
    this->lastCumulatedRtpTimestamp = currentRtpPacketTimestamp;
  }

  int64_t difference = ((currentRtpPacketTimestamp < this->lastRtpPacketTimestamp) ? 0x0000000100000000 : 0);
  difference += currentRtpPacketTimestamp;
  difference -= this->lastRtpPacketTimestamp;

  if (currentRtpPacketTimestamp < this->lastRtpPacketTimestamp)
  {
    // try to identify if overflow occured or RTP timestamp is only slightly decreased
    uint64_t diff = this->lastRtpPacketTimestamp - currentRtpPacketTimestamp;

    // on this place is difference always greater than or equal to zero, we can safely cast it to uint64_t
    if (diff < (uint64_t)difference)
    {
      // RTP timestamp decrease is more probable than overflow
      difference -= 0x0000000100000000;
    }
  }

  this->lastCumulatedRtpTimestamp += difference;
  this->lastRtpPacketTimestamp = currentRtpPacketTimestamp;
}
