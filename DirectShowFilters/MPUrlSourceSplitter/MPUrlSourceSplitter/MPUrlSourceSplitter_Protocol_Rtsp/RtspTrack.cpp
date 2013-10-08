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

CRtspTrack::CRtspTrack(void)
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
  this->payloadType = new CPayloadType();
  this->rtpPackets = new CRtpPacketCollection();
  this->flags = RTSP_TRACK_FLAG_NONE;

  // create GUID and set SSRC to its first 4 bytes (Data1)
  GUID guid;
  if (CoCreateGuid(&guid) == S_OK)
  {
    this->synchronizationSourceIdentifier = (unsigned int)guid.Data1;
  }

  this->senderSynchronizationSourceIdentifier = 0;

  this->statistics = new CRtspTrackStatistics();
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

CPayloadType *CRtspTrack::GetPayloadType(void)
{
  return this->payloadType;
}

CRtpPacketCollection *CRtspTrack::GetRtpPackets(void)
{
  return this->rtpPackets;
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
    this->transportResponseHeader = header->Clone();
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
  return this->IsFlags(RTSP_TRACK_FLAG_SENDER_SYNCHRONIZATION_SOURCE_IDENTIFIER_SET);
}

CRtspTrack *CRtspTrack::Clone(void)
{
  CRtspTrack *clone = new CRtspTrack();
  bool result = (clone != NULL);

  if (result)
  {
    clone->clientControlPort = this->clientControlPort;
    clone->clientDataPort = this->clientDataPort;
    clone->serverControlPort = this->serverControlPort;
    clone->serverDataPort = this->serverDataPort;
    clone->lastReceiverReportTime = this->lastReceiverReportTime;
    clone->receiverReportInterval = this->receiverReportInterval;
    clone->synchronizationSourceIdentifier = this->synchronizationSourceIdentifier;
    clone->senderSynchronizationSourceIdentifier = this->senderSynchronizationSourceIdentifier;
    clone->flags = this->flags;

    SET_STRING_AND_RESULT_WITH_NULL(clone->trackUrl, this->trackUrl, result);

    if (this->transportResponseHeader != NULL)
    {
      clone->transportResponseHeader = this->transportResponseHeader->Clone();
      result &= (clone->transportResponseHeader != NULL);
    }

    result &= clone->rtpPackets->Append(this->rtpPackets);
  }

  CHECK_CONDITION_EXECUTE(!result, FREE_MEM_CLASS(clone));
  return clone;
}

bool CRtspTrack::IsEndOfStream(void)
{
  return this->IsFlags(RTSP_TRACK_FLAG_END_OF_STREAM);
}

bool CRtspTrack::IsFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}