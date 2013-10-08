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

#include "RtspStreamTrack.h"

CRtspStreamTrack::CRtspStreamTrack(void)
{
  this->streamFragments = new CRtspStreamFragmentCollection();
  this->lastFragmentRtpTimestamp = 0;
  this->lastRtpPacketTimestamp = 0;
  this->firstRtpPacketTimestamp = 0;
  this->streamFragmentDownloading = UINT_MAX;
  this->streamFragmentProcessing = 0;
  this->streamFragmentToDownload = UINT_MAX;
  this->streamLength = 0;
  this->bytePosition = 0;
  this->flags = RTSP_STREAM_TRACK_FLAG_NONE;
  this->storeFilePath = NULL;
  this->lastRtpPacketStreamFragmentIndex = 0;
  this->lastRtpPacketFragmentReceivedDataPosition = 0;
}

CRtspStreamTrack::~CRtspStreamTrack(void)
{
  CHECK_CONDITION_NOT_NULL_EXECUTE(this->storeFilePath, DeleteFile(this->storeFilePath));

  FREE_MEM_CLASS(this->streamFragments);
  FREE_MEM(this->storeFilePath);
}

/* get methods */

CRtspStreamFragmentCollection *CRtspStreamTrack::GetStreamFragments(void)
{
  return this->streamFragments;
}

uint64_t CRtspStreamTrack::GetFragmentTimestamp(unsigned int currentRtpPacketTimestamp, unsigned int clockFrequency, uint64_t initialTime)
{
  uint64_t correction = (this->IsSetFirstRtpPacketTimestamp()) ? 0 : (initialTime * clockFrequency / 1000);
  uint64_t fragmentRtpTimestamp = this->GetFragmentRtpTimestamp(currentRtpPacketTimestamp);

  // correct values of last fragment timestamp and current fragment timestamp
  this->lastFragmentRtpTimestamp += correction;
  fragmentRtpTimestamp += correction;

  fragmentRtpTimestamp -= this->firstRtpPacketTimestamp;
  // clock frequency is per second, we need to adjust result to ms
  fragmentRtpTimestamp *= 1000;
  fragmentRtpTimestamp /= clockFrequency;

  return fragmentRtpTimestamp;
}

uint64_t CRtspStreamTrack::GetFragmentRtpTimestamp(unsigned int currentRtpPacketTimestamp)
{
  if (!this->IsSetFirstRtpPacketTimestamp())
  {
    this->lastRtpPacketTimestamp = currentRtpPacketTimestamp;
    this->lastFragmentRtpTimestamp = currentRtpPacketTimestamp;
    this->firstRtpPacketTimestamp = currentRtpPacketTimestamp;
    this->flags |= RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP;
  }

  uint64_t difference = ((currentRtpPacketTimestamp < this->lastRtpPacketTimestamp) ? 0x0000000100000000 : 0);
  difference += currentRtpPacketTimestamp;
  difference -= this->lastRtpPacketTimestamp;

  this->lastFragmentRtpTimestamp += difference;
  this->lastRtpPacketTimestamp = currentRtpPacketTimestamp;

  return this->lastFragmentRtpTimestamp;
}

unsigned int CRtspStreamTrack::GetStreamFragmentDownloading(void)
{
  return this->streamFragmentDownloading;
}

unsigned int CRtspStreamTrack::GetStreamFragmentProcessing(void)
{
  return this->streamFragmentProcessing;
}

unsigned int CRtspStreamTrack::GetStreamFragmentToDownload(void)
{
  return this->streamFragmentToDownload;
}

int64_t CRtspStreamTrack::GetStreamLength(void)
{
  return this->streamLength;
}

int64_t CRtspStreamTrack::GetBytePosition(void)
{
  return this->bytePosition;
}

const wchar_t *CRtspStreamTrack::GetStoreFilePath(void)
{
  return this->storeFilePath;
}

unsigned int CRtspStreamTrack::GetLastRtpPacketStreamFragmentIndex(void)
{
  return this->lastRtpPacketStreamFragmentIndex;
}
  
unsigned int CRtspStreamTrack::GetLastRtpPacketFragmentReceivedDataPosition(void)
{
  return this->lastRtpPacketFragmentReceivedDataPosition;
}

/* set methods */

void CRtspStreamTrack::SetStreamFragmentDownloading(unsigned int streamFragmentDownloading)
{
  this->streamFragmentDownloading = streamFragmentDownloading;
}

void CRtspStreamTrack::SetStreamFragmentProcessing(unsigned int streamFragmentProcessing)
{
  this->streamFragmentProcessing = streamFragmentProcessing;
}

void CRtspStreamTrack::SetStreamFragmentToDownload(unsigned int streamFragmentToDownload)
{
  this->streamFragmentToDownload = streamFragmentToDownload;
}

void CRtspStreamTrack::SetStreamLength(int64_t streamLength)
{
  this->streamLength = streamLength;
}

void CRtspStreamTrack::SetBytePosition(int64_t bytePosition)
{
  this->bytePosition = bytePosition;
}

void CRtspStreamTrack::SetStreamLengthFlag(bool setStreamLengthFlag)
{
  this->flags &= ~RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH;
  this->flags |= setStreamLengthFlag ? RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH : RTSP_STREAM_TRACK_FLAG_NONE;
}

void CRtspStreamTrack::SetEndOfStreamFlag(bool endOfStreamFlag)
{
  this->flags &= ~RTSP_STREAM_TRACK_FLAG_END_OF_STREAM;
  this->flags |= endOfStreamFlag ? RTSP_STREAM_TRACK_FLAG_END_OF_STREAM : RTSP_STREAM_TRACK_FLAG_NONE;
}

bool CRtspStreamTrack::SetStoreFilePath(const wchar_t *storeFilePath)
{
  SET_STRING_RETURN_WITH_NULL(this->storeFilePath, storeFilePath);
}

void CRtspStreamTrack::SetFirstRtpPacketTimestampFlag(bool firstRtpPacketTimestampFlag)
{
  this->flags &= ~RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP;
  this->flags |= firstRtpPacketTimestampFlag ? RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP : RTSP_STREAM_TRACK_FLAG_NONE;
}

void CRtspStreamTrack::SetLastRtpPacketStreamFragmentIndex(unsigned int lastRtpPacketStreamFragmentIndex)
{
  this->lastRtpPacketStreamFragmentIndex = lastRtpPacketStreamFragmentIndex;
}
  
void CRtspStreamTrack::SetLastRtpPacketFragmentReceivedDataPosition(unsigned int lastRtpPacketFragmentReceivedDataPosition)
{
  this->lastRtpPacketFragmentReceivedDataPosition = lastRtpPacketFragmentReceivedDataPosition;
}

/* other methods */

bool CRtspStreamTrack::IsSetFirstRtpPacketTimestamp(void)
{
  return this->IsFlags(RTSP_STREAM_TRACK_FLAG_SET_FIRST_RTP_PACKET_TIMESTAMP);
}

bool CRtspStreamTrack::IsSetStreamLength(void)
{
  return this->IsFlags(RTSP_STREAM_TRACK_FLAG_SET_STREAM_LENGTH);
}

bool CRtspStreamTrack::IsSetEndOfStream(void)
{
  return this->IsFlags(RTSP_STREAM_TRACK_FLAG_END_OF_STREAM);
}

bool CRtspStreamTrack::IsFlags(unsigned int flags)
{
  return ((this->flags & flags) == flags);
}
